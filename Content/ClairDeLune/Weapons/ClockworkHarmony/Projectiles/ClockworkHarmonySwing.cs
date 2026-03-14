using System;
using MagnumOpus.Common.BaseClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.ClockworkHarmony.Projectiles
{
    /// <summary>
    /// Clockwork Harmony — Clair de Lune theme melee. Exoblade-architecture swing.
    /// Moonlit gear-tinted trail with pearl white highlights and soft blue shimmer.
    /// </summary>
    public class ClockworkHarmonySwing : ExobladeStyleSwing
    {
        protected override float BladeLength => 100f;
        protected override int BaseSwingFrames => 76;
        protected override float TextureDrawScale => 0.89f;
        protected override string GradientLUTPath => "MagnumOpus/Assets/VFX Asset Library/ColorGradients/ClairDeLuneGradientLUTandRAMP";
        protected override Color SlashPrimaryColor => new Color(150, 200, 255);
        protected override Color SlashSecondaryColor => new Color(25, 35, 75);
        protected override Color SlashAccentColor => new Color(230, 240, 255);

        public override string Texture => "MagnumOpus/Content/ClairDeLune/Weapons/ClockworkHarmony/ClockworkHarmony";

        protected override Color GetLensFlareColor(float p)
            => Color.Lerp(new Color(130, 180, 245), new Color(235, 245, 255), (float)Math.Pow(p, 2));

        protected override Color GetSwingDustColor()
        {
            float t = Main.rand.NextFloat();
            return t < 0.5f
                ? Color.Lerp(new Color(120, 170, 230), new Color(170, 210, 250), Main.rand.NextFloat())
                : Color.Lerp(new Color(150, 200, 255), new Color(230, 240, 255), Main.rand.NextFloat());
        }

        protected override void OnSwingFrame()
        {
            if (Progression > 0.3f && Progression < 0.85f && Main.rand.NextFloat() < 0.35f)
            {
                Vector2 pos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * Main.rand.NextFloat(0.4f, 1f);
                Vector2 vel = SwordDirection.RotatedBy(MathHelper.PiOver2 * Direction * Main.rand.NextFloat(0.3f, 1.2f)) * Main.rand.NextFloat(1f, 3f);
                vel.Y -= Main.rand.NextFloat(0.5f, 1.5f);
                Dust gear = Dust.NewDustPerfect(pos, DustID.WhiteTorch, vel, 80,
                    ClairDeLunePalette.GetClockworkGradient(Main.rand.NextFloat()), Main.rand.NextFloat(0.6f, 1f));
                gear.noGravity = true;
                gear.fadeIn = 0.8f;
            }
            if (Progression > 0.4f && Main.rand.NextFloat() < 0.15f)
            {
                Vector2 tip = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale;
                Dust spark = Dust.NewDustPerfect(tip, DustID.IceTorch, Main.rand.NextVector2Circular(1f, 1f), 0, default, 0.5f);
                spark.noGravity = true;
            }
        }

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 6; i++)
            {
                Dust ice = Dust.NewDustPerfect(target.Center, DustID.IceTorch,
                    Main.rand.NextVector2CircularEdge(4f, 4f), 60, default, Main.rand.NextFloat(0.8f, 1.2f));
                ice.noGravity = true;
                ice.fadeIn = 1f;
            }
            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = (target.Center - Owner.Center).SafeNormalize(Vector2.UnitX).RotatedByRandom(0.5f) * Main.rand.NextFloat(3f, 6f);
                Dust spark = Dust.NewDustPerfect(target.Center, DustID.WhiteTorch, vel, 0,
                    ClairDeLunePalette.SoftBlue, 0.6f);
                spark.noGravity = true;
            }
            target.AddBuff(BuffID.Frostburn, 180);
        }

        protected override void OnDashHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 12; i++)
            {
                Dust ice = Dust.NewDustPerfect(target.Center, DustID.IceTorch,
                    Main.rand.NextVector2CircularEdge(6f, 6f), 40, default, Main.rand.NextFloat(1f, 1.5f));
                ice.noGravity = true;
            }
            target.AddBuff(BuffID.Frostburn, 300);
        }
    }
}
