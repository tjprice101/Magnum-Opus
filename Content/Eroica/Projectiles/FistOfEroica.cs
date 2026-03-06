using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.FoundationWeapons.SparkleProjectileFoundation;
using ReLogic.Content;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// Fist of Eroica - Spawns at screen edges, pauses briefly, then blasts across to the other side.
    /// Part of Eroica boss Phase 2 attack pattern.
    /// Uses HandOfValor.png texture.
    /// ai[0] = timer, ai[1] = charge direction encoded (0=down, 1=up, 2=right, 3=left)
    /// </summary>
    public class FistOfEroica : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/Eroica/Projectiles/HandOfValor";
        
        private const int WindupTime = 30; // 0.5 seconds before charging
        private bool hasCharged = false;
        private Vector2 chargeDirection = Vector2.Zero;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = -1; // Goes through everything
            Projectile.timeLeft = 180; // Long enough for windup + crossing screen
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.light = 0.8f;
            Projectile.scale = 0.272f; // 15% smaller than 0.32
        }

        public override void AI()
        {
            Projectile.ai[0]++;
            
            // Decode charge direction from ai[1]
            if (chargeDirection == Vector2.Zero)
            {
                switch ((int)Projectile.ai[1])
                {
                    case 0: chargeDirection = new Vector2(0, 1); break;   // Down
                    case 1: chargeDirection = new Vector2(0, -1); break;  // Up
                    case 2: chargeDirection = new Vector2(1, 0); break;   // Right
                    case 3: chargeDirection = new Vector2(-1, 0); break;  // Left
                    default: chargeDirection = new Vector2(0, 1); break;
                }
            }
            
            // Windup phase - hover in place with warning effects
            if (Projectile.ai[0] < WindupTime)
            {
                Projectile.velocity = Vector2.Zero;
                
                // Orient toward charge direction
                Projectile.rotation = chargeDirection.ToRotation() - MathHelper.PiOver2;
                
                // Pulsing warning glow effect
                float pulse = (float)Math.Sin(Projectile.ai[0] * 0.3f) * 0.5f + 0.5f;
                float progress = Projectile.ai[0] / WindupTime;
                
                // === PHASE 1: Central pulsing flare - building energy ===
                Color centralColor = Color.Lerp(EroicaPalette.Scarlet, EroicaPalette.Gold, pulse);
                EroicaVFXLibrary.BloomFlare(Projectile.Center, centralColor, 0.5f + progress * 0.4f, 8);
                
                // === PHASE 2: Converging gradient particles - power gathering ===
                if (Projectile.ai[0] % 2 == 0)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        float radius = 80f * (1f - progress * 0.5f);
                        Vector2 spawnPos = Projectile.Center + angle.ToRotationVector2() * radius;
                        Vector2 vel = (Projectile.Center - spawnPos).SafeNormalize(Vector2.Zero) * (4f + progress * 3f);
                        float gradientProgress = Main.rand.NextFloat();
                        Color particleColor = Color.Lerp(EroicaPalette.Scarlet, EroicaPalette.Gold, gradientProgress);
                        
                        var glow = new GenericGlowParticle(spawnPos, vel, particleColor, 0.35f + progress * 0.15f, 18, true);
                        MagnumParticleHandler.SpawnParticle(glow);
                    }
                }
                
                // === PHASE 3: Orbiting warning ring - geometric buildup ===
                if (Projectile.ai[0] % 4 == 0)
                {
                    int points = 6;
                    for (int i = 0; i < points; i++)
                    {
                        float orbitAngle = MathHelper.TwoPi * i / points + Projectile.ai[0] * 0.15f;
                        float orbitRadius = 35f + pulse * 10f;
                        Vector2 orbitPos = Projectile.Center + orbitAngle.ToRotationVector2() * orbitRadius;
                        Color orbitColor = Color.Lerp(EroicaPalette.Crimson, EroicaPalette.Sakura, (float)i / points);
                        EroicaVFXLibrary.BloomFlare(orbitPos, orbitColor, 0.3f + progress * 0.2f, 10);
                    }
                }
                
                // === PHASE 4: Sakura petal warning - thematic signature ===
                if (Main.rand.NextBool(4))
                {
                    EroicaVFXLibrary.SpawnSakuraPetals(Projectile.Center, 1, 45f);
                }
                
                // === PHASE 5: Music notes as the fist prepares ===
                if (Main.rand.NextBool(6) && progress > 0.4f)
                {
                    Vector2 noteVel = Main.rand.NextVector2Circular(2f, 2f);
                    EroicaVFXLibrary.SpawnMusicNote(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f), 
                        noteVel, EroicaPalette.Gold, 0.35f, 25);
                }
                
                // Enhanced pulsing lighting
                float lightIntensity = 0.8f + progress * 0.5f + pulse * 0.2f;
                Lighting.AddLight(Projectile.Center, EroicaPalette.Crimson.ToVector3() * lightIntensity);
                
                return;
            }
            
            // Start charging!
            if (!hasCharged)
            {
                hasCharged = true;
                Projectile.velocity = chargeDirection * 60f; // Very fast charge!
                SoundEngine.PlaySound(SoundID.Item117 with { Pitch = 0.3f, Volume = 0.7f }, Projectile.Center);
                
                // === CHARGE START: MASSIVE FRACTAL BURST ===
                
                // Central white flash - the moment of release
                EroicaVFXLibrary.BloomFlare(Projectile.Center, Color.White, 1.4f, 20);
                
                // Eroica themed impact
                EroicaVFXLibrary.HeroicImpact(Projectile.Center, 1.5f);
                
                // 8-point fractal star burst with Eroica gradient
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 flareOffset = angle.ToRotationVector2() * 45f;
                    float gradientProgress = (float)i / 8f;
                    Color fractalColor = Color.Lerp(EroicaPalette.Scarlet, EroicaPalette.Gold, gradientProgress);
                    EroicaVFXLibrary.BloomFlare(Projectile.Center + flareOffset, fractalColor, 0.6f, 18);
                    
                    // Secondary ring
                    Vector2 innerOffset = angle.ToRotationVector2() * 25f;
                    Color innerColor = Color.Lerp(EroicaPalette.Crimson, EroicaPalette.Sakura, gradientProgress);
                    EroicaVFXLibrary.BloomFlare(Projectile.Center + innerOffset, innerColor, 0.45f, 15);
                }
                
                // Cascading halo rings
                for (int ring = 0; ring < 4; ring++)
                {
                    EroicaVFXLibrary.SpawnGradientHaloRings(Projectile.Center, 1, 0.5f + ring * 0.2f);
                }
                
                // Sakura petal explosion for thematic signature
                EroicaVFXLibrary.SpawnSakuraPetals(Projectile.Center, 8, 60f);
                
                // Spark spray in charge direction
                for (int i = 0; i < 12; i++)
                {
                    float spreadAngle = chargeDirection.ToRotation() + Main.rand.NextFloat(-0.6f, 0.6f);
                    Vector2 sparkVel = spreadAngle.ToRotationVector2() * Main.rand.NextFloat(8f, 16f);
                    float gradientProgress = Main.rand.NextFloat();
                    Color sparkColor = Color.Lerp(EroicaPalette.Flame, EroicaPalette.Gold, gradientProgress);
                    
                    var spark = new GenericGlowParticle(Projectile.Center, sparkVel, sparkColor, 0.4f, 20, true);
                    MagnumParticleHandler.SpawnParticle(spark);
                }
                
                // Music note flourish
                for (int i = 0; i < 4; i++)
                {
                    float noteAngle = MathHelper.TwoPi * i / 4f;
                    Vector2 notePos = Projectile.Center + noteAngle.ToRotationVector2() * 30f;
                    Vector2 noteVel = noteAngle.ToRotationVector2() * 3f;
                    Color noteColor = Color.Lerp(EroicaPalette.Gold, EroicaPalette.Sakura, (float)i / 4f);
                    EroicaVFXLibrary.SpawnMusicNote(notePos, noteVel, noteColor, 0.4f, 30);
                }
            }
            
            // Orient to direction of travel
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
            
            // === CHARGE TRAIL: Dense gradient particle trail ===
            
            // Core glow every frame - the blazing fist
            float trailPulse = (float)Math.Sin(Projectile.ai[0] * 0.15f) * 0.15f + 0.85f;
            EroicaVFXLibrary.BloomFlare(Projectile.Center, EroicaPalette.Gold * trailPulse, 0.55f, 6);
            
            // Dense gradient glow particles
            for (int i = 0; i < 4; i++)
            {
                float gradientProgress = Main.rand.NextFloat();
                Color particleColor = Color.Lerp(EroicaPalette.Scarlet, EroicaPalette.Gold, gradientProgress);
                Vector2 randomOffset = Main.rand.NextVector2Circular(12f, 12f);
                Vector2 vel = -Projectile.velocity * 0.12f + Main.rand.NextVector2Circular(2f, 2f);
                
                var glow = new GenericGlowParticle(Projectile.Center + randomOffset, vel, particleColor, 
                    0.3f + Main.rand.NextFloat(0.15f), 18, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Sakura petals in the wake - thematic elegance
            if (Main.rand.NextBool(3))
            {
                Vector2 petalOffset = Main.rand.NextVector2Circular(15f, 15f);
                EroicaVFXLibrary.SpawnSakuraPetals(Projectile.Center + petalOffset, 1, 25f);
            }
            
            // Music notes occasionally streaming behind
            if (Main.rand.NextBool(5))
            {
                float noteProgress = Main.rand.NextFloat();
                Color noteColor = Color.Lerp(EroicaPalette.Gold, EroicaPalette.Sakura, noteProgress);
                Vector2 noteVel = -Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                EroicaVFXLibrary.SpawnMusicNote(Projectile.Center, noteVel, noteColor, 0.3f, 25);
            }
            
            // Periodic flare bursts - rhythmic intensity
            if (Projectile.ai[0] % 8 == 0)
            {
                // Small fractal ring around the fist
                for (int i = 0; i < 4; i++)
                {
                    float angle = MathHelper.TwoPi * i / 4f + Projectile.ai[0] * 0.1f;
                    Vector2 flarePos = Projectile.Center + angle.ToRotationVector2() * 25f;
                    Color flareColor = Color.Lerp(EroicaPalette.Crimson, EroicaPalette.Flame, (float)i / 4f);
                    EroicaVFXLibrary.BloomFlare(flarePos, flareColor, 0.35f, 10);
                }
                
                // Mini halo pulse
                EroicaVFXLibrary.SpawnGradientHaloRings(Projectile.Center, 1, 0.3f);
            }
            
            // Strong dynamic lighting
            float lightPulse = (float)Math.Sin(Projectile.ai[0] * 0.2f) * 0.1f + 1.1f;
            Lighting.AddLight(Projectile.Center, EroicaPalette.Gold.ToVector3() * lightPulse);
            
            // Screen shake removed - weapons should not cause screen shake
        }

        public override void OnKill(int timeLeft)
        {
            // Powerful valor burst for heroic fist
            EroicaVFXLibrary.DeathHeroicFlash(Projectile.Center, 1.1f);
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.6f, Pitch = 0f }, Projectile.Center);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            // === Central impact flash ===
            EroicaVFXLibrary.BloomFlare(target.Center, Color.White, 1.0f, 15);
            EroicaVFXLibrary.BloomFlare(target.Center, EroicaPalette.Crimson, 0.75f, 18);
            
            // === Eroica themed impact ===
            EroicaVFXLibrary.HeroicImpact(target.Center, 1.2f);
            
            // === 6-point fractal burst ===
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 flareOffset = angle.ToRotationVector2() * 30f;
                float gradientProgress = (float)i / 6f;
                Color fractalColor = Color.Lerp(EroicaPalette.Scarlet, EroicaPalette.Gold, gradientProgress);
                EroicaVFXLibrary.BloomFlare(target.Center + flareOffset, fractalColor, 0.45f, 15);
            }
            
            // === Gradient spark spray ===
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                float gradientProgress = Main.rand.NextFloat();
                Color sparkColor = Color.Lerp(EroicaPalette.Flame, EroicaPalette.Gold, gradientProgress);
                
                var spark = new GenericGlowParticle(target.Center, sparkVel, sparkColor, 0.3f, 18, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // === Sakura petals on impact ===
            EroicaVFXLibrary.SpawnSakuraPetals(target.Center, 3, 35f);
            
            // === Halo rings ===
            EroicaVFXLibrary.SpawnGradientHaloRings(target.Center, 1, 0.4f);
            EroicaVFXLibrary.SpawnGradientHaloRings(target.Center, 1, 0.3f);
            
            // Screen shake removed - weapons should not cause screen shake
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Texture2D glowTex = SPFTextures.SparkleSoft.Value;
            Vector2 drawOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
            Vector2 glowOrigin = glowTex.Size() / 2f;
            
            try
            {
                // === Multi-layer trail with additive blending ===
                if (hasCharged && Projectile.oldPos.Length > 0)
                {
                    EroicaVFXLibrary.BeginEroicaAdditive(spriteBatch);
                    
                    for (int k = 0; k < Projectile.oldPos.Length; k++)
                    {
                        if (Projectile.oldPos[k] == Vector2.Zero) continue;
                        
                        Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + new Vector2(Projectile.width / 2, Projectile.height / 2);
                        float trailProgress = (float)k / Projectile.oldPos.Length;
                        float trailAlpha = 1f - trailProgress;
                        float trailScale = Projectile.scale * (1f - trailProgress * 0.4f);
                        
                        // Layer 1: Outer scarlet glow
                        Color outerColor = EroicaPalette.Scarlet * trailAlpha * 0.4f;
                        spriteBatch.Draw(glowTex, drawPos, null, outerColor, Projectile.oldRot[k], 
                            glowOrigin, trailScale * 2.2f, SpriteEffects.None, 0f);
                        
                        // Layer 2: Mid gold glow
                        Color midColor = EroicaPalette.Gold * trailAlpha * 0.35f;
                        spriteBatch.Draw(glowTex, drawPos, null, midColor, Projectile.oldRot[k], 
                            glowOrigin, trailScale * 1.6f, SpriteEffects.None, 0f);
                        
                        // Layer 3: Inner sakura/flame core
                        float gradientProgress = trailProgress;
                        Color innerColor = Color.Lerp(EroicaPalette.Flame, EroicaPalette.Sakura, gradientProgress) * trailAlpha * 0.5f;
                        spriteBatch.Draw(glowTex, drawPos, null, innerColor, Projectile.oldRot[k], 
                            glowOrigin, trailScale * 1.1f, SpriteEffects.None, 0f);
                        
                        // Texture afterimage every 3rd position
                        if (k % 3 == 0)
                        {
                            Color textureTrailColor = Color.Lerp(EroicaPalette.Gold, EroicaPalette.Crimson, gradientProgress) * trailAlpha * 0.6f;
                            spriteBatch.Draw(texture, drawPos, null, textureTrailColor, Projectile.oldRot[k], 
                                drawOrigin, trailScale * 0.9f, SpriteEffects.None, 0f);
                        }
                    }
                    
                    EroicaVFXLibrary.EndEroicaAdditive(spriteBatch);
                }
                
                // === Main projectile with enhanced glow layers ===
                Vector2 mainPos = Projectile.Center - Main.screenPosition;
                float pulse = (float)Math.Sin(Projectile.ai[0] * 0.15f) * 0.1f + 1f;
                float glowScale = hasCharged ? 1.2f : 1f + (float)Math.Sin(Projectile.ai[0] * 0.3f) * 0.3f;
                
                EroicaVFXLibrary.BeginEroicaAdditive(spriteBatch);
                
                // Outer scarlet bloom
                spriteBatch.Draw(glowTex, mainPos, null, EroicaPalette.Scarlet * 0.35f, Projectile.rotation, 
                    glowOrigin, Projectile.scale * glowScale * pulse * 2.5f, SpriteEffects.None, 0f);
                
                // Mid gold bloom
                spriteBatch.Draw(glowTex, mainPos, null, EroicaPalette.Gold * 0.4f, Projectile.rotation, 
                    glowOrigin, Projectile.scale * glowScale * pulse * 1.8f, SpriteEffects.None, 0f);
                
                // Inner flame bloom
                spriteBatch.Draw(glowTex, mainPos, null, EroicaPalette.Flame * 0.5f, Projectile.rotation, 
                    glowOrigin, Projectile.scale * glowScale * pulse * 1.2f, SpriteEffects.None, 0f);
                
                EroicaVFXLibrary.EndEroicaAdditive(spriteBatch);
                
                // Main texture with bright glow tint
                spriteBatch.Draw(texture, mainPos, null, new Color(255, 230, 200), Projectile.rotation, 
                    drawOrigin, Projectile.scale * glowScale, SpriteEffects.None, 0f);
            }
            finally
            {
                // Ensure SpriteBatch is restored to default state on any exception
                try { spriteBatch.End(); } catch { }
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            // Eroica theme accent
            EroicaVFXLibrary.BeginEroicaAdditive(spriteBatch);
            EroicaVFXLibrary.DrawThemeSakuraAccent(spriteBatch, Projectile.Center, 1f, 0.5f);
            EroicaVFXLibrary.EndEroicaAdditive(spriteBatch);

            return false;
        }
    }
}