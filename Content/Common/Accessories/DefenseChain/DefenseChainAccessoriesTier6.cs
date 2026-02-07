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
                player.lifeRegen += 15; // Enhanced from Fate's 10
            }
            else
            {
                player.lifeRegen += 10;
            }
            
            // Thorns and dodge
            player.thorns = 0.28f; // Slightly better than Fate
            player.blackBelt = true;
            
            // Damage boost
            player.GetDamage(DamageClass.Generic) += 0.12f;
            
            // Constellation particles
            if (!hideVisual && Main.rand.NextBool(10))
            {
                float angle = Main.GameUpdateCount * 0.02f + Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 dustPos = player.Center + angle.ToRotationVector2() * Main.rand.NextFloat(30f, 50f);
                
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.GoldCoin, Vector2.Zero);
                dust.noGravity = true;
                dust.scale = 0.5f;
                dust.color = NachtmusikGold;
            }
            
            // Star particles at night
            if (!hideVisual && !Main.dayTime && Main.rand.NextBool(15))
            {
                Vector2 starPos = player.Center + Main.rand.NextVector2Circular(45f, 55f);
                Dust star = Dust.NewDustPerfect(starPos, DustID.MagicMirror, Vector2.Zero);
                star.noGravity = true;
                star.scale = 0.5f;
                star.color = NachtmusikSilver;
            }
            
            // Shield glow
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.03f) * 0.08f + 0.22f;
            Lighting.AddLight(player.Center, NachtmusikPurple.ToVector3() * pulse);
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "System", "Resonant Shield System - STELLAR AEGIS")
            {
                OverrideColor = NachtmusikPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "70% max HP as regenerating shield")
            {
                OverrideColor = NachtmusikGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "At night: +50% shield regen rate")
            {
                OverrideColor = NachtmusikPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+38 defense, +12% damage, 28% thorns")
            {
                OverrideColor = NachtmusikSilver
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Shield displays constellation patterns")
            {
                OverrideColor = NachtmusikGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Inherits ALL previous shield abilities")
            {
                OverrideColor = new Color(200, 180, 220)
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
            player.lifeRegen += 12;
            
            // Enhanced thorns - fiery
            player.thorns = 0.35f;
            player.blackBelt = true;
            
            // Damage boost
            player.GetDamage(DamageClass.Generic) += 0.14f;
            
            // Infernal particles
            if (!hideVisual && Main.rand.NextBool(8))
            {
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(40f, 50f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.2f, 0.2f), Main.rand.NextFloat(-0.8f, -0.2f));
                
                Dust dust = Dust.NewDustPerfect(pos, DustID.Torch, vel);
                dust.noGravity = true;
                dust.scale = 0.7f;
                dust.color = DiesIraeOrange;
            }
            
            // Rising embers on shield
            if (!hideVisual && Main.rand.NextBool(15))
            {
                Vector2 emberPos = player.Center + new Vector2(Main.rand.NextFloat(-30f, 30f), 20f);
                Dust ember = Dust.NewDustPerfect(emberPos, DustID.FlameBurst, Vector2.UnitY * -1f);
                ember.noGravity = true;
                ember.scale = 0.5f;
            }
            
            // Infernal glow
            float flicker = Main.rand.NextFloat(0.8f, 1f);
            Lighting.AddLight(player.Center, DiesIraeOrange.ToVector3() * 0.25f * flicker);
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "System", "Resonant Shield System - INFERNAL FORTRESS")
            {
                OverrideColor = DiesIraeCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "80% max HP as regenerating shield")
            {
                OverrideColor = DiesIraeOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Shield hits inflict Hellfire on attackers")
            {
                OverrideColor = DiesIraeCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+42 defense, +14% damage, 35% thorns")
            {
                OverrideColor = DiesIraeOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Shield break causes infernal shockwave")
            {
                OverrideColor = DiesIraeCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Inherits ALL previous shield abilities")
            {
                OverrideColor = new Color(200, 180, 220)
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
            player.lifeRegen += 15;
            
            // Thorns and dodge
            player.thorns = 0.38f;
            player.blackBelt = true;
            
            // Damage boost
            player.GetDamage(DamageClass.Generic) += 0.15f;
            
            // Iridescent particles
            if (!hideVisual && Main.rand.NextBool(8))
            {
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(42f, 52f);
                float hue = (Main.GameUpdateCount * 0.012f + Main.rand.NextFloat()) % 1f;
                Color shimmerColor = Main.hslToRgb(hue, 0.5f, 0.85f);
                
                Dust dust = Dust.NewDustPerfect(pos, DustID.RainbowMk2, Main.rand.NextVector2Circular(0.4f, 0.4f));
                dust.noGravity = true;
                dust.scale = 0.55f;
                dust.color = shimmerColor;
            }
            
            // Rose petals
            if (!hideVisual && Main.rand.NextBool(12))
            {
                Vector2 petalPos = player.Center + Main.rand.NextVector2Circular(45f, 55f);
                Dust petal = Dust.NewDustPerfect(petalPos, DustID.PinkFairy, new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), Main.rand.NextFloat(-0.3f, 0.3f)));
                petal.noGravity = true;
                petal.scale = 0.65f;
            }
            
            // Prismatic glow
            float hueShift = (Main.GameUpdateCount * 0.007f) % 1f;
            Color lightColor = Main.hslToRgb(hueShift, 0.3f, 0.7f);
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.06f + 0.22f;
            Lighting.AddLight(player.Center, lightColor.ToVector3() * pulse);
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "System", "Resonant Shield System - JUBILANT FORTRESS")
            {
                OverrideColor = OdeToJoyIridescent
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "90% max HP as regenerating shield")
            {
                OverrideColor = OdeToJoyWhite
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Hits absorbed heal 5% of shield damage as HP")
            {
                OverrideColor = OdeToJoyRose
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+45 defense, +15% damage, 38% thorns")
            {
                OverrideColor = OdeToJoyIridescent
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Shield break triggers joyful celebration burst")
            {
                OverrideColor = OdeToJoyRose
            });
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Inherits ALL previous shield abilities")
            {
                OverrideColor = new Color(200, 180, 220)
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
            
            // Temporal/clockwork particles
            if (!hideVisual && Main.rand.NextBool(10))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 gearPos = player.Center + angle.ToRotationVector2() * Main.rand.NextFloat(35f, 55f);
                
                Dust dust = Dust.NewDustPerfect(gearPos, DustID.Enchanted_Gold, Vector2.Zero);
                dust.noGravity = true;
                dust.scale = 0.5f;
                dust.color = ClairDeLuneBrass;
            }
            
            // Temporal flame wisps
            if (!hideVisual && Main.rand.NextBool(12))
            {
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(40f, 50f);
                Dust dust = Dust.NewDustPerfect(pos, DustID.PinkFairy, new Vector2(0, Main.rand.NextFloat(-0.4f, 0.2f)));
                dust.noGravity = true;
                dust.scale = 0.55f;
                dust.color = ClairDeLuneCrimson;
            }
            
            // Temporal glow
            float timeShift = Main.GameUpdateCount * 0.012f;
            float pulse = (float)Math.Sin(timeShift) * 0.08f + 0.24f;
            Color lightColor = Color.Lerp(ClairDeLuneCrimson, ClairDeLuneBrass, (float)Math.Sin(timeShift * 0.5f) * 0.5f + 0.5f);
            Lighting.AddLight(player.Center, lightColor.ToVector3() * pulse);
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "System", "Resonant Shield System - ETERNAL BASTION")
            {
                OverrideColor = ClairDeLuneCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Ultimate", "✦✦✦ ULTIMATE DEFENSE ACCESSORY ✦✦✦")
            {
                OverrideColor = ClairDeLuneBrass
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "100% max HP as regenerating shield")
            {
                OverrideColor = ClairDeLuneIridescent
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Shield break: 5s Temporal Stasis invincibility (90s CD)")
            {
                OverrideColor = ClairDeLuneCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+50 defense, +18% damage, 45% thorns")
            {
                OverrideColor = ClairDeLuneBrass
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Shield regenerates 50% faster while standing still")
            {
                OverrideColor = ClairDeLuneIridescent
            });
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Inherits ALL previous shield abilities")
            {
                OverrideColor = new Color(200, 180, 220)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The eternal bastion stands beyond the reach of time'")
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
            player.lifeRegen += 13;
            
            // Enhanced thorns
            player.thorns = 0.36f;
            player.blackBelt = true;
            
            // Combined damage boost
            player.GetDamage(DamageClass.Generic) += 0.15f;
            
            // Dual-theme particles
            if (!hideVisual && Main.rand.NextBool(7))
            {
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(45f, 55f);
                
                if (Main.rand.NextBool())
                {
                    // Stellar
                    Dust dust = Dust.NewDustPerfect(pos, DustID.GoldCoin, Vector2.Zero);
                    dust.noGravity = true;
                    dust.scale = 0.5f;
                    dust.color = NachtmusikPurple;
                }
                else
                {
                    // Infernal
                    Dust dust = Dust.NewDustPerfect(pos, DustID.Torch, Vector2.UnitY * -0.4f);
                    dust.noGravity = true;
                    dust.scale = 0.6f;
                    dust.color = DiesIraeCrimson;
                }
            }
            
            // Combined glow
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.03f) * 0.08f + 0.22f;
            Color lightColor = Color.Lerp(NachtmusikPurple, DiesIraeCrimson, (float)Math.Sin(Main.GameUpdateCount * 0.015f) * 0.5f + 0.5f);
            Lighting.AddLight(player.Center, lightColor.ToVector3() * pulse);
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Fusion", "⚔ STARFALL INFERNAL FUSION ⚔")
            {
                OverrideColor = FusionGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "System", "Resonant Shield System - COSMIC INFERNO")
            {
                OverrideColor = NachtmusikPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Combines Nocturnal Guardian's Ward and Infernal Rampart")
            {
                OverrideColor = DiesIraeCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "85% max HP as regenerating shield")
            {
                OverrideColor = FusionGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Stellar hellfire: +50% thorns damage as fire")
            {
                OverrideColor = DiesIraeCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Inherits ALL abilities from both component accessories")
            {
                OverrideColor = new Color(200, 180, 220)
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
            player.lifeRegen += 16;
            
            // Enhanced thorns
            player.thorns = 0.40f;
            player.blackBelt = true;
            
            // Maximum damage boost
            player.GetDamage(DamageClass.Generic) += 0.17f;
            
            // Triple-theme particles
            if (!hideVisual && Main.rand.NextBool(6))
            {
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(48f, 58f);
                
                int theme = Main.rand.Next(3);
                int dustType = theme switch { 0 => DustID.GoldCoin, 1 => DustID.Torch, _ => DustID.RainbowMk2 };
                Color dustColor = theme switch { 0 => NachtmusikPurple, 1 => DiesIraeCrimson, _ => OdeToJoyWhite };
                
                Dust dust = Dust.NewDustPerfect(pos, dustType, Main.rand.NextVector2Circular(0.35f, 0.35f));
                dust.noGravity = true;
                dust.scale = 0.55f;
                dust.color = dustColor;
            }
            
            // Triumphant glow
            float hueShift = (Main.GameUpdateCount * 0.005f) % 1f;
            Color lightColor = Main.hslToRgb(hueShift, 0.35f, 0.65f);
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.03f) * 0.08f + 0.24f;
            Lighting.AddLight(player.Center, lightColor.ToVector3() * pulse);
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Fusion", "⚔ TRIUMPHANT JUBILANT FUSION ⚔")
            {
                OverrideColor = FusionTriumph
            });
            
            tooltips.Add(new TooltipLine(Mod, "System", "Resonant Shield System - TRIPLE HARMONY")
            {
                OverrideColor = NachtmusikPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Combines Starfall Infernal Shield with Jubilant Bulwark")
            {
                OverrideColor = DiesIraeCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "95% max HP as regenerating shield")
            {
                OverrideColor = OdeToJoyWhite
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Shield hits heal 8% of damage absorbed")
            {
                OverrideColor = FusionTriumph
            });
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Inherits ALL abilities from all three theme accessories")
            {
                OverrideColor = new Color(200, 180, 220)
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
            player.statDefense += 55;
            
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
            player.thorns = 0.50f;
            player.blackBelt = true;
            
            // Maximum damage boost
            player.GetDamage(DamageClass.Generic) += 0.20f;
            
            // Quad-theme particle spectacle
            if (!hideVisual && Main.rand.NextBool(5))
            {
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(52f, 62f);
                
                int theme = Main.rand.Next(4);
                int dustType = theme switch { 0 => DustID.GoldCoin, 1 => DustID.Torch, 2 => DustID.RainbowMk2, _ => DustID.Enchanted_Gold };
                Color dustColor = theme switch { 0 => NachtmusikPurple, 1 => DiesIraeCrimson, 2 => OdeToJoyWhite, _ => ClairDeLuneBrass };
                
                Dust dust = Dust.NewDustPerfect(pos, dustType, Main.rand.NextVector2Circular(0.4f, 0.4f));
                dust.noGravity = true;
                dust.scale = 0.6f;
                dust.color = dustColor;
            }
            
            // Orbiting quad points
            if (!hideVisual && Main.rand.NextBool(18))
            {
                float angle = Main.GameUpdateCount * 0.015f;
                Color[] colors = { NachtmusikPurple, DiesIraeCrimson, OdeToJoyWhite, ClairDeLuneBrass };
                for (int i = 0; i < 4; i++)
                {
                    float orbitAngle = angle + MathHelper.TwoPi * i / 4f;
                    Vector2 orbitPos = player.Center + orbitAngle.ToRotationVector2() * 55f;
                    
                    Dust dust = Dust.NewDustPerfect(orbitPos, DustID.MagicMirror, Vector2.Zero);
                    dust.noGravity = true;
                    dust.scale = 0.45f;
                    dust.color = colors[i];
                }
            }
            
            // Ultimate prismatic glow
            float timeShift = Main.GameUpdateCount * 0.008f;
            float pulse = (float)Math.Sin(timeShift * 2f) * 0.1f + 0.3f;
            float hue = (timeShift * 0.35f) % 1f;
            Color lightColor = Main.hslToRgb(hue, 0.45f, 0.75f);
            Lighting.AddLight(player.Center, lightColor.ToVector3() * pulse);
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Ultimate", "✦✦✦ ETERNAL BASTION - ULTIMATE FUSION ✦✦✦")
            {
                OverrideColor = UltimatePrismatic
            });
            
            tooltips.Add(new TooltipLine(Mod, "System", "Resonant Shield System - GRAND AEGIS")
            {
                OverrideColor = ClairDeLuneBrass
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Combines ALL four Post-Fate theme accessories")
            {
                OverrideColor = NachtmusikPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "120% max HP as regenerating shield")
            {
                OverrideColor = DiesIraeCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Eternal Stasis: 8s invincibility on shield break (60s CD)")
            {
                OverrideColor = OdeToJoyWhite
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect4", "+55 defense, +20% damage, 50% thorns")
            {
                OverrideColor = ClairDeLuneBrass
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect5", "All shield effects trigger simultaneously")
            {
                OverrideColor = UltimatePrismatic
            });
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Masters ALL abilities from the complete Post-Fate arsenal")
            {
                OverrideColor = new Color(220, 200, 240)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The eternal bastion commands the very fabric of protection'")
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
