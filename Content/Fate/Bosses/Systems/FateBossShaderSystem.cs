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
    /// Phase-aware rendering system for the Warden of Melodies.
    /// Called from PreDraw/PostDraw to layer shader effects on the boss sprite.
    ///
    /// Phase 1 (Awakening):   Concentric glyph rings orbiting the boss, each glyph
    ///                        occasionally flashing a different theme color.
    /// Phase 2 (Convergence): Glyph rings accelerate and merge, dual-theme attack accents,
    ///                        reality distortion shimmer around boss.
    /// Phase 3 (Singularity): Black hole inward-spiral aura -- particles and light bend
    ///                        toward boss before launching outward as attacks.
    /// Enrage (Unraveling):   Boss transcends screen boundaries, chromatic aberration
    ///                        ripples on every impact, telegraph lines from off-screen.
    /// </summary>
    public static class FateBossShaderSystem
    {
        // 10 theme colors for glyph flashes in Phase 1
        private static readonly Color[] ThemeColors = new Color[]
        {
            new Color(200, 120, 180),  // Spring -- pink
            new Color(255, 200, 50),   // Eroica -- gold
            new Color(80, 200, 100),   // Enigma -- eerie green
            new Color(140, 100, 200),  // Moonlight Sonata -- purple
            new Color(240, 240, 255),  // Swan Lake -- white
            new Color(255, 140, 40),   // La Campanella -- orange
            new Color(200, 50, 30),    // Dies Irae -- blood red
            new Color(150, 200, 255),  // Clair de Lune -- ice blue
            new Color(100, 120, 200),  // Nachtmusik -- indigo
            new Color(255, 200, 50),   // Ode to Joy -- warm gold
        };

        /// <summary>
        /// Draws concentric glyph orbit rings around the boss.
        /// Phase 1: Slow orbiting glyphs, each occasionally flashing a theme color.
        /// Phase 2: Rings accelerate and begin merging -- dual colors bleed together.
        /// Phase 3: Rings collapse inward, glyphs spiral into the singularity.
        /// </summary>
        public static void DrawGlyphRings(SpriteBatch sb, NPC npc, Vector2 screenPos,
            int fatePhase, bool isAwakened, float aggressionLevel)
        {
            float time = (float)Main.timeForVisualEffects;
            Vector2 drawPos = npc.Center - screenPos;

            int ringCount = 2 + Math.Min(fatePhase, 2);
            float baseSpeed = 0.008f + fatePhase * 0.004f + aggressionLevel * 0.003f;

            if (fatePhase >= 1)
                baseSpeed *= 1.5f;

            float collapseProgress = fatePhase >= 2 ? Math.Min(1f, aggressionLevel * 1.5f) : 0f;

            for (int ring = 0; ring < ringCount; ring++)
            {
                float ringProgress = (float)ring / ringCount;
                float baseRadius = 80f + ring * 50f;
                float radius = MathHelper.Lerp(baseRadius, 30f + ring * 12f, collapseProgress);
                float speed = baseSpeed * (1f + ring * 0.3f) * (ring % 2 == 0 ? 1f : -1f);
                int glyphsInRing = 4 + ring * 2;

                for (int g = 0; g < glyphsInRing; g++)
                {
                    float angle = time * speed + MathHelper.TwoPi * g / glyphsInRing + ring * 0.5f;
                    Vector2 glyphPos = drawPos + new Vector2(
                        (float)Math.Cos(angle) * radius,
                        (float)Math.Sin(angle) * radius * 0.85f);

                    // Normally Fate colors, but occasionally flash a theme color
                    Color glyphColor = FatePalette.DarkPink * 0.5f;
                    int themeFlashIndex = ((int)(time * 0.02f) + g * 3 + ring * 7) % 80;
                    if (themeFlashIndex < 10)
                    {
                        Color flashColor = ThemeColors[themeFlashIndex];
                        float flashIntensity = 0.6f + 0.4f * (float)Math.Sin(time * 0.08f + g);
                        glyphColor = Color.Lerp(glyphColor, flashColor, flashIntensity);
                    }

                    // Phase 2: dual-theme blending -- adjacent glyphs bleed into each other
                    if (fatePhase >= 1 && g > 0)
                    {
                        int themeA = (g + ring) % 10;
                        int themeB = (g + ring + 1) % 10;
                        float blend = 0.5f + 0.5f * (float)Math.Sin(time * 0.015f + g * 0.8f);
                        Color dualColor = Color.Lerp(ThemeColors[themeA], ThemeColors[themeB], blend);
                        glyphColor = Color.Lerp(glyphColor, dualColor, 0.3f * (fatePhase >= 2 ? 0.6f : 0.3f));
                    }

                    float scale = 0.3f + ringProgress * 0.1f;
                    CustomParticles.Glyph(glyphPos + screenPos, glyphColor, scale, (g + ring * 3) % 12 + 1);
                }
            }
        }

        /// <summary>
        /// Draws the cosmic aura around the boss -- layered bloom stacking.
        /// Phase 1: Subdued dark pink/crimson halo.
        /// Phase 2: Intensified with reality-distortion shimmer.
        /// Phase 3: Black hole accretion glow -- bright core with dark void edge.
        /// </summary>
        public static void DrawCosmicAura(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float aggressionLevel, int fatePhase, bool isAwakened)
        {
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow == null) return;
            Vector2 glowOrigin = glow.Size() / 2f;
            Vector2 drawPos = npc.Center - screenPos;
            float time = (float)Main.timeForVisualEffects;

            float baseIntensity = 0.3f + aggressionLevel * 0.3f + fatePhase * 0.1f;
            float pulse = 0.85f + 0.15f * (float)Math.Sin(time * 0.025f);

            if (fatePhase >= 2)
            {
                // Phase 3: Black hole aura -- dark void ring with bright inner accretion
                Color voidOuter = FatePalette.CosmicVoid * (0.25f * baseIntensity * pulse);
                voidOuter.A = 0;
                sb.Draw(glow, drawPos, null, voidOuter, 0f, glowOrigin, 2.28f, SpriteEffects.None, 0f);

                Color accretion = FatePalette.BrightCrimson * (0.2f * baseIntensity * pulse);
                accretion.A = 0;
                sb.Draw(glow, drawPos, null, accretion, 0f, glowOrigin, 2.0f, SpriteEffects.None, 0f);

                Color hotCore = Color.Lerp(FatePalette.WhiteCelestial, FatePalette.DarkPink, 0.3f) * (0.18f * baseIntensity);
                hotCore.A = 0;
                sb.Draw(glow, drawPos, null, hotCore, 0f, glowOrigin, 0.8f, SpriteEffects.None, 0f);
            }
            else if (fatePhase >= 1)
            {
                // Phase 2: Reality distortion shimmer
                float shimmer = 0.7f + 0.3f * (float)Math.Sin(time * 0.04f);
                Color outerGlow = FatePalette.DarkPink * (0.18f * baseIntensity * shimmer);
                outerGlow.A = 0;
                sb.Draw(glow, drawPos, null, outerGlow, 0f, glowOrigin, 1.82f, SpriteEffects.None, 0f);

                Color midGlow = FatePalette.BrightCrimson * (0.14f * baseIntensity * pulse);
                midGlow.A = 0;
                sb.Draw(glow, drawPos, null, midGlow, 0f, glowOrigin, 1.8f, SpriteEffects.None, 0f);

                Color coreGlow = FatePalette.WhiteCelestial * (0.1f * baseIntensity);
                coreGlow.A = 0;
                sb.Draw(glow, drawPos, null, coreGlow, 0f, glowOrigin, 0.7f, SpriteEffects.None, 0f);
            }
            else
            {
                // Phase 1: Subdued cosmic presence
                Color outerGlow = FatePalette.DarkPink * (0.12f * baseIntensity * pulse);
                outerGlow.A = 0;
                sb.Draw(glow, drawPos, null, outerGlow, 0f, glowOrigin, 2.0f, SpriteEffects.None, 0f);

                Color coreGlow = FatePalette.ConstellationSilver * (0.08f * baseIntensity);
                coreGlow.A = 0;
                sb.Draw(glow, drawPos, null, coreGlow, 0f, glowOrigin, 0.9f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draws constellation trail when boss moves fast.
        /// Phase 2+: Trail streaks show dual theme colors fading in afterimages.
        /// </summary>
        public static void DrawConstellationTrail(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, int fatePhase, bool isAwakened)
        {
            Color trailColor = fatePhase >= 2
                ? FatePalette.WhiteCelestial
                : (fatePhase >= 1 ? FatePalette.BrightCrimson : FatePalette.DarkPink);
            float width = 1.2f + fatePhase * 0.3f;

            BossRenderHelper.DrawShaderTrail(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.FateConstellationTrail, trailColor, width,
                (float)Main.timeForVisualEffects * 0.018f, 8f);
        }

        /// <summary>
        /// Draws the awakening shatter transition -- reality glass cracking.
        /// </summary>
        public static void DrawAwakeningShatter(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float transitionProgress)
        {
            BossRenderHelper.DrawPhaseTransition(sb, npc, screenPos,
                BossShaderManager.FateAwakeningShatter, transitionProgress,
                FatePalette.CosmicVoid, FatePalette.WhiteCelestial, 1.5f + transitionProgress * 0.5f);
        }

        /// <summary>
        /// Draws the spiral black hole dissolve death effect.
        /// </summary>
        public static void DrawCosmicDeathRift(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, float dissolveProgress)
        {
            BossRenderHelper.DrawDissolve(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.FateCosmicDeathRift, dissolveProgress,
                FatePalette.DarkPink, 0.04f);

            if (dissolveProgress > 0.2f)
            {
                Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
                if (glow == null) return;
                Vector2 glowOrigin = glow.Size() / 2f;
                Vector2 drawPos = npc.Center - screenPos;
                float edgeIntensity = (dissolveProgress - 0.2f) / 0.8f;
                Color edgeColor = Color.Lerp(FatePalette.DarkPink, FatePalette.WhiteCelestial, edgeIntensity) * (0.25f * edgeIntensity);
                edgeColor.A = 0;
                sb.Draw(glow, drawPos, null, edgeColor, 0f, glowOrigin, 1.5f + edgeIntensity * 1.5f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Spawns boss-fragment echoes -- fleeting visual references to previous bosses.
        /// Phase 1: Occasional single-theme fragments (bell toll ring, feather arc, flame slash).
        /// Phase 2: Dual-theme fusion fragments (flame + frost, void + light).
        /// Phase 3: Rapid fragments all collapsing to Fate crimson.
        /// </summary>
        public static void SpawnBossFragmentEchoes(NPC npc, int timer, int fatePhase)
        {
            int interval = fatePhase >= 2 ? 30 : (fatePhase >= 1 ? 50 : 80);
            if (timer % interval != 0) return;

            Vector2 bossCenter = npc.Center;
            float time = (float)Main.timeForVisualEffects;

            if (fatePhase >= 1)
            {
                // Dual-theme fusion: two theme colors blended
                int themeA = Main.rand.Next(10);
                int themeB = (themeA + 3 + Main.rand.Next(4)) % 10;
                Color fusionColor = Color.Lerp(ThemeColors[themeA], ThemeColors[themeB], 0.5f);
                fusionColor = Color.Lerp(fusionColor, FatePalette.BrightCrimson, 0.4f + fatePhase * 0.1f);

                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 pos = bossCenter + angle.ToRotationVector2() * (60f + Main.rand.NextFloat(80f));
                CustomParticles.GenericFlare(pos, fusionColor, 0.5f, 18);
                CustomParticles.Glyph(pos, ThemeColors[themeA] * 0.6f, 0.3f, Main.rand.Next(1, 13));
            }
            else
            {
                // Single theme fragment echo
                int themeIdx = Main.rand.Next(10);
                Color echoColor = Color.Lerp(ThemeColors[themeIdx], FatePalette.BrightCrimson, 0.35f);

                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 pos = bossCenter + angle.ToRotationVector2() * (80f + Main.rand.NextFloat(60f));
                CustomParticles.GenericFlare(pos, echoColor, 0.4f, 15);
            }
        }

        /// <summary>
        /// Spawns musical accents and ambient particles driven by phase and HP.
        /// </summary>
        public static void SpawnMusicalAccents(NPC npc, int timer, int fatePhase, bool isAwakened)
        {
            float hpDrive = 1f - ((float)npc.life / npc.lifeMax);

            // Glyph orbit
            int glyphInterval = Math.Max(1, (int)(80 - hpDrive * 40 - fatePhase * 10));
            if (timer % glyphInterval == 0)
            {
                Phase10BossVFX.StaffLineConvergence(npc.Center, FatePalette.DarkPink, 0.5f + fatePhase * 0.2f);
            }

            // Cosmic chord progression
            int chordInterval = Math.Max(1, (int)(55 - hpDrive * 25 - fatePhase * 8));
            if (timer % chordInterval == 0 && fatePhase >= 1)
            {
                Phase10Integration.Fate.CosmicChordProgression(npc.Center, timer / 60);
            }

            // Phase 3: Star particles streaming inward constantly
            if (fatePhase >= 2 && timer % 4 == 0)
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 spawnPos = npc.Center + angle.ToRotationVector2() * (200f + Main.rand.NextFloat(100f));
                Vector2 inwardVel = (npc.Center - spawnPos).SafeNormalize(Vector2.Zero) * (2f + hpDrive * 3f);
                var star = new SparkleParticle(spawnPos, inwardVel, FatePalette.WhiteCelestial * 0.7f, 0.2f + hpDrive * 0.1f, 20);
                MagnumParticleHandler.SpawnParticle(star);
            }

            // Celestial fortissimo pulses during awakened state
            int fortissimoInterval = Math.Max(1, (int)(40 - hpDrive * 20));
            if (timer % fortissimoInterval == 0 && isAwakened)
            {
                Phase10BossVFX.FortissimoFlashWarning(npc.Center, FatePalette.WhiteCelestial, 1.0f);
                CustomParticles.FateCosmicBurst(npc.Center, 6);
            }

            // Glyph ring
            int glyphRingInterval = Math.Max(1, (int)(100 - hpDrive * 50));
            if (timer % glyphRingInterval == 0 && isAwakened)
            {
                CustomParticles.GlyphCircle(npc.Center, FatePalette.DarkPink, 8, 80f, 0.02f);
            }

            // Bloom orbiting motes
            if (timer % 10 == 0)
            {
                float moteAngle = timer * 0.08f;
                Vector2 orbitPos = npc.Center + moteAngle.ToRotationVector2() * (55f + hpDrive * 25f);
                Color orbitColor = fatePhase >= 2 ? FatePalette.WhiteCelestial : (isAwakened ? FatePalette.BrightCrimson : FatePalette.DarkPink);
                var bloom = new BloomParticle(orbitPos, Vector2.Zero, orbitColor, 0.2f + hpDrive * 0.15f, 16);
                MagnumParticleHandler.SpawnParticle(bloom);
            }

            // Ascending celestial wisps at high HP drive
            if (hpDrive > 0.4f && timer % 16 == 0)
            {
                Vector2 sparkVel = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-3f, -1.5f));
                var sparkle = new SparkleParticle(npc.Center + Main.rand.NextVector2Circular(40f, 40f), sparkVel, FatePalette.WhiteCelestial, 0.3f, 22);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Boss fragment echoes -- visual references to previous bosses
            SpawnBossFragmentEchoes(npc, timer, fatePhase);
        }
    }
}
