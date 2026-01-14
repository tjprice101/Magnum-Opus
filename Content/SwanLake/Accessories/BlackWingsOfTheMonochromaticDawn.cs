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
            Item.rare = ModContent.RarityType<SwanRarity>();
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

            // Visual effects
            if (!hideVisual)
            {
                // Wing particles emanating from back
                if (Main.rand.NextBool(4))
                {
                    float wingSpread = 20f;
                    Vector2 leftWing = player.Center + new Vector2(-wingSpread * player.direction, -8f);
                    Vector2 rightWing = player.Center + new Vector2(wingSpread * player.direction, -8f);
                    
                    if (modPlayer.wingsIsBlackMode)
                    {
                        // Black ethereal wing flames
                        Dust leftBlack = Dust.NewDustPerfect(leftWing + Main.rand.NextVector2Circular(5f, 8f),
                            DustID.Smoke, new Vector2(-player.direction * 1f, -1.5f), 220, Color.Black, 1.4f);
                        leftBlack.noGravity = true;
                        
                        Dust rightBlack = Dust.NewDustPerfect(rightWing + Main.rand.NextVector2Circular(5f, 8f),
                            DustID.Smoke, new Vector2(player.direction * 1f, -1.5f), 220, Color.Black, 1.4f);
                        rightBlack.noGravity = true;
                    }
                    else
                    {
                        // White ethereal wing flames
                        Dust leftWhite = Dust.NewDustPerfect(leftWing + Main.rand.NextVector2Circular(5f, 8f),
                            DustID.WhiteTorch, new Vector2(-player.direction * 0.8f, -1.2f), 100, default, 1.2f);
                        leftWhite.noGravity = true;
                        
                        Dust rightWhite = Dust.NewDustPerfect(rightWing + Main.rand.NextVector2Circular(5f, 8f),
                            DustID.WhiteTorch, new Vector2(player.direction * 0.8f, -1.2f), 100, default, 1.2f);
                        rightWhite.noGravity = true;
                    }
                }

                // Pearlescent feather particles falling with GRADIENT shimmer
                if (Main.rand.NextBool(15))
                {
                    // GRADIENT: Black → White with rainbow shimmer overlay
                    float progress = Main.rand.NextFloat();
                    Color baseColor = Color.Lerp(new Color(20, 20, 30), Color.White, progress);
                    // Add rainbow shimmer
                    float hue = (Main.GameUpdateCount * 0.015f + progress * 0.5f) % 1f;
                    Color rainbow = Main.hslToRgb(hue, 0.4f, 0.9f);
                    Color pearlescent = Color.Lerp(baseColor, rainbow, 0.25f);
                    
                    float side = Main.rand.NextBool() ? -1f : 1f;
                    Vector2 featherPos = player.Center + new Vector2(side * 18f * player.direction, -12f + Main.rand.NextFloat(-5f, 5f));
                    
                    Dust feather = Dust.NewDustPerfect(featherPos, DustID.TintableDustLighted,
                        new Vector2(side * 0.3f, 0.8f), 0, pearlescent, 0.9f);
                    feather.noGravity = false;
                    feather.velocity *= 0.4f;
                }
                
                // Swan feather particles from wings
                if (Main.rand.NextBool(10))
                {
                    float side = Main.rand.NextBool() ? -1f : 1f;
                    Color featherColor = modPlayer.wingsIsBlackMode ? new Color(30, 30, 35) : Color.White;
                    CustomParticles.SwanFeatherTrail(player.Center + new Vector2(side * 20f, -10f), new Vector2(side * 0.5f, 0.8f), 0.25f);
                }

                // Mode transition shimmer effect
                float pulseIntensity = (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.5f + 0.5f;
                if (Main.rand.NextBool(10))
                {
                    Color pulseColor = modPlayer.wingsIsBlackMode 
                        ? Color.Lerp(new Color(20, 20, 30), new Color(60, 50, 80), pulseIntensity)
                        : Color.Lerp(new Color(220, 230, 245), new Color(255, 250, 255), pulseIntensity);
                    
                    Dust pulse = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(25f, 20f),
                        DustID.TintableDustLighted, Vector2.Zero, 0, pulseColor, 0.7f);
                    pulse.noGravity = true;
                }

                // Minion connection particles (shows bond between player and minions)
                if (Main.rand.NextBool(20))
                {
                    foreach (Projectile proj in Main.ActiveProjectiles)
                    {
                        if (proj.owner == player.whoAmI && proj.minion && Vector2.Distance(proj.Center, player.Center) < 300f)
                        {
                            // Line of particles connecting player to minion
                            Vector2 direction = (proj.Center - player.Center).SafeNormalize(Vector2.Zero);
                            float distance = Vector2.Distance(proj.Center, player.Center);
                            
                            for (int i = 0; i < 3; i++)
                            {
                                float progress = Main.rand.NextFloat();
                                Vector2 particlePos = player.Center + direction * (distance * progress);
                                
                                int dustType = modPlayer.wingsIsBlackMode ? DustID.Smoke : DustID.WhiteTorch;
                                Color color = modPlayer.wingsIsBlackMode ? Color.Black : default;
                                Dust bond = Dust.NewDustPerfect(particlePos, dustType,
                                    Vector2.Zero, modPlayer.wingsIsBlackMode ? 180 : 100, color, 0.5f);
                                bond.noGravity = true;
                            }
                            break; // Only draw to one minion per update
                        }
                    }
                }
                
                // WHITE MODE - Prominent protective shield visual around player when minions are nearby
                if (!modPlayer.wingsIsBlackMode)
                {
                    // Count nearby minions for shield intensity
                    int nearbyMinions = 0;
                    foreach (Projectile proj in Main.ActiveProjectiles)
                    {
                        if (proj.owner == player.whoAmI && proj.minion && Vector2.Distance(proj.Center, player.Center) < 200f)
                            nearbyMinions++;
                    }
                    
                    if (nearbyMinions > 0)
                    {
                        float shieldIntensity = Math.Min(1f, nearbyMinions * 0.2f); // Up to 5 minions = full intensity
                        float rainbowIntensity = Math.Min(1f, nearbyMinions * 0.15f); // Rainbow increases with minions
                        
                        // === PROMINENT ROTATING SHIELD RING ===
                        float shieldAngle = Main.GameUpdateCount * 0.05f;
                        int ringSegments = 8 + nearbyMinions * 2; // More segments with more minions
                        float baseRadius = 40f;
                        float pulseRadius = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 5f;
                        
                        for (int i = 0; i < ringSegments; i++)
                        {
                            float segmentAngle = shieldAngle + (MathHelper.TwoPi * i / ringSegments);
                            float segmentRadius = baseRadius + pulseRadius;
                            Vector2 shieldPos = player.Center + new Vector2((float)Math.Cos(segmentAngle), (float)Math.Sin(segmentAngle)) * segmentRadius;
                            
                            // Calculate rainbow hue based on segment position and minion count
                            float hueBase = (float)i / ringSegments;
                            float hueCycle = (Main.GameUpdateCount * 0.02f + hueBase) % 1f;
                            Color rainbowColor = Main.hslToRgb(hueCycle, 0.7f + rainbowIntensity * 0.3f, 0.65f + shieldIntensity * 0.2f);
                            
                            // Blend between white and rainbow based on minion count
                            Color shieldColor = Color.Lerp(Color.White, rainbowColor, rainbowIntensity);
                            
                            // Main shield orb - larger and brighter
                            Dust shield = Dust.NewDustPerfect(shieldPos, DustID.RainbowTorch,
                                new Vector2((float)Math.Cos(segmentAngle + MathHelper.PiOver2), (float)Math.Sin(segmentAngle + MathHelper.PiOver2)) * 0.8f,
                                0, shieldColor, 1.3f + shieldIntensity * 0.5f);
                            shield.noGravity = true;
                            
                            // Add white glow behind for prominence
                            if (i % 2 == 0)
                            {
                                Dust glow = Dust.NewDustPerfect(shieldPos, DustID.WhiteTorch,
                                    Vector2.Zero, 50, Color.White, 0.9f + shieldIntensity * 0.3f);
                                glow.noGravity = true;
                            }
                        }
                        
                        // === INNER ROTATING RING - counter-rotating ===
                        float innerAngle = -Main.GameUpdateCount * 0.06f;
                        float innerRadius = 28f + pulseRadius * 0.5f;
                        int innerSegments = 6 + nearbyMinions;
                        
                        for (int i = 0; i < innerSegments; i++)
                        {
                            float segmentAngle = innerAngle + (MathHelper.TwoPi * i / innerSegments);
                            Vector2 innerPos = player.Center + new Vector2((float)Math.Cos(segmentAngle), (float)Math.Sin(segmentAngle)) * innerRadius;
                            
                            // Rainbow with offset hue
                            float hue = ((Main.GameUpdateCount * 0.025f) + (float)i / innerSegments + 0.5f) % 1f;
                            Color innerColor = Color.Lerp(new Color(240, 245, 255), Main.hslToRgb(hue, 0.8f, 0.7f), rainbowIntensity);
                            
                            Dust inner = Dust.NewDustPerfect(innerPos, DustID.TintableDustLighted,
                                new Vector2((float)Math.Cos(segmentAngle - MathHelper.PiOver2), (float)Math.Sin(segmentAngle - MathHelper.PiOver2)) * 0.5f,
                                0, innerColor, 1.0f + shieldIntensity * 0.4f);
                            inner.noGravity = true;
                        }
                        
                        // === MINION CONNECTION BEAMS - rainbow when high minion count ===
                        if (Main.rand.NextBool(2))
                        {
                            int beamMinion = 0;
                            foreach (Projectile proj in Main.ActiveProjectiles)
                            {
                                if (proj.owner == player.whoAmI && proj.minion && Vector2.Distance(proj.Center, player.Center) < 200f)
                                {
                                    Vector2 direction = (proj.Center - player.Center).SafeNormalize(Vector2.Zero);
                                    float distance = Vector2.Distance(proj.Center, player.Center);
                                    
                                    for (int i = 0; i < 4; i++)
                                    {
                                        float progress = i / 4f;
                                        Vector2 particlePos = player.Center + direction * (distance * progress);
                                        
                                        // Rainbow connection beam
                                        float hue = (Main.GameUpdateCount * 0.03f + progress + beamMinion * 0.2f) % 1f;
                                        Color beamColor = Color.Lerp(Color.White, Main.hslToRgb(hue, 0.9f, 0.75f), rainbowIntensity);
                                        
                                        Dust beam = Dust.NewDustPerfect(particlePos, DustID.TintableDustLighted,
                                            Vector2.Zero, 0, beamColor, 0.6f + rainbowIntensity * 0.3f);
                                        beam.noGravity = true;
                                    }
                                    beamMinion++;
                                    if (beamMinion >= 3) break; // Limit beam minions for performance
                                }
                            }
                        }
                        
                        // === AMBIENT RAINBOW SPARKLE FIELD ===
                        if (Main.rand.NextBool(3))
                        {
                            float sparkleAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                            float sparkleRadius = 30f + Main.rand.NextFloat(15f);
                            Vector2 sparklePos = player.Center + new Vector2((float)Math.Cos(sparkleAngle), (float)Math.Sin(sparkleAngle)) * sparkleRadius;
                            
                            float hue = Main.rand.NextFloat();
                            Color sparkleColor = Main.hslToRgb(hue, 0.9f, 0.8f);
                            sparkleColor = Color.Lerp(Color.White, sparkleColor, rainbowIntensity);
                            
                            Dust sparkle = Dust.NewDustPerfect(sparklePos, DustID.RainbowTorch,
                                Vector2.Zero, 0, sparkleColor, 0.8f + shieldIntensity * 0.4f);
                            sparkle.noGravity = true;
                        }
                        
                        // === BLACK ACCENT PARTICLES - keeps monochromatic theme ===
                        if (Main.rand.NextBool(8))
                        {
                            float blackAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                            Vector2 blackPos = player.Center + new Vector2((float)Math.Cos(blackAngle), (float)Math.Sin(blackAngle)) * (35f + Main.rand.NextFloat(8f));
                            
                            Dust black = Dust.NewDustPerfect(blackPos, DustID.Smoke,
                                Vector2.Zero, 180, Color.Black, 0.6f);
                            black.noGravity = true;
                        }
                        
                        // === ENHANCED LIGHTING - rainbow tint with more minions ===
                        float r = 0.35f + rainbowIntensity * 0.15f;
                        float g = 0.35f + rainbowIntensity * 0.15f;
                        float b = 0.4f + rainbowIntensity * 0.1f;
                        
                        // Add subtle rainbow cycling to light
                        float lightHue = (Main.GameUpdateCount * 0.01f) % 1f;
                        Color lightColor = Main.hslToRgb(lightHue, 0.3f, 0.5f);
                        r += lightColor.R / 255f * rainbowIntensity * 0.2f;
                        g += lightColor.G / 255f * rainbowIntensity * 0.2f;
                        b += lightColor.B / 255f * rainbowIntensity * 0.2f;
                        
                        Lighting.AddLight(player.Center, r * shieldIntensity, g * shieldIntensity, b * shieldIntensity);
                    }
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
                    // GRADIENT: Black → White with rainbow shimmer
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
