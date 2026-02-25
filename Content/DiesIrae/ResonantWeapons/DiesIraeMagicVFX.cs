using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.DiesIrae.ResonantWeapons
{
    // =============================================================================
    //  STAFF OF FINAL JUDGEMENT — VFX
    //  Identity: Divine condemnation, 5 floating ignitions, orbit-then-home explosions.
    //  Deliberate, absolute, ecclesiastical.
    // =============================================================================

    public static class StaffOfFinalJudgementVFX
    {
        public static void HoldItemVFX(Player player, Item item)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Ecclesiastical shimmer (judgment gold + doom purple)
            if (Main.rand.NextBool(5))
            {
                Color shimmer = DiesIraePalette.GetJudgmentShimmer(time);
                Dust d = Dust.NewDustPerfect(center + Main.rand.NextVector2Circular(20f, 20f),
                    DustID.Enchanted_Gold, new Vector2(0, -0.3f), 0, shimmer, 0.6f);
                d.noGravity = true;
            }

            // Doom purple wisps
            if (Main.rand.NextBool(8))
            {
                Vector2 pos = center + Main.rand.NextVector2Circular(18f, 18f);
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, new Vector2(0, -0.5f), 0,
                    DiesIraePalette.DoomPurple, 0.7f);
                d.noGravity = true;
            }

            DiesIraeVFXLibrary.SpawnAmbientSmoke(center, 22f);

            if (Main.rand.NextBool(25))
                DiesIraeVFXLibrary.SpawnMusicNotes(center, 1, 20f, 0.7f, 0.9f, 30);

            Lighting.AddLight(center, DiesIraePalette.JudgmentGold.ToVector3() * 0.35f);
        }

        public static void PreDrawInWorldBloom(
            Microsoft.Xna.Framework.Graphics.SpriteBatch sb,
            Microsoft.Xna.Framework.Graphics.Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.04f) * 0.03f;
            DiesIraePalette.DrawItemBloom(sb, tex, pos, origin, rotation, scale, pulse);
        }

        /// <summary>
        /// Cast VFX — staff fires 5 floating ignitions.
        /// </summary>
        public static void CastVFX(Vector2 castPos)
        {
            if (Main.dedServ) return;

            DiesIraeVFXLibrary.DrawBloom(castPos, 0.5f);
            DiesIraeVFXLibrary.SpawnMusicNotes(castPos, 4, 25f, 0.8f, 1.0f, 30);
            DiesIraeVFXLibrary.SpawnEmberScatter(castPos, 6, 3f);

            // Doom purple flash ring
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Dust d = Dust.NewDustPerfect(castPos, DustID.Torch, vel, 0,
                    DiesIraePalette.GetJudgmentGradient((float)i / 8f), 1.2f);
                d.noGravity = true;
            }

            Lighting.AddLight(castPos, DiesIraePalette.JudgmentGold.ToVector3() * 0.8f);
        }

        /// <summary>
        /// Floating ignition orbit frame VFX.
        /// </summary>
        public static void IgnitionOrbitVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            if (Main.rand.NextBool(2))
            {
                Vector2 vel = Main.rand.NextVector2Circular(0.3f, 0.3f);
                Color col = Color.Lerp(DiesIraePalette.JudgmentGold, DiesIraePalette.WrathWhite, Main.rand.NextFloat(0.3f));
                Dust d = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(4f, 4f),
                    DustID.Torch, vel, 0, col, 0.8f);
                d.noGravity = true;
            }

            Lighting.AddLight(pos, DiesIraePalette.JudgmentGold.ToVector3() * 0.4f);
        }

        /// <summary>
        /// Ignition homing trail VFX — seeking judgment orb.
        /// </summary>
        public static void IgnitionHomingTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 vel = -velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 2f)
                + Main.rand.NextVector2Circular(0.3f, 0.3f);
            Color col = DiesIraePalette.GetJudgmentGradient(Main.rand.NextFloat());
            Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.0f);
            d.noGravity = true;

            Lighting.AddLight(pos, DiesIraePalette.JudgmentGold.ToVector3() * 0.5f);
        }

        /// <summary>
        /// Ignition explosion VFX — judgment detonation.
        /// </summary>
        public static void IgnitionExplosionVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            DiesIraeVFXLibrary.ProjectileImpact(pos, 0.7f);
            CustomParticles.DiesIraeHellfireBurst(pos, 8);
            DiesIraeVFXLibrary.SpawnBoneAshScatter(pos, 3, 2.5f);

            MagnumScreenEffects.AddScreenShake(2f);
            Lighting.AddLight(pos, DiesIraePalette.WrathWhite.ToVector3() * 1.0f);
        }
    }

    // =============================================================================
    //  ECLIPSE OF WRATH — VFX
    //  Identity: Throwable orb, cursor-tracking, spawns wrath shards while airborne.
    //  Ominous, orbiting, the eclipse of divine judgment.
    // =============================================================================

    public static class EclipseOfWrathVFX
    {
        public static void HoldItemVFX(Player player, Item item)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Dark eclipse aura
            if (Main.rand.NextBool(6))
            {
                Vector2 pos = center + Main.rand.NextVector2Circular(20f, 20f);
                Color col = Color.Lerp(DiesIraePalette.CharcoalBlack, DiesIraePalette.BloodRed, Main.rand.NextFloat(0.3f));
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, new Vector2(0, -0.4f), 0, col, 0.7f);
                d.noGravity = true;
            }

            DiesIraeVFXLibrary.SpawnAmbientSmoke(center, 25f);

            if (Main.rand.NextBool(28))
                DiesIraeVFXLibrary.SpawnMusicNotes(center, 1, 18f, 0.65f, 0.85f, 30);

            Lighting.AddLight(center, DiesIraePalette.BloodRed.ToVector3() * 0.3f);
        }

        public static void PreDrawInWorldBloom(
            Microsoft.Xna.Framework.Graphics.SpriteBatch sb,
            Microsoft.Xna.Framework.Graphics.Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.04f) * 0.03f;
            DiesIraePalette.DrawItemBloom(sb, tex, pos, origin, rotation, scale, pulse);
        }

        /// <summary>
        /// Eclipse orb per-frame trail while airborne and tracking cursor.
        /// </summary>
        public static void EclipseOrbTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            // Dark eclipse trail
            Vector2 vel = -velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 2f);
            Color col = DiesIraePalette.GetWrathGradient(Main.rand.NextFloat());
            Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.1f);
            d.noGravity = true;

            // Orbiting wrath particles
            if (Main.rand.NextBool(3))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 orbPos = pos + angle.ToRotationVector2() * 12f;
                Dust orb = Dust.NewDustPerfect(orbPos, DustID.Torch, Vector2.Zero, 0,
                    DiesIraePalette.InfernalRed, 0.7f);
                orb.noGravity = true;
            }

            Lighting.AddLight(pos, DiesIraePalette.InfernalRed.ToVector3() * 0.5f);
        }

        /// <summary>
        /// Wrath shard spawn VFX — spawned while orb is airborne.
        /// </summary>
        public static void WrathShardSpawnVFX(Vector2 pos, Vector2 direction)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = direction * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(0.5f, 0.5f);
                Color col = DiesIraePalette.GetWrathGradient(Main.rand.NextFloat());
                Dust d2 = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.0f);
                d2.noGravity = true;
            }

            Lighting.AddLight(pos, DiesIraePalette.InfernalRed.ToVector3() * 0.4f);
        }

        /// <summary>
        /// Wrath shard trail VFX.
        /// </summary>
        public static void WrathShardTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 vel = -velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 1.5f);
            Color col = Color.Lerp(DiesIraePalette.BloodRed, DiesIraePalette.InfernalRed, Main.rand.NextFloat());
            Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 0.8f);
            d.noGravity = true;
        }

        /// <summary>
        /// Eclipse orb impact VFX.
        /// </summary>
        public static void EclipseOrbImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            DiesIraeVFXLibrary.ProjectileImpact(pos, 0.8f);
            CustomParticles.DiesIraeHellfireBurst(pos, 10);
            DiesIraeVFXLibrary.SpawnBoneAshScatter(pos, 5, 3f);

            MagnumScreenEffects.AddScreenShake(3f);
        }
    }

    // =============================================================================
    //  GRIMOIRE OF CONDEMNATION — VFX
    //  Identity: 3 spiraling music shards, chain electricity on impact.
    //  Arcane, condemning, ecclesiastical dread.
    // =============================================================================

    public static class GrimoireOfCondemnationVFX
    {
        public static void HoldItemVFX(Player player, Item item)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Arcane rune shimmer
            if (Main.rand.NextBool(6))
            {
                Color shimmer = DiesIraePalette.GetShimmer(time);
                Dust d = Dust.NewDustPerfect(center + Main.rand.NextVector2Circular(18f, 18f),
                    DustID.Enchanted_Gold, Vector2.Zero, 0, shimmer, 0.5f);
                d.noGravity = true;
            }

            // Faint parchment particles
            if (Main.rand.NextBool(10))
                DiesIraeVFXLibrary.SpawnBoneAshScatter(center, 1, 0.5f);

            DiesIraeVFXLibrary.SpawnAmbientSmoke(center, 20f);

            if (Main.rand.NextBool(25))
                DiesIraeVFXLibrary.SpawnMusicNotes(center, 1, 18f, 0.7f, 0.9f, 30);

            Lighting.AddLight(center, DiesIraePalette.InfernalRed.ToVector3() * 0.3f);
        }

        public static void PreDrawInWorldBloom(
            Microsoft.Xna.Framework.Graphics.SpriteBatch sb,
            Microsoft.Xna.Framework.Graphics.Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.04f) * 0.03f;
            DiesIraePalette.DrawItemBloom(sb, tex, pos, origin, rotation, scale, pulse);
        }

        /// <summary>
        /// Cast VFX — grimoire opens, condemnation fires.
        /// </summary>
        public static void CastVFX(Vector2 castPos, Vector2 direction)
        {
            if (Main.dedServ) return;

            DiesIraeVFXLibrary.DrawBloom(castPos, 0.4f);

            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = direction * Main.rand.NextFloat(2f, 4f)
                    + Main.rand.NextVector2Circular(1.5f, 1.5f);
                Color col = DiesIraePalette.GetWrathGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(castPos, DustID.Torch, vel, 0, col, 1.1f);
                d.noGravity = true;
            }

            DiesIraeVFXLibrary.SpawnMusicNotes(castPos, 3, 15f, 0.8f, 1.0f, 25);
            Lighting.AddLight(castPos, DiesIraePalette.InfernalRed.ToVector3() * 0.6f);
        }

        /// <summary>
        /// Spiraling music shard trail VFX.
        /// </summary>
        public static void MusicShardTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 vel = -velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 1.5f)
                + Main.rand.NextVector2Circular(0.3f, 0.3f);
            Color col = Color.Lerp(DiesIraePalette.InfernalRed, DiesIraePalette.JudgmentGold, Main.rand.NextFloat());
            Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 0.9f);
            d.noGravity = true;

            if (Main.rand.NextBool(5))
                DiesIraeVFXLibrary.SpawnMusicNotes(pos, 1, 6f, 0.6f, 0.8f, 20);

            Lighting.AddLight(pos, DiesIraePalette.InfernalRed.ToVector3() * 0.4f);
        }

        /// <summary>
        /// Music shard impact with chain electricity VFX.
        /// </summary>
        public static void MusicShardImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            DiesIraeVFXLibrary.ProjectileImpact(pos, 0.5f);
            DiesIraeVFXLibrary.SpawnBoneAshScatter(pos, 2, 2f);
        }

        /// <summary>
        /// Chain electricity arc from shard impact.
        /// </summary>
        public static void ChainElectricityVFX(Vector2 from, Vector2 to)
        {
            if (Main.dedServ) return;

            float dist = Vector2.Distance(from, to);
            int segments = (int)(dist / 12f);
            Vector2 dir = (to - from).SafeNormalize(Vector2.UnitX);

            for (int i = 0; i < segments; i++)
            {
                float t = (float)i / segments;
                Vector2 pos = Vector2.Lerp(from, to, t) + Main.rand.NextVector2Circular(5f, 5f);
                Color col = Color.Lerp(DiesIraePalette.InfernalRed, DiesIraePalette.JudgmentGold, t);
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, dir * 0.2f, 0, col, 0.8f);
                d.noGravity = true;
            }

            DiesIraeVFXLibrary.DrawBloom(to, 0.25f);
        }
    }
}
