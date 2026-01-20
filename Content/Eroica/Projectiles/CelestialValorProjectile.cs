using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// Projectile fired by Celestial Valor greatsword on swing.
    /// A powerful energy slash with red and gold effects.
    /// Collides with walls and creates gold/red AOE explosions on impact.
    /// </summary>
    public class CelestialValorProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = true; // Now collides with walls
            Projectile.ignoreWater = true;
            Projectile.alpha = 50;
            Projectile.light = 0.7f;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            // Face direction of travel
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Slight homing toward nearby enemies
            float homingRange = 150f;
            float homingStrength = 0.02f;
            
            NPC closestNPC = null;
            float closestDist = homingRange;
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage)
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestNPC = npc;
                    }
                }
            }
            
            if (closestNPC != null)
            {
                Vector2 toTarget = (closestNPC.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), homingStrength);
            }
            
            // Sword arc wave trail - traveling heroic slash
            if (Main.rand.NextBool(4))
            {
                CustomParticles.SwordArcWave(Projectile.Center, Projectile.velocity * 0.15f, CustomParticleSystem.EroicaColors.Gold * 0.8f, 0.35f);
            }
            
            // Trail particles - red and gold (reduced)
            if (Main.rand.NextBool(3))
            {
                int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                Dust trail = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    dustType, 0f, 0f, 100, default, 1.2f);
                trail.noGravity = true;
                trail.velocity = -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1.5f, 1.5f);
            }
            
            // Heroic musical trail - fiery notes (reduced frequency)
            if (Main.rand.NextBool(2))
            {
                ThemedParticles.EroicaMusicTrail(Projectile.Center, Projectile.velocity);
            }
            
            // Custom flare trail (reduced)
            if (Main.rand.NextBool(3))
            {
                CustomParticles.EroicaFlare(Projectile.Center, 0.45f);
            }
            
            // Lighting
            Lighting.AddLight(Projectile.Center, 1f, 0.6f, 0.3f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Create AOE explosion on hit
            CreateAOEExplosion(target.Center);
            
            // === ENHANCED IMPACT WITH MULTI-LAYER BLOOM ===
            // Central flash with proper bloom stacking
            EnhancedParticles.BloomFlare(target.Center, Color.White, 0.7f, 20, 4, 1.0f);
            EnhancedParticles.BloomFlare(target.Center, ThemedParticles.EroicaGold, 0.55f, 18, 3, 0.85f);
            
            // Enhanced Eroica impact with full bloom
            UnifiedVFXBloom.Eroica.ImpactEnhanced(target.Center, 0.8f);
            
            // Enhanced music notes with bloom
            EnhancedThemedParticles.EroicaMusicNotesEnhanced(target.Center, 5, 32f);
            
            // Deal 5% bonus explosion damage to nearby enemies
            int explosionDamage = (int)(damageDone * 0.05f);
            float explosionRadius = 100f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage && npc.whoAmI != target.whoAmI)
                {
                    if (Vector2.Distance(npc.Center, target.Center) <= explosionRadius)
                    {
                        Player player = Main.player[Projectile.owner];
                        npc.SimpleStrikeNPC(explosionDamage, 0, false, 0f, DamageClass.Melee);
                    }
                }
            }
        }
        
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // Create AOE explosion on wall collision
            CreateAOEExplosion(Projectile.Center);
            return true; // Destroy projectile
        }

        private void CreateAOEExplosion(Vector2 position)
        {
            // Large red and gold spiral explosion
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.3f, Volume = 0.5f }, position);
            
            // Magic sparkle field burst - heroic valor explosion
            CustomParticles.MagicSparkleFieldBurst(position, CustomParticleSystem.EroicaColors.Gold, 5, 30f);
            
            // Sword arc burst - radial heroic slashes
            CustomParticles.SwordArcBurst(position, CustomParticleSystem.EroicaColors.Scarlet, 5, 0.4f);
            
            // Golden sun halo
            var sunHalo = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.GlowingHalos[0], position, Vector2.Zero,
                new Color(255, 220, 100), 0.8f, 30, 0.015f, true, true).WithScaleVelocity(0.025f);
            CustomParticleSystem.SpawnParticle(sunHalo);
            
            // Prismatic sparkle radial burst
            CustomParticles.PrismaticSparkleBurst(position, CustomParticleSystem.EroicaColors.Gold, 8);
            
            // Black and red lightning effect
            SpawnLightningEffect(position);
            
            // Dramatic musical burst with clef for explosion!
            ThemedParticles.EroicaMusicalImpact(position, 1.2f, true);
            
            // Outer ring - gold
            for (int i = 0; i < 30; i++)
            {
                float angle = MathHelper.TwoPi * i / 30f;
                float speed = Main.rand.NextFloat(6f, 12f);
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
                
                Dust gold = Dust.NewDustPerfect(position, DustID.GoldFlame, vel, 100, default, 2.5f);
                gold.noGravity = true;
                gold.fadeIn = 1.3f;
            }
            
            // Inner burst - scarlet red
            for (int i = 0; i < 25; i++)
            {
                float angle = MathHelper.TwoPi * i / 25f;
                float speed = Main.rand.NextFloat(4f, 9f);
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
                
                Dust scarlet = Dust.NewDustPerfect(position, DustID.CrimsonTorch, vel, 100, default, 2.2f);
                scarlet.noGravity = true;
                scarlet.fadeIn = 1.2f;
            }
            
            // Fire sparks
            for (int i = 0; i < 15; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
                Dust spark = Dust.NewDustPerfect(position, DustID.Torch, vel, 100, default, 1.8f);
                spark.noGravity = false;
            }
            
            // Light burst
            Lighting.AddLight(position, 1.5f, 0.8f, 0.3f);
        }
        
        private void SpawnLightningEffect(Vector2 position)
        {
            // Create black and red lightning bolts radiating outward
            int boltCount = 4;
            for (int b = 0; b < boltCount; b++)
            {
                float baseAngle = MathHelper.TwoPi * b / boltCount + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 boltStart = position;
                Vector2 boltDirection = new Vector2((float)Math.Cos(baseAngle), (float)Math.Sin(baseAngle));
                
                // Create jagged lightning segments
                int segments = Main.rand.Next(6, 10);
                float segmentLength = Main.rand.NextFloat(15f, 25f);
                
                for (int s = 0; s < segments; s++)
                {
                    // Add randomness to direction for jagged effect
                    float angleOffset = Main.rand.NextFloat(-0.5f, 0.5f);
                    boltDirection = boltDirection.RotatedBy(angleOffset);
                    
                    Vector2 boltEnd = boltStart + boltDirection * segmentLength;
                    
                    // Spawn particles along the segment - alternating black and red
                    int particlesPerSegment = 5;
                    for (int p = 0; p < particlesPerSegment; p++)
                    {
                        float t = (float)p / particlesPerSegment;
                        Vector2 particlePos = Vector2.Lerp(boltStart, boltEnd, t);
                        
                        // Alternate between black smoke and red torch
                        if (Main.rand.NextBool())
                        {
                            // Black lightning
                            Dust black = Dust.NewDustPerfect(particlePos, DustID.Smoke, 
                                Main.rand.NextVector2Circular(1f, 1f), 200, Color.Black, 1.5f);
                            black.noGravity = true;
                        }
                        else
                        {
                            // Red lightning
                            Dust red = Dust.NewDustPerfect(particlePos, DustID.CrimsonTorch, 
                                Main.rand.NextVector2Circular(1f, 1f), 100, default, 1.8f);
                            red.noGravity = true;
                        }
                    }
                    
                    boltStart = boltEnd;
                }
            }
            
            // Lightning sound
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.5f, Volume = 0.4f }, position);
        }

        public override void OnKill(int timeLeft)
        {
            // Death burst if killed by time (not wall collision)
            if (timeLeft > 0)
                return;
                
            for (int i = 0; i < 15; i++)
            {
                int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                Dust burst = Dust.NewDustDirect(Projectile.Center, 1, 1, dustType, 0f, 0f, 100, default, 2f);
                burst.noGravity = true;
                burst.velocity = Main.rand.NextVector2Circular(6f, 6f);
            }
            
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.5f }, Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
            
            // Switch to additive blending for prismatic gem effects
            MagnumVFX.BeginAdditiveBlend(spriteBatch);
            
            // Draw prismatic gem trail using oldPos for brilliant diamond effect
            MagnumVFX.DrawPrismaticGemTrail(spriteBatch, Projectile.oldPos, true, 0.4f, (float)Projectile.timeLeft);
            
            // Draw central prismatic gem at projectile position
            MagnumVFX.DrawEroicaPrismaticGem(spriteBatch, Projectile.Center, 0.7f, 0.9f, (float)Projectile.timeLeft);
            
            MagnumVFX.EndAdditiveBlend(spriteBatch);
            
            // Draw trail
            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + new Vector2(Projectile.width / 2, Projectile.height / 2);
                float progress = (float)(Projectile.oldPos.Length - k) / Projectile.oldPos.Length;
                
                // Red to gold gradient trail
                Color trailColor = Color.Lerp(new Color(200, 50, 30, 80), new Color(255, 220, 100, 120), progress) * progress;
                float scale = Projectile.scale * (0.5f + progress * 0.5f);
                
                spriteBatch.Draw(texture, drawPos, null, trailColor, Projectile.oldRot[k], drawOrigin, scale, SpriteEffects.None, 0f);
            }
            
            // Glow effect
            Color glowColor = new Color(255, 200, 100, 0) * 0.5f;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(4f, 0).RotatedBy(MathHelper.PiOver2 * i);
                spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition + offset, null, glowColor, 
                    Projectile.rotation, drawOrigin, Projectile.scale * 1.2f, SpriteEffects.None, 0f);
            }
            
            // Draw main projectile
            Color mainColor = new Color(255, 240, 220, 220);
            spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, mainColor, 
                Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);

            return false;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 240, 200, 180);
        }
    }
}
