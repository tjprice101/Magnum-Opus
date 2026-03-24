using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.MoonlightSonata.Enemies;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// Adds tempo shard drops to Moon Lord.
    /// These drops are used for weapon upgrades and high-tier crafting.
    /// </summary>
    public class VanillaBossDropSystem : GlobalNPC
    {
        public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        {
            // Apply to Moon Lord only
            return entity.type == NPCID.MoonLordCore;
        }

        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            switch (npc.type)
            {
                case NPCID.MoonLordCore:
                    // Moon Lord ALWAYS drops Shards of Moonlit Tempo (10-20) - Moonlight Sonata
                    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ShardsOfMoonlitTempo>(), 1, 10, 20));
                    break;
            }
        }
    }
}
