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

namespace MagnumOpus.Content.EnigmaVariations
{
    /// <summary>
    /// Shared Enigma Variations VFX library — canonical palette, bloom stacking,
    /// shader setup, trail helpers, music notes, dust, eye spawns, glyph effects,
    /// and impact VFX used by ALL Enigma weapons, accessories, projectiles,
    /// minions, and enemies.
    /// </summary>
    public static class EnigmaVFXLibrary
    {
        // ─────────── CANONICAL PALETTE (forwarded from EnigmaPalette) ───────────
        // These are convenience aliases so calling code can use either
        // EnigmaVFXLibrary.Purple or EnigmaPalette.Purple interchangeably.
        public static readonly Color VoidBlack     = EnigmaPalette.VoidBlack;
        public static readonly Color DeepPurple    = EnigmaPalette.DeepPurple;
        public static readonly Color Purple        = EnigmaPalette.Purple;
        public static readonly Color GreenFlame    = EnigmaPalette.GreenFlame;
        public static readonly Color BrightGreen   = EnigmaPalette.BrightGreen;
        public static readonly Color WhiteGreenFlash = EnigmaPalette.WhiteGreenFlash;

        // Convenience accessors
        public static readonly Color EyeGreen      = EnigmaPalette.EyeGreen;
        public static readonly Color GlyphPurple   = EnigmaPalette.GlyphPurple;

        // Palette as array for indexed access
        private static readonly Color[] Palette = { VoidBlack, DeepPurple, Purple, GreenFlame, BrightGreen, WhiteGreenFlash };

        // Hue range for HueShiftingMusicNoteParticle (purple-green band)
        private const float HueMin = 0.28f;
        private const float HueMax = 0.45f;
        private const float NoteSaturation = 0.85f;
        private const float NoteLuminosity = 0.65f;

        // Enigma glow profile for GlowRenderer
        public static readonly GlowRenderer.GlowLayer[] EnigmaGlowProfile = new[]
        {
            new GlowRenderer.GlowLayer(1.0f, 1.0f, Color.White),
            new GlowRenderer.GlowLayer(1.6f, 0.65f, new Color(50, 220, 100)),   // GreenFlame
            new GlowRenderer.GlowLayer(2.5f, 0.4f, new Color(140, 60, 200)),    // Purple
            new GlowRenderer.GlowLayer(4.0f, 0.2f, new Color(80, 20, 120))      // DeepPurple
        };

        // ─────────── PALETTE INTERPOLATION ───────────

        /// <summary>
        /// Lerp through the 6-colour Enigma palette. t=0 -> VoidBlack, t=1 -> WhiteGreenFlash.
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
        /// </summary>
        public static Color GetPaletteColorWithWhitePush(float t, float push)
        {
            Color baseCol = GetPaletteColor(t);
            return Color.Lerp(baseCol, Color.White, MathHelper.Clamp(push, 0f, 1f));
        }

        /// <summary>
        /// Drop-in replacement for the GetEnigmaGradient() duplicated in every weapon file.
        /// VoidBlack -> Purple -> GreenFlame over 0->1.
        /// </summary>
        public static Color GetEnigmaGradient(float progress)
            => EnigmaPalette.GetEnigmaGradient(progress);

        // ─────────── SPRITEBATCH STATE HELPERS ───────────

        /// <summary>
        /// Switch SpriteBatch to additive blend for Enigma VFX rendering.
        /// Call EndEnigmaAdditive when done.
        /// </summary>
        public static void BeginEnigmaAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Restore SpriteBatch to standard AlphaBlend after additive rendering.
        /// </summary>
        public static void EndEnigmaAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Switch SpriteBatch to Immediate + Additive for shader-driven drawing.
        /// </summary>
        public static void BeginShaderAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                Main.Rasterizer, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Restore SpriteBatch to normal deferred alpha-blend mode.
        /// </summary>
        public static void RestoreSpriteBatch(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                Main.Rasterizer, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        // ─────────── BLOOM STACKING (DIRECT {A=0} PATTERN) ───────────

        /// <summary>
        /// 4-layer bloom stack using {A=0} premultiplied alpha trick.
        /// Renders additively under AlphaBlend without SpriteBatch restart.
        /// </summary>
        public static void DrawEnigmaBloomStack(SpriteBatch sb, Vector2 worldPos,
            float scale, float paletteT = 0.4f, float opacity = 1f)
        {
            Texture2D bloom = MagnumTextureRegistry.GetBloom();
            if (bloom == null) return;

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = bloom.Size() * 0.5f;

            Color outer = GetPaletteColor(MathHelper.Clamp(paletteT - 0.2f, 0f, 1f));
            Color inner = GetPaletteColor(MathHelper.Clamp(paletteT + 0.2f, 0f, 1f));

            // Layer 1: Outer halo (DeepPurple-ish)
            sb.Draw(bloom, drawPos, null,
                (outer with { A = 0 }) * 0.3f * opacity, 0f, origin, scale * 2.0f, SpriteEffects.None, 0f);

            // Layer 2: Mid glow (Purple)
            sb.Draw(bloom, drawPos, null,
                (outer with { A = 0 }) * 0.5f * opacity, 0f, origin, scale * 1.4f, SpriteEffects.None, 0f);

            // Layer 3: Inner bloom (GreenFlame)
            sb.Draw(bloom, drawPos, null,
                (inner with { A = 0 }) * 0.7f * opacity, 0f, origin, scale * 0.9f, SpriteEffects.None, 0f);

            // Layer 4: White-hot green core
            sb.Draw(bloom, drawPos, null,
                (WhiteGreenFlash with { A = 0 }) * 0.85f * opacity, 0f, origin, scale * 0.4f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Two-color bloom stack using {A=0} pattern.
        /// </summary>
        public static void DrawEnigmaBloomStack(SpriteBatch sb, Vector2 worldPos,
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
                // Behind layer: larger, softer, DeepPurple
                sb.Draw(bloom, drawPos, null,
                    (DeepPurple with { A = 0 }) * 0.25f * opacity, 0f, origin, scale * 2.5f, SpriteEffects.None, 0f);
                sb.Draw(bloom, drawPos, null,
                    (Purple with { A = 0 }) * 0.35f * opacity, 0f, origin, scale * 1.6f, SpriteEffects.None, 0f);
            }
            else
            {
                // Front layer: smaller, brighter, GreenFlame -> White
                sb.Draw(bloom, drawPos, null,
                    (GreenFlame with { A = 0 }) * 0.5f * opacity, 0f, origin, scale * 0.8f, SpriteEffects.None, 0f);
                sb.Draw(bloom, drawPos, null,
                    (Color.White with { A = 0 }) * 0.7f * opacity, 0f, origin, scale * 0.35f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Counter-rotating double flare — two bloom textures spinning in opposite directions.
        /// Creates dynamic arcane energy appearance at projectile centers.
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
                (GreenFlame with { A = 0 }) * 0.6f * opacity, rot1, origin, scale * 0.7f, SpriteEffects.None, 0f);
            sb.Draw(flare, drawPos, null,
                (Purple with { A = 0 }) * 0.5f * opacity, rot2, origin, scale * 0.5f, SpriteEffects.None, 0f);
        }

        // ─────────── SELF-CONTAINED BLOOM (MANAGES OWN SPRITEBATCH) ───────────

        /// <summary>
        /// Standard Enigma bloom at a blade tip or projectile centre.
        /// Uses the two-colour overload (DeepPurple outer -> GreenFlame inner).
        /// Safe to call from PreDraw — handles SpriteBatch state internally.
        /// </summary>
        public static void DrawBloom(Vector2 worldPos, float scale, float opacity = 1f)
        {
            BloomRenderer.DrawBloomStackAdditive(worldPos, DeepPurple, GreenFlame, scale, opacity);
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
        /// Draw Enigma-themed multi-layer glow via GlowRenderer.
        /// Caller must have SpriteBatch in additive blend.
        /// </summary>
        public static void DrawEnigmaGlow(SpriteBatch sb, Vector2 worldPos,
            float scale = 1f, float intensity = 1f, string rotationId = null)
        {
            GlowRenderer.DrawGlow(sb, worldPos, EnigmaGlowProfile, GreenFlame, intensity * scale, rotationId);
        }

        /// <summary>
        /// Draw Enigma glow with automatic SpriteBatch state management.
        /// Safe to call from any context.
        /// </summary>
        public static void DrawEnigmaGlowManaged(SpriteBatch sb, Vector2 worldPos,
            float scale = 1f, float intensity = 1f, string rotationId = null)
        {
            GlowRenderer.DrawGlowManaged(sb, worldPos, EnigmaGlowProfile, GreenFlame, intensity * scale, rotationId);
        }

        // ─────────── TRAIL WIDTH/COLOR FUNCTIONS ───────────
        // These return delegates compatible with CalamityStyleTrailRenderer.

        /// <summary>
        /// Standard Enigma trail width: wide at head, tapers to tail.
        /// </summary>
        public static float EnigmaTrailWidth(float completionRatio, float baseWidth = 18f)
        {
            float tipFade = MathF.Pow(1f - completionRatio, 0.6f);
            float headFade = MathF.Pow(completionRatio, 3f);
            return baseWidth * tipFade * (1f - headFade);
        }

        /// <summary>
        /// Thin precision trail for silent/precise weapons — surgical void cut.
        /// </summary>
        public static float PrecisionTrailWidth(float completionRatio, float baseWidth = 6f)
        {
            float taper = 1f - completionRatio;
            return baseWidth * taper * taper;
        }

        /// <summary>
        /// Thick void trail for heavy weapons — cascading mystery.
        /// </summary>
        public static float VoidTrailWidth(float completionRatio, float baseWidth = 16f)
        {
            float tipFade = MathF.Pow(1f - completionRatio, 0.4f);
            return baseWidth * tipFade;
        }

        /// <summary>
        /// Trail color function with {A=0} for additive rendering under AlphaBlend.
        /// Gradient from outer palette colour at edges to white-pushed center along trail.
        /// </summary>
        public static Color EnigmaTrailColor(float completionRatio, float whitePush = 0.45f)
        {
            float t = 0.3f + completionRatio * 0.5f;
            Color baseCol = GetPaletteColorWithWhitePush(t, whitePush * (1f - completionRatio));
            float fade = 1f - MathF.Pow(completionRatio, 1.5f);
            return (baseCol * fade) with { A = 0 };
        }

        /// <summary>
        /// Returns a pair of Vector3 colours for shader gradient uniforms.
        /// Pass 0: DeepPurple -> Purple, Pass 1: GreenFlame -> BrightGreen, Pass 2: BrightGreen -> WhiteGreenFlash
        /// </summary>
        public static (Vector3 primary, Vector3 secondary) GetShaderGradient(int passIndex)
        {
            return passIndex switch
            {
                0 => (DeepPurple.ToVector3(), Purple.ToVector3()),
                1 => (GreenFlame.ToVector3(), BrightGreen.ToVector3()),
                2 => (BrightGreen.ToVector3(), WhiteGreenFlash.ToVector3()),
                _ => (Purple.ToVector3(), GreenFlame.ToVector3()),
            };
        }

        // ─────────── MUSIC NOTES ───────────

        /// <summary>
        /// Spawn visible, hue-shifting Enigma music notes at the given position.
        /// Notes use the purple-green hue band (0.28-0.45) and are spawned
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
        /// Dense Enigma dust trail at a blade tip during a swing.
        /// Uses purple and green torch dust for the signature dual-color effect.
        /// </summary>
        public static void SpawnSwingDust(Vector2 pos, Vector2 awayDirection, int dustType = DustID.PurpleTorch)
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
        /// Enigma dual-color swing dust — alternating purple and green torches.
        /// </summary>
        public static void SpawnEnigmaSwingDust(Vector2 pos, Vector2 awayDirection)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = awayDirection * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(0.5f, 0.5f);
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.GreenTorch;
                Color col = dustType == DustID.PurpleTorch ? Purple : GreenFlame;
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, 0, col, 1.5f);
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }
        }

        /// <summary>
        /// Radial dust burst for on-hit / impact VFX.
        /// Gradient from DeepPurple to GreenFlame around the burst ring.
        /// </summary>
        public static void SpawnRadialDustBurst(Vector2 pos, int count = 12,
            float speed = 5f, int dustType = DustID.PurpleTorch)
        {
            for (int i = 0; i < count; i++)
            {
                float progress = (float)i / count;
                float angle = MathHelper.TwoPi * i / count;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(speed * 0.6f, speed);
                Color col = Color.Lerp(DeepPurple, GreenFlame, progress);
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, 0, col, 1.3f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Contrasting green flame sparkle dust — call every other frame for dual-colour trail.
        /// </summary>
        public static void SpawnContrastSparkle(Vector2 pos, Vector2 awayDirection)
        {
            if (!Main.rand.NextBool(2)) return;
            Dust d = Dust.NewDustPerfect(pos, DustID.GreenTorch,
                awayDirection * 0.5f + Main.rand.NextVector2Circular(1.5f, 1.5f), 0, GreenFlame, 1.0f);
            d.noGravity = true;
        }

        // ─────────── ENIGMA-SPECIFIC VFX: EYES ───────────

        /// <summary>
        /// Spawn watching eye particles around a position.
        /// The signature Enigma Variations visual identity.
        /// </summary>
        public static void SpawnWatchingEyes(Vector2 pos, int count = 3, float radius = 40f, float scale = 0.35f)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 eyePos = pos + angle.ToRotationVector2() * Main.rand.NextFloat(radius * 0.5f, radius);
                CustomParticles.EnigmaEyeGaze(eyePos, Purple * 0.8f, scale, null);
            }
        }

        /// <summary>
        /// Spawn a single watching eye that gazes toward a target.
        /// </summary>
        public static void SpawnGazingEye(Vector2 eyePos, Vector2 lookTarget, float scale = 0.4f)
        {
            CustomParticles.EnigmaEyeGaze(eyePos, EyeGreen * 0.8f, scale, lookTarget);
        }

        /// <summary>
        /// Spawn an eye burst on impact — eyes exploding outward from hit point.
        /// </summary>
        public static void SpawnEyeImpactBurst(Vector2 pos, int count = 6, float speed = 4f)
        {
            CustomParticles.EnigmaEyeExplosion(pos, Purple, count, speed);
        }

        // ─────────── ENIGMA-SPECIFIC VFX: GLYPHS ───────────

        /// <summary>
        /// Spawn orbiting glyph circle around a position.
        /// </summary>
        public static void SpawnGlyphCircle(Vector2 pos, int count = 6, float radius = 40f, float rotSpeed = 0.06f)
        {
            CustomParticles.GlyphCircle(pos, Purple, count: count, radius: radius, rotationSpeed: rotSpeed);
        }

        /// <summary>
        /// Spawn a glyph burst — glyphs exploding outward radially.
        /// </summary>
        public static void SpawnGlyphBurst(Vector2 pos, int count = 12, float speed = 6f)
        {
            CustomParticles.GlyphBurst(pos, GreenFlame, count: count, speed: speed);
        }

        /// <summary>
        /// Spawn a single floating glyph accent at a position.
        /// </summary>
        public static void SpawnGlyphAccent(Vector2 pos, float scale = 0.25f)
        {
            CustomParticles.Glyph(pos, GetEnigmaGradient(Main.rand.NextFloat()), scale, -1);
        }

        // ─────────── ENIGMA-SPECIFIC VFX: VOID SWIRL ───────────

        /// <summary>
        /// Spawn void swirl particles spiraling inward toward a center.
        /// Creates the signature swirling void effect.
        /// </summary>
        public static void SpawnVoidSwirl(Vector2 center, int count = 6, float radius = 60f, float opacity = 0.65f)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = radius + Main.rand.NextFloat(30f);
                Vector2 particlePos = center + angle.ToRotationVector2() * dist;
                Vector2 vel = (center - particlePos).SafeNormalize(Vector2.Zero) * 3f;

                var glow = new GenericGlowParticle(particlePos, vel,
                    GetEnigmaGradient(Main.rand.NextFloat()) * opacity,
                    0.28f, 22, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
        }

        // ─────────── GRADIENT HALO RINGS ───────────

        /// <summary>
        /// Cascading gradient halo rings (DeepPurple -> GreenFlame).
        /// </summary>
        public static void SpawnGradientHaloRings(Vector2 pos, int count = 5, float baseScale = 0.3f)
        {
            for (int i = 0; i < count; i++)
            {
                float progress = (float)i / count;
                Color ringCol = Color.Lerp(DeepPurple, GreenFlame, progress);
                CustomParticles.HaloRing(pos, ringCol, baseScale + i * 0.12f, 14);
            }
        }

        // ─────────── IMPACTS ───────────

        /// <summary>
        /// Full Enigma melee impact VFX — bloom flash, halo cascade,
        /// radial dust burst, watching eye, glyph accents, and music note scatter.
        /// Scales with combo step.
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

            // Enigma signature: watching eye at impact
            if (comboStep >= 1 || Main.rand.NextBool(3))
                SpawnGazingEye(pos + new Vector2(0, -20f), pos, 0.35f);

            // Glyph accent
            SpawnGlyphAccent(pos, 0.3f + comboStep * 0.05f);

            CustomParticles.GenericFlare(pos, GreenFlame, 0.4f + comboStep * 0.08f, 14);

            Lighting.AddLight(pos, Purple.ToVector3() * (0.8f + comboStep * 0.15f));
        }

        /// <summary>
        /// Projectile death / on-kill VFX — bigger, flashier version of MeleeImpact.
        /// Includes eye burst, glyph circle, and enhanced bloom.
        /// </summary>
        public static void ProjectileImpact(Vector2 pos, float intensity = 1f)
        {
            DrawBloom(pos, 0.6f * intensity);
            SpawnGradientHaloRings(pos, 6, 0.3f * intensity);
            SpawnMusicNotes(pos, 6, 30f * intensity, 0.75f, 1.1f, 30);
            SpawnRadialDustBurst(pos, 15, 7f * intensity);
            SpawnEyeImpactBurst(pos, 4, 4f * intensity);
            SpawnGlyphCircle(pos, 6, 40f * intensity);
            CustomParticles.GenericFlare(pos, GreenFlame, 0.5f * intensity, 16);
            Lighting.AddLight(pos, GreenFlame.ToVector3() * 1.2f * intensity);
        }

        // ─────────── SWING HELPERS ───────────

        /// <summary>
        /// Per-frame VFX to call from a swing projectile's AI().
        /// Handles dense dual-color dust trail, contrast sparkles, and periodic music notes + eyes.
        /// </summary>
        public static void SwingFrameVFX(Vector2 tipPos, Vector2 swordDirection, int comboStep,
            int timer, int dustType = DustID.PurpleTorch)
        {
            SpawnEnigmaSwingDust(tipPos, -swordDirection);
            SpawnContrastSparkle(tipPos, -swordDirection);

            if (timer % 5 == 0)
                SpawnMusicNotes(tipPos, 1, 10f, 0.7f, 0.9f, 25);

            // Enigma signature: periodic watching eyes along swing arc
            if (timer % 8 == 0)
                SpawnGazingEye(tipPos + Main.rand.NextVector2Circular(15f, 15f), tipPos, 0.25f);

            Lighting.AddLight(tipPos, GetPaletteColor(0.4f + comboStep * 0.15f).ToVector3() * 0.6f);
        }

        // ─────────── FINISHER EFFECTS ───────────

        /// <summary>
        /// Phase-3 / finisher slam VFX — screen shake, massive bloom, mystery cascade,
        /// glyph explosion, eye burst, music note scatter.
        /// </summary>
        public static void FinisherSlam(Vector2 pos, float intensity = 1f)
        {
            MagnumScreenEffects.AddScreenShake(8f * intensity);
            DrawBloom(pos, 0.8f * intensity);
            SpawnGradientHaloRings(pos, 7, 0.35f * intensity);
            SpawnMusicNotes(pos, 6, 40f, 0.8f, 1.2f, 40);
            SpawnRadialDustBurst(pos, 20, 8f * intensity);
            SpawnEyeImpactBurst(pos, 8, 5f * intensity);
            SpawnGlyphCircle(pos, 10, 60f * intensity, 0.08f);
            SpawnGlyphBurst(pos, 16, 8f * intensity);
            SpawnVoidSwirl(pos, 10, 80f * intensity);
            Lighting.AddLight(pos, WhiteGreenFlash.ToVector3() * 1.5f * intensity);
        }

        // ─────────── DYNAMIC LIGHTING ───────────

        /// <summary>
        /// Add standard Enigma ambient light at a position.
        /// </summary>
        public static void AddEnigmaLight(Vector2 worldPos, float intensity = 0.6f)
        {
            Lighting.AddLight(worldPos, Purple.ToVector3() * intensity);
        }

        /// <summary>
        /// Add palette-interpolated dynamic light. Higher t = brighter, greener.
        /// </summary>
        public static void AddPaletteLighting(Vector2 worldPos, float paletteT, float intensity = 0.8f)
        {
            Color col = GetPaletteColor(paletteT);
            Lighting.AddLight(worldPos, col.ToVector3() * intensity);
        }

        /// <summary>
        /// Add pulsing enigma light with color shift between purple and green.
        /// </summary>
        public static void AddPulsingLight(Vector2 worldPos, float time, float intensity = 0.6f)
        {
            float shift = (float)Math.Sin(time * 0.06f) * 0.5f + 0.5f;
            Color lightColor = Color.Lerp(Purple, GreenFlame, shift * 0.4f);
            float pulse = (float)Math.Sin(time * 0.08f) * 0.15f + 0.85f;
            Lighting.AddLight(worldPos, lightColor.ToVector3() * pulse * intensity);
        }
    }
}
