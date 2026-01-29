using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Nachtmusik.Debuffs;

namespace MagnumOpus.Content.Nachtmusik.Projectiles
{
    #region Melee Projectiles
    
    /// <summary>
    /// Nocturnal Executioner's massive spectral blade projectile.
    /// Homes toward enemies and explodes on contact.
    /// </summary>
    public class NocturnalBladeProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private float orbitAngle;
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
            Projectile.extraUpdates = 1;
        }
        
        public override void AI()
        {
            orbitAngle += 0.12f;
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Homing toward enemies
            NPC target = FindClosestEnemy(600f);
            if (target != null)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 18f, 0.06f);
            }
            
            // Trail VFX
            if (Projectile.timeLeft % 2 == 0)
            {
                NachtmusikCosmicVFX.SpawnCelestialCloudTrail(Projectile.Center, Projectile.velocity, 0.6f);
            }
            
            // Orbiting star particles
            if (Projectile.timeLeft % 6 == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    float sparkAngle = orbitAngle + MathHelper.TwoPi * i / 3f;
                    Vector2 sparkPos = Projectile.Center + sparkAngle.ToRotationVector2() * 16f;
                    CustomParticles.GenericFlare(sparkPos, NachtmusikCosmicVFX.Gold, 0.25f, 10);
                }
            }
            
            // Sparkle dust trail
            if (Main.rand.NextBool(2))
            {
                Color trailColor = NachtmusikCosmicVFX.GetCelestialGradient(Main.rand.NextFloat());
                var trail = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f),
                    trailColor * 0.7f, 0.25f, 18, true);
                MagnumParticleHandler.SpawnParticle(trail);
                
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch, -Projectile.velocity * 0.3f, 0, default, 0.9f);
                dust.noGravity = true;
            }
            
            Lighting.AddLight(Projectile.Center, NachtmusikCosmicVFX.Violet.ToVector3() * 0.6f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Celestial Harmony debuff
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 480);
            target.GetGlobalNPC<CelestialHarmonyNPC>().AddStack(target, 2);
            
            // Impact VFX
            NachtmusikCosmicVFX.SpawnCelestialImpact(target.Center, 1.2f);
        }
        
        public override void OnKill(int timeLeft)
        {
            // Death explosion
            NachtmusikCosmicVFX.SpawnCelestialExplosion(Projectile.Center, 0.8f);
            SoundEngine.PlaySound(SoundID.Item62 with { Pitch = 0.3f, Volume = 0.7f }, Projectile.Center);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 origin = tex.Size() / 2f;
            
            // Draw trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Color trailColor = NachtmusikCosmicVFX.GetCelestialGradient(progress) * (1f - progress) * 0.6f;
                float scale = 0.5f * (1f - progress * 0.6f);
                sb.Draw(tex, drawPos, null, trailColor, Projectile.oldRot[i], origin, scale, SpriteEffects.None, 0f);
            }
            
            // Draw core
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.1f + 0.9f;
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, NachtmusikCosmicVFX.DeepPurple * 0.4f, 0f, origin, 0.7f * pulse, SpriteEffects.None, 0f);
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, NachtmusikCosmicVFX.Violet * 0.6f, 0f, origin, 0.5f * pulse, SpriteEffects.None, 0f);
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, NachtmusikCosmicVFX.Gold * 0.7f, 0f, origin, 0.35f * pulse, SpriteEffects.None, 0f);
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, NachtmusikCosmicVFX.StarWhite * 0.9f, 0f, origin, 0.2f * pulse, SpriteEffects.None, 0f);
            
            return false;
        }
        
        private NPC FindClosestEnemy(float range)
        {
            NPC closest = null;
            float closestDist = range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy(Projectile))
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
    }
    
    /// <summary>
    /// Midnight's Crescendo's crescendo wave projectile.
    /// Expands as it travels, dealing more damage at larger sizes.
    /// </summary>
    public class CrescendoWaveProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private float growthFactor = 1f;
        
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
        }
        
        public override void AI()
        {
            // Grow over time
            growthFactor = 1f + (120 - Projectile.timeLeft) / 60f; // Grows up to 3x
            Projectile.scale = growthFactor;
            
            // Slow down as it grows
            Projectile.velocity *= 0.985f;
            
            // Rotation
            Projectile.rotation += 0.08f;
            
            // Trail VFX
            if (Main.rand.NextBool(2))
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f * growthFactor, 20f * growthFactor);
                Color trailColor = NachtmusikCosmicVFX.GetCelestialGradient(Main.rand.NextFloat());
                var trail = new GenericGlowParticle(Projectile.Center + offset, Main.rand.NextVector2Circular(2f, 2f),
                    trailColor * 0.5f, 0.25f * growthFactor, 15, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // Music notes spinning around
            if (Projectile.timeLeft % 8 == 0)
            {
                float noteAngle = Projectile.rotation * 2f;
                Vector2 notePos = Projectile.Center + noteAngle.ToRotationVector2() * 25f * growthFactor;
                ThemedParticles.MusicNote(notePos, noteAngle.ToRotationVector2() * 2f, NachtmusikCosmicVFX.Gold, 0.35f, 20);
            }
            
            Lighting.AddLight(Projectile.Center, NachtmusikCosmicVFX.Violet.ToVector3() * 0.8f * growthFactor);
        }
        
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // More damage at larger sizes
            modifiers.FinalDamage *= growthFactor;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 360);
            target.GetGlobalNPC<CelestialHarmonyNPC>().AddStack(target, 1);
            
            NachtmusikCosmicVFX.SpawnCelestialImpact(target.Center, 0.8f * growthFactor);
        }
        
        public override void OnKill(int timeLeft)
        {
            NachtmusikCosmicVFX.SpawnCelestialExplosion(Projectile.Center, growthFactor * 0.7f);
            NachtmusikCosmicVFX.SpawnMusicNoteBurst(Projectile.Center, 8, 5f * growthFactor);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.15f + 0.85f;
            float scale = growthFactor * pulse;
            
            // Multi-layer glow
            sb.Draw(tex, drawPos, null, NachtmusikCosmicVFX.DeepPurple * 0.3f, Projectile.rotation, origin, scale * 1.2f, SpriteEffects.None, 0f);
            sb.Draw(tex, drawPos, null, NachtmusikCosmicVFX.Violet * 0.5f, Projectile.rotation * 0.8f, origin, scale * 0.9f, SpriteEffects.None, 0f);
            sb.Draw(tex, drawPos, null, NachtmusikCosmicVFX.Gold * 0.6f, Projectile.rotation * 0.5f, origin, scale * 0.6f, SpriteEffects.None, 0f);
            sb.Draw(tex, drawPos, null, NachtmusikCosmicVFX.StarWhite * 0.8f, 0f, origin, scale * 0.35f, SpriteEffects.None, 0f);
            
            return false;
        }
    }
    
    #endregion
    
    #region Ranged Projectiles
    
    /// <summary>
    /// Constellation Piercer's star bolt that chains to nearby enemies.
    /// </summary>
    public class ConstellationBoltProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private int chainCount = 0;
        private const int MaxChains = 4;
        private List<int> hitEnemies = new List<int>();
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.extraUpdates = 1;
            Projectile.arrow = true;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            
            // Light trail
            NachtmusikCosmicVFX.SpawnRadiantBeamTrail(Projectile.Center, Projectile.velocity, 0.5f);
            
            // Star sparkles
            if (Main.rand.NextBool(3))
            {
                Dust star = Dust.NewDustPerfect(Projectile.Center, DustID.GoldFlame, -Projectile.velocity * 0.2f, 0, default, 0.8f);
                star.noGravity = true;
            }
            
            Lighting.AddLight(Projectile.Center, NachtmusikCosmicVFX.Gold.ToVector3() * 0.5f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 300);
            target.GetGlobalNPC<CelestialHarmonyNPC>().AddStack(target, 1);
            
            NachtmusikCosmicVFX.SpawnCelestialImpact(target.Center, 0.7f);
            
            hitEnemies.Add(target.whoAmI);
            
            // Chain to next target
            if (chainCount < MaxChains)
            {
                NPC nextTarget = FindNextChainTarget(target.Center, 300f);
                if (nextTarget != null)
                {
                    chainCount++;
                    Vector2 toNext = (nextTarget.Center - target.Center).SafeNormalize(Vector2.UnitX);
                    Projectile.velocity = toNext * Projectile.velocity.Length() * 0.95f;
                    Projectile.Center = target.Center;
                    
                    // Chain lightning VFX
                    for (int i = 0; i < 6; i++)
                    {
                        Vector2 linePos = Vector2.Lerp(target.Center, target.Center + toNext * 50f, i / 6f);
                        var line = new GenericGlowParticle(linePos, Vector2.Zero, NachtmusikCosmicVFX.Gold, 0.2f, 10, true);
                        MagnumParticleHandler.SpawnParticle(line);
                    }
                }
            }
        }
        
        public override void OnKill(int timeLeft)
        {
            NachtmusikCosmicVFX.SpawnCelestialImpact(Projectile.Center, 0.6f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 origin = tex.Size() / 2f;
            
            // Trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Color trailColor = Color.Lerp(NachtmusikCosmicVFX.Gold, NachtmusikCosmicVFX.StarWhite, progress) * (1f - progress) * 0.5f;
                sb.Draw(tex, drawPos, null, trailColor, 0f, origin, 0.3f * (1f - progress * 0.5f), SpriteEffects.None, 0f);
            }
            
            // Core
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, NachtmusikCosmicVFX.Gold * 0.7f, 0f, origin, 0.4f, SpriteEffects.None, 0f);
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, NachtmusikCosmicVFX.StarWhite * 0.9f, 0f, origin, 0.25f, SpriteEffects.None, 0f);
            
            return false;
        }
        
        private NPC FindNextChainTarget(Vector2 from, float range)
        {
            NPC closest = null;
            float closestDist = range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (hitEnemies.Contains(i)) continue;
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy(Projectile))
                {
                    float dist = Vector2.Distance(from, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }
            return closest;
        }
    }
    
    /// <summary>
    /// Nebula's Whisper's ethereal arrow that splits on hit.
    /// </summary>
    public class NebulaArrowProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private int splitCount = 0;
        
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.arrow = true;
            Projectile.extraUpdates = 1;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Nebula cloud trail
            if (Main.rand.NextBool(2))
            {
                Color nebulaColor = Color.Lerp(NachtmusikCosmicVFX.NebulaPink, NachtmusikCosmicVFX.Violet, Main.rand.NextFloat());
                var cloud = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    nebulaColor * 0.5f, 0.22f, 18, true);
                MagnumParticleHandler.SpawnParticle(cloud);
            }
            
            Lighting.AddLight(Projectile.Center, NachtmusikCosmicVFX.NebulaPink.ToVector3() * 0.4f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 300);
            
            // Split into smaller projectiles
            if (splitCount == 0 && Projectile.ai[0] == 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    float angle = MathHelper.TwoPi * i / 4f + Main.rand.NextFloat(-0.2f, 0.2f);
                    Vector2 splitVel = angle.ToRotationVector2() * 10f;
                    int split = Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, splitVel,
                        Projectile.type, Projectile.damage / 2, Projectile.knockBack / 2, Projectile.owner);
                    if (split >= 0 && split < Main.maxProjectiles)
                    {
                        Main.projectile[split].ai[0] = 1; // Mark as split
                        Main.projectile[split].penetrate = 1;
                    }
                }
            }
            
            NachtmusikCosmicVFX.SpawnCelestialImpact(target.Center, 0.8f);
        }
        
        public override void OnKill(int timeLeft)
        {
            NachtmusikCosmicVFX.SpawnCelestialImpact(Projectile.Center, 0.5f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 origin = tex.Size() / 2f;
            
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, NachtmusikCosmicVFX.NebulaPink * 0.6f, 0f, origin, 0.4f, SpriteEffects.None, 0f);
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, NachtmusikCosmicVFX.StarWhite * 0.8f, 0f, origin, 0.2f, SpriteEffects.None, 0f);
            
            return false;
        }
    }
    
    /// <summary>
    /// Serenade of Distant Stars' homing star projectile.
    /// </summary>
    public class SerenadeStarProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }
        
        public override void AI()
        {
            Projectile.rotation += 0.15f;
            
            // Strong homing after initial delay
            if (Projectile.timeLeft < 270)
            {
                NPC target = FindClosestEnemy(800f);
                if (target != null)
                {
                    Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 16f, 0.08f);
                }
            }
            
            // Star trail
            if (Main.rand.NextBool(2))
            {
                var star = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f,
                    NachtmusikCosmicVFX.StarWhite * 0.6f, 0.2f, 15, true);
                MagnumParticleHandler.SpawnParticle(star);
            }
            
            Lighting.AddLight(Projectile.Center, NachtmusikCosmicVFX.StarWhite.ToVector3() * 0.6f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 360);
            target.GetGlobalNPC<CelestialHarmonyNPC>().AddStack(target, 1);
            NachtmusikCosmicVFX.SpawnCelestialImpact(target.Center, 0.9f);
        }
        
        public override void OnKill(int timeLeft)
        {
            NachtmusikCosmicVFX.SpawnCelestialExplosion(Projectile.Center, 0.6f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 origin = tex.Size() / 2f;
            
            // Star trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Color trailColor = NachtmusikCosmicVFX.GetCelestialGradient(progress) * (1f - progress) * 0.5f;
                sb.Draw(tex, drawPos, null, trailColor, Projectile.oldRot[i], origin, 0.35f * (1f - progress * 0.5f), SpriteEffects.None, 0f);
            }
            
            // Star core
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.15f + 0.85f;
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, NachtmusikCosmicVFX.Gold * 0.5f, Projectile.rotation, origin, 0.5f * pulse, SpriteEffects.None, 0f);
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, NachtmusikCosmicVFX.StarWhite * 0.9f, Projectile.rotation * 0.5f, origin, 0.3f * pulse, SpriteEffects.None, 0f);
            
            return false;
        }
        
        private NPC FindClosestEnemy(float range)
        {
            NPC closest = null;
            float closestDist = range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy(Projectile))
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
    }
    
    #endregion
    
    #region Magic Projectiles
    
    /// <summary>
    /// Starweaver's Grimoire's cosmic orb projectile.
    /// Creates mini-explosions along its path.
    /// </summary>
    public class StarweaverOrbProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private int explosionTimer = 0;
        
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }
        
        public override void AI()
        {
            Projectile.rotation += 0.08f;
            explosionTimer++;
            
            // Slow homing
            NPC target = FindClosestEnemy(600f);
            if (target != null)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 12f, 0.04f);
            }
            
            // Periodic mini-explosions
            if (explosionTimer % 20 == 0)
            {
                // Mini star burst
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f;
                    Vector2 burstPos = Projectile.Center + angle.ToRotationVector2() * 20f;
                    CustomParticles.GenericFlare(burstPos, NachtmusikCosmicVFX.Violet, 0.3f, 12);
                }
                
                // Damage enemies in radius
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && npc.CanBeChasedBy(Projectile) && Vector2.Distance(npc.Center, Projectile.Center) < 80f)
                    {
                        npc.SimpleStrikeNPC(Projectile.damage / 3, 0, false, 0f, null, false, 0, false);
                    }
                }
            }
            
            // Ambient particles
            if (Main.rand.NextBool(2))
            {
                Color orbColor = NachtmusikCosmicVFX.GetCelestialGradient(Main.rand.NextFloat());
                var particle = new GenericGlowParticle(Projectile.Center + Main.rand.NextVector2Circular(15f, 15f),
                    Main.rand.NextVector2Circular(1f, 1f), orbColor * 0.6f, 0.2f, 15, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }
            
            Lighting.AddLight(Projectile.Center, NachtmusikCosmicVFX.Violet.ToVector3() * 0.7f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 420);
            target.GetGlobalNPC<CelestialHarmonyNPC>().AddStack(target, 2);
            NachtmusikCosmicVFX.SpawnCelestialImpact(target.Center, 1f);
        }
        
        public override void OnKill(int timeLeft)
        {
            NachtmusikCosmicVFX.SpawnCelestialExplosion(Projectile.Center, 1.2f);
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 0.7f }, Projectile.Center);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.2f + 0.8f;
            
            // Multi-layer orb
            sb.Draw(tex, drawPos, null, NachtmusikCosmicVFX.DeepPurple * 0.4f, Projectile.rotation, origin, 0.8f * pulse, SpriteEffects.None, 0f);
            sb.Draw(tex, drawPos, null, NachtmusikCosmicVFX.Violet * 0.6f, -Projectile.rotation * 0.5f, origin, 0.55f * pulse, SpriteEffects.None, 0f);
            sb.Draw(tex, drawPos, null, NachtmusikCosmicVFX.Gold * 0.7f, Projectile.rotation * 0.3f, origin, 0.35f * pulse, SpriteEffects.None, 0f);
            sb.Draw(tex, drawPos, null, NachtmusikCosmicVFX.StarWhite, 0f, origin, 0.2f * pulse, SpriteEffects.None, 0f);
            
            return false;
        }
        
        private NPC FindClosestEnemy(float range)
        {
            NPC closest = null;
            float closestDist = range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy(Projectile))
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
    }
    
    /// <summary>
    /// Requiem of the Cosmos' cosmic beam that pierces everything.
    /// </summary>
    public class CosmicRequiemBeamProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 6;
            Projectile.extraUpdates = 2;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Intense beam trail
            if (Main.rand.NextBool())
            {
                Color beamColor = NachtmusikCosmicVFX.GetCelestialGradient(Main.rand.NextFloat());
                var beam = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.05f + Main.rand.NextVector2Circular(1f, 1f),
                    beamColor * 0.7f, 0.3f, 12, true);
                MagnumParticleHandler.SpawnParticle(beam);
            }
            
            // Electric dust
            if (Main.rand.NextBool(3))
            {
                Dust electric = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch, Main.rand.NextVector2Circular(2f, 2f), 0, default, 1f);
                electric.noGravity = true;
            }
            
            Lighting.AddLight(Projectile.Center, NachtmusikCosmicVFX.Violet.ToVector3());
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 480);
            target.GetGlobalNPC<CelestialHarmonyNPC>().AddStack(target, 3);
            
            // Intense hit VFX
            CustomParticles.GenericFlare(target.Center, NachtmusikCosmicVFX.StarWhite, 0.7f, 12);
            NachtmusikCosmicVFX.SpawnGlyphBurst(target.Center, 3, 4f, 0.3f);
        }
        
        public override void OnKill(int timeLeft)
        {
            NachtmusikCosmicVFX.SpawnCelestialExplosion(Projectile.Center, 0.8f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 origin = tex.Size() / 2f;
            
            // Long trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Color trailColor = NachtmusikCosmicVFX.GetCelestialGradient(progress) * (1f - progress) * 0.7f;
                float scale = 0.5f * (1f - progress * 0.7f);
                sb.Draw(tex, drawPos, null, trailColor, Projectile.oldRot[i], origin, scale, SpriteEffects.None, 0f);
            }
            
            // Beam core
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.25f) * 0.1f + 0.9f;
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, NachtmusikCosmicVFX.Violet * 0.6f, Projectile.rotation, origin, 0.6f * pulse, SpriteEffects.None, 0f);
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, NachtmusikCosmicVFX.StarWhite, Projectile.rotation, origin, 0.35f * pulse, SpriteEffects.None, 0f);
            
            return false;
        }
    }
    
    /// <summary>
    /// Twilight Severance's fast dimension-cutting slash projectile.
    /// Ultra-fast, short-range, high damage slashes.
    /// </summary>
    public class TwilightSlashProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private bool isDimensionSever => Projectile.ai[0] == 1f;
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 4;
            Projectile.timeLeft = 40; // Short-lived fast slash
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
            Projectile.extraUpdates = 2;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Faster for dimension sever
            if (isDimensionSever)
            {
                Projectile.velocity *= 1.02f;
                
                // More intense trail
                if (Main.rand.NextBool(2))
                {
                    Color trailColor = Color.Lerp(NachtmusikCosmicVFX.Gold, NachtmusikCosmicVFX.StarWhite, Main.rand.NextFloat());
                    var glow = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(2f, 2f),
                        trailColor, 0.4f, 15, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
            }
            else
            {
                // Normal slash trail
                if (Main.rand.NextBool(3))
                {
                    Color trailColor = NachtmusikCosmicVFX.GetCelestialGradient(Main.rand.NextFloat());
                    var glow = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.05f,
                        trailColor * 0.7f, 0.25f, 10, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
            }
            
            Lighting.AddLight(Projectile.Center, NachtmusikCosmicVFX.Violet.ToVector3() * (isDimensionSever ? 0.6f : 0.35f));
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Celestial Harmony
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 240);
            if (target.TryGetGlobalNPC(out CelestialHarmonyNPC harmonyNPC))
            {
                harmonyNPC.AddStack(target, isDimensionSever ? 2 : 1);
            }
            
            // Hit VFX
            if (isDimensionSever)
            {
                NachtmusikCosmicVFX.SpawnCelestialImpact(target.Center, 0.8f);
                CustomParticles.HaloRing(target.Center, NachtmusikCosmicVFX.Gold, 0.4f, 12);
            }
            else
            {
                CustomParticles.GenericFlare(target.Center, NachtmusikCosmicVFX.Violet, 0.4f, 10);
            }
        }
        
        public override void OnKill(int timeLeft)
        {
            if (isDimensionSever)
            {
                NachtmusikCosmicVFX.SpawnCelestialExplosion(Projectile.Center, 0.6f);
            }
            else
            {
                CustomParticles.GenericFlare(Projectile.Center, NachtmusikCosmicVFX.Violet, 0.35f, 12);
            }
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 origin = tex.Size() / 2f;
            
            Color coreColor = isDimensionSever ? NachtmusikCosmicVFX.Gold : NachtmusikCosmicVFX.Violet;
            float baseScale = isDimensionSever ? 0.5f : 0.35f;
            
            // Slash trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Color trailColor = Color.Lerp(coreColor, NachtmusikCosmicVFX.DeepPurple, progress) * (1f - progress) * 0.6f;
                float scale = baseScale * (1f - progress * 0.5f);
                
                // Elongated for slash effect
                sb.Draw(tex, drawPos, null, trailColor, Projectile.oldRot[i], origin, new Vector2(scale * 2f, scale * 0.6f), SpriteEffects.None, 0f);
            }
            
            // Core
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.3f) * 0.1f + 0.9f;
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, coreColor * 0.7f, Projectile.rotation, origin, new Vector2(baseScale * 1.5f * pulse, baseScale * 0.5f * pulse), SpriteEffects.None, 0f);
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, NachtmusikCosmicVFX.StarWhite, Projectile.rotation, origin, new Vector2(baseScale * 0.8f * pulse, baseScale * 0.25f * pulse), SpriteEffects.None, 0f);
            
            return false;
        }
    }
    
    #endregion
}
