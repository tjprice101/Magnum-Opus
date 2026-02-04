using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Nachtmusik.Debuffs;

namespace MagnumOpus.Content.Nachtmusik.Projectiles
{
    #region Summon Minions
    
    /// <summary>
    /// Nocturnal Guardian minion - a spectral warrior that orbits the player.
    /// Aggressively slashes at enemies with celestial blades.
    /// </summary>
    public class NocturnalGuardianMinion : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/Nachtmusik/ResonantWeapons/NocturnalGuardianMinion";
        
        private float orbitAngle;
        private int attackCooldown;
        private bool isAttacking;
        private Vector2 attackTarget;
        private int attackTimer;
        
        public override void SetStaticDefaults()
        {
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
        }
        
        public override bool? CanCutTiles() => false;
        
        public override bool MinionContactDamage() => true;
        
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            
            if (!CheckActive(owner))
                return;
            
            orbitAngle += 0.04f;
            attackCooldown = Math.Max(0, attackCooldown - 1);
            
            NPC target = FindTarget(owner, 800f);
            
            if (!isAttacking)
            {
                // Orbit around player
                float orbitRadius = 80f + 30f * (float)Math.Sin(orbitAngle * 0.5f);
                Vector2 idealPos = owner.Center + orbitAngle.ToRotationVector2() * orbitRadius;
                Vector2 toIdeal = idealPos - Projectile.Center;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdeal * 0.15f, 0.1f);
                
                // Check for attack opportunity
                if (target != null && attackCooldown == 0)
                {
                    isAttacking = true;
                    attackTarget = target.Center;
                    attackTimer = 0;
                    attackCooldown = 45;
                }
            }
            else
            {
                attackTimer++;
                
                // Dash attack
                if (attackTimer < 15)
                {
                    Vector2 toTarget = (attackTarget - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    Projectile.velocity = toTarget * 22f;
                    
                    // Attack VFX
                    NachtmusikCosmicVFX.SpawnCelestialCloudTrail(Projectile.Center, Projectile.velocity, 0.4f);
                }
                else
                {
                    // Return
                    isAttacking = false;
                }
            }
            
            Projectile.rotation = Projectile.velocity.X * 0.02f;
            Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;
            
            // Ambient particles
            if (Main.rand.NextBool(5))
            {
                var glow = new GenericGlowParticle(Projectile.Center + Main.rand.NextVector2Circular(15f, 15f),
                    Main.rand.NextVector2Circular(0.5f, 0.5f), NachtmusikCosmicVFX.Violet * 0.5f, 0.2f, 15, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // ☁EMUSICAL PRESENCE - Celestial guardian aura - VISIBLE SCALE 0.72f+
            if (Main.rand.NextBool(15))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -0.8f);
                ThemedParticles.MusicNote(Projectile.Center + Main.rand.NextVector2Circular(15f, 15f), noteVel, NachtmusikCosmicVFX.DeepPurple, 0.72f, 30);
                
                // Celestial sparkle accent
                var sparkle = new SparkleParticle(Projectile.Center + Main.rand.NextVector2Circular(12f, 12f), noteVel * 0.6f, NachtmusikCosmicVFX.StarWhite * 0.6f, 0.3f, 22);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            Lighting.AddLight(Projectile.Center, NachtmusikCosmicVFX.DeepPurple.ToVector3() * 0.5f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 300);
            target.GetGlobalNPC<CelestialHarmonyNPC>().AddStack(target, 1);
            NachtmusikCosmicVFX.SpawnCelestialImpact(target.Center, 0.8f);
            
            // ☁EMUSICAL IMPACT - Guardian strike chord - VISIBLE SCALE 0.72f+
            ThemedParticles.MusicNoteBurst(target.Center, NachtmusikCosmicVFX.Violet, 5, 3.5f);
            
            // Celestial sparkle burst
            for (int i = 0; i < 4; i++)
            {
                var sparkle = new SparkleParticle(target.Center, Main.rand.NextVector2Circular(3f, 3f), NachtmusikCosmicVFX.StarWhite * 0.7f, 0.28f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            
            // Glow behind sprite
            Texture2D glow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow3").Value;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.1f + 0.9f;
            sb.Draw(glow, drawPos, null, NachtmusikCosmicVFX.DeepPurple * 0.4f, 0f, glow.Size() / 2f, 0.7f * pulse, SpriteEffects.None, 0f);
            sb.Draw(glow, drawPos, null, NachtmusikCosmicVFX.Violet * 0.3f, 0f, glow.Size() / 2f, 0.5f * pulse, SpriteEffects.None, 0f);
            
            // Main sprite
            sb.Draw(tex, drawPos, null, Color.White, Projectile.rotation, origin, Projectile.scale, effects, 0f);
            
            return false;
        }
        
        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<CelestialChorusBatonBuff>());
                return false;
            }
            
            if (owner.HasBuff(ModContent.BuffType<CelestialChorusBatonBuff>()))
                Projectile.timeLeft = 2;
            
            return true;
        }
        
        private NPC FindTarget(Player owner, float range)
        {
            // Check for manual target
            if (owner.HasMinionAttackTargetNPC)
            {
                NPC manual = Main.npc[owner.MinionAttackTargetNPC];
                if (manual.active && manual.CanBeChasedBy(Projectile) && Vector2.Distance(owner.Center, manual.Center) < range * 1.5f)
                    return manual;
            }
            
            NPC closest = null;
            float closestDist = range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy(Projectile))
                {
                    float dist = Vector2.Distance(owner.Center, npc.Center);
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
    /// Celestial Muse minion - a musical spirit that buffs and attacks.
    /// Fires melodic projectiles and provides aura bonuses.
    /// </summary>
    public class CelestialMuseMinion : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/Nachtmusik/ResonantWeapons/CelestialMuseMinion";
        
        private float hoverAngle;
        private int attackCooldown;
        
        public override void SetStaticDefaults()
        {
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }
        
        public override bool? CanCutTiles() => false;
        
        public override bool MinionContactDamage() => false;
        
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            
            if (!CheckActive(owner))
                return;
            
            hoverAngle += 0.02f;
            attackCooldown = Math.Max(0, attackCooldown - 1);
            
            // Hover near player
            float hoverOffset = (float)Math.Sin(hoverAngle) * 30f;
            Vector2 idealPos = owner.Center + new Vector2(owner.direction * -60f, -50f + hoverOffset);
            Vector2 toIdeal = idealPos - Projectile.Center;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdeal * 0.1f, 0.08f);
            
            Projectile.spriteDirection = owner.direction;
            
            // Find and attack target
            NPC target = FindTarget(owner, 700f);
            if (target != null && attackCooldown == 0)
            {
                // Fire musical projectile
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, toTarget * 14f,
                    ModContent.ProjectileType<MuseNoteProjectile>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                
                attackCooldown = 20;
                
                // Fire VFX - VISIBLE SCALE 0.7f+
                CustomParticles.GenericFlare(Projectile.Center, NachtmusikCosmicVFX.Gold, 0.4f, 12);
                ThemedParticles.MusicNote(Projectile.Center + toTarget * 15f, toTarget * 2f, NachtmusikCosmicVFX.Gold, 0.7f, 15);
            }
            
            // Ambient music notes - VISIBLE SCALE 0.7f+
            if (Main.rand.NextBool(20))
            {
                ThemedParticles.MusicNote(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f),
                    new Vector2(0, -1f), NachtmusikCosmicVFX.Violet * 0.7f, 0.7f, 25);
                
                // Golden sparkle accent
                var sparkle = new SparkleParticle(Projectile.Center + Main.rand.NextVector2Circular(15f, 15f), new Vector2(0, -0.8f), NachtmusikCosmicVFX.Gold * 0.5f, 0.25f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            Lighting.AddLight(Projectile.Center, NachtmusikCosmicVFX.Gold.ToVector3() * 0.4f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            
            // Golden glow
            Texture2D glow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow4").Value;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.15f + 0.85f;
            sb.Draw(glow, drawPos, null, NachtmusikCosmicVFX.Gold * 0.3f, 0f, glow.Size() / 2f, 0.6f * pulse, SpriteEffects.None, 0f);
            sb.Draw(glow, drawPos, null, NachtmusikCosmicVFX.Violet * 0.2f, 0f, glow.Size() / 2f, 0.4f * pulse, SpriteEffects.None, 0f);
            
            sb.Draw(tex, drawPos, null, Color.White, 0f, origin, Projectile.scale, effects, 0f);
            
            return false;
        }
        
        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<GalacticOvertureBuff>());
                return false;
            }
            
            if (owner.HasBuff(ModContent.BuffType<GalacticOvertureBuff>()))
                Projectile.timeLeft = 2;
            
            return true;
        }
        
        private NPC FindTarget(Player owner, float range)
        {
            if (owner.HasMinionAttackTargetNPC)
            {
                NPC manual = Main.npc[owner.MinionAttackTargetNPC];
                if (manual.active && manual.CanBeChasedBy(Projectile) && Vector2.Distance(owner.Center, manual.Center) < range * 1.5f)
                    return manual;
            }
            
            NPC closest = null;
            float closestDist = range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy(Projectile))
                {
                    float dist = Vector2.Distance(owner.Center, npc.Center);
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
    /// Muse's musical note attack projectile.
    /// </summary>
    public class MuseNoteProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/CursiveMusicNote";
        
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }
        
        public override void AI()
        {
            Projectile.rotation += 0.2f;
            
            // Musical trail
            if (Main.rand.NextBool(3))
            {
                var trail = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f,
                    NachtmusikCosmicVFX.Gold * 0.6f, 0.15f, 12, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // ☁EMUSICAL NOTATION - Muse note melody trail - VISIBLE SCALE 0.7f+
            if (Main.rand.NextBool(5))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), -0.9f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, NachtmusikCosmicVFX.Gold, 0.7f, 25);
                
                // Golden sparkle accent
                var sparkle = new SparkleParticle(Projectile.Center, noteVel * 0.5f, NachtmusikCosmicVFX.Gold * 0.5f, 0.22f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            Lighting.AddLight(Projectile.Center, NachtmusikCosmicVFX.Gold.ToVector3() * 0.3f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 240);
            CustomParticles.GenericFlare(target.Center, NachtmusikCosmicVFX.Gold, 0.4f, 10);
            
            // ☁EMUSICAL IMPACT - Muse melody chord - VISIBLE SCALE 0.7f+
            ThemedParticles.MusicNoteBurst(target.Center, NachtmusikCosmicVFX.Gold, 4, 3f);
            
            // Sparkle burst
            for (int i = 0; i < 3; i++)
            {
                var sparkle = new SparkleParticle(target.Center, Main.rand.NextVector2Circular(2.5f, 2.5f), NachtmusikCosmicVFX.StarWhite * 0.6f, 0.22f, 15);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }
        
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                var burst = new GenericGlowParticle(Projectile.Center, Main.rand.NextVector2Circular(3f, 3f),
                    NachtmusikCosmicVFX.Gold * 0.6f, 0.2f, 12, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
            
            // ☁EMUSICAL FINALE - Muse note finale - VISIBLE SCALE 0.72f+
            ThemedParticles.MusicNoteBurst(Projectile.Center, NachtmusikCosmicVFX.Gold, 5, 3.5f);
            
            // Finale sparkle cascade
            for (int i = 0; i < 4; i++)
            {
                var sparkle = new SparkleParticle(Projectile.Center, Main.rand.NextVector2Circular(3f, 3f), NachtmusikCosmicVFX.Gold * 0.6f, 0.25f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/CursiveMusicNote").Value;
            Vector2 origin = tex.Size() / 2f;
            
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, NachtmusikCosmicVFX.Gold * 0.7f, Projectile.rotation, origin, 0.3f, SpriteEffects.None, 0f);
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, NachtmusikCosmicVFX.StarWhite * 0.8f, Projectile.rotation, origin, 0.15f, SpriteEffects.None, 0f);
            
            return false;
        }
    }
    
    /// <summary>
    /// Stellar Conductor minion - commands multiple smaller star spirits.
    /// The most powerful summon from Nachtmusik.
    /// </summary>
    public class StellarConductorMinion : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/Nachtmusik/ResonantWeapons/StellarConductorMinion";
        
        private float conductAngle;
        private int attackCooldown;
        private int orchestraTimer;
        
        public override void SetStaticDefaults()
        {
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 36;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 2f; // Takes 2 slots but is VERY powerful
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }
        
        public override bool? CanCutTiles() => false;
        
        public override bool MinionContactDamage() => false;
        
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            
            if (!CheckActive(owner))
                return;
            
            conductAngle += 0.015f;
            attackCooldown = Math.Max(0, attackCooldown - 1);
            orchestraTimer++;
            
            // Hover above player
            float hoverY = (float)Math.Sin(conductAngle * 2f) * 15f;
            Vector2 idealPos = owner.Center + new Vector2(0, -100f + hoverY);
            Vector2 toIdeal = idealPos - Projectile.Center;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdeal * 0.08f, 0.1f);
            
            NPC target = FindTarget(owner, 900f);
            
            // Attack: Command a barrage of star attacks
            if (target != null && attackCooldown == 0)
            {
                // Fire 3 star projectiles in sequence
                for (int i = 0; i < 3; i++)
                {
                    float angleOffset = MathHelper.ToRadians(-15f + 15f * i);
                    Vector2 baseDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    Vector2 dir = baseDir.RotatedBy(angleOffset);
                    
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + dir * 20f,
                        dir * 16f, ModContent.ProjectileType<ConductorStarProjectile>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                }
                
                attackCooldown = 25;
                
                // Conducting VFX
                NachtmusikCosmicVFX.SpawnConstellationCircle(Projectile.Center, 40f, 6, 0.3f);
                SoundEngine.PlaySound(SoundID.Item25 with { Pitch = 0.5f, Volume = 0.6f }, Projectile.Center);
            }
            
            // Periodic orchestra burst
            if (orchestraTimer % 180 == 0 && target != null)
            {
                // Major attack: Ring of stars
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 dir = angle.ToRotationVector2();
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center,
                        dir * 12f, ModContent.ProjectileType<ConductorStarProjectile>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                }
                
                NachtmusikCosmicVFX.SpawnCelestialExplosion(Projectile.Center, 0.7f);
                NachtmusikCosmicVFX.SpawnMusicNoteBurst(Projectile.Center, 12, 6f);
            }
            
            // Ambient conducting particles
            if (Main.rand.NextBool(8))
            {
                float sparkAngle = conductAngle * 3f;
                Vector2 sparkPos = Projectile.Center + sparkAngle.ToRotationVector2() * 30f;
                CustomParticles.GenericFlare(sparkPos, NachtmusikCosmicVFX.Gold, 0.25f, 12);
            }
            
            // ☁EMUSICAL PRESENCE - Conductor's celestial baton - VISIBLE SCALE 0.75f+
            if (Main.rand.NextBool(12))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), -1f);
                ThemedParticles.MusicNote(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f), noteVel, NachtmusikCosmicVFX.Gold, 0.75f, 35);
                
                // Majestic star sparkle accent
                var sparkle = new SparkleParticle(Projectile.Center + Main.rand.NextVector2Circular(18f, 18f), noteVel * 0.6f, NachtmusikCosmicVFX.StarWhite * 0.65f, 0.32f, 25);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Orbiting glyphs
            if (orchestraTimer % 30 == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    float glyphAngle = conductAngle + MathHelper.TwoPi * i / 3f;
                    Vector2 glyphPos = Projectile.Center + glyphAngle.ToRotationVector2() * 45f;
                    CustomParticles.Glyph(glyphPos, NachtmusikCosmicVFX.Violet, 0.3f, -1);
                }
            }
            
            Lighting.AddLight(Projectile.Center, NachtmusikCosmicVFX.Gold.ToVector3() * 0.6f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            // Majestic glow
            Texture2D glow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/GlowingHalo4").Value;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.2f + 0.8f;
            sb.Draw(glow, drawPos, null, NachtmusikCosmicVFX.DeepPurple * 0.5f, 0f, glow.Size() / 2f, 1f * pulse, SpriteEffects.None, 0f);
            sb.Draw(glow, drawPos, null, NachtmusikCosmicVFX.Gold * 0.4f, 0f, glow.Size() / 2f, 0.7f * pulse, SpriteEffects.None, 0f);
            sb.Draw(glow, drawPos, null, NachtmusikCosmicVFX.Violet * 0.3f, 0f, glow.Size() / 2f, 0.5f * pulse, SpriteEffects.None, 0f);
            
            sb.Draw(tex, drawPos, null, Color.White, 0f, origin, Projectile.scale, SpriteEffects.None, 0f);
            
            return false;
        }
        
        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<ConductorOfConstellationsBuff>());
                return false;
            }
            
            if (owner.HasBuff(ModContent.BuffType<ConductorOfConstellationsBuff>()))
                Projectile.timeLeft = 2;
            
            return true;
        }
        
        private NPC FindTarget(Player owner, float range)
        {
            if (owner.HasMinionAttackTargetNPC)
            {
                NPC manual = Main.npc[owner.MinionAttackTargetNPC];
                if (manual.active && manual.CanBeChasedBy(Projectile) && Vector2.Distance(owner.Center, manual.Center) < range * 1.5f)
                    return manual;
            }
            
            NPC closest = null;
            float closestDist = range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy(Projectile))
                {
                    float dist = Vector2.Distance(owner.Center, npc.Center);
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
    /// Stellar Conductor's star attack projectile.
    /// </summary>
    public class ConductorStarProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/Star";
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 150;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }
        
        public override void AI()
        {
            Projectile.rotation += 0.15f;
            
            // Light homing
            if (Projectile.timeLeft < 120)
            {
                NPC target = FindClosestEnemy(500f);
                if (target != null)
                {
                    Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), 0.04f);
                }
            }
            
            // Star trail
            if (Main.rand.NextBool(2))
            {
                var trail = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f,
                    NachtmusikCosmicVFX.StarWhite * 0.5f, 0.15f, 10, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // ☁EMUSICAL NOTATION - Conductor star melody - VISIBLE SCALE 0.7f+
            if (Main.rand.NextBool(6))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, NachtmusikCosmicVFX.StarWhite, 0.7f, 28);
                
                // Star sparkle accent
                var sparkle = new SparkleParticle(Projectile.Center, noteVel * 0.4f, NachtmusikCosmicVFX.Gold * 0.5f, 0.2f, 16);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            Lighting.AddLight(Projectile.Center, NachtmusikCosmicVFX.StarWhite.ToVector3() * 0.4f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 300);
            target.GetGlobalNPC<CelestialHarmonyNPC>().AddStack(target, 1);
            NachtmusikCosmicVFX.SpawnCelestialImpact(target.Center, 0.6f);
            
            // ☁EMUSICAL IMPACT - Conductor star chord - VISIBLE SCALE 0.7f+
            ThemedParticles.MusicNoteBurst(target.Center, NachtmusikCosmicVFX.StarWhite, 4, 3f);
            
            // Star sparkle burst
            for (int i = 0; i < 3; i++)
            {
                var sparkle = new SparkleParticle(target.Center, Main.rand.NextVector2Circular(3f, 3f), NachtmusikCosmicVFX.Gold * 0.6f, 0.22f, 15);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }
        
        public override void OnKill(int timeLeft)
        {
            CustomParticles.GenericFlare(Projectile.Center, NachtmusikCosmicVFX.Gold, 0.4f, 12);
            for (int i = 0; i < 5; i++)
            {
                var burst = new GenericGlowParticle(Projectile.Center, Main.rand.NextVector2Circular(4f, 4f),
                    NachtmusikCosmicVFX.GetCelestialGradient(Main.rand.NextFloat()) * 0.6f, 0.15f, 10, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
            
            // ☁EMUSICAL FINALE - Conductor star finale - VISIBLE SCALE 0.72f+
            ThemedParticles.MusicNoteBurst(Projectile.Center, NachtmusikCosmicVFX.Gold, 5, 3.5f);
            
            // Finale sparkle cascade
            for (int i = 0; i < 4; i++)
            {
                var sparkle = new SparkleParticle(Projectile.Center, Main.rand.NextVector2Circular(4f, 4f), NachtmusikCosmicVFX.StarWhite * 0.6f, 0.25f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/Star").Value;
            Vector2 origin = tex.Size() / 2f;
            
            // Trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Color trailColor = NachtmusikCosmicVFX.GetCelestialGradient(progress) * (1f - progress) * 0.5f;
                sb.Draw(tex, drawPos, null, trailColor, 0f, origin, 0.25f * (1f - progress * 0.4f), SpriteEffects.None, 0f);
            }
            
            // Core
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.1f + 0.9f;
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, NachtmusikCosmicVFX.Gold * 0.6f, Projectile.rotation, origin, 0.35f * pulse, SpriteEffects.None, 0f);
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, NachtmusikCosmicVFX.StarWhite * 0.8f, Projectile.rotation, origin, 0.2f * pulse, SpriteEffects.None, 0f);
            
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
    
    #region Minion Buffs
    
    public class CelestialChorusBatonBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }
        
        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<NocturnalGuardianMinion>()] > 0)
            {
                player.buffTime[buffIndex] = 18000;
            }
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
    
    public class GalacticOvertureBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }
        
        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<CelestialMuseMinion>()] > 0)
            {
                player.buffTime[buffIndex] = 18000;
            }
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
    
    public class ConductorOfConstellationsBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }
        
        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<StellarConductorMinion>()] > 0)
            {
                player.buffTime[buffIndex] = 18000;
            }
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
    
    #endregion
}
