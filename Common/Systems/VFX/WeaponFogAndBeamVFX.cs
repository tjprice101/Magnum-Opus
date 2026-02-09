using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// WEAPON FOG VFX SYSTEM
    /// 
    /// Creates unique fog/mist effects on weapon attacks per theme.
    /// Uses interpolated rendering for 144Hz+ smoothness.
    /// </summary>
    public static class WeaponFogVFX
    {
        #region Active Fog Instances
        
        private static List<AttackFog> _activeFogs = new List<AttackFog>();
        private const int MaxActiveFogs = 30;
        
        private class AttackFog
        {
            public Vector2 Position;
            public Vector2 PreviousPosition;
            public Vector2 Velocity;
            public int Timer;
            public int MaxLifetime;
            public float Scale;
            public float MaxScale;
            public UniqueWeaponVFXStyles.FogStyle Style;
            public float Rotation;
            public float RotationSpeed;
            public List<FogParticle> Particles;
            public uint SpawnTime; // For unique animation offset
            
            public float Progress => (float)Timer / MaxLifetime;
            public bool IsExpired => Timer >= MaxLifetime;
        }
        
        private struct FogParticle
        {
            public Vector2 LocalOffset;
            public Vector2 BaseOffset; // Original position for morphing
            public float Size;
            public float Alpha;
            public Color Tint;
            public float RotationOffset;
            public float MorphPhase; // Unique phase for each particle's morph animation
            public float TwinklePhase; // For star twinkle effect
            public bool IsTwinkle; // Whether this particle is a star twinkle
        }
        
        #endregion
        
        #region Interpolation Helpers
        
        private static float _partialTicks;
        
        public static void UpdatePartialTicks()
        {
            _partialTicks = Main.GameUpdateCount % 1f;
        }
        
        private static Vector2 Interpolate(Vector2 previous, Vector2 current)
        {
            return Vector2.Lerp(previous, current, _partialTicks);
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Spawns fog effect at position based on theme style.
        /// </summary>
        public static void SpawnAttackFog(Vector2 position, string theme, float scale = 1f, Vector2? velocity = null)
        {
            if (_activeFogs.Count >= MaxActiveFogs)
                _activeFogs.RemoveAt(0);
            
            var style = UniqueWeaponVFXStyles.GetStyle(theme).Fog;
            
            var fog = new AttackFog
            {
                Position = position,
                PreviousPosition = position,
                Velocity = velocity ?? Vector2.Zero,
                Timer = 0,
                MaxLifetime = style.Lifetime,
                Scale = 0f,
                MaxScale = scale,
                Style = style,
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi),
                RotationSpeed = Main.rand.NextFloat(-0.02f, 0.02f) * style.Turbulence,
                Particles = GenerateFogParticles(style, scale),
                SpawnTime = Main.GameUpdateCount
            };
            
            _activeFogs.Add(fog);
        }
        
        /// <summary>
        /// Spawns fog along a swing arc - LAYERED NEBULA STYLE (Ark of the Cosmos quality).
        /// Uses the new LayeredNebulaFog system for buttery smooth, edge-free fog rendering.
        /// </summary>
        public static void SpawnSwingFog(Player player, float swingProgress, string theme, float scale = 1f)
        {
            // Get theme-specific colors
            var style = UniqueWeaponVFXStyles.GetStyle(theme).Fog;
            Color primaryColor = style.PrimaryColor;
            Color secondaryColor = style.SecondaryColor;
            
            // For Fate theme, use BRIGHT cosmic colors (additive blending requires luminous colors!)
            // Dark colors create black blobs with additive blending
            if (theme.Contains("Fate") || theme.Contains("fate"))
            {
                primaryColor = new Color(255, 120, 180);   // Bright pink (NOT dark!)
                secondaryColor = new Color(180, 100, 220); // Bright purple (NOT dark!)
            }
            
            // === USE NEW LAYERED NEBULA SYSTEM ===
            // This creates soft, edge-free fog with proper additive blending
            LayeredNebulaFog.SpawnSwingNebula(player, swingProgress, primaryColor, secondaryColor, scale);
            
            // === ADDITIONAL THEME-SPECIFIC ACCENTS (sparse) ===
            // Only spawn additional particles occasionally for extra flair
            if (swingProgress < 0.15f || swingProgress > 0.85f) return;
            if (Main.GameUpdateCount % 5 != 0) return; // Very sparse
            
            float swingAngle = player.itemRotation + (swingProgress - 0.5f) * 1.5f;
            
            // Occasional soft glow particle
            if (Main.rand.NextBool(3))
            {
                float particleDistance = Main.rand.NextFloat(45f, 85f) * scale;
                Vector2 particlePos = player.Center + (swingAngle + Main.rand.NextFloat(-0.2f, 0.2f)).ToRotationVector2() * particleDistance;
                Vector2 particleVel = swingAngle.ToRotationVector2().RotatedBy(MathHelper.PiOver2 * player.direction) * Main.rand.NextFloat(0.5f, 1.5f);
                
                var glow = new GenericGlowParticle(
                    particlePos,
                    particleVel,
                    Color.Lerp(primaryColor, secondaryColor, Main.rand.NextFloat()) * 0.4f,
                    Main.rand.NextFloat(0.15f, 0.25f),
                    Main.rand.Next(12, 20),
                    true
                );
                MagnumParticleHandler.SpawnParticle(glow);
            }
        }
        
        private static List<FogParticle> GenerateFogParticles(UniqueWeaponVFXStyles.FogStyle style, float scale)
        {
            var particles = new List<FogParticle>();
            
            // === COSMIC NEBULA MESH: Smaller, more translucent cloud pockets ===
            // Use fewer, smaller particles for a wispy mesh effect instead of dense blob
            int cloudCount = (int)(8 + style.Density * 10); // Fewer clouds
            int twinkleCount = (int)(4 + style.SparkleIntensity * 6); // Star twinkles
            
            // Create cloud mesh pockets at clustered positions
            for (int i = 0; i < cloudCount; i++)
            {
                // Create clusters of small clouds rather than uniform distribution
                float clusterAngle = MathHelper.TwoPi * (i / 3) / (cloudCount / 3f); // Group into clusters
                float clusterOffset = Main.rand.NextFloat(-0.5f, 0.5f);
                float angle = clusterAngle + clusterOffset;
                
                // Smaller, more varied distances for wispy pockets
                float dist = Main.rand.NextFloat(8f, 45f) * scale;
                Vector2 baseOffset = angle.ToRotationVector2() * dist;
                
                // BRIGHT pink and purple tones for additive blending (no dark colors!)
                // Dark colors create black blobs with additive blending
                Color cloudTint;
                float colorChoice = Main.rand.NextFloat();
                if (colorChoice < 0.4f)
                {
                    // Bright pink
                    cloudTint = new Color(255, 140, 180);
                }
                else if (colorChoice < 0.8f)
                {
                    // Bright purple
                    cloudTint = new Color(180, 120, 220);
                }
                else
                {
                    // Blend between primary and secondary
                    cloudTint = Color.Lerp(style.PrimaryColor, style.SecondaryColor, Main.rand.NextFloat());
                }
                
                particles.Add(new FogParticle
                {
                    LocalOffset = baseOffset,
                    BaseOffset = baseOffset,
                    Size = Main.rand.NextFloat(0.15f, 0.4f),  // MUCH smaller for wispy effect
                    Alpha = Main.rand.NextFloat(0.15f, 0.35f), // VERY translucent
                    Tint = cloudTint,
                    RotationOffset = Main.rand.NextFloat(MathHelper.TwoPi),
                    MorphPhase = Main.rand.NextFloat(MathHelper.TwoPi), // Unique morph timing
                    TwinklePhase = 0f,
                    IsTwinkle = false
                });
            }
            
            // Add white twinkle star particles
            for (int i = 0; i < twinkleCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float dist = Main.rand.NextFloat(5f, 50f) * scale;
                Vector2 baseOffset = angle.ToRotationVector2() * dist;
                
                particles.Add(new FogParticle
                {
                    LocalOffset = baseOffset,
                    BaseOffset = baseOffset,
                    Size = Main.rand.NextFloat(0.08f, 0.18f),  // Tiny star points
                    Alpha = Main.rand.NextFloat(0.6f, 1.0f), // Bright when visible
                    Tint = Color.White,
                    RotationOffset = Main.rand.NextFloat(MathHelper.TwoPi),
                    MorphPhase = Main.rand.NextFloat(MathHelper.TwoPi),
                    TwinklePhase = Main.rand.NextFloat(MathHelper.TwoPi), // Unique twinkle timing
                    IsTwinkle = true
                });
            }
            
            return particles;
        }
        
        #endregion
        
        #region Update & Render
        
        public static void Update()
        {
            for (int i = _activeFogs.Count - 1; i >= 0; i--)
            {
                var fog = _activeFogs[i];
                
                fog.PreviousPosition = fog.Position;
                fog.Position += fog.Velocity;
                fog.Velocity *= 0.95f;
                fog.Timer++;
                fog.Rotation += fog.RotationSpeed;
                
                // Scale animation
                float progress = fog.Progress;
                if (progress < 0.2f)
                {
                    fog.Scale = fog.MaxScale * EaseOutQuad(progress / 0.2f);
                }
                else if (progress > 0.7f)
                {
                    fog.Scale = fog.MaxScale * (1f - EaseInQuad((progress - 0.7f) / 0.3f));
                }
                else
                {
                    fog.Scale = fog.MaxScale;
                }
                
                // Spawn theme particles
                if (fog.Style.SparkleIntensity > 0 && Main.rand.NextFloat() < fog.Style.SparkleIntensity * 0.15f)
                {
                    Vector2 sparklePos = fog.Position + Main.rand.NextVector2Circular(40f * fog.Scale, 40f * fog.Scale);
                    SpawnFogTypeParticle(sparklePos, fog.Style);
                }
                
                if (fog.IsExpired)
                    _activeFogs.RemoveAt(i);
            }
        }
        
        private static void SpawnFogTypeParticle(Vector2 position, UniqueWeaponVFXStyles.FogStyle style)
        {
            switch (style.Type)
            {
                case UniqueWeaponVFXStyles.FogType.CosmicNebula:
                    // Tiny white twinkle star
                    CustomParticles.GenericFlare(position, Color.White, 0.15f, 10);
                    break;
                case UniqueWeaponVFXStyles.FogType.InfernalSmoke:
                    var ember = new GenericGlowParticle(position, Main.rand.NextVector2Circular(1f, 1f) + new Vector2(0, -1f),
                        new Color(255, 150, 50), 0.2f, 18, true);
                    MagnumParticleHandler.SpawnParticle(ember);
                    break;
                case UniqueWeaponVFXStyles.FogType.CrystallineHaze:
                    float hue = (Main.GameUpdateCount * 0.01f + Main.rand.NextFloat()) % 1f;
                    CustomParticles.PrismaticSparkle(position, Main.hslToRgb(hue, 0.8f, 0.75f), 0.3f);
                    break;
                case UniqueWeaponVFXStyles.FogType.SakuraPetals:
                    CustomParticles.SwanFeatherDrift(position, style.PrimaryColor, 0.25f);
                    break;
                case UniqueWeaponVFXStyles.FogType.FrostCloud:
                    CustomParticles.GenericFlare(position, Color.White, 0.2f, 15);
                    break;
                case UniqueWeaponVFXStyles.FogType.VoidRift:
                    CustomParticles.Glyph(position, style.SecondaryColor, 0.25f, -1);
                    break;
                default:
                    CustomParticles.GenericFlare(position, style.SecondaryColor, 0.2f, 10);
                    break;
            }
        }
        
        public static void Render(SpriteBatch spriteBatch)
        {
            if (_activeFogs.Count == 0) return;
            
            UpdatePartialTicks();
            
            // Use the cloud/smoke texture for proper fog appearance
            Texture2D cloudTex = MagnumTextureRegistry.GetCloudSmoke();
            Texture2D flareTex = MagnumTextureRegistry.GetFlare(); // For twinkle stars
            if (cloudTex == null) return;
            
            // End current batch and switch to alpha blending for translucent clouds
            try { spriteBatch.End(); } catch { }
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            float gameTime = Main.GameUpdateCount * 0.02f; // Slow oscillation time
            
            foreach (var fog in _activeFogs)
            {
                Vector2 drawPos = Interpolate(fog.PreviousPosition, fog.Position) - Main.screenPosition;
                float baseAlpha = 1f - fog.Progress;
                float fogTime = (Main.GameUpdateCount - fog.SpawnTime) * 0.03f; // Time since spawn for animation
                
                foreach (var particle in fog.Particles)
                {
                    // === MORPHING ANIMATION: Particles drift and oscillate ===
                    float morphTime = fogTime + particle.MorphPhase;
                    float morphX = (float)Math.Sin(morphTime * 1.2f) * 8f;
                    float morphY = (float)Math.Cos(morphTime * 0.9f) * 6f;
                    Vector2 morphOffset = new Vector2(morphX, morphY) * fog.Scale;
                    
                    Vector2 animatedOffset = (particle.BaseOffset + morphOffset).RotatedBy(fog.Rotation) * fog.Scale;
                    Vector2 particlePos = drawPos + animatedOffset;
                    
                    if (particle.IsTwinkle)
                    {
                        // === WHITE TWINKLING STARS ===
                        // Fade in and out with different timing for each star
                        float twinkle = (float)Math.Sin(gameTime * 3f + particle.TwinklePhase);
                        float twinkleAlpha = Math.Max(0f, twinkle) * particle.Alpha * baseAlpha;
                        
                        if (twinkleAlpha > 0.05f && flareTex != null)
                        {
                            // Bright white star point
                            Color starColor = Color.White * twinkleAlpha;
                            float starScale = particle.Size * fog.Scale * (0.8f + twinkle * 0.3f);
                            
                            // Core star
                            spriteBatch.Draw(flareTex, particlePos, null, starColor * 0.9f, 
                                particle.RotationOffset, flareTex.Size() / 2f, starScale * 0.5f, SpriteEffects.None, 0f);
                            // Glow halo
                            spriteBatch.Draw(cloudTex, particlePos, null, starColor * 0.4f, 
                                0f, cloudTex.Size() / 2f, starScale * 0.8f, SpriteEffects.None, 0f);
                        }
                    }
                    else
                    {
                        // === COSMIC CLOUD POCKETS ===
                        // Color oscillates between BRIGHT pink and purple (additive blending!)
                        float colorShift = (float)Math.Sin(morphTime * 0.7f + particle.MorphPhase * 2f) * 0.5f + 0.5f;
                        Color brightPink = new Color(255, 140, 180);   // Bright for additive
                        Color brightPurple = new Color(180, 120, 220); // Bright for additive
                        Color shiftedColor = Color.Lerp(brightPink, brightPurple, colorShift);
                        
                        // Mix with particle's original tint for variation
                        Color finalColor = Color.Lerp(particle.Tint, shiftedColor, 0.6f);
                        
                        // Very translucent - use lower alpha for wispy effect
                        float cloudAlpha = baseAlpha * particle.Alpha * 0.5f; // Half the alpha
                        cloudAlpha = Math.Min(cloudAlpha, 0.35f); // Cap at 35% opacity
                        
                        Color drawColor = finalColor * cloudAlpha;
                        float particleRot = fog.Rotation + particle.RotationOffset + (float)Math.Sin(morphTime) * 0.2f;
                        float particleScale = particle.Size * fog.Scale;
                        
                        // Size pulses slightly for organic movement
                        float sizePulse = 1f + (float)Math.Sin(morphTime * 1.5f) * 0.15f;
                        particleScale *= sizePulse;
                        
                        // Single soft cloud layer (not multiple heavy layers)
                        spriteBatch.Draw(cloudTex, particlePos, null, drawColor, particleRot, 
                            cloudTex.Size() / 2f, particleScale, SpriteEffects.None, 0f);
                        
                        // Subtle inner glow for depth
                        Color innerGlow = Color.Lerp(drawColor, Color.White * 0.1f, 0.2f);
                        spriteBatch.Draw(cloudTex, particlePos, null, innerGlow * 0.3f, particleRot, 
                            cloudTex.Size() / 2f, particleScale * 0.5f, SpriteEffects.None, 0f);
                    }
                }
            }
            
            // Restore SpriteBatch to additive blending for other effects
            try { spriteBatch.End(); } catch { }
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        
        #endregion
        
        #region Easing
        
        private static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
        private static float EaseInQuad(float t) => t * t;
        
        #endregion
    }
    
    /// <summary>
    /// LIGHT BEAM IMPACT SYSTEM
    /// 
    /// Creates stretching light beams on tile/enemy impacts with theme-specific styling.
    /// Uses interpolation for smooth 144Hz+ rendering.
    /// </summary>
    public static class LightBeamImpactVFX
    {
        #region Active Beams
        
        private static List<LightBeamBurst> _activeBeams = new List<LightBeamBurst>();
        private const int MaxActiveBeams = 40;
        
        private class LightBeamBurst
        {
            public Vector2 Position;
            public Vector2 PreviousPosition;
            public Vector2 ImpactDirection;
            public int Timer;
            public int MaxLifetime;
            public UniqueWeaponVFXStyles.LightBeamStyle Style;
            public LightBeam[] Beams;
            public float Scale;
            public uint SpawnTime;
            
            public float Progress => (float)Timer / MaxLifetime;
            public bool IsExpired => Timer >= MaxLifetime;
        }
        
        private struct LightBeam
        {
            public float Angle;
            public float CurrentLength;
            public float PreviousLength;
            public float MaxLength;
            public float Width;
            public float ShimmerPhase;
            public Color BeamColor;
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Spawns light beam impact at position.
        /// </summary>
        public static void SpawnImpact(Vector2 position, string theme, float scale = 1f, Vector2? impactDirection = null)
        {
            if (_activeBeams.Count >= MaxActiveBeams)
                _activeBeams.RemoveAt(0);
            
            var style = UniqueWeaponVFXStyles.GetStyle(theme).LightBeams;
            
            var burst = new LightBeamBurst
            {
                Position = position,
                PreviousPosition = position,
                ImpactDirection = impactDirection ?? Vector2.UnitY,
                Timer = 0,
                MaxLifetime = (int)(35 * (1f / style.StretchSpeed)),
                Style = style,
                Beams = GenerateBeams(style, scale, impactDirection ?? Vector2.UnitY),
                Scale = scale,
                SpawnTime = Main.GameUpdateCount
            };
            
            _activeBeams.Add(burst);
            
            // Dynamic lighting
            Lighting.AddLight(position, style.CoreColor.ToVector3() * 1.5f * scale);
            
            // Music notes if enabled
            if (style.IncludeMusicNotes && Main.rand.NextBool(2))
            {
                ThemedParticles.MusicNote(position + Main.rand.NextVector2Circular(20f, 20f), 
                    Main.rand.NextVector2Circular(2f, 2f), style.CoreColor, 0.75f, 25);
            }
        }
        
        /// <summary>
        /// Spawns light beam on tile collision.
        /// </summary>
        public static void SpawnTileImpact(Vector2 position, Vector2 velocity, string theme, float scale = 0.8f)
        {
            Vector2 impactDir = -velocity.SafeNormalize(Vector2.UnitY);
            SpawnImpact(position, theme, scale, impactDir);
            
            // Extra tile dust
            var style = UniqueWeaponVFXStyles.GetStyle(theme).LightBeams;
            for (int i = 0; i < 4; i++)
            {
                Vector2 dustVel = impactDir.RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * Main.rand.NextFloat(2f, 5f);
                Dust d = Dust.NewDustPerfect(position, DustID.MagicMirror, dustVel, 0, style.CoreColor, 0.8f);
                d.noGravity = true;
            }
        }
        
        /// <summary>
        /// Spawns light beam on enemy hit.
        /// </summary>
        public static void SpawnEnemyImpact(Vector2 position, Vector2 hitDirection, string theme, float scale = 1f)
        {
            SpawnImpact(position, theme, scale, hitDirection);
            
            // Extra impact particles
            var style = UniqueWeaponVFXStyles.GetStyle(theme).LightBeams;
            CustomParticles.GenericFlare(position, style.CoreColor, 0.6f * scale, 18);
            CustomParticles.HaloRing(position, style.EdgeColor, 0.4f * scale, 15);
        }
        
        private static LightBeam[] GenerateBeams(UniqueWeaponVFXStyles.LightBeamStyle style, float scale, Vector2 impactDir)
        {
            var beams = new LightBeam[style.RayCount];
            float baseAngle = impactDir.ToRotation();
            
            for (int i = 0; i < style.RayCount; i++)
            {
                float angle;
                
                switch (style.Type)
                {
                    case UniqueWeaponVFXStyles.LightBeamType.Directional:
                        // Spread around impact direction
                        angle = baseAngle + (i - style.RayCount / 2f) * 0.3f;
                        break;
                    case UniqueWeaponVFXStyles.LightBeamType.Crescent:
                        // Arc shape
                        angle = baseAngle - MathHelper.PiOver2 + MathHelper.Pi * i / (style.RayCount - 1);
                        break;
                    case UniqueWeaponVFXStyles.LightBeamType.Constellation:
                    case UniqueWeaponVFXStyles.LightBeamType.Starburst:
                        // Even radial spread with slight randomness
                        angle = MathHelper.TwoPi * i / style.RayCount + Main.rand.NextFloat(-0.1f, 0.1f);
                        break;
                    default:
                        // Radial spread
                        angle = MathHelper.TwoPi * i / style.RayCount;
                        break;
                }
                
                beams[i] = new LightBeam
                {
                    Angle = angle,
                    CurrentLength = 0f,
                    PreviousLength = 0f,
                    MaxLength = style.BaseLength * scale * Main.rand.NextFloat(0.7f, 1.3f),
                    Width = style.BaseWidth * scale * Main.rand.NextFloat(0.8f, 1.2f),
                    ShimmerPhase = Main.rand.NextFloat(MathHelper.TwoPi),
                    BeamColor = Color.Lerp(style.CoreColor, style.EdgeColor, Main.rand.NextFloat())
                };
            }
            
            return beams;
        }
        
        #endregion
        
        #region Update & Render
        
        public static void Update()
        {
            for (int i = _activeBeams.Count - 1; i >= 0; i--)
            {
                var burst = _activeBeams[i];
                burst.Timer++;
                
                float progress = burst.Progress;
                
                // Update each beam
                for (int b = 0; b < burst.Beams.Length; b++)
                {
                    ref var beam = ref burst.Beams[b];
                    beam.PreviousLength = beam.CurrentLength;
                    
                    // Stretch curve: fast extend, slow retract
                    if (progress < 0.25f)
                    {
                        float stretchT = progress / 0.25f;
                        beam.CurrentLength = beam.MaxLength * EaseOutQuart(stretchT);
                    }
                    else
                    {
                        float fadeT = (progress - 0.25f) / 0.75f;
                        beam.CurrentLength = beam.MaxLength * (1f - EaseInQuad(fadeT));
                    }
                }
                
                // Spawn constellation stars for constellation type
                if (burst.Style.Type == UniqueWeaponVFXStyles.LightBeamType.Constellation && Main.rand.NextBool(4))
                {
                    int randomBeam = Main.rand.Next(burst.Beams.Length);
                    Vector2 starPos = burst.Position + burst.Beams[randomBeam].Angle.ToRotationVector2() * burst.Beams[randomBeam].CurrentLength;
                    CustomParticles.GenericFlare(starPos, Color.White, 0.3f, 10);
                }
                
                if (burst.IsExpired)
                    _activeBeams.RemoveAt(i);
            }
        }
        
        public static void Render(SpriteBatch spriteBatch)
        {
            if (_activeBeams.Count == 0) return;
            
            Texture2D beamTex = MagnumTextureRegistry.GetSoftGlow();
            if (beamTex == null) return;
            
            foreach (var burst in _activeBeams)
            {
                Vector2 basePos = burst.Position - Main.screenPosition;
                float alpha = 1f - burst.Progress * 0.7f;
                float shimmerTime = (Main.GameUpdateCount - burst.SpawnTime) * 0.1f;
                
                foreach (var beam in burst.Beams)
                {
                    if (beam.CurrentLength < 1f) continue;
                    
                    // Shimmer effect
                    float shimmer = 1f + MathF.Sin(shimmerTime + beam.ShimmerPhase) * burst.Style.ShimmerIntensity * 0.3f;
                    
                    // Calculate beam drawing
                    Vector2 direction = beam.Angle.ToRotationVector2();
                    
                    // Draw multiple segments for smooth beam
                    int segments = Math.Max(3, (int)(beam.CurrentLength / 15f));
                    for (int s = 0; s < segments; s++)
                    {
                        float segProgress = (float)s / segments;
                        float nextProgress = (float)(s + 1) / segments;
                        
                        Vector2 segStart = basePos + direction * (beam.CurrentLength * segProgress);
                        Vector2 segEnd = basePos + direction * (beam.CurrentLength * nextProgress);
                        Vector2 segCenter = (segStart + segEnd) / 2f;
                        
                        // Tapered width
                        float widthMult = 1f - segProgress * 0.7f;
                        float segWidth = beam.Width * widthMult * shimmer;
                        
                        // Color gradient
                        Color segColor = Color.Lerp(burst.Style.CoreColor, burst.Style.EdgeColor, segProgress);
                        segColor *= alpha;
                        segColor.A = 0;
                        
                        // Draw segment
                        float segLength = Vector2.Distance(segStart, segEnd);
                        float segRotation = (segEnd - segStart).ToRotation();
                        
                        spriteBatch.Draw(beamTex, segCenter, null, segColor * 0.6f, segRotation,
                            beamTex.Size() / 2f, new Vector2(segLength / beamTex.Width * 2f, segWidth / beamTex.Height),
                            SpriteEffects.None, 0f);
                    }
                    
                    // Core flare at base
                    Color coreColor = burst.Style.CoreColor * alpha;
                    coreColor.A = 0;
                    spriteBatch.Draw(beamTex, basePos, null, coreColor * 0.8f, 0f,
                        beamTex.Size() / 2f, 0.4f * burst.Scale * shimmer, SpriteEffects.None, 0f);
                }
            }
        }
        
        #endregion
        
        #region Easing
        
        private static float EaseOutQuart(float t)
        {
            t = 1f - t;
            return 1f - t * t * t * t;
        }
        
        private static float EaseInQuad(float t) => t * t;
        
        #endregion
    }
    
    /// <summary>
    /// UNIQUE PROJECTILE RENDERING
    /// 
    /// Draws projectiles with theme-specific styles using interpolation.
    /// </summary>
    public static class UniqueProjectileRenderVFX
    {
        private static float _partialTicks;
        
        public static void UpdatePartialTicks()
        {
            _partialTicks = Main.GameUpdateCount % 1f;
        }
        
        /// <summary>
        /// Renders a projectile with theme-specific styling.
        /// </summary>
        public static bool RenderProjectile(SpriteBatch spriteBatch, Projectile projectile, string theme, Color lightColor)
        {
            var style = UniqueWeaponVFXStyles.GetStyle(theme).ProjectileRender;
            
            // Interpolated position
            Vector2 prevPos = projectile.oldPos.Length > 0 ? projectile.oldPos[0] : projectile.position;
            Vector2 drawPos = Vector2.Lerp(prevPos, projectile.position, _partialTicks) + projectile.Size / 2f - Main.screenPosition;
            
            Texture2D glowTex = MagnumTextureRegistry.GetSoftGlow();
            Texture2D flareTex = MagnumTextureRegistry.GetEnergyFlare();
            if (glowTex == null) return true;
            
            float time = Main.GameUpdateCount * style.PulseSpeed;
            float pulse = 1f + MathF.Sin(time) * 0.15f;
            float rotation = projectile.rotation + Main.GameUpdateCount * style.RotationSpeed;
            
            // === MULTI-LAYER BLOOM CORE ===
            for (int layer = 0; layer < (int)style.GlowLayers; layer++)
            {
                float layerProgress = layer / style.GlowLayers;
                float layerScale = style.CoreScale * (1.5f - layerProgress * 0.5f) * pulse;
                float layerAlpha = 0.3f + layerProgress * 0.2f;
                
                Color layerColor = Color.Lerp(style.OuterGlow, style.CoreGlow, layerProgress);
                layerColor.A = 0;
                
                spriteBatch.Draw(glowTex, drawPos, null, layerColor * layerAlpha, rotation + layer * 0.2f,
                    glowTex.Size() / 2f, layerScale, SpriteEffects.None, 0f);
            }
            
            // === WHITE CORE ===
            Color coreColor = Color.White;
            coreColor.A = 0;
            spriteBatch.Draw(glowTex, drawPos, null, coreColor * 0.8f, rotation,
                glowTex.Size() / 2f, style.CoreScale * 0.4f * pulse, SpriteEffects.None, 0f);
            
            // === ORBITING ELEMENTS ===
            if (style.HasOrbitingElements && flareTex != null)
            {
                float orbitTime = Main.GameUpdateCount * 0.08f;
                for (int i = 0; i < style.OrbitCount; i++)
                {
                    float orbitAngle = orbitTime + MathHelper.TwoPi * i / style.OrbitCount;
                    Vector2 orbitPos = drawPos + orbitAngle.ToRotationVector2() * style.OrbitRadius;
                    
                    Color orbitColor = Color.Lerp(style.CoreGlow, style.TrailColor, (float)i / style.OrbitCount);
                    orbitColor.A = 0;
                    
                    spriteBatch.Draw(flareTex, orbitPos, null, orbitColor * 0.6f, orbitAngle,
                        flareTex.Size() / 2f, 0.15f * pulse, SpriteEffects.None, 0f);
                }
            }
            
            return false; // We handled rendering
        }
        
        /// <summary>
        /// Renders projectile trail with theme styling.
        /// </summary>
        public static void RenderTrail(SpriteBatch spriteBatch, Projectile projectile, string theme)
        {
            if (projectile.oldPos.Length < 2) return;
            
            var style = UniqueWeaponVFXStyles.GetStyle(theme).Trail;
            Texture2D trailTex = MagnumTextureRegistry.GetSoftGlow();
            if (trailTex == null) return;
            
            int trailLength = Math.Min(style.TrailLength, projectile.oldPos.Length);
            
            for (int i = 0; i < trailLength - 1; i++)
            {
                if (projectile.oldPos[i] == Vector2.Zero || projectile.oldPos[i + 1] == Vector2.Zero)
                    continue;
                
                float progress = (float)i / trailLength;
                
                Vector2 start = projectile.oldPos[i] + projectile.Size / 2f - Main.screenPosition;
                Vector2 end = projectile.oldPos[i + 1] + projectile.Size / 2f - Main.screenPosition;
                Vector2 center = (start + end) / 2f;
                
                float segLength = Vector2.Distance(start, end);
                float segRotation = (end - start).ToRotation();
                float width = MathHelper.Lerp(style.StartWidth, style.EndWidth, progress);
                
                Color segColor = Color.Lerp(style.StartColor, style.EndColor, progress);
                segColor *= 1f - progress * style.FadeSpeed * 10f;
                segColor.A = 0;
                
                // Rainbow shimmer for prismatic trails
                if (style.Type == UniqueWeaponVFXStyles.TrailType.Prismatic)
                {
                    float hue = (Main.GameUpdateCount * 0.02f + progress) % 1f;
                    segColor = Main.hslToRgb(hue, 0.8f, 0.75f);
                    segColor.A = 0;
                    segColor *= 1f - progress;
                }
                
                spriteBatch.Draw(trailTex, center, null, segColor * 0.5f, segRotation,
                    trailTex.Size() / 2f, new Vector2(segLength / trailTex.Width * 1.5f, width / trailTex.Height * 0.5f),
                    SpriteEffects.None, 0f);
            }
        }
    }
}
