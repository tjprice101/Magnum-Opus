using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace MagnumOpus.Content.Eroica.Minions
{
    /// <summary>
    /// Sakura Flame Projectile - Black and deep scarlet flaming projectile.
    /// Fired by the Sakura of Fate minion.
    /// </summary>
    public class SakuraFlameProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Flames;
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionShot[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 4;  // Very small hitbox
            Projectile.height = 4;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.scale = 0.15f; // Very small scale
        }

        public override void AI()
        {
            // Rotation follows velocity
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            
            // Black and red flame sparkle trail - smaller particles - NO PURPLE
            for (int i = 0; i < 2; i++)
            {
                // Black smoke (not Shadowflame)
                if (Main.rand.NextBool(2))
                {
                    Vector2 offset = Main.rand.NextVector2Circular(3f, 3f);
                    Dust shadow = Dust.NewDustPerfect(Projectile.Center + offset, DustID.Smoke, 
                        -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.5f, 0.5f), 150, Color.Black, 0.8f);
                    shadow.noGravity = true;
                    shadow.fadeIn = 0.8f;
                }
                
                // Deep red/crimson
                if (Main.rand.NextBool(2))
                {
                    Vector2 offset = Main.rand.NextVector2Circular(3f, 3f);
                    Dust crimson = Dust.NewDustPerfect(Projectile.Center + offset, DustID.CrimsonTorch, 
                        -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.5f, 0.5f), 100, default, 0.8f);
                    crimson.noGravity = true;
                    crimson.fadeIn = 0.8f;
                }
            }
            
            // Black smoke wisps
            if (Main.rand.NextBool(4))
            {
                Dust smoke = Dust.NewDustPerfect(Projectile.Center, DustID.Smoke, 
                    -Projectile.velocity * 0.05f + Main.rand.NextVector2Circular(0.3f, 0.3f), 100, Color.Black, 0.6f);
                smoke.noGravity = true;
            }
            
            // Red ember sparks
            if (Main.rand.NextBool(3))
            {
                Dust ember = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, 
                    -Projectile.velocity * 0.05f + Main.rand.NextVector2Circular(1f, 1f), 100, new Color(180, 30, 30), 0.5f);
                ember.noGravity = true;
            }
            
            // Strong dark red lighting
            Lighting.AddLight(Projectile.Center, 0.5f, 0.1f, 0.15f);
            
            // Slight homing toward enemies
            NPC target = FindNearestEnemy();
            if (target != null)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity.SafeNormalize(Vector2.Zero), toTarget, 0.02f) * Projectile.velocity.Length();
            }
        }
        
        private NPC FindNearestEnemy()
        {
            float closestDist = 400f;
            NPC closest = null;
            
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.CanBeChasedBy(this))
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }
            
            return closest;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Explosion on hit - black and red burst
            CreateHitExplosion(target.Center);
            
            // Sound
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.3f, Pitch = -0.3f }, target.Center);
        }
        
        private void CreateHitExplosion(Vector2 position)
        {
            // Outer ring - black smoke (NO purple)
            for (int i = 0; i < 15; i++)
            {
                float angle = MathHelper.TwoPi * i / 15f;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(4f, 8f);
                Dust dust = Dust.NewDustPerfect(position, DustID.Smoke, vel, 150, Color.Black, 2f);
                dust.noGravity = true;
                dust.fadeIn = 1.3f;
            }
            
            // Inner ring - deep crimson/scarlet
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(3f, 6f);
                Dust dust = Dust.NewDustPerfect(position, DustID.CrimsonTorch, vel, 100, default, 1.8f);
                dust.noGravity = true;
                dust.fadeIn = 1.2f;
            }
            
            // Random burst - smoke and crimson
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                int dustType = Main.rand.NextBool() ? DustID.Smoke : DustID.CrimsonTorch;
                Color dustColor = Main.rand.NextBool() ? Color.Black : default;
                Dust dust = Dust.NewDustPerfect(position, dustType, vel, 100, dustColor, 1.5f);
                dust.noGravity = true;
            }
            
            // Black smoke puff
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Dust smoke = Dust.NewDustPerfect(position, DustID.Smoke, vel, 100, Color.Black, 1.3f);
                smoke.noGravity = true;
            }
            
            // Strong dark lighting
            Lighting.AddLight(position, 0.7f, 0.2f, 0.25f);
        }
        
        public override void OnKill(int timeLeft)
        {
            // Death puff - NO purple
            for (int i = 0; i < 10; i++)
            {
                int dustType = Main.rand.NextBool() ? DustID.Smoke : DustID.CrimsonTorch;
                Color dustColor = Main.rand.NextBool() ? Color.Black : default;
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, dustType, vel, 100, dustColor, 1.3f);
                dust.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            // Dark red outer glow
            Color redGlow = new Color(150, 30, 40, 0) * 0.6f;
            for (int i = 0; i < 6; i++)
            {
                Vector2 offset = new Vector2(4f, 0f).RotatedBy(i * MathHelper.TwoPi / 6f + Main.GameUpdateCount * 0.1f);
                Main.EntitySpriteDraw(texture, drawPos + offset, null, redGlow, Projectile.rotation, origin, 2f, SpriteEffects.None, 0);
            }
            
            // Black/shadow inner glow
            Color shadowGlow = new Color(60, 20, 60, 0) * 0.5f;
            for (int i = 0; i < 6; i++)
            {
                Vector2 offset = new Vector2(2f, 0f).RotatedBy(i * MathHelper.TwoPi / 6f + Main.GameUpdateCount * 0.15f);
                Main.EntitySpriteDraw(texture, drawPos + offset, null, shadowGlow, Projectile.rotation, origin, 1.5f, SpriteEffects.None, 0);
            }
            
            // Core - dark red/black blend
            Color coreColor = new Color(180, 50, 60);
            Main.EntitySpriteDraw(texture, drawPos, null, coreColor, Projectile.rotation, origin, 1.2f, SpriteEffects.None, 0);
            
            // Trail
            for (int i = 0; i < Projectile.oldPos.Length && i < 8; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero)
                    continue;
                    
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                float progress = 1f - (i / 8f);
                
                // Dark red trail
                Color trailRed = new Color(140, 30, 40) * progress * 0.5f;
                trailRed.A = 0;
                Main.EntitySpriteDraw(texture, trailPos, null, trailRed, Projectile.rotation, origin, progress * 1.5f, SpriteEffects.None, 0);
                
                // Black/shadow trail
                Color trailShadow = new Color(50, 20, 50) * progress * 0.3f;
                trailShadow.A = 0;
                Main.EntitySpriteDraw(texture, trailPos, null, trailShadow, Projectile.rotation, origin, progress * 1.2f, SpriteEffects.None, 0);
            }
            
            return false;
        }
    }
}
