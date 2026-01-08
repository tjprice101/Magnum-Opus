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
            
            // Ambient particles - sakura petals and embers
            if (!hideVisual && Main.rand.NextBool(6))
            {
                // Sakura pink petal
                Vector2 offset = new Vector2(Main.rand.NextFloat(-30f, 30f), Main.rand.NextFloat(-20f, 20f));
                Dust petal = Dust.NewDustPerfect(player.Center + offset, DustID.PinkTorch, 
                    new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-2f, 0f)), 100, default, 1.2f);
                petal.noGravity = true;
            }
            
            if (!hideVisual && Main.rand.NextBool(8))
            {
                // Scarlet ember
                Vector2 offset = Main.rand.NextVector2Circular(25f, 25f);
                Dust ember = Dust.NewDustPerfect(player.Center + offset, DustID.CrimsonTorch, 
                    new Vector2(0, -1.5f), 80, default, 1f);
                ember.noGravity = true;
            }
            
            // Spirit summoning timer visual
            if (!hideVisual && modPlayer.heroicSpiritTimer > 600) // Last 2 seconds before summon
            {
                float progress = (modPlayer.heroicSpiritTimer - 600f) / 120f;
                if (Main.rand.NextFloat() < progress)
                {
                    // Building energy particles
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
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }
}
