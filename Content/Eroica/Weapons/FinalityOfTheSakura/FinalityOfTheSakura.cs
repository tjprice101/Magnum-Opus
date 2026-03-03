using MagnumOpus.Common;
using MagnumOpus.Content.Eroica.Minions;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace MagnumOpus.Content.Eroica.Weapons.FinalityOfTheSakura
{
    public class FinalityOfTheSakura : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 48;
            Item.damage = 320;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 20;
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5f;
            Item.value = Item.buyPrice(platinum: 1);
            Item.rare = ModContent.RarityType<EroicaRainbowRarity>();
            Item.UseSound = null;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<SakuraOfFate>();
            Item.buffType = ModContent.BuffType<SakuraOfFateBuff>();
            Item.maxStack = 1;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1",
            "Summons a spectral sakura spirit — the ghost of a fallen hero"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
            "Spirit fires petal blade volleys and periodically creates a sakura shield"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
            "Every 15 seconds the spirit unleashes a Final Bloom petal supernova"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
            "'The sakura does not mourn its own falling; it becomes the wind.'")
            {
                OverrideColor = new Color(200, 50, 50)
            });
        }
    }
}
