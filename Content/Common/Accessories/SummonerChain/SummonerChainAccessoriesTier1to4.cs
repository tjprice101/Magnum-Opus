using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Materials.Foundation;
using MagnumOpus.Content.Spring.Materials;
using MagnumOpus.Content.Summer.Materials;
using MagnumOpus.Content.Autumn.Materials;
using MagnumOpus.Content.Winter.Materials;
using MagnumOpus.Content.Seasons.Accessories;

namespace MagnumOpus.Content.Common.Accessories.SummonerChain
{
    /// <summary>
    /// Resonant Conductor's Wand - Base tier summoner accessory.
    /// Simple effect: +1 minion slot.
    /// </summary>
    public class ResonantConductorsWand : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(silver: 50);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var conductor = player.GetModPlayer<ConductorPlayer>();
            conductor.HasConductorsWand = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+1 minion slot")
            {
                OverrideColor = new Color(200, 180, 255)
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'The first step in mastering the orchestra'")
            {
                OverrideColor = new Color(150, 200, 100) * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ResonantCrystalShard>(10)
                .AddIngredient(ItemID.FlinxFur, 3)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    /// <summary>
    /// Spring Maestro's Badge - Spring tier summoner accessory.
    /// Simple effect: +1 minion slot, +10% summon damage.
    /// </summary>
    public class SpringMaestrosBadge : ModItem
    {
        private static readonly Color SpringGreen = new Color(144, 238, 144);

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.rare = ItemRarityID.Green;
            Item.value = Item.sellPrice(gold: 1);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var conductor = player.GetModPlayer<ConductorPlayer>();
            conductor.HasSpringMaestrosBadge = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+1 minion slot")
            {
                OverrideColor = new Color(200, 180, 255)
            });

            tooltips.Add(new TooltipLine(Mod, "Effect2", "+10% summon damage")
            {
                OverrideColor = new Color(255, 200, 200)
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'Spring awakens the conductor within'")
            {
                OverrideColor = SpringGreen * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ResonantConductorsWand>(1)
                .AddIngredient<ResonantCrystalShard>(5)
                .AddIngredient<VernalBar>(15)
                .AddIngredient<SpringResonantEnergy>(1)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    /// <summary>
    /// Solar Director's Crest - Summer tier summoner accessory.
    /// Simple effect: +1 minion slot, +15% summon damage.
    /// </summary>
    public class SolarDirectorsCrest : ModItem
    {
        private static readonly Color SummerOrange = new Color(255, 140, 0);

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(gold: 2);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var conductor = player.GetModPlayer<ConductorPlayer>();
            conductor.HasSolarDirectorsCrest = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+1 minion slot")
            {
                OverrideColor = new Color(200, 180, 255)
            });

            tooltips.Add(new TooltipLine(Mod, "Effect2", "+15% summon damage")
            {
                OverrideColor = new Color(255, 200, 200)
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'The summer sun empowers your command'")
            {
                OverrideColor = SummerOrange * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SpringMaestrosBadge>(1)
                .AddIngredient<ResonantCrystalShard>(5)
                .AddIngredient<SolsticeBar>(15)
                .AddIngredient<SummerResonantEnergy>(1)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    /// <summary>
    /// Harvest Beastlord's Horn - Autumn tier summoner accessory.
    /// Simple effect: +1 minion slot, +5% summon crit.
    /// </summary>
    public class HarvestBeastlordsHorn : ModItem
    {
        private static readonly Color AutumnBrown = new Color(180, 100, 40);

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(gold: 4);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var conductor = player.GetModPlayer<ConductorPlayer>();
            conductor.HasHarvestBeastlordsHorn = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+1 minion slot")
            {
                OverrideColor = new Color(200, 180, 255)
            });

            tooltips.Add(new TooltipLine(Mod, "Effect2", "+5% summon critical strike chance")
            {
                OverrideColor = new Color(255, 220, 180)
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'Command the beasts of the harvest'")
            {
                OverrideColor = AutumnBrown * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SolarDirectorsCrest>(1)
                .AddIngredient<ResonantCrystalShard>(5)
                .AddIngredient<HarvestBar>(20)
                .AddIngredient<AutumnResonantEnergy>(1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    /// <summary>
    /// Permafrost Commander's Crown - Winter tier summoner accessory.
    /// Simple effect: +2 minion slots, +20% summon damage.
    /// </summary>
    public class PermafrostCommandersCrown : ModItem
    {
        private static readonly Color WinterBlue = new Color(150, 220, 255);

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(gold: 8);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var conductor = player.GetModPlayer<ConductorPlayer>();
            conductor.HasPermafrostCommandersCrown = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+2 minion slots")
            {
                OverrideColor = new Color(200, 180, 255)
            });

            tooltips.Add(new TooltipLine(Mod, "Effect2", "+20% summon damage")
            {
                OverrideColor = new Color(255, 200, 200)
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'Winter's chill commands absolute obedience'")
            {
                OverrideColor = WinterBlue * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<HarvestBeastlordsHorn>(1)
                .AddIngredient<ResonantCrystalShard>(5)
                .AddIngredient<PermafrostBar>(25)
                .AddIngredient<WinterResonantEnergy>(1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    /// <summary>
    /// Vivaldi's Orchestra Baton - Vivaldi (all seasons) tier summoner accessory.
    /// Simple effect: +2 minion slots, +25% summon damage.
    /// </summary>
    public class VivaldisOrchestraBaton : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.rare = ItemRarityID.Lime;
            Item.value = Item.sellPrice(gold: 15);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var conductor = player.GetModPlayer<ConductorPlayer>();
            conductor.HasVivaldisOrchestraBaton = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            float hue = (Main.GameUpdateCount * 0.01f) % 1f;
            Color cyclingColor = Main.hslToRgb(hue, 0.8f, 0.6f);

            tooltips.Add(new TooltipLine(Mod, "Effect1", "+2 minion slots")
            {
                OverrideColor = new Color(200, 180, 255)
            });

            tooltips.Add(new TooltipLine(Mod, "Effect2", "+25% summon damage")
            {
                OverrideColor = new Color(255, 200, 200)
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'The four seasons unite under your baton'")
            {
                OverrideColor = cyclingColor * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<PermafrostCommandersCrown>(1)
                .AddIngredient<CycleOfSeasons>(1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
