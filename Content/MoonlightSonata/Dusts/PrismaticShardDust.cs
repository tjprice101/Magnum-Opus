using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using ReLogic.Content;

namespace MagnumOpus.Content.MoonlightSonata.Dusts
{
    /// <summary>
    /// Prismatic crystalline shard dust for Moonlight's Calling — "The Serenade".
    /// Creates the feel of light refracting through a crystal prism into spectral colors.
    /// Rotates as it floats, cycling through hue over its lifetime.
    /// Uses CrispStar4.png for sharp, crystalline sparkle appearance.
    /// </summary>
    public class PrismaticShardDust : ModDust
    {
        public override string Texture => "MagnumOpus/Assets/Particles/CrispStar4";

        public override void OnSpawn(Dust dust)
        {
            dust.alpha = 0;
            dust.fadeIn = 1f;
            dust.noGravity = true;
            dust.noLight = false;
            dust.frame = new Rectangle(0, 0, 32, 32);
            dust.scale *= 0.25f;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color * dust.fadeIn;
        }

        public override bool Update(Dust dust)
        {
            dust.alpha++;

            if (dust.customData is PrismaticShardBehavior behavior)
            {
                float lifeProgress = (float)dust.alpha / behavior.Lifetime;

                // Hue cycling — refracts through spectral range over lifetime
                float hue = (behavior.BaseHue + lifeProgress * behavior.HueRange
                    + Main.GlobalTimeWrappedHourly * behavior.HueCycleSpeed) % 1f;
                dust.color = Main.hslToRgb(hue, behavior.Saturation, behavior.Lightness);

                // Movement with gentle deceleration
                dust.position += dust.velocity;
                dust.velocity *= behavior.VelocityDecay;

                // Rotation — spinning crystal shard
                dust.rotation += behavior.RotationSpeed;

                // Scale pulse — twinkling like light hitting a facet
                float twinkle = MathF.Sin(dust.alpha * behavior.TwinkleFrequency + behavior.PhaseOffset);
                float twinkleScale = 1f + twinkle * 0.3f;
                dust.scale = behavior.BaseScale * twinkleScale * (1f - lifeProgress * 0.5f);

                // Fade — rapid fade-in, slow fade-out
                if (dust.alpha < 4)
                    dust.fadeIn = dust.alpha / 4f;
                else
                    dust.fadeIn = MathHelper.Clamp(1f - (lifeProgress - 0.3f) / 0.7f, 0f, 1f);

                if (dust.alpha > behavior.Lifetime || dust.fadeIn < 0.02f)
                    dust.active = false;
            }
            else
            {
                // Default behavior — simple spectral fade
                dust.position += dust.velocity;
                dust.velocity *= 0.96f;
                dust.rotation += 0.08f;
                dust.scale *= 0.97f;

                float hue = (0.7f + (float)dust.alpha * 0.015f) % 1f;
                dust.color = Main.hslToRgb(hue, 0.8f, 0.7f);
                dust.fadeIn = MathHelper.Clamp(1f - (float)dust.alpha / 35f, 0f, 1f);

                if (dust.alpha > 35 || dust.fadeIn < 0.02f)
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
            Vector2 origin = new Vector2(16, 16);

            // Layer 1: Outer prismatic glow (larger, softer)
            Main.EntitySpriteDraw(mainTex, drawPos, dust.frame,
                (dust.color with { A = 0 }) * 0.45f * dust.fadeIn,
                dust.rotation, origin, dust.scale * 1.6f,
                SpriteEffects.None);

            // Layer 2: Inner bright core (smaller, sharper)
            Color coreColor = Color.Lerp(dust.color, Color.White, 0.4f);
            Main.EntitySpriteDraw(mainTex, drawPos, dust.frame,
                (coreColor with { A = 0 }) * 0.6f * dust.fadeIn,
                -dust.rotation * 0.5f, origin, dust.scale * 0.9f,
                SpriteEffects.None);

            // Layer 3: Faint counter-rotating halo for depth
            Main.EntitySpriteDraw(mainTex, drawPos, dust.frame,
                (dust.color with { A = 0 }) * 0.15f * dust.fadeIn,
                dust.rotation * 2f, origin, dust.scale * 2.0f,
                SpriteEffects.None);

            return false;
        }
    }

    public class PrismaticShardBehavior
    {
        public float BaseHue = 0.7f;          // Starting hue (purple)
        public float HueRange = 0.3f;         // How far through spectrum to cycle
        public float HueCycleSpeed = 0.5f;    // Speed of ambient hue cycling
        public float Saturation = 0.8f;
        public float Lightness = 0.7f;
        public float VelocityDecay = 0.96f;
        public float RotationSpeed = 0.08f;
        public float TwinkleFrequency = 0.4f;
        public float PhaseOffset;
        public float BaseScale = 0.25f;
        public int Lifetime = 30;

        public PrismaticShardBehavior()
        {
            PhaseOffset = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public PrismaticShardBehavior(float baseHue, float hueRange, int lifetime)
        {
            BaseHue = baseHue;
            HueRange = hueRange;
            Lifetime = lifetime;
            PhaseOffset = Main.rand.NextFloat(MathHelper.TwoPi);
        }
    }
}
