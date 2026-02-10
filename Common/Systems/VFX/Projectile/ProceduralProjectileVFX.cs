using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// PROCEDURAL PROJECTILE VFX SYSTEM
    /// 
    /// This system provides ready-to-use procedural drawing methods for projectiles.
    /// Instead of loading PNG textures, these methods draw effects entirely in code.
    /// 
    /// USAGE:
    /// Replace your PreDraw override with calls to these methods.
    /// The effects are theme-aware and automatically adapt to color schemes.
    /// 
    /// BENEFITS:
    /// - No PNG texture loading (faster, smaller mod size)
    /// - Infinite resolution at any scale
    /// - Dynamic animation built-in
    /// - Easy theming with color parameters
    /// </summary>
    public static class ProceduralProjectileVFX
    {
        private static Texture2D _pixel;
        
        /// <summary>
        /// Gets or creates a 1x1 white pixel texture.
        /// </summary>
        private static Texture2D Pixel
        {
            get
            {
                if (_pixel == null || _pixel.IsDisposed)
                {
                    _pixel = new Texture2D(Main.graphics.GraphicsDevice, 1, 1);
                    _pixel.SetData(new[] { Color.White });
                }
                return _pixel;
            }
        }
        
        /// <summary>
        /// Draws a complete procedural projectile with spinning flare layers, trail, and glow.
        /// Replaces the entire PreDraw method for most projectiles.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch</param>
        /// <param name="projectile">The projectile being drawn</param>
        /// <param name="primaryColor">Main theme color</param>
        /// <param name="secondaryColor">Secondary theme color</param>
        /// <param name="accentColor">Accent/highlight color (often white)</param>
        /// <param name="scale">Base scale multiplier</param>
        /// <param name="rayCount">Number of flare rays (4, 6, or 8 typical)</param>
        public static void DrawProceduralProjectile(SpriteBatch spriteBatch, Projectile projectile,
            Color primaryColor, Color secondaryColor, Color accentColor, float scale = 1f, int rayCount = 6)
        {
            Vector2 drawPos = projectile.Center - Main.screenPosition;
            float time = Main.GameUpdateCount * 0.05f;
            float pulse = 1f + (float)Math.Sin(time * 2f) * 0.12f;
            
            // Alpha-removed colors for additive blending
            Color primaryBloom = primaryColor with { A = 0 };
            Color secondaryBloom = secondaryColor with { A = 0 };
            Color accentBloom = accentColor with { A = 0 };
            
            // Begin additive blend mode
            try { spriteBatch.End(); } catch { }
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // === TRAIL RENDERING ===
            DrawProceduralTrail(spriteBatch, projectile, primaryColor, secondaryColor, scale);
            
            // === LAYER 1: Outer soft glow base ===
            DrawSoftGlow(spriteBatch, drawPos, primaryBloom * 0.3f, scale * 35f * pulse);
            
            // === LAYER 2: Primary spinning flare ===
            DrawSpinningFlare(spriteBatch, drawPos, primaryBloom * 0.5f, scale * 25f * pulse, rayCount, time);
            
            // === LAYER 3: Secondary counter-rotating flare ===
            DrawSpinningFlare(spriteBatch, drawPos, secondaryBloom * 0.45f, scale * 20f * pulse, rayCount, -time * 0.75f);
            
            // === LAYER 4: Third flare at different speed ===
            DrawSpinningFlare(spriteBatch, drawPos, primaryBloom * 0.4f, scale * 15f * pulse, rayCount / 2, time * 1.3f);
            
            // === LAYER 5: Main projectile glow ===
            DrawCoreGlow(spriteBatch, drawPos, primaryBloom * 0.65f, scale * 12f * pulse);
            
            // === LAYER 6: White-hot core ===
            DrawCoreGlow(spriteBatch, drawPos, accentBloom * 0.85f, scale * 5f);
            
            // === ORBITING SPARK POINTS ===
            DrawOrbitingPoints(spriteBatch, drawPos, primaryColor, secondaryColor, scale * 12f, 4, time * 1.4f);
            
            // Restore normal blend mode
            try { spriteBatch.End(); } catch { }
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        
        /// <summary>
        /// Draws the projectile's trail using position history.
        /// </summary>
        public static void DrawProceduralTrail(SpriteBatch spriteBatch, Projectile projectile,
            Color startColor, Color endColor, float scale)
        {
            for (int i = 0; i < projectile.oldPos.Length; i++)
            {
                if (projectile.oldPos[i] == Vector2.Zero) continue;
                
                float progress = (float)i / projectile.oldPos.Length;
                float alpha = (1f - progress) * 0.55f;
                float trailScale = scale * 8f * (1f - progress * 0.5f);
                
                Color trailColor = Color.Lerp(startColor, endColor, progress) with { A = 0 };
                Vector2 trailPos = projectile.oldPos[i] + projectile.Size / 2f - Main.screenPosition;
                
                // Draw trail segment as overlapping circles
                DrawCoreGlow(spriteBatch, trailPos, trailColor * alpha, trailScale);
            }
        }
        
        /// <summary>
        /// Draws a soft circular glow.
        /// </summary>
        public static void DrawSoftGlow(SpriteBatch spriteBatch, Vector2 position, Color color, float radius)
        {
            int segments = 20;
            for (int layer = 0; layer < 4; layer++)
            {
                float layerRadius = radius * (1f - layer * 0.2f);
                float layerAlpha = 0.15f / (layer + 1);
                
                for (int i = 0; i < segments; i++)
                {
                    float angle = MathHelper.TwoPi * i / segments;
                    Vector2 offset = angle.ToRotationVector2() * layerRadius;
                    
                    spriteBatch.Draw(Pixel, position + offset, null, color * layerAlpha,
                        0f, new Vector2(0.5f, 0.5f), 3f + layer, SpriteEffects.None, 0f);
                }
            }
        }
        
        /// <summary>
        /// Draws a bright core glow.
        /// </summary>
        public static void DrawCoreGlow(SpriteBatch spriteBatch, Vector2 position, Color color, float radius)
        {
            int segments = 12;
            for (int layer = 0; layer < 3; layer++)
            {
                float layerRadius = radius * (1f - layer * 0.3f);
                float layerAlpha = 0.4f / (layer + 1);
                
                for (int i = 0; i < segments; i++)
                {
                    float angle = MathHelper.TwoPi * i / segments;
                    Vector2 offset = angle.ToRotationVector2() * layerRadius;
                    
                    spriteBatch.Draw(Pixel, position + offset, null, color * layerAlpha,
                        0f, new Vector2(0.5f, 0.5f), 2f, SpriteEffects.None, 0f);
                }
            }
            
            // Bright center point
            spriteBatch.Draw(Pixel, position, null, color * 0.8f,
                0f, new Vector2(0.5f, 0.5f), 3f, SpriteEffects.None, 0f);
        }
        
        /// <summary>
        /// Draws a spinning flare with dynamic rays.
        /// </summary>
        public static void DrawSpinningFlare(SpriteBatch spriteBatch, Vector2 position, Color color, 
            float radius, int rayCount, float rotation)
        {
            float angleStep = MathHelper.TwoPi / rayCount;
            
            for (int i = 0; i < rayCount; i++)
            {
                float rayAngle = angleStep * i + rotation;
                float rayVariation = 0.8f + (float)Math.Sin(Main.GameUpdateCount * 0.1f + i * 1.5f) * 0.2f;
                float rayLength = radius * rayVariation;
                
                // Draw ray as series of points fading outward
                int raySegments = 6;
                for (int seg = 0; seg < raySegments; seg++)
                {
                    float segProgress = (float)seg / raySegments;
                    float segRadius = rayLength * segProgress;
                    float segAlpha = (1f - segProgress) * 0.5f;
                    float segSize = 2f * (1f - segProgress * 0.5f);
                    
                    Vector2 segPos = position + rayAngle.ToRotationVector2() * segRadius;
                    
                    spriteBatch.Draw(Pixel, segPos, null, color * segAlpha,
                        0f, new Vector2(0.5f, 0.5f), segSize, SpriteEffects.None, 0f);
                }
            }
        }
        
        /// <summary>
        /// Draws orbiting spark points around a center.
        /// </summary>
        public static void DrawOrbitingPoints(SpriteBatch spriteBatch, Vector2 position,
            Color startColor, Color endColor, float radius, int count, float rotation)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = rotation + MathHelper.TwoPi * i / count;
                Vector2 sparkPos = position + angle.ToRotationVector2() * radius;
                
                float colorProgress = (float)i / count;
                Color sparkColor = Color.Lerp(startColor, endColor, colorProgress) with { A = 0 };
                
                // Draw spark with small glow
                spriteBatch.Draw(Pixel, sparkPos, null, sparkColor * 0.7f,
                    0f, new Vector2(0.5f, 0.5f), 2f, SpriteEffects.None, 0f);
                spriteBatch.Draw(Pixel, sparkPos, null, sparkColor * 0.3f,
                    0f, new Vector2(0.5f, 0.5f), 4f, SpriteEffects.None, 0f);
            }
        }
        
        #region Theme-Specific Presets
        
        /// <summary>
        /// Winter theme preset - icy blue with crystal sparkles
        /// </summary>
        public static void DrawWinterProjectile(SpriteBatch spriteBatch, Projectile projectile, float scale = 1f)
        {
            Color iceBlue = new Color(150, 220, 255);
            Color crystalCyan = new Color(100, 255, 255);
            Color frostWhite = new Color(240, 250, 255);
            
            DrawProceduralProjectile(spriteBatch, projectile, iceBlue, crystalCyan, frostWhite, scale, 6);
        }
        
        /// <summary>
        /// Winter homing projectile preset - enhanced effects when targeting
        /// </summary>
        public static void DrawWinterHomingProjectile(SpriteBatch spriteBatch, Projectile projectile, bool hasTarget, float scale = 1f)
        {
            Color iceBlue = new Color(150, 220, 255);
            Color crystalCyan = new Color(100, 255, 255);
            Color deepBlue = new Color(80, 140, 200);
            Color stormPurple = new Color(160, 120, 200);
            Color frostWhite = new Color(240, 250, 255);
            
            Vector2 drawPos = projectile.Center - Main.screenPosition;
            float time = Main.GameUpdateCount * 0.055f;
            float pulse = 1f + (float)Math.Sin(time * 2f) * 0.14f;
            
            // Colors change based on target state
            Color primaryColor = hasTarget ? crystalCyan : deepBlue;
            Color secondaryColor = hasTarget ? iceBlue : stormPurple;
            
            try { spriteBatch.End(); } catch { }
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Trail with gradient
            DrawProceduralTrail(spriteBatch, projectile, primaryColor, secondaryColor, scale * 0.45f);
            
            // Layer 1: Soft glow base (larger when homing)
            float glowSize = hasTarget ? 30f : 24f;
            DrawSoftGlow(spriteBatch, drawPos, primaryColor with { A = 0 } * 0.4f, glowSize * scale * pulse);
            
            // Layer 2-4: Spinning flares
            DrawSpinningFlare(spriteBatch, drawPos, crystalCyan with { A = 0 } * 0.55f, 18f * scale * pulse, 4, time);
            DrawSpinningFlare(spriteBatch, drawPos, stormPurple with { A = 0 } * 0.5f, 16f * scale * pulse, 4, -time * 0.7f);
            DrawSpinningFlare(spriteBatch, drawPos, iceBlue with { A = 0 } * 0.45f, 14f * scale * pulse, 4, time * 1.4f);
            
            // Layer 5: Main core
            DrawCoreGlow(spriteBatch, drawPos, secondaryColor with { A = 0 } * 0.7f, 16f * scale * pulse);
            
            // Layer 6: White-hot center
            DrawCoreGlow(spriteBatch, drawPos, frostWhite with { A = 0 } * 0.9f, 8f * scale);
            
            // Orbiting sparks (more when homing)
            int sparkCount = hasTarget ? 5 : 4;
            float sparkRadius = hasTarget ? 14f * scale : 12f * scale;
            float sparkScale = hasTarget ? 5f : 4f;
            for (int i = 0; i < sparkCount; i++)
            {
                float sparkAngle = time * 1.5f + MathHelper.TwoPi * i / sparkCount;
                Vector2 sparkPos = drawPos + sparkAngle.ToRotationVector2() * sparkRadius;
                float progress = i / (float)sparkCount;
                Color sparkColor = Color.Lerp(crystalCyan, iceBlue, progress) with { A = 0 };
                DrawCoreGlow(spriteBatch, sparkPos, sparkColor * 0.75f, sparkScale * scale * pulse);
            }
            
            try { spriteBatch.End(); } catch { }
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        
        /// <summary>
        /// Nachtmusik theme preset - starry night with twinkling
        /// </summary>
        public static void DrawNachtmusikProjectile(SpriteBatch spriteBatch, Projectile projectile, float scale = 1f)
        {
            Color nightBlue = new Color(60, 80, 140);
            Color starGold = new Color(255, 255, 200);
            Color moonSilver = new Color(220, 230, 255);
            
            Vector2 drawPos = projectile.Center - Main.screenPosition;
            float time = Main.GameUpdateCount * 0.05f;
            
            // Standard projectile
            DrawProceduralProjectile(spriteBatch, projectile, nightBlue, starGold, moonSilver, scale, 8);
            
            // Add extra twinkling stars
            try { spriteBatch.End(); } catch { }
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Random twinkling around projectile
            for (int i = 0; i < 5; i++)
            {
                float twinkleAngle = time * 0.5f + i * MathHelper.TwoPi / 5f;
                float twinkleRadius = 15f + (float)Math.Sin(time * 3f + i) * 5f;
                Vector2 twinklePos = drawPos + twinkleAngle.ToRotationVector2() * twinkleRadius;
                float twinkleAlpha = 0.3f + (float)Math.Sin(time * 5f + i * 2f) * 0.3f;
                
                // 4-point star twinkle
                DrawSpinningFlare(spriteBatch, twinklePos, starGold with { A = 0 } * twinkleAlpha, 
                    scale * 8f, 4, twinkleAngle);
            }
            
            try { spriteBatch.End(); } catch { }
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        
        /// <summary>
        /// La Campanella theme preset - infernal with smoky glow
        /// </summary>
        public static void DrawLaCampanellaProjectile(SpriteBatch spriteBatch, Projectile projectile, float scale = 1f)
        {
            Color infernalOrange = new Color(255, 140, 40);
            Color bellGold = new Color(255, 200, 80);
            Color flameWhite = new Color(255, 240, 220);
            
            DrawProceduralProjectile(spriteBatch, projectile, infernalOrange, bellGold, flameWhite, scale, 6);
        }
        
        /// <summary>
        /// Eroica theme preset - heroic scarlet and gold
        /// </summary>
        public static void DrawEroicaProjectile(SpriteBatch spriteBatch, Projectile projectile, float scale = 1f)
        {
            Color eroicaScarlet = new Color(200, 50, 50);
            Color eroicaGold = new Color(255, 200, 80);
            Color heroicWhite = new Color(255, 240, 230);
            
            DrawProceduralProjectile(spriteBatch, projectile, eroicaScarlet, eroicaGold, heroicWhite, scale, 8);
        }
        
        /// <summary>
        /// Swan Lake theme preset - monochrome with prismatic shimmer
        /// </summary>
        public static void DrawSwanLakeProjectile(SpriteBatch spriteBatch, Projectile projectile, float scale = 1f)
        {
            // Rainbow color that cycles over time
            float hue = (Main.GameUpdateCount * 0.01f) % 1f;
            Color rainbow = Main.hslToRgb(hue, 0.9f, 0.8f);
            
            Color swanWhite = Color.White;
            Color swanBlack = new Color(30, 30, 40);
            
            Vector2 drawPos = projectile.Center - Main.screenPosition;
            float time = Main.GameUpdateCount * 0.05f;
            float pulse = 1f + (float)Math.Sin(time * 2f) * 0.12f;
            
            try { spriteBatch.End(); } catch { }
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Trail with rainbow gradient
            DrawProceduralTrail(spriteBatch, projectile, swanWhite, rainbow, scale);
            
            // White outer glow
            DrawSoftGlow(spriteBatch, drawPos, swanWhite with { A = 0 } * 0.3f, scale * 35f * pulse);
            
            // Rainbow spinning flare
            DrawSpinningFlare(spriteBatch, drawPos, rainbow with { A = 0 } * 0.5f, scale * 25f * pulse, 6, time);
            
            // Black contrasting elements
            DrawSpinningFlare(spriteBatch, drawPos, swanBlack with { A = 0 } * 0.3f, scale * 20f * pulse, 6, -time);
            
            // Prismatic core
            for (int i = 0; i < 6; i++)
            {
                float layerHue = (hue + i * 0.15f) % 1f;
                Color layerColor = Main.hslToRgb(layerHue, 1f, 0.85f) with { A = 0 };
                DrawCoreGlow(spriteBatch, drawPos, layerColor * 0.3f, scale * (15f - i * 2f));
            }
            
            // White center
            DrawCoreGlow(spriteBatch, drawPos, Color.White with { A = 0 } * 0.9f, scale * 5f);
            
            try { spriteBatch.End(); } catch { }
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        
        /// <summary>
        /// Moonlight Sonata theme preset - purple and silver lunar glow
        /// </summary>
        public static void DrawMoonlightSonataProjectile(SpriteBatch spriteBatch, Projectile projectile, float scale = 1f)
        {
            Color moonPurple = new Color(100, 80, 160);
            Color lunarSilver = new Color(200, 210, 230);
            Color moonWhite = new Color(240, 245, 255);
            
            DrawProceduralProjectile(spriteBatch, projectile, moonPurple, lunarSilver, moonWhite, scale, 6);
        }
        
        /// <summary>
        /// Enigma theme preset - mysterious void with green accents
        /// </summary>
        public static void DrawEnigmaProjectile(SpriteBatch spriteBatch, Projectile projectile, float scale = 1f)
        {
            Color enigmaPurple = new Color(120, 60, 180);
            Color voidGreen = new Color(80, 200, 120);
            Color mysteryWhite = new Color(220, 200, 255);
            
            DrawProceduralProjectile(spriteBatch, projectile, enigmaPurple, voidGreen, mysteryWhite, scale, 6);
        }
        
        /// <summary>
        /// Fate theme preset - cosmic dark prismatic with celestial highlights
        /// </summary>
        public static void DrawFateProjectile(SpriteBatch spriteBatch, Projectile projectile, float scale = 1f)
        {
            Color fatePink = new Color(200, 80, 140);
            Color fateRed = new Color(255, 60, 100);
            Color cosmicWhite = Color.White;
            
            Vector2 drawPos = projectile.Center - Main.screenPosition;
            float time = Main.GameUpdateCount * 0.05f;
            float pulse = 1f + (float)Math.Sin(time * 2f) * 0.12f;
            
            try { spriteBatch.End(); } catch { }
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Trail
            DrawProceduralTrail(spriteBatch, projectile, fatePink, fateRed, scale);
            
            // Dark cosmic glow layers
            DrawSoftGlow(spriteBatch, drawPos, new Color(30, 10, 40) with { A = 0 } * 0.4f, scale * 40f * pulse);
            DrawSoftGlow(spriteBatch, drawPos, fatePink with { A = 0 } * 0.3f, scale * 30f * pulse);
            
            // Spinning flares with cosmic gradient
            DrawSpinningFlare(spriteBatch, drawPos, fatePink with { A = 0 } * 0.5f, scale * 25f * pulse, 8, time);
            DrawSpinningFlare(spriteBatch, drawPos, fateRed with { A = 0 } * 0.45f, scale * 20f * pulse, 8, -time * 0.7f);
            
            // Reality distortion - slight offset layers
            Vector2 distortOffset = new Vector2((float)Math.Sin(time * 5f) * 2f, (float)Math.Cos(time * 5f) * 2f);
            DrawCoreGlow(spriteBatch, drawPos + distortOffset, fatePink with { A = 0 } * 0.4f, scale * 12f);
            DrawCoreGlow(spriteBatch, drawPos - distortOffset, fateRed with { A = 0 } * 0.4f, scale * 12f);
            
            // Star sparkle orbits
            for (int i = 0; i < 6; i++)
            {
                float starAngle = time + MathHelper.TwoPi * i / 6f;
                float starRadius = 20f + (float)Math.Sin(time * 2f + i) * 5f;
                Vector2 starPos = drawPos + starAngle.ToRotationVector2() * starRadius * scale;
                DrawSpinningFlare(spriteBatch, starPos, cosmicWhite with { A = 0 } * 0.5f, scale * 6f, 4, starAngle * 2f);
            }
            
            // Cosmic white core
            DrawCoreGlow(spriteBatch, drawPos, cosmicWhite with { A = 0 } * 0.9f, scale * 6f);
            
            try { spriteBatch.End(); } catch { }
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        
        /// <summary>
        /// Spring theme preset - soft pinks, greens, and cherry blossom tones
        /// </summary>
        public static void DrawSpringProjectile(SpriteBatch spriteBatch, Projectile projectile, float scale = 1f)
        {
            Color cherryPink = new Color(255, 180, 200);
            Color leafGreen = new Color(120, 200, 120);
            Color blossomWhite = new Color(255, 250, 245);
            
            DrawProceduralProjectile(spriteBatch, projectile, cherryPink, leafGreen, blossomWhite, scale, 6);
        }
        
        /// <summary>
        /// Summer theme preset - warm golds, oranges, and sun-bright yellows
        /// </summary>
        public static void DrawSummerProjectile(SpriteBatch spriteBatch, Projectile projectile, float scale = 1f)
        {
            Color sunGold = new Color(255, 220, 100);
            Color sunOrange = new Color(255, 160, 50);
            Color sunWhite = new Color(255, 255, 230);
            
            Vector2 drawPos = projectile.Center - Main.screenPosition;
            float time = Main.GameUpdateCount * 0.06f;
            float pulse = 1f + (float)Math.Sin(time * 2f) * 0.18f;
            
            // More radiant than standard projectile for solar feel
            try { spriteBatch.End(); } catch { }
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            DrawProceduralTrail(spriteBatch, projectile, sunGold, sunOrange, scale);
            
            // Extra large outer glow for sun effect
            DrawSoftGlow(spriteBatch, drawPos, sunOrange with { A = 0 } * 0.25f, scale * 50f * pulse);
            DrawSoftGlow(spriteBatch, drawPos, sunGold with { A = 0 } * 0.35f, scale * 35f * pulse);
            
            // Multiple spinning flares
            DrawSpinningFlare(spriteBatch, drawPos, sunGold with { A = 0 } * 0.55f, scale * 28f * pulse, 8, time);
            DrawSpinningFlare(spriteBatch, drawPos, new Color(255, 100, 50) with { A = 0 } * 0.4f, scale * 22f * pulse, 8, -time * 0.8f);
            DrawSpinningFlare(spriteBatch, drawPos, sunOrange with { A = 0 } * 0.5f, scale * 18f * pulse, 6, time * 1.4f);
            
            // Hot core
            DrawCoreGlow(spriteBatch, drawPos, sunGold with { A = 0 } * 0.7f, scale * 15f * pulse);
            DrawCoreGlow(spriteBatch, drawPos, sunWhite with { A = 0 } * 0.9f, scale * 8f);
            
            // Solar corona orbits
            DrawOrbitingPoints(spriteBatch, drawPos, sunGold, sunOrange, scale * 16f, 6, time * 1.2f);
            
            try { spriteBatch.End(); } catch { }
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        
        /// <summary>
        /// Autumn theme preset - warm amber, rust, and harvest tones
        /// </summary>
        public static void DrawAutumnProjectile(SpriteBatch spriteBatch, Projectile projectile, float scale = 1f)
        {
            Color autumnAmber = new Color(220, 140, 40);
            Color rustRed = new Color(180, 80, 40);
            Color harvestGold = new Color(255, 200, 100);
            
            DrawProceduralProjectile(spriteBatch, projectile, autumnAmber, rustRed, harvestGold, scale, 6);
        }
        
        #endregion
        
        /// <summary>
        /// Dynamic seasonal projectile - routes to correct season based on index.
        /// Use this for projectiles that change appearance based on the current season.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch</param>
        /// <param name="projectile">The projectile being drawn</param>
        /// <param name="seasonIndex">0=Spring, 1=Summer, 2=Autumn, 3=Winter</param>
        /// <param name="scale">Scale multiplier</param>
        public static void DrawSeasonalProjectile(SpriteBatch spriteBatch, Projectile projectile, int seasonIndex, float scale = 1f)
        {
            switch (seasonIndex)
            {
                case 0:
                    DrawSpringProjectile(spriteBatch, projectile, scale);
                    break;
                case 1:
                    DrawSummerProjectile(spriteBatch, projectile, scale);
                    break;
                case 2:
                    DrawAutumnProjectile(spriteBatch, projectile, scale);
                    break;
                default:
                    DrawWinterProjectile(spriteBatch, projectile, scale);
                    break;
            }
        }
        
        #region Ode to Joy Theme
        
        /// <summary>
        /// Ode to Joy theme projectile - Triumphant blossoming nature and joyous celebration.
        /// Colors: Verdant Green → Rose Pink → Golden Pollen → White Bloom
        /// </summary>
        public static void DrawOdeToJoyProjectile(SpriteBatch spriteBatch, Projectile projectile, float scale = 1f)
        {
            // Ode to Joy theme colors
            Color verdantGreen = new Color(76, 175, 80);
            Color rosePink = new Color(255, 182, 193);
            Color goldenPollen = new Color(255, 215, 0);
            Color whiteBloom = new Color(255, 255, 255);
            
            DrawProceduralProjectile(spriteBatch, projectile, verdantGreen, rosePink, whiteBloom, scale, 6);
        }
        
        /// <summary>
        /// Ode to Joy petal projectile - Emphasis on rose/pink tones.
        /// </summary>
        public static void DrawOdeToJoyPetalProjectile(SpriteBatch spriteBatch, Projectile projectile, float scale = 1f)
        {
            Color rosePink = new Color(255, 182, 193);
            Color petalPink = new Color(255, 105, 180);
            Color whiteBloom = new Color(255, 255, 255);
            
            DrawProceduralProjectile(spriteBatch, projectile, rosePink, petalPink, whiteBloom, scale, 8);
        }
        
        /// <summary>
        /// Ode to Joy golden projectile - Emphasis on golden/radiant tones.
        /// </summary>
        public static void DrawOdeToJoyGoldenProjectile(SpriteBatch spriteBatch, Projectile projectile, float scale = 1f)
        {
            Color goldenPollen = new Color(255, 215, 0);
            Color sunlightYellow = new Color(255, 250, 205);
            Color whiteBloom = new Color(255, 255, 255);
            
            DrawProceduralProjectile(spriteBatch, projectile, goldenPollen, sunlightYellow, whiteBloom, scale, 6);
        }
        
        #endregion
        
        /// <summary>
        /// Cleanup on mod unload
        /// </summary>
        public static void Dispose()
        {
            _pixel?.Dispose();
            _pixel = null;
        }
    }
}
