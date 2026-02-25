using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.LaCampanella
{
    /// <summary>
    /// Shared La Campanella VFX library — canonical palette, bloom stacking,
    /// shader setup, trail helpers, music notes, dust, smoke, bell chime,
    /// and impact VFX used by ALL La Campanella weapons, accessories,
    /// projectiles, minions, and enemies.
    /// </summary>
    public static class LaCampanellaVFXLibrary
    {
        // ─────────── CANONICAL PALETTE (delegates to LaCampanellaPalette) ───────────
        // 6-colour musical dynamic scale (pianissimo → sforzando)
        public static readonly Color SootBlack       = LaCampanellaPalette.SootBlack;       // [0] Pianissimo
        public static readonly Color DeepEmber       = LaCampanellaPalette.DeepEmber;       // [1] Piano
        public static readonly Color InfernalOrange  = LaCampanellaPalette.InfernalOrange;  // [2] Mezzo
        public static readonly Color FlameYellow     = LaCampanellaPalette.FlameYellow;     // [3] Forte
        public static readonly Color BellGold        = LaCampanellaPalette.BellGold;        // [4] Fortissimo
        public static readonly Color WhiteHot        = LaCampanellaPalette.WhiteHot;        // [5] Sforzando

        // Extended convenience
        public static readonly Color BellBronze   = LaCampanellaPalette.BellBronze;
        public static readonly Color ChimeShimmer = LaCampanellaPalette.ChimeShimmer;
        public static readonly Color SmokeGray    = LaCampanellaPalette.SmokeGray;
        public static readonly Color EmberRed     = LaCampanellaPalette.EmberRed;
        public static readonly Color MoltenCore   = LaCampanellaPalette.MoltenCore;

        // Palette as array for indexed access
        private static readonly Color[] Palette = {
            LaCampanellaPalette.SootBlack, LaCampanellaPalette.DeepEmber,
            LaCampanellaPalette.InfernalOrange, LaCampanellaPalette.FlameYellow,
            LaCampanellaPalette.BellGold, LaCampanellaPalette.WhiteHot
        };

        // Hue range for HueShiftingMusicNoteParticle (orange-gold band)
        private const float HueMin = 0.05f;
        private const float HueMax = 0.13f;
        private const float NoteSaturation = 0.9f;
        private const float NoteLuminosity = 0.55f;

        // Glow profile for GlowRenderer
        public static readonly GlowRenderer.GlowLayer[] LaCampanellaGlowProfile = new[]
        {
            new GlowRenderer.GlowLayer(1.0f, 1.0f, Color.White),
            new GlowRenderer.GlowLayer(1.6f, 0.65f, LaCampanellaPalette.BellGold),
            new GlowRenderer.GlowLayer(2.5f, 0.4f, LaCampanellaPalette.InfernalOrange),
            new GlowRenderer.GlowLayer(4.0f, 0.2f, LaCampanellaPalette.DeepEmber)
        };

        // ─────────── PALETTE INTERPOLATION ───────────

        /// <summary>
        /// Lerp through the 6-colour La Campanella palette. t=0 → SootBlack, t=1 → WhiteHot.
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
        /// Switch SpriteBatch to additive blend for La Campanella VFX rendering.
        /// Call EndLaCampanellaAdditive when done.
        /// </summary>
        public static void BeginLaCampanellaAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Restore SpriteBatch to standard AlphaBlend after additive rendering.
        /// </summary>
        public static void EndLaCampanellaAdditive(SpriteBatch sb)
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
        /// paletteT: 0=soot black, 0.4=infernal orange, 0.8=bell gold, 1=white-hot
        /// </summary>
        public static void DrawLaCampanellaBloomStack(SpriteBatch sb, Vector2 worldPos,
            float scale, float paletteT = 0.3f, float opacity = 1f)
        {
            Texture2D bloom = MagnumTextureRegistry.GetBloom();
            if (bloom == null) return;

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = bloom.Size() * 0.5f;

            Color outer = GetPaletteColor(MathHelper.Clamp(paletteT - 0.15f, 0f, 1f));
            Color inner = GetPaletteColor(MathHelper.Clamp(paletteT + 0.2f, 0f, 1f));

            // Layer 1: Outer halo (DeepEmber-ish)
            sb.Draw(bloom, drawPos, null,
                (outer with { A = 0 }) * 0.3f * opacity, 0f, origin, scale * 2.0f, SpriteEffects.None, 0f);

            // Layer 2: Mid glow (InfernalOrange)
            sb.Draw(bloom, drawPos, null,
                (outer with { A = 0 }) * 0.5f * opacity, 0f, origin, scale * 1.4f, SpriteEffects.None, 0f);

            // Layer 3: Inner bloom (FlameYellow/BellGold)
            sb.Draw(bloom, drawPos, null,
                (inner with { A = 0 }) * 0.7f * opacity, 0f, origin, scale * 0.9f, SpriteEffects.None, 0f);

            // Layer 4: White-hot core
            sb.Draw(bloom, drawPos, null,
                (Color.White with { A = 0 }) * 0.85f * opacity, 0f, origin, scale * 0.4f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Two-color bloom stack using {A=0} pattern.
        /// </summary>
        public static void DrawLaCampanellaBloomStack(SpriteBatch sb, Vector2 worldPos,
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
                // Behind layer: larger, softer, DeepEmber
                sb.Draw(bloom, drawPos, null,
                    (DeepEmber with { A = 0 }) * 0.25f * opacity, 0f, origin, scale * 2.5f, SpriteEffects.None, 0f);
                sb.Draw(bloom, drawPos, null,
                    (InfernalOrange with { A = 0 }) * 0.35f * opacity, 0f, origin, scale * 1.6f, SpriteEffects.None, 0f);
            }
            else
            {
                // Front layer: smaller, brighter, BellGold → White
                sb.Draw(bloom, drawPos, null,
                    (BellGold with { A = 0 }) * 0.5f * opacity, 0f, origin, scale * 0.8f, SpriteEffects.None, 0f);
                sb.Draw(bloom, drawPos, null,
                    (Color.White with { A = 0 }) * 0.7f * opacity, 0f, origin, scale * 0.35f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Counter-rotating double flare — two bloom textures spinning in opposite directions.
        /// Creates dynamic bell-fire energy appearance at projectile centers.
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
                (BellGold with { A = 0 }) * 0.6f * opacity, rot1, origin, scale * 0.7f, SpriteEffects.None, 0f);
            sb.Draw(flare, drawPos, null,
                (InfernalOrange with { A = 0 }) * 0.5f * opacity, rot2, origin, scale * 0.5f, SpriteEffects.None, 0f);
        }

        // ─────────── SELF-CONTAINED BLOOM (MANAGES OWN SPRITEBATCH) ───────────

        /// <summary>
        /// Standard La Campanella bloom at a blade tip or projectile centre.
        /// Uses the two-colour overload (DeepEmber outer → BellGold inner).
        /// Safe to call from PreDraw — handles SpriteBatch state internally.
        /// </summary>
        public static void DrawBloom(Vector2 worldPos, float scale, float opacity = 1f)
        {
            BloomRenderer.DrawBloomStackAdditive(worldPos, DeepEmber, BellGold, scale, opacity);
        }

        /// <summary>
        /// Combo-step-aware bloom (bigger + brighter on later hits).
        /// </summary>
        public static void DrawComboBloom(Vector2 worldPos, int comboStep, float baseScale = 0.4f, float opacity = 1f)
        {
            float scale = baseScale + comboStep * 0.07f;
            DrawBloom(worldPos, scale, opacity);
        }

        // ─────────── GLOW RENDERER INTEGRATION ───────────

        /// <summary>
        /// Draw La Campanella-themed multi-layer glow via GlowRenderer.
        /// Caller must have SpriteBatch in additive blend.
        /// </summary>
        public static void DrawLaCampanellaGlow(SpriteBatch sb, Vector2 worldPos,
            float scale = 1f, float intensity = 1f, string rotationId = null)
        {
            GlowRenderer.DrawGlow(sb, worldPos, LaCampanellaGlowProfile, InfernalOrange, intensity * scale, rotationId);
        }

        /// <summary>
        /// Draw La Campanella glow with automatic SpriteBatch state management.
        /// Safe to call from any context.
        /// </summary>
        public static void DrawLaCampanellaGlowManaged(SpriteBatch sb, Vector2 worldPos,
            float scale = 1f, float intensity = 1f, string rotationId = null)
        {
            GlowRenderer.DrawGlowManaged(sb, worldPos, LaCampanellaGlowProfile, InfernalOrange, intensity * scale, rotationId);
        }

        // ─────────── SHADER SETUP HELPERS ───────────

        /// <summary>
        /// Configure HeroicFlameTrail.fx shader parameters for infernal trail rendering.
        /// La Campanella reuses the heroic flame shader with fire-bell parameters.
        /// Call after EnterShaderRegion, before drawing trail geometry.
        /// </summary>
        public static void ApplyInfernalTrailShader(float time, Color primary, Color secondary,
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
            shader.Parameters["uHasSecondaryTex"]?.SetValue(0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(3f);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(0.5f);

            shader.CurrentTechnique = shader.Techniques["HeroicFlameFlow"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Configure HeroicFlameTrail.fx with noise texture bound to sampler 1.
        /// </summary>
        public static void ApplyInfernalTrailShaderWithNoise(float time, Color primary, Color secondary,
            float scrollSpeed = 1.5f, float distortionAmt = 0.08f, float overbrightMult = 3f,
            float noiseScale = 3f, float noiseScroll = 0.5f)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("NoiseSmoke");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("SoftCircularCaustics");
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
            shader.Parameters["uHasSecondaryTex"]?.SetValue(noise != null ? 1f : 0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(noiseScale);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(noiseScroll);

            shader.CurrentTechnique = shader.Techniques["HeroicFlameFlow"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Configure RadialScrollShader for bell ring shockwave effects.
        /// </summary>
        public static void ApplyBellRingShader(float time, Color primary, Color secondary,
            float scrollSpeed = 2f, float overbrightMult = 3f)
        {
            Effect shader = ShaderLoader.RadialScroll;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(1.5f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);

            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Configure ScrollingTrailShader for general bell-fire trail rendering.
        /// </summary>
        public static void ApplyScrollingTrailShader(float time, Color primary, Color secondary,
            float scrollSpeed = 1.5f, float overbrightMult = 2.5f)
        {
            Effect shader = ShaderLoader.ScrollingTrail;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(1.5f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);

            shader.CurrentTechnique.Passes[0].Apply();
        }

        // ─────────── TRAIL WIDTH/COLOR FUNCTIONS ───────────
        // These return values compatible with CalamityStyleTrailRenderer.

        /// <summary>
        /// Standard La Campanella trail width: wide at head, tapers to smoky tail.
        /// </summary>
        public static float InfernalTrailWidth(float completionRatio, float baseWidth = 18f)
        {
            float tipFade = MathF.Pow(1f - completionRatio, 0.6f);
            float headFade = MathF.Pow(completionRatio, 3f);
            return baseWidth * tipFade * (1f - headFade);
        }

        /// <summary>
        /// Sharp fang trail for FangOfTheInfiniteBell — precise bell-metal cut.
        /// </summary>
        public static float FangTrailWidth(float completionRatio, float baseWidth = 10f)
        {
            float taper = 1f - completionRatio;
            return baseWidth * taper * taper;
        }

        /// <summary>
        /// Thick fire comet trail for ranger weapons — heavy, smoky.
        /// </summary>
        public static float CometTrailWidth(float completionRatio, float baseWidth = 16f)
        {
            float tipFade = MathF.Pow(1f - completionRatio, 0.4f);
            return baseWidth * tipFade;
        }

        /// <summary>
        /// Trail color function with {A=0} for additive rendering under AlphaBlend.
        /// Gradient from ember at edges to white-pushed bell gold center along trail.
        /// </summary>
        public static Color InfernalTrailColor(float completionRatio, float whitePush = 0.45f)
        {
            float t = 0.3f + completionRatio * 0.5f;
            Color baseCol = GetPaletteColorWithWhitePush(t, whitePush * (1f - completionRatio));
            float fade = 1f - MathF.Pow(completionRatio, 1.5f);
            return (baseCol * fade) with { A = 0 };
        }

        /// <summary>
        /// Returns a pair of Vector3 colours for shader gradient uniforms.
        /// passIndex selects which pair from the palette to use for multi-pass rendering.
        /// Pass 0: DeepEmber → InfernalOrange, Pass 1: FlameYellow → BellGold, Pass 2: BellGold → WhiteHot
        /// </summary>
        public static (Vector3 primary, Vector3 secondary) GetShaderGradient(int passIndex)
        {
            return passIndex switch
            {
                0 => (DeepEmber.ToVector3(), InfernalOrange.ToVector3()),
                1 => (FlameYellow.ToVector3(), BellGold.ToVector3()),
                2 => (BellGold.ToVector3(), WhiteHot.ToVector3()),
                _ => (InfernalOrange.ToVector3(), BellGold.ToVector3()),
            };
        }

        // ─────────── MUSIC NOTES ───────────

        /// <summary>
        /// Spawn visible, hue-shifting La Campanella music notes at the given position.
        /// Notes use the canonical orange-gold hue band (0.05-0.13) and are spawned
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

        // ─────────── DUST HELPERS ───────────

        /// <summary>
        /// Dense infernal dust trail at a blade tip during a swing.
        /// </summary>
        public static void SpawnSwingDust(Vector2 pos, Vector2 awayDirection, int dustType = DustID.Torch)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = awayDirection * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(0.5f, 0.5f);
                Color col = GetPaletteColor(Main.rand.NextFloat(0.2f, 0.8f));
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, 0, col, 1.5f);
                d.noGravity = true;
            }
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
                Color col = Color.Lerp(DeepEmber, BellGold, progress);
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, 0, col, 1.3f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Ember scatter dust — small fiery particles drifting from impacts and swings.
        /// </summary>
        public static void SpawnEmberScatter(Vector2 pos, int count = 5, float speed = 2f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(speed, speed) + new Vector2(0, -1f);
                Color col = Color.Lerp(EmberRed, FlameYellow, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 0.9f + Main.rand.NextFloat(0.4f));
                d.noGravity = true;
                d.fadeIn = 0.8f;
            }
        }

        /// <summary>
        /// Contrasting bronze sparkle dust — call every other frame for dual-colour trail.
        /// </summary>
        public static void SpawnContrastSparkle(Vector2 pos, Vector2 awayDirection)
        {
            if (!Main.rand.NextBool(2)) return;
            Dust d = Dust.NewDustPerfect(pos, DustID.Enchanted_Gold,
                awayDirection * 0.5f + Main.rand.NextVector2Circular(1.5f, 1.5f), 0, BellBronze, 1.0f);
            d.noGravity = true;
        }

        // ─────────── SMOKE HELPERS ───────────

        /// <summary>
        /// Spawn heavy smoke particles using HeavySmokeParticle.
        /// Critical for La Campanella's smoky atmosphere identity.
        /// </summary>
        public static void SpawnHeavySmoke(Vector2 pos, int count = 3, float scale = 1f,
            float speed = 1.5f, int lifetime = 60)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(speed, speed) + new Vector2(0, -0.5f);
                Color col = LaCampanellaPalette.GetSmokeGradient(Main.rand.NextFloat());

                var smoke = new HeavySmokeParticle(
                    pos + Main.rand.NextVector2Circular(10f, 10f),
                    vel, col, lifetime + Main.rand.Next(20),
                    scale * (0.8f + Main.rand.NextFloat(0.4f)),
                    0.98f
                );
                MagnumParticleHandler.SpawnParticle(smoke);
            }
        }

        /// <summary>
        /// Spawn ambient smoke drift around an entity.
        /// Lighter, slower version for idle effects.
        /// </summary>
        public static void SpawnAmbientSmoke(Vector2 pos, float radius = 30f)
        {
            if (!Main.rand.NextBool(4)) return;

            Vector2 smokePos = pos + Main.rand.NextVector2Circular(radius, radius);
            Vector2 vel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -0.4f - Main.rand.NextFloat(0.3f));
            Color col = LaCampanellaPalette.GetSmokeGradient(Main.rand.NextFloat(0.3f, 0.7f));

            var smoke = new HeavySmokeParticle(
                smokePos, vel, col, 80 + Main.rand.Next(40),
                0.5f + Main.rand.NextFloat(0.3f),
                0.99f
            );
            MagnumParticleHandler.SpawnParticle(smoke);
        }

        // ─────────── GRADIENT HALO RINGS ───────────

        /// <summary>
        /// Cascading gradient halo rings (DeepEmber → BellGold).
        /// Creates bell ring shockwave visual.
        /// </summary>
        public static void SpawnGradientHaloRings(Vector2 pos, int count = 5, float baseScale = 0.3f)
        {
            for (int i = 0; i < count; i++)
            {
                float progress = (float)i / count;
                Color ringCol = Color.Lerp(DeepEmber, BellGold, progress);
                CustomParticles.LaCampanellaBellChime(pos, 6 + i * 2);
            }
        }

        /// <summary>
        /// Bell chime ring burst — concentric expanding golden rings.
        /// The signature La Campanella impact motif.
        /// </summary>
        public static void SpawnBellChimeRings(Vector2 pos, int ringCount = 3, float baseScale = 0.3f)
        {
            for (int i = 0; i < ringCount; i++)
            {
                float progress = (float)i / ringCount;
                float delay = i * 0.15f;
                CustomParticles.LaCampanellaBellChime(pos, 8 + i * 3);
            }
        }

        // ─────────── IMPACTS ───────────

        /// <summary>
        /// Full La Campanella melee impact VFX — bloom flash, bell chime ring,
        /// radial dust burst, ember scatter, and music note scatter. Scales with combo step.
        /// </summary>
        public static void MeleeImpact(Vector2 pos, int comboStep = 0)
        {
            float bloomScale = 0.5f + comboStep * 0.1f;
            DrawBloom(pos, bloomScale);

            CustomParticles.LaCampanellaImpactBurst(pos, 6 + comboStep * 2);

            int dustCount = 8 + comboStep * 4;
            SpawnRadialDustBurst(pos, dustCount, 5f + comboStep);

            SpawnEmberScatter(pos, 4 + comboStep * 2, 3f);

            int noteCount = 2 + comboStep;
            SpawnMusicNotes(pos, noteCount, 25f);

            SpawnHeavySmoke(pos, 2 + comboStep, 0.6f, 2f, 40);

            Lighting.AddLight(pos, InfernalOrange.ToVector3() * (0.8f + comboStep * 0.15f));
        }

        /// <summary>
        /// Projectile death / on-kill VFX — bigger, flashier version of MeleeImpact.
        /// </summary>
        public static void ProjectileImpact(Vector2 pos, float intensity = 1f)
        {
            DrawBloom(pos, 0.6f * intensity);
            CustomParticles.LaCampanellaImpactBurst(pos, (int)(10 * intensity));
            SpawnMusicNotes(pos, 6, 30f * intensity, 0.75f, 1.1f, 30);
            SpawnRadialDustBurst(pos, 15, 7f * intensity);
            SpawnEmberScatter(pos, (int)(8 * intensity), 4f);
            SpawnHeavySmoke(pos, (int)(4 * intensity), 0.8f, 3f, 50);
            Lighting.AddLight(pos, BellGold.ToVector3() * 1.2f * intensity);
        }

        /// <summary>
        /// Bell shockwave impact — concentric ring burst with heavy bloom.
        /// Use for major impacts and bell-themed weapon specials.
        /// </summary>
        public static void BellShockwaveImpact(Vector2 pos, float intensity = 1f)
        {
            DrawBloom(pos, 0.8f * intensity);
            SpawnBellChimeRings(pos, 4, 0.4f * intensity);
            SpawnRadialDustBurst(pos, 20, 8f * intensity);
            SpawnEmberScatter(pos, (int)(12 * intensity), 5f);
            SpawnMusicNotes(pos, 8, 40f, 0.8f, 1.2f, 35);
            SpawnHeavySmoke(pos, (int)(6 * intensity), 1f, 4f, 60);
            Lighting.AddLight(pos, WhiteHot.ToVector3() * 1.5f * intensity);
        }

        // ─────────── SWING HELPERS ───────────

        /// <summary>
        /// Per-frame VFX to call from a swing projectile's AI().
        /// Handles dense fire dust trail, contrast sparkles, ember scatter,
        /// smoke, and periodic music notes.
        /// </summary>
        public static void SwingFrameVFX(Vector2 tipPos, Vector2 swordDirection, int comboStep,
            int timer, int dustType = DustID.Torch)
        {
            SpawnSwingDust(tipPos, -swordDirection, dustType);
            SpawnContrastSparkle(tipPos, -swordDirection);

            // Ember scatter every few frames
            if (timer % 3 == 0)
                SpawnEmberScatter(tipPos, 2, 1.5f);

            // Smoke trail
            if (timer % 4 == 0)
                SpawnAmbientSmoke(tipPos, 10f);

            if (timer % 5 == 0)
                SpawnMusicNotes(tipPos, 1, 10f, 0.7f, 0.9f, 25);

            Lighting.AddLight(tipPos, GetPaletteColor(0.4f + comboStep * 0.15f).ToVector3() * 0.6f);
        }

        // ─────────── FINISHER EFFECTS ───────────

        /// <summary>
        /// Finisher slam VFX — screen shake, massive bloom, bell shockwave,
        /// music note cascade, heavy smoke burst.
        /// </summary>
        public static void FinisherSlam(Vector2 pos, float intensity = 1f)
        {
            MagnumScreenEffects.AddScreenShake(8f * intensity);
            DrawBloom(pos, 0.8f * intensity);
            BellShockwaveImpact(pos, intensity);
            SpawnMusicNotes(pos, 6, 40f, 0.8f, 1.2f, 40);
            SpawnRadialDustBurst(pos, 20, 8f * intensity);
            SpawnHeavySmoke(pos, (int)(8 * intensity), 1.2f, 5f, 70);
            Lighting.AddLight(pos, WhiteHot.ToVector3() * 1.5f * intensity);
        }

        /// <summary>
        /// Infernal eruption — volcanic burst for special attacks and boss VFX.
        /// </summary>
        public static void InfernalEruption(Vector2 pos, float intensity = 1f)
        {
            MagnumScreenEffects.AddScreenShake(12f * intensity);
            DrawBloom(pos, 1.2f * intensity);

            // Massive radial burst
            SpawnRadialDustBurst(pos, 30, 10f * intensity);
            SpawnEmberScatter(pos, (int)(20 * intensity), 6f);

            // Bell chime cascade
            SpawnBellChimeRings(pos, 5, 0.5f * intensity);

            // Heavy smoke cloud
            SpawnHeavySmoke(pos, (int)(10 * intensity), 1.5f, 6f, 80);

            // Music note cascade
            SpawnMusicNotes(pos, 10, 50f, 0.8f, 1.3f, 45);

            Lighting.AddLight(pos, WhiteHot.ToVector3() * 2f * intensity);
        }

        // ─────────── DYNAMIC LIGHTING ───────────

        /// <summary>
        /// Add standard La Campanella ambient light at a position.
        /// </summary>
        public static void AddInfernalLight(Vector2 worldPos, float intensity = 0.6f)
        {
            Lighting.AddLight(worldPos, InfernalOrange.ToVector3() * intensity);
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
