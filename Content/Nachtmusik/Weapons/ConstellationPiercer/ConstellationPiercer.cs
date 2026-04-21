using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.Nachtmusik.Weapons.ConstellationPiercer.Projectiles;
using MagnumOpus.Content.Nachtmusik.Systems;

namespace MagnumOpus.Content.Nachtmusik.Weapons.ConstellationPiercer
{
    public class ConstellationPiercer : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 66;
            Item.damage = 1250;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 12;
            Item.useAnimation = 12;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4.5f;
            Item.value = Item.sellPrice(gold: 45);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.UseSound = SoundID.Item41 with { Pitch = -0.2f, Volume = 0.9f };
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<ConstellationBoltProjectile>();
            Item.shootSpeed = 22f;
            Item.useAmmo = AmmoID.Bullet;
            Item.crit = 22;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity,
                ModContent.ProjectileType<ConstellationBoltProjectile>(), damage, knockback, player.whoAmI);
            return false;
        }

        public override Vector2? HoldoutOffset() => new Vector2(-5f, 0f);

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Piercing bolts mark hit positions"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "After 3 marks, a constellation detonation connects them"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Each star is an enemy. Each line of light between them is a death sentence.'")
            {
                OverrideColor = NachtmusikPalette.LoreText
            });
        }
    }
}
