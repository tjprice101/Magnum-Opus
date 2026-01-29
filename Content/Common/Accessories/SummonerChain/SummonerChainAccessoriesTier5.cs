using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

// Rarity imports
using MagnumOpus.Common;

// Theme core imports (CORRECT NAMESPACES)
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Content.LaCampanella.HarmonicCores;
using MagnumOpus.Content.EnigmaVariations.ResonanceEnergies;
using MagnumOpus.Content.SwanLake.ResonanceEnergies;
using MagnumOpus.Content.Fate.ResonanceEnergies;

namespace MagnumOpus.Content.Common.Accessories.SummonerChain
{
    // ==========================================
    // TIER 5: POST-MOON LORD THEME CHAIN
    // ==========================================
    
    /// <summary>
    /// Theme Chain T1: Moonlit Symphony Wand
    /// Conducting at night: +10% minion damage globally for duration
    /// </summary>
    public class MoonlitSymphonyWand : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.rare = ModContent.RarityType<MoonlightSonataRarity>();
            Item.value = Item.sellPrice(gold: 25);
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
            
            // New ability
            conductor.HasMoonlitSymphonyWand = true;
            
            if (Main.mouseRight && Main.mouseRightRelease && player.whoAmI == Main.myPlayer)
            {
                conductor.TryConduct();
            }
            
            // Ambient VFX at night
            if (!hideVisual && !Main.dayTime && Main.rand.NextBool(30))
            {
                Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(20f, 20f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.PurpleTorch, Vector2.UnitY * -1f);
                dust.noGravity = true;
                dust.scale = 0.8f;
            }
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<VivaldisOrchestraBaton>()
                .AddIngredient<ResonantCoreOfMoonlightSonata>(20)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    /// <summary>
    /// Theme Chain T2: Heroic General's Baton
    /// Conduct grants minions brief invincibility (1s). Rally your troops!
    /// </summary>
    public class HeroicGeneralsBaton : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.rare = ModContent.RarityType<EroicaRarity>();
            Item.value = Item.sellPrice(gold: 35);
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
            
            // New ability
            conductor.HasHeroicGeneralsBaton = true;
            
            if (Main.mouseRight && Main.mouseRightRelease && player.whoAmI == Main.myPlayer)
            {
                conductor.TryConduct();
            }
            
            // Ambient VFX - scarlet sparkles
            if (!hideVisual && Main.rand.NextBool(35))
            {
                Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(25f, 25f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.RedTorch, Main.rand.NextVector2Circular(1f, 1f));
                dust.noGravity = true;
                dust.scale = 0.7f;
            }
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<MoonlitSymphonyWand>()
                .AddIngredient<ResonantCoreOfEroica>(20)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    /// <summary>
    /// Theme Chain T3: Infernal Choir Master's Rod
    /// Conducted minions explode on hit (doesn't kill them). +50% damage as AoE
    /// </summary>
    public class InfernalChoirMastersRod : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.rare = ModContent.RarityType<LaCampanellaRarity>();
            Item.value = Item.sellPrice(gold: 45);
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
            
            // New ability
            conductor.HasInfernalChoirMastersRod = true;
            
            if (Main.mouseRight && Main.mouseRightRelease && player.whoAmI == Main.myPlayer)
            {
                conductor.TryConduct();
            }
            
            // Ambient VFX - infernal embers
            if (!hideVisual && Main.rand.NextBool(25))
            {
                Vector2 dustPos = player.Center + new Vector2(Main.rand.NextFloat(-30f, 30f), 20f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.Torch, Vector2.UnitY * -2f);
                dust.noGravity = true;
                dust.scale = 1.0f;
            }
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<HeroicGeneralsBaton>()
                .AddIngredient<ResonantCoreOfLaCampanella>(20)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    /// <summary>
    /// Theme Chain T4: Enigma's Hivemind Link
    /// Minions can phase through blocks during Conduct. Ambush from anywhere
    /// </summary>
    public class EnigmasHivemindLink : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.rare = ModContent.RarityType<EnigmaVariationsRarity>();
            Item.value = Item.sellPrice(gold: 55);
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
            
            // New ability
            conductor.HasEnigmasHivemindLink = true;
            
            if (Main.mouseRight && Main.mouseRightRelease && player.whoAmI == Main.myPlayer)
            {
                conductor.TryConduct();
            }
            
            // Ambient VFX - enigmatic purple wisps
            if (!hideVisual && Main.rand.NextBool(30))
            {
                Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.PurpleTorch, Main.rand.NextVector2Circular(0.5f, 0.5f));
                dust.noGravity = true;
                dust.scale = 0.6f;
                dust.fadeIn = 0.8f;
            }
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<InfernalChoirMastersRod>()
                .AddIngredient<ResonantCoreOfEnigma>(20)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    /// <summary>
    /// Theme Chain T5: Swan's Graceful Direction
    /// Perfect Conduct (full HP): minions deal double damage for focus duration
    /// </summary>
    public class SwansGracefulDirection : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.rare = ModContent.RarityType<SwanRarity>();
            Item.value = Item.sellPrice(gold: 70);
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
            
            // New ability
            conductor.HasSwansGracefulDirection = true;
            
            if (Main.mouseRight && Main.mouseRightRelease && player.whoAmI == Main.myPlayer)
            {
                conductor.TryConduct();
            }
            
            // Ambient VFX - graceful white feathers
            if (!hideVisual && Main.rand.NextBool(40))
            {
                Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.Cloud, Vector2.UnitY * -0.5f);
                dust.noGravity = true;
                dust.scale = 0.9f;
            }
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<EnigmasHivemindLink>()
                .AddIngredient<ResonantCoreOfSwanLake>(20)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    /// <summary>
    /// Theme Chain T6: Fate's Cosmic Dominion
    /// Conduct cooldown 5s. "Finale": hold Conduct 2s to sacrifice all minions for massive single hit
    /// </summary>
    public class FatesCosmicDominion : ModItem
    {
        private static readonly Color FateCrimson = new Color(180, 40, 80);
        private static readonly Color FatePurple = new Color(120, 30, 140);
        
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.value = Item.sellPrice(gold: 100);
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
            
            // New ability
            conductor.HasFatesCosmicDominion = true;
            
            // Right-click to conduct (hold for Finale)
            if (player.whoAmI == Main.myPlayer)
            {
                if (Main.mouseRight && Main.mouseRightRelease)
                {
                    conductor.TryConduct();
                }
                else if (!Main.mouseRight && conductor.IsChargingFinale)
                {
                    conductor.ReleaseConductButton();
                }
            }
            
            // Ambient VFX - cosmic glyphs and stars
            if (!hideVisual)
            {
                // Orbiting cosmic particles
                if (Main.rand.NextBool(20))
                {
                    float angle = Main.GameUpdateCount * 0.03f + Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 dustPos = player.Center + angle.ToRotationVector2() * Main.rand.NextFloat(25f, 40f);
                    Color dustColor = Color.Lerp(FateCrimson, FatePurple, Main.rand.NextFloat());
                    
                    Dust dust = Dust.NewDustPerfect(dustPos, DustID.PurpleTorch, Vector2.Zero);
                    dust.noGravity = true;
                    dust.scale = 0.7f;
                    dust.color = dustColor;
                }
                
                // Star twinkles
                if (Main.rand.NextBool(50))
                {
                    Vector2 starPos = player.Center + Main.rand.NextVector2Circular(45f, 45f);
                    Dust star = Dust.NewDustPerfect(starPos, DustID.MagicMirror, Vector2.Zero);
                    star.noGravity = true;
                    star.scale = 0.5f;
                }
            }
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SwansGracefulDirection>()
                .AddIngredient<ResonantCoreOfFate>(30)
                .AddIngredient<FateResonantEnergy>()
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}
