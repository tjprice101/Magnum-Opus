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
    /// Right-click combo swing for the Sandbox Terra Blade.
    /// Two-phase held-projectile: quick downswing → quick upswing → spawns beam.
    /// Uses the same 4-layer Exoblade-style rendering pipeline as SandboxTerraBladeSwing.
    /// </summary>
    public class SandboxTerraBladeComboSwing : ModProjectile
    {
        #region Constants & Animation Curves

        private const int TrailLength = 60;

        // Phase 0: Quick downswing — starts above-forward, ends below
        private static readonly CurveSegment[] DownswingCurve = new CurveSegment[]
        {
            new CurveSegment(EasingType.SineIn,  0.00f, -1.0f,   0.25f, 2),  // Slow wind-up
            new CurveSegment(EasingType.PolyIn,  0.30f, -0.75f,  1.70f, 4),  // Explosive quartic flick
            new CurveSegment(EasingType.SineOut, 0.85f,  0.95f,  0.05f, 2),  // Snap stop
        };

        // Phase 1: Quick upswing — starts below, ends above-forward (flipped)
        private static readonly CurveSegment[] UpswingCurve = new CurveSegment[]
        {
            new CurveSegment(EasingType.SineIn,  0.00f,  1.0f,  -0.25f, 2),  // Slow wind-up
            new CurveSegment(EasingType.PolyIn,  0.30f,  0.75f, -1.70f, 4),  // Explosive quartic flick
            new CurveSegment(EasingType.SineOut, 0.85f, -0.95f, -0.05f, 2),  // Snap stop
        };

        private const float DownswingMaxAngle = MathHelper.PiOver2 * 2.0f;  // ~160°
        private const float UpswingMaxAngle = MathHelper.PiOver2 * 1.9f;    // ~150°

        private const int DownswingBaseDuration = 22;
        private const int UpswingBaseDuration = 20;

        private const float BladeLength = 160f;
        private const float SquishRange = 0.78f;

        #endregion

        #region Properties

        private Player Owner => Main.player[Projectile.owner];

        /// <summary>Current combo phase: 0 = downswing, 1 = upswing.</summary>
        private int ComboPhase
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        private int SwingTime
        {
            get => (int)Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }

        private float SquishFactor
        {
            get => Projectile.localAI[1];
            set => Projectile.localAI[1] = value;
        }

        private int Timer => SwingTime - Projectile.timeLeft;
        private float Progression => SwingTime > 0 ? (float)Timer / SwingTime : 0f;
        private int Direction => Math.Sign(Projectile.velocity.X) <= 0 ? -1 : 1;
        private float BaseRotation => Projectile.velocity.ToRotation();

        private CurveSegment[] CurrentCurve => ComboPhase == 0 ? DownswingCurve : UpswingCurve;
        private float CurrentMaxAngle => ComboPhase == 0 ? DownswingMaxAngle : UpswingMaxAngle;

        #endregion

        #region Animation Helpers

        private float SwingAngleShiftAtProgress(float progress)
        {
            return CurrentMaxAngle * PiecewiseAnimation(progress, CurrentCurve);
        }

        private float SwordRotationAtProgress(float progress)
        {
            return BaseRotation + SwingAngleShiftAtProgress(progress) * Direction;
        }

        private float SquishAtProgress(float progress)
        {
            // Compute angular velocity via finite difference
            float dp = 0.015f;
            float a0 = SwingAngleShiftAtProgress(MathHelper.Clamp(progress - dp, 0f, 1f));
            float a1 = SwingAngleShiftAtProgress(MathHelper.Clamp(progress + dp, 0f, 1f));
            float angularSpeed = MathHelper.Clamp(Math.Abs(a1 - a0) / (dp * 2f) * 0.25f, 0f, 1f);

            return MathHelper.Lerp(1f, SquishRange, angularSpeed);
        }

        private float SwordRotation => SwordRotationAtProgress(Progression);

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

        // Persistent smear particle that follows the blade (Calamity-style)
        private Particle swingSmear;

        #endregion

        #region Setup

        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.None;

        public override void SetStaticDefaults()
        {
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
            Projectile.extraUpdates = 2;
        }

        #endregion

        #region Collision

        public override bool ShouldUpdatePosition() => false;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 start = Owner.MountedCenter;
            Vector2 end = start + SwordDirection * BladeLength;
            float collisionPoint = 0f;
            return Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(), targetHitbox.Size(),
                start, end, 40f, ref collisionPoint);
        }

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
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
            int maxUpdates = 1 + Projectile.extraUpdates;
            int totalTime = (int)(DownswingBaseDuration / Owner.GetAttackSpeed(DamageClass.MeleeNoSpeed));
            SwingTime = Math.Max(totalTime * maxUpdates, 12);
            Projectile.timeLeft = SwingTime;

            SoundEngine.PlaySound(SoundID.Item1 with { Pitch = 0.3f, Volume = 0.9f }, Owner.Center);
        }

        private void DoBehavior_Swinging()
        {
            Projectile.Center = Owner.MountedCenter;

            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;

            float armRotation = SwordRotation - MathHelper.PiOver2;
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRotation);
            Owner.direction = Direction;

            // Track blade base and tip positions for trail rendering
            Vector2 baseWorld = Owner.MountedCenter;
            Vector2 tipWorld = baseWorld + SwordDirection * BladeLength;
            basePositions[trailIndex % TrailLength] = baseWorld;
            tipPositions[trailIndex % TrailLength] = tipWorld;
            tipRotations[trailIndex % TrailLength] = SwordRotation;
            trailIndex++;

            // Dust trail during active swing (scales with speed)
            if (Progression > 0.05f && Progression < 0.95f)
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
                        swingSmear.Time = 0;
                    }
                }

                // Screen Feel: Subtle Trauma + Ripple at Peak Speed
                if (localSwingSpeed > 0.3f)
                    Projectile.AddTrauma(0.04f * localSwingSpeed);

                if (localSwingSpeed > 0.6f && Timer % 8 == 0)
                {
                    Vector2 rippleTipWorld = Owner.MountedCenter + SwordDirection * BladeLength;
                    Color rippleColor = TerraBladeShaderManager.GetPaletteColor(0.5f);
                    ScreenDistortionManager.TriggerRipple(rippleTipWorld, rippleColor, localSwingSpeed * 0.4f, 12);
                }
            }

            // Beam Charge Buildup VFX (upswing final 30%)
            if (ComboPhase == 1 && Progression > 0.70f)
            {
                float chargeBuildup = (Progression - 0.70f) / 0.30f;
                Vector2 chargeTipWorld = Owner.MountedCenter + SwordDirection * BladeLength;

                // Converging green sparks
                if (Timer % 2 == 0)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        float dist = MathHelper.Lerp(60f, 15f, chargeBuildup);
                        Vector2 sparkStart = chargeTipWorld + angle.ToRotationVector2() * dist;
                        Vector2 sparkVel = (chargeTipWorld - sparkStart).SafeNormalize(Vector2.Zero) * 3f;
                        Color sparkColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.4f, 0.8f));
                        Dust d = Dust.NewDustPerfect(sparkStart, DustID.GreenTorch, sparkVel, 0, sparkColor,
                            0.6f + chargeBuildup * 0.6f);
                        d.noGravity = true;
                        d.fadeIn = 1.1f;
                    }
                }

                // Growing light at tip
                Color chargeLight = TerraBladeShaderManager.GetPaletteColor(0.6f);
                Lighting.AddLight(chargeTipWorld, chargeLight.ToVector3() * (0.3f + chargeBuildup * 0.8f));

                // Sound cue at 70% start
                if (Progression >= 0.70f && Progression < 0.72f)
                    SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.6f, Volume = 0.4f }, chargeTipWorld);
            }

            // Check for phase transition
            if (Projectile.timeLeft <= 1)
            {
                if (ComboPhase == 0)
                {
                    // Transition to upswing
                    TransitionToUpswing();
                }
                else
                {
                    // Upswing complete — fire the beam
                    SpawnBeam();
                    Projectile.Kill();
                }
            }
        }

        private void TransitionToUpswing()
        {
            ComboPhase = 1;

            int maxUpdates = 1 + Projectile.extraUpdates;
            int totalTime = (int)(UpswingBaseDuration / Owner.GetAttackSpeed(DamageClass.MeleeNoSpeed));
            SwingTime = Math.Max(totalTime * maxUpdates, 12);
            Projectile.timeLeft = SwingTime;

            // Reset trail buffer for clean upswing trail
            trailIndex = 0;

            SoundEngine.PlaySound(SoundID.Item1 with { Pitch = 0.5f, Volume = 0.85f }, Owner.Center);
        }

        private void SpawnBeam()
        {
            if (Main.myPlayer != Projectile.owner) return;

            Vector2 toMouse = (Main.MouseWorld - Owner.MountedCenter).SafeNormalize(Vector2.UnitX);
            Vector2 bladeTip = Owner.MountedCenter + toMouse * BladeLength;

            int beamIndex = Projectile.NewProjectile(
                Projectile.GetSource_FromThis(), bladeTip, Vector2.Zero,
                ModContent.ProjectileType<SandboxTerraBladeBeam>(),
                Projectile.damage, Projectile.knockBack, Projectile.owner);

            // Initialize cursor position in the beam's localAI slots
            if (beamIndex >= 0 && beamIndex < Main.maxProjectiles)
            {
                Main.projectile[beamIndex].localAI[0] = Main.MouseWorld.X;
                Main.projectile[beamIndex].localAI[1] = Main.MouseWorld.Y;
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

            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                Dust d = Dust.NewDustPerfect(hitPos, DustID.GreenTorch, vel, 0,
                    TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat()), 1.3f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Dust d = Dust.NewDustPerfect(hitPos, DustID.Enchanted_Gold, vel, 0, Color.White, 0.9f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            for (int i = 0; i < 3; i++)
            {
                Vector2 noteVel = Main.rand.NextVector2Circular(3f, 3f) + new Vector2(0f, -1.5f);
                Color noteColor = TerraBladeShaderManager.GetPaletteColor(0.3f + Main.rand.NextFloat() * 0.5f);
                float noteScale = Main.rand.NextFloat(0.7f, 1.0f);
                ThemedParticles.MusicNote(hitPos, noteVel, noteColor, noteScale, 35);
            }

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
            writer.Write(ComboPhase);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            SwingTime = reader.ReadInt32();
            SquishFactor = reader.ReadSingle();
            ComboPhase = reader.ReadInt32();
        }

        #endregion
    }
}
