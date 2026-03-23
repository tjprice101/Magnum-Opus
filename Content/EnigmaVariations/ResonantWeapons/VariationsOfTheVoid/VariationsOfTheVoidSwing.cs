using System;
using MagnumOpus.Common.BaseClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.VariationsOfTheVoid
{
    /// <summary>
    /// Variations of the Void — Enigma Variations theme melee. Exoblade-architecture swing.
    /// Eerie green/void purple trail with arcane highlights.
    /// </summary>
    public class VariationsOfTheVoidSwing : ExobladeStyleSwing
    {
        protected override bool SupportsDash => false;

        protected override float BladeLength => 108f;
        protected override int BaseSwingFrames => 78;
        protected override Color SlashPrimaryColor => new Color(100, 255, 150);
        protected override Color SlashSecondaryColor => new Color(60, 15, 100);
        protected override Color SlashAccentColor => new Color(180, 140, 255);

        public override string Texture => "MagnumOpus/Content/EnigmaVariations/ResonantWeapons/VariationsOfTheVoid/VariationsOfTheVoid";

        private int comboPhase;

        protected override Color GetLensFlareColor(float p)
            => Color.Lerp(new Color(80, 220, 140), new Color(180, 120, 255), (float)Math.Pow(p, 2));

        /// <summary>
        /// Called on every swing start including hold re-swings.
        /// Every 3rd swing spawns VoidConvergenceBeamSet.
        /// </summary>
        protected override void OnSwingStart(bool isFirstSwing)
        {
            comboPhase++;
            if (comboPhase >= 3)
            {
                comboPhase = 0;
                int beamType = ModContent.ProjectileType<VoidConvergenceBeamSet>();
                if (Owner.ownedProjectileCounts[beamType] == 0)
                {
                    Vector2 toCursor = (Main.MouseWorld - Owner.Center).SafeNormalize(Vector2.UnitX);
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(), Owner.Center, toCursor,
                        beamType, Projectile.damage, Projectile.knockBack, Owner.whoAmI);
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.1f, Volume = 0.7f }, Owner.Center);
                }
            }
        }

        protected override Color GetSwingDustColor()
        {
            float t = Main.rand.NextFloat();
            return t < 0.5f
                ? Color.Lerp(new Color(80, 255, 130), new Color(120, 200, 140), Main.rand.NextFloat())
                : Color.Lerp(new Color(120, 60, 200), new Color(160, 100, 255), Main.rand.NextFloat());
        }

        protected override void OnSwingFrame()
        {
            // Void-green arcane sparks along the blade
            if (Progression > 0.3f && Progression < 0.85f && Main.rand.NextFloat() < 0.35f)
            {
                Vector2 pos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * Main.rand.NextFloat(0.3f, 1f);
                Vector2 vel = SwordDirection.RotatedByRandom(0.7f) * Main.rand.NextFloat(1f, 3f);
                Dust spark = Dust.NewDustPerfect(pos, DustID.CursedTorch, vel, 60, default, Main.rand.NextFloat(0.6f, 1f));
                spark.noGravity = true;
                spark.fadeIn = 0.7f;
            }

            // Enigma purple wisps at blade tip
            if (Progression > 0.45f && Main.rand.NextFloat() < 0.15f)
            {
                Vector2 tip = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale;
                Dust wisp = Dust.NewDustPerfect(tip, DustID.ShadowbeamStaff,
                    Main.rand.NextVector2Circular(1f, 1f), 40, new Color(140, 60, 200), 0.6f);
                wisp.noGravity = true;
            }
        }

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.CursedInferno, 180);

            for (int i = 0; i < 8; i++)
            {
                Color c = i % 2 == 0 ? new Color(80, 255, 120) : new Color(140, 60, 200);
                Dust burst = Dust.NewDustPerfect(target.Center, DustID.CursedTorch,
                    Main.rand.NextVector2CircularEdge(5f, 5f), 40, c, Main.rand.NextFloat(0.8f, 1.2f));
                burst.noGravity = true;
            }
        }

        protected override void OnDashHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.CursedInferno, 300);

            for (int i = 0; i < 14; i++)
            {
                Dust burst = Dust.NewDustPerfect(target.Center, DustID.CursedTorch,
                    Main.rand.NextVector2CircularEdge(8f, 8f), 20, default, Main.rand.NextFloat(1.2f, 1.8f));
                burst.noGravity = true;
            }
            for (int i = 0; i < 4; i++)
            {
                Dust purple = Dust.NewDustPerfect(target.Center, DustID.ShadowbeamStaff,
                    Main.rand.NextVector2Circular(3f, 3f), 40, default, 0.8f);
                purple.noGravity = true;
            }
        }
    }
}
