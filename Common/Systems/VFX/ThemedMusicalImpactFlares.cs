using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static MagnumOpus.Common.Systems.Particles.Particle;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// THEMED MUSICAL IMPACT FLARES
    /// 
    /// Creates per-theme visual flare/glint effects on impacts:
    /// - Each theme has a unique impact style
    /// - Incorporates music note PNGs flying outward
    /// - Multi-layer bloom rendering for professional glow
    /// - Cyan slashes/glints like the Exoblade impact reference
    /// - Integrates with ImpactLightRays for complete impact system
    /// 
    /// Call ThemedMusicalImpactFlares.SpawnImpact(position, theme, scale) on any hit.
    /// </summary>
    public static class ThemedMusicalImpactFlares
    {
        // Active impact flares
        private static List<ImpactFlare> _activeFlares = new List<ImpactFlare>();
        private const int MaxActiveFlares = 50;
        
        private class ImpactFlare
        {
            public Vector2 Position;
            public string Theme;
            public Color[] Palette;
            public float Scale;
            public int Timer;
            public int MaxLifetime;
            public float BaseRotation;
            public FlareType Type;
            public List<MusicNoteParticle> MusicNotes;
            public List<GlintSlash> Slashes;
        }
        
        private struct MusicNoteParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Rotation;
            public float RotationSpeed;
            public float Scale;
            public int Variant; // 1-6 for different note textures
            public int LifeOffset;
        }
        
        private struct GlintSlash
        {
            public Vector2 Offset;
            public float Angle;
            public float Length;
            public float MaxLength;
            public Color Color;
            public int LifeOffset;
        }
        
        public enum FlareType
        {
            Standard,       // Default burst
            Bell,           // La Campanella - bell chime rings
            Heroic,         // Eroica - sakura burst
            Graceful,       // Swan Lake - feather swirl
            Lunar,          // Moonlight - soft moon glow
            Mysterious,     // Enigma - eye glint
            Cosmic,         // Fate - star constellation
            Infernal,       // Dies Irae - flame burst
            Peaceful,       // Clair de Lune - gentle ripple
            Joyful          // Ode to Joy - radiant burst
        }
        
        #region Public API
        
        /// <summary>
        /// Spawns a themed musical impact flare at the given position.
        /// </summary>
        public static void SpawnImpact(Vector2 position, string theme, float scale = 1f, bool includeLightRays = true)
        {
            if (_activeFlares.Count >= MaxActiveFlares)
                _activeFlares.RemoveAt(0);
            
            Color[] palette = MagnumThemePalettes.GetThemePalette(theme) ?? new[] { Color.White };
            FlareType type = GetFlareType(theme);
            
            var flare = new ImpactFlare
            {
                Position = position,
                Theme = theme,
                Palette = palette,
                Scale = scale,
                Timer = 0,
                MaxLifetime = 25,
                BaseRotation = Main.rand.NextFloat(MathHelper.TwoPi),
                Type = type,
                MusicNotes = GenerateMusicNotes(position, palette, scale),
                Slashes = GenerateSlashes(position, palette, type, scale)
            };
            
            _activeFlares.Add(flare);
            
            // Also spawn impact light rays for complete effect
            if (includeLightRays)
            {
                ImpactLightRays.SpawnImpactRays(position, theme, 
                    Main.rand.Next(4, 8), scale * Main.rand.NextFloat(0.8f, 1.2f), true);
            }
            
            // Spawn immediate dust explosion for density
            SpawnDustExplosion(position, palette, type);
            
            // Add bright lighting
            Lighting.AddLight(position, palette[0].ToVector3() * 1.2f * scale);
        }
        
        /// <summary>
        /// Spawns a simple quick glint without full flare effects.
        /// </summary>
        public static void SpawnQuickGlint(Vector2 position, Color color, float scale = 1f)
        {
            CustomParticles.GenericFlare(position, Color.White, 0.6f * scale, 10);
            CustomParticles.GenericFlare(position, color, 0.4f * scale, 12);
            
            // Quick slash lines
            for (int i = 0; i < 3; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 slashEnd = position + angle.ToRotationVector2() * (15f + Main.rand.NextFloat(10f)) * scale;
                CustomParticles.GenericFlare(slashEnd, color * 0.7f, 0.25f * scale, 8);
            }
        }
        
        #endregion
        
        #region Update & Draw
        
        internal static void Update()
        {
            for (int i = _activeFlares.Count - 1; i >= 0; i--)
            {
                var flare = _activeFlares[i];
                flare.Timer++;
                
                // Update music notes
                foreach (var note in flare.MusicNotes)
                {
                    // Notes handled by draw with offsets
                }
                
                if (flare.Timer >= flare.MaxLifetime)
                {
                    _activeFlares.RemoveAt(i);
                }
            }
        }
        
        internal static void Draw(SpriteBatch spriteBatch)
        {
            if (_activeFlares.Count == 0) return;
            
            // Load flare textures
            Texture2D flareTex, slashTex;
            Texture2D[] noteTex = new Texture2D[6]; // We have 6 music note variants
            
            // Actual music note file names in Assets/Particles
            string[] noteNames = new string[]
            {
                "MusicNote",
                "CursiveMusicNote",
                "MusicNoteWithSlashes",
                "QuarterNote",
                "TallMusicNote",
                "WholeNote"
            };
            
            try
            {
                flareTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
                slashTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SwordArc1").Value;
                
                for (int n = 0; n < noteNames.Length; n++)
                {
                    try
                    {
                        noteTex[n] = ModContent.Request<Texture2D>($"MagnumOpus/Assets/Particles/{noteNames[n]}").Value;
                    }
                    catch
                    {
                        noteTex[n] = flareTex; // Fallback
                    }
                }
            }
            catch
            {
                return;
            }
            
            // Save blend state
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            foreach (var flare in _activeFlares)
            {
                float progress = (float)flare.Timer / flare.MaxLifetime;
                Vector2 drawPos = flare.Position - Main.screenPosition;
                
                // Draw central flare burst
                DrawCentralFlare(spriteBatch, flareTex, drawPos, flare, progress);
                
                // Draw glint slashes
                DrawSlashes(spriteBatch, slashTex, drawPos, flare, progress);
                
                // Draw music notes
                DrawMusicNotes(spriteBatch, noteTex, drawPos, flare, progress);
                
                // Draw theme-specific elements
                DrawThemeElements(spriteBatch, flareTex, drawPos, flare, progress);
            }
            
            // Restore blend state
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        
        #endregion
        
        #region Drawing Methods
        
        private static void DrawCentralFlare(SpriteBatch spriteBatch, Texture2D tex, Vector2 pos, 
            ImpactFlare flare, float progress)
        {
            Vector2 origin = tex.Size() / 2f;
            
            // Expansion then fade
            float expandProgress = progress < 0.3f ? progress / 0.3f : 1f;
            float fadeProgress = progress > 0.4f ? (progress - 0.4f) / 0.6f : 0f;
            float alpha = 1f - fadeProgress;
            
            float rotation = flare.BaseRotation + progress * MathHelper.Pi;
            Color primary = flare.Palette[0];
            Color secondary = flare.Palette.Length > 1 ? flare.Palette[1] : primary;
            
            // Layer 1: Large outer glow
            float outerScale = (0.5f + expandProgress * 0.8f) * flare.Scale;
            spriteBatch.Draw(tex, pos, null, primary * 0.3f * alpha, rotation,
                origin, outerScale, SpriteEffects.None, 0f);
            
            // Layer 2: Medium glow (counter-rotation)
            float midScale = (0.3f + expandProgress * 0.5f) * flare.Scale;
            spriteBatch.Draw(tex, pos, null, secondary * 0.5f * alpha, -rotation * 0.7f,
                origin, midScale, SpriteEffects.None, 0f);
            
            // Layer 3: Bright core
            float coreScale = (0.15f + expandProgress * 0.2f) * flare.Scale;
            spriteBatch.Draw(tex, pos, null, Color.White * 0.8f * alpha, rotation * 1.3f,
                origin, coreScale, SpriteEffects.None, 0f);
        }
        
        private static void DrawSlashes(SpriteBatch spriteBatch, Texture2D tex, Vector2 centerPos,
            ImpactFlare flare, float progress)
        {
            Vector2 origin = new Vector2(0, tex.Height / 2f); // Left-center origin for slash
            
            foreach (var slash in flare.Slashes)
            {
                float slashProgress = Math.Clamp((progress * flare.MaxLifetime - slash.LifeOffset) / 12f, 0f, 1f);
                if (slashProgress <= 0 || slashProgress >= 1) continue;
                
                // Slash stretches out then fades
                float stretchCurve = 1f - MathF.Pow(slashProgress * 2f - 1f, 2);
                float currentLength = slash.MaxLength * stretchCurve;
                float alpha = 1f - slashProgress;
                
                Vector2 slashPos = centerPos + slash.Offset;
                float scaleX = (currentLength / tex.Width) * flare.Scale;
                float scaleY = 0.15f * flare.Scale * stretchCurve;
                
                // Outer glow
                spriteBatch.Draw(tex, slashPos, null, slash.Color * 0.4f * alpha, slash.Angle,
                    origin, new Vector2(scaleX * 1.3f, scaleY * 2f), SpriteEffects.None, 0f);
                
                // Main slash
                spriteBatch.Draw(tex, slashPos, null, slash.Color * 0.8f * alpha, slash.Angle,
                    origin, new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
                
                // Bright edge
                spriteBatch.Draw(tex, slashPos, null, Color.White * 0.6f * alpha, slash.Angle,
                    origin, new Vector2(scaleX * 0.6f, scaleY * 0.3f), SpriteEffects.None, 0f);
            }
        }
        
        private static void DrawMusicNotes(SpriteBatch spriteBatch, Texture2D[] noteTex, Vector2 centerPos,
            ImpactFlare flare, float progress)
        {
            foreach (var note in flare.MusicNotes)
            {
                float noteProgress = Math.Clamp((progress * flare.MaxLifetime - note.LifeOffset) / 18f, 0f, 1f);
                if (noteProgress <= 0 || noteProgress >= 1) continue;
                
                Texture2D tex = noteTex[Math.Clamp(note.Variant - 1, 0, noteTex.Length - 1)];
                Vector2 origin = tex.Size() / 2f;
                
                // Notes fly outward with drift
                Vector2 currentPos = note.Position + note.Velocity * noteProgress * 25f;
                Vector2 drawNotePos = centerPos + (currentPos - flare.Position);
                
                float rotation = note.Rotation + note.RotationSpeed * noteProgress * MathHelper.TwoPi;
                float alpha = 1f - noteProgress * 0.8f;
                float scale = note.Scale * (1f + noteProgress * 0.3f);
                
                Color noteColor = flare.Palette[Main.rand.Next(flare.Palette.Length)];
                
                // Bloom layers
                spriteBatch.Draw(tex, drawNotePos, null, noteColor * 0.35f * alpha, rotation,
                    origin, scale * 1.5f, SpriteEffects.None, 0f);
                
                spriteBatch.Draw(tex, drawNotePos, null, noteColor * 0.7f * alpha, rotation,
                    origin, scale, SpriteEffects.None, 0f);
                
                spriteBatch.Draw(tex, drawNotePos, null, Color.White * 0.5f * alpha, rotation,
                    origin, scale * 0.6f, SpriteEffects.None, 0f);
            }
        }
        
        private static void DrawThemeElements(SpriteBatch spriteBatch, Texture2D tex, Vector2 pos,
            ImpactFlare flare, float progress)
        {
            Vector2 origin = tex.Size() / 2f;
            float alpha = 1f - progress;
            
            switch (flare.Type)
            {
                case FlareType.Bell:
                    // Concentric rings like bell waves
                    for (int ring = 0; ring < 3; ring++)
                    {
                        float ringProgress = Math.Clamp((progress - ring * 0.1f) * 2f, 0f, 1f);
                        float ringRadius = ringProgress * 60f * flare.Scale;
                        float ringAlpha = (1f - ringProgress) * 0.4f;
                        
                        for (int i = 0; i < 8; i++)
                        {
                            float angle = MathHelper.TwoPi * i / 8f + ring * 0.3f;
                            Vector2 ringPos = pos + angle.ToRotationVector2() * ringRadius;
                            spriteBatch.Draw(tex, ringPos, null, flare.Palette[0] * ringAlpha, angle,
                                origin, 0.1f * flare.Scale, SpriteEffects.None, 0f);
                        }
                    }
                    break;
                    
                case FlareType.Cosmic:
                    // Star constellation pattern
                    for (int star = 0; star < 6; star++)
                    {
                        float starAngle = flare.BaseRotation + MathHelper.TwoPi * star / 6f;
                        float starDist = (20f + star * 8f) * progress * flare.Scale;
                        Vector2 starPos = pos + starAngle.ToRotationVector2() * starDist;
                        float starAlpha = alpha * 0.6f;
                        
                        spriteBatch.Draw(tex, starPos, null, Color.White * starAlpha, Main.GameUpdateCount * 0.1f,
                            origin, 0.15f * flare.Scale, SpriteEffects.None, 0f);
                    }
                    break;
                    
                case FlareType.Graceful:
                    // Swirling pattern
                    for (int swirl = 0; swirl < 5; swirl++)
                    {
                        float swirlAngle = flare.BaseRotation + progress * MathHelper.Pi + swirl * MathHelper.TwoPi / 5f;
                        float swirlDist = 25f * progress * flare.Scale;
                        Vector2 swirlPos = pos + swirlAngle.ToRotationVector2() * swirlDist;
                        
                        Color swirlColor = swirl % 2 == 0 ? Color.White : flare.Palette[0];
                        spriteBatch.Draw(tex, swirlPos, null, swirlColor * alpha * 0.5f, swirlAngle,
                            origin, 0.12f * flare.Scale, SpriteEffects.None, 0f);
                    }
                    break;
            }
        }
        
        #endregion
        
        #region Generation Helpers
        
        private static List<MusicNoteParticle> GenerateMusicNotes(Vector2 position, Color[] palette, float scale)
        {
            var notes = new List<MusicNoteParticle>();
            int count = Main.rand.Next(4, 8);
            
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.3f, 0.3f);
                
                notes.Add(new MusicNoteParticle
                {
                    Position = position,
                    Velocity = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f),
                    Rotation = Main.rand.NextFloat(MathHelper.TwoPi),
                    RotationSpeed = Main.rand.NextFloat(-0.5f, 0.5f),
                    Scale = Main.rand.NextFloat(0.65f, 0.9f) * scale,
                    Variant = Main.rand.Next(1, 7),
                    LifeOffset = Main.rand.Next(0, 5)
                });
            }
            
            return notes;
        }
        
        private static List<GlintSlash> GenerateSlashes(Vector2 position, Color[] palette, FlareType type, float scale)
        {
            var slashes = new List<GlintSlash>();
            
            // Number of slashes based on type
            int count = type switch
            {
                FlareType.Heroic => 6,
                FlareType.Cosmic => 8,
                FlareType.Infernal => 5,
                _ => 4
            };
            
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.2f, 0.2f);
                Color slashColor = type == FlareType.Graceful ? Color.Cyan : palette[i % palette.Length];
                
                slashes.Add(new GlintSlash
                {
                    Offset = Main.rand.NextVector2Circular(5f, 5f),
                    Angle = angle,
                    Length = 0f,
                    MaxLength = Main.rand.NextFloat(20f, 40f) * scale,
                    Color = slashColor,
                    LifeOffset = Main.rand.Next(0, 4)
                });
            }
            
            return slashes;
        }
        
        private static void SpawnDustExplosion(Vector2 position, Color[] palette, FlareType type)
        {
            int dustType = type switch
            {
                FlareType.Bell => DustID.Torch,
                FlareType.Heroic => DustID.Enchanted_Pink,
                FlareType.Graceful => DustID.MagicMirror,
                FlareType.Lunar => DustID.PurpleTorch,
                FlareType.Mysterious => DustID.GemAmethyst,
                FlareType.Cosmic => DustID.Enchanted_Gold,
                FlareType.Infernal => DustID.Torch,
                _ => DustID.MagicMirror
            };
            
            for (int i = 0; i < 12; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(6f, 6f);
                Dust dust = Dust.NewDustPerfect(position, dustType, dustVel, 0, 
                    palette[i % palette.Length], 1.4f);
                dust.noGravity = true;
                dust.fadeIn = 1.2f;
            }
        }
        
        private static FlareType GetFlareType(string theme)
        {
            return theme switch
            {
                "LaCampanella" => FlareType.Bell,
                "Eroica" => FlareType.Heroic,
                "SwanLake" => FlareType.Graceful,
                "MoonlightSonata" => FlareType.Lunar,
                "EnigmaVariations" => FlareType.Mysterious,
                "Fate" => FlareType.Cosmic,
                "DiesIrae" => FlareType.Infernal,
                "ClairDeLune" => FlareType.Peaceful,
                "OdeToJoy" => FlareType.Joyful,
                _ => FlareType.Standard
            };
        }
        
        #endregion
    }
    
    /// <summary>
    /// ModSystem to update and draw themed impact flares.
    /// </summary>
    public class ThemedMusicalImpactFlaresSystem : ModSystem
    {
        public override void PostUpdateEverything()
        {
            ThemedMusicalImpactFlares.Update();
        }
        
        public override void PostDrawTiles()
        {
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            ThemedMusicalImpactFlares.Draw(Main.spriteBatch);
            
            Main.spriteBatch.End();
        }
    }
}
