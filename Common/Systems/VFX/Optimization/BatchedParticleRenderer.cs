using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;

namespace MagnumOpus.Common.Systems.VFX.Optimization
{
    /// <summary>
    /// Batched particle renderer that queues all particles and draws them
    /// in a single draw call using a texture atlas.
    /// Dramatically reduces draw call count for particle-heavy effects.
    /// </summary>
    public class BatchedParticleRenderer
    {
        private struct ParticleDrawData
        {
            public string AtlasRegion;
            public Vector2 Position;
            public Color Color;
            public float Rotation;
            public Vector2 Scale;
        }

        private TextureAtlas atlas;
        private List<ParticleDrawData> drawQueue;
        private const int MaxBatchSize = 5000;

        public int QueuedCount => drawQueue.Count;
        public int MaxCapacity => MaxBatchSize;

        public BatchedParticleRenderer(TextureAtlas atlas)
        {
            this.atlas = atlas;
            drawQueue = new List<ParticleDrawData>(MaxBatchSize);
        }

        /// <summary>
        /// Queue a particle for drawing. Does not draw immediately.
        /// Call Flush() to draw all queued particles in a single batch.
        /// </summary>
        public bool QueueParticle(string type, Vector2 position, Color color,
                                  float rotation = 0f, float scale = 1f)
        {
            return QueueParticle(type, position, color, rotation, new Vector2(scale));
        }

        /// <summary>
        /// Queue a particle with separate X/Y scale.
        /// </summary>
        public bool QueueParticle(string type, Vector2 position, Color color,
                                  float rotation, Vector2 scale)
        {
            if (drawQueue.Count >= MaxBatchSize)
            {
                // Queue is full - could auto-flush here if desired
                return false;
            }

            drawQueue.Add(new ParticleDrawData
            {
                AtlasRegion = type,
                Position = position,
                Color = color,
                Rotation = rotation,
                Scale = scale
            });

            return true;
        }

        /// <summary>
        /// Queue multiple particles in a burst pattern.
        /// </summary>
        public void QueueBurst(string type, Vector2 center, int count, float radius,
                               Color color, float scale = 1f)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                Vector2 offset = angle.ToRotationVector2() * radius;
                QueueParticle(type, center + offset, color, angle, scale);
            }
        }

        /// <summary>
        /// Queue particles along a line.
        /// </summary>
        public void QueueLine(string type, Vector2 start, Vector2 end, int count,
                              Color color, float scale = 1f)
        {
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)(count - 1);
                Vector2 position = Vector2.Lerp(start, end, t);
                float rotation = (end - start).ToRotation();
                QueueParticle(type, position, color, rotation, scale);
            }
        }

        /// <summary>
        /// Draw all queued particles in a single batch with additive blending.
        /// Clears the queue after drawing.
        /// </summary>
        public void Flush(SpriteBatch spriteBatch)
        {
            FlushWithBlend(spriteBatch, BlendState.Additive);
        }

        /// <summary>
        /// Draw all queued particles with specified blend state.
        /// </summary>
        public void FlushWithBlend(SpriteBatch spriteBatch, BlendState blendState)
        {
            if (drawQueue.Count == 0)
                return;

            // Single Begin/End = single draw call (if all same texture)
            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                blendState,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                Main.GameViewMatrix.TransformationMatrix
            );

            foreach (var particle in drawQueue)
            {
                atlas.Draw(
                    spriteBatch,
                    particle.AtlasRegion,
                    particle.Position - Main.screenPosition,
                    particle.Color,
                    particle.Rotation,
                    particle.Scale
                );
            }

            spriteBatch.End();

            // Clear queue for next frame
            drawQueue.Clear();
        }

        /// <summary>
        /// Clear the draw queue without rendering.
        /// </summary>
        public void Clear()
        {
            drawQueue.Clear();
        }

        /// <summary>
        /// Get the percentage of queue capacity used.
        /// </summary>
        public float GetQueueUsage()
        {
            return drawQueue.Count / (float)MaxBatchSize;
        }
    }

    /// <summary>
    /// State-sorted batch renderer that groups draw calls by render state.
    /// Minimizes state changes which break batching.
    /// </summary>
    public class StateSortedRenderer
    {
        private class RenderBatch
        {
            public BlendState BlendState;
            public Effect Shader;
            public Texture2D Texture;
            public List<SpriteDrawData> Sprites;

            public RenderBatch()
            {
                Sprites = new List<SpriteDrawData>();
            }
        }

        private struct SpriteDrawData
        {
            public Rectangle? SourceRect;
            public Vector2 Position;
            public Color Color;
            public float Rotation;
            public Vector2 Origin;
            public Vector2 Scale;
        }

        private Dictionary<int, RenderBatch> batches;

        public int BatchCount => batches.Count;

        public StateSortedRenderer()
        {
            batches = new Dictionary<int, RenderBatch>();
        }

        /// <summary>
        /// Queue a sprite with specific render state.
        /// Sprites with matching state will be batched together.
        /// </summary>
        public void QueueSprite(Texture2D texture, Vector2 position, Color color,
                                BlendState blendState, Effect shader = null,
                                Rectangle? sourceRect = null, float rotation = 0f,
                                Vector2? origin = null, Vector2? scale = null)
        {
            int batchKey = GetBatchKey(texture, blendState, shader);

            if (!batches.ContainsKey(batchKey))
            {
                batches[batchKey] = new RenderBatch
                {
                    BlendState = blendState,
                    Shader = shader,
                    Texture = texture
                };
            }

            Rectangle? source = sourceRect ?? (texture != null ? new Rectangle(0, 0, texture.Width, texture.Height) : null);

            batches[batchKey].Sprites.Add(new SpriteDrawData
            {
                SourceRect = source,
                Position = position,
                Color = color,
                Rotation = rotation,
                Origin = origin ?? (source.HasValue ? new Vector2(source.Value.Width, source.Value.Height) * 0.5f : Vector2.Zero),
                Scale = scale ?? Vector2.One
            });
        }

        /// <summary>
        /// Draw all queued sprites, sorted by render state.
        /// Minimizes state changes between batches.
        /// </summary>
        public void Flush(SpriteBatch spriteBatch)
        {
            foreach (var batch in batches.Values)
            {
                if (batch.Sprites.Count == 0)
                    continue;

                // One Begin/End per unique render state
                spriteBatch.Begin(
                    SpriteSortMode.Deferred,
                    batch.BlendState,
                    SamplerState.LinearClamp,
                    DepthStencilState.None,
                    RasterizerState.CullNone,
                    batch.Shader,
                    Main.GameViewMatrix.TransformationMatrix
                );

                foreach (var sprite in batch.Sprites)
                {
                    spriteBatch.Draw(
                        batch.Texture,
                        sprite.Position - Main.screenPosition,
                        sprite.SourceRect,
                        sprite.Color,
                        sprite.Rotation,
                        sprite.Origin,
                        sprite.Scale,
                        SpriteEffects.None,
                        0f
                    );
                }

                spriteBatch.End();
            }

            // Clear for next frame
            foreach (var batch in batches.Values)
            {
                batch.Sprites.Clear();
            }
        }

        /// <summary>
        /// Clear all batches without rendering.
        /// </summary>
        public void Clear()
        {
            foreach (var batch in batches.Values)
            {
                batch.Sprites.Clear();
            }
        }

        private int GetBatchKey(Texture2D texture, BlendState blend, Effect shader)
        {
            unchecked
            {
                int hash = texture?.GetHashCode() ?? 0;
                hash = hash * 31 + blend.GetHashCode();
                hash = hash * 31 + (shader?.GetHashCode() ?? 0);
                return hash;
            }
        }

        /// <summary>
        /// Get total sprites queued across all batches.
        /// </summary>
        public int GetTotalSprites()
        {
            int total = 0;
            foreach (var batch in batches.Values)
            {
                total += batch.Sprites.Count;
            }
            return total;
        }
    }
}
