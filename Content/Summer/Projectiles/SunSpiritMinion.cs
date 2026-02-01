using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Summer.Projectiles
{
    /// <summary>
    /// Sun Spirit Minion - Orbiting sun spirits that attack with solar flares.
    /// Features:
    /// - Orbital movement around player
    /// - Solar flare ranged attacks
    /// - Zenith Formation burst when 3+ spirits present
    /// - Dynamic glowing appearance
    /// </summary>
    public class SunSpiritMinion : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/StarBurst1";
        
        private static readonly Color SunGold = new Color(255, 215, 0);
        private static readonly Color SunOrange = new Color(255, 140, 0);
        private static readonly Color SunWhite = new Color(255, 250, 240);
        private static readonly Color SunRed = new Color(255, 100, 50);

        private float orbitAngle;
        private float orbitRadius = 80f;
        private int attackCooldown = 0;
        private int zenithCooldown = 0;
        private bool inZenithFormation = false;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            Main.projPet[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.netImportant = true;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 2;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Summon;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => true;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!CheckActive(owner)) return;

            // Get minion index for spacing
            int minionIndex = GetMinionIndex(owner);
            int minionCount = owner.ownedProjectileCounts[Projectile.type];

            // Update cooldowns
            if (attackCooldown > 0) attackCooldown--;
            if (zenithCooldown > 0) zenithCooldown--;

            // Find target
            NPC target = FindTarget(owner);
            
            // Orbital movement with attack behavior
            if (target != null && target.active)
            {
                AttackTarget(owner, target, minionIndex, minionCount);
            }
            else
            {
                IdleMovement(owner, minionIndex, minionCount);
            }

            // Visual effects
            SpawnAmbientEffects();

            // Light emission
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f + minionIndex * 0.5f) * 0.2f + 0.8f;
            Lighting.AddLight(Projectile.Center, SunGold.ToVector3() * pulse);

            // Zenith Formation check - 3+ spirits = synchronized burst
            if (minionCount >= 3 && zenithCooldown <= 0 && target != null)
            {
                TryZenithFormation(owner, target, minionIndex, minionCount);
            }
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<Weapons.SunSpiritBuff>());
                return false;
            }
            
            if (owner.HasBuff(ModContent.BuffType<Weapons.SunSpiritBuff>()))
            {
                Projectile.timeLeft = 2;
            }
            
            return true;
        }

        private int GetMinionIndex(Player owner)
        {
            int index = 0;
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.owner == Projectile.owner && proj.type == Projectile.type)
                {
                    if (proj.whoAmI == Projectile.whoAmI) return index;
                    index++;
                }
            }
            return index;
        }

        private NPC FindTarget(Player owner)
        {
            float maxRange = 600f;
            NPC target = null;
            float closestDist = float.MaxValue;

            // Check if player is targeting something
            if (owner.HasMinionAttackTargetNPC)
            {
                NPC targetNPC = Main.npc[owner.MinionAttackTargetNPC];
                if (targetNPC.CanBeChasedBy() && Vector2.Distance(owner.Center, targetNPC.Center) < maxRange)
                {
                    return targetNPC;
                }
            }

            // Find closest enemy
            foreach (NPC npc in Main.npc)
            {
                if (!npc.CanBeChasedBy()) continue;
                
                float dist = Vector2.Distance(owner.Center, npc.Center);
                if (dist < maxRange && dist < closestDist)
                {
                    closestDist = dist;
                    target = npc;
                }
            }

            return target;
        }

        private void AttackTarget(Player owner, NPC target, int minionIndex, int minionCount)
        {
            inZenithFormation = false;
            
            // Position between player and target
            float angleOffset = MathHelper.TwoPi * minionIndex / minionCount;
            orbitAngle += 0.06f;
            
            Vector2 toTarget = target.Center - owner.Center;
            float targetAngle = toTarget.ToRotation();
            
            // Orbit around attack point halfway between player and target
            Vector2 attackOrbit = owner.Center + toTarget * 0.5f;
            float attackRadius = 60f + minionIndex * 12f;
            Vector2 idealPos = attackOrbit + (orbitAngle + angleOffset).ToRotationVector2() * attackRadius;
            
            // Smooth movement
            Vector2 toIdeal = idealPos - Projectile.Center;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdeal * 0.12f, 0.15f);
            
            // Rotation toward target
            Projectile.rotation = (target.Center - Projectile.Center).ToRotation();

            // Fire solar flare
            if (attackCooldown <= 0 && Vector2.Distance(Projectile.Center, target.Center) < 400f)
            {
                FireSolarFlare(target);
                attackCooldown = 50 - minionCount * 5; // Faster with more spirits
                attackCooldown = Math.Max(attackCooldown, 25);
            }
        }

        private void FireSolarFlare(NPC target)
        {
            if (Main.myPlayer != Projectile.owner) return;

            Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
            float speed = 14f + Main.rand.NextFloat(0f, 2f);
            
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                direction * speed,
                ModContent.ProjectileType<SolarFlareProjectile>(),
                Projectile.damage,
                Projectile.knockBack,
                Projectile.owner
            );

            // Fire VFX
            CustomParticles.GenericFlare(Projectile.Center, SunOrange, 0.5f, 15);
            CustomParticles.GenericFlare(Projectile.Center, SunGold * 0.5f, 0.35f, 12);
            CustomParticles.GenericFlare(Projectile.Center, SunGold * 0.3f, 0.25f, 10);
            
            // ☁EMUSICAL BURST on fire!
            ThemedParticles.MusicNoteBurst(Projectile.Center, SunGold, 5, 3f);
            
            // Solar ray burst
            for (int ray = 0; ray < 4; ray++)
            {
                float rayAngle = MathHelper.TwoPi * ray / 4f;
                Vector2 rayPos = Projectile.Center + rayAngle.ToRotationVector2() * 10f;
                CustomParticles.GenericFlare(rayPos, SunOrange * 0.6f, 0.15f, 8);
            }
            
            // Particle burst in fire direction
            for (int i = 0; i < 6; i++)
            {
                Vector2 burstVel = direction * Main.rand.NextFloat(3f, 6f) + Main.rand.NextVector2Circular(2f, 2f);
                Color burstColor = Color.Lerp(SunGold, SunRed, Main.rand.NextFloat()) * 0.6f;
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor, 0.25f, 18, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
        }

        private void TryZenithFormation(Player owner, NPC target, int minionIndex, int minionCount)
        {
            // Only first spirit triggers the formation
            if (minionIndex != 0) return;

            inZenithFormation = true;

            // Zenith Formation: All spirits converge and unleash synchronized burst
            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 formationCenter = (owner.Center + target.Center) / 2f;
                
                // VFX buildup - layered bloom cascade
                CustomParticles.GenericFlare(formationCenter, SunWhite, 1.2f, 25);
                CustomParticles.GenericFlare(formationCenter, SunGold, 0.9f, 22);
                CustomParticles.GenericFlare(formationCenter, SunOrange, 0.7f, 20);
                CustomParticles.GenericFlare(formationCenter, SunRed, 0.55f, 18);
                
                // ☁EMUSICAL SYMPHONY - Grand note burst for Zenith Formation!
                ThemedParticles.MusicNoteBurst(formationCenter, SunGold, 12, 6f);
                ThemedParticles.MusicNoteRing(formationCenter, SunOrange, 60f, 8);
                
                // Solar ray burst - 8-point star
                for (int ray = 0; ray < 8; ray++)
                {
                    float rayAngle = MathHelper.TwoPi * ray / 8f;
                    Vector2 rayPos = formationCenter + rayAngle.ToRotationVector2() * 25f;
                    Color rayColor = ray % 2 == 0 ? SunGold : SunOrange;
                    CustomParticles.GenericFlare(rayPos, rayColor * 0.8f, 0.3f, 14);
                }

                // Radial solar burst
                int burstCount = 8 + minionCount * 2;
                for (int i = 0; i < burstCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / burstCount;
                    Vector2 velocity = angle.ToRotationVector2() * (10f + Main.rand.NextFloat(2f));
                    
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        formationCenter,
                        velocity,
                        ModContent.ProjectileType<ZenithFlareProjectile>(),
                        (int)(Projectile.damage * 1.5f),
                        Projectile.knockBack,
                        Projectile.owner
                    );
                }

                // Intense particle explosion
                for (int i = 0; i < 20; i++)
                {
                    float angle = MathHelper.TwoPi * i / 20f;
                    Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 12f);
                    Color burstColor = Color.Lerp(SunGold, SunRed, (float)i / 20f);
                    var burst = new GenericGlowParticle(formationCenter, burstVel, burstColor * 0.8f, 0.45f, 28, true);
                    MagnumParticleHandler.SpawnParticle(burst);
                }
            }

            zenithCooldown = 240; // 4 second cooldown
        }

        private void IdleMovement(Player owner, int minionIndex, int minionCount)
        {
            inZenithFormation = false;
            
            // Peaceful orbit around player
            float angleOffset = MathHelper.TwoPi * minionIndex / minionCount;
            orbitAngle += 0.035f;
            
            orbitRadius = MathHelper.Lerp(orbitRadius, 80f + minionIndex * 15f, 0.05f);
            
            Vector2 idealPos = owner.Center + (orbitAngle + angleOffset).ToRotationVector2() * orbitRadius;
            Vector2 toIdeal = idealPos - Projectile.Center;
            
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdeal * 0.08f, 0.1f);
            Projectile.rotation = orbitAngle + angleOffset + MathHelper.PiOver2;
        }

        private void SpawnAmbientEffects()
        {
            // ☁EMUSICAL NOTATION - Notes orbit the sun spirit! - VISIBLE SCALE 0.72f+
            if (Main.rand.NextBool(8))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-1.5f, -0.5f));
                Color noteColor = Color.Lerp(SunGold, SunOrange, Main.rand.NextFloat());
                ThemedParticles.MusicNote(Projectile.Center + Main.rand.NextVector2Circular(12f, 12f), noteVel, noteColor, 0.72f, 40);
                
                // Solar sparkle accent
                var sparkle = new SparkleParticle(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f), noteVel * 0.5f, SunGold * 0.5f, 0.28f, 22);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Core glow trail
            if (Main.rand.NextBool(3))
            {
                Vector2 trailVel = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f);
                Color trailColor = Color.Lerp(SunGold, SunOrange, Main.rand.NextFloat()) * 0.5f;
                var trail = new GenericGlowParticle(Projectile.Center, trailVel, trailColor, 0.28f, 20, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // Solar prominence wisps
            if (Main.rand.NextBool(10))
            {
                Vector2 wispVel = Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0, -1f);
                Color wispColor = Color.Lerp(SunOrange, SunRed, Main.rand.NextFloat()) * 0.4f;
                var wisp = new GenericGlowParticle(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), wispVel, wispColor, 0.2f, 25, true);
                MagnumParticleHandler.SpawnParticle(wisp);
            }

            // Zenith formation glow
            if (inZenithFormation)
            {
                CustomParticles.GenericFlare(Projectile.Center, SunWhite * 0.5f, 0.4f, 8);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/StarBurst1").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f + Projectile.whoAmI * 0.3f) * 0.15f + 1f;
            float zenithMult = inZenithFormation ? 1.3f : 1f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Outer corona
            spriteBatch.Draw(texture, drawPos, null, SunOrange * 0.25f * zenithMult, 0f, origin, 0.65f * pulse * zenithMult, SpriteEffects.None, 0f);
            // Middle radiance
            spriteBatch.Draw(texture, drawPos, null, SunGold * 0.4f * zenithMult, 0f, origin, 0.45f * pulse * zenithMult, SpriteEffects.None, 0f);
            // Core
            spriteBatch.Draw(texture, drawPos, null, SunWhite * 0.55f * zenithMult, 0f, origin, 0.28f * pulse * zenithMult, SpriteEffects.None, 0f);
            // Hot center
            spriteBatch.Draw(texture, drawPos, null, SunWhite * 0.75f, 0f, origin, 0.12f * pulse, SpriteEffects.None, 0f);

            // Orbiting prominences
            for (int i = 0; i < 4; i++)
            {
                float promAngle = Main.GameUpdateCount * 0.03f + MathHelper.TwoPi * i / 4f;
                float promDist = 14f + (float)Math.Sin(Main.GameUpdateCount * 0.08f + i) * 4f;
                Vector2 promPos = drawPos + promAngle.ToRotationVector2() * promDist;
                Color promColor = i % 2 == 0 ? SunOrange : SunRed;
                spriteBatch.Draw(texture, promPos, null, promColor * 0.5f, 0f, origin, 0.1f * pulse, SpriteEffects.None, 0f);
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    /// <summary>
    /// Solar Flare - Basic attack projectile from sun spirits
    /// </summary>
    public class SolarFlareProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/EnergyFlare";
        
        private static readonly Color SunGold = new Color(255, 215, 0);
        private static readonly Color SunOrange = new Color(255, 140, 0);
        private static readonly Color SunRed = new Color(255, 100, 50);

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Trail
            if (Main.rand.NextBool(2))
            {
                Vector2 trailVel = -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                Color trailColor = Color.Lerp(SunGold, SunOrange, Main.rand.NextFloat()) * 0.6f;
                var trail = new GenericGlowParticle(Projectile.Center, trailVel, trailColor, 0.3f, 18, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // Core flare
            CustomParticles.GenericFlare(Projectile.Center, SunOrange * 0.4f, 0.25f, 5);

            // ☁EMUSICAL NOTATION - Notes trail from solar flare! - VISIBLE SCALE 0.7f+
            if (Main.rand.NextBool(5))
            {
                Vector2 noteVel = -Projectile.velocity * 0.04f + new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), Main.rand.NextFloat(-0.8f, -0.2f));
                ThemedParticles.MusicNote(Projectile.Center, noteVel, SunGold, 0.7f, 35);
                
                // Solar sparkle accent
                var sparkle = new SparkleParticle(Projectile.Center, noteVel * 0.5f, SunOrange * 0.4f, 0.2f, 16);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            Lighting.AddLight(Projectile.Center, SunGold.ToVector3() * 0.6f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply On Fire
            target.AddBuff(BuffID.OnFire3, 180);

            // ☁EMUSICAL IMPACT - Notes burst with solar fire!
            ThemedParticles.MusicNoteBurst(target.Center, SunGold, 6, 3.5f);

            // Hit VFX - layered bloom
            CustomParticles.GenericFlare(target.Center, SunGold, 0.45f, 15);
            CustomParticles.GenericFlare(target.Center, SunOrange * 0.5f, 0.35f, 12);
            CustomParticles.GenericFlare(target.Center, SunOrange * 0.3f, 0.25f, 10);
            // Solar ray burst
            for (int ray = 0; ray < 4; ray++)
            {
                float rayAngle = MathHelper.TwoPi * ray / 4f;
                Vector2 rayPos = target.Center + rayAngle.ToRotationVector2() * 12f;
                CustomParticles.GenericFlare(rayPos, SunGold * 0.6f, 0.15f, 8);
            }

            for (int i = 0; i < 6; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(5f, 5f);
                Color sparkColor = Color.Lerp(SunGold, SunRed, Main.rand.NextFloat()) * 0.7f;
                var spark = new GenericGlowParticle(target.Center, sparkVel, sparkColor, 0.22f, 16, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Layered bloom death effect
            CustomParticles.GenericFlare(Projectile.Center, SunOrange, 0.4f, 15);
            CustomParticles.GenericFlare(Projectile.Center, SunGold * 0.4f, 0.3f, 12);
            CustomParticles.GenericFlare(Projectile.Center, SunGold * 0.25f, 0.2f, 10);
            // Solar ray burst
            for (int ray = 0; ray < 4; ray++)
            {
                float rayAngle = MathHelper.TwoPi * ray / 4f;
                Vector2 rayPos = Projectile.Center + rayAngle.ToRotationVector2() * 10f;
                CustomParticles.GenericFlare(rayPos, SunOrange * 0.5f, 0.12f, 8);
            }

            for (int i = 0; i < 8; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(4f, 4f);
                Color burstColor = Color.Lerp(SunGold, SunOrange, Main.rand.NextFloat()) * 0.5f;
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor, 0.25f, 16, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.12f + 1f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, drawPos, null, SunOrange * 0.35f, 0f, origin, 0.35f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, SunGold * 0.5f, 0f, origin, 0.22f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, Color.White * 0.6f, 0f, origin, 0.1f * pulse, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    /// <summary>
    /// Zenith Flare - Powerful burst projectile from Zenith Formation
    /// </summary>
    public class ZenithFlareProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/EnergyFlare2";
        
        private static readonly Color SunGold = new Color(255, 215, 0);
        private static readonly Color SunWhite = new Color(255, 250, 240);
        private static readonly Color SunRed = new Color(255, 100, 50);

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Intense trail
            if (Main.rand.NextBool())
            {
                Vector2 trailVel = -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(2f, 2f);
                Color trailColor = Color.Lerp(SunGold, SunWhite, Main.rand.NextFloat()) * 0.7f;
                var trail = new GenericGlowParticle(Projectile.Center, trailVel, trailColor, 0.4f, 22, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // Bright core
            CustomParticles.GenericFlare(Projectile.Center, SunWhite * 0.5f, 0.35f, 5);

            // ☁EMUSICAL NOTATION - Blazing notes trail from zenith flare! - VISIBLE SCALE 0.75f+
            if (Main.rand.NextBool(3))
            {
                Vector2 noteVel = -Projectile.velocity * 0.05f + new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-1.2f, -0.4f));
                ThemedParticles.MusicNote(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), noteVel, Color.Lerp(SunGold, SunWhite, Main.rand.NextFloat()), 0.75f, 40);
                
                // Blazing sparkle
                var sparkle = new SparkleParticle(Projectile.Center, noteVel * 0.6f, SunWhite * 0.5f, 0.25f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            Lighting.AddLight(Projectile.Center, SunWhite.ToVector3() * 0.8f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Strong burn
            target.AddBuff(BuffID.OnFire3, 300);
            target.AddBuff(BuffID.Daybreak, 150);

            // ☁EMUSICAL IMPACT - Grand solar symphony!
            ThemedParticles.MusicNoteBurst(target.Center, SunGold, 10, 5f);
            ThemedParticles.MusicNoteRing(target.Center, SunWhite, 45f, 6);

            // Intense hit VFX - layered bloom cascade
            CustomParticles.GenericFlare(target.Center, SunWhite, 0.7f, 20);
            CustomParticles.GenericFlare(target.Center, SunGold, 0.55f, 18);
            CustomParticles.GenericFlare(target.Center, SunGold * 0.6f, 0.4f, 16);
            CustomParticles.GenericFlare(target.Center, SunRed * 0.6f, 0.35f, 14);
            // Intense solar ray burst - 6-point star
            for (int ray = 0; ray < 6; ray++)
            {
                float rayAngle = MathHelper.TwoPi * ray / 6f;
                Vector2 rayPos = target.Center + rayAngle.ToRotationVector2() * 18f;
                Color rayColor = ray % 2 == 0 ? SunGold : SunRed;
                CustomParticles.GenericFlare(rayPos, rayColor * 0.7f, 0.22f, 12);
            }

            for (int i = 0; i < 10; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(7f, 7f);
                Color sparkColor = Color.Lerp(SunGold, SunWhite, Main.rand.NextFloat()) * 0.8f;
                var spark = new GenericGlowParticle(target.Center, sparkVel, sparkColor, 0.32f, 20, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
        }

        public override void OnKill(int timeLeft)
        {
            // ☁EMUSICAL FINALE - Zenith explosion with notes!
            ThemedParticles.MusicNoteBurst(Projectile.Center, SunGold, 8, 4.5f);
            
            // Zenith explosion - layered bloom cascade
            CustomParticles.GenericFlare(Projectile.Center, SunWhite, 0.6f, 20);
            CustomParticles.GenericFlare(Projectile.Center, SunGold, 0.5f, 18);
            CustomParticles.GenericFlare(Projectile.Center, SunGold * 0.6f, 0.38f, 16);
            CustomParticles.GenericFlare(Projectile.Center, SunRed * 0.5f, 0.28f, 14);
            // Zenith solar ray burst - 6-point star
            for (int ray = 0; ray < 6; ray++)
            {
                float rayAngle = MathHelper.TwoPi * ray / 6f;
                Vector2 rayPos = Projectile.Center + rayAngle.ToRotationVector2() * 15f;
                Color rayColor = ray % 2 == 0 ? SunGold : SunWhite;
                CustomParticles.GenericFlare(rayPos, rayColor * 0.7f, 0.2f, 12);
            }

            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                Color burstColor = Color.Lerp(SunGold, SunWhite, (float)i / 12f) * 0.6f;
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor, 0.3f, 20, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare2").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.18f) * 0.15f + 1f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, drawPos, null, SunGold * 0.4f, 0f, origin, 0.5f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, SunWhite * 0.55f, 0f, origin, 0.32f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, Color.White * 0.7f, 0f, origin, 0.15f * pulse, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}
