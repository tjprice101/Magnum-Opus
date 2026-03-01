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
    /// Shader-driven VFX for Midnight's Crescendo — the building musical intensity blade.
    /// Uses CrescendoRise.fx for trails that intensify with crescendo stacks.
    /// Each swing crescendos from deep blue to starlit brilliance.
    /// </summary>
    public static class MidnightsCrescendoVFX
    {
        // =====================================================================
        //  HoldItemVFX — Pulsing crescendo aura that builds
        // =====================================================================
        public static void HoldItemVFX(Player player)
        {
            float time = (float)Main.timeForVisualEffects * 0.05f;

            // === ASCENDING MUSICAL STAIRCASE === Dust spawns in rising step positions
            if (Main.rand.NextBool(3))
            {
                int step = (int)(time * 2f) % 5;
                float xOffset = -16f + step * 8f;
                float yOffset = 15f - step * 8f; // Ascending
                Vector2 pos = player.Center + new Vector2(xOffset, yOffset);
                Vector2 vel = new Vector2(0.2f, -1.2f - step * 0.3f);
                Dust d = Dust.NewDustPerfect(pos, DustID.BlueTorch, vel, 0, default, 0.55f + step * 0.08f);
                d.noGravity = true;
                d.fadeIn = 0.8f;
            }

            // === INTENSITY PULSING === Bloom-like dust that pulses with musical rhythm
            if (Main.rand.NextBool(5))
            {
                float pulse = (float)Math.Sin(time * 3.5f) * 0.5f + 0.5f;
                float radius = 18f + pulse * 12f;
                Vector2 offset = Main.rand.NextVector2CircularEdge(radius, radius * 0.6f);
                Vector2 vel = new Vector2(0, -0.6f * (1f + pulse));
                Dust d = Dust.NewDustPerfect(player.Center + offset, DustID.BlueTorch,
                    vel, 0, default, 0.4f + pulse * 0.3f);
                d.noGravity = true;
                d.fadeIn = 0.7f;
            }

            // === RISING STAR FOUNTAIN === Bright ascending sparks from below
            if (Main.rand.NextBool(8))
            {
                Vector2 sparkPos = player.Center + new Vector2(Main.rand.NextFloat(-10f, 10f), 10f);
                NachtmusikVFXLibrary.SpawnTwinklingStars(sparkPos, 1, 15f);
            }

            NachtmusikVFXLibrary.AddNachtmusikLight(player.Center, 0.28f);
        }

        // =====================================================================
        //  PreDrawInWorldBloom — Shader-driven crescendo glow
        // =====================================================================
        public static void PreDrawInWorldBloom(SpriteBatch sb, Texture2D tex, Vector2 pos,
            Vector2 origin, float rotation, float scale)
        {
            if (NachtmusikShaderManager.HasCrescendoRise)
            {
                NachtmusikShaderManager.BeginShaderAdditive(sb);
                NachtmusikShaderManager.ApplyCrescendoRiseGlow(
                    (float)Main.timeForVisualEffects * 0.02f, 0.5f);

                sb.Draw(tex, pos - Main.screenPosition, null,
                    NachtmusikPalette.StarlitBlue * 0.5f, rotation, origin, scale * 1.05f,
                    SpriteEffects.None, 0f);

                NachtmusikShaderManager.RestoreSpriteBatch(sb);
            }
            else
            {
                NachtmusikVFXLibrary.DrawNachtmusikBloomStack(sb, pos,
                    NachtmusikPalette.DeepBlue, NachtmusikPalette.StarWhite, scale * 0.35f, 0.4f);
            }
        }

        // =====================================================================
        //  SwingTrailVFX — Crescendo dust that brightens with combo
        // =====================================================================
        public static void SwingTrailVFX(Vector2 tipPos, Vector2 swordDirection, int comboStep, int timer)
        {
            float comboIntensity = 1f + comboStep * 0.25f;

            // Ascending dust trail — always biased upward
            if (timer % 2 == 0)
            {
                Vector2 perpendicular = new Vector2(-swordDirection.Y, swordDirection.X);
                Vector2 dustVel = perpendicular * Main.rand.NextFloat(-1f, 1f)
                    + new Vector2(0, -1.5f * comboIntensity); // Ascending bias

                Color dustColor = Color.Lerp(NachtmusikPalette.DeepBlue, NachtmusikPalette.StarWhite,
                    comboStep / 3f);

                Dust d = Dust.NewDustPerfect(tipPos, DustID.BlueTorch, dustVel, 0, default,
                    0.8f * comboIntensity);
                d.noGravity = true;
                d.fadeIn = 1f;
            }

            // Crescendo sparkle accents — frequency increases with combo
            int sparkInterval = Math.Max(2, 6 - comboStep);
            if (timer % sparkInterval == 0)
            {
                NachtmusikVFXLibrary.SpawnTwinklingStars(tipPos, 1, 8f);
            }

            // Music notes that build with crescendo
            if (timer % Math.Max(3, 8 - comboStep * 2) == 0)
            {
                NachtmusikVFXLibrary.SpawnMusicNotes(tipPos, 1, 12f, 0.4f, 0.7f * comboIntensity, 25);
            }

            NachtmusikVFXLibrary.AddPaletteLighting(tipPos, 0.3f + comboStep * 0.1f,
                0.4f + comboStep * 0.15f);
        }

        // =====================================================================
        //  SwingImpactVFX — Growing intensity impact
        // =====================================================================
        public static void SwingImpactVFX(Vector2 hitPos, int comboStep = 0)
        {
            float comboIntensity = 1f + comboStep * 0.3f;

            NachtmusikVFXLibrary.MeleeImpact(hitPos, comboStep);

            // Ascending star spark burst — sparks fly upward
            int sparkCount = 6 + comboStep * 3;
            for (int i = 0; i < sparkCount; i++)
            {
                float angle = MathHelper.TwoPi * i / sparkCount;
                Vector2 vel = angle.ToRotationVector2() * (3f + Main.rand.NextFloat() * 2f);
                vel.Y -= 2f * comboIntensity; // Ascending bias

                Dust d = Dust.NewDustPerfect(hitPos, DustID.BlueTorch, vel, 0, default,
                    1f * comboIntensity);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // Halo rings that grow with combo
            if (comboStep >= 1)
            {
                NachtmusikVFXLibrary.SpawnGradientHaloRings(hitPos, 2 + comboStep, 0.25f * comboIntensity);
            }

            NachtmusikVFXLibrary.SpawnMusicNotes(hitPos, 1 + comboStep, 18f, 0.5f, 0.9f, 28);
            NachtmusikVFXLibrary.DrawBloom(hitPos, 0.3f * comboIntensity, 0.7f);
            NachtmusikVFXLibrary.AddPaletteLighting(hitPos, 0.4f, 0.7f * comboIntensity);
        }

        // =====================================================================
        //  FinisherVFX — Crescendo climax: ascending starburst fountain
        // =====================================================================
        public static void FinisherVFX(Vector2 pos, float intensity = 1f)
        {
            // Ascending starburst fountain — the crescendo peaks
            NachtmusikVFXLibrary.SpawnStarburstCascade(pos, 6, intensity, 1f);

            // Upward shattered starlight spray
            NachtmusikVFXLibrary.SpawnShatteredStarlight(pos, 8, 6f * intensity, 0.7f, false);

            // Radiance halo rings expanding outward
            NachtmusikVFXLibrary.SpawnRadianceHaloRings(pos, 6, 0.4f * intensity);

            // Grand music note cascade — the musical climax
            NachtmusikVFXLibrary.SpawnMusicNotes(pos, 8, 35f, 0.6f, 1.1f, 45);

            // Ascending dust fountain
            for (int i = 0; i < 15; i++)
            {
                Vector2 vel = new Vector2(Main.rand.NextFloat(-3f, 3f), -6f * intensity + Main.rand.NextFloat(-2f, 0f));
                Dust d = Dust.NewDustPerfect(pos, DustID.BlueTorch, vel, 0, default, 1.3f * intensity);
                d.noGravity = true;
                d.fadeIn = 1.5f;
            }

            NachtmusikVFXLibrary.DrawComboBloom(pos, 2, 0.5f * intensity, 1f);
            NachtmusikVFXLibrary.AddPaletteLighting(pos, 0.2f, 1f * intensity);
        }
    }
}
