using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.Graphics.Shaders;

namespace MagnumOpus.Content.Fate.ResonantWeapons.TheConductorsLastConstellation.Primitives
{
    /// <summary>
    /// Configuration struct for Conductor trail rendering.
    /// Defines width, color, and offset functions + shader binding.
    /// </summary>
    public struct ConductorTrailSettings
    {
        /// <summary>Width of the trail at a given progress (0=newest, 1=oldest) and trail index.</summary>
        public Func<float, int, float> WidthFunction;

        /// <summary>Color of the trail at a given progress (0=newest, 1=oldest).</summary>
        public Func<float, Color> ColorFunction;

        /// <summary>Offset applied to the trail centre at each point (world space). Third param is trail index.</summary>
        public Func<float, int, Vector2> OffsetFunction;

        /// <summary>Optional MiscShaderData to apply during rendering.</summary>
        public MiscShaderData Shader;

        public ConductorTrailSettings(
            Func<float, int, float> width,
            Func<float, Color> color,
            Func<float, int, Vector2> offset = null,
            MiscShaderData shader = null)
        {
            WidthFunction = width;
            ColorFunction = color;
            OffsetFunction = offset;
            Shader = shader;
        }
    }
}
