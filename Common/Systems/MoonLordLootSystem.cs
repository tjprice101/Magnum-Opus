using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;

namespace MagnumOpus.Common.Systems
{
    public class MoonLordLootSystem : GlobalNPC
    {
        public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        {
            return entity.type == NPCID.MoonLordCore;
        }

        public override void OnKill(NPC npc)
        {
            if (npc.type == NPCID.MoonLordCore)
            {
                // First kill - spawn ore
                if (!MoonlightSonataSystem.MoonLordKilledOnce)
                {
                    MoonlightSonataSystem.OnFirstMoonLordKill();
                }
                // Subsequent kills - drop items directly (not in expert mode, those go in bag)
                else if (Main.netMode != NetmodeID.MultiplayerClient && !Main.expertMode)
                {
                    DropMoonlightLoot(npc);
                }
            }
        }

        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            if (npc.type == NPCID.MoonLordCore)
            {
                // This adds drops for non-expert mode after first kill
                // We use a custom condition to check if Moon Lord was already killed once
                var afterFirstKillCondition = new AfterFirstMoonLordKillCondition();
                var notExpertCondition = new Conditions.NotExpert();

                // Combine conditions: not expert AND after first kill
                var combinedRule = new LeadingConditionRule(notExpertCondition);
                combinedRule.OnSuccess(
                    new LeadingConditionRule(afterFirstKillCondition)
                        .OnSuccess(ItemDropRule.Common(ModContent.ItemType<MoonlightsResonantEnergy>(), 1, 5, 10))
                );
                combinedRule.OnSuccess(
                    new LeadingConditionRule(afterFirstKillCondition)
                        .OnSuccess(ItemDropRule.Common(ModContent.ItemType<RemnantOfMoonlightsHarmony>(), 1, 20, 30))
                );

                npcLoot.Add(combinedRule);
            }
        }

        private static void DropMoonlightLoot(NPC npc)
        {
            // Drop Moonlight Sonata Resonant Energy (5-10)
            int energyAmount = Main.rand.Next(5, 11);
            Item.NewItem(npc.GetSource_Loot(), npc.getRect(), 
                ModContent.ItemType<MoonlightsResonantEnergy>(), energyAmount);

            // Drop Remnants of Moonlight's Harmony (20-30)
            int crystalAmount = Main.rand.Next(20, 31);
            Item.NewItem(npc.GetSource_Loot(), npc.getRect(), 
                ModContent.ItemType<RemnantOfMoonlightsHarmony>(), crystalAmount);
        }
    }

    public class AfterFirstMoonLordKillCondition : IItemDropRuleCondition
    {
        public bool CanDrop(DropAttemptInfo info)
        {
            return MoonlightSonataSystem.MoonLordKilledOnce;
        }

        public bool CanShowItemDropInUI()
        {
            return true;
        }

        public string GetConditionDescription()
        {
            return "After Moon Lord has been defeated once";
        }
    }
}
