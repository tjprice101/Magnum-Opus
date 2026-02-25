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
    /// VFX helper for Variations of the Void — "Three Questions. One Answer. The Void."
    /// Melee sword with converging void beams on finisher combo.
    /// Visual identity: cosmic, empty, absolute, void depth, converging mysteries.
    /// </summary>
    public static class VariationsOfTheVoidVFX
    {
        // Per-weapon accent colors
        private static readonly Color VoidDepth = new Color(8, 5, 12);           // Absolute void — the deepest dark
        private static readonly Color VoidFlame = new Color(60, 180, 90);        // Void flame — dark fire from nothing
        private static readonly Color ConvergenceFlash = new Color(100, 200, 160); // Riddle shimmer — convergence

        // =====================================================================
        //  HOLD ITEM VFX — void emanation while sword is held
        // =====================================================================

        public static void HoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            // Subtle void mist particles
            if (Main.rand.NextBool(12))
            {
                Vector2 mistPos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                Color mistColor = EnigmaPalette.GetVoidGradient(Main.rand.NextFloat()) * 0.3f;
                var mist = new GenericGlowParticle(mistPos, Main.rand.NextVector2Circular(0.5f, 0.5f),
                    mistColor, 0.18f, 18, true);
                MagnumParticleHandler.SpawnParticle(mist);
            }

            // Void watching eye
            if (Main.rand.NextBool(30))
            {
                Vector2 eyePos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                CustomParticles.EnigmaEyeGaze(eyePos, EnigmaPalette.EyeGreen * 0.4f, 0.25f);
            }

            // Glyph accent
            if (Main.rand.NextBool(20))
                EnigmaVFXLibrary.SpawnGlyphAccent(player.Center + Main.rand.NextVector2Circular(25f, 25f), 0.2f);

            // Music notes — the void's melody
            if (Main.rand.NextBool(15))
                EnigmaVFXLibrary.SpawnMusicNotes(player.Center, 1, 15f, 0.6f, 0.8f, 25);

            EnigmaVFXLibrary.AddPulsingLight(player.Center, Main.GameUpdateCount, 0.3f);
        }

        // =====================================================================
        //  SWING VFX — per-frame effects during melee swing
        // =====================================================================

        public static void SwingFrameVFX(Vector2 ownerCenter, Vector2 tipPos,
            Vector2 swordDirection, int comboStep, float progression)
        {
            if (Main.dedServ || progression < 0.08f || progression > 0.92f) return;

            float bladeLength = Vector2.Distance(ownerCenter, tipPos);

            // Dense void dust trail
            for (int i = 0; i < 2; i++)
            {
                Vector2 dustPos = ownerCenter + swordDirection * bladeLength * Main.rand.NextFloat(0.4f, 1f);
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.CursedTorch;
                Dust d = Dust.NewDustPerfect(dustPos, dustType,
                    -swordDirection * Main.rand.NextFloat(1f, 3f), 0,
                    Color.Lerp(EnigmaPalette.Purple, VoidFlame, Main.rand.NextFloat()), 1.4f);
                d.noGravity = true;
            }

            // Void shimmer particles
            if (Main.rand.NextBool(4))
            {
                Vector2 shimmerPos = ownerCenter + swordDirection * bladeLength * Main.rand.NextFloat(0.3f, 0.9f);
                Color shimmerColor = Color.Lerp(VoidDepth, EnigmaPalette.GreenFlame, Main.rand.NextFloat()) * 0.6f;
                var shimmer = new GenericGlowParticle(shimmerPos, -swordDirection * Main.rand.NextFloat(0.5f, 2f),
                    shimmerColor, 0.22f, 14, true);
                MagnumParticleHandler.SpawnParticle(shimmer);
            }

            // Enigma eye (1-in-4)
            if (Main.rand.NextBool(4))
            {
                Vector2 eyePos = ownerCenter + swordDirection * bladeLength * Main.rand.NextFloat(0.5f, 0.95f);
                CustomParticles.EnigmaEyeGaze(eyePos, EnigmaPalette.Purple * 0.4f, 0.25f);
            }

            // Music notes (1-in-6)
            if (Main.rand.NextBool(6))
            {
                Vector2 notePos = ownerCenter + swordDirection * bladeLength * Main.rand.NextFloat(0.6f, 1f);
                MagnumParticleHandler.SpawnParticle(new HueShiftingMusicNoteParticle(
                    notePos, -swordDirection * 1.5f,
                    hueMin: 0.38f, hueMax: 0.77f,
                    saturation: 0.85f, luminosity: 0.6f,
                    scale: 0.7f, lifetime: 22, hueSpeed: 0.025f));
            }

            // Blade-tip bloom
            float bloomOpacity = MathHelper.Clamp((progression - 0.10f) / 0.15f, 0f, 1f)
                               * MathHelper.Clamp((0.90f - progression) / 0.15f, 0f, 1f);
            BloomRenderer.DrawBloomStackAdditive(tipPos, EnigmaPalette.DeepPurple, VoidFlame,
                scale: 0.35f + comboStep * 0.07f, opacity: bloomOpacity * 0.6f);

            Lighting.AddLight(tipPos, EnigmaPalette.GetPaletteColor(0.35f + comboStep * 0.12f).ToVector3() * 0.5f);
        }

        // =====================================================================
        //  ON-HIT IMPACT VFX
        // =====================================================================

        public static void OnHitImpactVFX(Vector2 hitPos, int comboStep, bool isCrit)
        {
            if (Main.dedServ) return;

            // Base impact
            EnigmaVFXLibrary.MeleeImpact(hitPos, comboStep);

            // Void dust burst
            for (int i = 0; i < 5 + comboStep * 2; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(5f, 5f);
                Dust d = Dust.NewDustPerfect(hitPos, DustID.PurpleTorch, dustVel, 0,
                    Color.Lerp(EnigmaPalette.Purple, VoidFlame, Main.rand.NextFloat()), 1.3f);
                d.noGravity = true;
            }

            // Crit: void flash
            if (isCrit)
            {
                CustomParticles.GenericFlare(hitPos, ConvergenceFlash, 0.5f, 14);
                EnigmaVFXLibrary.SpawnWatchingEyes(hitPos, 2, 20f, 0.25f);
            }

            Lighting.AddLight(hitPos, EnigmaPalette.Purple.ToVector3() * 0.7f);
        }

        // =====================================================================
        //  VOID BEAM CONVERGENCE VFX — finisher tri-beam effects
        // =====================================================================

        /// <summary>
        /// Per-frame VFX for the convergence beam set projectile.
        /// Three beams slowly converging toward the cursor.
        /// </summary>
        public static void BeamConvergenceFrameVFX(Vector2 beamCenter, float convergenceProgress)
        {
            if (Main.dedServ) return;

            // Beam intersection glow
            float intensity = convergenceProgress * 0.8f;
            if (intensity > 0.1f)
            {
                BloomRenderer.DrawBloomStackAdditive(beamCenter, EnigmaPalette.DeepPurple, VoidFlame,
                    0.2f + intensity * 0.3f, intensity);
            }

            // Void swirl at convergence point
            if (Main.rand.NextBool(6) && convergenceProgress > 0.3f)
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = 20f * (1f - convergenceProgress);
                Vector2 particlePos = beamCenter + angle.ToRotationVector2() * dist;
                Vector2 vel = (beamCenter - particlePos).SafeNormalize(Vector2.Zero) * 2f;
                Color col = EnigmaPalette.GetVoidGradient(Main.rand.NextFloat()) * 0.5f;
                var voidParticle = new GenericGlowParticle(particlePos, vel, col, 0.18f, 15, true);
                MagnumParticleHandler.SpawnParticle(voidParticle);
            }

            // Eye watching at convergence
            if (Main.rand.NextBool(15) && convergenceProgress > 0.5f)
                CustomParticles.EnigmaEyeGaze(beamCenter + Main.rand.NextVector2Circular(10f, 10f),
                    EnigmaPalette.EyeGreen * 0.6f, 0.3f);
        }

        /// <summary>
        /// Resonance VFX when all 3 beams align — massive convergence explosion.
        /// </summary>
        public static void BeamResonanceVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            // Use shared finisher as base
            EnigmaVFXLibrary.FinisherSlam(pos, 1.0f);

            // Unique void convergence effects
            CustomParticles.GenericFlare(pos, ConvergenceFlash, 0.8f, 20);

            // Three directional flares (representing the three beams)
            for (int i = 0; i < 3; i++)
            {
                float angle = MathHelper.TwoPi * i / 3f;
                Vector2 flarePos = pos + angle.ToRotationVector2() * 25f;
                CustomParticles.GenericFlare(flarePos, VoidFlame, 0.4f, 14);
            }

            Lighting.AddLight(pos, EnigmaPalette.GreenFlame.ToVector3() * 1.0f);
        }

        // =====================================================================
        //  ON-SHOOT VFX
        // =====================================================================

        public static void OnShootVFX(Vector2 playerCenter)
        {
            CustomParticles.GenericFlare(playerCenter, EnigmaPalette.GreenFlame, 0.5f, 12);
            CustomParticles.HaloRing(playerCenter, EnigmaPalette.Purple, 0.35f, 10);
            EnigmaVFXLibrary.SpawnMusicNotes(playerCenter, 2, 25f);
        }

        /// <summary>
        /// Finisher combo VFX when void beam set is spawned.
        /// </summary>
        public static void FinisherBeamSpawnVFX(Vector2 playerCenter)
        {
            CustomParticles.GenericFlare(playerCenter, EnigmaPalette.GreenFlame, 0.7f, 16);
            CustomParticles.HaloRing(playerCenter, EnigmaPalette.Purple, 0.4f, 14);
            EnigmaVFXLibrary.SpawnGlyphCircle(playerCenter, 4, 35f, 0.07f);
            EnigmaVFXLibrary.SpawnWatchingEyes(playerCenter, 3, 30f, 0.3f);
            EnigmaVFXLibrary.SpawnMusicNotes(playerCenter, 3, 20f, 0.8f, 1.0f, 28);
        }

        // =====================================================================
        //  TRAIL FUNCTIONS
        // =====================================================================

        public static Color VoidTrailColor(float completionRatio)
        {
            Color c = Color.Lerp(EnigmaPalette.DeepPurple, VoidFlame, completionRatio);
            float fade = 1f - MathF.Pow(completionRatio, 1.3f);
            return (c * fade) with { A = 0 };
        }

        public static float VoidTrailWidth(float completionRatio)
            => EnigmaVFXLibrary.VoidTrailWidth(completionRatio, 14f);

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
