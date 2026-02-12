using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX.Screen
{
    /// <summary>
    /// Screen flash system with basic flash and chromatic aberration flash modes.
    /// 
    /// Inspired by VFX+ ScreenFlashSystem patterns.
    /// 
    /// Flash Types:
    /// - BasicFlash: Simple screen-wide color overlay that fades
    /// - ChromaticFlash: RGB channel separation that expands/contracts
    /// 
    /// Usage:
    ///   ScreenFlashSystem.Instance.Flash(Color.White, 0.8f, 20);
    ///   ScreenFlashSystem.Instance.ChromaticFlash(0.5f, 25, moveInward: true);
    /// </summary>
    public class ScreenFlashSystem : ModSystem
    {
        public static ScreenFlashSystem Instance { get; private set; }

        // Flash state
        private float flashIntensity;
        private int flashLifetime;
        private int flashTimer;
        private Color flashColor;
        private FlashType currentFlashType;

        // Chromatic aberration specific
        private float caIntensity;
        private float caWhiteIntensity;
        private float caDistanceMult;
        private bool caMoveInward;

        // Render target for CA effect
        private RenderTarget2D caRenderTarget;

        /// <summary>
        /// Current flash alpha (0-1).
        /// </summary>
        public float FlashAlpha => flashIntensity * (1f - (float)flashTimer / flashLifetime);

        /// <summary>
        /// Whether a flash is currently active.
        /// </summary>
        public bool IsFlashing => flashTimer < flashLifetime;

        public override void Load()
        {
            Instance = this;
            On_Main.DrawDust += DrawFlashOverlay;
        }

        public override void Unload()
        {
            Instance = null;
            On_Main.DrawDust -= DrawFlashOverlay;
            caRenderTarget?.Dispose();
            caRenderTarget = null;
        }

        public override void PostUpdateEverything()
        {
            if (flashTimer < flashLifetime)
            {
                flashTimer++;
            }
        }

        #region Public Flash Methods

        /// <summary>
        /// Trigger a basic screen flash.
        /// </summary>
        /// <param name="color">Flash color</param>
        /// <param name="intensity">Flash intensity (0-1)</param>
        /// <param name="lifetime">Duration in frames</param>
        public void Flash(Color color, float intensity, int lifetime)
        {
            flashColor = color;
            flashIntensity = MathHelper.Clamp(intensity, 0f, 1f);
            flashLifetime = Math.Max(1, lifetime);
            flashTimer = 0;
            currentFlashType = FlashType.Basic;
        }

        /// <summary>
        /// Trigger a chromatic aberration flash.
        /// </summary>
        /// <param name="intensity">CA offset intensity</param>
        /// <param name="lifetime">Duration in frames</param>
        /// <param name="whiteIntensity">Optional white flash overlay</param>
        /// <param name="distanceMult">Distance multiplier for CA spread</param>
        /// <param name="moveInward">If true, CA contracts. If false, CA expands.</param>
        public void ChromaticFlash(float intensity, int lifetime, float whiteIntensity = 0.3f, 
            float distanceMult = 1f, bool moveInward = true)
        {
            caIntensity = intensity;
            caWhiteIntensity = whiteIntensity;
            caDistanceMult = distanceMult;
            caMoveInward = moveInward;
            flashIntensity = intensity;
            flashLifetime = Math.Max(1, lifetime);
            flashTimer = 0;
            currentFlashType = FlashType.ChromaticAberration;
        }

        /// <summary>
        /// Trigger an impact flash (white flash + screen shake combo).
        /// </summary>
        /// <param name="intensity">Overall intensity</param>
        public void ImpactFlash(float intensity)
        {
            Flash(Color.White, intensity * 0.7f, 8);
            Effects.ScreenShakeManager.Instance?.AddImpactShake(intensity * 10f);
        }

        /// <summary>
        /// Trigger a damage flash (red tint).
        /// </summary>
        public void DamageFlash(float intensity)
        {
            Flash(new Color(255, 50, 50), intensity * 0.4f, 12);
        }

        /// <summary>
        /// Trigger a heal flash (green tint).
        /// </summary>
        public void HealFlash(float intensity)
        {
            Flash(new Color(100, 255, 150), intensity * 0.3f, 15);
        }

        /// <summary>
        /// Trigger a cosmic/fate themed flash.
        /// </summary>
        public void CosmicFlash(float intensity)
        {
            ChromaticFlash(intensity * 0.6f, 20, whiteIntensity: 0.5f, distanceMult: 1.5f, moveInward: false);
        }

        #endregion

        #region Drawing

        private void DrawFlashOverlay(On_Main.orig_DrawDust orig, Main self)
        {
            orig(self);

            if (!IsFlashing || Main.dedServ)
                return;

            float progress = (float)flashTimer / flashLifetime;
            SpriteBatch sb = Main.spriteBatch;

            switch (currentFlashType)
            {
                case FlashType.Basic:
                    DrawBasicFlash(sb, progress);
                    break;

                case FlashType.ChromaticAberration:
                    DrawChromaticFlash(sb, progress);
                    break;
            }
        }

        private void DrawBasicFlash(SpriteBatch sb, float progress)
        {
            // Calculate fading alpha
            float alpha = flashIntensity * (1f - EaseOutQuad(progress));

            if (alpha <= 0.001f)
                return;

            // Draw screen-wide overlay
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            Texture2D pixel = GetPixelTexture();
            Rectangle screenRect = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
            Color drawColor = flashColor * alpha;

            sb.Draw(pixel, screenRect, drawColor);

            sb.End();
        }

        private void DrawChromaticFlash(SpriteBatch sb, float progress)
        {
            float alpha = flashIntensity * (1f - EaseOutCubic(progress));

            if (alpha <= 0.001f)
                return;

            // Calculate CA offset based on progress
            float caProgress = caMoveInward ? (1f - progress) : progress;
            float offset = caIntensity * caDistanceMult * caProgress * 15f;

            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            Texture2D pixel = GetPixelTexture();
            Rectangle screenRect = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);

            // Draw white flash component
            if (caWhiteIntensity > 0)
            {
                float whiteAlpha = caWhiteIntensity * (1f - EaseOutQuad(progress));
                sb.Draw(pixel, screenRect, Color.White * whiteAlpha);
            }

            // Draw color channel offsets (simulated CA)
            float channelAlpha = alpha * 0.3f;

            // Red channel - offset right
            Rectangle redRect = new Rectangle((int)offset, 0, Main.screenWidth, Main.screenHeight);
            sb.Draw(pixel, redRect, new Color(255, 0, 0) * channelAlpha);

            // Blue channel - offset left
            Rectangle blueRect = new Rectangle((int)-offset, 0, Main.screenWidth, Main.screenHeight);
            sb.Draw(pixel, blueRect, new Color(0, 0, 255) * channelAlpha);

            sb.End();
        }

        #endregion

        #region Helpers

        private static Texture2D _pixelTexture;
        private static Texture2D GetPixelTexture()
        {
            if (_pixelTexture == null || _pixelTexture.IsDisposed)
            {
                _pixelTexture = new Texture2D(Main.instance.GraphicsDevice, 1, 1);
                _pixelTexture.SetData(new[] { Color.White });
            }
            return _pixelTexture;
        }

        private static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
        private static float EaseOutCubic(float t) => 1f - (1f - t) * (1f - t) * (1f - t);

        #endregion
    }

    public enum FlashType
    {
        Basic,
        ChromaticAberration
    }
}
