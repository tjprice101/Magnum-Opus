using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace MagnumOpus.Content.MoonlightSonata.Minions
{
    /// <summary>
    /// Goliath of Moonlight — A massive lunar guardian minion.
    /// Currently a basic contact-damage minion awaiting VFX reimplementation.
    /// Uses a 6x6 spritesheet animation (36 frames).
    /// </summary>
    public class GoliathOfMoonlight : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/MoonlightSonata/Minions/GoliathOfMoonlight/GoliathOfMoonlight";

        public const int FrameColumns = 6;
        public const int FrameRows = 6;
        public const int TotalFrames = 36;
        public const int FrameTime = 4;

        private int frameCounter = 0;
        private int currentFrame = 0;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 1;
            Main.projPet[Type] = true;
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 18000;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.minionSlots = 1f;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => true;

        public override bool OnTileCollide(Vector2 oldVelocity) => false;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!CheckActive(owner))
                return;

            UpdateAnimation();

            // Apply gravity
            Projectile.velocity.Y += 0.35f;
            if (Projectile.velocity.Y > 16f)
                Projectile.velocity.Y = 16f;

            NPC target = FindTarget(owner);
            float distToPlayer = Vector2.Distance(Projectile.Center, owner.Center);

            if (distToPlayer > 1500f)
            {
                // Teleport if too far
                Projectile.Center = owner.Center + new Vector2(-40f * owner.direction, 0f);
                Projectile.velocity = Vector2.Zero;
            }
            else if (distToPlayer > 600f)
            {
                // Float toward player
                FloatTowardPlayer(owner);
            }
            else if (target != null)
            {
                // Chase target
                ChaseTarget(target);
            }
            else
            {
                // Idle near player
                IdleMovement(owner);
            }

            if (Math.Abs(Projectile.velocity.X) > 0.5f)
                Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;
        }

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

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<GoliathOfMoonlightBuff>());
                Projectile.Kill();
                return false;
            }

            if (owner.HasBuff(ModContent.BuffType<GoliathOfMoonlightBuff>()))
                Projectile.timeLeft = 2;

            return true;
        }

        private NPC FindTarget(Player owner)
        {
            float maxDistance = 700f;
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

        private void FloatTowardPlayer(Player owner)
        {
            Projectile.tileCollide = false;
            Vector2 targetPos = owner.Center + new Vector2(-40f * owner.direction, -60f);
            Vector2 direction = targetPos - Projectile.Center;
            float distance = direction.Length();

            if (distance > 20f)
            {
                direction.Normalize();
                direction *= Math.Min(distance / 8f, 12f);
                Projectile.velocity = (Projectile.velocity * 14f + direction) / 15f;
            }
            else
            {
                Projectile.velocity *= 0.9f;
            }

            if (distance < 100f)
                Projectile.tileCollide = true;
        }

        private void ChaseTarget(NPC target)
        {
            Projectile.tileCollide = true;
            Vector2 moveDir = target.Center - Projectile.Center;

            bool onGround = IsOnGround();
            if (onGround)
            {
                float targetX = Math.Sign(moveDir.X) * 6f;
                Projectile.velocity.X = MathHelper.Lerp(Projectile.velocity.X, targetX, 0.1f);

                if (target.Center.Y < Projectile.Center.Y - 80f && Math.Abs(moveDir.X) < 200f)
                    Projectile.velocity.Y = -10f;
            }
            else
            {
                Projectile.velocity.X += Math.Sign(moveDir.X) * 0.08f;
                Projectile.velocity.X = MathHelper.Clamp(Projectile.velocity.X, -8f, 8f);
            }
        }

        private void IdleMovement(Player owner)
        {
            Projectile.tileCollide = true;
            Vector2 direction = owner.Center + new Vector2(-60f * owner.direction, 0f) - Projectile.Center;

            bool onGround = IsOnGround();
            if (onGround)
            {
                if (Math.Abs(direction.X) > 40f)
                    Projectile.velocity.X = MathHelper.Lerp(Projectile.velocity.X, Math.Sign(direction.X) * 4f, 0.08f);
                else
                    Projectile.velocity.X *= 0.9f;

                if (owner.Center.Y < Projectile.Center.Y - 100f && Math.Abs(direction.X) < 150f)
                    Projectile.velocity.Y = -12f;
            }
            else
            {
                Projectile.velocity.X += Math.Sign(direction.X) * 0.05f;
                Projectile.velocity.X = MathHelper.Clamp(Projectile.velocity.X, -6f, 6f);
            }
        }

        private bool IsOnGround()
        {
            Vector2 checkPos = Projectile.BottomLeft;
            for (int x = 0; x < Projectile.width / 16 + 1; x++)
            {
                int tileX = (int)((checkPos.X + x * 16) / 16);
                int tileY = (int)((checkPos.Y + 4) / 16);
                Tile tile = Framing.GetTileSafely(tileX, tileY);
                if (tile.HasTile && Main.tileSolid[tile.TileType])
                    return true;
            }
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            int frameWidth = texture.Width / FrameColumns;
            int frameHeight = texture.Height / FrameRows;

            int col = currentFrame % FrameColumns;
            int row = currentFrame / FrameColumns;

            Rectangle sourceRect = new Rectangle(col * frameWidth, row * frameHeight, frameWidth, frameHeight);
            Vector2 origin = new Vector2(frameWidth / 2f, frameHeight);
            Vector2 drawPos = new Vector2(Projectile.Center.X, Projectile.position.Y + Projectile.height) - Main.screenPosition;

            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Main.EntitySpriteDraw(texture, drawPos, sourceRect, lightColor, 0f, origin, Projectile.scale, effects, 0);

            return false;
        }
    }
}
