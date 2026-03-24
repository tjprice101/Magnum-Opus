using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Autumn.Bosses.Systems
{
    /// <summary>
    /// Autunno boss shader-driven rendering system — 4-phase visual progression.
    /// Phase 1 (Twilight Hunt):   Warm foliage aura, golden leaf trail, gentle bloom
    /// Phase 2 (Harvest Reaping): Darkening decay aura, scythe-arc trails, ground fog glow
    /// Phase 3 (Death of Year):   Desaturated wither glow, ashen trail, skeletal telegraph glows
    /// Phase 4 (Funeral Pyre):    Ember silhouette aura, dying flame wreath, boss is the only light
    /// </summary>
    public static class AutunnoBossShaderSystem
    {
        // Theme colors — full autumn arc from warm to cold to ember
        private static readonly Color TwilightAmber = new Color(200, 120, 40);
        private static readonly Color HarvestGold = new Color(218, 165, 32);
        private static readonly Color DecayBrown = new Color(100, 60, 30);
        private static readonly Color WitheredRed = new Color(150, 50, 30);
        private static readonly Color AshenGray = new Color(120, 110, 105);
        private static readonly Color TwilightPurple = new Color(100, 60, 120);
        private static readonly Color EmberOrange = new Color(255, 100, 30);
        private static readonly Color AutumnWhite = new Color(255, 240, 220);

        /// <summary>
        /// Master draw call — selects phase-appropriate rendering layers.
        /// Called from the boss's PreDraw/PostDraw.
        /// </summary>
        public static void DrawAllLayers(SpriteBatch sb, NPC npc, Vector2 screenPos,
            int phase, float aggressionLevel, int difficultyTier, bool isEnraged)
        {
            if (phase <= 1)
            {
                DrawTwilightAura(sb, npc, screenPos, aggressionLevel, difficultyTier);
                DrawFoliageTrail(sb, npc, screenPos, false);
            }
            else if (phase == 2)
            {
                DrawDecayAura(sb, npc, screenPos, aggressionLevel, difficultyTier);
                DrawFoliageTrail(sb, npc, screenPos, false);
                DrawGroundDecayGlow(sb, npc, screenPos, aggressionLevel);
            }
            else if (phase == 3)
            {
                DrawWitherGlow(sb, npc, screenPos, aggressionLevel);
                DrawFoliageTrail(sb, npc, screenPos, true);
            }
            else if (isEnraged)
            {
                DrawSilhouetteAura(sb, npc, screenPos);
                DrawDyingFlameWreath(sb, npc, screenPos);
            }
        }

        #region Phase 1 — Twilight Hunt

        /// <summary>
        /// Warm amber glow with 3-layer bloom stacking — golden autumn presence.
        /// </summary>
        private static void DrawTwilightAura(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float aggressionLevel, int difficultyTier)
        {
            float baseRadius = 80f + difficultyTier * 15f;
            float intensity = 0.3f + aggressionLevel * 0.35f;

            BossRenderHelper.DrawShaderAura(sb, npc, screenPos,
                BossShaderManager.AutunnoDecayAura, TwilightAmber, HarvestGold,
                baseRadius, intensity, (float)Main.timeForVisualEffects * 0.015f);

            // 3-layer bloom: warm outer → gold mid → white core
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            Vector2 drawPos = npc.Center - screenPos;
            Vector2 origin = glow.Size() / 2f;
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + (float)Math.Sin(time * 0.035f) * 0.08f;

            Color outer = TwilightAmber * 0.15f * intensity;
            outer.A = 0;
            sb.Draw(glow, drawPos, null, outer, 0f, origin, 1.82f * pulse, SpriteEffects.None, 0f);

            Color mid = HarvestGold * 0.22f * intensity;
            mid.A = 0;
            float midPulse = 1f + (float)Math.Sin(time * 0.05f) * 0.1f;
            sb.Draw(glow, drawPos, null, mid, 0f, origin, 1.6f * midPulse, SpriteEffects.None, 0f);

            Color core = AutumnWhite * 0.3f * intensity;
            core.A = 0;
            sb.Draw(glow, drawPos, null, core, 0f, origin, 0.7f, SpriteEffects.None, 0f);
        }

        #endregion

        #region Phase 2 — Harvest Reaping

        /// <summary>
        /// Darkening decay aura — bruised amber with brown undertone.
        /// Intensity ramps up with aggression.
        /// </summary>
        private static void DrawDecayAura(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float aggressionLevel, int difficultyTier)
        {
            float baseRadius = 100f + difficultyTier * 20f;
            float intensity = 0.4f + aggressionLevel * 0.5f;

            BossRenderHelper.DrawShaderAura(sb, npc, screenPos,
                BossShaderManager.AutunnoDecayAura, DecayBrown, WitheredRed,
                baseRadius, intensity, (float)Main.timeForVisualEffects * 0.02f);

            // 3-layer bloom: brown outer → withered mid → amber core
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            Vector2 drawPos = npc.Center - screenPos;
            Vector2 origin = glow.Size() / 2f;
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + (float)Math.Sin(time * 0.04f) * 0.1f;

            Color outer = DecayBrown * 0.2f * intensity;
            outer.A = 0;
            sb.Draw(glow, drawPos, null, outer, 0f, origin, 2.28f * pulse, SpriteEffects.None, 0f);

            Color mid = Color.Lerp(WitheredRed, TwilightAmber, 0.3f) * 0.28f * intensity;
            mid.A = 0;
            sb.Draw(glow, drawPos, null, mid, 0f, origin, 1.3f, SpriteEffects.None, 0f);

            Color core = TwilightAmber * 0.35f * intensity;
            core.A = 0;
            sb.Draw(glow, drawPos, null, core, 0f, origin, 0.9f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Subtle ground-level decay glow beneath the boss — harvest fog on the ground.
        /// </summary>
        private static void DrawGroundDecayGlow(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float aggressionLevel)
        {
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            Vector2 groundPos = new Vector2(npc.Center.X, npc.Bottom.Y + 10f) - screenPos;
            Vector2 origin = glow.Size() / 2f;
            float time = (float)Main.timeForVisualEffects;

            float breathe = 1f + (float)Math.Sin(time * 0.02f) * 0.15f;
            Color fogColor = DecayBrown * 0.18f * (0.5f + aggressionLevel * 0.5f);
            fogColor.A = 0;
            sb.Draw(glow, groundPos, null, fogColor, 0f, origin,
                new Vector2(4f * breathe, 1.2f), SpriteEffects.None, 0f);
        }

        #endregion

        #region Phase 3 — Death of the Year

        /// <summary>
        /// Desaturated wither glow — the boss's only remaining color.
        /// Purple-gray aura with dim flicker, death made visible.
        /// </summary>
        private static void DrawWitherGlow(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float aggressionLevel)
        {
            float intensity = 0.3f + aggressionLevel * 0.4f;

            BossRenderHelper.DrawShaderAura(sb, npc, screenPos,
                BossShaderManager.AutunnoDecayAura, TwilightPurple, AshenGray,
                80f, intensity, (float)Main.timeForVisualEffects * 0.012f);

            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            Vector2 drawPos = npc.Center - screenPos;
            Vector2 origin = glow.Size() / 2f;
            float time = (float)Main.timeForVisualEffects;

            // Dim, flickering wither glow — the only warmth left
            float flicker = 0.7f + (float)Math.Sin(time * 0.08f) * 0.15f
                + (float)Math.Sin(time * 0.13f + 1.7f) * 0.1f;

            Color withered = TwilightPurple * 0.2f * intensity * flicker;
            withered.A = 0;
            sb.Draw(glow, drawPos, null, withered, 0f, origin, 2.5f, SpriteEffects.None, 0f);

            Color ashen = AshenGray * 0.15f * intensity;
            ashen.A = 0;
            sb.Draw(glow, drawPos, null, ashen, 0f, origin, 1.3f, SpriteEffects.None, 0f);
        }

        #endregion

        #region Phase 4 — Funeral Pyre (Enrage)

        /// <summary>
        /// Ember silhouette aura — the boss is a dark form wreathed in dying embers.
        /// Boss outline burns hot but the inside is void.
        /// </summary>
        private static void DrawSilhouetteAura(SpriteBatch sb, NPC npc, Vector2 screenPos)
        {
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            Vector2 drawPos = npc.Center - screenPos;
            Vector2 origin = glow.Size() / 2f;
            float time = (float)Main.timeForVisualEffects;

            // Ember edge glow — bright orange outline
            float edgePulse = 1f + (float)Math.Sin(time * 0.06f) * 0.12f;
            Color edgeColor = EmberOrange * 0.4f;
            edgeColor.A = 0;
            sb.Draw(glow, drawPos, null, edgeColor, 0f, origin, 2.0f * edgePulse, SpriteEffects.None, 0f);

            // Dim withered red inner glow
            Color innerColor = WitheredRed * 0.25f;
            innerColor.A = 0;
            sb.Draw(glow, drawPos, null, innerColor, 0f, origin, 1.2f, SpriteEffects.None, 0f);

            // Faint white-hot core that flickers
            float coreFlicker = 0.5f + (float)Math.Sin(time * 0.1f) * 0.3f;
            Color coreColor = AutumnWhite * 0.2f * coreFlicker;
            coreColor.A = 0;
            sb.Draw(glow, drawPos, null, coreColor, 0f, origin, 0.5f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Dying flame wreath — ember particles orbiting the boss in a thinning circle.
        /// </summary>
        private static void DrawDyingFlameWreath(SpriteBatch sb, NPC npc, Vector2 screenPos)
        {
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            Vector2 drawPos = npc.Center - screenPos;
            Vector2 origin = glow.Size() / 2f;
            float time = (float)Main.timeForVisualEffects;

            int embers = 10;
            for (int i = 0; i < embers; i++)
            {
                float angle = time * 0.012f + MathHelper.TwoPi * i / embers;
                float radius = 55f + (float)Math.Sin(time * 0.04f + i * 1.2f) * 12f;
                Vector2 pos = drawPos + angle.ToRotationVector2() * radius;

                float fade = (float)Math.Sin(time * 0.06f + i * 0.9f) * 0.3f + 0.7f;
                Color emberColor = Color.Lerp(EmberOrange, WitheredRed, (float)i / embers) * 0.3f * fade;
                emberColor.A = 0;
                sb.Draw(glow, pos, null, emberColor, 0f, origin, 0.5f + fade * 0.3f, SpriteEffects.None, 0f);
            }
        }

        #endregion

        #region Shared Effects

        /// <summary>
        /// Leaf trail drawn behind the boss during dashes and movement.
        /// Phase-aware: warm gold (P1-2) vs desaturated gray (P3).
        /// </summary>
        private static void DrawFoliageTrail(SpriteBatch sb, NPC npc, Vector2 screenPos, bool desaturated)
        {
            Color trailColor = desaturated ? AshenGray : HarvestGold;
            float width = desaturated ? 0.8f : 1.0f;

            BossRenderHelper.DrawShaderTrail(sb, npc, screenPos,
                null, Rectangle.Empty, Vector2.Zero,
                BossShaderManager.AutunnoLeafTrail, trailColor, width,
                (float)Main.timeForVisualEffects * 0.02f, 5f);
        }

        /// <summary>
        /// Phase transition shader effect — golden harvest glow expanding.
        /// </summary>
        public static void DrawPhaseTransition(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float transitionProgress, int toPhase)
        {
            Color from = toPhase switch
            {
                2 => TwilightAmber,
                3 => DecayBrown,
                _ => WitheredRed
            };
            Color to = toPhase switch
            {
                2 => HarvestGold,
                3 => TwilightPurple,
                _ => EmberOrange
            };
            float intensity = 0.8f + toPhase * 0.2f;

            BossRenderHelper.DrawPhaseTransition(sb, npc, screenPos,
                BossShaderManager.AutunnoHarvestMoon, transitionProgress,
                from, to, intensity);
        }

        /// <summary>
        /// Death dissolve with widening edge glow — final harvest.
        /// </summary>
        public static void DrawDeathDissolve(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, float dissolveProgress)
        {
            BossRenderHelper.DrawDissolve(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.AutunnoFinalHarvest, dissolveProgress,
                HarvestGold, 0.07f);

            // Widening ember edge glow
            if (dissolveProgress > 0.2f)
            {
                Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
                Vector2 drawPos = npc.Center - screenPos;
                Vector2 glowOrigin = glow.Size() / 2f;

                float edgeIntensity = (dissolveProgress - 0.2f) / 0.8f;
                Color edgeColor = Color.Lerp(EmberOrange, AutumnWhite, edgeIntensity) * edgeIntensity * 0.6f;
                edgeColor.A = 0;
                float edgeScale = 1.5f + edgeIntensity * 2f;

                sb.Draw(glow, drawPos, null, edgeColor, 0f, glowOrigin, edgeScale, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Spawns musical VFX particles — phase-aware ambient accents around the boss.
        /// </summary>
        public static void SpawnMusicalAccents(NPC npc, int timer, int phase, int difficultyTier)
        {
            float hpDrive = 1f - (float)npc.life / npc.lifeMax;

            // Phase-dependent accent behavior
            if (phase <= 1)
                SpawnTwilightAccents(npc, timer, difficultyTier, hpDrive);
            else if (phase == 2)
                SpawnHarvestAccents(npc, timer, difficultyTier, hpDrive);
            else if (phase == 3)
                SpawnWitheringAccents(npc, timer, difficultyTier, hpDrive);
            else
                SpawnFuneralAccents(npc, timer, hpDrive);
        }

        private static void SpawnTwilightAccents(NPC npc, int timer, int difficultyTier, float hpDrive)
        {
            // Gentle golden leaf convergence
            int leafInterval = Math.Max(1, (int)(100 - hpDrive * 40));
            if (timer % leafInterval == 0)
                Phase10BossVFX.StaffLineConvergence(npc.Center, HarvestGold, 0.4f + difficultyTier * 0.15f);

            // Ambient golden motes
            if (timer % 12 == 0)
            {
                Vector2 offset = Main.rand.NextVector2Circular(70f, 70f);
                CustomParticles.GenericFlare(npc.Center + offset, TwilightAmber, 0.18f, 25);
            }

            // Bloom orbit — golden bloom particles
            if (timer % 14 == 0)
            {
                float angle = timer * 0.06f;
                Vector2 orbitPos = npc.Center + angle.ToRotationVector2() * 55f;
                MagnumParticleHandler.SpawnParticle(new BloomParticle(orbitPos, Vector2.Zero,
                    HarvestGold, 0.2f, 10));
            }
        }

        private static void SpawnHarvestAccents(NPC npc, int timer, int difficultyTier, float hpDrive)
        {
            // Decay-tinged metronome warnings
            int windInterval = Math.Max(1, (int)(50 - hpDrive * 25));
            if (timer % windInterval == 0 && difficultyTier >= 1)
                Phase10BossVFX.MetronomeTickWarning(npc.Center, DecayBrown, 3, 5);

            // Withering leaf storm accents
            int stormInterval = Math.Max(1, (int)(80 - hpDrive * 35));
            if (timer % stormInterval == 0 && difficultyTier >= 2)
                BossSignatureVFX.AutumnLeafStorm(npc.Center, 0.8f);

            // Drifting motes with occasional music notes
            if (timer % 10 == 0)
            {
                Vector2 offset = Main.rand.NextVector2Circular(80f, 80f);
                Color moteColor = Color.Lerp(DecayBrown, TwilightAmber, Main.rand.NextFloat());
                CustomParticles.GenericFlare(npc.Center + offset, moteColor, 0.2f, 20);
            }

            if (timer % 30 == 0)
                CustomParticles.GenericMusicNotes(npc.Center, HarvestGold, 2, 40f);

            // Bloom orbit — harvest bloom
            if (timer % 10 == 0)
            {
                float angle = timer * 0.08f;
                Vector2 orbitPos = npc.Center + angle.ToRotationVector2() * (60f + hpDrive * 25f);
                Color orbitColor = Color.Lerp(HarvestGold, DecayBrown, (float)Math.Sin(timer * 0.05f) * 0.5f + 0.5f);
                MagnumParticleHandler.SpawnParticle(new BloomParticle(orbitPos, Vector2.Zero,
                    orbitColor, 0.22f + hpDrive * 0.1f, 12));
            }
        }

        private static void SpawnWitheringAccents(NPC npc, int timer, int difficultyTier, float hpDrive)
        {
            // Sparse, eerie glows — the dying season
            if (timer % 20 == 0)
            {
                Vector2 offset = Main.rand.NextVector2Circular(60f, 60f);
                Color glowColor = Color.Lerp(TwilightPurple, AshenGray, Main.rand.NextFloat());
                CustomParticles.GenericGlow(npc.Center + offset, glowColor, 0.15f, 18);
            }

            // Rare skeletal branch telegraph glints
            if (timer % 45 == 0 && difficultyTier >= 1)
                Phase10BossVFX.FermataHoldIndicator(npc.Center, TwilightPurple, 0.3f);

            // Ascending ashen sparkles
            if (timer % 15 == 0 && hpDrive > 0.5f)
            {
                Vector2 sparkPos = npc.Center + Main.rand.NextVector2Circular(45f, 45f);
                Vector2 sparkVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(1f, 2.5f));
                MagnumParticleHandler.SpawnParticle(new SparkleParticle(sparkPos, sparkVel, AshenGray, 0.2f, 22));
            }
        }

        private static void SpawnFuneralAccents(NPC npc, int timer, float hpDrive)
        {
            // Dying flame ember sparks — sparse, fading
            if (timer % 8 == 0)
            {
                Vector2 offset = Main.rand.NextVector2Circular(50f, 50f);
                Color emberColor = Color.Lerp(EmberOrange, WitheredRed, Main.rand.NextFloat());
                CustomParticles.GenericFlare(npc.Center + offset, emberColor, 0.15f, 12);
            }

            // Ember bloom orbit — tighter, fading circle
            if (timer % 6 == 0)
            {
                float angle = timer * 0.1f;
                Vector2 orbitPos = npc.Center + angle.ToRotationVector2() * 40f;
                Color orbitColor = Color.Lerp(EmberOrange, new Color(80, 30, 10), Main.rand.NextFloat());
                MagnumParticleHandler.SpawnParticle(new BloomParticle(orbitPos, Vector2.Zero,
                    orbitColor, 0.18f, 8));
            }

            // Ascending dying sparks
            if (timer % 12 == 0)
            {
                Vector2 sparkPos = npc.Center + Main.rand.NextVector2Circular(35f, 35f);
                Vector2 sparkVel = new Vector2(Main.rand.NextFloat(-0.6f, 0.6f), -Main.rand.NextFloat(1.5f, 3.5f));
                MagnumParticleHandler.SpawnParticle(new SparkleParticle(sparkPos, sparkVel,
                    EmberOrange, 0.25f, 18));
            }
        }

        #endregion
    }
}