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
            }
        }

        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            // Moon Lord drops ShardsOfMoonlitTempo are handled by VanillaBossDropSystem
            // This system is kept for potential future functionality
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
