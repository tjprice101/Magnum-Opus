using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Common.Accessories.RangerChain
{
    /// <summary>
    /// ModPlayer that handles the Marked for Death System for the ranger accessory chain.
    /// Ranged attacks mark enemies, and marked enemies take bonus effects based on equipped accessories.
    /// Higher tiers increase mark duration, add damage bonuses, death explosions, and special effects.
    /// </summary>
    public class MarkingPlayer : ModPlayer
    {
        // ===== MARK CONFIGURATION =====
        /// <summary>Base duration of marks in frames (60 = 1 second)</summary>
        public int baseMarkDuration = 0;
        
        /// <summary>Maximum number of enemies that can be marked at once</summary>
        public int maxMarkedEnemies = 3;
        
        /// <summary>Bonus damage multiplier against marked enemies (0.05 = 5%)</summary>
        public float markedDamageBonus = 0f;
        
        /// <summary>Whether marked enemies are slowed</summary>
        public bool markedSlowsEnemies = false;
        
        /// <summary>Slow percentage (0.15 = 15% slower)</summary>
        public float markSlowPercent = 0f;
        
        // ===== ACCESSORY FLAGS =====
        public bool hasResonantSpotter;         // Base tier: enables marking system, 5s marks
        public bool hasSpringHuntersLens;       // Marks last 8s, 10% heart drop chance on marked hit
        public bool hasSolarTrackersBadge;      // Marks last 10s, +5% damage from ALL sources
        public bool hasHarvestReapersMark;      // Death explosion + chain marking
        public bool hasPermafrostHuntersEye;    // 15% slow, kill refreshes nearby marks
        public bool hasVivaldisSeSonalSight;    // Seasonal debuffs, 15s marks
        
        // Post-Moon Lord theme chain
        public bool hasMoonlitPredatorsGaze;    // Mark up to 8 enemies, visible through walls
        public bool hasHeroicDeadeye;           // +8% damage, first hit auto-crit
        public bool hasInfernalExecutionersBrand; // Burn DoT, +50% explosion radius
        public bool hasEnigmasParadoxMark;      // 15% spread chance, dimensional marks
        public bool hasSwansGracefulHunt;       // Perfect shots = Swan Mark (+15% crit)
        public bool hasFatesCosmicVerdict;      // +12% damage, boss bonus loot
        
        // ===== SPECIAL STATE =====
        /// <summary>Timer for tracking "perfect shot" (no damage taken for 3 seconds)</summary>
        private int perfectShotTimer;
        
        /// <summary>Whether next shot qualifies as a "perfect shot"</summary>
        public bool IsPerfectShot => perfectShotTimer >= 180; // 3 seconds
        
        /// <summary>Cooldown for auto-crit effect (per enemy)</summary>
        private int autoCritCooldown;
        
        // ===== COLORS =====
        public static readonly Color MarkBaseRed = new Color(255, 100, 100);
        public static readonly Color SpringGreen = new Color(144, 238, 144);
        public static readonly Color SummerOrange = new Color(255, 140, 0);
        public static readonly Color AutumnBrown = new Color(180, 100, 40);
        public static readonly Color WinterBlue = new Color(150, 220, 255);
        public static readonly Color MoonlightPurple = new Color(138, 43, 226);
        public static readonly Color EroicaGold = new Color(255, 200, 80);
        public static readonly Color CampanellaOrange = new Color(255, 140, 40);
        public static readonly Color EnigmaPurple = new Color(140, 60, 200);
        public static readonly Color SwanWhite = new Color(255, 255, 255);
        public static readonly Color FateCrimson = new Color(200, 80, 120);
        
        public override void ResetEffects()
        {
            // Reset all accessory flags each frame
            hasResonantSpotter = false;
            hasSpringHuntersLens = false;
            hasSolarTrackersBadge = false;
            hasHarvestReapersMark = false;
            hasPermafrostHuntersEye = false;
            hasVivaldisSeSonalSight = false;
            hasMoonlitPredatorsGaze = false;
            hasHeroicDeadeye = false;
            hasInfernalExecutionersBrand = false;
            hasEnigmasParadoxMark = false;
            hasSwansGracefulHunt = false;
            hasFatesCosmicVerdict = false;
            
            // Reset configuration
            baseMarkDuration = 0;
            maxMarkedEnemies = 3;
            markedDamageBonus = 0f;
            markedSlowsEnemies = false;
            markSlowPercent = 0f;
        }
        
        public override void PostUpdateEquips()
        {
            // If no marking accessory equipped, do nothing
            if (!hasResonantSpotter)
            {
                perfectShotTimer = 0;
                return;
            }
            
            // Determine mark configuration based on equipped accessories
            DetermineMarkConfiguration();
            
            // Track perfect shot timer for Swan's Graceful Hunt
            perfectShotTimer++;
            
            // Cooldown management
            if (autoCritCooldown > 0)
                autoCritCooldown--;
        }
        
        private void DetermineMarkConfiguration()
        {
            // Base tier: 5 second marks (300 frames)
            baseMarkDuration = 300;
            maxMarkedEnemies = 3;
            
            // Spring Hunter's Lens: 8 second marks
            if (hasSpringHuntersLens)
                baseMarkDuration = 480;
            
            // Solar Tracker's Badge: 10 second marks, +5% damage
            if (hasSolarTrackersBadge)
            {
                baseMarkDuration = 600;
                markedDamageBonus = 0.05f;
            }
            
            // Harvest Reaper's Mark: same duration, has death explosion
            // (explosion handled in GlobalNPC)
            
            // Permafrost Hunter's Eye: adds slow
            if (hasPermafrostHuntersEye)
            {
                markedSlowsEnemies = true;
                markSlowPercent = 0.15f;
            }
            
            // Vivaldi's Seasonal Sight: 15 second marks
            if (hasVivaldisSeSonalSight)
                baseMarkDuration = 900;
            
            // Post-Moon Lord upgrades
            
            // Moonlit Predator's Gaze: Mark up to 8 enemies
            if (hasMoonlitPredatorsGaze)
                maxMarkedEnemies = 8;
            
            // Heroic Deadeye: +8% damage bonus
            if (hasHeroicDeadeye)
                markedDamageBonus = Math.Max(markedDamageBonus, 0.08f);
            
            // Infernal Executioner's Brand: keeps previous bonuses
            // (burn handled in GlobalNPC)
            
            // Enigma's Paradox Mark: spread chance
            // (spread handled in GlobalNPC)
            
            // Swan's Graceful Hunt: perfect shot bonus
            // (handled via IsPerfectShot)
            
            // Fate's Cosmic Verdict: +12% damage
            if (hasFatesCosmicVerdict)
                markedDamageBonus = Math.Max(markedDamageBonus, 0.12f);
        }
        
        /// <summary>
        /// Called when the player takes damage. Resets perfect shot timer.
        /// </summary>
        public override void OnHurt(Player.HurtInfo info)
        {
            perfectShotTimer = 0;
        }
        
        /// <summary>
        /// Returns the current mark color based on equipped accessories.
        /// </summary>
        public Color GetMarkColor()
        {
            if (hasFatesCosmicVerdict) return FateCrimson;
            if (hasSwansGracefulHunt) return SwanWhite;
            if (hasEnigmasParadoxMark) return EnigmaPurple;
            if (hasInfernalExecutionersBrand) return CampanellaOrange;
            if (hasHeroicDeadeye) return EroicaGold;
            if (hasMoonlitPredatorsGaze) return MoonlightPurple;
            if (hasVivaldisSeSonalSight) return GetSeasonalColor();
            if (hasPermafrostHuntersEye) return WinterBlue;
            if (hasHarvestReapersMark) return AutumnBrown;
            if (hasSolarTrackersBadge) return SummerOrange;
            if (hasSpringHuntersLens) return SpringGreen;
            return MarkBaseRed;
        }
        
        /// <summary>
        /// Returns a color based on the current season for Vivaldi's Seasonal Sight.
        /// </summary>
        private Color GetSeasonalColor()
        {
            // Cycle through seasons based on game time
            int seasonIndex = (int)(Main.GameUpdateCount / 600) % 4;
            return seasonIndex switch
            {
                0 => SpringGreen,
                1 => SummerOrange,
                2 => AutumnBrown,
                3 => WinterBlue,
                _ => MarkBaseRed
            };
        }
        
        /// <summary>
        /// Tries to use auto-crit against a target. Returns true if auto-crit should apply.
        /// </summary>
        public bool TryUseAutoCrit()
        {
            if (!hasHeroicDeadeye || autoCritCooldown > 0)
                return false;
            
            // 5 second cooldown per auto-crit
            autoCritCooldown = 300;
            return true;
        }
        
        /// <summary>
        /// Resets auto-crit cooldown. Called when hitting a previously unmarked enemy.
        /// </summary>
        public void ResetAutoCritCooldown()
        {
            autoCritCooldown = 0;
        }
        
        /// <summary>
        /// Gets the current seasonal debuff type for Vivaldi's Seasonal Sight.
        /// Returns: 0 = Spring (bloom/heal), 1 = Summer (burn), 2 = Autumn (wither), 3 = Winter (chill)
        /// </summary>
        public int GetCurrentSeasonalDebuffType()
        {
            return (int)(Main.GameUpdateCount / 600) % 4;
        }
        
        /// <summary>
        /// Counts how many enemies are currently marked by this player.
        /// </summary>
        public int CountMarkedEnemies()
        {
            int count = 0;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.TryGetGlobalNPC<MarkingGlobalNPC>(out var markNPC))
                {
                    if (markNPC.IsMarkedBy(Player.whoAmI))
                        count++;
                }
            }
            return count;
        }
    }
}
