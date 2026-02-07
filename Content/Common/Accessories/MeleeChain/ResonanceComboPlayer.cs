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
        
        // ===== POST-FATE TIER 7-10 CHAIN =====
        public bool hasNocturnalSymphonyBand;   // T7: Max 70, +2 at night, constellation trails at 50+, Starfall Slash at 60
        public bool hasInfernalFortissimoBandT8;// T8: Max 80, Judgment Burn at 60+, no decay during bosses, Hellfire Crescendo at 70
        public bool hasJubilantCrescendoBand;   // T9: Max 90, 2% lifesteal at 70+, +5 on kill, Blooming Fury at 80
        public bool hasEternalResonanceBand;    // T10: Max 100, never decays, temporal echoes at 80+, Temporal Finale at 90
        
        // ===== FUSION ACCESSORIES =====
        public bool hasStarfallJudgmentGauntlet;      // Fusion T1: Nocturnal + Infernal, Max 85
        public bool hasTriumphantCosmosGauntlet;      // Fusion T2: Starfall + Jubilant, Max 95
        public bool hasGauntletOfTheEternalSymphony;  // Fusion T3: Triumphant + Eternal, Max 100
        
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
        
        // Post-Fate theme colors
        private static readonly Color NachtmusikGold = new Color(255, 215, 0);
        private static readonly Color DiesIraeCrimson = new Color(180, 40, 40);
        private static readonly Color OdeToJoyIridescent = new Color(255, 220, 255);
        private static readonly Color ClairDeLuneBrass = new Color(205, 170, 125);
        
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
            
            // Post-Fate T7-T10 flags
            hasNocturnalSymphonyBand = false;
            hasInfernalFortissimoBandT8 = false;
            hasJubilantCrescendoBand = false;
            hasEternalResonanceBand = false;
            
            // Fusion flags
            hasStarfallJudgmentGauntlet = false;
            hasTriumphantCosmosGauntlet = false;
            hasGauntletOfTheEternalSymphony = false;
            
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
            // Ultimate Fusion and T10 Eternal
            if (hasGauntletOfTheEternalSymphony || hasEternalResonanceBand)
                maxResonance = 100;
            // Fusion Tier 2
            else if (hasTriumphantCosmosGauntlet)
                maxResonance = 95;
            // T9 Jubilant
            else if (hasJubilantCrescendoBand)
                maxResonance = 90;
            // Fusion Tier 1
            else if (hasStarfallJudgmentGauntlet)
                maxResonance = 85;
            // T8 Infernal
            else if (hasInfernalFortissimoBandT8)
                maxResonance = 80;
            // T7 Nocturnal
            else if (hasNocturnalSymphonyBand)
                maxResonance = 70;
            // T6 Fate
            else if (hasFatesCosmicSymphony)
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
            
            // Decay rate modifiers
            // Default: 2 seconds
            decayRate = 120;
            
            // Moonlit Sonata Band: Slower decay at night
            if (hasMoonlitSonataBand && !Main.dayTime)
            {
                decayRate = 180; // 3 seconds instead of 2
            }
            
            // Nocturnal Symphony Band: Extra resonance at night handled in OnMeleeHit
            
            // T8 Infernal: No decay during boss fights
            if (hasInfernalFortissimoBandT8 && AnyBossAlive())
            {
                decayRate = int.MaxValue; // Effectively no decay
            }
            
            // T10 Eternal and Ultimate Fusion: Resonance never decays
            if (hasEternalResonanceBand || hasGauntletOfTheEternalSymphony)
            {
                decayRate = int.MaxValue; // Never decay
            }
            
            // T9 Jubilant: 2% lifesteal at 70+ stacks (handled in OnHitNPC)
            
            // T10 Eternal: Time slow at 100 stacks (applied to nearby enemies)
            if ((hasEternalResonanceBand || hasGauntletOfTheEternalSymphony) && resonanceStacks >= 100)
            {
                // Time slow effect handled in GlobalNPC
            }
        }
        
        /// <summary>
        /// Checks if any boss is currently alive.
        /// </summary>
        private bool AnyBossAlive()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.boss)
                    return true;
            }
            return false;
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
            
            // T7 Nocturnal: +2 per hit at night
            if (hasNocturnalSymphonyBand && !Main.dayTime)
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
        
        /// <summary>
        /// Called when the player kills an enemy. Grants bonus resonance for T9+.
        /// </summary>
        public void OnKill(NPC target)
        {
            if (!hasResonantRhythmBand || maxResonance <= 0)
                return;
            
            // T9 Jubilant: +5 resonance on kill
            if (hasJubilantCrescendoBand || hasTriumphantCosmosGauntlet || hasGauntletOfTheEternalSymphony)
            {
                int oldStacks = resonanceStacks;
                resonanceStacks = Math.Min(resonanceStacks + 5, maxResonance);
                
                if (resonanceStacks > oldStacks)
                {
                    // Special kill VFX
                    Vector2 pos = target.Center;
                    for (int i = 0; i < 5; i++)
                    {
                        Vector2 vel = Main.rand.NextVector2Circular(3f, 3f) - Vector2.UnitY * 2f;
                        CustomParticles.GenericFlare(pos + Main.rand.NextVector2Circular(20f, 20f), OdeToJoyIridescent, 0.35f, 20);
                    }
                }
            }
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
            // Ultimate and T10
            if (hasGauntletOfTheEternalSymphony) return ClairDeLuneBrass;
            if (hasEternalResonanceBand) return ClairDeLuneBrass;
            
            // Fusion Tier 2
            if (hasTriumphantCosmosGauntlet) return OdeToJoyIridescent;
            
            // T9
            if (hasJubilantCrescendoBand) return OdeToJoyIridescent;
            
            // Fusion Tier 1
            if (hasStarfallJudgmentGauntlet) return Color.Lerp(NachtmusikGold, DiesIraeCrimson, 0.5f);
            
            // T8
            if (hasInfernalFortissimoBandT8) return DiesIraeCrimson;
            
            // T7
            if (hasNocturnalSymphonyBand) return NachtmusikGold;
            
            // T6 and earlier
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
