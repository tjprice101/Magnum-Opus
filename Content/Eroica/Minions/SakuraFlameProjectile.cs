using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Common.Systems;
using MagnumOpus.Content.FoundationWeapons.RibbonFoundation;
using ReLogic.Content;

namespace MagnumOpus.Content.Eroica.Minions
{
    /// <summary>
    /// Sakura Flame 窶・dark flamethrower projectile from Sakura of Fate minion.
    /// Self-contained VFX: flame trail, dark fire afterimages, impact sparks.
    /// </summary>
    public class SakuraFlameProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarSoft";

        private float fadeProgress = 0f;
        private float ageTimer = 0f;

        // 笏笏 Trail tracking 笏笏
        private const int TrailLength = 12;
        private Vector2[] trailPositions = new Vector2[TrailLength];
        private float[] trailRotations = new float[TrailLength];
        private bool trailInitialized = false;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionShot[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 3;
            Projectile.height = 3;
            Projectile.scale = 0.35f;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 45;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.extraUpdates = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.alpha = 100;
        }

        public override void AI()
        {
            ageTimer++;
            fadeProgress = 1f - (Projectile.timeLeft / 45f);

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.rotation += (float)Math.Sin(Main.GameUpdateCount * 0.5f + Projectile.whoAmI) * 0.1f;

            // Slow down slightly
            Projectile.velocity *= 0.98f;
            Projectile.velocity += Main.rand.NextVector2Circular(0.15f, 0.15f);

            // 笏笏 Trail tracking 笏笏
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

            // 笏笏 Flight particles 笏笏
            SpawnFlightParticles();
        }

        #region Particle Spawning

        private void SpawnFlightParticles()
        {
            // Dark flame wisps trailing behind
            if (Main.rand.NextBool(2))
            {
                Color flameColor = Color.Lerp(FinalityUtils.AbyssalCrimson, FinalityUtils.EmberGold, Main.rand.NextFloat(0.3f));
                FinalityParticleHandler.SpawnParticle(new DarkFlameParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(3f, 3f),
                    -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 1.5f)
                        + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    flameColor,
                    Main.rand.NextFloat(0.05f, 0.1f),
                    Main.rand.Next(6, 14)
                ));
            }

            // Dust trail
            if (Main.rand.NextBool(3))
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    ModContent.DustType<FinalityDust>(), 0f, 0f, 0,
                    FinalityUtils.AbyssalCrimson, Main.rand.NextFloat(0.1f, 0.25f));
                Main.dust[dust].noGravity = true;
            }
        }

        private void SpawnImpactParticles()
        {
            // Spark burst
            int sparkCount = 5;
            for (int i = 0; i < sparkCount; i++)
            {
                float angle = MathHelper.TwoPi * i / sparkCount + Main.rand.NextFloatDirection() * 0.3f;
                FinalityParticleHandler.SpawnParticle(new FateSpark(
                    Projectile.Center,
                    angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f),
                    Color.Lerp(FinalityUtils.EmberGold, FinalityUtils.SakuraFlame, Main.rand.NextFloat()),
                    Main.rand.NextFloat(0.2f, 0.4f),
                    Main.rand.Next(6, 12)
                ));
            }

            // Small bloom
            Color bloom = FinalityUtils.SakuraFlame;
            bloom.A = 0;
            FinalityParticleHandler.SpawnParticle(new DarkBloomParticle(
                Projectile.Center, Vector2.Zero, bloom, 0.4f, 10
            ));

            // Additional wider crimson bloom for richer explosion
            Color outerBloom = FinalityUtils.AbyssalCrimson;
            outerBloom.A = 0;
            FinalityParticleHandler.SpawnParticle(new DarkBloomParticle(
                Projectile.Center, Vector2.Zero, outerBloom, 0.55f, 12
            ));

            // White-hot core flash
            Color coreFlash = FinalityUtils.CoreWhite;
            coreFlash.A = 0;
            FinalityParticleHandler.SpawnParticle(new DarkBloomParticle(
                Projectile.Center, Vector2.Zero, coreFlash, 0.2f, 6
            ));
        }

        #endregion

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Seeking crystals
            if (Main.rand.NextBool(4))
            {
                SeekingCrystalHelper.SpawnEroicaCrystals(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    Projectile.velocity,
                    (int)(damageDone * 0.15f),
                    Projectile.knockBack,
                    Projectile.owner,
                    3
                );
            }

            // Hit spark
            for (int i = 0; i < 3; i++)
            {
                FinalityParticleHandler.SpawnParticle(new FateSpark(
                    target.Center + Main.rand.NextVector2Circular(8f, 8f),
                    Main.rand.NextVector2CircularEdge(3f, 3f),
                    Color.Lerp(FinalityUtils.EmberGold, FinalityUtils.CoreWhite, Main.rand.NextFloat(0.3f)),
                    Main.rand.NextFloat(0.2f, 0.4f),
                    Main.rand.Next(5, 10)
                ));
            }
        }

        public override void OnKill(int timeLeft)
        {
            SpawnImpactParticles();
        }

        #region Rendering

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = tex.Size() / 2f;

            // ── Layer 1: GPU Flame Trail ──
            DrawFlameTrail(sb);

            // ── Layer 1b: Ember Drift Ribbon (RibbonFoundation Mode 5) ──
            DrawEmberDriftRibbon(sb);

            // ── Layer 2: Afterimages ──
            DrawAfterimages(sb, tex, origin);

            // ── Layer 3: Core flame sprite ──
            DrawCore(sb, tex, origin, lightColor);

            // ── Layer 4: Additive bloom ──
            DrawBloom(sb, tex, origin);

            // Eroica theme accent
            EroicaVFXLibrary.BeginEroicaAdditive(sb);
            EroicaVFXLibrary.DrawThemeSakuraAccent(sb, Projectile.Center, 0.35f, 0.14f);
            EroicaVFXLibrary.EndEroicaAdditive(sb);

            }
            catch { }
            finally
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            return false;
        }

        private void DrawFlameTrail(SpriteBatch sb)
        {
            if (ageTimer < 2) return;

            int validCount = 0;
            for (int i = 0; i < TrailLength; i++)
            {
                if (trailPositions[i] != Vector2.Zero) validCount++;
                else break;
            }
            if (validCount < 3) return;

            Vector2[] positions = new Vector2[validCount];
            Array.Copy(trailPositions, positions, validCount);

            var settings = new FinalityTrailSettings(
                completionRatio => MathHelper.Lerp(1.75f, 0.2f, completionRatio),
                completionRatio =>
                {
                    float fade = (1f - completionRatio);
                    fade = fade * fade;
                    Color baseCol = Color.Lerp(FinalityUtils.SakuraFlame, FinalityUtils.AbyssalCrimson, completionRatio * 0.7f);
                    return baseCol * fade * 0.5f;
                },
                smoothen: true
            );

            try
            {
                sb.End();
                FinalityTrailRenderer.RenderTrail(positions, settings);
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            catch { }
        }

        /// <summary>
        /// RibbonFoundation Mode 5 (Ember Drift) — crimson-violet ember texture strip overlay.
        /// Width 12→2, fast decay matching the 45-tick flame lifetime.
        /// </summary>
        private void DrawEmberDriftRibbon(SpriteBatch sb)
        {
            if (ageTimer < 2) return;

            int validCount = 0;
            for (int i = 0; i < TrailLength; i++)
            {
                if (trailPositions[i] != Vector2.Zero) validCount++;
                else break;
            }
            if (validCount < 3) return;

            // Use RibbonFoundation BasicTrail strip texture for ember drift ribbon
            Texture2D stripTex = RBFTextures.BasicTrail.Value;
            if (stripTex == null) return;

            int texW = stripTex.Width;
            int texH = stripTex.Height;
            float time = (float)Main.timeForVisualEffects * 0.008f;
            const float WidthHead = 4.2f;
            const float WidthTail = 0.7f;
            float lifeAlpha = 1f - fadeProgress;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            try
            {
            int srcWidth = Math.Max(1, texW / validCount);

            for (int i = 0; i < validCount - 1; i++)
            {
                float progress = 1f - (float)i / validCount; // 1 = head, 0 = tail
                float fade = progress * progress * lifeAlpha;
                if (fade < 0.01f) continue;

                float width = MathHelper.Lerp(WidthTail, WidthHead, progress);

                Vector2 segDir = trailPositions[i] - trailPositions[i + 1];
                float segLength = segDir.Length();
                if (segLength < 0.5f) continue;
                float segAngle = segDir.ToRotation();

                float uStart = ((float)i / validCount + time * 4f) % 1f;
                int srcX = (int)(uStart * texW) % texW;
                Rectangle srcRect = new Rectangle(srcX, 0, srcWidth, texH);

                float scaleX = segLength / (float)srcWidth;
                float scaleY = width / (float)texH;

                Vector2 pos = trailPositions[i] - Main.screenPosition;
                Vector2 drawOrigin = new Vector2(0, texH / 2f);

                // Crimson-violet ember body
                Color bodyColor = Color.Lerp(FinalityUtils.AbyssalCrimson, FinalityUtils.FateViolet, progress * 0.4f) with { A = 0 };
                sb.Draw(stripTex, pos, srcRect, bodyColor * (fade * 0.4f), segAngle, drawOrigin,
                    new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);

                // Ember-gold hot inner line
                if (progress > 0.5f)
                {
                    Color hotColor = FinalityUtils.EmberGold with { A = 0 };
                    sb.Draw(stripTex, pos, srcRect, hotColor * (fade * 0.15f), segAngle, drawOrigin,
                        new Vector2(scaleX, scaleY * 0.5f), SpriteEffects.None, 0f);
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

        private void DrawAfterimages(SpriteBatch sb, Texture2D tex, Vector2 origin)
        {
            int imageCount = 5;
            FinalityUtils.EnterShaderRegion(sb);

            for (int i = imageCount - 1; i >= 0; i--)
            {
                float progress = (float)i / imageCount;
                float trailIndex = progress * (TrailLength - 1);
                int idx = (int)trailIndex;
                float frac = trailIndex - idx;

                if (idx + 1 >= TrailLength) continue;
                if (trailPositions[idx] == Vector2.Zero || trailPositions[idx + 1] == Vector2.Zero) continue;

                Vector2 pos = Vector2.Lerp(trailPositions[idx], trailPositions[idx + 1], frac) - Main.screenPosition;
                float rot = MathHelper.Lerp(trailRotations[idx], trailRotations[idx + 1], frac);

                float fadeFactor = (1f - progress);
                fadeFactor *= fadeFactor;
                Color afterColor = Color.Lerp(FinalityUtils.SakuraFlame, FinalityUtils.AbyssalCrimson, progress * 0.5f) * (fadeFactor * 0.3f);
                afterColor.A = 0;

                float scale = Projectile.scale * (1f - progress * 0.2f) * (1f - fadeProgress * 0.3f);
                sb.Draw(tex, pos, null, afterColor, rot, origin, scale, SpriteEffects.None, 0f);
            }

            FinalityUtils.ExitShaderRegion(sb);
        }

        private void DrawCore(SpriteBatch sb, Texture2D tex, Vector2 origin, Color lightColor)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float lifeAlpha = 1f - fadeProgress * 0.5f;
            Color coreTint = Color.Lerp(FinalityUtils.SakuraFlame, FinalityUtils.EmberGold, 0.4f) * lifeAlpha;

            sb.Draw(tex, drawPos, null, coreTint, Projectile.rotation, origin,
                Projectile.scale * (1f - fadeProgress * 0.3f), SpriteEffects.None, 0f);

            // Hot core
            Color hotCore = FinalityUtils.CoreWhite;
            hotCore.A = 0;
            sb.Draw(tex, drawPos, null, hotCore * (0.4f * lifeAlpha), Projectile.rotation, origin,
                Projectile.scale * 0.6f * (1f - fadeProgress * 0.3f), SpriteEffects.None, 0f);
        }

        private void DrawBloom(SpriteBatch sb, Texture2D tex, Vector2 origin)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float bloomAlpha = (1f - fadeProgress) * 0.45f;
            float bloomScale = Projectile.scale * 0.7f * (1f - fadeProgress * 0.4f);

            FinalityUtils.EnterShaderRegion(sb);

            // Outer crimson bloom haze
            Color outerColor = FinalityUtils.AbyssalCrimson;
            outerColor.A = 0;
            sb.Draw(tex, drawPos, null, outerColor * bloomAlpha * 0.6f, Projectile.rotation, origin, bloomScale * 1.4f, SpriteEffects.None, 0f);

            // Main sakura flame bloom
            Color bloomColor = FinalityUtils.SakuraFlame;
            bloomColor.A = 0;
            sb.Draw(tex, drawPos, null, bloomColor * bloomAlpha, Projectile.rotation, origin, bloomScale, SpriteEffects.None, 0f);

            // Ember-gold hot core
            Color coreColor = FinalityUtils.EmberGold;
            coreColor.A = 0;
            sb.Draw(tex, drawPos, null, coreColor * bloomAlpha * 0.5f, Projectile.rotation, origin, bloomScale * 0.5f, SpriteEffects.None, 0f);

            FinalityUtils.ExitShaderRegion(sb);
        }

        #endregion
    }
}
