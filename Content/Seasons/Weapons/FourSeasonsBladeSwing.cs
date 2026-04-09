using System;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static MagnumOpus.Common.Systems.Particles.Particle;
using static MagnumOpus.Common.Systems.ThemedParticles;

namespace MagnumOpus.Content.Seasons.Weapons
{
    /// <summary>
    /// Four Seasons Blade swing projectile — cycles through Spring, Summer, Autumn, Winter.
    /// ExobladeStyleSwing architecture with dynamic per-season colors, debuffs, and VFX.
    /// </summary>
    public class FourSeasonsBladeSwing : ExobladeStyleSwing
    {
        #region Season Palettes

        private static readonly Color[] SeasonPrimary =
        {
            new Color(255, 183, 197),    // Spring pink
            new Color(255, 215, 0),      // Summer gold
            new Color(255, 140, 50),     // Autumn orange
            new Color(150, 220, 255)     // Winter blue
        };

        private static readonly Color[] SeasonSecondary =
        {
            new Color(144, 238, 144),    // Spring green
            new Color(255, 140, 0),      // Summer orange
            new Color(139, 90, 43),      // Autumn brown
            new Color(240, 250, 255)     // Winter white
        };

        private static readonly Color[] SeasonAccent =
        {
            new Color(200, 255, 200),    // Spring pale green
            new Color(255, 140, 0),      // Summer orange
            new Color(218, 165, 32),     // Autumn gold
            new Color(240, 250, 255)     // Winter white
        };

        private static readonly Color[] SeasonSlashSecondary =
        {
            new Color(120, 80, 100),     // Spring rose shadow
            new Color(120, 50, 10),      // Summer deep ember
            new Color(80, 40, 20),       // Autumn dark bark
            new Color(30, 50, 100)       // Winter deep ocean
        };

        private static readonly string[] SeasonGradients =
        {
            "MagnumOpus/Assets/VFX Asset Library/ColorGradients/EroicaGradientPALELUTandRAMP",     // Spring
            "MagnumOpus/Assets/VFX Asset Library/ColorGradients/LaCampanellaGradientLUTandRAMP",   // Summer
            "MagnumOpus/Assets/VFX Asset Library/ColorGradients/LaCampanellaGradientLUTandRAMP",   // Autumn
            "MagnumOpus/Assets/VFX Asset Library/ColorGradients/ClairDeLuneGradientLUTandRAMP"     // Winter
        };

        #endregion

        private int seasonPhase = 0;
        private bool hasSpawnedSpecial = false;

        private int Season => seasonPhase % 4;

        public override string Texture => "MagnumOpus/Content/Seasons/Weapons/FourSeasonsBlade";

        // ═══ Dynamic season-based overrides ═══
        protected override bool SupportsDash => false;
        protected override float BladeLength => 155f + Season * 5f; // 155-170 per season
        protected override int BaseSwingFrames => Season switch { 0 => 52, 1 => 45, 2 => 55, _ => 58 };
        protected override float TextureDrawScale => 0.12f;

        protected override Color SlashPrimaryColor => SeasonPrimary[Season];
        protected override Color SlashSecondaryColor => SeasonSlashSecondary[Season];
        protected override Color SlashAccentColor => SeasonAccent[Season];
        protected override string GradientLUTPath => SeasonGradients[Season];

        protected override float SwingArcMultiplier => Season switch { 0 => 1.4f, 1 => 1.6f, 2 => 1.8f, _ => 2.0f };

        protected override SoundStyle SwingSoundStyle => Season switch
        {
            0 => SoundID.Item71 with { Volume = 0.7f, Pitch = 0.1f },
            1 => SoundID.Item71 with { Volume = 0.7f, Pitch = 0.0f },
            2 => SoundID.Item71 with { Volume = 0.7f, Pitch = -0.2f },
            _ => SoundID.Item71 with { Volume = 0.7f, Pitch = -0.4f }
        };

        protected override Color GetLensFlareColor(float p)
            => Color.Lerp(SeasonPrimary[Season], SeasonAccent[Season], (float)Math.Pow(p, 2));

        protected override Color GetLightColor(float p)
            => SeasonPrimary[Season] * (0.4f + p * 0.3f);

        protected override Color GetSwingDustColor()
        {
            float t = Main.rand.NextFloat();
            return Color.Lerp(SeasonPrimary[Season], SeasonSecondary[Season], t);
        }

        // ═══ Swing lifecycle ═══

        protected override void OnSwingStart(bool isFirstSwing)
        {
            if (!isFirstSwing)
                seasonPhase++;
            hasSpawnedSpecial = false;

            Color primary = SeasonPrimary[Season];

            // VFX burst on season start
            CustomParticles.GenericFlare(Owner.Center, primary, 0.6f, 15);
            CustomParticles.HaloRing(Owner.Center, primary * 0.8f, 0.35f, 12);

            for (int i = 0; i < 3; i++)
            {
                Vector2 noteVel = Main.rand.NextVector2Circular(3f, 3f);
                MusicNote(Owner.Center + noteVel * 5f, noteVel, primary, 0.75f, 25);
            }

            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Color c = Color.Lerp(primary, SeasonSecondary[Season], Main.rand.NextFloat());
                var glow = new GenericGlowParticle(Owner.Center + vel * 3f, vel, c * 0.7f, 0.3f, 18, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Crescendo check: every 16th swing (4 complete 4-season cycles)
            if (seasonPhase > 0 && seasonPhase % 16 == 0)
            {
                SpawnCrescendo();
            }
        }

        protected override void OnSwingFrame()
        {
            if (hasSpawnedSpecial) return;

            // Season-specific blade-tip particles at ~75% swing progress
            if (Progression >= 0.75f)
            {
                hasSpawnedSpecial = true;
                Color primary = SeasonPrimary[Season];
                Vector2 tipPos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale;

                // Sparkle burst at blade tip
                for (int i = 0; i < 5; i++)
                {
                    Vector2 sparkleVel = Main.rand.NextVector2Circular(4f, 4f);
                    var spark = new SparkleParticle(tipPos, sparkleVel, primary, 0.4f, 20);
                    MagnumParticleHandler.SpawnParticle(spark);
                }

                // Music note from tip
                if (Main.rand.NextBool(2))
                {
                    float noteScale = Main.rand.NextFloat(0.7f, 0.9f);
                    MusicNote(tipPos, -SwordDirection * 2f, primary, noteScale, 30);
                }

                // Season-specific sub-effects
                switch (Season)
                {
                    case 0: // Spring — petal scatter
                        for (int i = 0; i < 3; i++)
                        {
                            Vector2 petalVel = -SwordDirection.RotatedByRandom(0.5f) * Main.rand.NextFloat(2f, 5f);
                            Dust d = Dust.NewDustPerfect(tipPos, DustID.PinkTorch, petalVel, 0, default, 1.2f);
                            d.noGravity = true;
                        }
                        break;
                    case 1: // Summer — ember burst
                        for (int i = 0; i < 4; i++)
                        {
                            Vector2 emberVel = -SwordDirection.RotatedByRandom(0.4f) * Main.rand.NextFloat(3f, 7f);
                            Dust d = Dust.NewDustPerfect(tipPos, DustID.Torch, emberVel, 0, default, 1.4f);
                            d.noGravity = true;
                        }
                        break;
                    case 2: // Autumn — leaf drift
                        for (int i = 0; i < 3; i++)
                        {
                            Vector2 leafVel = -SwordDirection.RotatedByRandom(0.6f) * Main.rand.NextFloat(2f, 4f);
                            leafVel.Y -= 1f;
                            Dust d = Dust.NewDustPerfect(tipPos, DustID.AmberBolt, leafVel, 0, default, 1.3f);
                            d.noGravity = true;
                        }
                        break;
                    case 3: // Winter — frost shards
                        for (int i = 0; i < 4; i++)
                        {
                            Vector2 iceVel = -SwordDirection.RotatedByRandom(0.4f) * Main.rand.NextFloat(3f, 6f);
                            Dust d = Dust.NewDustPerfect(tipPos, DustID.Frost, iceVel, 0, default, 1.2f);
                            d.noGravity = true;
                        }
                        break;
                }
            }
        }

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player owner = Main.player[Projectile.owner];
            Color primary = SeasonPrimary[Season];
            Color secondary = SeasonSecondary[Season];

            switch (Season)
            {
                case 0: // Spring — healing bloom + poison
                    owner.statLife = Math.Min(owner.statLife + 8, owner.statLifeMax2);
                    owner.HealEffect(8);
                    target.AddBuff(BuffID.Poisoned, 300);
                    for (int i = 0; i < 6; i++)
                    {
                        Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                        var glow = new GenericGlowParticle(target.Center, vel, primary * 0.8f, 0.35f, 20, true);
                        MagnumParticleHandler.SpawnParticle(glow);
                    }
                    break;

                case 1: // Summer — scorching blaze
                    target.AddBuff(BuffID.OnFire3, 300);
                    target.AddBuff(BuffID.Daybreak, 180);
                    for (int i = 0; i < 8; i++)
                    {
                        Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                        Dust d = Dust.NewDustPerfect(target.Center, DustID.Torch, vel, 0, default, 1.5f);
                        d.noGravity = true;
                    }
                    CustomParticles.GenericFlare(target.Center, primary, 0.6f, 15);
                    break;

                case 2: // Autumn — harvest lifesteal + cursed flames
                    int stolen = Math.Min(damageDone / 10, 15);
                    if (stolen > 0)
                    {
                        owner.statLife = Math.Min(owner.statLife + stolen, owner.statLifeMax2);
                        owner.HealEffect(stolen);
                    }
                    target.AddBuff(BuffID.CursedInferno, 300);
                    for (int i = 0; i < 6; i++)
                    {
                        Vector2 toOwner = (owner.Center - target.Center).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(3f, 7f);
                        var glow = new GenericGlowParticle(target.Center, toOwner, primary * 0.7f, 0.3f, 25, true);
                        MagnumParticleHandler.SpawnParticle(glow);
                    }
                    break;

                case 3: // Winter — deep freeze
                    target.AddBuff(BuffID.Frostburn2, 300);
                    if (Main.rand.NextBool(4))
                        target.AddBuff(BuffID.Frozen, 60);
                    for (int i = 0; i < 8; i++)
                    {
                        Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                        Dust d = Dust.NewDustPerfect(target.Center, DustID.Frost, vel, 0, default, 1.3f);
                        d.noGravity = true;
                    }
                    CustomParticles.GenericFlare(target.Center, primary, 0.5f, 18);
                    break;
            }

            // Universal gradient halo
            CustomParticles.HaloRing(target.Center, Color.Lerp(primary, secondary, 0.5f), 0.35f, 15);
        }

        // ═══ Crescendo system ═══
        private void SpawnCrescendo()
        {
            var source = Owner.GetSource_ItemUse(Owner.HeldItem);
            Vector2 direction = (Main.MouseWorld - Owner.Center).SafeNormalize(Vector2.UnitX);

            for (int i = 0; i < 4; i++)
            {
                float angleOffset = MathHelper.ToRadians(-30f + i * 20f);
                Vector2 vel = direction.RotatedBy(angleOffset) * 16f;
                int damage = (int)(Projectile.damage * 1.5f);
                Projectile.NewProjectile(source, Owner.Center, vel,
                    ModContent.ProjectileType<Projectiles.VivaldiSeasonalWave>(),
                    damage, Projectile.knockBack, Projectile.owner);
            }

            // Massive Crescendo VFX
            CustomParticles.GenericFlare(Owner.Center, Color.White, 1.2f, 25);
            CustomParticles.GenericFlare(Owner.Center, SeasonPrimary[0], 0.9f, 22);
            CustomParticles.GenericFlare(Owner.Center, SeasonPrimary[1], 0.8f, 20);
            CustomParticles.GenericFlare(Owner.Center, SeasonPrimary[2], 0.7f, 18);
            CustomParticles.GenericFlare(Owner.Center, SeasonPrimary[3], 0.6f, 16);

            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                MusicNote(Owner.Center, noteVel, SeasonPrimary[i], 0.9f, 35);
            }

            for (int ring = 0; ring < 6; ring++)
            {
                Color[] ringColors = { SeasonPrimary[0], SeasonPrimary[1], SeasonPrimary[2], SeasonPrimary[3], SeasonSecondary[0], SeasonSecondary[1] };
                CustomParticles.HaloRing(Owner.Center, ringColors[ring], 0.3f + ring * 0.12f, 15 + ring * 3);
            }

            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Color[] allColors = { SeasonPrimary[0], SeasonSecondary[0], SeasonPrimary[1], SeasonSecondary[1],
                                      SeasonPrimary[2], SeasonSecondary[2], SeasonPrimary[3], SeasonSecondary[3] };
                Color c = allColors[i % allColors.Length];
                var burst = new GenericGlowParticle(Owner.Center, vel, c * 0.8f, 0.4f, 22, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            MagnumScreenEffects.AddScreenShake(6f);
        }
    }
}
