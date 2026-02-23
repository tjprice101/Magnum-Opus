using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Effects;
using MagnumOpus.Common.Systems.VFX.Screen;
using MagnumOpus.Content.TestWeapons.SandboxWeapons.Shaders;
using MagnumOpus.Common.Systems.Particles;
using static MagnumOpus.Common.Systems.Particles.Particle;

namespace MagnumOpus.Content.TestWeapons.SandboxWeapons
{
    /// <summary>
    /// Held-projectile swing for the Sandbox Terra Blade — Exoblade-style rendering.
    /// Implements a held swing with a clean 4-layer rendering pipeline:
    ///
    ///   Layer 1: Slash Trail — 40-point arc trail with 2-pass glow strips (additive)
    ///   Layer 2: Blade — Sprite with optional SwingSpriteShader deformation
    ///   Layer 3: Lens Flare — Cross-oriented flare at blade tip during peak swing
    ///   Layer 4: Lighting — Dynamic palette-colored lighting
    ///
    /// Uses squash-and-stretch via SwordDirection.
    /// </summary>
    public class SandboxTerraBladeSwing : ModProjectile
    {
        #region Constants & Animation Curve

        /// <summary>
        /// Number of tip positions to store for the arc trail.
        /// </summary>
        private const int TrailLength = 60;

        /// <summary>
        /// The swing animation curve (full 360° revolution):
        /// Segment 1 (0-15%): Gentle wind-up (SineIn easing — subtle anticipation)
        /// Segment 2 (15-82%): Fast acceleration through main arc (PolyIn cubic — snappy flick)
        /// Segment 3 (82-100%): Smooth deceleration to stop (SineOut — clean finish, no overshoot)
        /// </summary>
        private static readonly CurveSegment[] SwingCurve = new CurveSegment[]
        {
            new CurveSegment(EasingType.SineIn,  0.00f, -0.50f,  0.10f, 2),  // Long slow wind-up
            new CurveSegment(EasingType.PolyIn,  0.30f, -0.40f,  0.86f, 4),  // Explosive quartic flick
            new CurveSegment(EasingType.SineOut, 0.85f,  0.46f,  0.04f, 2),  // Snap stop
        };

        /// <summary>
        /// Total arc of the swing in radians. Full 360° revolution.
        /// </summary>
        private const float MaxSwingAngle = MathHelper.TwoPi;

        /// <summary>
        /// Duration of the swing in game frames (before attack speed scaling).
        /// </summary>
        private const int BaseDuration = 48;

        /// <summary>
        /// Visual length of the blade in pixels (how far the tip reaches from the player center).
        /// </summary>
        private const float BladeLength = 160f;

        /// <summary>
        /// How much the blade squishes at the extremes of the swing (1.0 = no squish).
        /// Lower values = more squash-and-stretch feel.
        /// </summary>
        private const float SquishRange = 0.78f;

        #endregion

        #region Properties

        private Player Owner => Main.player[Projectile.owner];

        /// <summary>Total time for this swing in update ticks.</summary>
        private int SwingTime
        {
            get => (int)Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }

        /// <summary>Current squish factor for squash-and-stretch.</summary>
        private float SquishFactor
        {
            get => Projectile.localAI[1];
            set => Projectile.localAI[1] = value;
        }

        /// <summary>Frames elapsed since swing start.</summary>
        private int Timer => SwingTime - Projectile.timeLeft;

        /// <summary>Normalized progress through the swing (0 → 1).</summary>
        private float Progression => SwingTime > 0 ? (float)Timer / SwingTime : 0f;

        /// <summary>Swing direction: 1 = right-to-left, -1 = left-to-right.</summary>
        private int Direction => Math.Sign(Projectile.velocity.X) <= 0 ? -1 : 1;

        /// <summary>Base rotation angle derived from the initial velocity (toward cursor).</summary>
        private float BaseRotation => Projectile.velocity.ToRotation();

        #endregion

        #region Animation Helpers

        /// <summary>
        /// Gets the angular offset from the base rotation at a given progress point.
        /// </summary>
        private float SwingAngleShiftAtProgress(float progress)
        {
            return MaxSwingAngle * PiecewiseAnimation(progress, SwingCurve);
        }

        /// <summary>
        /// Gets the absolute sword rotation at a given progress point.
        /// </summary>
        private float SwordRotationAtProgress(float progress)
        {
            return BaseRotation + SwingAngleShiftAtProgress(progress) * Direction;
        }

        /// <summary>
        /// Gets the squish factor at a given progress point for squash-and-stretch.
        /// Uses angular velocity (how fast the blade is moving) rather than absolute angle,
        /// so the blade only stretches when actually swinging fast.
        /// </summary>
        private float SquishAtProgress(float progress)
        {
            // Compute angular velocity via finite difference
            float dp = 0.015f;
            float a0 = SwingAngleShiftAtProgress(MathHelper.Clamp(progress - dp, 0f, 1f));
            float a1 = SwingAngleShiftAtProgress(MathHelper.Clamp(progress + dp, 0f, 1f));
            float angularSpeed = MathHelper.Clamp(Math.Abs(a1 - a0) / (dp * 2f) * 0.25f, 0f, 1f);

            // Lerp: slow movement → no squish (1.0), fast movement → full squish (SquishRange)
            return MathHelper.Lerp(1f, SquishRange, angularSpeed);
        }

        /// <summary>Current sword rotation accounting for direction and animation.</summary>
        private float SwordRotation => SwordRotationAtProgress(Progression);

        /// <summary>Unit direction vector from player center toward blade tip, with squish applied.</summary>
        private Vector2 SwordDirection
        {
            get
            {
                float rot = SwordRotation;
                Vector2 dir = rot.ToRotationVector2();
                float squish = SquishAtProgress(Progression);
                SquishFactor = squish;
                return dir * squish;
            }
        }

        #endregion

        #region Trail Point Generation

        /// <summary>
        /// Trailing edge progress — determines how far back the slash trail extends.
        /// Adapted from Exoblade reference implementation.
        /// </summary>
        private float TrailEndProgression
        {
            get
            {
                float endProgression;
                if (Progression < 0.75f)
                    endProgression = Progression - 0.5f + 0.1f * (Progression / 0.75f);
                else
                    endProgression = Progression - 0.4f * (1 - (Progression - 0.75f) / 0.75f);
                return Math.Clamp(endProgression, 0, 1);
            }
        }

        /// <summary>
        /// Maps a trail completion value (0=leading edge, 1=trailing edge) to actual swing progress.
        /// </summary>
        private float RealProgressionAtTrailCompletion(float completion)
            => MathHelper.Lerp(Progression, TrailEndProgression, completion);

        /// <summary>
        /// Gets the direction vector at a given progress with squish-deformed angle.
        /// Uses the squish factor to warp the angle for more organic trail shape.
        /// </summary>
        private Vector2 DirectionAtProgressScuffed(float progress)
        {
            float angleShift = SwingAngleShiftAtProgress(progress);
            float squish = SquishAtProgress(progress);
            Vector2 anglePoint = angleShift.ToRotationVector2();
            anglePoint.X *= (1f + (1f - squish) * 0.6f);
            anglePoint.Y *= squish;
            angleShift = anglePoint.ToRotation();
            return (BaseRotation + angleShift * Direction).ToRotationVector2() * squish;
        }

        /// <summary>
        /// Generates 40 arc points for the slash trail, from leading edge to trailing edge.
        /// Each point is an offset from Projectile.Center (player center).
        /// </summary>
        private List<Vector2> GenerateSlashPoints()
        {
            List<Vector2> result = new List<Vector2>();
            for (int i = 0; i < 40; i++)
            {
                float progress = MathHelper.Lerp(Progression, TrailEndProgression, i / 40f);
                result.Add(DirectionAtProgressScuffed(progress) * (BladeLength - 6f) * Projectile.scale);
            }
            return result;
        }

        #endregion

        #region Trail Tracking

        private Vector2[] basePositions = new Vector2[TrailLength];
        private Vector2[] tipPositions = new Vector2[TrailLength];
        private float[] tipRotations = new float[TrailLength];
        private int trailIndex = 0;
        private int shardSpawnCount = 0;

        // Persistent smear particle that follows the blade (Calamity-style)
        private Particle swingSmear;

        #endregion

        #region Setup

        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.None;

        public override void SetStaticDefaults()
        {
            // Store trail positions for arc trail rendering
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TrailLength;
        }

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 9999;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 24;

            // Multiple updates per frame for smoother animation
            Projectile.extraUpdates = 2;
        }

        #endregion

        #region Collision

        public override bool ShouldUpdatePosition() => false;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Line collision from player center to blade tip
            Vector2 start = Owner.MountedCenter;
            Vector2 end = start + SwordDirection * BladeLength;
            float collisionPoint = 0f;

            return Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(), targetHitbox.Size(),
                start, end, 40f, ref collisionPoint);
        }

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            // Center hitbox on the midpoint of the blade
            Vector2 tipPos = Owner.MountedCenter + SwordDirection * BladeLength * 0.5f;
            hitbox.X = (int)tipPos.X - hitbox.Width / 2;
            hitbox.Y = (int)tipPos.Y - hitbox.Height / 2;
        }

        #endregion

        #region AI

        private bool initialized = false;

        public override void AI()
        {
            if (!initialized)
            {
                InitializeSwing();
                initialized = true;
            }

            if (Owner.dead || !Owner.active)
            {
                Projectile.Kill();
                return;
            }

            DoBehavior_Swinging();
        }

        private void InitializeSwing()
        {
            // Calculate swing time based on attack speed, accounting for extra updates
            int maxUpdates = 1 + Projectile.extraUpdates; // 3 total (1 base + 2 extra)
            int totalTime = (int)(BaseDuration / Owner.GetAttackSpeed(DamageClass.MeleeNoSpeed));
            SwingTime = Math.Max(totalTime * maxUpdates, 12);
            Projectile.timeLeft = SwingTime;

            shardSpawnCount = 0;
            SoundEngine.PlaySound(SoundID.Item1 with { Pitch = 0.2f, Volume = 0.8f }, Owner.Center);
        }

        private void DoBehavior_Swinging()
        {
            // Lock projectile to owner
            Projectile.Center = Owner.MountedCenter;

            // Keep as held projectile
            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;

            // Rotate player arm to follow the sword
            float armRotation = SwordRotation - MathHelper.PiOver2;
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRotation);

            // Face the swing direction
            Owner.direction = Direction;

            // Track blade base and tip positions for trail rendering
            Vector2 baseWorld = Owner.MountedCenter;
            Vector2 tipWorld = baseWorld + SwordDirection * BladeLength;
            basePositions[trailIndex % TrailLength] = baseWorld;
            tipPositions[trailIndex % TrailLength] = tipWorld;
            tipRotations[trailIndex % TrailLength] = SwordRotation;
            trailIndex++;

            // Dense dust trail from blade during active swing (scales with speed)
            if (Progression > 0.03f && Progression < 0.97f)
            {
                // Compute local swing speed for dust scaling
                float dp = 0.02f;
                float a0 = SwingAngleShiftAtProgress(Progression);
                float a1 = SwingAngleShiftAtProgress(Math.Min(Progression + dp, 1f));
                float localSwingSpeed = MathHelper.Clamp(Math.Abs(a1 - a0) / dp * 0.3f, 0f, 1f);

                int dustCount = 1 + (int)(localSwingSpeed * 4);
                for (int i = 0; i < dustCount; i++)
                {
                    Vector2 dustPos = Owner.MountedCenter + SwordDirection * BladeLength * Main.rand.NextFloat(0.4f, 1f);
                    Dust d = Dust.NewDustPerfect(dustPos, DustID.GreenTorch,
                        -SwordDirection * Main.rand.NextFloat(1f, 3f), 0,
                        TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.2f, 0.8f)), 1.5f);
                    d.noGravity = true;
                    d.fadeIn = 1.2f;
                }

                // GlowSpark particles along blade (sparkle dust)
                if (Timer % 2 == 0)
                {
                    float bladeT = Main.rand.NextFloat(0.3f, 1f);
                    Vector2 sparkPos = Owner.MountedCenter + SwordDirection * BladeLength * bladeT;
                    Vector2 perpendicular = new Vector2(-SwordDirection.Y, SwordDirection.X);
                    Vector2 sparkVel = perpendicular * Main.rand.NextFloat(-2f, 2f) + -SwordDirection * Main.rand.NextFloat(0.5f, 1.5f);
                    Color sparkColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.3f, 0.8f));
                    var spark = new GlowSparkParticle(sparkPos, sparkVel, sparkColor, Main.rand.NextFloat(0.15f, 0.30f), 15);
                    MagnumParticleHandler.SpawnParticle(spark);
                }

                // Persistent Smear Particle (Calamity CircularSmearSmokeyVFX)
                if (Progression > 0.10f && Progression < 0.90f)
                {
                    if (swingSmear == null || swingSmear.Time >= swingSmear.Lifetime)
                    {
                        Color smearColor = TerraBladeShaderManager.GetPaletteColor(0.4f) * 0.5f;
                        swingSmear = new CircularSmearSmokeyVFX(
                            Owner.MountedCenter + SwordDirection * BladeLength * 0.5f,
                            smearColor, SwordRotation + MathHelper.PiOver4,
                            MathHelper.Lerp(2.0f, 3.2f, localSwingSpeed));
                        MagnumParticleHandler.SpawnParticle(swingSmear);
                    }
                    else
                    {
                        swingSmear.Position = Owner.MountedCenter + SwordDirection * BladeLength * 0.5f;
                        swingSmear.Rotation = SwordRotation + MathHelper.PiOver4 + (Direction < 0 ? MathHelper.Pi : 0f);
                        swingSmear.Scale = MathHelper.Lerp(2.0f, 3.2f, localSwingSpeed);
                        swingSmear.Time = 0; // Keep alive
                    }
                }

                // Screen Feel: Subtle Trauma + Ripple at Peak Speed
                if (localSwingSpeed > 0.3f)
                    Projectile.AddTrauma(0.04f * localSwingSpeed);

                // Localized screen ripple at blade tip during peak velocity
                if (localSwingSpeed > 0.6f && Timer % 8 == 0)
                {
                    Vector2 rippleTipWorld = Owner.MountedCenter + SwordDirection * BladeLength;
                    Color rippleColor = TerraBladeShaderManager.GetPaletteColor(0.5f);
                    ScreenDistortionManager.TriggerRipple(rippleTipWorld, rippleColor, localSwingSpeed * 0.4f, 12);
                }

                // Spawn 4 crystal bolts spread across 50-85% of swing (Exoblade pattern)
                const int maxShards = 4;
                const float spawnStart = 0.50f;
                const float spawnEnd = 0.85f;
                float spawnInterval = (spawnEnd - spawnStart) / maxShards;

                if (Progression >= spawnStart + shardSpawnCount * spawnInterval && shardSpawnCount < maxShards)
                {
                    if (Main.myPlayer == Projectile.owner)
                    {
                        Vector2 starPos = Owner.MountedCenter + SwordDirection * BladeLength * 0.8f;
                        Vector2 toMouse = (Main.MouseWorld - starPos).SafeNormalize(Vector2.UnitX);
                        float angle = toMouse.ToRotation() + Main.rand.NextFloat(-0.08f, 0.08f);
                        Vector2 starVel = angle.ToRotationVector2() * 18f;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), starPos, starVel,
                            ModContent.ProjectileType<CrispStarProjectile>(), Projectile.damage / 3, 0f, Projectile.owner,
                            ai0: shardSpawnCount);
                    }
                    shardSpawnCount++;
                }
            }
        }

        #endregion

        #region Draw Context

        /// <summary>Shared draw locals for the 4-layer rendering pipeline.</summary>
        private struct DrawContext
        {
            public Texture2D BladeTex;
            public Vector2 Origin;
            public Vector2 DrawPos;
            public Vector2 Scale;
            public float Rotation;
            public SpriteEffects Effects;
            public Vector2 TipScreen;
            public Vector2 TipWorld;
            public float SwingSpeed;
        }

        #endregion

        #region Rendering

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            PrepareDrawLocals(out var ctx);

            DrawSlashTrail(sb, in ctx);
            DrawBladeSprite(sb, in ctx, lightColor);
            DrawLensFlare(sb, in ctx);
            DrawDynamicLighting(in ctx);

            return false;
        }

        private void PrepareDrawLocals(out DrawContext ctx)
        {
            Texture2D bladeTex = Terraria.GameContent.TextureAssets.Item[ItemID.TerraBlade].Value;
            Vector2 tipWorld = Owner.MountedCenter + SwordRotation.ToRotationVector2() * BladeLength;

            float dp = 0.02f;
            float a0 = SwingAngleShiftAtProgress(Progression);
            float a1 = SwingAngleShiftAtProgress(Math.Min(Progression + dp, 1f));
            float swingSpeed = MathHelper.Clamp(Math.Abs(a1 - a0) / dp * 0.3f, 0f, 1f);

            float baseScale = BladeLength / bladeTex.Height;
            float squish = SquishAtProgress(Progression);

            ctx = new DrawContext
            {
                BladeTex = bladeTex,
                Origin = new Vector2(0, bladeTex.Height),
                DrawPos = Owner.MountedCenter - Main.screenPosition,
                Scale = new Vector2(baseScale * (1f + (1f - squish) * 0.6f), baseScale * squish),
                Rotation = SwordRotation + (Direction == -1 ? MathHelper.Pi : 0),
                Effects = Direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                TipWorld = tipWorld,
                TipScreen = tipWorld - Main.screenPosition,
                SwingSpeed = swingSpeed
            };
        }

        // ═══════════════════════════════════════════════════════════════
        // LAYER 1: Slash Trail (Exoblade-style 40-point arc with 2-pass glow strips)
        // ═══════════════════════════════════════════════════════════════
        private void DrawSlashTrail(SpriteBatch sb, in DrawContext ctx)
        {
            if (Progression < 0.35f)
                return;

            List<Vector2> slashPoints = GenerateSlashPoints();
            if (slashPoints.Count < 2)
                return;

            Texture2D glowTex = Terraria.GameContent.TextureAssets.Extra[98].Value;
            SwingShaderSystem.BeginAdditive(sb);

            for (int i = 0; i < slashPoints.Count - 1; i++)
            {
                float completion = (float)i / slashPoints.Count;
                float opacity = Utils.GetLerpValue(0.95f, 0.4f, completion, true) * Projectile.Opacity;
                float realProgress = RealProgressionAtTrailCompletion(completion);
                float width = SquishAtProgress(realProgress) * Projectile.scale * 55f;

                Vector2 start = Projectile.Center + slashPoints[i] - Main.screenPosition;
                Vector2 end = Projectile.Center + slashPoints[i + 1] - Main.screenPosition;

                // TerraBlade green palette color (maps completion to 0.15-0.85 range)
                float colorProgress = (completion + Progression * 0.5f) % 1f;
                Color trailColor = TerraBladeShaderManager.GetPaletteColor(colorProgress * 0.7f + 0.15f);
                trailColor.A = 0;

                Vector2 diff = end - start;
                float rotation = diff.ToRotation();
                float length = diff.Length();
                if (length < 1f) continue;

                // Pass 1: Outer glow
                sb.Draw(glowTex, start, null, trailColor * opacity * 0.7f, rotation,
                    new Vector2(0, glowTex.Height / 2f), new Vector2(length / glowTex.Width, width / glowTex.Height * 0.6f),
                    SpriteEffects.None, 0f);

                // Pass 2: White-hot core
                Color coreColor = Color.Lerp(trailColor, Color.White, 0.4f);
                coreColor.A = 0;
                sb.Draw(glowTex, start, null, coreColor * opacity * 0.9f, rotation,
                    new Vector2(0, glowTex.Height / 2f), new Vector2(length / glowTex.Width, width / glowTex.Height * 0.3f),
                    SpriteEffects.None, 0f);
            }

            SwingShaderSystem.RestoreSpriteBatch(sb);
        }

        // ═══════════════════════════════════════════════════════════════
        // LAYER 2: Blade Sprite (shader deformation or additive glow fallback)
        // ═══════════════════════════════════════════════════════════════
        private void DrawBladeSprite(SpriteBatch sb, in DrawContext ctx, Color lightColor)
        {
            bool shaderApplied = SwingShaderSystem.ApplySwingShader(sb, SwingAngleShiftAtProgress(Progression), 0.05f, Color.White);

            if (!shaderApplied)
            {
                // Fallback: additive glow layer behind the blade
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                Color glowColor = TerraBladeShaderManager.GetPaletteColor(Progression * 0.7f + 0.15f);
                glowColor.A = 0;
                float glowIntensity = (float)Math.Sin(Progression * MathHelper.Pi) * 0.6f;
                sb.Draw(ctx.BladeTex, ctx.DrawPos, null, glowColor * glowIntensity,
                    ctx.Rotation, ctx.Origin, ctx.Scale * 1.15f, ctx.Effects, 0f);

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            // Main blade draw
            sb.Draw(ctx.BladeTex, ctx.DrawPos, null, lightColor,
                ctx.Rotation, ctx.Origin, ctx.Scale, ctx.Effects, 0f);

            if (shaderApplied)
                SwingShaderSystem.RestoreSpriteBatch(sb);
        }

        // ═══════════════════════════════════════════════════════════════
        // LAYER 3: Lens Flare at blade tip during peak swing
        // ═══════════════════════════════════════════════════════════════
        private void DrawLensFlare(SpriteBatch sb, in DrawContext ctx)
        {
            // Only show during peak motion (25-85% of swing)
            float lensFlareOpacity = 0f;
            if (Progression >= 0.25f)
            {
                lensFlareOpacity = (float)Math.Sin(MathHelper.Pi * (Progression - 0.25f) / 0.6f) * 0.85f;
                lensFlareOpacity = Math.Clamp(lensFlareOpacity, 0f, 1f);
            }
            if (lensFlareOpacity <= 0f) return;

            Texture2D shineTex = Terraria.GameContent.TextureAssets.Extra[98].Value;
            Vector2 shineScale = new Vector2(0.8f, 2.5f);

            SwingShaderSystem.BeginAdditive(sb);

            Color lensFlareColor = TerraBladeShaderManager.GetPaletteColor(Progression * 0.7f + 0.15f);
            lensFlareColor.A = 0;

            // Blade tip position
            Vector2 bladePos = Owner.MountedCenter + DirectionAtProgressScuffed(Progression) * Projectile.scale * BladeLength;
            Vector2 bladePosScreen = bladePos - Main.screenPosition;

            // Vertical flare
            sb.Draw(shineTex, bladePosScreen, null,
                lensFlareColor * lensFlareOpacity, MathHelper.PiOver2, shineTex.Size() / 2f,
                shineScale * Projectile.scale, SpriteEffects.None, 0f);

            // Horizontal flare (dimmer, smaller)
            sb.Draw(shineTex, bladePosScreen, null,
                lensFlareColor * lensFlareOpacity * 0.5f, 0f, shineTex.Size() / 2f,
                shineScale * Projectile.scale * 0.6f, SpriteEffects.None, 0f);

            SwingShaderSystem.RestoreSpriteBatch(sb);
        }

        // ═══════════════════════════════════════════════════════════════
        // LAYER 4: Dynamic Lighting
        // ═══════════════════════════════════════════════════════════════
        private void DrawDynamicLighting(in DrawContext ctx)
        {
            Color tipLight = TerraBladeShaderManager.GetPaletteColor(0.5f);
            Lighting.AddLight(ctx.TipWorld, tipLight.ToVector3() * 0.4f * ctx.SwingSpeed);

            Vector2 midWorld = Owner.MountedCenter + SwordDirection * BladeLength * 0.5f;
            Lighting.AddLight(midWorld, tipLight.ToVector3() * 0.2f);
        }

        #endregion

        #region Hit Effects

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 hitPos = target.Center;

            // Screen-level impact effects
            ScreenDistortionManager.TriggerChromaticBurst(hitPos, intensity: 0.5f, duration: 10);
            ScreenFlashSystem.Instance?.ImpactFlash(0.25f);
            Projectile.ShakeScreen(0.4f);

            // Green torch dust burst — palette gradient
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                Dust d = Dust.NewDustPerfect(hitPos, DustID.GreenTorch, vel, 0,
                    TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat()), 1.3f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // Contrasting white/gold sparks
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Dust d = Dust.NewDustPerfect(hitPos, DustID.Enchanted_Gold, vel, 0, Color.White, 0.9f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // Visible music notes — THIS IS A MUSIC MOD (scale 0.7f+)
            for (int i = 0; i < 3; i++)
            {
                Vector2 noteVel = Main.rand.NextVector2Circular(3f, 3f) + new Vector2(0f, -1.5f);
                Color noteColor = TerraBladeShaderManager.GetPaletteColor(0.3f + Main.rand.NextFloat() * 0.5f);
                float noteScale = Main.rand.NextFloat(0.7f, 1.0f);
                ThemedParticles.MusicNote(hitPos, noteVel, noteColor, noteScale, 35);
            }

            // Bright lighting flash
            Lighting.AddLight(hitPos, 0.5f, 1f, 0.6f);

            // Impact light beams (3-4 radially distributed)
            if (Main.myPlayer == Projectile.owner)
            {
                int beamCount = Main.rand.Next(3, 5);
                float baseAngle = SwordRotation;
                float spread = MathHelper.TwoPi / beamCount;
                for (int i = 0; i < beamCount; i++)
                {
                    float angle = baseAngle + spread * i + Main.rand.NextFloat(-0.3f, 0.3f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), hitPos, Vector2.Zero,
                        ModContent.ProjectileType<ImpactLightBeamVFX>(), 0, 0f, Projectile.owner, ai0: angle);
                }

                // Anamorphic streaks (1-2 perpendicular to blade)
                int streakCount = Main.rand.Next(1, 3);
                for (int i = 0; i < streakCount; i++)
                {
                    float streakRot = SwordRotation + MathHelper.PiOver2 + Main.rand.NextFloat(-0.2f, 0.2f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), hitPos, Vector2.Zero,
                        ModContent.ProjectileType<AnamorphicStreakVFX>(), 0, 0f, Projectile.owner, ai0: streakRot);
                }
            }
        }

        #endregion

        #region Networking

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(SwingTime);
            writer.Write(SquishFactor);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            SwingTime = reader.ReadInt32();
            SquishFactor = reader.ReadSingle();
        }

        #endregion
    }
}
