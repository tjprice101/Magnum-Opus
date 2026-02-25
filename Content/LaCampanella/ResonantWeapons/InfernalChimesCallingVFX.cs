using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons
{
    /// <summary>
    /// VFX helper for the Infernal Chimes Calling summon weapon (item).
    /// Handles summoning ritual VFX, spawn burst, and ambient summoner aura.
    /// </summary>
    public static class InfernalChimesCallingVFX
    {
        // =====================================================================
        //  HOLD ITEM VFX
        // =====================================================================

        /// <summary>
        /// Per-frame held-item VFX: subtle fire aura and bell resonance.
        /// </summary>
        public static void HoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Subtle fire aura
            if (Main.rand.NextBool(6))
            {
                Vector2 pos = center + Main.rand.NextVector2Circular(18f, 18f);
                Color col = Color.Lerp(LaCampanellaPalette.DeepEmber, LaCampanellaPalette.InfernalOrange,
                    Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, new Vector2(0, -0.3f), 0, col, 0.6f);
                d.noGravity = true;
            }

            // Bell resonance shimmer
            if (Main.rand.NextBool(10))
            {
                Color shimmer = LaCampanellaPalette.GetBellShimmer(time);
                Dust d = Dust.NewDustPerfect(center + Main.rand.NextVector2Circular(12f, 12f),
                    DustID.Enchanted_Gold, Vector2.Zero, 0, shimmer, 0.4f);
                d.noGravity = true;
            }

            float pulse = 0.35f + MathF.Sin(time * 0.04f) * 0.1f;
            Lighting.AddLight(center, LaCampanellaPalette.InfernalOrange.ToVector3() * pulse);
        }

        // =====================================================================
        //  SUMMONING RITUAL VFX
        // =====================================================================

        /// <summary>
        /// Grand summoning ritual VFX — fires and bells calling forth the minion.
        /// Call when the summon weapon is used.
        /// </summary>
        public static void SummoningRitualVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            // Central fire flash cascade
            LaCampanellaVFXLibrary.DrawBloom(pos, 0.8f);

            // Fire circle (8-point glyph flares)
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 glyphPos = pos + angle.ToRotationVector2() * 35f;
                Color col = LaCampanellaPalette.PaletteLerp(LaCampanellaPalette.InfernalChimesBeam,
                    (float)i / 8f);
                Dust d = Dust.NewDustPerfect(glyphPos, DustID.Torch,
                    angle.ToRotationVector2() * 2f, 0, col, 1.5f);
                d.noGravity = true;
            }

            // Bell chime symbols (6 bell shapes around summon point)
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f + MathHelper.PiOver4;
                Vector2 bellPos = pos + angle.ToRotationVector2() * 25f;
                Color col = LaCampanellaPalette.BellGold;
                Dust d = Dust.NewDustPerfect(bellPos, DustID.GoldFlame,
                    (pos - bellPos).SafeNormalize(Vector2.Zero) * 1.5f, 0, col, 1.2f);
                d.noGravity = true;
            }

            // Bell chime cascade
            LaCampanellaVFXLibrary.SpawnBellChimeRings(pos, 3, 0.3f);

            // Heavy smoke burst
            LaCampanellaVFXLibrary.SpawnHeavySmoke(pos, 6, 0.8f, 3f, 50);

            // Music notes scatter
            LaCampanellaVFXLibrary.SpawnMusicNotes(pos, 6, 35f, 0.8f, 1.1f, 35);

            // Ember scatter
            LaCampanellaVFXLibrary.SpawnEmberScatter(pos, 10, 4f);

            MagnumScreenEffects.AddScreenShake(3f);
            Lighting.AddLight(pos, LaCampanellaPalette.WhiteHot.ToVector3() * 1.0f);
        }

        // =====================================================================
        //  MINION SPAWN BURST
        // =====================================================================

        /// <summary>
        /// Burst VFX at the exact minion spawn point.
        /// Fire eruption + bell chime + smoke cloud.
        /// </summary>
        public static void MinionSpawnBurstVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            // Fire eruption
            LaCampanellaVFXLibrary.DrawBloom(pos, 0.5f);
            LaCampanellaVFXLibrary.SpawnRadialDustBurst(pos, 10, 4f);

            // Bell chime
            CustomParticles.LaCampanellaBellChime(pos, 8);

            // Smoke cloud
            LaCampanellaVFXLibrary.SpawnHeavySmoke(pos, 4, 0.7f, 2f, 40);

            // Music notes
            LaCampanellaVFXLibrary.SpawnMusicNotes(pos, 3, 20f);

            Lighting.AddLight(pos, LaCampanellaPalette.InfernalOrange.ToVector3() * 0.8f);
        }

        // =====================================================================
        //  BEAM / PROJECTILE TRAIL
        // =====================================================================

        /// <summary>
        /// Per-frame chiming projectile trail VFX.
        /// For any projectiles spawned by the InfernalChimesCalling weapon.
        /// </summary>
        public static void ChimeProjectileTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            // Orange-gold fire trail
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = -velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 2f)
                    + Main.rand.NextVector2Circular(0.5f, 0.5f);
                Color col = Color.Lerp(LaCampanellaPalette.DeepEmber, LaCampanellaPalette.ChimeShimmer,
                    Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.1f);
                d.noGravity = true;
            }

            // Chime sparkle
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustPerfect(pos, DustID.Enchanted_Gold,
                    Main.rand.NextVector2Circular(1f, 1f), 0, LaCampanellaPalette.ChimeShimmer, 0.6f);
                d.noGravity = true;
            }

            Lighting.AddLight(pos, LaCampanellaPalette.BellGold.ToVector3() * 0.4f);
        }
    }
}
