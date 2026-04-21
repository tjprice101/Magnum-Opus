using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.Nachtmusik.Weapons.RequiemOfTheCosmos.Projectiles;
using MagnumOpus.Content.Nachtmusik.Systems;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.Nachtmusik.Weapons.RequiemOfTheCosmos
{
    public class RequiemOfTheCosmos : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 36;
            Item.damage = 1400;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 22;
            Item.useTime = 28;
            Item.useAnimation = 28;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.knockBack = 8f;
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.value = Item.sellPrice(gold: 30);
            Item.shoot = ModContent.ProjectileType<CosmicRequiemOrbProjectile>();
            Item.shootSpeed = 10f;
            Item.autoReuse = true;
            Item.crit = 24;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var combat = player.GetModPlayer<NachtmusikCombatPlayer>();
            combat.RequiemShotCounter++;

            Vector2 toMouse = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);

            if (combat.RequiemShotCounter % 10 == 0)
            {
                // Every 10th shot: Event Horizon — large gravity well
                GenericDamageZone.SpawnZone(
                    source, Main.MouseWorld, damage, knockback, player.whoAmI,
                    GenericDamageZone.FLAG_PULL | GenericDamageZone.FLAG_SLOW,
                    200f, GenericHomingOrbChild.THEME_NACHTMUSIK, durationFrames: 180);
            }
            else if (combat.RequiemShotCounter % 3 == 0)
            {
                // Every 3rd shot: gravity well
                GenericDamageZone.SpawnZone(
                    source, Main.MouseWorld, damage / 2, knockback, player.whoAmI,
                    GenericDamageZone.FLAG_PULL,
                    100f, GenericHomingOrbChild.THEME_NACHTMUSIK, durationFrames: 120);
            }
            else
            {
                // Normal shot: homing orb
                GenericHomingOrbChild.SpawnChild(
                    source, position, toMouse * Item.shootSpeed,
                    damage, knockback, player.whoAmI,
                    homingStrength: 0.08f,
                    behaviorFlags: 0,
                    themeIndex: GenericHomingOrbChild.THEME_NACHTMUSIK,
                    scaleMult: 1f,
                    timeLeft: 120);
            }

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires homing cosmic orbs"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Every 3rd shot creates a gravity well, every 10th an Event Horizon"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The cosmos has a final note. Those who hear it do not remain.'")
            {
                OverrideColor = NachtmusikPalette.LoreText
            });
        }
    }
}
