using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Content.MoonlightSonata.Enemies;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon
{
    /// <summary>
    /// Resurrection of the Moon — "The Final Movement".
    /// A devastating moonlight ranged weapon.
    /// Currently a vanilla-style husk awaiting VFX reimplementation.
    /// </summary>
    public class ResurrectionOfTheMoon : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 70;
            Item.height = 26;
            Item.damage = 1500;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 90;
            Item.useAnimation = 90;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 12f;
            Item.value = Item.buyPrice(platinum: 3);
            Item.rare = ModContent.RarityType<MoonlightSonataRarity>();
            Item.UseSound = SoundID.Item40;
            Item.autoReuse = false;
            Item.noMelee = true;
            Item.shoot = ProjectileID.BulletHighVelocity;
            Item.shootSpeed = 24f;
            Item.useAmmo = AmmoID.Bullet;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'From death comes rebirth in silver light — the final movement that silences all'")
            { OverrideColor = new Color(140, 100, 200) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<MoonlightsResonantEnergy>(), 10)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfMoonlightSonata>(), 20)
                .AddIngredient(ModContent.ItemType<ShardsOfMoonlitTempo>(), 10)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }
}
