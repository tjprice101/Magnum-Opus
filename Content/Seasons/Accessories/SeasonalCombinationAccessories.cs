using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using MagnumOpus.Content.Spring.Materials;
using MagnumOpus.Content.Spring.Accessories;
using MagnumOpus.Content.Summer.Materials;
using MagnumOpus.Content.Summer.Accessories;
using MagnumOpus.Content.Autumn.Materials;
using MagnumOpus.Content.Autumn.Accessories;
using MagnumOpus.Content.Winter.Materials;
using MagnumOpus.Content.Winter.Accessories;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems;
using static MagnumOpus.Common.Systems.ThemedParticles;
using System;

namespace MagnumOpus.Content.Seasons.Accessories
{
    /// <summary>
    /// Relic of the Equinox - Spring + Autumn combination
    /// Combines growth and decay powers - balanced life and death
    /// </summary>
    public class RelicOfTheEquinox : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.rare = ItemRarityID.Cyan;
            Item.value = Item.sellPrice(gold: 20);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // Spring bonuses
            player.lifeRegen += 4;
            player.GetDamage(DamageClass.Generic) += 0.08f;
            
            // Autumn bonuses
            player.GetCritChance(DamageClass.Generic) += 8;
            player.statDefense += 10;
            
            // Equinox unique: life steal + thorns
            player.thorns = 0.8f;
            player.GetModPlayer<ReapersCharmPlayer>().reapersCharmEquipped = true;
            
            // Dual visual - spring and autumn particles alternating with music notes!
            if (!hideVisual && Main.rand.NextBool(8))
            {
                bool isSpring = Main.GameUpdateCount % 120 < 60;
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(40f, 40f);
                
                if (isSpring)
                {
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(0.5f, 1.5f));
                    Color springColor = Color.Lerp(new Color(255, 183, 197), new Color(144, 238, 144), Main.rand.NextFloat());
                    CustomParticles.GenericGlow(pos, vel, springColor, 0.28f, 26, true);
                    
                    // ☁ESPARKLE accent
                    var sparkle = new SparkleParticle(pos, vel * 0.5f, springColor * 0.5f, 0.2f, 18);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
                else
                {
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(0.5f, 1.5f));
                    Color autumnColor = Color.Lerp(new Color(255, 100, 30), new Color(139, 69, 19), Main.rand.NextFloat());
                    CustomParticles.GenericGlow(pos, vel, autumnColor, 0.28f, 26, true);
                    
                    // ☁EGLYPH for autumn decay theme
                    if (Main.rand.NextBool(3))
                        CustomParticles.Glyph(pos, autumnColor * 0.6f, 0.3f, -1);
                }
            }
            
            // ☁EMUSICAL NOTATION - Equinox harmony! - VISIBLE SCALE 0.68f+
            if (!hideVisual && Main.rand.NextBool(18))
            {
                bool isSpring = Main.GameUpdateCount % 120 < 60;
                Color noteColor = isSpring ? new Color(255, 183, 197) : new Color(255, 100, 30);
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(25f, 25f);
                Vector2 noteVel = new Vector2(0, isSpring ? -1.2f : 0.8f);
                ThemedParticles.MusicNote(notePos, noteVel, noteColor * 0.85f, 0.68f, 28);
            }
            
            // Balanced light
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.03f) * 0.5f + 0.5f;
            Color lightColor = Color.Lerp(new Color(200, 255, 200), new Color(255, 150, 50), pulse);
            Lighting.AddLight(player.Center, lightColor.ToVector3() * 0.4f);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color springGreen = new Color(144, 238, 144);
            Color autumnOrange = new Color(255, 100, 30);
            
            tooltips.Add(new TooltipLine(Mod, "Combo", "Spring + Autumn Combination") { OverrideColor = Color.Lerp(springGreen, autumnOrange, 0.5f) });
            tooltips.Add(new TooltipLine(Mod, "Spring", "+4 life regen, +8% damage") { OverrideColor = springGreen });
            tooltips.Add(new TooltipLine(Mod, "Autumn", "+8% crit chance, +10 defense") { OverrideColor = autumnOrange });
            tooltips.Add(new TooltipLine(Mod, "LifeSteal", "20% chance to life steal on hit") { OverrideColor = autumnOrange });
            tooltips.Add(new TooltipLine(Mod, "Thorns", "Attackers take 80% damage back") { OverrideColor = autumnOrange });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Balance between life and death, growth and decay'") { OverrideColor = Color.Lerp(springGreen, autumnOrange, 0.5f) });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<BloomCrest>(), 1)
                .AddIngredient(ModContent.ItemType<GrowthBand>(), 1)
                .AddIngredient(ModContent.ItemType<ReapersCharm>(), 1)
                .AddIngredient(ModContent.ItemType<TwilightRing>(), 1)
                .AddIngredient(ModContent.ItemType<DormantSpringCore>(), 1)
                .AddIngredient(ModContent.ItemType<DormantAutumnCore>(), 1)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    /// <summary>
    /// Solstice Ring - Summer + Winter combination
    /// Combines fire and ice powers - extreme temperature
    /// </summary>
    public class SolsticeRing : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.rare = ItemRarityID.Cyan;
            Item.value = Item.sellPrice(gold: 20);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // Summer bonuses
            player.GetDamage(DamageClass.Generic) += 0.1f;
            player.magmaStone = true; // On Fire
            player.GetAttackSpeed(DamageClass.Generic) += 0.1f;
            
            // Winter bonuses
            player.frostBurn = true; // Frostburn
            player.statDefense += 10;
            player.buffImmune[BuffID.Frozen] = true;
            player.buffImmune[BuffID.OnFire] = true;
            
            // Solstice unique: extreme damage when at full HP or low HP
            float hpPercent = (float)player.statLife / player.statLifeMax2;
            if (hpPercent > 0.9f || hpPercent < 0.25f)
            {
                player.GetDamage(DamageClass.Generic) += 0.08f; // +8% more at extremes
                player.GetCritChance(DamageClass.Generic) += 6;
            }
            
            // Dual visual - fire and ice with music notes!
            if (!hideVisual && Main.rand.NextBool(6))
            {
                bool isFire = Main.rand.NextBool();
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(38f, 38f);
                
                if (isFire)
                {
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(1f, 2f));
                    Color fireColor = Color.Lerp(new Color(255, 140, 0), new Color(255, 215, 0), Main.rand.NextFloat());
                    CustomParticles.GenericGlow(pos, vel, fireColor, 0.3f, 22, true);
                    
                    // ☁ESPARKLE accent
                    var sparkle = new SparkleParticle(pos, vel * 0.5f, fireColor * 0.55f, 0.2f, 16);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
                else
                {
                    Vector2 vel = Main.rand.NextVector2Circular(1f, 1f) + new Vector2(0, 0.5f);
                    Color iceColor = Color.Lerp(new Color(173, 216, 230), new Color(0, 255, 255), Main.rand.NextFloat());
                    CustomParticles.GenericGlow(pos, vel, iceColor, 0.3f, 24, true);
                    
                    // ☁ESPARKLE accent for ice crystalline effect
                    var sparkle = new SparkleParticle(pos, vel * 0.4f, iceColor * 0.6f, 0.22f, 18);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }
            
            // ☁EMUSICAL NOTATION - Solstice melody! - VISIBLE SCALE 0.68f+
            if (!hideVisual && Main.rand.NextBool(16))
            {
                bool isFire = Main.rand.NextBool();
                Color noteColor = isFire ? new Color(255, 140, 0) : new Color(150, 220, 255);
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(22f, 22f);
                Vector2 noteVel = new Vector2(0, isFire ? -1.5f : 0.8f);
                ThemedParticles.MusicNote(notePos, noteVel, noteColor * 0.85f, 0.68f, 26);
            }
            
            // Dual colored light
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.5f + 0.5f;
            Color lightColor = Color.Lerp(new Color(255, 160, 50), new Color(150, 200, 255), pulse);
            Lighting.AddLight(player.Center, lightColor.ToVector3() * 0.45f);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color summerOrange = new Color(255, 140, 0);
            Color winterCyan = new Color(0, 255, 255);
            
            tooltips.Add(new TooltipLine(Mod, "Combo", "Summer + Winter Combination") { OverrideColor = Color.Lerp(summerOrange, winterCyan, 0.5f) });
            tooltips.Add(new TooltipLine(Mod, "Summer", "+10% damage, +10% attack speed, inflicts On Fire!") { OverrideColor = summerOrange });
            tooltips.Add(new TooltipLine(Mod, "Winter", "+10 defense, inflicts Frostburn") { OverrideColor = winterCyan });
            tooltips.Add(new TooltipLine(Mod, "Extreme", "At 90%+ or 25%- HP: +8% additional damage, +6% crit") { OverrideColor = Color.Lerp(summerOrange, winterCyan, 0.5f) });
            tooltips.Add(new TooltipLine(Mod, "Immunity", "Immune to Frozen and On Fire!") { OverrideColor = winterCyan });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Fire and ice united in perfect opposition'") { OverrideColor = Color.Lerp(summerOrange, winterCyan, 0.5f) });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SunfirePendant>(), 1)
                .AddIngredient(ModContent.ItemType<RadiantCrown>(), 1)
                .AddIngredient(ModContent.ItemType<FrostbiteAmulet>(), 1)
                .AddIngredient(ModContent.ItemType<GlacialHeart>(), 1)
                .AddIngredient(ModContent.ItemType<DormantSummerCore>(), 1)
                .AddIngredient(ModContent.ItemType<DormantWinterCore>(), 1)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    /// <summary>
    /// Cycle of Seasons - All 4 seasons combined
    /// The penultimate seasonal accessory
    /// </summary>
    public class CycleOfSeasons : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 34;
            Item.accessory = true;
            Item.rare = ItemRarityID.Red;
            Item.value = Item.sellPrice(gold: 40);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // Combined bonuses from all seasons
            player.GetDamage(DamageClass.Generic) += 0.15f;
            player.GetCritChance(DamageClass.Generic) += 12;
            player.GetAttackSpeed(DamageClass.Generic) += 0.08f;
            player.statDefense += 15;
            player.lifeRegen += 5;
            player.endurance += 0.08f; // 8% DR
            
            // Elemental effects
            player.magmaStone = true;
            player.frostBurn = true;
            player.thorns = 1f;
            
            // Immunities
            player.buffImmune[BuffID.Frozen] = true;
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            
            // Life steal
            player.GetModPlayer<ReapersCharmPlayer>().reapersCharmEquipped = true;
            
            // Cycling season visual based on in-game time
            int season = (int)((Main.time / 10000) % 4);
            
            if (!hideVisual && Main.rand.NextBool(5))
            {
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(45f, 45f);
                Color seasonColor;
                Vector2 vel;
                
                switch (season)
                {
                    case 0: // Spring
                        vel = new Vector2(Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(0.5f, 1.5f));
                        seasonColor = Color.Lerp(new Color(255, 183, 197), new Color(144, 238, 144), Main.rand.NextFloat());
                        break;
                    case 1: // Summer
                        vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(1f, 2f));
                        seasonColor = Color.Lerp(new Color(255, 140, 0), new Color(255, 215, 0), Main.rand.NextFloat());
                        break;
                    case 2: // Autumn
                        vel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(0.5f, 1.5f));
                        seasonColor = Color.Lerp(new Color(255, 100, 30), new Color(139, 69, 19), Main.rand.NextFloat());
                        break;
                    default: // Winter
                        vel = Main.rand.NextVector2Circular(1f, 1f);
                        seasonColor = Color.Lerp(new Color(173, 216, 230), new Color(0, 255, 255), Main.rand.NextFloat());
                        break;
                }
                
                CustomParticles.GenericGlow(pos, vel, seasonColor, 0.32f, 28, true);
                
                // ☁ESPARKLE accent for current season
                var sparkle = new SparkleParticle(pos, vel * 0.5f, seasonColor * 0.55f, 0.2f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // ☁EMUSICAL NOTATION - Cycling season notes! - VISIBLE SCALE 0.7f+
            if (!hideVisual && Main.rand.NextBool(14))
            {
                int currentSeason = (int)((Main.time / 10000) % 4);
                Color noteColor = currentSeason switch
                {
                    0 => new Color(255, 183, 197), // Spring
                    1 => new Color(255, 215, 0),   // Summer
                    2 => new Color(255, 100, 30),  // Autumn
                    _ => new Color(150, 220, 255)  // Winter
                };
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(28f, 28f);
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1.2f);
                ThemedParticles.MusicNote(notePos, noteVel, noteColor * 0.85f, 0.7f, 28);
            }
            
            // Rainbow cycling light
            float hue = (Main.GameUpdateCount * 0.005f) % 1f;
            Color lightColor = Main.hslToRgb(hue, 0.7f, 0.6f);
            Lighting.AddLight(player.Center, lightColor.ToVector3() * 0.5f);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color springGreen = new Color(144, 238, 144);
            Color summerOrange = new Color(255, 140, 0);
            Color autumnBrown = new Color(139, 69, 19);
            Color winterCyan = new Color(0, 255, 255);
            
            tooltips.Add(new TooltipLine(Mod, "Combo", "All Four Seasons Combined") { OverrideColor = Main.hslToRgb((Main.GameUpdateCount * 0.01f) % 1f, 0.8f, 0.7f) });
            tooltips.Add(new TooltipLine(Mod, "Damage", "+15% damage, +12% crit chance, +8% attack speed") { OverrideColor = summerOrange });
            tooltips.Add(new TooltipLine(Mod, "Defense", "+15 defense, +8% damage reduction") { OverrideColor = autumnBrown });
            tooltips.Add(new TooltipLine(Mod, "Regen", "+5 life regeneration") { OverrideColor = springGreen });
            tooltips.Add(new TooltipLine(Mod, "Elements", "Inflicts On Fire! and Frostburn, applies Thorns") { OverrideColor = Color.Lerp(summerOrange, winterCyan, 0.5f) });
            tooltips.Add(new TooltipLine(Mod, "LifeSteal", "20% chance to life steal on hit") { OverrideColor = autumnBrown });
            tooltips.Add(new TooltipLine(Mod, "Immunity", "Immune to Frozen, On Fire!, and Frostburn") { OverrideColor = winterCyan });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The eternal cycle turns, granting power over all seasons'") { OverrideColor = Main.hslToRgb((Main.GameUpdateCount * 0.01f + 0.5f) % 1f, 0.8f, 0.7f) });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<RelicOfTheEquinox>(), 1)
                .AddIngredient(ModContent.ItemType<SolsticeRing>(), 1)
                .AddIngredient(ItemID.LunarBar, 10)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    /// <summary>
    /// Vivaldi's Masterwork - The ultimate seasonal accessory
    /// Requires defeating all 4 seasonal bosses
    /// </summary>
    public class VivaldisMasterwork : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 36;
            Item.accessory = true;
            Item.rare = ItemRarityID.Purple;
            Item.value = Item.sellPrice(platinum: 1);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+20% damage, +15% critical strike chance"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+12% attack speed, +20 defense"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+12% damage reduction"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Increased life and mana regeneration"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Immunity to Frozen, On Fire, Frostburn, Chilled, and Poisoned"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Melee attacks inflict fire and frostburn, enhanced thorns damage"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The eternal cycle of Four Seasons united in perfect harmony'") 
            { 
                OverrideColor = new Color(200, 150, 255) 
            });
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // MASSIVE bonuses - end-game accessory
            player.GetDamage(DamageClass.Generic) += 0.2f; // 20% damage
            player.GetCritChance(DamageClass.Generic) += 15;
            player.GetAttackSpeed(DamageClass.Generic) += 0.12f; // 12% attack speed
            player.statDefense += 20;
            player.lifeRegen += 8;
            player.manaRegen += 4;
            player.endurance += 0.12f; // 12% DR
            player.moveSpeed += 0.15f;
            
            // All elemental effects
            player.magmaStone = true;
            player.frostBurn = true;
            player.thorns = 1.5f;
            
            // All immunities
            player.buffImmune[BuffID.Frozen] = true;
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            player.buffImmune[BuffID.Chilled] = true;
            player.buffImmune[BuffID.Poisoned] = true;
            
            // Enhanced life steal
            player.GetModPlayer<VivaldiPlayer>().vivaldiEquipped = true;
            
            // VIVALDI'S MASTERWORK - Grand symphonic visual
            if (!hideVisual)
            {
                // Four orbiting seasonal particles
                float baseAngle = Main.GameUpdateCount * 0.02f;
                Color[] seasonColors = new Color[]
                {
                    new Color(255, 183, 197), // Spring pink
                    new Color(255, 180, 50),  // Summer gold
                    new Color(200, 100, 30),  // Autumn orange
                    new Color(150, 220, 255)  // Winter blue
                };
                
                for (int i = 0; i < 4; i++)
                {
                    if (Main.rand.NextBool(8))
                    {
                        float angle = baseAngle + MathHelper.TwoPi * i / 4f;
                        Vector2 orbitPos = player.Center + angle.ToRotationVector2() * 50f;
                        CustomParticles.GenericFlare(orbitPos, seasonColors[i] * 0.85f, 0.3f, 18);
                        
                        // ☁ESPARKLE at orbit points
                        var sparkle = new SparkleParticle(orbitPos, angle.ToRotationVector2() * 0.5f, seasonColors[i] * 0.6f, 0.2f, 14);
                        MagnumParticleHandler.SpawnParticle(sparkle);
                    }
                }
                
                // Central musical glow with layered bloom
                if (Main.rand.NextBool(10))
                {
                    float hue = (Main.GameUpdateCount * 0.008f) % 1f;
                    Color musicColor = Main.hslToRgb(hue, 1f, 0.8f);
                    CustomParticles.GenericFlare(player.Center, Color.White * 0.5f, 0.4f, 24);
                    CustomParticles.GenericFlare(player.Center, musicColor * 0.65f, 0.38f, 22);
                }
                
                // ☁EMUSICAL NOTATION - Grand maestro notes! - VISIBLE SCALE 0.72f+
                if (Main.rand.NextBool(12))
                {
                    int seasonIndex = Main.rand.Next(4);
                    Vector2 notePos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                    Vector2 noteVel = new Vector2(Main.rand.NextFloat(-1f, 1f), -1.5f);
                    ThemedParticles.MusicNote(notePos, noteVel, seasonColors[seasonIndex] * 0.9f, 0.72f, 30);
                }
                
                // Grand light
                float lightHue = (Main.GameUpdateCount * 0.006f) % 1f;
                Color lightColor = Main.hslToRgb(lightHue, 0.8f, 0.65f);
                Lighting.AddLight(player.Center, lightColor.ToVector3() * 0.6f);
            }
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<CycleOfSeasons>(), 1)
                .AddIngredient(ModContent.ItemType<DormantSpringCore>(), 1)
                .AddIngredient(ModContent.ItemType<DormantSummerCore>(), 1)
                .AddIngredient(ModContent.ItemType<DormantAutumnCore>(), 1)
                .AddIngredient(ModContent.ItemType<DormantWinterCore>(), 1)
                .AddIngredient(ItemID.LunarBar, 20)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    public class VivaldiPlayer : ModPlayer
    {
        public bool vivaldiEquipped = false;
        
        public override void ResetEffects()
        {
            vivaldiEquipped = false;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (vivaldiEquipped && Main.rand.NextBool(3)) // 33% chance
            {
                int healAmount = (int)(damageDone * 0.06f); // 6% lifesteal
                healAmount = Math.Max(1, Math.Min(healAmount, 15)); // Cap at 15 HP
                Player.Heal(healAmount);
                
                // Grand lifesteal VFX with music note!
                float hue = Main.rand.NextFloat();
                Color vfxColor = Main.hslToRgb(hue, 0.8f, 0.7f);
                CustomParticles.GenericFlare(target.Center, Color.White * 0.6f, 0.55f, 20);
                CustomParticles.GenericFlare(target.Center, vfxColor, 0.52f, 18);
                
                // ☁ESPARKLE accent
                var sparkle = new SparkleParticle(target.Center, Main.rand.NextVector2Circular(2f, 2f), vfxColor * 0.6f, 0.22f, 14);
                MagnumParticleHandler.SpawnParticle(sparkle);
                
                // ☁EMUSICAL NOTATION - Lifesteal note! - VISIBLE SCALE 0.7f+
                ThemedParticles.MusicNote(target.Center, new Vector2(0, -2f), vfxColor * 0.9f, 0.7f, 25);
            }
        }
    }
}
