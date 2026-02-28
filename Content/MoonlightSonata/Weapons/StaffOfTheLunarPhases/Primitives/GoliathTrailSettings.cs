using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Shaders;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.StaffOfTheLunarPhases.Primitives
{
    /// <summary>
    /// Configuration for the Goliath beam trail renderer.
    /// </summary>
    public struct GoliathTrailSettings
    {
        /// <summary>Width of the trail at a given completion ratio (0 = head, 1 = tail).</summary>
        public Func<float, Vector2, float> WidthFunction;

        /// <summary>Color of the trail at a given completion ratio.</summary>
        public Func<float, Vector2, Color> ColorFunction;

        /// <summary>Optional per-vertex offset function.</summary>
        public Func<float, Vector2, Vector2> OffsetFunction;

        /// <summary>Whether to apply CatmullRom smoothing.</summary>
        public bool Smoothen;

        /// <summary>Shader to apply during rendering.</summary>
        public MiscShaderData Shader;

        /// <summary>UV scroll offset along the trail.</summary>
        public float TextureScrollOffset;

        public GoliathTrailSettings(
            Func<float, Vector2, float> widthFunction,
            Func<float, Vector2, Color> colorFunction,
            Func<float, Vector2, Vector2> offsetFunction = null,
            bool smoothen = true,
            MiscShaderData shader = null,
            float textureScrollOffset = 0f)
        {
            WidthFunction = widthFunction;
            ColorFunction = colorFunction;
            OffsetFunction = offsetFunction;
            Smoothen = smoothen;
            Shader = shader;
            TextureScrollOffset = textureScrollOffset;
        }
    }
}
