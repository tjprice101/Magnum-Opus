using MagnumOpus.Content.DiesIrae;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.DamnationsCannon
{
    public class DamnationsCannon : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 80;
            Item.height = 36;
            Item.damage = 2350;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 45;
            Item.useAnimation = 45;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 10f;
            Item.value = Item.sellPrice(platinum: 2, gold: 50);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
            Item.UseSound = SoundID.Item38 with { Pitch = -0.3f, Volume = 1.2f };
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<Projectiles.IgnitedWrathBallProjectile>();
            Item.shootSpeed = 14f;
            Item.useAmmo = AmmoID.Rocket;
            Item.crit = 20;
        }

        public override Vector2? HoldoutOffset() => new Vector2(-14f, 2f);

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, player.MountedCenter, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Lobs an arcing ball of concentrated wrath that explodes on impact"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Explosions spawn 5 homing shrapnel fragments and a hellfire zone"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Hellfire zones persist for 5 seconds with escalating damage"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'This is not a weapon. This is a sentence.'")
            {
                OverrideColor = DiesIraePalette.LoreText
            });
        }
    }
}
