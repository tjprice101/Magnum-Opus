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
    /// Shader-driven VFX for the Nocturnal Executioner — the heavy midnight authority blade.
    /// Uses ExecutionDecree.fx for void-rip slash trails and NachtmusikSerenade.fx for ambient aura.
    /// Every swing is a decree written in cosmic void and violet lightning.
    /// </summary>
    public static class NocturnalExecutionerVFX
    {
        // =====================================================================
        //  HoldItemVFX — Ambient authority aura while holding
        // =====================================================================
        public static void HoldItemVFX(Player player)
        {
            if (Main.rand.NextBool(3))
            {
                // Dark cosmic motes drift around the blade — void authority
                Vector2 offset = Main.rand.NextVector2Circular(30f, 30f);
                Vector2 pos = player.Center + offset;
                Dust d = Dust.NewDustPerfect(pos, DustID.PurpleTorch,
                    Main.rand.NextVector2Circular(0.4f, 0.4f), 0, default, 0.7f);
                d.noGravity = true;
                d.fadeIn = 0.9f;
            }

            if (Main.rand.NextBool(8))
            {
                // Occasional violet authority spark
                NachtmusikVFXLibrary.SpawnTwinklingStars(player.Center, 1, 25f);
            }

            // Subtle cosmic lighting
            NachtmusikVFXLibrary.AddNachtmusikLight(player.Center, 0.3f);
        }

        // =====================================================================
        //  PreDrawInWorldBloom — Shader-driven weapon glow
        // =====================================================================
        public static void PreDrawInWorldBloom(SpriteBatch sb, Texture2D tex, Vector2 pos,
            Vector2 origin, float rotation, float scale)
        {
            // Draw weapon with ExecutionDecree glow pass overlay
            if (NachtmusikShaderManager.HasExecutionDecree)
            {
                NachtmusikShaderManager.BeginShaderAdditive(sb);
                NachtmusikShaderManager.ApplyExecutionDecreeGlow((float)Main.timeForVisualEffects * 0.02f);

                sb.Draw(tex, pos - Main.screenPosition, null,
                    NachtmusikPalette.Violet * 0.6f, rotation, origin, scale * 1.08f,
                    SpriteEffects.None, 0f);

                NachtmusikShaderManager.RestoreSpriteBatch(sb);
            }
            else
            {
                // Fallback: simple bloom stack
                NachtmusikVFXLibrary.DrawNachtmusikBloomStack(sb, pos,
                    NachtmusikPalette.Violet, NachtmusikPalette.CosmicVoid, scale * 0.4f, 0.5f);
            }
        }

        // =====================================================================
        //  SwingTrailVFX — Per-frame cosmic void trail during swing
        // =====================================================================
        public static void SwingTrailVFX(Vector2 tipPos, Vector2 swordDirection, int comboStep, int timer)
        {
            // Void-crack dust at blade tip
            if (timer % 2 == 0)
            {
                Vector2 perpendicular = new Vector2(-swordDirection.Y, swordDirection.X);
                Vector2 dustVel = perpendicular * Main.rand.NextFloat(-2f, 2f) + swordDirection * 0.5f;

                Dust d = Dust.NewDustPerfect(tipPos, DustID.PurpleTorch, dustVel, 0, default, 1.1f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // Cosmic authority sparks — wider spread on later combo phases
            if (timer % (4 - Math.Min(comboStep, 2)) == 0)
            {
                float sparkScale = 0.3f + comboStep * 0.1f;
                Vector2 sparkOffset = Main.rand.NextVector2Circular(8f, 8f);
                NachtmusikVFXLibrary.SpawnTwinklingStars(tipPos + sparkOffset, 1, 10f);
            }

            // Music notes on later combo steps (the executioner's melody)
            if (comboStep >= 1 && timer % 8 == 0)
            {
                NachtmusikVFXLibrary.SpawnMusicNotes(tipPos, 1, 15f, 0.5f, 0.8f, 30);
            }

            NachtmusikVFXLibrary.AddPaletteLighting(tipPos, 0.2f, 0.5f + comboStep * 0.15f);
        }

        // =====================================================================
        //  SwingImpactVFX — Shader-driven hit impact
        // =====================================================================
        public static void SwingImpactVFX(Vector2 hitPos, int comboStep = 0)
        {
            float intensity = 1f + comboStep * 0.3f;

            // Core impact: void burst + violet flash
            NachtmusikVFXLibrary.MeleeImpact(hitPos, comboStep);

            // Radial void dust explosion
            int dustCount = 8 + comboStep * 4;
            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                Vector2 vel = angle.ToRotationVector2() * (4f + Main.rand.NextFloat() * 3f) * intensity;
                Dust d = Dust.NewDustPerfect(hitPos, DustID.PurpleTorch, vel, 0, default, 1.2f * intensity);
                d.noGravity = true;
                d.fadeIn = 1.4f;
            }

            // Constellation spark ring on higher combos
            if (comboStep >= 1)
            {
                NachtmusikVFXLibrary.SpawnConstellationCircle(hitPos, 30f * intensity, 6 + comboStep * 2, 
                    Main.rand.NextFloat() * MathHelper.TwoPi);
            }

            // Glyph authority burst on combo 2
            if (comboStep >= 2)
            {
                NachtmusikVFXLibrary.SpawnOrbitingGlyphs(hitPos, 4, 35f, Main.rand.NextFloat() * MathHelper.TwoPi);
            }

            // Music note scatter
            NachtmusikVFXLibrary.SpawnMusicNotes(hitPos, 2 + comboStep, 20f, 0.6f, 1f, 30);

            // Impact bloom
            NachtmusikVFXLibrary.DrawBloom(hitPos, 0.4f * intensity, 0.8f);
            NachtmusikVFXLibrary.AddPaletteLighting(hitPos, 0.3f, 0.8f * intensity);
        }

        // =====================================================================
        //  FinisherVFX — Execution decree: massive void-rip cascade
        // =====================================================================
        public static void FinisherVFX(Vector2 pos, float intensity = 1f)
        {
            // Grand void-crack explosion
            NachtmusikVFXLibrary.SpawnStarburstCascade(pos, 5, intensity, 1f);

            // Outward shattered starlight spray
            NachtmusikVFXLibrary.SpawnShatteredStarlight(pos, 10, 7f * intensity, 0.8f, false);

            // Golden authority glyph ring
            NachtmusikVFXLibrary.SpawnOrbitingGlyphs(pos, 6, 50f * intensity, 
                Main.rand.NextFloat() * MathHelper.TwoPi);

            // Constellation decree circle — the execution signature
            NachtmusikVFXLibrary.SpawnConstellationCircle(pos, 60f * intensity, 8, 
                Main.rand.NextFloat() * MathHelper.TwoPi);

            // Music note cascade
            NachtmusikVFXLibrary.SpawnMusicNotes(pos, 6, 40f, 0.7f, 1.2f, 40);

            // Massive dust burst
            NachtmusikVFXLibrary.SpawnRadialDustBurst(pos, 20, 8f * intensity);

            // Heavy bloom stack
            NachtmusikVFXLibrary.DrawComboBloom(pos, 2, 0.6f * intensity, 1f);

            // Strong lighting
            NachtmusikVFXLibrary.AddPaletteLighting(pos, 0.1f, 1.2f * intensity);
        }
    }
}
