using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using MagnumOpus.Common.Systems.VFX.Core;

namespace MagnumOpus.Common.Systems.VFX.Effects
{
    /// <summary>
    /// Enhanced afterimage system inspired by VFX+ patterns.
    /// Provides ghostly trailing copies of entities with customizable fade, scale, and color.
    /// </summary>
    public class AfterimageSystem : ModSystem
    {
        private static List<Afterimage> activeAfterimages = new List<Afterimage>();
        private const int MAX_AFTERIMAGES = 200;
        
        public override void PostUpdateEverything()
        {
            // Update all active afterimages
            for (int i = activeAfterimages.Count - 1; i >= 0; i--)
            {
                activeAfterimages[i].Update();
                if (activeAfterimages[i].IsDead)
                {
                    activeAfterimages.RemoveAt(i);
                }
            }
        }
        
        /// <summary>
        /// Spawn afterimage trail for an entity.
        /// </summary>
        public static void SpawnAfterimage(
            Vector2 position,
            Texture2D texture,
            Rectangle? sourceRect,
            Color color,
            float rotation,
            Vector2 origin,
            float scale,
            SpriteEffects effects,
            AfterimageStyle style = AfterimageStyle.Standard,
            int lifetime = 20,
            float fadeRate = 0.05f)
        {
            if (activeAfterimages.Count >= MAX_AFTERIMAGES)
                return;
                
            activeAfterimages.Add(new Afterimage
            {
                Position = position,
                Texture = texture,
                SourceRect = sourceRect,
                Color = color,
                Rotation = rotation,
                Origin = origin,
                Scale = scale,
                Effects = effects,
                Style = style,
                Lifetime = lifetime,
                MaxLifetime = lifetime,
                FadeRate = fadeRate,
                Alpha = 1f
            });
        }
        
        /// <summary>
        /// Spawn chromatic aberration afterimage (RGB separation).
        /// </summary>
        public static void SpawnChromaticAfterimage(
            Vector2 position,
            Texture2D texture,
            Rectangle? sourceRect,
            float rotation,
            Vector2 origin,
            float scale,
            SpriteEffects effects,
            float separationDistance = 3f,
            int lifetime = 15)
        {
            if (activeAfterimages.Count >= MAX_AFTERIMAGES - 3)
                return;
                
            Vector2 offset = Vector2.UnitX.RotatedBy(rotation) * separationDistance;
            
            // Red channel - offset backward
            activeAfterimages.Add(new Afterimage
            {
                Position = position - offset,
                Texture = texture,
                SourceRect = sourceRect,
                Color = new Color(255, 0, 0, 0),
                Rotation = rotation,
                Origin = origin,
                Scale = scale,
                Effects = effects,
                Style = AfterimageStyle.Chromatic,
                Lifetime = lifetime,
                MaxLifetime = lifetime,
                FadeRate = 0.07f,
                Alpha = 0.6f,
                ChromaticChannel = 0
            });
            
            // Green channel - center
            activeAfterimages.Add(new Afterimage
            {
                Position = position,
                Texture = texture,
                SourceRect = sourceRect,
                Color = new Color(0, 255, 0, 0),
                Rotation = rotation,
                Origin = origin,
                Scale = scale,
                Effects = effects,
                Style = AfterimageStyle.Chromatic,
                Lifetime = lifetime,
                MaxLifetime = lifetime,
                FadeRate = 0.07f,
                Alpha = 0.6f,
                ChromaticChannel = 1
            });
            
            // Blue channel - offset forward
            activeAfterimages.Add(new Afterimage
            {
                Position = position + offset,
                Texture = texture,
                SourceRect = sourceRect,
                Color = new Color(0, 0, 255, 0),
                Rotation = rotation,
                Origin = origin,
                Scale = scale,
                Effects = effects,
                Style = AfterimageStyle.Chromatic,
                Lifetime = lifetime,
                MaxLifetime = lifetime,
                FadeRate = 0.07f,
                Alpha = 0.6f,
                ChromaticChannel = 2
            });
        }
        
        /// <summary>
        /// Spawn scaling afterimage (grows or shrinks over time).
        /// </summary>
        public static void SpawnScalingAfterimage(
            Vector2 position,
            Texture2D texture,
            Rectangle? sourceRect,
            Color color,
            float rotation,
            Vector2 origin,
            float startScale,
            float endScale,
            SpriteEffects effects,
            int lifetime = 25)
        {
            if (activeAfterimages.Count >= MAX_AFTERIMAGES)
                return;
                
            activeAfterimages.Add(new Afterimage
            {
                Position = position,
                Texture = texture,
                SourceRect = sourceRect,
                Color = color,
                Rotation = rotation,
                Origin = origin,
                Scale = startScale,
                StartScale = startScale,
                EndScale = endScale,
                Effects = effects,
                Style = AfterimageStyle.Scaling,
                Lifetime = lifetime,
                MaxLifetime = lifetime,
                FadeRate = 0.04f,
                Alpha = 1f
            });
        }
        
        /// <summary>
        /// Spawn rotating afterimage (spins as it fades).
        /// </summary>
        public static void SpawnRotatingAfterimage(
            Vector2 position,
            Texture2D texture,
            Rectangle? sourceRect,
            Color color,
            float rotation,
            Vector2 origin,
            float scale,
            float rotationSpeed,
            SpriteEffects effects,
            int lifetime = 30)
        {
            if (activeAfterimages.Count >= MAX_AFTERIMAGES)
                return;
                
            activeAfterimages.Add(new Afterimage
            {
                Position = position,
                Texture = texture,
                SourceRect = sourceRect,
                Color = color,
                Rotation = rotation,
                Origin = origin,
                Scale = scale,
                RotationSpeed = rotationSpeed,
                Effects = effects,
                Style = AfterimageStyle.Rotating,
                Lifetime = lifetime,
                MaxLifetime = lifetime,
                FadeRate = 0.035f,
                Alpha = 1f
            });
        }
        
        /// <summary>
        /// Draw all afterimages. Call in appropriate draw layer.
        /// </summary>
        public static void DrawAfterimages(SpriteBatch spriteBatch, bool useAdditive = true)
        {
            if (activeAfterimages.Count == 0)
                return;
                
            SpriteBatch sb = spriteBatch ?? Main.spriteBatch;
            
            if (useAdditive)
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            
            foreach (var afterimage in activeAfterimages)
            {
                afterimage.Draw(sb);
            }
            
            if (useAdditive)
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }
        
        /// <summary>
        /// Get number of active afterimages.
        /// </summary>
        public static int ActiveCount => activeAfterimages.Count;
        
        /// <summary>
        /// Clear all afterimages.
        /// </summary>
        public static void ClearAll()
        {
            activeAfterimages.Clear();
        }
    }
    
    /// <summary>
    /// Afterimage visual style.
    /// </summary>
    public enum AfterimageStyle
    {
        Standard,       // Simple fade out
        Chromatic,      // RGB channel separation
        Scaling,        // Grows/shrinks as it fades
        Rotating,       // Spins as it fades
        Distorted,      // Wavy distortion effect
        Ghostly         // Ethereal glow with bloom
    }
    
    /// <summary>
    /// Individual afterimage data.
    /// </summary>
    internal class Afterimage
    {
        public Vector2 Position;
        public Texture2D Texture;
        public Rectangle? SourceRect;
        public Color Color;
        public float Rotation;
        public Vector2 Origin;
        public float Scale;
        public SpriteEffects Effects;
        public AfterimageStyle Style;
        public int Lifetime;
        public int MaxLifetime;
        public float FadeRate;
        public float Alpha;
        
        // Style-specific
        public float StartScale;
        public float EndScale;
        public float RotationSpeed;
        public int ChromaticChannel;
        public Vector2 Velocity;
        
        public bool IsDead => Lifetime <= 0 || Alpha <= 0;
        
        public void Update()
        {
            Lifetime--;
            Alpha -= FadeRate;
            
            Position += Velocity;
            
            switch (Style)
            {
                case AfterimageStyle.Scaling:
                    float progress = 1f - (float)Lifetime / MaxLifetime;
                    Scale = MathHelper.Lerp(StartScale, EndScale, progress);
                    break;
                    
                case AfterimageStyle.Rotating:
                    Rotation += RotationSpeed;
                    break;
                    
                case AfterimageStyle.Distorted:
                    // Add subtle wave offset
                    float wave = MathF.Sin(Main.GlobalTimeWrappedHourly * 10f + Position.X * 0.1f) * 2f;
                    Position.Y += wave * 0.1f;
                    break;
            }
        }
        
        public void Draw(SpriteBatch spriteBatch)
        {
            if (Texture == null || Alpha <= 0)
                return;
                
            Color drawColor = Color * Alpha;
            
            // Apply style-specific modifications
            switch (Style)
            {
                case AfterimageStyle.Ghostly:
                    // Draw bloom layer first
                    Color bloomColor = drawColor with { A = 0 };
                    spriteBatch.Draw(Texture, Position - Main.screenPosition, SourceRect,
                        bloomColor * 0.3f, Rotation, Origin, Scale * 1.3f, Effects, 0f);
                    spriteBatch.Draw(Texture, Position - Main.screenPosition, SourceRect,
                        bloomColor * 0.5f, Rotation, Origin, Scale * 1.15f, Effects, 0f);
                    break;
            }
            
            // Draw main afterimage
            spriteBatch.Draw(Texture, Position - Main.screenPosition, SourceRect,
                drawColor, Rotation, Origin, Scale, Effects, 0f);
        }
    }
    
    /// <summary>
    /// Extension methods for easy afterimage spawning on projectiles.
    /// </summary>
    public static class AfterimageExtensions
    {
        /// <summary>
        /// Spawn afterimage trail for this projectile.
        /// </summary>
        public static void SpawnAfterimage(this Projectile proj, AfterimageStyle style = AfterimageStyle.Standard,
            int lifetime = 20, float opacity = 0.8f)
        {
            if (Main.netMode == NetmodeID.Server)
                return;
                
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[proj.type].Value;
            Rectangle sourceRect = texture.Frame(1, Main.projFrames[proj.type], 0, proj.frame);
            Vector2 origin = sourceRect.Size() / 2f;
            
            Color color = Lighting.GetColor(proj.Center.ToTileCoordinates()) * opacity;
            
            AfterimageSystem.SpawnAfterimage(
                proj.Center,
                texture,
                sourceRect,
                color,
                proj.rotation,
                origin,
                proj.scale,
                proj.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                style,
                lifetime
            );
        }
        
        /// <summary>
        /// Spawn chromatic aberration afterimage for this projectile.
        /// </summary>
        public static void SpawnChromaticAfterimage(this Projectile proj, float separation = 3f, int lifetime = 15)
        {
            if (Main.netMode == NetmodeID.Server)
                return;
                
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[proj.type].Value;
            Rectangle sourceRect = texture.Frame(1, Main.projFrames[proj.type], 0, proj.frame);
            Vector2 origin = sourceRect.Size() / 2f;
            
            AfterimageSystem.SpawnChromaticAfterimage(
                proj.Center,
                texture,
                sourceRect,
                proj.rotation,
                origin,
                proj.scale,
                proj.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                separation,
                lifetime
            );
        }
    }
}
