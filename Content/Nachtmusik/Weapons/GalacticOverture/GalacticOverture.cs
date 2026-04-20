using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.Nachtmusik.Weapons.GalacticOverture.Projectiles;
using MagnumOpus.Content.Nachtmusik.Weapons.GalacticOverture.Buffs;

namespace MagnumOpus.Content.Nachtmusik.Weapons.GalacticOverture
{
    public class GalacticOverture : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.damage = 1200;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 18;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(gold: 42);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<CelestialMuseMinion>();
            Item.buffType = ModContent.BuffType<GalacticOvertureBuff>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            Projectile.NewProjectile(source, Main.MouseWorld, Vector2.Zero, type, damage, knockback, player.whoAmI);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Summon", "Summons a Celestial Muse to fight for you"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Let the overture begin'")
            {
                OverrideColor = new Color(100, 120, 200)
            });
        }
    }
}
