using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;

namespace MagnumOpus.Content.Nachtmusik.ResonanceEnergies
{
    /// <summary>
    /// Nachtmusik Resonant Core - Higher tier crafting material.
    /// Used in creating Harmonic Core and advanced Nachtmusik items.
    /// </summary>
    public class NachtmusikResonantCore : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 10;
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(gold: 8);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A heart that beats only under starlight'") { OverrideColor = new Color(100, 80, 160) });
        }

        public override void PostUpdate()
        {
            // Powerful nocturnal aura
            float pulse = 0.9f + (float)System.Math.Sin(Main.GameUpdateCount * 0.05f) * 0.2f;
            float goldPulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.09f + 2f) * 0.3f;
            
            // Strong purple-gold light
            Lighting.AddLight(Item.Center, 0.55f * pulse + goldPulse * 0.45f, 0.35f * pulse + goldPulse * 0.4f, 0.8f * pulse);
            
            // Deep purple cosmic particles
            if (Main.rand.NextBool(6))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.PurpleTorch, 0f, -1f, 80, default, 1.4f);
                dust.noGravity = true;
                dust.velocity *= 0.5f;
            }

            // Bright golden starlight
            if (Main.rand.NextBool(10))
            {
                Dust gold = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.GoldFlame, 0f, -0.7f, 0, default, 1.2f);
                gold.noGravity = true;
                gold.velocity *= 0.35f;
            }
            
            // Violet magical aura
            if (Main.rand.NextBool(15))
            {
                Dust violet = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Enchanted_Gold, Main.rand.NextFloat(-1.2f, 1.2f), -0.4f, 0, default, 0.9f);
                violet.noGravity = true;
            }
            
            // Constellation star twinkle
            if (Main.rand.NextBool(22))
            {
                Dust star = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.WhiteTorch, 0f, 0f, 0, default, 0.7f);
                star.noGravity = true;
                star.velocity = Main.rand.NextVector2Circular(0.6f, 0.6f);
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            // Vibrant purple with golden highlights
            return new Color(220, 190, 255, 170);
        }

        public override void AddRecipes()
        {
            // Craft from Resonant Energy
            CreateRecipe(1)
                .AddIngredient(ModContent.ItemType<NachtmusikResonantEnergy>(), 5)
                .AddIngredient(ModContent.ItemType<RemnantOfNachtmusiksHarmony>(), 10)
                .AddTile(TileID.AdamantiteForge)
                .Register();
        }
    }
}
