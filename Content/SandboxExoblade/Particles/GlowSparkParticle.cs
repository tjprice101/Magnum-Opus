using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.SandboxExoblade.Particles
{
    public class ExoGlowSparkParticle : ExoParticle
    {
        public Color InitialColor;
        public bool AffectedByGravity;
        public bool QuickShrink;
        public bool Glowing;
        public float ShrinkSpeed = 1;
        public Vector2 Squash = new Vector2(0.5f, 1.6f);
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;
        public override bool UseAdditiveBlend => true;

        public ExoGlowSparkParticle(Vector2 relativePosition, Vector2 velocity, bool affectedByGravity, int lifetime, float scale, Color color, Vector2 squash, bool quickShrink = false, bool glow = true, float shrinkSpeed = 1)
        {
            Position = relativePosition;
            Velocity = velocity;
            AffectedByGravity = affectedByGravity;
            Scale = scale;
            Lifetime = lifetime;
            Color = InitialColor = color;
            Squash = squash;
            QuickShrink = quickShrink;
            Glowing = glow;
            Rotation = Velocity.ToRotation() + MathHelper.PiOver2;
            ShrinkSpeed = shrinkSpeed;
        }

        public override void Update()
        {
            Scale *= 0.95f;
            Color = Color.Lerp(InitialColor, Color.Transparent, (float)Math.Pow(LifetimeCompletion, 3D));
            Velocity *= 0.95f;
            if (QuickShrink)
            {
                if (ShrinkSpeed == 1)
                {
                    Squash.X *= 0.8f;
                    Squash.Y *= 1.2f;
                }
                else
                {
                    Squash.X *= (1 - 0.2f * ShrinkSpeed);
                    Squash.Y *= (1 + 0.2f * ShrinkSpeed);
                }
            }
            if (Velocity.Length() < 12f && AffectedByGravity)
            {
                Velocity.X *= 0.94f;
                Velocity.Y += 0.25f;
            }
            Rotation = Velocity.ToRotation() + MathHelper.PiOver2;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Vector2 scale = Squash * Scale;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Content/SandboxExoblade/Particles/GlowSpark").Value;
            spriteBatch.Draw(texture, Position - Main.screenPosition, null, Color, Rotation, texture.Size() * 0.5f, scale, 0, 0f);
            if (Glowing)
                spriteBatch.Draw(texture, Position - Main.screenPosition, null, Color.Lerp(Color.White, Color.Transparent, (float)Math.Pow(LifetimeCompletion, 3D)), Rotation, texture.Size() * 0.5f, scale * new Vector2(0.45f, 1f), 0, 0f);
        }
    }
}
