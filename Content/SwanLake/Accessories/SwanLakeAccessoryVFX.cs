using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.SwanLake.Accessories
{
    /// <summary>
    /// Shared VFX helper for ALL Swan Lake accessories.
    /// Provides ambient feather drift, prismatic shimmer aura,
    /// dual-polarity glow, and accessory-specific visual effects.
    /// Each accessory can call shared methods + its own unique overrides.
    /// </summary>
    public static class SwanLakeAccessoryVFX
    {
        // =====================================================================
        //  AMBIENT FEATHER DRIFT — All accessories
        // =====================================================================

        /// <summary>
        /// Shared ambient feather drift for equipped Swan Lake accessories.
        /// Spawns gentle floating feathers near the player.
        /// Frequency is low — call every frame; internal random gates.
        /// </summary>
        public static void AmbientFeatherDrift(Player player, float frequency = 0.04f)
        {
            if (Main.dedServ) return;
            if (!Main.rand.NextBool((int)(1f / frequency))) return;

            Vector2 center = player.MountedCenter;
            Vector2 driftPos = center + Main.rand.NextVector2Circular(25f, 20f);
            Color featherCol = Main.rand.NextBool() ? SwanLakePalette.FeatherWhite : SwanLakePalette.FeatherBlack;
            try { CustomParticles.SwanFeatherDrift(driftPos, featherCol, Main.rand.NextFloat(0.12f, 0.2f)); } catch { }
        }

        // =====================================================================
        //  PRISMATIC SHIMMER AURA — All accessories
        // =====================================================================

        /// <summary>
        /// Shared prismatic shimmer aura for equipped Swan Lake accessories.
        /// Subtle rainbow-tinted glow particles orbiting near the player.
        /// </summary>
        public static void PrismaticShimmerAura(Player player, float frequency = 0.08f)
        {
            if (Main.dedServ) return;
            if (!Main.rand.NextBool((int)(1f / frequency))) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;
            Color rainbow = SwanLakePalette.GetRainbow(time * 0.01f + Main.rand.NextFloat());

            Vector2 shimmerPos = center + Main.rand.NextVector2Circular(18f, 18f);
            var glow = new GenericGlowParticle(shimmerPos,
                new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-0.5f, -0.1f)),
                rainbow * 0.35f, 0.12f, 20, true);
            MagnumParticleHandler.SpawnParticle(glow);
        }

        // =====================================================================
        //  DUAL-POLARITY AMBIENT GLOW — All accessories
        // =====================================================================

        /// <summary>
        /// Shared dual-polarity ambient glow for equipped Swan Lake accessories.
        /// Alternating black and white dust motes near the player.
        /// </summary>
        public static void DualPolarityAmbient(Player player, float frequency = 0.06f)
        {
            if (Main.dedServ) return;
            if (!Main.rand.NextBool((int)(1f / frequency))) return;

            Vector2 center = player.MountedCenter;
            Vector2 motePos = center + Main.rand.NextVector2Circular(22f, 22f);
            bool isWhite = Main.rand.NextBool();
            int dustType = isWhite ? DustID.WhiteTorch : DustID.Shadowflame;
            Color col = isWhite ? SwanLakePalette.PureWhite : SwanLakePalette.ObsidianBlack;
            Dust d = Dust.NewDustPerfect(motePos, dustType, Vector2.Zero, isWhite ? 0 : 100, col, 0.4f);
            d.noGravity = true;
        }

        // =====================================================================
        //  AMBIENT MUSIC NOTES — All accessories
        // =====================================================================

        /// <summary>
        /// Shared ambient music notes for equipped Swan Lake accessories.
        /// </summary>
        public static void AmbientMusicNotes(Player player, float frequency = 0.02f)
        {
            if (Main.dedServ) return;
            if (!Main.rand.NextBool((int)(1f / frequency))) return;

            SwanLakeVFXLibrary.SpawnMusicNotes(player.MountedCenter, 1, 18f, 0.7f, 0.85f, 28);
        }

        // =====================================================================
        //  AMBIENT LIGHT — All accessories
        // =====================================================================

        /// <summary>
        /// Shared ambient light pulse for equipped Swan Lake accessories.
        /// </summary>
        public static void AmbientLight(Player player, float intensity = 0.2f)
        {
            if (Main.dedServ) return;
            float time = (float)Main.timeForVisualEffects;
            SwanLakeVFXLibrary.AddPulsingLight(player.MountedCenter, time, intensity);
        }

        // =====================================================================
        //  FULL AMBIENT — Convenience method combining all ambient effects
        // =====================================================================

        /// <summary>
        /// Full ambient VFX for a standard Swan Lake accessory.
        /// Combines feather drift + prismatic shimmer + dual-polarity + music notes + light.
        /// Call once per frame from the accessory's UpdateEquip/UpdateVanity.
        /// </summary>
        public static void FullAmbientVFX(Player player)
        {
            AmbientFeatherDrift(player);
            PrismaticShimmerAura(player);
            DualPolarityAmbient(player);
            AmbientMusicNotes(player);
            AmbientLight(player);
        }

        // =====================================================================
        //  ACCESSORY-SPECIFIC: BLACK WINGS OF THE MONOCHROMATIC DAWN
        // =====================================================================

        /// <summary>
        /// VFX for Black Wings: enhanced dual-polarity wing particles,
        /// trailing feathers during flight, monochromatic flash on dash.
        /// </summary>
        public static void BlackWingsAmbientVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;

            // Enhanced wing particles (black/white alternating near wings)
            if (Main.rand.NextBool(5))
            {
                float side = Main.rand.NextBool() ? -1f : 1f;
                Vector2 wingPos = center + new Vector2(side * Main.rand.NextFloat(15f, 30f), Main.rand.NextFloat(-10f, 5f));
                bool isBlack = side < 0; // Left wing = black, right = white
                int dustType = isBlack ? DustID.Shadowflame : DustID.WhiteTorch;
                Color col = isBlack ? SwanLakePalette.ObsidianBlack : SwanLakePalette.PureWhite;
                Dust d = Dust.NewDustPerfect(wingPos, dustType, new Vector2(0f, -0.3f), isBlack ? 100 : 0, col, 0.6f);
                d.noGravity = true;
            }

            // Trailing feathers during flight
            if (player.velocity.LengthSquared() > 4f && Main.rand.NextBool(4))
            {
                Color featherCol = Main.rand.NextBool() ? SwanLakePalette.FeatherWhite : SwanLakePalette.FeatherBlack;
                try { CustomParticles.SwanFeatherDrift(center + new Vector2(0f, 5f), featherCol, 0.2f); } catch { }
            }

            FullAmbientVFX(player);
        }

        // =====================================================================
        //  ACCESSORY-SPECIFIC: CROWN OF THE SWAN
        // =====================================================================

        /// <summary>
        /// VFX for Crown of the Swan: prismatic shimmer halo above head,
        /// gentle floating sparkles, regal light.
        /// </summary>
        public static void CrownAmbientVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Prismatic shimmer halo above head
            if (Main.rand.NextBool(6))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float radius = 12f;
                Vector2 haloPos = center + new Vector2(0f, -30f) + angle.ToRotationVector2() * radius;
                Color rainbow = SwanLakePalette.GetRainbow(angle / MathHelper.TwoPi);
                var glow = new GenericGlowParticle(haloPos,
                    new Vector2(0f, -0.2f),
                    rainbow * 0.4f, 0.1f, 16, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            FullAmbientVFX(player);
        }

        // =====================================================================
        //  ACCESSORY-SPECIFIC: DUAL FEATHER QUIVER
        // =====================================================================

        /// <summary>
        /// VFX for Dual Feather Quiver: enhanced feather drift at higher frequency,
        /// dual-polarity feather trail while moving.
        /// </summary>
        public static void QuiverAmbientVFX(Player player)
        {
            if (Main.dedServ) return;

            // Extra feather drift (doubled frequency)
            AmbientFeatherDrift(player, 0.08f);

            // Dual-polarity feather trail while moving
            if (player.velocity.LengthSquared() > 2f && Main.rand.NextBool(6))
            {
                SwanLakeVFXLibrary.SpawnFeatherDuality(
                    player.MountedCenter + new Vector2(-player.direction * 8f, 0f), 1, 0.15f);
            }

            FullAmbientVFX(player);
        }

        // =====================================================================
        //  ACCESSORY-SPECIFIC: PENDANT OF THE TWO SWANS
        // =====================================================================

        /// <summary>
        /// VFX for Pendant of the Two Swans: orbiting dual glow (one black, one white),
        /// periodic prismatic convergence flare.
        /// </summary>
        public static void PendantAmbientVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Orbiting dual glow (one black, one white)
            if (Main.rand.NextBool(6))
            {
                float angle = time * 0.04f;
                float radius = 20f;

                // White orbit
                Vector2 whitePos = center + angle.ToRotationVector2() * radius;
                Dust w = Dust.NewDustPerfect(whitePos, DustID.WhiteTorch, Vector2.Zero, 0, SwanLakePalette.PureWhite * 0.4f, 0.4f);
                w.noGravity = true;

                // Black orbit (opposite side)
                Vector2 blackPos = center + (angle + MathHelper.Pi).ToRotationVector2() * radius;
                Dust b = Dust.NewDustPerfect(blackPos, DustID.Shadowflame, Vector2.Zero, 100, SwanLakePalette.ObsidianBlack, 0.4f);
                b.noGravity = true;
            }

            FullAmbientVFX(player);
        }

        // =====================================================================
        //  ON-HIT PROC VFX — Shared accessory trigger effects
        // =====================================================================

        /// <summary>
        /// Generic on-hit proc VFX for Swan Lake accessories that trigger on hit.
        /// Prismatic spark burst, monochrome dust, and music notes.
        /// </summary>
        public static void OnHitProcVFX(Vector2 hitPos, float intensity = 1f)
        {
            if (Main.dedServ) return;

            SwanLakeVFXLibrary.SpawnPrismaticSparkles(hitPos, (int)(4 * intensity), 15f);
            SwanLakeVFXLibrary.SpawnRadialDustBurst(hitPos, (int)(6 * intensity), 4f);
            SwanLakeVFXLibrary.SpawnMusicNotes(hitPos, 2, 15f, 0.7f, 0.9f, 22);
            SwanLakeVFXLibrary.SpawnFeatherDrift(hitPos, 2, 12f);
            try { CustomParticles.HaloRing(hitPos, SwanLakePalette.SwanSilver, 0.2f, 10); } catch { }

            Lighting.AddLight(hitPos, SwanLakePalette.PureWhite.ToVector3() * 0.5f * intensity);
        }
    }
}
