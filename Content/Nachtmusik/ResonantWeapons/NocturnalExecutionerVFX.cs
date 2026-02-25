using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Nachtmusik.ResonantWeapons
{
    /// <summary>
    /// VFX helper for the Nocturnal Executioner melee weapon.
    /// Heavy midnight authority — commanding swings with violet flash,
    /// golden radiance accents, and cascading constellation impacts.
    /// The executioner's decree echoes through the night sky.
    /// </summary>
    public static class NocturnalExecutionerVFX
    {
        // =====================================================================
        //  HOLD ITEM VFX
        // =====================================================================

        public static void HoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Midnight authority aura — deep blue shimmer
            if (Main.rand.NextBool(6))
            {
                Vector2 offset = Main.rand.NextVector2Circular(25f, 25f);
                Color auraColor = NachtmusikPalette.GetCelestialGradient(Main.rand.NextFloat(0f, 0.4f));
                var glow = new GenericGlowParticle(center + offset, Main.rand.NextVector2Circular(0.5f, 0.5f),
                    auraColor * 0.4f, 0.18f, 20, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Orbiting violet authority glyphs
            if (Main.rand.NextBool(15))
            {
                float angle = time * 0.03f + Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 glyphPos = center + angle.ToRotationVector2() * 22f;
                try { CustomParticles.Glyph(glyphPos, NachtmusikPalette.Violet * 0.5f, 0.3f, -1); } catch { }
            }

            // Twinkling star motes
            if (Main.rand.NextBool(12))
            {
                Vector2 starPos = center + Main.rand.NextVector2Circular(20f, 20f);
                try { CustomParticles.GenericFlare(starPos, NachtmusikPalette.StarWhite * 0.4f, 0.15f, 10); } catch { }
            }

            NachtmusikVFXLibrary.AddNachtmusikLight(center, 0.25f);
        }

        // =====================================================================
        //  PREDRAW IN WORLD BLOOM
        // =====================================================================

        public static void PreDrawInWorldBloom(SpriteBatch sb, Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.04f) * 0.03f;
            NachtmusikPalette.DrawItemBloom(sb, tex, pos, origin, rotation, scale, pulse);
        }

        // =====================================================================
        //  SWING TRAIL VFX
        // =====================================================================

        /// <summary>
        /// Per-frame swing trail VFX: violet dust, starlit sparks, midnight bloom.
        /// The executioner's blade cleaves through the night with commanding authority.
        /// </summary>
        public static void SwingTrailVFX(Vector2 tipPos, Vector2 swordDirection, int comboStep, int timer)
        {
            if (Main.dedServ) return;

            // Violet authority dust
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = -swordDirection * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(0.5f, 0.5f);
                Dust d = Dust.NewDustPerfect(tipPos, DustID.PurpleTorch, vel, 0,
                    NachtmusikPalette.Violet, 1.4f + comboStep * 0.15f);
                d.noGravity = true;
            }

            // Golden radiance edge shimmer (1-in-2)
            NachtmusikVFXLibrary.SpawnRadianceShimmer(tipPos, -swordDirection);

            // Starlit sparkle accent (1-in-3)
            if (Main.rand.NextBool(3))
            {
                try { CustomParticles.GenericFlare(
                    tipPos + Main.rand.NextVector2Circular(6f, 6f),
                    NachtmusikPalette.StarWhite, 0.25f, 12); } catch { }
            }

            // Music notes (every 5 frames)
            if (timer % 5 == 0)
                NachtmusikVFXLibrary.SpawnMusicNotes(tipPos, 1, 10f, 0.7f, 0.9f, 25);

            // Orbiting glyph accent (every 8 frames)
            if (timer % 8 == 0)
            {
                try { CustomParticles.Glyph(tipPos + Main.rand.NextVector2Circular(8f, 8f),
                    NachtmusikPalette.RadianceGold * 0.6f, 0.3f, -1); } catch { }
            }

            Lighting.AddLight(tipPos, NachtmusikPalette.Violet.ToVector3() * (0.5f + comboStep * 0.12f));
        }

        // =====================================================================
        //  SWING IMPACT VFX
        // =====================================================================

        /// <summary>
        /// On-hit impact: violet authority flash, golden radiance burst,
        /// constellation spark ring, and starburst cascade.
        /// </summary>
        public static void SwingImpactVFX(Vector2 hitPos, int comboStep = 0)
        {
            if (Main.dedServ) return;

            // Commanding bloom flash
            NachtmusikVFXLibrary.DrawBloom(hitPos, 0.5f + comboStep * 0.12f);

            // Violet authority halo rings
            NachtmusikVFXLibrary.SpawnGradientHaloRings(hitPos, 3 + comboStep);

            // Purple dust burst
            for (int i = 0; i < 10 + comboStep * 3; i++)
            {
                float angle = MathHelper.TwoPi * i / (10 + comboStep * 3);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f + comboStep);
                Dust d = Dust.NewDustPerfect(hitPos, DustID.PurpleTorch, vel, 0,
                    NachtmusikPalette.Violet, 1.3f);
                d.noGravity = true;
            }

            // Golden radiance accent sparks
            NachtmusikVFXLibrary.SpawnStarBurst(hitPos, 4 + comboStep * 2, 0.3f);

            // Twinkling stars
            NachtmusikVFXLibrary.SpawnTwinklingStars(hitPos, 2 + comboStep, 18f);

            // Music notes
            NachtmusikVFXLibrary.SpawnMusicNotes(hitPos, 2 + comboStep, 20f);

            // Starburst cascade on combo 2+
            if (comboStep >= 2)
                NachtmusikVFXLibrary.SpawnStarburstCascade(hitPos, 3, 0.8f);

            Lighting.AddLight(hitPos, NachtmusikPalette.Violet.ToVector3() * (0.8f + comboStep * 0.15f));
        }

        // =====================================================================
        //  FINISHER VFX
        // =====================================================================

        /// <summary>
        /// Finisher slam: massive authority explosion with midnight decree VFX.
        /// Screen shake, constellation circle, golden radiance supernova.
        /// </summary>
        public static void FinisherVFX(Vector2 pos, float intensity = 1f)
        {
            if (Main.dedServ) return;

            NachtmusikVFXLibrary.FinisherSlam(pos, intensity);

            // Unique executioner authority: extra violet-gold flash cascade
            for (int i = 0; i < 5; i++)
            {
                float progress = (float)i / 5f;
                Color flashColor = Color.Lerp(NachtmusikPalette.Violet, NachtmusikPalette.RadianceGold, progress);
                try { CustomParticles.GenericFlare(pos + Main.rand.NextVector2Circular(20f * intensity, 20f * intensity),
                    flashColor, 0.5f * intensity, 18 + i * 3); } catch { }
            }

            // Golden glyph circle
            NachtmusikVFXLibrary.SpawnOrbitingGlyphs(pos, 6, 50f * intensity, Main.GameUpdateCount * 0.02f);
        }
    }
}
