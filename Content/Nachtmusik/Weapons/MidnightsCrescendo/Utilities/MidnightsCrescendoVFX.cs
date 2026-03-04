using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Nachtmusik;
using static MagnumOpus.Common.Systems.Particles.Particle;

namespace MagnumOpus.Content.Nachtmusik.Weapons.MidnightsCrescendo.Utilities
{
    /// <summary>
    /// Static VFX helper for Midnight's Crescendo — the rapid crescendo-building sword.
    /// Night Void → Deep Indigo → Cosmic Blue → Starlight Silver → Moon Pearl → Stellar White palette.
    /// All effects scale dramatically with crescendo stacks, from subtle shimmer to blinding storm.
    /// </summary>
    public static class MidnightsCrescendoVFX
    {
        #region Palette

        private static readonly Color NightVoid = new Color(10, 10, 30);
        private static readonly Color DeepIndigo = new Color(40, 30, 100);
        private static readonly Color CosmicBlue = new Color(60, 80, 180);
        private static readonly Color StarlightSilver = new Color(180, 200, 230);
        private static readonly Color MoonPearl = new Color(220, 225, 245);
        private static readonly Color StellarWhite = new Color(240, 245, 255);

        private static readonly Color[] Palette = new Color[]
        {
            NightVoid, DeepIndigo, CosmicBlue, StarlightSilver, MoonPearl, StellarWhite
        };

        /// <summary>Lerp through the 6-colour crescendo palette. t=0 → NightVoid, t=1 → StellarWhite.</summary>
        private static Color GetPaletteColor(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float scaled = t * (Palette.Length - 1);
            int idx = (int)scaled;
            int next = Math.Min(idx + 1, Palette.Length - 1);
            return Color.Lerp(Palette[idx], Palette[next], scaled - idx);
        }

        #endregion

        // =====================================================================
        //  HoldItemVFX — Ambient VFX that scales dramatically with stacks
        // =====================================================================

        /// <summary>
        /// Ambient hold VFX for Midnight's Crescendo.
        /// At 0 stacks: subtle indigo shimmer.
        /// At 5+: orbiting cosmic motes appear.
        /// At 8+: orbiting cosmic motes intensify, music notes drift.
        /// At 15: constant stellar sparkle storm.
        /// </summary>
        /// <param name="player">The player holding the weapon.</param>
        /// <param name="stackProgress">0-1 ratio of stacks/maxStacks.</param>
        /// <param name="stacks">Raw stack count (0-15).</param>
        public static void HoldItemVFX(Player player, float stackProgress, int stacks)
        {
            float time = (float)Main.timeForVisualEffects * 0.04f;

            // === SUBTLE INDIGO SHIMMER (always active) ===
            if (Main.rand.NextBool(3))
            {
                Vector2 shimmerPos = player.Center + Main.rand.NextVector2Circular(16f, 20f);
                Color shimmerColor = Color.Lerp(DeepIndigo, CosmicBlue, stackProgress * 0.5f);
                Dust shimmer = Dust.NewDustPerfect(shimmerPos, DustID.PurpleTorch,
                    Main.rand.NextVector2Circular(0.5f, 0.5f), 0, shimmerColor, 0.5f + stackProgress * 0.3f);
                shimmer.noGravity = true;
                shimmer.fadeIn = 0.8f;
            }

            // === ORBITING COSMIC MOTES (5+ stacks) ===
            if (stacks >= 5)
            {
                float orbitSpeed = 1.2f + stackProgress * 0.8f;
                float orbitRadius = 20f + stackProgress * 15f;
                int moteCount = 1 + stacks / 5; // 1 at 5, 2 at 10, 3 at 15

                for (int i = 0; i < moteCount; i++)
                {
                    float angle = time * orbitSpeed + MathHelper.TwoPi * i / moteCount;
                    Vector2 orbitPos = player.Center + new Vector2(
                        (float)Math.Cos(angle) * orbitRadius,
                        (float)Math.Sin(angle) * orbitRadius * 0.7f);
                    Vector2 tangent = new Vector2(-(float)Math.Sin(angle), (float)Math.Cos(angle)) * 0.5f;

                    Color moteColor = Color.Lerp(DeepIndigo, StarlightSilver, stackProgress);
                    Dust mote = Dust.NewDustPerfect(orbitPos, DustID.PurpleTorch,
                        tangent, 0, moteColor, 0.6f + stackProgress * 0.4f);
                    mote.noGravity = true;
                    mote.fadeIn = 1.0f;
                }
            }

            // === ORBITING COSMIC MOTES INTENSIFY + MUSIC NOTES (8+ stacks) ===
            if (stacks >= 8)
            {
                // Gravity-well inward motes
                if (Main.rand.NextBool(3))
                {
                    float spawnAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                    float spawnDist = 28f + Main.rand.NextFloat() * 12f;
                    Vector2 spawnPos = player.Center + spawnAngle.ToRotationVector2() * spawnDist;
                    Vector2 toCenter = (player.Center - spawnPos).SafeNormalize(Vector2.Zero);
                    Vector2 tangent = new Vector2(-toCenter.Y, toCenter.X) * 0.3f;
                    Vector2 vel = toCenter * (0.8f + stackProgress * 0.6f) + tangent;

                    Color inwardColor = Color.Lerp(CosmicBlue, StarlightSilver, stackProgress * 0.7f);
                    Dust d = Dust.NewDustPerfect(spawnPos, DustID.PurpleTorch, vel, 0, inwardColor, 0.5f + stackProgress * 0.3f);
                    d.noGravity = true;
                }

                // Drifting music notes
                if (Main.rand.NextBool(8))
                {
                    Vector2 notePos = player.Center + Main.rand.NextVector2Circular(20f, 20f);
                    Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), -1f);
                    MagnumParticleHandler.SpawnParticle(new HueShiftingMusicNoteParticle(
                        notePos, noteVel,
                        hueMin: 0.58f, hueMax: 0.72f,
                        saturation: 0.65f, luminosity: 0.6f + stackProgress * 0.15f,
                        scale: 0.6f + stackProgress * 0.15f, lifetime: 30, hueSpeed: 0.02f));
                }

                // Twinkling star accents
                if (Main.rand.NextBool(6))
                    NachtmusikVFXLibrary.SpawnTwinklingStars(player.Center, 1, 25f + stackProgress * 10f);
            }

            // === STELLAR SPARKLE STORM (15 stacks — MAX CRESCENDO) ===
            if (stacks >= 15)
            {
                // Constant stellar dust storm
                for (int i = 0; i < 2; i++)
                {
                    Vector2 stormPos = player.Center + Main.rand.NextVector2Circular(25f, 25f);
                    Vector2 stormVel = Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0, -0.5f);
                    Color stormColor = Color.Lerp(MoonPearl, StellarWhite, Main.rand.NextFloat());
                    Dust storm = Dust.NewDustPerfect(stormPos, DustID.GoldFlame, stormVel, 0, stormColor, 0.9f);
                    storm.noGravity = true;
                }

                // Persistent twinkling stars
                if (Main.rand.NextBool(3))
                    NachtmusikVFXLibrary.SpawnTwinklingStars(player.Center, 1, 30f);

                // Constellation circle hint
                if (Main.rand.NextBool(30))
                    NachtmusikVFXLibrary.SpawnConstellationCircle(player.Center, 40f, 6, time);

                // Extra music notes cascading outward
                if (Main.rand.NextBool(5))
                {
                    Vector2 cascadeVel = Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0, -1.2f);
                    MagnumParticleHandler.SpawnParticle(new HueShiftingMusicNoteParticle(
                        player.Center + Main.rand.NextVector2Circular(15f, 15f), cascadeVel,
                        hueMin: 0.55f, hueMax: 0.75f,
                        saturation: 0.8f, luminosity: 0.75f,
                        scale: 0.8f, lifetime: 28, hueSpeed: 0.03f));
                }
            }

            // Dynamic lighting — scales with stacks
            NachtmusikVFXLibrary.AddPaletteLighting(player.Center, stackProgress * 0.6f, 0.2f + stackProgress * 0.4f);
        }

        // =====================================================================
        //  WaveReleaseVFX — Burst VFX when crescendo wave is released
        // =====================================================================

        /// <summary>
        /// Cosmic burst VFX when the crescendo wave arc is released at 8+ stacks.
        /// Cosmic dust explosion, music notes, halo rings.
        /// </summary>
        /// <param name="pos">Release position (player center).</param>
        /// <param name="intensity">Stack intensity (0-1).</param>
        public static void WaveReleaseVFX(Vector2 pos, float intensity)
        {
            // === COSMIC DUST EXPLOSION ===
            int dustCount = 12 + (int)(intensity * 10);
            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                float dp = (float)i / dustCount;
                Color dc = Color.Lerp(CosmicBlue, Color.Lerp(StarlightSilver, StellarWhite, intensity), dp);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f) * (0.8f + intensity * 0.5f);
                Dust d = Dust.NewDustPerfect(pos, DustID.PurpleTorch, vel, 0, dc, 1.2f + intensity * 0.5f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // === HALO RINGS — expanding cosmic ripple ===
            int ringCount = 3 + (int)(intensity * 2);
            for (int ring = 0; ring < ringCount; ring++)
            {
                float p = (float)ring / ringCount;
                Color ringColor = Color.Lerp(DeepIndigo, StarlightSilver, p + intensity * 0.2f);
                CustomParticles.HaloRing(pos, ringColor, 0.3f + ring * 0.1f + intensity * 0.05f, 14 + ring * 2);
            }

            // === MUSIC NOTES — crescendo release signature ===
            NachtmusikVFXLibrary.SpawnMusicNotes(pos, 4 + (int)(intensity * 3), 30f, 0.7f, 1.0f, 28);

            // === STARLIGHT ACCENTS ===
            NachtmusikVFXLibrary.SpawnTwinklingStars(pos, 3 + (int)(intensity * 2), 25f);
            NachtmusikVFXLibrary.SpawnStarBurst(pos, 4 + (int)(intensity * 4), 0.5f + intensity * 0.3f);

            // === GENERIC FLARE — release flash ===
            CustomParticles.GenericFlare(pos, Color.Lerp(CosmicBlue, StellarWhite, intensity), 0.5f + intensity * 0.3f, 16);

            // === SHATTERED STARLIGHT at high intensity ===
            if (intensity > 0.6f)
                NachtmusikVFXLibrary.SpawnShatteredStarlight(pos, 4 + (int)((intensity - 0.6f) * 8), 5f, 0.5f + intensity * 0.2f, false);

            // === BLOOM ===
            NachtmusikVFXLibrary.DrawBloom(pos, 0.5f + intensity * 0.3f, 0.9f);
            NachtmusikVFXLibrary.AddPaletteLighting(pos, intensity * 0.5f, 0.8f + intensity * 0.4f);
        }

        // =====================================================================
        //  SwingImpactVFX — Hit impact that scales with combo and stacks
        // =====================================================================

        /// <summary>
        /// On-hit VFX for Midnight's Crescendo swing impacts.
        /// Scales with both combo step and crescendo stack progress.
        /// </summary>
        /// <param name="hitPos">Position of the hit target.</param>
        /// <param name="comboStep">Current combo step (0-2).</param>
        /// <param name="stackProgress">Stack ratio (0-1).</param>
        public static void SwingImpactVFX(Vector2 hitPos, int comboStep, float stackProgress)
        {
            float intensity = 1f + comboStep * 0.2f + stackProgress * 0.5f;

            // Use NachtmusikVFXLibrary's standardized melee impact
            NachtmusikVFXLibrary.MeleeImpact(hitPos, comboStep);

            // === RADIAL COSMIC DUST BURST — scales with stacks ===
            int dustCount = 6 + comboStep * 3 + (int)(stackProgress * 6);
            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                float dp = (float)i / dustCount;
                Color dc = Color.Lerp(DeepIndigo, Color.Lerp(StarlightSilver, StellarWhite, stackProgress), dp);
                Vector2 vel = angle.ToRotationVector2() * (3f + Main.rand.NextFloat() * 3f) * intensity;
                Dust d = Dust.NewDustPerfect(hitPos, DustID.PurpleTorch, vel, 0, dc, 1.1f * intensity);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // === CONSTELLATION SPARK RING at higher combos ===
            if (comboStep >= 1 || stackProgress > 0.5f)
            {
                float circleRadius = 25f + stackProgress * 15f;
                int starCount = 4 + comboStep * 2 + (int)(stackProgress * 3);
                NachtmusikVFXLibrary.SpawnConstellationCircle(hitPos, circleRadius * intensity, starCount,
                    Main.rand.NextFloat() * MathHelper.TwoPi);
            }

            // === MUSIC NOTES from impact ===
            NachtmusikVFXLibrary.SpawnMusicNotes(hitPos, 2 + comboStep + (int)(stackProgress * 2), 18f, 0.6f, 0.9f, 25);

            // === BLOOM — crescendo-scaled ===
            float bloomScale = 0.3f + comboStep * 0.05f + stackProgress * 0.15f;
            NachtmusikVFXLibrary.DrawBloom(hitPos, bloomScale * intensity, 0.7f + stackProgress * 0.2f);
            NachtmusikVFXLibrary.AddPaletteLighting(hitPos, stackProgress * 0.4f, 0.6f * intensity);

            // === SCREEN SHAKE at high stacks + combo 2 ===
            if (comboStep >= 2 && stackProgress > 0.5f)
                BloomRenderer.DrawBloomStackAdditive(hitPos, CosmicBlue, StellarWhite, 0.3f + stackProgress * 0.15f, 0.6f);
        }
    }
}
