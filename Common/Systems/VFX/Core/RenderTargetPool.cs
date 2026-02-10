using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// Unified RenderTarget2D pooling and management system.
    /// 
    /// This system provides:
    /// - Automatic render target pooling to reduce allocations
    /// - Proper disposal and recreation on resolution changes
    /// - Named persistent targets for screen effects
    /// - Transient targets for single-frame operations
    /// 
    /// Key patterns from research:
    /// - RenderTargetUsage.PreserveContents for multi-pass effects
    /// - Proper SetRenderTarget(null) restoration
    /// - Screen size change detection and recreation
    /// 
    /// Usage:
    ///   // Get a transient target for single-frame use
    ///   using var scope = RenderTargetPool.GetTransient(out var target);
    ///   device.SetRenderTarget(target);
    ///   // ... draw to target ...
    ///   scope.Dispose(); // Returns to pool
    ///   
    ///   // Get a persistent named target (cached across frames)
    ///   var bloomTarget = RenderTargetPool.GetPersistent("BloomBuffer");
    /// </summary>
    public class RenderTargetPool : ModSystem
    {
        #region Singleton Access
        
        private static RenderTargetPool _instance;
        public static RenderTargetPool Instance => _instance;
        
        #endregion
        
        #region Pool Storage
        
        // Pool of available transient targets (by size hash)
        private readonly Dictionary<int, Queue<RenderTarget2D>> _transientPool = new();
        
        // Persistent named targets
        private readonly Dictionary<string, RenderTarget2D> _persistentTargets = new();
        
        // All targets for disposal tracking
        private readonly List<RenderTarget2D> _allTargets = new();
        
        // Screen size tracking for recreation
        private int _lastScreenWidth;
        private int _lastScreenHeight;
        
        // Pool limits
        private const int MaxTransientPerSize = 4;
        private const int MaxTotalTargets = 20;
        
        #endregion
        
        #region Lifecycle
        
        public override void Load()
        {
            if (Main.dedServ)
                return;
            
            _instance = this;
            _lastScreenWidth = Main.screenWidth;
            _lastScreenHeight = Main.screenHeight;
        }
        
        public override void Unload()
        {
            DisposeAll();
            _instance = null;
        }
        
        public override void PostUpdateEverything()
        {
            if (Main.dedServ)
                return;
            
            // Check for resolution changes
            if (Main.screenWidth != _lastScreenWidth || Main.screenHeight != _lastScreenHeight)
            {
                OnResolutionChanged();
                _lastScreenWidth = Main.screenWidth;
                _lastScreenHeight = Main.screenHeight;
            }
        }
        
        private void OnResolutionChanged()
        {
            Mod.Logger.Debug($"RenderTargetPool: Resolution changed to {Main.screenWidth}x{Main.screenHeight}, recreating targets");
            
            // Dispose all screen-sized targets
            foreach (var target in _allTargets.ToArray())
            {
                if (target != null && !target.IsDisposed &&
                    (target.Width == _lastScreenWidth && target.Height == _lastScreenHeight))
                {
                    target.Dispose();
                    _allTargets.Remove(target);
                }
            }
            
            // Clear persistent targets (they'll be recreated on next request)
            _persistentTargets.Clear();
            
            // Clear transient pools for old screen size
            int oldSizeHash = GetSizeHash(_lastScreenWidth, _lastScreenHeight);
            if (_transientPool.TryGetValue(oldSizeHash, out var oldQueue))
            {
                while (oldQueue.Count > 0)
                {
                    var target = oldQueue.Dequeue();
                    target?.Dispose();
                }
                _transientPool.Remove(oldSizeHash);
            }
        }
        
        private void DisposeAll()
        {
            foreach (var target in _allTargets)
            {
                try { target?.Dispose(); } catch { }
            }
            _allTargets.Clear();
            _transientPool.Clear();
            _persistentTargets.Clear();
        }
        
        #endregion
        
        #region Transient Target Pool
        
        /// <summary>
        /// Gets a transient render target for single-frame use.
        /// Returns a disposable scope that returns the target to the pool.
        /// </summary>
        /// <param name="target">The acquired render target</param>
        /// <param name="width">Target width (defaults to screen width)</param>
        /// <param name="height">Target height (defaults to screen height)</param>
        /// <returns>Disposable scope that returns target to pool</returns>
        public static TransientTargetScope GetTransient(out RenderTarget2D target, 
            int width = -1, int height = -1)
        {
            if (width < 0) width = Main.screenWidth;
            if (height < 0) height = Main.screenHeight;
            
            target = Instance?.AcquireTransient(width, height);
            return new TransientTargetScope(target, width, height);
        }
        
        /// <summary>
        /// Gets a transient target at half resolution (for blur passes).
        /// </summary>
        public static TransientTargetScope GetHalfResTransient(out RenderTarget2D target)
        {
            return GetTransient(out target, Main.screenWidth / 2, Main.screenHeight / 2);
        }
        
        /// <summary>
        /// Gets a transient target at quarter resolution (for heavy blur).
        /// </summary>
        public static TransientTargetScope GetQuarterResTransient(out RenderTarget2D target)
        {
            return GetTransient(out target, Main.screenWidth / 4, Main.screenHeight / 4);
        }
        
        private RenderTarget2D AcquireTransient(int width, int height)
        {
            int sizeHash = GetSizeHash(width, height);
            
            // Try to get from pool
            if (_transientPool.TryGetValue(sizeHash, out var pool) && pool.Count > 0)
            {
                var pooled = pool.Dequeue();
                if (pooled != null && !pooled.IsDisposed)
                    return pooled;
            }
            
            // Create new target
            return CreateTarget(width, height, RenderTargetUsage.DiscardContents);
        }
        
        internal void ReturnTransient(RenderTarget2D target, int width, int height)
        {
            if (target == null || target.IsDisposed)
                return;
            
            int sizeHash = GetSizeHash(width, height);
            
            if (!_transientPool.TryGetValue(sizeHash, out var pool))
            {
                pool = new Queue<RenderTarget2D>();
                _transientPool[sizeHash] = pool;
            }
            
            // Only pool if under limit
            if (pool.Count < MaxTransientPerSize)
            {
                pool.Enqueue(target);
            }
            else
            {
                // Too many - dispose
                target.Dispose();
                _allTargets.Remove(target);
            }
        }
        
        #endregion
        
        #region Persistent Named Targets
        
        /// <summary>
        /// Gets or creates a persistent named render target.
        /// These are cached across frames and automatically recreated on resolution change.
        /// </summary>
        /// <param name="name">Unique name for this target</param>
        /// <param name="width">Target width (defaults to screen width)</param>
        /// <param name="height">Target height (defaults to screen height)</param>
        /// <param name="preserveContents">Whether to preserve contents after unbind</param>
        public static RenderTarget2D GetPersistent(string name, 
            int width = -1, int height = -1, bool preserveContents = false)
        {
            if (width < 0) width = Main.screenWidth;
            if (height < 0) height = Main.screenHeight;
            
            return Instance?.AcquirePersistent(name, width, height, preserveContents);
        }
        
        private RenderTarget2D AcquirePersistent(string name, int width, int height, bool preserveContents)
        {
            // Check if we have a valid cached target
            if (_persistentTargets.TryGetValue(name, out var existing))
            {
                if (existing != null && !existing.IsDisposed && 
                    existing.Width == width && existing.Height == height)
                {
                    return existing;
                }
                
                // Dispose old invalid target
                existing?.Dispose();
                _allTargets.Remove(existing);
            }
            
            // Create new target
            var usage = preserveContents ? RenderTargetUsage.PreserveContents : RenderTargetUsage.DiscardContents;
            var target = CreateTarget(width, height, usage);
            _persistentTargets[name] = target;
            
            return target;
        }
        
        #endregion
        
        #region Target Creation
        
        private RenderTarget2D CreateTarget(int width, int height, RenderTargetUsage usage)
        {
            // Respect total limit
            if (_allTargets.Count >= MaxTotalTargets)
            {
                // Try to find and dispose unused transient targets
                CleanupOldTargets();
            }
            
            var device = Main.graphics.GraphicsDevice;
            var target = new RenderTarget2D(
                device,
                width, height,
                mipMap: false,
                preferredFormat: SurfaceFormat.Color,
                preferredDepthFormat: DepthFormat.None,
                preferredMultiSampleCount: 0,
                usage: usage
            );
            
            _allTargets.Add(target);
            return target;
        }
        
        private void CleanupOldTargets()
        {
            // Remove disposed targets from tracking
            _allTargets.RemoveAll(t => t == null || t.IsDisposed);
            
            // If still over limit, dispose oldest transient targets
            foreach (var pool in _transientPool.Values)
            {
                while (pool.Count > 0 && _allTargets.Count >= MaxTotalTargets)
                {
                    var target = pool.Dequeue();
                    target?.Dispose();
                    _allTargets.Remove(target);
                }
            }
        }
        
        private static int GetSizeHash(int width, int height)
        {
            return (width << 16) | (height & 0xFFFF);
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Safely sets a render target and clears it.
        /// </summary>
        public static void SetAndClear(RenderTarget2D target, Color? clearColor = null)
        {
            var device = Main.graphics.GraphicsDevice;
            device.SetRenderTarget(target);
            device.Clear(clearColor ?? Color.Transparent);
        }
        
        /// <summary>
        /// Restores rendering to the back buffer.
        /// </summary>
        public static void RestoreBackBuffer()
        {
            Main.graphics.GraphicsDevice.SetRenderTarget(null);
        }
        
        /// <summary>
        /// Draws a render target to the screen with specified blend state.
        /// Handles SpriteBatch state management automatically.
        /// </summary>
        public static void DrawToScreen(SpriteBatch sb, RenderTarget2D target, 
            BlendState blendState = null, float opacity = 1f)
        {
            if (target == null || target.IsDisposed)
                return;
            
            blendState ??= BlendState.AlphaBlend;
            
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, blendState, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone);
            
            sb.Draw(target, Vector2.Zero, Color.White * opacity);
            
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
        
        /// <summary>
        /// Performs a multi-pass blur operation on a render target.
        /// </summary>
        public static void ApplyGaussianBlur(RenderTarget2D source, RenderTarget2D destination,
            int passes = 2)
        {
            // Without shaders, we simulate blur by drawing at multiple scales
            var device = Main.graphics.GraphicsDevice;
            var sb = Main.spriteBatch;
            
            using var scope1 = GetHalfResTransient(out var temp1);
            using var scope2 = GetHalfResTransient(out var temp2);
            
            // Pass 1: Draw to half-res
            SetAndClear(temp1);
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone);
            sb.Draw(source, new Rectangle(0, 0, temp1.Width, temp1.Height), Color.White);
            sb.End();
            
            // Pass 2: Multi-sample blur simulation
            SetAndClear(temp2);
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone);
            
            // Draw multiple times with slight offsets
            float offsetAmount = 1f;
            Vector2[] offsets = new[]
            {
                new Vector2(-offsetAmount, 0),
                new Vector2(offsetAmount, 0),
                new Vector2(0, -offsetAmount),
                new Vector2(0, offsetAmount)
            };
            
            float alpha = 0.25f;
            foreach (var offset in offsets)
            {
                sb.Draw(temp1, offset, Color.White * alpha);
            }
            sb.End();
            
            // Final pass: Draw back to destination at full res
            SetAndClear(destination);
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone);
            sb.Draw(temp2, new Rectangle(0, 0, destination.Width, destination.Height), Color.White);
            sb.End();
            
            RestoreBackBuffer();
        }
        
        #endregion
    }
    
    /// <summary>
    /// Disposable scope for transient render targets.
    /// Returns the target to the pool on dispose.
    /// </summary>
    public readonly struct TransientTargetScope : IDisposable
    {
        private readonly RenderTarget2D _target;
        private readonly int _width;
        private readonly int _height;
        
        internal TransientTargetScope(RenderTarget2D target, int width, int height)
        {
            _target = target;
            _width = width;
            _height = height;
        }
        
        public void Dispose()
        {
            RenderTargetPool.Instance?.ReturnTransient(_target, _width, _height);
        }
    }
}
