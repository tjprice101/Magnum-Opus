// ============================================================================
// REFERENCE FILE: DebugSWING.cs — The ModItem That Spawns the Swing Projectile
// ============================================================================
// Demonstrates the held-projectile pattern:
//   - Item uses Swing style but with noMelee, noUseGraphic, channel = true
//   - CanShoot prevents spawning a second projectile while one is active
//   - The projectile IS the swing — no GlobalProjectile/ModPlayer needed
//
// Original location: Content/DebugWeapons/DebugSWING/DebugSWING.cs
// ============================================================================

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace MagnumOpus.Documentation.Reference
{
    /// <summary>
    /// REFERENCE ONLY — DO NOT USE IN PRODUCTION.
    /// Preserved copy of DebugSWING ModItem for study purposes.
    ///
    /// KEY PATTERN:
    /// 1. useStyle = Swing → triggers standard swing animation
    /// 2. noMelee = true → item sprite doesn't deal damage
    /// 3. noUseGraphic = true → hides the item sprite
    /// 4. channel = true → detects held click for combo swings
    /// 5. shoot → spawns DebugSWINGProj which IS the visual swing
    /// 6. CanShoot → prevents multiple active swings
    /// </summary>
    // Commented out to prevent compilation — this is reference only
    /*
    public class DebugSWING : ModItem
    {
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/CodaOfAnnihilation";

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Debug Swing Weapon");
        }

        public override void SetDefaults()
        {
            // Base properties
            Item.damage = 250;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.width = 80;
            Item.height = 80;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.autoReuse = true;
            Item.rare = ItemRarityID.Red;
            Item.value = Item.sellPrice(gold: 50);

            // Swing style with hidden sprite
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;

            // Projectile spawning
            Item.shoot = ModContent.ProjectileType<DebugSWINGProj>();
            Item.shootSpeed = 1f;
        }

        public override bool CanShoot(Player player)
        {
            // Only shoot if there's no active swing projectile
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == player.whoAmI && p.type == Item.shoot)
                {
                    // If the projectile is in post-swing stasis, it can be refreshed
                    if (p.ModProjectile is DebugSWINGProj swing && swing.InPostSwingStasis)
                        continue;
                    return false;
                }
            }
            return true;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity,
            ref int type, ref int damage, ref float knockback)
        {
            // Position at player center, velocity just carries direction
            position = player.MountedCenter;
            velocity = player.MountedCenter.DirectionTo(Main.MouseWorld);
        }
    }
    */
}
