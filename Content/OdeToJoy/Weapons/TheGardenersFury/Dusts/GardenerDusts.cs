using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.OdeToJoy.Dusts;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TheGardenersFury.Dusts
{
    /// <summary>
    /// PetalFragmentDust — Rose petal fragments that flutter and drift.
    /// Used for swing arcs, impacts, petal projectile trails.
    /// Features gentle tumble rotation and sine-wave drift.
    /// </summary>
    public class PetalFragmentDust : OdeToJoyDustBase
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Projectiles/OJ Rose Petal";

        protected override void OnSpawnExtra(Dust dust)
        {
            dust.frame = new Rectangle(0, 0, 64, 64);
            dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            dust.noGravity = true;
            dust.scale *= Main.rand.NextFloat(0.8f, 1.2f);

            if (dust.customData is OdeToJoyDustBehavior b)
            {
                b.RotationSpeed = Main.rand.NextFloat(-0.08f, 0.08f);
                b.VelocityDecay = 0.96f;
                b.ScaleDecay = 0.985f;
                b.FadeDecay = 0.97f;
                b.DriftAmplitude = Main.rand.NextFloat(0.3f, 0.8f);
                b.DriftFrequency = Main.rand.NextFloat(0.04f, 0.08f);
            }
        }

        protected override void UpdateBehavior(Dust dust)
        {
            if (dust.customData is OdeToJoyDustBehavior b)
            {
                dust.rotation += b.RotationSpeed;
                dust.velocity *= b.VelocityDecay;
                dust.scale *= b.ScaleDecay;
                dust.color *= b.FadeDecay;

                // Flutter drift — gentle sine wave perpendicular to velocity
                b.LifeTimer++;
                float drift = MathF.Sin(b.LifeTimer * b.DriftFrequency) * b.DriftAmplitude;
                Vector2 perpendicular = new Vector2(-dust.velocity.Y, dust.velocity.X).SafeNormalize(Vector2.UnitX);
                dust.position += perpendicular * drift;

                // Slight upward tendency (petals float)
                dust.velocity.Y -= 0.02f;

                if (dust.scale < 0.15f || dust.color.A < 10)
                    dust.active = false;
            }
        }
    }

    /// <summary>
    /// PollenMistDust — Golden pollen motes that drift slowly upward.
    /// Used for pollen pod effects, ambient particles, slow debuff visuals.
    /// Features slow upward drift with gentle horizontal wander.
    /// </summary>
    public class PollenMistDust : OdeToJoyDustBase
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow";

        protected override void OnSpawnExtra(Dust dust)
        {
            dust.frame = new Rectangle(0, 0, 128, 128);
            dust.rotation = 0f;
            dust.noGravity = true;
            dust.scale *= Main.rand.NextFloat(0.5f, 0.9f);

            if (dust.customData is OdeToJoyDustBehavior b)
            {
                b.RotationSpeed = 0f;
                b.VelocityDecay = 0.94f;
                b.ScaleDecay = 0.99f;
                b.FadeDecay = 0.965f;
                b.DriftAmplitude = Main.rand.NextFloat(0.2f, 0.5f);
                b.DriftFrequency = Main.rand.NextFloat(0.03f, 0.06f);
            }
        }

        protected override void UpdateBehavior(Dust dust)
        {
            if (dust.customData is OdeToJoyDustBehavior b)
            {
                dust.velocity *= b.VelocityDecay;
                dust.scale *= b.ScaleDecay;
                dust.color *= b.FadeDecay;

                // Upward drift with horizontal wander
                b.LifeTimer++;
                dust.velocity.Y -= 0.04f;
                dust.velocity.X += MathF.Sin(b.LifeTimer * b.DriftFrequency) * b.DriftAmplitude * 0.1f;

                if (dust.scale < 0.1f || dust.color.A < 10)
                    dust.active = false;
            }
        }

        protected override void DrawExtraLayers(Dust dust, Texture2D mainTex, Vector2 drawPos, Vector2 origin, float fade)
        {
            // Extra wide outer glow for pollen mist feel
            if (mainTex == null) return;
            Color wideGlow = (dust.color with { A = 0 }) * 0.15f * fade;
            drawPos = dust.position - Main.screenPosition;
            Main.EntitySpriteDraw(mainTex, drawPos, null, wideGlow, 0f, origin, dust.scale * 2.5f, SpriteEffects.None);
        }
    }
}
