using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.Nachtmusik.Weapons.RequiemOfTheCosmos.Projectiles;

namespace MagnumOpus.Content.Nachtmusik.Weapons.RequiemOfTheCosmos
{
    public class RequiemOfTheCosmos : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 36;
            Item.damage = 1400;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 22;
            Item.useTime = 28;
            Item.useAnimation = 28;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.knockBack = 8f;
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.value = Item.sellPrice(gold: 30);
            Item.shoot = ModContent.ProjectileType<CosmicRequiemOrbProjectile>();
            Item.shootSpeed = 10f;
            Item.autoReuse = true;
            Item.crit = 24;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 toMouse = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
            Projectile.NewProjectile(source, position, toMouse * Item.shootSpeed,
                ModContent.ProjectileType<CosmicRequiemOrbProjectile>(), damage, knockback, player.whoAmI);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires cosmic orbs"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The cosmos has a final note. Those who hear it do not remain.'")
            {
                OverrideColor = NachtmusikPalette.LoreText
            });
        }
    }
}
