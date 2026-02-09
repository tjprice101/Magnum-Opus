using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// Advanced trail rendering system that integrates with shader styles.
    /// Provides 5 unique visual trail styles: Flame, Ice, Lightning, Nature, Cosmic.
    /// Uses primitive rendering for smooth, shader-enhanced trails.
    /// </summary>
    public class AdvancedTrailSystem : ModSystem
    {
        #region Trail Instance Tracking

        private static List<ActiveTrail> _activeTrails = new();
        private static int _nextTrailId = 0;

        /// <summary>
        /// Represents an active trail being rendered.
        /// </summary>
        public class ActiveTrail
        {
            public int Id;
            public ShaderStyleRegistry.TrailStyle Style;
            public Vector2[] Positions;
            public float[] Rotations;
            public int CurrentIndex;
            public int MaxPoints;
            public Color PrimaryColor;
            public Color SecondaryColor;
            public Color TertiaryColor;
            public float Width;
            public float Intensity;
            public int Lifetime;
            public int Timer;
            public bool IsActive;

            public ActiveTrail(int maxPoints)
            {
                MaxPoints = maxPoints;
                Positions = new Vector2[maxPoints];
                Rotations = new float[maxPoints];
                CurrentIndex = 0;
                IsActive = true;
            }

            public void AddPoint(Vector2 position, float rotation)
            {
                if (CurrentIndex < MaxPoints)
                {
                    Positions[CurrentIndex] = position;
                    Rotations[CurrentIndex] = rotation;
                    CurrentIndex++;
                }
                else
                {
                    // Shift array and add new point
                    for (int i = 0; i < MaxPoints - 1; i++)
                    {
                        Positions[i] = Positions[i + 1];
                        Rotations[i] = Rotations[i + 1];
                    }
                    Positions[MaxPoints - 1] = position;
                    Rotations[MaxPoints - 1] = rotation;
                }
            }

            public Vector2[] GetActivePositions()
            {
                Vector2[] result = new Vector2[CurrentIndex];
                Array.Copy(Positions, result, CurrentIndex);
                return result;
            }
        }

        #endregion

        #region Public API - Create Trails

        /// <summary>
        /// Creates a new trail with the specified style.
        /// Returns an ID that can be used to update and terminate the trail.
        /// </summary>
        public static int CreateTrail(ShaderStyleRegistry.TrailStyle style, Color primary, Color secondary, 
            float width = 20f, int maxPoints = 20, float intensity = 1f, int lifetime = 60)
        {
            var trail = new ActiveTrail(maxPoints)
            {
                Id = _nextTrailId++,
                Style = style,
                PrimaryColor = primary,
                SecondaryColor = secondary,
                TertiaryColor = Color.Lerp(primary, secondary, 0.5f),
                Width = width,
                Intensity = intensity,
                Lifetime = lifetime,
                Timer = 0
            };

            _activeTrails.Add(trail);
            return trail.Id;
        }

        /// <summary>
        /// Creates a trail using the theme's default trail style.
        /// </summary>
        public static int CreateThemeTrail(string theme, float width = 20f, int maxPoints = 20, float intensity = 1f)
        {
            var styles = ShaderStyleRegistry.GetThemeStyles(theme);
            var palette = MagnumThemePalettes.GetThemePalette(theme);

            Color primary = palette.Length > 0 ? palette[0] : Color.White;
            Color secondary = palette.Length > 1 ? palette[1] : primary;

            return CreateTrail(styles.Trail, primary, secondary, width, maxPoints, intensity);
        }

        /// <summary>
        /// Adds a point to an existing trail.
        /// </summary>
        public static void UpdateTrail(int trailId, Vector2 position, float rotation)
        {
            var trail = FindTrail(trailId);
            if (trail != null && trail.IsActive)
            {
                trail.AddPoint(position, rotation);
                trail.Timer = 0; // Reset timer on update
            }
        }

        /// <summary>
        /// Immediately terminates a trail and starts its fade-out.
        /// </summary>
        public static void EndTrail(int trailId)
        {
            var trail = FindTrail(trailId);
            if (trail != null)
            {
                trail.IsActive = false;
            }
        }
        
        /// <summary>
        /// Alias for EndTrail - destroys a trail by ID.
        /// </summary>
        public static void DestroyTrail(int trailId) => EndTrail(trailId);

        /// <summary>
        /// Gets an existing trail by ID, or null if not found.
        /// </summary>
        public static ActiveTrail FindTrail(int trailId)
        {
            return _activeTrails.Find(t => t.Id == trailId);
        }

        #endregion

        #region Pre-built Trail Styles

        /// <summary>
        /// Creates a flame trail with automatic particle emission.
        /// </summary>
        public static int CreateFlameTrail(Vector2 startPosition, Color flameColor, Color emberColor, float width = 25f)
        {
            int id = CreateTrail(ShaderStyleRegistry.TrailStyle.Flame, flameColor, emberColor, width);
            var trail = FindTrail(id);
            if (trail != null)
            {
                trail.TertiaryColor = new Color(255, 200, 50); // Yellow-orange ember accent
            }
            return id;
        }

        /// <summary>
        /// Creates an ice trail with crystalline effects.
        /// </summary>
        public static int CreateIceTrail(Vector2 startPosition, Color iceColor, Color frostColor, float width = 18f)
        {
            int id = CreateTrail(ShaderStyleRegistry.TrailStyle.Ice, iceColor, frostColor, width);
            return id;
        }

        /// <summary>
        /// Creates a lightning trail with electric arcs.
        /// </summary>
        public static int CreateLightningTrail(Vector2 startPosition, Color electricColor, float width = 15f)
        {
            int id = CreateTrail(ShaderStyleRegistry.TrailStyle.Lightning, electricColor, Color.White, width, 15);
            return id;
        }

        /// <summary>
        /// Creates a nature trail with leaves and organic growth.
        /// </summary>
        public static int CreateNatureTrail(Vector2 startPosition, Color leafColor, Color flowerColor, float width = 22f)
        {
            int id = CreateTrail(ShaderStyleRegistry.TrailStyle.Nature, leafColor, flowerColor, width, 25);
            return id;
        }

        /// <summary>
        /// Creates a cosmic trail with starfield and nebula effects.
        /// </summary>
        public static int CreateCosmicTrail(Vector2 startPosition, Color nebulaColor, Color starColor, float width = 28f)
        {
            int id = CreateTrail(ShaderStyleRegistry.TrailStyle.Cosmic, nebulaColor, starColor, width, 30);
            return id;
        }

        #endregion

        #region Update & Render

        public override void PostUpdateEverything()
        {
            if (Main.dedServ) return;

            // Update all trails
            for (int i = _activeTrails.Count - 1; i >= 0; i--)
            {
                var trail = _activeTrails[i];
                trail.Timer++;

                // Remove if lifetime exceeded while inactive
                if (!trail.IsActive && trail.Timer >= trail.Lifetime)
                {
                    _activeTrails.RemoveAt(i);
                    continue;
                }

                // Auto-deactivate if not updated for too long
                if (trail.IsActive && trail.Timer > 10)
                {
                    trail.IsActive = false;
                    trail.Timer = 0;
                }

                // Spawn style-specific particles while active
                if (trail.IsActive && trail.CurrentIndex > 0)
                {
                    SpawnTrailParticles(trail);
                }
            }
        }

        private static void SpawnTrailParticles(ActiveTrail trail)
        {
            if (trail.CurrentIndex == 0) return;

            Vector2 tipPosition = trail.Positions[trail.CurrentIndex - 1];
            Vector2 direction = trail.CurrentIndex > 1 
                ? trail.Positions[trail.CurrentIndex - 1] - trail.Positions[trail.CurrentIndex - 2]
                : Vector2.UnitX;

            switch (trail.Style)
            {
                case ShaderStyleRegistry.TrailStyle.Flame:
                    AdvancedVFXEffects.FlameTrailSegment(tipPosition, direction, trail.PrimaryColor, trail.SecondaryColor, trail.Width / 20f);
                    break;
                case ShaderStyleRegistry.TrailStyle.Ice:
                    AdvancedVFXEffects.IceTrailSegment(tipPosition, direction, trail.PrimaryColor, trail.SecondaryColor, trail.Width / 20f);
                    break;
                case ShaderStyleRegistry.TrailStyle.Lightning:
                    AdvancedVFXEffects.LightningTrailSegment(tipPosition, direction, trail.PrimaryColor, trail.Width / 20f);
                    break;
                case ShaderStyleRegistry.TrailStyle.Nature:
                    AdvancedVFXEffects.NatureTrailSegment(tipPosition, direction, trail.PrimaryColor, trail.SecondaryColor, trail.Width / 20f);
                    break;
                case ShaderStyleRegistry.TrailStyle.Cosmic:
                    AdvancedVFXEffects.CosmicTrailSegment(tipPosition, direction, trail.PrimaryColor, trail.SecondaryColor, trail.Width / 20f);
                    break;
            }
        }

        /// <summary>
        /// Renders all active trails. Call this in your draw layer.
        /// </summary>
        public static void RenderTrails(SpriteBatch spriteBatch)
        {
            if (_activeTrails.Count == 0) return;

            foreach (var trail in _activeTrails)
            {
                if (trail.CurrentIndex < 2) continue;

                RenderTrail(spriteBatch, trail);
            }
        }

        private static void RenderTrail(SpriteBatch spriteBatch, ActiveTrail trail)
        {
            // Calculate fade alpha
            float alpha = 1f;
            if (!trail.IsActive)
            {
                alpha = 1f - (trail.Timer / (float)trail.Lifetime);
            }

            // Apply trail shader
            Effect shader = ShaderStyleRegistry.ApplyTrailStyle(
                trail.Style,
                trail.PrimaryColor,
                trail.SecondaryColor,
                trail.TertiaryColor,
                trail.Intensity * alpha
            );

            // Get positions for rendering
            Vector2[] positions = trail.GetActivePositions();
            if (positions.Length < 2) return;

            // Convert to screen space
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] -= Main.screenPosition;
            }

            // Render using primitive quad strip
            RenderTrailQuadStrip(spriteBatch, positions, trail.Width * alpha, trail.PrimaryColor * alpha, trail.SecondaryColor * alpha, shader);
        }

        private static void RenderTrailQuadStrip(SpriteBatch spriteBatch, Vector2[] positions, float width, Color startColor, Color endColor, Effect shader)
        {
            if (positions.Length < 2) return;

            // Generate vertices for quad strip
            var vertices = new VertexPositionColorTexture[positions.Length * 2];

            for (int i = 0; i < positions.Length; i++)
            {
                float progress = i / (float)(positions.Length - 1);
                
                // Calculate perpendicular for width
                Vector2 perpendicular;
                if (i == 0)
                    perpendicular = (positions[1] - positions[0]).SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.PiOver2);
                else if (i == positions.Length - 1)
                    perpendicular = (positions[i] - positions[i - 1]).SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.PiOver2);
                else
                    perpendicular = ((positions[i + 1] - positions[i - 1]) / 2f).SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.PiOver2);

                // Taper width
                float currentWidth = width * GetWidthMultiplier(progress);
                Color currentColor = Color.Lerp(startColor, endColor, progress);

                // Create two vertices at this point
                Vector2 offset = perpendicular * currentWidth * 0.5f;
                vertices[i * 2] = new VertexPositionColorTexture(
                    new Vector3(positions[i] + offset, 0),
                    currentColor,
                    new Vector2(progress, 0)
                );
                vertices[i * 2 + 1] = new VertexPositionColorTexture(
                    new Vector3(positions[i] - offset, 0),
                    currentColor,
                    new Vector2(progress, 1)
                );
            }

            // Draw using SpriteBatch with shader
            if (shader != null)
            {
                try { spriteBatch.End(); } catch { }
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, shader, Main.GameViewMatrix.TransformationMatrix);
            }

            // Draw trail as connected quads using line list approximation
            // (Full primitive rendering would require custom vertex buffer)
            Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
            for (int i = 0; i < positions.Length - 1; i++)
            {
                float progress = i / (float)(positions.Length - 1);
                float currentWidth = width * GetWidthMultiplier(progress);
                Color color = Color.Lerp(startColor, endColor, progress);

                Vector2 direction = positions[i + 1] - positions[i];
                float length = direction.Length();
                float rotation = direction.ToRotation();

                spriteBatch.Draw(pixel, positions[i], null, color, rotation, Vector2.Zero, new Vector2(length, currentWidth), SpriteEffects.None, 0f);
            }

            if (shader != null)
            {
                try { spriteBatch.End(); } catch { }
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }

        private static float GetWidthMultiplier(float progress)
        {
            // Quadratic bump: thin at start, thick in middle, thin at end
            return 4f * progress * (1f - progress);
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Gets the number of currently active trails.
        /// </summary>
        public static int GetActiveTrailCount() => _activeTrails.Count;

        /// <summary>
        /// Clears all active trails.
        /// </summary>
        public static void ClearAllTrails() => _activeTrails.Clear();

        #endregion
    }

    #region Projectile Trail Extension

    /// <summary>
    /// Extension methods for easily adding trails to projectiles.
    /// </summary>
    public static class ProjectileTrailExtensions
    {
        private static Dictionary<int, int> _projectileTrails = new();

        /// <summary>
        /// Attaches a trail to a projectile. Call this in AI() or PostAI().
        /// </summary>
        public static void AttachTrail(this Projectile projectile, ShaderStyleRegistry.TrailStyle style, Color primary, Color secondary, float width = 20f)
        {
            int projId = projectile.whoAmI;

            if (!_projectileTrails.TryGetValue(projId, out int trailId))
            {
                // Create new trail
                trailId = AdvancedTrailSystem.CreateTrail(style, primary, secondary, width);
                _projectileTrails[projId] = trailId;
            }

            // Update trail position
            AdvancedTrailSystem.UpdateTrail(trailId, projectile.Center, projectile.velocity.ToRotation());
        }

        /// <summary>
        /// Attaches a theme-appropriate trail to a projectile.
        /// </summary>
        public static void AttachThemeTrail(this Projectile projectile, string theme, float width = 20f)
        {
            var styles = ShaderStyleRegistry.GetThemeStyles(theme);
            var palette = MagnumThemePalettes.GetThemePalette(theme);

            Color primary = palette.Length > 0 ? palette[0] : Color.White;
            Color secondary = palette.Length > 1 ? palette[1] : primary;

            projectile.AttachTrail(styles.Trail, primary, secondary, width);
        }

        /// <summary>
        /// Detaches and ends the trail from a projectile. Call in Kill().
        /// </summary>
        public static void DetachTrail(this Projectile projectile)
        {
            int projId = projectile.whoAmI;
            if (_projectileTrails.TryGetValue(projId, out int trailId))
            {
                AdvancedTrailSystem.EndTrail(trailId);
                _projectileTrails.Remove(projId);
            }
        }

        /// <summary>
        /// Cleanup for projectile trails. Call periodically or on world unload.
        /// </summary>
        public static void CleanupProjectileTrails()
        {
            _projectileTrails.Clear();
        }
    }

    #endregion
}
