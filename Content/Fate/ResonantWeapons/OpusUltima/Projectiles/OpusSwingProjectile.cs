using System;
using MagnumOpus.Common.BaseClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.ResonantWeapons.OpusUltima.Projectiles
{
    /// <summary>
    /// Opus Ultima — Fate theme melee. Exoblade-architecture swing.
    /// Warm gold/amber trail with luminous celestial white core and crimson edges.
    /// </summary>
    public class OpusSwingProjectile : ExobladeStyleSwing
    {
        protected override bool SupportsDash => false;

        protected override float BladeLength => 118f;
        protected override int BaseSwingFrames => 78;
        protected override Color SlashPrimaryColor => new Color(255, 160, 80);
        protected override Color SlashSecondaryColor => new Color(60, 20, 40);
        protected override Color SlashAccentColor => new Color(255, 240, 200);

        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/OpusUltima";

        protected override Color GetLensFlareColor(float p)
            => Color.Lerp(new Color(255, 180, 100), new Color(255, 250, 230), (float)Math.Pow(p, 2));

        protected override Color GetSwingDustColor()
        {
            float t = Main.rand.NextFloat();
            return t < 0.4f
                ? Color.Lerp(new Color(255, 180, 80), new Color(255, 220, 140), Main.rand.NextFloat())
                : Color.Lerp(new Color(200, 60, 60), new Color(255, 140, 80), Main.rand.NextFloat());
        }

        protected override void OnSwingFrame()
        {
            // Golden radiance dust along blade arc
            if (Progression > 0.3f && Progression < 0.85f && Main.rand.NextFloat() < 0.4f)
            {
                Vector2 pos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * Main.rand.NextFloat(0.3f, 1f);
                Vector2 vel = SwordDirection.RotatedByRandom(0.5f) * Main.rand.NextFloat(1f, 3f);
                Dust gold = Dust.NewDustPerfect(pos, DustID.GoldFlame, vel, 40, default, Main.rand.NextFloat(0.7f, 1.2f));
                gold.noGravity = true;
                gold.fadeIn = 0.7f;
            }

            // Warm celestial sparkle at tip
            if (Progression > 0.45f && Main.rand.NextFloat() < 0.2f)
            {
                Vector2 tip = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale;
                Dust spark = Dust.NewDustPerfect(tip, DustID.WhiteTorch,
                    Main.rand.NextVector2Circular(1.5f, 1.5f), 0, new Color(255, 240, 200), 0.6f);
                spark.noGravity = true;
            }
        }

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 10; i++)
            {
                Color c = i % 2 == 0 ? new Color(255, 200, 100) : new Color(180, 40, 80);
                Dust burst = Dust.NewDustPerfect(target.Center, DustID.GoldFlame,
                    Main.rand.NextVector2CircularEdge(6f, 6f), 20, c, Main.rand.NextFloat(0.8f, 1.4f));
                burst.noGravity = true;
            }
        }

        protected override void OnDashHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 16; i++)
            {
                Dust gold = Dust.NewDustPerfect(target.Center, DustID.GoldFlame,
                    Main.rand.NextVector2CircularEdge(9f, 9f), 10, default, Main.rand.NextFloat(1.2f, 1.8f));
                gold.noGravity = true;
            }
            for (int i = 0; i < 6; i++)
            {
                Dust white = Dust.NewDustPerfect(target.Center, DustID.WhiteTorch,
                    Main.rand.NextVector2Circular(3f, 3f), 0, default, 0.8f);
                white.noGravity = true;
            }
        }
    }
}
