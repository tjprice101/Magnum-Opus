using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.GrandioseChime.Primitives
{
    public struct GrandioseChimeVertexType : IVertexType
    {
        public Vector2 Position;
        public Color Color;
        public Vector3 TextureCoordinates;

        public static readonly VertexDeclaration _vertexDeclaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
            new VertexElement(8, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0)
        );

        public VertexDeclaration VertexDeclaration => _vertexDeclaration;

        public GrandioseChimeVertexType(Vector2 pos, Color color, Vector3 texCoord)
        {
            Position = pos;
            Color = color;
            TextureCoordinates = texCoord;
        }
    }
}
