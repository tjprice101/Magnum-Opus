using MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Projectiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning
{
    /// <summary>
    /// Thornbound Reckoning — Ode to Joy melee sword.
    /// Left-click: Swing fires 1 orb with 3 bounces. Each bounce +20% damage.
    /// Right-click charge: fires 3 orbs with 2 bounces each at ±10° angles.
    /// Thorn convergence: 2+ orbs hitting same enemy in 10 frames = bonus damage.
    /// </summary>
    public class ThornboundReckoning : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 70;
            Item.height = 70;
            Item.scale = 0.09f;
            Item.damage = 290;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useTurn = true;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.knockBack = 7.5f;
            Item.autoReuse = true;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.channel = true;
            Item.value = Item.sellPrice(gold: 45);
            Item.shoot = ModContent.ProjectileType<ThornboundSwingProj>();
            Item.shootSpeed = 8f;
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
        }

        public override bool CanShoot(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;
        public override bool? CanHitNPC(Player player, NPC target) => false;
        public override bool CanHitPvp(Player player, Player target) => false;

        public override bool AltFunctionUse(Player player) => true;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                // Right-click: Fire 3 bouncing thorns at different angles
                Vector2 dir = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX);
                float speed = 14f;

                // Fire at -10°, 0°, +10°
                float[] angles = { -MathHelper.ToRadians(10f), 0f, MathHelper.ToRadians(10f) };
                foreach (float angle in angles)
                {
                    Vector2 vel = dir.RotatedBy(angle) * speed;
                    Projectile.NewProjectile(
                        source, player.MountedCenter + dir * 30f, vel,
                        ModContent.ProjectileType<BouncingThornProjectile>(),
                        damage, knockback, player.whoAmI,
                        ai0: 2, ai1: 1f); // 2 bounces for right-click
                }

                SoundEngine.PlaySound(SoundID.Item17 with { Pitch = 0.4f, Volume = 0.8f }, player.Center);

                // VFX burst
                OdeToJoyVFXLibrary.SpawnBloomBurst(player.MountedCenter + dir * 30f, 6, 0.8f);

                return false;
            }

            // Left-click: Normal swing
            Projectile.NewProjectile(source, player.MountedCenter,
                (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX),
                type, damage, knockback, player.whoAmI, 0f, 0);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Swing fires a bouncing thorn orb (3 bounces)"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Each bounce increases damage by 20%"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Right-click fires 3 thorns at spread angles (2 bounces each)"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Multiple thorns hitting the same target trigger convergence bonus"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
            "'The vine does not ask permission to grow. It simply overcomes.'")
            { OverrideColor = OdeToJoyPalette.LoreText });
        }
    }
}
