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
            if (Main.rand.NextBool(3))
            {
                // Orbiting warm gold motes — like notes drifting gently
                float angle = (float)Main.timeForVisualEffects * 0.03f + Main.rand.NextFloat() * MathHelper.TwoPi;
                float radius = 18f + Main.rand.NextFloat() * 14f;
                Vector2 orbPos = player.Center + new Vector2(
                    (float)Math.Cos(angle) * radius,
                    (float)Math.Sin(angle) * radius * 0.6f);
                Vector2 vel = new Vector2(-(float)Math.Sin(angle), (float)Math.Cos(angle)) * 0.3f;

                Dust d = Dust.NewDustPerfect(orbPos, DustID.GoldFlame, vel, 0, default, 0.5f);
                d.noGravity = true;
                d.fadeIn = 0.7f;
            }

            if (Main.rand.NextBool(6))
            {
                NachtmusikVFXLibrary.SpawnTwinklingStars(player.Center, 1, 22f);
            }

            NachtmusikVFXLibrary.AddNachtmusikLight(player.Center, 0.2f);
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
            // Clean golden flash with star points
            for (int i = 0; i < 5; i++)
            {
                float spread = MathHelper.ToRadians(15f);
                Vector2 vel = direction.RotatedBy(Main.rand.NextFloat(-spread, spread))
                    * (3f + Main.rand.NextFloat() * 2f);
                Dust d = Dust.NewDustPerfect(muzzlePos, DustID.GoldFlame, vel, 0, default, 0.8f);
                d.noGravity = true;
                d.fadeIn = 0.9f;
            }

            // Blue accent sparks
            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = direction * 2f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                Dust d = Dust.NewDustPerfect(muzzlePos, DustID.BlueTorch, vel, 0, default, 0.5f);
                d.noGravity = true;
            }

            NachtmusikVFXLibrary.SpawnMusicNotes(muzzlePos, 2, 14f, 0.5f, 0.7f, 22);
            NachtmusikVFXLibrary.DrawBloom(muzzlePos, 0.35f, 0.65f);
            NachtmusikVFXLibrary.AddPaletteLighting(muzzlePos, 0.5f, 0.5f);
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
