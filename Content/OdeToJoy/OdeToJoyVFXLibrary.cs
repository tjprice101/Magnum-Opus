using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.OdeToJoy
{
    /// <summary>
    /// Shared Ode to Joy VFX library used by bosses, accessories, and tools.
    /// Weapons have their own per-weapon particle systems in their self-contained folders.
    /// Includes: bloom stacking, screen effects, musical VFX, trail draw helpers.
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

            // Color-ramped sparkle explosion
            SpawnGardenSparkleExplosion(position, (int)(8 * scale), 5f * scale, 0.3f);
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

            // Blossom sparkle explosion
            SpawnBlossomSparkleExplosion(position, (int)(6 * scale), 4f * scale, 0.25f);
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

            // Triumphant sparkle cascade
            SpawnTriumphantStarburst(position, scale * 0.8f);

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

            // Tiered sparkle based on combo step
            if (variant >= 2)
                SpawnTriumphantStarburst(position, 0.3f + variant * 0.15f);
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

            // Color-ramped sparkle burst on every projectile impact
            SpawnGardenSparkleExplosion(position, (int)(6 * scale), 4f * scale, 0.25f);
        }

        public static void FinisherSlam(Vector2 position, float scale)
        {
            TriumphantCelebration(position, scale);

            // Massive triumphant starburst for finisher
            SpawnTriumphantStarburst(position, scale * 1.5f);

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

        // ─────────── COLOR-RAMPED SPARKLE EXPLOSIONS ───────────

        /// <summary>
        /// Spawns a color-ramped sparkle explosion using the Ode to Joy garden gradient.
        /// Particles range from deep forest green through golden pollen to white bloom.
        /// </summary>
        public static void SpawnGardenSparkleExplosion(Vector2 position, int count, float speed, float baseScale)
        {
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow == null) return;

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / Math.Max(1, count - 1);
                Color sparkleColor = OdeToJoyPalette.GetGardenGradient(t);
                sparkleColor = Color.Lerp(sparkleColor, Color.White, 0.15f);

                Vector2 vel = Main.rand.NextVector2Circular(speed, speed);
                float scale = baseScale * Main.rand.NextFloat(0.6f, 1.2f);

                try
                {
                    var particle = new GlowSparkParticle(
                        position + Main.rand.NextVector2Circular(4f, 4f),
                        vel,
                        sparkleColor with { A = 0 },
                        scale,
                        Main.rand.Next(15, 30));
                    MagnumParticleHandler.SpawnParticle(particle);
                }
                catch
                {
                    Dust d = Dust.NewDustPerfect(position, DustID.GreenFairy, vel, 0, sparkleColor, scale * 3f);
                    d.noGravity = true;
                }
            }
        }

        /// <summary>
        /// Spawns a blossom sparkle explosion using pink-to-gold-to-white gradient.
        /// </summary>
        public static void SpawnBlossomSparkleExplosion(Vector2 position, int count, float speed, float baseScale)
        {
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow == null) return;

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / Math.Max(1, count - 1);
                Color sparkleColor = OdeToJoyPalette.GetBlossomGradient(t);
                sparkleColor = Color.Lerp(sparkleColor, Color.White, 0.2f);

                Vector2 vel = Main.rand.NextVector2Circular(speed, speed);
                float scale = baseScale * Main.rand.NextFloat(0.5f, 1.1f);

                try
                {
                    var particle = new GlowSparkParticle(
                        position + Main.rand.NextVector2Circular(3f, 3f),
                        vel,
                        sparkleColor with { A = 0 },
                        scale,
                        Main.rand.Next(12, 25));
                    MagnumParticleHandler.SpawnParticle(particle);
                }
                catch
                {
                    Dust d = Dust.NewDustPerfect(position, DustID.PinkFairy, vel, 0, sparkleColor, scale * 3f);
                    d.noGravity = true;
                }
            }
        }

        /// <summary>
        /// Triumphant starburst — the biggest sparkle effect. Full palette explosion
        /// with concentric rings of garden green → blossom pink → golden pollen → white bloom.
        /// </summary>
        public static void SpawnTriumphantStarburst(Vector2 position, float intensity = 1f)
        {
            // Inner ring: golden pollen
            SpawnGardenSparkleExplosion(position, (int)(12 * intensity), 8f * intensity, 0.4f);

            // Middle ring: blossom pink
            SpawnBlossomSparkleExplosion(position, (int)(10 * intensity), 6f * intensity, 0.35f);

            // Outer ring: wide soft green
            for (int i = 0; i < (int)(8 * intensity); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(10f * intensity, 10f * intensity);
                Color col = Color.Lerp(OdeToJoyPalette.GoldenPollen, OdeToJoyPalette.WhiteBloom, Main.rand.NextFloat(0.3f, 0.8f));

                try
                {
                    var particle = new GlowSparkParticle(
                        position + Main.rand.NextVector2Circular(8f, 8f),
                        vel,
                        col with { A = 0 },
                        0.45f * intensity,
                        Main.rand.Next(20, 35));
                    MagnumParticleHandler.SpawnParticle(particle);
                }
                catch
                {
                    Dust d = Dust.NewDustPerfect(position, DustID.GoldFlame, vel, 0, col, 1.2f * intensity);
                    d.noGravity = true;
                }
            }
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

        // ═════════════════════════════════════════════════════════════════
        //  BLOOM STACKING — Multi-layer additive bloom with {A=0} pattern
        // ═════════════════════════════════════════════════════════════════

        /// <summary>
        /// Draws a multi-layer additive bloom stack at a world position.
        /// Uses the {A=0} premultiplied alpha pattern for correct additive blending.
        /// Call while SpriteBatch is in additive blend mode (TrueAdditive or ShaderAdditive).
        /// </summary>
        /// <param name="sb">SpriteBatch (must be in additive mode).</param>
        /// <param name="worldPos">World position for the bloom center.</param>
        /// <param name="coreColor">Brightest inner color.</param>
        /// <param name="outerColor">Dimmer outer glow color.</param>
        /// <param name="scale">Base scale (1.0 = texture native size).</param>
        /// <param name="intensity">Overall intensity multiplier 0-1.</param>
        /// <param name="layers">Number of bloom layers (3-6 recommended).</param>
        public static void DrawBloomStack(SpriteBatch sb, Vector2 worldPos,
            Color coreColor, Color outerColor, float scale, float intensity = 1f, int layers = 4)
        {
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow == null) return;

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = glow.Size() * 0.5f;

            for (int i = layers - 1; i >= 0; i--)
            {
                float t = (float)i / Math.Max(1, layers - 1);
                float layerScale = scale * MathHelper.Lerp(0.4f, 2.2f, t);
                float layerOpacity = MathHelper.Lerp(0.7f, 0.15f, t) * intensity;
                Color layerColor = Color.Lerp(coreColor, outerColor, t) with { A = 0 };

                sb.Draw(glow, drawPos, null, layerColor * layerOpacity, 0f,
                    origin, layerScale, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draws a directional lens flare at a world position.
        /// Perfect for melee blade tips and magic projectile heads.
        /// </summary>
        public static void DrawLensFlare(SpriteBatch sb, Vector2 worldPos,
            Color color, float scale, float rotation, float intensity = 1f)
        {
            Texture2D flare = MagnumTextureRegistry.GetSoftGlow();
            if (flare == null) return;

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = flare.Size() * 0.5f;
            Color flareColor = color with { A = 0 };

            // Horizontal bar
            sb.Draw(flare, drawPos, null, flareColor * 0.6f * intensity,
                rotation, origin, new Vector2(scale * 2f, scale * 0.3f), SpriteEffects.None, 0f);
            // Vertical bar
            sb.Draw(flare, drawPos, null, flareColor * 0.4f * intensity,
                rotation + MathHelper.PiOver2, origin, new Vector2(scale * 1.5f, scale * 0.2f), SpriteEffects.None, 0f);
            // Core dot
            sb.Draw(flare, drawPos, null, (Color.White with { A = 0 }) * 0.5f * intensity,
                0f, origin, scale * 0.4f, SpriteEffects.None, 0f);
        }

        // ═════════════════════════════════════════════════════════════════
        //  SCREEN EFFECTS — Shake, flash, pulse
        // ═════════════════════════════════════════════════════════════════

        /// <summary>
        /// Triggers screen shake via player's ScreenPosition offset.
        /// Strength is in pixels of maximum displacement.
        /// </summary>
        public static void ScreenShake(float strength, int durationTicks = 8)
        {
            if (Main.LocalPlayer.dead) return;

            // Use Terraria's built-in screen shake system
            if (strength > 0f)
            {
                // PunchCameraModifier is the preferred tModLoader way
                var shake = new Terraria.Graphics.CameraModifiers.PunchCameraModifier(
                    Main.LocalPlayer.Center, Main.rand.NextVector2CircularEdge(1f, 1f),
                    strength, 6f, durationTicks, 1000f);
                Main.instance.CameraModifiers.Add(shake);
            }
        }

        /// <summary>
        /// Triggers a screen flash effect by spawning a large additive bloom at the player's center.
        /// </summary>
        public static void ScreenFlash(Color color, float intensity = 1f, int lifetime = 12)
        {
            try
            {
                var flash = new BloomParticle(
                    Main.LocalPlayer.Center,
                    Vector2.Zero,
                    color with { A = 0 },
                    8f * intensity, 12f * intensity,
                    lifetime, true);
                MagnumParticleHandler.SpawnParticle(flash);
            }
            catch { }
        }

        // ═════════════════════════════════════════════════════════════════
        //  TRAIL DRAWING — High-level wrappers around CalamityStyleTrailRenderer
        // ═════════════════════════════════════════════════════════════════

        /// <summary>
        /// Draws a VertexStrip trail for a projectile using CalamityStyleTrailRenderer
        /// with Ode to Joy colors. Uses the Nature trail style by default (green-gold).
        /// </summary>
        public static void DrawProjectileTrail(Projectile proj, float width = 30f,
            Color? primary = null, Color? secondary = null, float intensity = 1f)
        {
            Color p = primary ?? OdeToJoyPalette.VerdantGreen;
            Color s = secondary ?? OdeToJoyPalette.GoldenPollen;
            CalamityStyleTrailRenderer.DrawProjectileTrail(proj, CalamityStyleTrailRenderer.TrailStyle.Nature,
                width, p, s, intensity);
        }

        /// <summary>
        /// Draws a VertexStrip trail with multi-pass bloom for a projectile.
        /// Creates the "body + glow halo" look.
        /// </summary>
        public static void DrawProjectileTrailWithBloom(Projectile proj, float width = 30f,
            Color? primary = null, Color? secondary = null,
            float intensity = 1f, float bloomMult = 2.5f)
        {
            Color p = primary ?? OdeToJoyPalette.VerdantGreen;
            Color s = secondary ?? OdeToJoyPalette.GoldenPollen;
            CalamityStyleTrailRenderer.DrawProjectileTrailWithBloom(proj, CalamityStyleTrailRenderer.TrailStyle.Nature,
                width, p, s, intensity, bloomMult);
        }

        /// <summary>
        /// Draws a custom trail from a position buffer (not tied to a projectile).
        /// Useful for swing trails and custom effect paths.
        /// </summary>
        public static void DrawTrailFromPositions(Vector2[] positions, float width = 30f,
            Color? primary = null, Color? secondary = null, float intensity = 1f,
            CalamityStyleTrailRenderer.TrailStyle style = CalamityStyleTrailRenderer.TrailStyle.Nature)
        {
            Color p = primary ?? OdeToJoyPalette.VerdantGreen;
            Color s = secondary ?? OdeToJoyPalette.GoldenPollen;
            CalamityStyleTrailRenderer.DrawTrail(positions, style, width, p, s, intensity);
        }

        // ═════════════════════════════════════════════════════════════════
        //  MUSICAL VFX — Note particles, harmonic pulses, rhythmic effects
        // ═════════════════════════════════════════════════════════════════

        /// <summary>
        /// Spawns a ring of expanding music note particles.
        /// Used at combo phase transitions and finishers.
        /// </summary>
        public static void HarmonicPulseRing(Vector2 position, float radius, int noteCount,
            Color? color = null, float speed = 3f)
        {
            Color col = color ?? OdeToJoyPalette.GoldenPollen;

            for (int i = 0; i < noteCount; i++)
            {
                float angle = MathHelper.TwoPi * i / noteCount;
                Vector2 vel = angle.ToRotationVector2() * speed;
                Vector2 offset = angle.ToRotationVector2() * radius * 0.3f;

                try
                {
                    var sparkle = new GlowSparkParticle(
                        position + offset, vel,
                        col with { A = 0 },
                        0.3f, Main.rand.Next(20, 35));
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
                catch
                {
                    Dust d = Dust.NewDustPerfect(position + offset, DustID.GoldFlame, vel, 0, col, 0.8f);
                    d.noGravity = true;
                }
            }
        }

        /// <summary>
        /// Spawns a cascade of music note dust rising upward.
        /// Perfect for celebration/finisher moments.
        /// </summary>
        public static void MusicNoteCascade(Vector2 position, int count, float spread,
            Color? primary = null, Color? secondary = null)
        {
            Color p = primary ?? OdeToJoyPalette.GoldenPollen;
            Color s = secondary ?? OdeToJoyPalette.SunlightYellow;

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / Math.Max(1, count - 1);
                Color col = Color.Lerp(p, s, t);
                Vector2 vel = new Vector2(
                    Main.rand.NextFloat(-spread, spread),
                    Main.rand.NextFloat(-6f, -2f));

                try
                {
                    var note = new SparkleParticle(
                        position + Main.rand.NextVector2Circular(spread * 2f, 8f),
                        vel, col with { A = 0 },
                        Main.rand.NextFloat(0.2f, 0.5f),
                        Main.rand.Next(25, 50));
                    MagnumParticleHandler.SpawnParticle(note);
                }
                catch
                {
                    Dust d = Dust.NewDustPerfect(position, DustID.YellowStarDust, vel, 0, col, 0.7f);
                    d.noGravity = true;
                }
            }
        }

        /// <summary>
        /// Spawns a rhythmic pulsing bloom ring that expands outward.
        /// </summary>
        public static void RhythmicPulse(Vector2 position, float scale, Color? color = null)
        {
            Color col = color ?? OdeToJoyPalette.GoldenPollen;

            try
            {
                var ring = new BloomRingParticle(
                    position, Vector2.Zero,
                    col with { A = 0 },
                    scale * 0.3f, 25);
                MagnumParticleHandler.SpawnParticle(ring);
            }
            catch { }
        }

        /// <summary>
        /// Full celebration burst — combines harmonic pulse ring, music note cascade,
        /// rhythmic pulse, bloom stack, and screen effects.
        /// </summary>
        public static void CelebrationBurst(Vector2 position, float scale, bool withScreenEffects = true)
        {
            HarmonicPulseRing(position, 40f * scale, 12 + (int)(8 * scale));
            MusicNoteCascade(position, 8 + (int)(6 * scale), 4f * scale);
            RhythmicPulse(position, scale);

            // Layered garden + blossom sparkle bursts
            SpawnGardenSparkleExplosion(position, (int)(12 * scale), 6f * scale, 0.35f);
            SpawnBlossomSparkleExplosion(position, (int)(10 * scale), 5f * scale, 0.3f);
            SpawnTriumphantStarburst(position, scale * 0.8f);

            if (withScreenEffects)
            {
                ScreenShake(4f * scale, 10);
                ScreenFlash(OdeToJoyPalette.GoldenPollen, 0.6f * scale);
            }
        }
    }
}
