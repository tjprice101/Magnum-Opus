using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// GlobalNPC that helps with enemy hit detection.
    /// Note: ModifyHitbox was removed in newer tModLoader versions.
    /// Hitbox expansion is now handled differently - NPCs use their actual hitbox dimensions.
    /// This class remains as a placeholder for any future hit detection modifications.
    /// </summary>
    public class EnemyHitboxExpansion : GlobalNPC
    {
        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            // This hook is called when a projectile hits
            // Can be used for custom hit detection logic if needed
        }
        
        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            // This hook is called when melee hits
            // Can be used for custom hit detection logic if needed
        }
    }
}
