using System;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.DiesIrae;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.ChainOfJudgment.Projectiles
{
    /// <summary>
    /// Judgment Chain — Dies Irae theme melee. Exoblade-architecture swing.
    /// Burning chains that bind: Fire bouncing orbs that apply slow debuff.
    /// Swings release 2 bouncing chain-orbs that ricochet between enemies.
    /// Right-click dash: Chain burst spawning 8 bouncing orbs in all directions.
    /// </summary>
    public class JudgmentChainProjectile : ExobladeStyleSwing
    {
        protected override bool SupportsDash => true;

        private int swingCounter = 0;

        protected override float BladeLength => 100f;
        protected override int BaseSwingFrames => 68;
        protected override float TextureDrawScale => 0.93f;
        protected override string GradientLUTPath => "MagnumOpus/Assets/VFX Asset Library/ColorGradients/DiesIraeGradientLUTandRAMP";
        protected override Color SlashPrimaryColor => DiesIraePalette.BloodRed;
        protected override Color SlashSecondaryColor => DiesIraePalette.DarkBlood;
        protected override Color SlashAccentColor => DiesIraePalette.EmberOrange;

        public override string Texture => "MagnumOpus/Content/DiesIrae/Weapons/ChainOfJudgment/ChainOfJudgment";

        protected override Color GetLensFlareColor(float p)
            => Color.Lerp(DiesIraePalette.BloodRed, DiesIraePalette.EmberOrange, (float)Math.Pow(p, 2));

        protected override Color GetSwingDustColor()
        {
            float t = Main.rand.NextFloat();
            return t < 0.5f
                ? Color.Lerp(DiesIraePalette.BloodRed, DiesIraePalette.InfernalRed, Main.rand.NextFloat())
                : Color.Lerp(DiesIraePalette.EmberOrange, DiesIraePalette.JudgmentGold, Main.rand.NextFloat());
        }

        protected override void OnSwingStart(bool isFirstSwing)
        {
            if (Main.myPlayer != Projectile.owner) return;

            swingCounter++;

            // Fire 2 bouncing chain-orbs per swing
            Vector2 fireDir = (Main.MouseWorld - Owner.MountedCenter).SafeNormalize(Vector2.UnitX);
            Vector2 firePos = Owner.MountedCenter + fireDir * 35f;

            for (int i = 0; i < 2; i++)
            {
                float angleOffset = (i - 0.5f) * 0.3f;
                Vector2 vel = fireDir.RotatedBy(angleOffset) * 13f;

                GenericHomingOrbChild.SpawnChild(
                    Projectile.GetSource_FromThis(),
                    firePos, vel,
                    Projectile.damage / 2, Projectile.knockBack * 0.5f, Projectile.owner,
                    homingStrength: 0.04f,
                    behaviorFlags: GenericHomingOrbChild.FLAG_BOUNCE | GenericHomingOrbChild.FLAG_ACCELERATE,
                    themeIndex: GenericHomingOrbChild.THEME_DIESIRAE,
                    scaleMult: 0.9f,
                    timeLeft: 120);
            }

            // Every 3rd swing, fire an additional piercing orb
            if (swingCounter % 3 == 0)
            {
                GenericHomingOrbChild.SpawnChild(
                    Projectile.GetSource_FromThis(),
                    firePos, fireDir * 15f,
                    Projectile.damage, Projectile.knockBack, Projectile.owner,
                    homingStrength: 0.08f,
                    behaviorFlags: GenericHomingOrbChild.FLAG_PIERCE | GenericHomingOrbChild.FLAG_ACCELERATE,
                    themeIndex: GenericHomingOrbChild.THEME_DIESIRAE,
                    scaleMult: 1.2f,
                    timeLeft: 90);

                SoundEngine.PlaySound(SoundID.Item73 with { Pitch = 0.1f, Volume = 0.6f }, firePos);
            }
        }

        protected override void OnSwingFrame()
        {
            // Ember chain trail along blade
            if (Progression > 0.25f && Progression < 0.85f && Main.rand.NextFloat() < 0.45f)
            {
                Vector2 pos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * Main.rand.NextFloat(0.3f, 1f);
                Vector2 vel = SwordDirection.RotatedBy(MathHelper.PiOver2 * Direction * Main.rand.NextFloat(0.3f, 1.2f)) * Main.rand.NextFloat(1.5f, 4f);
                vel.Y -= Main.rand.NextFloat(0.5f, 2f);
                Color emberCol = Color.Lerp(DiesIraePalette.InfernalRed, DiesIraePalette.EmberOrange, Main.rand.NextFloat());
                Dust ember = Dust.NewDustPerfect(pos, DustID.Torch, vel, 60, emberCol, Main.rand.NextFloat(0.8f, 1.3f));
                ember.noGravity = true;
                ember.fadeIn = 0.9f;
            }

            // Solar flare sparks at tip
            if (Progression > 0.4f && Main.rand.NextFloat() < 0.25f)
            {
                Vector2 tip = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale;
                Dust spark = Dust.NewDustPerfect(tip, DustID.SolarFlare, Main.rand.NextVector2Circular(2f, 2f), 0, default, 0.7f);
                spark.noGravity = true;
            }

            // Chain link particles (golden ember dust in a trail)
            if (Progression > 0.3f && Progression < 0.8f && Main.rand.NextBool(6))
            {
                Vector2 pos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * Main.rand.NextFloat(0.5f, 0.9f);
                Dust chain = Dust.NewDustPerfect(pos, DustID.GoldFlame, -SwordDirection * Main.rand.NextFloat(1f, 2f), 0, default, 0.5f);
                chain.noGravity = true;
            }

            // Music notes
            if ((int)(Progression * 100) % 25 == 0 && Progression > 0.2f && Progression < 0.85f)
            {
                Vector2 tip = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * 0.6f;
                DiesIraeVFXLibrary.SpawnMusicNotes(tip, 1, 12f, 0.5f, 0.85f, 25);
            }

            // Dynamic lighting
            float intensity = 0.4f + 0.1f * MathF.Sin((float)Main.timeForVisualEffects * 0.1f);
            Lighting.AddLight(Owner.MountedCenter + SwordDirection * BladeLength * 0.5f,
                DiesIraePalette.InfernalRed.ToVector3() * intensity);
        }

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply fire and slow debuffs (binding chain effect)
            target.AddBuff(BuffID.OnFire3, 180);
            target.AddBuff(BuffID.Slow, 120); // Binding effect

            // Standard impact VFX
            DiesIraeVFXLibrary.MeleeImpact(target.Center, 1);

            // Fire burst
            int burstCount = 8;
            for (int i = 0; i < burstCount; i++)
            {
                float angle = MathHelper.TwoPi * i / burstCount;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color fireCol = Color.Lerp(DiesIraePalette.InfernalRed, DiesIraePalette.EmberOrange, Main.rand.NextFloat());
                Dust fire = Dust.NewDustPerfect(target.Center, DustID.Torch, vel, 50, fireCol, Main.rand.NextFloat(0.9f, 1.3f));
                fire.noGravity = true;
                fire.fadeIn = 1f;
            }

            // Solar flare sparks
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = (target.Center - Owner.Center).SafeNormalize(Vector2.UnitX).RotatedByRandom(0.6f) * Main.rand.NextFloat(4f, 7f);
                Dust spark = Dust.NewDustPerfect(target.Center, DustID.SolarFlare, vel, 0, default, 0.7f);
                spark.noGravity = true;
            }

            // Spawn a bouncing orb on hit that chains to nearby enemies
            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 dir = Main.rand.NextVector2CircularEdge(1f, 1f);
                GenericHomingOrbChild.SpawnChild(
                    Projectile.GetSource_FromThis(),
                    target.Center, dir * 10f,
                    Projectile.damage / 3, Projectile.knockBack * 0.3f, Projectile.owner,
                    homingStrength: 0.06f,
                    behaviorFlags: GenericHomingOrbChild.FLAG_BOUNCE,
                    themeIndex: GenericHomingOrbChild.THEME_DIESIRAE,
                    scaleMult: 0.7f,
                    timeLeft: 90);
            }
        }

        protected override void OnDashHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 300);
            target.AddBuff(BuffID.Slow, 180);

            // Chain burst: 8 bouncing orbs in all directions
            if (Main.myPlayer == Projectile.owner)
            {
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8 + Main.rand.NextFloat(-0.1f, 0.1f);
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(12f, 16f);

                    GenericHomingOrbChild.SpawnChild(
                        Projectile.GetSource_FromThis(),
                        target.Center, vel,
                        Projectile.damage, Projectile.knockBack, Projectile.owner,
                        homingStrength: 0.07f,
                        behaviorFlags: GenericHomingOrbChild.FLAG_BOUNCE | GenericHomingOrbChild.FLAG_ACCELERATE | GenericHomingOrbChild.FLAG_PIERCE,
                        themeIndex: GenericHomingOrbChild.THEME_DIESIRAE,
                        scaleMult: 1.1f,
                        timeLeft: 120);
                }
            }

            // Massive infernal chain burst VFX
            DiesIraeVFXLibrary.FinisherSlam(target.Center, 1.3f);
            SoundEngine.PlaySound(SoundID.Item73 with { Pitch = -0.2f, Volume = 0.9f }, target.Center);

            // Radial fire explosion
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(7f, 11f);
                Color fireCol = Color.Lerp(DiesIraePalette.InfernalRed, DiesIraePalette.BloodRed, Main.rand.NextFloat());
                Dust fire = Dust.NewDustPerfect(target.Center, DustID.Torch, vel, 40, fireCol, Main.rand.NextFloat(1.3f, 1.8f));
                fire.noGravity = true;
            }

            // Golden chain link sparks
            for (int i = 0; i < 10; i++)
            {
                Dust chain = Dust.NewDustPerfect(target.Center, DustID.GoldFlame,
                    Main.rand.NextVector2Circular(5f, 5f), 0, default, Main.rand.NextFloat(0.8f, 1.2f));
                chain.noGravity = true;
            }

            // Solar flare burst
            for (int i = 0; i < 8; i++)
            {
                Dust spark = Dust.NewDustPerfect(target.Center, DustID.SolarFlare,
                    Main.rand.NextVector2Circular(6f, 6f), 0, default, 1.0f);
                spark.noGravity = true;
            }
        }
    }
}
