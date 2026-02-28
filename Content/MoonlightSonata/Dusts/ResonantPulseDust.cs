using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using ReLogic.Content;

namespace MagnumOpus.Content.MoonlightSonata.Dusts
{
    /// <summary>
    /// Expanding resonance ring dust  Erepresents tuning-fork vibration waves.
    /// Inspired by VFX+ WindLine's directional scaling approach.
    /// Expands outward while fading, creating visible sonic-like resonance rings.
    /// Used for Incisor's precision impact effects and frequency cascade.
    /// </summary>
    public class ResonantPulseDust : ModDust
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle";

        public override void OnSpawn(Dust dust)
        {
            dust.alpha = 0;
            dust.fadeIn = 1f;
            dust.noGravity = true;
            dust.noLight = false;
            dust.frame = new Rectangle(0, 0, 128, 128);
            dust.scale *= 0.15f;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color * dust.fadeIn;
        }

        public override bool Update(Dust dust)
        {
            dust.alpha++;

            if (dust.customData is ResonantPulseBehavior behavior)
            {
                // Expand outward
                dust.scale += behavior.ExpansionRate;

                // Slow down expansion over time
                behavior.ExpansionRate *= behavior.ExpansionDecay;

                // Fade as it expands
                float lifeProgress = (float)dust.alpha / behavior.Lifetime;
                dust.fadeIn = MathHelper.Clamp(1f - lifeProgress, 0f, 1f);

                // Slight pulse overlay
                dust.fadeIn *= 0.9f + MathF.Sin(dust.alpha * behavior.PulseFrequency) * 0.1f;

                // Gentle rotation
                dust.rotation += behavior.RotationSpeed;

                // Stay at spawn position (stationary expanding ring)
                dust.velocity = Vector2.Zero;

                if (dust.alpha > behavior.Lifetime || dust.fadeIn < 0.02f)
                    dust.active = false;
            }
            else
            {
                dust.scale += 0.04f;
                float lifeProgress = (float)dust.alpha / 30f;
                dust.fadeIn = MathHelper.Clamp(1f - lifeProgress, 0f, 1f);
                dust.rotation += 0.01f;
                dust.velocity = Vector2.Zero;

                if (dust.alpha > 30 || dust.fadeIn < 0.02f)
                    dust.active = false;
            }

            if (!dust.noLight)
                Lighting.AddLight(dust.position,
                    dust.color.ToVector3() * 0.3f * dust.fadeIn * Math.Min(dust.scale, 1f));

            return false;
        }

        public override bool PreDraw(Dust dust)
        {
            var mainTex = Texture2D.Value;
            Vector2 drawPos = dust.position - Main.screenPosition;
            Vector2 origin = new Vector2(64, 64);

            // Layer 1: Outer glow ring (main)
            Main.EntitySpriteDraw(mainTex, drawPos, dust.frame,
                (dust.color with { A = 0 }) * 0.6f * dust.fadeIn,
                dust.rotation, origin, dust.scale,
                SpriteEffects.None);

            // Layer 2: Inner brighter ring (slightly smaller)
            Main.EntitySpriteDraw(mainTex, drawPos, dust.frame,
                (Color.Lerp(dust.color, Color.White, 0.4f) with { A = 0 }) * 0.4f * dust.fadeIn,
                -dust.rotation * 0.5f, origin, dust.scale * 0.7f,
                SpriteEffects.None);

            // Layer 3: Counter-rotating outer ring for depth
            Main.EntitySpriteDraw(mainTex, drawPos, dust.frame,
                (dust.color with { A = 0 }) * 0.15f * dust.fadeIn,
                dust.rotation * 1.3f, origin, dust.scale * 1.3f,
                SpriteEffects.None);

            return false;
        }
    }

    public class ResonantPulseBehavior
    {
        public float ExpansionRate = 0.06f;
        public float ExpansionDecay = 0.96f;
        public float PulseFrequency = 0.3f;
        public float RotationSpeed = 0.02f;
        public int Lifetime = 30;

        public ResonantPulseBehavior() { }

        public ResonantPulseBehavior(float expansionRate, int lifetime)
        {
            ExpansionRate = expansionRate;
            Lifetime = lifetime;
        }
    }
}
