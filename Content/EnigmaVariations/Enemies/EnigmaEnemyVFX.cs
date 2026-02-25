using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.EnigmaVariations.Enemies
{
    /// <summary>
    /// VFX helper for ALL Enigma Variations enemies.
    /// Provides attack VFX, ambient aura, death effects, and boss-phase VFX
    /// for Mystery's End and any future Enigma enemies.
    /// </summary>
    public static class EnigmaEnemyVFX
    {
        // =====================================================================
        //  AMBIENT AURA — per-frame NPC ambient VFX
        // =====================================================================

        /// <summary>
        /// Per-frame ambient aura for any Enigma enemy.
        /// Produces void mist, orbiting eyes, occasional glyphs.
        /// </summary>
        public static void AmbientAuraVFX(NPC npc, float intensity = 1f)
        {
            if (Main.dedServ) return;

            // Void mist particles
            if (Main.rand.NextBool(5))
            {
                Vector2 mistPos = npc.Center + Main.rand.NextVector2Circular(npc.width * 0.6f, npc.height * 0.6f);
                Color mistColor = EnigmaPalette.GetVoidGradient(Main.rand.NextFloat()) * 0.35f * intensity;
                var mist = new GenericGlowParticle(mistPos, Main.rand.NextVector2Circular(0.5f, 0.5f),
                    mistColor, 0.2f, 20, true);
                MagnumParticleHandler.SpawnParticle(mist);
            }

            // Orbiting watching eyes
            if (Main.rand.NextBool(15))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float radius = Math.Max(npc.width, npc.height) * 0.5f + 15f;
                Vector2 eyePos = npc.Center + angle.ToRotationVector2() * radius;
                NPC target = FindNearestPlayer(npc.Center);
                CustomParticles.EnigmaEyeGaze(eyePos, EnigmaPalette.EyeGreen * 0.5f * intensity, 0.25f,
                    target?.Center);
            }

            // Occasional glyph
            if (Main.rand.NextBool(25))
            {
                Vector2 glyphPos = npc.Center + Main.rand.NextVector2Circular(npc.width * 0.4f, npc.height * 0.4f);
                CustomParticles.Glyph(glyphPos, EnigmaPalette.GlyphPurple * intensity, 0.2f);
            }

            // Pulsing light
            float pulse = MathF.Sin(Main.GameUpdateCount * 0.06f) * 0.1f + 0.9f;
            Lighting.AddLight(npc.Center, EnigmaPalette.Purple.ToVector3() * 0.4f * intensity * pulse);
        }

        // =====================================================================
        //  ATTACK VFX — per-attack type visual effects
        // =====================================================================

        /// <summary>
        /// Paradox Gaze attack — eyes spawn around target player.
        /// </summary>
        public static void ParadoxGazeVFX(Vector2 targetCenter, int eyeCount = 6)
        {
            if (Main.dedServ) return;

            EnigmaVFXLibrary.SpawnWatchingEyes(targetCenter, eyeCount, 50f, 0.4f);

            // Central focus flash
            CustomParticles.GenericFlare(targetCenter, EnigmaPalette.EyeGreen * 0.6f, 0.4f, 14);

            // Glyph ring around target
            EnigmaVFXLibrary.SpawnGlyphCircle(targetCenter, 4, 45f, 0.05f);

            Lighting.AddLight(targetCenter, EnigmaPalette.EyeGreen.ToVector3() * 0.5f);
        }

        /// <summary>
        /// Glyph Cascade attack — raining arcane glyphs from above.
        /// </summary>
        public static void GlyphCascadeVFX(Vector2 sourcePos, int glyphCount = 8)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < glyphCount; i++)
            {
                float xOffset = Main.rand.NextFloat(-80f, 80f);
                Vector2 glyphPos = sourcePos + new Vector2(xOffset, -60f);
                Color col = EnigmaPalette.GetEnigmaGradient(Main.rand.NextFloat());
                CustomParticles.Glyph(glyphPos, col, 0.25f + Main.rand.NextFloat(0.1f));
            }

            CustomParticles.GenericFlare(sourcePos, EnigmaPalette.Purple, 0.5f, 14);
            EnigmaVFXLibrary.SpawnMusicNotes(sourcePos, 3, 30f, 0.7f, 0.95f, 25);
        }

        /// <summary>
        /// Watching Volley attack — projectile launch with trailing eyes.
        /// </summary>
        public static void WatchingVolleyLaunchVFX(Vector2 launchPos, Vector2 direction)
        {
            if (Main.dedServ) return;

            CustomParticles.GenericFlare(launchPos, EnigmaPalette.GreenFlame, 0.5f, 12);
            CustomParticles.HaloRing(launchPos, EnigmaPalette.Purple, 0.25f, 10);

            // Eyes following the volley direction
            for (int i = 0; i < 2; i++)
            {
                Vector2 eyePos = launchPos + direction * (10f + i * 12f);
                CustomParticles.EnigmaEyeGaze(eyePos, EnigmaPalette.EyeGreen * 0.6f, 0.3f,
                    launchPos + direction * 100f);
            }
        }

        /// <summary>
        /// Mystery Vortex attack — swirling glyph circle formation.
        /// </summary>
        public static void MysteryVortexVFX(Vector2 vortexCenter, float radius)
        {
            if (Main.dedServ) return;

            // Inward-spiraling void particles
            EnigmaVFXLibrary.SpawnVoidSwirl(vortexCenter, 8, radius * 1.5f);

            // Glyph circle
            EnigmaVFXLibrary.SpawnGlyphCircle(vortexCenter, 8, radius, 0.08f);

            // Central eye
            CustomParticles.EnigmaEyeGaze(vortexCenter, EnigmaPalette.EyeGreen * 0.7f, 0.4f);

            // Bloom at center
            BloomRenderer.DrawBloomStackAdditive(vortexCenter, EnigmaPalette.DeepPurple, EnigmaPalette.GreenFlame,
                0.4f, 0.6f);

            Lighting.AddLight(vortexCenter, EnigmaPalette.Purple.ToVector3() * 0.6f);
        }

        /// <summary>
        /// Enigma Revelation — ultimate eye explosion with glyph burst.
        /// The mystery's deepest attack.
        /// </summary>
        public static void EnigmaRevelationVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            // Central flash cascade
            CustomParticles.GenericFlare(pos, Color.White, 1.0f, 22);
            CustomParticles.GenericFlare(pos, EnigmaPalette.WhiteGreenFlash, 0.8f, 20);
            CustomParticles.GenericFlare(pos, EnigmaPalette.GreenFlame, 0.6f, 18);

            // Massive eye explosion
            EnigmaVFXLibrary.SpawnEyeImpactBurst(pos, 10, 6f);

            // Glyph explosion
            EnigmaVFXLibrary.SpawnGlyphCircle(pos, 10, 80f, 0.06f);
            EnigmaVFXLibrary.SpawnGlyphBurst(pos, 16, 8f);

            // Void swirl
            EnigmaVFXLibrary.SpawnVoidSwirl(pos, 10, 80f);

            // Halo rings
            EnigmaVFXLibrary.SpawnGradientHaloRings(pos, 7, 0.35f);

            // Radial dust burst
            EnigmaVFXLibrary.SpawnRadialDustBurst(pos, 20, 8f);

            // Bloom
            EnigmaVFXLibrary.DrawBloom(pos, 0.9f);

            // Music notes
            EnigmaVFXLibrary.SpawnMusicNotes(pos, 8, 50f, 0.9f, 1.2f, 40);

            MagnumScreenEffects.AddScreenShake(8f);
            Lighting.AddLight(pos, EnigmaPalette.WhiteGreenFlash.ToVector3() * 1.5f);
        }

        // =====================================================================
        //  DEATH VFX — enemy death effects
        // =====================================================================

        /// <summary>
        /// Standard Enigma enemy death VFX — eyes scatter, glyphs dissolve.
        /// </summary>
        public static void DeathVFX(Vector2 pos, float intensity = 1f)
        {
            if (Main.dedServ) return;

            EnigmaVFXLibrary.ProjectileImpact(pos, intensity);

            // Extra eye scatter on death
            EnigmaVFXLibrary.SpawnWatchingEyes(pos, 5, 40f * intensity, 0.3f);

            // Void swirl collapse
            EnigmaVFXLibrary.SpawnVoidSwirl(pos, 6, 50f * intensity);

            Lighting.AddLight(pos, EnigmaPalette.GreenFlame.ToVector3() * 0.8f * intensity);
        }

        /// <summary>
        /// Boss / mini-boss death VFX — enhanced with screen effects.
        /// </summary>
        public static void BossDeathVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            // Base death
            DeathVFX(pos, 1.5f);

            // Enhanced eye burst
            EnigmaVFXLibrary.SpawnEyeImpactBurst(pos, 12, 7f);

            // Glyph explosion
            EnigmaVFXLibrary.SpawnGlyphCircle(pos, 10, 80f, 0.05f);
            EnigmaVFXLibrary.SpawnGlyphBurst(pos, 20, 10f);

            // Massive bloom
            EnigmaVFXLibrary.DrawBloom(pos, 1.0f);

            // Screen effects
            MagnumScreenEffects.AddScreenShake(10f);
        }

        // =====================================================================
        //  PROJECTILE TRAIL VFX — for enemy projectiles
        // =====================================================================

        /// <summary>
        /// Per-frame trail VFX for an Enigma enemy projectile.
        /// </summary>
        public static void EnemyProjectileTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            // Dual-color dust trail
            if (Main.rand.NextBool(2))
            {
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.GreenTorch;
                Dust d = Dust.NewDustPerfect(pos, dustType,
                    -velocity * 0.1f + Main.rand.NextVector2Circular(0.5f, 0.5f), 0, default, 1.0f);
                d.noGravity = true;
            }

            // Trail eye (rare)
            if (Main.rand.NextBool(10))
                CustomParticles.EnigmaEyeGaze(pos + Main.rand.NextVector2Circular(5f, 5f),
                    EnigmaPalette.EyeGreen * 0.4f, 0.2f);

            Lighting.AddLight(pos, EnigmaPalette.Purple.ToVector3() * 0.25f);
        }

        /// <summary>
        /// Enemy projectile impact VFX.
        /// </summary>
        public static void EnemyProjectileImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            CustomParticles.GenericFlare(pos, EnigmaPalette.GreenFlame, 0.4f, 12);
            CustomParticles.HaloRing(pos, EnigmaPalette.Purple, 0.25f, 10);
            EnigmaVFXLibrary.SpawnRadialDustBurst(pos, 8, 4f);
            EnigmaVFXLibrary.SpawnMusicNotes(pos, 1, 10f, 0.6f, 0.85f, 20);

            Lighting.AddLight(pos, EnigmaPalette.GreenFlame.ToVector3() * 0.5f);
        }

        // =====================================================================
        //  EYE GLOW EFFECT — pulsing eye per-frame
        // =====================================================================

        /// <summary>
        /// Pulsing eye glow effect for enemies with visible eyes.
        /// Returns the current glow intensity for use in draw code.
        /// </summary>
        public static float UpdateEyeGlow(float currentGlow, float time, float baseIntensity = 0.6f)
        {
            float target = baseIntensity + MathF.Sin(time * 0.05f) * 0.2f;
            return MathHelper.Lerp(currentGlow, target, 0.05f);
        }

        /// <summary>
        /// Pulsing aura effect for enemies with mystery aura.
        /// Returns the current aura pulse value for use in draw code.
        /// </summary>
        public static float UpdateAuraPulse(float currentPulse, float time)
        {
            float target = MathF.Sin(time * 0.04f) * 0.5f + 0.5f;
            return MathHelper.Lerp(currentPulse, target, 0.03f);
        }

        // =====================================================================
        //  HELPER
        // =====================================================================

        private static NPC FindNearestPlayer(Vector2 center)
        {
            // Just return null — the eye gaze with null target looks outward randomly
            return null;
        }
    }
}
