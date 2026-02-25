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
    /// VFX helper for the Midnight's Crescendo melee weapon.
    /// Building musical intensity — each swing crescendos from deep blue
    /// to starlit brilliance. Rising sparkle cascades, ascending star trails,
    /// the crescendo builds to a blinding climax.
    /// </summary>
    public static class MidnightsCrescendoVFX
    {
        // =====================================================================
        //  HOLD ITEM VFX
        // =====================================================================

        public static void HoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Pulsing crescendo aura — intensity builds and falls
            if (Main.rand.NextBool(5))
            {
                float pulse = (float)Math.Sin(time * 0.06f) * 0.5f + 0.5f;
                Vector2 offset = Main.rand.NextVector2Circular(22f, 22f);
                Color auraColor = NachtmusikPalette.GetCelestialGradient(pulse);
                var glow = new GenericGlowParticle(center + offset, new Vector2(0, -0.3f),
                    auraColor * 0.35f, 0.16f, 18, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Ascending star motes — stars rise upward (crescendo motif)
            if (Main.rand.NextBool(10))
            {
                Vector2 motePos = center + new Vector2(Main.rand.NextFloat(-18f, 18f), 12f);
                var mote = new GenericGlowParticle(motePos, new Vector2(0, -1.2f),
                    NachtmusikPalette.StarWhite * 0.5f, 0.12f, 22, true);
                MagnumParticleHandler.SpawnParticle(mote);
            }

            NachtmusikVFXLibrary.AddTwinklingLight(center, time, 0.25f);
        }

        // =====================================================================
        //  PREDRAW IN WORLD BLOOM
        // =====================================================================

        public static void PreDrawInWorldBloom(SpriteBatch sb, Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.05f) * 0.04f;
            NachtmusikPalette.DrawItemBloom(sb, tex, pos, origin, rotation, scale, pulse);
        }

        // =====================================================================
        //  SWING TRAIL VFX
        // =====================================================================

        /// <summary>
        /// Per-frame swing trail: ascending starlit dust, crescendo sparkles,
        /// intensity builds with combo step.
        /// </summary>
        public static void SwingTrailVFX(Vector2 tipPos, Vector2 swordDirection, int comboStep, int timer)
        {
            if (Main.dedServ) return;

            // Crescendo dust — brighter with each combo
            float comboIntensity = 1f + comboStep * 0.25f;
            Color dustColor = NachtmusikPalette.GetCelestialGradient(0.3f + comboStep * 0.15f);
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = -swordDirection * Main.rand.NextFloat(1f, 3f) + new Vector2(0, -0.5f);
                Dust d = Dust.NewDustPerfect(tipPos, DustID.BlueTorch, vel, 0,
                    dustColor, 1.3f * comboIntensity);
                d.noGravity = true;
            }

            // Ascending star sparkles (rise upward)
            if (Main.rand.NextBool(2))
            {
                Vector2 sparkVel = new Vector2(Main.rand.NextFloat(-1f, 1f), -2f - Main.rand.NextFloat(1f));
                var spark = new GlowSparkParticle(tipPos, sparkVel,
                    NachtmusikPalette.StarWhite * 0.7f, 0.2f * comboIntensity, 14);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Crescendo music notes (more frequent at higher combos)
            if (timer % (6 - comboStep) == 0)
                NachtmusikVFXLibrary.SpawnMusicNotes(tipPos, 1, 10f, 0.7f, 0.9f, 25);

            // Twinkling stars along arc
            if (timer % 7 == 0)
                NachtmusikVFXLibrary.SpawnTwinklingStars(tipPos, 1, 8f);

            Lighting.AddLight(tipPos, NachtmusikPalette.StarlitBlue.ToVector3() * (0.5f + comboStep * 0.15f));
        }

        // =====================================================================
        //  SWING IMPACT VFX
        // =====================================================================

        /// <summary>
        /// On-hit impact: crescendo builds with combo — bloom grows,
        /// star particles intensify, halo rings cascade upward.
        /// </summary>
        public static void SwingImpactVFX(Vector2 hitPos, int comboStep = 0)
        {
            if (Main.dedServ) return;

            float intensity = 1f + comboStep * 0.3f;

            // Growing bloom flash
            NachtmusikVFXLibrary.DrawBloom(hitPos, 0.4f * intensity);

            // Gradient halo cascade
            NachtmusikVFXLibrary.SpawnGradientHaloRings(hitPos, 3 + comboStep);

            // Crescendo dust burst — intensifies
            NachtmusikVFXLibrary.SpawnRadialDustBurst(hitPos, 8 + comboStep * 3, 4f + comboStep);

            // Ascending star sparks
            for (int i = 0; i < 4 + comboStep * 2; i++)
            {
                float angle = MathHelper.TwoPi * i / (4 + comboStep * 2) - MathHelper.PiOver2;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f) * intensity;
                vel.Y -= 2f; // Upward bias
                Color sparkColor = NachtmusikPalette.GetCelestialGradient((float)i / (4 + comboStep * 2));
                var spark = new GlowSparkParticle(hitPos, vel, sparkColor, 0.25f * intensity, 16);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Music notes — crescendo scatter
            NachtmusikVFXLibrary.SpawnMusicNotes(hitPos, 2 + comboStep, 20f);

            // Twinkling stars
            NachtmusikVFXLibrary.SpawnTwinklingStars(hitPos, 2 + comboStep, 15f);

            Lighting.AddLight(hitPos, NachtmusikPalette.StarWhite.ToVector3() * (0.6f + comboStep * 0.2f));
        }

        // =====================================================================
        //  FINISHER VFX
        // =====================================================================

        /// <summary>
        /// Finisher: crescendo climax — maximum intensity starlit explosion,
        /// ascending starburst cascade, culminating music note shower.
        /// </summary>
        public static void FinisherVFX(Vector2 pos, float intensity = 1f)
        {
            if (Main.dedServ) return;

            NachtmusikVFXLibrary.FinisherSlam(pos, intensity);

            // Unique crescendo climax: ascending starburst fountain
            for (int i = 0; i < 8; i++)
            {
                float spreadAngle = Main.rand.NextFloat(-0.5f, 0.5f);
                Vector2 vel = new Vector2(spreadAngle * 4f, -6f - Main.rand.NextFloat(4f)) * intensity;
                Color starColor = NachtmusikPalette.GetCelestialGradient((float)i / 8f);
                var star = new GlowSparkParticle(pos, vel, starColor, 0.4f * intensity, 25);
                MagnumParticleHandler.SpawnParticle(star);
            }

            // Ascending shattered starlight
            NachtmusikVFXLibrary.SpawnShatteredStarlight(pos, 10, 8f * intensity, intensity, false);
        }
    }
}
