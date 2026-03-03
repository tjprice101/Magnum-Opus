using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy.Weapons.HymnOfTheVictorious.Projectiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Content.OdeToJoy.HarmonicCores;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;

namespace MagnumOpus.Content.OdeToJoy.Weapons.HymnOfTheVictorious
{
    public class HymnOfTheVictorious : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 44;
            Item.damage = 3600;
            Item.DamageType = DamageClass.Magic;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(platinum: 4);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.OdeToJoyRarity>();
            Item.UseSound = SoundID.Item43;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.crit = 15;
            Item.mana = 25;
            Item.shoot = ModContent.ProjectileType<OrbitalNoteProjectile>();
            Item.shootSpeed = 0.01f;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<ResonantCoreOfOdeToJoy>(), 25)
            .AddIngredient(ModContent.ItemType<OdeToJoyResonantEnergy>(), 20)
            .AddIngredient(ModContent.ItemType<HarmonicCoreOfOdeToJoy>(), 3)
            .AddIngredient(ItemID.LunarBar, 20)
            .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
            .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Summons 8 orbiting music notes that gather energy then launch toward the cursor"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Launched notes home toward enemies and inflict Poisoned"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Every 5th cast triggers a massive Symphonic Explosion at the cursor"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Symphonic Explosions heal 30 HP, inflict Poisoned and Venom, and deal 1.5x damage"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Let every voice rise as one — for in triumph of spirit, the hymn of the victorious echoes through eternity'")
            {
                OverrideColor = new Color(255, 200, 50)
            });
        }
    }
}