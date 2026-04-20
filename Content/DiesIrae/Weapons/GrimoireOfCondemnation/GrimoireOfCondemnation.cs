using MagnumOpus.Content.DiesIrae;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.GrimoireOfCondemnation
{
    public class GrimoireOfCondemnation : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.damage = 1650;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 12;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(platinum: 1, gold: 50);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
            Item.UseSound = SoundID.Item103;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<Projectiles.BlazingShardProjectile>();
            Item.shootSpeed = 12f;
            Item.crit = 15;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, player.MountedCenter, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Channels a sweepable Condemnation Wave beam that widens over time"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Every 7th cast: Page Turn — beam deals +30% damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Right-click: Dark Sermon — summons a ritual circle that detonates after 3 seconds"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Kills power next cast (+5% per condemned, max +50%)"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Every name written in this book burns twice — once on the page, once in flesh.'")
            {
                OverrideColor = DiesIraePalette.LoreText
            });
        }
    }
}
