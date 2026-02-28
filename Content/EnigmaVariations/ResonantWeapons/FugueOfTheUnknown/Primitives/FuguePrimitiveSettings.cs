using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.FugueOfTheUnknown.Primitives
{
    /// <summary>
    /// Settings for FugueOfTheUnknown primitive trail rendering.
    /// </summary>
    public readonly struct FuguePrimitiveSettings
    {
        public readonly Func<float, float> WidthFunction;
        public readonly Func<float, Color> ColorFunction;
        public readonly Func<float, Vector2> OffsetFunction;
        public readonly Effect Shader;
        public readonly bool Smoothing;
        public readonly int MaxPoints;

        public FuguePrimitiveSettings(
            Func<float, float> widthFunction,
            Func<float, Color> colorFunction,
            Effect shader,
            Func<float, Vector2> offsetFunction = null,
            bool smoothing = true,
            int maxPoints = 150)
        {
            WidthFunction = widthFunction;
            ColorFunction = colorFunction;
            OffsetFunction = offsetFunction;
            Shader = shader;
            Smoothing = smoothing;
            MaxPoints = maxPoints;
        }
    }
}
