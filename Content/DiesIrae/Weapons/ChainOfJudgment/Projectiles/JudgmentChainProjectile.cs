using System;
using MagnumOpus.Common.BaseClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.ChainOfJudgment.Projectiles
{
    /// <summary>
    /// Judgment Chain — Dies Irae theme melee. Exoblade-architecture swing.
    /// Blood red trail with ember dust and orange accent highlights.
    /// </summary>
    public class JudgmentChainProjectile : ExobladeStyleSwing
    {
        protected override bool SupportsDash => false;

        protected override float BladeLength => 95f;
        protected override int BaseSwingFrames => 72;
        protected override float TextureDrawScale => 0.93f;
        protected override string GradientLUTPath => "MagnumOpus/Assets/VFX Asset Library/ColorGradients/DiesIraeGradientLUTandRAMP";
        protected override Color SlashPrimaryColor => new Color(220, 40, 20);
        protected override Color SlashSecondaryColor => new Color(60, 5, 5);
        protected override Color SlashAccentColor => new Color(255, 140, 40);

        public override string Texture => "MagnumOpus/Content/DiesIrae/Weapons/ChainOfJudgment/ChainOfJudgment";

        protected override Color GetLensFlareColor(float p)
            => Color.Lerp(new Color(220, 40, 20), new Color(255, 140, 40), (float)Math.Pow(p, 2));

        protected override Color GetSwingDustColor()
        {
            float t = Main.rand.NextFloat();
            return t < 0.5f
                ? Color.Lerp(new Color(220, 40, 20), new Color(160, 10, 10), Main.rand.NextFloat())
                : Color.Lerp(new Color(255, 140, 40), new Color(255, 80, 30), Main.rand.NextFloat());
        }

        protected override void OnSwingFrame()
        {
            if (Progression > 0.3f && Progression < 0.85f && Main.rand.NextFloat() < 0.35f)
            {
                Vector2 pos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * Main.rand.NextFloat(0.4f, 1f);
                Vector2 vel = SwordDirection.RotatedBy(MathHelper.PiOver2 * Direction * Main.rand.NextFloat(0.3f, 1.2f)) * Main.rand.NextFloat(1f, 3f);
                vel.Y -= Main.rand.NextFloat(0.5f, 1.5f);
                Dust ember = Dust.NewDustPerfect(pos, DustID.Torch, vel, 80, default, Main.rand.NextFloat(0.6f, 1f));
                ember.noGravity = true;
                ember.fadeIn = 0.8f;
            }
            if (Progression > 0.4f && Main.rand.NextFloat() < 0.15f)
            {
                Vector2 tip = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale;
                Dust spark = Dust.NewDustPerfect(tip, DustID.SolarFlare, Main.rand.NextVector2Circular(1f, 1f), 0, default, 0.5f);
                spark.noGravity = true;
            }
        }

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 6; i++)
            {
                Dust fire = Dust.NewDustPerfect(target.Center, DustID.Torch,
                    Main.rand.NextVector2CircularEdge(4f, 4f), 60, default, Main.rand.NextFloat(0.8f, 1.2f));
                fire.noGravity = true;
                fire.fadeIn = 1f;
            }
            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = (target.Center - Owner.Center).SafeNormalize(Vector2.UnitX).RotatedByRandom(0.5f) * Main.rand.NextFloat(3f, 6f);
                Dust spark = Dust.NewDustPerfect(target.Center, DustID.SolarFlare, vel, 0, default, 0.6f);
                spark.noGravity = true;
            }
        }

        protected override void OnDashHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 12; i++)
            {
                Dust fire = Dust.NewDustPerfect(target.Center, DustID.Torch,
                    Main.rand.NextVector2CircularEdge(6f, 6f), 40, default, Main.rand.NextFloat(1f, 1.5f));
                fire.noGravity = true;
            }
        }
    }
}
