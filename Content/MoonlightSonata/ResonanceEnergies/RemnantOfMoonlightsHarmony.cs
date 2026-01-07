using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;

namespace MagnumOpus.Content.MoonlightSonata.ResonanceEnergies
{
    /// <summary>
    /// Remnant of Moonlight's Harmony - the raw ore clump dropped from mining Moonlit Resonance Ore.
    /// Can be smelted at a Moonlight Furnace to create Resonant Core of Moonlight Sonata.
    /// </summary>
    public class RemnantOfMoonlightsHarmony : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(silver: 25);
            Item.rare = ModContent.RarityType<MoonlightSonataRarity>();
        }

        public override void PostUpdate()
        {
            // Subtle glow when dropped
            Lighting.AddLight(Item.Center, 0.25f, 0.12f, 0.4f);
            
            if (Main.rand.NextBool(35))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.PurpleTorch, 0f, 0f, 150, default, 0.5f);
                dust.noGravity = true;
                dust.velocity *= 0.15f;
            }
        }
    }
}
