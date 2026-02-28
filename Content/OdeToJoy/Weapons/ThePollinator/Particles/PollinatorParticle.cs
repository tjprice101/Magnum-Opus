using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThePollinator.Particles
{
    /// <summary>
    /// Abstract base class for The Pollinator particles.
    /// Self-contained particle type — no global systems.
    /// </summary>
    public abstract class PollinatorParticle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Rotation;
        public float Scale;
        public Color DrawColor;
        public int Lifetime;
        public int MaxLifetime;
        public bool IsAdditive;
        public bool Active = true;

        /// <summary>
        /// Normalized progress from 0 (spawn) to 1 (death).
        /// </summary>
        public float LifeRatio => MaxLifetime > 0 ? (float)Lifetime / MaxLifetime : 0f;

        public virtual void Update()
        {
            Position += Velocity;
            Lifetime++;
            if (Lifetime >= MaxLifetime)
                Active = false;
        }

        public abstract void Draw(SpriteBatch sb);
    }
}
