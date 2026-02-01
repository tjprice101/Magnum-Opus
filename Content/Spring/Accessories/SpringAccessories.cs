using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using MagnumOpus.Content.Spring.Materials;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems;
using static MagnumOpus.Common.Systems.ThemedParticles;

namespace MagnumOpus.Content.Spring.Accessories
{
    /// <summary>
    /// Petal Shield - Defensive Spring accessory
    /// Post-WoF tier, provides defense and damage reduction when hit
    /// </summary>
    public class PetalShield : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.accessory = true;
            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(gold: 3);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.statDefense += 8;
            player.endurance += 0.06f; // 6% damage reduction
            
            // Petal visual effect
            if (!hideVisual && Main.rand.NextBool(12))
            {
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(0.5f, 1.5f));
                Color petalColor = Main.rand.NextBool() ? new Color(255, 183, 197) : new Color(255, 218, 233);
                CustomParticles.GenericGlow(pos, vel, petalColor, 0.25f, 28, true);
            }
            
            // Floating spring melody notes
            if (!hideVisual && Main.rand.NextBool(18))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                Vector2 noteVel = new Vector2(0, -Main.rand.NextFloat(0.3f, 0.6f));
                Color noteColor = Color.Lerp(new Color(255, 183, 197), new Color(144, 238, 144), Main.rand.NextFloat()) * 0.55f;
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.68f, 40);
            }
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color springPink = new Color(255, 183, 197);
            Color springGreen = new Color(144, 238, 144);
            
            tooltips.Add(new TooltipLine(Mod, "Season", "Spring Accessory") { OverrideColor = springPink });
            tooltips.Add(new TooltipLine(Mod, "Defense", "+8 defense") { OverrideColor = springGreen });
            tooltips.Add(new TooltipLine(Mod, "DR", "+6% damage reduction") { OverrideColor = springGreen });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Petals form an impenetrable shield of renewal'") { OverrideColor = Color.Lerp(springPink, Color.White, 0.3f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SpringResonantEnergy>(), 2)
                .AddIngredient(ModContent.ItemType<BlossomEssence>(), 4)
                .AddIngredient(ModContent.ItemType<PetalOfRebirth>(), 10)
                .AddIngredient(ItemID.CobaltShield, 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    
    /// <summary>
    /// Growth Band - Regeneration Spring accessory
    /// Post-WoF tier, provides life regen and mana regen
    /// </summary>
    public class GrowthBand : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.accessory = true;
            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(gold: 3);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.lifeRegen += 3;
            player.manaRegen += 2;
            player.GetDamage(DamageClass.Generic) += 0.04f; // 4% damage boost
            
            // Growing glow effect
            if (!hideVisual && Main.rand.NextBool(16))
            {
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(25f, 25f);
                Color glowColor = Color.Lerp(new Color(144, 238, 144), new Color(255, 218, 233), Main.rand.NextFloat());
                CustomParticles.GenericFlare(pos, glowColor * 0.6f, 0.2f, 18);
            }
            
            // Floating spring melody notes
            if (!hideVisual && Main.rand.NextBool(20))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(32f, 32f);
                Vector2 noteVel = new Vector2(0, -Main.rand.NextFloat(0.3f, 0.7f));
                Color noteColor = new Color(144, 238, 144) * 0.55f;
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.68f, 38);
            }
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color springPink = new Color(255, 183, 197);
            Color springGreen = new Color(144, 238, 144);
            
            tooltips.Add(new TooltipLine(Mod, "Season", "Spring Accessory") { OverrideColor = springPink });
            tooltips.Add(new TooltipLine(Mod, "Regen", "+3 life regeneration") { OverrideColor = springGreen });
            tooltips.Add(new TooltipLine(Mod, "ManaRegen", "+2 mana regeneration") { OverrideColor = springGreen });
            tooltips.Add(new TooltipLine(Mod, "Damage", "+4% damage") { OverrideColor = springGreen });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Life flows through the wearer like sap through a tree'") { OverrideColor = Color.Lerp(springGreen, Color.White, 0.3f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SpringResonantEnergy>(), 2)
                .AddIngredient(ModContent.ItemType<BlossomEssence>(), 4)
                .AddIngredient(ModContent.ItemType<PetalOfRebirth>(), 10)
                .AddIngredient(ItemID.BandofRegeneration, 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    
    /// <summary>
    /// Bloom Crest - Offensive Spring accessory
    /// Post-WoF tier, boosts damage and crit when moving
    /// </summary>
    public class BloomCrest : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.accessory = true;
            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(gold: 3, silver: 50);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Generic) += 0.06f; // 6% base damage
            
            // Bonus when moving
            if (player.velocity.Length() > 3f)
            {
                player.GetDamage(DamageClass.Generic) += 0.04f; // +4% when moving fast
                player.GetCritChance(DamageClass.Generic) += 5;
                
                // Bloom trail when moving
                if (!hideVisual && Main.rand.NextBool(4))
                {
                    Vector2 pos = player.Center - player.velocity * Main.rand.NextFloat(0.2f, 0.5f);
                    Color bloomColor = Main.rand.NextBool() ? new Color(255, 183, 197) : new Color(144, 238, 144);
                    CustomParticles.GenericFlare(pos, bloomColor * 0.7f, 0.22f, 20);
                }
                
                // Music notes while moving fast
                if (!hideVisual && Main.rand.NextBool(8))
                {
                    Vector2 notePos = player.Center - player.velocity * Main.rand.NextFloat(0.3f, 0.6f);
                    Vector2 noteVel = -player.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f);
                    Color noteColor = Color.Lerp(new Color(255, 183, 197), new Color(144, 238, 144), Main.rand.NextFloat()) * 0.7f;
                    ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.68f, 30);
                    
                    // Sparkle companion
                    var sparkle = new SparkleParticle(notePos, noteVel * 0.5f, new Color(255, 255, 255) * 0.4f, 0.18f, 16);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }
            
            Lighting.AddLight(player.Center, new Color(255, 200, 220).ToVector3() * 0.3f);
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color springPink = new Color(255, 183, 197);
            Color springGreen = new Color(144, 238, 144);
            
            tooltips.Add(new TooltipLine(Mod, "Season", "Spring Accessory") { OverrideColor = springPink });
            tooltips.Add(new TooltipLine(Mod, "Damage", "+6% damage (base)") { OverrideColor = springGreen });
            tooltips.Add(new TooltipLine(Mod, "Moving", "While moving: +4% additional damage, +5% crit chance") { OverrideColor = springPink });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The crest blooms brighter with each step forward'") { OverrideColor = Color.Lerp(springPink, Color.White, 0.3f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SpringResonantEnergy>(), 2)
                .AddIngredient(ModContent.ItemType<BlossomEssence>(), 4)
                .AddIngredient(ModContent.ItemType<PetalOfRebirth>(), 12)
                .AddIngredient(ItemID.WarriorEmblem, 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
