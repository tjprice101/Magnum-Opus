using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems;
using MagnumOpus.Content.Nachtmusik.ResonanceEnergies;
using MagnumOpus.Content.DiesIrae.ResonanceEnergies;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;
using MagnumOpus.Content.ClairDeLune.ResonanceEnergies;

namespace MagnumOpus.Content.Common.Accessories.MeleeChain
{
    #region T7 - Nocturnal Symphony Band

    /// <summary>
    /// T7: Nocturnal Symphony Band - Post-Fate Nachtmusik tier.
    /// Max Resonance 70. +2 per hit at night, constellation trails at 50+, Starfall Slash at 60 cost.
    /// </summary>
    public class NocturnalSymphonyBand : ModItem
    {
        private static readonly Color NachtmusikDeepPurple = new Color(45, 27, 78);
        private static readonly Color NachtmusikGold = new Color(255, 215, 0);
        private static readonly Color NachtmusikViolet = new Color(123, 104, 238);
        private static readonly Color NachtmusikStarWhite = new Color(255, 255, 255);

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 1, gold: 20);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
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
            resonancePlayer.hasFatesCosmicSymphony = true;
            
            // Enable T7
            resonancePlayer.hasNocturnalSymphonyBand = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var resonancePlayer = player.GetModPlayer<ResonanceComboPlayer>();

            float phase = Main.GameUpdateCount * 0.02f;
            float gradient = (float)Math.Sin(phase) * 0.5f + 0.5f;
            Color titleColor = Color.Lerp(NachtmusikDeepPurple, NachtmusikGold, gradient);

            tooltips.Add(new TooltipLine(Mod, "NocturnalTitle", "★ STELLAR RESONANCE ★")
            {
                OverrideColor = titleColor
            });

            tooltips.Add(new TooltipLine(Mod, "Effect1", "Max Resonance: 70")
            {
                OverrideColor = NachtmusikViolet
            });

            tooltips.Add(new TooltipLine(Mod, "Effect2", "Resonance builds +2 per hit at night")
            {
                OverrideColor = NachtmusikGold
            });

            tooltips.Add(new TooltipLine(Mod, "Effect3", "At 50+ Resonance: Attacks leave constellation trails")
            {
                OverrideColor = NachtmusikStarWhite
            });

            tooltips.Add(new TooltipLine(Mod, "Starfall", "Consume 60 Resonance: Starfall Slash")
            {
                OverrideColor = NachtmusikGold
            });

            tooltips.Add(new TooltipLine(Mod, "StarfallEffect", "Summons a crescent of starlight that rains star projectiles")
            {
                OverrideColor = new Color(200, 180, 255)
            });

            if (resonancePlayer.hasNocturnalSymphonyBand)
            {
                bool canBurst = resonancePlayer.resonanceStacks >= 60 && !resonancePlayer.IsBurstOnCooldown;
                Color stackColor = canBurst ? Color.Yellow :
                                   resonancePlayer.resonanceStacks >= 50 ? NachtmusikGold :
                                   Color.Lerp(Color.Gray, NachtmusikViolet, resonancePlayer.GetResonancePercent());

                tooltips.Add(new TooltipLine(Mod, "Stacks", $"Current Resonance: {resonancePlayer.resonanceStacks}/{resonancePlayer.maxResonance}")
                {
                    OverrideColor = stackColor
                });

                if (canBurst)
                {
                    tooltips.Add(new TooltipLine(Mod, "Ready", "★ STARFALL SLASH READY ★")
                    {
                        OverrideColor = Color.Yellow
                    });
                }
            }

            tooltips.Add(new TooltipLine(Mod, "Lore", "'The night sky conducts its eternal symphony'")
            {
                OverrideColor = NachtmusikDeepPurple * 0.9f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<FatesCosmicSymphony>(1)
                .AddIngredient<NachtmusikResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    #endregion

    #region T8 - Infernal Fortissimo Band

    /// <summary>
    /// T8: Infernal Fortissimo Band - Post-Fate Dies Irae tier.
    /// Max Resonance 80. Judgment Burn at 60+, no decay during bosses, Hellfire Crescendo at 70 cost.
    /// </summary>
    public class InfernalFortissimoBandT8 : ModItem
    {
        public override string Texture => "MagnumOpus/Content/Common/Accessories/MeleeChain/InfernalFortissimoBand";
        
        private static readonly Color DiesIraeCrimson = new Color(180, 40, 40);
        private static readonly Color DiesIraeBlack = new Color(30, 20, 25);
        private static readonly Color DiesIraeOrange = new Color(255, 120, 40);
        private static readonly Color DiesIraeGold = new Color(255, 200, 80);

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 1, gold: 50);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
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
            resonancePlayer.hasFatesCosmicSymphony = true;
            resonancePlayer.hasNocturnalSymphonyBand = true;
            
            // Enable T8
            resonancePlayer.hasInfernalFortissimoBandT8 = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var resonancePlayer = player.GetModPlayer<ResonanceComboPlayer>();

            float phase = Main.GameUpdateCount * 0.025f;
            float flicker = (float)Math.Sin(phase * 3f) * 0.3f + 0.7f;
            Color titleColor = Color.Lerp(DiesIraeCrimson, DiesIraeOrange, flicker);

            tooltips.Add(new TooltipLine(Mod, "InfernalTitle", "★ INFERNAL RESONANCE ★")
            {
                OverrideColor = titleColor
            });

            tooltips.Add(new TooltipLine(Mod, "Effect1", "Max Resonance: 80")
            {
                OverrideColor = DiesIraeOrange
            });

            tooltips.Add(new TooltipLine(Mod, "Effect2", "At 60+ Resonance: Attacks inflict Judgment Burn (3% max HP/s)")
            {
                OverrideColor = DiesIraeCrimson
            });

            tooltips.Add(new TooltipLine(Mod, "Effect3", "Resonance doesn't decay during boss fights")
            {
                OverrideColor = DiesIraeGold
            });

            tooltips.Add(new TooltipLine(Mod, "Hellfire", "Consume 70 Resonance: Hellfire Crescendo")
            {
                OverrideColor = DiesIraeOrange
            });

            tooltips.Add(new TooltipLine(Mod, "HellfireEffect", "Unleashes a massive explosion leaving burning ground for 5s")
            {
                OverrideColor = new Color(255, 180, 120)
            });

            if (resonancePlayer.hasInfernalFortissimoBandT8)
            {
                bool canBurst = resonancePlayer.resonanceStacks >= 70 && !resonancePlayer.IsBurstOnCooldown;
                Color stackColor = canBurst ? Color.Yellow :
                                   resonancePlayer.resonanceStacks >= 60 ? DiesIraeCrimson :
                                   Color.Lerp(Color.Gray, DiesIraeOrange, resonancePlayer.GetResonancePercent());

                tooltips.Add(new TooltipLine(Mod, "Stacks", $"Current Resonance: {resonancePlayer.resonanceStacks}/{resonancePlayer.maxResonance}")
                {
                    OverrideColor = stackColor
                });

                if (canBurst)
                {
                    tooltips.Add(new TooltipLine(Mod, "Ready", "★ HELLFIRE CRESCENDO READY ★")
                    {
                        OverrideColor = Color.Yellow
                    });
                }
            }

            tooltips.Add(new TooltipLine(Mod, "Lore", "'The day of wrath burns with righteous fury'")
            {
                OverrideColor = DiesIraeCrimson * 0.9f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<NocturnalSymphonyBand>(1)
                .AddIngredient<DiesIraeResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    #endregion

    #region T9 - Jubilant Crescendo Band

    /// <summary>
    /// T9: Jubilant Crescendo Band - Post-Fate Ode to Joy tier.
    /// Max Resonance 90. 2% lifesteal at 70+, +5 resonance on kill, Blooming Fury at 80 cost.
    /// </summary>
    public class JubilantCrescendoBand : ModItem
    {
        private static readonly Color OdeWhite = new Color(255, 255, 255);
        private static readonly Color OdeBlack = new Color(30, 30, 35);
        private static readonly Color OdeIridescent = new Color(255, 220, 255);
        private static readonly Color OdeRose = new Color(255, 200, 220);
        private static readonly Color OdeGold = new Color(255, 215, 0);

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 2);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
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
            resonancePlayer.hasFatesCosmicSymphony = true;
            resonancePlayer.hasNocturnalSymphonyBand = true;
            resonancePlayer.hasInfernalFortissimoBandT8 = true;
            
            // Enable T9
            resonancePlayer.hasJubilantCrescendoBand = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var resonancePlayer = player.GetModPlayer<ResonanceComboPlayer>();

            float hue = (Main.GameUpdateCount * 0.015f) % 1f;
            Color titleColor = Main.hslToRgb(hue, 0.7f, 0.8f);

            tooltips.Add(new TooltipLine(Mod, "JubilantTitle", "★ JUBILANT RESONANCE ★")
            {
                OverrideColor = titleColor
            });

            tooltips.Add(new TooltipLine(Mod, "Effect1", "Max Resonance: 90")
            {
                OverrideColor = OdeIridescent
            });

            tooltips.Add(new TooltipLine(Mod, "Effect2", "At 70+ Resonance: 2% lifesteal on all melee attacks")
            {
                OverrideColor = OdeRose
            });

            tooltips.Add(new TooltipLine(Mod, "Effect3", "Kills grant +5 Resonance instantly")
            {
                OverrideColor = OdeGold
            });

            tooltips.Add(new TooltipLine(Mod, "Effect4", "At max Resonance: Nearby allies gain +10% melee damage")
            {
                OverrideColor = OdeWhite
            });

            tooltips.Add(new TooltipLine(Mod, "Blooming", "Consume 80 Resonance: Blooming Fury")
            {
                OverrideColor = OdeRose
            });

            tooltips.Add(new TooltipLine(Mod, "BloomingEffect", "A nature explosion that heals allies and damages enemies")
            {
                OverrideColor = new Color(220, 255, 220)
            });

            if (resonancePlayer.hasJubilantCrescendoBand)
            {
                bool canBurst = resonancePlayer.resonanceStacks >= 80 && !resonancePlayer.IsBurstOnCooldown;
                Color stackColor = canBurst ? Color.Yellow :
                                   resonancePlayer.resonanceStacks >= 70 ? OdeRose :
                                   Color.Lerp(Color.Gray, OdeIridescent, resonancePlayer.GetResonancePercent());

                tooltips.Add(new TooltipLine(Mod, "Stacks", $"Current Resonance: {resonancePlayer.resonanceStacks}/{resonancePlayer.maxResonance}")
                {
                    OverrideColor = stackColor
                });

                if (canBurst)
                {
                    tooltips.Add(new TooltipLine(Mod, "Ready", "★ BLOOMING FURY READY ★")
                    {
                        OverrideColor = Color.Yellow
                    });
                }
            }

            tooltips.Add(new TooltipLine(Mod, "Lore", "'Freude, schöner Götterfunken - Joy, beautiful spark of divinity'")
            {
                OverrideColor = OdeGold * 0.9f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<InfernalFortissimoBandT8>(1)
                .AddIngredient<OdeToJoyResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    #endregion

    #region T10 - Eternal Resonance Band

    /// <summary>
    /// T10: Eternal Resonance Band - Post-Fate Clair de Lune tier (ULTIMATE).
    /// Max Resonance 100. Never decays, temporal echoes at 80+, time slow at 100, Temporal Finale at 90 cost.
    /// </summary>
    public class EternalResonanceBand : ModItem
    {
        private static readonly Color ClairGray = new Color(80, 75, 90);
        private static readonly Color ClairIridescent = new Color(200, 180, 220);
        private static readonly Color ClairBrass = new Color(205, 170, 125);
        private static readonly Color ClairCrimson = new Color(180, 60, 80);
        private static readonly Color ClairWhite = new Color(255, 255, 255);

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 3);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
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
            resonancePlayer.hasFatesCosmicSymphony = true;
            resonancePlayer.hasNocturnalSymphonyBand = true;
            resonancePlayer.hasInfernalFortissimoBandT8 = true;
            resonancePlayer.hasJubilantCrescendoBand = true;
            
            // Enable T10
            resonancePlayer.hasEternalResonanceBand = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var resonancePlayer = player.GetModPlayer<ResonanceComboPlayer>();

            float phase = Main.GameUpdateCount * 0.015f;
            float gradient = (float)Math.Sin(phase) * 0.5f + 0.5f;
            Color titleColor = Color.Lerp(ClairGray, ClairCrimson, gradient);

            tooltips.Add(new TooltipLine(Mod, "EternalTitle", "★ ETERNAL RESONANCE ★")
            {
                OverrideColor = titleColor
            });

            tooltips.Add(new TooltipLine(Mod, "Effect1", "Max Resonance: 100 (Ultimate)")
            {
                OverrideColor = ClairCrimson
            });

            tooltips.Add(new TooltipLine(Mod, "Effect2", "Resonance never decays (persists until consumed)")
            {
                OverrideColor = ClairBrass
            });

            tooltips.Add(new TooltipLine(Mod, "Effect3", "At 80+ Resonance: Attacks hit twice (temporal echo at 50% damage)")
            {
                OverrideColor = ClairIridescent
            });

            tooltips.Add(new TooltipLine(Mod, "Effect4", "At 100 Resonance: Time slows 15% for nearby enemies")
            {
                OverrideColor = ClairWhite
            });

            tooltips.Add(new TooltipLine(Mod, "Temporal", "Consume 90 Resonance: Temporal Finale")
            {
                OverrideColor = ClairCrimson
            });

            tooltips.Add(new TooltipLine(Mod, "TemporalEffect", "A slash that hits all on-screen enemies in past, present, and future (3 hits)")
            {
                OverrideColor = new Color(255, 220, 230)
            });

            if (resonancePlayer.hasEternalResonanceBand)
            {
                bool canBurst = resonancePlayer.resonanceStacks >= 90 && !resonancePlayer.IsBurstOnCooldown;
                Color stackColor = canBurst ? Color.Yellow :
                                   resonancePlayer.resonanceStacks >= 80 ? ClairCrimson :
                                   Color.Lerp(Color.Gray, ClairIridescent, resonancePlayer.GetResonancePercent());

                tooltips.Add(new TooltipLine(Mod, "Stacks", $"Current Resonance: {resonancePlayer.resonanceStacks}/{resonancePlayer.maxResonance}")
                {
                    OverrideColor = stackColor
                });

                if (canBurst)
                {
                    tooltips.Add(new TooltipLine(Mod, "Ready", "★ TEMPORAL FINALE READY ★")
                    {
                        OverrideColor = Color.Yellow
                    });
                }
            }

            tooltips.Add(new TooltipLine(Mod, "Lore", "'Time bends to the rhythm of eternity itself'")
            {
                OverrideColor = ClairGray * 0.9f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<JubilantCrescendoBand>(1)
                .AddIngredient<ClairDeLuneResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    #endregion

    #region Fusion Tier 1 - Starfall Judgment Gauntlet

    /// <summary>
    /// Starfall Judgment Gauntlet - Fusion of Nachtmusik + Dies Irae.
    /// Combines stellar and infernal resonance powers.
    /// </summary>
    public class StarfallJudgmentGauntlet : ModItem
    {
        private static readonly Color StarfallPurple = new Color(100, 50, 150);
        private static readonly Color StarfallCrimson = new Color(200, 60, 60);
        private static readonly Color StarfallGold = new Color(255, 200, 100);

        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 2, gold: 50);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var resonancePlayer = player.GetModPlayer<ResonanceComboPlayer>();

            // Enable all previous tiers including both fusion sources
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
            resonancePlayer.hasFatesCosmicSymphony = true;
            resonancePlayer.hasNocturnalSymphonyBand = true;
            resonancePlayer.hasInfernalFortissimoBandT8 = true;
            
            // Enable Fusion
            resonancePlayer.hasStarfallJudgmentGauntlet = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var resonancePlayer = player.GetModPlayer<ResonanceComboPlayer>();

            float phase = Main.GameUpdateCount * 0.02f;
            float blend = (float)Math.Sin(phase) * 0.5f + 0.5f;
            Color titleColor = Color.Lerp(StarfallPurple, StarfallCrimson, blend);

            tooltips.Add(new TooltipLine(Mod, "FusionTitle", "★ STARFALL JUDGMENT ★")
            {
                OverrideColor = titleColor
            });

            tooltips.Add(new TooltipLine(Mod, "FusionDesc", "Fuses the power of Nachtmusik and Dies Irae")
            {
                OverrideColor = StarfallGold
            });

            tooltips.Add(new TooltipLine(Mod, "Effect1", "Max Resonance: 85")
            {
                OverrideColor = StarfallPurple
            });

            tooltips.Add(new TooltipLine(Mod, "Effect2", "Combines constellation trails with judgment burn")
            {
                OverrideColor = StarfallCrimson
            });

            tooltips.Add(new TooltipLine(Mod, "Effect3", "+2 Resonance per hit at night, no decay during bosses")
            {
                OverrideColor = StarfallGold
            });

            tooltips.Add(new TooltipLine(Mod, "FusionBurst", "Consume 75 Resonance: Starfall Judgment")
            {
                OverrideColor = Color.Lerp(StarfallPurple, StarfallCrimson, 0.5f)
            });

            tooltips.Add(new TooltipLine(Mod, "FusionBurstEffect", "Stars and hellfire rain together in devastating unison")
            {
                OverrideColor = new Color(220, 180, 220)
            });

            if (resonancePlayer.hasStarfallJudgmentGauntlet)
            {
                bool canBurst = resonancePlayer.resonanceStacks >= 75 && !resonancePlayer.IsBurstOnCooldown;
                Color stackColor = canBurst ? Color.Yellow :
                                   Color.Lerp(Color.Gray, StarfallGold, resonancePlayer.GetResonancePercent());

                tooltips.Add(new TooltipLine(Mod, "Stacks", $"Current Resonance: {resonancePlayer.resonanceStacks}/{resonancePlayer.maxResonance}")
                {
                    OverrideColor = stackColor
                });
            }

            tooltips.Add(new TooltipLine(Mod, "Lore", "'When stars fall, judgment follows'")
            {
                OverrideColor = StarfallPurple * 0.9f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<NocturnalSymphonyBand>(1)
                .AddIngredient<InfernalFortissimoBandT8>(1)
                .AddIngredient<NachtmusikResonantEnergy>(10)
                .AddIngredient<DiesIraeResonantEnergy>(10)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    #endregion

    #region Fusion Tier 2 - Triumphant Cosmos Gauntlet

    /// <summary>
    /// Triumphant Cosmos Gauntlet - Fusion of Nachtmusik + Dies Irae + Ode to Joy.
    /// Three-theme cosmic gauntlet of celestial might.
    /// </summary>
    public class TriumphantCosmosGauntlet : ModItem
    {
        private static readonly Color TriumphPurple = new Color(120, 80, 180);
        private static readonly Color TriumphCrimson = new Color(220, 80, 100);
        private static readonly Color TriumphRose = new Color(255, 200, 220);
        private static readonly Color TriumphGold = new Color(255, 220, 120);

        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 42;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
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
            resonancePlayer.hasFatesCosmicSymphony = true;
            resonancePlayer.hasNocturnalSymphonyBand = true;
            resonancePlayer.hasInfernalFortissimoBandT8 = true;
            resonancePlayer.hasJubilantCrescendoBand = true;
            resonancePlayer.hasStarfallJudgmentGauntlet = true;
            
            // Enable Fusion Tier 2
            resonancePlayer.hasTriumphantCosmosGauntlet = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var resonancePlayer = player.GetModPlayer<ResonanceComboPlayer>();

            float hue = (Main.GameUpdateCount * 0.012f) % 1f;
            Color titleColor = Main.hslToRgb(hue, 0.6f, 0.75f);

            tooltips.Add(new TooltipLine(Mod, "FusionTitle", "★★ TRIUMPHANT COSMOS ★★")
            {
                OverrideColor = titleColor
            });

            tooltips.Add(new TooltipLine(Mod, "FusionDesc", "Fuses Nachtmusik, Dies Irae, and Ode to Joy")
            {
                OverrideColor = TriumphGold
            });

            tooltips.Add(new TooltipLine(Mod, "Effect1", "Max Resonance: 95")
            {
                OverrideColor = TriumphPurple
            });

            tooltips.Add(new TooltipLine(Mod, "Effect2", "All Tier 1-2 fusion effects combined")
            {
                OverrideColor = TriumphCrimson
            });

            tooltips.Add(new TooltipLine(Mod, "Effect3", "2% lifesteal, +5 resonance on kill, judgment burn")
            {
                OverrideColor = TriumphRose
            });

            tooltips.Add(new TooltipLine(Mod, "FusionBurst", "Consume 85 Resonance: Triumphant Cosmos")
            {
                OverrideColor = TriumphGold
            });

            tooltips.Add(new TooltipLine(Mod, "FusionBurstEffect", "Stars, hellfire, and blooming nature erupt in cosmic harmony")
            {
                OverrideColor = new Color(240, 220, 255)
            });

            if (resonancePlayer.hasTriumphantCosmosGauntlet)
            {
                bool canBurst = resonancePlayer.resonanceStacks >= 85 && !resonancePlayer.IsBurstOnCooldown;
                Color stackColor = canBurst ? Color.Yellow :
                                   Color.Lerp(Color.Gray, TriumphGold, resonancePlayer.GetResonancePercent());

                tooltips.Add(new TooltipLine(Mod, "Stacks", $"Current Resonance: {resonancePlayer.resonanceStacks}/{resonancePlayer.maxResonance}")
                {
                    OverrideColor = stackColor
                });
            }

            tooltips.Add(new TooltipLine(Mod, "Lore", "'Three symphonies become one cosmic triumph'")
            {
                OverrideColor = TriumphPurple * 0.9f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<StarfallJudgmentGauntlet>(1)
                .AddIngredient<JubilantCrescendoBand>(1)
                .AddIngredient<OdeToJoyResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    #endregion

    #region Fusion Tier 3 - Gauntlet of the Eternal Symphony

    /// <summary>
    /// Gauntlet of the Eternal Symphony - Ultimate fusion of all four Post-Fate themes.
    /// The pinnacle of melee resonance power.
    /// </summary>
    public class GauntletOfTheEternalSymphony : ModItem
    {
        private static readonly Color EternalPurple = new Color(140, 100, 200);
        private static readonly Color EternalCrimson = new Color(200, 80, 120);
        private static readonly Color EternalRose = new Color(255, 210, 230);
        private static readonly Color EternalBrass = new Color(205, 170, 125);
        private static readonly Color EternalWhite = new Color(255, 255, 255);

        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 44;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var resonancePlayer = player.GetModPlayer<ResonanceComboPlayer>();

            // Enable ALL tiers
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
            resonancePlayer.hasFatesCosmicSymphony = true;
            resonancePlayer.hasNocturnalSymphonyBand = true;
            resonancePlayer.hasInfernalFortissimoBandT8 = true;
            resonancePlayer.hasJubilantCrescendoBand = true;
            resonancePlayer.hasEternalResonanceBand = true;
            resonancePlayer.hasStarfallJudgmentGauntlet = true;
            resonancePlayer.hasTriumphantCosmosGauntlet = true;
            
            // Enable Ultimate Fusion
            resonancePlayer.hasGauntletOfTheEternalSymphony = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var resonancePlayer = player.GetModPlayer<ResonanceComboPlayer>();

            float phase = Main.GameUpdateCount * 0.01f;
            float hue = (phase * 0.2f) % 1f;
            Color titleColor = Main.hslToRgb(hue, 0.7f, 0.85f);

            tooltips.Add(new TooltipLine(Mod, "UltimateTitle", "★★★ GAUNTLET OF THE ETERNAL SYMPHONY ★★★")
            {
                OverrideColor = titleColor
            });

            tooltips.Add(new TooltipLine(Mod, "UltimateDesc", "The ultimate fusion of all four Post-Fate themes")
            {
                OverrideColor = EternalWhite
            });

            tooltips.Add(new TooltipLine(Mod, "Effect1", "Max Resonance: 100 (Eternal)")
            {
                OverrideColor = EternalCrimson
            });

            tooltips.Add(new TooltipLine(Mod, "Effect2", "All previous fusion and tier effects combined")
            {
                OverrideColor = EternalPurple
            });

            tooltips.Add(new TooltipLine(Mod, "Effect3", "Temporal echoes, time slow, never-decaying resonance")
            {
                OverrideColor = EternalBrass
            });

            tooltips.Add(new TooltipLine(Mod, "Effect4", "Attacks hit THREE times at max resonance")
            {
                OverrideColor = EternalRose
            });

            tooltips.Add(new TooltipLine(Mod, "UltimateBurst", "Consume 95 Resonance: Eternal Symphony Finale")
            {
                OverrideColor = EternalWhite
            });

            tooltips.Add(new TooltipLine(Mod, "UltimateBurstEffect", "All four themes erupt in the ultimate temporal-cosmic explosion")
            {
                OverrideColor = new Color(255, 240, 255)
            });

            if (resonancePlayer.hasGauntletOfTheEternalSymphony)
            {
                bool canBurst = resonancePlayer.resonanceStacks >= 95 && !resonancePlayer.IsBurstOnCooldown;
                bool isMax = resonancePlayer.resonanceStacks >= 100;
                Color stackColor = canBurst ? Color.Yellow :
                                   isMax ? EternalWhite :
                                   Color.Lerp(Color.Gray, EternalCrimson, resonancePlayer.GetResonancePercent());

                tooltips.Add(new TooltipLine(Mod, "Stacks", $"Current Resonance: {resonancePlayer.resonanceStacks}/{resonancePlayer.maxResonance}")
                {
                    OverrideColor = stackColor
                });

                if (canBurst)
                {
                    tooltips.Add(new TooltipLine(Mod, "Ready", "★★★ ETERNAL SYMPHONY FINALE READY ★★★")
                    {
                        OverrideColor = Color.Yellow
                    });
                }
            }

            tooltips.Add(new TooltipLine(Mod, "Lore", "'Four movements become one eternal masterpiece'")
            {
                OverrideColor = EternalBrass * 0.9f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<TriumphantCosmosGauntlet>(1)
                .AddIngredient<EternalResonanceBand>(1)
                .AddIngredient<ClairDeLuneResonantEnergy>(20)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    #endregion
}
