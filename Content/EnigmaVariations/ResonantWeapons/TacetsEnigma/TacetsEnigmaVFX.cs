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
    /// VFX helper for the Tacet's Enigma ranged bullet weapon.
    /// Handles hold-item ambient, muzzle flash, bullet trails,
    /// paradox bolt VFX, and paradox stack explosions.
    /// Call from TacetsEnigma, TacetEnigmaShot, and TacetParadoxBolt.
    /// </summary>
    public static class TacetsEnigmaVFX
    {
        // =====================================================================
        //  HOLD ITEM VFX
        // =====================================================================

        /// <summary>
        /// Per-frame held-item VFX: subtle void aura near weapon muzzle.
        /// </summary>
        public static void HoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Subtle ambient muzzle aura
            if (Main.rand.NextBool(6))
            {
                Vector2 muzzleOffset = new Vector2(player.direction * 20f, -5f);
                Vector2 muzzlePos = center + muzzleOffset + Main.rand.NextVector2Circular(5f, 5f);
                Color col = EnigmaPalette.GetEnigmaGradient(Main.rand.NextFloat(0.3f, 0.6f));
                Dust d = Dust.NewDustPerfect(muzzlePos, DustID.PurpleTorch, Vector2.Zero, 0, col, 0.5f);
                d.noGravity = true;
            }

            // Enigma glow
            float pulse = 0.3f + MathF.Sin(time * 0.05f) * 0.1f;
            Lighting.AddLight(center, EnigmaPalette.Purple.ToVector3() * pulse);
        }

        // =====================================================================
        //  PREDRAW IN WORLD BLOOM
        // =====================================================================

        public static void PreDrawInWorldBloom(
            Microsoft.Xna.Framework.Graphics.SpriteBatch sb,
            Microsoft.Xna.Framework.Graphics.Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.04f) * 0.03f;
            EnigmaPalette.DrawItemBloom(sb, tex, pos, origin, rotation, scale, pulse);
        }

        // =====================================================================
        //  NORMAL SHOT MUZZLE FLASH
        // =====================================================================

        /// <summary>
        /// Muzzle flash for normal shot: sparkles, flares, halos, music notes.
        /// </summary>
        public static void NormalMuzzleFlashVFX(Vector2 muzzlePos, Vector2 direction)
        {
            if (Main.dedServ) return;

            // Sparkle burst at muzzle
            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkVel = direction.RotatedByRandom(0.3f) * Main.rand.NextFloat(2f, 4f);
                Color col = EnigmaPalette.GetEnigmaGradient((float)i / 4f);
                var spark = new GenericGlowParticle(muzzlePos, sparkVel, col * 0.7f, 0.25f, 12, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Flare
            CustomParticles.GenericFlare(muzzlePos, EnigmaPalette.GreenFlame, 0.4f, 10);

            // Halo
            CustomParticles.HaloRing(muzzlePos, EnigmaPalette.Purple, 0.2f, 8);

            // Occasional music note
            if (Main.rand.NextBool(3))
                EnigmaVFXLibrary.SpawnMusicNotes(muzzlePos, 1, 12f, 0.7f, 0.85f, 20);

            Lighting.AddLight(muzzlePos, EnigmaPalette.GreenFlame.ToVector3() * 0.5f);
        }

        // =====================================================================
        //  PARADOX SHOT MUZZLE FLASH
        // =====================================================================

        /// <summary>
        /// Enhanced muzzle flash for paradox bolt (every 5th shot).
        /// </summary>
        public static void ParadoxMuzzleFlashVFX(Vector2 muzzlePos, Vector2 direction)
        {
            if (Main.dedServ) return;

            // Enhanced flash
            CustomParticles.GenericFlare(muzzlePos, Color.White, 0.7f, 15);
            CustomParticles.GenericFlare(muzzlePos, EnigmaPalette.GreenFlame, 0.6f, 18);

            // Glyph burst at muzzle
            CustomParticles.GlyphBurst(muzzlePos, EnigmaPalette.Purple, 4, 3f);

            // Enhanced sparkle burst
            for (int i = 0; i < 6; i++)
            {
                Vector2 sparkVel = direction.RotatedByRandom(0.5f) * Main.rand.NextFloat(3f, 6f);
                Color col = EnigmaPalette.GetEnigmaGradient((float)i / 6f);
                var spark = new GenericGlowParticle(muzzlePos, sparkVel, col * 0.8f, 0.3f, 15, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Music notes
            EnigmaVFXLibrary.SpawnMusicNotes(muzzlePos, 3, 15f, 0.8f, 1.0f, 25);

            Lighting.AddLight(muzzlePos, EnigmaPalette.WhiteGreenFlash.ToVector3() * 0.8f);
        }

        // =====================================================================
        //  BULLET TRAIL VFX
        // =====================================================================

        /// <summary>
        /// Per-frame normal bullet trail: dust, sparkles, shimmer, music notes.
        /// </summary>
        public static void BulletTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            // Dense void dust
            EnigmaVFXLibrary.SpawnSwingDust(pos, -velocity.SafeNormalize(Vector2.Zero));
            EnigmaVFXLibrary.SpawnContrastSparkle(pos, -velocity.SafeNormalize(Vector2.Zero));

            // Enigma shimmer
            if (Main.rand.NextBool(3))
            {
                float hue = 0.28f + (Main.GameUpdateCount * 0.015f % 0.17f);
                Color shimmer = Main.hslToRgb(hue, 0.85f, 0.65f);
                var glow = new GenericGlowParticle(pos, -velocity * 0.08f,
                    shimmer * 0.5f, 0.2f, 14, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Music note trail
            if (Main.rand.NextBool(6))
                EnigmaVFXLibrary.SpawnMusicNotes(pos, 1, 8f, 0.7f, 0.85f, 20);

            EnigmaVFXLibrary.AddPulsingLight(pos, 0.4f);
        }

        // =====================================================================
        //  BULLET IMPACT VFX
        // =====================================================================

        /// <summary>
        /// Normal bullet on-hit impact.
        /// </summary>
        public static void BulletImpactVFX(Vector2 pos, Vector2 targetCenter)
        {
            if (Main.dedServ) return;

            // Standard impact
            CustomParticles.GenericFlare(pos, EnigmaPalette.GreenFlame, 0.5f, 15);
            CustomParticles.HaloRing(pos, EnigmaPalette.Purple, 0.25f, 12);

            // Glyph stack
            CustomParticles.GlyphStack(pos, EnigmaPalette.Purple, 2, 0.25f);

            // Music notes
            EnigmaVFXLibrary.SpawnMusicNotes(pos, 2, 12f, 0.7f, 0.9f, 22);

            Lighting.AddLight(pos, EnigmaPalette.GreenFlame.ToVector3() * 0.6f);
        }

        // =====================================================================
        //  PARADOX BOLT TRAIL VFX
        // =====================================================================

        /// <summary>
        /// Enhanced per-frame paradox bolt trail: denser particles,
        /// glyph trail, intensified effects.
        /// </summary>
        public static void ParadoxBoltTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            // Enhanced dense dust (3 per frame)
            for (int i = 0; i < 3; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(4f, 4f);
                Color col = EnigmaPalette.GetEnigmaGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos + offset, DustID.PurpleTorch,
                    -velocity * 0.15f + Main.rand.NextVector2Circular(0.5f, 0.5f), 0, col, 1.4f);
                d.noGravity = true;
            }

            // Green flame contrast
            EnigmaVFXLibrary.SpawnContrastSparkle(pos, -velocity.SafeNormalize(Vector2.Zero));

            // Glyph trail
            if (Main.rand.NextBool(4))
                CustomParticles.GlyphTrail(pos, velocity, EnigmaPalette.Purple, 0.3f);

            // Music notes
            if (Main.rand.NextBool(6))
                EnigmaVFXLibrary.SpawnMusicNotes(pos, 1, 8f, 0.8f, 0.95f, 22);

            EnigmaVFXLibrary.AddPulsingLight(pos, 0.55f);
        }

        // =====================================================================
        //  PARADOX BOLT IMPACT VFX
        // =====================================================================

        /// <summary>
        /// Paradox bolt on-hit impact with chain lightning visuals.
        /// </summary>
        public static void ParadoxBoltImpactVFX(Vector2 pos, Vector2 targetCenter)
        {
            if (Main.dedServ) return;

            // Enhanced impact
            CustomParticles.GenericFlare(pos, Color.White, 0.7f, 18);
            CustomParticles.GenericFlare(pos, EnigmaPalette.GreenFlame, 0.6f, 20);

            // Offset flares
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 offset = angle.ToRotationVector2() * 15f;
                Color col = EnigmaPalette.GetEnigmaGradient((float)i / 6f);
                CustomParticles.GenericFlare(pos + offset, col, 0.3f, 14);
            }

            // Halo + glyph burst
            CustomParticles.HaloRing(pos, EnigmaPalette.Purple, 0.35f, 15);
            CustomParticles.GlyphBurst(pos, EnigmaPalette.GreenFlame, 6, 4f);

            // Eye watching
            CustomParticles.EnigmaEyeImpact(pos, targetCenter, EnigmaPalette.GreenFlame, 0.45f);

            // Music notes
            EnigmaVFXLibrary.SpawnMusicNotes(pos, 3, 18f, 0.8f, 1.0f, 28);

            Lighting.AddLight(pos, EnigmaPalette.GreenFlame.ToVector3() * 0.8f);
        }

        // =====================================================================
        //  PARADOX STACK EXPLOSION VFX
        // =====================================================================

        /// <summary>
        /// Paradox explosion at 5 stacks: glyph burst, chain lightning visual,
        /// sparkle burst, screen effects.
        /// </summary>
        public static void ParadoxStackExplosionVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            // Central flash
            EnigmaVFXLibrary.DrawBloom(pos, 0.8f);
            CustomParticles.GenericFlare(pos, Color.White, 0.9f, 20);
            CustomParticles.GenericFlare(pos, EnigmaPalette.GreenFlame, 0.7f, 22);

            // Glyph burst
            CustomParticles.GlyphBurst(pos, EnigmaPalette.Purple, 8, 5f);

            // Sparkle burst
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                Color col = EnigmaPalette.GetEnigmaGradient((float)i / 8f);
                var spark = new GenericGlowParticle(pos, vel, col * 0.7f, 0.35f, 20, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Eye burst
            EnigmaVFXLibrary.SpawnEyeImpactBurst(pos, 4, 4f);

            // Expanding halos
            for (int i = 0; i < 3; i++)
            {
                Color col = EnigmaPalette.GetEnigmaGradient((float)i / 3f);
                CustomParticles.HaloRing(pos, col, 0.3f + i * 0.12f, 14 + i * 3);
            }

            // Music notes
            EnigmaVFXLibrary.SpawnMusicNotes(pos, 6, 30f, 0.8f, 1.1f, 35);

            // Radial dust burst
            EnigmaVFXLibrary.SpawnRadialDustBurst(pos, 16, 6f);

            Lighting.AddLight(pos, EnigmaPalette.WhiteGreenFlash.ToVector3() * 1.2f);
        }
    }
}
