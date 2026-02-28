using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.DualFatedChime.Utilities;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.DualFatedChime.Particles;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.DualFatedChime.Primitives;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.DualFatedChime.Shaders;
using MagnumOpus.Content.LaCampanella.Debuffs;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.DualFatedChime.Projectiles
{
    /// <summary>
    /// Dual Fated Chime  EMain held swing projectile.
    /// Exoblade-style state machine handling 3-phase infernal combo.
    /// 
    /// Phase 0 "Bell Strike":  Quick ringing slash  Efast, sharp, 155px blade
    /// Phase 1 "Toll Sweep":   Broad reversed sweep  Eheavy, dramatic, 160px blade  
    /// Phase 2 "Grand Toll":   Devastating overhead slam  Emassive, fires flame waves, 175px blade
    /// 
    /// All rendering (blade sprite, shader trails, particles, bloom) is self-contained.
    /// </summary>
    public class DualFatedChimeSwingProj : ModProjectile
    {
        #region Constants

        private const int BladeLength_BellStrike = 155;
        private const int BladeLength_TollSweep = 160;
        private const int BladeLength_GrandToll = 175;

        private const int Duration_BellStrike = 18;
        private const int Duration_TollSweep = 22;
        private const int Duration_GrandToll = 26;

        private const float DamageMult_BellStrike = 0.9f;
        private const float DamageMult_TollSweep = 1.1f;
        private const float DamageMult_GrandToll = 1.35f;

        private const float MaxSwingAngle_BellStrike = MathHelper.PiOver2 * 1.4f;
        private const float MaxSwingAngle_TollSweep = MathHelper.PiOver2 * 1.6f;
        private const float MaxSwingAngle_GrandToll = MathHelper.PiOver2 * 2.0f;

        private const int TrailPointCount = 40;
        private const int RenderPointCount = 80;

        #endregion

        #region Curve Definitions

        // Phase 0: Quick bell strike  Esharp musical accent
        private static readonly DualFatedChimeUtils.CurveSegment[] BellStrikeAnimation = new[]
        {
            new DualFatedChimeUtils.CurveSegment(DualFatedChimeUtils.SineOutEasing, 0f, -0.85f, 0.2f, 2),
            new DualFatedChimeUtils.CurveSegment(DualFatedChimeUtils.PolyInEasing, 0.12f, -0.65f, 1.60f, 3),
            new DualFatedChimeUtils.CurveSegment(DualFatedChimeUtils.SineOutEasing, 0.78f, 0.95f, 0.05f, 2),
        };

        // Phase 1: Reversed toll sweep  Ebroad dramatic arc
        private static readonly DualFatedChimeUtils.CurveSegment[] TollSweepAnimation = new[]
        {
            new DualFatedChimeUtils.CurveSegment(DualFatedChimeUtils.PolyOutEasing, 0f, -1.0f, 0.30f, 2),
            new DualFatedChimeUtils.CurveSegment(DualFatedChimeUtils.PolyInEasing, 0.20f, -0.70f, 1.65f, 4),
            new DualFatedChimeUtils.CurveSegment(DualFatedChimeUtils.PolyOutEasing, 0.80f, 0.95f, 0.05f, 2),
        };

        // Phase 2: Grand Toll  Edramatic wind-back + explosive forward slam
        private static readonly DualFatedChimeUtils.CurveSegment[] GrandTollAnimation = new[]
        {
            new DualFatedChimeUtils.CurveSegment(DualFatedChimeUtils.SineBumpEasing, 0f, -1.0f, -0.18f, 2),
            new DualFatedChimeUtils.CurveSegment(DualFatedChimeUtils.PolyOutEasing, 0.10f, -1.18f, 0.48f, 2),
            new DualFatedChimeUtils.CurveSegment(DualFatedChimeUtils.PolyInEasing, 0.32f, -0.70f, 1.75f, 5),
            new DualFatedChimeUtils.CurveSegment(DualFatedChimeUtils.PolyOutEasing, 0.82f, 1.05f, -0.05f, 2),
        };

        #endregion

        #region State Properties

        public int ComboPhase => (int)Projectile.ai[0];

        public float Progression => Math.Clamp((float)Timer / SwingDuration, 0f, 1f);

        public int Timer => SwingDuration - Projectile.timeLeft;

        public int SwingDuration => ComboPhase switch
        {
            0 => Duration_BellStrike,
            1 => Duration_TollSweep,
            _ => Duration_GrandToll
        };

        public float BladeLength => ComboPhase switch
        {
            0 => BladeLength_BellStrike,
            1 => BladeLength_TollSweep,
            _ => BladeLength_GrandToll
        };

        public float MaxAngle => ComboPhase switch
        {
            0 => MaxSwingAngle_BellStrike,
            1 => MaxSwingAngle_TollSweep,
            _ => MaxSwingAngle_GrandToll
        };

        public DualFatedChimeUtils.CurveSegment[] CurrentAnimation => ComboPhase switch
        {
            0 => BellStrikeAnimation,
            1 => TollSweepAnimation,
            _ => GrandTollAnimation
        };

        public float DamageMultiplier => ComboPhase switch
        {
            0 => DamageMult_BellStrike,
            1 => DamageMult_TollSweep,
            _ => DamageMult_GrandToll
        };

        public bool IsFlipped => ComboPhase == 1;

        public int Direction { get; private set; }
        public float BaseRotation { get; private set; }

        private Player Owner => Main.player[Projectile.owner];
        private bool _initialized;
        private float _squishFactor;
        private bool _flamesSpawned;
        private bool _soundPlayed;

        private Vector2[] _trailPositions;
        private DualFatedChimePrimitiveRenderer _trailRenderer;

        #endregion

        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/DualFatedChime/DualFatedChime";

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

            Projectile.Center = Owner.MountedCenter;
            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;

            float animValue = DualFatedChimeUtils.PiecewiseAnimation(Progression, CurrentAnimation);
            int flipSign = IsFlipped ? -1 : 1;
            float swingAngle = animValue * MaxAngle * Direction * flipSign;
            float currentRotation = BaseRotation + swingAngle;

            Projectile.rotation = currentRotation;

            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, currentRotation - MathHelper.PiOver2);

            DoSwingVFX(currentRotation);

            // Phase 2: Fire flame waves at 70% progress
            if (ComboPhase == 2 && Progression >= 0.70f && !_flamesSpawned)
            {
                SpawnBellFlameWaves(currentRotation);
                _flamesSpawned = true;
            }

            // Play sound at 20% progress
            if (Progression >= 0.20f && !_soundPlayed)
            {
                SoundEngine.PlaySound(SoundID.Item71 with { Pitch = -0.1f + ComboPhase * 0.08f, Volume = 0.7f }, Projectile.Center);
                _soundPlayed = true;
            }

            // Shrink toward end
            float targetScale = 1f;
            if (Progression > 0.75f)
            {
                float shrinkProgress = (Progression - 0.75f) / 0.25f;
                Projectile.scale = MathHelper.Lerp(targetScale, 0.3f, shrinkProgress);
            }
            else
            {
                Projectile.scale = MathHelper.Lerp(Projectile.scale, targetScale, 0.15f);
            }

            // Infernal lighting along blade
            Vector2 swordDir = currentRotation.ToRotationVector2();
            for (int i = 0; i < 5; i++)
            {
                float bladeT = (float)i / 4f;
                Vector2 lightPos = Projectile.Center + swordDir * BladeLength * bladeT * Projectile.scale;
                float intensity = 0.5f * (1f - bladeT * 0.3f);
                Vector3 fireLight = new Vector3(0.8f, 0.35f + bladeT * 0.3f, 0.05f);
                Lighting.AddLight(lightPos, fireLight * intensity);
            }

            if (Projectile.timeLeft <= 0)
                Projectile.Kill();
        }

        private void Initialize()
        {
            _initialized = true;
            Direction = Math.Sign(Projectile.velocity.X) != 0 ? Math.Sign(Projectile.velocity.X) : 1;
            BaseRotation = Projectile.velocity.ToRotation();
            _squishFactor = ComboPhase switch { 0 => 0.88f, 1 => 0.85f, _ => 0.82f };
            Projectile.timeLeft = SwingDuration;
            _trailPositions = new Vector2[TrailPointCount];
            _trailRenderer = new DualFatedChimePrimitiveRenderer();

            Projectile.damage = (int)(Projectile.damage * DamageMultiplier);
            Owner.direction = Direction;
        }

        #region Swing VFX

        private void DoSwingVFX(float currentRotation)
        {
            Vector2 swordDir = currentRotation.ToRotationVector2();
            Vector2 tipPos = Projectile.Center + swordDir * BladeLength * Projectile.scale;

            // Infernal embers flying off blade during active swing
            if (Progression > 0.15f && Progression < 0.90f)
            {
                float dustChance = MathHelper.Lerp(0.4f, 1f, Progression);
                if (Main.rand.NextFloat() < dustChance)
                {
                    float bladeT = Main.rand.NextFloat(0.3f, 1f);
                    Vector2 dustPos = Projectile.Center + swordDir * BladeLength * bladeT * Projectile.scale;
                    Vector2 perpVel = new Vector2(-swordDir.Y, swordDir.X) * Main.rand.NextFloat(1.5f, 4f) * Direction;

                    // Fire dust
                    Dust d = Dust.NewDustPerfect(dustPos, DustID.Torch, perpVel, 0,
                        DualFatedChimeUtils.GetFireFlicker(bladeT), 1.3f);
                    d.noGravity = true;
                    d.fadeIn = 0.9f;

                    // Black smoke trail
                    if (Main.rand.NextBool(3))
                    {
                        Dust s = Dust.NewDustPerfect(dustPos, DustID.Smoke, perpVel * 0.4f, 80,
                            new Color(20, 15, 20), 1.5f);
                        s.noGravity = true;
                    }
                }
            }

            // Phase 1 midpoint: ember spark burst
            if (ComboPhase == 1 && Math.Abs(Progression - 0.55f) < 0.03f)
            {
                for (int i = 0; i < 6; i++)
                {
                    Vector2 vel = Main.rand.NextVector2CircularEdge(3f, 3f) + swordDir * 2f;
                    float heat = Main.rand.NextFloat(0.3f, 0.9f);
                    DualFatedChimeParticleHandler.SpawnParticle(
                        new InfernalEmberParticle(tipPos + Main.rand.NextVector2Circular(8f, 8f),
                            vel, heat, 20, 0.5f));
                }
            }

            // Phase 2 at 65%: smoke burst + music note sparks
            if (ComboPhase == 2 && Math.Abs(Progression - 0.65f) < 0.03f)
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector2 smokeVel = Main.rand.NextVector2Circular(3f, 3f) + new Vector2(0, -1f);
                    DualFatedChimeParticleHandler.SpawnParticle(
                        new BellSmokeParticle(tipPos + Main.rand.NextVector2Circular(15f, 15f),
                            smokeVel, 45, 1.3f, 0.7f));
                }

                for (int i = 0; i < 3; i++)
                {
                    Vector2 noteVel = Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0, -2f);
                    DualFatedChimeParticleHandler.SpawnParticle(
                        new MusicalFlameParticle(tipPos + Main.rand.NextVector2Circular(10f, 10f),
                            noteVel, 35, 0.6f));
                }
            }

            GenerateTrailPoints();
        }

        private void GenerateTrailPoints()
        {
            float trailLength = Math.Min(Progression, 0.5f);
            float startProgress = Math.Max(0f, Progression - trailLength);

            for (int i = 0; i < TrailPointCount; i++)
            {
                float t = MathHelper.Lerp(startProgress, Progression, (float)i / (TrailPointCount - 1));
                float animValue = DualFatedChimeUtils.PiecewiseAnimation(t, CurrentAnimation);
                int flipSign = IsFlipped ? -1 : 1;
                float angle = BaseRotation + animValue * MaxAngle * Direction * flipSign;
                Vector2 dir = angle.ToRotationVector2();
                _trailPositions[i] = Projectile.Center + dir * BladeLength * Projectile.scale;
            }
        }

        #endregion

        #region Bell Flame Wave Spawning

        private void SpawnBellFlameWaves(float rotation)
        {
            Vector2 swordDir = rotation.ToRotationVector2();
            Vector2 tipPos = Projectile.Center + swordDir * BladeLength * Projectile.scale;
            int count = 3;
            float spreadAngle = MathHelper.ToRadians(45f);

            for (int i = 0; i < count; i++)
            {
                float angleOffset = MathHelper.Lerp(-spreadAngle / 2f, spreadAngle / 2f,
                    count > 1 ? (float)i / (count - 1) : 0.5f);
                Vector2 flameDir = (rotation + angleOffset).ToRotationVector2();
                Vector2 flameVel = flameDir * 10f + Main.rand.NextVector2Circular(1f, 1f);

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    tipPos, flameVel,
                    ModContent.ProjectileType<BellFlameWaveProj>(),
                    Projectile.damage / 3, Projectile.knockBack * 0.3f,
                    Projectile.owner);

                // Ember burst at spawn
                DualFatedChimeParticleHandler.SpawnParticle(
                    new InfernalEmberParticle(tipPos, flameVel * 0.3f, 0.7f, 18, 0.5f));
            }

            // Bell chime flash at tip
            DualFatedChimeParticleHandler.SpawnParticle(
                new BellChimeFlashParticle(tipPos, 18, 1.8f));

            // Smoke burst
            for (int i = 0; i < 4; i++)
            {
                Vector2 smokeVel = Main.rand.NextVector2Circular(4f, 4f);
                DualFatedChimeParticleHandler.SpawnParticle(
                    new BellSmokeParticle(tipPos, smokeVel, 40, 1.5f, 0.6f));
            }

            SoundEngine.PlaySound(SoundID.Item45 with { Pitch = 0.2f, Volume = 0.6f }, tipPos);
        }

        #endregion

        #region Collision

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
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
            int expand = 20;
            hitbox.Inflate(expand, expand);
        }

        #endregion

        #region On Hit

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Resonant Toll
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);

            // Charge Inferno Waltz gauge
            Owner.DualFatedChime().AddCharge(DualFatedChimePlayer.ChargePerHit);

            // Impact VFX  Einfernal ember burst
            Vector2 hitPos = target.Center;
            for (int i = 0; i < 8; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(5f, 5f);
                float heat = Main.rand.NextFloat(0.4f, 0.9f);
                DualFatedChimeParticleHandler.SpawnParticle(
                    new InfernalEmberParticle(hitPos, sparkVel, heat, 22, 0.5f));
            }

            // Smoke puff on hit
            for (int i = 0; i < 3; i++)
            {
                Vector2 smokeVel = Main.rand.NextVector2Circular(2f, 2f);
                DualFatedChimeParticleHandler.SpawnParticle(
                    new BellSmokeParticle(hitPos + Main.rand.NextVector2Circular(10f, 10f),
                        smokeVel, 35, 1f, 0.5f));
            }

            // Bell chime flash on crit
            if (hit.Crit)
            {
                DualFatedChimeParticleHandler.SpawnParticle(
                    new BellChimeFlashParticle(hitPos, 20, 2f));

                // Music note sparks on crit
                for (int i = 0; i < 2; i++)
                {
                    Vector2 noteVel = Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0, -2f);
                    DualFatedChimeParticleHandler.SpawnParticle(
                        new MusicalFlameParticle(hitPos, noteVel, 30, 0.5f));
                }
            }
        }

        #endregion

        #region Rendering

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;

            // Layer 1: Shader-driven infernal swing trail
            if (Progression > 0.18f)
                DrawSlashTrail(sb);

            // Layer 2: Bloom underlays at blade tip
            DrawBloomUnderlays(sb);

            // Layer 3: Blade sprite
            DrawBlade(sb, lightColor);

            // Layer 4: Particles
            DualFatedChimeParticleHandler.DrawAllParticles(sb);

            return false;
        }

        private void DrawSlashTrail(SpriteBatch sb)
        {
            if (_trailPositions == null || _trailRenderer == null)
                return;

            // Infernal color scheme based on phase
            Color primaryColor = DualFatedChimeUtils.GetInfernalGradient(0.4f + ComboPhase * 0.1f);
            Color secondaryColor = DualFatedChimeUtils.GetInfernalGradient(0.6f + ComboPhase * 0.1f);
            Color edgeColor = DualFatedChimeUtils.GetInfernalGradient(0.8f); // Gold edge

            float trailOpacity = MathHelper.Clamp((Progression - 0.18f) / 0.15f, 0f, 1f);
            if (Progression > 0.85f)
                trailOpacity *= 1f - (Progression - 0.85f) / 0.15f;

            MiscShaderData shader = DualFatedChimeShaderLoader.GetSlashShader();

            if (shader != null)
            {
                shader.UseColor(primaryColor);
                shader.UseSecondaryColor(secondaryColor);

                try
                {
                    shader.Shader.Parameters["fireColor"]?.SetValue(edgeColor.ToVector4());
                    shader.Shader.Parameters["uTime"]?.SetValue(Main.GameUpdateCount * 0.025f);
                    shader.Shader.Parameters["flipped"]?.SetValue(IsFlipped);
                }
                catch { }

                try
                {
                    var noiseTexture = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/TileableFBMNoise",
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
            var mainSettings = new DualFatedChimeTrailSettings(
                width: (float t) =>
                {
                    float baseWidth = (1f - t * 0.6f) * BladeLength * 0.5f * Projectile.scale * _squishFactor;
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
                    shader.UseColor(edgeColor * 0.6f);
                    shader.UseSecondaryColor(new Color(255, 240, 200) * 0.3f);
                }
                catch { }
            }

            var glowSettings = new DualFatedChimeTrailSettings(
                width: (float t) =>
                {
                    float baseWidth = (1f - t * 0.5f) * BladeLength * 0.6f * Projectile.scale * _squishFactor;
                    return baseWidth * trailOpacity * 0.5f;
                },
                trailColor: (float t) =>
                {
                    return DualFatedChimeUtils.Additive(edgeColor, trailOpacity * 0.35f * (1f - t));
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
            float currentAngle = Projectile.rotation;
            Vector2 swordDir = currentAngle.ToRotationVector2();
            Vector2 tipPos = Projectile.Center + swordDir * BladeLength * Projectile.scale - Main.screenPosition;

            Texture2D bloomTex = null;
            try
            {
                bloomTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow",
                    ReLogic.Content.AssetRequestMode.ImmediateLoad)?.Value;
            }
            catch { }

            if (bloomTex == null) return;

            Vector2 bloomOrigin = new Vector2(bloomTex.Width / 2f, bloomTex.Height / 2f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            float bloomPulse = 0.8f + 0.2f * (float)Math.Sin(Main.GameUpdateCount * 0.15f);

            // Large outer flame glow  Eorange
            Color outerBloom = DualFatedChimeUtils.Additive(new Color(255, 100, 0), 0.2f * bloomPulse);
            sb.Draw(bloomTex, tipPos, null, outerBloom, 0f, bloomOrigin, 1.4f * Projectile.scale, SpriteEffects.None, 0f);

            // Mid bloom  Egold
            Color midBloom = DualFatedChimeUtils.Additive(new Color(255, 200, 50), 0.3f * bloomPulse);
            sb.Draw(bloomTex, tipPos, null, midBloom, 0f, bloomOrigin, 0.7f * Projectile.scale, SpriteEffects.None, 0f);

            // White-hot core
            Color coreBloom = DualFatedChimeUtils.Additive(new Color(255, 240, 200), 0.5f * bloomPulse);
            sb.Draw(bloomTex, tipPos, null, coreBloom, 0f, bloomOrigin, 0.3f * Projectile.scale, SpriteEffects.None, 0f);

            // Phase 2: Extra intense bloom
            if (ComboPhase == 2)
            {
                Color phaseBloom = DualFatedChimeUtils.Additive(new Color(255, 60, 0), 0.15f * bloomPulse);
                sb.Draw(bloomTex, tipPos, null, phaseBloom, 0f, bloomOrigin, 2.0f * Projectile.scale, SpriteEffects.None, 0f);
            }

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

            float squish = MathHelper.Lerp(_squishFactor, 1f, Progression);
            Vector2 squishScale = new Vector2(1f + (1f - squish) * 0.5f, squish) * Projectile.scale;

            // Shadow copy
            sb.Draw(bladeTex, drawPos + new Vector2(-1, 1), null,
                new Color(0, 0, 0, 100) * 0.3f, rot, origin, squishScale * 1.02f, effects, 0f);

            // Main blade
            sb.Draw(bladeTex, drawPos, null, lightColor, rot, origin, squishScale, effects, 0f);

            // Infernal fire glow overlay
            Color fireGlow = DualFatedChimeUtils.Additive(DualFatedChimeUtils.GetFireFlicker(), 0.2f);
            sb.Draw(bladeTex, drawPos, null, fireGlow, rot, origin, squishScale * 1.01f, effects, 0f);
        }

        #endregion

        public override void OnKill(int timeLeft)
        {
            _trailRenderer?.Dispose();
        }
    }
}
