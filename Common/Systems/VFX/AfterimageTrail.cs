using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// Calamity-style afterimage trail system.
    /// Stores position history and draws afterimages with decreasing transparency.
    /// 
    /// Usage in projectile:
    ///   private AfterimageTrail trail;
    ///   
    ///   public override void AI()
    ///   {
    ///       trail ??= new AfterimageTrail(12);
    ///       trail.Update(Projectile.Center, Projectile.rotation);
    ///   }
    ///   
    ///   public override bool PreDraw(ref Color lightColor)
    ///   {
    ///       trail?.Draw(Main.spriteBatch, texture, origin, lightColor, Projectile.scale);
    ///       return true;
    ///   }
    /// </summary>
    public class AfterimageTrail
    {
        /// <summary>
        /// Position history array.
        /// </summary>
        public Vector2[] Positions { get; private set; }

        /// <summary>
        /// Rotation history array.
        /// </summary>
        public float[] Rotations { get; private set; }

        /// <summary>
        /// Number of afterimages to store.
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// Current write index in the circular buffer.
        /// </summary>
        private int _index;

        /// <summary>
        /// Whether the trail has been initialized with enough data.
        /// </summary>
        private bool _initialized;

        /// <summary>
        /// Frame counter for update frequency.
        /// </summary>
        private int _frameCounter;

        /// <summary>
        /// Creates a new afterimage trail.
        /// </summary>
        /// <param name="length">Number of afterimages (8-20 recommended)</param>
        public AfterimageTrail(int length = 12)
        {
            Length = Math.Max(1, length);
            Positions = new Vector2[Length];
            Rotations = new float[Length];
            _index = 0;
            _initialized = false;
        }

        /// <summary>
        /// Updates the trail with a new position and rotation.
        /// Call this every frame in AI().
        /// </summary>
        /// <param name="position">Current world position</param>
        /// <param name="rotation">Current rotation</param>
        /// <param name="everyNFrames">Only record every N frames (1 = every frame)</param>
        public void Update(Vector2 position, float rotation, int everyNFrames = 1)
        {
            _frameCounter++;
            
            if (_frameCounter >= everyNFrames)
            {
                _frameCounter = 0;
                
                Positions[_index] = position;
                Rotations[_index] = rotation;
                
                _index = (_index + 1) % Length;
                
                if (!_initialized && _index == 0)
                    _initialized = true;
            }
        }

        /// <summary>
        /// Draws the afterimage trail with standard decreasing alpha.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch to draw with</param>
        /// <param name="texture">Texture to draw</param>
        /// <param name="origin">Texture origin</param>
        /// <param name="color">Base color</param>
        /// <param name="scale">Base scale</param>
        /// <param name="alphaMultiplier">Overall alpha multiplier (0-1)</param>
        public void Draw(SpriteBatch spriteBatch, Texture2D texture, Vector2 origin, 
            Color color, float scale, float alphaMultiplier = 1f)
        {
            if (!_initialized || spriteBatch == null || texture == null)
                return;

            // Draw from oldest to newest (so newer ones are on top)
            for (int i = 0; i < Length; i++)
            {
                // Calculate the actual index (oldest first)
                int actualIndex = (_index + i) % Length;
                Vector2 pos = Positions[actualIndex];
                
                // Skip uninitialized positions
                if (pos == Vector2.Zero)
                    continue;

                // Progress: 0 = oldest, 1 = newest
                float progress = (float)i / (Length - 1);
                
                // Alpha decreases for older afterimages
                float alpha = progress * alphaMultiplier;
                
                // Scale slightly decreases for older afterimages
                float afterimageScale = scale * (0.7f + progress * 0.3f);
                
                Vector2 drawPos = pos - Main.screenPosition;
                
                spriteBatch.Draw(texture, drawPos, null, color * alpha, 
                    Rotations[actualIndex], origin, afterimageScale, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draws the afterimage trail with gradient colors (two colors blending from old to new).
        /// </summary>
        public void DrawGradient(SpriteBatch spriteBatch, Texture2D texture, Vector2 origin,
            Color oldColor, Color newColor, float scale, float alphaMultiplier = 1f)
        {
            if (!_initialized || spriteBatch == null || texture == null)
                return;

            for (int i = 0; i < Length; i++)
            {
                int actualIndex = (_index + i) % Length;
                Vector2 pos = Positions[actualIndex];
                
                if (pos == Vector2.Zero)
                    continue;

                float progress = (float)i / (Length - 1);
                float alpha = progress * alphaMultiplier;
                float afterimageScale = scale * (0.7f + progress * 0.3f);
                
                // Gradient color lerp
                Color gradientColor = Color.Lerp(oldColor, newColor, progress);
                
                Vector2 drawPos = pos - Main.screenPosition;
                
                spriteBatch.Draw(texture, drawPos, null, gradientColor * alpha,
                    Rotations[actualIndex], origin, afterimageScale, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draws the afterimage trail with additive blending for glow effect.
        /// </summary>
        public void DrawAdditive(SpriteBatch spriteBatch, Texture2D texture, Vector2 origin,
            Color color, float scale, float alphaMultiplier = 1f)
        {
            if (!_initialized || spriteBatch == null || texture == null)
                return;

            // Switch to additive blending
            try { spriteBatch.End(); } catch { }
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            for (int i = 0; i < Length; i++)
            {
                int actualIndex = (_index + i) % Length;
                Vector2 pos = Positions[actualIndex];
                
                if (pos == Vector2.Zero)
                    continue;

                float progress = (float)i / (Length - 1);
                float alpha = progress * alphaMultiplier;
                float afterimageScale = scale * (0.7f + progress * 0.3f);
                
                // Remove alpha channel for proper additive blending
                Color glowColor = new Color(color.R, color.G, color.B, 0) * alpha;
                
                Vector2 drawPos = pos - Main.screenPosition;
                
                spriteBatch.Draw(texture, drawPos, null, glowColor,
                    Rotations[actualIndex], origin, afterimageScale, SpriteEffects.None, 0f);
            }

            // Restore normal blending
            try { spriteBatch.End(); } catch { }
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Draws the afterimage trail with a custom drawing action per image.
        /// </summary>
        public void DrawCustom(SpriteBatch spriteBatch, Action<SpriteBatch, Vector2, float, float, int> drawAction)
        {
            if (!_initialized || spriteBatch == null || drawAction == null)
                return;

            for (int i = 0; i < Length; i++)
            {
                int actualIndex = (_index + i) % Length;
                Vector2 pos = Positions[actualIndex];
                
                if (pos == Vector2.Zero)
                    continue;

                float progress = (float)i / (Length - 1);
                
                drawAction(spriteBatch, pos, Rotations[actualIndex], progress, i);
            }
        }

        /// <summary>
        /// Draws afterimages with shader support.
        /// Uses ShaderRenderer for enhanced bloom/glow effects.
        /// </summary>
        public void DrawWithShader(SpriteBatch spriteBatch, Texture2D texture, Vector2 origin,
            Color color, float scale, Shaders.ShaderRenderer.ShaderType shaderType, float intensity = 1f)
        {
            if (!_initialized || spriteBatch == null || texture == null)
                return;

            using (Shaders.ShaderRenderer.BeginShaderScope(spriteBatch, shaderType, color, intensity))
            {
                for (int i = 0; i < Length; i++)
                {
                    int actualIndex = (_index + i) % Length;
                    Vector2 pos = Positions[actualIndex];
                    
                    if (pos == Vector2.Zero)
                        continue;

                    float progress = (float)i / (Length - 1);
                    float alpha = progress;
                    float afterimageScale = scale * (0.7f + progress * 0.3f);
                    
                    Vector2 drawPos = pos - Main.screenPosition;
                    
                    spriteBatch.Draw(texture, drawPos, null, Color.White * alpha,
                        Rotations[actualIndex], origin, afterimageScale, SpriteEffects.None, 0f);
                }
            }
        }

        /// <summary>
        /// Gets the most recent position in the trail.
        /// </summary>
        public Vector2 GetNewestPosition()
        {
            int newestIndex = (_index - 1 + Length) % Length;
            return Positions[newestIndex];
        }

        /// <summary>
        /// Gets the oldest position in the trail.
        /// </summary>
        public Vector2 GetOldestPosition()
        {
            return Positions[_index];
        }

        /// <summary>
        /// Clears all stored positions.
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < Length; i++)
            {
                Positions[i] = Vector2.Zero;
                Rotations[i] = 0f;
            }
            _index = 0;
            _initialized = false;
        }

        /// <summary>
        /// Resizes the trail. Clears existing data.
        /// </summary>
        public void Resize(int newLength)
        {
            Length = Math.Max(1, newLength);
            Positions = new Vector2[Length];
            Rotations = new float[Length];
            Clear();
        }
    }

    /// <summary>
    /// Enhanced afterimage trail with velocity and scale history.
    /// </summary>
    public class EnhancedAfterimageTrail : AfterimageTrail
    {
        /// <summary>
        /// Scale history array.
        /// </summary>
        public float[] Scales { get; private set; }

        /// <summary>
        /// Velocity history array.
        /// </summary>
        public Vector2[] Velocities { get; private set; }

        private int _enhancedIndex;

        public EnhancedAfterimageTrail(int length = 12) : base(length)
        {
            Scales = new float[Length];
            Velocities = new Vector2[Length];
            _enhancedIndex = 0;
        }

        /// <summary>
        /// Updates with position, rotation, scale, and velocity.
        /// </summary>
        public void Update(Vector2 position, float rotation, float scale, Vector2 velocity, int everyNFrames = 1)
        {
            base.Update(position, rotation, everyNFrames);
            
            // Store additional data
            Scales[_enhancedIndex] = scale;
            Velocities[_enhancedIndex] = velocity;
            _enhancedIndex = (_enhancedIndex + 1) % Length;
        }

        /// <summary>
        /// Draws with velocity-based stretch effect.
        /// </summary>
        public void DrawStretched(SpriteBatch spriteBatch, Texture2D texture, Vector2 origin,
            Color color, float stretchFactor = 0.5f, float alphaMultiplier = 1f)
        {
            if (spriteBatch == null || texture == null)
                return;

            for (int i = 0; i < Length; i++)
            {
                int actualIndex = (_enhancedIndex + i) % Length;
                Vector2 pos = Positions[actualIndex];
                
                if (pos == Vector2.Zero)
                    continue;

                float progress = (float)i / (Length - 1);
                float alpha = progress * alphaMultiplier;
                float baseScale = Scales[actualIndex];
                
                // Calculate stretch based on velocity
                Vector2 vel = Velocities[actualIndex];
                float speed = vel.Length();
                float stretchX = 1f + (speed * stretchFactor * 0.01f);
                float stretchY = 1f / stretchX; // Compress perpendicular
                
                Vector2 stretch = new Vector2(baseScale * stretchX, baseScale * stretchY);
                float velRotation = vel.ToRotation();
                
                Vector2 drawPos = pos - Main.screenPosition;
                
                spriteBatch.Draw(texture, drawPos, null, color * alpha,
                    velRotation, origin, stretch, SpriteEffects.None, 0f);
            }
        }
    }
}
