using System;
using System.Collections.Generic;
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

namespace MagnumOpus.Content.TestWeapons._01_InfernalCleaver
{
    /// <summary>
    /// 🔥 Infernal Cleaver Swing — 4-step combo projectile.
    /// Step 0: Slow Overhead (top→down, wide arc, heavy)
    /// Step 1: Fast Horizontal (left→right, quick, tight)
    /// Step 2: Rising Uppercut (bottom→top, launches EmberShards)
    /// Step 3: Massive Slam (overhead→down, erupts MagmaPillars, screen shake)
    /// </summary>
    public class InfernalCleaverSwing : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.BreakerBlade;

        #region Combo Phase Definition
        private struct ComboPhase
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

        #region Combo Phases
        // Step 0: Slow Overhead — deliberate, heavy downward cleave
        private static readonly ComboPhase Phase0_SlowOverhead = new ComboPhase(
            new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -1f, 0.25f, 2),
                new CurveSegment(EasingType.PolyIn, 0.25f, -0.75f, 1.65f, 3),
                new CurveSegment(EasingType.PolyOut, 0.85f, 0.9f, 0.1f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 2.0f,
            duration: 78,
            bladeLength: 170f,
            flip: false,
            squish: 0.85f,
            damageMult: 1.0f
        );

        // Step 1: Fast Horizontal — quick snappy horizontal slash
        private static readonly ComboPhase Phase1_FastHorizontal = new ComboPhase(
            new CurveSegment[]
            {
                new CurveSegment(EasingType.SineOut, 0f, -0.8f, 0.15f, 1),
                new CurveSegment(EasingType.ExpIn, 0.12f, -0.65f, 1.55f, 1),
                new CurveSegment(EasingType.PolyOut, 0.65f, 0.9f, 0.1f, 3)
            },
            maxAngle: MathHelper.PiOver2 * 1.5f,
            duration: 42,
            bladeLength: 150f,
            flip: true,
            squish: 0.9f,
            damageMult: 0.8f
        );

        // Step 2: Rising Uppercut — sweeps upward, spawns ember shards at apex
        private static readonly ComboPhase Phase2_RisingUppercut = new ComboPhase(
            new CurveSegment[]
            {
                new CurveSegment(EasingType.CircOut, 0f, 0.9f, -0.3f, 1),
                new CurveSegment(EasingType.PolyIn, 0.2f, 0.6f, -1.6f, 4),
                new CurveSegment(EasingType.SineOut, 0.8f, -1f, 0.08f, 1)
            },
            maxAngle: MathHelper.PiOver2 * 1.8f,
            duration: 54,
            bladeLength: 155f,
            flip: false,
            squish: 0.88f,
            damageMult: 1.1f
        );

        // Step 3: Massive Slam — biggest arc, slowest windup, most damage, erupts magma
        private static readonly ComboPhase Phase3_MassiveSlam = new ComboPhase(
            new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -1f, 0.15f, 2),
                new CurveSegment(EasingType.ExpIn, 0.3f, -0.85f, 1.95f, 1),
                new CurveSegment(EasingType.PolyOut, 0.8f, 1.1f, -0.1f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 2.4f,
            duration: 90,
            bladeLength: 190f,
            flip: true,
            squish: 0.8f,
            damageMult: 1.5f
        );

        private static readonly ComboPhase[] AllPhases = { Phase0_SlowOverhead, Phase1_FastHorizontal, Phase2_RisingUppercut, Phase3_MassiveSlam };
        #endregion

        #region Constants
        private const int TrailLength = 60;

        // Fire color palette
        private static readonly Color[] FirePalette = new Color[]
        {
            new Color(80, 10, 0),
            new Color(200, 50, 10),
            new Color(255, 120, 20),
            new Color(255, 180, 40),
            new Color(255, 220, 100),
            new Color(255, 250, 220)
        };
        #endregion

        #region Properties
        private Player Owner => Main.player[Projectile.owner];
        private int ComboStep => (int)Projectile.ai[0];
        private ComboPhase CurrentPhase => AllPhases[Math.Clamp(ComboStep, 0, AllPhases.Length - 1)];

        public int SwingTime { get => (int)Projectile.localAI[0]; set => Projectile.localAI[0] = value; }
        public float SquishFactor { get => Projectile.localAI[1]; set => Projectile.localAI[1] = value; }

        private float Timer => SwingTime - Projectile.timeLeft;
        private float Progression => SwingTime > 0 ? Timer / SwingTime : 0f;

        public bool InPostSwingStasis { get => Projectile.ai[1] > 0; set => Projectile.ai[1] = value ? 1 : 0; }

        private int Direction
        {
            get
            {
                int baseDir = Math.Sign(Projectile.velocity.X) <= 0 ? -1 : 1;
                return CurrentPhase.FlipDirection ? -baseDir : baseDir;
            }
        }

        private float BaseRotation => Projectile.velocity.ToRotation();
        #endregion

        #region Trail Tracking
        private Vector2[] tipPositions = new Vector2[TrailLength];
        private float[] tipRotations = new float[TrailLength];
        private int trailIndex = 0;
        private bool hasSpawnedSpecial = false;
        #endregion

        #region Piecewise Animation
        private float SwingAngleShiftAtProgress(float progress)
        {
            return CurrentPhase.MaxAngle * PiecewiseAnimation(progress, CurrentPhase.AnimCurves);
        }

        private float SwordRotationAtProgress(float progress)
        {
            return BaseRotation + SwingAngleShiftAtProgress(progress) * Direction;
        }

        private float SquishAtProgress(float progress)
        {
            float angleShift = Math.Abs(SwingAngleShiftAtProgress(progress));
            float squishRange = CurrentPhase.SquishRange;
            return MathHelper.Lerp(1f + (1 - squishRange) * 0.6f, squishRange,
                (float)Math.Abs(Math.Sin(angleShift)));
        }

        private Vector2 DirectionAtProgress(float progress)
        {
            return SwordRotationAtProgress(progress).ToRotationVector2() * SquishAtProgress(progress);
        }

        private float SwordRotation => SwordRotationAtProgress(Progression);
        private Vector2 SwordDirection => DirectionAtProgress(Progression);
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

            // Combo-specific special effects at key moments
            HandleComboSpecials();
        }

        private void InitializeSwing()
        {
            ComboPhase phase = CurrentPhase;
            int totalTime = (int)(phase.Duration / Owner.GetAttackSpeed(DamageClass.MeleeNoSpeed));
            SwingTime = Math.Max(totalTime * Projectile.MaxUpdates, 12);
            Projectile.timeLeft = SwingTime;
            SquishFactor = phase.SquishRange;

            Projectile.damage = (int)(Projectile.damage * phase.DamageMultiplier);

            // Spawn initial VFX burst - scale with combo step
            float burstScale = 0.4f + ComboStep * 0.15f;
            for (int i = 0; i < 6 + ComboStep * 2; i++)
            {
                Vector2 dustVel = (Projectile.velocity.ToRotation() + Main.rand.NextFloat(-0.5f, 0.5f))
                    .ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Dust d = Dust.NewDustPerfect(Owner.MountedCenter, DustID.Torch, dustVel, 0,
                    GetFireColor(Main.rand.NextFloat()), 1.2f + ComboStep * 0.3f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            SoundEngine.PlaySound(SoundID.Item71 with { Pitch = -0.3f + ComboStep * 0.15f }, Owner.Center);
        }

        private void DoBehavior_Swinging()
        {
            Projectile.Center = Owner.MountedCenter;
            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, SwordRotation - MathHelper.PiOver2);

            // Dynamic lighting that intensifies through combo
            float lightIntensity = 0.3f + ComboStep * 0.15f;
            Vector2 tipPos = Owner.MountedCenter + SwordDirection * CurrentPhase.BladeLength;
            Lighting.AddLight(tipPos, 1f * lightIntensity, 0.5f * lightIntensity, 0.1f * lightIntensity);

            // Ember dust trail — denser each combo step
            float swingSpeed = Math.Abs(SwingAngleShiftAtProgress(Progression) -
                SwingAngleShiftAtProgress(Math.Max(0, Progression - 0.02f)));

            if (swingSpeed > 0.01f && Main.rand.NextBool(Math.Max(1, 3 - ComboStep)))
            {
                float bladeProgress = Main.rand.NextFloat(0.4f, 1f);
                Vector2 dustPos = Owner.MountedCenter + SwordDirection * CurrentPhase.BladeLength * bladeProgress;
                Vector2 dustVel = SwordDirection.RotatedBy(MathHelper.PiOver2 * Direction) * Main.rand.NextFloat(1f, 4f);

                Dust d = Dust.NewDustPerfect(dustPos, DustID.Torch, dustVel, 0,
                    GetFireColor(bladeProgress), 1.5f + ComboStep * 0.2f);
                d.noGravity = true;
                d.fadeIn = 1.3f;

                if (Main.rand.NextBool(2))
                {
                    Dust d2 = Dust.NewDustPerfect(dustPos, DustID.Enchanted_Gold, dustVel * 0.5f, 0,
                        default, 0.8f);
                    d2.noGravity = true;
                }
            }

            // Post-swing stasis check
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

        private void HandleComboSpecials()
        {
            if (hasSpawnedSpecial) return;

            // Step 2 (Uppercut): spawn ember shards at apex (~70% through swing)
            if (ComboStep == 2 && Progression >= 0.7f)
            {
                hasSpawnedSpecial = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tipPos = Owner.MountedCenter + SwordDirection * CurrentPhase.BladeLength;
                    for (int i = 0; i < 5; i++)
                    {
                        float angle = MathHelper.ToRadians(-45 + i * 22);
                        Vector2 vel = (SwordRotation + angle).ToRotationVector2() * Main.rand.NextFloat(8f, 14f);
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), tipPos, vel,
                            ModContent.ProjectileType<EmberShardProjectile>(),
                            Projectile.damage / 3, 2f, Projectile.owner);
                    }
                }
                SoundEngine.PlaySound(SoundID.Item45 with { Pitch = 0.3f }, Owner.Center);
            }

            // Step 3 (Slam): erupt magma pillars at ~85% progress (impact moment)
            if (ComboStep == 3 && Progression >= 0.85f)
            {
                hasSpawnedSpecial = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 slamPoint = Owner.MountedCenter + SwordDirection * CurrentPhase.BladeLength;
                    for (int i = -2; i <= 2; i++)
                    {
                        Vector2 pillarPos = slamPoint + new Vector2(i * 60, 0);
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), pillarPos, Vector2.UnitY * -2f,
                            ModContent.ProjectileType<MagmaPillarProjectile>(),
                            Projectile.damage / 2, 4f, Projectile.owner);
                    }
                }
                SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.5f, Volume = 1.5f }, Owner.Center);

                // Screen shake on slam
                for (int k = 0; k < Main.player.Length; k++)
                {
                    Player p = Main.player[k];
                    if (p.active && !p.dead && p.Distance(Owner.Center) < 800f)
                        p.GetModPlayer<global::MagnumOpus.Content.LaCampanella.Debuffs.ScreenShakePlayer>().AddShake(12f, 20);
                }
            }
        }
        #endregion

        #region Hit Effects
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int remainingDamageCount)
        {
            // Impact VFX scales with combo step
            int sparkCount = 4 + ComboStep * 3;
            for (int i = 0; i < sparkCount; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(6f, 6f);
                Color sparkColor = GetFireColor(Main.rand.NextFloat());
                var spark = new GlowSparkParticle(target.Center + Main.rand.NextVector2Circular(10f, 10f),
                    sparkVel, sparkColor, Main.rand.NextFloat(0.4f, 0.7f), Main.rand.Next(15, 25));
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Dust burst on hit
            for (int i = 0; i < 8 + ComboStep * 4; i++)
            {
                Dust d = Dust.NewDustPerfect(target.Center, DustID.Torch,
                    Main.rand.NextVector2Circular(8f, 8f), 0,
                    GetFireColor(Main.rand.NextFloat()), 1.4f + ComboStep * 0.2f);
                d.noGravity = true;
            }

            // Step 3 slam hit: extra halo ring burst
            if (ComboStep == 3)
            {
                var ring = new BloomRingParticle(target.Center, Vector2.Zero,
                    new Color(255, 140, 40), 0.6f, 20);
                MagnumParticleHandler.SpawnParticle(ring);
            }
        }
        #endregion

        #region Drawing
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;

            // Build ordered trail positions for CalamityStyleTrailRenderer
            Vector2[] orderedPositions = BuildTrailPositions();
            float[] orderedRotations = BuildTrailRotations();

            // Draw fire trail with bloom — width and intensity scale with combo step
            float trailWidth = 18f + ComboStep * 6f;
            float trailIntensity = 0.6f + ComboStep * 0.12f;
            Color trailPrimary = GetFireColor(0.3f + ComboStep * 0.15f);
            Color trailSecondary = GetFireColor(0.7f + ComboStep * 0.08f);

            CalamityStyleTrailRenderer.DrawTrailWithBloom(
                orderedPositions, orderedRotations, CalamityStyleTrailRenderer.TrailStyle.Flame,
                trailWidth, trailPrimary, trailSecondary, trailIntensity, 2.2f + ComboStep * 0.3f);

            // Draw infernal smear overlay behind the blade
            DrawSmearOverlay(sb);

            // Draw the blade sprite
            DrawBlade(sb, lightColor);

            // Draw lens flare at tip — bigger each combo step
            DrawLensFlare(sb);

            return false;
        }

        /// <summary>
        /// Draws the appropriate infernal smear texture as a swing arc overlay.
        /// Step 0 & 1 → Semi-Circular Smear (standard arc swings)
        /// Step 2     → Circular Smear (rising uppercut, full circle energy)
        /// Step 3     → Trient Smear (massive slam, triple-pronged devastation)
        /// </summary>
        private void DrawSmearOverlay(SpriteBatch sb)
        {
            // Only draw during the active swing portion (10%-90% progression)
            if (Progression < 0.10f || Progression > 0.92f) return;

            // Select smear texture based on combo step
            string smearPath = ComboStep switch
            {
                2 => "MagnumOpus/Content/TestWeapons/01_InfernalCleaver/InfernalCircularSmear",
                3 => "MagnumOpus/Content/TestWeapons/01_InfernalCleaver/InfernalTrientSmear",
                _ => "MagnumOpus/Content/TestWeapons/01_InfernalCleaver/InfernalSemiCircularSmear"
            };

            Texture2D smearTex = ModContent.Request<Texture2D>(smearPath).Value;
            Vector2 smearOrigin = smearTex.Size() * 0.5f;
            Vector2 drawPos = Owner.MountedCenter - Main.screenPosition;

            // Scale smear to match blade length — normalize against texture size
            float maxDim = Math.Max(smearTex.Width, smearTex.Height);
            float baseScale = (CurrentPhase.BladeLength * 2.2f) / maxDim;

            // Smear rotation follows the sword midpoint angle for the swing
            float midSwingAngle = SwordRotation;

            // Opacity ramps in/out smoothly during the swing
            float swingWindow = MathHelper.Clamp((Progression - 0.10f) / 0.15f, 0f, 1f);
            float fadeOut = MathHelper.Clamp((0.92f - Progression) / 0.15f, 0f, 1f);
            float smearAlpha = swingWindow * fadeOut;

            // Intensity increases with combo step
            float intensityMult = 0.55f + ComboStep * 0.12f;

            // Flip based on swing direction
            SpriteEffects smearFlip = Direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // Draw smear in additive blend mode for fiery glow
            SwingShaderSystem.BeginAdditive(sb);

            // Layer 1: Outer fire glow (large, dim)
            Color outerColor = GetFireColor(0.25f + Progression * 0.4f);
            outerColor.A = 0;
            sb.Draw(smearTex, drawPos, null, outerColor * smearAlpha * intensityMult * 0.35f,
                midSwingAngle, smearOrigin, baseScale * 1.1f, smearFlip, 0f);

            // Layer 2: Main smear (primary fire color)
            Color mainColor = GetFireColor(0.45f + Progression * 0.3f);
            mainColor.A = 0;
            sb.Draw(smearTex, drawPos, null, mainColor * smearAlpha * intensityMult * 0.65f,
                midSwingAngle, smearOrigin, baseScale, smearFlip, 0f);

            // Layer 3: Hot inner core (bright, smaller)
            Color coreColor = GetFireColor(0.75f + Progression * 0.15f);
            coreColor.A = 0;
            sb.Draw(smearTex, drawPos, null, coreColor * smearAlpha * intensityMult * 0.45f,
                midSwingAngle, smearOrigin, baseScale * 0.9f, smearFlip, 0f);

            SwingShaderSystem.RestoreSpriteBatch(sb);
        }

        private void DrawBlade(SpriteBatch sb, Color lightColor)
        {
            Texture2D bladeTex = Terraria.GameContent.TextureAssets.Item[ItemID.BreakerBlade].Value;
            Vector2 origin = new Vector2(0, bladeTex.Height);
            float rotation = SwordRotation + (Direction == -1 ? MathHelper.Pi : 0);
            SpriteEffects effects = Direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            float scaleVal = CurrentPhase.BladeLength / bladeTex.Height;
            Vector2 drawPos = Owner.MountedCenter - Main.screenPosition;

            // Normal blade
            sb.Draw(bladeTex, drawPos, null, lightColor, rotation, origin, scaleVal, effects, 0f);

            // Additive glow pass — intensity scales with combo step
            float glowIntensity = 0.3f + ComboStep * 0.1f;
            Color glowColor = GetFireColor(0.4f + Progression * 0.5f) * glowIntensity;
            glowColor.A = 0;

            SwingShaderSystem.BeginAdditive(sb);
            sb.Draw(bladeTex, drawPos, null, glowColor, rotation, origin, scaleVal * 1.05f, effects, 0f);
            sb.Draw(bladeTex, drawPos, null, glowColor * 0.5f, rotation, origin, scaleVal * 1.12f, effects, 0f);
            SwingShaderSystem.RestoreSpriteBatch(sb);
        }

        private void DrawLensFlare(SpriteBatch sb)
        {
            Texture2D flareTex = Terraria.GameContent.TextureAssets.Extra[98].Value;
            Vector2 tipWorld = Owner.MountedCenter + SwordDirection * CurrentPhase.BladeLength;
            Vector2 tipScreen = tipWorld - Main.screenPosition;
            Vector2 flareOrigin = flareTex.Size() * 0.5f;

            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.15f;
            float baseScale = (0.25f + ComboStep * 0.08f) * pulse;

            Color flareColor = GetFireColor(0.6f + Progression * 0.3f);
            flareColor.A = 0;

            SwingShaderSystem.BeginAdditive(sb);
            sb.Draw(flareTex, tipScreen, null, flareColor * 0.7f, 0f, flareOrigin, baseScale, SpriteEffects.None, 0f);
            sb.Draw(flareTex, tipScreen, null, flareColor * 0.4f, MathHelper.PiOver4, flareOrigin, baseScale * 0.7f, SpriteEffects.None, 0f);
            sb.Draw(flareTex, tipScreen, null, Color.White * 0.3f, 0f, flareOrigin, baseScale * 0.35f, SpriteEffects.None, 0f);
            SwingShaderSystem.RestoreSpriteBatch(sb);
        }
        #endregion

        #region Trail Helpers
        private Vector2[] BuildTrailPositions()
        {
            int count = Math.Min(trailIndex, TrailLength);
            Vector2[] result = new Vector2[count];
            for (int i = 0; i < count; i++)
            {
                int idx = (trailIndex - 1 - i + TrailLength) % TrailLength;
                result[i] = tipPositions[idx];
            }
            return result;
        }

        private float[] BuildTrailRotations()
        {
            int count = Math.Min(trailIndex, TrailLength);
            float[] result = new float[count];
            for (int i = 0; i < count; i++)
            {
                int idx = (trailIndex - 1 - i + TrailLength) % TrailLength;
                result[i] = tipRotations[idx];
            }
            return result;
        }

        private static Color GetFireColor(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float scaled = t * (FirePalette.Length - 1);
            int idx = (int)scaled;
            int next = Math.Min(idx + 1, FirePalette.Length - 1);
            return Color.Lerp(FirePalette[idx], FirePalette[next], scaled - idx);
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
