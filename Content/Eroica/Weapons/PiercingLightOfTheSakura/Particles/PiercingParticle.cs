using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MagnumOpus.Content.Eroica.Weapons.PiercingLightOfTheSakura.Particles
{
    public abstract class PiercingParticle
    {
        public int Time;
        public int Lifetime;
        public Vector2 Position;
        public Vector2 Velocity;
        public Color DrawColor;
        public float Rotation;
        public float Scale;

        public float LifetimeCompletion => Lifetime <= 0 ? 1f : (float)Time / Lifetime;

        public virtual bool SetLifetime => false;
        public virtual bool UseAdditiveBlend => false;
        public virtual bool UseCustomDraw => false;

        public abstract void Update();
        public virtual void CustomDraw(SpriteBatch spriteBatch) { }

        public bool ShouldRemove()
        {
            if (SetLifetime && Time >= Lifetime) return true;
            return false;
        }

        public void StandardUpdate()
        {
            Position += Velocity;
            Time++;
        }
    }
}
