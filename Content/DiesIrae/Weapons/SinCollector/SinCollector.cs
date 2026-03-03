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

namespace MagnumOpus.Content.DiesIrae.Weapons.SinCollector
{
    public class SinCollector : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 70;
            Item.height = 28;
            Item.damage = 2400;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 8f;
            Item.value = Item.sellPrice(platinum: 2, gold: 50);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.DiesIraeRarity>();
            Item.UseSound = SoundID.Item40 with { Pitch = -0.2f };
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<Projectiles.SinBulletProjectile>();
            Item.shootSpeed = 25f;
            Item.useAmmo = AmmoID.Bullet;
            Item.crit = 35;
        }

        public override Vector2? HoldoutOffset() => new Vector2(-10f, 0f);

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<Content.DiesIrae.ResonanceEnergies.ResonantCoreOfDiesIrae>(), 20)
            .AddIngredient(ModContent.ItemType<Content.DiesIrae.ResonanceEnergies.DiesIraeResonantEnergy>(), 15)
            .AddIngredient(ModContent.ItemType<Content.DiesIrae.HarmonicCores.HarmonicCoreOfDiesIrae>(), 2)
            .AddIngredient(ItemID.LunarBar, 15)
            .AddTile(ModContent.TileType<Content.Fate.CraftingStations.FatesCosmicAnvilTile>())
            .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Converts bullets into sin-seeking rounds"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "On hit, chains lightning to 3 nearby enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Every 5th shot spawns 3 spinning phantom cleaver copies"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Each bullet claims another soul for judgment'")
            {
                OverrideColor = new Color(200, 50, 30)
            });
        }
    }
}