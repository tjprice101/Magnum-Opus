using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;

namespace MagnumOpus.Common.Systems
{
    public class MoonLordTreasureBagLoot : GlobalItem
    {
        public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
        {
            if (item.type == ItemID.MoonLordBossBag)
            {
                // Add drops to treasure bag for expert/master mode
                var afterFirstKillCondition = new AfterFirstMoonLordKillCondition();
                
                itemLoot.Add(new LeadingConditionRule(afterFirstKillCondition)
                    .OnSuccess(ItemDropRule.Common(ModContent.ItemType<MoonlightsResonantEnergy>(), 1, 5, 10)));
                    
                itemLoot.Add(new LeadingConditionRule(afterFirstKillCondition)
                    .OnSuccess(ItemDropRule.Common(ModContent.ItemType<RemnantOfMoonlightsHarmony>(), 1, 20, 30)));
            }
        }
    }
}
