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
    /// Shader-driven VFX for Constellation Piercer — the precision triple-bolt ranged weapon.
    /// Uses StarChainBeam.fx for constellation-linked bullet trails.
    /// Each bolt is a star point in a constellation chain.
    /// </summary>
    public static class ConstellationPiercerVFX
    {
        // =====================================================================
        //  HoldItemVFX — Crosshair constellation ambient
        // =====================================================================
        public static void HoldItemVFX(Player player)
        {
            float time = (float)Main.timeForVisualEffects * 0.05f;

            // === CONSTELLATION TARGETING RETICLE === 4 fixed star points in crosshair formation
            for (int i = 0; i < 4; i++)
            {
                float baseAngle = MathHelper.PiOver2 * i + time * 0.5f;
                float radius = 18f + (float)Math.Sin(time * 2f + i * MathHelper.PiOver2) * 4f;
                Vector2 starPos = player.Center + new Vector2(
                    (float)Math.Cos(baseAngle) * radius,
                    (float)Math.Sin(baseAngle) * radius);

                if (Main.rand.NextBool(3))
                {
                    Dust d = Dust.NewDustPerfect(starPos, DustID.BlueTorch,
                        Vector2.Zero, 0, default, 0.4f);
                    d.noGravity = true;
                    d.fadeIn = 0.5f;
                }
            }

            // === CROSSHAIR LOCK ACCENT === Occasional gold center flash
            if (Main.rand.NextBool(6))
            {
                Dust gold = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(5f, 5f),
                    DustID.Enchanted_Gold, Vector2.Zero, 0, default, 0.35f);
                gold.noGravity = true;
                gold.fadeIn = 0.5f;
            }

            // Precision twinkling at reticle edge
            if (Main.rand.NextBool(10))
            {
                NachtmusikVFXLibrary.SpawnTwinklingStars(player.Center, 1, 22f);
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
                NachtmusikPalette.ConstellationBlue, NachtmusikPalette.StarGold, scale * 0.25f, 0.4f);
        }

        // =====================================================================
        //  MuzzleFlashVFX — Constellation-point muzzle flash
        // =====================================================================
        public static void MuzzleFlashVFX(Vector2 muzzlePos, Vector2 direction)
        {
            // === CONSTELLATION-POINT PRECISION FLASH === Sharp 4-directional crosshair pattern
            NachtmusikVFXLibrary.SpawnStarBurst(muzzlePos, 5, 0.4f);

            // Precision directional bolt chain
            for (int i = 0; i < 6; i++)
            {
                float spread = MathHelper.ToRadians(8f);
                Vector2 vel = direction.RotatedBy(Main.rand.NextFloat(-spread, spread))
                    * (4f + Main.rand.NextFloat() * 3f);
                Dust d = Dust.NewDustPerfect(muzzlePos, DustID.BlueTorch, vel, 0, default, 0.9f);
                d.noGravity = true;
                d.fadeIn = 0.9f;
            }

            // Crosshair flash lines — 4 sharp dust lines outward from muzzle
            for (int i = 0; i < 4; i++)
            {
                float angle = direction.ToRotation() + MathHelper.PiOver2 * i;
                Vector2 lineDir = angle.ToRotationVector2();
                for (int j = 1; j <= 3; j++)
                {
                    Dust line = Dust.NewDustPerfect(muzzlePos + lineDir * (j * 6f),
                        DustID.BlueTorch, lineDir * 0.8f, 0, default, 0.5f - j * 0.1f);
                    line.noGravity = true;
                    line.fadeIn = 0.4f;
                }
            }

            // Gold accent flash
            Dust gold = Dust.NewDustPerfect(muzzlePos, DustID.Enchanted_Gold,
                direction * 1.5f, 0, default, 0.6f);
            gold.noGravity = true;

            NachtmusikVFXLibrary.SpawnMusicNotes(muzzlePos, 1, 10f, 0.4f, 0.6f, 18);
            NachtmusikVFXLibrary.DrawBloom(muzzlePos, 0.35f, 0.75f);
            NachtmusikVFXLibrary.AddPaletteLighting(muzzlePos, 0.3f, 0.65f);
        }

        // =====================================================================
        //  ProjectileTrailVFX — Bolt in flight
        // =====================================================================
        public static void ProjectileTrailVFX(Vector2 pos, Vector2 velocity)
        {
            // Clean star-point trail
            Vector2 dustVel = -velocity * 0.08f + Main.rand.NextVector2Circular(0.5f, 0.5f);
            Dust d = Dust.NewDustPerfect(pos, DustID.BlueTorch, dustVel, 0, default, 0.6f);
            d.noGravity = true;

            if (Main.rand.NextBool(3))
            {
                NachtmusikVFXLibrary.SpawnTwinklingStars(pos, 1, 5f);
            }

            NachtmusikVFXLibrary.AddPaletteLighting(pos, 0.4f, 0.35f);
        }

        // =====================================================================
        //  SmallHitVFX — Bolt impact
        // =====================================================================
        public static void SmallHitVFX(Vector2 hitPos)
        {
            NachtmusikVFXLibrary.ProjectileImpact(hitPos, 0.7f);

            // Constellation fragment scatter
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f + Main.rand.NextFloat() * 0.3f;
                Vector2 vel = angle.ToRotationVector2() * (2f + Main.rand.NextFloat() * 2f);
                Dust d = Dust.NewDustPerfect(hitPos, DustID.BlueTorch, vel, 0, default, 0.7f);
                d.noGravity = true;
            }

            NachtmusikVFXLibrary.SpawnMusicNotes(hitPos, 1, 10f, 0.3f, 0.6f, 18);
            NachtmusikVFXLibrary.DrawBloom(hitPos, 0.25f, 0.5f);
        }

        // =====================================================================
        //  ProjectileDeathVFX — Bolt expiry
        // =====================================================================
        public static void ProjectileDeathVFX(Vector2 pos)
        {
            NachtmusikVFXLibrary.SpawnShatteredStarlight(pos, 3, 3f, 0.5f, true);
        }
    }
}
