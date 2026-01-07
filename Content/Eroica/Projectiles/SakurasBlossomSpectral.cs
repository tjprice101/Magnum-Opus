using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// Spectral copy of Sakura's Blossom that seeks and hits enemies.
    /// Features dramatic scarlet/pink arc trail with white sparkles.
    /// </summary>
    public class SakurasBlossomSpectral : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/Eroica/ResonantWeapons/SakurasBlossom";

        private int targetNPC = -1;

        public override void SetStaticDefaults()
        {
            // Extended trail for dramatic arc effect
            ProjectileID.Sets.TrailCacheLength[Type] = 20;
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
            // Align rotation with velocity direction
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // White sparkle dust effect
            if (Main.rand.NextBool(2))
            {
                Dust sparkle = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(15f, 15f), 1, 1,
                    DustID.SparksMech, 0f, 0f, 0, Color.White, 1.3f);
                sparkle.noGravity = true;
                sparkle.velocity = Main.rand.NextVector2Circular(2f, 2f);
                sparkle.fadeIn = 1.2f;
            }

            // Scarlet/pink petal trail
            if (Main.rand.NextBool(2))
            {
                Dust trail = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.RedTorch, 0f, 0f, 100, default, 1.2f);
                trail.noGravity = true;
                trail.velocity *= 0.3f;
            }
            
            // Pink accent particles
            if (Main.rand.NextBool(3))
            {
                Dust pink = Dust.NewDustDirect(Projectile.Center, 1, 1,
                    DustID.PinkTorch, 0f, 0f, 100, default, 1.0f);
                pink.noGravity = true;
                pink.velocity = Main.rand.NextVector2Circular(2f, 2f);
            }

            // Find and home towards nearest enemy
            if (targetNPC < 0 || !Main.npc[targetNPC].active)
            {
                targetNPC = -1;
                float maxDistance = 800f;
                bool foundBoss = false;

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

            if (targetNPC >= 0 && Main.npc[targetNPC].active)
            {
                Vector2 direction = Main.npc[targetNPC].Center - Projectile.Center;
                direction.Normalize();
                Projectile.velocity = (Projectile.velocity * 20f + direction * 15f) / 21f;
            }

            Lighting.AddLight(Projectile.Center, 0.8f, 0.2f, 0.3f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Massive scarlet explosion with white sparkles
            for (int i = 0; i < 40; i++)
            {
                Dust explosion = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.RedTorch, 0f, 0f, 100, default, 2.5f);
                explosion.noGravity = true;
                explosion.velocity = Main.rand.NextVector2Circular(8f, 8f);
            }
            
            // White sparkle burst
            for (int i = 0; i < 15; i++)
            {
                Dust sparkle = Dust.NewDustDirect(target.Center, 1, 1, DustID.SparksMech, 0f, 0f, 0, Color.White, 1.8f);
                sparkle.noGravity = true;
                sparkle.velocity = Main.rand.NextVector2Circular(6f, 6f);
            }
            
            // Pink accents
            for (int i = 0; i < 20; i++)
            {
                Dust pink = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.PinkTorch, 0f, 0f, 100, default, 2.0f);
                pink.noGravity = true;
                pink.velocity = Main.rand.NextVector2Circular(7f, 7f);
            }

            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, Projectile.position);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 20; i++)
            {
                Dust explosion = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.RedTorch, 0f, 0f, 100, default, 1.5f);
                explosion.noGravity = true;
                explosion.velocity = Main.rand.NextVector2Circular(4f, 4f);
            }
            
            for (int i = 0; i < 8; i++)
            {
                Dust sparkle = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.SparksMech, 0f, 0f, 0, Color.White, 1.2f);
                sparkle.noGravity = true;
                sparkle.velocity = Main.rand.NextVector2Circular(3f, 3f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = texture.Size() / 2f;
            
            // Switch to additive blending for dramatic glow
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, 
                SamplerState.LinearClamp, DepthStencilState.None, 
                RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Draw sweeping arc trail - scarlet to pink gradient
            for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;

                float progress = (float)i / Projectile.oldPos.Length;
                float alpha = (1f - progress) * 0.7f;
                float trailScale = Projectile.scale * (1f - progress * 0.4f) * 0.9f;
                
                Vector2 drawPos = Projectile.oldPos[i] - Main.screenPosition + Projectile.Size / 2f;
                
                // Scarlet core trail
                Color scarletColor = new Color(220, 50, 70) * alpha;
                spriteBatch.Draw(texture, drawPos, null, scarletColor, 
                    Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
                
                // Pink outer glow
                Color pinkGlow = new Color(255, 120, 150) * alpha * 0.6f;
                spriteBatch.Draw(texture, drawPos, null, pinkGlow, 
                    Projectile.oldRot[i], origin, trailScale * 1.25f, SpriteEffects.None, 0f);
                
                // White sparkle highlights on recent positions
                if (i < 6)
                {
                    Color whiteGlow = Color.White * alpha * 0.5f;
                    spriteBatch.Draw(texture, drawPos, null, whiteGlow, 
                        Projectile.oldRot[i], origin, trailScale * 0.6f, SpriteEffects.None, 0f);
                }
            }
            
            // Reset to normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Draw main projectile with glow
            Vector2 mainPos = Projectile.Center - Main.screenPosition;
            Color trailColor = new Color(220, 80, 100, 100);
            spriteBatch.Draw(texture, mainPos, null, trailColor, Projectile.rotation, origin,
                Projectile.scale, SpriteEffects.None, 0f);

            return false;
        }
    }
}
