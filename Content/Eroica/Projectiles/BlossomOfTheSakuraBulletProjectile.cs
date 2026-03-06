using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Content.Eroica;
using MagnumOpus.Content.FoundationWeapons.RibbonFoundation;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// Bullet projectile for Blossom of the Sakura 窶・heat-reactive homing tracer round.
    /// 
    /// Self-contained VFX: Multi-layer rendering with GPU trail, afterimage chain,
    /// heat-reactive glow core, and particle spawning.
    /// 
    /// ai[0] = heatProgress (0-1, set by weapon item via Shoot)
    /// ai[1] = age timer
    /// </summary>
    public class BlossomOfTheSakuraBulletProjectile : ModProjectile
    {
        private ref float HeatProgress => ref Projectile.ai[0];
        private ref float AgeTimer => ref Projectile.ai[1];

        private int targetNPC = -1;

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
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.alpha = 0;
            Projectile.light = 0.6f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            AgeTimer++;
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

            // Pulsating visual scale 窶・subtle heat shimmer
            float pulse = (float)Math.Sin(AgeTimer * 0.15f) * 0.06f;
            Projectile.scale = 1f + pulse + HeatProgress * 0.1f;

            // 笏笏 Particle Spawning 笏笏
            SpawnTracerParticles();

            // 笏笏 Muzzle flash on first frame 笏笏
            if (AgeTimer == 1)
                SpawnMuzzleFlash();

            // 笏笏 HOMING AI 窶・boss-priority with gentle tracking 笏笏
            if (targetNPC < 0 || !Main.npc[targetNPC].active)
            {
                targetNPC = -1;
                float maxDistance = 850f;
                bool foundBoss = false;

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.boss && !npc.dontTakeDamage)
                    {
                        float distance = Vector2.Distance(Projectile.Center, npc.Center);
                        if (distance < maxDistance)
                        {
                            maxDistance = distance;
                            targetNPC = i;
                            foundBoss = true;
                        }
                    }
                }

                if (!foundBoss)
                {
                    maxDistance = 650f;
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage)
                        {
                            float distance = Vector2.Distance(Projectile.Center, npc.Center);
                            if (distance < maxDistance)
                            {
                                maxDistance = distance;
                                targetNPC = i;
                            }
                        }
                    }
                }
            }

            // Gentle homing 窶・tighter when hot, looser when cool
            if (targetNPC >= 0 && Main.npc[targetNPC].active)
            {
                Vector2 direction = (Main.npc[targetNPC].Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                float speed = Projectile.velocity.Length();
                float turnWeight = 28f - HeatProgress * 6f;
                turnWeight = MathHelper.Clamp(turnWeight, 20f, 30f);
                Projectile.velocity = (Projectile.velocity * turnWeight + direction * speed) / (turnWeight + 1f);
            }
        }

        #region Particle Spawning

        private void SpawnTracerParticles()
        {
            // Tracer sparks flying off the bullet trail
            if (Main.rand.NextBool(3))
            {
                Vector2 perpendicular = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2);
                Vector2 sparkOffset = perpendicular * Main.rand.NextFloatDirection() * 4f;
                Vector2 sparkVel = -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 3f)
                    + perpendicular * Main.rand.NextFloatDirection() * 1.5f;

                Color sparkColor = BlossomUtils.GetHeatGradient(HeatProgress);
                BlossomParticleHandler.SpawnParticle(new TracerSparkParticle(
                    Projectile.Center + sparkOffset,
                    sparkVel,
                    sparkColor,
                    Main.rand.NextFloat(0.3f, 0.6f + HeatProgress * 0.3f),
                    Main.rand.Next(8, 16)
                ));
            }

            // Heat shimmer particles 窶・more frequent at high heat
            if (HeatProgress > 0.3f && Main.rand.NextBool(Math.Max(1, (int)(6 - HeatProgress * 4))))
            {
                Vector2 shimmerOffset = Main.rand.NextVector2Circular(6f, 6f);
                BlossomParticleHandler.SpawnParticle(new HeatShimmerParticle(
                    Projectile.Center + shimmerOffset,
                    new Vector2(Main.rand.NextFloatDirection() * 0.3f, -Main.rand.NextFloat(0.3f, 0.8f)),
                    Color.White * (HeatProgress * 0.4f),
                    Main.rand.NextFloat(0.4f, 0.7f),
                    Main.rand.Next(12, 24)
                ));
            }

            // Sakura petal drift 窶・more frequent at low heat (cool blossoms)
            if (HeatProgress < 0.5f && Main.rand.NextBool(6))
            {
                Vector2 petalVel = -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 1.5f)
                    + new Vector2(Main.rand.NextFloatDirection() * 0.8f, Main.rand.NextFloat(0.2f, 0.6f));
                BlossomParticleHandler.SpawnParticle(new BulletPetalParticle(
                    Projectile.Center,
                    petalVel,
                    Color.Lerp(BlossomUtils.CoolPetal, BlossomUtils.SakuraBody, Main.rand.NextFloat()),
                    Main.rand.NextFloat(0.3f, 0.55f),
                    Main.rand.Next(30, 55)
                ));
            }
        }

        private void SpawnMuzzleFlash()
        {
            // Bright flash at spawn position
            Color flashColor = BlossomUtils.GetHeatGradient(HeatProgress);
            flashColor.A = 0;
            BlossomParticleHandler.SpawnParticle(new MuzzleFlashParticle(
                Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.Zero) * 8f,
                Vector2.Zero,
                flashColor,
                0.6f + HeatProgress * 0.4f,
                8
            ));

            // Directional spark burst from muzzle
            for (int i = 0; i < 4 + (int)(HeatProgress * 4); i++)
            {
                Vector2 sparkVel = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(MathHelper.ToRadians(25))
                    * Main.rand.NextFloat(2f, 5f);
                BlossomParticleHandler.SpawnParticle(new TracerSparkParticle(
                    Projectile.Center,
                    sparkVel,
                    Color.Lerp(flashColor, BlossomUtils.WhiteHot, Main.rand.NextFloat(0.3f)),
                    Main.rand.NextFloat(0.2f, 0.5f),
                    Main.rand.Next(6, 12)
                ));
            }
        }

        private void SpawnImpactParticles()
        {
            Color impactColor = BlossomUtils.GetHeatGradient(HeatProgress);

            // Central impact bloom
            impactColor.A = 0;
            BlossomParticleHandler.SpawnParticle(new ImpactBloomParticle(
                Projectile.Center,
                Vector2.Zero,
                impactColor,
                0.8f + HeatProgress * 0.6f,
                12
            ));

            // Radial spark burst
            int sparkCount = 6 + (int)(HeatProgress * 8);
            for (int i = 0; i < sparkCount; i++)
            {
                float angle = MathHelper.TwoPi * i / sparkCount + Main.rand.NextFloatDirection() * 0.2f;
                float speed = Main.rand.NextFloat(2f, 6f + HeatProgress * 3f);
                BlossomParticleHandler.SpawnParticle(new TracerSparkParticle(
                    Projectile.Center,
                    angle.ToRotationVector2() * speed,
                    Color.Lerp(impactColor, BlossomUtils.MuzzleFlash, Main.rand.NextFloat(0.4f)),
                    Main.rand.NextFloat(0.3f, 0.7f),
                    Main.rand.Next(10, 20)
                ));
            }

            // Petal scatter on impact
            for (int i = 0; i < 3 + (int)(3 * (1f - HeatProgress)); i++)
            {
                Vector2 petalVel = Main.rand.NextVector2Circular(3f, 3f) + new Vector2(0, Main.rand.NextFloat(-1f, 0.5f));
                BlossomParticleHandler.SpawnParticle(new BulletPetalParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    petalVel,
                    Color.Lerp(BlossomUtils.CoolPetal, BlossomUtils.WarmCrimson, HeatProgress),
                    Main.rand.NextFloat(0.35f, 0.6f),
                    Main.rand.Next(25, 50)
                ));
            }

            // Dust ring for vanilla visual mixing
            for (int i = 0; i < 8; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(4f, 4f);
                int dust = Dust.NewDust(Projectile.Center - new Vector2(4), 8, 8,
                    ModContent.DustType<BlossomTracerDust>(), dustVel.X, dustVel.Y, 0, impactColor, Main.rand.NextFloat(0.6f, 1f));
                Main.dust[dust].noGravity = true;
            }
        }

        #endregion

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SpawnImpactParticles();

            // Seeking crystals 窶・25% chance
            if (Main.rand.NextBool(4) && Main.myPlayer == Projectile.owner)
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

            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.1f, Volume = 0.55f }, Projectile.position);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            SpawnImpactParticles();
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.2f, Volume = 0.4f }, Projectile.position);
            return true;
        }

        public override void OnKill(int timeLeft)
        {
            // Final burst if not already handled by OnHit/OnTileCollide
            if (AgeTimer > 2)
                SpawnImpactParticles();
        }

        #region Rendering

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = tex.Size() / 2f;
            float time = (float)Main.gameTimeCache.TotalGameTime.TotalSeconds;

            // ═══════════════════════════════════════════════════════════════════
            //  LAYER 1: GPU TRACER TRAIL — BlossomTracerTrail bloom strip
            // ═══════════════════════════════════════════════════════════════════
            DrawTracerTrail(sb);

            // ═══════════════════════════════════════════════════════════════════
            //  LAYER 2: SHADER TRACER TRAIL — TracerTrail.fx heat-reactive ribbon
            //  Uses ApplyBlossomTracerTrail for GPU-driven heat-reactive coloring.
            // ═══════════════════════════════════════════════════════════════════
            DrawShaderTracerStrip(sb, time);

            // ═══════════════════════════════════════════════════════════════════
            //  LAYER 3: SHADER AFTERIMAGE CHAIN — TracerTrailGlow pass on sprites
            // ═══════════════════════════════════════════════════════════════════
            DrawShaderAfterimages(sb, tex, origin, time);

            // ═══════════════════════════════════════════════════════════════════
            //  LAYER 4: CORE BULLET SPRITE + heat glow
            // ═══════════════════════════════════════════════════════════════════
            DrawBulletCore(sb, tex, origin, lightColor);

            // ═══════════════════════════════════════════════════════════════════
            //  LAYER 5: BLOOM + HEAT SHIMMER OVERLAY — HeatDistortion.fx
            // ═══════════════════════════════════════════════════════════════════
            DrawShaderBloomOverlay(sb, origin, time);

            // Eroica theme accent
            EroicaVFXLibrary.BeginEroicaAdditive(sb);
            EroicaVFXLibrary.DrawThemeSakuraAccent(sb, Projectile.Center, 1f, 0.4f);
            EroicaVFXLibrary.EndEroicaAdditive(sb);

            return false;
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  LAYER 2: Shader Tracer Strip — TracerTrail.fx on trail positions
        //  Heat-reactive coloring shifts from cool sakura → white-hot gold.
        //  Dual pass: TracerTrailMain body + TracerTrailGlow overlay.
        // ═══════════════════════════════════════════════════════════════════════
        private void DrawShaderTracerStrip(SpriteBatch sb, float time)
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

            Texture2D stripTex = EroicaTextures.EmberScatter?.Value ?? EroicaTextures.EnergyTrailUV?.Value;
            if (stripTex == null) return;

            int texW = stripTex.Width;
            int texH = stripTex.Height;
            float scrollTime = (float)Main.timeForVisualEffects * 0.006f;
            int srcWidth = Math.Max(1, texW / validCount);
            const float WidthHead = 8f;
            const float WidthTail = 1f;

            bool hasShader = EroicaShaderManager.HasTracerTrail;

            if (hasShader)
            {
                // ── PASS 1: TracerTrailMain body ──
                EroicaShaderManager.BeginShaderAdditive(sb);
                try
                {
                    EroicaShaderManager.ApplyBlossomTracerTrail(time, HeatProgress, glowPass: false);

                    for (int i = 0; i < validCount - 1; i++)
                    {
                        float progress = 1f - (float)i / validCount;
                        float fade = progress * progress;
                        if (fade < 0.01f) continue;

                        float width = MathHelper.Lerp(WidthTail, WidthHead, progress);
                        Vector2 segDir = trailPositions[i] - trailPositions[i + 1];
                        float segLength = segDir.Length();
                        if (segLength < 0.5f) continue;
                        float segAngle = segDir.ToRotation();

                        float uStart = ((float)i / validCount + scrollTime * 3f) % 1f;
                        int srcX = (int)(uStart * texW) % texW;
                        Rectangle srcRect = new Rectangle(srcX, 0, srcWidth, texH);

                        float scaleX = segLength / (float)srcWidth;
                        float scaleY = width / (float)texH;
                        Vector2 pos = trailPositions[i] - Main.screenPosition;
                        Vector2 drawOrigin = new Vector2(0, texH / 2f);

                        sb.Draw(stripTex, pos, srcRect, Color.White * (fade * 0.6f), segAngle, drawOrigin,
                            new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
                    }
                }
                finally
                {
                    EroicaShaderManager.RestoreSpriteBatch(sb);
                }

                // ── PASS 2: TracerTrailGlow overlay — wider, brighter ──
                EroicaShaderManager.BeginShaderAdditive(sb);
                try
                {
                    EroicaShaderManager.ApplyBlossomTracerTrail(time, HeatProgress, glowPass: true);

                    for (int i = 0; i < validCount - 1; i++)
                    {
                        float progress = 1f - (float)i / validCount;
                        float fade = progress * progress;
                        if (fade < 0.02f) continue;

                        float width = MathHelper.Lerp(WidthTail, WidthHead, progress) * 1.5f;
                        Vector2 segDir = trailPositions[i] - trailPositions[i + 1];
                        float segLength = segDir.Length();
                        if (segLength < 0.5f) continue;
                        float segAngle = segDir.ToRotation();

                        float uStart = ((float)i / validCount + scrollTime * 3f) % 1f;
                        int srcX = (int)(uStart * texW) % texW;
                        Rectangle srcRect = new Rectangle(srcX, 0, srcWidth, texH);

                        float scaleX = segLength / (float)srcWidth;
                        float scaleY = width / (float)texH;
                        Vector2 pos = trailPositions[i] - Main.screenPosition;
                        Vector2 drawOrigin = new Vector2(0, texH / 2f);

                        sb.Draw(stripTex, pos, srcRect, Color.White * (fade * 0.3f), segAngle, drawOrigin,
                            new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
                    }
                }
                finally
                {
                    EroicaShaderManager.RestoreSpriteBatch(sb);
                }
            }
            else
            {
                // Fallback: basic additive strip with heat gradient
                EroicaShaderManager.BeginAdditive(sb);
                try
                {
                    Color heatColor = BlossomUtils.GetHeatGradient(HeatProgress);
                    for (int i = 0; i < validCount - 1; i++)
                    {
                        float progress = 1f - (float)i / validCount;
                        float fade = progress * progress;
                        if (fade < 0.01f) continue;

                        float width = MathHelper.Lerp(WidthTail, WidthHead, progress);
                        Vector2 segDir = trailPositions[i] - trailPositions[i + 1];
                        float segLength = segDir.Length();
                        if (segLength < 0.5f) continue;
                        float segAngle = segDir.ToRotation();

                        float uStart = ((float)i / validCount + scrollTime * 3f) % 1f;
                        int srcX = (int)(uStart * texW) % texW;
                        Rectangle srcRect = new Rectangle(srcX, 0, srcWidth, texH);
                        float scaleX = segLength / (float)srcWidth;
                        float scaleY = width / (float)texH;
                        Vector2 pos = trailPositions[i] - Main.screenPosition;
                        Vector2 drawOrigin = new Vector2(0, texH / 2f);

                        Color bodyColor = Color.Lerp(BlossomUtils.CoolPetal, heatColor, progress * 0.6f) with { A = 0 };
                        sb.Draw(stripTex, pos, srcRect, bodyColor * (fade * 0.5f), segAngle, drawOrigin,
                            new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
                    }
                }
                finally
                {
                    EroicaShaderManager.RestoreSpriteBatch(sb);
                }
            }
        }

        private void DrawTracerTrail(SpriteBatch sb)
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

            Color trailColor = BlossomUtils.GetHeatGradient(HeatProgress);

            var settings = new BlossomTrailSettings(
                completionRatio => MathHelper.Lerp(4f + HeatProgress * 3f, 1f, completionRatio),
                completionRatio =>
                {
                    float fade = (1f - completionRatio);
                    fade = fade * fade;
                    Color baseCol = Color.Lerp(trailColor, BlossomUtils.CoolPetal, completionRatio * 0.6f);
                    return baseCol * fade * (0.6f + HeatProgress * 0.4f);
                },
                smoothen: true
            );

            sb.End();
            try
            {
                BlossomTrailRenderer.RenderTrail(positions, settings);
            }
            finally
            {
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  LAYER 3: Shader Afterimages — TracerTrailGlow on bullet sprites
        // ═══════════════════════════════════════════════════════════════════════
        private void DrawShaderAfterimages(SpriteBatch sb, Texture2D tex, Vector2 origin, float time)
        {
            int imageCount = 6 + (int)(HeatProgress * 4);

            if (EroicaShaderManager.HasTracerTrail)
            {
                EroicaShaderManager.BeginShaderAdditive(sb);
                try
                {
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
                        float alpha = fadeFactor * (0.35f + HeatProgress * 0.2f);
                        float scale = Projectile.scale * (1f - progress * 0.3f);

                        // Apply TracerTrail glow pass with per-afterimage time offset
                        EroicaShaderManager.ApplyBlossomTracerTrail(time + i * 0.05f, HeatProgress, glowPass: true);

                        sb.Draw(tex, pos, null, Color.White * alpha, rot, origin, scale, SpriteEffects.None, 0f);
                    }
                }
                finally
                {
                    EroicaShaderManager.RestoreSpriteBatch(sb);
                }
            }
            else
            {
                // Fallback: palette-colored afterimages — use TrueAdditive for A=0 colors
                Color afterColor = BlossomUtils.GetHeatGradient(HeatProgress);
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
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
                    float alpha = fadeFactor * (0.35f + HeatProgress * 0.2f);
                    float scale = Projectile.scale * (1f - progress * 0.3f);

                    Color drawColor = afterColor * alpha;
                    drawColor.A = 0;

                    sb.Draw(tex, pos, null, drawColor, rot, origin, scale, SpriteEffects.None, 0f);
                }
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }

        private void DrawBulletCore(SpriteBatch sb, Texture2D tex, Vector2 origin, Color lightColor)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Color heatTint = Color.Lerp(lightColor, BlossomUtils.GetHeatGradient(HeatProgress), 0.5f + HeatProgress * 0.4f);

            // Base bullet sprite
            sb.Draw(tex, drawPos, null, heatTint, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);

            // Bright core overlay for high heat — drawn in TrueAdditive so A=0 colors glow
            if (HeatProgress > 0.2f)
            {
                Color coreColor = BlossomUtils.GetHeatGradient(Math.Min(HeatProgress + 0.2f, 1f));
                coreColor.A = 0;
                float coreAlpha = (HeatProgress - 0.2f) * 0.8f;

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                sb.Draw(tex, drawPos, null, coreColor * coreAlpha, Projectile.rotation, origin,
                    Projectile.scale * 1.05f, SpriteEffects.None, 0f);

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  LAYER 5: Shader Bloom Overlay — HeatDistortion.fx shimmer + bloom
        //  Uses ApplyBlossomHeatShimmer for GPU-driven heat haze on hot bullets.
        // ═══════════════════════════════════════════════════════════════════════
        private void DrawShaderBloomOverlay(SpriteBatch sb, Vector2 origin, float time)
        {
            Texture2D bloomTex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Color bloomColor = BlossomUtils.GetHeatGradient(HeatProgress);
            bloomColor.A = 0;
            float bloomAlpha = 0.25f + HeatProgress * 0.35f;
            float bloomScale = Projectile.scale * (1.6f + HeatProgress * 0.8f);

            float pulse = (float)Math.Sin(AgeTimer * 0.2f) * 0.1f;
            bloomScale += pulse;

            // ── HeatDistortion shader overlay when hot ──
            if (HeatProgress > 0.3f && EroicaShaderManager.HasHeatDistortion)
            {
                EroicaShaderManager.BeginShaderAdditive(sb);
                try
                {
                    EroicaShaderManager.ApplyBlossomHeatShimmer(time, HeatProgress);
                    sb.Draw(bloomTex, drawPos, null, Color.White * bloomAlpha, Projectile.rotation, origin,
                        bloomScale * 1.2f, SpriteEffects.None, 0f);
                }
                finally
                {
                    EroicaShaderManager.RestoreSpriteBatch(sb);
                }
            }

            // Standard bloom glow (always drawn) — use TrueAdditive for A=0 premultiplied colors
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            sb.Draw(bloomTex, drawPos, null, bloomColor * bloomAlpha, Projectile.rotation, origin,
                bloomScale, SpriteEffects.None, 0f);

            // ER Power Effect Ring — drawn in TrueAdditive for A=0 colors
            Texture2D ringTex = EroicaThemeTextures.ERPowerEffectRing;
            if (ringTex != null)
            {
                Color ringColor = EroicaVFXLibrary.GetPaletteColor(0.3f + HeatProgress * 0.3f) with { A = 0 };
                float ringPulse = 0.8f + 0.2f * (float)Math.Sin(AgeTimer * 0.15f);
                sb.Draw(ringTex, drawPos, null, ringColor * 0.2f * ringPulse,
                    AgeTimer * 0.02f, ringTex.Size() * 0.5f, Projectile.scale * 0.5f * ringPulse, SpriteEffects.None, 0f);
            }

            // ER Radial Slash Star on hot bullets — drawn in additive
            if (HeatProgress > 0.5f)
            {
                Texture2D starTex = EroicaThemeTextures.ERRadialSlashStar;
                if (starTex != null)
                {
                    float starOpacity = (HeatProgress - 0.5f) * 2f * 0.3f;
                    Color starColor = EroicaPalette.Gold with { A = 0 };
                    sb.Draw(starTex, drawPos, null, starColor * starOpacity,
                        AgeTimer * 0.03f, starTex.Size() * 0.5f, Projectile.scale * 0.4f, SpriteEffects.None, 0f);
                }
            }
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
        }

        #endregion
    }
}