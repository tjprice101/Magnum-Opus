using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.IgnitionOfTheBell.Particles
{
    /// <summary>
    /// Abstract base class for IgnitionOfTheBell particles.
    /// Each concrete type implements its own drawing with magma-focused visuals.
    /// </summary>
    public abstract class IgnitionOfTheBellParticle
    {
        public int Time;
        public int Lifetime;
        public Vector2 Position;
        public Vector2 Velocity;
        public Color DrawColor;
        public float Rotation;
        public float Scale;

        public float LifetimeCompletion => Lifetime > 0 ? (float)Time / Lifetime : 1f;

        public void SetLifetime(int lifetime) { Lifetime = lifetime; Time = 0; }

        public virtual bool UseAdditiveBlend => true;

        public virtual void Update()
        {
            Position += Velocity;
            Velocity *= 0.97f;
            Time++;
        }

        public abstract void Draw(SpriteBatch sb);

        public bool ShouldRemove => Time >= Lifetime;
    }
}
