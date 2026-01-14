using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
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
    /// Crown of the Swan - Mage Accessory
    /// A regal crown that channels the dual nature of swan magic.
    /// 
    /// WHITE MODE (Efficiency):
    /// - -20% mana cost
    /// - Protective wisps orbit the player (up to 5)
    /// - Wisps absorb damage and break when hit
    /// 
    /// BLACK MODE (Power):
    /// - +30% magic damage
    /// - +15% mana cost
    /// - Spells apply Flame of the Swan (10% damage vulnerability, 5s duration, 2s cooldown)
    /// 
    /// Right-click while in inventory to toggle modes.
    /// </summary>
    public class CrownOfTheSwan : ModItem
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
            modPlayer.hasCrownOfTheSwan = true;

            // Visual effects
            if (!hideVisual)
            {
                // Crown glow above head
                if (Main.rand.NextBool(6))
                {
                    Vector2 crownPos = player.Top + new Vector2(Main.rand.NextFloat(-8f, 8f), -10f);
                    
                    if (modPlayer.crownIsBlackMode)
                    {
                        Dust black = Dust.NewDustPerfect(crownPos, DustID.Smoke,
                            new Vector2(0, -1f), 200, Color.Black, 1f);
                        black.noGravity = true;
                    }
                    else
                    {
                        Dust white = Dust.NewDustPerfect(crownPos, DustID.WhiteTorch,
                            new Vector2(0, -0.8f), 100, default, 0.9f);
                        white.noGravity = true;
                    }
                }

                // Pearlescent crown sparkle
                if (Main.rand.NextBool(12))
                {
                    Color pearlescent = Main.rand.Next(3) switch
                    {
                        0 => new Color(255, 240, 245),
                        1 => new Color(240, 245, 255),
                        _ => new Color(250, 255, 245)
                    };
                    Vector2 sparklePos = player.Top + new Vector2(Main.rand.NextFloat(-12f, 12f), Main.rand.NextFloat(-15f, -5f));
                    Dust sparkle = Dust.NewDustPerfect(sparklePos, DustID.TintableDustLighted,
                        Vector2.Zero, 0, pearlescent, 0.8f);
                    sparkle.noGravity = true;
                }
                
                // Swan feathers floating from crown
                if (Main.rand.NextBool(15))
                {
                    Color featherColor = modPlayer.crownIsBlackMode ? new Color(30, 30, 35) : Color.White;
                    CustomParticles.SwanFeatherDrift(player.Top + new Vector2(Main.rand.NextFloat(-10f, 10f), -5f), featherColor, 0.25f);
                }

                // Orbiting wisp visuals (white mode only) - BIGGER AND MORE RAINBOW
                if (!modPlayer.crownIsBlackMode && modPlayer.protectiveWispCount > 0)
                {
                    float baseAngle = Main.GameUpdateCount * 0.03f;
                    for (int i = 0; i < modPlayer.protectiveWispCount; i++)
                    {
                        float angle = baseAngle + (MathHelper.TwoPi * i / modPlayer.protectiveWispCount);
                        Vector2 wispPos = player.Center + new Vector2((float)Math.Cos(angle) * 55f, (float)Math.Sin(angle) * 38f - 10f);
                        
                        // Core crystal dust - alternating black and white
                        bool isBlackCrystal = i % 2 == 0;
                        
                        // === BIGGER CORE PARTICLES ===
                        // Main wisp core - larger size
                        if (isBlackCrystal)
                        {
                            // Black crystal core - BIGGER
                            Dust blackCore = Dust.NewDustPerfect(wispPos, DustID.Smoke,
                                Main.rand.NextVector2Circular(0.4f, 0.4f), 200, Color.Black, 2.2f);
                            blackCore.noGravity = true;
                            
                            // White glow outline - larger
                            Dust whiteGlow = Dust.NewDustPerfect(wispPos, DustID.WhiteTorch,
                                Vector2.Zero, 100, default, 1.4f);
                            whiteGlow.noGravity = true;
                        }
                        else
                        {
                            // White crystal core - BIGGER
                            Dust whiteCore = Dust.NewDustPerfect(wispPos, DustID.WhiteTorch,
                                Main.rand.NextVector2Circular(0.4f, 0.4f), 80, default, 2.2f);
                            whiteCore.noGravity = true;
                            
                            // Black glow outline
                            Dust blackGlow = Dust.NewDustPerfect(wispPos, DustID.Smoke,
                                Vector2.Zero, 200, Color.Black, 1.0f);
                            blackGlow.noGravity = true;
                        }
                        
                        // === RAINBOW AURA AROUND EACH WISP ===
                        float rainbowHue = (Main.GameUpdateCount * 0.02f + (float)i / modPlayer.protectiveWispCount) % 1f;
                        Color rainbowColor = Main.hslToRgb(rainbowHue, 0.85f, 0.7f);
                        
                        // Rainbow ring particles orbiting each wisp
                        for (int j = 0; j < 3; j++)
                        {
                            float ringAngle = Main.GameUpdateCount * 0.08f + j * MathHelper.TwoPi / 3f;
                            float ringRadius = 8f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 2f;
                            Vector2 ringPos = wispPos + new Vector2((float)Math.Cos(ringAngle), (float)Math.Sin(ringAngle)) * ringRadius;
                            
                            float particleHue = (rainbowHue + j * 0.33f) % 1f;
                            Color ringColor = Main.hslToRgb(particleHue, 0.9f, 0.75f);
                            
                            Dust ring = Dust.NewDustPerfect(ringPos, DustID.RainbowTorch,
                                Vector2.Zero, 0, ringColor, 0.8f);
                            ring.noGravity = true;
                        }
                        
                        // === RAINBOW SHIMMER TRAIL ===
                        if (Main.rand.NextBool(2))
                        {
                            Vector2 trailPos = wispPos + new Vector2((float)Math.Cos(angle - 0.25f) * 10f, (float)Math.Sin(angle - 0.25f) * 6f);
                            float trailHue = (rainbowHue + 0.5f) % 1f;
                            Color trailColor = Color.Lerp(isBlackCrystal ? new Color(80, 80, 90) : new Color(240, 245, 255), 
                                Main.hslToRgb(trailHue, 0.8f, 0.65f), 0.6f);
                            
                            Dust trail = Dust.NewDustPerfect(trailPos, DustID.TintableDustLighted,
                                Vector2.Zero, 0, trailColor, 1.0f);
                            trail.noGravity = true;
                        }
                        
                        // === SPARKLE PARTICLES ===
                        if (Main.rand.NextBool(4))
                        {
                            Vector2 sparklePos = wispPos + Main.rand.NextVector2Circular(12f, 12f);
                            float sparkleHue = Main.rand.NextFloat();
                            Color sparkleColor = Main.hslToRgb(sparkleHue, 1f, 0.8f);
                            
                            Dust sparkle = Dust.NewDustPerfect(sparklePos, DustID.RainbowTorch,
                                Main.rand.NextVector2Circular(0.5f, 0.5f), 0, sparkleColor, 0.6f);
                            sparkle.noGravity = true;
                        }
                        
                        // Add stronger light for visibility with rainbow tint
                        float lightInt = isBlackCrystal ? 0.4f : 0.6f;
                        Color lightCol = rainbowColor * 0.4f;
                        Lighting.AddLight(wispPos, lightInt + lightCol.R / 255f * 0.2f, 
                            lightInt + lightCol.G / 255f * 0.2f, 
                            lightInt + 0.15f + lightCol.B / 255f * 0.2f);
                    }
                }

                // Black flame aura (black mode only)
                if (modPlayer.crownIsBlackMode && Main.rand.NextBool(5))
                {
                    Vector2 offset = Main.rand.NextVector2Circular(25f, 30f);
                    Dust aura = Dust.NewDustPerfect(player.Center + offset, DustID.Smoke,
                        new Vector2(0, -1.5f), 220, Color.Black, 1.3f);
                    aura.noGravity = true;
                }
            }
        }

        public override bool CanRightClick()
        {
            return true;
        }

        public override void RightClick(Player player)
        {
            var modPlayer = player.GetModPlayer<SwanLakeAccessoryPlayer>();
            modPlayer.ToggleCrownMode();
            Item.stack++;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var modPlayer = player.GetModPlayer<SwanLakeAccessoryPlayer>();
            
            string modeText = modPlayer.crownIsBlackMode ? "BLACK SWAN (Power)" : "WHITE SWAN (Efficiency)";
            Color modeColor = modPlayer.crownIsBlackMode ? new Color(30, 30, 40) : new Color(240, 245, 255);
            
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
            Color whiteColor = modPlayer.crownIsBlackMode ? new Color(100, 100, 110) : new Color(240, 245, 255);
            tooltips.Add(new TooltipLine(Mod, "WhiteHeader", "White Swan (Efficiency):")
            {
                OverrideColor = whiteColor
            });
            tooltips.Add(new TooltipLine(Mod, "WhiteEffect1", "  -20% mana cost")
            {
                OverrideColor = whiteColor
            });
            tooltips.Add(new TooltipLine(Mod, "WhiteEffect2", "  Protective wisps orbit you (up to 5)")
            {
                OverrideColor = whiteColor
            });
            tooltips.Add(new TooltipLine(Mod, "WhiteEffect3", "  Wisps absorb incoming damage")
            {
                OverrideColor = whiteColor
            });
            
            // Show wisp count if in white mode
            if (!modPlayer.crownIsBlackMode)
            {
                tooltips.Add(new TooltipLine(Mod, "WispCount", $"  Current Wisps: {modPlayer.protectiveWispCount}/5")
                {
                    OverrideColor = new Color(150, 200, 255)
                });
            }
            
            tooltips.Add(new TooltipLine(Mod, "Spacer2", " "));
            
            // Black mode tooltip
            Color blackColor = modPlayer.crownIsBlackMode ? new Color(200, 180, 220) : new Color(80, 80, 90);
            tooltips.Add(new TooltipLine(Mod, "BlackHeader", "Black Swan (Power):")
            {
                OverrideColor = blackColor
            });
            tooltips.Add(new TooltipLine(Mod, "BlackEffect1", "  +30% magic damage")
            {
                OverrideColor = blackColor
            });
            tooltips.Add(new TooltipLine(Mod, "BlackEffect2", "  +15% mana cost")
            {
                OverrideColor = blackColor
            });
            tooltips.Add(new TooltipLine(Mod, "BlackEffect3", "  Spells apply Flame of the Swan (5s)")
            {
                OverrideColor = blackColor
            });
            tooltips.Add(new TooltipLine(Mod, "BlackEffect4", "  Enemies take 10% more damage")
            {
                OverrideColor = new Color(255, 180, 180)
            });
            tooltips.Add(new TooltipLine(Mod, "BlackEffect5", "  2 second cooldown between applications")
            {
                OverrideColor = new Color(180, 180, 200)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer3", " "));
            
            tooltips.Add(new TooltipLine(Mod, "Flavor", "'Worn by royalty who understood that true power lies in choice'")
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
                .AddIngredient(ItemID.SoulofLight, 5)
                .AddIngredient(ItemID.SoulofFlight, 6)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }

    /// <summary>
    /// Handles the protective wisp damage absorption for Crown of the Swan.
    /// </summary>
    public class CrownOfTheSwanDamageHandler : ModPlayer
    {
        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            var swanPlayer = Player.GetModPlayer<SwanLakeAccessoryPlayer>();
            
            // White mode wisps absorb damage
            if (swanPlayer.hasCrownOfTheSwan && !swanPlayer.crownIsBlackMode && swanPlayer.protectiveWispCount > 0)
            {
                // Consume a wisp to reduce damage by 50%
                swanPlayer.protectiveWispCount--;
                modifiers.SourceDamage *= 0.5f;
                
                // Wisp break visual
                float breakAngle = Main.GameUpdateCount * 0.03f + (MathHelper.TwoPi * swanPlayer.protectiveWispCount / 5f);
                Vector2 wispPos = Player.Center + new Vector2((float)Math.Cos(breakAngle) * 45f, (float)Math.Sin(breakAngle) * 30f - 10f);
                
                for (int i = 0; i < 20; i++)
                {
                    Dust burst = Dust.NewDustPerfect(wispPos, DustID.WhiteTorch,
                        Main.rand.NextVector2Circular(4f, 4f), 100, default, 1.5f);
                    burst.noGravity = true;
                }
                
                // Pearlescent burst
                for (int i = 0; i < 10; i++)
                {
                    Color pearl = Main.rand.Next(3) switch
                    {
                        0 => new Color(255, 240, 245),
                        1 => new Color(240, 245, 255),
                        _ => new Color(250, 255, 245)
                    };
                    Dust shimmer = Dust.NewDustPerfect(wispPos + Main.rand.NextVector2Circular(15f, 15f),
                        DustID.TintableDustLighted, Main.rand.NextVector2Circular(3f, 3f), 0, pearl, 1f);
                    shimmer.noGravity = true;
                }
                
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item27 with { Pitch = 0.5f }, wispPos);
                Main.NewText($"Protective Wisp absorbed damage! ({swanPlayer.protectiveWispCount}/5 remaining)", new Color(200, 220, 255));
            }
        }
    }
}
