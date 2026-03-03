using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy.Weapons.AnthemOfGlory.Projectiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Content.OdeToJoy.HarmonicCores;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;

namespace MagnumOpus.Content.OdeToJoy.Weapons.AnthemOfGlory
{
    public class AnthemOfGlory : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 42;
            Item.damage = 2800;
            Item.DamageType = DamageClass.Magic;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.OdeToJoyRarity>();
            Item.UseSound = SoundID.Item21;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.mana = 15;
            Item.crit = 12;
            Item.shoot = ModContent.ProjectileType<GloryShardProjectile>();
            Item.shootSpeed = 18f;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<ResonantCoreOfOdeToJoy>(), 20)
            .AddIngredient(ModContent.ItemType<OdeToJoyResonantEnergy>(), 15)
            .AddIngredient(ModContent.ItemType<HarmonicCoreOfOdeToJoy>(), 2)
            .AddIngredient(ItemID.LunarBar, 15)
            .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
            .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires 3 golden shards per cast in a wide spread"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Shards chain lightning to nearby enemies on hit"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Every 3rd cast unleashes a massive glory beam that pierces infinitely"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Chain lightning can arc up to 2 times between enemies"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Let every note ring with triumph — for this is the anthem that crowns the victorious'")
            {
                OverrideColor = new Color(255, 200, 50)
            });
        }
    }
}