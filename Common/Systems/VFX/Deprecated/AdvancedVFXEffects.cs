using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// Advanced VFX Effects Library - Combines shader styles with particle systems
    /// for rich, layered visual effects that use the new shader infrastructure.
    /// </summary>
    public static class AdvancedVFXEffects
    {
        #region Composite Impact Effects

        /// <summary>
        /// Creates a massive themed impact effect using advanced shaders.
        /// Combines bloom, particles, and screen distortion.
        /// </summary>
        public static void ThemedImpact(string theme, Vector2 position, float scale = 1f)
        {
            var styles = ShaderStyleRegistry.GetThemeStyles(theme);
            var palette = MagnumThemePalettes.GetThemePalette(theme);

            Color primary = palette.Length > 0 ? palette[0] : Color.White;
            Color secondary = palette.Length > 1 ? palette[1] : primary;
            Color tertiary = palette.Length > 2 ? palette[2] : secondary;

            // Layer 1: Core flash
            SpawnBloomFlare(position, primary, scale * 1.5f, styles.Bloom);

            // Layer 2: Secondary burst
            SpawnBloomFlare(position, secondary, scale * 1.2f, styles.Bloom);

            // Layer 3: Cascading rings
            for (int i = 0; i < 4; i++)
            {
                float ringScale = scale * (0.3f + i * 0.2f);
                Color ringColor = Color.Lerp(primary, secondary, i / 4f);
                SpawnExpandingRing(position, ringColor, ringScale, i * 3);
            }

            // Layer 4: Radial particle burst
            SpawnRadialBurst(position, primary, secondary, 12, scale * 4f);

            // Layer 5: Theme particles
            SpawnThemeParticles(theme, position, 8, scale * 30f);

            // Layer 6: Music notes (always - this is a music mod!)
            SpawnMusicalBurst(position, primary, 6, scale * 2f);

            // Screen effect
            TriggerScreenEffect(styles.Screen, position, primary, secondary, scale * 0.3f);

            // Lighting
            Lighting.AddLight(position, primary.ToVector3() * scale * 1.5f);
        }

        /// <summary>
        /// Creates an ethereal, soft, dreamy impact effect.
        /// </summary>
        public static void EtherealImpact(Vector2 position, Color primaryColor, Color secondaryColor, float scale = 1f)
        {
            // Soft breathing bloom layers
            for (int i = 0; i < 5; i++)
            {
                float layerScale = scale * (1.5f - i * 0.2f);
                float alpha = 0.6f - i * 0.1f;
                Vector2 offset = Main.rand.NextVector2Circular(5f, 5f);
                SpawnBloomFlare(position + offset, primaryColor * alpha, layerScale, ShaderStyleRegistry.BloomStyle.Ethereal);
            }

            // Gossamer particles drifting outward
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(1f, 3f);
                Color particleColor = Color.Lerp(primaryColor, secondaryColor, Main.rand.NextFloat());
                SpawnDriftingGlow(position, vel, particleColor, 0.4f * scale, 45);
            }

            // Soft expanding halos
            SpawnExpandingRing(position, primaryColor * 0.5f, scale * 0.4f, 0);
            SpawnExpandingRing(position, secondaryColor * 0.3f, scale * 0.6f, 5);

            // Screen ripple for ethereal feel
            TriggerScreenEffect(ShaderStyleRegistry.ScreenStyle.Ripple, position, primaryColor, secondaryColor, scale * 0.15f);
        }

        /// <summary>
        /// Creates a harsh, fiery, infernal impact effect.
        /// </summary>
        public static void InfernalImpact(Vector2 position, Color primaryColor, Color secondaryColor, float scale = 1f)
        {
            // Central explosion flash
            SpawnBloomFlare(position, Color.White, scale * 1.2f, ShaderStyleRegistry.BloomStyle.Infernal);
            SpawnBloomFlare(position, primaryColor, scale * 1f, ShaderStyleRegistry.BloomStyle.Infernal);

            // Rising embers
            for (int i = 0; i < 15; i++)
            {
                Vector2 vel = new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-8f, -2f));
                Color emberColor = Color.Lerp(primaryColor, secondaryColor, Main.rand.NextFloat());
                SpawnEmber(position + Main.rand.NextVector2Circular(20f, 10f) * scale, vel, emberColor, 0.3f * scale);
            }

            // Fire burst
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 8f);
                SpawnFireParticle(position, vel, primaryColor, 0.5f * scale);
            }

            // Smoke plume
            for (int i = 0; i < 6; i++)
            {
                Vector2 smokeVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-2f, -0.5f));
                SpawnSmoke(position + Main.rand.NextVector2Circular(15f, 15f) * scale, smokeVel, 0.4f * scale);
            }

            // Screen shatter for impact
            TriggerScreenEffect(ShaderStyleRegistry.ScreenStyle.Shatter, position, primaryColor, secondaryColor, scale * 0.25f);
        }

        /// <summary>
        /// Creates a star-like, celestial impact effect.
        /// </summary>
        public static void CelestialImpact(Vector2 position, Color primaryColor, Color secondaryColor, float scale = 1f)
        {
            // 6-point star burst
            SpawnBloomFlare(position, Color.White, scale * 1.3f, ShaderStyleRegistry.BloomStyle.Celestial);
            
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 rayPos = position + angle.ToRotationVector2() * 30f * scale;
                SpawnBloomFlare(rayPos, primaryColor, scale * 0.6f, ShaderStyleRegistry.BloomStyle.Celestial);
            }

            // Orbital ring of particles
            float orbitRadius = 45f * scale;
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 orbitPos = position + angle.ToRotationVector2() * orbitRadius;
                SpawnSparkle(orbitPos, primaryColor, 0.35f * scale);
            }

            // Twinkling star particles
            for (int i = 0; i < 10; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(40f, 40f) * scale;
                Color starColor = Color.Lerp(primaryColor, Color.White, Main.rand.NextFloat(0.5f, 1f));
                SpawnSparkle(position + offset, starColor, Main.rand.NextFloat(0.2f, 0.5f) * scale);
            }

            // Screen pulse for celestial rhythm
            TriggerScreenEffect(ShaderStyleRegistry.ScreenStyle.Pulse, position, primaryColor, secondaryColor, scale * 0.2f);
        }

        /// <summary>
        /// Creates a rainbow, chromatic, prismatic impact effect.
        /// </summary>
        public static void ChromaticImpact(Vector2 position, float scale = 1f)
        {
            // Rainbow spectrum burst
            for (int i = 0; i < 8; i++)
            {
                float hue = i / 8f;
                Color rainbowColor = Main.hslToRgb(hue, 1f, 0.6f);
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 offset = angle.ToRotationVector2() * 25f * scale;
                SpawnBloomFlare(position + offset, rainbowColor, scale * 0.7f, ShaderStyleRegistry.BloomStyle.Chromatic);
            }

            // Central white core
            SpawnBloomFlare(position, Color.White, scale * 1.2f, ShaderStyleRegistry.BloomStyle.Chromatic);

            // Prismatic particle spray
            for (int i = 0; i < 16; i++)
            {
                float hue = (i / 16f + Main.rand.NextFloat(0.05f)) % 1f;
                Color particleColor = Main.hslToRgb(hue, 1f, 0.7f);
                float angle = MathHelper.TwoPi * i / 16f + Main.rand.NextFloat(-0.1f, 0.1f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                SpawnSparkle(position, particleColor, 0.35f * scale);
            }

            // Rainbow ring expansion
            SpawnExpandingRing(position, Main.hslToRgb(Main.GameUpdateCount * 0.01f % 1f, 1f, 0.6f), scale * 0.5f, 0);
        }

        /// <summary>
        /// Creates a void, dark, event-horizon impact effect.
        /// </summary>
        public static void VoidImpact(Vector2 position, Color primaryColor, Color accentColor, float scale = 1f)
        {
            // Dark core (inverted bloom)
            SpawnBloomFlare(position, new Color(10, 5, 20), scale * 1.5f, ShaderStyleRegistry.BloomStyle.Void);
            
            // Bright edge ring
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                float radius = 35f * scale;
                Vector2 edgePos = position + angle.ToRotationVector2() * radius;
                SpawnBloomFlare(edgePos, accentColor, scale * 0.3f, ShaderStyleRegistry.BloomStyle.Void);
            }

            // Void tendrils spiraling inward
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f + Main.GameUpdateCount * 0.05f;
                for (int j = 0; j < 5; j++)
                {
                    float spiralRadius = (5 - j) * 10f * scale;
                    float spiralAngle = angle + j * 0.3f;
                    Vector2 spiralPos = position + spiralAngle.ToRotationVector2() * spiralRadius;
                    Color tendrilColor = Color.Lerp(primaryColor, accentColor, j / 5f);
                    SpawnDriftingGlow(spiralPos, (position - spiralPos) * 0.05f, tendrilColor, 0.2f * scale, 20);
                }
            }

            // Reality warp screen effect
            TriggerScreenEffect(ShaderStyleRegistry.ScreenStyle.Warp, position, primaryColor, accentColor, scale * 0.35f);
        }

        #endregion

        #region Trail Effects

        /// <summary>
        /// Creates a flame trail segment with embers.
        /// </summary>
        public static void FlameTrailSegment(Vector2 position, Vector2 velocity, Color primaryColor, Color secondaryColor, float scale = 1f)
        {
            // Main fire glow
            SpawnFireParticle(position, -velocity * 0.2f, primaryColor, scale * 0.5f);

            // Rising embers
            if (Main.rand.NextBool(3))
            {
                Vector2 emberVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-2f, -0.5f));
                SpawnEmber(position, emberVel, Color.Lerp(primaryColor, secondaryColor, Main.rand.NextFloat()), scale * 0.2f);
            }

            // Smoke trail
            if (Main.rand.NextBool(4))
            {
                SpawnSmoke(position, -velocity * 0.1f + new Vector2(0, -0.5f), scale * 0.25f);
            }
        }

        /// <summary>
        /// Creates an ice trail segment with frost particles.
        /// </summary>
        public static void IceTrailSegment(Vector2 position, Vector2 velocity, Color primaryColor, Color secondaryColor, float scale = 1f)
        {
            // Crystalline glow
            SpawnSparkle(position, Color.Lerp(primaryColor, Color.White, 0.5f), scale * 0.4f);

            // Frost crystals
            if (Main.rand.NextBool(2))
            {
                Vector2 crystalOffset = Main.rand.NextVector2Circular(10f, 10f) * scale;
                SpawnSparkle(position + crystalOffset, primaryColor, scale * 0.25f);
            }

            // Ice mist
            if (Main.rand.NextBool(3))
            {
                SpawnDriftingGlow(position, -velocity * 0.1f, secondaryColor * 0.5f, scale * 0.3f, 25);
            }
        }

        /// <summary>
        /// Creates a lightning trail segment with electric arcs.
        /// </summary>
        public static void LightningTrailSegment(Vector2 position, Vector2 velocity, Color primaryColor, float scale = 1f)
        {
            // Core electric glow
            SpawnBloomFlare(position, primaryColor, scale * 0.35f, ShaderStyleRegistry.BloomStyle.Celestial);

            // Electric sparks
            if (Main.rand.NextBool(2))
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 sparkVel = Main.rand.NextVector2Circular(5f, 5f);
                    SpawnSpark(position, sparkVel, primaryColor, scale * 0.2f);
                }
            }

            // Arc flash
            if (Main.rand.NextBool(5))
            {
                Vector2 arcEnd = position + Main.rand.NextVector2Circular(30f, 30f) * scale;
                SpawnLightningArc(position, arcEnd, primaryColor, scale * 0.15f);
            }
        }

        /// <summary>
        /// Creates a nature trail segment with leaves and pollen.
        /// </summary>
        public static void NatureTrailSegment(Vector2 position, Vector2 velocity, Color primaryColor, Color secondaryColor, float scale = 1f)
        {
            // Soft glow
            SpawnDriftingGlow(position, -velocity * 0.15f, primaryColor * 0.6f, scale * 0.35f, 30);

            // Floating pollen/petals
            if (Main.rand.NextBool(3))
            {
                Vector2 floatVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-0.5f, 0.5f));
                Color petalColor = Color.Lerp(primaryColor, secondaryColor, Main.rand.NextFloat());
                SpawnPetal(position, floatVel, petalColor, scale * 0.3f);
            }

            // Sparkle dust
            if (Main.rand.NextBool(4))
            {
                SpawnSparkle(position + Main.rand.NextVector2Circular(8f, 8f) * scale, primaryColor * 0.7f, scale * 0.2f);
            }
        }

        /// <summary>
        /// Creates a cosmic trail segment with stars and nebula.
        /// </summary>
        public static void CosmicTrailSegment(Vector2 position, Vector2 velocity, Color primaryColor, Color secondaryColor, float scale = 1f)
        {
            // Nebula glow
            SpawnDriftingGlow(position, -velocity * 0.1f, primaryColor * 0.5f, scale * 0.5f, 35);

            // Twinkling stars
            if (Main.rand.NextBool(3))
            {
                Color starColor = Color.Lerp(primaryColor, Color.White, Main.rand.NextFloat(0.5f, 1f));
                SpawnSparkle(position + Main.rand.NextVector2Circular(15f, 15f) * scale, starColor, scale * 0.25f);
            }

            // Cosmic dust
            if (Main.rand.NextBool(2))
            {
                float hue = Main.rand.NextFloat();
                Color dustColor = Color.Lerp(primaryColor, secondaryColor, hue);
                SpawnDriftingGlow(position, Main.rand.NextVector2Circular(1f, 1f), dustColor * 0.4f, scale * 0.2f, 25);
            }
        }

        #endregion

        #region Particle Spawning Helpers

        private static void SpawnBloomFlare(Vector2 position, Color color, float scale, ShaderStyleRegistry.BloomStyle style)
        {
            // Uses existing particle system with style tag for shader selection
            CustomParticles.GenericFlare(position, color, scale, 15);
        }

        private static void SpawnExpandingRing(Vector2 position, Color color, float scale, int delay)
        {
            CustomParticles.HaloRing(position, color, scale, 18 + delay);
        }

        private static void SpawnRadialBurst(Vector2 position, Color primary, Color secondary, int count, float speed)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                Vector2 vel = angle.ToRotationVector2() * speed;
                Color burstColor = Color.Lerp(primary, secondary, i / (float)count);
                var particle = new GenericGlowParticle(position, vel, burstColor, 0.35f, 25, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }
        }

        private static void SpawnThemeParticles(string theme, Vector2 position, int count, float spread)
        {
            // Spawn theme-appropriate particles
            switch (theme)
            {
                case "LaCampanella":
                    for (int i = 0; i < count; i++)
                        SpawnSmoke(position + Main.rand.NextVector2Circular(spread, spread), Main.rand.NextVector2Circular(2f, 2f), 0.3f);
                    break;
                case "Eroica":
                    ThemedParticles.SakuraPetals(position, count, spread);
                    break;
                case "SwanLake":
                    ThemedParticles.SwanFeatherBurst(position, count, spread);
                    break;
                case "MoonlightSonata":
                    ThemedParticles.MoonlightMusicNotes(position, count, spread);
                    break;
                case "Enigma":
                    CustomParticles.GlyphBurst(position, MagnumThemePalettes.EnigmaPurple, count, spread / 10f);
                    break;
                case "Fate":
                    CustomParticles.GlyphBurst(position, MagnumThemePalettes.FateDarkPink, count, spread / 10f);
                    break;
                default:
                    // Generic sparkles for unknown themes
                    for (int i = 0; i < count; i++)
                        SpawnSparkle(position + Main.rand.NextVector2Circular(spread, spread), Color.White, 0.3f);
                    break;
            }
        }

        private static void SpawnMusicalBurst(Vector2 position, Color color, int count, float speed)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                Vector2 vel = angle.ToRotationVector2() * speed;
                ThemedParticles.MusicNote(position, vel, color, 0.75f, 30);
            }
        }

        private static void SpawnDriftingGlow(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            var particle = new GenericGlowParticle(position, velocity, color, scale, lifetime, true);
            MagnumParticleHandler.SpawnParticle(particle);
        }

        private static void SpawnSparkle(Vector2 position, Color color, float scale)
        {
            var sparkle = new SparkleParticle(position, Main.rand.NextVector2Circular(1f, 1f), color, scale, 25);
            MagnumParticleHandler.SpawnParticle(sparkle);
        }

        private static void SpawnEmber(Vector2 position, Vector2 velocity, Color color, float scale)
        {
            var spark = new GlowSparkParticle(position, velocity, color, scale, 35);
            MagnumParticleHandler.SpawnParticle(spark);
        }

        private static void SpawnFireParticle(Vector2 position, Vector2 velocity, Color color, float scale)
        {
            var glow = new GenericGlowParticle(position, velocity + new Vector2(0, -1f), color, scale, 20, true);
            MagnumParticleHandler.SpawnParticle(glow);
        }

        private static void SpawnSmoke(Vector2 position, Vector2 velocity, float scale)
        {
            var smoke = new HeavySmokeParticle(position, velocity, Color.DarkGray, 30, scale, 0.6f, 0.02f, false);
            MagnumParticleHandler.SpawnParticle(smoke);
        }

        private static void SpawnSpark(Vector2 position, Vector2 velocity, Color color, float scale)
        {
            var spark = new GlowSparkParticle(position, velocity, color, scale, 15);
            MagnumParticleHandler.SpawnParticle(spark);
        }

        private static void SpawnPetal(Vector2 position, Vector2 velocity, Color color, float scale)
        {
            var petal = new GenericGlowParticle(position, velocity, color, scale, 40, true);
            MagnumParticleHandler.SpawnParticle(petal);
        }

        private static void SpawnLightningArc(Vector2 start, Vector2 end, Color color, float scale)
        {
            // Draw lightning using MagnumVFX
            MagnumVFX.DrawLaCampanellaLightning(start, end, 6, 8f * scale, 2, 0.4f);
        }

        private static void TriggerScreenEffect(ShaderStyleRegistry.ScreenStyle style, Vector2 position, Color primary, Color secondary, float intensity)
        {
            if (style == ShaderStyleRegistry.ScreenStyle.None) return;
            
            // Add to active screen effects queue
            ActiveScreenEffects.Add(new ScreenEffectInstance
            {
                Style = style,
                Position = position,
                Primary = primary,
                Secondary = secondary,
                Intensity = intensity,
                Progress = 0f,
                Duration = 30
            });
        }

        #endregion

        #region Active Screen Effects

        private static List<ScreenEffectInstance> ActiveScreenEffects = new();

        private struct ScreenEffectInstance
        {
            public ShaderStyleRegistry.ScreenStyle Style;
            public Vector2 Position;
            public Color Primary;
            public Color Secondary;
            public float Intensity;
            public float Progress;
            public int Duration;
            public int Timer;
        }

        /// <summary>
        /// Updates all active screen effects. Call this from a ModSystem.PostUpdateEverything or similar.
        /// </summary>
        public static void UpdateScreenEffects()
        {
            for (int i = ActiveScreenEffects.Count - 1; i >= 0; i--)
            {
                var effect = ActiveScreenEffects[i];
                effect.Timer++;
                effect.Progress = effect.Timer / (float)effect.Duration;

                if (effect.Timer >= effect.Duration)
                {
                    ActiveScreenEffects.RemoveAt(i);
                }
                else
                {
                    ActiveScreenEffects[i] = effect;
                }
            }
        }

        /// <summary>
        /// Applies the current active screen effects. Call from ModSystem.PostDrawTiles or similar.
        /// </summary>
        public static void ApplyActiveScreenEffects(SpriteBatch spriteBatch)
        {
            foreach (var effect in ActiveScreenEffects)
            {
                float fadeIntensity = effect.Intensity * (1f - effect.Progress);
                ShaderStyleRegistry.ApplyScreenStyle(
                    effect.Style,
                    effect.Position,
                    effect.Primary,
                    effect.Secondary,
                    fadeIntensity,
                    effect.Progress
                );
            }
        }

        #endregion
    }
}
