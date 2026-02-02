using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.DiesIrae.ResonanceEnergies;
using MagnumOpus.Content.DiesIrae.HarmonicCores;
using MagnumOpus.Content.DiesIrae.ResonantWeapons;
using MagnumOpus.Content.DiesIrae.Accessories;

namespace MagnumOpus.Content.DiesIrae.Bosses
{
    /// <summary>
    /// Treasure Bag for Dies Irae, Herald of Judgment.
    /// Contains boss loot for Expert/Master mode.
    /// </summary>
    public class DiesIraeTreasureBag : ModItem
    {
        private static readonly Color BloodRed = new Color(139, 0, 0);

        public override void SetStaticDefaults()
        {
            ItemID.Sets.BossBag[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true;
            Item.rare = ItemRarityID.Expert;
            Item.expert = true;
        }

        public override bool CanRightClick()
        {
            return true;
        }

        public override void ModifyItemLoot(ItemLoot itemLoot)
        {
            // Materials
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<ResonantCoreOfDiesIrae>(), 1, 30, 40));
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<DiesIraeResonantEnergy>(), 1, 20, 30));
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<HarmonicCoreOfDiesIrae>(), 1, 4, 6));
            
            // One weapon guaranteed
            int[] weapons = new int[]
            {
                ModContent.ItemType<WrathsCleaver>(),
                ModContent.ItemType<ChainOfJudgment>(),
                ModContent.ItemType<ExecutionersVerdict>(),
                ModContent.ItemType<SinCollector>(),
                ModContent.ItemType<DamnationsCannon>(),
                ModContent.ItemType<ArbitersSentence>(),
                ModContent.ItemType<StaffOfFinalJudgement>(),
                ModContent.ItemType<EclipseOfWrath>(),
                ModContent.ItemType<GrimoireOfCondemnation>()
            };
            itemLoot.Add(ItemDropRule.OneFromOptions(1, weapons));
            
            // Expert exclusive accessory (one random)
            int[] accessories = new int[]
            {
                ModContent.ItemType<EmberOfTheCondemned>(),
                ModContent.ItemType<SealOfDamnation>(),
                ModContent.ItemType<ChainOfFinalJudgment>(),
                ModContent.ItemType<RequiemsShackle>()
            };
            itemLoot.Add(ItemDropRule.OneFromOptions(1, accessories));
            
            // Money
            itemLoot.Add(ItemDropRule.Common(ItemID.GoldCoin, 1, 15, 25));
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.Lerp(lightColor, Color.White, 0.4f);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "BagInfo", "Right click to open"));
        }
    }
}
