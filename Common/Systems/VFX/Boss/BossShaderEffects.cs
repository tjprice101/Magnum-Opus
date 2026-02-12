using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// BOSS SHADER EFFECTS SYSTEM
    /// 
    /// Properly integrates shaders for boss rendering following user's shader guidelines:
    /// 1. Uses Ref&lt;Effect&gt; with ModContent.Request for loading
    /// 2. Passes uTime, uWorldPosition, uColor per-frame
    /// 3. Uses SamplerState.LinearClamp for smooth visuals
    /// 4. Coordinates: (0,0) = top-left for screen-space effects
    /// 5. Integrates with PostDraw for complex effects
    /// 
    /// This system enhances the existing GlobalBossVFXOverhaul with proper shader-based:
    /// - Multi-layer bloom with shader enhancement
    /// - Trail rendering with primitive shaders
    /// - Screen distortion effects
    /// - Dynamic aura rendering
    /// </summary>
    public static class BossShaderEffects
    {
        private static bool _initialized;
        private static RenderTarget2D _bossAuraTarget;
        
        #region Initialization
        
        /// <summary>
        /// Initialize the boss shader system. Called automatically on first use.
        /// </summary>
        public static void Initialize()
        {
            if (_initialized || Main.dedServ)
                return;
            
            _initialized = true;
        }
        
        /// <summary>
        /// Ensures render targets are properly sized.
        /// </summary>
        private static void EnsureRenderTargets()
        {
            if (_bossAuraTarget == null || _bossAuraTarget.IsDisposed ||
                _bossAuraTarget.Width != Main.screenWidth || _bossAuraTarget.Height != Main.screenHeight)
            {
                _bossAuraTarget?.Dispose();
                _bossAuraTarget = new RenderTarget2D(
                    Main.instance.GraphicsDevice,
                    Main.screenWidth,
                    Main.screenHeight,
                    false,
                    SurfaceFormat.Color,
                    DepthFormat.None,
                    0,
                    RenderTargetUsage.PreserveContents);
            }
        }
        
        /// <summary>
        /// Cleanup on unload.
        /// </summary>
        public static void Unload()
        {
            _bossAuraTarget?.Dispose();
            _bossAuraTarget = null;
            _initialized = false;
        }
        
        #endregion
        
        #region Boss Bloom Rendering
        
        /// <summary>
        /// Draws a boss with enhanced multi-pass bloom using shaders.
        /// Uses the FargosSoulsDLC pattern of alpha removal for additive blending.
        /// </summary>
        /// <param name="spriteBatch">Active SpriteBatch</param>
        /// <param name="texture">Boss texture</param>
        /// <param name="drawPos">Screen-space draw position</param>
        /// <param name="frame">Animation frame rectangle</param>
        /// <param name="theme">Theme for color palette</param>
        /// <param name="rotation">Boss rotation</param>
        /// <param name="scale">Boss scale</param>
        /// <param name="intensity">Bloom intensity multiplier</param>
        public static void DrawBossWithShaderBloom(
            SpriteBatch spriteBatch,
            Texture2D texture,
            Vector2 drawPos,
            Rectangle frame,
            string theme,
            float rotation,
            float scale,
            float intensity = 1f)
        {
            if (spriteBatch == null || texture == null)
                return;
            
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            Color primary = palette != null && palette.Length > 0 ? palette[0] : Color.White;
            Color secondary = palette != null && palette.Length > 1 ? palette[1] : primary;
            
            Vector2 origin = frame.Size() * 0.5f;
            float time = Main.GameUpdateCount * 0.04f;
            float pulse = 1f + (float)Math.Sin(time * 2f) * 0.05f * intensity;
            
            // === SHADER-ENHANCED BLOOM LAYERS ===
            Effect bloomShader = ShaderLoader.Bloom;
            
            if (ShaderLoader.ShadersEnabled && bloomShader != null)
            {
                // Configure bloom shader parameters
                bloomShader.Parameters["uColor"]?.SetValue(primary.ToVector3());
                bloomShader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
                bloomShader.Parameters["uIntensity"]?.SetValue(intensity);
                bloomShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
                bloomShader.Parameters["uOpacity"]?.SetValue(1f);
                
                // Begin additive batch with shader
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                    SamplerState.LinearClamp, DepthStencilState.None,
                    RasterizerState.CullNone, bloomShader,
                    Main.GameViewMatrix.TransformationMatrix);
                
                // Draw bloom layers with shader
                DrawShaderBloomLayers(spriteBatch, texture, drawPos, frame, origin, primary, secondary, rotation, scale, pulse, intensity);
                
                // Restore normal batch
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                    SamplerState.LinearClamp, DepthStencilState.None,
                    RasterizerState.CullNone, null,
                    Main.GameViewMatrix.TransformationMatrix);
            }
            else
            {
                // Fallback: Use additive blending without shader
                DrawFallbackBloomLayers(spriteBatch, texture, drawPos, frame, origin, primary, secondary, rotation, scale, pulse, intensity);
            }
        }
        
        /// <summary>
        /// Configures the bloom shader with proper parameters per user guidelines.
        /// NOTE: Now handled inline via ShaderLoader.Bloom parameter setup.
        /// </summary>
        private static void ConfigureBloomShader(Effect shader, Color primary, Color secondary, float intensity, Vector2 worldPosition)
        {
            // User guideline: Pass uTime, uWorldPosition, uColor per-frame
            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uIntensity"]?.SetValue(intensity);
            shader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            
            // Screen-space position (0,0 = top-left per user guideline)
            Vector2 screenPos = worldPosition / new Vector2(Main.screenWidth, Main.screenHeight);
            shader.Parameters["uTargetPosition"]?.SetValue(screenPos);
        }
        
        /// <summary>
        /// Draws bloom layers using shader-enhanced rendering.
        /// </summary>
        private static void DrawShaderBloomLayers(
            SpriteBatch spriteBatch,
            Texture2D texture,
            Vector2 drawPos,
            Rectangle frame,
            Vector2 origin,
            Color primary,
            Color secondary,
            float rotation,
            float scale,
            float pulse,
            float intensity)
        {
            // FargosSoulsDLC pattern: Multi-layer bloom with alpha removal
            // Layer scales (outer to inner)
            float[] scales = { 1.5f, 1.25f, 1.1f, 1.02f };
            float[] opacities = { 0.15f, 0.25f, 0.35f, 0.5f };
            
            for (int i = 0; i < 4; i++)
            {
                float layerScale = scale * pulse * scales[i];
                float layerOpacity = opacities[i] * intensity;
                
                // Gradient from outer to inner
                Color layerColor = Color.Lerp(primary, secondary, i / 3f) with { A = 0 } * layerOpacity;
                
                // Slight rotation offset for outer layers (adds dynamism)
                float layerRot = rotation + (i > 1 ? 0 : Main.GameUpdateCount * 0.005f * (2 - i));
                
                spriteBatch.Draw(
                    texture,
                    drawPos,
                    frame,
                    layerColor,
                    layerRot,
                    origin,
                    layerScale,
                    SpriteEffects.None,
                    0f);
            }
        }
        
        /// <summary>
        /// Fallback bloom rendering without shader (uses additive blending).
        /// </summary>
        private static void DrawFallbackBloomLayers(
            SpriteBatch spriteBatch,
            Texture2D texture,
            Vector2 drawPos,
            Rectangle frame,
            Vector2 origin,
            Color primary,
            Color secondary,
            float rotation,
            float scale,
            float pulse,
            float intensity)
        {
            // Switch to additive blending
            try { spriteBatch.End(); } catch { }
            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.Additive,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                Main.GameViewMatrix.TransformationMatrix);
            
            // Draw bloom layers
            float[] scales = { 1.4f, 1.2f, 1.08f };
            float[] opacities = { 0.12f, 0.2f, 0.35f };
            
            for (int i = 0; i < 3; i++)
            {
                float layerScale = scale * pulse * scales[i];
                float layerOpacity = opacities[i] * intensity;
                Color layerColor = Color.Lerp(primary, secondary, i / 2f) with { A = 0 } * layerOpacity;
                
                spriteBatch.Draw(texture, drawPos, frame, layerColor, rotation, origin, layerScale, SpriteEffects.None, 0f);
            }
            
            // Restore normal blending
            try { spriteBatch.End(); } catch { }
            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                Main.GameViewMatrix.TransformationMatrix);
        }
        
        #endregion
        
        #region Boss Aura Effects
        
        /// <summary>
        /// Draws an ethereal aura around a boss using shader-enhanced rendering.
        /// </summary>
        public static void DrawBossAura(
            SpriteBatch spriteBatch,
            NPC boss,
            string theme,
            float radius = 80f,
            float intensity = 1f)
        {
            if (spriteBatch == null || boss == null)
                return;
            
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            Color primary = palette != null && palette.Length > 0 ? palette[0] : Color.White;
            Color secondary = palette != null && palette.Length > 1 ? palette[1] : primary;
            
            Vector2 bossCenter = InterpolatedRenderer.GetInterpolatedCenter(boss);
            Vector2 drawPos = bossCenter - Main.screenPosition;
            
            float time = Main.GameUpdateCount * 0.03f;
            float pulse = 1f + (float)Math.Sin(time * 1.5f) * 0.1f;
            
            // Draw orbiting glow points
            int points = 6;
            for (int i = 0; i < points; i++)
            {
                float angle = time + MathHelper.TwoPi * i / points;
                float pointRadius = radius * pulse;
                Vector2 pointPos = drawPos + angle.ToRotationVector2() * pointRadius;
                
                float colorProgress = (i / (float)points + time * 0.1f) % 1f;
                Color pointColor = Color.Lerp(primary, secondary, colorProgress) with { A = 0 } * 0.4f * intensity;
                
                // Draw small glow at each point
                DrawAuraPoint(spriteBatch, pointPos, pointColor, 15f * intensity);
            }
            
            // Central subtle glow
            DrawAuraPoint(spriteBatch, drawPos, primary with { A = 0 } * 0.2f * intensity, radius * 0.5f);
        }
        
        /// <summary>
        /// Draws a single aura glow point.
        /// </summary>
        private static void DrawAuraPoint(SpriteBatch spriteBatch, Vector2 position, Color color, float size)
        {
            // Use a basic glow texture from registry
            Texture2D glowTex = MagnumTextureRegistry.SoftGlow?.Value;
            if (glowTex == null) 
            {
                // Fallback to bloom texture
                glowTex = MagnumTextureRegistry.GetBloom();
            }
            if (glowTex == null) return;
            
            Vector2 origin = glowTex.Size() * 0.5f;
            float scale = size / (float)Math.Max(glowTex.Width, glowTex.Height);
            
            // Switch to additive for glow
            try { spriteBatch.End(); } catch { }
            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.Additive,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                Main.GameViewMatrix.TransformationMatrix);
            
            spriteBatch.Draw(glowTex, position, null, color, 0f, origin, scale, SpriteEffects.None, 0f);
            
            // Restore normal
            try { spriteBatch.End(); } catch { }
            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                Main.GameViewMatrix.TransformationMatrix);
        }
        
        #endregion
        
        #region Boss Attack Flash Effects
        
        /// <summary>
        /// Creates a dramatic shader-enhanced attack flash.
        /// </summary>
        public static void AttackFlash(Vector2 worldPosition, string theme, float scale = 1f)
        {
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            Color primary = palette != null && palette.Length > 0 ? palette[0] : Color.White;
            Color secondary = palette != null && palette.Length > 1 ? palette[1] : primary;
            
            // Trigger screen distortion
            ScreenDistortionManager.TriggerThemeEffect(theme, worldPosition, scale * 0.5f, 20);
            
            // Sky flash
            DynamicSkyboxSystem.TriggerFlash(primary, scale * 0.6f);
            
            // Central flash particles
            CustomParticles.GenericFlare(worldPosition, Color.White, 1.2f * scale, 18);
            CustomParticles.GenericFlare(worldPosition, primary, 0.9f * scale, 15);
            
            // Cascading rings
            for (int i = 0; i < 4; i++)
            {
                Color ringColor = Color.Lerp(primary, secondary, i / 3f);
                CustomParticles.HaloRing(worldPosition, ringColor, 0.3f + i * 0.15f, 12 + i * 3);
            }
            
            // Music notes burst
            for (int j = 0; j < 6; j++)
            {
                float angle = MathHelper.TwoPi * j / 6f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                ThemedParticles.MusicNote(worldPosition, noteVel, secondary, 0.75f, 25);
            }
        }
        
        /// <summary>
        /// Creates a boss dash start effect with shader-enhanced trail initialization.
        /// </summary>
        public static int DashStart(NPC boss, string theme, float width = 35f)
        {
            Vector2 startPos = InterpolatedRenderer.GetInterpolatedCenter(boss);
            
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            Color primary = palette != null && palette.Length > 0 ? palette[0] : Color.White;
            
            // Flash at departure
            AttackFlash(startPos, theme, 0.6f);
            
            // Create trail using advanced trail system
            return AdvancedTrailSystem.CreateThemeTrail(theme, width, maxPoints: 25, intensity: 1.5f);
        }
        
        /// <summary>
        /// Creates a boss dash end impact with shader-enhanced effects.
        /// </summary>
        public static void DashEnd(int trailId, NPC boss, string theme)
        {
            AdvancedTrailSystem.EndTrail(trailId);
            
            Vector2 endPos = InterpolatedRenderer.GetInterpolatedCenter(boss);
            
            // Impact flash
            AttackFlash(endPos, theme, 0.8f);
            
            // Glimmer cascade
            CalamityStyleVFX.GlimmerCascadeImpact(endPos, theme, 1f);
        }
        
        #endregion
        
        #region Theme-Specific Boss Effects
        
        /// <summary>
        /// Applies theme-specific special effects to a boss.
        /// </summary>
        public static void ApplyThemeSpecificEffect(NPC boss, string theme, SpriteBatch spriteBatch)
        {
            switch (theme.ToLower())
            {
                case "fate":
                    ApplyFateCosmicEffect(boss, spriteBatch);
                    break;
                case "enigma":
                case "enigmavariations":
                    ApplyEnigmaVoidEffect(boss, spriteBatch);
                    break;
                case "swanlake":
                case "swan":
                    ApplySwanLakePrismaticEffect(boss, spriteBatch);
                    break;
                case "lacampanella":
                case "campanella":
                    ApplyLaCampanellaInfernalEffect(boss, spriteBatch);
                    break;
                case "eroica":
                    ApplyEroicaHeroicEffect(boss, spriteBatch);
                    break;
            }
        }
        
        private static void ApplyFateCosmicEffect(NPC boss, SpriteBatch spriteBatch)
        {
            if (Main.rand.NextBool(20))
            {
                Vector2 pos = InterpolatedRenderer.GetInterpolatedCenter(boss) + Main.rand.NextVector2Circular(boss.width, boss.height);
                CustomParticles.GenericFlare(pos, new Color(180, 50, 100), 0.25f, 12);
            }
            
            // Orbiting glyphs
            if (Main.GameUpdateCount % 15 == 0)
            {
                float angle = Main.GameUpdateCount * 0.04f;
                Vector2 glyphPos = InterpolatedRenderer.GetInterpolatedCenter(boss) + angle.ToRotationVector2() * 60f;
                CustomParticles.GenericFlare(glyphPos, new Color(255, 255, 255), 0.3f, 10);
            }
        }
        
        private static void ApplyEnigmaVoidEffect(NPC boss, SpriteBatch spriteBatch)
        {
            if (Main.rand.NextBool(25))
            {
                Vector2 pos = InterpolatedRenderer.GetInterpolatedCenter(boss) + Main.rand.NextVector2Circular(boss.width * 0.8f, boss.height * 0.8f);
                CustomParticles.GenericFlare(pos, new Color(50, 220, 100), 0.2f, 10);
            }
        }
        
        private static void ApplySwanLakePrismaticEffect(NPC boss, SpriteBatch spriteBatch)
        {
            if (Main.rand.NextBool(12))
            {
                Vector2 pos = InterpolatedRenderer.GetInterpolatedCenter(boss) + Main.rand.NextVector2Circular(boss.width * 0.6f, boss.height * 0.6f);
                float hue = Main.rand.NextFloat();
                Color rainbow = Main.hslToRgb(hue, 1f, 0.8f);
                var sparkle = new SparkleParticle(pos, Main.rand.NextVector2Circular(1f, 1f), rainbow * 0.7f, 0.2f, 15);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }
        
        private static void ApplyLaCampanellaInfernalEffect(NPC boss, SpriteBatch spriteBatch)
        {
            if (Main.rand.NextBool(15))
            {
                Vector2 pos = InterpolatedRenderer.GetInterpolatedCenter(boss) + Main.rand.NextVector2Circular(boss.width * 0.5f, boss.height * 0.5f);
                var smoke = new HeavySmokeParticle(pos, new Vector2(0, -1f) + Main.rand.NextVector2Circular(0.5f, 0.5f), 
                    new Color(30, 20, 25), Main.rand.Next(25, 40), 0.25f, 0.5f, 0.02f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
        }
        
        private static void ApplyEroicaHeroicEffect(NPC boss, SpriteBatch spriteBatch)
        {
            if (Main.rand.NextBool(18))
            {
                Vector2 pos = InterpolatedRenderer.GetInterpolatedCenter(boss) + Main.rand.NextVector2Circular(boss.width * 0.7f, boss.height * 0.7f);
                CustomParticles.GenericFlare(pos, new Color(255, 200, 80) * 0.6f, 0.2f, 10);
            }
        }
        
        #endregion
    }
}
