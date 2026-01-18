using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.EnigmaVariations.ResonanceEnergies
{
    /// <summary>
    /// Remnant of Mysteries - Theme-specific crafting material dropped by Enigma mini-bosses.
    /// Equivalent to Eroica's ShardOfTriumphsTempo.
    /// Used for crafting Enigma-themed weapons and accessories.
    /// </summary>
    public class RemnantOfMysteries : ModItem
    {
        // Enigma theme colors
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
            ItemID.Sets.ItemNoGravity[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(gold: 8);
            Item.rare = ItemRarityID.Lime;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Material", "Enigma crafting material")
            {
                OverrideColor = EnigmaGreen
            });
            
            tooltips.Add(new TooltipLine(Mod, "Flavor", "'A fragment of questions left unanswered'")
            {
                OverrideColor = EnigmaPurple
            });
        }

        public override void PostUpdate()
        {
            // Mysterious pulsing glow
            float pulse = 0.5f + (float)System.Math.Sin(Main.GameUpdateCount * 0.08f) * 0.2f;
            Lighting.AddLight(Item.Center, EnigmaGreen.ToVector3() * pulse * 0.5f);
            
            // Purple-green alternating dust particles
            if (Main.rand.NextBool(12))
            {
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.GreenTorch;
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, dustType, 0f, -0.5f, 100, default, 1.1f);
                dust.noGravity = true;
                dust.velocity *= 0.4f;
            }

            // Occasional magic spark
            if (Main.rand.NextBool(25))
            {
                Dust sparkle = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.TerraBlade, 0f, 0f, 0, default, 0.8f);
                sparkle.noGravity = true;
                sparkle.velocity *= 0.2f;
            }
            
            // Glyph-like magic dust
            if (Main.rand.NextBool(20))
            {
                Dust glyph = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Enchanted_Gold, 
                    Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-0.5f, -0.2f), 150, default, 0.7f);
                glyph.noGravity = true;
            }
        }
    }
}
