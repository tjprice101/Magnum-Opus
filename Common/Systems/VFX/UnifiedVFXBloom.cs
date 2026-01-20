using Microsoft.Xna.Framework;
using System;
using Terraria;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// Extension methods adding Fargos-style enhanced bloom effects to UnifiedVFX.
    /// These methods use the new EnhancedParticle system with multi-layer bloom stacking.
    /// 
    /// USAGE: 
    ///   UnifiedVFXBloom.LaCampanella.ImpactEnhanced(position, 1.5f);
    ///   UnifiedVFXBloom.Fate.CosmicBurst(position);
    ///   UnifiedVFXBloom.Generic.BloomImpact(position, primaryColor, secondaryColor);
    /// </summary>
    public static class UnifiedVFXBloom
    {
        // ============================================================================
        // LA CAMPANELLA ENHANCED - Infernal bloom with heavy smoke
        // ============================================================================
        public static class LaCampanella
        {
            /// <summary>
            /// Enhanced impact using Fargos-style multi-layer bloom.
            /// Includes smoke layers and gradient bloom stacking.
            /// </summary>
            public static void ImpactEnhanced(Vector2 position, float scale = 1f)
            {
                // Use the enhanced themed particles
                EnhancedThemedParticles.LaCampanellaBloomBurstEnhanced(position, scale);
                
                // Add the original UnifiedVFX effects on top
                UnifiedVFX.LaCampanella.Impact(position, scale);
            }
            
            /// <summary>
            /// Enhanced bell chime with pulsing bloom rings.
            /// </summary>
            public static void BellChimeEnhanced(Vector2 position, float scale = 1f)
            {
                EnhancedThemedParticles.BellChimeEnhanced(position, scale);
            }
            
            /// <summary>
            /// Enhanced explosion with maximum infernal spectacle.
            /// </summary>
            public static void ExplosionEnhanced(Vector2 position, float scale = 1f)
            {
                EnhancedThemedParticles.LaCampanellaBloomBurstEnhanced(position, scale * 1.5f);
                UnifiedVFX.LaCampanella.Explosion(position, scale);
            }
            
            /// <summary>
            /// Enhanced trail with bloom particles.
            /// </summary>
            public static void TrailEnhanced(Vector2 position, Vector2 velocity)
            {
                UnifiedVFX.LaCampanella.Trail(position, velocity, 1f);
                
                // Add enhanced bloom trail
                if (Main.rand.NextBool(3))
                {
                    float progress = Main.rand.NextFloat();
                    Color trailColor = Color.Lerp(new Color(20, 15, 20), new Color(255, 100, 0), progress);
                    
                    var trail = EnhancedParticlePool.GetParticle()
                        .Setup(CustomParticleSystem.RandomGlow(), position + Main.rand.NextVector2Circular(6f, 6f),
                            -velocity * 0.1f, trailColor, 0.3f, 20)
                        .WithBloom(2, 0.6f)
                        .WithDrag(0.96f);
                    EnhancedParticlePool.SpawnParticle(trail);
                }
            }
        }
        
        // ============================================================================
        // EROICA ENHANCED - Heroic bloom with sakura petals
        // ============================================================================
        public static class Eroica
        {
            /// <summary>
            /// Enhanced impact with heroic bloom stacking.
            /// </summary>
            public static void ImpactEnhanced(Vector2 position, float scale = 1f)
            {
                EnhancedThemedParticles.EroicaBloomBurstEnhanced(position, scale);
                UnifiedVFX.Eroica.Impact(position, scale);
            }
            
            /// <summary>
            /// Enhanced explosion with triumphant bloom.
            /// </summary>
            public static void ExplosionEnhanced(Vector2 position, float scale = 1f)
            {
                EnhancedThemedParticles.EroicaImpactEnhanced(position, scale);
                UnifiedVFX.Eroica.Explosion(position, scale);
            }
            
            /// <summary>
            /// Enhanced sakura petal burst with bloom.
            /// </summary>
            public static void SakuraBurstEnhanced(Vector2 position, int count = 12, float spread = 60f)
            {
                EnhancedThemedParticles.SakuraPetalsEnhanced(position, count, spread);
            }
            
            /// <summary>
            /// Enhanced music notes with bloom.
            /// </summary>
            public static void MusicNotesEnhanced(Vector2 position, int count = 6, float spread = 40f)
            {
                EnhancedThemedParticles.EroicaMusicNotesEnhanced(position, count, spread);
            }
            
            /// <summary>
            /// Enhanced trail with heroic bloom.
            /// </summary>
            public static void TrailEnhanced(Vector2 position, Vector2 velocity)
            {
                UnifiedVFX.Eroica.Trail(position, velocity, 1f);
                EnhancedThemedParticles.GenericEnhancedTrail(position, velocity, 
                    new Color(139, 0, 0), new Color(255, 215, 0));
            }
        }
        
        // ============================================================================
        // MOONLIGHT SONATA ENHANCED - Ethereal purple bloom
        // ============================================================================
        public static class MoonlightSonata
        {
            /// <summary>
            /// Enhanced impact with ethereal bloom stacking.
            /// </summary>
            public static void ImpactEnhanced(Vector2 position, float scale = 1f)
            {
                EnhancedThemedParticles.MoonlightImpactEnhanced(position, scale);
                UnifiedVFX.MoonlightSonata.Impact(position, scale);
            }
            
            /// <summary>
            /// Enhanced bloom burst with purple-to-blue gradient.
            /// </summary>
            public static void BloomBurstEnhanced(Vector2 position, float scale = 1f)
            {
                EnhancedThemedParticles.MoonlightBloomBurstEnhanced(position, scale);
            }
            
            /// <summary>
            /// Enhanced music notes with ethereal bloom.
            /// </summary>
            public static void MusicNotesEnhanced(Vector2 position, int count = 5, float spread = 35f)
            {
                EnhancedThemedParticles.MoonlightMusicNotesEnhanced(position, count, spread);
            }
            
            /// <summary>
            /// Enhanced aura with pulsing bloom.
            /// </summary>
            public static void AuraEnhanced(Vector2 center, float radius = 40f)
            {
                EnhancedThemedParticles.MoonlightAuraEnhanced(center, radius);
            }
            
            /// <summary>
            /// Enhanced trail with ethereal bloom.
            /// </summary>
            public static void TrailEnhanced(Vector2 position, Vector2 velocity)
            {
                UnifiedVFX.MoonlightSonata.Trail(position, velocity, 1f);
                EnhancedThemedParticles.GenericEnhancedTrail(position, velocity,
                    new Color(75, 0, 130), new Color(135, 206, 250));
            }
        }
        
        // ============================================================================
        // FATE ENHANCED - Celestial cosmic bloom (ENDGAME)
        // ============================================================================
        public static class Fate
        {
            /// <summary>
            /// Enhanced cosmic impact with glyphs, stars, and cosmic clouds.
            /// The ultimate Fate effect with all celestial elements.
            /// </summary>
            public static void ImpactEnhanced(Vector2 position, float scale = 1f)
            {
                EnhancedThemedParticles.FateImpactEnhanced(position, scale);
                UnifiedVFX.Fate.Impact(position, scale);
            }
            
            /// <summary>
            /// Enhanced cosmic bloom burst with celestial glory.
            /// </summary>
            public static void CosmicBurst(Vector2 position, float scale = 1f)
            {
                EnhancedThemedParticles.FateBloomBurstEnhanced(position, scale);
            }
            
            /// <summary>
            /// Enhanced glyph burst for cosmic magic.
            /// </summary>
            public static void GlyphBurstEnhanced(Vector2 position, int count = 8, float speed = 6f)
            {
                EnhancedThemedParticles.FateGlyphBurstEnhanced(position, count, speed);
            }
            
            /// <summary>
            /// Enhanced star trail for cosmic projectiles.
            /// Includes glyphs, star sparkles, and cosmic clouds.
            /// </summary>
            public static void StarTrailEnhanced(Vector2 position, Vector2 velocity)
            {
                EnhancedThemedParticles.FateStarTrailEnhanced(position, velocity);
            }
            
            /// <summary>
            /// Constellation burst - creates star pattern with connecting lines.
            /// </summary>
            public static void ConstellationBurst(Vector2 position, int starCount = 6, float radius = 50f, float scale = 1f)
            {
                // Create star points in a constellation pattern
                for (int i = 0; i < starCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / starCount + Main.rand.NextFloat(-0.3f, 0.3f);
                    float dist = radius * Main.rand.NextFloat(0.6f, 1f) * scale;
                    Vector2 starPos = position + angle.ToRotationVector2() * dist;
                    
                    // Star sparkle
                    EnhancedParticles.ShineFlare(starPos, Color.White, 0.4f, 25);
                    
                    // Glyph at star
                    var glyph = EnhancedParticlePool.GetParticle()
                        .Setup(CustomParticleSystem.RandomGlyph(), starPos, Vector2.Zero,
                            new Color(180, 50, 100), 0.3f, 35)
                        .WithBloom(2, 0.6f);
                    EnhancedParticlePool.SpawnParticle(glyph);
                }
                
                // Central cosmic burst
                EnhancedParticles.BloomFlare(position, Color.White, 0.6f * scale, 20, 4, 1.2f);
            }
            
            /// <summary>
            /// Reality tear effect - cosmic energy bleeding through reality.
            /// </summary>
            public static void RealityTear(Vector2 position, Vector2 direction, float length = 100f, float scale = 1f)
            {
                // Line of cosmic energy
                int points = (int)(length / 15f);
                for (int i = 0; i < points; i++)
                {
                    float progress = (float)i / points;
                    Vector2 pointPos = position + direction * (length * progress);
                    
                    // Dark prismatic gradient
                    Color tearColor;
                    if (progress < 0.4f)
                        tearColor = Color.Lerp(new Color(15, 5, 20), new Color(180, 50, 100), progress / 0.4f);
                    else
                        tearColor = Color.Lerp(new Color(180, 50, 100), new Color(255, 60, 80), (progress - 0.4f) / 0.6f);
                    
                    var particle = EnhancedParticlePool.GetParticle()
                        .Setup(CustomParticleSystem.RandomFlare(), pointPos + Main.rand.NextVector2Circular(5f, 5f),
                            direction.RotatedByRandom(0.3f) * Main.rand.NextFloat(1f, 3f),
                            tearColor, 0.3f * scale, 20 + Main.rand.Next(10))
                        .WithBloom(3, 0.8f);
                    EnhancedParticlePool.SpawnParticle(particle);
                    
                    // Star sparkle
                    if (Main.rand.NextBool(3))
                    {
                        EnhancedParticles.ShineFlare(pointPos, Color.White, 0.2f * scale, 15);
                    }
                }
            }
        }
        
        // ============================================================================
        // SWAN LAKE ENHANCED - Elegant feather bloom
        // ============================================================================
        public static class SwanLake
        {
            /// <summary>
            /// Enhanced impact with elegant bloom and rainbow shimmer.
            /// </summary>
            public static void ImpactEnhanced(Vector2 position, float scale = 1f)
            {
                EnhancedThemedParticles.SwanLakeBloomBurstEnhanced(position, scale);
                UnifiedVFX.SwanLake.Impact(position, scale);
            }
            
            /// <summary>
            /// Enhanced explosion with graceful bloom.
            /// </summary>
            public static void ExplosionEnhanced(Vector2 position, float scale = 1f)
            {
                EnhancedThemedParticles.SwanLakeBloomBurstEnhanced(position, scale * 1.5f);
                UnifiedVFX.SwanLake.Explosion(position, scale);
            }
            
            /// <summary>
            /// Enhanced feather burst with graceful bloom.
            /// </summary>
            public static void FeatherBurstEnhanced(Vector2 position, int count = 12, float scale = 1f)
            {
                EnhancedThemedParticles.SwanFeatherBurstEnhanced(position, count, scale);
            }
            
            /// <summary>
            /// Enhanced trail with elegant bloom.
            /// </summary>
            public static void TrailEnhanced(Vector2 position, Vector2 velocity)
            {
                UnifiedVFX.SwanLake.Trail(position, velocity, 1f);
                
                // Rainbow shimmer trail
                if (Main.rand.NextBool(4))
                {
                    float hue = (Main.GameUpdateCount * 0.02f + Main.rand.NextFloat()) % 1f;
                    Color rainbowColor = Main.hslToRgb(hue, 0.7f, 0.85f);
                    
                    var trail = EnhancedParticlePool.GetParticle()
                        .Setup(CustomParticleSystem.RandomGlow(), position + Main.rand.NextVector2Circular(6f, 6f),
                            -velocity * 0.08f, rainbowColor, 0.2f, 25)
                        .WithBloom(2, 0.4f)
                        .WithPulse(0.12f, 0.1f);
                    EnhancedParticlePool.SpawnParticle(trail);
                }
            }
        }
        
        // ============================================================================
        // ENIGMA VARIATIONS ENHANCED - Mysterious green flame bloom
        // ============================================================================
        public static class EnigmaVariations
        {
            /// <summary>
            /// Enhanced impact with mysterious bloom and eye particles.
            /// </summary>
            public static void ImpactEnhanced(Vector2 position, float scale = 1f)
            {
                EnhancedThemedParticles.EnigmaBloomBurstEnhanced(position, scale);
                UnifiedVFX.EnigmaVariations.Impact(position, scale);
            }
            
            /// <summary>
            /// Enhanced explosion with mysterious arcane bloom.
            /// </summary>
            public static void ExplosionEnhanced(Vector2 position, float scale = 1f)
            {
                EnhancedThemedParticles.EnigmaBloomBurstEnhanced(position, scale * 1.5f);
                UnifiedVFX.EnigmaVariations.Explosion(position, scale);
            }
            
            /// <summary>
            /// Enhanced trail with eerie green bloom.
            /// </summary>
            public static void TrailEnhanced(Vector2 position, Vector2 velocity)
            {
                UnifiedVFX.EnigmaVariations.Trail(position, velocity, 1f);
                EnhancedThemedParticles.GenericEnhancedTrail(position, velocity,
                    new Color(80, 20, 120), new Color(50, 220, 100));
            }
        }
        
        // ============================================================================
        // GENERIC ENHANCED - Theme-agnostic bloom utilities
        // ============================================================================
        public static class Generic
        {
            /// <summary>
            /// Generic bloom impact for any color scheme.
            /// </summary>
            public static void BloomImpact(Vector2 position, Color primaryColor, Color secondaryColor, float scale = 1f)
            {
                EnhancedThemedParticles.GenericEnhancedImpact(position, primaryColor, secondaryColor, scale);
            }
            
            /// <summary>
            /// Generic bloom trail for any projectile.
            /// </summary>
            public static void BloomTrail(Vector2 position, Vector2 velocity, Color primaryColor, Color secondaryColor)
            {
                EnhancedThemedParticles.GenericEnhancedTrail(position, velocity, primaryColor, secondaryColor);
            }
            
            /// <summary>
            /// Generic bloom flare with multi-layer stacking.
            /// </summary>
            public static void BloomFlare(Vector2 position, Color color, float scale = 0.5f, int lifetime = 20, 
                int layers = 4, float intensity = 1f)
            {
                EnhancedParticles.BloomFlare(position, color, scale, lifetime, layers, intensity);
            }
            
            /// <summary>
            /// Generic bloom burst with radial spread.
            /// </summary>
            public static void BloomBurst(Vector2 position, Color primaryColor, Color secondaryColor,
                int count = 8, float speed = 4f, float scale = 0.3f, int lifetime = 25)
            {
                EnhancedParticles.BloomBurst(position, primaryColor, secondaryColor, count, speed, scale, lifetime);
            }
            
            /// <summary>
            /// Shine flare (star sparkle) effect.
            /// </summary>
            public static void ShineFlare(Vector2 position, Color color, float scale = 0.4f, int lifetime = 15)
            {
                EnhancedParticles.ShineFlare(position, color, scale, lifetime);
            }
            
            /// <summary>
            /// Pulsing aura particles around a position.
            /// </summary>
            public static void PulsingAura(Vector2 position, Color color, int count = 5, float radius = 30f)
            {
                EnhancedParticles.PulsingAura(position, color, count, radius);
            }
            
            /// <summary>
            /// Themed bloom burst using theme name.
            /// Valid themes: "LaCampanella", "Eroica", "MoonlightSonata", "Fate", "SwanLake", "EnigmaVariations", etc.
            /// </summary>
            public static void ThemedBloomBurst(Vector2 position, string themeName, int count = 8, 
                float speed = 4f, float scale = 0.3f, int lifetime = 25)
            {
                EnhancedParticles.ThemedBloomBurst(position, themeName, count, speed, scale, lifetime);
            }
        }
    }
}
