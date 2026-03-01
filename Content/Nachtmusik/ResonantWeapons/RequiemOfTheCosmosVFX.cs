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
    /// Shader-driven VFX for Requiem of the Cosmos — the channeled cosmic beam weapon.
    /// Uses CosmicRequiem.fx for the nebula-swirl channeled beam.
    /// A cosmic funeral dirge that builds intensity as channeling continues.
    /// </summary>
    public static class RequiemOfTheCosmosVFX
    {
        // =====================================================================
        //  HoldItemVFX — Cosmic void ambient
        // =====================================================================
        public static void HoldItemVFX(Player player)
        {
            float time = (float)Main.timeForVisualEffects * 0.03f;

            // === INWARD-SPIRALING VOID === Dust spiraling inward like cosmic accretion
            if (Main.rand.NextBool(3))
            {
                float spawnAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float spawnDist = 32f + Main.rand.NextFloat() * 12f;
                Vector2 spawnPos = player.Center + spawnAngle.ToRotationVector2() * spawnDist;
                Vector2 toCenter = player.Center - spawnPos;
                if (toCenter != Vector2.Zero) toCenter.Normalize();
                // Strong tangential component — creates spiral
                Vector2 tangent = new Vector2(-toCenter.Y, toCenter.X);
                Vector2 vel = toCenter * 0.8f + tangent * 1.4f;

                Dust d = Dust.NewDustPerfect(spawnPos, DustID.PurpleTorch, vel, 0, default, 0.65f);
                d.noGravity = true;
                d.fadeIn = 0.85f;
            }

            // === COSMIC VOID DEEPENING === Dense dust concentrated near center
            if (Main.rand.NextBool(5))
            {
                Vector2 coreOffset = Main.rand.NextVector2Circular(10f, 10f);
                Dust core = Dust.NewDustPerfect(player.Center + coreOffset, DustID.PurpleTorch,
                    -coreOffset * 0.02f, 0, default, 0.7f);
                core.noGravity = true;
                core.fadeIn = 0.9f;
            }

            // === GOLDEN REQUIEM FLECKS === Rare golden motes in the void
            if (Main.rand.NextBool(8))
            {
                float goldAngle = time * 1.2f + Main.rand.NextFloat() * MathHelper.TwoPi;
                float goldRadius = 15f + Main.rand.NextFloat() * 10f;
                Vector2 goldPos = player.Center + new Vector2(
                    (float)Math.Cos(goldAngle) * goldRadius,
                    (float)Math.Sin(goldAngle) * goldRadius);
                Dust g = Dust.NewDustPerfect(goldPos, DustID.Enchanted_Gold,
                    Main.rand.NextVector2Circular(0.3f, 0.3f), 0, default, 0.4f);
                g.noGravity = true;
            }

            NachtmusikVFXLibrary.AddNachtmusikLight(player.Center, 0.3f);
        }

        // =====================================================================
        //  PreDrawInWorldBloom
        // =====================================================================
        public static void PreDrawInWorldBloom(SpriteBatch sb, Texture2D tex, Vector2 pos,
            Vector2 origin, float rotation, float scale)
        {
            NachtmusikVFXLibrary.DrawNachtmusikBloomStack(sb, pos,
                NachtmusikPalette.CosmicVoid, NachtmusikPalette.RadianceGold, scale * 0.35f, 0.5f);
        }

        // =====================================================================
        //  CastBurstVFX — Beam startup burst
        // =====================================================================
        public static void CastBurstVFX(Vector2 castPos)
        {
            // Cosmic ignition
            NachtmusikVFXLibrary.SpawnStarBurst(castPos, 8, 0.5f);

            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * (3f + Main.rand.NextFloat() * 3f);
                Dust d = Dust.NewDustPerfect(castPos, DustID.PurpleTorch, vel, 0, default, 1f);
                d.noGravity = true;
                d.fadeIn = 1.1f;
            }

            NachtmusikVFXLibrary.SpawnMusicNotes(castPos, 3, 20f, 0.5f, 0.9f, 28);
            NachtmusikVFXLibrary.DrawBloom(castPos, 0.4f, 0.8f);
            NachtmusikVFXLibrary.AddPaletteLighting(castPos, 0.2f, 0.7f);
        }

        // =====================================================================
        //  ProjectileTrailVFX — Beam particle trail
        // =====================================================================
        public static void ProjectileTrailVFX(Vector2 pos, Vector2 velocity)
        {
            // Cosmic void particles along beam
            Vector2 perp = new Vector2(-velocity.Y, velocity.X);
            if (perp != Vector2.Zero) perp.Normalize();

            Vector2 dustVel = perp * Main.rand.NextFloat(-2f, 2f) + velocity * 0.05f;
            Dust d = Dust.NewDustPerfect(pos + perp * Main.rand.NextFloat(-8f, 8f),
                DustID.PurpleTorch, dustVel, 0, default, 0.8f);
            d.noGravity = true;

            if (Main.rand.NextBool(3))
            {
                NachtmusikVFXLibrary.SpawnTwinklingStars(pos, 1, 10f);
            }

            NachtmusikVFXLibrary.AddPaletteLighting(pos, 0.2f, 0.5f);
        }

        // =====================================================================
        //  SmallHitVFX — Beam hitting enemy
        // =====================================================================
        public static void SmallHitVFX(Vector2 hitPos)
        {
            NachtmusikVFXLibrary.ProjectileImpact(hitPos, 1f);
            NachtmusikVFXLibrary.SpawnMusicNotes(hitPos, 2, 15f, 0.5f, 0.8f, 22);
            NachtmusikVFXLibrary.DrawBloom(hitPos, 0.35f, 0.7f);
        }

        // =====================================================================
        //  CosmicFinaleVFX — Channel release: cosmic requiem explosion
        // =====================================================================
        public static void CosmicFinaleVFX(Vector2 pos, float intensity = 1f)
        {
            // Grand cosmic finale — the requiem's final movement
            NachtmusikVFXLibrary.SpawnStarburstCascade(pos, 6, intensity, 1f);

            // Radial cosmic void explosion
            NachtmusikVFXLibrary.SpawnShatteredStarlight(pos, 12, 8f * intensity, 0.9f, false);

            // Golden requiem glyphs
            NachtmusikVFXLibrary.SpawnOrbitingGlyphs(pos, 5, 45f * intensity,
                Main.rand.NextFloat() * MathHelper.TwoPi);

            // Constellation funeral ring
            NachtmusikVFXLibrary.SpawnConstellationCircle(pos, 55f * intensity, 8,
                Main.rand.NextFloat() * MathHelper.TwoPi);

            // Grand music note cascade — the requiem's final chord
            NachtmusikVFXLibrary.SpawnMusicNotes(pos, 8, 40f, 0.7f, 1.2f, 45);

            // Massive dust burst
            NachtmusikVFXLibrary.SpawnRadialDustBurst(pos, 24, 9f * intensity);

            // Radiance halo rings
            NachtmusikVFXLibrary.SpawnRadianceHaloRings(pos, 6, 0.5f * intensity);

            NachtmusikVFXLibrary.DrawComboBloom(pos, 2, 0.6f * intensity, 1f);
            NachtmusikVFXLibrary.AddPaletteLighting(pos, 0.1f, 1.3f * intensity);
        }

        // =====================================================================
        //  ProjectileDeathVFX — Beam termination
        // =====================================================================
        public static void ProjectileDeathVFX(Vector2 pos)
        {
            NachtmusikVFXLibrary.SpawnShatteredStarlight(pos, 6, 5f, 0.6f, true);
            NachtmusikVFXLibrary.SpawnMusicNotes(pos, 3, 18f, 0.5f, 0.8f, 25);
            NachtmusikVFXLibrary.SpawnRadialDustBurst(pos, 8, 4f);
        }
    }
}
