using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;
using MagnumOpus.Content.Eroica.Weapons.FinalityOfTheSakura.Utilities;
using MagnumOpus.Content.Eroica.Weapons.FinalityOfTheSakura.Particles;
using MagnumOpus.Content.Eroica.Weapons.FinalityOfTheSakura.Primitives;
using MagnumOpus.Content.Eroica.Weapons.FinalityOfTheSakura.Dusts;

namespace MagnumOpus.Content.Eroica.Minions
{
    /// <summary>
    /// Sakura of Fate — spectral dark-flame guardian minion.
    /// Self-contained VFX: 6×6 spritesheet rendering, dark flame aura, ambient particles.
    /// </summary>
    public class SakuraOfFate : ModProjectile
    {
        // ── Spritesheet configuration — 6×6 grid ──
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

        // ── Trail tracking for aura effect ──
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

            // ── Ambient VFX ──
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
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;

            // Calculate 6×6 frame rect
            int frameW = tex.Width / FrameColumns;
            int frameH = tex.Height / FrameRows;
            int col = currentFrame % FrameColumns;
            int row = currentFrame / FrameColumns;
            Rectangle frameRect = new Rectangle(col * frameW, row * frameH, frameW, frameH);
            Vector2 origin = new Vector2(frameW / 2f, frameH / 2f);
            SpriteEffects flipEffect = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // ── Layer 1: Dark flame aura trail ──
            DrawAuraTrail(sb);

            // ── Layer 2: Afterimages ──
            DrawAfterimages(sb, tex, frameRect, origin, flipEffect);

            // ── Layer 3: Core sprite ──
            DrawCore(sb, tex, frameRect, origin, flipEffect, lightColor);

            // ── Layer 4: Additive bloom overlay ──
            DrawBloomOverlay(sb, tex, frameRect, origin, flipEffect);

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

        private void DrawAfterimages(SpriteBatch sb, Texture2D tex, Rectangle frameRect, Vector2 origin, SpriteEffects flip)
        {
            int imageCount = 6;
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

        private void DrawCore(SpriteBatch sb, Texture2D tex, Rectangle frameRect, Vector2 origin, SpriteEffects flip, Color lightColor)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Color coreTint = Color.Lerp(lightColor, FinalityUtils.SakuraFlame, 0.3f);

            // Base sprite
            sb.Draw(tex, drawPos, frameRect, coreTint, Projectile.rotation, origin, Projectile.scale, flip, 0f);

            // Inner warm glow
            Color coreGlow = FinalityUtils.EmberGold;
            coreGlow.A = 0;
            sb.Draw(tex, drawPos, frameRect, coreGlow * 0.25f, Projectile.rotation, origin,
                Projectile.scale * 0.95f, flip, 0f);
        }

        private void DrawBloomOverlay(SpriteBatch sb, Texture2D tex, Rectangle frameRect, Vector2 origin, SpriteEffects flip)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Color bloomColor = FinalityUtils.AbyssalCrimson;
            bloomColor.A = 0;
            float pulse = (float)Math.Sin(Timer * 0.08f) * 0.08f;
            float bloomScale = Projectile.scale * 1.25f + pulse;

            FinalityUtils.EnterShaderRegion(sb);
            sb.Draw(tex, drawPos, frameRect, bloomColor * 0.3f, Projectile.rotation, origin, bloomScale, flip, 0f);
            FinalityUtils.ExitShaderRegion(sb);
        }

        #endregion
    }
}
