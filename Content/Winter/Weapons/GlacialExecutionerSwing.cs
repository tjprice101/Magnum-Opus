using System;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Winter.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Winter.Weapons
{
    /// <summary>
    /// Glacial Executioner swing projectile — Winter theme melee. ExobladeStyleSwing architecture.
    /// 4-phase combo: icicle bolts, avalanche waves, 25% freeze chance, Frostburn2.
    /// </summary>
    public class GlacialExecutionerSwing : ExobladeStyleSwing
    {
        private static readonly Color IceBlue = new Color(150, 220, 255);
        private static readonly Color FrostWhite = new Color(240, 250, 255);
        private static readonly Color DeepBlue = new Color(60, 100, 180);
        private static readonly Color CrystalCyan = new Color(100, 255, 255);

        private int comboPhase = 0;
        private bool hasSpawnedSpecial = false;

        protected override bool SupportsDash => false;
        protected override float BladeLength => 115f;
        protected override int BaseSwingFrames => 78;
        protected override float TextureDrawScale => 0.12f;
        protected override Color SlashPrimaryColor => IceBlue;
        protected override Color SlashSecondaryColor => new Color(40, 60, 120);
        protected override Color SlashAccentColor => CrystalCyan;
        protected override string GradientLUTPath => "MagnumOpus/Assets/VFX Asset Library/ColorGradients/ClairDeLuneGradientLUTandRAMP";

        public override string Texture => "MagnumOpus/Content/Winter/Weapons/GlacialExecutioner";

        protected override Color GetLensFlareColor(float p)
            => Color.Lerp(IceBlue, CrystalCyan, (float)Math.Pow(p, 2));

        protected override Color GetSwingDustColor()
        {
            float t = Main.rand.NextFloat();
            return t < 0.5f
                ? Color.Lerp(IceBlue, FrostWhite, Main.rand.NextFloat())
                : Color.Lerp(DeepBlue, CrystalCyan, Main.rand.NextFloat());
        }

        protected override void OnSwingStart(bool isFirstSwing)
        {
            hasSpawnedSpecial = false;
            comboPhase++;
        }

        protected override void OnSwingFrame()
        {
            int phase = comboPhase % 4;

            // Phase 2 at ~70%: spawn 5 IcicleBolts in a fan
            if (!hasSpawnedSpecial && phase == 2 && Progression >= 0.70f)
            {
                hasSpawnedSpecial = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tipPos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale;
                    for (int i = 0; i < 5; i++)
                    {
                        float spread = MathHelper.ToRadians(-40f + i * 20f);
                        Vector2 vel = SwordDirection.RotatedBy(spread) * Main.rand.NextFloat(8f, 12f);
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(), tipPos, vel,
                            ModContent.ProjectileType<IcicleBolt>(),
                            Projectile.damage / 3, Projectile.knockBack * 0.4f, Projectile.owner);
                    }
                }
            }

            // Phase 3 at ~85%: AvalancheWave + 4 seeking winter crystals
            if (!hasSpawnedSpecial && phase == 3 && Progression >= 0.85f)
            {
                hasSpawnedSpecial = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tipPos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale;
                    Vector2 waveVel = SwordDirection * 16f;

                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(), tipPos, waveVel,
                        ModContent.ProjectileType<AvalancheWave>(),
                        (int)(Projectile.damage * 1.5f), Projectile.knockBack, Projectile.owner);

                    SeekingCrystalHelper.SpawnWinterCrystals(
                        Projectile.GetSource_FromThis(), tipPos, SwordDirection * 6f,
                        (int)(Projectile.damage * 0.35f), Projectile.knockBack * 0.3f,
                        Projectile.owner, count: 4);

                    SoundEngine.PlaySound(SoundID.Item120 with { Pitch = -0.5f, Volume = 1f }, tipPos);
                }
            }

            // Frost dust along blade during active swing
            if (Progression > 0.10f && Progression < 0.92f)
            {
                Vector2 tipPos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale;

                for (int i = 0; i < 2; i++)
                {
                    Vector2 dustPos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * Main.rand.NextFloat(0.4f, 1f);
                    Dust d = Dust.NewDustPerfect(dustPos, DustID.IceTorch,
                        -SwordDirection * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(1f, 1f),
                        0, IceBlue, 1.3f);
                    d.noGravity = true;
                    d.fadeIn = 1.2f;
                }

                if (Main.GameUpdateCount % 2 == 0)
                {
                    Dust g = Dust.NewDustPerfect(
                        Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * Main.rand.NextFloat(0.5f, 0.9f),
                        DustID.Frost,
                        -SwordDirection * Main.rand.NextFloat(0.5f, 2f),
                        0, FrostWhite, 1.1f);
                    g.noGravity = true;
                }

                if (Main.rand.NextBool(3))
                {
                    Vector2 shardPos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * Main.rand.NextFloat(0.3f, 1f);
                    Dust shard = Dust.NewDustPerfect(shardPos, DustID.BlueCrystalShard,
                        -SwordDirection * Main.rand.NextFloat(1f, 2.5f) + Main.rand.NextVector2Circular(0.5f, 0.5f),
                        0, CrystalCyan, 0.9f);
                    shard.noGravity = true;
                }

                if (Main.rand.NextBool(4))
                {
                    Color noteColor = Color.Lerp(IceBlue, CrystalCyan, Main.rand.NextFloat());
                    Dust note = Dust.NewDustPerfect(
                        tipPos + Main.rand.NextVector2Circular(6f, 6f),
                        DustID.BlueCrystalShard,
                        -SwordDirection * 1.5f + Main.rand.NextVector2Circular(1f, 1f),
                        0, noteColor, Main.rand.NextFloat(0.7f, 0.95f) * 1.6f);
                    note.noGravity = true;
                }
            }
        }

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player owner = Main.player[Projectile.owner];

            // Always apply Frostburn2
            target.AddBuff(BuffID.Frostburn2, 240);

            // 25% freeze chance + seeking crystals
            if (Main.rand.NextFloat() < 0.25f)
            {
                target.AddBuff(BuffID.Frozen, 90);

                if (Main.myPlayer == Projectile.owner)
                {
                    SeekingCrystalHelper.SpawnWinterCrystals(
                        Projectile.GetSource_FromThis(), target.Center,
                        (target.Center - owner.Center).SafeNormalize(Vector2.Zero) * 5f,
                        (int)(Projectile.damage * 0.35f), Projectile.knockBack * 0.3f,
                        Projectile.owner, count: 5);
                }

                SoundEngine.PlaySound(SoundID.Item27 with { Pitch = 0.3f, Volume = 0.7f }, target.Center);
                for (int i = 0; i < 8; i++)
                {
                    Dust freeze = Dust.NewDustPerfect(target.Center, DustID.Frost,
                        Main.rand.NextVector2Circular(6f, 6f), 0, CrystalCyan, 1.6f);
                    freeze.noGravity = true;
                }
            }

            // Gradient halo rings — deep blue to frost white
            for (int i = 0; i < 4; i++)
            {
                float progress = i / 4f;
                Color ringColor = Color.Lerp(DeepBlue, FrostWhite, progress);
                for (int j = 0; j < 2; j++)
                {
                    float angle = MathHelper.TwoPi * j / 2f + i * MathHelper.PiOver4;
                    Vector2 offset = angle.ToRotationVector2() * (15f + i * 8f);
                    Dust ring = Dust.NewDustPerfect(target.Center + offset, DustID.IceTorch,
                        offset.SafeNormalize(Vector2.Zero) * 2f, 0, ringColor, 1.3f);
                    ring.noGravity = true;
                }
            }

            for (int i = 0; i < 3; i++)
            {
                Dust sparkle = Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(14f, 14f),
                    DustID.BlueCrystalShard,
                    Main.rand.NextVector2Circular(3f, 3f), 0, CrystalCyan, 1.0f);
                sparkle.noGravity = true;
            }

            for (int i = 0; i < 10; i++)
            {
                Dust burst = Dust.NewDustPerfect(target.Center, DustID.IceTorch,
                    Main.rand.NextVector2Circular(5f, 5f), 0,
                    Color.Lerp(IceBlue, FrostWhite, Main.rand.NextFloat()), 1.3f);
                burst.noGravity = true;
                burst.fadeIn = 1.2f;
            }

            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(4f, 4f);
                Color sparkColor = Color.Lerp(IceBlue, FrostWhite, Main.rand.NextFloat()) * 0.6f;
                var spark = new GenericGlowParticle(target.Center, sparkVel, sparkColor, 0.30f, 20, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Music note burst
            for (int n = 0; n < 3; n++)
            {
                float angle = MathHelper.TwoPi * n / 3f + Main.rand.NextFloat(-0.4f, 0.4f);
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color noteColor = Color.Lerp(IceBlue, CrystalCyan, Main.rand.NextFloat());
                Dust note = Dust.NewDustPerfect(target.Center, DustID.BlueCrystalShard,
                    noteVel, 0, noteColor, Main.rand.NextFloat(1.4f, 1.8f));
                note.noGravity = true;
            }
        }
    }
}
