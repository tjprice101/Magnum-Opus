using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Primitives
{
    /// <summary>
    /// Custom vertex type for the Incisor's self-contained primitive trail renderer.
    /// Layout: 2D Position + Color + UV.xy + width-correction Z.
    /// </summary>
    public readonly struct IncisorVertex : IVertexType
    {
        public readonly Vector2 Position;
        public readonly Color Color;
        public readonly Vector3 TextureCoordinates;

        public VertexDeclaration VertexDeclaration => VertexDecl;

        public static readonly VertexDeclaration VertexDecl = new(new VertexElement[]
        {
            new(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
            new(8, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new(12, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0)
        });

        public IncisorVertex(Vector2 position, Color color, Vector2 textureCoordinates, float widthCorrectionFactor)
        {
            Position = position;
            Color = color;
            TextureCoordinates = new(textureCoordinates, widthCorrectionFactor);
        }
    }
}
