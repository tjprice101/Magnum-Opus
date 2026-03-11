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
    /// La Campanella boss shader-driven rendering system.
    /// Multi-layered: bell aura (3-layer bloom), infernal trail with glow overlays,
    /// resonance wave rings, firewall transitions, chime dissolve with ember edge.
    /// Includes DrawBossGlow and HP-driven SpawnMusicalAccents.
    /// </summary>
    public static class LaCampanellaBossShaderSystem
    {
        // Theme colors
        private static readonly Color InfernalOrange = new Color(255, 140, 40);
        private static readonly Color SmokeBlack = new Color(30, 20, 15);
        private static readonly Color BellGold = new Color(220, 180, 80);
        private static readonly Color FlameWhite = new Color(255, 230, 200);
        private static readonly Color EmberCrimson = new Color(200, 50, 30);

        /// <summary>
        /// Draws multi-layered bell resonance aura behind the boss.
        /// 3 bloom layers: inner white-hot core, orange mid, wide smoky ambient.
        /// Intensity scales with phase, HP, and enrage.
        /// </summary>
        public static void DrawBellAura(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float aggressionLevel, int difficultyTier, bool isEnraged)
        {
            float baseRadius = 90f + difficultyTier * 25f;
            float intensity = 0.35f + aggressionLevel * 0.45f;
            if (isEnraged) intensity *= 1.6f;

            Color primary = isEnraged ? FlameWhite : InfernalOrange;
            Color secondary = isEnraged ? EmberCrimson : BellGold;

            BossRenderHelper.DrawShaderAura(sb, npc, screenPos,
                BossShaderManager.CampanellaBellAura, primary, secondary,
                baseRadius, intensity, (float)Main.timeForVisualEffects * 0.015f);
            
            // Multi-layer bloom stacking on top of shader aura
            Texture2D bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (bloomTex != null)
            {
                Vector2 drawCenter = npc.Center - screenPos;
                Vector2 bloomOrigin = new Vector2(bloomTex.Width, bloomTex.Height) * 0.5f;
                float breathe = (float)Math.Sin(Main.timeForVisualEffects * 0.012f) * 0.5f + 0.5f;
                
                // Wide smoky ambient
                Color outerBloom = SmokeBlack * (0.06f + aggressionLevel * 0.04f);
                outerBloom.A = 0;
                sb.Draw(bloomTex, drawCenter, null, outerBloom, 0f, bloomOrigin, 0.5f + breathe * 0.05f, SpriteEffects.None, 0f);
                
                // Orange mid glow
                Color midBloom = InfernalOrange * (0.1f + aggressionLevel * 0.06f);
                midBloom.A = 0;
                sb.Draw(bloomTex, drawCenter, null, midBloom, 0f, bloomOrigin, 0.3f + breathe * 0.03f, SpriteEffects.None, 0f);
                
                // White-hot core when enraged
                if (isEnraged)
                {
                    Color coreBloom = FlameWhite * 0.15f;
                    coreBloom.A = 0;
                    sb.Draw(bloomTex, drawCenter, null, coreBloom, 0f, bloomOrigin, 0.15f, SpriteEffects.None, 0f);
                }
            }
        }
        
        /// <summary>
        /// Draws 3-layer NPC bloom glow. Called before main sprite in PreDraw.
        /// </summary>
        public static void DrawBossGlow(SpriteBatch sb, NPC npc, Vector2 screenPos, float lifeRatio, bool isEnraged)
        {
            Texture2D bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (bloomTex == null) return;
            
            Vector2 drawCenter = npc.Center - screenPos;
            Vector2 bloomOrigin = new Vector2(bloomTex.Width, bloomTex.Height) * 0.5f;
            float hpDrive = 1f - lifeRatio;
            float breathe = (float)Math.Sin(Main.timeForVisualEffects * 0.01f) * 0.5f + 0.5f;
            
            // Layer 1: Wide smoky outer
            float outerScale = MathHelper.Lerp(0.35f, 0.55f, hpDrive) + breathe * 0.03f;
            Color outerColor = Color.Lerp(SmokeBlack, InfernalOrange, hpDrive * 0.3f) * MathHelper.Lerp(0.06f, 0.12f, hpDrive);
            outerColor.A = 0;
            sb.Draw(bloomTex, drawCenter, null, outerColor, 0f, bloomOrigin, outerScale, SpriteEffects.None, 0f);
            
            // Layer 2: Orange mid
            float midScale = MathHelper.Lerp(0.2f, 0.35f, hpDrive) + breathe * 0.02f;
            Color midColor = InfernalOrange * MathHelper.Lerp(0.08f, 0.16f, hpDrive);
            midColor.A = 0;
            sb.Draw(bloomTex, drawCenter, null, midColor, 0f, bloomOrigin, midScale, SpriteEffects.None, 0f);
            
            // Layer 3: White-hot core
            float coreScale = MathHelper.Lerp(0.08f, 0.18f, hpDrive);
            Color coreColor = Color.Lerp(BellGold, FlameWhite, hpDrive) * MathHelper.Lerp(0.1f, 0.2f, hpDrive);
            if (isEnraged) coreColor = FlameWhite * 0.25f;
            coreColor.A = 0;
            sb.Draw(bloomTex, drawCenter, null, coreColor, 0f, bloomOrigin, coreScale, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Draws infernal smoke trail with glow overlay at each afterimage position.
        /// </summary>
        public static void DrawInfernalTrail(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, bool isEnraged)
        {
            Color trailColor = isEnraged ? InfernalOrange : SmokeBlack;
            float width = isEnraged ? 1.6f : 1.1f;

            BossRenderHelper.DrawShaderTrail(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.CampanellaInfernalTrail, trailColor, width,
                (float)Main.timeForVisualEffects * 0.025f, 5f);
            
            // Soft glow orb at each afterimage for additive shimmer
            Texture2D bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (bloomTex != null)
            {
                Vector2 bloomOrigin = new Vector2(bloomTex.Width, bloomTex.Height) * 0.5f;
                for (int i = 0; i < npc.oldPos.Length; i += 2)
                {
                    float progress = (float)i / npc.oldPos.Length;
                    Vector2 pos = npc.oldPos[i] + npc.Size / 2f - screenPos;
                    Color glowCol = Color.Lerp(InfernalOrange, BellGold, progress) * (1f - progress) * 0.12f;
                    glowCol.A = 0;
                    sb.Draw(bloomTex, pos, null, glowCol, 0f, bloomOrigin, 0.08f * (1f - progress), SpriteEffects.None, 0f);
                }
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
        /// Draws the infernal firewall during phase transitions. Triggers sky flash.
        /// </summary>
        public static void DrawFirewall(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float transitionProgress, bool isPhase2Transition)
        {
            Color from = isPhase2Transition ? InfernalOrange : BellGold;
            Color to = isPhase2Transition ? FlameWhite : InfernalOrange;
            float intensity = isPhase2Transition ? 1.4f : 0.9f;

            BossRenderHelper.DrawPhaseTransition(sb, npc, screenPos,
                BossShaderManager.CampanellaFirewall, transitionProgress,
                from, to, intensity);
            
            // Trigger sky flash at transition midpoint
            if (transitionProgress > 0.4f && transitionProgress < 0.5f)
            {
                LaCampanellaSkySystem.TriggerWhiteFlash(isPhase2Transition ? 0.7f : 0.4f);
            }
        }

        /// <summary>
        /// Draws the chime dissolve death animation  Ebell shattering into embers.
        /// </summary>
        public static void DrawChimeDissolve(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, float dissolveProgress)
        {
            // Edge glow widens as dissolution progresses
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
                Color ambientBloom = InfernalOrange * (dissolveProgress * 0.18f);
                ambientBloom.A = 0;
                sb.Draw(bloomTex, drawCenter, null, ambientBloom, 0f, bloomOrigin, 0.3f + dissolveProgress * 0.2f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Spawns musical VFX particles — HP-driven frequency, bloom accents, danger rings.
        /// </summary>
        public static void SpawnMusicalAccents(NPC npc, int timer, int difficultyTier)
        {
            float hpRatio = npc.life / (float)npc.lifeMax;
            int staffInterval = (int)MathHelper.Lerp(80, 160, hpRatio);
            
            // Bell resonance convergence — more frequent at low HP
            if (timer % staffInterval == 0)
            {
                Phase10BossVFX.StaffLineConvergence(npc.Center, InfernalOrange, 0.5f + difficultyTier * 0.2f);
            }

            // Low-HP crescendo danger rings
            if (hpRatio < 0.35f && timer % 90 == 0)
            {
                Phase10BossVFX.CrescendoDangerRings(npc.Center, EmberCrimson, 0.8f);
            }
            
            // Ambient golden bloom particles
            if (timer % 25 == 0)
            {
                Vector2 bloomPos = npc.Center + Main.rand.NextVector2Circular(50f, 50f);
                Vector2 bloomVel = new Vector2(0, -1f) + Main.rand.NextVector2Circular(0.5f, 0.3f);
                Color bloomColor = Color.Lerp(BellGold, InfernalOrange, Main.rand.NextFloat());
                MagnumParticleHandler.SpawnParticle(new BloomParticle(bloomPos, bloomVel, bloomColor, 0.2f + difficultyTier * 0.05f, 30));
            }

            // Rhythmic bell tick on attack windows
            if (timer % 35 == 0)
            {
                Phase10BossVFX.MetronomeTickWarning(npc.Center, BellGold, 1, 4);
            }

            // Infernal flame accents during high aggression
            if (timer % 90 == 0 && difficultyTier >= 2)
            {
                Phase10Integration.LaCampanella.InfernalTremolo(npc.Center);
            }
        }
    }
}
