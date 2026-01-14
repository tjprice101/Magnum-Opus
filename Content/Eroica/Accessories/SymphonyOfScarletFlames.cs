using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Content.Eroica.Enemies;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

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
            
            // === UnifiedVFX EROICA AMBIENT EFFECTS ===
            if (!hideVisual)
            {
                // Eroica themed aura with UnifiedVFX
                UnifiedVFX.Eroica.Aura(player.Center, 35f, 0.3f);
                
                // Orbiting gradient flares - signature geometric look
                if (Main.rand.NextBool(8))
                {
                    float baseAngle = Main.GameUpdateCount * 0.025f;
                    for (int i = 0; i < 3; i++)
                    {
                        float angle = baseAngle + MathHelper.TwoPi * i / 3f;
                        float radius = 30f + (float)System.Math.Sin(Main.GameUpdateCount * 0.05f + i * 0.7f) * 8f;
                        Vector2 flarePos = player.Center + new Vector2((float)System.Math.Cos(angle), (float)System.Math.Sin(angle)) * radius;
                        float progress = (float)i / 3f;
                        Color fractalColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, progress);
                        CustomParticles.GenericFlare(flarePos, fractalColor, 0.32f, 16);
                    }
                }
                
                // Sakura petal drift
                if (Main.rand.NextBool(15))
                    ThemedParticles.SakuraPetals(player.Center, 1, 35f);
                
                // Enhanced sparkles
                if (Main.rand.NextBool(10))
                    ThemedParticles.EroicaSparkles(player.Center, 2, 25f);
            }
            
            // Consecutive hit counter visual - gradient buildup effect
            if (!hideVisual && modPlayer.consecutiveHits > 0)
            {
                float intensity = modPlayer.consecutiveHits / 3f;
                if (Main.rand.NextFloat() < intensity * 0.4f)
                {
                    // Gradient colored buildup: Scarlet → Gold based on stacks
                    Color buildupColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, intensity);
                    CustomParticles.GenericFlare(player.Center + Main.rand.NextVector2Circular(22f, 22f), 
                        buildupColor, 0.3f + intensity * 0.3f, 14);
                    
                    // Halo ring when near max
                    if (intensity > 0.8f && Main.rand.NextBool(5))
                        CustomParticles.HaloRing(player.Center, UnifiedVFX.Eroica.Gold * 0.6f, 0.3f, 15);
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
                .AddIngredient(ItemID.SoulofMight, 10)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }
}
