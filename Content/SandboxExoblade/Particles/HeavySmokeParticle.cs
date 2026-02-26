using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.SandboxExoblade.Particles
{
    public class ExoHeavySmokeParticle : ExoParticle
    {
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;
        public override bool UseAdditiveBlend => Glowing;

        private float Opacity;
        private float Spin;
        private bool Glowing;
        private float HueShift;
        private int Variant;
        static int FrameAmount = 6;

        public ExoHeavySmokeParticle(Vector2 position, Vector2 velocity, Color color, int lifetime, float scale, float opacity, float rotationSpeed = 0f, bool glowing = false, float hueshift = 0f, bool required = false, bool affectedByLight = false)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Scale = scale;
            Variant = Main.rand.Next(7);
            Lifetime = lifetime;
            Opacity = opacity;
            Spin = rotationSpeed;
            Glowing = glowing;
            HueShift = hueshift;
        }

        public override void Update()
        {
            if (Time / (float)Lifetime < 0.2f)
                Scale += 0.01f;
            else
                Scale *= 0.975f;
            Color = Main.hslToRgb((Main.rgbToHsl(Color).X + HueShift) % 1, Main.rgbToHsl(Color).Y, Main.rgbToHsl(Color).Z);
            Opacity *= 0.98f;
            Rotation += Spin * ((Velocity.X > 0) ? 1f : -1f);
            Velocity *= 0.85f;
            float opacity = Utils.GetLerpValue(1f, 0.85f, LifetimeCompletion, true);
            Color *= opacity;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Content/SandboxExoblade/Particles/HeavySmoke").Value;
            int animationFrame = (int)Math.Floor(Time / ((float)(Lifetime / (float)FrameAmount)));
            Rectangle frame = new Rectangle(80 * Variant, 80 * animationFrame, 80, 80);
            Color col = Color * Opacity;
            spriteBatch.Draw(tex, Position - Main.screenPosition, frame, col, Rotation, frame.Size() / 2f, Scale, SpriteEffects.None, 0);
        }
    }
}
