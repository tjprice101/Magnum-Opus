using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Projectiles
{
    #region IncisorParticleHandler

    /// <summary>
    /// Particle spawner for Incisor of Moonlight effects.
    /// Accepts both Particle (MagnumParticleHandler) and IncisorParticle types.
    /// </summary>
    public static class IncisorParticleHandler
    {
        public static void SpawnParticle(Particle particle)
        {
            MagnumParticleHandler.SpawnParticle(particle);
        }

        /// <summary>
        /// Overload for self-contained IncisorParticle types (MoonlightMistParticle etc).
        /// Stub: in the future this would feed into a dedicated IncisorParticle draw system.
        /// For now, no-ops since these particles aren't in the MagnumParticleHandler pool.
        /// </summary>
        public static void SpawnParticle(IncisorParticle particle)
        {
            // IncisorParticle types use their own rendering system.
            // This stub allows compilation; a full IncisorParticle handler would manage
            // its own particle array and draw loop (similar to MagnumParticleHandler).
        }
    }

    #endregion

    #region ConstellationSparkParticle

    /// <summary>
    /// Stretched directional spark for constellation/moonlight effects.
    /// Supports additive blending and quick-shrink mode.
    /// </summary>
    public class ConstellationSparkParticle : Particle
    {
        public override string Texture => "MagnumOpus/Common/Systems/Particles/Textures/GlowDot";
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => isAdditive;

        private bool isAdditive;
        private float initialScale;
        private Vector2 stretch;
        private bool quickShrink;

        public ConstellationSparkParticle(Vector2 position, Vector2 velocity, bool additive,
            int lifetime, float scale, Color color, Vector2 stretch, bool quickShrink = false)
        {
            Position = position;
            Velocity = velocity;
            this.isAdditive = additive;
            Lifetime = lifetime;
            Scale = scale;
            Color = color;
            this.stretch = stretch;
            this.quickShrink = quickShrink;
            initialScale = scale;
            Rotation = velocity != Vector2.Zero ? velocity.ToRotation() : Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            Velocity *= 0.92f;
            float progress = LifetimeCompletion;
            if (quickShrink)
                Scale = initialScale * (1f - progress * progress);
            else
                Scale = initialScale * (1f - progress);
            Rotation = Velocity != Vector2.Zero ? Velocity.ToRotation() : Rotation;
        }
    }

    #endregion

    #region LunarMoteParticle

    /// <summary>
    /// Soft lunar mote particle — drifts gently with optional hue shifting.
    /// </summary>
    public class LunarMoteParticle : Particle
    {
        public override string Texture => "MagnumOpus/Common/Systems/Particles/Textures/GlowDot";
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;

        private float initialScale;
        private float innerRadius;
        private float outerRadius;
        private float hueShift;
        private float currentHue;

        public LunarMoteParticle(Vector2 position, Vector2 velocity, float scale, Color color,
            int lifetime, float opacity = 1f, float squishStrength = 2.5f, float maxSquish = 3f, float hueShift = 0f)
        {
            Position = position;
            Velocity = velocity;
            Scale = scale;
            Color = color * opacity;
            Lifetime = lifetime;
            this.innerRadius = squishStrength;
            this.outerRadius = maxSquish;
            this.hueShift = hueShift;
            initialScale = scale;
            currentHue = 0f;
        }

        public override void Update()
        {
            Velocity *= 0.95f;
            float progress = LifetimeCompletion;
            float fadeCurve = progress < 0.2f ? progress / 0.2f : (1f - progress) / 0.8f;
            Scale = initialScale * fadeCurve;

            // Gentle hue shifting if enabled
            if (hueShift > 0f)
            {
                currentHue += hueShift;
                float hueOffset = (float)Math.Sin(currentHue) * 0.1f;
                // Subtle color variation rather than full hue shift
                float r = MathHelper.Clamp(Color.R / 255f + hueOffset * 0.2f, 0f, 1f);
                float g = MathHelper.Clamp(Color.G / 255f + hueOffset * 0.15f, 0f, 1f);
                float b = MathHelper.Clamp(Color.B / 255f - hueOffset * 0.1f, 0f, 1f);
                Color = new Color(r, g, b) * (Color.A / 255f);
            }
        }
    }

    #endregion
}
