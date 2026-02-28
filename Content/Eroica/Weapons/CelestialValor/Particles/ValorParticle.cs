using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MagnumOpus.Content.Eroica.Weapons.CelestialValor.Particles
{
    public abstract class ValorParticle
    {
        public int Time;
        public int Lifetime;
        public Vector2 Position;
        public Vector2 Velocity;
        public Color DrawColor;
        public float Rotation;
        public float Scale;

        public float LifetimeCompletion => Lifetime > 0 ? Time / (float)Lifetime : 0f;

        public virtual bool SetLifetime => false;
        public virtual bool UseAdditiveBlend => false;
        public virtual bool UseCustomDraw => false;

        public bool ShouldRemove => SetLifetime && Time >= Lifetime;

        public virtual void Update() { }
        public virtual void CustomDraw(SpriteBatch spriteBatch) { }
    }
}
