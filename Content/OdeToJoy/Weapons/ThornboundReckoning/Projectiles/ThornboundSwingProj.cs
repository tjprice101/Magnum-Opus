using System;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Content.OdeToJoy;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Projectiles
{
    /// <summary>
    /// ThornboundReckoning swing — Ode to Joy theme melee. Exoblade-architecture swing.
    /// Bouncing Thorns pattern: Swing fires 1 orb with 3 bounces (off tiles/enemies).
    /// Each bounce increases damage by +20% (compounding).
    /// </summary>
    public class ThornboundSwingProj : ExobladeStyleSwing
    {
        protected override bool SupportsDash => false;

        protected override float BladeLength => 105f;
        protected override int BaseSwingFrames => 78;
        protected override float TextureDrawScale => 0.96f;
        protected override string GradientLUTPath => "MagnumOpus/Assets/VFX Asset Library/ColorGradients/OdeToJoyGradientLUTandRAMP";
        protected override Color SlashPrimaryColor => OdeToJoyPalette.VerdantGreen;
        protected override Color SlashSecondaryColor => OdeToJoyPalette.MossShadow;
        protected override Color SlashAccentColor => OdeToJoyPalette.GoldenPollen;

        public override string Texture => "MagnumOpus/Content/OdeToJoy/Weapons/ThornboundReckoning/ThornboundReckoning";

        private bool _firedThorn;

        protected override Color GetLensFlareColor(float p)
            => Color.Lerp(OdeToJoyPalette.LeafGreen, OdeToJoyPalette.GoldenPollen, (float)Math.Pow(p, 2));

        protected override Color GetSwingDustColor()
        {
            float t = Main.rand.NextFloat();
            return t < 0.5f
                ? Color.Lerp(OdeToJoyPalette.BudGreen, OdeToJoyPalette.VerdantGreen, Main.rand.NextFloat())
                : Color.Lerp(OdeToJoyPalette.LeafGreen, OdeToJoyPalette.GoldenPollen, Main.rand.NextFloat());
        }

        protected override void OnSwingFrame()
        {
            // Fire bouncing thorn at swing apex (Progression ~0.5)
            if (!_firedThorn && Progression > 0.45f && Progression < 0.55f)
            {
                _firedThorn = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    FireBouncingThorn(3); // 3 bounces for normal swing

                    // Fire sound
                    SoundEngine.PlaySound(SoundID.Item17 with { Pitch = 0.2f, Volume = 0.7f }, Owner.Center);
                }
            }

            // Verdant swing dust
            if (Progression > 0.3f && Progression < 0.85f && Main.rand.NextFloat() < 0.4f)
            {
                Vector2 pos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * Main.rand.NextFloat(0.4f, 1f);
                Vector2 vel = SwordDirection.RotatedBy(MathHelper.PiOver2 * Direction * Main.rand.NextFloat(0.3f, 1.2f)) * Main.rand.NextFloat(1f, 3f);
                vel.Y -= Main.rand.NextFloat(0.5f, 1.5f);

                Color dustCol = GetSwingDustColor();
                Dust petal = Dust.NewDustPerfect(pos, DustID.GreenTorch, vel, 80, dustCol, Main.rand.NextFloat(0.7f, 1.1f));
                petal.noGravity = true;
                petal.fadeIn = 0.9f;
            }

            // Golden pollen sparkles at tip
            if (Progression > 0.4f && Main.rand.NextFloat() < 0.2f)
            {
                Vector2 tip = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale;
                Dust spark = Dust.NewDustPerfect(tip, DustID.GoldFlame, Main.rand.NextVector2Circular(1.5f, 1.5f), 0, OdeToJoyPalette.GoldenPollen, 0.6f);
                spark.noGravity = true;
            }

            // Rose petal accents
            if (Main.rand.NextBool(8))
            {
                Vector2 pos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * Main.rand.NextFloat(0.5f, 0.9f);
                Dust rose = Dust.NewDustPerfect(pos, DustID.PinkTorch, Main.rand.NextVector2Circular(1f, 1f), 100, OdeToJoyPalette.RosePink, 0.5f);
                rose.noGravity = true;
            }
        }

        private void FireBouncingThorn(int bounces)
        {
            Vector2 spawnPos = Owner.MountedCenter + SwordDirection * BladeLength * 0.6f;
            Vector2 velocity = SwordDirection * 14f;

            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                spawnPos, velocity,
                ModContent.ProjectileType<BouncingThornProjectile>(),
                Projectile.damage, Projectile.knockBack, Projectile.owner,
                ai0: bounces, ai1: 1f);
        }

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Verdant burst on hit
            OdeToJoyVFXLibrary.SpawnRadialDustBurst(target.Center, 10, 5f);

            // Petal scatter
            for (int i = 0; i < 6; i++)
            {
                Color petalCol = Main.rand.NextBool() ? OdeToJoyPalette.LeafGreen : OdeToJoyPalette.RosePink;
                Dust petal = Dust.NewDustPerfect(target.Center, DustID.GreenTorch,
                    Main.rand.NextVector2CircularEdge(5f, 5f), 60, petalCol, Main.rand.NextFloat(0.9f, 1.3f));
                petal.noGravity = true;
                petal.fadeIn = 1f;
            }

            // Golden spark burst
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = (target.Center - Owner.Center).SafeNormalize(Vector2.UnitX).RotatedByRandom(0.6f) * Main.rand.NextFloat(4f, 7f);
                Dust spark = Dust.NewDustPerfect(target.Center, DustID.GoldFlame, vel, 0, OdeToJoyPalette.GoldenPollen, 0.7f);
                spark.noGravity = true;
            }
        }

        protected override void OnDashHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Enhanced dash impact
            OdeToJoyVFXLibrary.SpawnBloomBurst(target.Center, 12, 1.2f);

            for (int i = 0; i < 14; i++)
            {
                Color dustCol = (i % 3) switch
                {
                    0 => OdeToJoyPalette.LeafGreen,
                    1 => OdeToJoyPalette.GoldenPollen,
                    _ => OdeToJoyPalette.RosePink
                };
                Dust petal = Dust.NewDustPerfect(target.Center, DustID.GreenTorch,
                    Main.rand.NextVector2CircularEdge(7f, 7f), 40, dustCol, Main.rand.NextFloat(1.1f, 1.6f));
                petal.noGravity = true;
            }
        }
    }
}
