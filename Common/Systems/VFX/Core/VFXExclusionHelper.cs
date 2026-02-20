using Terraria;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// Helper class for excluding debug weapons from global VFX systems.
    /// All GlobalProjectile and GlobalItem classes should use these methods.
    /// </summary>
    public static class VFXExclusionHelper
    {
        /// <summary>
        /// Returns true if this projectile should be EXCLUDED from global VFX.
        /// Debug weapons and test projectiles return true.
        /// </summary>
        public static bool ShouldExcludeProjectile(Projectile projectile)
        {
            if (projectile?.ModProjectile == null)
                return false;
                
            string typeName = projectile.ModProjectile.GetType().FullName ?? "";
            
            // Exclude all debug weapons, test projectiles, and sandbox weapons
            return typeName.Contains("Debug") || 
                   typeName.Contains("DebugWeapons") ||
                   typeName.Contains("Test") ||
                   typeName.Contains("Sandbox");
        }
        
        /// <summary>
        /// Returns true if this item should be EXCLUDED from global VFX.
        /// Debug weapons and sandbox weapons return true.
        /// </summary>
        public static bool ShouldExcludeItem(Item item)
        {
            if (item?.ModItem == null)
                return false;
                
            string typeName = item.ModItem.GetType().FullName ?? "";
            
            // Exclude all debug weapons, test items, and sandbox weapons
            return typeName.Contains("Debug") || 
                   typeName.Contains("DebugWeapons") ||
                   typeName.Contains("Test") ||
                   typeName.Contains("Sandbox");
        }
    }
}
