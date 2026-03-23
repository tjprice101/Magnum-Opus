using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using ReLogic.Content;
using MagnumOpus.Content.Summer.Materials;
using MagnumOpus.Content.Summer.Weapons;

namespace MagnumOpus.Content.Summer.Bosses
{
    public class LEstateTreasureBag : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.BossBag[Type] = true;
            ItemID.Sets.PreHardmodeLikeBossBag[Type] = false;
            Item.ResearchUnlockCount = 3;
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

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The sun's fury lingers even after it sets'") { OverrideColor = new Color(255, 140, 0) });
        }

        public override bool CanRightClick() => true;

        public override void ModifyItemLoot(ItemLoot itemLoot)
        {
            // 5-8 Summer Resonant Energy
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<SummerResonantEnergy>(), 1, 5, 8));

            // 25-35 Ember of Intensity
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<EmberOfIntensity>(), 1, 25, 35));

            // 2 random weapons (no duplicates)
            itemLoot.Add(new LEstateTreasureBagWeaponRule());
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 220, 180, 255);
        }
    }

    public class LEstateTreasureBagWeaponRule : IItemDropRule
    {
        public List<IItemDropRuleChainAttempt> ChainedRules => new List<IItemDropRuleChainAttempt>();

        public bool CanDrop(DropAttemptInfo info) => true;

        public void ReportDroprates(List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo)
        {
            int[] possibleDrops = GetPossibleDrops();
            float individualChance = 2f / possibleDrops.Length;

            foreach (int itemType in possibleDrops)
            {
                drops.Add(new DropRateInfo(itemType, 1, 1, individualChance, ratesInfo.conditions));
            }
        }

        public ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info)
        {
            int[] possibleDrops = GetPossibleDrops();

            List<int> shuffled = new List<int>(possibleDrops);
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int j = Main.rand.Next(i + 1);
                int temp = shuffled[i];
                shuffled[i] = shuffled[j];
                shuffled[j] = temp;
            }

            for (int i = 0; i < 2 && i < shuffled.Count; i++)
            {
                CommonCode.DropItem(info, shuffled[i], 1);
            }

            return new ItemDropAttemptResult
            {
                State = ItemDropAttemptResultState.Success
            };
        }

        private int[] GetPossibleDrops()
        {
            return new int[]
            {
                ModContent.ItemType<ZenithCleaver>(),
                ModContent.ItemType<SolarScorcher>(),
                ModContent.ItemType<SolsticeTome>(),
                ModContent.ItemType<SolarCrest>()
            };
        }
    }
}
