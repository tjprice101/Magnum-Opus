using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Particles
{
    /// <summary>
    /// Sharp, directional constellation spark particle.
    /// Appears at beam/slash impact points as bright silver-purple streaks.
    /// Can be affected by gravity for falling star effects.
    /// </summary>
    public class ConstellationSparkParticle : IncisorParticle
    {
        public Color InitialColor;
        public bool AffectedByGravity;
        public bool QuickShrink;
        public bool Glowing;
        public float ShrinkSpeed = 1;
        public Vector2 Squash = new(0.5f, 1.6f);

        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;
        public override bool UseAdditiveBlend => true;

        public ConstellationSparkParticle(Vector2 position, Vector2 velocity, bool affectedByGravity,
            int lifetime, float scale, Color color, Vector2 squash, bool quickShrink = false,
            bool glow = true, float shrinkSpeed = 1)
        {
            Position = position;
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
                Squash.X *= 1 - 0.2f * ShrinkSpeed;
                Squash.Y *= 1 + 0.2f * ShrinkSpeed;
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
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/Stars/ThinTall4PointedStar").Value;
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, Color, Rotation,
                tex.Size() * 0.5f, scale, SpriteEffects.None, 0f);

            if (Glowing)
            {
                // Bright silver-white inner core for star-like brilliance
                Color coreColor = Color.Lerp(Color.White, Color.Transparent, (float)Math.Pow(LifetimeCompletion, 3D));
                spriteBatch.Draw(tex, Position - Main.screenPosition, null, coreColor, Rotation,
                    tex.Size() * 0.5f, scale * new Vector2(0.45f, 1f), SpriteEffects.None, 0f);

                // Soft outer bloom glow
                Texture2D bloomTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom").Value;
                Color bloomColor = Color * 0.3f;
                bloomColor.A = 0;
                spriteBatch.Draw(bloomTex, Position - Main.screenPosition, null, bloomColor, 0f,
                    bloomTex.Size() * 0.5f, Scale * 0.6f, SpriteEffects.None, 0f);
            }
        }
    }
}
