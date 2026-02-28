using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.InfernalChimesCalling.Primitives
{
    public readonly struct InfernalChimesVertex : IVertexType
    {
        public readonly Vector2 Position;
        public readonly Color Color;
        public readonly Vector3 TextureCoordinates;

        private static readonly VertexDeclaration _declaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
            new VertexElement(8, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0));

        public VertexDeclaration VertexDeclaration => _declaration;

        public InfernalChimesVertex(Vector2 pos, Color color, Vector3 tex)
        { Position = pos; Color = color; TextureCoordinates = tex; }
    }
}
