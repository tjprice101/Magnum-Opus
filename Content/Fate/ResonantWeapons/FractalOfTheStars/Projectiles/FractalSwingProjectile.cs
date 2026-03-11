using System;
using MagnumOpus.Common.BaseClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.ResonantWeapons.FractalOfTheStars.Projectiles
{
    /// <summary>
    /// Fractal of the Stars — Fate theme melee. Exoblade-architecture swing.
    /// Stellar icy blue/white trail with cosmic indigo edges and starlight shimmer.
    /// </summary>
    public class FractalSwingProjectile : ExobladeStyleSwing
    {
        protected override float BladeLength => 106f;
        protected override int BaseSwingFrames => 76;
        protected override Color SlashPrimaryColor => new Color(160, 200, 255);
        protected override Color SlashSecondaryColor => new Color(20, 20, 60);
        protected override Color SlashAccentColor => new Color(255, 255, 230);

        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/FractalOfTheStars";

        protected override Color GetLensFlareColor(float p)
            => Color.Lerp(new Color(140, 180, 255), new Color(255, 255, 240), (float)Math.Pow(p, 2));

        protected override Color GetSwingDustColor()
        {
            float t = Main.rand.NextFloat();
            return t < 0.5f
                ? Color.Lerp(new Color(120, 180, 255), new Color(200, 220, 255), Main.rand.NextFloat())
                : Color.Lerp(new Color(180, 160, 255), new Color(255, 255, 240), Main.rand.NextFloat());
        }

        protected override void OnSwingFrame()
        {
            // Stellar sparkle dust along the blade
            if (Progression > 0.3f && Progression < 0.85f && Main.rand.NextFloat() < 0.35f)
            {
                Vector2 pos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * Main.rand.NextFloat(0.3f, 1f);
                Vector2 vel = SwordDirection.RotatedByRandom(0.6f) * Main.rand.NextFloat(0.8f, 2.5f);
                Dust star = Dust.NewDustPerfect(pos, DustID.BlueFairy, vel, 60, new Color(180, 220, 255), Main.rand.NextFloat(0.5f, 0.9f));
                star.noGravity = true;
                star.fadeIn = 0.7f;
            }

            // Cold white sparkle at tip
            if (Progression > 0.4f && Main.rand.NextFloat() < 0.15f)
            {
                Vector2 tip = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale;
                Dust glow = Dust.NewDustPerfect(tip, DustID.WhiteTorch,
                    Main.rand.NextVector2Circular(1f, 1f), 0, default, 0.5f);
                glow.noGravity = true;
            }
        }

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn, 180);

            for (int i = 0; i < 8; i++)
            {
                Color c = i % 2 == 0 ? new Color(140, 200, 255) : new Color(200, 180, 255);
                Dust burst = Dust.NewDustPerfect(target.Center, DustID.BlueFairy,
                    Main.rand.NextVector2CircularEdge(5f, 5f), 40, c, Main.rand.NextFloat(0.7f, 1.1f));
                burst.noGravity = true;
            }
        }

        protected override void OnDashHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn2, 300);

            for (int i = 0; i < 14; i++)
            {
                Dust burst = Dust.NewDustPerfect(target.Center, DustID.BlueFairy,
                    Main.rand.NextVector2CircularEdge(8f, 8f), 20, default, Main.rand.NextFloat(1f, 1.6f));
                burst.noGravity = true;
            }
        }
    }
}
