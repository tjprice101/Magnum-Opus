using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.IgnitionOfTheBell.Primitives
{
    /// <summary>
    /// Custom vertex type for IgnitionOfTheBell trail rendering.
    /// Position (Vector2) + Color + TextureCoordinates (Vector3: U, V, CompletionRatio).
    /// 24 bytes per vertex.
    /// </summary>
    public readonly struct IgnitionOfTheBellVertex : IVertexType
    {
        public readonly Vector2 Position;
        public readonly Color Color;
        public readonly Vector3 TextureCoordinates;

        private static readonly VertexDeclaration _declaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
            new VertexElement(8, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0)
        );

        public VertexDeclaration VertexDeclaration => _declaration;

        public IgnitionOfTheBellVertex(Vector2 position, Color color, Vector3 texCoords)
        {
            Position = position;
            Color = color;
            TextureCoordinates = texCoords;
        }
    }
}
