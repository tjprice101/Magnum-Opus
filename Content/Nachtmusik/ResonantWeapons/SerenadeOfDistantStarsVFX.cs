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
    /// Shader-driven VFX for Serenade of Distant Stars — the homing star ranged weapon.
    /// Uses StarHomingTrail.fx for graceful arcing ribbon trails with embedded stars.
    /// Warm, melodic, starlit — projectiles are singing stars that home on enemies.
    /// </summary>
    public static class SerenadeOfDistantStarsVFX
    {
        // =====================================================================
        //  HoldItemVFX — Warm starlit melody aura
        // =====================================================================
        public static void HoldItemVFX(Player player)
        {
            float time = (float)Main.timeForVisualEffects * 0.035f;

            // === INNER GOLDEN ORRERY ORBIT === Fast, tight, warm gold
            if (Main.rand.NextBool(3))
            {
                float innerAngle = time * 2.2f + Main.rand.NextFloat() * 0.4f;
                float innerRadius = 12f + (float)Math.Sin(time * 3f) * 3f;
                Vector2 innerPos = player.Center + new Vector2(
                    (float)Math.Cos(innerAngle) * innerRadius,
                    (float)Math.Sin(innerAngle) * innerRadius * 0.5f);
                Vector2 tangent = new Vector2(-(float)Math.Sin(innerAngle), (float)Math.Cos(innerAngle)) * 0.4f;

                Dust inner = Dust.NewDustPerfect(innerPos, DustID.GoldFlame, tangent, 0, default, 0.5f);
                inner.noGravity = true;
                inner.fadeIn = 0.7f;
            }

            // === OUTER BLUE ACCENT ORBIT === Slower, wider, harmonic complement
            if (Main.rand.NextBool(4))
            {
                float outerAngle = time * 0.9f + MathHelper.Pi;
                float outerRadius = 24f + (float)Math.Sin(time * 1.5f) * 5f;
                Vector2 outerPos = player.Center + new Vector2(
                    (float)Math.Cos(outerAngle) * outerRadius,
                    (float)Math.Sin(outerAngle) * outerRadius * 0.6f);
                Vector2 tangent = new Vector2(-(float)Math.Sin(outerAngle), (float)Math.Cos(outerAngle)) * 0.25f;

                Dust outer = Dust.NewDustPerfect(outerPos, DustID.BlueTorch, tangent, 0, default, 0.4f);
                outer.noGravity = true;
                outer.fadeIn = 0.6f;
            }

            // === ASCENDING MELODY NOTES === Warm gold dust rising like a sung melody
            if (Main.rand.NextBool(6))
            {
                Vector2 notePos = player.Center + new Vector2(Main.rand.NextFloat(-20f, 20f), 14f);
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -0.7f);
                Dust note = Dust.NewDustPerfect(notePos, DustID.GoldFlame, noteVel, 0, default, 0.4f);
                note.noGravity = true;
                note.fadeIn = 0.6f;
            }

            // Warm starlight twinkle
            if (Main.rand.NextBool(8))
            {
                NachtmusikVFXLibrary.SpawnTwinklingStars(player.Center, 1, 22f);
            }

            NachtmusikVFXLibrary.AddNachtmusikLight(player.Center, 0.24f);
        }

        // =====================================================================
        //  PreDrawInWorldBloom
        // =====================================================================
        public static void PreDrawInWorldBloom(SpriteBatch sb, Texture2D tex, Vector2 pos,
            Vector2 origin, float rotation, float scale)
        {
            NachtmusikVFXLibrary.DrawNachtmusikBloomStack(sb, pos,
                NachtmusikPalette.StarGold, NachtmusikPalette.DeepBlue, scale * 0.25f, 0.35f);
        }

        // =====================================================================
        //  MuzzleFlashVFX — Star melody launch
        // =====================================================================
        public static void MuzzleFlashVFX(Vector2 muzzlePos, Vector2 direction)
        {
            // === MELODIC ARC SPRAY === Golden stars in a musical arc pattern
            for (int i = 0; i < 7; i++)
            {
                float arcT = (i / 6f) * 2f - 1f; // -1 to 1
                float spread = MathHelper.ToRadians(20f) * arcT;
                Vector2 vel = direction.RotatedBy(spread) * (3.5f + Math.Abs(arcT) * 1.5f);
                Dust d = Dust.NewDustPerfect(muzzlePos, DustID.GoldFlame, vel, 0, default, 0.85f);
                d.noGravity = true;
                d.fadeIn = 0.9f;
            }

            // Blue harmony accent sparks
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = direction.RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f)) * 2.5f
                    + Main.rand.NextVector2Circular(1f, 1f);
                Dust d = Dust.NewDustPerfect(muzzlePos, DustID.BlueTorch, vel, 0, default, 0.55f);
                d.noGravity = true;
            }

            // Musical note cascade on every shot
            NachtmusikVFXLibrary.SpawnMusicNotes(muzzlePos, 3, 16f, 0.5f, 0.8f, 24);
            NachtmusikVFXLibrary.DrawBloom(muzzlePos, 0.4f, 0.7f);
            NachtmusikVFXLibrary.AddPaletteLighting(muzzlePos, 0.5f, 0.55f);
        }

        // =====================================================================
        //  ProjectileTrailVFX — Singing star ribbon trail
        // =====================================================================
        public static void ProjectileTrailVFX(Vector2 pos, Vector2 velocity)
        {
            // Golden star dust trail
            Vector2 dustVel = -velocity * 0.1f + Main.rand.NextVector2Circular(0.8f, 0.8f);
            Dust d = Dust.NewDustPerfect(pos, DustID.GoldFlame, dustVel, 0, default, 0.6f);
            d.noGravity = true;
            d.fadeIn = 0.8f;

            // Occasional twinkling accent
            if (Main.rand.NextBool(4))
            {
                Dust b = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(6f, 6f),
                    DustID.BlueTorch, Vector2.Zero, 0, default, 0.35f);
                b.noGravity = true;
                b.fadeIn = 0.6f;
            }

            if (Main.rand.NextBool(6))
            {
                NachtmusikVFXLibrary.SpawnTwinklingStars(pos, 1, 10f);
            }

            NachtmusikVFXLibrary.AddPaletteLighting(pos, 0.35f, 0.3f);
        }

        // =====================================================================
        //  SmallHitVFX — Star chime impact
        // =====================================================================
        public static void SmallHitVFX(Vector2 hitPos)
        {
            NachtmusikVFXLibrary.ProjectileImpact(hitPos, 0.8f);

            // Star-point burst on hit
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * (2.5f + Main.rand.NextFloat() * 1.5f);
                Dust d = Dust.NewDustPerfect(hitPos, DustID.GoldFlame, vel, 0, default, 0.7f);
                d.noGravity = true;
            }

            NachtmusikVFXLibrary.SpawnMusicNotes(hitPos, 2, 12f, 0.4f, 0.7f, 20);
            NachtmusikVFXLibrary.DrawBloom(hitPos, 0.3f, 0.55f);
        }

        // =====================================================================
        //  ProjectileDeathVFX — Star melody fade
        // =====================================================================
        public static void ProjectileDeathVFX(Vector2 pos)
        {
            // Warm golden star burst fading gently
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2.5f, 2.5f);
                Dust d = Dust.NewDustPerfect(pos, DustID.GoldFlame, vel, 0, default, 0.7f);
                d.noGravity = true;
                d.fadeIn = 1f;
            }

            NachtmusikVFXLibrary.SpawnTwinklingStars(pos, 3, 16f);
            NachtmusikVFXLibrary.SpawnMusicNotes(pos, 2, 14f, 0.5f, 0.7f, 20);
        }
    }
}
