using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.MoonlightSonata.Enemies;
using MagnumOpus.Content.Common.Consumables;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// Adds drops to Moon Lord: tempo shards, Crystallized Harmony, and Arcane Harmonic Prism.
    /// </summary>
    public class VanillaBossDropSystem : GlobalNPC
    {
        public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        {
            return entity.type == NPCID.MoonLordCore;
        }

        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            switch (npc.type)
            {
                case NPCID.MoonLordCore:
                    // Moon Lord ALWAYS drops Shards of Moonlit Tempo (10-20) - Moonlight Sonata
                    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ShardsOfMoonlitTempo>(), 1, 10, 20));

                    // Moon Lord ALWAYS drops Crystallized Harmony (3-5) - permanent +5 max health per use
                    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<CrystallizedHarmony>(), 1, 3, 5));

                    // Moon Lord ALWAYS drops Arcane Harmonic Prism (3-5) - permanent +20 max mana per use
                    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ArcaneHarmonicPrism>(), 1, 3, 5));
                    break;
            }
        }
    }
}
