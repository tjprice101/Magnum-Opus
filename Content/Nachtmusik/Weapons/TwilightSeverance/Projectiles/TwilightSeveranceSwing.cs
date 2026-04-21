using System;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.Nachtmusik.Debuffs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Nachtmusik.Weapons.TwilightSeverance.Projectiles
{
    /// <summary>
    /// Twilight Severance — Nachtmusik theme melee. Exoblade-architecture swing.
    /// Indigo trail with starlight silver sparkles and deep night accents.
    /// </summary>
    public class TwilightSeveranceSwing : ExobladeStyleSwing
    {
        protected override bool SupportsDash => false;

        protected override float BladeLength => 105f;
        protected override int BaseSwingFrames => 78;
        protected override float TextureDrawScale => 0.85f;
        protected override string GradientLUTPath => "MagnumOpus/Assets/VFX Asset Library/ColorGradients/NachtmusikGradientLUTandRAMP";
        protected override Color SlashPrimaryColor => NachtmusikPalette.StarlitBlue;
        protected override Color SlashSecondaryColor => NachtmusikPalette.MidnightBlue;
        protected override Color SlashAccentColor => NachtmusikPalette.StarWhite;

        public override string Texture => "MagnumOpus/Content/Nachtmusik/Weapons/TwilightSeverance/TwilightSeverance";

        protected override Color GetLensFlareColor(float p)
            => Color.Lerp(NachtmusikPalette.ConstellationBlue, NachtmusikPalette.StarWhite, (float)Math.Pow(p, 2));

        protected override Color GetSwingDustColor()
        {
            float t = Main.rand.NextFloat();
            return t < 0.5f
                ? Color.Lerp(NachtmusikPalette.StarlitBlue, NachtmusikPalette.StarWhite, Main.rand.NextFloat())
                : Color.Lerp(NachtmusikPalette.ConstellationBlue, NachtmusikPalette.MoonlitSilver, Main.rand.NextFloat());
        }

        protected override void OnSwingFrame()
        {
            if (Progression > 0.3f && Progression < 0.85f && Main.rand.NextFloat() < 0.35f)
            {
                Vector2 pos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * Main.rand.NextFloat(0.4f, 1f);
                Vector2 vel = SwordDirection.RotatedBy(MathHelper.PiOver2 * Direction * Main.rand.NextFloat(0.3f, 1.2f)) * Main.rand.NextFloat(1f, 3f);
                vel.Y -= Main.rand.NextFloat(0.5f, 1.5f);
                Dust sparkle = Dust.NewDustPerfect(pos, DustID.WhiteTorch, vel, 80, default, Main.rand.NextFloat(0.6f, 1f));
                sparkle.noGravity = true;
                sparkle.fadeIn = 0.8f;
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
            // Spawn homing orb toward target
            Vector2 orbVel = (target.Center - Owner.Center).SafeNormalize(Vector2.UnitX) * 10f;
            GenericHomingOrbChild.SpawnChild(
                Projectile.GetSource_FromThis(),
                Owner.MountedCenter, orbVel,
                Projectile.damage, Projectile.knockBack, Projectile.owner,
                homingStrength: 0.04f,
                behaviorFlags: 0,
                themeIndex: GenericHomingOrbChild.THEME_NACHTMUSIK,
                scaleMult: 1f,
                timeLeft: 120);

            // Apply CelestialHarmony from melee hit
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 600);
            target.GetGlobalNPC<CelestialHarmonyNPC>().AddStack(target, 1);

            // Impact VFX — golden + purple dust burst
            for (int i = 0; i < 6; i++)
            {
                Color dustColor = Main.rand.NextBool() ? NachtmusikPalette.RadianceGold : NachtmusikPalette.CosmicPurple;
                Dust sparkle = Dust.NewDustPerfect(target.Center, DustID.WhiteTorch,
                    Main.rand.NextVector2CircularEdge(4f, 4f), 60, dustColor, Main.rand.NextFloat(0.8f, 1.2f));
                sparkle.noGravity = true;
                sparkle.fadeIn = 1f;
            }
            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = (target.Center - Owner.Center).SafeNormalize(Vector2.UnitX).RotatedByRandom(0.5f) * Main.rand.NextFloat(3f, 6f);
                Dust spark = Dust.NewDustPerfect(target.Center, DustID.WhiteTorch, vel, 0, NachtmusikPalette.StarlitBlue, 0.6f);
                spark.noGravity = true;
            }
        }

        protected override void OnDashHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 12; i++)
            {
                Dust sparkle = Dust.NewDustPerfect(target.Center, DustID.WhiteTorch,
                    Main.rand.NextVector2CircularEdge(6f, 6f), 40, default, Main.rand.NextFloat(1f, 1.5f));
                sparkle.noGravity = true;
            }
        }
    }
}
