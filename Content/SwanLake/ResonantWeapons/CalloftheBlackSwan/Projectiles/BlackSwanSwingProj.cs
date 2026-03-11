using System;
using MagnumOpus.Common.BaseClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.CalloftheBlackSwan.Projectiles
{
    /// <summary>
    /// Call of the Black Swan — Swan Lake theme melee. Exoblade-architecture swing.
    /// Monochrome white/black trail with prismatic rainbow edge shimmer and feather drift.
    /// </summary>
    public class BlackSwanSwingProj : ExobladeStyleSwing
    {
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
