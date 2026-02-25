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
    //  DEATH TOLLING BELL — VFX
    //  Identity: Funeral bell minion. Solemn toll shockwaves, bone ash atmosphere,
    //  expanding judgment rings. Every toll is a death sentence.
    // =============================================================================

    public static class DeathTollingBellVFX
    {
        public static void HoldItemVFX(Player player, Item item)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;

            // Solemn ember glow
            if (Main.rand.NextBool(6))
            {
                Vector2 pos = center + Main.rand.NextVector2Circular(18f, 18f);
                Color col = Color.Lerp(DiesIraePalette.CharcoalBlack, DiesIraePalette.EmberOrange,
                    Main.rand.NextFloat(0.4f));
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, new Vector2(0, -0.3f), 0, col, 0.6f);
                d.noGravity = true;
            }

            DiesIraeVFXLibrary.SpawnAmbientSmoke(center, 20f);

            if (Main.rand.NextBool(30))
                DiesIraeVFXLibrary.SpawnMusicNotes(center, 1, 18f, 0.65f, 0.85f, 30);

            Lighting.AddLight(center, DiesIraePalette.EmberOrange.ToVector3() * 0.2f);
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
        /// Summon VFX — infernal bell materialization.
        /// </summary>
        public static void SummonVFX(Vector2 spawnPos)
        {
            if (Main.dedServ) return;

            DiesIraeVFXLibrary.ProjectileImpact(spawnPos, 1.2f);
            DiesIraeVFXLibrary.SpawnMusicNotes(spawnPos, 6, 25f, 0.8f, 1.0f, 35);
            DiesIraeVFXLibrary.SpawnHeavySmoke(spawnPos, 8, 0.8f, 3f, 50);
            DiesIraeVFXLibrary.SpawnBoneAshScatter(spawnPos, 4, 2f);

            Lighting.AddLight(spawnPos, DiesIraePalette.EmberOrange.ToVector3() * 0.8f);
        }

        /// <summary>
        /// Minion ambient aura — solemn funeral presence.
        /// </summary>
        public static void MinionAmbientVFX(Vector2 center)
        {
            if (Main.dedServ) return;

            // Ambient fire glow
            if (Main.rand.NextBool(4))
            {
                Vector2 pos = center + Main.rand.NextVector2Circular(12f, 12f);
                Vector2 vel = new Vector2(0, -0.5f) + Main.rand.NextVector2Circular(0.2f, 0.2f);
                Color col = Color.Lerp(DiesIraePalette.EmberOrange, DiesIraePalette.HellfireGold,
                    Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 0.7f);
                d.noGravity = true;
            }

            DiesIraeVFXLibrary.SpawnAmbientSmoke(center, 15f);

            if (Main.rand.NextBool(20))
                DiesIraeVFXLibrary.SpawnMusicNotes(center, 1, 12f, 0.6f, 0.8f, 25);

            Lighting.AddLight(center, DiesIraePalette.EmberOrange.ToVector3() * 0.5f);
        }

        /// <summary>
        /// Bell toll charge VFX — converging fire particles.
        /// </summary>
        public static void TollChargeVFX(Vector2 center, float progress)
        {
            if (Main.dedServ) return;

            int dustCount = 3 + (int)(progress * 6);
            for (int i = 0; i < dustCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = 40f * (1f - progress * 0.4f) + Main.rand.NextFloat(15f);
                Vector2 dustPos = center + angle.ToRotationVector2() * radius;
                Vector2 toCenter = (center - dustPos).SafeNormalize(Vector2.Zero) * (2f + progress * 3f);
                Color col = Color.Lerp(DiesIraePalette.EmberOrange, DiesIraePalette.HellfireGold, progress);
                Dust d = Dust.NewDustPerfect(dustPos, DustID.Torch, toCenter, 0, col, 0.8f + progress * 0.4f);
                d.noGravity = true;
            }

            Lighting.AddLight(center, DiesIraePalette.EmberOrange.ToVector3() * (0.5f + progress * 0.3f));
        }

        /// <summary>
        /// Bell toll attack VFX — shockwave toll burst.
        /// </summary>
        public static void TollAttackVFX(Vector2 center)
        {
            if (Main.dedServ) return;

            DiesIraeVFXLibrary.ProjectileImpact(center, 1.4f);
            DiesIraeVFXLibrary.SpawnJudgmentRings(center, 3, 0.4f);
            DiesIraeVFXLibrary.SpawnHeavySmoke(center, 10, 0.9f, 4f, 50);
            DiesIraeVFXLibrary.SpawnBoneAshScatter(center, 5, 3f);
            DiesIraeVFXLibrary.SpawnMusicNotes(center, 6, 30f, 0.8f, 1.0f, 35);

            MagnumScreenEffects.AddScreenShake(4f);
            Lighting.AddLight(center, DiesIraePalette.HellfireGold.ToVector3() * 1.0f);
        }

        /// <summary>
        /// BellTollWave projectile trail VFX.
        /// </summary>
        public static void TollWaveTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 vel = -velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 1.5f)
                + Main.rand.NextVector2Circular(0.3f, 0.3f);
            Color col = Color.Lerp(DiesIraePalette.EmberOrange, DiesIraePalette.HellfireGold,
                Main.rand.NextFloat());
            Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 0.9f);
            d.noGravity = true;

            if (Main.rand.NextBool(5))
                DiesIraeVFXLibrary.SpawnMusicNotes(pos, 1, 6f, 0.5f, 0.7f, 20);

            Lighting.AddLight(pos, DiesIraePalette.EmberOrange.ToVector3() * 0.4f);
        }

        /// <summary>
        /// BellTollWave impact VFX.
        /// </summary>
        public static void TollWaveImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            DiesIraeVFXLibrary.ProjectileImpact(pos, 0.5f);
            DiesIraeVFXLibrary.SpawnBoneAshScatter(pos, 2, 1.5f);
        }
    }

    // =============================================================================
    //  HARMONY OF JUDGEMENT — VFX
    //  Identity: Angelic judge minion. Golden judgment rays, divine aureate glow,
    //  precise ecclesiastical judgment. Every ray is a sentence from the divine.
    // =============================================================================

    public static class HarmonyOfJudgementVFX
    {
        public static void HoldItemVFX(Player player, Item item)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;

            // Golden judgment shimmer
            if (Main.rand.NextBool(5))
            {
                Color shimmer = DiesIraePalette.GetJudgmentShimmer((float)Main.timeForVisualEffects);
                Dust d = Dust.NewDustPerfect(center + Main.rand.NextVector2Circular(18f, 18f),
                    DustID.Enchanted_Gold, Vector2.Zero, 0, shimmer, 0.5f);
                d.noGravity = true;
            }

            DiesIraeVFXLibrary.SpawnAmbientSmoke(center, 18f);

            if (Main.rand.NextBool(28))
                DiesIraeVFXLibrary.SpawnMusicNotes(center, 1, 18f, 0.65f, 0.85f, 30);

            Lighting.AddLight(center, DiesIraePalette.HellfireGold.ToVector3() * 0.2f);
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
        /// Summon VFX — angelic entrance.
        /// </summary>
        public static void SummonVFX(Vector2 spawnPos)
        {
            if (Main.dedServ) return;

            DiesIraeVFXLibrary.ProjectileImpact(spawnPos, 0.8f);
            DiesIraeVFXLibrary.SpawnMusicNotes(spawnPos, 5, 20f, 0.75f, 1.0f, 30);

            // Golden flare burst
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Dust d = Dust.NewDustPerfect(spawnPos, DustID.Enchanted_Gold, vel, 0,
                    DiesIraePalette.HellfireGold, 1.0f);
                d.noGravity = true;
            }

            Lighting.AddLight(spawnPos, DiesIraePalette.HellfireGold.ToVector3() * 0.8f);
        }

        /// <summary>
        /// Minion ambient aura — golden divine presence.
        /// </summary>
        public static void MinionAmbientVFX(Vector2 center)
        {
            if (Main.dedServ) return;

            if (Main.rand.NextBool(4))
            {
                Vector2 pos = center + Main.rand.NextVector2Circular(10f, 10f);
                Dust d = Dust.NewDustPerfect(pos, DustID.Enchanted_Gold,
                    new Vector2(0, -0.3f), 0, DiesIraePalette.HellfireGold, 0.5f);
                d.noGravity = true;
            }

            if (Main.rand.NextBool(20))
                DiesIraeVFXLibrary.SpawnMusicNotes(center, 1, 10f, 0.6f, 0.8f, 25);

            Lighting.AddLight(center, DiesIraePalette.HellfireGold.ToVector3() * 0.5f);
        }

        /// <summary>
        /// Judgment ray fire VFX — golden divine ray cast.
        /// </summary>
        public static void FireRayVFX(Vector2 origin, Vector2 direction)
        {
            if (Main.dedServ) return;

            DiesIraeVFXLibrary.DrawBloom(origin, 0.4f);
            DiesIraeVFXLibrary.SpawnMusicNotes(origin, 2, 12f, 0.7f, 0.9f, 20);

            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = direction * Main.rand.NextFloat(2f, 4f)
                    + Main.rand.NextVector2Circular(0.5f, 0.5f);
                Dust d = Dust.NewDustPerfect(origin, DustID.Enchanted_Gold, vel, 0,
                    DiesIraePalette.JudgmentGold, 1.0f);
                d.noGravity = true;
            }

            Lighting.AddLight(origin, DiesIraePalette.JudgmentGold.ToVector3() * 0.6f);
        }

        /// <summary>
        /// JudgementRay projectile trail VFX.
        /// </summary>
        public static void RayTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 vel = -velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 1.5f)
                + Main.rand.NextVector2Circular(0.2f, 0.2f);
            Color col = Color.Lerp(DiesIraePalette.HellfireGold, DiesIraePalette.JudgmentGold,
                Main.rand.NextFloat());
            Dust d = Dust.NewDustPerfect(pos, DustID.Enchanted_Gold, vel, 0, col, 0.8f);
            d.noGravity = true;

            // Orbiting golden sparks
            if (Main.rand.NextBool(3))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 orbPos = pos + angle.ToRotationVector2() * 8f;
                Dust orb = Dust.NewDustPerfect(orbPos, DustID.Enchanted_Gold, Vector2.Zero, 0,
                    DiesIraePalette.JudgmentGold, 0.5f);
                orb.noGravity = true;
            }

            if (Main.rand.NextBool(6))
                DiesIraeVFXLibrary.SpawnMusicNotes(pos, 1, 6f, 0.5f, 0.7f, 18);

            Lighting.AddLight(pos, DiesIraePalette.HellfireGold.ToVector3() * 0.45f);
        }

        /// <summary>
        /// JudgementRay impact VFX.
        /// </summary>
        public static void RayImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            DiesIraeVFXLibrary.ProjectileImpact(pos, 0.5f);

            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(1.5f, 3f);
                Dust d = Dust.NewDustPerfect(pos, DustID.Enchanted_Gold, vel, 0,
                    DiesIraePalette.JudgmentGold, 0.8f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// JudgementRay kill flare — gold flare burst on enemy kill.
        /// </summary>
        public static void RayKillFlareVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            DiesIraeVFXLibrary.DrawBloom(pos, 0.5f);

            for (int i = 0; i < 15; i++)
            {
                float angle = MathHelper.TwoPi * i / 15f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Color col = DiesIraePalette.GetJudgmentGradient((float)i / 15f);
                Dust d = Dust.NewDustPerfect(pos, DustID.Enchanted_Gold, vel, 0, col, 1.0f);
                d.noGravity = true;
            }

            DiesIraeVFXLibrary.SpawnMusicNotes(pos, 3, 15f, 0.8f, 1.0f, 25);
            Lighting.AddLight(pos, DiesIraePalette.JudgmentGold.ToVector3() * 0.8f);
        }
    }

    // =============================================================================
    //  WRATHFUL CONTRACT — VFX
    //  Identity: Contracted wrath demon minion. Brutal dash attacks, combo-triggered
    //  fire bursts, ominous blood-red presence. Every strike fulfills the contract.
    // =============================================================================

    public static class WrathfulContractVFX
    {
        public static void HoldItemVFX(Player player, Item item)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;

            // Ominous blood-red flame particles
            if (Main.rand.NextBool(5))
            {
                Vector2 pos = center + Main.rand.NextVector2Circular(20f, 20f);
                Color col = Color.Lerp(DiesIraePalette.BloodRed, DiesIraePalette.EmberOrange,
                    Main.rand.NextFloat(0.3f));
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, new Vector2(0, -0.4f), 0, col, 0.7f);
                d.noGravity = true;
            }

            DiesIraeVFXLibrary.SpawnAmbientSmoke(center, 22f);

            if (Main.rand.NextBool(25))
                DiesIraeVFXLibrary.SpawnMusicNotes(center, 1, 18f, 0.65f, 0.85f, 30);

            Lighting.AddLight(center, DiesIraePalette.BloodRed.ToVector3() * 0.25f);
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
        /// Summon VFX — grand demonic entrance.
        /// </summary>
        public static void SummonVFX(Vector2 spawnPos)
        {
            if (Main.dedServ) return;

            DiesIraeVFXLibrary.ProjectileImpact(spawnPos, 1.5f);
            DiesIraeVFXLibrary.SpawnMusicNotes(spawnPos, 8, 30f, 0.8f, 1.0f, 35);

            // Alternating blood-red / ember-orange flare ring
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color col = i % 2 == 0 ? DiesIraePalette.BloodRed : DiesIraePalette.EmberOrange;
                Dust d = Dust.NewDustPerfect(spawnPos, DustID.Torch, vel, 0, col, 1.3f);
                d.noGravity = true;
            }

            DiesIraeVFXLibrary.SpawnHeavySmoke(spawnPos, 12, 1.0f, 4f, 60);
            MagnumScreenEffects.AddScreenShake(6f);
            Lighting.AddLight(spawnPos, DiesIraePalette.BloodRed.ToVector3() * 1.0f);
        }

        /// <summary>
        /// Minion ambient aura — ominous wrath presence.
        /// </summary>
        public static void MinionAmbientVFX(Vector2 center)
        {
            if (Main.dedServ) return;

            // Heavy ominous flame particles
            if (Main.rand.NextBool(3))
            {
                Vector2 pos = center + Main.rand.NextVector2Circular(15f, 15f);
                Color col = Color.Lerp(DiesIraePalette.BloodRed, DiesIraePalette.EmberOrange,
                    Main.rand.NextFloat(0.4f));
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch,
                    Main.rand.NextVector2Circular(0.5f, 0.5f), 0, col, 0.8f);
                d.noGravity = true;
            }

            DiesIraeVFXLibrary.SpawnAmbientSmoke(center, 18f);

            if (Main.rand.NextBool(6))
                DiesIraeVFXLibrary.SpawnHeavySmoke(center, 1, 0.3f, 1f, 30);

            if (Main.rand.NextBool(20))
                DiesIraeVFXLibrary.SpawnMusicNotes(center, 1, 12f, 0.6f, 0.8f, 25);

            Lighting.AddLight(center, DiesIraePalette.BloodRed.ToVector3() * 0.6f);
        }

        /// <summary>
        /// Dash attack trail VFX — brutal fire trail during dash.
        /// </summary>
        public static void DashTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = -velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 3f)
                    + Main.rand.NextVector2Circular(0.5f, 0.5f);
                Color col = DiesIraePalette.GetWrathGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(6f, 6f),
                    DustID.Torch, vel, 0, col, 1.2f);
                d.noGravity = true;
            }

            DiesIraeVFXLibrary.SpawnHeavySmoke(pos, 1, 0.4f, 1.5f, 25);
            Lighting.AddLight(pos, DiesIraePalette.BloodRed.ToVector3() * 0.5f);
        }

        /// <summary>
        /// Combo explosion VFX — triggers on every 3rd combo hit.
        /// </summary>
        public static void ComboExplosionVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            DiesIraeVFXLibrary.ProjectileImpact(pos, 1.0f);
            DiesIraeVFXLibrary.SpawnWrathPulseRings(pos, 3, 0.3f);
            DiesIraeVFXLibrary.SpawnMusicNotes(pos, 4, 20f, 0.8f, 1.0f, 30);

            MagnumScreenEffects.AddScreenShake(3f);
            Lighting.AddLight(pos, DiesIraePalette.EmberOrange.ToVector3() * 0.8f);
        }

        /// <summary>
        /// Minion hit impact VFX.
        /// </summary>
        public static void HitImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            DiesIraeVFXLibrary.MeleeImpact(pos, 0);
            DiesIraeVFXLibrary.SpawnMusicNotes(pos, 4, 15f, 0.7f, 0.9f, 25);
        }

        /// <summary>
        /// WrathFireball projectile trail VFX.
        /// </summary>
        public static void FireballTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 vel = -velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 2.5f)
                + Main.rand.NextVector2Circular(0.3f, 0.3f);
            Color col = Color.Lerp(DiesIraePalette.EmberOrange, DiesIraePalette.HellfireGold,
                Main.rand.NextFloat());
            Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.0f);
            d.noGravity = true;

            // Orbiting ember sparks
            if (Main.rand.NextBool(3))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 orbPos = pos + angle.ToRotationVector2() * 10f;
                Dust orb = Dust.NewDustPerfect(orbPos, DustID.Torch, Vector2.Zero, 0,
                    DiesIraePalette.EmberOrange, 0.6f);
                orb.noGravity = true;
            }

            if (Main.rand.NextBool(5))
                DiesIraeVFXLibrary.SpawnMusicNotes(pos, 1, 6f, 0.5f, 0.7f, 18);

            Lighting.AddLight(pos, DiesIraePalette.EmberOrange.ToVector3() * 0.5f);
        }

        /// <summary>
        /// WrathFireball impact VFX.
        /// </summary>
        public static void FireballImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            DiesIraeVFXLibrary.ProjectileImpact(pos, 0.5f);
            DiesIraeVFXLibrary.SpawnEmberScatter(pos, 4, 2.5f);
        }

        /// <summary>
        /// WrathFireball kill VFX — wrath burst on enemy kill.
        /// </summary>
        public static void FireballKillVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            DiesIraeVFXLibrary.ProjectileImpact(pos, 0.4f);
            DiesIraeVFXLibrary.SpawnEmberScatter(pos, 3, 2f);
        }
    }
}
