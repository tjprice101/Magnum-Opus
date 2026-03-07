using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.EnigmaVariations
{
    /// <summary>
    /// Shared Enigma Variations VFX library — provides theme-textured draw helpers
    /// for all Enigma weapons, accessories, and bosses.
    /// Uses EnigmaThemeTextures for void/mystery-themed visuals.
    /// </summary>
    public static class EnigmaVFXLibrary
    {
        // ─────────── PALETTE SHORTCUTS ───────────
        public static readonly Color VoidBlack        = EnigmaPalette.VoidBlack;
        public static readonly Color DeepPurple       = EnigmaPalette.DeepPurple;
        public static readonly Color Purple           = EnigmaPalette.Purple;
        public static readonly Color GreenFlame       = EnigmaPalette.GreenFlame;
        public static readonly Color BrightGreen      = EnigmaPalette.BrightGreen;
        public static readonly Color WhiteGreenFlash  = EnigmaPalette.WhiteGreenFlash;
        public static readonly Color EyeGreen         = EnigmaPalette.EyeGreen;
        public static readonly Color GlyphPurple      = EnigmaPalette.GlyphPurple;
        public static readonly Color ArcaneFlash      = EnigmaPalette.ArcaneFlash;

        // ─────────── DUST-BASED VFX ───────────

        /// <summary>
        /// Spawn eerie green fire dust.
        /// </summary>
        public static void SpawnVoidFlameDust(Vector2 position, int count, float speed)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(speed, speed);
                Color col = Color.Lerp(GreenFlame, BrightGreen, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(position, DustID.GreenTorch, vel, 0, col, Main.rand.NextFloat(0.8f, 1.4f));
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }
        }

        /// <summary>
        /// Spawn mystery-mist purple dust.
        /// </summary>
        public static void SpawnMysteryMist(Vector2 position, int count, float radius)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(radius, radius);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), Main.rand.NextFloat(-0.6f, -0.1f));
                Color col = Color.Lerp(DeepPurple, Purple, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(position + offset, DustID.PurpleTorch, vel, 50, col, 0.7f);
                d.noGravity = true;
                d.fadeIn = 0.9f;
            }
        }

        /// <summary>
        /// Spawn arcane glyph ring burst.
        /// </summary>
        public static void SpawnGlyphRing(Vector2 position, int dustCount, float radius)
        {
            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                Vector2 offset = angle.ToRotationVector2() * radius;
                Color col = Color.Lerp(GlyphPurple, EyeGreen, (float)i / dustCount);
                Dust d = Dust.NewDustPerfect(position + offset, DustID.GreenTorch, offset * 0.02f, 0, col, 0.6f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Combined dust impact — void flames + glyph ring.
        /// </summary>
        public static void VoidImpact(Vector2 position, float scale)
        {
            SpawnVoidFlameDust(position, (int)(10 * scale), 5f * scale);
            SpawnGlyphRing(position, (int)(12 * scale), 25f * scale);
        }

        // ─────────── THEME TEXTURE VFX ───────────
        // Uses EnigmaThemeTextures for arcane, void-themed visuals.

        /// <summary>
        /// Draws a themed enigma bloom stack — 3 layers of soft void/green glow.
        /// Must be called in Additive blend mode (or {A=0} pattern).
        /// </summary>
        public static void DrawEnigmaBloomStack(SpriteBatch sb, Vector2 worldPos,
            float scale, float coreScale = 0.3f, float intensity = 1f)
        {
            Texture2D bloom = MagnumTextureRegistry.GetBloom();
            if (bloom == null) return;

            // 2160px bloom — cap so largest layer (scale*0.04) ≤ 0.139 → ≤300px
            scale = MathHelper.Min(scale, 3.475f);

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = bloom.Size() * 0.5f;

            // Outer: deep purple haze
            sb.Draw(bloom, drawPos, null,
                (DeepPurple with { A = 0 }) * 0.25f * intensity, 0f, origin,
                scale * 0.04f, SpriteEffects.None, 0f);
            // Mid: green flame glow
            sb.Draw(bloom, drawPos, null,
                (GreenFlame with { A = 0 }) * 0.35f * intensity, 0f, origin,
                scale * 0.025f, SpriteEffects.None, 0f);
            // Core: bright flash
            sb.Draw(bloom, drawPos, null,
                (WhiteGreenFlash with { A = 0 }) * 0.5f * intensity, 0f, origin,
                scale * coreScale * 0.07f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Draws a themed power ring using Enigma Power Effect Ring + Harmonic Impact.
        /// Must be called in Additive blend mode (or {A=0} pattern).
        /// </summary>
        public static void DrawThemeImpactRing(SpriteBatch sb, Vector2 worldPos,
            float scale, float intensity = 1f, float rotation = 0f)
        {
            Vector2 drawPos = worldPos - Main.screenPosition;

            Texture2D ring = EnigmaThemeTextures.ENPowerEffectRing?.Value;
            if (ring != null)
            {
                Vector2 origin = ring.Size() * 0.5f;
                sb.Draw(ring, drawPos, null,
                    (GreenFlame with { A = 0 }) * 0.5f * intensity, rotation, origin,
                    scale * 0.14f, SpriteEffects.None, 0f);
                sb.Draw(ring, drawPos, null,
                    (GlyphPurple with { A = 0 }) * 0.35f * intensity, -rotation * 0.6f, origin,
                    scale * 0.10f, SpriteEffects.None, 0f);
            }

            Texture2D impact = EnigmaThemeTextures.ENHarmonicImpact?.Value;
            if (impact != null)
            {
                Vector2 impOrigin = impact.Size() * 0.5f;
                sb.Draw(impact, drawPos, null,
                    (WhiteGreenFlash with { A = 0 }) * 0.4f * intensity, rotation * 1.3f, impOrigin,
                    scale * 0.11f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draws a themed star/flare burst at a position.
        /// Uses ENStarFlare for dual rotating layers.
        /// </summary>
        public static void DrawThemeStarFlare(SpriteBatch sb, Vector2 worldPos,
            float scale, float intensity = 1f)
        {
            Texture2D star = EnigmaThemeTextures.ENStarFlare?.Value;
            if (star == null) return;

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = star.Size() * 0.5f;
            float rot = (float)Main.GameUpdateCount * 0.03f;

            sb.Draw(star, drawPos, null,
                (EyeGreen with { A = 0 }) * 0.5f * intensity, rot, origin,
                scale * 0.08f, SpriteEffects.None, 0f);
            sb.Draw(star, drawPos, null,
                (Purple with { A = 0 }) * 0.35f * intensity, -rot * 0.6f, origin,
                scale * 0.06f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Draws the enigma eye at a position — an eerie watching void.
        /// Slowly rotating, pulsing in and out.
        /// </summary>
        public static void DrawThemeEnigmaEye(SpriteBatch sb, Vector2 worldPos,
            float scale, float intensity = 1f)
        {
            Texture2D eye = EnigmaThemeTextures.ENEnigmaEye?.Value;
            if (eye == null) return;

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = eye.Size() * 0.5f;
            float pulse = 0.85f + 0.15f * (float)Math.Sin(Main.GameUpdateCount * 0.05f);

            sb.Draw(eye, drawPos, null,
                (GreenFlame with { A = 0 }) * 0.6f * intensity * pulse, 0f, origin,
                scale * 0.07f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Combined theme impact: bloom + star flare + impact ring + eye overlay.
        /// </summary>
        public static void DrawThemeImpactFull(SpriteBatch sb, Vector2 worldPos,
            float scale, float intensity = 1f)
        {
            DrawEnigmaBloomStack(sb, worldPos, scale, 0.3f, intensity);
            DrawThemeStarFlare(sb, worldPos, scale, intensity * 0.6f);
            float rot = (float)Main.GameUpdateCount * 0.02f;
            DrawThemeImpactRing(sb, worldPos, scale, intensity * 0.5f, rot);
            DrawThemeEnigmaEye(sb, worldPos, scale * 0.6f, intensity * 0.3f);
        }

        /// <summary>
        /// Add pulsing eerie green light at a world position.
        /// </summary>
        public static void AddPulsingLight(Vector2 worldPos, float time, float intensity = 0.5f)
        {
            float shift = (float)Math.Sin(time * 0.05f) * 0.5f + 0.5f;
            Color lightColor = Color.Lerp(GreenFlame, Purple, shift * 0.4f);
            float pulse = (float)Math.Sin(time * 0.07f) * 0.15f + 0.85f;
            Lighting.AddLight(worldPos, lightColor.ToVector3() * pulse * intensity);
        }

        /// <summary>
        /// Add pulsing light at a world position with a custom color.
        /// </summary>
        public static void AddPulsingLight(Vector2 worldPos, Color color, float time, float intensity = 0.5f)
        {
            float pulse = (float)Math.Sin(time * 0.07f) * 0.15f + 0.85f;
            Lighting.AddLight(worldPos, color.ToVector3() * pulse * intensity);
        }
    }
}
