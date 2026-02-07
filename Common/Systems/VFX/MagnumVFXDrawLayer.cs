using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// Handles the rendering pipeline for all VFX effects in MagnumOpus.
    /// Implements proper layering and blend state management inspired by FargosSoulsDLC.
    /// 
    /// Rendering Order:
    /// 1. Behind NPCs/Projectiles (background glows, auras)
    /// 2. With NPCs/Projectiles (standard effects)
    /// 3. Above NPCs/Projectiles (bloom overlays, flares)
    /// 4. Screen-space effects (screen shake, color grading)
    /// </summary>
    public class MagnumVFXDrawLayer : ModSystem
    {
        // Queued effects for deferred rendering
        private static List<QueuedBloomDraw> _queuedBackgroundBlooms = new();
        private static List<QueuedBloomDraw> _queuedForegroundBlooms = new();
        private static bool _isDrawingVFX = false;
        
        /// <summary>
        /// Struct to hold queued bloom draw calls for batch rendering.
        /// </summary>
        private struct QueuedBloomDraw
        {
            public Vector2 WorldPosition;
            public Color PrimaryColor;
            public Color SecondaryColor;
            public float Scale;
            public float Opacity;
            public BloomDrawType DrawType;
            public float Rotation;
        }
        
        /// <summary>
        /// Types of bloom draws for the queue.
        /// </summary>
        public enum BloomDrawType
        {
            Standard,
            TwoColor,
            Simple,
            Flare,
            Theme_LaCampanella,
            Theme_Eroica,
            Theme_Moonlight,
            Theme_SwanLake,
            Theme_Enigma,
            Theme_Fate
        }
        
        public override void Load()
        {
            On_Main.DrawDust += DrawBackgroundVFX;
            On_Main.DrawProjectiles += DrawForegroundVFX;
        }
        
        public override void Unload()
        {
            On_Main.DrawDust -= DrawBackgroundVFX;
            On_Main.DrawProjectiles -= DrawForegroundVFX;
            
            _queuedBackgroundBlooms?.Clear();
            _queuedForegroundBlooms?.Clear();
            _queuedBackgroundBlooms = null;
            _queuedForegroundBlooms = null;
        }
        
        /// <summary>
        /// Draws background VFX (behind dust/projectiles).
        /// </summary>
        private void DrawBackgroundVFX(On_Main.orig_DrawDust orig, Main self)
        {
            // Draw our background VFX first
            if (_queuedBackgroundBlooms.Count > 0 && !_isDrawingVFX)
            {
                _isDrawingVFX = true;
                DrawQueuedBlooms(Main.spriteBatch, _queuedBackgroundBlooms);
                _queuedBackgroundBlooms.Clear();
                _isDrawingVFX = false;
            }
            
            // Then draw vanilla dust
            orig(self);
        }
        
        /// <summary>
        /// Draws foreground VFX (above projectiles).
        /// </summary>
        private void DrawForegroundVFX(On_Main.orig_DrawProjectiles orig, Main self)
        {
            // Draw vanilla projectiles first
            orig(self);
            
            // Then draw our foreground VFX on top
            if (_queuedForegroundBlooms.Count > 0 && !_isDrawingVFX)
            {
                _isDrawingVFX = true;
                DrawQueuedBlooms(Main.spriteBatch, _queuedForegroundBlooms);
                _queuedForegroundBlooms.Clear();
                _isDrawingVFX = false;
            }
        }
        
        /// <summary>
        /// Draws all queued bloom effects with proper blend state management.
        /// </summary>
        private void DrawQueuedBlooms(SpriteBatch spriteBatch, List<QueuedBloomDraw> queue)
        {
            if (queue.Count == 0) return;
            
            // Try to end current spritebatch state - it may not be active
            bool endedSuccessfully = false;
            try
            {
                spriteBatch.End();
                endedSuccessfully = true;
            }
            catch (System.InvalidOperationException)
            {
                // SpriteBatch wasn't active - that's okay, we'll start our own
            }
            
            try
            {
                // Begin with additive blending for blooms
                spriteBatch.Begin(
                    SpriteSortMode.Deferred,
                    BlendState.Additive,
                    SamplerState.LinearClamp,
                    DepthStencilState.None,
                    RasterizerState.CullNone,
                    null,
                    Main.GameViewMatrix.TransformationMatrix);
                
                foreach (var bloom in queue)
                {
                    DrawBloomByType(spriteBatch, bloom);
                }
                
                // End our additive batch
                spriteBatch.End();
                
                // Only restore if we successfully ended before
                if (endedSuccessfully)
                {
                    spriteBatch.Begin(
                        SpriteSortMode.Deferred,
                        BlendState.AlphaBlend,
                        Main.DefaultSamplerState,
                        DepthStencilState.None,
                        Main.Rasterizer,
                        null,
                        Main.GameViewMatrix.TransformationMatrix);
                }
            }
            catch (System.Exception)
            {
                // VFX failed - don't crash the game
                // Try to restore a valid spritebatch state if possible
                try
                {
                    if (endedSuccessfully)
                    {
                        spriteBatch.Begin(
                            SpriteSortMode.Deferred,
                            BlendState.AlphaBlend,
                            Main.DefaultSamplerState,
                            DepthStencilState.None,
                            Main.Rasterizer,
                            null,
                            Main.GameViewMatrix.TransformationMatrix);
                    }
                }
                catch { }
            }
        }
        
        /// <summary>
        /// Draws a single bloom based on its type.
        /// </summary>
        private void DrawBloomByType(SpriteBatch spriteBatch, QueuedBloomDraw bloom)
        {
            switch (bloom.DrawType)
            {
                case BloomDrawType.Standard:
                    BloomRenderer.DrawBloomStack(spriteBatch, bloom.WorldPosition, 
                        bloom.PrimaryColor, bloom.Scale, bloom.Opacity);
                    break;
                    
                case BloomDrawType.TwoColor:
                    BloomRenderer.DrawBloomStack(spriteBatch, bloom.WorldPosition,
                        bloom.PrimaryColor, bloom.SecondaryColor, bloom.Scale, bloom.Opacity);
                    break;
                    
                case BloomDrawType.Simple:
                    BloomRenderer.DrawSimpleBloom(spriteBatch, bloom.WorldPosition,
                        bloom.PrimaryColor, bloom.Scale, bloom.Opacity);
                    break;
                    
                case BloomDrawType.Flare:
                    BloomRenderer.DrawShineFlare(spriteBatch, bloom.WorldPosition,
                        bloom.PrimaryColor, bloom.Scale, bloom.Rotation, bloom.Opacity);
                    break;
                    
                case BloomDrawType.Theme_LaCampanella:
                    BloomRenderer.DrawLaCampanellaBloom(spriteBatch, bloom.WorldPosition, 
                        bloom.Scale, bloom.Opacity);
                    break;
                    
                case BloomDrawType.Theme_Eroica:
                    BloomRenderer.DrawEroicaBloom(spriteBatch, bloom.WorldPosition,
                        bloom.Scale, bloom.Opacity);
                    break;
                    
                case BloomDrawType.Theme_Moonlight:
                    BloomRenderer.DrawMoonlightBloom(spriteBatch, bloom.WorldPosition,
                        bloom.Scale, bloom.Opacity);
                    break;
                    
                case BloomDrawType.Theme_SwanLake:
                    BloomRenderer.DrawSwanLakeBloom(spriteBatch, bloom.WorldPosition,
                        bloom.Scale, bloom.Opacity);
                    break;
                    
                case BloomDrawType.Theme_Enigma:
                    BloomRenderer.DrawEnigmaBloom(spriteBatch, bloom.WorldPosition,
                        bloom.Scale, bloom.Opacity);
                    break;
                    
                case BloomDrawType.Theme_Fate:
                    BloomRenderer.DrawFateBloom(spriteBatch, bloom.WorldPosition,
                        bloom.Scale, bloom.Opacity);
                    break;
            }
        }
        
        #region Public Queue Methods
        
        /// <summary>
        /// Queues a bloom to be drawn behind projectiles/NPCs.
        /// Use for auras, background glows, and atmospheric effects.
        /// </summary>
        public static void QueueBackgroundBloom(Vector2 worldPosition, Color color, 
            float scale = 1f, float opacity = 1f)
        {
            _queuedBackgroundBlooms.Add(new QueuedBloomDraw
            {
                WorldPosition = worldPosition,
                PrimaryColor = color,
                Scale = scale,
                Opacity = opacity,
                DrawType = BloomDrawType.Standard
            });
        }
        
        /// <summary>
        /// Queues a two-color bloom to be drawn behind projectiles/NPCs.
        /// </summary>
        public static void QueueBackgroundBloom(Vector2 worldPosition, Color outerColor, 
            Color innerColor, float scale = 1f, float opacity = 1f)
        {
            _queuedBackgroundBlooms.Add(new QueuedBloomDraw
            {
                WorldPosition = worldPosition,
                PrimaryColor = outerColor,
                SecondaryColor = innerColor,
                Scale = scale,
                Opacity = opacity,
                DrawType = BloomDrawType.TwoColor
            });
        }
        
        /// <summary>
        /// Queues a bloom to be drawn above projectiles/NPCs.
        /// Use for impact flashes, highlights, and prominent effects.
        /// </summary>
        public static void QueueForegroundBloom(Vector2 worldPosition, Color color,
            float scale = 1f, float opacity = 1f)
        {
            _queuedForegroundBlooms.Add(new QueuedBloomDraw
            {
                WorldPosition = worldPosition,
                PrimaryColor = color,
                Scale = scale,
                Opacity = opacity,
                DrawType = BloomDrawType.Standard
            });
        }
        
        /// <summary>
        /// Queues a two-color bloom to be drawn above projectiles/NPCs.
        /// </summary>
        public static void QueueForegroundBloom(Vector2 worldPosition, Color outerColor,
            Color innerColor, float scale = 1f, float opacity = 1f)
        {
            _queuedForegroundBlooms.Add(new QueuedBloomDraw
            {
                WorldPosition = worldPosition,
                PrimaryColor = outerColor,
                SecondaryColor = innerColor,
                Scale = scale,
                Opacity = opacity,
                DrawType = BloomDrawType.TwoColor
            });
        }
        
        /// <summary>
        /// Queues a shine flare to be drawn in the foreground.
        /// </summary>
        public static void QueueShineFlare(Vector2 worldPosition, Color color,
            float scale = 1f, float rotation = 0f, float opacity = 1f)
        {
            _queuedForegroundBlooms.Add(new QueuedBloomDraw
            {
                WorldPosition = worldPosition,
                PrimaryColor = color,
                Scale = scale,
                Rotation = rotation,
                Opacity = opacity,
                DrawType = BloomDrawType.Flare
            });
        }
        
        /// <summary>
        /// Queues a themed bloom effect.
        /// </summary>
        public static void QueueThemedBloom(Vector2 worldPosition, string themeName,
            float scale = 1f, float opacity = 1f, bool foreground = true)
        {
            BloomDrawType drawType = themeName.ToLowerInvariant() switch
            {
                "lacampanella" or "campanella" => BloomDrawType.Theme_LaCampanella,
                "eroica" => BloomDrawType.Theme_Eroica,
                "moonlight" or "moonlightsonata" => BloomDrawType.Theme_Moonlight,
                "swanlake" or "swan" => BloomDrawType.Theme_SwanLake,
                "enigma" or "enigmavariations" => BloomDrawType.Theme_Enigma,
                "fate" => BloomDrawType.Theme_Fate,
                _ => BloomDrawType.Standard
            };
            
            var queue = foreground ? _queuedForegroundBlooms : _queuedBackgroundBlooms;
            queue.Add(new QueuedBloomDraw
            {
                WorldPosition = worldPosition,
                Scale = scale,
                Opacity = opacity,
                DrawType = drawType
            });
        }
        
        #endregion
    }
}
