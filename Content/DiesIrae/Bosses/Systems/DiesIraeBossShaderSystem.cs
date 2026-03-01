using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.DiesIrae.Bosses.Systems
{
    /// <summary>
    /// Dies Irae boss shader-driven rendering system.
    /// Manages all 5 boss shaders: DiesHellfireAura, DiesJudgmentTrail,
    /// DiesApocalypseRay, DiesWrathEscalation, and DiesFinalJudgmentDissolve.
    ///
    /// Dies Irae is post-Nachtmusik  Eeffects should feel apocalyptic and overwhelming.
    /// DiesWrathEscalation is unique  Ea spreading vein pattern that intensifies
    /// with each HP tier, visually conveying accumulated wrath.
    /// </summary>
    public static class DiesIraeBossShaderSystem
    {
        // Theme colors  Eblood, fire, judgment, and ash
        private static readonly Color BloodRed = new Color(200, 30, 20);
        private static readonly Color DarkCrimson = new Color(120, 15, 15);
        private static readonly Color EmberOrange = new Color(220, 100, 30);
        private static readonly Color AshenBlack = new Color(25, 15, 10);

        /// <summary>
        /// Draws the hellfire aura  Epulsing blood-red energy field
        /// with ember sparks. Intensifies dramatically with each HP tier.
        /// </summary>
        public static void DrawHellfireAura(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float aggressionLevel, int hpTier, bool isEnraged)
        {
            float baseRadius = 90f + hpTier * 25f;
            float intensity = 0.4f + aggressionLevel * 0.4f + hpTier * 0.15f;
            if (isEnraged)
            {
                intensity *= 1.7f;
                baseRadius *= 1.3f;
            }

            Color primary = isEnraged ? EmberOrange : BloodRed;
            Color secondary = isEnraged ? BloodRed : DarkCrimson;

            BossRenderHelper.DrawShaderAura(sb, npc, screenPos,
                BossShaderManager.DiesHellfireAura, primary, secondary,
                baseRadius, intensity, (float)Main.timeForVisualEffects * 0.025f);
        }

        /// <summary>
        /// Draws the judgment trail during charges  Ehellfire wake
        /// with ash particles trailing behind.
        /// </summary>
        public static void DrawJudgmentTrail(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, bool isEnraged)
        {
            Color trailColor = isEnraged ? EmberOrange : BloodRed;
            float width = isEnraged ? 1.7f : 1.2f;

            BossRenderHelper.DrawShaderTrail(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.DiesJudgmentTrail, trailColor, width,
                (float)Main.timeForVisualEffects * 0.03f, 7f);
        }

        /// <summary>
        /// Draws the apocalypse ray  Eenormous beam of divine wrath.
        /// </summary>
        public static void DrawApocalypseRay(SpriteBatch sb, Vector2 position, Vector2 screenPos,
            float intensity)
        {
            BossRenderHelper.DrawAttackFlash(sb, position, screenPos,
                BossShaderManager.DiesApocalypseRay, BloodRed, intensity,
                (float)Main.timeForVisualEffects * 0.04f);
        }

        /// <summary>
        /// Draws the wrath escalation effect  Eunique spreading vein pattern.
        /// Veins spread across the boss's body with each HP tier,
        /// glowing with increasing intensity. At max tier, the boss
        /// appears to be cracking apart with hellfire bleeding through.
        /// </summary>
        public static void DrawWrathEscalation(SpriteBatch sb, NPC npc, Vector2 screenPos,
            int hpTier, float escalationProgress)
        {
            // Tier 0-4 maps to increasing vein coverage
            float veinCoverage = hpTier * 0.2f + escalationProgress * 0.1f;
            Color veinColor = Color.Lerp(DarkCrimson, EmberOrange, veinCoverage);
            float veinIntensity = 0.5f + veinCoverage * 1.5f;

            BossRenderHelper.DrawPhaseTransition(sb, npc, screenPos,
                BossShaderManager.DiesWrathEscalation, veinCoverage,
                DarkCrimson, EmberOrange, veinIntensity);

            // At high tiers, spawn ember sparks from the veins
            if (hpTier >= 3 && Main.rand.NextBool(3))
            {
                Vector2 sparkPos = npc.Center + Main.rand.NextVector2Circular(
                    npc.width * 0.5f, npc.height * 0.5f);
                CustomParticles.GenericFlare(sparkPos, EmberOrange, 0.2f + hpTier * 0.05f, 10);
            }
        }

        /// <summary>
        /// Draws the final judgment dissolve  Ethe Herald disintegrates
        /// into ash and hellfire embers.
        /// </summary>
        public static void DrawFinalJudgmentDissolve(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, float dissolveProgress)
        {
            BossRenderHelper.DrawDissolve(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.DiesFinalJudgmentDissolve, dissolveProgress,
                BloodRed, 0.05f);
        }

        /// <summary>
        /// Spawns musical VFX particles  Ewrathful drumrolls, judgment fanfares,
        /// and hellfire rhythm pulses. Each HP tier adds more intensity.
        /// </summary>
        public static void SpawnMusicalAccents(NPC npc, int timer, int hpTier, bool isEnraged)
        {
            // Timpani drumroll impacts on rhythm
            if (timer % 80 == 0)
            {
                Phase10BossVFX.TimpaniDrumrollImpact(npc.Center, BloodRed, 0.6f + hpTier * 0.2f);
            }

            // Sforzando spikes on attack beats
            if (timer % 50 == 0 && hpTier >= 1)
            {
                Phase10BossVFX.SforzandoSpike(npc.Center, EmberOrange, 0.5f + hpTier * 0.15f);
            }

            // Hellfire burst accents during high intensity
            if (timer % 40 == 0 && hpTier >= 2)
            {
                CustomParticles.DiesIraeHellfireBurst(npc.Center, 4 + hpTier * 2);
            }

            // Enraged  Econstant ember shower
            if (isEnraged && timer % 15 == 0)
            {
                Vector2 sparkPos = npc.Center + Main.rand.NextVector2Circular(60f, 60f);
                CustomParticles.GenericFlare(sparkPos, EmberOrange, 0.25f, 12);
            }

            // Dissonance storm at high HP tiers
            if (timer % 120 == 0 && hpTier >= 3)
            {
                Phase10BossVFX.DissonanceStorm(npc.Center, 100f, BloodRed, EmberOrange);
            }
        }
    }
}
