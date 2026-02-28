using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Nachtmusik.ResonantWeapons
{
    /// <summary>
    /// Shader-driven VFX for Twilight Severance — the ultra-fast dimensional katana.
    /// Uses DimensionalRift.fx for razor-sharp dimensional tear trails.
    /// Every cut severs the boundary between day and night.
    /// </summary>
    public static class TwilightSeveranceVFX
    {
        // =====================================================================
        //  HoldItemVFX — Twilight shimmer oscillating dusk↔dawn
        // =====================================================================
        public static void HoldItemVFX(Player player)
        {
            if (Main.rand.NextBool(4))
            {
                // Oscillating dusk↔dawn shimmer particles
                float oscillation = (float)Math.Sin(Main.timeForVisualEffects * 0.08) * 0.5f + 0.5f;
                Color shimmerColor = Color.Lerp(NachtmusikPalette.DuskViolet, NachtmusikPalette.MoonlitSilver, oscillation);

                Vector2 offset = Main.rand.NextVector2Circular(25f, 25f);
                Vector2 vel = Main.rand.NextVector2Circular(0.3f, 0.3f);

                Dust d = Dust.NewDustPerfect(player.Center + offset, DustID.PurpleTorch, vel, 0, default, 0.5f);
                d.noGravity = true;
                d.fadeIn = 0.7f;
            }

            if (Main.rand.NextBool(10))
            {
                // Silver edge mote — the blade's dimensional echo
                Vector2 edgeOffset = Main.rand.NextVector2CircularEdge(20f, 20f);
                Dust silver = Dust.NewDustPerfect(player.Center + edgeOffset, DustID.SilverFlame,
                    edgeOffset * 0.02f, 0, default, 0.4f);
                silver.noGravity = true;
            }

            NachtmusikVFXLibrary.AddNachtmusikLight(player.Center, 0.2f);
        }

        // =====================================================================
        //  PreDrawInWorldBloom — Shader-driven dimensional glow
        // =====================================================================
        public static void PreDrawInWorldBloom(SpriteBatch sb, Texture2D tex, Vector2 pos,
            Vector2 origin, float rotation, float scale)
        {
            if (NachtmusikShaderManager.HasDimensionalRift)
            {
                NachtmusikShaderManager.BeginShaderAdditive(sb);
                NachtmusikShaderManager.ApplyDimensionalRiftGlow((float)Main.timeForVisualEffects * 0.03f);

                sb.Draw(tex, pos - Main.screenPosition, null,
                    NachtmusikPalette.MoonlitSilver * 0.5f, rotation, origin, scale * 1.04f,
                    SpriteEffects.None, 0f);

                NachtmusikShaderManager.RestoreSpriteBatch(sb);
            }
            else
            {
                NachtmusikVFXLibrary.DrawNachtmusikBloomStack(sb, pos,
                    NachtmusikPalette.DuskViolet, NachtmusikPalette.MoonlitSilver, scale * 0.3f, 0.4f);
            }
        }

        // =====================================================================
        //  SwingTrailVFX — Dimensional tear dust at blade edge
        // =====================================================================
        public static void SwingTrailVFX(Vector2 tipPos, Vector2 swordDirection, int comboStep, int timer)
        {
            // Ultra-fast katana: every frame gets dust
            Vector2 perpendicular = new Vector2(-swordDirection.Y, swordDirection.X);

            // Dusk violet cut line
            Vector2 dustVel = perpendicular * Main.rand.NextFloat(-1.5f, 1.5f) + swordDirection * 1f;
            Dust d = Dust.NewDustPerfect(tipPos, DustID.PurpleTorch, dustVel, 0, default, 0.9f);
            d.noGravity = true;
            d.fadeIn = 1f;

            // Silver edge sparkle — the dimensional boundary
            if (timer % 2 == 0)
            {
                Vector2 silverVel = swordDirection * 2f + Main.rand.NextVector2Circular(1f, 1f);
                Dust silver = Dust.NewDustPerfect(tipPos, DustID.SilverFlame, silverVel, 0, default, 0.6f);
                silver.noGravity = true;
            }

            // Quick star accents
            if (timer % 3 == 0)
            {
                NachtmusikVFXLibrary.SpawnTwinklingStars(tipPos, 1, 6f);
            }

            // Music notes at high combo only
            if (comboStep >= 2 && timer % 5 == 0)
            {
                NachtmusikVFXLibrary.SpawnMusicNotes(tipPos, 1, 10f, 0.4f, 0.6f, 20);
            }

            NachtmusikVFXLibrary.AddPaletteLighting(tipPos, 0.5f, 0.4f);
        }

        // =====================================================================
        //  SwingImpactVFX — Split-polarity dimensional impact
        // =====================================================================
        public static void SwingImpactVFX(Vector2 hitPos, int comboStep = 0)
        {
            float intensity = 1f + comboStep * 0.2f;

            NachtmusikVFXLibrary.MeleeImpact(hitPos, comboStep);

            // Split dust burst — half dusk, half silver (the dimensional tear)
            int dustCount = 10 + comboStep * 3;
            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                Vector2 vel = angle.ToRotationVector2() * (4f + Main.rand.NextFloat() * 2f) * intensity;

                // Alternate between dusk and silver
                int dustType = (i % 2 == 0) ? DustID.PurpleTorch : DustID.SilverFlame;
                Dust d = Dust.NewDustPerfect(hitPos, dustType, vel, 0, default, 1f * intensity);
                d.noGravity = true;
                d.fadeIn = 1.1f;
            }

            // Nebula pink accent sparkles at higher combo
            if (comboStep >= 1)
            {
                NachtmusikVFXLibrary.SpawnStarBurst(hitPos, 4 + comboStep * 2, 0.35f);
            }

            // Silver dimensional ring
            if (comboStep >= 2)
            {
                NachtmusikVFXLibrary.SpawnGradientHaloRings(hitPos, 3, 0.3f * intensity);
            }

            NachtmusikVFXLibrary.SpawnMusicNotes(hitPos, 1 + comboStep, 15f, 0.5f, 0.8f, 25);
            NachtmusikVFXLibrary.DrawBloom(hitPos, 0.35f * intensity, 0.7f);
            NachtmusikVFXLibrary.AddPaletteLighting(hitPos, 0.5f, 0.6f * intensity);
        }

        // =====================================================================
        //  FinisherVFX — Dimensional severance: dual-polarity cascade
        // =====================================================================
        public static void FinisherVFX(Vector2 pos, float intensity = 1f)
        {
            // Alternating dusk/silver spark waves
            NachtmusikVFXLibrary.SpawnStarburstCascade(pos, 4, intensity, 1f);

            // Shattered dimensional fragments
            NachtmusikVFXLibrary.SpawnShatteredStarlight(pos, 8, 6f * intensity, 0.7f, false);

            // Nebula pink accent cascade
            NachtmusikVFXLibrary.SpawnRadianceBurst(pos, 10, 5f * intensity);

            // Silver dimensional halo rings
            NachtmusikVFXLibrary.SpawnGradientHaloRings(pos, 5, 0.35f * intensity);

            // Music notes
            NachtmusikVFXLibrary.SpawnMusicNotes(pos, 5, 30f, 0.6f, 1f, 35);

            // Dual-polarity dust wave
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 vel = angle.ToRotationVector2() * (5f * intensity + Main.rand.NextFloat() * 2f);

                int dustType = (i % 2 == 0) ? DustID.PurpleTorch : DustID.SilverFlame;
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, 0, default, 1.2f * intensity);
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }

            NachtmusikVFXLibrary.DrawComboBloom(pos, 2, 0.45f * intensity, 0.9f);
            NachtmusikVFXLibrary.AddPaletteLighting(pos, 0.4f, 1f * intensity);
        }
    }
}
