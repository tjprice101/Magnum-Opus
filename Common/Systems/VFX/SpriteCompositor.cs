using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using ReLogic.Content;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// Multi-layered sprite composition system for Calamity-style boss rendering.
    /// 
    /// Calamity bosses are rarely single sprites - they're "assembled" during the draw call:
    /// 1. Base Layer: The standard texture
    /// 2. Glowmask Layer: Drawn with Color.White to ignore environmental lighting
    /// 3. VFX Layer: Semi-transparent, often distorted by sine-wave for "breathing" effects
    /// 4. Bloom Layer: Additive glow around the sprite
    /// 
    /// This system handles all layers automatically with configurable effects.
    /// </summary>
    public class SpriteCompositor
    {
        #region Layer Configuration

        /// <summary>
        /// Configuration for a single draw layer.
        /// </summary>
        public class LayerConfig
        {
            public Texture2D Texture;
            public Color Color = Color.White;
            public float Scale = 1f;
            public float Rotation = 0f;
            public Vector2 Origin = Vector2.Zero;
            public Vector2 Offset = Vector2.Zero;
            public bool IgnoreLighting = false;
            public BlendState BlendState = BlendState.AlphaBlend;
            public float Opacity = 1f;
            public SpriteEffects Effects = SpriteEffects.None;
            public Rectangle? SourceRect = null;

            // Animation settings
            public bool UsePulse = false;
            public float PulseSpeed = 3f;
            public float PulseAmplitude = 0.1f;
            
            public bool UseWave = false;
            public float WaveSpeed = 2f;
            public float WaveAmplitude = 3f;
            
            public bool UseRotate = false;
            public float RotateSpeed = 1f;
            
            public bool UseFade = false;
            public float FadeSpeed = 2f;
            public float FadeMin = 0.3f;
            public float FadeMax = 1f;
        }

        private LayerConfig[] layers;
        private int layerCount;
        private const int MaxLayers = 8;

        #endregion

        #region Preset Configurations

        /// <summary>
        /// Standard boss layer preset: Base + Glowmask + Breathing VFX
        /// </summary>
        public static SpriteCompositor CreateBossPreset(Texture2D baseTexture, Texture2D glowmask = null)
        {
            var compositor = new SpriteCompositor();

            // Base layer - normal sprite
            compositor.AddLayer(new LayerConfig
            {
                Texture = baseTexture,
                Color = Color.White,
                IgnoreLighting = false
            });

            // Glowmask layer - ignores lighting
            if (glowmask != null)
            {
                compositor.AddLayer(new LayerConfig
                {
                    Texture = glowmask,
                    Color = Color.White,
                    IgnoreLighting = true,
                    Opacity = 1f
                });
            }

            // Breathing VFX layer - subtle pulse effect
            compositor.AddLayer(new LayerConfig
            {
                Texture = baseTexture,
                Color = Color.White * 0.3f,
                BlendState = BlendState.Additive,
                UsePulse = true,
                PulseSpeed = 2f,
                PulseAmplitude = 0.15f,
                Scale = 1.05f
            });

            return compositor;
        }

        /// <summary>
        /// Intense boss preset with bloom layers.
        /// </summary>
        public static SpriteCompositor CreateIntenseBossPreset(Texture2D baseTexture, Texture2D glowmask, Color glowColor)
        {
            var compositor = CreateBossPreset(baseTexture, glowmask);

            // Outer bloom layer
            compositor.AddLayer(new LayerConfig
            {
                Texture = glowmask ?? baseTexture,
                Color = glowColor * 0.3f,
                BlendState = BlendState.Additive,
                Scale = 1.4f,
                UsePulse = true,
                PulseSpeed = 3f,
                PulseAmplitude = 0.2f,
                IgnoreLighting = true
            });

            // Inner bloom layer
            compositor.AddLayer(new LayerConfig
            {
                Texture = glowmask ?? baseTexture,
                Color = glowColor * 0.5f,
                BlendState = BlendState.Additive,
                Scale = 1.15f,
                UsePulse = true,
                PulseSpeed = 3f,
                PulseAmplitude = 0.1f,
                IgnoreLighting = true
            });

            return compositor;
        }

        /// <summary>
        /// Ethereal preset with wave distortion.
        /// </summary>
        public static SpriteCompositor CreateEtherealPreset(Texture2D baseTexture, Color etherealColor)
        {
            var compositor = new SpriteCompositor();

            // Base layer
            compositor.AddLayer(new LayerConfig
            {
                Texture = baseTexture,
                Color = Color.White,
                IgnoreLighting = false
            });

            // Ethereal wave layer
            compositor.AddLayer(new LayerConfig
            {
                Texture = baseTexture,
                Color = etherealColor * 0.4f,
                BlendState = BlendState.Additive,
                UseWave = true,
                WaveSpeed = 2f,
                WaveAmplitude = 4f,
                UseFade = true,
                FadeSpeed = 3f,
                FadeMin = 0.2f,
                FadeMax = 0.6f,
                IgnoreLighting = true
            });

            // Outer glow
            compositor.AddLayer(new LayerConfig
            {
                Texture = baseTexture,
                Color = etherealColor * 0.2f,
                BlendState = BlendState.Additive,
                Scale = 1.3f,
                UsePulse = true,
                PulseSpeed = 1.5f,
                PulseAmplitude = 0.1f,
                IgnoreLighting = true
            });

            return compositor;
        }

        /// <summary>
        /// Spinning energy aura preset.
        /// </summary>
        public static SpriteCompositor CreateSpinningAuraPreset(Texture2D baseTexture, Texture2D auraTexture, Color auraColor)
        {
            var compositor = new SpriteCompositor();

            // Spinning outer aura
            compositor.AddLayer(new LayerConfig
            {
                Texture = auraTexture ?? baseTexture,
                Color = auraColor * 0.2f,
                BlendState = BlendState.Additive,
                Scale = 1.8f,
                UseRotate = true,
                RotateSpeed = -0.5f,
                UsePulse = true,
                PulseSpeed = 2f,
                PulseAmplitude = 0.3f,
                IgnoreLighting = true
            });

            // Spinning inner aura (opposite direction)
            compositor.AddLayer(new LayerConfig
            {
                Texture = auraTexture ?? baseTexture,
                Color = auraColor * 0.4f,
                BlendState = BlendState.Additive,
                Scale = 1.3f,
                UseRotate = true,
                RotateSpeed = 1f,
                UsePulse = true,
                PulseSpeed = 3f,
                PulseAmplitude = 0.15f,
                IgnoreLighting = true
            });

            // Base layer
            compositor.AddLayer(new LayerConfig
            {
                Texture = baseTexture,
                Color = Color.White,
                IgnoreLighting = false
            });

            return compositor;
        }

        #endregion

        #region Constructor and Management

        public SpriteCompositor()
        {
            layers = new LayerConfig[MaxLayers];
            layerCount = 0;
        }

        /// <summary>
        /// Adds a new layer to the compositor.
        /// Layers are drawn in the order they are added.
        /// </summary>
        public void AddLayer(LayerConfig layer)
        {
            if (layerCount >= MaxLayers) return;
            layers[layerCount++] = layer;
        }

        /// <summary>
        /// Clears all layers.
        /// </summary>
        public void ClearLayers()
        {
            layerCount = 0;
            for (int i = 0; i < MaxLayers; i++)
                layers[i] = null;
        }

        /// <summary>
        /// Gets a layer for modification.
        /// </summary>
        public LayerConfig GetLayer(int index)
        {
            if (index < 0 || index >= layerCount) return null;
            return layers[index];
        }

        #endregion

        #region Drawing

        /// <summary>
        /// Draws all layers at the specified position.
        /// Handles all animation and blending automatically.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, Vector2 position, float baseRotation = 0f, 
            float baseScale = 1f, Color? environmentColor = null)
        {
            float time = Main.GlobalTimeWrappedHourly;
            Color envColor = environmentColor ?? Lighting.GetColor((int)(position.X / 16f), (int)(position.Y / 16f));

            BlendState currentBlend = null;
            bool needsReset = false;

            for (int i = 0; i < layerCount; i++)
            {
                var layer = layers[i];
                if (layer == null || layer.Texture == null) continue;

                // Calculate animated values
                float animatedScale = baseScale * layer.Scale;
                float animatedRotation = baseRotation + layer.Rotation;
                float animatedOpacity = layer.Opacity;
                Vector2 animatedOffset = layer.Offset;

                // Apply pulse animation
                if (layer.UsePulse)
                {
                    float pulse = (float)Math.Sin(time * layer.PulseSpeed) * layer.PulseAmplitude;
                    animatedScale *= (1f + pulse);
                }

                // Apply wave animation (position offset)
                if (layer.UseWave)
                {
                    float waveX = (float)Math.Sin(time * layer.WaveSpeed) * layer.WaveAmplitude;
                    float waveY = (float)Math.Cos(time * layer.WaveSpeed * 1.3f) * layer.WaveAmplitude * 0.5f;
                    animatedOffset += new Vector2(waveX, waveY);
                }

                // Apply rotation animation
                if (layer.UseRotate)
                {
                    animatedRotation += time * layer.RotateSpeed;
                }

                // Apply fade animation
                if (layer.UseFade)
                {
                    float fade = VFXUtilities.Sin01(time * layer.FadeSpeed);
                    animatedOpacity *= MathHelper.Lerp(layer.FadeMin, layer.FadeMax, fade);
                }

                // Handle blend state changes
                if (layer.BlendState != currentBlend)
                {
                    if (needsReset)
                    {
                        spriteBatch.End();
                    }

                    currentBlend = layer.BlendState;
                    spriteBatch.Begin(SpriteSortMode.Deferred, layer.BlendState, SamplerState.LinearClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                    needsReset = true;
                }

                // Determine color
                Color drawColor;
                if (layer.IgnoreLighting)
                {
                    drawColor = layer.Color * animatedOpacity;
                }
                else
                {
                    drawColor = layer.Color.MultiplyRGB(envColor.R / 255f) * animatedOpacity;
                }

                // For additive blending, remove alpha channel
                if (layer.BlendState == BlendState.Additive)
                {
                    drawColor = new Color(drawColor.R, drawColor.G, drawColor.B, 0);
                }

                // Calculate origin
                Vector2 origin = layer.Origin;
                if (origin == Vector2.Zero)
                {
                    origin = new Vector2(layer.Texture.Width / 2f, layer.Texture.Height / 2f);
                }

                // Draw
                Vector2 drawPos = position + animatedOffset;
                spriteBatch.Draw(layer.Texture, drawPos, layer.SourceRect, drawColor,
                    animatedRotation, origin, animatedScale, layer.Effects, 0f);
            }

            // Reset to default blend state
            if (needsReset && currentBlend != BlendState.AlphaBlend)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }

        /// <summary>
        /// Draws for NPC with interpolated position.
        /// </summary>
        public void DrawForNPC(SpriteBatch spriteBatch, NPC npc, Color? environmentColor = null)
        {
            Vector2 drawPos = InterpolatedRenderer.GetInterpolatedDrawPos(npc);
            Draw(spriteBatch, drawPos, npc.rotation, npc.scale, environmentColor);
        }

        /// <summary>
        /// Draws for Projectile with interpolated position.
        /// </summary>
        public void DrawForProjectile(SpriteBatch spriteBatch, Projectile proj, Color? environmentColor = null)
        {
            Vector2 drawPos = InterpolatedRenderer.GetInterpolatedDrawPos(proj);
            float rotation = InterpolatedRenderer.GetInterpolatedRotation(proj);
            Draw(spriteBatch, drawPos, rotation, proj.scale, environmentColor);
        }

        #endregion

        #region Simplified API

        /// <summary>
        /// Quick method to draw a boss with standard layering.
        /// </summary>
        public static void DrawBossStandard(SpriteBatch spriteBatch, NPC npc, 
            Texture2D baseTexture, Texture2D glowmask, Color glowColor, float pulseIntensity = 0.15f)
        {
            Vector2 drawPos = InterpolatedRenderer.GetInterpolatedDrawPos(npc);
            Vector2 origin = new Vector2(baseTexture.Width / 2f, baseTexture.Height / 2f);
            Color lightColor = Lighting.GetColor((int)(npc.Center.X / 16f), (int)(npc.Center.Y / 16f));
            float time = Main.GlobalTimeWrappedHourly;
            float pulse = 1f + (float)Math.Sin(time * 3f) * pulseIntensity;

            // Layer 1: Base sprite with lighting
            spriteBatch.Draw(baseTexture, drawPos, null, lightColor, npc.rotation, origin, npc.scale, 
                npc.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);

            // Layer 2: Glowmask (ignores lighting)
            if (glowmask != null)
            {
                spriteBatch.Draw(glowmask, drawPos, null, Color.White, npc.rotation, origin, npc.scale,
                    npc.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            }

            // Layer 3: Breathing glow (additive)
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Color glowDraw = new Color(glowColor.R, glowColor.G, glowColor.B, 0) * 0.4f;
            spriteBatch.Draw(glowmask ?? baseTexture, drawPos, null, glowDraw, npc.rotation, origin, 
                npc.scale * pulse, npc.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);

            // Return to normal blend state
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Quick method to add a breathing effect to any sprite.
        /// Call after drawing the main sprite.
        /// </summary>
        public static void AddBreathingEffect(SpriteBatch spriteBatch, Texture2D texture, 
            Vector2 position, Color glowColor, float rotation, Vector2 origin, float scale,
            float breathSpeed = 3f, float breathIntensity = 0.15f, float glowOpacity = 0.3f)
        {
            float time = Main.GlobalTimeWrappedHourly;
            float breath = 1f + (float)Math.Sin(time * breathSpeed) * breathIntensity;
            Color glow = new Color(glowColor.R, glowColor.G, glowColor.B, 0) * glowOpacity;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, position, null, glow, rotation, origin, scale * breath, 
                SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        #endregion
    }
}
