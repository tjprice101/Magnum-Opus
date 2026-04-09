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

namespace MagnumOpus.Content.Common.Accessories.DefenseChain
{
    #region T7: Nocturnal Guardian's Ward (Nachtmusik Theme)
    
    /// <summary>
    /// T7 Defense accessory - Nachtmusik theme (post-Fate).
    /// Starlight shields the bearer in darkness.
    /// 70% shield at night, constellation shield particles.
    /// </summary>
    public class NocturnalGuardiansWard : ModItem
    {
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
            var shieldPlayer = player.GetModPlayer<ResonantShieldPlayer>();
            
            // Set T7 flag (this also implies lower tiers through the ShieldPercent check)
            shieldPlayer.HasNocturnalGuardiansWard = true;
            
            // Defense bonus
            player.statDefense += 38;
            
            // Full immunities from Fate tier
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.buffImmune[BuffID.OnFire3] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            player.buffImmune[BuffID.Chilled] = true;
            player.buffImmune[BuffID.Frozen] = true;
            player.buffImmune[BuffID.Confused] = true;
            player.buffImmune[BuffID.Slow] = true;
            player.lavaImmune = true;
            
            // Enhanced regen at night
            if (!Main.dayTime)
            {
                player.lifeRegen += 18;
            }
            else
            {
                player.lifeRegen += 14;
            }
            
            // Thorns and dodge
            player.thorns = 0.32f;
            player.blackBelt = true;
            
            // Damage boost
            player.GetDamage(DamageClass.Generic) += 0.12f;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "70% of max HP as a regenerating shield")
            {
                OverrideColor = NachtmusikGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+38 defense, +12% damage, 32% thorns, dodge chance")
            {
                OverrideColor = NachtmusikSilver
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+18 life regen at night, +14 during day")
            {
                OverrideColor = NachtmusikPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Immunity to fire, frost, confusion, and slow; lava immunity")
            {
                OverrideColor = NachtmusikSilver
            });
            tooltips.Add(new TooltipLine(Mod, "Effect5", "At night: Sotto Voce — attackers are slowed for 2s")
            {
                OverrideColor = NachtmusikPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The night sky shields those who watch the stars'")
            {
                OverrideColor = new Color(140, 120, 180)
            });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<FatesCosmicAegis>(1)
                .AddIngredient<NachtmusikResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    #endregion
    
    #region T8: Infernal Rampart of Dies Irae (Dies Irae Theme)
    
    /// <summary>
    /// T8 Defense accessory - Dies Irae theme (post-Fate).
    /// Hellfire burns those who strike the shield.
    /// 80% shield, attackers receive hellfire debuff.
    /// </summary>
    public class InfernalRampartOfDiesIrae : ModItem
    {
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
            var shieldPlayer = player.GetModPlayer<ResonantShieldPlayer>();
            
            // Set T8 flag
            shieldPlayer.HasInfernalRampartOfDiesIrae = true;
            
            // Enhanced defense
            player.statDefense += 42;
            
            // Full immunities
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.buffImmune[BuffID.OnFire3] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            player.buffImmune[BuffID.Chilled] = true;
            player.buffImmune[BuffID.Frozen] = true;
            player.buffImmune[BuffID.Confused] = true;
            player.buffImmune[BuffID.Slow] = true;
            player.lavaImmune = true;
            
            // Life regen
            player.lifeRegen += 15;
            
            // Enhanced thorns - fiery
            player.thorns = 0.36f;
            player.blackBelt = true;
            
            // Damage boost
            player.GetDamage(DamageClass.Generic) += 0.14f;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "80% of max HP as a regenerating shield")
            {
                OverrideColor = DiesIraeOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Shield hits inflict hellfire on attackers")
            {
                OverrideColor = DiesIraeCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+42 defense, +14% damage, 36% thorns, dodge chance")
            {
                OverrideColor = DiesIraeOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect4", "+15 life regen, immunity to fire, frost, confusion, and slow")
            {
                OverrideColor = DiesIraeCrimson
            });
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Quantus Tremor — attackers burn and slow for 3s")
            {
                OverrideColor = DiesIraeOrange
            });
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Shield break: Mors Stupebit — nearby enemies confused and armor broken for 2s")
            {
                OverrideColor = DiesIraeCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The wrath of the final day forges unbreakable ramparts'")
            {
                OverrideColor = new Color(180, 100, 80)
            });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<NocturnalGuardiansWard>(1)
                .AddIngredient<DiesIraeResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    #endregion
    
    #region T9: Jubilant Bulwark of Joy (Ode to Joy Theme)
    
    /// <summary>
    /// T9 Defense accessory - Ode to Joy theme (post-Fate).
    /// Joy empowers the shield with healing.
    /// 90% shield, shield hits heal player, celebration burst on break.
    /// </summary>
    public class JubilantBulwarkOfJoy : ModItem
    {
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
            var shieldPlayer = player.GetModPlayer<ResonantShieldPlayer>();
            
            // Set T9 flag
            shieldPlayer.HasJubilantBulwarkOfJoy = true;
            
            // Enhanced defense
            player.statDefense += 45;
            
            // Full immunities
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.buffImmune[BuffID.OnFire3] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            player.buffImmune[BuffID.Chilled] = true;
            player.buffImmune[BuffID.Frozen] = true;
            player.buffImmune[BuffID.Confused] = true;
            player.buffImmune[BuffID.Slow] = true;
            player.lavaImmune = true;
            
            // Enhanced life regen
            player.lifeRegen += 17;
            
            // Thorns and dodge
            player.thorns = 0.40f;
            player.blackBelt = true;
            
            // Damage boost
            player.GetDamage(DamageClass.Generic) += 0.15f;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "90% of max HP as a regenerating shield")
            {
                OverrideColor = OdeToJoyWhite
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Absorbing hits heals 5% of shield damage as HP")
            {
                OverrideColor = OdeToJoyRose
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+45 defense, +15% damage, 40% thorns, dodge chance")
            {
                OverrideColor = OdeToJoyIridescent
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect4", "+17 life regen, immunity to fire, frost, confusion, and slow")
            {
                OverrideColor = OdeToJoyRose
            });
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Hymn of Fortitude — standing still 2s grants +8 def, +5 regen, +5% DR for 6s")
            {
                OverrideColor = OdeToJoyIridescent
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Joy shields the heart from all despair'")
            {
                OverrideColor = new Color(200, 220, 180)
            });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<InfernalRampartOfDiesIrae>(1)
                .AddIngredient<OdeToJoyResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    #endregion
    
    #region T10: Eternal Bastion of the Moonlight (Clair de Lune Theme)
    
    /// <summary>
    /// T10 Defense accessory - Clair de Lune theme (post-Fate).
    /// Time itself cannot break the eternal bastion.
    /// 100% shield, Temporal Stasis on break (invincibility).
    /// </summary>
    public class EternalBastionOfTheMoonlight : ModItem
    {
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
            var shieldPlayer = player.GetModPlayer<ResonantShieldPlayer>();
            
            // Set T10 flag
            shieldPlayer.HasEternalBastionOfTheMoonlight = true;
            
            // Maximum defense
            player.statDefense += 50;
            
            // Full immunities
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.buffImmune[BuffID.OnFire3] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            player.buffImmune[BuffID.Chilled] = true;
            player.buffImmune[BuffID.Frozen] = true;
            player.buffImmune[BuffID.Confused] = true;
            player.buffImmune[BuffID.Slow] = true;
            player.lavaImmune = true;
            
            // Maximum life regen
            player.lifeRegen += 18;
            
            // Maximum thorns
            player.thorns = 0.45f;
            player.blackBelt = true;
            
            // Maximum damage boost
            player.GetDamage(DamageClass.Generic) += 0.18f;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "100% of max HP as a regenerating shield")
            {
                OverrideColor = ClairDeLuneIridescent
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+50% life regeneration while standing still")
            {
                OverrideColor = ClairDeLuneCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect4", "+50 defense, +18% damage, 45% thorns, dodge chance")
            {
                OverrideColor = ClairDeLuneBrass
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect5", "+18 life regen, immunity to fire, frost, confusion, and slow")
            {
                OverrideColor = ClairDeLuneIridescent
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A shield forged from every moonlit movement, unyielding and resolute'")
            {
                OverrideColor = new Color(160, 140, 180)
            });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<JubilantBulwarkOfJoy>(1)
                .AddIngredient<ClairDeLuneResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    #endregion
    
    #region Fusion Tier 1: Starfall Infernal Shield (Nachtmusik + Dies Irae)
    
    /// <summary>
    /// Fusion Tier 1 Defense accessory - combines Nachtmusik and Dies Irae.
    /// Stellar fire forms an impenetrable barrier.
    /// </summary>
    public class StarfallInfernalShield : ModItem
    {
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
            var shieldPlayer = player.GetModPlayer<ResonantShieldPlayer>();
            
            // Set fusion flag
            shieldPlayer.HasStarfallInfernalShield = true;
            
            // Enhanced defense
            player.statDefense += 44;
            
            // Full immunities
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.buffImmune[BuffID.OnFire3] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            player.buffImmune[BuffID.Chilled] = true;
            player.buffImmune[BuffID.Frozen] = true;
            player.buffImmune[BuffID.Confused] = true;
            player.buffImmune[BuffID.Slow] = true;
            player.lavaImmune = true;
            
            // Combined regen
            player.lifeRegen += 16;
            
            // Enhanced thorns
            player.thorns = 0.36f;
            player.blackBelt = true;
            
            // Fusion identity: tankier health pool and mitigation instead of generic damage scaling
            player.statLifeMax2 += 40;
            player.endurance += 0.05f;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "85% of max HP as a regenerating shield")
            {
                OverrideColor = FusionGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Thorns damage inflicts fire, shield hits apply hellfire")
            {
                OverrideColor = DiesIraeCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+44 defense, 36% thorns, dodge chance, +40 max HP, 5% DR")
            {
                OverrideColor = FusionGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect4", "+16 life regen, lava immunity, full debuff immunity")
            {
                OverrideColor = NachtmusikPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Starfall and hellfire unite in eternal defense'")
            {
                OverrideColor = new Color(180, 120, 160)
            });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<NocturnalGuardiansWard>(1)
                .AddIngredient<InfernalRampartOfDiesIrae>(1)
                .AddIngredient<NachtmusikResonantEnergy>(10)
                .AddIngredient<DiesIraeResonantEnergy>(10)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    #endregion
    
    #region Fusion Tier 2: Triumphant Jubilant Aegis (+ Ode to Joy)
    
    /// <summary>
    /// Fusion Tier 2 Defense accessory - adds Ode to Joy to the fusion.
    /// Triple harmony of stellar, infernal, and jubilant shield.
    /// </summary>
    public class TriumphantJubilantAegis : ModItem
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
            var shieldPlayer = player.GetModPlayer<ResonantShieldPlayer>();
            
            // Set fusion flag
            shieldPlayer.HasTriumphantJubilantAegis = true;
            
            // Enhanced defense
            player.statDefense += 48;
            
            // Full immunities
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.buffImmune[BuffID.OnFire3] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            player.buffImmune[BuffID.Chilled] = true;
            player.buffImmune[BuffID.Frozen] = true;
            player.buffImmune[BuffID.Confused] = true;
            player.buffImmune[BuffID.Slow] = true;
            player.lavaImmune = true;
            
            // Enhanced regen
            player.lifeRegen += 18;
            
            // Enhanced thorns
            player.thorns = 0.42f;
            player.blackBelt = true;
            
            // Fusion identity: stronger guard profile, not a generic offense spike
            player.statLifeMax2 += 60;
            player.endurance += 0.07f;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "95% of max HP as a regenerating shield")
            {
                OverrideColor = OdeToJoyWhite
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Absorbing shield hits heals 8% of damage taken")
            {
                OverrideColor = FusionTriumph
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+48 defense, 42% thorns, dodge chance, +60 max HP, 7% DR")
            {
                OverrideColor = FusionTriumph
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect4", "+18 life regen, lava immunity, full debuff immunity")
            {
                OverrideColor = OdeToJoyWhite
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Three harmonies unite in triumphant protection'")
            {
                OverrideColor = new Color(220, 200, 180)
            });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<StarfallInfernalShield>(1)
                .AddIngredient<JubilantBulwarkOfJoy>(1)
                .AddIngredient<OdeToJoyResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    #endregion
    
    #region Fusion Tier 3: Aegis of the Eternal Bastion (Ultimate - + Clair de Lune)
    
    /// <summary>
    /// Ultimate Fusion Defense accessory - all four Post-Fate themes combined.
    /// The pinnacle of the defense system.
    /// </summary>
    public class AegisOfTheEternalBastion : ModItem
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
            var shieldPlayer = player.GetModPlayer<ResonantShieldPlayer>();
            
            // Set ultimate flag
            shieldPlayer.HasAegisOfTheEternalBastion = true;
            
            // Maximum defense
            player.statDefense += 50;
            
            // Full immunities
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.buffImmune[BuffID.OnFire3] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            player.buffImmune[BuffID.Chilled] = true;
            player.buffImmune[BuffID.Frozen] = true;
            player.buffImmune[BuffID.Confused] = true;
            player.buffImmune[BuffID.Slow] = true;
            player.lavaImmune = true;
            
            // Maximum life regen
            player.lifeRegen += 20;
            
            // Maximum thorns
            player.thorns = 0.45f;
            player.blackBelt = true;
            
            // Ultimate identity: extreme durability profile
            player.statLifeMax2 += 80;
            player.endurance += 0.08f;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "100% of max HP as a regenerating shield")
            {
                OverrideColor = UltimatePrismatic
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Shield absorbs up to 50% of incoming damage")
            {
                OverrideColor = ClairDeLuneBrass
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+50 defense, 45% thorns, dodge chance, +80 max HP, 8% damage reduction")
            {
                OverrideColor = UltimatePrismatic
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect4", "+20 life regen, lava immunity, full debuff immunity")
            {
                OverrideColor = ClairDeLuneBrass
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'An unyielding harmony of every shield ever forged'")
            {
                OverrideColor = new Color(200, 180, 160)
            });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<TriumphantJubilantAegis>(1)
                .AddIngredient<EternalBastionOfTheMoonlight>(1)
                .AddIngredient<ClairDeLuneResonantEnergy>(20)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    #endregion
}
