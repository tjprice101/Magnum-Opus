using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Fate.Bosses.Systems
{
    /// <summary>
    /// Fate boss shader-driven rendering system.
    /// Manages all 5 boss shaders: FateCosmicAura, FateConstellationTrail,
    /// FateTimeSlice, FateAwakeningShatter, and FateCosmicDeathRift.
    ///
    /// Called from FateWardenOfMelodies.PreDraw/PostDraw to layer shader
    /// effects on top of the standard sprite drawing.
    /// The Warden of Melodies is ENDGAME  Eeffects should feel cosmic and inevitable.
    /// </summary>
    public static class FateBossShaderSystem
    {
        // Theme colors  Eblack void bleeding to dark pink to crimson with celestial white highlights
        private static readonly Color CosmicBlack = new Color(10, 5, 15);
        private static readonly Color DarkPink = new Color(180, 40, 80);
        private static readonly Color BrightCrimson = new Color(220, 40, 60);
        private static readonly Color CelestialWhite = new Color(230, 220, 255);

        /// <summary>
        /// Draws the cosmic aura with orbiting ancient glyphs and constellation patterns.
        /// Intensity scales with phase and awakened state.
        /// </summary>
        public static void DrawCosmicAura(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float aggressionLevel, int fatePhase, bool isAwakened)
        {
            float baseRadius = 100f + fatePhase * 25f;
            float intensity = 0.4f + aggressionLevel * 0.4f;
            if (isAwakened)
            {
                intensity *= 1.8f;
                baseRadius *= 1.3f;
            }

            Color primary = isAwakened ? BrightCrimson : DarkPink;
            Color secondary = isAwakened ? CelestialWhite : CosmicBlack;

            BossRenderHelper.DrawShaderAura(sb, npc, screenPos,
                BossShaderManager.FateCosmicAura, primary, secondary,
                baseRadius, intensity, (float)Main.timeForVisualEffects * 0.015f);

            // Orbiting constellation glyphs during aura
            if (isAwakened)
            {
                float time = (float)Main.timeForVisualEffects * 0.01f;
                for (int i = 0; i < 6; i++)
                {
                    float angle = time + MathHelper.TwoPi * i / 6f;
                    Vector2 glyphPos = npc.Center + angle.ToRotationVector2() * baseRadius * 0.7f;
                    CustomParticles.Glyph(glyphPos - screenPos, CelestialWhite * 0.6f, 0.3f, i + 1);
                }
            }
        }

        /// <summary>
        /// Draws star-point constellation trails during cosmic dashes.
        /// Each trail point is a star connected by faint lines.
        /// </summary>
        public static void DrawConstellationTrail(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, bool isAwakened)
        {
            Color trailColor = isAwakened ? CelestialWhite : DarkPink;
            float width = isAwakened ? 1.8f : 1.2f;

            BossRenderHelper.DrawShaderTrail(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.FateConstellationTrail, trailColor, width,
                (float)Main.timeForVisualEffects * 0.018f, 8f);
        }

        /// <summary>
        /// Draws chromatic aberration-style reality split for the TimeSlice attack.
        /// Red, blue, and white channels separated to convey temporal fracture.
        /// </summary>
        public static void DrawTimeSlice(SpriteBatch sb, Vector2 position, Vector2 screenPos,
            float intensity, float sliceAngle)
        {
            BossRenderHelper.DrawAttackFlash(sb, position, screenPos,
                BossShaderManager.FateTimeSlice, BrightCrimson, intensity,
                (float)Main.timeForVisualEffects * 0.04f);

            // Chromatic split accents
            Vector2 redOffset = new Vector2((float)Math.Cos(sliceAngle), (float)Math.Sin(sliceAngle)) * 4f * intensity;
            Vector2 blueOffset = -redOffset;
            CustomParticles.GenericFlare(position + redOffset, new Color(255, 40, 60) * 0.4f, 0.5f * intensity, 8);
            CustomParticles.GenericFlare(position + blueOffset, new Color(60, 40, 255) * 0.4f, 0.5f * intensity, 8);
        }

        /// <summary>
        /// Draws the True Form awakening shatter effect  Ereality glass cracking
        /// to reveal cosmic energy underneath.
        /// </summary>
        public static void DrawAwakeningShatter(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float transitionProgress)
        {
            Color from = CosmicBlack;
            Color to = CelestialWhite;
            float intensity = 1.5f + transitionProgress * 0.5f;

            BossRenderHelper.DrawPhaseTransition(sb, npc, screenPos,
                BossShaderManager.FateAwakeningShatter, transitionProgress,
                from, to, intensity);
        }

        /// <summary>
        /// Draws the spiral black hole dissolve death effect.
        /// Stars and cosmic energy spiral inward as the Warden unravels.
        /// </summary>
        public static void DrawCosmicDeathRift(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, float dissolveProgress)
        {
            BossRenderHelper.DrawDissolve(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.FateCosmicDeathRift, dissolveProgress,
                DarkPink, 0.04f);
        }

        /// <summary>
        /// Spawns musical VFX particles  Ecosmic chord progressions,
        /// constellation arpeggios, and celestial rhythm pulses.
        /// </summary>
        public static void SpawnMusicalAccents(NPC npc, int timer, int fatePhase, bool isAwakened)
        {
            // Constellation glyph orbits every 1.5 seconds
            if (timer % 90 == 0)
            {
                Phase10BossVFX.StaffLineConvergence(npc.Center, DarkPink, 0.6f + fatePhase * 0.2f);
            }

            // Cosmic chord progression on attack beats
            if (timer % 60 == 0 && fatePhase >= 1)
            {
                Phase10Integration.Fate.CosmicChordProgression(npc.Center, timer / 60);
            }

            // Celestial fortissimo pulses during awakened state
            if (timer % 45 == 0 && isAwakened)
            {
                Phase10BossVFX.FortissimoFlashWarning(npc.Center, CelestialWhite, 1.0f);
                CustomParticles.FateCosmicBurst(npc.Center, 6);
            }

            // Ancient glyph ring in True Form
            if (timer % 120 == 0 && isAwakened)
            {
                CustomParticles.GlyphCircle(npc.Center, DarkPink, 8, 80f, 0.02f);
            }
        }
    }
}
