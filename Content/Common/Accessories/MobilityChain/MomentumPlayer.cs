using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.LaCampanella.Debuffs;

namespace MagnumOpus.Content.Common.Accessories.MobilityChain
{
    /// <summary>
    /// ModPlayer for the Momentum System - Mobility Chain
    /// Moving continuously builds Momentum which grants various effects
    /// Standing still causes Momentum to decay
    /// </summary>
    public class MomentumPlayer : ModPlayer
    {
        // ========== ACCESSORY FLAGS ==========
        // Tier 1: Resonant Velocity Band
        public bool HasVelocityBand { get; set; }
        
        // Tier 2: Spring Zephyr Boots
        public bool HasSpringZephyrBoots { get; set; }
        
        // Tier 3: Solar Blitz Treads
        public bool HasSolarBlitzTreads { get; set; }
        
        // Tier 4: Harvest Phantom Stride
        public bool HasHarvestPhantomStride { get; set; }
        
        // Tier 5: Permafrost Avalanche Step
        public bool HasPermafrostAvalancheStep { get; set; }
        
        // Tier 6: Vivaldi's Seasonal Sprint
        public bool HasVivaldisSeasonalSprint { get; set; }
        
        // Post-Moon Lord Theme Chain
        public bool HasMoonlitPhantomsRush { get; set; }    // T1 - Semi-transparent at 100+
        public bool HasHeroicChargeBoots { get; set; }      // T2 - Dash attack (consume 80)
        public bool HasInfernalMeteorStride { get; set; }   // T3 - Impact crater on landing
        public bool HasEnigmasPhaseShift { get; set; }      // T4 - Teleport (consume 100)
        public bool HasSwansEternalGlide { get; set; }      // T5 - Slower decay, infinite flight at max
        public bool HasFatesCosmicVelocity { get; set; }    // T6 - Max 150, time slow at max
        
        // Post-Fate Chain Extension (T7-T10)
        public bool HasNocturnalPhantomTreads { get; set; } // T7 - Max 175, star dash, constellation trail
        public bool HasInfernalMeteorTreads { get; set; }   // T8 - Max 200, meteor dash, burning trail
        public bool HasJubilantZephyrTreads { get; set; }   // T9 - Max 225, zephyr burst, infinite flight at 175+
        public bool HasEternalVelocityTreads { get; set; }  // T10 - Max 250, lightspeed mode, time manipulation
        
        // ========== MOMENTUM STATE ==========
        public float CurrentMomentum { get; private set; }
        public float MaxMomentum => GetMaxMomentum();
        
        private Vector2 lastPosition;
        private int extraJumpUsed;
        private bool wasOnGround;
        private bool isFalling;
        private float fallStartY;
        
        // Trail timers
        private int fireTrailTimer;
        private int iceTrailTimer;
        
        // Ability cooldowns
        private int dashCooldown;
        private int teleportCooldown;
        private int impactCooldown;
        
        public override void ResetEffects()
        {
            HasVelocityBand = false;
            HasSpringZephyrBoots = false;
            HasSolarBlitzTreads = false;
            HasHarvestPhantomStride = false;
            HasPermafrostAvalancheStep = false;
            HasVivaldisSeasonalSprint = false;
            HasMoonlitPhantomsRush = false;
            HasHeroicChargeBoots = false;
            HasInfernalMeteorStride = false;
            HasEnigmasPhaseShift = false;
            HasSwansEternalGlide = false;
            HasFatesCosmicVelocity = false;
            
            // Post-Fate T7-T10
            HasNocturnalPhantomTreads = false;
            HasInfernalMeteorTreads = false;
            HasJubilantZephyrTreads = false;
            HasEternalVelocityTreads = false;
        }
        
        /// <summary>
        /// Check if any mobility accessory is equipped
        /// </summary>
        public bool HasAnyMobilityAccessory =>
            HasVelocityBand || HasSpringZephyrBoots || HasSolarBlitzTreads ||
            HasHarvestPhantomStride || HasPermafrostAvalancheStep || HasVivaldisSeasonalSprint ||
            HasMoonlitPhantomsRush || HasHeroicChargeBoots || HasInfernalMeteorStride ||
            HasEnigmasPhaseShift || HasSwansEternalGlide || HasFatesCosmicVelocity ||
            HasNocturnalPhantomTreads || HasInfernalMeteorTreads || HasJubilantZephyrTreads ||
            HasEternalVelocityTreads;
        
        /// <summary>
        /// Get max momentum based on equipped accessory
        /// </summary>
        private float GetMaxMomentum()
        {
            // Post-Fate T7-T10
            if (HasEternalVelocityTreads) return 250f;
            if (HasJubilantZephyrTreads) return 225f;
            if (HasInfernalMeteorTreads) return 200f;
            if (HasNocturnalPhantomTreads) return 175f;
            
            // Post-Moon Lord T6
            if (HasFatesCosmicVelocity) return 150f;
            if (HasVivaldisSeasonalSprint || HasMoonlitPhantomsRush || HasHeroicChargeBoots ||
                HasInfernalMeteorStride || HasEnigmasPhaseShift || HasSwansEternalGlide)
                return 120f;
            return 100f;
        }
        
        /// <summary>
        /// Get momentum decay rate per second
        /// </summary>
        private float GetDecayRate()
        {
            float baseRate = 5f; // 5 per second when standing still
            
            // Eternal Velocity Treads: No decay during boss fights
            if (HasEternalVelocityTreads && Main.CurrentFrameFlags.AnyActiveBossNPC)
                return 0f;
            
            // Jubilant Zephyr Treads and above: 50% slower decay
            if (HasJubilantZephyrTreads || HasEternalVelocityTreads)
                baseRate *= 0.5f;
            // Swan's Eternal Glide and above: 50% slower decay
            else if (HasSwansEternalGlide || HasFatesCosmicVelocity || 
                     HasNocturnalPhantomTreads || HasInfernalMeteorTreads)
                baseRate *= 0.5f;
            
            return baseRate;
        }
        
        public override void PostUpdate()
        {
            if (!HasAnyMobilityAccessory)
            {
                CurrentMomentum = 0;
                return;
            }
            
            // Update cooldowns
            if (dashCooldown > 0) dashCooldown--;
            if (teleportCooldown > 0) teleportCooldown--;
            if (impactCooldown > 0) impactCooldown--;
            
            // Calculate movement
            float distanceMoved = Vector2.Distance(Player.Center, lastPosition);
            lastPosition = Player.Center;
            
            // Build or decay momentum
            if (distanceMoved > 1f) // Player is moving
            {
                // Build momentum based on movement speed
                float buildRate = distanceMoved * 0.15f;
                CurrentMomentum = Math.Min(CurrentMomentum + buildRate, MaxMomentum);
            }
            else // Standing still
            {
                // Decay momentum
                CurrentMomentum = Math.Max(0, CurrentMomentum - GetDecayRate() / 60f);
            }
            
            // Track falling state for meteor impact
            bool isOnGround = Player.velocity.Y == 0;
            if (!isOnGround && Player.velocity.Y > 0)
            {
                if (!isFalling)
                {
                    isFalling = true;
                    fallStartY = Player.position.Y;
                }
            }
            
            // Check for landing with meteor stride/treads
            if (isFalling && isOnGround)
            {
                float fallDistance = Player.position.Y - fallStartY;
                if (fallDistance > 200f && CurrentMomentum >= 100f && impactCooldown <= 0 &&
                    (HasInfernalMeteorStride || HasEnigmasPhaseShift || HasSwansEternalGlide || HasFatesCosmicVelocity ||
                     HasNocturnalPhantomTreads || HasInfernalMeteorTreads || HasJubilantZephyrTreads || HasEternalVelocityTreads))
                {
                    CreateMeteorImpact();
                    impactCooldown = 60; // 1 second cooldown
                }
                isFalling = false;
            }
            
            wasOnGround = isOnGround;
            
            // Apply momentum effects
            ApplyMomentumEffects();
        }
        
        /// <summary>
        /// Apply all momentum-based effects
        /// </summary>
        private void ApplyMomentumEffects()
        {
            // Spring Zephyr Boots: +10% speed at 50+, reset jump at 80+
            if (HasSpringZephyrBoots || HasSolarBlitzTreads || HasHarvestPhantomStride ||
                HasPermafrostAvalancheStep || HasVivaldisSeasonalSprint || HasMoonlitPhantomsRush ||
                HasHeroicChargeBoots || HasInfernalMeteorStride || HasEnigmasPhaseShift ||
                HasSwansEternalGlide || HasFatesCosmicVelocity ||
                HasNocturnalPhantomTreads || HasInfernalMeteorTreads || HasJubilantZephyrTreads || HasEternalVelocityTreads)
            {
                if (CurrentMomentum >= 50f)
                {
                    Player.moveSpeed += 0.1f;
                    Player.maxRunSpeed += 0.5f;
                }
                
                // Reset extra jump at 80+ momentum
                bool onGround = Player.velocity.Y == 0;
                if (CurrentMomentum >= 80f && Player.velocity.Y != 0 && !onGround)
                {
                    if (Player.jump == 0 && extraJumpUsed < 1)
                    {
                        Player.RefreshExtraJumps();
                        extraJumpUsed++;
                    }
                }
                else if (onGround)
                {
                    extraJumpUsed = 0;
                }
            }
            
            // Solar Blitz Treads: Fire trail at 70+
            if ((HasSolarBlitzTreads || HasHarvestPhantomStride || HasPermafrostAvalancheStep ||
                 HasVivaldisSeasonalSprint || HasMoonlitPhantomsRush || HasHeroicChargeBoots ||
                 HasInfernalMeteorStride || HasEnigmasPhaseShift || HasSwansEternalGlide ||
                 HasFatesCosmicVelocity || HasNocturnalPhantomTreads || HasInfernalMeteorTreads ||
                 HasJubilantZephyrTreads || HasEternalVelocityTreads) && CurrentMomentum >= 70f)
            {
                if (fireTrailTimer++ >= 5)
                {
                    fireTrailTimer = 0;
                    SpawnFireTrail();
                }
            }
            
            // Harvest Phantom Stride: Phase through enemies at 80+
            if ((HasHarvestPhantomStride || HasPermafrostAvalancheStep || HasVivaldisSeasonalSprint ||
                 HasMoonlitPhantomsRush || HasHeroicChargeBoots || HasInfernalMeteorStride ||
                 HasEnigmasPhaseShift || HasSwansEternalGlide || HasFatesCosmicVelocity ||
                 HasNocturnalPhantomTreads || HasInfernalMeteorTreads || HasJubilantZephyrTreads ||
                 HasEternalVelocityTreads) && 
                CurrentMomentum >= 80f)
            {
                Player.noKnockback = true;
                // Ghost effect handled in ModifyHurt
            }
            
            // Permafrost Avalanche Step: Ice trail at 90+
            if ((HasPermafrostAvalancheStep || HasVivaldisSeasonalSprint || HasMoonlitPhantomsRush ||
                 HasHeroicChargeBoots || HasInfernalMeteorStride || HasEnigmasPhaseShift ||
                 HasSwansEternalGlide || HasFatesCosmicVelocity || HasNocturnalPhantomTreads ||
                 HasInfernalMeteorTreads || HasJubilantZephyrTreads || HasEternalVelocityTreads) && CurrentMomentum >= 90f)
            {
                if (iceTrailTimer++ >= 8)
                {
                    iceTrailTimer = 0;
                    SpawnIceTrail();
                }
            }
            
            // Moonlit Phantom's Rush: Semi-transparent at 100+
            if ((HasMoonlitPhantomsRush || HasHeroicChargeBoots || HasInfernalMeteorStride ||
                 HasEnigmasPhaseShift || HasSwansEternalGlide || HasFatesCosmicVelocity ||
                 HasNocturnalPhantomTreads || HasInfernalMeteorTreads || HasJubilantZephyrTreads ||
                 HasEternalVelocityTreads) && 
                CurrentMomentum >= 100f)
            {
                Player.aggro -= 200; // Enemies target player less
                // Visual transparency handled in draw layer
            }
            
            // Swan's Eternal Glide or Jubilant Zephyr Treads: Infinite flight at 175+ or max momentum
            if ((HasSwansEternalGlide || HasFatesCosmicVelocity) && CurrentMomentum >= MaxMomentum)
            {
                Player.wingTime = Player.wingTimeMax;
            }
            // Jubilant Zephyr Treads and above: Infinite flight at 175+
            else if ((HasJubilantZephyrTreads || HasEternalVelocityTreads) && CurrentMomentum >= 175f)
            {
                Player.wingTime = Player.wingTimeMax;
            }
            
            // Fate's Cosmic Velocity or Eternal Velocity Treads: Time slow at high momentum
            if (HasFatesCosmicVelocity && CurrentMomentum >= 150f)
            {
                ApplyTimeSlowToNearbyEnemies();
            }
            // Eternal Velocity Treads: Time slow at 225+
            else if (HasEternalVelocityTreads && CurrentMomentum >= 225f)
            {
                ApplyTimeSlowToNearbyEnemies();
            }
        }
        
        /// <summary>
        /// Try to execute heroic dash attack (consumes 80 momentum)
        /// </summary>
        public void TryHeroicDash()
        {
            if (!HasHeroicChargeBoots && !HasInfernalMeteorStride && !HasEnigmasPhaseShift &&
                !HasSwansEternalGlide && !HasFatesCosmicVelocity && !HasNocturnalPhantomTreads &&
                !HasInfernalMeteorTreads && !HasJubilantZephyrTreads && !HasEternalVelocityTreads)
                return;
            
            if (CurrentMomentum < 80f || dashCooldown > 0)
                return;
            
            CurrentMomentum -= 80f;
            dashCooldown = 45; // 0.75s cooldown
            
            // Dash in movement direction
            Vector2 dashDir = Player.velocity.SafeNormalize(Player.direction == 1 ? Vector2.UnitX : -Vector2.UnitX);
            Player.velocity = dashDir * 25f;
            
            // Damage enemies in path
            Rectangle dashHitbox = Player.Hitbox;
            dashHitbox.Inflate(30, 30);
            
            int damage = (int)(Player.GetDamage(DamageClass.Generic).ApplyTo(50));
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && !npc.dontTakeDamage && dashHitbox.Intersects(npc.Hitbox))
                {
                    int dir = npc.Center.X > Player.Center.X ? 1 : -1;
                    Player.ApplyDamageToNPC(npc, damage, 10f, dir, crit: false);
                }
            }
            
            SoundEngine.PlaySound(SoundID.Item66, Player.Center);
            
            // Brief invincibility
            Player.immune = true;
            Player.immuneTime = 20;
        }
        
        /// <summary>
        /// Try to execute phase shift teleport (consumes 100 momentum)
        /// </summary>
        public void TryPhaseShift()
        {
            if (!HasEnigmasPhaseShift && !HasSwansEternalGlide && !HasFatesCosmicVelocity &&
                !HasNocturnalPhantomTreads && !HasInfernalMeteorTreads && !HasJubilantZephyrTreads &&
                !HasEternalVelocityTreads)
                return;
            
            if (CurrentMomentum < 100f || teleportCooldown > 0)
                return;
            
            CurrentMomentum -= 100f;
            teleportCooldown = 60; // 1s cooldown
            
            // Teleport in movement direction
            Vector2 teleportDir = Player.velocity.SafeNormalize(Player.direction == 1 ? Vector2.UnitX : -Vector2.UnitX);
            float teleportDist = 200f; // 12.5 tiles
            Vector2 targetPos = Player.Center + teleportDir * teleportDist;
            
            // Find valid position (not inside tiles)
            for (int attempt = 0; attempt < 10; attempt++)
            {
                Vector2 testPos = Player.Center + teleportDir * (teleportDist - attempt * 16f);
                Point tilePos = testPos.ToTileCoordinates();
                
                if (!WorldGen.SolidTile(tilePos.X, tilePos.Y) && !WorldGen.SolidTile(tilePos.X, tilePos.Y + 1))
                {
                    targetPos = testPos;
                    break;
                }
            }
            
            Player.Teleport(targetPos, TeleportationStyleID.RodOfDiscord);
            
            SoundEngine.PlaySound(SoundID.Item8, targetPos);
        }
        
        /// <summary>
        /// Create meteor impact crater
        /// </summary>
        private void CreateMeteorImpact()
        {
            // Damage nearby enemies
            float impactRadius = 150f;
            int damage = (int)(Player.GetDamage(DamageClass.Generic).ApplyTo(80));
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && !npc.dontTakeDamage)
                {
                    float dist = Vector2.Distance(Player.Bottom, npc.Center);
                    if (dist <= impactRadius)
                    {
                        int dir = npc.Center.X > Player.Center.X ? 1 : -1;
                        float damageMult = 1f - (dist / impactRadius) * 0.5f;
                        Player.ApplyDamageToNPC(npc, (int)(damage * damageMult), 15f, dir, crit: false);
                    }
                }
            }
            
            SoundEngine.PlaySound(SoundID.Item14, Player.Center);
            Player.GetModPlayer<ScreenShakePlayer>()?.AddShake(10f, 15);
        }
        
        /// <summary>
        /// Apply time slow to nearby enemies (Fate ability)
        /// </summary>
        private void ApplyTimeSlowToNearbyEnemies()
        {
            float slowRadius = 300f;
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && !npc.boss)
                {
                    float dist = Vector2.Distance(Player.Center, npc.Center);
                    if (dist <= slowRadius)
                    {
                        // Apply Slow debuff
                        npc.AddBuff(BuffID.Slow, 2);
                        
                        // Reduce velocity directly for more dramatic effect
                        npc.velocity *= 0.92f;
                    }
                }
            }
        }
        
        // ========== TRAIL EFFECTS ==========
        
        private void SpawnFireTrail()
        {
            if (Player.velocity.Y != 0) return;
            
            Vector2 trailPos = Player.Bottom - new Vector2(0, 4);
            
            // Damage enemies that touch trail (handled via projectile or direct check)
            Rectangle trailHitbox = new Rectangle((int)trailPos.X - 10, (int)trailPos.Y - 5, 20, 10);
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && !npc.dontTakeDamage && trailHitbox.Intersects(npc.Hitbox))
                {
                    if (Main.rand.NextBool(15)) // Occasional damage tick
                    {
                        int dir = npc.Center.X > Player.Center.X ? 1 : -1;
                        Player.ApplyDamageToNPC(npc, 15, 2f, dir, crit: false);
                    }
                }
            }
        }
        
        private void SpawnIceTrail()
        {
            if (Player.velocity.Y != 0) return;
            
            Vector2 trailPos = Player.Bottom - new Vector2(0, 4);
            
            // Slow enemies that touch trail
            Rectangle trailHitbox = new Rectangle((int)trailPos.X - 15, (int)trailPos.Y - 5, 30, 10);
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && trailHitbox.Intersects(npc.Hitbox))
                {
                    npc.AddBuff(BuffID.Chilled, 60);
                    npc.AddBuff(BuffID.Slow, 60);
                }
            }
        }
        
        // ========== HURT / DRAW MODIFIERS ==========
        
        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            // Harvest Phantom Stride and above: Phase through enemies at 80+ momentum
            if ((HasHarvestPhantomStride || HasPermafrostAvalancheStep || HasVivaldisSeasonalSprint ||
                 HasMoonlitPhantomsRush || HasHeroicChargeBoots || HasInfernalMeteorStride ||
                 HasEnigmasPhaseShift || HasSwansEternalGlide || HasFatesCosmicVelocity ||
                 HasNocturnalPhantomTreads || HasInfernalMeteorTreads || HasJubilantZephyrTreads ||
                 HasEternalVelocityTreads) && 
                CurrentMomentum >= 80f)
            {
                // Reduce contact damage significantly (ghost running)
                modifiers.FinalDamage *= 0.25f;
            }
        }
        
        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        {
            // Moonlit Phantom's Rush and above: Semi-transparent at 100+ momentum
            if ((HasMoonlitPhantomsRush || HasHeroicChargeBoots || HasInfernalMeteorStride ||
                 HasEnigmasPhaseShift || HasSwansEternalGlide || HasFatesCosmicVelocity ||
                 HasNocturnalPhantomTreads || HasInfernalMeteorTreads || HasJubilantZephyrTreads ||
                 HasEternalVelocityTreads) && 
                CurrentMomentum >= 100f)
            {
                // Make player semi-transparent
                float alpha = 0.5f + (1f - CurrentMomentum / MaxMomentum) * 0.3f;
                drawInfo.colorArmorBody *= alpha;
                drawInfo.colorArmorHead *= alpha;
                drawInfo.colorArmorLegs *= alpha;
                drawInfo.colorBodySkin *= alpha;
                drawInfo.colorHead *= alpha;
            }
        }
    }
}
