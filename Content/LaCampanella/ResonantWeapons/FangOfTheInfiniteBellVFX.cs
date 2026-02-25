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
    /// VFX helper for the Fang of the Infinite Bell magic weapon.
    /// Handles projectile trails, fang bite mark impacts, empowerment
    /// burst, and hit counter VFX.
    /// </summary>
    public static class FangOfTheInfiniteBellVFX
    {
        // =====================================================================
        //  HOLD ITEM VFX
        // =====================================================================

        /// <summary>
        /// Per-frame held-item VFX: subtle bronze gleam, bell-metal shimmer.
        /// </summary>
        public static void HoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Bell-metal shimmer
            if (Main.rand.NextBool(6))
            {
                Vector2 sparkPos = center + Main.rand.NextVector2Circular(15f, 15f);
                Color col = LaCampanellaPalette.GetBellShimmer(time);
                Dust d = Dust.NewDustPerfect(sparkPos, DustID.Enchanted_Gold, Vector2.Zero, 0, col, 0.5f);
                d.noGravity = true;
            }

            // Subtle ambient light
            float pulse = 0.4f + MathF.Sin(time * 0.04f) * 0.1f;
            Lighting.AddLight(center, LaCampanellaPalette.BellGold.ToVector3() * pulse);
        }

        // =====================================================================
        //  PROJECTILE TRAIL VFX
        // =====================================================================

        /// <summary>
        /// Per-frame fang projectile trail VFX.
        /// Sharp bronze-gold dust trail with bell-metal sparkles.
        /// </summary>
        public static void FangProjectileTrail(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            // Sharp bronze dust trail
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = -velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 2.5f)
                    + Main.rand.NextVector2Circular(0.5f, 0.5f);
                Color col = Color.Lerp(LaCampanellaPalette.BellBronze, LaCampanellaPalette.BellGold,
                    Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.2f);
                d.noGravity = true;
            }

            // Bell-metal sparkle
            if (Main.rand.NextBool(2))
            {
                Color sparkCol = LaCampanellaPalette.ChimeShimmer;
                Dust d = Dust.NewDustPerfect(pos, DustID.Enchanted_Gold,
                    Main.rand.NextVector2Circular(1f, 1f), 0, sparkCol, 0.7f);
                d.noGravity = true;
            }

            // Occasional music note
            if (Main.rand.NextBool(8))
                LaCampanellaVFXLibrary.SpawnMusicNotes(pos, 1, 8f, 0.7f, 0.85f, 20);

            Lighting.AddLight(pos, LaCampanellaPalette.BellGold.ToVector3() * 0.5f);
        }

        // =====================================================================
        //  FANG BITE MARK VFX
        // =====================================================================

        /// <summary>
        /// Fang bite mark impact pattern — dual crescent dust forming a bite shape.
        /// Creates the signature fang visual on hit.
        /// </summary>
        public static void FangBiteMarkVFX(Vector2 pos, Vector2 hitDirection)
        {
            if (Main.dedServ) return;

            float baseAngle = hitDirection.ToRotation();

            // Upper fang arc (5 points)
            for (int i = 0; i < 5; i++)
            {
                float arcAngle = baseAngle - 0.6f + 1.2f * i / 4f;
                Vector2 dustPos = pos + arcAngle.ToRotationVector2() * 20f;
                Vector2 vel = arcAngle.ToRotationVector2() * 3f;
                Color col = LaCampanellaPalette.PaletteLerp(LaCampanellaPalette.FangBlade, (float)i / 4f);
                Dust d = Dust.NewDustPerfect(dustPos, DustID.Torch, vel, 0, col, 1.4f);
                d.noGravity = true;
            }

            // Lower fang arc (5 points, mirrored)
            for (int i = 0; i < 5; i++)
            {
                float arcAngle = baseAngle + MathHelper.Pi - 0.6f + 1.2f * i / 4f;
                Vector2 dustPos = pos + arcAngle.ToRotationVector2() * 20f;
                Vector2 vel = arcAngle.ToRotationVector2() * 3f;
                Color col = LaCampanellaPalette.PaletteLerp(LaCampanellaPalette.FangBlade, (float)i / 4f);
                Dust d = Dust.NewDustPerfect(dustPos, DustID.Torch, vel, 0, col, 1.4f);
                d.noGravity = true;
            }

            // Center impact bloom
            LaCampanellaVFXLibrary.DrawBloom(pos, 0.4f);

            // Bell chime on bite
            CustomParticles.LaCampanellaBellChime(pos, 6);

            LaCampanellaVFXLibrary.SpawnMusicNotes(pos, 2, 15f);

            Lighting.AddLight(pos, LaCampanellaPalette.BellGold.ToVector3() * 0.8f);
        }

        // =====================================================================
        //  EMPOWERMENT BURST
        // =====================================================================

        /// <summary>
        /// Empowerment activation VFX — triggers on 3-hit counter.
        /// Grand bell flash with ascending golden particles.
        /// </summary>
        public static void EmpowermentBurstVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            // Grand bloom flash
            LaCampanellaVFXLibrary.DrawBloom(pos, 0.9f);

            // Ascending golden dust ring
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 vel = angle.ToRotationVector2() * 5f + new Vector2(0, -2f);
                Color col = LaCampanellaPalette.GetBellGradient((float)i / 16f);
                Dust d = Dust.NewDustPerfect(pos, DustID.GoldFlame, vel, 0, col, 1.5f);
                d.noGravity = true;
            }

            // Bell shockwave
            LaCampanellaVFXLibrary.SpawnBellChimeRings(pos, 3, 0.4f);

            // Music notes
            LaCampanellaVFXLibrary.SpawnMusicNotes(pos, 6, 35f, 0.8f, 1.1f, 35);

            // Smoke flourish
            LaCampanellaVFXLibrary.SpawnHeavySmoke(pos, 4, 0.7f, 3f, 50);

            MagnumScreenEffects.AddScreenShake(3f);
            Lighting.AddLight(pos, LaCampanellaPalette.WhiteHot.ToVector3() * 1.2f);
        }

        // =====================================================================
        //  PROJECTILE DEATH
        // =====================================================================

        /// <summary>
        /// Fang projectile on-kill / expiration VFX.
        /// </summary>
        public static void FangProjectileDeathVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            LaCampanellaVFXLibrary.ProjectileImpact(pos, 0.8f);
            LaCampanellaVFXLibrary.SpawnEmberScatter(pos, 6, 3f);
        }
    }
}
