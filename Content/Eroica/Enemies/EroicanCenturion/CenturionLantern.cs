using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Enemies
{
    /// <summary>
    /// Centurion Lantern - Floating lanterns that orbit the Eroican Centurion.
    /// Uses FLOAT1 sprite sheet. Shoots gold/red sword projectiles.
    /// </summary>
    public class CenturionLantern : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/Eroica/Enemies/CenturionLantern";

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
        private const int FrameTime = 4;
        private const int FrameColumns = 6;
        private const int FrameRows = 6;
        private const int TotalFrames = 36;

        // Orbit parameters
        private const float OrbitRadiusX = 100f;
        private const float OrbitRadiusY = 60f;
        private const float OrbitSpeed = 0.03f;
        private const float BobSpeed = 0.05f;
        private const float BobAmount = 12f;

        private const float ShootCooldownMax = 50f;

        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.damage = 55;
        }

        public override void AI()
        {
            // Check if parent exists
            NPC parent = null;
            if (ParentNPCIndex >= 0 && ParentNPCIndex < Main.maxNPCs)
            {
                NPC potentialParent = Main.npc[ParentNPCIndex];
                if (potentialParent.active && potentialParent.type == ModContent.NPCType<EroicanCenturion>())
                {
                    parent = potentialParent;
                }
            }

            if (parent == null)
            {
                Projectile.Kill();
                return;
            }

            Projectile.timeLeft = 60;

            // Animation
            FrameCounter++;
            if (FrameCounter >= FrameTime)
            {
                FrameCounter = 0;
                currentFrame++;
                if (currentFrame >= TotalFrames)
                    currentFrame = 0;
            }

            // Update orbit
            OrbitAngle += OrbitSpeed;
            if (OrbitAngle > MathHelper.TwoPi)
                OrbitAngle -= MathHelper.TwoPi;

            // Calculate position
            float bobOffset = (float)Math.Sin(Main.GameUpdateCount * BobSpeed + OrbitAngle) * BobAmount;
            Vector2 orbitOffset = new Vector2(
                (float)Math.Cos(OrbitAngle) * OrbitRadiusX,
                (float)Math.Sin(OrbitAngle) * OrbitRadiusY * 0.5f + bobOffset - 50f
            );

            Vector2 targetPos = parent.Center + orbitOffset;

            // Smooth movement
            Vector2 direction = targetPos - Projectile.Center;
            float distance = direction.Length();

            if (distance > 2f)
            {
                float speed = Math.Min(distance * 0.1f, 8f);
                Projectile.velocity = direction.SafeNormalize(Vector2.Zero) * speed;
            }
            else
            {
                Projectile.velocity *= 0.5f;
            }

            // Rotation
            Projectile.rotation = Projectile.velocity.X * 0.02f;

            // Glowing light - gold and red
            float lightPulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f + OrbitAngle * 2f) * 0.2f + 0.8f;
            Lighting.AddLight(Projectile.Center, 0.7f * lightPulse, 0.4f * lightPulse, 0.1f * lightPulse);

            // Gold and red particles
            if (Main.rand.NextBool(8))
            {
                Dust glow = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame, 0f, 0f, 50, default, 0.9f);
                glow.noGravity = true;
                glow.velocity = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-1f, -0.3f));
            }

            if (Main.rand.NextBool(12))
            {
                Dust red = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, Color.DarkRed, 0.8f);
                red.noGravity = true;
                red.velocity *= 0.3f;
            }

            // Shooting
            ShootCooldown++;
            if (ShootCooldown >= ShootCooldownMax)
            {
                ShootCooldown = 0f;
                ShootAtPlayer();
            }
        }

        private void ShootAtPlayer()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            Player target = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
            if (target == null || !target.active || target.dead)
                return;

            float distanceToTarget = Vector2.Distance(Projectile.Center, target.Center);
            if (distanceToTarget > 600f)
                return;

            Vector2 shootDirection = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
            Vector2 velocity = shootDirection * 14f;

            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity,
                ModContent.ProjectileType<CenturionSwordProjectile>(), 60, 2f, Main.myPlayer);

            SoundEngine.PlaySound(SoundID.Item71, Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;

            int frameWidth = texture.Width / FrameColumns;
            int frameHeight = texture.Height / FrameRows;
            int frameX = currentFrame % FrameColumns;
            int frameY = currentFrame / FrameColumns;

            Rectangle sourceRect = new Rectangle(frameX * frameWidth, frameY * frameHeight, frameWidth, frameHeight);
            Vector2 origin = new Vector2(frameWidth / 2f, frameHeight / 2f);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float drawScale = 1.2f;

            // Gold/red glow
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.2f + 0.8f;
            Color glowColor = new Color(255, 150, 50, 0) * 0.4f * pulse;

            for (int i = 0; i < 4; i++)
            {
                Vector2 glowOffset = new Vector2(3f, 0f).RotatedBy(i * MathHelper.PiOver2);
                Main.EntitySpriteDraw(texture, drawPos + glowOffset, sourceRect, glowColor, Projectile.rotation,
                    origin, drawScale, SpriteEffects.None, 0);
            }

            // Main sprite
            Main.EntitySpriteDraw(texture, drawPos, sourceRect, lightColor, Projectile.rotation,
                origin, drawScale, SpriteEffects.None, 0);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 15; i++)
            {
                Dust death = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame, 0f, 0f, 50, default, 1.5f);
                death.noGravity = true;
                death.velocity = Main.rand.NextVector2Circular(5f, 5f);
            }

            for (int i = 0; i < 10; i++)
            {
                Dust red = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, Color.DarkRed, 1.2f);
                red.noGravity = true;
                red.velocity = Main.rand.NextVector2Circular(4f, 4f);
            }
        }
    }

    /// <summary>
    /// Sword projectile shot by Centurion Lanterns
    /// </summary>
    public class CenturionSwordProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/Eroica/Enemies/CenturionSwordProjectile";

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            // Rotation to face velocity (90° clockwise from original)
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.Pi;

            // Gold and red flame trail
            if (Main.rand.NextBool(2))
            {
                Dust gold = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame, 0f, 0f, 50, default, 1.3f);
                gold.noGravity = true;
                gold.velocity = -Projectile.velocity * 0.1f;
            }

            if (Main.rand.NextBool(2))
            {
                Dust red = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, Color.DarkRed, 1.2f);
                red.noGravity = true;
                red.velocity = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f);
            }

            Lighting.AddLight(Projectile.Center, 0.6f, 0.3f, 0.1f);
        }

        public override void OnKill(int timeLeft)
        {
            // Gold and red explosion
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.6f }, Projectile.Center);

            for (int i = 0; i < 20; i++)
            {
                Dust explode = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame, 0f, 0f, 50, default, 2f);
                explode.noGravity = true;
                explode.velocity = Main.rand.NextVector2Circular(8f, 8f);
            }

            for (int i = 0; i < 15; i++)
            {
                Dust red = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, Color.DarkRed, 2f);
                red.noGravity = true;
                red.velocity = Main.rand.NextVector2Circular(7f, 7f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // Glowing effect
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.15f + 0.85f;
            Color glowColor = new Color(255, 180, 80, 0) * 0.5f * pulse;

            for (int i = 0; i < 4; i++)
            {
                Vector2 glowOffset = new Vector2(3f, 0f).RotatedBy(i * MathHelper.PiOver2);
                Main.EntitySpriteDraw(texture, drawPos + glowOffset, null, glowColor, Projectile.rotation,
                    origin, Projectile.scale, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(texture, drawPos, null, lightColor, Projectile.rotation,
                origin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }
    }
}
