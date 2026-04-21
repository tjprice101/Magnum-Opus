using System;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.Nachtmusik.Debuffs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Nachtmusik.Weapons.NocturnalExecutioner.Projectiles
{
    /// <summary>
    /// Nocturnal Executioner — Day/Night behavior shift.
    /// Day: 5 orbs in spread. Night: 1 aggressive homing orb + zone on kill.
    /// </summary>
    public class NocturnalExecutionerSwing : ExobladeStyleSwing
    {
        protected override bool SupportsDash => false;

        protected override float BladeLength => 115f;
        protected override int BaseSwingFrames => 82;
        protected override float TextureDrawScale => 0.81f;
        protected override string GradientLUTPath => "MagnumOpus/Assets/VFX Asset Library/ColorGradients/NachtmusikGradientLUTandRAMP";
        protected override Color SlashPrimaryColor => NachtmusikPalette.StarlitBlue;
        protected override Color SlashSecondaryColor => NachtmusikPalette.MidnightBlue;
        protected override Color SlashAccentColor => NachtmusikPalette.StarWhite;

        public override string Texture => "MagnumOpus/Content/Nachtmusik/Weapons/NocturnalExecutioner/NocturnalExecutioner";

        protected override Color GetLensFlareColor(float p)
            => Color.Lerp(NachtmusikPalette.ConstellationBlue, NachtmusikPalette.StarWhite, (float)Math.Pow(p, 2));

        protected override Color GetSwingDustColor()
        {
            float t = Main.rand.NextFloat();
            return t < 0.4f
                ? Color.Lerp(NachtmusikPalette.MidnightBlue, NachtmusikPalette.StarlitBlue, Main.rand.NextFloat())
                : Color.Lerp(NachtmusikPalette.ConstellationBlue, NachtmusikPalette.MoonlitSilver, Main.rand.NextFloat());
        }

        protected override void OnSwingFrame()
        {
            if (Progression > 0.3f && Progression < 0.85f && Main.rand.NextFloat() < 0.4f)
            {
                Vector2 pos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * Main.rand.NextFloat(0.3f, 1f);
                Vector2 vel = SwordDirection.RotatedByRandom(0.6f) * Main.rand.NextFloat(1.5f, 3.5f);
                vel.Y -= Main.rand.NextFloat(0.5f, 2f);
                Color dustCol = Main.dayTime ? NachtmusikPalette.StarGold : NachtmusikPalette.CosmicPurple;
                Dust ember = Dust.NewDustPerfect(pos, DustID.WhiteTorch, vel, 0, dustCol, Main.rand.NextFloat(0.8f, 1.3f));
                ember.noGravity = true;
                ember.fadeIn = 0.6f;
            }
            if (Progression > 0.5f && Main.rand.NextFloat() < 0.2f)
            {
                Vector2 tip = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale;
                Dust star = Dust.NewDustPerfect(tip, DustID.WhiteTorch, Main.rand.NextVector2Circular(2f, 2f), 0, NachtmusikPalette.RadianceGold, 0.7f);
                star.noGravity = true;
            }
        }

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply CelestialHarmony
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 600);
            target.GetGlobalNPC<CelestialHarmonyNPC>().AddStack(target, 1);

            Vector2 toTarget = (target.Center - Owner.Center).SafeNormalize(Vector2.UnitX);

            if (Main.dayTime)
            {
                // Day: 5 orbs in ±30° spread, short-lived
                for (int i = 0; i < 5; i++)
                {
                    float spreadAngle = MathHelper.Lerp(-0.52f, 0.52f, i / 4f); // ±30°
                    Vector2 orbVel = toTarget.RotatedBy(spreadAngle) * 12f;
                    GenericHomingOrbChild.SpawnChild(
                        Projectile.GetSource_FromThis(),
                        Owner.MountedCenter, orbVel,
                        Projectile.damage / 3, Projectile.knockBack, Projectile.owner,
                        homingStrength: 0.06f,
                        behaviorFlags: 0,
                        themeIndex: GenericHomingOrbChild.THEME_NACHTMUSIK,
                        scaleMult: 0.8f,
                        timeLeft: 60);
                }
            }
            else
            {
                // Night: 1 aggressive homing orb with zone on kill, applies 2 stacks
                Vector2 orbVel = toTarget * 10f;
                GenericHomingOrbChild.SpawnChild(
                    Projectile.GetSource_FromThis(),
                    Owner.MountedCenter, orbVel,
                    Projectile.damage, Projectile.knockBack, Projectile.owner,
                    homingStrength: 0.14f,
                    behaviorFlags: GenericHomingOrbChild.FLAG_ZONE_ON_KILL,
                    themeIndex: GenericHomingOrbChild.THEME_NACHTMUSIK,
                    scaleMult: 1.3f,
                    timeLeft: 240);
            }

            // Impact VFX
            for (int i = 0; i < 8; i++)
            {
                Color col = Main.rand.NextBool() ? NachtmusikPalette.RadianceGold : NachtmusikPalette.StarlitBlue;
                Dust frost = Dust.NewDustPerfect(target.Center, DustID.WhiteTorch,
                    Main.rand.NextVector2CircularEdge(5f, 5f), 0, col, Main.rand.NextFloat(1f, 1.4f));
                frost.noGravity = true;
            }
        }

        protected override void OnDashHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 600);
            target.GetGlobalNPC<CelestialHarmonyNPC>().AddStack(target, 2);

            for (int i = 0; i < 16; i++)
            {
                Color col = i % 3 == 0 ? NachtmusikPalette.RadianceGold : NachtmusikPalette.CosmicPurple;
                Dust frost = Dust.NewDustPerfect(target.Center, DustID.WhiteTorch,
                    Main.rand.NextVector2CircularEdge(8f, 8f), 0, col, Main.rand.NextFloat(1.2f, 1.8f));
                frost.noGravity = true;
            }
        }
    }
}
