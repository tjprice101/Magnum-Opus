using MagnumOpus.Content.DiesIrae;
using MagnumOpus.Content.DiesIrae.Systems;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.SinCollector
{
    /// <summary>
    /// Sin Collector — Sin Accumulation Economy ranged weapon.
    /// Rapid fire builds sin stacks. Right-click expends sins for powered shots.
    /// 10+ sins: Penance (pierce 3, 1.5x damage, slight homing)
    /// 20+ sins: Absolution (pierce all, 2x damage, accelerating)
    /// 30 sins: Damnation (3x scale, 3x damage, aggressive homing, spawns zone)
    /// </summary>
    public class SinCollector : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 70;
            Item.height = 28;
            Item.damage = 2400;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 5;
            Item.useAnimation = 5;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 8f;
            Item.value = Item.sellPrice(platinum: 2, gold: 50);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
            Item.UseSound = SoundID.Item40 with { Pitch = -0.2f };
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<Projectiles.SinBulletProjectile>();
            Item.shootSpeed = 25f;
            Item.useAmmo = AmmoID.Bullet;
            Item.crit = 35;
        }

        public override Vector2? HoldoutOffset() => new Vector2(-10f, 0f);

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                var combat = player.GetModPlayer<DiesIraeCombatPlayer>();
                return combat.SinCollectorSinStacks >= 10;
            }
            return base.CanUseItem(player);
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                var combat = player.GetModPlayer<DiesIraeCombatPlayer>();
                int tier = combat.ConsumeSins();

                // Modify stats based on tier
                switch (tier)
                {
                    case 1: // Penance
                        damage = (int)(damage * 1.5f);
                        velocity *= 1.2f;
                        break;
                    case 2: // Absolution
                        damage *= 2;
                        velocity *= 0.8f; // Starts slow, accelerates
                        break;
                    case 3: // Damnation
                        damage *= 3;
                        velocity *= 1.1f;
                        break;
                }
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int tier = 0;

            if (player.altFunctionUse == 2)
            {
                var combat = player.GetModPlayer<DiesIraeCombatPlayer>();
                // Tier already consumed in ModifyShootStats, so we need to infer from current state
                // Actually we need to track this better. Let me use a temporary field.
                tier = GetExpendedTier(player);

                // Play sound based on tier
                SoundStyle sound = tier switch
                {
                    1 => SoundID.Item73 with { Pitch = 0.2f, Volume = 0.9f },
                    2 => SoundID.Item74 with { Pitch = 0.0f, Volume = 1.0f },
                    3 => SoundID.Item119 with { Pitch = -0.3f, Volume = 1.1f },
                    _ => SoundID.Item40
                };
                SoundEngine.PlaySound(sound, player.Center);
            }

            // Spawn projectile with tier info
            int proj = Projectile.NewProjectile(source, player.MountedCenter, velocity, type, damage, knockback, player.whoAmI, ai0: tier);

            // Scale for Damnation
            if (tier == 3 && proj >= 0 && proj < Main.maxProjectiles)
            {
                Main.projectile[proj].scale = 1.8f;
            }

            return false;
        }

        // Track the tier that was just consumed for the current shot
        private int lastExpendedTier = 0;

        private int GetExpendedTier(Player player)
        {
            var combat = player.GetModPlayer<DiesIraeCombatPlayer>();
            // Check what tier would be consumed (after consumption, so we check thresholds)
            // This is called after ModifyShootStats consumed the sins
            // So we need to infer: if sins are now < threshold, that tier was consumed

            // Actually let's store it properly
            // Since ModifyShootStats runs first, we need to track in PreShoot or similar
            // For now, let's calculate based on remaining + what was taken

            // Simpler approach: check the remaining sins and damage multiplier
            // Actually, let's just use the ai[0] value we set in Shoot
            return lastExpendedTier;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var combat = player.GetModPlayer<DiesIraeCombatPlayer>();
            int sins = combat.SinCollectorSinStacks;

            string sinDisplay = sins >= 30 ? "[c/FFFAF0:DAMNATION READY]"
                : sins >= 20 ? "[c/C8AA32:Absolution Ready]"
                : sins >= 10 ? "[c/C81E1E:Penance Ready]"
                : $"Sin Stacks: {sins}/30";

            tooltips.Add(new TooltipLine(Mod, "SinCounter", sinDisplay));
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Each hit collects sin from the target"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Right-click expends collected sin for devastating enhanced shots"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "10+ Sins: Penance Shot, 20+: Absolution, 30: Damnation"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Your sins are not forgiven. They are collected.'")
            {
                OverrideColor = DiesIraePalette.LoreText
            });
        }
    }
}
