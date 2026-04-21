using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy.Weapons.TheGardenersFury.Projectiles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TheGardenersFury
{
    public class TheGardenersFury : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 70;
            Item.height = 70;
            Item.scale = 0.09f;
            Item.damage = 270;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useTurn = true;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.knockBack = 7.5f;
            Item.autoReuse = true;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.channel = false; // Changed: not channeled, normal swing
            Item.value = Item.sellPrice(gold: 45);
            Item.shoot = ModContent.ProjectileType<GardenerFuryProjectile>();
            Item.shootSpeed = 8f;
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
        }

        public override bool CanShoot(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;
        public override bool? CanHitNPC(Player player, NPC target) => false;
        public override bool CanHitPvp(Player player, Player target) => false;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Main swing
            Projectile.NewProjectile(source, player.MountedCenter,
                (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX),
                type, damage, knockback, player.whoAmI, 0f, 0);

            // Fire 5 seeds downward as part of the attack
            FireSeeds(source, player, damage, knockback);

            return false;
        }

        private void FireSeeds(IEntitySource source, Player player, int damage, float knockback)
        {
            int seedType = ModContent.ProjectileType<GardenerFurySeedProjectile>();

            // Fire 5 seeds in a spread pattern downward
            for (int i = 0; i < 5; i++)
            {
                float spreadAngle = MathHelper.Lerp(-0.4f, 0.4f, (float)i / 4);
                Vector2 vel = new Vector2(MathF.Sin(spreadAngle) * 4f, 4f);

                Projectile.NewProjectile(source, player.MountedCenter, vel,
                    seedType, damage, knockback, player.whoAmI);
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Behavior",
                "Swings an axe and plants 5 seeds downward. Seeds fall with gravity and become stationary zones. After 1.5s, each zone fires a homing child upward."));

            tooltips.Add(new TooltipLine(Mod, "Lore",
            "'Plant in silence. Harvest in thunder.'")
            { OverrideColor = OdeToJoyPalette.LoreText });
        }
    }
}
