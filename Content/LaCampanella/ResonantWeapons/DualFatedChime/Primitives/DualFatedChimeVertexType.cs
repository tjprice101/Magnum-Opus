using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.DualFatedChime.Primitives
{
    /// <summary>
    /// Custom vertex type for Dual Fated Chime primitive trail rendering.
    /// Position (2D) + Color + TextureCoordinates (UV + width correction).
    /// </summary>
    public readonly struct DualFatedChimeVertex : IVertexType
    {
        public readonly Vector2 Position;
        public readonly Color Color;
        public readonly Vector3 TextureCoordinates;

        public static readonly VertexDeclaration Declaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
            new VertexElement(8, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0)
        );

        VertexDeclaration IVertexType.VertexDeclaration => Declaration;

        public DualFatedChimeVertex(Vector2 position, Color color, Vector3 texCoords)
        {
            Position = position;
            Color = color;
            TextureCoordinates = texCoords;
        }
    }
}
