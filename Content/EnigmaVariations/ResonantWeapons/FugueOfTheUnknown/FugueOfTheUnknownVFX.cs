using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons
{
    /// <summary>
    /// VFX helper for Fugue of the Unknown — "Every Voice Adds Another Secret".
    /// Orbiting voice projectiles that build into harmonic convergence.
    /// Visual identity: contrapuntal, layered, multiplying, interweaving.
    /// </summary>
    public static class FugueOfTheUnknownVFX
    {
        // Per-weapon accent colors
        private static readonly Color VoiceGlow = new Color(100, 200, 160);     // Riddle shimmer — each voice's tone
        private static readonly Color HarmonicFlash = new Color(220, 255, 230); // White-green — convergence peak
        private static readonly Color EchoMark = new Color(160, 80, 220);       // Weapon purple — echo resonance

        // =====================================================================
        //  HOLD ITEM VFX — fugue aura with orbiting glyphs and eyes
        // =====================================================================

        public static void HoldItemVFX(Player player, int activeVoiceCount)
        {
            if (Main.dedServ) return;

            float time = Main.GameUpdateCount * 0.03f;

            // Orbiting voice indicators proportional to active voices
            if (Main.rand.NextBool(6) && activeVoiceCount > 0)
            {
                for (int i = 0; i < activeVoiceCount; i++)
                {
                    float angle = time + MathHelper.TwoPi * i / Math.Max(activeVoiceCount, 1);
                    float radius = 28f + MathF.Sin(Main.GameUpdateCount * 0.04f + i * 0.9f) * 6f;
                    Vector2 voicePos = player.Center + angle.ToRotationVector2() * radius;
                    float progress = (float)i / Math.Max(activeVoiceCount, 1);
                    Color voiceColor = Color.Lerp(EnigmaPalette.Purple, VoiceGlow, progress);
                    var mote = new GenericGlowParticle(voicePos, Vector2.Zero, voiceColor * 0.6f, 0.2f, 14, true);
                    MagnumParticleHandler.SpawnParticle(mote);
                }
            }

            // Glyph accents around the player
            if (Main.rand.NextBool(15) && activeVoiceCount >= 2)
            {
                Vector2 glyphPos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                CustomParticles.Glyph(glyphPos, EnigmaPalette.GetEnigmaGradient(Main.rand.NextFloat()), 0.22f);
            }

            // Watching eyes — more voices = more watchers
            if (Main.rand.NextBool(25 - activeVoiceCount * 3))
            {
                Vector2 eyePos = player.Center + Main.rand.NextVector2Circular(40f, 40f);
                CustomParticles.EnigmaEyeGaze(eyePos, EnigmaPalette.EyeGreen * 0.5f, 0.25f);
            }

            // Music notes — the fugue's melody
            if (Main.rand.NextBool(10))
                EnigmaVFXLibrary.SpawnMusicNotes(player.Center, 1, 20f, 0.65f, 0.85f, 30);

            float intensity = 0.3f + activeVoiceCount * 0.06f;
            EnigmaVFXLibrary.AddPulsingLight(player.Center, Main.GameUpdateCount, intensity);
        }

        // =====================================================================
        //  VOICE PROJECTILE VFX — per-frame effects on orbiting voice projectile
        // =====================================================================

        /// <summary>
        /// Per-frame VFX for an orbiting voice projectile.
        /// Each voice has a distinct visual presence with trailing particles.
        /// </summary>
        public static void VoiceOrbitVFX(Vector2 voicePos, Vector2 velocity, int voiceIndex)
        {
            if (Main.dedServ) return;

            // Trailing glow particles
            if (Main.rand.NextBool(3))
            {
                float progress = (float)voiceIndex / 5f;
                Color trailColor = Color.Lerp(EnigmaPalette.Purple, VoiceGlow, progress) * 0.6f;
                var trail = new GenericGlowParticle(voicePos, -velocity * 0.3f + Main.rand.NextVector2Circular(1f, 1f),
                    trailColor, 0.18f, 15, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // Purple/green dust alternating
            if (Main.rand.NextBool(4))
            {
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.GreenTorch;
                Dust d = Dust.NewDustPerfect(voicePos, dustType,
                    Main.rand.NextVector2Circular(1.5f, 1.5f), 0, default, 1.0f);
                d.noGravity = true;
            }

            // Occasional watching eye on voice
            if (Main.rand.NextBool(20))
                CustomParticles.EnigmaEyeGaze(voicePos + Main.rand.NextVector2Circular(8f, 8f),
                    EnigmaPalette.EyeGreen * 0.5f, 0.2f);

            // Bloom
            BloomRenderer.DrawBloomStackAdditive(voicePos, EnigmaPalette.DeepPurple, VoiceGlow, 0.2f, 0.4f);

            Lighting.AddLight(voicePos, EnigmaPalette.Purple.ToVector3() * 0.4f);
        }

        // =====================================================================
        //  VOICE RELEASE VFX — when voices are launched at enemies
        // =====================================================================

        public static void VoiceReleaseVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            CustomParticles.GenericFlare(pos, EnigmaPalette.GreenFlame, 0.5f, 15);
            CustomParticles.HaloRing(pos, EnigmaPalette.Purple, 0.35f, 12);
            EnigmaVFXLibrary.SpawnMusicNotes(pos, 2, 12f, 0.7f, 0.95f, 25);
        }

        // =====================================================================
        //  HARMONIC CONVERGENCE VFX — when echo marks detonate
        // =====================================================================

        /// <summary>
        /// Harmonic Convergence explosion — all echo marks on an enemy detonate.
        /// The fugue's voices align in a single devastating chord.
        /// </summary>
        public static void HarmonicConvergenceVFX(Vector2 pos, int echoMarkCount)
        {
            if (Main.dedServ) return;

            float intensity = 0.7f + echoMarkCount * 0.1f;

            // Central chord flash
            CustomParticles.GenericFlare(pos, HarmonicFlash, 0.8f * intensity, 20);
            CustomParticles.GenericFlare(pos, EnigmaPalette.GreenFlame, 0.6f * intensity, 18);

            // Glyph burst — mystery symbols scatter
            EnigmaVFXLibrary.SpawnGlyphBurst(pos, 6 + echoMarkCount, 5f * intensity);

            // Eye burst
            EnigmaVFXLibrary.SpawnEyeImpactBurst(pos, 3 + echoMarkCount / 2, 4f);

            // Halo rings
            EnigmaVFXLibrary.SpawnGradientHaloRings(pos, 4, 0.3f * intensity);

            // Radial dust
            EnigmaVFXLibrary.SpawnRadialDustBurst(pos, 10 + echoMarkCount * 2, 6f * intensity);

            // Bloom
            EnigmaVFXLibrary.DrawBloom(pos, 0.5f * intensity);

            // Music note burst — the convergence chord
            EnigmaVFXLibrary.SpawnMusicNotes(pos, 4 + echoMarkCount, 30f, 0.8f, 1.1f, 30);

            Lighting.AddLight(pos, EnigmaPalette.GreenFlame.ToVector3() * 1.0f * intensity);
        }

        // =====================================================================
        //  ECHO MARK VFX — visual on a marked enemy
        // =====================================================================

        public static void EchoMarkVFX(Vector2 enemyCenter, int markCount)
        {
            if (Main.dedServ) return;

            // Orbiting mark indicators
            if (Main.rand.NextBool(8))
            {
                for (int i = 0; i < markCount; i++)
                {
                    float angle = Main.GameUpdateCount * 0.06f + MathHelper.TwoPi * i / markCount;
                    Vector2 markPos = enemyCenter + angle.ToRotationVector2() * 20f;
                    Color markColor = Color.Lerp(EchoMark, EnigmaPalette.GreenFlame, (float)i / markCount);
                    var glow = new GenericGlowParticle(markPos, Vector2.Zero, markColor * 0.5f, 0.15f, 10, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
            }

            // Warning glow near threshold
            if (markCount >= 2)
                Lighting.AddLight(enemyCenter, EnigmaPalette.GreenFlame.ToVector3() * 0.3f * markCount);
        }

        // =====================================================================
        //  VOICE TRAIL FUNCTIONS
        // =====================================================================

        public static Color VoiceTrailColor(float completionRatio)
        {
            Color c = Color.Lerp(EchoMark, VoiceGlow, completionRatio);
            float fade = 1f - completionRatio * 0.7f;
            return (c * fade) with { A = 0 };
        }

        public static float VoiceTrailWidth(float completionRatio)
        {
            float taper = 1f - completionRatio;
            return taper * 12f;
        }

        // =====================================================================
        //  PREDRAW BLOOM
        // =====================================================================

        public static void DrawWorldItemBloom(SpriteBatch sb, Texture2D texture,
            Vector2 position, Vector2 origin, float rotation, float scale)
        {
            float pulse = 1f + MathF.Sin(Main.GameUpdateCount * 0.04f) * 0.10f;
            float time = Main.GameUpdateCount * 0.03f;
            EnigmaPalette.DrawItemBloomEnhanced(sb, texture, position, origin, rotation, scale, pulse, time);
        }
    }
}
