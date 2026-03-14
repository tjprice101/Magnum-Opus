using System;
using MagnumOpus.Common.BaseClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Projectiles
{
    /// <summary>
    /// ThornboundReckoning swing — Ode to Joy theme melee. Exoblade-architecture swing.
    /// Verdant green trail with scattered leaf dust and golden pollen highlights.
    /// </summary>
    public class ThornboundSwingProj : ExobladeStyleSwing
    {
        protected override float BladeLength => 105f;
        protected override int BaseSwingFrames => 78;
        protected override float TextureDrawScale => 0.96f;
        protected override string GradientLUTPath => "MagnumOpus/Assets/VFX Asset Library/ColorGradients/OdeToJoyGradientLUTandRAMP";
        protected override Color SlashPrimaryColor => new Color(90, 200, 60);
        protected override Color SlashSecondaryColor => new Color(30, 55, 20);
        protected override Color SlashAccentColor => new Color(255, 210, 60);

        public override string Texture => "MagnumOpus/Content/OdeToJoy/Weapons/ThornboundReckoning/ThornboundReckoning";

        protected override Color GetLensFlareColor(float p)
            => Color.Lerp(new Color(60, 180, 40), new Color(255, 220, 100), (float)Math.Pow(p, 2));

        protected override Color GetSwingDustColor()
        {
            float t = Main.rand.NextFloat();
            return t < 0.5f
                ? Color.Lerp(new Color(80, 190, 50), new Color(120, 220, 80), Main.rand.NextFloat())
                : Color.Lerp(new Color(60, 170, 40), new Color(255, 210, 60), Main.rand.NextFloat());
        }

        protected override void OnSwingFrame()
        {
            if (Progression > 0.3f && Progression < 0.85f && Main.rand.NextFloat() < 0.35f)
            {
                Vector2 pos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * Main.rand.NextFloat(0.4f, 1f);
                Vector2 vel = SwordDirection.RotatedBy(MathHelper.PiOver2 * Direction * Main.rand.NextFloat(0.3f, 1.2f)) * Main.rand.NextFloat(1f, 3f);
                vel.Y -= Main.rand.NextFloat(0.5f, 1.5f);
                Dust petal = Dust.NewDustPerfect(pos, DustID.GreenTorch, vel, 80, default, Main.rand.NextFloat(0.6f, 1f));
                petal.noGravity = true;
                petal.fadeIn = 0.8f;
            }
            if (Progression > 0.4f && Main.rand.NextFloat() < 0.15f)
            {
                Vector2 tip = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale;
                Dust spark = Dust.NewDustPerfect(tip, DustID.GoldFlame, Main.rand.NextVector2Circular(1f, 1f), 0, default, 0.5f);
                spark.noGravity = true;
            }
        }

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 6; i++)
            {
                Dust petal = Dust.NewDustPerfect(target.Center, DustID.GreenTorch,
                    Main.rand.NextVector2CircularEdge(4f, 4f), 60, default, Main.rand.NextFloat(0.8f, 1.2f));
                petal.noGravity = true;
                petal.fadeIn = 1f;
            }
            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = (target.Center - Owner.Center).SafeNormalize(Vector2.UnitX).RotatedByRandom(0.5f) * Main.rand.NextFloat(3f, 6f);
                Dust spark = Dust.NewDustPerfect(target.Center, DustID.GoldFlame, vel, 0, default, 0.6f);
                spark.noGravity = true;
            }
        }

        protected override void OnDashHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 12; i++)
            {
                Dust petal = Dust.NewDustPerfect(target.Center, DustID.GreenTorch,
                    Main.rand.NextVector2CircularEdge(6f, 6f), 40, default, Main.rand.NextFloat(1f, 1.5f));
                petal.noGravity = true;
            }
        }
    }
}
