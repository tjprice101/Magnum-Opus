using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.MoonlightSonata.Projectiles
{
    /// <summary>
    /// Resurrection Projectile - The devastating bullet from Resurrection of the Moon.
    /// Ricochets 10 times very quickly to nearby enemies.
    /// Creates a radial dark purple to white explosion on each hit.
    /// </summary>
    public class ResurrectionProjectile : ModProjectile
    {
        private int ricochetCount = 0;
        private const int MaxRicochets = 10;
        private float pulseTimer = 0f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 12;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = MaxRicochets + 1; // Can hit once per ricochet plus initial
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 2; // Fast movement
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            pulseTimer += 0.2f;
            
            // Pulsing glow scale
            float pulseScale = 1f + (float)Math.Sin(pulseTimer) * 0.2f;
            
            // Use new themed particle trail
            ThemedParticles.MoonlightTrail(Projectile.Center, Projectile.velocity);
            
            // Main purple core trail
            for (int i = 0; i < 2; i++)
            {
                Vector2 trailOffset = Main.rand.NextVector2Circular(4f, 4f);
                Dust core = Dust.NewDustPerfect(Projectile.Center + trailOffset, DustID.PurpleTorch, 
                    -Projectile.velocity * 0.1f, 100, default, 2f * pulseScale);
                core.noGravity = true;
                core.fadeIn = 1.5f;
            }
            
            // White center sparkle
            if (Main.rand.NextBool(2))
            {
                Dust white = Dust.NewDustPerfect(Projectile.Center, DustID.SilverCoin, 
                    Main.rand.NextVector2Circular(1f, 1f), 0, Color.White, 1.2f);
                white.noGravity = true;
            }
            
            // Lighting
            Lighting.AddLight(Projectile.Center, 0.6f, 0.3f, 0.8f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Musical Dissonance debuff
            target.AddBuff(ModContent.BuffType<Debuffs.MusicsDissonance>(), 300); // 5 seconds
            
            // Create the radial explosion effect (dark purple to white gradient)
            CreateRadialExplosion(target.Center);
            
            // Attempt to ricochet to another enemy
            if (ricochetCount < MaxRicochets)
            {
                NPC newTarget = FindNearestEnemy(target.Center, 800f, target.whoAmI);
                if (newTarget != null)
                {
                    ricochetCount++;
                    
                    // Redirect projectile toward new target
                    Vector2 direction = (newTarget.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    float speed = Projectile.velocity.Length();
                    if (speed < 20f) speed = 20f; // Maintain fast speed
                    Projectile.velocity = direction * speed;
                    
                    // Reset time left for continued flight
                    Projectile.timeLeft = Math.Max(Projectile.timeLeft, 60);
                    
                    // Ricochet sound - escalating pitch
                    float pitch = -0.3f + (ricochetCount * 0.1f);
                    SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.5f, Pitch = pitch }, Projectile.Center);
                    
                    // Ricochet visual - Moonlight-themed fractal lightning to new target
                    MagnumVFX.DrawMoonlightLightning(Projectile.Center, newTarget.Center, 8, 25f, 2, 0.3f);
                    
                    // Enhanced sparkle burst with new particles
                    ThemedParticles.MoonlightSparks(Projectile.Center, (newTarget.Center - Projectile.Center).SafeNormalize(Vector2.UnitX), 8, 6f);
                    ThemedParticles.MoonlightSparkles(Projectile.Center, 6, 20f);
                    
                    // Musical burst for the ricochet
                    MagnumVFX.CreateMusicalBurst(Projectile.Center, new Color(150, 80, 200), Color.White, 1);
                }
            }
        }

        private void CreateRadialExplosion(Vector2 position)
        {
            // Use new themed particle system for enhanced explosion
            ThemedParticles.MoonlightImpact(position, 1.2f);
            
            // Resurrection radial burst - ascending ethereal rings using GlowingHalos[3]
            var outerHalo = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.GlowingHalos[3], position, Vector2.Zero,
                CustomParticleSystem.MoonlightColors.Silver, 0.9f, 40, 0.008f, true, true).WithScaleVelocity(0.02f);
            CustomParticleSystem.SpawnParticle(outerHalo);
            var innerHalo = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.GlowingHalos[3], position, Vector2.Zero,
                CustomParticleSystem.MoonlightColors.Violet, 0.5f, 35, 0.01f, true, true).WithScaleVelocity(0.015f);
            CustomParticleSystem.SpawnParticle(innerHalo);
            // Use SoftGlows[2] for ethereal glow
            var etherealGlow = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.SoftGlows[2], position, Vector2.Zero,
                new Color(160, 120, 220), 1.4f, 40, 0f, true, false);
            CustomParticleSystem.SpawnParticle(etherealGlow);
            CustomParticles.MoonlightMusicNotes(position, 5);
            
            // Radial explosion with gradient from dark purple center to white edges
            int ringCount = 4;
            
            for (int ring = 0; ring < ringCount; ring++)
            {
                float radius = 20f + ring * 25f;
                int particlesInRing = 12 + ring * 6;
                
                // Calculate color gradient - dark purple at center, white at edges
                float t = (float)ring / (ringCount - 1);
                Color ringColor = Color.Lerp(new Color(80, 20, 120), Color.White, t);
                
                for (int i = 0; i < particlesInRing; i++)
                {
                    float angle = MathHelper.TwoPi * i / particlesInRing + Main.rand.NextFloat(-0.2f, 0.2f);
                    Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                    Vector2 velocity = offset.SafeNormalize(Vector2.Zero) * (3f + ring * 1.5f);
                    
                    // Select dust type based on ring (inner = purple, outer = white/ice)
                    int dustType;
                    if (ring == 0)
                        dustType = DustID.Shadowflame;
                    else if (ring == 1)
                        dustType = DustID.PurpleTorch;
                    else if (ring == 2)
                        dustType = DustID.PinkTorch;
                    else
                        dustType = DustID.IceTorch;
                    
                    Dust dust = Dust.NewDustPerfect(position + offset * 0.3f, dustType, velocity, 100, default, 2f - ring * 0.3f);
                    dust.noGravity = true;
                    dust.fadeIn = 1.2f;
                }
            }
            
            // Central white burst
            for (int i = 0; i < 15; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(8f, 8f);
                Dust spark = Dust.NewDustPerfect(position, DustID.SilverCoin, sparkVel, 0, Color.White, 1.5f);
                spark.noGravity = true;
            }
            
            // Deep purple core
            for (int i = 0; i < 12; i++)
            {
                Vector2 coreVel = Main.rand.NextVector2Circular(4f, 4f);
                Dust core = Dust.NewDustPerfect(position, DustID.Shadowflame, coreVel, 100, default, 2.2f);
                core.noGravity = true;
            }
            
            // Explosion sound
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.5f, Pitch = 0.3f }, position);
        }

        private NPC FindNearestEnemy(Vector2 position, float range, int excludeWhoAmI)
        {
            NPC closest = null;
            float closestDist = range;
            
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.whoAmI != excludeWhoAmI && npc.CanBeChasedBy(Projectile) && 
                    Collision.CanHitLine(position, 1, 1, npc.position, npc.width, npc.height))
                {
                    float dist = Vector2.Distance(position, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }
            
            return closest;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // Create smaller explosion on wall hit
            CreateWallHitEffect(Projectile.Center);
            return true; // Kill projectile
        }

        private void CreateWallHitEffect(Vector2 position)
        {
            // Smaller radial burst on tile collision
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(3f, 6f);
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch;
                Dust dust = Dust.NewDustPerfect(position, dustType, velocity, 100, default, 1.5f);
                dust.noGravity = true;
            }
            
            // White sparks
            for (int i = 0; i < 8; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(5f, 5f);
                Dust spark = Dust.NewDustPerfect(position, DustID.SilverCoin, sparkVel, 0, Color.White, 1f);
                spark.noGravity = true;
            }
            
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.7f, Pitch = 0.2f }, position);
        }

        public override void OnKill(int timeLeft)
        {
            // Final burst effect
            for (int i = 0; i < 15; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(4f, 4f);
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.SilverCoin;
                Dust dust = Dust.NewDustPerfect(Projectile.Center, dustType, velocity, 100, default, 1.3f);
                dust.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;
            
            // Switch to additive blending for glow
            MagnumVFX.BeginAdditiveBlend(spriteBatch);
            
            // Draw enhanced glowing trail with moonlight gradient
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                
                float progress = (float)i / Projectile.oldPos.Length;
                // Gradient from bright purple to dark purple with white center
                Color trailColor = Color.Lerp(new Color(200, 150, 255), new Color(80, 40, 120), progress);
                trailColor *= (1f - progress) * 0.9f;
                float scale = Projectile.scale * (1f - progress * 0.4f);
                
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                
                // Outer glow trail
                spriteBatch.Draw(texture, drawPos, null, trailColor * 0.5f, Projectile.oldRot[i], origin, scale * 1.5f, SpriteEffects.None, 0);
                // Core trail
                spriteBatch.Draw(texture, drawPos, null, trailColor, Projectile.oldRot[i], origin, scale, SpriteEffects.None, 0);
            }
            
            // Draw main projectile with pulsing glow
            float pulse = MagnumVFX.GetPulse(0.15f, 0.8f, 1.2f);
            Vector2 mainPos = Projectile.Center - Main.screenPosition;
            
            // Outer glow
            spriteBatch.Draw(texture, mainPos, null, new Color(150, 80, 200) * 0.4f, Projectile.rotation, origin, Projectile.scale * 2f * pulse, SpriteEffects.None, 0);
            // Mid glow
            spriteBatch.Draw(texture, mainPos, null, new Color(200, 150, 255) * 0.6f, Projectile.rotation, origin, Projectile.scale * 1.4f * pulse, SpriteEffects.None, 0);
            // White core
            spriteBatch.Draw(texture, mainPos, null, Color.White * 0.8f, Projectile.rotation, origin, Projectile.scale * 0.8f, SpriteEffects.None, 0);
            
            MagnumVFX.EndAdditiveBlend(spriteBatch);
            
            // Main sprite (drawn normally)
            Main.EntitySpriteDraw(texture, mainPos, null, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            
            return false;
        }
    }
}
