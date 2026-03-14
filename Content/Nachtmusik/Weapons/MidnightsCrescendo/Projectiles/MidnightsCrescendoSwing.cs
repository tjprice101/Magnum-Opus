using System;
using MagnumOpus.Common.BaseClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Nachtmusik.Weapons.MidnightsCrescendo.Projectiles
{
    /// <summary>
    /// Midnight's Crescendo — Nachtmusik theme melee. Exoblade-architecture swing.
    /// Starlight shimmer trail with indigo sparkles and silver accents.
    /// </summary>
    public class MidnightsCrescendoSwing : ExobladeStyleSwing
    {
        protected override float BladeLength => 100f;
        protected override int BaseSwingFrames => 75;
        protected override float TextureDrawScale => 0.85f;
        protected override string GradientLUTPath => "MagnumOpus/Assets/VFX Asset Library/ColorGradients/NachtmusikGradientLUTandRAMP";
        protected override Color SlashPrimaryColor => new Color(100, 120, 220);
        protected override Color SlashSecondaryColor => new Color(20, 15, 50);
        protected override Color SlashAccentColor => new Color(180, 200, 255);

        public override string Texture => "MagnumOpus/Content/Nachtmusik/Weapons/MidnightsCrescendo/MidnightsCrescendo";

        protected override Color GetLensFlareColor(float p)
            => Color.Lerp(new Color(80, 100, 200), new Color(200, 220, 255), (float)Math.Pow(p, 2));

        protected override Color GetSwingDustColor()
        {
            float t = Main.rand.NextFloat();
            return t < 0.5f
                ? Color.Lerp(new Color(100, 120, 220), new Color(180, 200, 255), Main.rand.NextFloat())
                : Color.Lerp(new Color(80, 100, 200), new Color(200, 220, 255), Main.rand.NextFloat());
        }

        protected override void OnSwingFrame()
        {
            if (Progression > 0.3f && Progression < 0.85f && Main.rand.NextFloat() < 0.35f)
            {
                Vector2 pos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * Main.rand.NextFloat(0.4f, 1f);
                Vector2 vel = SwordDirection.RotatedBy(MathHelper.PiOver2 * Direction * Main.rand.NextFloat(0.3f, 1.2f)) * Main.rand.NextFloat(1f, 3f);
                vel.Y -= Main.rand.NextFloat(0.5f, 1.5f);
                Dust shimmer = Dust.NewDustPerfect(pos, DustID.WhiteTorch, vel, 80, default, Main.rand.NextFloat(0.6f, 1f));
                shimmer.noGravity = true;
                shimmer.fadeIn = 0.8f;
            }
            if (Progression > 0.4f && Main.rand.NextFloat() < 0.15f)
            {
                Vector2 tip = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale;
                Dust spark = Dust.NewDustPerfect(tip, DustID.BlueTorch, Main.rand.NextVector2Circular(1f, 1f), 0, default, 0.5f);
                spark.noGravity = true;
            }
        }

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 6; i++)
            {
                Dust shimmer = Dust.NewDustPerfect(target.Center, DustID.WhiteTorch,
                    Main.rand.NextVector2CircularEdge(4f, 4f), 60, default, Main.rand.NextFloat(0.8f, 1.2f));
                shimmer.noGravity = true;
                shimmer.fadeIn = 1f;
            }
            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = (target.Center - Owner.Center).SafeNormalize(Vector2.UnitX).RotatedByRandom(0.5f) * Main.rand.NextFloat(3f, 6f);
                Dust spark = Dust.NewDustPerfect(target.Center, DustID.BlueTorch, vel, 0, default, 0.6f);
                spark.noGravity = true;
            }
        }

        protected override void OnDashHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 12; i++)
            {
                Dust shimmer = Dust.NewDustPerfect(target.Center, DustID.WhiteTorch,
                    Main.rand.NextVector2CircularEdge(6f, 6f), 40, default, Main.rand.NextFloat(1f, 1.5f));
                shimmer.noGravity = true;
            }
        }
    }
}
