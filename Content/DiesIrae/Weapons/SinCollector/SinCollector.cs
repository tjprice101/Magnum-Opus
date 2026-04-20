using MagnumOpus.Content.DiesIrae;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

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
            Item.useTime = 5;
            Item.useAnimation = 5;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 8f;
            Item.value = Item.sellPrice(platinum: 2, gold: 50);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
            Item.UseSound = SoundID.Item40 with { Pitch = -0.2f };
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<Projectiles.SinBulletProjectile>();
            Item.shootSpeed = 25f;
            Item.useAmmo = AmmoID.Bullet;
            Item.crit = 35;
        }

        public override Vector2? HoldoutOffset() => new Vector2(-10f, 0f);

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, player.MountedCenter, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Each hit collects sin from the target"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Right-click expends collected sin for devastating enhanced shots"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "10+ Sins: Penance Shot, 20+: Absolution, 30: Damnation"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Your sins are not forgiven. They are collected.'")
            {
                OverrideColor = DiesIraePalette.LoreText
            });
        }
    }
}
