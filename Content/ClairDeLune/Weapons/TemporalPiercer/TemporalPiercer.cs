using MagnumOpus.Common;
using MagnumOpus.Content.ClairDeLune.Weapons.TemporalPiercer.Projectiles;
using MagnumOpus.Content.ClairDeLune.Weapons.TemporalPiercer.Utilities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.TemporalPiercer
{
    /// <summary>
    /// Temporal Piercer — Rapier that punctures through time.
    /// Ultra-precise thrusts leave Temporal Puncture marks (max 5).
    /// At 5 marks → Frozen Moment (stun + massive burst).
    /// Alt fire launches a time-pierce boomerang through marked enemies.
    /// </summary>
    public class TemporalPiercer : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 84;
            Item.height = 84;
            Item.DamageType = DamageClass.Melee;
            Item.damage = 250;
            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 7f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item71;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.crit = 22;
            Item.shoot = ModContent.ProjectileType<TemporalThrustProjectile>();
            Item.shootSpeed = 5f;
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // Alt fire: Time-Pierce Boomerang
                Item.shoot = ModContent.ProjectileType<TimePierceBoomerangProjectile>();
                Item.useTime = 30;
                Item.useAnimation = 30;
                Item.shootSpeed = 14f;
            }
            else
            {
                // Normal: Rapier thrust
                Item.shoot = ModContent.ProjectileType<TemporalThrustProjectile>();
                Item.useTime = 16;
                Item.useAnimation = 16;
                Item.shootSpeed = 5f;
            }
            return base.CanUseItem(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                // Fire boomerang toward cursor
                Vector2 dir = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
                Projectile.NewProjectile(source, player.Center, dir * 14f,
                    ModContent.ProjectileType<TimePierceBoomerangProjectile>(),
                    damage, knockback, player.whoAmI);
                return false;
            }
            else
            {
                // Thrust toward cursor
                Vector2 dir = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
                Projectile.NewProjectile(source, player.Center, dir * 5f,
                    ModContent.ProjectileType<TemporalThrustProjectile>(),
                    damage, knockback, player.whoAmI);
                return false;
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Ultra-precise rapier thrusts inflict Temporal Puncture marks"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Five marks trigger Frozen Moment — a burst of shattered time"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "15% chance to pierce through enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Right click to launch a time-pierce boomerang through marked enemies"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Five marks upon the hours. And when the fifth chimes — the moment freezes.'")
            {
                OverrideColor = ClairDeLunePalette.LoreText
            });
        }
    }
}
