using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
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
    /// Pendant of the Two Swans - Melee Accessory
    /// A dual-natured pendant embodying both Odette (White Swan) and Odile (Black Swan).
    /// 
    /// WHITE MODE (Odette - Defensive):
    /// - On dodge, creates a monochromatic (white &amp; black) halo around you
    /// - Reduces incoming damage by 20% for 30 seconds (3 minute cooldown)
    /// - Dodges the triggering hit
    /// 
    /// BLACK MODE (Odile - Offensive):
    /// - Critical hits unleash vivid black &amp; white flares with pearlescent rainbow explosion
    /// - +25% critical strike damage
    /// - Applies Flame of the Swan (10% damage vulnerability)
    /// 
    /// Right-click while in inventory to toggle modes.
    /// </summary>
    public class PendantOfTheTwoSwans : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 3);
            Item.rare = ModContent.RarityType<SwanRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<SwanLakeAccessoryPlayer>();
            modPlayer.hasPendantOfTheTwoSwans = true;

            // Ambient particles based on mode
            if (!hideVisual)
            {
                // Pearlescent shimmer (both modes)
                if (Main.rand.NextBool(8))
                {
                    Color pearlescent = Main.rand.Next(3) switch
                    {
                        0 => new Color(255, 240, 245),
                        1 => new Color(240, 245, 255),
                        _ => new Color(250, 255, 245)
                    };
                    Vector2 offset = Main.rand.NextVector2Circular(20f, 25f);
                    Dust shimmer = Dust.NewDustPerfect(player.Center + offset, DustID.TintableDustLighted,
                        new Vector2(0, -0.5f), 0, pearlescent, 0.7f);
                    shimmer.noGravity = true;
                }
                
                // Swan feather aura around pendant
                if (Main.rand.NextBool(12))
                {
                    Color featherColor = modPlayer.pendantIsBlackMode ? new Color(30, 30, 35) : Color.White;
                    CustomParticles.SwanFeatherAura(player.Center, 25f, 1);
                }

                // Mode-specific particles
                if (Main.rand.NextBool(6))
                {
                    if (modPlayer.pendantIsBlackMode)
                    {
                        // Black flame wisps
                        Dust black = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(15f, 20f),
                            DustID.Smoke, new Vector2(0, -1.5f), 200, Color.Black, 1.2f);
                        black.noGravity = true;
                    }
                    else
                    {
                        // White flame wisps
                        Dust white = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(15f, 20f),
                            DustID.WhiteTorch, new Vector2(0, -1.2f), 100, default, 1.1f);
                        white.noGravity = true;
                    }
                }
            }
        }

        public override bool CanRightClick()
        {
            return true; // Allow right-click to toggle mode
        }

        public override void RightClick(Player player)
        {
            var modPlayer = player.GetModPlayer<SwanLakeAccessoryPlayer>();
            modPlayer.TogglePendantMode();
            
            // Cancel the right-click consumption - we don't want to consume the item
            Item.stack++;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var modPlayer = player.GetModPlayer<SwanLakeAccessoryPlayer>();
            
            // Current mode indicator
            string modeText = modPlayer.pendantIsBlackMode ? "BLACK SWAN (Odile)" : "WHITE SWAN (Odette)";
            Color modeColor = modPlayer.pendantIsBlackMode ? new Color(30, 30, 40) : new Color(240, 245, 255);
            
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
            Color whiteColor = modPlayer.pendantIsBlackMode ? new Color(100, 100, 110) : new Color(240, 245, 255);
            tooltips.Add(new TooltipLine(Mod, "WhiteHeader", "White Swan (Odette):")
            {
                OverrideColor = whiteColor
            });
            tooltips.Add(new TooltipLine(Mod, "WhiteEffect1", "  On dodge, creates monochromatic halo")
            {
                OverrideColor = whiteColor
            });
            tooltips.Add(new TooltipLine(Mod, "WhiteEffect2", "  -20% incoming damage for 30s (3 min cooldown)")
            {
                OverrideColor = whiteColor
            });
            
            // Show halo status if in white mode
            if (!modPlayer.pendantIsBlackMode)
            {
                if (modPlayer.whiteHaloActive)
                {
                    int secondsLeft = modPlayer.whiteHaloTimer / 60;
                    tooltips.Add(new TooltipLine(Mod, "HaloActive", $"  Halo Active: {secondsLeft}s remaining")
                    {
                        OverrideColor = new Color(100, 255, 150)
                    });
                }
                else if (modPlayer.whiteHaloCooldown > 0)
                {
                    int secondsLeft = modPlayer.whiteHaloCooldown / 60;
                    tooltips.Add(new TooltipLine(Mod, "HaloCooldown", $"  Halo on cooldown: {secondsLeft}s")
                    {
                        OverrideColor = new Color(255, 150, 100)
                    });
                }
                else
                {
                    tooltips.Add(new TooltipLine(Mod, "HaloReady", "  Halo ready!")
                    {
                        OverrideColor = new Color(100, 255, 100)
                    });
                }
            }
            
            tooltips.Add(new TooltipLine(Mod, "Spacer2", " "));
            
            // Black mode tooltip
            Color blackColor = modPlayer.pendantIsBlackMode ? new Color(200, 180, 220) : new Color(80, 80, 90);
            tooltips.Add(new TooltipLine(Mod, "BlackHeader", "Black Swan (Odile):")
            {
                OverrideColor = blackColor
            });
            tooltips.Add(new TooltipLine(Mod, "BlackEffect1", "  Critical hits unleash vivid black & white flares")
            {
                OverrideColor = blackColor
            });
            tooltips.Add(new TooltipLine(Mod, "BlackEffect2", "  with pearlescent rainbow explosion")
            {
                OverrideColor = blackColor
            });
            tooltips.Add(new TooltipLine(Mod, "BlackEffect3", "  +25% critical strike damage")
            {
                OverrideColor = blackColor
            });
            tooltips.Add(new TooltipLine(Mod, "BlackEffect4", "  Applies Flame of the Swan (3s)")
            {
                OverrideColor = blackColor
            });
            tooltips.Add(new TooltipLine(Mod, "BlackEffect5", "  Enemies take 10% more damage")
            {
                OverrideColor = new Color(255, 180, 180)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer3", " "));
            
            tooltips.Add(new TooltipLine(Mod, "Flavor", "'Two souls entwined in eternal dance - light and shadow, love and deception'")
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
                .AddIngredient(ItemID.SoulofMight, 5)
                .AddIngredient(ItemID.SoulofFlight, 10)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }
}
