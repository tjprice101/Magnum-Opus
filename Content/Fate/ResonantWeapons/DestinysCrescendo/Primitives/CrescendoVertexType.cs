using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MagnumOpus.Content.Fate.ResonantWeapons.DestinysCrescendo
{
    /// <summary>
    /// Custom vertex type for Destiny's Crescendo primitive trail rendering.
    /// Position2 + TexCoord2 + Color4 = 20 bytes per vertex.
    /// </summary>
    public struct CrescendoVertexType : IVertexType
    {
        public Vector2 Position;
        public Vector2 TexCoord;
        public Color Color;

        public static readonly VertexDeclaration _Declaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
            new VertexElement(8, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(16, VertexElementFormat.Color, VertexElementUsage.Color, 0)
        );

        VertexDeclaration IVertexType.VertexDeclaration => _Declaration;

        public CrescendoVertexType(Vector2 position, Vector2 texCoord, Color color)
        {
            Position = position;
            TexCoord = texCoord;
            Color = color;
        }
    }
}
