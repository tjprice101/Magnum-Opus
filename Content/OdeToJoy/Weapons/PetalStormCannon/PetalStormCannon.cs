using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy.Weapons.PetalStormCannon.Projectiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.PetalStormCannon
{
    public class PetalStormCannon : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 62;
            Item.height = 32;
            Item.damage = 2900;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 50;
            Item.useAnimation = 50;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 12f;
            Item.value = Item.sellPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
            Item.UseSound = SoundID.Item62;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.crit = 20;
            Item.shoot = ModContent.ProjectileType<PetalBombProjectile>();
            Item.shootSpeed = 10f;
            Item.useAmmo = AmmoID.Rocket;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity,
                ModContent.ProjectileType<PetalBombProjectile>(), damage, knockback, player.whoAmI);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore",
            "'The storm does not discriminate. Joy and ruin travel together.'")
            { OverrideColor = OdeToJoyPalette.LoreText });
        }
    }
}
