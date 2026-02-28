using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.GrandioseChime.Particles
{
    public abstract class GrandioseChimeParticle
    {
        public int Time, Lifetime;
        public Vector2 Position, Velocity;
        public Color DrawColor;
        public float Rotation, Scale;
        public float LifetimeCompletion => Lifetime > 0 ? (float)Time / Lifetime : 1f;
        public void SetLifetime(int lt) { Lifetime = lt; Time = 0; }
        public virtual bool UseAdditiveBlend => true;
        public virtual void Update() { Position += Velocity; Velocity *= 0.97f; Time++; }
        public abstract void Draw(SpriteBatch sb);
        public bool ShouldRemove => Time >= Lifetime;
    }
}
