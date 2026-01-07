using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// Bullet projectile for Blossom of the Sakura with explosion on impact.
    /// </summary>
    public class BlossomOfTheSakuraBulletProjectile : ModProjectile
    {
        private int targetNPC = -1;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 2;
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
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Find and home towards nearest enemy (prioritize bosses)
            if (targetNPC < 0 || !Main.npc[targetNPC].active)
            {
                targetNPC = -1;
                float maxDistance = 800f;
                bool foundBoss = false;

                // First pass: look for bosses
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.boss)
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

                // Second pass: if no boss, target any enemy
                if (!foundBoss)
                {
                    maxDistance = 600f;
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (npc.active && !npc.friendly && npc.lifeMax > 5)
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

            // Home towards target with moderate tracking
            if (targetNPC >= 0 && Main.npc[targetNPC].active)
            {
                Vector2 direction = Main.npc[targetNPC].Center - Projectile.Center;
                direction.Normalize();
                // Gentle homing so bullets don't make sharp turns
                Projectile.velocity = (Projectile.velocity * 30f + direction * 12f) / 31f;
            }

            // Red and black fire trail
            Dust flame = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                DustID.RedTorch, 0f, 0f, 100, default, 1.2f);
            flame.noGravity = true;
            flame.velocity *= 0.3f;

            if (Main.rand.NextBool(3))
            {
                Dust smoke = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Smoke, 0f, 0f, 100, Color.Black, 0.8f);
                smoke.noGravity = true;
                smoke.velocity *= 0.2f;
            }

            Lighting.AddLight(Projectile.Center, 0.6f, 0.1f, 0.1f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            CreateExplosion();
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            CreateExplosion();
            return true;
        }

        public override void OnKill(int timeLeft)
        {
            CreateExplosion();
        }

        private void CreateExplosion()
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);

            // Scarlet red explosion
            for (int i = 0; i < 25; i++)
            {
                Dust explosion = Dust.NewDustDirect(Projectile.position - new Vector2(15, 15),
                    Projectile.width + 30, Projectile.height + 30,
                    DustID.RedTorch, 0f, 0f, 100, default, 2.0f);
                explosion.noGravity = true;
                explosion.velocity = Main.rand.NextVector2Circular(6f, 6f);
            }

            // Black smoke explosion
            for (int i = 0; i < 15; i++)
            {
                Dust smoke = Dust.NewDustDirect(Projectile.position - new Vector2(10, 10),
                    Projectile.width + 20, Projectile.height + 20,
                    DustID.Smoke, 0f, 0f, 100, Color.Black, 1.5f);
                smoke.noGravity = true;
                smoke.velocity = Main.rand.NextVector2Circular(4f, 4f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = texture.Size() / 2f;

            // Draw trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;

                Vector2 drawPos = Projectile.oldPos[i] - Main.screenPosition + Projectile.Size / 2f;
                Color trailColor = new Color(200, 50, 50, 0) * ((float)(Projectile.oldPos.Length - i) / Projectile.oldPos.Length);

                spriteBatch.Draw(texture, drawPos, null, trailColor, Projectile.oldRot[i], origin,
                    Projectile.scale, SpriteEffects.None, 0f);
            }

            return true;
        }
    }
}
