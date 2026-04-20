using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.Nachtmusik.Weapons.CelestialChorusBaton.Projectiles;
using MagnumOpus.Content.Nachtmusik.Weapons.CelestialChorusBaton.Buffs;

namespace MagnumOpus.Content.Nachtmusik.Weapons.CelestialChorusBaton
{
    public class CelestialChorusBaton : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 44;
            Item.damage = 1250;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 20;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(gold: 44);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<NocturnalGuardianMinion>();
            Item.buffType = ModContent.BuffType<CelestialChorusBatonBuff>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            Projectile.NewProjectile(source, Main.MouseWorld, Vector2.Zero, type, damage, knockback, player.whoAmI);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Summon", "Summons a Nocturnal Guardian to fight for you"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Conduct the symphony of the night'")
            {
                OverrideColor = new Color(100, 120, 200)
            });
        }
    }
}
