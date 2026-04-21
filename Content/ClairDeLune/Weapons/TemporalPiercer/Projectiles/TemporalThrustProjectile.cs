using System;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.ClairDeLune;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.TemporalPiercer.Projectiles
{
    /// <summary>
    /// Temporal Piercer — Clair de Lune theme melee. Exoblade-architecture swing.
    /// Temporal Echo: Fires 1 orb with standard homing. On hit, spawns 60% damage ghost copy returning opposite direction.
    /// Right-click: aggressive homing (0.14) + stationary zone (1.5s, 100px).
    /// </summary>
    public class TemporalThrustProjectile : ExobladeStyleSwing
    {
        protected override bool SupportsDash => true;

        protected override float BladeLength => 98f;
        protected override int BaseSwingFrames => 74;
        protected override float TextureDrawScale => 0.84f;
        protected override string GradientLUTPath => "MagnumOpus/Assets/VFX Asset Library/ColorGradients/ClairDeLuneGradientLUTandRAMP";
        protected override Color SlashPrimaryColor => ClairDeLunePalette.PearlFrost;
        protected override Color SlashSecondaryColor => ClairDeLunePalette.DeepNight;
        protected override Color SlashAccentColor => ClairDeLunePalette.WhiteHot;

        public override string Texture => "MagnumOpus/Content/ClairDeLune/Weapons/TemporalPiercer/TemporalPiercer";

        protected override Color GetLensFlareColor(float p)
            => Color.Lerp(ClairDeLunePalette.PearlBlue, ClairDeLunePalette.WhiteHot, (float)Math.Pow(p, 2));

        protected override Color GetSwingDustColor()
        {
            float t = Main.rand.NextFloat();
            return t < 0.5f
                ? Color.Lerp(ClairDeLunePalette.SoftBlue, ClairDeLunePalette.MoonlitFrost, Main.rand.NextFloat())
                : Color.Lerp(ClairDeLunePalette.PearlFrost, ClairDeLunePalette.WhiteHot, Main.rand.NextFloat());
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

            // Temporal Echo — spawn reverse-direction homing orb at hit position
            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 echoVel = -SwordDirection * 8f;
                GenericHomingOrbChild.SpawnChild(
                    Projectile.GetSource_FromThis(),
                    target.Center, echoVel,
                    (int)(Projectile.damage * 0.6f), Projectile.knockBack * 0.5f, Projectile.owner,
                    homingStrength: 0.08f, behaviorFlags: 0,
                    themeIndex: GenericHomingOrbChild.THEME_CLAIRDELUNE,
                    scaleMult: 0.8f);
            }
        }

        protected override void OnDashHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Right-click dash: aggressive homing orb + stationary zone
            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 dashVel = SwordDirection * 12f;
                GenericHomingOrbChild.SpawnChild(
                    Projectile.GetSource_FromThis(),
                    target.Center, dashVel,
                    (int)(Projectile.damage * 0.8f), Projectile.knockBack * 0.7f, Projectile.owner,
                    homingStrength: 0.14f,
                    behaviorFlags: GenericHomingOrbChild.FLAG_ACCELERATE,
                    themeIndex: GenericHomingOrbChild.THEME_CLAIRDELUNE,
                    scaleMult: 1.0f);

                // Spawn stationary zone at hit point (100px radius, 90 frames duration)
                GenericDamageZone.SpawnZone(
                    Projectile.GetSource_FromThis(),
                    target.Center, Projectile.damage, Projectile.knockBack, Projectile.owner,
                    GenericDamageZone.FLAG_SLOW, 100f, 8, durationFrames: 90);
            }

            ClairDeLuneVFXLibrary.FinisherSlam(target.Center, 1.2f);
        }
    }
}
