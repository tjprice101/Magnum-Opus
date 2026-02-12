using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX.Effects
{
    /// <summary>
    /// Glow pixel dust system with multiple behavior types.
    /// 
    /// Inspired by VFX+ GlowPixel dusts - provides glowing pixel particles
    /// with various movement behaviors.
    /// 
    /// Dust Types:
    /// - GlowPixel: Standard glow pixel with gravity
    /// - GlowPixelFast: Fast-moving glow pixel
    /// - GlowPixelRise: Rising glow pixel (defies gravity)
    /// - GlowPixelCross: Cross-shaped glow (4 pixels in + pattern)
    /// - CirclePulse: Expanding ring that fades out
    /// 
    /// Usage:
    ///   GlowDustSystem.SpawnGlowPixel(pos, vel, color, scale, lifetime);
    ///   GlowDustSystem.SpawnCirclePulse(center, color, maxRadius, lifetime);
    /// </summary>
    public class GlowDustSystem : ModSystem
    {
        private static List<GlowDust> activeDusts = new List<GlowDust>(500);
        private static List<CirclePulse> activePulses = new List<CirclePulse>(50);
        private static Texture2D pixelTexture;

        public override void Load()
        {
            On_Main.DrawDust += DrawGlowDusts;
        }

        public override void Unload()
        {
            On_Main.DrawDust -= DrawGlowDusts;
            activeDusts?.Clear();
            activePulses?.Clear();
            pixelTexture?.Dispose();
            pixelTexture = null;
        }

        public override void PostUpdateDusts()
        {
            // Update glow dusts
            for (int i = activeDusts.Count - 1; i >= 0; i--)
            {
                var dust = activeDusts[i];
                dust.Update();

                if (dust.IsDead)
                {
                    activeDusts.RemoveAt(i);
                }
            }

            // Update circle pulses
            for (int i = activePulses.Count - 1; i >= 0; i--)
            {
                var pulse = activePulses[i];
                pulse.Update();

                if (pulse.IsDead)
                {
                    activePulses.RemoveAt(i);
                }
            }
        }

        #region Spawn Methods

        /// <summary>
        /// Spawn a standard glow pixel.
        /// </summary>
        public static void SpawnGlowPixel(Vector2 position, Vector2 velocity, Color color, float scale = 1f, int lifetime = 30)
        {
            if (activeDusts.Count >= 500) return;

            activeDusts.Add(new GlowDust
            {
                Position = position,
                Velocity = velocity,
                Color = color,
                Scale = scale,
                Lifetime = lifetime,
                Age = 0,
                Type = GlowDustType.Standard,
                Gravity = 0.1f,
                Drag = 0.98f
            });
        }

        /// <summary>
        /// Spawn a fast glow pixel.
        /// </summary>
        public static void SpawnGlowPixelFast(Vector2 position, Vector2 velocity, Color color, float scale = 1f, int lifetime = 20)
        {
            if (activeDusts.Count >= 500) return;

            activeDusts.Add(new GlowDust
            {
                Position = position,
                Velocity = velocity * 1.5f,
                Color = color,
                Scale = scale,
                Lifetime = lifetime,
                Age = 0,
                Type = GlowDustType.Fast,
                Gravity = 0.05f,
                Drag = 0.99f
            });
        }

        /// <summary>
        /// Spawn a rising glow pixel (defies gravity).
        /// </summary>
        public static void SpawnGlowPixelRise(Vector2 position, Vector2 velocity, Color color, float scale = 1f, int lifetime = 40)
        {
            if (activeDusts.Count >= 500) return;

            activeDusts.Add(new GlowDust
            {
                Position = position,
                Velocity = velocity + new Vector2(0, -1f),
                Color = color,
                Scale = scale,
                Lifetime = lifetime,
                Age = 0,
                Type = GlowDustType.Rise,
                Gravity = -0.08f, // Negative gravity = rises
                Drag = 0.97f
            });
        }

        /// <summary>
        /// Spawn a cross-shaped glow (4 pixels in + pattern).
        /// </summary>
        public static void SpawnGlowPixelCross(Vector2 position, Vector2 velocity, Color color, float scale = 1f, int lifetime = 25)
        {
            if (activeDusts.Count >= 500) return;

            activeDusts.Add(new GlowDust
            {
                Position = position,
                Velocity = velocity,
                Color = color,
                Scale = scale,
                Lifetime = lifetime,
                Age = 0,
                Type = GlowDustType.Cross,
                Gravity = 0.1f,
                Drag = 0.98f
            });
        }

        /// <summary>
        /// Spawn an expanding circle pulse.
        /// </summary>
        public static void SpawnCirclePulse(Vector2 center, Color color, float maxRadius = 100f, int lifetime = 30, float thickness = 3f)
        {
            if (activePulses.Count >= 50) return;

            activePulses.Add(new CirclePulse
            {
                Center = center,
                Color = color,
                MaxRadius = maxRadius,
                Lifetime = lifetime,
                Age = 0,
                Thickness = thickness
            });
        }

        /// <summary>
        /// Spawn a burst of glow pixels.
        /// </summary>
        public static void SpawnGlowBurst(Vector2 center, Color color, int count = 12, float speed = 5f, float scale = 1f, int lifetime = 25)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                Vector2 vel = angle.ToRotationVector2() * speed * Main.rand.NextFloat(0.8f, 1.2f);
                SpawnGlowPixel(center, vel, color, scale, lifetime);
            }
        }

        /// <summary>
        /// Spawn rising glow particles (like embers).
        /// </summary>
        public static void SpawnGlowEmbers(Vector2 position, Color color, int count = 8, float spread = 20f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(spread, spread * 0.5f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-3f, -1f));
                SpawnGlowPixelRise(position + offset, vel, color, Main.rand.NextFloat(0.5f, 1f), Main.rand.Next(30, 50));
            }
        }

        #endregion

        #region Drawing

        private void DrawGlowDusts(On_Main.orig_DrawDust orig, Main self)
        {
            orig(self);

            if (Main.dedServ || (activeDusts.Count == 0 && activePulses.Count == 0))
                return;

            EnsurePixelTexture();

            SpriteBatch sb = Main.spriteBatch;

            // Draw with additive blending
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Draw circle pulses first (behind dusts)
            foreach (var pulse in activePulses)
            {
                DrawCirclePulse(sb, pulse);
            }

            // Draw glow dusts
            foreach (var dust in activeDusts)
            {
                DrawGlowDust(sb, dust);
            }

            sb.End();
        }

        private void DrawGlowDust(SpriteBatch sb, GlowDust dust)
        {
            float progress = (float)dust.Age / dust.Lifetime;
            float alpha = 1f - progress;
            float scale = dust.Scale * (1f - progress * 0.3f);

            Vector2 drawPos = dust.Position - Main.screenPosition;
            Color color = dust.Color * alpha;

            // Remove alpha for additive blending
            color = new Color(color.R, color.G, color.B, 0);

            switch (dust.Type)
            {
                case GlowDustType.Cross:
                    // Draw 5 pixels in + pattern
                    DrawPixel(sb, drawPos, color, scale);
                    DrawPixel(sb, drawPos + new Vector2(scale * 2, 0), color, scale);
                    DrawPixel(sb, drawPos + new Vector2(-scale * 2, 0), color, scale);
                    DrawPixel(sb, drawPos + new Vector2(0, scale * 2), color, scale);
                    DrawPixel(sb, drawPos + new Vector2(0, -scale * 2), color, scale);
                    break;

                default:
                    // Single pixel with multi-layer bloom
                    DrawPixel(sb, drawPos, color * 0.3f, scale * 3f);
                    DrawPixel(sb, drawPos, color * 0.5f, scale * 2f);
                    DrawPixel(sb, drawPos, color * 0.8f, scale * 1.2f);
                    DrawPixel(sb, drawPos, color, scale);
                    break;
            }
        }

        private void DrawCirclePulse(SpriteBatch sb, CirclePulse pulse)
        {
            float progress = (float)pulse.Age / pulse.Lifetime;
            float radius = pulse.MaxRadius * EaseOutQuad(progress);
            float alpha = 1f - progress;

            Vector2 center = pulse.Center - Main.screenPosition;
            Color color = new Color(pulse.Color.R, pulse.Color.G, pulse.Color.B, 0) * alpha;

            // Draw ring as series of points
            int segments = Math.Max(16, (int)(radius * 0.5f));
            for (int i = 0; i < segments; i++)
            {
                float angle = MathHelper.TwoPi * i / segments;
                Vector2 offset = angle.ToRotationVector2() * radius;

                // Draw with bloom layers
                DrawPixel(sb, center + offset, color * 0.3f, pulse.Thickness * 2f);
                DrawPixel(sb, center + offset, color * 0.6f, pulse.Thickness * 1.3f);
                DrawPixel(sb, center + offset, color, pulse.Thickness);
            }
        }

        private void DrawPixel(SpriteBatch sb, Vector2 position, Color color, float scale)
        {
            sb.Draw(pixelTexture, position, null, color, 0f, new Vector2(0.5f, 0.5f), scale, SpriteEffects.None, 0f);
        }

        private static void EnsurePixelTexture()
        {
            if (pixelTexture == null || pixelTexture.IsDisposed)
            {
                pixelTexture = new Texture2D(Main.instance.GraphicsDevice, 1, 1);
                pixelTexture.SetData(new[] { Color.White });
            }
        }

        private static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);

        #endregion
    }

    #region Data Classes

    internal class GlowDust
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Color Color;
        public float Scale;
        public int Lifetime;
        public int Age;
        public GlowDustType Type;
        public float Gravity;
        public float Drag;

        public bool IsDead => Age >= Lifetime;

        public void Update()
        {
            Position += Velocity;
            Velocity.Y += Gravity;
            Velocity *= Drag;
            Age++;
        }
    }

    internal class CirclePulse
    {
        public Vector2 Center;
        public Color Color;
        public float MaxRadius;
        public int Lifetime;
        public int Age;
        public float Thickness;

        public bool IsDead => Age >= Lifetime;

        public void Update()
        {
            Age++;
        }
    }

    public enum GlowDustType
    {
        Standard,
        Fast,
        Rise,
        Cross
    }

    #endregion
}
