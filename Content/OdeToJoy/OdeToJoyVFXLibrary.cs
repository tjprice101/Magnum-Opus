using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;

namespace MagnumOpus.Content.OdeToJoy
{
    /// <summary>
    /// Shared Ode to Joy VFX library used by bosses, accessories, and tools.
    /// Weapons have their own per-weapon particle systems in their self-contained folders.
    /// These methods use vanilla dust as a lightweight shared system for non-weapon code.
    /// </summary>
    public static class OdeToJoyVFXLibrary
    {
        public static void SpawnRosePetals(Vector2 position, int count, float speed)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(speed, speed);
                Color col = Color.Lerp(OdeToJoyPalette.RosePink, OdeToJoyPalette.PetalPink, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(position, DustID.PinkFairy, vel, 0, col, Main.rand.NextFloat(0.8f, 1.4f));
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }
        }

        public static void SpawnMusicNotes(Vector2 position, int count, float speed, float scale = 1f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(speed, speed) + new Vector2(0, -2f);
                Color col = Color.Lerp(OdeToJoyPalette.GoldenPollen, OdeToJoyPalette.SunlightYellow, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(position, DustID.YellowStarDust, vel, 0, col, scale * Main.rand.NextFloat(0.6f, 1f));
                d.noGravity = true;
                d.fadeIn = 1.1f;
            }
        }

        public static void SpawnVineTrailDust(Vector2 position, Vector2 velocity)
        {
            Color col = Color.Lerp(OdeToJoyPalette.LeafGreen, OdeToJoyPalette.VerdantGreen, Main.rand.NextFloat());
            Dust d = Dust.NewDustPerfect(position, DustID.GreenFairy, velocity, 0, col, 0.8f);
            d.noGravity = true;
        }

        public static void SpawnPetalMusicNotes(Vector2 position, int count, float speed)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(speed, speed) + new Vector2(0, -3f);
                Color col = Color.Lerp(OdeToJoyPalette.RosePink, OdeToJoyPalette.GoldenPollen, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(position, DustID.PinkFairy, vel, 0, col, Main.rand.NextFloat(0.5f, 0.9f));
                d.noGravity = true;
                d.fadeIn = 0.9f;
            }
        }

        public static void SpawnPetalHaloRings(Vector2 position, int rings, float scale)
        {
            for (int r = 0; r < rings; r++)
            {
                float radius = 30f * (r + 1) * scale;
                int dustCount = 8 + r * 4;
                for (int i = 0; i < dustCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / dustCount;
                    Vector2 offset = angle.ToRotationVector2() * radius;
                    Color col = OdeToJoyPalette.GetBlossomGradient((float)i / dustCount);
                    Dust d = Dust.NewDustPerfect(position + offset, DustID.PinkFairy, offset * 0.02f, 0, col, 0.6f * scale);
                    d.noGravity = true;
                }
            }
        }

        public static void SpawnPollenSparkles(Vector2 position, int count, float radius)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(radius, radius);
                Dust d = Dust.NewDustPerfect(position + offset, DustID.YellowStarDust, Vector2.Zero, 0, OdeToJoyPalette.GoldenPollen, 0.5f);
                d.noGravity = true;
                d.fadeIn = 0.8f;
            }
        }

        public static void SpawnGradientHaloRings(Vector2 position, int count, float scale)
        {
            for (int r = 0; r < count; r++)
            {
                float radius = 40f * (r + 1) * scale;
                int dustCount = 10 + r * 4;
                for (int i = 0; i < dustCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / dustCount;
                    Vector2 offset = angle.ToRotationVector2() * radius;
                    Color col = OdeToJoyPalette.GetGardenGradient((float)i / dustCount);
                    Dust d = Dust.NewDustPerfect(position + offset, DustID.GreenFairy, offset * 0.01f, 0, col, 0.5f * scale);
                    d.noGravity = true;
                }
            }
        }

        public static void SpawnGardenAura(Vector2 position, float radius)
        {
            int count = (int)(radius / 5f);
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(radius, radius);
                Color col = OdeToJoyPalette.GetGardenGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(position + offset, DustID.GreenFairy, new Vector2(0, -0.5f), 0, col, 0.4f);
                d.noGravity = true;
                d.fadeIn = 0.7f;
            }
        }

        public static void GardenImpact(Vector2 position, float scale)
        {
            int count = (int)(10 * scale);
            for (int i = 0; i < count; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f * scale, 6f * scale);
                Color col = OdeToJoyPalette.GetGardenGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(position, DustID.GreenFairy, vel, 0, col, scale * 0.8f);
                d.noGravity = true;
            }
            SpawnPollenSparkles(position, (int)(4 * scale), 20f * scale);
        }

        public static void BlossomImpact(Vector2 position, float scale)
        {
            int count = (int)(12 * scale);
            for (int i = 0; i < count; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(8f * scale, 8f * scale);
                Color col = OdeToJoyPalette.GetBlossomGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(position, DustID.PinkFairy, vel, 0, col, scale * 0.9f);
                d.noGravity = true;
                d.fadeIn = 1.1f;
            }
            SpawnPetalMusicNotes(position, (int)(3 * scale), 4f * scale);
        }

        public static void BloomBurst(Vector2 position, float scale)
        {
            int count = (int)(16 * scale);
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                Vector2 vel = angle.ToRotationVector2() * (6f * scale);
                Color col = Color.Lerp(OdeToJoyPalette.GoldenPollen, OdeToJoyPalette.WhiteBloom, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(position, DustID.YellowStarDust, vel, 0, col, scale * 1.1f);
                d.noGravity = true;
            }
            BlossomImpact(position, scale * 0.6f);
        }

        public static void TriumphantCelebration(Vector2 position, float scale)
        {
            BloomBurst(position, scale);
            SpawnRosePetals(position, (int)(20 * scale), 12f * scale);
            SpawnMusicNotes(position, (int)(8 * scale), 8f * scale);
            SpawnPetalHaloRings(position, 3, scale * 0.5f);

            for (int i = 0; i < (int)(8 * scale); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(10f * scale, 10f * scale);
                Dust d = Dust.NewDustPerfect(position, DustID.GoldFlame, vel, 0, OdeToJoyPalette.GoldenPollen, scale);
                d.noGravity = true;
            }
        }

        public static void DeathGardenFlash(Vector2 position, float scale)
        {
            TriumphantCelebration(position, scale * 1.5f);
            for (int i = 0; i < (int)(24 * scale); i++)
            {
                float angle = MathHelper.TwoPi * i / (int)(24 * scale);
                Vector2 vel = angle.ToRotationVector2() * (14f * scale);
                Dust d = Dust.NewDustPerfect(position, DustID.GoldFlame, vel, 0, OdeToJoyPalette.WhiteBloom, scale * 1.5f);
                d.noGravity = true;
            }
        }

        public static void MusicNoteBurst(Vector2 position, Color color, int count, float speed)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(speed, speed) + new Vector2(0, -2f);
                Dust d = Dust.NewDustPerfect(position, DustID.YellowStarDust, vel, 0, color, Main.rand.NextFloat(0.5f, 1f));
                d.noGravity = true;
                d.fadeIn = 1f;
            }
        }

        public static void MeleeImpact(Vector2 position, int variant)
        {
            float scale = 0.8f + variant * 0.2f;
            GardenImpact(position, scale);
            SpawnRosePetals(position, 4 + variant * 2, 5f * scale);
        }

        public static void MusicalImpact(Vector2 position, float scale, bool withNotes)
        {
            BlossomImpact(position, scale);
            if (withNotes)
                SpawnMusicNotes(position, (int)(4 * scale), 6f * scale);
        }

        public static void ProjectileImpact(Vector2 position, float scale)
        {
            GardenImpact(position, scale * 0.7f);
            SpawnPollenSparkles(position, (int)(5 * scale), 15f * scale);
        }

        public static void FinisherSlam(Vector2 position, float scale)
        {
            TriumphantCelebration(position, scale);
            for (int i = 0; i < (int)(12 * scale); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(8f * scale, 8f * scale);
                Color col = OdeToJoyPalette.GetPetalGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(position, DustID.PinkFairy, vel, 0, col, scale * 1.2f);
                d.noGravity = true;
            }
        }

        public static Color GetPaletteColor(float t)
        {
            return OdeToJoyPalette.GetGradient(t);
        }

        // ─────────── THEME TEXTURE VFX ───────────
        // Uses OdeToJoyThemeTextures for garden/blossom-themed visuals.

        /// <summary>
        /// Draws a themed power ring using OJ Power Effect Ring + Harmonic Impact.
        /// Must be called in Additive blend mode (or {A=0} pattern).
        /// </summary>
        public static void DrawThemeImpactRing(SpriteBatch sb, Vector2 worldPos,
            float scale, float intensity = 1f, float rotation = 0f)
        {
            Vector2 drawPos = worldPos - Main.screenPosition;

            Texture2D ring = OdeToJoyThemeTextures.OJPowerEffectRing?.Value;
            if (ring != null)
            {
                Vector2 origin = ring.Size() * 0.5f;
                sb.Draw(ring, drawPos, null,
                    (OdeToJoyPalette.GoldenPollen with { A = 0 }) * 0.5f * intensity, rotation, origin,
                    scale * 0.14f, SpriteEffects.None, 0f);
                sb.Draw(ring, drawPos, null,
                    (OdeToJoyPalette.RosePink with { A = 0 }) * 0.35f * intensity, -rotation * 0.6f, origin,
                    scale * 0.10f, SpriteEffects.None, 0f);
            }

            Texture2D impact = OdeToJoyThemeTextures.OJHarmonicImpact?.Value;
            if (impact != null)
            {
                Vector2 impOrigin = impact.Size() * 0.5f;
                sb.Draw(impact, drawPos, null,
                    (OdeToJoyPalette.WhiteBloom with { A = 0 }) * 0.4f * intensity, rotation * 1.3f, impOrigin,
                    scale * 0.12f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draws a themed blossom sparkle accent at a position.
        /// Perfect for melee impacts and projectile hit effects.
        /// </summary>
        public static void DrawThemeBlossomSparkle(SpriteBatch sb, Vector2 worldPos,
            float scale, float intensity = 1f)
        {
            Texture2D sparkle = OdeToJoyThemeTextures.OJBlossomSparkle?.Value;
            if (sparkle == null) return;

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = sparkle.Size() * 0.5f;
            float rot = (float)Main.GameUpdateCount * 0.035f;

            sb.Draw(sparkle, drawPos, null,
                (OdeToJoyPalette.PetalPink with { A = 0 }) * 0.5f * intensity, rot, origin,
                scale * 0.08f, SpriteEffects.None, 0f);
            sb.Draw(sparkle, drawPos, null,
                (OdeToJoyPalette.SunlightYellow with { A = 0 }) * 0.35f * intensity, -rot * 0.5f, origin,
                scale * 0.06f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Draws a themed thorn fragment burst accent.
        /// Great for melee weapon slash impacts.
        /// </summary>
        public static void DrawThemeThornAccent(SpriteBatch sb, Vector2 worldPos,
            float scale, float intensity = 1f)
        {
            Texture2D thorn = OdeToJoyThemeTextures.OJThornFragment?.Value;
            if (thorn == null) return;

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = thorn.Size() * 0.5f;
            float rot = (float)Main.GameUpdateCount * 0.04f;

            sb.Draw(thorn, drawPos, null,
                (OdeToJoyPalette.LeafGreen with { A = 0 }) * 0.4f * intensity, rot, origin,
                scale * 0.07f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Draws a themed floral wave overlay using harmonic wave texture.
        /// </summary>
        public static void DrawThemeFloralWave(SpriteBatch sb, Vector2 worldPos,
            float scale, float intensity = 1f)
        {
            Texture2D wave = OdeToJoyThemeTextures.OJHarmonicWaveFloral?.Value;
            if (wave == null) return;

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = wave.Size() * 0.5f;
            float rot = (float)Main.GameUpdateCount * 0.015f;

            sb.Draw(wave, drawPos, null,
                (OdeToJoyPalette.GoldenPollen with { A = 0 }) * 0.3f * intensity, rot, origin,
                scale * 0.12f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Combined theme impact: blossom sparkle + impact ring + thorn accent.
        /// </summary>
        public static void DrawThemeImpactFull(SpriteBatch sb, Vector2 worldPos,
            float scale, float intensity = 1f)
        {
            DrawThemeBlossomSparkle(sb, worldPos, scale, intensity * 0.7f);
            float rot = (float)Main.GameUpdateCount * 0.02f;
            DrawThemeImpactRing(sb, worldPos, scale, intensity * 0.5f, rot);
            DrawThemeThornAccent(sb, worldPos, scale * 0.8f, intensity * 0.4f);
        }
    }
}
