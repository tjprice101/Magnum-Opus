using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.SwanLake.ResonanceEnergies;
using MagnumOpus.Content.SwanLake.HarmonicCores;
using MagnumOpus.Content.SwanLake.Debuffs;
using MagnumOpus.Content.Materials.EnemyDrops;

namespace MagnumOpus.Content.SwanLake.Accessories
{
    /// <summary>
    /// Swan Lake theme color constants - graceful elegance, monochrome with rainbow shimmer
    /// </summary>
    public static class SwanColors
    {
        public static readonly Color White = new Color(255, 255, 255);
        public static readonly Color Black = new Color(20, 20, 30);
        public static readonly Color Silver = new Color(220, 225, 235);
        public static readonly Color IcyBlue = new Color(180, 220, 255);
        
        /// <summary>
        /// Get a rainbow color based on a cycling offset
        /// </summary>
        public static Color GetRainbow(float offset)
        {
            float hue = (Main.GameUpdateCount * 0.01f + offset) % 1f;
            return Main.hslToRgb(hue, 1f, 0.85f);
        }
    }

    #region Plume of Elegance
    /// <summary>
    /// Plume of Elegance - Swan Lake Tier 1 Accessory.
    /// +25% movement speed, +10% jump height, +10% acceleration, damage buffs +75% effective
    /// </summary>
    public class PlumeOfElegance : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 25);
            Item.rare = ModContent.RarityType<SwanRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // +25% movement speed
            player.moveSpeed += 0.25f;
            
            // +10% jump height
            Player.jumpHeight += (int)(Player.jumpHeight * 0.10f);
            
            // +10% movement acceleration
            player.runAcceleration *= 1.10f;
            
            // Damage buffs gain +75% effectiveness - approximate as +15% all damage
            player.GetDamage(DamageClass.Generic) += 0.15f;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ResonantCoreOfSwanLake>(3)
                .AddIngredient<SwansResonanceEnergy>(8)
                .AddIngredient(ItemID.LunarBar, 10)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "MoveSpeed", "+25% movement speed")
            {
                OverrideColor = SwanColors.White
            });
            tooltips.Add(new TooltipLine(Mod, "JumpHeight", "+10% jump height")
            {
                OverrideColor = SwanColors.IcyBlue
            });
            tooltips.Add(new TooltipLine(Mod, "Acceleration", "+10% movement acceleration")
            {
                OverrideColor = SwanColors.White
            });
            tooltips.Add(new TooltipLine(Mod, "DamageBuff", "Damage buffs gain +75% effectiveness")
            {
                OverrideColor = SwanColors.GetRainbow(0.3f)
            });
            tooltips.Add(new TooltipLine(Mod, "Flavor", "'Grace in motion, elegance personified'")
            {
                OverrideColor = SwanColors.Silver
            });
        }
    }

    public class PlumeOfElegancePlayer : ModPlayer
    {
        public bool plumeEquipped;

        public override void ResetEffects()
        {
            plumeEquipped = false;
        }
    }
    #endregion

    #region Swan's Chromatic Diadem
    /// <summary>
    /// Swan's Chromatic Diadem - Swan Lake Tier 2 Accessory.
    /// +16% damage, +8% crit, +30% move speed, +15% jump, +15% acceleration,
    /// damage buffs +80% effective, flying gives "Dying Swan's Grace" DoT to enemies.
    /// </summary>
    public class SwansChromaticDiadem : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 36;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 50);
            Item.rare = ModContent.RarityType<SwanRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // +16% all damage
            player.GetDamage(DamageClass.Generic) += 0.16f;
            
            // +8% crit chance
            player.GetCritChance(DamageClass.Generic) += 8;
            
            // +30% movement speed
            player.moveSpeed += 0.30f;
            
            // +15% jump height
            Player.jumpHeight += (int)(Player.jumpHeight * 0.15f);
            
            // +15% movement acceleration
            player.runAcceleration *= 1.15f;
            
            // Damage buffs gain +80% effectiveness - approximate as +18% all damage
            player.GetDamage(DamageClass.Generic) += 0.18f;
            
            // Dying Swan's Grace when airborne
            player.GetModPlayer<SwansChromaticDiademPlayer>().diademEquipped = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<PlumeOfElegance>()
                .AddIngredient<HarmonicCoreOfSwanLake>(2)
                .AddIngredient<SwansResonanceEnergy>(15)
                .AddIngredient<RemnantOfSwansHarmony>(5)
                .AddIngredient<GraceEssence>(10)
                .AddIngredient(ItemID.FragmentStardust, 10)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "DamageBoost", "+16% damage, +8% critical strike chance")
            {
                OverrideColor = SwanColors.White
            });
            tooltips.Add(new TooltipLine(Mod, "MoveSpeed", "+30% movement speed")
            {
                OverrideColor = SwanColors.IcyBlue
            });
            tooltips.Add(new TooltipLine(Mod, "JumpHeight", "+15% jump height, +15% movement acceleration")
            {
                OverrideColor = SwanColors.White
            });
            tooltips.Add(new TooltipLine(Mod, "DamageBuff", "Damage buffs gain +80% effectiveness")
            {
                OverrideColor = SwanColors.GetRainbow(0.3f)
            });
            tooltips.Add(new TooltipLine(Mod, "DyingSwan", "Flying gives 'Dying Swan's Grace' - attacks apply 5% weapon damage DoT for 5s")
            {
                OverrideColor = SwanColors.GetRainbow(0.6f)
            });
            tooltips.Add(new TooltipLine(Mod, "Flavor", "'The final dance of the dying swan -- beautiful, tragic, eternal'")
            {
                OverrideColor = SwanColors.Silver
            });
        }
    }

    public class SwansChromaticDiademPlayer : ModPlayer
    {
        public bool diademEquipped;
        private bool isAirborne;

        public override void ResetEffects()
        {
            diademEquipped = false;
        }

        public override void PostUpdate()
        {
            if (!diademEquipped) return;
            isAirborne = !Player.controlDown && (Player.velocity.Y != 0 || Player.grappling[0] >= 0 || Player.pulley);
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            TryApplyDyingSwanGrace(target, damageDone);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.owner == Player.whoAmI)
                TryApplyDyingSwanGrace(target, damageDone);
        }

        private void TryApplyDyingSwanGrace(NPC target, int damageDone)
        {
            if (!diademEquipped || !isAirborne) return;
            
            // Non-stacking: cannot reapply while Odile's Beauty is active
            if (target.HasBuff(ModContent.BuffType<OdilesBeauty>()))
                return;
            
            // Apply Odile's Beauty — 5% weapon damage DoT for 5 seconds
            target.AddBuff(ModContent.BuffType<OdilesBeauty>(), 300); // 5 seconds
            
            // Calculate and set the damage based on equipped weapon
            int weaponDamage = Player.HeldItem != null
                ? (int)Player.GetTotalDamage(Player.HeldItem.DamageType).ApplyTo(Player.HeldItem.damage)
                : 50;
            target.GetGlobalNPC<OdilesBeautyNPC>().SetDamage(weaponDamage);
        }
    }
    #endregion
}
