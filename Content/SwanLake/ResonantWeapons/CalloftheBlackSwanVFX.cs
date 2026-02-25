using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons
{
    /// <summary>
    /// VFX helper for the Call of the Black Swan greatsword.
    /// Handles hold-item ambient, item bloom, swing-frame dust,
    /// blade-tip effects, on-hit impacts, combo specials, and finisher.
    /// The Black Swan: dual-polarity (black/white) with prismatic accents.
    /// Call from CalloftheBlackSwan and CalloftheBlackSwanSwing.
    /// </summary>
    public static class CalloftheBlackSwanVFX
    {
        // =====================================================================
        //  HOLD ITEM VFX
        // =====================================================================

        /// <summary>
        /// Per-frame held-item VFX: dual-polarity motes drifting near the blade,
        /// subtle prismatic shimmer, and occasional feather drift.
        /// </summary>
        public static void HoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Dual-polarity motes near blade edge
            if (Main.rand.NextBool(4))
            {
                Vector2 tipOffset = new Vector2(player.direction * 28f, -12f);
                Vector2 tipPos = center + tipOffset + Main.rand.NextVector2Circular(8f, 8f);
                bool isWhite = Main.rand.NextBool();
                int dustType = isWhite ? DustID.WhiteTorch : DustID.Shadowflame;
                Color col = isWhite ? SwanLakePalette.PureWhite : SwanLakePalette.ObsidianBlack;
                Dust d = Dust.NewDustPerfect(tipPos, dustType, Vector2.Zero, isWhite ? 0 : 100, col, 0.5f);
                d.noGravity = true;
            }

            // Rare prismatic sparkle near blade tip
            if (Main.rand.NextBool(10))
            {
                Vector2 sparkOffset = new Vector2(player.direction * 30f, -14f);
                Vector2 sparkPos = center + sparkOffset + Main.rand.NextVector2Circular(4f, 4f);
                Color rainbow = SwanLakePalette.GetRainbow(Main.rand.NextFloat());
                Dust r = Dust.NewDustPerfect(sparkPos, DustID.RainbowTorch,
                    new Vector2(0f, -0.3f), 0, rainbow * 0.6f, 0.4f);
                r.noGravity = true;
            }

            // Subtle ambient feather drift
            if (Main.rand.NextBool(25))
            {
                Color featherCol = Main.rand.NextBool() ? SwanLakePalette.FeatherWhite : SwanLakePalette.FeatherBlack;
                try { CustomParticles.SwanFeatherDrift(center + Main.rand.NextVector2Circular(20f, 20f), featherCol, 0.18f); } catch { }
            }

            // Pulsing dual-polarity glow
            float pulse = 0.25f + MathF.Sin(time * 0.05f) * 0.1f;
            SwanLakeVFXLibrary.AddDualPolarityLight(center, time, pulse);
        }

        // =====================================================================
        //  PREDRAW IN WORLD BLOOM
        // =====================================================================

        /// <summary>
        /// Standard 3-layer dual-polarity item bloom for the Black Swan blade sprite.
        /// </summary>
        public static void PreDrawInWorldBloom(
            Microsoft.Xna.Framework.Graphics.SpriteBatch sb,
            Microsoft.Xna.Framework.Graphics.Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.04f) * 0.03f;
            SwanLakePalette.DrawItemBloom(sb, tex, pos, origin, rotation, scale, pulse);
        }

        // =====================================================================
        //  SWING FRAME VFX
        // =====================================================================

        /// <summary>
        /// Per-frame blade swing VFX: dual-polarity dust trail, rainbow shimmer,
        /// periodic feather drifts, and music notes along the arc.
        /// Call from CalloftheBlackSwanSwing.DrawCustomVFX.
        /// </summary>
        public static void SwingFrameVFX(Vector2 tipPos, Vector2 swordDirection,
            int comboStep, int timer)
        {
            if (Main.dedServ) return;

            // Dense dual-polarity dust trail
            SwanLakeVFXLibrary.SpawnDualPolarityDust(tipPos, -swordDirection);

            // Rainbow shimmer along blade edge (1-in-2)
            SwanLakeVFXLibrary.SpawnRainbowShimmer(tipPos, -swordDirection);

            // Periodic feather drift (every 8 frames)
            if (timer % 8 == 0)
            {
                Color featherCol = Main.rand.NextBool() ? SwanLakePalette.FeatherWhite : SwanLakePalette.FeatherBlack;
                try { CustomParticles.SwanFeatherDrift(tipPos + Main.rand.NextVector2Circular(10f, 10f), featherCol, 0.25f); } catch { }
            }

            // Music notes (every 5 frames) — rainbow hue-shifting
            if (timer % 5 == 0)
                SwanLakeVFXLibrary.SpawnMusicNotes(tipPos, 1, 10f, 0.7f, 0.9f, 25);

            // Bloom at blade tip
            float bloomOpacity = 0.5f + comboStep * 0.15f;
            SwanLakeVFXLibrary.DrawBloom(tipPos, 0.3f + comboStep * 0.06f, bloomOpacity);

            SwanLakeVFXLibrary.AddPaletteLighting(tipPos, 0.4f + comboStep * 0.15f, 0.6f);
        }

        // =====================================================================
        //  COMBO SPECIAL: FEATHER SCATTER (Phase 1, 60%)
        // =====================================================================

        /// <summary>
        /// Phase 1 combo special: dual-polarity feather burst with prismatic sparkles
        /// erupting from the blade tip mid-swing.
        /// </summary>
        public static void ComboFeatherScatter(Vector2 tipPos)
        {
            if (Main.dedServ) return;

            // Black/white feather burst
            SwanLakeVFXLibrary.SpawnFeatherDuality(tipPos, 3, 0.3f);

            // Monochrome dust burst
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                bool isWhite = i % 2 == 0;
                int dustType = isWhite ? DustID.WhiteTorch : DustID.Shadowflame;
                Color col = isWhite ? SwanLakePalette.PureWhite : SwanLakePalette.ObsidianBlack;
                Dust d = Dust.NewDustPerfect(tipPos, dustType, vel, isWhite ? 0 : 100, col, 1.6f);
                d.noGravity = true;
            }

            SwanLakeVFXLibrary.SpawnMusicNotes(tipPos, 2, 18f);
        }

        // =====================================================================
        //  COMBO SPECIAL: FLARE RELEASE (Phase 2, 70%)
        // =====================================================================

        /// <summary>
        /// Phase 2 combo special: massive prismatic burst when sub-projectiles launch.
        /// Dual halo rings (black + white), rainbow flare ring, and music note scatter.
        /// </summary>
        public static void ComboFlareRelease(Vector2 tipPos)
        {
            if (Main.dedServ) return;

            // Central impact
            try { UnifiedVFX.SwanLake.Impact(tipPos, 1.0f); } catch { }
            SwanLakeVFXLibrary.SpawnRainbowExplosion(tipPos, 0.8f);

            // Rainbow flare ring
            SwanLakeVFXLibrary.SpawnPrismaticSparkles(tipPos, 6, 15f);

            // Dual-polarity halo rings
            try { CustomParticles.HaloRing(tipPos, SwanLakePalette.PureWhite, 0.4f, 15); } catch { }
            try { CustomParticles.HaloRing(tipPos, SwanLakePalette.ObsidianBlack, 0.3f, 12); } catch { }

            SwanLakeVFXLibrary.SpawnMusicNotes(tipPos, 4, 25f);
        }

        // =====================================================================
        //  ON HIT VFX
        // =====================================================================

        /// <summary>
        /// On-hit impact VFX: dual-polarity bloom, monochrome dust burst,
        /// rainbow flares, feather scatter, and combo-scaling music notes.
        /// </summary>
        public static void OnHitVFX(Vector2 hitPos, int comboStep, bool isCrit)
        {
            if (Main.dedServ) return;

            // Core impact
            SwanLakeVFXLibrary.MeleeImpact(hitPos, comboStep);

            // Swan Lake themed impact
            try { UnifiedVFX.SwanLake.Impact(hitPos, 1.2f + comboStep * 0.2f); } catch { }
            SwanLakeVFXLibrary.SpawnRainbowExplosion(hitPos, 0.9f);

            // Music accidentals
            try { ThemedParticles.SwanLakeAccidentals(hitPos, 2 + comboStep, 20f); } catch { }

            // Crit: massive rainbow + prismatic swirl
            if (isCrit)
            {
                SwanLakeVFXLibrary.SpawnRainbowExplosion(hitPos, 1.8f);
                SwanLakeVFXLibrary.SpawnPrismaticSparkles(hitPos, 10, 20f);
                SwanLakeVFXLibrary.SpawnPrismaticSwirl(hitPos, 6, 40f);
                SwanLakeVFXLibrary.SpawnFeatherBurst(hitPos, 6, 0.35f);
            }

            Lighting.AddLight(hitPos, 1.2f, 1.2f, 1.5f);
        }

        // =====================================================================
        //  EMPOWERMENT VFX
        // =====================================================================

        /// <summary>
        /// VFX feedback when empowerment is gained: prismatic burst with halo rings.
        /// </summary>
        public static void EmpowermentGainedVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            try { UnifiedVFX.SwanLake.Impact(pos, 0.8f); } catch { }
            SwanLakeVFXLibrary.SpawnRainbowExplosion(pos, 0.6f);
            SwanLakeVFXLibrary.SpawnPrismaticSparkles(pos, 6, 20f);

            try { CustomParticles.HaloRing(pos, SwanLakePalette.PureWhite, 0.35f, 14); } catch { }
            try { CustomParticles.HaloRing(pos, SwanLakePalette.ObsidianBlack, 0.25f, 12); } catch { }

            SwanLakeVFXLibrary.SpawnMusicNotes(pos, 3, 20f);
        }

        // =====================================================================
        //  PROJECTILE (BLACK SWAN FLARE) VFX
        // =====================================================================

        /// <summary>
        /// Per-frame trail VFX for BlackSwanFlare sub-projectiles.
        /// Dual-polarity blazing trail with rainbow shimmer.
        /// </summary>
        public static void FlareTrailVFX(Vector2 pos, Vector2 velocity, int flareType)
        {
            if (Main.dedServ) return;

            Vector2 away = -velocity.SafeNormalize(Vector2.Zero);

            // Monochrome blazing trail (2 per frame)
            for (int i = 0; i < 2; i++)
            {
                bool isWhite = flareType == 0 ? (i == 0) : (i == 1);
                int dustType = isWhite ? DustID.WhiteTorch : DustID.Shadowflame;
                Color col = isWhite ? SwanLakePalette.PureWhite : SwanLakePalette.ObsidianBlack;
                Vector2 vel = away * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(0.5f, 0.5f);
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, isWhite ? 0 : 100, col, 1.4f);
                d.noGravity = true;
            }

            // Rainbow shimmer (1-in-3)
            if (Main.rand.NextBool(3))
            {
                Color rainbow = SwanLakePalette.GetRainbow(Main.rand.NextFloat());
                Dust r = Dust.NewDustPerfect(pos, DustID.RainbowTorch,
                    away * 0.5f + Main.rand.NextVector2Circular(1f, 1f), 0, rainbow, 1.0f);
                r.noGravity = true;
            }

            // Music notes (1-in-6)
            if (Main.rand.NextBool(6))
                SwanLakeVFXLibrary.SpawnMusicNotes(pos, 1, 8f, 0.7f, 0.85f, 22);

            SwanLakeVFXLibrary.AddSwanLight(pos, 0.4f);
        }

        /// <summary>
        /// Impact VFX when a BlackSwanFlare sub-projectile hits.
        /// </summary>
        public static void FlareImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            SwanLakeVFXLibrary.ProjectileImpact(pos, 0.7f);
        }
    }
}
