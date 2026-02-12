using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Trails;
using static MagnumOpus.Common.Systems.Particles.Particle;

namespace MagnumOpus.Content.TestWeapons._04_VerdantCrescendo
{
    /// <summary>
    /// 🌿 Verdant Crescendo 4-Step Combo Swing Projectile
    /// Step 0: Vine Lash — quick horizontal slash
    /// Step 1: Briar Sweep — wide rising arc
    /// Step 2: Thorn Eruption — downward slam, spawns 5 ThornVolleyProjectiles at 65%
    /// Step 3: Overgrowth Cataclysm — massive overhead slam, spawns BloomBurstProjectile at 85% + screen shake
    /// </summary>
    public class VerdantCrescendoSwing : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.BladeofGrass;

        #region ComboPhase Definition
        private struct ComboPhase
        {
            public CurveSegment[] AnimCurves;
            public float MaxAngle;
            public int Duration;
            public float BladeLength;
            public bool FlipDirection;
            public float SquishRange;
            public float DamageMultiplier;
        }

        // Step 0: Vine Lash — quick horizontal slash
        private static readonly ComboPhase Phase0_VineLash = new ComboPhase
        {
            AnimCurves = new CurveSegment[]
            {
                new CurveSegment(EasingType.SineOut, 0f, -1f, 0.3f),
                new CurveSegment(EasingType.PolyIn, 0.2f, -0.7f, 1.5f, 3),
                new CurveSegment(EasingType.PolyOut, 0.8f, 0.8f, 0.2f, 2)
            },
            MaxAngle = MathHelper.Pi * 0.62f,
            Duration = 44,
            BladeLength = 145f,
            FlipDirection = false,
            SquishRange = 0.15f,
            DamageMultiplier = 0.9f
        };

        // Step 1: Briar Sweep — wide rising arc (flipped)
        private static readonly ComboPhase Phase1_BriarSweep = new ComboPhase
        {
            AnimCurves = new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -1f, 0.4f, 2),
                new CurveSegment(EasingType.SineIn, 0.25f, -0.6f, 1.4f),
                new CurveSegment(EasingType.PolyOut, 0.82f, 0.8f, 0.2f, 2)
            },
            MaxAngle = MathHelper.Pi * 0.88f,
            Duration = 52,
            BladeLength = 158f,
            FlipDirection = true,
            SquishRange = 0.18f,
            DamageMultiplier = 1.0f
        };

        // Step 2: Thorn Eruption — downward strike, spawns ThornVolley at 65%
        private static readonly ComboPhase Phase2_ThornEruption = new ComboPhase
        {
            AnimCurves = new CurveSegment[]
            {
                new CurveSegment(EasingType.SineOut, 0f, -1f, 0.25f),
                new CurveSegment(EasingType.ExpIn, 0.18f, -0.75f, 1.7f),
                new CurveSegment(EasingType.PolyOut, 0.75f, 0.95f, 0.05f, 2)
            },
            MaxAngle = MathHelper.Pi * 0.78f,
            Duration = 46,
            BladeLength = 150f,
            FlipDirection = false,
            SquishRange = 0.2f,
            DamageMultiplier = 1.15f
        };

        // Step 3: Overgrowth Cataclysm — massive overhead slam, spawns BloomBurst at 85%
        private static readonly ComboPhase Phase3_OvergrowthCataclysm = new ComboPhase
        {
            AnimCurves = new CurveSegment[]
            {
                new CurveSegment(EasingType.SineOut, 0f, -1f, 0.2f),
                new CurveSegment(EasingType.PolyIn, 0.15f, -0.8f, 0.5f, 2),
                new CurveSegment(EasingType.ExpIn, 0.45f, -0.3f, 1.5f),
                new CurveSegment(EasingType.PolyOut, 0.88f, 1.2f, -0.2f, 2)
            },
            MaxAngle = MathHelper.Pi * 1.1f,
            Duration = 80,
            BladeLength = 180f,
            FlipDirection = true,
            SquishRange = 0.25f,
            DamageMultiplier = 1.4f
        };

        private static readonly ComboPhase[] Phases = { Phase0_VineLash, Phase1_BriarSweep, Phase2_ThornEruption, Phase3_OvergrowthCataclysm };
        #endregion

        #region Nature Palette
        private static readonly Color[] NaturePalette = new Color[]
        {
            new Color(20, 80, 30),
            new Color(40, 160, 60),
            new Color(100, 210, 90),
            new Color(180, 220, 80),
            new Color(240, 250, 180),
            new Color(255, 255, 220)
        };

        private Color GetPaletteColor(float t)
        {
            float scaled = t * (NaturePalette.Length - 1);
            int idx = Math.Clamp((int)scaled, 0, NaturePalette.Length - 2);
            return Color.Lerp(NaturePalette[idx], NaturePalette[idx + 1], scaled - idx);
        }
        #endregion

        #region State
        private const int TrailLength = 60;
        private const int StasisDuration = 12;

        private Vector2[] tipPositions = new Vector2[TrailLength];
        private float[] tipRotations = new float[TrailLength];
        private int trailIndex = 0;
        private bool hasSpawnedSecondary = false;
        private int currentStep = 0;

        private Player Owner => Main.player[Projectile.owner];
        private ComboPhase ActivePhase => Phases[Math.Clamp(currentStep, 0, Phases.Length - 1)];
        public int SwingTime { get => (int)Projectile.localAI[0]; set => Projectile.localAI[0] = value; }
        public float SquishFactor { get => Projectile.localAI[1]; set => Projectile.localAI[1] = value; }
        private float Timer => SwingTime - Projectile.timeLeft;
        private float Progression => SwingTime > 0 ? Timer / SwingTime : 0f;
        public bool InPostSwingStasis { get; set; }
        private int Direction => Math.Sign(Projectile.velocity.X) <= 0 ? -1 : 1;
        private float BaseRotation => Projectile.velocity.ToRotation();
        #endregion

        #region Animation
        private float SwingAngleAtProgress(float progress)
        {
            var phase = ActivePhase;
            return phase.MaxAngle * PiecewiseAnimation(progress, phase.AnimCurves)
                   * (phase.FlipDirection ? -1f : 1f);
        }

        private float SwordRotation => BaseRotation + SwingAngleAtProgress(Progression) * Direction;

        private float SquishAtProgress(float progress)
        {
            float shift = Math.Abs(SwingAngleAtProgress(progress));
            float range = ActivePhase.SquishRange;
            return MathHelper.Lerp(1f + range * 0.6f, 1f - range, (float)Math.Abs(Math.Sin(shift)));
        }

        private Vector2 SwordDirection => SwordRotation.ToRotationVector2() * SquishAtProgress(Progression);
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

        public override bool ShouldUpdatePosition() => false;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            Vector2 start = Owner.RotatedRelativePoint(Owner.MountedCenter, true);
            Vector2 end = start + SwordDirection * ActivePhase.BladeLength * Projectile.scale;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 40f, ref _);
        }

        public override void AI()
        {
            Player player = Owner;
            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }

            // Initialize on first real frame
            if (SwingTime == 0)
            {
                currentStep = Math.Clamp((int)Projectile.ai[0], 0, Phases.Length - 1);
                SwingTime = ActivePhase.Duration * Projectile.MaxUpdates;
                Projectile.timeLeft = SwingTime + StasisDuration * Projectile.MaxUpdates;
                SquishFactor = 1f - ActivePhase.SquishRange;
                SoundEngine.PlaySound(SoundID.Item71 with { Pitch = -0.1f + currentStep * 0.1f }, player.Center);
            }

            // Post-swing stasis
            if (Timer >= SwingTime)
            {
                InPostSwingStasis = true;
                player.heldProj = Projectile.whoAmI;
                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, SwordRotation - MathHelper.PiOver2);
                return;
            }

            InPostSwingStasis = false;
            float progress = Progression;
            Projectile.damage = (int)(player.GetWeaponDamage(player.HeldItem) * ActivePhase.DamageMultiplier);

            // Player attachment
            Projectile.Center = player.RotatedRelativePoint(player.MountedCenter, true);
            player.heldProj = Projectile.whoAmI;
            player.ChangeDir(Direction);
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, SwordRotation - MathHelper.PiOver2);

            // Trail tracking
            Vector2 tipWorld = Projectile.Center + SwordDirection * ActivePhase.BladeLength * Projectile.scale;
            trailIndex = (trailIndex + 1) % TrailLength;
            tipPositions[trailIndex] = tipWorld;
            tipRotations[trailIndex] = SwordRotation;

            // VFX — nature dust
            if (!Main.dedServ && Timer % Projectile.MaxUpdates == 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 dustPos = tipWorld + Main.rand.NextVector2Circular(10f, 10f);
                    Vector2 dustVel = -SwordDirection * Main.rand.NextFloat(0.5f, 2f);
                    Dust d = Dust.NewDustPerfect(dustPos, DustID.JungleGrass, dustVel, 0,
                        GetPaletteColor(progress), Main.rand.NextFloat(1.0f, 1.5f));
                    d.noGravity = true;
                    d.fadeIn = 1.2f;
                }

                if (Main.rand.NextBool(2))
                {
                    Dust gem = Dust.NewDustPerfect(tipWorld + Main.rand.NextVector2Circular(12f, 12f),
                        DustID.GemEmerald, -SwordDirection * 0.8f, 0, Color.White, 0.8f);
                    gem.noGravity = true;
                }

                // Glow trail particles
                if (Main.rand.NextBool(3))
                {
                    Color glowColor = GetPaletteColor(progress) * 0.6f;
                    var glow = new GenericGlowParticle(tipWorld, -SwordDirection * 0.5f,
                        glowColor, Main.rand.NextFloat(0.15f, 0.3f), Main.rand.Next(8, 14), true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
            }

            // Step 2: Spawn thorn volleys at 65% progress
            if (currentStep == 2 && !hasSpawnedSecondary && progress >= 0.65f)
            {
                hasSpawnedSecondary = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    SoundEngine.PlaySound(SoundID.Item17 with { Pitch = 0.3f }, tipWorld);
                    for (int i = 0; i < 5; i++)
                    {
                        float spread = MathHelper.ToRadians(-30f + 15f * i);
                        Vector2 thornVel = SwordDirection.RotatedBy(spread) * Main.rand.NextFloat(9f, 13f);
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), tipWorld, thornVel,
                            ModContent.ProjectileType<ThornVolleyProjectile>(),
                            (int)(Projectile.damage * 0.45f), Projectile.knockBack * 0.3f, Projectile.owner);
                    }
                }
            }

            // Step 3: Spawn bloom burst at 85% progress
            if (currentStep == 3 && !hasSpawnedSecondary && progress >= 0.85f)
            {
                hasSpawnedSecondary = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    SoundEngine.PlaySound(SoundID.Item29 with { Pitch = -0.3f, Volume = 1.2f }, tipWorld);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), tipWorld, Vector2.Zero,
                        ModContent.ProjectileType<BloomBurstProjectile>(),
                        (int)(Projectile.damage * 0.6f), Projectile.knockBack * 0.5f, Projectile.owner);
                }

                // Screen shake
                if (player.TryGetModPlayer(out global::MagnumOpus.Content.LaCampanella.Debuffs.ScreenShakePlayer shakePlayer))
                    shakePlayer.AddShake(10f, 18);
            }

            Projectile.rotation = SwordRotation;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(5f, 5f);
                Color sparkColor = Color.Lerp(new Color(60, 180, 70), new Color(200, 240, 100), Main.rand.NextFloat());
                var spark = new GlowSparkParticle(target.Center, sparkVel, sparkColor,
                    Main.rand.NextFloat(0.15f, 0.3f), Main.rand.Next(8, 14));
                MagnumParticleHandler.SpawnParticle(spark);
            }

            for (int i = 0; i < 5; i++)
            {
                Dust d = Dust.NewDustPerfect(target.Center, DustID.JungleGrass,
                    Main.rand.NextVector2Circular(5f, 5f), 0, default, 1.3f);
                d.noGravity = true;
            }

            // Extra ring on finisher slam hit
            if (currentStep == 3)
            {
                var ring = new BloomRingParticle(target.Center, Vector2.Zero, new Color(100, 220, 80), 0.6f, 14);
                MagnumParticleHandler.SpawnParticle(ring);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ || SwingTime <= 0) return false;

            SpriteBatch sb = Main.spriteBatch;
            Player player = Owner;
            Vector2 mountedCenter = player.RotatedRelativePoint(player.MountedCenter, true);

            // Build ordered trail arrays
            Vector2[] orderedPositions = new Vector2[TrailLength];
            float[] orderedRotations = new float[TrailLength];
            for (int i = 0; i < TrailLength; i++)
            {
                int idx = (trailIndex - i + TrailLength * 2) % TrailLength;
                orderedPositions[i] = tipPositions[idx];
                orderedRotations[i] = tipRotations[idx];
            }

            // Nature trail via CalamityStyleTrailRenderer
            try
            {
                CalamityStyleTrailRenderer.DrawTrailWithBloom(
                    orderedPositions, orderedRotations, CalamityStyleTrailRenderer.TrailStyle.Nature,
                    ActivePhase.BladeLength * 0.22f,
                    new Color(60, 180, 70), new Color(200, 240, 100),
                    intensity: 0.9f, bloomMultiplier: 2.2f);
            }
            catch { }

            // Blade glow
            Texture2D bladeTex = TextureAssets.Projectile[Type].Value;
            Vector2 origin = new Vector2(0, bladeTex.Height * 0.5f);
            float rot = SwordRotation;
            float bladeScale = ActivePhase.BladeLength / bladeTex.Width * Projectile.scale;

            SwingShaderSystem.BeginAdditive(sb);

            Color outerGlow = new Color(40, 160, 60, 0) * 0.3f;
            sb.Draw(bladeTex, mountedCenter - Main.screenPosition, null, outerGlow,
                rot, origin, bladeScale * 1.15f, Direction < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None, 0f);

            Color innerGlow = new Color(150, 230, 120, 0) * 0.5f;
            sb.Draw(bladeTex, mountedCenter - Main.screenPosition, null, innerGlow,
                rot, origin, bladeScale, Direction < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None, 0f);

            // Lens flare at tip
            float progress = Progression;
            float flareIntensity = (float)Math.Sin(progress * MathHelper.Pi) * 0.7f;
            if (flareIntensity > 0.05f)
            {
                Texture2D flareTex = TextureAssets.Extra[98].Value;
                Vector2 tipScreen = (mountedCenter + SwordDirection * ActivePhase.BladeLength * Projectile.scale) - Main.screenPosition;
                Vector2 flareOrigin = flareTex.Size() * 0.5f;
                float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.12f;
                Color flareColor = GetPaletteColor(progress) with { A = 0 } * flareIntensity;

                sb.Draw(flareTex, tipScreen, null, flareColor * 0.5f, Main.GameUpdateCount * 0.03f,
                    flareOrigin, 0.35f * pulse, SpriteEffects.None, 0f);
                sb.Draw(flareTex, tipScreen, null, new Color(255, 255, 220, 0) * flareIntensity * 0.4f, 0f,
                    flareOrigin, 0.15f * pulse, SpriteEffects.None, 0f);
            }

            SwingShaderSystem.RestoreSpriteBatch(sb);

            return false;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(currentStep);
            writer.Write(hasSpawnedSecondary);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            currentStep = reader.ReadInt32();
            hasSpawnedSecondary = reader.ReadBoolean();
        }
    }
}
