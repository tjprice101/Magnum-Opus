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

namespace MagnumOpus.Content.TestWeapons._02_FrostbiteEdge
{
    /// <summary>
    /// ❄️ Frostbite Edge Swing — 4-step combo projectile.
    /// Step 0: Icy Lunge — quick forward thrust-like arc
    /// Step 1: Frost Sweep — wide horizontal sweep
    /// Step 2: Shatter Strike — diagonal slash that launches IceShards
    /// Step 3: Glacial Cataclysm — massive overhead arc that detonates a FrostNova
    /// </summary>
    public class FrostbiteEdgeSwing : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.IceBlade;

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
        // Step 0: Icy Lunge — quick forward thrust with tight arc
        private static readonly ComboPhase Phase0_IcyLunge = new ComboPhase(
            new CurveSegment[]
            {
                new CurveSegment(EasingType.ExpIn, 0f, -0.4f, 0.85f, 1),
                new CurveSegment(EasingType.SineOut, 0.45f, 0.45f, 0.45f, 1),
                new CurveSegment(EasingType.PolyOut, 0.85f, 0.9f, 0.1f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 1.2f,
            duration: 48,
            bladeLength: 145f,
            flip: false,
            squish: 0.92f,
            damageMult: 0.9f
        );

        // Step 1: Frost Sweep — wide horizontal sweep
        private static readonly ComboPhase Phase1_FrostSweep = new ComboPhase(
            new CurveSegment[]
            {
                new CurveSegment(EasingType.SineOut, 0f, -0.9f, 0.2f, 1),
                new CurveSegment(EasingType.PolyIn, 0.15f, -0.7f, 1.6f, 3),
                new CurveSegment(EasingType.PolyOut, 0.75f, 0.9f, 0.1f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 1.8f,
            duration: 54,
            bladeLength: 160f,
            flip: true,
            squish: 0.85f,
            damageMult: 1.0f
        );

        // Step 2: Shatter Strike — sharp diagonal slash, spawns ice shards
        private static readonly ComboPhase Phase2_ShatterStrike = new ComboPhase(
            new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -0.6f, 0.1f, 2),
                new CurveSegment(EasingType.ExpIn, 0.15f, -0.5f, 1.5f, 1),
                new CurveSegment(EasingType.SineOut, 0.7f, 1f, -0.05f, 1)
            },
            maxAngle: MathHelper.PiOver2 * 1.6f,
            duration: 45,
            bladeLength: 150f,
            flip: false,
            squish: 0.88f,
            damageMult: 1.15f
        );

        // Step 3: Glacial Cataclysm — massive overhead slam, spawns frost nova
        private static readonly ComboPhase Phase3_GlacialCataclysm = new ComboPhase(
            new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -1f, 0.12f, 2),
                new CurveSegment(EasingType.ExpIn, 0.25f, -0.88f, 2.08f, 1),
                new CurveSegment(EasingType.PolyOut, 0.82f, 1.2f, -0.2f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 2.2f,
            duration: 84,
            bladeLength: 180f,
            flip: true,
            squish: 0.82f,
            damageMult: 1.4f
        );

        private static readonly ComboPhase[] AllPhases = { Phase0_IcyLunge, Phase1_FrostSweep, Phase2_ShatterStrike, Phase3_GlacialCataclysm };
        #endregion

        #region Constants
        private const int TrailLength = 60;

        private static readonly Color[] IcePalette = new Color[]
        {
            new Color(20, 40, 80),
            new Color(50, 100, 180),
            new Color(100, 180, 240),
            new Color(160, 220, 255),
            new Color(200, 240, 255),
            new Color(240, 250, 255)
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
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 36f, ref _);
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

            Vector2 tipWorld = Owner.MountedCenter + SwordDirection * CurrentPhase.BladeLength;
            tipPositions[trailIndex % TrailLength] = tipWorld;
            tipRotations[trailIndex % TrailLength] = SwordRotation;
            trailIndex++;

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

            // Ice crystal burst on swing start
            for (int i = 0; i < 5 + ComboStep * 2; i++)
            {
                Vector2 dustVel = (Projectile.velocity.ToRotation() + Main.rand.NextFloat(-0.5f, 0.5f))
                    .ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Dust d = Dust.NewDustPerfect(Owner.MountedCenter, DustID.IceTorch, dustVel, 0,
                    GetIceColor(Main.rand.NextFloat()), 1.0f + ComboStep * 0.2f);
                d.noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.Item28 with { Pitch = 0.2f + ComboStep * 0.1f }, Owner.Center);
        }

        private void DoBehavior_Swinging()
        {
            Projectile.Center = Owner.MountedCenter;
            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, SwordRotation - MathHelper.PiOver2);

            float lightIntensity = 0.3f + ComboStep * 0.12f;
            Vector2 tipPos = Owner.MountedCenter + SwordDirection * CurrentPhase.BladeLength;
            Lighting.AddLight(tipPos, 0.2f * lightIntensity, 0.6f * lightIntensity, 1f * lightIntensity);

            // Frost dust trail
            float swingSpeed = Math.Abs(SwingAngleShiftAtProgress(Progression) -
                SwingAngleShiftAtProgress(Math.Max(0, Progression - 0.02f)));

            if (swingSpeed > 0.01f && Main.rand.NextBool(Math.Max(1, 3 - ComboStep)))
            {
                float bladeProgress = Main.rand.NextFloat(0.4f, 1f);
                Vector2 dustPos = Owner.MountedCenter + SwordDirection * CurrentPhase.BladeLength * bladeProgress;
                Vector2 dustVel = SwordDirection.RotatedBy(MathHelper.PiOver2 * Direction) * Main.rand.NextFloat(0.5f, 2.5f);

                Dust d = Dust.NewDustPerfect(dustPos, DustID.IceTorch, dustVel, 0,
                    GetIceColor(bladeProgress), 1.2f + ComboStep * 0.15f);
                d.noGravity = true;

                // Snowflake accents
                if (Main.rand.NextBool(3))
                {
                    Dust d2 = Dust.NewDustPerfect(dustPos, DustID.Frost, dustVel * 0.4f, 100, default, 0.6f);
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
                    Projectile.Kill();
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

            // Step 2 (Shatter Strike): spawn ice shards at 65% progress
            if (ComboStep == 2 && Progression >= 0.65f)
            {
                hasSpawnedSpecial = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tipPos = Owner.MountedCenter + SwordDirection * CurrentPhase.BladeLength;
                    for (int i = 0; i < 6; i++)
                    {
                        float angle = MathHelper.ToRadians(-60 + i * 24);
                        Vector2 vel = (SwordRotation + angle).ToRotationVector2() * Main.rand.NextFloat(9f, 15f);
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), tipPos, vel,
                            ModContent.ProjectileType<IceShardProjectile>(),
                            Projectile.damage / 3, 2f, Projectile.owner);
                    }
                }
                SoundEngine.PlaySound(SoundID.Item27 with { Pitch = 0.4f }, Owner.Center);
            }

            // Step 3 (Glacial Cataclysm): frost nova at 88% progress
            if (ComboStep == 3 && Progression >= 0.88f)
            {
                hasSpawnedSpecial = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 slamPoint = Owner.MountedCenter + SwordDirection * CurrentPhase.BladeLength;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), slamPoint, Vector2.Zero,
                        ModContent.ProjectileType<FrostNovaProjectile>(),
                        Projectile.damage, 6f, Projectile.owner);
                }
                SoundEngine.PlaySound(SoundID.Item120 with { Pitch = -0.3f, Volume = 1.4f }, Owner.Center);

                for (int k = 0; k < Main.player.Length; k++)
                {
                    Player p = Main.player[k];
                    if (p.active && !p.dead && p.Distance(Owner.Center) < 800f)
                        p.GetModPlayer<global::MagnumOpus.Content.LaCampanella.Debuffs.ScreenShakePlayer>().AddShake(10f, 18);
                }
            }
        }
        #endregion

        #region Hit Effects
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int remainingDamageCount)
        {
            int sparkCount = 3 + ComboStep * 2;
            for (int i = 0; i < sparkCount; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(5f, 5f);
                Color sparkColor = GetIceColor(Main.rand.NextFloat());
                var spark = new GlowSparkParticle(target.Center + Main.rand.NextVector2Circular(8f, 8f),
                    sparkVel, sparkColor, Main.rand.NextFloat(0.3f, 0.6f), Main.rand.Next(12, 22));
                MagnumParticleHandler.SpawnParticle(spark);
            }

            for (int i = 0; i < 6 + ComboStep * 3; i++)
            {
                Dust d = Dust.NewDustPerfect(target.Center, DustID.IceTorch,
                    Main.rand.NextVector2Circular(6f, 6f), 0,
                    GetIceColor(Main.rand.NextFloat()), 1.2f + ComboStep * 0.15f);
                d.noGravity = true;
            }

            if (ComboStep == 3)
            {
                var ring = new BloomRingParticle(target.Center, Vector2.Zero,
                    new Color(100, 200, 255), 0.5f, 18);
                MagnumParticleHandler.SpawnParticle(ring);
            }
        }
        #endregion

        #region Drawing
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;

            Vector2[] orderedPositions = BuildTrailPositions();
            float[] orderedRotations = BuildTrailRotations();

            float trailWidth = 16f + ComboStep * 5f;
            float trailIntensity = 0.55f + ComboStep * 0.1f;
            Color trailPrimary = GetIceColor(0.3f + ComboStep * 0.12f);
            Color trailSecondary = GetIceColor(0.7f + ComboStep * 0.06f);

            CalamityStyleTrailRenderer.DrawTrailWithBloom(
                orderedPositions, orderedRotations, CalamityStyleTrailRenderer.TrailStyle.Ice,
                trailWidth, trailPrimary, trailSecondary, trailIntensity, 2.0f + ComboStep * 0.25f);

            DrawBlade(sb, lightColor);
            DrawLensFlare(sb);

            return false;
        }

        private void DrawBlade(SpriteBatch sb, Color lightColor)
        {
            Texture2D bladeTex = Terraria.GameContent.TextureAssets.Item[ItemID.IceBlade].Value;
            Vector2 origin = new Vector2(0, bladeTex.Height);
            float rotation = SwordRotation + (Direction == -1 ? MathHelper.Pi : 0);
            SpriteEffects effects = Direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            float scaleVal = CurrentPhase.BladeLength / bladeTex.Height;
            Vector2 drawPos = Owner.MountedCenter - Main.screenPosition;

            sb.Draw(bladeTex, drawPos, null, lightColor, rotation, origin, scaleVal, effects, 0f);

            float glowIntensity = 0.25f + ComboStep * 0.08f;
            Color glowColor = GetIceColor(0.5f + Progression * 0.4f) * glowIntensity;
            glowColor.A = 0;

            SwingShaderSystem.BeginAdditive(sb);
            sb.Draw(bladeTex, drawPos, null, glowColor, rotation, origin, scaleVal * 1.05f, effects, 0f);
            sb.Draw(bladeTex, drawPos, null, glowColor * 0.5f, rotation, origin, scaleVal * 1.1f, effects, 0f);
            SwingShaderSystem.RestoreSpriteBatch(sb);
        }

        private void DrawLensFlare(SpriteBatch sb)
        {
            Texture2D flareTex = Terraria.GameContent.TextureAssets.Extra[98].Value;
            Vector2 tipWorld = Owner.MountedCenter + SwordDirection * CurrentPhase.BladeLength;
            Vector2 tipScreen = tipWorld - Main.screenPosition;
            Vector2 flareOrigin = flareTex.Size() * 0.5f;

            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.12f;
            float baseScale = (0.2f + ComboStep * 0.06f) * pulse;

            Color flareColor = GetIceColor(0.6f + Progression * 0.3f);
            flareColor.A = 0;

            SwingShaderSystem.BeginAdditive(sb);
            sb.Draw(flareTex, tipScreen, null, flareColor * 0.65f, 0f, flareOrigin, baseScale, SpriteEffects.None, 0f);
            sb.Draw(flareTex, tipScreen, null, flareColor * 0.35f, MathHelper.PiOver4, flareOrigin, baseScale * 0.6f, SpriteEffects.None, 0f);
            sb.Draw(flareTex, tipScreen, null, Color.White * 0.25f, 0f, flareOrigin, baseScale * 0.3f, SpriteEffects.None, 0f);
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

        private static Color GetIceColor(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float scaled = t * (IcePalette.Length - 1);
            int idx = (int)scaled;
            int next = Math.Min(idx + 1, IcePalette.Length - 1);
            return Color.Lerp(IcePalette[idx], IcePalette[next], scaled - idx);
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
