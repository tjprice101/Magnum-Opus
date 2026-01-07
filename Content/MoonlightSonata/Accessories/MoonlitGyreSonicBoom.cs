using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace MagnumOpus.Content.MoonlightSonata.Accessories
{
    /// <summary>
    /// Moonlit Gyre Sonic Boom - Large spiral dark purple and light blue explosion.
    /// Pierces through 5 enemies with unique visual effects.
    /// </summary>
    public class MoonlitGyreSonicBoom : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.SolarWhipSwordExplosion;
        
        private float boomRadius = 0f;
        private const float MaxRadius = 180f;
        private const float ExpansionSpeed = 25f;
        private int enemiesHit = 0;
        private const int MaxPierces = 5;
        private float spiralAngle = 0f;
        
        public override void SetDefaults()
        {
            Projectile.width = 360;
            Projectile.height = 360;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = MaxPierces;
            Projectile.timeLeft = 30;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }
        
        public override void AI()
        {
            // Expand the boom
            boomRadius += ExpansionSpeed;
            if (boomRadius > MaxRadius)
                boomRadius = MaxRadius;
            
            float progress = boomRadius / MaxRadius;
            spiralAngle += 0.15f; // Rotate spiral
            
            // Initial burst sound
            if (Projectile.timeLeft == 29)
            {
                SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.6f, Pitch = 0.6f }, Projectile.Center);
                SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.4f, Pitch = 0.3f }, Projectile.Center);
            }
            
            // === LARGE SPIRAL EXPLOSION ===
            // Multiple spiral arms of dark purple and light blue
            int spiralArms = 6;
            for (int arm = 0; arm < spiralArms; arm++)
            {
                float armBaseAngle = MathHelper.TwoPi * arm / spiralArms + spiralAngle;
                
                // Draw particles along each spiral arm
                for (float r = 10f; r < boomRadius; r += 8f)
                {
                    // Spiral outward - angle increases with radius
                    float spiralOffset = (r / MaxRadius) * MathHelper.Pi * 1.5f;
                    float angle = armBaseAngle + spiralOffset;
                    
                    Vector2 pos = Projectile.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * r;
                    Vector2 tangentVel = new Vector2((float)Math.Cos(angle + MathHelper.PiOver2), (float)Math.Sin(angle + MathHelper.PiOver2)) * 3f;
                    
                    // Alternate dark purple (Shadowflame) and light blue
                    int dustType = arm % 2 == 0 ? DustID.Shadowflame : DustID.IceTorch;
                    float dustScale = 2.5f * (1f - progress * 0.4f) * (1f - r / MaxRadius * 0.3f);
                    
                    Dust spiral = Dust.NewDustPerfect(pos, dustType, tangentVel, arm % 2 == 0 ? 100 : 0, default, dustScale);
                    spiral.noGravity = true;
                    spiral.fadeIn = 1.3f;
                }
            }
            
            // === OUTER EXPLOSION RING - Dark Purple ===
            int outerParticles = (int)(35 + progress * 20);
            for (int i = 0; i < outerParticles; i++)
            {
                float angle = MathHelper.TwoPi * i / outerParticles + spiralAngle * 0.5f;
                Vector2 ringPos = Projectile.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * boomRadius;
                Vector2 outwardVel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 4f;
                
                Dust outer = Dust.NewDustPerfect(ringPos, DustID.Shadowflame, outwardVel, 100, default, 2.2f * (1f - progress * 0.4f));
                outer.noGravity = true;
                outer.fadeIn = 1.4f;
            }
            
            // === INNER EXPLOSION BURST - Light Blue ===
            int innerParticles = (int)(25 + progress * 15);
            for (int i = 0; i < innerParticles; i++)
            {
                float angle = MathHelper.TwoPi * i / innerParticles - spiralAngle * 0.3f;
                Vector2 innerPos = Projectile.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * boomRadius * 0.6f;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 3f;
                
                Dust inner = Dust.NewDustPerfect(innerPos, DustID.IceTorch, vel, 0, default, 2f * (1f - progress * 0.3f));
                inner.noGravity = true;
                inner.fadeIn = 1.2f;
            }
            
            // === CENTER BURST ===
            for (int i = 0; i < 5; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(6f, 6f);
                int dustType = Main.rand.NextBool() ? DustID.Shadowflame : DustID.IceTorch;
                Dust burst = Dust.NewDustPerfect(Projectile.Center, dustType, burstVel, dustType == DustID.Shadowflame ? 100 : 0, default, 2.5f);
                burst.noGravity = true;
            }
            
            // === Lighting ===
            float lightIntensity = 1f - progress * 0.4f;
            Lighting.AddLight(Projectile.Center, 0.4f * lightIntensity, 0.2f * lightIntensity, 0.6f * lightIntensity);
        }
        
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Ring collision
            Vector2 targetCenter = targetHitbox.Center.ToVector2();
            float dist = Vector2.Distance(Projectile.Center, targetCenter);
            
            // Hit if within the expanding ring area
            return dist <= boomRadius + 20f;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            enemiesHit++;
            
            // Impact effect - burst of dark purple and light blue
            for (int ring = 0; ring < 2; ring++)
            {
                for (int i = 0; i < 15; i++)
                {
                    float angle = MathHelper.TwoPi * i / 15f;
                    Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (4f + ring * 2f);
                    
                    int dustType = (i + ring) % 2 == 0 ? DustID.Shadowflame : DustID.IceTorch;
                    Dust impact = Dust.NewDustPerfect(target.Center, dustType, vel, 100, default, 1.6f);
                    impact.noGravity = true;
                }
            }
            
            // Sound on each hit
            SoundEngine.PlaySound(SoundID.Item12 with { Volume = 0.3f, Pitch = 0.6f + enemiesHit * 0.1f }, target.Center);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
            
            float progress = boomRadius / MaxRadius;
            float alpha = (1f - progress) * 0.5f;
            
            // Draw concentric rings
            // Outer dark purple ring
            DrawRing(spriteBatch, pixel, Projectile.Center - Main.screenPosition, boomRadius, 6f, new Color(80, 40, 120) * alpha);
            
            // Middle light blue ring
            DrawRing(spriteBatch, pixel, Projectile.Center - Main.screenPosition, boomRadius * 0.7f, 4f, new Color(100, 150, 220) * alpha * 0.8f);
            
            // Inner purple ring
            DrawRing(spriteBatch, pixel, Projectile.Center - Main.screenPosition, boomRadius * 0.4f, 3f, new Color(140, 80, 180) * alpha * 0.6f);
            
            return false;
        }
        
        private void DrawRing(SpriteBatch spriteBatch, Texture2D texture, Vector2 center, float radius, float thickness, Color color)
        {
            int segments = 40;
            for (int i = 0; i < segments; i++)
            {
                float angle1 = MathHelper.TwoPi * i / segments;
                float angle2 = MathHelper.TwoPi * (i + 1) / segments;
                
                Vector2 pos1 = center + new Vector2((float)Math.Cos(angle1), (float)Math.Sin(angle1)) * radius;
                Vector2 pos2 = center + new Vector2((float)Math.Cos(angle2), (float)Math.Sin(angle2)) * radius;
                
                Vector2 direction = pos2 - pos1;
                float length = direction.Length();
                float rotation = direction.ToRotation();
                
                spriteBatch.Draw(texture, pos1, null, color, rotation, Vector2.Zero, new Vector2(length, thickness), SpriteEffects.None, 0f);
            }
        }
    }
}
