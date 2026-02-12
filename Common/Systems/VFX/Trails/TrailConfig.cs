using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.Graphics;
using ReLogic.Content;

namespace MagnumOpus.Common.Systems.VFX.Trails
{
    /// <summary>
    /// Unified trail configuration class inspired by VFX+ BaseTrailInfo.
    /// Provides a centralized way to configure and render trails with consistent settings.
    /// </summary>
    public class TrailConfig
    {
        #region Properties
        
        /// <summary>
        /// The texture to use for the trail. Use a tileable horizontal gradient.
        /// </summary>
        public Texture2D TrailTexture { get; set; }
        
        /// <summary>
        /// Maximum number of trail points to store.
        /// </summary>
        public int TrailPointLimit { get; set; } = 30;
        
        /// <summary>
        /// Base width of the trail in pixels.
        /// </summary>
        public float TrailWidth { get; set; } = 20f;
        
        /// <summary>
        /// Maximum length of the trail before oldest points are removed.
        /// </summary>
        public float TrailMaxLength { get; set; } = 300f;
        
        /// <summary>
        /// Number of times to draw the trail (for layered effects).
        /// </summary>
        public int TimesToDraw { get; set; } = 1;
        
        /// <summary>
        /// Whether to smooth the trail using Catmull-Rom interpolation.
        /// </summary>
        public bool ShouldSmooth { get; set; } = true;
        
        /// <summary>
        /// Whether to pinch the head (newest end) of the trail to a point.
        /// </summary>
        public bool PinchHead { get; set; } = false;
        
        /// <summary>
        /// Whether to pinch the tail (oldest end) of the trail to a point.
        /// </summary>
        public bool PinchTail { get; set; } = true;
        
        /// <summary>
        /// Primary trail color.
        /// </summary>
        public Color TrailColor { get; set; } = Color.White;
        
        /// <summary>
        /// Secondary color for gradient effects.
        /// </summary>
        public Color SecondaryColor { get; set; } = Color.White;
        
        /// <summary>
        /// Whether to use the game's view matrix for correct camera positioning.
        /// </summary>
        public bool UseEffectMatrix { get; set; } = true;
        
        /// <summary>
        /// Blend state to use when drawing.
        /// </summary>
        public BlendState BlendState { get; set; } = BlendState.Additive;
        
        /// <summary>
        /// Alpha/opacity multiplier.
        /// </summary>
        public float Alpha { get; set; } = 1f;
        
        /// <summary>
        /// Width taper function. Input: 0-1 progress, Output: width multiplier.
        /// </summary>
        public Func<float, float> WidthFunction { get; set; }
        
        /// <summary>
        /// Color function. Input: 0-1 progress, Output: color at that point.
        /// </summary>
        public Func<float, Color> ColorFunction { get; set; }
        
        /// <summary>
        /// Optional effect/shader to apply.
        /// </summary>
        public Effect Shader { get; set; }
        
        /// <summary>
        /// Shader parameters to set before drawing.
        /// </summary>
        public Dictionary<string, object> ShaderParameters { get; set; } = new Dictionary<string, object>();
        
        #endregion
        
        #region Position Tracking
        
        private List<Vector2> positions = new List<Vector2>();
        private List<float> rotations = new List<float>();
        
        /// <summary>
        /// Add a new position to the trail.
        /// </summary>
        public void AddPosition(Vector2 position, float rotation = 0f)
        {
            positions.Insert(0, position);
            rotations.Insert(0, rotation);
            
            // Trim to limit
            while (positions.Count > TrailPointLimit)
            {
                positions.RemoveAt(positions.Count - 1);
                rotations.RemoveAt(rotations.Count - 1);
            }
            
            // Also trim by length
            TrimByLength();
        }
        
        /// <summary>
        /// Clear all trail positions.
        /// </summary>
        public void ClearPositions()
        {
            positions.Clear();
            rotations.Clear();
        }
        
        /// <summary>
        /// Get current trail positions.
        /// </summary>
        public List<Vector2> GetPositions() => positions;
        
        /// <summary>
        /// Get current trail rotations.
        /// </summary>
        public List<float> GetRotations() => rotations;
        
        private void TrimByLength()
        {
            if (positions.Count < 2)
                return;
                
            float totalLength = 0f;
            int lastValidIndex = positions.Count - 1;
            
            for (int i = 1; i < positions.Count; i++)
            {
                totalLength += Vector2.Distance(positions[i - 1], positions[i]);
                if (totalLength > TrailMaxLength)
                {
                    lastValidIndex = i;
                    break;
                }
            }
            
            // Remove positions beyond max length
            while (positions.Count > lastValidIndex + 1)
            {
                positions.RemoveAt(positions.Count - 1);
                rotations.RemoveAt(rotations.Count - 1);
            }
        }
        
        #endregion
        
        #region Preset Factories
        
        /// <summary>
        /// Standard projectile trail preset.
        /// </summary>
        public static TrailConfig ProjectileTrail(Color color, float width = 15f, int points = 25)
        {
            return new TrailConfig
            {
                TrailColor = color,
                TrailWidth = width,
                TrailPointLimit = points,
                TrailMaxLength = 200f,
                PinchTail = true,
                PinchHead = false,
                ShouldSmooth = true,
                BlendState = BlendState.Additive,
                WidthFunction = progress => (1f - progress) * (1f - progress), // Quadratic falloff
                ColorFunction = progress => Color.Lerp(color, color * 0.3f, progress)
            };
        }
        
        /// <summary>
        /// Melee swing trail preset.
        /// </summary>
        public static TrailConfig MeleeSwingTrail(Color color, float width = 30f)
        {
            return new TrailConfig
            {
                TrailColor = color,
                TrailWidth = width,
                TrailPointLimit = 15,
                TrailMaxLength = 150f,
                PinchTail = true,
                PinchHead = true,
                ShouldSmooth = true,
                BlendState = BlendState.Additive,
                TimesToDraw = 2,
                WidthFunction = progress => MathF.Sin(progress * MathF.PI), // Bulge in middle
                ColorFunction = progress => color * (1f - progress * 0.7f)
            };
        }
        
        /// <summary>
        /// Energy beam trail preset.
        /// </summary>
        public static TrailConfig BeamTrail(Color color, float width = 25f)
        {
            return new TrailConfig
            {
                TrailColor = color,
                TrailWidth = width,
                TrailPointLimit = 40,
                TrailMaxLength = 400f,
                PinchTail = false,
                PinchHead = false,
                ShouldSmooth = false, // Sharp for beams
                BlendState = BlendState.Additive,
                TimesToDraw = 3,
                Alpha = 0.8f,
                WidthFunction = progress => 1f, // Constant width
                ColorFunction = progress =>
                {
                    // Pulsing color
                    float pulse = MathF.Sin(progress * 10f + (float)Main.timeForVisualEffects * 0.1f) * 0.2f + 0.8f;
                    return color * pulse;
                }
            };
        }
        
        /// <summary>
        /// Lightning/electric trail preset.
        /// </summary>
        public static TrailConfig LightningTrail(Color color, float width = 12f)
        {
            return new TrailConfig
            {
                TrailColor = color,
                TrailWidth = width,
                TrailPointLimit = 20,
                TrailMaxLength = 200f,
                PinchTail = true,
                PinchHead = false,
                ShouldSmooth = false, // Jagged for lightning
                BlendState = BlendState.Additive,
                TimesToDraw = 2,
                WidthFunction = progress =>
                {
                    // Jagged width variation
                    float jag = MathF.Sin(progress * 30f) * 0.3f + 0.7f;
                    return (1f - progress) * jag;
                },
                ColorFunction = progress =>
                {
                    // Flash brightness variation
                    float flash = Main.rand.NextFloat(0.7f, 1f);
                    return color * flash * (1f - progress * 0.5f);
                }
            };
        }
        
        /// <summary>
        /// Fire/flame trail preset.
        /// </summary>
        public static TrailConfig FireTrail(Color hotColor, Color coolColor, float width = 20f)
        {
            return new TrailConfig
            {
                TrailColor = hotColor,
                SecondaryColor = coolColor,
                TrailWidth = width,
                TrailPointLimit = 30,
                TrailMaxLength = 180f,
                PinchTail = true,
                PinchHead = false,
                ShouldSmooth = true,
                BlendState = BlendState.Additive,
                TimesToDraw = 2,
                WidthFunction = progress => (1f - progress * 0.8f), // Slight taper
                ColorFunction = progress => Color.Lerp(hotColor, coolColor, progress)
            };
        }
        
        /// <summary>
        /// Ghostly/ethereal trail preset.
        /// </summary>
        public static TrailConfig GhostlyTrail(Color color, float width = 18f)
        {
            return new TrailConfig
            {
                TrailColor = color,
                TrailWidth = width,
                TrailPointLimit = 35,
                TrailMaxLength = 250f,
                PinchTail = true,
                PinchHead = false,
                ShouldSmooth = true,
                BlendState = BlendState.Additive,
                TimesToDraw = 3,
                Alpha = 0.6f,
                WidthFunction = progress =>
                {
                    // Wavering width
                    float wave = MathF.Sin(progress * 5f + (float)Main.timeForVisualEffects * 0.05f) * 0.15f + 0.85f;
                    return (1f - progress * 0.6f) * wave;
                },
                ColorFunction = progress =>
                {
                    float fade = 1f - MathF.Pow(progress, 1.5f);
                    return color * fade;
                }
            };
        }
        
        #endregion
        
        #region Theme Presets
        
        /// <summary>
        /// La Campanella (fire/infernal) trail preset.
        /// </summary>
        public static TrailConfig LaCampanellaTrail(float width = 22f)
        {
            Color orange = new Color(255, 140, 40);
            Color black = new Color(30, 20, 25);
            
            return new TrailConfig
            {
                TrailColor = orange,
                SecondaryColor = black,
                TrailWidth = width,
                TrailPointLimit = 28,
                TrailMaxLength = 200f,
                PinchTail = true,
                ShouldSmooth = true,
                BlendState = BlendState.Additive,
                TimesToDraw = 2,
                WidthFunction = progress => (1f - progress * 0.7f),
                ColorFunction = progress =>
                {
                    float flicker = Main.rand.NextFloat(0.85f, 1f);
                    return Color.Lerp(orange, black, progress * 0.6f) * flicker;
                }
            };
        }
        
        /// <summary>
        /// Eroica (heroic/scarlet-gold) trail preset.
        /// </summary>
        public static TrailConfig EroicaTrail(float width = 20f)
        {
            Color scarlet = new Color(200, 50, 50);
            Color gold = new Color(255, 200, 80);
            
            return new TrailConfig
            {
                TrailColor = scarlet,
                SecondaryColor = gold,
                TrailWidth = width,
                TrailPointLimit = 25,
                TrailMaxLength = 180f,
                PinchTail = true,
                ShouldSmooth = true,
                BlendState = BlendState.Additive,
                TimesToDraw = 2,
                WidthFunction = progress => MathF.Pow(1f - progress, 1.5f),
                ColorFunction = progress => Color.Lerp(gold, scarlet, progress)
            };
        }
        
        /// <summary>
        /// Moonlight Sonata (purple-blue lunar) trail preset.
        /// </summary>
        public static TrailConfig MoonlightSonataTrail(float width = 18f)
        {
            Color purple = new Color(140, 100, 200);
            Color blue = new Color(100, 150, 220);
            
            return new TrailConfig
            {
                TrailColor = purple,
                SecondaryColor = blue,
                TrailWidth = width,
                TrailPointLimit = 30,
                TrailMaxLength = 220f,
                PinchTail = true,
                ShouldSmooth = true,
                BlendState = BlendState.Additive,
                TimesToDraw = 2,
                Alpha = 0.9f,
                WidthFunction = progress =>
                {
                    float wave = MathF.Sin(progress * 4f + (float)Main.timeForVisualEffects * 0.03f) * 0.1f + 0.9f;
                    return (1f - progress * 0.5f) * wave;
                },
                ColorFunction = progress => Color.Lerp(blue, purple, progress)
            };
        }
        
        /// <summary>
        /// Swan Lake (monochrome rainbow) trail preset.
        /// </summary>
        public static TrailConfig SwanLakeTrail(float width = 16f)
        {
            return new TrailConfig
            {
                TrailColor = Color.White,
                TrailWidth = width,
                TrailPointLimit = 32,
                TrailMaxLength = 240f,
                PinchTail = true,
                ShouldSmooth = true,
                BlendState = BlendState.Additive,
                TimesToDraw = 3,
                WidthFunction = progress => (1f - progress * 0.6f),
                ColorFunction = progress =>
                {
                    // Rainbow shimmer
                    float hue = (progress + (float)Main.timeForVisualEffects * 0.005f) % 1f;
                    Color rainbow = Main.hslToRgb(hue, 0.8f, 0.85f);
                    float fade = 1f - progress * 0.4f;
                    return Color.Lerp(Color.White, rainbow, 0.3f) * fade;
                }
            };
        }
        
        /// <summary>
        /// Fate (cosmic dark prismatic) trail preset.
        /// </summary>
        public static TrailConfig FateTrail(float width = 24f)
        {
            Color darkPink = new Color(180, 50, 100);
            Color purple = new Color(120, 30, 140);
            Color white = Color.White;
            
            return new TrailConfig
            {
                TrailColor = darkPink,
                SecondaryColor = purple,
                TrailWidth = width,
                TrailPointLimit = 35,
                TrailMaxLength = 280f,
                PinchTail = true,
                ShouldSmooth = true,
                BlendState = BlendState.Additive,
                TimesToDraw = 3,
                WidthFunction = progress =>
                {
                    // Sharp cosmic taper with occasional star bursts
                    float base_ = 1f - MathF.Pow(progress, 1.2f);
                    float star = MathF.Sin(progress * 15f) * 0.1f;
                    return MathF.Max(0, base_ + star);
                },
                ColorFunction = progress =>
                {
                    // Cosmic gradient: white -> pink -> purple
                    if (progress < 0.3f)
                        return Color.Lerp(white, darkPink, progress / 0.3f);
                    else
                        return Color.Lerp(darkPink, purple, (progress - 0.3f) / 0.7f);
                }
            };
        }
        
        /// <summary>
        /// Enigma (void purple-green) trail preset.
        /// </summary>
        public static TrailConfig EnigmaTrail(float width = 20f)
        {
            Color purple = new Color(140, 60, 200);
            Color green = new Color(50, 180, 100);
            Color black = new Color(15, 10, 20);
            
            return new TrailConfig
            {
                TrailColor = purple,
                SecondaryColor = green,
                TrailWidth = width,
                TrailPointLimit = 28,
                TrailMaxLength = 200f,
                PinchTail = true,
                ShouldSmooth = true,
                BlendState = BlendState.Additive,
                TimesToDraw = 2,
                Alpha = 0.85f,
                WidthFunction = progress =>
                {
                    // Mysterious pulsing
                    float pulse = MathF.Sin(progress * 8f + (float)Main.timeForVisualEffects * 0.08f) * 0.2f + 0.8f;
                    return (1f - progress * 0.5f) * pulse;
                },
                ColorFunction = progress =>
                {
                    // Purple to green with black void
                    Color gradient = Color.Lerp(purple, green, progress);
                    return Color.Lerp(gradient, black, progress * 0.4f);
                }
            };
        }
        
        #endregion
        
        #region Rendering
        
        /// <summary>
        /// Draw the trail using the current configuration.
        /// Call this each frame to render the trail.
        /// </summary>
        public void DrawTrail(SpriteBatch spriteBatch = null)
        {
            if (positions.Count < 2)
                return;
                
            SpriteBatch sb = spriteBatch ?? Main.spriteBatch;
            
            // Get smoothed positions if enabled
            List<Vector2> drawPositions = ShouldSmooth ? SmoothPositions(positions) : positions;
            
            for (int pass = 0; pass < TimesToDraw; pass++)
            {
                float passScale = 1f + pass * 0.15f;
                float passAlpha = Alpha / (pass + 1);
                
                DrawTrailPass(sb, drawPositions, passScale, passAlpha);
            }
        }
        
        private void DrawTrailPass(SpriteBatch sb, List<Vector2> drawPositions, float scaleMultiplier, float alphaMult)
        {
            Texture2D tex = TrailTexture ?? GetDefaultTexture();
            if (tex == null)
                return;
                
            // Switch blend state
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, Shader,
                UseEffectMatrix ? Main.GameViewMatrix.TransformationMatrix : Matrix.Identity);
            
            // Apply shader parameters
            if (Shader != null)
            {
                foreach (var kvp in ShaderParameters)
                {
                    var param = Shader.Parameters[kvp.Key];
                    if (param != null)
                    {
                        if (kvp.Value is float f)
                            param.SetValue(f);
                        else if (kvp.Value is Vector2 v2)
                            param.SetValue(v2);
                        else if (kvp.Value is Vector3 v3)
                            param.SetValue(v3);
                        else if (kvp.Value is Vector4 v4)
                            param.SetValue(v4);
                        else if (kvp.Value is Color c)
                            param.SetValue(c.ToVector4());
                        else if (kvp.Value is Matrix m)
                            param.SetValue(m);
                        else if (kvp.Value is Texture2D t)
                            param.SetValue(t);
                    }
                }
            }
            
            Vector2 texSize = tex.Size();
            
            for (int i = 0; i < drawPositions.Count - 1; i++)
            {
                float progress = (float)i / (drawPositions.Count - 1);
                float nextProgress = (float)(i + 1) / (drawPositions.Count - 1);
                
                Vector2 pos1 = drawPositions[i];
                Vector2 pos2 = drawPositions[i + 1];
                Vector2 dir = pos2 - pos1;
                float length = dir.Length();
                
                if (length < 0.1f)
                    continue;
                    
                float rotation = dir.ToRotation();
                
                // Calculate width at this point
                float widthMult = WidthFunction?.Invoke(progress) ?? (1f - progress);
                
                // Apply pinching
                if (PinchHead && i == 0)
                    widthMult *= 0.1f;
                if (PinchTail && i == drawPositions.Count - 2)
                    widthMult *= 0.1f;
                    
                float width = TrailWidth * widthMult * scaleMultiplier;
                
                // Get color at this point
                Color color = ColorFunction?.Invoke(progress) ?? Color.Lerp(TrailColor, SecondaryColor, progress);
                color = color with { A = 0 } * alphaMult;
                
                // Draw segment
                Vector2 drawPos = pos1 - Main.screenPosition;
                Vector2 scale = new Vector2(length / texSize.X, width / texSize.Y);
                
                sb.Draw(tex, drawPos, null, color, rotation, new Vector2(0, texSize.Y / 2f), scale, SpriteEffects.None, 0f);
            }
            
            // Restore blend state
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        
        private List<Vector2> SmoothPositions(List<Vector2> input)
        {
            if (input.Count < 4)
                return input;
                
            List<Vector2> smoothed = new List<Vector2>();
            int resolution = input.Count * 2;
            
            for (int i = 0; i < resolution; i++)
            {
                float t = (float)i / (resolution - 1) * (input.Count - 1);
                int segment = (int)t;
                float localT = t - segment;
                
                int p0 = Math.Max(0, segment - 1);
                int p1 = segment;
                int p2 = Math.Min(input.Count - 1, segment + 1);
                int p3 = Math.Min(input.Count - 1, segment + 2);
                
                smoothed.Add(CatmullRom(input[p0], input[p1], input[p2], input[p3], localT));
            }
            
            return smoothed;
        }
        
        private Vector2 CatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            
            return 0.5f * (
                2f * p1 +
                (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                (-p0 + 3f * p1 - 3f * p2 + p3) * t3
            );
        }
        
        private static Texture2D GetDefaultTexture()
        {
            try
            {
                return ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX/Trails/DefaultTrail", AssetRequestMode.ImmediateLoad).Value;
            }
            catch
            {
                // Fallback to vanilla texture
                return Terraria.GameContent.TextureAssets.MagicPixel.Value;
            }
        }
        
        #endregion
    }
}
