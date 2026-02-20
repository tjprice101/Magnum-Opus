using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Content.SwanLake.Debuffs;
using static MagnumOpus.Common.Systems.Particles.Particle;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons
{
    public sealed class CalloftheBlackSwanSwing : MeleeSwingBase
    {
        #region Theme Colors — Monochrome + Rainbow Shimmer

        private static readonly Color SwanBlack = MagnumThemePalettes.SwanBlack;
        private static readonly Color SwanDarkGray = MagnumThemePalettes.SwanDarkGray;
        private static readonly Color SwanMidGray = MagnumThemePalettes.SwanMidGray;
        private static readonly Color SwanLightGray = MagnumThemePalettes.SwanLightGray;
        private static readonly Color SwanSilver = MagnumThemePalettes.SwanSilver;
        private static readonly Color SwanWhite = MagnumThemePalettes.SwanWhite;

        private static readonly Color[] SwanPalette = new Color[]
        {
            SwanBlack,
            SwanDarkGray,
            SwanMidGray,
            SwanLightGray,
            SwanSilver,
            SwanWhite
        };

        /// <summary>Get rainbow color cycling for prismatic accents.</summary>
        private static Color GetRainbow(float offset = 0f)
        {
            float hue = (Main.GameUpdateCount * 0.012f + offset) % 1f;
            return Main.hslToRgb(hue, 0.85f, 0.8f);
        }

        #endregion

        #region Combo Phases — Ballet Movements

        // Phase 0: Graceful Plié — light, fast opener with gentle arc
        private static readonly ComboPhase Phase0_Plie = new ComboPhase(
            new CurveSegment[]
            {
                new CurveSegment(EasingType.SineOut, 0f, -0.85f, 0.2f, 2),
                new CurveSegment(EasingType.PolyIn, 0.18f, -0.65f, 1.5f, 3),
                new CurveSegment(EasingType.SineOut, 0.78f, 0.85f, 0.15f, 2),
            },
            maxAngle: MathHelper.PiOver2 * 1.5f,
            duration: 20,
            bladeLength: 155f,
            flip: false,
            squish: 0.90f,
            damageMult: 0.85f
        );

        // Phase 1: Arabesque Sweep — wide, flowing arc in opposite direction
        private static readonly ComboPhase Phase1_Arabesque = new ComboPhase(
            new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -1f, 0.25f, 2),
                new CurveSegment(EasingType.PolyIn, 0.22f, -0.75f, 1.65f, 3),
                new CurveSegment(EasingType.SineOut, 0.82f, 0.9f, 0.1f, 2),
            },
            maxAngle: MathHelper.PiOver2 * 1.7f,
            duration: 24,
            bladeLength: 160f,
            flip: true,
            squish: 0.86f,
            damageMult: 1.0f
        );

        // Phase 2: Grand Jeté — massive leap strike, finisher
        private static readonly ComboPhase Phase2_GrandJete = new ComboPhase(
            new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -1.1f, 0.3f, 2),
                new CurveSegment(EasingType.PolyIn, 0.25f, -0.8f, 1.9f, 3),
                new CurveSegment(EasingType.PolyOut, 0.85f, 1.1f, -0.1f, 2),
            },
            maxAngle: MathHelper.PiOver2 * 2.1f,
            duration: 28,
            bladeLength: 175f,
            flip: false,
            squish: 0.82f,
            damageMult: 1.4f
        );

        #endregion

        #region Abstract Overrides

        protected override ComboPhase[] GetAllPhases() => new[]
        {
            Phase0_Plie,
            Phase1_Arabesque,
            Phase2_GrandJete
        };

        protected override Color[] GetPalette() => SwanPalette;

        protected override CalamityStyleTrailRenderer.TrailStyle GetTrailStyle()
            => CalamityStyleTrailRenderer.TrailStyle.Ice;

        protected override string GetSmearTexturePath(int comboStep)
        {
            return comboStep switch
            {
                2 => "MagnumOpus/Assets/Particles/CurvedSwordSlash",
                _ => "MagnumOpus/Assets/Particles/SwordArc2"
            };
        }

        #endregion

        #region Virtual Overrides

        protected override Texture2D GetBladeTexture()
            => Terraria.GameContent.TextureAssets.Item[ItemID.BreakerBlade].Value;

        protected override SoundStyle GetSwingSound()
            => SoundID.Item29 with { Pitch = -0.1f + ComboStep * 0.1f, Volume = 0.75f };

        protected override int GetInitialDustType() => DustID.WhiteTorch;

        protected override int GetSecondaryDustType() => DustID.Shadowflame;

        protected override Vector3 GetLightColor()
        {
            float intensity = 0.4f + ComboStep * 0.15f;
            Color rainbow = GetRainbow(ComboStep * 0.33f);
            return rainbow.ToVector3() * intensity;
        }

        #endregion

        #region Combo Specials

        protected override void HandleComboSpecials()
        {
            if (hasSpawnedSpecial) return;

            // Phase 1 at 60%: Feather scatter + prismatic sparkles
            if (ComboStep == 1 && Progression >= 0.60f)
            {
                hasSpawnedSpecial = true;
                Vector2 tipPos = GetBladeTipPosition();

                // Black/white feather burst from blade tip
                for (int i = 0; i < 4; i++)
                {
                    float angle = MathHelper.TwoPi * i / 4f;
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                    Color col = i % 2 == 0 ? SwanWhite : SwanBlack;
                    int dustType = i % 2 == 0 ? DustID.WhiteTorch : DustID.Shadowflame;
                    Dust d = Dust.NewDustPerfect(tipPos, dustType, vel, i % 2 == 0 ? 0 : 100, col, 1.6f);
                    d.noGravity = true;
                }

                try { CustomParticles.SwanFeatherDuality(tipPos, 3, 0.3f); } catch { }
                try { ThemedParticles.SwanLakeMusicNotes(tipPos, 2, 18f); } catch { }
            }

            // Phase 2 at 70%: Spawn BlackSwanFlare sub-projectiles
            if (ComboStep == 2 && Progression >= 0.70f)
            {
                hasSpawnedSpecial = true;
                Vector2 tipPos = GetBladeTipPosition();

                if (Main.myPlayer == Projectile.owner)
                {
                    bool empowered = CalloftheBlackSwan.IsEmpowered(Projectile.owner);
                    int flareCount = empowered ? 8 : 3;
                    int flareDamage = empowered ? Projectile.damage : Projectile.damage / 2;

                    if (empowered) CalloftheBlackSwan.ConsumeEmpowerment(Projectile.owner);
                    else CalloftheBlackSwan.ResetFlareCount(Projectile.owner);

                    float spread = MathHelper.ToRadians(empowered ? 50f : 30f);
                    for (int i = 0; i < flareCount; i++)
                    {
                        float angle = MathHelper.Lerp(-spread, spread, flareCount > 1 ? i / (float)(flareCount - 1) : 0.5f);
                        Vector2 vel = SwordRotation.ToRotationVector2().RotatedBy(angle) * Main.rand.NextFloat(12f, 16f);
                        int flareType = i % 2;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), tipPos, vel,
                            ModContent.ProjectileType<BlackSwanFlare>(),
                            flareDamage, 2f, Projectile.owner, flareType, empowered ? 1 : 0);
                    }
                }

                // Release VFX
                try { UnifiedVFX.SwanLake.Impact(tipPos, 1.0f); } catch { }
                try { ThemedParticles.SwanLakeRainbowExplosion(tipPos, 0.8f); } catch { }
                try { ThemedParticles.SwanLakeMusicNotes(tipPos, 4, 25f); } catch { }

                for (int i = 0; i < 6; i++)
                {
                    float hue = i / 6f;
                    Color sparkColor = Main.hslToRgb(hue, 1f, 0.75f);
                    try { CustomParticles.GenericFlare(tipPos + Main.rand.NextVector2Circular(10f, 10f), sparkColor, 0.5f, 18); } catch { }
                }

                try { CustomParticles.HaloRing(tipPos, SwanWhite, 0.4f, 15); } catch { }
                try { CustomParticles.HaloRing(tipPos, SwanBlack, 0.3f, 12); } catch { }
            }
        }

        #endregion

        #region On Hit

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<FlameOfTheSwan>(), 360);

            Vector2 hitPos = target.Center;

            // Monochrome → rainbow impact
            try { UnifiedVFX.SwanLake.Impact(hitPos, 1.2f + ComboStep * 0.2f); } catch { }
            try { ThemedParticles.SwanLakeRainbowExplosion(hitPos, 0.9f); } catch { }

            // Black/white dust burst
            int dustCount = 10 + ComboStep * 4;
            for (int i = 0; i < dustCount; i++)
            {
                Color col = i % 2 == 0 ? SwanWhite : SwanBlack;
                int dustType = i % 2 == 0 ? DustID.WhiteTorch : DustID.Shadowflame;
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                Dust d = Dust.NewDustPerfect(hitPos, dustType, vel, i % 2 == 0 ? 0 : 100, col, 1.6f);
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }

            // Music notes
            try { ThemedParticles.SwanLakeMusicNotes(hitPos, 4 + ComboStep * 2, 30f); } catch { }
            try { ThemedParticles.SwanLakeAccidentals(hitPos, 2 + ComboStep, 20f); } catch { }

            // Gradient halo rings
            try { CustomParticles.HaloRing(hitPos, SwanWhite, 0.5f, 18); } catch { }
            try { CustomParticles.HaloRing(hitPos, SwanBlack, 0.35f, 15); } catch { }

            // Rainbow flare ring
            for (int i = 0; i < 4 + ComboStep * 2; i++)
            {
                float hue = (float)i / (4 + ComboStep * 2);
                Color flareColor = Main.hslToRgb(hue, 1f, 0.75f);
                try { CustomParticles.GenericFlare(hitPos + Main.rand.NextVector2Circular(15f, 15f), flareColor, 0.45f, 18); } catch { }
            }

            // Swan feather burst
            try { CustomParticles.SwanFeatherBurst(hitPos, 3 + ComboStep, 0.3f); } catch { }

            // Crit: massive rainbow + seeking crystals
            if (hit.Crit)
            {
                try { ThemedParticles.SwanLakeRainbowExplosion(hitPos, 1.8f); } catch { }

                for (int i = 0; i < 10; i++)
                {
                    float hue = i / 10f;
                    Color sparkColor = Main.hslToRgb(hue, 1f, 0.8f);
                    try { CustomParticles.GenericFlare(hitPos + Main.rand.NextVector2Circular(20f, 20f), sparkColor, 0.7f, 25); } catch { }
                }

                if (Main.myPlayer == Projectile.owner)
                {
                    try
                    {
                        SeekingCrystalHelper.SpawnSwanLakeCrystals(
                            Projectile.GetSource_FromThis(), hitPos,
                            (Main.MouseWorld - hitPos).SafeNormalize(Vector2.UnitX) * 8f,
                            Projectile.damage / 3, 2f, Projectile.owner, 5);
                    }
                    catch { }
                }
            }

            Lighting.AddLight(hitPos, 1.2f, 1.2f, 1.5f);
        }

        #endregion

        #region Custom VFX

        protected override void DrawCustomVFX(SpriteBatch sb)
        {
            if (Progression <= 0.08f || Progression >= 0.92f) return;

            Vector2 tipPos = GetBladeTipPosition();

            // Black/white dust trail along blade
            for (int i = 0; i < 2; i++)
            {
                float t = Main.rand.NextFloat(0.4f, 1f);
                Vector2 dustPos = Vector2.Lerp(Owner.MountedCenter, tipPos, t);
                bool isWhite = Main.rand.NextBool();
                int dustType = isWhite ? DustID.WhiteTorch : DustID.Shadowflame;
                Color dustCol = isWhite ? SwanWhite : SwanBlack;
                Dust d = Dust.NewDustPerfect(dustPos, dustType,
                    -SwordDirection * Main.rand.NextFloat(1f, 3f),
                    isWhite ? 0 : 100, dustCol, 1.5f);
                d.noGravity = true;
            }

            // Rainbow shimmer along blade edge
            if (Main.rand.NextBool(4))
            {
                float t = Main.rand.NextFloat(0.5f, 0.95f);
                Vector2 shimmerPos = Vector2.Lerp(Owner.MountedCenter, tipPos, t);
                Color rainbow = GetRainbow(t);
                Dust r = Dust.NewDustPerfect(shimmerPos, DustID.RainbowTorch,
                    -SwordDirection * Main.rand.NextFloat(0.5f, 2f), 0, rainbow, 1.2f);
                r.noGravity = true;
            }

            // Feather drift accents
            if (Main.rand.NextBool(8))
            {
                Color featherCol = Main.rand.NextBool() ? SwanWhite : SwanBlack;
                try { CustomParticles.SwanFeatherDrift(tipPos + Main.rand.NextVector2Circular(10f, 10f), featherCol, 0.25f); } catch { }
            }

            // Music notes — rainbow spectrum (hue-shifting)
            if (Main.rand.NextBool(6))
            {
                Vector2 noteVel = -SwordDirection * 0.5f + Main.rand.NextVector2Circular(0.3f, 0.3f);
                MagnumParticleHandler.SpawnParticle(new HueShiftingMusicNoteParticle(
                    tipPos, noteVel,
                    hueMin: 0.0f, hueMax: 1.0f,
                    saturation: 0.80f, luminosity: 0.85f,
                    scale: 0.80f, lifetime: 30, hueSpeed: 0.03f));
            }

            // Blade-tip bloom — monochrome shimmer
            {
                float bloomOpacity = MathHelper.Clamp((Progression - 0.08f) / 0.12f, 0f, 1f)
                                   * MathHelper.Clamp((0.92f - Progression) / 0.12f, 0f, 1f);
                float bloomScale = 0.4f + ComboStep * 0.07f;
                BloomRenderer.DrawBloomStackAdditive(tipPos, SwanBlack, SwanWhite, bloomScale, bloomOpacity);
            }
        }

        #endregion
    }
}
