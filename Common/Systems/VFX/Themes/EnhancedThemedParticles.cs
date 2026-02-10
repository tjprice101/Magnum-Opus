using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// Enhanced themed particle effects using FargosSoulsDLC rendering patterns.
    /// This class provides upgraded versions of ThemedParticles methods with:
    /// - Multi-layer bloom stacking
    /// - { A = 0 } pattern for proper additive blending
    /// - Theme palette gradients
    /// - Enhanced visual depth
    /// 
    /// Use these methods for maximum visual impact.
    /// </summary>
    public static class EnhancedThemedParticles
    {
        #region Moonlight Sonata Enhanced Effects
        
        /// <summary>
        /// Enhanced moonlight bloom burst with multi-layer stacking.
        /// Creates a stunning purple-to-blue radial burst with proper bloom layering.
        /// </summary>
        public static void MoonlightBloomBurstEnhanced(Vector2 position, float intensity = 1f)
        {
            // Core white flash with heavy bloom
            EnhancedParticles.BloomFlare(position, Color.White, 0.5f * intensity, 15, 4, 1.2f);
            
            // Primary bloom burst with theme colors
            EnhancedParticles.ThemedBloomBurst(position, "MoonlightSonata", 
                (int)(8 * intensity), 3f * intensity, 0.35f * intensity, 30);
            
            // Outer purple halo rings with gradient
            for (int i = 0; i < 3; i++)
            {
                float progress = i / 3f;
                Color ringColor = Color.Lerp(ThemedParticles.MoonlightDarkPurple, ThemedParticles.MoonlightLightBlue, progress);
                
                var ring = EnhancedParticlePool.GetParticle()
                    .Setup(CustomParticleSystem.RandomHalo(), position, Vector2.Zero,
                        ringColor, 0.3f + i * 0.2f, 25 + i * 8)
                    .WithBloom(3, 0.7f)
                    .WithScaleVelocity(0.02f + i * 0.01f);
                EnhancedParticlePool.SpawnParticle(ring);
            }
            
            // Silver accent sparkles
            for (int i = 0; i < (int)(5 * intensity); i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(25f * intensity, 25f * intensity);
                EnhancedParticles.ShineFlare(position + offset, ThemedParticles.MoonlightSilver, 0.25f, 20);
            }
            
            // Dynamic lighting
            Lighting.AddLight(position, ThemedParticles.MoonlightMediumPurple.ToVector3() * intensity);
        }
        
        /// <summary>
        /// Enhanced moonlight impact with fractal burst pattern.
        /// Ultimate version of MoonlightImpact with maximum visual spectacle.
        /// </summary>
        public static void MoonlightImpactEnhanced(Vector2 position, float intensity = 1f)
        {
            // Central bloom burst
            MoonlightBloomBurstEnhanced(position, intensity);
            
            // Fractal geometric pattern
            int points = 6;
            for (int i = 0; i < points; i++)
            {
                float angle = MathHelper.TwoPi * i / points;
                Vector2 offset = angle.ToRotationVector2() * (30f * intensity);
                float progress = (float)i / points;
                Color fractalColor = Color.Lerp(ThemedParticles.MoonlightDarkPurple, ThemedParticles.MoonlightSilver, progress);
                
                EnhancedParticles.BloomFlare(position + offset, fractalColor, 0.4f * intensity, 22, 3, 0.8f);
            }
            
            // Radial spark spray with gradient
            for (int i = 0; i < (int)(12 * intensity); i++)
            {
                float angle = MathHelper.TwoPi * i / (12 * intensity);
                float progress = (float)i / (12 * intensity);
                Vector2 velocity = angle.ToRotationVector2() * (5f + Main.rand.NextFloat(3f)) * intensity;
                Color sparkColor = Color.Lerp(ThemedParticles.MoonlightLightPurple, ThemedParticles.MoonlightIceBlue, progress);
                
                var spark = EnhancedParticlePool.GetParticle()
                    .Setup(CustomParticleSystem.RandomFlare(), position, velocity,
                        sparkColor, 0.25f, 25 + Main.rand.Next(10))
                    .WithBloom(2, 0.6f)
                    .WithDrag(0.95f)
                    .WithGravity(0.02f);
                EnhancedParticlePool.SpawnParticle(spark);
            }
            
            // Music notes with bloom
            MoonlightMusicNotesEnhanced(position, (int)(4 * intensity), 30f * intensity);
        }
        
        /// <summary>
        /// Enhanced moonlight music notes with bloom and gradient fading.
        /// </summary>
        public static void MoonlightMusicNotesEnhanced(Vector2 position, int count = 4, float spread = 30f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(spread, spread);
                Vector2 velocity = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), -1.2f - Main.rand.NextFloat(0.6f));
                float progress = (float)i / count;
                Color noteColor = Color.Lerp(ThemedParticles.MoonlightLightPurple, ThemedParticles.MoonlightSilver, progress);
                
                var note = EnhancedParticlePool.GetParticle()
                    .Setup(CustomParticleSystem.RandomNote(), position + offset, velocity,
                        noteColor, 0.3f + Main.rand.NextFloat(0.15f), 60 + Main.rand.Next(25))
                    .WithBloom(2, 0.5f)
                    .WithGravity(-0.015f)
                    .WithDrag(0.995f)
                    .WithGradient(ThemedParticles.MoonlightIceBlue);
                EnhancedParticlePool.SpawnParticle(note);
            }
        }
        
        /// <summary>
        /// Enhanced moonlight aura with pulsing bloom particles.
        /// </summary>
        public static void MoonlightAuraEnhanced(Vector2 center, float radius = 40f)
        {
            if (!Main.rand.NextBool(6)) return;
            
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            Vector2 offset = angle.ToRotationVector2() * Main.rand.NextFloat(radius * 0.5f, radius);
            Vector2 velocity = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-0.8f, -0.2f));
            
            Color color = Main.rand.NextBool(3) ? ThemedParticles.MoonlightSilver : ThemedParticles.MoonlightLightPurple;
            
            var auraParticle = EnhancedParticlePool.GetParticle()
                .Setup(CustomParticleSystem.RandomGlow(), center + offset, velocity,
                    color, 0.2f + Main.rand.NextFloat(0.15f), 50 + Main.rand.Next(30))
                .WithBloom(3, 0.4f)
                .WithPulse(0.1f, 0.15f);
            EnhancedParticlePool.SpawnParticle(auraParticle);
        }
        
        #endregion
        
        #region Eroica Enhanced Effects
        
        /// <summary>
        /// Enhanced eroica bloom burst with fiery multi-layer stacking.
        /// Creates a heroic scarlet-to-gold radial burst.
        /// </summary>
        public static void EroicaBloomBurstEnhanced(Vector2 position, float intensity = 1f)
        {
            // Core hot white flash
            EnhancedParticles.BloomFlare(position, new Color(255, 240, 220), 0.6f * intensity, 12, 4, 1.3f);
            
            // Primary fiery burst
            EnhancedParticles.ThemedBloomBurst(position, "Eroica",
                (int)(10 * intensity), 4f * intensity, 0.4f * intensity, 25);
            
            // Gold accent halos
            for (int i = 0; i < 3; i++)
            {
                float progress = i / 3f;
                Color ringColor = Color.Lerp(ThemedParticles.EroicaCrimson, ThemedParticles.EroicaGold, progress);
                
                var ring = EnhancedParticlePool.GetParticle()
                    .Setup(CustomParticleSystem.RandomHalo(), position, Vector2.Zero,
                        ringColor, 0.35f + i * 0.2f, 22 + i * 6)
                    .WithBloom(3, 0.8f)
                    .WithScaleVelocity(0.025f + i * 0.01f);
                EnhancedParticlePool.SpawnParticle(ring);
            }
            
            // Sakura petal accents
            for (int i = 0; i < (int)(4 * intensity); i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f * intensity, 20f * intensity);
                Vector2 velocity = Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0, -1f);
                
                var petal = EnhancedParticlePool.GetParticle()
                    .Setup(CustomParticleSystem.RandomGlow(), position + offset, velocity,
                        ThemedParticles.EroicaSakura, 0.3f, 40)
                    .WithBloom(2, 0.5f)
                    .WithGravity(0.03f)
                    .WithDrag(0.97f)
                    .WithRotationSpeed(Main.rand.NextFloat(-0.08f, 0.08f));
                EnhancedParticlePool.SpawnParticle(petal);
            }
            
            Lighting.AddLight(position, ThemedParticles.EroicaCrimson.ToVector3() * 1.2f * intensity);
        }
        
        /// <summary>
        /// Enhanced eroica impact with heroic fractal burst.
        /// Ultimate spectacle for triumphant moments.
        /// </summary>
        public static void EroicaImpactEnhanced(Vector2 position, float intensity = 1f)
        {
            // Central bloom
            EroicaBloomBurstEnhanced(position, intensity);
            
            // 8-point star fractal pattern
            int points = 8;
            for (int i = 0; i < points; i++)
            {
                float angle = MathHelper.TwoPi * i / points;
                float progress = (float)i / points;
                Vector2 offset = angle.ToRotationVector2() * (35f * intensity);
                Color fractalColor = Color.Lerp(ThemedParticles.EroicaScarlet, ThemedParticles.EroicaGold, progress);
                
                EnhancedParticles.BloomFlare(position + offset, fractalColor, 0.45f * intensity, 20, 3, 0.9f);
            }
            
            // Radial fire sparks
            for (int i = 0; i < (int)(16 * intensity); i++)
            {
                float angle = MathHelper.TwoPi * i / (16 * intensity);
                float progress = (float)i / (16 * intensity);
                Vector2 velocity = angle.ToRotationVector2() * (6f + Main.rand.NextFloat(4f)) * intensity;
                Color sparkColor = Color.Lerp(ThemedParticles.EroicaFlame, ThemedParticles.EroicaGold, progress);
                
                var spark = EnhancedParticlePool.GetParticle()
                    .Setup(CustomParticleSystem.RandomFlare(), position, velocity,
                        sparkColor, 0.3f, 22 + Main.rand.Next(12))
                    .WithBloom(2, 0.7f)
                    .WithDrag(0.94f)
                    .WithGravity(0.04f);
                EnhancedParticlePool.SpawnParticle(spark);
            }
            
            // Heavy smoke with bloom
            for (int i = 0; i < (int)(5 * intensity); i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(3f, 3f) + new Vector2(0, -1.5f);
                
                var smoke = EnhancedParticlePool.GetParticle()
                    .Setup(CustomParticleSystem.RandomGlow(), position + Main.rand.NextVector2Circular(15f, 15f), velocity,
                        ThemedParticles.EroicaBlack, 0.5f * intensity, 45 + Main.rand.Next(20))
                    .WithBloom(2, 0.3f)
                    .WithGravity(-0.02f)
                    .WithDrag(0.96f)
                    .WithScaleVelocity(0.015f);
                EnhancedParticlePool.SpawnParticle(smoke);
            }
            
            // Music notes
            EroicaMusicNotesEnhanced(position, (int)(5 * intensity), 35f * intensity);
        }
        
        /// <summary>
        /// Enhanced eroica music notes with heroic bloom.
        /// </summary>
        public static void EroicaMusicNotesEnhanced(Vector2 position, int count = 5, float spread = 35f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(spread, spread);
                Vector2 velocity = new Vector2(Main.rand.NextFloat(-1f, 1f), -1.5f - Main.rand.NextFloat(0.8f));
                Color noteColor = Main.rand.NextBool() ? ThemedParticles.EroicaGold : ThemedParticles.EroicaCrimson;
                
                var note = EnhancedParticlePool.GetParticle()
                    .Setup(CustomParticleSystem.RandomNote(), position + offset, velocity,
                        noteColor, 0.35f + Main.rand.NextFloat(0.15f), 55 + Main.rand.Next(25))
                    .WithBloom(2, 0.6f)
                    .WithGravity(-0.025f)
                    .WithDrag(0.99f);
                EnhancedParticlePool.SpawnParticle(note);
            }
        }
        
        /// <summary>
        /// Enhanced sakura petals with elegant bloom.
        /// </summary>
        public static void SakuraPetalsEnhanced(Vector2 position, int count = 8, float spread = 50f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(spread, spread);
                Vector2 velocity = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-0.5f, 0.5f));
                
                var petal = EnhancedParticlePool.GetParticle()
                    .Setup(CustomParticleSystem.RandomGlow(), position + offset, velocity,
                        ThemedParticles.EroicaSakura, 0.25f + Main.rand.NextFloat(0.15f), 70 + Main.rand.Next(40))
                    .WithBloom(2, 0.4f)
                    .WithGravity(0.015f)
                    .WithDrag(0.98f)
                    .WithRotationSpeed(Main.rand.NextFloat(-0.05f, 0.05f))
                    .WithPulse(0.08f, 0.1f);
                EnhancedParticlePool.SpawnParticle(petal);
            }
        }
        
        #endregion
        
        #region La Campanella Enhanced Effects
        
        /// <summary>
        /// Enhanced La Campanella bloom burst with infernal fire and smoke.
        /// </summary>
        public static void LaCampanellaBloomBurstEnhanced(Vector2 position, float intensity = 1f)
        {
            // Core infernal flash
            EnhancedParticles.BloomFlare(position, new Color(255, 200, 100), 0.7f * intensity, 15, 4, 1.4f);
            
            // Primary fire burst (black to orange gradient)
            EnhancedParticles.ThemedBloomBurst(position, "LaCampanella",
                (int)(12 * intensity), 5f * intensity, 0.45f * intensity, 22);
            
            // Heavy smoke layer
            for (int i = 0; i < (int)(8 * intensity); i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(3f, 3f) + new Vector2(0, -2f);
                
                var smoke = EnhancedParticlePool.GetParticle()
                    .Setup(CustomParticleSystem.RandomGlow(), position + Main.rand.NextVector2Circular(20f, 20f), velocity,
                        new Color(30, 25, 25), 0.6f * intensity, 50 + Main.rand.Next(25))
                    .WithBloom(2, 0.25f)
                    .WithGravity(-0.03f)
                    .WithDrag(0.95f)
                    .WithScaleVelocity(0.02f);
                EnhancedParticlePool.SpawnParticle(smoke);
            }
            
            Lighting.AddLight(position, new Vector3(1f, 0.5f, 0.2f) * 1.3f * intensity);
        }
        
        /// <summary>
        /// Enhanced La Campanella bell chime effect.
        /// </summary>
        public static void BellChimeEnhanced(Vector2 position, float intensity = 1f)
        {
            // Golden bell flash
            EnhancedParticles.ShineFlare(position, new Color(255, 215, 100), 0.6f * intensity, 25);
            
            // Resonance rings (sound waves)
            for (int i = 0; i < 4; i++)
            {
                float delay = i * 0.15f;
                Color ringColor = Color.Lerp(new Color(255, 200, 80), new Color(255, 100, 0), i / 4f);
                
                var ring = EnhancedParticlePool.GetParticle()
                    .Setup(CustomParticleSystem.RandomHalo(), position, Vector2.Zero,
                        ringColor, 0.2f + i * 0.1f, 30 + i * 8)
                    .WithBloom(3, 0.6f)
                    .WithScaleVelocity(0.04f + i * 0.01f);
                EnhancedParticlePool.SpawnParticle(ring);
            }
            
            // Music notes cascade
            LaCampanellaMusicNotesEnhanced(position, (int)(6 * intensity), 40f * intensity);
        }
        
        /// <summary>
        /// Enhanced La Campanella music notes with fiery glow.
        /// </summary>
        public static void LaCampanellaMusicNotesEnhanced(Vector2 position, int count = 6, float spread = 40f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(spread, spread);
                Vector2 velocity = new Vector2(Main.rand.NextFloat(-1.2f, 1.2f), -1.8f - Main.rand.NextFloat(1f));
                float progress = (float)i / count;
                Color noteColor = Color.Lerp(new Color(255, 100, 0), new Color(255, 215, 100), progress);
                
                var note = EnhancedParticlePool.GetParticle()
                    .Setup(CustomParticleSystem.RandomNote(), position + offset, velocity,
                        noteColor, 0.35f + Main.rand.NextFloat(0.2f), 50 + Main.rand.Next(25))
                    .WithBloom(2, 0.7f)
                    .WithGravity(-0.03f)
                    .WithDrag(0.985f);
                EnhancedParticlePool.SpawnParticle(note);
            }
        }
        
        #endregion
        
        #region Fate Enhanced Effects (Celestial Cosmic)
        
        /// <summary>
        /// Enhanced Fate cosmic bloom burst with celestial glory.
        /// Includes glyphs, star sparkles, and cosmic cloud energy.
        /// </summary>
        public static void FateBloomBurstEnhanced(Vector2 position, float intensity = 1f)
        {
            // Core celestial white flash
            EnhancedParticles.BloomFlare(position, Color.White, 0.8f * intensity, 18, 4, 1.5f);
            
            // Primary cosmic burst (dark prismatic gradient)
            EnhancedParticles.ThemedBloomBurst(position, "Fate",
                (int)(10 * intensity), 4f * intensity, 0.4f * intensity, 28);
            
            // Glyph orbit burst
            for (int i = 0; i < (int)(6 * intensity); i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 glyphPos = position + angle.ToRotationVector2() * (35f * intensity);
                float progress = (float)i / 6f;
                Color glyphColor = Color.Lerp(new Color(180, 50, 100), new Color(255, 60, 80), progress);
                
                var glyph = EnhancedParticlePool.GetParticle()
                    .Setup(CustomParticleSystem.RandomGlyph(), glyphPos, Vector2.Zero,
                        glyphColor, 0.35f, 35)
                    .WithBloom(3, 0.8f)
                    .WithRotationSpeed(0.03f);
                EnhancedParticlePool.SpawnParticle(glyph);
            }
            
            // Star sparkles
            for (int i = 0; i < (int)(8 * intensity); i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(40f * intensity, 40f * intensity);
                EnhancedParticles.ShineFlare(position + offset, Color.White, 0.25f, 18);
            }
            
            // Cosmic cloud wisps
            for (int i = 0; i < (int)(6 * intensity); i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(2f, 2f);
                Color cloudColor = Color.Lerp(new Color(15, 5, 20), new Color(120, 30, 140), Main.rand.NextFloat());
                
                var cloud = EnhancedParticlePool.GetParticle()
                    .Setup(CustomParticleSystem.RandomGlow(), position + Main.rand.NextVector2Circular(25f, 25f), velocity,
                        cloudColor, 0.4f, 40)
                    .WithBloom(2, 0.4f)
                    .WithDrag(0.97f)
                    .WithScaleVelocity(0.01f);
                EnhancedParticlePool.SpawnParticle(cloud);
            }
            
            Lighting.AddLight(position, new Vector3(0.8f, 0.4f, 0.6f) * 1.2f * intensity);
        }
        
        /// <summary>
        /// Enhanced Fate cosmic impact - reality-shattering spectacle.
        /// </summary>
        public static void FateImpactEnhanced(Vector2 position, float intensity = 1f)
        {
            // Central cosmic burst
            FateBloomBurstEnhanced(position, intensity);
            
            // Celestial star fractal pattern
            int points = 8;
            for (int i = 0; i < points; i++)
            {
                float angle = MathHelper.TwoPi * i / points;
                float progress = (float)i / points;
                Vector2 offset = angle.ToRotationVector2() * (40f * intensity);
                
                // Dark prismatic gradient: black ↁEpink ↁEred
                Color fractalColor;
                if (progress < 0.4f)
                    fractalColor = Color.Lerp(new Color(15, 5, 20), new Color(180, 50, 100), progress / 0.4f);
                else if (progress < 0.8f)
                    fractalColor = Color.Lerp(new Color(180, 50, 100), new Color(255, 60, 80), (progress - 0.4f) / 0.4f);
                else
                    fractalColor = Color.Lerp(new Color(255, 60, 80), Color.White, (progress - 0.8f) / 0.2f);
                
                EnhancedParticles.BloomFlare(position + offset, fractalColor, 0.5f * intensity, 25, 3, 1f);
                
                // Star sparkle at each point
                EnhancedParticles.ShineFlare(position + offset, Color.White, 0.3f, 20);
            }
            
            // Radial cosmic sparks
            for (int i = 0; i < (int)(14 * intensity); i++)
            {
                float angle = MathHelper.TwoPi * i / (14 * intensity);
                Vector2 velocity = angle.ToRotationVector2() * (5f + Main.rand.NextFloat(3f)) * intensity;
                float progress = (float)i / (14 * intensity);
                Color sparkColor = Color.Lerp(new Color(180, 50, 100), new Color(255, 60, 80), progress);
                
                var spark = EnhancedParticlePool.GetParticle()
                    .Setup(CustomParticleSystem.RandomFlare(), position, velocity,
                        sparkColor, 0.3f, 26 + Main.rand.Next(12))
                    .WithBloom(2, 0.8f)
                    .WithDrag(0.95f);
                EnhancedParticlePool.SpawnParticle(spark);
            }
            
            // Glyph cascade
            FateGlyphBurstEnhanced(position, (int)(8 * intensity), 6f * intensity);
        }
        
        /// <summary>
        /// Enhanced Fate glyph burst for cosmic magic effects.
        /// </summary>
        public static void FateGlyphBurstEnhanced(Vector2 position, int count = 8, float speed = 6f)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 velocity = angle.ToRotationVector2() * (speed * 0.5f + Main.rand.NextFloat(speed * 0.5f));
                float progress = (float)i / count;
                Color glyphColor = Color.Lerp(new Color(120, 30, 140), new Color(255, 60, 80), progress);
                
                var glyph = EnhancedParticlePool.GetParticle()
                    .Setup(CustomParticleSystem.RandomGlyph(), position, velocity,
                        glyphColor, 0.35f + Main.rand.NextFloat(0.15f), 35 + Main.rand.Next(15))
                    .WithBloom(3, 0.7f)
                    .WithDrag(0.96f)
                    .WithRotationSpeed(Main.rand.NextFloat(-0.1f, 0.1f));
                EnhancedParticlePool.SpawnParticle(glyph);
            }
        }
        
        /// <summary>
        /// Enhanced Fate star trail for projectiles.
        /// </summary>
        public static void FateStarTrailEnhanced(Vector2 position, Vector2 velocity)
        {
            // Cosmic cloud trail
            if (Main.rand.NextBool(2))
            {
                Color cloudColor = Color.Lerp(new Color(15, 5, 20), new Color(120, 30, 140), Main.rand.NextFloat());
                
                var cloud = EnhancedParticlePool.GetParticle()
                    .Setup(CustomParticleSystem.RandomGlow(), position + Main.rand.NextVector2Circular(8f, 8f), 
                        -velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f),
                        cloudColor, 0.35f, 25)
                    .WithBloom(2, 0.4f)
                    .WithDrag(0.98f);
                EnhancedParticlePool.SpawnParticle(cloud);
            }
            
            // Star sparkles
            if (Main.rand.NextBool(4))
            {
                Vector2 offset = Main.rand.NextVector2Circular(12f, 12f);
                EnhancedParticles.ShineFlare(position + offset, Color.White, 0.2f, 15);
            }
            
            // Occasional glyph
            if (Main.rand.NextBool(8))
            {
                var glyph = EnhancedParticlePool.GetParticle()
                    .Setup(CustomParticleSystem.RandomGlyph(), position, -velocity * 0.05f,
                        new Color(180, 50, 100), 0.25f, 30)
                    .WithBloom(2, 0.5f)
                    .WithRotationSpeed(0.05f);
                EnhancedParticlePool.SpawnParticle(glyph);
            }
        }
        
        #endregion
        
        #region Swan Lake Enhanced Effects
        
        /// <summary>
        /// Enhanced Swan Lake bloom burst with elegant feathers.
        /// </summary>
        public static void SwanLakeBloomBurstEnhanced(Vector2 position, float intensity = 1f)
        {
            // Pure white core flash
            EnhancedParticles.BloomFlare(position, Color.White, 0.7f * intensity, 20, 4, 1.3f);
            
            // Primary elegant burst
            EnhancedParticles.ThemedBloomBurst(position, "SwanLake",
                (int)(8 * intensity), 3f * intensity, 0.35f * intensity, 32);
            
            // Rainbow shimmer accents
            for (int i = 0; i < (int)(6 * intensity); i++)
            {
                float hue = (Main.GameUpdateCount * 0.02f + i * 0.15f) % 1f;
                Color rainbowColor = Main.hslToRgb(hue, 0.7f, 0.85f);
                Vector2 offset = Main.rand.NextVector2Circular(30f * intensity, 30f * intensity);
                
                var shimmer = EnhancedParticlePool.GetParticle()
                    .Setup(CustomParticleSystem.RandomGlow(), position + offset, Main.rand.NextVector2Circular(1f, 1f),
                        rainbowColor, 0.25f, 30)
                    .WithBloom(2, 0.5f)
                    .WithPulse(0.15f, 0.1f);
                EnhancedParticlePool.SpawnParticle(shimmer);
            }
            
            Lighting.AddLight(position, new Vector3(0.9f, 0.95f, 1f) * 1.1f * intensity);
        }
        
        /// <summary>
        /// Enhanced Swan Lake feather burst with graceful bloom.
        /// </summary>
        public static void SwanFeatherBurstEnhanced(Vector2 position, int count = 12, float intensity = 1f)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 velocity = angle.ToRotationVector2() * (2f + Main.rand.NextFloat(1.5f)) * intensity;
                
                // Alternate white and black feathers
                Color featherColor = i % 2 == 0 ? Color.White : new Color(30, 30, 40);
                
                var feather = EnhancedParticlePool.GetParticle()
                    .Setup(CustomParticleSystem.RandomSwanFeather(), position, velocity,
                        featherColor, 0.35f + Main.rand.NextFloat(0.2f), 50 + Main.rand.Next(30))
                    .WithBloom(2, featherColor == Color.White ? 0.5f : 0.2f)
                    .WithGravity(0.02f)
                    .WithDrag(0.97f)
                    .WithRotationSpeed(Main.rand.NextFloat(-0.04f, 0.04f));
                EnhancedParticlePool.SpawnParticle(feather);
            }
            
            // Central elegant flash
            EnhancedParticles.ShineFlare(position, Color.White, 0.5f * intensity, 20);
        }
        
        /// <summary>
        /// Enhanced Swan Lake music notes with elegant bloom.
        /// </summary>
        public static void SwanLakeMusicNotesEnhanced(Vector2 position, int count = 5, float spread = 35f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(spread, spread);
                Vector2 velocity = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-2f, -0.5f));
                
                // Alternate white and rainbow shimmer notes
                float hue = (Main.GameUpdateCount * 0.02f + i * 0.2f) % 1f;
                Color noteColor = i % 2 == 0 ? Color.White : Main.hslToRgb(hue, 0.7f, 0.85f);
                
                var note = EnhancedParticlePool.GetParticle()
                    .Setup(CustomParticleSystem.RandomNote(), position + offset, velocity,
                        noteColor, 0.35f + Main.rand.NextFloat(0.15f), 40 + Main.rand.Next(20))
                    .WithBloom(2, 0.5f)
                    .WithGravity(-0.01f)
                    .WithRotationSpeed(Main.rand.NextFloat(-0.02f, 0.02f));
                EnhancedParticlePool.SpawnParticle(note);
            }
        }
        
        #endregion
        
        #region Enigma Variations Enhanced Effects
        
        /// <summary>
        /// Enhanced Enigma bloom burst with mysterious green flames.
        /// </summary>
        public static void EnigmaBloomBurstEnhanced(Vector2 position, float intensity = 1f)
        {
            // Eerie green core
            EnhancedParticles.BloomFlare(position, new Color(50, 220, 100), 0.6f * intensity, 18, 4, 1.2f);
            
            // Primary mysterious burst
            EnhancedParticles.ThemedBloomBurst(position, "EnigmaVariations",
                (int)(10 * intensity), 4f * intensity, 0.4f * intensity, 26);
            
            // Eye particles at impact (meaningful placement - looking at center)
            for (int i = 0; i < (int)(3 * intensity); i++)
            {
                float angle = MathHelper.TwoPi * i / 3f;
                Vector2 eyePos = position + angle.ToRotationVector2() * (30f * intensity);
                
                var eye = EnhancedParticlePool.GetParticle()
                    .Setup(CustomParticleSystem.RandomEnigmaEye(), eyePos, Vector2.Zero,
                        new Color(140, 60, 200), 0.35f, 40)
                    .WithBloom(2, 0.6f)
                    .WithRotationSpeed(0f); // Eyes don't rotate - they watch
                EnhancedParticlePool.SpawnParticle(eye);
            }
            
            // Green flame wisps
            for (int i = 0; i < (int)(6 * intensity); i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0, -1.5f);
                
                var flame = EnhancedParticlePool.GetParticle()
                    .Setup(CustomParticleSystem.RandomGlow(), position + Main.rand.NextVector2Circular(20f, 20f), velocity,
                        new Color(50, 220, 100), 0.3f, 35)
                    .WithBloom(3, 0.6f)
                    .WithGravity(-0.02f)
                    .WithDrag(0.97f);
                EnhancedParticlePool.SpawnParticle(flame);
            }
            
            Lighting.AddLight(position, new Vector3(0.3f, 0.8f, 0.4f) * 1.1f * intensity);
        }
        
        /// <summary>
        /// Enhanced Enigma music notes with mysterious bloom.
        /// </summary>
        public static void EnigmaMusicNotesEnhanced(Vector2 position, int count = 5, float spread = 35f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(spread, spread);
                Vector2 velocity = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-2f, -0.5f));
                
                // Enigma theme colors: purple to green gradient
                float progress = (float)i / count;
                Color noteColor = Color.Lerp(new Color(140, 60, 200), new Color(50, 220, 100), progress);
                
                var note = EnhancedParticlePool.GetParticle()
                    .Setup(CustomParticleSystem.RandomNote(), position + offset, velocity,
                        noteColor, 0.35f + Main.rand.NextFloat(0.15f), 40 + Main.rand.Next(20))
                    .WithBloom(3, 0.6f)
                    .WithGravity(-0.015f)
                    .WithRotationSpeed(Main.rand.NextFloat(-0.03f, 0.03f));
                EnhancedParticlePool.SpawnParticle(note);
            }
        }
        
        #endregion
        
        #region Generic Enhanced Effects
        
        /// <summary>
        /// Generic enhanced impact for any theme.
        /// </summary>
        public static void GenericEnhancedImpact(Vector2 position, Color primaryColor, Color secondaryColor, float intensity = 1f)
        {
            // Core flash
            EnhancedParticles.BloomFlare(position, Color.White, 0.6f * intensity, 15, 4, 1.2f);
            
            // Themed burst
            EnhancedParticles.BloomBurst(position, primaryColor, secondaryColor,
                (int)(10 * intensity), 4f * intensity, 0.35f * intensity, 25);
            
            // Fractal pattern
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 offset = angle.ToRotationVector2() * (30f * intensity);
                float progress = (float)i / 6f;
                Color fractalColor = Color.Lerp(primaryColor, secondaryColor, progress);
                
                EnhancedParticles.BloomFlare(position + offset, fractalColor, 0.4f * intensity, 20, 3, 0.8f);
            }
        }
        
        /// <summary>
        /// Generic enhanced trail for any projectile.
        /// </summary>
        public static void GenericEnhancedTrail(Vector2 position, Vector2 velocity, Color primaryColor, Color secondaryColor)
        {
            if (Main.rand.NextBool(2))
            {
                float progress = Main.rand.NextFloat();
                Color trailColor = Color.Lerp(primaryColor, secondaryColor, progress);
                
                var trail = EnhancedParticlePool.GetParticle()
                    .Setup(CustomParticleSystem.RandomGlow(), position + Main.rand.NextVector2Circular(6f, 6f),
                        -velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f),
                        trailColor, 0.25f + Main.rand.NextFloat(0.15f), 18 + Main.rand.Next(10))
                    .WithBloom(2, 0.5f)
                    .WithDrag(0.97f);
                EnhancedParticlePool.SpawnParticle(trail);
            }
        }
        
        #endregion
    }
}
