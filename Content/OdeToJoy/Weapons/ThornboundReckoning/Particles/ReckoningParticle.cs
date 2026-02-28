using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Particles
{
    /// <summary>
    /// Abstract base class for Thornbound Reckoning particles.
    /// Self-contained particle type — no global systems.
    /// </summary>
    public abstract class ReckoningParticle
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
