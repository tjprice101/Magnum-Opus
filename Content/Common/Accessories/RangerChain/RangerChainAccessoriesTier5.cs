using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Content.LaCampanella.HarmonicCores;
using MagnumOpus.Content.EnigmaVariations.ResonanceEnergies;
using MagnumOpus.Content.SwanLake.ResonanceEnergies;
using MagnumOpus.Content.Fate.ResonanceEnergies;

namespace MagnumOpus.Content.Common.Accessories.RangerChain
{
    #region Tier 5: Post-Moon Lord Theme Chain

    /// <summary>
    /// Moonlit Predator's Gaze - Post-Moonlight Sonata boss.
    /// Can mark up to 8 enemies. Marked enemies visible through walls.
    /// </summary>
    public class MoonlitPredatorsGaze : ModItem
    {
        private static readonly Color MoonlightPurple = new Color(138, 43, 226);
        private static readonly Color MoonlightSilver = new Color(200, 200, 230);
        private static readonly Color MoonlightBlue = new Color(135, 206, 250);
        
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
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            // Enable all previous tiers
            markingPlayer.hasResonantSpotter = true;
            markingPlayer.hasSpringHuntersLens = true;
            markingPlayer.hasSolarTrackersBadge = true;
            markingPlayer.hasHarvestReapersMark = true;
            markingPlayer.hasPermafrostHuntersEye = true;
            markingPlayer.hasVivaldisSeSonalSight = true;
            // Enable this tier
            markingPlayer.hasMoonlitPredatorsGaze = true;
            
            // Lunar particles
            if (!hideVisual && Main.rand.NextBool(12))
            {
                float angle = Main.GameUpdateCount * 0.03f;
                Vector2 orbitPos = player.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 35f;
                Vector2 vel = new Vector2(0, -Main.rand.NextFloat(0.3f, 0.6f));
                Color lunarColor = Color.Lerp(MoonlightPurple, MoonlightSilver, Main.rand.NextFloat());
                CustomParticles.GenericGlow(orbitPos, vel, lunarColor * 0.6f, 0.22f, 20, true);
                
                // Moonbeam sparkles
                if (Main.rand.NextBool(5))
                {
                    CustomParticles.GenericFlare(orbitPos, MoonlightSilver * 0.4f, 0.18f, 12);
                }
            }
            
            // Music notes for the sonata theme
            if (!hideVisual && Main.rand.NextBool(25))
            {
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                ThemedParticles.MusicNote(pos, new Vector2(0, -0.5f), MoonlightPurple, 0.25f, 20);
            }
            
            // Ethereal moonlight
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.15f + 0.25f;
            Lighting.AddLight(player.Center, MoonlightPurple.ToVector3() * pulse);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            
            tooltips.Add(new TooltipLine(Mod, "System", "Marked for Death System - Moonlit Enhancement")
            {
                OverrideColor = MoonlightPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Can mark up to 8 enemies simultaneously")
            {
                OverrideColor = MoonlightSilver
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Marked enemies glow through walls")
            {
                OverrideColor = MoonlightBlue
            });
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Inherits all seasonal mark effects")
            {
                OverrideColor = new Color(180, 180, 200)
            });
            
            if (markingPlayer.hasResonantSpotter)
            {
                int markedCount = markingPlayer.CountMarkedEnemies();
                tooltips.Add(new TooltipLine(Mod, "Marks", $"Currently Marked: {markedCount}/{markingPlayer.maxMarkedEnemies}")
                {
                    OverrideColor = markedCount > 0 ? MoonlightPurple : Color.Gray
                });
            }
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Under the moonlight, no prey can hide'")
            {
                OverrideColor = new Color(150, 150, 180)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<VivaldisSeSonalSight>(1)
                .AddIngredient<ResonantCoreOfMoonlightSonata>(20)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    /// <summary>
    /// Heroic Deadeye - Post-Eroica boss.
    /// Marked enemies take +8% damage. First shot on marked enemy is auto-crit.
    /// </summary>
    public class HeroicDeadeye : ModItem
    {
        private static readonly Color EroicaGold = new Color(255, 200, 80);
        private static readonly Color EroicaScarlet = new Color(200, 50, 50);
        private static readonly Color EroicaCrimson = new Color(180, 30, 60);
        
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
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            // Enable all previous tiers
            markingPlayer.hasResonantSpotter = true;
            markingPlayer.hasSpringHuntersLens = true;
            markingPlayer.hasSolarTrackersBadge = true;
            markingPlayer.hasHarvestReapersMark = true;
            markingPlayer.hasPermafrostHuntersEye = true;
            markingPlayer.hasVivaldisSeSonalSight = true;
            markingPlayer.hasMoonlitPredatorsGaze = true;
            // Enable this tier
            markingPlayer.hasHeroicDeadeye = true;
            
            // Heroic flames and sakura
            if (!hideVisual && Main.rand.NextBool(10))
            {
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(32f, 32f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(0.8f, 1.5f));
                Color flameColor = Color.Lerp(EroicaScarlet, EroicaGold, Main.rand.NextFloat());
                CustomParticles.GenericGlow(pos, vel, flameColor * 0.7f, 0.24f, 22, true);
                
                // Golden flares for heroic essence
                if (Main.rand.NextBool(5))
                {
                    CustomParticles.GenericFlare(pos, EroicaGold * 0.5f, 0.2f, 12);
                }
            }
            
            // Sakura petals (Eroica theme)
            if (!hideVisual && Main.rand.NextBool(20))
            {
                ThemedParticles.SakuraPetals(player.Center, 1, 40f);
            }
            
            // Heroic golden light
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.12f + 0.25f;
            Lighting.AddLight(player.Center, EroicaGold.ToVector3() * pulse);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            
            tooltips.Add(new TooltipLine(Mod, "System", "Marked for Death System - Heroic Enhancement")
            {
                OverrideColor = EroicaGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Marked enemies take +8% damage from your attacks")
            {
                OverrideColor = EroicaScarlet
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "First ranged hit on a newly marked enemy is guaranteed critical")
            {
                OverrideColor = EroicaGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Inherits Moonlit Predator's abilities")
            {
                OverrideColor = new Color(180, 180, 200)
            });
            
            if (markingPlayer.hasResonantSpotter)
            {
                int markedCount = markingPlayer.CountMarkedEnemies();
                tooltips.Add(new TooltipLine(Mod, "Marks", $"Currently Marked: {markedCount}/{markingPlayer.maxMarkedEnemies}")
                {
                    OverrideColor = markedCount > 0 ? EroicaGold : Color.Gray
                });
            }
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A hero's aim never wavers'")
            {
                OverrideColor = new Color(180, 150, 100)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<MoonlitPredatorsGaze>(1)
                .AddIngredient<ResonantCoreOfEroica>(20)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    /// <summary>
    /// Infernal Executioner's Brand - Post-La Campanella boss.
    /// Marked enemies burn (fire DoT). Death explosion radius +50%.
    /// </summary>
    public class InfernalExecutionersBrand : ModItem
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
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            // Enable all previous tiers
            markingPlayer.hasResonantSpotter = true;
            markingPlayer.hasSpringHuntersLens = true;
            markingPlayer.hasSolarTrackersBadge = true;
            markingPlayer.hasHarvestReapersMark = true;
            markingPlayer.hasPermafrostHuntersEye = true;
            markingPlayer.hasVivaldisSeSonalSight = true;
            markingPlayer.hasMoonlitPredatorsGaze = true;
            markingPlayer.hasHeroicDeadeye = true;
            // Enable this tier
            markingPlayer.hasInfernalExecutionersBrand = true;
            
            // Infernal flames and smoke
            if (!hideVisual && Main.rand.NextBool(8))
            {
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), -Main.rand.NextFloat(1f, 2f));
                Color fireColor = Color.Lerp(CampanellaOrange, CampanellaGold, Main.rand.NextFloat());
                CustomParticles.GenericGlow(pos, vel, fireColor * 0.8f, 0.28f, 20, true);
                
                // Black smoke wisps
                if (Main.rand.NextBool(3))
                {
                    Vector2 smokeVel = vel * 0.5f;
                    CustomParticles.GenericGlow(pos, smokeVel, CampanellaBlack * 0.6f, 0.35f, 28, true);
                }
            }
            
            // Bell chime flares
            if (!hideVisual && Main.rand.NextBool(30))
            {
                CustomParticles.GenericFlare(player.Center + Main.rand.NextVector2Circular(25f, 25f), 
                    CampanellaOrange * 0.5f, 0.25f, 15);
            }
            
            // Infernal light
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.07f) * 0.15f + 0.28f;
            Lighting.AddLight(player.Center, CampanellaOrange.ToVector3() * pulse);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            
            tooltips.Add(new TooltipLine(Mod, "System", "Marked for Death System - Infernal Enhancement")
            {
                OverrideColor = CampanellaOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Marked enemies burn with infernal fire")
            {
                OverrideColor = CampanellaGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Death explosion radius increased by 50%")
            {
                OverrideColor = CampanellaOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Inherits Heroic Deadeye's abilities")
            {
                OverrideColor = new Color(180, 180, 200)
            });
            
            if (markingPlayer.hasResonantSpotter)
            {
                int markedCount = markingPlayer.CountMarkedEnemies();
                tooltips.Add(new TooltipLine(Mod, "Marks", $"Currently Marked: {markedCount}/{markingPlayer.maxMarkedEnemies}")
                {
                    OverrideColor = markedCount > 0 ? CampanellaOrange : Color.Gray
                });
            }
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The bell tolls for those who bear the brand'")
            {
                OverrideColor = new Color(180, 130, 80)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<HeroicDeadeye>(1)
                .AddIngredient<ResonantCoreOfLaCampanella>(20)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    /// <summary>
    /// Enigma's Paradox Mark - Post-Enigma boss.
    /// Marks can spread to unmarked enemies on hit (15% chance). Dimensional marks.
    /// </summary>
    public class EnigmasParadoxMark : ModItem
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
            Item.rare = ModContent.RarityType<EnigmaRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            // Enable all previous tiers
            markingPlayer.hasResonantSpotter = true;
            markingPlayer.hasSpringHuntersLens = true;
            markingPlayer.hasSolarTrackersBadge = true;
            markingPlayer.hasHarvestReapersMark = true;
            markingPlayer.hasPermafrostHuntersEye = true;
            markingPlayer.hasVivaldisSeSonalSight = true;
            markingPlayer.hasMoonlitPredatorsGaze = true;
            markingPlayer.hasHeroicDeadeye = true;
            markingPlayer.hasInfernalExecutionersBrand = true;
            // Enable this tier
            markingPlayer.hasEnigmasParadoxMark = true;
            
            // Enigmatic void particles
            if (!hideVisual && Main.rand.NextBool(10))
            {
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                Vector2 vel = Main.rand.NextVector2Circular(0.5f, 0.5f);
                Color voidColor = Main.rand.NextBool() ? EnigmaPurple : EnigmaGreen;
                CustomParticles.GenericGlow(pos, vel, voidColor * 0.6f, 0.26f, 22, true);
                
                // Glyphs for arcane mystery
                if (Main.rand.NextBool(6))
                {
                    CustomParticles.GlyphBurst(pos, EnigmaPurple, 2, 2f);
                }
            }
            
            // Watching eye particles (Enigma theme)
            if (!hideVisual && Main.rand.NextBool(40))
            {
                Vector2 eyePos = player.Center + Main.rand.NextVector2Circular(40f, 40f);
                CustomParticles.EnigmaEyeGaze(eyePos, EnigmaPurple * 0.7f, 0.3f, null);
            }
            
            // Mysterious pulsing light
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.18f + 0.22f;
            Lighting.AddLight(player.Center, EnigmaPurple.ToVector3() * pulse);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            
            tooltips.Add(new TooltipLine(Mod, "System", "Marked for Death System - Paradox Enhancement")
            {
                OverrideColor = EnigmaPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Hitting marked enemies has 15% chance to spread marks")
            {
                OverrideColor = EnigmaGreen
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Marks transcend dimensions, touching the void")
            {
                OverrideColor = EnigmaPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Inherits Infernal Executioner's abilities")
            {
                OverrideColor = new Color(180, 180, 200)
            });
            
            if (markingPlayer.hasResonantSpotter)
            {
                int markedCount = markingPlayer.CountMarkedEnemies();
                tooltips.Add(new TooltipLine(Mod, "Marks", $"Currently Marked: {markedCount}/{markingPlayer.maxMarkedEnemies}")
                {
                    OverrideColor = markedCount > 0 ? EnigmaPurple : Color.Gray
                });
            }
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The mark spreads like questions without answers'")
            {
                OverrideColor = new Color(120, 80, 160)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<InfernalExecutionersBrand>(1)
                .AddIngredient<ResonantCoreOfEnigma>(20)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    /// <summary>
    /// Swan's Graceful Hunt - Post-Swan Lake boss.
    /// Perfect shots (no damage taken for 3s) apply "Swan Mark" — +15% crit chance against target.
    /// </summary>
    public class SwansGracefulHunt : ModItem
    {
        private static readonly Color SwanWhite = new Color(255, 255, 255);
        private static readonly Color SwanSilver = new Color(220, 225, 235);
        private static readonly Color SwanBlack = new Color(20, 20, 30);
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 50);
            Item.rare = ModContent.RarityType<SwanRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            // Enable all previous tiers
            markingPlayer.hasResonantSpotter = true;
            markingPlayer.hasSpringHuntersLens = true;
            markingPlayer.hasSolarTrackersBadge = true;
            markingPlayer.hasHarvestReapersMark = true;
            markingPlayer.hasPermafrostHuntersEye = true;
            markingPlayer.hasVivaldisSeSonalSight = true;
            markingPlayer.hasMoonlitPredatorsGaze = true;
            markingPlayer.hasHeroicDeadeye = true;
            markingPlayer.hasInfernalExecutionersBrand = true;
            markingPlayer.hasEnigmasParadoxMark = true;
            // Enable this tier
            markingPlayer.hasSwansGracefulHunt = true;
            
            // Graceful feather particles
            if (!hideVisual && Main.rand.NextBool(12))
            {
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-0.3f, 0.5f));
                Color featherColor = Main.rand.NextBool() ? SwanWhite : SwanSilver;
                CustomParticles.GenericGlow(pos, vel, featherColor * 0.7f, 0.28f, 30, true);
                
                // Black feather contrast
                if (Main.rand.NextBool(4))
                {
                    CustomParticles.GenericGlow(pos + Main.rand.NextVector2Circular(10f, 10f), 
                        vel, SwanBlack * 0.5f, 0.22f, 25, true);
                }
            }
            
            // Rainbow shimmer accents (Swan Lake theme)
            if (!hideVisual && Main.rand.NextBool(20))
            {
                float hue = Main.rand.NextFloat();
                Color rainbowColor = Main.hslToRgb(hue, 0.8f, 0.7f);
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                CustomParticles.GenericFlare(pos, rainbowColor * 0.4f, 0.2f, 12);
            }
            
            // Perfect shot indicator
            if (!hideVisual && markingPlayer.IsPerfectShot)
            {
                // Glowing halo when ready for perfect shot
                if (Main.rand.NextBool(15))
                {
                    CustomParticles.HaloRing(player.Center, SwanWhite * 0.3f, 0.15f, 12);
                }
            }
            
            // Elegant white light
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.12f + 0.28f;
            Lighting.AddLight(player.Center, SwanWhite.ToVector3() * pulse);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            
            tooltips.Add(new TooltipLine(Mod, "System", "Marked for Death System - Graceful Enhancement")
            {
                OverrideColor = SwanWhite
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Perfect shots apply 'Swan Mark'")
            {
                OverrideColor = SwanSilver
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Perfect shot: No damage taken for 3 seconds")
            {
                OverrideColor = new Color(200, 200, 210)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Swan Marked enemies: +15% crit chance")
            {
                OverrideColor = SwanWhite
            });
            
            // Perfect shot ready indicator
            if (markingPlayer.hasSwansGracefulHunt)
            {
                if (markingPlayer.IsPerfectShot)
                {
                    tooltips.Add(new TooltipLine(Mod, "Ready", "✦ Perfect Shot READY")
                    {
                        OverrideColor = SwanWhite
                    });
                }
                else
                {
                    tooltips.Add(new TooltipLine(Mod, "Charging", "Perfect Shot: Avoid damage to charge")
                    {
                        OverrideColor = Color.Gray
                    });
                }
            }
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Inherits Enigma's Paradox Mark abilities")
            {
                OverrideColor = new Color(180, 180, 200)
            });
            
            if (markingPlayer.hasResonantSpotter)
            {
                int markedCount = markingPlayer.CountMarkedEnemies();
                tooltips.Add(new TooltipLine(Mod, "Marks", $"Currently Marked: {markedCount}/{markingPlayer.maxMarkedEnemies}")
                {
                    OverrideColor = markedCount > 0 ? SwanWhite : Color.Gray
                });
            }
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Grace in the hunt, elegance in the kill'")
            {
                OverrideColor = new Color(200, 200, 210)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<EnigmasParadoxMark>(1)
                .AddIngredient<ResonantCoreOfSwanLake>(20)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    /// <summary>
    /// Fate's Cosmic Verdict - Ultimate tier (Post-Fate boss).
    /// Marked enemies take +12% damage. Killing marked boss drops bonus loot bag.
    /// </summary>
    public class FatesCosmicVerdict : ModItem
    {
        private static readonly Color FateCrimson = new Color(200, 80, 120);
        private static readonly Color FatePink = new Color(255, 150, 200);
        private static readonly Color FateWhite = new Color(255, 255, 255);
        private static readonly Color FatePurple = new Color(140, 50, 160);
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 75);
            Item.rare = ModContent.RarityType<FateRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            // Enable all previous tiers
            markingPlayer.hasResonantSpotter = true;
            markingPlayer.hasSpringHuntersLens = true;
            markingPlayer.hasSolarTrackersBadge = true;
            markingPlayer.hasHarvestReapersMark = true;
            markingPlayer.hasPermafrostHuntersEye = true;
            markingPlayer.hasVivaldisSeSonalSight = true;
            markingPlayer.hasMoonlitPredatorsGaze = true;
            markingPlayer.hasHeroicDeadeye = true;
            markingPlayer.hasInfernalExecutionersBrand = true;
            markingPlayer.hasEnigmasParadoxMark = true;
            markingPlayer.hasSwansGracefulHunt = true;
            // Enable this tier
            markingPlayer.hasFatesCosmicVerdict = true;
            
            // CELESTIAL COSMIC particles - Fate theme
            if (!hideVisual && Main.rand.NextBool(8))
            {
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(40f, 40f);
                Vector2 vel = Main.rand.NextVector2Circular(0.8f, 0.8f);
                
                // Cosmic gradient colors
                float gradientProgress = Main.rand.NextFloat();
                Color cosmicColor = gradientProgress < 0.33f 
                    ? Color.Lerp(FatePurple, FateCrimson, gradientProgress * 3f)
                    : gradientProgress < 0.66f 
                        ? Color.Lerp(FateCrimson, FatePink, (gradientProgress - 0.33f) * 3f)
                        : Color.Lerp(FatePink, FateWhite, (gradientProgress - 0.66f) * 3f);
                
                CustomParticles.GenericGlow(pos, vel, cosmicColor * 0.7f, 0.3f, 25, true);
                
                // Star sparkles
                if (Main.rand.NextBool(4))
                {
                    CustomParticles.GenericFlare(pos, FateWhite * 0.5f, 0.2f, 12);
                }
            }
            
            // Orbiting glyphs (Fate celestial theme)
            if (!hideVisual && Main.rand.NextBool(20))
            {
                float angle = Main.GameUpdateCount * 0.02f;
                for (int i = 0; i < 3; i++)
                {
                    float glyphAngle = angle + MathHelper.TwoPi * i / 3f;
                    Vector2 glyphPos = player.Center + new Vector2((float)Math.Cos(glyphAngle), (float)Math.Sin(glyphAngle)) * 45f;
                    CustomParticles.Glyph(glyphPos, FateCrimson * 0.6f, 0.25f, -1);
                }
            }
            
            // Cosmic halo pulses
            if (!hideVisual && Main.rand.NextBool(35))
            {
                CustomParticles.HaloRing(player.Center, FateCrimson * 0.3f, 0.2f, 15);
            }
            
            // Cosmic light with color shifting
            float timeShift = Main.GameUpdateCount * 0.02f;
            float pulse = (float)Math.Sin(timeShift) * 0.12f + 0.3f;
            Color lightColor = Color.Lerp(FateCrimson, FatePink, (float)Math.Sin(timeShift * 0.5f) * 0.5f + 0.5f);
            Lighting.AddLight(player.Center, lightColor.ToVector3() * pulse);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            
            tooltips.Add(new TooltipLine(Mod, "System", "Marked for Death System - COSMIC VERDICT")
            {
                OverrideColor = FateCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Marked enemies take +12% damage")
            {
                OverrideColor = FatePink
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Killing a marked boss drops bonus treasure")
            {
                OverrideColor = FateWhite
            });
            
            tooltips.Add(new TooltipLine(Mod, "Ultimate", "✦✦ ULTIMATE RANGER ACCESSORY ✦✦")
            {
                OverrideColor = FateCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Inherits ALL previous mark abilities")
            {
                OverrideColor = new Color(200, 180, 220)
            });
            
            if (markingPlayer.hasResonantSpotter)
            {
                int markedCount = markingPlayer.CountMarkedEnemies();
                tooltips.Add(new TooltipLine(Mod, "Marks", $"Currently Marked: {markedCount}/{markingPlayer.maxMarkedEnemies}")
                {
                    OverrideColor = markedCount > 0 ? FateCrimson : Color.Gray
                });
            }
            
            // Perfect shot indicator
            if (markingPlayer.hasSwansGracefulHunt && markingPlayer.IsPerfectShot)
            {
                tooltips.Add(new TooltipLine(Mod, "Ready", "✦ Perfect Shot READY")
                {
                    OverrideColor = FateWhite
                });
            }
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The cosmos itself judges those marked for death'")
            {
                OverrideColor = new Color(180, 120, 160)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SwansGracefulHunt>(1)
                .AddIngredient<ResonantCoreOfFate>(30)
                .AddIngredient<FateResonantEnergy>(10)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    #endregion
}
