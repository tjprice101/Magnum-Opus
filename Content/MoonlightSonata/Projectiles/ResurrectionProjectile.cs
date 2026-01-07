using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;

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
            
            // Outer purple glow particles
            if (Main.rand.NextBool(3))
            {
                Vector2 offset = Main.rand.NextVector2Circular(12f, 12f);
                Dust glow = Dust.NewDustPerfect(Projectile.Center + offset, DustID.Shadowflame, 
                    -Projectile.velocity * 0.05f, 150, default, 1.5f);
                glow.noGravity = true;
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
                    
                    // Ricochet visual - sparkle burst
                    for (int i = 0; i < 10; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 10f;
                        Vector2 sparkVel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 4f;
                        int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.SilverCoin;
                        Dust spark = Dust.NewDustPerfect(Projectile.Center, dustType, sparkVel, 0, default, 1.3f);
                        spark.noGravity = true;
                    }
                }
            }
        }

        private void CreateRadialExplosion(Vector2 position)
        {
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
            // Draw trail
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;
            
            // Draw afterimages
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                
                float progress = (float)i / Projectile.oldPos.Length;
                Color trailColor = Color.Lerp(new Color(150, 80, 200, 150), new Color(80, 40, 120, 0), progress);
                float scale = Projectile.scale * (1f - progress * 0.5f);
                
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Main.EntitySpriteDraw(texture, drawPos, null, trailColor, Projectile.oldRot[i], origin, scale, SpriteEffects.None, 0);
            }
            
            // Draw main projectile with glow
            Color glowColor = new Color(180, 100, 255, 0) * 0.6f;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(3f, 0f).RotatedBy(i * MathHelper.PiOver2);
                Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + offset, null, glowColor, Projectile.rotation, origin, Projectile.scale * 1.1f, SpriteEffects.None, 0);
            }
            
            // Main sprite
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            
            return false;
        }
    }
}
