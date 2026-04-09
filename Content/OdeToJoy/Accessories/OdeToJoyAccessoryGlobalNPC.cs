using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Accessories
{
    /// <summary>
    /// Global NPC tracking Ode to Joy accessory debuffs on enemies.
    /// Hymnal Anchor: -20% movement speed, -5 defense (from The Verdant Refrain minion hits).
    /// </summary>
    public class OdeToJoyAccessoryGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        // --- Hymnal Anchor (The Verdant Refrain) ---
        public int hymnalAnchorTimer;

        public override void ResetEffects(NPC npc)
        {
            if (hymnalAnchorTimer > 0)
                hymnalAnchorTimer--;
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            // No damage modifier for Hymnal Anchor - it's a slow/defense debuff
        }

        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            // No damage modifier for Hymnal Anchor
        }

        public override void PostAI(NPC npc)
        {
            if (hymnalAnchorTimer > 0)
            {
                npc.velocity *= 0.80f; // -20% movement speed
                npc.defense = System.Math.Max(0, npc.defense - 5);
            }
        }

        public void ApplyHymnalAnchor(NPC npc, int duration = 180)
        {
            hymnalAnchorTimer = duration; // default 3 seconds
        }

        public bool HasHymnalAnchor => hymnalAnchorTimer > 0;
    }
}