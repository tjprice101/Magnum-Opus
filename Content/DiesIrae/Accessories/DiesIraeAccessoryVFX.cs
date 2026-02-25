using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.DiesIrae.Accessories
{
    // =============================================================================
    //  EMBER OF THE CONDEMNED (Magic Accessory)
    //  Identity: Condemned fire aura, hellfire proc on magic attacks.
    //  Ecclesiastical dread, smoldering judgment writ.
    // =============================================================================

    /// <summary>
    /// VFX for Ember of the Condemned — magic damage accessory.
    /// Hellfire enchantment aura, fire proc, condemned hit VFX.
    /// </summary>
    public static class EmberOfTheCondemnedVFX
    {
        private static readonly Color CondemnedCore = DiesIraePalette.InfernalRed;
        private static readonly Color CondemnedAccent = DiesIraePalette.EmberOrange;
        private static readonly Color CondemnedFlash = DiesIraePalette.HellfireGold;

        /// <summary>
        /// Ambient aura — smoldering condemnation aura around the player.
        /// Converging condemned embers, rising ash, smoke wisps, judgment shimmer.
        /// </summary>
        public static void AmbientAura(Vector2 playerCenter, float timer)
        {
            // Converging condemned fire motes
            if ((int)timer % 6 == 0)
            {
                float baseAngle = timer * 0.025f;
                for (int i = 0; i < 3; i++)
                {
                    float angle = baseAngle + MathHelper.TwoPi * i / 3f;
                    float radius = 28f + MathF.Sin(timer * 0.04f + i * 1.5f) * 5f;
                    Vector2 motePos = playerCenter + angle.ToRotationVector2() * radius;
                    Vector2 toCenter = (playerCenter - motePos).SafeNormalize(Vector2.Zero) * 0.7f;
                    Color col = Color.Lerp(CondemnedCore, CondemnedAccent, (float)i / 3f);
                    Dust d = Dust.NewDustPerfect(motePos, DustID.Torch, toCenter, 0, col, 0.8f);
                    d.noGravity = true;
                    d.fadeIn = 0.5f;
                }
            }

            // Rising condemned embers
            if ((int)timer % 8 == 0)
            {
                Vector2 pos = playerCenter + Main.rand.NextVector2Circular(18f, 18f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -0.7f - Main.rand.NextFloat(0.5f));
                Color col = DiesIraePalette.GetFireGradient(Main.rand.NextFloat(0.3f, 0.7f));
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 0.7f);
                d.noGravity = true;
            }

            DiesIraeVFXLibrary.SpawnAmbientSmoke(playerCenter, 22f);

            // Judgment shimmer
            if ((int)timer % 15 == 0)
            {
                Color shimmer = DiesIraePalette.GetShimmer(timer);
                Dust d = Dust.NewDustPerfect(playerCenter + Main.rand.NextVector2Circular(16f, 16f),
                    DustID.Enchanted_Gold, Vector2.Zero, 0, shimmer, 0.4f);
                d.noGravity = true;
            }

            if ((int)timer % 30 == 0)
                DiesIraeVFXLibrary.SpawnMusicNotes(playerCenter, 1, 22f, 0.65f, 0.85f, 28);

            float pulse = 0.25f + MathF.Sin(timer * 0.04f) * 0.07f;
            Lighting.AddLight(playerCenter, CondemnedCore.ToVector3() * pulse);
        }

        /// <summary>
        /// Hellfire proc VFX — fire burst when magic attack inflicts hellfire debuff.
        /// </summary>
        public static void HellfireProcVFX(Vector2 enemyCenter)
        {
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(1.5f, 3f);
                Color col = DiesIraePalette.GetWrathGradient((float)i / 6f);
                Dust d = Dust.NewDustPerfect(enemyCenter, DustID.Torch, vel, 0, col, 1.1f);
                d.noGravity = true;
            }

            DiesIraeVFXLibrary.SpawnEmberScatter(enemyCenter, 3, 2f);
            Lighting.AddLight(enemyCenter, CondemnedFlash.ToVector3() * 0.5f);
        }

        /// <summary>
        /// On-hit accent VFX.
        /// </summary>
        public static void HitVFX(Vector2 hitPos)
        {
            DiesIraeVFXLibrary.SpawnEmberScatter(hitPos, 2, 1.5f);

            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustPerfect(hitPos, DustID.Enchanted_Gold,
                    Main.rand.NextVector2Circular(1f, 1f), 0, DiesIraePalette.JudgmentGold, 0.6f);
                d.noGravity = true;
            }
        }
    }

    // =============================================================================
    //  SEAL OF DAMNATION (Summoner Accessory)
    //  Identity: Damned seal aura, damnation mark on minion attacks.
    //  Solemn, binding, the contract of the condemned.
    // =============================================================================

    /// <summary>
    /// VFX for Seal of Damnation — summoner damage accessory.
    /// Damnation seal aura, daybreak proc, sealed hit VFX.
    /// </summary>
    public static class SealOfDamnationVFX
    {
        private static readonly Color SealCore = DiesIraePalette.BloodRed;
        private static readonly Color SealAccent = DiesIraePalette.DoomPurple;
        private static readonly Color SealFlash = DiesIraePalette.JudgmentGold;

        /// <summary>
        /// Ambient aura — damnation seal orbiting the player.
        /// Dark pulsing rune motes, doom purple wisps, blood-red shimmer.
        /// </summary>
        public static void AmbientAura(Vector2 playerCenter, float timer)
        {
            // Orbiting doom seal motes
            if ((int)timer % 5 == 0)
            {
                float baseAngle = timer * 0.02f;
                for (int i = 0; i < 4; i++)
                {
                    float angle = baseAngle + MathHelper.TwoPi * i / 4f;
                    float radius = 32f + MathF.Sin(timer * 0.035f + i * 1.2f) * 6f;
                    Vector2 motePos = playerCenter + angle.ToRotationVector2() * radius;
                    Color col = Color.Lerp(SealCore, SealAccent, (float)i / 4f);
                    Dust d = Dust.NewDustPerfect(motePos, DustID.Torch,
                        angle.ToRotationVector2() * 0.2f, 0, col, 0.7f);
                    d.noGravity = true;
                }
            }

            // Doom purple wisps
            if ((int)timer % 10 == 0)
            {
                Vector2 pos = playerCenter + Main.rand.NextVector2Circular(20f, 20f);
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch,
                    new Vector2(0, -0.4f), 0, DiesIraePalette.DoomPurple, 0.6f);
                d.noGravity = true;
            }

            DiesIraeVFXLibrary.SpawnAmbientSmoke(playerCenter, 24f);

            // Blood-red shimmer
            if ((int)timer % 18 == 0)
            {
                Color shimmer = DiesIraePalette.GetShimmer(timer);
                Dust d = Dust.NewDustPerfect(playerCenter + Main.rand.NextVector2Circular(18f, 18f),
                    DustID.Enchanted_Gold, Vector2.Zero, 0, shimmer, 0.4f);
                d.noGravity = true;
            }

            if ((int)timer % 35 == 0)
                DiesIraeVFXLibrary.SpawnMusicNotes(playerCenter, 1, 24f, 0.6f, 0.8f, 28);

            float pulse = 0.2f + MathF.Sin(timer * 0.035f) * 0.06f;
            Lighting.AddLight(playerCenter, SealCore.ToVector3() * pulse);
        }

        /// <summary>
        /// Daybreak proc VFX — damnation mark applied to enemy.
        /// </summary>
        public static void DamnationProcVFX(Vector2 enemyCenter)
        {
            // Doom purple/blood-red damnation ring
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(1f, 2.5f);
                Color col = i % 2 == 0 ? DiesIraePalette.BloodRed : DiesIraePalette.DoomPurple;
                Dust d = Dust.NewDustPerfect(enemyCenter, DustID.Torch, vel, 0, col, 1.0f);
                d.noGravity = true;
            }

            DiesIraeVFXLibrary.SpawnBoneAshScatter(enemyCenter, 2, 1.5f);
            Lighting.AddLight(enemyCenter, SealFlash.ToVector3() * 0.4f);
        }

        /// <summary>
        /// On-hit accent VFX.
        /// </summary>
        public static void HitVFX(Vector2 hitPos)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                Color col = Color.Lerp(SealCore, SealAccent, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(hitPos, DustID.Torch, vel, 0, col, 0.8f);
                d.noGravity = true;
            }
        }
    }

    // =============================================================================
    //  CHAIN OF FINAL JUDGMENT (Melee Accessory)
    //  Identity: Judgment chains binding the player, lifesteal with golden healing
    //  flash, execute mechanic with wrath burst. The chain of absolute sentencing.
    // =============================================================================

    /// <summary>
    /// VFX for Chain of Final Judgment — melee damage accessory.
    /// Judgment chain aura, lifesteal VFX, execute burst VFX.
    /// </summary>
    public static class ChainOfFinalJudgmentVFX
    {
        private static readonly Color ChainCore = DiesIraePalette.JudgmentGold;
        private static readonly Color ChainAccent = DiesIraePalette.InfernalRed;
        private static readonly Color ChainFlash = DiesIraePalette.WrathWhite;

        /// <summary>
        /// Ambient aura — judgment chains orbiting the player.
        /// Golden chain links, judgment shimmer, infernal red trailing dust.
        /// </summary>
        public static void AmbientAura(Vector2 playerCenter, float timer)
        {
            // Orbiting judgment chain links
            if ((int)timer % 5 == 0)
            {
                float baseAngle = timer * 0.03f;
                for (int i = 0; i < 4; i++)
                {
                    float angle = baseAngle + MathHelper.TwoPi * i / 4f;
                    float radius = 25f + MathF.Sin(timer * 0.05f + i * 1.3f) * 4f;
                    Vector2 motePos = playerCenter + angle.ToRotationVector2() * radius;
                    Color col = Color.Lerp(ChainAccent, ChainCore, (float)i / 4f);
                    Dust d = Dust.NewDustPerfect(motePos, DustID.Enchanted_Gold,
                        angle.ToRotationVector2() * 0.3f, 0, col, 0.7f);
                    d.noGravity = true;
                }
            }

            // Infernal red trailing wisps
            if ((int)timer % 8 == 0)
            {
                Vector2 pos = playerCenter + Main.rand.NextVector2Circular(18f, 18f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.2f, 0.2f), -0.6f);
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, ChainAccent, 0.6f);
                d.noGravity = true;
            }

            DiesIraeVFXLibrary.SpawnAmbientSmoke(playerCenter, 20f);

            // Judgment gold shimmer
            if ((int)timer % 12 == 0)
            {
                Color shimmer = DiesIraePalette.GetJudgmentShimmer(timer);
                Dust d = Dust.NewDustPerfect(playerCenter + Main.rand.NextVector2Circular(16f, 16f),
                    DustID.Enchanted_Gold, Vector2.Zero, 0, shimmer, 0.5f);
                d.noGravity = true;
            }

            if ((int)timer % 25 == 0)
                DiesIraeVFXLibrary.SpawnMusicNotes(playerCenter, 1, 20f, 0.7f, 0.9f, 30);

            float pulse = 0.3f + MathF.Sin(timer * 0.04f) * 0.08f;
            Lighting.AddLight(playerCenter, ChainCore.ToVector3() * pulse);
        }

        /// <summary>
        /// Lifesteal VFX — golden healing flash when melee attack heals.
        /// </summary>
        public static void LifestealVFX(Vector2 playerCenter, Vector2 enemyCenter)
        {
            // Golden healing particles streaming from enemy to player
            Vector2 dir = (playerCenter - enemyCenter).SafeNormalize(Vector2.UnitX);
            float dist = Vector2.Distance(playerCenter, enemyCenter);
            int segments = Math.Max(3, (int)(dist / 20f));

            for (int i = 0; i < segments; i++)
            {
                float t = (float)i / segments;
                Vector2 pos = Vector2.Lerp(enemyCenter, playerCenter, t)
                    + Main.rand.NextVector2Circular(4f, 4f);
                Vector2 vel = dir * 2f;
                Color col = DiesIraePalette.GetJudgmentGradient(t);
                Dust d = Dust.NewDustPerfect(pos, DustID.Enchanted_Gold, vel, 0, col, 0.7f);
                d.noGravity = true;
            }

            // Healing flash at player
            DiesIraeVFXLibrary.DrawBloom(playerCenter, 0.3f);
            Lighting.AddLight(playerCenter, ChainCore.ToVector3() * 0.5f);
        }

        /// <summary>
        /// Execute VFX — wrath burst when enemy is executed below threshold.
        /// </summary>
        public static void ExecuteVFX(Vector2 enemyCenter)
        {
            DiesIraeVFXLibrary.WrathShockwaveImpact(enemyCenter, 0.8f);

            // Judgment gold execution ring
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color col = DiesIraePalette.GetJudgmentGradient((float)i / 12f);
                Dust d = Dust.NewDustPerfect(enemyCenter, DustID.Torch, vel, 0, col, 1.3f);
                d.noGravity = true;
            }

            DiesIraeVFXLibrary.SpawnMusicNotes(enemyCenter, 4, 20f, 0.8f, 1.1f, 30);
            MagnumScreenEffects.AddScreenShake(3f);
            Lighting.AddLight(enemyCenter, ChainFlash.ToVector3() * 1.0f);
        }

        /// <summary>
        /// On-hit accent VFX.
        /// </summary>
        public static void HitVFX(Vector2 hitPos)
        {
            DiesIraeVFXLibrary.SpawnEmberScatter(hitPos, 2, 1.5f);
            DiesIraeVFXLibrary.SpawnContrastSparkle(hitPos, Main.rand.NextVector2Circular(1f, 1f));
        }
    }

    // =============================================================================
    //  REQUIEM'S SHACKLE (Ranger Accessory)
    //  Identity: Cursed shackle aura, wrath mark on ranged attacks, bone ash
    //  atmosphere. The shackle that binds all targets for divine retribution.
    // =============================================================================

    /// <summary>
    /// VFX for Requiem's Shackle — ranger damage accessory.
    /// Wrath shackle aura, mark proc VFX, on-hit accent.
    /// </summary>
    public static class RequiemsShackleVFX
    {
        private static readonly Color ShackleCore = DiesIraePalette.InfernalRed;
        private static readonly Color ShackleAccent = DiesIraePalette.BoneWhite;
        private static readonly Color ShackleFlash = DiesIraePalette.JudgmentGold;

        /// <summary>
        /// Ambient aura — wrath shackle presence around the player.
        /// Bone-white chain fragments, infernal red glow, parchment wisps.
        /// </summary>
        public static void AmbientAura(Vector2 playerCenter, float timer)
        {
            // Orbiting shackle fragments (bone-white)
            if ((int)timer % 6 == 0)
            {
                float baseAngle = timer * 0.025f;
                for (int i = 0; i < 3; i++)
                {
                    float angle = baseAngle + MathHelper.TwoPi * i / 3f;
                    float radius = 26f + MathF.Sin(timer * 0.04f + i * 1.5f) * 5f;
                    Vector2 motePos = playerCenter + angle.ToRotationVector2() * radius;
                    Color col = DiesIraePalette.GetBoneGradient(0.5f + (float)i / 6f);
                    Dust d = Dust.NewDustPerfect(motePos, DustID.TintableDust,
                        angle.ToRotationVector2() * 0.2f, 150, col, 0.7f);
                    d.noGravity = true;
                }
            }

            // Infernal red glow wisps
            if ((int)timer % 9 == 0)
            {
                Vector2 pos = playerCenter + Main.rand.NextVector2Circular(18f, 18f);
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch,
                    new Vector2(0, -0.5f), 0, ShackleCore, 0.6f);
                d.noGravity = true;
            }

            DiesIraeVFXLibrary.SpawnAmbientSmoke(playerCenter, 22f);

            // Bone ash drift
            if ((int)timer % 20 == 0)
                DiesIraeVFXLibrary.SpawnBoneAshScatter(playerCenter, 1, 0.5f);

            if ((int)timer % 30 == 0)
                DiesIraeVFXLibrary.SpawnMusicNotes(playerCenter, 1, 22f, 0.6f, 0.8f, 28);

            float pulse = 0.22f + MathF.Sin(timer * 0.04f) * 0.07f;
            Lighting.AddLight(playerCenter, ShackleCore.ToVector3() * pulse);
        }

        /// <summary>
        /// Mark proc VFX — wrath mark applied when ranged attack hits.
        /// </summary>
        public static void MarkProcVFX(Vector2 enemyCenter)
        {
            // Infernal red mark ring
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(1f, 2f);
                Color col = Color.Lerp(ShackleCore, ShackleFlash, (float)i / 8f);
                Dust d = Dust.NewDustPerfect(enemyCenter, DustID.Torch, vel, 0, col, 0.9f);
                d.noGravity = true;
            }

            // Bone-white shackle lock particles
            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(1.5f, 1.5f);
                Dust d = Dust.NewDustPerfect(enemyCenter, DustID.TintableDust, vel, 150,
                    DiesIraePalette.BoneWhite, 0.7f);
                d.noGravity = true;
            }

            Lighting.AddLight(enemyCenter, ShackleFlash.ToVector3() * 0.4f);
        }

        /// <summary>
        /// On-hit accent VFX.
        /// </summary>
        public static void HitVFX(Vector2 hitPos)
        {
            DiesIraeVFXLibrary.SpawnEmberScatter(hitPos, 2, 1.5f);

            if (Main.rand.NextBool(3))
                DiesIraeVFXLibrary.SpawnBoneAshScatter(hitPos, 1, 1f);
        }
    }

    // =============================================================================
    //  LEGACY COMPATIBILITY WRAPPER
    // =============================================================================

    /// <summary>
    /// Legacy shared VFX helper — delegates to per-accessory VFX classes.
    /// </summary>
    public static class DiesIraeAccessoryVFX
    {
        // Ember of the Condemned delegates
        public static void EmberAmbientAura(Vector2 playerCenter, float timer) =>
            EmberOfTheCondemnedVFX.AmbientAura(playerCenter, timer);

        public static void EmberHellfireProcVFX(Vector2 enemyCenter) =>
            EmberOfTheCondemnedVFX.HellfireProcVFX(enemyCenter);

        public static void EmberHitVFX(Vector2 hitPos) =>
            EmberOfTheCondemnedVFX.HitVFX(hitPos);

        // Seal of Damnation delegates
        public static void SealAmbientAura(Vector2 playerCenter, float timer) =>
            SealOfDamnationVFX.AmbientAura(playerCenter, timer);

        public static void SealDamnationProcVFX(Vector2 enemyCenter) =>
            SealOfDamnationVFX.DamnationProcVFX(enemyCenter);

        public static void SealHitVFX(Vector2 hitPos) =>
            SealOfDamnationVFX.HitVFX(hitPos);

        // Chain of Final Judgment delegates
        public static void ChainAmbientAura(Vector2 playerCenter, float timer) =>
            ChainOfFinalJudgmentVFX.AmbientAura(playerCenter, timer);

        public static void ChainLifestealVFX(Vector2 playerCenter, Vector2 enemyCenter) =>
            ChainOfFinalJudgmentVFX.LifestealVFX(playerCenter, enemyCenter);

        public static void ChainExecuteVFX(Vector2 enemyCenter) =>
            ChainOfFinalJudgmentVFX.ExecuteVFX(enemyCenter);

        public static void ChainHitVFX(Vector2 hitPos) =>
            ChainOfFinalJudgmentVFX.HitVFX(hitPos);

        // Requiem's Shackle delegates
        public static void ShackleAmbientAura(Vector2 playerCenter, float timer) =>
            RequiemsShackleVFX.AmbientAura(playerCenter, timer);

        public static void ShackleMarkProcVFX(Vector2 enemyCenter) =>
            RequiemsShackleVFX.MarkProcVFX(enemyCenter);

        public static void ShackleHitVFX(Vector2 hitPos) =>
            RequiemsShackleVFX.HitVFX(hitPos);
    }
}
