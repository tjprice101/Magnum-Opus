using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MagnumOpus.Content.SandboxExoblade.Primitives;
using MagnumOpus.Content.SandboxExoblade.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static MagnumOpus.Content.SandboxExoblade.Utilities.ExobladeUtils;

namespace MagnumOpus.Common.BaseClasses
{
    /// <summary>
    /// Base class for Exoblade-architecture melee swing projectiles.
    /// Provides complete swing animation, GPU primitive trail rendering, dash mechanics,
    /// and a multi-layer VFX pipeline. Derived classes provide theme colors, blade dimensions,
    /// and optional behavior hooks for sub-projectiles and on-hit effects.
    /// </summary>
    public abstract class ExobladeStyleSwing : ModProjectile
    {
        #region === REQUIRED OVERRIDES ===

        /// <summary>Blade reach in pixels from player center to blade tip.</summary>
        protected abstract float BladeLength { get; }

        /// <summary>Swing duration in ticks (with MaxUpdates=3, effective frames = this/3).</summary>
        protected abstract int BaseSwingFrames { get; }

        /// <summary>Primary slash trail color — the dominant glow color of the arc.</summary>
        protected abstract Color SlashPrimaryColor { get; }

        /// <summary>Secondary slash trail color — the deeper/shadow accent of the arc.</summary>
        protected abstract Color SlashSecondaryColor { get; }

        /// <summary>Fire/accent color — hot highlights and lens flare tint.</summary>
        protected abstract Color SlashAccentColor { get; }

        #endregion

        #region === OPTIONAL VISUAL OVERRIDES ===

        /// <summary>Noise texture for the slash trail shader. Override for theme-specific noise.</summary>
        protected virtual string NoiseTexturePath => "MagnumOpus/Content/SandboxExoblade/Textures/VoronoiShapes";

        /// <summary>Streak texture for the pierce/dash trail shader.</summary>
        protected virtual string StreakTexturePath => "MagnumOpus/Content/SandboxExoblade/Textures/EternityStreak";

        /// <summary>Lens flare texture drawn at the blade tip during swings.</summary>
        protected virtual string LensFlareTexturePath => "MagnumOpus/Content/SandboxExoblade/Textures/HalfStar";

        /// <summary>Override to provide a square texture for the SwingSprite UV-rotation shader.
        /// Return null (default) to use simple rotation drawing instead.</summary>
        protected virtual string SquareTexturePath => null;

        /// <summary>Color gradient LUT texture path for theme-specific slash coloring.
        /// Return null (default) to use SlashPrimaryColor/SecondaryColor/AccentColor.
        /// When set, the slash trail samples colors from this gradient ramp texture.</summary>
        protected virtual string GradientLUTPath => null;

        /// <summary>Swing arc width. Default 1.8 produces a ~162° arc (PiOver2 * 1.8).</summary>
        protected virtual float SwingArcMultiplier => 1.8f;

        /// <summary>Lens flare color at the blade tip, blended by swing progression.</summary>
        protected virtual Color GetLensFlareColor(float progression)
            => Color.Lerp(SlashPrimaryColor, SlashAccentColor, (float)Math.Pow(progression, 3));

        /// <summary>Dynamic lighting color emitted by the blade.</summary>
        protected virtual Color GetLightColor(float progression)
            => Color.Lerp(SlashPrimaryColor, SlashAccentColor, (float)Math.Pow(progression, 3));

        /// <summary>Dust color for particles shed during swing.</summary>
        protected virtual Color GetSwingDustColor()
            => Color.Lerp(SlashPrimaryColor, SlashAccentColor, Main.rand.NextFloat());

        /// <summary>Pierce trail primary color during dash, cycling over time.</summary>
        protected virtual Color GetPierceMainColor(float timeWrapped)
            => MulticolorLerp(timeWrapped, SlashPrimaryColor, Color.Lerp(SlashPrimaryColor, Color.White, 0.5f), SlashAccentColor);

        /// <summary>Pierce trail secondary color during dash.</summary>
        protected virtual Color GetPierceSecondaryColor(float timeWrapped)
            => MulticolorLerp(timeWrapped, SlashSecondaryColor, Color.Lerp(SlashSecondaryColor, Color.White, 0.3f), SlashAccentColor);

        /// <summary>Energy glow color around blade during dash lunge.</summary>
        protected virtual Color GetDashEnergyColor(float progression)
            => Color.Lerp(SlashPrimaryColor, SlashAccentColor, progression) with { A = 0 };

        /// <summary>Scale of the lens flare effect. X=horizontal, Y=vertical.</summary>
        protected virtual Vector2 LensFlareScale => new Vector2(1f, 3f);

        /// <summary>Maximum lens flare opacity (0-1).</summary>
        protected virtual float LensFlareMaxOpacity => 0.6f;

        #endregion

        #region === SOUND OVERRIDES ===

        protected virtual SoundStyle SwingSoundStyle => SoundID.Item71 with { Volume = 0.7f, PitchVariance = 0.4f };
        protected virtual SoundStyle BigSwingSoundStyle => SoundID.Item71 with { Volume = 0.9f, PitchVariance = 0.2f };
        protected virtual SoundStyle DashSoundStyle => SoundID.Item73 with { Volume = 0.6f };
        protected virtual SoundStyle DashHitSoundStyle => SoundID.Item62 with { Volume = 0.85f };

        #endregion

        #region === DASH PARAMETER OVERRIDES ===

        /// <summary>Whether this weapon supports the right-click dash mechanic.</summary>
        protected virtual bool SupportsDash => true;

        /// <summary>Dash animation time in ticks (before extraUpdates).</summary>
        protected virtual int DashTimeFrames => 49;

        /// <summary>Dash lunge movement speed.</summary>
        protected virtual float DashLungeSpeed => 60f;

        /// <summary>Rebound speed after dash-hitting an enemy.</summary>
        protected virtual float DashReboundSpeed => 6f;

        /// <summary>Cooldown frames after dash completes.</summary>
        protected virtual int PostDashCooldownFrames => 180;

        /// <summary>Window of opportunity to perform a powered slash after dash-hit.</summary>
        protected virtual int BigSlashWindowFrames => 111;

        /// <summary>Scale multiplier for the empowered post-dash slash.</summary>
        protected virtual float BigSlashScaleFactor => 1.5f;

        /// <summary>Scale factor applied to weapon texture draw calls to compensate for oversized sprites.
        /// Does not affect collision, dust positions, or VFX calculations—only the texture rendering size.</summary>
        protected virtual float TextureDrawScale => 1.0f;

        /// <summary>What fraction of the dash animation is the actual lunge (0-1).</summary>
        protected virtual float DashPercentLunging => 0.6f;

        #endregion

        #region === BEHAVIOR HOOKS ===

        /// <summary>Called every tick during swing state. Use for spawning particles, 
        /// sub-projectiles, and other per-frame swing effects.</summary>
        protected virtual void OnSwingFrame() { }

        /// <summary>Called every tick during dash state. Use for spawning dash particles.</summary>
        protected virtual void OnDashFrame() { }

        /// <summary>Called when an NPC is struck during a swing. Use for on-hit sub-projectile spawning,
        /// debuffs, and special effects.</summary>
        protected virtual void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int damageDone) { }

        /// <summary>Called when an NPC is struck during a dash lunge.</summary>
        protected virtual void OnDashHitNPC(NPC target, NPC.HitInfo hit, int damageDone) { }

        /// <summary>Called once when a new swing begins (including re-swings from channel).
        /// Use for per-swing initialization like sounds at specific moments.</summary>
        protected virtual void OnSwingStart(bool isFirstSwing) { }

        #endregion

        #region === CORE STATE ===

        public Player Owner => Main.player[Projectile.owner];

        public int GetSwingTime
        {
            get
            {
                if (State == SwingState.BonkDash)
                    return DashTimeFrames * Projectile.extraUpdates;
                else
                    return BaseSwingFrames;
            }
        }

        public float Timer => SwingTime - Projectile.timeLeft;
        public float Progression => SwingTime > 0 ? Timer / SwingTime : 0f;
        public float LungeProgression => Progression < (1 - DashPercentLunging) ? 0 : (Progression - (1 - DashPercentLunging)) / DashPercentLunging;

        public enum SwingState { Swinging, BonkDash }

        public SwingState State
        {
            get => Projectile.ai[0] == 1 ? SwingState.BonkDash : SwingState.Swinging;
            set => Projectile.ai[0] = (int)value;
        }

        public bool PerformingPowerfulSlash => Projectile.ai[0] > 1;

        public bool InPostBonkStasis
        {
            get => Projectile.ai[1] > 0;
            set => Projectile.ai[1] = value ? 1 : 0;
        }

        public ref float SwingTime => ref Projectile.localAI[0];
        public ref float SquishFactor => ref Projectile.localAI[1];
        public float IdealSize => PerformingPowerfulSlash ? BigSlashScaleFactor : 1f;

        #endregion

        #region === ANGLES AND ANIMATION CURVES ===

        public int Direction => Math.Sign(Projectile.velocity.X) <= 0 ? -1 : 1;
        public float BaseRotation => Projectile.velocity.ToRotation();
        public Vector2 SquishVector => new Vector2(1f + (1 - SquishFactor) * 0.6f, SquishFactor);

        public float MaxSwingAngle => MathHelper.PiOver2 * SwingArcMultiplier;

        // Exoblade swing curves — identical for all weapons to ensure consistent feel
        private CurveSegment SlowStart = new(PolyOutEasing, 0f, -1f, 0.3f, 2);
        private CurveSegment SwingFast = new(PolyInEasing, 0.27f, -0.7f, 1.6f, 4);
        private CurveSegment EndSwing = new(PolyOutEasing, 0.85f, 0.9f, 0.1f, 2);

        public float SwingAngleShiftAtProgress(float progress) =>
            State == SwingState.BonkDash ? 0 : MaxSwingAngle * PiecewiseAnimation(progress, new CurveSegment[] { SlowStart, SwingFast, EndSwing });

        public float SwordRotationAtProgress(float progress) =>
            State == SwingState.BonkDash ? BaseRotation : BaseRotation + SwingAngleShiftAtProgress(progress) * Direction;

        public float SquishAtProgress(float progress) =>
            State == SwingState.BonkDash ? 1 : MathHelper.Lerp(SquishVector.X, SquishVector.Y, (float)Math.Abs(Math.Sin(SwingAngleShiftAtProgress(progress))));

        public Vector2 DirectionAtProgress(float progress) =>
            State == SwingState.BonkDash ? Projectile.velocity : SwordRotationAtProgress(progress).ToRotationVector2() * SquishAtProgress(progress);

        public float SwingAngleShift => SwingAngleShiftAtProgress(Progression);
        public float SwordRotation => SwordRotationAtProgress(Progression);
        public float CurrentSquish => SquishAtProgress(Progression);
        public Vector2 SwordDirection => DirectionAtProgress(Progression);

        #endregion

        #region === TRAIL PRIMITIVES ===

        public float TrailEndProgression
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

        public float RealProgressionAtTrailCompletion(float completion) => MathHelper.Lerp(Progression, TrailEndProgression, completion);

        public Vector2 DirectionAtProgressScuffed(float progress)
        {
            float angleShift = SwingAngleShiftAtProgress(progress);
            Vector2 anglePoint = angleShift.ToRotationVector2();
            anglePoint.X *= SquishVector.X;
            anglePoint.Y *= SquishVector.Y;
            angleShift = anglePoint.ToRotation();
            return (BaseRotation + angleShift * Direction).ToRotationVector2() * SquishAtProgress(progress);
        }

        #endregion

        #region === DASH DISPLACEMENT ===

        private CurveSegment GoBack = new(SineBumpEasing, 0f, -10f, -14f);
        private CurveSegment AndThrust => new(PolyOutEasing, 1 - DashPercentLunging, -10, 12f, 5);
        public float DashDisplace => PiecewiseAnimation(Progression, new CurveSegment[] { GoBack, AndThrust });

        #endregion

        #region === DUST PROBABILITY ===

        public float RiskOfDust
        {
            get
            {
                if (Progression > 0.85f) return 0;
                if (Progression < 0.4f) return (float)Math.Pow(Progression / 0.3f, 2) * 0.2f;
                if (Progression < 0.5f) return 0.2f + 0.7f * (Progression - 0.4f) / 0.1f;
                return 0.9f;
            }
        }

        #endregion

        #region === STATIC ASSETS ===

        private static Asset<Texture2D> _cachedLensFlare;

        #endregion

        #region === PROJECTILE SETUP ===

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 120;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 98;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 9999;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.MaxUpdates = 3;
            Projectile.localNPCHitCooldown = Projectile.MaxUpdates * 8;
            Projectile.noEnchantmentVisuals = true;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(SwingTime);
            writer.Write(SquishFactor);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            SwingTime = reader.ReadSingle();
            SquishFactor = reader.ReadSingle();
        }

        #endregion

        #region === PROJECTILE BEHAVIOR ===

        public override bool ShouldUpdatePosition() => State == SwingState.BonkDash && !InPostBonkStasis;

        public override bool? CanDamage()
        {
            if (State != SwingState.BonkDash)
                return null;
            if (InPostBonkStasis)
                return false;
            if (Projectile.timeLeft > SwingTime * DashPercentLunging)
                return false;
            return null;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            Vector2 start = Projectile.Center;
            Vector2 end = start + SwordDirection * (BladeLength + 50) * Projectile.scale;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end,
                State == SwingState.BonkDash ? Projectile.scale * 45 : Projectile.scale * 30f, ref _);
        }

        public void InitializationEffects(bool startInitialization)
        {
            Projectile.velocity = Owner.MountedCenter.DirectionTo(Main.MouseWorld);
            SquishFactor = Main.rand.NextFloat(0.67f, 1f);

            if (startInitialization && State != SwingState.BonkDash)
                Projectile.scale = 0.02f;
            else
            {
                Projectile.scale = 1f;
                if (PerformingPowerfulSlash)
                    State = SwingState.Swinging;
            }

            if (PerformingPowerfulSlash)
                SquishFactor = 0.7f;

            SwingTime = GetSwingTime;
            Projectile.timeLeft = (int)SwingTime;
            Projectile.netUpdate = true;

            OnSwingStart(startInitialization);
        }

        public override void AI()
        {
            if (InPostBonkStasis || Projectile.timeLeft == 0)
                return;

            if (Projectile.timeLeft >= 9999 || (Projectile.timeLeft == 1 && Owner.channel && State != SwingState.BonkDash))
                InitializationEffects(Projectile.timeLeft >= 9999);

            switch (State)
            {
                case SwingState.Swinging:
                    DoBehavior_Swinging();
                    break;
                case SwingState.BonkDash:
                    if (SupportsDash)
                        DoBehavior_BonkDash();
                    break;
            }

            // Glue the sword to its owner.
            Projectile.Center = Owner.RotatedRelativePoint(Owner.MountedCenter, true);
            Owner.heldProj = Projectile.whoAmI;
            Owner.SetDummyItemTime(2);
            Owner.ChangeDir(Direction);

            // Arm rotation
            float armRotation = SwordRotation - MathHelper.PiOver2;
            Owner.SetCompositeArmFront(Math.Abs(armRotation) > 0.01f, Player.CompositeArmStretchAmount.Full, armRotation);

            // Freeze the projectile on its last frame for cooldown
            if (Projectile.timeLeft == 1 && State == SwingState.BonkDash && !InPostBonkStasis)
            {
                Projectile.timeLeft = PostDashCooldownFrames;
                InPostBonkStasis = true;
                Owner.fullRotation = 0f;
                Owner.ExoBlade().LungingDown = false;
            }
        }

        public void DoBehavior_Swinging()
        {
            // Play swing sound at ~20% through the swing
            if (Projectile.timeLeft == (int)(SwingTime / 5))
                SoundEngine.PlaySound(PerformingPowerfulSlash ? BigSwingSoundStyle : SwingSoundStyle, Projectile.Center);

            // Dynamic lighting from the blade
            Color lightColor = GetLightColor(Progression);
            Lighting.AddLight(Owner.MountedCenter + SwordDirection * 100,
                lightColor.ToVector3() * 1.6f * (float)Math.Sin(Progression * MathHelper.Pi));

            // Scale up to ideal size
            if (Projectile.scale < IdealSize)
                Projectile.scale = MathHelper.Lerp(Projectile.scale, IdealSize, 0.08f);

            // Shrink near end of slash when not channeling
            if (!Owner.channel && Progression > 0.7f)
                Projectile.scale = (0.5f + 0.5f * (float)Math.Pow(1 - (Progression - 0.7f) / 0.3f, 0.5)) * IdealSize;

            // Themed dust particles along the blade
            if (Main.rand.NextFloat() * 3f < RiskOfDust)
            {
                Color dustColor = GetSwingDustColor();
                Dust sparkDust = Dust.NewDustPerfect(
                    Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * (float)Math.Pow(Main.rand.NextFloat(0.5f, 1f), 0.5f),
                    DustID.RainbowMk2,
                    SwordDirection.RotatedBy(-MathHelper.PiOver2 * Direction) * 2f,
                    0, dustColor);
                sparkDust.noGravity = true;
                sparkDust.scale = 0.35f;
                sparkDust.fadeIn = Main.rand.NextFloat() * 0.8f;
            }

            if (Main.rand.NextFloat() < RiskOfDust * 0.6f)
            {
                Color dustColor = GetSwingDustColor();
                Dust trailDust = Dust.NewDustPerfect(
                    Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * (float)Math.Pow(Main.rand.NextFloat(0.2f, 1f), 0.5f),
                    DustID.RainbowMk2,
                    SwordDirection.RotatedBy(MathHelper.PiOver2 * Direction) * 2.6f,
                    0, dustColor);
                trailDust.scale = 0.3f;
                trailDust.fadeIn = Main.rand.NextFloat() * 1.2f;
                trailDust.noGravity = true;
            }

            // Hook for derived classes
            OnSwingFrame();
        }

        public void DoBehavior_BonkDash()
        {
            Owner.mount?.Dismount(Owner);
            Owner.RemoveAllGrapplingHooks();

            if (LungeProgression == 0)
            {
                if (Projectile.timeLeft == 1 + (int)(SwingTime * DashPercentLunging))
                    SoundEngine.PlaySound(DashSoundStyle, Projectile.Center);

                Projectile.velocity = Owner.MountedCenter.DirectionTo(Main.MouseWorld);
                Projectile.oldPos = new Vector2[Projectile.oldPos.Length];
                for (int i = 0; i < Projectile.oldPos.Length; ++i)
                    Projectile.oldPos[i] = Projectile.position;
            }
            else
            {
                float rotationStrength = MathHelper.PiOver4 * 0.05f * (float)Math.Pow(LungeProgression, 3);
                float currentRotation = Projectile.velocity.ToRotation();
                float idealRotation = Owner.MountedCenter.DirectionTo(Main.MouseWorld).ToRotation();
                Projectile.velocity = currentRotation.AngleTowards(idealRotation, rotationStrength).ToRotationVector2();

                Owner.fallStart = (int)(Owner.position.Y / 16f);

                float velocityPower = (float)Math.Sin(MathHelper.Pi * LungeProgression);
                velocityPower = (float)Math.Pow(Math.Abs(velocityPower), 0.6f);
                Vector2 newVelocity = Projectile.velocity * DashLungeSpeed * (0.24f + 0.76f * velocityPower);
                Owner.velocity = newVelocity;
                Owner.ExoBlade().LungingDown = true;

                // Dash dust
                if (Main.rand.NextBool())
                {
                    Color dustColor = GetSwingDustColor();
                    Dust dashDust = Dust.NewDustPerfect(
                        Owner.MountedCenter + Main.rand.NextVector2Circular(20f, 20f),
                        DustID.RainbowMk2,
                        SwordDirection * -2.6f,
                        0, dustColor);
                    dashDust.scale = 0.3f;
                    dashDust.fadeIn = Main.rand.NextFloat() * 1.2f;
                    dashDust.noGravity = true;
                }
            }

            // Hook for derived classes
            OnDashFrame();

            if (Projectile.timeLeft == 1)
                Owner.velocity *= 0.2f;

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4 * Direction;
        }

        #endregion

        #region === DRAWING ===

        public float SlashWidthFunction(float completionRatio, Vector2 vertexPos) =>
            SquishAtProgress(RealProgressionAtTrailCompletion(completionRatio)) * Projectile.scale * 60.5f;

        public Color SlashColorFunction(float completionRatio, Vector2 vertexPos) =>
            SlashPrimaryColor * Utils.GetLerpValue(0.9f, 0.4f, completionRatio, true) * Projectile.Opacity;

        public float PierceWidthFunction(float completionRatio, Vector2 vertexPos)
        {
            float width = Utils.GetLerpValue(0f, 0.2f, completionRatio, true) * Projectile.scale * 50f;
            width *= (1 - (float)Math.Pow(LungeProgression, 5));
            return width;
        }

        public Color PierceColorFunction(float completionRatio, Vector2 vertexPos) =>
            SlashPrimaryColor * Projectile.Opacity;

        public List<Vector2> GenerateSlashPoints()
        {
            List<Vector2> result = new();
            for (int i = 0; i < 40; i++)
            {
                float progress = MathHelper.Lerp(Progression, TrailEndProgression, i / 40f);
                result.Add(DirectionAtProgressScuffed(progress) * (BladeLength - 6f) * Projectile.scale);
            }
            return result;
        }

        private static bool _swingErrorLogged;

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            if (Projectile.Opacity <= 0f || InPostBonkStasis)
                return false;

            DrawSlash();
            DrawPierceTrail();
            DrawBlade();
            }
            catch (Exception ex)
            {
                if (!_swingErrorLogged)
                {
                    _swingErrorLogged = true;
                    Main.NewText($"[MagnumOpus] Swing render error: {ex.Message}", Color.OrangeRed);
                }
            }
            finally
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            return false;
        }

        public void DrawSlash()
        {
            if (State != SwingState.Swinging || Progression < 0.45f)
                return;

            Main.spriteBatch.EnterShaderRegion();

            var slashShader = GameShaders.Misc["MagnumOpus:ExobladeSlash"];
            slashShader.UseImage1(ModContent.Request<Texture2D>(NoiseTexturePath));
            slashShader.UseColor(SlashPrimaryColor);
            slashShader.UseSecondaryColor(SlashSecondaryColor);
            slashShader.Shader.Parameters["fireColor"].SetValue(SlashAccentColor.ToVector3());
            slashShader.Shader.Parameters["flipped"].SetValue(Direction == 1);

            // Enable gradient LUT mode if a theme gradient path is provided
            if (GradientLUTPath != null)
            {
                slashShader.UseImage2(ModContent.Request<Texture2D>(GradientLUTPath));
                slashShader.Shader.Parameters["uShaderSpecificData"].SetValue(new Vector4(1f, 0f, 0f, 0f));
            }
            else
            {
                slashShader.Shader.Parameters["uShaderSpecificData"].SetValue(Vector4.Zero);
            }

            slashShader.Apply();

            PrimitiveRenderer.RenderTrail(GenerateSlashPoints(),
                new(SlashWidthFunction, SlashColorFunction,
                    (_, _) => Projectile.Center,
                    shader: GameShaders.Misc["MagnumOpus:ExobladeSlash"]), 95);

            Main.spriteBatch.ExitShaderRegion();
        }

        public void DrawPierceTrail()
        {
            if (State != SwingState.BonkDash || !SupportsDash)
                return;

            Main.spriteBatch.EnterShaderRegion();

            float timeWrapped = (Main.GlobalTimeWrappedHourly * 2f) % 1;
            Color mainColor = GetPierceMainColor(timeWrapped);
            Color secondaryColor = GetPierceSecondaryColor(timeWrapped);

            mainColor = Color.Lerp(Color.White, mainColor, 0.4f + 0.6f * (float)Math.Pow(LungeProgression, 0.5f));
            secondaryColor = Color.Lerp(Color.White, secondaryColor, 0.4f + 0.6f * (float)Math.Pow(LungeProgression, 0.5f));

            Vector2 trailOffset = (Projectile.rotation - Direction * MathHelper.PiOver4).ToRotationVector2() * 98f + Projectile.Size * 0.5f;
            GameShaders.Misc["MagnumOpus:ExobladePierce"].UseImage1(ModContent.Request<Texture2D>(StreakTexturePath));
            GameShaders.Misc["MagnumOpus:ExobladePierce"].UseImage2("Images/Extra_189");
            GameShaders.Misc["MagnumOpus:ExobladePierce"].UseColor(mainColor);
            GameShaders.Misc["MagnumOpus:ExobladePierce"].UseSecondaryColor(secondaryColor);
            GameShaders.Misc["MagnumOpus:ExobladePierce"].Apply();

            int numPointsRendered = 30;
            int numPointsProvided = 60;
            var positionsToUse = Projectile.oldPos.Take(numPointsProvided).ToArray();
            PrimitiveRenderer.RenderTrail(positionsToUse,
                new(PierceWidthFunction, PierceColorFunction,
                    (_, _) => trailOffset,
                    shader: GameShaders.Misc["MagnumOpus:ExobladePierce"]), numPointsRendered);

            Main.spriteBatch.ExitShaderRegion();
        }

        public void DrawBlade()
        {
            var texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            SpriteEffects direction = Direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            if (State == SwingState.Swinging)
            {
                // Check if we have a square texture for the SwingSprite shader
                if (SquareTexturePath != null)
                    DrawBladeWithShader(texture, direction);
                else
                    DrawBladeSimple(texture, direction);

                DrawLensFlare();
            }
            else
            {
                DrawBladeDash(texture, direction);
            }
        }

        /// <summary>Draw blade using the SwingSprite UV-rotation shader (requires square texture).</summary>
        private void DrawBladeWithShader(Texture2D texture, SpriteEffects direction)
        {
            Texture2D squareTex = ModContent.Request<Texture2D>(SquareTexturePath).Value;

            Effect swingFX = Filters.Scene["MagnumOpus:ExobladeSwingSprite"].GetShader().Shader;
            swingFX.Parameters["rotation"].SetValue(SwingAngleShift + MathHelper.PiOver4 + (Direction == -1 ? MathHelper.Pi : 0f));
            swingFX.Parameters["pommelToOriginPercent"].SetValue(0.05f);
            swingFX.Parameters["color"].SetValue(Color.White.ToVector4());

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, swingFX, Main.GameViewMatrix.TransformationMatrix);

            Main.EntitySpriteDraw(squareTex, Owner.MountedCenter - Main.screenPosition, null,
                Color.White, BaseRotation, squareTex.Size() / 2f, SquishVector * 3f * Projectile.scale * TextureDrawScale, direction, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>Draw blade using simple rotation math (works with any texture shape).</summary>
        private void DrawBladeSimple(Texture2D texture, SpriteEffects direction)
        {
            float rotation = SwordRotation + MathHelper.PiOver4;
            Vector2 origin = new Vector2(0, texture.Height);

            if (Direction == -1)
            {
                rotation += MathHelper.PiOver2;
                origin.X = texture.Width;
            }

            Vector2 drawPos = Owner.MountedCenter - Main.screenPosition;

            // Main blade
            Main.EntitySpriteDraw(texture, drawPos, null, Color.White, rotation, origin, Projectile.scale * TextureDrawScale, direction, 0);

            // Subtle energy glow outline
            float energyPower = (float)Math.Sin(Progression * MathHelper.Pi) * 0.12f;
            if (energyPower > 0.01f)
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector2 drawOffset = (MathHelper.TwoPi * i / 4f).ToRotationVector2() * energyPower * Projectile.scale * 6f;
                    Main.EntitySpriteDraw(texture, drawPos + drawOffset, null,
                        GetDashEnergyColor(Progression) * 0.2f, rotation, origin, Projectile.scale * TextureDrawScale, direction, 0);
                }
            }
        }

        /// <summary>Draws the lens flare at the blade tip during swings.</summary>
        private void DrawLensFlare()
        {
            if (_cachedLensFlare == null)
                _cachedLensFlare = ModContent.Request<Texture2D>(LensFlareTexturePath);

            Texture2D shineTex = _cachedLensFlare.Value;

            float lensFlareOpacity = (Progression < 0.3f ? 0f : 0.2f + 0.8f * (float)Math.Sin(MathHelper.Pi * (Progression - 0.3f) / 0.7f)) * LensFlareMaxOpacity;
            Color lensFlareColor = GetLensFlareColor(Progression);
            lensFlareColor.A = 0;

            Vector2 tipPos = Owner.MountedCenter + DirectionAtProgressScuffed(Progression) * Projectile.scale * BladeLength - Main.screenPosition;

            Main.EntitySpriteDraw(shineTex, tipPos, null,
                lensFlareColor * lensFlareOpacity, MathHelper.PiOver2, shineTex.Size() / 2f,
                LensFlareScale * Projectile.scale, 0, 0);
        }

        /// <summary>Draw blade during dash state.</summary>
        private void DrawBladeDash(Texture2D texture, SpriteEffects direction)
        {
            float rotation = BaseRotation + MathHelper.PiOver4;
            Vector2 origin = new Vector2(0, texture.Height);
            Vector2 drawPosition = Projectile.Center + Projectile.velocity * Projectile.scale * DashDisplace - Main.screenPosition;

            if (Direction == -1)
            {
                rotation += MathHelper.PiOver2;
                origin.X = texture.Width;
            }

            Projectile.scale = MathHelper.Lerp(1f, 0.22f, MathF.Pow(LungeProgression, 7));

            Main.EntitySpriteDraw(texture, drawPosition, null, Color.White, rotation, origin, Projectile.scale * TextureDrawScale, direction, 0);

            // Energy glow during dash
            float energyPower = Utils.GetLerpValue(0f, 0.32f, Progression, true) * Utils.GetLerpValue(1f, 0.85f, Progression, true);
            for (int i = 0; i < 4; i++)
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / 4f + BaseRotation).ToRotationVector2() * energyPower * Projectile.scale * 7f;
                Main.spriteBatch.Draw(texture, drawPosition + drawOffset, null,
                    GetDashEnergyColor(Progression) * 0.16f, rotation, origin, Projectile.scale * TextureDrawScale, direction, 0);
            }
        }

        #endregion

        #region === COMBAT ===

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            ItemLoader.OnHitNPC(Owner.HeldItem, Owner, target, hit, damageDone);
            NPCLoader.OnHitByItem(target, Owner, Owner.HeldItem, hit, damageDone);
            PlayerLoader.OnHitNPC(Owner, target, hit, damageDone);

            if (State == SwingState.BonkDash && SupportsDash)
            {
                Owner.itemAnimation = 0;
                Owner.velocity = Owner.SafeDirectionTo(target.Center) * -DashReboundSpeed;
                Projectile.timeLeft = BigSlashWindowFrames + PostDashCooldownFrames;
                InPostBonkStasis = true;
                Projectile.netUpdate = true;

                SoundEngine.PlaySound(DashHitSoundStyle, target.Center);

                OnDashHitNPC(target, hit, damageDone);
            }

            if (State == SwingState.Swinging)
            {
                OnSwingHitNPC(target, hit, damageDone);
            }
        }

        #endregion

        #region === CLEANUP ===

        public override void OnKill(int timeLeft)
        {
            Owner.fullRotation = 0f;
            Owner.ExoBlade().LungingDown = false;
        }

        #endregion
    }
}
