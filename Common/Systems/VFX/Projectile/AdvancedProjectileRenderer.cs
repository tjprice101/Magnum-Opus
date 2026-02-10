using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// Advanced Projectile Renderer - Provides buttery smooth Calamity-style rendering for all projectiles.
    /// Features: Bézier curve interpolation, multi-pass bloom, screen effects, unique trail identities.
    /// </summary>
    public class AdvancedProjectileRenderer : ModSystem
    {
        // Interpolation data for sub-frame smoothness (144Hz+ compatible)
        private static Dictionary<int, ProjectileRenderState> _renderStates = new Dictionary<int, ProjectileRenderState>();
        
        // Bloom textures cache
        private static Texture2D _bloomTexture;
        private static Texture2D _softGlowTexture;
        private static Texture2D _flareTexture;
        
        public class ProjectileRenderState
        {
            public Vector2 PreviousPosition;
            public Vector2 InterpolatedPosition;
            public float PreviousRotation;
            public float InterpolatedRotation;
            public Vector2[] BezierTrail = new Vector2[24];
            public int TrailIndex = 0;
            public float[] TrailOpacity = new float[24];
            public float[] TrailScale = new float[24];
            public Color[] TrailColors = new Color[24];
            public float LifetimeProgress;
            public float PulsePhase;
            public float OrbitAngle;
            public int SpawnFrame;
            
            public void RecordPosition(Vector2 pos, float rot)
            {
                PreviousPosition = InterpolatedPosition;
                PreviousRotation = InterpolatedRotation;
                InterpolatedPosition = pos;
                InterpolatedRotation = rot;
                
                // Record trail point
                BezierTrail[TrailIndex] = pos;
                TrailOpacity[TrailIndex] = 1f;
                TrailScale[TrailIndex] = 1f;
                TrailIndex = (TrailIndex + 1) % BezierTrail.Length;
            }
            
            public Vector2 GetInterpolatedCenter(float lerpAmount)
            {
                if (PreviousPosition == Vector2.Zero) return InterpolatedPosition;
                return Vector2.Lerp(PreviousPosition, InterpolatedPosition, lerpAmount);
            }
            
            public float GetInterpolatedRotation(float lerpAmount)
            {
                return MathHelper.Lerp(PreviousRotation, InterpolatedRotation, lerpAmount);
            }
        }
        
        public override void Load()
        {
            On_Main.DrawProjectiles += DrawProjectilesWithInterpolation;
        }
        
        public override void Unload()
        {
            On_Main.DrawProjectiles -= DrawProjectilesWithInterpolation;
            _renderStates.Clear();
            _bloomTexture = null;
            _softGlowTexture = null;
            _flareTexture = null;
        }
        
        private void DrawProjectilesWithInterpolation(On_Main.orig_DrawProjectiles orig, Main self)
        {
            // Pre-draw: Handle interpolated bloom/glow layer for all MagnumOpus projectiles
            DrawInterpolatedBloomLayer();
            
            // Original draw call
            orig(self);
            
            // Post-draw: Add trail afterglow effects
            DrawTrailAfterglowLayer();
        }
        
        private void DrawInterpolatedBloomLayer()
        {
            if (Main.gameMenu) return;
            
            SpriteBatch sb = Main.spriteBatch;
            
            try
            {
                // Begin additive blending for bloom
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                
                foreach (Projectile proj in Main.ActiveProjectiles)
                {
                    if (proj.ModProjectile?.Mod?.Name != "MagnumOpus") continue;
                    if (!_renderStates.TryGetValue(proj.whoAmI, out var state)) continue;
                    
                    // Get interpolation amount for sub-frame smoothness
                    float lerpAmount = Main.GameUpdateCount % 2 == 0 ? 0.5f : 1f;
                    Vector2 interpolatedPos = state.GetInterpolatedCenter(lerpAmount) - Main.screenPosition;
                    
                    // Get theme palette
                    string theme = DetectProjectileTheme(proj);
                    Color[] palette = MagnumThemePalettes.GetThemePalette(theme);
                    
                    // Calculate bloom properties
                    float pulse = 1f + (float)Math.Sin(state.PulsePhase) * 0.15f;
                    float baseScale = proj.scale * 0.5f;
                    
                    // Draw multi-layer bloom (outer → inner)
                    DrawProjectileBloomStack(sb, interpolatedPos, palette, baseScale, pulse, state.LifetimeProgress);
                }
                
                sb.End();
            }
            catch (System.Exception ex)
            {
                Mod?.Logger.Error($"AdvancedProjectileRenderer.DrawInterpolatedBloomLayer error: {ex.Message}");
                try { sb.End(); } catch { /* Already ended or never started */ }
            }
        }
        
        private void DrawProjectileBloomStack(SpriteBatch sb, Vector2 pos, Color[] palette, float baseScale, float pulse, float lifeProgress)
        {
            if (_softGlowTexture == null)
            {
                _softGlowTexture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            }
            if (_flareTexture == null)
            {
                _flareTexture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            }
            
            Vector2 origin = _softGlowTexture.Size() * 0.5f;
            Vector2 flareOrigin = _flareTexture.Size() * 0.5f;
            
            // Color from palette with gradient
            Color primaryColor = VFXUtilities.PaletteLerp(palette, lifeProgress * 0.5f);
            Color secondaryColor = VFXUtilities.PaletteLerp(palette, (lifeProgress * 0.5f + 0.3f) % 1f);
            
            // Layer 1: Outer ethereal glow (largest, dimmest)
            Color outerGlow = primaryColor.WithoutAlpha() * 0.25f;
            sb.Draw(_softGlowTexture, pos, null, outerGlow, 0f, origin, baseScale * pulse * 2.0f, SpriteEffects.None, 0f);
            
            // Layer 2: Middle energy layer
            Color middleGlow = secondaryColor.WithoutAlpha() * 0.4f;
            sb.Draw(_softGlowTexture, pos, null, middleGlow, 0f, origin, baseScale * pulse * 1.4f, SpriteEffects.None, 0f);
            
            // Layer 3: Inner core flare (rotating)
            float rotation = Main.GameUpdateCount * 0.05f;
            Color coreGlow = Color.Lerp(primaryColor, Color.White, 0.3f).WithoutAlpha() * 0.6f;
            sb.Draw(_flareTexture, pos, null, coreGlow, rotation, flareOrigin, baseScale * pulse * 0.8f, SpriteEffects.None, 0f);
            
            // Layer 4: White-hot center
            Color whiteCore = Color.White.WithoutAlpha() * 0.5f;
            sb.Draw(_flareTexture, pos, null, whiteCore, -rotation * 0.7f, flareOrigin, baseScale * pulse * 0.4f, SpriteEffects.None, 0f);
        }
        
        private void DrawTrailAfterglowLayer()
        {
            if (Main.gameMenu) return;
            
            SpriteBatch sb = Main.spriteBatch;
            
            try
            {
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                
                foreach (Projectile proj in Main.ActiveProjectiles)
                {
                    if (proj.ModProjectile?.Mod?.Name != "MagnumOpus") continue;
                    if (!_renderStates.TryGetValue(proj.whoAmI, out var state)) continue;
                    
                    string theme = DetectProjectileTheme(proj);
                    Color[] palette = MagnumThemePalettes.GetThemePalette(theme);
                    
                    // Draw Bézier-interpolated trail
                    DrawBezierTrail(sb, state, palette, theme);
                }
                
                sb.End();
            }
            catch (System.Exception ex)
            {
                Mod?.Logger.Error($"AdvancedProjectileRenderer.DrawTrailAfterglowLayer error: {ex.Message}");
                try { sb.End(); } catch { /* Already ended or never started */ }
            }
        }
        
        private void DrawBezierTrail(SpriteBatch sb, ProjectileRenderState state, Color[] palette, string theme)
        {
            if (_softGlowTexture == null) return;
            
            // Collect valid trail points
            List<Vector2> validPoints = new List<Vector2>();
            for (int i = 0; i < state.BezierTrail.Length; i++)
            {
                int idx = (state.TrailIndex - 1 - i + state.BezierTrail.Length) % state.BezierTrail.Length;
                if (state.BezierTrail[idx] != Vector2.Zero)
                    validPoints.Add(state.BezierTrail[idx]);
                if (validPoints.Count >= 12) break;
            }
            
            if (validPoints.Count < 3) return;
            
            // Generate smooth Bézier curve through points
            Vector2[] smoothTrail = BezierWeaponTrails.GenerateFlowingTrail(validPoints.ToArray(), 4);
            
            Vector2 origin = _softGlowTexture.Size() * 0.5f;
            
            // Draw trail segments with gradient
            for (int i = 0; i < smoothTrail.Length; i++)
            {
                float progress = (float)i / smoothTrail.Length;
                float opacity = (1f - progress) * 0.6f;
                float scale = (1f - progress * 0.7f) * 0.3f;
                
                Color trailColor = VFXUtilities.PaletteLerp(palette, progress);
                trailColor = trailColor.WithoutAlpha() * opacity;
                
                Vector2 drawPos = smoothTrail[i] - Main.screenPosition;
                sb.Draw(_softGlowTexture, drawPos, null, trailColor, 0f, origin, scale, SpriteEffects.None, 0f);
            }
        }
        
        /// <summary>
        /// Register a projectile for advanced rendering. Call this in AI().
        /// </summary>
        public static void RegisterProjectile(Projectile proj)
        {
            if (!_renderStates.TryGetValue(proj.whoAmI, out var state))
            {
                state = new ProjectileRenderState();
                state.SpawnFrame = (int)Main.GameUpdateCount;
                _renderStates[proj.whoAmI] = state;
            }
            
            state.RecordPosition(proj.Center, proj.rotation);
            state.PulsePhase += 0.08f;
            state.OrbitAngle += 0.06f;
            state.LifetimeProgress = 1f - (float)proj.timeLeft / 180f; // Assume 180 frame lifetime
        }
        
        /// <summary>
        /// Clean up when projectile is killed.
        /// </summary>
        public static void UnregisterProjectile(int whoAmI)
        {
            _renderStates.Remove(whoAmI);
        }
        
        /// <summary>
        /// Get the render state for advanced custom rendering.
        /// </summary>
        public static ProjectileRenderState GetRenderState(Projectile proj)
        {
            _renderStates.TryGetValue(proj.whoAmI, out var state);
            return state;
        }
        
        private static string DetectProjectileTheme(Projectile proj)
        {
            if (proj.ModProjectile == null) return "Generic";
            
            string fullName = proj.ModProjectile.GetType().FullName ?? "";
            string name = proj.ModProjectile.Name ?? "";
            
            // Check namespace and name for theme detection
            if (fullName.Contains("SwanLake") || name.Contains("Swan") || name.Contains("Feather") || name.Contains("Prismatic"))
                return "SwanLake";
            if (fullName.Contains("Eroica") || name.Contains("Eroica") || name.Contains("Valor") || name.Contains("Sakura") || name.Contains("Heroic"))
                return "Eroica";
            if (fullName.Contains("LaCampanella") || name.Contains("Campanella") || name.Contains("Bell") || name.Contains("Inferno") || name.Contains("Toll"))
                return "LaCampanella";
            if (fullName.Contains("EnigmaVariations") || name.Contains("Enigma") || name.Contains("Paradox") || name.Contains("Void") || name.Contains("Mystery"))
                return "EnigmaVariations";
            if (fullName.Contains("Fate") || name.Contains("Fate") || name.Contains("Cosmic") || name.Contains("Destiny") || name.Contains("Coda") || name.Contains("Constellation"))
                return "Fate";
            if (fullName.Contains("MoonlightSonata") || name.Contains("Moonlight") || name.Contains("Lunar") || name.Contains("Eternal"))
                return "MoonlightSonata";
            if (fullName.Contains("DiesIrae") || name.Contains("Wrath") || name.Contains("Fury") || name.Contains("Judgment"))
                return "DiesIrae";
            if (fullName.Contains("ClairDeLune") || name.Contains("Clair") || name.Contains("Lune") || name.Contains("Dream"))
                return "ClairDeLune";
            if (fullName.Contains("Spring") || name.Contains("Bloom") || name.Contains("Blossom") || name.Contains("Vernal") || name.Contains("Flower"))
                return "Spring";
            if (fullName.Contains("Summer") || name.Contains("Solar") || name.Contains("Sun") || name.Contains("Flame"))
                return "Summer";
            if (fullName.Contains("Autumn") || name.Contains("Decay") || name.Contains("Harvest") || name.Contains("Twilight"))
                return "Autumn";
            if (fullName.Contains("Winter") || name.Contains("Frost") || name.Contains("Ice") || name.Contains("Permafrost") || name.Contains("Blizzard"))
                return "Winter";
            if (fullName.Contains("OdeToJoy") || name.Contains("Joy") || name.Contains("Celebration"))
                return "OdeToJoy";
            if (fullName.Contains("Nachtmusik") || name.Contains("Nacht") || name.Contains("Night") || name.Contains("Serenade"))
                return "Nachtmusik";
            
            return "Generic";
        }
    }
}
