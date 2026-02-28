using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Primitives
{
    /// <summary>
    /// Custom vertex type for Wrath's Cleaver trail rendering.
    /// Position2D + Color + UV3 (UV.z carries width correction).
    /// </summary>
    public struct WrathVertexType : IVertexType
    {
        public Vector2 Position;
        public Color Color;
        public Vector3 TextureCoordinates;

        public static readonly VertexDeclaration VertexDecl = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
            new VertexElement(8, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0)
        );

        public VertexDeclaration VertexDeclaration => VertexDecl;

        public WrathVertexType(Vector2 position, Color color, Vector3 uv)
        {
            Position = position;
            Color = color;
            TextureCoordinates = uv;
        }
    }
}
