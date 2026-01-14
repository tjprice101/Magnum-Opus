using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.LaCampanella.Debuffs;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// A ModPlayer that manages the charged melee attack system.
    /// When holding right-click with a compatible melee weapon, the player charges up.
    /// On release, the player explodes, thrusts toward the nearest enemy, 
    /// rains music notes, and thunder strikes nearby enemies.
    /// </summary>
    public class ChargedMeleePlayer : ModPlayer
    {
        // Charge state
        public bool IsCharging { get; private set; }
        public float ChargeProgress { get; private set; }
        public float MaxChargeTime { get; private set; } = 60f; // 1 second to full charge
        
        // Current weapon's charge config
        private ChargedMeleeConfig currentConfig;
        private Item chargingWeapon;
        private float chargeTimer;
        
        // Release state
        public bool IsReleasing { get; private set; }
        private int releaseTimer;
        private Vector2 thrustDirection;
        private NPC targetEnemy;
        private int dashTimer;
        private const int DashDuration = 15; // Frames for dash
        private const float DashSpeed = 25f;
        
        // Cooldown
        private int cooldownTimer;
        private const int CooldownDuration = 90; // 1.5 second cooldown
        
        /// <summary>
        /// Attempts to start charging if conditions are met.
        /// Call this from the weapon's HoldItem when right mouse is pressed.
        /// </summary>
        public bool TryStartCharging(Item weapon, ChargedMeleeConfig config)
        {
            if (cooldownTimer > 0) return false;
            if (IsCharging || IsReleasing) return false;
            if (weapon == null || config == null) return false;
            
            IsCharging = true;
            ChargeProgress = 0f;
            chargeTimer = 0f;
            currentConfig = config;
            chargingWeapon = weapon;
            MaxChargeTime = config.ChargeTime;
            
            return true;
        }
        
        /// <summary>
        /// Updates charging state. Call from weapon's HoldItem.
        /// Returns true if still charging, false if released.
        /// </summary>
        public void UpdateCharging(bool rightMouseHeld)
        {
            if (!IsCharging) return;
            
            if (rightMouseHeld && !IsReleasing)
            {
                // Continue charging
                chargeTimer++;
                ChargeProgress = Math.Min(chargeTimer / MaxChargeTime, 1f);
                
                // Charging VFX
                SpawnChargingEffects();
                
                // Prevent weapon use while charging
                Player.itemTime = 2;
                Player.itemAnimation = 2;
            }
            else if (ChargeProgress > 0.3f) // Minimum charge to release
            {
                // Release the charge!
                ReleaseCharge();
            }
            else
            {
                // Cancel charge - not enough charge built up
                CancelCharge();
            }
        }
        
        private void SpawnChargingEffects()
        {
            if (currentConfig == null) return;
            
            // Pulsing energy aura around player with gradient colors
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.15f + ChargeProgress * 10f) * 0.3f + 0.7f;
            float intensity = ChargeProgress * pulse;
            
            // Orbiting gradient flares - get more intense as charge builds
            int flareCount = 4 + (int)(ChargeProgress * 4);
            float baseAngle = Main.GameUpdateCount * 0.05f;
            float radius = 30f + ChargeProgress * 25f;
            
            for (int i = 0; i < flareCount; i++)
            {
                float angle = baseAngle + MathHelper.TwoPi * i / flareCount;
                float wobble = (float)Math.Sin(Main.GameUpdateCount * 0.1f + i) * 5f;
                Vector2 flarePos = Player.Center + angle.ToRotationVector2() * (radius + wobble);
                
                // Gradient from primary to secondary color
                float gradientProgress = (float)i / flareCount;
                Color flareColor = Color.Lerp(currentConfig.PrimaryColor, currentConfig.SecondaryColor, gradientProgress);
                flareColor *= intensity;
                
                if (Main.rand.NextBool(3))
                {
                    CustomParticles.GenericFlare(flarePos, flareColor, 0.3f + ChargeProgress * 0.4f, 12);
                }
            }
            
            // Central pulsing glow
            if (Main.rand.NextBool(4))
            {
                Color pulseColor = Color.Lerp(currentConfig.PrimaryColor, currentConfig.SecondaryColor, ChargeProgress);
                CustomParticles.GenericGlow(Player.Center + Main.rand.NextVector2Circular(15f, 15f),
                    pulseColor * intensity, 0.3f + ChargeProgress * 0.3f, 15);
            }
            
            // Expanding halo rings as charge increases
            if (ChargeProgress > 0.5f && Main.rand.NextBool(8))
            {
                Color ringColor = Color.Lerp(currentConfig.PrimaryColor, currentConfig.SecondaryColor, ChargeProgress);
                CustomParticles.HaloRing(Player.Center, ringColor * 0.6f, 0.3f + ChargeProgress * 0.3f, 20);
            }
            
            // Music notes swirling at high charge
            if (ChargeProgress > 0.7f && Main.rand.NextBool(10))
            {
                currentConfig.SpawnThemeMusicNotes?.Invoke(Player.Center, 2, 25f);
            }
            
            // Charging sound periodically
            if ((int)chargeTimer % 20 == 0 && ChargeProgress < 1f)
            {
                SoundEngine.PlaySound(SoundID.Item15 with { Pitch = ChargeProgress * 0.5f, Volume = 0.3f }, Player.Center);
            }
            
            // Full charge sound and effect
            if (ChargeProgress >= 1f && (int)chargeTimer == (int)MaxChargeTime)
            {
                SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.3f, Volume = 0.6f }, Player.Center);
                
                // Full charge flash
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 offset = angle.ToRotationVector2() * 40f;
                    Color flashColor = Color.Lerp(currentConfig.PrimaryColor, Color.White, 0.5f);
                    CustomParticles.GenericFlare(Player.Center + offset, flashColor, 0.6f, 20);
                }
                CustomParticles.HaloRing(Player.Center, Color.White, 0.8f, 25);
            }
            
            // Themed lighting
            Vector3 lightColor = Color.Lerp(currentConfig.PrimaryColor, currentConfig.SecondaryColor, ChargeProgress).ToVector3();
            Lighting.AddLight(Player.Center, lightColor * intensity * 0.8f);
            
            // Screen shake at high charge
            if (ChargeProgress > 0.8f)
            {
                Player.GetModPlayer<ScreenShakePlayer>()?.AddShake(ChargeProgress * 2f, 3);
            }
        }
        
        private void ReleaseCharge()
        {
            IsCharging = false;
            IsReleasing = true;
            releaseTimer = 0;
            dashTimer = 0;
            
            // Find nearest enemy
            targetEnemy = FindNearestEnemy(800f);
            
            if (targetEnemy != null)
            {
                thrustDirection = (targetEnemy.Center - Player.Center).SafeNormalize(Vector2.UnitX);
            }
            else
            {
                thrustDirection = (Main.MouseWorld - Player.Center).SafeNormalize(Vector2.UnitX);
            }
            
            // Epic release effects
            SpawnReleaseExplosion();
            
            // Play epic release sound
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.2f, Volume = 0.9f }, Player.Center);
            SoundEngine.PlaySound(SoundID.Item45 with { Pitch = 0.2f, Volume = 0.7f }, Player.Center);
            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Volume = 0.5f }, Player.Center);
        }
        
        private void SpawnReleaseExplosion()
        {
            if (currentConfig == null) return;
            
            float intensity = ChargeProgress;
            
            // === MASSIVE EXPLOSION ===
            // Radial flare burst with gradient
            int flareCount = (int)(12 * intensity) + 8;
            for (int i = 0; i < flareCount; i++)
            {
                float angle = MathHelper.TwoPi * i / flareCount;
                Vector2 flarePos = Player.Center + angle.ToRotationVector2() * Main.rand.NextFloat(40f, 80f);
                float progress = (float)i / flareCount;
                Color flareColor = Color.Lerp(currentConfig.PrimaryColor, currentConfig.SecondaryColor, progress);
                CustomParticles.GenericFlare(flarePos, flareColor, 0.7f * intensity, 25);
            }
            
            // Fractal geometric pattern
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 offset = angle.ToRotationVector2() * 50f;
                float progress = (float)i / 8f;
                Color fractalColor = Color.Lerp(currentConfig.SecondaryColor, currentConfig.PrimaryColor, progress);
                CustomParticles.GenericFlare(Player.Center + offset, fractalColor, 0.6f * intensity, 22);
            }
            
            // Multiple halo rings
            for (int ring = 0; ring < 5; ring++)
            {
                float progress = (float)ring / 5f;
                Color ringColor = Color.Lerp(currentConfig.PrimaryColor, currentConfig.SecondaryColor, progress);
                CustomParticles.HaloRing(Player.Center, ringColor, 0.5f + ring * 0.2f, 20 + ring * 5);
            }
            
            // White core flash
            CustomParticles.GenericFlare(Player.Center, Color.White, 1.2f * intensity, 15);
            CustomParticles.HaloRing(Player.Center, Color.White, 1f, 30);
            
            // Theme-specific explosion
            currentConfig.SpawnThemeExplosion?.Invoke(Player.Center, intensity * 2f);
            
            // Screen shake
            Player.GetModPlayer<ScreenShakePlayer>()?.AddShake(12f * intensity, 25);
            
            // Intense light
            Lighting.AddLight(Player.Center, currentConfig.PrimaryColor.ToVector3() * 2f);
        }
        
        private void CancelCharge()
        {
            IsCharging = false;
            ChargeProgress = 0f;
            chargeTimer = 0f;
            currentConfig = null;
            chargingWeapon = null;
        }
        
        private NPC FindNearestEnemy(float range)
        {
            NPC nearest = null;
            float nearestDist = range;
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                
                float dist = Vector2.Distance(Player.Center, npc.Center);
                if (dist < nearestDist)
                {
                    nearest = npc;
                    nearestDist = dist;
                }
            }
            
            return nearest;
        }
        
        public override void PreUpdate()
        {
            // Handle cooldown
            if (cooldownTimer > 0)
            {
                cooldownTimer--;
            }
            
            // Handle release/dash phase
            if (IsReleasing)
            {
                UpdateRelease();
            }
        }
        
        private void UpdateRelease()
        {
            releaseTimer++;
            
            // Phase 1: Dash toward target (frames 0-15)
            if (dashTimer < DashDuration)
            {
                dashTimer++;
                
                // Move player toward target
                Player.velocity = thrustDirection * DashSpeed * (1f - (float)dashTimer / DashDuration * 0.3f);
                Player.immune = true;
                Player.immuneTime = 5;
                Player.immuneNoBlink = true;
                
                // Trail effects during dash
                if (currentConfig != null)
                {
                    // Afterimage trail with gradient
                    if (dashTimer % 2 == 0)
                    {
                        float trailProgress = (float)dashTimer / DashDuration;
                        Color trailColor = Color.Lerp(currentConfig.PrimaryColor, currentConfig.SecondaryColor, trailProgress);
                        CustomParticles.GenericFlare(Player.Center, trailColor, 0.5f, 15);
                        
                        // Sparks trailing behind
                        Vector2 sparkVel = -thrustDirection * Main.rand.NextFloat(2f, 5f) + Main.rand.NextVector2Circular(2f, 2f);
                        var spark = new GlowSparkParticle(Player.Center, sparkVel, trailColor, 0.4f, 20);
                        MagnumParticleHandler.SpawnParticle(spark);
                    }
                }
            }
            // Phase 2: Music note rain and thunder (frames 15-75)
            else if (releaseTimer < 75)
            {
                // Spawn music note rain
                SpawnMusicNoteRain();
                
                // Spawn thunder strikes
                SpawnThunderStrikes();
            }
            // End release
            else
            {
                EndRelease();
            }
        }
        
        private void SpawnMusicNoteRain()
        {
            if (currentConfig == null) return;
            
            // Spawn falling music notes around the player
            if (releaseTimer % 3 == 0)
            {
                float xOffset = Main.rand.NextFloat(-200f, 200f);
                Vector2 spawnPos = Player.Center + new Vector2(xOffset, -300f);
                
                // Spawn themed music notes
                currentConfig.SpawnThemeMusicNotes?.Invoke(spawnPos, 2, 30f);
                
                // Also spawn a falling music note projectile
                if (chargingWeapon != null && releaseTimer % 6 == 0)
                {
                    int damage = (int)(chargingWeapon.damage * ChargeProgress * 0.5f);
                    Vector2 velocity = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(8f, 12f));
                    
                    Projectile.NewProjectile(
                        Player.GetSource_ItemUse(chargingWeapon),
                        spawnPos,
                        velocity,
                        ModContent.ProjectileType<FallingMusicNoteProjectile>(),
                        damage,
                        2f,
                        Player.whoAmI,
                        currentConfig.PrimaryColor.PackedValue,
                        currentConfig.SecondaryColor.PackedValue
                    );
                }
            }
        }
        
        private void SpawnThunderStrikes()
        {
            if (currentConfig == null || chargingWeapon == null) return;
            
            // Spawn thunder strikes hitting nearby enemies
            if (releaseTimer % 8 == 0)
            {
                // Find nearby enemies
                List<NPC> nearbyEnemies = new List<NPC>();
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                    
                    float dist = Vector2.Distance(Player.Center, npc.Center);
                    if (dist < 500f)
                    {
                        nearbyEnemies.Add(npc);
                    }
                }
                
                // Strike a random nearby enemy
                if (nearbyEnemies.Count > 0)
                {
                    NPC target = Main.rand.Next(nearbyEnemies);
                    
                    // Spawn thunder strike
                    int damage = (int)(chargingWeapon.damage * ChargeProgress * 0.8f);
                    Vector2 startPos = target.Center + new Vector2(Main.rand.NextFloat(-50f, 50f), -400f);
                    
                    Projectile.NewProjectile(
                        Player.GetSource_ItemUse(chargingWeapon),
                        startPos,
                        Vector2.Zero,
                        ModContent.ProjectileType<ChargedThunderStrikeProjectile>(),
                        damage,
                        6f,
                        Player.whoAmI,
                        target.Center.X,
                        target.Center.Y
                    );
                    
                    // Draw themed lightning to target
                    currentConfig.DrawThemeLightning?.Invoke(startPos, target.Center);
                }
            }
        }
        
        private void EndRelease()
        {
            IsReleasing = false;
            ChargeProgress = 0f;
            chargeTimer = 0f;
            releaseTimer = 0;
            cooldownTimer = CooldownDuration;
            
            // Final burst
            if (currentConfig != null)
            {
                currentConfig.SpawnThemeExplosion?.Invoke(Player.Center, 1f);
                currentConfig.SpawnThemeMusicNotes?.Invoke(Player.Center, 8, 50f);
            }
            
            currentConfig = null;
            chargingWeapon = null;
        }
        
        /// <summary>
        /// Returns true if right-click is being used for charging (prevents alt-use)
        /// </summary>
        public bool IsUsingRightClickForCharge()
        {
            return IsCharging || IsReleasing || cooldownTimer > 0;
        }
    }
    
    /// <summary>
    /// Configuration for themed charged melee attacks.
    /// Each melee weapon should create its own config with theme-appropriate colors and effects.
    /// </summary>
    public class ChargedMeleeConfig
    {
        public Color PrimaryColor { get; set; }
        public Color SecondaryColor { get; set; }
        public float ChargeTime { get; set; } = 60f; // 1 second default
        
        /// <summary>Action to spawn theme-appropriate music notes</summary>
        public Action<Vector2, int, float> SpawnThemeMusicNotes { get; set; }
        
        /// <summary>Action to spawn theme-appropriate explosion</summary>
        public Action<Vector2, float> SpawnThemeExplosion { get; set; }
        
        /// <summary>Action to draw theme-appropriate lightning</summary>
        public Action<Vector2, Vector2> DrawThemeLightning { get; set; }
    }
    
    #region Charged Attack Projectiles
    
    /// <summary>
    /// Falling music note projectile that damages enemies on contact.
    /// Spawned during the music note rain phase of charged attack release.
    /// </summary>
    public class FallingMusicNoteProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/MusicNote";
        
        private Color primaryColor;
        private Color secondaryColor;
        private int noteVariant;
        private float rotation;
        private float rotationSpeed;
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 50;
        }
        
        public override void OnSpawn(IEntitySource source)
        {
            // Unpack colors from ai parameters using PackedValue
            uint packed1 = (uint)Projectile.ai[0];
            uint packed2 = (uint)Projectile.ai[1];
            primaryColor = new Color((byte)(packed1 >> 0), (byte)(packed1 >> 8), (byte)(packed1 >> 16), (byte)(packed1 >> 24));
            secondaryColor = new Color((byte)(packed2 >> 0), (byte)(packed2 >> 8), (byte)(packed2 >> 16), (byte)(packed2 >> 24));
            noteVariant = Main.rand.Next(6);
            rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            rotationSpeed = Main.rand.NextFloat(-0.1f, 0.1f);
        }
        
        public override void AI()
        {
            // Gentle swaying motion
            Projectile.velocity.X += (float)Math.Sin(Projectile.timeLeft * 0.1f) * 0.1f;
            Projectile.velocity.Y += 0.15f; // Gravity
            Projectile.velocity.Y = Math.Min(Projectile.velocity.Y, 12f);
            
            // Rotate gently
            rotation += rotationSpeed;
            Projectile.rotation = rotation;
            
            // Gradient trail particles
            if (Main.rand.NextBool(3))
            {
                float progress = 1f - (float)Projectile.timeLeft / 180f;
                Color trailColor = Color.Lerp(primaryColor, secondaryColor, progress);
                CustomParticles.GenericGlow(Projectile.Center, trailColor * 0.7f, 0.25f, 15);
            }
            
            // Sparkle occasionally
            if (Main.rand.NextBool(8))
            {
                CustomParticles.GenericFlare(Projectile.Center, Color.White * 0.6f, 0.2f, 10);
            }
            
            // Light
            Vector3 lightColor = Color.Lerp(primaryColor, secondaryColor, 0.5f).ToVector3();
            Lighting.AddLight(Projectile.Center, lightColor * 0.5f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Impact burst
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 offset = angle.ToRotationVector2() * 15f;
                float progress = (float)i / 6f;
                Color burstColor = Color.Lerp(primaryColor, secondaryColor, progress);
                CustomParticles.GenericFlare(target.Center + offset, burstColor, 0.4f, 15);
            }
            
            // Sound
            SoundEngine.PlaySound(SoundID.Item25 with { Pitch = Main.rand.NextFloat(0.3f, 0.7f), Volume = 0.4f }, target.Center);
        }
        
        public override void OnKill(int timeLeft)
        {
            // Burst on death
            for (int i = 0; i < 4; i++)
            {
                float progress = (float)i / 4f;
                Color burstColor = Color.Lerp(primaryColor, secondaryColor, progress);
                CustomParticles.GenericFlare(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f), 
                    burstColor, 0.3f, 12);
            }
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            // Get note texture variant
            string texturePath = $"MagnumOpus/Assets/Particles/MusicNote{(noteVariant > 0 ? (noteVariant + 1).ToString() : "")}";
            Texture2D texture;
            try
            {
                texture = ModContent.Request<Texture2D>(texturePath).Value;
            }
            catch
            {
                texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/MusicNote").Value;
            }
            
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            float scale = 0.8f;
            
            // Draw trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                
                float progress = (float)i / Projectile.oldPos.Length;
                Color trailColor = Color.Lerp(primaryColor, secondaryColor, progress) * (1f - progress) * 0.5f;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                float trailScale = scale * (1f - progress * 0.5f);
                
                Main.EntitySpriteDraw(texture, trailPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0);
            }
            
            // Draw main note with gradient color
            float mainProgress = (float)(180 - Projectile.timeLeft) / 180f;
            Color noteColor = Color.Lerp(primaryColor, secondaryColor, mainProgress);
            
            // Additive glow layer
            Main.EntitySpriteDraw(texture, drawPos, null, noteColor * 0.4f, rotation, origin, scale * 1.2f, SpriteEffects.None, 0);
            // Main layer
            Main.EntitySpriteDraw(texture, drawPos, null, noteColor, rotation, origin, scale, SpriteEffects.None, 0);
            
            return false;
        }
    }
    
    /// <summary>
    /// Thunder strike projectile that instantly hits a target location.
    /// Spawned during the thunder strike phase of charged attack release.
    /// </summary>
    public class ChargedThunderStrikeProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.ThunderSpear;
        
        private Vector2 targetPosition;
        private bool hasStruck;
        private int strikeTimer;
        
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 30;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255; // Invisible - we draw custom
        }
        
        public override void OnSpawn(IEntitySource source)
        {
            targetPosition = new Vector2(Projectile.ai[0], Projectile.ai[1]);
            hasStruck = false;
            strikeTimer = 0;
        }
        
        public override void AI()
        {
            strikeTimer++;
            
            // Strike on frame 5
            if (strikeTimer == 5 && !hasStruck)
            {
                hasStruck = true;
                
                // Move projectile to target for damage
                Projectile.Center = targetPosition;
                
                // Epic thunder effects
                SpawnThunderEffects();
                
                // Sound
                SoundEngine.PlaySound(SoundID.DD2_LightningBugZap with { Pitch = -0.3f, Volume = 0.8f }, targetPosition);
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.5f, Volume = 0.5f }, targetPosition);
            }
            
            // Fade out
            if (strikeTimer > 5)
            {
                Projectile.alpha = (int)(255 * (float)(strikeTimer - 5) / 25f);
            }
        }
        
        private void SpawnThunderEffects()
        {
            // Get player for theme color (owner)
            Player owner = Main.player[Projectile.owner];
            Color primaryColor = Color.White;
            Color secondaryColor = new Color(200, 200, 255);
            
            // Try to get theme colors from the charged melee player
            var chargedPlayer = owner.GetModPlayer<ChargedMeleePlayer>();
            // Use default thunder colors
            
            // Lightning flash at target
            CustomParticles.GenericFlare(targetPosition, Color.White, 1.5f, 8);
            CustomParticles.HaloRing(targetPosition, Color.White, 1f, 15);
            
            // Radial spark burst
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 12f);
                Color sparkColor = Color.Lerp(primaryColor, secondaryColor, (float)i / 12f);
                var spark = new GlowSparkParticle(targetPosition, sparkVel, sparkColor, 0.6f, 20);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // Fractal flares
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 offset = angle.ToRotationVector2() * 30f;
                CustomParticles.GenericFlare(targetPosition + offset, Color.White * 0.8f, 0.5f, 12);
            }
            
            // Screen shake
            owner.GetModPlayer<ScreenShakePlayer>()?.AddShake(5f, 8);
            
            // Bright light
            Lighting.AddLight(targetPosition, 2f, 2f, 2.5f);
        }
        
        public override bool? CanHitNPC(NPC target)
        {
            // Only hit on strike frame
            return hasStruck && strikeTimer >= 5 && strikeTimer <= 10;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Extra impact flash
            CustomParticles.GenericFlare(target.Center, Color.White, 0.8f, 10);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            // Draw lightning bolt from spawn position to target
            if (!hasStruck && strikeTimer < 5)
            {
                // Pre-strike warning glow
                float warning = (float)strikeTimer / 5f;
                CustomParticles.GenericFlare(targetPosition, Color.White * warning, 0.5f * warning, 5);
            }
            else if (hasStruck)
            {
                // Draw the lightning bolt visually
                float fade = 1f - (float)(strikeTimer - 5) / 25f;
                if (fade > 0)
                {
                    // Simple vertical lightning from above
                    Vector2 start = targetPosition + new Vector2(0, -400f);
                    Vector2 end = targetPosition;
                    
                    // Use themed lightning if available, otherwise simple
                    // Draw as dust line for now
                    int segments = 12;
                    for (int i = 0; i < segments; i++)
                    {
                        float t = (float)i / segments;
                        Vector2 point = Vector2.Lerp(start, end, t);
                        point.X += Main.rand.NextFloat(-15f, 15f) * (1f - t); // More spread at top
                        
                        Dust bolt = Dust.NewDustPerfect(point, DustID.Electric, Vector2.Zero, 0, Color.White, 2f * fade);
                        bolt.noGravity = true;
                        bolt.noLight = false;
                    }
                }
            }
            
            return false; // Don't draw default sprite
        }
    }
    
    #endregion
}
