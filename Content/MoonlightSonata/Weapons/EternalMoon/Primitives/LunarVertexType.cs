using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon.Primitives
{
    /// <summary>
    /// Custom vertex type for the Eternal Moon primitive trail renderer.
    /// Contains 2D position, color, and extended texture coordinates (UV + width correction).
    /// </summary>
    public readonly struct LunarVertexType : IVertexType
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

        public LunarVertexType(Vector2 position, Color color,
            Vector2 textureCoordinates, float widthCorrectionFactor)
        {
            Position = position;
            Color = color;
            TextureCoordinates = new(textureCoordinates, widthCorrectionFactor);
        }
    }
}
