using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Common.Systems.Metaballs
{
    /// <summary>
    /// Manages all metaball systems - handles render targets, drawing, and layer compositing.
    /// 
    /// This implementation follows Calamity's MetaballManager architecture:
    /// 
    /// 1. PrepareMetaballTargets (OnPrepareDraw hook):
    ///    - For each active metaball, draw particles to render targets
    ///    - Uses metaball.PrepareSpriteBatch for custom blend states
    ///    - Uses metaball.DrawInstances to draw the particles
    /// 
    /// 2. DrawMetaballs (OnDrawLayer hook):
    ///    - For each layer target, apply shader and draw to screen
    ///    - Uses metaball.PrepareShaderForTarget for shader parameters
    ///    - SpriteSortMode.Immediate for shader application
    /// 
    /// Uses MagnumMetaballShaders for shader availability.
    /// When shaders are not available, uses additive blending fallback.
    /// </summary>
    public class MetaballManager : ModSystem
    {
        /// <summary>
        /// All registered metaball instances.
        /// </summary>
        internal static readonly List<Metaball> metaballs = new();
        
        // Soft glow texture for drawing metaball instances
        private static Asset<Texture2D> circleTextureAsset;
        private static bool initialized = false;

        // Track if shaders are available - check MagnumMetaballShaders (our new shader system)
        private static bool ShadersAvailable => MagnumMetaballShaders.ShadersAvailable;

        // Store the backbuffer so we can restore it after render target operations
        private static RenderTargetBinding[] previousTargets;

        public static void RegisterMetaball(Metaball metaball)
        {
            if (!metaballs.Contains(metaball))
            {
                metaballs.Add(metaball);
            }
        }

        public override void Load()
        {
            if (Main.dedServ)
                return;

            // Hook into draw pipeline at appropriate points
            // We use DrawDust as it's after projectiles and safe
            On_Main.DrawDust += DrawMetaballsAfterDust;
            
            // Hook to prepare targets before drawing - use UpdateParticleSystems 
            // which runs early in the frame, before rendering starts
            On_Main.UpdateParticleSystems += PrepareMetaballTargets;
        }

        public override void Unload()
        {
            On_Main.DrawDust -= DrawMetaballsAfterDust;
            On_Main.UpdateParticleSystems -= PrepareMetaballTargets;

            // Dispose all metaball render targets on main thread
            Main.QueueMainThreadAction(() =>
            {
                foreach (var metaball in metaballs)
                {
                    metaball?.Dispose();
                }
            });

            metaballs.Clear();
            circleTextureAsset = null;
            initialized = false;
            previousTargets = null;
        }

        public override void PostUpdateEverything()
        {
            if (Main.dedServ)
                return;

            // Update all metaball particle systems (non-IgnoreFPS ones)
            foreach (var metaball in metaballs)
            {
                // IgnoreFPS metaballs update during draw cycle, not here
                if (!metaball.IgnoreFPS && !Main.gamePaused)
                {
                    metaball.Update();
                }
            }
        }

        public override void OnWorldUnload()
        {
            // Clear all particles when leaving world
            foreach (var metaball in metaballs)
            {
                metaball.ClearInstances();
            }
        }

        private static void EnsureInitialized()
        {
            if (initialized && circleTextureAsset?.Value != null && !circleTextureAsset.Value.IsDisposed)
                return;

            // Load soft glow texture for metaball instances
            circleTextureAsset = ModContent.Request<Texture2D>(
                "MagnumOpus/Assets/Particles/SoftGlow3", 
                AssetRequestMode.ImmediateLoad);
            
            // Shader availability is checked via MagnumShaderSystem.ShadersAvailable property
            
            initialized = true;
        }

        /// <summary>
        /// Gets the circle texture used for drawing metaball instances.
        /// </summary>
        public static Texture2D GetCircleTexture()
        {
            EnsureInitialized();
            return circleTextureAsset?.Value;
        }

        /// <summary>
        /// Checks if any metaballs of the given layer type are active.
        /// </summary>
        internal static bool AnyActiveMetaballsAtLayer(MetaballDrawLayer layerType) =>
            metaballs.Any(m => m.AnythingToDraw && m.DrawLayer == layerType);

        /// <summary>
        /// Prepares metaball render targets by drawing particles to them.
        /// Called during UpdateParticleSystems (similar to Calamity's OnPrepareDraw).
        /// This happens BEFORE the main render loop, so targets are ready for compositing.
        /// </summary>
        private void PrepareMetaballTargets(On_Main.orig_UpdateParticleSystems orig, Main self)
        {
            orig(self);

            if (Main.dedServ || Main.gameMenu)
                return;

            EnsureInitialized();

            // Get all active metaballs
            var activeMetaballs = metaballs.Where(m => m.AnythingToDraw).ToList();
            if (activeMetaballs.Count == 0)
                return;

            var gd = Main.instance.GraphicsDevice;

            foreach (var metaball in activeMetaballs)
            {
                // Update IgnoreFPS metaballs during this cycle
                if (metaball.IgnoreFPS && !Main.gamePaused)
                {
                    metaball.Update();
                }

                // Ensure render targets are valid (handles screen resize)
                metaball.EnsureTargetsValid();

                if (metaball.LayerTargets.Count == 0)
                    continue;

                // Draw to each layer target
                for (int i = 0; i < metaball.LayerTargets.Count; i++)
                {
                    var target = metaball.LayerTargets[i];
                    if (target == null || target.IsDisposed)
                        continue;

                    // Store current render target
                    previousTargets = gd.GetRenderTargets();

                    try
                    {
                        // Set our render target and clear it
                        gd.SetRenderTarget(target);
                        gd.Clear(Color.Transparent);

                        // Start sprite batch for drawing to this target
                        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, 
                            SamplerState.PointClamp, DepthStencilState.Default, 
                            RasterizerState.CullNone, null, Matrix.Identity);

                        // Offset screen position slightly to allow drawing at screen edge
                        // This is the -2,-2 offset Calamity uses for edge detection
                        var offset = new Vector2(-2, -2);
                        Main.screenPosition += offset;

                        // Let the metaball customize sprite batch (e.g., additive blending)
                        metaball.PrepareSpriteBatch(Main.spriteBatch);
                        
                        // Draw the metaball instances to this target
                        metaball.DrawInstances();

                        Main.screenPosition -= offset;

                        Main.spriteBatch.End();
                    }
                    finally
                    {
                        // CRITICAL: Always restore the render target
                        if (previousTargets != null && previousTargets.Length > 0)
                        {
                            gd.SetRenderTargets(previousTargets);
                        }
                        else
                        {
                            gd.SetRenderTarget(null);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Draws metaballs to the screen after dust drawing.
        /// </summary>
        private void DrawMetaballsAfterDust(On_Main.orig_DrawDust orig, Main self)
        {
            orig(self);

            if (Main.dedServ)
                return;

            DrawAllMetaballs();
        }

        /// <summary>
        /// Draws all active metaballs to the screen by compositing their render targets.
        /// Uses SpriteBatchScope for proper state management with FancyLighting compatibility.
        /// </summary>
        private static void DrawAllMetaballs()
        {
            EnsureInitialized();

            var metaballsToDraw = metaballs.Where(m => m.AnythingToDraw).ToList();
            if (metaballsToDraw.Count == 0)
                return;

            var spriteBatch = Main.spriteBatch;

            try
            {
                // Use SpriteBatchScope for proper state capture/restoration
                // This is the Calamity-proven pattern for FancyLighting compatibility
                using (var scope = spriteBatch.Scope())
                {
                    foreach (var metaball in metaballsToDraw)
                    {
                        try
                        {
                            if (metaball.LayerTargets.Count > 0 && metaball.LayerTargets[0] != null)
                            {
                                if (ShadersAvailable)
                                {
                                    // SHADER PATH: Use proper edge detection shader
                                    DrawMetaballWithShader(metaball, spriteBatch);
                                }
                                else
                                {
                                    // FALLBACK: Draw render targets with additive blending
                                    DrawMetaballFallback(metaball, spriteBatch);
                                }
                            }
                            else
                            {
                                // No render targets - draw directly (legacy fallback)
                                DrawMetaballDirect(metaball, spriteBatch);
                            }
                        }
                        catch (System.Exception)
                        {
                            // Individual metaball failed - try to end any open batch and continue
                            spriteBatch.TryEnd();
                        }
                    }
                }
                // SpriteBatchScope.Dispose() automatically restores original state
            }
            catch (System.Exception)
            {
                // Metaball drawing failed - scope handles cleanup
            }
        }

        /// <summary>
        /// Draws metaball using edge detection shader (when shaders are available).
        /// This is the proper Calamity-style rendering.
        /// </summary>
        private static void DrawMetaballWithShader(Metaball metaball, SpriteBatch spriteBatch)
        {
            bool begun = false;
            try
            {
                // Use Immediate sort mode for shader application
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, 
                    SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullCounterClockwise);
                begun = true;

                for (int i = 0; i < metaball.LayerTargets.Count; i++)
                {
                    var target = metaball.LayerTargets[i];
                    if (target == null || target.IsDisposed)
                        continue;

                    // Apply shader for this layer
                    var offset = new Vector2(-2, -2);
                    Main.screenPosition += offset;
                    metaball.PrepareShaderForTarget(i);
                    Main.screenPosition -= offset;

                    // Draw the render target with shader applied
                    spriteBatch.Draw(target, offset, Color.White);
                }
            }
            finally
            {
                if (begun)
                    spriteBatch.TryEnd();
            }
        }

        /// <summary>
        /// Fallback rendering using render targets but without shaders.
        /// Uses additive blending for a glow effect.
        /// NOTE: Without shaders, we draw the metaball circles directly with bright colors
        /// rather than using the render targets (which would need shader processing).
        /// </summary>
        private static void DrawMetaballFallback(Metaball metaball, SpriteBatch spriteBatch)
        {
            bool begun = false;
            try
            {
                // Without shaders, the render targets contain white circles.
                // Drawing them with EdgeColor (which is typically dark) makes them invisible.
                // Instead, draw with a bright version of the EdgeColor for visibility.
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                begun = true;

                foreach (var target in metaball.LayerTargets)
                {
                    if (target == null || target.IsDisposed)
                        continue;

                    // Use a BRIGHT color for fallback visibility
                    // The EdgeColor is typically dark for shader-based edge detection
                    // For fallback, we want the shapes to actually be visible
                    Color edgeColor = metaball.EdgeColor;
                    // Brighten the color significantly for additive rendering
                    Color tint = new Color(
                        Math.Min(255, edgeColor.R * 3 + 80),
                        Math.Min(255, edgeColor.G * 3 + 80),
                        Math.Min(255, edgeColor.B * 3 + 80),
                        0  // Alpha = 0 for proper additive blending
                    );
                    
                    spriteBatch.Draw(target, new Vector2(-2, -2), tint);
                }
            }
            finally
            {
                if (begun)
                    spriteBatch.TryEnd();
            }

            // Second pass: Draw layer textures over the shapes for detail
            DrawLayerTexturesFallback(metaball, spriteBatch);
        }

        /// <summary>
        /// Direct drawing without render targets (legacy fallback if targets aren't ready).
        /// </summary>
        private static void DrawMetaballDirect(Metaball metaball, SpriteBatch spriteBatch)
        {
            bool begun = false;
            try
            {
                // Use ADDITIVE blending so overlapping circles merge together and glow
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                begun = true;

                metaball.PrepareSpriteBatch(spriteBatch);
                metaball.DrawInstances();
            }
            finally
            {
                if (begun)
                    spriteBatch.TryEnd();
            }

            // Draw layer textures for detail
            DrawLayerTexturesFallback(metaball, spriteBatch);
        }

        /// <summary>
        /// Draws scrolling layer textures over the metaball shapes (fallback mode).
        /// </summary>
        private static void DrawLayerTexturesFallback(Metaball metaball, SpriteBatch spriteBatch)
        {
            var layers = metaball.Layers.ToList();
            if (layers.Count == 0)
                return;

            int layerIndex = 0;
            foreach (var layer in layers)
            {
                if (layer == null)
                    continue;

                // Get layer offset for scrolling effect
                Vector2 uvOffset = metaball.CalculateManualOffsetForLayer(layerIndex);
                
                if (!metaball.FixedToScreen)
                {
                    // Add world scroll offset
                    Vector2 screenSize = new Vector2(Main.screenWidth, Main.screenHeight);
                    uvOffset += Main.screenPosition / screenSize;
                }

                bool begun = false;
                try
                {
                    // Draw with additive blending at low opacity
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearWrap,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                    begun = true;

                    // Get layer color
                    Color layerColor = Color.White;
                    if (metaball.LayerColors.Count > layerIndex)
                    {
                        var v4 = metaball.LayerColors[layerIndex];
                        layerColor = new Color(v4.X, v4.Y, v4.Z, v4.W);
                    }

                    // Draw tiled layer
                    float opacity = layerIndex == 0 ? 0.15f : 0.08f;
                    DrawTiledLayer(spriteBatch, layer, uvOffset, layerColor * opacity);
                }
                finally
                {
                    if (begun)
                        spriteBatch.TryEnd();
                }

                layerIndex++;
            }
        }

        /// <summary>
        /// Draws a tiled layer texture across the screen.
        /// </summary>
        private static void DrawTiledLayer(SpriteBatch spriteBatch, Texture2D layer, Vector2 uvOffset, Color color)
        {
            // Calculate pixel offset from UV offset
            float pixelOffsetX = (uvOffset.X % 1f) * layer.Width;
            float pixelOffsetY = (uvOffset.Y % 1f) * layer.Height;

            // How many tiles needed
            int tilesX = (Main.screenWidth / layer.Width) + 3;
            int tilesY = (Main.screenHeight / layer.Height) + 3;

            for (int y = -1; y < tilesY; y++)
            {
                for (int x = -1; x < tilesX; x++)
                {
                    Vector2 pos = new Vector2(
                        x * layer.Width + pixelOffsetX,
                        y * layer.Height + pixelOffsetY
                    );

                    spriteBatch.Draw(layer, pos, color);
                }
            }
        }
    }
}
