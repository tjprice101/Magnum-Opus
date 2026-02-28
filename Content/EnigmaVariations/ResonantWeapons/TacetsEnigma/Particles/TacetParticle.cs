using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TacetsEnigma.Particles
{
    /// <summary>
    /// Abstract base for all TacetsEnigma-exclusive particles.
    /// </summary>
    public abstract class TacetParticle
    {
        public int Time;
        public int Lifetime;
        public Vector2 Position;
        public Vector2 Velocity;
        public Color Color;
        public float Rotation;
        public float Scale;

        public float LifetimeCompletion => Lifetime > 0 ? Time / (float)Lifetime : 0f;

        public virtual bool SetLifetime => false;
        public virtual bool UseAdditiveBlend => false;
        public virtual bool UseCustomDraw => false;

        public virtual void CustomDraw(SpriteBatch spriteBatch) { }
        public virtual void Update() { }

        public bool ShouldRemove => SetLifetime && Time >= Lifetime;
    }
}
