using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Prefixes;

namespace MagnumOpus.Content.Common.Accessories.MageChain
{
    /// <summary>
    /// Simplified ModPlayer for mage accessories.
    /// No more Overflow system - just tracks which accessories are equipped
    /// and applies their simple, static effects.
    /// </summary>
    public class OverflowPlayer : ModPlayer
    {
        // ===== TIER 1-6 (SEASONAL + VIVALDI) FLAGS =====
        public bool hasResonantOverflowGem;      // +5% magic damage, +20 max mana
        public bool hasSpringArcaneConduit;      // +10% magic damage, healing petals
        public bool hasSolarManaCrucible;        // Magic attacks inflict On Fire!
        public bool hasHarvestSoulVessel;        // Kills restore 15 mana
        public bool hasPermafrostVoidHeart;      // +15% magic damage, +50 max mana
        public bool hasVivaldisHarmonicCore;     // +20% magic damage, biome debuffs

        // ===== T3-T4 RESONANCE SYNERGY FLAGS =====
        public bool hasSearedManaConduit;        // T3: -3% mana cost per burn stack, free spell at max
        public bool hasArcaneResonanceCatalyst;  // T4: +6% magic damage per stack, shockwave at max

        // ===== TIER 5 (THEME VARIANTS) FLAGS =====
        public bool hasMoonlitOverflowStar;      // At <50 mana: next spell costs 0
        public bool hasHeroicArcaneSurge;        // Taking damage grants brief invincibility (30s CD)
        public bool hasInfernalManaInferno;      // Magic attacks leave fire trails
        public bool hasEnigmasNegativeSpace;     // Spells hit twice (50% second hit)
        public bool hasSwansBalancedFlow;        // Killing enemies grants +20% magic damage for 5s
        public bool hasFatesCosmicReservoir;     // Spells ignore 25% enemy defense

        // ===== T7-T10 (POST-FATE) FLAGS =====
        public bool hasNocturnalHarmonicOverflow;   // +20% magic damage at night
        public bool hasInfernalManaCataclysm;       // +25% magic damage during boss fights
        public bool hasJubilantArcaneCelebration;   // Casting spells heals 1 HP per 20 mana spent
        public bool hasEternalOverflowMastery;      // Magic attacks hit twice

        // ===== FUSION FLAGS =====
        public bool hasStarfallHarmonicPendant;      // Nachtmusik + Dies Irae fusion
        public bool hasTriumphantOverflowPendant;    // 3-theme fusion
        public bool hasPendantOfTheEternalOverflow;  // Ultimate: triple hit, +40% damage

        // ===== COOLDOWNS & STATE =====
        public int invincibilityCooldown;  // Heroic Arcane Surge cooldown
        public int graceBuffTimer;         // Swan's Grace buff timer
        public bool freeSpellReady;        // Moonlit Overflow Star state

        // ===== RESONANCE SYNERGY STATE =====
        public bool resonanceFreeSpellReady;     // T3: Free spell ready from max burn stacks
        public bool resonanceShockwaveReady;     // T4: Shockwave ready from max burn stacks

        // ===== LEGACY COMPATIBILITY STUBS =====
        // These properties exist for backwards compatibility with old code
        public int currentOverflow => 0;
        public int maxOverflow => 0;
        public bool isInOverflow => false;
        public bool zeroManaFreeSpell => freeSpellReady;
        public bool justRecoveredFromOverflow => false;

        public override void Initialize()
        {
            ResonantBurnNPC.OnMaxStacksReached += OnMaxBurnStacksReached;
        }

        public override void Unload()
        {
            ResonantBurnNPC.OnMaxStacksReached -= OnMaxBurnStacksReached;
        }

        /// <summary>
        /// Handles max burn stack triggers for Resonance Synergy accessories.
        /// </summary>
        private void OnMaxBurnStacksReached(NPC npc, Player triggeringPlayer)
        {
            // Only respond to our own burn applications
            if (triggeringPlayer?.whoAmI != Player.whoAmI)
                return;

            // T3 SearedManaConduit: Enable free spell
            if (hasSearedManaConduit)
            {
                resonanceFreeSpellReady = true;
            }

            // T4 ArcaneResonanceCatalyst: Enable shockwave on next hit
            if (hasArcaneResonanceCatalyst)
            {
                resonanceShockwaveReady = true;
            }
        }

        public override void ResetEffects()
        {
            // Reset all accessory flags each frame
            hasResonantOverflowGem = false;
            hasSpringArcaneConduit = false;
            hasSolarManaCrucible = false;
            hasHarvestSoulVessel = false;
            hasPermafrostVoidHeart = false;
            hasVivaldisHarmonicCore = false;
            hasSearedManaConduit = false;
            hasArcaneResonanceCatalyst = false;
            hasMoonlitOverflowStar = false;
            hasHeroicArcaneSurge = false;
            hasInfernalManaInferno = false;
            hasEnigmasNegativeSpace = false;
            hasSwansBalancedFlow = false;
            hasFatesCosmicReservoir = false;
            hasNocturnalHarmonicOverflow = false;
            hasInfernalManaCataclysm = false;
            hasJubilantArcaneCelebration = false;
            hasEternalOverflowMastery = false;
            hasStarfallHarmonicPendant = false;
            hasTriumphantOverflowPendant = false;
            hasPendantOfTheEternalOverflow = false;
        }

        public override void PostUpdateEquips()
        {
            // === CHAIN INHERITANCE ===
            // Higher-tier accessories inherit all lower-tier effects.
            if (hasVivaldisHarmonicCore) hasPermafrostVoidHeart = true;
            if (hasPermafrostVoidHeart) hasArcaneResonanceCatalyst = true;
            if (hasArcaneResonanceCatalyst) hasSearedManaConduit = true;
            if (hasSearedManaConduit) hasSpringArcaneConduit = true;
            if (hasSpringArcaneConduit) hasResonantOverflowGem = true;

            // Apply simple static effects from equipped accessories

            // === BASE STATS: Priority system (highest main-chain tier only) ===
            // Prevents stat stacking from cascade inheritance.
            {
                float magicDmg = 0f;
                int maxMana = 0;
                if (hasVivaldisHarmonicCore) { magicDmg = 0.20f; maxMana = 50; }
                else if (hasPermafrostVoidHeart) { magicDmg = 0.15f; maxMana = 50; }
                else if (hasSpringArcaneConduit) { magicDmg = 0.10f; maxMana = 20; }
                else if (hasResonantOverflowGem) { magicDmg = 0.05f; maxMana = 20; }
                Player.GetDamage(DamageClass.Magic) += magicDmg;
                Player.statManaMax2 += maxMana;
            }

            // ===== RESONANCE SYNERGY: T3 SearedManaConduit =====
            // -3% mana cost per burn stack on any enemy (max -15% at 5 stacks)
            if (hasSearedManaConduit)
            {
                int highestStacks = GetHighestBurnStacks();
                if (highestStacks > 0)
                {
                    float manaCostReduction = 0.03f * highestStacks; // 3% per stack
                    Player.manaCost -= manaCostReduction;
                }
            }

            // ===== RESONANCE SYNERGY: T4 ArcaneResonanceCatalyst =====
            // +6% magic damage per burn stack on any enemy (max +30% at 5 stacks)
            if (hasArcaneResonanceCatalyst)
            {
                int highestStacks = GetHighestBurnStacks();
                if (highestStacks > 0)
                {
                    float damageBonus = 0.06f * highestStacks; // 6% per stack
                    Player.GetDamage(DamageClass.Magic) += damageBonus;
                }
            }

            // (Permafrost Void Heart and Vivaldi's Harmonic Core base stats handled by priority system above)

            // Nocturnal Harmonic Overflow: +20% magic damage at night
            if (hasNocturnalHarmonicOverflow && !Main.dayTime)
            {
                Player.GetDamage(DamageClass.Magic) += 0.20f;
            }

            // Infernal Mana Cataclysm: +25% magic damage during boss fights
            if (hasInfernalManaCataclysm && AnyBossAlive())
            {
                Player.GetDamage(DamageClass.Magic) += 0.25f;
            }

            // Pendant of the Eternal Overflow: +30% magic damage
            if (hasPendantOfTheEternalOverflow)
            {
                Player.GetDamage(DamageClass.Magic) += 0.30f;
            }

            // Moonlit Overflow Star: At low mana, enable free spell
            if (hasMoonlitOverflowStar && Player.statMana < 50)
            {
                freeSpellReady = true;
            }
            else if (Player.statMana >= 50)
            {
                freeSpellReady = false;
            }

            // Swan's Balanced Flow: Grace buff timer
            if (graceBuffTimer > 0)
            {
                graceBuffTimer--;
                Player.GetDamage(DamageClass.Magic) += 0.20f;
            }

            // Cooldown management
            if (invincibilityCooldown > 0)
                invincibilityCooldown--;
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            // Heroic Arcane Surge: Taking damage grants brief invincibility
            if (hasHeroicArcaneSurge && invincibilityCooldown <= 0 && info.Dodgeable)
            {
                Player.immune = true;
                Player.immuneTime = 60; // 1 second
                invincibilityCooldown = 1800; // 30 second cooldown
            }
        }

        public override void ModifyManaCost(Item item, ref float reduce, ref float mult)
        {
            // Moonlit Overflow Star: Free spell when mana < 50
            if (freeSpellReady && item.DamageType.Equals(DamageClass.Magic))
            {
                mult = 0f;
                freeSpellReady = false;
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Only apply mage effects if this is a magic item
            if (!item.DamageType.Equals(DamageClass.Magic))
                return;

            // Solar Mana Crucible: Magic attacks inflict On Fire!
            if (hasSolarManaCrucible)
            {
                target.AddBuff(BuffID.OnFire, 300);
            }

            // Harvest Soul Vessel: Kills restore 15 mana
            if (hasHarvestSoulVessel && target.life <= 0)
            {
                Player.statMana += 15;
                if (Player.statMana > Player.statManaMax2)
                    Player.statMana = Player.statManaMax2;
            }

            // Vivaldi's Harmonic Core: Biome-based debuffs
            if (hasVivaldisHarmonicCore)
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

            // Infernal Mana Inferno: Magic attacks leave fire trails
            if (hasInfernalManaInferno)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 offset = Vector2.One.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(5f, 15f);
                    Dust dust = Dust.NewDustDirect(target.Center + offset, 1, 1, DustID.Torch);
                    dust.velocity = Vector2.One.RotatedByRandom(MathHelper.TwoPi) * 1.5f;
                }
            }

            // Swan's Balanced Flow: Killing enemies grants +20% damage buff for 5s
            if (hasSwansBalancedFlow && target.life <= 0)
            {
                graceBuffTimer = 300; // 5 seconds
            }

            // Fate's Cosmic Reservoir: Spells ignore 25% enemy defense (via ModifyHitNPC)
            // Handled in ModifyHitNPC method

            // Spring Arcane Conduit: Healing petals (simplified - small chance to heal)
            if (hasSpringArcaneConduit && Main.rand.NextFloat() < 0.05f)
            {
                Player.Heal(1);
            }

            // Jubilant Arcane Celebration: Magic attacks heal 2 HP on hit
            if (hasJubilantArcaneCelebration)
            {
                Player.Heal(2);
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Only apply mage effects for magic projectiles
            if (!proj.DamageType.Equals(DamageClass.Magic) || proj.owner != Player.whoAmI)
                return;

            // Solar Mana Crucible: Magic attacks inflict On Fire!
            if (hasSolarManaCrucible)
            {
                target.AddBuff(BuffID.OnFire, 300);
            }

            // Harvest Soul Vessel: Kills restore 15 mana
            if (hasHarvestSoulVessel && target.life <= 0)
            {
                Player.statMana += 15;
                if (Player.statMana > Player.statManaMax2)
                    Player.statMana = Player.statManaMax2;
            }

            // Vivaldi's Harmonic Core: Biome-based debuffs
            if (hasVivaldisHarmonicCore)
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

            // Infernal Mana Inferno: Magic attacks leave fire trails
            if (hasInfernalManaInferno)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 offset = Vector2.One.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(5f, 15f);
                    Dust dust = Dust.NewDustDirect(target.Center + offset, 1, 1, DustID.Torch);
                    dust.velocity = Vector2.One.RotatedByRandom(MathHelper.TwoPi) * 1.5f;
                }
            }

            // Swan's Balanced Flow: Killing enemies grants +20% damage buff for 5s
            if (hasSwansBalancedFlow && target.life <= 0)
            {
                graceBuffTimer = 300; // 5 seconds
            }

            // Spring Arcane Conduit: Healing petals (simplified)
            if (hasSpringArcaneConduit && Main.rand.NextFloat() < 0.05f)
            {
                Player.Heal(1);
            }

            // Jubilant Arcane Celebration: Magic attacks heal 2 HP on hit
            if (hasJubilantArcaneCelebration)
            {
                Player.Heal(2);
            }

            // ===== RESONANCE SYNERGY: T3 SearedManaConduit =====
            // At max stacks: Next spell refunds its mana cost
            if (hasSearedManaConduit && resonanceFreeSpellReady && ResonancePrefixHelper.IsEnemyBurning(target))
            {
                // Refund mana cost (estimate based on weapon)
                int manaRefund = Player.HeldItem.mana > 0 ? Player.HeldItem.mana : 20;
                Player.statMana = Math.Min(Player.statMana + manaRefund, Player.statManaMax2);
                resonanceFreeSpellReady = false;

                // Visual feedback: mana surge effect
                for (int i = 0; i < 8; i++)
                {
                    Dust dust = Dust.NewDustDirect(Player.Center, 1, 1, DustID.MagicMirror);
                    dust.velocity = Vector2.One.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(2f, 5f);
                    dust.scale = Main.rand.NextFloat(1.2f, 1.8f);
                    dust.noGravity = true;
                }
            }

            // ===== RESONANCE SYNERGY: T4 ArcaneResonanceCatalyst =====
            // At max stacks: Release arcane shockwave that damages all nearby enemies
            if (hasArcaneResonanceCatalyst && resonanceShockwaveReady && ResonancePrefixHelper.IsEnemyBurning(target))
            {
                SpawnArcaneShockwave(target.Center, damageDone);
                resonanceShockwaveReady = false;
            }
        }

        public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!item.DamageType.Equals(DamageClass.Magic))
                return;

            // Fate's Cosmic Reservoir: Spells ignore 25% enemy defense
            if (hasFatesCosmicReservoir)
            {
                modifiers.Defense -= target.defense / 4; // Ignore 25% defense
            }
        }

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!proj.DamageType.Equals(DamageClass.Magic) || proj.owner != Player.whoAmI)
                return;

            // Fate's Cosmic Reservoir: Spells ignore 25% enemy defense
            if (hasFatesCosmicReservoir)
            {
                modifiers.Defense -= target.defense / 4; // Ignore 25% defense
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
        /// Gets the number of times magic attacks should hit.
        /// </summary>
        public int GetHitMultiplier()
        {
            if (hasPendantOfTheEternalOverflow)
                return 3; // Triple hit

            if (hasEternalOverflowMastery || hasTriumphantOverflowPendant || hasEnigmasNegativeSpace)
                return 2; // Double hit

            return 1;
        }

        /// <summary>
        /// Gets the highest burn stack count from any active NPC.
        /// Used for Resonance Synergy accessories.
        /// </summary>
        private int GetHighestBurnStacks()
        {
            int highest = 0;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly)
                {
                    int stacks = ResonancePrefixHelper.GetBurnStacks(npc);
                    if (stacks > highest)
                        highest = stacks;
                }
            }
            return highest;
        }

        /// <summary>
        /// Spawns an arcane shockwave effect for T4 ArcaneResonanceCatalyst.
        /// Deals 50% of trigger damage to all enemies in range.
        /// </summary>
        private void SpawnArcaneShockwave(Vector2 center, int baseDamage)
        {
            float shockwaveRadius = 200f;
            int shockwaveDamage = (int)(baseDamage * 0.5f);

            // Visual: expanding ring of arcane particles
            for (int i = 0; i < 24; i++)
            {
                float angle = MathHelper.TwoPi * i / 24f;
                Vector2 offset = Vector2.UnitX.RotatedBy(angle) * shockwaveRadius * 0.5f;
                Dust dust = Dust.NewDustDirect(center + offset, 1, 1, DustID.PurpleTorch);
                dust.velocity = Vector2.UnitX.RotatedBy(angle) * 4f;
                dust.scale = Main.rand.NextFloat(1.5f, 2.5f);
                dust.noGravity = true;
            }

            // Damage all enemies in range
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && !npc.dontTakeDamage)
                {
                    float distance = Vector2.Distance(center, npc.Center);
                    if (distance < shockwaveRadius)
                    {
                        // Apply damage
                        Player.ApplyDamageToNPC(npc, shockwaveDamage, 0f, 0, false);

                        // Apply Resonant Burn if not already burning
                        if (!ResonancePrefixHelper.IsEnemyBurning(npc))
                        {
                            ResonancePrefixHelper.ApplyBurnDebuff(npc, 180, Player);
                        }
                    }
                }
            }
        }
    }
}
