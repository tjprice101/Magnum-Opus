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
            if (Main.rand.NextBool(5))
            {
                // Faint constellation star motes orbiting the weapon
                float angle = (float)Main.timeForVisualEffects * 0.05f + Main.rand.NextFloat() * MathHelper.TwoPi;
                float radius = 18f + Main.rand.NextFloat() * 10f;
                Vector2 pos = player.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;

                Dust d = Dust.NewDustPerfect(pos, DustID.BlueTorch,
                    Main.rand.NextVector2Circular(0.2f, 0.2f), 0, default, 0.4f);
                d.noGravity = true;
                d.fadeIn = 0.6f;
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
                NachtmusikPalette.ConstellationBlue, NachtmusikPalette.StarGold, scale * 0.25f, 0.4f);
        }

        // =====================================================================
        //  MuzzleFlashVFX — Constellation-point muzzle flash
        // =====================================================================
        public static void MuzzleFlashVFX(Vector2 muzzlePos, Vector2 direction)
        {
            // Sharp constellation-point flash
            NachtmusikVFXLibrary.SpawnStarBurst(muzzlePos, 4, 0.35f);

            // Directional star chain particles
            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = direction * (3f + Main.rand.NextFloat() * 2f)
                    + Main.rand.NextVector2Circular(1.5f, 1.5f);
                Dust d = Dust.NewDustPerfect(muzzlePos, DustID.BlueTorch, vel, 0, default, 0.8f);
                d.noGravity = true;
                d.fadeIn = 0.9f;
            }

            // Gold accent flash
            Dust gold = Dust.NewDustPerfect(muzzlePos, DustID.Enchanted_Gold,
                direction * 1.5f, 0, default, 0.5f);
            gold.noGravity = true;

            NachtmusikVFXLibrary.SpawnMusicNotes(muzzlePos, 1, 10f, 0.4f, 0.6f, 18);
            NachtmusikVFXLibrary.DrawBloom(muzzlePos, 0.3f, 0.7f);
            NachtmusikVFXLibrary.AddPaletteLighting(muzzlePos, 0.3f, 0.6f);
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
