using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;
using MagnumOpus.Content.FoundationWeapons.SparkleProjectileFoundation;
using MagnumOpus.Content.FoundationWeapons.RibbonFoundation;
using MagnumOpus.Common.Systems.Shaders;
using ReLogic.Content;

namespace MagnumOpus.Content.Eroica.Minions
{
    /// <summary>
    /// Sakura of Fate  -- spectral dark-flame guardian minion.
    /// Self-contained VFX: 6�E�E�E spritesheet rendering, dark flame aura, ambient particles.
    /// </summary>
    public class SakuraOfFate : ModProjectile
    {
        // 笏笏 Spritesheet configuration  -- 6�E�E�E grid 笏笏
        public const int FrameColumns = 6;
        public const int FrameRows = 6;
        public const int TotalFrames = 36;
        public const int FrameTime = 4;

        private enum AIState
        {
            Idle,
            Attacking
        }

        private AIState State
        {
            get => (AIState)Projectile.ai[0];
            set => Projectile.ai[0] = (float)value;
        }

        private float Timer
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        private int frameCounter = 0;
        private int currentFrame = 0;
        private int attackCooldown = 0;
        private float hoverOffset = 0f;

        // 笏笏 Trail tracking for aura effect 笏笏
        private const int TrailLength = 14;
        private Vector2[] trailPositions = new Vector2[TrailLength];
        private bool trailInitialized = false;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 1;
            Main.projPet[Type] = true;
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 48;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 18000;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.minionSlots = 1f;
            Projectile.scale = 0.35f;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => true;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            if (!CheckActive(owner))
                return;

            Timer++;
            UpdateAnimation();
            UpdateTrailPositions();
            FloatBesidePlayer(owner);

            NPC target = FindTarget(owner);

            if (target != null)
            {
                State = AIState.Attacking;
                AttackTarget(target, owner);
            }
            else
            {
                State = AIState.Idle;
            }

            // Update facing direction
            if (target != null)
                Projectile.spriteDirection = target.Center.X > Projectile.Center.X ? 1 : -1;
            else if (Math.Abs(Projectile.velocity.X) > 0.5f)
                Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;

            // 笏笏 Ambient VFX 笏笏
            SpawnAmbientParticles();
        }

        #region Core AI

        private void UpdateAnimation()
        {
            frameCounter++;
            if (frameCounter >= FrameTime)
            {
                frameCounter = 0;
                currentFrame++;
                if (currentFrame >= TotalFrames)
                    currentFrame = 0;
            }
        }

        private void UpdateTrailPositions()
        {
            if (!trailInitialized)
            {
                for (int i = 0; i < TrailLength; i++)
                    trailPositions[i] = Projectile.Center;
                trailInitialized = true;
            }
            else
            {
                for (int i = TrailLength - 1; i > 0; i--)
                    trailPositions[i] = trailPositions[i - 1];
                trailPositions[0] = Projectile.Center;
            }
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<SakuraOfFateBuff>());
                Projectile.Kill();
                return false;
            }

            if (owner.HasBuff(ModContent.BuffType<SakuraOfFateBuff>()))
                Projectile.timeLeft = 2;

            return true;
        }

        private NPC FindTarget(Player owner)
        {
            float maxDistance = 600f;
            NPC closestTarget = null;
            float closestDist = maxDistance;

            if (owner.HasMinionAttackTargetNPC)
            {
                NPC target = Main.npc[owner.MinionAttackTargetNPC];
                if (target.CanBeChasedBy(this) && Vector2.Distance(Projectile.Center, target.Center) < maxDistance)
                    return target;
            }

            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.CanBeChasedBy(this))
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestTarget = npc;
                    }
                }
            }

            return closestTarget;
        }

        private void FloatBesidePlayer(Player owner)
        {
            hoverOffset += 0.05f;
            float hoverY = (float)Math.Sin(hoverOffset) * 8f;

            Vector2 targetPos = owner.Center + new Vector2(-50f * owner.direction, -40f + hoverY);
            Vector2 direction = targetPos - Projectile.Center;
            float distance = direction.Length();

            float speed = 10f;
            float inertia = 20f;

            if (distance > 10f)
            {
                direction.Normalize();
                direction *= Math.Min(distance / 6f, speed);
                Projectile.velocity = (Projectile.velocity * (inertia - 1) + direction) / inertia;
            }
            else
            {
                Projectile.velocity *= 0.95f;
            }

            if (distance > 1200f)
            {
                Projectile.Center = owner.Center + new Vector2(-40f * owner.direction, -30f);
                Projectile.velocity = Vector2.Zero;
                SoundEngine.PlaySound(SoundID.Item8 with { Pitch = -0.3f }, Projectile.Center);
            }
        }

        private void AttackTarget(NPC target, Player owner)
        {
            Vector2 direction = target.Center - Projectile.Center;
            float distance = direction.Length();

            attackCooldown--;
            if (attackCooldown <= 0 && distance < 400f && Main.myPlayer == Projectile.owner)
            {
                FireFlameProjectile(target);
                attackCooldown = 4;
            }
        }

        private void FireFlameProjectile(NPC target)
        {
            Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
            float speed = 14f + Main.rand.NextFloat(-2f, 2f);
            Vector2 velocity = toTarget.RotatedByRandom(0.15f) * speed;

            if (Main.rand.NextBool(8))
                SoundEngine.PlaySound(SoundID.Item34 with { Pitch = 0.3f, Volume = 0.3f }, Projectile.Center);

            // Muzzle flash particle on fire
            Color flashColor = FinalityUtils.EmberGold;
            flashColor.A = 0;
            FinalityParticleHandler.SpawnParticle(new DarkBloomParticle(
                Projectile.Center, Vector2.Zero, flashColor, 0.3f, 6
            ));

            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                velocity,
                ModContent.ProjectileType<SakuraFlameProjectile>(),
                Projectile.damage / 3,
                Projectile.knockBack * 0.3f,
                Projectile.owner
            );
        }

        #endregion

        #region Ambient VFX

        private void SpawnAmbientParticles()
        {
            // Dark flame wisps hovering around minion
            if (Main.rand.NextBool(3))
            {
                Vector2 offset = Main.rand.NextVector2Circular(18f, 18f);
                Vector2 vel = new Vector2(Main.rand.NextFloatDirection() * 0.3f, -Main.rand.NextFloat(0.2f, 0.6f));
                Color flameColor = Color.Lerp(FinalityUtils.AbyssalCrimson, FinalityUtils.FateViolet, Main.rand.NextFloat());

                FinalityParticleHandler.SpawnParticle(new DarkFlameParticle(
                    Projectile.Center + offset, vel, flameColor,
                    Main.rand.NextFloat(0.25f, 0.5f), Main.rand.Next(15, 30)
                ));
            }

            // Occasional ash mote
            if (Main.rand.NextBool(8))
            {
                FinalityParticleHandler.SpawnParticle(new SummonAshParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(20f, 20f),
                    new Vector2(Main.rand.NextFloatDirection() * 0.4f, -Main.rand.NextFloat(0.1f, 0.3f)),
                    FinalityUtils.AshGray * 0.6f,
                    Main.rand.NextFloat(0.2f, 0.4f),
                    Main.rand.Next(25, 45)
                ));
            }

            // Rare music note
            if (Main.rand.NextBool(40))
            {
                FinalityParticleHandler.SpawnParticle(new FinalityNoteParticle(
                    Projectile.Center + new Vector2(Main.rand.NextFloatDirection() * 15f, -20f),
                    new Vector2(Main.rand.NextFloatDirection() * 0.3f, -Main.rand.NextFloat(0.3f, 0.7f)),
                    Color.Lerp(FinalityUtils.AbyssalCrimson, FinalityUtils.SummonGlow, Main.rand.NextFloat()),
                    Main.rand.NextFloat(0.25f, 0.4f),
                    Main.rand.Next(35, 55)
                ));
            }

            // Dust trail
            if (Main.rand.NextBool(4))
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    ModContent.DustType<FinalityDust>(), 0f, 0f, 0,
                    FinalityUtils.AbyssalCrimson, Main.rand.NextFloat(0.5f, 1f));
                Main.dust[dust].noGravity = true;
            }
        }

        #endregion

        #region Hit Effects

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Spark burst from contact hit
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f + Main.rand.NextFloatDirection() * 0.2f;
                FinalityParticleHandler.SpawnParticle(new FateSpark(
                    target.Center,
                    angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f),
                    Color.Lerp(FinalityUtils.EmberGold, FinalityUtils.SakuraFlame, Main.rand.NextFloat()),
                    Main.rand.NextFloat(0.3f, 0.6f),
                    Main.rand.Next(8, 14)
                ));
            }

            // Small bloom on hit
            Color bloom = FinalityUtils.SakuraFlame;
            bloom.A = 0;
            FinalityParticleHandler.SpawnParticle(new DarkBloomParticle(
                target.Center, Vector2.Zero, bloom, 0.5f, 10
            ));
        }

        #endregion

        #region Rendering

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            float time = (float)Main.gameTimeCache.TotalGameTime.TotalSeconds;

            // Calculate 6x6 frame rect
            int frameW = tex.Width / FrameColumns;
            int frameH = tex.Height / FrameRows;
            int col = currentFrame % FrameColumns;
            int row = currentFrame / FrameColumns;
            Rectangle frameRect = new Rectangle(col * frameW, row * frameH, frameW, frameH);
            Vector2 origin = new Vector2(frameW / 2f, frameH / 2f);
            SpriteEffects flipEffect = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // Layer 1: GPU Dark flame aura trail
            DrawAuraTrail(sb);

            // Layer 2: Shader Dark Flame Aura
            DrawShaderDarkFlameAura(sb, time);

            // Layer 3: Shader Dark Funeral Trail Strip
            DrawShaderDarkFuneralStrip(sb, time);

            // Layer 4: Shader Afterimages
            DrawShaderAfterimages(sb, tex, frameRect, origin, flipEffect, time);

            // Layer 5: Core sprite
            DrawCore(sb, tex, frameRect, origin, flipEffect, lightColor);

            // Layer 6: Shader Summon Circle (subtle ambient)
            DrawShaderSummonCircle(sb, time);

            // Layer 7: Shader Bloom overlay
            DrawShaderBloomOverlay(sb, tex, frameRect, origin, flipEffect, time);

            // Eroica theme accent
            EroicaVFXLibrary.BeginEroicaAdditive(sb);
            EroicaVFXLibrary.DrawThemeSakuraAccent(sb, Projectile.Center, 1f, 0.4f);
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

        private void DrawAuraTrail(SpriteBatch sb)
        {
            if (Timer < 5) return;

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
                completionRatio => MathHelper.Lerp(14f, 2f, completionRatio),
                completionRatio =>
                {
                    float fade = (1f - completionRatio);
                    fade = fade * fade;
                    Color baseCol = Color.Lerp(FinalityUtils.AbyssalCrimson, FinalityUtils.FateViolet, completionRatio * 0.6f);
                    return baseCol * fade * 0.4f;
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

        private void DrawShaderDarkFlameAura(SpriteBatch sb, float time)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = 0.85f + 0.15f * (float)Math.Sin(Timer * 0.05f);
            float attackFlare = State == AIState.Attacking ? 1.2f : 1f;

            if (EroicaShaderManager.HasDarkFlameAura)
            {
                // PASS 1: Dark flame body  Einner crimson fire
                EroicaShaderManager.BeginShaderAdditive(sb);
                try
                {
                    EroicaShaderManager.ApplyFinalityDarkFlameAura(time, glowPass: false);

                    Texture2D softGlow = EroicaTextures.SoftGlow?.Value ?? SPFTextures.SoftGlow?.Value;
                    if (softGlow != null)
                    {
                        Vector2 glowOrigin = softGlow.Size() * 0.5f;

                        // Mid abyssal crimson ring
                        sb.Draw(softGlow, drawPos, null, Color.White * (0.22f * pulse * attackFlare),
                            0f, glowOrigin, 0.24f * Projectile.scale * attackFlare, SpriteEffects.None, 0f);

                        // Inner ember core
                        sb.Draw(softGlow, drawPos, null, Color.White * (0.15f * pulse),
                            0f, glowOrigin, 0.15f * Projectile.scale, SpriteEffects.None, 0f);
                    }
                }
                finally
                {
                    EroicaShaderManager.RestoreSpriteBatch(sb);
                }

                // PASS 2: Dark flame glow  Ewider violet haze
                EroicaShaderManager.BeginShaderAdditive(sb);
                try
                {
                    EroicaShaderManager.ApplyFinalityDarkFlameAura(time, glowPass: true);

                    Texture2D softGlow = EroicaTextures.SoftGlow?.Value ?? SPFTextures.SoftGlow?.Value;
                    if (softGlow != null)
                    {
                        Vector2 glowOrigin = softGlow.Size() * 0.5f;
                        sb.Draw(softGlow, drawPos, null, Color.White * (0.12f * pulse * attackFlare),
                            0f, glowOrigin, 0.24f * Projectile.scale * attackFlare, SpriteEffects.None, 0f);
                    }
                }
                finally
                {
                    EroicaShaderManager.RestoreSpriteBatch(sb);
                }
            }
            else
            {
                // Fallback: simple additive multi-scale bloom
                Texture2D softGlow = SPFTextures.SoftGlow.Value;
                if (softGlow == null) return;

                Vector2 glowOrigin = softGlow.Size() * 0.5f;

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    SamplerState.LinearClamp, DepthStencilState.None,
                    RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                try
                {
                    Color outerColor = FinalityUtils.FateViolet with { A = 0 };
                    sb.Draw(softGlow, drawPos, null, outerColor * (0.12f * pulse * attackFlare),
                        0f, glowOrigin, 0.24f * Projectile.scale * attackFlare, SpriteEffects.None, 0f);

                    Color midColor = FinalityUtils.AbyssalCrimson with { A = 0 };
                    sb.Draw(softGlow, drawPos, null, midColor * (0.18f * pulse * attackFlare),
                        0f, glowOrigin, 0.15f * Projectile.scale * attackFlare, SpriteEffects.None, 0f);

                    Color innerColor = FinalityUtils.EmberGold with { A = 0 };
                    sb.Draw(softGlow, drawPos, null, innerColor * (0.1f * pulse),
                        0f, glowOrigin, 0.15f * Projectile.scale, SpriteEffects.None, 0f);
                }
                finally
                {
                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                        Main.DefaultSamplerState, DepthStencilState.None,
                        RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }
        }

        private void DrawShaderDarkFuneralStrip(SpriteBatch sb, float time)
        {
            if (Timer < 5) return;

            int validCount = 0;
            for (int i = 0; i < TrailLength; i++)
            {
                if (trailPositions[i] != Vector2.Zero) validCount++;
                else break;
            }
            if (validCount < 3) return;

            Texture2D stripTex = EroicaTextures.EmberScatter?.Value ?? EroicaTextures.EnergyTrailUV?.Value;
            if (stripTex == null) return;

            int texW = stripTex.Width;
            int texH = stripTex.Height;
            float scrollTime = (float)Main.timeForVisualEffects * 0.005f;
            int srcWidth = Math.Max(1, texW / validCount);

            if (EroicaShaderManager.HasFuneralTrail)
            {
                // PASS 1: DarkFuneralTrail body  Edark crimson stream
                EroicaShaderManager.BeginShaderAdditive(sb);
                try
                {
                    EroicaShaderManager.ApplyFinalityDarkFuneralTrail(time, glowPass: false);

                    for (int i = 0; i < validCount - 1; i++)
                    {
                        float progress = (float)i / validCount;
                        float fade = (1f - progress);
                        fade = fade * fade;
                        if (fade < 0.01f) continue;

                        float width = MathHelper.Lerp(2f, 12f, 1f - progress);
                        Vector2 segDir = trailPositions[i] - trailPositions[i + 1];
                        float segLength = segDir.Length();
                        if (segLength < 0.5f) continue;
                        float segAngle = segDir.ToRotation();

                        float uStart = (progress + scrollTime * 2.5f) % 1f;
                        int srcX = (int)(uStart * texW) % texW;
                        Rectangle srcRect = new Rectangle(srcX, 0, srcWidth, texH);

                        float scaleX = segLength / (float)srcWidth;
                        float scaleY = width / (float)texH;
                        Vector2 pos = trailPositions[i] - Main.screenPosition;
                        Vector2 drawOrigin = new Vector2(0, texH / 2f);

                        sb.Draw(stripTex, pos, srcRect, Color.White * (fade * 0.45f), segAngle, drawOrigin,
                            new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
                    }
                }
                finally
                {
                    EroicaShaderManager.RestoreSpriteBatch(sb);
                }

                // PASS 2: DarkFuneralTrail glow  Ewider smolder haze
                EroicaShaderManager.BeginShaderAdditive(sb);
                try
                {
                    EroicaShaderManager.ApplyFinalityDarkFuneralTrail(time, glowPass: true);

                    for (int i = 0; i < validCount - 1; i++)
                    {
                        float progress = (float)i / validCount;
                        float fade = (1f - progress);
                        fade = fade * fade;
                        if (fade < 0.02f) continue;

                        float width = MathHelper.Lerp(2f, 12f, 1f - progress) * 1.6f;
                        Vector2 segDir = trailPositions[i] - trailPositions[i + 1];
                        float segLength = segDir.Length();
                        if (segLength < 0.5f) continue;
                        float segAngle = segDir.ToRotation();

                        float uStart = (progress + scrollTime * 2.5f) % 1f;
                        int srcX = (int)(uStart * texW) % texW;
                        Rectangle srcRect = new Rectangle(srcX, 0, srcWidth, texH);

                        float scaleX = segLength / (float)srcWidth;
                        float scaleY = width / (float)texH;
                        Vector2 pos = trailPositions[i] - Main.screenPosition;
                        Vector2 drawOrigin = new Vector2(0, texH / 2f);

                        sb.Draw(stripTex, pos, srcRect, Color.White * (fade * 0.18f), segAngle, drawOrigin,
                            new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
                    }
                }
                finally
                {
                    EroicaShaderManager.RestoreSpriteBatch(sb);
                }
            }
        }

        private void DrawShaderAfterimages(SpriteBatch sb, Texture2D tex, Rectangle frameRect, Vector2 origin, SpriteEffects flip, float time)
        {
            int imageCount = 6;

            if (EroicaShaderManager.HasDarkFlameAura)
            {
                EroicaShaderManager.BeginShaderAdditive(sb);
                try
                {
                    for (int i = imageCount - 1; i >= 0; i--)
                    {
                        float progress = (float)i / imageCount;
                        float trailIndex = progress * (TrailLength - 1);
                        int idx = (int)trailIndex;
                        float frac = trailIndex - idx;

                        if (idx + 1 >= TrailLength) continue;
                        if (trailPositions[idx] == Vector2.Zero || trailPositions[idx + 1] == Vector2.Zero) continue;

                        Vector2 pos = Vector2.Lerp(trailPositions[idx], trailPositions[idx + 1], frac) - Main.screenPosition;

                        float fadeFactor = (1f - progress);
                        fadeFactor *= fadeFactor;
                        float alpha = fadeFactor * 0.25f;
                        float scale = Projectile.scale * (1f - progress * 0.15f);

                        EroicaShaderManager.ApplyFinalityDarkFlameAura(time + i * 0.04f, glowPass: true);

                        sb.Draw(tex, pos, frameRect, Color.White * alpha, Projectile.rotation, origin, scale, flip, 0f);
                    }
                }
                finally
                {
                    EroicaShaderManager.RestoreSpriteBatch(sb);
                }
            }
            else
            {
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

                    float fadeFactor = (1f - progress);
                    fadeFactor *= fadeFactor;
                    Color drawColor = Color.Lerp(FinalityUtils.AbyssalCrimson, FinalityUtils.FateViolet, progress * 0.5f) * (fadeFactor * 0.25f);
                    drawColor.A = 0;

                    float scale = Projectile.scale * (1f - progress * 0.15f);
                    sb.Draw(tex, pos, frameRect, drawColor, Projectile.rotation, origin, scale, flip, 0f);
                }
                FinalityUtils.ExitShaderRegion(sb);
            }
        }

        private void DrawCore(SpriteBatch sb, Texture2D tex, Rectangle frameRect, Vector2 origin, SpriteEffects flip, Color lightColor)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Color coreTint = Color.Lerp(lightColor, FinalityUtils.SakuraFlame, 0.3f);

            sb.Draw(tex, drawPos, frameRect, coreTint, Projectile.rotation, origin, Projectile.scale, flip, 0f);

            Color coreGlow = FinalityUtils.EmberGold;
            coreGlow.A = 0;
            sb.Draw(tex, drawPos, frameRect, coreGlow * 0.25f, Projectile.rotation, origin,
                Projectile.scale * 0.95f, flip, 0f);
        }

        private void DrawShaderSummonCircle(SpriteBatch sb, float time)
        {
            if (!EroicaShaderManager.HasFateSummonCircle) return;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float ritualPhase = Math.Clamp(Timer / 30f, 0f, 1f);
            float pulse = 0.9f + 0.1f * (float)Math.Sin(Timer * 0.06f);

            // Summon circle body  Eslowly rotating sigil beneath the minion
            EroicaShaderManager.BeginShaderAdditive(sb);
            try
            {
                EroicaShaderManager.ApplyFinalitySummonCircle(time, ritualPhase, glowPass: false);

                Texture2D ringTex = EroicaTextures.HaloRing?.Value ?? EroicaTextures.SoftCircle?.Value;
                if (ringTex != null)
                {
                    float ringScale = Projectile.scale * 0.4f * pulse;
                    sb.Draw(ringTex, drawPos + new Vector2(0, 4f), null, Color.White * 0.2f,
                        time * 0.2f, ringTex.Size() * 0.5f, ringScale, SpriteEffects.None, 0f);
                }
            }
            finally
            {
                EroicaShaderManager.RestoreSpriteBatch(sb);
            }

            // Summon circle glow  Esubtle ambient glow
            EroicaShaderManager.BeginShaderAdditive(sb);
            try
            {
                EroicaShaderManager.ApplyFinalitySummonCircle(time, ritualPhase, glowPass: true);

                Texture2D bloomTex = EroicaTextures.BloomOrb?.Value ?? EroicaTextures.SoftGlow?.Value;
                if (bloomTex != null)
                {
                    float glowScale = Projectile.scale * 0.35f * pulse;
                    sb.Draw(bloomTex, drawPos + new Vector2(0, 4f), null, Color.White * 0.12f,
                        0f, bloomTex.Size() * 0.5f, glowScale, SpriteEffects.None, 0f);
                }
            }
            finally
            {
                EroicaShaderManager.RestoreSpriteBatch(sb);
            }
        }

        private void DrawShaderBloomOverlay(SpriteBatch sb, Texture2D tex, Rectangle frameRect, Vector2 origin, SpriteEffects flip, float time)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = (float)Math.Sin(Timer * 0.08f) * 0.08f;
            float bloomScale = Projectile.scale * 1.25f + pulse;

            if (EroicaShaderManager.HasDarkFlameAura)
            {
                EroicaShaderManager.BeginShaderAdditive(sb);
                try
                {
                    EroicaShaderManager.ApplyFinalityDarkFlameAura(time, glowPass: true);
                    sb.Draw(tex, drawPos, frameRect, Color.White * 0.3f, Projectile.rotation, origin,
                        bloomScale, flip, 0f);
                }
                finally
                {
                    EroicaShaderManager.RestoreSpriteBatch(sb);
                }
            }
            else
            {
                Color bloomColor = FinalityUtils.AbyssalCrimson;
                bloomColor.A = 0;
                FinalityUtils.EnterShaderRegion(sb);
                sb.Draw(tex, drawPos, frameRect, bloomColor * 0.3f, Projectile.rotation, origin, bloomScale, flip, 0f);
                FinalityUtils.ExitShaderRegion(sb);
            }
        }

        #endregion
    }
}
