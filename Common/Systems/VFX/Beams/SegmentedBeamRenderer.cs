using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;

namespace MagnumOpus.Common.Systems.VFX.Beams
{
    /// <summary>
    /// Three-part beam architecture: Muzzle (origin) → Body (middle) → Impact (termination).
    /// 
    /// Visual Hierarchy:
    /// - Muzzle: 100% intensity, brightest, particle emitter
    /// - Body: 60% intensity, repeating pattern
    /// - Impact: 80% intensity, collision effects
    /// </summary>
    public class SegmentedBeam
    {
        public BeamMuzzle Muzzle { get; private set; }
        public BeamBody Body { get; private set; }
        public BeamImpact Impact { get; private set; }

        public Vector2 Origin { get; set; }
        public Vector2 Direction { get; set; }
        public float Length { get; set; }
        public Color BaseColor { get; set; }
        public float Intensity { get; set; }

        private float animationTime;
        private float pulsePhase;

        public SegmentedBeam(Vector2 origin, Vector2 direction, float length)
        {
            Origin = origin;
            Direction = Vector2.Normalize(direction);
            Length = length;
            BaseColor = Color.Cyan;
            Intensity = 1f;

            Muzzle = new BeamMuzzle(origin);
            Body = new BeamBody(origin, Direction, length);
            Impact = new BeamImpact(origin + Direction * length);
        }

        public void Update(float deltaTime)
        {
            animationTime += deltaTime;
            pulsePhase = (float)Math.Sin(animationTime * 5f) * 0.5f + 0.5f;

            Vector2 endPoint = Origin + Direction * Length;

            Muzzle.Update(Origin, Intensity * (1f + pulsePhase * 0.3f));
            Body.Update(Origin, endPoint, animationTime, Intensity);
            Impact.Update(endPoint, Direction, Intensity * (0.8f + pulsePhase * 0.2f));
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw order: Body → Muzzle → Impact (for proper layering)
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);

            Body.Draw(spriteBatch, BaseColor);
            Impact.Draw(spriteBatch, BaseColor);
            Muzzle.Draw(spriteBatch, BaseColor);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);
        }
    }

    /// <summary>
    /// Beam origin point - brightest, with rotating energy bursts and corona particles.
    /// </summary>
    public class BeamMuzzle
    {
        private Vector2 position;
        private float intensity;
        private float rotationAngle;
        private List<MuzzleParticle> particles;

        private Texture2D coreFlash;
        private Texture2D coronaGlow;
        private Texture2D energyBurst;

        private class MuzzleParticle
        {
            public Vector2 Offset;
            public float Angle;
            public float Distance;
            public float Alpha;
            public float Speed;
        }

        public BeamMuzzle(Vector2 position)
        {
            this.position = position;
            particles = new List<MuzzleParticle>();

            // Create procedural textures
            var device = Main.graphics.GraphicsDevice;
            coreFlash = CreateRadialGradient(device, 32, Color.White);
            coronaGlow = CreateRadialGradient(device, 64, Color.Cyan);
            energyBurst = CreateStarBurst(device, 64, 8);

            // Initialize corona particles
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.ToRadians(i * 30f);
                particles.Add(new MuzzleParticle
                {
                    Angle = angle,
                    Distance = 0f,
                    Alpha = 1f,
                    Speed = Main.rand.NextFloat(0.5f, 1.5f)
                });
            }
        }

        public void Update(Vector2 newPosition, float intensity)
        {
            this.position = newPosition;
            this.intensity = intensity;
            rotationAngle += 0.05f;

            // Update expanding corona particles
            foreach (var particle in particles)
            {
                particle.Distance += particle.Speed;
                particle.Alpha = 1f - (particle.Distance / 40f);

                if (particle.Distance > 40f)
                {
                    particle.Distance = 0f;
                    particle.Alpha = 1f;
                }

                particle.Offset = new Vector2(
                    (float)Math.Cos(particle.Angle) * particle.Distance,
                    (float)Math.Sin(particle.Angle) * particle.Distance
                );
            }
        }

        public void Draw(SpriteBatch spriteBatch, Color baseColor)
        {
            Vector2 drawPos = position - Main.screenPosition;

            // Layer 1: Core flash (brightest)
            spriteBatch.Draw(
                coreFlash,
                drawPos,
                null,
                Color.White * intensity,
                0f,
                new Vector2(coreFlash.Width, coreFlash.Height) * 0.5f,
                1.5f,
                SpriteEffects.None,
                0f
            );

            // Layer 2: Rotating energy burst
            spriteBatch.Draw(
                energyBurst,
                drawPos,
                null,
                baseColor * intensity * 0.8f,
                rotationAngle,
                new Vector2(energyBurst.Width, energyBurst.Height) * 0.5f,
                2f,
                SpriteEffects.None,
                0f
            );

            // Layer 3: Expanding corona particles
            foreach (var particle in particles)
            {
                spriteBatch.Draw(
                    coronaGlow,
                    drawPos + particle.Offset,
                    null,
                    baseColor * particle.Alpha * intensity * 0.6f,
                    particle.Angle,
                    new Vector2(coronaGlow.Width, coronaGlow.Height) * 0.5f,
                    0.5f,
                    SpriteEffects.None,
                    0f
                );
            }

            // Layer 4: Screen-space glow (large, soft)
            spriteBatch.Draw(
                coronaGlow,
                drawPos,
                null,
                baseColor * intensity * 0.3f,
                -rotationAngle * 0.5f,
                new Vector2(coronaGlow.Width, coronaGlow.Height) * 0.5f,
                4f,
                SpriteEffects.None,
                0f
            );
        }

        private static Texture2D CreateRadialGradient(GraphicsDevice device, int size, Color color)
        {
            Texture2D texture = new Texture2D(device, size, size);
            Color[] data = new Color[size * size];

            Vector2 center = new Vector2(size * 0.5f);
            float maxDist = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    float alpha = 1f - MathHelper.Clamp(dist / maxDist, 0f, 1f);
                    alpha = (float)Math.Pow(alpha, 2);
                    data[y * size + x] = color * alpha;
                }
            }

            texture.SetData(data);
            return texture;
        }

        private static Texture2D CreateStarBurst(GraphicsDevice device, int size, int points)
        {
            Texture2D texture = new Texture2D(device, size, size);
            Color[] data = new Color[size * size];

            Vector2 center = new Vector2(size * 0.5f);
            float maxDist = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 pos = new Vector2(x, y) - center;
                    float angle = (float)Math.Atan2(pos.Y, pos.X);
                    float dist = pos.Length();

                    float angleFactor = (float)Math.Abs(Math.Cos(angle * points * 0.5f));
                    float distFactor = 1f - MathHelper.Clamp(dist / maxDist, 0f, 1f);

                    float alpha = angleFactor * distFactor;
                    data[y * size + x] = Color.White * alpha;
                }
            }

            texture.SetData(data);
            return texture;
        }
    }

    /// <summary>
    /// Beam body - the main visual element with wave distortion and UV scrolling.
    /// </summary>
    public class BeamBody
    {
        private Vector2 start;
        private Vector2 end;
        private List<Vector2> points;

        private float baseWidth = 20f;
        private float glowWidth = 40f;

        private Texture2D coreTexture;
        private Texture2D glowTexture;

        private float uvOffset;
        private float turbulencePhase;

        public BeamBody(Vector2 start, Vector2 direction, float length)
        {
            this.start = start;
            this.end = start + direction * length;
            points = new List<Vector2>();

            var device = Main.graphics.GraphicsDevice;
            coreTexture = CreateBeamGradient(device, 256, 32);
            glowTexture = CreateSoftGlow(device, 256, 64);
        }

        public void Update(Vector2 newStart, Vector2 newEnd, float time, float intensity)
        {
            this.start = newStart;
            this.end = newEnd;

            uvOffset = (time * 2f) % 1f;
            turbulencePhase = time * 3f;

            GenerateBeamPoints();
        }

        private void GenerateBeamPoints()
        {
            points.Clear();

            Vector2 direction = end - start;
            float length = direction.Length();
            if (length < 1f) return;

            direction /= length;

            int segments = Math.Max(2, (int)(length / 10f) + 1);

            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
                Vector2 basePoint = Vector2.Lerp(start, end, t);

                // Add wave distortion
                Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
                float wave = (float)Math.Sin(t * 15f + turbulencePhase) * 2f;
                wave += (float)Math.Sin(t * 8f - turbulencePhase * 0.7f) * 1.5f;

                Vector2 point = basePoint + perpendicular * wave;
                points.Add(point);
            }
        }

        public void Draw(SpriteBatch spriteBatch, Color baseColor)
        {
            if (points.Count < 2) return;

            // Layer 1: Outer glow
            DrawBeamLayer(spriteBatch, glowTexture, glowWidth * 1.5f, baseColor * 0.2f);

            // Layer 2: Mid glow
            DrawBeamLayer(spriteBatch, glowTexture, glowWidth, baseColor * 0.5f);

            // Layer 3: Core
            DrawBeamLayer(spriteBatch, coreTexture, baseWidth, baseColor);
        }

        private void DrawBeamLayer(SpriteBatch spriteBatch, Texture2D texture, float width, Color color)
        {
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector2 segStart = points[i] - Main.screenPosition;
                Vector2 segEnd = points[i + 1] - Main.screenPosition;

                Vector2 diff = segEnd - segStart;
                float rotation = diff.ToRotation();
                float length = diff.Length();

                // Width taper
                float progress = i / (float)(points.Count - 1);
                float taperFactor = MathHelper.Lerp(1f, 0.6f, progress);

                spriteBatch.Draw(
                    texture,
                    segStart,
                    new Rectangle(0, 0, (int)length, texture.Height),
                    color,
                    rotation,
                    new Vector2(0, texture.Height * 0.5f),
                    new Vector2(1f, width * taperFactor / texture.Height),
                    SpriteEffects.None,
                    0f
                );
            }
        }

        private static Texture2D CreateBeamGradient(GraphicsDevice device, int width, int height)
        {
            Texture2D texture = new Texture2D(device, width, height);
            Color[] data = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                float v = y / (float)height;
                float centerDist = Math.Abs(v - 0.5f) * 2f;
                float alpha = 1f - (float)Math.Pow(centerDist, 2);

                for (int x = 0; x < width; x++)
                {
                    data[y * width + x] = Color.White * alpha;
                }
            }

            texture.SetData(data);
            return texture;
        }

        private static Texture2D CreateSoftGlow(GraphicsDevice device, int width, int height)
        {
            Texture2D texture = new Texture2D(device, width, height);
            Color[] data = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                float v = y / (float)height;
                float centerDist = Math.Abs(v - 0.5f) * 2f;
                float alpha = 1f - (float)Math.Pow(centerDist, 1.5f);
                alpha = MathHelper.Clamp(alpha, 0f, 1f);

                for (int x = 0; x < width; x++)
                {
                    data[y * width + x] = Color.White * alpha;
                }
            }

            texture.SetData(data);
            return texture;
        }
    }

    /// <summary>
    /// Beam impact point - rotating flash effects and spark particles.
    /// </summary>
    public class BeamImpact
    {
        private Vector2 position;
        private Vector2 normal;
        private float intensity;

        private List<ImpactParticle> particles;
        private float flashScale;
        private float flashRotation;

        private Texture2D flashTexture;
        private Texture2D sparkTexture;
        private Texture2D glowTexture;

        private class ImpactParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Rotation;
            public float RotationSpeed;
            public float Scale;
            public float Alpha;
            public int Lifetime;
            public int Age;
        }

        public BeamImpact(Vector2 position)
        {
            this.position = position;
            this.normal = Vector2.UnitX;
            particles = new List<ImpactParticle>();

            var device = Main.graphics.GraphicsDevice;
            flashTexture = CreateStarFlash(device, 64, 4);
            sparkTexture = CreateSparkTexture(device, 16, 4);
            glowTexture = CreateRadialGlow(device, 128);
        }

        public void Update(Vector2 newPosition, Vector2 impactNormal, float intensity)
        {
            this.position = newPosition;
            this.normal = impactNormal.LengthSquared() > 0 ? Vector2.Normalize(impactNormal) : Vector2.UnitX;
            this.intensity = intensity;

            flashScale = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.3f) * 0.2f;
            flashRotation += 0.08f;

            // Spawn new particles
            if (Main.rand.NextFloat() < 0.3f)
            {
                SpawnImpactParticle();
            }

            // Update existing particles
            for (int i = particles.Count - 1; i >= 0; i--)
            {
                var p = particles[i];
                p.Age++;

                if (p.Age >= p.Lifetime)
                {
                    particles.RemoveAt(i);
                    continue;
                }

                p.Velocity.Y += 0.2f;
                p.Position += p.Velocity;
                p.Rotation += p.RotationSpeed;

                float lifeProgress = p.Age / (float)p.Lifetime;
                p.Alpha = 1f - lifeProgress;
                p.Scale = 1f - lifeProgress * 0.5f;
            }
        }

        private void SpawnImpactParticle()
        {
            float angle = normal.ToRotation() + MathHelper.ToRadians(Main.rand.NextFloat(-60f, 60f));
            float speed = Main.rand.NextFloat(2f, 6f);

            particles.Add(new ImpactParticle
            {
                Position = position + Main.rand.NextVector2Circular(5f, 5f),
                Velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed,
                Rotation = Main.rand.NextFloat(0f, MathHelper.TwoPi),
                RotationSpeed = Main.rand.NextFloat(-0.2f, 0.2f),
                Scale = Main.rand.NextFloat(0.5f, 1.5f),
                Alpha = 1f,
                Lifetime = Main.rand.Next(15, 30),
                Age = 0
            });
        }

        public void Draw(SpriteBatch spriteBatch, Color baseColor)
        {
            Vector2 drawPos = position - Main.screenPosition;

            // Layer 1: Large background glow
            spriteBatch.Draw(
                glowTexture,
                drawPos,
                null,
                baseColor * intensity * 0.3f,
                0f,
                new Vector2(glowTexture.Width, glowTexture.Height) * 0.5f,
                3f,
                SpriteEffects.None,
                0f
            );

            // Layer 2: Rotating flash (X-shape)
            spriteBatch.Draw(
                flashTexture,
                drawPos,
                null,
                Color.White * intensity,
                flashRotation,
                new Vector2(flashTexture.Width, flashTexture.Height) * 0.5f,
                flashScale * 2f,
                SpriteEffects.None,
                0f
            );

            // Layer 3: Counter-rotating flash
            spriteBatch.Draw(
                flashTexture,
                drawPos,
                null,
                baseColor * intensity * 0.7f,
                -flashRotation * 1.5f,
                new Vector2(flashTexture.Width, flashTexture.Height) * 0.5f,
                flashScale * 1.5f,
                SpriteEffects.None,
                0f
            );

            // Layer 4: Impact particles (sparks)
            foreach (var particle in particles)
            {
                spriteBatch.Draw(
                    sparkTexture,
                    particle.Position - Main.screenPosition,
                    null,
                    baseColor * particle.Alpha,
                    particle.Rotation,
                    new Vector2(sparkTexture.Width, sparkTexture.Height) * 0.5f,
                    particle.Scale,
                    SpriteEffects.None,
                    0f
                );
            }
        }

        private static Texture2D CreateStarFlash(GraphicsDevice device, int size, int points)
        {
            Texture2D texture = new Texture2D(device, size, size);
            Color[] data = new Color[size * size];

            Vector2 center = new Vector2(size * 0.5f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 pos = new Vector2(x, y) - center;
                    float angle = (float)Math.Atan2(pos.Y, pos.X);
                    float dist = pos.Length();
                    float maxDist = size * 0.5f;

                    float star = (float)Math.Abs(Math.Cos(angle * points));
                    star = (float)Math.Pow(star, 0.5f);

                    float distFactor = 1f - MathHelper.Clamp(dist / maxDist, 0f, 1f);
                    distFactor = (float)Math.Pow(distFactor, 1.5f);

                    float alpha = star * distFactor;
                    data[y * size + x] = Color.White * alpha;
                }
            }

            texture.SetData(data);
            return texture;
        }

        private static Texture2D CreateSparkTexture(GraphicsDevice device, int width, int height)
        {
            Texture2D texture = new Texture2D(device, width, height);
            Color[] data = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                float v = y / (float)height;
                float vFactor = 1f - Math.Abs(v - 0.5f) * 2f;
                vFactor = (float)Math.Pow(vFactor, 2);

                for (int x = 0; x < width; x++)
                {
                    float u = x / (float)width;
                    float uFactor = 1f - Math.Abs(u - 0.5f) * 2f;

                    float alpha = uFactor * vFactor;
                    data[y * width + x] = Color.White * alpha;
                }
            }

            texture.SetData(data);
            return texture;
        }

        private static Texture2D CreateRadialGlow(GraphicsDevice device, int size)
        {
            Texture2D texture = new Texture2D(device, size, size);
            Color[] data = new Color[size * size];

            Vector2 center = new Vector2(size * 0.5f);
            float maxDist = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    float alpha = 1f - MathHelper.Clamp(dist / maxDist, 0f, 1f);
                    alpha = (float)Math.Pow(alpha, 1.5f);
                    data[y * size + x] = Color.White * alpha;
                }
            }

            texture.SetData(data);
            return texture;
        }
    }
}
