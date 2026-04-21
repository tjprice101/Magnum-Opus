using System;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Common.Systems;
using MagnumOpus.Content.ClairDeLune;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.Chronologicality.Projectiles
{
    /// <summary>
    /// Chronologicality — Clair de Lune theme melee. Exoblade-architecture swing.
    /// Wrath Escalation: 4-phase combo (1 orb straight → 2 orbs 0.06 homing → 3 orbs 0.08 homing + pierce → 1 orb 0.14 homing splits to 8).
    /// Right-click dash: pierce all orbs at 2x speed.
    /// </summary>
    public class ChronologicalitySwing : ExobladeStyleSwing
    {
        protected override bool SupportsDash => true;

        protected override float BladeLength => 102f;
        protected override int BaseSwingFrames => 76;
        protected override float TextureDrawScale => 0.91f;
        protected override string GradientLUTPath => "MagnumOpus/Assets/VFX Asset Library/ColorGradients/ClairDeLuneGradientLUTandRAMP";
        protected override Color SlashPrimaryColor => ClairDeLunePalette.PearlBlue;
        protected override Color SlashSecondaryColor => ClairDeLunePalette.MidnightBlue;
        protected override Color SlashAccentColor => ClairDeLunePalette.WhiteHot;

        public override string Texture => "MagnumOpus/Content/ClairDeLune/Weapons/Chronologicality/Chronologicality";

        protected override Color GetLensFlareColor(float p)
            => Color.Lerp(ClairDeLunePalette.PearlBlue, ClairDeLunePalette.WhiteHot, (float)Math.Pow(p, 2));

        protected override Color GetSwingDustColor()
        {
            float t = Main.rand.NextFloat();
            return t < 0.5f
                ? Color.Lerp(ClairDeLunePalette.SoftBlue, ClairDeLunePalette.PearlBlue, Main.rand.NextFloat())
                : Color.Lerp(ClairDeLunePalette.PearlBlue, ClairDeLunePalette.WhiteHot, Main.rand.NextFloat());
        }

        protected override void OnSwingFrame()
        {
            ClairDeLuneVFXLibrary.SwingFrameVFX(
                Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * 0.7f,
                -SwordDirection, (int)Progression, Projectile.timeLeft);
        }

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            ClairDeLuneVFXLibrary.MeleeImpact(target.Center, (int)Progression);

            // Wrath Escalation — fire orbs based on combo phase
            var combatPlayer = Owner.GetModPlayer<ClairDeLuneCombatPlayer>();

            if (Main.myPlayer == Projectile.owner)
            {
                switch (combatPlayer.ChronologicalityComboPhase)
                {
                    case 1: // 1 orb straight, no homing
                        FireOrbSpread(1, 0f, 0, 1.0f, 8f);
                        break;

                    case 2: // 2 orbs, 0.06 homing
                        FireOrbSpread(2, 0.06f, 0, 1.0f, 8f);
                        break;

                    case 3: // 3 orbs, 0.08 homing + pierce
                        FireOrbSpread(3, 0.08f, GenericHomingOrbChild.FLAG_PIERCE, 1.0f, 8f);
                        break;

                    case 4: // 1 orb, 0.14 homing, splits to 8 on kill
                        FireOrbSpread(1, 0.14f, GenericHomingOrbChild.FLAG_ZONE_ON_KILL, 1.2f, 10f);
                        break;
                }

                // Increment combo phase (will wrap back to 1 after phase 4)
                combatPlayer.ChronologicalityComboPhase++;
                if (combatPlayer.ChronologicalityComboPhase > 4)
                    combatPlayer.ChronologicalityComboPhase = 1;

                combatPlayer.ChronologicalityComboTimer = 90; // 1.5 seconds before reset
            }

            target.AddBuff(BuffID.Frostburn, 180);
        }

        protected override void OnDashHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Right-click dash: pierce all orb at 2x speed
            if (Main.myPlayer == Projectile.owner)
            {
                var combatPlayer = Owner.GetModPlayer<ClairDeLuneCombatPlayer>();

                // Fire 8 orbs (split effect) with high speed and pierce
                int orbCount = 8;
                for (int i = 0; i < orbCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / orbCount;
                    Vector2 orbVel = angle.ToRotationVector2() * 16f;

                    GenericHomingOrbChild.SpawnChild(
                        Projectile.GetSource_FromThis(),
                        target.Center, orbVel,
                        (int)(Projectile.damage * 1.2f), Projectile.knockBack, Projectile.owner,
                        homingStrength: 0.10f,
                        behaviorFlags: GenericHomingOrbChild.FLAG_PIERCE | GenericHomingOrbChild.FLAG_ACCELERATE,
                        themeIndex: GenericHomingOrbChild.THEME_CLAIRDELUNE,
                        scaleMult: 0.9f);
                }
            }

            ClairDeLuneVFXLibrary.FinisherSlam(target.Center, 1.5f);
        }

        private void FireOrbSpread(int orbCount, float homingStrength, int flags, float scaleMult, float speed)
        {
            if (orbCount == 1)
            {
                // Single orb straight ahead
                GenericHomingOrbChild.SpawnChild(
                    Projectile.GetSource_FromThis(),
                    Owner.MountedCenter + SwordDirection * 40f,
                    SwordDirection * speed,
                    Projectile.damage, Projectile.knockBack, Projectile.owner,
                    homingStrength, flags,
                    GenericHomingOrbChild.THEME_CLAIRDELUNE,
                    scaleMult);
            }
            else
            {
                // Spread formation
                for (int i = 0; i < orbCount; i++)
                {
                    float angle = SwordDirection.ToRotation() + (i - (orbCount - 1) * 0.5f) * 0.3f;
                    Vector2 orbVel = angle.ToRotationVector2() * speed;

                    GenericHomingOrbChild.SpawnChild(
                        Projectile.GetSource_FromThis(),
                        Owner.MountedCenter + SwordDirection * 40f,
                        orbVel,
                        Projectile.damage, Projectile.knockBack, Projectile.owner,
                        homingStrength, flags,
                        GenericHomingOrbChild.THEME_CLAIRDELUNE,
                        scaleMult);
                }
            }
        }
    }
}
