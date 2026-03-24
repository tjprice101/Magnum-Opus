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
    /// Phase-aware L'Inverno boss shader-driven rendering system.
    /// 4-phase glow configs, phase-specific trails, storm obscuration,
    /// and HP-driven musical accents that evolve with the fight.
    /// </summary>
    public static class LInvernoBossShaderSystem
    {
        // Updated palette — colder, more distinct per-phase
        private static readonly Color IceBlue = new Color(168, 216, 234);
        private static readonly Color FrostWhite = new Color(232, 244, 248);
        private static readonly Color DeepGlacialBlue = new Color(27, 79, 114);
        private static readonly Color CrystalCyan = new Color(0, 229, 255);
        private static readonly Color BlizzardWhite = new Color(240, 248, 255);
        private static readonly Color GlacialPurple = new Color(123, 104, 174);
        private static readonly Color PaleSilverBlue = new Color(190, 210, 230);
        private static readonly Color AbsoluteZeroBlue = new Color(200, 230, 255);

        /// <summary>
        /// SHADER LAYER 0 — Phase-aware 3-layer boss glow drawn behind everything.
        /// Phase 1: Delicate crystalline shimmer. Phase 2: Stronger cold aura.
        /// Phase 3: Storm-wreathed intensity. Phase 4: Stark absolute glow.
        /// </summary>
        public static void DrawBossGlow(SpriteBatch sb, NPC npc, Vector2 screenPos, bool isEnraged)
        {
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            Vector2 drawPos = npc.Center - screenPos;
            Vector2 glowOrigin = new Vector2(glow.Width / 2f, glow.Height / 2f);
            float pulse = (float)Math.Sin((float)Main.timeForVisualEffects * 0.05f) * 0.08f + 1f;
            int phase = LInvernoSky.GetVFXPhase();

            Color outerC, midC, coreC;
            float outerS, midS, coreS;

            switch (phase)
            {
                case 1: // First Frost — subtle, crystalline
                    outerC = PaleSilverBlue * 0.10f;
                    midC = IceBlue * 0.15f;
                    coreC = FrostWhite * 0.22f;
                    outerS = 0.975f; midS = 0.585f; coreS = 0.293f;
                    break;
                case 2: // Frozen Expanse — stronger cold presence
                    outerC = IceBlue * 0.15f;
                    midC = CrystalCyan * 0.20f;
                    coreC = FrostWhite * 0.28f;
                    outerS = 1.17f; midS = 0.715f; coreS = 0.358f;
                    break;
                case 3: // Blizzard — storm-wreathed
                    outerC = DeepGlacialBlue * 0.20f;
                    midC = IceBlue * 0.25f;
                    coreC = BlizzardWhite * 0.33f;
                    outerS = 1.43f; midS = 0.91f; coreS = 0.423f;
                    break;
                default: // Absolute Zero — stark, blinding core
                    outerC = new Color(15, 25, 50) * 0.25f;
                    midC = AbsoluteZeroBlue * 0.30f;
                    coreC = Color.White * 0.40f;
                    outerS = 1.625f; midS = 1.04f; coreS = 0.488f;
                    break;
            }

            outerC.A = 0; midC.A = 0; coreC.A = 0;

            sb.Draw(glow, drawPos, null, outerC, 0f, glowOrigin, outerS * pulse, SpriteEffects.None, 0f);
            sb.Draw(glow, drawPos, null, midC, 0f, glowOrigin, midS * pulse, SpriteEffects.None, 0f);
            sb.Draw(glow, drawPos, null, coreC, 0f, glowOrigin, coreS * pulse, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Phase-aware frost aura with 3-layer bloom.
        /// Phase 1: Crystalline shimmer ring. Phase 2: Thicker permafrost aura.
        /// Phase 3: Wild storm aura. Phase 4: Frozen stillness aura.
        /// </summary>
        public static void DrawFrostAura(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float aggressionLevel, int difficultyTier, bool isEnraged)
        {
            int phase = LInvernoSky.GetVFXPhase();
            float baseRadius = 85f + difficultyTier * 20f;
            float intensity = 0.3f + aggressionLevel * 0.5f;

            Color primary, secondary;
            switch (phase)
            {
                case 1:
                    primary = PaleSilverBlue; secondary = IceBlue;
                    intensity *= 0.7f;
                    break;
                case 2:
                    primary = IceBlue; secondary = DeepGlacialBlue;
                    break;
                case 3:
                    primary = CrystalCyan; secondary = DeepGlacialBlue;
                    intensity *= 1.3f;
                    baseRadius *= 1.2f;
                    break;
                default:
                    primary = AbsoluteZeroBlue; secondary = new Color(15, 25, 50);
                    intensity *= 1.5f;
                    baseRadius *= 1.4f;
                    break;
            }

            BossRenderHelper.DrawShaderAura(sb, npc, screenPos,
                BossShaderManager.InvernoFrostAura, primary, secondary,
                baseRadius, intensity, (float)Main.timeForVisualEffects * 0.015f);

            // 3-layer additive bloom on aura
            Texture2D bloom = MagnumTextureRegistry.GetBloom();
            Vector2 drawPos = npc.Center - screenPos;
            Vector2 bOrigin = new Vector2(bloom.Width / 2f, bloom.Height / 2f);
            float pulse = (float)Math.Sin((float)Main.timeForVisualEffects * 0.04f) * 0.06f + 1f;

            Color outerBloom = (phase >= 3 ? DeepGlacialBlue : IceBlue) * (0.10f * intensity);
            Color midBloom = Color.Lerp(PaleSilverBlue, FrostWhite, 0.3f) * (0.16f * intensity);
            Color coreBloom = BlizzardWhite * (0.22f * intensity);
            outerBloom.A = 0; midBloom.A = 0; coreBloom.A = 0;

            sb.Draw(bloom, drawPos, null, outerBloom, 0f, bOrigin, 3.2f * pulse, SpriteEffects.None, 0f);
            sb.Draw(bloom, drawPos, null, midBloom, 0f, bOrigin, 2.0f * pulse, SpriteEffects.None, 0f);
            sb.Draw(bloom, drawPos, null, coreBloom, 0f, bOrigin, 0.9f * pulse, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Phase-aware ice trail during movement.
        /// Phase 1: Suspended crystals. Phase 2: Permafrost wake.
        /// Phase 3: Wind-torn shards. Phase 4: Frozen wake.
        /// </summary>
        public static void DrawIceTrail(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, bool isEnraged)
        {
            int phase = LInvernoSky.GetVFXPhase();
            Color trailColor;
            float width;

            switch (phase)
            {
                case 1:
                    trailColor = PaleSilverBlue;
                    width = 0.7f;
                    break;
                case 2:
                    trailColor = IceBlue;
                    width = 1.0f;
                    break;
                case 3:
                    trailColor = CrystalCyan;
                    width = 1.4f;
                    break;
                default:
                    trailColor = AbsoluteZeroBlue;
                    width = 1.8f;
                    break;
            }

            BossRenderHelper.DrawShaderTrail(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.InvernoIceTrail, trailColor, width,
                (float)Main.timeForVisualEffects * 0.018f, 5f);
        }

        /// <summary>
        /// Storm obscuration layer — Phase 3 only.
        /// Draws frosty noise between player and boss for partial visual occlusion.
        /// </summary>
        public static void DrawStormObscuration(SpriteBatch sb, NPC npc, Vector2 screenPos)
        {
            int phase = LInvernoSky.GetVFXPhase();
            if (phase < 3) return;

            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            Vector2 drawPos = npc.Center - screenPos;
            Vector2 glowOrigin = new Vector2(glow.Width / 2f, glow.Height / 2f);
            float time = (float)Main.timeForVisualEffects;

            // Swirling storm particles around the boss
            int particleCount = phase == 3 ? 8 : 5;
            float baseRadius = phase == 3 ? 120f : 80f;
            float alphaBase = phase == 3 ? 0.12f : 0.08f;

            for (int i = 0; i < particleCount; i++)
            {
                float angle = time * 0.02f + MathHelper.TwoPi * i / particleCount;
                float radiusVariation = (float)Math.Sin(time * 0.008f + i * 1.3f) * 30f;
                float radius = baseRadius + radiusVariation;
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;

                float alpha = alphaBase * (0.6f + (float)Math.Sin(time * 0.015f + i) * 0.4f);
                Color stormColor = (phase == 3 ? BlizzardWhite : AbsoluteZeroBlue) * alpha;
                stormColor.A = 0;

                float scale = 0.4f + (float)Math.Sin(time * 0.01f + i * 0.7f) * 0.15f;
                sb.Draw(glow, drawPos + offset, null, stormColor, angle, glowOrigin, scale, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draws the blizzard vortex attack visual.
        /// </summary>
        public static void DrawBlizzardVortex(SpriteBatch sb, Vector2 position, Vector2 screenPos,
            float intensity)
        {
            BossRenderHelper.DrawAttackFlash(sb, position, screenPos,
                BossShaderManager.InvernoBlizzardVortex, IceBlue, intensity,
                (float)Main.timeForVisualEffects * 0.02f);
        }

        /// <summary>
        /// Draws the concentrated freeze ray beam effect.
        /// </summary>
        public static void DrawFreezeRay(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float transitionProgress, bool isPhase2Transition)
        {
            Color from = isPhase2Transition ? IceBlue : PaleSilverBlue;
            Color to = isPhase2Transition ? FrostWhite : AbsoluteZeroBlue;
            float intensity = isPhase2Transition ? 1.2f : 0.8f;

            BossRenderHelper.DrawPhaseTransition(sb, npc, screenPos,
                BossShaderManager.InvernoFreezeRay, transitionProgress,
                from, to, intensity);
        }

        /// <summary>
        /// Draws the absolute zero frozen shatter death dissolve with phase-aware edge glow.
        /// </summary>
        public static void DrawAbsoluteZeroDissolve(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, float dissolveProgress)
        {
            BossRenderHelper.DrawDissolve(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.InvernoAbsoluteZeroDissolve, dissolveProgress,
                FrostWhite, 0.06f);

            // Edge glow intensifies after 20%
            if (dissolveProgress > 0.2f)
            {
                Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
                Vector2 drawPos = npc.Center - screenPos;
                Vector2 glowOrigin = new Vector2(glow.Width / 2f, glow.Height / 2f);
                float edgeAlpha = (dissolveProgress - 0.2f) / 0.8f;
                Color edgeColor = Color.Lerp(IceBlue, BlizzardWhite, edgeAlpha) * (edgeAlpha * 0.4f);
                edgeColor.A = 0;
                sb.Draw(glow, drawPos, null, edgeColor, 0f, glowOrigin, 1.4f + edgeAlpha * 0.8f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Phase-aware HP-driven musical accents with bloom orbit and ascending sparkles.
        /// Phase 1: Gentle crystal convergence. Phase 2: Rhythmic frost pulses.
        /// Phase 3: Storm-driven bursts. Phase 4: Frozen silence accents.
        /// </summary>
        public static void SpawnMusicalAccents(NPC npc, int timer, int difficultyTier)
        {
            float hpDrive = 1f - (npc.life / (float)npc.lifeMax);
            int phase = LInvernoSky.GetVFXPhase();

            // Ice crystal convergence — frequency scales with phase
            int crystalInterval = phase switch
            {
                1 => Math.Max(1, (int)MathHelper.Lerp(140, 80, hpDrive)),
                2 => Math.Max(1, (int)MathHelper.Lerp(100, 50, hpDrive)),
                3 => Math.Max(1, (int)MathHelper.Lerp(70, 35, hpDrive)),
                _ => Math.Max(1, (int)MathHelper.Lerp(200, 120, hpDrive)) // Sparse in Absolute Zero
            };

            if (timer % crystalInterval == 0)
            {
                Color convergenceColor = phase <= 2 ? IceBlue : CrystalCyan;
                Phase10BossVFX.StaffLineConvergence(npc.Center, convergenceColor, 0.5f + difficultyTier * 0.2f);
            }

            // Rhythmic frost pulse — Phase 2+
            if (phase >= 2)
            {
                int frostInterval = Math.Max(1, (int)MathHelper.Lerp(60, 30, hpDrive));
                if (timer % frostInterval == 0)
                {
                    Color pulseColor = phase == 3 ? BlizzardWhite : PaleSilverBlue;
                    Phase10BossVFX.MetronomeTickWarning(npc.Center, pulseColor, 3, 6);
                }
            }

            // Winter frost burst accents — Phase 3+
            if (phase >= 3 && difficultyTier >= 1)
            {
                int burstInterval = Math.Max(1, (int)MathHelper.Lerp(90, 45, hpDrive));
                if (timer % burstInterval == 0)
                {
                    BossSignatureVFX.WinterFrostBurst(npc.Center, 1.0f);
                    CustomParticles.GenericMusicNotes(npc.Center, FrostWhite, 3, 45f);
                }
            }

            // Bloom orbit — phase-colored, every 12 frames
            if (timer % 12 == 0)
            {
                float angle = (float)Main.timeForVisualEffects * 0.035f;
                float orbitRadius = 50f + hpDrive * 20f;
                Vector2 orbitPos = npc.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * orbitRadius;

                Color orbitColor;
                switch (phase)
                {
                    case 1: orbitColor = PaleSilverBlue * 0.25f; break;
                    case 2: orbitColor = IceBlue * 0.30f; break;
                    case 3: orbitColor = CrystalCyan * 0.35f; break;
                    default: orbitColor = AbsoluteZeroBlue * 0.20f; break;
                }
                orbitColor.A = 0;
                MagnumParticleHandler.SpawnParticle(new BloomParticle(orbitPos, Vector2.Zero, orbitColor, 0.18f + hpDrive * 0.12f, 8));
            }

            // Ascending sparkles at high damage (Phase 2+)
            if (hpDrive > 0.5f && phase >= 2)
            {
                int sparkInterval = Math.Max(1, (int)MathHelper.Lerp(15, 8, hpDrive));
                if (timer % sparkInterval == 0)
                {
                    Vector2 sparkPos = npc.Center + Main.rand.NextVector2Circular(30f, 30f);
                    Vector2 sparkVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(1.5f, 3f));
                    Color sparkColor = phase == 4 ? AbsoluteZeroBlue : FrostWhite;
                    MagnumParticleHandler.SpawnParticle(new SparkleParticle(sparkPos, sparkVel, sparkColor, 0.22f, 20));
                }
            }
        }
    }
}