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
    /// VFX helper for the Twilight Severance melee weapon.
    /// The razor boundary between day and night — dusk violet cutting arcs,
    /// nebula pink accents, twilight shimmer dissolve, and clean silver edges.
    /// Every cut severs light from darkness.
    /// </summary>
    public static class TwilightSeveranceVFX
    {
        // =====================================================================
        //  HOLD ITEM VFX
        // =====================================================================

        public static void HoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Twilight shimmer — oscillating between dusk and dawn
            if (Main.rand.NextBool(6))
            {
                float shift = (float)Math.Sin(time * 0.05f) * 0.5f + 0.5f;
                Color shimmerColor = Color.Lerp(NachtmusikPalette.DuskViolet, NachtmusikPalette.NebulaPink, shift);
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                var glow = new GenericGlowParticle(center + offset, Main.rand.NextVector2Circular(0.5f, 0.5f),
                    shimmerColor * 0.4f, 0.15f, 18, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Silver edge motes
            if (Main.rand.NextBool(12))
            {
                Vector2 edgeOffset = new Vector2(player.direction * 20f + Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-15f, 15f));
                try { CustomParticles.GenericFlare(center + edgeOffset, NachtmusikPalette.MoonlitSilver * 0.4f, 0.12f, 10); } catch { }
            }

            NachtmusikVFXLibrary.AddNachtmusikLight(center, 0.2f);
        }

        // =====================================================================
        //  PREDRAW IN WORLD BLOOM
        // =====================================================================

        public static void PreDrawInWorldBloom(SpriteBatch sb, Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.045f) * 0.03f;
            NachtmusikPalette.DrawItemBloom(sb, tex, pos, origin, rotation, scale, pulse);
        }

        // =====================================================================
        //  SWING TRAIL VFX
        // =====================================================================

        /// <summary>
        /// Per-frame swing trail: dusk violet cuts, nebula pink accents,
        /// clean silver edge shimmer. Precise and sharp.
        /// </summary>
        public static void SwingTrailVFX(Vector2 tipPos, Vector2 swordDirection, int comboStep, int timer)
        {
            if (Main.dedServ) return;

            // Dusk violet cutting dust
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = -swordDirection * Main.rand.NextFloat(1.5f, 3.5f) + Main.rand.NextVector2Circular(0.4f, 0.4f);
                Dust d = Dust.NewDustPerfect(tipPos, DustID.PurpleTorch, vel, 0,
                    NachtmusikPalette.DuskViolet, 1.3f);
                d.noGravity = true;
            }

            // Clean silver edge sparkle (1-in-2)
            if (Main.rand.NextBool(2))
            {
                Vector2 sparkVel = -swordDirection * Main.rand.NextFloat(1f, 2f);
                var spark = new GlowSparkParticle(tipPos, sparkVel,
                    NachtmusikPalette.MoonlitSilver, 0.2f, 12);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Nebula pink accent (1-in-4)
            if (Main.rand.NextBool(4))
            {
                try { CustomParticles.GenericFlare(
                    tipPos + Main.rand.NextVector2Circular(5f, 5f),
                    NachtmusikPalette.NebulaPink * 0.6f, 0.22f, 14); } catch { }
            }

            // Music notes
            if (timer % 5 == 0)
                NachtmusikVFXLibrary.SpawnMusicNotes(tipPos, 1, 10f, 0.7f, 0.9f, 25);

            // Twilight twinkling
            if (timer % 9 == 0)
                NachtmusikVFXLibrary.SpawnTwinklingStars(tipPos, 1, 8f);

            Lighting.AddLight(tipPos, NachtmusikPalette.DuskViolet.ToVector3() * (0.4f + comboStep * 0.1f));
        }

        // =====================================================================
        //  SWING IMPACT VFX
        // =====================================================================

        /// <summary>
        /// On-hit impact: twilight severance flash, dusk-dawn split burst,
        /// nebula sparkles, clean silver ring.
        /// </summary>
        public static void SwingImpactVFX(Vector2 hitPos, int comboStep = 0)
        {
            if (Main.dedServ) return;

            // Twilight bloom
            NachtmusikVFXLibrary.DrawBloom(hitPos, 0.45f + comboStep * 0.1f);

            // Dusk-violet halo rings
            NachtmusikVFXLibrary.SpawnGradientHaloRings(hitPos, 3 + comboStep);

            // Split dust burst — half dusk, half starlit
            int dustCount = 8 + comboStep * 3;
            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                bool isDusk = i % 2 == 0;
                Color col = isDusk ? NachtmusikPalette.DuskViolet : NachtmusikPalette.MoonlitSilver;
                int dustType = isDusk ? DustID.PurpleTorch : DustID.BlueTorch;
                Dust d = Dust.NewDustPerfect(hitPos, dustType, vel, 0, col, 1.3f);
                d.noGravity = true;
            }

            // Nebula pink sparkle accents
            for (int i = 0; i < 3 + comboStep; i++)
            {
                try { CustomParticles.GenericFlare(
                    hitPos + Main.rand.NextVector2Circular(12f, 12f),
                    NachtmusikPalette.NebulaPink, 0.3f, 16); } catch { }
            }

            // Clean silver ring
            try { CustomParticles.HaloRing(hitPos, NachtmusikPalette.MoonlitSilver, 0.35f, 14); } catch { }

            // Music notes
            NachtmusikVFXLibrary.SpawnMusicNotes(hitPos, 2 + comboStep, 20f);

            // Twinkling
            NachtmusikVFXLibrary.SpawnTwinklingStars(hitPos, 2 + comboStep, 15f);

            Lighting.AddLight(hitPos, NachtmusikPalette.MoonlitSilver.ToVector3() * (0.7f + comboStep * 0.15f));
        }

        // =====================================================================
        //  FINISHER VFX
        // =====================================================================

        /// <summary>
        /// Finisher: twilight severance — reality splits between light and dark,
        /// dusk-dawn polarity explosion, nebula cascade, silver edge ring.
        /// </summary>
        public static void FinisherVFX(Vector2 pos, float intensity = 1f)
        {
            if (Main.dedServ) return;

            NachtmusikVFXLibrary.FinisherSlam(pos, intensity);

            // Unique twilight split: alternating dusk/silver spark waves
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 vel = angle.ToRotationVector2() * 6f * intensity;
                bool isDusk = i % 2 == 0;
                Color splitColor = isDusk ? NachtmusikPalette.DuskViolet : NachtmusikPalette.MoonlitSilver;
                var spark = new GlowSparkParticle(pos, vel, splitColor, 0.35f * intensity, 22);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Nebula pink cascade
            for (int i = 0; i < 6; i++)
            {
                Vector2 nebOffset = Main.rand.NextVector2Circular(30f * intensity, 30f * intensity);
                try { CustomParticles.GenericFlare(pos + nebOffset,
                    NachtmusikPalette.NebulaPink, 0.4f * intensity, 20 + i * 2); } catch { }
            }
        }
    }
}
