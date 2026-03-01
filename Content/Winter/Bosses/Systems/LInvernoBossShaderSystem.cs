using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Winter.Bosses.Systems
{
    /// <summary>
    /// L'Inverno boss shader-driven rendering system.
    /// Manages all 5 boss shaders: FrostAura, IceTrail, BlizzardVortex,
    /// FreezeRay, and AbsoluteZeroDissolve.
    ///
    /// Called from LInverno.PreDraw/PostDraw to layer shader
    /// effects on top of the standard sprite drawing.
    /// </summary>
    public static class LInvernoBossShaderSystem
    {
        // Theme colors
        private static readonly Color FrostBlue = new Color(120, 180, 240);
        private static readonly Color IceWhite = new Color(220, 235, 255);
        private static readonly Color DeepIndigo = new Color(40, 50, 100);
        private static readonly Color CrystalSilver = new Color(190, 200, 220);

        /// <summary>
        /// Draws the ice crystal formation aura with 6-fold symmetry.
        /// Intensity scales with aggression level and HP tier.
        /// </summary>
        public static void DrawFrostAura(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float aggressionLevel, int difficultyTier, bool isEnraged)
        {
            float baseRadius = 85f + difficultyTier * 20f;
            float intensity = 0.3f + aggressionLevel * 0.5f;
            if (isEnraged) intensity *= 1.5f;

            Color primary = isEnraged ? new Color(160, 200, 255) : FrostBlue;
            Color secondary = isEnraged ? new Color(80, 100, 180) : DeepIndigo;

            BossRenderHelper.DrawShaderAura(sb, npc, screenPos,
                BossShaderManager.InvernoFrostAura, primary, secondary,
                baseRadius, intensity, (float)Main.timeForVisualEffects * 0.015f);
        }

        /// <summary>
        /// Draws the frozen shard crystalline trail during dashes.
        /// </summary>
        public static void DrawIceTrail(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, bool isEnraged)
        {
            Color trailColor = isEnraged ? IceWhite : FrostBlue;
            float width = isEnraged ? 1.5f : 1.0f;

            BossRenderHelper.DrawShaderTrail(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.InvernoIceTrail, trailColor, width,
                (float)Main.timeForVisualEffects * 0.018f, 5f);
        }

        /// <summary>
        /// Draws the blizzard vortex attack visual.
        /// </summary>
        public static void DrawBlizzardVortex(SpriteBatch sb, Vector2 position, Vector2 screenPos,
            float intensity)
        {
            BossRenderHelper.DrawAttackFlash(sb, position, screenPos,
                BossShaderManager.InvernoBlizzardVortex, FrostBlue, intensity,
                (float)Main.timeForVisualEffects * 0.02f);
        }

        /// <summary>
        /// Draws the concentrated freeze ray beam effect (Phase 2).
        /// </summary>
        public static void DrawFreezeRay(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float transitionProgress, bool isPhase2Transition)
        {
            Color from = isPhase2Transition ? FrostBlue : CrystalSilver;
            Color to = isPhase2Transition ? IceWhite : new Color(180, 220, 255);
            float intensity = isPhase2Transition ? 1.2f : 0.8f;

            BossRenderHelper.DrawPhaseTransition(sb, npc, screenPos,
                BossShaderManager.InvernoFreezeRay, transitionProgress,
                from, to, intensity);
        }

        /// <summary>
        /// Draws the absolute zero frozen shatter death dissolve.
        /// </summary>
        public static void DrawAbsoluteZeroDissolve(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, float dissolveProgress)
        {
            BossRenderHelper.DrawDissolve(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.InvernoAbsoluteZeroDissolve, dissolveProgress,
                IceWhite, 0.06f);
        }

        /// <summary>
        /// Spawns musical VFX particles with ice crystal and blizzard themed accents.
        /// </summary>
        public static void SpawnMusicalAccents(NPC npc, int timer, int difficultyTier)
        {
            // Ice crystal convergence every 2 seconds
            if (timer % 120 == 0)
            {
                Phase10BossVFX.StaffLineConvergence(npc.Center, FrostBlue, 0.5f + difficultyTier * 0.2f);
            }

            // Rhythmic frost pulse on attack windows
            if (timer % 60 == 0 && difficultyTier >= 1)
            {
                Phase10BossVFX.MetronomeTickWarning(npc.Center, CrystalSilver, 3, 6);
            }

            // Winter frost burst accents during high aggression
            if (timer % 90 == 0 && difficultyTier >= 2)
            {
                BossSignatureVFX.WinterFrostBurst(npc.Center, 1.0f);
            }

            // Ambient ice crystal particles forming around the boss
            if (timer % 14 == 0)
            {
                // 6-fold symmetry crystal placement
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = 40f + Main.rand.NextFloat() * 50f;
                Vector2 offset = angle.ToRotationVector2() * dist;
                Color crystalColor = Color.Lerp(FrostBlue, IceWhite, Main.rand.NextFloat());
                CustomParticles.GenericFlare(npc.Center + offset, crystalColor, 0.18f, 28);
            }
        }
    }
}
