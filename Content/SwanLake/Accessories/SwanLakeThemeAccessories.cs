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
    /// Phase 3 Swan Lake Tier 1 Accessory - Post-Moon Lord
    /// +10% all damage
    /// +15% movement speed
    /// Dodging leaves rainbow afterimages
    /// Graceful feathers drift around player
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
            // +10% all damage
            player.GetDamage(DamageClass.Generic) += 0.10f;
            
            // +15% movement speed
            player.moveSpeed += 0.15f;
            player.runAcceleration *= 1.15f;
            
            // Enable rainbow afterimage mechanic
            player.GetModPlayer<PlumeOfElegancePlayer>().plumeEquipped = true;
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
            tooltips.Add(new TooltipLine(Mod, "DamageBoost", "+10% damage")
            {
                OverrideColor = SwanColors.White
            });
            tooltips.Add(new TooltipLine(Mod, "SpeedBoost", "+15% movement speed")
            {
                OverrideColor = SwanColors.IcyBlue
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

        public override void PostUpdate()
        {
            if (!plumeEquipped) return;
        }
    }
    #endregion

    #region Swan's Chromatic Diadem
    /// <summary>
    /// Phase 3 Swan Lake Tier 2 Accessory - Post-Moon Lord (Combination)
    /// Includes all Plume of Elegance benefits maximized:
    /// +16% all damage
    /// +20% movement speed
    /// +10% dodge chance
    /// Perfect dodges trigger "Dying Swan" - a massive prismatic burst that damages nearby enemies
    /// Constant prismatic aura with drifting feathers
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
            
            // +20% movement speed
            player.moveSpeed += 0.20f;
            player.runAcceleration *= 1.20f;
            
            // +10% dodge chance (through Brain of Confusion-like effect)
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
            tooltips.Add(new TooltipLine(Mod, "SpeedBoost", "+20% movement speed")
            {
                OverrideColor = SwanColors.IcyBlue
            });
            tooltips.Add(new TooltipLine(Mod, "DodgeChance", "+10% dodge chance")
            {
                OverrideColor = SwanColors.GetRainbow(0.3f)
            });
            tooltips.Add(new TooltipLine(Mod, "DyingSwan", "Perfect dodges trigger 'Dying Swan' - prismatic damage burst")
            {
                OverrideColor = SwanColors.GetRainbow(0.6f)
            });
            tooltips.Add(new TooltipLine(Mod, "Flavor", "'The final dance of the dying swan - beautiful, tragic, eternal'")
            {
                OverrideColor = SwanColors.Silver
            });
        }
    }

    public class SwansChromaticDiademPlayer : ModPlayer
    {
        public bool diademEquipped;
        private int dodgeCooldown;

        public override void ResetEffects()
        {
            diademEquipped = false;
        }

        public override void PostUpdate()
        {
            if (!diademEquipped) return;
            
            if (dodgeCooldown > 0)
                dodgeCooldown--;
        }

        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (!diademEquipped) return false;
            if (dodgeCooldown > 0) return false;
            
            // 10% dodge chance
            if (Main.rand.NextFloat() < 0.10f)
            {
                dodgeCooldown = 120; // 2 second cooldown
                TriggerDyingSwanBurst();
                return true;
            }
            
            return false;
        }

        private void TriggerDyingSwanBurst()
        {
            // "DYING SWAN" - Massive prismatic explosion
            
            // Phase 1: Central white flash
            
            // Phase 2: Rainbow core burst
            
            // Phase 3: Cascading prismatic halos
            
            // Phase 4: Black and white contrast rings
            
            // Phase 5: Feather explosion
            
            // Phase 6: Rainbow sparkle spiral
            for (int layer = 0; layer < 3; layer++)
            {
                for (int i = 0; i < 10; i++)
                {
                    float angle = MathHelper.TwoPi * i / 10f + layer * 0.2f;
                    float radius = 40f + layer * 25f;
                    Vector2 sparklePos = Player.Center + angle.ToRotationVector2() * radius;
                    Color sparkleColor = SwanColors.GetRainbow((float)i / 10f + layer * 0.33f);
                    
                }
            }
            
            // Phase 7: Particle explosion
            
            // Deal damage to nearby enemies (base 100 + 10% of player's max damage stat)
            if (Main.myPlayer == Player.whoAmI)
            {
                int baseDamage = 100 + (int)(Player.GetTotalDamage(DamageClass.Generic).ApplyTo(100) * 0.5f);
                float damageRadius = 250f;
                
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && !npc.immortal && !npc.dontTakeDamage)
                    {
                        float distance = Vector2.Distance(npc.Center, Player.Center);
                        if (distance <= damageRadius)
                        {
                            // Damage falls off with distance
                            float damageMult = 1f - (distance / damageRadius) * 0.5f;
                            int finalDamage = (int)(baseDamage * damageMult);
                            
                            npc.SimpleStrikeNPC(finalDamage, 0, false, 0, null, false, 0, true);
                            
                            // VFX on hit
                            
                            // Feather burst on enemy
                        }
                    }
                }
            }
            
            // Brief invincibility frames
            Player.immune = true;
            Player.immuneTime = 30;
            
            // Screen shake
            
            // Sound effect
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.5f, Volume = 1.2f }, Player.Center);
        }
    }
    #endregion
}
