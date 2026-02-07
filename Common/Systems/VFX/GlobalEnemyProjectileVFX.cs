using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// GLOBAL ENEMY PROJECTILE VFX
    /// 
    /// Automatically applies unique visual effects to ALL enemy projectiles:
    /// 
    /// 1. THEME DETECTION - Detects which theme the projectile belongs to
    /// 2. MULTI-LAYER TRAILS - Dense dust trails with theme colors
    /// 3. ORBITING PARTICLES - Music notes, glyphs, sparks orbit projectiles  
    /// 4. BLOOM CORES - Pulsing additive glow cores
    /// 5. DEATH GLIMMERS - Spectacular death effects instead of puffs
    /// 6. INTERPOLATED RENDERING - Smooth 144Hz+ visuals
    /// 
    /// Boss projectiles get ENHANCED effects with more particles and bloom.
    /// </summary>
    public class GlobalEnemyProjectileVFX : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        
        // State tracking
        private bool _initialized = false;
        private string _detectedTheme = "";
        private bool _isBossProjectile = false;
        private Vector2 _previousPosition;
        private float[] _trailRotations = new float[12];
        private Vector2[] _trailPositions = new Vector2[12];
        private int _trailIndex = 0;
        private float _orbitAngle = 0f;
        
        // Effect settings
        private Color _primaryColor;
        private Color _secondaryColor;
        private float _pulseTimer = 0f;
        
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            // Only apply to hostile projectiles from MagnumOpus
            if (entity.ModProjectile?.Mod != ModContent.GetInstance<MagnumOpus>())
                return false;
            
            return entity.hostile;
        }
        
        public override void OnSpawn(Projectile projectile, Terraria.DataStructures.IEntitySource source)
        {
            Initialize(projectile);
        }
        
        private void Initialize(Projectile projectile)
        {
            if (_initialized) return;
            
            _detectedTheme = DetectTheme(projectile);
            _isBossProjectile = IsBossProjectile(projectile);
            _previousPosition = projectile.Center;
            
            // Initialize trail
            for (int i = 0; i < _trailPositions.Length; i++)
            {
                _trailPositions[i] = projectile.Center;
                _trailRotations[i] = projectile.rotation;
            }
            
            // Get theme colors
            var style = UniqueWeaponVFXStyles.GetStyle(_detectedTheme);
            _primaryColor = style.Fog.PrimaryColor;
            _secondaryColor = style.Fog.SecondaryColor;
            
            // Spawn entrance effect
            SpawnEntranceEffect(projectile);
            
            _initialized = true;
        }
        
        public override void AI(Projectile projectile)
        {
            if (!_initialized)
                Initialize(projectile);
            
            // Update trail
            _trailPositions[_trailIndex] = projectile.Center;
            _trailRotations[_trailIndex] = projectile.rotation;
            _trailIndex = (_trailIndex + 1) % _trailPositions.Length;
            
            // Update orbit angle
            _orbitAngle += 0.08f;
            _pulseTimer += 0.05f;
            
            // Spawn trail particles
            SpawnTrailParticles(projectile);
            
            // Spawn orbiting elements for boss projectiles
            if (_isBossProjectile && Main.rand.NextBool(3))
            {
                SpawnOrbitingElements(projectile);
            }
            
            // Update previous position for interpolation
            _previousPosition = projectile.Center;
            
            // Dynamic lighting
            float intensity = _isBossProjectile ? 0.8f : 0.4f;
            Lighting.AddLight(projectile.Center, _primaryColor.ToVector3() * intensity);
        }
        
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            if (string.IsNullOrEmpty(_detectedTheme))
                return true;
            
            SpriteBatch spriteBatch = Main.spriteBatch;
            
            // Draw trail
            DrawTrail(projectile, spriteBatch);
            
            // Draw bloom core behind projectile
            DrawBloomCore(projectile, spriteBatch);
            
            return true;
        }
        
        public override void PostDraw(Projectile projectile, Color lightColor)
        {
            // Additional post-draw effects can go here
        }
        
        public override void OnKill(Projectile projectile, int timeLeft)
        {
            if (string.IsNullOrEmpty(_detectedTheme))
                return;
            
            // Glimmer death effect instead of puff
            SpawnDeathEffect(projectile);
        }
        
        #region Theme Detection
        
        private string DetectTheme(Projectile projectile)
        {
            string typeName = projectile.ModProjectile?.GetType().FullName ?? "";
            string name = projectile.ModProjectile?.Name ?? "";
            
            // Check namespace/name for theme keywords
            if (typeName.Contains("Fate") || name.Contains("Fate") || name.Contains("Cosmic") || name.Contains("Star"))
                return "Fate";
            if (typeName.Contains("Eroica") || name.Contains("Eroica") || name.Contains("Valor") || name.Contains("Hero"))
                return "Eroica";
            if (typeName.Contains("SwanLake") || name.Contains("Swan") || name.Contains("Prima") || name.Contains("Feather"))
                return "SwanLake";
            if (typeName.Contains("LaCampanella") || name.Contains("Campanella") || name.Contains("Bell") || name.Contains("Flame"))
                return "LaCampanella";
            if (typeName.Contains("MoonlightSonata") || name.Contains("Moonlight") || name.Contains("Lunar"))
                return "MoonlightSonata";
            if (typeName.Contains("Enigma") || name.Contains("Enigma") || name.Contains("Void") || name.Contains("Paradox"))
                return "EnigmaVariations";
            if (typeName.Contains("DiesIrae") || name.Contains("Dies") || name.Contains("Wrath"))
                return "DiesIrae";
            if (typeName.Contains("Spring") || name.Contains("Spring") || name.Contains("Petal"))
                return "Spring";
            if (typeName.Contains("Summer") || name.Contains("Summer") || name.Contains("Heat"))
                return "Summer";
            if (typeName.Contains("Autumn") || name.Contains("Autumn") || name.Contains("Leaf"))
                return "Autumn";
            if (typeName.Contains("Winter") || name.Contains("Winter") || name.Contains("Frost"))
                return "Winter";
            
            return "Eroica";
        }
        
        private bool IsBossProjectile(Projectile projectile)
        {
            // Check if spawned by a boss NPC
            if (projectile.owner >= 0 && projectile.owner < Main.maxNPCs)
            {
                NPC owner = Main.npc[projectile.owner];
                if (owner.active && (owner.boss || owner.lifeMax > 50000))
                    return true;
            }
            
            // Check projectile damage - boss projectiles tend to hit harder
            if (projectile.damage > 80)
                return true;
            
            // Check projectile name for boss-related keywords
            string name = projectile.ModProjectile?.Name ?? "";
            return name.Contains("Boss") || name.Contains("Judgment") || name.Contains("Ultimate");
        }
        
        #endregion
        
        #region Effect Spawning
        
        private void SpawnEntranceEffect(Projectile projectile)
        {
            // Spawn flare on creation
            float scale = _isBossProjectile ? 0.6f : 0.35f;
            CustomParticles.GenericFlare(projectile.Center, _primaryColor, scale, 15);
            
            if (_isBossProjectile)
            {
                CustomParticles.HaloRing(projectile.Center, _secondaryColor, 0.3f, 12);
            }
        }
        
        private void SpawnTrailParticles(Projectile projectile)
        {
            int particleFrequency = _isBossProjectile ? 2 : 4;
            
            if (Main.rand.NextBool(particleFrequency))
            {
                // Main trail dust
                Vector2 offset = Main.rand.NextVector2Circular(4f, 4f);
                Color trailColor = Color.Lerp(_primaryColor, _secondaryColor, Main.rand.NextFloat());
                float scale = _isBossProjectile ? 1.4f : 0.9f;
                
                Dust d = Dust.NewDustPerfect(projectile.Center + offset, DustID.MagicMirror, 
                    -projectile.velocity * 0.15f, 100, trailColor, scale);
                d.noGravity = true;
                d.fadeIn = 1.2f;
                
                // Contrasting sparkle
                if (Main.rand.NextBool(_isBossProjectile ? 2 : 3))
                {
                    CustomParticles.GenericFlare(projectile.Center + offset, trailColor, 
                        _isBossProjectile ? 0.25f : 0.15f, 12);
                }
            }
            
            // Theme-specific trail elements
            SpawnThemeTrailElements(projectile);
        }
        
        private void SpawnThemeTrailElements(Projectile projectile)
        {
            if (!Main.rand.NextBool(_isBossProjectile ? 4 : 8))
                return;
            
            switch (_detectedTheme.ToLower())
            {
                case "fate":
                    // Glyphs + star sparkles
                    if (Main.rand.NextBool(3))
                        CustomParticles.Glyph(projectile.Center, _secondaryColor, 0.3f, -1);
                    CustomParticles.GenericFlare(projectile.Center, Color.White, 0.2f, 10);
                    break;
                    
                case "eroica":
                    // Golden embers
                    Dust ember = Dust.NewDustPerfect(projectile.Center, DustID.Enchanted_Gold,
                        -projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f), 0, Color.Gold, 0.8f);
                    ember.noGravity = true;
                    break;
                    
                case "swanlake":
                    // Feathers + prismatic
                    if (Main.rand.NextBool(4))
                        CustomParticles.SwanFeatherDrift(projectile.Center, Main.rand.NextBool() ? Color.White : new Color(20, 20, 30), 0.35f);
                    float hue = (Main.GameUpdateCount * 0.02f) % 1f;
                    CustomParticles.PrismaticSparkle(projectile.Center, Main.hslToRgb(hue, 1f, 0.8f), 0.2f);
                    break;
                    
                case "lacampanella":
                    // Fire + smoke
                    Dust flame = Dust.NewDustPerfect(projectile.Center, DustID.Torch,
                        -projectile.velocity * 0.1f, 0, _primaryColor, 1.2f);
                    flame.noGravity = true;
                    break;
                    
                case "moonlightsonata":
                    // Lunar mist
                    Dust lunar = Dust.NewDustPerfect(projectile.Center, DustID.PurpleTorch,
                        Main.rand.NextVector2Circular(1f, 1f), 100, _primaryColor, 0.9f);
                    lunar.noGravity = true;
                    lunar.fadeIn = 1.3f;
                    break;
                    
                case "enigmavariations":
                case "enigma":
                    // Mystery glyphs
                    if (Main.rand.NextBool(4))
                        CustomParticles.Glyph(projectile.Center, _primaryColor, 0.35f, -1);
                    break;
                    
                default:
                    CustomParticles.GenericFlare(projectile.Center, _primaryColor * 0.5f, 0.15f, 10);
                    break;
            }
        }
        
        private void SpawnOrbitingElements(Projectile projectile)
        {
            // Orbiting sparkles/glyphs for boss projectiles
            for (int i = 0; i < 3; i++)
            {
                float angle = _orbitAngle + MathHelper.TwoPi * i / 3f;
                Vector2 orbitPos = projectile.Center + angle.ToRotationVector2() * 12f;
                
                switch (_detectedTheme.ToLower())
                {
                    case "fate":
                        CustomParticles.GenericFlare(orbitPos, Color.White, 0.15f, 8);
                        break;
                    case "swanlake":
                        float hue = ((float)i / 3f + Main.GameUpdateCount * 0.01f) % 1f;
                        CustomParticles.PrismaticSparkle(orbitPos, Main.hslToRgb(hue, 1f, 0.8f), 0.15f);
                        break;
                    default:
                        CustomParticles.GenericFlare(orbitPos, _secondaryColor, 0.12f, 8);
                        break;
                }
            }
        }
        
        private void SpawnDeathEffect(Projectile projectile)
        {
            float scale = _isBossProjectile ? 1.2f : 0.6f;
            int particleCount = _isBossProjectile ? 12 : 6;
            
            // Central glimmer
            CustomParticles.GenericFlare(projectile.Center, Color.White, scale * 0.8f, 20);
            CustomParticles.GenericFlare(projectile.Center, _primaryColor, scale * 0.6f, 18);
            
            // Expanding halos
            int ringCount = _isBossProjectile ? 5 : 2;
            for (int ring = 0; ring < ringCount; ring++)
            {
                Color ringColor = Color.Lerp(_primaryColor, _secondaryColor, (float)ring / ringCount);
                float ringScale = 0.25f + ring * 0.1f;
                CustomParticles.HaloRing(projectile.Center, ringColor, ringScale, 12 + ring * 2);
            }
            
            // Radial sparkle burst
            for (int i = 0; i < particleCount; i++)
            {
                float angle = MathHelper.TwoPi * i / particleCount;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color color = Color.Lerp(_primaryColor, _secondaryColor, (float)i / particleCount);
                
                CustomParticles.GenericFlare(projectile.Center + vel * 2f, color, scale * 0.3f, 15);
            }
            
            // Dust burst
            for (int i = 0; i < particleCount; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                Dust d = Dust.NewDustPerfect(projectile.Center, DustID.MagicMirror, vel, 0, _primaryColor, 1.1f);
                d.noGravity = true;
            }
            
            // Theme-specific death extras
            SpawnThemeDeathExtras(projectile);
        }
        
        private void SpawnThemeDeathExtras(Projectile projectile)
        {
            switch (_detectedTheme.ToLower())
            {
                case "fate":
                    // Glyph burst
                    for (int i = 0; i < 4; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 4f;
                        Vector2 pos = projectile.Center + angle.ToRotationVector2() * 20f;
                        CustomParticles.Glyph(pos, _secondaryColor, 0.4f, i % 12);
                    }
                    break;
                    
                case "swanlake":
                    // Feather scatter
                    for (int i = 0; i < 4; i++)
                    {
                        CustomParticles.SwanFeatherDrift(projectile.Center + Main.rand.NextVector2Circular(15f, 15f),
                            Main.rand.NextBool() ? Color.White : new Color(20, 20, 30), 0.4f);
                    }
                    break;
                    
                case "enigmavariations":
                case "enigma":
                    // Mystery glyphs
                    for (int i = 0; i < 3; i++)
                    {
                        CustomParticles.Glyph(projectile.Center + Main.rand.NextVector2Circular(12f, 12f),
                            _primaryColor, 0.35f, -1);
                    }
                    break;
            }
        }
        
        #endregion
        
        #region Rendering
        
        private void DrawTrail(Projectile projectile, SpriteBatch spriteBatch)
        {
            Texture2D trailTex = MagnumTextureRegistry.GetBloom();
            
            // Draw fading trail points
            for (int i = 0; i < _trailPositions.Length; i++)
            {
                int actualIndex = (_trailIndex - 1 - i + _trailPositions.Length) % _trailPositions.Length;
                Vector2 pos = _trailPositions[actualIndex] - Main.screenPosition;
                
                float progress = (float)i / _trailPositions.Length;
                float alpha = (1f - progress) * 0.4f;
                float scale = (1f - progress) * (_isBossProjectile ? 0.4f : 0.25f);
                
                Color trailColor = Color.Lerp(_primaryColor, _secondaryColor, progress) * alpha;
                
                spriteBatch.Draw(trailTex, pos, null, trailColor, 0f,
                    trailTex.Size() / 2f, scale, SpriteEffects.None, 0f);
            }
        }
        
        private void DrawBloomCore(Projectile projectile, SpriteBatch spriteBatch)
        {
            Texture2D bloomTex = MagnumTextureRegistry.GetBloom();
            Vector2 drawPos = projectile.Center - Main.screenPosition;
            
            float pulse = 1f + (float)Math.Sin(_pulseTimer) * 0.15f;
            float baseScale = _isBossProjectile ? 0.5f : 0.3f;
            
            // Outer glow
            spriteBatch.Draw(bloomTex, drawPos, null, _primaryColor * 0.2f, 0f,
                bloomTex.Size() / 2f, baseScale * pulse * 1.5f, SpriteEffects.None, 0f);
            
            // Middle glow
            spriteBatch.Draw(bloomTex, drawPos, null, _secondaryColor * 0.15f, 0f,
                bloomTex.Size() / 2f, baseScale * pulse * 1.2f, SpriteEffects.None, 0f);
            
            // Core
            spriteBatch.Draw(bloomTex, drawPos, null, Color.White * 0.1f, 0f,
                bloomTex.Size() / 2f, baseScale * pulse * 0.6f, SpriteEffects.None, 0f);
        }
        
        #endregion
    }
}
