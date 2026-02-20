using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.MoonlightSonata
{
    /// <summary>
    /// Shared Moonlight Sonata VFX library — canonical palette, bloom, dust,
    /// music-note, and impact helpers used by ALL Moonlight weapons, accessories,
    /// and projectiles.  Wraps lower-level APIs so callers stay DRY.
    /// </summary>
    public static class MoonlightVFXLibrary
    {
        // ─────────── CANONICAL PALETTE ───────────
        // 6-colour piano dynamic scale (pianissimo → sforzando)
        public static readonly Color NightPurple    = new Color(40, 10, 60);     // [0] Pianissimo
        public static readonly Color DarkPurple     = new Color(75, 0, 130);     // [1] Piano
        public static readonly Color Violet         = new Color(138, 43, 226);   // [2] Mezzo
        public static readonly Color Lavender       = new Color(180, 150, 255);  // [3] Forte
        public static readonly Color IceBlue        = new Color(135, 206, 250);  // [4] Fortissimo
        public static readonly Color MoonWhite      = new Color(240, 235, 255);  // [5] Sforzando

        // Convenience accessors matching MagnumThemePalettes names
        public static readonly Color Silver         = new Color(220, 220, 235);
        public static readonly Color WeaponLavender = new Color(200, 170, 255);

        // Hue range for HueShiftingMusicNoteParticle (purple-blue band)
        private const float HueMin = 0.72f;
        private const float HueMax = 0.83f;
        private const float NoteSaturation = 0.7f;
        private const float NoteLuminosity = 0.6f;

        // ─────────── PALETTE INTERPOLATION ───────────

        /// <summary>
        /// Lerp through the 6-colour Moonlight palette.  t=0 → NightPurple, t=1 → MoonWhite.
        /// </summary>
        public static Color GetPaletteColor(float t)
        {
            Color[] pal = { NightPurple, DarkPurple, Violet, Lavender, IceBlue, MoonWhite };
            t = MathHelper.Clamp(t, 0f, 1f);
            float scaled = t * (pal.Length - 1);
            int idx = (int)scaled;
            int next = Math.Min(idx + 1, pal.Length - 1);
            return Color.Lerp(pal[idx], pal[next], scaled - idx);
        }

        // ─────────── BLOOM ───────────

        /// <summary>
        /// Standard Moonlight bloom at a blade tip or projectile centre.
        /// Uses the two-colour overload (DarkPurple outer → IceBlue inner).
        /// Safe to call from PreDraw — handles SpriteBatch state internally.
        /// </summary>
        public static void DrawBloom(Vector2 worldPos, float scale, float opacity = 1f)
        {
            BloomRenderer.DrawBloomStackAdditive(worldPos, DarkPurple, IceBlue, scale, opacity);
        }

        /// <summary>
        /// Combo-step–aware bloom (bigger + brighter on later hits).
        /// </summary>
        public static void DrawComboBloom(Vector2 worldPos, int comboStep, float baseScale = 0.4f, float opacity = 1f)
        {
            float scale = baseScale + comboStep * 0.07f;
            DrawBloom(worldPos, scale, opacity);
        }

        // ─────────── MUSIC NOTES ───────────

        /// <summary>
        /// Spawn visible, hue-shifting Moonlight music notes at the given position.
        /// Notes use the canonical purple-blue hue band (0.72–0.83) and are spawned
        /// at scale 0.7 f+ so they are clearly visible.
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
        /// Dense Moonlight dust trail at a blade tip during a swing.
        /// Spawns 2 dust per call with gradient colour + noGravity.
        /// Call every frame during active swing for Calamity-level density.
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
        /// Radial dust burst for on-hit / impact VFX.
        /// Fires dust outward in all directions with a gradient from startColor → endColor.
        /// </summary>
        public static void SpawnRadialDustBurst(Vector2 pos, int count = 12,
            float speed = 5f, int dustType = DustID.PurpleTorch)
        {
            for (int i = 0; i < count; i++)
            {
                float progress = (float)i / count;
                float angle = MathHelper.TwoPi * i / count;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(speed * 0.6f, speed);
                Color col = Color.Lerp(DarkPurple, IceBlue, progress);
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, 0, col, 1.3f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Contrasting silver sparkle dust — call every other frame for dual-colour trail.
        /// </summary>
        public static void SpawnContrastSparkle(Vector2 pos, Vector2 awayDirection)
        {
            if (!Main.rand.NextBool(2)) return;
            Dust d = Dust.NewDustPerfect(pos, DustID.Enchanted_Pink,
                awayDirection * 0.5f + Main.rand.NextVector2Circular(1.5f, 1.5f), 0, Silver, 1.0f);
            d.noGravity = true;
        }

        // ─────────── GRADIENT HALO RINGS ───────────

        /// <summary>
        /// Cascading gradient halo rings (DarkPurple → IceBlue).
        /// Used for impacts, phase transitions, and finisher hits.
        /// </summary>
        public static void SpawnGradientHaloRings(Vector2 pos, int count = 5, float baseScale = 0.3f)
        {
            for (int i = 0; i < count; i++)
            {
                float progress = (float)i / count;
                Color ringCol = Color.Lerp(DarkPurple, IceBlue, progress);
                CustomParticles.MoonlightHalo(pos, baseScale + i * 0.12f);
            }
        }

        // ─────────── IMPACTS ───────────

        /// <summary>
        /// Full Moonlight melee impact VFX — combines bloom flash, gradient halo cascade,
        /// radial dust burst, and music note scatter.  Scales with combo step.
        /// </summary>
        public static void MeleeImpact(Vector2 pos, int comboStep = 0)
        {
            // 1. Central bloom flash
            float bloomScale = 0.5f + comboStep * 0.1f;
            DrawBloom(pos, bloomScale);

            // 2. Gradient halo cascade
            int rings = 3 + comboStep;
            SpawnGradientHaloRings(pos, rings);

            // 3. Radial dust burst — more at higher combos
            int dustCount = 8 + comboStep * 4;
            SpawnRadialDustBurst(pos, dustCount, 5f + comboStep);

            // 4. Music note burst — visible!
            int noteCount = 2 + comboStep;
            SpawnMusicNotes(pos, noteCount, 25f);

            // 5. Theme particle accent
            CustomParticles.MoonlightFlare(pos, 0.4f + comboStep * 0.08f);

            // 6. Dynamic lighting
            Lighting.AddLight(pos, Violet.ToVector3() * (0.8f + comboStep * 0.15f));
        }

        /// <summary>
        /// Projectile death / on-kill VFX — bigger, flashier version of MeleeImpact.
        /// </summary>
        public static void ProjectileImpact(Vector2 pos, float intensity = 1f)
        {
            // Central flash cascade
            DrawBloom(pos, 0.6f * intensity);

            // Expanding halo rings
            SpawnGradientHaloRings(pos, 6, 0.3f * intensity);

            // Music note burst (visible scale)
            SpawnMusicNotes(pos, 6, 30f * intensity, 0.75f, 1.1f, 30);

            // Radial dust
            SpawnRadialDustBurst(pos, 15, 7f * intensity);

            // Sparkle scatter via custom particle system
            CustomParticles.MoonlightImpactBurst(pos, 10);

            Lighting.AddLight(pos, IceBlue.ToVector3() * 1.2f * intensity);
        }

        // ─────────── SWING HELPERS ───────────

        /// <summary>
        /// Per-frame VFX to call from a swing projectile's AI().
        /// Handles dense dust trail, contrast sparkles, and periodic music notes.
        /// </summary>
        public static void SwingFrameVFX(Vector2 tipPos, Vector2 swordDirection, int comboStep,
            int timer, int dustType = DustID.PurpleTorch)
        {
            // Dense dust every frame
            SpawnSwingDust(tipPos, -swordDirection, dustType);

            // Contrasting sparkles every other frame
            SpawnContrastSparkle(tipPos, -swordDirection);

            // Periodic music notes (every 5 frames)
            if (timer % 5 == 0)
            {
                SpawnMusicNotes(tipPos, 1, 10f, 0.7f, 0.9f, 25);
            }

            // Dynamic lighting at tip
            Lighting.AddLight(tipPos, GetPaletteColor(0.4f + comboStep * 0.15f).ToVector3() * 0.6f);
        }

        // ─────────── FINISHER EFFECTS ───────────

        /// <summary>
        /// Phase-3 / finisher slam VFX — screen shake, massive bloom, music note cascade.
        /// Call once at ~85% swing progression for the climactic hit.
        /// </summary>
        public static void FinisherSlam(Vector2 pos, float intensity = 1f)
        {
            // Screen shake (only for finishers!)
            MagnumScreenEffects.AddScreenShake(8f * intensity);

            // Large bloom flash
            DrawBloom(pos, 0.8f * intensity);

            // Big halo cascade
            SpawnGradientHaloRings(pos, 7, 0.35f * intensity);

            // Dense music note cascade
            SpawnMusicNotes(pos, 6, 40f, 0.8f, 1.2f, 40);

            // Heavy radial burst
            SpawnRadialDustBurst(pos, 20, 8f * intensity);

            // Crescendo particle accent
            CustomParticles.MoonlightCrescendo(pos, intensity);

            Lighting.AddLight(pos, MoonWhite.ToVector3() * 1.5f * intensity);
        }
    }
}
