using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.TestWeapons.SandboxWeapons
{
    /// <summary>
    /// A sandbox Terra Blade using the held-projectile swing pattern.
    /// Left-click: Single swing (SandboxTerraBladeSwing).
    /// Right-click: Combo system — downswing + upswing + thrown spinning blade
    ///              that spawns cosmic shards, then snaps back with a flash.
    /// Automatically excluded from all global VFX systems via VFXExclusionHelper.
    /// </summary>
    public class SandboxTerraBlade : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.TerraBlade;

        public override void SetDefaults()
        {
            // Clone the vanilla Terra Blade stats as a baseline
            Item.CloneDefaults(ItemID.TerraBlade);

            // Held-projectile swing pattern (Exoblade/Calamity style)
            Item.useStyle = ItemUseStyleID.Swing;   // Required for arm animation
            Item.noMelee = true;                     // Projectile does damage, not item
            Item.noUseGraphic = true;                // Hide vanilla sprite (we draw ourselves)
            Item.channel = true;                      // Enable held-click behavior

            // Spawn the swing projectile
            Item.shoot = ModContent.ProjectileType<SandboxTerraBladeSwing>();
            Item.shootSpeed = 1f;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanShoot(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // Right-click: block if combo swing or spin throw are active
                return player.ownedProjectileCounts[ModContent.ProjectileType<SandboxTerraBladeComboSwing>()] <= 0
                    && player.ownedProjectileCounts[ModContent.ProjectileType<SandboxTerraBladeSpinThrow>()] <= 0;
            }

            // Left-click: prevent spawning a new swing while one is already active
            return player.ownedProjectileCounts[Item.shoot] <= 0;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 direction = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX);

            if (player.altFunctionUse == 2)
            {
                // Right-click: combo swing (downswing + upswing + throw)
                Projectile.NewProjectile(source, player.MountedCenter, direction,
                    ModContent.ProjectileType<SandboxTerraBladeComboSwing>(),
                    damage, knockback, player.whoAmI);
            }
            else
            {
                // Left-click: existing single swing
                Projectile.NewProjectile(source, player.MountedCenter, direction,
                    type, damage, knockback, player.whoAmI);
            }

            return false;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "SandboxInfo",
                "Sandbox weapon — left-click swing, right-click combo throw"));
        }
    }
}
