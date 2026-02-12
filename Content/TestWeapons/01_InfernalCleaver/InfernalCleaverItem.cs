using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace MagnumOpus.Content.TestWeapons._01_InfernalCleaver
{
    /// <summary>
    /// 🔥 Infernal Cleaver — Combo Test Weapon 01
    /// 4-step combo: Slow Overhead → Fast Horizontal → Rising Uppercut → Massive Slam.
    /// Each use advances comboStep. Resets after 45 frames of inactivity.
    /// </summary>
    public class InfernalCleaverItem : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.BreakerBlade;

        private int comboStep = 0;
        private int comboResetTimer = 0;
        private const int ComboResetDelay = 45;
        public const int MaxComboSteps = 4;

        public override void SetDefaults()
        {
            Item.damage = 200;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.width = 80;
            Item.height = 80;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.autoReuse = true;
            Item.rare = ItemRarityID.Red;
            Item.value = Item.sellPrice(gold: 25);

            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;

            Item.shoot = ModContent.ProjectileType<InfernalCleaverSwing>();
            Item.shootSpeed = 1f;
        }

        public override void HoldItem(Player player)
        {
            if (comboResetTimer > 0)
            {
                comboResetTimer--;
                if (comboResetTimer <= 0)
                    comboStep = 0;
            }
        }

        public override bool CanShoot(Player player)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == player.whoAmI && p.type == Item.shoot)
                {
                    if (p.ModProjectile is InfernalCleaverSwing swing && swing.InPostSwingStasis)
                        continue;
                    return false;
                }
            }
            return true;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity,
            ref int type, ref int damage, ref float knockback)
        {
            position = player.MountedCenter;
            velocity = player.MountedCenter.DirectionTo(Main.MouseWorld);
        }

        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int proj = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI,
                ai0: comboStep);

            comboStep = (comboStep + 1) % MaxComboSteps;
            comboResetTimer = ComboResetDelay;

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "4-hit infernal combo that grows in intensity"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Uppercut launches ember shards, slam erupts magma pillars"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Each strike fans the flame — the fourth ignites the earth'")
            {
                OverrideColor = new Color(255, 140, 40)
            });
        }
    }
}
