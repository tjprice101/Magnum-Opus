using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy.Weapons.AnthemOfGlory.Projectiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

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
            Item.useTime = 6;
            Item.useAnimation = 6;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
            Item.UseSound = SoundID.Item21;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.mana = 15;
            Item.crit = 12;
            Item.shoot = ModContent.ProjectileType<AnthemGloryProjectile>();
            Item.shootSpeed = 12f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore",
            "'Let every note ring with triumph — for this is the anthem that crowns the victorious'")
            { OverrideColor = OdeToJoyPalette.LoreText });
        }
    }
}
