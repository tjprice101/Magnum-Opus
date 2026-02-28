using Microsoft.Xna.Framework;
using System;

namespace MagnumOpus.Content.Fate.ResonantWeapons.TheFinalFermata.Particles
{
    /// <summary>
    /// Lightweight self-contained particle for Fermata VFX.
    /// No dependency on any shared particle system.
    /// </summary>
    public class FermataParticle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Color DrawColor;
        public float Scale;
        public float Rotation;
        public float RotationSpeed;
        public int LifeTime;
        public int MaxLifeTime;
        public FermataParticleType Type;
        public bool Active;
        public bool Additive;
        public float Opacity;

        /// <summary>Progress from 0 (spawn) to 1 (death).</summary>
        public float Progress => MaxLifeTime > 0 ? (float)LifeTime / MaxLifeTime : 1f;

        public void Update()
        {
            if (!Active) return;

            Position += Velocity;
            Velocity *= 0.97f;
            Rotation += RotationSpeed;
            LifeTime++;

            // Fade out over the last 30% of lifetime
            float fadeStart = 0.7f;
            if (Progress > fadeStart)
            {
                Opacity = 1f - (Progress - fadeStart) / (1f - fadeStart);
            }

            // Scale shrink in last 20%
            if (Progress > 0.8f)
            {
                Scale *= 0.96f;
            }

            if (LifeTime >= MaxLifeTime)
                Active = false;
        }
    }

    /// <summary>
    /// The 6 Fermata particle types.
    /// </summary>
    public enum FermataParticleType
    {
        FermataMote,        // Soft drifting mote
        FermataSpark,       // Sharp directional spark
        FermataTimeShard,   // Frozen crystal-like shard
        FermataGlyph,       // Temporal glyph cross symbol
        FermataBloomFlare,  // Bright bloom flash
        FermataNebulaWisp   // Wispy nebula trail
    }
}
