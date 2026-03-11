using MagnumOpus.Common;
using MagnumOpus.Content.Eroica;
using MagnumOpus.Content.Eroica.Weapons.SakurasBlossom.Projectiles;
using MagnumOpus.Content.SandboxExoblade.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.SakurasBlossom
{
    /// <summary>
    /// Sakura's Blossom — Eroica melee weapon embodying the hero's beauty and sacrifice.
    /// Features a fluid 3-phase Petal Dance combo that scatters cherry blossom petals, a Blossom Counter
    /// reflect mechanic that turns enemy projectiles into homing petal-blades, and a Sakura Meditation
    /// stance that empowers the next swing with doubled range and petals.
    /// </summary>
    public class SakurasBlossom : ModItem
    {
        /// <summary>Tracks the Petal Dance combo phase (0-2). Phase 2 = Final Bloom burst.</summary>
        private int swingCounter = 0;
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.ResearchUnlockCount = 1;
            Item.width = 70;
            Item.height = 70;
            Item.damage = 350;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 8f;
            Item.scale = 0.08f;
            Item.value = Item.sellPrice(gold: 40);
            Item.rare = ModContent.RarityType<EroicaRainbowRarity>();
            Item.UseSound = null;
            Item.autoReuse = true;
            Item.channel = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<SakurasBlossomSwing>();
            Item.shootSpeed = 6f;
            Item.maxStack = 1;
        }

        public override bool CanShoot(Player player)
        {
            bool isDash = player.altFunctionUse == 2;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (!p.active || p.owner != player.whoAmI || p.type != Item.shoot)
                    continue;
                if (isDash) return false;
                if (!(p.ai[0] == 1 && p.ai[1] == 1)) return false;
            }
            return true;
        }

        public override void HoldItem(Player player)
        {
            player.ExoBlade().rightClickListener = true;
            player.ExoBlade().mouseWorldListener = true;
        }

        public override bool AltFunctionUse(Player player) => true;
        public override bool? CanHitNPC(Player player, NPC target) => false;
        public override bool CanHitPvp(Player player, Player target) => false;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float state = player.altFunctionUse == 2 ? 1f : 0f;
            Projectile.NewProjectile(source, player.MountedCenter,
                (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX),
                type, damage, knockback, player.whoAmI, state, 0);

            // --- Petal Dance combo system ---
            // Phase 0: 3 scattered petals | Phase 1: 4 petals + 1 empowered | Phase 2: Final Bloom (8-petal burst)
            Vector2 aimDir = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX);
            int phase = swingCounter % 3;
            swingCounter++;
            int petalType = ModContent.ProjectileType<SakuraPetalProj>();
            int petalDmg = (int)(damage * 0.3f);

            if (phase == 0)
            {
                // Phase 1: 3 petals in a scattered spread
                for (int i = -1; i <= 1; i++)
                {
                    Vector2 petalVel = aimDir.RotatedBy(MathHelper.ToRadians(20 * i)) * Main.rand.NextFloat(6f, 9f);
                    petalVel += Main.rand.NextVector2Circular(1f, 1f);
                    Projectile.NewProjectile(source, player.MountedCenter, petalVel,
                        petalType, petalDmg, knockback * 0.3f, player.whoAmI,
                        0f, Main.rand.Next(4));
                }
            }
            else if (phase == 1)
            {
                // Phase 2: 4 petals in wider spread + 1 empowered center petal
                for (int i = 0; i < 4; i++)
                {
                    float angle = MathHelper.ToRadians(-30 + 20 * i);
                    Vector2 petalVel = aimDir.RotatedBy(angle) * Main.rand.NextFloat(7f, 10f);
                    Projectile.NewProjectile(source, player.MountedCenter, petalVel,
                        petalType, petalDmg, knockback * 0.3f, player.whoAmI,
                        0f, Main.rand.Next(4));
                }
                // Empowered center petal — larger, faster homing
                Projectile.NewProjectile(source, player.MountedCenter, aimDir * 11f,
                    petalType, (int)(damage * 0.45f), knockback * 0.5f, player.whoAmI,
                    1f, Main.rand.Next(4));
            }
            else // phase == 2
            {
                // Final Bloom: 8 petals in a radial burst, all empowered
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi / 8f * i;
                    Vector2 petalVel = aimDir.RotatedBy(angle) * Main.rand.NextFloat(5f, 8f);
                    Projectile.NewProjectile(source, player.MountedCenter, petalVel,
                        petalType, (int)(damage * 0.4f), knockback * 0.5f, player.whoAmI,
                        1f, (float)(i % 4));
                }
            }

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1",
            "Petal Dance — 3-phase flowing combo scatters sakura petals and homing spectral copies")
            { OverrideColor = new Color(255, 180, 200) });
            tooltips.Add(new TooltipLine(Mod, "Effect2",
            "Final Bloom unleashes a 360-degree petal burst that converges on struck enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
            "Sakura Meditation: hold without nearby enemies for enhanced next swing")
            { OverrideColor = EroicaPalette.Gold });
            tooltips.Add(new TooltipLine(Mod, "Lore",
            "'A single petal falls. An army follows.'")
            { OverrideColor = new Color(200, 50, 50) });
        }
    }
}
