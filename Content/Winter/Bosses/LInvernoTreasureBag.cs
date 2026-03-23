using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using ReLogic.Content;
using MagnumOpus.Content.Winter.Materials;
using MagnumOpus.Content.Winter.Weapons;

namespace MagnumOpus.Content.Winter.Bosses
{
    public class LInvernoTreasureBag : ModItem
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
            tooltips.Add(new TooltipLine(Mod, "Lore", "'In silence, the coldest truths are spoken'") { OverrideColor = new Color(173, 216, 230) });
        }

        public override bool CanRightClick() => true;

        public override void ModifyItemLoot(ItemLoot itemLoot)
        {
            // 5-8 Winter Resonant Energy
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<WinterResonantEnergy>(), 1, 5, 8));

            // 25-35 Shard of Stillness
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<ShardOfStillness>(), 1, 25, 35));

            // 2 random weapons (no duplicates)
            itemLoot.Add(new LInvernoTreasureBagWeaponRule());
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(220, 235, 255, 255);
        }
    }

    public class LInvernoTreasureBagWeaponRule : IItemDropRule
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
                ModContent.ItemType<GlacialExecutioner>(),
                ModContent.ItemType<FrostbiteRepeater>(),
                ModContent.ItemType<PermafrostCodex>(),
                ModContent.ItemType<FrozenHeart>()
            };
        }
    }
}
