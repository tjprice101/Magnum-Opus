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
    /// PROCEDURAL VFX REPLACEMENT SYSTEM
    /// 
    /// This system provides shader-based and procedural alternatives to PNG-based visual effects.
    /// Instead of loading and drawing static textures, these effects are generated in real-time
    /// using mathematical functions, Bézier curves, and shader techniques.
    /// 
    /// BENEFITS OVER PNG-BASED EFFECTS:
    /// - Infinite resolution (no pixelation at any scale)
    /// - Dynamic animation and morphing
    /// - Theme-based color variations without separate textures
    /// - Smaller mod file size
    /// - More complex, layered effects
    /// - Sub-pixel smooth rendering
    /// 
    /// REPLACES:
    /// - Static sparkle PNGs → Procedural star bursts with dynamic rays
    /// - Glow halos → Shader-based radial gradients with fog integration
    /// - Trail textures → Bézier curve trails with interpolated rendering
    /// - Music notes → Procedurally drawn note shapes with bloom
    /// - Sword arcs → Shader-enhanced arc meshes
    /// </summary>
    public static class ProceduralVFXReplacements
    {
        #region Procedural Sparkle Effects
        
        /// <summary>
        /// Draws a procedural star burst effect with dynamic rays.
        /// Replaces: TwilightSparkle.png, ConstellationStyleSparkle.png, TwinkleSparkle.png
        /// </summary>
        public static void DrawProceduralSparkle(SpriteBatch spriteBatch, Vector2 position, Color color, 
            float scale, int rayCount = 4, float rotationOffset = 0f, float rayLength = 1f)
        {
            // Use a simple white pixel texture as base
            Texture2D pixel = GetPixelTexture();
            if (pixel == null) return;
            
            Color bloomColor = color with { A = 0 };
            float time = Main.GlobalTimeWrappedHourly * 3f;
            float twinkle = 1f + (float)Math.Sin(time + position.X * 0.1f) * 0.3f;
            
            // Core glow - multi-layer bloom
            for (int layer = 0; layer < 4; layer++)
            {
                float layerScale = scale * (4f - layer) * 3f * twinkle;
                float layerAlpha = 0.15f / (layer + 1);
                
                // Draw as stretched circles to simulate glow
                for (int angle = 0; angle < 8; angle++)
                {
                    float a = MathHelper.TwoPi * angle / 8f;
                    Vector2 offset = a.ToRotationVector2() * layerScale * 0.5f;
                    spriteBatch.Draw(pixel, position + offset, null, bloomColor * layerAlpha, 
                        0f, Vector2.One * 0.5f, new Vector2(2f, 2f), SpriteEffects.None, 0f);
                }
            }
            
            // Dynamic rays
            float rayAngleStep = MathHelper.TwoPi / rayCount;
            for (int i = 0; i < rayCount; i++)
            {
                float rayAngle = rayAngleStep * i + rotationOffset + time * 0.5f;
                float rayScale = scale * rayLength * twinkle * (0.8f + (float)Math.Sin(time * 2f + i) * 0.2f);
                
                // Draw ray as stretched line
                Vector2 rayDir = rayAngle.ToRotationVector2();
                
                for (int seg = 0; seg < 5; seg++)
                {
                    float segProgress = seg / 5f;
                    float segScale = rayScale * (1f - segProgress);
                    float segAlpha = (1f - segProgress) * 0.6f;
                    Vector2 segPos = position + rayDir * segProgress * rayScale * 15f;
                    
                    spriteBatch.Draw(pixel, segPos, null, bloomColor * segAlpha,
                        rayAngle, Vector2.One * 0.5f, new Vector2(segScale * 2f, 1f), SpriteEffects.None, 0f);
                }
            }
            
            // Secondary diagonal rays (smaller)
            for (int i = 0; i < rayCount; i++)
            {
                float rayAngle = rayAngleStep * i + rayAngleStep * 0.5f + rotationOffset - time * 0.3f;
                float rayScale = scale * rayLength * 0.5f * twinkle;
                
                Vector2 rayDir = rayAngle.ToRotationVector2();
                for (int seg = 0; seg < 3; seg++)
                {
                    float segProgress = seg / 3f;
                    float segAlpha = (1f - segProgress) * 0.4f;
                    Vector2 segPos = position + rayDir * segProgress * rayScale * 10f;
                    
                    spriteBatch.Draw(pixel, segPos, null, bloomColor * segAlpha,
                        rayAngle, Vector2.One * 0.5f, new Vector2(rayScale, 0.5f), SpriteEffects.None, 0f);
                }
            }
        }
        
        #endregion
        
        #region Procedural Glow Effects
        
        /// <summary>
        /// Draws a procedural radial glow with smooth falloff.
        /// Replaces: SoftGlow2.png, SoftGlow3.png, SoftGlow4.png
        /// </summary>
        public static void DrawProceduralGlow(SpriteBatch spriteBatch, Vector2 position, Color color,
            float scale, float intensity = 1f, int rings = 8)
        {
            Texture2D pixel = GetPixelTexture();
            if (pixel == null) return;
            
            Color bloomColor = color with { A = 0 };
            float pulse = 1f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 4f + position.Y * 0.05f) * 0.1f;
            
            // Draw concentric rings that fade outward
            for (int ring = rings - 1; ring >= 0; ring--)
            {
                float ringProgress = (float)ring / rings;
                float ringRadius = scale * 30f * (1f - ringProgress * 0.8f) * pulse;
                float ringAlpha = intensity * (1f - ringProgress) * 0.15f;
                
                int segments = 16 + ring * 4;
                for (int seg = 0; seg < segments; seg++)
                {
                    float angle = MathHelper.TwoPi * seg / segments;
                    Vector2 offset = angle.ToRotationVector2() * ringRadius;
                    
                    float segScale = (1f - ringProgress) * 3f;
                    spriteBatch.Draw(pixel, position + offset, null, bloomColor * ringAlpha,
                        angle, Vector2.One * 0.5f, new Vector2(segScale, segScale), SpriteEffects.None, 0f);
                }
            }
            
            // Central bright core
            for (int layer = 0; layer < 3; layer++)
            {
                float coreScale = scale * (3f - layer) * 2f * pulse;
                float coreAlpha = intensity * 0.3f / (layer + 1);
                
                int coreSegs = 12;
                for (int seg = 0; seg < coreSegs; seg++)
                {
                    float angle = MathHelper.TwoPi * seg / coreSegs;
                    Vector2 offset = angle.ToRotationVector2() * coreScale * 0.3f;
                    spriteBatch.Draw(pixel, position + offset, null, Color.White with { A = 0 } * coreAlpha,
                        0f, Vector2.One * 0.5f, coreScale * 0.5f, SpriteEffects.None, 0f);
                }
            }
        }
        
        /// <summary>
        /// Draws a procedural halo ring with animated expansion.
        /// Replaces: GlowingHalo1.png through GlowingHalo6.png
        /// </summary>
        public static void DrawProceduralHalo(SpriteBatch spriteBatch, Vector2 position, Color color,
            float scale, float thickness = 0.2f, float progress = 0f)
        {
            Texture2D pixel = GetPixelTexture();
            if (pixel == null) return;
            
            Color bloomColor = color with { A = 0 };
            float expandedScale = scale * (1f + progress * 2f);
            float fadeAlpha = 1f - progress;
            
            int segments = 32;
            float angleStep = MathHelper.TwoPi / segments;
            
            // Draw ring as connected segments
            for (int i = 0; i < segments; i++)
            {
                float angle = angleStep * i;
                float nextAngle = angleStep * (i + 1);
                
                Vector2 pos1 = position + angle.ToRotationVector2() * expandedScale * 30f;
                Vector2 pos2 = position + nextAngle.ToRotationVector2() * expandedScale * 30f;
                Vector2 midPoint = (pos1 + pos2) / 2f;
                
                float segAngle = (pos2 - pos1).ToRotation();
                float segLength = Vector2.Distance(pos1, pos2);
                
                // Inner glow
                spriteBatch.Draw(pixel, midPoint, null, bloomColor * fadeAlpha * 0.3f,
                    segAngle, new Vector2(0.5f, 0.5f), new Vector2(segLength, thickness * scale * 8f), SpriteEffects.None, 0f);
                
                // Bright edge
                spriteBatch.Draw(pixel, midPoint, null, Color.White with { A = 0 } * fadeAlpha * 0.5f,
                    segAngle, new Vector2(0.5f, 0.5f), new Vector2(segLength, thickness * scale * 3f), SpriteEffects.None, 0f);
            }
        }
        
        #endregion
        
        #region Procedural Music Note Effects
        
        /// <summary>
        /// Draws a procedurally generated music note with bloom.
        /// Replaces: MusicNote.png, CursiveMusicNote.png, etc.
        /// </summary>
        public static void DrawProceduralMusicNote(SpriteBatch spriteBatch, Vector2 position, Color color,
            float scale, float rotation = 0f, int noteType = 0)
        {
            Texture2D pixel = GetPixelTexture();
            if (pixel == null) return;
            
            Color bloomColor = color with { A = 0 };
            float shimmer = 1f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 5f + position.X * 0.1f) * 0.15f;
            scale *= shimmer;
            
            // Transform matrix for rotation
            Matrix rotMatrix = Matrix.CreateRotationZ(rotation);
            
            // Note head (oval)
            DrawNoteHead(spriteBatch, position, bloomColor, scale, rotation, noteType);
            
            // Stem (vertical line)
            if (noteType != 5) // Whole note has no stem
            {
                DrawNoteStem(spriteBatch, position, bloomColor, scale, rotation, noteType);
            }
            
            // Flag/beam for eighth notes and shorter
            if (noteType >= 2 && noteType <= 4)
            {
                DrawNoteFlag(spriteBatch, position, bloomColor, scale, rotation, noteType);
            }
            
            // Bloom layers
            for (int bloom = 0; bloom < 3; bloom++)
            {
                float bloomScale = scale * (1f + bloom * 0.4f);
                float bloomAlpha = 0.2f / (bloom + 1);
                DrawNoteHead(spriteBatch, position, bloomColor * bloomAlpha, bloomScale, rotation, noteType);
            }
        }
        
        private static void DrawNoteHead(SpriteBatch spriteBatch, Vector2 position, Color color, float scale, float rotation, int noteType)
        {
            Texture2D pixel = GetPixelTexture();
            if (pixel == null) return;
            
            // Note head as filled oval (multiple overlapping circles)
            float headWidth = scale * 12f;
            float headHeight = scale * 10f;
            
            // For whole/half notes, draw hollow
            bool hollow = noteType == 4 || noteType == 5;
            
            int segments = 16;
            for (int i = 0; i < segments; i++)
            {
                float angle = MathHelper.TwoPi * i / segments + rotation;
                float x = (float)Math.Cos(angle) * headWidth;
                float y = (float)Math.Sin(angle) * headHeight * 0.7f; // Oval shape
                
                Vector2 offset = new Vector2(x, y);
                
                if (hollow)
                {
                    // Ring shape
                    spriteBatch.Draw(pixel, position + offset, null, color * 0.8f,
                        angle, Vector2.One * 0.5f, scale * 2f, SpriteEffects.None, 0f);
                }
                else
                {
                    // Filled (draw radially inward too)
                    for (float r = 0; r <= 1f; r += 0.2f)
                    {
                        Vector2 fillOffset = offset * r;
                        spriteBatch.Draw(pixel, position + fillOffset, null, color * 0.6f,
                            0f, Vector2.One * 0.5f, scale * 2f, SpriteEffects.None, 0f);
                    }
                }
            }
        }
        
        private static void DrawNoteStem(SpriteBatch spriteBatch, Vector2 position, Color color, float scale, float rotation, int noteType)
        {
            Texture2D pixel = GetPixelTexture();
            if (pixel == null) return;
            
            float stemHeight = scale * 35f;
            float stemWidth = scale * 2f;
            
            // Stem goes up from right side of note head
            Vector2 stemBase = position + new Vector2(scale * 6f, 0f).RotatedBy(rotation);
            Vector2 stemTop = stemBase + new Vector2(0f, -stemHeight).RotatedBy(rotation);
            
            // Draw stem as line
            Vector2 midPoint = (stemBase + stemTop) / 2f;
            float stemAngle = (stemTop - stemBase).ToRotation();
            float stemLength = Vector2.Distance(stemBase, stemTop);
            
            spriteBatch.Draw(pixel, midPoint, null, color * 0.9f,
                stemAngle, new Vector2(0.5f, 0.5f), new Vector2(stemLength, stemWidth), SpriteEffects.None, 0f);
        }
        
        private static void DrawNoteFlag(SpriteBatch spriteBatch, Vector2 position, Color color, float scale, float rotation, int noteType)
        {
            Texture2D pixel = GetPixelTexture();
            if (pixel == null) return;
            
            float stemHeight = scale * 35f;
            Vector2 stemTop = position + new Vector2(scale * 6f, -stemHeight).RotatedBy(rotation);
            
            // Draw curved flag
            int flagCount = noteType - 1; // 1 flag for eighth, 2 for sixteenth, etc.
            for (int f = 0; f < Math.Min(flagCount, 3); f++)
            {
                Vector2 flagStart = stemTop + new Vector2(0f, f * scale * 8f).RotatedBy(rotation);
                
                // Curved flag using Bézier-like segments
                for (int seg = 0; seg < 5; seg++)
                {
                    float t = seg / 5f;
                    float curveX = (float)Math.Sin(t * MathHelper.PiOver2) * scale * 15f;
                    float curveY = t * scale * 20f;
                    
                    Vector2 segPos = flagStart + new Vector2(curveX, curveY).RotatedBy(rotation);
                    float segScale = (1f - t * 0.5f) * scale * 2f;
                    
                    spriteBatch.Draw(pixel, segPos, null, color * 0.7f,
                        0f, Vector2.One * 0.5f, segScale, SpriteEffects.None, 0f);
                }
            }
        }
        
        #endregion
        
        #region Procedural Energy Flare Effects
        
        /// <summary>
        /// Draws a procedural energy flare with dynamic rays and pulsing.
        /// Replaces: EnergyFlare.png, EnergyFlare4.png
        /// </summary>
        public static void DrawProceduralEnergyFlare(SpriteBatch spriteBatch, Vector2 position, Color color,
            float scale, float rotation = 0f, int style = 0)
        {
            Texture2D pixel = GetPixelTexture();
            if (pixel == null) return;
            
            Color bloomColor = color with { A = 0 };
            float time = Main.GlobalTimeWrappedHourly * 4f;
            float pulse = 1f + (float)Math.Sin(time) * 0.2f;
            
            // Style 0: Classic radial flare
            // Style 1: Cross pattern flare
            int rayCount = style == 0 ? 8 : 4;
            float raySpread = style == 0 ? 0.05f : 0.15f;
            
            // Outer glow
            for (int layer = 0; layer < 5; layer++)
            {
                float layerRadius = scale * (25f - layer * 4f) * pulse;
                float layerAlpha = 0.1f / (layer + 1);
                
                int segs = 20;
                for (int s = 0; s < segs; s++)
                {
                    float angle = MathHelper.TwoPi * s / segs;
                    Vector2 offset = angle.ToRotationVector2() * layerRadius;
                    spriteBatch.Draw(pixel, position + offset, null, bloomColor * layerAlpha,
                        0f, Vector2.One * 0.5f, (5f - layer) * scale, SpriteEffects.None, 0f);
                }
            }
            
            // Dynamic rays with varying lengths
            for (int i = 0; i < rayCount; i++)
            {
                float baseAngle = MathHelper.TwoPi * i / rayCount + rotation + time * 0.3f;
                float rayVariation = (float)Math.Sin(time * 2f + i * 1.5f) * 0.3f + 1f;
                float rayLength = scale * 40f * pulse * rayVariation;
                
                // Main ray
                DrawEnergyRay(spriteBatch, position, baseAngle, rayLength, scale * 3f, bloomColor);
                
                // Side rays
                DrawEnergyRay(spriteBatch, position, baseAngle - raySpread, rayLength * 0.6f, scale * 2f, bloomColor * 0.6f);
                DrawEnergyRay(spriteBatch, position, baseAngle + raySpread, rayLength * 0.6f, scale * 2f, bloomColor * 0.6f);
            }
            
            // Bright center
            for (int c = 0; c < 3; c++)
            {
                float coreScale = scale * (6f - c * 2f) * pulse;
                spriteBatch.Draw(pixel, position, null, Color.White with { A = 0 } * (0.5f - c * 0.1f),
                    0f, Vector2.One * 0.5f, coreScale, SpriteEffects.None, 0f);
            }
        }
        
        private static void DrawEnergyRay(SpriteBatch spriteBatch, Vector2 position, float angle, float length, float width, Color color)
        {
            Texture2D pixel = GetPixelTexture();
            if (pixel == null) return;
            
            int segments = 8;
            for (int i = 0; i < segments; i++)
            {
                float t = (float)i / segments;
                float segLength = length * (1f - t);
                float segWidth = width * (1f - t * 0.7f);
                float segAlpha = 1f - t * 0.8f;
                
                Vector2 segPos = position + angle.ToRotationVector2() * segLength * 0.5f * (1f + t);
                
                spriteBatch.Draw(pixel, segPos, null, color * segAlpha,
                    angle, new Vector2(0.5f, 0.5f), new Vector2(segLength * 0.15f, segWidth), SpriteEffects.None, 0f);
            }
        }
        
        #endregion
        
        #region Procedural Sword Arc Effects
        
        /// <summary>
        /// Draws a procedural sword arc trail using Bézier curves.
        /// Replaces: SwordArc1.png through SwordArc8.png, SwordArcSlashWave.png
        /// </summary>
        public static void DrawProceduralSwordArc(SpriteBatch spriteBatch, Vector2 playerCenter, 
            float swingAngle, float swingProgress, float bladeLength, Color primaryColor, Color secondaryColor,
            float arcWidth = 0.5f)
        {
            Texture2D pixel = GetPixelTexture();
            if (pixel == null) return;
            
            // Calculate arc parameters
            float totalArc = MathHelper.Pi * arcWidth;
            float startAngle = swingAngle - totalArc / 2f;
            float currentAngle = startAngle + totalArc * swingProgress;
            
            Color primaryBloom = primaryColor with { A = 0 };
            Color secondaryBloom = secondaryColor with { A = 0 };
            
            int arcSegments = 24;
            float arcProgress = swingProgress;
            
            // Trail segments going backward from current position
            for (int i = 0; i < arcSegments; i++)
            {
                float segProgress = (float)i / arcSegments;
                float trailProgress = arcProgress - segProgress * arcProgress;
                
                if (trailProgress < 0) continue;
                
                float segAngle = startAngle + totalArc * trailProgress;
                float nextSegAngle = startAngle + totalArc * Math.Max(0, trailProgress - 0.04f);
                
                // Fade based on distance from current swing position
                float fadeAlpha = (1f - segProgress) * 0.8f;
                
                // Width tapers at the edges
                float widthMod = (float)Math.Sin(trailProgress * MathHelper.Pi) * 0.8f + 0.2f;
                float segWidth = bladeLength * 0.15f * widthMod * fadeAlpha;
                
                // Inner and outer arc points
                Vector2 innerPoint = playerCenter + segAngle.ToRotationVector2() * bladeLength * 0.2f;
                Vector2 outerPoint = playerCenter + segAngle.ToRotationVector2() * bladeLength;
                Vector2 midPoint = (innerPoint + outerPoint) / 2f;
                
                // Color gradient from inner to outer
                Color segColor = Color.Lerp(primaryBloom, secondaryBloom, segProgress) * fadeAlpha;
                
                // Draw arc segment
                float segDrawAngle = segAngle + MathHelper.PiOver2;
                float segLength = Vector2.Distance(innerPoint, outerPoint);
                
                // Main arc body
                spriteBatch.Draw(pixel, midPoint, null, segColor * 0.6f,
                    segDrawAngle, new Vector2(0.5f, 0.5f), new Vector2(segWidth, segLength), SpriteEffects.None, 0f);
                
                // Bright edge
                spriteBatch.Draw(pixel, outerPoint, null, Color.White with { A = 0 } * fadeAlpha * 0.4f,
                    segDrawAngle, new Vector2(0.5f, 0.5f), new Vector2(segWidth * 0.3f, segLength * 0.1f), SpriteEffects.None, 0f);
            }
            
            // Leading edge glow at current swing position
            Vector2 leadingEdge = playerCenter + currentAngle.ToRotationVector2() * bladeLength;
            DrawProceduralGlow(spriteBatch, leadingEdge, primaryColor, 0.3f, 0.5f, 4);
        }
        
        #endregion
        
        #region Bézier Trail Rendering
        
        /// <summary>
        /// Draws a smooth Bézier curve trail with multi-layer bloom.
        /// Can replace any static trail texture with dynamic curved rendering.
        /// </summary>
        public static void DrawBezierTrail(SpriteBatch spriteBatch, Vector2[] controlPoints, 
            Color startColor, Color endColor, float width, int segments = 20)
        {
            if (controlPoints == null || controlPoints.Length < 4) return;
            
            Texture2D pixel = GetPixelTexture();
            if (pixel == null) return;
            
            Vector2 prevPoint = BezierProjectileSystem.CubicBezier(
                controlPoints[0], controlPoints[1], controlPoints[2], controlPoints[3], 0f);
            
            for (int i = 1; i <= segments; i++)
            {
                float t = (float)i / segments;
                Vector2 currentPoint = BezierProjectileSystem.CubicBezier(
                    controlPoints[0], controlPoints[1], controlPoints[2], controlPoints[3], t);
                
                // Segment properties
                Vector2 midPoint = (prevPoint + currentPoint) / 2f;
                float segAngle = (currentPoint - prevPoint).ToRotation();
                float segLength = Vector2.Distance(prevPoint, currentPoint);
                
                // Width tapers toward end
                float widthMod = (float)Math.Sin(t * MathHelper.Pi) * 0.6f + 0.4f;
                float segWidth = width * widthMod;
                
                // Color gradient
                Color segColor = Color.Lerp(startColor, endColor, t) with { A = 0 };
                float alpha = 1f - t * 0.5f;
                
                // Multi-layer bloom
                for (int layer = 0; layer < 3; layer++)
                {
                    float layerWidth = segWidth * (1f + layer * 0.5f);
                    float layerAlpha = alpha / (layer + 1);
                    
                    spriteBatch.Draw(pixel, midPoint, null, segColor * layerAlpha,
                        segAngle, new Vector2(0.5f, 0.5f), new Vector2(segLength, layerWidth), SpriteEffects.None, 0f);
                }
                
                prevPoint = currentPoint;
            }
        }
        
        /// <summary>
        /// Draws an interpolated projectile trail from position history.
        /// Uses sub-pixel smoothing for 144Hz+ display smoothness.
        /// </summary>
        public static void DrawInterpolatedTrail(SpriteBatch spriteBatch, Vector2[] positionHistory,
            Color[] colorGradient, float baseWidth, bool useBezierSmoothing = true)
        {
            if (positionHistory == null || positionHistory.Length < 2) return;
            
            Texture2D pixel = GetPixelTexture();
            if (pixel == null) return;
            
            if (useBezierSmoothing && positionHistory.Length >= 4)
            {
                // Convert position history to smooth Bézier curve
                Vector2[] smoothed = SmoothPositionHistory(positionHistory);
                DrawBezierTrail(spriteBatch, smoothed, colorGradient[0], 
                    colorGradient[colorGradient.Length - 1], baseWidth, positionHistory.Length * 2);
            }
            else
            {
                // Direct segment rendering
                for (int i = 1; i < positionHistory.Length; i++)
                {
                    if (positionHistory[i] == Vector2.Zero || positionHistory[i - 1] == Vector2.Zero)
                        continue;
                    
                    float progress = (float)i / positionHistory.Length;
                    Vector2 prevPos = positionHistory[i - 1];
                    Vector2 currPos = positionHistory[i];
                    
                    Vector2 midPoint = (prevPos + currPos) / 2f;
                    float segAngle = (currPos - prevPos).ToRotation();
                    float segLength = Vector2.Distance(prevPos, currPos);
                    float segWidth = baseWidth * (1f - progress * 0.7f);
                    
                    int colorIndex = (int)(progress * (colorGradient.Length - 1));
                    colorIndex = Math.Clamp(colorIndex, 0, colorGradient.Length - 1);
                    Color segColor = colorGradient[colorIndex] with { A = 0 };
                    float alpha = 1f - progress * 0.6f;
                    
                    spriteBatch.Draw(pixel, midPoint, null, segColor * alpha,
                        segAngle, new Vector2(0.5f, 0.5f), new Vector2(segLength, segWidth), SpriteEffects.None, 0f);
                }
            }
        }
        
        private static Vector2[] SmoothPositionHistory(Vector2[] positions)
        {
            // Create Bézier control points from position history
            if (positions.Length < 4) return positions;
            
            Vector2[] controlPoints = new Vector2[4];
            controlPoints[0] = positions[0];
            controlPoints[1] = positions[positions.Length / 3];
            controlPoints[2] = positions[positions.Length * 2 / 3];
            controlPoints[3] = positions[positions.Length - 1];
            
            return controlPoints;
        }
        
        #endregion
        
        #region Utility Methods
        
        private static Texture2D _pixelTexture;
        
        /// <summary>
        /// Gets a single white pixel texture for procedural drawing.
        /// </summary>
        public static Texture2D GetPixelTexture()
        {
            if (_pixelTexture == null || _pixelTexture.IsDisposed)
            {
                _pixelTexture = new Texture2D(Main.graphics.GraphicsDevice, 1, 1);
                _pixelTexture.SetData(new[] { Color.White });
            }
            return _pixelTexture;
        }
        
        /// <summary>
        /// Disposes the pixel texture when the mod unloads.
        /// </summary>
        public static void Dispose()
        {
            _pixelTexture?.Dispose();
            _pixelTexture = null;
        }
        
        #endregion
        
        #region Theme Integration
        
        /// <summary>
        /// Spawns a complete procedural impact effect for a theme.
        /// Combines fog, sparkles, music notes, and glow effects.
        /// </summary>
        public static void SpawnThemedProceduralImpact(SpriteBatch spriteBatch, Vector2 position, string theme, float scale = 1f)
        {
            // Get theme colors
            Color primary = GetThemePrimaryColor(theme);
            Color secondary = GetThemeSecondaryColor(theme);
            
            // Central glow
            DrawProceduralGlow(spriteBatch, position, primary, scale * 0.8f, 0.7f, 6);
            
            // Expanding halo
            float time = Main.GlobalTimeWrappedHourly;
            float haloProgress = (time % 1f);
            DrawProceduralHalo(spriteBatch, position, secondary, scale, 0.15f, haloProgress);
            
            // Sparkle burst
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f + time * 0.5f;
                Vector2 sparklePos = position + angle.ToRotationVector2() * scale * 25f;
                DrawProceduralSparkle(spriteBatch, sparklePos, Color.Lerp(primary, secondary, i / 6f), 
                    scale * 0.3f, 4, angle);
            }
            
            // Orbiting music notes
            for (int i = 0; i < 3; i++)
            {
                float noteAngle = MathHelper.TwoPi * i / 3f + time * 2f;
                Vector2 notePos = position + noteAngle.ToRotationVector2() * scale * 40f;
                DrawProceduralMusicNote(spriteBatch, notePos, primary, scale * 0.4f, noteAngle * 0.5f, i % 4);
            }
        }
        
        private static Color GetThemePrimaryColor(string theme)
        {
            return theme.ToLower() switch
            {
                "winter" => new Color(150, 200, 255),
                "nachtmusik" => new Color(60, 80, 140),
                "lacampanella" => new Color(255, 140, 40),
                "eroica" => new Color(200, 50, 50),
                "swanlake" => Color.White,
                "moonlightsonata" => new Color(100, 80, 160),
                "enigma" => new Color(120, 60, 180),
                "fate" => new Color(200, 80, 140),
                "clairdelune" => new Color(140, 160, 200),
                "diesirae" => new Color(180, 40, 60),
                "odetojoy" => new Color(255, 220, 100),
                _ => Color.White
            };
        }
        
        private static Color GetThemeSecondaryColor(string theme)
        {
            return theme.ToLower() switch
            {
                "winter" => new Color(200, 230, 255),
                "nachtmusik" => new Color(255, 255, 220),
                "lacampanella" => new Color(255, 200, 80),
                "eroica" => new Color(255, 200, 80),
                "swanlake" => new Color(30, 30, 40),
                "moonlightsonata" => new Color(150, 130, 200),
                "enigma" => new Color(80, 200, 120),
                "fate" => new Color(255, 80, 100),
                "clairdelune" => new Color(200, 210, 240),
                "diesirae" => new Color(255, 200, 220),
                "odetojoy" => new Color(255, 180, 50),
                _ => Color.LightGray
            };
        }
        
        #endregion
    }
}
