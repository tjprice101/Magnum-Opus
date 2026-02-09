using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;
using static MagnumOpus.Common.Systems.Particles.Particle;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// EXO BLADE-STYLE CURVED SWING TRAILS
    /// 
    /// Creates interpolated, curved melee swing trails similar to Calamity's Exoblade:
    /// - Catmull-Rom spline interpolation for smooth curves
    /// - Piecewise animation for snappy swing feel (slow start → fast swing → ease out)
    /// - Multi-pass rendering (bloom + main + core)
    /// - Rainbow/theme gradient coloring along the arc
    /// - Music note spawning along the swing path
    /// 
    /// Automatically applies to MagnumOpus melee weapons.
    /// </summary>
    public class ExoBladeSwingTrails : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        
        // Exo Blade swing curve: Slow anticipation → Fast snap → Smooth follow-through
        private static readonly CurveSegment[] SwingCurve = new CurveSegment[]
        {
            new CurveSegment(EasingType.SineBump, 0f, 0f, 0.1f),     // Slow start (0-20%)
            new CurveSegment(EasingType.PolyIn, 0.2f, 0.1f, 0.75f, 3), // Fast swing (20-70%)
            new CurveSegment(EasingType.PolyOut, 0.7f, 0.85f, 0.15f, 2) // Ease out (70-100%)
        };
        
        // Per-projectile data
        private bool _isSwingProjectile;
        private string _theme;
        private Color[] _palette;
        private float _trailWidth;
        private bool _useRainbow;
        
        // Position tracking for curved trail
        private List<SwingPoint> _swingPoints = new List<SwingPoint>();
        private const int MaxSwingPoints = 25;
        private const int MinPointsForTrail = 4;
        
        private struct SwingPoint
        {
            public Vector2 Position;
            public float Rotation;
            public uint TimeStamp;
            public float SwingProgress; // 0-1 through the swing animation
        }
        
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            // MASTER TOGGLE: When disabled, this global system does nothing
            if (!VFXMasterToggle.GlobalSystemsEnabled)
                return false;
            
            if (entity.ModProjectile == null) return false;
            
            // Exclude debug weapons
            if (VFXExclusionHelper.ShouldExcludeProjectile(entity)) return false;
            
            string fullName = entity.ModProjectile.GetType().FullName ?? "";
            if (!fullName.StartsWith("MagnumOpus.")) return false;
            
            // Apply to melee-type projectiles
            return fullName.Contains("Melee") ||
                   fullName.Contains("Sword") ||
                   fullName.Contains("Blade") ||
                   fullName.Contains("Slash") ||
                   fullName.Contains("Swing") ||
                   fullName.Contains("Arc") ||
                   entity.aiStyle == ProjAIStyleID.Spear ||
                   entity.aiStyle == ProjAIStyleID.ShortSword;
        }
        
        public override void SetDefaults(Projectile projectile)
        {
            if (projectile.ModProjectile == null) return;
            
            string fullName = projectile.ModProjectile.GetType().FullName ?? "";
            _theme = DetectTheme(fullName);
            
            if (string.IsNullOrEmpty(_theme))
            {
                _isSwingProjectile = false;
                return;
            }
            
            _isSwingProjectile = true;
            _palette = MagnumThemePalettes.GetThemePalette(_theme) ?? new[] { Color.White };
            _trailWidth = 30f;
            _useRainbow = _theme == "SwanLake"; // Rainbow for Swan Lake
            
            _swingPoints.Clear();
        }
        
        public override void AI(Projectile projectile)
        {
            if (!_isSwingProjectile) return;
            
            // Track blade tip position with eased progress
            TrackSwingPosition(projectile);
            
            // Spawn particles along the swing
            SpawnSwingParticles(projectile);
        }
        
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            if (!_isSwingProjectile || _swingPoints.Count < MinPointsForTrail)
                return true;
            
            DrawCurvedSwingTrail(projectile, Main.spriteBatch);
            
            return true;
        }
        
        public override void OnKill(Projectile projectile, int timeLeft)
        {
            if (_isSwingProjectile)
            {
                _swingPoints.Clear();
            }
        }
        
        #region Position Tracking
        
        private void TrackSwingPosition(Projectile projectile)
        {
            // Calculate blade tip (end of weapon)
            float tipDistance = projectile.width * 0.6f;
            Vector2 tipOffset = projectile.velocity.SafeNormalize(Vector2.Zero) * tipDistance;
            Vector2 tipPosition = projectile.Center + tipOffset;
            
            // Estimate swing progress (0-1) based on projectile lifetime
            float linearProgress = 1f - (float)projectile.timeLeft / projectile.localAI[0].Clamp(1, 1000);
            if (projectile.localAI[0] == 0) linearProgress = 0.5f;
            
            // Apply Exo Blade swing curve for snappy feel
            float easedProgress = GetSwingCurveValue(Math.Clamp(linearProgress, 0f, 1f));
            
            var newPoint = new SwingPoint
            {
                Position = tipPosition,
                Rotation = projectile.rotation,
                TimeStamp = Main.GameUpdateCount,
                SwingProgress = easedProgress
            };
            
            _swingPoints.Add(newPoint);
            
            // Remove old points
            while (_swingPoints.Count > MaxSwingPoints)
            {
                _swingPoints.RemoveAt(0);
            }
            
            // Remove stale points (older than 15 frames)
            while (_swingPoints.Count > 0 && Main.GameUpdateCount - _swingPoints[0].TimeStamp > 15)
            {
                _swingPoints.RemoveAt(0);
            }
        }
        
        /// <summary>
        /// Evaluates the swing curve at a given progress (0-1).
        /// Uses piecewise animation like Calamity's CurveSegment system.
        /// </summary>
        private static float GetSwingCurveValue(float progress)
        {
            float value = 0f;
            
            foreach (var segment in SwingCurve)
            {
                if (progress < segment.StartX)
                    break;
                
                float nextStart = 1f;
                int index = Array.IndexOf(SwingCurve, segment);
                if (index < SwingCurve.Length - 1)
                    nextStart = SwingCurve[index + 1].StartX;
                
                float segmentProgress = (progress - segment.StartX) / (nextStart - segment.StartX);
                segmentProgress = Math.Clamp(segmentProgress, 0f, 1f);
                
                float easedProgress = ApplyEasing(segment.Easing, segmentProgress, segment.Power);
                value = segment.StartY + segment.Lift * easedProgress;
            }
            
            return value;
        }
        
        private static float ApplyEasing(EasingType type, float t, float power)
        {
            return type switch
            {
                EasingType.Linear => t,
                EasingType.SineIn => 1f - MathF.Cos(t * MathHelper.PiOver2),
                EasingType.SineOut => MathF.Sin(t * MathHelper.PiOver2),
                EasingType.SineBump => MathF.Sin(t * MathHelper.Pi),
                EasingType.PolyIn => MathF.Pow(t, power > 0 ? power : 2),
                EasingType.PolyOut => 1f - MathF.Pow(1f - t, power > 0 ? power : 2),
                EasingType.ExpIn => MathF.Pow(2, 10 * (t - 1)),
                EasingType.ExpOut => 1f - MathF.Pow(2, -10 * t),
                EasingType.CircIn => 1f - MathF.Sqrt(1f - t * t),
                EasingType.CircOut => MathF.Sqrt(1f - (t - 1f) * (t - 1f)),
                _ => t
            };
        }
        
        #endregion
        
        #region Trail Rendering
        
        private void DrawCurvedSwingTrail(Projectile projectile, SpriteBatch spriteBatch)
        {
            if (_swingPoints.Count < MinPointsForTrail) return;
            
            // Smooth the positions using Catmull-Rom interpolation
            List<Vector2> smoothedPositions = SmoothWithCatmullRom(_swingPoints, 50);
            if (smoothedPositions.Count < 2) return;
            
            // Convert to array for the renderer
            Vector2[] trailPoints = smoothedPositions.ToArray();
            
            // Multi-pass rendering for bloom effect
            // Pass 1: Outer bloom (large, dim)
            EnhancedTrailRenderer.RenderTrail(trailPoints, new EnhancedTrailRenderer.PrimitiveSettings(
                GetSwingWidth(2.5f),
                GetSwingColor(0.3f),
                null, true, null
            ));
            
            // Pass 2: Main trail
            EnhancedTrailRenderer.RenderTrail(trailPoints, new EnhancedTrailRenderer.PrimitiveSettings(
                GetSwingWidth(1f),
                GetSwingColor(0.9f),
                null, true, null
            ));
            
            // Pass 3: Bright core
            EnhancedTrailRenderer.RenderTrail(trailPoints, new EnhancedTrailRenderer.PrimitiveSettings(
                GetSwingWidth(0.3f),
                _ => Color.White * 0.8f,
                null, true, null
            ));
        }
        
        private EnhancedTrailRenderer.WidthFunction GetSwingWidth(float multiplier)
        {
            return progress =>
            {
                // Quadratic bump: thin at edges, thick in middle
                float bump = MathF.Sin(progress * MathHelper.Pi);
                return _trailWidth * bump * multiplier;
            };
        }
        
        private EnhancedTrailRenderer.ColorFunction GetSwingColor(float opacity)
        {
            return progress =>
            {
                Color color;
                
                if (_useRainbow)
                {
                    // Rainbow gradient along swing (Swan Lake style)
                    float hue = (progress + Main.GameUpdateCount * 0.01f) % 1f;
                    color = Main.hslToRgb(hue, 1f, 0.7f);
                }
                else if (_palette.Length > 1)
                {
                    // Theme gradient
                    color = VFXUtilities.PaletteLerp(_palette, progress);
                }
                else
                {
                    color = _palette[0];
                }
                
                // Fade at the trailing end
                float fade = 1f - progress * 0.5f;
                return color * opacity * fade;
            };
        }
        
        /// <summary>
        /// Smooth a list of swing points using Catmull-Rom spline interpolation.
        /// This creates the fluid, curved trail like Exoblade.
        /// </summary>
        private static List<Vector2> SmoothWithCatmullRom(List<SwingPoint> points, int outputCount)
        {
            if (points.Count < 2)
                return new List<Vector2>();
            
            List<Vector2> positions = new List<Vector2>();
            foreach (var p in points)
                positions.Add(p.Position);
            
            List<Vector2> result = new List<Vector2>();
            
            for (int i = 0; i < outputCount; i++)
            {
                float t = (float)i / (outputCount - 1) * (positions.Count - 1);
                int segment = (int)t;
                float segmentT = t - segment;
                
                int p0 = Math.Max(0, segment - 1);
                int p1 = segment;
                int p2 = Math.Min(positions.Count - 1, segment + 1);
                int p3 = Math.Min(positions.Count - 1, segment + 2);
                
                result.Add(CatmullRom(positions[p0], positions[p1], positions[p2], positions[p3], segmentT));
            }
            
            return result;
        }
        
        private static Vector2 CatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            
            return 0.5f * (
                2f * p1 +
                (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                (-p0 + 3f * p1 - 3f * p2 + p3) * t3
            );
        }
        
        #endregion
        
        #region Particles
        
        private void SpawnSwingParticles(Projectile projectile)
        {
            if (_swingPoints.Count < 2) return;
            
            var latestPoint = _swingPoints[_swingPoints.Count - 1];
            
            // Dense sparkle trail
            if (Main.rand.NextBool(2))
            {
                Vector2 dustPos = latestPoint.Position + Main.rand.NextVector2Circular(8f, 8f);
                Color dustColor = _useRainbow ? 
                    Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.7f) : 
                    _palette[Main.rand.Next(_palette.Length)];
                
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.MagicMirror, 
                    projectile.velocity.RotatedByRandom(0.5f) * 0.3f, 0, dustColor, 1.5f);
                dust.noGravity = true;
                dust.fadeIn = 1.2f;
            }
            
            // Music notes along swing (every 4 frames)
            if (Main.GameUpdateCount % 4 == 0 && Main.rand.NextBool(3))
            {
                Vector2 noteVel = projectile.velocity.RotatedByRandom(0.8f) * 0.2f;
                Color noteColor = _palette.Length > 0 ? _palette[0] : Color.White;
                ThemedParticles.MusicNote(latestPoint.Position, noteVel, noteColor, 0.7f, 25);
            }
            
            // Contrasting sparkle
            if (Main.rand.NextBool(4))
            {
                var sparkle = new SparkleParticle(
                    latestPoint.Position + Main.rand.NextVector2Circular(10f, 10f),
                    Main.rand.NextVector2Circular(2f, 2f),
                    Color.White * 0.8f,
                    0.3f,
                    15
                );
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }
        
        #endregion
        
        #region Theme Detection
        
        private static string DetectTheme(string fullName)
        {
            if (fullName.Contains("LaCampanella")) return "LaCampanella";
            if (fullName.Contains("Eroica")) return "Eroica";
            if (fullName.Contains("SwanLake")) return "SwanLake";
            if (fullName.Contains("MoonlightSonata") || fullName.Contains("Moonlight")) return "MoonlightSonata";
            if (fullName.Contains("EnigmaVariations") || fullName.Contains("Enigma")) return "EnigmaVariations";
            if (fullName.Contains("Fate")) return "Fate";
            if (fullName.Contains("DiesIrae")) return "DiesIrae";
            if (fullName.Contains("ClairDeLune") || fullName.Contains("Clair")) return "ClairDeLune";
            if (fullName.Contains("Nachtmusik")) return "Nachtmusik";
            if (fullName.Contains("OdeToJoy")) return "OdeToJoy";
            if (fullName.Contains("Spring")) return "Spring";
            if (fullName.Contains("Summer")) return "Summer";
            if (fullName.Contains("Autumn")) return "Autumn";
            if (fullName.Contains("Winter")) return "Winter";
            return "";
        }
        
        #endregion
    }
    
    /// <summary>
    /// Extension to clamp floats.
    /// </summary>
    internal static class FloatExtensions
    {
        public static float Clamp(this float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
        }
    }
}
