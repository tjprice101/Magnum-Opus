using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Materials.Foundation
{
    /// <summary>
    /// Dull Resonator - Rare find in jungle chests.
    /// A circular resonating disc awaiting activation.
    /// </summary>
    public class DullResonator : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 5;
        }

        public override void SetDefaults()
        {
            Item.width = 14;
            Item.height = 14;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(silver: 30);
            Item.rare = ItemRarityID.Green;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Awaiting the vibration that will awaken its song'") { OverrideColor = new Color(100, 200, 100) });
        }

        public override void PostUpdate()
        {
            // Dormant resonance waiting to awaken
            float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.03f) * 0.1f + 0.2f;
            Lighting.AddLight(Item.Center, 0.15f * pulse, 0.25f * pulse, 0.15f * pulse);

            if (Main.rand.NextBool(40))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.JungleSpore, 0f, 0f, 130, default, 0.5f);
                dust.noGravity = true;
                dust.velocity *= 0.15f;
            }
        }

        public override void AddRecipes()
        {
            // Alternative recipe for chest-only item
            CreateRecipe()
                .AddIngredient(ItemID.JungleSpores, 8)
                .AddIngredient(ItemID.RichMahogany, 15)
                .AddIngredient<MinorMusicNote>(3)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
