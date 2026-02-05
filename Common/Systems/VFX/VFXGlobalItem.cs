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
    /// VFX GLOBAL ITEM - Automatically applies enhanced VFX to all MagnumOpus weapons.
    /// 
    /// This system provides:
    /// - Automatic theme detection from item namespace
    /// - Enhanced melee swing VFX with particle trails
    /// - Enhanced impact VFX on hit
    /// - Music note integration for the music theme
    /// - Muzzle flash enhancement for ranged weapons
    /// </summary>
    public class VFXGlobalItem : GlobalItem
    {
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
        /// Detect theme from item's namespace
        /// </summary>
        private static string DetectTheme(Item item)
        {
            if (item.ModItem == null)
                return null;

            string fullName = item.ModItem.GetType().FullName ?? "";
            
            foreach (var kvp in _namespaceToTheme)
            {
                if (fullName.Contains(kvp.Key))
                    return kvp.Value;
            }

            return null;
        }

        /// <summary>
        /// Calculate swing progress from animation timer
        /// </summary>
        private static float GetSwingProgress(Player player)
        {
            if (player.itemAnimation <= 0 || player.itemAnimationMax <= 0)
                return 0f;
            
            return 1f - (float)player.itemAnimation / player.itemAnimationMax;
        }

        public override void MeleeEffects(Item item, Player player, Rectangle hitbox)
        {
            // Only process MagnumOpus items
            if (item.ModItem == null || !item.ModItem.Mod.Name.Equals("MagnumOpus"))
                return;

            // Detect theme
            string theme = DetectTheme(item);
            if (theme == null)
                return;

            float swingProgress = GetSwingProgress(player);
            
            // Apply enhanced melee swing VFX (during active swing phase)
            if (swingProgress > 0.2f && swingProgress < 0.8f)
            {
                ApplyEnhancedMeleeSwingVFX(player, hitbox, theme, swingProgress);
            }
        }

        /// <summary>
        /// Apply enhanced melee swing VFX - subtle additions that complement existing effects
        /// </summary>
        private static void ApplyEnhancedMeleeSwingVFX(Player player, Rectangle hitbox, string theme, float swingProgress)
        {
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            if (palette == null || palette.Length == 0)
                return;

            Color primary = palette[0];
            Color secondary = palette.Length > 1 ? palette[1] : primary;
            Vector2 center = hitbox.Center.ToVector2();

            // Color oscillation effect (every 2 frames)
            if (Main.GameUpdateCount % 2 == 0 && Main.rand.NextBool(3))
            {
                float hue = (Main.GameUpdateCount * 0.025f) % 1f;
                Color oscillatedColor = Color.Lerp(primary, secondary, hue);
                CustomParticles.GenericFlare(center + Main.rand.NextVector2Circular(12f, 12f), oscillatedColor * 0.4f, 0.28f, 10);
            }

            // Contrasting sparkle accents
            if (Main.rand.NextBool(4))
            {
                var sparkle = new SparkleParticle(
                    center + Main.rand.NextVector2Circular(15f, 15f),
                    player.velocity * 0.2f + Main.rand.NextVector2Circular(1.5f, 1.5f),
                    Color.White * 0.7f,
                    0.28f,
                    15
                );
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Music notes scattered in swing (rare)
            if (Main.rand.NextBool(12))
            {
                Vector2 notePos = center + Main.rand.NextVector2Circular(20f, 20f);
                Vector2 swingDir = player.itemRotation.ToRotationVector2();
                Vector2 noteVel = swingDir.RotatedByRandom(0.5f) * Main.rand.NextFloat(1.5f, 3f);
                
                float shimmer = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.1f;
                ThemedParticles.MusicNote(notePos, noteVel, primary * 0.85f, 0.7f * shimmer, 28);
            }

            // Subtle dust enhancement (complements existing dust)
            if (Main.rand.NextBool(3))
            {
                Vector2 dustPos = center + Main.rand.NextVector2Circular(hitbox.Width / 3f, hitbox.Height / 3f);
                Vector2 dustVel = player.velocity * 0.25f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                
                Dust d = Dust.NewDustPerfect(dustPos, DustID.MagicMirror, dustVel, 0, primary, 1.1f);
                d.noGravity = true;
                d.fadeIn = 0.9f;
            }
        }

        public override void OnHitNPC(Item item, Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Only process MagnumOpus items
            if (item.ModItem == null || !item.ModItem.Mod.Name.Equals("MagnumOpus"))
                return;

            // Detect theme
            string theme = DetectTheme(item);
            if (theme == null)
                return;

            // Apply enhanced impact VFX
            ApplyEnhancedImpactVFX(target.Center, theme, hit.Crit ? 1.3f : 1f);
        }

        /// <summary>
        /// Apply enhanced impact VFX - subtle additions that complement existing OnHit effects
        /// </summary>
        private static void ApplyEnhancedImpactVFX(Vector2 position, string theme, float scale)
        {
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            if (palette == null || palette.Length == 0)
                return;

            Color primary = palette[0];
            Color secondary = palette.Length > 1 ? palette[1] : primary;

            // Small screen distortion on crit
            if (scale > 1f)
            {
                ScreenDistortionManager.TriggerThemeEffect(theme, position, 0.12f, 6);
            }

            // Additional glimmer layer
            CustomParticles.GenericFlare(position, Color.Lerp(primary, Color.White, 0.3f), 0.35f * scale, 12);

            // Music note impact accent
            if (Main.rand.NextBool(2))
            {
                Vector2 noteVel = Main.rand.NextVector2Circular(2f, 2f);
                ThemedParticles.MusicNote(position, noteVel, primary, 0.65f, 22);
            }

            // Sparkle burst
            for (int i = 0; i < 3; i++)
            {
                Vector2 sparklePos = position + Main.rand.NextVector2Circular(12f, 12f);
                Vector2 sparkleVel = Main.rand.NextVector2Circular(2.5f, 2.5f);
                Color sparkleColor = Color.Lerp(primary, Color.White, Main.rand.NextFloat(0.2f, 0.5f));
                
                var sparkle = new SparkleParticle(sparklePos, sparkleVel, sparkleColor, 0.25f * scale, 15);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }

        public override void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            // Only process MagnumOpus ranged weapons
            if (item.ModItem == null || !item.ModItem.Mod.Name.Equals("MagnumOpus"))
                return;

            if (!item.DamageType.CountsAsClass(DamageClass.Ranged) && !item.DamageType.CountsAsClass(DamageClass.Magic))
                return;

            // Detect theme
            string theme = DetectTheme(item);
            if (theme == null)
                return;

            // Apply enhanced muzzle flash/cast VFX
            ApplyEnhancedShootVFX(position, velocity, theme, item.DamageType.CountsAsClass(DamageClass.Magic));
        }

        /// <summary>
        /// Apply enhanced shoot VFX for ranged and magic weapons
        /// </summary>
        private static void ApplyEnhancedShootVFX(Vector2 position, Vector2 velocity, string theme, bool isMagic)
        {
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            if (palette == null || palette.Length == 0)
                return;

            Color primary = palette[0];
            Color secondary = palette.Length > 1 ? palette[1] : primary;

            // Subtle muzzle/cast flash
            CustomParticles.GenericFlare(position, primary * 0.6f, 0.3f, 10);

            // Small halo on magic cast
            if (isMagic)
            {
                CustomParticles.HaloRing(position, primary * 0.4f, 0.18f, 8);
            }

            // Directional particles
            Vector2 dir = velocity.SafeNormalize(Vector2.UnitX);
            for (int i = 0; i < 2; i++)
            {
                Vector2 sparkPos = position + dir.RotatedByRandom(0.4f) * Main.rand.NextFloat(5f, 15f);
                Color sparkColor = Color.Lerp(primary, secondary, Main.rand.NextFloat());
                
                var sparkle = new SparkleParticle(sparkPos, dir * Main.rand.NextFloat(1f, 2f), sparkColor * 0.7f, 0.2f, 10);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Music note on cast (rare)
            if (Main.rand.NextBool(4))
            {
                Vector2 noteVel = dir.RotatedByRandom(0.6f) * Main.rand.NextFloat(1f, 2.5f);
                ThemedParticles.MusicNote(position + dir * 10f, noteVel, primary * 0.8f, 0.6f, 20);
            }
        }
    }
}
