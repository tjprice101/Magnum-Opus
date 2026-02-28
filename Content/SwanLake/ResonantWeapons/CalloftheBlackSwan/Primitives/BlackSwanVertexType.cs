using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.CalloftheBlackSwan.Primitives
{
    /// <summary>
    /// Custom vertex type for Black Swan primitive trail rendering.
    /// Position (2D) + Color + TextureCoordinates (UV + width correction).
    /// </summary>
    public readonly struct BlackSwanVertex : IVertexType
    {
        public readonly Vector2 Position;
        public readonly Color Color;
        public readonly Vector3 TextureCoordinates; // X=U, Y=V, Z=widthCorrection

        public static readonly VertexDeclaration Declaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
            new VertexElement(8, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0)
        );

        VertexDeclaration IVertexType.VertexDeclaration => Declaration;

        public BlackSwanVertex(Vector2 position, Color color, Vector3 texCoords)
        {
            Position = position;
            Color = color;
            TextureCoordinates = texCoords;
        }
    }
}
