using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Common.Accessories.MageChain
{
    /// <summary>
    /// ModPlayer that handles the Harmonic Overflow System for the magic accessory chain.
    /// The Overflow system allows casting spells beyond 0 mana (going into negative mana)
    /// with various effects triggering while in the negative mana state.
    /// Different accessory tiers allow deeper overflow and unlock special effects.
    /// </summary>
    public class OverflowPlayer : ModPlayer
    {
        // ===== OVERFLOW STATE =====
        /// <summary>Current overflow amount (how far below 0 mana we are)</summary>
        public int currentOverflow;
        
        /// <summary>Maximum overflow allowed (increases with better accessories)</summary>
        public int maxOverflow = 0;
        
        /// <summary>Whether the player is currently in overflow state (negative mana)</summary>
        public bool isInOverflow => currentOverflow > 0;
        
        /// <summary>Timer for overflow recovery</summary>
        private int overflowRecoveryTimer;
        
        // ===== ACCESSORY FLAGS =====
        public bool hasResonantOverflowGem;      // Base tier: enables system, -20 overflow
        public bool hasSpringArcaneConduit;     // -40 overflow, healing petals while negative
        public bool hasSolarManaCrucible;       // -60 overflow, spells inflict Sunburn
        public bool hasHarvestSoulVessel;       // -80 overflow, kills restore +15 mana
        public bool hasPermafrostVoidHeart;     // -100 overflow, +15% damage while negative
        public bool hasVivaldisHarmonicCore;    // -120 overflow, seasonal burst on recovery
        
        // Post-Moon Lord theme chain
        public bool hasMoonlitOverflowStar;     // At exactly 0 mana: next spell free
        public bool hasHeroicArcaneSurge;       // Going negative triggers 1s invincibility (30s CD)
        public bool hasInfernalManaInferno;     // While negative: leave fire trail
        public bool hasEnigmasNegativeSpace;    // -150 overflow, at -100: spells hit twice but take 5% HP/s
        public bool hasSwansBalancedFlow;       // Gain "Grace" buff on recovery (+20% damage 5s)
        public bool hasFatesCosmicReservoir;    // -200 overflow, at -150: spells pierce walls
        
        // Post-Fate theme chain (T7-T10)
        public bool hasNocturnalHarmonicOverflow;  // T7: -250 overflow, +10% night damage, spell echoes
        public bool hasInfernalManaCataclysm;      // T8: -300 overflow, DoT conversion, +50% potion healing
        public bool hasJubilantArcaneCelebration;  // T9: -350 overflow, healing per mana spent, +crit on recovery
        public bool hasEternalOverflowMastery;     // T10: -400 overflow, no penalty, Temporal Overflow
        
        // Post-Fate Fusion chain
        public bool hasStarfallHarmonicPendant;     // Fusion 1: T7+T8, enhanced night damage
        public bool hasTriumphantOverflowPendant;   // Fusion 2: +T9, triple theme synergy
        public bool hasPendantOfTheEternalOverflow; // Ultimate: All four themes combined
        
        // ===== SPECIAL STATE =====
        public bool zeroManaFreeSpell;          // Next spell is free (Moonlit Overflow Star)
        public int invincibilityCooldown;       // Cooldown for Heroic Arcane Surge
        public int graceBuffTimer;              // Timer for Swan's Grace buff
        public bool justRecoveredFromOverflow;  // Flag for seasonal burst trigger
        
        // ===== COLORS =====
        private static readonly Color OverflowBasePurple = new Color(100, 150, 255);
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SummerOrange = new Color(255, 140, 0);
        private static readonly Color AutumnBrown = new Color(180, 100, 40);
        private static readonly Color WinterBlue = new Color(150, 220, 255);
        private static readonly Color MoonlightPurple = new Color(138, 43, 226);
        private static readonly Color EroicaGold = new Color(255, 200, 80);
        private static readonly Color CampanellaOrange = new Color(255, 140, 40);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color SwanWhite = new Color(255, 255, 255);
        private static readonly Color FateCrimson = new Color(200, 80, 120);
        
        // Post-Fate Colors
        private static readonly Color NachtmusikGold = new Color(255, 215, 140);
        private static readonly Color DiesIraeCrimson = new Color(200, 50, 50);
        private static readonly Color OdeToJoyIridescent = new Color(220, 200, 255);
        private static readonly Color ClairDeLuneBrass = new Color(200, 170, 100);
        
        public override void ResetEffects()
        {
            // Reset all accessory flags each frame
            hasResonantOverflowGem = false;
            hasSpringArcaneConduit = false;
            hasSolarManaCrucible = false;
            hasHarvestSoulVessel = false;
            hasPermafrostVoidHeart = false;
            hasVivaldisHarmonicCore = false;
            hasMoonlitOverflowStar = false;
            hasHeroicArcaneSurge = false;
            hasInfernalManaInferno = false;
            hasEnigmasNegativeSpace = false;
            hasSwansBalancedFlow = false;
            hasFatesCosmicReservoir = false;
            
            // Post-Fate flags
            hasNocturnalHarmonicOverflow = false;
            hasInfernalManaCataclysm = false;
            hasJubilantArcaneCelebration = false;
            hasEternalOverflowMastery = false;
            hasStarfallHarmonicPendant = false;
            hasTriumphantOverflowPendant = false;
            hasPendantOfTheEternalOverflow = false;
            
            justRecoveredFromOverflow = false;
        }
        
        public override void PostUpdateEquips()
        {
            // If no overflow accessory equipped, clear overflow
            if (!hasResonantOverflowGem)
            {
                currentOverflow = 0;
                maxOverflow = 0;
                return;
            }
            
            // Determine max overflow based on equipped accessories
            DetermineMaxOverflow();
            
            // Apply overflow effects
            ApplyOverflowEffects();
            
            // Handle cooldowns
            if (invincibilityCooldown > 0)
                invincibilityCooldown--;
            
            if (graceBuffTimer > 0)
            {
                graceBuffTimer--;
                // Grace buff: +20% magic damage
                Player.GetDamage(DamageClass.Magic) += 0.20f;
            }
            
            // Check for zero mana precision (Moonlit Overflow Star)
            if (hasMoonlitOverflowStar && Player.statMana == 0 && currentOverflow == 0)
            {
                zeroManaFreeSpell = true;
            }
            else if (Player.statMana > 0)
            {
                zeroManaFreeSpell = false;
            }
            
            // Handle overflow recovery
            HandleOverflowRecovery();
            
            // Enigma's Negative Space: Take 5% max HP/s at -100 or below
            if (hasEnigmasNegativeSpace && currentOverflow >= 100)
            {
                if (Main.GameUpdateCount % 60 == 0) // Once per second
                {
                    int damage = (int)(Player.statLifeMax2 * 0.05f);
                    Player.Hurt(Terraria.DataStructures.PlayerDeathReason.ByCustomReason(Terraria.Localization.NetworkText.FromLiteral($"{Player.name} was consumed by negative space")), damage, 0, false, false, -1, false, 0, 0, 0);
                }
            }
            
            // Infernal Mana Inferno: Leave fire trail while negative
            if (hasInfernalManaInferno && isInOverflow && Player.velocity.Length() > 2f)
            {
                if (Main.GameUpdateCount % 6 == 0)
                {
                    // Spawn fire dust/projectile at player's position
                    Vector2 trailPos = Player.Center + new Vector2(0, Player.height / 2f);
                    for (int i = 0; i < 2; i++)
                    {
                        Dust fire = Dust.NewDustDirect(trailPos, 8, 4, Terraria.ID.DustID.Torch, 0, 0, 100, CampanellaOrange, 1.5f);
                        fire.noGravity = true;
                        fire.velocity = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-0.5f, 0.5f));
                    }
                    
                    // Spawn lingering fire projectile
                    if (Main.myPlayer == Player.whoAmI && Main.GameUpdateCount % 12 == 0)
                    {
                        // Note: Would spawn a fire trail projectile here
                        // For now, just visual dust effect
                        CustomParticles.GenericFlare(trailPos, CampanellaOrange, 0.3f, 30);
                    }
                }
            }
        }
        
        private void DetermineMaxOverflow()
        {
            // Hierarchy: Higher tier overrides lower tier max
            // Post-Fate Ultimate Fusion
            if (hasPendantOfTheEternalOverflow)
                maxOverflow = 400;
            // Post-Fate Fusion Tier 2
            else if (hasTriumphantOverflowPendant)
                maxOverflow = 350;
            // Post-Fate Fusion Tier 1
            else if (hasStarfallHarmonicPendant)
                maxOverflow = 300;
            // Post-Fate T10
            else if (hasEternalOverflowMastery)
                maxOverflow = 400;
            // Post-Fate T9
            else if (hasJubilantArcaneCelebration)
                maxOverflow = 350;
            // Post-Fate T8
            else if (hasInfernalManaCataclysm)
                maxOverflow = 300;
            // Post-Fate T7
            else if (hasNocturnalHarmonicOverflow)
                maxOverflow = 250;
            // Fate tier (T6)
            else if (hasFatesCosmicReservoir)
                maxOverflow = 200;
            else if (hasEnigmasNegativeSpace)
                maxOverflow = 150;
            else if (hasSwansBalancedFlow || hasInfernalManaInferno || hasHeroicArcaneSurge || hasMoonlitOverflowStar)
                maxOverflow = 120; // Same as Vivaldi's
            else if (hasVivaldisHarmonicCore)
                maxOverflow = 120;
            else if (hasPermafrostVoidHeart)
                maxOverflow = 100;
            else if (hasHarvestSoulVessel)
                maxOverflow = 80;
            else if (hasSolarManaCrucible)
                maxOverflow = 60;
            else if (hasSpringArcaneConduit)
                maxOverflow = 40;
            else
                maxOverflow = 20;
            
            // Clamp current overflow to new max
            currentOverflow = Math.Min(currentOverflow, maxOverflow);
        }
        
        private void ApplyOverflowEffects()
        {
            if (!isInOverflow) return;
            
            // Eternal Overflow Mastery: No damage penalty while in overflow (mastered!)
            if (!hasEternalOverflowMastery && !hasPendantOfTheEternalOverflow)
            {
                // Base effect: -25% magic damage while in overflow (risk!)
                Player.GetDamage(DamageClass.Magic) -= 0.25f;
                
                // Permafrost Void Heart: +15% damage while negative (offsets the penalty!)
                if (hasPermafrostVoidHeart)
                {
                    Player.GetDamage(DamageClass.Magic) += 0.15f;
                }
            }
            
            // Visual feedback while in overflow
            if (Main.rand.NextBool(10))
            {
                Vector2 pos = Player.Center + Main.rand.NextVector2Circular(25f, 35f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(1f, 2f));
                Color overflowColor = GetOverflowColor();
                CustomParticles.GenericGlow(pos, vel, overflowColor * 0.7f, 0.25f, 20, true);
            }
        }
        
        private void HandleOverflowRecovery()
        {
            if (!isInOverflow) return;
            
            // Overflow recovery: +50% faster mana regen helps recover
            // Natural recovery of 1 overflow per 30 frames (0.5 seconds)
            overflowRecoveryTimer++;
            
            if (overflowRecoveryTimer >= 30)
            {
                overflowRecoveryTimer = 0;
                currentOverflow--;
                
                // Check if we just fully recovered
                if (currentOverflow <= 0)
                {
                    currentOverflow = 0;
                    justRecoveredFromOverflow = true;
                    
                    // Swan's Balanced Flow: Gain Grace buff on recovery
                    if (hasSwansBalancedFlow)
                    {
                        graceBuffTimer = 300; // 5 seconds
                        
                        // Grace recovery VFX
                        for (int i = 0; i < 12; i++)
                        {
                            float angle = MathHelper.TwoPi * i / 12f;
                            Vector2 vel = angle.ToRotationVector2() * 3f;
                            CustomParticles.GenericFlare(Player.Center, SwanWhite, 0.4f, 20);
                        }
                    }
                    
                    // Vivaldi's Harmonic Core: Seasonal burst on recovery
                    if (hasVivaldisHarmonicCore)
                    {
                        SpawnSeasonalBurst();
                    }
                }
            }
        }
        
        /// <summary>
        /// Called when the player tries to cast a spell. Returns whether the spell can be cast
        /// and handles mana consumption including overflow.
        /// </summary>
        public bool TryConsumeManWithOverflow(int manaCost)
        {
            if (!hasResonantOverflowGem) return false; // System not enabled
            
            // Check for free spell (Moonlit Overflow Star)
            if (zeroManaFreeSpell)
            {
                zeroManaFreeSpell = false;
                
                // VFX for free spell
                CustomParticles.GenericFlare(Player.Center, MoonlightPurple, 0.6f, 25);
                CustomParticles.HaloRing(Player.Center, MoonlightPurple, 0.4f, 20);
                
                return true;
            }
            
            // Calculate how much mana we have (including allowing overflow)
            int effectiveMana = Player.statMana - currentOverflow;
            int potentialOverflow = currentOverflow + (manaCost - Player.statMana);
            
            // Check if we can afford it with overflow
            if (potentialOverflow > maxOverflow)
            {
                return false; // Can't overflow that deep
            }
            
            // If we're going into overflow
            if (manaCost > Player.statMana)
            {
                int overflowAmount = manaCost - Player.statMana;
                int previousOverflow = currentOverflow;
                
                Player.statMana = 0;
                currentOverflow += overflowAmount;
                
                // Heroic Arcane Surge: Trigger invincibility when going negative
                if (hasHeroicArcaneSurge && previousOverflow == 0 && invincibilityCooldown <= 0)
                {
                    Player.immune = true;
                    Player.immuneTime = 60; // 1 second
                    invincibilityCooldown = 1800; // 30 seconds
                    
                    // Invincibility VFX
                    CustomParticles.GenericFlare(Player.Center, EroicaGold, 0.8f, 30);
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 8f;
                        Vector2 offset = angle.ToRotationVector2() * 40f;
                        CustomParticles.GenericFlare(Player.Center + offset, EroicaGold, 0.4f, 20);
                    }
                }
                
                // VFX for entering overflow
                if (previousOverflow == 0)
                {
                    Color overflowColor = GetOverflowColor();
                    CustomParticles.HaloRing(Player.Center, overflowColor, 0.5f, 25);
                }
            }
            else
            {
                // Normal mana consumption
                Player.statMana -= manaCost;
            }
            
            return true;
        }
        
        /// <summary>
        /// Called when the player kills an enemy. Handles Harvest Soul Vessel mana restore.
        /// </summary>
        public void OnKillEnemy(NPC target)
        {
            // Harvest Soul Vessel: Killing enemies while negative restores +15 mana
            if (hasHarvestSoulVessel && isInOverflow)
            {
                // Reduce overflow by 15 (effectively restoring mana)
                currentOverflow = Math.Max(0, currentOverflow - 15);
                
                // VFX
                Vector2 soulPos = target.Center;
                CustomParticles.GenericFlare(soulPos, AutumnBrown, 0.5f, 20);
                for (int i = 0; i < 5; i++)
                {
                    Vector2 vel = (Player.Center - soulPos).SafeNormalize(Vector2.Zero) * 4f;
                    CustomParticles.GenericGlow(soulPos, vel + Main.rand.NextVector2Circular(1f, 1f), AutumnBrown, 0.3f, 30, true);
                }
            }
        }
        
        /// <summary>
        /// Called when the player casts a spell while in overflow with Spring Arcane Conduit.
        /// Spawns healing petals.
        /// </summary>
        public void SpawnHealingPetals(Vector2 spellPosition)
        {
            if (!hasSpringArcaneConduit || !isInOverflow) return;
            
            // Spawn 2-3 healing petals that drift toward player
            int petalCount = Main.rand.Next(2, 4);
            for (int i = 0; i < petalCount; i++)
            {
                Vector2 petalPos = spellPosition + Main.rand.NextVector2Circular(30f, 30f);
                Vector2 vel = (Player.Center - petalPos).SafeNormalize(Vector2.Zero) * 2f;
                
                CustomParticles.GenericGlow(petalPos, vel, SpringPink, 0.3f, 40, true);
                
                // Actual healing handled elsewhere or via projectile
            }
        }
        
        private void SpawnSeasonalBurst()
        {
            // Determine current season for burst effect
            // For simplicity, cycle based on game time
            int season = (int)((Main.time / 54000) % 4); // Changes every in-game day quarter
            
            Color burstColor;
            switch (season)
            {
                case 0: burstColor = SpringPink; break;
                case 1: burstColor = SummerOrange; break;
                case 2: burstColor = AutumnBrown; break;
                default: burstColor = WinterBlue; break;
            }
            
            // Seasonal burst VFX
            CustomParticles.GenericFlare(Player.Center, burstColor, 0.9f, 30);
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                CustomParticles.GenericGlow(Player.Center, vel, burstColor, 0.35f, 25, true);
            }
            CustomParticles.HaloRing(Player.Center, burstColor, 0.6f, 25);
            
            // Spawn seasonal damage burst projectile (would need projectile implementation)
        }
        
        /// <summary>
        /// Gets the color for overflow particles based on highest equipped tier.
        /// </summary>
        public Color GetOverflowColor()
        {
            // Post-Fate Ultimate Fusion
            if (hasPendantOfTheEternalOverflow)
            {
                // Prismatic blend of all four Post-Fate themes
                float t = (Main.GameUpdateCount * 0.015f) % 1f;
                if (t < 0.25f) return Color.Lerp(NachtmusikGold, DiesIraeCrimson, t * 4f);
                if (t < 0.5f) return Color.Lerp(DiesIraeCrimson, OdeToJoyIridescent, (t - 0.25f) * 4f);
                if (t < 0.75f) return Color.Lerp(OdeToJoyIridescent, ClairDeLuneBrass, (t - 0.5f) * 4f);
                return Color.Lerp(ClairDeLuneBrass, NachtmusikGold, (t - 0.75f) * 4f);
            }
            
            // Post-Fate Fusion Tier 2
            if (hasTriumphantOverflowPendant)
            {
                float t = (Main.GameUpdateCount * 0.02f) % 1f;
                if (t < 0.33f) return Color.Lerp(NachtmusikGold, DiesIraeCrimson, t * 3f);
                if (t < 0.67f) return Color.Lerp(DiesIraeCrimson, OdeToJoyIridescent, (t - 0.33f) * 3f);
                return Color.Lerp(OdeToJoyIridescent, NachtmusikGold, (t - 0.67f) * 3f);
            }
            
            // Post-Fate Fusion Tier 1
            if (hasStarfallHarmonicPendant)
                return Color.Lerp(NachtmusikGold, DiesIraeCrimson, (float)Math.Sin(Main.GameUpdateCount * 0.025f) * 0.5f + 0.5f);
            
            // Post-Fate T10
            if (hasEternalOverflowMastery) return ClairDeLuneBrass;
            // Post-Fate T9
            if (hasJubilantArcaneCelebration) return OdeToJoyIridescent;
            // Post-Fate T8
            if (hasInfernalManaCataclysm) return DiesIraeCrimson;
            // Post-Fate T7
            if (hasNocturnalHarmonicOverflow) return NachtmusikGold;
            
            // Fate tier (T6)
            if (hasFatesCosmicReservoir) return FateCrimson;
            if (hasSwansBalancedFlow) return SwanWhite;
            if (hasEnigmasNegativeSpace) return EnigmaPurple;
            if (hasInfernalManaInferno) return CampanellaOrange;
            if (hasHeroicArcaneSurge) return EroicaGold;
            if (hasMoonlitOverflowStar) return MoonlightPurple;
            if (hasVivaldisHarmonicCore) return Color.Lerp(SpringPink, WinterBlue, 0.5f);
            if (hasPermafrostVoidHeart) return WinterBlue;
            if (hasHarvestSoulVessel) return AutumnBrown;
            if (hasSolarManaCrucible) return SummerOrange;
            if (hasSpringArcaneConduit) return SpringPink;
            return OverflowBasePurple;
        }
        
        /// <summary>
        /// Gets the percentage of overflow used (0-1).
        /// </summary>
        public float GetOverflowPercent()
        {
            if (maxOverflow <= 0) return 0f;
            return (float)currentOverflow / maxOverflow;
        }
        
        /// <summary>
        /// Returns true if Grace buff is active (Swan's Balanced Flow).
        /// </summary>
        public bool HasGraceBuff => graceBuffTimer > 0;
        
        /// <summary>
        /// Returns true if invincibility from Heroic Arcane Surge is on cooldown.
        /// </summary>
        public bool IsInvincibilityOnCooldown => invincibilityCooldown > 0;
    }
}
