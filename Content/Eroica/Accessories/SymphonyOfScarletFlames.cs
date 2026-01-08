using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Content.Eroica.Enemies;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;

namespace MagnumOpus.Content.Eroica.Accessories
{
    /// <summary>
    /// Symphony of Scarlet Flames - Ranger accessory.
    /// Triumphant Precision: Hitting same enemy 3 times marks them - 4th hit deals 300% damage with petal explosion.
    /// +15% ranged damage, +10% ranged crit.
    /// </summary>
    public class SymphonyOfScarletFlames : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 3);
            Item.rare = ModContent.RarityType<EroicaRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<EroicaAccessoryPlayer>();
            modPlayer.hasSymphonyOfScarletFlames = true;
            
            // Base stat boosts
            player.GetDamage(DamageClass.Ranged) += 0.15f; // +15% ranged damage
            player.GetCritChance(DamageClass.Ranged) += 10f; // +10% ranged crit
            
            // Ambient particles - scarlet flames and golden sparks
            if (!hideVisual && Main.rand.NextBool(6))
            {
                // Scarlet flame
                Vector2 offset = Main.rand.NextVector2Circular(25f, 25f);
                Dust flame = Dust.NewDustPerfect(player.Center + offset, DustID.CrimsonTorch, 
                    new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1.5f), 100, default, 1.2f);
                flame.noGravity = true;
            }
            
            if (!hideVisual && Main.rand.NextBool(10))
            {
                // Golden spark
                Vector2 offset = Main.rand.NextVector2Circular(30f, 30f);
                Dust spark = Dust.NewDustPerfect(player.Center + offset, DustID.GoldCoin, 
                    Main.rand.NextVector2Circular(1f, 1f), 0, default, 1f);
                spark.noGravity = true;
            }
            
            // Consecutive hit counter visual
            if (!hideVisual && modPlayer.consecutiveHits > 0)
            {
                float intensity = modPlayer.consecutiveHits / 3f;
                if (Main.rand.NextFloat() < intensity * 0.3f)
                {
                    Dust buildup = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(20f, 20f),
                        DustID.Torch, Main.rand.NextVector2Circular(2f, 2f), 50, new Color(255, 200, 100), 1f + intensity * 0.5f);
                    buildup.noGravity = true;
                }
            }
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "StatBoosts", "+15% ranged damage, +10% ranged critical strike chance")
            {
                OverrideColor = new Color(255, 200, 100)
            });
            
            tooltips.Add(new TooltipLine(Mod, "PrecisionHeader", "Triumphant Precision:")
            {
                OverrideColor = new Color(255, 100, 100)
            });
            
            tooltips.Add(new TooltipLine(Mod, "MarkEffect", "Hitting the same enemy 3 times marks them as 'Heroic Target'")
            {
                OverrideColor = new Color(255, 180, 100)
            });
            
            tooltips.Add(new TooltipLine(Mod, "ExplosionEffect", "The 4th hit deals 300% damage and creates a petal explosion")
            {
                OverrideColor = new Color(255, 150, 150)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Flavor", "'Each note builds to the symphony's triumphant crescendo'")
            {
                OverrideColor = new Color(180, 80, 80)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<EroicasResonantEnergy>(), 5)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfEroica>(), 5)
                .AddIngredient(ModContent.ItemType<ShardOfTriumphsTempo>(), 5)
                .AddIngredient(ItemID.SoulofSight, 5)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }
}
