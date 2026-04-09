using System;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Autumn.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Autumn.Weapons
{
    /// <summary>
    /// Harvest Reaper swing projectile — Autumn theme melee. ExobladeStyleSwing architecture.
    /// 4-phase combo with decay bolts, crescent waves, soul harvest, and Ichor stacking.
    /// </summary>
    public class HarvestReaperSwing : ExobladeStyleSwing
    {
        private static readonly Color AutumnOrange = new Color(255, 140, 50);
        private static readonly Color AutumnBrown = new Color(139, 90, 43);
        private static readonly Color AutumnRed = new Color(178, 34, 34);
        private static readonly Color AutumnGold = new Color(218, 165, 32);
        private static readonly Color DecayPurple = new Color(100, 50, 120);

        private int comboPhase = 0;
        private int hitCounter = 0;
        private bool hasSpawnedSpecial = false;

        protected override bool SupportsDash => false;
        protected override float BladeLength => 110f;
        protected override int BaseSwingFrames => 78;
        protected override float TextureDrawScale => 0.12f;
        protected override Color SlashPrimaryColor => AutumnOrange;
        protected override Color SlashSecondaryColor => new Color(100, 50, 20);
        protected override Color SlashAccentColor => AutumnGold;
        protected override string GradientLUTPath => "MagnumOpus/Assets/VFX Asset Library/ColorGradients/LaCampanellaGradientLUTandRAMP";

        public override string Texture => "MagnumOpus/Content/Autumn/Weapons/HarvestReaper";

        protected override Color GetLensFlareColor(float p)
            => Color.Lerp(AutumnOrange, AutumnGold, (float)Math.Pow(p, 2));

        protected override Color GetSwingDustColor()
        {
            float t = Main.rand.NextFloat();
            return t < 0.5f
                ? Color.Lerp(AutumnOrange, AutumnGold, Main.rand.NextFloat())
                : Color.Lerp(AutumnRed, AutumnBrown, Main.rand.NextFloat());
        }

        protected override void OnSwingStart(bool isFirstSwing)
        {
            hasSpawnedSpecial = false;
            comboPhase++;
        }

        protected override void OnSwingFrame()
        {
            int phase = comboPhase % 4;

            // Phase 2 at ~70%: spawn 4 DecayBoltProjectiles in a fan
            if (!hasSpawnedSpecial && phase == 2 && Progression >= 0.70f)
            {
                hasSpawnedSpecial = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tipPos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale;
                    for (int i = 0; i < 4; i++)
                    {
                        float spread = MathHelper.ToRadians(-35f + i * 23f);
                        Vector2 vel = SwordDirection.RotatedBy(spread) * Main.rand.NextFloat(7f, 10f);
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(), tipPos, vel,
                            ModContent.ProjectileType<DecayBoltProjectile>(),
                            Projectile.damage / 3, Projectile.knockBack * 0.4f, Projectile.owner);
                    }
                }
            }

            // Phase 3 at ~85%: DecayCrescentWave + 4 seeking autumn crystals
            if (!hasSpawnedSpecial && phase == 3 && Progression >= 0.85f)
            {
                hasSpawnedSpecial = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tipPos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale;
                    Vector2 waveVel = SwordDirection * 14f;

                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(), tipPos, waveVel,
                        ModContent.ProjectileType<DecayCrescentWave>(),
                        (int)(Projectile.damage * 1.6f), Projectile.knockBack, Projectile.owner);

                    SeekingCrystalHelper.SpawnAutumnCrystals(
                        Projectile.GetSource_FromThis(), tipPos, SwordDirection * 6f,
                        (int)(Projectile.damage * 0.35f), Projectile.knockBack * 0.3f,
                        Projectile.owner, count: 4);

                    SoundEngine.PlaySound(SoundID.Item73 with { Pitch = -0.4f, Volume = 1f }, tipPos);
                }
            }

            // Dust along blade during active swing
            if (Progression > 0.10f && Progression < 0.92f)
            {
                Vector2 tipPos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale;

                for (int i = 0; i < 2; i++)
                {
                    Vector2 dustPos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * Main.rand.NextFloat(0.4f, 1f);
                    Dust d = Dust.NewDustPerfect(dustPos, DustID.Torch,
                        -SwordDirection * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(1f, 1f),
                        0, AutumnOrange, 1.3f);
                    d.noGravity = true;
                    d.fadeIn = 1.2f;
                }

                if (Main.GameUpdateCount % 2 == 0)
                {
                    Dust g = Dust.NewDustPerfect(
                        Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * Main.rand.NextFloat(0.5f, 0.9f),
                        DustID.GoldCoin,
                        -SwordDirection * Main.rand.NextFloat(0.5f, 2f),
                        0, AutumnGold, 1.0f);
                    g.noGravity = true;
                }

                if (Main.rand.NextBool(3))
                {
                    Vector2 leafPos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * Main.rand.NextFloat(0.3f, 1f);
                    Vector2 leafVel = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(0.5f, 2f));
                    Color leafColor = Main.rand.NextBool() ? AutumnOrange : AutumnRed;
                    var leaf = new GenericGlowParticle(leafPos, leafVel, leafColor * 0.65f, 0.28f, 30, true);
                    MagnumParticleHandler.SpawnParticle(leaf);
                }

                if (Main.rand.NextBool(4))
                {
                    Color noteColor = Color.Lerp(AutumnOrange, AutumnGold, Main.rand.NextFloat());
                    Dust note = Dust.NewDustPerfect(
                        tipPos + Main.rand.NextVector2Circular(6f, 6f),
                        DustID.Enchanted_Gold,
                        -SwordDirection * 1.5f + Main.rand.NextVector2Circular(1f, 1f),
                        0, noteColor, Main.rand.NextFloat(0.7f, 0.95f) * 1.6f);
                    note.noGravity = true;
                }
            }
        }

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player owner = Main.player[Projectile.owner];
            hitCounter++;

            // Gradient halo rings — orange to purple
            for (int i = 0; i < 4; i++)
            {
                float progress = i / 4f;
                Color ringColor = Color.Lerp(AutumnOrange, DecayPurple, progress);
                for (int j = 0; j < 2; j++)
                {
                    float angle = MathHelper.TwoPi * j / 2f + i * MathHelper.PiOver4;
                    Vector2 offset = angle.ToRotationVector2() * (15f + i * 8f);
                    Dust ring = Dust.NewDustPerfect(target.Center + offset, DustID.Torch,
                        offset.SafeNormalize(Vector2.Zero) * 2f, 0, ringColor, 1.2f);
                    ring.noGravity = true;
                }
            }

            for (int i = 0; i < 3; i++)
            {
                Dust shimmer = Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(12f, 12f),
                    DustID.Enchanted_Gold,
                    Main.rand.NextVector2Circular(3f, 3f), 0, AutumnGold, 1.4f);
                shimmer.noGravity = true;
            }

            for (int i = 0; i < 10; i++)
            {
                Dust burst = Dust.NewDustPerfect(target.Center,
                    Main.rand.NextBool() ? DustID.Torch : DustID.GoldCoin,
                    Main.rand.NextVector2Circular(5f, 5f), 0,
                    Color.Lerp(AutumnOrange, AutumnRed, Main.rand.NextFloat()), 1.3f);
                burst.noGravity = true;
                burst.fadeIn = 1.2f;
            }

            for (int i = 0; i < 4; i++)
            {
                Vector2 leafVel = new Vector2(Main.rand.NextFloat(-3f, 3f), -Main.rand.NextFloat(2f, 4f));
                Color leafColor = Main.rand.NextBool() ? AutumnOrange : AutumnRed;
                var leaf = new GenericGlowParticle(target.Center, leafVel, leafColor * 0.7f, 0.28f, 30, true);
                MagnumParticleHandler.SpawnParticle(leaf);
            }

            // Autumn's Decay — every 5th hit: Ichor + seeking crystals
            if (hitCounter >= 5)
            {
                hitCounter = 0;
                target.AddBuff(BuffID.Ichor, 300);

                if (Main.myPlayer == Projectile.owner)
                {
                    SeekingCrystalHelper.SpawnAutumnCrystals(
                        Projectile.GetSource_FromThis(), target.Center,
                        (target.Center - owner.Center).SafeNormalize(Vector2.Zero) * 4f,
                        (int)(Projectile.damage * 0.4f), Projectile.knockBack * 0.3f,
                        Projectile.owner, count: 5);
                }

                SoundEngine.PlaySound(SoundID.Item103 with { Pitch = -0.3f, Volume = 0.6f }, target.Center);
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 decayVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                    Color decayColor = Color.Lerp(DecayPurple, AutumnBrown, Main.rand.NextFloat()) * 0.6f;
                    Dust decay = Dust.NewDustPerfect(target.Center, DustID.PurpleTorch,
                        decayVel, 0, decayColor, 1.5f);
                    decay.noGravity = true;
                }
            }

            // Soul Harvest — kills spawn healing wisp
            if (target.life <= 0)
            {
                if (Main.myPlayer == Projectile.owner && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(), target.Center, Vector2.Zero,
                        ModContent.ProjectileType<SoulWisp>(), 0, 0, Projectile.owner);
                }

                for (int i = 0; i < 6; i++)
                {
                    Vector2 soulVel = Main.rand.NextVector2Circular(4f, 4f) + new Vector2(0, -2f);
                    Color soulColor = Color.Lerp(AutumnGold, Color.White, Main.rand.NextFloat()) * 0.6f;
                    Dust soul = Dust.NewDustPerfect(target.Center, DustID.Enchanted_Gold,
                        soulVel, 0, soulColor, 1.6f);
                    soul.noGravity = true;
                }
            }

            // Music note burst
            for (int n = 0; n < 3; n++)
            {
                float angle = MathHelper.TwoPi * n / 3f + Main.rand.NextFloat(-0.4f, 0.4f);
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color noteColor = Color.Lerp(AutumnOrange, AutumnGold, Main.rand.NextFloat());
                Dust note = Dust.NewDustPerfect(target.Center, DustID.Enchanted_Gold,
                    noteVel, 0, noteColor, Main.rand.NextFloat(1.4f, 1.8f));
                note.noGravity = true;
            }
        }
    }
}
