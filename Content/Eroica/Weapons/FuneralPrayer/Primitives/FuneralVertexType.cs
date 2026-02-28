using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MagnumOpus.Content.Eroica.Weapons.FuneralPrayer.Primitives
{
    public struct FuneralVertexType : IVertexType
    {
        public Vector2 Position;
        public Color Color;
        public Vector3 TextureCoordinates;

        public static readonly VertexDeclaration _vertexDeclaration = new(new VertexElement[]
        {
            new(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
            new(8, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new(12, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0)
        });

        public VertexDeclaration VertexDeclaration => _vertexDeclaration;

        public FuneralVertexType(Vector2 position, Color color, Vector3 texCoords)
        {
            Position = position;
            Color = color;
            TextureCoordinates = texCoords;
        }
    }
}
