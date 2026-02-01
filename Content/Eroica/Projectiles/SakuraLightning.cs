using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// Spiral explosion effect spawned by Piercing Light of the Sakura projectile.
    /// Red and gold spiral explosion with scarlet particles.
    /// </summary>
    public class SakuraLightning : ModProjectile
    {
        // Override texture to use vanilla since we draw with particles
        public override string Texture => "Terraria/Images/Projectile_0";
        
        private bool initialized = false;
        private float spiralAngle = 0f;
        private int spiralCounter = 0;
        
        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 45;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.light = 1.2f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            if (!initialized)
            {
                initialized = true;
                SoundEngine.PlaySound(SoundID.Item74 with { Pitch = 0.3f, Volume = 0.8f }, Projectile.Center);
                
                // Initial burst
                CreateInitialBurst();
            }
            
            // Projectile stays in place
            Projectile.velocity = Vector2.Zero;
            
            // Intense lighting - red and gold
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.3f) * 0.3f + 0.7f;
            Lighting.AddLight(Projectile.Center, 1.2f * pulse, 0.5f * pulse, 0.1f * pulse);
            
            // Create expanding spiral effect
            spiralCounter++;
            spiralAngle += 0.3f;
            
            float progress = 1f - (Projectile.timeLeft / 45f);
            float radius = 20f + progress * 60f;
            
            // Spiral arms - scarlet red and gold
            for (int arm = 0; arm < 3; arm++)
            {
                float armAngle = spiralAngle + (MathHelper.TwoPi / 3f) * arm;
                Vector2 spiralPos = Projectile.Center + new Vector2((float)Math.Cos(armAngle), (float)Math.Sin(armAngle)) * radius;
                
                // Scarlet red particles
                Dust scarlet = Dust.NewDustPerfect(spiralPos, DustID.CrimsonTorch, 
                    Main.rand.NextVector2Circular(2f, 2f), 100, default, 2.0f);
                scarlet.noGravity = true;
                scarlet.fadeIn = 1.2f;
                
                // Gold particles
                if (Main.rand.NextBool(2))
                {
                    Dust gold = Dust.NewDustPerfect(spiralPos, DustID.GoldFlame, 
                        Main.rand.NextVector2Circular(2f, 2f), 100, default, 1.8f);
                    gold.noGravity = true;
                    gold.fadeIn = 1.0f;
                }
            }
            
            // Inner scarlet glow
            if (spiralCounter % 2 == 0)
            {
                float innerRadius = radius * 0.5f;
                for (int i = 0; i < 4; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 pos = Projectile.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(innerRadius);
                    
                    Dust inner = Dust.NewDustPerfect(pos, DustID.CrimsonTorch, Vector2.Zero, 150, default, 1.5f);
                    inner.noGravity = true;
                }
            }
            
            // Expanding ring effect
            if (spiralCounter % 5 == 0)
            {
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi * i / 12f;
                    Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (3f + progress * 5f);
                    
                    int dustType = Main.rand.NextBool() ? DustID.CrimsonTorch : DustID.GoldFlame;
                    Dust ring = Dust.NewDustPerfect(Projectile.Center, dustType, vel, 100, default, 1.5f);
                    ring.noGravity = true;
                }
            }
        }

        private void CreateInitialBurst()
        {
            // Enhanced explosion using ThemedParticles
            ThemedParticles.EroicaImpact(Projectile.Center, 2.5f);
            ThemedParticles.SakuraPetals(Projectile.Center, 15, 50f);
            
            // ★ MUSICAL NOTATION - Heroic chord burst
            ThemedParticles.MusicNoteBurst(Projectile.Center, new Color(255, 215, 0), 6, 4f);
            
            // Large scarlet explosion (reduced count)
            for (int i = 0; i < 25; i++)
            {
                float angle = MathHelper.TwoPi * i / 25f;
                float speed = Main.rand.NextFloat(4f, 12f);
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
                
                Dust burst = Dust.NewDustPerfect(Projectile.Center, DustID.CrimsonTorch, vel, 100, default, 2.5f);
                burst.noGravity = true;
                burst.fadeIn = 1.5f;
            }
            
            // Gold accents
            for (int i = 0; i < 15; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(10f, 10f);
                Dust gold = Dust.NewDustPerfect(Projectile.Center, DustID.GoldFlame, vel, 100, default, 2.2f);
                gold.noGravity = true;
                gold.fadeIn = 1.3f;
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Enhanced final explosion using ThemedParticles
            ThemedParticles.EroicaBloomBurst(Projectile.Center, 3f);
            ThemedParticles.EroicaShockwave(Projectile.Center, 2f);
            
            // ★ MUSICAL FINALE - Hero's symphony
            ThemedParticles.MusicNoteBurst(Projectile.Center, new Color(200, 50, 50), 8, 5f);
            
            // Final explosion burst (reduced count)
            for (int i = 0; i < 30; i++)
            {
                float angle = MathHelper.TwoPi * i / 30f;
                float speed = Main.rand.NextFloat(6f, 15f);
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
                
                int dustType = Main.rand.NextBool() ? DustID.CrimsonTorch : DustID.GoldFlame;
                Dust explosion = Dust.NewDustPerfect(Projectile.Center, dustType, vel, 100, default, 2.8f);
                explosion.noGravity = true;
                explosion.fadeIn = 1.5f;
            }
            
            // Screen shake
            if (Main.LocalPlayer.Distance(Projectile.Center) < 400f)
            {
                Main.LocalPlayer.velocity += Main.rand.NextVector2Circular(0.8f, 0.8f);
            }
            
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.2f, Volume = 0.7f }, Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // All visuals done with particles
            return false;
        }
    }
}
