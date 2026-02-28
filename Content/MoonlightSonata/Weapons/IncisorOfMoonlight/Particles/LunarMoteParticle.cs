using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Particles
{
    /// <summary>
    /// Soft, velocity-squished lunar glow mote.
    /// Drifts with gentle hue shift through the Moonlight Sonata purple-to-blue palette.
    /// Used for dash energy streaks, swing accents, and nova cinders.
    /// </summary>
    public class LunarMoteParticle : IncisorParticle
    {
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;
        public override bool SetLifetime => true;

        public float Opacity;
        public float SquishStrength;
        public float MaxSquish;
        public float HueShift;

        // Cache the base hue so we avoid per-frame HSL round-trips
        private float baseHue;

        public LunarMoteParticle(Vector2 position, Vector2 velocity, float scale, Color color,
            int lifetime, float opacity = 1f, float squishStrength = 1f, float maxSquish = 3f, float hueShift = 0f)
        {
            Position = position;
            Velocity = velocity;
            Scale = scale;
            Color = color;
            Opacity = opacity;
            Rotation = 0;
            Lifetime = lifetime;
            SquishStrength = squishStrength;
            MaxSquish = maxSquish;
            HueShift = hueShift;
            baseHue = Main.rgbToHsl(color).X;
        }

        public override void Update()
        {
            Velocity *= LifetimeCompletion >= 0.34f ? 0.93f : 1.02f;

            // Smooth sine-based opacity curve
            float lifeSin = (float)Math.Sin(LifetimeCompletion * MathHelper.Pi);
            Opacity = LifetimeCompletion > 0.5f ? lifeSin * 0.2f + 0.8f : lifeSin;

            Scale *= 0.96f;

            // Efficient hue shift without per-frame HSL round-trip
            if (Math.Abs(HueShift) > 0.0001f)
            {
                baseHue = (baseHue + HueShift) % 1f;
                if (baseHue < 0f) baseHue += 1f;
                Color = Main.hslToRgb(baseHue, 0.6f, 0.8f);
            }
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom").Value;
            Texture2D bloomTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom").Value;

            float squish = MathHelper.Clamp(Velocity.Length() / 10f * SquishStrength, 1f, MaxSquish);
            float rot = Velocity.ToRotation() + MathHelper.PiOver2;
            Vector2 origin = tex.Size() / 2f;
            Vector2 scale = new(Scale - Scale * squish * 0.3f, Scale * squish);
            float properBloomSize = (float)tex.Height / (float)bloomTex.Height;
            Vector2 drawPos = Position - Main.screenPosition;

            // Outer bloom halo
            spriteBatch.Draw(bloomTex, drawPos, null, Color * Opacity * 0.55f, rot,
                bloomTex.Size() / 2f, scale * 2f * properBloomSize, SpriteEffects.None, 0f);
            // Colored core
            spriteBatch.Draw(tex, drawPos, null, Color * Opacity * 0.75f, rot,
                origin, scale * 1.1f, SpriteEffects.None, 0f);
            // Bright white-hot center
            spriteBatch.Draw(tex, drawPos, null, Color.White * Opacity * 0.8f, rot,
                origin, scale * 0.5f, SpriteEffects.None, 0f);
        }
    }
}
