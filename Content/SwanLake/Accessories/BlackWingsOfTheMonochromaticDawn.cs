using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.SwanLake.ResonantOres;
using MagnumOpus.Content.SwanLake.ResonanceEnergies;
using MagnumOpus.Content.SwanLake.HarmonicCores;
using MagnumOpus.Content.SwanLake.Debuffs;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.SwanLake.Accessories
{
    /// <summary>
    /// Black Wings of the Monochromatic Dawn - Summoner Accessory
    /// Ethereal wings that empower minions with the dual nature of swans.
    /// 
    /// WHITE MODE (Protective):
    /// - Minions create protective white shields around player
    /// - Damage reduction when minions are nearby (up to 25%)
    /// - Minions prioritize protecting the player
    /// 
    /// BLACK MODE (Aggressive):
    /// - +35% minion damage
    /// - -20% defense (glass cannon trade-off)
    /// - Minions apply Flame of the Swan (10% damage vulnerability)
    /// 
    /// Right-click while in inventory to toggle modes.
    /// Note: Charge effects cannot be activated while inventory or other screens are open.
    /// </summary>
    public class BlackWingsOfTheMonochromaticDawn : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 3);
            Item.rare = ModContent.RarityType<SwanRainbowRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<SwanLakeAccessoryPlayer>();
            modPlayer.hasBlackWings = true;

            // Black mode: -20% defense trade-off
            if (modPlayer.wingsIsBlackMode)
            {
                player.statDefense -= (int)(player.statDefense * 0.2f);
            }
        }

        public override bool CanRightClick()
        {
            return true;
        }

        public override void RightClick(Player player)
        {
            var modPlayer = player.GetModPlayer<SwanLakeAccessoryPlayer>();
            modPlayer.ToggleWingsMode();
            Item.stack++;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var modPlayer = player.GetModPlayer<SwanLakeAccessoryPlayer>();
            
            string modeText = modPlayer.wingsIsBlackMode ? "BLACK SWAN (Aggressive)" : "WHITE SWAN (Protective)";
            Color modeColor = modPlayer.wingsIsBlackMode ? new Color(30, 30, 40) : new Color(240, 245, 255);
            
            tooltips.Add(new TooltipLine(Mod, "CurrentMode", $"Current Mode: {modeText}")
            {
                OverrideColor = modeColor
            });
            
            tooltips.Add(new TooltipLine(Mod, "ToggleHint", "[Right-click to toggle mode]")
            {
                OverrideColor = new Color(180, 180, 200)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer1", " "));
            
            // White mode tooltip
            Color whiteColor = modPlayer.wingsIsBlackMode ? new Color(100, 100, 110) : new Color(240, 245, 255);
            tooltips.Add(new TooltipLine(Mod, "WhiteHeader", "White Swan (Protective):")
            {
                OverrideColor = whiteColor
            });
            tooltips.Add(new TooltipLine(Mod, "WhiteEffect1", "  Minions create protective white shields")
            {
                OverrideColor = whiteColor
            });
            tooltips.Add(new TooltipLine(Mod, "WhiteEffect2", "  Up to 25% damage reduction when minions nearby")
            {
                OverrideColor = whiteColor
            });
            tooltips.Add(new TooltipLine(Mod, "WhiteEffect3", "  (5% per minion within range)")
            {
                OverrideColor = whiteColor
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer2", " "));
            
            // Black mode tooltip
            Color blackColor = modPlayer.wingsIsBlackMode ? new Color(200, 180, 220) : new Color(80, 80, 90);
            tooltips.Add(new TooltipLine(Mod, "BlackHeader", "Black Swan (Aggressive):")
            {
                OverrideColor = blackColor
            });
            tooltips.Add(new TooltipLine(Mod, "BlackEffect1", "  +35% minion damage")
            {
                OverrideColor = blackColor
            });
            tooltips.Add(new TooltipLine(Mod, "BlackEffect2", "  -20% defense (glass cannon)")
            {
                OverrideColor = new Color(255, 150, 100)
            });
            tooltips.Add(new TooltipLine(Mod, "BlackEffect3", "  Minions apply Flame of the Swan (3s)")
            {
                OverrideColor = blackColor
            });
            tooltips.Add(new TooltipLine(Mod, "BlackEffect4", "  Enemies take 10% more damage")
            {
                OverrideColor = new Color(255, 180, 180)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer3", " "));
            
            tooltips.Add(new TooltipLine(Mod, "Flavor", "'At dawn, when darkness meets light, the monochromatic wings spread - neither fully black nor white, but eternally both'")
            {
                OverrideColor = new Color(150, 140, 170)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SwansResonanceEnergy>(), 5)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfSwanLake>(), 5)
                .AddIngredient(ModContent.ItemType<RemnantOfSwansHarmony>(), 5)
                .AddIngredient(ModContent.ItemType<ShardOfTheFeatheredTempo>(), 5)
                .AddIngredient(ItemID.SoulofNight, 5)
                .AddIngredient(ItemID.SoulofFlight, 15)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }

    /// <summary>
    /// GlobalProjectile to handle Black Wings minion effects.
    /// Applies Flame of the Swan in black mode.
    /// Note: Charge effects cannot be activated while inventory or other screens are open.
    /// </summary>
    public class BlackWingsMinionEffects : GlobalProjectile
    {
        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Check for minions, sentries, and summon damage type projectiles
            bool isSummonProjectile = projectile.minion || projectile.sentry || 
                                       projectile.DamageType == DamageClass.Summon ||
                                       projectile.DamageType.CountsAsClass(DamageClass.Summon);
            
            if (!isSummonProjectile)
                return;

            if (projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
                return;

            Player player = Main.player[projectile.owner];
            if (player == null || !player.active)
                return;
                
            var swanPlayer = player.GetModPlayer<SwanLakeAccessoryPlayer>();
            
            if (!swanPlayer.hasBlackWings)
                return;

            if (swanPlayer.wingsIsBlackMode)
            {
                // Vivid black and white flame effect on hit
                for (int i = 0; i < 12; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(2f, 5f);
                    
                    // Alternating black and white flames
                    if (i % 2 == 0)
                    {
                        Dust black = Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(15f, 15f),
                            DustID.Smoke, vel, 220, Color.Black, 1.6f);
                        black.noGravity = true;
                    }
                    else
                    {
                        Dust white = Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(15f, 15f),
                            DustID.WhiteTorch, vel * 0.8f, 80, default, 1.2f);
                        white.noGravity = true;
                    }
                }
                
                // Pearlescent shimmer accents with GRADIENT
                for (int i = 0; i < 4; i++)
                {
                    // GRADIENT: Black ↁEWhite with rainbow shimmer
                    float progress = (float)i / 4f;
                    Color baseColor = Color.Lerp(new Color(20, 20, 30), Color.White, progress);
                    float hue = (Main.GameUpdateCount * 0.015f + progress * 0.5f) % 1f;
                    Color rainbow = Main.hslToRgb(hue, 0.4f, 0.9f);
                    Color pearlescent = Color.Lerp(baseColor, rainbow, 0.25f);
                    Dust shimmer = Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(20f, 20f),
                        DustID.TintableDustLighted, Main.rand.NextVector2Circular(2f, 2f), 0, pearlescent, 0.9f);
                    shimmer.noGravity = true;
                }
                
                // Apply Flame of the Swan (3 seconds = 180 ticks)
                target.AddBuff(ModContent.BuffType<FlameOfTheSwan>(), 180);
            }
            else
            {
                // White protective shield shimmer
                if (Main.rand.NextBool(3))
                {
                    for (int i = 0; i < 5; i++)
                    {
                        Dust shield = Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                            DustID.WhiteTorch, Main.rand.NextVector2Circular(1f, 1f), 100, default, 0.9f);
                        shield.noGravity = true;
                    }
                }
            }
        }
    }
}
