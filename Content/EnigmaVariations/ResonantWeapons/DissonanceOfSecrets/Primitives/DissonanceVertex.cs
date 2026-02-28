using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.DissonanceOfSecrets.Primitives
{
    /// <summary>
    /// Custom vertex type for DissonanceOfSecrets primitive rendering.
    /// Position + Color + UV.xy + WidthCorrection.z
    /// </summary>
    public readonly struct DissonanceVertex : IVertexType
    {
        public readonly Vector2 Position;
        public readonly Color Color;
        public readonly Vector3 TextureCoordinates; // .XY = UV, .Z = width correction factor

        public VertexDeclaration VertexDeclaration => VertexDecl;

        public static readonly VertexDeclaration VertexDecl = new(new VertexElement[]
        {
            new(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
            new(8, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new(12, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0)
        });

        public DissonanceVertex(Vector2 position, Color color, Vector2 uv, float widthCorrection)
        {
            Position = position;
            Color = color;
            TextureCoordinates = new Vector3(uv, widthCorrection);
        }
    }
}
