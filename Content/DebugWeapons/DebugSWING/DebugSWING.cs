using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;

namespace MagnumOpus.Content.DebugWeapons.DebugSWING
{
    /// <summary>
    /// DEBUG WEAPON: Exo Blade Swing Animation Test
    /// 
    /// This weapon replicates the Exo Blade's swing animation from Calamity:
    /// - Piecewise curved swing animation using CurveSegments
    /// - Blade stretching/squishing during swing
    /// - SwingSprite shader for blade deformation
    /// - Trail slash effect with custom shader
    /// 
    /// The focus is purely on the swing animation mechanics, not particles/effects.
    /// </summary>
    public class DebugSWING : ModItem
    {
        // Use Coda of Annihilation sprite as the base texture
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/CodaOfAnnihilation";

        public override void SetDefaults()
        {
            Item.width = 138;
            Item.height = 184;
            Item.damage = 500;
            Item.knockBack = 9f;
            Item.useTime = 49;
            Item.useAnimation = 49;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.autoReuse = true;
            Item.noMelee = true;           // Don't do melee hitbox, projectile handles it
            Item.noUseGraphic = true;      // Don't draw item sprite, projectile handles it
            Item.channel = true;           // Allow channeling for continuous swings
            Item.shoot = ModContent.ProjectileType<DebugSWINGProj>();
            Item.shootSpeed = 9f;
            Item.rare = ItemRarityID.Red;
            Item.value = Item.sellPrice(gold: 50);
        }

        public override bool CanShoot(Player player)
        {
            // Prevent spawning multiple swing projectiles
            // Allow new swing only if no active swing exists, or existing one is in "stasis"
            foreach (Projectile p in Main.ActiveProjectiles)
            {
                if (p.active && p.owner == player.whoAmI && p.type == Item.shoot)
                {
                    var swingProj = p.ModProjectile as DebugSWINGProj;
                    if (swingProj != null && !swingProj.InPostSwingStasis)
                        return false;
                }
            }
            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Spawn the swing projectile
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0, 0);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "DEBUG: Exo Blade swing animation test"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Piecewise curved animation with blade stretch/squish"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "SwingSprite shader deformation + trail slash"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The blade bends to the will of the cosmos.'")
            {
                OverrideColor = new Color(105, 240, 220)
            });
        }
    }
}
