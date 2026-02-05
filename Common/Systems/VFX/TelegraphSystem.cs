using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// Visual Telegraph System for boss attacks.
    /// 
    /// Provides warning indicators so players can react to incoming attacks:
    /// - Threat Lines: Show projectile trajectories
    /// - Warning Zones: Mark danger areas
    /// - Converging Particles: Indicate charge-up
    /// - Safe Zone Markers: Show escape routes
    /// 
    /// This creates fair, readable combat while maintaining visual spectacle.
    /// </summary>
    public class TelegraphSystem : ModSystem
    {
        #region Static Singleton
        
        private static TelegraphSystem _instance;
        public static TelegraphSystem Instance => _instance;
        
        #endregion

        #region Telegraph Data Structures
        
        public enum TelegraphType
        {
            ThreatLine,         // Line showing projectile path
            DangerZone,         // Circular danger area
            SafeZone,           // Safe area (cyan)
            ConvergingRing,     // Charge-up indicator
            LaserPath,          // Beam trajectory
            ImpactPoint,        // Ground impact warning
            ArcPath,            // Curved attack path
            SectorCone          // Cone-shaped danger area
        }
        
        private struct ActiveTelegraph
        {
            public TelegraphType Type;
            public Vector2 Position;
            public Vector2 Direction;
            public float Size;
            public float Duration;
            public float Timer;
            public Color Color;
            public float Intensity;
            public float Angle; // For sectors/cones
            public Vector2 EndPosition; // For lines/arcs
        }
        
        private List<ActiveTelegraph> _activeTelegraphs = new List<ActiveTelegraph>(64);
        
        #endregion

        #region Lifecycle
        
        public override void Load()
        {
            _instance = this;
            On_Main.DrawDust += DrawTelegraphs;
        }
        
        public override void Unload()
        {
            On_Main.DrawDust -= DrawTelegraphs;
            _instance = null;
            _activeTelegraphs?.Clear();
        }
        
        public override void PostUpdateEverything()
        {
            UpdateTelegraphs();
        }
        
        #endregion

        #region Public API - Spawning Telegraphs
        
        /// <summary>
        /// Shows a line indicating where a projectile will travel.
        /// </summary>
        /// <param name="start">Start position</param>
        /// <param name="direction">Direction (will be normalized)</param>
        /// <param name="length">Line length in pixels</param>
        /// <param name="duration">How long to show (frames)</param>
        /// <param name="color">Line color (default red)</param>
        /// <param name="intensity">Brightness/size multiplier</param>
        public static void ThreatLine(Vector2 start, Vector2 direction, float length, int duration, Color? color = null, float intensity = 1f)
        {
            if (_instance == null) return;
            
            direction = direction.SafeNormalize(Vector2.UnitX);
            
            _instance._activeTelegraphs.Add(new ActiveTelegraph
            {
                Type = TelegraphType.ThreatLine,
                Position = start,
                Direction = direction,
                Size = length,
                Duration = duration,
                Timer = 0,
                Color = color ?? new Color(255, 50, 50),
                Intensity = intensity,
                EndPosition = start + direction * length
            });
        }
        
        /// <summary>
        /// Shows a circular danger zone.
        /// </summary>
        /// <param name="center">Center of danger zone</param>
        /// <param name="radius">Radius of danger zone</param>
        /// <param name="duration">How long to show</param>
        /// <param name="color">Zone color (default red)</param>
        public static void DangerZone(Vector2 center, float radius, int duration, Color? color = null)
        {
            if (_instance == null) return;
            
            _instance._activeTelegraphs.Add(new ActiveTelegraph
            {
                Type = TelegraphType.DangerZone,
                Position = center,
                Size = radius,
                Duration = duration,
                Timer = 0,
                Color = color ?? new Color(255, 50, 50, 150),
                Intensity = 1f
            });
        }
        
        /// <summary>
        /// Shows a safe zone where the player should move.
        /// </summary>
        /// <param name="center">Center of safe zone</param>
        /// <param name="radius">Radius of safe zone</param>
        /// <param name="duration">How long to show</param>
        public static void SafeZone(Vector2 center, float radius, int duration)
        {
            if (_instance == null) return;
            
            _instance._activeTelegraphs.Add(new ActiveTelegraph
            {
                Type = TelegraphType.SafeZone,
                Position = center,
                Size = radius,
                Duration = duration,
                Timer = 0,
                Color = new Color(50, 200, 255, 150),
                Intensity = 1f
            });
        }
        
        /// <summary>
        /// Shows a converging ring for charge-up attacks.
        /// Ring shrinks toward center as attack charges.
        /// </summary>
        /// <param name="center">Center position</param>
        /// <param name="maxRadius">Starting radius</param>
        /// <param name="duration">Time to fully converge</param>
        /// <param name="color">Ring color</param>
        public static void ConvergingRing(Vector2 center, float maxRadius, int duration, Color? color = null)
        {
            if (_instance == null) return;
            
            _instance._activeTelegraphs.Add(new ActiveTelegraph
            {
                Type = TelegraphType.ConvergingRing,
                Position = center,
                Size = maxRadius,
                Duration = duration,
                Timer = 0,
                Color = color ?? new Color(255, 200, 50),
                Intensity = 1f
            });
        }
        
        /// <summary>
        /// Shows where a laser/beam will fire.
        /// </summary>
        /// <param name="start">Beam origin</param>
        /// <param name="end">Beam endpoint</param>
        /// <param name="width">Beam width</param>
        /// <param name="duration">Warning duration</param>
        /// <param name="color">Beam color</param>
        public static void LaserPath(Vector2 start, Vector2 end, float width, int duration, Color? color = null)
        {
            if (_instance == null) return;
            
            _instance._activeTelegraphs.Add(new ActiveTelegraph
            {
                Type = TelegraphType.LaserPath,
                Position = start,
                EndPosition = end,
                Size = width,
                Duration = duration,
                Timer = 0,
                Color = color ?? new Color(255, 100, 100, 180),
                Intensity = 1f
            });
        }
        
        /// <summary>
        /// Shows where a ground slam or dive attack will land.
        /// </summary>
        /// <param name="position">Impact center</param>
        /// <param name="radius">Impact radius</param>
        /// <param name="duration">Warning duration</param>
        public static void ImpactPoint(Vector2 position, float radius, int duration)
        {
            if (_instance == null) return;
            
            _instance._activeTelegraphs.Add(new ActiveTelegraph
            {
                Type = TelegraphType.ImpactPoint,
                Position = position,
                Size = radius,
                Duration = duration,
                Timer = 0,
                Color = new Color(255, 150, 50),
                Intensity = 1f
            });
        }
        
        /// <summary>
        /// Shows a cone-shaped danger zone (for sweeping attacks).
        /// </summary>
        /// <param name="origin">Cone apex</param>
        /// <param name="direction">Cone center direction</param>
        /// <param name="angle">Half-angle of cone in radians</param>
        /// <param name="length">Cone length</param>
        /// <param name="duration">Warning duration</param>
        /// <param name="color">Cone color</param>
        public static void SectorCone(Vector2 origin, Vector2 direction, float angle, float length, int duration, Color? color = null)
        {
            if (_instance == null) return;
            
            _instance._activeTelegraphs.Add(new ActiveTelegraph
            {
                Type = TelegraphType.SectorCone,
                Position = origin,
                Direction = direction.SafeNormalize(Vector2.UnitX),
                Angle = angle,
                Size = length,
                Duration = duration,
                Timer = 0,
                Color = color ?? new Color(255, 80, 80, 150),
                Intensity = 1f
            });
        }
        
        /// <summary>
        /// Clears all active telegraphs (e.g., when boss dies).
        /// </summary>
        public static void ClearAll()
        {
            _instance?._activeTelegraphs.Clear();
        }
        
        #endregion

        #region Update & Rendering
        
        private void UpdateTelegraphs()
        {
            for (int i = _activeTelegraphs.Count - 1; i >= 0; i--)
            {
                var telegraph = _activeTelegraphs[i];
                telegraph.Timer++;
                
                if (telegraph.Timer >= telegraph.Duration)
                {
                    _activeTelegraphs.RemoveAt(i);
                }
                else
                {
                    _activeTelegraphs[i] = telegraph;
                }
            }
        }
        
        private void DrawTelegraphs(On_Main.orig_DrawDust orig, Main self)
        {
            orig(self);
            
            if (_activeTelegraphs.Count == 0)
                return;
                
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, 
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, 
                null, Main.GameViewMatrix.TransformationMatrix);
            
            try
            {
                Texture2D pixel = MagnumTextureRegistry.GetPixelTexture();
                
                foreach (var telegraph in _activeTelegraphs)
                {
                    float progress = telegraph.Timer / telegraph.Duration;
                    float alpha = GetAlphaForProgress(progress);
                    
                    switch (telegraph.Type)
                    {
                        case TelegraphType.ThreatLine:
                            DrawThreatLine(pixel, telegraph, alpha);
                            break;
                            
                        case TelegraphType.DangerZone:
                            DrawDangerZone(pixel, telegraph, alpha);
                            break;
                            
                        case TelegraphType.SafeZone:
                            DrawSafeZone(pixel, telegraph, alpha);
                            break;
                            
                        case TelegraphType.ConvergingRing:
                            DrawConvergingRing(pixel, telegraph, progress, alpha);
                            break;
                            
                        case TelegraphType.LaserPath:
                            DrawLaserPath(pixel, telegraph, progress, alpha);
                            break;
                            
                        case TelegraphType.ImpactPoint:
                            DrawImpactPoint(pixel, telegraph, progress, alpha);
                            break;
                            
                        case TelegraphType.SectorCone:
                            DrawSectorCone(pixel, telegraph, alpha);
                            break;
                    }
                }
            }
            finally
            {
                Main.spriteBatch.End();
            }
        }
        
        private float GetAlphaForProgress(float progress)
        {
            // Pulse effect - brighter as attack approaches
            float pulse = 0.5f + (float)Math.Sin(progress * MathHelper.Pi * 8f) * 0.3f;
            float fadeIn = Math.Min(1f, progress * 4f); // Quick fade in
            float urgency = 0.3f + progress * 0.7f; // Gets more intense over time
            
            return fadeIn * pulse * urgency;
        }
        
        private void DrawThreatLine(Texture2D pixel, ActiveTelegraph telegraph, float alpha)
        {
            Vector2 start = telegraph.Position - Main.screenPosition;
            Vector2 end = telegraph.EndPosition - Main.screenPosition;
            
            float length = Vector2.Distance(start, end);
            float rotation = (end - start).ToRotation();
            
            // Draw dashed line
            int segments = (int)(length / 20f);
            for (int i = 0; i < segments; i += 2)
            {
                float t = i / (float)segments;
                Vector2 segStart = Vector2.Lerp(start, end, t);
                
                Main.spriteBatch.Draw(pixel, segStart, null, 
                    telegraph.Color * alpha * telegraph.Intensity,
                    rotation, Vector2.Zero, new Vector2(15f, 2f * telegraph.Intensity),
                    SpriteEffects.None, 0f);
            }
            
            // Draw arrow at end
            DrawArrow(pixel, end, telegraph.Direction, telegraph.Color * alpha, 10f * telegraph.Intensity);
        }
        
        private void DrawDangerZone(Texture2D pixel, ActiveTelegraph telegraph, float alpha)
        {
            Vector2 center = telegraph.Position - Main.screenPosition;
            
            // Draw ring
            int segments = 32;
            for (int i = 0; i < segments; i++)
            {
                float angle = MathHelper.TwoPi * i / segments;
                float nextAngle = MathHelper.TwoPi * (i + 1) / segments;
                
                Vector2 point1 = center + angle.ToRotationVector2() * telegraph.Size;
                Vector2 point2 = center + nextAngle.ToRotationVector2() * telegraph.Size;
                
                float lineAngle = (point2 - point1).ToRotation();
                float lineLength = Vector2.Distance(point1, point2);
                
                Main.spriteBatch.Draw(pixel, point1, null,
                    telegraph.Color * alpha,
                    lineAngle, Vector2.Zero, new Vector2(lineLength, 3f),
                    SpriteEffects.None, 0f);
            }
            
            // Draw X in center
            float xSize = 15f;
            Main.spriteBatch.Draw(pixel, center - new Vector2(xSize, xSize), null,
                telegraph.Color * alpha,
                MathHelper.PiOver4, Vector2.Zero, new Vector2(xSize * 2.8f, 2f),
                SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(pixel, center - new Vector2(-xSize, xSize), null,
                telegraph.Color * alpha,
                -MathHelper.PiOver4, Vector2.Zero, new Vector2(xSize * 2.8f, 2f),
                SpriteEffects.None, 0f);
        }
        
        private void DrawSafeZone(Texture2D pixel, ActiveTelegraph telegraph, float alpha)
        {
            Vector2 center = telegraph.Position - Main.screenPosition;
            
            // Draw pulsing safe ring
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.1f;
            
            int segments = 24;
            for (int i = 0; i < segments; i++)
            {
                float angle = MathHelper.TwoPi * i / segments;
                float nextAngle = MathHelper.TwoPi * (i + 1) / segments;
                
                Vector2 point1 = center + angle.ToRotationVector2() * telegraph.Size * pulse;
                Vector2 point2 = center + nextAngle.ToRotationVector2() * telegraph.Size * pulse;
                
                float lineAngle = (point2 - point1).ToRotation();
                float lineLength = Vector2.Distance(point1, point2);
                
                Main.spriteBatch.Draw(pixel, point1, null,
                    telegraph.Color * alpha,
                    lineAngle, Vector2.Zero, new Vector2(lineLength, 4f),
                    SpriteEffects.None, 0f);
            }
            
            // Draw checkmark in center
            Vector2 checkStart = center + new Vector2(-8f, 0f);
            Vector2 checkMid = center + new Vector2(-2f, 8f);
            Vector2 checkEnd = center + new Vector2(10f, -8f);
            
            DrawLine(pixel, checkStart, checkMid, telegraph.Color * alpha, 3f);
            DrawLine(pixel, checkMid, checkEnd, telegraph.Color * alpha, 3f);
        }
        
        private void DrawConvergingRing(Texture2D pixel, ActiveTelegraph telegraph, float progress, float alpha)
        {
            Vector2 center = telegraph.Position - Main.screenPosition;
            
            // Ring shrinks as progress increases
            float currentRadius = telegraph.Size * (1f - progress);
            
            int segments = 32;
            for (int i = 0; i < segments; i++)
            {
                float angle = MathHelper.TwoPi * i / segments;
                float nextAngle = MathHelper.TwoPi * (i + 1) / segments;
                
                Vector2 point1 = center + angle.ToRotationVector2() * currentRadius;
                Vector2 point2 = center + nextAngle.ToRotationVector2() * currentRadius;
                
                float lineAngle = (point2 - point1).ToRotation();
                float lineLength = Vector2.Distance(point1, point2);
                
                // Thickness increases as ring converges
                float thickness = 2f + progress * 4f;
                
                Main.spriteBatch.Draw(pixel, point1, null,
                    telegraph.Color * alpha * (0.5f + progress * 0.5f),
                    lineAngle, Vector2.Zero, new Vector2(lineLength, thickness),
                    SpriteEffects.None, 0f);
            }
            
            // Converging particle effect (spawn dust at ring edges)
            if (Main.GameUpdateCount % 3 == 0)
            {
                float particleAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 particlePos = telegraph.Position + particleAngle.ToRotationVector2() * currentRadius;
                Dust d = Dust.NewDustPerfect(particlePos, Terraria.ID.DustID.Torch, 
                    -particleAngle.ToRotationVector2() * 2f, 0, telegraph.Color, 0.8f);
                d.noGravity = true;
            }
        }
        
        private void DrawLaserPath(Texture2D pixel, ActiveTelegraph telegraph, float progress, float alpha)
        {
            Vector2 start = telegraph.Position - Main.screenPosition;
            Vector2 end = telegraph.EndPosition - Main.screenPosition;
            
            // Beam gets more solid as attack approaches
            float solidify = progress * 0.6f;
            
            // Draw beam outline
            float rotation = (end - start).ToRotation();
            float length = Vector2.Distance(start, end);
            
            // Outer glow
            Main.spriteBatch.Draw(pixel, start, null,
                telegraph.Color * alpha * 0.3f,
                rotation, new Vector2(0, 0.5f), new Vector2(length, telegraph.Size + 10f),
                SpriteEffects.None, 0f);
            
            // Main beam
            Main.spriteBatch.Draw(pixel, start, null,
                telegraph.Color * (alpha * 0.4f + solidify),
                rotation, new Vector2(0, 0.5f), new Vector2(length, telegraph.Size),
                SpriteEffects.None, 0f);
            
            // Bright core (appears near end)
            if (progress > 0.7f)
            {
                float coreAlpha = (progress - 0.7f) / 0.3f;
                Main.spriteBatch.Draw(pixel, start, null,
                    Color.White * coreAlpha * 0.8f,
                    rotation, new Vector2(0, 0.5f), new Vector2(length, telegraph.Size * 0.3f),
                    SpriteEffects.None, 0f);
            }
        }
        
        private void DrawImpactPoint(Texture2D pixel, ActiveTelegraph telegraph, float progress, float alpha)
        {
            Vector2 center = telegraph.Position - Main.screenPosition;
            
            // Crosshairs that converge
            float currentRadius = telegraph.Size * (1f - progress * 0.5f);
            
            // Draw 4 converging lines
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.PiOver2 * i + MathHelper.PiOver4;
                Vector2 lineStart = center + angle.ToRotationVector2() * currentRadius;
                Vector2 lineEnd = center + angle.ToRotationVector2() * (currentRadius * 0.3f);
                
                DrawLine(pixel, lineStart, lineEnd, telegraph.Color * alpha, 3f);
            }
            
            // Inner pulsing circle
            float innerPulse = 0.5f + (float)Math.Sin(progress * MathHelper.TwoPi * 4f) * 0.5f;
            DrawCircle(pixel, center, telegraph.Size * 0.2f * innerPulse, telegraph.Color * alpha * 0.5f, 2f);
            
            // Outer danger ring
            DrawCircle(pixel, center, telegraph.Size, telegraph.Color * alpha, 2f + progress * 2f);
        }
        
        private void DrawSectorCone(Texture2D pixel, ActiveTelegraph telegraph, float alpha)
        {
            Vector2 origin = telegraph.Position - Main.screenPosition;
            float centerAngle = telegraph.Direction.ToRotation();
            float halfAngle = telegraph.Angle;
            float length = telegraph.Size;
            
            // Draw cone edges
            Vector2 edge1 = origin + (centerAngle - halfAngle).ToRotationVector2() * length;
            Vector2 edge2 = origin + (centerAngle + halfAngle).ToRotationVector2() * length;
            
            DrawLine(pixel, origin, edge1, telegraph.Color * alpha, 2f);
            DrawLine(pixel, origin, edge2, telegraph.Color * alpha, 2f);
            
            // Draw arc at end
            int arcSegments = (int)(halfAngle * 10f) + 5;
            for (int i = 0; i < arcSegments; i++)
            {
                float t = i / (float)arcSegments;
                float angle1 = centerAngle - halfAngle + t * halfAngle * 2f;
                float angle2 = centerAngle - halfAngle + (t + 1f / arcSegments) * halfAngle * 2f;
                
                Vector2 point1 = origin + angle1.ToRotationVector2() * length;
                Vector2 point2 = origin + angle2.ToRotationVector2() * length;
                
                DrawLine(pixel, point1, point2, telegraph.Color * alpha, 2f);
            }
            
            // Fill with faded color (danger zone indication)
            // This would ideally use a shader, but for now just draw radial lines
            int fillLines = 8;
            for (int i = 1; i < fillLines; i++)
            {
                float t = i / (float)fillLines;
                float angle = centerAngle - halfAngle + t * halfAngle * 2f;
                Vector2 endPoint = origin + angle.ToRotationVector2() * length;
                
                DrawLine(pixel, origin, endPoint, telegraph.Color * alpha * 0.2f, 1f);
            }
        }
        
        #endregion

        #region Helper Drawing Methods
        
        private void DrawLine(Texture2D pixel, Vector2 start, Vector2 end, Color color, float thickness)
        {
            float rotation = (end - start).ToRotation();
            float length = Vector2.Distance(start, end);
            
            Main.spriteBatch.Draw(pixel, start, null, color,
                rotation, Vector2.Zero, new Vector2(length, thickness),
                SpriteEffects.None, 0f);
        }
        
        private void DrawCircle(Texture2D pixel, Vector2 center, float radius, Color color, float thickness)
        {
            int segments = Math.Max(16, (int)(radius / 3f));
            
            for (int i = 0; i < segments; i++)
            {
                float angle = MathHelper.TwoPi * i / segments;
                float nextAngle = MathHelper.TwoPi * (i + 1) / segments;
                
                Vector2 point1 = center + angle.ToRotationVector2() * radius;
                Vector2 point2 = center + nextAngle.ToRotationVector2() * radius;
                
                DrawLine(pixel, point1, point2, color, thickness);
            }
        }
        
        private void DrawArrow(Texture2D pixel, Vector2 tip, Vector2 direction, Color color, float size)
        {
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
            
            Vector2 back = tip - direction * size;
            Vector2 left = back + perpendicular * size * 0.5f;
            Vector2 right = back - perpendicular * size * 0.5f;
            
            DrawLine(pixel, tip, left, color, 2f);
            DrawLine(pixel, tip, right, color, 2f);
        }
        
        #endregion
    }
}
