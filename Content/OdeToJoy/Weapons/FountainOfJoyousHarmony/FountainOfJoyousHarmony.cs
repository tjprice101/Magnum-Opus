using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy.Weapons.FountainOfJoyousHarmony.Buffs;
using MagnumOpus.Content.OdeToJoy.Weapons.FountainOfJoyousHarmony.Projectiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.FountainOfJoyousHarmony
{
    public class FountainOfJoyousHarmony : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 46;
            Item.height = 46;
            Item.damage = 2200;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 25;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 2f;
            Item.crit = 4;
            Item.value = Item.sellPrice(platinum: 4);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
            Item.UseSound = SoundID.Item44;
            Item.autoReuse = false;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<JoyousFountainMinion>();
            Item.shootSpeed = 0.01f;
            Item.buffType = ModContent.BuffType<JoyousFountainBuff>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            Projectile.NewProjectile(source, Main.MouseWorld, Vector2.Zero, type, damage, knockback, player.whoAmI);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore",
            "'Where the fountain flows, joy follows. Where joy flows, nothing can stand against it.'")
            { OverrideColor = OdeToJoyPalette.LoreText });
        }
    }
}
