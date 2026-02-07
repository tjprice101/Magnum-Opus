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

namespace MagnumOpus.Content.Common.Accessories.MageChain
{
    #region T7: Nocturnal Harmonic Overflow (Nachtmusik Theme)
    
    /// <summary>
    /// T7 Mage accessory - Nachtmusik theme (post-Fate).
    /// Starlight harmonics amplify overflow power.
    /// -250 overflow, +10% magic damage at night, spell echoes.
    /// </summary>
    public class NocturnalHarmonicOverflow : ModItem
    {
        public override string Texture => "MagnumOpus/Content/Common/Accessories/MageChain/NocturnalOverflowStar";
        
        private static readonly Color NachtmusikPurple = new Color(100, 80, 180);
        private static readonly Color NachtmusikGold = new Color(255, 215, 140);
        private static readonly Color NachtmusikSilver = new Color(200, 210, 230);
        
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 85);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var overflowPlayer = player.GetModPlayer<OverflowPlayer>();
            
            // Enable all previous tiers
            overflowPlayer.hasResonantOverflowGem = true;
            overflowPlayer.hasSpringArcaneConduit = true;
            overflowPlayer.hasSolarManaCrucible = true;
            overflowPlayer.hasHarvestSoulVessel = true;
            overflowPlayer.hasPermafrostVoidHeart = true;
            overflowPlayer.hasVivaldisHarmonicCore = true;
            overflowPlayer.hasMoonlitOverflowStar = true;
            overflowPlayer.hasHeroicArcaneSurge = true;
            overflowPlayer.hasInfernalManaInferno = true;
            overflowPlayer.hasEnigmasNegativeSpace = true;
            overflowPlayer.hasSwansBalancedFlow = true;
            overflowPlayer.hasFatesCosmicReservoir = true;
            
            // T7 flag
            overflowPlayer.hasNocturnalHarmonicOverflow = true;
            
            // Night damage bonus
            if (!Main.dayTime)
            {
                player.GetDamage(DamageClass.Magic) += 0.10f;
            }
            
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
            
            // Orbiting constellation points at night
            if (!hideVisual && !Main.dayTime && Main.rand.NextBool(20))
            {
                float angle = Main.GameUpdateCount * 0.025f;
                for (int i = 0; i < 5; i++)
                {
                    float starAngle = angle + MathHelper.TwoPi * i / 5f;
                    Vector2 starPos = player.Center + starAngle.ToRotationVector2() * 40f;
                    CustomParticles.GenericFlare(starPos, NachtmusikGold * 0.5f, 0.18f, 10);
                }
            }
            
            // Music notes
            if (!hideVisual && Main.rand.NextBool(15))
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
            tooltips.Add(new TooltipLine(Mod, "System", "Harmonic Overflow System - STELLAR HARMONICS")
            {
                OverrideColor = NachtmusikPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Can overflow to -250 mana")
            {
                OverrideColor = NachtmusikGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "At night: +10% magic damage")
            {
                OverrideColor = NachtmusikPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Spells create stellar echoes (30% damage copy)")
            {
                OverrideColor = NachtmusikSilver
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Echo strikes leave constellation trails")
            {
                OverrideColor = NachtmusikGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Inherits ALL previous overflow abilities")
            {
                OverrideColor = new Color(200, 180, 220)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The night sky sings through your spellwork'")
            {
                OverrideColor = new Color(140, 120, 180)
            });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<FatesCosmicReservoir>(1)
                .AddIngredient<NachtmusikResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    #endregion
    
    #region T8: Infernal Mana Cataclysm (Dies Irae Theme)
    
    /// <summary>
    /// T8 Mage accessory - Dies Irae theme (post-Fate).
    /// Hellfire fuels the overflow with destructive power.
    /// -300 overflow, bonus damage converts to DoT, +50% mana potion healing, fire orbs.
    /// </summary>
    public class InfernalManaCataclysm : ModItem
    {
        public override string Texture => "MagnumOpus/Content/Common/Accessories/MageChain/InfernalManaInferno";
        
        private static readonly Color DiesIraeCrimson = new Color(200, 50, 50);
        private static readonly Color DiesIraeOrange = new Color(255, 120, 40);
        private static readonly Color DiesIraeBlack = new Color(30, 20, 25);
        
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 95);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var overflowPlayer = player.GetModPlayer<OverflowPlayer>();
            
            // Enable all previous tiers
            overflowPlayer.hasResonantOverflowGem = true;
            overflowPlayer.hasSpringArcaneConduit = true;
            overflowPlayer.hasSolarManaCrucible = true;
            overflowPlayer.hasHarvestSoulVessel = true;
            overflowPlayer.hasPermafrostVoidHeart = true;
            overflowPlayer.hasVivaldisHarmonicCore = true;
            overflowPlayer.hasMoonlitOverflowStar = true;
            overflowPlayer.hasHeroicArcaneSurge = true;
            overflowPlayer.hasInfernalManaInferno = true;
            overflowPlayer.hasEnigmasNegativeSpace = true;
            overflowPlayer.hasSwansBalancedFlow = true;
            overflowPlayer.hasFatesCosmicReservoir = true;
            overflowPlayer.hasNocturnalHarmonicOverflow = true;
            
            // T8 flag
            overflowPlayer.hasInfernalManaCataclysm = true;
            
            // +50% mana potion healing
            player.manaSickReduction *= 0.5f; // Less mana sickness = more effective healing
            
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
            
            // Infernal glow
            float flicker = Main.rand.NextFloat(0.8f, 1f);
            Lighting.AddLight(player.Center, DiesIraeOrange.ToVector3() * 0.3f * flicker);
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "System", "Harmonic Overflow System - INFERNAL CATACLYSM")
            {
                OverrideColor = DiesIraeCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Can overflow to -300 mana")
            {
                OverrideColor = DiesIraeOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "10% of magic damage bonus applies as burn DoT")
            {
                OverrideColor = DiesIraeCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Mana potions heal 50% more effectively")
            {
                OverrideColor = DiesIraeOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect4", "While in overflow: Summon orbiting fire orbs that attack enemies")
            {
                OverrideColor = DiesIraeCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Inherits ALL previous overflow abilities")
            {
                OverrideColor = new Color(200, 180, 220)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Hellfire consumes the void, forging destruction anew'")
            {
                OverrideColor = new Color(180, 100, 80)
            });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<NocturnalHarmonicOverflow>(1)
                .AddIngredient<DiesIraeResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    #endregion
    
    #region T9: Jubilant Arcane Celebration (Ode to Joy Theme)
    
    /// <summary>
    /// T9 Mage accessory - Ode to Joy theme (post-Fate).
    /// Joy flows through the mana streams.
    /// -350 overflow, heals 1% max HP per 100 mana spent, +5% crit on full recovery.
    /// </summary>
    public class JubilantArcaneCelebration : ModItem
    {
        public override string Texture => "MagnumOpus/Content/Common/Accessories/MageChain/JubilantOverflowBlossom";
        
        private static readonly Color OdeToJoyWhite = new Color(255, 255, 255);
        private static readonly Color OdeToJoyIridescent = new Color(220, 200, 255);
        private static readonly Color OdeToJoyRose = new Color(255, 180, 200);
        
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 105);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var overflowPlayer = player.GetModPlayer<OverflowPlayer>();
            
            // Enable all previous tiers
            overflowPlayer.hasResonantOverflowGem = true;
            overflowPlayer.hasSpringArcaneConduit = true;
            overflowPlayer.hasSolarManaCrucible = true;
            overflowPlayer.hasHarvestSoulVessel = true;
            overflowPlayer.hasPermafrostVoidHeart = true;
            overflowPlayer.hasVivaldisHarmonicCore = true;
            overflowPlayer.hasMoonlitOverflowStar = true;
            overflowPlayer.hasHeroicArcaneSurge = true;
            overflowPlayer.hasInfernalManaInferno = true;
            overflowPlayer.hasEnigmasNegativeSpace = true;
            overflowPlayer.hasSwansBalancedFlow = true;
            overflowPlayer.hasFatesCosmicReservoir = true;
            overflowPlayer.hasNocturnalHarmonicOverflow = true;
            overflowPlayer.hasInfernalManaCataclysm = true;
            
            // T9 flag
            overflowPlayer.hasJubilantArcaneCelebration = true;
            
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
            
            // Prismatic glow
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.08f + 0.22f;
            float hueShift = (Main.GameUpdateCount * 0.01f) % 1f;
            Color lightColor = Main.hslToRgb(hueShift, 0.4f, 0.7f);
            Lighting.AddLight(player.Center, lightColor.ToVector3() * pulse);
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "System", "Harmonic Overflow System - JUBILANT CELEBRATION")
            {
                OverrideColor = OdeToJoyIridescent
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Can overflow to -350 mana")
            {
                OverrideColor = OdeToJoyWhite
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Heal 1% max HP for every 100 mana spent")
            {
                OverrideColor = OdeToJoyRose
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Full overflow recovery grants +5% magic crit for 10s")
            {
                OverrideColor = OdeToJoyIridescent
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Spells leave trails of celebrating light")
            {
                OverrideColor = OdeToJoyRose
            });
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Inherits ALL previous overflow abilities")
            {
                OverrideColor = new Color(200, 180, 220)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Joy flows through the very fabric of magic'")
            {
                OverrideColor = new Color(200, 220, 180)
            });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<InfernalManaCataclysm>(1)
                .AddIngredient<OdeToJoyResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    #endregion
    
    #region T10: Eternal Overflow Mastery (Clair de Lune Theme)
    
    /// <summary>
    /// T10 Mage accessory - Clair de Lune theme (post-Fate).
    /// Time itself bends to the master of overflow.
    /// -400 overflow, no damage penalty, 50% faster recovery, Temporal Overflow.
    /// </summary>
    public class EternalOverflowMastery : ModItem
    {
        public override string Texture => "MagnumOpus/Content/Common/Accessories/MageChain/EternalOverflowNexus";
        
        private static readonly Color ClairDeLuneGray = new Color(120, 110, 130);
        private static readonly Color ClairDeLuneBrass = new Color(200, 170, 100);
        private static readonly Color ClairDeLuneCrimson = new Color(180, 80, 100);
        private static readonly Color ClairDeLuneIridescent = new Color(180, 170, 200);
        
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 120);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var overflowPlayer = player.GetModPlayer<OverflowPlayer>();
            
            // Enable all previous tiers
            overflowPlayer.hasResonantOverflowGem = true;
            overflowPlayer.hasSpringArcaneConduit = true;
            overflowPlayer.hasSolarManaCrucible = true;
            overflowPlayer.hasHarvestSoulVessel = true;
            overflowPlayer.hasPermafrostVoidHeart = true;
            overflowPlayer.hasVivaldisHarmonicCore = true;
            overflowPlayer.hasMoonlitOverflowStar = true;
            overflowPlayer.hasHeroicArcaneSurge = true;
            overflowPlayer.hasInfernalManaInferno = true;
            overflowPlayer.hasEnigmasNegativeSpace = true;
            overflowPlayer.hasSwansBalancedFlow = true;
            overflowPlayer.hasFatesCosmicReservoir = true;
            overflowPlayer.hasNocturnalHarmonicOverflow = true;
            overflowPlayer.hasInfernalManaCataclysm = true;
            overflowPlayer.hasJubilantArcaneCelebration = true;
            
            // T10 flag
            overflowPlayer.hasEternalOverflowMastery = true;
            
            // Clockwork gear particles
            if (!hideVisual && Main.rand.NextBool(10))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 gearPos = player.Center + angle.ToRotationVector2() * Main.rand.NextFloat(25f, 45f);
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
            
            // Eternal music notes
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
            tooltips.Add(new TooltipLine(Mod, "System", "Harmonic Overflow System - ETERNAL MASTERY")
            {
                OverrideColor = ClairDeLuneCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Ultimate", "✦✦✦ ULTIMATE MAGE ACCESSORY ✦✦✦")
            {
                OverrideColor = ClairDeLuneBrass
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Can overflow to -400 mana")
            {
                OverrideColor = ClairDeLuneIridescent
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "No damage penalty while in overflow (mastered!)")
            {
                OverrideColor = ClairDeLuneCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Overflow recovers 50% faster")
            {
                OverrideColor = ClairDeLuneBrass
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Temporal Overflow: Go beyond -400 for 3s once per minute")
            {
                OverrideColor = ClairDeLuneIridescent
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect5", "During Temporal Overflow: Spells cost no mana")
            {
                OverrideColor = ClairDeLuneCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Inherits ALL previous overflow abilities")
            {
                OverrideColor = new Color(200, 180, 220)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Time itself flows through the eternal void'")
            {
                OverrideColor = new Color(160, 140, 180)
            });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<JubilantArcaneCelebration>(1)
                .AddIngredient<ClairDeLuneResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    #endregion
    
    #region Fusion Tier 1: Starfall Harmonic Pendant (Nachtmusik + Dies Irae)
    
    /// <summary>
    /// Fusion Tier 1 Mage accessory - combines Nachtmusik and Dies Irae.
    /// Stellar fire merges with infernal void.
    /// </summary>
    public class StarfallHarmonicPendant : ModItem
    {
        public override string Texture => "MagnumOpus/Content/Common/Accessories/MageChain/StarfallCruciblePendant";
        
        private static readonly Color NachtmusikPurple = new Color(100, 80, 180);
        private static readonly Color DiesIraeCrimson = new Color(200, 50, 50);
        private static readonly Color FusionGold = new Color(255, 180, 80);
        
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 130);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var overflowPlayer = player.GetModPlayer<OverflowPlayer>();
            
            // Enable all previous tiers
            overflowPlayer.hasResonantOverflowGem = true;
            overflowPlayer.hasSpringArcaneConduit = true;
            overflowPlayer.hasSolarManaCrucible = true;
            overflowPlayer.hasHarvestSoulVessel = true;
            overflowPlayer.hasPermafrostVoidHeart = true;
            overflowPlayer.hasVivaldisHarmonicCore = true;
            overflowPlayer.hasMoonlitOverflowStar = true;
            overflowPlayer.hasHeroicArcaneSurge = true;
            overflowPlayer.hasInfernalManaInferno = true;
            overflowPlayer.hasEnigmasNegativeSpace = true;
            overflowPlayer.hasSwansBalancedFlow = true;
            overflowPlayer.hasFatesCosmicReservoir = true;
            overflowPlayer.hasNocturnalHarmonicOverflow = true;
            overflowPlayer.hasInfernalManaCataclysm = true;
            
            // Fusion flag
            overflowPlayer.hasStarfallHarmonicPendant = true;
            
            // Enhanced night bonus from fusion
            if (!Main.dayTime)
            {
                player.GetDamage(DamageClass.Magic) += 0.15f; // Enhanced from 10%
            }
            
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
            tooltips.Add(new TooltipLine(Mod, "Fusion", "⚔ STARFALL HARMONIC FUSION ⚔")
            {
                OverrideColor = FusionGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "System", "Harmonic Overflow System - COSMIC INFERNO")
            {
                OverrideColor = NachtmusikPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Combines Nocturnal Harmonic Overflow and Infernal Mana Cataclysm")
            {
                OverrideColor = DiesIraeCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Can overflow to -300 mana")
            {
                OverrideColor = FusionGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "At night: +15% magic damage (enhanced)")
            {
                OverrideColor = NachtmusikPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Stellar echoes explode with hellfire on impact")
            {
                OverrideColor = DiesIraeCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Inherits ALL abilities from both component accessories")
            {
                OverrideColor = new Color(200, 180, 220)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Starfall and hellfire merge in harmonic destruction'")
            {
                OverrideColor = new Color(180, 120, 160)
            });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<NocturnalHarmonicOverflow>(1)
                .AddIngredient<InfernalManaCataclysm>(1)
                .AddIngredient<NachtmusikResonantEnergy>(10)
                .AddIngredient<DiesIraeResonantEnergy>(10)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    #endregion
    
    #region Fusion Tier 2: Triumphant Overflow Pendant (+ Ode to Joy)
    
    /// <summary>
    /// Fusion Tier 2 Mage accessory - adds Ode to Joy to the fusion.
    /// Triple harmony of stellar, infernal, and jubilant power.
    /// </summary>
    public class TriumphantOverflowPendant : ModItem
    {
        private static readonly Color NachtmusikPurple = new Color(100, 80, 180);
        private static readonly Color DiesIraeCrimson = new Color(200, 50, 50);
        private static readonly Color OdeToJoyWhite = new Color(255, 255, 255);
        private static readonly Color FusionTriumph = new Color(255, 220, 160);
        
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 160);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var overflowPlayer = player.GetModPlayer<OverflowPlayer>();
            
            // Enable all previous tiers
            overflowPlayer.hasResonantOverflowGem = true;
            overflowPlayer.hasSpringArcaneConduit = true;
            overflowPlayer.hasSolarManaCrucible = true;
            overflowPlayer.hasHarvestSoulVessel = true;
            overflowPlayer.hasPermafrostVoidHeart = true;
            overflowPlayer.hasVivaldisHarmonicCore = true;
            overflowPlayer.hasMoonlitOverflowStar = true;
            overflowPlayer.hasHeroicArcaneSurge = true;
            overflowPlayer.hasInfernalManaInferno = true;
            overflowPlayer.hasEnigmasNegativeSpace = true;
            overflowPlayer.hasSwansBalancedFlow = true;
            overflowPlayer.hasFatesCosmicReservoir = true;
            overflowPlayer.hasNocturnalHarmonicOverflow = true;
            overflowPlayer.hasInfernalManaCataclysm = true;
            overflowPlayer.hasJubilantArcaneCelebration = true;
            overflowPlayer.hasStarfallHarmonicPendant = true;
            
            // Fusion flag
            overflowPlayer.hasTriumphantOverflowPendant = true;
            
            // Enhanced night bonus
            if (!Main.dayTime)
            {
                player.GetDamage(DamageClass.Magic) += 0.18f;
            }
            
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
            tooltips.Add(new TooltipLine(Mod, "Fusion", "⚔ TRIUMPHANT OVERFLOW FUSION ⚔")
            {
                OverrideColor = FusionTriumph
            });
            
            tooltips.Add(new TooltipLine(Mod, "System", "Harmonic Overflow System - TRIPLE SYMPHONY")
            {
                OverrideColor = NachtmusikPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Combines Starfall Harmonic Pendant with Jubilant Arcane Celebration")
            {
                OverrideColor = DiesIraeCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Can overflow to -350 mana")
            {
                OverrideColor = OdeToJoyWhite
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Healing from spellcasting triggers stellar-infernal bursts")
            {
                OverrideColor = FusionTriumph
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Recovery crit bonus enhanced to +8%")
            {
                OverrideColor = NachtmusikPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Inherits ALL abilities from all three theme accessories")
            {
                OverrideColor = new Color(200, 180, 220)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Three harmonies unite in triumphant overflow'")
            {
                OverrideColor = new Color(220, 200, 180)
            });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<StarfallHarmonicPendant>(1)
                .AddIngredient<JubilantArcaneCelebration>(1)
                .AddIngredient<OdeToJoyResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    #endregion
    
    #region Fusion Tier 3: Pendant of the Eternal Overflow (Ultimate - + Clair de Lune)
    
    /// <summary>
    /// Ultimate Fusion Mage accessory - all four Post-Fate themes combined.
    /// The pinnacle of the mage overflow system.
    /// </summary>
    public class PendantOfTheEternalOverflow : ModItem
    {
        private static readonly Color NachtmusikPurple = new Color(100, 80, 180);
        private static readonly Color DiesIraeCrimson = new Color(200, 50, 50);
        private static readonly Color OdeToJoyWhite = new Color(255, 255, 255);
        private static readonly Color ClairDeLuneBrass = new Color(200, 170, 100);
        private static readonly Color UltimatePrismatic = new Color(255, 230, 200);
        
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 200);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var overflowPlayer = player.GetModPlayer<OverflowPlayer>();
            
            // Enable all previous tiers
            overflowPlayer.hasResonantOverflowGem = true;
            overflowPlayer.hasSpringArcaneConduit = true;
            overflowPlayer.hasSolarManaCrucible = true;
            overflowPlayer.hasHarvestSoulVessel = true;
            overflowPlayer.hasPermafrostVoidHeart = true;
            overflowPlayer.hasVivaldisHarmonicCore = true;
            overflowPlayer.hasMoonlitOverflowStar = true;
            overflowPlayer.hasHeroicArcaneSurge = true;
            overflowPlayer.hasInfernalManaInferno = true;
            overflowPlayer.hasEnigmasNegativeSpace = true;
            overflowPlayer.hasSwansBalancedFlow = true;
            overflowPlayer.hasFatesCosmicReservoir = true;
            overflowPlayer.hasNocturnalHarmonicOverflow = true;
            overflowPlayer.hasInfernalManaCataclysm = true;
            overflowPlayer.hasJubilantArcaneCelebration = true;
            overflowPlayer.hasEternalOverflowMastery = true;
            overflowPlayer.hasStarfallHarmonicPendant = true;
            overflowPlayer.hasTriumphantOverflowPendant = true;
            
            // Ultimate flag
            overflowPlayer.hasPendantOfTheEternalOverflow = true;
            
            // Always has night bonus (mastered)
            player.GetDamage(DamageClass.Magic) += 0.20f;
            
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
            tooltips.Add(new TooltipLine(Mod, "Ultimate", "✦✦✦ ETERNAL OVERFLOW - ULTIMATE FUSION ✦✦✦")
            {
                OverrideColor = UltimatePrismatic
            });
            
            tooltips.Add(new TooltipLine(Mod, "System", "Harmonic Overflow System - GRAND SYMPHONY")
            {
                OverrideColor = ClairDeLuneBrass
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Combines ALL four Post-Fate theme accessories")
            {
                OverrideColor = NachtmusikPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Can overflow to -400 mana with no penalties")
            {
                OverrideColor = DiesIraeCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+20% magic damage always (night mastered)")
            {
                OverrideColor = OdeToJoyWhite
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Eternal Temporal Overflow: Unlimited overflow for 5s (90s CD)")
            {
                OverrideColor = ClairDeLuneBrass
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect5", "All spell effects trigger simultaneously across time")
            {
                OverrideColor = UltimatePrismatic
            });
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Masters ALL abilities from the complete Post-Fate arsenal")
            {
                OverrideColor = new Color(220, 200, 240)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The eternal overflow echoes through all of existence'")
            {
                OverrideColor = new Color(200, 180, 160)
            });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<TriumphantOverflowPendant>(1)
                .AddIngredient<EternalOverflowMastery>(1)
                .AddIngredient<ClairDeLuneResonantEnergy>(20)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    #endregion
}
