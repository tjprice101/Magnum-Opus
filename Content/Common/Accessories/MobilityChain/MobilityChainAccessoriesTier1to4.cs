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
            
            // VFX when momentum is building
            if (!hideVisual && momentumPlayer.CurrentMomentum > 30f && Main.rand.NextBool(8))
            {
                Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(15f, 20f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.MagicMirror, Vector2.Zero);
                dust.noGravity = true;
                dust.scale = 0.5f;
                dust.color = new Color(255, 220, 100);
            }
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
            
            // VFX when moving fast
            if (!hideVisual && momentumPlayer.CurrentMomentum >= 50f && Main.rand.NextBool(6))
            {
                Vector2 dustPos = player.Bottom + Main.rand.NextVector2Circular(10f, 5f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.PinkTorch, 
                    new Vector2(0, -Main.rand.NextFloat(1f, 2f)));
                dust.noGravity = true;
                dust.scale = 0.7f;
                dust.color = new Color(255, 180, 200);
            }
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
            
            // VFX aura at high momentum
            if (!hideVisual && momentumPlayer.CurrentMomentum >= 70f && Main.rand.NextBool(4))
            {
                Vector2 dustPos = player.Bottom + Main.rand.NextVector2Circular(12f, 6f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.Torch, 
                    new Vector2(Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(1f, 3f)));
                dust.noGravity = true;
                dust.scale = 1.0f;
            }
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
            
            // Ghost VFX when phasing
            if (!hideVisual && momentumPlayer.CurrentMomentum >= 80f && Main.rand.NextBool(5))
            {
                Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(20f, 30f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.Wraith, 
                    Main.rand.NextVector2Circular(1f, 1f));
                dust.noGravity = true;
                dust.scale = 0.6f;
                dust.color = new Color(180, 120, 200);
            }
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
            
            // Frost VFX at high momentum
            if (!hideVisual && momentumPlayer.CurrentMomentum >= 90f && Main.rand.NextBool(4))
            {
                Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(18f, 25f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.IceTorch, 
                    Main.rand.NextVector2Circular(0.5f, 1f));
                dust.noGravity = true;
                dust.scale = 0.7f;
                dust.color = new Color(150, 200, 255);
            }
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
            
            // Seasonal aura VFX
            if (!hideVisual && momentumPlayer.CurrentMomentum >= 60f && Main.rand.NextBool(4))
            {
                int season = (int)(Main.time / 54000) % 4;
                int dustType = season switch
                {
                    0 => DustID.PinkTorch,    // Spring - Cherry blossom
                    1 => DustID.Torch,        // Summer - Fire
                    2 => DustID.OrangeTorch,  // Autumn - Harvest
                    3 => DustID.IceTorch,     // Winter - Frost
                    _ => DustID.MagicMirror
                };
                
                Color seasonColor = season switch
                {
                    0 => new Color(255, 180, 200),
                    1 => new Color(255, 140, 50),
                    2 => new Color(200, 120, 80),
                    3 => new Color(150, 200, 255),
                    _ => Color.White
                };
                
                Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(20f, 30f);
                Dust dust = Dust.NewDustPerfect(dustPos, dustType, 
                    Main.rand.NextVector2Circular(1f, 2f));
                dust.noGravity = true;
                dust.scale = 0.8f;
                dust.color = seasonColor;
            }
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
