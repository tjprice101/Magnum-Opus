using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX.Effects
{
    /// <summary>
    /// High-resolution smoke particle system with soft glow underlayer.
    /// 
    /// Inspired by VFX+ HighResSmoke patterns - provides soft, billowing
    /// smoke effects with multi-layer rendering.
    /// 
    /// Features:
    /// - Soft glow underlayer for volumetric look
    /// - Multiple smoke variants (wispy, thick, fire smoke)
    /// - Turbulence simulation
    /// - Auto-rotation for natural movement
    /// 
    /// Usage:
    ///   HighResSmokeSystem.SpawnSmoke(pos, vel, color, scale, lifetime);
    ///   HighResSmokeSystem.SpawnFireSmoke(pos, vel, scale);
    ///   HighResSmokeSystem.SpawnSmokeCloud(center, color, count, spread);
    /// </summary>
    public class HighResSmokeSystem : ModSystem
    {
        private static List<SmokeParticle> activeSmoke = new List<SmokeParticle>(200);
        private static Texture2D smokeTexture;
        private static Texture2D glowTexture;

        public override void Load()
        {
            On_Main.DrawDust += DrawSmoke;
        }

        public override void Unload()
        {
            On_Main.DrawDust -= DrawSmoke;
            activeSmoke?.Clear();
            smokeTexture?.Dispose();
            glowTexture?.Dispose();
            smokeTexture = null;
            glowTexture = null;
        }

        public override void PostUpdateDusts()
        {
            for (int i = activeSmoke.Count - 1; i >= 0; i--)
            {
                var smoke = activeSmoke[i];
                smoke.Update();

                if (smoke.IsDead)
                {
                    activeSmoke.RemoveAt(i);
                }
            }
        }

        #region Spawn Methods

        /// <summary>
        /// Spawn a standard smoke particle.
        /// </summary>
        public static void SpawnSmoke(Vector2 position, Vector2 velocity, Color color, float scale = 1f, int lifetime = 60)
        {
            if (activeSmoke.Count >= 200) return;

            activeSmoke.Add(new SmokeParticle
            {
                Position = position,
                Velocity = velocity,
                Color = color,
                Scale = scale,
                Lifetime = lifetime,
                Age = 0,
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi),
                RotationSpeed = Main.rand.NextFloat(-0.03f, 0.03f),
                TurbulenceOffset = Main.rand.NextFloat(MathHelper.TwoPi),
                TurbulenceStrength = Main.rand.NextFloat(0.3f, 0.8f),
                Type = SmokeType.Standard,
                GrowthRate = Main.rand.NextFloat(0.01f, 0.03f),
                FadeInDuration = 0.15f
            });
        }

        /// <summary>
        /// Spawn wispy, fast-dissipating smoke.
        /// </summary>
        public static void SpawnWispySmoke(Vector2 position, Vector2 velocity, Color color, float scale = 0.8f, int lifetime = 40)
        {
            if (activeSmoke.Count >= 200) return;

            activeSmoke.Add(new SmokeParticle
            {
                Position = position,
                Velocity = velocity * 1.3f,
                Color = color * 0.6f,
                Scale = scale,
                Lifetime = lifetime,
                Age = 0,
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi),
                RotationSpeed = Main.rand.NextFloat(-0.05f, 0.05f),
                TurbulenceOffset = Main.rand.NextFloat(MathHelper.TwoPi),
                TurbulenceStrength = Main.rand.NextFloat(0.5f, 1.2f),
                Type = SmokeType.Wispy,
                GrowthRate = Main.rand.NextFloat(0.02f, 0.05f),
                FadeInDuration = 0.1f
            });
        }

        /// <summary>
        /// Spawn thick, slow-moving smoke.
        /// </summary>
        public static void SpawnThickSmoke(Vector2 position, Vector2 velocity, Color color, float scale = 1.5f, int lifetime = 90)
        {
            if (activeSmoke.Count >= 200) return;

            activeSmoke.Add(new SmokeParticle
            {
                Position = position,
                Velocity = velocity * 0.5f,
                Color = color,
                Scale = scale,
                Lifetime = lifetime,
                Age = 0,
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi),
                RotationSpeed = Main.rand.NextFloat(-0.02f, 0.02f),
                TurbulenceOffset = Main.rand.NextFloat(MathHelper.TwoPi),
                TurbulenceStrength = Main.rand.NextFloat(0.2f, 0.4f),
                Type = SmokeType.Thick,
                GrowthRate = Main.rand.NextFloat(0.005f, 0.015f),
                FadeInDuration = 0.2f
            });
        }

        /// <summary>
        /// Spawn fire smoke (black/gray with orange glow underlayer).
        /// </summary>
        public static void SpawnFireSmoke(Vector2 position, Vector2 velocity, float scale = 1f)
        {
            if (activeSmoke.Count >= 200) return;

            activeSmoke.Add(new SmokeParticle
            {
                Position = position,
                Velocity = velocity + new Vector2(0, -1.5f), // Rise faster
                Color = new Color(40, 35, 30), // Dark smoke
                GlowColor = new Color(255, 120, 30) * 0.4f, // Orange glow
                Scale = scale,
                Lifetime = 70,
                Age = 0,
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi),
                RotationSpeed = Main.rand.NextFloat(-0.03f, 0.03f),
                TurbulenceOffset = Main.rand.NextFloat(MathHelper.TwoPi),
                TurbulenceStrength = Main.rand.NextFloat(0.4f, 0.9f),
                Type = SmokeType.Fire,
                GrowthRate = Main.rand.NextFloat(0.015f, 0.035f),
                FadeInDuration = 0.1f
            });
        }

        /// <summary>
        /// Spawn a cloud of smoke particles.
        /// </summary>
        public static void SpawnSmokeCloud(Vector2 center, Color color, int count = 8, float spread = 30f, float scale = 1f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(spread, spread);
                Vector2 vel = offset.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 2f);
                vel.Y -= Main.rand.NextFloat(0.5f, 1.5f); // Slight upward bias

                SpawnSmoke(center + offset, vel, color * Main.rand.NextFloat(0.6f, 1f),
                    scale * Main.rand.NextFloat(0.7f, 1.3f), Main.rand.Next(50, 80));
            }
        }

        /// <summary>
        /// Spawn smoke trail (for projectiles).
        /// </summary>
        public static void SpawnSmokeTrail(Vector2 position, Vector2 oppositeVelocity, Color color, float scale = 0.6f)
        {
            SpawnWispySmoke(position, oppositeVelocity * 0.3f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                color, scale, 35);
        }

        /// <summary>
        /// Spawn impact smoke burst.
        /// </summary>
        public static void SpawnImpactSmoke(Vector2 center, Color color, int count = 12, float scale = 1f)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.3f, 0.3f);
                float speed = Main.rand.NextFloat(1f, 4f);
                Vector2 vel = angle.ToRotationVector2() * speed;

                SpawnSmoke(center, vel, color * Main.rand.NextFloat(0.5f, 0.9f),
                    scale * Main.rand.NextFloat(0.8f, 1.5f), Main.rand.Next(40, 70));
            }
        }

        #endregion

        #region Drawing

        private void DrawSmoke(On_Main.orig_DrawDust orig, Main self)
        {
            orig(self);

            if (Main.dedServ || activeSmoke.Count == 0)
                return;

            EnsureTextures();

            SpriteBatch sb = Main.spriteBatch;

            // Draw glow underlayers first (additive)
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            foreach (var smoke in activeSmoke)
            {
                DrawSmokeGlow(sb, smoke);
            }

            sb.End();

            // Draw smoke particles (alpha blend for solid look)
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            foreach (var smoke in activeSmoke)
            {
                DrawSmokeParticle(sb, smoke);
            }

            sb.End();
        }

        private void DrawSmokeGlow(SpriteBatch sb, SmokeParticle smoke)
        {
            float progress = (float)smoke.Age / smoke.Lifetime;
            float alpha = CalculateAlpha(progress, smoke.FadeInDuration);
            float scale = smoke.Scale * (1f + progress * smoke.GrowthRate * smoke.Lifetime);

            Vector2 drawPos = smoke.Position - Main.screenPosition;
            Color glowColor = smoke.GlowColor != default ? smoke.GlowColor : smoke.Color;
            glowColor = new Color(glowColor.R, glowColor.G, glowColor.B, 0) * alpha * 0.4f;

            Vector2 origin = glowTexture.Size() * 0.5f;

            // Draw glow underlayer (larger, softer)
            sb.Draw(glowTexture, drawPos, null, glowColor, smoke.Rotation,
                origin, scale * 1.5f, SpriteEffects.None, 0f);
        }

        private void DrawSmokeParticle(SpriteBatch sb, SmokeParticle smoke)
        {
            float progress = (float)smoke.Age / smoke.Lifetime;
            float alpha = CalculateAlpha(progress, smoke.FadeInDuration);
            float scale = smoke.Scale * (1f + progress * smoke.GrowthRate * smoke.Lifetime);

            Vector2 drawPos = smoke.Position - Main.screenPosition;
            Color color = smoke.Color * alpha;

            Vector2 origin = smokeTexture.Size() * 0.5f;

            sb.Draw(smokeTexture, drawPos, null, color, smoke.Rotation,
                origin, scale, SpriteEffects.None, 0f);
        }

        private static float CalculateAlpha(float progress, float fadeInDuration)
        {
            // Fade in during first portion
            if (progress < fadeInDuration)
                return progress / fadeInDuration;

            // Fade out during rest
            return 1f - ((progress - fadeInDuration) / (1f - fadeInDuration));
        }

        private static void EnsureTextures()
        {
            if (smokeTexture == null || smokeTexture.IsDisposed)
            {
                smokeTexture = GenerateSmokeTexture(64);
            }

            if (glowTexture == null || glowTexture.IsDisposed)
            {
                glowTexture = GenerateGlowTexture(64);
            }
        }

        /// <summary>
        /// Generate a procedural smoke texture (soft circle with noise).
        /// </summary>
        private static Texture2D GenerateSmokeTexture(int size)
        {
            Texture2D tex = new Texture2D(Main.instance.GraphicsDevice, size, size);
            Color[] data = new Color[size * size];

            float center = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = MathF.Sqrt(dx * dx + dy * dy) / center;

                    // Soft falloff with some noise
                    float alpha = MathHelper.Clamp(1f - dist, 0f, 1f);
                    alpha = alpha * alpha; // Quadratic falloff for softer edge

                    // Add slight noise for texture
                    float noise = 0.8f + 0.2f * ((float)((x * 7 + y * 13) % 17) / 17f);
                    alpha *= noise;

                    byte a = (byte)(alpha * 255);
                    data[y * size + x] = new Color(255, 255, 255, a);
                }
            }

            tex.SetData(data);
            return tex;
        }

        /// <summary>
        /// Generate a procedural glow texture (radial gradient).
        /// </summary>
        private static Texture2D GenerateGlowTexture(int size)
        {
            Texture2D tex = new Texture2D(Main.instance.GraphicsDevice, size, size);
            Color[] data = new Color[size * size];

            float center = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = MathF.Sqrt(dx * dx + dy * dy) / center;

                    float alpha = MathHelper.Clamp(1f - dist, 0f, 1f);
                    alpha = alpha * alpha * alpha; // Cubic falloff for very soft glow

                    byte a = (byte)(alpha * 255);
                    data[y * size + x] = new Color(255, 255, 255, a);
                }
            }

            tex.SetData(data);
            return tex;
        }

        #endregion
    }

    #region Data Class

    internal class SmokeParticle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Color Color;
        public Color GlowColor;
        public float Scale;
        public int Lifetime;
        public int Age;
        public float Rotation;
        public float RotationSpeed;
        public float TurbulenceOffset;
        public float TurbulenceStrength;
        public SmokeType Type;
        public float GrowthRate;
        public float FadeInDuration;

        public bool IsDead => Age >= Lifetime;

        public void Update()
        {
            // Apply turbulence
            float time = Age * 0.05f + TurbulenceOffset;
            Vector2 turbulence = new Vector2(
                MathF.Sin(time * 2.3f) * TurbulenceStrength,
                MathF.Cos(time * 1.7f) * TurbulenceStrength * 0.5f
            );

            Position += Velocity + turbulence;

            // Slow down and rise
            Velocity *= 0.98f;
            if (Type != SmokeType.Fire)
                Velocity.Y -= 0.02f; // Slight rise
            else
                Velocity.Y -= 0.05f; // Fire smoke rises faster

            Rotation += RotationSpeed;
            Age++;
        }
    }

    public enum SmokeType
    {
        Standard,
        Wispy,
        Thick,
        Fire
    }

    #endregion
}
