using System;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Graphics.Shaders;

namespace MagnumOpus.Content.SandboxLastPrism.Dusts
{
    public static class SLPDustBehaviorUtil
    {
        public static GlowPixelCrossBehavior AssignBehavior_GPCBase(
            float rotPower = 0.15f, int timeBeforeSlow = 3, float preSlowPower = 0.99f, float postSlowPower = 0.92f, float velToBeginShrink = 1f, float fadePower = 0.95f,
            bool shouldFadeColor = true, float colorFadePower = 0.95f)
        {
            GlowPixelCrossBehavior b = new GlowPixelCrossBehavior();

            b.behaviorToUse = GlowPixelCrossBehavior.Behavior.Base;

            b.base_rotPower = rotPower;
            b.base_timeBeforeSlow = timeBeforeSlow;
            b.base_preSlowPower = preSlowPower;
            b.base_postSlowPower = postSlowPower;
            b.base_velToBeginShrink = velToBeginShrink;
            b.base_fadePower = fadePower;

            b.base_shouldFadeColor = shouldFadeColor;
            b.base_colorFadePower = colorFadePower;

            return b;
        }

        public static LineSparkBehavior AssignBehavior_LSBase(
            float velFadePower = 0.97f, float preShrinkPower = 0.99f, float postShrinkPower = 0.97f, int timeToStartShrink = 40, int killEarlyTime = 60,
            float XScale = 1f, float YScale = 1f, bool shouldFadeColor = true, float colorFadePower = 0.95f)
        {
            LineSparkBehavior b = new LineSparkBehavior();

            b.base_velFadePower = velFadePower;
            b.base_preShrinkPower = preShrinkPower;
            b.base_postShrinkPower = postShrinkPower;
            b.base_timeToStartShrink = timeToStartShrink;
            b.base_killEarlyTime = killEarlyTime;
            b.Vector2DrawScale = new Vector2(XScale, YScale);

            b.base_shouldFadeColor = shouldFadeColor;
            b.base_colorFadePower = colorFadePower;

            return b;
        }

        public static SoftGlowDustBehavior AssignBehavior_SGDBase(float timeToStartFade = 5, float timeToChangeScale = 5, float fadeSpeed = 0.95f, float sizeChangeSpeed = 0.9f, int timeToKill = 60,
            float overallAlpha = 1f, bool DrawWhiteCore = false, float XScale = 1f, float YScale = 1f)
        {
            SoftGlowDustBehavior b = new SoftGlowDustBehavior();
            b.base_timeToStartFade = timeToStartFade;
            b.base_timeToChangeScale = timeToChangeScale;
            b.base_fadeSpeed = fadeSpeed;
            b.base_sizeChangeSpeed = sizeChangeSpeed;
            b.base_timeToKill = timeToKill;

            b.DrawWhiteCore = DrawWhiteCore;
            b.Vector2DrawScale = new Vector2(XScale, YScale);
            b.overallAlpha = overallAlpha;

            return b;
        }

        public static PixelGlowOrbBehavior AssignBehavior_PGOBase(
            float rotPower = 0.05f, int killEarlyTime = -1, int timeBeforeSlow = 3, float preSlowPower = 0.99f, float postSlowPower = 0.92f, float velToBeginShrink = 1f, float fadePower = 0.95f,
            bool dontDrawOrb = false, float glowIntensity = 0.2f, float colorFadePower = 0.95f)
        {
            PixelGlowOrbBehavior b = new PixelGlowOrbBehavior();

            b.base_rotPower = rotPower;
            b.base_killEarlyTime = killEarlyTime;
            b.base_timeBeforeSlow = timeBeforeSlow;
            b.base_preSlowPower = preSlowPower;
            b.base_postSlowPower = postSlowPower;
            b.base_velToBeginShrink = velToBeginShrink;
            b.base_fadePower = fadePower;

            b.base_dontDrawOrb = dontDrawOrb;
            b.base_glowIntensity = glowIntensity;
            b.base_colorFadePower = colorFadePower;

            return b;
        }
    }
}
