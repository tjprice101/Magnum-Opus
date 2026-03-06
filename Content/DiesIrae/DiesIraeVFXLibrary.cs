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

namespace MagnumOpus.Content.DiesIrae
{
    /// <summary>
    /// Shared Dies Irae VFX library — canonical palette, bloom stacking,
    /// shader setup, trail helpers, music notes, dust, bone ash, hellfire,
    /// judgment beams, and impact VFX used by ALL Dies Irae weapons, accessories,
    /// projectiles, minions, and enemies.
    /// </summary>
    public static class DiesIraeVFXLibrary
    {
        // ─────────── CANONICAL PALETTE (delegates to DiesIraePalette) ───────────
        // 6-colour musical dynamic scale (pianissimo → sforzando)
        public static readonly Color CharcoalBlack  = DiesIraePalette.CharcoalBlack;  // [0] Pianissimo
        public static readonly Color BloodRed       = DiesIraePalette.BloodRed;       // [1] Piano
        public static readonly Color InfernalRed    = DiesIraePalette.InfernalRed;    // [2] Mezzo
        public static readonly Color JudgmentGold   = DiesIraePalette.JudgmentGold;   // [3] Forte
        public static readonly Color BoneWhite      = DiesIraePalette.BoneWhite;      // [4] Fortissimo
        public static readonly Color WrathWhite     = DiesIraePalette.WrathWhite;     // [5] Sforzando

        // Extended convenience
        public static readonly Color EmberOrange    = DiesIraePalette.EmberOrange;
        public static readonly Color HellfireGold   = DiesIraePalette.HellfireGold;
        public static readonly Color DoomPurple     = DiesIraePalette.DoomPurple;
        public static readonly Color AshGray        = DiesIraePalette.AshGray;
        public static readonly Color SmolderingEmber = DiesIraePalette.SmolderingEmber;

        // Palette as array for indexed access
        private static readonly Color[] Palette = {
            DiesIraePalette.CharcoalBlack, DiesIraePalette.BloodRed,
            DiesIraePalette.InfernalRed, DiesIraePalette.JudgmentGold,
            DiesIraePalette.BoneWhite, DiesIraePalette.WrathWhite
        };

        // Hue range for HueShiftingMusicNoteParticle (blood-red to gold band)
        private const float HueMin = 0.97f;
        private const float HueMax = 0.08f;
        private const float NoteSaturation = 0.85f;
        private const float NoteLuminosity = 0.45f;

        // Glow profile for GlowRenderer
        public static readonly GlowRenderer.GlowLayer[] DiesIraeGlowProfile = new[]
        {
            new GlowRenderer.GlowLayer(1.0f, 1.0f, Color.White),
            new GlowRenderer.GlowLayer(1.6f, 0.65f, DiesIraePalette.JudgmentGold),
            new GlowRenderer.GlowLayer(2.5f, 0.4f, DiesIraePalette.InfernalRed),
            new GlowRenderer.GlowLayer(4.0f, 0.2f, DiesIraePalette.BloodRed)
        };

        // ─────────── PALETTE INTERPOLATION ───────────

        /// <summary>
        /// Lerp through the 6-colour Dies Irae palette. t=0 → CharcoalBlack, t=1 → WrathWhite.
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
        /// Switch SpriteBatch to additive blend for Dies Irae VFX rendering.
        /// Call EndDiesIraeAdditive when done.
        /// </summary>
        public static void BeginDiesIraeAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Restore SpriteBatch to standard AlphaBlend after additive rendering.
        /// </summary>
        public static void EndDiesIraeAdditive(SpriteBatch sb)
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
        /// paletteT: 0=charcoal black, 0.3=infernal red, 0.6=judgment gold, 1=wrath white
        /// </summary>
        public static void DrawDiesIraeBloomStack(SpriteBatch sb, Vector2 worldPos,
            float scale, float paletteT = 0.3f, float opacity = 1f)
        {
            Texture2D bloom = MagnumTextureRegistry.GetBloom();
            if (bloom == null) return;

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = bloom.Size() * 0.5f;

            Color outer = GetPaletteColor(MathHelper.Clamp(paletteT - 0.15f, 0f, 1f));
            Color inner = GetPaletteColor(MathHelper.Clamp(paletteT + 0.2f, 0f, 1f));

            // Layer 1: Outer halo (BloodRed-ish)
            sb.Draw(bloom, drawPos, null,
                (outer with { A = 0 }) * 0.3f * opacity, 0f, origin, scale * 2.0f, SpriteEffects.None, 0f);

            // Layer 2: Mid glow (InfernalRed)
            sb.Draw(bloom, drawPos, null,
                (outer with { A = 0 }) * 0.5f * opacity, 0f, origin, scale * 1.4f, SpriteEffects.None, 0f);

            // Layer 3: Inner bloom (JudgmentGold)
            sb.Draw(bloom, drawPos, null,
                (inner with { A = 0 }) * 0.7f * opacity, 0f, origin, scale * 0.9f, SpriteEffects.None, 0f);

            // Layer 4: White-hot core
            sb.Draw(bloom, drawPos, null,
                (Color.White with { A = 0 }) * 0.85f * opacity, 0f, origin, scale * 0.4f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Two-color bloom stack using {A=0} pattern.
        /// </summary>
        public static void DrawDiesIraeBloomStack(SpriteBatch sb, Vector2 worldPos,
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
                // Behind layer: larger, softer, BloodRed
                sb.Draw(bloom, drawPos, null,
                    (BloodRed with { A = 0 }) * 0.25f * opacity, 0f, origin, scale * 2.5f, SpriteEffects.None, 0f);
                sb.Draw(bloom, drawPos, null,
                    (InfernalRed with { A = 0 }) * 0.35f * opacity, 0f, origin, scale * 1.6f, SpriteEffects.None, 0f);
            }
            else
            {
                // Front layer: smaller, brighter, JudgmentGold → White
                sb.Draw(bloom, drawPos, null,
                    (JudgmentGold with { A = 0 }) * 0.5f * opacity, 0f, origin, scale * 0.8f, SpriteEffects.None, 0f);
                sb.Draw(bloom, drawPos, null,
                    (Color.White with { A = 0 }) * 0.7f * opacity, 0f, origin, scale * 0.35f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Counter-rotating double flare — two bloom textures spinning in opposite directions.
        /// Creates dynamic hellfire energy appearance at projectile centers.
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
                (InfernalRed with { A = 0 }) * 0.6f * opacity, rot1, origin, scale * 0.7f, SpriteEffects.None, 0f);
            sb.Draw(flare, drawPos, null,
                (JudgmentGold with { A = 0 }) * 0.5f * opacity, rot2, origin, scale * 0.5f, SpriteEffects.None, 0f);
        }

        // ─────────── SELF-CONTAINED BLOOM (MANAGES OWN SPRITEBATCH) ───────────

        /// <summary>
        /// Standard Dies Irae bloom at a blade tip or projectile centre.
        /// Uses the two-colour overload (BloodRed outer → JudgmentGold inner).
        /// Safe to call from PreDraw — handles SpriteBatch state internally.
        /// </summary>
        public static void DrawBloom(Vector2 worldPos, float scale, float opacity = 1f)
        {
            BloomRenderer.DrawBloomStackAdditive(worldPos, BloodRed, JudgmentGold, scale, opacity);
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
        /// Draw Dies Irae-themed multi-layer glow via GlowRenderer.
        /// Caller must have SpriteBatch in additive blend.
        /// </summary>
        public static void DrawDiesIraeGlow(SpriteBatch sb, Vector2 worldPos,
            float scale = 1f, float intensity = 1f, string rotationId = null)
        {
            GlowRenderer.DrawGlow(sb, worldPos, DiesIraeGlowProfile, InfernalRed, intensity * scale, rotationId);
        }

        /// <summary>
        /// Draw Dies Irae glow with automatic SpriteBatch state management.
        /// Safe to call from any context.
        /// </summary>
        public static void DrawDiesIraeGlowManaged(SpriteBatch sb, Vector2 worldPos,
            float scale = 1f, float intensity = 1f, string rotationId = null)
        {
            GlowRenderer.DrawGlowManaged(sb, worldPos, DiesIraeGlowProfile, InfernalRed, intensity * scale, rotationId);
        }

        // ─────────── SHADER SETUP HELPERS ───────────

        /// <summary>
        /// Configure HeroicFlameTrail.fx shader parameters for infernal hellfire trail rendering.
        /// Dies Irae reuses the heroic flame shader with blood-red hellfire parameters.
        /// Call after EnterShaderRegion, before drawing trail geometry.
        /// </summary>
        public static void ApplyHellfireTrailShader(float time, Color primary, Color secondary,
            float scrollSpeed = 1.2f, float distortionAmt = 0.10f, float overbrightMult = 3f)
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
        public static void ApplyHellfireTrailShaderWithNoise(float time, Color primary, Color secondary,
            float scrollSpeed = 1.2f, float distortionAmt = 0.10f, float overbrightMult = 3f,
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
        /// Configure RadialScrollShader for wrath pulse shockwave effects.
        /// </summary>
        public static void ApplyWrathPulseShader(float time, Color primary, Color secondary,
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
        /// Configure ScrollingTrailShader for general hellfire trail rendering.
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
        /// Standard Dies Irae trail width: wide at head, tapers to ashy tail.
        /// </summary>
        public static float HellfireTrailWidth(float completionRatio, float baseWidth = 18f)
        {
            float tipFade = MathF.Pow(1f - completionRatio, 0.6f);
            float headFade = MathF.Pow(completionRatio, 3f);
            return baseWidth * tipFade * (1f - headFade);
        }

        /// <summary>
        /// Sharp blade trail for melee weapons — precise judgment cut.
        /// </summary>
        public static float BladeTrailWidth(float completionRatio, float baseWidth = 12f)
        {
            float taper = 1f - completionRatio;
            return baseWidth * taper * taper;
        }

        /// <summary>
        /// Thick wrath trail for heavy attacks — devastating, smoky.
        /// </summary>
        public static float WrathTrailWidth(float completionRatio, float baseWidth = 20f)
        {
            float tipFade = MathF.Pow(1f - completionRatio, 0.4f);
            return baseWidth * tipFade;
        }

        /// <summary>
        /// Trail color function with {A=0} for additive rendering under AlphaBlend.
        /// Gradient from blood red at edges to white-pushed judgment gold center along trail.
        /// </summary>
        public static Color HellfireTrailColor(float completionRatio, float whitePush = 0.45f)
        {
            float t = 0.3f + completionRatio * 0.5f;
            Color baseCol = GetPaletteColorWithWhitePush(t, whitePush * (1f - completionRatio));
            float fade = 1f - MathF.Pow(completionRatio, 1.5f);
            return (baseCol * fade) with { A = 0 };
        }

        /// <summary>
        /// Returns a pair of Vector3 colours for shader gradient uniforms.
        /// passIndex selects which pair from the palette to use for multi-pass rendering.
        /// Pass 0: BloodRed → InfernalRed, Pass 1: InfernalRed → JudgmentGold, Pass 2: JudgmentGold → WrathWhite
        /// </summary>
        public static (Vector3 primary, Vector3 secondary) GetShaderGradient(int passIndex)
        {
            return passIndex switch
            {
                0 => (BloodRed.ToVector3(), InfernalRed.ToVector3()),
                1 => (InfernalRed.ToVector3(), JudgmentGold.ToVector3()),
                2 => (JudgmentGold.ToVector3(), WrathWhite.ToVector3()),
                _ => (InfernalRed.ToVector3(), JudgmentGold.ToVector3()),
            };
        }

        // ─────────── MUSIC NOTES ───────────

        /// <summary>
        /// Spawn visible, hue-shifting Dies Irae music notes at the given position.
        /// Notes use the canonical blood-red to gold hue band and are spawned
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
        /// Dense hellfire dust trail at a blade tip during a swing.
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
                Color col = Color.Lerp(BloodRed, JudgmentGold, progress);
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, 0, col, 1.3f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Ember scatter dust — fiery particles drifting from impacts and swings.
        /// </summary>
        public static void SpawnEmberScatter(Vector2 pos, int count = 5, float speed = 2f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(speed, speed) + new Vector2(0, -1f);
                Color col = Color.Lerp(SmolderingEmber, HellfireGold, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 0.9f + Main.rand.NextFloat(0.4f));
                d.noGravity = true;
                d.fadeIn = 0.8f;
            }
        }

        /// <summary>
        /// Bone ash scatter — pale off-white drifting particles, the signature of Dies Irae.
        /// Crumbling stone and bleached bone debris floating upward.
        /// </summary>
        public static void SpawnBoneAshScatter(Vector2 pos, int count = 4, float speed = 1.5f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(speed, speed) + new Vector2(0, -0.8f);
                Color col = DiesIraePalette.GetBoneGradient(Main.rand.NextFloat(0.3f, 1f));
                Dust d = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.TintableDust, vel, 150, col, 0.8f + Main.rand.NextFloat(0.3f));
                d.noGravity = true;
                d.fadeIn = 0.6f;
            }
        }

        /// <summary>
        /// Contrasting judgment gold sparkle dust — call every other frame for dual-colour trail.
        /// </summary>
        public static void SpawnContrastSparkle(Vector2 pos, Vector2 awayDirection)
        {
            if (!Main.rand.NextBool(2)) return;
            Dust d = Dust.NewDustPerfect(pos, DustID.Enchanted_Gold,
                awayDirection * 0.5f + Main.rand.NextVector2Circular(1.5f, 1.5f), 0, JudgmentGold, 1.0f);
            d.noGravity = true;
        }

        // ─────────── SMOKE / ASH HELPERS ───────────

        /// <summary>
        /// Spawn heavy smoke particles using HeavySmokeParticle.
        /// Dies Irae smoke is darker, thicker, more oppressive than other themes.
        /// </summary>
        public static void SpawnHeavySmoke(Vector2 pos, int count = 3, float scale = 1f,
            float speed = 1.5f, int lifetime = 60)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(speed, speed) + new Vector2(0, -0.5f);
                Color col = DiesIraePalette.GetBoneGradient(Main.rand.NextFloat(0f, 0.4f));

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
        /// Lighter, slower version for idle effects. Darker than other themes.
        /// </summary>
        public static void SpawnAmbientSmoke(Vector2 pos, float radius = 30f)
        {
            if (!Main.rand.NextBool(4)) return;

            Vector2 smokePos = pos + Main.rand.NextVector2Circular(radius, radius);
            Vector2 vel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -0.4f - Main.rand.NextFloat(0.3f));
            Color col = DiesIraePalette.GetBoneGradient(Main.rand.NextFloat(0f, 0.3f));

            var smoke = new HeavySmokeParticle(
                smokePos, vel, col, 80 + Main.rand.Next(40),
                0.5f + Main.rand.NextFloat(0.3f),
                0.99f
            );
            MagnumParticleHandler.SpawnParticle(smoke);
        }

        // ─────────── WRATH PULSE RINGS ───────────

        /// <summary>
        /// Cascading wrath pulse rings (BloodRed → JudgmentGold).
        /// Creates judgment shockwave visual — the Dies Irae signature motif.
        /// </summary>
        public static void SpawnWrathPulseRings(Vector2 pos, int count = 5, float baseScale = 0.3f)
        {
            for (int i = 0; i < count; i++)
            {
                CustomParticles.DiesIraeHellfireBurst(pos, 6 + i * 2);
            }
        }

        /// <summary>
        /// Judgment burst ring — concentric expanding blood-gold rings.
        /// The signature Dies Irae impact motif.
        /// </summary>
        public static void SpawnJudgmentRings(Vector2 pos, int ringCount = 3, float baseScale = 0.3f)
        {
            for (int i = 0; i < ringCount; i++)
            {
                CustomParticles.DiesIraeHellfireBurst(pos, 8 + i * 3);
            }
        }

        // ─────────── IMPACTS ───────────

        /// <summary>
        /// Full Dies Irae melee impact VFX — bloom flash, wrath pulse ring,
        /// radial dust burst, ember scatter, bone ash, and music note scatter.
        /// Scales with combo step.
        /// </summary>
        public static void MeleeImpact(Vector2 pos, int comboStep = 0)
        {
            float bloomScale = 0.5f + comboStep * 0.1f;
            DrawBloom(pos, bloomScale);

            CustomParticles.DiesIraeImpactBurst(pos, 6 + comboStep * 2);

            int dustCount = 8 + comboStep * 4;
            SpawnRadialDustBurst(pos, dustCount, 5f + comboStep);

            SpawnEmberScatter(pos, 4 + comboStep * 2, 3f);
            SpawnBoneAshScatter(pos, 2 + comboStep, 2f);

            int noteCount = 2 + comboStep;
            SpawnMusicNotes(pos, noteCount, 25f);

            SpawnHeavySmoke(pos, 2 + comboStep, 0.6f, 2f, 40);

            Lighting.AddLight(pos, InfernalRed.ToVector3() * (0.8f + comboStep * 0.15f));
        }

        /// <summary>
        /// Projectile death / on-kill VFX — bigger, flashier version of MeleeImpact.
        /// </summary>
        public static void ProjectileImpact(Vector2 pos, float intensity = 1f)
        {
            DrawBloom(pos, 0.6f * intensity);
            CustomParticles.DiesIraeImpactBurst(pos, (int)(10 * intensity));
            SpawnMusicNotes(pos, 6, 30f * intensity, 0.75f, 1.1f, 30);
            SpawnRadialDustBurst(pos, 15, 7f * intensity);
            SpawnEmberScatter(pos, (int)(8 * intensity), 4f);
            SpawnBoneAshScatter(pos, (int)(4 * intensity), 2.5f);
            SpawnHeavySmoke(pos, (int)(4 * intensity), 0.8f, 3f, 50);
            Lighting.AddLight(pos, JudgmentGold.ToVector3() * 1.2f * intensity);
        }

        /// <summary>
        /// Wrath shockwave impact — concentric ring burst with heavy bloom.
        /// Use for major impacts and wrath-themed weapon specials.
        /// </summary>
        public static void WrathShockwaveImpact(Vector2 pos, float intensity = 1f)
        {
            DrawBloom(pos, 0.8f * intensity);
            SpawnJudgmentRings(pos, 4, 0.4f * intensity);
            SpawnRadialDustBurst(pos, 20, 8f * intensity);
            SpawnEmberScatter(pos, (int)(12 * intensity), 5f);
            SpawnBoneAshScatter(pos, (int)(6 * intensity), 3f);
            SpawnMusicNotes(pos, 8, 40f, 0.8f, 1.2f, 35);
            SpawnHeavySmoke(pos, (int)(6 * intensity), 1f, 4f, 60);
            Lighting.AddLight(pos, WrathWhite.ToVector3() * 1.5f * intensity);
        }

        // ─────────── SWING HELPERS ───────────

        /// <summary>
        /// Per-frame VFX to call from a swing projectile's AI().
        /// Handles dense hellfire dust trail, contrast sparkles, ember scatter,
        /// bone ash, smoke, and periodic music notes.
        /// </summary>
        public static void SwingFrameVFX(Vector2 tipPos, Vector2 swordDirection, int comboStep,
            int timer, int dustType = DustID.Torch)
        {
            SpawnSwingDust(tipPos, -swordDirection, dustType);
            SpawnContrastSparkle(tipPos, -swordDirection);

            // Ember scatter every few frames
            if (timer % 3 == 0)
                SpawnEmberScatter(tipPos, 2, 1.5f);

            // Bone ash drift
            if (timer % 5 == 0)
                SpawnBoneAshScatter(tipPos, 1, 1f);

            // Smoke trail
            if (timer % 4 == 0)
                SpawnAmbientSmoke(tipPos, 10f);

            if (timer % 5 == 0)
                SpawnMusicNotes(tipPos, 1, 10f, 0.7f, 0.9f, 25);

            Lighting.AddLight(tipPos, GetPaletteColor(0.4f + comboStep * 0.15f).ToVector3() * 0.6f);
        }

        // ─────────── FINISHER EFFECTS ───────────

        /// <summary>
        /// Finisher slam VFX — screen shake, massive bloom, wrath shockwave,
        /// music note cascade, bone ash burst, heavy smoke burst.
        /// </summary>
        public static void FinisherSlam(Vector2 pos, float intensity = 1f)
        {
            MagnumScreenEffects.AddScreenShake(8f * intensity);
            DrawBloom(pos, 0.8f * intensity);
            WrathShockwaveImpact(pos, intensity);
            SpawnMusicNotes(pos, 6, 40f, 0.8f, 1.2f, 40);
            SpawnRadialDustBurst(pos, 20, 8f * intensity);
            SpawnBoneAshScatter(pos, (int)(8 * intensity), 3f);
            SpawnHeavySmoke(pos, (int)(8 * intensity), 1.2f, 5f, 70);
            Lighting.AddLight(pos, WrathWhite.ToVector3() * 1.5f * intensity);
        }

        /// <summary>
        /// Hellfire eruption — volcanic wrath burst for special attacks and boss VFX.
        /// The most intense VFX in the Dies Irae library.
        /// </summary>
        public static void HellfireEruption(Vector2 pos, float intensity = 1f)
        {
            MagnumScreenEffects.AddScreenShake(12f * intensity);
            DrawBloom(pos, 1.2f * intensity);

            // Massive radial burst
            SpawnRadialDustBurst(pos, 30, 10f * intensity);
            SpawnEmberScatter(pos, (int)(20 * intensity), 6f);
            SpawnBoneAshScatter(pos, (int)(10 * intensity), 4f);

            // Wrath pulse cascade
            SpawnJudgmentRings(pos, 5, 0.5f * intensity);

            // Heavy smoke cloud
            SpawnHeavySmoke(pos, (int)(10 * intensity), 1.5f, 6f, 80);

            // Music note cascade
            SpawnMusicNotes(pos, 10, 50f, 0.8f, 1.3f, 45);

            Lighting.AddLight(pos, WrathWhite.ToVector3() * 2f * intensity);
        }

        /// <summary>
        /// Judgment beam VFX — vertical column of divine wrath light.
        /// </summary>
        public static void JudgmentBeam(Vector2 basePos, float height = 200f, float intensity = 1f)
        {
            // Vertical judgment column
            int tiers = (int)(height / 8f);
            for (int i = 0; i < tiers; i++)
            {
                float h = i * 8f;
                Vector2 pos = basePos + new Vector2(Main.rand.NextFloat(-6f, 6f), -h);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -2f - Main.rand.NextFloat(2f));
                Color col = DiesIraePalette.GetJudgmentGradient((float)i / tiers);
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.3f * intensity);
                d.noGravity = true;
            }

            // Base wrath pulse
            CustomParticles.DiesIraeHellfireBurst(basePos, 8);
            DrawBloom(basePos, 0.5f * intensity);

            SpawnMusicNotes(basePos, 3, 15f, 0.7f, 1.0f, 35);
            SpawnBoneAshScatter(basePos, 4, 2f);

            MagnumScreenEffects.AddScreenShake(3f * intensity);
            Lighting.AddLight(basePos, JudgmentGold.ToVector3() * 1.0f * intensity);
        }

        // ─────────── DYNAMIC LIGHTING ───────────

        /// <summary>
        /// Add standard Dies Irae ambient light at a position.
        /// </summary>
        public static void AddHellfireLight(Vector2 worldPos, float intensity = 0.6f)
        {
            Lighting.AddLight(worldPos, InfernalRed.ToVector3() * intensity);
        }

        /// <summary>
        /// Add palette-interpolated dynamic light. Higher t = brighter, more golden.
        /// </summary>
        public static void AddPaletteLighting(Vector2 worldPos, float paletteT, float intensity = 0.8f)
        {
            Color col = GetPaletteColor(paletteT);
            Lighting.AddLight(worldPos, col.ToVector3() * intensity);
        }

        // ─────────── THEME TEXTURE VFX ───────────
        // Uses DiesIraeThemeTextures for theme-specific visuals
        // that go beyond the universal MagnumTextureRegistry blooms.

        /// <summary>
        /// Draws a themed judgment impact ring using DiesIrae Power Effect Ring + Harmonic Impact.
        /// Must be called while SpriteBatch is in Additive blend mode (or using {A=0} pattern).
        /// </summary>
        public static void DrawThemeImpactRing(SpriteBatch sb, Vector2 worldPos,
            float scale, float intensity = 1f, float rotation = 0f)
        {
            Vector2 drawPos = worldPos - Main.screenPosition;

            // Layer 1: Power Effect Ring — expanding concentric wrath ring
            Texture2D ring = DiesIraeThemeTextures.DIPowerEffectRing?.Value;
            if (ring != null)
            {
                Vector2 origin = ring.Size() * 0.5f;
                sb.Draw(ring, drawPos, null,
                    (BloodRed with { A = 0 }) * 0.55f * intensity, rotation, origin,
                    scale * 0.15f, SpriteEffects.None, 0f);
                sb.Draw(ring, drawPos, null,
                    (EmberOrange with { A = 0 }) * 0.35f * intensity, -rotation * 0.7f, origin,
                    scale * 0.10f, SpriteEffects.None, 0f);
            }

            // Layer 2: Harmonic Impact — shockwave overlay
            Texture2D impact = DiesIraeThemeTextures.DIHarmonicImpact?.Value;
            if (impact != null)
            {
                Vector2 impOrigin = impact.Size() * 0.5f;
                sb.Draw(impact, drawPos, null,
                    (JudgmentGold with { A = 0 }) * 0.5f * intensity, rotation * 1.3f, impOrigin,
                    scale * 0.12f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draws theme-specific hellfire star flares at a position using DI Star Flare textures.
        /// Must be called while SpriteBatch is in Additive blend mode (or using {A=0} pattern).
        /// </summary>
        public static void DrawThemeStarFlare(SpriteBatch sb, Vector2 worldPos,
            float scale, float intensity = 1f)
        {
            Vector2 drawPos = worldPos - Main.screenPosition;

            Texture2D flare = DiesIraeThemeTextures.DIStarFlare?.Value;
            if (flare != null)
            {
                Vector2 origin = flare.Size() * 0.5f;
                float rot = (float)Main.GameUpdateCount * 0.04f;
                sb.Draw(flare, drawPos, null,
                    (EmberOrange with { A = 0 }) * 0.5f * intensity, rot, origin,
                    scale * 0.08f, SpriteEffects.None, 0f);
            }

            Texture2D flare2 = DiesIraeThemeTextures.DIStarFlare2?.Value;
            if (flare2 != null)
            {
                Vector2 origin = flare2.Size() * 0.5f;
                float rot = -(float)Main.GameUpdateCount * 0.03f;
                sb.Draw(flare2, drawPos, null,
                    (JudgmentGold with { A = 0 }) * 0.35f * intensity, rot, origin,
                    scale * 0.06f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draws a themed radial slash burst for melee hit impacts using DI Radial Slash Star.
        /// Must be called in Additive blend mode.
        /// </summary>
        public static void DrawThemeRadialSlash(SpriteBatch sb, Vector2 worldPos,
            float scale, float intensity = 1f, float rotation = 0f)
        {
            Texture2D slashStar = DiesIraeThemeTextures.DIRadialSlashStar?.Value;
            if (slashStar == null) return;

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = slashStar.Size() * 0.5f;

            sb.Draw(slashStar, drawPos, null,
                (BloodRed with { A = 0 }) * 0.4f * intensity, rotation, origin,
                scale * 0.14f, SpriteEffects.None, 0f);
            sb.Draw(slashStar, drawPos, null,
                (EmberOrange with { A = 0 }) * 0.6f * intensity, -rotation * 0.5f, origin,
                scale * 0.08f, SpriteEffects.None, 0f);
            sb.Draw(slashStar, drawPos, null,
                (JudgmentGold with { A = 0 }) * 0.7f * intensity, rotation * 1.5f, origin,
                scale * 0.04f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Draws the DI Cracked Earth noise texture as a distortion overlay.
        /// Useful for hellfire ground impacts and judgment zones.
        /// Must be called in Additive blend mode.
        /// </summary>
        public static void DrawCrackedEarthOverlay(SpriteBatch sb, Vector2 worldPos,
            float scale, float intensity = 1f)
        {
            Texture2D noise = DiesIraeThemeTextures.DICrackedEarthNoise?.Value;
            if (noise == null) return;

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = noise.Size() * 0.5f;

            sb.Draw(noise, drawPos, null,
                (InfernalRed with { A = 0 }) * 0.3f * intensity, 0f, origin,
                scale * 0.1f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Combined theme bloom stack: universal bloom layers + theme star flare + impact ring.
        /// The "full package" for Dies Irae impacts. Manages its own SpriteBatch state.
        /// </summary>
        public static void DrawThemeImpactFull(SpriteBatch sb, Vector2 worldPos,
            float scale, float intensity = 1f, int comboStep = 0)
        {
            float stepMult = 1f + comboStep * 0.15f;
            float adjustedScale = scale * stepMult;
            float adjustedIntensity = intensity * stepMult;

            // Universal bloom layers
            DrawDiesIraeBloomStack(sb, worldPos, adjustedScale, 0.3f, adjustedIntensity);

            // Theme star flare
            DrawThemeStarFlare(sb, worldPos, adjustedScale, adjustedIntensity * 0.7f);

            // Theme impact ring
            float rot = (float)Main.GameUpdateCount * 0.02f;
            DrawThemeImpactRing(sb, worldPos, adjustedScale, adjustedIntensity * 0.6f, rot);
        }
    }
}
