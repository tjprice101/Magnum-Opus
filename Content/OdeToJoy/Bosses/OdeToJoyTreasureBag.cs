using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using ReLogic.Content;
using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;
using MagnumOpus.Content.OdeToJoy.HarmonicCores;
using MagnumOpus.Content.OdeToJoy.Accessories;
using MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning;
using MagnumOpus.Content.OdeToJoy.Weapons.TheGardenersFury;
using MagnumOpus.Content.OdeToJoy.Weapons.RoseThornChainsaw;
using MagnumOpus.Content.OdeToJoy.Weapons.ThornSprayRepeater;
using MagnumOpus.Content.OdeToJoy.Weapons.ThePollinator;
using MagnumOpus.Content.OdeToJoy.Weapons.PetalStormCannon;
using MagnumOpus.Content.OdeToJoy.Weapons.AnthemOfGlory;
using MagnumOpus.Content.OdeToJoy.Weapons.HymnOfTheVictorious;
using MagnumOpus.Content.OdeToJoy.Weapons.ElysianVerdict;
using MagnumOpus.Content.OdeToJoy.Weapons.TriumphantChorus;
using MagnumOpus.Content.OdeToJoy.Weapons.TheStandingOvation;
using MagnumOpus.Content.OdeToJoy.Weapons.FountainOfJoyousHarmony;

namespace MagnumOpus.Content.OdeToJoy.Bosses
{
    /// <summary>
    /// Treasure Bag for Ode to Joy, Chromatic Rose Conductor (Tier 9).
    /// Contains boss loot for Expert/Master mode.
    /// </summary>
    public class OdeToJoyTreasureBag : ModItem
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
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A symphony of blooming triumph'")
            {
                OverrideColor = new Color(255, 200, 50) // Warm Gold
            });
        }

        public override bool CanRightClick() => true;

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            return false;
        }

        public override void ModifyItemLoot(ItemLoot itemLoot)
        {
            // Materials
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<ResonantCoreOfOdeToJoy>(), 1, 30, 40));
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<OdeToJoyResonantEnergy>(), 1, 20, 30));
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<HarmonicCoreOfOdeToJoy>(), 1, 4, 6));
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<RemnantOfOdeToJoysBloom>(), 1, 30, 35));

            // 3 random weapons (no duplicates) — custom drop rule
            itemLoot.Add(new OdeToJoyTreasureBagWeaponRule());

            // Accessory (one random)
            itemLoot.Add(ItemDropRule.OneFromOptions(1,
                ModContent.ItemType<TheFloweringCoda>(),
                ModContent.ItemType<TheVerdantRefrain>(),
                ModContent.ItemType<ConductorsCorsage>(),
                ModContent.ItemType<SymphonyOfBlossoms>()));

            // Money
            itemLoot.Add(ItemDropRule.Common(ItemID.GoldCoin, 1, 20, 30));
        }

        public override Color? GetAlpha(Color lightColor)
        {
            float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.04f) * 0.1f + 0.9f;
            return new Color((int)(255 * pulse), (int)(240 * pulse), (int)(180 * pulse), 255);
        }
    }

    /// <summary>
    /// Custom drop rule: drops 3 random weapons without duplicates from the full Ode to Joy weapon pool (12 weapons).
    /// </summary>
    public class OdeToJoyTreasureBagWeaponRule : IItemDropRule
    {
        public List<IItemDropRuleChainAttempt> ChainedRules => new List<IItemDropRuleChainAttempt>();
        public bool CanDrop(DropAttemptInfo info) => true;

        public void ReportDroprates(List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo)
        {
            int[] possibleDrops = GetPossibleDrops();
            float individualChance = 3f / possibleDrops.Length;
            foreach (int itemType in possibleDrops)
                drops.Add(new DropRateInfo(itemType, 1, 1, individualChance, ratesInfo.conditions));
        }

        public ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info)
        {
            int[] possibleDrops = GetPossibleDrops();
            List<int> shuffled = new List<int>(possibleDrops);
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int j = Main.rand.Next(i + 1);
                (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
            }
            for (int i = 0; i < 3 && i < shuffled.Count; i++)
                CommonCode.DropItem(info, shuffled[i], 1);

            return new ItemDropAttemptResult { State = ItemDropAttemptResultState.Success };
        }

        private int[] GetPossibleDrops()
        {
            return new int[]
            {
                ModContent.ItemType<ThornboundReckoning>(),         // Melee
                ModContent.ItemType<TheGardenersFury>(),            // Melee
                ModContent.ItemType<RoseThornChainsaw>(),           // Melee (Chainsaw)
                ModContent.ItemType<ThornSprayRepeater>(),          // Ranged
                ModContent.ItemType<ThePollinator>(),               // Ranged
                ModContent.ItemType<PetalStormCannon>(),            // Ranged
                ModContent.ItemType<AnthemOfGlory>(),               // Magic
                ModContent.ItemType<HymnOfTheVictorious>(),         // Magic
                ModContent.ItemType<ElysianVerdict>(),              // Magic
                ModContent.ItemType<TriumphantChorus>(),            // Summon
                ModContent.ItemType<TheStandingOvation>(),          // Summon
                ModContent.ItemType<FountainOfJoyousHarmony>()      // Summon
            };
        }
    }
}
