using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.Fate;
using MagnumOpus.Content.Fate.Accessories;
using MagnumOpus.Content.Fate.HarmonicCores;
using MagnumOpus.Content.Seasons.Accessories;
using MagnumOpus.Content.Spring.Materials;
using MagnumOpus.Content.Summer.Materials;
using MagnumOpus.Content.Autumn.Materials;
using MagnumOpus.Content.Winter.Materials;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Content.LaCampanella.ResonanceEnergies;
using MagnumOpus.Content.EnigmaVariations.ResonanceEnergies;
using MagnumOpus.Content.SwanLake.ResonanceEnergies;

namespace MagnumOpus.Content.Common.Accessories
{
    /// <summary>
    /// Applies Complete Harmony stats and conditional bonuses (Harmonic Resonance tiers, Full Harmony, Heroic Surge).
    /// Used by Opus of Four Movements, Theme Wanderer, and Summoner's Magnum Opus.
    /// </summary>
    internal static class CompleteHarmonyStatsHelper
    {
        public static void Apply(Player player)
        {
            var chPlayer = player.GetModPlayer<CompleteHarmonyPlayer>();
            chPlayer.completeHarmonyEquipped = true;

            bool isNight = !Main.dayTime;

            // From Sonata's Embrace
            if (isNight)
                player.GetDamage(DamageClass.Generic) += 0.15f;
            else
                player.GetDamage(DamageClass.Generic) += 0.10f;
            player.manaCost -= 0.12f;

            // From Hero's Symphony
            player.GetAttackSpeed(DamageClass.Melee) += 0.15f;

            // From Infernal Virtuoso
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.lavaImmune = true;
            player.maxMinions += 1;

            // From Riddle of the Void
            player.GetDamage(DamageClass.Generic) += 0.15f;

            // From Swan's Chromatic Diadem
            player.moveSpeed += 0.25f;
            player.runAcceleration *= 1.25f;
            player.GetDamage(DamageClass.Generic) += 0.14f;

            // Harmonic Resonance tier bonuses
            if (chPlayer.harmonicResonanceStacks >= 1)
                player.endurance += 0.05f;
            if (chPlayer.harmonicResonanceStacks >= 3)
            {
                player.GetAttackSpeed(DamageClass.Generic) += 0.10f;
                player.lifeRegen += 5;
            }

            // Full Harmony buff
            if (chPlayer.fullHarmonyTimer > 0)
                player.GetDamage(DamageClass.Generic) += 0.25f;

            // Heroic Surge buff
            if (chPlayer.heroicSurgeTimer > 0)
                player.GetDamage(DamageClass.Generic) += 0.25f;
        }
    }

    /// <summary>
    /// Applies Vivaldi's Masterwork stats and enables lifesteal via VivaldiPlayer.
    /// Used by Opus of Four Movements and Seasonal Destiny.
    /// </summary>
    internal static class VivaldiStatsHelper
    {
        public static void Apply(Player player)
        {
            player.GetModPlayer<VivaldiPlayer>().vivaldiEquipped = true;

            player.GetDamage(DamageClass.Generic) += 0.25f;
            player.GetCritChance(DamageClass.Generic) += 18;
            player.GetAttackSpeed(DamageClass.Generic) += 0.15f;
            player.statDefense += 23;
            player.moveSpeed += 0.20f;
            player.endurance += 0.15f;
            player.lifeRegen += 10;
            player.manaRegenBonus += 40;
            player.magmaStone = true;
            player.frostBurn = true;
            player.thorns = 1.5f;

            player.buffImmune[BuffID.Frozen] = true;
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            player.buffImmune[BuffID.Chilled] = true;
            player.buffImmune[BuffID.Poisoned] = true;
        }
    }

    /// <summary>
    /// Applies Machination of the Event Horizon mobility stats and cosmic dodge.
    /// Used by CosmicWardensRegalia and ThemeWanderer.
    /// </summary>
    internal static class EventHorizonStatsHelper
    {
        public static void Apply(Player player, float dodgeChance)
        {
            var ehPlayer = player.GetModPlayer<EventHorizonPlayer>();
            ehPlayer.hasEventHorizon = true;
            ehPlayer.cosmicDodgeChance += dodgeChance;

            // Master Ninja Gear
            player.dashType = 1;
            player.spikedBoots = 2;
            player.blackBelt = true;

            // Terraspark Boots
            player.accRunSpeed = 6.75f;
            player.rocketBoots = player.vanityRocketBoots = 3;
            player.moveSpeed += 0.08f;
            player.iceSkate = true;
            player.waterWalk = true;
            player.fireWalk = true;
            player.lavaMax += 7 * 60;

            // Frog Leg
            player.autoJump = true;
            player.jumpSpeedBoost += 2.4f;
            player.fallStart = (int)(player.fallStart + player.maxFallSpeed);
        }
    }

    #region Opus of Four Movements - Complete Harmony + Vivaldi's Masterwork
    /// <summary>
    /// Grand Combination: Complete Harmony + Vivaldi's Masterwork + All Resonant Energies.
    /// Delegates to CompleteHarmonyPlayer (all 5 theme procs, Harmonic Resonance, Dissonance)
    /// and VivaldiPlayer (33% lifesteal 8% damage).
    /// </summary>
    public class OpusOfFourMovements : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 3);
            Item.rare = ModContent.RarityType<FateRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CompleteHarmonyStatsHelper.Apply(player);
            VivaldiStatsHelper.Apply(player);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<CompleteHarmony>()
                .AddIngredient<VivaldisMasterwork>()
                .AddIngredient<MoonlightsResonantEnergy>()
                .AddIngredient<EroicasResonantEnergy>()
                .AddIngredient<LaCampanellaResonantEnergy>()
                .AddIngredient<EnigmaResonantEnergy>()
                .AddIngredient<SwansResonanceEnergy>()
                .AddIngredient<DormantSpringCore>()
                .AddIngredient<DormantSummerCore>()
                .AddIngredient<DormantAutumnCore>()
                .AddIngredient<DormantWinterCore>()
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color fateColor = new Color(180, 40, 80);

            tooltips.Add(new TooltipLine(Mod, "Effect1", "Combines Complete Harmony with Vivaldi's Masterwork"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "All 5 theme procs: Moonstruck, double damage, Tolling Death, Paradox, Dying Swan's Grace"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Harmonic Resonance stacking and Dissonance (3+ debuffs = 20% bonus damage)"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Vivaldi: +25% all damage, +18% crit, +15% speed, +23 defense, +15% DR"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "33% chance on hit to lifesteal 8% damage (max 35 HP)"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Fire/lava immunity, +1 minion, 150% thorns, inflicts On Fire! and Frostburn"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Four seasons, five themes — the opus reaches its grandest crescendo'")
            {
                OverrideColor = fateColor
            });
        }
    }
    #endregion

    #region Symphony of Fate's Tempo - All 5 Fate Accessories Combined
    /// <summary>
    /// Grand Combination: All 5 Fate class accessories with +2% stat boost.
    /// Delegates to all 5 Fate ModPlayers for proc mechanics.
    /// Display name: "Symphony of Fate's Tempo" (class kept as CosmicWardensRegalia for compatibility).
    /// </summary>
    public class CosmicWardensRegalia : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 4);
            Item.rare = ModContent.RarityType<FateRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // === Enable all 5 Fate ModPlayer proc systems ===
            player.GetModPlayer<AstralConduitPlayer>().hasAstralConduit = true;
            player.GetModPlayer<ParadoxChronometerPlayer>().hasParadoxChronometer = true;
            player.GetModPlayer<ConstellationCompassPlayer>().hasConstellationCompass = true;
            player.GetModPlayer<OrreryPlayer>().hasOrrery = true;

            // Event Horizon: flag + dodge chance (resets each frame in ResetEffects)
            EventHorizonStatsHelper.Apply(player, 0.10f); // 8% + 2% boost

            // === Stats from Astral Conduit (+2%) ===
            player.GetDamage(DamageClass.Magic) += 0.22f;
            player.manaRegenBonus += 25;
            player.manaCost -= 0.12f;

            // === Stats from Paradox Chronometer (+2%) ===
            player.GetDamage(DamageClass.Melee) += 0.20f;
            player.GetAttackSpeed(DamageClass.Melee) += 0.22f;
            player.GetCritChance(DamageClass.Melee) += 12;

            // === Stats from Constellation Compass (+2%) ===
            player.GetDamage(DamageClass.Ranged) += 0.20f;
            player.GetCritChance(DamageClass.Ranged) += 17;
            player.GetAttackSpeed(DamageClass.Ranged) += 0.14f;

            // === Stats from Orrery of Infinite Orbits (+2%) ===
            player.GetDamage(DamageClass.Summon) += 0.24f;
            player.maxMinions += 1;
            player.GetKnockback(DamageClass.Summon) += 0.12f;

            // === Combined immunities ===
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.CursedInferno] = true;
            player.buffImmune[BuffID.ShadowFlame] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            player.buffImmune[BuffID.Confused] = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ParadoxChronometer>()
                .AddIngredient<ConstellationCompass>()
                .AddIngredient<AstralConduit>()
                .AddIngredient<MachinationoftheEventHorizon>()
                .AddIngredient<OrreryofInfiniteOrbits>()
                .AddIngredient<HarmonicCoreOfFate>(50)
                .AddIngredient<MoonlightsResonantEnergy>()
                .AddIngredient<EroicasResonantEnergy>()
                .AddIngredient<LaCampanellaResonantEnergy>()
                .AddIngredient<EnigmaResonantEnergy>()
                .AddIngredient<SwansResonanceEnergy>()
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color fateColor = new Color(180, 40, 80);

            tooltips.Add(new TooltipLine(Mod, "Effect1", "Combines all five Fate class accessories with +2% stat boost"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Magic: +22% damage, +25 mana regen, -12% mana cost, 15% cosmic flare chain"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Melee: +20% damage, +22% speed, +12 crit, every 7th hit temporal echo (75% damage)"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Ranged: +20% damage, +17% crit, +14% speed, homing and crit starbursts"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Summon: +24% damage, +1 minion, +12% knockback, periodic Cosmic Empowerment (+25%)"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Full mobility: dash, wall climb, Terraspark speed, Frog Leg jump, 10% cosmic dodge"));
            tooltips.Add(new TooltipLine(Mod, "Effect7", "Immune to On Fire, Cursed Inferno, Shadowflame, Frostburn, Confused"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Five destinies converge — the symphony of fate plays its final movement'")
            {
                OverrideColor = fateColor
            });
        }
    }
    #endregion

    #region Seasonal Destiny - Vivaldi's Masterwork + Paradox Chronometer
    /// <summary>
    /// Grand Combination: Vivaldi's Masterwork + Paradox Chronometer.
    /// Delegates to VivaldiPlayer (lifesteal) and ParadoxChronometerPlayer (7th-hit echo).
    /// </summary>
    public class SeasonalDestiny : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 2);
            Item.rare = ModContent.RarityType<FateRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            VivaldiStatsHelper.Apply(player);
            player.GetModPlayer<ParadoxChronometerPlayer>().hasParadoxChronometer = true;

            // Stats from Paradox Chronometer
            player.GetDamage(DamageClass.Melee) += 0.18f;
            player.GetAttackSpeed(DamageClass.Melee) += 0.20f;
            player.GetCritChance(DamageClass.Melee) += 10;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<VivaldisMasterwork>()
                .AddIngredient<ParadoxChronometer>()
                .AddIngredient<HarmonicCoreOfFate>(30)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color destinyColor = new Color(200, 150, 180);

            tooltips.Add(new TooltipLine(Mod, "Effect1", "Combines Vivaldi's Masterwork with Paradox Chronometer"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Vivaldi: +25% all damage, +18% crit, +15% speed, +23 defense, +15% DR, +10 life regen"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "33% lifesteal (8% damage, max 35 HP), 150% thorns, inflicts On Fire! and Frostburn"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Chronometer: +18% melee damage, +20% melee speed, +10 melee crit"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Every 7th melee strike triggers a temporal echo (75% damage)"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Immune to Frozen, On Fire, Frostburn, Chilled, Poisoned"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The seasons turn, and time echoes with each blade's stroke'")
            {
                OverrideColor = destinyColor
            });
        }
    }
    #endregion

    #region Theme Wanderer - Complete Harmony + Event Horizon
    /// <summary>
    /// Grand Combination: Complete Harmony + Machination of the Event Horizon.
    /// Delegates to CompleteHarmonyPlayer (all 5 theme procs, Harmonic Resonance, Dissonance)
    /// and EventHorizonPlayer (cosmic dodge).
    /// </summary>
    public class ThemeWanderer : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 2);
            Item.rare = ModContent.RarityType<FateRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CompleteHarmonyStatsHelper.Apply(player);
            EventHorizonStatsHelper.Apply(player, 0.08f);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<CompleteHarmony>()
                .AddIngredient<MachinationoftheEventHorizon>()
                .AddIngredient<HarmonicCoreOfFate>(30)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            float hue = (Main.GameUpdateCount * 0.01f) % 1f;
            Color wandererColor = Main.hslToRgb(hue, 0.8f, 0.6f);

            tooltips.Add(new TooltipLine(Mod, "Effect1", "Combines Complete Harmony with Machination of the Event Horizon"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Harmony: +10-15% damage (night/day), -12% mana cost, +15% melee speed, +25% movement"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "All 5 theme procs: Moonstruck, double damage, Tolling Death, Paradox, Dying Swan's Grace"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Harmonic Resonance stacking and Dissonance, fire/lava immunity, +1 minion"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Full mobility: dash, wall climb, Terraspark speed, Frog Leg jump"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "8% cosmic dodge with 3s cooldown"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Wandering between worlds of sound, carried by cosmic winds'")
            {
                OverrideColor = wandererColor
            });
        }
    }
    #endregion

    #region Summoner's Magnum Opus - Complete Harmony + Orrery
    /// <summary>
    /// Grand Combination: Complete Harmony + Orrery of Infinite Orbits.
    /// Delegates to CompleteHarmonyPlayer (all 5 theme procs, Harmonic Resonance, Dissonance)
    /// and OrreryPlayer (periodic Cosmic Empowerment for minions).
    /// </summary>
    public class SummonersMagnumOpus : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 2);
            Item.rare = ModContent.RarityType<FateRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CompleteHarmonyStatsHelper.Apply(player);
            player.GetModPlayer<OrreryPlayer>().hasOrrery = true;

            // Stats from Orrery of Infinite Orbits
            player.GetDamage(DamageClass.Summon) += 0.22f;
            player.maxMinions += 1;
            player.GetKnockback(DamageClass.Summon) += 0.10f;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<CompleteHarmony>()
                .AddIngredient<OrreryofInfiniteOrbits>()
                .AddIngredient<HarmonicCoreOfFate>(30)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color summonColor = new Color(150, 100, 200);

            tooltips.Add(new TooltipLine(Mod, "Effect1", "Combines Complete Harmony with Orrery of Infinite Orbits"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Harmony: +10-15% damage (night/day), -12% mana cost, +15% melee speed, +25% movement"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "All 5 theme procs: Moonstruck, double damage, Tolling Death, Paradox, Dying Swan's Grace"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Harmonic Resonance stacking and Dissonance, fire/lava immunity, +1 minion"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Orrery: +22% summon damage, +1 minion, +10% knockback"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Periodic Cosmic Empowerment: empowered minions deal +25% damage"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The conductor raises the baton — every minion plays its part in the magnum opus'")
            {
                OverrideColor = summonColor
            });
        }
    }
    #endregion
}
