using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace MagnumOpus.Content.OdeToJoy
{
    /// <summary>
    /// Shared Ode to Joy color palette used by bosses, accessories, and tools.
    /// Weapons have their own per-weapon palettes in their self-contained folders.
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

        /// <summary>General palette gradient (green -> pink -> gold -> white)</summary>
        public static Color GetGradient(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            if (t < 0.33f)
                return Color.Lerp(LeafGreen, RosePink, t / 0.33f);
            if (t < 0.66f)
                return Color.Lerp(RosePink, GoldenPollen, (t - 0.33f) / 0.33f);
            return Color.Lerp(GoldenPollen, WhiteBloom, (t - 0.66f) / 0.34f);
        }

        /// <summary>Garden gradient (deep green -> verdant green -> golden pollen)</summary>
        public static Color GetGardenGradient(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            if (t < 0.5f)
                return Color.Lerp(DeepForest, VerdantGreen, t / 0.5f);
            return Color.Lerp(VerdantGreen, GoldenPollen, (t - 0.5f) / 0.5f);
        }

        /// <summary>Blossom gradient (rose pink -> petal pink -> white bloom)</summary>
        public static Color GetBlossomGradient(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            if (t < 0.5f)
                return Color.Lerp(RosePink, PetalPink, t / 0.5f);
            return Color.Lerp(PetalPink, WhiteBloom, (t - 0.5f) / 0.5f);
        }

        /// <summary>Petal gradient (deep pink -> golden pollen -> sunlight)</summary>
        public static Color GetPetalGradient(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            if (t < 0.5f)
                return Color.Lerp(RosePink, GoldenPollen, t / 0.5f);
            return Color.Lerp(GoldenPollen, SunlightYellow, (t - 0.5f) / 0.5f);
        }

        /// <summary>Draw additive bloom behind an item texture</summary>
        public static void DrawItemBloom(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Vector2 origin, float rotation, float scale, float pulse)
        {
            float bloomScale = scale * (1f + pulse * 0.15f);
            Color bloomColor = GoldenPollen * (0.3f + pulse * 0.1f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

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
