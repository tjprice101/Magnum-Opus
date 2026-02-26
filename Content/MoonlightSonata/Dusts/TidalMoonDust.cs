using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using ReLogic.Content;

namespace MagnumOpus.Content.MoonlightSonata.Dusts
{
    /// <summary>
    /// Flowing water-like dust for Eternal Moon  E"The Eternal Tide".
    /// Creates the feel of moonlight reflecting off ocean waves.
    /// Moves with a gentle sinusoidal drift, fading outward like retreating tides.
    /// Uses SoftGlow2.png for soft, flowing luminescence.
    /// </summary>
    public class TidalMoonDust : ModDust
    {
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/SoftGlow2";

        public override void OnSpawn(Dust dust)
        {
            dust.alpha = 0;
            dust.fadeIn = 1f;
            dust.noGravity = true;
            dust.noLight = false;
            dust.frame = new Rectangle(0, 0, 64, 64);
            dust.scale *= 0.2f;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color * dust.fadeIn;
        }

        public override bool Update(Dust dust)
        {
            dust.alpha++;

            if (dust.customData is TidalMoonBehavior behavior)
            {
                float lifeProgress = (float)dust.alpha / behavior.Lifetime;

                // Sinusoidal lateral drift  Ecreates tidal sloshing motion
                float driftPhase = dust.alpha * behavior.DriftFrequency + behavior.PhaseOffset;
                float driftAmount = MathF.Sin(driftPhase) * behavior.DriftAmplitude;
                Vector2 driftDir = new Vector2(-dust.velocity.Y, dust.velocity.X);
                if (driftDir != Vector2.Zero)
                    driftDir.Normalize();

                dust.position += dust.velocity + driftDir * driftAmount;

                // Decelerate gently  Eretreating tide feel
                dust.velocity *= behavior.VelocityDecay;

                // Scale pulse  Egentle breathing
                float scalePulse = 1f + MathF.Sin(dust.alpha * 0.15f + behavior.PhaseOffset) * 0.15f;
                dust.scale = behavior.BaseScale * scalePulse * (1f - lifeProgress * 0.4f);

                // Fade with tidal rhythm
                float fadeBase = MathHelper.Clamp(1f - lifeProgress, 0f, 1f);
                float fadeRipple = 0.9f + MathF.Sin(dust.alpha * behavior.DriftFrequency * 0.5f) * 0.1f;
                dust.fadeIn = fadeBase * fadeRipple;

                // Gentle rotation following drift
                dust.rotation += behavior.RotationSpeed * MathF.Sign(driftAmount + 0.01f);

                if (dust.alpha > behavior.Lifetime || dust.fadeIn < 0.02f)
                    dust.active = false;
            }
            else
            {
                // Default behavior  Esimple flowing fade
                dust.position += dust.velocity;
                dust.velocity *= 0.95f;
                dust.scale *= 0.98f;
                dust.fadeIn = MathHelper.Clamp(1f - (float)dust.alpha / 40f, 0f, 1f);
                dust.rotation += 0.02f;

                if (dust.alpha > 40 || dust.fadeIn < 0.02f)
                    dust.active = false;
            }

            if (!dust.noLight)
                Lighting.AddLight(dust.position,
                    dust.color.ToVector3() * 0.25f * dust.fadeIn * Math.Min(dust.scale, 1f));

            return false;
        }

        public override bool PreDraw(Dust dust)
        {
            var mainTex = Texture2D.Value;
            Vector2 drawPos = dust.position - Main.screenPosition;
            Vector2 origin = new Vector2(32, 32);

            // Layer 1: Outer tidal glow
            Main.EntitySpriteDraw(mainTex, drawPos, dust.frame,
                (dust.color with { A = 0 }) * 0.5f * dust.fadeIn,
                dust.rotation, origin, dust.scale * 1.4f,
                SpriteEffects.None);

            // Layer 2: Inner bright core
            Main.EntitySpriteDraw(mainTex, drawPos, dust.frame,
                (Color.Lerp(dust.color, Color.White, 0.3f) with { A = 0 }) * 0.35f * dust.fadeIn,
                -dust.rotation * 0.3f, origin, dust.scale * 0.8f,
                SpriteEffects.None);

            // Layer 3: Faint counter-rotating outer ring for depth
            Main.EntitySpriteDraw(mainTex, drawPos, dust.frame,
                (dust.color with { A = 0 }) * 0.12f * dust.fadeIn,
                dust.rotation * 1.5f, origin, dust.scale * 1.8f,
                SpriteEffects.None);

            return false;
        }
    }

    public class TidalMoonBehavior
    {
        public float DriftAmplitude = 2f;
        public float DriftFrequency = 0.2f;
        public float PhaseOffset;
        public float VelocityDecay = 0.96f;
        public float RotationSpeed = 0.015f;
        public float BaseScale = 0.2f;
        public int Lifetime = 35;

        public TidalMoonBehavior()
        {
            PhaseOffset = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public TidalMoonBehavior(float driftAmplitude, int lifetime)
        {
            DriftAmplitude = driftAmplitude;
            Lifetime = lifetime;
            PhaseOffset = Main.rand.NextFloat(MathHelper.TwoPi);
        }
    }
}
