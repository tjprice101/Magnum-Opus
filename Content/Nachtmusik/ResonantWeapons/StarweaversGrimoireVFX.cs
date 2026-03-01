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
    /// Shader-driven VFX for Starweaver's Grimoire — the constellation-charge magic weapon.
    /// Uses ConstellationWeave.fx for the living star-map charge orb.
    /// Stars connect as charge builds, forming a complete constellation before bursting.
    /// </summary>
    public static class StarweaversGrimoireVFX
    {
        // =====================================================================
        //  HoldItemVFX — Arcane starfield ambient
        // =====================================================================
        public static void HoldItemVFX(Player player)
        {
            float time = (float)Main.timeForVisualEffects * 0.04f;

            // === CONSTELLATION WEB BUILDING === Star points form connecting pattern
            int webPoints = 5;
            if (Main.rand.NextBool(4))
            {
                for (int i = 0; i < webPoints; i++)
                {
                    float angle = MathHelper.TwoPi * i / webPoints + time * 0.6f;
                    float radius = 22f + (float)Math.Sin(time * 1.5f + i) * 5f;
                    Vector2 starPos = player.Center + new Vector2(
                        (float)Math.Cos(angle) * radius,
                        (float)Math.Sin(angle) * radius * 0.55f);

                    if (Main.rand.NextBool(2))
                    {
                        Dust d = Dust.NewDustPerfect(starPos, DustID.PurpleTorch,
                            Vector2.Zero, 0, default, 0.45f);
                        d.noGravity = true;
                        d.fadeIn = 0.6f;
                    }
                }
            }

            // === STAR-WEB CONNECTING LINES === Occasional gold dust between constellation points
            if (Main.rand.NextBool(10))
            {
                int a = Main.rand.Next(webPoints);
                int b = (a + 2) % webPoints;
                float angleA = MathHelper.TwoPi * a / webPoints + time * 0.6f;
                float angleB = MathHelper.TwoPi * b / webPoints + time * 0.6f;
                float rA = 22f, rB = 22f;
                Vector2 posA = player.Center + new Vector2((float)Math.Cos(angleA) * rA, (float)Math.Sin(angleA) * rA * 0.55f);
                Vector2 posB = player.Center + new Vector2((float)Math.Cos(angleB) * rB, (float)Math.Sin(angleB) * rB * 0.55f);
                float t = Main.rand.NextFloat();
                Vector2 linePos = Vector2.Lerp(posA, posB, t);
                Dust line = Dust.NewDustPerfect(linePos, DustID.GoldFlame,
                    Vector2.Zero, 0, default, 0.3f);
                line.noGravity = true;
                line.fadeIn = 0.4f;
            }

            // Arcane shimmer
            if (Main.rand.NextBool(10))
            {
                NachtmusikVFXLibrary.SpawnTwinklingStars(player.Center, 1, 28f);
            }

            NachtmusikVFXLibrary.AddNachtmusikLight(player.Center, 0.22f);
        }

        // =====================================================================
        //  PreDrawInWorldBloom
        // =====================================================================
        public static void PreDrawInWorldBloom(SpriteBatch sb, Texture2D tex, Vector2 pos,
            Vector2 origin, float rotation, float scale)
        {
            NachtmusikVFXLibrary.DrawNachtmusikBloomStack(sb, pos,
                NachtmusikPalette.CosmicPurple, NachtmusikPalette.StarGold, scale * 0.3f, 0.4f);
        }

        // =====================================================================
        //  CastBurstVFX — Burst when firing orb projectile
        // =====================================================================
        public static void CastBurstVFX(Vector2 castPos)
        {
            // Star-weave discharge burst
            NachtmusikVFXLibrary.SpawnStarBurst(castPos, 6, 0.4f);
            NachtmusikVFXLibrary.SpawnMusicNotes(castPos, 2, 15f, 0.5f, 0.8f, 25);

            // Constellation fragment scatter
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * (3f + Main.rand.NextFloat() * 2f);
                Dust d = Dust.NewDustPerfect(castPos, DustID.PurpleTorch, vel, 0, default, 0.9f);
                d.noGravity = true;
            }

            NachtmusikVFXLibrary.DrawBloom(castPos, 0.35f, 0.7f);
            NachtmusikVFXLibrary.AddPaletteLighting(castPos, 0.3f, 0.6f);
        }

        // =====================================================================
        //  ProjectileTrailVFX — Constellation orb in flight
        // =====================================================================
        public static void ProjectileTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.rand.NextBool(2))
            {
                Vector2 dustVel = -velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f);
                Dust d = Dust.NewDustPerfect(pos, DustID.PurpleTorch, dustVel, 0, default, 0.7f);
                d.noGravity = true;
            }

            if (Main.rand.NextBool(4))
            {
                NachtmusikVFXLibrary.SpawnTwinklingStars(pos, 1, 8f);
            }

            NachtmusikVFXLibrary.AddPaletteLighting(pos, 0.3f, 0.4f);
        }

        // =====================================================================
        //  SmallHitVFX — Projectile impact
        // =====================================================================
        public static void SmallHitVFX(Vector2 hitPos)
        {
            NachtmusikVFXLibrary.ProjectileImpact(hitPos, 0.8f);
            NachtmusikVFXLibrary.SpawnMusicNotes(hitPos, 2, 12f, 0.4f, 0.7f, 20);
            NachtmusikVFXLibrary.DrawBloom(hitPos, 0.3f, 0.6f);
        }

        // =====================================================================
        //  SpecialCastVFX — Constellation burst (right-click release)
        // =====================================================================
        public static void SpecialCastVFX(Vector2 pos, float intensity = 1f)
        {
            // Grand constellation completion burst
            NachtmusikVFXLibrary.SpawnConstellationCircle(pos, 50f * intensity, 7,
                Main.rand.NextFloat() * MathHelper.TwoPi);

            NachtmusikVFXLibrary.SpawnStarburstCascade(pos, 5, intensity, 1f);
            NachtmusikVFXLibrary.SpawnOrbitingGlyphs(pos, 4, 40f * intensity,
                Main.rand.NextFloat() * MathHelper.TwoPi);

            NachtmusikVFXLibrary.SpawnMusicNotes(pos, 6, 30f, 0.6f, 1f, 35);
            NachtmusikVFXLibrary.SpawnRadialDustBurst(pos, 15, 6f * intensity);

            NachtmusikVFXLibrary.DrawComboBloom(pos, 2, 0.5f * intensity, 0.9f);
            NachtmusikVFXLibrary.AddPaletteLighting(pos, 0.2f, 1f * intensity);
        }

        // =====================================================================
        //  ProjectileDeathVFX — Orb expiry
        // =====================================================================
        public static void ProjectileDeathVFX(Vector2 pos)
        {
            NachtmusikVFXLibrary.SpawnShatteredStarlight(pos, 5, 4f, 0.6f, true);
            NachtmusikVFXLibrary.SpawnMusicNotes(pos, 2, 15f, 0.4f, 0.7f, 20);
        }
    }
}
