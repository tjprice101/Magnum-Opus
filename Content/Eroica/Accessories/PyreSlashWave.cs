using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;
using MagnumOpus.Content.MoonlightSonata.Debuffs;

namespace MagnumOpus.Content.Eroica.Accessories
{
    /// <summary>
    /// 360° sakura slash wave projectile spawned by Pyre of the Fallen Hero at max Fury stacks.
    /// Deals 400% melee damage in a massive circular area.
    /// Visual: Expanding ring of scarlet/pink flame with sakura petals and custom trail rendering.
    /// </summary>
    public class PyreSlashWave : ModProjectile
    {
        // Custom texture - no vanilla textures allowed
        public override string Texture => "MagnumOpus/Assets/Particles/SwordArc5";
        
        private float currentRadius = 0f;
        private const float MaxRadius = 250f;
        private const float ExpansionSpeed = 25f;
        
        // Trail history for custom rendering
        private List<float> radiusHistory = new List<float>();
        private const int TrailLength = 8;
        
        public override void SetDefaults()
        {
            Projectile.width = 500;
            Projectile.height = 500;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 20;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            // Store radius history for trail
            radiusHistory.Insert(0, currentRadius);
            if (radiusHistory.Count > TrailLength)
                radiusHistory.RemoveAt(radiusHistory.Count - 1);
            
            // Expand the radius
            currentRadius += ExpansionSpeed;
            
            // Update hitbox based on current radius
            int size = (int)(currentRadius * 2);
            Projectile.width = size;
            Projectile.height = size;
            Projectile.Center = Projectile.Center; // Recenter
            
            // Create the expanding ring of flames
            CreateFlameRing();
            
            // Create sakura petals
            CreateSakuraPetals();
            
            // Inner fire burst
            CreateInnerBurst();
            
            // Lighting - intense glow
            float glowIntensity = 1f - (currentRadius / MaxRadius) * 0.5f;
            Lighting.AddLight(Projectile.Center, 1.2f * glowIntensity, 0.5f * glowIntensity, 0.4f * glowIntensity);
            
            // Add additional lights around the ring
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 lightPos = Projectile.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * currentRadius;
                Lighting.AddLight(lightPos, 0.8f * glowIntensity, 0.3f * glowIntensity, 0.3f * glowIntensity);
            }
            
            if (currentRadius >= MaxRadius)
            {
                Projectile.Kill();
            }
        }
        
        private void CreateFlameRing()
        {
            int particleCount = (int)(currentRadius / 3f);
            
            for (int i = 0; i < particleCount; i++)
            {
                float angle = MathHelper.TwoPi * i / particleCount;
                Vector2 pos = Projectile.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * currentRadius;
                
                // Scarlet flame on the ring
                Dust flame = Dust.NewDustPerfect(pos, DustID.CrimsonTorch, 
                    new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 2f, 0, default, 2.5f);
                flame.noGravity = true;
                flame.fadeIn = 1.5f;
                
                // Alternate with pink for sakura effect
                if (i % 3 == 0)
                {
                    Dust pink = Dust.NewDustPerfect(pos, DustID.PinkTorch, 
                        new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 1.5f, 0, default, 2f);
                    pink.noGravity = true;
                }
            }
        }
        
        private void CreateSakuraPetals()
        {
            // Sakura petals scattered throughout
            for (int i = 0; i < 8; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float dist = Main.rand.NextFloat(currentRadius * 0.3f, currentRadius);
                Vector2 pos = Projectile.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * dist;
                
                // Golden petal
                Dust petal = Dust.NewDustPerfect(pos, DustID.GoldFlame, 
                    Main.rand.NextVector2Circular(3f, 3f), 0, default, 1.5f);
                petal.noGravity = true;
            }
        }
        
        private void CreateInnerBurst()
        {
            // Inner golden sparks
            for (int i = 0; i < 3; i++)
            {
                Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(currentRadius * 0.5f, currentRadius * 0.5f);
                Dust spark = Dust.NewDustPerfect(pos, DustID.GoldCoin, 
                    Main.rand.NextVector2Circular(4f, 4f), 0, default, 1.3f);
                spark.noGravity = true;
            }
            
            // Black smoke in center
            if (Main.rand.NextBool(2))
            {
                Dust smoke = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(30f, 30f),
                    DustID.Smoke, new Vector2(0, -3f), 180, Color.Black, 2f);
                smoke.noGravity = true;
            }
        }
        
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Circular collision based on current radius
            float dist = Vector2.Distance(Projectile.Center, targetHitbox.Center.ToVector2());
            return dist < currentRadius + Math.Max(targetHitbox.Width, targetHitbox.Height) / 2f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply debuff
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 300);
            
            // Hit effect - flame burst on enemy
            for (int i = 0; i < 15; i++)
            {
                Dust hitFlame = Dust.NewDustPerfect(target.Center, DustID.CrimsonTorch,
                    Main.rand.NextVector2Circular(6f, 6f), 0, default, 2f);
                hitFlame.noGravity = true;
            }
            
            // Pink sparks
            for (int i = 0; i < 8; i++)
            {
                Dust pink = Dust.NewDustPerfect(target.Center, DustID.PinkTorch,
                    Main.rand.NextVector2Circular(4f, 4f), 0, default, 1.5f);
                pink.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            
            // End the current sprite batch and restart with additive blending for glow
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Draw trail rings (older = more transparent)
            for (int t = radiusHistory.Count - 1; t >= 0; t--)
            {
                float trailRadius = radiusHistory[t];
                float trailAlpha = (1f - (float)t / TrailLength) * 0.4f;
                DrawSlashRing(spriteBatch, trailRadius, trailAlpha, t);
            }
            
            // Draw main ring with glow
            DrawSlashRing(spriteBatch, currentRadius, 0.8f, -1);
            
            // Draw bright inner glow
            DrawCenterGlow(spriteBatch);
            
            // Restore normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
        
        private void DrawSlashRing(SpriteBatch spriteBatch, float radius, float alpha, int trailIndex)
        {
            // Use a simple pixel texture for the ring segments
            Texture2D glowTex = TextureAssets.MagicPixel.Value;
            
            int segments = 60;
            float thickness = 20f - (trailIndex >= 0 ? trailIndex * 2f : 0f);
            
            for (int i = 0; i < segments; i++)
            {
                float angle = MathHelper.TwoPi * i / segments;
                float nextAngle = MathHelper.TwoPi * (i + 1) / segments;
                
                Vector2 pos1 = Projectile.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                Vector2 pos2 = Projectile.Center + new Vector2((float)Math.Cos(nextAngle), (float)Math.Sin(nextAngle)) * radius;
                
                // Alternating colors for sakura effect
                Color baseColor = i % 3 == 0 ? new Color(255, 100, 150) : new Color(255, 50, 50);
                Color glowColor = baseColor * alpha;
                
                // Draw segment as a line
                Vector2 direction = pos2 - pos1;
                float segmentLength = direction.Length();
                float rotation = direction.ToRotation();
                
                spriteBatch.Draw(glowTex, pos1 - Main.screenPosition, new Rectangle(0, 0, 1, 1), 
                    glowColor, rotation, Vector2.Zero, new Vector2(segmentLength, thickness), SpriteEffects.None, 0f);
            }
        }
        
        private void DrawCenterGlow(SpriteBatch spriteBatch)
        {
            // Draw a large soft glow in the center
            Texture2D glowTex = TextureAssets.Extra[ExtrasID.SharpTears].Value; // Soft glow texture
            
            float glowScale = (currentRadius / MaxRadius) * 2f + 1f;
            float glowAlpha = 1f - (currentRadius / MaxRadius) * 0.7f;
            
            // Scarlet glow
            spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null,
                new Color(255, 80, 80) * glowAlpha * 0.6f, 0f, glowTex.Size() / 2f, glowScale, SpriteEffects.None, 0f);
            
            // Pink inner glow
            spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null,
                new Color(255, 150, 180) * glowAlpha * 0.4f, 0f, glowTex.Size() / 2f, glowScale * 0.6f, SpriteEffects.None, 0f);
            
            // Golden core
            spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null,
                new Color(255, 200, 100) * glowAlpha * 0.3f, 0f, glowTex.Size() / 2f, glowScale * 0.3f, SpriteEffects.None, 0f);
        }
    }
}

