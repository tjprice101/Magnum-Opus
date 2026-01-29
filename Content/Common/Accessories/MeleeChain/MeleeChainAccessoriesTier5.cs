using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

// Theme-specific imports
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Content.LaCampanella.HarmonicCores;
using MagnumOpus.Content.LaCampanella.ResonanceEnergies;
using MagnumOpus.Content.EnigmaVariations.ResonanceEnergies;
using MagnumOpus.Content.SwanLake.ResonanceEnergies;
using MagnumOpus.Content.Fate.ResonanceEnergies;

namespace MagnumOpus.Content.Common.Accessories.MeleeChain
{
    #region Tier 5A: Post-Moonlight Sonata Boss

    /// <summary>
    /// Moonlit Sonata Band - Post-Moonlight Sonata Boss tier.
    /// Max Resonance 45. At 35+ stacks: melee crits spawn lunar wisps that home on enemies.
    /// </summary>
    public class MoonlitSonataBand : ModItem
    {
        private static readonly Color MoonlightPurple = new Color(138, 43, 226);
        private static readonly Color MoonlightBlue = new Color(135, 206, 250);
        private static readonly Color MoonlightSilver = new Color(220, 220, 235);
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 30);
            Item.rare = ModContent.RarityType<MoonlightSonataRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var resonancePlayer = player.GetModPlayer<ResonanceComboPlayer>();
            // Enable all previous tiers
            resonancePlayer.hasResonantRhythmBand = true;
            resonancePlayer.hasSpringTempoCharm = true;
            resonancePlayer.hasSolarCrescendoRing = true;
            resonancePlayer.hasHarvestRhythmSignet = true;
            resonancePlayer.hasPermafrostCadenceSeal = true;
            resonancePlayer.hasVivaldisTempoMaster = true;
            // Enable this tier
            resonancePlayer.hasMoonlitSonataBand = true;
            
            // Moonlight particles
            if (!hideVisual && resonancePlayer.resonanceStacks > 0 && Main.rand.NextBool(7))
            {
                float intensity = resonancePlayer.GetResonancePercent();
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(40f, 40f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.7f, 0.7f), Main.rand.NextFloat(-1.2f, -0.5f));
                Color moonColor = Color.Lerp(MoonlightPurple, MoonlightBlue, Main.rand.NextFloat());
                CustomParticles.GenericGlow(pos, vel, moonColor * intensity, 0.28f + intensity * 0.14f, 28, true);
                
                // Lunar wisps at threshold
                if (resonancePlayer.resonanceStacks >= 35 && Main.rand.NextBool(5))
                {
                    float angle = Main.GameUpdateCount * 0.04f + Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 wispPos = player.Center + angle.ToRotationVector2() * 45f;
                    CustomParticles.GenericFlare(wispPos, MoonlightSilver * 0.7f, 0.25f, 18);
                    ThemedParticles.MusicNote(wispPos, vel * 0.4f, MoonlightPurple, 0.22f, 20);
                }
            }
            
            // Ethereal purple light
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.15f + 0.35f;
            float stackIntensity = (float)resonancePlayer.resonanceStacks / 45f;
            Lighting.AddLight(player.Center, MoonlightPurple.ToVector3() * pulse * stackIntensity);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var resonancePlayer = player.GetModPlayer<ResonanceComboPlayer>();
            
            tooltips.Add(new TooltipLine(Mod, "Upgrade", "Resonance Combo System (Moonlit)")
            {
                OverrideColor = MoonlightPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Max Resonance increased to 45")
            {
                OverrideColor = MoonlightBlue
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Includes all previous tier bonuses")
            {
                OverrideColor = new Color(180, 160, 220)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "At 35+ stacks: melee crits spawn lunar wisps")
            {
                OverrideColor = MoonlightSilver
            });
            
            tooltips.Add(new TooltipLine(Mod, "WispNote", "Lunar wisps home on nearby enemies")
            {
                OverrideColor = new Color(180, 180, 200)
            });
            
            // Show current stacks if equipped
            if (resonancePlayer.hasMoonlitSonataBand)
            {
                bool thresholdMet = resonancePlayer.resonanceStacks >= 35;
                tooltips.Add(new TooltipLine(Mod, "Stacks", $"Current Resonance: {resonancePlayer.resonanceStacks}/{resonancePlayer.maxResonance}")
                {
                    OverrideColor = thresholdMet ? MoonlightSilver : Color.Lerp(Color.Gray, MoonlightPurple, resonancePlayer.GetResonancePercent())
                });
            }
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The moon's soft melody guides your blade through the darkness'")
            {
                OverrideColor = MoonlightPurple * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<VivaldisTempoMaster>(1)
                .AddIngredient<ResonantCoreOfMoonlightSonata>(1)
                .AddIngredient<MoonlightsResonantEnergy>(5)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    #endregion

    #region Tier 5B: Post-Eroica Boss

    /// <summary>
    /// Heroic Crescendo - Post-Eroica Boss tier.
    /// Max Resonance 50. At 40+ stacks: +15% melee damage, +10% crit.
    /// </summary>
    public class HeroicCrescendo : ModItem
    {
        private static readonly Color EroicaScarlet = new Color(200, 50, 50);
        private static readonly Color EroicaGold = new Color(255, 200, 80);
        private static readonly Color SakuraPink = new Color(255, 150, 180);
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 35);
            Item.rare = ModContent.RarityType<EroicaRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var resonancePlayer = player.GetModPlayer<ResonanceComboPlayer>();
            // Enable all previous tiers
            resonancePlayer.hasResonantRhythmBand = true;
            resonancePlayer.hasSpringTempoCharm = true;
            resonancePlayer.hasSolarCrescendoRing = true;
            resonancePlayer.hasHarvestRhythmSignet = true;
            resonancePlayer.hasPermafrostCadenceSeal = true;
            resonancePlayer.hasVivaldisTempoMaster = true;
            resonancePlayer.hasMoonlitSonataBand = true;
            // Enable this tier
            resonancePlayer.hasHeroicCrescendo = true;
            
            // Heroic particles
            if (!hideVisual && resonancePlayer.resonanceStacks > 0 && Main.rand.NextBool(6))
            {
                float intensity = resonancePlayer.GetResonancePercent();
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(42f, 42f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), Main.rand.NextFloat(-1.5f, -0.5f));
                Color heroColor = Color.Lerp(EroicaScarlet, EroicaGold, Main.rand.NextFloat());
                CustomParticles.GenericGlow(pos, vel, heroColor * intensity, 0.3f + intensity * 0.15f, 26, true);
                
                // Sakura petals at threshold
                if (resonancePlayer.resonanceStacks >= 40 && Main.rand.NextBool(4))
                {
                    CustomParticles.GenericGlow(pos, vel * 0.6f, SakuraPink * 0.8f, 0.25f, 22, true);
                    ThemedParticles.MusicNote(pos, vel * 0.3f, EroicaGold, 0.24f, 20);
                }
            }
            
            // Golden heroic light
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.2f + 0.4f;
            float stackIntensity = (float)resonancePlayer.resonanceStacks / 50f;
            Lighting.AddLight(player.Center, EroicaGold.ToVector3() * pulse * stackIntensity);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var resonancePlayer = player.GetModPlayer<ResonanceComboPlayer>();
            
            tooltips.Add(new TooltipLine(Mod, "Upgrade", "Resonance Combo System (Heroic)")
            {
                OverrideColor = EroicaGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Max Resonance increased to 50")
            {
                OverrideColor = new Color(255, 220, 150)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Includes all previous tier bonuses")
            {
                OverrideColor = new Color(220, 180, 120)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "At 40+ stacks: +15% melee damage, +10% melee crit")
            {
                OverrideColor = EroicaScarlet
            });
            
            // Show current stacks if equipped
            if (resonancePlayer.hasHeroicCrescendo)
            {
                bool thresholdMet = resonancePlayer.resonanceStacks >= 40;
                tooltips.Add(new TooltipLine(Mod, "Stacks", $"Current Resonance: {resonancePlayer.resonanceStacks}/{resonancePlayer.maxResonance}")
                {
                    OverrideColor = thresholdMet ? EroicaScarlet : Color.Lerp(Color.Gray, EroicaGold, resonancePlayer.GetResonancePercent())
                });
                
                if (thresholdMet)
                {
                    tooltips.Add(new TooltipLine(Mod, "Active", "✓ Heroic power surging!")
                    {
                        OverrideColor = EroicaScarlet
                    });
                }
            }
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A hero's strength crescendos when hope is needed most'")
            {
                OverrideColor = EroicaScarlet * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<MoonlitSonataBand>(1)
                .AddIngredient<ResonantCoreOfEroica>(1)
                .AddIngredient<EroicasResonantEnergy>(5)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    #endregion

    #region Tier 5C: Post-La Campanella Boss

    /// <summary>
    /// Infernal Fortissimo - Post-La Campanella Boss tier.
    /// Max Resonance 55. At 45+ stacks: Scorched inflicts 3x damage, enemies explode on death.
    /// </summary>
    public class InfernalFortissimo : ModItem
    {
        private static readonly Color CampanellaOrange = new Color(255, 140, 40);
        private static readonly Color CampanellaGold = new Color(255, 200, 80);
        private static readonly Color CampanellaBlack = new Color(30, 20, 25);
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 40);
            Item.rare = ModContent.RarityType<LaCampanellaRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var resonancePlayer = player.GetModPlayer<ResonanceComboPlayer>();
            // Enable all previous tiers
            resonancePlayer.hasResonantRhythmBand = true;
            resonancePlayer.hasSpringTempoCharm = true;
            resonancePlayer.hasSolarCrescendoRing = true;
            resonancePlayer.hasHarvestRhythmSignet = true;
            resonancePlayer.hasPermafrostCadenceSeal = true;
            resonancePlayer.hasVivaldisTempoMaster = true;
            resonancePlayer.hasMoonlitSonataBand = true;
            resonancePlayer.hasHeroicCrescendo = true;
            // Enable this tier
            resonancePlayer.hasInfernalFortissimo = true;
            
            // Infernal particles with smoke
            if (!hideVisual && resonancePlayer.resonanceStacks > 0 && Main.rand.NextBool(6))
            {
                float intensity = resonancePlayer.GetResonancePercent();
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(44f, 44f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.6f, 0.6f), Main.rand.NextFloat(-2f, -0.8f));
                
                // Fire and smoke
                Color fireColor = Color.Lerp(CampanellaOrange, CampanellaGold, Main.rand.NextFloat());
                CustomParticles.GenericGlow(pos, vel, fireColor * intensity, 0.32f + intensity * 0.15f, 24, true);
                
                // Smoke particles
                if (Main.rand.NextBool(2))
                {
                    CustomParticles.GenericGlow(pos, vel * 0.5f, CampanellaBlack * 0.5f, 0.35f, 30, true);
                }
                
                // Bell chime flares at threshold
                if (resonancePlayer.resonanceStacks >= 45 && Main.rand.NextBool(4))
                {
                    CustomParticles.GenericFlare(pos, CampanellaGold, 0.35f, 16);
                    ThemedParticles.MusicNote(player.Center, vel * 0.4f, CampanellaOrange, 0.26f, 18);
                }
            }
            
            // Fiery light
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.25f + 0.45f;
            float stackIntensity = (float)resonancePlayer.resonanceStacks / 55f;
            Lighting.AddLight(player.Center, CampanellaOrange.ToVector3() * pulse * stackIntensity);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var resonancePlayer = player.GetModPlayer<ResonanceComboPlayer>();
            
            tooltips.Add(new TooltipLine(Mod, "Upgrade", "Resonance Combo System (Infernal)")
            {
                OverrideColor = CampanellaOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Max Resonance increased to 55")
            {
                OverrideColor = CampanellaGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Includes all previous tier bonuses")
            {
                OverrideColor = new Color(255, 180, 100)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "At 45+ stacks: Scorched deals 3x damage")
            {
                OverrideColor = new Color(255, 100, 50)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Enemies killed with Scorched explode")
            {
                OverrideColor = new Color(255, 80, 30)
            });
            
            // Show current stacks if equipped
            if (resonancePlayer.hasInfernalFortissimo)
            {
                bool thresholdMet = resonancePlayer.resonanceStacks >= 45;
                tooltips.Add(new TooltipLine(Mod, "Stacks", $"Current Resonance: {resonancePlayer.resonanceStacks}/{resonancePlayer.maxResonance}")
                {
                    OverrideColor = thresholdMet ? new Color(255, 100, 50) : Color.Lerp(Color.Gray, CampanellaOrange, resonancePlayer.GetResonancePercent())
                });
            }
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The bell's infernal toll echoes through eternity'")
            {
                OverrideColor = CampanellaOrange * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<HeroicCrescendo>(1)
                .AddIngredient<ResonantCoreOfLaCampanella>(1)
                .AddIngredient<LaCampanellaResonantEnergy>(5)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    #endregion

    #region Tier 5D: Post-Enigma Boss

    /// <summary>
    /// Enigma's Dissonance - Post-Enigma Boss tier.
    /// Max Resonance 60. At 50+ stacks: hits apply "Paradox" - enemies take delayed burst damage.
    /// </summary>
    public class EnigmasDissonance : ModItem
    {
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 45);
            Item.rare = ModContent.RarityType<EnigmaVariationsRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var resonancePlayer = player.GetModPlayer<ResonanceComboPlayer>();
            // Enable all previous tiers
            resonancePlayer.hasResonantRhythmBand = true;
            resonancePlayer.hasSpringTempoCharm = true;
            resonancePlayer.hasSolarCrescendoRing = true;
            resonancePlayer.hasHarvestRhythmSignet = true;
            resonancePlayer.hasPermafrostCadenceSeal = true;
            resonancePlayer.hasVivaldisTempoMaster = true;
            resonancePlayer.hasMoonlitSonataBand = true;
            resonancePlayer.hasHeroicCrescendo = true;
            resonancePlayer.hasInfernalFortissimo = true;
            // Enable this tier
            resonancePlayer.hasEnigmasDissonance = true;
            
            // Enigmatic void particles
            if (!hideVisual && resonancePlayer.resonanceStacks > 0 && Main.rand.NextBool(6))
            {
                float intensity = resonancePlayer.GetResonancePercent();
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(45f, 45f);
                Vector2 vel = Main.rand.NextVector2Circular(1.2f, 1.2f);
                
                // Void particles
                Color voidColor = Color.Lerp(EnigmaPurple, EnigmaGreen, Main.rand.NextFloat());
                CustomParticles.GenericGlow(pos, vel, voidColor * intensity, 0.3f + intensity * 0.15f, 26, true);
                
                // Glyphs at threshold
                if (resonancePlayer.resonanceStacks >= 50 && Main.rand.NextBool(4))
                {
                    CustomParticles.GlyphBurst(pos, EnigmaPurple * 0.8f, 3, 2f);
                }
            }
            
            // Mysterious pulsing light
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.03f) * 0.2f + 0.35f;
            float stackIntensity = (float)resonancePlayer.resonanceStacks / 60f;
            float colorShift = (float)Math.Sin(Main.GameUpdateCount * 0.02f) * 0.5f + 0.5f;
            Color lightColor = Color.Lerp(EnigmaPurple, EnigmaGreen, colorShift);
            Lighting.AddLight(player.Center, lightColor.ToVector3() * pulse * stackIntensity);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var resonancePlayer = player.GetModPlayer<ResonanceComboPlayer>();
            
            // Color that shifts between purple and green
            float colorShift = (float)Math.Sin(Main.GameUpdateCount * 0.02f) * 0.5f + 0.5f;
            Color titleColor = Color.Lerp(EnigmaPurple, EnigmaGreen, colorShift);
            
            tooltips.Add(new TooltipLine(Mod, "Upgrade", "Resonance Combo System (Enigmatic)")
            {
                OverrideColor = titleColor
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Max Resonance increased to 60")
            {
                OverrideColor = EnigmaPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Includes all previous tier bonuses")
            {
                OverrideColor = new Color(160, 100, 200)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "At 50+ stacks: melee hits apply Paradox")
            {
                OverrideColor = EnigmaGreen
            });
            
            tooltips.Add(new TooltipLine(Mod, "Paradox", "Paradox: Enemies take delayed burst damage after 2 seconds")
            {
                OverrideColor = new Color(80, 180, 120)
            });
            
            // Show current stacks if equipped
            if (resonancePlayer.hasEnigmasDissonance)
            {
                bool thresholdMet = resonancePlayer.resonanceStacks >= 50;
                tooltips.Add(new TooltipLine(Mod, "Stacks", $"Current Resonance: {resonancePlayer.resonanceStacks}/{resonancePlayer.maxResonance}")
                {
                    OverrideColor = thresholdMet ? EnigmaGreen : Color.Lerp(Color.Gray, EnigmaPurple, resonancePlayer.GetResonancePercent())
                });
            }
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The answer to the enigma... is another question'")
            {
                OverrideColor = EnigmaPurple * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<InfernalFortissimo>(1)
                .AddIngredient<ResonantCoreOfEnigma>(1)
                .AddIngredient<EnigmaResonantEnergy>(5)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    #endregion

    #region Tier 5E: Post-Swan Lake Boss

    /// <summary>
    /// Swan's Perfect Measure - Post-Swan Lake Boss tier.
    /// Max Resonance 60. At 55+ stacks: perfect dodges (within 0.3s of hit) grant 2s invuln + full refill.
    /// At 60 stacks: melee damage +25%, attacks have rainbow trail.
    /// </summary>
    public class SwansPerfectMeasure : ModItem
    {
        private static readonly Color SwanWhite = new Color(255, 255, 255);
        private static readonly Color SwanBlack = new Color(30, 30, 40);
        private static readonly Color SwanSilver = new Color(220, 225, 235);
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 55);
            Item.rare = ModContent.RarityType<SwanRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var resonancePlayer = player.GetModPlayer<ResonanceComboPlayer>();
            // Enable all previous tiers
            resonancePlayer.hasResonantRhythmBand = true;
            resonancePlayer.hasSpringTempoCharm = true;
            resonancePlayer.hasSolarCrescendoRing = true;
            resonancePlayer.hasHarvestRhythmSignet = true;
            resonancePlayer.hasPermafrostCadenceSeal = true;
            resonancePlayer.hasVivaldisTempoMaster = true;
            resonancePlayer.hasMoonlitSonataBand = true;
            resonancePlayer.hasHeroicCrescendo = true;
            resonancePlayer.hasInfernalFortissimo = true;
            resonancePlayer.hasEnigmasDissonance = true;
            // Enable this tier
            resonancePlayer.hasSwansPerfectMeasure = true;
            
            // Elegant monochrome particles with rainbow shimmer
            if (!hideVisual && resonancePlayer.resonanceStacks > 0 && Main.rand.NextBool(5))
            {
                float intensity = resonancePlayer.GetResonancePercent();
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(48f, 48f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), Main.rand.NextFloat(-1.2f, -0.4f));
                
                // Monochrome base
                Color baseColor = Main.rand.NextBool() ? SwanWhite : SwanBlack;
                CustomParticles.GenericGlow(pos, vel, baseColor * intensity, 0.28f + intensity * 0.14f, 28, true);
                
                // Rainbow shimmer overlay at high stacks
                if (resonancePlayer.resonanceStacks >= 55)
                {
                    float hue = (Main.GameUpdateCount * 0.02f + Main.rand.NextFloat()) % 1f;
                    Color rainbowColor = Main.hslToRgb(hue, 1f, 0.75f);
                    CustomParticles.GenericGlow(pos, vel * 0.5f, rainbowColor * 0.5f, 0.22f, 22, true);
                }
                
                // Feather drift at max
                if (resonancePlayer.resonanceStacks >= 60 && Main.rand.NextBool(3))
                {
                    CustomParticles.GenericGlow(pos, vel * 0.3f, SwanSilver * 0.7f, 0.25f, 30, true);
                }
            }
            
            // Prismatic light at high stacks
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.035f) * 0.2f + 0.5f;
            float stackIntensity = (float)resonancePlayer.resonanceStacks / 60f;
            
            if (resonancePlayer.resonanceStacks >= 55)
            {
                // Rainbow light
                float hue = (Main.GameUpdateCount * 0.01f) % 1f;
                Color rainbowLight = Main.hslToRgb(hue, 0.8f, 0.5f);
                Lighting.AddLight(player.Center, rainbowLight.ToVector3() * pulse * stackIntensity);
            }
            else
            {
                // Silver light
                Lighting.AddLight(player.Center, SwanSilver.ToVector3() * pulse * stackIntensity * 0.4f);
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var resonancePlayer = player.GetModPlayer<ResonanceComboPlayer>();
            
            // Alternating black and white with rainbow shimmer
            bool isWhite = (Main.GameUpdateCount / 30) % 2 == 0;
            float hue = (Main.GameUpdateCount * 0.01f) % 1f;
            Color titleColor = isWhite ? SwanWhite : Color.Lerp(SwanBlack, Main.hslToRgb(hue, 0.6f, 0.5f), 0.3f);
            
            tooltips.Add(new TooltipLine(Mod, "Upgrade", "Resonance Combo System (Perfect)")
            {
                OverrideColor = titleColor
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Max Resonance: 60")
            {
                OverrideColor = SwanSilver
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Includes all previous tier bonuses")
            {
                OverrideColor = new Color(200, 200, 210)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "At 55+ stacks: Perfect Measure active")
            {
                OverrideColor = Main.hslToRgb(hue, 0.8f, 0.7f)
            });
            
            tooltips.Add(new TooltipLine(Mod, "PerfectMeasure", "Dodge within 0.3s of being hit: 2s invulnerability + full Resonance")
            {
                OverrideColor = new Color(180, 220, 255)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect4", "At 60 stacks: +25% melee damage, rainbow attack trails")
            {
                OverrideColor = SwanWhite
            });
            
            // Show current stacks if equipped
            if (resonancePlayer.hasSwansPerfectMeasure)
            {
                bool perfectMeasure = resonancePlayer.resonanceStacks >= 55;
                bool maxStacks = resonancePlayer.resonanceStacks >= 60;
                
                Color stackColor = maxStacks ? Main.hslToRgb(hue, 1f, 0.8f) :
                                   perfectMeasure ? SwanSilver :
                                   Color.Lerp(Color.Gray, SwanWhite, resonancePlayer.GetResonancePercent());
                
                tooltips.Add(new TooltipLine(Mod, "Stacks", $"Current Resonance: {resonancePlayer.resonanceStacks}/{resonancePlayer.maxResonance}")
                {
                    OverrideColor = stackColor
                });
                
                if (perfectMeasure && resonancePlayer.GraceTimer > 0)
                {
                    float graceRemaining = resonancePlayer.GraceTimer / 60f;
                    tooltips.Add(new TooltipLine(Mod, "Grace", $"Perfect Measure window: {graceRemaining:F1}s")
                    {
                        OverrideColor = new Color(255, 255, 150)
                    });
                }
                
                if (maxStacks)
                {
                    tooltips.Add(new TooltipLine(Mod, "MaxActive", "✓ Maximum power achieved!")
                    {
                        OverrideColor = Main.hslToRgb(hue, 1f, 0.9f)
                    });
                }
            }
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'In the swan's final dance, every measure is perfect'")
            {
                OverrideColor = SwanSilver * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<EnigmasDissonance>(1)
                .AddIngredient<ResonantCoreOfSwanLake>(1)
                .AddIngredient<SwansResonanceEnergy>(5)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    #endregion

    #region Tier 5F: Post-Fate Boss (FINAL)

    /// <summary>
    /// Fate's Cosmic Symphony - Post-Fate Boss tier (FINAL accessory).
    /// Max Resonance 60. At 60 stacks: ALL bonuses + cosmic aura damages nearby enemies.
    /// Consume 60 stacks to trigger "Destiny's Crescendo" - massive cosmic explosion.
    /// </summary>
    public class FatesCosmicSymphony : ModItem
    {
        private static readonly Color FateWhite = new Color(255, 255, 255);
        private static readonly Color FateDarkPink = new Color(200, 80, 120);
        private static readonly Color FatePurple = new Color(140, 50, 160);
        private static readonly Color FateCrimson = new Color(180, 30, 60);
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 1);
            Item.rare = ModContent.RarityType<FateRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var resonancePlayer = player.GetModPlayer<ResonanceComboPlayer>();
            // Enable all previous tiers
            resonancePlayer.hasResonantRhythmBand = true;
            resonancePlayer.hasSpringTempoCharm = true;
            resonancePlayer.hasSolarCrescendoRing = true;
            resonancePlayer.hasHarvestRhythmSignet = true;
            resonancePlayer.hasPermafrostCadenceSeal = true;
            resonancePlayer.hasVivaldisTempoMaster = true;
            resonancePlayer.hasMoonlitSonataBand = true;
            resonancePlayer.hasHeroicCrescendo = true;
            resonancePlayer.hasInfernalFortissimo = true;
            resonancePlayer.hasEnigmasDissonance = true;
            resonancePlayer.hasSwansPerfectMeasure = true;
            // Enable FINAL tier
            resonancePlayer.hasFatesCosmicSymphony = true;
            
            // Celestial cosmic particles - MAXIMUM SPECTACLE
            if (!hideVisual && Main.rand.NextBool(4))
            {
                float intensity = resonancePlayer.GetResonancePercent();
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(55f, 55f);
                Vector2 vel = Main.rand.NextVector2Circular(1.5f, 1.5f);
                
                // Dark prismatic gradient
                float gradient = Main.rand.NextFloat();
                Color cosmicColor = gradient < 0.33f ? Color.Lerp(FateWhite, FateDarkPink, gradient * 3f) :
                                    gradient < 0.66f ? Color.Lerp(FateDarkPink, FatePurple, (gradient - 0.33f) * 3f) :
                                    Color.Lerp(FatePurple, FateCrimson, (gradient - 0.66f) * 3f);
                
                CustomParticles.GenericGlow(pos, vel, cosmicColor * intensity, 0.35f + intensity * 0.18f, 30, true);
                
                // Orbiting glyphs
                if (Main.rand.NextBool(3))
                {
                    float angle = Main.GameUpdateCount * 0.03f + Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 glyphPos = player.Center + angle.ToRotationVector2() * 50f;
                    CustomParticles.GlyphBurst(glyphPos, FateDarkPink * 0.7f, 2, 1.5f);
                }
                
                // Star sparkles
                if (Main.rand.NextBool(4))
                {
                    CustomParticles.GenericFlare(pos, FateWhite * 0.8f, 0.25f, 15);
                }
                
                // Music notes at high stacks
                if (resonancePlayer.resonanceStacks >= 50 && Main.rand.NextBool(5))
                {
                    ThemedParticles.MusicNote(pos, vel * 0.4f, FateDarkPink, 0.28f, 22);
                }
                
                // COSMIC AURA DAMAGE at max stacks
                if (resonancePlayer.resonanceStacks >= 60)
                {
                    // Visual: cosmic cloud energy billowing
                    for (int i = 0; i < 2; i++)
                    {
                        Vector2 cloudPos = player.Center + Main.rand.NextVector2Circular(60f, 60f);
                        Vector2 cloudVel = (cloudPos - player.Center).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 2f);
                        CustomParticles.GenericGlow(cloudPos, cloudVel, FatePurple * 0.4f, 0.4f, 25, true);
                    }
                }
            }
            
            // Cosmic light with chromatic effect
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.025f) * 0.25f + 0.55f;
            float stackIntensity = (float)resonancePlayer.resonanceStacks / 60f;
            float colorPhase = Main.GameUpdateCount * 0.015f;
            
            // RGB channel separation for cosmic feel
            float r = (float)Math.Sin(colorPhase) * 0.5f + 0.5f;
            float g = (float)Math.Sin(colorPhase + MathHelper.TwoPi / 3f) * 0.5f + 0.5f;
            float b = (float)Math.Sin(colorPhase + 2f * MathHelper.TwoPi / 3f) * 0.5f + 0.5f;
            
            Lighting.AddLight(player.Center, new Vector3(r, g * 0.5f, b) * pulse * stackIntensity);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var resonancePlayer = player.GetModPlayer<ResonanceComboPlayer>();
            
            // Cosmic gradient title
            float phase = Main.GameUpdateCount * 0.02f;
            float gradient = (float)Math.Sin(phase) * 0.5f + 0.5f;
            Color titleColor = Color.Lerp(FateWhite, FateDarkPink, gradient);
            
            tooltips.Add(new TooltipLine(Mod, "Final", "★ ULTIMATE RESONANCE SYSTEM ★")
            {
                OverrideColor = titleColor
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Max Resonance: 60 (Final)")
            {
                OverrideColor = FatePurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Includes ALL previous tier bonuses")
            {
                OverrideColor = FateDarkPink
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "At 60 stacks: Cosmic Aura damages nearby enemies")
            {
                OverrideColor = FateCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Destiny", "Consume 60 Resonance: Destiny's Crescendo")
            {
                OverrideColor = FateWhite
            });
            
            tooltips.Add(new TooltipLine(Mod, "DestinyEffect", "Unleashes a massive cosmic explosion")
            {
                OverrideColor = new Color(255, 200, 220)
            });
            
            // Show current stacks if equipped
            if (resonancePlayer.hasFatesCosmicSymphony)
            {
                bool maxStacks = resonancePlayer.resonanceStacks >= 60;
                bool canBurst = maxStacks && !resonancePlayer.IsBurstOnCooldown;
                
                Color stackColor = canBurst ? Color.Yellow :
                                   maxStacks ? FateCrimson :
                                   Color.Lerp(Color.Gray, FateDarkPink, resonancePlayer.GetResonancePercent());
                
                tooltips.Add(new TooltipLine(Mod, "Stacks", $"Current Resonance: {resonancePlayer.resonanceStacks}/{resonancePlayer.maxResonance}")
                {
                    OverrideColor = stackColor
                });
                
                if (canBurst)
                {
                    tooltips.Add(new TooltipLine(Mod, "Ready", "★ DESTINY'S CRESCENDO READY ★")
                    {
                        OverrideColor = Color.Yellow
                    });
                }
                else if (resonancePlayer.IsBurstOnCooldown)
                {
                    float cooldownRemaining = resonancePlayer.BurstCooldown / 60f;
                    tooltips.Add(new TooltipLine(Mod, "Cooldown", $"Cooldown: {cooldownRemaining:F1}s")
                    {
                        OverrideColor = Color.Gray
                    });
                }
            }
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The cosmos itself plays the final symphony of destiny'")
            {
                OverrideColor = FatePurple * 0.9f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SwansPerfectMeasure>(1)
                .AddIngredient<ResonantCoreOfFate>(1)
                .AddIngredient<FateResonantEnergy>(10)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    #endregion
}
