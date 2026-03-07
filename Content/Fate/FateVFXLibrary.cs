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

namespace MagnumOpus.Content.Fate
{
    /// <summary>
    /// Shared Fate VFX library  Ecanonical palette, bloom stacking,
    /// shader setup, trail helpers, music notes, dust, glyph effects,
    /// constellation patterns, cosmic cloud VFX, and impact/finisher effects
    /// used by ALL Fate weapons, accessories, projectiles, minions, and enemies.
    /// </summary>
    public static class FateVFXLibrary
    {
        // ─────────── CANONICAL PALETTE (forwarded from FatePalette) ───────────
        public static readonly Color CosmicVoid       = FatePalette.CosmicVoid;
        public static readonly Color DarkPink         = FatePalette.DarkPink;
        public static readonly Color BrightCrimson    = FatePalette.BrightCrimson;
        public static readonly Color StarGold         = FatePalette.StarGold;
        public static readonly Color WhiteCelestial   = FatePalette.WhiteCelestial;
        public static readonly Color SupernovaWhite   = FatePalette.SupernovaWhite;

        // Convenience accessors
        public static readonly Color FatePurple       = FatePalette.FatePurple;
        public static readonly Color FateCyan         = FatePalette.FateCyan;
        public static readonly Color ConstellationSilver = FatePalette.ConstellationSilver;
        public static readonly Color NebulaPurple     = FatePalette.NebulaPurple;

        // Palette as array for indexed access
        private static readonly Color[] Palette = { CosmicVoid, DarkPink, BrightCrimson, StarGold, WhiteCelestial, SupernovaWhite };

        // Hue range for HueShiftingMusicNoteParticle (pink-crimson-gold band)
        private const float HueMin = 0.93f;
        private const float HueMax = 0.08f;  // wraps through 0 (red)
        private const float NoteSaturation = 0.85f;
        private const float NoteLuminosity = 0.70f;

        // Fate glow profile for GlowRenderer
        public static readonly GlowRenderer.GlowLayer[] FateGlowProfile = new[]
        {
            new GlowRenderer.GlowLayer(1.0f, 1.0f, Color.White),
            new GlowRenderer.GlowLayer(1.6f, 0.65f, new Color(255, 60, 80)),    // BrightCrimson
            new GlowRenderer.GlowLayer(2.5f, 0.4f, new Color(180, 50, 100)),    // DarkPink
            new GlowRenderer.GlowLayer(4.0f, 0.2f, new Color(120, 30, 140))     // FatePurple
        };

        // ─────────── PALETTE INTERPOLATION ───────────

        /// <summary>
        /// Lerp through the 6-colour Fate palette. t=0 -> CosmicVoid, t=1 -> SupernovaWhite.
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
        /// Drop-in replacement for the GetCosmicGradient() duplicated in every weapon file.
        /// CosmicVoid -> DarkPink -> BrightCrimson -> FatePurple -> WhiteCelestial over 0->1.
        /// </summary>
        public static Color GetCosmicGradient(float progress)
            => FatePalette.GetCosmicGradient(progress);

        // ─────────── SPRITEBATCH STATE HELPERS ───────────

        /// <summary>
        /// Switch SpriteBatch to additive blend for Fate VFX rendering.
        /// Call EndFateAdditive when done.
        /// </summary>
        public static void BeginFateAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Restore SpriteBatch to standard AlphaBlend after additive rendering.
        /// </summary>
        public static void EndFateAdditive(SpriteBatch sb)
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
            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive,
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
        public static void DrawFateBloomStack(SpriteBatch sb, Vector2 worldPos,
            float scale, float paletteT = 0.4f, float opacity = 1f)
        {
            Texture2D bloom = MagnumTextureRegistry.GetBloom();
            if (bloom == null) return;

            // 2160px bloom — cap so largest layer (scale*0.115) ≤ 0.139 → ≤300px
            scale = MathHelper.Min(scale, 1.209f);

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = bloom.Size() * 0.5f;

            Color outer = GetPaletteColor(MathHelper.Clamp(paletteT - 0.2f, 0f, 1f));
            Color inner = GetPaletteColor(MathHelper.Clamp(paletteT + 0.2f, 0f, 1f));

            // Layer 1: Outer halo (FatePurple-ish)
            sb.Draw(bloom, drawPos, null,
                (outer with { A = 0 }) * 0.3f * opacity, 0f, origin, scale * 0.115f, SpriteEffects.None, 0f);

            // Layer 2: Mid glow (DarkPink)
            sb.Draw(bloom, drawPos, null,
                (outer with { A = 0 }) * 0.5f * opacity, 0f, origin, scale * 0.08f, SpriteEffects.None, 0f);

            // Layer 3: Inner bloom (BrightCrimson)
            sb.Draw(bloom, drawPos, null,
                (inner with { A = 0 }) * 0.7f * opacity, 0f, origin, scale * 0.052f, SpriteEffects.None, 0f);

            // Layer 4: White-hot stellar core
            sb.Draw(bloom, drawPos, null,
                (SupernovaWhite with { A = 0 }) * 0.85f * opacity, 0f, origin, scale * 0.023f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Two-color bloom stack using {A=0} pattern.
        /// </summary>
        public static void DrawFateBloomStack(SpriteBatch sb, Vector2 worldPos,
            Color outerColor, Color innerColor, float scale, float opacity = 1f)
        {
            Texture2D bloom = MagnumTextureRegistry.GetBloom();
            if (bloom == null) return;

            // 2160px bloom — cap so largest layer (scale*0.115) ≤ 0.139 → ≤300px
            scale = MathHelper.Min(scale, 1.209f);

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = bloom.Size() * 0.5f;

            sb.Draw(bloom, drawPos, null,
                (outerColor with { A = 0 }) * 0.3f * opacity, 0f, origin, scale * 0.115f, SpriteEffects.None, 0f);
            sb.Draw(bloom, drawPos, null,
                (outerColor with { A = 0 }) * 0.5f * opacity, 0f, origin, scale * 0.08f, SpriteEffects.None, 0f);
            sb.Draw(bloom, drawPos, null,
                (innerColor with { A = 0 }) * 0.7f * opacity, 0f, origin, scale * 0.052f, SpriteEffects.None, 0f);
            sb.Draw(bloom, drawPos, null,
                (Color.White with { A = 0 }) * 0.85f * opacity, 0f, origin, scale * 0.023f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Bloom sandwich layer  Erenders bloom BEHIND a projectile body for depth.
        /// Call before drawing the projectile sprite, then call again after for front glow.
        /// </summary>
        public static void DrawBloomSandwichLayer(SpriteBatch sb, Vector2 worldPos,
            float scale, float opacity, bool isFrontLayer)
        {
            Texture2D bloom = MagnumTextureRegistry.GetBloom();
            if (bloom == null) return;

            // 2160px bloom — cap so largest layer (scale*0.14) ≤ 0.139 → ≤300px
            scale = MathHelper.Min(scale, 0.993f);

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = bloom.Size() * 0.5f;

            if (!isFrontLayer)
            {
                // Behind layer: larger, softer, FatePurple
                sb.Draw(bloom, drawPos, null,
                    (FatePurple with { A = 0 }) * 0.25f * opacity, 0f, origin, scale * 0.14f, SpriteEffects.None, 0f);
                sb.Draw(bloom, drawPos, null,
                    (DarkPink with { A = 0 }) * 0.35f * opacity, 0f, origin, scale * 0.09f, SpriteEffects.None, 0f);
            }
            else
            {
                // Front layer: smaller, brighter, BrightCrimson -> White
                sb.Draw(bloom, drawPos, null,
                    (BrightCrimson with { A = 0 }) * 0.5f * opacity, 0f, origin, scale * 0.046f, SpriteEffects.None, 0f);
                sb.Draw(bloom, drawPos, null,
                    (Color.White with { A = 0 }) * 0.7f * opacity, 0f, origin, scale * 0.02f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Counter-rotating double flare  Etwo bloom textures spinning in opposite directions.
        /// Creates dynamic cosmic energy appearance at projectile centers.
        /// </summary>
        public static void DrawCounterRotatingFlares(SpriteBatch sb, Vector2 worldPos,
            float scale, float time, float opacity = 1f)
        {
            Texture2D flare = MagnumTextureRegistry.GetFlare();
            if (flare == null) return;

            // 1024px flare — cap so largest layer (scale*0.7) ≤ 0.293 → ≤300px
            scale = MathHelper.Min(scale, 0.419f);

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = flare.Size() * 0.5f;

            float rot1 = time * 2.5f;
            float rot2 = -time * 1.8f;

            sb.Draw(flare, drawPos, null,
                (BrightCrimson with { A = 0 }) * 0.6f * opacity, rot1, origin, scale * 0.7f, SpriteEffects.None, 0f);
            sb.Draw(flare, drawPos, null,
                (StarGold with { A = 0 }) * 0.5f * opacity, rot2, origin, scale * 0.5f, SpriteEffects.None, 0f);
        }

        // ─────────── SELF-CONTAINED BLOOM (MANAGES OWN SPRITEBATCH) ───────────

        /// <summary>
        /// Standard Fate bloom at a blade tip or projectile centre.
        /// Uses the two-colour overload (FatePurple outer -> BrightCrimson inner).
        /// Safe to call from PreDraw  Ehandles SpriteBatch state internally.
        /// </summary>
        public static void DrawBloom(Vector2 worldPos, float scale, float opacity = 1f)
        {
            BloomRenderer.DrawBloomStackAdditive(worldPos, FatePurple, BrightCrimson, scale, opacity);
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
        /// Draw Fate-themed multi-layer glow via GlowRenderer.
        /// Caller must have SpriteBatch in additive blend.
        /// </summary>
        public static void DrawFateGlow(SpriteBatch sb, Vector2 worldPos,
            float scale = 1f, float intensity = 1f, string rotationId = null)
        {
            GlowRenderer.DrawGlow(sb, worldPos, FateGlowProfile, BrightCrimson, intensity * scale, rotationId);
        }

        /// <summary>
        /// Draw Fate glow with automatic SpriteBatch state management.
        /// Safe to call from any context.
        /// </summary>
        public static void DrawFateGlowManaged(SpriteBatch sb, Vector2 worldPos,
            float scale = 1f, float intensity = 1f, string rotationId = null)
        {
            GlowRenderer.DrawGlowManaged(sb, worldPos, FateGlowProfile, BrightCrimson, intensity * scale, rotationId);
        }

        // ─────────── TRAIL WIDTH/COLOR FUNCTIONS ───────────

        /// <summary>
        /// Standard Fate trail width: wide at head, tapers to tail.
        /// </summary>
        public static float FateTrailWidth(float completionRatio, float baseWidth = 20f)
        {
            float tipFade = MathF.Pow(1f - completionRatio, 0.6f);
            float headFade = MathF.Pow(completionRatio, 3f);
            return baseWidth * tipFade * (1f - headFade);
        }

        /// <summary>
        /// Cosmic beam trail width  Ebroader, for ranged projectiles and beams.
        /// </summary>
        public static float CosmicBeamWidth(float completionRatio, float baseWidth = 14f)
        {
            float taper = 1f - completionRatio;
            return baseWidth * taper * taper;
        }

        /// <summary>
        /// Thick cosmic trail for heavy weapons  Enebula cascade.
        /// </summary>
        public static float CosmicTrailWidth(float completionRatio, float baseWidth = 18f)
        {
            float tipFade = MathF.Pow(1f - completionRatio, 0.4f);
            return baseWidth * tipFade;
        }

        /// <summary>
        /// Trail color function with {A=0} for additive rendering under AlphaBlend.
        /// Gradient from outer palette colour at edges to white-pushed center along trail.
        /// </summary>
        public static Color FateTrailColor(float completionRatio, float whitePush = 0.45f)
        {
            float t = 0.3f + completionRatio * 0.5f;
            Color baseCol = GetPaletteColorWithWhitePush(t, whitePush * (1f - completionRatio));
            float fade = 1f - MathF.Pow(completionRatio, 1.5f);
            return (baseCol * fade) with { A = 0 };
        }

        /// <summary>
        /// Returns a pair of Vector3 colours for shader gradient uniforms.
        /// Pass 0: DarkPink -> BrightCrimson, Pass 1: BrightCrimson -> StarGold, Pass 2: StarGold -> SupernovaWhite
        /// </summary>
        public static (Vector3 primary, Vector3 secondary) GetShaderGradient(int passIndex)
        {
            return passIndex switch
            {
                0 => (DarkPink.ToVector3(), BrightCrimson.ToVector3()),
                1 => (BrightCrimson.ToVector3(), StarGold.ToVector3()),
                2 => (StarGold.ToVector3(), SupernovaWhite.ToVector3()),
                _ => (BrightCrimson.ToVector3(), StarGold.ToVector3()),
            };
        }

        // ─────────── MUSIC NOTES ───────────

        /// <summary>
        /// Spawn visible, hue-shifting Fate music notes at the given position.
        /// Notes use the pink-crimson-gold hue band and are spawned
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
        /// Dense Fate dust trail at a blade tip during a swing.
        /// Uses pink and crimson torch dust for the signature dual-color effect.
        /// </summary>
        public static void SpawnSwingDust(Vector2 pos, Vector2 awayDirection, int dustType = DustID.PinkTorch)
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
        /// Fate dual-color swing dust  Ealternating pink and crimson torches.
        /// </summary>
        public static void SpawnFateSwingDust(Vector2 pos, Vector2 awayDirection)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = awayDirection * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(0.5f, 0.5f);
                int dustType = Main.rand.NextBool() ? DustID.PinkTorch : DustID.RedTorch;
                Color col = dustType == DustID.PinkTorch ? DarkPink : BrightCrimson;
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, 0, col, 1.5f);
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }
        }

        /// <summary>
        /// Radial dust burst for on-hit / impact VFX.
        /// Gradient from FatePurple to BrightCrimson around the burst ring.
        /// </summary>
        public static void SpawnRadialDustBurst(Vector2 pos, int count = 12,
            float speed = 5f, int dustType = DustID.PinkTorch)
        {
            for (int i = 0; i < count; i++)
            {
                float progress = (float)i / count;
                float angle = MathHelper.TwoPi * i / count;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(speed * 0.6f, speed);
                Color col = Color.Lerp(FatePurple, BrightCrimson, progress);
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, 0, col, 1.3f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Contrasting star gold sparkle dust  Ecall every other frame for dual-colour trail.
        /// </summary>
        public static void SpawnContrastSparkle(Vector2 pos, Vector2 awayDirection)
        {
            if (!Main.rand.NextBool(2)) return;
            Dust d = Dust.NewDustPerfect(pos, DustID.YellowTorch,
                awayDirection * 0.5f + Main.rand.NextVector2Circular(1.5f, 1.5f), 0, StarGold, 1.0f);
            d.noGravity = true;
        }

        // ─────────── FATE-SPECIFIC VFX: COSMIC CLOUDS ───────────

        /// <summary>
        /// Spawn billowing cosmic cloud trail particles.
        /// Multiple layered particles for nebula effect.
        /// </summary>
        public static void SpawnCosmicCloudTrail(Vector2 position, Vector2 velocity, float scale = 1f)
        {
            for (int layer = 0; layer < 4; layer++)
            {
                float layerProgress = layer / 4f;
                Color cloudColor = Color.Lerp(CosmicVoid, FatePurple, layerProgress) * 0.6f;
                float particleScale = (0.3f + layer * 0.12f) * scale;

                Vector2 offset = Main.rand.NextVector2Circular(8f * scale, 8f * scale);
                Vector2 cloudVel = -velocity * (0.05f + layer * 0.02f) + Main.rand.NextVector2Circular(1.5f, 1.5f);

                var cloud = new GenericGlowParticle(position + offset, cloudVel, cloudColor, particleScale, 28, true);
                MagnumParticleHandler.SpawnParticle(cloud);
            }

            if (Main.rand.NextBool(3))
            {
                Vector2 starOffset = Main.rand.NextVector2Circular(15f * scale, 15f * scale);
                var star = new GenericGlowParticle(position + starOffset, Main.rand.NextVector2Circular(0.5f, 0.5f),
                    WhiteCelestial, 0.2f * scale, 15, true);
                MagnumParticleHandler.SpawnParticle(star);
            }
        }

        /// <summary>
        /// Spawn cosmic cloud burst (explosion of nebula energy) with multi-layer bloom.
        /// </summary>
        public static void SpawnCosmicCloudBurst(Vector2 position, float scale = 1f, int cloudCount = 16)
        {
            EnhancedParticles.BloomFlare(position, WhiteCelestial, 0.8f * scale, 20, 4, 1.2f);
            EnhancedParticles.BloomFlare(position, DarkPink, 0.6f * scale, 18, 3, 1.0f);

            for (int i = 0; i < cloudCount; i++)
            {
                float angle = MathHelper.TwoPi * i / cloudCount + Main.rand.NextFloat(-0.2f, 0.2f);
                float speed = Main.rand.NextFloat(3f, 8f) * scale;
                Vector2 cloudVel = angle.ToRotationVector2() * speed;

                Color cloudColor = Color.Lerp(CosmicVoid, FatePurple, Main.rand.NextFloat()) * 0.5f;
                float particleScale = Main.rand.NextFloat(0.3f, 0.6f) * scale;

                var cloud = new GenericGlowParticle(position, cloudVel, cloudColor, particleScale, 35, true);
                MagnumParticleHandler.SpawnParticle(cloud);
            }

            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 starVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f) * scale;

                var star = new GlowSparkParticle(position, starVel, WhiteCelestial * 0.8f, 0.25f * scale, 20);
                MagnumParticleHandler.SpawnParticle(star);
            }
        }

        // ─────────── FATE-SPECIFIC VFX: STAR PARTICLES ───────────

        /// <summary>
        /// Spawn twinkling star particles around a position.
        /// The signature Fate Variations visual identity.
        /// </summary>
        public static void SpawnStarSparkles(Vector2 pos, int count = 5, float radius = 30f, float scale = 0.25f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(radius, radius);
                Vector2 vel = Main.rand.NextVector2Circular(0.8f, 0.8f);
                Color starColor = Main.rand.NextBool(3) ? StarGold : WhiteCelestial;
                float starScale = scale * Main.rand.NextFloat(0.6f, 1.2f);

                var star = new GenericGlowParticle(pos + offset, vel, starColor, starScale, 20, true);
                MagnumParticleHandler.SpawnParticle(star);
            }
        }

        /// <summary>
        /// Spawn a constellation pattern of stars with connecting particle lines.
        /// </summary>
        public static void SpawnConstellationBurst(Vector2 center, int starCount = 6, float radius = 60f, float scale = 1f)
        {
            Vector2[] starPositions = new Vector2[starCount];

            for (int i = 0; i < starCount; i++)
            {
                float angle = MathHelper.TwoPi * i / starCount + Main.rand.NextFloat(-0.3f, 0.3f);
                float dist = radius * Main.rand.NextFloat(0.5f, 1f);
                starPositions[i] = center + angle.ToRotationVector2() * dist;

                EnhancedParticles.BloomFlare(starPositions[i], WhiteCelestial, 0.4f * scale, 30, 3, 0.9f);
            }

            for (int i = 0; i < starCount; i++)
            {
                int next = (i + 1) % starCount;
                SpawnConstellationLine(starPositions[i], starPositions[next], ConstellationSilver * 0.5f);
            }
        }

        /// <summary>
        /// Spawn particles along a line to create constellation connection.
        /// </summary>
        public static void SpawnConstellationLine(Vector2 start, Vector2 end, Color color)
        {
            float dist = Vector2.Distance(start, end);
            int segments = (int)(dist / 6f);

            for (int i = 0; i < segments; i++)
            {
                float progress = (float)i / segments;
                Vector2 pos = Vector2.Lerp(start, end, progress);
                var line = new GenericGlowParticle(pos, Main.rand.NextVector2Circular(0.3f, 0.3f), color * 0.6f, 0.08f, 20, true);
                MagnumParticleHandler.SpawnParticle(line);
            }
        }

        // ─────────── FATE-SPECIFIC VFX: GLYPHS ───────────

        /// <summary>
        /// Spawn orbiting glyph circle around a position.
        /// </summary>
        public static void SpawnGlyphCircle(Vector2 pos, int count = 6, float radius = 40f, float rotSpeed = 0.06f)
        {
            CustomParticles.GlyphCircle(pos, DarkPink, count: count, radius: radius, rotationSpeed: rotSpeed);
        }

        /// <summary>
        /// Spawn a glyph burst  Eglyphs exploding outward radially.
        /// </summary>
        public static void SpawnGlyphBurst(Vector2 pos, int count = 12, float speed = 6f)
        {
            CustomParticles.GlyphBurst(pos, BrightCrimson, count: count, speed: speed);
        }

        /// <summary>
        /// Spawn a single floating glyph accent at a position.
        /// </summary>
        public static void SpawnGlyphAccent(Vector2 pos, float scale = 0.25f)
        {
            CustomParticles.Glyph(pos, GetCosmicGradient(Main.rand.NextFloat()), scale, -1);
        }

        // ─────────── FATE-SPECIFIC VFX: COSMIC SWIRL ───────────

        /// <summary>
        /// Spawn cosmic swirl particles spiraling inward toward a center.
        /// Creates a nebula vortex collapsing effect.
        /// </summary>
        public static void SpawnCosmicSwirl(Vector2 center, int count = 6, float radius = 60f, float opacity = 0.65f)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = radius + Main.rand.NextFloat(30f);
                Vector2 particlePos = center + angle.ToRotationVector2() * dist;
                Vector2 vel = (center - particlePos).SafeNormalize(Vector2.Zero) * 3f;

                var glow = new GenericGlowParticle(particlePos, vel,
                    GetCosmicGradient(Main.rand.NextFloat()) * opacity,
                    0.28f, 22, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
        }

        // ─────────── GRADIENT HALO RINGS ───────────

        /// <summary>
        /// Cascading gradient halo rings (DarkPink -> BrightCrimson -> StarGold).
        /// </summary>
        public static void SpawnGradientHaloRings(Vector2 pos, int count = 5, float baseScale = 0.3f)
        {
            for (int i = 0; i < count; i++)
            {
                float progress = (float)i / count;
                Color ringCol = Color.Lerp(DarkPink, StarGold, progress);
                CustomParticles.HaloRing(pos, ringCol, baseScale + i * 0.12f, 14);
            }
        }

        // ─────────── COSMIC LIGHTNING ───────────

        /// <summary>
        /// Draw cosmic lightning between two points with particle sparks.
        /// </summary>
        public static void DrawCosmicLightning(Vector2 start, Vector2 end, int segments = 12,
            float amplitude = 40f, Color? primaryColor = null, Color? secondaryColor = null)
        {
            Color primary = primaryColor ?? DarkPink;
            Color secondary = secondaryColor ?? WhiteCelestial;

            Vector2 direction = (end - start).SafeNormalize(Vector2.Zero);
            Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
            float totalDist = Vector2.Distance(start, end);
            float segmentLength = totalDist / segments;

            for (int i = 1; i < segments; i++)
            {
                float progress = (float)i / segments;
                Vector2 basePos = start + direction * (segmentLength * i);
                float offset = Main.rand.NextFloat(-amplitude, amplitude) * (1f - Math.Abs(progress - 0.5f) * 2f);
                Vector2 currentPos = basePos + perpendicular * offset;

                Color particleColor = Color.Lerp(primary, secondary, progress);
                var spark = new GlowSparkParticle(currentPos, Main.rand.NextVector2Circular(2f, 2f), particleColor, 0.2f, 8);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            var endFlare = new GenericGlowParticle(end, Vector2.Zero, secondary, 0.5f, 10, true);
            MagnumParticleHandler.SpawnParticle(endFlare);

            Lighting.AddLight(end, primary.ToVector3() * 0.8f);
        }

        // ─────────── IMPACTS ───────────

        /// <summary>
        /// Full Fate melee impact VFX  Ebloom flash, halo cascade,
        /// radial dust burst, star sparkles, glyph accents, and music note scatter.
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

            // Fate signature: star sparkles at impact
            SpawnStarSparkles(pos, 3 + comboStep, 25f);

            // Glyph accent
            SpawnGlyphAccent(pos, 0.3f + comboStep * 0.05f);

            CustomParticles.GenericFlare(pos, BrightCrimson, 0.4f + comboStep * 0.08f, 14);

            Lighting.AddLight(pos, BrightCrimson.ToVector3() * (0.8f + comboStep * 0.15f));
        }

        /// <summary>
        /// Projectile death / on-kill VFX  Ebigger, flashier version of MeleeImpact.
        /// Includes constellation burst, glyph circle, cosmic cloud burst, and enhanced bloom.
        /// </summary>
        public static void ProjectileImpact(Vector2 pos, float intensity = 1f)
        {
            DrawBloom(pos, 0.6f * intensity);
            SpawnGradientHaloRings(pos, 6, 0.3f * intensity);
            SpawnMusicNotes(pos, 6, 30f * intensity, 0.75f, 1.1f, 30);
            SpawnRadialDustBurst(pos, 15, 7f * intensity);
            SpawnStarSparkles(pos, 8, 40f * intensity);
            SpawnGlyphCircle(pos, 6, 40f * intensity);
            SpawnConstellationBurst(pos, 5, 50f * intensity);
            CustomParticles.GenericFlare(pos, StarGold, 0.5f * intensity, 16);
            Lighting.AddLight(pos, StarGold.ToVector3() * 1.2f * intensity);
        }

        // ─────────── SWING HELPERS ───────────

        /// <summary>
        /// Per-frame VFX to call from a swing projectile's AI().
        /// Handles dense dual-color dust trail, contrast sparkles, and periodic music notes + stars.
        /// </summary>
        public static void SwingFrameVFX(Vector2 tipPos, Vector2 swordDirection, int comboStep,
            int timer, int dustType = DustID.PinkTorch)
        {
            SpawnFateSwingDust(tipPos, -swordDirection);
            SpawnContrastSparkle(tipPos, -swordDirection);

            if (timer % 5 == 0)
                SpawnMusicNotes(tipPos, 1, 10f, 0.7f, 0.9f, 25);

            // Fate signature: periodic star sparkles along swing arc
            if (timer % 8 == 0)
                SpawnStarSparkles(tipPos, 2, 15f, 0.2f);

            Lighting.AddLight(tipPos, GetPaletteColor(0.4f + comboStep * 0.15f).ToVector3() * 0.6f);
        }

        // ─────────── FINISHER EFFECTS ───────────

        /// <summary>
        /// Finisher slam VFX  Escreen shake, massive bloom, cosmic cloud burst,
        /// constellation explosion, glyph cascade, music note scatter.
        /// </summary>
        public static void FinisherSlam(Vector2 pos, float intensity = 1f)
        {
            MagnumScreenEffects.AddScreenShake(8f * intensity);
            DrawBloom(pos, 0.8f * intensity);
            SpawnGradientHaloRings(pos, 7, 0.35f * intensity);
            SpawnMusicNotes(pos, 6, 40f, 0.8f, 1.2f, 40);
            SpawnRadialDustBurst(pos, 20, 8f * intensity);
            SpawnStarSparkles(pos, 12, 50f * intensity);
            SpawnConstellationBurst(pos, 8, 70f * intensity);
            SpawnGlyphCircle(pos, 10, 60f * intensity, 0.08f);
            SpawnGlyphBurst(pos, 16, 8f * intensity);
            SpawnCosmicSwirl(pos, 10, 80f * intensity);
            SpawnCosmicCloudBurst(pos, intensity, 20);
            Lighting.AddLight(pos, SupernovaWhite.ToVector3() * 1.5f * intensity);
        }

        /// <summary>
        /// Cosmic supernova explosion  Ethe ultimate Fate VFX.
        /// Screen-filling cosmic explosion for endgame-tier kills and phase transitions.
        /// </summary>
        public static void SupernovaExplosion(Vector2 pos, float scale = 1f)
        {
            MagnumScreenEffects.AddScreenShake(12f * scale);

            // Multi-layer bloom flash
            EnhancedParticles.BloomFlare(pos, WhiteCelestial, 1.0f * scale, 20, 4, 1.5f);
            EnhancedParticles.BloomFlare(pos, DarkPink, 0.8f * scale, 18, 3, 1.2f);
            EnhancedParticles.BloomFlare(pos, FatePurple, 0.6f * scale, 16, 2, 1.0f);

            SpawnCosmicCloudBurst(pos, scale, 24);
            SpawnGlyphBurst(pos, 12, 8f * scale);
            SpawnConstellationBurst(pos, 8, 80f * scale, scale);
            SpawnMusicNotes(pos, 8, 50f, 0.8f, 1.2f, 40);

            // Ring expansion
            for (int ring = 0; ring < 4; ring++)
            {
                float ringProgress = ring / 4f;
                Color ringColor = GetCosmicGradient(ringProgress);
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi * i / 12f;
                    Vector2 ringPos = pos + angle.ToRotationVector2() * (30f + ring * 20f) * scale;
                    var ringParticle = new GenericGlowParticle(ringPos, angle.ToRotationVector2() * 2f, ringColor, 0.3f, 20 + ring * 5, true);
                    MagnumParticleHandler.SpawnParticle(ringParticle);
                }
            }

            Lighting.AddLight(pos, BrightCrimson.ToVector3() * 2f * scale);
        }

        // ─────────── DYNAMIC LIGHTING ───────────

        /// <summary>
        /// Add standard Fate ambient light at a position.
        /// </summary>
        public static void AddFateLight(Vector2 worldPos, float intensity = 0.6f)
        {
            Lighting.AddLight(worldPos, BrightCrimson.ToVector3() * intensity);
        }

        /// <summary>
        /// Add palette-interpolated dynamic light. Higher t = brighter, more golden.
        /// </summary>
        public static void AddPaletteLighting(Vector2 worldPos, float paletteT, float intensity = 0.8f)
        {
            Color col = GetPaletteColor(paletteT);
            Lighting.AddLight(worldPos, col.ToVector3() * intensity);
        }

        /// <summary>
        /// Add pulsing fate light with color shift between crimson and gold.
        /// </summary>
        public static void AddPulsingLight(Vector2 worldPos, float time, float intensity = 0.6f)
        {
            float shift = (float)Math.Sin(time * 0.06f) * 0.5f + 0.5f;
            Color lightColor = Color.Lerp(BrightCrimson, StarGold, shift * 0.4f);
            float pulse = (float)Math.Sin(time * 0.08f) * 0.15f + 0.85f;
            Lighting.AddLight(worldPos, lightColor.ToVector3() * pulse * intensity);
        }

        // ─────────── THEME TEXTURE VFX ───────────
        // Uses FateThemeTextures for theme-specific celestial visuals.

        /// <summary>
        /// Draws a themed celestial impact using Fate Power Effect Ring + Harmonic Impact.
        /// Must be called in Additive blend mode (or {A=0} pattern).
        /// </summary>
        public static void DrawThemeImpactRing(SpriteBatch sb, Vector2 worldPos,
            float scale, float intensity = 1f, float rotation = 0f)
        {
            Vector2 drawPos = worldPos - Main.screenPosition;

            Texture2D ring = FateThemeTextures.FAPowerEffectRing?.Value;
            if (ring != null)
            {
                Vector2 origin = ring.Size() * 0.5f;
                sb.Draw(ring, drawPos, null,
                    (BrightCrimson with { A = 0 }) * 0.5f * intensity, rotation, origin,
                    scale * 0.15f, SpriteEffects.None, 0f);
                sb.Draw(ring, drawPos, null,
                    (DarkPink with { A = 0 }) * 0.3f * intensity, -rotation * 0.7f, origin,
                    scale * 0.10f, SpriteEffects.None, 0f);
            }

            Texture2D impact = FateThemeTextures.FAHarmonicImpact?.Value;
            if (impact != null)
            {
                Vector2 impOrigin = impact.Size() * 0.5f;
                sb.Draw(impact, drawPos, null,
                    (WhiteCelestial with { A = 0 }) * 0.45f * intensity, rotation * 1.3f, impOrigin,
                    scale * 0.12f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draws themed celestial glyph at a position using Fate glyph texture.
        /// Must be called in Additive blend mode.
        /// </summary>
        public static void DrawThemeCelestialGlyph(SpriteBatch sb, Vector2 worldPos,
            float scale, float intensity = 1f)
        {
            Vector2 drawPos = worldPos - Main.screenPosition;

            Texture2D glyph = FateThemeTextures.FACelestialGlyph?.Value;
            if (glyph != null)
            {
                Vector2 origin = glyph.Size() * 0.5f;
                float rot = (float)Main.GameUpdateCount * 0.025f;
                sb.Draw(glyph, drawPos, null,
                    (StarGold with { A = 0 }) * 0.45f * intensity, rot, origin,
                    scale * 0.08f, SpriteEffects.None, 0f);
                sb.Draw(glyph, drawPos, null,
                    (DarkPink with { A = 0 }) * 0.3f * intensity, -rot * 0.6f, origin,
                    scale * 0.06f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draws a themed supernova core burst at a position.
        /// Must be called in Additive blend mode.
        /// </summary>
        public static void DrawThemeSupernovaCore(SpriteBatch sb, Vector2 worldPos,
            float scale, float intensity = 1f)
        {
            Texture2D core = FateThemeTextures.FASupernovaCore?.Value;
            if (core == null) return;

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = core.Size() * 0.5f;

            sb.Draw(core, drawPos, null,
                (BrightCrimson with { A = 0 }) * 0.5f * intensity, 0f, origin,
                scale * 0.10f, SpriteEffects.None, 0f);
            sb.Draw(core, drawPos, null,
                (WhiteCelestial with { A = 0 }) * 0.7f * intensity, 0f, origin,
                scale * 0.05f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Combined theme impact: bloom + celestial glyph + impact ring + supernova core.
        /// </summary>
        public static void DrawThemeImpactFull(SpriteBatch sb, Vector2 worldPos,
            float scale, float intensity = 1f)
        {
            DrawFateBloomStack(sb, worldPos, scale, 0.3f, intensity);
            DrawThemeCelestialGlyph(sb, worldPos, scale, intensity * 0.6f);
            float rot = (float)Main.GameUpdateCount * 0.02f;
            DrawThemeImpactRing(sb, worldPos, scale, intensity * 0.5f, rot);
            DrawThemeSupernovaCore(sb, worldPos, scale * 0.8f, intensity * 0.4f);
        }
    }
}
