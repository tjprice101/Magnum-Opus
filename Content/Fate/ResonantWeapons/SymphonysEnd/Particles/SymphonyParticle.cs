using Microsoft.Xna.Framework;

namespace MagnumOpus.Content.Fate.ResonantWeapons.SymphonysEnd
{
    /// <summary>
    /// Lightweight particle struct for the self-contained Symphony's End particle pool.
    /// Updated and drawn by <see cref="SymphonyParticleHandler"/>.
    /// </summary>
    public struct SymphonyParticle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Color Color;
        public float Scale;
        public float Rotation;
        public float RotationSpeed;
        public int TimeLeft;
        public int MaxTime;
        public SymphonyParticleType Type;
        public bool Active;
        public float Opacity;
        public bool Additive;

        /// <summary>0 at birth → 1 at death.</summary>
        public float Progress => MaxTime > 0 ? 1f - (float)TimeLeft / MaxTime : 1f;

        public void Update()
        {
            if (!Active) return;

            Position += Velocity;
            Velocity *= 0.97f;
            Rotation += RotationSpeed;
            TimeLeft--;

            // Smooth fade envelope: 15 % fade-in, hold, 35 % fade-out
            if (Progress < 0.15f)
                Opacity = Progress / 0.15f;
            else if (Progress > 0.65f)
                Opacity = (1f - Progress) / 0.35f;
            else
                Opacity = 1f;

            if (TimeLeft <= 0)
                Active = false;
        }
    }
}
