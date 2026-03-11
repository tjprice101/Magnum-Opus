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
    // Base accessory: Enables Momentum system, max 100
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
            
            // Base movement boost
            player.moveSpeed += 0.08f;
            player.maxRunSpeed += 0.3f;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Enables the Momentum system (max 100)"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+8% movement speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+0.3 max run speed"));
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
    // +10% speed at 50+ momentum, double jump reset at 80+
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
            
            // Base stats inherited from lower tier
            player.moveSpeed += 0.10f;
            player.maxRunSpeed += 0.5f;
            
            // Enhanced jump height
            player.jumpBoost = true;
            player.autoJump = true;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Enables the Momentum system (max 100)"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+10% movement speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+0.5 max run speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Enhanced jump height and auto jump"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "+10% speed at 50+ momentum"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Double jump resets at 80+ momentum"));
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
    // Fire trail at 70+ momentum that damages enemies
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
            
            // Enhanced stats
            player.moveSpeed += 0.12f;
            player.maxRunSpeed += 0.7f;
            player.jumpBoost = true;
            player.autoJump = true;
            
            // Fire immunity at high momentum
            if (momentumPlayer.CurrentMomentum >= 70f)
            {
                player.buffImmune[BuffID.OnFire] = true;
                player.buffImmune[BuffID.Burning] = true;
            }
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Enables the Momentum system (max 100)"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+12% movement speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+0.7 max run speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Enhanced jump height and auto jump"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Fire immunity at 70+ momentum"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Leave a fire trail that damages enemies"));
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
    // Phase through enemies at 80+ momentum (reduced contact damage)
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
            
            // Enhanced stats
            player.moveSpeed += 0.14f;
            player.maxRunSpeed += 0.9f;
            player.jumpBoost = true;
            player.autoJump = true;
            
            // Fire immunity
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Enables the Momentum system (max 100)"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+14% movement speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+0.9 max run speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Enhanced jump height and auto jump"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Fire immunity"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Phase through enemies at 80+ momentum"));
            tooltips.Add(new TooltipLine(Mod, "Effect7", "Reduced contact damage while phasing"));
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
    // Ice trail at 90+ momentum, ice dash at 100
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
            
            // Enhanced stats
            player.moveSpeed += 0.16f;
            player.maxRunSpeed += 1.1f;
            player.jumpBoost = true;
            player.autoJump = true;
            player.jumpSpeedBoost += 0.5f;
            
            // Fire immunity
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            
            // Ice armor at high momentum
            if (momentumPlayer.CurrentMomentum >= 90f)
            {
                player.statDefense += 5;
            }
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Enables the Momentum system (max 100)"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+16% movement speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+1.1 max run speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "+0.5 jump speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Enhanced jump height and auto jump"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Fire immunity"));
            tooltips.Add(new TooltipLine(Mod, "Effect7", "+5 defense at 90+ momentum"));
            tooltips.Add(new TooltipLine(Mod, "Effect8", "Leave an ice trail and perform ice dash at 100 momentum"));
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
    // Max 120 momentum, seasonal trail effects that cycle
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
            
            // Max stats for pre-Moon Lord
            player.moveSpeed += 0.18f;
            player.maxRunSpeed += 1.3f;
            player.jumpBoost = true;
            player.autoJump = true;
            player.jumpSpeedBoost += 0.8f;
            
            // Full immunities
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            player.buffImmune[BuffID.Chilled] = true;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Enables the Momentum system (max 120)"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+18% movement speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+1.3 max run speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "+0.8 jump speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Enhanced jump height and auto jump"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Fire and ice immunity"));
            tooltips.Add(new TooltipLine(Mod, "Effect7", "Seasonal trail effects cycle with time"));
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
