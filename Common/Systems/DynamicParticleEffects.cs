using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
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
        
        #region Nachtmusik Unique Effects (3 Distinct Styles)
        
        // Nachtmusik Colors
        private static readonly Color NachtDeepPurple = new Color(45, 27, 78);
        private static readonly Color NachtViolet = new Color(123, 104, 238);
        private static readonly Color NachtGold = new Color(255, 215, 0);
        private static readonly Color NachtStarWhite = new Color(255, 255, 255);
        private static readonly Color NachtMidnight = new Color(20, 15, 45);
        private static readonly float NachtHueMin = 0.72f;  // Purple
        private static readonly float NachtHueMax = 0.85f;  // Violet
        
        /// <summary>
        /// NACHTMUSIK STYLE 1: Constellation Cascade
        /// Creates a shower of star points that form constellation-like patterns.
        /// Perfect for: Starweaver weapons, cosmic projectiles, celestial abilities.
        /// </summary>
        public static void NachtConstellationCascade(Vector2 center, float intensity = 1f)
        {
            // Central bright star flash
            DramaticImpact(center, NachtStarWhite, NachtGold, 0.8f * intensity, (int)(30 * intensity));
            
            // Constellation star points - form a geometric pattern
            int starCount = (int)(8 * intensity);
            for (int i = 0; i < starCount; i++)
            {
                float angle = MathHelper.TwoPi * i / starCount;
                float radius = 30f + Main.rand.NextFloat(20f);
                Vector2 starPos = center + angle.ToRotationVector2() * radius * intensity;
                
                // Main star
                var star = new TwinklingSparkleParticle(starPos, Vector2.Zero, NachtStarWhite, NachtGold,
                    0.2f * intensity, 0.5f * intensity, (int)(45 * intensity), 0.18f, 1.5f);
                MagnumParticleHandler.SpawnParticle(star);
                
                // Connecting line particle (constellation thread)
                Vector2 toCenter = (center - starPos).SafeNormalize(Vector2.Zero);
                var thread = new StreakParticle(starPos, toCenter * 1.5f * intensity, NachtViolet * 0.6f, NachtGold * 0.3f,
                    0.08f * intensity, 1f, 2f, (int)(25 * intensity));
                MagnumParticleHandler.SpawnParticle(thread);
            }
            
            // Inner purple glow spiral
            SpiralBurst(center, NachtDeepPurple, NachtViolet * 0.5f, (int)(6 * intensity),
                0.07f, 1.2f * intensity, 0.2f * intensity, (int)(35 * intensity));
        }
        
        /// <summary>
        /// NACHTMUSIK STYLE 2: Nocturnal Crescent Wave
        /// Creates sweeping crescent-shaped energy waves with starlight trails.
        /// Perfect for: Blade weapons, slashing attacks, wave projectiles.
        /// </summary>
        public static void NachtCrescentWave(Vector2 center, Vector2 direction, float intensity = 1f)
        {
            // Crescent flash - brighter on one side
            Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
            
            // Core crescent flash
            DramaticImpact(center, NachtGold, NachtViolet, 0.6f * intensity, (int)(25 * intensity));
            
            // Trailing crescent particles - sweep in an arc
            int sweepCount = (int)(12 * intensity);
            for (int i = 0; i < sweepCount; i++)
            {
                float arcAngle = MathHelper.Lerp(-MathHelper.PiOver2, MathHelper.PiOver2, (float)i / sweepCount);
                Vector2 arcDir = direction.RotatedBy(arcAngle);
                Vector2 particlePos = center + arcDir * (15f + i * 2f) * intensity;
                Vector2 particleVel = arcDir * (2f + Main.rand.NextFloat(2f)) * intensity;
                
                Color arcColor = Color.Lerp(NachtGold, NachtViolet, Math.Abs(arcAngle) / MathHelper.PiOver2);
                PulsingGlow(particlePos, particleVel, arcColor, NachtDeepPurple,
                    0.25f * intensity * (1f - Math.Abs(arcAngle) / MathHelper.PiOver2 * 0.5f),
                    (int)(30 * intensity), 0.2f, 0.35f);
            }
            
            // Star dust trailing
            CometShower(center, direction, NachtStarWhite, NachtViolet * 0.4f,
                (int)(4 * intensity), 3f * intensity, 0.5f, 0.18f * intensity, (int)(28 * intensity));
        }
        
        /// <summary>
        /// NACHTMUSIK STYLE 3: Serenade Resonance
        /// Creates musical note patterns with orbiting star particles.
        /// Perfect for: Magic weapons, musical abilities, sustained effects.
        /// </summary>
        public static void NachtSerenadeResonance(Vector2 center, float intensity = 1f)
        {
            // Central violet bloom with pulsing gold core
            PulsingGlow(center, Vector2.Zero, NachtViolet, NachtGold, 0.5f * intensity,
                (int)(50 * intensity), 0.12f, 0.4f);
            
            // Musical notes hue-shifting in the night sky range (purple to gold)
            HueShiftingMusicNotes(center, new Vector2(0, -1.2f * intensity), 0.72f, 0.82f,
                (int)(6 * intensity), 0.75f * intensity, (int)(55 * intensity));
            
            // Triple concentric orbiting stars
            ConcentricOrbits(center, NachtStarWhite, NachtViolet, 3, (int)(3 * intensity),
                15f * intensity, 12f * intensity, 0.05f, 0.18f * intensity, (int)(60 * intensity));
            
            // Scattered twilight sparkles
            TwinklingSparks(center, Color.Lerp(NachtViolet, NachtGold, 0.3f),
                (int)(12 * intensity), 50f * intensity, 0.25f * intensity, (int)(55 * intensity));
            
            // Deep purple ambient glow
            for (int i = 0; i < (int)(4 * intensity); i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(25f, 25f) * intensity;
                var glow = new PhasedBloomParticle(center + offset, Main.rand.NextVector2Circular(0.5f, 0.5f),
                    NachtDeepPurple * 0.6f, NachtMidnight, 0.3f * intensity, 0.5f * intensity, (int)(40 * intensity), 0.3f, 0.4f);
                MagnumParticleHandler.SpawnParticle(glow);
            }
        }
        
        #endregion
        
        #region Dies Irae Unique Effects (3 Distinct Styles)
        
        // Dies Irae Colors
        private static readonly Color DiesBlack = new Color(26, 26, 26);
        private static readonly Color DiesBloodRed = new Color(139, 0, 0);
        private static readonly Color DiesBrightFlame = new Color(255, 36, 0);
        private static readonly Color DiesCrimson = new Color(220, 20, 60);
        private static readonly Color DiesEmber = new Color(255, 100, 20);
        private static readonly Color DiesCharcoal = new Color(50, 40, 40);
        
        /// <summary>
        /// DIES IRAE STYLE 1: Hellfire Eruption
        /// Creates volcanic eruption of flames with dark ember trails.
        /// Perfect for: Heavy melee weapons, slam attacks, explosive impacts.
        /// </summary>
        public static void DiesHellfireEruption(Vector2 center, float intensity = 1f)
        {
            // Central black-core explosion with crimson outline
            DramaticImpact(center, DiesBlack, DiesCrimson, 0.9f * intensity, (int)(35 * intensity), 3);
            DramaticImpact(center, DiesBrightFlame, DiesEmber, 0.7f * intensity, (int)(30 * intensity), 5);
            
            // Upward erupting flame pillars
            int pillarCount = (int)(6 * intensity);
            for (int i = 0; i < pillarCount; i++)
            {
                float spreadAngle = MathHelper.Lerp(-0.6f, 0.6f, (float)i / pillarCount);
                Vector2 pillarDir = new Vector2((float)Math.Sin(spreadAngle), -1f).SafeNormalize(Vector2.UnitY * -1f);
                
                // Flame pillar - multiple comets
                for (int j = 0; j < 3; j++)
                {
                    float speedMod = 1f + j * 0.4f;
                    Color flameColor = Color.Lerp(DiesBrightFlame, DiesEmber, j / 3f);
                    Comet(center, pillarDir * (4f + Main.rand.NextFloat(3f)) * intensity * speedMod,
                        flameColor, DiesBloodRed * 0.5f, 0.3f * intensity, 5 + j * 2, (int)(35 * intensity));
                }
            }
            
            // Ground-level ember scatter
            SpeedStreaks(center, Vector2.UnitX, DiesEmber, (int)(10 * intensity),
                0.12f * intensity, (int)(25 * intensity));
            SpeedStreaks(center, Vector2.UnitX * -1f, DiesEmber, (int)(10 * intensity),
                0.12f * intensity, (int)(25 * intensity));
            
            // Dark smoke pulsing at base
            PulsingBurst(center, DiesCharcoal, DiesBlack, (int)(8 * intensity),
                1.5f * intensity, 0.4f * intensity, (int)(45 * intensity));
        }
        
        /// <summary>
        /// DIES IRAE STYLE 2: Wrath Chain Lightning
        /// Creates branching crimson lightning with blood-red afterglow.
        /// Perfect for: Ranged weapons, chain attacks, lightning-based abilities.
        /// </summary>
        public static void DiesWrathChainLightning(Vector2 center, Vector2 direction, float intensity = 1f)
        {
            // Central wrath flash
            DramaticImpact(center, DiesCrimson, DiesBrightFlame, 0.6f * intensity, (int)(22 * intensity), 2);
            
            // Main lightning branch
            Vector2 mainDir = direction.SafeNormalize(Vector2.UnitX);
            int segments = (int)(5 * intensity);
            Vector2 currentPos = center;
            
            for (int i = 0; i < segments; i++)
            {
                float zigzag = (i % 2 == 0 ? 1f : -1f) * Main.rand.NextFloat(10f, 20f);
                Vector2 perpOffset = mainDir.RotatedBy(MathHelper.PiOver2) * zigzag;
                Vector2 nextPos = currentPos + mainDir * (20f + Main.rand.NextFloat(15f)) * intensity + perpOffset;
                
                // Lightning segment as streak
                Vector2 segmentVel = (nextPos - currentPos).SafeNormalize(Vector2.Zero) * 8f * intensity;
                var lightning = new StreakParticle(currentPos, segmentVel, DiesCrimson, DiesBrightFlame,
                    0.15f * intensity, 1f, 4f, (int)(18 * intensity));
                MagnumParticleHandler.SpawnParticle(lightning);
                
                // Branch point spark
                if (Main.rand.NextBool(2))
                {
                    DramaticImpact(nextPos, DiesBrightFlame, DiesBloodRed, 0.25f * intensity, (int)(15 * intensity), 6);
                }
                
                currentPos = nextPos;
            }
            
            // Afterglow trail
            HueRangeBurst(center, 0.0f, 0.05f, 0.95f, 0.6f, (int)(6 * intensity),
                2f * intensity, 0.2f * intensity, (int)(28 * intensity));
        }
        
        /// <summary>
        /// DIES IRAE STYLE 3: Judgment Inferno Vortex
        /// Creates spiraling hellfire vortex with converging dark energy.
        /// Perfect for: Magic weapons, summoning effects, channeled abilities.
        /// </summary>
        public static void DiesJudgmentVortex(Vector2 center, float intensity = 1f)
        {
            // Inward spiraling flames - creates vortex effect
            SpiralVortex(center, DiesBrightFlame, DiesBloodRed * 0.6f, (int)(10 * intensity),
                55f * intensity, 0.12f, 1.8f * intensity, 0.28f * intensity, (int)(38 * intensity));
            
            // Counter-spiral of dark energy
            SpiralVortex(center, DiesCharcoal, DiesBlack * 0.5f, (int)(8 * intensity),
                65f * intensity, -0.08f, 1.5f * intensity, 0.22f * intensity, (int)(42 * intensity));
            
            // Central pulsing crimson core
            PulsingGlow(center, Vector2.Zero, DiesCrimson, DiesBlack, 0.6f * intensity,
                (int)(55 * intensity), 0.15f, 0.5f);
            
            // Orbiting ember sparks
            OrbitingRing(center, DiesEmber, (int)(6 * intensity), 25f * intensity,
                0.08f, 0.15f * intensity, (int)(50 * intensity));
            
            // Musical judgment notes (darker hue range - reds/oranges)
            HueShiftingMusicNotes(center, new Vector2(0, -0.8f * intensity), 0.0f, 0.08f,
                (int)(4 * intensity), 0.65f * intensity, (int)(48 * intensity));
        }
        
        #endregion
        
        #region Seasonal Unique Effects (3 Distinct Styles Each)
        
        // === SPRING EFFECTS ===
        
        private static readonly Color SpringBlossom = new Color(255, 183, 197);
        private static readonly Color SpringLeaf = new Color(144, 238, 144);
        private static readonly Color SpringPetal = new Color(255, 220, 230);
        private static readonly Color SpringSunlight = new Color(255, 255, 200);
        
        /// <summary>
        /// SPRING STYLE 1: Petal Bloom Burst
        /// Creates expanding flower bloom with swirling petals.
        /// </summary>
        public static void SpringPetalBloom(Vector2 center, float intensity = 1f)
        {
            DramaticImpact(center, SpringPetal, SpringBlossom, 0.65f * intensity, (int)(32 * intensity));
            
            // Petal spiral outward
            int petalCount = (int)(12 * intensity);
            for (int i = 0; i < petalCount; i++)
            {
                float angle = MathHelper.TwoPi * i / petalCount + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 petalVel = angle.ToRotationVector2() * (2f + Main.rand.NextFloat(2f)) * intensity;
                
                Color petalColor = Color.Lerp(SpringBlossom, SpringPetal, Main.rand.NextFloat());
                var petal = new SpiralParticle(center, 5f, angle, 0.04f, 1.5f * intensity,
                    petalColor, SpringLeaf * 0.4f, 0.25f * intensity, (int)(38 * intensity), true);
                MagnumParticleHandler.SpawnParticle(petal);
            }
            
            TwinklingSparks(center, SpringSunlight, (int)(10 * intensity), 35f * intensity,
                0.22f * intensity, (int)(45 * intensity));
        }
        
        /// <summary>
        /// SPRING STYLE 2: Verdant Growth Trail
        /// Creates sprouting vine-like energy trails with leaf accents.
        /// </summary>
        public static void SpringVerdantTrail(Vector2 center, Vector2 direction, float intensity = 1f)
        {
            // Main vine-like comet trail
            CometShower(center, direction, SpringLeaf, SpringBlossom * 0.5f,
                (int)(5 * intensity), 3.5f * intensity, 0.4f, 0.22f * intensity, (int)(32 * intensity));
            
            // Branching leaf sparkles
            Vector2 perpDir = direction.RotatedBy(MathHelper.PiOver2);
            for (int branch = -1; branch <= 1; branch += 2)
            {
                Vector2 branchDir = (direction + perpDir * branch * 0.4f).SafeNormalize(direction);
                SpeedStreaks(center, branchDir * 2f, SpringLeaf * 0.8f, (int)(4 * intensity),
                    0.1f * intensity, (int)(25 * intensity));
            }
            
            PulsingGlow(center, direction * 0.5f, SpringBlossom, SpringLeaf, 0.3f * intensity,
                (int)(28 * intensity), 0.18f, 0.3f);
        }
        
        /// <summary>
        /// SPRING STYLE 3: Sunshower Radiance
        /// Creates rain-of-light effect with floral sparkles.
        /// </summary>
        public static void SpringSunshower(Vector2 center, float intensity = 1f)
        {
            // Sunlight rays descending
            int rayCount = (int)(8 * intensity);
            for (int i = 0; i < rayCount; i++)
            {
                Vector2 rayStart = center + new Vector2(Main.rand.NextFloat(-40f, 40f), -30f) * intensity;
                Vector2 rayVel = new Vector2(0, 2.5f + Main.rand.NextFloat(1.5f)) * intensity;
                
                Comet(rayStart, rayVel, SpringSunlight, SpringBlossom * 0.4f,
                    0.2f * intensity, 4, (int)(30 * intensity), true);
            }
            
            // Ground-level bloom sparkles
            TwinklingSparks(center, SpringPetal, (int)(14 * intensity), 45f * intensity,
                0.28f * intensity, (int)(50 * intensity));
            
            // Central warm glow
            PulsingGlow(center, Vector2.Zero, SpringSunlight * 0.7f, SpringBlossom, 0.4f * intensity,
                (int)(45 * intensity), 0.1f, 0.25f);
        }
        
        // === SUMMER EFFECTS ===
        
        private static readonly Color SummerSunGold = new Color(255, 200, 50);
        private static readonly Color SummerSunset = new Color(255, 140, 50);
        private static readonly Color SummerBright = new Color(255, 255, 150);
        private static readonly Color SummerHeat = new Color(255, 180, 80);
        
        /// <summary>
        /// SUMMER STYLE 1: Solar Flare Burst
        /// Creates explosive sun-like burst with heat shimmer.
        /// </summary>
        public static void SummerSolarFlare(Vector2 center, float intensity = 1f)
        {
            // Bright central flash
            DramaticImpact(center, SummerBright, SummerSunGold, 0.85f * intensity, (int)(35 * intensity), 1);
            DramaticImpact(center, SummerSunGold, SummerSunset, 0.65f * intensity, (int)(30 * intensity), 4);
            
            // Solar flare tendrils
            int flareCount = (int)(6 * intensity);
            for (int i = 0; i < flareCount; i++)
            {
                float angle = MathHelper.TwoPi * i / flareCount + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 flareDir = angle.ToRotationVector2();
                
                // Extended solar flare
                for (int j = 0; j < 4; j++)
                {
                    Vector2 flareVel = flareDir * (3f + j * 1.5f) * intensity;
                    Color flareColor = Color.Lerp(SummerBright, SummerSunset, j / 4f);
                    PulsingGlow(center + flareDir * j * 8f, flareVel, flareColor, SummerHeat,
                        (0.3f - j * 0.05f) * intensity, (int)((35 - j * 4) * intensity), 0.2f, 0.35f);
                }
            }
            
            HueRangeBurst(center, 0.08f, 0.14f, 0.95f, 0.85f, (int)(10 * intensity),
                2.5f * intensity, 0.22f * intensity, (int)(32 * intensity));
        }
        
        /// <summary>
        /// SUMMER STYLE 2: Heat Wave Mirage
        /// Creates shimmering heat wave effect with rising thermals.
        /// </summary>
        public static void SummerHeatWave(Vector2 center, float intensity = 1f)
        {
            // Rising heat thermals
            int thermalCount = (int)(10 * intensity);
            for (int i = 0; i < thermalCount; i++)
            {
                Vector2 thermalPos = center + new Vector2(Main.rand.NextFloat(-35f, 35f), Main.rand.NextFloat(-10f, 10f)) * intensity;
                Vector2 thermalVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1.5f - Main.rand.NextFloat(1f)) * intensity;
                
                Color thermalColor = Color.Lerp(SummerHeat, SummerBright, Main.rand.NextFloat()) * 0.6f;
                var thermal = new PhasedBloomParticle(thermalPos, thermalVel, thermalColor * 0.3f, thermalColor,
                    0.15f * intensity, 0.35f * intensity, (int)(50 * intensity), 0.2f, 0.5f);
                MagnumParticleHandler.SpawnParticle(thermal);
            }
            
            // Shimmering sparkles
            TwinklingSparks(center, SummerSunGold, (int)(8 * intensity), 40f * intensity,
                0.2f * intensity, (int)(40 * intensity));
        }
        
        /// <summary>
        /// SUMMER STYLE 3: Golden Zenith Strike
        /// Creates powerful descending sun strike with radial burst.
        /// </summary>
        public static void SummerZenithStrike(Vector2 center, float intensity = 1f)
        {
            // Descending golden beam
            CometShower(center + new Vector2(0, -50f * intensity), Vector2.UnitY, SummerBright, SummerSunGold * 0.5f,
                (int)(4 * intensity), 5f * intensity, 0.2f, 0.35f * intensity, (int)(25 * intensity));
            
            // Ground impact flash
            DramaticBurst(center, SummerSunGold, SummerSunset, (int)(6 * intensity), 0.5f * intensity, (int)(30 * intensity));
            
            // Radial heat wave
            SpiralBurst(center, SummerHeat, SummerSunset * 0.5f, (int)(8 * intensity),
                0.05f, 2f * intensity, 0.25f * intensity, (int)(35 * intensity));
        }
        
        // === AUTUMN EFFECTS ===
        
        private static readonly Color AutumnMaple = new Color(200, 80, 40);
        private static readonly Color AutumnAmber = new Color(255, 180, 80);
        private static readonly Color AutumnBrown = new Color(140, 90, 50);
        private static readonly Color AutumnRust = new Color(180, 100, 50);
        
        /// <summary>
        /// AUTUMN STYLE 1: Falling Leaf Cascade
        /// Creates swirling leaf-fall effect with warm color gradient.
        /// </summary>
        public static void AutumnLeafCascade(Vector2 center, float intensity = 1f)
        {
            DramaticImpact(center, AutumnMaple, AutumnAmber, 0.55f * intensity, (int)(30 * intensity));
            
            // Falling leaf spirals
            int leafCount = (int)(14 * intensity);
            for (int i = 0; i < leafCount; i++)
            {
                float startAngle = MathHelper.TwoPi * i / leafCount;
                float xDrift = (float)Math.Sin(startAngle) * 25f * intensity;
                Vector2 leafStart = center + new Vector2(xDrift, -20f * intensity);
                
                // Gentle falling motion
                Vector2 leafVel = new Vector2(Main.rand.NextFloat(-1f, 1f), 1.5f + Main.rand.NextFloat(1f)) * intensity;
                Color leafColor = Color.Lerp(AutumnMaple, AutumnAmber, Main.rand.NextFloat());
                
                var leaf = new SpiralParticle(leafStart, 10f, startAngle, 0.03f, 0.5f * intensity,
                    leafColor, AutumnBrown * 0.5f, 0.2f * intensity, (int)(45 * intensity), true);
                MagnumParticleHandler.SpawnParticle(leaf);
            }
            
            TwinklingSparks(center, AutumnAmber * 0.8f, (int)(8 * intensity), 35f * intensity,
                0.18f * intensity, (int)(40 * intensity));
        }
        
        /// <summary>
        /// AUTUMN STYLE 2: Harvest Moon Glow
        /// Creates warm, golden-orange ambient glow with drifting particles.
        /// </summary>
        public static void AutumnHarvestMoon(Vector2 center, float intensity = 1f)
        {
            // Central moon-like glow
            PulsingGlow(center, Vector2.Zero, AutumnAmber, AutumnMaple, 0.55f * intensity,
                (int)(55 * intensity), 0.08f, 0.3f);
            
            // Orbiting harvest particles
            ConcentricOrbits(center, AutumnMaple, AutumnAmber, 2, (int)(4 * intensity),
                20f * intensity, 15f * intensity, 0.04f, 0.18f * intensity, (int)(50 * intensity));
            
            // Drifting ember-like particles
            int driftCount = (int)(8 * intensity);
            for (int i = 0; i < driftCount; i++)
            {
                Vector2 driftStart = center + Main.rand.NextVector2Circular(40f, 40f) * intensity;
                Vector2 driftVel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -0.5f - Main.rand.NextFloat(0.3f)) * intensity;
                
                var drift = new CometParticle(driftStart, driftVel, AutumnRust, AutumnBrown * 0.4f,
                    0.15f * intensity, 3, (int)(40 * intensity), false);
                MagnumParticleHandler.SpawnParticle(drift);
            }
        }
        
        /// <summary>
        /// AUTUMN STYLE 3: Decay Spiral
        /// Creates withering spiral effect representing seasonal change.
        /// </summary>
        public static void AutumnDecaySpiral(Vector2 center, float intensity = 1f)
        {
            // Inward spiral of fading colors
            SpiralVortex(center, AutumnMaple, AutumnBrown * 0.6f, (int)(10 * intensity),
                50f * intensity, 0.06f, 1.2f * intensity, 0.22f * intensity, (int)(40 * intensity));
            
            // Central fading glow
            DramaticImpact(center, AutumnAmber, AutumnBrown, 0.5f * intensity, (int)(35 * intensity));
            
            // Scattered rust particles
            PulsingBurst(center, AutumnRust, AutumnBrown, (int)(6 * intensity),
                1.8f * intensity, 0.25f * intensity, (int)(38 * intensity));
        }
        
        // === WINTER EFFECTS ===
        
        private static readonly Color WinterIce = new Color(180, 220, 255);
        private static readonly Color WinterFrost = new Color(220, 240, 255);
        private static readonly Color WinterSnow = new Color(245, 250, 255);
        private static readonly Color WinterDeepBlue = new Color(100, 150, 220);
        
        /// <summary>
        /// WINTER STYLE 1: Crystalline Shatter
        /// Creates shattering ice crystal effect with sharp sparkles.
        /// </summary>
        public static void WinterCrystallineShatter(Vector2 center, float intensity = 1f)
        {
            // Central ice flash
            DramaticImpact(center, WinterSnow, WinterIce, 0.75f * intensity, (int)(30 * intensity), 2);
            
            // Shattering crystal shards - radial speed streaks
            int shardCount = (int)(10 * intensity);
            for (int i = 0; i < shardCount; i++)
            {
                float angle = MathHelper.TwoPi * i / shardCount + Main.rand.NextFloat(-0.15f, 0.15f);
                Vector2 shardDir = angle.ToRotationVector2();
                
                SpeedStreaks(center, shardDir * (4f + Main.rand.NextFloat(3f)), WinterIce,
                    1, 0.12f * intensity, (int)(22 * intensity));
                
                // Crystal sparkle at tip
                Vector2 sparklePos = center + shardDir * (25f + Main.rand.NextFloat(15f)) * intensity;
                var sparkle = new TwinklingSparkleParticle(sparklePos, shardDir * 0.5f, WinterSnow, WinterFrost,
                    0.15f * intensity, 0.35f * intensity, (int)(35 * intensity), 0.2f, 1.8f);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }
        
        /// <summary>
        /// WINTER STYLE 2: Blizzard Veil
        /// Creates swirling snowstorm effect with frost particles.
        /// </summary>
        public static void WinterBlizzardVeil(Vector2 center, float intensity = 1f)
        {
            // Swirling snow particles
            SpiralBurst(center, WinterSnow * 0.8f, WinterFrost * 0.5f, (int)(12 * intensity),
                0.1f, 2f * intensity, 0.18f * intensity, (int)(45 * intensity));
            
            // Counter-swirl for blizzard effect
            SpiralBurst(center, WinterIce * 0.7f, WinterDeepBlue * 0.4f, (int)(10 * intensity),
                -0.08f, 1.8f * intensity, 0.15f * intensity, (int)(42 * intensity));
            
            // Falling snowflake sparkles
            CometShower(center, new Vector2(0.3f, 1f), WinterSnow, WinterFrost * 0.3f,
                (int)(8 * intensity), 2f * intensity, 0.8f, 0.15f * intensity, (int)(38 * intensity), true);
            
            // Central frost glow
            PulsingGlow(center, Vector2.Zero, WinterFrost * 0.6f, WinterDeepBlue * 0.4f, 0.4f * intensity,
                (int)(50 * intensity), 0.1f, 0.2f);
        }
        
        /// <summary>
        /// WINTER STYLE 3: Frozen Aurora
        /// Creates northern lights-like effect with color shimmer.
        /// </summary>
        public static void WinterFrozenAurora(Vector2 center, float intensity = 1f)
        {
            // Aurora ribbons - vertical shimmer
            int ribbonCount = (int)(6 * intensity);
            for (int i = 0; i < ribbonCount; i++)
            {
                float xOffset = MathHelper.Lerp(-35f, 35f, (float)i / ribbonCount) * intensity;
                Vector2 ribbonStart = center + new Vector2(xOffset, 30f * intensity);
                Vector2 ribbonVel = new Vector2(Main.rand.NextFloat(-0.2f, 0.2f), -1.8f - Main.rand.NextFloat(0.5f)) * intensity;
                
                // Aurora uses winter hue range with slight green tint
                float auroraHue = 0.45f + Main.rand.NextFloat(0.15f); // Cyan to green
                Color auroraColor = Main.hslToRgb(auroraHue, 0.6f, 0.7f);
                
                var aurora = new CometParticle(ribbonStart, ribbonVel, auroraColor, WinterIce * 0.3f,
                    0.25f * intensity, 6, (int)(45 * intensity), false);
                MagnumParticleHandler.SpawnParticle(aurora);
            }
            
            // Twinkling stars
            TwinklingSparks(center, WinterSnow, (int)(10 * intensity), 50f * intensity,
                0.25f * intensity, (int)(55 * intensity));
            
            // Ambient frost glow
            PulsingGlow(center, Vector2.Zero, WinterIce * 0.5f, WinterDeepBlue * 0.3f, 0.35f * intensity,
                (int)(50 * intensity), 0.12f, 0.25f);
        }
        
        #endregion
        
        #region Unique OnKill/Death Effects Per Theme
        
        // ====================================================================
        // NACHTMUSIK UNIQUE DEATH EFFECTS (6 Distinct Styles)
        // Each projectile type should use ONE of these instead of cookie-cutter
        // ====================================================================
        
        /// <summary>
        /// NACHTMUSIK DEATH STYLE 1: Constellation Implosion
        /// Stars converge to center then explode outward as connected constellation.
        /// USE FOR: NocturnalBladeProjectile (glyph-based attacks)
        /// </summary>
        public static void NachtDeathConstellation(Vector2 center, float intensity = 1f)
        {
            // Inner implosion flash
            CustomParticles.GenericFlare(center, Color.White, 0.8f * intensity, 15);
            CustomParticles.GenericFlare(center, NachtViolet, 0.6f * intensity, 18);
            
            // Constellation star points at cardinal + intercardinal directions
            int starPoints = 8;
            Vector2[] starPositions = new Vector2[starPoints];
            for (int i = 0; i < starPoints; i++)
            {
                float angle = MathHelper.TwoPi * i / starPoints;
                float radius = (25f + Main.rand.NextFloat(15f)) * intensity;
                starPositions[i] = center + angle.ToRotationVector2() * radius;
                
                // Star flare at each point
                float hue = NachtHueMin + (i / (float)starPoints) * (NachtHueMax - NachtHueMin);
                Color starColor = Main.hslToRgb(hue, 0.9f, 0.8f);
                CustomParticles.GenericFlare(starPositions[i], starColor, 0.35f * intensity, 22);
                
                // Twinkling sparkle
                var sparkle = new SparkleParticle(starPositions[i], Main.rand.NextVector2Circular(1f, 1f), 
                    starColor, 0.28f * intensity, 28);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Connecting threads between adjacent stars (constellation lines)
            for (int i = 0; i < starPoints; i++)
            {
                int next = (i + 1) % starPoints;
                Vector2 start = starPositions[i];
                Vector2 end = starPositions[next];
                
                for (int p = 0; p < 4; p++)
                {
                    Vector2 linePos = Vector2.Lerp(start, end, p / 4f);
                    float lineHue = NachtHueMin + ((i + p * 0.25f) / starPoints) * (NachtHueMax - NachtHueMin);
                    Color lineColor = Main.hslToRgb(lineHue, 0.85f, 0.7f);
                    var linePart = new GenericGlowParticle(linePos, Vector2.Zero, lineColor * 0.6f, 
                        0.12f * intensity, 20, true);
                    MagnumParticleHandler.SpawnParticle(linePart);
                }
            }
            
            // Central music note trio
            for (int n = 0; n < 3; n++)
            {
                float noteAngle = MathHelper.TwoPi * n / 3f + Main.rand.NextFloat(0.3f);
                Vector2 noteVel = noteAngle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f) * intensity;
                ThemedParticles.MusicNote(center, noteVel, NachtGold, 0.75f * intensity, 35);
            }
            
            Lighting.AddLight(center, NachtGold.ToVector3() * 1.2f * intensity);
        }
        
        /// <summary>
        /// NACHTMUSIK DEATH STYLE 2: Crescent Shatter
        /// Crescent moon shape shatters into shards that fly outward.
        /// USE FOR: CrescendoWaveProjectile (wave-based attacks)
        /// </summary>
        public static void NachtDeathCrescentShatter(Vector2 center, Vector2 direction, float intensity = 1f)
        {
            // Central crescent flash
            CustomParticles.GenericFlare(center, NachtViolet, 0.7f * intensity, 16);
            
            // Shatter shards flying in arc pattern
            int shardCount = (int)(10 * intensity);
            float baseAngle = direction.ToRotation();
            
            for (int i = 0; i < shardCount; i++)
            {
                // Arc from -60 to +60 degrees relative to direction
                float arcProgress = (float)i / (shardCount - 1);
                float shardAngle = baseAngle + MathHelper.ToRadians(-60f + 120f * arcProgress);
                float speed = Main.rand.NextFloat(4f, 8f) * intensity;
                Vector2 shardVel = shardAngle.ToRotationVector2() * speed;
                
                float shardHue = NachtHueMin + arcProgress * (NachtHueMax - NachtHueMin);
                Color shardColor = Main.hslToRgb(shardHue, 0.88f, 0.75f);
                
                // Streak particle for shard
                var shard = new StreakParticle(center, shardVel, shardColor, NachtGold * 0.5f,
                    0.1f * intensity, 0.8f, 3f, (int)(25 * intensity));
                MagnumParticleHandler.SpawnParticle(shard);
                
                // Trailing glow
                var trail = new GenericGlowParticle(center, shardVel * 0.6f, shardColor * 0.5f, 
                    0.2f * intensity, 22, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // Crescent-shaped halo (two offset arcs)
            CustomParticles.HaloRing(center + direction * 8f * intensity, NachtViolet, 0.35f * intensity, 18);
            CustomParticles.HaloRing(center - direction * 8f * intensity, NachtGold, 0.3f * intensity, 20);
            
            // Music notes following crescent curve
            for (int n = 0; n < 4; n++)
            {
                float noteArc = baseAngle + MathHelper.ToRadians(-45f + 30f * n);
                Vector2 noteVel = noteArc.ToRotationVector2() * Main.rand.NextFloat(2f, 4f) * intensity;
                float noteHue = NachtHueMin + (n / 4f) * (NachtHueMax - NachtHueMin);
                ThemedParticles.MusicNote(center, noteVel, Main.hslToRgb(noteHue, 0.9f, 0.75f), 0.7f * intensity, 32);
            }
            
            Lighting.AddLight(center, NachtViolet.ToVector3() * 1.1f * intensity);
        }
        
        /// <summary>
        /// NACHTMUSIK DEATH STYLE 3: Nebula Bloom
        /// Soft nebula cloud expands with embedded stars.
        /// USE FOR: NebulaArrowProjectile, NebulaStarfallProjectile (ranged)
        /// </summary>
        public static void NachtDeathNebulaBoom(Vector2 center, float intensity = 1f)
        {
            // Soft nebula expansion (multiple layers)
            for (int layer = 0; layer < 3; layer++)
            {
                float layerScale = (0.5f + layer * 0.25f) * intensity;
                float layerHue = NachtHueMin + (layer / 3f) * (NachtHueMax - NachtHueMin);
                Color nebulaColor = Main.hslToRgb(layerHue, 0.6f, 0.55f);
                
                // Soft expanding cloud particles
                int cloudCount = (int)(6 * intensity);
                for (int c = 0; c < cloudCount; c++)
                {
                    float angle = MathHelper.TwoPi * c / cloudCount + layer * 0.2f;
                    Vector2 cloudVel = angle.ToRotationVector2() * (1.5f + layer * 0.5f) * intensity;
                    
                    var cloud = new GenericGlowParticle(center, cloudVel, nebulaColor * 0.4f, 
                        layerScale * 0.4f, (int)(35 + layer * 5), true);
                    MagnumParticleHandler.SpawnParticle(cloud);
                }
            }
            
            // Embedded bright stars
            int starCount = (int)(8 * intensity);
            for (int s = 0; s < starCount; s++)
            {
                Vector2 starOffset = Main.rand.NextVector2Circular(25f, 25f) * intensity;
                float starHue = NachtHueMin + Main.rand.NextFloat() * (NachtHueMax - NachtHueMin);
                Color starColor = Main.hslToRgb(starHue, 0.95f, 0.85f);
                
                // Delayed twinkling effect
                var star = new SparkleParticle(center + starOffset, Main.rand.NextVector2Circular(2f, 2f) * intensity,
                    starColor, 0.3f * intensity, (int)(30 + Main.rand.Next(15)));
                MagnumParticleHandler.SpawnParticle(star);
            }
            
            // Central glow pulse
            PulsingGlow(center, Vector2.Zero, NachtDeepPurple * 0.6f, NachtViolet * 0.3f, 
                0.4f * intensity, (int)(40 * intensity), 0.15f, 0.3f);
            
            // Sparse music notes drifting upward
            for (int n = 0; n < 3; n++)
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), -Main.rand.NextFloat(1f, 2.5f)) * intensity;
                float noteHue = NachtHueMin + Main.rand.NextFloat() * (NachtHueMax - NachtHueMin);
                ThemedParticles.MusicNote(center, noteVel, Main.hslToRgb(noteHue, 0.9f, 0.75f), 0.68f * intensity, 38);
            }
            
            Lighting.AddLight(center, NachtDeepPurple.ToVector3() * 0.9f * intensity);
        }
        
        /// <summary>
        /// NACHTMUSIK DEATH STYLE 4: Serenade Spiral
        /// Musical notes spiral outward in elegant formation.
        /// USE FOR: SerenadeStarProjectile (star-based magic)
        /// </summary>
        public static void NachtDeathSerenadeSpiral(Vector2 center, float intensity = 1f)
        {
            // Central soft flash
            CustomParticles.GenericFlare(center, NachtGold, 0.5f * intensity, 18);
            
            // Triple spiral arms of music notes
            int spiralArms = 3;
            int notesPerArm = (int)(5 * intensity);
            float baseRotation = Main.rand.NextFloat(MathHelper.TwoPi);
            
            for (int arm = 0; arm < spiralArms; arm++)
            {
                float armAngle = baseRotation + MathHelper.TwoPi * arm / spiralArms;
                
                for (int n = 0; n < notesPerArm; n++)
                {
                    // Spiral outward with increasing angle
                    float spiralProgress = (float)n / notesPerArm;
                    float radius = (10f + 35f * spiralProgress) * intensity;
                    float spiralAngle = armAngle + spiralProgress * MathHelper.Pi * 0.7f;
                    
                    Vector2 notePos = center + spiralAngle.ToRotationVector2() * radius;
                    Vector2 noteVel = spiralAngle.ToRotationVector2() * (1.5f + spiralProgress * 2f) * intensity;
                    
                    float noteHue = NachtHueMin + spiralProgress * (NachtHueMax - NachtHueMin);
                    Color noteColor = Main.hslToRgb(noteHue, 0.9f, 0.78f);
                    
                    ThemedParticles.MusicNote(notePos, noteVel, noteColor, (0.6f + spiralProgress * 0.25f) * intensity, 35);
                    
                    // Sparkle trail along spiral
                    var sparkle = new SparkleParticle(notePos, noteVel * 0.3f, noteColor * 0.7f, 
                        0.18f * intensity, 28);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }
            
            // Single expanding halo
            CustomParticles.HaloRing(center, NachtViolet, 0.4f * intensity, 22);
            
            // Soft glow particles
            for (int g = 0; g < 6; g++)
            {
                Vector2 glowVel = Main.rand.NextVector2Circular(3f, 3f) * intensity;
                float glowHue = NachtHueMin + Main.rand.NextFloat() * (NachtHueMax - NachtHueMin);
                var glow = new GenericGlowParticle(center, glowVel, Main.hslToRgb(glowHue, 0.8f, 0.65f) * 0.5f,
                    0.22f * intensity, 25, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            Lighting.AddLight(center, NachtGold.ToVector3() * 1f * intensity);
        }
        
        /// <summary>
        /// NACHTMUSIK DEATH STYLE 5: Cosmic Nova
        /// Sudden bright flash with radial light rays.
        /// USE FOR: StarweaverOrbProjectile, CosmicRequiemBeamProjectile (powerful)
        /// </summary>
        public static void NachtDeathCosmicNova(Vector2 center, float intensity = 1f)
        {
            // Blinding white core
            CustomParticles.GenericFlare(center, Color.White, 1.0f * intensity, 12);
            CustomParticles.GenericFlare(center, NachtGold, 0.75f * intensity, 16);
            CustomParticles.GenericFlare(center, NachtViolet, 0.55f * intensity, 20);
            
            // Radial light rays (16 directions)
            int rayCount = 16;
            for (int r = 0; r < rayCount; r++)
            {
                float rayAngle = MathHelper.TwoPi * r / rayCount;
                float rayLength = Main.rand.NextFloat(30f, 50f) * intensity;
                
                // Ray as multiple streak particles
                for (int seg = 0; seg < 3; seg++)
                {
                    float segStart = seg / 3f;
                    float segEnd = (seg + 1) / 3f;
                    Vector2 rayStart = center + rayAngle.ToRotationVector2() * rayLength * segStart;
                    Vector2 rayVel = rayAngle.ToRotationVector2() * (6f + seg * 2f) * intensity;
                    
                    float rayHue = NachtHueMin + (r / (float)rayCount) * (NachtHueMax - NachtHueMin);
                    Color rayColor = Main.hslToRgb(rayHue, 0.85f, 0.8f);
                    
                    var ray = new StreakParticle(rayStart, rayVel, rayColor, Color.White * 0.4f,
                        0.08f * intensity, 0.6f, 2.5f, (int)(18 * intensity));
                    MagnumParticleHandler.SpawnParticle(ray);
                }
            }
            
            // Expanding shockwave rings
            CustomParticles.HaloRing(center, NachtGold, 0.5f * intensity, 15);
            CustomParticles.HaloRing(center, NachtViolet, 0.65f * intensity, 18);
            CustomParticles.HaloRing(center, Color.White * 0.6f, 0.35f * intensity, 12);
            
            // Music notes burst outward
            for (int n = 0; n < 6; n++)
            {
                float noteAngle = MathHelper.TwoPi * n / 6f + Main.rand.NextFloat(0.2f);
                Vector2 noteVel = noteAngle.ToRotationVector2() * Main.rand.NextFloat(5f, 8f) * intensity;
                float noteHue = NachtHueMin + (n / 6f) * (NachtHueMax - NachtHueMin);
                ThemedParticles.MusicNote(center, noteVel, Main.hslToRgb(noteHue, 0.92f, 0.8f), 0.8f * intensity, 30);
            }
            
            // Bright sparkles everywhere
            for (int s = 0; s < 12; s++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(8f, 8f) * intensity;
                float sparkHue = NachtHueMin + Main.rand.NextFloat() * (NachtHueMax - NachtHueMin);
                var sparkle = new SparkleParticle(center, sparkVel, Main.hslToRgb(sparkHue, 0.9f, 0.85f),
                    0.35f * intensity, 22);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            Lighting.AddLight(center, (NachtGold.ToVector3() + NachtViolet.ToVector3()) * 0.8f * intensity);
        }
        
        /// <summary>
        /// NACHTMUSIK DEATH STYLE 6: Twilight Fade
        /// Elegant dissolve into motes of light that drift upward.
        /// USE FOR: TwilightSlashProjectile (melee slashes)
        /// </summary>
        public static void NachtDeathTwilightFade(Vector2 center, Vector2 direction, float intensity = 1f)
        {
            // Soft directional flash
            Vector2 flashOffset = direction * 10f * intensity;
            CustomParticles.GenericFlare(center + flashOffset, NachtViolet * 0.8f, 0.5f * intensity, 20);
            
            // Dissolving motes rising upward
            int moteCount = (int)(15 * intensity);
            for (int m = 0; m < moteCount; m++)
            {
                // Spread along the slash direction
                float spread = MathHelper.Lerp(-20f, 20f, (float)m / moteCount) * intensity;
                Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
                Vector2 moteStart = center + perpendicular * spread;
                
                // Drift upward with slight horizontal variance
                Vector2 moteVel = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), -Main.rand.NextFloat(1.5f, 3f)) * intensity;
                
                float moteHue = NachtHueMin + (m / (float)moteCount) * (NachtHueMax - NachtHueMin);
                Color moteColor = Main.hslToRgb(moteHue, 0.85f, 0.72f);
                
                var mote = new GenericGlowParticle(moteStart, moteVel, moteColor * 0.65f,
                    0.18f * intensity, (int)(35 + Main.rand.Next(15)), true);
                MagnumParticleHandler.SpawnParticle(mote);
                
                // Occasional sparkle
                if (Main.rand.NextBool(3))
                {
                    var sparkle = new SparkleParticle(moteStart, moteVel * 0.5f, moteColor * 0.8f,
                        0.22f * intensity, 30);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }
            
            // Gentle halo fade
            CustomParticles.HaloRing(center, NachtDeepPurple * 0.6f, 0.3f * intensity, 25);
            
            // Few music notes drifting
            for (int n = 0; n < 3; n++)
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(1f, 2f)) * intensity;
                float noteHue = NachtHueMin + Main.rand.NextFloat() * (NachtHueMax - NachtHueMin);
                ThemedParticles.MusicNote(center, noteVel, Main.hslToRgb(noteHue, 0.88f, 0.7f), 0.6f * intensity, 40);
            }
            
            Lighting.AddLight(center, NachtDeepPurple.ToVector3() * 0.7f * intensity);
        }
        
        // ===================================================================================
        // LA CAMPANELLA UNIQUE DEATH EFFECTS (6 Distinct Styles)
        // Theme: Infernal bell, black smoke → orange fire, virtuosic passion
        // ===================================================================================
        
        // La Campanella theme colors
        private static readonly Color CampanellaOrange = new Color(255, 140, 40);
        private static readonly Color CampanellaGold = new Color(255, 200, 80);
        private static readonly Color CampanellaBlack = new Color(30, 20, 25);
        private static readonly Color CampanellaCrimson = new Color(200, 50, 30);
        private static readonly float CampanellaHueMin = 0.02f;  // Red-orange
        private static readonly float CampanellaHueMax = 0.12f;  // Orange-gold
        
        /// <summary>
        /// LA CAMPANELLA DEATH STYLE 1: Bell Toll Shatter
        /// Bell-shaped shockwave that shatters into metallic shards radiating outward.
        /// USE FOR: Bell/chime-related projectiles
        /// </summary>
        public static void CampanellaDeathBellTollShatter(Vector2 center, float intensity = 1f)
        {
            // Central bell flash (white-hot core)
            CustomParticles.GenericFlare(center, Color.White, 0.8f * intensity, 18);
            CustomParticles.GenericFlare(center, CampanellaGold, 0.65f * intensity, 22);
            
            // Bell-shaped shockwave - oval halo expanding
            for (int ring = 0; ring < 3; ring++)
            {
                Color ringColor = Color.Lerp(CampanellaOrange, CampanellaGold, ring / 3f);
                CustomParticles.HaloRing(center, ringColor * (0.7f - ring * 0.15f), (0.35f + ring * 0.15f) * intensity, 18 + ring * 3);
            }
            
            // Metallic shards radiating outward (like a bell cracking)
            int shardCount = (int)(12 * intensity);
            for (int s = 0; s < shardCount; s++)
            {
                float angle = MathHelper.TwoPi * s / shardCount + Main.rand.NextFloat(-0.1f, 0.1f);
                Vector2 shardVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f) * intensity;
                
                // Alternate gold and orange shards
                Color shardColor = (s % 2 == 0) ? CampanellaGold : CampanellaOrange;
                
                var shard = new GenericGlowParticle(center, shardVel, shardColor * 0.8f,
                    0.22f * intensity, 25, true);
                MagnumParticleHandler.SpawnParticle(shard);
                
                // Trailing sparks from each shard
                if (Main.rand.NextBool(2))
                {
                    var spark = new SparkleParticle(center + shardVel * 2f, shardVel * 0.5f,
                        CampanellaGold * 0.7f, 0.18f * intensity, 20);
                    MagnumParticleHandler.SpawnParticle(spark);
                }
            }
            
            // Black smoke puffs at edges
            for (int sm = 0; sm < 4; sm++)
            {
                Vector2 smokePos = center + Main.rand.NextVector2Circular(20f, 20f) * intensity;
                var smoke = new HeavySmokeParticle(smokePos, Main.rand.NextVector2Circular(1f, 1f),
                    CampanellaBlack, 40, 0.25f * intensity, 0.45f * intensity, 0.015f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Music notes (bell chime)
            for (int n = 0; n < 4; n++)
            {
                float noteAngle = MathHelper.TwoPi * n / 4f;
                Vector2 noteVel = noteAngle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f) * intensity;
                ThemedParticles.MusicNote(center, noteVel, CampanellaGold, 0.7f * intensity, 35);
            }
            
            Lighting.AddLight(center, CampanellaOrange.ToVector3() * 1.2f * intensity);
        }
        
        /// <summary>
        /// LA CAMPANELLA DEATH STYLE 2: Infernal Pillar
        /// Fire erupts upward in a brief pillar, embers raining down.
        /// USE FOR: Fire/flame-based projectiles
        /// </summary>
        public static void CampanellaDeathInfernalPillar(Vector2 center, float intensity = 1f)
        {
            // Core flash at base
            CustomParticles.GenericFlare(center, CampanellaOrange, 0.6f * intensity, 20);
            
            // Fire pillar rising
            int pillarHeight = (int)(8 * intensity);
            for (int p = 0; p < pillarHeight; p++)
            {
                float heightOffset = p * 12f * intensity;
                Vector2 pillarPos = center - new Vector2(0, heightOffset);
                
                // Flame particles rising with variance
                Vector2 flameVel = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), -Main.rand.NextFloat(2f, 4f)) * intensity;
                float hue = CampanellaHueMin + (p / (float)pillarHeight) * (CampanellaHueMax - CampanellaHueMin);
                Color flameColor = Main.hslToRgb(hue, 1f, 0.65f);
                
                var flame = new GenericGlowParticle(pillarPos, flameVel, flameColor * 0.85f,
                    0.28f * intensity * (1f - p / (float)pillarHeight * 0.5f), 22, true);
                MagnumParticleHandler.SpawnParticle(flame);
            }
            
            // Embers raining down
            int emberCount = (int)(10 * intensity);
            for (int e = 0; e < emberCount; e++)
            {
                Vector2 emberStart = center - new Vector2(Main.rand.NextFloat(-25f, 25f) * intensity, Main.rand.NextFloat(40f, 80f) * intensity);
                Vector2 emberVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(1f, 2.5f));
                
                var ember = new SparkleParticle(emberStart, emberVel, CampanellaOrange * 0.9f,
                    0.15f * intensity, 35);
                MagnumParticleHandler.SpawnParticle(ember);
            }
            
            // Black smoke at base
            for (int sm = 0; sm < 3; sm++)
            {
                var smoke = new HeavySmokeParticle(center + Main.rand.NextVector2Circular(15f, 10f),
                    new Vector2(0, -0.5f), CampanellaBlack, 45, 0.3f * intensity, 0.55f * intensity, 0.012f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Single halo at base
            CustomParticles.HaloRing(center, CampanellaCrimson * 0.6f, 0.35f * intensity, 20);
            
            Lighting.AddLight(center, CampanellaOrange.ToVector3() * 1.0f * intensity);
        }
        
        /// <summary>
        /// LA CAMPANELLA DEATH STYLE 3: Smoke Wisp Dissolve
        /// Heavy black smoke billows out, orange sparks embedded within.
        /// USE FOR: Arrow/bolt projectiles, subtle deaths
        /// </summary>
        public static void CampanellaDeathSmokeWispDissolve(Vector2 center, Vector2 direction, float intensity = 1f)
        {
            // Soft orange flash
            CustomParticles.GenericFlare(center, CampanellaOrange * 0.7f, 0.4f * intensity, 18);
            
            // Heavy smoke billowing in movement direction
            int smokeCount = (int)(8 * intensity);
            for (int s = 0; s < smokeCount; s++)
            {
                float spread = MathHelper.Lerp(-0.5f, 0.5f, (float)s / smokeCount);
                Vector2 smokeDir = direction.RotatedBy(spread * MathHelper.Pi);
                Vector2 smokeVel = smokeDir * Main.rand.NextFloat(1f, 2.5f) * intensity;
                
                var smoke = new HeavySmokeParticle(center, smokeVel,
                    CampanellaBlack, (int)(35 + Main.rand.Next(15)), 0.25f * intensity, 0.5f * intensity, 0.018f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
                
                // Embedded sparks within smoke
                if (Main.rand.NextBool(2))
                {
                    var spark = new SparkleParticle(center + smokeVel * 3f, smokeVel * 0.3f,
                        CampanellaOrange * 0.85f, 0.12f * intensity, 22);
                    MagnumParticleHandler.SpawnParticle(spark);
                }
            }
            
            // Few orange glow motes
            for (int g = 0; g < 4; g++)
            {
                Vector2 glowVel = Main.rand.NextVector2Circular(2f, 2f) * intensity;
                var glow = new GenericGlowParticle(center, glowVel, CampanellaOrange * 0.6f,
                    0.18f * intensity, 20, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Couple music notes drifting with smoke
            ThemedParticles.MusicNote(center, direction * 1.5f * intensity, CampanellaGold * 0.8f, 0.55f * intensity, 30);
            
            Lighting.AddLight(center, CampanellaOrange.ToVector3() * 0.7f * intensity);
        }
        
        /// <summary>
        /// LA CAMPANELLA DEATH STYLE 4: Ember Cascade
        /// Fountain of embers spraying upward then falling.
        /// USE FOR: Explosive/burst projectiles
        /// </summary>
        public static void CampanellaDeathEmberCascade(Vector2 center, float intensity = 1f)
        {
            // Bright center flash
            CustomParticles.GenericFlare(center, Color.White * 0.8f, 0.7f * intensity, 16);
            CustomParticles.GenericFlare(center, CampanellaOrange, 0.55f * intensity, 20);
            
            // Ember fountain - particles spray upward in arc
            int emberCount = (int)(20 * intensity);
            for (int e = 0; e < emberCount; e++)
            {
                float arcAngle = MathHelper.Lerp(-MathHelper.PiOver4, MathHelper.PiOver4, (float)e / emberCount);
                Vector2 emberVel = new Vector2((float)Math.Sin(arcAngle) * 4f, -Main.rand.NextFloat(5f, 9f)) * intensity;
                
                float hue = CampanellaHueMin + Main.rand.NextFloat() * (CampanellaHueMax - CampanellaHueMin);
                Color emberColor = Main.hslToRgb(hue, 1f, 0.7f);
                
                var ember = new GenericGlowParticle(center, emberVel, emberColor * 0.85f,
                    0.15f * intensity, (int)(30 + Main.rand.Next(20)), true);
                MagnumParticleHandler.SpawnParticle(ember);
                
                // Trailing sparkle for some embers
                if (Main.rand.NextBool(3))
                {
                    var trail = new SparkleParticle(center, emberVel * 0.8f, CampanellaGold * 0.7f,
                        0.12f * intensity, 25);
                    MagnumParticleHandler.SpawnParticle(trail);
                }
            }
            
            // Ground-level halos
            for (int h = 0; h < 2; h++)
            {
                Color haloColor = (h == 0) ? CampanellaCrimson : CampanellaOrange;
                CustomParticles.HaloRing(center, haloColor * 0.6f, (0.3f + h * 0.12f) * intensity, 16 + h * 3);
            }
            
            // Smoke at base
            for (int sm = 0; sm < 2; sm++)
            {
                var smoke = new HeavySmokeParticle(center + new Vector2(Main.rand.NextFloat(-10f, 10f), 0),
                    new Vector2(0, -0.3f), CampanellaBlack, 35, 0.2f * intensity, 0.4f * intensity, 0.015f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            Lighting.AddLight(center, CampanellaOrange.ToVector3() * 1.1f * intensity);
        }
        
        /// <summary>
        /// LA CAMPANELLA DEATH STYLE 5: Ring of Fire
        /// Circular ring of flames expands outward.
        /// USE FOR: Area effect/ring projectiles
        /// </summary>
        public static void CampanellaDeathRingOfFire(Vector2 center, float intensity = 1f)
        {
            // Core flash
            CustomParticles.GenericFlare(center, CampanellaGold, 0.6f * intensity, 18);
            
            // Expanding fire ring
            int flameCount = (int)(16 * intensity);
            for (int f = 0; f < flameCount; f++)
            {
                float angle = MathHelper.TwoPi * f / flameCount;
                Vector2 flameVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 6f) * intensity;
                
                // Alternating colors for visual variety
                float hue = CampanellaHueMin + (f / (float)flameCount) * (CampanellaHueMax - CampanellaHueMin);
                Color flameColor = Main.hslToRgb(hue, 1f, 0.68f);
                
                var flame = new GenericGlowParticle(center, flameVel, flameColor * 0.8f,
                    0.22f * intensity, 22, true);
                MagnumParticleHandler.SpawnParticle(flame);
                
                // Secondary smaller flame
                Vector2 innerVel = flameVel * 0.6f;
                var innerFlame = new GenericGlowParticle(center, innerVel, CampanellaOrange * 0.65f,
                    0.15f * intensity, 18, true);
                MagnumParticleHandler.SpawnParticle(innerFlame);
            }
            
            // Multiple expanding halos
            for (int h = 0; h < 3; h++)
            {
                Color haloColor = Color.Lerp(CampanellaCrimson, CampanellaOrange, h / 3f);
                CustomParticles.HaloRing(center, haloColor * (0.65f - h * 0.15f), (0.3f + h * 0.18f) * intensity, 15 + h * 4);
            }
            
            // Light smoke wisps
            for (int sm = 0; sm < 3; sm++)
            {
                float smokeAngle = MathHelper.TwoPi * sm / 3f;
                Vector2 smokeVel = smokeAngle.ToRotationVector2() * 1.5f * intensity;
                var smoke = new HeavySmokeParticle(center, smokeVel,
                    CampanellaBlack, 30, 0.2f * intensity, 0.35f * intensity, 0.02f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Music notes in circle
            for (int n = 0; n < 4; n++)
            {
                float noteAngle = MathHelper.TwoPi * n / 4f + MathHelper.PiOver4;
                Vector2 noteVel = noteAngle.ToRotationVector2() * 3f * intensity;
                ThemedParticles.MusicNote(center, noteVel, CampanellaGold, 0.65f * intensity, 32);
            }
            
            Lighting.AddLight(center, CampanellaOrange.ToVector3() * 1.15f * intensity);
        }
        
        /// <summary>
        /// LA CAMPANELLA DEATH STYLE 6: Virtuosic Finale
        /// Dramatic multi-layered explosion with piano key sparkles.
        /// USE FOR: Ultimate/charged projectiles, powerful attacks
        /// </summary>
        public static void CampanellaDeathVirtuosicFinale(Vector2 center, float intensity = 1f)
        {
            // Bright multi-layer flash cascade
            CustomParticles.GenericFlare(center, Color.White, 0.9f * intensity, 22);
            CustomParticles.GenericFlare(center, CampanellaGold, 0.75f * intensity, 25);
            CustomParticles.GenericFlare(center, CampanellaOrange, 0.6f * intensity, 22);
            CustomParticles.GenericFlare(center, CampanellaCrimson, 0.45f * intensity, 20);
            
            // Piano key sparkle pattern - alternating black/white
            int keyCount = (int)(12 * intensity);
            for (int k = 0; k < keyCount; k++)
            {
                float angle = MathHelper.TwoPi * k / keyCount;
                Vector2 keyVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 8f) * intensity;
                
                // Alternate between gold (white key) and dark (black key)
                Color keyColor = (k % 2 == 0) ? CampanellaGold : new Color(60, 40, 35);
                
                var key = new SparkleParticle(center, keyVel, keyColor * 0.9f,
                    0.28f * intensity, 28);
                MagnumParticleHandler.SpawnParticle(key);
            }
            
            // Fire burst layer
            int fireCount = (int)(10 * intensity);
            for (int f = 0; f < fireCount; f++)
            {
                float angle = MathHelper.TwoPi * f / fireCount + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 fireVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f) * intensity;
                
                float hue = CampanellaHueMin + Main.rand.NextFloat() * (CampanellaHueMax - CampanellaHueMin);
                Color fireColor = Main.hslToRgb(hue, 1f, 0.72f);
                
                var fire = new GenericGlowParticle(center, fireVel, fireColor * 0.85f,
                    0.25f * intensity, 25, true);
                MagnumParticleHandler.SpawnParticle(fire);
            }
            
            // Expanding halo cascade
            for (int h = 0; h < 4; h++)
            {
                Color haloColor = Color.Lerp(CampanellaGold, CampanellaCrimson, h / 4f);
                CustomParticles.HaloRing(center, haloColor * (0.7f - h * 0.12f), (0.35f + h * 0.15f) * intensity, 18 + h * 3);
            }
            
            // Heavy smoke billows
            for (int sm = 0; sm < 5; sm++)
            {
                Vector2 smokeOffset = Main.rand.NextVector2Circular(15f, 15f) * intensity;
                var smoke = new HeavySmokeParticle(center + smokeOffset, Main.rand.NextVector2Circular(1.5f, 1.5f),
                    CampanellaBlack, 50, 0.3f * intensity, 0.6f * intensity, 0.012f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Music note finale burst
            for (int n = 0; n < 6; n++)
            {
                float noteAngle = MathHelper.TwoPi * n / 6f;
                Vector2 noteVel = noteAngle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f) * intensity;
                Color noteColor = (n % 2 == 0) ? CampanellaGold : CampanellaOrange;
                ThemedParticles.MusicNote(center, noteVel, noteColor, 0.8f * intensity, 38);
            }
            
            Lighting.AddLight(center, CampanellaOrange.ToVector3() * 1.4f * intensity);
        }
        
        #endregion
        
        #region Enigma Variations Death Styles
        
        // Enigma Variations theme colors
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaDeepPurple = new Color(80, 20, 120);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        private static readonly Color EnigmaVoid = new Color(25, 15, 35);
        private static readonly Color EnigmaMystic = new Color(100, 80, 180);
        private const float EnigmaHueMin = 0.75f; // Purple range
        private const float EnigmaHueMax = 0.85f;
        
        /// <summary>
        /// ENIGMA DEATH STYLE 1: Void Implode
        /// Reality collapses inward with swirling void particles.
        /// USE FOR: Basic projectiles, arrows, standard attacks
        /// </summary>
        public static void EnigmaDeathVoidImplode(Vector2 center, float intensity = 1f)
        {
            // Central void flash that seems to pull light inward
            CustomParticles.GenericFlare(center, EnigmaDeepPurple * 0.7f, 0.5f * intensity, 15);
            CustomParticles.GenericFlare(center, EnigmaPurple * 0.5f, 0.35f * intensity, 12);
            
            // Inward spiraling particles (implode effect)
            int particleCount = (int)(12 * intensity);
            for (int i = 0; i < particleCount; i++)
            {
                float angle = MathHelper.TwoPi * i / particleCount;
                float radius = Main.rand.NextFloat(25f, 40f) * intensity;
                Vector2 startPos = center + angle.ToRotationVector2() * radius;
                Vector2 vel = (center - startPos).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(3f, 5f) * intensity;
                
                Color voidColor = Color.Lerp(EnigmaPurple, EnigmaVoid, Main.rand.NextFloat());
                var implode = new GenericGlowParticle(startPos, vel, voidColor * 0.7f, 0.2f * intensity, 18, true);
                MagnumParticleHandler.SpawnParticle(implode);
            }
            
            // Question mark sparkles (mystery dissolve)
            for (int q = 0; q < 4; q++)
            {
                Vector2 sparkleOffset = Main.rand.NextVector2Circular(12f, 12f) * intensity;
                CustomParticles.PrismaticSparkle(center + sparkleOffset, EnigmaPurple, 0.25f * intensity);
            }
            
            // Small halo that fades quickly
            CustomParticles.HaloRing(center, EnigmaPurple * 0.5f, 0.25f * intensity, 10);
            
            Lighting.AddLight(center, EnigmaPurple.ToVector3() * 0.6f * intensity);
        }
        
        /// <summary>
        /// ENIGMA DEATH STYLE 2: Eye Blink Shatter
        /// Multiple watching eyes appear then shatter into fragments.
        /// USE FOR: Projectiles with tracking/homing, watching eye themes
        /// </summary>
        public static void EnigmaDeathEyeBlinkShatter(Vector2 center, float intensity = 1f)
        {
            // Central eye flash
            CustomParticles.GenericFlare(center, EnigmaGreen * 0.8f, 0.6f * intensity, 18);
            CustomParticles.GenericFlare(center, EnigmaPurple * 0.6f, 0.45f * intensity, 15);
            
            // Eyes spawn around center then "blink out"
            int eyeCount = (int)(5 * intensity);
            for (int e = 0; e < eyeCount; e++)
            {
                float angle = MathHelper.TwoPi * e / eyeCount;
                Vector2 eyePos = center + angle.ToRotationVector2() * (15f + Main.rand.NextFloat(10f)) * intensity;
                
                // Eye gaze effect (green core)
                CustomParticles.GenericFlare(eyePos, EnigmaGreen * 0.7f, 0.3f * intensity, 12);
                
                // Shattering fragments from each eye
                for (int f = 0; f < 3; f++)
                {
                    float fragAngle = angle + Main.rand.NextFloat(-0.5f, 0.5f);
                    Vector2 fragVel = fragAngle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f) * intensity;
                    Color fragColor = (f % 2 == 0) ? EnigmaGreen : EnigmaPurple;
                    
                    var frag = new SparkleParticle(eyePos, fragVel, fragColor * 0.8f, 0.18f * intensity, 22);
                    MagnumParticleHandler.SpawnParticle(frag);
                }
            }
            
            // Glyph accent
            CustomParticles.GlyphBurst(center, EnigmaPurple, (int)(3 * intensity), 4f * intensity);
            
            Lighting.AddLight(center, EnigmaGreen.ToVector3() * 0.7f * intensity);
        }
        
        /// <summary>
        /// ENIGMA DEATH STYLE 3: Mystery Unravel
        /// Spiraling threads of void energy unwind outward like unraveling cloth.
        /// USE FOR: Magic projectiles, orbs, energy-based attacks
        /// </summary>
        public static void EnigmaDeathMysteryUnravel(Vector2 center, float intensity = 1f)
        {
            // Soft core flash
            CustomParticles.GenericFlare(center, EnigmaMystic * 0.7f, 0.55f * intensity, 20);
            CustomParticles.GenericFlare(center, EnigmaPurple * 0.5f, 0.4f * intensity, 18);
            
            // Unraveling thread particles (spiral outward)
            int threadCount = (int)(18 * intensity);
            float baseAngle = Main.rand.NextFloat(MathHelper.TwoPi);
            for (int t = 0; t < threadCount; t++)
            {
                float spiralAngle = baseAngle + t * 0.35f;
                float radius = t * 2.5f * intensity;
                Vector2 threadPos = center + spiralAngle.ToRotationVector2() * radius;
                Vector2 threadVel = spiralAngle.ToRotationVector2() * Main.rand.NextFloat(1.5f, 3f) * intensity;
                
                float hue = EnigmaHueMin + (t / (float)threadCount) * (EnigmaHueMax - EnigmaHueMin);
                Color threadColor = Main.hslToRgb(hue, 0.8f, 0.55f);
                
                var thread = new GenericGlowParticle(threadPos, threadVel, threadColor * 0.65f, 0.15f * intensity, 25, true);
                MagnumParticleHandler.SpawnParticle(thread);
            }
            
            // Question mark particles scattered
            for (int q = 0; q < 6; q++)
            {
                Vector2 qPos = center + Main.rand.NextVector2Circular(25f, 25f) * intensity;
                Vector2 qVel = Main.rand.NextVector2Circular(2f, 2f);
                CustomParticles.GenericFlare(qPos, EnigmaPurple * 0.6f, 0.2f * intensity, 15);
            }
            
            // Fading halo
            CustomParticles.HaloRing(center, EnigmaMystic * 0.4f, 0.3f * intensity, 15);
            
            Lighting.AddLight(center, EnigmaPurple.ToVector3() * 0.5f * intensity);
        }
        
        /// <summary>
        /// ENIGMA DEATH STYLE 4: Green Flame Whisper
        /// Eerie green flames flicker and whisper away into nothing.
        /// USE FOR: Fire-based enigma attacks, torch-like projectiles
        /// </summary>
        public static void EnigmaDeathGreenFlameWhisper(Vector2 center, float intensity = 1f)
        {
            // Green flame burst core
            CustomParticles.GenericFlare(center, EnigmaGreen, 0.65f * intensity, 18);
            CustomParticles.GenericFlare(center, new Color(80, 255, 120) * 0.5f, 0.5f * intensity, 15);
            
            // Whispering flame particles that rise and fade
            int flameCount = (int)(15 * intensity);
            for (int f = 0; f < flameCount; f++)
            {
                Vector2 flameStart = center + Main.rand.NextVector2Circular(12f, 8f) * intensity;
                Vector2 flameVel = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-3f, -1.5f)) * intensity;
                
                // Green-tinted flame with subtle purple edges
                Color flameColor = Color.Lerp(EnigmaGreen, EnigmaPurple, Main.rand.NextFloat(0.3f));
                var flame = new GenericGlowParticle(flameStart, flameVel, flameColor * 0.75f, 
                    Main.rand.NextFloat(0.2f, 0.32f) * intensity, Main.rand.Next(18, 28), true);
                MagnumParticleHandler.SpawnParticle(flame);
            }
            
            // Void smoke wisps underneath
            for (int s = 0; s < 3; s++)
            {
                Vector2 smokeOffset = Main.rand.NextVector2Circular(8f, 8f) * intensity;
                var smoke = new HeavySmokeParticle(center + smokeOffset, 
                    new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -0.8f), EnigmaVoid, 
                    28, 0.2f * intensity, 0.45f * intensity, 0.015f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Small halo ring
            CustomParticles.HaloRing(center, EnigmaGreen * 0.5f, 0.22f * intensity, 12);
            
            Lighting.AddLight(center, EnigmaGreen.ToVector3() * 0.7f * intensity);
        }
        
        /// <summary>
        /// ENIGMA DEATH STYLE 5: Paradox Fracture
        /// Reality cracks with purple/green fracture lines spreading outward.
        /// USE FOR: Powerful strikes, heavy attacks, charged projectiles
        /// </summary>
        public static void EnigmaDeathParadoxFracture(Vector2 center, float intensity = 1f)
        {
            // Central paradox flash
            CustomParticles.GenericFlare(center, Color.White * 0.6f, 0.7f * intensity, 15);
            CustomParticles.GenericFlare(center, EnigmaPurple, 0.6f * intensity, 18);
            CustomParticles.GenericFlare(center, EnigmaGreen * 0.7f, 0.45f * intensity, 16);
            
            // Fracture lines radiating outward (alternating colors)
            int fractures = (int)(8 * intensity);
            for (int fr = 0; fr < fractures; fr++)
            {
                float angle = MathHelper.TwoPi * fr / fractures;
                Color lineColor = (fr % 2 == 0) ? EnigmaPurple : EnigmaGreen;
                
                // Multiple particles along each fracture line
                for (int p = 0; p < 4; p++)
                {
                    float dist = (p + 1) * 8f * intensity;
                    Vector2 pos = center + angle.ToRotationVector2() * dist;
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f) * intensity;
                    
                    var fracture = new SparkleParticle(pos, vel, lineColor * 0.8f, 0.2f * intensity, 20);
                    MagnumParticleHandler.SpawnParticle(fracture);
                }
            }
            
            // Dual-colored halo rings
            CustomParticles.HaloRing(center, EnigmaPurple * 0.6f, 0.35f * intensity, 14);
            CustomParticles.HaloRing(center, EnigmaGreen * 0.5f, 0.28f * intensity, 12);
            
            // Void smoke at fracture center
            for (int v = 0; v < 4; v++)
            {
                Vector2 voidPos = center + Main.rand.NextVector2Circular(10f, 10f) * intensity;
                var void_p = new GenericGlowParticle(voidPos, Main.rand.NextVector2Circular(1.5f, 1.5f),
                    EnigmaVoid * 0.8f, 0.18f * intensity, 22, true);
                MagnumParticleHandler.SpawnParticle(void_p);
            }
            
            Lighting.AddLight(center, (EnigmaPurple.ToVector3() + EnigmaGreen.ToVector3() * 0.5f) * 0.6f * intensity);
        }
        
        /// <summary>
        /// ENIGMA DEATH STYLE 6: Riddle Answered
        /// Grand reveal effect - the mystery is solved with dramatic flair.
        /// USE FOR: Ultimate attacks, special abilities, boss projectiles
        /// </summary>
        public static void EnigmaDeathRiddleAnswered(Vector2 center, float intensity = 1f)
        {
            // Bright revelation flash (white -> purple -> green cascade)
            CustomParticles.GenericFlare(center, Color.White * 0.9f, 0.85f * intensity, 22);
            CustomParticles.GenericFlare(center, EnigmaPurple, 0.7f * intensity, 25);
            CustomParticles.GenericFlare(center, EnigmaGreen * 0.8f, 0.55f * intensity, 22);
            CustomParticles.GenericFlare(center, EnigmaDeepPurple, 0.4f * intensity, 20);
            
            // Radial glyph burst (answer revealed)
            CustomParticles.GlyphBurst(center, EnigmaPurple, (int)(6 * intensity), 6f * intensity);
            
            // Eye formation around revelation
            int eyeCount = (int)(6 * intensity);
            for (int e = 0; e < eyeCount; e++)
            {
                float angle = MathHelper.TwoPi * e / eyeCount;
                Vector2 eyePos = center + angle.ToRotationVector2() * 25f * intensity;
                
                // Eye watches then fades
                CustomParticles.GenericFlare(eyePos, EnigmaGreen * 0.7f, 0.35f * intensity, 18);
                
                // Sparkle from each eye
                var eyeSpark = new SparkleParticle(eyePos, angle.ToRotationVector2() * 3f * intensity,
                    EnigmaGreen * 0.9f, 0.22f * intensity, 25);
                MagnumParticleHandler.SpawnParticle(eyeSpark);
            }
            
            // Expanding dual halos
            for (int h = 0; h < 3; h++)
            {
                Color haloColor = (h % 2 == 0) ? EnigmaPurple : EnigmaGreen;
                CustomParticles.HaloRing(center, haloColor * (0.6f - h * 0.15f), (0.35f + h * 0.15f) * intensity, 16 + h * 3);
            }
            
            // Central music note burst
            for (int n = 0; n < 5; n++)
            {
                float noteAngle = MathHelper.TwoPi * n / 5f;
                Vector2 noteVel = noteAngle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f) * intensity;
                Color noteColor = (n % 2 == 0) ? EnigmaPurple : EnigmaGreen;
                ThemedParticles.MusicNote(center, noteVel, noteColor, 0.75f * intensity, 35);
            }
            
            // Void smoke wisps
            for (int s = 0; s < 5; s++)
            {
                Vector2 smokePos = center + Main.rand.NextVector2Circular(15f, 15f) * intensity;
                var smoke = new HeavySmokeParticle(smokePos, Main.rand.NextVector2Circular(1.5f, 1.5f),
                    EnigmaVoid, 45, 0.25f * intensity, 0.5f * intensity, 0.012f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            Lighting.AddLight(center, (EnigmaPurple.ToVector3() * 0.8f + EnigmaGreen.ToVector3() * 0.5f) * intensity);
        }
        
        #endregion

        // ===================================================================================
        // EROICA UNIQUE DEATH EFFECTS (6 Distinct Styles)
        // Theme: Heroic triumph, scarlet → gold gradient, sakura petals
        // SIMPLIFIED per user request - clean, elegant, not busy
        // ===================================================================================

        #region Eroica Death Styles

        // Eroica theme colors
        private static readonly Color EroicaScarlet = new Color(200, 50, 50);
        private static readonly Color EroicaCrimson = new Color(180, 30, 60);
        private static readonly Color EroicaGold = new Color(255, 200, 80);
        private static readonly Color EroicaSakura = new Color(255, 150, 180);
        private static readonly Color EroicaWhite = new Color(255, 255, 255);

        /// <summary>
        /// EROICA DEATH STYLE 1: Heroic Flash
        /// A clean, bright burst of heroic gold. Simple but impactful.
        /// Best for: Basic projectiles, arrows, standard attacks.
        /// </summary>
        public static void EroicaDeathHeroicFlash(Vector2 center, float intensity = 1f)
        {
            // Central bright flash
            CustomParticles.GenericFlare(center, EroicaWhite, 0.6f * intensity, 15);
            CustomParticles.GenericFlare(center, EroicaGold, 0.45f * intensity, 12);
            
            // Simple expanding halo
            CustomParticles.HaloRing(center, EroicaGold, 0.3f * intensity, 14);
            
            // A few subtle sparks
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f) * intensity;
                var spark = new SparkleParticle(center, vel, EroicaGold, 0.25f * intensity, 18);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            Lighting.AddLight(center, EroicaGold.ToVector3() * 0.7f * intensity);
        }

        /// <summary>
        /// EROICA DEATH STYLE 2: Valor Burst
        /// A scarlet-to-gold gradient burst. Clean and powerful.
        /// Best for: Charged attacks, strong projectiles, empowered abilities.
        /// </summary>
        public static void EroicaDeathValorBurst(Vector2 center, float intensity = 1f)
        {
            // Layered central flares
            CustomParticles.GenericFlare(center, EroicaWhite, 0.7f * intensity, 18);
            CustomParticles.GenericFlare(center, EroicaScarlet, 0.55f * intensity, 15);
            CustomParticles.GenericFlare(center, EroicaGold, 0.4f * intensity, 12);
            
            // Gradient halos
            CustomParticles.HaloRing(center, EroicaScarlet, 0.35f * intensity, 16);
            CustomParticles.HaloRing(center, EroicaGold, 0.25f * intensity, 14);
            
            // Radial sparkle burst
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f) * intensity;
                Color sparkColor = Color.Lerp(EroicaScarlet, EroicaGold, i / 6f);
                var spark = new SparkleParticle(center, vel, sparkColor, 0.3f * intensity, 20);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            Lighting.AddLight(center, EroicaScarlet.ToVector3() * 0.9f * intensity);
        }

        /// <summary>
        /// EROICA DEATH STYLE 3: Sakura Scatter
        /// Sakura petals gently scatter. Elegant and graceful.
        /// Best for: Melee swings, graceful attacks, wind-up projectiles.
        /// </summary>
        public static void EroicaDeathSakuraScatter(Vector2 center, float intensity = 1f)
        {
            // Soft central glow
            CustomParticles.GenericFlare(center, EroicaSakura, 0.45f * intensity, 14);
            CustomParticles.GenericFlare(center, EroicaWhite * 0.7f, 0.35f * intensity, 10);
            
            // Gentle halo
            CustomParticles.HaloRing(center, EroicaSakura, 0.25f * intensity, 15);
            
            // Sakura petals drifting outward
            for (int p = 0; p < (int)(5 * intensity); p++)
            {
                float angle = MathHelper.TwoPi * p / 5f + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 petalVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                petalVel.Y -= Main.rand.NextFloat(0.5f, 1.5f); // Slight upward drift
                Color petalColor = Color.Lerp(EroicaSakura, EroicaWhite, Main.rand.NextFloat(0.3f));
                var petal = new GenericGlowParticle(center, petalVel, petalColor, 0.25f * intensity, 25, true);
                MagnumParticleHandler.SpawnParticle(petal);
            }
            
            Lighting.AddLight(center, EroicaSakura.ToVector3() * 0.6f * intensity);
        }

        /// <summary>
        /// EROICA DEATH STYLE 4: Golden Glow
        /// A warm, lingering golden fade. Subtle and majestic.
        /// Best for: Homing projectiles, buff effects, support abilities.
        /// </summary>
        public static void EroicaDeathGoldenGlow(Vector2 center, float intensity = 1f)
        {
            // Warm golden bloom
            CustomParticles.GenericFlare(center, EroicaGold, 0.5f * intensity, 20);
            CustomParticles.GenericFlare(center, EroicaWhite * 0.5f, 0.3f * intensity, 15);
            
            // Soft expanding glow
            var glowParticle = new GenericGlowParticle(center, Vector2.Zero, EroicaGold * 0.8f, 
                0.4f * intensity, 25, true);
            MagnumParticleHandler.SpawnParticle(glowParticle);
            
            // Gentle sparkle motes
            for (int i = 0; i < 3; i++)
            {
                Vector2 motePos = center + Main.rand.NextVector2Circular(10f, 10f) * intensity;
                Vector2 moteVel = Main.rand.NextVector2Circular(1.5f, 1.5f);
                var mote = new SparkleParticle(motePos, moteVel, EroicaGold, 0.2f * intensity, 22);
                MagnumParticleHandler.SpawnParticle(mote);
            }
            
            Lighting.AddLight(center, EroicaGold.ToVector3() * 0.8f * intensity);
        }

        /// <summary>
        /// EROICA DEATH STYLE 5: Crimson Spark
        /// A sharp crimson flash with ember sparks. Fierce but controlled.
        /// Best for: Fire attacks, aggressive projectiles, damage-focused effects.
        /// </summary>
        public static void EroicaDeathCrimsonSpark(Vector2 center, float intensity = 1f)
        {
            // Sharp central flash
            CustomParticles.GenericFlare(center, EroicaCrimson, 0.55f * intensity, 12);
            CustomParticles.GenericFlare(center, EroicaScarlet * 0.8f, 0.4f * intensity, 10);
            
            // Quick expanding ring
            CustomParticles.HaloRing(center, EroicaCrimson, 0.3f * intensity, 12);
            
            // Ember sparks shooting outward
            for (int e = 0; e < 5; e++)
            {
                float angle = MathHelper.TwoPi * e / 5f + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 emberVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 6f) * intensity;
                Color emberColor = Color.Lerp(EroicaCrimson, EroicaGold, Main.rand.NextFloat(0.4f));
                var ember = new GlowSparkParticle(center, emberVel, emberColor, 0.2f * intensity, 16);
                MagnumParticleHandler.SpawnParticle(ember);
            }
            
            Lighting.AddLight(center, EroicaCrimson.ToVector3() * intensity);
        }

        /// <summary>
        /// EROICA DEATH STYLE 6: Triumph Fade
        /// A majestic, multi-layered fade. The grand finale style.
        /// Best for: Ultimate attacks, special abilities, boss-tier projectiles.
        /// </summary>
        public static void EroicaDeathTriumphFade(Vector2 center, float intensity = 1f)
        {
            // Bright white core flash
            CustomParticles.GenericFlare(center, EroicaWhite, 0.8f * intensity, 20);
            
            // Layered gradient flares
            CustomParticles.GenericFlare(center, EroicaGold, 0.6f * intensity, 18);
            CustomParticles.GenericFlare(center, EroicaScarlet, 0.45f * intensity, 15);
            CustomParticles.GenericFlare(center, EroicaCrimson, 0.35f * intensity, 12);
            
            // Cascading halos
            for (int h = 0; h < 3; h++)
            {
                Color haloColor = Color.Lerp(EroicaScarlet, EroicaGold, h / 3f);
                CustomParticles.HaloRing(center, haloColor, (0.25f + h * 0.1f) * intensity, 14 + h * 2);
            }
            
            // Radial sparkle spray
            for (int s = 0; s < 8; s++)
            {
                float angle = MathHelper.TwoPi * s / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f) * intensity;
                Color sparkColor = Color.Lerp(EroicaScarlet, EroicaGold, s / 8f);
                var spark = new SparkleParticle(center, vel, sparkColor, 0.35f * intensity, 22);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // Music note finale
            for (int n = 0; n < 3; n++)
            {
                float noteAngle = MathHelper.TwoPi * n / 3f;
                Vector2 noteVel = noteAngle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f) * intensity;
                ThemedParticles.MusicNote(center, noteVel, EroicaGold, 0.65f * intensity, 30);
            }
            
            Lighting.AddLight(center, (EroicaGold.ToVector3() + EroicaScarlet.ToVector3() * 0.5f) * intensity);
        }

        #endregion

        #region ========== MOONLIGHT SONATA SIMPLIFIED DEATH STYLES ==========
        
        // Moonlight colors - serene, silver, purple, blue, water/moon imagery
        private static readonly Color MoonlightPurple = new Color(100, 60, 160);
        private static readonly Color MoonlightSilver = new Color(200, 210, 230);
        private static readonly Color MoonlightBlue = new Color(80, 120, 200);
        private static readonly Color MoonlightViolet = new Color(140, 90, 180);
        private static readonly Color MoonlightWhite = new Color(230, 235, 255);

        /// <summary>
        /// Style 1: LunarRipple - Gentle water-like ripples expanding outward
        /// Use for: Basic projectiles, waves, calm effects
        /// </summary>
        public static void MoonlightDeathLunarRipple(Vector2 center, float intensity = 1f)
        {
            // Soft central glow
            CustomParticles.GenericFlare(center, MoonlightSilver * 0.8f, 0.4f * intensity, 18);
            
            // 3 gentle expanding rings like water ripples
            for (int ring = 0; ring < 3; ring++)
            {
                float delay = ring * 0.12f;
                Color ringColor = Color.Lerp(MoonlightPurple, MoonlightSilver, ring / 3f) * (0.6f - ring * 0.15f);
                CustomParticles.HaloRing(center, ringColor, (0.25f + ring * 0.12f) * intensity, 14 + ring * 3);
            }
            
            // Few soft silver particles drifting
            for (int i = 0; i < (int)(4 * intensity); i++)
            {
                Vector2 drift = Main.rand.NextVector2Circular(2f, 2f);
                Dust d = Dust.NewDustPerfect(center, DustID.PurpleTorch, drift, 80, MoonlightSilver, 1.0f);
                d.noGravity = true;
            }
            
            Lighting.AddLight(center, MoonlightSilver.ToVector3() * 0.6f * intensity);
        }

        /// <summary>
        /// Style 2: MoonbeamFade - Gentle upward shimmer like moonlight ascending
        /// Use for: Beam projectiles, light attacks, ascending effects
        /// </summary>
        public static void MoonlightDeathMoonbeamFade(Vector2 center, float intensity = 1f)
        {
            // Central soft bloom
            CustomParticles.GenericFlare(center, MoonlightWhite * 0.7f, 0.35f * intensity, 16);
            CustomParticles.GenericFlare(center, MoonlightBlue * 0.5f, 0.5f * intensity, 20);
            
            // Upward drifting silver particles
            for (int i = 0; i < (int)(5 * intensity); i++)
            {
                Vector2 upDrift = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-3f, -1f)) * intensity;
                Color shimmer = Color.Lerp(MoonlightSilver, MoonlightWhite, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(center + Main.rand.NextVector2Circular(8f, 4f), DustID.BlueTorch, upDrift, 60, shimmer, 1.1f);
                d.noGravity = true;
            }
            
            // Single soft halo
            CustomParticles.HaloRing(center, MoonlightPurple * 0.5f, 0.3f * intensity, 15);
            
            Lighting.AddLight(center, MoonlightBlue.ToVector3() * 0.7f * intensity);
        }

        /// <summary>
        /// Style 3: TwilightSparkle - Soft scattered stardust sparkles
        /// Use for: Magic projectiles, sparkle effects, ethereal attacks
        /// </summary>
        public static void MoonlightDeathTwilightSparkle(Vector2 center, float intensity = 1f)
        {
            // Soft central flash
            CustomParticles.GenericFlare(center, MoonlightViolet * 0.6f, 0.4f * intensity, 15);
            
            // Scattered sparkle points
            int sparkleCount = (int)(6 * intensity);
            for (int i = 0; i < sparkleCount; i++)
            {
                float angle = MathHelper.TwoPi * i / sparkleCount;
                float dist = Main.rand.NextFloat(15f, 30f) * intensity;
                Vector2 sparklePos = center + angle.ToRotationVector2() * dist;
                Vector2 sparkleVel = angle.ToRotationVector2() * Main.rand.NextFloat(1f, 2.5f);
                Color sparkColor = Color.Lerp(MoonlightSilver, MoonlightPurple, i / (float)sparkleCount);
                
                Dust s = Dust.NewDustPerfect(sparklePos, DustID.PurpleTorch, sparkleVel, 40, sparkColor, 0.9f);
                s.noGravity = true;
            }
            
            // Single music note
            if (intensity > 0.5f)
            {
                ThemedParticles.MusicNote(center, Main.rand.NextVector2Circular(1f, 1f), MoonlightViolet, 0.55f * intensity, 25);
            }
            
            Lighting.AddLight(center, MoonlightViolet.ToVector3() * 0.5f * intensity);
        }

        /// <summary>
        /// Style 4: SilverMist - Soft misty dissipation
        /// Use for: Homing projectiles, orbs, support effects
        /// </summary>
        public static void MoonlightDeathSilverMist(Vector2 center, float intensity = 1f)
        {
            // Soft bloom core
            CustomParticles.GenericFlare(center, MoonlightSilver * 0.5f, 0.45f * intensity, 20);
            
            // Misty particles expanding gently
            for (int i = 0; i < (int)(6 * intensity); i++)
            {
                Vector2 mistDrift = Main.rand.NextVector2Circular(3f, 3f);
                Dust mist = Dust.NewDustPerfect(center + Main.rand.NextVector2Circular(6f, 6f), DustID.Cloud, mistDrift, 100, MoonlightSilver, 1.3f);
                mist.noGravity = true;
                mist.fadeIn = 0.8f;
            }
            
            // Soft purple undertone
            Dust purple = Dust.NewDustPerfect(center, DustID.PurpleTorch, Vector2.Zero, 60, MoonlightPurple, 1.5f);
            purple.noGravity = true;
            
            Lighting.AddLight(center, MoonlightSilver.ToVector3() * 0.5f * intensity);
        }

        /// <summary>
        /// Style 5: NightfallGlimmer - Brief bright flash then fade
        /// Use for: Quick/aggressive projectiles, impact effects
        /// </summary>
        public static void MoonlightDeathNightfallGlimmer(Vector2 center, float intensity = 1f)
        {
            // Quick bright flash
            CustomParticles.GenericFlare(center, MoonlightWhite * 0.9f, 0.5f * intensity, 12);
            CustomParticles.GenericFlare(center, MoonlightBlue * 0.7f, 0.35f * intensity, 16);
            
            // Small burst of particles
            for (int i = 0; i < (int)(5 * intensity); i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f) * intensity;
                Dust burst = Dust.NewDustPerfect(center, DustID.BlueTorch, burstVel, 50, MoonlightBlue, 1.0f);
                burst.noGravity = true;
            }
            
            // Quick fading halo
            CustomParticles.HaloRing(center, MoonlightPurple * 0.6f, 0.25f * intensity, 12);
            
            Lighting.AddLight(center, MoonlightBlue.ToVector3() * 0.8f * intensity);
        }

        /// <summary>
        /// Style 6: SerenadeFinale - Grand multi-layer moonlight burst
        /// Use for: Ultimate attacks, charged projectiles, special abilities
        /// </summary>
        public static void MoonlightDeathSerenadeFinale(Vector2 center, float intensity = 1f)
        {
            // Layered central bloom
            CustomParticles.GenericFlare(center, MoonlightWhite * 0.85f, 0.6f * intensity, 20);
            CustomParticles.GenericFlare(center, MoonlightSilver * 0.7f, 0.5f * intensity, 22);
            CustomParticles.GenericFlare(center, MoonlightPurple * 0.5f, 0.7f * intensity, 25);
            
            // 4 expanding rings in gradient
            for (int ring = 0; ring < 4; ring++)
            {
                Color ringColor = Color.Lerp(MoonlightPurple, MoonlightSilver, ring / 4f) * (0.6f - ring * 0.1f);
                CustomParticles.HaloRing(center, ringColor, (0.3f + ring * 0.15f) * intensity, 16 + ring * 3);
            }
            
            // Radial particle spray
            for (int i = 0; i < (int)(8 * intensity); i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f) * intensity;
                Color particleColor = Color.Lerp(MoonlightBlue, MoonlightViolet, i / 8f);
                Dust d = Dust.NewDustPerfect(center, DustID.PurpleTorch, vel, 40, particleColor, 1.2f);
                d.noGravity = true;
            }
            
            // Music notes arc
            for (int n = 0; n < 3; n++)
            {
                float noteAngle = MathHelper.TwoPi * n / 3f;
                Vector2 noteVel = noteAngle.ToRotationVector2() * Main.rand.NextFloat(1.5f, 3f) * intensity;
                ThemedParticles.MusicNote(center, noteVel, MoonlightSilver, 0.6f * intensity, 28);
            }
            
            Lighting.AddLight(center, (MoonlightSilver.ToVector3() + MoonlightPurple.ToVector3() * 0.3f) * intensity);
        }

        #endregion
    }
}
