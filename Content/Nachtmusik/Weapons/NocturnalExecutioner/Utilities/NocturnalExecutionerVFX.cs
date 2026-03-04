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

namespace MagnumOpus.Content.Nachtmusik.Weapons.NocturnalExecutioner.Utilities
{
    /// <summary>
    /// VFX helper for Nocturnal Executioner — the heavy midnight cosmic greatsword.
    /// Night Void → Deep Indigo → Cosmic Blue → Starlight Silver → Stellar White palette.
    /// Uses NK theme-specific VFX assets where available.
    /// </summary>
    public static class NocturnalExecutionerVFX
    {
        // Nocturnal Executioner palette
        private static readonly Color NightVoid = new Color(10, 10, 30);
        private static readonly Color DeepIndigo = new Color(40, 30, 100);
        private static readonly Color CosmicBlue = new Color(60, 80, 180);
        private static readonly Color StarlightSilver = new Color(180, 200, 230);
        private static readonly Color MoonPearl = new Color(220, 225, 245);
        private static readonly Color StellarWhite = new Color(240, 245, 255);

        // =====================================================================
        //  HoldItemVFX — Ambient cosmic authority aura, scales with charge
        // =====================================================================
        public static void HoldItemVFX(Player player, float chargeProgress)
        {
            float time = (float)Main.timeForVisualEffects * 0.04f;

            // === ORBITING VOID GLYPHS === Indigo dust orbiting at fixed radius
            if (Main.rand.NextBool(2))
            {
                float orbitAngle = time * 1.5f + Main.rand.NextFloat() * 0.3f;
                float orbitRadius = 22f + chargeProgress * 12f + (float)Math.Sin(time * 2f) * 5f;
                Vector2 orbitPos = player.Center + new Vector2(
                    (float)Math.Cos(orbitAngle) * orbitRadius,
                    (float)Math.Sin(orbitAngle) * orbitRadius * 0.7f);
                Vector2 tangent = new Vector2(-(float)Math.Sin(orbitAngle), (float)Math.Cos(orbitAngle)) * 0.6f;

                Color glyphColor = Color.Lerp(DeepIndigo, CosmicBlue, chargeProgress);
                Dust glyph = Dust.NewDustPerfect(orbitPos, DustID.PurpleTorch, tangent, 0, glyphColor, 0.7f + chargeProgress * 0.3f);
                glyph.noGravity = true;
                glyph.fadeIn = 1.1f;
            }

            // === GRAVITY-WELL DUST === Motes spiraling inward — drawn to the blade's authority
            if (chargeProgress > 0.2f && Main.rand.NextBool(3))
            {
                float spawnAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float spawnDist = 30f + Main.rand.NextFloat() * 15f + chargeProgress * 10f;
                Vector2 spawnPos = player.Center + spawnAngle.ToRotationVector2() * spawnDist;
                Vector2 toCenter = (player.Center - spawnPos).SafeNormalize(Vector2.Zero);
                Vector2 tangent2 = new Vector2(-toCenter.Y, toCenter.X) * 0.4f;
                Vector2 vel = toCenter * (1f + chargeProgress) + tangent2;

                Color inwardColor = Color.Lerp(DeepIndigo, StarlightSilver, chargeProgress * 0.5f);
                Dust d = Dust.NewDustPerfect(spawnPos, DustID.PurpleTorch, vel, 0, inwardColor, 0.5f + chargeProgress * 0.3f);
                d.noGravity = true;
                d.fadeIn = 0.7f;
            }

            // === STELLAR SHIMMER === Occasional starlight twinkle at high charge
            if (chargeProgress > 0.5f && Main.rand.NextBool(8))
            {
                NachtmusikVFXLibrary.SpawnTwinklingStars(player.Center, 1, 25f + chargeProgress * 10f);
            }

            // === VOID CRACK FLASH === Thin dust line — the execution decree signature
            if (chargeProgress > 0.7f && Main.rand.NextBool(20))
            {
                float crackAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 crackDir = crackAngle.ToRotationVector2();
                for (int i = 0; i < 5; i++)
                {
                    Vector2 crackPos = player.Center + crackDir * (8f + i * 7f);
                    Color crackColor = Color.Lerp(CosmicBlue, StarlightSilver, (float)i / 5f);
                    Dust crack = Dust.NewDustPerfect(crackPos, DustID.PurpleTorch,
                        crackDir * 0.3f, 0, crackColor, 0.45f);
                    crack.noGravity = true;
                    crack.fadeIn = 0.5f;
                }
            }

            NachtmusikVFXLibrary.AddNachtmusikLight(player.Center, 0.3f + chargeProgress * 0.3f);
        }

        // =====================================================================
        //  Phase 0 Accent — Shadow Cleave tip VFX
        // =====================================================================
        public static void Phase0AccentVFX(Vector2 tipPos)
        {
            CustomParticles.GenericFlare(tipPos, CosmicBlue, 0.5f, 14);
            CustomParticles.HaloRing(tipPos, DeepIndigo, 0.3f, 12);
            NachtmusikVFXLibrary.SpawnMusicNotes(tipPos, 2, 15f, 0.6f, 0.9f, 25);
            NachtmusikVFXLibrary.SpawnTwinklingStars(tipPos, 2, 12f);
        }

        // =====================================================================
        //  Phase 1 Divide — V-pattern blade VFX
        // =====================================================================
        public static void Phase1DivideVFX(Vector2 tipPos)
        {
            CustomParticles.GenericFlare(tipPos, StellarWhite, 0.7f, 18);
            CustomParticles.GenericFlare(tipPos, CosmicBlue, 0.5f, 16);
            CustomParticles.HaloRing(tipPos, DeepIndigo, 0.4f, 14);
            NachtmusikVFXLibrary.SpawnMusicNotes(tipPos, 4, 25f, 0.7f, 0.9f, 25);
            NachtmusikVFXLibrary.SpawnShatteredStarlight(tipPos, 5, 4f, 0.5f, false);
            NachtmusikVFXLibrary.SpawnTwinklingStars(tipPos, 3, 18f);

            // Cosmic dust cloud burst
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                Color c = Color.Lerp(DeepIndigo, StarlightSilver, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(tipPos, DustID.PurpleTorch, vel, 0, c, 1.2f);
                d.noGravity = true;
            }
        }

        // =====================================================================
        //  Phase 2 Stellar Execution — Grand slam VFX with shockwave
        // =====================================================================
        public static void Phase2StellarExecutionVFX(Vector2 tipPos)
        {
            // Stellar burst cascade
            NachtmusikVFXLibrary.SpawnStarburstCascade(tipPos, 4, 1.2f, 1f);
            NachtmusikVFXLibrary.SpawnShatteredStarlight(tipPos, 8, 6f, 0.7f, false);

            // Multi-ring cosmic ripple — expanding ground impact
            for (int i = 0; i < 5; i++)
            {
                float p = i / 5f;
                Color ringColor = Color.Lerp(DeepIndigo, StarlightSilver, p);
                CustomParticles.HaloRing(tipPos, ringColor, 0.35f + i * 0.12f, 14 + i * 3);
            }

            // Dense cosmic fragment burst
            for (int i = 0; i < 15; i++)
            {
                float angle = MathHelper.TwoPi * i / 15f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Color c = Color.Lerp(DeepIndigo, StellarWhite, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(tipPos, DustID.PurpleTorch, vel, 0, c, 1.5f);
                d.noGravity = true;
            }

            NachtmusikVFXLibrary.SpawnConstellationCircle(tipPos, 50f, 8, Main.rand.NextFloat() * MathHelper.TwoPi);
            NachtmusikVFXLibrary.SpawnMusicNotes(tipPos, 6, 35f, 0.7f, 1.0f, 30);
            NachtmusikVFXLibrary.DrawComboBloom(tipPos, 2, 0.6f, 1f);
            NachtmusikVFXLibrary.AddPaletteLighting(tipPos, 0.1f, 1.2f);
        }

        // =====================================================================
        //  SwingImpactVFX — On-hit cosmic impact, scales with combo
        // =====================================================================
        public static void SwingImpactVFX(Vector2 hitPos, int comboStep)
        {
            float intensity = 1f + comboStep * 0.25f;

            NachtmusikVFXLibrary.MeleeImpact(hitPos, comboStep);

            // Radial cosmic dust burst
            int dustCount = 8 + comboStep * 4;
            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                float dp = (float)i / dustCount;
                Color dc = Color.Lerp(DeepIndigo, StarlightSilver, dp);
                Vector2 vel = angle.ToRotationVector2() * (4f + Main.rand.NextFloat() * 3f) * intensity;
                Dust d = Dust.NewDustPerfect(hitPos, DustID.PurpleTorch, vel, 0, dc, 1.2f * intensity);
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }

            // Constellation spark ring on later combos
            if (comboStep >= 1)
                NachtmusikVFXLibrary.SpawnConstellationCircle(hitPos, 30f * intensity, 6 + comboStep * 2, Main.rand.NextFloat() * MathHelper.TwoPi);

            // Authority glyphs on combo 2
            if (comboStep >= 2)
                NachtmusikVFXLibrary.SpawnOrbitingGlyphs(hitPos, 4, 35f, Main.rand.NextFloat() * MathHelper.TwoPi);

            NachtmusikVFXLibrary.SpawnMusicNotes(hitPos, 2 + comboStep, 20f, 0.6f, 1f, 30);
            NachtmusikVFXLibrary.DrawBloom(hitPos, 0.4f * intensity, 0.8f);
            NachtmusikVFXLibrary.AddPaletteLighting(hitPos, 0.3f, 0.8f * intensity);
        }

        // =====================================================================
        //  ExecutionFanVFX — Execution Charge release: 5-blade fan
        // =====================================================================
        public static void ExecutionFanVFX(Vector2 pos, bool isMaxCharge)
        {
            float intensity = isMaxCharge ? 1.5f : 1f;

            // Grand cosmic explosion
            NachtmusikVFXLibrary.SpawnStarburstCascade(pos, isMaxCharge ? 6 : 4, intensity, 1f);
            NachtmusikVFXLibrary.SpawnShatteredStarlight(pos, isMaxCharge ? 12 : 8, 7f * intensity, 0.8f, false);
            NachtmusikVFXLibrary.SpawnOrbitingGlyphs(pos, isMaxCharge ? 8 : 6, 50f * intensity, Main.rand.NextFloat() * MathHelper.TwoPi);
            NachtmusikVFXLibrary.SpawnConstellationCircle(pos, 60f * intensity, 10, Main.rand.NextFloat() * MathHelper.TwoPi);
            NachtmusikVFXLibrary.SpawnMusicNotes(pos, isMaxCharge ? 8 : 5, 40f, 0.7f, 1.2f, 40);
            NachtmusikVFXLibrary.SpawnRadialDustBurst(pos, (int)(25 * intensity), 8f * intensity);
            NachtmusikVFXLibrary.DrawComboBloom(pos, 2, 0.7f * intensity, 1f);
            NachtmusikVFXLibrary.AddPaletteLighting(pos, 0.1f, 1.3f * intensity);

            if (isMaxCharge)
            {
                // Extra max-charge VFX: stellar whiteness flash
                CustomParticles.GenericFlare(pos, StellarWhite, 1.0f, 24);
                for (int i = 0; i < 3; i++)
                    CustomParticles.HaloRing(pos, Color.Lerp(CosmicBlue, StellarWhite, i / 3f), 0.5f + i * 0.15f, 18 + i * 4);

                NachtmusikVFXLibrary.SpawnTwinklingStars(pos, 8, 50f);
            }
        }
    }
}
