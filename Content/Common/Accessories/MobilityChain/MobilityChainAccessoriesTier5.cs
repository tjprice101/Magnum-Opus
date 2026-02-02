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
            player.moveSpeed += 0.20f;
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
            
            // Moonlit phantom VFX
            if (!hideVisual && momentumPlayer.CurrentMomentum >= 100f && Main.rand.NextBool(4))
            {
                Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(22f, 32f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.PurpleTorch, 
                    Main.rand.NextVector2Circular(0.5f, 1f));
                dust.noGravity = true;
                dust.scale = 0.6f;
                dust.color = new Color(150, 120, 200);
                dust.alpha = 100;
            }
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Enables the Momentum system (max 100)"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+20% movement speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+1.5 max run speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "+1.0 jump speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Night vision and +5% damage at night"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Fire and ice immunity"));
            tooltips.Add(new TooltipLine(Mod, "Effect7", "Semi-transparent at 100+ momentum, reducing enemy aggro"));
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
            player.moveSpeed += 0.22f;
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
            
            // Heroic aura VFX
            if (!hideVisual && momentumPlayer.CurrentMomentum >= 60f && Main.rand.NextBool(4))
            {
                Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(20f, 30f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.RedTorch, 
                    Main.rand.NextVector2Circular(0.8f, 1.2f));
                dust.noGravity = true;
                dust.scale = 0.7f;
                dust.color = new Color(200, 80, 80);
            }
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Enables the Momentum system (max 100)"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+22% movement speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+1.7 max run speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "+1.2 jump speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Grants Shield of Cthulhu dash"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Fire and ice immunity"));
            tooltips.Add(new TooltipLine(Mod, "Effect7", "+8 defense at 60+ momentum"));
            tooltips.Add(new TooltipLine(Mod, "Effect8", "Consume 80 momentum for a powerful dash attack"));
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
            player.moveSpeed += 0.24f;
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
            
            // Infernal flame VFX
            if (!hideVisual && momentumPlayer.CurrentMomentum >= 70f && Main.rand.NextBool(3))
            {
                Vector2 dustPos = player.Bottom + Main.rand.NextVector2Circular(15f, 8f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.Torch, 
                    new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(2f, 4f)));
                dust.noGravity = true;
                dust.scale = 1.1f;
                dust.color = new Color(255, 100, 30);
            }
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Enables the Momentum system (max 100)"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+24% movement speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+1.9 max run speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "+1.4 jump speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Grants Shield of Cthulhu dash"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Complete fire and lava immunity"));
            tooltips.Add(new TooltipLine(Mod, "Effect7", "+8% damage at 70+ momentum"));
            tooltips.Add(new TooltipLine(Mod, "Effect8", "Create an impact crater on landing at 100+ momentum, dealing AoE damage"));
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
            player.moveSpeed += 0.26f;
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
            
            // Enigma void VFX
            if (!hideVisual && momentumPlayer.CurrentMomentum >= 80f && Main.rand.NextBool(5))
            {
                Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(25f, 35f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.PurpleTorch, 
                    Main.rand.NextVector2Circular(0.3f, 0.5f));
                dust.noGravity = true;
                dust.scale = 0.5f;
                dust.color = new Color(140, 60, 200);
            }
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Enables the Momentum system (max 100)"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+26% movement speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+2.1 max run speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "+1.6 jump speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Grants Shield of Cthulhu dash"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Full fire, lava, and confusion immunity"));
            tooltips.Add(new TooltipLine(Mod, "Effect7", "Black belt dodge chance at 80+ momentum"));
            tooltips.Add(new TooltipLine(Mod, "Effect8", "Consume 100 momentum to teleport 12.5 tiles"));
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
            player.moveSpeed += 0.28f;
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
            
            // Swan feather VFX
            if (!hideVisual && momentumPlayer.CurrentMomentum >= 100f && Main.rand.NextBool(4))
            {
                Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(30f, 40f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.Cloud, 
                    new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-0.5f, 1f)));
                dust.noGravity = true;
                dust.scale = 0.9f;
                dust.color = new Color(240, 245, 255);
            }
            
            // Prismatic shimmer at max momentum
            if (!hideVisual && momentumPlayer.CurrentMomentum >= momentumPlayer.MaxMomentum && Main.rand.NextBool(6))
            {
                float hue = (Main.GameUpdateCount * 0.02f) % 1f;
                Color rainbowColor = Main.hslToRgb(hue, 0.8f, 0.7f);
                
                Vector2 shimmerPos = player.Center + Main.rand.NextVector2Circular(25f, 35f);
                Dust shimmer = Dust.NewDustPerfect(shimmerPos, DustID.RainbowTorch, Vector2.Zero);
                shimmer.noGravity = true;
                shimmer.scale = 0.5f;
                shimmer.color = rainbowColor;
            }
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Enables the Momentum system (max 100)"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+28% movement speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+2.3 max run speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "+1.8 jump speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "+100 wing flight time"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "All previous immunities plus black belt dodge"));
            tooltips.Add(new TooltipLine(Mod, "Effect7", "Greatly reduced enemy aggro"));
            tooltips.Add(new TooltipLine(Mod, "Effect8", "50% slower momentum decay"));
            tooltips.Add(new TooltipLine(Mod, "Effect9", "Infinite flight at maximum momentum"));
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
            player.moveSpeed += 0.30f;
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
            
            // Cosmic velocity VFX
            if (!hideVisual && momentumPlayer.CurrentMomentum >= 100f)
            {
                // Dark cosmic trail
                if (Main.rand.NextBool(3))
                {
                    Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(25f, 35f);
                    Dust dust = Dust.NewDustPerfect(dustPos, DustID.PurpleTorch, 
                        Main.rand.NextVector2Circular(0.5f, 1f));
                    dust.noGravity = true;
                    dust.scale = 0.6f;
                    dust.color = new Color(180, 40, 80);
                }
                
                // Star sparkles at max momentum
                if (momentumPlayer.CurrentMomentum >= 150f && Main.rand.NextBool(5))
                {
                    Vector2 starPos = player.Center + Main.rand.NextVector2Circular(35f, 45f);
                    Dust star = Dust.NewDustPerfect(starPos, DustID.MagicMirror, Vector2.Zero);
                    star.noGravity = true;
                    star.scale = 0.4f;
                    star.color = Color.White;
                }
                
                // Glyph accents
                if (momentumPlayer.CurrentMomentum >= 150f && Main.rand.NextBool(15))
                {
                    Vector2 glyphPos = player.Center + Main.rand.NextVector2Circular(40f, 50f);
                    Dust glyph = Dust.NewDustPerfect(glyphPos, DustID.Enchanted_Pink, 
                        Main.rand.NextVector2Circular(0.3f, 0.3f));
                    glyph.noGravity = true;
                    glyph.scale = 0.5f;
                }
            }
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Enables the Momentum system (max 150)"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+30% movement speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+2.5 max run speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "+2.0 jump speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "+150 wing flight time"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "+10% damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect7", "All immunities including slow"));
            tooltips.Add(new TooltipLine(Mod, "Effect8", "Black belt dodge and reduced aggro"));
            tooltips.Add(new TooltipLine(Mod, "Effect9", "At 150 momentum, nearby enemies are slowed by 20%"));
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
