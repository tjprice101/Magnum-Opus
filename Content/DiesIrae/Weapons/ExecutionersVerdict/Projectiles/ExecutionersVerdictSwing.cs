using System;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.DiesIrae;
using MagnumOpus.Content.DiesIrae.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict.Projectiles
{
    /// <summary>
    /// Executioner's Verdict — Dies Irae theme melee. Exoblade-architecture swing.
    /// Judicial Crescendo: Hit counter per target builds to Final Verdict (massive burst).
    /// 5 hits on same target → Final Verdict: 3x damage orb + massive golden explosion.
    /// Right-click dash: Golden judgment burst that fires radial orbs.
    /// </summary>
    public class ExecutionersVerdictSwing : ExobladeStyleSwing
    {
        protected override bool SupportsDash => true;
        private const int HitsForVerdict = 5;

        private DiesIraeCombatPlayer CombatPlayer => Owner.GetModPlayer<DiesIraeCombatPlayer>();

        protected override float BladeLength => 115f;
        protected override int BaseSwingFrames => 72;
        protected override float TextureDrawScale => 0.98f;
        protected override string GradientLUTPath => "MagnumOpus/Assets/VFX Asset Library/ColorGradients/DiesIraeGradientLUTandRAMP";
        protected override Color SlashPrimaryColor => DiesIraePalette.JudgmentGold;
        protected override Color SlashSecondaryColor => DiesIraePalette.SmolderingEmber;
        protected override Color SlashAccentColor => DiesIraePalette.HellfireGold;

        public override string Texture => "MagnumOpus/Content/DiesIrae/Weapons/ExecutionersVerdict/ExecutionersVerdict";

        protected override Color GetLensFlareColor(float p)
            => Color.Lerp(DiesIraePalette.JudgmentGold, DiesIraePalette.HellfireGold, (float)Math.Pow(p, 2));

        protected override Color GetSwingDustColor()
        {
            float t = Main.rand.NextFloat();
            return t < 0.5f
                ? Color.Lerp(DiesIraePalette.JudgmentGold, DiesIraePalette.HellfireGold, Main.rand.NextFloat())
                : Color.Lerp(DiesIraePalette.EmberOrange, DiesIraePalette.InfernalRed, Main.rand.NextFloat());
        }

        protected override void OnSwingStart(bool isFirstSwing)
        {
            if (Main.myPlayer != Projectile.owner) return;

            // Fire a judgment orb on each swing
            Vector2 fireDir = (Main.MouseWorld - Owner.MountedCenter).SafeNormalize(Vector2.UnitX);
            Vector2 firePos = Owner.MountedCenter + fireDir * 40f;

            GenericHomingOrbChild.SpawnChild(
                Projectile.GetSource_FromThis(),
                firePos, fireDir * 12f,
                Projectile.damage / 2, Projectile.knockBack * 0.5f, Projectile.owner,
                homingStrength: 0.05f,
                behaviorFlags: GenericHomingOrbChild.FLAG_ACCELERATE,
                themeIndex: GenericHomingOrbChild.THEME_DIESIRAE,
                scaleMult: 0.85f,
                timeLeft: 80);
        }

        protected override void OnSwingFrame()
        {
            // Golden judgment sparks along blade
            if (Progression > 0.25f && Progression < 0.85f && Main.rand.NextFloat() < 0.45f)
            {
                Vector2 pos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * Main.rand.NextFloat(0.3f, 1f);
                Vector2 vel = SwordDirection.RotatedBy(MathHelper.PiOver2 * Direction * Main.rand.NextFloat(0.3f, 1.2f)) * Main.rand.NextFloat(1.5f, 4f);
                vel.Y -= Main.rand.NextFloat(0.5f, 2f);
                Color sparkCol = Color.Lerp(DiesIraePalette.JudgmentGold, DiesIraePalette.HellfireGold, Main.rand.NextFloat());
                Dust spark = Dust.NewDustPerfect(pos, DustID.GoldFlame, vel, 0, sparkCol, Main.rand.NextFloat(0.8f, 1.3f));
                spark.noGravity = true;
                spark.fadeIn = 0.8f;
            }

            // Ember trail at tip
            if (Progression > 0.35f && Main.rand.NextFloat() < 0.25f)
            {
                Vector2 tip = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale;
                Color emberCol = Color.Lerp(DiesIraePalette.InfernalRed, DiesIraePalette.EmberOrange, Main.rand.NextFloat());
                Dust ember = Dust.NewDustPerfect(tip, DustID.Torch, Main.rand.NextVector2Circular(1.5f, 1.5f), 0, emberCol, 0.7f);
                ember.noGravity = true;
            }

            // Solar flare sparks at tip on higher progression
            if (Progression > 0.5f && Main.rand.NextFloat() < 0.2f)
            {
                Vector2 tip = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale;
                Dust spark = Dust.NewDustPerfect(tip, DustID.SolarFlare, Main.rand.NextVector2Circular(2f, 2f), 0, default, 0.7f);
                spark.noGravity = true;
            }

            // Periodic music notes
            if ((int)(Progression * 100) % 30 == 0 && Progression > 0.2f && Progression < 0.9f)
            {
                Vector2 tip = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * 0.7f;
                DiesIraeVFXLibrary.SpawnMusicNotes(tip, 1, 10f, 0.6f, 0.8f, 25);
            }

            // Dynamic lighting with hit counter intensity
            int hitCount = CombatPlayer.ExecutionerHitCount;
            float intensity = 0.35f + Math.Min(hitCount, HitsForVerdict) * 0.1f;
            Lighting.AddLight(Owner.MountedCenter + SwordDirection * BladeLength * 0.5f,
                DiesIraePalette.JudgmentGold.ToVector3() * intensity);
        }

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 180);

            // Track hits on same target
            int hitCount = CombatPlayer.IncrementExecutionerHits(target.whoAmI);

            // Standard impact VFX (scales with hit counter)
            int burstCount = 6 + Math.Min(hitCount, HitsForVerdict) * 2;
            for (int i = 0; i < burstCount; i++)
            {
                float angle = MathHelper.TwoPi * i / burstCount;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color goldCol = Color.Lerp(DiesIraePalette.JudgmentGold, DiesIraePalette.HellfireGold, Main.rand.NextFloat());
                Dust gold = Dust.NewDustPerfect(target.Center, DustID.GoldFlame, vel, 0, goldCol, Main.rand.NextFloat(0.9f, 1.3f));
                gold.noGravity = true;
            }

            // Fire sparks
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = (target.Center - Owner.Center).SafeNormalize(Vector2.UnitX).RotatedByRandom(0.6f) * Main.rand.NextFloat(4f, 7f);
                Color fireCol = Color.Lerp(DiesIraePalette.InfernalRed, DiesIraePalette.EmberOrange, Main.rand.NextFloat());
                Dust fire = Dust.NewDustPerfect(target.Center, DustID.Torch, vel, 0, fireCol, 0.7f);
                fire.noGravity = true;
            }

            // Standard orb on hit
            if (Main.myPlayer == Projectile.owner && hitCount < HitsForVerdict)
            {
                Vector2 dir = (target.Center - Owner.Center).SafeNormalize(Vector2.UnitX);
                GenericHomingOrbChild.SpawnChild(
                    Projectile.GetSource_FromThis(),
                    target.Center, dir.RotatedByRandom(0.3f) * 8f,
                    Projectile.damage / 3, Projectile.knockBack * 0.3f, Projectile.owner,
                    homingStrength: 0.06f,
                    behaviorFlags: 0,
                    themeIndex: GenericHomingOrbChild.THEME_DIESIRAE,
                    scaleMult: 0.7f + hitCount * 0.1f,
                    timeLeft: 60);
            }

            // Final Verdict at 5 hits
            if (hitCount >= HitsForVerdict && Main.myPlayer == Projectile.owner)
            {
                // Fire massive 3x damage orb
                Vector2 dir = (target.Center - Owner.Center).SafeNormalize(Vector2.UnitX);
                GenericHomingOrbChild.SpawnChild(
                    Projectile.GetSource_FromThis(),
                    target.Center, dir * 16f,
                    Projectile.damage * 3, Projectile.knockBack * 2, Projectile.owner,
                    homingStrength: 0.10f,
                    behaviorFlags: GenericHomingOrbChild.FLAG_ACCELERATE | GenericHomingOrbChild.FLAG_ZONE_ON_KILL,
                    themeIndex: GenericHomingOrbChild.THEME_DIESIRAE,
                    scaleMult: 1.8f,
                    timeLeft: 120);

                // Reset hit counter
                CombatPlayer.ResetExecutionerHits();

                // Massive VFX
                DiesIraeVFXLibrary.FinisherSlam(target.Center, 1.2f);
                SoundEngine.PlaySound(SoundID.Item119 with { Pitch = -0.3f, Volume = 1.0f }, target.Center);

                // Golden explosion burst
                for (int i = 0; i < 20; i++)
                {
                    float angle = MathHelper.TwoPi * i / 20;
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 10f);
                    Color goldCol = Color.Lerp(DiesIraePalette.JudgmentGold, DiesIraePalette.WrathWhite, Main.rand.NextFloat(0.3f));
                    Dust gold = Dust.NewDustPerfect(target.Center, DustID.GoldFlame, vel, 0, goldCol, Main.rand.NextFloat(1.4f, 2f));
                    gold.noGravity = true;
                }

                // Solar flare sparks
                for (int i = 0; i < 10; i++)
                {
                    Dust spark = Dust.NewDustPerfect(target.Center, DustID.SolarFlare,
                        Main.rand.NextVector2Circular(5f, 5f), 0, default, 1.0f);
                    spark.noGravity = true;
                }
            }
            else
            {
                // Build-up VFX based on hit count
                DiesIraeVFXLibrary.MeleeImpact(target.Center, Math.Min(hitCount - 1, 3));
            }
        }

        protected override void OnDashHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 300);

            // Fire radial orbs on dash hit
            if (Main.myPlayer == Projectile.owner)
            {
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6;
                    Vector2 vel = angle.ToRotationVector2() * 14f;

                    GenericHomingOrbChild.SpawnChild(
                        Projectile.GetSource_FromThis(),
                        target.Center, vel,
                        Projectile.damage, Projectile.knockBack, Projectile.owner,
                        homingStrength: 0.08f,
                        behaviorFlags: GenericHomingOrbChild.FLAG_ACCELERATE | GenericHomingOrbChild.FLAG_PIERCE,
                        themeIndex: GenericHomingOrbChild.THEME_DIESIRAE,
                        scaleMult: 1.2f,
                        timeLeft: 90);
                }
            }

            // Massive golden judgment burst
            DiesIraeVFXLibrary.FinisherSlam(target.Center, 1.5f);
            SoundEngine.PlaySound(SoundID.Item73 with { Pitch = -0.2f, Volume = 0.9f }, target.Center);

            for (int i = 0; i < 24; i++)
            {
                float angle = MathHelper.TwoPi * i / 24;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(7f, 12f);
                Color goldCol = Color.Lerp(DiesIraePalette.JudgmentGold, DiesIraePalette.WrathWhite, Main.rand.NextFloat(0.4f));
                Dust gold = Dust.NewDustPerfect(target.Center, DustID.GoldFlame, vel, 0, goldCol, Main.rand.NextFloat(1.3f, 2f));
                gold.noGravity = true;
            }

            for (int i = 0; i < 12; i++)
            {
                Dust spark = Dust.NewDustPerfect(target.Center, DustID.SolarFlare,
                    Main.rand.NextVector2Circular(6f, 6f), 0, default, 1.1f);
                spark.noGravity = true;
            }
        }
    }
}
