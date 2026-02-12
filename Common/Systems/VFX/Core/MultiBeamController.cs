using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace MagnumOpus.Common.Systems.VFX.Core
{
    /// <summary>
    /// Multi-beam synchronization controller.
    /// Manages multiple beams with coordinated animations, state machines, and smooth transitions.
    /// 
    /// USAGE:
    /// var controller = new MultiBeamController(origin, 5); // 5 beams
    /// controller.StartFiring(targetDirection);
    /// controller.Update(gameTime);
    /// controller.Draw(spriteBatch);
    /// controller.StopFiring();
    /// </summary>
    public class MultiBeamController
    {
        #region Beam State Machine
        
        public enum BeamState
        {
            Idle,
            Charging,
            Firing,
            Dissipating
        }
        
        public class ManagedBeam
        {
            public BeamState State;
            public float StateTime;
            public Vector2 TargetDirection;
            public float IntensityTarget;
            public int BeamIndex;
            
            // Smoothing
            public Vector2 CurrentDirection;
            public float CurrentIntensity;
            public float CurrentLength;
            public float CurrentWidth;
            
            // Visual properties
            public Color BeamColor;
            public float MaxLength;
            public float MaxWidth;
            
            // Origin tracking
            public Vector2 Origin;
        }
        
        #endregion
        
        #region Fields
        
        private List<ManagedBeam> beams;
        private Vector2 origin;
        private int numBeams;
        
        // Configuration
        public float SpreadAngle { get; set; } = 15f;      // Degrees between beams
        public float ChargeTime { get; set; } = 30f;       // Frames
        public float DissipateTime { get; set; } = 20f;    // Frames
        public bool SynchronizeAnimations { get; set; } = true;
        public float BeamLength { get; set; } = 1000f;
        public float BeamWidth { get; set; } = 20f;
        public Color PrimaryColor { get; set; } = Color.Cyan;
        public Color SecondaryColor { get; set; } = Color.White;
        
        // Animation
        private float globalAnimationPhase;
        
        // Beam texture (set externally)
        public Texture2D BeamTexture { get; set; }
        
        #endregion
        
        #region Properties
        
        public IReadOnlyList<ManagedBeam> Beams => beams;
        
        public Vector2 Origin
        {
            get => origin;
            set
            {
                origin = value;
                foreach (var beam in beams)
                    beam.Origin = value;
            }
        }
        
        public bool IsActive => beams.Exists(b => b.State != BeamState.Idle);
        public bool IsFiring => beams.Exists(b => b.State == BeamState.Firing);
        
        #endregion
        
        #region Constructor
        
        public MultiBeamController(Vector2 origin, int numBeams)
        {
            this.origin = origin;
            this.numBeams = numBeams;
            this.beams = new List<ManagedBeam>();
            
            InitializeBeams();
        }
        
        private void InitializeBeams()
        {
            for (int i = 0; i < numBeams; i++)
            {
                float angleOffset = (i - numBeams * 0.5f + 0.5f) * SpreadAngle;
                
                var managedBeam = new ManagedBeam
                {
                    State = BeamState.Idle,
                    StateTime = 0f,
                    BeamIndex = i,
                    CurrentDirection = Vector2.UnitX.RotatedBy(MathHelper.ToRadians(angleOffset)),
                    CurrentIntensity = 0f,
                    CurrentLength = 0f,
                    CurrentWidth = 0f,
                    BeamColor = PrimaryColor,
                    MaxLength = BeamLength,
                    MaxWidth = BeamWidth,
                    Origin = origin
                };
                
                beams.Add(managedBeam);
            }
        }
        
        #endregion
        
        #region Control Methods
        
        /// <summary>
        /// Start firing all beams toward target direction.
        /// </summary>
        public void StartFiring(Vector2 targetDirection)
        {
            foreach (var beam in beams)
            {
                if (beam.State == BeamState.Idle || beam.State == BeamState.Dissipating)
                {
                    beam.State = BeamState.Charging;
                    beam.StateTime = 0f;
                    
                    float angleOffset = (beam.BeamIndex - numBeams * 0.5f + 0.5f) * SpreadAngle;
                    beam.TargetDirection = targetDirection.RotatedBy(MathHelper.ToRadians(angleOffset));
                    beam.IntensityTarget = 1f;
                    beam.MaxLength = BeamLength;
                    beam.MaxWidth = BeamWidth;
                }
            }
        }
        
        /// <summary>
        /// Stop firing (smooth dissipation).
        /// </summary>
        public void StopFiring()
        {
            foreach (var beam in beams)
            {
                if (beam.State == BeamState.Firing || beam.State == BeamState.Charging)
                {
                    beam.State = BeamState.Dissipating;
                    beam.StateTime = 0f;
                }
            }
        }
        
        /// <summary>
        /// Force stop all beams immediately.
        /// </summary>
        public void ForceStop()
        {
            foreach (var beam in beams)
            {
                beam.State = BeamState.Idle;
                beam.StateTime = 0f;
                beam.CurrentIntensity = 0f;
                beam.CurrentLength = 0f;
            }
        }
        
        /// <summary>
        /// Update target direction while firing.
        /// </summary>
        public void UpdateDirection(Vector2 newDirection)
        {
            foreach (var beam in beams)
            {
                float angleOffset = (beam.BeamIndex - numBeams * 0.5f + 0.5f) * SpreadAngle;
                beam.TargetDirection = newDirection.RotatedBy(MathHelper.ToRadians(angleOffset));
            }
        }
        
        /// <summary>
        /// Set beam configuration for a specific beam.
        /// </summary>
        public void SetBeamConfig(int index, float length, float width, Color color)
        {
            if (index >= 0 && index < beams.Count)
            {
                beams[index].MaxLength = length;
                beams[index].MaxWidth = width;
                beams[index].BeamColor = color;
            }
        }
        
        #endregion
        
        #region Update
        
        public void Update()
        {
            globalAnimationPhase += 0.05f;
            
            foreach (var beam in beams)
            {
                beam.StateTime++;
                beam.Origin = origin;
                
                switch (beam.State)
                {
                    case BeamState.Idle:
                        beam.CurrentIntensity = 0f;
                        beam.CurrentLength = 0f;
                        break;
                        
                    case BeamState.Charging:
                        UpdateCharging(beam);
                        break;
                        
                    case BeamState.Firing:
                        UpdateFiring(beam);
                        break;
                        
                    case BeamState.Dissipating:
                        UpdateDissipating(beam);
                        break;
                }
            }
        }
        
        private void UpdateCharging(ManagedBeam beam)
        {
            float chargeProgress = beam.StateTime / ChargeTime;
            
            if (chargeProgress >= 1f)
            {
                beam.State = BeamState.Firing;
                beam.StateTime = 0f;
            }
            
            // Smooth intensity ramp
            beam.CurrentIntensity = EaseOutCubic(chargeProgress);
            
            // Smooth direction transition
            beam.CurrentDirection = Vector2.Lerp(
                beam.CurrentDirection,
                beam.TargetDirection,
                0.2f
            );
            if (beam.CurrentDirection.LengthSquared() > 0.001f)
                beam.CurrentDirection.Normalize();
            
            // Length increases during charge
            beam.CurrentLength = MathHelper.Lerp(0f, beam.MaxLength, EaseOutCubic(chargeProgress));
            beam.CurrentWidth = MathHelper.Lerp(beam.MaxWidth * 0.3f, beam.MaxWidth, chargeProgress);
            
            // Add jitter during charge (lessens as charge completes)
            if (chargeProgress < 0.8f)
            {
                float jitter = (1f - chargeProgress) * 0.1f;
                beam.CurrentDirection += Main.rand.NextVector2Circular(jitter, jitter);
                beam.CurrentDirection.Normalize();
            }
            
            // Color transition
            beam.BeamColor = Color.Lerp(SecondaryColor, PrimaryColor, chargeProgress);
        }
        
        private void UpdateFiring(ManagedBeam beam)
        {
            // Smooth tracking to target direction
            beam.CurrentDirection = Vector2.Lerp(
                beam.CurrentDirection,
                beam.TargetDirection,
                0.15f
            );
            if (beam.CurrentDirection.LengthSquared() > 0.001f)
                beam.CurrentDirection.Normalize();
            
            // Full intensity
            beam.CurrentIntensity = beam.IntensityTarget;
            
            // Add slight wave motion if synchronized
            if (SynchronizeAnimations)
            {
                float wave = (float)Math.Sin(globalAnimationPhase + beam.BeamIndex * 0.5f) * 0.05f;
                float angleOffset = (beam.BeamIndex - numBeams * 0.5f + 0.5f) * SpreadAngle;
                beam.CurrentDirection = beam.TargetDirection.RotatedBy(
                    MathHelper.ToRadians(angleOffset + wave * 5f)
                );
            }
            
            // Full length with slight pulse
            float lengthPulse = 1f + (float)Math.Sin(globalAnimationPhase * 2f) * 0.02f;
            beam.CurrentLength = beam.MaxLength * lengthPulse;
            
            // Width pulse
            float widthPulse = 1f + (float)Math.Sin(globalAnimationPhase * 3f + beam.BeamIndex) * 0.1f;
            beam.CurrentWidth = beam.MaxWidth * widthPulse;
            
            // Color stays at primary
            beam.BeamColor = PrimaryColor;
        }
        
        private void UpdateDissipating(ManagedBeam beam)
        {
            float dissipateProgress = beam.StateTime / DissipateTime;
            
            if (dissipateProgress >= 1f)
            {
                beam.State = BeamState.Idle;
                beam.StateTime = 0f;
                beam.CurrentIntensity = 0f;
                beam.CurrentLength = 0f;
                return;
            }
            
            // Fade out intensity
            beam.CurrentIntensity = MathHelper.Lerp(beam.IntensityTarget, 0f, EaseInCubic(dissipateProgress));
            
            // Shrink length
            beam.CurrentLength = MathHelper.Lerp(beam.MaxLength, 0f, EaseInCubic(dissipateProgress));
            
            // Shrink width
            beam.CurrentWidth = MathHelper.Lerp(beam.MaxWidth, 0f, dissipateProgress);
            
            // Color fades to secondary
            beam.BeamColor = Color.Lerp(PrimaryColor, SecondaryColor, dissipateProgress);
        }
        
        #endregion
        
        #region Draw
        
        public void Draw(SpriteBatch spriteBatch)
        {
            if (BeamTexture == null)
                return;
            
            foreach (var beam in beams)
            {
                if (beam.State != BeamState.Idle && beam.CurrentIntensity > 0.01f)
                {
                    DrawBeam(spriteBatch, beam);
                }
            }
        }
        
        private void DrawBeam(SpriteBatch spriteBatch, ManagedBeam beam)
        {
            Vector2 start = beam.Origin;
            Vector2 end = start + beam.CurrentDirection * beam.CurrentLength;
            
            float rotation = beam.CurrentDirection.ToRotation();
            Color drawColor = beam.BeamColor * beam.CurrentIntensity;
            
            // Calculate draw parameters
            Vector2 scale = new Vector2(
                beam.CurrentLength / BeamTexture.Width,
                beam.CurrentWidth / BeamTexture.Height
            );
            
            // Draw beam body
            spriteBatch.Draw(
                BeamTexture,
                start - Main.screenPosition,
                null,
                drawColor,
                rotation,
                new Vector2(0, BeamTexture.Height * 0.5f),
                scale,
                SpriteEffects.None,
                0f
            );
            
            // Draw glow layer (additive)
            Color glowColor = drawColor * 0.5f;
            glowColor.A = 0;
            
            spriteBatch.Draw(
                BeamTexture,
                start - Main.screenPosition,
                null,
                glowColor,
                rotation,
                new Vector2(0, BeamTexture.Height * 0.5f),
                scale * 1.5f,
                SpriteEffects.None,
                0f
            );
        }
        
        #endregion
        
        #region Easing Functions
        
        private float EaseOutCubic(float t)
        {
            return 1f - (float)Math.Pow(1f - t, 3);
        }
        
        private float EaseInCubic(float t)
        {
            return (float)Math.Pow(t, 3);
        }
        
        private float EaseInOutCubic(float t)
        {
            return t < 0.5f 
                ? 4f * t * t * t 
                : 1f - (float)Math.Pow(-2f * t + 2f, 3) / 2f;
        }
        
        #endregion
    }
    
    #region Converging Beam System
    
    /// <summary>
    /// Specialized beam system where multiple beams converge on a single target point.
    /// </summary>
    public class ConvergingBeamSystem
    {
        private List<ConvergingBeam> beams;
        private Vector2 convergencePoint;
        private bool isConverging;
        
        public class ConvergingBeam
        {
            public Vector2 StartOffset;
            public Vector2 Direction;
            public float Intensity;
            public float Length;
            public Color Color;
        }
        
        public bool IsConverging => isConverging;
        public Vector2 ConvergencePoint => convergencePoint;
        
        public ConvergingBeamSystem()
        {
            beams = new List<ConvergingBeam>();
        }
        
        /// <summary>
        /// Start convergence from multiple origins toward a target.
        /// </summary>
        public void StartConvergence(Vector2 center, Vector2 target, int numBeams, float startRadius)
        {
            beams.Clear();
            convergencePoint = target;
            isConverging = true;
            
            for (int i = 0; i < numBeams; i++)
            {
                float startAngle = (i / (float)numBeams) * MathHelper.TwoPi;
                Vector2 startOffset = startAngle.ToRotationVector2() * startRadius;
                Vector2 startPos = center + startOffset;
                
                Vector2 direction = Vector2.Normalize(target - startPos);
                float length = Vector2.Distance(startPos, target);
                
                beams.Add(new ConvergingBeam
                {
                    StartOffset = startOffset,
                    Direction = direction,
                    Intensity = 1f,
                    Length = length,
                    Color = Color.Cyan
                });
            }
        }
        
        /// <summary>
        /// Update convergence point (for tracking targets).
        /// </summary>
        public void UpdateTarget(Vector2 center, Vector2 newTarget)
        {
            if (!isConverging)
                return;
            
            convergencePoint = newTarget;
            
            foreach (var beam in beams)
            {
                Vector2 startPos = center + beam.StartOffset;
                beam.Direction = Vector2.Normalize(newTarget - startPos);
                beam.Length = Vector2.Distance(startPos, newTarget);
            }
        }
        
        public void Stop()
        {
            isConverging = false;
        }
        
        public void Draw(SpriteBatch spriteBatch, Vector2 center, Texture2D texture)
        {
            if (!isConverging || texture == null)
                return;
            
            foreach (var beam in beams)
            {
                Vector2 start = center + beam.StartOffset;
                float rotation = beam.Direction.ToRotation();
                
                Vector2 scale = new Vector2(
                    beam.Length / texture.Width,
                    20f / texture.Height
                );
                
                spriteBatch.Draw(
                    texture,
                    start - Main.screenPosition,
                    null,
                    beam.Color * beam.Intensity,
                    rotation,
                    new Vector2(0, texture.Height * 0.5f),
                    scale,
                    SpriteEffects.None,
                    0f
                );
            }
        }
    }
    
    #endregion
    
    #region Pulsing Beam Array
    
    /// <summary>
    /// Array of beams with synchronized pulsing effects.
    /// </summary>
    public class PulsingBeamArray
    {
        public class PulsingBeam
        {
            public Vector2 Direction;
            public float Length;
            public float BaseWidth;
            public float CurrentWidth;
            public float Intensity;
            public Color Color;
        }
        
        private List<PulsingBeam> beams;
        private float pulsePhase;
        
        public float PulseFrequency { get; set; } = 2f; // Hz
        public float PulseAmplitude { get; set; } = 0.3f;
        public float WaveOffset { get; set; } = 0.3f; // Phase offset between beams
        
        public PulsingBeamArray()
        {
            beams = new List<PulsingBeam>();
        }
        
        public void AddBeam(Vector2 direction, float length, float width, Color color)
        {
            beams.Add(new PulsingBeam
            {
                Direction = Vector2.Normalize(direction),
                Length = length,
                BaseWidth = width,
                CurrentWidth = width,
                Intensity = 1f,
                Color = color
            });
        }
        
        public void Clear()
        {
            beams.Clear();
        }
        
        public void Update(float deltaTime)
        {
            pulsePhase += deltaTime * PulseFrequency * MathHelper.TwoPi;
            
            for (int i = 0; i < beams.Count; i++)
            {
                var beam = beams[i];
                
                // Offset phase per beam for wave effect
                float beamPhase = pulsePhase + i * WaveOffset;
                
                // Sinusoidal intensity
                float pulse = (float)Math.Sin(beamPhase) * 0.5f + 0.5f; // 0-1
                pulse = pulse * pulse; // Sharper pulse
                
                beam.Intensity = 0.5f + pulse * 0.5f; // Range 0.5-1.0
                
                // Synchronized width pulse
                float widthMod = 1f + pulse * PulseAmplitude;
                beam.CurrentWidth = beam.BaseWidth * widthMod;
            }
        }
        
        public void Draw(SpriteBatch spriteBatch, Vector2 origin, Texture2D texture)
        {
            if (texture == null)
                return;
            
            foreach (var beam in beams)
            {
                float rotation = beam.Direction.ToRotation();
                
                Vector2 scale = new Vector2(
                    beam.Length / texture.Width,
                    beam.CurrentWidth / texture.Height
                );
                
                spriteBatch.Draw(
                    texture,
                    origin - Main.screenPosition,
                    null,
                    beam.Color * beam.Intensity,
                    rotation,
                    new Vector2(0, texture.Height * 0.5f),
                    scale,
                    SpriteEffects.None,
                    0f
                );
            }
        }
    }
    
    #endregion
}
