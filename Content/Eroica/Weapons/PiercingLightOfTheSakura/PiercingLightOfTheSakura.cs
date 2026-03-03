using MagnumOpus.Common;
using MagnumOpus.Content.Eroica;
using MagnumOpus.Content.Eroica.Projectiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace MagnumOpus.Content.Eroica.Weapons.PiercingLightOfTheSakura
{
    public class PiercingLightOfTheSakura : ModItem
    {
        private int shotCounter = 0;

        public override void SetDefaults()
        {
            Item.ResearchUnlockCount = 1;
            Item.damage = 155;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 64;
            Item.height = 24;
            Item.useTime = 8;
            Item.useAnimation = 8;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 2.5f;
            Item.value = Item.sellPrice(gold: 40);
            Item.rare = ModContent.RarityType<EroicaRainbowRarity>();
            Item.UseSound = SoundID.Item11;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<PiercingLightOfTheSakuraProjectile>();
            Item.shootSpeed = 18f;
            Item.useAmmo = AmmoID.Bullet;
            Item.noMelee = true;
            Item.maxStack = 1;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            shotCounter++;
            bool isCulmination = shotCounter % 8 == 0;
            bool heroFinalLight = player.statLife < player.statLifeMax2 * 0.2f;
            if (heroFinalLight)
                isCulmination = shotCounter % 4 == 0;

            // ai[0] = 1 for Culmination shots
            Projectile.NewProjectile(source, position, velocity,
                ModContent.ProjectileType<PiercingLightOfTheSakuraProjectile>(),
                isCulmination ? (int)(damage * 1.5f) : damage,
                knockback, player.whoAmI, ai0: isCulmination ? 1f : 0f);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1",
            "Fast piercing sakura projectiles that burst through enemies")
            { OverrideColor = EroicaPalette.Sakura });
            tooltips.Add(new TooltipLine(Mod, "Effect2",
            "Every 8th shot is a Culmination — pierces infinitely with detonating sakura lightning"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
            "Hero's Final Light: below 20% HP, Culmination fires every 4th shot")
            { OverrideColor = EroicaPalette.Gold });
            tooltips.Add(new TooltipLine(Mod, "Lore",
            "'The light that pierces is the one that never faltered.'")
            {
                OverrideColor = new Color(200, 50, 50)
            });
        }
    }
}
