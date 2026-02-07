using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// CALAMITY-STYLE MELEE SWING PRIMITIVES
    /// 
    /// Implements Exoblade-style swing trails for melee weapons:
    /// - Tracks blade tip positions during swing
    /// - Renders as Bézier-interpolated vertex strip
    /// - Uses multi-layer bloom for glow
    /// - Theme-aware coloring
    /// 
    /// Automatically applies to MagnumOpus melee weapons based on namespace.
    /// </summary>
    public class MeleeSwingPrimitives : GlobalProjectile
    {
        // Track which projectiles should have swing trails
        private static HashSet<int> _swingProjectileTypes = new HashSet<int>();
        
        public override bool InstancePerEntity => true;
        
        // Per-projectile swing data
        private bool _hasSwingTrail;
        private Color _primaryColor;
        private Color _secondaryColor;
        private float _trailWidth;
        private string _theme;
        
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            // Apply to all melee-type projectiles from MagnumOpus
            if (entity.ModProjectile == null)
                return false;
            
            string fullName = entity.ModProjectile.GetType().FullName ?? "";
            
            // Check if it's a MagnumOpus melee projectile
            if (!fullName.StartsWith("MagnumOpus."))
                return false;
            
            // Check for melee indicators in the name or namespace
            bool isMelee = fullName.Contains("Melee") ||
                           fullName.Contains("Sword") ||
                           fullName.Contains("Blade") ||
                           fullName.Contains("Slash") ||
                           fullName.Contains("Swing") ||
                           fullName.Contains("Arc") ||
                           entity.aiStyle == ProjAIStyleID.Spear ||
                           entity.aiStyle == ProjAIStyleID.ShortSword ||
                           entity.aiStyle == ProjAIStyleID.Flail;
            
            return isMelee;
        }
        
        public override void SetDefaults(Projectile projectile)
        {
            if (projectile.ModProjectile == null)
                return;
            
            string fullName = projectile.ModProjectile.GetType().FullName ?? "";
            
            // Determine theme from namespace
            _theme = DetectTheme(fullName);
            if (string.IsNullOrEmpty(_theme))
            {
                _hasSwingTrail = false;
                return;
            }
            
            _hasSwingTrail = true;
            
            // Set colors based on theme
            var config = GetThemeConfig(_theme);
            _primaryColor = config.Primary;
            _secondaryColor = config.Secondary;
            _trailWidth = 25f;
        }
        
        public override void AI(Projectile projectile)
        {
            if (!_hasSwingTrail)
                return;
            
            // Track swing position (blade tip)
            Vector2 tipOffset = projectile.velocity.SafeNormalize(Vector2.Zero) * (projectile.width * 0.5f);
            Vector2 tipPosition = projectile.Center + tipOffset;
            
            PrimitiveTrailRenderer.TrackPosition(
                projectile.whoAmI,
                tipPosition,
                projectile.rotation,
                _primaryColor,
                _secondaryColor,
                _trailWidth
            );
            
            // Spawn particles along swing
            SpawnSwingParticles(projectile);
            
            // Dynamic lighting
            Lighting.AddLight(projectile.Center, _primaryColor.ToVector3() * 0.5f);
        }
        
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            if (!_hasSwingTrail)
                return true;
            
            SpriteBatch spriteBatch = Main.spriteBatch;
            
            // Render swing trail
            PrimitiveTrailRenderer.RenderTrailCustom(
                projectile.whoAmI,
                spriteBatch,
                progress => _trailWidth * PrimitiveTrailRenderer.QuadraticBump(1f - progress),
                progress => Color.Lerp(_primaryColor, _secondaryColor, progress) * (1f - progress * 0.6f)
            );
            
            // Render bloom at blade position
            RenderSwingBloom(projectile, spriteBatch);
            
            return true; // Still draw the projectile
        }
        
        public override void OnKill(Projectile projectile, int timeLeft)
        {
            if (_hasSwingTrail)
            {
                PrimitiveTrailRenderer.ClearTrail(projectile.whoAmI);
            }
        }
        
        #region Private Methods
        
        private void SpawnSwingParticles(Projectile projectile)
        {
            // Dense dust trail
            if (Main.rand.NextBool(2))
            {
                Vector2 dustPos = projectile.Center + Main.rand.NextVector2Circular(10f, 10f);
                Vector2 dustVel = projectile.velocity.RotatedByRandom(0.5f) * 0.3f;
                
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.MagicMirror, dustVel, 0, _primaryColor, 1.4f);
                dust.noGravity = true;
                dust.fadeIn = 1.2f;
            }
            
            // Contrasting sparkle
            if (Main.rand.NextBool(3))
            {
                Dust sparkle = Dust.NewDustPerfect(
                    projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.WhiteTorch,
                    Vector2.Zero,
                    0,
                    Color.White,
                    0.8f
                );
                sparkle.noGravity = true;
            }
        }
        
        private void RenderSwingBloom(Projectile projectile, SpriteBatch spriteBatch)
        {
            Texture2D glowTex = GetBloomTexture();
            if (glowTex == null)
                return;
            
            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Vector2 origin = glowTex.Size() * 0.5f;
            float pulse = 1f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 6f) * 0.1f;
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);
            
            // Multi-layer bloom
            for (int i = 0; i < 3; i++)
            {
                float scale = (0.4f + i * 0.15f) * pulse;
                float opacity = 0.4f / (i + 1);
                Color bloomColor = _primaryColor with { A = 0 } * opacity;
                
                spriteBatch.Draw(glowTex, drawPos, null, bloomColor, 0f, origin, scale, SpriteEffects.None, 0f);
            }
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);
        }
        
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
        
        private static (Color Primary, Color Secondary) GetThemeConfig(string theme)
        {
            return theme switch
            {
                "LaCampanella" => (new Color(255, 140, 40), new Color(30, 20, 25)),
                "Eroica" => (new Color(200, 50, 50), new Color(255, 200, 80)),
                "SwanLake" => (Color.White, new Color(30, 30, 40)),
                "MoonlightSonata" => (new Color(75, 0, 130), new Color(135, 206, 250)),
                "EnigmaVariations" => (new Color(140, 60, 200), new Color(50, 220, 100)),
                "Fate" => (new Color(180, 50, 100), new Color(255, 255, 255)),
                "DiesIrae" => (new Color(139, 0, 0), new Color(50, 0, 0)),
                "ClairDeLune" => (new Color(100, 149, 237), new Color(240, 248, 255)),
                "Nachtmusik" => (new Color(25, 25, 112), new Color(255, 215, 0)),
                "OdeToJoy" => (new Color(255, 215, 0), new Color(255, 255, 240)),
                "Spring" => (new Color(255, 182, 193), new Color(144, 238, 144)),
                "Summer" => (new Color(255, 140, 0), new Color(255, 255, 0)),
                "Autumn" => (new Color(205, 92, 0), new Color(139, 69, 19)),
                "Winter" => (new Color(135, 206, 250), new Color(240, 248, 255)),
                _ => (Color.White, Color.Gray)
            };
        }
        
        private static Texture2D _bloomTexture;
        private static Texture2D GetBloomTexture()
        {
            if (_bloomTexture != null && !_bloomTexture.IsDisposed)
                return _bloomTexture;
            
            int size = 64;
            _bloomTexture = new Texture2D(Main.graphics.GraphicsDevice, size, size);
            Color[] data = new Color[size * size];
            
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float maxDist = center.Length();
            
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center) / maxDist;
                    float alpha = 1f - dist * dist;
                    alpha = MathHelper.Clamp(alpha, 0f, 1f);
                    data[y * size + x] = Color.White * alpha;
                }
            }
            
            _bloomTexture.SetData(data);
            return _bloomTexture;
        }
        
        #endregion
    }
}
