using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// GlobalProjectile that randomly triggers lens flare effects on MagnumOpus projectile deaths.
    /// Only affects projectiles from this mod to avoid visual clutter with vanilla/other mods.
    /// </summary>
    public class LensFlareGlobalProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => false;
        
        public override void OnKill(Projectile projectile, int timeLeft)
        {
            // Only affect our mod's projectiles
            if (projectile.ModProjectile == null || projectile.ModProjectile.Mod != Mod)
                return;
            
            // Skip tiny/invisible projectiles and minions
            if (projectile.width < 8 || projectile.height < 8)
                return;
            if (projectile.minion || projectile.sentry)
                return;
            
            // Get the projectile owner's weapon theme colors
            Color primary, secondary;
            if (!TryGetProjectileThemeColors(projectile, out primary, out secondary))
                return;
            
            // Random chance for lens flare - 12% base, higher for explosions
            float chance = 0.12f;
            
            // Increase chance for larger/more impactful projectiles
            if (projectile.width >= 20 || projectile.height >= 20)
                chance = 0.2f;
            if (projectile.width >= 40 || projectile.height >= 40)
                chance = 0.3f;
            
            // Trigger the lens flare
            WeaponLensFlare.TrySpawnImpactLensFlare(projectile.Center, primary, secondary, chance);
        }
        
        private bool TryGetProjectileThemeColors(Projectile projectile, out Color primary, out Color secondary)
        {
            primary = Color.White;
            secondary = Color.LightGray;
            
            // Try to determine theme from projectile's full type name
            string typeName = projectile.ModProjectile?.GetType().FullName ?? "";
            
            if (typeName.Contains("Eroica"))
            {
                primary = new Color(255, 50, 50);     // Scarlet
                secondary = new Color(255, 200, 80);  // Gold
                return true;
            }
            if (typeName.Contains("Moonlight") || typeName.Contains("MoonlightSonata"))
            {
                primary = new Color(138, 43, 226);    // Purple
                secondary = new Color(135, 206, 250); // Ice blue
                return true;
            }
            if (typeName.Contains("LaCampanella") || typeName.Contains("Campanella"))
            {
                primary = new Color(255, 140, 40);    // Orange
                secondary = new Color(255, 200, 50);  // Yellow
                return true;
            }
            if (typeName.Contains("SwanLake") || typeName.Contains("Swan"))
            {
                primary = Color.White;
                secondary = new Color(200, 220, 255); // Pale blue
                return true;
            }
            if (typeName.Contains("Enigma"))
            {
                primary = new Color(140, 60, 200);    // Purple
                secondary = new Color(50, 220, 100);  // Green
                return true;
            }
            if (typeName.Contains("Fate"))
            {
                primary = new Color(200, 80, 120);    // Dark pink
                secondary = new Color(140, 50, 160);  // Purple
                return true;
            }
            
            // Default - still allow lens flares for other mod projectiles with neutral colors
            return true;
        }
    }
}
