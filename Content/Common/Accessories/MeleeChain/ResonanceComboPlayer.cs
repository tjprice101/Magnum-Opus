using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Prefixes;

namespace MagnumOpus.Content.Common.Accessories.MeleeChain
{
    /// <summary>
    /// Simplified ModPlayer for melee accessories.
    /// No more stacking system - just tracks which accessories are equipped
    /// and applies their simple, static effects.
    /// </summary>
    public class ResonanceComboPlayer : ModPlayer
    {
        // ===== TIER 1-4 (SEASONAL) FLAGS =====
        public bool hasResonantRhythmBand;      // +5% damage, +3% speed
        public bool hasSpringTempoCharm;        // +10% speed, chance for healing petal
        public bool hasResonantCleaversEdge;    // Resonance Sliced synergy: +50% burn DoT, 2% heal vs burning
        public bool hasInfernoTempoSignet;      // +4% speed per burning enemy, extend burn on hit
        public bool hasPermafrostCadenceSeal;   // 10% freeze chance
        public bool hasVivaldisTempoMaster;     // +12% damage, biome debuffs

        // ===== TIER 3-4 ACCESSORY FLAGS (for GlobalNPC effects) =====
        public bool hasSolarCrescendoRing;      // Inflicts Scorched stacks
        public bool hasHarvestRhythmSignet;     // 2% lifesteal

        // ===== TIER 5 (THEME VARIANTS) FLAGS =====
        public bool hasMoonlitSonataBand;       // Crits spawn moonbeams
        public bool hasHeroicCrescendo;         // +15% damage, +10% crit
        public bool hasInfernalFortissimo;      // Kills cause explosions
        public bool hasEnigmasDissonance;       // Delayed burst damage
        public bool hasSwansPerfectMeasure;     // Perfect dodge invuln
        public bool hasFatesCosmicSymphony;     // Day/night scaling damage

        // ===== T7-T10 (POST-FATE) FLAGS =====
        public bool hasNocturnalSymphonyBand;   // +20% night damage, constellation trails
        public bool hasInfernalFortissimoBandT8;// Judgment Burn, +25% boss damage
        public bool hasJubilantCrescendoBand;   // 3% lifesteal, kill healing
        public bool hasEternalResonanceBand;    // Double hit (50% second)

        // ===== FUSION FLAGS =====
        public bool hasStarfallJudgmentGauntlet;      // Nachtmusik + Dies Irae
        public bool hasTriumphantCosmosGauntlet;      // 3-theme fusion
        public bool hasGauntletOfTheEternalSymphony;  // Ultimate: triple hit, +40% damage

        // ===== RESONANCE SYNERGY STATE (from Resonant Burn stacks) =====
        public bool resonanceSynergyBonusDamageReady;   // T3: Ready for +200% hit
        public int resonanceSynergySpeedBoostTimer;     // T4: 2-second speed boost timer

        // ===== COOLDOWNS =====
        public int perfectDodgeCooldown;        // Swan's Perfect Measure cooldown

        // ===== LEGACY COMPATIBILITY STUBS =====
        // These properties exist for backwards compatibility with old UI/tooltip code
        // The stacking system has been simplified, so these return constant values
        public int resonanceStacks => 0;
        public int maxResonance => 100;
        public bool IsBurstOnCooldown => true; // Bursts removed, always "on cooldown"
        public int BurstCooldown => 0;
        public bool IsGraceful => false;
        public float GetResonancePercent() => 0f;
        public Color GetResonanceColor() => GetThemeColor();
        // ===== COLORS =====
        private static readonly Color MoonlightPurple = new Color(138, 43, 226);
        private static readonly Color EroicaGold = new Color(255, 200, 80);
        private static readonly Color CampanellaOrange = new Color(255, 140, 40);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color SwanWhite = new Color(255, 255, 255);
        private static readonly Color FateCrimson = new Color(200, 80, 120);
        private static readonly Color NachtmusikGold = new Color(255, 215, 0);
        private static readonly Color DiesIraeCrimson = new Color(180, 40, 40);
        private static readonly Color OdeToJoyRose = new Color(255, 200, 220);

        public override void Initialize()
        {
            // Subscribe to the max burn stacks event for Resonance Synergy
            ResonantBurnNPC.OnMaxStacksReached += OnMaxBurnStacksReached;
        }

        public override void Unload()
        {
            ResonantBurnNPC.OnMaxStacksReached -= OnMaxBurnStacksReached;
        }

        /// <summary>
        /// Called when Resonant Burn reaches 5 stacks on an enemy.
        /// Triggers max-stack bonuses for T3-T4 accessories.
        /// </summary>
        private void OnMaxBurnStacksReached(NPC npc, Player triggerPlayer)
        {
            if (triggerPlayer.whoAmI != Player.whoAmI)
                return;

            // T3 ResonantCleaversEdge: Ready +200% hit, consumes stacks
            if (hasResonantCleaversEdge)
            {
                resonanceSynergyBonusDamageReady = true;

                // Visual cue
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f;
                    Vector2 offset = angle.ToRotationVector2() * 20f;
                    CustomParticles.GenericFlare(Player.Center + offset, CampanellaOrange, 0.4f, 15);
                }
            }

            // T4 InfernoTempoSignet: +30% attack speed for 2 seconds
            if (hasInfernoTempoSignet)
            {
                resonanceSynergySpeedBoostTimer = 120; // 2 seconds

                // Visual cue
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    float hue = (float)i / 8f;
                    Color rainbowColor = Main.hslToRgb(hue, 0.9f, 0.7f);
                    Vector2 velocity = angle.ToRotationVector2() * 4f;
                    CustomParticles.GenericGlow(Player.Center, velocity, rainbowColor * 0.6f, 0.3f, 20, true);
                }

                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item4, Player.Center);
            }
        }

        public override void ResetEffects()
        {
            // Reset all accessory flags each frame
            hasResonantRhythmBand = false;
            hasSpringTempoCharm = false;
            hasResonantCleaversEdge = false;
            hasInfernoTempoSignet = false;
            hasPermafrostCadenceSeal = false;
            hasVivaldisTempoMaster = false;
            hasSolarCrescendoRing = false;
            hasHarvestRhythmSignet = false;
            hasMoonlitSonataBand = false;
            hasHeroicCrescendo = false;
            hasInfernalFortissimo = false;
            hasEnigmasDissonance = false;
            hasSwansPerfectMeasure = false;
            hasFatesCosmicSymphony = false;
            hasNocturnalSymphonyBand = false;
            hasInfernalFortissimoBandT8 = false;
            hasJubilantCrescendoBand = false;
            hasEternalResonanceBand = false;
            hasStarfallJudgmentGauntlet = false;
            hasTriumphantCosmosGauntlet = false;
            hasGauntletOfTheEternalSymphony = false;
        }

        public override void PostUpdateEquips()
        {
            // Apply simple static effects from equipped accessories

            // Resonant Rhythm Band: +5% damage, +3% speed
            if (hasResonantRhythmBand)
            {
                Player.GetDamage(DamageClass.Melee) += 0.05f;
                Player.GetAttackSpeed(DamageClass.Melee) += 0.03f;
            }

            // Spring Tempo Charm: +10% speed (healing handled in OnHitNPC)
            if (hasSpringTempoCharm)
            {
                Player.GetAttackSpeed(DamageClass.Melee) += 0.10f;
            }

            // ===== RESONANCE SYNERGY BONUSES (T3-T4) =====

            // Inferno Tempo Signet: +4% speed per burning enemy + stack bonus
            if (hasInfernoTempoSignet)
            {
                int burningCount = ResonancePrefixHelper.CountBurningEnemies();
                float speedBonus = Math.Min(burningCount * 0.04f, 0.20f); // Cap at 20% (5 enemies)
                Player.GetAttackSpeed(DamageClass.Melee) += speedBonus;

                // Additional +2% per burn stack on any enemy
                int maxStacks = GetHighestBurnStacks();
                Player.GetAttackSpeed(DamageClass.Melee) += maxStacks * 0.02f;
            }

            // Resonance Synergy speed boost (from max stacks trigger)
            if (resonanceSynergySpeedBoostTimer > 0)
            {
                resonanceSynergySpeedBoostTimer--;
                Player.GetAttackSpeed(DamageClass.Melee) += 0.30f; // +30% for 2 seconds
            }

            // Vivaldi's Tempo Master: +12% damage
            if (hasVivaldisTempoMaster)
            {
                Player.GetDamage(DamageClass.Melee) += 0.12f;
            }

            // Heroic Crescendo: +15% damage, +10% crit
            if (hasHeroicCrescendo)
            {
                Player.GetDamage(DamageClass.Melee) += 0.15f;
                Player.GetCritChance(DamageClass.Melee) += 10;
            }

            // Nocturnal Symphony Band: +20% damage at night
            if (hasNocturnalSymphonyBand && !Main.dayTime)
            {
                Player.GetDamage(DamageClass.Melee) += 0.20f;
            }

            // Infernal Fortissimo T8: +25% damage during boss fights
            if (hasInfernalFortissimoBandT8 && AnyBossAlive())
            {
                Player.GetDamage(DamageClass.Melee) += 0.25f;
            }

            // Gauntlet of the Eternal Symphony: +40% damage
            if (hasGauntletOfTheEternalSymphony)
            {
                Player.GetDamage(DamageClass.Melee) += 0.40f;
            }

            // Cooldown management
            if (perfectDodgeCooldown > 0)
                perfectDodgeCooldown--;
        }

        /// <summary>
        /// Gets the highest burn stack count on any nearby enemy.
        /// </summary>
        private int GetHighestBurnStacks()
        {
            int maxStacks = 0;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && Vector2.Distance(Player.Center, npc.Center) < 2000f)
                {
                    int stacks = ResonancePrefixHelper.GetBurnStacks(npc);
                    if (stacks > maxStacks)
                        maxStacks = stacks;
                }
            }
            return maxStacks;
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            // Swan's Perfect Measure: Perfect dodge grants invulnerability
            // "Perfect dodge" = dodge within very short window, handled via accessory effect
            // For simplicity: when hit while wearing this and cooldown is 0, trigger invuln
            if (hasSwansPerfectMeasure && perfectDodgeCooldown <= 0 && info.Dodgeable)
            {
                // Grant 2 seconds of invulnerability
                Player.immune = true;
                Player.immuneTime = 120;
                perfectDodgeCooldown = 1800; // 30 second cooldown

                // VFX
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi * i / 12f;
                    Vector2 offset = angle.ToRotationVector2() * 30f;
                    CustomParticles.GenericFlare(Player.Center + offset, SwanWhite, 0.5f, 25);
                }
            }
        }

        public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers)
        {
            // ===== RESONANCE SYNERGY: T3 ResonantCleaversEdge =====
            // +15% damage vs enemies with 3+ burn stacks
            if (hasResonantCleaversEdge)
            {
                int targetStacks = ResonancePrefixHelper.GetBurnStacks(target);
                if (targetStacks >= 3)
                {
                    modifiers.FinalDamage += 0.15f;
                }

                // +200% damage on charged hit (max stacks triggered)
                if (resonanceSynergyBonusDamageReady)
                {
                    modifiers.FinalDamage *= 2f; // 200% total
                }
            }

            // Lifesteal: convert a percentage of damage to healing
            float lifestealPercent = GetLifestealPercent();
            if (lifestealPercent > 0f)
            {
                int healAmount = (int)(modifiers.FinalDamage.Multiplicative * lifestealPercent);
                if (healAmount > 0)
                {
                    Player.Heal(healAmount);
                }
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // ===== RESONANCE SYNERGY EFFECTS =====

            // T3 ResonantCleaversEdge: Consume bonus damage charge + heal 2% vs burning
            if (hasResonantCleaversEdge)
            {
                // Consume the charged hit bonus
                if (resonanceSynergyBonusDamageReady)
                {
                    resonanceSynergyBonusDamageReady = false;
                    ResonancePrefixHelper.ConsumeBurnStacks(target);

                    // Big impact VFX
                    for (int i = 0; i < 12; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 12f;
                        float hue = (float)i / 12f;
                        Color rainbowColor = Main.hslToRgb(hue, 0.9f, 0.75f);
                        Vector2 velocity = angle.ToRotationVector2() * 6f;
                        CustomParticles.GenericFlare(target.Center, rainbowColor, 0.6f, 25);
                    }

                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item14, target.Center);
                }

                // 2% lifesteal vs burning enemies
                if (ResonancePrefixHelper.IsEnemyBurning(target))
                {
                    int healAmount = Math.Max(1, (int)(damageDone * 0.02f));
                    Player.Heal(healAmount);
                }
            }

            // T4 InfernoTempoSignet: Extend burn duration by 2 seconds
            if (hasInfernoTempoSignet && ResonancePrefixHelper.IsEnemyBurning(target))
            {
                ResonancePrefixHelper.ExtendBurnDuration(target, 120); // 2 seconds
            }

            // Spring Tempo Charm: 5% chance to heal 1 HP on melee hit
            if (hasSpringTempoCharm && Main.rand.NextFloat() < 0.05f)
            {
                Player.Heal(1);
                // Small flower petal particle
                int dustType = DustID.Grass;
                Dust dust = Dust.NewDustDirect(target.Center, target.width, target.height, dustType);
                dust.velocity = Vector2.One.RotatedByRandom(MathHelper.TwoPi) * 2f;
            }

            // Solar Crescendo Ring: Inflict On Fire! for 5 seconds
            if (hasSolarCrescendoRing)
            {
                target.AddBuff(BuffID.OnFire, 300);
            }

            // Harvest Rhythm Signet: Lifesteal is handled in ModifyHitNPCWithItem
            // (already implemented above via GetLifestealPercent)

            // Permafrost Cadence Seal: 10% chance to freeze for 1 second
            if (hasPermafrostCadenceSeal && Main.rand.NextFloat() < 0.10f)
            {
                target.AddBuff(BuffID.Frostburn, 60);
            }

            // Vivaldi's Tempo Master: Biome-based debuffs
            if (hasVivaldisTempoMaster)
            {
                if (Player.ZoneSnow)
                {
                    target.AddBuff(BuffID.Frostburn, 300);
                }
                else if (Player.ZoneDesert)
                {
                    target.AddBuff(BuffID.OnFire, 300);
                }
                else if (Player.ZoneJungle)
                {
                    target.AddBuff(BuffID.Poisoned, 300);
                }
                else
                {
                    target.AddBuff(BuffID.Confused, 240);
                }
            }

            // Moonlit Sonata Band: Crits spawn moonbeams (special effect - minimal implementation)
            if (hasMoonlitSonataBand && hit.Crit)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 velocity = Vector2.One.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(2f, 4f);
                    CustomParticles.GenericFlare(target.Center, MoonlightPurple, 0.6f, 20);
                }
            }

            // Heroic Crescendo: Extra bonuses on hit (damage already applied)
            // No special on-hit effect needed

            // Infernal Fortissimo: Kills cause explosions
            if (hasInfernalFortissimo && target.life <= 0)
            {
                // Small explosion effect
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 velocity = angle.ToRotationVector2() * 4f;
                    int dustType = DustID.Torch;
                    Dust dust = Dust.NewDustDirect(target.Center, 1, 1, dustType);
                    dust.velocity = velocity;
                }
            }

            // Enigma's Dissonance: Delayed burst damage (more complex, simplified here)
            if (hasEnigmasDissonance)
            {
                // Would require projectile spawning - mark for later implementation
            }

            // Swan's Perfect Measure: Damage reduction on hit (no on-hit effect)
            // Handled via dodge chance in OnHurt

            // Fate's Cosmic Symphony: Day/night scaling damage (already applied in stats)
            // No additional on-hit effect needed
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply same effects for projectile hits where applicable
            if (proj.owner != Player.whoAmI)
                return;

            // Lifesteal for melee projectiles
            float lifestealPercent = GetLifestealPercent();
            if (lifestealPercent > 0f && proj.DamageType.Equals(DamageClass.Melee))
            {
                int healAmount = (int)(hit.Damage * lifestealPercent);
                if (healAmount > 0)
                {
                    Player.Heal(healAmount);
                }
            }

            // Apply same debuffs as melee
            if (hasSolarCrescendoRing && proj.DamageType.Equals(DamageClass.Melee))
            {
                target.AddBuff(BuffID.OnFire, 300);
            }

            if (hasPermafrostCadenceSeal && Main.rand.NextFloat() < 0.10f && proj.DamageType.Equals(DamageClass.Melee))
            {
                target.AddBuff(BuffID.Frostburn, 60);
            }

            if (hasVivaldisTempoMaster && proj.DamageType.Equals(DamageClass.Melee))
            {
                if (Player.ZoneSnow)
                {
                    target.AddBuff(BuffID.Frostburn, 300);
                }
                else if (Player.ZoneDesert)
                {
                    target.AddBuff(BuffID.OnFire, 300);
                }
                else if (Player.ZoneJungle)
                {
                    target.AddBuff(BuffID.Poisoned, 300);
                }
                else
                {
                    target.AddBuff(BuffID.Confused, 240);
                }
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

        /// <summary>
        /// Gets lifesteal percentage based on equipped accessories.
        /// </summary>
        public float GetLifestealPercent()
        {
            float lifesteal = 0f;

            if (hasHarvestRhythmSignet)
                lifesteal += 0.02f;

            if (hasJubilantCrescendoBand)
                lifesteal += 0.03f;

            if (hasTriumphantCosmosGauntlet)
                lifesteal += 0.03f;

            if (hasGauntletOfTheEternalSymphony)
                lifesteal += 0.04f;

            return lifesteal;
        }

        /// <summary>
        /// Gets the number of times melee attacks should hit.
        /// </summary>
        public int GetHitMultiplier()
        {
            if (hasGauntletOfTheEternalSymphony)
                return 3; // Triple hit

            if (hasEternalResonanceBand || hasTriumphantCosmosGauntlet)
                return 2; // Double hit

            return 1;
        }

        /// <summary>
        /// Gets the color associated with the highest tier accessory equipped.
        /// Used for VFX theming.
        /// </summary>
        public Color GetThemeColor()
        {
            if (hasGauntletOfTheEternalSymphony) return Color.Lerp(OdeToJoyRose, NachtmusikGold, 0.5f);
            if (hasTriumphantCosmosGauntlet) return OdeToJoyRose;
            if (hasStarfallJudgmentGauntlet) return Color.Lerp(NachtmusikGold, DiesIraeCrimson, 0.5f);
            if (hasEternalResonanceBand) return new Color(200, 170, 100);
            if (hasJubilantCrescendoBand) return OdeToJoyRose;
            if (hasInfernalFortissimoBandT8) return DiesIraeCrimson;
            if (hasNocturnalSymphonyBand) return NachtmusikGold;
            if (hasFatesCosmicSymphony) return FateCrimson;
            if (hasSwansPerfectMeasure) return SwanWhite;
            if (hasEnigmasDissonance) return EnigmaPurple;
            if (hasInfernalFortissimo) return CampanellaOrange;
            if (hasHeroicCrescendo) return EroicaGold;
            if (hasMoonlitSonataBand) return MoonlightPurple;
            return new Color(180, 130, 255); // Default purple
        }
    }
}
