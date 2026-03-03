using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// Special crescendo projectile fired every 10th shot from Piercing Light of the Sakura.
    /// Self-contained VFX: multi-layer rendering with 6ﾃ・ spritesheet, GPU trail,
    /// afterimage chain, lightning spark particles, and impact lightning explosions.
    /// 
    /// ai[0] = chargeProgress (0-1, always 1.0 for crescendo shots)
    /// ai[1] = age timer
    /// </summary>
    public class PiercingLightOfTheSakuraProjectile : ModProjectile
    {
        // 笏笏 Animation 笏笏 6ﾃ・ sprite sheet
        private const int FrameColumns = 6;
        private const int FrameRows = 6;
        private const int TotalFrames = 36;
        private const int FrameTime = 2;

        private int frameCounter = 0;
        private int currentFrame = 0;

        // 笏笏 AI state accessors 笏笏
        private ref float ChargeProgress => ref Projectile.ai[0];
        private ref float AgeTimer => ref Projectile.ai[1];

        // 笏笏 Trail tracking 笏笏
        private const int TrailLength = 24;
        private Vector2[] trailPositions = new Vector2[TrailLength];
        private float[] trailRotations = new float[TrailLength];
        private bool trailInitialized = false;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.light = 0.8f;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            AgeTimer++;

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

            // Pulsating scale 窶・intense crescendo projectile
            float scalePulse = (float)Math.Sin(AgeTimer * 0.08f) * 0.06f;
            Projectile.scale = 1.0f + ChargeProgress * 0.15f + scalePulse;

            // Update sprite sheet animation
            frameCounter++;
            if (frameCounter >= FrameTime)
            {
                frameCounter = 0;
                currentFrame++;
                if (currentFrame >= TotalFrames)
                    currentFrame = 0;
            }

            // 笏笏 Particle Spawning 笏笏
            SpawnParticles();

            // 笏笏 Crescendo flash on first frame 笏笏
            if (AgeTimer == 1)
                SpawnCrescendoFlash();
        }

        #region Particle Spawning

        private void SpawnParticles()
        {
            // Lightning sparks flying off the projectile
            if (Main.rand.NextBool(2))
            {
                Vector2 perpendicular = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2);
                Vector2 sparkVel = -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1.5f, 4f)
                    + perpendicular * Main.rand.NextFloatDirection() * 2.5f;

                Color sparkColor = Color.Lerp(PiercingUtils.LightGold, PiercingUtils.LightningCore, Main.rand.NextFloat());
                PiercingParticleHandler.SpawnParticle(new LightningSparkParticle(
                    Projectile.Center + perpendicular * Main.rand.NextFloatDirection() * 6f,
                    sparkVel,
                    sparkColor,
                    Main.rand.NextFloat(0.3f, 0.7f),
                    Main.rand.Next(6, 14)
                ));
            }

            // Energy wisps along trail
            if (Main.rand.NextBool(4))
            {
                Vector2 wispVel = Main.rand.NextVector2Circular(0.4f, 0.4f);
                PiercingParticleHandler.SpawnParticle(new SniperTrailParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    wispVel,
                    Color.Lerp(PiercingUtils.LightGold, PiercingUtils.SakuraGlow, Main.rand.NextFloat()) * 0.5f,
                    Main.rand.NextFloat(0.4f, 0.7f),
                    Main.rand.Next(15, 30)
                ));
            }

            // Music notes drifting from flight path
            if (Main.rand.NextBool(8))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloatDirection() * 0.8f, -Main.rand.NextFloat(0.5f, 1.2f));
                PiercingParticleHandler.SpawnParticle(new CrescendoNoteParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(12f, 12f),
                    noteVel,
                    Color.Lerp(PiercingUtils.LightGold, PiercingUtils.CrescendoPink, Main.rand.NextFloat()),
                    Main.rand.NextFloat(0.3f, 0.55f),
                    Main.rand.Next(30, 50)
                ));
            }
        }

        private void SpawnCrescendoFlash()
        {
            // Massive golden flash at spawn
            Color flashColor = PiercingUtils.BrilliantWhite;
            flashColor.A = 0;
            PiercingParticleHandler.SpawnParticle(new CrescendoFlashParticle(
                Projectile.Center,
                Vector2.Zero,
                flashColor,
                1.2f,
                10
            ));

            // Burst of lightning sparks
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f + Main.rand.NextFloatDirection() * 0.3f;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f);
                PiercingParticleHandler.SpawnParticle(new LightningSparkParticle(
                    Projectile.Center,
                    sparkVel,
                    Color.Lerp(PiercingUtils.LightGold, PiercingUtils.LightningCore, Main.rand.NextFloat()),
                    Main.rand.NextFloat(0.4f, 0.7f),
                    Main.rand.Next(8, 16)
                ));
            }

            // Crescendo music notes fanning out
            for (int i = 0; i < 5; i++)
            {
                Vector2 noteVel = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(MathHelper.ToRadians(60))
                    * Main.rand.NextFloat(1f, 3f);
                noteVel.Y -= Main.rand.NextFloat(0.5f, 1.5f);
                PiercingParticleHandler.SpawnParticle(new CrescendoNoteParticle(
                    Projectile.Center,
                    noteVel,
                    PiercingUtils.LightGold,
                    Main.rand.NextFloat(0.4f, 0.65f),
                    Main.rand.Next(35, 55)
                ));
            }
        }

        private void SpawnImpactParticles()
        {
            // Impact star burst
            Color impactColor = PiercingUtils.BrilliantWhite;
            impactColor.A = 0;
            PiercingParticleHandler.SpawnParticle(new PiercingImpactParticle(
                Projectile.Center,
                Vector2.Zero,
                impactColor,
                1.0f,
                14
            ));

            // Radial lightning spark burst
            int sparkCount = 12;
            for (int i = 0; i < sparkCount; i++)
            {
                float angle = MathHelper.TwoPi * i / sparkCount + Main.rand.NextFloatDirection() * 0.2f;
                float speed = Main.rand.NextFloat(3f, 8f);
                PiercingParticleHandler.SpawnParticle(new LightningSparkParticle(
                    Projectile.Center,
                    angle.ToRotationVector2() * speed,
                    Color.Lerp(PiercingUtils.LightGold, PiercingUtils.LightningEdge, Main.rand.NextFloat(0.3f)),
                    Main.rand.NextFloat(0.3f, 0.8f),
                    Main.rand.Next(10, 20)
                ));
            }

            // Dust ring
            for (int i = 0; i < 10; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(5f, 5f);
                int dust = Dust.NewDust(Projectile.Center - new Vector2(4), 8, 8,
                    ModContent.DustType<PiercingLightDust>(), dustVel.X, dustVel.Y, 0,
                    PiercingUtils.LightGold, Main.rand.NextFloat(0.7f, 1.2f));
                Main.dust[dust].noGravity = true;
            }
        }

        #endregion

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SpawnImpactParticles();
            SpawnLightningExplosions(target.Center);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            SpawnImpactParticles();
            SpawnLightningExplosions(Projectile.Center);
            return true;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item62 with { Volume = 0.6f }, Projectile.Center);
            if (AgeTimer > 2)
                SpawnImpactParticles();
        }

        private void SpawnLightningExplosions(Vector2 position)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            for (int i = 0; i < 3; i++)
            {
                Vector2 offset = new Vector2(Main.rand.NextFloat(-50f, 50f), Main.rand.NextFloat(-30f, 30f));
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), position + offset, Vector2.Zero,
                    ModContent.ProjectileType<SakuraLightning>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            }
        }

        #region Rendering

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;

            // 笏笏 6ﾃ・ Spritesheet frame calculation 笏笏
            int frameWidth = tex.Width / FrameColumns;
            int frameHeight = tex.Height / FrameRows;
            int col = currentFrame % FrameColumns;
            int row = currentFrame / FrameColumns;
            Rectangle sourceRect = new Rectangle(col * frameWidth, row * frameHeight, frameWidth, frameHeight);
            Vector2 origin = new Vector2(frameWidth / 2f, frameHeight / 2f);

            // 笏笏 Layer 1: GPU Energy Trail 笏笏
            DrawEnergyTrail(sb);

            // 笏笏 Layer 2: Afterimage chain 笏笏
            DrawAfterimages(sb, tex, sourceRect, origin);

            // 笏笏 Layer 3: Core spritesheet frame with glow 笏笏
            DrawProjectileCore(sb, tex, sourceRect, origin, lightColor);

            // 笏笏 Layer 4: Additive bloom overlay 笏笏
            DrawBloomOverlay(sb, origin, frameWidth, frameHeight);

            return false;
        }

        private void DrawEnergyTrail(SpriteBatch sb)
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

            var settings = new PiercingTrailSettings(
                completionRatio => MathHelper.Lerp(8f, 2f, completionRatio),
                completionRatio =>
                {
                    float fade = (1f - completionRatio);
                    fade = fade * fade;
                    Color baseCol = Color.Lerp(PiercingUtils.LightGold, PiercingUtils.LightningEdge, completionRatio * 0.5f);
                    return baseCol * fade * 0.8f;
                },
                smoothen: true
            );

            try
            {
                sb.End();
                PiercingTrailRenderer.RenderTrail(positions, settings);
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            catch { }
        }

        private void DrawAfterimages(SpriteBatch sb, Texture2D tex, Rectangle sourceRect, Vector2 origin)
        {
            int imageCount = 8;
            Color afterColor = PiercingUtils.LightGold;

            PiercingUtils.EnterShaderRegion(sb);

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
                float alpha = fadeFactor * 0.4f;
                float scale = Projectile.scale * (1f - progress * 0.25f);

                Color drawColor = afterColor * alpha;
                drawColor.A = 0;

                sb.Draw(tex, pos, sourceRect, drawColor, rot, origin, scale, SpriteEffects.None, 0f);
            }

            PiercingUtils.ExitShaderRegion(sb);
        }

        private void DrawProjectileCore(SpriteBatch sb, Texture2D tex, Rectangle sourceRect, Vector2 origin, Color lightColor)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Color coreTint = Color.Lerp(lightColor, PiercingUtils.LightGold, 0.6f);

            // Base spritesheet frame
            sb.Draw(tex, drawPos, sourceRect, coreTint, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);

            // Inner bright core overlay
            Color coreGlow = PiercingUtils.BrilliantWhite;
            coreGlow.A = 0;
            sb.Draw(tex, drawPos, sourceRect, coreGlow * 0.5f, Projectile.rotation, origin,
                Projectile.scale * 1.05f, SpriteEffects.None, 0f);
        }

        private void DrawBloomOverlay(SpriteBatch sb, Vector2 origin, int frameWidth, int frameHeight)
        {
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Color bloomColor = PiercingUtils.LightGold;
            bloomColor.A = 0;
            float bloomAlpha = 0.4f;
            float bloomScale = Projectile.scale * 1.8f;

            float pulse = (float)Math.Sin(AgeTimer * 0.15f) * 0.12f;
            bloomScale += pulse;

            // Use full texture as glow source since spritesheet frame is complex
            PiercingUtils.EnterShaderRegion(sb);
            sb.Draw(tex, drawPos, new Rectangle(0, 0, frameWidth, frameHeight),
                bloomColor * bloomAlpha, Projectile.rotation, origin, bloomScale, SpriteEffects.None, 0f);
            PiercingUtils.ExitShaderRegion(sb);
        }

        #endregion
    }
}