using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Eroica.Enemies
{
    /// <summary>
    /// Stolen Valor Minion - Orbiting projectiles that follow StolenValor.
    /// Uses 6x6 sprite sheet animation. Faster and more aggressive than Moonlight minions.
    /// </summary>
    public class StolenValorMinion : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/Eroica/Enemies/StolenValorMinion";

        private int ParentNPCIndex => (int)Projectile.ai[0];
        private float OrbitAngle
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        private ref float ShootCooldown => ref Projectile.localAI[0];
        private ref float FrameCounter => ref Projectile.localAI[1];

        // Animation - 6x6 sprite sheet (36 frames)
        private int currentFrame = 0;
        private const int FrameTime = 3; // Ticks per frame (fast animation)
        private const int FrameColumns = 6;
        private const int FrameRows = 6;
        private const int TotalFrames = 36;

        // Orbit parameters - FASTER than Moonlight minions
        private const float OrbitRadiusX = 120f;
        private const float OrbitRadiusY = 70f;
        private const float OrbitSpeed = 0.035f; // Faster orbit than Moonlight (0.02f)
        private const float BobSpeed = 0.04f;
        private const float BobAmount = 15f;

        // More aggressive shooting - faster cooldown
        private const float ShootCooldownMax = 35f; // Shoots every ~0.58 seconds (very aggressive)

        public override void SetDefaults()
        {
            Projectile.width = 29;
            Projectile.height = 29;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.light = 0f;
            Projectile.damage = 80; // Higher damage than Moonlight minions
            Projectile.scale = 0.9f; // 10% size reduction
        }

        public override void AI()
        {
            // Check if parent Stolen Valor still exists
            NPC parent = null;
            if (ParentNPCIndex >= 0 && ParentNPCIndex < Main.maxNPCs)
            {
                NPC potentialParent = Main.npc[ParentNPCIndex];
                if (potentialParent.active && potentialParent.type == ModContent.NPCType<StolenValor>())
                {
                    parent = potentialParent;
                }
            }

            // Despawn if parent is gone
            if (parent == null)
            {
                Projectile.Kill();
                return;
            }

            // Stay alive while parent exists
            Projectile.timeLeft = 60;

            // Animation update
            FrameCounter++;
            if (FrameCounter >= FrameTime)
            {
                FrameCounter = 0;
                currentFrame++;
                if (currentFrame >= TotalFrames)
                    currentFrame = 0;
            }

            // Update orbit angle - FASTER than Moonlight enemies
            OrbitAngle += OrbitSpeed;
            if (OrbitAngle > MathHelper.TwoPi)
                OrbitAngle -= MathHelper.TwoPi;

            // Calculate target position (elliptical orbit around parent)
            float bobOffset = (float)Math.Sin(Main.GameUpdateCount * BobSpeed + OrbitAngle) * BobAmount;
            Vector2 orbitOffset = new Vector2(
                (float)Math.Cos(OrbitAngle) * OrbitRadiusX,
                (float)Math.Sin(OrbitAngle) * OrbitRadiusY * 0.5f + bobOffset - 40f // Float above parent
            );

            Vector2 targetPos = parent.Center + orbitOffset;

            // Smooth movement to target position - FASTER than Moonlight minions
            Vector2 direction = targetPos - Projectile.Center;
            float distance = direction.Length();

            if (distance > 2f)
            {
                // Fast, aggressive following
                float speed = Math.Min(distance * 0.12f, 10f); // Faster than Moonlight (0.08f, 7f)
                Projectile.velocity = direction.SafeNormalize(Vector2.Zero) * speed;
            }
            else
            {
                Projectile.velocity *= 0.5f;
            }

            // Rotation based on movement
            Projectile.rotation = Projectile.velocity.X * 0.02f;

            // Dark red ambient lighting
            float lightPulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f + OrbitAngle * 2f) * 0.15f + 0.85f;
            Lighting.AddLight(Projectile.Center, 0.6f * lightPulse, 0.15f * lightPulse, 0.1f * lightPulse);

            // Dark red/black particles
            if (Main.rand.NextBool(10))
            {
                Dust glow = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 150, Color.DarkRed, 0.9f);
                glow.noGravity = true;
                glow.velocity = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-1f, -0.3f));
            }

            // Gold sparkle particles
            if (Main.rand.NextBool(15))
            {
                Dust sparkle = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(12f, 12f), 1, 1, DustID.GoldFlame, 0f, 0f, 0, default, 0.7f);
                sparkle.noGravity = true;
                sparkle.velocity = new Vector2(0f, Main.rand.NextFloat(-0.5f, 0f));
            }

            // Black smoke particles
            if (Main.rand.NextBool(25))
            {
                Dust smoke = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.Smoke, 0f, 0f, 100, Color.Black, 0.6f);
                smoke.noGravity = true;
                smoke.velocity = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-0.5f, -0.2f));
            }

            // Aggressive shooting at player
            ShootCooldown++;
            if (ShootCooldown >= ShootCooldownMax)
            {
                ShootCooldown = 0f;
                ShootAtPlayer();
            }
        }

        private void ShootAtPlayer()
        {
            // Only shoot on server/single player
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            // Find closest player
            Player target = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
            if (target == null || !target.active || target.dead)
                return;

            float distanceToTarget = Vector2.Distance(Projectile.Center, target.Center);
            if (distanceToTarget > 700f)
                return;

            Vector2 shootDirection = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
            Vector2 velocity = shootDirection * 12f; // Fast projectiles

            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity,
                ModContent.ProjectileType<StolenValorFlame>(), 65, 2f, Main.myPlayer);

            SoundEngine.PlaySound(SoundID.Item34, Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;

            // Calculate frame dimensions from 6x6 sprite sheet
            int frameWidth = texture.Width / FrameColumns;
            int frameHeight = texture.Height / FrameRows;
            int frameX = currentFrame % FrameColumns;
            int frameY = currentFrame / FrameColumns;

            Rectangle sourceRect = new Rectangle(frameX * frameWidth, frameY * frameHeight, frameWidth, frameHeight);
            Vector2 origin = new Vector2(frameWidth / 2f, frameHeight / 2f);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // Pulsing glow effect - dark red/gold
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f + OrbitAngle * 2f) * 0.3f + 0.7f;

            // Draw glow layers (behind) - dark red and gold
            Color redGlow = new Color(140, 30, 20) * pulse * 0.5f;
            Color goldGlow = new Color(180, 140, 40) * pulse * 0.3f;

            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(4f, 0f).RotatedBy(MathHelper.TwoPi * i / 4);
                Color glowColor = i % 2 == 0 ? redGlow : goldGlow;
                Main.EntitySpriteDraw(texture, drawPos + offset, sourceRect, glowColor, Projectile.rotation, origin, Projectile.scale * 1.05f, SpriteEffects.None, 0);
            }

            // Outer soft glow
            Color outerGlow = new Color(100, 20, 10) * pulse * 0.25f;
            for (int i = 0; i < 6; i++)
            {
                Vector2 offset = new Vector2(8f, 0f).RotatedBy(MathHelper.TwoPi * i / 6);
                Main.EntitySpriteDraw(texture, drawPos + offset, sourceRect, outerGlow, Projectile.rotation, origin, Projectile.scale * 1.1f, SpriteEffects.None, 0);
            }

            // Draw main sprite
            Color drawColor = new Color(255, 255, 255, 220);
            Main.EntitySpriteDraw(texture, drawPos, sourceRect, drawColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 255, 255, 200);
        }

        public override void OnKill(int timeLeft)
        {
            // Enemy minion death - smaller golden glow
            DynamicParticleEffects.EroicaDeathGoldenGlow(Projectile.Center, 0.5f);
        }
    }
}
