using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        
        // VFX timers
        private int speedLineTimer;
        
        // Theme colors
        private static readonly Color MomentumGold = new Color(255, 220, 100);
        private static readonly Color SpringPink = new Color(255, 180, 200);
        private static readonly Color SolarOrange = new Color(255, 140, 50);
        private static readonly Color HarvestPurple = new Color(180, 120, 200);
        private static readonly Color FrostBlue = new Color(150, 200, 255);
        private static readonly Color MoonlitPurple = new Color(150, 120, 200);
        private static readonly Color HeroicScarlet = new Color(200, 80, 80);
        private static readonly Color InfernalOrange = new Color(255, 100, 30);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color SwanWhite = new Color(240, 245, 255);
        private static readonly Color FateCrimson = new Color(180, 40, 80);
        
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
        }
        
        /// <summary>
        /// Check if any mobility accessory is equipped
        /// </summary>
        public bool HasAnyMobilityAccessory =>
            HasVelocityBand || HasSpringZephyrBoots || HasSolarBlitzTreads ||
            HasHarvestPhantomStride || HasPermafrostAvalancheStep || HasVivaldisSeasonalSprint ||
            HasMoonlitPhantomsRush || HasHeroicChargeBoots || HasInfernalMeteorStride ||
            HasEnigmasPhaseShift || HasSwansEternalGlide || HasFatesCosmicVelocity;
        
        /// <summary>
        /// Get max momentum based on equipped accessory
        /// </summary>
        private float GetMaxMomentum()
        {
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
            
            // Swan's Eternal Glide reduces decay by 50%
            if (HasSwansEternalGlide || HasFatesCosmicVelocity)
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
            
            // Check for landing with meteor stride
            if (isFalling && isOnGround)
            {
                float fallDistance = Player.position.Y - fallStartY;
                if (fallDistance > 200f && CurrentMomentum >= 100f && impactCooldown <= 0 &&
                    (HasInfernalMeteorStride || HasEnigmasPhaseShift || HasSwansEternalGlide || HasFatesCosmicVelocity))
                {
                    CreateMeteorImpact();
                    impactCooldown = 60; // 1 second cooldown
                }
                isFalling = false;
            }
            
            wasOnGround = isOnGround;
            
            // Apply momentum effects
            ApplyMomentumEffects();
            
            // Spawn visual effects
            SpawnMomentumVFX();
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
                HasSwansEternalGlide || HasFatesCosmicVelocity)
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
                 HasFatesCosmicVelocity) && CurrentMomentum >= 70f)
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
                 HasEnigmasPhaseShift || HasSwansEternalGlide || HasFatesCosmicVelocity) && 
                CurrentMomentum >= 80f)
            {
                Player.noKnockback = true;
                // Ghost effect handled in ModifyHurt
            }
            
            // Permafrost Avalanche Step: Ice trail at 90+
            if ((HasPermafrostAvalancheStep || HasVivaldisSeasonalSprint || HasMoonlitPhantomsRush ||
                 HasHeroicChargeBoots || HasInfernalMeteorStride || HasEnigmasPhaseShift ||
                 HasSwansEternalGlide || HasFatesCosmicVelocity) && CurrentMomentum >= 90f)
            {
                if (iceTrailTimer++ >= 8)
                {
                    iceTrailTimer = 0;
                    SpawnIceTrail();
                }
            }
            
            // Moonlit Phantom's Rush: Semi-transparent at 100+
            if ((HasMoonlitPhantomsRush || HasHeroicChargeBoots || HasInfernalMeteorStride ||
                 HasEnigmasPhaseShift || HasSwansEternalGlide || HasFatesCosmicVelocity) && 
                CurrentMomentum >= 100f)
            {
                Player.aggro -= 200; // Enemies target player less
                // Visual transparency handled in draw layer
            }
            
            // Swan's Eternal Glide: Infinite flight at max momentum
            if ((HasSwansEternalGlide || HasFatesCosmicVelocity) && CurrentMomentum >= MaxMomentum)
            {
                Player.wingTime = Player.wingTimeMax;
            }
            
            // Fate's Cosmic Velocity: Time slow at 150 momentum
            if (HasFatesCosmicVelocity && CurrentMomentum >= 150f)
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
                !HasSwansEternalGlide && !HasFatesCosmicVelocity)
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
                    
                    // VFX on hit
                    for (int d = 0; d < 8; d++)
                    {
                        Dust dust = Dust.NewDustPerfect(npc.Center, DustID.RedTorch, 
                            Main.rand.NextVector2Circular(5f, 5f));
                        dust.noGravity = true;
                        dust.scale = 1.2f;
                    }
                }
            }
            
            // VFX
            SoundEngine.PlaySound(SoundID.Item66, Player.Center);
            SpawnDashVFX();
            
            // Brief invincibility
            Player.immune = true;
            Player.immuneTime = 20;
        }
        
        /// <summary>
        /// Try to execute phase shift teleport (consumes 100 momentum)
        /// </summary>
        public void TryPhaseShift()
        {
            if (!HasEnigmasPhaseShift && !HasSwansEternalGlide && !HasFatesCosmicVelocity)
                return;
            
            if (CurrentMomentum < 100f || teleportCooldown > 0)
                return;
            
            CurrentMomentum -= 100f;
            teleportCooldown = 60; // 1s cooldown
            
            // Teleport in movement direction
            Vector2 teleportDir = Player.velocity.SafeNormalize(Player.direction == 1 ? Vector2.UnitX : -Vector2.UnitX);
            float teleportDist = 200f; // 12.5 tiles
            
            // VFX at departure
            SpawnTeleportVFX(Player.Center, true);
            
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
            
            // VFX at arrival
            SpawnTeleportVFX(targetPos, false);
            
            SoundEngine.PlaySound(SoundID.Item8, targetPos);
        }
        
        /// <summary>
        /// Create meteor impact crater
        /// </summary>
        private void CreateMeteorImpact()
        {
            // VFX explosion
            for (int i = 0; i < 30; i++)
            {
                float angle = MathHelper.TwoPi * i / 30f;
                Vector2 dustVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 12f);
                Dust dust = Dust.NewDustPerfect(Player.Bottom, DustID.Torch, dustVel);
                dust.noGravity = true;
                dust.scale = 1.5f;
                dust.color = InfernalOrange;
            }
            
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
                        
                        // Occasional time-slow VFX
                        if (Main.rand.NextBool(30))
                        {
                            Dust dust = Dust.NewDustPerfect(npc.Center + Main.rand.NextVector2Circular(20f, 20f),
                                DustID.PurpleTorch, Vector2.Zero);
                            dust.noGravity = true;
                            dust.scale = 0.6f;
                            dust.color = FateCrimson;
                        }
                    }
                }
            }
        }
        
        // ========== TRAIL EFFECTS ==========
        
        private void SpawnFireTrail()
        {
            if (Player.velocity.Y != 0) return;
            
            Vector2 trailPos = Player.Bottom - new Vector2(0, 4);
            
            // Create fire dust
            for (int i = 0; i < 2; i++)
            {
                Dust dust = Dust.NewDustPerfect(trailPos + Main.rand.NextVector2Circular(8f, 4f),
                    DustID.Torch, new Vector2(0, -Main.rand.NextFloat(1f, 2f)));
                dust.noGravity = true;
                dust.scale = 1.0f;
            }
            
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
            
            // Create frost dust
            for (int i = 0; i < 2; i++)
            {
                Dust dust = Dust.NewDustPerfect(trailPos + Main.rand.NextVector2Circular(10f, 4f),
                    DustID.IceTorch, new Vector2(0, -Main.rand.NextFloat(0.5f, 1.5f)));
                dust.noGravity = true;
                dust.scale = 0.9f;
                dust.color = FrostBlue;
            }
            
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
        
        // ========== VFX METHODS ==========
        
        private void SpawnMomentumVFX()
        {
            if (CurrentMomentum < 30f) return;
            
            // Speed lines at high momentum
            if (speedLineTimer++ >= 3 && CurrentMomentum >= 60f)
            {
                speedLineTimer = 0;
                
                Color lineColor = GetMomentumColor();
                Vector2 linePos = Player.Center + Main.rand.NextVector2Circular(15f, 25f);
                Vector2 lineVel = -Player.velocity.SafeNormalize(Vector2.UnitX) * 3f;
                
                Dust dust = Dust.NewDustPerfect(linePos, DustID.MagicMirror, lineVel);
                dust.noGravity = true;
                dust.scale = 0.5f + (CurrentMomentum / MaxMomentum) * 0.5f;
                dust.color = lineColor;
            }
            
            // Additional VFX at max momentum
            if (CurrentMomentum >= MaxMomentum && Main.rand.NextBool(5))
            {
                Color maxColor = HasFatesCosmicVelocity ? FateCrimson : 
                                 HasSwansEternalGlide ? SwanWhite : MomentumGold;
                
                Vector2 auraPos = Player.Center + Main.rand.NextVector2Circular(25f, 35f);
                Dust auraDust = Dust.NewDustPerfect(auraPos, DustID.MagicMirror, Vector2.Zero);
                auraDust.noGravity = true;
                auraDust.scale = 0.8f;
                auraDust.color = maxColor;
            }
        }
        
        private void SpawnDashVFX()
        {
            Vector2 dashDir = Player.velocity.SafeNormalize(Vector2.UnitX);
            
            for (int i = 0; i < 15; i++)
            {
                Vector2 dustPos = Player.Center - dashDir * i * 8f;
                Dust dust = Dust.NewDustPerfect(dustPos + Main.rand.NextVector2Circular(10f, 10f),
                    DustID.RedTorch, -dashDir * 2f);
                dust.noGravity = true;
                dust.scale = 1.3f - i * 0.06f;
                dust.color = HeroicScarlet;
            }
        }
        
        private void SpawnTeleportVFX(Vector2 position, bool isDeparture)
        {
            Color vfxColor = isDeparture ? EnigmaPurple : new Color(80, 200, 120);
            
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Vector2 dustVel = angle.ToRotationVector2() * (isDeparture ? 5f : -3f);
                Dust dust = Dust.NewDustPerfect(position, DustID.PurpleTorch, dustVel);
                dust.noGravity = true;
                dust.scale = 1.2f;
                dust.color = vfxColor;
            }
        }
        
        private Color GetMomentumColor()
        {
            if (HasFatesCosmicVelocity) return FateCrimson;
            if (HasSwansEternalGlide) return SwanWhite;
            if (HasEnigmasPhaseShift) return EnigmaPurple;
            if (HasInfernalMeteorStride) return InfernalOrange;
            if (HasHeroicChargeBoots) return HeroicScarlet;
            if (HasMoonlitPhantomsRush) return MoonlitPurple;
            if (HasVivaldisSeasonalSprint) return GetSeasonalColor();
            if (HasPermafrostAvalancheStep) return FrostBlue;
            if (HasHarvestPhantomStride) return HarvestPurple;
            if (HasSolarBlitzTreads) return SolarOrange;
            if (HasSpringZephyrBoots) return SpringPink;
            return MomentumGold;
        }
        
        private Color GetSeasonalColor()
        {
            // Get seasonal color based on in-game time/season
            int season = (int)(Main.time / 54000) % 4;
            return season switch
            {
                0 => SpringPink,
                1 => SolarOrange,
                2 => HarvestPurple,
                3 => FrostBlue,
                _ => MomentumGold
            };
        }
        
        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            // Harvest Phantom Stride: Phase through enemies at 80+ momentum
            if ((HasHarvestPhantomStride || HasPermafrostAvalancheStep || HasVivaldisSeasonalSprint ||
                 HasMoonlitPhantomsRush || HasHeroicChargeBoots || HasInfernalMeteorStride ||
                 HasEnigmasPhaseShift || HasSwansEternalGlide || HasFatesCosmicVelocity) && 
                CurrentMomentum >= 80f)
            {
                // Reduce contact damage significantly (ghost running)
                modifiers.FinalDamage *= 0.25f;
            }
        }
        
        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        {
            // Moonlit Phantom's Rush: Semi-transparent at 100+ momentum
            if ((HasMoonlitPhantomsRush || HasHeroicChargeBoots || HasInfernalMeteorStride ||
                 HasEnigmasPhaseShift || HasSwansEternalGlide || HasFatesCosmicVelocity) && 
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
