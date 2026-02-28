using Microsoft.Xna.Framework;

namespace MagnumOpus.Content.Fate.ResonantWeapons.ResonanceOfABygoneReality
{
    /// <summary>
    /// Lightweight particle struct for Resonance weapon effects.
    /// Pure value type — no allocations, pooled in a fixed-size array.
    /// </summary>
    public struct ResonanceParticle
    {
        public bool Active;
        public ResonanceParticleType Type;
        public Vector2 Position;
        public Vector2 Velocity;
        public Color Color;
        public float Scale;
        public float Rotation;
        public float RotationSpeed;
        public int Life;
        public int MaxLife;
        public float Opacity;

        /// <summary>
        /// Progress from 0 (just spawned) to 1 (about to die).
        /// </summary>
        public float Progress => MaxLife > 0 ? 1f - (float)Life / MaxLife : 1f;

        public void Update()
        {
            if (!Active) return;

            Position += Velocity;
            Rotation += RotationSpeed;
            Life--;

            if (Life <= 0)
            {
                Active = false;
                return;
            }

            // Apply per-type gravity
            Velocity.Y += ResonanceParticleTypes.GetGravity(Type);

            // Fade out in last 30% of life
            float t = Progress;
            Opacity = t < 0.7f ? 1f : 1f - (t - 0.7f) / 0.3f;
        }
    }
}
