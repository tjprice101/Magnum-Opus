using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.MoonlightSonata.Projectiles
{
    /// <summary>
    /// Goliath Moonlight Beam - A small, fast dark purple beam that ricochets.
    /// Compact beam-like projectile with purple/blue effects.
    /// </summary>
    public class GoliathMoonlightBeam : ModProjectile
    {
        // Custom texture - no vanilla textures allowed
        public override string Texture => "MagnumOpus/Assets/Particles/MagicSparklField6";
        
        private const int MaxRicochets = 10;
        private const float RicochetRange = 500f;
        private const float BeamSpeed = 20f;
        
        private int ricochetCount = 0;
        private int lastHitNPC = -1;
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionShot[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = MaxRicochets + 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 3; // Very fast
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            // Rotation follows velocity
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Enhanced trail using ThemedParticles
            ThemedParticles.MoonlightTrail(Projectile.Center, Projectile.velocity);
            
            // Additional beam trail
            for (int i = 0; i < 2; i++)
            {
                int dustType = Main.rand.NextBool(3) ? DustID.IceTorch : DustID.PurpleTorch;
                Vector2 offset = Main.rand.NextVector2Circular(4f, 4f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, dustType, -Projectile.velocity * 0.08f, 0, default, 1.8f);
                dust.noGravity = true;
                dust.fadeIn = 1.2f;
            }
            
            // Strong lighting
            Lighting.AddLight(Projectile.Center, 0.5f, 0.25f, 0.7f);
            
            // Slight homing after a bit
            if (Projectile.timeLeft < 250)
            {
                NPC target = FindNearestEnemy();
                if (target != null && target.whoAmI != lastHitNPC)
                {
                    Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity.SafeNormalize(Vector2.Zero), toTarget, 0.03f) * Projectile.velocity.Length();
                }
            }
        }
        
        private NPC FindNearestEnemy()
        {
            float closestDist = RicochetRange;
            NPC closest = null;
            
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.CanBeChasedBy(this) && npc.whoAmI != lastHitNPC)
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
            // Apply debuff
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 180);
            
            // HEAL THE PLAYER - 10 health per hit
            Player owner = Main.player[Projectile.owner];
            if (owner != null && owner.active)
            {
                int healAmount = 10;
                owner.statLife = Math.Min(owner.statLife + healAmount, owner.statLifeMax2);
                owner.HealEffect(healAmount, true);
            }
            
            // === SIGNATURE FRACTAL FLARE BURST ===
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 flareOffset = angle.ToRotationVector2() * 30f;
                float progress = (float)i / 6f;
                Color fractalColor = Color.Lerp(new Color(75, 0, 130), new Color(135, 206, 250), progress);
                CustomParticles.GenericFlare(target.Center + flareOffset, fractalColor, 0.45f, 18);
            }
            
            // Music notes on hit
            ThemedParticles.MoonlightMusicNotes(target.Center, 4, 30f);
            
            // EXPLOSION EFFECTS!
            CreateHitExplosion(target.Center);
            
            // Sound
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.4f, Pitch = 0.8f }, target.Center);
            
            // Ricochet to next target
            ricochetCount++;
            lastHitNPC = target.whoAmI;
            
            if (ricochetCount <= MaxRicochets)
            {
                NPC nextTarget = FindNearestEnemy();
                if (nextTarget != null)
                {
                    // Ricochet toward next enemy
                    Vector2 newDirection = (nextTarget.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = newDirection * BeamSpeed;
                    Projectile.netUpdate = true;
                    
                    // Small ricochet effect
                    for (int i = 0; i < 6; i++)
                    {
                        Vector2 vel = newDirection.RotatedByRandom(0.5f) * Main.rand.NextFloat(2f, 5f);
                        int dustType = Main.rand.NextBool() ? DustID.IceTorch : DustID.PurpleTorch;
                        Dust dust = Dust.NewDustPerfect(Projectile.Center, dustType, vel, 100, default, 0.9f);
                        dust.noGravity = true;
                    }
                    
                    // Soft ricochet sound
                    SoundEngine.PlaySound(SoundID.Item12 with { Volume = 0.25f, Pitch = 0.8f + ricochetCount * 0.05f }, Projectile.Center);
                }
            }
        }
        
        private void CreateHitExplosion(Vector2 position)
        {
            // Enhanced explosion using ThemedParticles
            ThemedParticles.MoonlightImpact(position, 1.5f);
            
            // Outer ring - deep purple (reduced count - ThemedParticles handles additional visuals)
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(6f, 12f);
                Dust dust = Dust.NewDustPerfect(position, DustID.PurpleTorch, vel, 0, default, 2.5f);
                dust.noGravity = true;
                dust.fadeIn = 1.5f;
            }
            
            // Inner ring - light blue
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(4f, 9f);
                Dust dust = Dust.NewDustPerfect(position, DustID.IceTorch, vel, 0, default, 2.2f);
                dust.noGravity = true;
                dust.fadeIn = 1.3f;
            }
            
            // Electric sparks
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                Dust electric = Dust.NewDustPerfect(position, DustID.Electric, vel, 100, Color.LightBlue, 1.2f);
                electric.noGravity = true;
            }
            
            // Shadowflame accent
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Dust shadow = Dust.NewDustPerfect(position, DustID.Shadowflame, vel, 100, default, 1.5f);
                shadow.noGravity = true;
            }
            
            // Strong lighting
            Lighting.AddLight(position, 0.8f, 0.4f, 1f);
        }
        
        public override void OnKill(int timeLeft)
        {
            // Small death puff
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(2f, 4f);
                int dustType = Main.rand.NextBool(3) ? DustID.IceTorch : DustID.PurpleTorch;
                Dust dust = Dust.NewDustPerfect(Projectile.Center, dustType, vel, 80, default, 0.9f);
                dust.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // ENHANCED beam drawing - very visible!
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            // Large outer purple glow
            Color purpleGlow = new Color(140, 60, 200, 0) * 0.7f;
            for (int i = 0; i < 6; i++)
            {
                Vector2 offset = new Vector2(5f, 0f).RotatedBy(i * MathHelper.PiOver2 + Main.GameUpdateCount * 0.1f);
                Main.EntitySpriteDraw(texture, drawPos + offset, null, purpleGlow, Projectile.rotation, origin, 2.5f, SpriteEffects.None, 0);
            }
            
            // Medium light blue glow
            Color blueGlow = new Color(120, 180, 255, 0) * 0.6f;
            for (int i = 0; i < 6; i++)
            {
                Vector2 offset = new Vector2(3f, 0f).RotatedBy(i * MathHelper.PiOver2 + Main.GameUpdateCount * 0.15f);
                Main.EntitySpriteDraw(texture, drawPos + offset, null, blueGlow, Projectile.rotation, origin, 2f, SpriteEffects.None, 0);
            }
            
            // Core - bright purple/white
            Color coreColor = new Color(230, 200, 255);
            Main.EntitySpriteDraw(texture, drawPos, null, coreColor, Projectile.rotation, origin, 1.5f, SpriteEffects.None, 0);
            
            // Enhanced trail
            for (int i = 0; i < Projectile.oldPos.Length && i < 10; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero)
                    continue;
                    
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                float progress = 1f - (i / 10f);
                
                // Purple trail
                Color trailPurple = new Color(140, 80, 200) * progress * 0.6f;
                trailPurple.A = 0;
                Main.EntitySpriteDraw(texture, trailPos, null, trailPurple, Projectile.rotation, origin, progress * 2f, SpriteEffects.None, 0);
                
                // Blue trail
                Color trailBlue = new Color(100, 160, 255) * progress * 0.4f;
                trailBlue.A = 0;
                Main.EntitySpriteDraw(texture, trailPos, null, trailBlue, Projectile.rotation, origin, progress * 1.5f, SpriteEffects.None, 0);
            }
            
            return false;
        }
    }
}
