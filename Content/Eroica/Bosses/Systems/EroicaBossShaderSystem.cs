using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Shaders;

namespace MagnumOpus.Content.Eroica.Bosses.Systems
{
    /// <summary>
    /// Eroica boss rendering system — Beethoven's Third Symphony in visual form.
    /// Every rendering method is movement-aware: M1 (Call to Arms), M2 (Funeral March),
    /// M3 (Scherzo), M4 (Apotheosis/Enrage). Delegates shader work to BossRenderHelper
    /// with pre-applied uMovement/uHeroIntensity uniforms, then layers additive bloom
    /// overlays for visual depth.
    /// </summary>
    public static class EroicaBossShaderSystem
    {
        // Core palette
        private static readonly Color ValorGold = new Color(255, 200, 80);
        private static readonly Color ValorScarlet = new Color(200, 50, 50);
        private static readonly Color FuneralCrimson = new Color(180, 30, 60);
        private static readonly Color SakuraPink = new Color(255, 150, 180);
        private static readonly Color PhoenixWhite = new Color(255, 240, 220);
        private static readonly Color EmberOrange = new Color(255, 140, 30);
        private static readonly Color FuneralAsh = new Color(80, 60, 50);

        // Movement mapping: difficultyTier 0 → M1, 1 → M2, 2 → M3, enraged → M4
        public static float GetMovement(int difficultyTier, bool isEnraged)
        {
            if (isEnraged) return 4f;
            return difficultyTier switch
            {
                0 => 1f,
                1 => 2f,
                _ => 3f
            };
        }

        public static float GetHeroIntensity(float aggressionLevel, int difficultyTier, bool isEnraged)
        {
            if (isEnraged) return 1f;
            return MathHelper.Clamp(aggressionLevel * 0.6f + difficultyTier * 0.2f, 0f, 1f);
        }

        /// <summary>
        /// Pre-applies movement phase uniforms to a shader before BossRenderHelper call.
        /// </summary>
        private static void SetMovementUniforms(string shaderKey, float movement, float heroIntensity)
        {
            var shader = BossShaderManager.GetShader(shaderKey);
            BossShaderManager.ApplyMovementParams(shader, movement, heroIntensity);
        }

        /// <summary>
        /// Draws the heroic valor aura. Each movement has distinct character:
        /// M1: Bold golden radiance, heroic rings. M2: Contracted, smoldering crimson.
        /// M3: Flickering staccato flashes. M4: White-hot with cracked armor.
        /// </summary>
        public static void DrawValorAura(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float aggressionLevel, int difficultyTier, bool isEnraged)
        {
            float movement = GetMovement(difficultyTier, isEnraged);
            float heroIntensity = GetHeroIntensity(aggressionLevel, difficultyTier, isEnraged);
            float lifeRatio = (float)npc.life / npc.lifeMax;
            float time = (float)Main.GlobalTimeWrappedHourly;

            // Movement-driven aura parameters
            float baseRadius, intensity;
            Color primary, secondary;
            float pulseSpeed;

            if (movement < 1.5f)
            {
                // M1: Call to Arms — Bold and radiant
                baseRadius = 90f;
                intensity = 0.4f + aggressionLevel * 0.4f;
                primary = ValorGold;
                secondary = ValorScarlet;
                pulseSpeed = 1.5f;
            }
            else if (movement < 2.5f)
            {
                // M2: Funeral March — Contracted and somber
                baseRadius = 60f;
                intensity = 0.25f + aggressionLevel * 0.2f;
                primary = FuneralCrimson;
                secondary = FuneralAsh;
                pulseSpeed = 0.6f; // Slow, heavy breathing
            }
            else if (movement < 3.5f)
            {
                // M3: Scherzo — Electric and flickering
                baseRadius = 80f + (float)Math.Sin(time * 8f) * 15f; // Erratic size
                intensity = 0.5f + aggressionLevel * 0.5f;
                primary = ValorScarlet;
                secondary = ValorGold;
                pulseSpeed = 4f; // Rapid staccato
            }
            else
            {
                // M4: Apotheosis — Burning white-hot
                baseRadius = 120f + heroIntensity * 40f;
                intensity = 0.8f + heroIntensity * 0.7f;
                primary = PhoenixWhite;
                secondary = new Color(255, 100, 50);
                pulseSpeed = 3f;
            }

            float pulse = 1f + (float)Math.Sin(time * pulseSpeed) *
                MathHelper.Lerp(0.05f, 0.18f, 1f - lifeRatio);

            SetMovementUniforms(BossShaderManager.EroicaValorAura, movement, heroIntensity);

            BossRenderHelper.DrawShaderAura(sb, npc, screenPos,
                BossShaderManager.EroicaValorAura, primary, secondary,
                baseRadius * pulse, intensity, (float)Main.timeForVisualEffects * 0.02f);

            // Bloom overlay layers (movement-specific)
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow == null) return;

            Vector2 drawPos = npc.Center - screenPos;
            Vector2 glowOrigin = new Vector2(glow.Width * 0.5f, glow.Height * 0.5f);

            if (movement < 1.5f)
            {
                // M1: Bright gold halo + scarlet ambient
                Color mid = ValorGold * (0.14f * intensity * pulse);
                mid.A = 0;
                sb.Draw(glow, drawPos, null, mid, 0f, glowOrigin, baseRadius * 1.82f * pulse / glow.Width, SpriteEffects.None, 0f);

                Color outer = ValorScarlet * (0.06f * intensity);
                outer.A = 0;
                sb.Draw(glow, drawPos, null, outer, 0f, glowOrigin, baseRadius * 2.93f * pulse / glow.Width, SpriteEffects.None, 0f);
            }
            else if (movement < 2.5f)
            {
                // M2: Dim crimson core + ashen outer haze
                Color mid = FuneralCrimson * (0.08f * intensity * pulse);
                mid.A = 0;
                sb.Draw(glow, drawPos, null, mid, 0f, glowOrigin, baseRadius * 2f * pulse / glow.Width, SpriteEffects.None, 0f);

                // Occasional golden breakthrough (sunlight through clouds)
                float breakthrough = (float)Math.Sin(time * 0.7f + 2.3f);
                if (breakthrough > 0.7f)
                {
                    float btIntensity = (breakthrough - 0.7f) / 0.3f;
                    Color btColor = ValorGold * (0.1f * btIntensity);
                    btColor.A = 0;
                    sb.Draw(glow, drawPos, null, btColor, 0f, glowOrigin, baseRadius * 1.95f / glow.Width, SpriteEffects.None, 0f);
                }
            }
            else if (movement < 3.5f)
            {
                // M3: Alternating flicker between gold and scarlet
                float flicker = (float)Math.Sin(time * 12f);
                Color flickerColor = flicker > 0 ? ValorGold : ValorScarlet;
                Color mid = flickerColor * (0.12f * intensity * Math.Abs(flicker));
                mid.A = 0;
                sb.Draw(glow, drawPos, null, mid, 0f, glowOrigin, baseRadius * 1.63f / glow.Width, SpriteEffects.None, 0f);
            }
            else
            {
                // M4: White-hot core + scarlet corona + crimson outer ring
                Color core = PhoenixWhite * (0.25f * intensity * pulse);
                core.A = 0;
                sb.Draw(glow, drawPos, null, core, 0f, glowOrigin, baseRadius * 0.98f * pulse / glow.Width, SpriteEffects.None, 0f);

                Color corona = new Color(255, 100, 50) * (0.15f * intensity);
                corona.A = 0;
                sb.Draw(glow, drawPos, null, corona, 0f, glowOrigin, baseRadius * 2.28f * pulse / glow.Width, SpriteEffects.None, 0f);

                float enragePulse = 1f + (float)Math.Sin(time * 5f) * 0.15f;
                Color outerRing = FuneralCrimson * (0.1f * enragePulse);
                outerRing.A = 0;
                sb.Draw(glow, drawPos, null, outerRing, 0f, glowOrigin, baseRadius * 3.58f * enragePulse / glow.Width, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draws the movement trail during dashes and charges.
        /// M1: War banner streams. M2: Heavy funeral smoke. M3: Staccato afterimages. M4: Supernova trail.
        /// </summary>
        public static void DrawHeroicTrail(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, bool isEnraged,
            int difficultyTier, float aggressionLevel)
        {
            float movement = GetMovement(difficultyTier, isEnraged);
            float heroIntensity = GetHeroIntensity(aggressionLevel, difficultyTier, isEnraged);

            // Movement-driven trail parameters
            Color trailColor;
            float width;
            float velocityThreshold;

            if (movement < 1.5f)
            {
                trailColor = ValorGold;
                width = 1.0f;
                velocityThreshold = 6f;
            }
            else if (movement < 2.5f)
            {
                trailColor = FuneralCrimson;
                width = 1.3f; // Wider for heavy smoke
                velocityThreshold = 3f; // Shows even at slow movement
            }
            else if (movement < 3.5f)
            {
                trailColor = Color.Lerp(ValorScarlet, ValorGold, (float)Math.Sin(Main.GlobalTimeWrappedHourly * 10f) * 0.5f + 0.5f);
                width = 0.8f; // Tight, staccato
                velocityThreshold = 4f;
            }
            else
            {
                trailColor = PhoenixWhite;
                width = 2.0f; // Maximum width — supernova
                velocityThreshold = 2f;
            }

            SetMovementUniforms(BossShaderManager.EroicaHeroicTrail, movement, heroIntensity);

            BossRenderHelper.DrawShaderTrail(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.EroicaHeroicTrail, trailColor, width,
                (float)Main.timeForVisualEffects * 0.02f, velocityThreshold);

            // Movement-specific bloom overlays on afterimage positions
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow == null || npc.velocity.Length() < velocityThreshold) return;

            Vector2 glowOrigin = new Vector2(glow.Width * 0.5f, glow.Height * 0.5f);
            int trailLen = Math.Min(npc.oldPos.Length, 10);

            for (int i = 0; i < trailLen; i++)
            {
                if (npc.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / trailLen;
                Vector2 pos = npc.oldPos[i] + npc.Size / 2f - screenPos;

                if (movement < 1.5f)
                {
                    // M1: Gold-sakura shimmer
                    float alpha = (1f - progress) * 0.15f;
                    Color shimmer = i % 2 == 0 ? ValorGold : Color.Lerp(ValorGold, SakuraPink, 0.3f);
                    Color c = shimmer * alpha;
                    c.A = 0;
                    sb.Draw(glow, pos, null, c, 0f, glowOrigin, (1f - progress * 0.3f) * 0.15f, SpriteEffects.None, 0f);
                }
                else if (movement < 2.5f)
                {
                    // M2: Lingering ashen smoke with rare golden sparks
                    float alpha = (1f - progress * 0.5f) * 0.1f; // Lingers longer
                    Color c = FuneralAsh * alpha;
                    c.A = 0;
                    sb.Draw(glow, pos, null, c, 0f, glowOrigin, (1f - progress * 0.15f) * 0.2f, SpriteEffects.None, 0f);

                    if (i == 2 || i == 5) // Rare golden memory sparks
                    {
                        Color spark = ValorGold * (0.08f * (1f - progress));
                        spark.A = 0;
                        sb.Draw(glow, pos, null, spark, 0f, glowOrigin, 0.05f, SpriteEffects.None, 0f);
                    }
                }
                else if (movement < 3.5f)
                {
                    // M3: Sharp discrete segments — only draw every other position
                    if (i % 2 == 0)
                    {
                        float alpha = (1f - progress) * 0.2f;
                        Color c = (i % 4 == 0 ? ValorGold : ValorScarlet) * alpha;
                        c.A = 0;
                        sb.Draw(glow, pos, null, c, 0f, glowOrigin, 0.1f, SpriteEffects.None, 0f);
                    }
                }
                else
                {
                    // M4: Supernova — every position bleeds bright light
                    float alpha = (1f - progress) * 0.25f;
                    Color c = Color.Lerp(PhoenixWhite, ValorGold, progress) * alpha;
                    c.A = 0;
                    float scale = (1f - progress * 0.2f) * 0.25f;
                    sb.Draw(glow, pos, null, c, 0f, glowOrigin, scale, SpriteEffects.None, 0f);
                }
            }
        }

        /// <summary>
        /// Draws attack flash. M1: Rising phoenix. M2: Funeral pyre. M3: Electric burst. M4: Supernova.
        /// </summary>
        public static void DrawPhoenixFlame(SpriteBatch sb, Vector2 position, Vector2 screenPos,
            float intensity, int difficultyTier, bool isEnraged, float aggressionLevel)
        {
            float movement = GetMovement(difficultyTier, isEnraged);
            float heroIntensity = GetHeroIntensity(aggressionLevel, difficultyTier, isEnraged);

            SetMovementUniforms(BossShaderManager.EroicaPhoenixFlame, movement, heroIntensity);

            BossRenderHelper.DrawAttackFlash(sb, position, screenPos,
                BossShaderManager.EroicaPhoenixFlame, ValorGold, intensity,
                (float)Main.timeForVisualEffects * 0.03f);

            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow == null || intensity <= 0.1f) return;

            Vector2 drawPos = position - screenPos;
            Vector2 glowOrigin = new Vector2(glow.Width * 0.5f, glow.Height * 0.5f);

            if (movement < 1.5f)
            {
                // M1: Classical phoenix wings — gold flare + white core + wing spreads
                Color goldFlare = ValorGold * (0.3f * intensity);
                goldFlare.A = 0;
                sb.Draw(glow, drawPos, null, goldFlare, 0f, glowOrigin, intensity * 0.6f, SpriteEffects.None, 0f);

                Color whiteCore = PhoenixWhite * (0.5f * intensity);
                whiteCore.A = 0;
                sb.Draw(glow, drawPos, null, whiteCore, 0f, glowOrigin, intensity * 0.2f, SpriteEffects.None, 0f);

                for (int side = -1; side <= 1; side += 2)
                {
                    Vector2 wingOffset = new Vector2(side * 40f * intensity, -15f * intensity);
                    Color wingColor = EmberOrange * (0.15f * intensity);
                    wingColor.A = 0;
                    sb.Draw(glow, drawPos + wingOffset, null, wingColor, 0f, glowOrigin, new Vector2(intensity * 0.5f, intensity * 0.15f), SpriteEffects.None, 0f);
                }
            }
            else if (movement < 2.5f)
            {
                // M2: Funeral pyre — compressed, dark. Heavy crimson with down-pointing glow
                Color crimsonGlow = FuneralCrimson * (0.25f * intensity);
                crimsonGlow.A = 0;
                sb.Draw(glow, drawPos, null, crimsonGlow, 0f, glowOrigin, intensity * 0.35f, SpriteEffects.None, 0f);

                // Downward-stretched ember glow (fire falls in mourning)
                Color ashGlow = FuneralAsh * (0.15f * intensity);
                ashGlow.A = 0;
                sb.Draw(glow, drawPos + new Vector2(0, 20f * intensity), null, ashGlow, 0f, glowOrigin,
                    new Vector2(intensity * 0.3f, intensity * 0.5f), SpriteEffects.None, 0f);
            }
            else if (movement < 3.5f)
            {
                // M3: Electric starburst — radial spikes
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f + Main.GlobalTimeWrappedHourly * 3f;
                    Vector2 spikeOffset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 30f * intensity;
                    Color spikeColor = (i % 2 == 0 ? ValorGold : ValorScarlet) * (0.2f * intensity);
                    spikeColor.A = 0;
                    sb.Draw(glow, drawPos + spikeOffset, null, spikeColor, angle, glowOrigin,
                        new Vector2(intensity * 0.3f, intensity * 0.06f), SpriteEffects.None, 0f);
                }
            }
            else
            {
                // M4: Supernova — concentric rings: white core → gold → scarlet → crimson
                Color whiteCore = PhoenixWhite * (0.6f * intensity);
                whiteCore.A = 0;
                sb.Draw(glow, drawPos, null, whiteCore, 0f, glowOrigin, intensity * 0.25f, SpriteEffects.None, 0f);

                Color goldRing = ValorGold * (0.35f * intensity);
                goldRing.A = 0;
                sb.Draw(glow, drawPos, null, goldRing, 0f, glowOrigin, intensity * 0.55f, SpriteEffects.None, 0f);

                Color scarletRing = ValorScarlet * (0.2f * intensity);
                scarletRing.A = 0;
                sb.Draw(glow, drawPos, null, scarletRing, 0f, glowOrigin, intensity * 0.85f, SpriteEffects.None, 0f);

                Color crimsonRing = FuneralCrimson * (0.1f * intensity);
                crimsonRing.A = 0;
                sb.Draw(glow, drawPos, null, crimsonRing, 0f, glowOrigin, intensity * 1.2f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draws phase transition effect between movements.
        /// To M2: Sakura petals fall like tears. To M3: Petals ignite. To M4: Apotheosis ring.
        /// </summary>
        public static void DrawPhaseTransition(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float transitionProgress, int targetMovement, float aggressionLevel)
        {
            float heroIntensity = MathHelper.Clamp(aggressionLevel * 0.6f + (targetMovement - 1) * 0.25f, 0f, 1f);

            // Colors shift based on which movement we're transitioning TO
            Color from, to;
            float intensity;

            if (targetMovement <= 2)
            {
                from = ValorGold;
                to = FuneralCrimson;
                intensity = 0.9f;
            }
            else if (targetMovement <= 3)
            {
                from = FuneralCrimson;
                to = ValorScarlet;
                intensity = 1.2f;
            }
            else
            {
                from = ValorScarlet;
                to = PhoenixWhite;
                intensity = 1.8f;
            }

            SetMovementUniforms(BossShaderManager.EroicaSakuraTransition, (float)targetMovement, heroIntensity);

            BossRenderHelper.DrawPhaseTransition(sb, npc, screenPos,
                BossShaderManager.EroicaSakuraTransition, transitionProgress,
                from, to, intensity);

            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow == null || transitionProgress <= 0f || transitionProgress >= 1f) return;

            Vector2 drawPos = npc.Center - screenPos;
            Vector2 glowOrigin = new Vector2(glow.Width * 0.5f, glow.Height * 0.5f);
            float ringProgress = (float)Math.Sin(transitionProgress * MathHelper.Pi);

            Color ringColor = Color.Lerp(from, to, transitionProgress) * (ringProgress * 0.25f * intensity);
            ringColor.A = 0;
            float ringScale = (0.2f + transitionProgress * 0.8f) * intensity;
            sb.Draw(glow, drawPos, null, ringColor, 0f, glowOrigin, ringScale, SpriteEffects.None, 0f);

            // Sky flash tied to the transition moment
            if (transitionProgress > 0.4f && transitionProgress < 0.6f)
            {
                if (targetMovement <= 2)
                    EroicaSkySystem.TriggerScarletFlash(0.5f);
                else if (targetMovement <= 3)
                    EroicaSkySystem.TriggerAttackFlash(0.6f, ValorGold);
                else
                    EroicaSkySystem.TriggerGoldenFlash(0.9f);
            }
        }

        /// <summary>
        /// Draws the heroic death dissolve — the hero's final moment of transcendence.
        /// </summary>
        public static void DrawDeathDissolve(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, float dissolveProgress)
        {
            float edgeWidth = MathHelper.Lerp(0.04f, 0.14f, dissolveProgress);
            Color edgeColor = dissolveProgress < 0.35f
                ? Color.Lerp(ValorGold, PhoenixWhite, dissolveProgress / 0.35f)
                : Color.Lerp(PhoenixWhite, new Color(255, 255, 240), (dissolveProgress - 0.35f) / 0.65f);

            SetMovementUniforms(BossShaderManager.EroicaDeathDissolveFx, 4f, 1f);

            BossRenderHelper.DrawDissolve(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.EroicaDeathDissolveFx, dissolveProgress,
                edgeColor, edgeWidth);

            // Ambient bloom intensifies as hero dissolves into golden light
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow != null && dissolveProgress > 0.15f)
            {
                Vector2 drawPos = npc.Center - screenPos;
                Vector2 glowOrigin = new Vector2(glow.Width * 0.5f, glow.Height * 0.5f);
                float bloomStrength = (dissolveProgress - 0.15f) / 0.85f;

                // Golden radiance
                Color goldBloom = ValorGold * (0.15f * bloomStrength);
                goldBloom.A = 0;
                sb.Draw(glow, drawPos, null, goldBloom, 0f, glowOrigin, 0.25f + bloomStrength * 0.4f, SpriteEffects.None, 0f);

                // Phoenix white core (appears past 50% dissolve)
                if (dissolveProgress > 0.5f)
                {
                    float whiteStrength = (dissolveProgress - 0.5f) / 0.5f;
                    Color whiteBloom = PhoenixWhite * (0.25f * whiteStrength);
                    whiteBloom.A = 0;
                    sb.Draw(glow, drawPos, null, whiteBloom, 0f, glowOrigin, 0.15f + whiteStrength * 0.35f, SpriteEffects.None, 0f);
                }
            }
        }

        /// <summary>
        /// Draws the boss glow behind the sprite. Movement-aware: brightens and shifts palette.
        /// </summary>
        public static void DrawBossGlow(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float lifeRatio, bool isEnraged, int difficultyTier)
        {
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow == null) return;

            float movement = GetMovement(difficultyTier, isEnraged);
            Vector2 drawPos = npc.Center - screenPos;
            Vector2 glowOrigin = new Vector2(glow.Width * 0.5f, glow.Height * 0.5f);
            float pulse = 1f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 1.8f) * 0.06f;
            float hpIntensity = MathHelper.Lerp(0.6f, 1.3f, 1f - lifeRatio);

            Color coreColor, midColor, ambientColor;

            if (movement < 1.5f)
            {
                coreColor = PhoenixWhite;
                midColor = ValorGold;
                ambientColor = ValorScarlet;
            }
            else if (movement < 2.5f)
            {
                coreColor = Color.Lerp(PhoenixWhite, FuneralCrimson, 0.3f);
                midColor = FuneralCrimson;
                ambientColor = FuneralAsh;
            }
            else if (movement < 3.5f)
            {
                float flicker = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 10f);
                coreColor = flicker > 0 ? PhoenixWhite : ValorGold;
                midColor = ValorScarlet;
                ambientColor = ValorGold;
            }
            else
            {
                coreColor = new Color(255, 255, 240); // Near-white hot
                midColor = PhoenixWhite;
                ambientColor = new Color(255, 100, 50);
            }

            Color core = coreColor * (0.18f * hpIntensity * pulse);
            core.A = 0;
            sb.Draw(glow, drawPos, null, core, 0f, glowOrigin, 0.06f * pulse, SpriteEffects.None, 0f);

            Color mid = midColor * (0.1f * hpIntensity);
            mid.A = 0;
            sb.Draw(glow, drawPos, null, mid, 0f, glowOrigin, 0.12f * pulse, SpriteEffects.None, 0f);

            Color ambient = ambientColor * (0.05f * hpIntensity);
            ambient.A = 0;
            sb.Draw(glow, drawPos, null, ambient, 0f, glowOrigin, 0.22f * pulse, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Draws heroic phoenix wings. Visible during dives and when enraged.
        /// Wings change character per movement: elegant M1, smoldering M2, frantic M3, blazing M4.
        /// </summary>
        public static void DrawPhoenixWings(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float intensity, int difficultyTier, bool isEnraged)
        {
            if (intensity <= 0.05f) return;

            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow == null) return;

            float movement = GetMovement(difficultyTier, isEnraged);
            Vector2 drawPos = npc.Center - screenPos;
            Vector2 glowOrigin = new Vector2(glow.Width * 0.5f, glow.Height * 0.5f);
            float time = (float)Main.GlobalTimeWrappedHourly;

            float flapSpeed = movement < 2.5f ? 3f : (movement < 3.5f ? 8f : 5f);
            float wingFlap = (float)Math.Sin(time * flapSpeed) * 0.15f;

            Color outerColor, innerColor;
            if (movement < 2.5f)
            {
                outerColor = EmberOrange;
                innerColor = ValorGold;
            }
            else if (movement < 3.5f)
            {
                outerColor = ValorScarlet;
                innerColor = ValorGold;
            }
            else
            {
                outerColor = PhoenixWhite;
                innerColor = new Color(255, 255, 240);
            }

            for (int side = -1; side <= 1; side += 2)
            {
                float baseAngle = side * (0.8f + wingFlap);
                Vector2 wingOffset = new Vector2(side * 35f * intensity, -20f * intensity);

                Color outer = outerColor * (0.12f * intensity);
                outer.A = 0;
                sb.Draw(glow, drawPos + wingOffset, null, outer, baseAngle, glowOrigin,
                    new Vector2(intensity * 0.4f, intensity * 0.1f), SpriteEffects.None, 0f);

                Color inner = innerColor * (0.18f * intensity);
                inner.A = 0;
                sb.Draw(glow, drawPos + wingOffset * 0.5f, null, inner, baseAngle * 0.7f, glowOrigin,
                    new Vector2(intensity * 0.25f, intensity * 0.07f), SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Spawns movement-aware musical accent particles.
        /// M1: Staff lines + golden blooms. M2: Slow dirges + ashen particles.
        /// M3: Rapid metronome + staccato sparks. M4: Full ensemble + ember fountains.
        /// </summary>
        public static void SpawnMusicalAccents(NPC npc, int timer, int difficultyTier,
            float aggressionLevel, bool isEnraged)
        {
            float movement = GetMovement(difficultyTier, isEnraged);
            float lifeRatio = (float)npc.life / npc.lifeMax;

            if (movement < 1.5f)
            {
                // M1: Triumphant golden bloom particles + staff lines
                if (timer % 120 == 0)
                    Phase10BossVFX.StaffLineConvergence(npc.Center, ValorGold, 0.5f);

                if (timer % 30 == 0)
                {
                    Vector2 offset = Main.rand.NextVector2Circular(80f, 80f);
                    Vector2 vel = -Vector2.UnitY * Main.rand.NextFloat(0.3f, 0.8f);
                    var bloom = new BloomParticle(npc.Center + offset, vel, ValorGold, Main.rand.NextFloat(0.2f, 0.5f), Main.rand.Next(40, 80));
                    MagnumParticleHandler.SpawnParticle(bloom);
                }
            }
            else if (movement < 2.5f)
            {
                // M2: Slow dirge — falling sakura + ashen smoke + rare golden break
                if (timer % 90 == 0)
                    Phase10BossVFX.StaffLineConvergence(npc.Center, FuneralCrimson, 0.3f);

                if (timer % 20 == 0)
                {
                    // Sakura falling like tears
                    Vector2 offset = Main.rand.NextVector2Circular(100f, 40f);
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(0.5f, 1.5f)); // Downward
                    Color col = Color.Lerp(SakuraPink, FuneralCrimson, Main.rand.NextFloat(0.5f));
                    var bloom = new BloomParticle(npc.Center + offset + new Vector2(0, -50f), vel, col, Main.rand.NextFloat(0.15f, 0.35f), Main.rand.Next(60, 120));
                    MagnumParticleHandler.SpawnParticle(bloom);
                }

                if (timer % 150 == 0) // Rare golden memory
                {
                    var goldBloom = new BloomParticle(npc.Center, -Vector2.UnitY * 0.5f, ValorGold, 0.4f, 60);
                    MagnumParticleHandler.SpawnParticle(goldBloom);
                }
            }
            else if (movement < 3.5f)
            {
                // M3: Rapid staccato — fast metronome + electric sparks
                if (timer % 40 == 0)
                    Phase10BossVFX.MetronomeTickWarning(npc.Center, ValorScarlet, 3, 6);

                if (timer % 15 == 0)
                {
                    Vector2 offset = Main.rand.NextVector2Circular(60f, 60f);
                    Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                    Color col = Main.rand.NextBool() ? ValorGold : ValorScarlet;
                    var spark = new GlowSparkParticle(npc.Center + offset, vel, col, Main.rand.NextFloat(0.1f, 0.25f), Main.rand.Next(8, 18));
                    MagnumParticleHandler.SpawnParticle(spark);
                }
            }
            else
            {
                // M4: Full ensemble — everything at maximum
                if (timer % 60 == 0)
                {
                    Phase10BossVFX.StaffLineConvergence(npc.Center, PhoenixWhite, 1.0f);
                    Phase10Integration.Eroica.HeroicImpact(npc.Center, 1.0f);
                }

                if (timer % 30 == 0)
                    Phase10BossVFX.CrescendoDangerRings(npc.Center, ValorScarlet, 0.8f);

                // Ember fountains ascending
                if (timer % 10 == 0)
                {
                    Vector2 offset = Main.rand.NextVector2Circular(50f, 30f);
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-4f, -1.5f));
                    Color col = Color.Lerp(EmberOrange, ValorGold, Main.rand.NextFloat());
                    var spark = new GlowSparkParticle(npc.Center + offset, vel, col, Main.rand.NextFloat(0.2f, 0.5f), Main.rand.Next(25, 50));
                    MagnumParticleHandler.SpawnParticle(spark);
                }

                // Golden bloom particles
                if (timer % 20 == 0)
                {
                    Vector2 offset = Main.rand.NextVector2Circular(80f, 80f);
                    Vector2 vel = -Vector2.UnitY * Main.rand.NextFloat(0.5f, 1.2f);
                    var bloom = new BloomParticle(npc.Center + offset, vel, PhoenixWhite, Main.rand.NextFloat(0.3f, 0.6f), Main.rand.Next(30, 60));
                    MagnumParticleHandler.SpawnParticle(bloom);
                }
            }
        }
    }
}
