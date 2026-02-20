using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Trails;
using static MagnumOpus.Common.Systems.Particles.Particle;
using static MagnumOpus.Common.Systems.ThemedParticles;

namespace MagnumOpus.Content.Seasons.Weapons
{
    /// <summary>
    /// Swing projectile for the Four Seasons Blade.
    /// Each combo phase represents a different season with unique palette, trail, dust, and on-hit effects.
    /// Phase 0 = Spring (nature sweep), Phase 1 = Summer (blazing slash),
    /// Phase 2 = Autumn (heavy reap), Phase 3 = Winter (frost sweep).
    /// </summary>
    public sealed class FourSeasonsBladeSwing : MeleeSwingBase
    {
        public override string Texture => "MagnumOpus/Content/Seasons/Weapons/FourSeasonsBlade";

        #region Season Palettes

        private static readonly Color[] SpringPalette = new Color[]
        {
            new Color(60, 80, 40),       // [0] dark moss
            new Color(120, 180, 80),     // [1] fresh leaf
            new Color(255, 183, 197),    // [2] cherry blossom
            new Color(144, 238, 144),    // [3] spring green
            new Color(210, 255, 210),    // [4] pale bloom
            new Color(255, 250, 245)     // [5] white petal
        };

        private static readonly Color[] SummerPalette = new Color[]
        {
            new Color(100, 40, 0),       // [0] deep ember
            new Color(200, 100, 10),     // [1] amber
            new Color(255, 215, 0),      // [2] sunlight gold
            new Color(255, 180, 40),     // [3] solar flare
            new Color(255, 230, 100),    // [4] bright haze
            new Color(255, 250, 220)     // [5] white heat
        };

        private static readonly Color[] AutumnPalette = new Color[]
        {
            new Color(80, 40, 10),       // [0] dark bark
            new Color(139, 90, 43),      // [1] walnut
            new Color(255, 140, 50),     // [2] harvest orange
            new Color(220, 120, 40),     // [3] amber leaf
            new Color(255, 200, 100),    // [4] golden canopy
            new Color(255, 240, 200)     // [5] autumn mist
        };

        private static readonly Color[] WinterPalette = new Color[]
        {
            new Color(20, 40, 80),       // [0] deep midnight
            new Color(60, 120, 200),     // [1] cold cerulean
            new Color(150, 220, 255),    // [2] frost blue
            new Color(200, 240, 255),    // [3] ice crystal
            new Color(230, 248, 255),    // [4] pale frost
            new Color(240, 250, 255)     // [5] snow white
        };

        #endregion

        #region Quick Season Colors

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

        #endregion

        #region Combo Phases

        private static readonly ComboPhase[] Phases = new ComboPhase[]
        {
            // Phase 0: SPRING — gentle flowing sweep (nature's breath)
            new ComboPhase(
                new CurveSegment[]
                {
                    new CurveSegment(EasingType.SineOut, 0f, -0.8f, 0.2f, 2),
                    new CurveSegment(EasingType.SineIn, 0.2f, -0.6f, 1.4f, 2),
                    new CurveSegment(EasingType.SineOut, 0.82f, 0.8f, 0.1f, 2)
                },
                MathHelper.PiOver2 * 1.4f, 52, 155f, false, 0.92f, 0.9f),

            // Phase 1: SUMMER — blazing fast horizontal slash (solar fury)
            new ComboPhase(
                new CurveSegment[]
                {
                    new CurveSegment(EasingType.PolyOut, 0f, -1f, 0.15f, 2),
                    new CurveSegment(EasingType.PolyIn, 0.15f, -0.85f, 1.75f, 3),
                    new CurveSegment(EasingType.PolyOut, 0.82f, 0.9f, 0.1f, 2)
                },
                MathHelper.PiOver2 * 1.6f, 45, 160f, true, 0.90f, 1.0f),

            // Phase 2: AUTUMN — heavy downward reaping strike (harvest's weight)
            new ComboPhase(
                new CurveSegment[]
                {
                    new CurveSegment(EasingType.PolyOut, 0f, -1.1f, 0.3f, 2),
                    new CurveSegment(EasingType.PolyIn, 0.3f, -0.8f, 1.7f, 3),
                    new CurveSegment(EasingType.PolyOut, 0.88f, 0.9f, 0.1f, 2)
                },
                MathHelper.PiOver2 * 1.8f, 55, 165f, false, 0.88f, 1.1f),

            // Phase 3: WINTER — wide frost sweep (winter's embrace)
            new ComboPhase(
                new CurveSegment[]
                {
                    new CurveSegment(EasingType.ExpOut, 0f, -1.2f, 0.25f, 2),
                    new CurveSegment(EasingType.PolyIn, 0.25f, -0.95f, 2.0f, 3),
                    new CurveSegment(EasingType.SineOut, 0.9f, 1.05f, 0.05f, 2)
                },
                MathHelper.PiOver2 * 2.0f, 58, 170f, true, 0.85f, 1.2f)
        };

        #endregion

        private int Season => Math.Clamp(ComboStep, 0, 3);

        protected override ComboPhase[] GetAllPhases() => Phases;

        protected override Color[] GetPalette()
        {
            return Season switch
            {
                0 => SpringPalette,
                1 => SummerPalette,
                2 => AutumnPalette,
                _ => WinterPalette
            };
        }

        protected override CalamityStyleTrailRenderer.TrailStyle GetTrailStyle()
        {
            return Season switch
            {
                0 => CalamityStyleTrailRenderer.TrailStyle.Nature,
                1 => CalamityStyleTrailRenderer.TrailStyle.Flame,
                2 => CalamityStyleTrailRenderer.TrailStyle.Nature,
                _ => CalamityStyleTrailRenderer.TrailStyle.Ice
            };
        }

        protected override string GetSmearTexturePath(int comboStep)
        {
            return comboStep switch
            {
                2 => "MagnumOpus/Assets/Particles/InfernalCircularSmear",
                3 => "MagnumOpus/Assets/Particles/InfernalTrientSmear",
                _ => "MagnumOpus/Assets/Particles/InfernalSemiCircularSmear"
            };
        }

        protected override int GetInitialDustType()
        {
            return Season switch
            {
                0 => DustID.GrassBlades,
                1 => DustID.Torch,
                2 => DustID.OrangeTorch,
                _ => DustID.IceTorch
            };
        }

        protected override int GetSecondaryDustType()
        {
            return Season switch
            {
                0 => DustID.Enchanted_Pink,
                1 => DustID.Enchanted_Gold,
                2 => DustID.AmberBolt,
                _ => DustID.Frost
            };
        }

        protected override Vector3 GetLightColor()
        {
            return SeasonPrimary[Season].ToVector3() * 0.6f;
        }

        protected override SoundStyle GetSwingSound()
        {
            return Season switch
            {
                0 => SoundID.Item71 with { Pitch = 0.1f },
                1 => SoundID.Item71 with { Pitch = 0.0f },
                2 => SoundID.Item71 with { Pitch = -0.2f },
                _ => SoundID.Item71 with { Pitch = -0.4f }
            };
        }

        protected override void HandleComboSpecials()
        {
            if (hasSpawnedSpecial) return;

            // Season-specific blade-tip particles at ~75% swing progress
            if (Progression >= 0.75f)
            {
                hasSpawnedSpecial = true;
                Color primary = SeasonPrimary[Season];
                Vector2 tipPos = Owner.MountedCenter + SwordDirection * CurrentPhase.BladeLength;

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
                    ThemedParticles.MusicNote(tipPos, -SwordDirection * 2f, primary, noteScale, 30);
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
                            leafVel.Y -= 1f; // Slight upward drift
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

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int remainingDamageCount)
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
                    int stolen = Math.Min(remainingDamageCount / 10, 15);
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

        protected override void DrawCustomVFX(SpriteBatch sb)
        {
            if (Progression < 0.08f || Progression > 0.95f) return;

            Color primary = SeasonPrimary[Season];
            Color secondary = SeasonSecondary[Season];
            Vector2 tipWorld = GetBladeTipPosition();
            float pulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 5f) * 0.10f;
            float tipScale = (0.18f + ComboStep * 0.05f) * pulse;

            BloomRenderer.DrawBloomStackAdditive(tipWorld, primary, secondary, tipScale, Progression);

            if (Main.rand.NextBool(4))
            {
                float hMin = Season switch { 0 => 0.25f, 1 => 0.04f, 2 => 0.06f, _ => 0.52f };
                float hMax = Season switch { 0 => 0.42f, 1 => 0.14f, 2 => 0.12f, _ => 0.68f };
                Vector2 noteVel = (Projectile.velocity.SafeNormalize(Vector2.UnitX) * -1.5f).RotatedByRandom(0.4);
                MagnumParticleHandler.SpawnParticle(new HueShiftingMusicNoteParticle(
                    tipWorld, noteVel, hMin, hMax, 0.85f, 0.60f, 0.75f, 25, 0.025f));
            }
        }
    }
}
