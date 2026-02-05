using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MagnumOpus.Common.Systems.Particles
{
    /// <summary>
    /// Base class for all custom particles in MagnumOpus.
    /// Inspired by Calamity Mod's particle system for high-performance visual effects.
    /// </summary>
    public class Particle
    {
        /// <summary>
        /// The ID of the particle inside the particle handler's array. Set automatically.
        /// </summary>
        public int ID;

        /// <summary>
        /// The ID of the particle type as registered by the handler. Set automatically.
        /// </summary>
        public int Type;

        /// <summary>
        /// The amount of frames this particle has existed for.
        /// </summary>
        public int Time;

        /// <summary>
        /// The position of the particle in world coordinates.
        /// </summary>
        public Vector2 Position;

        /// <summary>
        /// The velocity of the particle.
        /// </summary>
        public Vector2 Velocity;

        /// <summary>
        /// The rotation of the particle in radians.
        /// </summary>
        public float Rotation;

        /// <summary>
        /// The scale of the particle.
        /// </summary>
        public float Scale = 1f;

        /// <summary>
        /// The color of the particle.
        /// </summary>
        public Color Color = Color.White;

        /// <summary>
        /// The maximum lifetime of the particle in frames.
        /// </summary>
        public int Lifetime;

        /// <summary>
        /// The variant frame to use for particles with multiple frame variants.
        /// </summary>
        public int Variant;

        /// <summary>
        /// Set this to true if the particle MUST render even if the particle cap is reached.
        /// Use sparingly - only for important visual feedback.
        /// </summary>
        public virtual bool Important => false;

        /// <summary>
        /// Set this to true to automatically remove the particle when its time reaches lifetime.
        /// </summary>
        public virtual bool SetLifetime => false;

        /// <summary>
        /// Returns a value from 0 to 1 representing how far through its lifetime the particle is.
        /// </summary>
        public float LifetimeCompletion => SetLifetime ? (float)Time / Lifetime : 0f;

        /// <summary>
        /// The texture path for this particle. Override in subclasses.
        /// </summary>
        public virtual string Texture => "";

        /// <summary>
        /// The number of frame variants this particle has vertically in its spritesheet.
        /// </summary>
        public virtual int FrameVariants => 1;

        /// <summary>
        /// Set this to true to use custom drawing instead of default particle drawing.
        /// </summary>
        public virtual bool UseCustomDraw => false;

        /// <summary>
        /// Set this to true to use additive blending for glow effects.
        /// </summary>
        public virtual bool UseAdditiveBlend => false;

        /// <summary>
        /// Set this to true to use half transparency blending.
        /// Overridden if UseAdditiveBlend is true.
        /// </summary>
        public virtual bool UseHalfTransparency => false;

        /// <summary>
        /// Override this method to handle custom particle drawing.
        /// Only called if UseCustomDraw is true.
        /// </summary>
        public virtual void CustomDraw(SpriteBatch spriteBatch) { }

        /// <summary>
        /// Override this method to handle custom particle drawing with a base position offset.
        /// </summary>
        public virtual void CustomDraw(SpriteBatch spriteBatch, Vector2 basePosition) { }

        /// <summary>
        /// Called every frame to update the particle.
        /// Position is automatically updated by velocity, and Time is automatically incremented.
        /// </summary>
        public virtual void Update() { }

        /// <summary>
        /// Removes this particle from the handler.
        /// </summary>
        public void Kill() => MagnumParticleHandler.RemoveParticle(this);
        
        /// <summary>
        /// Resets this particle to default values for object pooling.
        /// Override in subclasses to reset custom fields.
        /// </summary>
        public virtual void Reset()
        {
            ID = 0;
            Time = 0;
            Position = Vector2.Zero;
            Velocity = Vector2.Zero;
            Rotation = 0f;
            Scale = 1f;
            Color = Color.White;
            Lifetime = 0;
            Variant = 0;
        }

        #region Animation Helpers
        /// <summary>
        /// Helper struct for defining animation curve segments.
        /// </summary>
        public struct CurveSegment
        {
            public EasingType Easing;
            public float StartX;
            public float StartY;
            public float Lift;
            public int Power;

            public CurveSegment(EasingType easing, float startX, float startY, float lift, int power = 1)
            {
                Easing = easing;
                StartX = startX;
                StartY = startY;
                Lift = lift;
                Power = power;
            }
        }

        public enum EasingType
        {
            Linear,
            SineIn,
            SineOut,
            SineBump,
            PolyIn,
            PolyOut,
            ExpIn,
            ExpOut,
            CircIn,
            CircOut
        }

        /// <summary>
        /// Evaluates a piecewise animation curve at a given progress value.
        /// </summary>
        public static float PiecewiseAnimation(float progress, CurveSegment[] segments)
        {
            float output = 0f;

            for (int i = 0; i < segments.Length; i++)
            {
                CurveSegment segment = segments[i];
                float nextStart = i + 1 < segments.Length ? segments[i + 1].StartX : 1f;

                if (progress < segment.StartX || progress >= nextStart)
                    continue;

                float segmentProgress = (progress - segment.StartX) / (nextStart - segment.StartX);
                float easedProgress = ApplyEasing(segment.Easing, segmentProgress, segment.Power);
                output = segment.StartY + segment.Lift * easedProgress;
                break;
            }

            return output;
        }

        private static float ApplyEasing(EasingType easing, float t, int power)
        {
            return easing switch
            {
                EasingType.Linear => t,
                EasingType.SineIn => 1f - (float)Math.Cos(t * MathHelper.PiOver2),
                EasingType.SineOut => (float)Math.Sin(t * MathHelper.PiOver2),
                EasingType.SineBump => (float)Math.Sin(t * MathHelper.Pi),
                EasingType.PolyIn => (float)Math.Pow(t, power),
                EasingType.PolyOut => 1f - (float)Math.Pow(1f - t, power),
                EasingType.ExpIn => t == 0 ? 0 : (float)Math.Pow(2, 10 * (t - 1)),
                EasingType.ExpOut => t == 1 ? 1 : 1f - (float)Math.Pow(2, -10 * t),
                EasingType.CircIn => 1f - (float)Math.Sqrt(1 - t * t),
                EasingType.CircOut => (float)Math.Sqrt(1 - (t - 1) * (t - 1)),
                _ => t
            };
        }
        #endregion
    }
}
