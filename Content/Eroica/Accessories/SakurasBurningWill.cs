using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Content.Eroica.Enemies;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Eroica.Accessories
{
    /// <summary>
    /// Sakura's Burning Will - Summoner accessory.
    /// Banner of Eroica: Every 12 seconds, summon a temporary "Heroic Spirit" (5 seconds, high damage, no slot cost).
    /// Standing within 15 tiles of minions: they get +20% damage, you get +8 defense.
    /// </summary>
    public class SakurasBurningWill : ModItem
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
            modPlayer.hasSakurasBurningWill = true;
            
            // Enhanced ambient particles using ThemedParticles system
            if (!hideVisual)
            {
                // Sakura petals swirling around player
                ThemedParticles.SakuraPetals(player.Center, 8, 40f);
                
                // Eroica aura embers
                ThemedParticles.EroicaAura(player.Center, 30f);
            }
            
            // Spirit summoning timer visual with enhanced effects
            if (!hideVisual && modPlayer.heroicSpiritTimer > 600) // Last 2 seconds before summon
            {
                float progress = (modPlayer.heroicSpiritTimer - 600f) / 120f;
                
                // Pulsing halo effect building up to summon
                if (Main.GameUpdateCount % (int)(20 - progress * 15) == 0)
                {
                    CustomParticles.EroicaHalo(player.Center, 0.3f + progress * 0.4f);
                }
                
                // Building energy using sparkles from particle system
                if (Main.rand.NextFloat() < progress * 0.5f)
                {
                    ThemedParticles.EroicaSparkles(player.Center, 3, 25f);
                }
                
                // Original gold energy converging
                if (Main.rand.NextFloat() < progress)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 pos = player.Center + new Vector2((float)System.Math.Cos(angle), (float)System.Math.Sin(angle)) * 50f * (1f - progress);
                    Dust energy = Dust.NewDustPerfect(pos, DustID.GoldCoin, 
                        (player.Center - pos).SafeNormalize(Vector2.Zero) * 3f, 0, default, 1.2f);
                    energy.noGravity = true;
                }
            }
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "BannerHeader", "Banner of Eroica:")
            {
                OverrideColor = new Color(255, 150, 180)
            });
            
            tooltips.Add(new TooltipLine(Mod, "SpiritSummon", "Every 12 seconds, summon a Heroic Spirit (5s duration)")
            {
                OverrideColor = new Color(255, 200, 150)
            });
            
            tooltips.Add(new TooltipLine(Mod, "SpiritInfo", "The spirit fights independently and uses no minion slots")
            {
                OverrideColor = new Color(255, 180, 130)
            });
            
            tooltips.Add(new TooltipLine(Mod, "ProximityBonus", "+20% minion damage")
            {
                OverrideColor = new Color(200, 255, 150)
            });
            
            tooltips.Add(new TooltipLine(Mod, "DefenseBonus", "+8 defense when within 15 tiles of your minions")
            {
                OverrideColor = new Color(150, 200, 255)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Flavor", "'The sakura's will burns eternal in those who fight alongside it'")
            {
                OverrideColor = new Color(180, 100, 120)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<EroicasResonantEnergy>(), 5)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfEroica>(), 5)
                .AddIngredient(ModContent.ItemType<ShardOfTriumphsTempo>(), 5)
                .AddIngredient(ItemID.SoulofSight, 5)
                .AddIngredient(ItemID.SoulofMight, 8)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }
}
