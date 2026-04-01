using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Content.LaCampanella.HarmonicCores;
using MagnumOpus.Content.EnigmaVariations.ResonanceEnergies;
using MagnumOpus.Content.SwanLake.ResonanceEnergies;
using MagnumOpus.Content.Fate.ResonanceEnergies;

namespace MagnumOpus.Content.Common.Accessories.MobilityChain
{
    // ============================================================
    // THEME TIER 1: MOONLIT PHANTOM'S RUSH
    // Semi-transparent at 100+ momentum, reduced aggro
    // ============================================================
    public class MoonlitPhantomsRush : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.rare = ModContent.RarityType<MoonlightSonataRarity>();
            Item.value = Item.sellPrice(gold: 35, silver: 50);
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var momentumPlayer = player.GetModPlayer<MomentumPlayer>();
            momentumPlayer.HasMoonlitPhantomsRush = true;
            
            // Enhanced movement
            player.moveSpeed += 0.27f;
            player.maxRunSpeed += 1.5f;
            player.jumpBoost = true;
            player.autoJump = true;
            player.jumpSpeedBoost += 1.0f;
            
            // Night vision boost
            player.nightVision = true;
            
            // Full immunities
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            player.buffImmune[BuffID.Chilled] = true;
            
            // Lunar/night power boost
            if (!Main.dayTime)
            {
                player.moveSpeed += 0.05f;
                player.GetDamage(DamageClass.Generic) += 0.05f;
            }
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Enables the Momentum system (max 120)"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+27% movement speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+1.5 max run speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "+1.0 jump speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Night vision and +5% damage at night"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Fire and ice immunity"));
            tooltips.Add(new TooltipLine(Mod, "Effect7", "Reduced enemy aggro"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The moon's gentle light guides those who walk between worlds'") { OverrideColor = new Color(140, 100, 200) });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<VivaldisSeasonalSprint>()
                .AddIngredient<ResonantCoreOfMoonlightSonata>()
                .AddIngredient(ItemID.LunarBar, 8)
                .AddIngredient(ItemID.Ectoplasm, 10)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    // ============================================================
    // THEME TIER 2: HEROIC CHARGE BOOTS
    // Consume 80 momentum for dash attack
    // ============================================================
    public class HeroicChargeBoots : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.rare = ModContent.RarityType<EroicaRarity>();
            Item.value = Item.sellPrice(gold: 40);
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var momentumPlayer = player.GetModPlayer<MomentumPlayer>();
            momentumPlayer.HasHeroicChargeBoots = true;
            
            // Enhanced movement
            player.moveSpeed += 0.29f;
            player.maxRunSpeed += 1.7f;
            player.jumpBoost = true;
            player.autoJump = true;
            player.jumpSpeedBoost += 1.2f;
            player.dashType = 2; // Shield of Cthulhu dash
            
            // Full immunities
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            player.buffImmune[BuffID.Chilled] = true;
            
            // Heroic resolve - increased defense when momentum is high
            if (momentumPlayer.CurrentMomentum >= 60f)
            {
                player.statDefense += 8;
            }
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Enables the Momentum system (max 120)"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+29% movement speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+1.7 max run speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "+1.2 jump speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Grants Shield of Cthulhu dash"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Fire and ice immunity"));
            tooltips.Add(new TooltipLine(Mod, "Effect7", "+8 defense at 60+ momentum"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Heroes charge forward, never retreating'") { OverrideColor = new Color(200, 80, 80) });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<MoonlitPhantomsRush>()
                .AddIngredient<ResonantCoreOfEroica>()
                .AddIngredient(ItemID.BeetleHusk, 8)
                .AddIngredient(ItemID.LunarBar, 5)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    // ============================================================
    // THEME TIER 3: INFERNAL METEOR STRIDE
    // Impact crater on landing at 100+ momentum (AoE damage)
    // ============================================================
    public class InfernalMeteorStride : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.rare = ModContent.RarityType<LaCampanellaRarity>();
            Item.value = Item.sellPrice(gold: 45);
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var momentumPlayer = player.GetModPlayer<MomentumPlayer>();
            momentumPlayer.HasInfernalMeteorStride = true;
            
            // Enhanced movement
            player.moveSpeed += 0.31f;
            player.maxRunSpeed += 1.9f;
            player.jumpBoost = true;
            player.autoJump = true;
            player.jumpSpeedBoost += 1.4f;
            player.dashType = 2;
            
            // Full fire immunity
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            player.buffImmune[BuffID.Chilled] = true;
            player.buffImmune[BuffID.OnFire3] = true;
            player.lavaImmune = true;
            
            // Infernal power - fire damage boost
            if (momentumPlayer.CurrentMomentum >= 70f)
            {
                player.GetDamage(DamageClass.Generic) += 0.08f;
            }
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Enables the Momentum system (max 120)"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+31% movement speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+1.9 max run speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "+1.4 jump speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Grants Shield of Cthulhu dash"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Complete fire and lava immunity"));
            tooltips.Add(new TooltipLine(Mod, "Effect7", "+8% damage at 70+ momentum"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The bell tolls for those who stand in your path'") { OverrideColor = new Color(255, 140, 40) });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<HeroicChargeBoots>()
                .AddIngredient<ResonantCoreOfLaCampanella>()
                .AddIngredient(ItemID.HellstoneBar, 15)
                .AddIngredient(ItemID.LunarBar, 5)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    // ============================================================
    // THEME TIER 4: ENIGMA'S PHASE SHIFT
    // Consume 100 momentum for teleport (12.5 tiles)
    // ============================================================
    public class EnigmasPhaseShift : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.rare = ModContent.RarityType<EnigmaVariationsRarity>();
            Item.value = Item.sellPrice(gold: 50);
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var momentumPlayer = player.GetModPlayer<MomentumPlayer>();
            momentumPlayer.HasEnigmasPhaseShift = true;
            
            // Enhanced movement
            player.moveSpeed += 0.33f;
            player.maxRunSpeed += 2.1f;
            player.jumpBoost = true;
            player.autoJump = true;
            player.jumpSpeedBoost += 1.6f;
            player.dashType = 2;
            
            // Full immunities
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            player.buffImmune[BuffID.Chilled] = true;
            player.buffImmune[BuffID.OnFire3] = true;
            player.lavaImmune = true;
            player.buffImmune[BuffID.Confused] = true;
            
            // Enigma dodge - chance to phase through attacks
            if (momentumPlayer.CurrentMomentum >= 80f)
            {
                player.blackBelt = true; // Grants dodge chance
            }
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Enables the Momentum system (max 120)"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+33% movement speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+2.1 max run speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "+1.6 jump speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Grants Shield of Cthulhu dash"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Full fire, lava, and confusion immunity"));
            tooltips.Add(new TooltipLine(Mod, "Effect7", "Black belt dodge chance at 80+ momentum"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Reality bends for those who question its nature'") { OverrideColor = new Color(140, 60, 200) });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<InfernalMeteorStride>()
                .AddIngredient<ResonantCoreOfEnigma>()
                .AddIngredient(ItemID.RodofDiscord)
                .AddIngredient(ItemID.LunarBar, 5)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    // ============================================================
    // THEME TIER 5: SWAN'S ETERNAL GLIDE
    // 50% slower decay, infinite flight at max momentum
    // ============================================================
    public class SwansEternalGlide : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.rare = ModContent.RarityType<SwanRarity>();
            Item.value = Item.sellPrice(gold: 55);
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var momentumPlayer = player.GetModPlayer<MomentumPlayer>();
            momentumPlayer.HasSwansEternalGlide = true;
            
            // Enhanced movement
            player.moveSpeed += 0.35f;
            player.maxRunSpeed += 2.3f;
            player.jumpBoost = true;
            player.autoJump = true;
            player.jumpSpeedBoost += 1.8f;
            player.dashType = 2;
            
            // Enhanced flight
            player.wingTimeMax += 100;
            
            // Full immunities
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            player.buffImmune[BuffID.Chilled] = true;
            player.buffImmune[BuffID.OnFire3] = true;
            player.lavaImmune = true;
            player.buffImmune[BuffID.Confused] = true;
            player.blackBelt = true;
            
            // Graceful presence - reduced aggro
            player.aggro -= 400;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Enables the Momentum system (max 120)"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+35% movement speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+2.3 max run speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "+1.8 jump speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "+100 wing flight time"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "All immunities plus black belt dodge"));
            tooltips.Add(new TooltipLine(Mod, "Effect7", "Greatly reduced enemy aggro"));
            tooltips.Add(new TooltipLine(Mod, "Effect8", "Infinite flight at maximum momentum"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Glide eternally upon wings of grace and light'") { OverrideColor = new Color(240, 245, 255) });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<EnigmasPhaseShift>()
                .AddIngredient<ResonantCoreOfSwanLake>()
                .AddIngredient(ItemID.SoulofFlight, 20)
                .AddIngredient(ItemID.LunarBar, 5)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    // ============================================================
    // THEME TIER 6: FATE'S COSMIC VELOCITY
    // Max 150 momentum, time slow 20% for nearby enemies at 150
    // ============================================================
    public class FatesCosmicVelocity : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.value = Item.sellPrice(gold: 65);
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var momentumPlayer = player.GetModPlayer<MomentumPlayer>();
            momentumPlayer.HasFatesCosmicVelocity = true;
            
            // Maximum movement stats
            player.moveSpeed += 0.37f;
            player.maxRunSpeed += 2.5f;
            player.jumpBoost = true;
            player.autoJump = true;
            player.jumpSpeedBoost += 2.0f;
            player.dashType = 2;
            
            // Enhanced flight
            player.wingTimeMax += 150;
            
            // Full immunities
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            player.buffImmune[BuffID.Chilled] = true;
            player.buffImmune[BuffID.OnFire3] = true;
            player.lavaImmune = true;
            player.buffImmune[BuffID.Confused] = true;
            player.buffImmune[BuffID.Slow] = true;
            player.blackBelt = true;
            
            // Cosmic power - all damage boost
            player.GetDamage(DamageClass.Generic) += 0.10f;
            
            // Fate's grip - reduced aggro but intimidating presence
            player.aggro -= 200;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Enables the Momentum system (max 150)"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+37% movement speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+2.5 max run speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "+2.0 jump speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "+150 wing flight time"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "+10% damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect7", "All immunities including slow"));
            tooltips.Add(new TooltipLine(Mod, "Effect8", "Black belt dodge and reduced aggro"));
            tooltips.Add(new TooltipLine(Mod, "Effect9", "At 150 momentum, nearby enemies are slowed"));
            tooltips.Add(new TooltipLine(Mod, "Effect10", "Infinite flight at maximum momentum"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Destiny itself cannot outrun the wearer'") { OverrideColor = new Color(180, 40, 80) });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SwansEternalGlide>()
                .AddIngredient<ResonantCoreOfFate>()
                .AddIngredient<FateResonantEnergy>(5)
                .AddIngredient(ItemID.LunarBar, 10)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}
