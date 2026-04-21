using System;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Content.OdeToJoy;
using MagnumOpus.Content.OdeToJoy.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.RoseThornChainsaw.Projectiles
{
    /// <summary>
    /// RoseThornChainsaw swing — Ode to Joy theme melee. Exoblade-architecture swing.
    /// Rapid Stream Saw: fires 3 orbs in rapid burst (3-frame gaps) in tight cone (±5°).
    /// Each orb has short timeLeft (40 frames), no homing.
    /// Right-click empowerment: 5s duration, all orbs gain pierce 2, wider spread (±8°).
    /// </summary>
    public class RoseThornChainsawProjectile : ExobladeStyleSwing
    {
        protected override bool SupportsDash => false;

        protected override float BladeLength => 95f;
        protected override int BaseSwingFrames => 72;
        protected override float TextureDrawScale => 1.0f;
        protected override string GradientLUTPath => "MagnumOpus/Assets/VFX Asset Library/ColorGradients/OdeToJoyGradientLUTandRAMP";
        protected override Color SlashPrimaryColor => OdeToJoyPalette.RosePink;
        protected override Color SlashSecondaryColor => OdeToJoyPalette.MossShadow;
        protected override Color SlashAccentColor => OdeToJoyPalette.GoldenPollen;

        public override string Texture => "MagnumOpus/Content/OdeToJoy/Weapons/RoseThornChainsaw/RoseThornChainsaw";

        private bool _firedOrbBurst;
        private int _orbBurstFrameCounter;

        protected override Color GetLensFlareColor(float p)
            => Color.Lerp(OdeToJoyPalette.RosePink, OdeToJoyPalette.GoldenPollen, (float)Math.Pow(p, 2));

        protected override Color GetSwingDustColor()
        {
            float t = Main.rand.NextFloat();
            return t < 0.5f
                ? Color.Lerp(OdeToJoyPalette.RosePink, OdeToJoyPalette.PetalPink, Main.rand.NextFloat())
                : Color.Lerp(OdeToJoyPalette.RosePink, OdeToJoyPalette.GoldenPollen, Main.rand.NextFloat());
        }

        protected override void OnSwingFrame()
        {
            // Fire 3 orbs at swing apex with 3-frame gaps
            if (!_firedOrbBurst && Progression > 0.45f && Progression < 0.55f)
            {
                _firedOrbBurst = true;
                _orbBurstFrameCounter = 0;
                if (Main.myPlayer == Projectile.owner)
                {
                    SoundEngine.PlaySound(SoundID.Item17 with { Pitch = 0.3f, Volume = 0.6f }, Owner.Center);
                }
            }

            if (_firedOrbBurst)
            {
                if (Main.myPlayer == Projectile.owner && _orbBurstFrameCounter < 6)
                {
                    // Fire orb every 3 frames (-5°, 0°, +5°)
                    if (_orbBurstFrameCounter == 0 || _orbBurstFrameCounter == 3 || _orbBurstFrameCounter == 6)
                    {
                        float angle;
                        if (_orbBurstFrameCounter == 0)
                            angle = -MathHelper.ToRadians(5f);
                        else if (_orbBurstFrameCounter == 3)
                            angle = 0f;
                        else
                            angle = MathHelper.ToRadians(5f);

                        FireRapidStreakOrb(angle);
                    }
                }
                _orbBurstFrameCounter++;
            }

            // Rose petal swing dust
            if (Progression > 0.3f && Progression < 0.85f && Main.rand.NextFloat() < 0.4f)
            {
                Vector2 pos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * Main.rand.NextFloat(0.4f, 1f);
                Vector2 vel = SwordDirection.RotatedBy(MathHelper.PiOver2 * Direction * Main.rand.NextFloat(0.3f, 1.2f)) * Main.rand.NextFloat(1f, 3f);
                vel.Y -= Main.rand.NextFloat(0.5f, 1.5f);

                Color dustCol = GetSwingDustColor();
                Dust petal = Dust.NewDustPerfect(pos, DustID.PinkTorch, vel, 80, dustCol, Main.rand.NextFloat(0.7f, 1.1f));
                petal.noGravity = true;
                petal.fadeIn = 0.9f;
            }

            // Golden pollen sparkles at tip
            if (Progression > 0.4f && Main.rand.NextFloat() < 0.15f)
            {
                Vector2 tip = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale;
                Dust spark = Dust.NewDustPerfect(tip, DustID.GoldFlame, Main.rand.NextVector2Circular(1f, 1f), 0, OdeToJoyPalette.GoldenPollen, 0.6f);
                spark.noGravity = true;
            }
        }

        private void FireRapidStreakOrb(float angleOffset)
        {
            Vector2 dir = SwordDirection.RotatedBy(angleOffset);
            Vector2 velocity = dir * 18f;

            Player owner = Owner;
            var combat = owner.GetModPlayer<OdeToJoyCombatPlayer>();
            bool isEmpowered = combat.ChainsawEmpowered;

            // Adjust spread if empowered
            if (isEmpowered)
            {
                velocity = velocity.RotatedByRandom(MathHelper.ToRadians(8f));
            }

            Vector2 spawnPos = Owner.MountedCenter + dir * BladeLength * 0.4f;

            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                spawnPos, velocity,
                ModContent.ProjectileType<RapidStreakOrbProjectile>(),
                Projectile.damage, Projectile.knockBack, Projectile.owner,
                ai0: isEmpowered ? 1f : 0f);
        }

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Rose petal burst
            for (int i = 0; i < 8; i++)
            {
                Color petalCol = Color.Lerp(OdeToJoyPalette.RosePink, OdeToJoyPalette.PetalPink, Main.rand.NextFloat());
                Dust petal = Dust.NewDustPerfect(target.Center, DustID.PinkTorch,
                    Main.rand.NextVector2CircularEdge(5f, 5f), 80, petalCol, Main.rand.NextFloat(0.9f, 1.3f));
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
            // Enhanced dash impact with rose theme
            OdeToJoyVFXLibrary.SpawnBloomBurst(target.Center, 12, 1.0f);

            for (int i = 0; i < 12; i++)
            {
                Color dustCol = (i % 3) switch
                {
                    0 => OdeToJoyPalette.RosePink,
                    1 => OdeToJoyPalette.GoldenPollen,
                    _ => OdeToJoyPalette.PetalPink
                };
                Dust petal = Dust.NewDustPerfect(target.Center, DustID.PinkTorch,
                    Main.rand.NextVector2CircularEdge(6f, 6f), 80, dustCol, Main.rand.NextFloat(1.1f, 1.5f));
                petal.noGravity = true;
            }
        }
    }
}
