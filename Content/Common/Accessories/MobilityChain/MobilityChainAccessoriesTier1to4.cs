using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Materials.Foundation;
using MagnumOpus.Content.Spring.Materials;
using MagnumOpus.Content.Summer.Materials;
using MagnumOpus.Content.Autumn.Materials;
using MagnumOpus.Content.Winter.Materials;
using MagnumOpus.Content.Seasons.Accessories;

namespace MagnumOpus.Content.Common.Accessories.MobilityChain
{
    // ============================================================
    // TIER 1: RESONANT VELOCITY BAND
    // +15% movement speed, +0.3 flight time
    // ============================================================
    public class ResonantVelocityBand : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.accessory = true;
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(gold: 2);
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var momentumPlayer = player.GetModPlayer<MomentumPlayer>();
            momentumPlayer.HasVelocityBand = true;
            
            player.moveSpeed += 0.15f;
            player.wingTimeMax += 18; // +0.3s flight time
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+15% movement speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+0.3 flight time"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The first step toward transcending mortal limits'") { OverrideColor = new Color(150, 200, 100) });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ResonantCrystalShard>(12)
                .AddIngredient(ItemID.SwiftnessPotion, 5)
                .AddIngredient(ItemID.Aglet)
                .AddIngredient(ItemID.AnkletoftheWind)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
    
    // ============================================================
    // TIER 2: SPRING ZEPHYR BOOTS
    // +0.5 run speed, +18% movement speed, +0.5 flight time
    // ============================================================
    public class SpringZephyrBoots : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(gold: 5);
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var momentumPlayer = player.GetModPlayer<MomentumPlayer>();
            momentumPlayer.HasSpringZephyrBoots = true;
            
            player.moveSpeed += 0.18f;
            player.maxRunSpeed += 0.5f;
            player.wingTimeMax += 30; // +0.5s flight time
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+18% movement speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+0.5 run speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+0.5 flight time"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Spring winds carry those who embrace the changing seasons'") { OverrideColor = new Color(150, 200, 100) });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ResonantVelocityBand>()
                .AddIngredient<VernalBar>(10)
                .AddIngredient(ItemID.HermesBoots)
                .AddIngredient(ItemID.CloudinaBottle)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    
    // ============================================================
    // TIER 3: SOLAR BLITZ TREADS
    // Blazing trail, +0.7 run speed, +25% movement, +0.8 flight
    // ============================================================
    public class SolarBlitzTreads : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(gold: 8);
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var momentumPlayer = player.GetModPlayer<MomentumPlayer>();
            momentumPlayer.HasSolarBlitzTreads = true;
            
            player.moveSpeed += 0.25f;
            player.maxRunSpeed += 0.7f;
            player.wingTimeMax += 48; // +0.8s flight time
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+25% movement speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+0.7 run speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+0.8 flight time"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Running leaves a blazing trail behind you"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Summer's fury blazes beneath every step'") { OverrideColor = new Color(150, 200, 100) });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SpringZephyrBoots>()
                .AddIngredient<SolsticeBar>(10)
                .AddIngredient(ItemID.LavaCharm)
                .AddIngredient(ItemID.SoulofLight, 8)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    
    // ============================================================
    // TIER 4: HARVEST PHANTOM STRIDE
    // Blazing trail, +0.7 run speed, +30% movement, +1.0 flight
    // ============================================================
    public class HarvestPhantomStride : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.rare = ItemRarityID.LightPurple;
            Item.value = Item.sellPrice(gold: 12);
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var momentumPlayer = player.GetModPlayer<MomentumPlayer>();
            momentumPlayer.HasHarvestPhantomStride = true;
            
            player.moveSpeed += 0.30f;
            player.maxRunSpeed += 0.7f;
            player.wingTimeMax += 60; // +1.0s flight time
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+30% movement speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+0.7 run speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+1.0 flight time"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Running leaves a blazing trail behind you"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Autumn's spirits guide the worthy through shadow'") { OverrideColor = new Color(150, 200, 100) });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SolarBlitzTreads>()
                .AddIngredient<HarvestBar>(10)
                .AddIngredient(ItemID.SoulofNight, 8)
                .AddIngredient(ItemID.Ectoplasm, 5)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    
    // ============================================================
    // TIER 5: PERMAFROST AVALANCHE STEP
    // Blazing trail, +0.9 run speed, +35% movement, +1.2 flight
    // ============================================================
    public class PermafrostAvalancheStep : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.rare = ItemRarityID.Lime;
            Item.value = Item.sellPrice(gold: 18);
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var momentumPlayer = player.GetModPlayer<MomentumPlayer>();
            momentumPlayer.HasPermafrostAvalancheStep = true;
            
            player.moveSpeed += 0.35f;
            player.maxRunSpeed += 0.9f;
            player.wingTimeMax += 72; // +1.2s flight time
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+35% movement speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+0.9 run speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+1.2 flight time"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Running leaves a blazing trail behind you"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Winter's embrace hardens the soul against all adversity'") { OverrideColor = new Color(150, 200, 100) });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<HarvestPhantomStride>()
                .AddIngredient<PermafrostBar>(10)
                .AddIngredient(ItemID.FrostCore)
                .AddIngredient(ItemID.FragmentSolar, 5)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    // ============================================================
    // TIER 6: VIVALDI'S SEASONAL SPRINT
    // Blazing+frostburn trail, +1.2 run speed, +38% movement, +1.4 flight
    // ============================================================
    public class VivaldisSeasonalSprint : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.rare = ItemRarityID.Yellow;
            Item.value = Item.sellPrice(gold: 25);
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var momentumPlayer = player.GetModPlayer<MomentumPlayer>();
            momentumPlayer.HasVivaldisSeasonalSprint = true;
            
            player.moveSpeed += 0.38f;
            player.maxRunSpeed += 1.2f;
            player.wingTimeMax += 84; // +1.4s flight time
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+38% movement speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+1.2 run speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+1.4 flight time"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Running leaves a blazing and frostburn trail behind you"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Four seasons dance as one beneath your feet'") { OverrideColor = new Color(150, 200, 100) });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<PermafrostAvalancheStep>()
                .AddIngredient<CycleOfSeasons>()
                .AddIngredient(ItemID.FragmentVortex, 8)
                .AddIngredient(ItemID.FragmentStardust, 8)
                .AddIngredient(ItemID.LunarBar, 5)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}
