using System;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.Nachtmusik.Debuffs;
using MagnumOpus.Content.Nachtmusik.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Nachtmusik.Weapons.MidnightsCrescendo.Projectiles
{
    /// <summary>
    /// Midnight's Crescendo — Ascending Crescendo combo.
    /// Phase 0 (Piano): 1 orb. Phase 1 (Mezzo-Forte): 2 orbs spread.
    /// Phase 2 (Fortissimo): 3 orbs spread with pierce.
    /// Phase 3 (Finale): 1 large splitting orb.
    /// </summary>
    public class MidnightsCrescendoSwing : ExobladeStyleSwing
    {
        protected override bool SupportsDash => false;

        protected override float BladeLength => 100f;
        protected override int BaseSwingFrames => 75;
        protected override float TextureDrawScale => 0.85f;
        protected override string GradientLUTPath => "MagnumOpus/Assets/VFX Asset Library/ColorGradients/NachtmusikGradientLUTandRAMP";
        protected override Color SlashPrimaryColor => NachtmusikPalette.StarlitBlue;
        protected override Color SlashSecondaryColor => NachtmusikPalette.MidnightBlue;
        protected override Color SlashAccentColor => NachtmusikPalette.StarWhite;

        public override string Texture => "MagnumOpus/Content/Nachtmusik/Weapons/MidnightsCrescendo/MidnightsCrescendo";

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
                Color dustCol = NachtmusikPalette.StarGold;
                Dust shimmer = Dust.NewDustPerfect(pos, DustID.WhiteTorch, vel, 80, dustCol, Main.rand.NextFloat(0.6f, 1f));
                shimmer.noGravity = true;
                shimmer.fadeIn = 0.8f;
            }
            if (Progression > 0.4f && Main.rand.NextFloat() < 0.15f)
            {
                Vector2 tip = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale;
                Dust spark = Dust.NewDustPerfect(tip, DustID.WhiteTorch, Main.rand.NextVector2Circular(1f, 1f), 0, NachtmusikPalette.CosmicPurple, 0.5f);
                spark.noGravity = true;
            }
        }

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply CelestialHarmony
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 600);
            target.GetGlobalNPC<CelestialHarmonyNPC>().AddStack(target, 1);

            // Get and advance combo phase
            var combatPlayer = Owner.GetModPlayer<NachtmusikCombatPlayer>();
            int phase = combatPlayer.MidnightCrescendoComboPhase;
            combatPlayer.MidnightCrescendoComboPhase = (phase + 1) % 4;

            Vector2 toTarget = (target.Center - Owner.Center).SafeNormalize(Vector2.UnitX);

            switch (phase)
            {
                case 0: // Piano: 1 gentle orb
                    GenericHomingOrbChild.SpawnChild(
                        Projectile.GetSource_FromThis(), Owner.MountedCenter,
                        toTarget * 8f, Projectile.damage / 2, Projectile.knockBack, Projectile.owner,
                        0.04f, 0, GenericHomingOrbChild.THEME_NACHTMUSIK, 0.8f, 90);
                    break;

                case 1: // Mezzo-Forte: 2 orbs ±15° spread
                    for (int i = -1; i <= 1; i += 2)
                    {
                        Vector2 orbVel = toTarget.RotatedBy(0.26f * i) * 12f;
                        GenericHomingOrbChild.SpawnChild(
                            Projectile.GetSource_FromThis(), Owner.MountedCenter,
                            orbVel, Projectile.damage / 2, Projectile.knockBack, Projectile.owner,
                            0.06f, 0, GenericHomingOrbChild.THEME_NACHTMUSIK, 0.9f, 100);
                    }
                    break;

                case 2: // Fortissimo: 3 orbs ±20° with pierce
                    for (int i = -1; i <= 1; i++)
                    {
                        Vector2 orbVel = toTarget.RotatedBy(0.35f * i) * 16f;
                        GenericHomingOrbChild.SpawnChild(
                            Projectile.GetSource_FromThis(), Owner.MountedCenter,
                            orbVel, Projectile.damage / 2, Projectile.knockBack, Projectile.owner,
                            0.08f, GenericHomingOrbChild.FLAG_PIERCE,
                            GenericHomingOrbChild.THEME_NACHTMUSIK, 1f, 110);
                    }
                    break;

                case 3: // Finale: 1 large splitting orb
                    GenericHomingOrbChild.SpawnChild(
                        Projectile.GetSource_FromThis(), Owner.MountedCenter,
                        toTarget * 10f, Projectile.damage, Projectile.knockBack, Projectile.owner,
                        0.12f, GenericHomingOrbChild.FLAG_ZONE_ON_KILL,
                        GenericHomingOrbChild.THEME_NACHTMUSIK, 1.5f, 150);
                    break;
            }

            // Impact VFX — intensity scales with phase
            int dustCount = 4 + phase * 2;
            for (int i = 0; i < dustCount; i++)
            {
                Color col = Main.rand.NextBool() ? NachtmusikPalette.RadianceGold : NachtmusikPalette.StarlitBlue;
                Dust d = Dust.NewDustPerfect(target.Center, DustID.WhiteTorch,
                    Main.rand.NextVector2CircularEdge(3f + phase, 3f + phase), 0, col, 0.7f + phase * 0.15f);
                d.noGravity = true;
            }
        }

        protected override void OnDashHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 600);
            target.GetGlobalNPC<CelestialHarmonyNPC>().AddStack(target, 2);

            for (int i = 0; i < 12; i++)
            {
                Color col = i % 2 == 0 ? NachtmusikPalette.RadianceGold : NachtmusikPalette.CosmicPurple;
                Dust shimmer = Dust.NewDustPerfect(target.Center, DustID.WhiteTorch,
                    Main.rand.NextVector2CircularEdge(6f, 6f), 40, col, Main.rand.NextFloat(1f, 1.5f));
                shimmer.noGravity = true;
            }
        }
    }
}
