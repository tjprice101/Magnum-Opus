using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX.Sparkle
{
    /// <summary>
    /// SparkleBloomHelper — Replaces excessive SoftGlow bloom stacking with
    /// dazzling, shader-driven 4-point star sparkles that twinkle and shimmer.
    ///
    /// Instead of drawing 3-6 layers of SoftGlow at different scales (which create
    /// the "white blob" effect visible in the screenshot), this system draws:
    /// 1. A single subtle SoftGlow underlayer for ambient light
    /// 2. A field of twinkling 4-point star sparkles rendered with a per-theme
    ///    sparkle shader for crystalline shimmer, prismatic refraction, and flash peaks
    /// 3. A tiny hot core point bloom at the center
    ///
    /// Each theme has its own unique sparkle shader that gives the sparkles
    /// a visually distinct character matching the theme's identity.
    ///
    /// USAGE (replaces multi-layer SoftGlow stacking):
    /// <code>
    /// // OLD (excessive bloom):
    /// sb.Draw(softGlow, pos, null, color * 0.5f, 0f, origin, 0.15f, ...);
    /// sb.Draw(softGlow, pos, null, color * 0.3f, 0f, origin, 0.08f, ...);
    /// sb.Draw(softGlow, pos, null, Color.White * 0.2f, 0f, origin, 0.03f, ...);
    ///
    /// // NEW (sparkle bloom):
    /// SparkleBloomHelper.DrawSparkleBloom(sb, pos, SparkleTheme.SwanLake, 
    ///     intensity: 0.8f, radius: 80f, sparkleCount: 8, time: time);
    /// </code>
    /// </summary>
    public static class SparkleBloomHelper
    {
        // ---- TEXTURE PATHS ----
        private static readonly string Projectiles = "MagnumOpus/Assets/VFX Asset Library/Projectiles/";
        private static readonly string Bloom = "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/";

        // ---- CACHED TEXTURES ----
        private static Asset<Texture2D> _star4Point;
        private static Asset<Texture2D> _brightStar1;
        private static Asset<Texture2D> _brightStar2;
        private static Asset<Texture2D> _starburstFlare;
        private static Asset<Texture2D> _softGlow;
        private static Asset<Texture2D> _pointBloom;
        private static Asset<Texture2D> _starFlare;

        // ---- SHADER CACHE (one per theme) ----
        private static readonly Effect[] _themeShaders = new Effect[11];
        private static bool _texturesLoaded;

        /// <summary>
        /// Ensures all sparkle textures are loaded. Called lazily on first use.
        /// </summary>
        private static void EnsureTextures()
        {
            if (_texturesLoaded) return;
            _texturesLoaded = true;

            _star4Point = ModContent.Request<Texture2D>(Projectiles + "4PointStarShiningProjectile", AssetRequestMode.ImmediateLoad);
            _brightStar1 = ModContent.Request<Texture2D>(Projectiles + "BrightStarProjectile1", AssetRequestMode.ImmediateLoad);
            _brightStar2 = ModContent.Request<Texture2D>(Projectiles + "BrightStarProjectile2", AssetRequestMode.ImmediateLoad);
            _starburstFlare = ModContent.Request<Texture2D>(Projectiles + "8-Point Starburst Flare", AssetRequestMode.ImmediateLoad);
            _softGlow = ModContent.Request<Texture2D>(Bloom + "SoftGlow", AssetRequestMode.ImmediateLoad);
            _pointBloom = ModContent.Request<Texture2D>(Bloom + "PointBloom", AssetRequestMode.ImmediateLoad);
            _starFlare = ModContent.Request<Texture2D>(Bloom + "StarFlare", AssetRequestMode.ImmediateLoad);
        }

        // ---- PUBLIC TEXTURE ACCESSORS ----
        public static Texture2D Star4Point { get { EnsureTextures(); return _star4Point.Value; } }
        public static Texture2D BrightStar1 { get { EnsureTextures(); return _brightStar1.Value; } }
        public static Texture2D BrightStar2 { get { EnsureTextures(); return _brightStar2.Value; } }
        public static Texture2D StarburstFlare { get { EnsureTextures(); return _starburstFlare.Value; } }
        public static Texture2D SoftGlow { get { EnsureTextures(); return _softGlow.Value; } }
        public static Texture2D PointBloom { get { EnsureTextures(); return _pointBloom.Value; } }
        public static Texture2D StarFlare { get { EnsureTextures(); return _starFlare.Value; } }

        /// <summary>
        /// Gets the sparkle shader for the specified theme, loading it lazily.
        /// Each theme's shader path: Effects/{Theme}/Sparkle/{Theme}SparkleShader
        /// </summary>
        public static Effect GetThemeShader(SparkleTheme theme)
        {
            int idx = (int)theme;
            if (_themeShaders[idx] != null) return _themeShaders[idx];

            string path = theme switch
            {
                SparkleTheme.MoonlightSonata => "MagnumOpus/Effects/MoonlightSonata/Sparkle/LunarSparkleShader",
                SparkleTheme.Eroica => "MagnumOpus/Effects/Eroica/Sparkle/HeroicSparkleShader",
                SparkleTheme.SwanLake => "MagnumOpus/Effects/SwanLake/Sparkle/PrismaticSparkleShader",
                SparkleTheme.LaCampanella => "MagnumOpus/Effects/LaCampanella/Sparkle/BellfireSparkleShader",
                SparkleTheme.EnigmaVariations => "MagnumOpus/Effects/EnigmaVariations/Sparkle/VoidSparkleShader",
                SparkleTheme.Fate => "MagnumOpus/Effects/Fate/Sparkle/CelestialSparkleShader",
                SparkleTheme.Spring => "MagnumOpus/Effects/Eroica/Sparkle/HeroicSparkleShader",
                SparkleTheme.Summer => "MagnumOpus/Effects/LaCampanella/Sparkle/BellfireSparkleShader",
                SparkleTheme.Autumn => "MagnumOpus/Effects/EnigmaVariations/Sparkle/VoidSparkleShader",
                SparkleTheme.Winter => "MagnumOpus/Effects/MoonlightSonata/Sparkle/LunarSparkleShader",
                SparkleTheme.Seasons => "MagnumOpus/Effects/SwanLake/Sparkle/PrismaticSparkleShader",
                _ => "MagnumOpus/Effects/SwanLake/Sparkle/PrismaticSparkleShader",
            };

            _themeShaders[idx] = ModContent.Request<Effect>(path, AssetRequestMode.ImmediateLoad).Value;
            return _themeShaders[idx];
        }

        /// <summary>
        /// Draws a sparkle bloom effect — the primary replacement for excessive SoftGlow stacking.
        /// 
        /// Creates a field of twinkling 4-point star sparkles around a center point,
        /// with a subtle SoftGlow underlayer and tiny hot core. Each sparkle has independent
        /// flash timing for asynchronous twinkling.
        ///
        /// The sparkles are rendered through the theme's unique sparkle shader, which adds
        /// angular shimmer, prismatic effects, and flash peaks specific to the theme's identity.
        /// </summary>
        /// <param name="sb">SpriteBatch (must already be in additive blend mode)</param>
        /// <param name="worldPos">Center position in world coordinates</param>
        /// <param name="theme">Which theme's sparkle shader to use</param>
        /// <param name="colors">Theme color palette (primary, secondary, core, accent, warm)</param>
        /// <param name="intensity">Overall brightness 0-1</param>
        /// <param name="radius">Sparkle spread radius in pixels</param>
        /// <param name="sparkleCount">Number of sparkles to draw (4-16 typical)</param>
        /// <param name="time">Current time for animation (Main.timeForVisualEffects)</param>
        /// <param name="seed">Per-instance unique seed for sparkle positions</param>
        /// <param name="sparkleScale">Base scale of sparkle sprites (0.02-0.08 typical)</param>
        public static void DrawSparkleBloom(SpriteBatch sb, Vector2 worldPos, SparkleTheme theme,
            Color[] colors, float intensity, float radius, int sparkleCount, float time,
            float seed = 0f, float sparkleScale = 0.04f)
        {
            EnsureTextures();
            Vector2 screenPos = worldPos - Main.screenPosition;

            // Clamp parameters
            intensity = MathHelper.Clamp(intensity, 0f, 1f);
            sparkleCount = Math.Clamp(sparkleCount, 2, 24);

            // ---- LAYER 1: Subtle ambient SoftGlow (much smaller than before) ----
            Texture2D softGlow = _softGlow.Value;
            Vector2 glowOrigin = softGlow.Size() / 2f;
            float glowScale = MathHelper.Min(radius * 0.4f / softGlow.Width, 0.10f);
            sb.Draw(softGlow, screenPos, null, colors[0] * (intensity * 0.15f),
                0f, glowOrigin, glowScale, SpriteEffects.None, 0f);

            // ---- LAYER 2: Shader-driven sparkle field ----
            Effect shader = GetThemeShader(theme);
            if (shader != null)
            {
                DrawShaderSparkleField(sb, screenPos, shader, colors, intensity,
                    radius, sparkleCount, time, seed, sparkleScale);
            }
            else
            {
                // Fallback: non-shader sparkle field
                DrawFallbackSparkleField(sb, screenPos, colors, intensity,
                    radius, sparkleCount, time, seed, sparkleScale);
            }

            // ---- LAYER 3: Tiny hot core point ----
            Texture2D ptBloom = _pointBloom.Value;
            Vector2 ptOrigin = ptBloom.Size() / 2f;
            float coreScale = MathHelper.Min(sparkleScale * 0.4f, 0.035f);
            sb.Draw(ptBloom, screenPos, null, Color.White * (intensity * 0.4f),
                0f, ptOrigin, coreScale, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Draws a sparkle trail effect — replaces per-trail-point SoftGlow stacking.
        /// For each trail position, draws 1-3 small twinkling sparkles instead of 
        /// 2-3 SoftGlow layers at each point.
        /// </summary>
        public static void DrawSparkleTrail(SpriteBatch sb, Vector2[] trailPositions,
            int trailCount, SparkleTheme theme, Color[] colors, float intensity,
            float time, float baseScale = 0.025f)
        {
            EnsureTextures();

            Texture2D star4 = _star4Point.Value;
            Texture2D bright2 = _brightStar2.Value;
            Texture2D softGlow = _softGlow.Value;
            Vector2 star4Origin = star4.Size() / 2f;
            Vector2 bright2Origin = bright2.Size() / 2f;
            Vector2 glowOrigin = softGlow.Size() / 2f;

            for (int i = 0; i < trailCount; i++)
            {
                float progress = (float)i / trailCount;
                float alpha = (1f - progress) * intensity;
                if (alpha < 0.02f) break;

                Vector2 pos = trailPositions[i] - Main.screenPosition;
                float pointSeed = i * 2.37f;

                // Sin-wave flash for twinkling
                float flash = MathF.Max(0f, MathF.Sin(time * 0.12f + pointSeed));
                flash = flash * flash * flash; // Cubic sharpening

                float visAlpha = alpha * (0.15f + flash * 0.85f);
                float scale = baseScale * (1f - progress * 0.5f);

                // Tiny ambient glow (much smaller than original SoftGlow stacking)
                float glowScale = scale * 2f;
                sb.Draw(softGlow, pos, null, colors[0] * (visAlpha * 0.15f),
                    0f, glowOrigin, glowScale, SpriteEffects.None, 0f);

                // Main sparkle star
                Color sparkleColor = colors[i % colors.Length];
                float rot = time * 0.06f + pointSeed;
                sb.Draw(star4, pos, null, sparkleColor * visAlpha,
                    rot, star4Origin, scale, SpriteEffects.None, 0f);

                // Counter-rotated overlay for depth (every other point)
                if (i % 2 == 0 && flash > 0.3f)
                {
                    sb.Draw(bright2, pos, null, colors[Math.Min(2, colors.Length - 1)] * (visAlpha * 0.5f),
                        -rot * 0.7f, bright2Origin, scale * 0.6f, SpriteEffects.None, 0f);
                }

                // Core flash at peaks
                if (flash > 0.5f)
                {
                    Texture2D ptBloom = _pointBloom.Value;
                    Vector2 ptOrigin = ptBloom.Size() / 2f;
                    float corePow = (flash - 0.5f) / 0.5f;
                    sb.Draw(ptBloom, pos, null, Color.White * (corePow * visAlpha * 0.3f),
                        0f, ptOrigin, scale * 0.5f, SpriteEffects.None, 0f);
                }
            }
        }

        /// <summary>
        /// Draws a sparkle impact burst — replaces multi-layer impact bloom stacking.
        /// Instead of 4-6 SoftGlow layers, renders an expanding ring of sparkles with
        /// a brief center flash.
        /// </summary>
        public static void DrawSparkleImpact(SpriteBatch sb, Vector2 worldPos, SparkleTheme theme,
            Color[] colors, float progress, float intensity, float radius, float time,
            float seed = 0f, int sparkleCount = 12, float sparkleScale = 1f)
        {
            EnsureTextures();
            Vector2 screenPos = worldPos - Main.screenPosition;

            float fadeAlpha = 1f - progress * progress; // Quadratic fade
            if (fadeAlpha < 0.02f) return;

            // ---- BRIEF CENTER FLASH (first 30% of progress) ----
            if (progress < 0.3f)
            {
                float flashAlpha = 1f - (progress / 0.3f);
                Texture2D starburst = _starburstFlare.Value;
                Vector2 burstOrigin = starburst.Size() / 2f;
                float burstScale = MathHelper.Min((0.03f + progress * 0.04f), 0.06f);
                float burstRot = seed + progress * 2f;

                sb.Draw(starburst, screenPos, null, colors[Math.Min(2, colors.Length - 1)] * (flashAlpha * intensity * 0.5f),
                    burstRot, burstOrigin, burstScale, SpriteEffects.None, 0f);

                // Hot core point
                Texture2D ptBloom = _pointBloom.Value;
                Vector2 ptOrigin = ptBloom.Size() / 2f;
                sb.Draw(ptBloom, screenPos, null, Color.White * (flashAlpha * intensity * 0.6f),
                    0f, ptOrigin, 0.02f * (1f - progress), SpriteEffects.None, 0f);
            }

            // ---- EXPANDING SPARKLE RING ----
            Texture2D star4 = _star4Point.Value;
            Texture2D bright1 = _brightStar1.Value;
            Texture2D bright2 = _brightStar2.Value;
            Texture2D softGlow = _softGlow.Value;
            Vector2 star4Origin = star4.Size() / 2f;
            Vector2 bright1Origin = bright1.Size() / 2f;
            Vector2 bright2Origin = bright2.Size() / 2f;
            Vector2 glowOrigin = softGlow.Size() / 2f;

            float expandRadius = radius * progress;

            for (int i = 0; i < sparkleCount; i++)
            {
                float angle = (i / (float)sparkleCount) * MathHelper.TwoPi + seed;
                float pointSeed = seed + i * 3.17f;
                Vector2 offset = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * expandRadius;

                // Per-sparkle flash timing
                float flash = MathF.Max(0f, MathF.Sin(time * (0.1f + i * 0.015f) + pointSeed));
                flash = MathF.Pow(flash, 4f); // Sharp twinkle peaks

                float visAlpha = fadeAlpha * intensity * (0.1f + flash * 0.9f);
                if (visAlpha < 0.02f) continue;

                // Select texture type
                Texture2D tex;
                Vector2 origin;
                float baseScale;
                int typeRoll = (i * 7 + (int)(seed * 100)) % 3;
                switch (typeRoll)
                {
                    case 0: tex = star4; origin = star4Origin; baseScale = 0.035f * sparkleScale; break;
                    case 1: tex = bright1; origin = bright1Origin; baseScale = 0.03f * sparkleScale; break;
                    default: tex = bright2; origin = bright2Origin; baseScale = 0.025f * sparkleScale; break;
                }

                float pointScale = baseScale * (1f - progress * 0.3f) * (0.5f + flash * 0.5f);
                float rot = time * 0.08f + pointSeed;

                // Glow backdrop
                float glowScale = pointScale * 2.5f;
                Color glowColor = colors[i % colors.Length];
                sb.Draw(softGlow, screenPos + offset, null, glowColor * (visAlpha * 0.2f),
                    0f, glowOrigin, glowScale, SpriteEffects.None, 0f);

                // Star body
                Color starColor = colors[i % colors.Length];
                sb.Draw(tex, screenPos + offset, null, starColor * visAlpha,
                    rot, origin, pointScale, SpriteEffects.None, 0f);

                // White core at flash peak
                if (flash > 0.4f)
                {
                    float corePow = (flash - 0.4f) / 0.6f;
                    sb.Draw(softGlow, screenPos + offset, null, Color.White * (corePow * visAlpha * 0.3f),
                        0f, glowOrigin, pointScale * 0.6f, SpriteEffects.None, 0f);
                }
            }
        }

        // =====================================================================
        // SHADER-DRIVEN SPARKLE FIELD
        // =====================================================================

        /// <summary>
        /// Draws sparkles using the theme's unique shader for crystalline shimmer.
        /// Requires switching to Immediate SpriteSortMode to bind shader.
        /// </summary>
        private static void DrawShaderSparkleField(SpriteBatch sb, Vector2 screenPos,
            Effect shader, Color[] colors, float intensity, float radius,
            int sparkleCount, float time, float seed, float sparkleScale)
        {
            // End current batch, switch to Immediate for per-draw shader params
            sb.End();
            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            float shaderTime = time * 0.02f;
            shader.Parameters["uTime"]?.SetValue(shaderTime);
            shader.Parameters["highlightColor"]?.SetValue(Color.White.ToVector3());

            Texture2D star4 = _star4Point.Value;
            Texture2D bright1 = _brightStar1.Value;
            Texture2D bright2 = _brightStar2.Value;
            Vector2 star4Origin = star4.Size() / 2f;
            Vector2 bright1Origin = bright1.Size() / 2f;
            Vector2 bright2Origin = bright2.Size() / 2f;

            for (int i = 0; i < sparkleCount; i++)
            {
                float angle = (i / (float)sparkleCount) * MathHelper.TwoPi + seed * 6.28f;
                float dist = radius * (0.3f + 0.7f * HashFloat(seed + i * 1.37f));
                Vector2 offset = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * dist;

                float pointSeed = seed + i * 2.71f;
                float flashPhase = pointSeed * MathHelper.TwoPi;
                float flashSpeed = 0.12f + HashFloat(pointSeed + 3.14f) * 0.2f;

                // Flash timing
                float flash = MathF.Max(0f, MathF.Sin(time * flashSpeed + flashPhase));
                flash = MathF.Pow(flash, 4f);

                float visAlpha = intensity * (0.08f + flash * 0.92f);
                if (visAlpha < 0.02f) continue;

                // Select star texture
                int texType = ((int)(pointSeed * 100f) % 3 + 3) % 3;
                Texture2D tex;
                Vector2 origin;
                switch (texType)
                {
                    case 0: tex = star4; origin = star4Origin; break;
                    case 1: tex = bright1; origin = bright1Origin; break;
                    default: tex = bright2; origin = bright2Origin; break;
                }

                // Set per-sparkle shader parameters
                Color baseColor = colors[i % colors.Length];
                int accentIdx = (i + 2) % colors.Length;

                shader.Parameters["flashPhase"]?.SetValue(flashPhase);
                shader.Parameters["flashSpeed"]?.SetValue(flashSpeed * 50f);
                shader.Parameters["flashPower"]?.SetValue(4f + HashFloat(pointSeed) * 2f);
                shader.Parameters["baseAlpha"]?.SetValue(visAlpha);
                shader.Parameters["shimmerIntensity"]?.SetValue(0.6f + flash * 0.4f);
                shader.Parameters["primaryColor"]?.SetValue(baseColor.ToVector3());
                shader.Parameters["accentColor"]?.SetValue(colors[accentIdx].ToVector3());

                shader.CurrentTechnique.Passes[0].Apply();

                float scale = sparkleScale * (0.5f + flash * 0.5f);
                float rot = time * 0.05f + pointSeed;

                sb.Draw(tex, screenPos + offset, null, Color.White,
                    rot, origin, scale, SpriteEffects.None, 0f);
            }

            // Return to Deferred additive
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Non-shader fallback sparkle field for when shaders fail to load.
        /// </summary>
        private static void DrawFallbackSparkleField(SpriteBatch sb, Vector2 screenPos,
            Color[] colors, float intensity, float radius, int sparkleCount,
            float time, float seed, float sparkleScale)
        {
            Texture2D star4 = _star4Point.Value;
            Texture2D softGlow = _softGlow.Value;
            Vector2 star4Origin = star4.Size() / 2f;
            Vector2 glowOrigin = softGlow.Size() / 2f;

            for (int i = 0; i < sparkleCount; i++)
            {
                float angle = (i / (float)sparkleCount) * MathHelper.TwoPi + seed * 6.28f;
                float dist = radius * (0.3f + 0.7f * HashFloat(seed + i * 1.37f));
                Vector2 offset = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * dist;

                float pointSeed = seed + i * 2.71f;
                float flash = MathF.Max(0f, MathF.Sin(time * (0.12f + HashFloat(pointSeed) * 0.2f) + pointSeed));
                flash = flash * flash * flash * flash;

                float visAlpha = intensity * (0.08f + flash * 0.92f);
                if (visAlpha < 0.02f) continue;

                Color c = new Color(255, 0, 50); // NEON RED fallback!
                float scale = sparkleScale * (0.5f + flash * 0.5f);
                float rot = time * 0.05f + pointSeed;

                // Glow backdrop
                sb.Draw(softGlow, screenPos + offset, null, c * (visAlpha * 0.2f),
                    0f, glowOrigin, scale * 2f, SpriteEffects.None, 0f);

                // Star
                sb.Draw(star4, screenPos + offset, null, c * visAlpha,
                    rot, star4Origin, scale, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Simple deterministic hash for per-sparkle variation.
        /// Returns 0.0 to 1.0.
        /// </summary>
        private static float HashFloat(float input)
        {
            float x = MathF.Sin(input * 12.9898f + 78.233f) * 43758.5453f;
            return x - MathF.Floor(x);
        }
    }

    /// <summary>
    /// Identifies which theme's sparkle shader to use.
    /// </summary>
    public enum SparkleTheme
    {
        MoonlightSonata = 0,
        Eroica = 1,
        SwanLake = 2,
        LaCampanella = 3,
        EnigmaVariations = 4,
        Fate = 5,
        Spring = 6,
        Summer = 7,
        Autumn = 8,
        Winter = 9,
        Seasons = 10,
    }
}
