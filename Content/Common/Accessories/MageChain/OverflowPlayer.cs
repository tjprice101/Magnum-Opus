using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

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

        // ===== LEGACY COMPATIBILITY STUBS =====
        // These properties exist for backwards compatibility with old code
        public int currentOverflow => 0;
        public int maxOverflow => 0;
        public bool isInOverflow => false;
        public bool zeroManaFreeSpell => freeSpellReady;
        public bool justRecoveredFromOverflow => false;

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
            // Apply simple static effects from equipped accessories

            // Resonant Overflow Gem: +5% magic damage, +20 max mana
            if (hasResonantOverflowGem)
            {
                Player.GetDamage(DamageClass.Magic) += 0.05f;
                Player.statManaMax2 += 20;
            }

            // Spring Arcane Conduit: +10% magic damage
            if (hasSpringArcaneConduit)
            {
                Player.GetDamage(DamageClass.Magic) += 0.10f;
            }

            // Permafrost Void Heart: +15% magic damage, +50 max mana
            if (hasPermafrostVoidHeart)
            {
                Player.GetDamage(DamageClass.Magic) += 0.15f;
                Player.statManaMax2 += 50;
            }

            // Vivaldi's Harmonic Core: +20% magic damage
            if (hasVivaldisHarmonicCore)
            {
                Player.GetDamage(DamageClass.Magic) += 0.20f;
            }

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

            // Pendant of the Eternal Overflow: +40% magic damage
            if (hasPendantOfTheEternalOverflow)
            {
                Player.GetDamage(DamageClass.Magic) += 0.40f;
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
            if (hasSpringArcaneConduit && Main.rand.NextFloat() < 0.08f)
            {
                Player.Heal(1);
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
            if (hasSpringArcaneConduit && Main.rand.NextFloat() < 0.08f)
            {
                Player.Heal(1);
            }

            // Jubilant Arcane Celebration: Casting spells heals 1 HP per 20 mana spent
            if (hasJubilantArcaneCelebration)
            {
                // Estimate:  assume average spell costs ~40 mana
                Player.Heal(2);
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
    }
}
