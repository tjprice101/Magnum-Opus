using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// BOSS ARENA VFX SYSTEM
    /// 
    /// Creates persistent ambient visual effects during boss fights:
    /// - Floating ambient particles with parallax depth
    /// - Environmental effects (sky changes, lighting shifts)
    /// - Theme-specific atmospheric elements
    /// 
    /// ============================================
    /// DESIGN PHILOSOPHY - USES EXISTING SYSTEMS
    /// ============================================
    /// 
    /// This system BUILDS UPON the existing VFX infrastructure:
    /// 
    /// - InterpolatedRenderer: For 144Hz+ smooth particle positions
    /// - BloomRenderer: For soft glows and multi-layer bloom
    /// - GodRaySystem: For volumetric ambient light rays
    /// - ProceduralProjectileVFX: For PNG-free procedural shapes
    /// 
    /// All particles are rendered using:
    /// - Sub-pixel interpolation for smooth 144Hz+ movement
    /// - Multi-layer additive bloom for proper glow
    /// - Seeded random for consistent positions across frames
    /// 
    /// NO raw PNG loading - use BloomRenderer and procedural drawing.
    /// </summary>
    public class BossArenaVFX : ModSystem
    {
        #region Static Access

        private static BossArenaVFX _instance;
        public static BossArenaVFX Instance => _instance;

        #endregion

        #region State

        private bool _isActive = false;
        private string _currentTheme = "";
        private List<AmbientParticle> _particles = new List<AmbientParticle>();
        private List<EnvironmentalEffect> _effects = new List<EnvironmentalEffect>();
        private float _intensity = 1f;
        private Vector2 _arenaCenter = Vector2.Zero;
        private float _arenaRadius = 1000f;

        // Interpolation state
        private Vector2 _previousCameraPosition;
        private Vector2 _currentCameraPosition;

        // Configuration
        private const int MAX_PARTICLES = 120;
        private const int MAX_EFFECTS = 8;

        #endregion

        #region Lifecycle

        public override void Load()
        {
            _instance = this;
            On_Main.DrawDust += DrawArenaVFX;
        }

        public override void Unload()
        {
            On_Main.DrawDust -= DrawArenaVFX;
            _particles?.Clear();
            _effects?.Clear();
            _instance = null;
        }

        public override void PostUpdateDusts()
        {
            if (!_isActive) return;

            // Update interpolation positions
            _previousCameraPosition = _currentCameraPosition;
            _currentCameraPosition = Main.screenPosition;

            UpdateParticles();
            UpdateEffects();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Activates boss arena VFX with the specified theme.
        /// </summary>
        public static void Activate(string theme, Vector2 arenaCenter, float arenaRadius = 1000f, float intensity = 1f)
        {
            if (Instance == null) return;

            Instance._isActive = true;
            Instance._currentTheme = theme;
            Instance._arenaCenter = arenaCenter;
            Instance._arenaRadius = arenaRadius;
            Instance._intensity = intensity;
            Instance._particles.Clear();
            Instance._effects.Clear();

            // Initialize camera tracking
            Instance._currentCameraPosition = Main.screenPosition;
            Instance._previousCameraPosition = Main.screenPosition;

            // Spawn initial particles based on theme
            Instance.SpawnThemeParticles(theme);
            Instance.SpawnThemeEffects(theme);
        }

        /// <summary>
        /// Deactivates all boss arena VFX.
        /// </summary>
        public static void Deactivate()
        {
            if (Instance == null) return;

            Instance._isActive = false;
            Instance._particles.Clear();
            Instance._effects.Clear();
        }

        /// <summary>
        /// Sets the intensity of the arena VFX (0-2 range).
        /// </summary>
        public static void SetIntensity(float intensity)
        {
            if (Instance != null)
                Instance._intensity = MathHelper.Clamp(intensity, 0f, 2f);
        }

        /// <summary>
        /// Updates the arena center (for moving bosses).
        /// </summary>
        public static void UpdateArenaCenter(Vector2 newCenter)
        {
            if (Instance != null)
                Instance._arenaCenter = newCenter;
        }

        #endregion

        #region Theme Particle Spawning

        private void SpawnThemeParticles(string theme)
        {
            int count = (int)(60 * _intensity);
            count = Math.Min(count, MAX_PARTICLES);

            Color[] colors = GetThemeColors(theme);
            ParticleType[] types = GetThemeParticleTypes(theme);

            for (int i = 0; i < count; i++)
            {
                // Use seeded random for consistent positions
                float seedAngle = i * MathHelper.TwoPi / count + Main.rand.NextFloat(-0.2f, 0.2f);
                float seedDist = Main.rand.NextFloat(0.3f, 1f) * _arenaRadius;

                Vector2 offset = seedAngle.ToRotationVector2() * seedDist;
                Vector2 startPos = _arenaCenter + offset;

                var particle = new AmbientParticle
                {
                    Position = startPos,
                    PreviousPosition = startPos,
                    Velocity = GetThemeParticleVelocity(theme),
                    Color = colors[Main.rand.Next(colors.Length)],
                    Scale = Main.rand.NextFloat(0.15f, 0.4f) * _intensity,
                    Rotation = Main.rand.NextFloat(MathHelper.TwoPi),
                    RotationSpeed = Main.rand.NextFloat(-0.03f, 0.03f),
                    Lifetime = 9999, // Persistent until deactivated
                    MaxLifetime = 9999,
                    ParallaxDepth = Main.rand.NextFloat(0.3f, 1f),
                    Type = types[Main.rand.Next(types.Length)],
                    PulseSpeed = Main.rand.NextFloat(1f, 3f),
                    PulsePhase = Main.rand.NextFloat(MathHelper.TwoPi),
                    SeedIndex = i
                };

                _particles.Add(particle);
            }
        }

        private void SpawnThemeEffects(string theme)
        {
            // Ambient god rays
            _effects.Add(new EnvironmentalEffect
            {
                Type = EffectType.AmbientGlow,
                Position = _arenaCenter,
                Color = GetThemeColors(theme)[0],
                Scale = 2f * _intensity,
                Duration = 9999
            });

            // Theme-specific effects
            switch (theme.ToLowerInvariant())
            {
                case "lacampanella":
                    _effects.Add(new EnvironmentalEffect
                    {
                        Type = EffectType.SmokeHaze,
                        Position = _arenaCenter,
                        Color = new Color(30, 20, 25),
                        Scale = 1.5f,
                        Duration = 9999
                    });
                    break;

                case "fate":
                    _effects.Add(new EnvironmentalEffect
                    {
                        Type = EffectType.StarField,
                        Position = _arenaCenter,
                        Color = new Color(200, 180, 255),
                        Scale = 1.2f,
                        Duration = 9999
                    });
                    break;

                case "swanlake":
                    _effects.Add(new EnvironmentalEffect
                    {
                        Type = EffectType.Prismatic,
                        Position = _arenaCenter,
                        Color = Color.White,
                        Scale = 1f,
                        Duration = 9999
                    });
                    break;
            }
        }

        #endregion

        #region Theme Configuration

        private Color[] GetThemeColors(string theme)
        {
            return theme.ToLowerInvariant() switch
            {
                "lacampanella" => new[] { new Color(255, 140, 40), new Color(255, 100, 20), new Color(200, 80, 10) },
                "eroica" => new[] { new Color(255, 150, 180), new Color(255, 200, 220), new Color(255, 215, 0) },
                "swanlake" => new[] { Color.White, new Color(230, 230, 240), new Color(200, 200, 220) },
                "moonlightsonata" => new[] { new Color(135, 180, 255), new Color(100, 140, 220), new Color(180, 200, 255) },
                "enigma" or "enigmavariations" => new[] { new Color(140, 60, 200), new Color(80, 20, 140), new Color(50, 200, 100) },
                "fate" => new[] { new Color(200, 80, 120), new Color(150, 40, 100), new Color(255, 200, 220) },
                "diesirae" => new[] { new Color(180, 30, 30), new Color(220, 50, 30), new Color(140, 20, 20) },
                "clairdelune" => new[] { new Color(200, 220, 255), new Color(180, 200, 240), new Color(220, 230, 255) },
                _ => new[] { Color.White, new Color(200, 200, 200) }
            };
        }

        private ParticleType[] GetThemeParticleTypes(string theme)
        {
            return theme.ToLowerInvariant() switch
            {
                "lacampanella" => new[] { ParticleType.Ember, ParticleType.Glow, ParticleType.Spark },
                "eroica" => new[] { ParticleType.Petal, ParticleType.Glow, ParticleType.Sparkle },
                "swanlake" => new[] { ParticleType.Feather, ParticleType.Sparkle, ParticleType.Prismatic },
                "moonlightsonata" => new[] { ParticleType.Crystal, ParticleType.Glow, ParticleType.Dust },
                "enigma" or "enigmavariations" => new[] { ParticleType.Void, ParticleType.Glyph, ParticleType.Glow },
                "fate" => new[] { ParticleType.Star, ParticleType.Void, ParticleType.Glyph },
                "diesirae" => new[] { ParticleType.Ember, ParticleType.Spark, ParticleType.Glow },
                "clairdelune" => new[] { ParticleType.Glow, ParticleType.Dust, ParticleType.Crystal },
                _ => new[] { ParticleType.Glow, ParticleType.Dust }
            };
        }

        private Vector2 GetThemeParticleVelocity(string theme)
        {
            return theme.ToLowerInvariant() switch
            {
                "lacampanella" => new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-2f, -0.5f)), // Rising embers
                "eroica" => new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 0.5f)), // Drifting petals
                "swanlake" => new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-0.3f, 0.3f)), // Gentle drift
                "fate" => new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-0.3f, 0.3f)), // Slow cosmic drift
                _ => new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-0.5f, 0.5f))
            };
        }

        #endregion

        #region Update Logic

        private void UpdateParticles()
        {
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                var particle = _particles[i];

                // Store previous position for interpolation
                particle.PreviousPosition = particle.Position;

                // Update position
                particle.Position += particle.Velocity;
                particle.Rotation += particle.RotationSpeed;

                // Wrap around arena bounds
                Vector2 toCenter = _arenaCenter - particle.Position;
                if (toCenter.Length() > _arenaRadius * 1.5f)
                {
                    // Respawn on opposite side
                    Vector2 respawnDir = toCenter.SafeNormalize(Vector2.UnitY);
                    particle.Position = _arenaCenter + respawnDir * _arenaRadius * 0.9f;
                    particle.PreviousPosition = particle.Position;
                }

                // Add some drift variation
                particle.Velocity += Main.rand.NextVector2Circular(0.02f, 0.02f);
                particle.Velocity *= 0.99f; // Slight damping

                _particles[i] = particle;
            }
        }

        private void UpdateEffects()
        {
            for (int i = _effects.Count - 1; i >= 0; i--)
            {
                var effect = _effects[i];
                effect.Timer++;

                if (effect.Timer >= effect.Duration && effect.Duration < 9999)
                {
                    _effects.RemoveAt(i);
                    continue;
                }

                _effects[i] = effect;
            }
        }

        #endregion

        #region Drawing

        private void DrawArenaVFX(On_Main.orig_DrawDust orig, Main self)
        {
            orig(self);

            if (!_isActive || _particles.Count == 0) return;

            // Calculate interpolation factor
            InterpolatedRenderer.UpdatePartialTicks();
            float partialTicks = InterpolatedRenderer.PartialTicks;

            // Track SpriteBatch state for proper restoration
            bool endedSuccessfully = false;
            try
            {
                Main.spriteBatch.End();
                endedSuccessfully = true;
            }
            catch (System.InvalidOperationException)
            {
                // SpriteBatch wasn't active - that's fine, we'll just begin fresh
            }

            try
            {
                // Begin additive blending
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                // Draw environmental effects first
                foreach (var effect in _effects)
                {
                    DrawEnvironmentalEffect(effect, partialTicks);
                }

                // Sort particles by parallax depth (back to front)
                _particles.Sort((a, b) => a.ParallaxDepth.CompareTo(b.ParallaxDepth));

                // Draw particles with interpolation
                foreach (var particle in _particles)
                {
                    DrawParticle(particle, partialTicks);
                }

                // End our additive batch
                Main.spriteBatch.End();

                // Restore original state if we ended it
                if (endedSuccessfully)
                {
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }
            catch (System.Exception ex)
            {
                // Log error and attempt recovery
                Mod?.Logger.Error($"BossArenaVFX.DrawArenaVFX error: {ex.Message}");
                try
                {
                    if (endedSuccessfully)
                    {
                        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                            DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                    }
                }
                catch { /* Last resort recovery failed, let it pass */ }
            }
        }

        private void DrawParticle(AmbientParticle particle, float partialTicks)
        {
            // Calculate interpolated position for smooth 144Hz+ rendering
            Vector2 interpolatedPos = Vector2.Lerp(particle.PreviousPosition, particle.Position, partialTicks);

            // Apply parallax based on camera movement
            Vector2 cameraInterpolated = Vector2.Lerp(_previousCameraPosition, _currentCameraPosition, partialTicks);
            Vector2 parallaxOffset = (cameraInterpolated - _arenaCenter) * (1f - particle.ParallaxDepth) * 0.1f;
            Vector2 finalPos = interpolatedPos - parallaxOffset;

            // Convert to screen coordinates
            Vector2 screenPos = finalPos - Main.screenPosition;

            // Skip if off-screen
            if (screenPos.X < -100 || screenPos.X > Main.screenWidth + 100 ||
                screenPos.Y < -100 || screenPos.Y > Main.screenHeight + 100)
                return;

            // Calculate pulsing scale
            float time = Main.GlobalTimeWrappedHourly;
            float pulse = 1f + MathF.Sin(time * particle.PulseSpeed + particle.PulsePhase) * 0.2f;
            float finalScale = particle.Scale * pulse * _intensity;

            // Draw using BloomRenderer based on particle type
            DrawParticleByType(particle, screenPos, finalScale);
        }

        private void DrawParticleByType(AmbientParticle particle, Vector2 screenPos, float scale)
        {
            switch (particle.Type)
            {
                case ParticleType.Glow:
                    BloomRenderer.DrawSimpleBloom(Main.spriteBatch, screenPos, particle.Color, scale, 0.6f);
                    break;

                case ParticleType.Ember:
                    // Ember: Warm glow with slight flicker
                    float flicker = 0.8f + Main.rand.NextFloat(0.2f);
                    BloomRenderer.DrawSimpleBloom(Main.spriteBatch, screenPos, particle.Color, scale * flicker, 0.7f);
                    break;

                case ParticleType.Spark:
                    // Spark: Small, bright point
                    BloomRenderer.DrawSimpleBloom(Main.spriteBatch, screenPos, particle.Color, scale * 0.5f, 0.9f);
                    BloomRenderer.DrawSimpleBloom(Main.spriteBatch, screenPos, Color.White, scale * 0.2f, 0.8f);
                    break;

                case ParticleType.Petal:
                    // Petal: Elongated soft glow
                    BloomRenderer.DrawSimpleBloom(Main.spriteBatch, screenPos, particle.Color, scale * 0.8f, 0.5f);
                    break;

                case ParticleType.Feather:
                    // Feather: Soft with slight elongation
                    BloomRenderer.DrawSimpleBloom(Main.spriteBatch, screenPos, particle.Color, scale, 0.4f);
                    BloomRenderer.DrawSimpleBloom(Main.spriteBatch, screenPos, Color.White * 0.5f, scale * 0.3f, 0.6f);
                    break;

                case ParticleType.Crystal:
                    // Crystal: Sharp, bright with white core
                    BloomRenderer.DrawSimpleBloom(Main.spriteBatch, screenPos, particle.Color, scale * 0.6f, 0.8f);
                    BloomRenderer.DrawSimpleBloom(Main.spriteBatch, screenPos, Color.White, scale * 0.2f, 0.9f);
                    break;

                case ParticleType.Sparkle:
                    // Sparkle: Multi-layer with twinkle
                    float twinkle = 0.5f + MathF.Sin(Main.GlobalTimeWrappedHourly * 8f + particle.SeedIndex) * 0.5f;
                    BloomRenderer.DrawSimpleBloom(Main.spriteBatch, screenPos, particle.Color, scale * twinkle, 0.7f);
                    break;

                case ParticleType.Prismatic:
                    // Prismatic: Rainbow color cycling
                    float hue = (Main.GlobalTimeWrappedHourly * 0.5f + particle.SeedIndex * 0.1f) % 1f;
                    Color rainbowColor = Main.hslToRgb(hue, 1f, 0.7f);
                    BloomRenderer.DrawSimpleBloom(Main.spriteBatch, screenPos, rainbowColor, scale, 0.6f);
                    break;

                case ParticleType.Star:
                    // Star: Bright twinkling point
                    float starTwinkle = 0.4f + MathF.Sin(Main.GlobalTimeWrappedHourly * 6f + particle.PulsePhase) * 0.6f;
                    BloomRenderer.DrawSimpleBloom(Main.spriteBatch, screenPos, particle.Color, scale * starTwinkle * 0.5f, 0.9f);
                    BloomRenderer.DrawSimpleBloom(Main.spriteBatch, screenPos, Color.White, scale * starTwinkle * 0.15f, 1f);
                    break;

                case ParticleType.Void:
                    // Void: Dark core with colored edge
                    BloomRenderer.DrawSimpleBloom(Main.spriteBatch, screenPos, particle.Color, scale * 0.8f, 0.4f);
                    BloomRenderer.DrawSimpleBloom(Main.spriteBatch, screenPos, new Color(20, 10, 30), scale * 0.3f, 0.3f);
                    break;

                case ParticleType.Glyph:
                    // Glyph: Mystical, pulsing glow
                    float glyphPulse = 0.6f + MathF.Sin(Main.GlobalTimeWrappedHourly * 2f + particle.PulsePhase) * 0.4f;
                    BloomRenderer.DrawSimpleBloom(Main.spriteBatch, screenPos, particle.Color, scale * glyphPulse, 0.5f);
                    break;

                case ParticleType.Dust:
                default:
                    BloomRenderer.DrawSimpleBloom(Main.spriteBatch, screenPos, particle.Color, scale * 0.5f, 0.4f);
                    break;
            }

            // Add lighting for visible particles
            Lighting.AddLight(screenPos + Main.screenPosition, particle.Color.ToVector3() * scale * 0.3f);
        }

        private void DrawEnvironmentalEffect(EnvironmentalEffect effect, float partialTicks)
        {
            Vector2 screenPos = effect.Position - Main.screenPosition;

            switch (effect.Type)
            {
                case EffectType.AmbientGlow:
                    // Large, soft ambient glow at arena center
                    BloomRenderer.DrawBreathingBloom(Main.spriteBatch, screenPos, effect.Color * 0.3f, effect.Scale, 0.5f);
                    break;

                case EffectType.SmokeHaze:
                    // Subtle dark haze for smoky atmospheres
                    BloomRenderer.DrawSimpleBloom(Main.spriteBatch, screenPos, effect.Color, effect.Scale * 3f, 0.15f);
                    break;

                case EffectType.StarField:
                    // Draw additional cosmic background stars
                    DrawCosmicStars(screenPos, effect.Scale, effect.Color);
                    break;

                case EffectType.Prismatic:
                    // Subtle rainbow ambient glow
                    float hue = (Main.GlobalTimeWrappedHourly * 0.2f) % 1f;
                    Color rainbow = Main.hslToRgb(hue, 0.6f, 0.6f);
                    BloomRenderer.DrawSimpleBloom(Main.spriteBatch, screenPos, rainbow, effect.Scale * 2f, 0.2f);
                    break;
            }
        }

        private void DrawCosmicStars(Vector2 centerScreen, float scale, Color baseColor)
        {
            // Draw distant twinkling stars using seeded random
            Random starRandom = new Random(12345);
            float time = Main.GlobalTimeWrappedHourly;

            for (int i = 0; i < 30; i++)
            {
                float angle = (float)(starRandom.NextDouble() * MathHelper.TwoPi);
                float dist = (float)(starRandom.NextDouble() * 400f + 100f) * scale;
                Vector2 starPos = centerScreen + angle.ToRotationVector2() * dist;

                float twinkle = 0.3f + MathF.Sin(time * 3f + i * 1.5f) * 0.7f;
                float starScale = (float)(starRandom.NextDouble() * 0.1f + 0.05f) * twinkle;

                BloomRenderer.DrawSimpleBloom(Main.spriteBatch, starPos, baseColor, starScale, 0.8f);
            }
        }

        #endregion

        #region Data Structures

        private struct AmbientParticle
        {
            public Vector2 Position;
            public Vector2 PreviousPosition;
            public Vector2 Velocity;
            public Color Color;
            public float Scale;
            public float Rotation;
            public float RotationSpeed;
            public int Lifetime;
            public int MaxLifetime;
            public float ParallaxDepth;
            public ParticleType Type;
            public float PulseSpeed;
            public float PulsePhase;
            public int SeedIndex;
        }

        private struct EnvironmentalEffect
        {
            public EffectType Type;
            public Vector2 Position;
            public Color Color;
            public float Scale;
            public int Duration;
            public int Timer;
        }

        private enum ParticleType
        {
            Glow,
            Ember,
            Spark,
            Petal,
            Feather,
            Crystal,
            Sparkle,
            Prismatic,
            Star,
            Void,
            Glyph,
            Dust
        }

        private enum EffectType
        {
            AmbientGlow,
            SmokeHaze,
            StarField,
            Prismatic
        }

        #endregion
    }
}
