using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using MagnumOpus.Common.Systems;

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

            // Sword arc trailing wave effect - elegant crescent
            if (Main.rand.NextBool(3))
            {
                CustomParticles.SwordArcWave(Projectile.Center, Projectile.velocity * 0.2f, 
                    CustomParticleSystem.MoonlightColors.Lavender * 0.7f, 0.3f);
            }
            
            // Prismatic sparkle accents along the wave
            if (Main.rand.NextBool(4))
            {
                Vector2 offset = Main.rand.NextVector2Circular(15f, 15f);
                CustomParticles.PrismaticSparkle(Projectile.Center + offset, CustomParticleSystem.MoonlightColors.Silver, 0.2f);
            }

            // Purple flame trail particles - reduced for cleaner look
            if (Main.rand.NextBool(3))
            {
                Dust flame = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    DustID.PurpleTorch, 0f, 0f, 100, default, 1.1f);
                flame.noGravity = true;
                flame.velocity = Projectile.velocity * 0.05f + Main.rand.NextVector2Circular(0.8f, 0.8f);
            }

            // Light emission
            Lighting.AddLight(Projectile.Center, 0.5f, 0.2f, 0.7f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Music's Dissonance debuff
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 300);

            // Prismatic sparkle impact - cleaner gem-like effect
            CustomParticles.PrismaticSparkleBurst(target.Center, CustomParticleSystem.MoonlightColors.Violet, 5);
            
            // Burst of particles on hit - reduced count
            for (int i = 0; i < 10; i++)
            {
                Dust dust = Dust.NewDustDirect(target.Center, 1, 1, DustID.PurpleTorch, 0f, 0f, 100, default, 1.2f);
                dust.noGravity = true;
                dust.velocity = Main.rand.NextVector2Circular(5f, 5f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = texture.Size() / 2f;
            
            // Switch to additive blending for glow effect
            MagnumVFX.BeginAdditiveBlend(spriteBatch);
            
            // Draw prismatic gem trail using oldPos
            MagnumVFX.DrawPrismaticGemTrail(spriteBatch, Projectile.oldPos, false, 0.4f, Projectile.timeLeft);
            
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
            
            // Draw main prismatic gem at projectile center
            float fadeAlpha = 1f - (Projectile.alpha / 255f);
            MagnumVFX.DrawMoonlightPrismaticGem(spriteBatch, Projectile.Center, 0.8f * Projectile.scale, fadeAlpha, Projectile.timeLeft);
            
            // Reset to normal blending
            MagnumVFX.EndAdditiveBlend(spriteBatch);
            
            // Draw main projectile texture
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
            // Magic sparkle field burst on death - ethereal dissipation
            CustomParticles.MagicSparkleFieldBurst(Projectile.Center, CustomParticleSystem.MoonlightColors.Lavender, 4, 25f);
            
            // Sword arc burst - fading slash marks
            CustomParticles.SwordArcBurst(Projectile.Center, CustomParticleSystem.MoonlightColors.Silver * 0.7f, 3, 0.3f);
            
            // Themed particle impact
            ThemedParticles.MoonlightImpact(Projectile.Center, 0.9f);
            
            // Final sparkle burst - reduced
            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    DustID.PurpleTorch, 0f, 0f, 100, default, 0.7f);
                dust.noGravity = true;
                dust.velocity = Main.rand.NextVector2Circular(2.5f, 2.5f);
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            float alpha = 1f - (Projectile.alpha / 255f);
            return new Color(180, 100, 255) * alpha;
        }
    }
}
