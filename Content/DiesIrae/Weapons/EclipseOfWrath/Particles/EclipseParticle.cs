using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace MagnumOpus.Content.DiesIrae.Weapons.EclipseOfWrath.Particles
{
    /// <summary>
    /// Abstract base class for Eclipse of Wrath particles.
    /// </summary>
    public abstract class EclipseParticle
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
