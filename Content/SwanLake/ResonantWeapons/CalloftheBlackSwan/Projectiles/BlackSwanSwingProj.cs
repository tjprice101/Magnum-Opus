using System;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Content.SwanLake.ResonantWeapons.CalloftheBlackSwan.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.CalloftheBlackSwan.Projectiles
{
    /// <summary>
    /// Call of the Black Swan — Swan Lake theme melee. Exoblade-architecture swing.
    /// Monochrome white/black trail with prismatic rainbow edge shimmer and feather drift.
    /// 3-phase Swan Dance combo that advances each swing (including hold re-swings).
    /// </summary>
    public class BlackSwanSwingProj : ExobladeStyleSwing
    {
        protected override bool SupportsDash => false;

        /// <summary>Swan Dance combo phase (0-2). Advances each swing including hold re-swings.</summary>
        private int dancePhase = 0;

        protected override float BladeLength => 108f;
        protected override int BaseSwingFrames => 82;
        protected override Color SlashPrimaryColor => new Color(230, 230, 255);
        protected override Color SlashSecondaryColor => new Color(40, 30, 60);
        protected override Color SlashAccentColor => new Color(200, 160, 255);

        public override string Texture => "MagnumOpus/Content/SwanLake/ResonantWeapons/CalloftheBlackSwan/CalloftheBlackSwan";

        protected override Color GetLensFlareColor(float p)
            => Color.Lerp(new Color(220, 220, 255), new Color(180, 140, 255), (float)Math.Pow(p, 2));

        protected override Color GetSwingDustColor()
        {
            float t = Main.rand.NextFloat();
            if (t < 0.3f)
                return Color.Lerp(new Color(240, 240, 255), Color.White, Main.rand.NextFloat());
            else if (t < 0.6f)
                return Color.Lerp(new Color(180, 140, 255), new Color(140, 200, 255), Main.rand.NextFloat());
            else
                return Color.Lerp(new Color(255, 180, 220), new Color(180, 255, 220), Main.rand.NextFloat());
        }

        protected override void OnSwingStart(bool isFirstSwing)
        {
            if (Main.myPlayer != Projectile.owner) return;

            Player player = Owner;
            int damage = Projectile.damage;
            float knockback = Projectile.knockBack;
            Vector2 aimDir = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX);
            IEntitySource source = Projectile.GetSource_FromThis();

            int phase = dancePhase % 3;
            dancePhase++;
            int flareType = ModContent.ProjectileType<BlackSwanFlareProj>();
            int flareDmg = (int)(damage * 0.3f);

            var bsp = player.BlackSwan();
            bool maxGrace = bsp.IsMaxGrace;

            switch (phase)
            {
                case 0: // Entrechat — 3 feathers in fan arc
                    for (int i = -1; i <= 1; i++)
                    {
                        Vector2 flareVel = aimDir.RotatedBy(MathHelper.ToRadians(20 * i)) * 10f;
                        float polarity = (i + 1) % 2;
                        Projectile.NewProjectile(source, player.MountedCenter, flareVel,
                            flareType, flareDmg, knockback * 0.4f, player.whoAmI,
                            maxGrace ? 1f : 0f, polarity);
                    }
                    break;

                case 1: // Fouetté — 4 flares in spinning radial burst
                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 flareVel = aimDir.RotatedBy(MathHelper.ToRadians(90 * i - 135)) * 9f;
                        Projectile.NewProjectile(source, player.MountedCenter, flareVel,
                            flareType, flareDmg, knockback * 0.4f, player.whoAmI,
                            maxGrace ? 1f : 0f, i % 2);
                    }
                    break;

                case 2: // Grand Jeté — 5 empowered raining flares + 2 shockwave seeds
                    for (int i = 0; i < 5; i++)
                    {
                        float spread = MathHelper.ToRadians(-40 + 20 * i);
                        Vector2 flareVel = aimDir.RotatedBy(spread) * 11f;
                        Projectile.NewProjectile(source, player.MountedCenter, flareVel,
                            flareType, (int)(damage * 0.4f), knockback * 0.6f, player.whoAmI,
                            1f, i % 2);
                    }
                    for (int i = -1; i <= 1; i += 2)
                    {
                        Vector2 shockVel = aimDir.RotatedBy(MathHelper.ToRadians(50 * i)) * 6f;
                        Projectile.NewProjectile(source, player.MountedCenter, shockVel,
                            flareType, (int)(damage * 0.5f), knockback, player.whoAmI,
                            2f, 0f);
                    }
                    break;
            }

            // Rainbow sparkle burst at player on each swing
            try { SwanLakeVFXLibrary.SpawnPrismaticSparkles(player.MountedCenter, 4, 20f); } catch { }
        }

        protected override void OnSwingFrame()
        {
            // Prismatic edge sparkles
            if (Progression > 0.3f && Progression < 0.85f && Main.rand.NextFloat() < 0.35f)
            {
                Vector2 pos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * Main.rand.NextFloat(0.4f, 1f);
                Vector2 vel = SwordDirection.RotatedBy(MathHelper.PiOver2 * Direction * Main.rand.NextFloat(0.5f, 1.5f)) * Main.rand.NextFloat(0.5f, 2f);
                Color sparkleColor = Main.hslToRgb(Main.rand.NextFloat(), 0.5f, 0.85f);
                Dust sparkle = Dust.NewDustPerfect(pos, DustID.RainbowTorch, vel, 60, sparkleColor, Main.rand.NextFloat(0.4f, 0.8f));
                sparkle.noGravity = true;
                sparkle.fadeIn = 0.6f;
            }

            // White feather drift
            if (Progression > 0.35f && Main.rand.NextFloat() < 0.15f)
            {
                Vector2 pos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * Main.rand.NextFloat(0.6f, 0.95f);
                Dust feather = Dust.NewDustPerfect(pos, DustID.Cloud,
                    new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-2f, -0.5f)), 80, Color.White, 0.6f);
                feather.noGravity = true;
            }
        }

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn, 180);

            // Prismatic impact burst
            for (int i = 0; i < 8; i++)
            {
                Color c = Main.hslToRgb(i / 8f, 0.6f, 0.8f);
                Dust burst = Dust.NewDustPerfect(target.Center, DustID.RainbowTorch,
                    Main.rand.NextVector2CircularEdge(5f, 5f), 40, c, Main.rand.NextFloat(0.7f, 1.1f));
                burst.noGravity = true;
            }
            // White flash
            for (int i = 0; i < 4; i++)
            {
                Dust flash = Dust.NewDustPerfect(target.Center, DustID.Cloud,
                    Main.rand.NextVector2Circular(2f, 2f), 0, Color.White, 1f);
                flash.noGravity = true;
            }

            // Inner B&W + Outer rainbow sparkle explosion
            try { SwanLakeVFXLibrary.SpawnMixedSparkleImpact(target.Center, 0.7f, 5, 5); } catch { }
            try { SwanLakeVFXLibrary.SpawnPrismaticSparkles(target.Center, 4, 15f); } catch { }
        }

        protected override void OnDashHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn2, 300);

            for (int i = 0; i < 16; i++)
            {
                Color c = Main.hslToRgb(i / 16f, 0.6f, 0.9f);
                Dust burst = Dust.NewDustPerfect(target.Center, DustID.RainbowTorch,
                    Main.rand.NextVector2CircularEdge(8f, 8f), 20, c, Main.rand.NextFloat(1f, 1.6f));
                burst.noGravity = true;
            }

            // Inner B&W + Outer rainbow sparkle explosion (dash hit is more intense)
            try { SwanLakeVFXLibrary.SpawnMixedSparkleImpact(target.Center, 1.0f, 6, 6); } catch { }
            try { SwanLakeVFXLibrary.SpawnPrismaticSparkles(target.Center, 6, 20f); } catch { }
        }
    }
}
