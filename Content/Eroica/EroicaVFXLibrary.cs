using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using static MagnumOpus.Common.Systems.VFX.GodRaySystem;
using MagnumOpus.Common.Systems.VFX.Screen;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX.Bloom;

namespace MagnumOpus.Content.Eroica
{
    /// <summary>
    /// Shared Eroica VFX library — canonical palette, bloom stacking,
    /// shader setup, trail helpers, music notes, dust, and impact VFX used by
    /// ALL Eroica weapons, accessories, projectiles, minions, and enemies.
    ///
    /// Modeled after MoonlightVFXLibrary.cs. ALL Eroica content should call
    /// these methods instead of ThemedParticles, EnhancedThemedParticles,
    /// UnifiedVFX, or any other deprecated VFX API.
    /// </summary>
    public static class EroicaVFXLibrary
    {
        // ─────────── CANONICAL PALETTE (delegates to EroicaPalette) ───────────
        // 6-colour musical dynamic scale (pianissimo → sforzando)
        public static readonly Color Black      = EroicaPalette.Black;       // [0] Pianissimo
        public static readonly Color Scarlet    = EroicaPalette.Scarlet;     // [1] Piano
        public static readonly Color Crimson    = EroicaPalette.Crimson;     // [2] Mezzo
        public static readonly Color Gold       = EroicaPalette.Gold;        // [3] Forte
        public static readonly Color Sakura     = EroicaPalette.Sakura;      // [4] Fortissimo
        public static readonly Color HotCore    = EroicaPalette.HotCore;     // [5] Sforzando

        // Extended convenience
        public static readonly Color DeepScarlet = EroicaPalette.DeepScarlet;
        public static readonly Color Flame       = EroicaPalette.Flame;
        public static readonly Color OrangeGold  = EroicaPalette.OrangeGold;
        public static readonly Color PollenGold  = EroicaPalette.PollenGold;

        // Palette as array for indexed access
        private static readonly Color[] Palette = {
            EroicaPalette.Black, EroicaPalette.Scarlet, EroicaPalette.Crimson,
            EroicaPalette.Gold, EroicaPalette.Sakura, EroicaPalette.HotCore
        };

        // Hue range for HueShiftingMusicNoteParticle (scarlet→gold band)
        private const float HueMin = 0.0f;
        private const float HueMax = 0.12f;
        private const float NoteSaturation = 0.9f;
        private const float NoteLuminosity = 0.55f;

        // Sakura hue range for sakura-themed notes
        private const float SakuraHueMin = 0.90f;
        private const float SakuraHueMax = 0.98f;

        // Glow profile for GlowRenderer
        public static readonly GlowRenderer.GlowLayer[] EroicaGlowProfile = new[]
        {
            new GlowRenderer.GlowLayer(1.0f, 1.0f, Color.White),
            new GlowRenderer.GlowLayer(1.6f, 0.65f, EroicaPalette.Gold),
            new GlowRenderer.GlowLayer(2.5f, 0.4f, EroicaPalette.Scarlet),
            new GlowRenderer.GlowLayer(4.0f, 0.2f, EroicaPalette.DeepScarlet)
        };

        // ─────────── PALETTE INTERPOLATION ───────────

        /// <summary>
        /// Lerp through the 6-colour Eroica palette. t=0 → Black, t=1 → HotCore.
        /// </summary>
        public static Color GetPaletteColor(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float scaled = t * (Palette.Length - 1);
            int idx = (int)scaled;
            int next = Math.Min(idx + 1, Palette.Length - 1);
            return Color.Lerp(Palette[idx], Palette[next], scaled - idx);
        }

        /// <summary>
        /// Palette colour with Calamity-style white push for perceived brilliance.
        /// push=0 returns pure palette, push=1 returns full white.
        /// Typical usage: push 0.35-0.55 for trail/bloom cores.
        /// </summary>
        public static Color GetPaletteColorWithWhitePush(float t, float push)
        {
            Color baseCol = GetPaletteColor(t);
            return Color.Lerp(baseCol, Color.White, MathHelper.Clamp(push, 0f, 1f));
        }

        // ─────────── SPRITEBATCH STATE HELPERS ───────────

        /// <summary>
        /// Switch SpriteBatch to additive blend for Eroica VFX rendering.
        /// Call EndEroicaAdditive when done.
        /// </summary>
        public static void BeginEroicaAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Restore SpriteBatch to standard AlphaBlend after additive rendering.
        /// </summary>
        public static void EndEroicaAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);
        }

        // ─────────── BLOOM STACKING ({A=0} PATTERN) ───────────

        /// <summary>
        /// 4-layer bloom stack using {A=0} premultiplied alpha trick.
        /// Renders additively under AlphaBlend without SpriteBatch restart.
        /// Caller must already have SpriteBatch active.
        /// paletteT: 0=dark scarlet, 0.5=gold, 1=hot white
        /// </summary>
        public static void DrawEroicaBloomStack(SpriteBatch sb, Vector2 worldPos,
            float scale, float paletteT = 0.3f, float opacity = 1f)
        {
            Texture2D bloom = MagnumTextureRegistry.GetBloom();
            if (bloom == null) return;

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = bloom.Size() * 0.5f;

            Color outer = GetPaletteColor(MathHelper.Clamp(paletteT - 0.15f, 0f, 1f));
            Color inner = GetPaletteColor(MathHelper.Clamp(paletteT + 0.2f, 0f, 1f));

            // Layer 1: Outer halo (DeepScarlet-ish)
            sb.Draw(bloom, drawPos, null,
                (outer with { A = 0 }) * 0.3f * opacity, 0f, origin, scale * 2.0f, SpriteEffects.None, 0f);

            // Layer 2: Mid glow (Scarlet)
            sb.Draw(bloom, drawPos, null,
                (outer with { A = 0 }) * 0.5f * opacity, 0f, origin, scale * 1.4f, SpriteEffects.None, 0f);

            // Layer 3: Inner bloom (Gold)
            sb.Draw(bloom, drawPos, null,
                (inner with { A = 0 }) * 0.7f * opacity, 0f, origin, scale * 0.9f, SpriteEffects.None, 0f);

            // Layer 4: White-hot core
            sb.Draw(bloom, drawPos, null,
                (Color.White with { A = 0 }) * 0.85f * opacity, 0f, origin, scale * 0.4f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Two-color bloom stack using {A=0} pattern.
        /// </summary>
        public static void DrawEroicaBloomStack(SpriteBatch sb, Vector2 worldPos,
            Color outerColor, Color innerColor, float scale, float opacity = 1f)
        {
            Texture2D bloom = MagnumTextureRegistry.GetBloom();
            if (bloom == null) return;

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = bloom.Size() * 0.5f;

            sb.Draw(bloom, drawPos, null,
                (outerColor with { A = 0 }) * 0.3f * opacity, 0f, origin, scale * 2.0f, SpriteEffects.None, 0f);
            sb.Draw(bloom, drawPos, null,
                (outerColor with { A = 0 }) * 0.5f * opacity, 0f, origin, scale * 1.4f, SpriteEffects.None, 0f);
            sb.Draw(bloom, drawPos, null,
                (innerColor with { A = 0 }) * 0.7f * opacity, 0f, origin, scale * 0.9f, SpriteEffects.None, 0f);
            sb.Draw(bloom, drawPos, null,
                (Color.White with { A = 0 }) * 0.85f * opacity, 0f, origin, scale * 0.4f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Bloom sandwich layer — renders bloom BEHIND a projectile body for depth.
        /// Call before drawing the projectile sprite, then call again after for front glow.
        /// </summary>
        public static void DrawBloomSandwichLayer(SpriteBatch sb, Vector2 worldPos,
            float scale, float opacity, bool isFrontLayer)
        {
            Texture2D bloom = MagnumTextureRegistry.GetBloom();
            if (bloom == null) return;

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = bloom.Size() * 0.5f;

            if (!isFrontLayer)
            {
                // Behind layer: larger, softer, DeepScarlet
                sb.Draw(bloom, drawPos, null,
                    (DeepScarlet with { A = 0 }) * 0.25f * opacity, 0f, origin, scale * 2.5f, SpriteEffects.None, 0f);
                sb.Draw(bloom, drawPos, null,
                    (Scarlet with { A = 0 }) * 0.35f * opacity, 0f, origin, scale * 1.6f, SpriteEffects.None, 0f);
            }
            else
            {
                // Front layer: smaller, brighter, Gold → White
                sb.Draw(bloom, drawPos, null,
                    (Gold with { A = 0 }) * 0.5f * opacity, 0f, origin, scale * 0.8f, SpriteEffects.None, 0f);
                sb.Draw(bloom, drawPos, null,
                    (Color.White with { A = 0 }) * 0.7f * opacity, 0f, origin, scale * 0.35f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Counter-rotating double flare — two bloom textures spinning in opposite directions.
        /// Creates dynamic energy appearance at projectile centers.
        /// </summary>
        public static void DrawCounterRotatingFlares(SpriteBatch sb, Vector2 worldPos,
            float scale, float time, float opacity = 1f)
        {
            Texture2D flare = MagnumTextureRegistry.GetFlare();
            if (flare == null) return;

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = flare.Size() * 0.5f;

            float rot1 = time * 2.5f;
            float rot2 = -time * 1.8f;

            sb.Draw(flare, drawPos, null,
                (Gold with { A = 0 }) * 0.6f * opacity, rot1, origin, scale * 0.7f, SpriteEffects.None, 0f);
            sb.Draw(flare, drawPos, null,
                (Scarlet with { A = 0 }) * 0.5f * opacity, rot2, origin, scale * 0.5f, SpriteEffects.None, 0f);
        }

        // ─────────── SELF-CONTAINED BLOOM ───────────

        /// <summary>
        /// Standard Eroica bloom at a blade tip or projectile centre.
        /// Uses BloomRenderer for self-contained SpriteBatch management.
        /// </summary>
        public static void DrawBloom(Vector2 worldPos, float scale, float opacity = 1f)
        {
            BloomRenderer.DrawBloomStackAdditive(worldPos, DeepScarlet, Gold, scale, opacity);
        }

        /// <summary>
        /// Combo-step-aware bloom (bigger + brighter on later hits).
        /// </summary>
        public static void DrawComboBloom(Vector2 worldPos, int comboStep, float baseScale = 0.4f, float opacity = 1f)
        {
            float scale = baseScale + comboStep * 0.08f;
            DrawBloom(worldPos, scale, opacity);
        }

        // ─────────── GLOW RENDERER INTEGRATION ───────────

        /// <summary>
        /// Draw Eroica-themed multi-layer glow via GlowRenderer.
        /// Caller must have SpriteBatch in additive blend.
        /// </summary>
        public static void DrawEroicaGlow(SpriteBatch sb, Vector2 worldPos,
            float scale = 1f, float intensity = 1f, string rotationId = null)
        {
            GlowRenderer.DrawGlow(sb, worldPos, EroicaGlowProfile, Scarlet, intensity * scale, rotationId);
        }

        /// <summary>
        /// Draw Eroica glow with automatic SpriteBatch state management.
        /// </summary>
        public static void DrawEroicaGlowManaged(SpriteBatch sb, Vector2 worldPos,
            float scale = 1f, float intensity = 1f, string rotationId = null)
        {
            GlowRenderer.DrawGlowManaged(sb, worldPos, EroicaGlowProfile, Scarlet, intensity * scale, rotationId);
        }

        // ─────────── SHADER SETUP HELPERS ───────────

        /// <summary>
        /// Configure HeroicFlameTrail.fx shader parameters for trail rendering.
        /// Call after EnterShaderRegion, before drawing trail geometry.
        /// </summary>
        public static void ApplyHeroicFlameTrailShader(float time, Color primary, Color secondary,
            float scrollSpeed = 1.5f, float distortionAmt = 0.08f, float overbrightMult = 3f)
        {
            Effect shader = ShaderLoader.HeroicFlameTrail;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(1.5f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);
            shader.Parameters["uDistortionAmt"]?.SetValue(distortionAmt);
            shader.Parameters["uNoiseScale"]?.SetValue(3f);
            shader.Parameters["uHasSecondaryTex"]?.SetValue(0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(3f);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(0.5f);

            shader.CurrentTechnique = shader.Techniques["HeroicFlameFlow"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Configure HeroicFlameTrail.fx with noise texture bound to sampler 1.
        /// </summary>
        public static void ApplyHeroicFlameTrailShaderWithNoise(float time, Color primary, Color secondary,
            float scrollSpeed = 1.5f, float distortionAmt = 0.08f, float overbrightMult = 3f,
            float noiseScale = 3f, float noiseScroll = 0.5f)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("PerlinNoise");
            if (noise != null)
            {
                Main.graphics.GraphicsDevice.Textures[1] = noise;
                Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
            }

            Effect shader = ShaderLoader.HeroicFlameTrail;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(1.5f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);
            shader.Parameters["uDistortionAmt"]?.SetValue(distortionAmt);
            shader.Parameters["uNoiseScale"]?.SetValue(noiseScale);
            shader.Parameters["uHasSecondaryTex"]?.SetValue(noise != null ? 1f : 0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(noiseScale);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(noiseScroll);

            shader.CurrentTechnique = shader.Techniques["HeroicFlameFlow"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Configure SakuraBloom.fx shader for petal bloom overlays.
        /// </summary>
        public static void ApplySakuraBloomShader(float time, float phase,
            Color primary, Color secondary, float overbrightMult = 2f,
            float petalCount = 5f, float rotationSpeed = 0.5f)
        {
            Effect shader = ShaderLoader.SakuraBloom;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(1.5f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uPhase"]?.SetValue(MathHelper.Clamp(phase, 0f, 1f));
            shader.Parameters["uPetalCount"]?.SetValue(petalCount);
            shader.Parameters["uRotationSpeed"]?.SetValue(rotationSpeed);
            shader.Parameters["uHasSecondaryTex"]?.SetValue(0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(2f);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(0.3f);
            shader.Parameters["uDistortionAmt"]?.SetValue(0.1f);

            shader.CurrentTechnique = shader.Techniques["SakuraPetalBloom"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Configure EroicaFuneralTrail.fx for somber, smoky trail rendering.
        /// Used by FuneralPrayer, FuneralBlitzer, FuneralMarchInsignia VFX.
        /// </summary>
        public static void ApplyFuneralTrailShader(float time, Color primary, Color secondary,
            float scrollSpeed = 0.8f, float distortionAmt = 0.05f, float overbrightMult = 2.5f,
            float smokeIntensity = 0.6f, string technique = "FuneralFlameFlow")
        {
            Effect shader = ShaderLoader.EroicaFuneralTrail;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(1.3f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);
            shader.Parameters["uDistortionAmt"]?.SetValue(distortionAmt);
            shader.Parameters["uNoiseScale"]?.SetValue(2.5f);
            shader.Parameters["uHasSecondaryTex"]?.SetValue(0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(2.5f);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(0.4f);
            shader.Parameters["uSmokeIntensity"]?.SetValue(smokeIntensity);

            shader.CurrentTechnique = shader.Techniques[technique];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Configure EroicaFuneralTrail.fx with noise texture for richer smoke dissolution.
        /// </summary>
        public static void ApplyFuneralTrailShaderWithNoise(float time, Color primary, Color secondary,
            float scrollSpeed = 0.8f, float distortionAmt = 0.05f, float overbrightMult = 2.5f,
            float smokeIntensity = 0.6f, float noiseScale = 2.5f, float noiseScroll = 0.4f)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("NoiseSmoke");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("PerlinNoise");
            if (noise != null)
            {
                Main.graphics.GraphicsDevice.Textures[1] = noise;
                Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
            }

            Effect shader = ShaderLoader.EroicaFuneralTrail;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(1.3f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);
            shader.Parameters["uDistortionAmt"]?.SetValue(distortionAmt);
            shader.Parameters["uNoiseScale"]?.SetValue(noiseScale);
            shader.Parameters["uHasSecondaryTex"]?.SetValue(noise != null ? 1f : 0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(noiseScale);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(noiseScroll);
            shader.Parameters["uSmokeIntensity"]?.SetValue(smokeIntensity);

            shader.CurrentTechnique = shader.Techniques["FuneralFlameFlow"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Configure TriumphantFractalShader.fx for geometric hexagonal trail rendering.
        /// Used by TriumphantFractal weapon VFX.
        /// </summary>
        public static void ApplyFractalTrailShader(float time, Color primary, Color secondary,
            float scrollSpeed = 1.2f, float distortionAmt = 0.04f, float overbrightMult = 2.8f,
            float fractalDepth = 2.0f, float rotationSpeed = 0.3f, string technique = "FractalEnergyTrail")
        {
            Effect shader = ShaderLoader.TriumphantFractal;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(1.5f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);
            shader.Parameters["uDistortionAmt"]?.SetValue(distortionAmt);
            shader.Parameters["uNoiseScale"]?.SetValue(3f);
            shader.Parameters["uHasSecondaryTex"]?.SetValue(0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(3f);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(0.5f);
            shader.Parameters["uFractalDepth"]?.SetValue(fractalDepth);
            shader.Parameters["uRotationSpeed"]?.SetValue(rotationSpeed);

            shader.CurrentTechnique = shader.Techniques[technique];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Configure TriumphantFractalShader.fx with noise texture for organic variation.
        /// </summary>
        public static void ApplyFractalTrailShaderWithNoise(float time, Color primary, Color secondary,
            float scrollSpeed = 1.2f, float distortionAmt = 0.04f, float overbrightMult = 2.8f,
            float fractalDepth = 2.0f, float rotationSpeed = 0.3f,
            float noiseScale = 3f, float noiseScroll = 0.5f)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("PerlinNoise");
            if (noise != null)
            {
                Main.graphics.GraphicsDevice.Textures[1] = noise;
                Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
            }

            Effect shader = ShaderLoader.TriumphantFractal;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(1.5f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);
            shader.Parameters["uDistortionAmt"]?.SetValue(distortionAmt);
            shader.Parameters["uNoiseScale"]?.SetValue(noiseScale);
            shader.Parameters["uHasSecondaryTex"]?.SetValue(noise != null ? 1f : 0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(noiseScale);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(noiseScroll);
            shader.Parameters["uFractalDepth"]?.SetValue(fractalDepth);
            shader.Parameters["uRotationSpeed"]?.SetValue(rotationSpeed);

            shader.CurrentTechnique = shader.Techniques["FractalEnergyTrail"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Configure existing CelestialValorTrail.fx for CelestialValor weapon.
        /// </summary>
        public static void ApplyCelestialValorTrailShader(float time, Color primary, Color secondary,
            float scrollSpeed = 1.0f, float overbrightMult = 2.5f)
        {
            Effect shader = ShaderLoader.CelestialValorTrail;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(1.5f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);

            shader.CurrentTechnique.Passes[0].Apply();
        }

        // ─────────── TRAIL WIDTH/COLOR FUNCTIONS ───────────
        // These return values compatible with CalamityStyleTrailRenderer.

        /// <summary>
        /// Standard Heroic trail width: wide at head, tapers to tail. 14f base.
        /// </summary>
        public static float HeroicTrailWidth(float completionRatio, float baseWidth = 14f)
        {
            float tipFade = MathF.Pow(1f - completionRatio, 0.6f);
            float headFade = MathF.Pow(completionRatio, 3f);
            return baseWidth * tipFade * (1f - headFade);
        }

        /// <summary>
        /// Thin precision trail for piercing weapons — laser-sharp. 6f base.
        /// </summary>
        public static float PrecisionTrailWidth(float completionRatio, float baseWidth = 6f)
        {
            float taper = 1f - completionRatio;
            return baseWidth * taper * taper;
        }

        /// <summary>
        /// Thick comet trail for heavy slams — wide, impactful. 18f base.
        /// </summary>
        public static float CometTrailWidth(float completionRatio, float baseWidth = 18f)
        {
            float tipFade = MathF.Pow(1f - completionRatio, 0.4f);
            return baseWidth * tipFade;
        }

        /// <summary>
        /// Heroic trail color: Scarlet → Gold with {A=0} for additive rendering.
        /// </summary>
        public static Color HeroicTrailColor(float completionRatio, float whitePush = 0.40f)
        {
            float t = 0.2f + completionRatio * 0.5f;
            Color baseCol = Color.Lerp(Scarlet, Gold, completionRatio * 0.8f);
            baseCol = Color.Lerp(baseCol, Color.White, whitePush * (1f - completionRatio));
            float fade = 1f - MathF.Pow(completionRatio, 1.5f);
            return (baseCol * fade) with { A = 0 };
        }

        /// <summary>
        /// Sakura trail color: Sakura → PollenGold with {A=0}.
        /// </summary>
        public static Color SakuraTrailColor(float completionRatio, float whitePush = 0.35f)
        {
            Color baseCol = Color.Lerp(Sakura, PollenGold, completionRatio * 0.7f);
            baseCol = Color.Lerp(baseCol, Color.White, whitePush * (1f - completionRatio));
            float fade = 1f - MathF.Pow(completionRatio, 1.5f);
            return (baseCol * fade) with { A = 0 };
        }

        /// <summary>
        /// Funeral trail color: DeepScarlet → OrangeGold with {A=0}. Somber, heavy.
        /// </summary>
        public static Color FuneralTrailColor(float completionRatio, float whitePush = 0.30f)
        {
            Color baseCol = Color.Lerp(DeepScarlet, OrangeGold, completionRatio * 0.6f);
            baseCol = Color.Lerp(baseCol, Color.White, whitePush * (1f - completionRatio));
            float fade = 1f - MathF.Pow(completionRatio, 1.8f);
            return (baseCol * fade) with { A = 0 };
        }

        /// <summary>
        /// Returns shader gradient pairs for multi-pass rendering.
        /// Pass 0: DeepScarlet → Scarlet, Pass 1: Scarlet → Gold, Pass 2: Gold → HotCore
        /// </summary>
        public static (Vector3 primary, Vector3 secondary) GetShaderGradient(int passIndex)
        {
            return passIndex switch
            {
                0 => (DeepScarlet.ToVector3(), Scarlet.ToVector3()),
                1 => (Scarlet.ToVector3(), Gold.ToVector3()),
                2 => (Gold.ToVector3(), HotCore.ToVector3()),
                _ => (Scarlet.ToVector3(), Gold.ToVector3()),
            };
        }

        // ─────────── MUSIC NOTES ───────────

        /// <summary>
        /// Spawn visible, hue-shifting Eroica music notes at the given position.
        /// Notes use the canonical scarlet→gold hue band (0.0-0.12) and are spawned
        /// at scale 0.7f+ so they are clearly visible.
        /// </summary>
        public static void SpawnMusicNotes(Vector2 pos, int count = 3, float spread = 20f,
            float minScale = 0.7f, float maxScale = 1.0f, int lifetime = 35)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(spread, spread);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), -1.5f - Main.rand.NextFloat(1.5f));
                float scale = Main.rand.NextFloat(minScale, maxScale);

                var note = new HueShiftingMusicNoteParticle(
                    pos + offset, vel,
                    HueMin, HueMax,
                    NoteSaturation, NoteLuminosity,
                    scale, lifetime
                );
                MagnumParticleHandler.SpawnParticle(note);
            }
        }

        /// <summary>
        /// Spawn a single music note with precise control over velocity, color, scale.
        /// Wraps ThemedParticles.MusicNote for backward compat with same signature.
        /// </summary>
        public static void SpawnMusicNote(Vector2 pos, Vector2 vel, Color color,
            float scale = 0.22f, int lifetime = 45)
        {
            // Convert Color to hue for HueShiftingMusicNoteParticle
            float hue = 0.05f; // Default scarlet-gold midpoint
            if (color.R > 200 && color.G < 100) hue = 0.0f; // Scarlet
            else if (color.G > 200) hue = 0.12f; // Gold

            var note = new HueShiftingMusicNoteParticle(
                pos, vel,
                MathHelper.Clamp(hue - 0.03f, 0f, 1f),
                MathHelper.Clamp(hue + 0.03f, 0f, 1f),
                NoteSaturation, NoteLuminosity,
                Math.Max(scale, 0.7f), lifetime
            );
            MagnumParticleHandler.SpawnParticle(note);
        }

        /// <summary>
        /// Radial burst of music notes — notes fly outward in all directions.
        /// </summary>
        public static void MusicNoteBurst(Vector2 pos, Color color, int count = 12, float speed = 4f)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(speed * 0.6f, speed);
                SpawnMusicNote(pos, vel, color, Main.rand.NextFloat(0.7f, 1.0f), 35);
            }
        }

        /// <summary>
        /// Spawn orbiting music notes locked to a centre point (e.g. projectile).
        /// </summary>
        public static void SpawnOrbitingNotes(Vector2 centre, Vector2 hostVelocity,
            int noteCount = 3, float orbitRadius = 15f, float baseAngle = 0f)
        {
            for (int i = 0; i < noteCount; i++)
            {
                float angle = baseAngle + MathHelper.TwoPi * i / noteCount;
                Vector2 notePos = centre + angle.ToRotationVector2() * orbitRadius;
                Vector2 vel = hostVelocity * 0.8f;
                float scale = Main.rand.NextFloat(0.7f, 0.9f);

                var note = new HueShiftingMusicNoteParticle(
                    notePos, vel,
                    HueMin, HueMax,
                    NoteSaturation, NoteLuminosity,
                    scale, 30
                );
                MagnumParticleHandler.SpawnParticle(note);
            }
        }

        /// <summary>
        /// Spawn sakura-hue music notes (pink/rose range instead of scarlet/gold).
        /// </summary>
        public static void SpawnSakuraMusicNotes(Vector2 pos, int count = 3, float spread = 20f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(spread, spread);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-1f, 1f), -1.2f - Main.rand.NextFloat(1f));
                float scale = Main.rand.NextFloat(0.7f, 0.95f);

                var note = new HueShiftingMusicNoteParticle(
                    pos + offset, vel,
                    SakuraHueMin, SakuraHueMax,
                    0.85f, 0.60f,
                    scale, 30
                );
                MagnumParticleHandler.SpawnParticle(note);
            }
        }

        // ─────────── DUST HELPERS ───────────

        /// <summary>
        /// Dense Eroica dust trail at a blade tip during a swing.
        /// </summary>
        public static void SpawnSwingDust(Vector2 pos, Vector2 awayDirection, int dustType = DustID.Torch)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = awayDirection * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(0.5f, 0.5f);
                Color col = GetPaletteColor(Main.rand.NextFloat(0.2f, 0.7f));
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, 0, col, 1.5f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Sakura petal dust particles — pink/rose floating petals.
        /// Replaces ThemedParticles.SakuraPetals().
        /// </summary>
        public static void SpawnSakuraPetals(Vector2 pos, int count = 5, float spread = 40f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(spread, spread);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), -0.5f - Main.rand.NextFloat(1f));
                Color col = Color.Lerp(Sakura, EroicaPalette.SakuraPale, Main.rand.NextFloat());
                int dustType = Main.rand.NextBool(3) ? DustID.PinkTorch : DustID.Enchanted_Pink;
                Dust d = Dust.NewDustPerfect(pos + offset, dustType, vel, 0, col, Main.rand.NextFloat(1.2f, 1.8f));
                d.noGravity = true;
                d.fadeIn = Main.rand.NextFloat(0.8f, 1.2f);
            }
        }

        /// <summary>
        /// Heroic aura — pulsing ring of rising embers and flame dust.
        /// Replaces ThemedParticles.EroicaAura().
        /// </summary>
        public static void SpawnHeroicAura(Vector2 center, float radius = 40f)
        {
            for (int i = 0; i < 3; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 pos = center + angle.ToRotationVector2() * Main.rand.NextFloat(radius * 0.5f, radius);
                Vector2 vel = new Vector2(0, -Main.rand.NextFloat(0.5f, 1.5f));
                Color col = Color.Lerp(Scarlet, Gold, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, Main.rand.NextFloat(1.0f, 1.6f));
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Gold and flame sparkle particles.
        /// Replaces ThemedParticles.EroicaSparkles().
        /// </summary>
        public static void SpawnValorSparkles(Vector2 pos, int count = 8, float spread = 30f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(spread, spread);
                Vector2 vel = Main.rand.NextVector2Circular(1f, 1f);
                Color col = Color.Lerp(Gold, Flame, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos + offset, DustID.GoldFlame, vel, 0, col, Main.rand.NextFloat(0.8f, 1.4f));
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Directional ember sparks — fly in a specific direction with spread.
        /// Replaces ThemedParticles.EroicaSparks().
        /// </summary>
        public static void SpawnDirectionalSparks(Vector2 pos, Vector2 direction, int count = 6, float speed = 6f)
        {
            for (int i = 0; i < count; i++)
            {
                float spreadAngle = Main.rand.NextFloat(-0.5f, 0.5f);
                Vector2 vel = direction.RotatedBy(spreadAngle) * Main.rand.NextFloat(speed * 0.5f, speed);
                Color col = Color.Lerp(Scarlet, Gold, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, Main.rand.NextFloat(1.0f, 1.6f));
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Per-frame flame trail dust — single particle following a projectile.
        /// Replaces ThemedParticles.EroicaTrail().
        /// </summary>
        public static void SpawnFlameTrailDust(Vector2 pos, Vector2 velocity)
        {
            Vector2 vel = -velocity * 0.2f + Main.rand.NextVector2Circular(0.5f, 0.5f);
            Color col = Color.Lerp(Scarlet, Gold, Main.rand.NextFloat());
            Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, Main.rand.NextFloat(1.2f, 1.6f));
            d.noGravity = true;
        }

        /// <summary>
        /// Radial dust burst for on-hit / impact VFX.
        /// </summary>
        public static void SpawnRadialDustBurst(Vector2 pos, int count = 12,
            float speed = 5f, int dustType = DustID.Torch)
        {
            for (int i = 0; i < count; i++)
            {
                float progress = (float)i / count;
                float angle = MathHelper.TwoPi * i / count;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(speed * 0.6f, speed);
                Color col = Color.Lerp(DeepScarlet, Gold, progress);
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, 0, col, 1.3f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Musical accidental symbols (sharps/flats) in Eroica fire colors.
        /// Replaces ThemedParticles.EroicaAccidentals().
        /// </summary>
        public static void SpawnAccidentals(Vector2 pos, int count = 3, float spread = 20f)
        {
            // Use music notes with slightly different hue range for variety
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(spread, spread);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-1f, 1f), -1f - Main.rand.NextFloat(1f));

                var note = new HueShiftingMusicNoteParticle(
                    pos + offset, vel,
                    0.02f, 0.10f, // Slightly narrower scarlet-gold range
                    0.85f, 0.50f,
                    Main.rand.NextFloat(0.6f, 0.85f), 30,
                    0.03f, "accidental"
                );
                MagnumParticleHandler.SpawnParticle(note);
            }
        }

        /// <summary>
        /// Contrasting gold sparkle dust — call every other frame for dual-colour trail.
        /// </summary>
        public static void SpawnContrastSparkle(Vector2 pos, Vector2 awayDirection)
        {
            if (!Main.rand.NextBool(2)) return;
            Dust d = Dust.NewDustPerfect(pos, DustID.GoldFlame,
                awayDirection * 0.5f + Main.rand.NextVector2Circular(1.5f, 1.5f), 0, Gold, 1.0f);
            d.noGravity = true;
        }

        // ─────────── GRADIENT HALO RINGS ───────────

        /// <summary>
        /// Cascading gradient halo rings (DeepScarlet → Gold).
        /// </summary>
        public static void SpawnGradientHaloRings(Vector2 pos, int count = 5, float baseScale = 0.3f)
        {
            for (int i = 0; i < count; i++)
            {
                float progress = (float)i / count;
                Color ringCol = Color.Lerp(DeepScarlet, Gold, progress);
                float scale = baseScale + i * 0.12f;
                var ring = new BloomRingParticle(pos, Vector2.Zero, ringCol, scale, 25, 0.08f);
                MagnumParticleHandler.SpawnParticle(ring);
            }
        }

        // ─────────── IMPACTS ───────────

        /// <summary>
        /// Full Heroic impact VFX — bloom flash, halo cascade,
        /// radial dust burst, and music note scatter. Scales with intensity.
        /// Replaces ThemedParticles.EroicaImpact(), UnifiedVFX.Eroica.Impact(),
        /// UnifiedVFXBloom.Eroica.ImpactEnhanced().
        /// </summary>
        public static void HeroicImpact(Vector2 pos, float scale = 1f)
        {
            DrawBloom(pos, 0.5f * scale);

            int rings = (int)(3 + scale * 2);
            SpawnGradientHaloRings(pos, rings, 0.25f * scale);

            int dustCount = (int)(8 + scale * 6);
            SpawnRadialDustBurst(pos, dustCount, 5f * scale);

            int noteCount = (int)(2 + scale);
            SpawnMusicNotes(pos, noteCount, 25f * scale);

            CustomParticles.EroicaFlare(pos, 0.4f * scale);

            Lighting.AddLight(pos, Scarlet.ToVector3() * (0.8f + scale * 0.3f));
        }

        /// <summary>
        /// Expanding shockwave ring with bloom.
        /// Replaces ThemedParticles.EroicaShockwave().
        /// </summary>
        public static void Shockwave(Vector2 pos, float scale = 1f)
        {
            var ring = new BloomRingParticle(pos, Vector2.Zero, Scarlet, 0.5f * scale, 30, 0.15f * scale);
            MagnumParticleHandler.SpawnParticle(ring);

            var innerRing = new BloomRingParticle(pos, Vector2.Zero, Gold, 0.3f * scale, 25, 0.10f * scale);
            MagnumParticleHandler.SpawnParticle(innerRing);

            DrawBloom(pos, 0.3f * scale);
            Lighting.AddLight(pos, Gold.ToVector3() * scale);
        }

        /// <summary>
        /// Full Eroica melee impact VFX — scales with combo step.
        /// </summary>
        public static void MeleeImpact(Vector2 pos, int comboStep = 0)
        {
            float bloomScale = 0.5f + comboStep * 0.1f;
            DrawBloom(pos, bloomScale);

            int rings = 3 + comboStep;
            SpawnGradientHaloRings(pos, rings);

            int dustCount = 8 + comboStep * 4;
            SpawnRadialDustBurst(pos, dustCount, 5f + comboStep);

            int noteCount = 2 + comboStep;
            SpawnMusicNotes(pos, noteCount, 25f);

            CustomParticles.EroicaFlare(pos, 0.4f + comboStep * 0.08f);

            Lighting.AddLight(pos, Scarlet.ToVector3() * (0.8f + comboStep * 0.15f));
        }

        /// <summary>
        /// Musical burst impact with notes, sparkles, and optional clef.
        /// Replaces ThemedParticles.EroicaMusicalImpact().
        /// </summary>
        public static void MusicalImpact(Vector2 pos, float scale = 1f, bool enhanced = false)
        {
            int noteCount = enhanced ? 8 : 5;
            float noteSpread = enhanced ? 45f : 30f;
            SpawnMusicNotes(pos, noteCount, noteSpread, 0.8f, 1.2f, 40);
            SpawnValorSparkles(pos, enhanced ? 6 : 3, 25f * scale);
            HeroicImpact(pos, scale * 0.7f);
        }

        /// <summary>
        /// Projectile death / on-kill VFX — bigger, flashier version of HeroicImpact.
        /// </summary>
        public static void ProjectileImpact(Vector2 pos, float intensity = 1f)
        {
            DrawBloom(pos, 0.6f * intensity);
            SpawnGradientHaloRings(pos, 6, 0.3f * intensity);
            SpawnMusicNotes(pos, 6, 30f * intensity, 0.75f, 1.1f, 30);
            SpawnRadialDustBurst(pos, 15, 7f * intensity);
            CustomParticles.EroicaImpactBurst(pos, 10);
            Lighting.AddLight(pos, Gold.ToVector3() * 1.2f * intensity);
        }

        /// <summary>
        /// Bloom burst — quick expanding bloom flash.
        /// Replaces ThemedParticles.EroicaBloomBurst() and EnhancedThemedParticles.EroicaBloomBurstEnhanced().
        /// </summary>
        public static void BloomBurst(Vector2 pos, float scale = 1f)
        {
            DrawBloom(pos, 0.6f * scale);
            SpawnGradientHaloRings(pos, 4, 0.3f * scale);
            SpawnRadialDustBurst(pos, 8, 4f * scale);
            Lighting.AddLight(pos, Gold.ToVector3() * scale);
        }

        /// <summary>
        /// Bloom flare at position.
        /// Replaces EnhancedParticles.BloomFlare() for Eroica context.
        /// </summary>
        public static void BloomFlare(Vector2 pos, Color color, float scale = 0.55f,
            int lifetime = 18, int count = 3, float intensity = 0.85f)
        {
            CustomParticles.EroicaFlare(pos, scale);
            DrawBloom(pos, scale * 0.5f);
            Lighting.AddLight(pos, color.ToVector3() * intensity);
        }

        /// <summary>
        /// Halo burst — expanding halos with sakura accents.
        /// Replaces ThemedParticles.EroicaHaloBurst().
        /// </summary>
        public static void HaloBurst(Vector2 pos, float scale = 1f)
        {
            SpawnGradientHaloRings(pos, 5, 0.25f * scale);
            SpawnSakuraPetals(pos, 3, 30f * scale);
            DrawBloom(pos, 0.4f * scale);
            Lighting.AddLight(pos, Sakura.ToVector3() * 0.8f * scale);
        }

        /// <summary>
        /// Death heroic flash — massive final bloom + dust + notes on enemy/boss death.
        /// Replaces DynamicParticleEffects.EroicaDeathHeroicFlash().
        /// </summary>
        public static void DeathHeroicFlash(Vector2 pos, float scale = 1f)
        {
            DrawBloom(pos, 1.0f * scale);
            SpawnGradientHaloRings(pos, 8, 0.4f * scale);
            SpawnRadialDustBurst(pos, 25, 10f * scale);
            MusicNoteBurst(pos, Gold, 8, 5f * scale);
            SpawnSakuraPetals(pos, 10, 60f * scale);
            CustomParticles.EroicaFlare(pos, 0.6f * scale);
            MagnumScreenEffects.AddScreenShake(4f * scale);
            GodRaySystem.CreateBurst(pos, Scarlet, 8, 80f * scale, 35, GodRayStyle.Explosion, Gold);
            Lighting.AddLight(pos, HotCore.ToVector3() * 2f * scale);
        }

        // ─────────── SWING HELPERS ───────────

        /// <summary>
        /// Per-frame VFX to call from a swing projectile's AI().
        /// Handles dense dust trail, contrast sparkles, and periodic music notes.
        /// </summary>
        public static void SwingFrameVFX(Vector2 tipPos, Vector2 swordDirection, int comboStep,
            int timer, int dustType = DustID.Torch)
        {
            SpawnSwingDust(tipPos, -swordDirection, dustType);
            SpawnContrastSparkle(tipPos, -swordDirection);

            if (timer % 5 == 0)
                SpawnMusicNotes(tipPos, 1, 10f, 0.7f, 0.9f, 25);

            Lighting.AddLight(tipPos, GetPaletteColor(0.3f + comboStep * 0.15f).ToVector3() * 0.6f);
        }

        // ─────────── FINISHER EFFECTS ───────────

        /// <summary>
        /// Phase-3 / finisher slam VFX — screen shake, massive bloom, music note cascade.
        /// </summary>
        public static void FinisherSlam(Vector2 pos, float intensity = 1f)
        {
            MagnumScreenEffects.AddScreenShake(8f * intensity);
            DrawBloom(pos, 0.8f * intensity);
            SpawnGradientHaloRings(pos, 7, 0.35f * intensity);
            SpawnMusicNotes(pos, 6, 40f, 0.8f, 1.2f, 40);
            SpawnRadialDustBurst(pos, 20, 8f * intensity);
            SpawnSakuraPetals(pos, 8, 50f * intensity);
            GodRaySystem.CreateBurst(pos, Scarlet, 6, 100f * intensity, 40, GodRayStyle.Explosion, Gold);
            ScreenDistortionManager.TriggerRipple(pos, Scarlet, 0.8f * intensity, 25);
            Lighting.AddLight(pos, HotCore.ToVector3() * 1.5f * intensity);
        }

        // ─────────── MISC ───────────

        /// <summary>
        /// Dodge afterimage trail — scarlet/gold trailing dust.
        /// Replaces ThemedParticles.DodgeTrail().
        /// </summary>
        public static void DodgeTrail(Vector2 pos, Vector2 velocity)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = -velocity * 0.3f + Main.rand.NextVector2Circular(1f, 1f);
                Color col = Color.Lerp(Scarlet, Gold, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.Torch, vel, 0, col, Main.rand.NextFloat(1.0f, 1.5f));
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Teleport arrival burst — radial dust + bloom + notes.
        /// Replaces ThemedParticles.TeleportBurst().
        /// </summary>
        public static void TeleportBurst(Vector2 pos)
        {
            DrawBloom(pos, 0.5f);
            SpawnRadialDustBurst(pos, 12, 6f);
            SpawnMusicNotes(pos, 3, 20f);
            SpawnSakuraPetals(pos, 4, 30f);
            Lighting.AddLight(pos, Gold.ToVector3() * 1.2f);
        }

        /// <summary>
        /// Draw trail behind a projectile using {A=0} bloom pattern.
        /// Replaces MagnumVFX.DrawPrismaticGemTrail().
        /// </summary>
        public static void DrawProjectileTrail(SpriteBatch sb, Projectile proj, Color trailColor)
        {
            Texture2D bloom = MagnumTextureRegistry.GetBloom();
            if (bloom == null) return;

            Vector2 origin = bloom.Size() * 0.5f;

            for (int i = 0; i < proj.oldPos.Length; i++)
            {
                if (proj.oldPos[i] == Vector2.Zero) continue;

                float progress = (float)i / proj.oldPos.Length;
                float fade = 1f - progress;
                float scale = 0.3f * fade;

                Vector2 drawPos = proj.oldPos[i] + proj.Size * 0.5f - Main.screenPosition;
                Color col = Color.Lerp(trailColor, Gold, progress * 0.5f);

                sb.Draw(bloom, drawPos, null,
                    (col with { A = 0 }) * 0.5f * fade, 0f, origin, scale, SpriteEffects.None, 0f);
            }
        }

        // ─────────── DYNAMIC LIGHTING ───────────

        /// <summary>
        /// Add standard Eroica ambient light at a position.
        /// </summary>
        public static void AddEroicaLight(Vector2 worldPos, float intensity = 0.6f)
        {
            Lighting.AddLight(worldPos, Scarlet.ToVector3() * intensity);
        }

        /// <summary>
        /// Add palette-interpolated dynamic light. Higher t = brighter, more golden.
        /// </summary>
        public static void AddPaletteLighting(Vector2 worldPos, float paletteT, float intensity = 0.8f)
        {
            Color col = GetPaletteColor(paletteT);
            Lighting.AddLight(worldPos, col.ToVector3() * intensity);
        }
    }
}
