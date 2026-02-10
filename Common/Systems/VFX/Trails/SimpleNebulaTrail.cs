using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using ReLogic.Content;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// SIMPLE NEBULA TRAIL - SpriteBatch-based implementation that works reliably.
    /// 
    /// Uses multiple layers of soft glow sprites at trail points to create
    /// a flowing nebula cloud effect. Much simpler than GPU vertex buffers
    /// but still creates beautiful layered cloud trails.
    /// </summary>
    public static class SimpleNebulaTrail
    {
        #region Constants
        
        private const int MAX_TRAILS = 10;
        private const int MAX_SEGMENTS = 30;
        private const float MAX_AGE = 0.5f; // Seconds
        private const float SEGMENT_DISTANCE = 12f;
        
        #endregion
        
        #region Trail Data
        
        private static List<TrailData> _trails = new List<TrailData>();
        
        private class TrailData
        {
            public List<TrailSegment> Segments = new List<TrailSegment>();
            public int OwnerId;
            public Color ColorHot;
            public Color ColorMid;
            public Color ColorCool;
            public float BaseWidth;
            public bool IsActive;
            public bool IsFading;
            public float FadeProgress;
            public float ScrollOffset;
        }
        
        private struct TrailSegment
        {
            public Vector2 Position;
            public float Rotation;
            public float Age;
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Updates the trail each frame during a swing.
        /// Call this from your projectile's AI() method.
        /// </summary>
        public static void UpdateTrail(Player player, Vector2 tipPosition, float rotation, 
            float width = 60f, string theme = null)
        {
            if (Main.dedServ) return;
            
            var (hot, mid, cool) = GetPalette(theme);
            
            var trail = FindOrCreateTrail(player.whoAmI, hot, mid, cool, width);
            trail.IsActive = true;
            trail.IsFading = false;
            
            // Add new segment if moved enough
            if (trail.Segments.Count == 0 || 
                Vector2.DistanceSquared(trail.Segments[^1].Position, tipPosition) > SEGMENT_DISTANCE * SEGMENT_DISTANCE)
            {
                if (trail.Segments.Count >= MAX_SEGMENTS)
                    trail.Segments.RemoveAt(0);
                
                trail.Segments.Add(new TrailSegment
                {
                    Position = tipPosition,
                    Rotation = rotation,
                    Age = 0f
                });
            }
            
            // Age existing segments
            float dt = 1f / 60f;
            for (int i = 0; i < trail.Segments.Count; i++)
            {
                var seg = trail.Segments[i];
                seg.Age += dt;
                trail.Segments[i] = seg;
            }
            
            // Remove old segments
            trail.Segments.RemoveAll(s => s.Age > MAX_AGE);
            
            // Scroll effect
            trail.ScrollOffset += 0.05f;
        }
        
        /// <summary>
        /// Ends the trail (starts fading).
        /// </summary>
        public static void EndTrail(Player player)
        {
            foreach (var trail in _trails)
            {
                if (trail.OwnerId == player.whoAmI && trail.IsActive)
                {
                    trail.IsFading = true;
                    trail.FadeProgress = 0f;
                }
            }
        }
        
        /// <summary>
        /// Clears all trails.
        /// </summary>
        public static void Clear()
        {
            _trails.Clear();
        }
        
        /// <summary>
        /// Updates trail fading. Called by ModSystem.
        /// </summary>
        public static void Update()
        {
            float dt = 1f / 60f;
            
            for (int i = _trails.Count - 1; i >= 0; i--)
            {
                var trail = _trails[i];
                trail.ScrollOffset += 0.03f;
                
                if (trail.IsFading)
                {
                    trail.FadeProgress += dt * 3f;
                    
                    if (trail.FadeProgress >= 1f || trail.Segments.Count == 0)
                    {
                        _trails.RemoveAt(i);
                        continue;
                    }
                    
                    // Shrink trail
                    if (trail.Segments.Count > 0 && Main.GameUpdateCount % 2 == 0)
                        trail.Segments.RemoveAt(0);
                }
            }
        }
        
        /// <summary>
        /// Renders all active trails using SpriteBatch.
        /// </summary>
        public static void Render()
        {
            if (_trails.Count == 0) return;
            
            // Get glow texture
            Texture2D glowTex = null;
            try
            {
                glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2", 
                    AssetRequestMode.ImmediateLoad).Value;
            }
            catch
            {
                return; // Texture not available yet
            }
            
            if (glowTex == null) return;
            
            SpriteBatch sb = Main.spriteBatch;
            Vector2 origin = glowTex.Size() / 2f;
            
            try
            {
                // Begin a fresh additive spritebatch (don't End first - we're in a hook where it's already ended)
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            foreach (var trail in _trails)
            {
                if (trail.Segments.Count < 2) continue;
                
                float fadeAlpha = trail.IsFading ? 1f - trail.FadeProgress : 1f;
                fadeAlpha = Math.Clamp(fadeAlpha, 0f, 1f);
                
                // Draw each segment as layered glows
                for (int i = 0; i < trail.Segments.Count; i++)
                {
                    var seg = trail.Segments[i];
                    float completionRatio = (float)i / Math.Max(1, trail.Segments.Count - 1);
                    float ageRatio = seg.Age / MAX_AGE;
                    
                    // QuadraticBump: 0→1→0 (thick in middle, thin at ends)
                    float bump = completionRatio * (4f - completionRatio * 4f);
                    float ageFade = 1f - ageRatio;
                    
                    // Combined intensity
                    float intensity = bump * ageFade * fadeAlpha;
                    if (intensity < 0.01f) continue;
                    
                    Vector2 drawPos = seg.Position - Main.screenPosition;
                    
                    // Slight wave motion
                    float wave = MathF.Sin(completionRatio * MathHelper.TwoPi * 2f + trail.ScrollOffset * 3f) * 8f;
                    drawPos.Y += wave * bump;
                    
                    // ==========================================
                    // LAYER 1: Outer fog (cool color, large, low alpha)
                    // ==========================================
                    float outerScale = trail.BaseWidth / glowTex.Width * 3.5f * bump;
                    Color outerColor = (trail.ColorCool * 0.3f * intensity) with { A = 0 };
                    sb.Draw(glowTex, drawPos, null, outerColor, seg.Rotation, origin, outerScale, SpriteEffects.None, 0f);
                    
                    // ==========================================
                    // LAYER 2: Mid fog (mid color, medium size)
                    // ==========================================
                    float midScale = trail.BaseWidth / glowTex.Width * 2.2f * bump;
                    Color midColor = (trail.ColorMid * 0.5f * intensity) with { A = 0 };
                    sb.Draw(glowTex, drawPos, null, midColor, seg.Rotation + 0.3f, origin, midScale, SpriteEffects.None, 0f);
                    
                    // ==========================================
                    // LAYER 3: Core (hot color, smaller, brighter)
                    // ==========================================
                    float coreScale = trail.BaseWidth / glowTex.Width * 1.2f * bump;
                    Color coreColor = (trail.ColorHot * 0.7f * intensity) with { A = 0 };
                    sb.Draw(glowTex, drawPos, null, coreColor, seg.Rotation, origin, coreScale, SpriteEffects.None, 0f);
                    
                    // ==========================================
                    // LAYER 4: White hot center (smallest, brightest)
                    // ==========================================
                    float centerScale = trail.BaseWidth / glowTex.Width * 0.5f * bump;
                    Color centerColor = (Color.White * 0.5f * intensity) with { A = 0 };
                    sb.Draw(glowTex, drawPos, null, centerColor, seg.Rotation, origin, centerScale, SpriteEffects.None, 0f);
                }
                
                // Draw connecting glow between segments for smoother trail
                for (int i = 0; i < trail.Segments.Count - 1; i++)
                {
                    var seg1 = trail.Segments[i];
                    var seg2 = trail.Segments[i + 1];
                    
                    float ratio1 = (float)i / Math.Max(1, trail.Segments.Count - 1);
                    float ratio2 = (float)(i + 1) / Math.Max(1, trail.Segments.Count - 1);
                    float avgRatio = (ratio1 + ratio2) * 0.5f;
                    float avgAge = (seg1.Age + seg2.Age) * 0.5f / MAX_AGE;
                    
                    float bump = avgRatio * (4f - avgRatio * 4f);
                    float ageFade = 1f - avgAge;
                    float intensity = bump * ageFade * fadeAlpha * 0.6f;
                    
                    if (intensity < 0.01f) continue;
                    
                    Vector2 midPoint = (seg1.Position + seg2.Position) * 0.5f - Main.screenPosition;
                    
                    // Bridge glow
                    float bridgeScale = trail.BaseWidth / glowTex.Width * 1.5f * bump;
                    Color bridgeColor = (trail.ColorMid * 0.4f * intensity) with { A = 0 };
                    sb.Draw(glowTex, midPoint, null, bridgeColor, (seg1.Rotation + seg2.Rotation) * 0.5f, 
                        origin, bridgeScale, SpriteEffects.None, 0f);
                }
            }
            
            // Restore blend state - just end, don't restart (we're in a hook)
            sb.End();
            }
            catch (Exception ex)
            {
                // Silently catch - don't spam log
                try { sb.End(); } catch { }
            }
        }
        
        #endregion
        
        #region Helpers
        
        private static TrailData FindOrCreateTrail(int ownerId, Color hot, Color mid, Color cool, float width)
        {
            foreach (var t in _trails)
            {
                if (t.OwnerId == ownerId && t.IsActive && !t.IsFading)
                    return t;
            }
            
            if (_trails.Count >= MAX_TRAILS)
                _trails.RemoveAt(0);
            
            var trail = new TrailData
            {
                OwnerId = ownerId,
                ColorHot = hot,
                ColorMid = mid,
                ColorCool = cool,
                BaseWidth = width,
                IsActive = true,
                IsFading = false,
                FadeProgress = 0f,
                ScrollOffset = Main.rand.NextFloat(10f)
            };
            
            _trails.Add(trail);
            return trail;
        }
        
        private static (Color hot, Color mid, Color cool) GetPalette(string theme)
        {
            return (theme?.ToLowerInvariant()) switch
            {
                "fate" => (
                    new Color(255, 100, 150),  // Pink
                    new Color(180, 50, 120),   // Deep pink
                    new Color(80, 20, 140)     // Purple
                ),
                "eroica" => (
                    new Color(255, 200, 80),   // Gold
                    new Color(200, 50, 50),    // Scarlet
                    new Color(100, 20, 20)     // Dark red
                ),
                "swanlake" => (
                    new Color(255, 255, 255),  // White
                    new Color(200, 200, 255),  // Pale blue
                    new Color(100, 100, 150)   // Blue-gray
                ),
                "lacampanella" => (
                    new Color(255, 160, 50),   // Orange
                    new Color(255, 100, 20),   // Deep orange
                    new Color(30, 20, 20)      // Black
                ),
                "moonlightsonata" => (
                    new Color(180, 150, 255),  // Light purple
                    new Color(100, 50, 180),   // Purple
                    new Color(30, 20, 80)      // Dark purple
                ),
                "enigma" or "enigmavariations" => (
                    new Color(100, 255, 150),  // Green
                    new Color(80, 40, 160),    // Purple
                    new Color(20, 10, 30)      // Void
                ),
                _ => (
                    new Color(255, 100, 150),  // Default pink
                    new Color(180, 50, 120),
                    new Color(80, 20, 140)
                )
            };
        }
        
        #endregion
    }
    
    /// <summary>
    /// ModSystem to integrate SimpleNebulaTrail.
    /// </summary>
    public class SimpleNebulaTrailSystem : ModSystem
    {
        public override void Load()
        {
            On_Main.DrawProjectiles += DrawNebulaTrails;
        }
        
        public override void Unload()
        {
            On_Main.DrawProjectiles -= DrawNebulaTrails;
        }
        
        public override void PostUpdatePlayers()
        {
            SimpleNebulaTrail.Update();
        }
        
        private void DrawNebulaTrails(On_Main.orig_DrawProjectiles orig, Main self)
        {
            orig(self);
            
            try
            {
                SimpleNebulaTrail.Render();
            }
            catch (Exception ex)
            {
                Mod?.Logger?.Warn($"SimpleNebulaTrail render error: {ex.Message}");
            }
        }
        
        public override void OnWorldUnload()
        {
            SimpleNebulaTrail.Clear();
        }
    }
}
