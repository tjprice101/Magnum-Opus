using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Materials.Foundation;
using MagnumOpus.Content.Spring.Materials;
using MagnumOpus.Content.Summer.Materials;
using MagnumOpus.Content.Autumn.Materials;
using MagnumOpus.Content.Winter.Materials;

namespace MagnumOpus.Content.Common.Accessories.MageChain
{
    /// <summary>
    /// Resonant Overflow Gem - Base tier magic chain accessory
    /// Can cast spells up to -20 mana. While negative: -25% magic damage, +50% mana regen
    /// </summary>
    public class ResonantOverflowGem : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(0, 1, 0, 0);
            Item.rare = ItemRarityID.Blue;
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var overflowPlayer = player.GetModPlayer<OverflowPlayer>();
            overflowPlayer.hasResonantOverflowGem = true;
            
            // +50% mana regen while in overflow is handled by the OverflowPlayer
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.ManaRegenerationBand)
                .AddIngredient<ResonantCrystalShard>(10)
                .AddTile(TileID.TinkerersWorkbench)
                .Register();
        }
    }
    
    /// <summary>
    /// Spring Arcane Conduit - Tier 2 magic chain accessory
    /// Overflow to -40 mana. While negative: spells leave healing petal trails
    /// </summary>
    public class SpringArcaneConduit : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(0, 3, 0, 0);
            Item.rare = ItemRarityID.Green;
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var overflowPlayer = player.GetModPlayer<OverflowPlayer>();
            overflowPlayer.hasResonantOverflowGem = true;
            overflowPlayer.hasSpringArcaneConduit = true;
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ResonantOverflowGem>()
                .AddIngredient<VernalBar>(15)
                // .AddIngredient<PrimaveraTrophy>() // Primavera drop
                .AddTile(TileID.TinkerersWorkbench)
                .Register();
        }
    }
    
    /// <summary>
    /// Solar Mana Crucible - Tier 3 magic chain accessory
    /// Overflow to -60 mana. While negative: spells inflict "Sunburn" debuff
    /// </summary>
    public class SolarManaCrucible : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(0, 5, 0, 0);
            Item.rare = ItemRarityID.Orange;
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var overflowPlayer = player.GetModPlayer<OverflowPlayer>();
            overflowPlayer.hasResonantOverflowGem = true;
            overflowPlayer.hasSpringArcaneConduit = true;
            overflowPlayer.hasSolarManaCrucible = true;
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SpringArcaneConduit>()
                .AddIngredient<SolsticeBar>(15)
                // .AddIngredient<LEstateTrophy>() // L'Estate drop
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    
    /// <summary>
    /// Harvest Soul Vessel - Tier 4 magic chain accessory
    /// Overflow to -80 mana. Killing enemies while negative restores +15 mana instantly
    /// </summary>
    public class HarvestSoulVessel : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(0, 8, 0, 0);
            Item.rare = ItemRarityID.LightRed;
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var overflowPlayer = player.GetModPlayer<OverflowPlayer>();
            overflowPlayer.hasResonantOverflowGem = true;
            overflowPlayer.hasSpringArcaneConduit = true;
            overflowPlayer.hasSolarManaCrucible = true;
            overflowPlayer.hasHarvestSoulVessel = true;
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SolarManaCrucible>()
                .AddIngredient<HarvestBar>(20)
                // .AddIngredient<AutunnoTrophy>() // Autunno drop
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    
    /// <summary>
    /// Permafrost Void Heart - Tier 5 magic chain accessory
    /// Overflow to -100 mana. While negative: spells have +15% damage (risk/reward!)
    /// </summary>
    public class PermafrostVoidHeart : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(0, 12, 0, 0);
            Item.rare = ItemRarityID.Pink;
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var overflowPlayer = player.GetModPlayer<OverflowPlayer>();
            overflowPlayer.hasResonantOverflowGem = true;
            overflowPlayer.hasSpringArcaneConduit = true;
            overflowPlayer.hasSolarManaCrucible = true;
            overflowPlayer.hasHarvestSoulVessel = true;
            overflowPlayer.hasPermafrostVoidHeart = true;
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<HarvestSoulVessel>()
                .AddIngredient<PermafrostBar>(25)
                // .AddIngredient<LInvernoTrophy>() // L'Inverno drop
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    
    /// <summary>
    /// Vivaldi's Harmonic Core - Tier 6 magic chain accessory (Post-Plantera)
    /// Overflow to -120 mana. Recovering from negative mana releases a seasonal burst
    /// </summary>
    public class VivaldisHarmonicCore : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(0, 20, 0, 0);
            Item.rare = ItemRarityID.Lime;
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var overflowPlayer = player.GetModPlayer<OverflowPlayer>();
            overflowPlayer.hasResonantOverflowGem = true;
            overflowPlayer.hasSpringArcaneConduit = true;
            overflowPlayer.hasSolarManaCrucible = true;
            overflowPlayer.hasHarvestSoulVessel = true;
            overflowPlayer.hasPermafrostVoidHeart = true;
            overflowPlayer.hasVivaldisHarmonicCore = true;
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<PermafrostVoidHeart>()
                // .AddIngredient<CycleOfSeasons>() // Post-Plantera crafting item
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
