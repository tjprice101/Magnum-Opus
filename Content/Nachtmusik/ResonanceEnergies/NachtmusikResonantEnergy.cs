using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;

namespace MagnumOpus.Content.Nachtmusik.ResonanceEnergies
{
    /// <summary>
    /// Nachtmusik Resonant Energy - Refined energy material for Nachtmusik crafting.
    /// Crafted from Remnant of Nachtmusik's Harmony and drops from the Nachtmusik boss.
    /// </summary>
    public class NachtmusikResonantEnergy : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The serenade of the midnight hour'") { OverrideColor = new Color(100, 80, 160) });
        }

        public override void PostUpdate()
        {
            // Intense nocturnal celestial glow
            float pulse = 0.9f + (float)System.Math.Sin(Main.GameUpdateCount * 0.06f) * 0.15f;
            float goldPulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.1f + 1.5f) * 0.25f;
            
            // Deep purple core with bright golden star highlights
            Lighting.AddLight(Item.Center, 0.45f * pulse + goldPulse * 0.4f, 0.3f * pulse + goldPulse * 0.35f, 0.7f * pulse);
            
            // Deep purple cosmic particles
            if (Main.rand.NextBool(8))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.PurpleTorch, 0f, -0.8f, 100, default, 1.3f);
                dust.noGravity = true;
                dust.velocity *= 0.4f;
            }

            // Golden starlight particles
            if (Main.rand.NextBool(12))
            {
                Dust gold = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.GoldFlame, 0f, -0.5f, 0, default, 1.1f);
                gold.noGravity = true;
                gold.velocity *= 0.3f;
            }
            
            // Violet magic sparkles
            if (Main.rand.NextBool(18))
            {
                Dust sparkle = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Enchanted_Gold, Main.rand.NextFloat(-1f, 1f), -0.3f, 0, default, 0.8f);
                sparkle.noGravity = true;
            }
            
            // White star twinkle
            if (Main.rand.NextBool(30))
            {
                Dust star = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.WhiteTorch, 0f, 0f, 0, default, 0.6f);
                star.noGravity = true;
                star.velocity = Main.rand.NextVector2Circular(0.5f, 0.5f);
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            // Bright purple with golden glow
            return new Color(200, 180, 255, 180);
        }

        public override void AddRecipes()
        {
            // Craft from Remnants at a furnace
            CreateRecipe(1)
                .AddIngredient(ModContent.ItemType<RemnantOfNachtmusiksHarmony>(), 5)
                .AddTile(TileID.AdamantiteForge)
                .Register();
        }
    }
}
