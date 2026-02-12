using Microsoft.Xna.Framework;
using System;
using Terraria;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// Advanced VFX effects with themed impact styles.
    /// Provides high-level effect methods for various visual styles.
    /// </summary>
    public static class AdvancedVFXEffects
    {
        #region Impact Effects
        
        /// <summary>
        /// Creates an infernal-style impact with flames and smoke.
        /// </summary>
        public static void InfernalImpact(Vector2 position, Color primaryColor, Color secondaryColor, float scale = 1f)
        {
            // Core flash - use particles since we don't have SpriteBatch access
            CustomParticles.GenericFlare(position, Color.White, scale * 0.8f, 15);
            CustomParticles.GenericFlare(position, primaryColor, scale, 18);
            
            // Spawn particles
            for (int i = 0; i < (int)(8 * scale); i++)
            {
                float angle = MathHelper.TwoPi * i / (8 * scale);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f) * scale;
                CustomParticles.GenericFlare(position + vel * 3f, Color.Lerp(primaryColor, secondaryColor, Main.rand.NextFloat()), 0.4f * scale, 20);
            }
        }
        
        /// <summary>
        /// Creates a celestial-style impact with golden sparks.
        /// </summary>
        public static void CelestialImpact(Vector2 position, Color primaryColor, Color secondaryColor, float scale = 1f)
        {
            // Core flashes - use particles since we don't have SpriteBatch access
            CustomParticles.GenericFlare(position, Color.White, scale * 0.6f, 12);
            CustomParticles.GenericFlare(position, primaryColor, scale * 0.9f, 16);
            
            for (int i = 0; i < (int)(6 * scale); i++)
            {
                float angle = MathHelper.TwoPi * i / (6 * scale);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f) * scale;
                CustomParticles.GenericFlare(position + vel * 5f, Color.Lerp(primaryColor, secondaryColor, Main.rand.NextFloat()), 0.35f * scale, 18);
            }
        }
        
        /// <summary>
        /// Creates a chromatic/rainbow-style impact.
        /// </summary>
        public static void ChromaticImpact(Vector2 position, float scale = 1f)
        {
            // White core flash
            CustomParticles.GenericFlare(position, Color.White, scale * 0.6f, 15);
            
            for (int i = 0; i < (int)(12 * scale); i++)
            {
                float hue = (float)i / (12 * scale);
                Color rainbowColor = Main.hslToRgb(hue, 1f, 0.7f);
                float angle = MathHelper.TwoPi * i / (12 * scale);
                Vector2 offset = angle.ToRotationVector2() * 20f * scale;
                CustomParticles.GenericFlare(position + offset, rainbowColor, 0.3f * scale, 25);
            }
        }
        
        /// <summary>
        /// Creates an ethereal-style impact with soft glows.
        /// </summary>
        public static void EtherealImpact(Vector2 position, Color primaryColor, Color secondaryColor, float scale = 1f)
        {
            // Soft layered glows - use particles since we don't have SpriteBatch access
            CustomParticles.GenericFlare(position, primaryColor, scale * 1.2f, 25);
            CustomParticles.GenericFlare(position, secondaryColor, scale * 0.8f, 20);
            
            for (int i = 0; i < (int)(5 * scale); i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(30f * scale, 30f * scale);
                CustomParticles.GenericFlare(position + offset, Color.Lerp(primaryColor, secondaryColor, Main.rand.NextFloat()), 0.25f * scale, 22);
            }
        }
        
        /// <summary>
        /// Creates a void-style impact with dark energy.
        /// </summary>
        public static void VoidImpact(Vector2 position, Color primaryColor, Color secondaryColor, float scale = 1f)
        {
            // Dark core - use particles since we don't have SpriteBatch access
            CustomParticles.GenericFlare(position, new Color(40, 20, 60), scale * 0.5f, 18);
            
            // Outer ring
            for (int i = 0; i < (int)(10 * scale); i++)
            {
                float angle = MathHelper.TwoPi * i / (10 * scale);
                Vector2 offset = angle.ToRotationVector2() * 25f * scale;
                Color color = i % 2 == 0 ? primaryColor : secondaryColor;
                CustomParticles.GenericFlare(position + offset, color, 0.3f * scale, 20);
            }
        }
        
        /// <summary>
        /// Creates a themed impact based on theme name.
        /// </summary>
        public static void ThemedImpact(string themeName, Vector2 position, float scale = 1f)
        {
            Color primary = MagnumThemePalettes.GetThemePrimary(themeName);
            Color secondary = MagnumThemePalettes.GetThemeSecondary(themeName);
            
            switch (themeName.ToLowerInvariant())
            {
                case "lacampanella":
                case "campanella":
                    InfernalImpact(position, primary, secondary, scale);
                    break;
                case "swanlake":
                case "swan":
                    ChromaticImpact(position, scale);
                    break;
                case "enigma":
                case "enigmavariations":
                case "fate":
                    VoidImpact(position, primary, secondary, scale);
                    break;
                default:
                    CelestialImpact(position, primary, secondary, scale);
                    break;
            }
        }
        
        #endregion
        
        #region Trail Segment Effects
        
        /// <summary>
        /// Creates a flame trail segment particle effect.
        /// </summary>
        public static void FlameTrailSegment(Vector2 position, Vector2 direction, Color primaryColor, Color secondaryColor, float scale = 1f)
        {
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
            
            for (int i = 0; i < 2; i++)
            {
                Vector2 offset = perpendicular * Main.rand.NextFloat(-10f, 10f) * scale;
                Color color = Main.rand.NextBool() ? primaryColor : secondaryColor;
                CustomParticles.GenericFlare(position + offset, color, 0.2f * scale, 15);
            }
        }
        
        /// <summary>
        /// Creates an ice trail segment particle effect.
        /// </summary>
        public static void IceTrailSegment(Vector2 position, Vector2 direction, Color primaryColor, Color secondaryColor, float scale = 1f)
        {
            if (Main.rand.NextBool(3))
            {
                Vector2 offset = Main.rand.NextVector2Circular(5f, 5f) * scale;
                Color color = Color.Lerp(primaryColor, Color.White, Main.rand.NextFloat(0.3f));
                CustomParticles.GenericFlare(position + offset, color, 0.15f * scale, 20);
            }
        }
        
        /// <summary>
        /// Creates a lightning trail segment particle effect.
        /// </summary>
        public static void LightningTrailSegment(Vector2 position, Vector2 direction, Color color, float scale = 1f)
        {
            if (Main.rand.NextBool(4))
            {
                Vector2 offset = Main.rand.NextVector2Circular(8f, 8f) * scale;
                CustomParticles.GenericFlare(position + offset, color, 0.2f * scale, 10);
            }
        }
        
        /// <summary>
        /// Creates a nature trail segment particle effect.
        /// </summary>
        public static void NatureTrailSegment(Vector2 position, Vector2 direction, Color primaryColor, Color secondaryColor, float scale = 1f)
        {
            if (Main.rand.NextBool(5))
            {
                Color color = Main.rand.NextBool() ? primaryColor : secondaryColor;
                CustomParticles.GenericFlare(position, color, 0.18f * scale, 25);
            }
        }
        
        /// <summary>
        /// Creates a cosmic trail segment particle effect.
        /// </summary>
        public static void CosmicTrailSegment(Vector2 position, Vector2 direction, Color primaryColor, Color secondaryColor, float scale = 1f)
        {
            if (Main.rand.NextBool(3))
            {
                float hue = Main.rand.NextFloat();
                Color color = Main.rand.NextBool() ? Color.Lerp(primaryColor, secondaryColor, Main.rand.NextFloat()) : Main.hslToRgb(hue, 0.8f, 0.7f);
                CustomParticles.GenericFlare(position, color, 0.15f * scale, 18);
            }
        }
        
        #endregion
        
        #region Theme-Specific Effects
        
        /// <summary>
        /// Spawns theme-specific ambient particles.
        /// </summary>
        public static void SpawnThemeAmbient(string themeName, Vector2 position, float radius, int count)
        {
            Color primary = MagnumThemePalettes.GetThemePrimary(themeName);
            Color secondary = MagnumThemePalettes.GetThemeSecondary(themeName);
            
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(radius, radius);
                Color color = Color.Lerp(primary, secondary, Main.rand.NextFloat());
                CustomParticles.GenericFlare(position + offset, color * 0.5f, 0.15f, 30);
            }
        }
        
        /// <summary>
        /// Creates a shockwave ring effect.
        /// </summary>
        public static void ShockwaveRing(Vector2 position, Color color, float scale, int lifetime)
        {
            CustomParticles.HaloRing(position, color, scale, lifetime);
        }
        
        /// <summary>
        /// Creates a radial burst effect.
        /// </summary>
        public static void RadialBurst(Vector2 position, Color primaryColor, Color secondaryColor, int count, float speed, float scale)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                Vector2 vel = angle.ToRotationVector2() * speed;
                Color color = Color.Lerp(primaryColor, secondaryColor, (float)i / count);
                CustomParticles.GenericFlare(position + vel * 10f, color, scale, 20);
            }
        }
        
        #endregion
    }
}
