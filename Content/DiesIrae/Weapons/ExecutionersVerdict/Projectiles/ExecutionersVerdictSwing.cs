using System;
using MagnumOpus.Common.BaseClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict.Projectiles
{
    /// <summary>
    /// Executioner's Verdict — Dies Irae theme melee. Exoblade-architecture swing.
    /// Judgment gold trail with golden sparks and flame accent highlights.
    /// </summary>
    public class ExecutionersVerdictSwing : ExobladeStyleSwing
    {
        protected override float BladeLength => 108f;
        protected override int BaseSwingFrames => 76;
        protected override float TextureDrawScale => 0.98f;
        protected override string GradientLUTPath => "MagnumOpus/Assets/VFX Asset Library/ColorGradients/DiesIraeGradientLUTandRAMP";
        protected override Color SlashPrimaryColor => new Color(255, 200, 80);
        protected override Color SlashSecondaryColor => new Color(100, 40, 10);
        protected override Color SlashAccentColor => new Color(255, 100, 40);

        public override string Texture => "MagnumOpus/Content/DiesIrae/Weapons/ExecutionersVerdict/ExecutionersVerdict";

        protected override Color GetLensFlareColor(float p)
            => Color.Lerp(new Color(255, 200, 80), new Color(255, 100, 40), (float)Math.Pow(p, 2));

        protected override Color GetSwingDustColor()
        {
            float t = Main.rand.NextFloat();
            return t < 0.5f
                ? Color.Lerp(new Color(255, 200, 80), new Color(255, 220, 120), Main.rand.NextFloat())
                : Color.Lerp(new Color(255, 100, 40), new Color(200, 150, 30), Main.rand.NextFloat());
        }

        protected override void OnSwingFrame()
        {
            if (Progression > 0.3f && Progression < 0.85f && Main.rand.NextFloat() < 0.35f)
            {
                Vector2 pos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * Main.rand.NextFloat(0.4f, 1f);
                Vector2 vel = SwordDirection.RotatedBy(MathHelper.PiOver2 * Direction * Main.rand.NextFloat(0.3f, 1.2f)) * Main.rand.NextFloat(1f, 3f);
                vel.Y -= Main.rand.NextFloat(0.5f, 1.5f);
                Dust spark = Dust.NewDustPerfect(pos, DustID.GoldFlame, vel, 0, default, Main.rand.NextFloat(0.6f, 1f));
                spark.noGravity = true;
                spark.fadeIn = 0.8f;
            }
            if (Progression > 0.4f && Main.rand.NextFloat() < 0.15f)
            {
                Vector2 tip = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale;
                Dust ember = Dust.NewDustPerfect(tip, DustID.Torch, Main.rand.NextVector2Circular(1f, 1f), 0, default, 0.5f);
                ember.noGravity = true;
            }
        }

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 6; i++)
            {
                Dust gold = Dust.NewDustPerfect(target.Center, DustID.GoldFlame,
                    Main.rand.NextVector2CircularEdge(4f, 4f), 0, default, Main.rand.NextFloat(0.8f, 1.2f));
                gold.noGravity = true;
            }
            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = (target.Center - Owner.Center).SafeNormalize(Vector2.UnitX).RotatedByRandom(0.5f) * Main.rand.NextFloat(3f, 6f);
                Dust fire = Dust.NewDustPerfect(target.Center, DustID.Torch, vel, 0, default, 0.6f);
                fire.noGravity = true;
            }
        }

        protected override void OnDashHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 12; i++)
            {
                Dust gold = Dust.NewDustPerfect(target.Center, DustID.GoldFlame,
                    Main.rand.NextVector2CircularEdge(6f, 6f), 0, default, Main.rand.NextFloat(1f, 1.5f));
                gold.noGravity = true;
            }
        }
    }
}
