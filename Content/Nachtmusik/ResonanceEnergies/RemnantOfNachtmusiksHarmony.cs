using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;

namespace MagnumOpus.Content.Nachtmusik.ResonanceEnergies
{
    /// <summary>
    /// Remnant of Nachtmusik's Harmony - Raw material from Nachtmusik ore.
    /// Can be refined into Resonant Cores at a crafting station.
    /// Also drops from underground enemies (5% chance for 1-3) after Fate is defeated.
    /// </summary>
    public class RemnantOfNachtmusiksHarmony : ModItem
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
            Item.value = Item.sellPrice(silver: 95);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
        }

        public override void PostUpdate()
        {
            // Deep purple glow with golden star shimmer matching Nachtmusik's celestial night theme
            float pulse = 0.85f + (float)System.Math.Sin(Main.GameUpdateCount * 0.07f) * 0.15f;
            float goldPulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.12f) * 0.2f;
            
            // Purple base with golden accent
            Lighting.AddLight(Item.Center, 0.35f * pulse + goldPulse * 0.3f, 0.2f * pulse + goldPulse * 0.25f, 0.55f * pulse);
            
            // Purple cosmic particles
            if (Main.rand.NextBool(10))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.PurpleTorch, 0f, -0.6f, 150, default, 1.1f);
                dust.noGravity = true;
                dust.velocity *= 0.35f;
            }

            // Golden star shimmer
            if (Main.rand.NextBool(20))
            {
                Dust sparkle = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.GoldFlame, 0f, 0f, 0, default, 0.9f);
                sparkle.noGravity = true;
                sparkle.velocity = Main.rand.NextVector2Circular(0.8f, 0.8f);
            }
            
            // Occasional violet/star white sparkle
            if (Main.rand.NextBool(35))
            {
                Dust star = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Enchanted_Gold, Main.rand.NextFloat(-0.5f, 0.5f), -0.4f, 0, default, 0.7f);
                star.noGravity = true;
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            // Deep purple with golden tint
            return new Color(180, 150, 220, 200);
        }
    }
}
