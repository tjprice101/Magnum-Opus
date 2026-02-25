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
    //  WRATH'S CLEAVER — VFX
    //  Identity: Blood-soaked fury, wrath waves, homing crystallized flames.
    //  The executioner's blade. Raw, brutal, escalating.
    // =============================================================================

    /// <summary>
    /// VFX helper for the Wrath's Cleaver melee weapon.
    /// Handles hold-item ambient, world item bloom, swing frame VFX,
    /// combo impacts, wrath wave projectile, and crystallized flame specials.
    /// </summary>
    public static class WrathsCleaverVFX
    {
        public static void HoldItemVFX(Player player, Item item)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // 3-point orbiting blood embers
            for (int i = 0; i < 3; i++)
            {
                float angle = time * 0.04f + MathHelper.TwoPi * i / 3f;
                float radius = 18f + MathF.Sin(time * 0.06f + i) * 4f;
                Vector2 motePos = center + angle.ToRotationVector2() * radius;

                if (Main.rand.NextBool(3))
                {
                    Color col = DiesIraePalette.PaletteLerp(DiesIraePalette.WrathsCleaverBlade,
                        0.3f + (float)i / 3f * 0.5f);
                    Dust d = Dust.NewDustPerfect(motePos, DustID.Torch, Vector2.Zero, 0, col, 0.8f);
                    d.noGravity = true;
                    d.fadeIn = 0.6f;
                }
            }

            // Bone ash drift
            if (Main.rand.NextBool(8))
                DiesIraeVFXLibrary.SpawnBoneAshScatter(center, 1, 0.8f);

            DiesIraeVFXLibrary.SpawnAmbientSmoke(center, 25f);

            if (Main.rand.NextBool(25))
                DiesIraeVFXLibrary.SpawnMusicNotes(center, 1, 20f, 0.7f, 0.9f, 30);

            float pulse = 0.5f + MathF.Sin(time * 0.05f) * 0.15f;
            Lighting.AddLight(center, DiesIraePalette.InfernalRed.ToVector3() * pulse);
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

        public static void SwingFrameVFX(Vector2 tipPos, Vector2 swordDirection, int comboStep, int timer)
        {
            if (Main.dedServ) return;

            DiesIraeVFXLibrary.SpawnSwingDust(tipPos, -swordDirection, DustID.Torch);
            DiesIraeVFXLibrary.SpawnContrastSparkle(tipPos, -swordDirection);

            if (timer % (3 - Math.Min(comboStep, 2)) == 0)
                DiesIraeVFXLibrary.SpawnEmberScatter(tipPos, 2 + comboStep, 2f);

            if (timer % 4 == 0)
                DiesIraeVFXLibrary.SpawnBoneAshScatter(tipPos, 1, 1.5f);

            if (timer % (5 - comboStep) == 0)
                DiesIraeVFXLibrary.SpawnHeavySmoke(tipPos, 1 + comboStep / 2, 0.4f, 1.5f, 30);

            if (timer % 5 == 0)
                DiesIraeVFXLibrary.SpawnMusicNotes(tipPos, 1, 10f, 0.7f, 0.9f, 25);

            Color lightCol = DiesIraePalette.PaletteLerp(DiesIraePalette.WrathsCleaverBlade,
                0.4f + comboStep * 0.15f);
            Lighting.AddLight(tipPos, lightCol.ToVector3() * 0.6f);
        }

        public static void ComboImpact(Vector2 pos, int comboStep)
        {
            if (Main.dedServ) return;

            DiesIraeVFXLibrary.MeleeImpact(pos, comboStep);

            if (comboStep >= 1)
                CustomParticles.DiesIraeHellfireBurst(pos, 8 + comboStep * 4);

            if (comboStep >= 2)
            {
                DiesIraeVFXLibrary.DrawBloom(pos, 0.7f);
                MagnumScreenEffects.AddScreenShake(4f);
                DiesIraeVFXLibrary.SpawnHeavySmoke(pos, 4, 0.8f, 3f, 50);
                DiesIraeVFXLibrary.SpawnBoneAshScatter(pos, 4, 3f);
            }
        }

        /// <summary>
        /// Wrath wave projectile trail VFX — blood-red fire arc.
        /// </summary>
        public static void WrathWaveTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 vel = -velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 2.5f)
                + Main.rand.NextVector2Circular(0.4f, 0.4f);
            Color col = DiesIraePalette.GetWrathGradient(Main.rand.NextFloat());
            Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.2f);
            d.noGravity = true;

            Lighting.AddLight(pos, DiesIraePalette.InfernalRed.ToVector3() * 0.5f);
        }

        /// <summary>
        /// Wrath wave impact VFX.
        /// </summary>
        public static void WrathWaveImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;
            DiesIraeVFXLibrary.ProjectileImpact(pos, 0.6f);
        }

        /// <summary>
        /// Crystallized flame spawn VFX — homing flames on 3rd swing.
        /// </summary>
        public static void CrystallizedFlameSpawnVFX(Vector2 pos)
        {
            if (Main.dedServ) return;
            DiesIraeVFXLibrary.SpawnEmberScatter(pos, 4, 3f);
            DiesIraeVFXLibrary.DrawBloom(pos, 0.3f);
            Lighting.AddLight(pos, DiesIraePalette.HellfireGold.ToVector3() * 0.6f);
        }

        /// <summary>
        /// Crystallized flame trail VFX — homing golden fire.
        /// </summary>
        public static void CrystallizedFlameTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 vel = -velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 1.5f);
            Color col = Color.Lerp(DiesIraePalette.HellfireGold, DiesIraePalette.WrathWhite, Main.rand.NextFloat(0.3f));
            Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 0.9f);
            d.noGravity = true;

            Lighting.AddLight(pos, DiesIraePalette.HellfireGold.ToVector3() * 0.4f);
        }

        /// <summary>
        /// Crystallized flame impact VFX.
        /// </summary>
        public static void CrystallizedFlameImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;
            DiesIraeVFXLibrary.ProjectileImpact(pos, 0.4f);
        }
    }

    // =============================================================================
    //  CHAIN OF JUDGMENT — VFX
    //  Identity: Shackled fire, judgment links, ricochet chains, explosion on hit.
    //  Methodical, binding, inescapable.
    // =============================================================================

    /// <summary>
    /// VFX helper for the Chain of Judgment whip weapon.
    /// Handles whip trail, ricochet chain VFX, and on-hit explosions.
    /// </summary>
    public static class ChainOfJudgmentVFX
    {
        public static void HoldItemVFX(Player player, Item item)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Chain link orbit (2 golden links)
            for (int i = 0; i < 2; i++)
            {
                float angle = time * 0.05f + MathHelper.Pi * i;
                float radius = 20f + MathF.Sin(time * 0.07f) * 3f;
                Vector2 linkPos = center + angle.ToRotationVector2() * radius;

                if (Main.rand.NextBool(4))
                {
                    Dust d = Dust.NewDustPerfect(linkPos, DustID.Enchanted_Gold,
                        Vector2.Zero, 0, DiesIraePalette.JudgmentGold * 0.8f, 0.6f);
                    d.noGravity = true;
                }
            }

            DiesIraeVFXLibrary.SpawnAmbientSmoke(center, 20f);

            if (Main.rand.NextBool(30))
                DiesIraeVFXLibrary.SpawnMusicNotes(center, 1, 18f, 0.65f, 0.85f, 30);

            float pulse = 0.4f + MathF.Sin(time * 0.04f) * 0.1f;
            Lighting.AddLight(center, DiesIraePalette.JudgmentGold.ToVector3() * pulse);
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
        /// Whip swing trail VFX — chain links trailing with judgment sparks.
        /// </summary>
        public static void WhipTrailVFX(Vector2 tipPos, Vector2 direction, int timer)
        {
            if (Main.dedServ) return;

            DiesIraeVFXLibrary.SpawnSwingDust(tipPos, -direction, DustID.Torch);

            if (timer % 3 == 0)
            {
                Color col = DiesIraePalette.GetGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(tipPos, DustID.Enchanted_Gold,
                    Main.rand.NextVector2Circular(1.5f, 1.5f), 0, col, 0.8f);
                d.noGravity = true;
            }

            if (timer % 6 == 0)
                DiesIraeVFXLibrary.SpawnMusicNotes(tipPos, 1, 8f, 0.7f, 0.85f, 20);

            Lighting.AddLight(tipPos, DiesIraePalette.JudgmentGold.ToVector3() * 0.5f);
        }

        /// <summary>
        /// Whip hit impact VFX.
        /// </summary>
        public static void WhipHitImpact(Vector2 pos)
        {
            if (Main.dedServ) return;

            DiesIraeVFXLibrary.MeleeImpact(pos, 1);
            DiesIraeVFXLibrary.SpawnBoneAshScatter(pos, 3, 2f);
        }

        /// <summary>
        /// Ricochet chain VFX — fires between enemies.
        /// </summary>
        public static void RicochetChainVFX(Vector2 from, Vector2 to)
        {
            if (Main.dedServ) return;

            Vector2 dir = (to - from).SafeNormalize(Vector2.UnitX);
            float dist = Vector2.Distance(from, to);
            int segments = (int)(dist / 20f);

            for (int i = 0; i < segments; i++)
            {
                float t = (float)i / segments;
                Vector2 pos = Vector2.Lerp(from, to, t);
                Color col = DiesIraePalette.GetJudgmentGradient(t);
                Dust d = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(4f, 4f),
                    DustID.Torch, dir * 0.5f, 0, col, 1.0f);
                d.noGravity = true;
            }

            DiesIraeVFXLibrary.DrawBloom(to, 0.4f);
            Lighting.AddLight(to, DiesIraePalette.JudgmentGold.ToVector3() * 0.6f);
        }

        /// <summary>
        /// Chain explosion VFX — on hit after ricochets.
        /// </summary>
        public static void ChainExplosionVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            DiesIraeVFXLibrary.WrathShockwaveImpact(pos, 0.7f);
            DiesIraeVFXLibrary.SpawnHeavySmoke(pos, 4, 0.7f, 3f, 45);
            MagnumScreenEffects.AddScreenShake(3f);
        }
    }

    // =============================================================================
    //  EXECUTIONER'S VERDICT — VFX
    //  Identity: Guillotine descent, final judgment, execution flash below 15% HP.
    //  Absolute, irreversible, the blade falls.
    // =============================================================================

    /// <summary>
    /// VFX helper for the Executioner's Verdict weapon.
    /// Handles guillotine descent, ignited bolt spawns, and execution threshold VFX.
    /// </summary>
    public static class ExecutionersVerdictVFX
    {
        public static void HoldItemVFX(Player player, Item item)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Dark blood aura — ominous, heavy
            if (Main.rand.NextBool(4))
            {
                Vector2 pos = center + Main.rand.NextVector2Circular(22f, 22f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.2f, 0.2f), -0.6f - Main.rand.NextFloat(0.3f));
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, DiesIraePalette.DarkBlood, 0.9f);
                d.noGravity = true;
            }

            DiesIraeVFXLibrary.SpawnAmbientSmoke(center, 28f);

            if (Main.rand.NextBool(30))
                DiesIraeVFXLibrary.SpawnMusicNotes(center, 1, 22f, 0.65f, 0.85f, 35);

            float pulse = 0.4f + MathF.Sin(time * 0.03f) * 0.1f;
            Lighting.AddLight(center, DiesIraePalette.BloodRed.ToVector3() * pulse);
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

        public static void SwingFrameVFX(Vector2 tipPos, Vector2 swordDirection, int comboStep, int timer)
        {
            if (Main.dedServ) return;

            // Heavier, darker fire dust
            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = -swordDirection * Main.rand.NextFloat(1f, 4f) + Main.rand.NextVector2Circular(0.5f, 0.5f);
                Color col = DiesIraePalette.GetFireGradient(Main.rand.NextFloat(0f, 0.6f));
                Dust d = Dust.NewDustPerfect(tipPos, DustID.Torch, vel, 0, col, 1.6f);
                d.noGravity = true;
            }

            if (timer % 3 == 0)
                DiesIraeVFXLibrary.SpawnBoneAshScatter(tipPos, 2, 2f);

            if (timer % 4 == 0)
                DiesIraeVFXLibrary.SpawnHeavySmoke(tipPos, 2, 0.6f, 2f, 35);

            if (timer % 5 == 0)
                DiesIraeVFXLibrary.SpawnMusicNotes(tipPos, 1, 10f, 0.7f, 0.9f, 25);

            Lighting.AddLight(tipPos, DiesIraePalette.InfernalRed.ToVector3() * 0.7f);
        }

        public static void ComboImpact(Vector2 pos, int comboStep)
        {
            if (Main.dedServ) return;

            DiesIraeVFXLibrary.MeleeImpact(pos, comboStep);
            DiesIraeVFXLibrary.SpawnBoneAshScatter(pos, 3 + comboStep * 2, 3f);

            if (comboStep >= 1)
            {
                CustomParticles.DiesIraeHellfireBurst(pos, 10 + comboStep * 3);
                MagnumScreenEffects.AddScreenShake(3f + comboStep * 2f);
            }
        }

        /// <summary>
        /// Ignited bolt spawn VFX — 3 bolts that fly outward.
        /// </summary>
        public static void IgnitedBoltSpawnVFX(Vector2 pos, Vector2 direction)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = direction * Main.rand.NextFloat(2f, 4f) + Main.rand.NextVector2Circular(1f, 1f);
                Color col = DiesIraePalette.GetFireGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.3f);
                d.noGravity = true;
            }

            Lighting.AddLight(pos, DiesIraePalette.HellfireGold.ToVector3() * 0.5f);
        }

        /// <summary>
        /// Execution VFX — massive judgment flash when killing below 15% HP.
        /// </summary>
        public static void ExecutionVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            DiesIraeVFXLibrary.HellfireEruption(pos, 1.5f);

            // 24-point radial verdict burst
            for (int i = 0; i < 24; i++)
            {
                float angle = MathHelper.TwoPi * i / 24f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 10f);
                Color col = DiesIraePalette.GetJudgmentGradient((float)i / 24f);
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.8f);
                d.noGravity = true;
            }

            DiesIraeVFXLibrary.SpawnBoneAshScatter(pos, 10, 4f);

            MagnumScreenEffects.AddScreenShake(10f);
            Lighting.AddLight(pos, DiesIraePalette.WrathWhite.ToVector3() * 2.5f);
        }
    }
}
