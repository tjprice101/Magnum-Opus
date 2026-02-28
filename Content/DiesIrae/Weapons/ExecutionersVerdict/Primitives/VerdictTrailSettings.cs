using System;
using Microsoft.Xna.Framework;

namespace MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict.Primitives
{
    /// <summary>
    /// Configuration for Executioner's Verdict trail rendering.
    /// </summary>
    public struct VerdictTrailSettings
    {
        public Func<float, float> WidthFunction;
        public Func<float, Color> ColorFunction;
        public Func<float, Vector2> OffsetFunction;
        public int SmoothingSteps;
        public Action ShaderSetup;

        public VerdictTrailSettings(
            Func<float, float> widthFunc,
            Func<float, Color> colorFunc,
            Func<float, Vector2> offsetFunc = null,
            int smoothing = 3,
            Action shaderSetup = null)
        {
            WidthFunction = widthFunc;
            ColorFunction = colorFunc;
            OffsetFunction = offsetFunc ?? (_ => Vector2.Zero);
            SmoothingSteps = smoothing;
            ShaderSetup = shaderSetup;
        }
    }
}
