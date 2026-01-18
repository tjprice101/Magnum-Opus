using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.DataStructures;
using ReLogic.Content;
using MagnumOpus.Common.Systems;
using MagnumOpus.Content.SwanLake.Debuffs;

namespace MagnumOpus.Content.SwanLake.Accessories
{
    /// <summary>
    /// ModPlayer class that handles all Swan Lake accessory effects.
    /// Features a toggle system between White Swan (Odette/Defensive) and Black Swan (Odile/Offensive) modes.
    /// All "Black Flame" effects have been replaced with "Flame of the Swan" - a distinct black &amp; white
    /// burning effect that makes enemies 10% more vulnerable to all damage.
    /// </summary>
    public class SwanLakeAccessoryPlayer : ModPlayer
    {
        // ========== PENDANT OF THE TWO SWANS (Melee) ==========
        public bool hasPendantOfTheTwoSwans = false;
        public bool pendantIsBlackMode = false; // false = White (Odette), true = Black (Odile)
        
        // Shield system for white mode
        public int pendantShieldCharges = 3; // 3 hits before depleting
        private const int MaxShieldCharges = 3;
        public int pendantShieldCooldown = 0;
        private const int ShieldCooldownMax = 7200; // 2 minutes (120 seconds)
        public bool pendantShieldActive = true;
        
        // Black mode explosion cooldown
        public int blackModeExplosionCooldown = 0;
        private const int BlackModeExplosionCooldownMax = 30; // 0.5 second cooldown
        
        // Legacy fields for compatibility (kept but not used)
        public int whiteHaloCooldown = 0;
        private const int WhiteHaloCooldownMax = 10800; // 3 minutes (180 seconds)
        public int whiteHaloTimer = 0;
        private const int WhiteHaloDuration = 1800; // 30 seconds
        public bool whiteHaloActive = false;
        
        // ========== DUAL FEATHER QUIVER (Ranger) ==========
        public bool hasDualFeatherQuiver = false;
        public bool quiverIsBlackMode = false;
        
        // ========== CROWN OF THE SWAN (Mage) ==========
        public bool hasCrownOfTheSwan = false;
        public bool crownIsBlackMode = false;
        public int protectiveWispCount = 0;
        private const int MaxProtectiveWisps = 5;
        public int crownFlameOfSwanCooldown = 0; // Cooldown for applying Flame of the Swan
        private const int CrownFlameCooldownMax = 120; // 2 seconds cooldown
        
        // ========== BLACK WINGS OF THE MONOCHROMATIC DAWN (Summoner) ==========
        public bool hasBlackWings = false;
        public bool wingsIsBlackMode = false;
        
        // ========== FLOATING VISUAL ==========
        public float floatAngle = 0f;
        public int floatAnimationFrame = 0;
        public int floatAnimationTimer = 0;
        
        // ========== FEATHER EFFECT CONSOLIDATION ==========
        // Track if feather aura has been spawned this tick to prevent duplicates
        public bool hasSwanLakeFeatherEffect = false;
        // Track if holding a Swan Lake weapon
        public bool isHoldingSwanLakeWeapon = false;
        
        public override void ResetEffects()
        {
            hasPendantOfTheTwoSwans = false;
            hasDualFeatherQuiver = false;
            hasCrownOfTheSwan = false;
            hasBlackWings = false;
            hasSwanLakeFeatherEffect = false;
            isHoldingSwanLakeWeapon = false;
        }
        
        /// <summary>
        /// Checks if charge effects and active abilities should be usable.
        /// Returns false if inventory, map, or any other screen is open.
        /// </summary>
        public bool CanUseChargeEffects()
        {
            // Don't allow charge effects if any screen is open
            if (Main.playerInventory) return false;
            if (Main.mapFullscreen) return false;
            if (Main.InGameUI.IsVisible) return false;
            if (Main.ingameOptionsWindow) return false;
            if (Main.inFancyUI) return false;
            if (Player.talkNPC >= 0) return false;
            if (Player.chest >= 0) return false;
            if (Main.editSign) return false;
            if (Main.editChest) return false;
            
            return true;
        }
        
        public override void PostUpdate()
        {
            // ========== FLOATING VISUAL ANGLE ==========
            floatAngle += 0.025f;
            if (floatAngle > MathHelper.TwoPi)
                floatAngle -= MathHelper.TwoPi;
            
            // ========== FLOATING ANIMATION FRAME (6x6 sprite sheet) ==========
            floatAnimationTimer++;
            if (floatAnimationTimer >= 6) // Change frame every 6 ticks
            {
                floatAnimationTimer = 0;
                floatAnimationFrame++;
                if (floatAnimationFrame >= 36) // 6x6 = 36 frames
                    floatAnimationFrame = 0;
            }
            
            // ========== CROWN FLAME COOLDOWN ==========
            if (crownFlameOfSwanCooldown > 0)
                crownFlameOfSwanCooldown--;
            
            // ========== BLACK MODE EXPLOSION COOLDOWN ==========
            if (blackModeExplosionCooldown > 0)
                blackModeExplosionCooldown--;
            
            // ========== PENDANT SHIELD SYSTEM ==========
            if (pendantShieldCooldown > 0)
                pendantShieldCooldown--;
            
            // Recharge shield when cooldown is done
            if (pendantShieldCooldown <= 0 && pendantShieldCharges < MaxShieldCharges)
            {
                pendantShieldCharges = MaxShieldCharges;
                pendantShieldActive = true;
                if (hasPendantOfTheTwoSwans && !pendantIsBlackMode)
                {
                    Main.NewText("Monochromatic Shield recharged!", new Color(240, 245, 255));
                    SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.5f }, Player.Center);
                    
                    // Visual recharge effect
                    for (int i = 0; i < 30; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 30f;
                        Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 4f;
                        Dust white = Dust.NewDustPerfect(Player.Center, DustID.WhiteTorch, vel, 0, default, 1.5f);
                        white.noGravity = true;
                    }
                }
            }
            
            // Show shield visual when active (White mode only) - VERY PROMINENT HALO SYSTEM
            // Shield brightness scales with charges: 3 = full, 2 = 66%, 1 = 33%, 0 = fading out
            if (hasPendantOfTheTwoSwans && !pendantIsBlackMode)
            {
                // Calculate halo intensity based on shield state
                float haloIntensity = 0f;
                if (pendantShieldActive && pendantShieldCharges > 0)
                {
                    haloIntensity = pendantShieldCharges / (float)MaxShieldCharges; // 0.33, 0.66, or 1.0
                }
                else if (pendantShieldCooldown > 0)
                {
                    // During recharge, slowly pulse back in
                    float rechargeProgress = 1f - (pendantShieldCooldown / (float)ShieldCooldownMax);
                    haloIntensity = rechargeProgress * 0.3f; // Dim glow during recharge
                }
                
                // ALWAYS show SOMETHING when shield is equipped in white mode
                haloIntensity = Math.Max(0.3f, haloIntensity); // Minimum 30% intensity always
                
                // === PROMINENT CONSTANT SHIELD AURA ===
                // Rotating outer ring - ALWAYS visible
                float ringAngle = Main.GameUpdateCount * 0.03f;
                int ringSegments = 12;
                for (int i = 0; i < ringSegments; i++)
                {
                    float segmentAngle = ringAngle + MathHelper.TwoPi * i / ringSegments;
                    float radius = 50f + (float)Math.Sin(Main.GameUpdateCount * 0.08f + i) * 5f;
                    Vector2 pos = Player.Center + new Vector2((float)Math.Cos(segmentAngle), (float)Math.Sin(segmentAngle)) * radius;
                    
                    // Alternating black and white segments
                    if (i % 2 == 0)
                    {
                        Dust white = Dust.NewDustPerfect(pos, DustID.WhiteTorch, 
                            new Vector2((float)Math.Cos(segmentAngle + MathHelper.PiOver2), (float)Math.Sin(segmentAngle + MathHelper.PiOver2)) * 0.5f, 
                            (int)(80 - haloIntensity * 40), default, 1.2f * haloIntensity);
                        white.noGravity = true;
                    }
                    else
                    {
                        Dust black = Dust.NewDustPerfect(pos, DustID.Smoke, 
                            new Vector2((float)Math.Cos(segmentAngle + MathHelper.PiOver2), (float)Math.Sin(segmentAngle + MathHelper.PiOver2)) * 0.5f, 
                            180, Color.Black, 1.0f * haloIntensity);
                        black.noGravity = true;
                    }
                }
                
                // Rainbow shimmer particles in the shield
                if (Main.rand.NextBool(3))
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    float radius = 45f + Main.rand.NextFloat(15f);
                    Vector2 pos = Player.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                    float hue = (Main.GameUpdateCount * 0.02f + angle / MathHelper.TwoPi) % 1f;
                    Color rainbow = Main.hslToRgb(hue, 0.8f, 0.7f) * haloIntensity;
                    Dust r = Dust.NewDustPerfect(pos, DustID.RainbowTorch, Vector2.Zero, 0, rainbow, 0.8f);
                    r.noGravity = true;
                }
                
                // Pearlescent shimmer flares
                if (Main.rand.NextBool(6))
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 pos = Player.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 48f;
                    CustomParticles.GenericFlare(pos, Color.White * haloIntensity, 0.25f * haloIntensity, 12);
                }
                
                // Shield charge indicator - bright flares at cardinal directions based on charges
                for (int c = 0; c < pendantShieldCharges; c++)
                {
                    float chargeAngle = Main.GameUpdateCount * 0.02f + MathHelper.TwoPi * c / 3f;
                    Vector2 chargePos = Player.Center + new Vector2((float)Math.Cos(chargeAngle), (float)Math.Sin(chargeAngle)) * 55f;
                    
                    // Show charge indicator every few frames
                    if (Main.GameUpdateCount % 8 == c * 2)
                    {
                        CustomParticles.GenericFlare(chargePos, Color.White, 0.35f, 15);
                        Dust indicator = Dust.NewDustPerfect(chargePos, DustID.WhiteTorch, Vector2.Zero, 0, default, 1.5f);
                        indicator.noGravity = true;
                    }
                }
                
                // Add prominent ambient light based on halo intensity
                float lightPulse = 0.9f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.1f;
                Lighting.AddLight(Player.Center, haloIntensity * 0.6f * lightPulse, haloIntensity * 0.6f * lightPulse, haloIntensity * 0.7f * lightPulse);
            }
            
            // Legacy halo system - keep for backward compatibility
            if (whiteHaloCooldown > 0)
                whiteHaloCooldown--;
            
            if (whiteHaloActive)
            {
                whiteHaloTimer--;
                if (whiteHaloTimer <= 0)
                {
                    whiteHaloActive = false;
                }
            }
            
            if (!hasPendantOfTheTwoSwans)
            {
                whiteHaloActive = false;
                whiteHaloTimer = 0;
            }
            
            // ========== CROWN OF THE SWAN - Protective Wisps ==========
            if (hasCrownOfTheSwan && !crownIsBlackMode)
            {
                // Spawn wisps periodically in white mode
                if (protectiveWispCount < MaxProtectiveWisps && Main.rand.NextBool(180))
                {
                    protectiveWispCount++;
                    // Visual wisp spawn effect
                    for (int i = 0; i < 10; i++)
                    {
                        Dust wisp = Dust.NewDustPerfect(Player.Center, DustID.WhiteTorch,
                            Main.rand.NextVector2Circular(3f, 3f), 100, default, 1.2f);
                        wisp.noGravity = true;
                    }
                }
                
                // Draw orbiting wisps
                if (protectiveWispCount > 0 && Main.rand.NextBool(4))
                {
                    float wispAngle = floatAngle * 2f + Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 wispPos = Player.Center + new Vector2((float)Math.Cos(wispAngle), (float)Math.Sin(wispAngle)) * 40f;
                    Dust wispDust = Dust.NewDustPerfect(wispPos, DustID.WhiteTorch, Vector2.Zero, 150, default, 0.8f);
                    wispDust.noGravity = true;
                }
            }
            else
            {
                protectiveWispCount = 0;
            }
            
            // ========== BLACK WINGS - Minion visual effects ==========
            if (hasBlackWings)
            {
                foreach (Projectile proj in Main.ActiveProjectiles)
                {
                    if (proj.owner == Player.whoAmI && proj.minion && Main.rand.NextBool(15))
                    {
                        if (wingsIsBlackMode)
                        {
                            // Black flame claws effect
                            Dust claw = Dust.NewDustPerfect(proj.Center + Main.rand.NextVector2Circular(10f, 10f),
                                DustID.Smoke, new Vector2(0, -0.5f), 200, Color.Black, 1.2f);
                            claw.noGravity = true;
                        }
                        else
                        {
                            // White flame shield effect
                            Dust shield = Dust.NewDustPerfect(proj.Center + Main.rand.NextVector2Circular(15f, 15f),
                                DustID.WhiteTorch, Vector2.Zero, 100, default, 0.7f);
                            shield.noGravity = true;
                        }
                    }
                }
            }
            
            // ========== AMBIENT PARTICLES ==========
            SpawnAmbientParticles();
            
            // ========== UNIFIED SWAN LAKE FEATHER EFFECT ==========
            // Only spawn feather aura once regardless of how many Swan Lake items equipped
            SpawnUnifiedFeatherEffect();
        }
        
        private void SpawnAmbientParticles()
        {
            // Pendant particles
            if (hasPendantOfTheTwoSwans && Main.rand.NextBool(8))
            {
                int dustType = pendantIsBlackMode ? DustID.Smoke : DustID.WhiteTorch;
                Color color = pendantIsBlackMode ? Color.Black : default;
                Dust dust = Dust.NewDustPerfect(Player.Center + new Vector2(Main.rand.NextFloat(-20f, 20f), -40f),
                    dustType, new Vector2(0, -1f), pendantIsBlackMode ? 200 : 100, color, 0.9f);
                dust.noGravity = true;
            }
            
            // Quiver particles
            if (hasDualFeatherQuiver && Main.rand.NextBool(10))
            {
                int dustType = quiverIsBlackMode ? DustID.Smoke : DustID.WhiteTorch;
                Vector2 offset = new Vector2((float)Math.Cos(floatAngle + MathHelper.Pi) * 35f, -35f);
                Dust dust = Dust.NewDustPerfect(Player.Center + offset, dustType,
                    new Vector2(0, -0.5f), quiverIsBlackMode ? 180 : 100, quiverIsBlackMode ? Color.Black : default, 0.7f);
                dust.noGravity = true;
            }
        }
        
        /// <summary>
        /// Spawns a unified feather effect around the player if they have ANY Swan Lake accessory equipped.
        /// Only spawns once per tick regardless of how many items are equipped.
        /// </summary>
        private void SpawnUnifiedFeatherEffect()
        {
            // Check if player has any Swan Lake item (accessory OR held weapon)
            bool hasAnySwanLakeItem = hasPendantOfTheTwoSwans || hasDualFeatherQuiver || 
                                       hasCrownOfTheSwan || hasBlackWings || isHoldingSwanLakeWeapon;
            
            // If no Swan Lake items, don't spawn anything
            if (!hasAnySwanLakeItem) return;
            
            // If feathers already spawned this tick, don't spawn again
            if (hasSwanLakeFeatherEffect) return;
            
            // Mark that we've spawned feathers this tick
            hasSwanLakeFeatherEffect = true;
            
            // Spawn ONE set of subtle feathers (much less frequent)
            if (Main.rand.NextBool(25)) // 1 in 25 chance per tick
            {
                CustomParticles.SwanFeatherAura(Player.Center, 28f, 1);
            }
        }
        
        public override bool FreeDodge(Player.HurtInfo info)
        {
            // NEW SHIELD SYSTEM - White mode Pendant
            // Permanent shield that absorbs up to 3 hits before needing 2 minute recharge
            if (hasPendantOfTheTwoSwans && !pendantIsBlackMode && pendantShieldActive && pendantShieldCharges > 0)
            {
                pendantShieldCharges--;
                
                // === SHIELD ABSORB VISUAL ===
                // Expanding white and black ring
                for (int i = 0; i < 30; i++)
                {
                    float angle = MathHelper.TwoPi * i / 30f;
                    Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 5f;
                    
                    if (i % 2 == 0)
                    {
                        Dust white = Dust.NewDustPerfect(Player.Center, DustID.WhiteTorch, vel, 0, default, 1.8f);
                        white.noGravity = true;
                        white.fadeIn = 1.3f;
                    }
                    else
                    {
                        Dust black = Dust.NewDustPerfect(Player.Center, DustID.Smoke, vel, 220, Color.Black, 1.6f);
                        black.noGravity = true;
                        black.fadeIn = 1.1f;
                    }
                }
                
                // Pearlescent shimmer burst
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi * i / 12f;
                    Vector2 pos = Player.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 25f;
                    Color pearl = (i % 3) switch
                    {
                        0 => new Color(255, 240, 245),
                        1 => new Color(240, 245, 255),
                        _ => new Color(250, 255, 245)
                    };
                    Dust ring = Dust.NewDustPerfect(pos, DustID.TintableDustLighted, Vector2.Zero, 0, pearl, 1.4f);
                    ring.noGravity = true;
                }
                
                // Sound effect
                SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.3f, Volume = 0.8f }, Player.Center);
                
                // If all charges depleted, start cooldown
                if (pendantShieldCharges <= 0)
                {
                    pendantShieldActive = false;
                    pendantShieldCooldown = ShieldCooldownMax;
                    Main.NewText("Monochromatic Shield depleted! Recharging in 2 minutes...", new Color(255, 180, 180));
                    
                    // Shield break visual
                    for (int i = 0; i < 50; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 50f;
                        Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 8f;
                        Dust shatter = Dust.NewDustPerfect(Player.Center, i % 2 == 0 ? DustID.WhiteTorch : DustID.Smoke, 
                            vel, i % 2 == 0 ? 0 : 220, i % 2 == 0 ? default : Color.Black, 2f);
                        shatter.noGravity = true;
                    }
                    SoundEngine.PlaySound(SoundID.Shatter with { Pitch = 0.2f }, Player.Center);
                }
                else
                {
                    Main.NewText($"Shield absorbed hit! ({pendantShieldCharges}/3 charges remaining)", new Color(240, 245, 255));
                }
                
                return true; // This hit is dodged for free
            }
            
            return false; // Don't dodge
        }
        
        public override bool ConsumableDodge(Player.HurtInfo info)
        {
            // Crown white mode - Protective wisps can absorb a hit
            if (hasCrownOfTheSwan && !crownIsBlackMode && protectiveWispCount > 0)
            {
                protectiveWispCount--;
                
                // Wisp break effect
                for (int i = 0; i < 15; i++)
                {
                    Dust wisp = Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(20f, 20f),
                        DustID.WhiteTorch, Main.rand.NextVector2Circular(4f, 4f), 100, default, 1.4f);
                    wisp.noGravity = true;
                }
                
                // Pearlescent shimmer burst
                for (int i = 0; i < 8; i++)
                {
                    Color pearl = Main.rand.Next(3) switch
                    {
                        0 => new Color(255, 240, 245),
                        1 => new Color(240, 245, 255),
                        _ => new Color(250, 255, 245)
                    };
                    Dust shimmer = Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(25f, 25f),
                        DustID.TintableDustLighted, Main.rand.NextVector2Circular(3f, 3f), 0, pearl, 1f);
                    shimmer.noGravity = true;
                }
                
                SoundEngine.PlaySound(SoundID.Item27 with { Pitch = 0.8f, Volume = 0.6f }, Player.Center);
                Main.NewText($"Protective wisp absorbed the hit! ({protectiveWispCount} remaining)", new Color(240, 245, 255));
                
                return true; // Dodge consumed
            }
            
            return false;
        }
        
        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            HandleMeleeHit(target, hit, damageDone);
        }
        
        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Melee projectiles
            if (proj.DamageType == DamageClass.Melee)
                HandleMeleeHit(target, hit, damageDone);
            
            // Ranged effects are now handled in DualFeatherQuiverGlobalProjectile
            
            // Magic - Black mode Flame of the Swan effect with cooldown
            if (hasCrownOfTheSwan && crownIsBlackMode && proj.DamageType == DamageClass.Magic && crownFlameOfSwanCooldown <= 0)
            {
                // Vivid black and white flame burst on enemy
                for (int i = 0; i < 10; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(2f, 4f);
                    
                    if (i % 2 == 0)
                    {
                        Dust black = Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(20f, 20f),
                            DustID.Smoke, vel, 220, Color.Black, 1.5f);
                        black.noGravity = true;
                    }
                    else
                    {
                        Dust white = Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(20f, 20f),
                            DustID.WhiteTorch, vel * 0.8f, 80, default, 1.2f);
                        white.noGravity = true;
                    }
                }
                
                // Pearlescent shimmer
                for (int i = 0; i < 4; i++)
                {
                    Color pearl = Main.rand.Next(3) switch
                    {
                        0 => new Color(255, 240, 245),
                        1 => new Color(240, 245, 255),
                        _ => new Color(250, 255, 245)
                    };
                    Dust shimmer = Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(25f, 25f),
                        DustID.TintableDustLighted, Main.rand.NextVector2Circular(1.5f, 1.5f), 0, pearl, 0.9f);
                    shimmer.noGravity = true;
                }
                
                // Apply Flame of the Swan (5 seconds = 300 ticks)
                target.AddBuff(ModContent.BuffType<FlameOfTheSwan>(), 300);
                crownFlameOfSwanCooldown = 120; // 2 second cooldown
            }
            
            // Minions - Black mode damage boost is handled via ModifyHitNPCWithProj
        }
        
        private void HandleMeleeHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Black mode Pendant - 5% chance on ANY melee hit to create rainbow electrical explosion
            // This matches the tooltip description: "5% chance on melee hit"
            if (hasPendantOfTheTwoSwans && pendantIsBlackMode && Main.rand.NextBool(20) && blackModeExplosionCooldown <= 0)
            {
                blackModeExplosionCooldown = BlackModeExplosionCooldownMax;
                
                // === MASSIVE PEARLESCENT RAINBOW ELECTRICAL EXPLOSION - VERY VISIBLE! ===
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f, Volume = 1.1f }, target.Center);
                SoundEngine.PlaySound(SoundID.Item27 with { Pitch = 0.2f, Volume = 0.7f }, target.Center);
                
                // HUGE Black and white lightning explosion
                for (int i = 0; i < 50; i++)
                {
                    float angle = MathHelper.TwoPi * i / 50f;
                    Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(8f, 16f);
                    
                    // Alternating black and white lightning particles
                    if (i % 2 == 0)
                    {
                        Dust white = Dust.NewDustPerfect(target.Center, DustID.WhiteTorch, vel, 0, default, 2.8f);
                        white.noGravity = true;
                        white.fadeIn = 1.8f;
                    }
                    else
                    {
                        Dust black = Dust.NewDustPerfect(target.Center, DustID.Smoke, vel * 0.9f, 220, Color.Black, 2.5f);
                        black.noGravity = true;
                        black.fadeIn = 1.5f;
                    }
                }
                
                // === PEARLESCENT RAINBOW RING ===
                for (int i = 0; i < 24; i++)
                {
                    float angle = MathHelper.TwoPi * i / 24f;
                    Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(4f, 8f);
                    
                    // Rainbow cycle through pearlescent colors
                    float hue = (float)i / 24f;
                    Color rainbow = Main.hslToRgb(hue, 0.9f, 0.8f);
                    
                    Dust pearl = Dust.NewDustPerfect(target.Center, DustID.TintableDustLighted, vel, 0, rainbow, 1.8f);
                    pearl.noGravity = true;
                    pearl.fadeIn = 1.4f;
                }
                
                // Central flash burst
                for (int i = 0; i < 15; i++)
                {
                    float hue = Main.rand.NextFloat();
                    Color flash = Main.hslToRgb(hue, 1f, 0.85f);
                    Dust f = Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(10f, 10f), 
                        DustID.RainbowTorch, Main.rand.NextVector2Circular(3f, 3f), 0, flash, 2f);
                    f.noGravity = true;
                }
                
                // Apply Flame of the Swan
                target.AddBuff(ModContent.BuffType<FlameOfTheSwan>(), 180);
                
                // Lightning flash light - BRIGHT!
                Lighting.AddLight(target.Center, 2.5f, 2.5f, 3f);
                
                // Themed particles - use the big signature effects!
                ThemedParticles.SwanLakeRainbowExplosion(target.Center, 1.5f);
                ThemedParticles.SwanLakeMusicalImpact(target.Center, 1.2f, true);
                ThemedParticles.SwanLakeFractalGemBurst(target.Center, Color.White, 1.0f, 8, true);
                
                // Halo rings!
                CustomParticles.HaloRing(target.Center, Color.White, 0.8f, 20);
                CustomParticles.HaloRing(target.Center, Color.Black, 0.6f, 18);
                
                // Screen shake removed - weapons/accessories should not cause screen shake
            }
            
            // Legacy critical hit effect (kept for additional visual feedback)
            if (hasPendantOfTheTwoSwans && pendantIsBlackMode && hit.Crit)
            {
                // === VIVID BLACK AND WHITE FLARES ===
                // Main slash arc with alternating black and white flames
                for (int i = 0; i < 30; i++)
                {
                    float angle = Player.direction * (-MathHelper.PiOver4 + MathHelper.PiOver2 * i / 30f);
                    Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (5f + Main.rand.NextFloat(4f));
                    
                    // Alternating vivid black and white flames
                    if (i % 2 == 0)
                    {
                        // Intense black flame
                        Dust black = Dust.NewDustPerfect(target.Center, DustID.Smoke, vel, 220, Color.Black, 2.2f);
                        black.noGravity = true;
                        black.fadeIn = 1.5f;
                    }
                    else
                    {
                        // Brilliant white flame
                        Dust white = Dust.NewDustPerfect(target.Center, DustID.WhiteTorch, vel * 1.1f, 50, default, 1.8f);
                        white.noGravity = true;
                        white.fadeIn = 1.3f;
                    }
                }
                
                // === PEARLESCENT RAINBOW EXPLOSION ===
                // Radial burst of rainbow-tinted pearlescent particles
                for (int i = 0; i < 20; i++)
                {
                    float angle = MathHelper.TwoPi * i / 20f;
                    Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(3f, 6f);
                    
                    // Rainbow cycle through pearlescent colors
                    Color rainbow = (i % 6) switch
                    {
                        0 => new Color(255, 200, 200), // Pearl pink
                        1 => new Color(255, 230, 200), // Pearl orange
                        2 => new Color(255, 255, 200), // Pearl yellow
                        3 => new Color(200, 255, 200), // Pearl green
                        4 => new Color(200, 230, 255), // Pearl blue
                        _ => new Color(230, 200, 255)  // Pearl violet
                    };
                    
                    Dust pearlRainbow = Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(10f, 10f),
                        DustID.TintableDustLighted, vel, 0, rainbow, 1.4f);
                    pearlRainbow.noGravity = true;
                    pearlRainbow.fadeIn = 1.2f;
                }
                
                // === CENTRAL FLASH ===
                // Bright white flash at center
                for (int i = 0; i < 8; i++)
                {
                    Dust flash = Dust.NewDustPerfect(target.Center, DustID.WhiteTorch,
                        Main.rand.NextVector2Circular(2f, 2f), 0, default, 2.5f);
                    flash.noGravity = true;
                }
                
                // === SWIRLING ACCENT PARTICLES ===
                for (int i = 0; i < 12; i++)
                {
                    float spiralAngle = i * MathHelper.TwoPi / 12f + Main.GameUpdateCount * 0.1f;
                    Vector2 spiralPos = target.Center + new Vector2((float)Math.Cos(spiralAngle), (float)Math.Sin(spiralAngle)) * 25f;
                    Vector2 spiralVel = new Vector2((float)Math.Cos(spiralAngle + MathHelper.PiOver2), (float)Math.Sin(spiralAngle + MathHelper.PiOver2)) * 2f;
                    
                    Color pearlescent = Main.rand.Next(3) switch
                    {
                        0 => new Color(255, 240, 245),
                        1 => new Color(240, 245, 255),
                        _ => new Color(250, 255, 245)
                    };
                    
                    Dust spiral = Dust.NewDustPerfect(spiralPos, DustID.TintableDustLighted, spiralVel, 0, pearlescent, 1f);
                    spiral.noGravity = true;
                }
                
                // Apply Flame of the Swan (3 seconds = 180 ticks)
                target.AddBuff(ModContent.BuffType<FlameOfTheSwan>(), 180);
                
                SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.3f, Volume = 0.8f }, target.Center);
            }
        }
        
        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            // Black mode Pendant - +25% crit damage for melee projectiles
            if (hasPendantOfTheTwoSwans && pendantIsBlackMode)
            {
                bool isMelee = proj.DamageType == DamageClass.Melee || proj.DamageType.CountsAsClass(DamageClass.Melee);
                if (isMelee)
                {
                    modifiers.CritDamage += 0.25f;
                }
            }
            
            // Black Wings - +35% minion damage in black mode
            // Works for minion projectiles AND sentry projectiles
            if (hasBlackWings && wingsIsBlackMode)
            {
                bool isSummon = proj.minion || proj.sentry || 
                               proj.DamageType == DamageClass.Summon || 
                               proj.DamageType.CountsAsClass(DamageClass.Summon);
                if (isSummon)
                {
                    modifiers.SourceDamage += 0.35f;
                }
            }
        }
        
        public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers)
        {
            // Black mode Pendant - +25% crit damage for melee
            if (hasPendantOfTheTwoSwans && pendantIsBlackMode)
            {
                bool isMelee = item.DamageType == DamageClass.Melee || item.DamageType.CountsAsClass(DamageClass.Melee);
                if (isMelee)
                {
                    modifiers.CritDamage += 0.25f;
                }
            }
        }
        
        public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
        {
            // Dual Feather Quiver - +18% ranged damage in black mode
            if (hasDualFeatherQuiver && quiverIsBlackMode)
            {
                bool isRanged = item.DamageType == DamageClass.Ranged || item.DamageType.CountsAsClass(DamageClass.Ranged);
                if (isRanged)
                {
                    damage += 0.18f;
                }
            }
            
            // Crown of the Swan - +30% magic damage in black mode
            if (hasCrownOfTheSwan && crownIsBlackMode)
            {
                bool isMagic = item.DamageType == DamageClass.Magic || item.DamageType.CountsAsClass(DamageClass.Magic);
                if (isMagic)
                {
                    damage += 0.30f;
                }
            }
        }
        
        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            // White mode Pendant - Monochromatic Halo reduces incoming damage by 20%
            if (hasPendantOfTheTwoSwans && !pendantIsBlackMode && whiteHaloActive)
            {
                modifiers.SourceDamage *= 0.80f; // 20% damage reduction
                
                // Visual feedback on damage taken while halo active
                if (Main.rand.NextBool(2))
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 pos = Player.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 50f;
                    
                    // Flash of white and black particles
                    Dust white = Dust.NewDustPerfect(pos, DustID.WhiteTorch, Vector2.Zero, 0, default, 1.5f);
                    white.noGravity = true;
                    
                    Dust black = Dust.NewDustPerfect(pos + new Vector2(5f, 0).RotatedBy(angle), DustID.Smoke, Vector2.Zero, 220, Color.Black, 1.3f);
                    black.noGravity = true;
                }
            }
        }
        
        public override void ModifyManaCost(Item item, ref float reduce, ref float mult)
        {
            if (hasCrownOfTheSwan && item.DamageType == DamageClass.Magic)
            {
                if (crownIsBlackMode)
                    mult *= 1.15f; // +15% mana cost in black mode
                else
                    mult *= 0.80f; // -20% mana cost in white mode
            }
        }
        
        public override void ModifyShootStats(Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            // Dual Feather Quiver - +12% accuracy (tighter spread) in white mode
            // This is handled by reducing any random spread - for now just a stat note
        }
        
        public override bool Shoot(Item item, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Dual Feather Quiver - White mode healing trails
            if (hasDualFeatherQuiver && !quiverIsBlackMode && item.DamageType == DamageClass.Ranged)
            {
                // White flame trail effect on projectile spawn
                for (int i = 0; i < 5; i++)
                {
                    Dust trail = Dust.NewDustPerfect(position, DustID.WhiteTorch,
                        velocity.SafeNormalize(Vector2.Zero) * -2f + Main.rand.NextVector2Circular(1f, 1f),
                        100, default, 0.8f);
                    trail.noGravity = true;
                }
            }
            
            return true;
        }
        
        public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
        {
            // White Wings - Minions protect, reduce damage taken when minions nearby
            // This is a passive effect that should always work when accessory is equipped
            if (hasBlackWings && !wingsIsBlackMode)
            {
                ApplyMinionProtection(ref modifiers);
            }
        }
        
        public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
        {
            // White Wings - Minions protect, reduce damage taken when minions nearby
            if (hasBlackWings && !wingsIsBlackMode)
            {
                ApplyMinionProtection(ref modifiers);
            }
        }
        
        private void ApplyMinionProtection(ref Player.HurtModifiers modifiers)
        {
            int nearbyMinions = 0;
            foreach (Projectile proj in Main.ActiveProjectiles)
            {
                if (proj.owner == Player.whoAmI && proj.minion && Vector2.Distance(proj.Center, Player.Center) < 200f)
                    nearbyMinions++;
            }
            
            if (nearbyMinions > 0)
            {
                float reduction = Math.Min(0.25f, nearbyMinions * 0.05f); // Up to 25% DR
                modifiers.SourceDamage *= (1f - reduction);
                
                // White shield flash
                for (int i = 0; i < 10; i++)
                {
                    Dust shield = Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(30f, 30f),
                        DustID.WhiteTorch, Vector2.Zero, 100, default, 1f);
                    shield.noGravity = true;
                }
            }
        }
        
        // Toggle mode method - called by right-click on accessory
        public void TogglePendantMode()
        {
            pendantIsBlackMode = !pendantIsBlackMode;
            string mode = pendantIsBlackMode ? "Black Swan (Odile) - Offensive" : "White Swan (Odette) - Defensive";
            Color color = pendantIsBlackMode ? new Color(30, 30, 40) : new Color(240, 245, 255);
            Main.NewText($"Pendant of the Two Swans: {mode}", color);
            SoundEngine.PlaySound(SoundID.Item4, Player.Center);
        }
        
        public void ToggleQuiverMode()
        {
            quiverIsBlackMode = !quiverIsBlackMode;
            string mode = quiverIsBlackMode ? "Black Swan - Pierce & DoT" : "White Swan - Healing Trails";
            Color color = quiverIsBlackMode ? new Color(30, 30, 40) : new Color(240, 245, 255);
            Main.NewText($"Dual Feather Quiver: {mode}", color);
            SoundEngine.PlaySound(SoundID.Item4, Player.Center);
        }
        
        public void ToggleCrownMode()
        {
            crownIsBlackMode = !crownIsBlackMode;
            string mode = crownIsBlackMode ? "Black Swan - Power (+30% dmg, +15% cost)" : "White Swan - Efficiency (-20% cost, wisps)";
            Color color = crownIsBlackMode ? new Color(30, 30, 40) : new Color(240, 245, 255);
            Main.NewText($"Crown of the Swan: {mode}", color);
            SoundEngine.PlaySound(SoundID.Item4, Player.Center);
        }
        
        public void ToggleWingsMode()
        {
            wingsIsBlackMode = !wingsIsBlackMode;
            string mode = wingsIsBlackMode ? "Black Swan - Aggressive (+35% minion dmg)" : "White Swan - Protective (DR when near minions)";
            Color color = wingsIsBlackMode ? new Color(30, 30, 40) : new Color(240, 245, 255);
            Main.NewText($"Black Wings of Monochromatic Dawn: {mode}", color);
            SoundEngine.PlaySound(SoundID.Item4, Player.Center);
        }
    }
    
    /// <summary>
    /// Drawing layer for floating Swan Lake accessory visuals (Pendant and Quiver orbit the player).
    /// Uses 6x6 sprite sheets (36 frames) for smooth animation.
    /// </summary>
    public class SwanLakeAccessoryDrawLayer : PlayerDrawLayer
    {
        private Asset<Texture2D> pendantFloatTexture;
        private Asset<Texture2D> quiverFloatTexture;
        
        private const int FrameColumns = 6;
        private const int FrameRows = 6;
        private const int TotalFrames = 36;
        
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.BackAcc);
        
        public override bool GetDefaultVisibility(Terraria.DataStructures.PlayerDrawSet drawInfo)
        {
            var modPlayer = drawInfo.drawPlayer.GetModPlayer<SwanLakeAccessoryPlayer>();
            return modPlayer.hasPendantOfTheTwoSwans || modPlayer.hasDualFeatherQuiver;
        }
        
        /// <summary>
        /// Gets the source rectangle for the current animation frame from a 6x6 sprite sheet.
        /// </summary>
        private Rectangle GetFrameRect(Texture2D texture, int frame)
        {
            int frameWidth = texture.Width / FrameColumns;
            int frameHeight = texture.Height / FrameRows;
            int col = frame % FrameColumns;
            int row = frame / FrameColumns;
            return new Rectangle(col * frameWidth, row * frameHeight, frameWidth, frameHeight);
        }
        
        protected override void Draw(ref Terraria.DataStructures.PlayerDrawSet drawInfo)
        {
            var modPlayer = drawInfo.drawPlayer.GetModPlayer<SwanLakeAccessoryPlayer>();
            Player player = drawInfo.drawPlayer;
            
            // Load textures if needed
            if (pendantFloatTexture == null)
                pendantFloatTexture = ModContent.Request<Texture2D>("MagnumOpus/Content/SwanLake/Accessories/PendantOfTheTwoSwans_Float");
            if (quiverFloatTexture == null)
                quiverFloatTexture = ModContent.Request<Texture2D>("MagnumOpus/Content/SwanLake/Accessories/DualFeatherQuiver_Float");
            
            float baseAngle = modPlayer.floatAngle;
            int currentFrame = modPlayer.floatAnimationFrame;
            
            // Draw Pendant of the Two Swans float
            if (modPlayer.hasPendantOfTheTwoSwans && pendantFloatTexture.IsLoaded)
            {
                Texture2D texture = pendantFloatTexture.Value;
                Rectangle sourceRect = GetFrameRect(texture, currentFrame);
                Vector2 origin = new Vector2(sourceRect.Width / 2f, sourceRect.Height / 2f);
                
                // Orbit position with gentle bobbing
                float bob = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 3f;
                Vector2 offset = new Vector2((float)Math.Cos(baseAngle) * 32f, (float)Math.Sin(baseAngle) * 16f - 38f + bob);
                Vector2 drawPos = player.Center + offset - Main.screenPosition;
                
                SpriteEffects effects = SpriteEffects.None;
                Color lightColor = Lighting.GetColor((int)(player.Center.X / 16), (int)(player.Center.Y / 16));
                
                // Mode-based glow color
                Color glowColor = modPlayer.pendantIsBlackMode 
                    ? new Color(20, 20, 30, 0) * 0.6f 
                    : new Color(240, 245, 255, 0) * 0.5f;
                
                // Glow effect
                for (int i = 0; i < 4; i++)
                {
                    Vector2 glowOffset = new Vector2(2f, 0f).RotatedBy(i * MathHelper.PiOver2);
                    drawInfo.DrawDataCache.Add(new Terraria.DataStructures.DrawData(
                        texture,
                        drawPos + glowOffset,
                        sourceRect,
                        glowColor,
                        0f,
                        origin,
                        1f,
                        effects,
                        0
                    ));
                }
                
                // Pearlescent shimmer overlay
                float shimmer = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.5f + 0.5f;
                Color pearlColor = Color.Lerp(new Color(255, 240, 245), new Color(240, 245, 255), shimmer) * 0.3f;
                pearlColor.A = 0;
                
                drawInfo.DrawDataCache.Add(new Terraria.DataStructures.DrawData(
                    texture,
                    drawPos,
                    sourceRect,
                    pearlColor,
                    0f,
                    origin,
                    1.1f,
                    effects,
                    0
                ));
                
                // Main texture
                drawInfo.DrawDataCache.Add(new Terraria.DataStructures.DrawData(
                    texture,
                    drawPos,
                    sourceRect,
                    lightColor,
                    0f,
                    origin,
                    1f,
                    effects,
                    0
                ));
            }
            
            // Draw Dual Feather Quiver float (opposite side)
            if (modPlayer.hasDualFeatherQuiver && quiverFloatTexture.IsLoaded)
            {
                Texture2D texture = quiverFloatTexture.Value;
                Rectangle sourceRect = GetFrameRect(texture, currentFrame);
                Vector2 origin = new Vector2(sourceRect.Width / 2f, sourceRect.Height / 2f);
                
                // Orbit on opposite side with gentle bobbing (offset phase)
                float bob = (float)Math.Sin(Main.GameUpdateCount * 0.05f + MathHelper.Pi) * 3f;
                Vector2 offset = new Vector2((float)Math.Cos(baseAngle + MathHelper.Pi) * 36f, (float)Math.Sin(baseAngle + MathHelper.Pi) * 14f - 34f + bob);
                Vector2 drawPos = player.Center + offset - Main.screenPosition;
                
                SpriteEffects effects = SpriteEffects.None;
                Color lightColor = Lighting.GetColor((int)(player.Center.X / 16), (int)(player.Center.Y / 16));
                
                // Mode-based glow color
                Color glowColor = modPlayer.quiverIsBlackMode 
                    ? new Color(20, 20, 30, 0) * 0.6f 
                    : new Color(240, 245, 255, 0) * 0.5f;
                
                // Glow effect
                for (int i = 0; i < 4; i++)
                {
                    Vector2 glowOffset = new Vector2(2f, 0f).RotatedBy(i * MathHelper.PiOver2);
                    drawInfo.DrawDataCache.Add(new Terraria.DataStructures.DrawData(
                        texture,
                        drawPos + glowOffset,
                        sourceRect,
                        glowColor,
                        0f,
                        origin,
                        1f,
                        effects,
                        0
                    ));
                }
                
                // Pearlescent shimmer overlay
                float shimmer = (float)Math.Sin(Main.GameUpdateCount * 0.05f + 1f) * 0.5f + 0.5f;
                Color pearlColor = Color.Lerp(new Color(255, 240, 245), new Color(240, 245, 255), shimmer) * 0.3f;
                pearlColor.A = 0;
                
                drawInfo.DrawDataCache.Add(new Terraria.DataStructures.DrawData(
                    texture,
                    drawPos,
                    sourceRect,
                    pearlColor,
                    0f,
                    origin,
                    1.1f,
                    effects,
                    0
                ));
                
                // Main texture
                drawInfo.DrawDataCache.Add(new Terraria.DataStructures.DrawData(
                    texture,
                    drawPos,
                    sourceRect,
                    lightColor,
                    0f,
                    origin,
                    1f,
                    effects,
                    0
                ));
            }
        }
    }
}
