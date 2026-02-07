using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.Eroica.Accessories
{
    /// <summary>
    /// Petal explosion projectile spawned by Symphony of Scarlet Flames on Triumphant Precision.
    /// Deals 300% ranged damage with a beautiful sakura petal explosion and glow effects.
    /// </summary>
    [AllowLargeHitbox("Sakura petal explosion requires large hitbox for AoE damage")]
    public class PetalExplosion : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/MagicSparklField6";
        
        private int explosionTimer = 0;
        private const int ExplosionDuration = 20;
        private const float ExplosionRadius = 120f;
        
        // Glow effect variables
        private float glowIntensity = 1f;
        private float shockwaveRadius = 0f;
        
        public override void SetDefaults()
        {
            Projectile.width = 240;
            Projectile.height = 240;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = ExplosionDuration;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            explosionTimer++;
            
            // Update glow intensity (bright flash then fade)
            if (explosionTimer <= 3)
                glowIntensity = 1f + (3 - explosionTimer) * 0.5f; // Initial bright flash
            else
                glowIntensity = Math.Max(0f, 1f - (float)(explosionTimer - 3) / (ExplosionDuration - 3));
            
            // Shockwave expansion
            shockwaveRadius = (float)explosionTimer / ExplosionDuration * ExplosionRadius * 1.5f;
            
            if (explosionTimer == 1)
            {
                // Initial explosion burst
                CreateInitialBurst();
            }
            
            // Ongoing petal shower
            CreatePetalShower();
            
            // Intense lighting
            float lightIntensity = glowIntensity;
            Lighting.AddLight(Projectile.Center, 1.2f * lightIntensity, 0.6f * lightIntensity, 0.5f * lightIntensity);
            
            // Ring of lights
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f + explosionTimer * 0.1f;
                Vector2 lightPos = Projectile.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * shockwaveRadius * 0.7f;
                Lighting.AddLight(lightPos, 0.6f * lightIntensity, 0.3f * lightIntensity, 0.4f * lightIntensity);
            }
        }
        
        private void CreateInitialBurst()
        {
            // Unique sakura petal explosion - pearlescent bloom
            CustomParticles.SwanLakeHalo(Projectile.Center, 0.8f); // Pearlescent shimmer
            CustomParticles.GenericFlare(Projectile.Center, new Color(255, 180, 200), 1.8f, 40);
            CustomParticles.GenericGlow(Projectile.Center, new Color(255, 220, 230), 1.4f, 35);
            CustomParticles.ExplosionBurst(Projectile.Center, new Color(255, 150, 180), 14, 7f);
            
            // Large ring explosion
            for (int ring = 0; ring < 3; ring++)
            {
                int particlesInRing = 20 + ring * 10;
                for (int i = 0; i < particlesInRing; i++)
                {
                    float angle = MathHelper.TwoPi * i / particlesInRing;
                    float speed = 8f + ring * 3f;
                    Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
                    
                    // Scarlet flame
                    Dust flame = Dust.NewDustPerfect(Projectile.Center, DustID.CrimsonTorch, vel, 0, default, 2.5f - ring * 0.3f);
                    flame.noGravity = true;
                    flame.fadeIn = 1.5f;
                }
            }
            
            // Pink sakura burst
            for (int i = 0; i < 30; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(10f, 10f);
                Dust petal = Dust.NewDustPerfect(Projectile.Center, DustID.PinkTorch, vel, 0, default, 2f);
                petal.noGravity = true;
                petal.fadeIn = 1.3f;
            }
            
            // Golden center burst
            for (int i = 0; i < 20; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                Dust gold = Dust.NewDustPerfect(Projectile.Center, DustID.GoldCoin, vel, 0, default, 1.8f);
                gold.noGravity = true;
            }
            
            // Central black smoke
            for (int i = 0; i < 15; i++)
            {
                Dust smoke = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f),
                    DustID.Smoke, Main.rand.NextVector2Circular(4f, 4f), 180, Color.Black, 2f);
                smoke.noGravity = true;
            }
            
            // Play explosion sound
            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Volume = 0.8f, Pitch = 0.3f }, Projectile.Center);
        }
        
        private void CreatePetalShower()
        {
            // Falling sakura petals
            for (int i = 0; i < 5; i++)
            {
                Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(ExplosionRadius, ExplosionRadius);
                
                // Golden petals falling gently
                Dust petal = Dust.NewDustPerfect(pos, DustID.GoldFlame, 
                    new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(1f, 3f)), 0, default, 1.2f);
                petal.noGravity = false; // Let them fall
            }
            
            // Lingering embers
            if (Main.rand.NextBool(2))
            {
                Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(ExplosionRadius * 0.7f, ExplosionRadius * 0.7f);
                Dust ember = Dust.NewDustPerfect(pos, DustID.CrimsonTorch, new Vector2(0, -2f), 100, default, 1f);
                ember.noGravity = true;
            }
        }
        
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Only damage on initial explosion (first few frames)
            if (explosionTimer > 5)
                return false;
            
            // Circular collision
            float dist = Vector2.Distance(Projectile.Center, targetHitbox.Center.ToVector2());
            return dist < ExplosionRadius + Math.Max(targetHitbox.Width, targetHitbox.Height) / 2f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply debuff
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 240);
            
            // Hit effect - petal burst on enemy
            for (int i = 0; i < 12; i++)
            {
                Dust petal = Dust.NewDustPerfect(target.Center, DustID.PinkTorch,
                    Main.rand.NextVector2Circular(5f, 5f), 0, default, 1.5f);
                petal.noGravity = true;
            }
            
            // Golden sparks
            for (int i = 0; i < 8; i++)
            {
                Dust gold = Dust.NewDustPerfect(target.Center, DustID.GoldCoin,
                    Main.rand.NextVector2Circular(4f, 4f), 0, default, 1.2f);
                gold.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            
            // End current batch and start with additive blending for glow
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Draw layered glow effect
            DrawExplosionGlow(spriteBatch);
            
            // Draw shockwave ring
            DrawShockwaveRing(spriteBatch);
            
            // Restore normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
        
        private void DrawExplosionGlow(SpriteBatch spriteBatch)
        {
            Texture2D glowTex = TextureAssets.Extra[ExtrasID.SharpTears].Value; // Soft glow texture
            
            // Outer scarlet glow
            float outerScale = 2.5f * glowIntensity;
            spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null,
                new Color(255, 60, 60) * glowIntensity * 0.7f, 0f, glowTex.Size() / 2f, outerScale, SpriteEffects.None, 0f);
            
            // Middle pink glow
            float midScale = 1.8f * glowIntensity;
            spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null,
                new Color(255, 120, 150) * glowIntensity * 0.6f, 0f, glowTex.Size() / 2f, midScale, SpriteEffects.None, 0f);
            
            // Inner golden glow
            float innerScale = 1.2f * glowIntensity;
            spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null,
                new Color(255, 220, 150) * glowIntensity * 0.8f, 0f, glowTex.Size() / 2f, innerScale, SpriteEffects.None, 0f);
            
            // Bright white core (initial flash only)
            if (explosionTimer <= 5)
            {
                float coreScale = 0.8f * (1f - explosionTimer / 5f);
                spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null,
                    Color.White * (1f - explosionTimer / 5f), 0f, glowTex.Size() / 2f, coreScale, SpriteEffects.None, 0f);
            }
        }
        
        private void DrawShockwaveRing(SpriteBatch spriteBatch)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            
            int segments = 48;
            float thickness = 8f * glowIntensity;
            float alpha = glowIntensity * 0.5f;
            
            for (int i = 0; i < segments; i++)
            {
                float angle = MathHelper.TwoPi * i / segments;
                float nextAngle = MathHelper.TwoPi * (i + 1) / segments;
                
                Vector2 pos1 = Projectile.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * shockwaveRadius;
                Vector2 pos2 = Projectile.Center + new Vector2((float)Math.Cos(nextAngle), (float)Math.Sin(nextAngle)) * shockwaveRadius;
                
                // Color varies around the ring
                Color ringColor = Color.Lerp(new Color(255, 100, 120), new Color(255, 200, 150), (float)i / segments);
                ringColor *= alpha;
                
                Vector2 direction = pos2 - pos1;
                float segmentLength = direction.Length();
                float rotation = direction.ToRotation();
                
                spriteBatch.Draw(pixel, pos1 - Main.screenPosition, new Rectangle(0, 0, 1, 1),
                    ringColor, rotation, Vector2.Zero, new Vector2(segmentLength, thickness), SpriteEffects.None, 0f);
            }
        }
    }
}

