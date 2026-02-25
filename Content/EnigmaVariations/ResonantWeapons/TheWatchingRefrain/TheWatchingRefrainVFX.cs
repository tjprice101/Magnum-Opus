using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons
{
    /// <summary>
    /// VFX helper for The Watching Refrain summon weapon.
    /// Handles hold-item ambient, item bloom, phantom summoning,
    /// minion ambient/attack, phantom bolt trail/impact,
    /// paradox rift ambient/impact, and mystery zone ambient/damage VFX.
    /// Call from TheWatchingRefrain, UnsolvedPhantomMinion, PhantomBolt,
    /// PhantomRift, and MysteryZone.
    /// </summary>
    public static class TheWatchingRefrainVFX
    {
        // =====================================================================
        //  HOLD ITEM VFX
        // =====================================================================

        /// <summary>
        /// Per-frame held-item VFX: watching eye orbit, phantom wisp preview,
        /// and collective glyph formation at 3+ minions.
        /// </summary>
        public static void HoldItemVFX(Player player, int phantomCount)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Watching eye orbits the player when phantoms are active
            if (Main.rand.NextBool(25) && phantomCount > 0)
            {
                float eyeAngle = time * 0.02f;
                Vector2 eyePos = center + eyeAngle.ToRotationVector2() * 40f;
                CustomParticles.EnigmaEyeGaze(eyePos, EnigmaPalette.EyeGreen * 0.5f, 0.22f);
            }

            // Phantom wisp preview — ghostly wisps rise above player
            if (Main.rand.NextBool(18))
            {
                Vector2 previewPos = center + new Vector2(Main.rand.NextFloat(-40f, 40f), -40f);
                Color col = EnigmaPalette.GetEnigmaGradient(Main.rand.NextFloat()) * 0.4f;
                var wisp = new GenericGlowParticle(previewPos, new Vector2(0, -0.5f),
                    col, 0.15f, 15, true);
                MagnumParticleHandler.SpawnParticle(wisp);
            }

            // Collective glyph formation at 3+ minions
            if (phantomCount >= 3 && Main.rand.NextBool(12))
            {
                for (int i = 0; i < phantomCount; i++)
                {
                    float formAngle = time * 0.03f + MathHelper.TwoPi * i / phantomCount;
                    Vector2 glyphPos = center + formAngle.ToRotationVector2() * 50f;
                    CustomParticles.Glyph(glyphPos, EnigmaPalette.GetEnigmaGradient((float)i / phantomCount), 0.2f);
                }
            }

            // Ambient mystery mist
            if (Main.rand.NextBool(20))
            {
                Vector2 mistPos = center + Main.rand.NextVector2Circular(30f, 30f);
                var mist = new GenericGlowParticle(mistPos, Main.rand.NextVector2Circular(0.3f, 0.3f),
                    EnigmaPalette.MysteryMist * 0.3f, 0.15f, 20, true);
                MagnumParticleHandler.SpawnParticle(mist);
            }

            // Ambient phantom light — scales with minion count
            float intensity = 0.2f + phantomCount * 0.05f;
            float pulse = 0.3f + MathF.Sin(time * 0.05f) * 0.1f;
            Lighting.AddLight(center, EnigmaPalette.Purple.ToVector3() * pulse * intensity);
        }

        // =====================================================================
        //  PREDRAW IN WORLD BLOOM
        // =====================================================================

        /// <summary>
        /// Standard 3-layer PreDrawInWorld bloom for the summoning staff.
        /// Enhanced bloom with color shift for the summon weapon's mystical presence.
        /// </summary>
        public static void PreDrawInWorldBloom(
            Microsoft.Xna.Framework.Graphics.SpriteBatch sb,
            Microsoft.Xna.Framework.Graphics.Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.04f) * 0.10f;
            EnigmaPalette.DrawItemBloomEnhanced(sb, tex, pos, origin, rotation, scale, pulse, time * 0.03f);
        }

        // =====================================================================
        //  SUMMON VFX
        // =====================================================================

        /// <summary>
        /// One-shot VFX when a new Unsolved Phantom is summoned.
        /// Glyph circle, eye burst, gradient halos, music notes, bloom flash.
        /// </summary>
        public static void SummonVFX(Vector2 spawnPos)
        {
            if (Main.dedServ) return;

            // Summoning flash — dual flare
            CustomParticles.GenericFlare(spawnPos, EnigmaPalette.GreenFlame, 0.7f, 18);
            CustomParticles.GenericFlare(spawnPos, EnigmaPalette.Purple, 0.5f, 16);

            // Glyph circle — the phantom's seal forms
            EnigmaVFXLibrary.SpawnGlyphCircle(spawnPos, 6, 40f, 0.07f);

            // Eye burst — the phantom opens its many eyes
            EnigmaVFXLibrary.SpawnWatchingEyes(spawnPos, 4, 30f, 0.4f);

            // Gradient halo rings — summoning resonance
            EnigmaVFXLibrary.SpawnGradientHaloRings(spawnPos, 3, 0.3f);

            // Music notes — the refrain begins
            EnigmaVFXLibrary.SpawnMusicNotes(spawnPos, 4, 25f, 0.8f, 1.0f, 30);

            // Radial dust burst — phantom coalescing
            EnigmaVFXLibrary.SpawnRadialDustBurst(spawnPos, 10, 4f);

            // Bloom
            EnigmaVFXLibrary.DrawBloom(spawnPos, 0.5f);

            Lighting.AddLight(spawnPos, EnigmaPalette.GreenFlame.ToVector3() * 0.8f);
        }

        // =====================================================================
        //  MINION AMBIENT VFX
        // =====================================================================

        /// <summary>
        /// Per-frame minion particles: dense dust, sparkles, shimmer,
        /// pearlescent void, flares, phase-out effects, glyph orbit, music notes.
        /// Scales with the phantom's current visibility.
        /// </summary>
        public static void MinionAmbientVFX(Vector2 pos, float visibility)
        {
            if (Main.dedServ) return;

            // Dense void dust (2 per frame) — Calamity-standard density
            for (int d = 0; d < 2; d++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(15f, 15f);
                Color col = EnigmaPalette.GetEnigmaGradient(Main.rand.NextFloat(0.2f, 0.8f));
                Dust dust = Dust.NewDustPerfect(pos + offset, DustID.PurpleTorch,
                    Main.rand.NextVector2Circular(2f, 2f), 0, col, (1.1f + Main.rand.NextFloat(0.3f)) * visibility);
                dust.noGravity = true;
                dust.fadeIn = 1.4f;
            }

            // Contrasting green torch dust (1-in-2)
            if (Main.rand.NextBool(2))
            {
                Vector2 offset = Main.rand.NextVector2Circular(12f, 12f);
                Dust d = Dust.NewDustPerfect(pos + offset, DustID.GreenTorch,
                    Main.rand.NextVector2Circular(1.5f, 1.5f), 0,
                    EnigmaPalette.GreenFlame, (0.9f + Main.rand.NextFloat(0.3f)) * visibility);
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }

            // Contrasting sparkles (1-in-2)
            if (Main.rand.NextBool(2) && visibility > 0.5f)
            {
                Color sparkleCol = Main.rand.NextBool() ? EnigmaPalette.Purple : EnigmaPalette.GreenFlame;
                var sparkle = new SparkleParticle(pos + Main.rand.NextVector2Circular(20f, 20f),
                    Main.rand.NextVector2Circular(1.5f, 1.5f),
                    sparkleCol * visibility, 0.4f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Enigma shimmer via hslToRgb (1-in-3)
            if (Main.rand.NextBool(3) && visibility > 0.5f)
            {
                float hue = 0.28f + Main.rand.NextFloat(0.17f);
                Color shimmer = Main.hslToRgb(hue, 0.85f, 0.65f);
                var glow = new GenericGlowParticle(pos + Main.rand.NextVector2Circular(15f, 15f),
                    Main.rand.NextVector2Circular(2f, 2f),
                    shimmer * visibility * 0.9f, 0.38f, 18, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Pearlescent void effects (1-in-4)
            if (Main.rand.NextBool(4) && visibility > 0.5f)
            {
                float pearlShift = MathF.Sin((float)Main.timeForVisualEffects * 0.12f) * 0.5f + 0.5f;
                Color pearlCol = Color.Lerp(EnigmaPalette.VoidBlack, EnigmaPalette.Purple, pearlShift);
                pearlCol = Color.Lerp(pearlCol, EnigmaPalette.GreenFlame, pearlShift * 0.4f);
                var pearl = new GenericGlowParticle(pos + Main.rand.NextVector2Circular(12f, 12f),
                    Main.rand.NextVector2Circular(1f, 1f),
                    pearlCol * visibility * 0.85f, 0.32f, 18, true);
                MagnumParticleHandler.SpawnParticle(pearl);
            }

            // Frequent flares (1-in-2)
            if (Main.rand.NextBool(2) && visibility > 0.4f)
            {
                CustomParticles.GenericFlare(pos + Main.rand.NextVector2Circular(12f, 12f),
                    EnigmaPalette.GetEnigmaGradient(Main.rand.NextFloat()) * visibility * 0.8f, 0.38f, 15);
            }

            // Phase-out void particles when fading
            if (visibility < 0.6f && Main.GameUpdateCount % 15 == 0)
            {
                var voidGlow = new GenericGlowParticle(pos + Main.rand.NextVector2Circular(20f, 20f),
                    Main.rand.NextVector2Circular(1.5f, 1.5f),
                    EnigmaPalette.VoidBlack * 0.6f, 0.25f, 16, true);
                MagnumParticleHandler.SpawnParticle(voidGlow);
            }

            // Rotating glyph aura (periodic)
            if (Main.GameUpdateCount % 25 == 0)
            {
                CustomParticles.GlyphCircle(pos, EnigmaPalette.GlyphPurple * visibility * 0.7f,
                    count: 3, radius: 32f, rotationSpeed: 0.04f);
            }

            // Orbiting sparkle wisps (periodic)
            if (Main.GameUpdateCount % 15 == 0)
            {
                float baseAngle = Main.GameUpdateCount * 0.04f;
                for (int i = 0; i < 3; i++)
                {
                    float angle = baseAngle + MathHelper.TwoPi * i / 3f;
                    Vector2 wispPos = pos + angle.ToRotationVector2() * 35f;
                    CustomParticles.GenericFlare(wispPos,
                        EnigmaPalette.GetEnigmaGradient((float)i / 3f) * visibility, 0.4f * visibility, 15);
                }
            }

            // Music notes — the phantom's melody (1-in-6)
            if (Main.rand.NextBool(6) && visibility > 0.5f)
                EnigmaVFXLibrary.SpawnMusicNotes(pos, 1, 15f, 0.7f, 0.85f, 25);

            // Pulsing mystery light
            float pulse = 0.35f + MathF.Sin((float)Main.timeForVisualEffects * 0.12f) * 0.1f;
            Lighting.AddLight(pos, EnigmaPalette.Purple.ToVector3() * pulse * visibility);
        }

        // =====================================================================
        //  MINION ATTACK VFX
        // =====================================================================

        /// <summary>
        /// Attack launch VFX: flare, halo, gazing eye toward target,
        /// glyph burst, directional sparkle beam, music notes.
        /// </summary>
        public static void MinionAttackVFX(Vector2 minionPos, Vector2 direction)
        {
            if (Main.dedServ) return;

            // Attack flash
            CustomParticles.GenericFlare(minionPos, EnigmaPalette.GreenFlame, 0.6f, 16);
            CustomParticles.HaloRing(minionPos, EnigmaPalette.GlyphPurple, 0.35f, 12);

            // Eye gaze toward attack direction
            Vector2 gazeTarget = minionPos + direction * 100f;
            CustomParticles.EnigmaEyeGaze(minionPos, EnigmaPalette.EyeGreen * 0.8f, 0.35f, gazeTarget);

            // Glyph burst at launch point
            EnigmaVFXLibrary.SpawnGlyphBurst(minionPos, 3, 3f);

            // Sparkle targeting beam along direction
            for (int i = 0; i < 3; i++)
            {
                Vector2 beamPos = minionPos + direction * (10f + i * 12f);
                Color col = EnigmaPalette.GetEnigmaGradient((float)i / 3f);
                CustomParticles.GenericFlare(beamPos, col * 0.7f, 0.32f, 12);
            }

            // Music notes
            EnigmaVFXLibrary.SpawnMusicNotes(minionPos, 2, 15f, 0.7f, 0.95f, 25);

            Lighting.AddLight(minionPos, EnigmaPalette.GreenFlame.ToVector3() * 0.6f);
        }

        // =====================================================================
        //  PHANTOM BOLT TRAIL VFX
        // =====================================================================

        /// <summary>
        /// Per-frame phantom bolt trail: dense dual-color dust,
        /// sparkles, enigma shimmer, pearlescent void, flares, music notes.
        /// </summary>
        public static void PhantomBoltTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 trailDir = -velocity.SafeNormalize(Vector2.Zero);

            // Dense void dust (2 per frame)
            EnigmaVFXLibrary.SpawnSwingDust(pos, trailDir);

            // Contrasting green sparkle
            EnigmaVFXLibrary.SpawnContrastSparkle(pos, trailDir);

            // Contrasting sparkles (1-in-2)
            if (Main.rand.NextBool(2))
            {
                Color sparkleCol = Main.rand.NextBool() ? EnigmaPalette.Purple : EnigmaPalette.GreenFlame;
                var sparkle = new SparkleParticle(pos + Main.rand.NextVector2Circular(8f, 8f),
                    -velocity * 0.06f + Main.rand.NextVector2Circular(1.5f, 1.5f),
                    sparkleCol, 0.4f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Enigma shimmer (1-in-3)
            if (Main.rand.NextBool(3))
            {
                float hue = 0.28f + Main.rand.NextFloat(0.17f);
                Color shimmer = Main.hslToRgb(hue, 0.85f, 0.65f);
                var glow = new GenericGlowParticle(pos + Main.rand.NextVector2Circular(5f, 5f),
                    -velocity * 0.1f + Main.rand.NextVector2Circular(1.5f, 1.5f),
                    shimmer * 0.9f, 0.35f, 16, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Pearlescent void (1-in-4)
            if (Main.rand.NextBool(4))
            {
                float pearlShift = MathF.Sin((float)Main.timeForVisualEffects * 0.12f) * 0.5f + 0.5f;
                Color pearlCol = Color.Lerp(EnigmaPalette.VoidBlack, EnigmaPalette.Purple, pearlShift);
                pearlCol = Color.Lerp(pearlCol, EnigmaPalette.GreenFlame, pearlShift * 0.4f);
                var pearl = new GenericGlowParticle(pos,
                    -velocity * 0.05f + Main.rand.NextVector2Circular(1f, 1f),
                    pearlCol * 0.85f, 0.3f, 18, true);
                MagnumParticleHandler.SpawnParticle(pearl);
            }

            // Frequent flares (1-in-2)
            if (Main.rand.NextBool(2))
            {
                CustomParticles.GenericFlare(pos,
                    EnigmaPalette.GetEnigmaGradient(Main.rand.NextFloat()) * 0.8f, 0.38f, 15);
            }

            // Music note trail (1-in-6)
            if (Main.rand.NextBool(6))
                EnigmaVFXLibrary.SpawnMusicNotes(pos, 1, 8f, 0.7f, 0.85f, 20);

            EnigmaVFXLibrary.AddPulsingLight(pos, 0.4f);
        }

        // =====================================================================
        //  PHANTOM BOLT IMPACT VFX
        // =====================================================================

        /// <summary>
        /// Phantom bolt on-hit: cascading sparkle ring, watching eye,
        /// glyph circle, halo rings, music notes, radial dust burst.
        /// </summary>
        public static void PhantomBoltImpactVFX(Vector2 pos, Vector2 targetCenter)
        {
            if (Main.dedServ) return;

            // Impact flare
            CustomParticles.GenericFlare(pos, EnigmaPalette.GreenFlame, 0.5f, 16);
            CustomParticles.HaloRing(pos, EnigmaPalette.Purple, 0.4f, 15);

            // Cascading sparkle ring around impact
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 offset = angle.ToRotationVector2() * 25f;
                Color col = EnigmaPalette.GetEnigmaGradient((float)i / 8f);
                CustomParticles.GenericFlare(pos + offset, col, 0.4f, 15);
            }

            // Cascading sparkle shower above impact
            for (int i = 0; i < 6; i++)
            {
                float cascadeAngle = MathHelper.TwoPi * i / 6f;
                Vector2 cascadePos = pos - new Vector2(0, 28f) + cascadeAngle.ToRotationVector2() * 15f;
                CustomParticles.GenericFlare(cascadePos, EnigmaPalette.GetEnigmaGradient((float)i / 6f), 0.38f, 15);
            }
            CustomParticles.HaloRing(pos, EnigmaPalette.GreenFlame * 0.7f, 0.32f, 14);

            // Watching eye at impact — the phantom sees through its bolt
            CustomParticles.EnigmaEyeImpact(pos, targetCenter, EnigmaPalette.EyeGreen, 0.5f);

            // Glyph circle formation
            EnigmaVFXLibrary.SpawnGlyphCircle(pos, 6, 45f, 0.06f);

            // Music notes burst
            EnigmaVFXLibrary.SpawnMusicNotes(pos, 3, 18f, 0.8f, 1.0f, 28);

            // Radial dust burst
            EnigmaVFXLibrary.SpawnRadialDustBurst(pos, 10, 5f);

            Lighting.AddLight(pos, EnigmaPalette.GreenFlame.ToVector3() * 0.8f);
        }

        // =====================================================================
        //  PHANTOM RIFT AMBIENT VFX
        // =====================================================================

        /// <summary>
        /// Per-frame paradox rift vortex: swirling inward particles,
        /// dense dust, sparkles, shimmer, central void pulse, music notes.
        /// Fades with opacity as rift decays.
        /// </summary>
        public static void PhantomRiftAmbientVFX(Vector2 pos, float opacity)
        {
            if (Main.dedServ) return;

            // Dense void dust (2 per frame)
            for (int d = 0; d < 2; d++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(18f, 18f);
                Color col = EnigmaPalette.GetEnigmaGradient(Main.rand.NextFloat(0.2f, 0.8f));
                Dust dust = Dust.NewDustPerfect(pos + offset, DustID.PurpleTorch,
                    Main.rand.NextVector2Circular(2f, 2f), 0, col, (1.0f + Main.rand.NextFloat(0.3f)) * opacity);
                dust.noGravity = true;
                dust.fadeIn = 1.4f;
            }

            // Contrasting green torch (1-in-2)
            if (Main.rand.NextBool(2))
            {
                Vector2 offset = Main.rand.NextVector2Circular(15f, 15f);
                Dust d = Dust.NewDustPerfect(pos + offset, DustID.GreenTorch,
                    Main.rand.NextVector2Circular(1.5f, 1.5f), 0,
                    EnigmaPalette.GreenFlame, (0.85f + Main.rand.NextFloat(0.25f)) * opacity);
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }

            // Swirling vortex particles spiraling inward (every 3 frames)
            if (Main.GameUpdateCount % 3 == 0)
            {
                for (int i = 0; i < 5; i++)
                {
                    float angle = Main.GameUpdateCount * 0.1f + MathHelper.TwoPi * i / 5f;
                    float radius = 22f + MathF.Sin(Main.GameUpdateCount * 0.12f + i) * 8f;
                    Vector2 particlePos = pos + angle.ToRotationVector2() * radius;
                    Vector2 vel = (pos - particlePos).SafeNormalize(Vector2.Zero) * 2.5f;
                    Color col = EnigmaPalette.GetEnigmaGradient((float)i / 5f) * opacity;
                    var glow = new GenericGlowParticle(particlePos, vel, col * 0.6f, 0.26f, 14, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
            }

            // Contrasting sparkles (1-in-2)
            if (Main.rand.NextBool(2) && opacity > 0.3f)
            {
                Color sparkleCol = Main.rand.NextBool() ? EnigmaPalette.Purple : EnigmaPalette.GreenFlame;
                var sparkle = new SparkleParticle(pos + Main.rand.NextVector2Circular(15f, 15f),
                    Main.rand.NextVector2Circular(1.5f, 1.5f),
                    sparkleCol * opacity, 0.38f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Shimmer (1-in-3)
            if (Main.rand.NextBool(3) && opacity > 0.3f)
            {
                float hue = 0.28f + Main.rand.NextFloat(0.17f);
                Color shimmer = Main.hslToRgb(hue, 0.85f, 0.65f);
                var glow = new GenericGlowParticle(pos + Main.rand.NextVector2Circular(12f, 12f),
                    Main.rand.NextVector2Circular(2f, 2f),
                    shimmer * opacity * 0.9f, 0.32f, 16, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Central void pulse (every 6 frames)
            if (Main.GameUpdateCount % 6 == 0)
            {
                CustomParticles.GenericFlare(pos, EnigmaPalette.VoidBlack * opacity, 0.38f, 12);
            }

            // Watching eye at rift center (rare)
            if (Main.rand.NextBool(20))
                CustomParticles.EnigmaEyeGaze(pos, EnigmaPalette.EyeGreen * 0.4f * opacity, 0.25f);

            // Music notes — the rift's refrain (1-in-6)
            if (Main.rand.NextBool(6) && opacity > 0.3f)
                EnigmaVFXLibrary.SpawnMusicNotes(pos, 1, 18f, 0.7f, 0.85f, 25);

            // Pulsing rift light
            float pulse = 0.2f + MathF.Sin((float)Main.timeForVisualEffects * 0.12f) * 0.1f;
            Lighting.AddLight(pos, EnigmaPalette.Purple.ToVector3() * pulse * opacity);
        }

        // =====================================================================
        //  PHANTOM RIFT IMPACT VFX
        // =====================================================================

        /// <summary>
        /// Rift damage indicator when an enemy is hurt by the paradox rift.
        /// Subtle flare, watching eye, glyph accent.
        /// </summary>
        public static void PhantomRiftImpactVFX(Vector2 pos, Vector2 targetCenter)
        {
            if (Main.dedServ) return;

            // Damage indicator flare
            CustomParticles.GenericFlare(pos, EnigmaPalette.Purple * 0.5f, 0.3f, 10);

            // Watching eye toward the damaged enemy
            CustomParticles.EnigmaEyeImpact(pos, targetCenter, EnigmaPalette.EyeGreen, 0.35f);

            // Glyph accent
            EnigmaVFXLibrary.SpawnGlyphAccent(pos, 0.2f);

            // Music note
            EnigmaVFXLibrary.SpawnMusicNotes(pos, 1, 12f, 0.6f, 0.8f, 20);

            Lighting.AddLight(pos, EnigmaPalette.GreenFlame.ToVector3() * 0.4f);
        }

        // =====================================================================
        //  MYSTERY ZONE AMBIENT VFX
        // =====================================================================

        /// <summary>
        /// Per-frame mystery zone particles: outer sparkle ring,
        /// multi-radius glyph circles, swirling inward particles,
        /// dense dust, contrasting sparkles, shimmer, music notes, eyes.
        /// </summary>
        public static void MysteryZoneAmbientVFX(Vector2 center, float opacity)
        {
            if (Main.dedServ) return;

            float zoneRadius = 120f;

            // Dense void dust throughout zone (2 per frame)
            for (int d = 0; d < 2; d++)
            {
                float dustAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dustDist = Main.rand.NextFloat(20f, zoneRadius * 0.6f);
                Vector2 dustPos = center + dustAngle.ToRotationVector2() * dustDist;
                Color col = EnigmaPalette.GetEnigmaGradient(Main.rand.NextFloat(0.2f, 0.8f));
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.PurpleTorch,
                    Main.rand.NextVector2Circular(2f, 2f), 0, col, (1.0f + Main.rand.NextFloat(0.3f)) * opacity);
                dust.noGravity = true;
                dust.fadeIn = 1.4f;
            }

            // Contrasting green torch (1-in-2)
            if (Main.rand.NextBool(2))
            {
                float dustAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dustDist = Main.rand.NextFloat(15f, zoneRadius * 0.5f);
                Vector2 dustPos = center + dustAngle.ToRotationVector2() * dustDist;
                Dust d = Dust.NewDustPerfect(dustPos, DustID.GreenTorch,
                    Main.rand.NextVector2Circular(1.5f, 1.5f), 0,
                    EnigmaPalette.GreenFlame, (0.85f + Main.rand.NextFloat(0.25f)) * opacity);
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }

            // Outer sparkle ring pulsing inward (every 12 frames)
            if (Main.GameUpdateCount % 12 == 0)
            {
                float baseAngle = Main.GameUpdateCount * 0.02f;
                for (int i = 0; i < 6; i++)
                {
                    float angle = baseAngle + MathHelper.TwoPi * i / 6f;
                    Vector2 sparklePos = center + angle.ToRotationVector2() * (zoneRadius - 10f);
                    CustomParticles.GenericFlare(sparklePos,
                        EnigmaPalette.GetEnigmaGradient((float)i / 6f) * opacity, 0.4f * opacity, 15);

                    // Inward sparkle trail
                    Vector2 inwardVel = (center - sparklePos).SafeNormalize(Vector2.Zero) * 2f;
                    var trail = new GenericGlowParticle(sparklePos, inwardVel,
                        EnigmaPalette.Purple * opacity * 0.5f, 0.25f, 12, true);
                    MagnumParticleHandler.SpawnParticle(trail);
                }
            }

            // Multi-radius glyph circles (every 15 frames)
            if (Main.GameUpdateCount % 15 == 0)
            {
                CustomParticles.GlyphCircle(center, EnigmaPalette.Purple * opacity,
                    count: 8, radius: zoneRadius * 0.8f, rotationSpeed: 0.03f);
                CustomParticles.GlyphCircle(center, EnigmaPalette.GreenFlame * opacity * 0.7f,
                    count: 5, radius: zoneRadius * 0.5f, rotationSpeed: -0.04f);
            }

            // Swirling particles throughout zone (every 6 frames)
            if (Main.GameUpdateCount % 6 == 0)
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float radius = Main.rand.NextFloat(20f, zoneRadius);
                Vector2 particlePos = center + angle.ToRotationVector2() * radius;

                // Spiral inward with tangential component
                Vector2 vel = (center - particlePos).SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2) * 2f;
                vel += (center - particlePos).SafeNormalize(Vector2.Zero) * 0.5f;

                var glow = new GenericGlowParticle(particlePos, vel,
                    EnigmaPalette.GetEnigmaGradient(radius / zoneRadius) * opacity * 0.5f,
                    0.2f, 20, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Contrasting sparkles (1-in-2)
            if (Main.rand.NextBool(2) && opacity > 0.3f)
            {
                Color sparkleCol = Main.rand.NextBool() ? EnigmaPalette.Purple : EnigmaPalette.GreenFlame;
                float sparkleRadius = Main.rand.NextFloat(20f, zoneRadius * 0.6f);
                float sparkleAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 sparklePos = center + sparkleAngle.ToRotationVector2() * sparkleRadius;
                var sparkle = new SparkleParticle(sparklePos, Main.rand.NextVector2Circular(1.5f, 1.5f),
                    sparkleCol * opacity, 0.4f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Shimmer (1-in-3)
            if (Main.rand.NextBool(3) && opacity > 0.3f)
            {
                float hue = 0.28f + Main.rand.NextFloat(0.17f);
                Color shimmer = Main.hslToRgb(hue, 0.85f, 0.65f);
                float shimmerRadius = Main.rand.NextFloat(15f, zoneRadius * 0.5f);
                float shimmerAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 shimmerPos = center + shimmerAngle.ToRotationVector2() * shimmerRadius;
                var glow = new GenericGlowParticle(shimmerPos, Main.rand.NextVector2Circular(2f, 2f),
                    shimmer * opacity * 0.9f, 0.35f, 18, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Central dark core (every 6 frames)
            if (Main.GameUpdateCount % 6 == 0)
            {
                CustomParticles.GenericFlare(center, EnigmaPalette.VoidBlack * opacity, 0.4f, 12);
            }

            // Edge eyes watching inward (rare)
            if (Main.rand.NextBool(15))
            {
                float eyeAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 edgePos = center + eyeAngle.ToRotationVector2() * zoneRadius;
                CustomParticles.EnigmaEyeGaze(edgePos, EnigmaPalette.EyeGreen * 0.4f * opacity, 0.2f, center);
            }

            // Music notes — the zone's whisper (1-in-6)
            if (Main.rand.NextBool(6))
            {
                float noteRadius = Main.rand.NextFloat(20f, zoneRadius * 0.7f);
                float noteAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 notePos = center + noteAngle.ToRotationVector2() * noteRadius;
                EnigmaVFXLibrary.SpawnMusicNotes(notePos, 1, 10f, 0.7f, 0.85f, 30);
            }

            // Pulsing mystery light
            float pulse = 0.25f + MathF.Sin((float)Main.timeForVisualEffects * 0.1f) * 0.1f;
            Lighting.AddLight(center, EnigmaPalette.Purple.ToVector3() * pulse * opacity);
        }

        // =====================================================================
        //  MYSTERY ZONE DAMAGE VFX
        // =====================================================================

        /// <summary>
        /// Zone damage indicator flare on an enemy inside the mystery zone.
        /// Subtle purple flare and glyph accent.
        /// </summary>
        public static void MysteryZoneDamageVFX(Vector2 targetPos)
        {
            if (Main.dedServ) return;

            // Damage flare
            CustomParticles.GenericFlare(targetPos, EnigmaPalette.Purple * 0.5f, 0.22f, 8);

            // Glyph accent
            EnigmaVFXLibrary.SpawnGlyphAccent(targetPos, 0.18f);

            // Subtle eye flash
            if (Main.rand.NextBool(3))
                CustomParticles.EnigmaEyeGaze(targetPos + new Vector2(0, -15f), EnigmaPalette.EyeGreen * 0.3f, 0.15f);

            Lighting.AddLight(targetPos, EnigmaPalette.GreenFlame.ToVector3() * 0.3f);
        }
    }
}
