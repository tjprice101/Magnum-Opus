using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;

namespace MagnumOpus.Common.Systems.VFX.Bloom
{
    /// <summary>
    /// Multi-layer glow rendering system.
    /// 
    /// Layered Glow Architecture:
    /// Layer 4: Screen Glow (largest, 0.1x alpha, 4x size)
    /// Layer 3: Outer Glow (large, 0.3x alpha, 2x size)
    /// Layer 2: Mid Glow (medium, 0.6x alpha, 1.5x size)
    /// Layer 1: Core (smallest, 1.0x alpha, 1x size)
    /// 
    /// Why Multiple Layers:
    /// - Creates depth and volume
    /// - Simulates light scattering
    /// - Prevents harsh edges
    /// - More control over intensity falloff
    /// </summary>
    public class GlowRenderer
    {
        private static Texture2D _glowTexture;
        private static Dictionary<string, float[]> _layerRotations = new Dictionary<string, float[]>();

        /// <summary>
        /// Glow layer definition.
        /// </summary>
        public struct GlowLayer
        {
            public float Scale;
            public float Alpha;
            public Color Tint;
            public float RotationSpeed;
            public BlendState BlendMode;

            public GlowLayer(float scale, float alpha, Color tint,
                float rotationSpeed = 0f, BlendState blendMode = null)
            {
                Scale = scale;
                Alpha = alpha;
                Tint = tint;
                RotationSpeed = rotationSpeed;
                BlendMode = blendMode ?? BlendState.Additive;
            }
        }

        #region Predefined Glow Profiles

        /// <summary>
        /// Soft, diffuse glow - good for fire, magic, ambient effects.
        /// </summary>
        public static readonly GlowLayer[] SoftProfile = new[]
        {
            new GlowLayer(1.0f, 1.0f, Color.White),
            new GlowLayer(1.8f, 0.6f, Color.White),
            new GlowLayer(3.0f, 0.3f, Color.White),
            new GlowLayer(5.0f, 0.1f, Color.White)
        };

        /// <summary>
        /// Sharp, intense glow - good for lasers, electricity, focused energy.
        /// </summary>
        public static readonly GlowLayer[] SharpProfile = new[]
        {
            new GlowLayer(1.0f, 1.0f, Color.White),
            new GlowLayer(1.3f, 0.8f, Color.White),
            new GlowLayer(1.8f, 0.4f, Color.White),
            new GlowLayer(2.5f, 0.15f, Color.White)
        };

        /// <summary>
        /// Energy beam glow with rotating layers - good for charged attacks.
        /// </summary>
        public static readonly GlowLayer[] EnergyBeamProfile = new[]
        {
            new GlowLayer(1.0f, 1.0f, Color.White, 0f),
            new GlowLayer(1.5f, 0.7f, new Color(100, 200, 255), 0.03f),
            new GlowLayer(2.2f, 0.4f, new Color(50, 150, 255), -0.05f),
            new GlowLayer(3.5f, 0.2f, new Color(0, 100, 255), 0.02f)
        };

        /// <summary>
        /// Explosion glow with color progression - good for impacts.
        /// </summary>
        public static readonly GlowLayer[] ExplosionProfile = new[]
        {
            new GlowLayer(1.0f, 1.0f, Color.White),
            new GlowLayer(1.8f, 0.8f, Color.Orange),
            new GlowLayer(2.8f, 0.5f, Color.OrangeRed),
            new GlowLayer(4.5f, 0.2f, Color.Red)
        };

        /// <summary>
        /// Cosmic glow for celestial effects - good for Fate theme.
        /// </summary>
        public static readonly GlowLayer[] CosmicProfile = new[]
        {
            new GlowLayer(1.0f, 1.0f, Color.White),
            new GlowLayer(1.5f, 0.7f, new Color(200, 100, 150)),
            new GlowLayer(2.5f, 0.4f, new Color(150, 50, 100)),
            new GlowLayer(4.0f, 0.2f, new Color(80, 20, 60))
        };

        /// <summary>
        /// Infernal glow for fire effects - good for La Campanella theme.
        /// </summary>
        public static readonly GlowLayer[] InfernalProfile = new[]
        {
            new GlowLayer(1.0f, 1.0f, Color.White),
            new GlowLayer(1.6f, 0.75f, new Color(255, 200, 100)),
            new GlowLayer(2.4f, 0.5f, new Color(255, 140, 40)),
            new GlowLayer(4.0f, 0.25f, new Color(200, 50, 30))
        };

        /// <summary>
        /// Heroic glow - good for Eroica theme.
        /// </summary>
        public static readonly GlowLayer[] HeroicProfile = new[]
        {
            new GlowLayer(1.0f, 1.0f, Color.White),
            new GlowLayer(1.5f, 0.7f, new Color(255, 215, 0)),
            new GlowLayer(2.2f, 0.45f, new Color(200, 50, 50)),
            new GlowLayer(3.5f, 0.2f, new Color(139, 0, 0))
        };

        #endregion

        #region Core Rendering Methods

        /// <summary>
        /// Draw multi-layered glow effect at position.
        /// </summary>
        /// <param name="spriteBatch">Active SpriteBatch</param>
        /// <param name="position">World position</param>
        /// <param name="layers">Glow layer configuration</param>
        /// <param name="baseColor">Base tint color</param>
        /// <param name="intensity">Overall intensity multiplier (0-1+)</param>
        /// <param name="id">Unique ID for rotation tracking (optional)</param>
        public static void DrawGlow(SpriteBatch spriteBatch, Vector2 position, GlowLayer[] layers,
            Color baseColor, float intensity = 1f, string id = null)
        {
            EnsureGlowTexture();

            // Track rotations per ID
            float[] rotations = null;
            if (id != null)
            {
                if (!_layerRotations.TryGetValue(id, out rotations) || rotations.Length != layers.Length)
                {
                    rotations = new float[layers.Length];
                    _layerRotations[id] = rotations;
                }
            }

            Vector2 drawPos = position - Main.screenPosition;
            Vector2 origin = new Vector2(_glowTexture.Width, _glowTexture.Height) * 0.5f;

            // Draw each layer back to front
            for (int i = layers.Length - 1; i >= 0; i--)
            {
                var layer = layers[i];

                // Update rotation
                float rotation = 0f;
                if (rotations != null)
                {
                    rotations[i] += layer.RotationSpeed;
                    rotation = rotations[i];
                }

                // Calculate layer color
                Color layerColor = MultiplyColors(baseColor, layer.Tint);
                layerColor = layerColor with { A = 0 }; // Remove alpha for additive
                float alpha = layer.Alpha * intensity;

                spriteBatch.Draw(
                    _glowTexture,
                    drawPos,
                    null,
                    layerColor * alpha,
                    rotation,
                    origin,
                    layer.Scale,
                    SpriteEffects.None,
                    0f
                );
            }
        }

        /// <summary>
        /// Draw glow with automatic SpriteBatch state management.
        /// Switches to additive blending, then restores.
        /// </summary>
        public static void DrawGlowManaged(SpriteBatch spriteBatch, Vector2 position, 
            GlowLayer[] layers, Color baseColor, float intensity = 1f, string id = null)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);

            DrawGlow(spriteBatch, position, layers, baseColor, intensity, id);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Draw simple 4-layer bloom (quick API).
        /// </summary>
        public static void DrawSimpleGlow(SpriteBatch spriteBatch, Vector2 position,
            Color color, float scale = 1f, float intensity = 1f)
        {
            DrawGlow(spriteBatch, position, SoftProfile, color, intensity * scale);
        }

        /// <summary>
        /// Draw glow with pulsing animation.
        /// </summary>
        public static void DrawPulsingGlow(SpriteBatch spriteBatch, Vector2 position,
            GlowLayer[] layers, Color baseColor, float pulseSpeed = 2f, float pulseAmount = 0.3f,
            string id = null)
        {
            float pulse = (float)Math.Sin(Main.GlobalTimeWrappedHourly * pulseSpeed * MathHelper.TwoPi);
            pulse = pulse * 0.5f + 0.5f; // 0-1 range
            float intensity = 1f - pulseAmount + pulse * pulseAmount * 2f;

            DrawGlow(spriteBatch, position, layers, baseColor, intensity, id);
        }

        #endregion

        #region Specialized Glow Effects

        /// <summary>
        /// Draw charge-up glow that builds over time.
        /// </summary>
        /// <param name="chargeProgress">0 = no charge, 1 = fully charged</param>
        public static void DrawChargeGlow(SpriteBatch spriteBatch, Vector2 position,
            Color baseColor, float chargeProgress, float maxScale = 2f)
        {
            // Scale grows with charge
            float scale = 0.2f + chargeProgress * (maxScale - 0.2f);

            // Intensity also grows
            float intensity = 0.3f + chargeProgress * 0.7f;

            // Add jitter near full charge
            if (chargeProgress > 0.8f)
            {
                float jitter = (chargeProgress - 0.8f) / 0.2f * 3f;
                position += Main.rand.NextVector2Circular(jitter, jitter);
            }

            var layers = new[]
            {
                new GlowLayer(scale * 0.5f, 1.0f, Color.White),
                new GlowLayer(scale * 0.8f, 0.6f, baseColor),
                new GlowLayer(scale * 1.2f, 0.3f, baseColor),
                new GlowLayer(scale * 2.0f, 0.1f, baseColor)
            };

            DrawGlow(spriteBatch, position, layers, baseColor, intensity);
        }

        /// <summary>
        /// Draw fade-out glow effect.
        /// </summary>
        /// <param name="fadeProgress">0 = full, 1 = faded out</param>
        public static void DrawFadingGlow(SpriteBatch spriteBatch, Vector2 position,
            Color baseColor, float fadeProgress, float startScale = 1f)
        {
            // Scale expands as it fades
            float scale = startScale + VFXUtilities.EaseOut(fadeProgress, 3f) * 1.5f;

            // Intensity fades
            float intensity = 1f - VFXUtilities.EaseIn(fadeProgress, 2f);

            var layers = new[]
            {
                new GlowLayer(scale * 1.0f, 1.0f, Color.White),
                new GlowLayer(scale * 1.5f, 0.5f, baseColor),
                new GlowLayer(scale * 2.5f, 0.2f, baseColor)
            };

            DrawGlow(spriteBatch, position, layers, baseColor, intensity);
        }

        /// <summary>
        /// Draw impact flash glow.
        /// </summary>
        public static void DrawImpactGlow(SpriteBatch spriteBatch, Vector2 position,
            Color primaryColor, Color secondaryColor, float progress)
        {
            // Sharp flash that quickly fades
            float flashIntensity = 1f - (float)Math.Pow(progress, 0.5f);
            float expandScale = 0.5f + progress * 2f;

            var layers = new[]
            {
                new GlowLayer(expandScale * 0.5f, flashIntensity, Color.White),
                new GlowLayer(expandScale * 1.0f, flashIntensity * 0.6f, primaryColor),
                new GlowLayer(expandScale * 2.0f, flashIntensity * 0.3f, secondaryColor),
                new GlowLayer(expandScale * 4.0f, flashIntensity * 0.1f, secondaryColor)
            };

            DrawGlow(spriteBatch, position, layers, Color.White, 1f);
        }

        #endregion

        #region Theme-Specific Glows

        /// <summary>
        /// Draw La Campanella themed glow (infernal bell).
        /// </summary>
        public static void DrawLaCampanellaGlow(SpriteBatch spriteBatch, Vector2 position,
            float scale = 1f, float intensity = 1f)
        {
            DrawGlow(spriteBatch, position, InfernalProfile, new Color(255, 140, 40), intensity * scale);
        }

        /// <summary>
        /// Draw Eroica themed glow (heroic gold/scarlet).
        /// </summary>
        public static void DrawEroicaGlow(SpriteBatch spriteBatch, Vector2 position,
            float scale = 1f, float intensity = 1f)
        {
            DrawGlow(spriteBatch, position, HeroicProfile, new Color(255, 200, 80), intensity * scale);
        }

        /// <summary>
        /// Draw Fate themed glow (cosmic dark prismatic).
        /// </summary>
        public static void DrawFateGlow(SpriteBatch spriteBatch, Vector2 position,
            float scale = 1f, float intensity = 1f)
        {
            DrawGlow(spriteBatch, position, CosmicProfile, new Color(180, 50, 100), intensity * scale);
        }

        /// <summary>
        /// Draw Moonlight Sonata themed glow (lunar purple/blue).
        /// </summary>
        public static void DrawMoonlightGlow(SpriteBatch spriteBatch, Vector2 position,
            float scale = 1f, float intensity = 1f)
        {
            var moonlightProfile = new[]
            {
                new GlowLayer(1.0f, 1.0f, Color.White),
                new GlowLayer(1.6f, 0.65f, new Color(135, 206, 250)),
                new GlowLayer(2.5f, 0.4f, new Color(138, 43, 226)),
                new GlowLayer(4.0f, 0.2f, new Color(75, 0, 130))
            };

            DrawGlow(spriteBatch, position, moonlightProfile, new Color(138, 43, 226), intensity * scale);
        }

        /// <summary>
        /// Draw Swan Lake themed glow (prismatic white/rainbow).
        /// </summary>
        public static void DrawSwanLakeGlow(SpriteBatch spriteBatch, Vector2 position,
            float scale = 1f, float intensity = 1f)
        {
            // Rainbow shimmer effect
            float hue = (Main.GlobalTimeWrappedHourly * 0.5f) % 1f;
            Color rainbow = Main.hslToRgb(hue, 0.8f, 0.85f);

            var swanProfile = new[]
            {
                new GlowLayer(1.0f, 1.0f, Color.White),
                new GlowLayer(1.5f, 0.6f, Color.White),
                new GlowLayer(2.5f, 0.35f, rainbow),
                new GlowLayer(4.0f, 0.15f, rainbow)
            };

            DrawGlow(spriteBatch, position, swanProfile, Color.White, intensity * scale);
        }

        /// <summary>
        /// Draw Enigma themed glow (void purple/green).
        /// </summary>
        public static void DrawEnigmaGlow(SpriteBatch spriteBatch, Vector2 position,
            float scale = 1f, float intensity = 1f)
        {
            var enigmaProfile = new[]
            {
                new GlowLayer(1.0f, 1.0f, Color.White),
                new GlowLayer(1.6f, 0.7f, new Color(50, 220, 100)),
                new GlowLayer(2.4f, 0.45f, new Color(140, 60, 200)),
                new GlowLayer(4.0f, 0.2f, new Color(15, 10, 20))
            };

            DrawGlow(spriteBatch, position, enigmaProfile, new Color(140, 60, 200), intensity * scale);
        }

        #endregion

        #region Utility Methods

        private static void EnsureGlowTexture()
        {
            if (_glowTexture != null && !_glowTexture.IsDisposed) return;

            var device = Main.graphics.GraphicsDevice;
            int size = 256;
            _glowTexture = new Texture2D(device, size, size);
            Color[] data = new Color[size * size];

            Vector2 center = new Vector2(size * 0.5f);
            float radius = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 pos = new Vector2(x, y);
                    float dist = Vector2.Distance(pos, center);
                    float t = dist / radius;
                    float alpha = 1f - MathHelper.Clamp(t, 0f, 1f);
                    alpha = (float)Math.Pow(alpha, 1.5f);
                    data[y * size + x] = Color.White * alpha;
                }
            }

            _glowTexture.SetData(data);
        }

        private static Color MultiplyColors(Color a, Color b)
        {
            return new Color(
                (byte)(a.R * b.R / 255),
                (byte)(a.G * b.G / 255),
                (byte)(a.B * b.B / 255),
                (byte)(a.A * b.A / 255)
            );
        }

        /// <summary>
        /// Clear rotation tracking for a specific ID.
        /// </summary>
        public static void ClearRotations(string id)
        {
            _layerRotations.Remove(id);
        }

        /// <summary>
        /// Clear all rotation tracking.
        /// </summary>
        public static void ClearAllRotations()
        {
            _layerRotations.Clear();
        }

        #endregion
    }
}
