using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX.Effects
{
    /// <summary>
    /// Choreographed impact effect system with staged particle spawning.
    /// 
    /// Impact Stages:
    /// 1. Anticipation (0-5 frames): Energy buildup
    /// 2. Impact (5-10 frames): Main explosion
    /// 3. Shockwave (10-20 frames): Expanding ring
    /// 4. Debris (20-60 frames): Particles settle
    /// 5. Aftermath (60+ frames): Smoke, fading
    /// 
    /// Choreography Principles:
    /// - Timing curves: Sharp impact → slow dissipation
    /// - Layering: Multiple effects overlap
    /// - Scale variation: Small sparks → large smoke
    /// - Color progression: White → yellow → orange → gray
    /// </summary>
    public class ImpactEffectManager : ModSystem
    {
        private static List<ImpactEffect> _activeEffects = new List<ImpactEffect>();
        private static Texture2D _particleTexture;

        public override void Load()
        {
            _activeEffects = new List<ImpactEffect>();
        }

        public override void Unload()
        {
            _activeEffects?.Clear();
            _activeEffects = null;
            _particleTexture = null;
        }

        /// <summary>
        /// Spawn a new choreographed impact effect.
        /// </summary>
        /// <param name="position">World position of impact</param>
        /// <param name="normal">Surface normal (direction particles bounce)</param>
        /// <param name="intensity">Effect intensity (0.5 = half, 2.0 = double)</param>
        /// <param name="primaryColor">Main effect color</param>
        /// <param name="secondaryColor">Secondary/fade color</param>
        public static void SpawnImpact(Vector2 position, Vector2 normal, float intensity,
            Color primaryColor, Color secondaryColor)
        {
            _activeEffects.Add(new ImpactEffect(position, normal, intensity, primaryColor, secondaryColor));
        }

        /// <summary>
        /// Spawn a themed impact effect.
        /// </summary>
        public static void SpawnThemedImpact(Vector2 position, Vector2 normal, string theme, float intensity = 1f)
        {
            var (primary, secondary) = GetThemeColors(theme);
            SpawnImpact(position, normal, intensity, primary, secondary);
        }

        public override void PostUpdateEverything()
        {
            // Update all active effects
            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                _activeEffects[i].Update();
                if (_activeEffects[i].IsComplete)
                {
                    _activeEffects.RemoveAt(i);
                }
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            EnsureTexture();

            foreach (var effect in _activeEffects)
            {
                effect.Draw(spriteBatch, _particleTexture);
            }
        }

        private static (Color primary, Color secondary) GetThemeColors(string theme)
        {
            return theme.ToLower() switch
            {
                "lacampanella" => (new Color(255, 140, 40), new Color(200, 50, 30)),
                "eroica" => (new Color(255, 200, 80), new Color(200, 50, 50)),
                "fate" => (new Color(180, 50, 100), new Color(80, 20, 60)),
                "moonlight" => (new Color(138, 43, 226), new Color(75, 0, 130)),
                "swanlake" => (Color.White, new Color(150, 150, 180)),
                "enigma" => (new Color(140, 60, 200), new Color(50, 220, 100)),
                _ => (Color.Cyan, Color.Blue)
            };
        }

        private static void EnsureTexture()
        {
            if (_particleTexture != null && !_particleTexture.IsDisposed) return;

            var device = Main.graphics.GraphicsDevice;
            int size = 32;
            _particleTexture = new Texture2D(device, size, size);
            Color[] data = new Color[size * size];

            Vector2 center = new Vector2(size * 0.5f);
            float radius = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    float alpha = 1f - MathHelper.Clamp(dist / radius, 0f, 1f);
                    alpha = (float)Math.Pow(alpha, 2);
                    data[y * size + x] = Color.White * alpha;
                }
            }

            _particleTexture.SetData(data);
        }
    }

    /// <summary>
    /// Individual choreographed impact effect instance.
    /// </summary>
    public class ImpactEffect
    {
        private enum Stage
        {
            Anticipation,
            Impact,
            Shockwave,
            Debris,
            Aftermath
        }

        private Stage currentStage;
        private int frameCounter;
        private Vector2 position;
        private Vector2 normal;
        private float intensity;
        private Color primaryColor;
        private Color secondaryColor;

        private List<Particle> sparks;
        private List<Particle> debris;
        private List<Particle> smoke;
        private ShockwaveRing shockwave;
        private FlashEffect flash;

        public bool IsComplete { get; private set; }

        public ImpactEffect(Vector2 position, Vector2 normal, float intensity,
            Color primaryColor, Color secondaryColor)
        {
            this.position = position;
            this.normal = normal.LengthSquared() > 0 ? Vector2.Normalize(normal) : Vector2.UnitY;
            this.intensity = intensity;
            this.primaryColor = primaryColor;
            this.secondaryColor = secondaryColor;
            this.currentStage = Stage.Anticipation;

            sparks = new List<Particle>();
            debris = new List<Particle>();
            smoke = new List<Particle>();
            shockwave = new ShockwaveRing();
            flash = new FlashEffect();
        }

        public void Update()
        {
            frameCounter++;

            switch (currentStage)
            {
                case Stage.Anticipation:
                    UpdateAnticipation();
                    if (frameCounter > 5)
                    {
                        currentStage = Stage.Impact;
                        TriggerImpact();
                    }
                    break;

                case Stage.Impact:
                    UpdateImpact();
                    if (frameCounter > 10)
                    {
                        currentStage = Stage.Shockwave;
                        TriggerShockwave();
                    }
                    break;

                case Stage.Shockwave:
                    UpdateShockwave();
                    if (frameCounter > 20)
                        currentStage = Stage.Debris;
                    break;

                case Stage.Debris:
                    UpdateDebris();
                    if (frameCounter > 60)
                        currentStage = Stage.Aftermath;
                    break;

                case Stage.Aftermath:
                    UpdateAftermath();
                    if (frameCounter > 120 && AllParticlesGone())
                        IsComplete = true;
                    break;
            }

            UpdateParticles(sparks);
            UpdateParticles(debris);
            UpdateParticles(smoke);
            shockwave.Update();
            flash.Update();
        }

        private void UpdateAnticipation()
        {
            // Converging energy particles
            if (frameCounter % 2 == 0)
            {
                float angle = Main.rand.NextFloat(0f, MathHelper.TwoPi);
                Vector2 offset = new Vector2(
                    (float)Math.Cos(angle),
                    (float)Math.Sin(angle)
                ) * 30f;

                sparks.Add(new Particle
                {
                    Position = position + offset,
                    Velocity = -offset.SafeNormalize(Vector2.Zero) * 2f,
                    Color = Color.Lerp(primaryColor, Color.White, 0.5f),
                    Scale = 0.3f * intensity,
                    Lifetime = 5,
                    FadeIn = true
                });
            }
        }

        private void TriggerImpact()
        {
            flash.Trigger(position, 2f * intensity, primaryColor);

            // Screen shake (if available)
            if (ScreenShakeManager.Instance != null)
            {
                ScreenShakeManager.Instance.AddShake(8f * intensity, 0.3f);
            }

            // Burst of fast sparks
            int sparkCount = (int)(50 * intensity);
            for (int i = 0; i < sparkCount; i++)
            {
                float angle = normal.ToRotation() +
                    MathHelper.ToRadians(Main.rand.NextFloat(-90f, 90f));
                float speed = Main.rand.NextFloat(5f, 15f) * intensity;

                sparks.Add(new Particle
                {
                    Position = position,
                    Velocity = angle.ToRotationVector2() * speed,
                    Acceleration = new Vector2(0, 0.3f),
                    Color = Color.Lerp(Color.White, primaryColor, Main.rand.NextFloat()),
                    Scale = Main.rand.NextFloat(0.5f, 1.5f) * intensity,
                    Lifetime = Main.rand.Next(10, 25),
                    FadeOut = true,
                    Rotation = Main.rand.NextFloat(0f, MathHelper.TwoPi),
                    RotationSpeed = Main.rand.NextFloat(-0.3f, 0.3f)
                });
            }
        }

        private void TriggerShockwave()
        {
            shockwave.Trigger(position, 100f * intensity, 20, primaryColor);
        }

        private void UpdateImpact()
        {
            if (frameCounter % 3 == 0 && Main.rand.NextFloat() < 0.5f)
            {
                float angle = normal.ToRotation() +
                    MathHelper.ToRadians(Main.rand.NextFloat(-120f, 120f));
                float speed = Main.rand.NextFloat(3f, 8f);

                sparks.Add(new Particle
                {
                    Position = position,
                    Velocity = angle.ToRotationVector2() * speed,
                    Acceleration = new Vector2(0, 0.2f),
                    Color = primaryColor,
                    Scale = Main.rand.NextFloat(0.3f, 1f) * intensity,
                    Lifetime = Main.rand.Next(15, 30),
                    FadeOut = true
                });
            }
        }

        private void UpdateShockwave()
        {
            if (frameCounter % 2 == 0)
            {
                float angle = Main.rand.NextFloat(0f, MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(1f, 4f);

                debris.Add(new Particle
                {
                    Position = position,
                    Velocity = angle.ToRotationVector2() * speed,
                    Acceleration = new Vector2(0, 0.15f),
                    Color = Color.Lerp(secondaryColor, Color.Gray, Main.rand.NextFloat()),
                    Scale = Main.rand.NextFloat(0.8f, 2f) * intensity,
                    Lifetime = Main.rand.Next(40, 80),
                    FadeOut = true,
                    Rotation = Main.rand.NextFloat(0f, MathHelper.TwoPi),
                    RotationSpeed = Main.rand.NextFloat(-0.1f, 0.1f)
                });
            }
        }

        private void UpdateDebris()
        {
            if (frameCounter % 5 == 0 && smoke.Count < 20)
            {
                smoke.Add(new Particle
                {
                    Position = position + Main.rand.NextVector2Circular(20f, 20f),
                    Velocity = new Vector2(
                        Main.rand.NextFloat(-0.5f, 0.5f),
                        Main.rand.NextFloat(-1f, -0.3f)
                    ),
                    Color = Color.Gray,
                    Scale = Main.rand.NextFloat(2f, 4f) * intensity,
                    ScaleVelocity = 0.05f,
                    Lifetime = Main.rand.Next(60, 120),
                    FadeIn = true,
                    FadeOut = true,
                    Alpha = 0.5f
                });
            }
        }

        private void UpdateAftermath()
        {
            // Just let particles fade naturally
        }

        private void UpdateParticles(List<Particle> particles)
        {
            for (int i = particles.Count - 1; i >= 0; i--)
            {
                var p = particles[i];
                p.Age++;

                if (p.Age >= p.Lifetime)
                {
                    particles.RemoveAt(i);
                    continue;
                }

                p.Velocity += p.Acceleration;
                p.Position += p.Velocity;
                p.Rotation += p.RotationSpeed;
                p.Scale += p.ScaleVelocity;

                float lifeProgress = p.Age / (float)p.Lifetime;

                if (p.FadeIn && lifeProgress < 0.2f)
                {
                    p.Alpha = lifeProgress / 0.2f;
                }
                else if (p.FadeOut && lifeProgress > 0.7f)
                {
                    p.Alpha = (1f - lifeProgress) / 0.3f;
                }
                else if (!p.FadeIn && !p.FadeOut)
                {
                    p.Alpha = 1f;
                }
            }
        }

        private bool AllParticlesGone()
        {
            return sparks.Count == 0 && debris.Count == 0 && smoke.Count == 0;
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D particleTexture)
        {
            Vector2 origin = new Vector2(particleTexture.Width, particleTexture.Height) * 0.5f;

            // Draw smoke (alpha blend)
            DrawParticleList(spriteBatch, smoke, particleTexture, origin, BlendState.AlphaBlend);

            // Draw debris (alpha blend)
            DrawParticleList(spriteBatch, debris, particleTexture, origin, BlendState.AlphaBlend);

            // Draw sparks (additive)
            DrawParticleList(spriteBatch, sparks, particleTexture, origin, BlendState.Additive);

            // Draw flash and shockwave (additive)
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);

            flash.Draw(spriteBatch, particleTexture);
            shockwave.Draw(spriteBatch);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawParticleList(SpriteBatch spriteBatch, List<Particle> particles,
            Texture2D texture, Vector2 origin, BlendState blendState)
        {
            if (particles.Count == 0) return;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, blendState,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);

            foreach (var p in particles)
            {
                Color drawColor = p.Color * p.Alpha;
                if (blendState == BlendState.Additive)
                    drawColor = drawColor with { A = 0 };

                spriteBatch.Draw(
                    texture,
                    p.Position - Main.screenPosition,
                    null,
                    drawColor,
                    p.Rotation,
                    origin,
                    p.Scale,
                    SpriteEffects.None,
                    0f
                );
            }
        }

        private class Particle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public Vector2 Acceleration;
            public Color Color;
            public float Scale;
            public float ScaleVelocity;
            public float Rotation;
            public float RotationSpeed;
            public float Alpha = 1f;
            public int Lifetime;
            public int Age;
            public bool FadeIn;
            public bool FadeOut;
        }
    }

    /// <summary>
    /// Expanding shockwave ring effect.
    /// </summary>
    internal class ShockwaveRing
    {
        private Vector2 center;
        private float radius;
        private float maxRadius;
        private float thickness;
        private float alpha;
        private bool active;
        private int duration;
        private int age;
        private Color color;

        private static Texture2D _ringTexture;

        public void Trigger(Vector2 center, float maxRadius, int duration, Color color)
        {
            this.center = center;
            this.maxRadius = maxRadius;
            this.duration = duration;
            this.color = color;
            this.radius = 0f;
            this.age = 0;
            this.active = true;
            this.thickness = 8f;
        }

        public void Update()
        {
            if (!active) return;

            age++;
            float progress = age / (float)duration;

            radius = maxRadius * EaseOut(progress);
            alpha = 1f - progress;
            thickness = MathHelper.Lerp(8f, 2f, progress);

            if (age >= duration)
                active = false;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!active || alpha <= 0f) return;

            EnsureTexture();

            Vector2 drawPos = center - Main.screenPosition;
            float scale = radius * 2f / _ringTexture.Width;

            spriteBatch.Draw(
                _ringTexture,
                drawPos,
                null,
                (color with { A = 0 }) * alpha,
                0f,
                new Vector2(_ringTexture.Width, _ringTexture.Height) * 0.5f,
                scale,
                SpriteEffects.None,
                0f
            );
        }

        private float EaseOut(float t)
        {
            return 1f - (float)Math.Pow(1f - t, 3);
        }

        private static void EnsureTexture()
        {
            if (_ringTexture != null && !_ringTexture.IsDisposed) return;

            var device = Main.graphics.GraphicsDevice;
            int size = 128;
            _ringTexture = new Texture2D(device, size, size);
            Color[] data = new Color[size * size];

            Vector2 center = new Vector2(size * 0.5f);
            float outerRadius = size * 0.5f;
            float innerRadius = size * 0.35f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);

                    float alpha = 0f;
                    if (dist >= innerRadius && dist <= outerRadius)
                    {
                        float ringDist = Math.Abs(dist - (innerRadius + outerRadius) * 0.5f);
                        float ringWidth = (outerRadius - innerRadius) * 0.5f;
                        alpha = 1f - MathHelper.Clamp(ringDist / ringWidth, 0f, 1f);
                        alpha = (float)Math.Pow(alpha, 2);
                    }

                    data[y * size + x] = Color.White * alpha;
                }
            }

            _ringTexture.SetData(data);
        }
    }

    /// <summary>
    /// Central flash effect for impacts.
    /// </summary>
    internal class FlashEffect
    {
        private Vector2 position;
        private float scale;
        private float alpha;
        private Color color;
        private bool active;
        private int duration = 10;
        private int age;

        public void Trigger(Vector2 position, float scale, Color color)
        {
            this.position = position;
            this.scale = scale;
            this.color = color;
            this.alpha = 1f;
            this.age = 0;
            this.active = true;
        }

        public void Update()
        {
            if (!active) return;

            age++;
            float progress = age / (float)duration;

            alpha = 1f - (float)Math.Pow(progress, 0.5f);

            if (age >= duration)
                active = false;
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D texture)
        {
            if (!active || alpha <= 0f) return;

            Vector2 drawPos = position - Main.screenPosition;
            Vector2 origin = new Vector2(texture.Width, texture.Height) * 0.5f;

            // White core
            spriteBatch.Draw(
                texture,
                drawPos,
                null,
                Color.White * alpha,
                0f,
                origin,
                scale * 2f,
                SpriteEffects.None,
                0f
            );

            // Colored outer
            spriteBatch.Draw(
                texture,
                drawPos,
                null,
                (color with { A = 0 }) * alpha * 0.6f,
                0f,
                origin,
                scale * 4f,
                SpriteEffects.None,
                0f
            );
        }
    }
}
