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
            
            // T7 flag — stats applied via PostUpdateEquips
            conductor.HasNocturnalMaestrosBaton = true;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+25% summon damage at night, +8% during day")
            {
                OverrideColor = NachtmusikPurple
            });
            tooltips.Add(new TooltipLine(Mod, "Effect2", "10% on night minion hit: Lullaby — slows and weakens enemies for 3s")
            {
                OverrideColor = NachtmusikPurple
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
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Minions inflict Hellfire on hit")
            {
                OverrideColor = DiesIraeOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+30% minion damage during bosses, +15% otherwise")
            {
                OverrideColor = DiesIraeCrimson
            });

            tooltips.Add(new TooltipLine(Mod, "Effect3", "Minion crits apply Infernal Choir (increased damage taken for 3s)")
            {
                OverrideColor = DiesIraeCrimson
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
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+15% summon damage")
            {
                OverrideColor = OdeToJoyRose
            });

            tooltips.Add(new TooltipLine(Mod, "Effect2", "Minion hits heal 1 HP (max 5 HP/s)")
            {
                OverrideColor = OdeToJoyRose
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Every 15s: Tutti Fortissimo 3s (+50% minion damage)")
            {
                OverrideColor = OdeToJoyIridescent
            });

            tooltips.Add(new TooltipLine(Mod, "Effect4", "+10% minion attack speed")
            {
                OverrideColor = OdeToJoyIridescent
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

            // T10 stat bonus
            player.GetDamage(DamageClass.Summon) += 0.15f;
            player.GetAttackSpeed(DamageClass.Summon) += 0.12f;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+15% minion damage, +12% minion attack speed")
            {
                OverrideColor = ClairDeLuneIridescent
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

            player.maxMinions += 1;
            player.whipRangeMultiplier += 0.10f;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+20% minion damage at night")
            {
                OverrideColor = NachtmusikPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+1 max minion, +10% whip range")
            {
                OverrideColor = FusionGold
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
            player.maxMinions += 2;
            player.maxTurrets += 1;
            player.whipRangeMultiplier += 0.18f;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+22% minion damage at night, +12% attack speed")
            {
                OverrideColor = FusionTriumph
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+2 max minions, +1 max sentry, +18% whip range")
            {
                OverrideColor = OdeToJoyWhite
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
            
            // Ultimate flag — damage applied via PostUpdateEquips
            conductor.HasScepterOfTheEternalConductor = true;
            
            // Direct bonuses (attack speed, minions, sentries, whip range)
            player.GetAttackSpeed(DamageClass.Summon) += 0.15f;
            player.maxMinions += 3;
            player.maxTurrets += 2;
            player.whipRangeMultiplier += 0.25f;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+50% minion damage, +15% attack speed")
            {
                OverrideColor = UltimatePrismatic
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+3 max minions, +2 max sentries, +25% whip range")
            {
                OverrideColor = ClairDeLuneBrass
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Every minion plays in perfect unison'")
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
