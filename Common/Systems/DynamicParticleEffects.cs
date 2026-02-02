using Microsoft.Xna.Framework;
using System;
using Terraria;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// Static helper class for spawning dynamic particles with preset configurations.
    /// Makes it easy to create complex, aesthetically pleasing effects with simple method calls.
    /// 
    /// === USAGE PHILOSOPHY ===
    /// These methods provide PRESETS for common use cases. For full customization,
    /// instantiate the particle classes directly with custom parameters.
    /// </summary>
    public static class DynamicParticleEffects
    {
        #region Pulsing Effects
        
        /// <summary>
        /// Spawns a pulsing bloom that breathes between two colors.
        /// Perfect for: Magical cores, hearts, power sources, enchanted items.
        /// </summary>
        public static void PulsingGlow(Vector2 position, Vector2 velocity, Color primary, Color secondary, 
            float scale, int lifetime, float pulseSpeed = 0.15f, float pulseAmount = 0.3f)
        {
            var particle = new PulsingBloomParticle(position, velocity, primary, secondary,
                scale, lifetime, pulseAmount, pulseSpeed, true);
            MagnumParticleHandler.SpawnParticle(particle);
        }
        
        /// <summary>
        /// Spawns multiple pulsing blooms in a burst pattern.
        /// Perfect for: Impact effects, magical explosions, ability activations.
        /// </summary>
        public static void PulsingBurst(Vector2 center, Color primary, Color secondary, 
            int count, float speed, float scale, int lifetime)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                Vector2 vel = angle.ToRotationVector2() * speed * Main.rand.NextFloat(0.8f, 1.2f);
                
                float pulseOffset = Main.rand.NextFloat(0f, MathHelper.TwoPi);
                var particle = new PulsingBloomParticle(center, vel, primary, secondary,
                    scale * Main.rand.NextFloat(0.8f, 1.2f), lifetime, 
                    Main.rand.NextFloat(0.2f, 0.4f), Main.rand.NextFloat(0.1f, 0.2f), true);
                MagnumParticleHandler.SpawnParticle(particle);
            }
        }
        
        /// <summary>
        /// Spawns twinkling sparkles in a scattered area.
        /// Perfect for: Fairy dust, magical auras, enchantment effects.
        /// </summary>
        public static void TwinklingSparks(Vector2 center, Color color, int count, 
            float spread, float scale, int lifetime)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(spread, spread);
                Vector2 vel = Main.rand.NextVector2Circular(1f, 1f);
                
                var particle = new TwinklingSparkleParticle(center + offset, vel, color, 
                    color * 0.5f, scale * 0.5f, scale * 1.5f, lifetime,
                    Main.rand.NextFloat(0.15f, 0.25f), Main.rand.NextFloat(1.2f, 1.8f));
                MagnumParticleHandler.SpawnParticle(particle);
            }
        }
        
        #endregion
        
        #region Spiral/Orbit Effects
        
        /// <summary>
        /// Spawns particles in an outward spiral pattern.
        /// Perfect for: Vortex effects, galaxy bursts, magical releases.
        /// </summary>
        public static void SpiralBurst(Vector2 center, Color startColor, Color endColor,
            int count, float angularSpeed, float radialSpeed, float scale, int lifetime)
        {
            for (int i = 0; i < count; i++)
            {
                float startAngle = MathHelper.TwoPi * i / count;
                float startRadius = Main.rand.NextFloat(5f, 15f);
                
                var particle = new SpiralParticle(center, startRadius, startAngle,
                    angularSpeed * Main.rand.NextFloat(0.8f, 1.2f),
                    radialSpeed * Main.rand.NextFloat(0.9f, 1.1f),
                    startColor, endColor, scale, lifetime, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }
        }
        
        /// <summary>
        /// Spawns particles in an inward spiral (vortex/vacuum effect).
        /// Perfect for: Absorption effects, black hole visuals, gathering energy.
        /// </summary>
        public static void SpiralVortex(Vector2 center, Color startColor, Color endColor,
            int count, float startRadius, float angularSpeed, float radialSpeed, float scale, int lifetime)
        {
            for (int i = 0; i < count; i++)
            {
                float startAngle = MathHelper.TwoPi * i / count;
                
                var particle = new SpiralParticle(center, startRadius * Main.rand.NextFloat(0.8f, 1.2f), 
                    startAngle, angularSpeed * Main.rand.NextFloat(0.9f, 1.1f),
                    radialSpeed * Main.rand.NextFloat(0.9f, 1.1f),
                    startColor, endColor, scale, lifetime, false); // false = spiral inward
                MagnumParticleHandler.SpawnParticle(particle);
            }
        }
        
        /// <summary>
        /// Spawns orbiting particles around a center point.
        /// Perfect for: Auras, shields, magical rings, planetary effects.
        /// </summary>
        public static void OrbitingRing(Vector2 center, Color color, int count,
            float radius, float angularSpeed, float scale, int lifetime, string texture = "SoftGlow")
        {
            for (int i = 0; i < count; i++)
            {
                float startAngle = MathHelper.TwoPi * i / count;
                
                var particle = new OrbitingParticle(center, radius, startAngle, angularSpeed,
                    color, scale, lifetime, texture, false, 0.15f, 0.1f);
                MagnumParticleHandler.SpawnParticle(particle);
            }
        }
        
        /// <summary>
        /// Spawns multiple concentric orbiting rings with different speeds.
        /// Perfect for: Complex auras, magical shields, layered effects.
        /// </summary>
        public static void ConcentricOrbits(Vector2 center, Color innerColor, Color outerColor,
            int rings, int particlesPerRing, float baseRadius, float radiusStep,
            float baseSpeed, float scale, int lifetime)
        {
            for (int ring = 0; ring < rings; ring++)
            {
                float ringProgress = (float)ring / rings;
                Color ringColor = Color.Lerp(innerColor, outerColor, ringProgress);
                float ringRadius = baseRadius + ring * radiusStep;
                float ringSpeed = baseSpeed * (1f - ringProgress * 0.3f); // Outer rings slower
                
                // Alternate direction for visual interest
                if (ring % 2 == 1) ringSpeed = -ringSpeed;
                
                OrbitingRing(center, ringColor, particlesPerRing, ringRadius, ringSpeed,
                    scale * (1f - ringProgress * 0.2f), lifetime);
            }
        }
        
        #endregion
        
        #region Rainbow/Color Cycling Effects
        
        /// <summary>
        /// Spawns rainbow-cycling particles in a burst.
        /// Perfect for: Prismatic effects, celebration bursts, magical releases.
        /// </summary>
        public static void RainbowBurst(Vector2 center, int count, float speed, float scale, int lifetime)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                Vector2 vel = angle.ToRotationVector2() * speed * Main.rand.NextFloat(0.8f, 1.2f);
                
                var particle = new RainbowCyclingParticle(center, vel, scale, lifetime);
                MagnumParticleHandler.SpawnParticle(particle);
            }
        }
        
        /// <summary>
        /// Spawns hue-shifting particles within a specific color range.
        /// Perfect for: Theme-consistent rainbow effects, gradient trails.
        /// </summary>
        public static void HueRangeBurst(Vector2 center, float hueMin, float hueMax,
            float saturation, float luminosity, int count, float speed, float scale, int lifetime)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                Vector2 vel = angle.ToRotationVector2() * speed * Main.rand.NextFloat(0.8f, 1.2f);
                
                var particle = new RainbowCyclingParticle(center, vel, hueMin, hueMax,
                    saturation, luminosity, scale, lifetime, Main.rand.NextFloat(0.01f, 0.03f));
                MagnumParticleHandler.SpawnParticle(particle);
            }
        }
        
        /// <summary>
        /// Spawns hue-shifting music notes for themed musical effects.
        /// Perfect for: Musical weapon trails, bardic abilities, enchanted instruments.
        /// </summary>
        public static void HueShiftingMusicNotes(Vector2 position, Vector2 baseVelocity,
            float hueMin, float hueMax, int count, float scale, int lifetime)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(15f, 15f);
                Vector2 vel = baseVelocity + Main.rand.NextVector2Circular(1.5f, 1.5f) + new Vector2(0, -1f);
                
                var particle = new HueShiftingMusicNoteParticle(position + offset, vel,
                    hueMin, hueMax, 0.9f, 0.75f, scale * Main.rand.NextFloat(0.8f, 1.2f),
                    lifetime, Main.rand.NextFloat(0.015f, 0.025f));
                MagnumParticleHandler.SpawnParticle(particle);
            }
        }
        
        #endregion
        
        #region Phased/Dramatic Effects
        
        /// <summary>
        /// Spawns a dramatic flare with flash-bloom-fade animation.
        /// Perfect for: Impacts, explosions, critical hits, ability triggers.
        /// </summary>
        public static void DramaticImpact(Vector2 position, Color coreColor, Color glowColor,
            float scale, int lifetime, int flareVariant = -1)
        {
            var particle = new DramaticFlareParticle(position, Vector2.Zero, coreColor, glowColor,
                Color.White, scale * 0.4f, scale, scale * 1.8f, lifetime, flareVariant);
            MagnumParticleHandler.SpawnParticle(particle);
        }
        
        /// <summary>
        /// Spawns multiple dramatic flares in a staggered burst.
        /// Perfect for: Major impacts, boss hits, ultimate abilities.
        /// </summary>
        public static void DramaticBurst(Vector2 center, Color coreColor, Color glowColor,
            int count, float scale, int baseLifetime)
        {
            // Central flare
            DramaticImpact(center, coreColor, glowColor, scale * 1.5f, baseLifetime);
            
            // Surrounding flares with offset timing (via different lifetimes)
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                Vector2 offset = angle.ToRotationVector2() * (20f + Main.rand.NextFloat(15f));
                
                int lifetime = baseLifetime - Main.rand.Next(5, 15); // Staggered timing
                DramaticImpact(center + offset, coreColor, glowColor, 
                    scale * Main.rand.NextFloat(0.6f, 0.9f), lifetime, Main.rand.Next(1, 8));
            }
        }
        
        /// <summary>
        /// Spawns phased bloom particles with appear-hold-disappear animation.
        /// Perfect for: Summoning effects, reveals, persistent magical markers.
        /// </summary>
        public static void PhasedAppearance(Vector2 position, Vector2 velocity, Color color, Color peakColor,
            float scale, int lifetime, float appearDuration = 0.2f, float holdDuration = 0.5f)
        {
            var particle = new PhasedBloomParticle(position, velocity, color, peakColor,
                scale * 0.3f, scale, lifetime, appearDuration, holdDuration);
            MagnumParticleHandler.SpawnParticle(particle);
        }
        
        #endregion
        
        #region Trail/Streak Effects
        
        /// <summary>
        /// Spawns velocity-stretched streak particles.
        /// Perfect for: Speed lines, fast projectiles, dashing effects.
        /// </summary>
        public static void SpeedStreaks(Vector2 position, Vector2 baseVelocity, Color color,
            int count, float width, int lifetime, bool gravity = false)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 vel = baseVelocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(0.7f, 1.3f);
                Vector2 offset = Main.rand.NextVector2Circular(10f, 10f);
                
                var particle = new StreakParticle(position + offset, vel, color,
                    Color.Lerp(color, Color.White, 0.5f), width * Main.rand.NextFloat(0.8f, 1.2f),
                    1f, 4f, lifetime, gravity, 0.15f);
                MagnumParticleHandler.SpawnParticle(particle);
            }
        }
        
        /// <summary>
        /// Spawns comet particles with glowing head and fading tail.
        /// Perfect for: Shooting stars, magical missiles, celestial effects.
        /// </summary>
        public static void CometShower(Vector2 center, Vector2 direction, Color headColor, Color tailColor,
            int count, float speed, float spread, float scale, int lifetime, bool gravity = false)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 vel = direction.RotatedByRandom(spread) * speed * Main.rand.NextFloat(0.7f, 1.3f);
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                
                var particle = new CometParticle(center + offset, vel, headColor, tailColor,
                    scale * Main.rand.NextFloat(0.7f, 1.3f), Main.rand.Next(6, 12), lifetime, gravity);
                MagnumParticleHandler.SpawnParticle(particle);
            }
        }
        
        /// <summary>
        /// Spawns a single comet particle.
        /// Perfect for: Trail effects, magical projectile visuals.
        /// </summary>
        public static void Comet(Vector2 position, Vector2 velocity, Color headColor, Color tailColor,
            float scale, int tailSegments, int lifetime, bool gravity = false)
        {
            var particle = new CometParticle(position, velocity, headColor, tailColor,
                scale, tailSegments, lifetime, gravity);
            MagnumParticleHandler.SpawnParticle(particle);
        }
        
        #endregion
        
        #region Combined Preset Effects
        
        /// <summary>
        /// Creates a complete "magical impact" effect using multiple dynamic particle types.
        /// This combines dramatic flare, pulsing burst, orbiting ring, and sparkles.
        /// </summary>
        public static void MagicalImpact(Vector2 center, Color primary, Color secondary, float intensity = 1f)
        {
            // Central dramatic flare
            DramaticImpact(center, primary, secondary, 0.6f * intensity, (int)(35 * intensity));
            
            // Pulsing bloom burst
            PulsingBurst(center, primary, secondary, (int)(8 * intensity), 3f * intensity, 
                0.35f * intensity, (int)(40 * intensity));
            
            // Quick orbiting ring that expands
            int orbitCount = (int)(6 * intensity);
            for (int i = 0; i < orbitCount; i++)
            {
                float angle = MathHelper.TwoPi * i / orbitCount;
                var orbit = new SpiralParticle(center, 10f, angle, 0.1f, 2f * intensity,
                    primary, secondary * 0.3f, 0.25f * intensity, (int)(25 * intensity), true);
                MagnumParticleHandler.SpawnParticle(orbit);
            }
            
            // Twinkling sparkles
            TwinklingSparks(center, Color.Lerp(primary, Color.White, 0.3f), 
                (int)(10 * intensity), 30f * intensity, 0.3f * intensity, (int)(50 * intensity));
        }
        
        /// <summary>
        /// Creates a "celestial burst" effect with rainbow cycling and comet trails.
        /// Perfect for: Stellar impacts, cosmic abilities, divine effects.
        /// </summary>
        public static void CelestialBurst(Vector2 center, float intensity = 1f)
        {
            // Central rainbow explosion
            RainbowBurst(center, (int)(12 * intensity), 4f * intensity, 0.35f * intensity, (int)(40 * intensity));
            
            // Comet trails radiating outward
            for (int i = 0; i < (int)(6 * intensity); i++)
            {
                float angle = MathHelper.TwoPi * i / (6 * intensity);
                Vector2 vel = angle.ToRotationVector2() * 5f * intensity;
                Color headColor = Main.hslToRgb(i / (6f * intensity), 0.9f, 0.8f);
                Color tailColor = headColor * 0.3f;
                
                Comet(center, vel, headColor, tailColor, 0.3f * intensity, 6, (int)(35 * intensity));
            }
            
            // Spiral galaxy effect
            SpiralBurst(center, Color.White, new Color(200, 180, 255) * 0.5f,
                (int)(8 * intensity), 0.08f, 1.5f * intensity, 0.2f * intensity, (int)(45 * intensity));
        }
        
        /// <summary>
        /// Creates a "musical crescendo" effect with hue-shifting notes and pulsing blooms.
        /// Perfect for: Musical abilities, bardic spells, concert finales.
        /// </summary>
        public static void MusicalCrescendo(Vector2 center, float hueMin, float hueMax, float intensity = 1f)
        {
            // Hue-shifting music notes rising
            HueShiftingMusicNotes(center, new Vector2(0, -1.5f), hueMin, hueMax,
                (int)(8 * intensity), 0.8f * intensity, (int)(60 * intensity));
            
            // Pulsing blooms in hue range
            for (int i = 0; i < (int)(6 * intensity); i++)
            {
                float hue = MathHelper.Lerp(hueMin, hueMax, i / (6f * intensity));
                Color color = Main.hslToRgb(hue, 0.9f, 0.7f);
                Color secondary = Main.hslToRgb((hue + 0.1f) % 1f, 0.8f, 0.6f);
                
                float angle = MathHelper.TwoPi * i / (6 * intensity);
                Vector2 vel = angle.ToRotationVector2() * 2f * intensity;
                
                PulsingGlow(center, vel, color, secondary, 0.3f * intensity, (int)(45 * intensity));
            }
            
            // Orbiting sparkles
            ConcentricOrbits(center, Main.hslToRgb(hueMin, 0.9f, 0.8f), 
                Main.hslToRgb(hueMax, 0.9f, 0.8f), 2, (int)(4 * intensity), 
                20f * intensity, 15f * intensity, 0.06f, 0.2f * intensity, (int)(50 * intensity));
        }
        
        /// <summary>
        /// Creates a "vortex absorption" effect with inward spirals and converging streaks.
        /// Perfect for: Vacuum abilities, absorption effects, black hole visuals.
        /// </summary>
        public static void VortexAbsorption(Vector2 center, Color primary, Color secondary, float intensity = 1f)
        {
            // Inward spiraling particles
            SpiralVortex(center, primary, secondary * 0.5f, (int)(12 * intensity),
                60f * intensity, 0.1f, 1.5f * intensity, 0.25f * intensity, (int)(40 * intensity));
            
            // Converging streaks from edges
            for (int i = 0; i < (int)(8 * intensity); i++)
            {
                float angle = MathHelper.TwoPi * i / (8 * intensity);
                Vector2 startPos = center + angle.ToRotationVector2() * 80f * intensity;
                Vector2 vel = (center - startPos).SafeNormalize(Vector2.Zero) * 6f * intensity;
                
                var streak = new StreakParticle(startPos, vel, primary, secondary,
                    0.15f * intensity, 1f, 3f, (int)(25 * intensity));
                MagnumParticleHandler.SpawnParticle(streak);
            }
            
            // Central pulsing core
            PulsingGlow(center, Vector2.Zero, secondary, primary, 0.5f * intensity,
                (int)(50 * intensity), 0.2f, 0.4f);
        }
        
        #endregion
        
        #region Theme-Specific Presets
        
        /// <summary>
        /// Dies Irae themed impact - blood red with hellfire accents.
        /// </summary>
        public static void DiesIraeImpact(Vector2 center, float intensity = 1f)
        {
            Color blood = new Color(139, 0, 0);
            Color fire = new Color(255, 80, 0);
            Color gold = new Color(255, 200, 80);
            
            DramaticImpact(center, blood, fire, 0.7f * intensity, (int)(40 * intensity));
            PulsingBurst(center, fire, gold, (int)(6 * intensity), 3f * intensity, 
                0.3f * intensity, (int)(35 * intensity));
            SpeedStreaks(center, Vector2.Zero, blood, (int)(12 * intensity), 
                0.12f * intensity, (int)(25 * intensity));
            
            // Hellfire accents
            HueRangeBurst(center, 0.0f, 0.08f, 0.9f, 0.7f, (int)(8 * intensity),
                2f * intensity, 0.25f * intensity, (int)(30 * intensity));
        }
        
        /// <summary>
        /// Nachtmusik themed impact - celestial blues and golds.
        /// </summary>
        public static void NachtmusikImpact(Vector2 center, float intensity = 1f)
        {
            Color nightBlue = new Color(40, 60, 140);
            Color starGold = new Color(255, 220, 100);
            Color moonSilver = new Color(220, 230, 255);
            
            DramaticImpact(center, nightBlue, starGold, 0.7f * intensity, (int)(40 * intensity));
            CometShower(center, Vector2.UnitY * -1f, starGold, nightBlue * 0.4f,
                (int)(6 * intensity), 4f * intensity, 0.8f, 0.25f * intensity, (int)(35 * intensity));
            TwinklingSparks(center, moonSilver, (int)(12 * intensity), 40f * intensity,
                0.35f * intensity, (int)(50 * intensity));
            
            // Celestial hue range (blues to purples)
            HueRangeBurst(center, 0.6f, 0.75f, 0.8f, 0.75f, (int)(8 * intensity),
                2.5f * intensity, 0.2f * intensity, (int)(35 * intensity));
        }
        
        /// <summary>
        /// Spring themed impact - pinks and greens with floral feel.
        /// </summary>
        public static void SpringImpact(Vector2 center, float intensity = 1f)
        {
            Color blossom = new Color(255, 180, 200);
            Color leaf = new Color(120, 200, 120);
            Color petal = new Color(255, 220, 230);
            
            DramaticImpact(center, blossom, leaf, 0.6f * intensity, (int)(35 * intensity));
            PulsingBurst(center, petal, blossom, (int)(8 * intensity), 2.5f * intensity,
                0.3f * intensity, (int)(40 * intensity));
            SpiralBurst(center, blossom, leaf * 0.5f, (int)(6 * intensity),
                0.06f, 1f * intensity, 0.2f * intensity, (int)(35 * intensity));
        }
        
        /// <summary>
        /// Summer themed impact - golden oranges and bright yellows.
        /// </summary>
        public static void SummerImpact(Vector2 center, float intensity = 1f)
        {
            Color sunGold = new Color(255, 200, 50);
            Color sunsetOrange = new Color(255, 140, 50);
            Color brightYellow = new Color(255, 255, 150);
            
            DramaticImpact(center, sunGold, sunsetOrange, 0.7f * intensity, (int)(40 * intensity));
            HueRangeBurst(center, 0.08f, 0.15f, 0.95f, 0.8f, (int)(10 * intensity),
                3f * intensity, 0.3f * intensity, (int)(35 * intensity));
            SpeedStreaks(center, Vector2.UnitY * -2f, brightYellow, (int)(8 * intensity),
                0.15f * intensity, (int)(30 * intensity));
        }
        
        /// <summary>
        /// Autumn themed impact - warm oranges and browns.
        /// </summary>
        public static void AutumnImpact(Vector2 center, float intensity = 1f)
        {
            Color maple = new Color(200, 80, 40);
            Color amber = new Color(255, 180, 80);
            Color brown = new Color(140, 90, 50);
            
            DramaticImpact(center, maple, amber, 0.6f * intensity, (int)(38 * intensity));
            PulsingBurst(center, amber, brown, (int)(8 * intensity), 2f * intensity,
                0.28f * intensity, (int)(42 * intensity));
            SpiralBurst(center, maple, brown * 0.6f, (int)(6 * intensity),
                0.05f, 0.8f * intensity, 0.22f * intensity, (int)(38 * intensity));
        }
        
        /// <summary>
        /// Winter themed impact - icy blues and silvers.
        /// </summary>
        public static void WinterImpact(Vector2 center, float intensity = 1f)
        {
            Color ice = new Color(180, 220, 255);
            Color frost = new Color(220, 240, 255);
            Color snow = new Color(245, 250, 255);
            
            DramaticImpact(center, ice, frost, 0.65f * intensity, (int)(42 * intensity));
            TwinklingSparks(center, snow, (int)(15 * intensity), 45f * intensity,
                0.3f * intensity, (int)(55 * intensity));
            CometShower(center, Vector2.UnitY * 1.5f, frost, ice * 0.3f,
                (int)(6 * intensity), 2f * intensity, 1.2f, 0.2f * intensity, (int)(40 * intensity), true);
        }
        
        #endregion
    }
}
