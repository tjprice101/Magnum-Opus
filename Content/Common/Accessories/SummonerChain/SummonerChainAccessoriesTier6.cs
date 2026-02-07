using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Materials;
using MagnumOpus.Common;
using MagnumOpus.Content.ClairDeLune.ResonanceEnergies;
using MagnumOpus.Content.DiesIrae.ResonanceEnergies;
using MagnumOpus.Content.Nachtmusik.ResonanceEnergies;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;

namespace MagnumOpus.Content.Common.Accessories.SummonerChain
{
    #region T7: Nocturnal Maestro's Baton (Nachtmusik Theme)
    
    /// <summary>
    /// T7 Summoner accessory - Nachtmusik theme (post-Fate).
    /// Starlight empowers the conductor's commands.
    /// Minions gain +15% damage at night, constellation formations.
    /// </summary>
    public class NocturnalMaestrosBaton : ModItem
    {
        public override string Texture => "MagnumOpus/Content/Common/Accessories/SummonerChain/NocturnalConductorsWand";
        
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
            var conductor = player.GetModPlayer<ConductorPlayer>();
            
            // Inherit all previous tier abilities
            conductor.HasConductorsWand = true;
            conductor.HasSpringMaestrosBadge = true;
            conductor.HasSolarDirectorsCrest = true;
            conductor.HasHarvestBeastlordsHorn = true;
            conductor.HasPermafrostCommandersCrown = true;
            conductor.HasVivaldisOrchestraBaton = true;
            conductor.HasMoonlitSymphonyWand = true;
            conductor.HasHeroicGeneralsBaton = true;
            conductor.HasInfernalChoirMastersRod = true;
            conductor.HasEnigmasHivemindLink = true;
            conductor.HasSwansGracefulDirection = true;
            conductor.HasFatesCosmicDominion = true;
            
            // T7 flag
            conductor.HasNocturnalMaestrosBaton = true;
            
            // Night minion damage bonus
            if (!Main.dayTime)
            {
                player.GetDamage(DamageClass.Summon) += 0.15f;
            }
            
            // Constellation particles
            if (!hideVisual && Main.rand.NextBool(12))
            {
                float angle = Main.GameUpdateCount * 0.025f + Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 dustPos = player.Center + angle.ToRotationVector2() * Main.rand.NextFloat(28f, 45f);
                
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.GoldCoin, Vector2.Zero);
                dust.noGravity = true;
                dust.scale = 0.6f;
                dust.color = NachtmusikGold;
            }
            
            // Star twinkles at night
            if (!hideVisual && !Main.dayTime && Main.rand.NextBool(25))
            {
                Vector2 starPos = player.Center + Main.rand.NextVector2Circular(40f, 40f);
                Dust star = Dust.NewDustPerfect(starPos, DustID.MagicMirror, Vector2.Zero);
                star.noGravity = true;
                star.scale = 0.5f;
                star.color = NachtmusikSilver;
            }
            
            // Stellar lighting
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.03f) * 0.08f + 0.22f;
            Lighting.AddLight(player.Center, NachtmusikPurple.ToVector3() * pulse);
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "System", "Conductor's Baton System - STELLAR SYMPHONY")
            {
                OverrideColor = NachtmusikPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Right-click to conduct minions to focus target")
            {
                OverrideColor = NachtmusikGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "At night: Minions deal +15% damage")
            {
                OverrideColor = NachtmusikPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Conducted minions form constellation patterns")
            {
                OverrideColor = NachtmusikSilver
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Minions leave stellar trails at night")
            {
                OverrideColor = NachtmusikGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Inherits ALL previous conductor abilities")
            {
                OverrideColor = new Color(200, 180, 220)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The night sky conducts the starlight orchestra'")
            {
                OverrideColor = new Color(140, 120, 180)
            });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<FatesCosmicDominion>(1)
                .AddIngredient<NachtmusikResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    #endregion
    
    #region T8: Infernal Choirmaster's Scepter (Dies Irae Theme)
    
    /// <summary>
    /// T8 Summoner accessory - Dies Irae theme (post-Fate).
    /// Hellfire empowers minion attacks.
    /// Minions inflict burn, explosive death VFX, +20% damage to burning enemies.
    /// </summary>
    public class InfernalChoirmastersScepter : ModItem
    {
        public override string Texture => "MagnumOpus/Content/Common/Accessories/SummonerChain/InfernalChoirMastersWand";
        
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
            var conductor = player.GetModPlayer<ConductorPlayer>();
            
            // Inherit all previous tier abilities
            conductor.HasConductorsWand = true;
            conductor.HasSpringMaestrosBadge = true;
            conductor.HasSolarDirectorsCrest = true;
            conductor.HasHarvestBeastlordsHorn = true;
            conductor.HasPermafrostCommandersCrown = true;
            conductor.HasVivaldisOrchestraBaton = true;
            conductor.HasMoonlitSymphonyWand = true;
            conductor.HasHeroicGeneralsBaton = true;
            conductor.HasInfernalChoirMastersRod = true;
            conductor.HasEnigmasHivemindLink = true;
            conductor.HasSwansGracefulDirection = true;
            conductor.HasFatesCosmicDominion = true;
            conductor.HasNocturnalMaestrosBaton = true;
            
            // T8 flag
            conductor.HasInfernalChoirmastersScepter = true;
            
            // Infernal particles
            if (!hideVisual && Main.rand.NextBool(10))
            {
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-1f, -0.3f));
                
                Dust dust = Dust.NewDustPerfect(pos, DustID.Torch, vel);
                dust.noGravity = true;
                dust.scale = 0.8f;
                dust.color = DiesIraeOrange;
            }
            
            // Rising embers
            if (!hideVisual && Main.rand.NextBool(18))
            {
                Vector2 emberPos = player.Center + new Vector2(Main.rand.NextFloat(-20f, 20f), 15f);
                Dust ember = Dust.NewDustPerfect(emberPos, DustID.FlameBurst, Vector2.UnitY * -1.2f);
                ember.noGravity = true;
                ember.scale = 0.5f;
            }
            
            // Infernal glow
            float flicker = Main.rand.NextFloat(0.75f, 1f);
            Lighting.AddLight(player.Center, DiesIraeOrange.ToVector3() * 0.25f * flicker);
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "System", "Conductor's Baton System - INFERNAL CHORUS")
            {
                OverrideColor = DiesIraeCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Right-click to conduct minions to focus target")
            {
                OverrideColor = DiesIraeOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Minions inflict Hellfire burn on hit (3% HP/s)")
            {
                OverrideColor = DiesIraeCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+20% minion damage to burning enemies")
            {
                OverrideColor = DiesIraeOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Minion attacks on conducted targets cause hellfire explosions")
            {
                OverrideColor = DiesIraeCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Inherits ALL previous conductor abilities")
            {
                OverrideColor = new Color(200, 180, 220)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The infernal choir sings destruction's hymn'")
            {
                OverrideColor = new Color(180, 100, 80)
            });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<NocturnalMaestrosBaton>(1)
                .AddIngredient<DiesIraeResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    #endregion
    
    #region T9: Jubilant Orchestra's Staff (Ode to Joy Theme)
    
    /// <summary>
    /// T9 Summoner accessory - Ode to Joy theme (post-Fate).
    /// Joy flows through minion attacks.
    /// Minion hits heal player, +10% attack speed, celebration VFX.
    /// </summary>
    public class JubilantOrchestrasStaff : ModItem
    {
        public override string Texture => "MagnumOpus/Content/Common/Accessories/SummonerChain/JubilantOrchestraWand";
        
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
            var conductor = player.GetModPlayer<ConductorPlayer>();
            
            // Inherit all previous tier abilities
            conductor.HasConductorsWand = true;
            conductor.HasSpringMaestrosBadge = true;
            conductor.HasSolarDirectorsCrest = true;
            conductor.HasHarvestBeastlordsHorn = true;
            conductor.HasPermafrostCommandersCrown = true;
            conductor.HasVivaldisOrchestraBaton = true;
            conductor.HasMoonlitSymphonyWand = true;
            conductor.HasHeroicGeneralsBaton = true;
            conductor.HasInfernalChoirMastersRod = true;
            conductor.HasEnigmasHivemindLink = true;
            conductor.HasSwansGracefulDirection = true;
            conductor.HasFatesCosmicDominion = true;
            conductor.HasNocturnalMaestrosBaton = true;
            conductor.HasInfernalChoirmastersScepter = true;
            
            // T9 flag
            conductor.HasJubilantOrchestrasStaff = true;
            
            // Minion attack speed bonus
            player.GetAttackSpeed(DamageClass.Summon) += 0.10f;
            
            // Iridescent particles
            if (!hideVisual && Main.rand.NextBool(10))
            {
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                float hue = (Main.GameUpdateCount * 0.015f + Main.rand.NextFloat()) % 1f;
                Color shimmerColor = Main.hslToRgb(hue, 0.5f, 0.85f);
                
                Dust dust = Dust.NewDustPerfect(pos, DustID.RainbowMk2, Main.rand.NextVector2Circular(0.5f, 0.5f));
                dust.noGravity = true;
                dust.scale = 0.6f;
                dust.color = shimmerColor;
            }
            
            // Rose petal particles
            if (!hideVisual && Main.rand.NextBool(15))
            {
                Vector2 petalPos = player.Center + Main.rand.NextVector2Circular(40f, 40f);
                Dust petal = Dust.NewDustPerfect(petalPos, DustID.PinkFairy, new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-0.3f, 0.3f)));
                petal.noGravity = true;
                petal.scale = 0.7f;
            }
            
            // Prismatic glow
            float hueShift = (Main.GameUpdateCount * 0.008f) % 1f;
            Color lightColor = Main.hslToRgb(hueShift, 0.3f, 0.7f);
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.06f + 0.2f;
            Lighting.AddLight(player.Center, lightColor.ToVector3() * pulse);
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "System", "Conductor's Baton System - JUBILANT SYMPHONY")
            {
                OverrideColor = OdeToJoyIridescent
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Right-click to conduct minions to focus target")
            {
                OverrideColor = OdeToJoyWhite
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Minion hits on conducted targets heal 1 HP")
            {
                OverrideColor = OdeToJoyRose
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+10% minion attack speed")
            {
                OverrideColor = OdeToJoyIridescent
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Celebrating minions spray joyful particles on kill")
            {
                OverrideColor = OdeToJoyRose
            });
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Inherits ALL previous conductor abilities")
            {
                OverrideColor = new Color(200, 180, 220)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Joy flows through the symphony of summoned spirits'")
            {
                OverrideColor = new Color(200, 220, 180)
            });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<InfernalChoirmastersScepter>(1)
                .AddIngredient<OdeToJoyResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    #endregion
    
    #region T10: Eternal Conductor's Scepter (Clair de Lune Theme)
    
    /// <summary>
    /// T10 Summoner accessory - Clair de Lune theme (post-Fate).
    /// Time itself bows to the conductor.
    /// Instant conduct cooldown reset on kill, Temporal Finale ability.
    /// </summary>
    public class EternalConductorsScepter : ModItem
    {
        public override string Texture => "MagnumOpus/Content/Common/Accessories/SummonerChain/EternalConductorsBaton";
        
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
            var conductor = player.GetModPlayer<ConductorPlayer>();
            
            // Inherit all previous tier abilities
            conductor.HasConductorsWand = true;
            conductor.HasSpringMaestrosBadge = true;
            conductor.HasSolarDirectorsCrest = true;
            conductor.HasHarvestBeastlordsHorn = true;
            conductor.HasPermafrostCommandersCrown = true;
            conductor.HasVivaldisOrchestraBaton = true;
            conductor.HasMoonlitSymphonyWand = true;
            conductor.HasHeroicGeneralsBaton = true;
            conductor.HasInfernalChoirMastersRod = true;
            conductor.HasEnigmasHivemindLink = true;
            conductor.HasSwansGracefulDirection = true;
            conductor.HasFatesCosmicDominion = true;
            conductor.HasNocturnalMaestrosBaton = true;
            conductor.HasInfernalChoirmastersScepter = true;
            conductor.HasJubilantOrchestrasStaff = true;
            
            // T10 flag
            conductor.HasEternalConductorsScepter = true;
            
            // Clockwork/temporal particles
            if (!hideVisual && Main.rand.NextBool(12))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 gearPos = player.Center + angle.ToRotationVector2() * Main.rand.NextFloat(25f, 45f);
                
                Dust dust = Dust.NewDustPerfect(gearPos, DustID.Enchanted_Gold, Vector2.Zero);
                dust.noGravity = true;
                dust.scale = 0.5f;
                dust.color = ClairDeLuneBrass;
            }
            
            // Temporal flame wisps
            if (!hideVisual && Main.rand.NextBool(15))
            {
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                Dust dust = Dust.NewDustPerfect(pos, DustID.PinkFairy, new Vector2(0, Main.rand.NextFloat(-0.5f, 0.2f)));
                dust.noGravity = true;
                dust.scale = 0.6f;
                dust.color = ClairDeLuneCrimson;
            }
            
            // Temporal glow
            float timeShift = Main.GameUpdateCount * 0.015f;
            float pulse = (float)Math.Sin(timeShift) * 0.08f + 0.22f;
            Color lightColor = Color.Lerp(ClairDeLuneCrimson, ClairDeLuneBrass, (float)Math.Sin(timeShift * 0.5f) * 0.5f + 0.5f);
            Lighting.AddLight(player.Center, lightColor.ToVector3() * pulse);
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "System", "Conductor's Baton System - ETERNAL COMMAND")
            {
                OverrideColor = ClairDeLuneCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Ultimate", "✦✦✦ ULTIMATE SUMMONER ACCESSORY ✦✦✦")
            {
                OverrideColor = ClairDeLuneBrass
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Right-click to conduct minions to focus target")
            {
                OverrideColor = ClairDeLuneIridescent
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Kill during Conduct: Instantly reset cooldown")
            {
                OverrideColor = ClairDeLuneCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Temporal Finale: Minions attack 3x faster for 2s (60s CD)")
            {
                OverrideColor = ClairDeLuneBrass
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Time freezes briefly when Finale is triggered")
            {
                OverrideColor = ClairDeLuneIridescent
            });
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Inherits ALL previous conductor abilities")
            {
                OverrideColor = new Color(200, 180, 220)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The eternal conductor commands time itself'")
            {
                OverrideColor = new Color(160, 140, 180)
            });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<JubilantOrchestrasStaff>(1)
                .AddIngredient<ClairDeLuneResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    #endregion
    
    #region Fusion Tier 1: Starfall Infernal Baton (Nachtmusik + Dies Irae)
    
    /// <summary>
    /// Fusion Tier 1 Summoner accessory - combines Nachtmusik and Dies Irae.
    /// Stellar fire empowers the conductor.
    /// </summary>
    public class StarfallInfernalBaton : ModItem
    {
        public override string Texture => "MagnumOpus/Content/Common/Accessories/SummonerChain/StarfallChoirBaton";
        
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
            var conductor = player.GetModPlayer<ConductorPlayer>();
            
            // Inherit all previous tier abilities
            conductor.HasConductorsWand = true;
            conductor.HasSpringMaestrosBadge = true;
            conductor.HasSolarDirectorsCrest = true;
            conductor.HasHarvestBeastlordsHorn = true;
            conductor.HasPermafrostCommandersCrown = true;
            conductor.HasVivaldisOrchestraBaton = true;
            conductor.HasMoonlitSymphonyWand = true;
            conductor.HasHeroicGeneralsBaton = true;
            conductor.HasInfernalChoirMastersRod = true;
            conductor.HasEnigmasHivemindLink = true;
            conductor.HasSwansGracefulDirection = true;
            conductor.HasFatesCosmicDominion = true;
            conductor.HasNocturnalMaestrosBaton = true;
            conductor.HasInfernalChoirmastersScepter = true;
            
            // Fusion flag
            conductor.HasStarfallInfernalBaton = true;
            
            // Enhanced night bonus from fusion
            if (!Main.dayTime)
            {
                player.GetDamage(DamageClass.Summon) += 0.20f; // Enhanced from 15%
            }
            
            // Dual-theme particles
            if (!hideVisual && Main.rand.NextBool(8))
            {
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                
                if (Main.rand.NextBool())
                {
                    // Stellar
                    Dust dust = Dust.NewDustPerfect(pos, DustID.GoldCoin, Vector2.Zero);
                    dust.noGravity = true;
                    dust.scale = 0.55f;
                    dust.color = NachtmusikPurple;
                }
                else
                {
                    // Infernal
                    Dust dust = Dust.NewDustPerfect(pos, DustID.Torch, Vector2.UnitY * -0.5f);
                    dust.noGravity = true;
                    dust.scale = 0.65f;
                    dust.color = DiesIraeCrimson;
                }
            }
            
            // Combined glow
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.035f) * 0.08f + 0.22f;
            Color lightColor = Color.Lerp(NachtmusikPurple, DiesIraeCrimson, (float)Math.Sin(Main.GameUpdateCount * 0.02f) * 0.5f + 0.5f);
            Lighting.AddLight(player.Center, lightColor.ToVector3() * pulse);
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Fusion", "⚔ STARFALL INFERNAL FUSION ⚔")
            {
                OverrideColor = FusionGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "System", "Conductor's Baton System - COSMIC INFERNO")
            {
                OverrideColor = NachtmusikPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Combines Nocturnal Maestro's Baton and Infernal Choirmaster's Scepter")
            {
                OverrideColor = DiesIraeCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "At night: +20% minion damage (enhanced)")
            {
                OverrideColor = NachtmusikPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Stellar hellfire: Minion burn deals +50% damage")
            {
                OverrideColor = DiesIraeCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Inherits ALL abilities from both component accessories")
            {
                OverrideColor = new Color(200, 180, 220)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Starfall and hellfire unite the cosmic choir'")
            {
                OverrideColor = new Color(180, 120, 160)
            });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<NocturnalMaestrosBaton>(1)
                .AddIngredient<InfernalChoirmastersScepter>(1)
                .AddIngredient<NachtmusikResonantEnergy>(10)
                .AddIngredient<DiesIraeResonantEnergy>(10)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    #endregion
    
    #region Fusion Tier 2: Triumphant Symphony Baton (+ Ode to Joy)
    
    /// <summary>
    /// Fusion Tier 2 Summoner accessory - adds Ode to Joy to the fusion.
    /// Triple harmony of stellar, infernal, and jubilant conductor.
    /// </summary>
    public class TriumphantSymphonyBaton : ModItem
    {
        public override string Texture => "MagnumOpus/Content/Common/Accessories/SummonerChain/TriumphantOrchestraBaton";
        
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
            var conductor = player.GetModPlayer<ConductorPlayer>();
            
            // Inherit all previous tier abilities
            conductor.HasConductorsWand = true;
            conductor.HasSpringMaestrosBadge = true;
            conductor.HasSolarDirectorsCrest = true;
            conductor.HasHarvestBeastlordsHorn = true;
            conductor.HasPermafrostCommandersCrown = true;
            conductor.HasVivaldisOrchestraBaton = true;
            conductor.HasMoonlitSymphonyWand = true;
            conductor.HasHeroicGeneralsBaton = true;
            conductor.HasInfernalChoirMastersRod = true;
            conductor.HasEnigmasHivemindLink = true;
            conductor.HasSwansGracefulDirection = true;
            conductor.HasFatesCosmicDominion = true;
            conductor.HasNocturnalMaestrosBaton = true;
            conductor.HasInfernalChoirmastersScepter = true;
            conductor.HasJubilantOrchestrasStaff = true;
            conductor.HasStarfallInfernalBaton = true;
            
            // Fusion flag
            conductor.HasTriumphantSymphonyBaton = true;
            
            // Enhanced night + attack speed
            if (!Main.dayTime)
            {
                player.GetDamage(DamageClass.Summon) += 0.22f;
            }
            player.GetAttackSpeed(DamageClass.Summon) += 0.12f;
            
            // Triple-theme particles
            if (!hideVisual && Main.rand.NextBool(6))
            {
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(38f, 38f);
                
                int theme = Main.rand.Next(3);
                int dustType = theme switch { 0 => DustID.GoldCoin, 1 => DustID.Torch, _ => DustID.RainbowMk2 };
                Color dustColor = theme switch { 0 => NachtmusikPurple, 1 => DiesIraeCrimson, _ => OdeToJoyWhite };
                
                Dust dust = Dust.NewDustPerfect(pos, dustType, Main.rand.NextVector2Circular(0.4f, 0.4f));
                dust.noGravity = true;
                dust.scale = 0.6f;
                dust.color = dustColor;
            }
            
            // Triumphant glow
            float hueShift = (Main.GameUpdateCount * 0.006f) % 1f;
            Color lightColor = Main.hslToRgb(hueShift, 0.35f, 0.65f);
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.03f) * 0.08f + 0.25f;
            Lighting.AddLight(player.Center, lightColor.ToVector3() * pulse);
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Fusion", "⚔ TRIUMPHANT SYMPHONY FUSION ⚔")
            {
                OverrideColor = FusionTriumph
            });
            
            tooltips.Add(new TooltipLine(Mod, "System", "Conductor's Baton System - TRIPLE HARMONY")
            {
                OverrideColor = NachtmusikPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Combines Starfall Infernal Baton with Jubilant Orchestra's Staff")
            {
                OverrideColor = DiesIraeCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+12% minion attack speed (enhanced)")
            {
                OverrideColor = OdeToJoyWhite
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Minion healing triggers starfall-hellfire bursts")
            {
                OverrideColor = FusionTriumph
            });
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Inherits ALL abilities from all three theme accessories")
            {
                OverrideColor = new Color(200, 180, 220)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Three harmonies unite in triumphant command'")
            {
                OverrideColor = new Color(220, 200, 180)
            });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<StarfallInfernalBaton>(1)
                .AddIngredient<JubilantOrchestrasStaff>(1)
                .AddIngredient<OdeToJoyResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    #endregion
    
    #region Fusion Tier 3: Scepter of the Eternal Conductor (Ultimate - + Clair de Lune)
    
    /// <summary>
    /// Ultimate Fusion Summoner accessory - all four Post-Fate themes combined.
    /// The pinnacle of the conductor system.
    /// </summary>
    public class ScepterOfTheEternalConductor : ModItem
    {
        public override string Texture => "MagnumOpus/Content/Common/Accessories/SummonerChain/BatonOfTheEternalConductor";
        
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
            var conductor = player.GetModPlayer<ConductorPlayer>();
            
            // Inherit all previous tier abilities
            conductor.HasConductorsWand = true;
            conductor.HasSpringMaestrosBadge = true;
            conductor.HasSolarDirectorsCrest = true;
            conductor.HasHarvestBeastlordsHorn = true;
            conductor.HasPermafrostCommandersCrown = true;
            conductor.HasVivaldisOrchestraBaton = true;
            conductor.HasMoonlitSymphonyWand = true;
            conductor.HasHeroicGeneralsBaton = true;
            conductor.HasInfernalChoirMastersRod = true;
            conductor.HasEnigmasHivemindLink = true;
            conductor.HasSwansGracefulDirection = true;
            conductor.HasFatesCosmicDominion = true;
            conductor.HasNocturnalMaestrosBaton = true;
            conductor.HasInfernalChoirmastersScepter = true;
            conductor.HasJubilantOrchestrasStaff = true;
            conductor.HasEternalConductorsScepter = true;
            conductor.HasStarfallInfernalBaton = true;
            conductor.HasTriumphantSymphonyBaton = true;
            
            // Ultimate flag
            conductor.HasScepterOfTheEternalConductor = true;
            
            // Always has enhanced bonuses (mastered)
            player.GetDamage(DamageClass.Summon) += 0.25f;
            player.GetAttackSpeed(DamageClass.Summon) += 0.15f;
            
            // Quad-theme particle spectacle
            if (!hideVisual && Main.rand.NextBool(5))
            {
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(42f, 42f);
                
                int theme = Main.rand.Next(4);
                int dustType = theme switch { 0 => DustID.GoldCoin, 1 => DustID.Torch, 2 => DustID.RainbowMk2, _ => DustID.Enchanted_Gold };
                Color dustColor = theme switch { 0 => NachtmusikPurple, 1 => DiesIraeCrimson, 2 => OdeToJoyWhite, _ => ClairDeLuneBrass };
                
                Dust dust = Dust.NewDustPerfect(pos, dustType, Main.rand.NextVector2Circular(0.5f, 0.5f));
                dust.noGravity = true;
                dust.scale = 0.65f;
                dust.color = dustColor;
            }
            
            // Orbiting quad points
            if (!hideVisual && Main.rand.NextBool(20))
            {
                float angle = Main.GameUpdateCount * 0.02f;
                Color[] colors = { NachtmusikPurple, DiesIraeCrimson, OdeToJoyWhite, ClairDeLuneBrass };
                for (int i = 0; i < 4; i++)
                {
                    float orbitAngle = angle + MathHelper.TwoPi * i / 4f;
                    Vector2 orbitPos = player.Center + orbitAngle.ToRotationVector2() * 48f;
                    
                    Dust dust = Dust.NewDustPerfect(orbitPos, DustID.MagicMirror, Vector2.Zero);
                    dust.noGravity = true;
                    dust.scale = 0.5f;
                    dust.color = colors[i];
                }
            }
            
            // Ultimate prismatic glow
            float timeShift = Main.GameUpdateCount * 0.01f;
            float pulse = (float)Math.Sin(timeShift * 2f) * 0.1f + 0.3f;
            float hue = (timeShift * 0.4f) % 1f;
            Color lightColor = Main.hslToRgb(hue, 0.45f, 0.75f);
            Lighting.AddLight(player.Center, lightColor.ToVector3() * pulse);
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Ultimate", "✦✦✦ ETERNAL CONDUCTOR - ULTIMATE FUSION ✦✦✦")
            {
                OverrideColor = UltimatePrismatic
            });
            
            tooltips.Add(new TooltipLine(Mod, "System", "Conductor's Baton System - GRAND FINALE")
            {
                OverrideColor = ClairDeLuneBrass
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Combines ALL four Post-Fate theme accessories")
            {
                OverrideColor = NachtmusikPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+25% minion damage, +15% attack speed always")
            {
                OverrideColor = DiesIraeCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Eternal Temporal Finale: 5s infinite minion attack speed (90s CD)")
            {
                OverrideColor = OdeToJoyWhite
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Conduct cooldown reduced to 3 seconds")
            {
                OverrideColor = ClairDeLuneBrass
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect5", "All minion effects trigger simultaneously across time")
            {
                OverrideColor = UltimatePrismatic
            });
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Masters ALL abilities from the complete Post-Fate arsenal")
            {
                OverrideColor = new Color(220, 200, 240)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The eternal conductor commands the symphony of existence'")
            {
                OverrideColor = new Color(200, 180, 160)
            });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<TriumphantSymphonyBaton>(1)
                .AddIngredient<EternalConductorsScepter>(1)
                .AddIngredient<ClairDeLuneResonantEnergy>(20)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    #endregion
}
