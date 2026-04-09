using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Materials.Foundation;
using MagnumOpus.Content.Spring.Materials;
using MagnumOpus.Content.Summer.Materials;
using MagnumOpus.Content.Autumn.Materials;
using MagnumOpus.Content.Winter.Materials;
using MagnumOpus.Content.Seasons.Accessories;

namespace MagnumOpus.Content.Common.Accessories.MeleeChain
{
    #region Tier 1: Pre-Hardmode Foundation

    /// <summary>
    /// Resonant Rhythm Band - Entry-level melee accessory.
    /// Simple effect: +5% melee damage, +3% melee speed.
    /// </summary>
    public class ResonantRhythmBand : ModItem
    {
        private static readonly Color BasePurple = new Color(180, 130, 255);

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Blue;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<ResonanceComboPlayer>();
            modPlayer.hasResonantRhythmBand = true;
            
            player.GetDamage(DamageClass.Melee) += 0.05f;
            player.GetAttackSpeed(DamageClass.Melee) += 0.03f;
            player.lifeRegen += 2; // +1 HP/s
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+5% melee damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+3% melee attack speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+1 HP regeneration per second"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The rhythm of battle begins with a single beat'") { OverrideColor = new Color(180, 130, 255) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ResonantCrystalShard>(10)
                .AddIngredient(ItemID.BandofRegeneration, 1)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    #endregion

    #region Tier 2: Spring

    /// <summary>
    /// Spring Tempo Charm - Post-Primavera accessory.
    /// Simple effect: +10% melee speed, 5% chance to heal 1 HP on hit.
    /// No longer requires Tier 1.
    /// </summary>
    public class SpringTempoCharm : ModItem
    {
        private static readonly Color SpringPink = new Color(255, 183, 197);

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 3);
            Item.rare = ItemRarityID.Orange;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<ResonanceComboPlayer>();
            modPlayer.hasSpringTempoCharm = true;
            
            player.GetDamage(DamageClass.Melee) += 0.08f;
            player.GetAttackSpeed(DamageClass.Melee) += 0.06f;
            player.GetCritChance(DamageClass.Melee) += 5;
            player.lifeRegen += 6; // +3 HP/s
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+8% melee damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+6% melee attack speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+5% melee critical strike chance"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "+3 HP regeneration per second"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Spring awakens the tempo of new beginnings'") { OverrideColor = new Color(255, 183, 197) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ResonantRhythmBand>(1)
                .AddIngredient<ResonantCrystalShard>(5)
                .AddIngredient<VernalBar>(15)
                .AddIngredient<SpringResonantEnergy>(1)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    #endregion

    #region Tier 3: Summer - Resonance Synergy

    /// <summary>
    /// Resonant Cleaver's Edge - Post-L'Estate accessory.
    /// Synergizes with Resonance Sliced weapons:
    /// - While holding Resonance Sliced: Resonant Burn DoT increased by 50%
    /// - Melee hits against burning enemies restore 2% of damage dealt as HP
    /// </summary>
    public class ResonantCleaversEdge : ModItem
    {
        private static readonly Color SummerOrange = new Color(255, 140, 0);

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.LightRed;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<ResonanceComboPlayer>();
            modPlayer.hasResonantCleaversEdge = true;
            
            player.GetDamage(DamageClass.Melee) += 0.15f;
            player.GetAttackSpeed(DamageClass.Melee) += 0.10f;
            player.GetCritChance(DamageClass.Melee) += 8;
            player.lifeRegen += 10; // +5 HP/s
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+15% melee damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+10% melee attack speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+8% melee critical strike chance"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "+5 HP regeneration per second"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "All melee attacks inflict Burning"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Heal 2% of damage dealt as HP vs burning enemies"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The blade hungers for the flames it creates'") { OverrideColor = new Color(255, 140, 0) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SpringTempoCharm>(1)
                .AddIngredient<ResonantCrystalShard>(5)
                .AddIngredient<SolsticeBar>(15)
                .AddIngredient<SummerResonantEnergy>(1)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    #endregion

    #region Tier 4: Autumn - Resonance Synergy

    /// <summary>
    /// Inferno Tempo Signet - Post-Autunno accessory.
    /// Synergizes with Resonant Burn debuff:
    /// - +4% melee attack speed per enemy with Resonant Burn active (max 20% at 5 enemies)
    /// - Melee hits extend Resonant Burn duration by 2 seconds
    /// </summary>
    public class InfernoTempoSignet : ModItem
    {
        private static readonly Color AutumnOrange = new Color(255, 100, 30);

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 8);
            Item.rare = ItemRarityID.Pink;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<ResonanceComboPlayer>();
            modPlayer.hasInfernoTempoSignet = true;
            
            player.GetDamage(DamageClass.Melee) += 0.17f;
            player.GetAttackSpeed(DamageClass.Melee) += 0.13f;
            player.GetCritChance(DamageClass.Melee) += 10;
            player.lifeRegen += 14; // +7 HP/s
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+17% melee damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+13% melee attack speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+10% melee critical strike chance"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "+7 HP regeneration per second"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "All melee attacks inflict Burning"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Melee hits extend burn duration by 2 seconds"));
            tooltips.Add(new TooltipLine(Mod, "Effect7", "Heal 3% of damage dealt as HP vs burning enemies"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Each burning foe quickens the rhythm of battle'") { OverrideColor = new Color(255, 100, 30) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ResonantCleaversEdge>(1)
                .AddIngredient<ResonantCrystalShard>(5)
                .AddIngredient<HarvestBar>(20)
                .AddIngredient<AutumnResonantEnergy>(1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    #endregion

    #region Tier 5: Winter

    /// <summary>
    /// Permafrost Cadence Seal - Post-L'Inverno accessory.
    /// Simple effect: 10% chance to freeze enemies for 1 second on melee hit.
    /// No longer requires Tier 4.
    /// </summary>
    public class PermafrostCadenceSeal : ModItem
    {
        private static readonly Color WinterBlue = new Color(150, 220, 255);

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 12);
            Item.rare = ItemRarityID.LightPurple;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<ResonanceComboPlayer>();
            modPlayer.hasPermafrostCadenceSeal = true;
            
            player.GetDamage(DamageClass.Melee) += 0.20f;
            player.GetAttackSpeed(DamageClass.Melee) += 0.15f;
            player.GetCritChance(DamageClass.Melee) += 13;
            player.lifeRegen += 18; // +9 HP/s
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+20% melee damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+15% melee attack speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+13% melee critical strike chance"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "+9 HP regeneration per second"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "All melee attacks inflict Burning"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Melee hits extend burn duration by 2 seconds"));
            tooltips.Add(new TooltipLine(Mod, "Effect7", "Heal 5% of damage dealt as HP vs burning enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect8", "2% chance to freeze enemies for 1 second on hit"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Winter's cadence freezes time itself'") { OverrideColor = new Color(150, 220, 255) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<InfernoTempoSignet>(1)
                .AddIngredient<ResonantCrystalShard>(5)
                .AddIngredient<PermafrostBar>(25)
                .AddIngredient<WinterResonantEnergy>(1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    /// <summary>
    /// Vivaldi's Tempo Master - Post-Plantera accessory (All Seasons combined).
    /// Simple effect: +12% melee damage, biome-dependent debuff on hit.
    /// Requires CycleOfSeasons, not previous tier.
    /// </summary>
    public class VivaldisTempoMaster : ModItem
    {
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SummerOrange = new Color(255, 140, 0);
        private static readonly Color AutumnBrown = new Color(180, 100, 40);
        private static readonly Color WinterBlue = new Color(150, 220, 255);

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 20);
            Item.rare = ItemRarityID.Lime;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<ResonanceComboPlayer>();
            modPlayer.hasVivaldisTempoMaster = true;
            
            player.GetDamage(DamageClass.Melee) += 0.23f;
            player.GetAttackSpeed(DamageClass.Melee) += 0.18f;
            player.GetCritChance(DamageClass.Melee) += 15;
            player.lifeRegen += 22; // +11 HP/s
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+23% melee damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+18% melee attack speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+15% melee critical strike chance"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "+11 HP regeneration per second"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "All melee attacks inflict Burning, Frostburn, Poison, and Bleeding"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Melee hits extend all status effects by 2 seconds"));
            tooltips.Add(new TooltipLine(Mod, "Effect7", "Heal 7% of damage dealt as HP vs burning enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect8", "3% chance to freeze enemies for 1 second on hit"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The Four Seasons unite under the maestro's baton'") { OverrideColor = new Color(150, 200, 100) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<PermafrostCadenceSeal>(1)
                .AddIngredient<CycleOfSeasons>(1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    #endregion
}
