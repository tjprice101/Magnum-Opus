using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.Nachtmusik.Weapons.StarweaversGrimoire.Projectiles;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.Nachtmusik.Weapons.StarweaversGrimoire
{
    public class StarweaversGrimoire : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.damage = 1200;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 14;
            Item.useTime = 14;
            Item.useAnimation = 14;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.knockBack = 4f;
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.value = Item.sellPrice(gold: 25);
            Item.shoot = ModContent.ProjectileType<StarweaverNodeProjectile>();
            Item.shootSpeed = 14f;
            Item.autoReuse = true;
            Item.crit = 20;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                // Right-click: command all active nodes to fire orbs toward cursor
                Vector2 target = Main.MouseWorld;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (proj.active && proj.owner == player.whoAmI
                        && proj.type == ModContent.ProjectileType<StarweaverNodeProjectile>()
                        && proj.ai[0] == 1f) // Stationary node
                    {
                        Vector2 toTarget = (target - proj.Center).SafeNormalize(Vector2.UnitX) * 14f;
                        GenericHomingOrbChild.SpawnChild(
                            proj.GetSource_FromThis(), proj.Center, toTarget,
                            damage, knockback, player.whoAmI,
                            0.10f, GenericHomingOrbChild.FLAG_ACCELERATE,
                            GenericHomingOrbChild.THEME_NACHTMUSIK, 0.8f, 90);
                    }
                }
                return false;
            }

            // Left-click: spawn a node projectile that becomes stationary on hit
            Vector2 toMouse = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
            Projectile.NewProjectile(source, position, toMouse * Item.shootSpeed,
                ModContent.ProjectileType<StarweaverNodeProjectile>(), damage, knockback, player.whoAmI);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires orbs that become stationary constellation nodes on hit"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Nearby nodes tether to damage crossing enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Right click to command all nodes to fire homing orbs"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'She opened the book and read the sky. Every star rearranged itself to listen.'")
            {
                OverrideColor = NachtmusikPalette.LoreText
            });
        }
    }
}
