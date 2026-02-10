using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// BÉZIER WEAPON TRAILS - Exo Blade / Exo Weapons Style
    /// 
    /// This system creates smooth, curved weapon trails using Bézier curve interpolation.
    /// Inspired by Calamity's Ark of the Cosmos and Exo Blade weapons, these trails
    /// follow graceful arcs and curves with multi-layer rendering for maximum visual impact.
    /// 
    /// Features:
    /// - Quadratic and Cubic Bézier curves for smooth paths
    /// - Sub-pixel interpolation for 144Hz+ smoothness
    /// - Multi-pass rendering with bloom layers
    /// - Theme-based color gradients along curves
    /// - Particle spawning along curve paths
    /// </summary>
    public static class BezierWeaponTrails
    {
        #region Constants
        
        private const int DefaultSegmentCount = 24;        // Curve resolution
        private const int DefaultTrailLength = 16;         // Trail position history
        private const float DefaultWidthBase = 12f;        // Base trail width
        private const float DefaultBloomIntensity = 0.85f; // Bloom brightness
        
        #endregion
        
        #region Trail State Tracking
        
        /// <summary>
        /// Tracked weapon trail state for smooth Bézier rendering.
        /// </summary>
        public class WeaponTrailState
        {
            public Vector2[] PositionHistory;
            public float[] RotationHistory;
            public int HistoryIndex;
            public int HistoryLength;
            public float SwingProgress;
            public Vector2 ControlPointOffset;
            
            public WeaponTrailState(int length = DefaultTrailLength)
            {
                HistoryLength = length;
                PositionHistory = new Vector2[length];
                RotationHistory = new float[length];
                HistoryIndex = 0;
                SwingProgress = 0f;
                ControlPointOffset = Vector2.Zero;
            }
            
            public void RecordPosition(Vector2 position, float rotation)
            {
                PositionHistory[HistoryIndex] = position;
                RotationHistory[HistoryIndex] = rotation;
                HistoryIndex = (HistoryIndex + 1) % HistoryLength;
            }
            
            public Vector2 GetPosition(int framesBack)
            {
                int index = (HistoryIndex - 1 - framesBack + HistoryLength * 10) % HistoryLength;
                return PositionHistory[index];
            }
            
            public float GetRotation(int framesBack)
            {
                int index = (HistoryIndex - 1 - framesBack + HistoryLength * 10) % HistoryLength;
                return RotationHistory[index];
            }
        }
        
        // Trail state storage per entity
        private static Dictionary<int, WeaponTrailState> _playerTrails = new Dictionary<int, WeaponTrailState>();
        private static Dictionary<int, WeaponTrailState> _projectileTrails = new Dictionary<int, WeaponTrailState>();
        
        public static WeaponTrailState GetOrCreatePlayerTrail(int playerId)
        {
            if (!_playerTrails.TryGetValue(playerId, out var state))
            {
                state = new WeaponTrailState();
                _playerTrails[playerId] = state;
            }
            return state;
        }
        
        public static WeaponTrailState GetOrCreateProjectileTrail(int projId)
        {
            if (!_projectileTrails.TryGetValue(projId, out var state))
            {
                state = new WeaponTrailState();
                _projectileTrails[projId] = state;
            }
            return state;
        }
        
        public static void ClearProjectileTrail(int projId)
        {
            _projectileTrails.Remove(projId);
        }
        
        #endregion
        
        #region Bézier Curve Math
        
        /// <summary>
        /// Quadratic Bézier curve: B(t) = (1-t)²P0 + 2(1-t)tP1 + t²P2
        /// </summary>
        public static Vector2 QuadraticBezier(Vector2 p0, Vector2 p1, Vector2 p2, float t)
        {
            float u = 1f - t;
            return u * u * p0 + 2f * u * t * p1 + t * t * p2;
        }
        
        /// <summary>
        /// Cubic Bézier curve: B(t) = (1-t)³P0 + 3(1-t)²tP1 + 3(1-t)t²P2 + t³P3
        /// </summary>
        public static Vector2 CubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float u = 1f - t;
            float u2 = u * u;
            float u3 = u2 * u;
            float t2 = t * t;
            float t3 = t2 * t;
            return u3 * p0 + 3f * u2 * t * p1 + 3f * u * t2 * p2 + t3 * p3;
        }
        
        /// <summary>
        /// Tangent of quadratic Bézier at point t.
        /// </summary>
        public static Vector2 QuadraticBezierTangent(Vector2 p0, Vector2 p1, Vector2 p2, float t)
        {
            return 2f * (1f - t) * (p1 - p0) + 2f * t * (p2 - p1);
        }
        
        /// <summary>
        /// Tangent of cubic Bézier at point t.
        /// </summary>
        public static Vector2 CubicBezierTangent(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float u = 1f - t;
            return 3f * u * u * (p1 - p0) + 6f * u * t * (p2 - p1) + 3f * t * t * (p3 - p2);
        }
        
        /// <summary>
        /// Generate a smooth arc for melee swings using cubic Bézier.
        /// Control points are calculated to create graceful curved slashes.
        /// </summary>
        public static Vector2[] GenerateSwingArc(Vector2 playerCenter, float swingStart, float swingEnd, float swingRadius, int segments = DefaultSegmentCount)
        {
            Vector2[] arc = new Vector2[segments + 1];
            
            // Control points for the swing arc
            Vector2 startPoint = playerCenter + swingStart.ToRotationVector2() * swingRadius;
            Vector2 endPoint = playerCenter + swingEnd.ToRotationVector2() * swingRadius;
            
            // Calculate perpendicular control point for curve shape
            float midAngle = (swingStart + swingEnd) / 2f;
            float arcSpread = Math.Abs(MathHelper.WrapAngle(swingEnd - swingStart));
            float controlOffset = swingRadius * (0.5f + arcSpread * 0.25f);
            
            Vector2 controlPoint = playerCenter + midAngle.ToRotationVector2() * controlOffset;
            
            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                arc[i] = QuadraticBezier(startPoint, controlPoint, endPoint, t);
            }
            
            return arc;
        }
        
        /// <summary>
        /// Generate a flowing projectile trail using the position history.
        /// </summary>
        public static Vector2[] GenerateFlowingTrail(WeaponTrailState state, int usedPoints = -1)
        {
            if (usedPoints < 0) usedPoints = state.HistoryLength;
            usedPoints = Math.Min(usedPoints, state.HistoryLength);
            
            if (usedPoints < 3) return new Vector2[0];
            
            List<Vector2> trail = new List<Vector2>();
            
            // Use Catmull-Rom interpolation for smooth curve through points
            for (int i = 0; i < usedPoints - 1; i++)
            {
                Vector2 p0 = state.GetPosition(Math.Max(0, i - 1));
                Vector2 p1 = state.GetPosition(i);
                Vector2 p2 = state.GetPosition(Math.Min(usedPoints - 1, i + 1));
                Vector2 p3 = state.GetPosition(Math.Min(usedPoints - 1, i + 2));
                
                // Skip invalid positions
                if (p1 == Vector2.Zero || p2 == Vector2.Zero) continue;
                
                // Catmull-Rom to Bézier conversion
                Vector2 cp1 = p1 + (p2 - p0) / 6f;
                Vector2 cp2 = p2 - (p3 - p1) / 6f;
                
                // Generate curve segment
                int subSegments = 4;
                for (int j = 0; j < subSegments; j++)
                {
                    float t = (float)j / subSegments;
                    trail.Add(CubicBezier(p1, cp1, cp2, p2, t));
                }
            }
            
            if (trail.Count > 0)
                trail.Add(state.GetPosition(usedPoints - 1));
            
            return trail.ToArray();
        }
        
        /// <summary>
        /// Generate a flowing trail directly from a Vector2[] array (for projectile oldPos).
        /// This is a convenience overload for when you don't have a WeaponTrailState.
        /// </summary>
        public static Vector2[] GenerateFlowingTrail(Vector2[] positions, int segmentsPerCurve = 4)
        {
            if (positions == null || positions.Length < 3) return new Vector2[0];
            
            List<Vector2> trail = new List<Vector2>();
            int usedPoints = positions.Length;
            
            // Use Catmull-Rom interpolation for smooth curve through points
            for (int i = 0; i < usedPoints - 1; i++)
            {
                Vector2 p0 = positions[Math.Max(0, i - 1)];
                Vector2 p1 = positions[i];
                Vector2 p2 = positions[Math.Min(usedPoints - 1, i + 1)];
                Vector2 p3 = positions[Math.Min(usedPoints - 1, i + 2)];
                
                // Skip invalid positions
                if (p1 == Vector2.Zero || p2 == Vector2.Zero) continue;
                
                // Catmull-Rom to Bézier conversion
                Vector2 cp1 = p1 + (p2 - p0) / 6f;
                Vector2 cp2 = p2 - (p3 - p1) / 6f;
                
                // Generate curve segment
                for (int j = 0; j < segmentsPerCurve; j++)
                {
                    float t = (float)j / segmentsPerCurve;
                    trail.Add(CubicBezier(p1, cp1, cp2, p2, t));
                }
            }
            
            if (trail.Count > 0 && positions.Length > 0)
                trail.Add(positions[usedPoints - 1]);
            
            return trail.ToArray();
        }
        
        #endregion
        
        #region Trail Rendering
        
        /// <summary>
        /// Renders a smooth Bézier-curved melee swing trail.
        /// Uses multi-pass rendering for bloom effect.
        /// </summary>
        public static void RenderMeleeSwingTrail(
            SpriteBatch spriteBatch,
            Player player,
            float swingProgress,
            float swingStartAngle,
            float swingEndAngle,
            float swingRadius,
            Color[] palette,
            float widthMult = 1f)
        {
            if (palette == null || palette.Length == 0)
                palette = new[] { Color.White };
            
            // Generate the swing arc
            float currentAngle = MathHelper.Lerp(swingStartAngle, swingEndAngle, swingProgress);
            Vector2[] arc = GenerateSwingArc(player.Center, swingStartAngle, currentAngle, swingRadius);
            
            if (arc.Length < 2) return;
            
            // Width function: tapered at ends, thick in middle
            Func<float, float> widthFunc = t =>
            {
                float bump = VFXUtilities.QuadraticBump(t);
                return DefaultWidthBase * widthMult * bump;
            };
            
            // Color function: gradient along trail
            Func<float, Color> colorFunc = t =>
            {
                Color baseColor = VFXUtilities.PaletteLerp(palette, t);
                float opacity = VFXUtilities.QuadraticBump(t) * 0.9f;
                return baseColor.WithoutAlpha() * opacity;
            };
            
            // Multi-pass rendering for bloom
            RenderMultiPassTrail(spriteBatch, arc, widthFunc, colorFunc, palette);
        }
        
        /// <summary>
        /// Renders a flowing projectile Bézier trail.
        /// </summary>
        public static void RenderProjectileTrail(
            SpriteBatch spriteBatch,
            Projectile projectile,
            Color[] palette,
            float widthMult = 1f)
        {
            var state = GetOrCreateProjectileTrail(projectile.whoAmI);
            state.RecordPosition(projectile.Center, projectile.rotation);
            
            Vector2[] trail = GenerateFlowingTrail(state);
            if (trail.Length < 2) return;
            
            // Width function: narrow at tail, wide at head
            Func<float, float> widthFunc = t =>
            {
                return DefaultWidthBase * widthMult * t * (2f - t);
            };
            
            // Color function: fade at tail
            Func<float, Color> colorFunc = t =>
            {
                Color baseColor = VFXUtilities.PaletteLerp(palette, t);
                float opacity = t * 0.85f;
                return baseColor.WithoutAlpha() * opacity;
            };
            
            RenderMultiPassTrail(spriteBatch, trail, widthFunc, colorFunc, palette);
        }
        
        /// <summary>
        /// Multi-pass trail rendering with bloom layers.
        /// Pass 1: Outer bloom (wide, dim)
        /// Pass 2: Main trail body
        /// Pass 3: Inner core (narrow, bright)
        /// </summary>
        private static void RenderMultiPassTrail(
            SpriteBatch spriteBatch,
            Vector2[] points,
            Func<float, float> widthFunc,
            Func<float, Color> colorFunc,
            Color[] palette)
        {
            if (points.Length < 2) return;
            
            try
            {
                // Save current blend state
                var prevBlendState = Main.instance.GraphicsDevice.BlendState;
                
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                
                // Get pixel texture for trail rendering
                Texture2D pixel = GetPixelTexture();
                
                // === PASS 1: OUTER BLOOM ===
                for (int i = 0; i < points.Length - 1; i++)
                {
                    float t = (float)i / (points.Length - 1);
                    RenderTrailSegment(spriteBatch, pixel, points[i], points[i + 1],
                        widthFunc(t) * 2.5f, colorFunc(t) * 0.3f);
                }
                
                // === PASS 2: MAIN TRAIL ===
                for (int i = 0; i < points.Length - 1; i++)
                {
                    float t = (float)i / (points.Length - 1);
                    RenderTrailSegment(spriteBatch, pixel, points[i], points[i + 1],
                        widthFunc(t) * 1.2f, colorFunc(t) * 0.7f);
                }
                
                // === PASS 3: INNER CORE ===
                for (int i = 0; i < points.Length - 1; i++)
                {
                    float t = (float)i / (points.Length - 1);
                    Color coreColor = Color.Lerp(colorFunc(t), Color.White, 0.4f);
                    RenderTrailSegment(spriteBatch, pixel, points[i], points[i + 1],
                        widthFunc(t) * 0.5f, coreColor * 0.9f);
                }
                
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, prevBlendState, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            catch (Exception)
            {
                // Fallback if SpriteBatch state is invalid
            }
        }
        
        /// <summary>
        /// Renders a single trail segment between two points.
        /// </summary>
        private static void RenderTrailSegment(SpriteBatch spriteBatch, Texture2D pixel, Vector2 start, Vector2 end, float width, Color color)
        {
            Vector2 screenStart = start - Main.screenPosition;
            Vector2 screenEnd = end - Main.screenPosition;
            
            Vector2 edge = screenEnd - screenStart;
            float length = edge.Length();
            if (length < 0.5f) return;
            
            float rotation = edge.ToRotation();
            Vector2 origin = new Vector2(0f, 0.5f);
            Vector2 scale = new Vector2(length, width);
            
            spriteBatch.Draw(pixel, screenStart, null, color, rotation, origin, scale, SpriteEffects.None, 0f);
        }
        
        /// <summary>
        /// Gets or creates a 1x1 white pixel texture for trail rendering.
        /// </summary>
        private static Texture2D _pixelTexture;
        private static Texture2D GetPixelTexture()
        {
            if (_pixelTexture == null || _pixelTexture.IsDisposed)
            {
                _pixelTexture = new Texture2D(Main.instance.GraphicsDevice, 1, 1);
                _pixelTexture.SetData(new[] { Color.White });
            }
            return _pixelTexture;
        }
        
        #endregion
        
        #region Particle Trail Spawning
        
        /// <summary>
        /// Spawns particles along a Bézier curve path.
        /// Creates flowing particle rivers that follow the weapon's arc.
        /// </summary>
        public static void SpawnParticlesAlongCurve(Vector2[] curvePoints, Color[] palette, string theme, float density = 0.3f)
        {
            if (curvePoints == null || curvePoints.Length < 2) return;
            if (palette == null || palette.Length == 0) palette = new[] { Color.White };
            
            for (int i = 0; i < curvePoints.Length - 1; i++)
            {
                if (Main.rand.NextFloat() > density) continue;
                
                Vector2 point = curvePoints[i];
                Vector2 nextPoint = curvePoints[i + 1];
                Vector2 direction = (nextPoint - point).SafeNormalize(Vector2.Zero);
                
                float t = (float)i / curvePoints.Length;
                Color color = VFXUtilities.PaletteLerp(palette, t);
                
                // Perpendicular offset for spread
                Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
                Vector2 offset = perpendicular * Main.rand.NextFloat(-8f, 8f);
                
                Vector2 spawnPos = point + offset;
                Vector2 velocity = direction * Main.rand.NextFloat(0.5f, 2f) + perpendicular * Main.rand.NextFloat(-1f, 1f);
                
                // Spawn theme-appropriate particle
                UniqueTrailStyles.SpawnUniqueTrail(spawnPos, velocity, theme, DamageClass.Melee, palette);
            }
        }
        
        /// <summary>
        /// Creates an "afterimage cascade" effect - multiple fading copies of the swing arc.
        /// Inspired by Calamity's Exo Blade afterimages.
        /// </summary>
        public static void SpawnAfterImageCascade(
            SpriteBatch spriteBatch,
            Player player,
            float currentProgress,
            float swingStartAngle,
            float swingEndAngle,
            float swingRadius,
            Color[] palette,
            int imageCount = 5)
        {
            for (int i = 0; i < imageCount; i++)
            {
                // Each afterimage is slightly behind in the swing
                float delay = (i + 1) * 0.08f;
                float imageProgress = Math.Max(0f, currentProgress - delay);
                
                if (imageProgress <= 0f) continue;
                
                // Fade based on delay
                float alpha = 1f - (delay / (imageCount * 0.08f));
                float widthMult = 1f - delay;
                
                // Tint palette for afterimage
                Color[] fadedPalette = new Color[palette.Length];
                for (int c = 0; c < palette.Length; c++)
                {
                    fadedPalette[c] = palette[c] * alpha;
                }
                
                RenderMeleeSwingTrail(spriteBatch, player, imageProgress,
                    swingStartAngle, swingEndAngle, swingRadius, fadedPalette, widthMult);
            }
        }
        
        #endregion
        
        #region Specialized Trail Effects
        
        /// <summary>
        /// Creates a spiraling trail effect around a central point.
        /// Great for charged attacks and special abilities.
        /// </summary>
        public static void SpawnSpiralTrail(Vector2 center, Color[] palette, float radius, float progress, string theme)
        {
            int spiralArms = 3;
            int pointsPerArm = 12;
            float time = Main.GameUpdateCount * 0.08f;
            
            for (int arm = 0; arm < spiralArms; arm++)
            {
                float armOffset = MathHelper.TwoPi * arm / spiralArms;
                
                for (int p = 0; p < pointsPerArm; p++)
                {
                    float t = (float)p / pointsPerArm;
                    float angle = time + armOffset + t * MathHelper.TwoPi * 2f;
                    float currentRadius = radius * t * progress;
                    
                    Vector2 point = center + angle.ToRotationVector2() * currentRadius;
                    Vector2 velocity = angle.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * 2f;
                    
                    Color color = VFXUtilities.PaletteLerp(palette, t);
                    
                    if (Main.rand.NextBool(3))
                    {
                        var glow = new GenericGlowParticle(point, velocity * 0.5f, color * 0.7f, 0.25f, 15, true);
                        MagnumParticleHandler.SpawnParticle(glow);
                    }
                }
            }
        }
        
        /// <summary>
        /// Creates a constellation trail - connected star points along the path.
        /// Perfect for Fate theme weapons.
        /// </summary>
        public static void SpawnConstellationTrail(Vector2[] points, Color[] palette)
        {
            if (points == null || points.Length < 2) return;
            
            // Spawn star points at intervals
            for (int i = 0; i < points.Length; i += 3)
            {
                if (i >= points.Length) break;
                
                float t = (float)i / points.Length;
                Color starColor = VFXUtilities.PaletteLerp(palette, t);
                
                // Main star flare
                CustomParticles.GenericFlare(points[i], starColor, 0.35f, 20);
                
                // Connecting line to next star (drawn as particles)
                if (i + 3 < points.Length)
                {
                    Vector2 direction = (points[i + 3] - points[i]).SafeNormalize(Vector2.Zero);
                    float distance = (points[i + 3] - points[i]).Length();
                    
                    for (float d = 0; d < distance; d += 8f)
                    {
                        Vector2 linePoint = points[i] + direction * d;
                        if (Main.rand.NextBool(4))
                        {
                            CustomParticles.GenericFlare(linePoint, starColor * 0.4f, 0.15f, 10);
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Creates a snaking S-curve trail for projectiles.
        /// The trail weaves back and forth like a serpent.
        /// </summary>
        public static void SpawnSnakingTrail(Projectile projectile, Color[] palette, float amplitude = 10f)
        {
            var state = GetOrCreateProjectileTrail(projectile.whoAmI);
            
            // Calculate sine wave offset perpendicular to velocity
            float time = projectile.timeLeft * 0.15f;
            float wave = (float)Math.Sin(time) * amplitude;
            
            Vector2 perpendicular = projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2);
            Vector2 waveOffset = perpendicular * wave;
            
            // Record the offset position
            state.RecordPosition(projectile.Center + waveOffset * 0.5f, projectile.rotation);
            
            // Spawn particles with snake motion
            if (Main.rand.NextBool(2))
            {
                Vector2 spawnPos = projectile.Center + waveOffset;
                Vector2 velocity = -projectile.velocity * 0.1f + perpendicular * Main.rand.NextFloat(-1f, 1f);
                
                Color color = VFXUtilities.PaletteLerp(palette, (float)(projectile.timeLeft % 60) / 60f);
                
                var glow = new GenericGlowParticle(spawnPos, velocity, color * 0.7f, 0.28f, 20, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
        }
        
        #endregion
    }
}
