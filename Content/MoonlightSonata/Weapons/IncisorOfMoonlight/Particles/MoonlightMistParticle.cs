using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using ReLogic.Content;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Particles
{
    /// <summary>
    /// Soft moonlight mist particle 遯ｶ繝ｻa gently fading, slowly drifting glow puff.
    /// Used for impact smoke and lunar nova explosion clouds.
    /// Non-spritesheet: uses a single soft glow texture with rotation and fade.
    /// </summary>
    public class MoonlightMistParticle : IncisorParticle
    {
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;
        public override bool UseAdditiveBlend => Glowing;

        private float Opacity;
        private float Spin;
        private bool Glowing;
        private float HueShift;

        public MoonlightMistParticle(Vector2 position, Vector2 velocity, Color color,
            int lifetime, float scale, float opacity, float rotationSpeed = 0f,
            bool glowing = false, float hueShift = 0f)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Scale = scale;
            Lifetime = lifetime;
            Opacity = opacity;
            Spin = rotationSpeed;
            Glowing = glowing;
            HueShift = hueShift;
        }

        public override void Update()
        {
            if (Time / (float)Lifetime < 0.2f)
                Scale += 0.015f;
            else
                Scale *= 0.975f;

            Color = Main.hslToRgb(
                (Main.rgbToHsl(Color).X + HueShift) % 1,
                Main.rgbToHsl(Color).Y,
                Main.rgbToHsl(Color).Z);

            Opacity *= 0.98f;
            Rotation += Spin * ((Velocity.X > 0) ? 1f : -1f);
            Velocity *= 0.85f;

            float fadeOut = Utils.GetLerpValue(1f, 0.85f, LifetimeCompletion, true);
            Color *= fadeOut;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle", AssetRequestMode.ImmediateLoad).Value;
            Color col = Color * Opacity;
            Vector2 origin = tex.Size() / 2f;
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, col, Rotation,
                origin, Scale, SpriteEffects.None, 0f);

            // Secondary softer layer for depth
            Color glowCol = col * 0.35f;
            glowCol.A = 0;
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, glowCol, Rotation,
                origin, Scale * 1.5f, SpriteEffects.None, 0f);
        }
    }
}