using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// A snapshot of the current state of a SpriteBatch.
    /// Used to preserve and restore SpriteBatch settings across rendering operations.
    /// Inspired by Calamity's SpriteBatchSnapshot pattern.
    /// </summary>
    public struct SpriteBatchSnapshot
    {
        public SpriteSortMode SortMode;
        public BlendState BlendState;
        public SamplerState SamplerState;
        public DepthStencilState DepthStencilState;
        public RasterizerState RasterizerState;
        public Effect CustomEffect;
        public Matrix TransformMatrix;

        /// <summary>
        /// Creates a snapshot from raw parameters.
        /// </summary>
        public SpriteBatchSnapshot(
            SpriteSortMode sortMode,
            BlendState blendState,
            SamplerState samplerState,
            DepthStencilState depthStencilState,
            RasterizerState rasterizerState,
            Effect customEffect,
            Matrix transformMatrix)
        {
            SortMode = sortMode;
            BlendState = blendState;
            SamplerState = samplerState;
            DepthStencilState = depthStencilState;
            RasterizerState = rasterizerState;
            CustomEffect = customEffect;
            TransformMatrix = transformMatrix;
        }

        /// <summary>
        /// Creates a snapshot from the current settings of a SpriteBatch.
        /// Uses reflection to access internal fields since FNA/XNA doesn't expose them directly.
        /// </summary>
        public SpriteBatchSnapshot(SpriteBatch spriteBatch)
        {
            // Access internal fields via reflection (works on both FNA and XNA)
            var sbType = typeof(SpriteBatch);
            
            SortMode = GetFieldValue<SpriteSortMode>(spriteBatch, "sortMode");
            BlendState = GetFieldValue<BlendState>(spriteBatch, "blendState") ?? BlendState.AlphaBlend;
            SamplerState = GetFieldValue<SamplerState>(spriteBatch, "samplerState") ?? SamplerState.PointClamp;
            DepthStencilState = GetFieldValue<DepthStencilState>(spriteBatch, "depthStencilState") ?? DepthStencilState.None;
            RasterizerState = GetFieldValue<RasterizerState>(spriteBatch, "rasterizerState") ?? RasterizerState.CullNone;
            CustomEffect = GetFieldValue<Effect>(spriteBatch, "customEffect");
            TransformMatrix = GetFieldValue<Matrix>(spriteBatch, "transformMatrix");
        }

        private static T GetFieldValue<T>(SpriteBatch sb, string fieldName)
        {
            try
            {
                var field = typeof(SpriteBatch).GetField(fieldName, 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                    return (T)field.GetValue(sb);
            }
            catch { }
            return default;
        }

        /// <summary>
        /// Default snapshot for standard game rendering.
        /// </summary>
        public static SpriteBatchSnapshot Default => new SpriteBatchSnapshot(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            Main.DefaultSamplerState,
            DepthStencilState.None,
            Main.Rasterizer,
            null,
            Main.GameViewMatrix.TransformationMatrix
        );
        
        /// <summary>
        /// Snapshot for additive blend rendering.
        /// </summary>
        public static SpriteBatchSnapshot Additive => new SpriteBatchSnapshot(
            SpriteSortMode.Deferred,
            BlendState.Additive,
            SamplerState.PointClamp,
            DepthStencilState.None,
            RasterizerState.CullNone,
            null,
            Main.GameViewMatrix.TransformationMatrix
        );
    }

    /// <summary>
    /// A disposable scope that captures and restores SpriteBatch state.
    /// Use with 'using' statement for automatic cleanup.
    /// Inspired by Calamity's SpriteBatchScope pattern.
    /// </summary>
    public readonly struct SpriteBatchScope : IDisposable
    {
        private readonly SpriteBatch _spriteBatch;
        private readonly SpriteBatchSnapshot? _oldState;
        private readonly bool _wasBegun;

        /// <summary>
        /// Creates a new scope. If the SpriteBatch was already begun,
        /// its current parameters are saved and it is ended.
        /// The original parameters will be reapplied on disposal.
        /// </summary>
        public SpriteBatchScope(SpriteBatch spriteBatch)
        {
            _spriteBatch = spriteBatch;
            _wasBegun = spriteBatch.HasBeginBeenCalled();
            
            if (_wasBegun)
            {
                _oldState = new SpriteBatchSnapshot(spriteBatch);
                spriteBatch.End();
            }
            else
            {
                _oldState = null;
            }
        }

        /// <summary>
        /// Ends the SpriteBatch (if active) and restores the old state if one was captured.
        /// </summary>
        public void Dispose()
        {
            // End current batch if active
            if (_spriteBatch.HasBeginBeenCalled())
            {
                try { _spriteBatch.End(); } catch { }
            }
            
            // Restore old state if we had one
            if (_oldState.HasValue)
            {
                var ss = _oldState.Value;
                _spriteBatch.Begin(
                    ss.SortMode,
                    ss.BlendState,
                    ss.SamplerState,
                    ss.DepthStencilState,
                    ss.RasterizerState,
                    ss.CustomEffect,
                    ss.TransformMatrix
                );
            }
        }
    }

    /// <summary>
    /// Extension methods for SpriteBatch state management.
    /// Provides Calamity-style safe state handling.
    /// </summary>
    public static class SpriteBatchExtensions
    {
        /// <summary>
        /// Determines if Begin has been called on this SpriteBatch.
        /// Uses reflection to access the internal 'beginCalled' field.
        /// </summary>
        public static bool HasBeginBeenCalled(this SpriteBatch spriteBatch)
        {
            try
            {
                var field = typeof(SpriteBatch).GetField("beginCalled", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                    return (bool)field.GetValue(spriteBatch);
            }
            catch { }
            
            // Fallback: assume it's been called if we can't check
            return true;
        }

        /// <summary>
        /// Creates a SpriteBatchScope for this SpriteBatch.
        /// Use with 'using' for automatic state restoration.
        /// </summary>
        public static SpriteBatchScope Scope(this SpriteBatch spriteBatch)
        {
            return new SpriteBatchScope(spriteBatch);
        }

        /// <summary>
        /// Ends the SpriteBatch and outputs a snapshot of its state.
        /// </summary>
        public static void End(this SpriteBatch spriteBatch, out SpriteBatchSnapshot snapshot)
        {
            snapshot = new SpriteBatchSnapshot(spriteBatch);
            spriteBatch.End();
        }

        /// <summary>
        /// Begins the SpriteBatch with parameters from a snapshot.
        /// </summary>
        public static void Begin(this SpriteBatch spriteBatch, in SpriteBatchSnapshot snapshot)
        {
            spriteBatch.Begin(
                snapshot.SortMode,
                snapshot.BlendState,
                snapshot.SamplerState,
                snapshot.DepthStencilState,
                snapshot.RasterizerState,
                snapshot.CustomEffect,
                snapshot.TransformMatrix
            );
        }

        /// <summary>
        /// Ends and immediately restarts the SpriteBatch with the given snapshot.
        /// </summary>
        public static void Restart(this SpriteBatch spriteBatch, in SpriteBatchSnapshot snapshot)
        {
            spriteBatch.End();
            spriteBatch.Begin(snapshot);
        }

        /// <summary>
        /// Safely ends the SpriteBatch only if it was begun.
        /// Returns true if it was ended, false if it wasn't active.
        /// </summary>
        public static bool TryEnd(this SpriteBatch spriteBatch)
        {
            if (!spriteBatch.HasBeginBeenCalled())
                return false;
                
            spriteBatch.End();
            return true;
        }

        /// <summary>
        /// Safely begins the SpriteBatch only if it's not already begun.
        /// Returns true if it was begun, false if it was already active.
        /// </summary>
        public static bool TryBegin(this SpriteBatch spriteBatch, 
            SpriteSortMode sortMode = SpriteSortMode.Deferred,
            BlendState blendState = null,
            SamplerState samplerState = null,
            DepthStencilState depthStencilState = null,
            RasterizerState rasterizerState = null,
            Effect effect = null,
            Matrix? transformMatrix = null)
        {
            if (spriteBatch.HasBeginBeenCalled())
                return false;
                
            spriteBatch.Begin(
                sortMode,
                blendState ?? BlendState.AlphaBlend,
                samplerState ?? Main.DefaultSamplerState,
                depthStencilState ?? DepthStencilState.None,
                rasterizerState ?? Main.Rasterizer,
                effect,
                transformMatrix ?? Main.GameViewMatrix.TransformationMatrix
            );
            return true;
        }

        /// <summary>
        /// Sets the blend state by restarting the SpriteBatch.
        /// </summary>
        public static void SetBlendState(this SpriteBatch spriteBatch, BlendState blendState)
        {
            spriteBatch.End();
            spriteBatch.Begin(
                SpriteSortMode.Immediate,
                blendState,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullCounterClockwise,
                null,
                Main.GameViewMatrix.TransformationMatrix
            );
        }
    }
}
