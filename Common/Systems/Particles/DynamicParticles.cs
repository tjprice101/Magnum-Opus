using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MagnumOpus.Common.Systems.Particles
{
    /// <summary>
    /// Dynamic particle classes with advanced animation behaviors.
    /// These particles feature pulsing, spiraling, color cycling, and phased animations
    /// for creating aesthetically pleasing and unique visual effects.
    /// 
    /// === DESIGN PHILOSOPHY ===
    /// Every particle should feel ALIVE - breathing, pulsing, flowing.
    /// Static particles are boring. Dynamic particles captivate.
    /// </summary>
    
    #region Pulsing Particles
    
    /// <summary>
    /// A bloom particle that pulses/breathes - scale oscillates over time.
    /// Creates a hypnotic, living glow effect perfect for magical cores and hearts.
    /// </summary>
    public class PulsingBloomParticle : Particle
    {
        public override string Texture => "BloomCircle";
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;
        public override bool SetLifetime => true;
        
        private Color BaseColor;
        private Color SecondaryColor;
        private float BaseScale;
        private float PulseAmplitude;
        private float PulseSpeed;
        private float opacity;
        private bool UseColorGradient;
        
        /// <summary>
        /// Full constructor with all pulsing options.
        /// </summary>
        /// <param name="position">Spawn position</param>
        /// <param name="velocity">Movement velocity</param>
        /// <param name="color">Primary color</param>
        /// <param name="secondaryColor">Color to pulse toward (if useGradient is true)</param>
        /// <param name="scale">Base scale</param>
        /// <param name="lifetime">Duration in frames</param>
        /// <param name="pulseAmplitude">How much scale varies (0.0-1.0 range recommended)</param>
        /// <param name="pulseSpeed">Oscillation speed (0.1 = slow, 0.5 = fast)</param>
        /// <param name="useGradient">Whether to also pulse color</param>
        public PulsingBloomParticle(Vector2 position, Vector2 velocity, Color color, Color secondaryColor, 
            float scale, int lifetime, float pulseAmplitude = 0.3f, float pulseSpeed = 0.15f, bool useGradient = true)
        {
            Position = position;
            Velocity = velocity;
            BaseColor = color;
            SecondaryColor = secondaryColor;
            BaseScale = scale;
            Scale = scale;
            Lifetime = lifetime;
            PulseAmplitude = pulseAmplitude;
            PulseSpeed = pulseSpeed;
            UseColorGradient = useGradient;
        }
        
        /// <summary>
        /// Simple constructor - single color pulsing.
        /// </summary>
        public PulsingBloomParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
            : this(position, velocity, color, color * 0.7f, scale, lifetime, 0.25f, 0.12f, false)
        {
        }
        
        public override void Update()
        {
            // Sine-based fade: fade in, hold, fade out
            float lifeProg = LifetimeCompletion;
            if (lifeProg < 0.15f)
                opacity = lifeProg / 0.15f;
            else if (lifeProg > 0.7f)
                opacity = 1f - ((lifeProg - 0.7f) / 0.3f);
            else
                opacity = 1f;
            
            // PULSING SCALE - the breathing effect
            float pulse = (float)Math.Sin(Time * PulseSpeed) * PulseAmplitude;
            Scale = BaseScale * (1f + pulse);
            
            // Color gradient oscillation
            if (UseColorGradient)
            {
                float colorPulse = ((float)Math.Sin(Time * PulseSpeed * 0.7f) + 1f) * 0.5f;
                Color = Color.Lerp(BaseColor, SecondaryColor, colorPulse);
            }
            else
            {
                Color = BaseColor;
            }
            
            Velocity *= 0.97f;
            
            Lighting.AddLight(Position, Color.R / 255f * opacity * 0.6f, 
                Color.G / 255f * opacity * 0.6f, Color.B / 255f * opacity * 0.6f);
        }
        
        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ParticleTextureHelper.GetTexture(Texture);
            Vector2 drawPos = Position - Main.screenPosition;
            Vector2 origin = tex.Size() / 2f;
            
            Color bloomColor = Color with { A = 0 };
            
            // 4-layer Fargos-style bloom with pulsing scale
            float[] scales = { 2.0f, 1.4f, 0.9f, 0.4f };
            float[] opacities = { 0.3f, 0.5f, 0.7f, 0.85f };
            
            for (int i = 0; i < 4; i++)
            {
                spriteBatch.Draw(tex, drawPos, null, bloomColor * (opacity * opacities[i]),
                    0f, origin, Scale * scales[i], SpriteEffects.None, 0f);
            }
        }
    }
    
    /// <summary>
    /// A sparkle that pulses between two sizes, creating a twinkling effect.
    /// Perfect for stars, magical gems, and enchanted objects.
    /// </summary>
    public class TwinklingSparkleParticle : Particle
    {
        public override string Texture => "Sparkle";
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;
        public override bool SetLifetime => true;
        
        private Color BaseColor;
        private Color BloomColor;
        private float MinScale;
        private float MaxScale;
        private float TwinkleSpeed;
        private float Spin;
        private float opacity;
        private float BloomScale;
        
        /// <summary>
        /// Creates a twinkling sparkle with customizable pulse range.
        /// </summary>
        public TwinklingSparkleParticle(Vector2 position, Vector2 velocity, Color color, Color bloom,
            float minScale, float maxScale, int lifetime, float twinkleSpeed = 0.2f, float bloomScale = 1.5f)
        {
            Position = position;
            Velocity = velocity;
            BaseColor = color;
            BloomColor = bloom;
            MinScale = minScale;
            MaxScale = maxScale;
            Lifetime = lifetime;
            TwinkleSpeed = twinkleSpeed;
            BloomScale = bloomScale;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            Spin = Main.rand.NextFloat(-0.08f, 0.08f);
        }
        
        /// <summary>
        /// Simple constructor with sensible defaults.
        /// </summary>
        public TwinklingSparkleParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
            : this(position, velocity, color, color * 0.6f, scale * 0.6f, scale * 1.4f, lifetime, 0.18f, 1.4f)
        {
        }
        
        public override void Update()
        {
            opacity = (float)Math.Sin(LifetimeCompletion * MathHelper.Pi);
            
            // TWINKLING - rapid scale oscillation
            float twinkle = ((float)Math.Sin(Time * TwinkleSpeed) + 1f) * 0.5f;
            Scale = MathHelper.Lerp(MinScale, MaxScale, twinkle);
            
            Rotation += Spin * twinkle; // Spin speed also varies with twinkle
            Velocity *= 0.96f;
            
            Lighting.AddLight(Position, BaseColor.R / 255f * opacity * 0.5f,
                BaseColor.G / 255f * opacity * 0.5f, BaseColor.B / 255f * opacity * 0.5f);
        }
        
        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D starTex = ParticleTextureHelper.GetTexture(Texture);
            Texture2D bloomTex = ParticleTextureHelper.GetTexture("BloomCircle");
            
            Vector2 drawPos = Position - Main.screenPosition;
            float properBloomSize = (float)starTex.Height / (float)bloomTex.Height;
            
            Color bloomDrawColor = BloomColor with { A = 0 };
            Color sparkleDrawColor = BaseColor with { A = 0 };
            
            // Multi-layer bloom
            spriteBatch.Draw(bloomTex, drawPos, null, bloomDrawColor * opacity * 0.3f,
                0, bloomTex.Size() / 2f, Scale * BloomScale * properBloomSize * 1.6f, SpriteEffects.None, 0);
            spriteBatch.Draw(bloomTex, drawPos, null, bloomDrawColor * opacity * 0.5f,
                0, bloomTex.Size() / 2f, Scale * BloomScale * properBloomSize, SpriteEffects.None, 0);
            
            // Sparkle star
            spriteBatch.Draw(starTex, drawPos, null, sparkleDrawColor * opacity,
                Rotation, starTex.Size() / 2f, Scale, SpriteEffects.None, 0);
        }
    }
    
    #endregion
    
    #region Spiral/Orbiting Particles
    
    /// <summary>
    /// A particle that spirals outward or inward from a center point.
    /// Creates mesmerizing vortex and galaxy effects.
    /// </summary>
    public class SpiralParticle : Particle
    {
        public override string Texture => "SoftGlow";
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;
        public override bool SetLifetime => true;
        
        private Vector2 CenterPoint;
        private float Angle;
        private float Radius;
        private float AngularSpeed;
        private float RadialSpeed;
        private Color BaseColor;
        private Color EndColor;
        private float BaseScale;
        private float opacity;
        private bool SpiralOutward;
        
        /// <summary>
        /// Creates a spiraling particle.
        /// </summary>
        /// <param name="center">Center point to spiral around</param>
        /// <param name="startRadius">Initial distance from center</param>
        /// <param name="startAngle">Initial angle (radians)</param>
        /// <param name="angularSpeed">Rotation speed (radians per frame)</param>
        /// <param name="radialSpeed">How fast radius changes per frame</param>
        /// <param name="color">Start color</param>
        /// <param name="endColor">End color (fades toward this)</param>
        /// <param name="scale">Particle scale</param>
        /// <param name="lifetime">Duration in frames</param>
        /// <param name="outward">True = spiral out, False = spiral in</param>
        public SpiralParticle(Vector2 center, float startRadius, float startAngle, float angularSpeed,
            float radialSpeed, Color color, Color endColor, float scale, int lifetime, bool outward = true)
        {
            CenterPoint = center;
            Radius = startRadius;
            Angle = startAngle;
            AngularSpeed = angularSpeed;
            RadialSpeed = radialSpeed;
            BaseColor = color;
            EndColor = endColor;
            BaseScale = scale;
            Scale = scale;
            Lifetime = lifetime;
            SpiralOutward = outward;
            
            // Calculate initial position
            Position = center + Angle.ToRotationVector2() * Radius;
        }
        
        /// <summary>
        /// Simple constructor for outward spiral.
        /// </summary>
        public SpiralParticle(Vector2 center, Color color, float scale, int lifetime)
            : this(center, 5f, Main.rand.NextFloat(MathHelper.TwoPi), 0.08f, 1.5f, 
                  color, color * 0.3f, scale, lifetime, true)
        {
        }
        
        public override void Update()
        {
            // Fade based on lifetime
            opacity = (float)Math.Sin(LifetimeCompletion * MathHelper.Pi);
            
            // SPIRAL MOTION
            Angle += AngularSpeed;
            Radius += SpiralOutward ? RadialSpeed : -RadialSpeed;
            
            // Update position based on spiral
            Position = CenterPoint + Angle.ToRotationVector2() * Math.Max(0, Radius);
            
            // Color gradient over lifetime
            Color = Color.Lerp(BaseColor, EndColor, LifetimeCompletion);
            
            // Scale shrinks as it spirals
            Scale = BaseScale * (1f - LifetimeCompletion * 0.5f);
            
            // Slow down angular speed slightly for natural feel
            AngularSpeed *= 0.995f;
            
            Lighting.AddLight(Position, Color.R / 255f * opacity * 0.4f,
                Color.G / 255f * opacity * 0.4f, Color.B / 255f * opacity * 0.4f);
        }
        
        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ParticleTextureHelper.GetTexture(Texture);
            Vector2 drawPos = Position - Main.screenPosition;
            Vector2 origin = tex.Size() / 2f;
            
            Color bloomColor = Color with { A = 0 };
            
            // 3-layer bloom
            spriteBatch.Draw(tex, drawPos, null, bloomColor * (opacity * 0.3f),
                0f, origin, Scale * 1.6f, SpriteEffects.None, 0);
            spriteBatch.Draw(tex, drawPos, null, bloomColor * (opacity * 0.5f),
                0f, origin, Scale * 1.2f, SpriteEffects.None, 0);
            spriteBatch.Draw(tex, drawPos, null, bloomColor * (opacity * 0.8f),
                0f, origin, Scale, SpriteEffects.None, 0);
        }
    }
    
    /// <summary>
    /// A particle that orbits a fixed point at constant radius.
    /// Perfect for aura effects, magical shields, and planetary motifs.
    /// </summary>
    public class OrbitingParticle : Particle
    {
        public override string Texture => _textureName;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;
        public override bool SetLifetime => true;
        
        private string _textureName;
        private Vector2 CenterPoint;
        private float Angle;
        private float OrbitRadius;
        private float AngularSpeed;
        private Color BaseColor;
        private float BaseScale;
        private float opacity;
        private float ScalePulseSpeed;
        private float ScalePulseAmount;
        private bool FaceOutward;
        
        /// <summary>
        /// Creates an orbiting particle with full customization.
        /// </summary>
        /// <param name="center">Point to orbit around</param>
        /// <param name="radius">Orbit radius</param>
        /// <param name="startAngle">Initial angle</param>
        /// <param name="angularSpeed">Orbit speed (positive = counterclockwise)</param>
        /// <param name="color">Particle color</param>
        /// <param name="scale">Base scale</param>
        /// <param name="lifetime">Duration</param>
        /// <param name="textureName">Texture to use</param>
        /// <param name="faceOutward">Whether particle rotates to face away from center</param>
        /// <param name="scalePulse">Scale pulsing amount (0 = none)</param>
        public OrbitingParticle(Vector2 center, float radius, float startAngle, float angularSpeed,
            Color color, float scale, int lifetime, string textureName = "SoftGlow", 
            bool faceOutward = false, float scalePulse = 0.15f, float scalePulseSpeed = 0.1f)
        {
            _textureName = textureName;
            CenterPoint = center;
            OrbitRadius = radius;
            Angle = startAngle;
            AngularSpeed = angularSpeed;
            BaseColor = color;
            BaseScale = scale;
            Scale = scale;
            Lifetime = lifetime;
            FaceOutward = faceOutward;
            ScalePulseAmount = scalePulse;
            ScalePulseSpeed = scalePulseSpeed;
            
            Position = center + Angle.ToRotationVector2() * radius;
            Rotation = faceOutward ? Angle + MathHelper.PiOver2 : 0f;
        }
        
        /// <summary>
        /// Simple constructor.
        /// </summary>
        public OrbitingParticle(Vector2 center, float radius, Color color, float scale, int lifetime)
            : this(center, radius, Main.rand.NextFloat(MathHelper.TwoPi), 0.05f, color, scale, lifetime)
        {
        }
        
        /// <summary>
        /// Updates the orbit center point. Call this in projectile AI to make particle follow.
        /// </summary>
        public void UpdateCenter(Vector2 newCenter)
        {
            CenterPoint = newCenter;
        }
        
        public override void Update()
        {
            opacity = (float)Math.Sin(LifetimeCompletion * MathHelper.Pi);
            
            // ORBIT MOTION
            Angle += AngularSpeed;
            Position = CenterPoint + Angle.ToRotationVector2() * OrbitRadius;
            
            if (FaceOutward)
                Rotation = Angle + MathHelper.PiOver2;
            
            // Scale pulsing
            float pulse = (float)Math.Sin(Time * ScalePulseSpeed) * ScalePulseAmount;
            Scale = BaseScale * (1f + pulse);
            
            Color = BaseColor;
            
            Lighting.AddLight(Position, BaseColor.R / 255f * opacity * 0.3f,
                BaseColor.G / 255f * opacity * 0.3f, BaseColor.B / 255f * opacity * 0.3f);
        }
        
        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ParticleTextureHelper.GetTexture(_textureName);
            Vector2 drawPos = Position - Main.screenPosition;
            Vector2 origin = tex.Size() / 2f;
            
            Color bloomColor = BaseColor with { A = 0 };
            
            // 3-layer bloom
            spriteBatch.Draw(tex, drawPos, null, bloomColor * (opacity * 0.35f),
                Rotation, origin, Scale * 1.5f, SpriteEffects.None, 0);
            spriteBatch.Draw(tex, drawPos, null, bloomColor * (opacity * 0.55f),
                Rotation, origin, Scale * 1.15f, SpriteEffects.None, 0);
            spriteBatch.Draw(tex, drawPos, null, bloomColor * (opacity * 0.85f),
                Rotation, origin, Scale, SpriteEffects.None, 0);
        }
    }
    
    #endregion
    
    #region Color Cycling Particles
    
    /// <summary>
    /// A bloom particle that cycles through colors using HSL.
    /// Creates rainbow effects, prismatic glows, and shifting auras.
    /// </summary>
    public class RainbowCyclingParticle : Particle
    {
        public override string Texture => "BloomCircle";
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;
        public override bool SetLifetime => true;
        
        private float HueStart;
        private float HueEnd;
        private float Saturation;
        private float Luminosity;
        private float CycleSpeed;
        private float CurrentHue;
        private float BaseScale;
        private float opacity;
        private bool FullRainbow;
        
        /// <summary>
        /// Creates a color-cycling particle with hue range control.
        /// </summary>
        /// <param name="position">Spawn position</param>
        /// <param name="velocity">Movement velocity</param>
        /// <param name="hueStart">Starting hue (0-1)</param>
        /// <param name="hueEnd">Ending hue (0-1)</param>
        /// <param name="saturation">Color saturation (0-1)</param>
        /// <param name="luminosity">Color brightness (0-1)</param>
        /// <param name="scale">Particle scale</param>
        /// <param name="lifetime">Duration</param>
        /// <param name="cycleSpeed">How fast to cycle through hue range</param>
        public RainbowCyclingParticle(Vector2 position, Vector2 velocity, float hueStart, float hueEnd,
            float saturation, float luminosity, float scale, int lifetime, float cycleSpeed = 0.02f)
        {
            Position = position;
            Velocity = velocity;
            HueStart = hueStart;
            HueEnd = hueEnd;
            Saturation = saturation;
            Luminosity = luminosity;
            BaseScale = scale;
            Scale = scale;
            Lifetime = lifetime;
            CycleSpeed = cycleSpeed;
            CurrentHue = hueStart;
            FullRainbow = false;
        }
        
        /// <summary>
        /// Simple constructor for full rainbow cycling.
        /// </summary>
        public RainbowCyclingParticle(Vector2 position, Vector2 velocity, float scale, int lifetime)
            : this(position, velocity, 0f, 1f, 0.9f, 0.75f, scale, lifetime, 0.015f)
        {
            FullRainbow = true;
        }
        
        public override void Update()
        {
            opacity = (float)Math.Sin(LifetimeCompletion * MathHelper.Pi);
            
            // HUE CYCLING
            if (FullRainbow)
            {
                CurrentHue = (CurrentHue + CycleSpeed) % 1f;
            }
            else
            {
                // Oscillate between start and end hue
                float hueProgress = ((float)Math.Sin(Time * CycleSpeed * MathHelper.TwoPi) + 1f) * 0.5f;
                CurrentHue = MathHelper.Lerp(HueStart, HueEnd, hueProgress);
            }
            
            Color = Main.hslToRgb(CurrentHue, Saturation, Luminosity);
            
            Velocity *= 0.97f;
            
            Lighting.AddLight(Position, Color.R / 255f * opacity * 0.5f,
                Color.G / 255f * opacity * 0.5f, Color.B / 255f * opacity * 0.5f);
        }
        
        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ParticleTextureHelper.GetTexture(Texture);
            Vector2 drawPos = Position - Main.screenPosition;
            Vector2 origin = tex.Size() / 2f;
            
            Color bloomColor = Color with { A = 0 };
            
            // 4-layer Fargos-style bloom
            float[] scales = { 2.0f, 1.4f, 0.9f, 0.4f };
            float[] opacities = { 0.3f, 0.5f, 0.7f, 0.85f };
            
            for (int i = 0; i < 4; i++)
            {
                spriteBatch.Draw(tex, drawPos, null, bloomColor * (opacity * opacities[i]),
                    0f, origin, Scale * scales[i], SpriteEffects.None, 0f);
            }
        }
    }
    
    /// <summary>
    /// A music note particle that shifts through a constrained hue range.
    /// Perfect for themed musical effects with color variety but palette consistency.
    /// </summary>
    public class HueShiftingMusicNoteParticle : Particle
    {
        public override string Texture => _noteType;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;
        public override bool SetLifetime => true;
        
        private string _noteType;
        private float HueMin;
        private float HueMax;
        private float Saturation;
        private float Luminosity;
        private float HueSpeed;
        private float CurrentHue;
        private float opacity;
        private float Spin;
        private float Wobble;
        private float WobbleSpeed;
        private float OriginalX;
        private float BloomScale;
        
        /// <summary>
        /// Creates a hue-shifting music note within a theme palette.
        /// </summary>
        public HueShiftingMusicNoteParticle(Vector2 position, Vector2 velocity, 
            float hueMin, float hueMax, float saturation, float luminosity,
            float scale, int lifetime, float hueSpeed = 0.02f, string noteType = null)
        {
            Position = position;
            OriginalX = position.X;
            Velocity = velocity;
            HueMin = hueMin;
            HueMax = hueMax;
            Saturation = saturation;
            Luminosity = luminosity;
            Scale = scale;
            Lifetime = lifetime;
            HueSpeed = hueSpeed;
            CurrentHue = Main.rand.NextFloat(hueMin, hueMax);
            BloomScale = 1.4f;
            
            _noteType = noteType ?? GetRandomNoteType();
            Rotation = Main.rand.NextFloat(-0.2f, 0.2f);
            Spin = Main.rand.NextFloat(-0.03f, 0.03f);
            Wobble = Main.rand.NextFloat(0f, MathHelper.TwoPi);
            WobbleSpeed = Main.rand.NextFloat(0.05f, 0.12f);
        }
        
        private static string GetRandomNoteType()
        {
            return Main.rand.Next(4) switch
            {
                0 => "MusicNoteQuarter",
                1 => "MusicNoteEighth",
                2 => "MusicNoteSixteenth",
                _ => "MusicNoteDouble"
            };
        }
        
        public override void Update()
        {
            // Fade in then out
            if (LifetimeCompletion < 0.2f)
                opacity = LifetimeCompletion / 0.2f;
            else
                opacity = 1f - ((LifetimeCompletion - 0.2f) / 0.8f);
            
            // HUE CYCLING within theme range
            CurrentHue += HueSpeed;
            if (CurrentHue > HueMax) CurrentHue = HueMin;
            if (CurrentHue < HueMin) CurrentHue = HueMax;
            
            Color = Main.hslToRgb(CurrentHue, Saturation, Luminosity);
            
            // Wobble motion
            Wobble += WobbleSpeed;
            Position = new Vector2(OriginalX + (float)Math.Sin(Wobble) * 10f, Position.Y);
            OriginalX += Velocity.X;
            
            Rotation += Spin;
            Velocity *= 0.98f;
            
            if (opacity > 0.3f)
                Lighting.AddLight(Position, Color.R / 255f * opacity * 0.4f,
                    Color.G / 255f * opacity * 0.4f, Color.B / 255f * opacity * 0.4f);
        }
        
        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D noteTexture = ParticleTextureHelper.GetTexture(_noteType);
            Texture2D bloomTexture = ParticleTextureHelper.GetTexture("BloomCircle");
            
            Vector2 drawPos = Position - Main.screenPosition;
            
            Color bloomColor = Color with { A = 0 };
            
            // Multi-layer bloom behind note (3 layers)
            spriteBatch.Draw(bloomTexture, drawPos, null, bloomColor * opacity * 0.3f,
                0f, bloomTexture.Size() / 2f, Scale * BloomScale * 1.6f, SpriteEffects.None, 0f);
            spriteBatch.Draw(bloomTexture, drawPos, null, bloomColor * opacity * 0.5f,
                0f, bloomTexture.Size() / 2f, Scale * BloomScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(bloomTexture, drawPos, null, bloomColor * opacity * 0.7f,
                0f, bloomTexture.Size() / 2f, Scale * BloomScale * 0.6f, SpriteEffects.None, 0f);
            
            // Note itself
            spriteBatch.Draw(noteTexture, drawPos, null, Color * opacity,
                Rotation, noteTexture.Size() / 2f, Scale, SpriteEffects.None, 0f);
            
            // Bright core
            spriteBatch.Draw(noteTexture, drawPos, null, Color.White * opacity * 0.4f,
                Rotation, noteTexture.Size() / 2f, Scale * 0.5f, SpriteEffects.None, 0f);
        }
    }
    
    #endregion
    
    #region Phased Animation Particles
    
    /// <summary>
    /// A particle with distinct animation phases: Appear, Hold, Disappear.
    /// Each phase can have different behaviors and durations.
    /// </summary>
    public class PhasedBloomParticle : Particle
    {
        public override string Texture => "BloomCircle";
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;
        public override bool SetLifetime => true;
        
        // Phase timings (as fractions of lifetime)
        private float AppearDuration;   // 0-AppearDuration: fade in + expand
        private float HoldDuration;     // AppearDuration to HoldEnd: full opacity
        // HoldEnd to 1.0: fade out + shrink
        
        private Color BaseColor;
        private Color PeakColor;
        private float MinScale;
        private float MaxScale;
        private float opacity;
        private float Spin;
        
        public enum ParticlePhase { Appear, Hold, Disappear }
        public ParticlePhase CurrentPhase { get; private set; }
        
        /// <summary>
        /// Creates a phased particle with customizable phase durations.
        /// </summary>
        /// <param name="position">Spawn position</param>
        /// <param name="velocity">Movement velocity</param>
        /// <param name="color">Base color</param>
        /// <param name="peakColor">Color at peak (during Hold phase)</param>
        /// <param name="minScale">Scale during Appear start</param>
        /// <param name="maxScale">Scale during Hold phase</param>
        /// <param name="lifetime">Total duration</param>
        /// <param name="appearDuration">Fraction for appear phase (0-1)</param>
        /// <param name="holdDuration">Fraction for hold phase (0-1, added to appear)</param>
        public PhasedBloomParticle(Vector2 position, Vector2 velocity, Color color, Color peakColor,
            float minScale, float maxScale, int lifetime, float appearDuration = 0.2f, float holdDuration = 0.5f)
        {
            Position = position;
            Velocity = velocity;
            BaseColor = color;
            PeakColor = peakColor;
            MinScale = minScale;
            MaxScale = maxScale;
            Scale = minScale;
            Lifetime = lifetime;
            AppearDuration = appearDuration;
            HoldDuration = holdDuration;
            CurrentPhase = ParticlePhase.Appear;
            Spin = Main.rand.NextFloat(-0.02f, 0.02f);
        }
        
        /// <summary>
        /// Simple constructor.
        /// </summary>
        public PhasedBloomParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
            : this(position, velocity, color, Color.Lerp(color, Color.White, 0.3f), 
                  scale * 0.3f, scale, lifetime, 0.15f, 0.55f)
        {
        }
        
        public override void Update()
        {
            float progress = LifetimeCompletion;
            
            // Determine current phase and calculate values
            if (progress < AppearDuration)
            {
                // APPEAR PHASE: Fade in, scale up, color transitions to peak
                CurrentPhase = ParticlePhase.Appear;
                float phaseProgress = progress / AppearDuration;
                
                // Ease out for smooth appearance
                float eased = 1f - (1f - phaseProgress) * (1f - phaseProgress);
                
                opacity = eased;
                Scale = MathHelper.Lerp(MinScale, MaxScale, eased);
                Color = Color.Lerp(BaseColor, PeakColor, eased);
            }
            else if (progress < AppearDuration + HoldDuration)
            {
                // HOLD PHASE: Full opacity, max scale, peak color, gentle pulse
                CurrentPhase = ParticlePhase.Hold;
                float phaseProgress = (progress - AppearDuration) / HoldDuration;
                
                opacity = 1f;
                
                // Gentle pulse during hold
                float pulse = (float)Math.Sin(phaseProgress * MathHelper.TwoPi * 2f) * 0.1f;
                Scale = MaxScale * (1f + pulse);
                Color = PeakColor;
            }
            else
            {
                // DISAPPEAR PHASE: Fade out, scale down, color back to base
                CurrentPhase = ParticlePhase.Disappear;
                float disappearStart = AppearDuration + HoldDuration;
                float phaseProgress = (progress - disappearStart) / (1f - disappearStart);
                
                // Ease in for smooth disappearance
                float eased = phaseProgress * phaseProgress;
                
                opacity = 1f - eased;
                Scale = MathHelper.Lerp(MaxScale, MinScale * 0.5f, eased);
                Color = Color.Lerp(PeakColor, BaseColor, eased);
            }
            
            Rotation += Spin;
            Velocity *= 0.96f;
            
            if (opacity > 0.1f)
                Lighting.AddLight(Position, Color.R / 255f * opacity * 0.5f,
                    Color.G / 255f * opacity * 0.5f, Color.B / 255f * opacity * 0.5f);
        }
        
        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ParticleTextureHelper.GetTexture(Texture);
            Vector2 drawPos = Position - Main.screenPosition;
            Vector2 origin = tex.Size() / 2f;
            
            Color bloomColor = Color with { A = 0 };
            
            // 4-layer bloom
            float[] scales = { 2.2f, 1.5f, 1.0f, 0.5f };
            float[] opacities = { 0.25f, 0.45f, 0.7f, 0.9f };
            
            for (int i = 0; i < 4; i++)
            {
                spriteBatch.Draw(tex, drawPos, null, bloomColor * (opacity * opacities[i]),
                    Rotation, origin, Scale * scales[i], SpriteEffects.None, 0f);
            }
        }
    }
    
    /// <summary>
    /// A flare particle with dramatic 3-phase animation: Flash, Bloom, Fade.
    /// Perfect for impacts, explosions, and dramatic reveals.
    /// </summary>
    public class DramaticFlareParticle : Particle
    {
        public override string Texture => _flareType;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;
        public override bool SetLifetime => true;
        
        private string _flareType;
        private Color CoreColor;
        private Color GlowColor;
        private Color FlashColor;
        private float MinScale;
        private float MaxScale;
        private float FlashScale;
        private float opacity;
        private float[] SpinSpeeds;
        
        public enum FlarePhase { Flash, Bloom, Fade }
        public FlarePhase CurrentPhase { get; private set; }
        
        /// <summary>
        /// Creates a dramatic multi-phase flare effect.
        /// </summary>
        public DramaticFlareParticle(Vector2 position, Vector2 velocity, Color coreColor, Color glowColor,
            Color flashColor, float minScale, float maxScale, float flashScale, int lifetime, int flareVariant = -1)
        {
            Position = position;
            Velocity = velocity;
            CoreColor = coreColor;
            GlowColor = glowColor;
            FlashColor = flashColor;
            MinScale = minScale;
            MaxScale = maxScale;
            FlashScale = flashScale;
            Scale = flashScale;
            Lifetime = lifetime;
            CurrentPhase = FlarePhase.Flash;
            
            _flareType = flareVariant < 0 
                ? $"EnergyFlare{Main.rand.Next(1, 8)}"
                : $"EnergyFlare{MathHelper.Clamp(flareVariant, 1, 7)}";
            
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            SpinSpeeds = new float[]
            {
                Main.rand.NextFloat(0.03f, 0.06f),
                Main.rand.NextFloat(-0.04f, -0.02f),
                Main.rand.NextFloat(0.02f, 0.04f)
            };
        }
        
        /// <summary>
        /// Simple constructor.
        /// </summary>
        public DramaticFlareParticle(Vector2 position, Color color, float scale, int lifetime)
            : this(position, Vector2.Zero, color, color * 0.7f, Color.White,
                  scale * 0.4f, scale, scale * 1.8f, lifetime)
        {
        }
        
        public override void Update()
        {
            float progress = LifetimeCompletion;
            
            // Phase 1: FLASH (0-15%) - Bright white flash, oversized
            if (progress < 0.15f)
            {
                CurrentPhase = FlarePhase.Flash;
                float phaseProgress = progress / 0.15f;
                
                opacity = 1f;
                Scale = MathHelper.Lerp(FlashScale, MaxScale, phaseProgress);
                Color = Color.Lerp(FlashColor, CoreColor, phaseProgress);
            }
            // Phase 2: BLOOM (15-50%) - Full glow, gentle pulse
            else if (progress < 0.5f)
            {
                CurrentPhase = FlarePhase.Bloom;
                float phaseProgress = (progress - 0.15f) / 0.35f;
                
                opacity = 1f;
                float pulse = (float)Math.Sin(phaseProgress * MathHelper.Pi * 3f) * 0.15f;
                Scale = MaxScale * (1f + pulse);
                Color = Color.Lerp(CoreColor, GlowColor, phaseProgress * 0.5f);
            }
            // Phase 3: FADE (50-100%) - Graceful disappearance
            else
            {
                CurrentPhase = FlarePhase.Fade;
                float phaseProgress = (progress - 0.5f) / 0.5f;
                
                // Smooth ease-out
                float eased = 1f - (1f - phaseProgress) * (1f - phaseProgress);
                
                opacity = 1f - eased;
                Scale = MathHelper.Lerp(MaxScale, MinScale, eased);
                Color = Color.Lerp(GlowColor, CoreColor * 0.5f, eased);
            }
            
            // Multi-speed spinning for layered effect
            Rotation += SpinSpeeds[0];
            
            Velocity *= 0.95f;
            
            if (opacity > 0.1f)
                Lighting.AddLight(Position, Color.R / 255f * opacity * 0.7f,
                    Color.G / 255f * opacity * 0.7f, Color.B / 255f * opacity * 0.7f);
        }
        
        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D flareTex = ParticleTextureHelper.GetTexture(_flareType);
            Texture2D softGlowTex = ParticleTextureHelper.GetTexture("SoftGlow");
            
            Vector2 drawPos = Position - Main.screenPosition;
            Vector2 flareOrigin = flareTex.Size() / 2f;
            Vector2 glowOrigin = softGlowTex.Size() / 2f;
            
            Color coreBloom = CoreColor with { A = 0 };
            Color glowBloom = GlowColor with { A = 0 };
            Color flashBloom = FlashColor with { A = 0 };
            
            // Background soft glow
            spriteBatch.Draw(softGlowTex, drawPos, null, glowBloom * (opacity * 0.3f),
                0f, glowOrigin, Scale * 1.8f, SpriteEffects.None, 0f);
            
            // Layer 1: Outer spinning flare
            spriteBatch.Draw(flareTex, drawPos, null, glowBloom * (opacity * 0.4f),
                Rotation + Time * SpinSpeeds[1], flareOrigin, Scale * 1.4f, SpriteEffects.None, 0f);
            
            // Layer 2: Main spinning flare (opposite direction)
            spriteBatch.Draw(flareTex, drawPos, null, coreBloom * (opacity * 0.6f),
                Rotation, flareOrigin, Scale, SpriteEffects.None, 0f);
            
            // Layer 3: Inner bright core
            spriteBatch.Draw(flareTex, drawPos, null, flashBloom * (opacity * 0.8f),
                Rotation + Time * SpinSpeeds[2], flareOrigin, Scale * 0.5f, SpriteEffects.None, 0f);
            
            // White-hot center during Flash phase
            if (CurrentPhase == FlarePhase.Flash)
            {
                spriteBatch.Draw(softGlowTex, drawPos, null, Color.White with { A = 0 } * opacity,
                    0f, glowOrigin, Scale * 0.3f, SpriteEffects.None, 0f);
            }
        }
    }
    
    #endregion
    
    #region Trail/Streak Particles
    
    /// <summary>
    /// A streak particle that stretches based on velocity.
    /// Perfect for speed lines, meteor trails, and fast-moving effects.
    /// </summary>
    public class StreakParticle : Particle
    {
        public override string Texture => "SoftGlow";
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;
        public override bool SetLifetime => true;
        
        private Color BaseColor;
        private Color TipColor;
        private float BaseWidth;
        private float MinStretch;
        private float MaxStretch;
        private float opacity;
        private bool HasGravity;
        private float GravityStrength;
        
        /// <summary>
        /// Creates a velocity-stretched streak particle.
        /// </summary>
        public StreakParticle(Vector2 position, Vector2 velocity, Color baseColor, Color tipColor,
            float width, float minStretch, float maxStretch, int lifetime, bool gravity = false, float gravityStrength = 0.15f)
        {
            Position = position;
            Velocity = velocity;
            BaseColor = baseColor;
            TipColor = tipColor;
            BaseWidth = width;
            MinStretch = minStretch;
            MaxStretch = maxStretch;
            Lifetime = lifetime;
            HasGravity = gravity;
            GravityStrength = gravityStrength;
            Rotation = velocity.ToRotation();
        }
        
        /// <summary>
        /// Simple constructor.
        /// </summary>
        public StreakParticle(Vector2 position, Vector2 velocity, Color color, float width, int lifetime)
            : this(position, velocity, color, Color.Lerp(color, Color.White, 0.5f), width, 1f, 4f, lifetime)
        {
        }
        
        public override void Update()
        {
            opacity = 1f - LifetimeCompletion * LifetimeCompletion;
            
            if (HasGravity)
                Velocity.Y += GravityStrength;
            
            Rotation = Velocity.ToRotation();
            
            Velocity *= 0.97f;
            
            if (opacity > 0.2f)
                Lighting.AddLight(Position, BaseColor.R / 255f * opacity * 0.4f,
                    BaseColor.G / 255f * opacity * 0.4f, BaseColor.B / 255f * opacity * 0.4f);
        }
        
        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ParticleTextureHelper.GetTexture(Texture);
            Vector2 drawPos = Position - Main.screenPosition;
            Vector2 origin = tex.Size() / 2f;
            
            // Calculate stretch based on velocity
            float speed = Velocity.Length();
            float stretch = MathHelper.Clamp(speed * 0.3f, MinStretch, MaxStretch);
            
            Color baseBloom = BaseColor with { A = 0 };
            Color tipBloom = TipColor with { A = 0 };
            
            // Draw stretched ellipse (scaled in velocity direction)
            Vector2 scale = new Vector2(stretch, BaseWidth * 0.3f);
            
            // Outer glow
            spriteBatch.Draw(tex, drawPos, null, baseBloom * (opacity * 0.3f),
                Rotation, origin, scale * 1.5f, SpriteEffects.None, 0f);
            
            // Main streak
            spriteBatch.Draw(tex, drawPos, null, baseBloom * (opacity * 0.6f),
                Rotation, origin, scale, SpriteEffects.None, 0f);
            
            // Bright tip (at front of streak)
            Vector2 tipOffset = Velocity.SafeNormalize(Vector2.Zero) * stretch * tex.Width * 0.25f;
            spriteBatch.Draw(tex, drawPos + tipOffset, null, tipBloom * (opacity * 0.8f),
                Rotation, origin, new Vector2(BaseWidth * 0.4f, BaseWidth * 0.4f), SpriteEffects.None, 0f);
        }
    }
    
    /// <summary>
    /// A comet-like particle with a glowing head and fading tail.
    /// Perfect for shooting stars, magical projectiles, and celestial effects.
    /// </summary>
    public class CometParticle : Particle
    {
        public override string Texture => "SoftGlow";
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;
        public override bool SetLifetime => true;
        
        private Color HeadColor;
        private Color TailColor;
        private float HeadScale;
        private float TailLength;
        private int TailSegments;
        private Vector2[] PositionHistory;
        private int HistoryIndex;
        private float opacity;
        private bool HasGravity;
        
        /// <summary>
        /// Creates a comet particle with position-history-based tail.
        /// </summary>
        public CometParticle(Vector2 position, Vector2 velocity, Color headColor, Color tailColor,
            float headScale, int tailSegments, int lifetime, bool gravity = false)
        {
            Position = position;
            Velocity = velocity;
            HeadColor = headColor;
            TailColor = tailColor;
            HeadScale = headScale;
            TailSegments = Math.Max(3, tailSegments);
            TailLength = tailSegments;
            Lifetime = lifetime;
            HasGravity = gravity;
            
            PositionHistory = new Vector2[tailSegments];
            for (int i = 0; i < tailSegments; i++)
                PositionHistory[i] = position;
            HistoryIndex = 0;
        }
        
        /// <summary>
        /// Simple constructor.
        /// </summary>
        public CometParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
            : this(position, velocity, color, color * 0.4f, scale, 8, lifetime)
        {
        }
        
        public override void Update()
        {
            opacity = 1f - LifetimeCompletion;
            
            // Store position history for tail
            PositionHistory[HistoryIndex] = Position;
            HistoryIndex = (HistoryIndex + 1) % TailSegments;
            
            if (HasGravity)
                Velocity.Y += 0.12f;
            
            Velocity *= 0.99f;
            
            if (opacity > 0.2f)
                Lighting.AddLight(Position, HeadColor.R / 255f * opacity * 0.6f,
                    HeadColor.G / 255f * opacity * 0.6f, HeadColor.B / 255f * opacity * 0.6f);
        }
        
        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ParticleTextureHelper.GetTexture(Texture);
            Vector2 origin = tex.Size() / 2f;
            
            // Draw tail (oldest to newest positions)
            for (int i = 0; i < TailSegments; i++)
            {
                int actualIndex = (HistoryIndex + i) % TailSegments;
                Vector2 tailPos = PositionHistory[actualIndex] - Main.screenPosition;
                
                float tailProgress = (float)i / TailSegments;
                float tailOpacity = tailProgress * opacity * 0.6f;
                float tailScale = HeadScale * 0.3f * tailProgress;
                
                Color tailBloom = TailColor with { A = 0 };
                
                spriteBatch.Draw(tex, tailPos, null, tailBloom * tailOpacity,
                    0f, origin, tailScale, SpriteEffects.None, 0f);
            }
            
            // Draw head
            Vector2 drawPos = Position - Main.screenPosition;
            Color headBloom = HeadColor with { A = 0 };
            
            // Outer glow
            spriteBatch.Draw(tex, drawPos, null, headBloom * (opacity * 0.4f),
                0f, origin, HeadScale * 1.8f, SpriteEffects.None, 0f);
            // Main glow
            spriteBatch.Draw(tex, drawPos, null, headBloom * (opacity * 0.7f),
                0f, origin, HeadScale, SpriteEffects.None, 0f);
            // Bright core
            spriteBatch.Draw(tex, drawPos, null, Color.White with { A = 0 } * (opacity * 0.9f),
                0f, origin, HeadScale * 0.4f, SpriteEffects.None, 0f);
        }
    }
    
    #endregion
}
