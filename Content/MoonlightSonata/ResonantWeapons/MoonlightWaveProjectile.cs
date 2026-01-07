using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.MoonlightSonata.Debuffs;

namespace MagnumOpus.Content.MoonlightSonata.ResonantWeapons
{
    public class MoonlightWaveProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
            // Enable trail storage for arc effect
            ProjectileID.Sets.TrailCacheLength[Type] = 25;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 5; // Can hit 5 enemies
            Projectile.timeLeft = 60; // Lasts 1 second
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.alpha = 50;
            Projectile.scale = 1f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            // Grow slightly over time for wave effect
            Projectile.scale += 0.02f;
            Projectile.alpha += 3;

            if (Projectile.alpha > 255)
            {
                Projectile.Kill();
                return;
            }

            // Set rotation to match velocity direction
            Projectile.rotation = Projectile.velocity.ToRotation();

            // White sparkle dust effect
            if (Main.rand.NextBool(2))
            {
                Dust sparkle = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f), 1, 1, 
                    DustID.SparksMech, 0f, 0f, 0, Color.White, 1.2f);
                sparkle.noGravity = true;
                sparkle.velocity = Main.rand.NextVector2Circular(2f, 2f);
                sparkle.fadeIn = 1.3f;
            }

            // Purple flame trail particles
            if (Main.rand.NextBool(2))
            {
                Dust flame = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    DustID.PurpleTorch, 0f, 0f, 100, default, 1.4f);
                flame.noGravity = true;
                flame.velocity = Projectile.velocity * 0.05f + Main.rand.NextVector2Circular(1f, 1f);
            }

            // Light emission
            Lighting.AddLight(Projectile.Center, 0.6f, 0.2f, 0.8f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Music's Dissonance debuff
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 300);

            // Burst of particles on hit
            for (int i = 0; i < 15; i++)
            {
                Dust dust = Dust.NewDustDirect(target.Center, 1, 1, DustID.PurpleTorch, 0f, 0f, 100, default, 1.5f);
                dust.noGravity = true;
                dust.velocity = Main.rand.NextVector2Circular(6f, 6f);
            }
            
            // White sparkle burst
            for (int i = 0; i < 8; i++)
            {
                Dust sparkle = Dust.NewDustDirect(target.Center, 1, 1, DustID.SparksMech, 0f, 0f, 0, Color.White, 1.5f);
                sparkle.noGravity = true;
                sparkle.velocity = Main.rand.NextVector2Circular(5f, 5f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = texture.Size() / 2f;
            
            // Switch to additive blending for glow effect
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, 
                SamplerState.LinearClamp, DepthStencilState.None, 
                RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Draw sweeping arc trail
            for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero || Projectile.oldPos[i + 1] == Vector2.Zero) 
                    continue;
                
                float progress = (float)i / Projectile.oldPos.Length;
                float alpha = (1f - progress) * 0.8f;
                float trailScale = Projectile.scale * (1f - progress * 0.5f);
                
                Vector2 drawPos = Projectile.oldPos[i] - Main.screenPosition + Projectile.Size / 2f;
                
                // Purple core trail
                Color trailColor = new Color(180, 80, 255) * alpha;
                spriteBatch.Draw(texture, drawPos, null, trailColor, 
                    Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
                
                // Light blue outer glow
                Color glowColor = new Color(150, 200, 255) * alpha * 0.5f;
                spriteBatch.Draw(texture, drawPos, null, glowColor, 
                    Projectile.oldRot[i], origin, trailScale * 1.3f, SpriteEffects.None, 0f);
                
                // White highlight on recent trail
                if (i < 5)
                {
                    Color whiteGlow = Color.White * alpha * 0.4f;
                    spriteBatch.Draw(texture, drawPos, null, whiteGlow, 
                        Projectile.oldRot[i], origin, trailScale * 0.7f, SpriteEffects.None, 0f);
                }
            }
            
            // Reset to normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Draw main projectile
            float fadeAlpha = 1f - (Projectile.alpha / 255f);
            Color drawColor = new Color(180, 100, 255) * fadeAlpha;
            Vector2 mainPos = Projectile.Center - Main.screenPosition;
            
            // Glow layers
            for (int i = 0; i < 3; i++)
            {
                float layerScale = Projectile.scale * (1f + i * 0.15f);
                float layerAlpha = fadeAlpha * (1f - i * 0.25f);
                Color layerColor = drawColor * layerAlpha;
                Main.EntitySpriteDraw(texture, mainPos, null, layerColor, 
                    Projectile.rotation, origin, layerScale, SpriteEffects.None, 0);
            }

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // Final sparkle burst
            for (int i = 0; i < 12; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    DustID.PurpleTorch, 0f, 0f, 100, default, 0.8f);
                dust.noGravity = true;
                dust.velocity = Main.rand.NextVector2Circular(3f, 3f);
            }
            
            for (int i = 0; i < 6; i++)
            {
                Dust sparkle = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.SparksMech, 0f, 0f, 0, Color.White, 1.0f);
                sparkle.noGravity = true;
                sparkle.velocity = Main.rand.NextVector2Circular(4f, 4f);
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            float alpha = 1f - (Projectile.alpha / 255f);
            return new Color(180, 100, 255) * alpha;
        }
    }
}
