using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;
using static MagnumOpus.Common.Systems.Particles.Particle;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// EXO BLADE-STYLE BEAM PROJECTILE TRAILS
    /// 
    /// Creates beam-like projectile trails similar to the Exoblade's golden beam projectiles:
    /// - Elongated glowing beam shape
    /// - Multi-pass bloom rendering (outer glow + main beam + bright core)
    /// - Theme-based color gradients
    /// - Pulsing/breathing glow intensity
    /// - Catmull-Rom smoothed trails for fluid motion
    /// - Music note particles scattered in wake
    /// 
    /// Automatically applies to MagnumOpus projectiles that should look like beams.
    /// </summary>
    public class BeamProjectileTrails : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        
        // Per-projectile data
        private bool _isBeamProjectile;
        private string _theme;
        private Color[] _palette;
        private float _beamWidth;
        private bool _useRainbow;
        
        // Position history for trail
        private List<BeamPoint> _trailPoints = new List<BeamPoint>();
        private const int MaxTrailPoints = 20;
        private const int MinPointsForTrail = 3;
        
        private struct BeamPoint
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public uint TimeStamp;
        }
        
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            // MASTER TOGGLE: When disabled, this global system does nothing
            if (!VFXMasterToggle.GlobalSystemsEnabled)
                return false;
            
            if (entity.ModProjectile == null) return false;
            
            // Exclude debug weapons
            if (VFXExclusionHelper.ShouldExcludeProjectile(entity)) return false;
            
            string fullName = entity.ModProjectile.GetType().FullName ?? "";
            if (!fullName.StartsWith("MagnumOpus.")) return false;
            
            // Apply to beam/bolt/projectile types (not melee)
            return fullName.Contains("Beam") ||
                   fullName.Contains("Bolt") ||
                   fullName.Contains("Ray") ||
                   fullName.Contains("Laser") ||
                   fullName.Contains("Wave") ||
                   fullName.Contains("Burst") ||
                   fullName.Contains("Shot") ||
                   fullName.Contains("Projectile") ||
                   (entity.aiStyle == ProjAIStyleID.Arrow && !fullName.Contains("Melee"));
        }
        
        public override void SetDefaults(Projectile projectile)
        {
            if (projectile.ModProjectile == null) return;
            
            string fullName = projectile.ModProjectile.GetType().FullName ?? "";
            _theme = DetectTheme(fullName);
            
            if (string.IsNullOrEmpty(_theme))
            {
                _isBeamProjectile = false;
                return;
            }
            
            _isBeamProjectile = true;
            _palette = MagnumThemePalettes.GetThemePalette(_theme) ?? new[] { Color.Gold };
            _beamWidth = 18f;
            _useRainbow = _theme == "SwanLake";
            
            _trailPoints.Clear();
        }
        
        public override void AI(Projectile projectile)
        {
            if (!_isBeamProjectile) return;
            
            // Track position history for beam trail
            TrackBeamPosition(projectile);
            
            // Spawn beam particles
            SpawnBeamParticles(projectile);
        }
        
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            if (!_isBeamProjectile || _trailPoints.Count < MinPointsForTrail)
                return true;
            
            DrawBeamTrail(projectile, Main.spriteBatch);
            
            // Draw beam head glow
            DrawBeamHead(projectile, Main.spriteBatch);
            
            return true;
        }
        
        public override void OnKill(Projectile projectile, int timeLeft)
        {
            if (_isBeamProjectile)
            {
                // Spawn impact light rays on beam death!
                if (_trailPoints.Count > 0)
                {
                    var lastPoint = _trailPoints[_trailPoints.Count - 1];
                    ImpactLightRays.SpawnImpactRays(lastPoint.Position, _theme, 
                        Main.rand.Next(5, 9), Main.rand.NextFloat(0.8f, 1.2f), true);
                }
                
                _trailPoints.Clear();
            }
        }
        
        #region Position Tracking
        
        private void TrackBeamPosition(Projectile projectile)
        {
            var newPoint = new BeamPoint
            {
                Position = projectile.Center,
                Velocity = projectile.velocity,
                TimeStamp = Main.GameUpdateCount
            };
            
            _trailPoints.Add(newPoint);
            
            // Remove old points
            while (_trailPoints.Count > MaxTrailPoints)
            {
                _trailPoints.RemoveAt(0);
            }
            
            // Remove stale points
            while (_trailPoints.Count > 0 && Main.GameUpdateCount - _trailPoints[0].TimeStamp > 12)
            {
                _trailPoints.RemoveAt(0);
            }
        }
        
        #endregion
        
        #region Beam Rendering
        
        private void DrawBeamTrail(Projectile projectile, SpriteBatch spriteBatch)
        {
            if (_trailPoints.Count < MinPointsForTrail) return;
            
            // Smooth the trail with Catmull-Rom
            List<Vector2> smoothedPositions = SmoothTrail(_trailPoints, 40);
            if (smoothedPositions.Count < 2) return;
            
            Vector2[] trailPoints = smoothedPositions.ToArray();
            
            // Pulsing intensity
            float pulse = 1f + MathF.Sin(Main.GameUpdateCount * 0.15f) * 0.15f;
            
            // Pass 1: Outer bloom (large, dim glow)
            EnhancedTrailRenderer.RenderTrail(trailPoints, new EnhancedTrailRenderer.PrimitiveSettings(
                GetBeamWidth(3f * pulse),
                GetBeamColor(0.25f),
                null, true, null
            ));
            
            // Pass 2: Middle glow
            EnhancedTrailRenderer.RenderTrail(trailPoints, new EnhancedTrailRenderer.PrimitiveSettings(
                GetBeamWidth(1.8f * pulse),
                GetBeamColor(0.5f),
                null, true, null
            ));
            
            // Pass 3: Main beam
            EnhancedTrailRenderer.RenderTrail(trailPoints, new EnhancedTrailRenderer.PrimitiveSettings(
                GetBeamWidth(1f),
                GetBeamColor(0.9f),
                null, true, null
            ));
            
            // Pass 4: Bright core
            EnhancedTrailRenderer.RenderTrail(trailPoints, new EnhancedTrailRenderer.PrimitiveSettings(
                GetBeamWidth(0.35f),
                _ => Color.White * 0.95f,
                null, true, null
            ));
        }
        
        private void DrawBeamHead(Projectile projectile, SpriteBatch spriteBatch)
        {
            if (_trailPoints.Count == 0) return;
            
            Vector2 headPos = projectile.Center - Main.screenPosition;
            float pulse = 1f + MathF.Sin(Main.GameUpdateCount * 0.2f) * 0.2f;
            
            // Get flare texture
            Texture2D flareTex;
            try
            {
                flareTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare4").Value;
            }
            catch
            {
                return;
            }
            
            Vector2 origin = flareTex.Size() / 2f;
            float rotation = projectile.velocity.ToRotation();
            Color headColor = _palette.Length > 0 ? _palette[_palette.Length - 1] : Color.White;
            
            // Save blend state
            try { spriteBatch.End(); } catch { }
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Outer glow
            spriteBatch.Draw(flareTex, headPos, null, headColor * 0.4f, rotation,
                origin, 0.6f * pulse, SpriteEffects.None, 0f);
            
            // Main flare
            spriteBatch.Draw(flareTex, headPos, null, headColor * 0.7f, rotation + 0.5f,
                origin, 0.35f * pulse, SpriteEffects.None, 0f);
            
            // White core
            spriteBatch.Draw(flareTex, headPos, null, Color.White * 0.8f, rotation,
                origin, 0.15f * pulse, SpriteEffects.None, 0f);
            
            // Restore blend state
            try { spriteBatch.End(); } catch { }
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        
        private EnhancedTrailRenderer.WidthFunction GetBeamWidth(float multiplier)
        {
            return progress =>
            {
                // Beam tapers at the tail, full width at head
                // Uses inverse lerp bump for smooth taper
                float taper = MathF.Sqrt(1f - progress * 0.7f);
                return _beamWidth * taper * multiplier;
            };
        }
        
        private EnhancedTrailRenderer.ColorFunction GetBeamColor(float opacity)
        {
            return progress =>
            {
                Color color;
                
                if (_useRainbow)
                {
                    float hue = (progress * 0.5f + Main.GameUpdateCount * 0.01f) % 1f;
                    color = Main.hslToRgb(hue, 1f, 0.7f);
                }
                else if (_palette.Length > 1)
                {
                    // Gradient along beam (brighter at head)
                    float reversed = 1f - progress;
                    color = VFXUtilities.PaletteLerp(_palette, reversed);
                }
                else
                {
                    color = _palette[0];
                }
                
                // Fade at tail
                float fade = MathF.Pow(1f - progress, 0.5f);
                return color * opacity * fade;
            };
        }
        
        private static List<Vector2> SmoothTrail(List<BeamPoint> points, int outputCount)
        {
            if (points.Count < 2) return new List<Vector2>();
            
            List<Vector2> positions = new List<Vector2>();
            foreach (var p in points)
                positions.Add(p.Position);
            
            List<Vector2> result = new List<Vector2>();
            
            for (int i = 0; i < outputCount; i++)
            {
                float t = (float)i / (outputCount - 1) * (positions.Count - 1);
                int segment = (int)t;
                float segmentT = t - segment;
                
                int p0 = Math.Max(0, segment - 1);
                int p1 = segment;
                int p2 = Math.Min(positions.Count - 1, segment + 1);
                int p3 = Math.Min(positions.Count - 1, segment + 2);
                
                result.Add(CatmullRom(positions[p0], positions[p1], positions[p2], positions[p3], segmentT));
            }
            
            return result;
        }
        
        private static Vector2 CatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
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
        
        #endregion
        
        #region Particles
        
        private void SpawnBeamParticles(Projectile projectile)
        {
            // Dense glow dust every frame
            if (Main.rand.NextBool(2))
            {
                Color dustColor = _useRainbow ? 
                    Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.7f) : 
                    _palette[Main.rand.Next(_palette.Length)];
                
                Dust dust = Dust.NewDustPerfect(
                    projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    DustID.Enchanted_Gold,
                    -projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1f, 1f),
                    0, dustColor, 1.3f);
                dust.noGravity = true;
                dust.fadeIn = 1.1f;
            }
            
            // Contrasting sparkles
            if (Main.rand.NextBool(3))
            {
                Dust spark = Dust.NewDustPerfect(
                    projectile.Center,
                    DustID.WhiteTorch,
                    -projectile.velocity * 0.1f,
                    0, Color.White, 1.2f);
                spark.noGravity = true;
            }
            
            // Flares littering the air
            if (Main.rand.NextBool(2))
            {
                Color flareColor = _palette.Length > 0 ? _palette[Main.rand.Next(_palette.Length)] : Color.Gold;
                CustomParticles.GenericFlare(
                    projectile.Center + Main.rand.NextVector2Circular(5f, 5f),
                    flareColor, 0.35f, 12);
            }
            
            // Music notes in trail (every 5 frames)
            if (Main.GameUpdateCount % 5 == 0 && Main.rand.NextBool(3))
            {
                Color noteColor = _palette.Length > 0 ? _palette[0] : Color.Gold;
                ThemedParticles.MusicNote(
                    projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    -projectile.velocity * 0.08f,
                    noteColor, 0.75f, 28);
            }
        }
        
        #endregion
        
        #region Theme Detection
        
        private static string DetectTheme(string fullName)
        {
            if (fullName.Contains("LaCampanella")) return "LaCampanella";
            if (fullName.Contains("Eroica")) return "Eroica";
            if (fullName.Contains("SwanLake")) return "SwanLake";
            if (fullName.Contains("MoonlightSonata") || fullName.Contains("Moonlight")) return "MoonlightSonata";
            if (fullName.Contains("EnigmaVariations") || fullName.Contains("Enigma")) return "EnigmaVariations";
            if (fullName.Contains("Fate")) return "Fate";
            if (fullName.Contains("DiesIrae")) return "DiesIrae";
            if (fullName.Contains("ClairDeLune") || fullName.Contains("Clair")) return "ClairDeLune";
            if (fullName.Contains("Nachtmusik")) return "Nachtmusik";
            if (fullName.Contains("OdeToJoy")) return "OdeToJoy";
            if (fullName.Contains("Spring")) return "Spring";
            if (fullName.Contains("Summer")) return "Summer";
            if (fullName.Contains("Autumn")) return "Autumn";
            if (fullName.Contains("Winter")) return "Winter";
            return "";
        }
        
        #endregion
    }
}
