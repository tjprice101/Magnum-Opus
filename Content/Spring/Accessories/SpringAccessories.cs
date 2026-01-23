using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using MagnumOpus.Content.Spring.Materials;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems;

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
            }
            
            Lighting.AddLight(player.Center, new Color(255, 200, 220).ToVector3() * 0.3f);
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
