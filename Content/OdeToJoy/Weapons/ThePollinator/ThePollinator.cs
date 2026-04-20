using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy.Weapons.ThePollinator.Projectiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThePollinator
{
    public class ThePollinator : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 52;
            Item.height = 28;
            Item.damage = 3200;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
            Item.UseSound = SoundID.Item11;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.crit = 15;
            Item.shoot = ModContent.ProjectileType<PollinatorProjectile>();
            Item.shootSpeed = 14f;
            Item.useAmmo = AmmoID.Bullet;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity,
                ModContent.ProjectileType<PollinatorProjectile>(), damage, knockback, player.whoAmI);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore",
            "'The pollen does not hate. The pollen simply is. And soon, everything else simply was.'")
            { OverrideColor = OdeToJoyPalette.LoreText });
        }
    }
}
