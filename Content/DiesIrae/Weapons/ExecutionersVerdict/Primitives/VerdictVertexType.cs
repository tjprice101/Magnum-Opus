using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict.Primitives
{
    /// <summary>
    /// Custom vertex type for Executioner's Verdict trails.
    /// Position (2D screen) + Color + UV3 (progress, side, width).
    /// </summary>
    public struct VerdictVertexType : IVertexType
    {
        public Vector2 Position;
        public Color Color;
        public Vector3 TexCoord; // X = progress along trail, Y = 0/1 side, Z = width

        public static readonly VertexDeclaration VertexDecl = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
            new VertexElement(8, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0)
        );

        public VertexDeclaration VertexDeclaration => VertexDecl;

        public VerdictVertexType(Vector2 pos, Color col, Vector3 uv)
        {
            Position = pos;
            Color = col;
            TexCoord = uv;
        }
    }
}
