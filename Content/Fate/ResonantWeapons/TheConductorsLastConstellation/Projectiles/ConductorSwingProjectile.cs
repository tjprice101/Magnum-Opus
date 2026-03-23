using System;
using MagnumOpus.Common.BaseClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.ResonantWeapons.TheConductorsLastConstellation.Projectiles
{
    /// <summary>
    /// The Conductor's Last Constellation — Fate theme melee. Exoblade-architecture swing.
    /// Cosmic dark pink / celestial white trail with stellar blue highlights.
    /// </summary>
    public class ConductorSwingProjectile : ExobladeStyleSwing
    {
        protected override bool SupportsDash => false;

        protected override float BladeLength => 115f;
        protected override int BaseSwingFrames => 78;
        protected override Color SlashPrimaryColor => new Color(180, 60, 120);
        protected override Color SlashSecondaryColor => new Color(20, 10, 40);
        protected override Color SlashAccentColor => new Color(255, 220, 240);

        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/TheConductorsLastConstellation";

        protected override Color GetLensFlareColor(float p)
            => Color.Lerp(new Color(200, 80, 140), new Color(255, 230, 255), (float)Math.Pow(p, 2));

        protected override Color GetSwingDustColor()
        {
            float t = Main.rand.NextFloat();
            return t < 0.4f
                ? Color.Lerp(new Color(180, 50, 100), new Color(255, 100, 160), Main.rand.NextFloat())
                : Color.Lerp(new Color(100, 180, 255), new Color(220, 200, 255), Main.rand.NextFloat());
        }

        protected override void OnSwingFrame()
        {
            // Cosmic star dust along swing arc
            if (Progression > 0.3f && Progression < 0.85f && Main.rand.NextFloat() < 0.4f)
            {
                Vector2 pos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * Main.rand.NextFloat(0.3f, 1f);
                Vector2 vel = SwordDirection.RotatedByRandom(0.7f) * Main.rand.NextFloat(0.8f, 2.5f);
                Dust star = Dust.NewDustPerfect(pos, DustID.PinkFairy, vel, 60, default, Main.rand.NextFloat(0.5f, 0.9f));
                star.noGravity = true;
                star.fadeIn = 0.7f;
            }

            // Celestial white sparkle at blade tip
            if (Progression > 0.4f && Main.rand.NextFloat() < 0.2f)
            {
                Vector2 tip = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale;
                Dust glow = Dust.NewDustPerfect(tip, DustID.WhiteTorch,
                    Main.rand.NextVector2Circular(1.5f, 1.5f), 0, default, 0.6f);
                glow.noGravity = true;
            }
        }

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 10; i++)
            {
                Color c = i % 2 == 0 ? new Color(200, 80, 140) : new Color(120, 180, 255);
                Dust burst = Dust.NewDustPerfect(target.Center, DustID.PinkFairy,
                    Main.rand.NextVector2CircularEdge(6f, 6f), 40, c, Main.rand.NextFloat(0.8f, 1.3f));
                burst.noGravity = true;
            }

            // Spawn sword beam sub-projectile on hit
            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 dir = (target.Center - Owner.Center).SafeNormalize(Vector2.UnitX);
                int beamType = ModContent.ProjectileType<ConductorSwordBeam>();
                if (beamType > 0)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, dir * 8f,
                        beamType, Projectile.damage / 2, 2f, Projectile.owner);
                }
            }
        }

        protected override void OnDashHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 16; i++)
            {
                Dust burst = Dust.NewDustPerfect(target.Center, DustID.PinkFairy,
                    Main.rand.NextVector2CircularEdge(9f, 9f), 20, default, Main.rand.NextFloat(1.2f, 1.8f));
                burst.noGravity = true;
            }
        }
    }
}
