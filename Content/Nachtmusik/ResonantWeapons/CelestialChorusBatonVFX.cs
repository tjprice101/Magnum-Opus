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
    /// Shader-driven VFX for Celestial Chorus Baton — the musical summoner staff.
    /// Uses ChorusSummonAura.fx for orbiting music note summoning circles.
    /// Choral, harmonic, conducting — the baton conducts a chorus of celestial guardians.
    /// </summary>
    public static class CelestialChorusBatonVFX
    {
        // =====================================================================
        //  HoldItemVFX — Conductor's harmonic presence
        // =====================================================================
        public static void HoldItemVFX(Player player, int minionCount)
        {
            // Music note motes orbit the player — count scales with active minions
            int noteFreq = Math.Max(1, 6 - minionCount);
            if (Main.rand.NextBool(noteFreq))
            {
                float angle = (float)Main.timeForVisualEffects * 0.04f + Main.rand.NextFloat() * MathHelper.TwoPi;
                float radius = 20f + minionCount * 4f;
                Vector2 orbPos = player.Center + new Vector2(
                    (float)Math.Cos(angle) * radius,
                    (float)Math.Sin(angle) * radius * 0.5f);

                NachtmusikVFXLibrary.SpawnMusicNotes(orbPos, 1, 6f, 0.3f, 0.6f, 16);
            }

            // Soft harmonic haze intensifies with more minions
            if (Main.rand.NextBool(5))
            {
                float haze = 0.3f + minionCount * 0.05f;
                Vector2 offset = Main.rand.NextVector2Circular(24f, 24f);
                Dust d = Dust.NewDustPerfect(player.Center + offset, DustID.BlueTorch,
                    Main.rand.NextVector2Circular(0.3f, 0.3f), 0, default, haze);
                d.noGravity = true;
                d.fadeIn = 0.7f;
            }

            NachtmusikVFXLibrary.AddNachtmusikLight(player.Center, 0.15f + minionCount * 0.03f);
        }

        // =====================================================================
        //  PreDrawInWorldBloom
        // =====================================================================
        public static void PreDrawInWorldBloom(SpriteBatch sb, Texture2D tex, Vector2 pos,
            Vector2 origin, float rotation, float scale)
        {
            NachtmusikVFXLibrary.DrawNachtmusikBloomStack(sb, pos,
                NachtmusikPalette.SerenadeGlow, NachtmusikPalette.StarlitBlue, scale * 0.2f, 0.3f);
        }

        // =====================================================================
        //  SummonVFX — Choral summoning circle
        // =====================================================================
        public static void SummonVFX(Vector2 spawnPos)
        {
            // Ring of music notes around spawn point — the chorus calls the guardian
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 ringPos = spawnPos + angle.ToRotationVector2() * 32f;
                NachtmusikVFXLibrary.SpawnMusicNotes(ringPos, 1, 8f, 0.5f, 0.8f, 22);
            }

            // Central flash
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Dust d = Dust.NewDustPerfect(spawnPos, DustID.BlueTorch, vel, 0, default, 0.9f);
                d.noGravity = true;
                d.fadeIn = 1f;
            }

            // Gold accent sparks radiating outward
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * (2f + Main.rand.NextFloat() * 2f);
                Dust d = Dust.NewDustPerfect(spawnPos, DustID.GoldFlame, vel, 0, default, 0.6f);
                d.noGravity = true;
            }

            NachtmusikVFXLibrary.DrawBloom(spawnPos, 0.5f, 0.8f);
            NachtmusikVFXLibrary.AddPaletteLighting(spawnPos, 0.6f, 0.6f);
        }

        // =====================================================================
        //  MinionAmbientVFX — Guardian's harmonic aura
        // =====================================================================
        public static void MinionAmbientVFX(Vector2 pos, float visibility)
        {
            if (Main.rand.NextBool(4))
            {
                // Gentle revolving mote
                Vector2 offset = Main.rand.NextVector2Circular(16f, 16f);
                Dust d = Dust.NewDustPerfect(pos + offset, DustID.BlueTorch,
                    Main.rand.NextVector2Circular(0.2f, 0.2f), 0, default, 0.35f * visibility);
                d.noGravity = true;
                d.fadeIn = 0.6f;
            }

            if (Main.rand.NextBool(10))
            {
                NachtmusikVFXLibrary.SpawnMusicNotes(pos, 1, 12f, 0.2f * visibility, 0.5f, 16);
            }

            NachtmusikVFXLibrary.AddNachtmusikLight(pos, 0.1f * visibility);
        }

        // =====================================================================
        //  MinionAttackVFX — Choral strike flash
        // =====================================================================
        public static void MinionAttackVFX(Vector2 minionPos, Vector2 direction)
        {
            // Sharp harmonic flash in attack direction
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = direction * (3f + Main.rand.NextFloat() * 2f)
                    + Main.rand.NextVector2Circular(1f, 1f);
                Dust d = Dust.NewDustPerfect(minionPos, DustID.BlueTorch, vel, 0, default, 0.7f);
                d.noGravity = true;
            }

            NachtmusikVFXLibrary.SpawnMusicNotes(minionPos, 1, 10f, 0.4f, 0.6f, 18);
            NachtmusikVFXLibrary.DrawBloom(minionPos, 0.2f, 0.4f);
        }

        // =====================================================================
        //  MinionImpactVFX — Harmonic resonance impact
        // =====================================================================
        public static void MinionImpactVFX(Vector2 hitPos)
        {
            NachtmusikVFXLibrary.ProjectileImpact(hitPos, 0.6f);

            // Resonant chime burst
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 vel = angle.ToRotationVector2() * (2f + Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(hitPos, DustID.BlueTorch, vel, 0, default, 0.6f);
                d.noGravity = true;
            }

            NachtmusikVFXLibrary.SpawnMusicNotes(hitPos, 2, 10f, 0.3f, 0.6f, 16);
        }

        // =====================================================================
        //  DespawnVFX — Chorus fades to silence
        // =====================================================================
        public static void DespawnVFX(Vector2 pos)
        {
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(1.5f, 1.5f);
                vel.Y -= 1f; // Ascend gently
                Dust d = Dust.NewDustPerfect(pos, DustID.BlueTorch, vel, 0, default, 0.5f);
                d.noGravity = true;
                d.fadeIn = 0.8f;
            }

            NachtmusikVFXLibrary.SpawnMusicNotes(pos, 2, 14f, 0.3f, 0.5f, 18);
        }
    }
}
