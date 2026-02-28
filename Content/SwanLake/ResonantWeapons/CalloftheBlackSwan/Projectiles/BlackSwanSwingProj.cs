using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using MagnumOpus.Content.SwanLake.ResonantWeapons.CalloftheBlackSwan.Utilities;
using MagnumOpus.Content.SwanLake.ResonantWeapons.CalloftheBlackSwan.Particles;
using MagnumOpus.Content.SwanLake.ResonantWeapons.CalloftheBlackSwan.Primitives;
using MagnumOpus.Content.SwanLake.ResonantWeapons.CalloftheBlackSwan.Shaders;
using MagnumOpus.Content.SwanLake.Debuffs;
using ReLogic.Content;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.CalloftheBlackSwan.Projectiles
{
    /// <summary>
    /// Call of the Black Swan — Main held swing projectile.
    /// Exoblade-style state machine handling 3-phase ballet combo.
    /// 
    /// Phase 0 "Plié":    Graceful opening strike — fast, light, 155px blade
    /// Phase 1 "Arabesque": Dramatic reversed sweep — elegant, flipped, 160px blade
    /// Phase 2 "Grand Jeté": Powerful overhead slam — heavy, wide, 175px blade + fires flares
    /// 
    /// All rendering (blade sprite, shader trails, particles, bloom) is self-contained.
    /// </summary>
    public class BlackSwanSwingProj : ModProjectile
    {
        #region Constants

        private const int BladeLength_Plie = 155;
        private const int BladeLength_Arabesque = 160;
        private const int BladeLength_GrandJete = 175;

        private const int Duration_Plie = 20;
        private const int Duration_Arabesque = 24;
        private const int Duration_GrandJete = 28;

        private const float DamageMult_Plie = 0.85f;
        private const float DamageMult_Arabesque = 1.0f;
        private const float DamageMult_GrandJete = 1.4f;

        private const float MaxSwingAngle_Plie = MathHelper.PiOver2 * 1.5f;
        private const float MaxSwingAngle_Arabesque = MathHelper.PiOver2 * 1.6f;
        private const float MaxSwingAngle_GrandJete = MathHelper.PiOver2 * 1.8f;

        // Trail rendering
        private const int TrailPointCount = 40;
        private const int RenderPointCount = 80;

        #endregion

        #region Curve Definitions

        // Phase 0: Quick graceful arc
        private static readonly BlackSwanUtils.CurveSegment[] PlieAnimation = new[]
        {
            new BlackSwanUtils.CurveSegment(BlackSwanUtils.SineOutEasing, 0f, -0.85f, 0.2f, 2),
            new BlackSwanUtils.CurveSegment(BlackSwanUtils.PolyInEasing, 0.15f, -0.65f, 1.55f, 3),
            new BlackSwanUtils.CurveSegment(BlackSwanUtils.SineOutEasing, 0.80f, 0.90f, 0.10f, 2),
        };

        // Phase 1: Reversed elegant sweep (wider range)
        private static readonly BlackSwanUtils.CurveSegment[] ArabesqueAnimation = new[]
        {
            new BlackSwanUtils.CurveSegment(BlackSwanUtils.PolyOutEasing, 0f, -1.0f, 0.35f, 2),
            new BlackSwanUtils.CurveSegment(BlackSwanUtils.PolyInEasing, 0.25f, -0.65f, 1.55f, 4),
            new BlackSwanUtils.CurveSegment(BlackSwanUtils.PolyOutEasing, 0.82f, 0.90f, 0.10f, 2),
        };

        // Phase 2: Heavy overhead slam (dramatic build and fast release)
        private static readonly BlackSwanUtils.CurveSegment[] GrandJeteAnimation = new[]
        {
            new BlackSwanUtils.CurveSegment(BlackSwanUtils.SineBumpEasing, 0f, -1.0f, -0.15f, 2),   // Wind back
            new BlackSwanUtils.CurveSegment(BlackSwanUtils.PolyOutEasing, 0.12f, -1.15f, 0.45f, 2),  // Slow recovery
            new BlackSwanUtils.CurveSegment(BlackSwanUtils.PolyInEasing, 0.35f, -0.70f, 1.70f, 5),   // Explosive forward
            new BlackSwanUtils.CurveSegment(BlackSwanUtils.PolyOutEasing, 0.85f, 1.0f, 0.0f, 2),     // Decelerate
        };

        #endregion

        #region State Properties

        /// <summary>Combo phase: 0=Plié, 1=Arabesque, 2=Grand Jeté.</summary>
        public int ComboPhase => (int)Projectile.ai[0];

        /// <summary>Current swing progress 0→1.</summary>
        public float Progression => Math.Clamp((float)Timer / SwingDuration, 0f, 1f);

        /// <summary>Elapsed ticks since swing start.</summary>
        public int Timer => SwingDuration - Projectile.timeLeft;

        /// <summary>Total duration for current phase.</summary>
        public int SwingDuration => ComboPhase switch
        {
            0 => Duration_Plie,
            1 => Duration_Arabesque,
            _ => Duration_GrandJete
        };

        /// <summary>Blade length for current phase.</summary>
        public float BladeLength => ComboPhase switch
        {
            0 => BladeLength_Plie,
            1 => BladeLength_Arabesque,
            _ => BladeLength_GrandJete
        };

        /// <summary>Max arc angle for current phase.</summary>
        public float MaxAngle => ComboPhase switch
        {
            0 => MaxSwingAngle_Plie,
            1 => MaxSwingAngle_Arabesque,
            _ => MaxSwingAngle_GrandJete
        };

        /// <summary>Animation curves for current phase.</summary>
        public BlackSwanUtils.CurveSegment[] CurrentAnimation => ComboPhase switch
        {
            0 => PlieAnimation,
            1 => ArabesqueAnimation,
            _ => GrandJeteAnimation
        };

        /// <summary>Damage multiplier for current phase.</summary>
        public float DamageMultiplier => ComboPhase switch
        {
            0 => DamageMult_Plie,
            1 => DamageMult_Arabesque,
            _ => DamageMult_GrandJete
        };

        /// <summary>Whether phase 1 is flipped (reversed swing direction).</summary>
        public bool IsFlipped => ComboPhase == 1;

        /// <summary>Swing direction: -1 or 1.</summary>
        public int Direction { get; private set; }

        /// <summary>Base rotation toward mouse when swing started.</summary>
        public float BaseRotation { get; private set; }

        private Player Owner => Main.player[Projectile.owner];
        private bool _initialized;
        private float _squishFactor;
        private bool _flaresSpawned;
        private bool _soundPlayed;

        // Trail point cache
        private Vector2[] _trailPositions;
        private BlackSwanPrimitiveRenderer _trailRenderer;

        #endregion

        public override string Texture => "MagnumOpus/Content/SwanLake/ResonantWeapons/CalloftheBlackSwan/CalloftheBlackSwan";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 12;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
            Projectile.ownerHitCheck = true;
        }

        public override void AI()
        {
            if (!_initialized)
                Initialize();

            // Pin to player
            Projectile.Center = Owner.MountedCenter;
            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;

            // Evaluate swing angle
            float animValue = BlackSwanUtils.PiecewiseAnimation(Progression, CurrentAnimation);
            int flipSign = IsFlipped ? -1 : 1;
            float swingAngle = animValue * MaxAngle * Direction * flipSign;
            float currentRotation = BaseRotation + swingAngle;

            Projectile.rotation = currentRotation;

            // Set player arm rotation
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, currentRotation - MathHelper.PiOver2);

            // Swing VFX
            DoSwingVFX(currentRotation);

            // Phase 2: Fire flares at 70% progress
            if (ComboPhase == 2 && Progression >= 0.70f && !_flaresSpawned)
            {
                SpawnFlares(currentRotation);
                _flaresSpawned = true;
            }

            // Play sound at 20% progress
            if (Progression >= 0.20f && !_soundPlayed)
            {
                SoundEngine.PlaySound(SoundID.Item29 with { Pitch = -0.1f + ComboPhase * 0.15f, Volume = 0.85f }, Projectile.Center);
                _soundPlayed = true;
            }

            // Shrink toward end
            float targetScale = 1f;
            if (Owner.BlackSwan().IsEmpowered && ComboPhase == 2)
                targetScale = 1.5f;

            if (Progression > 0.75f)
            {
                float shrinkProgress = (Progression - 0.75f) / 0.25f;
                Projectile.scale = MathHelper.Lerp(targetScale, 0.3f, shrinkProgress);
            }
            else
            {
                Projectile.scale = MathHelper.Lerp(Projectile.scale, targetScale, 0.15f);
            }

            // Lighting along blade
            Vector2 swordDir = currentRotation.ToRotationVector2();
            for (int i = 0; i < 5; i++)
            {
                float bladeT = (float)i / 4f;
                Vector2 lightPos = Projectile.Center + swordDir * BladeLength * bladeT * Projectile.scale;
                float intensity = 0.4f * (1f - bladeT * 0.5f);
                bool isBlack = (Timer + i) % 2 == 0;
                Vector3 lightColor = isBlack ? new Vector3(0.1f, 0.1f, 0.15f) : new Vector3(0.6f, 0.6f, 0.7f);
                Lighting.AddLight(lightPos, lightColor * intensity);
            }

            // Kill at end of swing
            if (Projectile.timeLeft <= 0)
                Projectile.Kill();
        }

        private void Initialize()
        {
            _initialized = true;
            Direction = Math.Sign(Projectile.velocity.X) != 0 ? Math.Sign(Projectile.velocity.X) : 1;
            BaseRotation = Projectile.velocity.ToRotation();
            _squishFactor = Main.rand.NextFloat(0.75f, 1f);
            Projectile.timeLeft = SwingDuration;
            _trailPositions = new Vector2[TrailPointCount];
            _trailRenderer = new BlackSwanPrimitiveRenderer();

            // Apply phase damage multiplier
            Projectile.damage = (int)(Projectile.damage * DamageMultiplier);

            // Phase direction for visual variety  
            Owner.direction = Direction;
        }

        #region Swing VFX

        private void DoSwingVFX(float currentRotation)
        {
            Vector2 swordDir = currentRotation.ToRotationVector2();
            Vector2 tipPos = Projectile.Center + swordDir * BladeLength * Projectile.scale;

            // Dust along blade with dual-polarity
            if (Progression > 0.15f && Progression < 0.90f)
            {
                float dustChance = MathHelper.Lerp(0.3f, 0.9f, Progression);
                if (Main.rand.NextFloat() < dustChance)
                {
                    float bladeT = Main.rand.NextFloat(0.3f, 1f);
                    Vector2 dustPos = Projectile.Center + swordDir * BladeLength * bladeT * Projectile.scale;
                    Vector2 perpVel = new Vector2(-swordDir.Y, swordDir.X) * Main.rand.NextFloat(1f, 3f) * Direction;

                    bool isBlack = Main.rand.NextBool();
                    int dustType = isBlack ? DustID.Shadowflame : DustID.WhiteTorch;
                    Dust d = Dust.NewDustPerfect(dustPos, dustType, perpVel, 0,
                        isBlack ? new Color(30, 30, 40) : new Color(240, 240, 250), 1.2f);
                    d.noGravity = true;
                    d.fadeIn = 0.8f;
                }
            }

            // Feather particles on Phase 1 at midpoint
            if (ComboPhase == 1 && Math.Abs(Progression - 0.55f) < 0.03f)
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(2f, 2f) + swordDir * 2f;
                    BlackSwanParticleHandler.SpawnParticle(
                        new FeatherDriftParticle(tipPos + Main.rand.NextVector2Circular(10f, 10f),
                            vel, Main.rand.NextBool(), 50, 0.6f));
                }
            }

            // Phase 2 empowered aura at swing peak
            if (ComboPhase == 2 && Owner.BlackSwan().IsEmpowered && Progression > 0.3f && Progression < 0.7f)
            {
                // Intense dual-polarity sparks
                if (Main.rand.NextBool(2))
                {
                    float bladeT = Main.rand.NextFloat(0.5f, 1f);
                    Vector2 sparkPos = Projectile.Center + swordDir * BladeLength * bladeT * Projectile.scale;
                    Vector2 sparkVel = Main.rand.NextVector2Circular(4f, 4f);
                    BlackSwanParticleHandler.SpawnParticle(
                        new DualitySparkParticle(sparkPos, sparkVel, Main.rand.NextBool(), 20, 0.6f));
                }
            }

            // Generate trail points for shader rendering
            GenerateTrailPoints();
        }

        private void GenerateTrailPoints()
        {
            // Generate arc of blade tip positions for trail rendering
            float trailLength = Math.Min(Progression, 0.5f);
            float startProgress = Math.Max(0f, Progression - trailLength);

            for (int i = 0; i < TrailPointCount; i++)
            {
                float t = MathHelper.Lerp(startProgress, Progression, (float)i / (TrailPointCount - 1));
                float animValue = BlackSwanUtils.PiecewiseAnimation(t, CurrentAnimation);
                int flipSign = IsFlipped ? -1 : 1;
                float angle = BaseRotation + animValue * MaxAngle * Direction * flipSign;
                Vector2 dir = angle.ToRotationVector2();
                _trailPositions[i] = Projectile.Center + dir * BladeLength * Projectile.scale;
            }
        }

        #endregion

        #region Flare Spawning

        private void SpawnFlares(float rotation)
        {
            var bsp = Owner.BlackSwan();
            bool empowered = bsp.IsEmpowered;
            int count = empowered ? 8 : 3;
            int flareDamage = empowered ? Projectile.damage * 2 : Projectile.damage;

            if (empowered)
                bsp.ConsumeEmpowerment();

            Vector2 swordDir = rotation.ToRotationVector2();
            Vector2 tipPos = Projectile.Center + swordDir * BladeLength * Projectile.scale;

            float spreadAngle = empowered ? MathHelper.ToRadians(60f) : MathHelper.ToRadians(40f);

            for (int i = 0; i < count; i++)
            {
                float angleOffset = MathHelper.Lerp(-spreadAngle / 2f, spreadAngle / 2f,
                    count > 1 ? (float)i / (count - 1) : 0.5f);
                Vector2 flareDir = (rotation + angleOffset).ToRotationVector2();
                Vector2 flareVel = flareDir * 12f + Main.rand.NextVector2Circular(1f, 1f);

                int proj = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    tipPos, flareVel,
                    ModContent.ProjectileType<BlackSwanFlareProj>(),
                    flareDamage, Projectile.knockBack * 0.5f,
                    Projectile.owner,
                    ai0: empowered ? 1f : 0f);

                // Impact VFX at spawn point
                BlackSwanParticleHandler.SpawnParticle(
                    new DualitySparkParticle(tipPos, flareVel * 0.3f, i % 2 == 0, 15, 0.4f));
            }

            // Empowerment burst VFX
            if (empowered)
            {
                for (int i = 0; i < 12; i++)
                {
                    Vector2 burstVel = Main.rand.NextVector2CircularEdge(6f, 6f);
                    BlackSwanParticleHandler.SpawnParticle(
                        new DualitySparkParticle(tipPos, burstVel, i % 2 == 0, 25, 0.7f));
                }

                for (int i = 0; i < 6; i++)
                {
                    Vector2 smokeVel = Main.rand.NextVector2Circular(3f, 3f);
                    BlackSwanParticleHandler.SpawnParticle(
                        new MonochromaticSmokeParticle(tipPos, smokeVel, i % 2 == 0, 40, 1.2f, 0.5f));
                }

                SoundEngine.PlaySound(SoundID.Item119 with { Volume = 0.8f }, tipPos);
            }
        }

        #endregion

        #region Collision

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Line collision along blade
            float currentAngle = Projectile.rotation;
            Vector2 swordDir = currentAngle.ToRotationVector2();
            Vector2 start = Projectile.Center;
            Vector2 end = start + swordDir * (BladeLength + 40f) * Projectile.scale;
            float width = 30f;
            float _ = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, width, ref _);
        }

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            // Expand hitbox for better feel
            int expand = 20;
            hitbox.Inflate(expand, expand);
        }

        #endregion

        #region On Hit

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Flame of the Swan debuff
            target.AddBuff(ModContent.BuffType<FlameOfTheSwan>(), 360); // 6 seconds

            // Impact VFX
            Vector2 hitPos = target.Center;
            for (int i = 0; i < 8; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(5f, 5f);
                BlackSwanParticleHandler.SpawnParticle(
                    new DualitySparkParticle(hitPos, sparkVel, i % 2 == 0, 20, 0.5f));
            }

            for (int i = 0; i < 3; i++)
            {
                Vector2 featherVel = Main.rand.NextVector2Circular(3f, 3f) + new Vector2(0, -1.5f);
                BlackSwanParticleHandler.SpawnParticle(
                    new FeatherDriftParticle(hitPos + Main.rand.NextVector2Circular(15f, 15f),
                        featherVel, Main.rand.NextBool(), 50, 0.8f));
            }

            // Swan Lake music notes on every hit — escalating with combo
            SwanLakeVFXLibrary.SpawnMusicNotes(hitPos, 2 + ComboPhase, 20f, 0.6f, 1.0f, 28);

            // Prismatic sparkles on hit for iridescent flair
            SwanLakeVFXLibrary.SpawnPrismaticSparkles(hitPos, 3 + ComboPhase, 15f);

            // Screen shake on Phase 2
            if (ComboPhase == 2 && hit.Crit)
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector2 smokeVel = Main.rand.NextVector2Circular(4f, 4f);
                    BlackSwanParticleHandler.SpawnParticle(
                        new MonochromaticSmokeParticle(hitPos, smokeVel, i % 2 == 0, 35, 1.5f, 0.7f));
                }

                // Grand Jeté crit: full melee impact VFX from shared library
                SwanLakeVFXLibrary.MeleeImpact(hitPos, ComboPhase);
            }
        }

        #endregion

        #region Rendering

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;

            // Layer 1: Shader-driven swing trail
            if (Progression > 0.20f)
                DrawSlashTrail(sb);

            // Layer 2: Bloom underlays at blade tip
            DrawBloomUnderlays(sb);

            // Layer 3: Blade sprite
            DrawBlade(sb, lightColor);

            // Layer 4: Particles
            BlackSwanParticleHandler.DrawAllParticles(sb);

            return false;
        }

        private void DrawSlashTrail(SpriteBatch sb)
        {
            if (_trailPositions == null || _trailRenderer == null)
                return;

            // Determine trail colors based on phase
            bool isBlackPhase = ComboPhase % 2 == 0;
            Color primaryColor = isBlackPhase ? new Color(30, 30, 45) : new Color(220, 225, 240);
            Color secondaryColor = isBlackPhase ? new Color(80, 80, 100) : new Color(180, 185, 200);
            Color edgeColor = BlackSwanUtils.GetRainbow(Progression * 0.5f); // Prismatic edge shimmer

            float trailOpacity = MathHelper.Clamp((Progression - 0.20f) / 0.15f, 0f, 1f);
            if (Progression > 0.85f)
                trailOpacity *= 1f - (Progression - 0.85f) / 0.15f;

            MiscShaderData shader = BlackSwanShaderLoader.GetSlashShader();

            // Configure shader
            if (shader != null)
            {
                shader.UseColor(primaryColor);
                shader.UseSecondaryColor(secondaryColor);

                // Try to set custom parameters
                try
                {
                    shader.Shader.Parameters["fireColor"]?.SetValue(edgeColor.ToVector4());
                    shader.Shader.Parameters["uTime"]?.SetValue(Main.GameUpdateCount * 0.02f);
                    shader.Shader.Parameters["flipped"]?.SetValue(IsFlipped);
                }
                catch { }

                // Bind noise texture
                try
                {
                    var noiseTexture = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/SoftCircularCaustics",
                        ReLogic.Content.AssetRequestMode.ImmediateLoad);
                    if (noiseTexture?.Value != null)
                    {
                        Main.graphics.GraphicsDevice.Textures[1] = noiseTexture.Value;
                        Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
                    }
                }
                catch { }
            }

            // Main trail pass
            var mainSettings = new BlackSwanTrailSettings(
                width: (float t) =>
                {
                    float baseWidth = (1f - t * 0.6f) * BladeLength * 0.45f * Projectile.scale * _squishFactor;
                    return baseWidth * trailOpacity;
                },
                trailColor: (float t) =>
                {
                    Color col = Color.Lerp(primaryColor, secondaryColor, t);
                    return col * trailOpacity * (1f - t * 0.3f);
                },
                shader: shader,
                smoothen: true
            );

            sb.End();
            _trailRenderer.RenderTrail(_trailPositions, mainSettings, RenderPointCount);

            // Glow overlay pass (additive)
            if (shader != null)
            {
                try
                {
                    shader.UseColor(edgeColor * 0.5f);
                    shader.UseSecondaryColor(Color.White * 0.3f);
                }
                catch { }
            }

            var glowSettings = new BlackSwanTrailSettings(
                width: (float t) =>
                {
                    float baseWidth = (1f - t * 0.5f) * BladeLength * 0.55f * Projectile.scale * _squishFactor;
                    return baseWidth * trailOpacity * 0.6f;
                },
                trailColor: (float t) =>
                {
                    return edgeColor * trailOpacity * 0.3f * (1f - t);
                },
                shader: shader,
                smoothen: true
            );

            _trailRenderer.RenderTrail(_trailPositions, glowSettings, RenderPointCount);

            // Restore SpriteBatch
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawBloomUnderlays(SpriteBatch sb)
        {
            // Bloom at blade tip
            float currentAngle = Projectile.rotation;
            Vector2 swordDir = currentAngle.ToRotationVector2();
            Vector2 tipPos = Projectile.Center + swordDir * BladeLength * Projectile.scale - Main.screenPosition;

            // Load VFX Asset Library bloom textures
            Texture2D softRadial = null;
            Texture2D pointBloom = null;
            Texture2D starAccent = null;
            try
            {
                softRadial = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom",
                    AssetRequestMode.ImmediateLoad)?.Value;
                pointBloom = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom",
                    AssetRequestMode.ImmediateLoad)?.Value;
                starAccent = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/Stars/ThinTall4PointedStar",
                    AssetRequestMode.ImmediateLoad)?.Value;
            }
            catch { }

            if (softRadial == null && pointBloom == null) return;

            // Switch to additive
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            float bloomPulse = 0.8f + 0.2f * (float)Math.Sin(Main.GameUpdateCount * 0.15f);
            bool isBlackPhase = ComboPhase % 2 == 0;
            Color outerBloom = isBlackPhase ? new Color(30, 30, 50, 0) : new Color(200, 200, 220, 0);
            Color midBloom = isBlackPhase ? new Color(60, 60, 80, 0) : new Color(180, 185, 210, 0);

            // Layer 1: Wide outer halo (SoftRadialBloom) — polarity-colored
            if (softRadial != null)
            {
                Vector2 srOrigin = new Vector2(softRadial.Width / 2f, softRadial.Height / 2f);
                sb.Draw(softRadial, tipPos, null, outerBloom * 0.20f * bloomPulse, 0f, srOrigin, 1.4f * Projectile.scale, SpriteEffects.None, 0f);

                // Layer 2: Mid glow ring
                sb.Draw(softRadial, tipPos, null, midBloom * 0.35f * bloomPulse, 0f, srOrigin, 0.8f * Projectile.scale, SpriteEffects.None, 0f);
            }

            // Layer 3: Concentrated core (PointBloom) — white-hot
            if (pointBloom != null)
            {
                Vector2 pbOrigin = new Vector2(pointBloom.Width / 2f, pointBloom.Height / 2f);
                sb.Draw(pointBloom, tipPos, null, new Color(255, 255, 255, 0) * 0.55f * bloomPulse, 0f, pbOrigin, 0.35f * Projectile.scale, SpriteEffects.None, 0f);
            }

            // Layer 4: Star accent — rotating prismatic highlight
            if (starAccent != null)
            {
                Vector2 starOrigin = new Vector2(starAccent.Width / 2f, starAccent.Height / 2f);
                Color starColor = BlackSwanUtils.GetRainbow(Progression * 0.5f);
                float starRot = Main.GameUpdateCount * 0.08f;
                sb.Draw(starAccent, tipPos, null, new Color(starColor.R, starColor.G, starColor.B, 0) * 0.3f * bloomPulse,
                    starRot, starOrigin, 0.25f * Projectile.scale, SpriteEffects.None, 0f);
            }

            // Prismatic rainbow shimmer on empowered
            if (Owner.BlackSwan().IsEmpowered && ComboPhase == 2 && softRadial != null)
            {
                Vector2 srOrigin = new Vector2(softRadial.Width / 2f, softRadial.Height / 2f);
                Color rainbow = BlackSwanUtils.GetRainbow(Progression);
                sb.Draw(softRadial, tipPos, null, new Color(rainbow.R, rainbow.G, rainbow.B, 0) * 0.30f, 0f, srOrigin, 2.0f * Projectile.scale, SpriteEffects.None, 0f);

                if (pointBloom != null)
                {
                    Vector2 pbOrigin = new Vector2(pointBloom.Width / 2f, pointBloom.Height / 2f);
                    sb.Draw(pointBloom, tipPos, null, new Color(rainbow.R, rainbow.G, rainbow.B, 0) * 0.20f, 0f, pbOrigin, 0.6f * Projectile.scale, SpriteEffects.None, 0f);
                }
            }

            // Restore
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawBlade(SpriteBatch sb, Color lightColor)
        {
            Texture2D bladeTex = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = new Vector2(bladeTex.Width * 0.5f, bladeTex.Height);

            float rot = Projectile.rotation + MathHelper.PiOver4;
            SpriteEffects effects = Direction < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // Squish effect for weight feel
            float squish = MathHelper.Lerp(_squishFactor, 1f, Progression);
            Vector2 squishScale = new Vector2(1f + (1f - squish) * 0.5f, squish) * Projectile.scale;

            // Draw black shadow copy
            sb.Draw(bladeTex, drawPos + new Vector2(-1, 1), null,
                new Color(0, 0, 0, 100) * 0.3f, rot, origin, squishScale * 1.02f, effects, 0f);

            // Draw main blade
            sb.Draw(bladeTex, drawPos, null, lightColor, rot, origin, squishScale, effects, 0f);

            // Draw polarity overlay glow
            bool isBlackPhase = ComboPhase % 2 == 0;
            Color glowColor = isBlackPhase ? new Color(30, 30, 45, 0) : new Color(240, 240, 255, 0);
            float glowIntensity = 0.15f + 0.1f * (float)Math.Sin(Main.GameUpdateCount * 0.1f);
            sb.Draw(bladeTex, drawPos, null, glowColor * glowIntensity, rot, origin, squishScale * 1.01f, effects, 0f);
        }

        #endregion

        public override void OnKill(int timeLeft)
        {
            _trailRenderer?.Dispose();
        }
    }
}
