using System;
using MagnumOpus.Common.BaseClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.ResonantWeapons.RequiemOfReality.Projectiles
{
    /// <summary>
    /// Requiem of Reality — Fate theme melee. Exoblade-architecture swing.
    /// Dark cosmic crimson/void trail with spectral pink highlights.
    /// </summary>
    public class RequiemSwingProjectile : ExobladeStyleSwing
    {
        protected override float BladeLength => 112f;
        protected override int BaseSwingFrames => 80;
        protected override Color SlashPrimaryColor => new Color(180, 40, 100);
        protected override Color SlashSecondaryColor => new Color(30, 10, 50);
        protected override Color SlashAccentColor => new Color(255, 180, 220);

        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/RequiemOfReality";

        protected override Color GetLensFlareColor(float p)
            => Color.Lerp(new Color(200, 60, 120), new Color(255, 200, 240), (float)Math.Pow(p, 2));

        protected override Color GetSwingDustColor()
        {
            float t = Main.rand.NextFloat();
            return t < 0.5f
                ? Color.Lerp(new Color(160, 30, 80), new Color(220, 60, 120), Main.rand.NextFloat())
                : Color.Lerp(new Color(80, 20, 60), new Color(180, 60, 160), Main.rand.NextFloat());
        }

        protected override void OnSwingFrame()
        {
            // Spectral void dust along the blade
            if (Progression > 0.3f && Progression < 0.85f && Main.rand.NextFloat() < 0.35f)
            {
                Vector2 pos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * Main.rand.NextFloat(0.3f, 1f);
                Vector2 vel = SwordDirection.RotatedByRandom(0.6f) * Main.rand.NextFloat(1f, 3f);
                Dust void1 = Dust.NewDustPerfect(pos, DustID.PinkFairy, vel, 80, new Color(180, 40, 100), Main.rand.NextFloat(0.6f, 1f));
                void1.noGravity = true;
                void1.fadeIn = 0.8f;
            }

            // Dark spectral glint at tip
            if (Progression > 0.45f && Main.rand.NextFloat() < 0.15f)
            {
                Vector2 tip = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale;
                Dust glint = Dust.NewDustPerfect(tip, DustID.ShadowbeamStaff,
                    Main.rand.NextVector2Circular(1f, 1f), 40, default, 0.7f);
                glint.noGravity = true;
            }
        }

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.ShadowFlame, 180);

            for (int i = 0; i < 8; i++)
            {
                Dust burst = Dust.NewDustPerfect(target.Center, DustID.ShadowbeamStaff,
                    Main.rand.NextVector2CircularEdge(5f, 5f), 40, default, Main.rand.NextFloat(0.8f, 1.2f));
                burst.noGravity = true;
            }
            for (int i = 0; i < 4; i++)
            {
                Dust pink = Dust.NewDustPerfect(target.Center, DustID.PinkFairy,
                    Main.rand.NextVector2Circular(3f, 3f), 40, new Color(255, 150, 200), 0.7f);
                pink.noGravity = true;
            }
        }

        protected override void OnDashHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.ShadowFlame, 300);

            for (int i = 0; i < 16; i++)
            {
                Dust burst = Dust.NewDustPerfect(target.Center, DustID.ShadowbeamStaff,
                    Main.rand.NextVector2CircularEdge(8f, 8f), 20, default, Main.rand.NextFloat(1.2f, 1.8f));
                burst.noGravity = true;
            }
        }
    }
}
