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
        public int magicCastCounter;       // Tracks cast count for free-cast mechanic

        // ===== T7-T9 STATE =====
        public int serenadeRefrainTimer;        // T7: Serenade's Refrain (mana restore)
        public int tubaMirumTimer;              // T8: Tuba Mirum buff
        public int arcaneJubileeTimer;          // T9: Arcane Jubilee buff
        public int mageHealThisSecond;          // T9: Heal cap tracking (8 HP/s)
        public int mageHealCooldown;            // T9: Reset counter

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

            // --- Fusion chain inheritance ---
            if (hasPendantOfTheEternalOverflow) { hasTriumphantOverflowPendant = true; hasEternalOverflowMastery = true; }
            if (hasTriumphantOverflowPendant) { hasStarfallHarmonicPendant = true; hasJubilantArcaneCelebration = true; }
            if (hasStarfallHarmonicPendant) { hasNocturnalHarmonicOverflow = true; hasInfernalManaCataclysm = true; }

            // --- Post-Fate T7-T10 linear chain inheritance ---
            if (hasEternalOverflowMastery) hasJubilantArcaneCelebration = true;
            if (hasJubilantArcaneCelebration) hasInfernalManaCataclysm = true;
            if (hasInfernalManaCataclysm) hasNocturnalHarmonicOverflow = true;

            // T7 inherits all theme variants + seasonal chain
            if (hasNocturnalHarmonicOverflow)
            {
                hasFatesCosmicReservoir = true;
                hasSwansBalancedFlow = true;
                hasEnigmasNegativeSpace = true;
                hasInfernalManaInferno = true;
                hasHeroicArcaneSurge = true;
                hasMoonlitOverflowStar = true;
                hasVivaldisHarmonicCore = true;
            }

            // --- Seasonal T1-T6 chain inheritance ---
            if (hasVivaldisHarmonicCore) hasPermafrostVoidHeart = true;
            if (hasPermafrostVoidHeart) hasArcaneResonanceCatalyst = true;
            if (hasArcaneResonanceCatalyst) hasSearedManaConduit = true;
            if (hasSearedManaConduit) hasSpringArcaneConduit = true;
            if (hasSpringArcaneConduit) hasResonantOverflowGem = true;

            // T1-T6 stats are now applied directly in UpdateAccessory on each item.
            // Only T7+ stats and special effects are applied here.

            // (Resonance Synergy state preserved for T7+ only)

            // Nocturnal Harmonic Overflow: +20% magic damage at night, +10% during day
            if (hasNocturnalHarmonicOverflow)
            {
                if (!Main.dayTime)
                {
                    Player.GetDamage(DamageClass.Magic) += 0.20f;
                    Player.manaRegenBonus += 15; // +15 mana regen at night
                }
                else
                {
                    Player.GetDamage(DamageClass.Magic) += 0.10f;
                }
            }

            // T7 Serenade's Refrain buff timer
            if (serenadeRefrainTimer > 0)
            {
                serenadeRefrainTimer--;
                // Restore 5% max mana per second (spread across 60 frames)
                if (serenadeRefrainTimer % 60 == 0)
                {
                    int manaRestore = (int)(Player.statManaMax2 * 0.05f);
                    Player.statMana = Math.Min(Player.statMana + manaRestore, Player.statManaMax2);
                }
            }

            // Infernal Mana Cataclysm: +25% boss / +12% base, Tuba Mirum buff
            if (hasInfernalManaCataclysm)
            {
                if (AnyBossAlive())
                    Player.GetDamage(DamageClass.Magic) += 0.25f;
                else
                    Player.GetDamage(DamageClass.Magic) += 0.12f;
            }

            // T8 Tuba Mirum buff timer
            if (tubaMirumTimer > 0)
            {
                tubaMirumTimer--;
                // 8% max mana restored per second
                if (tubaMirumTimer % 60 == 0)
                {
                    int manaRestore = (int)(Player.statManaMax2 * 0.08f);
                    Player.statMana = Math.Min(Player.statMana + manaRestore, Player.statManaMax2);
                }
                Player.GetDamage(DamageClass.Magic) += 0.08f; // +8% magic damage
            }

            // Jubilant Arcane Celebration: +15% base magic damage
            if (hasJubilantArcaneCelebration)
            {
                Player.GetDamage(DamageClass.Magic) += 0.15f;
            }

            // T9 Arcane Jubilee buff timer
            if (arcaneJubileeTimer > 0)
            {
                arcaneJubileeTimer--;
                Player.GetDamage(DamageClass.Magic) += 0.05f; // +5% magic damage
            }

            // T9 heal cap reset (8 HP/s)
            if (mageHealCooldown > 0)
            {
                mageHealCooldown--;
            }
            else
            {
                mageHealThisSecond = 0;
                mageHealCooldown = 60;
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
            // Moonlit Overflow Star: Free spell when mana < 50 (T5 theme variant)
            if (freeSpellReady && item.DamageType.Equals(DamageClass.Magic))
            {
                mult = 0f;
                freeSpellReady = false;
                return;
            }

            // T5/T6 free-cast-every-Nth mechanic
            if (item.DamageType.Equals(DamageClass.Magic))
            {
                magicCastCounter++;

                int freeCastInterval = 0;
                if (hasVivaldisHarmonicCore) freeCastInterval = 8;
                else if (hasPermafrostVoidHeart) freeCastInterval = 10;

                if (freeCastInterval > 0 && magicCastCounter % freeCastInterval == 0)
                {
                    mult = 0f; // Free cast
                }

                // T2+: 5% chance to heal on magic cast
                if (hasSpringArcaneConduit && Main.rand.NextFloat() < 0.05f)
                {
                    int healAmount;
                    if (hasVivaldisHarmonicCore) healAmount = 35;
                    else if (hasPermafrostVoidHeart) healAmount = 32;
                    else if (hasArcaneResonanceCatalyst) healAmount = 30;
                    else if (hasSearedManaConduit) healAmount = 25;
                    else healAmount = 20;
                    Player.Heal(healAmount);
                }
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!item.DamageType.Equals(DamageClass.Magic))
                return;

            // ===== T1-T6 MAGE CHAIN COMBAT EFFECTS =====
            ApplyMageChainOnHit(target, hit, damageDone);

            // ===== T7+ EFFECTS =====

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

            // Swan's Balanced Flow: Kill grants +20% magic damage for 5s
            if (hasSwansBalancedFlow && target.life <= 0)
                graceBuffTimer = 300;

            // Harvest Soul Vessel: Kills restore 15 mana
            if (hasHarvestSoulVessel && target.life <= 0)
            {
                Player.statMana = Math.Min(Player.statMana + 15, Player.statManaMax2);
            }

            // T7: Serenade's Refrain on kill (5% max mana over 3s)
            if (hasNocturnalHarmonicOverflow && target.life <= 0)
            {
                serenadeRefrainTimer = 180; // 3 seconds
            }

            // T8: 5% chance on hit → Lacrimosa (enemy takes +10% magic damage for 4s)
            if (hasInfernalManaCataclysm && Main.rand.NextFloat() < 0.05f)
            {
                // Use Ichor as proxy for increased damage taken
                target.AddBuff(BuffID.Ichor, 240); // 4 seconds
            }

            // T8: Tuba Mirum on kill (8% max mana, +8% magic dmg for 3s)
            if (hasInfernalManaCataclysm && target.life <= 0)
            {
                tubaMirumTimer = 180; // 3 seconds
            }

            // T9: Jubilant Arcane Celebration: heal 2 HP on hit (capped at 8 HP/s)
            if (hasJubilantArcaneCelebration && mageHealThisSecond < 8)
            {
                int healAmount = Math.Min(2, 8 - mageHealThisSecond);
                if (healAmount > 0)
                {
                    Player.Heal(healAmount);
                    mageHealThisSecond += healAmount;
                }
            }

            // T9: 20% chance on kill → Arcane Jubilee (10 mana, +5% magic dmg for 3s)
            if (hasJubilantArcaneCelebration && target.life <= 0 && Main.rand.NextFloat() < 0.20f)
            {
                arcaneJubileeTimer = 180; // 3 seconds
                Player.statMana = Math.Min(Player.statMana + 10, Player.statManaMax2);
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!proj.DamageType.Equals(DamageClass.Magic) || proj.owner != Player.whoAmI)
                return;

            // ===== T1-T6 MAGE CHAIN COMBAT EFFECTS =====
            ApplyMageChainOnHit(target, hit, damageDone);

            // ===== T7+ EFFECTS =====

            // Infernal Mana Inferno: Fire trails
            if (hasInfernalManaInferno)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 offset = Vector2.One.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(5f, 15f);
                    Dust dust = Dust.NewDustDirect(target.Center + offset, 1, 1, DustID.Torch);
                    dust.velocity = Vector2.One.RotatedByRandom(MathHelper.TwoPi) * 1.5f;
                }
            }

            // Swan's Balanced Flow: Kill grants buff
            if (hasSwansBalancedFlow && target.life <= 0)
                graceBuffTimer = 300;

            // Harvest Soul Vessel: Kills restore 15 mana
            if (hasHarvestSoulVessel && target.life <= 0)
            {
                Player.statMana = Math.Min(Player.statMana + 15, Player.statManaMax2);
            }

            // T7: Serenade's Refrain on kill
            if (hasNocturnalHarmonicOverflow && target.life <= 0)
            {
                serenadeRefrainTimer = 180;
            }

            // T8: 5% chance on hit → Lacrimosa
            if (hasInfernalManaCataclysm && Main.rand.NextFloat() < 0.05f)
            {
                target.AddBuff(BuffID.Ichor, 240);
            }

            // T8: Tuba Mirum on kill
            if (hasInfernalManaCataclysm && target.life <= 0)
            {
                tubaMirumTimer = 180;
            }

            // T9: heal 2 HP on hit (capped at 8 HP/s)
            if (hasJubilantArcaneCelebration && mageHealThisSecond < 8)
            {
                int healAmt = Math.Min(2, 8 - mageHealThisSecond);
                if (healAmt > 0)
                {
                    Player.Heal(healAmt);
                    mageHealThisSecond += healAmt;
                }
            }

            // T9: 20% chance on kill → Arcane Jubilee
            if (hasJubilantArcaneCelebration && target.life <= 0 && Main.rand.NextFloat() < 0.20f)
            {
                arcaneJubileeTimer = 180;
                Player.statMana = Math.Min(Player.statMana + 10, Player.statManaMax2);
            }
        }

        /// <summary>
        /// Shared T1-T6 mage chain on-hit effects: burn, slow, multi-debuff.
        /// </summary>
        private void ApplyMageChainOnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // T6: Multi-debuff
            if (hasVivaldisHarmonicCore)
            {
                target.AddBuff(BuffID.OnFire, 180);
                target.AddBuff(BuffID.Frostburn, 180);
                target.AddBuff(BuffID.Poisoned, 180);
                target.AddBuff(BuffID.Bleeding, 180);
            }
            // T3-T5: Burn on hit
            else if (hasSearedManaConduit)
            {
                target.AddBuff(BuffID.OnFire, 180);
            }

            // T6: 8% slow chance
            if (hasVivaldisHarmonicCore && Main.rand.NextFloat() < 0.08f)
            {
                target.AddBuff(BuffID.Slow, 60);
            }
            // T5: 5% slow chance
            else if (hasPermafrostVoidHeart && Main.rand.NextFloat() < 0.05f)
            {
                target.AddBuff(BuffID.Slow, 60);
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
