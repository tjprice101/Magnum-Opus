using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon.Particles
{
    /// <summary>
    /// Abstract base class for Resurrection of the Moon's self-contained particle system.
    /// Mirrors the Serenade/Eternal Moon pattern but scoped to the Comet weapon.
    /// </summary>
    public abstract class CometParticle
    {
        public int Time;
        public int Lifetime;
        public Vector2 Position;
        public Vector2 Velocity;
        public Color DrawColor;
        public float Rotation;
        public float Scale;

        /// <summary>Progress from 0 (spawn) to 1 (death).</summary>
        public float LifetimeCompletion => Lifetime != 0 ? Time / (float)Lifetime : 0f;

        /// <summary>Whether this particle has a finite lifetime and should be auto-removed.</summary>
        public virtual bool SetLifetime => false;

        /// <summary>Whether to draw in the additive blend pass.</summary>
        public virtual bool UseAdditiveBlend => false;

        /// <summary>Whether this particle uses CustomDraw instead of default sprite rendering.</summary>
        public virtual bool UseCustomDraw => false;

        /// <summary>Called each frame to update particle state.</summary>
        public virtual void Update() { }

        /// <summary>Called during the appropriate blend pass to render the particle.</summary>
        public virtual void CustomDraw(SpriteBatch spriteBatch) { }

        /// <summary>Whether this particle should be removed.</summary>
        public bool ShouldRemove => SetLifetime && Time >= Lifetime;
    }
}
