using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using ReLogic.Content;
using MagnumOpus.Content.Spring.Materials;
using MagnumOpus.Content.Spring.Weapons;

namespace MagnumOpus.Content.Spring.Bosses
{
    public class PrimaveraTreasureBag : ModItem
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
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The bloom remembers what the frost forgot'") { OverrideColor = new Color(255, 183, 197) });
        }

        public override bool CanRightClick() => true;

        public override void ModifyItemLoot(ItemLoot itemLoot)
        {
            // 5-8 Spring Resonant Energy
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<SpringResonantEnergy>(), 1, 5, 8));

            // 20-30 Petal of Rebirth
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<PetalOfRebirth>(), 1, 20, 30));

            // 2 random weapons (no duplicates)
            itemLoot.Add(new PrimaveraTreasureBagWeaponRule());
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 230, 240, 255);
        }
    }

    public class PrimaveraTreasureBagWeaponRule : IItemDropRule
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
                ModContent.ItemType<BlossomsEdge>(),
                ModContent.ItemType<PetalStormBow>(),
                ModContent.ItemType<VernalScepter>(),
                ModContent.ItemType<PrimaverasBloom>()
            };
        }
    }
}
