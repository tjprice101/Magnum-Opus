using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Nachtmusik;
using static MagnumOpus.Common.Systems.Particles.Particle;

namespace MagnumOpus.Content.Nachtmusik.Weapons.TwilightSeverance.Utilities
{
    /// <summary>
    /// VFX helper for Twilight Severance — the ultra-fast dimensional katana.
    /// Night Void → Deep Indigo → Cosmic Blue → Starlight Silver → Stellar White palette.
    /// Prioritizes speed, precision, and razor-thin visual language.
    /// Every cut severs the boundary between light and dark.
    /// </summary>
    public static class TwilightSeveranceVFX
    {
        // Twilight Severance palette
        private static readonly Color NightVoid = new Color(10, 10, 30);
        private static readonly Color DeepIndigo = new Color(40, 30, 100);
        private static readonly Color CosmicBlue = new Color(60, 80, 180);
        private static readonly Color StarlightSilver = new Color(180, 200, 230);
        private static readonly Color MoonPearl = new Color(220, 225, 245);
        private static readonly Color StellarWhite = new Color(240, 245, 255);

        // =====================================================================
        //  HoldItemVFX — Ambient speed aura, cosmic streaks at high charge
        // =====================================================================
        public static void HoldItemVFX(Player player, float chargeProgress)
        {
            float time = (float)Main.timeForVisualEffects * 0.05f;

            // === SPEED AURA STREAKS === Thin razor lines radiating outward at high charge
            if (chargeProgress > 0.3f && Main.rand.NextBool(3))
            {
                float streakAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 streakDir = streakAngle.ToRotationVector2();
                float startDist = 15f + Main.rand.NextFloat() * 8f;
                Vector2 streakPos = player.Center + streakDir * startDist;
                Vector2 streakVel = streakDir * (2f + chargeProgress * 2.5f);

                Color streakColor = Color.Lerp(DeepIndigo, StarlightSilver, chargeProgress * 0.6f);
                Dust d = Dust.NewDustPerfect(streakPos, DustID.PurpleTorch, streakVel, 0,
                    streakColor, 0.4f + chargeProgress * 0.3f);
                d.noGravity = true;
                d.fadeIn = 0.5f;
            }

            // === DIMENSIONAL SHIMMER === Oscillating dusk/silver dust at moderate charge
            if (chargeProgress > 0.2f && Main.rand.NextBool(4))
            {
                float oscillation = (float)Math.Sin(time * 1.5f) * 0.5f + 0.5f;
                Vector2 offset = new Vector2(
                    Main.rand.NextFloat(-16f, 16f),
                    Main.rand.NextFloat(-10f, 10f));
                Vector2 vel = new Vector2(
                    (float)Math.Cos(time * 0.8f) * 0.4f,
                    -0.3f) * chargeProgress;

                int dustType = oscillation > 0.5f ? DustID.PurpleTorch : DustID.SilverFlame;
                Dust shimmer = Dust.NewDustPerfect(player.Center + offset, dustType,
                    vel, 0, default, 0.45f + chargeProgress * 0.2f);
                shimmer.noGravity = true;
                shimmer.fadeIn = 0.6f;
            }

            // === COSMIC STREAK LINES === Quick thin cosmic streaks at high charge
            if (chargeProgress > 0.6f && Main.rand.NextBool(5))
            {
                float lineAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 lineDir = lineAngle.ToRotationVector2();
                for (int i = 0; i < 3; i++)
                {
                    Vector2 linePos = player.Center + lineDir * (12f + i * 5f);
                    Color lineColor = Color.Lerp(CosmicBlue, StarlightSilver, (float)i / 3f);
                    Dust line = Dust.NewDustPerfect(linePos, DustID.PurpleTorch,
                        lineDir * (1f + chargeProgress), 0, lineColor, 0.35f);
                    line.noGravity = true;
                    line.fadeIn = 0.4f;
                }
            }

            // === TWINKLING ACCENT === Occasional star twinkle at very high charge
            if (chargeProgress > 0.7f && Main.rand.NextBool(10))
            {
                NachtmusikVFXLibrary.SpawnTwinklingStars(player.Center, 1, 20f + chargeProgress * 8f);
            }

            NachtmusikVFXLibrary.AddNachtmusikLight(player.Center, 0.18f + chargeProgress * 0.25f);
        }

        // =====================================================================
        //  SwingImpactVFX — Sharp katana impact: thin slash marks, precise dust
        // =====================================================================
        public static void SwingImpactVFX(Vector2 hitPos, int comboStep)
        {
            float intensity = 1f + comboStep * 0.2f;

            NachtmusikVFXLibrary.MeleeImpact(hitPos, comboStep);

            // Thin star slash marks — cross pattern (katana cut lines)
            int slashDustCount = 6 + comboStep * 2;
            for (int i = 0; i < slashDustCount; i++)
            {
                // Two perpendicular slash lines
                float angle = (i % 2 == 0)
                    ? MathHelper.PiOver4 + Main.rand.NextFloat(-0.15f, 0.15f)
                    : -MathHelper.PiOver4 + Main.rand.NextFloat(-0.15f, 0.15f);
                Vector2 vel = angle.ToRotationVector2() * (3.5f + Main.rand.NextFloat() * 2f) * intensity;

                int dustType = (i % 3 == 0) ? DustID.SilverFlame : DustID.PurpleTorch;
                Dust d = Dust.NewDustPerfect(hitPos, dustType, vel, 0,
                    Color.Lerp(DeepIndigo, StarlightSilver, Main.rand.NextFloat()), 0.85f * intensity);
                d.noGravity = true;
                d.fadeIn = 0.9f;
            }

            // Music note accent — the katana's voice
            NachtmusikVFXLibrary.SpawnMusicNotes(hitPos, 1 + comboStep, 15f, 0.5f, 0.7f, 22);

            // Precise thin halo
            if (comboStep >= 1)
            {
                NachtmusikVFXLibrary.SpawnGradientHaloRings(hitPos, 2 + comboStep, 0.2f * intensity);
            }

            NachtmusikVFXLibrary.DrawBloom(hitPos, 0.25f * intensity, 0.6f);
            NachtmusikVFXLibrary.AddPaletteLighting(hitPos, 0.4f, 0.5f * intensity);
        }

        // =====================================================================
        //  DimensionSeverVFX — Grand cross-slash signature explosion
        // =====================================================================
        public static void DimensionSeverVFX(Vector2 pos)
        {
            float intensity = 1.5f;

            // === MASSIVE COSMIC EXPLOSION === The dimensional tear opens
            NachtmusikVFXLibrary.SpawnStarburstCascade(pos, 5, intensity, 1f);
            NachtmusikVFXLibrary.SpawnShatteredStarlight(pos, 10, 7f * intensity, 0.8f, false);

            // === X-PATTERN DUST === Cross-slash dimensional signature
            for (int arm = 0; arm < 4; arm++)
            {
                float armAngle = MathHelper.PiOver4 + arm * MathHelper.PiOver2;
                Vector2 armDir = armAngle.ToRotationVector2();

                for (int i = 0; i < 8; i++)
                {
                    float dist = 5f + i * 8f;
                    Vector2 dustPos = pos + armDir * dist;
                    float dp = (float)i / 8f;
                    Color dustColor = Color.Lerp(DeepIndigo, StellarWhite, dp);
                    Vector2 vel = armDir * (4f + Main.rand.NextFloat() * 3f) * intensity;
                    Dust d = Dust.NewDustPerfect(dustPos, DustID.PurpleTorch, vel, 0, dustColor, 1.3f * intensity);
                    d.noGravity = true;
                    d.fadeIn = 1.4f;
                }
            }

            // === CONSTELLATION CIRCLE === Orbiting starfield
            NachtmusikVFXLibrary.SpawnConstellationCircle(pos, 55f, 10, Main.rand.NextFloat() * MathHelper.TwoPi);

            // === ORBITING GLYPHS === Dimensional sigils
            NachtmusikVFXLibrary.SpawnOrbitingGlyphs(pos, 6, 45f, Main.rand.NextFloat() * MathHelper.TwoPi);

            // === SHATTERED STARLIGHT === Dimensional fragments everywhere
            NachtmusikVFXLibrary.SpawnRadialDustBurst(pos, 20, 8f * intensity);

            // === MULTI-RING HALO CASCADE === Dimension Sever signature
            for (int i = 0; i < 5; i++)
            {
                float p = i / 5f;
                Color ringColor = Color.Lerp(DeepIndigo, StellarWhite, p);
                CustomParticles.HaloRing(pos, ringColor, 0.4f + i * 0.15f, 16 + i * 3);
            }

            // === STELLAR WHITE FLASH === The moment of severance
            CustomParticles.GenericFlare(pos, StellarWhite, 1.0f, 22);
            CustomParticles.GenericFlare(pos, CosmicBlue, 0.7f, 18);

            // === MUSIC NOTES CASCADE === The blade's grand aria
            NachtmusikVFXLibrary.SpawnMusicNotes(pos, 8, 40f, 0.7f, 1.1f, 35);

            // === TWINKLING STAR SCATTER ===
            NachtmusikVFXLibrary.SpawnTwinklingStars(pos, 6, 45f);

            NachtmusikVFXLibrary.DrawComboBloom(pos, 2, 0.6f * intensity, 1f);
            NachtmusikVFXLibrary.AddPaletteLighting(pos, 0.1f, 1.4f * intensity);
        }

        // =====================================================================
        //  PerpendicularSlashVFX — Small accent when perpendicular blades fire
        // =====================================================================
        public static void PerpendicularSlashVFX(Vector2 pos)
        {
            // Small flare accent
            CustomParticles.GenericFlare(pos, CosmicBlue, 0.4f, 12);
            CustomParticles.HaloRing(pos, DeepIndigo, 0.25f, 10);

            // Quick perpendicular dust lines
            for (int side = -1; side <= 1; side += 2)
            {
                float perpAngle = MathHelper.PiOver2 * side;
                Vector2 perpDir = perpAngle.ToRotationVector2();
                for (int i = 0; i < 4; i++)
                {
                    Vector2 dustPos = pos + perpDir * (6f + i * 5f);
                    Color c = Color.Lerp(CosmicBlue, StarlightSilver, (float)i / 4f);
                    Dust d = Dust.NewDustPerfect(dustPos, DustID.PurpleTorch,
                        perpDir * 1.5f, 0, c, 0.65f);
                    d.noGravity = true;
                    d.fadeIn = 0.6f;
                }
            }

            // Silver sparkle accent
            NachtmusikVFXLibrary.SpawnTwinklingStars(pos, 2, 12f);
            NachtmusikVFXLibrary.SpawnMusicNotes(pos, 1, 12f, 0.4f, 0.6f, 20);

            NachtmusikVFXLibrary.DrawBloom(pos, 0.25f, 0.5f);
            NachtmusikVFXLibrary.AddPaletteLighting(pos, 0.4f, 0.4f);
        }
    }
}
