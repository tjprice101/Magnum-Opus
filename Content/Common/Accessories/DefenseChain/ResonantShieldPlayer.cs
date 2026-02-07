using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Common.Accessories.DefenseChain
{
    /// <summary>
    /// ModPlayer for the Resonant Shield System - Defense Chain
    /// Provides a regenerating barrier that absorbs damage with unique break effects
    /// </summary>
    public class ResonantShieldPlayer : ModPlayer
    {
        // ========== ACCESSORY FLAGS ==========
        // Tier 1: Resonant Barrier Core
        public bool HasResonantBarrierCore { get; set; }
        
        // Tier 2: Spring Vitality Shell
        public bool HasSpringVitalityShell { get; set; }
        
        // Tier 3: Solar Flare Aegis
        public bool HasSolarFlareAegis { get; set; }
        
        // Tier 4: Harvest Thorned Guard
        public bool HasHarvestThornedGuard { get; set; }
        
        // Tier 5: Permafrost Crystal Ward
        public bool HasPermafrostCrystalWard { get; set; }
        
        // Tier 6: Vivaldi's Seasonal Bulwark
        public bool HasVivaldisSeasonalBulwark { get; set; }
        
        // Post-Moon Lord Theme Chain
        public bool HasMoonlitGuardiansVeil { get; set; }    // T1 - Faster regen at night, invis on break
        public bool HasHeroicValorsAegis { get; set; }       // T2 - +15% damage on break
        public bool HasInfernalBellsFortress { get; set; }   // T3 - 40% shield, bell shockwave
        public bool HasEnigmasVoidShell { get; set; }        // T4 - 10% phase through attacks
        public bool HasSwansImmortalGrace { get; set; }      // T5 - 50% shield, +5% dodge at full
        public bool HasFatesCosmicAegis { get; set; }        // T6 - 60% shield, Last Stand invincibility
        
        // Post-Fate Theme Chain (T7-T10)
        public bool HasNocturnalGuardiansWard { get; set; }      // T7 - Nachtmusik - 70% shield, stellar constellation
        public bool HasInfernalRampartOfDiesIrae { get; set; }   // T8 - Dies Irae - 80% shield, hellfire thorns
        public bool HasJubilantBulwarkOfJoy { get; set; }        // T9 - Ode to Joy - 90% shield, healing on block
        public bool HasEternalBastionOfTheMoonlight { get; set; } // T10 - Clair de Lune - 100% shield, Temporal Stasis
        
        // Post-Fate Fusion Accessories
        public bool HasStarfallInfernalShield { get; set; }      // Fusion T1 - Nachtmusik + Dies Irae
        public bool HasTriumphantJubilantAegis { get; set; }     // Fusion T2 - + Ode to Joy
        public bool HasAegisOfTheEternalBastion { get; set; }    // Ultimate Fusion - + Clair de Lune
        
        // ========== SHIELD STATE ==========
        public float CurrentShield { get; private set; }
        public float MaxShield => GetMaxShield();
        public float ShieldPercent => GetShieldPercent();
        
        private int timeSinceHit;
        private const int REGEN_DELAY = 300; // 5 seconds (60 ticks per second)
        private const float BASE_REGEN_RATE = 0.5f; // Shield regen per tick
        
        // Effect timers
        private int breakEffectCooldown;
        private int lastStandCooldown;
        private const int LAST_STAND_COOLDOWN = 7200; // 2 minutes
        private int lastStandDuration;
        private const int LAST_STAND_DURATION = 180; // 3 seconds
        
        private int invisibilityDuration;
        private int damageBoostDuration;
        private int freezeEnemiesDuration;
        
        // Thorns tracking
        private bool thornsActive;
        
        // VFX timers
        private int shieldPulseTimer;
        private int ambientParticleTimer;
        
        // Theme colors
        private static readonly Color ShieldBlue = new Color(100, 180, 255);
        private static readonly Color SpringPink = new Color(255, 180, 200);
        private static readonly Color SolarOrange = new Color(255, 140, 50);
        private static readonly Color HarvestAmber = new Color(200, 150, 80);
        private static readonly Color FrostBlue = new Color(150, 200, 255);
        private static readonly Color SeasonalGreen = new Color(150, 220, 150);
        private static readonly Color MoonlitPurple = new Color(150, 120, 200);
        private static readonly Color HeroicScarlet = new Color(200, 80, 80);
        private static readonly Color InfernalOrange = new Color(255, 100, 30);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color SwanWhite = new Color(240, 245, 255);
        private static readonly Color FateCrimson = new Color(180, 40, 80);
        
        // Post-Fate Theme Colors
        private static readonly Color NachtmusikPurple = new Color(100, 80, 180);
        private static readonly Color NachtmusikGold = new Color(255, 215, 140);
        private static readonly Color DiesIraeCrimson = new Color(200, 50, 50);
        private static readonly Color DiesIraeOrange = new Color(255, 120, 40);
        private static readonly Color OdeToJoyWhite = new Color(255, 255, 255);
        private static readonly Color OdeToJoyIridescent = new Color(220, 200, 255);
        private static readonly Color ClairDeLuneBrass = new Color(200, 170, 100);
        private static readonly Color ClairDeLuneCrimson = new Color(180, 80, 100);
        
        public override void ResetEffects()
        {
            HasResonantBarrierCore = false;
            HasSpringVitalityShell = false;
            HasSolarFlareAegis = false;
            HasHarvestThornedGuard = false;
            HasPermafrostCrystalWard = false;
            HasVivaldisSeasonalBulwark = false;
            HasMoonlitGuardiansVeil = false;
            HasHeroicValorsAegis = false;
            HasInfernalBellsFortress = false;
            HasEnigmasVoidShell = false;
            HasSwansImmortalGrace = false;
            HasFatesCosmicAegis = false;
            
            // Post-Fate T7-T10
            HasNocturnalGuardiansWard = false;
            HasInfernalRampartOfDiesIrae = false;
            HasJubilantBulwarkOfJoy = false;
            HasEternalBastionOfTheMoonlight = false;
            
            // Post-Fate Fusions
            HasStarfallInfernalShield = false;
            HasTriumphantJubilantAegis = false;
            HasAegisOfTheEternalBastion = false;
            
            thornsActive = false;
        }
        
        /// <summary>
        /// Check if any defense accessory is equipped
        /// </summary>
        public bool HasAnyDefenseAccessory()
        {
            return HasResonantBarrierCore || HasSpringVitalityShell || HasSolarFlareAegis ||
                   HasHarvestThornedGuard || HasPermafrostCrystalWard || HasVivaldisSeasonalBulwark ||
                   HasMoonlitGuardiansVeil || HasHeroicValorsAegis || HasInfernalBellsFortress ||
                   HasEnigmasVoidShell || HasSwansImmortalGrace || HasFatesCosmicAegis ||
                   HasNocturnalGuardiansWard || HasInfernalRampartOfDiesIrae || HasJubilantBulwarkOfJoy ||
                   HasEternalBastionOfTheMoonlight || HasStarfallInfernalShield || HasTriumphantJubilantAegis ||
                   HasAegisOfTheEternalBastion;
        }
        
        /// <summary>
        /// Get the shield percentage based on equipped accessory
        /// </summary>
        private float GetShieldPercent()
        {
            // Ultimate Fusion - 120%
            if (HasAegisOfTheEternalBastion) return 1.20f;      // 120%
            
            // Fusion Tier 2 - 95%
            if (HasTriumphantJubilantAegis) return 0.95f;       // 95%
            
            // Fusion Tier 1 and T10 - 85% and 100%
            if (HasEternalBastionOfTheMoonlight) return 1.00f;  // 100%
            if (HasStarfallInfernalShield) return 0.85f;        // 85%
            
            // T9 - 90%
            if (HasJubilantBulwarkOfJoy) return 0.90f;          // 90%
            
            // T8 - 80%
            if (HasInfernalRampartOfDiesIrae) return 0.80f;     // 80%
            
            // T7 - 70%
            if (HasNocturnalGuardiansWard) return 0.70f;        // 70%
            
            // Fate tier and below (original values)
            if (HasFatesCosmicAegis) return 0.60f;          // 60%
            if (HasSwansImmortalGrace) return 0.50f;        // 50%
            if (HasEnigmasVoidShell) return 0.45f;          // 45%
            if (HasInfernalBellsFortress) return 0.40f;     // 40%
            if (HasHeroicValorsAegis) return 0.38f;         // 38%
            if (HasMoonlitGuardiansVeil) return 0.36f;      // 36%
            if (HasVivaldisSeasonalBulwark) return 0.35f;   // 35%
            if (HasPermafrostCrystalWard) return 0.30f;     // 30%
            if (HasHarvestThornedGuard) return 0.25f;       // 25%
            if (HasSolarFlareAegis) return 0.20f;           // 20%
            if (HasSpringVitalityShell) return 0.15f;       // 15%
            if (HasResonantBarrierCore) return 0.10f;       // 10%
            return 0f;
        }
        
        /// <summary>
        /// Get the maximum shield amount based on player's max HP
        /// </summary>
        private float GetMaxShield()
        {
            return Player.statLifeMax2 * ShieldPercent;
        }
        
        /// <summary>
        /// Get regen rate based on equipped accessories and time of day
        /// </summary>
        private float GetRegenRate()
        {
            float rate = BASE_REGEN_RATE;
            
            // Post-Fate Ultimate: Maximum regen boost
            if (HasAegisOfTheEternalBastion)
            {
                rate *= 2.0f;
                if (!Main.dayTime) rate *= 1.3f; // Night bonus
            }
            // Eternal Bastion (T10): Very fast regen, bonus when standing still
            else if (HasEternalBastionOfTheMoonlight)
            {
                rate *= 1.8f;
                if (Player.velocity.Length() < 0.1f) rate *= 1.5f; // Standing still bonus
            }
            // Triumphant Jubilant Aegis (Fusion 2): Fast regen with healing
            else if (HasTriumphantJubilantAegis)
            {
                rate *= 1.7f;
            }
            // Jubilant Bulwark (T9): Fast regen with celebration
            else if (HasJubilantBulwarkOfJoy)
            {
                rate *= 1.6f;
            }
            // Starfall Infernal Shield (Fusion 1): Enhanced regen
            else if (HasStarfallInfernalShield)
            {
                rate *= 1.55f;
                if (!Main.dayTime) rate *= 1.3f; // Night bonus from Nachtmusik
            }
            // Infernal Rampart (T8): Moderate-fast regen
            else if (HasInfernalRampartOfDiesIrae)
            {
                rate *= 1.5f;
            }
            // Nocturnal Guardian's Ward (T7): Boosted at night
            else if (HasNocturnalGuardiansWard)
            {
                rate *= 1.4f;
                if (!Main.dayTime) rate *= 1.5f; // Night bonus
            }
            else
            {
                // Moonlit Guardian's Veil: faster regen at night
                if (HasMoonlitGuardiansVeil && !Main.dayTime)
                {
                    rate *= 1.5f;
                }
                
                // Swan's Immortal Grace: slightly faster base regen
                if (HasSwansImmortalGrace)
                {
                    rate *= 1.2f;
                }
                
                // Fate's Cosmic Aegis: even faster regen
                if (HasFatesCosmicAegis)
                {
                    rate *= 1.3f;
                }
            }
            
            return rate;
        }
        
        public override void PreUpdate()
        {
            if (!HasAnyDefenseAccessory())
            {
                CurrentShield = 0;
                return;
            }
            
            // Increment time since last hit
            timeSinceHit++;
            
            // Decrement cooldowns
            if (breakEffectCooldown > 0) breakEffectCooldown--;
            if (lastStandCooldown > 0) lastStandCooldown--;
            if (invisibilityDuration > 0) invisibilityDuration--;
            if (damageBoostDuration > 0) damageBoostDuration--;
            if (freezeEnemiesDuration > 0) freezeEnemiesDuration--;
            if (lastStandDuration > 0) lastStandDuration--;
            
            // Update VFX timers
            shieldPulseTimer++;
            ambientParticleTimer++;
            
            // Shield regeneration (after delay)
            if (timeSinceHit >= REGEN_DELAY && CurrentShield < MaxShield)
            {
                float regenRate = GetRegenRate();
                CurrentShield = Math.Min(CurrentShield + regenRate, MaxShield);
                
                // Regen VFX
                if (ambientParticleTimer % 10 == 0 && CurrentShield < MaxShield)
                {
                    SpawnRegenParticles();
                }
            }
            
            // Initialize shield if just equipped
            if (CurrentShield <= 0 && MaxShield > 0)
            {
                CurrentShield = MaxShield;
            }
            
            // Thorns effect
            thornsActive = HasHarvestThornedGuard || HasPermafrostCrystalWard || 
                          HasVivaldisSeasonalBulwark || HasMoonlitGuardiansVeil ||
                          HasHeroicValorsAegis || HasInfernalBellsFortress ||
                          HasEnigmasVoidShell || HasSwansImmortalGrace || HasFatesCosmicAegis;
            
            // Ambient shield particles
            if (CurrentShield > 0 && ambientParticleTimer % 8 == 0)
            {
                SpawnAmbientShieldParticles();
            }
            
            // Swan's Immortal Grace: +5% dodge at full shield
            if (HasSwansImmortalGrace && CurrentShield >= MaxShield)
            {
                // Dodge chance handled in ModifyHitByNPC/Projectile
            }
            
            // Enigma's Void Shell: 10% phase through attacks
            // Handled in ModifyHitByNPC/Projectile
        }
        
        public override void PostUpdate()
        {
            // Apply damage boost from break effect
            if (damageBoostDuration > 0)
            {
                // Visual indicator
                if (ambientParticleTimer % 5 == 0)
                {
                    Vector2 dustPos = Player.Center + Main.rand.NextVector2Circular(20f, 30f);
                    Dust dust = Dust.NewDustPerfect(dustPos, DustID.Torch, Vector2.Zero);
                    dust.noGravity = true;
                    dust.scale = 0.6f;
                    dust.color = HeroicScarlet;
                }
            }
            
            // Invisibility effect
            if (invisibilityDuration > 0)
            {
                Player.invis = true;
                Player.aggro -= 1000;
            }
            
            // Last Stand invincibility
            if (lastStandDuration > 0)
            {
                Player.immune = true;
                Player.immuneTime = 2;
                
                // Golden invincibility aura
                if (ambientParticleTimer % 3 == 0)
                {
                    float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                    Vector2 dustPos = Player.Center + angle.ToRotationVector2() * 40f;
                    Dust dust = Dust.NewDustPerfect(dustPos, DustID.GoldCoin, 
                        -angle.ToRotationVector2() * 2f);
                    dust.noGravity = true;
                    dust.scale = 1.2f;
                }
            }
        }
        
        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (!HasAnyDefenseAccessory() || CurrentShield <= 0)
                return;
            
            // Enigma's Void Shell: 10% chance to phase through attacks entirely
            if (HasEnigmasVoidShell && CurrentShield > 0 && Main.rand.NextFloat() < 0.10f)
            {
                modifiers.FinalDamage *= 0f;
                
                // Phase VFX
                SoundEngine.PlaySound(SoundID.Item8, Player.Center);
                for (int i = 0; i < 15; i++)
                {
                    Vector2 dustVel = Main.rand.NextVector2Circular(4f, 4f);
                    Dust dust = Dust.NewDustPerfect(Player.Center, DustID.PurpleTorch, dustVel);
                    dust.noGravity = true;
                    dust.scale = 0.8f;
                    dust.color = EnigmaPurple;
                }
                return;
            }
            
            // Swan's Immortal Grace: +5% dodge at full shield
            if (HasSwansImmortalGrace && CurrentShield >= MaxShield && Main.rand.NextFloat() < 0.05f)
            {
                modifiers.FinalDamage *= 0f;
                
                // Graceful dodge VFX
                SoundEngine.PlaySound(SoundID.Item24, Player.Center);
                for (int i = 0; i < 12; i++)
                {
                    Vector2 dustVel = Main.rand.NextVector2Circular(3f, 3f);
                    Dust dust = Dust.NewDustPerfect(Player.Center, DustID.Cloud, dustVel);
                    dust.noGravity = true;
                    dust.scale = 0.9f;
                    dust.color = SwanWhite;
                }
                return;
            }
        }
        
        public override void OnHurt(Player.HurtInfo info)
        {
            if (!HasAnyDefenseAccessory())
                return;
            
            // Last Stand trigger check
            if (HasFatesCosmicAegis && lastStandCooldown <= 0 && Player.statLife <= Player.statLifeMax2 * 0.15f)
            {
                TriggerLastStand();
            }
            
            // Reset regen timer
            timeSinceHit = 0;
            
            // Handle shield absorption
            if (CurrentShield > 0)
            {
                float damageToShield = Math.Min(info.Damage, CurrentShield);
                float remainingShield = CurrentShield - damageToShield;
                
                // Shield hit VFX
                SpawnShieldHitParticles(info.Damage);
                SoundEngine.PlaySound(SoundID.NPCHit4 with { Pitch = 0.5f, Volume = 0.7f }, Player.Center);
                
                // Shield broke?
                if (remainingShield <= 0 && CurrentShield > 0)
                {
                    OnShieldBreak();
                }
                
                CurrentShield = Math.Max(0, remainingShield);
            }
            
            // Thorns damage
            if (thornsActive && CurrentShield > 0 && info.DamageSource.TryGetCausingEntity(out Entity attacker))
            {
                if (attacker is NPC npc && !npc.friendly && npc.CanBeChasedBy())
                {
                    int thornsDamage = (int)(info.Damage * 0.15f);
                    if (thornsDamage > 0)
                    {
                        npc.SimpleStrikeNPC(thornsDamage, 0, false, 0, null, false, 0, true);
                        
                        // Thorns VFX
                        for (int i = 0; i < 8; i++)
                        {
                            Vector2 thornVel = (npc.Center - Player.Center).SafeNormalize(Vector2.Zero) * 5f;
                            thornVel = thornVel.RotatedByRandom(0.3f);
                            Dust dust = Dust.NewDustPerfect(Player.Center, DustID.t_Cactus, thornVel);
                            dust.noGravity = true;
                            dust.scale = 1.0f;
                            dust.color = HarvestAmber;
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Called when the shield breaks - triggers break effects
        /// </summary>
        private void OnShieldBreak()
        {
            if (breakEffectCooldown > 0) return;
            breakEffectCooldown = 60; // 1 second cooldown between break effects
            
            SoundEngine.PlaySound(SoundID.Shatter, Player.Center);
            
            // Spawn break VFX
            SpawnShieldBreakParticles();
            
            // Spring Vitality Shell: Healing petals
            if (HasSpringVitalityShell || HasVivaldisSeasonalBulwark)
            {
                TriggerHealingPetals();
            }
            
            // Solar Flare Aegis: Fire nova
            if (HasSolarFlareAegis || HasVivaldisSeasonalBulwark)
            {
                TriggerFireNova();
            }
            
            // Permafrost Crystal Ward: Freeze attackers
            if (HasPermafrostCrystalWard || HasVivaldisSeasonalBulwark)
            {
                TriggerFreezeEffect();
            }
            
            // Moonlit Guardian's Veil: Brief invisibility
            if (HasMoonlitGuardiansVeil)
            {
                invisibilityDuration = 120; // 2 seconds
                SoundEngine.PlaySound(SoundID.Item8, Player.Center);
                
                for (int i = 0; i < 20; i++)
                {
                    Vector2 dustVel = Main.rand.NextVector2Circular(5f, 5f);
                    Dust dust = Dust.NewDustPerfect(Player.Center, DustID.PurpleTorch, dustVel);
                    dust.noGravity = true;
                    dust.scale = 0.8f;
                    dust.color = MoonlitPurple;
                    dust.alpha = 100;
                }
            }
            
            // Heroic Valor's Aegis: +15% damage boost
            if (HasHeroicValorsAegis)
            {
                damageBoostDuration = 300; // 5 seconds
                SoundEngine.PlaySound(SoundID.Roar, Player.Center);
                
                for (int i = 0; i < 25; i++)
                {
                    float angle = MathHelper.TwoPi * i / 25f;
                    Vector2 dustPos = Player.Center + angle.ToRotationVector2() * 40f;
                    Dust dust = Dust.NewDustPerfect(dustPos, DustID.Torch, 
                        angle.ToRotationVector2() * 3f);
                    dust.noGravity = true;
                    dust.scale = 1.2f;
                    dust.color = HeroicScarlet;
                }
            }
            
            // Infernal Bell's Fortress: Bell shockwave
            if (HasInfernalBellsFortress)
            {
                TriggerBellShockwave();
            }
        }
        
        /// <summary>
        /// Spring effect: Release healing petals to nearby allies
        /// </summary>
        private void TriggerHealingPetals()
        {
            int healAmount = 10;
            float radius = 300f;
            
            // Heal self
            Player.Heal(healAmount);
            
            // Heal nearby players in multiplayer
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player other = Main.player[i];
                if (other.active && !other.dead && other.whoAmI != Player.whoAmI)
                {
                    float distance = Vector2.Distance(Player.Center, other.Center);
                    if (distance < radius)
                    {
                        other.Heal(healAmount);
                    }
                }
            }
            
            // Healing petal VFX
            for (int i = 0; i < 15; i++)
            {
                float angle = MathHelper.TwoPi * i / 15f;
                Vector2 petalVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Dust dust = Dust.NewDustPerfect(Player.Center, DustID.PinkTorch, petalVel);
                dust.noGravity = true;
                dust.scale = 1.0f;
                dust.color = SpringPink;
            }
            
            SoundEngine.PlaySound(SoundID.Item4, Player.Center);
        }
        
        /// <summary>
        /// Summer effect: Release fire nova damaging nearby enemies
        /// </summary>
        private void TriggerFireNova()
        {
            int damage = 50;
            float radius = 200f;
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                {
                    float distance = Vector2.Distance(Player.Center, npc.Center);
                    if (distance < radius)
                    {
                        npc.SimpleStrikeNPC(damage, 0, false, 0, null, false, 0, true);
                        npc.AddBuff(BuffID.OnFire, 180);
                    }
                }
            }
            
            // Fire nova VFX
            for (int ring = 0; ring < 3; ring++)
            {
                int particleCount = 16 + ring * 8;
                float ringRadius = 30f + ring * 40f;
                
                for (int i = 0; i < particleCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / particleCount;
                    Vector2 dustPos = Player.Center + angle.ToRotationVector2() * ringRadius;
                    Vector2 dustVel = angle.ToRotationVector2() * (3f + ring);
                    Dust dust = Dust.NewDustPerfect(dustPos, DustID.Torch, dustVel);
                    dust.noGravity = true;
                    dust.scale = 1.3f - ring * 0.2f;
                    dust.color = SolarOrange;
                }
            }
            
            SoundEngine.PlaySound(SoundID.Item45, Player.Center);
        }
        
        /// <summary>
        /// Winter effect: Freeze nearby attackers
        /// </summary>
        private void TriggerFreezeEffect()
        {
            float radius = 150f;
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                {
                    float distance = Vector2.Distance(Player.Center, npc.Center);
                    if (distance < radius)
                    {
                        npc.AddBuff(BuffID.Frozen, 60); // 1 second freeze
                        npc.AddBuff(BuffID.Frostburn, 180); // 3 seconds frostburn
                    }
                }
            }
            
            // Freeze VFX
            for (int i = 0; i < 25; i++)
            {
                Vector2 dustPos = Player.Center + Main.rand.NextVector2Circular(radius * 0.8f, radius * 0.8f);
                Vector2 dustVel = Main.rand.NextVector2Circular(2f, 2f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.IceTorch, dustVel);
                dust.noGravity = true;
                dust.scale = 1.1f;
                dust.color = FrostBlue;
            }
            
            SoundEngine.PlaySound(SoundID.Item27, Player.Center);
        }
        
        /// <summary>
        /// La Campanella effect: Massive bell shockwave
        /// </summary>
        private void TriggerBellShockwave()
        {
            int damage = 100;
            float radius = 300f;
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                {
                    float distance = Vector2.Distance(Player.Center, npc.Center);
                    if (distance < radius)
                    {
                        // Damage scales with proximity
                        float distanceFactor = 1f - (distance / radius);
                        int scaledDamage = (int)(damage * distanceFactor);
                        npc.SimpleStrikeNPC(scaledDamage, 0, false, 0, null, false, 0, true);
                        npc.AddBuff(BuffID.OnFire3, 120);
                        
                        // Knockback
                        Vector2 knockback = (npc.Center - Player.Center).SafeNormalize(Vector2.Zero) * 10f;
                        npc.velocity += knockback;
                    }
                }
            }
            
            // Bell shockwave VFX - expanding rings
            for (int ring = 0; ring < 5; ring++)
            {
                int particleCount = 20 + ring * 5;
                for (int i = 0; i < particleCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / particleCount;
                    float ringRadius = 20f + ring * 50f;
                    Vector2 dustPos = Player.Center + angle.ToRotationVector2() * ringRadius;
                    Vector2 dustVel = angle.ToRotationVector2() * (6f - ring);
                    
                    Color color = Color.Lerp(InfernalOrange, Color.Black, ring / 5f);
                    Dust dust = Dust.NewDustPerfect(dustPos, DustID.Torch, dustVel);
                    dust.noGravity = true;
                    dust.scale = 1.5f - ring * 0.2f;
                    dust.color = color;
                }
            }
            
            // Play bell sound
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.2f }, Player.Center);
        }
        
        /// <summary>
        /// Fate effect: Trigger Last Stand invincibility
        /// </summary>
        private void TriggerLastStand()
        {
            lastStandDuration = LAST_STAND_DURATION;
            lastStandCooldown = LAST_STAND_COOLDOWN;
            
            SoundEngine.PlaySound(SoundID.Roar with { Volume = 1.2f }, Player.Center);
            
            // Dramatic last stand VFX
            for (int wave = 0; wave < 3; wave++)
            {
                int particleCount = 30;
                for (int i = 0; i < particleCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / particleCount;
                    float radius = 60f + wave * 30f;
                    Vector2 dustPos = Player.Center + angle.ToRotationVector2() * radius;
                    Vector2 dustVel = angle.ToRotationVector2() * (4f + wave * 2f);
                    
                    Color color = Color.Lerp(FateCrimson, Color.Gold, wave / 3f);
                    Dust dust = Dust.NewDustPerfect(dustPos, DustID.GoldCoin, dustVel);
                    dust.noGravity = true;
                    dust.scale = 1.8f - wave * 0.3f;
                    dust.color = color;
                }
            }
            
            // Star particles
            for (int i = 0; i < 20; i++)
            {
                Vector2 starPos = Player.Center + Main.rand.NextVector2Circular(50f, 50f);
                Dust star = Dust.NewDustPerfect(starPos, DustID.MagicMirror, 
                    Main.rand.NextVector2Circular(3f, 3f));
                star.noGravity = true;
                star.scale = 0.8f;
                star.color = Color.White;
            }
            
            Main.NewText("Last Stand Activated!", Color.Gold);
        }
        
        public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (damageBoostDuration > 0)
            {
                modifiers.FinalDamage *= 1.15f;
            }
        }
        
        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (damageBoostDuration > 0 && proj.owner == Player.whoAmI)
            {
                modifiers.FinalDamage *= 1.15f;
            }
        }
        
        // ========== VFX METHODS ==========
        
        private void SpawnAmbientShieldParticles()
        {
            if (!HasAnyDefenseAccessory() || CurrentShield <= 0) return;
            
            float shieldRatio = CurrentShield / MaxShield;
            Color shieldColor = GetCurrentShieldColor();
            
            // Orbiting shield particles
            float angle = shieldPulseTimer * 0.05f;
            for (int i = 0; i < 2; i++)
            {
                float particleAngle = angle + MathHelper.Pi * i;
                float radius = 30f + (float)Math.Sin(shieldPulseTimer * 0.1f) * 5f;
                Vector2 dustPos = Player.Center + particleAngle.ToRotationVector2() * radius;
                
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.MagicMirror, Vector2.Zero);
                dust.noGravity = true;
                dust.scale = 0.4f * shieldRatio;
                dust.color = shieldColor;
                dust.alpha = 100;
            }
        }
        
        private void SpawnRegenParticles()
        {
            Color shieldColor = GetCurrentShieldColor();
            
            for (int i = 0; i < 3; i++)
            {
                Vector2 dustPos = Player.Bottom + new Vector2(Main.rand.NextFloat(-20f, 20f), 0f);
                Vector2 dustVel = new Vector2(0, -Main.rand.NextFloat(1f, 2f));
                
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.MagicMirror, dustVel);
                dust.noGravity = true;
                dust.scale = 0.5f;
                dust.color = shieldColor;
                dust.alpha = 150;
            }
        }
        
        private void SpawnShieldHitParticles(int damage)
        {
            Color shieldColor = GetCurrentShieldColor();
            int particleCount = Math.Min(damage / 5, 20) + 5;
            
            for (int i = 0; i < particleCount; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(6f, 6f);
                Dust dust = Dust.NewDustPerfect(Player.Center, DustID.MagicMirror, dustVel);
                dust.noGravity = true;
                dust.scale = 0.8f;
                dust.color = shieldColor;
            }
        }
        
        private void SpawnShieldBreakParticles()
        {
            Color shieldColor = GetCurrentShieldColor();
            
            // Dramatic break burst
            for (int i = 0; i < 40; i++)
            {
                float angle = MathHelper.TwoPi * i / 40f;
                Vector2 dustVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 10f);
                
                Dust dust = Dust.NewDustPerfect(Player.Center, DustID.MagicMirror, dustVel);
                dust.noGravity = true;
                dust.scale = 1.2f;
                dust.color = shieldColor;
            }
            
            // Inner bright burst
            for (int i = 0; i < 15; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(8f, 8f);
                Dust dust = Dust.NewDustPerfect(Player.Center, DustID.MagicMirror, dustVel);
                dust.noGravity = true;
                dust.scale = 1.5f;
                dust.color = Color.White;
            }
        }
        
        private Color GetCurrentShieldColor()
        {
            if (HasFatesCosmicAegis) return FateCrimson;
            if (HasSwansImmortalGrace) return SwanWhite;
            if (HasEnigmasVoidShell) return EnigmaPurple;
            if (HasInfernalBellsFortress) return InfernalOrange;
            if (HasHeroicValorsAegis) return HeroicScarlet;
            if (HasMoonlitGuardiansVeil) return MoonlitPurple;
            if (HasVivaldisSeasonalBulwark) return SeasonalGreen;
            if (HasPermafrostCrystalWard) return FrostBlue;
            if (HasHarvestThornedGuard) return HarvestAmber;
            if (HasSolarFlareAegis) return SolarOrange;
            if (HasSpringVitalityShell) return SpringPink;
            return ShieldBlue;
        }
    }
}
