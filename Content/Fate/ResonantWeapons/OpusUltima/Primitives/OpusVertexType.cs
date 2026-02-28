using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MagnumOpus.Content.Fate.ResonantWeapons.OpusUltima.Primitives
{
    /// <summary>
    /// Custom vertex type for Opus Ultima trail rendering.
    /// Position (2D) + Color + UV (3-component: U, V, Width for cross-section correction).
    /// </summary>
    public struct OpusVertexType : IVertexType
    {
        public Vector2 Position;
        public Color Color;
        public Vector3 TextureCoordinate; // U, V, Width

        public static readonly VertexDeclaration Declaration = new(
            new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
            new VertexElement(8, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0)
        );

        public readonly VertexDeclaration VertexDeclaration => Declaration;

        public OpusVertexType(Vector2 position, Color color, Vector3 uv)
        {
            Position = position;
            Color = color;
            TextureCoordinate = uv;
        }
    }
}
