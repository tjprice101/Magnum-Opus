using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// Hand of Valor projectile thrown by Eroica boss in Phase 2.
    /// Sprite points down by default, orients to direction of travel.
    /// Thrown in sets of 3 in a 120 degree cone.
    /// </summary>
    public class HandOfValor : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.light = 0.5f;
            Projectile.scale = 0.3f; // 70% size reduction
        }

        public override void AI()
        {
            // Orient to direction of travel
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
            
            float pulse = (float)Math.Sin(Projectile.timeLeft * 0.12f) * 0.15f + 1f;
            
            // === Core flare every frame ===
            CustomParticles.GenericFlare(Projectile.Center, UnifiedVFX.Eroica.Gold * pulse, 0.35f, 5);
            
            // === Dense gradient glow particle trail ===
            for (int i = 0; i < 2; i++)
            {
                float gradientProgress = Main.rand.NextFloat();
                Color particleColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, gradientProgress);
                Vector2 randomOffset = Main.rand.NextVector2Circular(8f, 8f);
                Vector2 vel = -Projectile.velocity * 0.12f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                
                var glow = new GenericGlowParticle(Projectile.Center + randomOffset, vel, particleColor, 
                    0.25f + Main.rand.NextFloat(0.1f), 15, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // === Orbiting flares - 3-point formation ===
            if (Projectile.timeLeft % 6 == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    float orbitAngle = MathHelper.TwoPi * i / 3f + Projectile.timeLeft * 0.15f;
                    float orbitRadius = 15f + pulse * 4f;
                    Vector2 orbitPos = Projectile.Center + orbitAngle.ToRotationVector2() * orbitRadius;
                    Color orbitColor = Color.Lerp(UnifiedVFX.Eroica.Crimson, UnifiedVFX.Eroica.Gold, (float)i / 3f);
                    CustomParticles.GenericFlare(orbitPos, orbitColor, 0.22f, 8);
                }
            }
            
            // === Sakura petals occasionally ===
            if (Main.rand.NextBool(5))
            {
                ThemedParticles.SakuraPetals(Projectile.Center, 1, 18f);
            }
            
            // === Music notes in trail ===
            if (Main.rand.NextBool(8))
            {
                float noteProgress = Main.rand.NextFloat();
                Color noteColor = Color.Lerp(UnifiedVFX.Eroica.Gold, UnifiedVFX.Eroica.Sakura, noteProgress);
                Vector2 noteVel = -Projectile.velocity * 0.06f + Main.rand.NextVector2Circular(1f, 1f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.25f, 20);
            }
            
            // === Dynamic lighting with Eroica colors ===
            float lightIntensity = 0.8f + pulse * 0.2f;
            Lighting.AddLight(Projectile.Center, UnifiedVFX.Eroica.Gold.ToVector3() * lightIntensity);
        }

        public override void OnKill(int timeLeft)
        {
            // === PHASE 1: Central flash ===
            CustomParticles.GenericFlare(Projectile.Center, Color.White, 0.9f, 15);
            CustomParticles.GenericFlare(Projectile.Center, UnifiedVFX.Eroica.Gold, 0.7f, 18);
            
            // === PHASE 2: UnifiedVFX themed impact ===
            UnifiedVFX.Eroica.Impact(Projectile.Center, 1.0f);
            
            // === PHASE 3: 6-point fractal burst ===
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                float gradientProgress = (float)i / 6f;
                Vector2 flareOffset = angle.ToRotationVector2() * 30f;
                Color fractalColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, gradientProgress);
                CustomParticles.GenericFlare(Projectile.Center + flareOffset, fractalColor, 0.4f, 15);
            }
            
            // === PHASE 4: Cascading halo rings ===
            for (int ring = 0; ring < 3; ring++)
            {
                float ringProgress = ring / 3f;
                Color ringColor = Color.Lerp(UnifiedVFX.Eroica.Gold, UnifiedVFX.Eroica.Crimson, ringProgress);
                CustomParticles.HaloRing(Projectile.Center, ringColor * (1f - ringProgress * 0.2f), 
                    0.3f + ring * 0.15f, 12 + ring * 4);
            }
            
            // === PHASE 5: Spark spray ===
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 9f);
                float gradientProgress = Main.rand.NextFloat();
                Color sparkColor = Color.Lerp(UnifiedVFX.Eroica.Flame, UnifiedVFX.Eroica.Gold, gradientProgress);
                
                var spark = new GenericGlowParticle(Projectile.Center, sparkVel, sparkColor, 0.28f, 18, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // === PHASE 6: Sakura petal burst ===
            ThemedParticles.SakuraPetals(Projectile.Center, 4, 35f);
            
            // === PHASE 7: Music note finale ===
            for (int i = 0; i < 3; i++)
            {
                float noteAngle = MathHelper.TwoPi * i / 3f;
                Vector2 notePos = Projectile.Center + noteAngle.ToRotationVector2() * 18f;
                Vector2 noteVel = noteAngle.ToRotationVector2() * 2f;
                Color noteColor = Color.Lerp(UnifiedVFX.Eroica.Gold, UnifiedVFX.Eroica.Sakura, (float)i / 3f);
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.28f, 22);
            }
            
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.6f, Pitch = 0.2f }, Projectile.Center);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            // === Central impact flash ===
            CustomParticles.GenericFlare(target.Center, Color.White, 0.8f, 12);
            CustomParticles.GenericFlare(target.Center, UnifiedVFX.Eroica.Crimson, 0.6f, 15);
            
            // === UnifiedVFX themed impact ===
            UnifiedVFX.Eroica.Impact(target.Center, 0.9f);
            
            // === Fractal burst ===
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                float gradientProgress = (float)i / 5f;
                Vector2 flareOffset = angle.ToRotationVector2() * 22f;
                Color fractalColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, gradientProgress);
                CustomParticles.GenericFlare(target.Center + flareOffset, fractalColor, 0.35f, 12);
            }
            
            // === Gradient spark spray ===
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                float gradientProgress = Main.rand.NextFloat();
                Color sparkColor = Color.Lerp(UnifiedVFX.Eroica.Flame, UnifiedVFX.Eroica.Gold, gradientProgress);
                
                var spark = new GenericGlowParticle(target.Center, sparkVel, sparkColor, 0.25f, 15, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // === Sakura petals ===
            ThemedParticles.SakuraPetals(target.Center, 2, 28f);
            
            // === Halo ring ===
            CustomParticles.HaloRing(target.Center, UnifiedVFX.Eroica.Gold, 0.35f, 12);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/PrismaticSparkle12").Value;
            Vector2 drawOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
            Vector2 glowOrigin = glowTex.Size() / 2f;
            
            // === Multi-layer trail with additive blending ===
            MagnumVFX.BeginAdditiveBlend(spriteBatch);
            
            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                if (Projectile.oldPos[k] == Vector2.Zero) continue;
                
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + new Vector2(Projectile.width / 2, Projectile.height / 2);
                float trailProgress = (float)k / Projectile.oldPos.Length;
                float trailAlpha = 1f - trailProgress;
                float trailScale = Projectile.scale * (1f - trailProgress * 0.3f);
                
                // Layer 1: Outer scarlet glow
                Color outerColor = UnifiedVFX.Eroica.Scarlet * trailAlpha * 0.35f;
                spriteBatch.Draw(glowTex, drawPos, null, outerColor, Projectile.oldRot[k], 
                    glowOrigin, trailScale * 3f, SpriteEffects.None, 0f);
                
                // Layer 2: Mid gold glow
                Color midColor = UnifiedVFX.Eroica.Gold * trailAlpha * 0.4f;
                spriteBatch.Draw(glowTex, drawPos, null, midColor, Projectile.oldRot[k], 
                    glowOrigin, trailScale * 2.2f, SpriteEffects.None, 0f);
                
                // Layer 3: Inner flame core
                Color innerColor = Color.Lerp(UnifiedVFX.Eroica.Flame, UnifiedVFX.Eroica.Sakura, trailProgress) * trailAlpha * 0.5f;
                spriteBatch.Draw(glowTex, drawPos, null, innerColor, Projectile.oldRot[k], 
                    glowOrigin, trailScale * 1.4f, SpriteEffects.None, 0f);
                
                // Texture afterimage every 2nd position
                if (k % 2 == 0)
                {
                    Color textureTrailColor = Color.Lerp(UnifiedVFX.Eroica.Gold, UnifiedVFX.Eroica.Crimson, trailProgress) * trailAlpha * 0.6f;
                    spriteBatch.Draw(texture, drawPos, null, textureTrailColor, Projectile.oldRot[k], 
                        drawOrigin, trailScale * 0.9f, SpriteEffects.None, 0f);
                }
            }
            
            MagnumVFX.EndAdditiveBlend(spriteBatch);
            
            // === Main projectile with enhanced glow layers ===
            Vector2 mainPos = Projectile.Center - Main.screenPosition;
            float pulse = (float)Math.Sin(Projectile.timeLeft * 0.1f) * 0.1f + 1f;
            
            MagnumVFX.BeginAdditiveBlend(spriteBatch);
            
            // Outer scarlet bloom
            spriteBatch.Draw(glowTex, mainPos, null, UnifiedVFX.Eroica.Scarlet * 0.3f, Projectile.rotation, 
                glowOrigin, Projectile.scale * pulse * 3.5f, SpriteEffects.None, 0f);
            
            // Mid gold bloom
            spriteBatch.Draw(glowTex, mainPos, null, UnifiedVFX.Eroica.Gold * 0.4f, Projectile.rotation, 
                glowOrigin, Projectile.scale * pulse * 2.5f, SpriteEffects.None, 0f);
            
            // Inner flame bloom
            spriteBatch.Draw(glowTex, mainPos, null, UnifiedVFX.Eroica.Flame * 0.45f, Projectile.rotation, 
                glowOrigin, Projectile.scale * pulse * 1.6f, SpriteEffects.None, 0f);
            
            MagnumVFX.EndAdditiveBlend(spriteBatch);
            
            // Main texture with bright glow tint
            spriteBatch.Draw(texture, mainPos, null, new Color(255, 245, 220), Projectile.rotation, 
                drawOrigin, Projectile.scale, SpriteEffects.None, 0f);

            return false;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 240, 200, 220);
        }
    }
}
