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
    //  SIN COLLECTOR — VFX
    //  Identity: Sin-seeking sniper, divine trajectory, chain lightning on hit.
    //  Precise, seeking, the rifleman of judgment.
    // =============================================================================

    public static class SinCollectorVFX
    {
        public static void HoldItemVFX(Player player, Item item)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Targeting reticle orbit (single precise dot)
            float angle = time * 0.06f;
            Vector2 dotPos = center + angle.ToRotationVector2() * 16f;
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustPerfect(dotPos, DustID.Torch, Vector2.Zero, 0,
                    DiesIraePalette.InfernalRed, 0.7f);
                d.noGravity = true;
            }

            DiesIraeVFXLibrary.SpawnAmbientSmoke(center, 20f);

            if (Main.rand.NextBool(30))
                DiesIraeVFXLibrary.SpawnMusicNotes(center, 1, 18f, 0.6f, 0.85f, 30);

            Lighting.AddLight(center, DiesIraePalette.BloodRed.ToVector3() * 0.35f);
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
        /// Muzzle flash VFX on fire.
        /// </summary>
        public static void MuzzleFlashVFX(Vector2 muzzlePos, Vector2 direction)
        {
            if (Main.dedServ) return;

            DiesIraeVFXLibrary.DrawBloom(muzzlePos, 0.4f);

            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = direction * Main.rand.NextFloat(3f, 6f)
                    + Main.rand.NextVector2Circular(1.5f, 1.5f);
                Color col = DiesIraePalette.GetFireGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(muzzlePos, DustID.Torch, vel, 0, col, 1.2f);
                d.noGravity = true;
            }

            DiesIraeVFXLibrary.SpawnHeavySmoke(muzzlePos, 2, 0.5f, 2f, 30);
            Lighting.AddLight(muzzlePos, DiesIraePalette.InfernalRed.ToVector3() * 0.8f);
        }

        /// <summary>
        /// Bullet trail VFX — precise sin-seeking tracer.
        /// </summary>
        public static void BulletTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 vel = -velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 2f)
                + Main.rand.NextVector2Circular(0.3f, 0.3f);
            Color col = Color.Lerp(DiesIraePalette.BloodRed, DiesIraePalette.JudgmentGold, Main.rand.NextFloat());
            Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.0f);
            d.noGravity = true;

            Lighting.AddLight(pos, DiesIraePalette.InfernalRed.ToVector3() * 0.4f);
        }

        /// <summary>
        /// Bullet impact with chain lightning effect.
        /// </summary>
        public static void BulletImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            DiesIraeVFXLibrary.ProjectileImpact(pos, 0.7f);
            DiesIraeVFXLibrary.SpawnBoneAshScatter(pos, 3, 2f);
        }

        /// <summary>
        /// Chain lightning arc between enemies.
        /// </summary>
        public static void ChainLightningVFX(Vector2 from, Vector2 to)
        {
            if (Main.dedServ) return;

            float dist = Vector2.Distance(from, to);
            int segments = (int)(dist / 15f);
            Vector2 dir = (to - from).SafeNormalize(Vector2.UnitX);

            for (int i = 0; i < segments; i++)
            {
                float t = (float)i / segments;
                Vector2 pos = Vector2.Lerp(from, to, t)
                    + Main.rand.NextVector2Circular(6f, 6f); // jagged lightning
                Color col = Color.Lerp(DiesIraePalette.JudgmentGold, DiesIraePalette.WrathWhite, t);
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, dir * 0.3f, 0, col, 0.9f);
                d.noGravity = true;
            }

            DiesIraeVFXLibrary.DrawBloom(to, 0.3f);
        }
    }

    // =============================================================================
    //  DAMNATION'S CANNON — VFX
    //  Identity: Explosive damnation, volcanic wrath balls, orbiting shrapnel.
    //  Heavy, devastating, apocalyptic.
    // =============================================================================

    public static class DamnationsCannonVFX
    {
        public static void HoldItemVFX(Player player, Item item)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Smoldering barrel heat
            if (Main.rand.NextBool(5))
            {
                Vector2 pos = center + Main.rand.NextVector2Circular(18f, 18f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -0.8f);
                Color col = DiesIraePalette.GetFireGradient(Main.rand.NextFloat(0.2f, 0.5f));
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 0.8f);
                d.noGravity = true;
            }

            DiesIraeVFXLibrary.SpawnAmbientSmoke(center, 22f);

            if (Main.rand.NextBool(30))
                DiesIraeVFXLibrary.SpawnMusicNotes(center, 1, 20f, 0.65f, 0.85f, 30);

            Lighting.AddLight(center, DiesIraePalette.SmolderingEmber.ToVector3() * 0.35f);
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
        /// Cannon fire muzzle blast — heavy, volcanic.
        /// </summary>
        public static void MuzzleBlastVFX(Vector2 muzzlePos, Vector2 direction)
        {
            if (Main.dedServ) return;

            DiesIraeVFXLibrary.DrawBloom(muzzlePos, 0.6f);

            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = direction * Main.rand.NextFloat(4f, 8f)
                    + Main.rand.NextVector2Circular(2f, 2f);
                Color col = DiesIraePalette.GetFireGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(muzzlePos, DustID.Torch, vel, 0, col, 1.5f);
                d.noGravity = true;
            }

            DiesIraeVFXLibrary.SpawnHeavySmoke(muzzlePos, 4, 0.8f, 3f, 40);
            MagnumScreenEffects.AddScreenShake(3f);
            Lighting.AddLight(muzzlePos, DiesIraePalette.EmberOrange.ToVector3() * 1.0f);
        }

        /// <summary>
        /// Wrath ball projectile trail — volcanic fire comet.
        /// </summary>
        public static void WrathBallTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = -velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 3f)
                    + Main.rand.NextVector2Circular(0.5f, 0.5f);
                Color col = DiesIraePalette.GetFireGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.3f);
                d.noGravity = true;
            }

            if (Main.rand.NextBool(3))
                DiesIraeVFXLibrary.SpawnHeavySmoke(pos, 1, 0.3f, 1f, 20);

            Lighting.AddLight(pos, DiesIraePalette.EmberOrange.ToVector3() * 0.6f);
        }

        /// <summary>
        /// Wrath ball explosion — massive AOE with orbiting shrapnel spawn.
        /// </summary>
        public static void WrathBallExplosionVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            DiesIraeVFXLibrary.WrathShockwaveImpact(pos, 1.0f);

            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 9f);
                Color col = DiesIraePalette.GetFireGradient((float)i / 20f);
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.6f);
                d.noGravity = true;
            }

            DiesIraeVFXLibrary.SpawnBoneAshScatter(pos, 6, 4f);
            DiesIraeVFXLibrary.SpawnHeavySmoke(pos, 8, 1.0f, 4f, 60);

            MagnumScreenEffects.AddScreenShake(6f);
            Lighting.AddLight(pos, DiesIraePalette.WrathWhite.ToVector3() * 1.5f);
        }

        /// <summary>
        /// Shrapnel trail VFX — small orbiting debris.
        /// </summary>
        public static void ShrapnelTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 vel = -velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 1.5f);
            Color col = Color.Lerp(DiesIraePalette.SmolderingEmber, DiesIraePalette.HellfireGold, Main.rand.NextFloat());
            Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 0.8f);
            d.noGravity = true;

            Lighting.AddLight(pos, DiesIraePalette.EmberOrange.ToVector3() * 0.3f);
        }

        /// <summary>
        /// Shrapnel impact VFX.
        /// </summary>
        public static void ShrapnelImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;
            DiesIraeVFXLibrary.ProjectileImpact(pos, 0.3f);
        }
    }

    // =============================================================================
    //  ARBITER'S SENTENCE — VFX
    //  Identity: Sweeping judgment flame, flamethrower of divine wrath.
    //  Sustained, relentless, purifying.
    // =============================================================================

    public static class ArbitersSentenceVFX
    {
        public static void HoldItemVFX(Player player, Item item)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Simmering heat haze
            if (Main.rand.NextBool(6))
            {
                Vector2 pos = center + Main.rand.NextVector2Circular(20f, 20f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.2f, 0.2f), -0.5f);
                Color col = DiesIraePalette.InfernalRed * 0.6f;
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 0.6f);
                d.noGravity = true;
            }

            DiesIraeVFXLibrary.SpawnAmbientSmoke(center, 22f);

            if (Main.rand.NextBool(35))
                DiesIraeVFXLibrary.SpawnMusicNotes(center, 1, 18f, 0.6f, 0.85f, 30);

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
        /// Flame stream trail VFX — sustained judgment fire.
        /// </summary>
        public static void FlameStreamTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = -velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 2f)
                    + Main.rand.NextVector2Circular(1f, 1f);
                Color col = DiesIraePalette.GetFireGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(6f, 6f),
                    DustID.Torch, vel, 0, col, 1.2f);
                d.noGravity = true;
            }

            if (Main.rand.NextBool(4))
                DiesIraeVFXLibrary.SpawnHeavySmoke(pos, 1, 0.3f, 1f, 25);

            Lighting.AddLight(pos, DiesIraePalette.InfernalRed.ToVector3() * 0.5f);
        }

        /// <summary>
        /// Flame impact explosion VFX.
        /// </summary>
        public static void FlameImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            DiesIraeVFXLibrary.ProjectileImpact(pos, 0.5f);
            DiesIraeVFXLibrary.SpawnBoneAshScatter(pos, 3, 2f);
            DiesIraeVFXLibrary.SpawnHeavySmoke(pos, 3, 0.6f, 2f, 35);
        }
    }
}
