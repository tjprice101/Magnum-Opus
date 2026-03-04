using MagnumOpus.Common;
using MagnumOpus.Content.ClairDeLune.Weapons.ClockworkHarmony.Projectiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.ClockworkHarmony
{
    /// <summary>
    /// Clockwork Harmony — Ranged launcher that fires 3 sizes of clockwork gears.
    /// Small Gear (direct, fast, 1 bounce). Medium Gear (arc-lob, 3 bounces).
    /// Drive Gear (hold+release, slow heavy 2x damage). Gears mesh on collision.
    /// Gear Recall pulls all deployed gears back creating vortex.
    /// </summary>
    public class ClockworkHarmony : ModItem
    {
        private int _shotCounter;

        public override void SetDefaults()
        {
            Item.width = 88;
            Item.height = 88;
            Item.DamageType = DamageClass.Ranged;
            Item.damage = 220;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 9f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item61;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.crit = 18;
            Item.shoot = ModContent.ProjectileType<SmallGearProjectile>();
            Item.shootSpeed = 16f;
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // Alt fire: Medium arc-lobbed gear
                Item.useTime = 28;
                Item.useAnimation = 28;
                Item.shootSpeed = 10f;
            }
            else
            {
                Item.useTime = 20;
                Item.useAnimation = 20;
                Item.shootSpeed = 16f;
            }
            return base.CanUseItem(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 dir = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);

            if (player.altFunctionUse == 2)
            {
                // Medium Gear — arc-lobbed, 3 bounces
                Vector2 vel = dir * 10f + new Vector2(0, -3f); // Slight upward arc
                Projectile.NewProjectile(source, player.Center, vel,
                    ModContent.ProjectileType<MediumGearProjectile>(),
                    (int)(damage * 1.2f), knockback, player.whoAmI);
            }
            else
            {
                _shotCounter++;

                if (_shotCounter % 8 == 0)
                {
                    // Every 8th shot: Drive Gear (heavy, slow, 2x damage)
                    Projectile.NewProjectile(source, player.Center, dir * 6f,
                        ModContent.ProjectileType<DriveGearProjectile>(),
                        damage * 2, knockback * 1.5f, player.whoAmI);
                }
                else
                {
                    // Normal: Small Gear (fast, direct)
                    Projectile.NewProjectile(source, player.Center, dir * 16f,
                        ModContent.ProjectileType<SmallGearProjectile>(),
                        damage, knockback, player.whoAmI);
                }
            }

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires clockwork gears of three sizes that mesh on collision"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Right click to launch medium arc-lobbed gears"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Every 8th shot fires a heavy Drive Gear dealing double damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Meshing gears create sustained spinning AoE damage zones"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Harmony isn't found. It's engineered.'")
            {
                OverrideColor = ClairDeLunePalette.LoreText
            });
        }
    }
}
