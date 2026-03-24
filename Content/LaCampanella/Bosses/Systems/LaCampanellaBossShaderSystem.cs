using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.LaCampanella.Bosses.Systems
{
    /// <summary>
    /// La Campanella boss shader-driven rendering system — phase-aware redesign.
    ///
    /// Phase 1 (First Toll): Smoky wreath around the bell with orange fire flickering through gaps.
    ///         Calm, singular — each layer breathes slowly.
    /// Phase 2 (Accelerando): Smoke thickens and churns, fire becomes more agitated in the wreath.
    ///         Trail afterimages are frantic, leaving dense fire streams.
    /// Phase 3 (Virtuoso Cascade): Smoke wreath fully aflame, constant fire pulsing through.
    ///         The bell is barely visible through the infernal shroud.
    /// Enrage (Bell Cracking): Fractured visual identity — glow flickers erratically,
    ///         smoke is all-consuming, fire bleeds violently through cracks in the aura.
    ///
    /// Palette: SootBlack(20,15,20), DeepEmber(180,60,0), InfernalOrange(255,100,0),
    ///          FlameYellow(255,200,50), BellGold(218,165,32), FlameWhite(255,230,200).
    /// </summary>
    public static class LaCampanellaBossShaderSystem
    {
        // Canonical palette — matches AttackVFX
        private static readonly Color SootBlack = new Color(20, 15, 20);
        private static readonly Color DeepEmber = new Color(180, 60, 0);
        private static readonly Color InfernalOrange = new Color(255, 100, 0);
        private static readonly Color FlameYellow = new Color(255, 200, 50);
        private static readonly Color BellGold = new Color(218, 165, 32);
        private static readonly Color MoltenGold = new Color(255, 180, 40);
        private static readonly Color FlameWhite = new Color(255, 230, 200);
        private static readonly Color EmberCrimson = new Color(200, 50, 30);

        /// <summary>
        /// Draws the bell's resonance aura — a smoke wreath with fire flickering through.
        /// Phase 1: gentle dark smoke ring, occasional orange fire peek.
        /// Phase 2: thicker churning smoke, agitated fire spilling through gaps.
        /// Phase 3: fully aflame — fire dominates the wreath.
        /// Enrage: fractured — smoke and fire fight violently, erratic flicker.
        /// </summary>
        public static void DrawBellAura(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float aggressionLevel, int difficultyTier, bool isEnraged)
        {
            float time = (float)Main.timeForVisualEffects;
            float breathe = (float)Math.Sin(time * 0.012f) * 0.5f + 0.5f;
            float baseRadius = 90f + difficultyTier * 25f;
            float intensity = 0.35f + aggressionLevel * 0.45f;

            // Phase escalation
            float phaseFireBleed = difficultyTier * 0.25f; // 0, 0.25, 0.5
            if (isEnraged) { intensity *= 1.6f; phaseFireBleed = 0.9f; }

            // Shader aura — shifts from smoke-dominant to fire-dominant with phase
            Color primary = Color.Lerp(SootBlack, InfernalOrange, phaseFireBleed);
            Color secondary = isEnraged ? FlameWhite : Color.Lerp(BellGold, FlameYellow, phaseFireBleed);

            BossRenderHelper.DrawShaderAura(sb, npc, screenPos,
                BossShaderManager.CampanellaBellAura, primary, secondary,
                baseRadius, intensity, time * 0.015f);

            // Multi-layer bloom wreath on top of shader
            Texture2D bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (bloomTex == null) return;

            Vector2 drawCenter = npc.Center - screenPos;
            Vector2 bloomOrigin = new Vector2(bloomTex.Width, bloomTex.Height) * 0.5f;

            // Layer 1: Wide dark smoke shroud — always present, thickens with phase
            float smokeScale = MathHelper.Lerp(0.293f, 0.455f, phaseFireBleed) + breathe * 0.04f;
            float smokeAlpha = MathHelper.Lerp(0.07f, 0.14f, phaseFireBleed);
            Color smokeShroud = SootBlack * smokeAlpha;
            smokeShroud.A = 0;
            sb.Draw(bloomTex, drawCenter, null, smokeShroud, 0f, bloomOrigin, smokeScale, SpriteEffects.None, 0f);

            // Layer 2: Fire flickering through smoke gaps — intensifies with phase
            float fireFlicker = (float)Math.Sin(time * 0.035f + npc.whoAmI) * 0.5f + 0.5f;
            if (isEnraged)
                fireFlicker = 0.5f + (float)Math.Sin(time * 0.08f) * 0.3f + (float)Math.Sin(time * 0.13f) * 0.2f;

            float fireScale = MathHelper.Lerp(0.163f, 0.325f, phaseFireBleed) + fireFlicker * 0.05f;
            float fireAlpha = MathHelper.Lerp(0.04f, 0.15f, phaseFireBleed) * (0.7f + fireFlicker * 0.3f);
            Color fireGlow = InfernalOrange * fireAlpha;
            fireGlow.A = 0;
            sb.Draw(bloomTex, drawCenter, null, fireGlow, 0f, bloomOrigin, fireScale, SpriteEffects.None, 0f);

            // Layer 3: White-hot core — only in Phase 3 and Enrage
            if (difficultyTier >= 2 || isEnraged)
            {
                float coreFlicker = isEnraged
                    ? 0.5f + (float)Math.Sin(time * 0.1f) * 0.5f
                    : 0.7f + breathe * 0.3f;
                Color coreBloom = FlameWhite * (0.08f + phaseFireBleed * 0.08f) * coreFlicker;
                coreBloom.A = 0;
                sb.Draw(bloomTex, drawCenter, null, coreBloom, 0f, bloomOrigin, 0.15f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draws 3-layer NPC bloom glow — HP-driven intensification.
        /// Called before main sprite in PreDraw.
        /// At low HP, the glow becomes more agitated and fire-colored.
        /// During enrage, glow flickers erratically.
        /// </summary>
        public static void DrawBossGlow(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float lifeRatio, bool isEnraged)
        {
            Texture2D bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (bloomTex == null) return;

            Vector2 drawCenter = npc.Center - screenPos;
            Vector2 bloomOrigin = new Vector2(bloomTex.Width, bloomTex.Height) * 0.5f;
            float hpDrive = 1f - lifeRatio;
            float time = (float)Main.timeForVisualEffects;
            float breathe = (float)Math.Sin(time * 0.01f) * 0.5f + 0.5f;

            // Enrage erratic flicker
            float enrageFlicker = 1f;
            if (isEnraged)
                enrageFlicker = 0.6f + (float)Math.Sin(time * 0.07f) * 0.2f + (float)Math.Sin(time * 0.15f) * 0.2f;

            // Layer 1: Wide smoky outer
            float outerScale = MathHelper.Lerp(0.228f, 0.358f, hpDrive) + breathe * 0.03f;
            Color outerColor = Color.Lerp(SootBlack, DeepEmber, hpDrive * 0.4f)
                * MathHelper.Lerp(0.06f, 0.14f, hpDrive) * enrageFlicker;
            outerColor.A = 0;
            sb.Draw(bloomTex, drawCenter, null, outerColor, 0f, bloomOrigin, outerScale, SpriteEffects.None, 0f);

            // Layer 2: Orange mid — fire intensity grows with damage
            float midScale = MathHelper.Lerp(0.13f, 0.228f, hpDrive) + breathe * 0.02f;
            Color midColor = Color.Lerp(InfernalOrange, FlameYellow, hpDrive * 0.3f)
                * MathHelper.Lerp(0.08f, 0.18f, hpDrive) * enrageFlicker;
            midColor.A = 0;
            sb.Draw(bloomTex, drawCenter, null, midColor, 0f, bloomOrigin, midScale, SpriteEffects.None, 0f);

            // Layer 3: White-hot core
            float coreScale = MathHelper.Lerp(0.052f, 0.117f, hpDrive);
            Color coreColor = Color.Lerp(BellGold, FlameWhite, hpDrive)
                * MathHelper.Lerp(0.1f, 0.22f, hpDrive);
            if (isEnraged)
                coreColor = FlameWhite * (0.15f + (float)Math.Sin(time * 0.12f) * 0.1f);
            coreColor.A = 0;
            sb.Draw(bloomTex, drawCenter, null, coreColor, 0f, bloomOrigin, coreScale, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Draws infernal smoke trail with glow overlay at each afterimage position.
        /// Phase 2+: trail is more frantic with denser glow.
        /// Enrage: trail flickers and has fire bleeding through.
        /// </summary>
        public static void DrawInfernalTrail(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin,
            bool isEnraged, int difficultyTier = 0)
        {
            Color trailColor = isEnraged ? InfernalOrange : SootBlack;
            float width = isEnraged ? 1.6f : 1.1f;
            if (difficultyTier >= 1) width += 0.2f;

            float scrollSpeed = 0.025f + difficultyTier * 0.008f;
            if (isEnraged) scrollSpeed = 0.045f;

            BossRenderHelper.DrawShaderTrail(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.CampanellaInfernalTrail, trailColor, width,
                (float)Main.timeForVisualEffects * scrollSpeed, 5f);

            // Glow orbs at afterimage positions
            Texture2D bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (bloomTex == null) return;

            Vector2 bloomOrigin = new Vector2(bloomTex.Width, bloomTex.Height) * 0.5f;
            int step = difficultyTier >= 2 ? 1 : 2;

            for (int i = 0; i < npc.oldPos.Length; i += step)
            {
                float progress = (float)i / npc.oldPos.Length;
                Vector2 pos = npc.oldPos[i] + npc.Size / 2f - screenPos;

                // Fire glow intensifies in later phases
                float glowAlpha = (1f - progress) * MathHelper.Lerp(0.12f, 0.2f, difficultyTier * 0.35f);
                if (isEnraged)
                    glowAlpha *= 1.4f;

                Color glowCol = Color.Lerp(InfernalOrange, MoltenGold, progress) * glowAlpha;
                glowCol.A = 0;
                float glowScale = 0.08f * (1f - progress) + difficultyTier * 0.015f;
                sb.Draw(bloomTex, pos, null, glowCol, 0f, bloomOrigin, glowScale, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draws resonance wave rings after bell toll/slam attacks.
        /// </summary>
        public static void DrawResonanceWave(SpriteBatch sb, Vector2 position, Vector2 screenPos,
            float intensity)
        {
            BossRenderHelper.DrawAttackFlash(sb, position, screenPos,
                BossShaderManager.CampanellaResonanceWave, InfernalOrange, intensity,
                (float)Main.timeForVisualEffects * 0.03f);
        }

        /// <summary>
        /// Draws the infernal firewall during phase transitions.
        /// Phase 2 transition: orange → white-hot.
        /// Phase 3 transition: MORE intense, crimson edges.
        /// </summary>
        public static void DrawFirewall(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float transitionProgress, bool isPhase2Transition)
        {
            Color from = isPhase2Transition ? InfernalOrange : EmberCrimson;
            Color to = isPhase2Transition ? FlameWhite : InfernalOrange;
            float intensity = isPhase2Transition ? 1.4f : 1.0f;

            BossRenderHelper.DrawPhaseTransition(sb, npc, screenPos,
                BossShaderManager.CampanellaFirewall, transitionProgress,
                from, to, intensity);

            // Sky flash at transition midpoint
            if (transitionProgress > 0.4f && transitionProgress < 0.5f)
            {
                LaCampanellaSkySystem.TriggerWhiteFlash(isPhase2Transition ? 0.7f : 0.5f);
            }
        }

        /// <summary>
        /// Draws the chime dissolve death animation — bell shattering into embers.
        /// Edge glow widens as dissolution progresses. Fire and smoke pour from the cracks.
        /// </summary>
        public static void DrawChimeDissolve(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, float dissolveProgress)
        {
            float edgeWidth = MathHelper.Lerp(0.05f, 0.14f, dissolveProgress);
            Color edgeColor = Color.Lerp(InfernalOrange, FlameWhite, dissolveProgress * 0.6f);

            BossRenderHelper.DrawDissolve(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.CampanellaChimeDissolve, dissolveProgress,
                edgeColor, edgeWidth);

            // Ambient bloom intensifies during dissolution
            Texture2D bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (bloomTex != null)
            {
                Vector2 bloomOrigin = new Vector2(bloomTex.Width, bloomTex.Height) * 0.5f;
                Vector2 drawCenter = npc.Center - screenPos;
                Color ambientBloom = InfernalOrange * (dissolveProgress * 0.2f);
                ambientBloom.A = 0;
                sb.Draw(bloomTex, drawCenter, null, ambientBloom, 0f, bloomOrigin,
                    0.3f + dissolveProgress * 0.25f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draws enrage fracture overlay — erratic bloom flickers simulating cracks in the bell.
        /// Called from PreDraw when isEnraged is true.
        /// </summary>
        public static void DrawEnrageFractureOverlay(SpriteBatch sb, NPC npc, Vector2 screenPos)
        {
            Texture2D bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (bloomTex == null) return;

            float time = (float)Main.timeForVisualEffects;
            Vector2 drawCenter = npc.Center - screenPos;
            Vector2 bloomOrigin = new Vector2(bloomTex.Width, bloomTex.Height) * 0.5f;

            // Erratic fire-crack flickers at random offsets around the bell
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f + time * 0.02f;
                float dist = 15f + (float)Math.Sin(time * 0.1f + i * 1.7f) * 10f;
                Vector2 crackPos = drawCenter + angle.ToRotationVector2() * dist;
                float crackAlpha = 0.08f + (float)Math.Sin(time * (0.05f + i * 0.03f)) * 0.06f;
                Color crackColor = Color.Lerp(InfernalOrange, FlameWhite, (float)Math.Sin(time * 0.08f + i) * 0.5f + 0.5f) * crackAlpha;
                crackColor.A = 0;
                sb.Draw(bloomTex, crackPos, null, crackColor, 0f, bloomOrigin, 0.06f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Spawns musical VFX particles — phase-driven, HP-reactive.
        ///
        /// Phase 1: gentle golden bloom particles, slow staff line convergence.
        /// Phase 2: faster ambient fire particles, smoke wisps, rapid bell ticks.
        /// Phase 3: constant fire motes, dense smoke, crescendo danger rings.
        /// Enrage: violent bloom bursts, infernal tremolo, choking smoke.
        /// </summary>
        public static void SpawnMusicalAccents(NPC npc, int timer, int difficultyTier, bool isEnraged)
        {
            float hpRatio = npc.life / (float)npc.lifeMax;
            float hpDrive = 1f - hpRatio;

            // Bell resonance — converging staff lines, more frequent at lower HP
            int staffInterval = (int)MathHelper.Lerp(140, 60, hpDrive);
            if (difficultyTier >= 2) staffInterval = (int)(staffInterval * 0.6f);
            if (timer % staffInterval == 0)
            {
                Phase10BossVFX.StaffLineConvergence(npc.Center, InfernalOrange, 0.5f + difficultyTier * 0.2f);
            }

            // Ambient golden bloom particles — phase-driven frequency
            int bloomInterval = Math.Max(5, 25 - difficultyTier * 7);
            if (timer % bloomInterval == 0)
            {
                Vector2 bloomPos = npc.Center + Main.rand.NextVector2Circular(50f + difficultyTier * 15f, 50f + difficultyTier * 15f);
                Vector2 bloomVel = new Vector2(0, -1f - difficultyTier * 0.3f) + Main.rand.NextVector2Circular(0.5f, 0.3f);
                Color bloomColor = Color.Lerp(BellGold, InfernalOrange, Main.rand.NextFloat() * (0.3f + difficultyTier * 0.2f));
                MagnumParticleHandler.SpawnParticle(new BloomParticle(bloomPos, bloomVel, bloomColor,
                    0.2f + difficultyTier * 0.05f, 30));
            }

            // Rhythmic bell tick warnings
            int tickInterval = Math.Max(10, 35 - difficultyTier * 10);
            if (timer % tickInterval == 0)
            {
                Phase10BossVFX.MetronomeTickWarning(npc.Center, BellGold, 1 + difficultyTier, 4 + difficultyTier);
            }

            // === Phase 2+ (difficultyTier >= 1): Fire particles and smoke wisps ===
            if (difficultyTier >= 1)
            {
                // Ambient fire motes
                if (timer % (18 - difficultyTier * 4) == 0)
                {
                    Vector2 firePos = npc.Center + Main.rand.NextVector2Circular(40f, 40f);
                    Vector2 fireVel = new Vector2(Main.rand.NextFloat(-1f, 1f), -1.5f - Main.rand.NextFloat(1f));
                    Color fireCol = Color.Lerp(InfernalOrange, FlameYellow, Main.rand.NextFloat() * 0.3f);
                    MagnumParticleHandler.SpawnParticle(new BloomParticle(firePos, fireVel, fireCol, 0.15f, 18));
                }

                // Smoke wisps trailing the bell
                if (timer % 12 == 0)
                {
                    Vector2 smokePos = npc.Center + Main.rand.NextVector2Circular(30f, 30f);
                    Vector2 smokeVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -0.8f);
                    var smoke = new HeavySmokeParticle(smokePos, smokeVel, SootBlack,
                        Main.rand.Next(25, 40), 0.2f, 0.5f, 0.008f, false);
                    MagnumParticleHandler.SpawnParticle(smoke);
                }
            }

            // === Phase 3 (difficultyTier >= 2): Crescendo danger, more fire ===
            if (difficultyTier >= 2)
            {
                if (timer % 70 == 0)
                {
                    Phase10BossVFX.CrescendoDangerRings(npc.Center, EmberCrimson, 0.8f);
                }
            }

            // === Enrage: violent particles, infernal tremolo, choking smoke ===
            if (isEnraged)
            {
                // Violent infernal bloom bursts
                if (timer % 8 == 0)
                {
                    Vector2 burstPos = npc.Center + Main.rand.NextVector2Circular(60f, 60f);
                    Vector2 burstVel = Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0, -1f);
                    Color burstCol = Color.Lerp(InfernalOrange, FlameWhite, Main.rand.NextFloat() * 0.4f);
                    MagnumParticleHandler.SpawnParticle(new BloomParticle(burstPos, burstVel, burstCol, 0.3f, 20));
                }

                // Choking black smoke
                if (timer % 6 == 0)
                {
                    Vector2 smokePos = npc.Center + Main.rand.NextVector2Circular(50f, 50f);
                    Vector2 smokeVel = Main.rand.NextVector2Circular(1.5f, 1.5f) + new Vector2(0, -0.5f);
                    var smoke = new HeavySmokeParticle(smokePos, smokeVel, SootBlack,
                        Main.rand.Next(35, 55), 0.4f, 1.0f, 0.013f, false);
                    MagnumParticleHandler.SpawnParticle(smoke);
                }

                // Infernal tremolo accent
                if (timer % 60 == 0)
                {
                    Phase10Integration.LaCampanella.InfernalTremolo(npc.Center);
                }
            }
        }
    }
}
