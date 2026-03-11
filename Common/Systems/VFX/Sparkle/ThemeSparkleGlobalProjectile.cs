using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX.Sparkle
{
    /// <summary>
    /// ThemeSparkleGlobalProjectile — Automatically spawns themed 4-point sparkle
    /// explosions when ANY MagnumOpus theme weapon projectile hits an enemy or tile.
    ///
    /// Detects theme membership by checking the projectile's ModProjectile namespace.
    /// This avoids needing to manually wire every weapon — any new weapon added to
    /// a theme folder automatically gets sparkle explosions.
    ///
    /// The sparkle explosion is purely visual (0 damage) and does not interfere
    /// with the weapon's own VFX — it layers on top as an accent.
    ///
    /// Intensity is scaled based on the projectile's damage to create proportional
    /// sparkle bursts (weak projectiles = small sparkle, strong = large sparkle).
    /// </summary>
    public class ThemeSparkleGlobalProjectile : GlobalProjectile
    {
        // Cache theme mappings: projectile type ID → SparkleTheme
        private static readonly Dictionary<int, SparkleTheme> _themeCache = new();
        private static bool _cacheBuilt;

        /// <summary>
        /// Namespace prefixes that map to each theme. Used for auto-detection.
        /// </summary>
        private static readonly (string prefix, SparkleTheme theme)[] ThemeNamespaces = new[]
        {
            ("MagnumOpus.Content.MoonlightSonata", SparkleTheme.MoonlightSonata),
            ("MagnumOpus.Content.Eroica", SparkleTheme.Eroica),
            ("MagnumOpus.Content.SwanLake", SparkleTheme.SwanLake),
            ("MagnumOpus.Content.LaCampanella", SparkleTheme.LaCampanella),
            ("MagnumOpus.Content.EnigmaVariations", SparkleTheme.EnigmaVariations),
            ("MagnumOpus.Content.Fate", SparkleTheme.Fate),
        };

        /// <summary>
        /// Builds the theme cache on first access by scanning all ModProjectile types.
        /// </summary>
        private static void EnsureCache()
        {
            if (_cacheBuilt) return;
            _cacheBuilt = true;
            _themeCache.Clear();

            // Scan all registered ModProjectiles and map their type IDs to themes
            foreach (var modProj in ModContent.GetContent<ModProjectile>())
            {
                if (modProj.Mod?.Name != "MagnumOpus") continue;

                string ns = modProj.GetType().Namespace;
                if (ns == null) continue;

                // Don't tag ThemeSparkleExplosion itself (would cause infinite recursion)
                if (ns.Contains("Sparkle")) continue;

                foreach (var (prefix, theme) in ThemeNamespaces)
                {
                    if (ns.StartsWith(prefix))
                    {
                        _themeCache[modProj.Projectile.type] = theme;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Tries to get the sparkle theme for a projectile type.
        /// </summary>
        public static bool TryGetTheme(int projectileType, out SparkleTheme theme)
        {
            EnsureCache();
            return _themeCache.TryGetValue(projectileType, out theme);
        }

        /// <summary>
        /// Calculate sparkle intensity from projectile damage.
        /// Low damage (10-30) = 0.5 intensity, Medium (30-100) = 0.8, High (100+) = 1.0-1.3
        /// </summary>
        private static float GetIntensity(Projectile proj)
        {
            int dmg = proj.damage;
            if (dmg <= 0) return 0.6f;
            if (dmg < 30) return 0.5f;
            if (dmg < 60) return 0.7f;
            if (dmg < 100) return 0.85f;
            if (dmg < 200) return 1.0f;
            return 1.2f;
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.netMode == NetmodeID.Server) return;
            if (!TryGetTheme(projectile.type, out SparkleTheme theme)) return;

            // Don't spawn if the projectile is the sparkle explosion itself
            if (projectile.ModProjectile is ThemeSparkleExplosion) return;

            float intensity = GetIntensity(projectile);
            Vector2 hitPos = target.Center;

            // Use the impact point (between projectile and target center) for more accurate placement
            if (projectile.Center != Vector2.Zero)
                hitPos = Vector2.Lerp(projectile.Center, target.Center, 0.4f);

            ThemeSparkleExplosion.Spawn(projectile.GetSource_OnHit(target, "ThemeSparkle"),
                hitPos, theme, intensity);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            if (Main.netMode == NetmodeID.Server) return true;
            if (!TryGetTheme(projectile.type, out SparkleTheme theme)) return true;
            if (projectile.ModProjectile is ThemeSparkleExplosion) return true;

            // Only spawn sparkles on tile collide if the projectile will die (penetrate == 0 or 1)
            // or if it bounces (don't spam sparkles for pass-through projectiles)
            if (projectile.penetrate > 1 && projectile.penetrate != -1) return true;

            float intensity = GetIntensity(projectile) * 0.7f; // Slightly less intense for tile hits
            ThemeSparkleExplosion.Spawn(projectile.GetSource_Death("ThemeSparkle"),
                projectile.Center, theme, intensity);

            return true;
        }

        public override void Unload()
        {
            _themeCache.Clear();
            _cacheBuilt = false;
        }
    }
}
