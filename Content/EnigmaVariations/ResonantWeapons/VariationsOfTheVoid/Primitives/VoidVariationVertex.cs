using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.VariationsOfTheVoid.Primitives
{
    /// <summary>
    /// Custom vertex type for VariationsOfTheVoid primitive rendering.
    /// Position + Color + UV.xy + WidthCorrection.z
    /// </summary>
    public readonly struct VoidVariationVertex : IVertexType
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

        public VoidVariationVertex(Vector2 position, Color color, Vector2 uv, float widthCorrection)
        {
            Position = position;
            Color = color;
            TextureCoordinates = new Vector3(uv, widthCorrection);
        }
    }
}
