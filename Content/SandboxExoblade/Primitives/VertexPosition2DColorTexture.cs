using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MagnumOpus.Content.SandboxExoblade.Primitives
{
    public readonly struct VertexPosition2DColorTexture : IVertexType
    {
        public readonly Vector2 Position;
        public readonly Color Color;
        public readonly Vector3 TextureCoordinates;

        public VertexDeclaration VertexDeclaration => VertexDeclaration2D;

        public static readonly VertexDeclaration VertexDeclaration2D = new(new VertexElement[]
        {
            new(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
            new(8, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new(12, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0)
        });

        public VertexPosition2DColorTexture(Vector2 position, Color color, Vector2 textureCoordinates, float widthCorrectionFactor)
        {
            Position = position;
            Color = color;
            TextureCoordinates = new(textureCoordinates, widthCorrectionFactor);
        }
    }
}
