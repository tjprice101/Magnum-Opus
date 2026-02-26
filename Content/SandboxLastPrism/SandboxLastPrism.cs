using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.SandboxLastPrism
{
    /// <summary>
    /// Sandbox Last Prism - a weapon that fires the vanilla Last Prism projectile
    /// with VFX+ enhanced visuals applied via GlobalProjectile overrides.
    /// </summary>
    public class SandboxLastPrism : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.LastPrism;

        public override void SetDefaults()
        {
            // Copy vanilla Last Prism stats exactly
            Item.CloneDefaults(ItemID.LastPrism);
            Item.damage = 100;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 12;
            Item.width = 28;
            Item.height = 30;
            Item.useTime = 10;
            Item.useAnimation = 10;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 0f;
            Item.value = Item.sellPrice(gold: 10);
            Item.rare = ItemRarityID.Red;
            Item.autoReuse = true;
            Item.shoot = ProjectileID.LastPrism;
            Item.shootSpeed = 30f;
            Item.channel = true;
            Item.noUseGraphic = true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Fires the vanilla Last Prism held projectile, which in turn spawns the laser projectiles
            Projectile.NewProjectile(source, position, velocity, ProjectileID.LastPrism, damage, knockback, player.whoAmI);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires a concentrated beam of rainbow energy"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Individual beams converge into a devastating combined laser"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A sandbox replica of prismatic devastation, enhanced with VFX+ visual rework'")
            {
                OverrideColor = Main.DiscoColor
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.DirtBlock, 1)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
