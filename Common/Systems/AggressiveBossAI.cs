using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// AGGRESSIVE BOSS AI FRAMEWORK - Calamity-Inspired Combat System
    /// 
    /// This framework provides utilities for creating ENGAGING, THREATENING boss fights:
    /// - Teleportation/Dashing to prevent kiting
    /// - Mirage/Clone attacks for multi-entity pressure
    /// - Projectile Barrages that fill the screen
    /// - Combo Attack Chaining for relentless assault
    /// - Distance Punishment for cowardly players
    /// - Rhythmic Attack Patterns that feel musical
    /// 
    /// DESIGN PHILOSOPHY:
    /// 1. Bosses should PURSUE the player, not float passively
    /// 2. Attacks should DEMAND dodging, not be easily ignored
    /// 3. Projectiles should FLOOD the arena, creating beautiful chaos
    /// 4. Combat should feel RHYTHMIC - tension and release
    /// 5. Players should feel ENGAGED, not just waiting for openings
    /// </summary>
    public static class AggressiveBossAI
    {
        #region Constants
        
        // Teleport thresholds
        public const float TELEPORT_DISTANCE_FAR = 900f;        // Teleport if player is this far
        public const float TELEPORT_DISTANCE_MEDIUM = 600f;     // Teleport during aggression
        public const float CHASE_SPEED_BASE = 18f;              // Base aggressive chase speed
        public const float DASH_SPEED_BASE = 32f;               // Base dash attack speed
        
        // Attack timing
        public const int ATTACK_TELEGRAPH_SHORT = 20;           // Fast attack warning (0.33s)
        public const int ATTACK_TELEGRAPH_MEDIUM = 35;          // Standard warning (0.58s)
        public const int ATTACK_TELEGRAPH_LONG = 50;            // Heavy attack warning (0.83s)
        public const int COMBO_TRANSITION_TIME = 8;             // Time between combo attacks
        
        // Projectile barrage settings
        public const int BARRAGE_LIGHT = 5;                     // Light barrage count
        public const int BARRAGE_MEDIUM = 12;                   // Medium barrage count
        public const int BARRAGE_HEAVY = 20;                    // Heavy barrage count
        public const int BARRAGE_OVERWHELMING = 36;             // Screen-filling barrage
        
        #endregion
        
        #region Teleportation System
        
        /// <summary>
        /// Checks if boss should teleport based on distance to player.
        /// Returns true if teleport should occur.
        /// </summary>
        public static bool ShouldTeleport(NPC npc, Player target, float threshold = TELEPORT_DISTANCE_FAR)
        {
            return Vector2.Distance(npc.Center, target.Center) > threshold;
        }
        
        /// <summary>
        /// Performs an aggressive teleport to the player with VFX.
        /// Boss appears in a threatening position ready to attack.
        /// </summary>
        public static void AggressiveTeleport(NPC npc, Player target, Color primaryColor, Color secondaryColor, 
            float distance = 250f, bool createMirage = false, float soundVolume = 1.2f)
        {
            Vector2 oldPos = npc.Center;
            
            // Departure VFX
            SpawnTeleportVFX(oldPos, primaryColor, secondaryColor, true);
            
            // Calculate arrival position - either above, to the side, or behind the player
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            // Bias towards positions in front of player movement
            if (target.velocity.X != 0)
            {
                float playerDirection = Math.Sign(target.velocity.X);
                angle = MathHelper.Lerp(angle, playerDirection > 0 ? MathHelper.Pi : 0, 0.5f);
            }
            
            Vector2 arrivalOffset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * distance;
            Vector2 newPos = target.Center + arrivalOffset;
            
            // Ensure valid position
            newPos = Collision.TileCollision(newPos - new Vector2(npc.width/2, npc.height/2), 
                Vector2.Zero, npc.width, npc.height);
            
            npc.Center = newPos;
            npc.velocity = Vector2.Zero;
            npc.netUpdate = true;
            
            // Arrival VFX
            SpawnTeleportVFX(newPos, primaryColor, secondaryColor, false);
            
            // Create mirage at old position if enabled
            if (createMirage)
            {
                SpawnMirageEffect(oldPos, npc, primaryColor, secondaryColor, 45);
            }
            
            // Sound
            SoundEngine.PlaySound(SoundID.Item8 with { Pitch = -0.3f, Volume = soundVolume }, newPos);
        }
        
        /// <summary>
        /// Teleports to a specific relative position around the target.
        /// </summary>
        public static void TeleportToPosition(NPC npc, Vector2 targetPos, Color primaryColor, Color secondaryColor)
        {
            Vector2 oldPos = npc.Center;
            SpawnTeleportVFX(oldPos, primaryColor, secondaryColor, true);
            
            npc.Center = targetPos;
            npc.velocity = Vector2.Zero;
            npc.netUpdate = true;
            
            SpawnTeleportVFX(targetPos, primaryColor, secondaryColor, false);
            SoundEngine.PlaySound(SoundID.Item8 with { Pitch = -0.3f, Volume = 1.2f }, targetPos);
        }
        
        /// <summary>
        /// Spawns teleport visual effects at a position.
        /// </summary>
        public static void SpawnTeleportVFX(Vector2 position, Color primary, Color secondary, bool isDeparture)
        {
            // Central flash
            CustomParticles.GenericFlare(position, Color.White, isDeparture ? 0.8f : 1.2f, 15);
            CustomParticles.GenericFlare(position, primary, isDeparture ? 0.6f : 1.0f, 18);
            
            // Expanding rings
            for (int i = 0; i < 4; i++)
            {
                float scale = 0.3f + i * 0.2f;
                int lifetime = 12 + i * 4;
                Color ringColor = Color.Lerp(primary, secondary, i / 4f);
                CustomParticles.HaloRing(position, ringColor, scale, lifetime);
            }
            
            // Particle burst
            int particleCount = isDeparture ? 12 : 18;
            for (int i = 0; i < particleCount; i++)
            {
                float angle = MathHelper.TwoPi * i / particleCount;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Color particleColor = Color.Lerp(primary, secondary, Main.rand.NextFloat());
                
                var particle = new GenericGlowParticle(position, vel, particleColor, 
                    Main.rand.NextFloat(0.2f, 0.4f), Main.rand.Next(20, 35), true);
                MagnumParticleHandler.SpawnParticle(particle);
            }
        }
        
        #endregion
        
        #region Mirage/Clone System
        
        /// <summary>
        /// Data structure for mirage attacks.
        /// </summary>
        public struct MirageData
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Rotation;
            public float Scale;
            public float Alpha;
            public int Lifetime;
            public int Timer;
            public Color Color;
            public int AttackFrame;
            
            public bool IsActive => Timer < Lifetime;
            public float Progress => (float)Timer / Lifetime;
        }
        
        private static List<MirageData> activeMirages = new List<MirageData>();
        
        /// <summary>
        /// Creates a mirage/afterimage effect that persists and can attack.
        /// </summary>
        public static void SpawnMirageEffect(Vector2 position, NPC sourceNPC, Color primary, Color secondary, int lifetime)
        {
            MirageData mirage = new MirageData
            {
                Position = position,
                Velocity = Vector2.Zero,
                Rotation = sourceNPC.rotation,
                Scale = sourceNPC.scale,
                Alpha = 0.7f,
                Lifetime = lifetime,
                Timer = 0,
                Color = primary,
                AttackFrame = -1
            };
            
            activeMirages.Add(mirage);
            
            // Spawn VFX at mirage position
            CustomParticles.GenericFlare(position, primary * 0.8f, 0.6f, 20);
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 particlePos = position + angle.ToRotationVector2() * 40f;
                CustomParticles.GenericFlare(particlePos, Color.Lerp(primary, secondary, i/8f) * 0.5f, 0.3f, 15);
            }
        }
        
        /// <summary>
        /// Creates multiple mirages in a formation around a center point.
        /// Each mirage will dash toward the player in sequence.
        /// </summary>
        public static void SpawnMirageFormation(NPC sourceNPC, Player target, Color primary, Color secondary,
            int mirageCount, float radius, int attackDelay, int projectileType, int damage)
        {
            for (int i = 0; i < mirageCount; i++)
            {
                float angle = MathHelper.TwoPi * i / mirageCount;
                Vector2 miragePos = target.Center + angle.ToRotationVector2() * radius;
                
                // Spawn mirage
                SpawnMirageEffect(miragePos, sourceNPC, primary, secondary, 60 + attackDelay * i);
                
                // Schedule projectile attack from mirage position
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    // Delayed projectile spawn using timer
                    int delay = 30 + attackDelay * i;
                    ScheduleMirageAttack(miragePos, target.Center, projectileType, damage, delay, primary);
                }
            }
        }
        
        private static void ScheduleMirageAttack(Vector2 from, Vector2 toward, int projectileType, int damage, int delay, Color color)
        {
            // This would need to be tracked in the boss AI itself
            // For now, spawn immediately with staggered approach
            Vector2 vel = (toward - from).SafeNormalize(Vector2.Zero) * 14f;
            
            // VFX at launch point
            CustomParticles.GenericFlare(from, color, 0.5f, 12);
            CustomParticles.HaloRing(from, color * 0.7f, 0.3f, 10);
        }
        
        #endregion
        
        #region Aggressive Dash System
        
        /// <summary>
        /// Performs a rapid chain of dashes toward/around the player.
        /// Returns true while dashing is active.
        /// </summary>
        public static bool PerformChainDash(NPC npc, Player target, ref int dashCount, ref int dashTimer, 
            ref Vector2 dashTarget, int maxDashes, float dashSpeed, int dashDuration, 
            Color trailColor, bool predictMovement = true)
        {
            dashTimer++;
            
            if (dashTimer < dashDuration)
            {
                // During dash - move toward target
                Vector2 direction = (dashTarget - npc.Center).SafeNormalize(Vector2.Zero);
                npc.velocity = direction * dashSpeed;
                
                // Trail effects
                if (dashTimer % 2 == 0)
                {
                    CustomParticles.GenericFlare(npc.Center, trailColor, 0.4f, 12);
                    var trail = new GenericGlowParticle(npc.Center, -npc.velocity * 0.1f, 
                        trailColor * 0.6f, 0.3f, 15, true);
                    MagnumParticleHandler.SpawnParticle(trail);
                }
                
                return true;
            }
            
            // Between dashes - brief pause and retarget
            if (dashTimer < dashDuration + COMBO_TRANSITION_TIME)
            {
                npc.velocity *= 0.85f;
                return true;
            }
            
            // Start next dash
            dashCount++;
            if (dashCount >= maxDashes)
            {
                return false; // Dashing complete
            }
            
            dashTimer = 0;
            
            // Calculate new dash target
            if (predictMovement && target.velocity.Length() > 2f)
            {
                // Predict where player will be
                dashTarget = target.Center + target.velocity * 20f;
            }
            else
            {
                // Mix of direct pursuit and flanking
                float angle = Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4);
                Vector2 toPlayer = (target.Center - npc.Center).SafeNormalize(Vector2.Zero);
                dashTarget = target.Center + toPlayer.RotatedBy(angle) * -80f;
            }
            
            // Dash start VFX
            CustomParticles.GenericFlare(npc.Center, Color.White, 0.7f, 10);
            CustomParticles.HaloRing(npc.Center, trailColor, 0.4f, 12);
            SoundEngine.PlaySound(SoundID.DD2_BetsyFlameBreath with { Pitch = 0.3f, Volume = 0.8f }, npc.Center);
            
            return true;
        }
        
        /// <summary>
        /// Performs a teleport-dash: teleport near player then immediately dash through them.
        /// </summary>
        public static void TeleportDash(NPC npc, Player target, Color primary, Color secondary, 
            float teleportDistance, float dashSpeed, int dashDuration, ref int timer)
        {
            if (timer == 0)
            {
                // Teleport to flanking position
                float side = Main.rand.NextBool() ? 1f : -1f;
                Vector2 teleportPos = target.Center + new Vector2(side * teleportDistance, -100f);
                TeleportToPosition(npc, teleportPos, primary, secondary);
                
                // Brief pause for player to react
                npc.velocity = Vector2.Zero;
            }
            else if (timer == 15) // Brief telegraph
            {
                // Start dash
                Vector2 dashDirection = (target.Center - npc.Center).SafeNormalize(Vector2.Zero);
                npc.velocity = dashDirection * dashSpeed;
                
                // Dash start VFX
                CustomParticles.GenericFlare(npc.Center, Color.White, 0.9f, 15);
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f;
                    CustomParticles.GenericFlare(npc.Center + angle.ToRotationVector2() * 30f, 
                        primary, 0.4f, 12);
                }
                SoundEngine.PlaySound(SoundID.DD2_BetsyFlameBreath with { Volume = 1.1f }, npc.Center);
            }
            else if (timer < 15 + dashDuration)
            {
                // During dash - trail effects
                if (timer % 2 == 0)
                {
                    var trail = new GenericGlowParticle(npc.Center, -npc.velocity * 0.15f, 
                        primary * 0.7f, 0.35f, 18, true);
                    MagnumParticleHandler.SpawnParticle(trail);
                }
            }
            else
            {
                // Dash complete - slow down
                npc.velocity *= 0.9f;
            }
            
            timer++;
        }
        
        #endregion
        
        #region Projectile Barrage System
        
        /// <summary>
        /// Fires a spread of projectiles in an arc pattern.
        /// </summary>
        public static void FireSpreadBarrage(Vector2 origin, Player target, int projectileType, int damage,
            int count, float speed, float spreadAngle, Color vfxColor, float knockback = 0f)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            
            Vector2 baseDirection = (target.Center - origin).SafeNormalize(Vector2.Zero);
            float baseAngle = baseDirection.ToRotation();
            float angleStep = spreadAngle / Math.Max(1, count - 1);
            float startAngle = baseAngle - spreadAngle / 2f;
            
            for (int i = 0; i < count; i++)
            {
                float angle = startAngle + angleStep * i;
                Vector2 vel = angle.ToRotationVector2() * speed;
                
                Projectile.NewProjectile(null, origin, vel, projectileType, damage, knockback, Main.myPlayer);
                
                // Muzzle flash
                CustomParticles.GenericFlare(origin + vel.SafeNormalize(Vector2.Zero) * 20f, 
                    vfxColor, 0.3f, 8);
            }
            
            // Central VFX
            CustomParticles.GenericFlare(origin, Color.White, 0.6f, 12);
            CustomParticles.HaloRing(origin, vfxColor, 0.4f, 15);
            SoundEngine.PlaySound(SoundID.Item12 with { Pitch = -0.1f }, origin);
        }
        
        /// <summary>
        /// Fires projectiles in a rotating spiral pattern.
        /// </summary>
        public static void FireSpiralBarrage(Vector2 origin, int projectileType, int damage,
            int count, float speed, float startAngle, float angleOffset, Color vfxColor)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            
            for (int i = 0; i < count; i++)
            {
                float angle = startAngle + angleOffset * i;
                Vector2 vel = angle.ToRotationVector2() * speed;
                
                Projectile.NewProjectile(null, origin, vel, projectileType, damage, 0f, Main.myPlayer);
                
                // Trail effect
                CustomParticles.GenericFlare(origin + vel.SafeNormalize(Vector2.Zero) * 15f, 
                    vfxColor * 0.7f, 0.25f, 6);
            }
            
            // Central pulse
            CustomParticles.GenericFlare(origin, vfxColor, 0.5f, 10);
        }
        
        /// <summary>
        /// Fires a ring of projectiles outward from a point.
        /// </summary>
        public static void FireRingBarrage(Vector2 origin, int projectileType, int damage,
            int count, float speed, float angleOffset, Color vfxColor)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + angleOffset;
                Vector2 vel = angle.ToRotationVector2() * speed;
                
                Projectile.NewProjectile(null, origin, vel, projectileType, damage, 0f, Main.myPlayer);
            }
            
            // Ring VFX
            CustomParticles.GenericFlare(origin, Color.White, 0.8f, 15);
            CustomParticles.HaloRing(origin, vfxColor, 0.6f, 20);
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f }, origin);
        }
        
        /// <summary>
        /// Fires a wall of projectiles that moves toward the player.
        /// </summary>
        public static void FireProjectileWall(Vector2 origin, Vector2 direction, int projectileType, int damage,
            int count, float spacing, float speed, Color vfxColor)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
            float totalWidth = spacing * (count - 1);
            Vector2 startPos = origin - perpendicular * totalWidth / 2f;
            
            for (int i = 0; i < count; i++)
            {
                Vector2 pos = startPos + perpendicular * spacing * i;
                Vector2 vel = direction.SafeNormalize(Vector2.Zero) * speed;
                
                Projectile.NewProjectile(null, pos, vel, projectileType, damage, 0f, Main.myPlayer);
                
                // Spawn effect
                CustomParticles.GenericFlare(pos, vfxColor * 0.6f, 0.3f, 10);
            }
            
            SoundEngine.PlaySound(SoundID.Item73 with { Pitch = -0.2f }, origin);
        }
        
        /// <summary>
        /// Spawns projectiles that rain down from above the player.
        /// </summary>
        public static void SpawnProjectileRain(Player target, int projectileType, int damage,
            int count, float spread, float speed, float heightAbove, Color vfxColor)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            
            for (int i = 0; i < count; i++)
            {
                float xOffset = Main.rand.NextFloat(-spread, spread);
                Vector2 spawnPos = target.Center + new Vector2(xOffset, -heightAbove);
                
                // Aim toward player with slight randomness
                Vector2 toPlayer = (target.Center - spawnPos).SafeNormalize(Vector2.Zero);
                toPlayer = toPlayer.RotatedByRandom(0.15f);
                Vector2 vel = toPlayer * speed;
                
                Projectile.NewProjectile(null, spawnPos, vel, projectileType, damage, 0f, Main.myPlayer);
                
                // Spawn indicator
                CustomParticles.GenericFlare(spawnPos, vfxColor, 0.4f, 8);
            }
            
            // Warning indicator at player
            CustomParticles.HaloRing(target.Center + new Vector2(0, -heightAbove/2), vfxColor * 0.5f, 0.3f, 15);
        }
        
        #endregion
        
        #region Combo Attack System
        
        /// <summary>
        /// Data structure for tracking combo attacks.
        /// </summary>
        public class ComboAttackState
        {
            public int CurrentAttackIndex;
            public int AttackTimer;
            public int ComboLength;
            public bool IsActive;
            public Action<NPC, Player, int>[] AttackSequence;
            public int[] AttackDurations;
            
            public ComboAttackState(int comboLength)
            {
                ComboLength = comboLength;
                AttackSequence = new Action<NPC, Player, int>[comboLength];
                AttackDurations = new int[comboLength];
                CurrentAttackIndex = 0;
                AttackTimer = 0;
                IsActive = false;
            }
        }
        
        /// <summary>
        /// Updates a combo attack sequence. Call every frame during combo.
        /// Returns true while combo is active.
        /// </summary>
        public static bool UpdateComboAttack(ComboAttackState combo, NPC npc, Player target)
        {
            if (!combo.IsActive) return false;
            
            // Execute current attack
            combo.AttackSequence[combo.CurrentAttackIndex]?.Invoke(npc, target, combo.AttackTimer);
            
            combo.AttackTimer++;
            
            // Check if current attack is done
            if (combo.AttackTimer >= combo.AttackDurations[combo.CurrentAttackIndex])
            {
                combo.CurrentAttackIndex++;
                combo.AttackTimer = 0;
                
                // Check if combo is complete
                if (combo.CurrentAttackIndex >= combo.ComboLength)
                {
                    combo.IsActive = false;
                    combo.CurrentAttackIndex = 0;
                    return false;
                }
            }
            
            return true;
        }
        
        #endregion
        
        #region Distance Punishment System
        
        /// <summary>
        /// Gets an aggression multiplier based on distance to player.
        /// Further away = more aggressive (faster, more projectiles).
        /// </summary>
        public static float GetDistanceAggressionMultiplier(NPC npc, Player target)
        {
            float distance = Vector2.Distance(npc.Center, target.Center);
            
            if (distance < 300f) return 1.0f;
            if (distance < 500f) return 1.2f;
            if (distance < 700f) return 1.5f;
            if (distance < 900f) return 1.8f;
            return 2.2f; // Max aggression for extremely far players
        }
        
        /// <summary>
        /// Checks if player is trying to cheese/kite the boss and returns punishment type.
        /// 0 = OK, 1 = Warning, 2 = Punish, 3 = Severe Punish
        /// </summary>
        public static int CheckCheesePunishment(NPC npc, Player target, ref int farAwayTimer, ref int stationaryTimer)
        {
            float distance = Vector2.Distance(npc.Center, target.Center);
            
            // Distance cheese check
            if (distance > TELEPORT_DISTANCE_FAR)
            {
                farAwayTimer++;
                if (farAwayTimer > 120) return 3; // 2 seconds = severe
                if (farAwayTimer > 60) return 2;  // 1 second = punish
                if (farAwayTimer > 30) return 1;  // 0.5 seconds = warning
            }
            else
            {
                farAwayTimer = Math.Max(0, farAwayTimer - 2);
            }
            
            // Stationary camping check
            if (target.velocity.Length() < 2f)
            {
                stationaryTimer++;
                if (stationaryTimer > 180) return 3; // 3 seconds stationary
                if (stationaryTimer > 120) return 2;
                if (stationaryTimer > 60) return 1;
            }
            else
            {
                stationaryTimer = Math.Max(0, stationaryTimer - 3);
            }
            
            return 0;
        }
        
        /// <summary>
        /// Performs punishment action based on cheese level.
        /// </summary>
        public static void ExecutePunishment(NPC npc, Player target, int punishLevel, 
            Color primary, Color secondary, int projectileType, int damage)
        {
            switch (punishLevel)
            {
                case 1: // Warning - visual indicator
                    CustomParticles.HaloRing(target.Center, primary * 0.5f, 0.4f, 20);
                    break;
                    
                case 2: // Punish - teleport + projectile burst
                    AggressiveTeleport(npc, target, primary, secondary, 200f, true);
                    FireRingBarrage(npc.Center, projectileType, damage, 8, 10f, 0f, primary);
                    break;
                    
                case 3: // Severe - teleport + heavy barrage + mirages
                    AggressiveTeleport(npc, target, primary, secondary, 150f, true);
                    FireRingBarrage(npc.Center, projectileType, damage, 16, 12f, 0f, primary);
                    SpawnMirageFormation(npc, target, primary, secondary, 4, 300f, 15, projectileType, damage);
                    MagnumScreenEffects.AddScreenShake(12f);
                    break;
            }
        }
        
        #endregion
        
        #region Rhythmic Attack Patterns
        
        /// <summary>
        /// Gets a rhythmic timing offset based on a BPM.
        /// Use for attacks that should feel musical.
        /// </summary>
        public static bool IsOnBeat(int timer, int bpm, int subdivision = 1)
        {
            // 60 ticks per second in Terraria
            float beatsPerTick = bpm / 3600f;
            float beatNumber = timer * beatsPerTick * subdivision;
            float fractionalBeat = beatNumber % 1f;
            
            // Check if we're close to a beat
            return fractionalBeat < 0.05f || fractionalBeat > 0.95f;
        }
        
        /// <summary>
        /// Returns attack intensity based on musical phrase structure.
        /// Creates tension-release patterns.
        /// </summary>
        public static float GetMusicalIntensity(int timer, int phraseLength = 240)
        {
            float progress = (timer % phraseLength) / (float)phraseLength;
            
            // Build tension through phrase, peak at 80%, release at end
            if (progress < 0.6f)
                return MathHelper.Lerp(0.5f, 0.8f, progress / 0.6f);
            if (progress < 0.8f)
                return MathHelper.Lerp(0.8f, 1.0f, (progress - 0.6f) / 0.2f);
            return MathHelper.Lerp(1.0f, 0.5f, (progress - 0.8f) / 0.2f);
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Aggressively pursues the player with high speed.
        /// </summary>
        public static void AggressivePursuit(NPC npc, Player target, float speed, float acceleration = 0.5f)
        {
            Vector2 direction = (target.Center - npc.Center).SafeNormalize(Vector2.Zero);
            Vector2 targetVel = direction * speed;
            npc.velocity = Vector2.Lerp(npc.velocity, targetVel, acceleration / 10f);
        }
        
        /// <summary>
        /// Predicts where the player will be and aims there.
        /// </summary>
        public static Vector2 PredictPlayerPosition(Player target, float projectileSpeed, Vector2 origin)
        {
            float distance = Vector2.Distance(origin, target.Center);
            float timeToReach = distance / projectileSpeed;
            return target.Center + target.velocity * timeToReach * 0.8f;
        }
        
        /// <summary>
        /// Creates telegraph warning indicators at a position.
        /// </summary>
        public static void CreateTelegraph(Vector2 position, Color color, float duration, TelegraphType type)
        {
            switch (type)
            {
                case TelegraphType.Circle:
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 8f;
                        CustomParticles.GenericFlare(position + angle.ToRotationVector2() * 40f, 
                            color * 0.6f, 0.3f, (int)duration);
                    }
                    CustomParticles.HaloRing(position, color * 0.4f, 0.5f, (int)duration);
                    break;
                    
                case TelegraphType.Line:
                    for (int i = 0; i < 5; i++)
                    {
                        CustomParticles.GenericFlare(position + new Vector2(i * 30f - 60f, 0), 
                            color * 0.5f, 0.25f, (int)duration);
                    }
                    break;
                    
                case TelegraphType.Cross:
                    for (int i = 0; i < 4; i++)
                    {
                        float angle = MathHelper.PiOver2 * i;
                        for (int j = 1; j <= 3; j++)
                        {
                            CustomParticles.GenericFlare(position + angle.ToRotationVector2() * j * 25f, 
                                color * (0.7f - j * 0.15f), 0.25f, (int)duration);
                        }
                    }
                    break;
            }
        }
        
        public enum TelegraphType
        {
            Circle,
            Line,
            Cross
        }
        
        #endregion
    }
}
