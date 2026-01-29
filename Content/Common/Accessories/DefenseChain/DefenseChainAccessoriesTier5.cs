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

namespace MagnumOpus.Content.Common.Accessories.DefenseChain
{
    // ============================================================
    // THEME TIER 1: MOONLIT GUARDIAN'S VEIL
    // 36% shield, faster regen at night, break grants 2s invisibility
    // ============================================================
    public class MoonlitGuardiansVeil : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.rare = ModContent.RarityType<MoonlightSonataRarity>();
            Item.value = Item.sellPrice(gold: 35, silver: 50);
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var shieldPlayer = player.GetModPlayer<ResonantShieldPlayer>();
            shieldPlayer.HasMoonlitGuardiansVeil = true;
            
            // Strong defense
            player.statDefense += 18;
            
            // Full immunities from previous tiers
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            player.buffImmune[BuffID.Chilled] = true;
            player.buffImmune[BuffID.Frozen] = true;
            
            // Life regen
            player.lifeRegen += 4;
            
            // Thorns
            player.thorns = 0.15f;
            
            // Night vision
            player.nightVision = true;
            
            // Night power boost
            if (!Main.dayTime)
            {
                player.statDefense += 5;
                player.GetDamage(DamageClass.Generic) += 0.05f;
            }
            
            // Moonlit veil VFX
            if (!hideVisual && shieldPlayer.CurrentShield > 0 && Main.rand.NextBool(8))
            {
                Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(36f, 46f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.PurpleTorch, 
                    Main.rand.NextVector2Circular(0.4f, 0.7f));
                dust.noGravity = true;
                dust.scale = 0.5f;
                dust.color = new Color(150, 120, 200);
                dust.alpha = 80;
            }
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<VivaldisSeasonalBulwark>()
                .AddIngredient<ResonantCoreOfMoonlightSonata>()
                .AddIngredient(ItemID.LunarBar, 8)
                .AddIngredient(ItemID.Ectoplasm, 10)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    // ============================================================
    // THEME TIER 2: HEROIC VALOR'S AEGIS
    // 38% shield, break grants +15% damage for 5s
    // ============================================================
    public class HeroicValorsAegis : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.rare = ModContent.RarityType<EroicaRarity>();
            Item.value = Item.sellPrice(gold: 40);
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var shieldPlayer = player.GetModPlayer<ResonantShieldPlayer>();
            shieldPlayer.HasHeroicValorsAegis = true;
            
            // Strong defense
            player.statDefense += 20;
            
            // Full immunities
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            player.buffImmune[BuffID.Chilled] = true;
            player.buffImmune[BuffID.Frozen] = true;
            
            // Life regen
            player.lifeRegen += 5;
            
            // Thorns
            player.thorns = 0.18f;
            
            // Base damage boost
            player.GetDamage(DamageClass.Generic) += 0.05f;
            
            // Heroic aura VFX
            if (!hideVisual && shieldPlayer.CurrentShield > 0 && Main.rand.NextBool(7))
            {
                Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(38f, 48f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.RedTorch, 
                    Main.rand.NextVector2Circular(0.6f, 1f));
                dust.noGravity = true;
                dust.scale = 0.6f;
                dust.color = new Color(200, 80, 80);
            }
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<MoonlitGuardiansVeil>()
                .AddIngredient<ResonantCoreOfEroica>()
                .AddIngredient(ItemID.BeetleHusk, 8)
                .AddIngredient(ItemID.LunarBar, 5)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    // ============================================================
    // THEME TIER 3: INFERNAL BELL'S FORTRESS
    // 40% shield, break releases massive bell shockwave
    // ============================================================
    public class InfernalBellsFortress : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.rare = ModContent.RarityType<LaCampanellaRarity>();
            Item.value = Item.sellPrice(gold: 45);
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var shieldPlayer = player.GetModPlayer<ResonantShieldPlayer>();
            shieldPlayer.HasInfernalBellsFortress = true;
            
            // Very strong defense
            player.statDefense += 24;
            
            // Full immunities including hellfire
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.buffImmune[BuffID.OnFire3] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            player.buffImmune[BuffID.Chilled] = true;
            player.buffImmune[BuffID.Frozen] = true;
            player.lavaImmune = true;
            
            // Life regen
            player.lifeRegen += 6;
            
            // Enhanced thorns
            player.thorns = 0.20f;
            
            // Fire damage boost
            player.GetDamage(DamageClass.Generic) += 0.08f;
            
            // Infernal fortress VFX
            if (!hideVisual && shieldPlayer.CurrentShield > 0 && Main.rand.NextBool(6))
            {
                Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(40f, 50f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.Torch, 
                    new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -Main.rand.NextFloat(1f, 2f)));
                dust.noGravity = true;
                dust.scale = 0.8f;
                dust.color = new Color(255, 100, 30);
            }
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<HeroicValorsAegis>()
                .AddIngredient<ResonantCoreOfLaCampanella>()
                .AddIngredient(ItemID.HellstoneBar, 15)
                .AddIngredient(ItemID.LunarBar, 5)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    // ============================================================
    // THEME TIER 4: ENIGMA'S VOID SHELL
    // 45% shield, 10% chance to phase through attacks entirely
    // ============================================================
    public class EnigmasVoidShell : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.rare = ModContent.RarityType<EnigmaVariationsRarity>();
            Item.value = Item.sellPrice(gold: 50);
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var shieldPlayer = player.GetModPlayer<ResonantShieldPlayer>();
            shieldPlayer.HasEnigmasVoidShell = true;
            
            // Very strong defense
            player.statDefense += 26;
            
            // Full immunities
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.buffImmune[BuffID.OnFire3] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            player.buffImmune[BuffID.Chilled] = true;
            player.buffImmune[BuffID.Frozen] = true;
            player.buffImmune[BuffID.Confused] = true;
            player.lavaImmune = true;
            
            // Life regen
            player.lifeRegen += 7;
            
            // Thorns
            player.thorns = 0.20f;
            
            // Black Belt dodge synergy
            player.blackBelt = true;
            
            // Void shell VFX
            if (!hideVisual && shieldPlayer.CurrentShield > 0 && Main.rand.NextBool(8))
            {
                Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(42f, 52f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.PurpleTorch, 
                    Main.rand.NextVector2Circular(0.3f, 0.5f));
                dust.noGravity = true;
                dust.scale = 0.5f;
                dust.color = new Color(140, 60, 200);
            }
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<InfernalBellsFortress>()
                .AddIngredient<ResonantCoreOfEnigma>()
                .AddIngredient(ItemID.RodofDiscord)
                .AddIngredient(ItemID.LunarBar, 5)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    // ============================================================
    // THEME TIER 5: SWAN'S IMMORTAL GRACE
    // 50% shield, +5% dodge chance at full shield
    // ============================================================
    public class SwansImmortalGrace : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.rare = ModContent.RarityType<SwanRarity>();
            Item.value = Item.sellPrice(gold: 55);
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var shieldPlayer = player.GetModPlayer<ResonantShieldPlayer>();
            shieldPlayer.HasSwansImmortalGrace = true;
            
            // Excellent defense
            player.statDefense += 30;
            
            // Full immunities
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.buffImmune[BuffID.OnFire3] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            player.buffImmune[BuffID.Chilled] = true;
            player.buffImmune[BuffID.Frozen] = true;
            player.buffImmune[BuffID.Confused] = true;
            player.lavaImmune = true;
            
            // Strong life regen
            player.lifeRegen += 8;
            
            // Thorns
            player.thorns = 0.22f;
            
            // Dodge synergy
            player.blackBelt = true;
            
            // Reduced enemy aggro (graceful presence)
            player.aggro -= 300;
            
            // Swan feather VFX
            if (!hideVisual && shieldPlayer.CurrentShield > 0 && Main.rand.NextBool(7))
            {
                Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(44f, 54f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.Cloud, 
                    new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-0.3f, 0.5f)));
                dust.noGravity = true;
                dust.scale = 0.7f;
                dust.color = new Color(240, 245, 255);
            }
            
            // Prismatic shimmer at full shield
            if (!hideVisual && shieldPlayer.CurrentShield >= shieldPlayer.MaxShield && Main.rand.NextBool(10))
            {
                float hue = (Main.GameUpdateCount * 0.02f) % 1f;
                Color rainbowColor = Main.hslToRgb(hue, 0.8f, 0.7f);
                
                Vector2 shimmerPos = player.Center + Main.rand.NextVector2Circular(35f, 45f);
                Dust shimmer = Dust.NewDustPerfect(shimmerPos, DustID.RainbowTorch, Vector2.Zero);
                shimmer.noGravity = true;
                shimmer.scale = 0.4f;
                shimmer.color = rainbowColor;
            }
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<EnigmasVoidShell>()
                .AddIngredient<ResonantCoreOfSwanLake>()
                .AddIngredient(ItemID.SoulofFlight, 20)
                .AddIngredient(ItemID.LunarBar, 5)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    // ============================================================
    // THEME TIER 6: FATE'S COSMIC AEGIS
    // 60% shield, break triggers "Last Stand" - 3s invincibility, 2min cooldown
    // ============================================================
    public class FatesCosmicAegis : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.value = Item.sellPrice(gold: 65);
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var shieldPlayer = player.GetModPlayer<ResonantShieldPlayer>();
            shieldPlayer.HasFatesCosmicAegis = true;
            
            // Maximum defense
            player.statDefense += 35;
            
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
            player.lifeRegen += 10;
            
            // Maximum thorns
            player.thorns = 0.25f;
            
            // All dodge synergies
            player.blackBelt = true;
            
            // Damage boost
            player.GetDamage(DamageClass.Generic) += 0.10f;
            
            // Cosmic VFX
            if (!hideVisual && shieldPlayer.CurrentShield > 0)
            {
                // Dark cosmic trail
                if (Main.rand.NextBool(6))
                {
                    Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(46f, 56f);
                    Dust dust = Dust.NewDustPerfect(dustPos, DustID.PurpleTorch, 
                        Main.rand.NextVector2Circular(0.4f, 0.7f));
                    dust.noGravity = true;
                    dust.scale = 0.5f;
                    dust.color = new Color(180, 40, 80);
                }
                
                // Star sparkles at full shield
                if (shieldPlayer.CurrentShield >= shieldPlayer.MaxShield && Main.rand.NextBool(10))
                {
                    Vector2 starPos = player.Center + Main.rand.NextVector2Circular(50f, 60f);
                    Dust star = Dust.NewDustPerfect(starPos, DustID.MagicMirror, Vector2.Zero);
                    star.noGravity = true;
                    star.scale = 0.4f;
                    star.color = Color.White;
                }
                
                // Glyph accents
                if (Main.rand.NextBool(20))
                {
                    Vector2 glyphPos = player.Center + Main.rand.NextVector2Circular(55f, 65f);
                    Dust glyph = Dust.NewDustPerfect(glyphPos, DustID.Enchanted_Pink, 
                        Main.rand.NextVector2Circular(0.2f, 0.2f));
                    glyph.noGravity = true;
                    glyph.scale = 0.4f;
                }
            }
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SwansImmortalGrace>()
                .AddIngredient<ResonantCoreOfFate>()
                .AddIngredient<FateResonantEnergy>(5)
                .AddIngredient(ItemID.LunarBar, 10)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}
