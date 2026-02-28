namespace MagnumOpus.Content.Fate.ResonantWeapons.ResonanceOfABygoneReality
{
    /// <summary>
    /// Particle type enum for Resonance weapon effects.
    /// Each type has distinct bloom scale and gravity behavior.
    /// </summary>
    public enum ResonanceParticleType
    {
        BulletGlow,
        MuzzleSpark,
        CosmicTrail,
        BladeArc,
        EchoRing,
        MemoryWisp
    }

    /// <summary>
    /// Per-type particle parameters (bloom scale, gravity).
    /// </summary>
    public static class ResonanceParticleTypes
    {
        /// <summary>
        /// Returns the bloom scale multiplier for the given particle type.
        /// </summary>
        public static float GetBloomScale(ResonanceParticleType type)
        {
            return type switch
            {
                ResonanceParticleType.BulletGlow => 2.5f,
                ResonanceParticleType.MuzzleSpark => 1.8f,
                ResonanceParticleType.CosmicTrail => 2.0f,
                ResonanceParticleType.BladeArc => 3.0f,
                ResonanceParticleType.EchoRing => 3.5f,
                ResonanceParticleType.MemoryWisp => 2.2f,
                _ => 2.0f
            };
        }

        /// <summary>
        /// Returns the per-frame gravity for the given particle type (0 = none).
        /// </summary>
        public static float GetGravity(ResonanceParticleType type)
        {
            return type switch
            {
                ResonanceParticleType.MuzzleSpark => 0.03f,
                ResonanceParticleType.MemoryWisp => -0.01f, // Floats upward
                _ => 0f
            };
        }
    }
}
