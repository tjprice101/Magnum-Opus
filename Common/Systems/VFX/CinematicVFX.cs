using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// CINEMATIC VFX SYSTEM - Anamorphic Lens Flares, Energy Streaks, Impact Glints
    /// 
    /// Uses VFXTextureRegistry for centralized texture management.
    /// 
    /// Effects provided:
    /// - Anamorphic horizontal lens flares (boss limit breaks, ultimates)
    /// - Energy streaks on weapon trails (triangle strip mesh mapping)
    /// - Impact glints (single-frame scaled bursts)
    /// - Post-process glare overlays
    /// 
    /// All textures use Additive Blending for proper light stacking.
    /// </summary>
    public static class CinematicVFX
    {
        #region Texture References (Delegated to VFXTextureRegistry)
        
        // ============================================
        // TEXTURE LOOKUPS - NOW USE VFXTextureRegistry
        // ============================================
        // These properties delegate to the centralized registry for
        // proper fallback handling and deduplication.
        
        /// <summary>Horizontal energy gradient for beam effects</summary>
        public static Texture2D HorizontalEnergyGradient => VFXTextureRegistry.LUT.HorizontalEnergy;
        
        /// <summary>Black core gradient for hollow beams</summary>
        public static Texture2D HorizontalBlackCore => VFXTextureRegistry.LUT.EnergyGradient;
        
        /// <summary>Nebula/cosmic wisp noise for Fate/Enigma themes</summary>
        public static Texture2D NebulaWispNoise => VFXTextureRegistry.Noise.NebulaWisp;
        
        /// <summary>Sparkly noise for Eroica/DiesIrae themes</summary>
        public static Texture2D SparklyNoise => VFXTextureRegistry.Noise.Sparkly;
        
        /// <summary>Tileable FBM noise for general procedural effects</summary>
        public static Texture2D FBMNoise => VFXTextureRegistry.Noise.TileableFBM;
        
        /// <summary>Marble/swirl noise for SwanLake/Moonlight themes</summary>
        public static Texture2D MarbleNoise => VFXTextureRegistry.Noise.Marble;
        
        // Debug flag - set to true to log texture loading issues
        public static bool DebugTextureLoading = false;
        
        #endregion
        
        #region Active Effects
        
        private static List<LensFlare> _activeLensFlares = new List<LensFlare>();
        private static List<ImpactGlint> _activeGlints = new List<ImpactGlint>();
        private static List<EnergyStreak> _activeStreaks = new List<EnergyStreak>();
        private static List<NebulaCloud> _nebulaClouds = new List<NebulaCloud>();
        
        private const int MaxLensFlares = 8;
        private const int MaxGlints = 30;
        private const int MaxStreaks = 20;
        private const int MaxNebulaClouds = 40;
        
        #endregion
        
        #region Effect Data Classes
        
        /// <summary>
        /// Anamorphic lens flare - horizontal light streak for cinematic impact
        /// </summary>
        private class LensFlare
        {
            public Vector2 Position;
            public Color Color;
            public float Scale;
            public float MaxScale;
            public float Rotation;
            public int Timer;
            public int MaxLifetime;
            public float Intensity;
            public bool UseBlackCore; // Use the black core center variant
            
            public float Progress => (float)Timer / MaxLifetime;
            public bool IsExpired => Timer >= MaxLifetime;
            
            public float GetAlpha()
            {
                float p = Progress;
                // Fast attack, slow release curve
                if (p < 0.1f) return p / 0.1f;
                return 1f - ((p - 0.1f) / 0.9f) * ((p - 0.1f) / 0.9f); // Quadratic fade out
            }
        }
        
        /// <summary>
        /// Impact glint - single-frame burst that scales up rapidly
        /// </summary>
        private class ImpactGlint
        {
            public Vector2 Position;
            public Color Color;
            public float BaseScale;
            public float MaxScale;
            public float Rotation;
            public int Timer;
            public int MaxLifetime;
            public bool IsHorizontal; // Use horizontal streak texture
            public bool IsSparkly; // Use sparkly noise texture
            
            public float Progress => (float)Timer / MaxLifetime;
            public bool IsExpired => Timer >= MaxLifetime;
            
            public float GetScale()
            {
                float p = Progress;
                // Explosive scale up, then fade
                if (p < 0.15f)
                {
                    // Overshoot spring effect
                    float t = p / 0.15f;
                    return BaseScale + (MaxScale - BaseScale) * (1f - (float)Math.Pow(1f - t, 3)) * 1.2f;
                }
                else if (p < 0.3f)
                {
                    // Settle to max
                    float t = (p - 0.15f) / 0.15f;
                    return MaxScale * 1.2f - (MaxScale * 0.2f) * t;
                }
                return MaxScale * (1f - (p - 0.3f) / 0.7f);
            }
            
            public float GetAlpha()
            {
                float p = Progress;
                if (p < 0.1f) return 1f;
                return 1f - (p - 0.1f) / 0.9f;
            }
        }
        
        /// <summary>
        /// Energy streak - flowing energy texture for trails
        /// </summary>
        private class EnergyStreak
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public Color PrimaryColor;
            public Color SecondaryColor;
            public float Scale;
            public float Rotation;
            public int Timer;
            public int MaxLifetime;
            public float ScrollSpeed;
            public float WidthMult;
            
            public float Progress => (float)Timer / MaxLifetime;
            public bool IsExpired => Timer >= MaxLifetime;
        }
        
        /// <summary>
        /// Enhanced nebula cloud using FBM/marble noise textures
        /// </summary>
        private class NebulaCloud
        {
            public Vector2 Position;
            public Vector2 PreviousPosition;
            public Vector2 Velocity;
            public Color PrimaryColor;
            public Color SecondaryColor;
            public float Scale;
            public float Rotation;
            public int Timer;
            public int MaxLifetime;
            public float Phase;
            public bool UseMarble; // Use marble noise for flowing look
            public bool UseFBM; // Use FBM for complex turbulence
            
            public float Progress => (float)Timer / MaxLifetime;
            public bool IsExpired => Timer >= MaxLifetime;
            
            public float GetAlpha()
            {
                float p = Progress;
                if (p < 0.15f) return EaseOutCubic(p / 0.15f);
                if (p > 0.5f) return 1f - EaseInCubic((p - 0.5f) / 0.5f);
                return 1f;
            }
        }
        
        #endregion
        
        #region Easing Functions
        
        private static float EaseOutCubic(float t) => 1f - (float)Math.Pow(1f - t, 3);
        private static float EaseInCubic(float t) => t * t * t;
        private static float EaseOutQuart(float t) => 1f - (float)Math.Pow(1f - t, 4);
        
        #endregion
        
        #region Public API - Lens Flares
        
        /// <summary>
        /// Spawns an anamorphic lens flare - horizontal light streak for cinematic moments.
        /// Use for: Boss ultimates, phase transitions, critical hits, limit breaks.
        /// </summary>
        public static void SpawnLensFlare(Vector2 worldPosition, Color color, float scale = 1f, 
            int lifetime = 25, bool useBlackCore = false, float intensity = 1f)
        {
            if (_activeLensFlares.Count >= MaxLensFlares)
                _activeLensFlares.RemoveAt(0);
            
            _activeLensFlares.Add(new LensFlare
            {
                Position = worldPosition,
                Color = color,
                Scale = 0f,
                MaxScale = scale,
                Rotation = 0f, // Horizontal
                Timer = 0,
                MaxLifetime = lifetime,
                Intensity = intensity,
                UseBlackCore = useBlackCore
            });
        }
        
        /// <summary>
        /// Spawns a boss limit break flare - massive anamorphic streak with screen-wide impact.
        /// </summary>
        public static void SpawnBossLimitBreak(Vector2 worldPosition, Color primaryColor, Color accentColor, float intensity = 1.5f)
        {
            // Main horizontal flare
            SpawnLensFlare(worldPosition, primaryColor, 3f * intensity, 40, true, intensity);
            
            // Secondary flares offset
            SpawnLensFlare(worldPosition + new Vector2(50, 0), accentColor * 0.6f, 2f * intensity, 35, false, intensity * 0.7f);
            SpawnLensFlare(worldPosition + new Vector2(-50, 0), accentColor * 0.6f, 2f * intensity, 35, false, intensity * 0.7f);
            
            // Vertical accent (cross flare)
            _activeLensFlares.Add(new LensFlare
            {
                Position = worldPosition,
                Color = Color.White * 0.5f,
                Scale = 0f,
                MaxScale = 1.5f * intensity,
                Rotation = MathHelper.PiOver2, // Vertical
                Timer = 0,
                MaxLifetime = 30,
                Intensity = intensity * 0.5f,
                UseBlackCore = false
            });
        }
        
        #endregion
        
        #region Public API - Impact Glints
        
        /// <summary>
        /// Spawns an impact glint - single-frame burst that scales up rapidly.
        /// Use for: Projectile impacts, melee hits, collision effects.
        /// </summary>
        public static void SpawnImpactGlint(Vector2 worldPosition, Color color, float maxScale = 1f,
            int lifetime = 12, bool horizontal = true, bool sparkly = false)
        {
            if (_activeGlints.Count >= MaxGlints)
                _activeGlints.RemoveAt(0);
            
            _activeGlints.Add(new ImpactGlint
            {
                Position = worldPosition,
                Color = color,
                BaseScale = maxScale * 0.1f,
                MaxScale = maxScale,
                Rotation = horizontal ? 0f : Main.rand.NextFloat(MathHelper.TwoPi),
                Timer = 0,
                MaxLifetime = lifetime,
                IsHorizontal = horizontal,
                IsSparkly = sparkly
            });
        }
        
        /// <summary>
        /// Spawns a critical hit glint burst - multiple glints for impactful hits.
        /// </summary>
        public static void SpawnCriticalHitBurst(Vector2 worldPosition, Color primaryColor, Color accentColor)
        {
            // Central glint
            SpawnImpactGlint(worldPosition, Color.White, 1.2f, 15, true, false);
            
            // Colored streaks
            SpawnImpactGlint(worldPosition, primaryColor, 0.8f, 18, true, false);
            
            // Sparkly accents
            for (int i = 0; i < 3; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                SpawnImpactGlint(worldPosition + offset, accentColor, 0.4f, 10, false, true);
            }
        }
        
        #endregion
        
        #region Public API - Energy Streaks
        
        /// <summary>
        /// Spawns an energy streak - flowing energy texture for trails.
        /// Use for: Weapon swing trails, projectile wakes, dash effects.
        /// </summary>
        public static void SpawnEnergyStreak(Vector2 worldPosition, Vector2 velocity, Color primaryColor,
            Color secondaryColor, float scale = 1f, int lifetime = 20, float scrollSpeed = 2f)
        {
            if (_activeStreaks.Count >= MaxStreaks)
                _activeStreaks.RemoveAt(0);
            
            _activeStreaks.Add(new EnergyStreak
            {
                Position = worldPosition,
                Velocity = velocity,
                PrimaryColor = primaryColor,
                SecondaryColor = secondaryColor,
                Scale = scale,
                Rotation = velocity.ToRotation(),
                Timer = 0,
                MaxLifetime = lifetime,
                ScrollSpeed = scrollSpeed,
                WidthMult = 1f
            });
        }
        
        #endregion
        
        #region Public API - Enhanced Nebula Fog
        
        /// <summary>
        /// Spawns an enhanced nebula cloud using the custom FBM/marble noise textures.
        /// Much smoother and more organic than basic fog particles.
        /// </summary>
        public static void SpawnEnhancedNebula(Vector2 worldPosition, Vector2 velocity, Color primaryColor,
            Color secondaryColor, float scale = 1f, int lifetime = 45, bool useMarble = false)
        {
            if (_nebulaClouds.Count >= MaxNebulaClouds)
                _nebulaClouds.RemoveAt(0);
            
            _nebulaClouds.Add(new NebulaCloud
            {
                Position = worldPosition,
                PreviousPosition = worldPosition,
                Velocity = velocity,
                PrimaryColor = primaryColor,
                SecondaryColor = secondaryColor,
                Scale = scale,
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi),
                Timer = 0,
                MaxLifetime = lifetime,
                Phase = Main.rand.NextFloat(MathHelper.TwoPi),
                UseMarble = useMarble,
                UseFBM = !useMarble
            });
        }
        
        /// <summary>
        /// Spawns nebula wisp clouds along a melee swing - uses NebulaWispNoise texture.
        /// </summary>
        public static void SpawnSwingNebulaWisps(Player player, float swingProgress, Color primaryColor,
            Color secondaryColor, float scale = 1f)
        {
            if (swingProgress < 0.1f || swingProgress > 0.9f) return;
            if (Main.GameUpdateCount % 2 != 0) return;
            
            float swingAngle = player.itemRotation + (swingProgress - 0.5f) * 1.8f;
            
            float[] distances = { 50f, 80f };
            foreach (float baseDist in distances)
            {
                float dist = baseDist * scale;
                Vector2 pos = player.Center + swingAngle.ToRotationVector2() * dist;
                Vector2 vel = swingAngle.ToRotationVector2().RotatedBy(MathHelper.PiOver2 * player.direction) * 1.5f;
                
                // Alternate between marble (flowing) and FBM (turbulent) for variety
                bool useMarble = Main.rand.NextBool();
                SpawnEnhancedNebula(pos, vel, primaryColor, secondaryColor, scale * 0.7f, 35, useMarble);
            }
        }
        
        /// <summary>
        /// Spawns Fate theme cinematic nebula - dark cosmic with stellar glints.
        /// </summary>
        public static void SpawnFateCosmicNebula(Vector2 worldPosition, Vector2 velocity, float scale = 1f)
        {
            Color darkPink = new Color(140, 50, 90);
            Color darkPurple = new Color(80, 35, 110);
            Color cosmicWhite = new Color(255, 240, 255);
            
            // Dark nebula base
            SpawnEnhancedNebula(worldPosition, velocity, darkPink, darkPurple, scale, 50, true);
            
            // Occasional stellar glint
            if (Main.rand.NextBool(4))
            {
                SpawnImpactGlint(worldPosition + Main.rand.NextVector2Circular(15f, 15f), 
                    cosmicWhite, 0.3f, 8, true, true);
            }
        }
        
        #endregion
        
        #region Update
        
        public static void Update()
        {
            // Update lens flares
            for (int i = _activeLensFlares.Count - 1; i >= 0; i--)
            {
                var flare = _activeLensFlares[i];
                flare.Timer++;
                flare.Scale = flare.MaxScale * EaseOutQuart(Math.Min(flare.Progress * 5f, 1f));
                
                if (flare.IsExpired)
                    _activeLensFlares.RemoveAt(i);
            }
            
            // Update glints
            for (int i = _activeGlints.Count - 1; i >= 0; i--)
            {
                var glint = _activeGlints[i];
                glint.Timer++;
                
                if (glint.IsExpired)
                    _activeGlints.RemoveAt(i);
            }
            
            // Update streaks
            for (int i = _activeStreaks.Count - 1; i >= 0; i--)
            {
                var streak = _activeStreaks[i];
                streak.Timer++;
                streak.Position += streak.Velocity;
                streak.Velocity *= 0.95f;
                
                // Fade width
                float p = streak.Progress;
                streak.WidthMult = 1f - p * p;
                
                if (streak.IsExpired)
                    _activeStreaks.RemoveAt(i);
            }
            
            // Update nebula clouds
            for (int i = _nebulaClouds.Count - 1; i >= 0; i--)
            {
                var cloud = _nebulaClouds[i];
                cloud.PreviousPosition = cloud.Position;
                cloud.Position += cloud.Velocity;
                cloud.Velocity *= 0.92f;
                cloud.Timer++;
                cloud.Rotation += 0.008f;
                
                if (cloud.IsExpired)
                    _nebulaClouds.RemoveAt(i);
            }
        }
        
        #endregion
        
        #region Render
        
        public static void Render(SpriteBatch spriteBatch)
        {
            if (_activeLensFlares.Count == 0 && _activeGlints.Count == 0 && 
                _activeStreaks.Count == 0 && _nebulaClouds.Count == 0)
                return;
            
            float gameTime = Main.GameUpdateCount * 0.02f;
            
            // === PASS 1: NEBULA CLOUDS (Background, soft alpha blending) ===
            RenderNebulaClouds(spriteBatch, gameTime);
            
            // === PASS 2: ENERGY STREAKS (Additive for glow) ===
            RenderEnergyStreaks(spriteBatch, gameTime);
            
            // === PASS 3: LENS FLARES & GLINTS (Additive, screen-space glare) ===
            RenderLensFlaresAndGlints(spriteBatch, gameTime);
        }
        
        private static void RenderNebulaClouds(SpriteBatch spriteBatch, float gameTime)
        {
            if (_nebulaClouds.Count == 0) return;
            
            Texture2D fbmTex = FBMNoise;
            Texture2D marbleTex = MarbleNoise;
            Texture2D nebulaWispTex = NebulaWispNoise;
            Texture2D softTex = MagnumTextureRegistry.GetBloom();
            
            // Background pass - NonPremultiplied for soft fog
            try { spriteBatch.End(); } catch { }
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearWrap,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            foreach (var cloud in _nebulaClouds)
            {
                float partialTick = (Main.GameUpdateCount % 60) / 60f;
                Vector2 drawPos = Vector2.Lerp(cloud.PreviousPosition, cloud.Position, partialTick) - Main.screenPosition;
                float alpha = cloud.GetAlpha();
                float time = gameTime + cloud.Phase;
                
                Texture2D noiseTex = cloud.UseMarble ? marbleTex : fbmTex;
                if (noiseTex == null) noiseTex = nebulaWispTex;
                if (noiseTex == null) continue;
                
                // Background layer - large, subtle
                float bgScale = cloud.Scale * 2.2f;
                float bgOffset1 = (float)Math.Sin(time * 0.4f) * 10f;
                float bgOffset2 = (float)Math.Cos(time * 0.35f) * 8f;
                Vector2 bgPos = drawPos + new Vector2(bgOffset1, bgOffset2);
                Color bgColor = cloud.SecondaryColor * (alpha * 0.12f);
                
                spriteBatch.Draw(noiseTex, bgPos, null, bgColor, cloud.Rotation + time * 0.05f,
                    noiseTex.Size() / 2f, bgScale, SpriteEffects.None, 0f);
                
                // Midground layer - color oscillation
                float colorOsc = (float)Math.Sin(time * 0.7f) * 0.5f + 0.5f;
                Color midColor = Color.Lerp(cloud.PrimaryColor, cloud.SecondaryColor, colorOsc);
                float midScale = cloud.Scale * 1.3f;
                
                spriteBatch.Draw(noiseTex, drawPos, null, midColor * (alpha * 0.2f), cloud.Rotation - time * 0.03f,
                    noiseTex.Size() / 2f, midScale, SpriteEffects.None, 0f);
                
                // Soft glow overlay
                if (softTex != null)
                {
                    spriteBatch.Draw(softTex, drawPos, null, midColor * (alpha * 0.15f), 0f,
                        softTex.Size() / 2f, cloud.Scale * 1.5f, SpriteEffects.None, 0f);
                }
            }
            
            // Additive pass for glow highlights
            try { spriteBatch.End(); } catch { }
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            foreach (var cloud in _nebulaClouds)
            {
                float partialTick = (Main.GameUpdateCount % 60) / 60f;
                Vector2 drawPos = Vector2.Lerp(cloud.PreviousPosition, cloud.Position, partialTick) - Main.screenPosition;
                float alpha = cloud.GetAlpha();
                
                if (softTex != null)
                {
                    // Bright core glow
                    Color glowColor = Color.Lerp(cloud.PrimaryColor, Color.White, 0.3f) with { A = 0 };
                    spriteBatch.Draw(softTex, drawPos, null, glowColor * (alpha * 0.25f), 0f,
                        softTex.Size() / 2f, cloud.Scale * 0.5f, SpriteEffects.None, 0f);
                }
            }
        }
        
        private static void RenderEnergyStreaks(SpriteBatch spriteBatch, float gameTime)
        {
            if (_activeStreaks.Count == 0) return;
            
            Texture2D energyTex = HorizontalEnergyGradient;
            Texture2D marbleTex = MarbleNoise;
            if (energyTex == null) return;
            
            // Already in additive mode from previous pass
            
            foreach (var streak in _activeStreaks)
            {
                Vector2 drawPos = streak.Position - Main.screenPosition;
                float alpha = 1f - streak.Progress;
                float time = gameTime + streak.Timer * streak.ScrollSpeed * 0.1f;
                
                // Color lerp over lifetime
                Color currentColor = Color.Lerp(streak.PrimaryColor, streak.SecondaryColor, streak.Progress);
                currentColor = currentColor with { A = 0 }; // Remove alpha for additive
                
                // Main energy streak
                float scaleX = streak.Scale * streak.WidthMult;
                float scaleY = streak.Scale * 0.5f;
                
                spriteBatch.Draw(energyTex, drawPos, null, currentColor * (alpha * 0.6f), streak.Rotation,
                    new Vector2(energyTex.Width / 2f, energyTex.Height / 2f), new Vector2(scaleX, scaleY), 
                    SpriteEffects.None, 0f);
                
                // Marble flow overlay for organic feel
                if (marbleTex != null)
                {
                    Color flowColor = Color.Lerp(currentColor, Color.White, 0.2f) with { A = 0 };
                    spriteBatch.Draw(marbleTex, drawPos, null, flowColor * (alpha * 0.3f), 
                        streak.Rotation + (float)Math.Sin(time) * 0.1f,
                        marbleTex.Size() / 2f, new Vector2(scaleX * 0.8f, scaleY * 0.6f), 
                        SpriteEffects.None, 0f);
                }
            }
        }
        
        private static void RenderLensFlaresAndGlints(SpriteBatch spriteBatch, float gameTime)
        {
            Texture2D energyTex = HorizontalEnergyGradient;
            Texture2D blackCoreTex = HorizontalBlackCore;
            Texture2D sparklyTex = SparklyNoise;
            Texture2D softTex = MagnumTextureRegistry.GetBloom();
            
            // Already in additive mode
            
            // Render lens flares
            foreach (var flare in _activeLensFlares)
            {
                Vector2 drawPos = flare.Position - Main.screenPosition;
                float alpha = flare.GetAlpha() * flare.Intensity;
                
                Texture2D tex = flare.UseBlackCore ? blackCoreTex : energyTex;
                if (tex == null) tex = energyTex;
                if (tex == null) continue;
                
                Color flareColor = flare.Color with { A = 0 };
                
                // Main flare
                float scaleX = flare.Scale * 2f;
                float scaleY = flare.Scale * 0.4f;
                
                spriteBatch.Draw(tex, drawPos, null, flareColor * (alpha * 0.7f), flare.Rotation,
                    new Vector2(tex.Width / 2f, tex.Height / 2f), new Vector2(scaleX, scaleY),
                    SpriteEffects.None, 0f);
                
                // Bright core (white)
                Color coreColor = Color.White with { A = 0 };
                spriteBatch.Draw(tex, drawPos, null, coreColor * (alpha * 0.4f), flare.Rotation,
                    new Vector2(tex.Width / 2f, tex.Height / 2f), new Vector2(scaleX * 0.6f, scaleY * 0.5f),
                    SpriteEffects.None, 0f);
                
                // Soft glow halo
                if (softTex != null)
                {
                    spriteBatch.Draw(softTex, drawPos, null, flareColor * (alpha * 0.3f), 0f,
                        softTex.Size() / 2f, flare.Scale * 1.2f, SpriteEffects.None, 0f);
                }
            }
            
            // Render impact glints
            foreach (var glint in _activeGlints)
            {
                Vector2 drawPos = glint.Position - Main.screenPosition;
                float alpha = glint.GetAlpha();
                float scale = glint.GetScale();
                
                Color glintColor = glint.Color with { A = 0 };
                
                if (glint.IsSparkly && sparklyTex != null)
                {
                    // Sparkly glint
                    spriteBatch.Draw(sparklyTex, drawPos, null, glintColor * (alpha * 0.8f), glint.Rotation,
                        sparklyTex.Size() / 2f, scale, SpriteEffects.None, 0f);
                }
                else if (glint.IsHorizontal && energyTex != null)
                {
                    // Horizontal streak glint
                    spriteBatch.Draw(energyTex, drawPos, null, glintColor * (alpha * 0.9f), glint.Rotation,
                        new Vector2(energyTex.Width / 2f, energyTex.Height / 2f), 
                        new Vector2(scale * 1.5f, scale * 0.3f), SpriteEffects.None, 0f);
                }
                
                // White core
                if (softTex != null)
                {
                    Color coreColor = Color.White with { A = 0 };
                    spriteBatch.Draw(softTex, drawPos, null, coreColor * (alpha * 0.5f), 0f,
                        softTex.Size() / 2f, scale * 0.3f, SpriteEffects.None, 0f);
                }
            }
        }
        
        #endregion
        
        #region Utility
        
        public static void Clear()
        {
            _activeLensFlares.Clear();
            _activeGlints.Clear();
            _activeStreaks.Clear();
            _nebulaClouds.Clear();
        }
        
        public static void Unload()
        {
            Clear();
            // Textures are now managed by VFXTextureRegistry, no need to null them here
        }
        
        #endregion
    }
}
