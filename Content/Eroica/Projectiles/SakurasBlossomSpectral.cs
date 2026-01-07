using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// Spectral copy of Sakura's Blossom that seeks and hits enemies.
    /// </summary>
    public class SakurasBlossomSpectral : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/Eroica/ResonantWeapons/SakurasBlossom";

        private int targetNPC = -1;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.alpha = 100;
            Projectile.light = 0.6f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            // Align rotation with velocity direction (no spinning)
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Scarlet red and black trail
            if (Main.rand.NextBool(2))
            {
                Dust trail = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.RedTorch, 0f, 0f, 100, default, 1.2f);
                trail.noGravity = true;
                trail.velocity *= 0.3f;
            }

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

            // Home towards target
            if (targetNPC >= 0 && Main.npc[targetNPC].active)
            {
                Vector2 direction = Main.npc[targetNPC].Center - Projectile.Center;
                direction.Normalize();
                Projectile.velocity = (Projectile.velocity * 20f + direction * 15f) / 21f;
            }

            Lighting.AddLight(Projectile.Center, 0.7f, 0.1f, 0.1f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Massive scarlet and black explosion
            for (int i = 0; i < 40; i++)
            {
                Dust explosion = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.RedTorch, 0f, 0f, 100, default, 2.5f);
                explosion.noGravity = true;
                explosion.velocity = Main.rand.NextVector2Circular(8f, 8f);
            }

            for (int i = 0; i < 25; i++)
            {
                Dust smoke = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Smoke, 0f, 0f, 100, Color.Black, 2.0f);
                smoke.noGravity = true;
                smoke.velocity = Main.rand.NextVector2Circular(6f, 6f);
            }

            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, Projectile.position);
        }

        public override void OnKill(int timeLeft)
        {
            // Smaller explosion on timeout
            for (int i = 0; i < 20; i++)
            {
                Dust explosion = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.RedTorch, 0f, 0f, 100, default, 1.5f);
                explosion.noGravity = true;
                explosion.velocity = Main.rand.NextVector2Circular(4f, 4f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = texture.Size() / 2f;

            // Draw ghostly trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;

                Vector2 drawPos = Projectile.oldPos[i] - Main.screenPosition + Projectile.Size / 2f;
                Color trailColor = new Color(200, 50, 50, 0) * ((float)(Projectile.oldPos.Length - i) / Projectile.oldPos.Length) * 0.5f;

                spriteBatch.Draw(texture, drawPos, null, trailColor, Projectile.oldRot[i], origin,
                    Projectile.scale * 0.8f, SpriteEffects.None, 0f);
            }

            return true;
        }
    }
}
