using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// LAYERED NEBULA FOG SYSTEM - Calamity/Ark of the Cosmos Style
    /// 
    /// Creates buttery smooth fog using:
    /// - Large soft quads instead of discrete particles
    /// - Three parallax layers (background, midground, foreground glow)
    /// - Additive blending for proper glow accumulation
    /// - Noise-based UV animation for organic movement
    /// - Smooth alpha interpolation (no harsh edges)
    /// </summary>
    public static class LayeredNebulaFog
    {
        #region Nebula Instances
        
        private static List<NebulaCloud> _activeClouds = new List<NebulaCloud>();
        private const int MaxClouds = 20;
        
        /// <summary>
        /// A nebula cloud is a single large soft quad with multiple animated layers
        /// </summary>
        private class NebulaCloud
        {
            public Vector2 Position;
            public Vector2 PreviousPosition;
            public Vector2 Velocity;
            public int Timer;
            public int MaxLifetime;
            public float Scale;
            public float MaxScale;
            public uint SpawnTime;
            
            // Colors
            public Color PrimaryColor;
            public Color SecondaryColor;
            public Color GlowColor;
            
            // Animation phases (unique per cloud for variety)
            public float Phase1;
            public float Phase2;
            public float Phase3;
            public float RotationPhase;
            
            public float Progress => (float)Timer / MaxLifetime;
            public bool IsExpired => Timer >= MaxLifetime;
            
            /// <summary>
            /// Smooth alpha curve: fade in quickly, sustain, fade out smoothly
            /// </summary>
            public float GetAlpha()
            {
                float p = Progress;
                if (p < 0.15f)
                {
                    // Fade in with ease-out curve
                    return EaseOutCubic(p / 0.15f);
                }
                else if (p > 0.6f)
                {
                    // Fade out with ease-in curve (slower, smoother)
                    return 1f - EaseInCubic((p - 0.6f) / 0.4f);
                }
                return 1f;
            }
        }
        
        #endregion
        
        #region Easing Functions
        
        private static float EaseOutCubic(float t) => 1f - (float)Math.Pow(1f - t, 3);
        private static float EaseInCubic(float t) => t * t * t;
        private static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
        private static float EaseInQuad(float t) => t * t;
        
        #endregion
        
        #region Interpolation
        
        private static float _partialTicks;
        
        public static void UpdatePartialTicks()
        {
            // Get sub-frame interpolation value
            _partialTicks = (float)(Main.GameUpdateCount % 60) / 60f;
        }
        
        private static Vector2 Lerp(Vector2 a, Vector2 b, float t) => Vector2.Lerp(a, b, t);
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Spawns a smooth nebula cloud at position.
        /// </summary>
        public static void SpawnNebulaCloud(Vector2 position, Color primaryColor, Color secondaryColor, 
            float scale = 1f, Vector2? velocity = null, int lifetime = 45)
        {
            if (_activeClouds.Count >= MaxClouds)
                _activeClouds.RemoveAt(0);
            
            // Generate glow color (brighter version for foreground layer)
            Color glowColor = Color.Lerp(primaryColor, Color.White, 0.4f);
            glowColor = new Color(
                Math.Min(255, glowColor.R + 40),
                Math.Min(255, glowColor.G + 40),
                Math.Min(255, glowColor.B + 40)
            );
            
            var cloud = new NebulaCloud
            {
                Position = position,
                PreviousPosition = position,
                Velocity = velocity ?? Vector2.Zero,
                Timer = 0,
                MaxLifetime = lifetime,
                Scale = 0f,
                MaxScale = scale,
                SpawnTime = Main.GameUpdateCount,
                PrimaryColor = primaryColor,
                SecondaryColor = secondaryColor,
                GlowColor = glowColor,
                // Unique phases for organic variety
                Phase1 = Main.rand.NextFloat(MathHelper.TwoPi),
                Phase2 = Main.rand.NextFloat(MathHelper.TwoPi),
                Phase3 = Main.rand.NextFloat(MathHelper.TwoPi),
                RotationPhase = Main.rand.NextFloat(MathHelper.TwoPi)
            };
            
            _activeClouds.Add(cloud);
        }
        
        /// <summary>
        /// Spawns nebula fog along a melee swing arc - Ark of the Cosmos style.
        /// </summary>
        public static void SpawnSwingNebula(Player player, float swingProgress, Color primaryColor, 
            Color secondaryColor, float scale = 1f)
        {
            // Only spawn during active swing portion
            if (swingProgress < 0.08f || swingProgress > 0.92f) return;
            
            // Spawn rate: every 2 frames for smooth coverage without overdoing it
            if (Main.GameUpdateCount % 2 != 0) return;
            
            float swingAngle = player.itemRotation + (swingProgress - 0.5f) * 1.8f;
            
            // Spawn at blade tip and mid-blade
            float[] distances = { 55f, 85f };
            
            foreach (float baseDist in distances)
            {
                float dist = baseDist * scale;
                Vector2 pos = player.Center + swingAngle.ToRotationVector2() * dist;
                
                // Velocity trails behind the swing
                Vector2 vel = swingAngle.ToRotationVector2().RotatedBy(MathHelper.PiOver2 * player.direction) * 2f;
                vel += Main.rand.NextVector2Circular(0.5f, 0.5f);
                
                SpawnNebulaCloud(pos, primaryColor, secondaryColor, scale * 0.8f, vel, 35);
            }
            
            // Occasional bright star twinkle
            if (Main.rand.NextBool(5))
            {
                float starDist = Main.rand.NextFloat(40f, 95f) * scale;
                Vector2 starPos = player.Center + (swingAngle + Main.rand.NextFloat(-0.25f, 0.25f)).ToRotationVector2() * starDist;
                
                var sparkle = new SparkleParticle(
                    starPos,
                    Main.rand.NextVector2Circular(1.5f, 1.5f),
                    Color.White,
                    Main.rand.NextFloat(0.2f, 0.4f),
                    Main.rand.Next(8, 14)
                );
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }
        
        /// <summary>
        /// Spawns Fate theme nebula (bright pink → purple with white stars)
        /// IMPORTANT: Additive blending requires BRIGHT colors, not dark!
        /// </summary>
        public static void SpawnFateNebula(Player player, float swingProgress, float scale = 1f)
        {
            Color brightPink = new Color(255, 120, 180);    // Bright pink (additive-friendly)
            Color brightPurple = new Color(180, 100, 220);  // Bright purple (additive-friendly)
            SpawnSwingNebula(player, swingProgress, brightPink, brightPurple, scale);
        }
        
        #endregion
        
        #region Update
        
        public static void Update()
        {
            for (int i = _activeClouds.Count - 1; i >= 0; i--)
            {
                var cloud = _activeClouds[i];
                
                cloud.PreviousPosition = cloud.Position;
                cloud.Position += cloud.Velocity;
                cloud.Velocity *= 0.92f; // Gentle deceleration
                cloud.Timer++;
                
                // Smooth scale animation
                float p = cloud.Progress;
                if (p < 0.2f)
                {
                    cloud.Scale = cloud.MaxScale * EaseOutCubic(p / 0.2f);
                }
                else if (p > 0.65f)
                {
                    cloud.Scale = cloud.MaxScale * (1f - EaseInQuad((p - 0.65f) / 0.35f));
                }
                else
                {
                    cloud.Scale = cloud.MaxScale;
                }
                
                if (cloud.IsExpired)
                    _activeClouds.RemoveAt(i);
            }
        }
        
        #endregion
        
        #region Render
        
        public static void Render(SpriteBatch spriteBatch)
        {
            if (_activeClouds.Count == 0) return;
            
            UpdatePartialTicks();
            
            // Get soft glow texture (should be a very soft, blurred circle)
            Texture2D softTex = MagnumTextureRegistry.GetBloom();
            if (softTex == null) return;
            
            // ============================================
            // TEXTURE LOOKUPS - NOW USE VFXTextureRegistry
            // ============================================
            // Centralized texture management with proper fallbacks.
            Texture2D fbmNoiseTex = VFXTextureRegistry.Noise.TileableFBM;      // Complex turbulent noise
            Texture2D marbleNoiseTex = VFXTextureRegistry.Noise.Marble;        // Flowing organic noise
            Texture2D nebulaWispTex = VFXTextureRegistry.Noise.NebulaWisp;     // Wispy fractal noise
            
            // Fallback to procedural if custom textures not loaded
            Texture2D cloudNoiseTex = nebulaWispTex ?? fbmNoiseTex ?? ParticleTextureGenerator.CloudNoise;
            Texture2D flowNoiseTex = marbleNoiseTex ?? fbmNoiseTex ?? cloudNoiseTex;
            
            float gameTime = Main.GameUpdateCount * 0.015f;
            
            // === RENDER IN THREE PASSES FOR LAYERED DEPTH ===
            // CRITICAL: All passes use BlendState.Additive to avoid black blob artifacts!
            // NonPremultiplied causes black areas with noise textures.
            
            // PASS 1: BACKGROUND LAYER (large, slow, low opacity) - ADDITIVE
            try { spriteBatch.End(); } catch { }
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            foreach (var cloud in _activeClouds)
            {
                Vector2 drawPos = Lerp(cloud.PreviousPosition, cloud.Position, _partialTicks) - Main.screenPosition;
                float alpha = cloud.GetAlpha();
                float time = (Main.GameUpdateCount - cloud.SpawnTime) * 0.02f;
                
                // Background: Large, slow-moving, subtle glow
                float bgOffset1 = (float)Math.Sin(time * 0.5f + cloud.Phase1) * 12f;
                float bgOffset2 = (float)Math.Cos(time * 0.4f + cloud.Phase2) * 10f;
                Vector2 bgPos = drawPos + new Vector2(bgOffset1, bgOffset2);
                
                float bgRotation = time * 0.1f + cloud.RotationPhase;
                float bgScale = cloud.Scale * 2.5f; // Large background
                
                // For additive blending: use bright colors with low alpha, remove A channel
                Color bgColor = cloud.SecondaryColor with { A = 0 } * (alpha * 0.12f);
                
                // Draw cloud noise layer first (for organic texture)
                if (cloudNoiseTex != null)
                {
                    spriteBatch.Draw(cloudNoiseTex, bgPos, null, bgColor * 0.5f, bgRotation,
                        cloudNoiseTex.Size() / 2f, bgScale * 0.8f, SpriteEffects.None, 0f);
                }
                
                // Draw soft tex on top
                spriteBatch.Draw(softTex, bgPos, null, bgColor, bgRotation,
                    softTex.Size() / 2f, bgScale, SpriteEffects.None, 0f);
            }
            
            // PASS 2: MIDGROUND LAYER (medium, faster, more color) - ADDITIVE
            foreach (var cloud in _activeClouds)
            {
                Vector2 drawPos = Lerp(cloud.PreviousPosition, cloud.Position, _partialTicks) - Main.screenPosition;
                float alpha = cloud.GetAlpha();
                float time = (Main.GameUpdateCount - cloud.SpawnTime) * 0.025f;
                
                // Midground: Follows weapon more closely, oscillates between colors
                float colorOsc = (float)Math.Sin(time * 0.8f + cloud.Phase3) * 0.5f + 0.5f;
                Color midColor = Color.Lerp(cloud.PrimaryColor, cloud.SecondaryColor, colorOsc);
                
                float midOffset1 = (float)Math.Sin(time * 0.9f + cloud.Phase2) * 6f;
                float midOffset2 = (float)Math.Cos(time * 0.7f + cloud.Phase1) * 5f;
                Vector2 midPos = drawPos + new Vector2(midOffset1, midOffset2);
                
                float midRotation = -time * 0.15f + cloud.RotationPhase;
                float midScale = cloud.Scale * 1.4f;
                Color midDrawColor = midColor with { A = 0 } * (alpha * 0.2f);
                
                // Draw cloud noise layer for organic texture
                if (flowNoiseTex != null)
                {
                    spriteBatch.Draw(flowNoiseTex, midPos, null, midDrawColor * 0.5f, midRotation + 0.3f,
                        flowNoiseTex.Size() / 2f, midScale * 0.7f, SpriteEffects.None, 0f);
                }
                
                spriteBatch.Draw(softTex, midPos, null, midDrawColor, midRotation,
                    softTex.Size() / 2f, midScale, SpriteEffects.None, 0f);
                
                // Second midground layer offset for depth
                float mid2Offset1 = (float)Math.Cos(time * 1.1f + cloud.Phase3) * 4f;
                float mid2Offset2 = (float)Math.Sin(time * 0.85f + cloud.Phase2) * 4f;
                Vector2 mid2Pos = drawPos + new Vector2(mid2Offset1, mid2Offset2);
                
                Color mid2Color = Color.Lerp(cloud.SecondaryColor, cloud.PrimaryColor, colorOsc) with { A = 0 };
                spriteBatch.Draw(softTex, mid2Pos, null, mid2Color * (alpha * 0.15f), midRotation + 0.5f,
                    softTex.Size() / 2f, midScale * 0.9f, SpriteEffects.None, 0f);
            }
            
            // PASS 3: FOREGROUND GLOW LAYER (additive blending for bright highlights)
            try { spriteBatch.End(); } catch { }
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            foreach (var cloud in _activeClouds)
            {
                Vector2 drawPos = Lerp(cloud.PreviousPosition, cloud.Position, _partialTicks) - Main.screenPosition;
                float alpha = cloud.GetAlpha();
                float time = (Main.GameUpdateCount - cloud.SpawnTime) * 0.03f;
                
                // Foreground: Small bright blobs with additive glow
                // Multiple small highlights that accumulate
                for (int i = 0; i < 3; i++)
                {
                    float highlightPhase = cloud.Phase1 + i * MathHelper.TwoPi / 3f;
                    float hOffset1 = (float)Math.Sin(time * 1.2f + highlightPhase) * 8f;
                    float hOffset2 = (float)Math.Cos(time * 1.0f + highlightPhase) * 6f;
                    Vector2 hPos = drawPos + new Vector2(hOffset1, hOffset2);
                    
                    float hScale = cloud.Scale * (0.4f + i * 0.15f);
                    
                    // Glow color with removed alpha for additive
                    Color glowDraw = cloud.GlowColor with { A = 0 };
                    glowDraw *= alpha * 0.35f;
                    
                    spriteBatch.Draw(softTex, hPos, null, glowDraw, time + i,
                        softTex.Size() / 2f, hScale, SpriteEffects.None, 0f);
                }
                
                // Central bright core
                Color coreDraw = Color.White with { A = 0 };
                coreDraw *= alpha * 0.2f;
                float coreScale = cloud.Scale * 0.25f;
                
                spriteBatch.Draw(softTex, drawPos, null, coreDraw, time * 0.5f,
                    softTex.Size() / 2f, coreScale, SpriteEffects.None, 0f);
            }
            
            // Restore to additive for other VFX systems
            // (Leave in additive mode as that's the expected state)
        }
        
        #endregion
        
        #region Utility
        
        public static void Clear()
        {
            _activeClouds.Clear();
        }
        
        public static int ActiveCount => _activeClouds.Count;
        
        #endregion
    }
}
