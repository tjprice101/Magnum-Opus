using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy.Weapons.TheStandingOvation.Buffs;
using MagnumOpus.Content.OdeToJoy.Weapons.TheStandingOvation.Projectiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TheStandingOvation
{
    public class TheStandingOvation : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 46;
            Item.height = 46;
            Item.damage = 2600;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 20;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3f;
            Item.crit = 4;
            Item.value = Item.sellPrice(platinum: 4);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
            Item.UseSound = SoundID.Item44;
            Item.autoReuse = false;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<StandingOvationMinion>();
            Item.shootSpeed = 10f;
            Item.buffType = ModContent.BuffType<StandingOvationBuff>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore",
            "'The audience loved the performance. The audience demands an encore.'")
            { OverrideColor = OdeToJoyPalette.LoreText });
        }
    }
}
