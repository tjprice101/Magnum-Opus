using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Primitives
{
    /// <summary>
    /// Custom vertex type for Moonlight's Calling beam trail rendering.
    /// Position2D + Color + TexCoord3 (U, V, widthCorrection).
    /// </summary>
    public struct SerenadeVertexType : IVertexType
    {
        public Vector2 Position;
        public Color Color;
        public Vector3 TextureCoordinates; // x=U, y=V, z=widthCorrection

        public static readonly VertexDeclaration VertexDecl = new(new VertexElement[]
        {
            new(0,  VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
            new(8,  VertexElementFormat.Color,   VertexElementUsage.Color, 0),
            new(12, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0),
        });

        public readonly VertexDeclaration VertexDeclaration => VertexDecl;

        public SerenadeVertexType(Vector2 position, Color color, Vector2 uv, float widthCorrection = 0f)
        {
            Position = position;
            Color = color;
            TextureCoordinates = new Vector3(uv.X, uv.Y, widthCorrection);
        }
    }
}
