using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using MagnumOpus.Content.Autumn.Materials;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Autumn.Accessories
{
    /// <summary>
    /// Reaper's Charm - Life Steal Autumn accessory
    /// Post-Plantera tier, provides life steal on hits
    /// </summary>
    public class ReapersCharm : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.accessory = true;
            Item.rare = ItemRarityID.Lime;
            Item.value = Item.sellPrice(gold: 7);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Generic) += 0.07f;
            
            // Life steal handled via OnHit
            player.GetModPlayer<ReapersCharmPlayer>().reapersCharmEquipped = true;
            
            // Decay aura
            if (!hideVisual && Main.rand.NextBool(10))
            {
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(0.5f, 1.5f));
                Color decayColor = Color.Lerp(new Color(255, 100, 30), new Color(139, 69, 19), Main.rand.NextFloat());
                CustomParticles.GenericGlow(pos, vel, decayColor, 0.24f, 26, true);
            }
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<AutumnResonantEnergy>(), 2)
                .AddIngredient(ModContent.ItemType<DecayEssence>(), 4)
                .AddIngredient(ModContent.ItemType<LeafOfEnding>(), 12)
                .AddIngredient(ItemID.Megashark, 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    
    public class ReapersCharmPlayer : ModPlayer
    {
        public bool reapersCharmEquipped = false;
        
        public override void ResetEffects()
        {
            reapersCharmEquipped = false;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (reapersCharmEquipped && Main.rand.NextBool(5)) // 20% chance
            {
                int healAmount = (int)(damageDone * 0.04f); // 4% lifesteal
                healAmount = System.Math.Max(1, System.Math.Min(healAmount, 8)); // Cap at 8 HP
                Player.Heal(healAmount);
                
                // Lifesteal VFX
                CustomParticles.GenericFlare(target.Center, new Color(180, 80, 40), 0.4f, 15);
            }
        }
    }
    
    /// <summary>
    /// Twilight Ring - Crit-based Autumn accessory
    /// Post-Plantera tier, boosts crit chance and crit damage
    /// </summary>
    public class TwilightRing : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.accessory = true;
            Item.rare = ItemRarityID.Lime;
            Item.value = Item.sellPrice(gold: 7);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetCritChance(DamageClass.Generic) += 10;
            
            // Twilight glow effect based on time of day
            float twilightMult = 1f;
            if (!Main.dayTime && Main.time < 16200) // Early night (sunset)
                twilightMult = 1.3f;
            else if (Main.dayTime && Main.time > 38000) // Late day (approaching sunset)
                twilightMult = 1.2f;
            
            player.GetDamage(DamageClass.Generic) += 0.05f * twilightMult;
            
            // Twilight ring effect
            if (!hideVisual && Main.rand.NextBool(12))
            {
                float angle = Main.GameUpdateCount * 0.03f + Main.rand.NextFloat();
                Vector2 pos = player.Center + angle.ToRotationVector2() * 25f;
                Color twilightColor = Color.Lerp(new Color(255, 100, 30), new Color(128, 64, 96), Main.rand.NextFloat());
                CustomParticles.GenericFlare(pos, twilightColor * 0.7f, 0.22f, 18);
            }
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<AutumnResonantEnergy>(), 2)
                .AddIngredient(ModContent.ItemType<DecayEssence>(), 4)
                .AddIngredient(ModContent.ItemType<LeafOfEnding>(), 12)
                .AddIngredient(ItemID.MoonStone, 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    
    /// <summary>
    /// Harvest Mantle - Tank Autumn accessory
    /// Post-Plantera tier, provides significant defense and thorns
    /// </summary>
    public class HarvestMantle : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.rare = ItemRarityID.Yellow;
            Item.value = Item.sellPrice(gold: 8);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.statDefense += 12;
            player.endurance += 0.08f; // 8% damage reduction
            player.thorns = 1f; // Thorns damage
            
            // Harvest shield effect
            if (!hideVisual && Main.rand.NextBool(10))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 pos = player.Center + angle.ToRotationVector2() * 35f;
                Vector2 vel = angle.ToRotationVector2() * 0.5f;
                Color harvestColor = Color.Lerp(new Color(139, 69, 19), new Color(218, 165, 32), Main.rand.NextFloat());
                CustomParticles.GenericGlow(pos, vel, harvestColor, 0.26f, 24, true);
            }
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<AutumnResonantEnergy>(), 3)
                .AddIngredient(ModContent.ItemType<DecayEssence>(), 6)
                .AddIngredient(ModContent.ItemType<LeafOfEnding>(), 15)
                .AddIngredient(ItemID.PaladinsShield, 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
