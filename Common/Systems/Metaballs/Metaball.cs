using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.Metaballs
{
    /// <summary>
    /// Defines when in the draw pipeline a metaball should render.
    /// Matches Calamity's GeneralDrawLayer enum for compatibility.
    /// </summary>
    public enum MetaballDrawLayer
    {
        BeforeNPCs,
        AfterNPCs,
        BeforeProjectiles,
        AfterProjectiles
    }

    /// <summary>
    /// Base class for all metaball systems. Metaballs are blob-like particles that merge together
    /// when overlapping, creating smooth organic shapes. They use render targets and shaders to
    /// achieve the merging effect.
    /// 
    /// This implementation closely follows Calamity Mod's metaball architecture:
    /// - LayerTargets: List of render targets for each texture layer
    /// - PrepareSpriteBatch: Custom blend states for drawing instances
    /// - PrepareShaderForTarget: Custom shader parameters per layer
    /// - DrawInstances: Draw particles to render target
    /// 
    /// When shaders are not available, falls back to simple additive blending.
    /// </summary>
    public abstract class Metaball : ModType
    {
        /// <summary>
        /// Render targets for each texture layer. Created in Register().
        /// One target per layer allows shader processing of each layer independently.
        /// </summary>
        internal List<RenderTarget2D> LayerTargets { get; private set; } = new();

        /// <summary>
        /// Whether this metaball system has anything to draw this frame.
        /// </summary>
        public abstract bool AnythingToDraw { get; }

        /// <summary>
        /// The texture layers to apply to the merged metaball shape.
        /// These scroll/animate to create the cosmic energy effect.
        /// </summary>
        public abstract IEnumerable<Texture2D> Layers { get; }

        /// <summary>
        /// When in the draw pipeline this metaball renders.
        /// </summary>
        public abstract MetaballDrawLayer DrawLayer { get; }

        /// <summary>
        /// The color of the edge/outline of the metaball blobs.
        /// Use "with { A = 0 }" for additive blending compatibility.
        /// </summary>
        public abstract Color EdgeColor { get; }

        /// <summary>
        /// Colors for each layer. Defaults to white for all layers.
        /// </summary>
        public virtual List<Vector4> LayerColors => new();

        /// <summary>
        /// If true, metaball updates even when game is paused (for menu effects).
        /// </summary>
        public virtual bool IgnoreFPS => false;

        /// <summary>
        /// If true, layer textures are fixed to screen rather than scrolling with world.
        /// </summary>
        public virtual bool FixedToScreen => false;

        /// <summary>
        /// Called every frame to update particle positions, velocities, sizes, etc.
        /// Remove particles that have shrunk below minimum size.
        /// </summary>
        public abstract void Update();

        /// <summary>
        /// Clears all particle instances. Called on world unload.
        /// </summary>
        public abstract void ClearInstances();

        /// <summary>
        /// Draws all particle instances as basic circles to the render target.
        /// These circles will be processed by the metaball shader to merge together.
        /// </summary>
        public abstract void DrawInstances();

        /// <summary>
        /// Optional: Calculate manual UV offset for a texture layer (for scrolling effects).
        /// Like Calamity's CalculateManualOffsetForLayer.
        /// </summary>
        public virtual Vector2 CalculateManualOffsetForLayer(int layerIndex)
        {
            return Vector2.Zero;
        }

        /// <summary>
        /// Optional: Prepare the SpriteBatch before drawing instances (custom blend states, etc).
        /// Override to use BlendState.Additive for fire/plasma effects.
        /// </summary>
        public virtual void PrepareSpriteBatch(SpriteBatch spriteBatch)
        {
            // Default: do nothing, use existing spritebatch state
        }

        /// <summary>
        /// Called before the shader processes this metaball's render target.
        /// Sets shader parameters and applies the shader pass.
        /// 
        /// Follows Calamity's Metaball.PrepareShaderForTarget pattern exactly:
        /// 1. Get MetaballEdgeShader
        /// 2. Calculate layerOffset from screen position for parallax
        /// 3. Set shader parameters (screenSize, layerSize, edgeColor, layerColor, etc.)
        /// 4. Set texture samplers (layer texture to sampler 1)
        /// 5. Apply the shader pass
        /// 
        /// Override in subclasses to use AdditiveMetaballEdgeShader for fire/plasma effects.
        /// </summary>
        public virtual void PrepareShaderForTarget(int layerIndex)
        {
            // Check if metaball shaders are available
            if (!MagnumMetaballShaders.ShadersAvailable)
                return;
            
            // Get the standard edge shader
            var metaballShader = MagnumMetaballShaders.MetaballEdgeShader;
            if (metaballShader?.Value == null)
                return;
            
            var gd = Main.instance.GraphicsDevice;
            
            // Fetch the layer texture
            var layers = Layers.ToList();
            if (layerIndex >= layers.Count || layers[layerIndex] == null)
                return;
                
            Texture2D layerTexture = layers[layerIndex];
            
            // Calculate screen size
            Vector2 screenSize = new Vector2(Main.screenWidth, Main.screenHeight);
            
            // Calculate layer scroll offset for parallax effect
            Vector2 layerScrollOffset = Main.screenPosition / screenSize + CalculateManualOffsetForLayer(layerIndex);
            if (FixedToScreen)
                layerScrollOffset = Vector2.Zero;
            
            // Set shader parameters (matching Calamity's parameter names exactly)
            metaballShader.Value.Parameters["layerSize"]?.SetValue(new Vector2(layerTexture.Width, layerTexture.Height));
            metaballShader.Value.Parameters["screenSize"]?.SetValue(screenSize);
            metaballShader.Value.Parameters["layerOffset"]?.SetValue(layerScrollOffset);
            metaballShader.Value.Parameters["edgeColor"]?.SetValue(EdgeColor.ToVector4());
            metaballShader.Value.Parameters["singleFrameScreenOffset"]?.SetValue((Main.screenLastPosition - Main.screenPosition) / screenSize);
            
            // Set layer color (default to white if not specified)
            Vector4 layerColor = LayerColors.Count > layerIndex ? LayerColors[layerIndex] : Color.White.ToVector4();
            metaballShader.Value.Parameters["layerColor"]?.SetValue(layerColor);
            
            // Set the layer texture on sampler 1
            gd.Textures[1] = layerTexture;
            gd.SamplerStates[1] = SamplerState.LinearWrap;
            
            // Apply the shader pass
            metaballShader.Value.CurrentTechnique.Passes["ParticlePass"].Apply();
        }

        protected sealed override void Register()
        {
            // Register with TML's ModType system
            ModTypeLookup<Metaball>.Register(this);

            // Register with our MetaballManager
            MetaballManager.RegisterMetaball(this);

            // Skip render target creation on servers
            if (Main.dedServ)
                return;

            // Create render targets on main thread (Calamity pattern)
            Main.QueueMainThreadAction(() =>
            {
                try
                {
                    // Create one render target per layer
                    int layerCount = Layers.Count();
                    for (int i = 0; i < layerCount; i++)
                    {
                        // +4 to account for drawing slightly off-screen (like Calamity)
                        var target = new RenderTarget2D(
                            Main.instance.GraphicsDevice,
                            Main.screenWidth + 4,
                            Main.screenHeight + 4,
                            false,
                            SurfaceFormat.Color,
                            DepthFormat.None,
                            0,
                            RenderTargetUsage.PreserveContents
                        );
                        LayerTargets.Add(target);
                    }
                }
                catch (Exception ex)
                {
                    ModContent.GetInstance<MagnumOpus>()?.Logger.Error(
                        $"Metaball.Register: Failed to create render targets - {ex.Message}");
                }
            });
        }

        public sealed override void SetupContent()
        {
            SetStaticDefaults();
        }

        /// <summary>
        /// Disposes of all render targets. Called on mod unload.
        /// </summary>
        public void Dispose()
        {
            foreach (var target in LayerTargets)
            {
                target?.Dispose();
            }
            LayerTargets.Clear();
        }

        /// <summary>
        /// Recreates render targets if screen size changed.
        /// </summary>
        internal void EnsureTargetsValid()
        {
            if (Main.dedServ)
                return;

            // Check if any targets are disposed or wrong size
            int expectedWidth = Main.screenWidth + 4;
            int expectedHeight = Main.screenHeight + 4;
            bool needsRecreate = LayerTargets.Count == 0 ||
                LayerTargets.Any(t => t == null || t.IsDisposed ||
                    t.Width != expectedWidth || t.Height != expectedHeight);

            if (needsRecreate)
            {
                // Dispose old targets
                Dispose();

                // Create new targets
                int layerCount = Layers.Count();
                for (int i = 0; i < layerCount; i++)
                {
                    var target = new RenderTarget2D(
                        Main.instance.GraphicsDevice,
                        expectedWidth,
                        expectedHeight,
                        false,
                        SurfaceFormat.Color,
                        DepthFormat.None,
                        0,
                        RenderTargetUsage.PreserveContents
                    );
                    LayerTargets.Add(target);
                }
            }
        }
    }
}
