using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MagnumOpus.Content.Fate.ResonantWeapons.LightOfTheFuture.Particles
{
    /// <summary>
    /// Abstract base class for all Light of the Future particles.
    /// Self-contained — no dependency on global particle systems.
    /// </summary>
    public abstract class LightParticle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Color DrawColor;
        public float Scale;
        public float Rotation;
        public float RotationSpeed;
        public int Lifetime;
        public int TimeAlive;
        public bool UseAdditiveBlend;
        public bool Active = true;

        /// <summary>0 → 1 over the particle's lifetime.</summary>
        public float LifetimeCompletion => Lifetime <= 0 ? 1f : (float)TimeAlive / Lifetime;

        /// <summary>Update position, velocity, state each frame. Return false to kill.</summary>
        public virtual bool Update()
        {
            if (!Active) return false;

            Position += Velocity;
            Rotation += RotationSpeed;
            TimeAlive++;

            if (TimeAlive >= Lifetime)
            {
                Active = false;
                return false;
            }

            return true;
        }

        /// <summary>Draw the particle. Called with the appropriate blend state already set.</summary>
        public abstract void Draw(SpriteBatch sb);
    }
}
