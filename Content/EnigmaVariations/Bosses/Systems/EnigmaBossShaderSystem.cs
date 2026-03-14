using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX.Screen;

namespace MagnumOpus.Content.EnigmaVariations.Bosses.Systems
{
    /// <summary>
    /// Enigma boss shader-driven rendering system — redesigned for phase identity.
    ///
    /// Phase 1 — The Riddle: Boss flickers between visible and translucent.
    ///           Subtle aura, minimal trail. The less you see, the more you fear.
    /// Phase 2 — The Unraveling: Aura intensifies with eerie green orbit flames,
    ///           shadow trail streaks deeply, reality warp at edges.
    /// Phase 3 — The Revelation: Full void aura wreathed in arcane green fire,
    ///           heavy shadow trail, paradox rift effects everywhere.
    /// Enrage — Total Mystery: Boss becomes void silhouette, screaming green
    ///          glow, inverted rendering feel.
    /// </summary>
    public static class EnigmaBossShaderSystem
    {
        // New palette — gradient of unknowing
        private static readonly Color VoidBlack = new Color(10, 5, 15);
        private static readonly Color DeepPurple = new Color(80, 20, 140);
        private static readonly Color EerieGreen = new Color(40, 220, 80);
        private static readonly Color ArcaneGreen = new Color(100, 255, 130);
        private static readonly Color UnsettlingWhite = new Color(220, 200, 255);
        private static readonly Color VoidPurple = new Color(50, 10, 80);

        /// <summary>
        /// Draws the swirling void aura — phase-driven intensity.
        /// Phase 1: barely visible, peripheral shimmer only.
        /// Phase 2: mid-intensity with eerie green threads.
        /// Phase 3: overwhelming void wreath with arcane green fire.
        /// Enrage: inverted — green-white core bleeds outward.
        /// </summary>
        public static void DrawVoidAura(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float aggressionLevel, int difficultyTier, bool isEnraged)
        {
            // Phase 1 is deliberately understated — the fear comes from what you CAN'T see
            float phaseIntensity = difficultyTier switch
            {
                0 => 0.15f + aggressionLevel * 0.15f,  // Whisper
                1 => 0.35f + aggressionLevel * 0.35f,  // Building dread
                _ => 0.6f + aggressionLevel * 0.4f      // Full revelation
            };
            if (isEnraged) phaseIntensity = 1.0f;

            float baseRadius = 70f + difficultyTier * 25f;
            if (isEnraged) baseRadius *= 1.3f;

            Color primary = isEnraged ? ArcaneGreen : Color.Lerp(DeepPurple, EerieGreen, aggressionLevel);
            Color secondary = isEnraged ? UnsettlingWhite : VoidBlack;

            BossRenderHelper.DrawShaderAura(sb, npc, screenPos,
                BossShaderManager.EnigmaVoidAura, primary, secondary,
                baseRadius, phaseIntensity, (float)Main.timeForVisualEffects * 0.012f);

            // 3-layer bloom stacking — scales with phase
            var bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (bloomTex == null) return;

            Vector2 drawPos = npc.Center - screenPos;
            Vector2 bloomOrigin = new Vector2(bloomTex.Width, bloomTex.Height) * 0.5f;
            float pulse = 0.9f + 0.1f * (float)Math.Sin(Main.timeForVisualEffects * 0.03f);

            // Layer 1: Wide void ambient — deeper in later phases
            Color voidOuter = VoidBlack;
            voidOuter.A = 0;
            float outerScale = baseRadius * 2.5f / bloomTex.Width * pulse;
            sb.Draw(bloomTex, drawPos, null, voidOuter * phaseIntensity * 0.25f, 0f, bloomOrigin, outerScale, SpriteEffects.None, 0f);

            // Layer 2: Purple mid glow — shifts green in Phase 3+
            Color midGlow = difficultyTier >= 2 ? Color.Lerp(DeepPurple, EerieGreen, 0.3f) : DeepPurple;
            midGlow.A = 0;
            float midScale = baseRadius * 1.6f / bloomTex.Width * pulse;
            sb.Draw(bloomTex, drawPos, null, midGlow * phaseIntensity * 0.35f, 0f, bloomOrigin, midScale, SpriteEffects.None, 0f);

            // Layer 3: Core — green when Phase 2+, arcane green when enraged
            Color coreColor = difficultyTier switch
            {
                0 => UnsettlingWhite,
                1 => EerieGreen,
                _ => ArcaneGreen
            };
            if (isEnraged) coreColor = ArcaneGreen;
            coreColor.A = 0;
            float coreScale = baseRadius * 0.9f / bloomTex.Width * pulse;
            sb.Draw(bloomTex, drawPos, null, coreColor * phaseIntensity * 0.4f, 0f, bloomOrigin, coreScale, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Draws the shadow trail with phase-driven density and color.
        /// Phase 1: sparse, barely there — flickers of movement.
        /// Phase 2: full trail with eerie green glow orbs.
        /// Phase 3+: dense trail, void-dark, with green fire highlights.
        /// </summary>
        public static void DrawShadowTrail(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, bool isEnraged)
        {
            Color trailColor = isEnraged ? ArcaneGreen : DeepPurple;
            float width = isEnraged ? 1.5f : 0.9f;

            BossRenderHelper.DrawShaderTrail(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.EnigmaShadowTrail, trailColor, width,
                (float)Main.timeForVisualEffects * 0.018f, 3f);

            // Glow orbs at afterimage positions — phase-colored
            var bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (bloomTex == null) return;

            Vector2 bloomOrigin = new Vector2(bloomTex.Width, bloomTex.Height) * 0.5f;
            int orbCount = Math.Min(6, npc.oldPos.Length);
            for (int i = 0; i < orbCount; i++)
            {
                if (npc.oldPos[i] == Vector2.Zero) continue;
                Vector2 orbPos = npc.oldPos[i] + npc.Size * 0.5f - screenPos;
                float fade = 1f - (float)i / orbCount;
                Color orbColor = Color.Lerp(trailColor, EerieGreen, fade * 0.3f);
                orbColor.A = 0;
                sb.Draw(bloomTex, orbPos, null, orbColor * fade * 0.3f, 0f, bloomOrigin, 0.5f * fade, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draws the paradox rift visual during reality-tearing attacks.
        /// Phase 3 makes this more vivid with arcane green fire.
        /// </summary>
        public static void DrawParadoxRift(SpriteBatch sb, Vector2 position, Vector2 screenPos,
            float intensity)
        {
            BossRenderHelper.DrawAttackFlash(sb, position, screenPos,
                BossShaderManager.EnigmaParadoxRift, EerieGreen, intensity,
                (float)Main.timeForVisualEffects * 0.025f);
        }

        /// <summary>
        /// Draws the teleport warp-in/warp-out — phase-escalated colors.
        /// Later phases warp harder with green-tinged distortion.
        /// </summary>
        public static void DrawTeleportWarp(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float warpProgress, bool isPhase3)
        {
            Color from = isPhase3 ? VoidBlack : VoidPurple;
            Color to = isPhase3 ? EerieGreen : DeepPurple;
            float intensity = isPhase3 ? 1.2f : 0.8f;

            BossRenderHelper.DrawPhaseTransition(sb, npc, screenPos,
                BossShaderManager.EnigmaTeleportWarp, warpProgress,
                from, to, intensity);
        }

        /// <summary>
        /// Draws the unveiling dissolve death animation.
        /// The hollow mystery finally reveals... nothing. Green revelation
        /// light floods outward as the form dissolves.
        /// </summary>
        public static void DrawUnveilingDissolve(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, float dissolveProgress)
        {
            Color edgeColor = Color.Lerp(DeepPurple, ArcaneGreen, dissolveProgress);

            BossRenderHelper.DrawDissolve(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.EnigmaUnveilingDissolve, dissolveProgress,
                edgeColor, 0.06f);

            var bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (bloomTex == null) return;

            Vector2 drawPos = npc.Center - screenPos;
            Vector2 bloomOrigin = new Vector2(bloomTex.Width, bloomTex.Height) * 0.5f;

            // Edge glow widens as dissolve progresses
            Color glowColor = edgeColor;
            glowColor.A = 0;
            float glowScale = MathHelper.Lerp(0.6f, 2.5f, dissolveProgress);
            float glowOpacity = MathHelper.Lerp(0.2f, 0.7f, dissolveProgress);
            sb.Draw(bloomTex, drawPos, null, glowColor * glowOpacity, 0f, bloomOrigin, glowScale, SpriteEffects.None, 0f);

            // Revelation core — arcane green, intense
            if (dissolveProgress > 0.4f)
            {
                float coreProgress = (dissolveProgress - 0.4f) / 0.6f;
                Color coreGlow = ArcaneGreen;
                coreGlow.A = 0;
                sb.Draw(bloomTex, drawPos, null, coreGlow * coreProgress * 0.5f, 0f, bloomOrigin, glowScale * 0.5f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draws 3-layer bloom glow on the boss — phase-aware color shifting.
        /// Phase 1: Subtle purple-white flicker — the boss barely wants to be seen.
        /// Phase 2: Stronger purple with green undertone.
        /// Phase 3: Green-dominant glow, the revelation burns.
        /// Enrage: Arcane green blaze with unsettling white core.
        /// </summary>
        public static void DrawBossGlow(SpriteBatch sb, NPC npc, Vector2 screenPos, bool isEnraged)
        {
            var bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (bloomTex == null) return;

            int phase = BossIndexTracker.EnigmaPhase;
            Vector2 drawPos = npc.Center - screenPos;
            Vector2 bloomOrigin = new Vector2(bloomTex.Width, bloomTex.Height) * 0.5f;
            float pulse = 0.85f + 0.15f * (float)Math.Sin(Main.timeForVisualEffects * 0.04f);

            // Phase 1 flicker — the boss stutters between existing and not
            if (phase == 0 && !isEnraged)
            {
                float flicker = (float)Math.Sin(Main.timeForVisualEffects * 0.15f);
                flicker *= (float)Math.Sin(Main.timeForVisualEffects * 0.23f); // Irregular beat
                if (flicker < -0.3f) return; // Sometimes the glow just... isn't there
                pulse *= MathHelper.Clamp(flicker + 0.5f, 0.2f, 1f);
            }

            // Layer 1: Wide void outer
            Color outerColor = VoidBlack;
            outerColor.A = 0;
            float outerOpacity = isEnraged ? 0.35f : 0.25f;
            sb.Draw(bloomTex, drawPos, null, outerColor * outerOpacity * pulse, 0f, bloomOrigin, 1.8f, SpriteEffects.None, 0f);

            // Layer 2: Purple mid — shifts green in Phase 3
            Color midColor = phase >= 2 ? Color.Lerp(DeepPurple, EerieGreen, 0.4f) : DeepPurple;
            if (isEnraged) midColor = EerieGreen;
            midColor.A = 0;
            sb.Draw(bloomTex, drawPos, null, midColor * 0.35f * pulse, 0f, bloomOrigin, 1.1f, SpriteEffects.None, 0f);

            // Layer 3: Inner bright — escalates with phase
            Color innerColor = phase switch
            {
                0 => UnsettlingWhite,
                1 => Color.Lerp(UnsettlingWhite, EerieGreen, 0.5f),
                _ => ArcaneGreen
            };
            if (isEnraged) innerColor = ArcaneGreen;
            innerColor.A = 0;
            float innerOpacity = isEnraged ? 0.45f : 0.3f;
            sb.Draw(bloomTex, drawPos, null, innerColor * innerOpacity * pulse, 0f, bloomOrigin, 0.6f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Spawns musical VFX particles — phase-aware density and type.
        /// Phase 1: sparse, peripheral — mysterious trill, occasional glyph.
        /// Phase 2: building — syncopation, watching eyes, orbiting glyphs.
        /// Phase 3: constant — void harmonics, frequent eyes, dense particles.
        /// Enrage: overwhelming — everything at max density, screaming vortex accents.
        /// </summary>
        public static void SpawnMusicalAccents(NPC npc, int timer, int difficultyTier)
        {
            float hpDrive = 1f - (float)npc.life / npc.lifeMax;
            bool enraged = EnigmaSkySystem.BossEnraged;

            // Mysterious trill — the sound of the unknowable
            int trillInterval = difficultyTier switch
            {
                0 => (int)MathHelper.Lerp(120, 80, hpDrive),
                1 => (int)MathHelper.Lerp(80, 50, hpDrive),
                _ => (int)MathHelper.Lerp(50, 25, hpDrive)
            };
            if (enraged) trillInterval = Math.Max(1, trillInterval / 2);
            if (timer % Math.Max(1, trillInterval) == 0)
            {
                Phase10Integration.Enigma.MysteriousTrill(npc.Center, timer);
            }

            // Paradox syncopation — Phase 2+ rhythmic accents
            if (difficultyTier >= 1)
            {
                int syncopInterval = (int)MathHelper.Lerp(60, 25, hpDrive);
                if (enraged) syncopInterval = Math.Max(1, syncopInterval / 2);
                if (timer % Math.Max(1, syncopInterval) == 0)
                {
                    Phase10Integration.Enigma.ParadoxSyncopation(npc.Center, 120f);
                }
            }

            // Watching eyes — appear Phase 2+, swarm during enrage
            if (difficultyTier >= 1)
            {
                int eyeInterval = difficultyTier >= 2 ? 30 : 60;
                if (enraged) eyeInterval = 15;
                if (timer % Math.Max(1, eyeInterval) == 0)
                {
                    Vector2 eyeOffset = Main.rand.NextVector2Circular(100f, 100f);
                    CustomParticles.EnigmaEyeGaze(
                        npc.Center + eyeOffset,
                        EerieGreen * 0.5f, 0.35f);
                }
            }

            // Orbiting glyphs — Phase 1 gets sparse hints, Phase 2+ gets proper circles
            if (difficultyTier == 0)
            {
                // Phase 1: rare lone glyph at periphery
                if (timer % 120 == 0)
                    CustomParticles.Glyph(npc.Center + Main.rand.NextVector2Circular(100f, 100f), DeepPurple * 0.3f, 0.1f);
            }
            else
            {
                int glyphInterval = (int)MathHelper.Lerp(90, 40, hpDrive);
                if (enraged) glyphInterval = Math.Max(1, glyphInterval / 2);
                if (timer % Math.Max(1, glyphInterval) == 0)
                    CustomParticles.GlyphCircle(npc.Center, Color.Lerp(DeepPurple, EerieGreen, hpDrive), 4, 60f, 0.02f);
            }

            // Phase 2+: Orbiting bloom particles — eerie green orbit
            if (difficultyTier >= 1 && timer % 6 == 0)
            {
                float angle = timer * 0.05f;
                float orbitRadius = MathHelper.Lerp(60f, 100f, hpDrive);
                Vector2 orbitPos = npc.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * orbitRadius;
                Color bloomColor = Color.Lerp(DeepPurple, EerieGreen, hpDrive);
                MagnumParticleHandler.SpawnParticle(new BloomParticle(
                    orbitPos, Main.rand.NextVector2Circular(0.5f, 0.5f),
                    bloomColor * 0.5f, MathHelper.Lerp(0.3f, 0.6f, hpDrive), 30));
            }

            // Phase 3: Ascending void sparks — the revelation leaks
            if (difficultyTier >= 2 && timer % 8 == 0)
            {
                Vector2 sparkPos = npc.Center + Main.rand.NextVector2Circular(100f, 100f);
                Color sparkColor = Color.Lerp(EerieGreen, ArcaneGreen, Main.rand.NextFloat());
                MagnumParticleHandler.SpawnParticle(new SparkleParticle(
                    sparkPos, new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-2f, -0.8f)),
                    sparkColor * 0.5f, MathHelper.Lerp(0.3f, 0.5f, hpDrive), 40));
            }

            // Enrage: Dense void smoke + unsettling white flashes
            if (enraged)
            {
                if (timer % 4 == 0)
                {
                    var smoke = new HeavySmokeParticle(
                        npc.Center + Main.rand.NextVector2Circular(40f, 40f),
                        Main.rand.NextVector2Circular(1.5f, 1.5f),
                        VoidBlack, Main.rand.Next(15, 30), 0.15f, 0.4f, 0.02f, false);
                    MagnumParticleHandler.SpawnParticle(smoke);
                }
                if (timer % 60 == 0)
                    EnigmaSkySystem.TriggerRevelationFlash(0.1f);
            }
        }
    }
}
