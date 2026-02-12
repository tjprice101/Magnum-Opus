using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.TestWeapons._04_VerdantCrescendo
{
    /// <summary>
    /// 🌿 Verdant Crescendo — Nature-themed 4-step combo melee weapon.
    /// Step 0: Vine Lash — fast horizontal sweep
    /// Step 1: Briar Sweep — wide upward arc
    /// Step 2: Thorn Eruption — downward slash spawning thorn shards
    /// Step 3: Overgrowth Cataclysm — massive slam spawning bloom burst AoE
    /// </summary>
    public class VerdantCrescendoItem : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.BladeofGrass;

        private const int MaxComboSteps = 4;
        private const int ComboResetDelay = 45;

        private int comboStep = 0;
        private int comboResetTimer = 0;

        public override void SetDefaults()
        {
            Item.damage = 190;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.width = 60;
            Item.height = 60;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6.5f;
            Item.value = Item.sellPrice(gold: 10);
            Item.rare = ItemRarityID.Red;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.shoot = ModContent.ProjectileType<VerdantCrescendoSwing>();
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
                    if (p.ModProjectile is VerdantCrescendoSwing swing && swing.InPostSwingStasis)
                        return true;
                    return false;
                }
            }
            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            position = player.RotatedRelativePoint(player.MountedCenter, true);
            velocity = position.DirectionTo(Main.MouseWorld);

            Projectile.NewProjectile(source, position, velocity, type, damage, knockback,
                player.whoAmI, ai0: comboStep);

            comboStep = (comboStep + 1) % MaxComboSteps;
            comboResetTimer = ComboResetDelay;
            return false;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "TestInfo", "[Test Weapon 04] 4-step nature combo with CalamityStyleTrailRenderer"));
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Four-swing combo culminating in a devastating bloom explosion"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Third strike launches thorn shards, final strike erupts with nature energy"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Growth follows every strike, even in battle'")
            {
                OverrideColor = new Color(80, 200, 80)
            });
        }
    }
}
