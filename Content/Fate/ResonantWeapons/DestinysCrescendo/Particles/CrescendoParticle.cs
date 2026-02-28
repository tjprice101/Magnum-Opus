using Microsoft.Xna.Framework;

namespace MagnumOpus.Content.Fate.ResonantWeapons.DestinysCrescendo
{
    /// <summary>
    /// Lightweight particle struct for the self-contained Destiny's Crescendo particle pool.
    /// Updated and drawn by <see cref="CrescendoParticleHandler"/>.
    /// </summary>
    public struct CrescendoParticle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Color Color;
        public float Scale;
        public float Rotation;
        public float RotationSpeed;
        public int TimeLeft;
        public int MaxTime;
        public CrescendoParticleType Type;
        public bool Active;
        public float Opacity;

        /// <summary>0 at birth → 1 at death.</summary>
        public float Progress => MaxTime > 0 ? 1f - (float)TimeLeft / MaxTime : 1f;

        public void Update()
        {
            if (!Active) return;

            Position += Velocity;
            Velocity *= 0.96f;
            Rotation += RotationSpeed;
            TimeLeft--;

            // Smooth fade envelope: 12% fade-in, hold, 40% fade-out
            if (Progress < 0.12f)
                Opacity = Progress / 0.12f;
            else if (Progress > 0.60f)
                Opacity = (1f - Progress) / 0.40f;
            else
                Opacity = 1f;

            if (TimeLeft <= 0)
                Active = false;
        }
    }
}
