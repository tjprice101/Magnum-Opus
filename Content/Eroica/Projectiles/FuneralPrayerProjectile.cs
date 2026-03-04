using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Content.Eroica;
using MagnumOpus.Content.FoundationWeapons.RibbonFoundation;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// Funeral Prayer projectile 窶・large flaming bolt with red/gold flames using 6ﾃ・ sprite sheet.
    /// Self-contained VFX: GPU trail, spritesheet rendering, afterimages, flame particles.
    /// </summary>
    public class FuneralPrayerProjectile : ModProjectile
    {
        private const int FrameCount = 36;
        private const int FramesPerRow = 6;
        private const int FrameRows = 6;
        private const int AnimationSpeed = 2;

        private int frameCounter = 0;
        private int currentFrame = 0;

        private ref float AgeTimer => ref Projectile.ai[1];

        // 笏笏 Trail tracking 笏笏
        private const int TrailLength = 20;
        private Vector2[] trailPositions = new Vector2[TrailLength];
        private float[] trailRotations = new float[TrailLength];
        private bool trailInitialized = false;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 48;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 180;
            Projectile.alpha = 0;
            Projectile.light = 0.6f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            AgeTimer++;

            // Pulsating scale 窶・the funeral flame breathes
            float scalePulse = (float)Math.Sin(AgeTimer * 0.07f) * 0.05f;
            Projectile.scale = 1.0f + scalePulse;

            // Animate through 6ﾃ・ sprite sheet
            frameCounter++;
            if (frameCounter >= AnimationSpeed)
            {
                frameCounter = 0;
                currentFrame = (currentFrame + 1) % FrameCount;
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            // 笏笏 Trail position tracking 笏笏
            if (!trailInitialized)
            {
                for (int i = 0; i < TrailLength; i++)
                {
                    trailPositions[i] = Projectile.Center;
                    trailRotations[i] = Projectile.rotation;
                }
                trailInitialized = true;
            }
            else
            {
                for (int i = TrailLength - 1; i > 0; i--)
                {
                    trailPositions[i] = trailPositions[i - 1];
                    trailRotations[i] = trailRotations[i - 1];
                }
                trailPositions[0] = Projectile.Center;
                trailRotations[0] = Projectile.rotation;
            }

            // 笏笏 Particle Spawning 笏笏
            SpawnFlightParticles();
        }

        #region Particle Spawning

        private void SpawnFlightParticles()
        {
            // Funeral flames trailing behind
            if (Main.rand.NextBool(2))
            {
                Vector2 perpendicular = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2);
                Vector2 flameVel = -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.8f, 2.5f)
                    + perpendicular * Main.rand.NextFloatDirection() * 1.8f
                    + new Vector2(0, -Main.rand.NextFloat(0.4f, 1f));

                FuneralParticleHandler.SpawnParticle(new FuneralFlameParticle(
                    Projectile.Center + perpendicular * Main.rand.NextFloatDirection() * 8f,
                    flameVel,
                    Color.Lerp(FuneralUtils.DeepCrimson, FuneralUtils.PrayerFlame, Main.rand.NextFloat()),
                    Main.rand.NextFloat(0.35f, 0.7f),
                    Main.rand.Next(14, 24)
                ));
            }

            // Prayer ash
            if (Main.rand.NextBool(5))
            {
                FuneralParticleHandler.SpawnParticle(new PrayerAshParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    new Vector2(Main.rand.NextFloatDirection() * 0.3f, -Main.rand.NextFloat(0.2f, 0.5f)),
                    FuneralUtils.AshGray * 0.4f,
                    Main.rand.NextFloat(0.3f, 0.55f),
                    Main.rand.Next(18, 32)
                ));
            }

            // Dust
            if (Main.rand.NextBool(4))
            {
                Vector2 dustVel = -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 2f)
                    + Main.rand.NextVector2Circular(1f, 1f);
                int dust = Dust.NewDust(Projectile.Center - new Vector2(4), 8, 8,
                    ModContent.DustType<FuneralAshDust>(), dustVel.X, dustVel.Y, 0,
                    FuneralUtils.EmberCore, Main.rand.NextFloat(0.5f, 0.9f));
                Main.dust[dust].noGravity = true;
            }
        }

        private void SpawnDeathParticles()
        {
            // Impact bloom
            Color bloomColor = FuneralUtils.PrayerFlame;
            bloomColor.A = 0;
            FuneralParticleHandler.SpawnParticle(new ConvergenceBloomParticle(
                Projectile.Center, Vector2.Zero, bloomColor, 0.8f, 12
            ));

            // Radial flame burst
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f + Main.rand.NextFloatDirection() * 0.2f;
                FuneralParticleHandler.SpawnParticle(new FuneralFlameParticle(
                    Projectile.Center,
                    angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f) + new Vector2(0, -0.5f),
                    Color.Lerp(FuneralUtils.SmolderingAmber, FuneralUtils.DeepCrimson, Main.rand.NextFloat()),
                    Main.rand.NextFloat(0.4f, 0.7f),
                    Main.rand.Next(14, 26)
                ));
            }

            // Requiem spark burst
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                FuneralParticleHandler.SpawnParticle(new RequiemSparkParticle(
                    Projectile.Center,
                    angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f),
                    FuneralUtils.PrayerFlame,
                    Main.rand.NextFloat(0.3f, 0.6f),
                    Main.rand.Next(8, 16)
                ));
            }
        }

        #endregion

        public override void OnKill(int timeLeft)
        {
            SpawnDeathParticles();
        }

        #region Rendering

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;

            // 6ﾃ・ spritesheet frame calculation
            int frameWidth = tex.Width / FramesPerRow;
            int frameHeight = tex.Height / FrameRows;
            int col = currentFrame % FramesPerRow;
            int row = currentFrame / FramesPerRow;
            Rectangle sourceRect = new Rectangle(col * frameWidth, row * frameHeight, frameWidth, frameHeight);
            Vector2 origin = new Vector2(frameWidth / 2f, frameHeight / 2f);

            // ── Layer 1: GPU Flame Trail ──
            DrawFlameTrail(sb);

            // ── Layer 1b: Energy Surge Ribbon overlay (RibbonFoundation Mode 6) ──
            DrawEnergySurgeRibbon(sb);

            // ── Layer 2: Afterimage chain ──
            DrawAfterimages(sb, tex, sourceRect, origin);

            // ── Layer 3: Core spritesheet frame ──
            DrawCore(sb, tex, sourceRect, origin, lightColor);

            // ── Layer 4: Additive bloom ──
            DrawBloomOverlay(sb, origin, frameWidth, frameHeight);

            // Eroica theme accent
            EroicaVFXLibrary.BeginEroicaAdditive(sb);
            EroicaVFXLibrary.DrawThemeSakuraAccent(sb, Projectile.Center, 1f, 0.4f);
            EroicaVFXLibrary.EndEroicaAdditive(sb);

            return false;
        }

        /// <summary>
        /// Energy Surge ribbon overlay (RibbonFoundation Mode 6 pattern).
        /// Draws EnergySurgeBeam texture strip along the trail for a fiery comet wake.
        /// </summary>
        private void DrawEnergySurgeRibbon(SpriteBatch sb)
        {
            if (AgeTimer < 3) return;

            int validCount = 0;
            for (int i = 0; i < TrailLength; i++)
            {
                if (trailPositions[i] != Vector2.Zero)
                    validCount++;
                else
                    break;
            }
            if (validCount < 3) return;

            Texture2D stripTex = RBFTextures.EnergySurgeBeam.Value;
            if (stripTex == null) return;

            int texW = stripTex.Width;
            int texH = stripTex.Height;
            float time = (float)Main.timeForVisualEffects * 0.006f;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);
            try
            {

            int srcWidth = Math.Max(1, texW / validCount);

            for (int i = 0; i < validCount - 1; i++)
            {
                float progress = (float)i / validCount;
                float fade = progress * progress;
                if (fade < 0.01f) continue;

                // Width: wide comet head tapering to narrow tail
                float width = MathHelper.Lerp(3f, 16f, progress);

                Vector2 segDir = trailPositions[i] - trailPositions[i + 1];
                float segLength = segDir.Length();
                if (segLength < 0.5f) continue;
                float segAngle = segDir.ToRotation();

                float uStart = (progress + time * 3f) % 1f;
                int srcX = (int)(uStart * texW) % texW;
                Rectangle srcRect = new Rectangle(srcX, 0, srcWidth, texH);

                float scaleX = segLength / (float)srcWidth;
                float scaleY = width / (float)texH;

                Vector2 pos = trailPositions[i] - Main.screenPosition;
                Vector2 origin = new Vector2(0, texH / 2f);

                // Crimson corona → ember amber gradient
                Color bodyColor = Color.Lerp(FuneralUtils.SmolderingAmber, FuneralUtils.DeepCrimson, progress) with { A = 0 };
                sb.Draw(stripTex, pos, srcRect, bodyColor * (fade * 0.6f), segAngle, origin,
                    new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);

                // Hot core near head
                if (progress > 0.5f)
                {
                    float coreFade = (progress - 0.5f) / 0.5f;
                    Color coreColor = FuneralUtils.PrayerFlame with { A = 0 };
                    sb.Draw(stripTex, pos, srcRect, coreColor * (fade * coreFade * 0.3f), segAngle, origin,
                        new Vector2(scaleX * 0.5f, scaleY * 0.5f), SpriteEffects.None, 0f);
                }
            }

            }
            finally
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);
            }
        }

        private void DrawFlameTrail(SpriteBatch sb)
        {
            if (AgeTimer < 3) return;

            int validCount = 0;
            for (int i = 0; i < TrailLength; i++)
            {
                if (trailPositions[i] != Vector2.Zero)
                    validCount++;
                else
                    break;
            }
            if (validCount < 3) return;

            Vector2[] positions = new Vector2[validCount];
            Array.Copy(trailPositions, positions, validCount);

            var settings = new FuneralTrailSettings(
                completionRatio => MathHelper.Lerp(12f, 3f, completionRatio),
                completionRatio =>
                {
                    float fade = (1f - completionRatio);
                    fade = fade * fade;
                    Color baseCol = Color.Lerp(FuneralUtils.PrayerFlame, FuneralUtils.FuneralBlack, completionRatio * 0.6f);
                    return baseCol * fade * 0.5f;
                },
                smoothen: true
            );

            sb.End();
            try
            {
                FuneralTrailRenderer.RenderTrail(positions, settings);
            }
            finally
            {
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }

        private void DrawAfterimages(SpriteBatch sb, Texture2D tex, Rectangle sourceRect, Vector2 origin)
        {
            int imageCount = 6;
            FuneralUtils.EnterShaderRegion(sb);

            for (int i = imageCount - 1; i >= 0; i--)
            {
                float progress = (float)i / imageCount;
                float trailIndex = progress * (TrailLength - 1);
                int idx = (int)trailIndex;
                float lerp = trailIndex - idx;

                if (idx + 1 >= TrailLength) continue;
                if (trailPositions[idx] == Vector2.Zero || trailPositions[idx + 1] == Vector2.Zero) continue;

                Vector2 pos = Vector2.Lerp(trailPositions[idx], trailPositions[idx + 1], lerp) - Main.screenPosition;
                float rot = MathHelper.Lerp(trailRotations[idx], trailRotations[idx + 1], lerp);

                float fadeFactor = (1f - progress);
                fadeFactor = fadeFactor * fadeFactor;
                Color drawColor = Color.Lerp(FuneralUtils.SmolderingAmber, FuneralUtils.DeepCrimson, progress) * (fadeFactor * 0.3f);
                drawColor.A = 0;

                float scale = Projectile.scale * (1f - progress * 0.2f);
                sb.Draw(tex, pos, sourceRect, drawColor, rot, origin, scale, SpriteEffects.None, 0f);
            }

            FuneralUtils.ExitShaderRegion(sb);
        }

        private void DrawCore(SpriteBatch sb, Texture2D tex, Rectangle sourceRect, Vector2 origin, Color lightColor)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Color coreTint = Color.Lerp(lightColor, FuneralUtils.PrayerFlame, 0.5f);

            // Base spritesheet frame
            sb.Draw(tex, drawPos, sourceRect, coreTint, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);

            // Inner hot core overlay
            Color coreGlow = FuneralUtils.SoulWhite;
            coreGlow.A = 0;
            sb.Draw(tex, drawPos, sourceRect, coreGlow * 0.35f, Projectile.rotation, origin,
                Projectile.scale * 0.9f, SpriteEffects.None, 0f);
        }

        private void DrawBloomOverlay(SpriteBatch sb, Vector2 origin, int frameWidth, int frameHeight)
        {
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Color bloomColor = FuneralUtils.SmolderingAmber;
            bloomColor.A = 0;
            float pulse = (float)Math.Sin(AgeTimer * 0.18f) * 0.12f;
            float bloomScale = Projectile.scale * 1.6f + pulse;

            FuneralUtils.EnterShaderRegion(sb);
            sb.Draw(tex, drawPos, new Rectangle(0, 0, frameWidth, frameHeight),
                bloomColor * 0.3f, Projectile.rotation, origin, bloomScale, SpriteEffects.None, 0f);
            FuneralUtils.ExitShaderRegion(sb);

            // === THEME-SPECIFIC: ER Rising Ember glow — smoldering embers around the prayer orb ===
            Texture2D emberTex = EroicaThemeTextures.ERRisingEmber;
            if (emberTex != null)
            {
                Color emberColor = EroicaVFXLibrary.GetPaletteColor(0.25f) with { A = 0 };
                float emberPulse = 0.85f + 0.15f * (float)Math.Sin(AgeTimer * 0.1f);
                // Two counter-rotating ember halos
                sb.Draw(emberTex, drawPos, null, emberColor * 0.18f * emberPulse,
                    AgeTimer * 0.02f, emberTex.Size() * 0.5f, Projectile.scale * 0.4f, SpriteEffects.None, 0f);
                sb.Draw(emberTex, drawPos, null, emberColor * 0.12f,
                    -AgeTimer * 0.015f, emberTex.Size() * 0.5f, Projectile.scale * 0.55f, SpriteEffects.None, 0f);
            }

            // === THEME-SPECIFIC: ER Harmonic Resonance Wave behind funeral prayer ===
            Texture2D waveTex = EroicaThemeTextures.ERHarmonicImpact;
            if (waveTex != null)
            {
                Color waveColor = Color.Lerp(EroicaVFXLibrary.Scarlet, EroicaVFXLibrary.Gold, 0.3f) with { A = 0 };
                sb.Draw(waveTex, drawPos, null, waveColor * 0.1f,
                    0f, waveTex.Size() * 0.5f, Projectile.scale * 0.65f, SpriteEffects.None, 0f);
            }
        }

        #endregion
    }
}