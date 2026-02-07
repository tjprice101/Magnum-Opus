using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Materials;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common;
using MagnumOpus.Content.ClairDeLune.ResonanceEnergies;
using MagnumOpus.Content.DiesIrae.ResonanceEnergies;
using MagnumOpus.Content.Nachtmusik.ResonanceEnergies;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;

namespace MagnumOpus.Content.Common.Accessories.RangerChain
{
    #region T7: Nocturnal Predator's Sight (Nachtmusik Theme)
    
    /// <summary>
    /// T7 Ranger accessory - Nachtmusik theme (post-Fate).
    /// Starlight guides marks through the darkness.
    /// Max 12 marks, visible through walls, +5% damage at night, star shower on kill.
    /// </summary>
    public class NocturnalPredatorsSight : ModItem
    {
        private static readonly Color NachtmusikPurple = new Color(100, 80, 180);
        private static readonly Color NachtmusikGold = new Color(255, 215, 140);
        private static readonly Color NachtmusikSilver = new Color(200, 210, 230);
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 85);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
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
            markingPlayer.hasFatesCosmicVerdict = true;
            
            // T7 flag
            markingPlayer.hasNocturnalPredatorsSight = true;
            
            // Constellation particles
            if (!hideVisual && Main.rand.NextBool(8))
            {
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                Vector2 vel = Main.rand.NextVector2Circular(0.5f, 0.5f);
                Color starColor = Main.rand.NextBool() ? NachtmusikGold : NachtmusikPurple;
                CustomParticles.GenericGlow(pos, vel, starColor * 0.7f, 0.25f, 22, true);
                
                // Star twinkle
                if (Main.rand.NextBool(4))
                {
                    CustomParticles.GenericFlare(pos, NachtmusikSilver * 0.6f, 0.2f, 15);
                }
            }
            
            // Orbiting constellation points
            if (!hideVisual && Main.rand.NextBool(20))
            {
                float angle = Main.GameUpdateCount * 0.025f;
                for (int i = 0; i < 4; i++)
                {
                    float starAngle = angle + MathHelper.TwoPi * i / 4f;
                    Vector2 starPos = player.Center + new Vector2((float)Math.Cos(starAngle), (float)Math.Sin(starAngle)) * 40f;
                    CustomParticles.GenericFlare(starPos, NachtmusikGold * 0.5f, 0.18f, 10);
                }
            }
            
            // Night time bonus indicator
            if (!hideVisual && !Main.dayTime && Main.rand.NextBool(15))
            {
                ThemedParticles.MusicNote(player.Center + Main.rand.NextVector2Circular(30f, 30f), 
                    Vector2.UnitY * -0.3f, NachtmusikPurple * 0.6f, 0.5f, 25);
            }
            
            // Stellar glow
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.03f) * 0.1f + 0.25f;
            Lighting.AddLight(player.Center, NachtmusikPurple.ToVector3() * pulse);
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            
            tooltips.Add(new TooltipLine(Mod, "System", "Marked for Death System - STELLAR PREDATOR")
            {
                OverrideColor = NachtmusikPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Mark up to 12 enemies at once")
            {
                OverrideColor = NachtmusikGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Marks are visible through walls at any distance")
            {
                OverrideColor = NachtmusikSilver
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "At night: Marked enemies take +5% additional damage")
            {
                OverrideColor = NachtmusikPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Killing marked enemy triggers star shower on nearby foes")
            {
                OverrideColor = NachtmusikGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Inherits ALL previous mark abilities")
            {
                OverrideColor = new Color(200, 180, 220)
            });
            
            if (markingPlayer.hasResonantSpotter)
            {
                int markedCount = markingPlayer.CountMarkedEnemies();
                tooltips.Add(new TooltipLine(Mod, "Marks", $"Currently Marked: {markedCount}/12")
                {
                    OverrideColor = markedCount > 0 ? NachtmusikPurple : Color.Gray
                });
            }
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The stars reveal all that hides in darkness'")
            {
                OverrideColor = new Color(140, 120, 180)
            });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<FatesCosmicVerdict>(1)
                .AddIngredient<NachtmusikResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    #endregion
    
    #region T8: Infernal Executioner's Sight (Dies Irae Theme)
    
    /// <summary>
    /// T8 Ranger accessory - Dies Irae theme (post-Fate).
    /// Hellfire brands targets for destruction.
    /// Max 14 marks, burning DoT, +100% explosion damage, 20% spread, judgment stacks.
    /// </summary>
    public class InfernalExecutionersSight : ModItem
    {
        private static readonly Color DiesIraeCrimson = new Color(200, 50, 50);
        private static readonly Color DiesIraeOrange = new Color(255, 120, 40);
        private static readonly Color DiesIraeBlack = new Color(30, 20, 25);
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 95);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
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
            markingPlayer.hasFatesCosmicVerdict = true;
            markingPlayer.hasNocturnalPredatorsSight = true;
            
            // T8 flag
            markingPlayer.hasInfernalExecutionersSight = true;
            
            // Hellfire particles
            if (!hideVisual && Main.rand.NextBool(6))
            {
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-1.5f, -0.5f));
                Color fireColor = Color.Lerp(DiesIraeOrange, DiesIraeCrimson, Main.rand.NextFloat());
                CustomParticles.GenericGlow(pos, vel, fireColor * 0.7f, 0.3f, 20, true);
            }
            
            // Rising embers
            if (!hideVisual && Main.rand.NextBool(10))
            {
                Vector2 emberPos = player.Center + new Vector2(Main.rand.NextFloat(-25f, 25f), 20f);
                Vector2 emberVel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-2f, -1f));
                CustomParticles.GenericFlare(emberPos, DiesIraeOrange * 0.8f, 0.2f, 18);
            }
            
            // Smoke wisps
            if (!hideVisual && Main.rand.NextBool(15))
            {
                var smoke = new HeavySmokeParticle(
                    player.Center + Main.rand.NextVector2Circular(20f, 20f),
                    new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-0.8f, -0.3f)),
                    DiesIraeBlack * 0.4f, Main.rand.Next(25, 40), 0.25f, 0.5f, 0.015f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Judgment flames glow
            float flicker = Main.rand.NextFloat(0.8f, 1f);
            Lighting.AddLight(player.Center, DiesIraeOrange.ToVector3() * 0.3f * flicker);
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "System", "Marked for Death System - INFERNAL EXECUTIONER")
            {
                OverrideColor = DiesIraeCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Mark up to 14 enemies at once")
            {
                OverrideColor = DiesIraeOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Marked enemies take burning damage (2% max HP/s)")
            {
                OverrideColor = DiesIraeCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Death explosions deal +100% damage and leave burning ground")
            {
                OverrideColor = DiesIraeOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Marks have 20% chance to spread to nearby enemies on hit")
            {
                OverrideColor = DiesIraeCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Judgment Stacks: +3% damage per hit on marked enemy (max +30%)")
            {
                OverrideColor = DiesIraeOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Inherits ALL previous mark abilities")
            {
                OverrideColor = new Color(200, 180, 220)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Hellfire brands those condemned to oblivion'")
            {
                OverrideColor = new Color(180, 100, 80)
            });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<NocturnalPredatorsSight>(1)
                .AddIngredient<DiesIraeResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    #endregion
    
    #region T9: Jubilant Hunter's Sight (Ode to Joy Theme)
    
    /// <summary>
    /// T9 Ranger accessory - Ode to Joy theme (post-Fate).
    /// Nature's blessing guides your aim.
    /// Max 16 marks, healing orbs, +8% damage buff on kill, vine entangle, Nature's Bounty.
    /// </summary>
    public class JubilantHuntersSight : ModItem
    {
        private static readonly Color OdeToJoyWhite = new Color(255, 255, 255);
        private static readonly Color OdeToJoyBlack = new Color(30, 30, 40);
        private static readonly Color OdeToJoyIridescent = new Color(220, 200, 255);
        private static readonly Color OdeToJoyRose = new Color(255, 180, 200);
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 105);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
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
            markingPlayer.hasFatesCosmicVerdict = true;
            markingPlayer.hasNocturnalPredatorsSight = true;
            markingPlayer.hasInfernalExecutionersSight = true;
            
            // T9 flag
            markingPlayer.hasJubilantHuntersSight = true;
            
            // Iridescent sparkles
            if (!hideVisual && Main.rand.NextBool(7))
            {
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                Vector2 vel = Main.rand.NextVector2Circular(0.6f, 0.6f);
                
                // Rainbow shimmer
                float hue = (Main.GameUpdateCount * 0.02f + Main.rand.NextFloat()) % 1f;
                Color shimmerColor = Main.hslToRgb(hue, 0.6f, 0.8f);
                CustomParticles.GenericGlow(pos, vel, shimmerColor * 0.6f, 0.25f, 20, true);
            }
            
            // Rose petals
            if (!hideVisual && Main.rand.NextBool(12))
            {
                Vector2 petalPos = player.Center + Main.rand.NextVector2Circular(40f, 40f);
                Vector2 petalVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-0.5f, 0.5f));
                Color petalColor = Color.Lerp(OdeToJoyRose, OdeToJoyWhite, Main.rand.NextFloat(0.3f));
                CustomParticles.GenericGlow(petalPos, petalVel, petalColor * 0.7f, 0.22f, 25, true);
            }
            
            // Music notes of joy
            if (!hideVisual && Main.rand.NextBool(18))
            {
                ThemedParticles.MusicNote(player.Center + Main.rand.NextVector2Circular(25f, 25f),
                    Vector2.UnitY * -0.4f, OdeToJoyIridescent * 0.6f, 0.5f, 28);
            }
            
            // Gentle prismatic glow
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.08f + 0.22f;
            float hueShift = (Main.GameUpdateCount * 0.01f) % 1f;
            Color lightColor = Main.hslToRgb(hueShift, 0.4f, 0.7f);
            Lighting.AddLight(player.Center, lightColor.ToVector3() * pulse);
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "System", "Marked for Death System - JUBILANT HUNTER")
            {
                OverrideColor = OdeToJoyIridescent
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Mark up to 16 enemies at once")
            {
                OverrideColor = OdeToJoyWhite
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Marked enemies drop healing orbs when hit (5% chance, heals 10 HP)")
            {
                OverrideColor = OdeToJoyRose
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Killing marked enemies grants +8% damage buff for 10s (stacks)")
            {
                OverrideColor = OdeToJoyIridescent
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Marks cause vines to entangle enemies, slowing them 20%")
            {
                OverrideColor = new Color(150, 200, 100)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Nature's Bounty: Kill 5 marked enemies within 10s to spawn homing projectile")
            {
                OverrideColor = OdeToJoyRose
            });
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Inherits ALL previous mark abilities")
            {
                OverrideColor = new Color(200, 180, 220)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Nature's blessing flows through the hunt'")
            {
                OverrideColor = new Color(200, 220, 180)
            });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<InfernalExecutionersSight>(1)
                .AddIngredient<OdeToJoyResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    #endregion
    
    #region T10: Eternal Verdict Sight (Clair de Lune Theme)
    
    /// <summary>
    /// T10 Ranger accessory - Clair de Lune theme (post-Fate).
    /// Time marks prey across all moments.
    /// Max 20 marks, persist after death, triple hit chance, linked damage, Temporal Judgment.
    /// </summary>
    public class EternalVerdictSight : ModItem
    {
        private static readonly Color ClairDeLuneGray = new Color(120, 110, 130);
        private static readonly Color ClairDeLuneBrass = new Color(200, 170, 100);
        private static readonly Color ClairDeLuneCrimson = new Color(180, 80, 100);
        private static readonly Color ClairDeLuneIridescent = new Color(180, 170, 200);
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 120);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
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
            markingPlayer.hasFatesCosmicVerdict = true;
            markingPlayer.hasNocturnalPredatorsSight = true;
            markingPlayer.hasInfernalExecutionersSight = true;
            markingPlayer.hasJubilantHuntersSight = true;
            
            // T10 flag
            markingPlayer.hasEternalVerdictSight = true;
            
            // Clockwork gear particles
            if (!hideVisual && Main.rand.NextBool(10))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 gearPos = player.Center + angle.ToRotationVector2() * Main.rand.NextFloat(25f, 45f);
                Vector2 gearVel = Main.rand.NextVector2Circular(0.5f, 0.5f);
                CustomParticles.Glyph(gearPos, ClairDeLuneBrass * 0.5f, 0.2f, -1);
            }
            
            // Temporal flame wisps
            if (!hideVisual && Main.rand.NextBool(8))
            {
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                Vector2 vel = Main.rand.NextVector2Circular(0.4f, 0.4f);
                Color temporalColor = Color.Lerp(ClairDeLuneCrimson, ClairDeLuneBrass, Main.rand.NextFloat());
                CustomParticles.GenericGlow(pos, vel, temporalColor * 0.6f, 0.28f, 22, true);
            }
            
            // Shattered glass effect
            if (!hideVisual && Main.rand.NextBool(20))
            {
                Vector2 shardPos = player.Center + new Vector2(Main.rand.NextFloat(-35f, 35f), Main.rand.NextFloat(-40f, 10f));
                Vector2 shardVel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(0.5f, 1.5f));
                CustomParticles.GenericFlare(shardPos, ClairDeLuneIridescent * 0.5f, 0.15f, 20);
            }
            
            // Eternal music of time
            if (!hideVisual && Main.rand.NextBool(16))
            {
                ThemedParticles.MusicNote(player.Center + Main.rand.NextVector2Circular(30f, 30f),
                    new Vector2(0, Main.rand.NextFloat(-0.5f, 0.2f)), ClairDeLuneCrimson * 0.6f, 0.55f, 30);
            }
            
            // Temporal glow with color shift
            float timeShift = Main.GameUpdateCount * 0.015f;
            float pulse = (float)Math.Sin(timeShift) * 0.1f + 0.25f;
            Color lightColor = Color.Lerp(ClairDeLuneCrimson, ClairDeLuneBrass, (float)Math.Sin(timeShift * 0.5f) * 0.5f + 0.5f);
            Lighting.AddLight(player.Center, lightColor.ToVector3() * pulse);
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "System", "Marked for Death System - ETERNAL VERDICT")
            {
                OverrideColor = ClairDeLuneCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Ultimate", "✦✦✦ ULTIMATE RANGER ACCESSORY ✦✦✦")
            {
                OverrideColor = ClairDeLuneBrass
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Mark up to 20 enemies at once")
            {
                OverrideColor = ClairDeLuneIridescent
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Marks persist after death and transfer to respawned enemies")
            {
                OverrideColor = ClairDeLuneCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Shots hit marked enemies in past and future positions (triple hit chance)")
            {
                OverrideColor = ClairDeLuneBrass
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect4", "At 15+ marks: All marked enemies are linked (25% shared damage)")
            {
                OverrideColor = ClairDeLuneIridescent
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Temporal Judgment: Killing a marked boss rewinds 5s of damage")
            {
                OverrideColor = ClairDeLuneCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Inherits ALL previous mark abilities")
            {
                OverrideColor = new Color(200, 180, 220)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Time itself marks your prey across all moments'")
            {
                OverrideColor = new Color(160, 140, 180)
            });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<JubilantHuntersSight>(1)
                .AddIngredient<ClairDeLuneResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    #endregion
    
    #region Fusion Tier 1: Starfall Executioner's Scope (Nachtmusik + Dies Irae)
    
    /// <summary>
    /// Fusion Tier 1 Ranger accessory - combines Nachtmusik and Dies Irae.
    /// Combines stellar precision with hellfire judgment.
    /// </summary>
    public class StarfallExecutionersScope : ModItem
    {
        private static readonly Color NachtmusikPurple = new Color(100, 80, 180);
        private static readonly Color DiesIraeCrimson = new Color(200, 50, 50);
        private static readonly Color FusionGold = new Color(255, 180, 80);
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 130);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
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
            markingPlayer.hasFatesCosmicVerdict = true;
            markingPlayer.hasNocturnalPredatorsSight = true;
            markingPlayer.hasInfernalExecutionersSight = true;
            
            // Fusion flag
            markingPlayer.hasStarfallExecutionersScope = true;
            
            // Dual-theme particle mix
            if (!hideVisual && Main.rand.NextBool(6))
            {
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                Vector2 vel = Main.rand.NextVector2Circular(0.6f, 0.6f);
                
                // Alternate between stellar and infernal
                if (Main.rand.NextBool())
                {
                    CustomParticles.GenericGlow(pos, vel, NachtmusikPurple * 0.6f, 0.25f, 20, true);
                }
                else
                {
                    CustomParticles.GenericGlow(pos, vel + Vector2.UnitY * -0.5f, DiesIraeCrimson * 0.6f, 0.28f, 18, true);
                }
            }
            
            // Orbiting fusion points
            if (!hideVisual && Main.rand.NextBool(15))
            {
                float angle = Main.GameUpdateCount * 0.03f;
                for (int i = 0; i < 3; i++)
                {
                    float orbitAngle = angle + MathHelper.TwoPi * i / 3f;
                    Vector2 orbitPos = player.Center + orbitAngle.ToRotationVector2() * 38f;
                    Color orbitColor = i % 2 == 0 ? NachtmusikPurple : DiesIraeCrimson;
                    CustomParticles.GenericFlare(orbitPos, orbitColor * 0.5f, 0.18f, 12);
                }
            }
            
            // Combined glow
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.035f) * 0.1f + 0.25f;
            Color lightColor = Color.Lerp(NachtmusikPurple, DiesIraeCrimson, (float)Math.Sin(Main.GameUpdateCount * 0.02f) * 0.5f + 0.5f);
            Lighting.AddLight(player.Center, lightColor.ToVector3() * pulse);
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Fusion", "⚔ STARFALL EXECUTIONER FUSION ⚔")
            {
                OverrideColor = FusionGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "System", "Marked for Death System - COSMIC JUDGMENT")
            {
                OverrideColor = NachtmusikPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Combines Nocturnal Predator's Sight and Infernal Executioner's Sight")
            {
                OverrideColor = DiesIraeCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Mark up to 14 enemies with stellar-infernal marks")
            {
                OverrideColor = FusionGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Star showers trigger hellfire explosions on impact")
            {
                OverrideColor = NachtmusikPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Judgment stacks build faster at night (+50%)")
            {
                OverrideColor = DiesIraeCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Inherits ALL abilities from both component accessories")
            {
                OverrideColor = new Color(200, 180, 220)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Starfall and hellfire unite in cosmic judgment'")
            {
                OverrideColor = new Color(180, 120, 160)
            });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<NocturnalPredatorsSight>(1)
                .AddIngredient<InfernalExecutionersSight>(1)
                .AddIngredient<NachtmusikResonantEnergy>(10)
                .AddIngredient<DiesIraeResonantEnergy>(10)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    #endregion
    
    #region Fusion Tier 2: Triumphant Verdict Scope (+ Ode to Joy)
    
    /// <summary>
    /// Fusion Tier 2 Ranger accessory - adds Ode to Joy to the fusion.
    /// Combines stellar precision, hellfire judgment, and nature's blessing.
    /// </summary>
    public class TriumphantVerdictScope : ModItem
    {
        private static readonly Color NachtmusikPurple = new Color(100, 80, 180);
        private static readonly Color DiesIraeCrimson = new Color(200, 50, 50);
        private static readonly Color OdeToJoyWhite = new Color(255, 255, 255);
        private static readonly Color FusionTriumph = new Color(255, 220, 160);
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 160);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
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
            markingPlayer.hasFatesCosmicVerdict = true;
            markingPlayer.hasNocturnalPredatorsSight = true;
            markingPlayer.hasInfernalExecutionersSight = true;
            markingPlayer.hasJubilantHuntersSight = true;
            markingPlayer.hasStarfallExecutionersScope = true;
            
            // Fusion flag
            markingPlayer.hasTriumphantVerdictScope = true;
            
            // Triple-theme particle mix
            if (!hideVisual && Main.rand.NextBool(5))
            {
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(38f, 38f);
                Vector2 vel = Main.rand.NextVector2Circular(0.6f, 0.6f);
                
                // Cycle through three themes
                int theme = Main.rand.Next(3);
                Color themeColor = theme switch
                {
                    0 => NachtmusikPurple,
                    1 => DiesIraeCrimson,
                    _ => OdeToJoyWhite
                };
                CustomParticles.GenericGlow(pos, vel, themeColor * 0.6f, 0.26f, 20, true);
            }
            
            // Rainbow rose petals
            if (!hideVisual && Main.rand.NextBool(12))
            {
                float hue = Main.rand.NextFloat();
                Color petalColor = Main.hslToRgb(hue, 0.5f, 0.8f);
                Vector2 petalPos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                Vector2 petalVel = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), Main.rand.NextFloat(-0.5f, 0.5f));
                CustomParticles.GenericGlow(petalPos, petalVel, petalColor * 0.6f, 0.2f, 25, true);
            }
            
            // Orbiting triple points
            if (!hideVisual && Main.rand.NextBool(18))
            {
                float angle = Main.GameUpdateCount * 0.025f;
                for (int i = 0; i < 3; i++)
                {
                    float orbitAngle = angle + MathHelper.TwoPi * i / 3f;
                    Vector2 orbitPos = player.Center + orbitAngle.ToRotationVector2() * 42f;
                    Color[] colors = { NachtmusikPurple, DiesIraeCrimson, OdeToJoyWhite };
                    CustomParticles.GenericFlare(orbitPos, colors[i] * 0.5f, 0.18f, 12);
                }
            }
            
            // Triumphant glow
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.03f) * 0.1f + 0.28f;
            float hueShift = (Main.GameUpdateCount * 0.008f) % 1f;
            Color lightColor = Main.hslToRgb(hueShift, 0.4f, 0.7f);
            Lighting.AddLight(player.Center, lightColor.ToVector3() * pulse);
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Fusion", "⚔ TRIUMPHANT VERDICT FUSION ⚔")
            {
                OverrideColor = FusionTriumph
            });
            
            tooltips.Add(new TooltipLine(Mod, "System", "Marked for Death System - TRIPLE SYMPHONY")
            {
                OverrideColor = NachtmusikPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Combines Starfall Executioner's Scope with Jubilant Hunter's Sight")
            {
                OverrideColor = DiesIraeCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Mark up to 16 enemies with triple-symphony marks")
            {
                OverrideColor = OdeToJoyWhite
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Healing orbs explode into star showers and hellfire")
            {
                OverrideColor = FusionTriumph
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Nature's Bounty projectiles leave burning starlight trails")
            {
                OverrideColor = NachtmusikPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Inherits ALL abilities from all three theme accessories")
            {
                OverrideColor = new Color(200, 180, 220)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Three symphonies unite in triumphant harmony'")
            {
                OverrideColor = new Color(220, 200, 180)
            });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<StarfallExecutionersScope>(1)
                .AddIngredient<JubilantHuntersSight>(1)
                .AddIngredient<OdeToJoyResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    #endregion
    
    #region Fusion Tier 3: Scope of the Eternal Verdict (Ultimate - + Clair de Lune)
    
    /// <summary>
    /// Ultimate Fusion Ranger accessory - all four Post-Fate themes combined.
    /// The pinnacle of the ranger marking system.
    /// </summary>
    public class ScopeOfTheEternalVerdict : ModItem
    {
        private static readonly Color NachtmusikPurple = new Color(100, 80, 180);
        private static readonly Color DiesIraeCrimson = new Color(200, 50, 50);
        private static readonly Color OdeToJoyWhite = new Color(255, 255, 255);
        private static readonly Color ClairDeLuneBrass = new Color(200, 170, 100);
        private static readonly Color UltimatePrismatic = new Color(255, 230, 200);
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 200);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
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
            markingPlayer.hasFatesCosmicVerdict = true;
            markingPlayer.hasNocturnalPredatorsSight = true;
            markingPlayer.hasInfernalExecutionersSight = true;
            markingPlayer.hasJubilantHuntersSight = true;
            markingPlayer.hasEternalVerdictSight = true;
            markingPlayer.hasStarfallExecutionersScope = true;
            markingPlayer.hasTriumphantVerdictScope = true;
            
            // Ultimate flag
            markingPlayer.hasScopeOfTheEternalVerdict = true;
            
            // Quad-theme particle spectacle
            if (!hideVisual && Main.rand.NextBool(4))
            {
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(40f, 40f);
                Vector2 vel = Main.rand.NextVector2Circular(0.7f, 0.7f);
                
                // Cycle through all four themes
                int theme = Main.rand.Next(4);
                Color themeColor = theme switch
                {
                    0 => NachtmusikPurple,
                    1 => DiesIraeCrimson,
                    2 => OdeToJoyWhite,
                    _ => ClairDeLuneBrass
                };
                CustomParticles.GenericGlow(pos, vel, themeColor * 0.65f, 0.28f, 22, true);
            }
            
            // Clockwork gears
            if (!hideVisual && Main.rand.NextBool(12))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 gearPos = player.Center + angle.ToRotationVector2() * Main.rand.NextFloat(30f, 50f);
                CustomParticles.Glyph(gearPos, ClairDeLuneBrass * 0.4f, 0.22f, -1);
            }
            
            // Shattered glass
            if (!hideVisual && Main.rand.NextBool(18))
            {
                Vector2 shardPos = player.Center + new Vector2(Main.rand.NextFloat(-40f, 40f), Main.rand.NextFloat(-45f, 15f));
                Vector2 shardVel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(0.4f, 1.2f));
                CustomParticles.GenericFlare(shardPos, UltimatePrismatic * 0.4f, 0.15f, 20);
            }
            
            // Ultimate orbiting quad
            if (!hideVisual && Main.rand.NextBool(20))
            {
                float angle = Main.GameUpdateCount * 0.02f;
                Color[] colors = { NachtmusikPurple, DiesIraeCrimson, OdeToJoyWhite, ClairDeLuneBrass };
                for (int i = 0; i < 4; i++)
                {
                    float orbitAngle = angle + MathHelper.TwoPi * i / 4f;
                    Vector2 orbitPos = player.Center + orbitAngle.ToRotationVector2() * 48f;
                    CustomParticles.GenericFlare(orbitPos, colors[i] * 0.5f, 0.2f, 14);
                }
            }
            
            // Eternal music
            if (!hideVisual && Main.rand.NextBool(14))
            {
                Color[] noteColors = { NachtmusikPurple, DiesIraeCrimson, OdeToJoyWhite, ClairDeLuneBrass };
                Color noteColor = noteColors[Main.rand.Next(4)];
                ThemedParticles.MusicNote(player.Center + Main.rand.NextVector2Circular(35f, 35f),
                    Vector2.UnitY * -0.4f, noteColor * 0.6f, 0.6f, 30);
            }
            
            // Ultimate prismatic glow with temporal shimmer
            float timeShift = Main.GameUpdateCount * 0.012f;
            float pulse = (float)Math.Sin(timeShift * 2f) * 0.12f + 0.35f;
            float hue = (timeShift * 0.5f) % 1f;
            Color lightColor = Main.hslToRgb(hue, 0.5f, 0.8f);
            Lighting.AddLight(player.Center, lightColor.ToVector3() * pulse);
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Ultimate", "✦✦✦ ETERNAL VERDICT - ULTIMATE FUSION ✦✦✦")
            {
                OverrideColor = UltimatePrismatic
            });
            
            tooltips.Add(new TooltipLine(Mod, "System", "Marked for Death System - GRAND SYMPHONY")
            {
                OverrideColor = ClairDeLuneBrass
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Combines ALL four Post-Fate theme accessories")
            {
                OverrideColor = NachtmusikPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Mark up to 20 enemies with eternal symphony marks")
            {
                OverrideColor = DiesIraeCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Temporal marks create starfall-hellfire-nature chains")
            {
                OverrideColor = OdeToJoyWhite
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Temporal Judgment triggers all theme death effects simultaneously")
            {
                OverrideColor = ClairDeLuneBrass
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Marks persist across dimensions and through time itself")
            {
                OverrideColor = UltimatePrismatic
            });
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Masters ALL abilities from the complete Post-Fate arsenal")
            {
                OverrideColor = new Color(220, 200, 240)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The eternal verdict echoes through all of existence'")
            {
                OverrideColor = new Color(200, 180, 160)
            });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<TriumphantVerdictScope>(1)
                .AddIngredient<EternalVerdictSight>(1)
                .AddIngredient<ClairDeLuneResonantEnergy>(20)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    #endregion
}
