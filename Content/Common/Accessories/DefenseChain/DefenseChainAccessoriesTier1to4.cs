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
    // Base accessory - 10% max HP shield, regenerates after 5s
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
            
            // Base defense bonus
            player.statDefense += 2;
            
            // Shield VFX when active
            if (!hideVisual && shieldPlayer.CurrentShield > 0 && Main.rand.NextBool(12))
            {
                Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(25f, 35f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.MagicMirror, 
                    Main.rand.NextVector2Circular(0.3f, 0.5f));
                dust.noGravity = true;
                dust.scale = 0.4f;
                dust.color = new Color(100, 180, 255);
                dust.alpha = 150;
            }
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
    // 15% shield, break releases healing petals (10 HP to allies)
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
            
            // Defense bonus
            player.statDefense += 4;
            
            // Life regen bonus
            player.lifeRegen += 2;
            
            // Spring healing VFX
            if (!hideVisual && shieldPlayer.CurrentShield > 0 && Main.rand.NextBool(10))
            {
                Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(28f, 38f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.PinkTorch, 
                    new Vector2(0, -Main.rand.NextFloat(0.5f, 1.5f)));
                dust.noGravity = true;
                dust.scale = 0.5f;
                dust.color = new Color(255, 180, 200);
            }
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
    // 20% shield, break releases fire nova (damages enemies)
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
            
            // Defense bonus
            player.statDefense += 6;
            
            // Fire immunity
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            
            // Solar flame VFX
            if (!hideVisual && shieldPlayer.CurrentShield > 0 && Main.rand.NextBool(8))
            {
                Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(30f, 40f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.Torch, 
                    new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(1f, 2f)));
                dust.noGravity = true;
                dust.scale = 0.7f;
                dust.color = new Color(255, 140, 50);
            }
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
    // 25% shield, thorns return 15% damage to melee attackers
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
            
            // Defense bonus
            player.statDefense += 8;
            
            // Natural thorns
            player.thorns = 0.15f;
            
            // Thorn VFX
            if (!hideVisual && shieldPlayer.CurrentShield > 0 && Main.rand.NextBool(10))
            {
                Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(32f, 42f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.t_Cactus, 
                    Main.rand.NextVector2Circular(0.5f, 0.8f));
                dust.noGravity = true;
                dust.scale = 0.6f;
                dust.color = new Color(200, 150, 80);
            }
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
    // 30% shield, break freezes attackers for 1s
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
            
            // Defense bonus
            player.statDefense += 10;
            
            // Ice immunity
            player.buffImmune[BuffID.Frostburn] = true;
            player.buffImmune[BuffID.Chilled] = true;
            player.buffImmune[BuffID.Frozen] = true;
            
            // Frost ward VFX
            if (!hideVisual && shieldPlayer.CurrentShield > 0 && Main.rand.NextBool(9))
            {
                Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(34f, 44f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.IceTorch, 
                    Main.rand.NextVector2Circular(0.4f, 0.6f));
                dust.noGravity = true;
                dust.scale = 0.5f;
                dust.color = new Color(150, 200, 255);
            }
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
            
            // Strong defense bonus
            player.statDefense += 14;
            
            // All immunities from previous tiers
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            player.buffImmune[BuffID.Chilled] = true;
            player.buffImmune[BuffID.Frozen] = true;
            
            // Life regen
            player.lifeRegen += 3;
            
            // Thorns
            player.thorns = 0.15f;
            
            // Seasonal VFX based on time
            if (!hideVisual && shieldPlayer.CurrentShield > 0 && Main.rand.NextBool(7))
            {
                Color seasonColor = GetSeasonColor();
                int dustType = GetSeasonDust();
                
                Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(36f, 46f);
                Dust dust = Dust.NewDustPerfect(dustPos, dustType, 
                    Main.rand.NextVector2Circular(0.6f, 1f));
                dust.noGravity = true;
                dust.scale = 0.6f;
                dust.color = seasonColor;
            }
        }
        
        private Color GetSeasonColor()
        {
            // Cycle through seasons based on game time
            int cycle = (int)(Main.GameUpdateCount / 600) % 4;
            return cycle switch
            {
                0 => new Color(255, 180, 200), // Spring pink
                1 => new Color(255, 140, 50),  // Summer orange
                2 => new Color(200, 150, 80),  // Autumn amber
                _ => new Color(150, 200, 255)  // Winter blue
            };
        }
        
        private int GetSeasonDust()
        {
            int cycle = (int)(Main.GameUpdateCount / 600) % 4;
            return cycle switch
            {
                0 => DustID.PinkTorch,
                1 => DustID.Torch,
                2 => DustID.AmberBolt,
                _ => DustID.IceTorch
            };
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
