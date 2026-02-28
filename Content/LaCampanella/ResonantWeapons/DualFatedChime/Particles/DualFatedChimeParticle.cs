using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.DualFatedChime.Particles
{
    /// <summary>
    /// Abstract base class for all Dual Fated Chime particles.
    /// Self-contained particle system — no dependency on shared mod particle infrastructure.
    /// </summary>
    public abstract class DualFatedChimeParticle
    {
        public int Time;
        public int Lifetime;
        public Vector2 Position;
        public Vector2 Velocity;
        public Color DrawColor;
        public float Rotation;
        public float Scale;

        /// <summary>0→1 lifecycle progress.</summary>
        public float LifetimeCompletion => Lifetime > 0 ? (float)Time / Lifetime : 0f;

        /// <summary>If true, particle auto-removes when Time >= Lifetime.</summary>
        public virtual bool SetLifetime => false;

        /// <summary>If true, uses additive blending.</summary>
        public virtual bool UseAdditiveBlend => false;

        /// <summary>Override to update behavior each frame.</summary>
        public virtual void Update() { }

        /// <summary>Override to draw the particle.</summary>
        public virtual void Draw(SpriteBatch spriteBatch) { }

        /// <summary>Whether this particle should be removed.</summary>
        public bool ShouldRemove => SetLifetime && Time >= Lifetime;
    }
}
