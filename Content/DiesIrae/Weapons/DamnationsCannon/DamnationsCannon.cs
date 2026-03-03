using MagnumOpus.Common;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using MagnumOpus.Content.DiesIrae.HarmonicCores;
using MagnumOpus.Content.DiesIrae.ResonanceEnergies;
using MagnumOpus.Content.Fate.CraftingStations;

namespace MagnumOpus.Content.DiesIrae.Weapons.DamnationsCannon
{
    public class DamnationsCannon : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 80;
            Item.height = 36;
            Item.damage = 3500;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 45;
            Item.useAnimation = 45;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 10f;
            Item.value = Item.sellPrice(platinum: 2, gold: 50);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.DiesIraeRarity>();
            Item.UseSound = SoundID.Item38 with { Pitch = -0.3f, Volume = 1.2f };
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<Projectiles.IgnitedWrathBallProjectile>();
            Item.shootSpeed = 14f;
            Item.useAmmo = AmmoID.Rocket;
            Item.crit = 20;
        }

        public override Vector2? HoldoutOffset() => new Vector2(-14f, 2f);

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<Content.DiesIrae.ResonanceEnergies.ResonantCoreOfDiesIrae>(), 25)
            .AddIngredient(ModContent.ItemType<Content.DiesIrae.ResonanceEnergies.DiesIraeResonantEnergy>(), 20)
            .AddIngredient(ModContent.ItemType<Content.DiesIrae.HarmonicCores.HarmonicCoreOfDiesIrae>(), 3)
            .AddIngredient(ItemID.LunarBar, 20)
            .AddTile(ModContent.TileType<Content.Fate.CraftingStations.FatesCosmicAnvilTile>())
            .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires massive exploding balls of wrath"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "On explosion, spawns 5 orbiting shrapnel that seek enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Direct hits cause devastating hellfire explosions"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The cannon that delivers damnation itself'")
            {
                OverrideColor = new Color(200, 50, 30)
            });
        }
    }
}