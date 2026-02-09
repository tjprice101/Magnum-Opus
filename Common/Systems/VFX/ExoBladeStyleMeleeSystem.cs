using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.DataStructures;
using MagnumOpus.Common.Systems.Particles;
using ReLogic.Content;
using static MagnumOpus.Common.Systems.Particles.Particle;

namespace MagnumOpus.Common.Systems.VFX
{
    #region Custom Vertex Structure
    
    /// <summary>
    /// Custom vertex structure for Exoblade-style primitive rendering.
    /// Includes position, color, texture coordinates, and time-based UV for shader effects.
    /// </summary>
    public struct ExoBladeVertex : IVertexType
    {
        public Vector3 Position;
        public Color Color;
        public Vector2 TexCoord;
        public float Time; // For shader-based animation
        
        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(16, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(24, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 1)
        );
        
        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
        
        public ExoBladeVertex(Vector3 position, Color color, Vector2 texCoord, float time)
        {
            Position = position;
            Color = color;
            TexCoord = texCoord;
            Time = time;
        }
        
        public ExoBladeVertex(Vector2 screenPos, Color color, Vector2 texCoord, float time)
            : this(new Vector3(screenPos, 0), color, texCoord, time) { }
    }
    
    #endregion
    
    #region Easing System
    
    /// <summary>
    /// Easing types for piecewise curve animation.
    /// Matches Calamity's CurveSegment easing system.
    /// </summary>
    public enum ExoEasingType
    {
        Linear,
        SineIn,
        SineOut,
        SineBump,
        PolyIn,
        PolyOut,
        PolyInOut,
        ExpIn,
        ExpOut,
        CircIn,
        CircOut,
        Anticipate, // Pulls back slightly before moving forward
        Overshoot   // Goes past target then settles
    }
    
    /// <summary>
    /// Represents a segment of a piecewise animation curve.
    /// </summary>
    public struct ExoCurveSegment
    {
        public ExoEasingType Easing;
        public float StartX;   // Progress value where this segment starts (0-1)
        public float StartY;   // Output value at segment start
        public float Lift;     // Change in Y over this segment
        public float Power;    // For polynomial easing (2=quadratic, 3=cubic, etc.)
        
        public ExoCurveSegment(ExoEasingType easing, float startX, float startY, float lift, float power = 2f)
        {
            Easing = easing;
            StartX = startX;
            StartY = startY;
            Lift = lift;
            Power = power;
        }
    }
    
    /// <summary>
    /// Piecewise animation utilities for Exoblade-style curves.
    /// </summary>
    public static class ExoPiecewiseAnimation
    {
        /// <summary>
        /// Evaluates a piecewise animation curve at the given progress.
        /// </summary>
        public static float Evaluate(float progress, ExoCurveSegment[] curve)
        {
            if (curve == null || curve.Length == 0)
                return progress;
            
            progress = MathHelper.Clamp(progress, 0f, 0.9999f);
            
            for (int i = 0; i < curve.Length; i++)
            {
                float nextStart = i < curve.Length - 1 ? curve[i + 1].StartX : 1f;
                
                if (progress >= curve[i].StartX && progress < nextStart)
                {
                    float segmentProgress = (progress - curve[i].StartX) / (nextStart - curve[i].StartX);
                    float easedProgress = ApplyEasing(curve[i].Easing, segmentProgress, curve[i].Power);
                    return curve[i].StartY + curve[i].Lift * easedProgress;
                }
            }
            
            // Return last segment's end value
            var last = curve[curve.Length - 1];
            return last.StartY + last.Lift;
        }
        
        private static float ApplyEasing(ExoEasingType type, float t, float power)
        {
            switch (type)
            {
                case ExoEasingType.Linear:
                    return t;
                case ExoEasingType.SineIn:
                    return 1f - MathF.Cos(t * MathHelper.PiOver2);
                case ExoEasingType.SineOut:
                    return MathF.Sin(t * MathHelper.PiOver2);
                case ExoEasingType.SineBump:
                    return MathF.Sin(t * MathHelper.Pi);
                case ExoEasingType.PolyIn:
                    return MathF.Pow(t, power);
                case ExoEasingType.PolyOut:
                    return 1f - MathF.Pow(1f - t, power);
                case ExoEasingType.PolyInOut:
                    return t < 0.5f 
                        ? MathF.Pow(2f * t, power) / 2f 
                        : 1f - MathF.Pow(2f * (1f - t), power) / 2f;
                case ExoEasingType.ExpIn:
                    return MathF.Pow(2f, 10f * (t - 1f));
                case ExoEasingType.ExpOut:
                    return 1f - MathF.Pow(2f, -10f * t);
                case ExoEasingType.CircIn:
                    return 1f - MathF.Sqrt(1f - t * t);
                case ExoEasingType.CircOut:
                    return MathF.Sqrt(1f - (t - 1f) * (t - 1f));
                case ExoEasingType.Anticipate:
                {
                    float s = 1.70158f;
                    return t * t * ((s + 1f) * t - s);
                }
                case ExoEasingType.Overshoot:
                {
                    float s = 1.70158f;
                    float t1 = t - 1f;
                    return t1 * t1 * ((s + 1f) * t1 + s) + 1f;
                }
                default:
                    return t;
            }
        }
    }
    
    #endregion
    
    #region Main System
    
    /// <summary>
    /// EXOBLADE-STYLE MELEE SWING SYSTEM
    /// 
    /// Implements the full Exoblade rendering pipeline:
    /// 1. Custom overhead swing logic with piecewise animation
    /// 2. Glowmask shader overlay with energy pulsing
    /// 3. Dynamic scale overrides (up to 230% for "Big Swing")
    /// 4. Primitive vertex strip trails
    /// 5. Multi-pass bloom rendering
    /// 6. Beam slash generation on hit with alpha erosion
    /// 
    /// Apply to any melee weapon by calling ExoBladeStyleMeleeSystem.ApplyToWeapon()
    /// or let the GlobalItem automatically detect MagnumOpus melee weapons.
    /// </summary>
    public class ExoBladeStyleMeleeSystem : ModSystem
    {
        #region Swing Curves
        
        /// <summary>
        /// Standard Exoblade swing: Anticipation → Fast Snap → Overshoot → Settle
        /// </summary>
        public static readonly ExoCurveSegment[] StandardSwingCurve = new ExoCurveSegment[]
        {
            new ExoCurveSegment(ExoEasingType.SineIn, 0f, 0f, 0.05f),        // 0-15%: Slow anticipation
            new ExoCurveSegment(ExoEasingType.PolyIn, 0.15f, 0.05f, 0.15f, 3), // 15-30%: Building speed
            new ExoCurveSegment(ExoEasingType.Linear, 0.30f, 0.20f, 0.55f),   // 30-60%: FAST snap
            new ExoCurveSegment(ExoEasingType.PolyOut, 0.60f, 0.75f, 0.18f, 2), // 60-80%: Deceleration
            new ExoCurveSegment(ExoEasingType.SineOut, 0.80f, 0.93f, 0.07f)    // 80-100%: Gentle settle
        };
        
        /// <summary>
        /// Heavy/Big Swing: More anticipation, bigger snap, slight overshoot
        /// </summary>
        public static readonly ExoCurveSegment[] HeavySwingCurve = new ExoCurveSegment[]
        {
            new ExoCurveSegment(ExoEasingType.SineIn, 0f, -0.02f, 0.02f),      // 0-20%: Pull back (anticipation)
            new ExoCurveSegment(ExoEasingType.Anticipate, 0.20f, 0f, 0.08f),   // 20-35%: More wind-up
            new ExoCurveSegment(ExoEasingType.PolyIn, 0.35f, 0.08f, 0.20f, 4), // 35-45%: Explosive acceleration
            new ExoCurveSegment(ExoEasingType.Linear, 0.45f, 0.28f, 0.58f),    // 45-70%: MAXIMUM SPEED
            new ExoCurveSegment(ExoEasingType.Overshoot, 0.70f, 0.86f, 0.16f), // 70-90%: Overshoot past target
            new ExoCurveSegment(ExoEasingType.SineOut, 0.90f, 1.02f, -0.02f)   // 90-100%: Settle back
        };
        
        /// <summary>
        /// Quick Slash: Minimal anticipation, very fast, clean finish
        /// </summary>
        public static readonly ExoCurveSegment[] QuickSlashCurve = new ExoCurveSegment[]
        {
            new ExoCurveSegment(ExoEasingType.PolyIn, 0f, 0f, 0.15f, 2),       // 0-20%: Quick start
            new ExoCurveSegment(ExoEasingType.Linear, 0.20f, 0.15f, 0.70f),    // 20-70%: Linear speed
            new ExoCurveSegment(ExoEasingType.PolyOut, 0.70f, 0.85f, 0.15f, 3) // 70-100%: Quick stop
        };
        
        #endregion
        
        #region State Tracking
        
        /// <summary>
        /// Per-player swing state for interpolated rendering.
        /// </summary>
        private static Dictionary<int, ExoSwingState> _playerSwingStates = new Dictionary<int, ExoSwingState>();
        
        /// <summary>
        /// Active beam slashes that need to be rendered.
        /// </summary>
        private static List<ExoBeamSlash> _activeSlashes = new List<ExoBeamSlash>();
        
        public override void Load()
        {
            _playerSwingStates = new Dictionary<int, ExoSwingState>();
            _activeSlashes = new List<ExoBeamSlash>();
        }
        
        public override void Unload()
        {
            _playerSwingStates?.Clear();
            _activeSlashes?.Clear();
            _playerSwingStates = null;
            _activeSlashes = null;
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Starts an Exoblade-style swing for a player.
        /// Call this from UseItem or Shoot when initiating a melee swing.
        /// </summary>
        public static void StartSwing(Player player, Item weapon, ExoSwingConfig config)
        {
            if (Main.dedServ) return;
            
            var state = GetOrCreateState(player);
            state.StartSwing(player, weapon, config);
        }
        
        /// <summary>
        /// Triggers a "Big Swing" with 230% scale boost after a successful dash hit.
        /// </summary>
        public static void TriggerBigSwing(Player player, float scaleMultiplier = 2.3f)
        {
            if (!_playerSwingStates.TryGetValue(player.whoAmI, out var state))
                return;
            
            state.TriggerBigSwing(scaleMultiplier);
        }
        
        /// <summary>
        /// Spawns a beam slash on hit (the "infinitely piercing slash" effect).
        /// </summary>
        public static void SpawnBeamSlash(Vector2 position, Vector2 direction, Color color, Color glowColor,
            float length = 200f, float maxWidth = 60f, int lifetime = 12)
        {
            if (Main.dedServ) return;
            
            _activeSlashes.Add(new ExoBeamSlash(position, direction, color, glowColor, length, maxWidth, lifetime));
        }
        
        /// <summary>
        /// Gets the current swing state for a player.
        /// </summary>
        public static ExoSwingState GetState(Player player)
        {
            _playerSwingStates.TryGetValue(player.whoAmI, out var state);
            return state;
        }
        
        #endregion
        
        #region System Updates
        
        public override void PostUpdatePlayers()
        {
            // Update all player swing states
            foreach (var kvp in _playerSwingStates)
            {
                if (Main.player[kvp.Key].active)
                {
                    kvp.Value.Update(Main.player[kvp.Key]);
                }
            }
            
            // Update beam slashes
            for (int i = _activeSlashes.Count - 1; i >= 0; i--)
            {
                _activeSlashes[i].Update();
                if (_activeSlashes[i].IsDead)
                {
                    _activeSlashes.RemoveAt(i);
                }
            }
        }
        
        #endregion
        
        #region Rendering
        
        /// <summary>
        /// Renders all active beam slashes. Call from a DrawLayer.
        /// </summary>
        public static void RenderBeamSlashes(SpriteBatch spriteBatch)
        {
            if (_activeSlashes == null || _activeSlashes.Count == 0)
                return;
            
            foreach (var slash in _activeSlashes)
            {
                slash.Render(spriteBatch);
            }
        }
        
        /// <summary>
        /// Renders the swing trail for a player. Call from a DrawLayer.
        /// </summary>
        public static void RenderSwingTrail(Player player, SpriteBatch spriteBatch)
        {
            if (!_playerSwingStates.TryGetValue(player.whoAmI, out var state))
                return;
            
            state.RenderTrail(spriteBatch);
        }
        
        #endregion
        
        private static ExoSwingState GetOrCreateState(Player player)
        {
            if (!_playerSwingStates.TryGetValue(player.whoAmI, out var state))
            {
                state = new ExoSwingState();
                _playerSwingStates[player.whoAmI] = state;
            }
            return state;
        }
    }
    
    #endregion
    
    #region Swing Configuration
    
    /// <summary>
    /// Configuration for an Exoblade-style melee swing.
    /// </summary>
    public class ExoSwingConfig
    {
        /// <summary>Primary color for the swing trail.</summary>
        public Color PrimaryColor { get; set; } = Color.White;
        
        /// <summary>Secondary color for gradient effects.</summary>
        public Color SecondaryColor { get; set; } = Color.Gray;
        
        /// <summary>Glowmask tint color (pulsing energy).</summary>
        public Color GlowColor { get; set; } = Color.White;
        
        /// <summary>Theme palette for rainbow/gradient effects.</summary>
        public Color[] Palette { get; set; }
        
        /// <summary>Use rainbow coloring (like Swan Lake).</summary>
        public bool UseRainbow { get; set; } = false;
        
        /// <summary>Theme name for VFX lookup.</summary>
        public string Theme { get; set; } = "generic";
        
        /// <summary>The swing animation curve to use.</summary>
        public ExoCurveSegment[] SwingCurve { get; set; } = ExoBladeStyleMeleeSystem.StandardSwingCurve;
        
        /// <summary>Base width of the swing trail.</summary>
        public float TrailWidth { get; set; } = 35f;
        
        /// <summary>Number of bloom passes for the trail (1-4).</summary>
        public int BloomPasses { get; set; } = 3;
        
        /// <summary>Enable glowmask pulsing effect.</summary>
        public bool EnableGlowmask { get; set; } = true;
        
        /// <summary>Glowmask pulse speed (radians per second).</summary>
        public float GlowPulseSpeed { get; set; } = 6f;
        
        /// <summary>Total swing arc in degrees.</summary>
        public float SwingArcDegrees { get; set; } = 160f;
        
        /// <summary>Enable spawning beam slashes on hit.</summary>
        public bool SpawnBeamSlashesOnHit { get; set; } = true;
        
        /// <summary>Base scale multiplier for the weapon.</summary>
        public float BaseScale { get; set; } = 1f;
        
        /// <summary>Maximum scale during "Big Swing" (after dash).</summary>
        public float MaxBigSwingScale { get; set; } = 2.3f;
        
        /// <summary>
        /// Creates a config from a theme name.
        /// </summary>
        public static ExoSwingConfig FromTheme(string theme)
        {
            var config = new ExoSwingConfig { Theme = theme };
            
            // Get theme palette
            config.Palette = MagnumThemePalettes.GetThemePalette(theme);
            
            if (config.Palette != null && config.Palette.Length >= 2)
            {
                config.PrimaryColor = config.Palette[0];
                config.SecondaryColor = config.Palette[config.Palette.Length - 1];
                config.GlowColor = VFXUtilities.PaletteLerp(config.Palette, 0.5f);
            }
            
            // Theme-specific settings
            switch (theme)
            {
                case "SwanLake":
                    config.UseRainbow = true;
                    config.TrailWidth = 40f;
                    config.BloomPasses = 4;
                    break;
                    
                case "Fate":
                    config.SwingCurve = ExoBladeStyleMeleeSystem.HeavySwingCurve;
                    config.TrailWidth = 45f;
                    config.BloomPasses = 4;
                    config.SpawnBeamSlashesOnHit = true;
                    break;
                    
                case "LaCampanella":
                    config.TrailWidth = 35f;
                    config.BloomPasses = 3;
                    break;
                    
                case "Nachtmusik":
                    config.SwingCurve = ExoBladeStyleMeleeSystem.HeavySwingCurve;
                    config.TrailWidth = 50f;
                    config.BloomPasses = 4;
                    config.MaxBigSwingScale = 2.5f;
                    break;
            }
            
            return config;
        }
    }
    
    #endregion
    
    #region Swing State
    
    /// <summary>
    /// Tracks the state of an Exoblade-style swing for a single player.
    /// </summary>
    public class ExoSwingState
    {
        #region State
        
        public bool IsSwinging { get; private set; }
        public float LinearProgress { get; private set; }
        public float EasedProgress { get; private set; }
        public float CurrentRotation { get; private set; }
        public float CurrentScale { get; private set; } = 1f;
        public Vector2 WeaponTipPosition { get; private set; }
        
        // Previous frame values for interpolation
        private float _prevRotation;
        private float _prevScale;
        private Vector2 _prevTipPosition;
        
        // Swing parameters
        private ExoSwingConfig _config;
        private Item _weapon;
        private int _swingDirection;
        private float _startAngle;
        private float _endAngle;
        private int _swingStartTick;
        private int _swingDuration;
        
        // Big swing boost
        private bool _isBigSwing;
        private float _bigSwingScale;
        private int _bigSwingDecayTicks;
        
        // Trail tracking
        private List<TrailPoint> _trailPoints = new List<TrailPoint>();
        private const int MaxTrailPoints = 30;
        
        private struct TrailPoint
        {
            public Vector2 Position;
            public float Rotation;
            public float Progress;
            public uint Tick;
        }
        
        #endregion
        
        #region Swing Control
        
        public void StartSwing(Player player, Item weapon, ExoSwingConfig config)
        {
            _config = config ?? new ExoSwingConfig();
            _weapon = weapon;
            IsSwinging = true;
            LinearProgress = 0f;
            EasedProgress = 0f;
            _swingStartTick = (int)Main.GameUpdateCount;
            _swingDirection = player.direction;
            
            // Calculate swing arc
            float arcRadians = MathHelper.ToRadians(_config.SwingArcDegrees);
            Vector2 toMouse = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX * player.direction);
            float targetAngle = toMouse.ToRotation();
            
            _startAngle = targetAngle - arcRadians * 0.5f * _swingDirection;
            _endAngle = targetAngle + arcRadians * 0.5f * _swingDirection;
            
            // Set swing duration based on weapon use time
            _swingDuration = Math.Max(weapon?.useAnimation ?? 20, 8);
            
            // Apply base scale
            CurrentScale = _config.BaseScale;
            _prevScale = CurrentScale;
            
            // Clear trail
            _trailPoints.Clear();
        }
        
        public void TriggerBigSwing(float scaleMultiplier)
        {
            if (!IsSwinging) return;
            
            _isBigSwing = true;
            _bigSwingScale = Math.Min(scaleMultiplier, _config?.MaxBigSwingScale ?? 2.3f);
            _bigSwingDecayTicks = 20; // Scale boost decays over 20 ticks
            
            // Use heavy swing curve for big swings
            if (_config != null)
            {
                _config.SwingCurve = ExoBladeStyleMeleeSystem.HeavySwingCurve;
            }
        }
        
        public void EndSwing()
        {
            IsSwinging = false;
            _isBigSwing = false;
            _trailPoints.Clear();
        }
        
        #endregion
        
        #region Update
        
        public void Update(Player player)
        {
            if (!IsSwinging) return;
            
            // Store previous frame values
            _prevRotation = CurrentRotation;
            _prevScale = CurrentScale;
            _prevTipPosition = WeaponTipPosition;
            
            // Calculate progress
            int elapsed = (int)Main.GameUpdateCount - _swingStartTick;
            LinearProgress = Math.Clamp((float)elapsed / _swingDuration, 0f, 1f);
            
            // Apply swing curve
            EasedProgress = ExoPiecewiseAnimation.Evaluate(LinearProgress, 
                _config?.SwingCurve ?? ExoBladeStyleMeleeSystem.StandardSwingCurve);
            
            // Calculate rotation
            CurrentRotation = MathHelper.Lerp(_startAngle, _endAngle, EasedProgress);
            
            // Handle big swing scale
            if (_isBigSwing)
            {
                float scaleProgress = 1f - ((float)_bigSwingDecayTicks / 20f);
                CurrentScale = MathHelper.Lerp(_bigSwingScale, _config?.BaseScale ?? 1f, scaleProgress);
                
                _bigSwingDecayTicks--;
                if (_bigSwingDecayTicks <= 0)
                {
                    _isBigSwing = false;
                    CurrentScale = _config?.BaseScale ?? 1f;
                }
            }
            
            // Calculate weapon tip position
            float weaponLength = (_weapon?.width ?? 32) * CurrentScale * 0.8f;
            WeaponTipPosition = player.MountedCenter + CurrentRotation.ToRotationVector2() * weaponLength;
            
            // Track trail point
            TrackTrailPoint();
            
            // End swing when complete
            if (LinearProgress >= 1f)
            {
                EndSwing();
            }
            
            // Spawn particles
            SpawnSwingParticles(player);
        }
        
        private void TrackTrailPoint()
        {
            var point = new TrailPoint
            {
                Position = WeaponTipPosition,
                Rotation = CurrentRotation,
                Progress = EasedProgress,
                Tick = Main.GameUpdateCount
            };
            
            _trailPoints.Add(point);
            
            // Remove old points
            while (_trailPoints.Count > MaxTrailPoints)
            {
                _trailPoints.RemoveAt(0);
            }
            
            // Remove stale points
            while (_trailPoints.Count > 0 && Main.GameUpdateCount - _trailPoints[0].Tick > 12)
            {
                _trailPoints.RemoveAt(0);
            }
        }
        
        #endregion
        
        #region Particles
        
        private void SpawnSwingParticles(Player player)
        {
            if (_config == null || _trailPoints.Count < 2) return;
            
            // Dense dust trail
            if (Main.rand.NextBool(2))
            {
                Color dustColor = _config.UseRainbow
                    ? Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.7f)
                    : _config.PrimaryColor;
                
                Vector2 dustPos = WeaponTipPosition + Main.rand.NextVector2Circular(10f, 10f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.MagicMirror, 
                    CurrentRotation.ToRotationVector2() * 0.5f, 0, dustColor, 1.6f);
                dust.noGravity = true;
                dust.fadeIn = 1.3f;
            }
            
            // Contrasting sparkle
            if (Main.rand.NextBool(3))
            {
                Dust sparkle = Dust.NewDustPerfect(WeaponTipPosition + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.WhiteTorch, Vector2.Zero, 0, Color.White, 0.9f);
                sparkle.noGravity = true;
            }
            
            // Music notes (scaled 0.7f+ for visibility)
            if (Main.rand.NextBool(6))
            {
                Vector2 noteVel = CurrentRotation.ToRotationVector2().RotatedByRandom(0.5f) * 0.5f;
                Color noteColor = _config.UseRainbow
                    ? Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.75f)
                    : _config.PrimaryColor;
                ThemedParticles.MusicNote(WeaponTipPosition, noteVel, noteColor, 0.75f, 30);
            }
            
            // Flares littering the air
            if (Main.rand.NextBool(2))
            {
                float hue = _config.UseRainbow
                    ? (Main.GlobalTimeWrappedHourly * 0.5f + Main.rand.NextFloat(0.1f)) % 1f
                    : 0f;
                    
                Color flareColor = _config.UseRainbow
                    ? Main.hslToRgb(hue, 1f, 0.8f)
                    : Color.Lerp(_config.PrimaryColor, _config.SecondaryColor, Main.rand.NextFloat());
                    
                CustomParticles.GenericFlare(WeaponTipPosition + Main.rand.NextVector2Circular(12f, 12f), 
                    flareColor, 0.4f, 15);
            }
        }
        
        #endregion
        
        #region Rendering
        
        /// <summary>
        /// Renders the swing trail using multi-pass bloom.
        /// </summary>
        public void RenderTrail(SpriteBatch spriteBatch)
        {
            if (!IsSwinging || _trailPoints.Count < 4 || _config == null)
                return;
            
            // Smooth trail positions using Catmull-Rom
            List<Vector2> smoothedPositions = SmoothTrailPositions(60);
            if (smoothedPositions.Count < 2) return;
            
            Vector2[] trailArray = smoothedPositions.ToArray();
            
            // Multi-pass bloom rendering
            int passes = Math.Clamp(_config.BloomPasses, 1, 4);
            
            for (int pass = passes - 1; pass >= 0; pass--)
            {
                float widthMult = 2.5f - pass * 0.6f;
                float opacityMult = 0.3f + pass * 0.2f;
                
                if (pass == 0)
                {
                    // Core pass (brightest, narrowest)
                    widthMult = 0.4f;
                    opacityMult = 0.85f;
                }
                
                EnhancedTrailRenderer.RenderTrail(trailArray, new EnhancedTrailRenderer.PrimitiveSettings(
                    GetWidthFunction(widthMult),
                    GetColorFunction(opacityMult),
                    null, true, null
                ));
            }
        }
        
        private EnhancedTrailRenderer.WidthFunction GetWidthFunction(float multiplier)
        {
            return progress =>
            {
                // Quadratic bump: thin at edges, thick in middle
                float bump = MathF.Sin(progress * MathHelper.Pi);
                return _config.TrailWidth * bump * multiplier * CurrentScale;
            };
        }
        
        private EnhancedTrailRenderer.ColorFunction GetColorFunction(float opacity)
        {
            return progress =>
            {
                Color color;
                
                if (_config.UseRainbow)
                {
                    float hue = (progress + Main.GlobalTimeWrappedHourly * 0.3f) % 1f;
                    color = Main.hslToRgb(hue, 1f, 0.75f);
                }
                else if (_config.Palette != null && _config.Palette.Length > 1)
                {
                    color = VFXUtilities.PaletteLerp(_config.Palette, progress);
                }
                else
                {
                    color = Color.Lerp(_config.PrimaryColor, _config.SecondaryColor, progress);
                }
                
                // Fade at trailing end
                float fade = 1f - progress * 0.4f;
                
                // Remove alpha for proper additive blending
                return color with { A = 0 } * opacity * fade;
            };
        }
        
        private List<Vector2> SmoothTrailPositions(int outputCount)
        {
            if (_trailPoints.Count < 2)
                return new List<Vector2>();
            
            List<Vector2> positions = new List<Vector2>();
            foreach (var p in _trailPoints)
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
        
        #region Interpolation Helpers
        
        /// <summary>
        /// Gets the interpolated rotation for sub-frame rendering (144Hz+).
        /// </summary>
        public float GetInterpolatedRotation(float frameProgress)
        {
            return MathHelper.Lerp(_prevRotation, CurrentRotation, frameProgress);
        }
        
        /// <summary>
        /// Gets the interpolated scale for sub-frame rendering.
        /// </summary>
        public float GetInterpolatedScale(float frameProgress)
        {
            return MathHelper.Lerp(_prevScale, CurrentScale, frameProgress);
        }
        
        /// <summary>
        /// Gets the interpolated weapon tip position.
        /// </summary>
        public Vector2 GetInterpolatedTipPosition(float frameProgress)
        {
            return Vector2.Lerp(_prevTipPosition, WeaponTipPosition, frameProgress);
        }
        
        #endregion
    }
    
    #endregion
    
    #region Beam Slash
    
    /// <summary>
    /// Represents an "infinitely piercing slash" beam effect.
    /// Uses alpha erosion for dissolve effect.
    /// </summary>
    public class ExoBeamSlash
    {
        public bool IsDead { get; private set; }
        
        private Vector2 _position;
        private Vector2 _direction;
        private Color _color;
        private Color _glowColor;
        private float _length;
        private float _maxWidth;
        private int _lifetime;
        private int _maxLifetime;
        private uint _spawnTick;
        
        // Erosion noise seed
        private float _noiseSeed;
        
        public ExoBeamSlash(Vector2 position, Vector2 direction, Color color, Color glowColor,
            float length, float maxWidth, int lifetime)
        {
            _position = position;
            _direction = direction.SafeNormalize(Vector2.UnitX);
            _color = color;
            _glowColor = glowColor;
            _length = length;
            _maxWidth = maxWidth;
            _lifetime = lifetime;
            _maxLifetime = lifetime;
            _spawnTick = Main.GameUpdateCount;
            _noiseSeed = Main.rand.NextFloat() * 1000f;
        }
        
        public void Update()
        {
            _lifetime--;
            
            if (_lifetime <= 0)
            {
                IsDead = true;
            }
            
            // Spawn particles along the slash
            if (Main.rand.NextBool(3))
            {
                float randomPos = Main.rand.NextFloat();
                Vector2 particlePos = _position + _direction * _length * randomPos;
                particlePos += _direction.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-_maxWidth * 0.3f, _maxWidth * 0.3f);
                
                Dust dust = Dust.NewDustPerfect(particlePos, DustID.WhiteTorch,
                    _direction.RotatedByRandom(0.5f) * Main.rand.NextFloat(1f, 3f), 0, _color, 0.8f);
                dust.noGravity = true;
            }
        }
        
        public void Render(SpriteBatch spriteBatch)
        {
            if (IsDead) return;
            
            float progress = 1f - ((float)_lifetime / _maxLifetime);
            
            // Width function: thin → thick → thin (sword-like profile)
            // Plus erosion based on progress
            float erosionThreshold = progress * 1.2f;
            
            // Generate slash mesh points
            int segments = 20;
            Vector2[] positions = new Vector2[segments];
            float[] widths = new float[segments];
            Color[] colors = new Color[segments];
            
            for (int i = 0; i < segments; i++)
            {
                float t = (float)i / (segments - 1);
                
                // Position along slash
                positions[i] = _position + _direction * _length * t;
                
                // Width: thin at start, max at 40%, thin at end
                float baseWidth = MathF.Sin(t * MathHelper.Pi);
                baseWidth = MathF.Pow(baseWidth, 0.7f); // Sharper profile
                
                // Alpha erosion: use noise to create cracking effect
                float noise = GetErosionNoise(t, Main.GlobalTimeWrappedHourly);
                float erosion = noise < erosionThreshold ? 0f : 1f;
                
                widths[i] = _maxWidth * baseWidth * erosion * (1f - progress * 0.5f);
                
                // Color fades with erosion
                float colorFade = erosion * (1f - progress);
                colors[i] = Color.Lerp(_glowColor, _color, t) * colorFade;
            }
            
            // Render multi-pass
            RenderSlashPass(spriteBatch, positions, widths, colors, 2.0f, 0.3f); // Outer glow
            RenderSlashPass(spriteBatch, positions, widths, colors, 1.0f, 0.7f); // Main
            RenderSlashPass(spriteBatch, positions, widths, colors, 0.3f, 0.9f); // Core (white)
        }
        
        private void RenderSlashPass(SpriteBatch spriteBatch, Vector2[] positions, float[] widths, 
            Color[] colors, float widthMult, float opacityMult)
        {
            // Use EnhancedTrailRenderer for the pass
            EnhancedTrailRenderer.RenderTrail(positions, new EnhancedTrailRenderer.PrimitiveSettings(
                progress => {
                    int index = (int)(progress * (widths.Length - 1));
                    index = Math.Clamp(index, 0, widths.Length - 1);
                    return widths[index] * widthMult;
                },
                progress => {
                    int index = (int)(progress * (colors.Length - 1));
                    index = Math.Clamp(index, 0, colors.Length - 1);
                    
                    Color c = widthMult < 0.5f ? Color.White : colors[index];
                    return c with { A = 0 } * opacityMult;
                },
                null, false, null
            ));
        }
        
        /// <summary>
        /// Generates pseudo-random erosion noise for alpha dissolve effect.
        /// </summary>
        private float GetErosionNoise(float position, float time)
        {
            // Simple noise function using sine waves
            float n1 = MathF.Sin(position * 12.9898f + _noiseSeed) * 43758.5453f;
            n1 = n1 - MathF.Floor(n1);
            
            float n2 = MathF.Sin((position + 0.1f) * 78.233f + _noiseSeed + time) * 43758.5453f;
            n2 = n2 - MathF.Floor(n2);
            
            return (n1 + n2) * 0.5f;
        }
    }
    
    #endregion
    
    #region Draw Layer
    
    /// <summary>
    /// Draw layer for rendering Exoblade-style swing effects.
    /// </summary>
    public class ExoBladeDrawLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.HeldItem);
        
        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            return drawInfo.drawPlayer.active && !drawInfo.drawPlayer.dead;
        }
        
        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            
            // Render swing trail
            ExoBladeStyleMeleeSystem.RenderSwingTrail(player, Main.spriteBatch);
            
            // Render beam slashes
            ExoBladeStyleMeleeSystem.RenderBeamSlashes(Main.spriteBatch);
            
            // Render glowmask overlay if applicable
            var state = ExoBladeStyleMeleeSystem.GetState(player);
            if (state != null && state.IsSwinging)
            {
                RenderGlowmaskOverlay(player, state, ref drawInfo);
            }
        }
        
        private void RenderGlowmaskOverlay(Player player, ExoSwingState state, ref PlayerDrawSet drawInfo)
        {
            // Skip if weapon doesn't have glowmask or config disabled
            Item weapon = player.HeldItem;
            if (weapon == null || weapon.IsAir) return;
            
            // Get weapon texture
            Texture2D weaponTex = TextureAssets.Item[weapon.type].Value;
            if (weaponTex == null) return;
            
            Vector2 drawPos = player.MountedCenter - Main.screenPosition;
            // Use 1f for "current frame" interpolation since tModLoader doesn't have Main.frameProgress
            // The Update() method handles storing previous values each tick for smooth transitions
            float rotation = state.GetInterpolatedRotation(1f);
            float scale = state.GetInterpolatedScale(1f);
            
            // Calculate weapon origin
            Vector2 origin = new Vector2(0, weaponTex.Height);
            if (player.direction == -1)
            {
                origin.X = weaponTex.Width;
            }
            
            // Pulsing glow effect based on GlobalTimeWrappedHourly
            float pulse = 0.7f + MathF.Sin(Main.GlobalTimeWrappedHourly * 6f) * 0.3f;
            
            // Get glow color from config or default
            Color glowColor = Color.White with { A = 0 };
            
            // Draw pulsing glow overlay
            try { Main.spriteBatch.End(); } catch { }
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);
            
            // Multi-layer glow
            for (int i = 0; i < 3; i++)
            {
                float layerScale = scale * (1f + i * 0.08f);
                float layerOpacity = pulse * (0.4f - i * 0.1f);
                
                Main.spriteBatch.Draw(weaponTex, drawPos, null, glowColor * layerOpacity,
                    rotation + MathHelper.PiOver4, origin, layerScale, 
                    player.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
            }
            
            try { Main.spriteBatch.End(); } catch { }
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
    
    #endregion
    
    #region Global Item Integration
    
    /// <summary>
    /// GlobalItem that automatically applies Exoblade-style swing to MagnumOpus melee weapons.
    /// </summary>
    public class ExoBladeGlobalItem : GlobalItem
    {
        public override bool InstancePerEntity => true;
        
        private bool _isExoBladeWeapon;
        
        [CloneByReference]
        private ExoSwingConfig _config;
        
        private string _detectedTheme;
        
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            // MASTER TOGGLE: When disabled, this global system does nothing
            if (!VFXMasterToggle.GlobalSystemsEnabled)
                return false;
            
            if (entity.ModItem == null) return false;
            
            // Exclude debug weapons
            if (VFXExclusionHelper.ShouldExcludeItem(entity)) return false;
            
            string fullName = entity.ModItem.GetType().FullName ?? "";
            if (!fullName.StartsWith("MagnumOpus.")) return false;
            
            // Check if it's a melee weapon
            bool isMelee = entity.DamageType == DamageClass.Melee ||
                           entity.DamageType.Type == DamageClass.Melee.Type;
            
            return isMelee && entity.useStyle == ItemUseStyleID.Swing;
        }
        
        public override void SetDefaults(Item item)
        {
            if (item.ModItem == null) return;
            
            string fullName = item.ModItem.GetType().FullName ?? "";
            _detectedTheme = DetectTheme(fullName);
            
            if (!string.IsNullOrEmpty(_detectedTheme))
            {
                _isExoBladeWeapon = true;
                _config = ExoSwingConfig.FromTheme(_detectedTheme);
            }
        }
        
        public override bool? UseItem(Item item, Player player)
        {
            if (!_isExoBladeWeapon || _config == null) return null;
            
            // Start the Exoblade-style swing
            ExoBladeStyleMeleeSystem.StartSwing(player, item, _config);
            
            return null;
        }
        
        public override void OnHitNPC(Item item, Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!_isExoBladeWeapon || _config == null) return;
            
            // Spawn beam slash on hit
            if (_config.SpawnBeamSlashesOnHit)
            {
                Vector2 direction = (target.Center - player.Center).SafeNormalize(Vector2.UnitX * player.direction);
                
                Color slashColor = _config.UseRainbow
                    ? Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.7f)
                    : _config.PrimaryColor;
                    
                Color glowColor = _config.UseRainbow
                    ? Main.hslToRgb((Main.rand.NextFloat() + 0.5f) % 1f, 1f, 0.85f)
                    : _config.GlowColor;
                
                // Spawn slash at impact point
                ExoBladeStyleMeleeSystem.SpawnBeamSlash(
                    target.Center,
                    direction.RotatedByRandom(0.3f),
                    slashColor,
                    glowColor,
                    length: 180f + Main.rand.NextFloat(40f),
                    maxWidth: 45f + Main.rand.NextFloat(15f),
                    lifetime: 10 + Main.rand.Next(4)
                );
            }
            
            // If we hit during a dash (player velocity > 10), trigger big swing
            if (player.velocity.Length() > 10f)
            {
                ExoBladeStyleMeleeSystem.TriggerBigSwing(player, _config.MaxBigSwingScale);
            }
        }
        
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
            return "generic";
        }
    }
    
    #endregion
}
