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

namespace MagnumOpus.Content.Common.Accessories.DefenseChain
{
    // ============================================================
    // TIER 1: RESONANT BARRIER CORE
    // +2 defense, 5% max HP, +2 HP regen/s after 5s without damage
    // ============================================================
    public class ResonantBarrierCore : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(gold: 1);
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var shieldPlayer = player.GetModPlayer<ResonantShieldPlayer>();
            shieldPlayer.HasResonantBarrierCore = true;
            
            player.statDefense += 2;
            player.statLifeMax2 += (int)(player.statLifeMax * 0.05f);
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+2 defense"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "5% increased maximum life"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+2 HP regeneration per second after 5 seconds without taking damage"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A crystal barrier hums with protective frequencies'") { OverrideColor = new Color(180, 200, 255) });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ResonantCrystalShard>(10)
                .AddIngredient(ItemID.Shackle)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
    
    // ============================================================
    // TIER 2: SPRING VITALITY SHELL
    // +4 defense, 7% max HP, +4 HP regen/s after 5s, +8 defense on hit
    // ============================================================
    public class SpringVitalityShell : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(gold: 3);
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var shieldPlayer = player.GetModPlayer<ResonantShieldPlayer>();
            shieldPlayer.HasSpringVitalityShell = true;
            
            player.statDefense += 4;
            player.statLifeMax2 += (int)(player.statLifeMax * 0.07f);
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+4 defense"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "7% increased maximum life"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+4 HP regeneration per second after 5 seconds without taking damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Hitting enemies grants +8 defense for 1 second"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Spring's gentle embrace, warding off winter's last chill'") { OverrideColor = new Color(150, 200, 100) });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ResonantBarrierCore>()
                .AddIngredient<VernalBar>(15)
                .AddIngredient<SpringResonantEnergy>()
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
    
    // ============================================================
    // TIER 3: SOLAR FLARE AEGIS
    // +6 defense, fire/lava immunity, hellfire <50% HP, 10% max HP,
    // +7 HP regen/s after 5s, +10 defense on hit
    // ============================================================
    public class SolarFlareAegis : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(gold: 5);
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var shieldPlayer = player.GetModPlayer<ResonantShieldPlayer>();
            shieldPlayer.HasSolarFlareAegis = true;
            
            player.statDefense += 6;
            player.statLifeMax2 += (int)(player.statLifeMax * 0.10f);
            
            // Fire and lava immunity
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.lavaImmune = true;
            player.fireWalk = true;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+6 defense"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "10% increased maximum life"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Immunity to fire and lava"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "+7 HP regeneration per second after 5 seconds without taking damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Hitting enemies grants +10 defense for 1 second"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Inflicts Hellfire on nearby enemies when below 50% health"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Forged in the heart of summer's blazing sun'") { OverrideColor = new Color(150, 200, 100) });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SpringVitalityShell>()
                .AddIngredient<SolsticeBar>(15)
                .AddIngredient<SummerResonantEnergy>()
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    
    // ============================================================
    // TIER 4: HARVEST THORNED GUARD
    // +10 defense, fire/lava/blindness immunity, hellfire <50% HP,
    // 12% max HP, +9 HP regen/s after 5s, +12 defense on hit, 90% thorns
    // ============================================================
    public class HarvestThornedGuard : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.rare = ItemRarityID.LightPurple;
            Item.value = Item.sellPrice(gold: 8);
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var shieldPlayer = player.GetModPlayer<ResonantShieldPlayer>();
            shieldPlayer.HasHarvestThornedGuard = true;
            
            player.statDefense += 10;
            player.statLifeMax2 += (int)(player.statLifeMax * 0.12f);
            player.thorns = 0.9f;
            
            // Fire, lava, and blindness immunity
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.buffImmune[BuffID.Darkness] = true;
            player.lavaImmune = true;
            player.fireWalk = true;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+10 defense"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "12% increased maximum life"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Immunity to fire, lava, and blindness"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "+9 HP regeneration per second after 5 seconds without taking damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Hitting enemies grants +12 defense for 1 second"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Inflicts Hellfire on nearby enemies when below 50% health"));
            tooltips.Add(new TooltipLine(Mod, "Effect7", "90% of contact damage is reflected back to attackers"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Autumn's thorns protect the harvest from all who would pillage'") { OverrideColor = new Color(150, 200, 100) });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SolarFlareAegis>()
                .AddIngredient<HarvestBar>(20)
                .AddIngredient<AutumnResonantEnergy>()
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    
    // ============================================================
    // TIER 5: PERMAFROST CRYSTAL WARD
    // +12 defense, fire/lava/blindness/ice immunity, hellfire <50% HP,
    // 15% max HP, +11 HP regen/s after 5s, +14 defense on hit, 90% thorns
    // ============================================================
    public class PermafrostCrystalWard : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.rare = ItemRarityID.Lime;
            Item.value = Item.sellPrice(gold: 12);
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var shieldPlayer = player.GetModPlayer<ResonantShieldPlayer>();
            shieldPlayer.HasPermafrostCrystalWard = true;
            
            player.statDefense += 12;
            player.statLifeMax2 += (int)(player.statLifeMax * 0.15f);
            player.thorns = 0.9f;
            
            // Fire, lava, blindness, and ice immunity
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.buffImmune[BuffID.Darkness] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            player.buffImmune[BuffID.Chilled] = true;
            player.buffImmune[BuffID.Frozen] = true;
            player.lavaImmune = true;
            player.fireWalk = true;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+12 defense"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "15% increased maximum life"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Immunity to fire, lava, blindness, frostburn, chill, and frozen"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "+11 HP regeneration per second after 5 seconds without taking damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Hitting enemies grants +14 defense for 1 second"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Inflicts Hellfire on nearby enemies when below 50% health"));
            tooltips.Add(new TooltipLine(Mod, "Effect7", "90% of contact damage is reflected back to attackers"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Winter's crystalline heart, unbreakable and eternal'") { OverrideColor = new Color(150, 200, 100) });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<HarvestThornedGuard>()
                .AddIngredient<PermafrostBar>(25)
                .AddIngredient<WinterResonantEnergy>()
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    
    // ============================================================
    // TIER 6: VIVALDI'S SEASONAL BULWARK
    // 35% shield, break effect changes with season (heal/fire/thorns/freeze)
    // ============================================================
    public class VivaldisSeasonalBulwark : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.rare = ItemRarityID.Yellow;
            Item.value = Item.sellPrice(gold: 18);
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var shieldPlayer = player.GetModPlayer<ResonantShieldPlayer>();
            shieldPlayer.HasVivaldisSeasonalBulwark = true;
            
            player.statDefense += 14;
            player.statLifeMax2 += (int)(player.statLifeMax * 0.18f);
            player.thorns = 0.9f;
            
            // Full immunity suite
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            player.buffImmune[BuffID.Chilled] = true;
            player.buffImmune[BuffID.Frozen] = true;
            player.buffImmune[BuffID.Darkness] = true;
            player.buffImmune[BuffID.Bleeding] = true;
            player.buffImmune[BuffID.Poisoned] = true;
            player.lavaImmune = true;
            player.fireWalk = true;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+14 defense"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "18% increased maximum life"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Immunity to fire, lava, frostburn, chill, frozen, blindness, bleeding, and poison"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "+13 HP regeneration per second after 5 seconds without taking damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Hitting enemies grants +16 defense for 1 second"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Inflicts Hellfire, Frostburn, Poison, and Bleeding on nearby enemies when below 50% health"));
            tooltips.Add(new TooltipLine(Mod, "Effect7", "90% of contact damage is reflected back to attackers"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The four seasons dance in eternal harmony within this bulwark'") { OverrideColor = new Color(150, 200, 100) });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<PermafrostCrystalWard>()
                .AddIngredient<CycleOfSeasons>()
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
