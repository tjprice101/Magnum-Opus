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
    /// Enigma Variations boss shader-driven rendering system.
    /// Manages all 5 boss shaders: EnigmaVoidAura, EnigmaShadowTrail,
    /// EnigmaParadoxRift, EnigmaTeleportWarp, EnigmaUnveilingDissolve.
    ///
    /// Called from EnigmaTheHollowMystery.PreDraw/PostDraw to layer shader
    /// effects on top of the standard sprite drawing.
    /// </summary>
    public static class EnigmaBossShaderSystem
    {
        // Theme colors
        private static readonly Color VoidBlack = new Color(15, 5, 25);
        private static readonly Color DeepPurple = new Color(100, 30, 150);
        private static readonly Color EerieGreen = new Color(80, 200, 100);
        private static readonly Color MysteryWhite = new Color(200, 180, 220);

        /// <summary>
        /// Draws the swirling void aura with 3-layer bloom stacking.
        /// Wide smoky void outer, purple mid glow, green-white core when enraged.
        /// </summary>
        public static void DrawVoidAura(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float aggressionLevel, int difficultyTier, bool isEnraged)
        {
            float baseRadius = 85f + difficultyTier * 22f;
            float intensity = 0.3f + aggressionLevel * 0.5f;
            if (isEnraged) intensity *= 1.4f;

            Color primary = isEnraged ? EerieGreen : DeepPurple;
            Color secondary = isEnraged ? MysteryWhite : VoidBlack;

            BossRenderHelper.DrawShaderAura(sb, npc, screenPos,
                BossShaderManager.EnigmaVoidAura, primary, secondary,
                baseRadius, intensity, (float)Main.timeForVisualEffects * 0.012f);
            
            // 3-layer bloom stacking around the aura
            var bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (bloomTex != null)
            {
                Vector2 drawPos = npc.Center - screenPos;
                Vector2 bloomOrigin = new Vector2(bloomTex.Width, bloomTex.Height) * 0.5f;
                float pulse = 0.9f + 0.1f * (float)Math.Sin(Main.timeForVisualEffects * 0.03f);
                
                // Layer 1: Wide smoky void ambient
                Color voidOuter = VoidBlack;
                voidOuter.A = 0;
                float outerScale = baseRadius * 2.5f / bloomTex.Width * pulse;
                sb.Draw(bloomTex, drawPos, null, voidOuter * intensity * 0.25f, 0f, bloomOrigin, outerScale, SpriteEffects.None, 0f);
                
                // Layer 2: Purple mid glow
                Color midGlow = DeepPurple;
                midGlow.A = 0;
                float midScale = baseRadius * 1.6f / bloomTex.Width * pulse;
                sb.Draw(bloomTex, drawPos, null, midGlow * intensity * 0.35f, 0f, bloomOrigin, midScale, SpriteEffects.None, 0f);
                
                // Layer 3: Bright core — green when enraged
                Color coreColor = isEnraged ? EerieGreen : MysteryWhite;
                coreColor.A = 0;
                float coreScale = baseRadius * 0.9f / bloomTex.Width * pulse;
                sb.Draw(bloomTex, drawPos, null, coreColor * intensity * 0.4f, 0f, bloomOrigin, coreScale, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draws the dark teleport shadow trail with glow orbs at afterimage positions.
        /// </summary>
        public static void DrawShadowTrail(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, bool isEnraged)
        {
            Color trailColor = isEnraged ? EerieGreen : DeepPurple;
            float width = isEnraged ? 1.3f : 0.9f;

            BossRenderHelper.DrawShaderTrail(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.EnigmaShadowTrail, trailColor, width,
                (float)Main.timeForVisualEffects * 0.018f, 3f);
            
            // Glow orbs at afterimage positions
            var bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (bloomTex != null)
            {
                Vector2 bloomOrigin = new Vector2(bloomTex.Width, bloomTex.Height) * 0.5f;
                int orbCount = Math.Min(6, npc.oldPos.Length);
                for (int i = 0; i < orbCount; i++)
                {
                    if (npc.oldPos[i] == Vector2.Zero) continue;
                    Vector2 orbPos = npc.oldPos[i] + npc.Size * 0.5f - screenPos;
                    float fade = 1f - (float)i / orbCount;
                    Color orbColor = trailColor;
                    orbColor.A = 0;
                    sb.Draw(bloomTex, orbPos, null, orbColor * fade * 0.3f, 0f, bloomOrigin, 0.5f * fade, SpriteEffects.None, 0f);
                }
            }
        }

        /// <summary>
        /// Draws the paradox rift visual during reality-tearing attacks.
        /// </summary>
        public static void DrawParadoxRift(SpriteBatch sb, Vector2 position, Vector2 screenPos,
            float intensity)
        {
            BossRenderHelper.DrawAttackFlash(sb, position, screenPos,
                BossShaderManager.EnigmaParadoxRift, EerieGreen, intensity,
                (float)Main.timeForVisualEffects * 0.025f);
        }

        /// <summary>
        /// Draws the teleport warp-in/warp-out animation effect.
        /// </summary>
        public static void DrawTeleportWarp(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float warpProgress, bool isWarpingIn)
        {
            Color from = isWarpingIn ? VoidBlack : DeepPurple;
            Color to = isWarpingIn ? DeepPurple : VoidBlack;
            float intensity = isWarpingIn ? 0.8f : 1.1f;

            BossRenderHelper.DrawPhaseTransition(sb, npc, screenPos,
                BossShaderManager.EnigmaTeleportWarp, warpProgress,
                from, to, intensity);
        }

        /// <summary>
        /// Draws the unveiling dissolve death animation with widening edge glow.
        /// Mystery revealed: dissolves from void into eerie green revelation light.
        /// </summary>
        public static void DrawUnveilingDissolve(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, float dissolveProgress)
        {
            // Edge transitions from purple to green as the mystery unravels
            Color edgeColor = Color.Lerp(DeepPurple, EerieGreen, dissolveProgress);

            BossRenderHelper.DrawDissolve(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.EnigmaUnveilingDissolve, dissolveProgress,
                edgeColor, 0.06f);
            
            // Widening edge glow + ambient bloom intensification
            var bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (bloomTex != null)
            {
                Vector2 drawPos = npc.Center - screenPos;
                Vector2 bloomOrigin = new Vector2(bloomTex.Width, bloomTex.Height) * 0.5f;
                
                // Edge glow widens as dissolve progresses
                Color glowColor = edgeColor;
                glowColor.A = 0;
                float glowScale = MathHelper.Lerp(0.6f, 2.5f, dissolveProgress);
                float glowOpacity = MathHelper.Lerp(0.2f, 0.7f, dissolveProgress);
                sb.Draw(bloomTex, drawPos, null, glowColor * glowOpacity, 0f, bloomOrigin, glowScale, SpriteEffects.None, 0f);
                
                // Inner revelation core — white-green, appears later
                if (dissolveProgress > 0.4f)
                {
                    float coreProgress = (dissolveProgress - 0.4f) / 0.6f;
                    Color coreGlow = MysteryWhite;
                    coreGlow.A = 0;
                    sb.Draw(bloomTex, drawPos, null, coreGlow * coreProgress * 0.5f, 0f, bloomOrigin, glowScale * 0.5f, SpriteEffects.None, 0f);
                }
            }
        }
        
        /// <summary>
        /// Draws 3-layer bloom glow directly on the boss NPC.
        /// Wide void outer, purple mid, green-white inner.
        /// Called from PreDraw Layer 0.
        /// </summary>
        public static void DrawBossGlow(SpriteBatch sb, NPC npc, Vector2 screenPos, bool isEnraged)
        {
            var bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (bloomTex == null) return;
            
            Vector2 drawPos = npc.Center - screenPos;
            Vector2 bloomOrigin = new Vector2(bloomTex.Width, bloomTex.Height) * 0.5f;
            float pulse = 0.85f + 0.15f * (float)Math.Sin(Main.timeForVisualEffects * 0.04f);
            
            // Layer 1: Wide smoky void outer
            Color outerColor = VoidBlack;
            outerColor.A = 0;
            sb.Draw(bloomTex, drawPos, null, outerColor * 0.25f * pulse, 0f, bloomOrigin, 1.8f, SpriteEffects.None, 0f);
            
            // Layer 2: Purple mid glow
            Color midColor = DeepPurple;
            midColor.A = 0;
            sb.Draw(bloomTex, drawPos, null, midColor * 0.35f * pulse, 0f, bloomOrigin, 1.1f, SpriteEffects.None, 0f);
            
            // Layer 3: Inner bright — green when enraged
            Color innerColor = isEnraged ? EerieGreen : MysteryWhite;
            innerColor.A = 0;
            sb.Draw(bloomTex, drawPos, null, innerColor * 0.3f * pulse, 0f, bloomOrigin, 0.6f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Spawns musical VFX particles during various boss states.
        /// HP-driven: more eyes, glyphs, and bloom particles at low HP.
        /// </summary>
        public static void SpawnMusicalAccents(NPC npc, int timer, int difficultyTier)
        {
            float hpDrive = 1f - (float)npc.life / npc.lifeMax;
            
            // Mysterious trill — more frequent at low HP
            int trillInterval = (int)MathHelper.Lerp(120, 60, hpDrive);
            if (timer % Math.Max(1, trillInterval) == 0)
            {
                Phase10Integration.Enigma.MysteriousTrill(npc.Center, timer);
            }

            // Paradox syncopation rhythmic accents
            int syncopInterval = (int)MathHelper.Lerp(60, 30, hpDrive);
            if (timer % Math.Max(1, syncopInterval) == 0 && difficultyTier >= 1)
            {
                Phase10Integration.Enigma.ParadoxSyncopation(npc.Center, 120f);
            }

            // Watching eyes spawn near the boss at higher tiers
            int eyeInterval = (int)MathHelper.Lerp(90, 40, hpDrive);
            if (timer % Math.Max(1, eyeInterval) == 0 && difficultyTier >= 2)
            {
                CustomParticles.EnigmaEyeGaze(
                    npc.Center + Main.rand.NextVector2Circular(80f, 80f),
                    EerieGreen, 0.4f);
            }

            // Orbiting glyphs
            int glyphInterval = (int)MathHelper.Lerp(150, 70, hpDrive);
            if (timer % Math.Max(1, glyphInterval) == 0 && difficultyTier >= 1)
            {
                CustomParticles.GlyphCircle(npc.Center, DeepPurple, 4, 60f, 0.02f);
            }
            
            // Bloom particles orbiting the boss
            if (timer % 8 == 0)
            {
                float angle = timer * 0.05f;
                float orbitRadius = MathHelper.Lerp(60f, 90f, hpDrive);
                Vector2 orbitPos = npc.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * orbitRadius;
                
                Color bloomColor = Color.Lerp(DeepPurple, EerieGreen, hpDrive);
                MagnumParticleHandler.SpawnParticle(new BloomParticle(
                    orbitPos, Main.rand.NextVector2Circular(0.5f, 0.5f),
                    bloomColor, MathHelper.Lerp(0.3f, 0.6f, hpDrive), 30));
            }
            
            // Low-HP danger: ascending void sparks
            if (hpDrive > 0.5f && timer % 12 == 0)
            {
                Vector2 sparkPos = npc.Center + Main.rand.NextVector2Circular(100f, 100f);
                Color sparkColor = Color.Lerp(DeepPurple, EerieGreen, Main.rand.NextFloat());
                MagnumParticleHandler.SpawnParticle(new SparkleParticle(
                    sparkPos, new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-2f, -1f)),
                    sparkColor, MathHelper.Lerp(0.3f, 0.5f, hpDrive), 40));
            }
        }
    }
}
