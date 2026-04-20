using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy.Weapons.TriumphantChorus.Buffs;
using MagnumOpus.Content.OdeToJoy.Weapons.TriumphantChorus.Projectiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TriumphantChorus
{
    public class TriumphantChorus : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 46;
            Item.height = 46;
            Item.damage = 3000;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 35;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5f;
            Item.crit = 4;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
            Item.UseSound = SoundID.Item44;
            Item.autoReuse = false;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<TriumphantChorusMinion>();
            Item.shootSpeed = 10f;
            Item.buffType = ModContent.BuffType<TriumphantChorusBuff>();
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
            "'When every voice rings true, the world itself sings back in jubilation'")
            { OverrideColor = OdeToJoyPalette.LoreText });
        }
    }
}
