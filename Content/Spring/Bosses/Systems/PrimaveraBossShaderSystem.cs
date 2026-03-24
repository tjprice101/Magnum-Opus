using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Spring.Bosses.Systems
{
    /// <summary>
    /// Primavera phase-aware rendering system — 6-layer boss draw.
    /// Each phase has distinct color palette, bloom intensity, and trail character.
    /// Phase 1 (Dawn): soft pink glow, gentle lavender orbit, thin pastel trail
    /// Phase 2 (Storm): green-infused aura, wind energy, medium green-pink trail
    /// Phase 3 (Full Bloom): intense hot pink bloom, dense floral trail, sparkle cascade
    /// Enrage: aggressive magenta-crimson, jagged high-contrast trail, hostile pulse
    /// </summary>
    public static class PrimaveraBossShaderSystem
    {
        // ===== NEW PALETTE (Vivaldi's Spring) =====
        private static readonly Color CherryPink = new Color(255, 183, 197);
        private static readonly Color FreshGreen = new Color(124, 252, 0);
        private static readonly Color Lavender = new Color(181, 126, 220);
        private static readonly Color HotPink = new Color(255, 105, 180);
        private static readonly Color ViolentMagenta = new Color(255, 0, 255);
        private static readonly Color DeepCrimson = new Color(139, 0, 0);
        private static readonly Color WarmWhite = new Color(255, 250, 240);
        private static readonly Color WarmAmber = new Color(255, 215, 120);

        private static int GetPhase(NPC npc)
        {
            float ratio = (float)npc.life / npc.lifeMax;
            if (ratio > 0.6f) return 0;
            if (ratio > 0.3f) return 1;
            return 2;
        }

        // Per-phase color tables
        private static void GetPhaseColors(int phase, bool isEnraged,
            out Color outerGlow, out Color midGlow, out Color coreGlow,
            out float pulseSpeed, out float bloomScale)
        {
            if (isEnraged)
            {
                outerGlow = DeepCrimson;
                midGlow = ViolentMagenta;
                coreGlow = WarmWhite;
                pulseSpeed = 0.09f;
                bloomScale = 1.8f;
                return;
            }

            switch (phase)
            {
                case 0: // Dawn — soft, warm
                    outerGlow = Lavender;
                    midGlow = CherryPink;
                    coreGlow = WarmWhite;
                    pulseSpeed = 0.035f;
                    bloomScale = 1.4f;
                    break;
                case 1: // Storm — green energy mixed
                    outerGlow = FreshGreen;
                    midGlow = Color.Lerp(CherryPink, FreshGreen, 0.4f);
                    coreGlow = WarmWhite;
                    pulseSpeed = 0.05f;
                    bloomScale = 1.6f;
                    break;
                default: // Full Bloom — intense pink
                    outerGlow = HotPink;
                    midGlow = CherryPink;
                    coreGlow = Color.White;
                    pulseSpeed = 0.07f;
                    bloomScale = 2.0f;
                    break;
            }
        }

        /// <summary>
        /// LAYER 1: Ambient boss glow — 3-layer bloom stack, phase-colored.
        /// </summary>
        public static void DrawBossGlow(SpriteBatch sb, NPC npc, Vector2 screenPos, bool isEnraged)
        {
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            Vector2 drawPos = npc.Center - screenPos;
            Vector2 origin = glow.Size() / 2f;
            float time = (float)Main.timeForVisualEffects;
            int phase = GetPhase(npc);

            GetPhaseColors(phase, isEnraged, out Color outer, out Color mid, out Color core,
                out float pulseSpeed, out float bloomScale);

            float pulse = 1f + (float)Math.Sin(time * pulseSpeed) * 0.08f;

            // Outer bloom
            Color outerC = outer * 0.2f; outerC.A = 0;
            sb.Draw(glow, drawPos, null, outerC, 0f, origin, bloomScale * pulse, SpriteEffects.None, 0f);

            // Mid bloom
            Color midC = mid * 0.3f; midC.A = 0;
            float midPulse = 1f + (float)Math.Sin(time * pulseSpeed * 1.5f) * 0.1f;
            sb.Draw(glow, drawPos, null, midC, 0f, origin, bloomScale * 0.65f * midPulse, SpriteEffects.None, 0f);

            // Core
            Color coreC = core * 0.4f; coreC.A = 0;
            sb.Draw(glow, drawPos, null, coreC, 0f, origin, bloomScale * 0.35f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// LAYER 2: Bloom aura with shader + 3-layer stacked bloom, phase intensity.
        /// </summary>
        public static void DrawBloomAura(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float aggressionLevel, int difficultyTier, bool isEnraged)
        {
            int phase = GetPhase(npc);
            // Derive actual enrage from npc state since caller always passes false
            bool actualEnraged = PrimaveraSky.BossIsEnraged;

            GetPhaseColors(phase, actualEnraged, out Color outer, out Color mid, out Color core,
                out float pulseSpeed, out float bloomScale);

            float intensity = 0.3f + aggressionLevel * 0.5f + difficultyTier * 0.15f;
            if (actualEnraged) intensity *= 1.4f;

            // Shader aura pass
            Color primary = actualEnraged ? ViolentMagenta : (phase == 1 ? FreshGreen : CherryPink);
            Color secondary = actualEnraged ? DeepCrimson : (phase == 1 ? CherryPink : Lavender);

            BossRenderHelper.DrawShaderAura(sb, npc, screenPos,
                BossShaderManager.PrimaveraBloomAura, primary, secondary,
                85f + difficultyTier * 20f, intensity,
                (float)Main.timeForVisualEffects * 0.02f);

            // Additional 3-layer bloom
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            Vector2 drawPos = npc.Center - screenPos;
            Vector2 origin = glow.Size() / 2f;
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + (float)Math.Sin(time * pulseSpeed) * 0.1f;

            Color outerBloom = outer * (0.18f * intensity); outerBloom.A = 0;
            sb.Draw(glow, drawPos, null, outerBloom, 0f, origin, 1.95f * pulse, SpriteEffects.None, 0f);

            Color midBloom = mid * (0.22f * intensity); midBloom.A = 0;
            float mp = 1f + (float)Math.Sin(time * pulseSpeed * 1.3f) * 0.12f;
            sb.Draw(glow, drawPos, null, midBloom, 0f, origin, 1.3f * mp, SpriteEffects.None, 0f);

            Color coreBloom = core * (0.28f * intensity); coreBloom.A = 0;
            sb.Draw(glow, drawPos, null, coreBloom, 0f, origin, 0.585f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// LAYER 3: Petal trail — phase changes trail character.
        /// Phase 1: thin pastel. Phase 2: green-pink mixed. Phase 3: dense floral. Enrage: jagged magenta.
        /// </summary>
        public static void DrawPetalTrail(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, bool isEnraged)
        {
            int phase = GetPhase(npc);
            bool actualEnraged = PrimaveraSky.BossIsEnraged;

            Color trailColor;
            float width;
            float scrollSpeed;

            if (actualEnraged)
            {
                trailColor = ViolentMagenta;
                width = 1.6f;
                scrollSpeed = 0.04f;
            }
            else switch (phase)
            {
                case 0:
                    trailColor = CherryPink * 0.6f;
                    width = 0.7f;
                    scrollSpeed = 0.015f;
                    break;
                case 1:
                    trailColor = Color.Lerp(CherryPink, FreshGreen, 0.35f);
                    width = 1.1f;
                    scrollSpeed = 0.025f;
                    break;
                default:
                    trailColor = HotPink;
                    width = 1.5f;
                    scrollSpeed = 0.03f;
                    break;
            }

            BossRenderHelper.DrawShaderTrail(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.PrimaveraPetalTrail, trailColor, width,
                (float)Main.timeForVisualEffects * scrollSpeed, 5f);
        }

        /// <summary>
        /// LAYER 4: Growth pulse flash on heal attacks.
        /// </summary>
        public static void DrawGrowthPulse(SpriteBatch sb, Vector2 position, Vector2 screenPos,
            float intensity)
        {
            BossRenderHelper.DrawAttackFlash(sb, position, screenPos,
                BossShaderManager.PrimaveraGrowthPulse, FreshGreen, intensity,
                (float)Main.timeForVisualEffects * 0.025f);
        }

        /// <summary>
        /// LAYER 5: Vernal storm phase transition effect.
        /// </summary>
        public static void DrawVernalStorm(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float transitionProgress, bool isPhase2Transition)
        {
            Color from = isPhase2Transition ? CherryPink : FreshGreen;
            Color to = isPhase2Transition ? WarmAmber : HotPink;
            float intensity = isPhase2Transition ? 1.2f : 1.0f;

            BossRenderHelper.DrawPhaseTransition(sb, npc, screenPos,
                BossShaderManager.PrimaveraVernalStorm, transitionProgress,
                from, to, intensity);
        }

        /// <summary>
        /// LAYER 6: Rebirth dissolve with phase-colored edge glow.
        /// </summary>
        public static void DrawRebirthDissolve(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, float dissolveProgress)
        {
            BossRenderHelper.DrawDissolve(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.PrimaveraRebirthDissolve, dissolveProgress,
                CherryPink, 0.06f);

            if (dissolveProgress > 0.2f)
            {
                Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
                Vector2 drawPos = npc.Center - screenPos;
                Vector2 glowOrigin = glow.Size() / 2f;

                float edgeT = (dissolveProgress - 0.2f) / 0.8f;
                Color edgeColor = Color.Lerp(CherryPink, Color.White, edgeT) * (edgeT * 0.6f);
                edgeColor.A = 0;
                sb.Draw(glow, drawPos, null, edgeColor, 0f, glowOrigin,
                    1.5f + edgeT * 2f, SpriteEffects.None, 0f);

                // Phase 3 death: additional hot pink halo
                if (dissolveProgress > 0.5f)
                {
                    Color haloC = HotPink * ((dissolveProgress - 0.5f) * 0.4f);
                    haloC.A = 0;
                    sb.Draw(glow, drawPos, null, haloC, 0f, glowOrigin,
                        3f + dissolveProgress * 2f, SpriteEffects.None, 0f);
                }
            }
        }

        /// <summary>
        /// Phase-aware musical accents and ambient particles around the boss.
        /// Phase 1: gentle pollen orbit, occasional music notes
        /// Phase 2: wind-driven green sparkles, more frequent notes
        /// Phase 3: dense bloom orbit, sparkle cascade, ground flowers
        /// Enrage: aggressive magenta sparks, discordant notes
        /// </summary>
        public static void SpawnMusicalAccents(NPC npc, int timer, int difficultyTier)
        {
            float hpDrive = 1f - (float)npc.life / npc.lifeMax;
            int phase = GetPhase(npc);
            bool enraged = PrimaveraSky.BossIsEnraged;

            // ===== ORBITING BLOOM PETALS =====
            int orbitCount = phase switch { 0 => 5, 1 => 8, 2 => 12, _ => 8 };
            float orbitRadius = phase switch { 0 => 60f, 1 => 55f + hpDrive * 20f, 2 => 45f + hpDrive * 30f, _ => 40f };
            float orbitSpeed = enraged ? 0.11f : (0.04f + phase * 0.02f);

            if (timer % 8 == 0)
            {
                for (int i = 0; i < orbitCount; i++)
                {
                    if (timer % (8 * orbitCount) != 8 * i) continue;
                    float angle = timer * orbitSpeed + MathHelper.TwoPi * i / orbitCount;

                    // Enrage: erratic orbit
                    float r = enraged
                        ? orbitRadius + Main.rand.NextFloat(-20f, 20f)
                        : orbitRadius + (float)Math.Sin(timer * 0.03f + i) * 10f;

                    Vector2 orbitPos = npc.Center + angle.ToRotationVector2() * r;
                    Color orbitColor = enraged
                        ? Color.Lerp(ViolentMagenta, DeepCrimson, Main.rand.NextFloat())
                        : phase switch
                        {
                            0 => Color.Lerp(CherryPink, WarmAmber, (float)Math.Sin(timer * 0.04f + i) * 0.5f + 0.5f),
                            1 => Color.Lerp(CherryPink, FreshGreen, (float)Math.Sin(timer * 0.05f + i) * 0.5f + 0.5f),
                            _ => Color.Lerp(HotPink, Color.White, (float)Math.Sin(timer * 0.06f + i) * 0.5f + 0.5f)
                        };

                    float scale = enraged ? 0.2f : (0.15f + phase * 0.05f);
                    MagnumParticleHandler.SpawnParticle(new BloomParticle(
                        orbitPos, Vector2.Zero, orbitColor, scale, 10));
                }
            }

            // ===== MUSICAL NOTES =====
            int noteInterval = enraged ? 20 : phase switch { 0 => 90, 1 => 50, 2 => 30, _ => 20 };
            if (timer % noteInterval == 0)
            {
                Color noteColor = enraged ? ViolentMagenta : phase switch
                {
                    0 => CherryPink,
                    1 => FreshGreen,
                    _ => HotPink
                };
                int noteCount = enraged ? 2 : (1 + phase);
                CustomParticles.GenericMusicNotes(npc.Center, noteColor, noteCount, 40f + phase * 10f);
            }

            // ===== POLLEN / SPARKLE =====
            int pollenInterval = Math.Max(2, 10 - phase * 2 - (enraged ? 3 : 0));
            if (timer % pollenInterval == 0)
            {
                Vector2 offset = Main.rand.NextVector2Circular(70f + phase * 20f, 70f + phase * 20f);
                Color pollenColor = enraged
                    ? Color.Lerp(ViolentMagenta, DeepCrimson, Main.rand.NextFloat())
                    : phase switch
                    {
                        0 => Color.Lerp(WarmAmber, CherryPink, Main.rand.NextFloat()),
                        1 => Color.Lerp(FreshGreen, CherryPink, Main.rand.NextFloat()),
                        _ => Color.Lerp(CherryPink, Color.White, Main.rand.NextFloat())
                    };
                CustomParticles.GenericFlare(npc.Center + offset, pollenColor,
                    0.12f + phase * 0.05f, 30 + phase * 5);
            }

            // ===== ASCENDING SPARKLES (Phase 2+) =====
            if (phase >= 1 && timer % (enraged ? 8 : 14) == 0)
            {
                Vector2 sparkPos = npc.Center + Main.rand.NextVector2Circular(50f, 50f);
                float upSpeed = enraged ? -Main.rand.NextFloat(2.5f, 5f) : -Main.rand.NextFloat(1.5f, 3f);
                Vector2 sparkVel = new Vector2(Main.rand.NextFloat(-0.7f, 0.7f), upSpeed);
                Color sparkColor = enraged ? DeepCrimson : (phase >= 2 ? HotPink : FreshGreen);
                MagnumParticleHandler.SpawnParticle(new SparkleParticle(
                    sparkPos, sparkVel, sparkColor, 0.2f + phase * 0.05f, 22));
            }

            // ===== STAFF LINE CONVERGENCE (musical flavor) =====
            int staffInterval = Math.Max(4, 80 - (int)(hpDrive * 40));
            if (timer % staffInterval == 0)
            {
                Color staffColor = enraged ? ViolentMagenta : CherryPink;
                Phase10BossVFX.StaffLineConvergence(npc.Center, staffColor,
                    0.4f + difficultyTier * 0.2f);
            }

            // ===== PHASE 2+: METRONOME TICKS =====
            if (phase >= 1 && timer % Math.Max(2, 45 - phase * 10) == 0)
            {
                Color metColor = enraged ? DeepCrimson : FreshGreen;
                Phase10BossVFX.MetronomeTickWarning(npc.Center, metColor, 3, 6);
            }

            // ===== PHASE 3: BLOOM BURST =====
            if (phase >= 2 && timer % Math.Max(4, 60 - (int)(hpDrive * 25)) == 0)
                BossSignatureVFX.SpringBloomBurst(npc.Center, enraged ? 1.3f : 1.0f);
        }
    }
}
