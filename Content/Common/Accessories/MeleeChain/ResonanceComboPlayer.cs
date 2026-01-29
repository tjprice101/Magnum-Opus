using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Common.Accessories.MeleeChain
{
    /// <summary>
    /// ModPlayer that handles the Resonance Combo System for the melee accessory chain.
    /// Resonance stacks build when hitting enemies with melee attacks and decay over time.
    /// Different accessory tiers unlock higher max stacks and special effects at stack thresholds.
    /// </summary>
    public class ResonanceComboPlayer : ModPlayer
    {
        // ===== RESONANCE STACK STATE =====
        /// <summary>Current number of Resonance stacks (0 to maxResonance)</summary>
        public int resonanceStacks;
        
        /// <summary>Maximum resonance stacks allowed (increases with better accessories)</summary>
        public int maxResonance = 0;
        
        /// <summary>Timer for stack decay (resets on hit)</summary>
        private int decayTimer;
        
        /// <summary>Frames between each stack decay</summary>
        public int decayRate = 120; // 2 seconds base
        
        // ===== ACCESSORY FLAGS =====
        public bool hasResonantRhythmBand;      // Base tier: enables system
        public bool hasSpringTempoCharm;        // +8% melee speed at 10+ stacks
        public bool hasSolarCrescendoRing;      // Scorched debuff at 15+ stacks
        public bool hasHarvestRhythmSignet;     // 1% lifesteal at 20+ stacks
        public bool hasPermafrostCadenceSeal;   // Freeze nearby enemies at 25+ stacks
        public bool hasVivaldisTempoMaster;     // Consume 30 for seasonal burst
        
        // Post-Moon Lord theme chain
        public bool hasMoonlitSonataBand;       // Slower decay at night, consume 25 for moonbeam
        public bool hasHeroicCrescendo;         // Crits grant +2 resonance, consume 30 for dash
        public bool hasInfernalFortissimo;      // Max 50, consume 40 for bell shockwave
        public bool hasEnigmasDissonance;       // Random ±3 fluctuation, Paradox DoT at 45+
        public bool hasSwansPerfectMeasure;     // Graceful hits +2, consume 35 for feather storm
        public bool hasFatesCosmicSymphony;     // Max 60, consume 50 for reality-rending slash
        
        // ===== SPECIAL STATE =====
        public bool consumedResonanceThisFrame; // Prevent multiple consumptions per frame
        private int gracefulTimer;              // Frames since taking damage (for Swan's)
        private int burstCooldown;              // Cooldown for special burst abilities
        
        // ===== COLORS =====
        private static readonly Color ResonanceBasePurple = new Color(180, 130, 255);
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
        
        public override void ResetEffects()
        {
            // Reset all accessory flags each frame
            hasResonantRhythmBand = false;
            hasSpringTempoCharm = false;
            hasSolarCrescendoRing = false;
            hasHarvestRhythmSignet = false;
            hasPermafrostCadenceSeal = false;
            hasVivaldisTempoMaster = false;
            hasMoonlitSonataBand = false;
            hasHeroicCrescendo = false;
            hasInfernalFortissimo = false;
            hasEnigmasDissonance = false;
            hasSwansPerfectMeasure = false;
            hasFatesCosmicSymphony = false;
            
            consumedResonanceThisFrame = false;
        }
        
        public override void PostUpdateEquips()
        {
            // If no resonance accessory equipped, clear stacks
            if (!hasResonantRhythmBand)
            {
                resonanceStacks = 0;
                maxResonance = 0;
                return;
            }
            
            // Determine max resonance based on equipped accessories (highest tier wins)
            DetermineMaxResonance();
            
            // Apply stack effects based on current stacks
            ApplyStackEffects();
            
            // Handle decay
            HandleDecay();
            
            // Handle Enigma's random fluctuation
            if (hasEnigmasDissonance && Main.rand.NextBool(60)) // Once per second on average
            {
                int fluctuation = Main.rand.Next(-3, 4); // -3 to +3
                resonanceStacks = Math.Clamp(resonanceStacks + fluctuation, 0, maxResonance);
            }
            
            // Track graceful timer for Swan's Perfect Measure
            gracefulTimer++;
            
            // Cooldown management
            if (burstCooldown > 0)
                burstCooldown--;
        }
        
        private void DetermineMaxResonance()
        {
            // Hierarchy: Higher tier overrides lower tier max
            if (hasFatesCosmicSymphony)
                maxResonance = 60;
            else if (hasInfernalFortissimo)
                maxResonance = 50;
            else if (hasEnigmasDissonance || hasSwansPerfectMeasure || hasHeroicCrescendo || hasMoonlitSonataBand)
                maxResonance = 40;
            else if (hasVivaldisTempoMaster)
                maxResonance = 40;
            else if (hasPermafrostCadenceSeal)
                maxResonance = 30;
            else if (hasHarvestRhythmSignet)
                maxResonance = 25;
            else if (hasSolarCrescendoRing)
                maxResonance = 20;
            else if (hasSpringTempoCharm)
                maxResonance = 15;
            else
                maxResonance = 10;
            
            // Clamp current stacks to new max
            resonanceStacks = Math.Min(resonanceStacks, maxResonance);
        }
        
        private void ApplyStackEffects()
        {
            // Spring Tempo Charm: +8% melee speed at 10+ stacks
            if (hasSpringTempoCharm && resonanceStacks >= 10)
            {
                Player.GetAttackSpeed(DamageClass.Melee) += 0.08f;
            }
            
            // Enigma's Dissonance: Paradox DoT at 45+ stacks
            // (Applied in OnHitNPC instead - on enemies)
            
            // Moonlit Sonata Band: Slower decay at night
            if (hasMoonlitSonataBand && !Main.dayTime)
            {
                decayRate = 180; // 3 seconds instead of 2
            }
            else
            {
                decayRate = 120; // Default 2 seconds
            }
        }
        
        private void HandleDecay()
        {
            decayTimer++;
            
            if (decayTimer >= decayRate && resonanceStacks > 0)
            {
                resonanceStacks--;
                decayTimer = 0;
                
                // Visual feedback on decay
                if (resonanceStacks > 0)
                {
                    Vector2 pos = Player.Center + Main.rand.NextVector2Circular(20f, 20f);
                    CustomParticles.GenericGlow(pos, Vector2.UnitY * -1f, GetResonanceColor() * 0.5f, 0.2f, 15, true);
                }
            }
        }
        
        public override void OnHurt(Player.HurtInfo info)
        {
            // Reset graceful timer when taking damage
            gracefulTimer = 0;
        }
        
        /// <summary>
        /// Called when the player hits an enemy with a melee attack.
        /// Adds resonance stacks based on equipped accessories.
        /// </summary>
        public void OnMeleeHit(NPC target, bool crit)
        {
            if (!hasResonantRhythmBand || maxResonance <= 0)
                return;
            
            // Reset decay timer on hit
            decayTimer = 0;
            
            // Base stack gain
            int stackGain = 1;
            
            // Heroic Crescendo: Crits grant +2 resonance
            if (hasHeroicCrescendo && crit)
            {
                stackGain += 2;
            }
            
            // Swan's Perfect Measure: Graceful hits (no damage for 3s) grant +2
            if (hasSwansPerfectMeasure && gracefulTimer >= 180)
            {
                stackGain += 2;
            }
            
            // Add stacks
            int oldStacks = resonanceStacks;
            resonanceStacks = Math.Min(resonanceStacks + stackGain, maxResonance);
            
            // Visual feedback on stack gain
            if (resonanceStacks > oldStacks)
            {
                SpawnStackGainParticles(stackGain);
            }
            
            // Check for threshold effects
            CheckThresholdEffects(target);
        }
        
        private void SpawnStackGainParticles(int stacksGained)
        {
            Color particleColor = GetResonanceColor();
            
            for (int i = 0; i < stacksGained; i++)
            {
                Vector2 pos = Player.Center + Main.rand.NextVector2Circular(15f, 15f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(1f, 2f));
                CustomParticles.GenericGlow(pos, vel, particleColor, 0.25f, 20, true);
                
                // Music note at milestones
                if ((resonanceStacks % 5 == 0) && Main.rand.NextBool(2))
                {
                    ThemedParticles.MusicNote(pos, vel * 0.5f, particleColor, 0.3f, 25);
                }
            }
        }
        
        private void CheckThresholdEffects(NPC target)
        {
            // Harvest Rhythm Signet: 1% lifesteal at 20+ stacks
            if (hasHarvestRhythmSignet && resonanceStacks >= 20)
            {
                // Lifesteal handled in MeleeChainGlobalNPC
            }
            
            // Permafrost Cadence Seal: Freeze nearby enemies at 25+ stacks
            if (hasPermafrostCadenceSeal && resonanceStacks >= 25)
            {
                // Freeze effect handled in MeleeChainGlobalNPC
            }
            
            // Enigma's Dissonance: Paradox DoT at 45+ stacks
            if (hasEnigmasDissonance && resonanceStacks >= 45)
            {
                // Apply Paradox debuff - handled in MeleeChainGlobalNPC
            }
        }
        
        /// <summary>
        /// Attempts to consume resonance stacks for a special ability.
        /// Returns true if consumption was successful.
        /// </summary>
        public bool TryConsumeResonance(int amount)
        {
            if (consumedResonanceThisFrame || burstCooldown > 0)
                return false;
            
            if (resonanceStacks >= amount)
            {
                resonanceStacks -= amount;
                consumedResonanceThisFrame = true;
                burstCooldown = 120; // 2 second cooldown between bursts
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Gets the color for resonance particles based on highest equipped tier.
        /// </summary>
        public Color GetResonanceColor()
        {
            if (hasFatesCosmicSymphony) return FateCrimson;
            if (hasSwansPerfectMeasure) return SwanWhite;
            if (hasEnigmasDissonance) return EnigmaPurple;
            if (hasInfernalFortissimo) return CampanellaOrange;
            if (hasHeroicCrescendo) return EroicaGold;
            if (hasMoonlitSonataBand) return MoonlightPurple;
            if (hasVivaldisTempoMaster) return Color.Lerp(SpringPink, WinterBlue, 0.5f);
            if (hasPermafrostCadenceSeal) return WinterBlue;
            if (hasHarvestRhythmSignet) return AutumnBrown;
            if (hasSolarCrescendoRing) return SummerOrange;
            if (hasSpringTempoCharm) return SpringPink;
            return ResonanceBasePurple;
        }
        
        /// <summary>
        /// Gets the percentage of resonance filled (0-1).
        /// </summary>
        public float GetResonancePercent()
        {
            if (maxResonance <= 0) return 0f;
            return (float)resonanceStacks / maxResonance;
        }
        
        /// <summary>
        /// Returns true if the player is "graceful" (hasn't taken damage for 3 seconds).
        /// Used by Swan's Perfect Measure.
        /// </summary>
        public bool IsGraceful => gracefulTimer >= 180;
        
        /// <summary>
        /// Returns true if burst abilities are on cooldown.
        /// </summary>
        public bool IsBurstOnCooldown => burstCooldown > 0;
        
        /// <summary>
        /// Gets the remaining grace timer in frames. Used for UI display.
        /// </summary>
        public int GraceTimer => gracefulTimer;
        
        /// <summary>
        /// Gets the remaining burst cooldown in frames. Used for UI display.
        /// </summary>
        public int BurstCooldown => burstCooldown;
    }
}
