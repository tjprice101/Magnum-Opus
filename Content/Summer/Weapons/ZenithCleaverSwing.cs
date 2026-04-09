using System;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Common.Systems;
using MagnumOpus.Content.Summer.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Summer.Weapons
{
    /// <summary>
    /// Zenith Cleaver swing projectile — Summer theme melee. ExobladeStyleSwing architecture.
    /// Every swing fires a SolarWave; every 7th swing unleashes Zenith Strike
    /// (ZenithFlare + 8 radial SolarWaves). Hits apply Sunstroke debuffs
    /// and spawn seeking SummerCrystals.
    /// </summary>
    public class ZenithCleaverSwing : ExobladeStyleSwing
    {
        private static readonly Color SunGold = new Color(255, 215, 0);
        private static readonly Color SunOrange = new Color(255, 140, 0);
        private static readonly Color SunWhite = new Color(255, 250, 240);
        private static readonly Color SunRed = new Color(255, 100, 50);

        private int swingCounter = 0;
        private int _crystalCooldown = 0;
        private bool hasSpawnedWave = false;

        protected override bool SupportsDash => false;
        protected override float BladeLength => 110f;
        protected override int BaseSwingFrames => 78;
        protected override float TextureDrawScale => 0.12f;
        protected override Color SlashPrimaryColor => SunGold;
        protected override Color SlashSecondaryColor => new Color(120, 50, 10);
        protected override Color SlashAccentColor => SunOrange;
        protected override string GradientLUTPath => "MagnumOpus/Assets/VFX Asset Library/ColorGradients/LaCampanellaGradientLUTandRAMP";

        public override string Texture => "MagnumOpus/Content/Summer/Weapons/ZenithCleaver";

        protected override Color GetLensFlareColor(float p)
            => Color.Lerp(SunGold, SunOrange, (float)Math.Pow(p, 2));

        protected override Color GetSwingDustColor()
        {
            float t = Main.rand.NextFloat();
            return t < 0.5f
                ? Color.Lerp(SunOrange, SunGold, Main.rand.NextFloat())
                : Color.Lerp(SunRed, SunWhite, Main.rand.NextFloat());
        }

        protected override void OnSwingStart(bool isFirstSwing)
        {
            hasSpawnedWave = false;
        }

        protected override void OnSwingFrame()
        {
            if (_crystalCooldown > 0) _crystalCooldown--;

            // Fire SolarWave at ~55% through swing
            if (!hasSpawnedWave && Progression >= 0.55f)
            {
                hasSpawnedWave = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tipPos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale;
                    Vector2 waveVel = SwordDirection * 14f;

                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(), tipPos, waveVel,
                        ModContent.ProjectileType<SolarWave>(),
                        Projectile.damage / 2, Projectile.knockBack * 0.5f, Projectile.owner);

                    for (int i = 0; i < 4; i++)
                    {
                        Dust flare = Dust.NewDustPerfect(
                            tipPos + Main.rand.NextVector2Circular(8f, 8f),
                            DustID.SolarFlare,
                            waveVel.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(2f, 5f),
                            0, SunGold, 1.5f);
                        flare.noGravity = true;
                    }

                    // Zenith Strike — every 7th swing
                    swingCounter++;
                    if (swingCounter >= 7)
                    {
                        swingCounter = 0;
                        TriggerZenithStrike(tipPos);
                    }
                }
            }

            // Solar dust along blade during active swing
            if (Progression > 0.10f && Progression < 0.92f)
            {
                Vector2 tipPos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale;

                for (int i = 0; i < 2; i++)
                {
                    Vector2 dustPos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * Main.rand.NextFloat(0.35f, 1f);
                    Dust d = Dust.NewDustPerfect(dustPos, DustID.SolarFlare,
                        -SwordDirection * Main.rand.NextFloat(1f, 3.5f) + Main.rand.NextVector2Circular(1.2f, 1.2f),
                        0, SunOrange, 1.5f);
                    d.noGravity = true;
                    d.fadeIn = 1.2f;
                }

                if (Main.GameUpdateCount % 2 == 0)
                {
                    Dust g = Dust.NewDustPerfect(
                        Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * Main.rand.NextFloat(0.45f, 0.95f),
                        DustID.Enchanted_Gold,
                        -SwordDirection * Main.rand.NextFloat(0.5f, 2.5f),
                        0, SunGold, 1.3f);
                    g.noGravity = true;
                }

                if (Main.rand.NextBool(3))
                {
                    Dust sparkle = Dust.NewDustPerfect(
                        Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * Main.rand.NextFloat(0.6f, 1f),
                        DustID.FireworkFountain_Yellow,
                        -SwordDirection * Main.rand.NextFloat(1f, 4f) + Main.rand.NextVector2Circular(1.5f, 1.5f),
                        0, SunWhite, 1.2f);
                    sparkle.noGravity = true;
                }

                if (Main.rand.NextBool(4))
                {
                    Color emberColor = Color.Lerp(SunOrange, SunRed, Main.rand.NextFloat());
                    Dust ember = Dust.NewDustPerfect(
                        tipPos + Main.rand.NextVector2Circular(12f, 12f),
                        DustID.Torch,
                        new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-3f, -1f)),
                        0, emberColor, 1.3f);
                    ember.noGravity = true;
                }

                Lighting.AddLight(tipPos, SunGold.ToVector3() * 0.6f);
            }
        }

        private void TriggerZenithStrike(Vector2 tipPos)
        {
            SoundEngine.PlaySound(SoundID.Item45 with { Pitch = 0.2f, Volume = 1.1f }, tipPos);

            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(), tipPos, SwordDirection * 16f,
                ModContent.ProjectileType<ZenithFlare>(),
                Projectile.damage * 2, Projectile.knockBack * 2f, Projectile.owner);

            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(10f, 14f);
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(), tipPos, vel,
                    ModContent.ProjectileType<SolarWave>(),
                    Projectile.damage / 3, Projectile.knockBack * 0.3f, Projectile.owner);
            }

            for (int i = 0; i < 6; i++)
            {
                Dust flash = Dust.NewDustPerfect(tipPos, DustID.SolarFlare,
                    Main.rand.NextVector2Circular(6f, 6f), 0, SunWhite, 2.0f);
                flash.noGravity = true;
            }

            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Dust burst = Dust.NewDustPerfect(tipPos, DustID.Enchanted_Gold,
                    burstVel, 0, SunGold, 1.8f);
                burst.noGravity = true;
            }

            for (int ring = 0; ring < 5; ring++)
            {
                float progress = ring / 5f;
                Color ringColor = Color.Lerp(SunGold, SunRed, progress);
                for (int j = 0; j < 4; j++)
                {
                    float angle = MathHelper.TwoPi * j / 4f + ring * MathHelper.PiOver4 * 0.5f;
                    Vector2 offset = angle.ToRotationVector2() * (20f + ring * 12f);
                    Dust halo = Dust.NewDustPerfect(tipPos + offset, DustID.SolarFlare,
                        offset.SafeNormalize(Vector2.Zero) * 2.5f, 0, ringColor, 1.4f);
                    halo.noGravity = true;
                }
            }
        }

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player owner = Main.player[Projectile.owner];

            // Sunstroke debuffs
            target.AddBuff(BuffID.OnFire3, 180);
            target.AddBuff(BuffID.Daybreak, 120);

            // Seeking summer crystals (30-frame cooldown)
            if (_crystalCooldown <= 0 && Main.myPlayer == Projectile.owner)
            {
                SeekingCrystalHelper.SpawnSummerCrystals(
                    Projectile.GetSource_FromThis(), target.Center,
                    (target.Center - owner.Center).SafeNormalize(Vector2.UnitY) * 5f,
                    (int)(Projectile.damage * 0.3f), Projectile.knockBack * 0.3f,
                    Projectile.owner, count: 2 + Main.rand.Next(2));
                _crystalCooldown = 30;
            }

            // Impact VFX — gradient halo rings
            for (int i = 0; i < 4; i++)
            {
                float progress = i / 4f;
                Color ringColor = Color.Lerp(SunGold, SunRed, progress);
                for (int j = 0; j < 3; j++)
                {
                    float angle = MathHelper.TwoPi * j / 3f + i * MathHelper.PiOver4;
                    Vector2 offset = angle.ToRotationVector2() * (12f + i * 10f);
                    Dust ring = Dust.NewDustPerfect(target.Center + offset, DustID.SolarFlare,
                        offset.SafeNormalize(Vector2.Zero) * 2f, 0, ringColor, 1.4f);
                    ring.noGravity = true;
                }
            }

            // Solar shimmer flares
            for (int i = 0; i < 6; i++)
            {
                float progress = i / 6f;
                float hue = 0.08f + progress * 0.10f;
                Color shimmerColor = Main.hslToRgb(hue, 1f, 0.75f);
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Dust shimmer = Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.Enchanted_Gold, vel, 0, shimmerColor, 1.5f);
                shimmer.noGravity = true;
            }

            // Radial dust burst
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                int dustType = i % 2 == 0 ? DustID.SolarFlare : DustID.Enchanted_Gold;
                Dust burst = Dust.NewDustPerfect(target.Center, dustType,
                    vel, 0, Color.Lerp(SunGold, SunOrange, Main.rand.NextFloat()), 1.5f);
                burst.noGravity = true;
            }

            // White-hot sparkles
            for (int i = 0; i < 4; i++)
            {
                Dust sparkle = Dust.NewDustPerfect(
                    target.Center + Main.rand.NextVector2Circular(10f, 10f),
                    DustID.FireworkFountain_Yellow,
                    Main.rand.NextVector2Circular(4f, 4f), 0, SunWhite, 1.4f);
                sparkle.noGravity = true;
            }

            // Ember burst
            for (int i = 0; i < 5; i++)
            {
                Color emberColor = Color.Lerp(SunOrange, SunRed, Main.rand.NextFloat());
                Dust ember = Dust.NewDustPerfect(target.Center, DustID.Torch,
                    Main.rand.NextVector2Circular(5f, 5f) + new Vector2(0, -Main.rand.NextFloat(1f, 3f)),
                    0, emberColor, 1.3f);
                ember.noGravity = true;
            }

            // Music notes from impact
            for (int i = 0; i < 3; i++)
            {
                float noteAngle = MathHelper.TwoPi * i / 3f + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 noteVel = noteAngle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Dust note = Dust.NewDustPerfect(
                    target.Center + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.Enchanted_Gold,
                    noteVel + new Vector2(0, -Main.rand.NextFloat(0.5f, 2f)),
                    0, SunGold, Main.rand.NextFloat(0.8f, 1.1f) * 1.5f);
                note.noGravity = true;
            }

            Lighting.AddLight(target.Center, SunGold.ToVector3() * 1.2f);
        }
    }
}
