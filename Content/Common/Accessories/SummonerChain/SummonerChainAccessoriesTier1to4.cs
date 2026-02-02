using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

// Material imports
using MagnumOpus.Content.Materials.Foundation;
using MagnumOpus.Content.Spring.Materials;
using MagnumOpus.Content.Summer.Materials;
using MagnumOpus.Content.Autumn.Materials;
using MagnumOpus.Content.Winter.Materials;
using MagnumOpus.Content.Seasons.Accessories;

namespace MagnumOpus.Content.Common.Accessories.SummonerChain
{
    // ==========================================
    // TIER 1: PRE-HARDMODE FOUNDATION
    // ==========================================
    
    /// <summary>
    /// Tier 1: Resonant Conductor's Wand
    /// Right-click to Conduct: all minions focus one enemy for 3s (+20% damage)
    /// 15s cooldown
    /// </summary>
    public class ResonantConductorsWand : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(silver: 50);
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var conductor = player.GetModPlayer<ConductorPlayer>();
            conductor.HasConductorsWand = true;
            
            // Handle right-click conduct
            if (Main.mouseRight && Main.mouseRightRelease && player.whoAmI == Main.myPlayer)
            {
                conductor.TryConduct();
            }
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Right-click to Conduct: focus all minions on one target"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Conducted minions deal +20% damage for 3 seconds"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "15 second cooldown"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The first step in mastering the orchestra'") { OverrideColor = new Color(150, 200, 100) });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ResonantCrystalShard>(10)
                .AddIngredient(ItemID.FlinxFur, 3)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
    
    /// <summary>
    /// Tier 2: Spring Maestro's Badge
    /// Conduct cooldown 12s. Conducted minions heal you 1HP/hit during focus
    /// </summary>
    public class SpringMaestrosBadge : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.rare = ItemRarityID.Green;
            Item.value = Item.sellPrice(gold: 1);
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var conductor = player.GetModPlayer<ConductorPlayer>();
            conductor.HasConductorsWand = true;
            conductor.HasSpringMaestrosBadge = true;
            
            if (Main.mouseRight && Main.mouseRightRelease && player.whoAmI == Main.myPlayer)
            {
                conductor.TryConduct();
            }
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Right-click to Conduct: focus all minions on one target"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Conducted minions deal +20% damage for 3 seconds"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "12 second cooldown"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Conducted minions heal you 1 HP per hit during focus"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Spring awakens the conductor within'") { OverrideColor = new Color(150, 200, 100) });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ResonantConductorsWand>()
                .AddIngredient<VernalBar>(15)
                // TODO: Add Primavera drop when boss is implemented
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
    
    // ==========================================
    // TIER 2: MID PRE-HARDMODE (POST-L'ESTATE)
    // ==========================================
    
    /// <summary>
    /// Tier 3: Solar Director's Crest
    /// Conduct cooldown 10s. Focus target takes "Performed" debuff: -5 defense
    /// </summary>
    public class SolarDirectorsCrest : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(gold: 2);
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var conductor = player.GetModPlayer<ConductorPlayer>();
            conductor.HasConductorsWand = true;
            conductor.HasSpringMaestrosBadge = true;
            conductor.HasSolarDirectorsCrest = true;
            
            if (Main.mouseRight && Main.mouseRightRelease && player.whoAmI == Main.myPlayer)
            {
                conductor.TryConduct();
            }
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Right-click to Conduct: focus all minions on one target"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Conducted minions deal +20% damage for 3 seconds"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "10 second cooldown"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Conducted minions heal you 1 HP per hit during focus"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Focused target receives 'Performed' debuff: -5 defense"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The summer sun empowers your command'") { OverrideColor = new Color(150, 200, 100) });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SpringMaestrosBadge>()
                .AddIngredient<SolsticeBar>(15)
                // TODO: Add L'Estate drop when boss is implemented
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
    
    // ==========================================
    // TIER 3: EARLY HARDMODE (POST-AUTUNNO)
    // ==========================================
    
    /// <summary>
    /// Tier 4: Harvest Beastlord's Horn
    /// Conduct grants minions +30% damage during focus
    /// Killing conducted target extends buff 2s
    /// </summary>
    public class HarvestBeastlordsHorn : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(gold: 4);
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var conductor = player.GetModPlayer<ConductorPlayer>();
            conductor.HasConductorsWand = true;
            conductor.HasSpringMaestrosBadge = true;
            conductor.HasSolarDirectorsCrest = true;
            conductor.HasHarvestBeastlordsHorn = true;
            
            if (Main.mouseRight && Main.mouseRightRelease && player.whoAmI == Main.myPlayer)
            {
                conductor.TryConduct();
            }
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Right-click to Conduct: focus all minions on one target"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Conducted minions deal +30% damage during focus"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Conducted minions heal you 1 HP per hit during focus"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Focused target receives 'Performed' debuff: -5 defense"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Killing conducted target extends focus duration by 2 seconds"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Command the beasts of the harvest'") { OverrideColor = new Color(150, 200, 100) });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SolarDirectorsCrest>()
                .AddIngredient<HarvestBar>(20)
                // TODO: Add Autunno drop when boss is implemented
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    
    // ==========================================
    // TIER 4: POST-MECH (POST-L'INVERNO)
    // ==========================================
    
    /// <summary>
    /// Tier 5: Permafrost Commander's Crown
    /// Conduct cooldown 8s. Conducted target is slowed 25%
    /// </summary>
    public class PermafrostCommandersCrown : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(gold: 8);
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var conductor = player.GetModPlayer<ConductorPlayer>();
            conductor.HasConductorsWand = true;
            conductor.HasSpringMaestrosBadge = true;
            conductor.HasSolarDirectorsCrest = true;
            conductor.HasHarvestBeastlordsHorn = true;
            conductor.HasPermafrostCommandersCrown = true;
            
            if (Main.mouseRight && Main.mouseRightRelease && player.whoAmI == Main.myPlayer)
            {
                conductor.TryConduct();
            }
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Right-click to Conduct: focus all minions on one target"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Conducted minions deal +30% damage during focus"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "8 second cooldown"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Conducted minions heal you 1 HP per hit during focus"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Focused target receives 'Performed' debuff: -5 defense"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Conducted target is slowed by 25%"));
            tooltips.Add(new TooltipLine(Mod, "Effect7", "Killing conducted target extends focus duration by 2 seconds"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Winter's chill commands absolute obedience'") { OverrideColor = new Color(150, 200, 100) });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<HarvestBeastlordsHorn>()
                .AddIngredient<PermafrostBar>(25)
                // TODO: Add L'Inverno drop when boss is implemented
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    
    /// <summary>
    /// Tier 6: Vivaldi's Orchestra Baton
    /// New "Scatter" command: Double-tap Conduct to spread minions to all nearby enemies
    /// </summary>
    public class VivaldisOrchestraBaton : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.rare = ItemRarityID.Lime;
            Item.value = Item.sellPrice(gold: 15);
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var conductor = player.GetModPlayer<ConductorPlayer>();
            conductor.HasConductorsWand = true;
            conductor.HasSpringMaestrosBadge = true;
            conductor.HasSolarDirectorsCrest = true;
            conductor.HasHarvestBeastlordsHorn = true;
            conductor.HasPermafrostCommandersCrown = true;
            conductor.HasVivaldisOrchestraBaton = true;
            
            if (Main.mouseRight && Main.mouseRightRelease && player.whoAmI == Main.myPlayer)
            {
                conductor.TryConduct();
            }
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Right-click to Conduct: focus all minions on one target"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Double-tap Conduct to Scatter: spread minions to all nearby enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Conducted minions deal +30% damage during focus"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "8 second cooldown"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Conducted minions heal you 1 HP per hit during focus"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Focused target receives 'Performed' debuff: -5 defense"));
            tooltips.Add(new TooltipLine(Mod, "Effect7", "Conducted target is slowed by 25%"));
            tooltips.Add(new TooltipLine(Mod, "Effect8", "Killing conducted target extends focus duration by 2 seconds"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The four seasons unite under your baton'") { OverrideColor = new Color(150, 200, 100) });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<PermafrostCommandersCrown>()
                .AddIngredient<CycleOfSeasons>()
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
