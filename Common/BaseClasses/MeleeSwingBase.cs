using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Common.Systems.Particles;
using static MagnumOpus.Common.Systems.Particles.Particle;

namespace MagnumOpus.Common.BaseClasses
{
    /// <summary>
    /// Abstract base for ALL Calamity-style held-projectile melee swings.
    /// Extracted from InfernalCleaverSwing's 614-line architecture.
    ///
    /// Shared infrastructure:
    ///   ComboPhase struct, PiecewiseAnimation, 60-point trail buffer,
    ///   Progression/SwingTime/ComboStep, InitializeSwing/DoBehavior,
    ///   sealed PreDraw pipeline (Trail → Smear → Blade → Flare),
    ///   networking (SendExtraAI/ReceiveExtraAI).
    ///
    /// Subclasses MUST define:
    ///   GetAllPhases(), GetPalette(), GetTrailStyle(), GetSmearTexturePath()
    ///
    /// Subclasses MAY override:
    ///   HandleComboSpecials(), DrawCustomVFX(), OnSwingHitNPC(),
    ///   GetInitialDustType(), GetSwingSound(), GetBladeTexture()
    /// </summary>
    public abstract class MeleeSwingBase : ModProjectile
    {
        // Swing projectiles are fully drawn via custom PreDraw rendering — no PNG texture needed.
        // This prevents MissingResourceException for subclass names that have no matching PNG.
        public override string Texture => "Terraria/Images/Projectile_0";

        #region ComboPhase Definition

        /// <summary>
        /// Defines a single combo phase (one "movement" in the weapon's symphony).
        /// </summary>
        public struct ComboPhase
        {
            public CurveSegment[] AnimCurves;
            public float MaxAngle;
            public int Duration;
            public float BladeLength;
            public bool FlipDirection;
            public float SquishRange;
            public float DamageMultiplier;

            public ComboPhase(CurveSegment[] curves, float maxAngle, int duration,
                float bladeLength, bool flip, float squish, float damageMult)
            {
                AnimCurves = curves;
                MaxAngle = maxAngle;
                Duration = duration;
                BladeLength = bladeLength;
                FlipDirection = flip;
                SquishRange = squish;
                DamageMultiplier = damageMult;
            }
        }

        #endregion

        #region Abstract Members — MUST override

        /// <summary>All combo phases for this weapon. Minimum 3, standard 4.</summary>
        protected abstract ComboPhase[] GetAllPhases();

        /// <summary>6-color palette from dark (index 0) to white-hot (index 5).</summary>
        protected abstract Color[] GetPalette();

        /// <summary>Trail shader style for CalamityStyleTrailRenderer.</summary>
        protected abstract CalamityStyleTrailRenderer.TrailStyle GetTrailStyle();

        /// <summary>
        /// Smear texture path per combo step.
        /// Return null to skip smear overlay for that step.
        /// </summary>
        protected abstract string GetSmearTexturePath(int comboStep);

        #endregion

        #region Virtual Members — MAY override

        /// <summary>Called at key moments during each combo phase for sub-projectile spawns, etc.</summary>
        protected virtual void HandleComboSpecials() { }

        /// <summary>Extra VFX drawn after the lens flare (theme particles, music notes, etc.).</summary>
        protected virtual void DrawCustomVFX(SpriteBatch sb) { }

        /// <summary>Called when this swing hits an NPC. Base implementation does spark + dust burst.</summary>
        protected virtual void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int remainingDamageCount) { }

        /// <summary>Vanilla dust type for the initial burst and swing trail. Default: DustID.Torch.</summary>
        protected virtual int GetInitialDustType() => DustID.Torch;

        /// <summary>Sound played when the swing starts. Default: Item71 with pitch scaling.</summary>
        protected virtual SoundStyle GetSwingSound() => SoundID.Item71 with { Pitch = -0.3f + ComboStep * 0.15f };

        /// <summary>
        /// Blade texture to draw. Default: BreakerBlade.
        /// Override to return your own weapon sprite.
        /// </summary>
        protected virtual Texture2D GetBladeTexture() => Terraria.GameContent.TextureAssets.Item[ItemID.BreakerBlade].Value;

        /// <summary>Light color per frame (RGB vector). Override for non-fire themes.</summary>
        protected virtual Vector3 GetLightColor()
        {
            float intensity = 0.3f + ComboStep * 0.15f;
            Color c = GetElementColor(0.5f);
            return c.ToVector3() * intensity;
        }

        /// <summary>Secondary dust type for contrast sparkles. -1 = none.</summary>
        protected virtual int GetSecondaryDustType() => -1;

        #endregion

        #region Constants

        protected const int TrailLength = 60;

        #endregion

        #region Cached Data

        private ComboPhase[] _phases;
        private Color[] _palette;

        private ComboPhase[] Phases => _phases ??= GetAllPhases();
        private Color[] Palette => _palette ??= GetPalette();

        // H-1 fix: Pre-allocated trail buffers to avoid per-frame GC pressure
        private Vector2[] _trailPosBuffer = new Vector2[TrailLength];
        private float[] _trailRotBuffer = new float[TrailLength];
        private int _lastTrailCount;

        // H-2/H-3 fix: Cached textures to avoid per-frame ModContent.Request lookups
        private Texture2D _cachedBladeTex;
        private Texture2D _cachedSmearTex;
        private int _cachedSmearComboStep = -1;

        #endregion

        #region Properties

        protected Player Owner => Main.player[Projectile.owner];

        /// <summary>Current combo step (0-based). Stored in ai[0].</summary>
        public int ComboStep => (int)Projectile.ai[0];

        protected ComboPhase CurrentPhase => Phases[Math.Clamp(ComboStep, 0, Phases.Length - 1)];

        public int SwingTime { get => (int)Projectile.localAI[0]; set => Projectile.localAI[0] = value; }
        public float SquishFactor { get => Projectile.localAI[1]; set => Projectile.localAI[1] = value; }

        protected float Timer => SwingTime - Projectile.timeLeft;
        protected float Progression => SwingTime > 0 ? Timer / SwingTime : 0f;

        /// <summary>
        /// When true, the swing projectile lingers waiting for the next input.
        /// Stored in ai[1].
        /// </summary>
        public bool InPostSwingStasis { get => Projectile.ai[1] > 0; set => Projectile.ai[1] = value ? 1 : 0; }

        protected int Direction
        {
            get
            {
                int baseDir = Math.Sign(Projectile.velocity.X) <= 0 ? -1 : 1;
                return CurrentPhase.FlipDirection ? -baseDir : baseDir;
            }
        }

        protected float BaseRotation => Projectile.velocity.ToRotation();

        #endregion

        #region Trail Tracking

        private Vector2[] tipPositions = new Vector2[TrailLength];
        private float[] tipRotations = new float[TrailLength];
        private int trailIndex = 0;
        protected bool hasSpawnedSpecial = false;

        #endregion

        #region Piecewise Animation (shared)

        protected float SwingAngleShiftAtProgress(float progress)
        {
            return CurrentPhase.MaxAngle * PiecewiseAnimation(progress, CurrentPhase.AnimCurves);
        }

        protected float SwordRotationAtProgress(float progress)
        {
            return BaseRotation + SwingAngleShiftAtProgress(progress) * Direction;
        }

        protected float SquishAtProgress(float progress)
        {
            float angleShift = Math.Abs(SwingAngleShiftAtProgress(progress));
            float squishRange = CurrentPhase.SquishRange;
            return MathHelper.Lerp(1f + (1 - squishRange) * 0.6f, squishRange,
                (float)Math.Abs(Math.Sin(angleShift)));
        }

        protected Vector2 DirectionAtProgress(float progress)
        {
            return SwordRotationAtProgress(progress).ToRotationVector2() * SquishAtProgress(progress);
        }

        protected float SwordRotation => SwordRotationAtProgress(Progression);
        protected Vector2 SwordDirection => DirectionAtProgress(Progression);

        #endregion

        #region Palette Lookup

        /// <summary>Get interpolated color from this weapon's palette (t = 0 dark → 1 bright).</summary>
        public Color GetElementColor(float t)
        {
            Color[] pal = Palette;
            t = MathHelper.Clamp(t, 0f, 1f);
            float scaled = t * (pal.Length - 1);
            int idx = (int)scaled;
            int next = Math.Min(idx + 1, pal.Length - 1);
            return Color.Lerp(pal[idx], pal[next], scaled - idx);
        }

        #endregion

        #region Setup

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = TrailLength;
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
            Projectile.MaxUpdates = 3;
            Projectile.localNPCHitCooldown = Projectile.MaxUpdates * 8;
            Projectile.noEnchantmentVisuals = true;
        }

        #endregion

        #region Collision

        public override bool ShouldUpdatePosition() => false;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            Vector2 start = Owner.MountedCenter;
            Vector2 end = start + SwordDirection * CurrentPhase.BladeLength;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 40f, ref _);
        }

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            Vector2 tipPos = Owner.MountedCenter + SwordDirection * CurrentPhase.BladeLength * 0.5f;
            int size = (int)(CurrentPhase.BladeLength * 0.8f);
            hitbox = new Rectangle((int)tipPos.X - size / 2, (int)tipPos.Y - size / 2, size, size);
        }

        #endregion

        #region AI

        public override void AI()
        {
            if (Timer == 0)
                InitializeSwing();

            DoBehavior_Swinging();

            // Track trail positions at blade tip
            Vector2 tipWorld = Owner.MountedCenter + SwordDirection * CurrentPhase.BladeLength;
            tipPositions[trailIndex % TrailLength] = tipWorld;
            tipRotations[trailIndex % TrailLength] = SwordRotation;
            trailIndex++;

            // Subclass combo specials
            HandleComboSpecials();
        }

        protected virtual void InitializeSwing()
        {
            ComboPhase phase = CurrentPhase;
            int totalTime = (int)(phase.Duration / Owner.GetAttackSpeed(DamageClass.MeleeNoSpeed));
            SwingTime = Math.Max(totalTime * Projectile.MaxUpdates, 12);
            Projectile.timeLeft = SwingTime;
            SquishFactor = phase.SquishRange;

            Projectile.damage = (int)(Projectile.damage * phase.DamageMultiplier);

            // Initial dust burst — scales with combo step
            int dustType = GetInitialDustType();
            for (int i = 0; i < 6 + ComboStep * 2; i++)
            {
                Vector2 dustVel = (Projectile.velocity.ToRotation() + Main.rand.NextFloat(-0.5f, 0.5f))
                    .ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Dust d = Dust.NewDustPerfect(Owner.MountedCenter, dustType, dustVel, 0,
                    GetElementColor(Main.rand.NextFloat()), 1.2f + ComboStep * 0.3f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            SoundEngine.PlaySound(GetSwingSound(), Owner.Center);
        }

        protected virtual void DoBehavior_Swinging()
        {
            Projectile.Center = Owner.MountedCenter;
            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, SwordRotation - MathHelper.PiOver2);

            // Dynamic lighting
            Vector2 tipPos = Owner.MountedCenter + SwordDirection * CurrentPhase.BladeLength;
            Vector3 light = GetLightColor();
            Lighting.AddLight(tipPos, light.X, light.Y, light.Z);

            // Swing dust trail
            float swingSpeed = Math.Abs(SwingAngleShiftAtProgress(Progression) -
                SwingAngleShiftAtProgress(Math.Max(0, Progression - 0.02f)));

            if (swingSpeed > 0.01f && Main.rand.NextBool(Math.Max(1, 3 - ComboStep)))
            {
                float bladeProgress = Main.rand.NextFloat(0.4f, 1f);
                Vector2 dustPos = Owner.MountedCenter + SwordDirection * CurrentPhase.BladeLength * bladeProgress;
                Vector2 dustVel = SwordDirection.RotatedBy(MathHelper.PiOver2 * Direction) * Main.rand.NextFloat(1f, 4f);

                int dustType = GetInitialDustType();
                Dust d = Dust.NewDustPerfect(dustPos, dustType, dustVel, 0,
                    GetElementColor(bladeProgress), 1.5f + ComboStep * 0.2f);
                d.noGravity = true;
                d.fadeIn = 1.3f;

                int secondaryDust = GetSecondaryDustType();
                if (secondaryDust >= 0 && Main.rand.NextBool(2))
                {
                    Dust d2 = Dust.NewDustPerfect(dustPos, secondaryDust, dustVel * 0.5f, 0, default, 0.8f);
                    d2.noGravity = true;
                }
            }

            // Post-swing stasis
            if (Projectile.timeLeft <= 2)
            {
                if (Owner.channel && Owner.HeldItem.shoot == Projectile.type)
                {
                    InPostSwingStasis = true;
                    Projectile.timeLeft = 5;
                }
                else
                {
                    Projectile.Kill();
                }
            }

            if (InPostSwingStasis)
            {
                Projectile.timeLeft = 5;
                if (!Owner.channel)
                    Projectile.Kill();
            }
        }

        #endregion

        #region Hit Effects

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int remainingDamageCount)
        {
            // Base VFX: spark burst that scales with combo step
            int sparkCount = 4 + ComboStep * 3;
            for (int i = 0; i < sparkCount; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(6f, 6f);
                Color sparkColor = GetElementColor(Main.rand.NextFloat());
                var spark = new GlowSparkParticle(target.Center + Main.rand.NextVector2Circular(10f, 10f),
                    sparkVel, sparkColor, Main.rand.NextFloat(0.4f, 0.7f), Main.rand.Next(15, 25));
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Dust burst on hit
            int dustType = GetInitialDustType();
            for (int i = 0; i < 8 + ComboStep * 4; i++)
            {
                Dust d = Dust.NewDustPerfect(target.Center, dustType,
                    Main.rand.NextVector2Circular(8f, 8f), 0,
                    GetElementColor(Main.rand.NextFloat()), 1.4f + ComboStep * 0.2f);
                d.noGravity = true;
            }

            // Finisher halo on last combo step
            if (ComboStep == Phases.Length - 1)
            {
                var ring = new BloomRingParticle(target.Center, Vector2.Zero,
                    GetElementColor(0.7f), 0.6f, 20);
                MagnumParticleHandler.SpawnParticle(ring);
            }

            // Subclass-specific hit logic
            OnSwingHitNPC(target, hit, remainingDamageCount);
        }

        #endregion

        #region Drawing — Sealed Pipeline

        public sealed override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;

            // 1. TRAIL — uses pre-allocated buffers (H-1 fix)
            int trailCount = BuildTrailPositions();
            BuildTrailRotations();

            if (trailCount > 1)
            {
                // Slice buffers to active count for trail renderer
                var posSpan = _trailPosBuffer.AsSpan(0, trailCount).ToArray();
                var rotSpan = _trailRotBuffer.AsSpan(0, trailCount).ToArray();

                float trailWidth = 18f + ComboStep * 6f;
                float trailIntensity = 0.6f + ComboStep * 0.12f;
                Color trailPrimary = GetElementColor(0.3f + ComboStep * 0.15f);
                Color trailSecondary = GetElementColor(0.7f + ComboStep * 0.08f);

                CalamityStyleTrailRenderer.DrawTrailWithBloom(
                    posSpan, rotSpan, GetTrailStyle(),
                    trailWidth, trailPrimary, trailSecondary, trailIntensity, 2.2f + ComboStep * 0.3f);
            }

            // 2. SMEAR (inner draw, no state change)
            DrawSmearOverlayInner(sb);

            // 3. BLADE — normal pass
            Texture2D bladeTex = _cachedBladeTex ??= GetBladeTexture();
            Vector2 bladeOrigin = new Vector2(0, bladeTex.Height);
            float bladeRotation = SwordRotation + (Direction == -1 ? MathHelper.Pi : 0);
            SpriteEffects bladeEffects = Direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            float bladeScale = CurrentPhase.BladeLength / bladeTex.Height;
            Vector2 bladeDrawPos = Owner.MountedCenter - Main.screenPosition;

            sb.Draw(bladeTex, bladeDrawPos, null, lightColor, bladeRotation, bladeOrigin, bladeScale, bladeEffects, 0f);

            // --- SINGLE ADDITIVE BATCH for glow, lens flare, and smear (M-1 fix) ---
            SwingShaderSystem.BeginAdditive(sb);

            // Blade glow (2 layers)
            float glowIntensity = 0.3f + ComboStep * 0.1f;
            Color glowColor = GetElementColor(0.4f + Progression * 0.5f) * glowIntensity;
            glowColor.A = 0;
            sb.Draw(bladeTex, bladeDrawPos, null, glowColor, bladeRotation, bladeOrigin, bladeScale * 1.05f, bladeEffects, 0f);
            sb.Draw(bladeTex, bladeDrawPos, null, glowColor * 0.5f, bladeRotation, bladeOrigin, bladeScale * 1.12f, bladeEffects, 0f);

            // Lens flare (3 layers)
            DrawLensFlareInner(sb);

            SwingShaderSystem.RestoreSpriteBatch(sb);

            // 5. MOTION BLUR BLOOM (shader-based directional blur on blade)
            if (Progression > 0.08f && Progression < 0.95f)
            {
                float swingSpeed = Progression < 0.5f ? Progression * 2f : 1f;
                float blurStrength = (0.04f + ComboStep * 0.015f) * swingSpeed;
                float intensityMult = 0.7f + ComboStep * 0.1f;
                MotionBlurBloomRenderer.DrawMeleeSwing(sb, bladeTex, bladeDrawPos, SwordDirection,
                    GetElementColor(0.5f), GetElementColor(0.8f),
                    bladeScale, bladeRotation, blurStrength, intensityMult,
                    origin: bladeOrigin);
            }

            // 6. CUSTOM (subclass theme VFX)
            DrawCustomVFX(sb);

            return false;
        }

        /// <summary>Draws smear overlay. Called BEFORE the additive batch — manages its own batch internally since it's drawn behind the blade.</summary>
        private void DrawSmearOverlayInner(SpriteBatch sb)
        {
            if (Progression < 0.10f || Progression > 0.92f) return;

            // H-2 fix: Cache smear texture, invalidate on combo step change
            if (_cachedSmearComboStep != ComboStep)
            {
                string smearPath = GetSmearTexturePath(ComboStep);
                _cachedSmearTex = string.IsNullOrEmpty(smearPath) ? null : ModContent.Request<Texture2D>(smearPath).Value;
                _cachedSmearComboStep = ComboStep;
            }
            if (_cachedSmearTex == null) return;

            Texture2D smearTex = _cachedSmearTex;
            Vector2 smearOrigin = smearTex.Size() * 0.5f;
            Vector2 drawPos = Owner.MountedCenter - Main.screenPosition;

            float maxDim = Math.Max(smearTex.Width, smearTex.Height);
            float baseScale = (CurrentPhase.BladeLength * 2.2f) / maxDim;
            float midSwingAngle = SwordRotation;

            float swingWindow = MathHelper.Clamp((Progression - 0.10f) / 0.15f, 0f, 1f);
            float fadeOut = MathHelper.Clamp((0.92f - Progression) / 0.15f, 0f, 1f);
            float smearAlpha = swingWindow * fadeOut;
            float intensityMult = 0.55f + ComboStep * 0.12f;

            SpriteEffects smearFlip = Direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            SwingShaderSystem.BeginAdditive(sb);

            // Layer 1: Outer glow
            Color outerColor = GetElementColor(0.25f + Progression * 0.4f);
            outerColor.A = 0;
            sb.Draw(smearTex, drawPos, null, outerColor * smearAlpha * intensityMult * 0.35f,
                midSwingAngle, smearOrigin, baseScale * 1.1f, smearFlip, 0f);

            // Layer 2: Main
            Color mainColor = GetElementColor(0.45f + Progression * 0.3f);
            mainColor.A = 0;
            sb.Draw(smearTex, drawPos, null, mainColor * smearAlpha * intensityMult * 0.65f,
                midSwingAngle, smearOrigin, baseScale, smearFlip, 0f);

            // Layer 3: Core
            Color coreColor = GetElementColor(0.75f + Progression * 0.15f);
            coreColor.A = 0;
            sb.Draw(smearTex, drawPos, null, coreColor * smearAlpha * intensityMult * 0.45f,
                midSwingAngle, smearOrigin, baseScale * 0.9f, smearFlip, 0f);

            SwingShaderSystem.RestoreSpriteBatch(sb);
        }

        /// <summary>Draws lens flare at blade tip. Called INSIDE the additive batch — no state changes needed (M-1 fix).</summary>
        private void DrawLensFlareInner(SpriteBatch sb)
        {
            Texture2D flareTex = Terraria.GameContent.TextureAssets.Extra[98].Value;
            Vector2 tipWorld = Owner.MountedCenter + SwordDirection * CurrentPhase.BladeLength;
            Vector2 tipScreen = tipWorld - Main.screenPosition;
            Vector2 flareOrigin = flareTex.Size() * 0.5f;

            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.15f;
            float baseScale = (0.25f + ComboStep * 0.08f) * pulse;

            Color flareColor = GetElementColor(0.6f + Progression * 0.3f);
            flareColor.A = 0;

            sb.Draw(flareTex, tipScreen, null, flareColor * 0.7f, 0f, flareOrigin, baseScale, SpriteEffects.None, 0f);
            sb.Draw(flareTex, tipScreen, null, flareColor * 0.4f, MathHelper.PiOver4, flareOrigin, baseScale * 0.7f, SpriteEffects.None, 0f);
            sb.Draw(flareTex, tipScreen, null, Color.White * 0.3f, 0f, flareOrigin, baseScale * 0.35f, SpriteEffects.None, 0f);
        }

        #endregion

        #region Trail Helpers

        /// <summary>Fills pre-allocated buffer with ordered trail positions (most recent first). Returns count.</summary>
        protected int BuildTrailPositions()
        {
            int count = Math.Min(trailIndex, TrailLength);
            _lastTrailCount = count;
            for (int i = 0; i < count; i++)
            {
                int idx = (trailIndex - 1 - i + TrailLength) % TrailLength;
                _trailPosBuffer[i] = tipPositions[idx];
            }
            return count;
        }

        /// <summary>Fills pre-allocated buffer with ordered trail rotations (most recent first). Returns count.</summary>
        protected int BuildTrailRotations()
        {
            int count = Math.Min(trailIndex, TrailLength);
            for (int i = 0; i < count; i++)
            {
                int idx = (trailIndex - 1 - i + TrailLength) % TrailLength;
                _trailRotBuffer[i] = tipRotations[idx];
            }
            return count;
        }

        /// <summary>Blade tip position in world coordinates.</summary>
        protected Vector2 GetBladeTipPosition()
        {
            return Owner.MountedCenter + SwordDirection * CurrentPhase.BladeLength;
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
