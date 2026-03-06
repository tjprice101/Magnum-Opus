using Microsoft.Xna.Framework.Graphics;

namespace MagnumOpus.Common
{
    /// <summary>
    /// Centralized blend state constants for MagnumOpus VFX rendering.
    /// 
    /// CRITICAL: MonoGame's built-in BlendState.Additive uses Blend.SourceAlpha as the
    /// source blend factor. This means that colors with Alpha = 0 contribute NOTHING
    /// to the final pixel (srcRGB × srcAlpha = srcRGB × 0 = 0), making them invisible.
    /// 
    /// MagnumOpus extensively uses the "A=0 premultiplied alpha trick" (Color with { A = 0 })
    /// which is designed to work under BlendState.AlphaBlend (where source blend is Blend.One).
    /// When this pattern is incorrectly used under BlendState.Additive, all draws are invisible.
    /// 
    /// TrueAdditive fixes this by using Blend.One for both source and destination:
    ///   Result = srcRGB × 1 + destRGB × 1 = pure additive regardless of alpha.
    /// 
    /// This blend state is safe for BOTH A=0 colors AND normal-alpha colors.
    /// </summary>
    public static class MagnumBlendStates
    {
        /// <summary>
        /// True additive blending: Result = srcRGB + destRGB.
        /// Source blend = One (not SourceAlpha), so alpha value doesn't matter.
        /// Use this for non-shader draws that use the "A=0 premultiplied alpha trick"
        /// (Color with { A = 0 }) where fade is baked into RGB channels.
        /// Do NOT use this for shader-driven draws — use ShaderAdditive instead.
        /// </summary>
        public static readonly BlendState TrueAdditive = new BlendState
        {
            Name = "MagnumOpus.TrueAdditive",
            ColorBlendFunction = BlendFunction.Add,
            AlphaBlendFunction = BlendFunction.Add,
            ColorSourceBlend = Blend.One,
            ColorDestinationBlend = Blend.One,
            AlphaSourceBlend = Blend.One,
            AlphaDestinationBlend = Blend.One,
        };

        /// <summary>
        /// Shader-aware additive blending: Result = srcRGB × srcAlpha + destRGB.
        /// Source blend = SourceAlpha, so the shader's alpha output controls brightness.
        /// Use this for SpriteSortMode.Immediate shader-driven draws where the .fx shader
        /// computes meaningful alpha for edge falloff, trail taper, and opacity masking.
        /// </summary>
        public static readonly BlendState ShaderAdditive = new BlendState
        {
            Name = "MagnumOpus.ShaderAdditive",
            ColorBlendFunction = BlendFunction.Add,
            AlphaBlendFunction = BlendFunction.Add,
            ColorSourceBlend = Blend.SourceAlpha,
            ColorDestinationBlend = Blend.One,
            AlphaSourceBlend = Blend.SourceAlpha,
            AlphaDestinationBlend = Blend.One,
        };
    }
}
