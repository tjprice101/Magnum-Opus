using MagnumOpus.Content.DiesIrae;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.EclipseOfWrath
{
    public class EclipseOfWrath : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 44;
            Item.damage = 1750;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 25;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 8f;
            Item.value = Item.sellPrice(platinum: 2, gold: 50);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
            Item.UseSound = SoundID.Item73 with { Pitch = -0.2f };
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<Projectiles.EclipseOrbProjectile>();
            Item.shootSpeed = 12f;
            Item.crit = 22;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, player.MountedCenter, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Throws a slow-moving eclipse orb with a dark core and fire corona"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Orb tracks your cursor and splits into 6 wrath shards on impact"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Critical hits cause Corona Flare — 12 shards + fire nova"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Destroyed orbs leave Eclipse Fields that increase damage taken by 15%"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The sun that rises for judgment is not the sun that brings dawn.'")
            {
                OverrideColor = DiesIraePalette.LoreText
            });
        }
    }
}
