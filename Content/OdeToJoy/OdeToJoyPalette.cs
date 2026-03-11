using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy
{
    /// <summary>
    /// Shared Ode to Joy color palette used by bosses, accessories, and tools.
    /// All gradient methods sample from OdeToJoyGradientLUTandRAMP.png for consistent color ramping.
    /// </summary>
    public static class OdeToJoyPalette
    {
        // Greens & Foliage
        public static readonly Color MossShadow = new Color(30, 45, 20);
        public static readonly Color DeepForest = new Color(20, 60, 15);
        public static readonly Color LeafGreen = new Color(50, 140, 40);
        public static readonly Color BudGreen = new Color(70, 170, 50);
        public static readonly Color VerdantGreen = new Color(90, 200, 60);

        // Pinks & Roses
        public static readonly Color RosePink = new Color(230, 120, 150);
        public static readonly Color PetalPink = new Color(240, 170, 180);

        // Golds & Ambers
        public static readonly Color WarmAmber = new Color(200, 150, 40);
        public static readonly Color GoldenPollen = new Color(255, 210, 60);
        public static readonly Color PollenGold = new Color(240, 200, 50);
        public static readonly Color SunlightYellow = new Color(255, 240, 120);

        // Whites
        public static readonly Color WhiteBloom = new Color(255, 250, 235);

        // ═══════════════════════════════════════════════════════
        //  LUT TEXTURE SAMPLING
        // ═══════════════════════════════════════════════════════

        private const string LUTPath = "MagnumOpus/Assets/VFX Asset Library/ColorGradients/OdeToJoyGradientLUTandRAMP";
        private static Color[] _lutColors;
        private static int _lutWidth;
        private static bool _lutLoadAttempted;

        /// <summary>
        /// Loads and caches the pixel row from the OdeToJoy gradient LUT texture.
        /// Samples the middle row (y = height/2) for the color ramp.
        /// </summary>
        private static void EnsureLUTLoaded()
        {
            if (_lutLoadAttempted)
                return;
            _lutLoadAttempted = true;

            if (!ModContent.HasAsset(LUTPath))
                return;

            var asset = ModContent.Request<Texture2D>(LUTPath, AssetRequestMode.ImmediateLoad);
            if (asset?.Value == null)
                return;

            Texture2D tex = asset.Value;
            _lutWidth = tex.Width;
            int height = tex.Height;

            Color[] allPixels = new Color[_lutWidth * height];
            tex.GetData(allPixels);

            // Sample the middle row of the texture as the 1D color ramp
            int midRow = height / 2;
            _lutColors = new Color[_lutWidth];
            for (int x = 0; x < _lutWidth; x++)
                _lutColors[x] = allPixels[midRow * _lutWidth + x];
        }

        /// <summary>
        /// Samples the LUT texture at position t (0-1). Falls back to LeafGreen->WhiteBloom lerp if texture unavailable.
        /// </summary>
        public static Color SampleLUT(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            EnsureLUTLoaded();

            if (_lutColors == null || _lutWidth == 0)
                return Color.Lerp(LeafGreen, WhiteBloom, t);

            float scaledT = t * (_lutWidth - 1);
            int index = (int)scaledT;
            float frac = scaledT - index;

            if (index >= _lutWidth - 1)
                return _lutColors[_lutWidth - 1];

            return Color.Lerp(_lutColors[index], _lutColors[index + 1], frac);
        }

        /// <summary>General palette gradient — samples full LUT range</summary>
        public static Color GetGradient(float t) => SampleLUT(t);

        /// <summary>Garden gradient — samples full LUT range (used for green-to-gold effects)</summary>
        public static Color GetGardenGradient(float t) => SampleLUT(t);

        /// <summary>Blossom gradient — samples full LUT range (used for pink-to-white effects)</summary>
        public static Color GetBlossomGradient(float t) => SampleLUT(t);

        /// <summary>Petal gradient — samples full LUT range (used for pink-to-gold effects)</summary>
        public static Color GetPetalGradient(float t) => SampleLUT(t);

        /// <summary>Draw additive bloom behind an item texture</summary>
        public static void DrawItemBloom(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Vector2 origin, float rotation, float scale, float pulse)
        {
            float bloomScale = scale * (1f + pulse * 0.15f);
            Color bloomColor = GoldenPollen * (0.3f + pulse * 0.1f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = (MathHelper.TwoPi * i / 4f).ToRotationVector2() * (2f + pulse);
                spriteBatch.Draw(texture, position + offset, null, bloomColor, rotation, origin, bloomScale, SpriteEffects.None, 0f);
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
