using System;
using Microsoft.Xna.Framework;

namespace MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Primitives
{
    /// <summary>
    /// Configuration for Wrath's Cleaver trail rendering passes.
    /// </summary>
    public readonly struct WrathTrailSettings
    {
        public readonly Func<float, float> WidthFunction;
        public readonly Func<float, Color> ColorFunction;
        public readonly Func<float, Vector2> OffsetFunction;
        public readonly int SmoothingSteps;
        public readonly Action ShaderSetup;

        public WrathTrailSettings(
            Func<float, float> widthFunc,
            Func<float, Color> colorFunc,
            Func<float, Vector2> offsetFunc = null,
            int smoothingSteps = 12,
            Action shaderSetup = null)
        {
            WidthFunction = widthFunc;
            ColorFunction = colorFunc;
            OffsetFunction = offsetFunc ?? (_ => Vector2.Zero);
            SmoothingSteps = smoothingSteps;
            ShaderSetup = shaderSetup;
        }
    }
}
