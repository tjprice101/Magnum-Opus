using System;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.DiesIrae;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Projectiles
{
    /// <summary>
    /// Wrath's Cleaver — Dies Irae theme melee. Exoblade-architecture swing.
    /// Wrath Escalation: 4-phase combo that fires escalating orbs.
    /// Phase 0: 1 straight orb (no homing) — the warning
    /// Phase 1: 2 orbs with mild homing (0.06) — judgment approaches
    /// Phase 2: 3 orbs with standard homing (0.08) + pierce — judgment weighs
    /// Phase 3: 1 aggressive orb (0.14) that splits into 8 on hit — judgment rendered
    /// </summary>
    public class WrathsCleaverSwing : ExobladeStyleSwing
    {
        protected override bool SupportsDash => true;

        /// <summary>Wrath combo phase (0-3). Advances each swing including hold re-swings.</summary>
        private int comboPhase = 0;

        /// <summary>Track if orbs were fired this swing to avoid double-firing.</summary>
        private bool orbsFiredThisSwing = false;

        protected override float BladeLength => 115f;
        protected override int BaseSwingFrames => 82;
        protected override float TextureDrawScale => 0.93f;
        protected override string GradientLUTPath => "MagnumOpus/Assets/VFX Asset Library/ColorGradients/DiesIraeGradientLUTandRAMP";
        protected override Color SlashPrimaryColor => DiesIraePalette.WrathfulFlame;
        protected override Color SlashSecondaryColor => DiesIraePalette.DarkBlood;
        protected override Color SlashAccentColor => DiesIraePalette.HellfireGold;

        public override string Texture => "MagnumOpus/Content/DiesIrae/Weapons/WrathsCleaver/WrathsCleaver";

        protected override void OnSwingStart(bool isFirstSwing)
        {
            orbsFiredThisSwing = false;

            if (Main.myPlayer != Projectile.owner) return;

            int phase = comboPhase % 4;

            // Fire orbs based on current phase
            Vector2 fireDir = (Main.MouseWorld - Owner.MountedCenter).SafeNormalize(Vector2.UnitX);
            Vector2 firePos = Owner.MountedCenter + fireDir * 30f;
            int baseDamage = Projectile.damage;

            switch (phase)
            {
                case 0:
                    // Phase 0: 1 straight orb (no homing) — the warning
                    GenericHomingOrbChild.SpawnChild(
                        Projectile.GetSource_FromThis(),
                        firePos, fireDir * 14f,
                        baseDamage, Projectile.knockBack, Projectile.owner,
                        homingStrength: 0f,
                        behaviorFlags: 0,
                        themeIndex: GenericHomingOrbChild.THEME_DIESIRAE,
                        scaleMult: 1f,
                        timeLeft: 90);
                    break;

                case 1:
                    // Phase 1: 2 orbs with mild homing (0.06) — judgment approaches
                    for (int i = 0; i < 2; i++)
                    {
                        float angleOffset = (i - 0.5f) * 0.2f;
                        Vector2 vel = fireDir.RotatedBy(angleOffset) * 12f;
                        GenericHomingOrbChild.SpawnChild(
                            Projectile.GetSource_FromThis(),
                            firePos, vel,
                            baseDamage, Projectile.knockBack, Projectile.owner,
                            homingStrength: 0.06f,
                            behaviorFlags: 0,
                            themeIndex: GenericHomingOrbChild.THEME_DIESIRAE,
                            scaleMult: 1f,
                            timeLeft: 100);
                    }
                    break;

                case 2:
                    // Phase 2: 3 orbs with standard homing (0.08) + pierce — judgment weighs
                    for (int i = 0; i < 3; i++)
                    {
                        float angleOffset = (i - 1) * 0.15f;
                        Vector2 vel = fireDir.RotatedBy(angleOffset) * 14f;
                        GenericHomingOrbChild.SpawnChild(
                            Projectile.GetSource_FromThis(),
                            firePos, vel,
                            baseDamage, Projectile.knockBack, Projectile.owner,
                            homingStrength: 0.08f,
                            behaviorFlags: GenericHomingOrbChild.FLAG_PIERCE,
                            themeIndex: GenericHomingOrbChild.THEME_DIESIRAE,
                            scaleMult: 1.1f,
                            timeLeft: 110);
                    }
                    break;

                case 3:
                    // Phase 3: 1 aggressive orb (0.14) that splits into 8 on hit — judgment rendered
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        firePos, fireDir * 16f,
                        ModContent.ProjectileType<WrathSplittingOrb>(),
                        (int)(baseDamage * 1.5f), Projectile.knockBack * 1.5f, Projectile.owner);

                    // Play empowered sound
                    SoundEngine.PlaySound(SoundID.Item73 with { Pitch = -0.3f, Volume = 0.8f }, Owner.MountedCenter);
                    break;
            }

            // Advance combo phase
            comboPhase++;
            orbsFiredThisSwing = true;
        }

        protected override Color GetLensFlareColor(float p)
            => Color.Lerp(DiesIraePalette.HellfireGold, DiesIraePalette.WrathWhite, (float)Math.Pow(p, 2));

        protected override Color GetSwingDustColor()
        {
            float t = Main.rand.NextFloat();
            return t < 0.4f
                ? Color.Lerp(DiesIraePalette.InfernalRed, DiesIraePalette.BloodRed, Main.rand.NextFloat())
                : Color.Lerp(DiesIraePalette.JudgmentGold, DiesIraePalette.EmberOrange, Main.rand.NextFloat());
        }

        protected override void OnSwingFrame()
        {
            // Ember trail along blade
            if (Progression > 0.3f && Progression < 0.85f && Main.rand.NextFloat() < 0.45f)
            {
                Vector2 pos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * Main.rand.NextFloat(0.3f, 1f);
                Vector2 vel = SwordDirection.RotatedByRandom(0.6f) * Main.rand.NextFloat(1.5f, 3.5f);
                vel.Y -= Main.rand.NextFloat(0.5f, 2f);
                Color emberCol = Color.Lerp(DiesIraePalette.InfernalRed, DiesIraePalette.JudgmentGold, Main.rand.NextFloat());
                Dust ember = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, emberCol, Main.rand.NextFloat(0.9f, 1.4f));
                ember.noGravity = true;
                ember.fadeIn = 0.6f;
            }

            // Solar flare sparks at tip
            if (Progression > 0.5f && Main.rand.NextFloat() < 0.25f)
            {
                Vector2 tip = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale;
                Dust spark = Dust.NewDustPerfect(tip, DustID.SolarFlare, Main.rand.NextVector2Circular(2f, 2f), 0, default, 0.8f);
                spark.noGravity = true;
            }

            // Periodic music notes
            if ((int)(Progression * 100) % 25 == 0 && Progression > 0.2f && Progression < 0.9f)
            {
                Vector2 tip = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * 0.7f;
                DiesIraeVFXLibrary.SpawnMusicNotes(tip, 1, 12f, 0.6f, 0.9f, 25);
            }

            // Dynamic lighting
            int phase = (comboPhase - 1) % 4;
            float intensity = 0.4f + phase * 0.15f;
            Lighting.AddLight(Owner.MountedCenter + SwordDirection * BladeLength * 0.5f,
                DiesIraePalette.GetFireGradient(Progression).ToVector3() * intensity);
        }

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 180);

            int phase = (comboPhase - 1) % 4;

            // Impact VFX scales with combo phase
            DiesIraeVFXLibrary.MeleeImpact(target.Center, phase);

            // Additional radial fire burst
            int burstCount = 6 + phase * 2;
            for (int i = 0; i < burstCount; i++)
            {
                float angle = MathHelper.TwoPi * i / burstCount;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 6f);
                Color fireCol = Color.Lerp(DiesIraePalette.InfernalRed, DiesIraePalette.JudgmentGold, Main.rand.NextFloat());
                Dust fire = Dust.NewDustPerfect(target.Center, DustID.Torch, vel, 0, fireCol, Main.rand.NextFloat(1f, 1.4f));
                fire.noGravity = true;
            }

            // Extra sparks on higher phases
            if (phase >= 2)
            {
                for (int i = 0; i < 6; i++)
                {
                    Dust spark = Dust.NewDustPerfect(target.Center, DustID.SolarFlare,
                        Main.rand.NextVector2Circular(4f, 4f), 0, default, 0.9f);
                    spark.noGravity = true;
                }
            }
        }

        protected override void OnDashHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 300);

            // Fire pierce-all orb on dash hit
            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 dashDir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
                GenericHomingOrbChild.SpawnChild(
                    Projectile.GetSource_FromThis(),
                    target.Center, dashDir * 20f,
                    Projectile.damage * 2, Projectile.knockBack * 2, Projectile.owner,
                    homingStrength: 0f,
                    behaviorFlags: GenericHomingOrbChild.FLAG_ACCELERATE,
                    themeIndex: GenericHomingOrbChild.THEME_DIESIRAE,
                    scaleMult: 1.5f,
                    timeLeft: 45);
            }

            // Massive fire burst on dash hit
            DiesIraeVFXLibrary.FinisherSlam(target.Center, 1.2f);

            for (int i = 0; i < 18; i++)
            {
                float angle = MathHelper.TwoPi * i / 18;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 10f);
                Color fireCol = Color.Lerp(DiesIraePalette.InfernalRed, DiesIraePalette.HellfireGold, Main.rand.NextFloat());
                Dust fire = Dust.NewDustPerfect(target.Center, DustID.Torch, vel, 0, fireCol, Main.rand.NextFloat(1.3f, 1.9f));
                fire.noGravity = true;
            }
        }
    }
}
