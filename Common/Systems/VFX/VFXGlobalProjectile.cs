using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// VFX GLOBAL PROJECTILE - Automatically applies enhanced VFX to all MagnumOpus projectiles.
    /// 
    /// This system provides:
    /// - Automatic theme detection from projectile namespace
    /// - Enhanced trail VFX with screen distortions
    /// - Enhanced death VFX with particle cascades
    /// - Music note integration for the music theme
    /// </summary>
    public class VFXGlobalProjectile : GlobalProjectile
    {
        // Cache to track which projectiles have active trails
        private static readonly Dictionary<int, int> _projectileTrails = new Dictionary<int, int>();
        
        // Theme detection from namespace
        private static readonly Dictionary<string, string> _namespaceToTheme = new Dictionary<string, string>
        {
            { "Eroica", "Eroica" },
            { "SwanLake", "SwanLake" },
            { "LaCampanella", "LaCampanella" },
            { "MoonlightSonata", "MoonlightSonata" },
            { "EnigmaVariations", "Enigma" },
            { "Fate", "Fate" },
            { "DiesIrae", "DiesIrae" },
            { "ClairDeLune", "ClairDeLune" },
            { "Nachtmusik", "Nachtmusik" },
            { "OdeToJoy", "OdeToJoy" },
            { "Spring", "Spring" },
            { "Summer", "Summer" },
            { "Autumn", "Autumn" },
            { "Winter", "Winter" }
        };

        public override bool InstancePerEntity => false;

        /// <summary>
        /// Detect theme from projectile's namespace
        /// </summary>
        private static string DetectTheme(Projectile projectile)
        {
            if (projectile.ModProjectile == null)
                return null;

            string fullName = projectile.ModProjectile.GetType().FullName ?? "";
            
            foreach (var kvp in _namespaceToTheme)
            {
                if (fullName.Contains(kvp.Key))
                    return kvp.Value;
            }

            return null;
        }

        public override void AI(Projectile projectile)
        {
            // Only process MagnumOpus projectiles
            if (projectile.ModProjectile == null || !projectile.ModProjectile.Mod.Name.Equals("MagnumOpus"))
                return;

            // Detect theme
            string theme = DetectTheme(projectile);
            if (theme == null)
                return;

            // Apply enhanced trail VFX periodically (every 3 frames for performance)
            if (Main.GameUpdateCount % 3 == 0)
            {
                ApplyEnhancedTrailVFX(projectile, theme);
            }

            // Apply orbiting music notes for projectiles that should have them
            if (projectile.friendly && Main.rand.NextBool(45))
            {
                ApplyOrbitingMusicNotes(projectile, theme);
            }
        }

        /// <summary>
        /// Apply enhanced trail VFX - subtle additions that complement existing trails
        /// </summary>
        private static void ApplyEnhancedTrailVFX(Projectile projectile, string theme)
        {
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            if (palette == null || palette.Length == 0)
                return;

            Color primary = palette[0];
            Color secondary = palette.Length > 1 ? palette[1] : primary;

            // Subtle color oscillation flare (complements existing effects)
            if (Main.rand.NextBool(8))
            {
                float hue = (Main.GameUpdateCount * 0.02f) % 1f;
                Color oscillatedColor = Color.Lerp(primary, secondary, hue);
                CustomParticles.GenericFlare(projectile.Center + Main.rand.NextVector2Circular(8f, 8f), oscillatedColor * 0.5f, 0.25f, 10);
            }

            // Contrasting sparkle (adds depth to trails)
            if (Main.rand.NextBool(12))
            {
                var sparkle = new SparkleParticle(
                    projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    -projectile.velocity * 0.08f,
                    Color.White * 0.6f,
                    0.2f,
                    12
                );
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }

        /// <summary>
        /// Apply orbiting music notes - the signature MagnumOpus effect
        /// </summary>
        private static void ApplyOrbitingMusicNotes(Projectile projectile, string theme)
        {
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            if (palette == null || palette.Length == 0)
                return;

            Color primary = palette[0];
            
            // Only spawn notes if projectile has significant velocity
            if (projectile.velocity.LengthSquared() < 4f)
                return;

            float orbitAngle = Main.GameUpdateCount * 0.08f;
            float orbitRadius = 12f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 4f;

            // Spawn 1-2 orbiting notes
            int noteCount = Main.rand.Next(1, 3);
            for (int i = 0; i < noteCount; i++)
            {
                float noteAngle = orbitAngle + MathHelper.TwoPi * i / noteCount;
                Vector2 noteOffset = noteAngle.ToRotationVector2() * orbitRadius;
                Vector2 notePos = projectile.Center + noteOffset;
                
                // Note velocity matches projectile + slight outward drift
                Vector2 noteVel = projectile.velocity * 0.6f + noteAngle.ToRotationVector2() * 0.3f;
                
                // Shimmer effect
                float shimmer = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f + i) * 0.1f;
                
                ThemedParticles.MusicNote(notePos, noteVel, primary, 0.7f * shimmer, 25);
            }
        }

        public override void OnKill(Projectile projectile, int timeLeft)
        {
            // Only process MagnumOpus projectiles
            if (projectile.ModProjectile == null || !projectile.ModProjectile.Mod.Name.Equals("MagnumOpus"))
                return;

            // Detect theme
            string theme = DetectTheme(projectile);
            if (theme == null)
                return;

            // Apply enhanced death VFX
            ApplyEnhancedDeathVFX(projectile, theme);
        }

        /// <summary>
        /// Apply enhanced death VFX - additional effects that complement existing OnKill
        /// </summary>
        private static void ApplyEnhancedDeathVFX(Projectile projectile, string theme)
        {
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            if (palette == null || palette.Length == 0)
                return;

            Color primary = palette[0];
            Color secondary = palette.Length > 1 ? palette[1] : primary;

            // Subtle screen distortion on death (very minor)
            if (projectile.damage > 50)
            {
                ScreenDistortionManager.TriggerThemeEffect(theme, projectile.Center, 0.15f, 8);
            }

            // Additional expanding ring
            CustomParticles.HaloRing(projectile.Center, primary * 0.6f, 0.2f, 12);

            // Music note finale (1-3 notes)
            int noteCount = Main.rand.Next(1, 4);
            for (int i = 0; i < noteCount; i++)
            {
                float angle = MathHelper.TwoPi * i / noteCount;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color noteColor = Color.Lerp(primary, secondary, (float)i / noteCount);
                ThemedParticles.MusicNote(projectile.Center, noteVel, noteColor, 0.75f, 25);
            }

            // Sparkle scatter
            for (int i = 0; i < 4; i++)
            {
                Vector2 sparklePos = projectile.Center + Main.rand.NextVector2Circular(15f, 15f);
                Vector2 sparkleVel = Main.rand.NextVector2Circular(3f, 3f);
                Color sparkleColor = Color.Lerp(primary, Color.White, Main.rand.NextFloat(0.3f, 0.6f));
                
                var sparkle = new SparkleParticle(sparklePos, sparkleVel, sparkleColor, 0.3f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }
    }
}
