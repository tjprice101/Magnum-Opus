using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.DiesIrae.ResonanceEnergies;
using MagnumOpus.Content.DiesIrae.HarmonicCores;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict.Projectiles;
using MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict.Utilities;

namespace MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict
{
    /// <summary>
    /// Executioner's Verdict — The heaviest Dies Irae melee weapon.
    /// A colossal guillotine blade that renders final judgment.
    /// 
    /// MECHANICS (preserved from original):
    /// - 4200 damage, 32 useTime, 12 KB, crit 25
    /// - 3 Verdict Bolts on swing → explode into spectral sword strikes
    /// - Execute non-boss enemies below 15% HP instantly
    /// - 50% bonus damage to enemies below 30% HP
    /// 
    /// NEW VFX SYSTEM:
    /// - 3-phase combo: Horizontal Cleave → Overhead Slam → GUILLOTINE DROP
    /// - Self-contained particle system (blood drips, judgment smoke, ember shards)
    /// - GPU primitive trail rendering with CatmullRom smoothing
    /// - Execution marks on low-HP enemies with pulsing intensity
    /// - Heavy screen shake on Guillotine Drop and executions
    /// - Dark crimson/blood/ash color palette
    /// </summary>
    public class ExecutionersVerdict : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 80;
            Item.height = 80;
            Item.damage = 4200;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 32;
            Item.useAnimation = 32;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 12f;
            Item.value = Item.sellPrice(platinum: 2, gold: 50);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
            Item.UseSound = SoundID.Item71 with { Pitch = -0.3f };
            Item.autoReuse = true;
            Item.useTurn = false;
            Item.scale = 1.8f;
            Item.crit = 25;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<ExecutionersVerdictSwing>();
            Item.shootSpeed = 1f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var vPlayer = player.ExecutionersVerdict();
            int comboStep = vPlayer.ComboStep;

            // Spawn swing projectile with combo phase index
            Projectile.NewProjectile(
                source,
                player.MountedCenter,
                Vector2.Zero,
                ModContent.ProjectileType<ExecutionersVerdictSwing>(),
                damage,
                knockback,
                player.whoAmI,
                ai0: 0f, // Timer
                ai1: comboStep); // Combo phase

            // Spawn 3 Verdict Bolts during swing
            Vector2 mouseDir = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot with { Pitch = -0.2f - comboStep * 0.1f }, player.Center);

            for (int i = 0; i < 3; i++)
            {
                float angle = mouseDir.ToRotation() + MathHelper.ToRadians(-30f + i * 30f);
                Vector2 spawnPos = player.Center + angle.ToRotationVector2() * 50f;
                Vector2 boltVel = angle.ToRotationVector2() * 10f;

                Projectile.NewProjectile(
                    source,
                    spawnPos,
                    boltVel,
                    ModContent.ProjectileType<VerdictBolt>(),
                    damage / 2,
                    knockback / 2,
                    player.whoAmI);
            }

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Each swing launches 3 judgment bolts that track enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Bolts explode into spectral sword strikes on impact"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Executes non-boss enemies below 15% health instantly"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Deals 50% more damage to enemies below 30% health"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "3-phase combo builds to a devastating guillotine drop"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The final sentence is always death'")
            {
                OverrideColor = new Color(200, 50, 30)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfDiesIrae>(), 25)
                .AddIngredient(ModContent.ItemType<DiesIraeResonantEnergy>(), 20)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfDiesIrae>(), 3)
                .AddIngredient(ItemID.LunarBar, 20)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }
}
