using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.Nachtmusik.Weapons.StarweaversGrimoire.Projectiles;

namespace MagnumOpus.Content.Nachtmusik.Weapons.StarweaversGrimoire
{
    public class StarweaversGrimoire : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.damage = 1200;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 14;
            Item.useTime = 14;
            Item.useAnimation = 14;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.knockBack = 4f;
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.value = Item.sellPrice(gold: 25);
            Item.shoot = ModContent.ProjectileType<StarweaverOrbProjectile>();
            Item.shootSpeed = 14f;
            Item.autoReuse = true;
            Item.crit = 20;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 toMouse = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
            Projectile.NewProjectile(source, position, toMouse * Item.shootSpeed,
                ModContent.ProjectileType<StarweaverOrbProjectile>(), damage, knockback, player.whoAmI);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires star orbs"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'She opened the book and read the sky. Every star rearranged itself to listen.'")
            {
                OverrideColor = NachtmusikPalette.LoreText
            });
        }
    }
}
