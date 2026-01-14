using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using MagnumOpus.Content.MoonlightSonata.Enemies;
using MagnumOpus.Content.Eroica.Enemies;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// Custom drop condition for post-Moon Lord drops
    /// </summary>
    public class DownedMoonLordCondition : IItemDropRuleCondition
    {
        public bool CanDrop(DropAttemptInfo info) => NPC.downedMoonlord;
        public bool CanShowItemDropInUI() => NPC.downedMoonlord;
        public string GetConditionDescription() => Language.GetTextValue("Mods.MagnumOpus.DropConditions.PostMoonLord");
    }

    /// <summary>
    /// Adds tempo shard drops to Moon Lord and Eye of Cthulhu.
    /// These drops are used for weapon upgrades and high-tier crafting.
    /// </summary>
    public class VanillaBossDropSystem : GlobalNPC
    {
        public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        {
            // Apply to Moon Lord and Eye of Cthulhu only
            return entity.type == NPCID.MoonLordCore ||
                   entity.type == NPCID.EyeofCthulhu;
        }

        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            // Post-Moon Lord condition for conditional drops
            var postMoonLordCondition = new DownedMoonLordCondition();

            switch (npc.type)
            {
                case NPCID.MoonLordCore:
                    // Moon Lord ALWAYS drops Shards of Moonlit Tempo (10-20) - Moonlight Sonata
                    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ShardsOfMoonlitTempo>(), 1, 10, 20));
                    break;

                case NPCID.EyeofCthulhu:
                    // Eye of Cthulhu (post-ML) drops Shard of Triumph's Tempo (4-8) - Eroica
                    npcLoot.Add(ItemDropRule.ByCondition(postMoonLordCondition, 
                        ModContent.ItemType<ShardOfTriumphsTempo>(), 1, 4, 8));
                    break;
            }
        }
    }
}
