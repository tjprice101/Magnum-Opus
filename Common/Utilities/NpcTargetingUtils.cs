using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;

namespace MagnumOpus.Common.Utilities
{
    public static class NpcTargetingUtils
    {
        public static List<NPC> CollectHostiles(Vector2 center, float radius)
        {
            List<NPC> list = new();
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.life <= 0)
                    continue;
                if (npc.Distance(center) <= radius)
                    list.Add(npc);
            }
            return list;
        }

        public static IEnumerable<NPC> EnumerateHostiles(Vector2 center, float radius)
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.life <= 0)
                    continue;
                if (npc.Distance(center) <= radius)
                    yield return npc;
            }
        }

        public static NPC FindClosestNpc(Vector2 center, float maxDist, HashSet<int> exclude = null)
        {
            NPC best = null;
            float bestDist = maxDist;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.life <= 0)
                    continue;
                if (exclude != null && exclude.Contains(npc.whoAmI))
                    continue;
                float dist = npc.Distance(center);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = npc;
                }
            }
            return best;
        }
    }
}
