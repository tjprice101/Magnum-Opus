using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace MagnumOpus.Common.Systems.VFX.Optimization
{
    /// <summary>
    /// Texture atlas for batch rendering optimization.
    /// Combines multiple textures into a single atlas to minimize draw calls.
    /// </summary>
    public class TextureAtlas
    {
        private Texture2D atlasTexture;
        private Dictionary<string, Rectangle> regions;

        public Texture2D Texture => atlasTexture;
        public int RegionCount => regions.Count;

        private TextureAtlas()
        {
            regions = new Dictionary<string, Rectangle>();
        }

        /// <summary>
        /// Builder for creating texture atlases.
        /// </summary>
        public class AtlasBuilder
        {
            private List<(string name, Texture2D texture)> textures;
            private int padding;

            public AtlasBuilder(int padding = 2)
            {
                textures = new List<(string, Texture2D)>();
                this.padding = padding;
            }

            /// <summary>
            /// Add a texture to be included in the atlas.
            /// </summary>
            public AtlasBuilder AddTexture(string name, Texture2D texture)
            {
                textures.Add((name, texture));
                return this;
            }

            /// <summary>
            /// Build the texture atlas from all added textures.
            /// Uses simple horizontal packing (for small atlases).
            /// </summary>
            public TextureAtlas Build(GraphicsDevice device)
            {
                if (textures.Count == 0)
                    return null;

                // Calculate atlas dimensions (simple horizontal packing)
                int totalWidth = padding;
                int maxHeight = 0;

                foreach (var (_, tex) in textures)
                {
                    totalWidth += tex.Width + padding;
                    maxHeight = System.Math.Max(maxHeight, tex.Height);
                }

                maxHeight += padding * 2;

                // Power of 2 dimensions (better GPU compatibility)
                int atlasWidth = NextPowerOfTwo(totalWidth);
                int atlasHeight = NextPowerOfTwo(maxHeight);

                // Create atlas texture
                Texture2D atlas = new Texture2D(device, atlasWidth, atlasHeight);
                Color[] atlasData = new Color[atlasWidth * atlasHeight];

                // Clear to transparent
                for (int i = 0; i < atlasData.Length; i++)
                    atlasData[i] = Color.Transparent;

                var regions = new Dictionary<string, Rectangle>();
                int currentX = padding;

                foreach (var (name, tex) in textures)
                {
                    // Get texture data
                    Color[] texData = new Color[tex.Width * tex.Height];
                    tex.GetData(texData);

                    // Copy to atlas
                    for (int y = 0; y < tex.Height; y++)
                    {
                        for (int x = 0; x < tex.Width; x++)
                        {
                            int srcIndex = y * tex.Width + x;
                            int dstIndex = (y + padding) * atlasWidth + (currentX + x);
                            atlasData[dstIndex] = texData[srcIndex];
                        }
                    }

                    // Record region
                    regions[name] = new Rectangle(currentX, padding, tex.Width, tex.Height);
                    currentX += tex.Width + padding;
                }

                atlas.SetData(atlasData);

                return new TextureAtlas
                {
                    atlasTexture = atlas,
                    regions = regions
                };
            }

            private int NextPowerOfTwo(int value)
            {
                int power = 1;
                while (power < value)
                    power *= 2;
                return power;
            }
        }

        /// <summary>
        /// Get the source rectangle for a named region.
        /// </summary>
        public Rectangle GetRegion(string name)
        {
            return regions.TryGetValue(name, out Rectangle rect) ? rect : Rectangle.Empty;
        }

        /// <summary>
        /// Check if a region exists in the atlas.
        /// </summary>
        public bool HasRegion(string name)
        {
            return regions.ContainsKey(name);
        }

        /// <summary>
        /// Draw a region from the atlas.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, string regionName, Vector2 position,
                         Color color, float rotation = 0f, Vector2? scale = null,
                         SpriteEffects effects = SpriteEffects.None)
        {
            Rectangle source = GetRegion(regionName);

            if (source == Rectangle.Empty)
                return;

            Vector2 origin = new Vector2(source.Width, source.Height) * 0.5f;

            spriteBatch.Draw(
                atlasTexture,
                position,
                source,
                color,
                rotation,
                origin,
                scale ?? Vector2.One,
                effects,
                0f
            );
        }

        /// <summary>
        /// Draw a region from the atlas with full control.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, string regionName, Vector2 position,
                         Rectangle? sourceSubRect, Color color, float rotation,
                         Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
        {
            Rectangle source = GetRegion(regionName);

            if (source == Rectangle.Empty)
                return;

            // Apply sub-rectangle if specified
            if (sourceSubRect.HasValue)
            {
                var sub = sourceSubRect.Value;
                source = new Rectangle(
                    source.X + sub.X,
                    source.Y + sub.Y,
                    sub.Width,
                    sub.Height
                );
            }

            spriteBatch.Draw(
                atlasTexture,
                position,
                source,
                color,
                rotation,
                origin,
                scale,
                effects,
                layerDepth
            );
        }

        /// <summary>
        /// Get all region names in the atlas.
        /// </summary>
        public IEnumerable<string> GetRegionNames()
        {
            return regions.Keys;
        }

        /// <summary>
        /// Dispose the atlas texture.
        /// </summary>
        public void Dispose()
        {
            atlasTexture?.Dispose();
            atlasTexture = null;
            regions.Clear();
        }
    }
}
