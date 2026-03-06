using MagnumOpus.Common;
using MagnumOpus.Content.Eroica.Projectiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace MagnumOpus.Content.Eroica.Weapons.FuneralPrayer
{
    /// <summary>
    /// Funeral Prayer — Eroica channeled beam weapon channeling the solemn Marcia funebre.
    /// Hold to fire a sustained funeral flame beam that burns through enemies.
    /// The beam tracks the cursor smoothly, with collision against tiles.
    /// </summary>
    public class FuneralPrayer : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.ResearchUnlockCount = 1;
            Item.damage = 105;
            Item.DamageType = DamageClass.Magic;
            Item.width = 50;
            Item.height = 50;
            Item.useTime = 10;
            Item.useAnimation = 10;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(gold: 35);
            Item.rare = ModContent.RarityType<EroicaRainbowRarity>();
            Item.UseSound = SoundID.Item20;
            Item.autoReuse = false;
            Item.channel = true;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<FuneralPrayerChanneledBeam>();
            Item.shootSpeed = 1f;
            Item.mana = 8;
            Item.maxStack = 1;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Prevent duplicate beams — only one channeled beam at a time
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].owner == player.whoAmI
                    && Main.projectile[i].type == type)
                    return false;
            }
            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1",
            "Hold to channel a sustained funeral flame beam that burns through enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
            "The beam tracks the cursor and scorches tiles in its path"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
            "'Even heroes kneel before the pyre.'")
            {
                OverrideColor = new Color(200, 50, 50)
            });
        }
    }
}
