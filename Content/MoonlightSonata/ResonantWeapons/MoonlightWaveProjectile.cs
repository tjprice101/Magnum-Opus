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
        // Use invisible texture - the projectile is rendered entirely through particle effects
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.None;
        
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
            
            // === SIGNATURE FRACTAL FLARE BURST ===
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 flareOffset = angle.ToRotationVector2() * 30f;
                float progress = (float)i / 6f;
                Color fractalColor = Color.Lerp(new Color(75, 0, 130), new Color(135, 206, 250), progress);
                CustomParticles.GenericFlare(target.Center + flareOffset, fractalColor, 0.45f, 18);
            }
            
            // Music notes on hit
            ThemedParticles.MoonlightMusicNotes(target.Center, 3, 25f);
            
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
            
            // Switch to additive blending for glow effect
            MagnumVFX.BeginAdditiveBlend(spriteBatch);
            
            // Draw glowing trail using soft glow texture instead of projectile texture
            Texture2D glowTex = Terraria.GameContent.TextureAssets.Extra[98].Value;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            
            // Draw sweeping arc trail with glowing orbs
            for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) 
                    continue;
                
                float progress = (float)i / Projectile.oldPos.Length;
                float alpha = (1f - progress) * 0.8f;
                float trailScale = (1f - progress * 0.5f) * 0.6f;
                
                Vector2 drawPos = Projectile.oldPos[i] - Main.screenPosition + Projectile.Size / 2f;
                
                // Purple core trail orb
                Color trailColor = new Color(180, 80, 255) * alpha;
                spriteBatch.Draw(glowTex, drawPos, null, trailColor, 
                    0f, glowOrigin, trailScale, SpriteEffects.None, 0f);
                
                // Light blue outer glow
                Color glowColor = new Color(150, 200, 255) * alpha * 0.5f;
                spriteBatch.Draw(glowTex, drawPos, null, glowColor, 
                    0f, glowOrigin, trailScale * 1.5f, SpriteEffects.None, 0f);
                
                // White highlight on recent trail
                if (i < 5)
                {
                    Color whiteGlow = Color.White * alpha * 0.4f;
                    spriteBatch.Draw(glowTex, drawPos, null, whiteGlow, 
                        0f, glowOrigin, trailScale * 0.5f, SpriteEffects.None, 0f);
                }
            }
            
            // Draw main glow at projectile center (replaces the gem)
            float fadeAlpha = 1f - (Projectile.alpha / 255f);
            Vector2 mainPos = Projectile.Center - Main.screenPosition;
            
            // Multi-layer glow at center
            spriteBatch.Draw(glowTex, mainPos, null, new Color(75, 0, 130) * fadeAlpha * 0.5f, 
                0f, glowOrigin, Projectile.scale * 1.2f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, mainPos, null, new Color(180, 100, 255) * fadeAlpha * 0.7f, 
                0f, glowOrigin, Projectile.scale * 0.8f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, mainPos, null, Color.White * fadeAlpha * 0.5f, 
                0f, glowOrigin, Projectile.scale * 0.4f, SpriteEffects.None, 0f);
            
            // Reset to normal blending
            MagnumVFX.EndAdditiveBlend(spriteBatch);

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
            
            // === SIGNATURE FRACTAL FLARE BURST ===
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 flareOffset = angle.ToRotationVector2() * 30f;
                float progress = (float)i / 6f;
                Color fractalColor = Color.Lerp(new Color(75, 0, 130), new Color(135, 206, 250), progress);
                CustomParticles.GenericFlare(Projectile.Center + flareOffset, fractalColor, 0.45f, 18);
            }
            
            // Music notes on death
            ThemedParticles.MoonlightMusicNotes(Projectile.Center, 4, 30f);
            
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
