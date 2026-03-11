using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems;
using MagnumOpus.Content.Fate;

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

            // === BLOOM STACKING: 3-layer cosmic glow ===
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            Vector2 glowOrigin = glow.Size() / 2f;
            Vector2 drawPos = npc.Center - screenPos;

            // Outer: dark pink cosmic halo
            Color outerGlow = primary * (0.15f * intensity);
            outerGlow.A = 0;
            sb.Draw(glow, drawPos, null, outerGlow, 0f, glowOrigin, baseRadius / glow.Width * 3.5f, SpriteEffects.None, 0f);

            // Mid: crimson pulse
            float pulse = 0.8f + 0.2f * (float)Math.Sin(Main.timeForVisualEffects * 0.03f);
            Color midGlow = BrightCrimson * (0.12f * intensity * pulse);
            midGlow.A = 0;
            sb.Draw(glow, drawPos, null, midGlow, 0f, glowOrigin, baseRadius / glow.Width * 2.2f, SpriteEffects.None, 0f);

            // Core: celestial white hot center
            Color coreGlow = CelestialWhite * (0.1f * intensity);
            coreGlow.A = 0;
            sb.Draw(glow, drawPos, null, coreGlow, 0f, glowOrigin, baseRadius / glow.Width * 1.0f, SpriteEffects.None, 0f);

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

            // Widening edge glow as dissolution progresses
            if (dissolveProgress > 0.2f)
            {
                Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
                Vector2 glowOrigin = glow.Size() / 2f;
                Vector2 drawPos = npc.Center - screenPos;
                float edgeIntensity = (dissolveProgress - 0.2f) / 0.8f;
                Color edgeColor = Color.Lerp(DarkPink, CelestialWhite, edgeIntensity) * (0.25f * edgeIntensity);
                edgeColor.A = 0;
                sb.Draw(glow, drawPos, null, edgeColor, 0f, glowOrigin, 1.5f + edgeIntensity * 1.5f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draws multilayer boss glow underlay — cosmic bloom aura beneath the sprite.
        /// 3-layer: outer cosmic void, mid crimson heartbeat, core celestial radiance.
        /// </summary>
        public static void DrawBossGlow(SpriteBatch sb, NPC npc, Vector2 screenPos, bool isAwakened)
        {
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            Vector2 glowOrigin = glow.Size() / 2f;
            Vector2 drawPos = npc.Center - screenPos;
            float time = (float)Main.timeForVisualEffects;
            float pulse = 0.85f + 0.15f * (float)Math.Sin(time * 0.025f);

            if (isAwakened)
            {
                // Awakened: fierce crimson + celestial core
                Color outer = FatePalette.BrightCrimson * (0.18f * pulse);
                outer.A = 0;
                sb.Draw(glow, drawPos, null, outer, 0f, glowOrigin, 2.0f, SpriteEffects.None, 0f);

                Color mid = FatePalette.DarkPink * (0.14f * pulse);
                mid.A = 0;
                sb.Draw(glow, drawPos, null, mid, 0f, glowOrigin, 1.3f, SpriteEffects.None, 0f);

                Color core = FatePalette.WhiteCelestial * (0.12f * pulse);
                core.A = 0;
                sb.Draw(glow, drawPos, null, core, 0f, glowOrigin, 0.6f, SpriteEffects.None, 0f);
            }
            else
            {
                // Phase 1: subdued dark pink glow
                Color outer = FatePalette.DarkPink * (0.12f * pulse);
                outer.A = 0;
                sb.Draw(glow, drawPos, null, outer, 0f, glowOrigin, 1.6f, SpriteEffects.None, 0f);

                Color core = FatePalette.ConstellationSilver * (0.08f * pulse);
                core.A = 0;
                sb.Draw(glow, drawPos, null, core, 0f, glowOrigin, 0.8f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Spawns musical VFX particles — cosmic chord progressions,
        /// constellation arpeggios, and celestial rhythm pulses.
        /// HP-driven density: intervals shrink as boss weakens.
        /// </summary>
        public static void SpawnMusicalAccents(NPC npc, int timer, int fatePhase, bool isAwakened)
        {
            float hpDrive = 1f - ((float)npc.life / npc.lifeMax);

            // Constellation glyph orbits — HP-driven interval (90→45)
            int glyphInterval = Math.Max(1, (int)(90 - hpDrive * 45));
            if (timer % glyphInterval == 0)
            {
                Phase10BossVFX.StaffLineConvergence(npc.Center, DarkPink, 0.6f + fatePhase * 0.2f);
            }

            // Cosmic chord progression on attack beats — HP-driven (60→30)
            int chordInterval = Math.Max(1, (int)(60 - hpDrive * 30));
            if (timer % chordInterval == 0 && fatePhase >= 1)
            {
                Phase10Integration.Fate.CosmicChordProgression(npc.Center, timer / 60);
            }

            // Celestial fortissimo pulses during awakened state — HP-driven (45→20)
            int fortissimoInterval = Math.Max(1, (int)(45 - hpDrive * 25));
            if (timer % fortissimoInterval == 0 && isAwakened)
            {
                Phase10BossVFX.FortissimoFlashWarning(npc.Center, CelestialWhite, 1.0f);
                CustomParticles.FateCosmicBurst(npc.Center, 6);
            }

            // Ancient glyph ring in True Form — HP-driven (120→60)
            int glyphRingInterval = Math.Max(1, (int)(120 - hpDrive * 60));
            if (timer % glyphRingInterval == 0 && isAwakened)
            {
                CustomParticles.GlyphCircle(npc.Center, DarkPink, 8, 80f, 0.02f);
            }

            // === NEW: Bloom orbiting motes every 12 frames ===
            if (timer % 12 == 0)
            {
                float angle = timer * 0.08f;
                Vector2 orbitPos = npc.Center + angle.ToRotationVector2() * (60f + hpDrive * 30f);
                Color orbitColor = isAwakened ? FatePalette.BrightCrimson : FatePalette.DarkPink;
                var bloom = new BloomParticle(orbitPos, Vector2.Zero, orbitColor, 0.25f + hpDrive * 0.15f, 18);
                MagnumParticleHandler.SpawnParticle(bloom);
            }

            // === NEW: Ascending celestial wisps at high hpDrive ===
            if (hpDrive > 0.5f && timer % 20 == 0)
            {
                Vector2 sparkVel = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-3f, -1.5f));
                var sparkle = new SparkleParticle(npc.Center + Main.rand.NextVector2Circular(40f, 40f), sparkVel, FatePalette.WhiteCelestial, 0.3f, 25);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }
    }
}
