using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// BOSS SIGNATURE VFX - Unique Attack Patterns for Each Boss
    /// 
    /// Each boss in MagnumOpus gets a signature set of VFX for their attacks.
    /// These are designed to make each boss fight visually memorable and distinct.
    /// 
    /// USAGE: Call these methods from individual boss attack methods to enhance them.
    /// 
    /// Bosses supported:
    /// - Eroica, God of Valor: Heroic flames, sakura bursts, golden valor strikes
    /// - Fate, Warden of Melodies: Cosmic glyphs, star cascades, reality tears
    /// - Swan Lake, Monochromatic Fractal: Prismatic feathers, graceful arcs, ballet strikes
    /// - La Campanella, Chime of Life: Bell resonance, infernal flames, smoke vortex
    /// - Enigma, The Hollow Mystery: Void eyes, paradox glyphs, reality distortion
    /// - Dies Irae, Herald of Judgment: Wrath flames, judgment strikes, divine fire
    /// - Moonlight Sonata: Lunar beams, ethereal mist, silver cascades
    /// - Nachtmusik, Queen of Radiance: Golden radiance, celestial light, star patterns
    /// - OdeToJoy, Chromatic Rose Conductor: Rainbow symphonies, harmonic bursts
    /// - Seasons (Primavera, L'Estate, L'Autunno, L'Inverno): Seasonal effects
    /// </summary>
    public static class BossSignatureVFX
    {
        #region Eroica - God of Valor
        
        /// <summary>
        /// Heroic valor strike - sword impacts with golden flames and sakura.
        /// </summary>
        public static void EroicaValorStrike(Vector2 position, Vector2 direction, float intensity = 1f)
        {
            // Golden core flash
            CustomParticles.GenericFlare(position, Color.White, 1.2f * intensity, 20);
            CustomParticles.GenericFlare(position, new Color(255, 200, 80), 0.9f * intensity, 22);
            
            // Sakura burst in impact direction
            for (int i = 0; i < (int)(8 * intensity); i++)
            {
                float angle = direction.ToRotation() + Main.rand.NextFloat(-0.5f, 0.5f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 10f);
                Color sakura = Color.Lerp(new Color(255, 150, 180), new Color(255, 200, 220), Main.rand.NextFloat());
                
                Dust d = Dust.NewDustPerfect(position, DustID.PinkTorch, vel, 0, sakura, 1.2f);
                d.noGravity = true;
            }
            
            // Fog burst
            WeaponFogVFX.SpawnAttackFog(position, "Eroica", 0.8f * intensity, direction);
            
            // Light beam
            LightBeamImpactVFX.SpawnImpact(position, "Eroica", 0.9f * intensity);
            
            // Cascading halos
            for (int ring = 0; ring < 4; ring++)
            {
                Color ringColor = Color.Lerp(new Color(255, 200, 80), new Color(200, 50, 50), ring / 4f);
                CustomParticles.HaloRing(position, ringColor, 0.3f * intensity + ring * 0.1f, 15 + ring * 3);
            }
        }
        
        /// <summary>
        /// Heroes Judgment radial burst signature effect.
        /// </summary>
        public static void EroicaHeroesJudgment(Vector2 center, int wave, int totalWaves, float intensity = 1f)
        {
            float waveProgress = (float)wave / totalWaves;
            
            // Escalating golden explosion
            CustomParticles.GenericFlare(center, Color.White, 1.5f * intensity * (1f + waveProgress * 0.5f), 25);
            
            // Multi-ring halos
            int ringCount = 6 + wave * 2;
            for (int ring = 0; ring < ringCount; ring++)
            {
                float progress = (float)ring / ringCount;
                Color ringColor = Color.Lerp(new Color(200, 50, 50), new Color(255, 200, 80), progress);
                float scale = (0.4f + ring * 0.12f) * intensity;
                CustomParticles.HaloRing(center, ringColor, scale, 20 + ring * 2);
            }
            
            // Radial sakura burst
            int petalCount = 12 + wave * 4;
            for (int i = 0; i < petalCount; i++)
            {
                float angle = MathHelper.TwoPi * i / petalCount;
                Vector2 pos = center + angle.ToRotationVector2() * 40f;
                
                Dust d = Dust.NewDustPerfect(pos, DustID.PinkTorch, angle.ToRotationVector2() * (8f + wave * 2f), 0, 
                    Color.Lerp(new Color(255, 150, 180), new Color(255, 200, 80), (float)i / petalCount), 1.3f);
                d.noGravity = true;
            }
            
            // Sky flash on final wave
            if (wave == totalWaves)
            {
                DynamicSkyboxSystem.TriggerFlash(new Color(255, 200, 80), 1f * intensity);
            }
            
            // Fog explosion
            WeaponFogVFX.SpawnAttackFog(center, "Eroica", 1.2f * intensity, Vector2.Zero);
        }
        
        /// <summary>
        /// Phoenix dive impact - flame eruption from below.
        /// </summary>
        public static void EroicaPhoenixDive(Vector2 impactPoint, float intensity = 1f)
        {
            // Ground impact flash
            CustomParticles.GenericFlare(impactPoint, Color.White, 2f * intensity, 25);
            CustomParticles.GenericFlare(impactPoint, new Color(255, 100, 50), 1.5f * intensity, 28);
            
            // Rising flame pillars
            for (int pillar = 0; pillar < 5; pillar++)
            {
                float xOffset = (pillar - 2) * 80f;
                Vector2 pillarPos = impactPoint + new Vector2(xOffset, 0);
                
                for (int height = 0; height < 8; height++)
                {
                    Vector2 flamePos = pillarPos + new Vector2(Main.rand.NextFloat(-10f, 10f), -height * 30f);
                    
                    Dust flame = Dust.NewDustPerfect(flamePos, DustID.Torch, new Vector2(0, -6f - height * 0.5f), 0, 
                        Color.Lerp(new Color(255, 100, 50), new Color(255, 200, 80), height / 8f), 2f - height * 0.15f);
                    flame.noGravity = true;
                }
            }
            
            // Shockwave halos
            for (int ring = 0; ring < 8; ring++)
            {
                CustomParticles.HaloRing(impactPoint, Color.Lerp(new Color(200, 50, 50), new Color(255, 200, 80), ring / 8f),
                    0.5f * intensity + ring * 0.2f, 18 + ring * 3);
            }
            
            // Major fog burst
            WeaponFogVFX.SpawnAttackFog(impactPoint, "Eroica", 2f * intensity, Vector2.UnitY * -5f);
            LightBeamImpactVFX.SpawnImpact(impactPoint, "Eroica", 2f * intensity);
            
            // Screen shake
            if (Main.LocalPlayer.Distance(impactPoint) < 1500f)
            {
                Main.LocalPlayer.GetModPlayer<MagnumScreenShakePlayer>()?.AddShake(15f * intensity, 30);
            }
        }
        
        #endregion
        
        #region Fate - Warden of Melodies
        
        /// <summary>
        /// Cosmic judgment burst with glyphs and stars.
        /// </summary>
        public static void FateCosmicJudgment(Vector2 center, int wave, int totalWaves, float intensity = 1f)
        {
            float waveProgress = (float)wave / totalWaves;
            
            // Cosmic core flash - dark to bright
            CustomParticles.GenericFlare(center, Color.White, 1.8f * intensity * (1f + waveProgress * 0.3f), 25);
            CustomParticles.GenericFlare(center, new Color(255, 60, 80), 1.2f * intensity, 28);
            CustomParticles.GenericFlare(center, new Color(180, 50, 100), 0.8f * intensity, 30);
            
            // Glyph circle explosion
            int glyphCount = 8 + wave * 2;
            for (int i = 0; i < glyphCount; i++)
            {
                float angle = MathHelper.TwoPi * i / glyphCount + Main.GameUpdateCount * 0.02f;
                float radius = 50f + wave * 20f;
                Vector2 glyphPos = center + angle.ToRotationVector2() * radius;
                
                CustomParticles.Glyph(glyphPos, new Color(200, 80, 120) * (0.7f + waveProgress * 0.3f), 0.5f * intensity, i % 12);
            }
            
            // Star sparkle burst
            int starCount = 15 + wave * 5;
            for (int i = 0; i < starCount; i++)
            {
                float angle = MathHelper.TwoPi * i / starCount;
                Vector2 starVel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 14f);
                
                CustomParticles.GenericFlare(center + starVel * 3f, Color.White, 0.3f, 20);
                CustomParticles.PrismaticSparkle(center + starVel * 5f, 
                    Color.Lerp(new Color(180, 50, 100), new Color(255, 255, 255), Main.rand.NextFloat()), 0.35f);
            }
            
            // Reality distortion halos
            for (int ring = 0; ring < 10; ring++)
            {
                float ringProgress = (float)ring / 10f;
                Color ringColor = Color.Lerp(new Color(180, 50, 100), new Color(255, 60, 80), ringProgress);
                CustomParticles.HaloRing(center, ringColor, 0.4f * intensity + ring * 0.15f, 22 + ring * 2);
            }
            
            // Sky flash
            if (wave >= totalWaves - 1)
            {
                DynamicSkyboxSystem.TriggerFlash(new Color(255, 60, 80), 1.2f * intensity);
            }
            
            // Cosmic fog
            WeaponFogVFX.SpawnAttackFog(center, "Fate", 1.5f * intensity, Vector2.Zero);
        }
        
        /// <summary>
        /// Constellation strike - connecting star pattern attack.
        /// </summary>
        public static void FateConstellationStrike(Vector2[] starPoints, float progress, float intensity = 1f)
        {
            if (starPoints == null || starPoints.Length < 2) return;
            
            // Star points with bloom
            for (int i = 0; i < starPoints.Length; i++)
            {
                float starProgress = (float)i / starPoints.Length;
                
                CustomParticles.GenericFlare(starPoints[i], Color.White, 0.6f * intensity * (0.5f + progress * 0.5f), 15);
                CustomParticles.GenericFlare(starPoints[i], new Color(255, 230, 180), 0.4f * intensity, 18);
                
                // Glyph at each star
                if (progress > starProgress)
                {
                    CustomParticles.Glyph(starPoints[i], new Color(200, 80, 120), 0.4f, i % 12);
                }
            }
            
            // Connecting lines (implied via particles)
            for (int i = 0; i < starPoints.Length - 1; i++)
            {
                if (progress < (float)(i + 1) / starPoints.Length) continue;
                
                Vector2 start = starPoints[i];
                Vector2 end = starPoints[i + 1];
                Vector2 direction = end - start;
                float length = direction.Length();
                direction.Normalize();
                
                int linePoints = (int)(length / 20f);
                for (int p = 0; p < linePoints; p++)
                {
                    float t = (float)p / linePoints;
                    Vector2 linePos = Vector2.Lerp(start, end, t);
                    CustomParticles.GenericFlare(linePos, new Color(180, 50, 100) * 0.5f, 0.2f, 10);
                }
            }
        }
        
        /// <summary>
        /// Time slice attack - reality-cutting slashes.
        /// </summary>
        public static void FateTimeSlice(Vector2 start, Vector2 end, float intensity = 1f)
        {
            Vector2 direction = (end - start);
            float length = direction.Length();
            direction.Normalize();
            
            // Chromatic aberration along the slice
            Color[] colors = { new Color(255, 100, 100, 0), new Color(100, 255, 100, 0), new Color(100, 100, 255, 0) };
            for (int c = 0; c < 3; c++)
            {
                Vector2 offset = direction.RotatedBy(MathHelper.PiOver2) * (c - 1) * 3f;
                
                int slicePoints = (int)(length / 15f);
                for (int i = 0; i < slicePoints; i++)
                {
                    float t = (float)i / slicePoints;
                    Vector2 slicePos = Vector2.Lerp(start, end, t) + offset;
                    CustomParticles.GenericFlare(slicePos, colors[c], 0.3f * intensity, 12);
                }
            }
            
            // Impact points at start and end
            CustomParticles.GenericFlare(start, Color.White, 0.8f * intensity, 18);
            CustomParticles.GenericFlare(end, Color.White, 0.8f * intensity, 18);
            
            // Glyphs at endpoints
            CustomParticles.Glyph(start, new Color(200, 80, 120), 0.5f * intensity, Main.rand.Next(12));
            CustomParticles.Glyph(end, new Color(200, 80, 120), 0.5f * intensity, Main.rand.Next(12));
        }
        
        #endregion
        
        #region Swan Lake - Monochromatic Fractal
        
        /// <summary>
        /// Graceful strike - elegant feather attack with prismatic edges.
        /// </summary>
        public static void SwanLakeGracefulStrike(Vector2 position, Vector2 direction, float intensity = 1f)
        {
            // Pure white/black contrast core
            CustomParticles.GenericFlare(position, Color.White, 1f * intensity, 20);
            CustomParticles.GenericFlare(position, new Color(30, 30, 40), 0.7f * intensity, 22);
            
            // Feather burst in direction
            for (int i = 0; i < (int)(10 * intensity); i++)
            {
                float angle = direction.ToRotation() + Main.rand.NextFloat(-0.4f, 0.4f);
                Vector2 featherVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 12f);
                
                bool isWhite = Main.rand.NextBool();
                CustomParticles.SwanFeatherDrift(position + featherVel * 2f, 
                    isWhite ? Color.White : new Color(20, 20, 30), 0.5f * intensity);
            }
            
            // Prismatic sparkle accents
            for (int i = 0; i < 6; i++)
            {
                float hue = (Main.GameUpdateCount * 0.02f + i * 0.15f) % 1f;
                Color rainbow = Main.hslToRgb(hue, 1f, 0.8f);
                CustomParticles.PrismaticSparkle(position + Main.rand.NextVector2Circular(30f, 30f), rainbow, 0.3f);
            }
            
            // Monochrome halos
            CustomParticles.HaloRing(position, Color.White, 0.4f * intensity, 18);
            CustomParticles.HaloRing(position, new Color(30, 30, 40), 0.3f * intensity, 20);
            
            // Fog and beam
            WeaponFogVFX.SpawnAttackFog(position, "SwanLake", 0.8f * intensity, direction);
            LightBeamImpactVFX.SpawnImpact(position, "SwanLake", 0.9f * intensity);
        }
        
        /// <summary>
        /// Swan Serenade - Hero's Judgment style with prismatic theme.
        /// </summary>
        public static void SwanLakeSerenade(Vector2 center, int wave, int totalWaves, float intensity = 1f)
        {
            float waveProgress = (float)wave / totalWaves;
            
            // Alternating black/white core flash
            bool isWhiteWave = wave % 2 == 0;
            CustomParticles.GenericFlare(center, Color.White, 1.6f * intensity, 25);
            CustomParticles.GenericFlare(center, isWhiteWave ? Color.White : new Color(30, 30, 40), 1.2f * intensity, 28);
            
            // Rainbow halo cascade
            int ringCount = 8 + wave * 2;
            for (int ring = 0; ring < ringCount; ring++)
            {
                float hue = (ring * 0.1f + Main.GameUpdateCount * 0.01f) % 1f;
                Color rainbowColor = Main.hslToRgb(hue, 1f, 0.7f);
                CustomParticles.HaloRing(center, rainbowColor, 0.3f * intensity + ring * 0.1f, 18 + ring * 2);
            }
            
            // Radial feather burst
            int featherCount = 16 + wave * 4;
            for (int i = 0; i < featherCount; i++)
            {
                float angle = MathHelper.TwoPi * i / featherCount;
                Vector2 featherVel = angle.ToRotationVector2() * (10f + wave * 3f);
                
                CustomParticles.SwanFeatherDrift(center + featherVel * 2f, 
                    i % 2 == 0 ? Color.White : new Color(20, 20, 30), 0.6f * intensity);
            }
            
            // Prismatic sparkle ring
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f + Main.GameUpdateCount * 0.03f;
                Vector2 sparklePos = center + angle.ToRotationVector2() * 60f;
                float hue = ((float)i / 12f + Main.GameUpdateCount * 0.01f) % 1f;
                CustomParticles.PrismaticSparkle(sparklePos, Main.hslToRgb(hue, 1f, 0.85f), 0.4f);
            }
            
            // Sky flash on climax
            if (wave == totalWaves)
            {
                float hue = (Main.GameUpdateCount * 0.02f) % 1f;
                DynamicSkyboxSystem.TriggerFlash(Main.hslToRgb(hue, 1f, 0.9f), 1f * intensity);
            }
            
            WeaponFogVFX.SpawnAttackFog(center, "SwanLake", 1.2f * intensity, Vector2.Zero);
        }
        
        /// <summary>
        /// Fractal laser sweep effect.
        /// </summary>
        public static void SwanLakeFractalLaser(Vector2 origin, float rotation, float length, float intensity = 1f)
        {
            Vector2 direction = rotation.ToRotationVector2();
            
            // Laser core particles along beam
            int beamPoints = (int)(length / 20f);
            for (int i = 0; i < beamPoints; i++)
            {
                float t = (float)i / beamPoints;
                Vector2 beamPos = origin + direction * length * t;
                
                // Alternating black/white
                bool isWhite = i % 2 == 0;
                CustomParticles.GenericFlare(beamPos, isWhite ? Color.White : new Color(30, 30, 40), 
                    0.3f * intensity * (1f - t * 0.5f), 10);
                
                // Rainbow sparkle edge
                if (Main.rand.NextBool(3))
                {
                    float hue = (t + Main.GameUpdateCount * 0.02f) % 1f;
                    Vector2 edgeOffset = direction.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-15f, 15f);
                    CustomParticles.PrismaticSparkle(beamPos + edgeOffset, Main.hslToRgb(hue, 1f, 0.8f), 0.25f);
                }
            }
            
            // Feathers along beam
            if (Main.rand.NextBool(3))
            {
                Vector2 featherPos = origin + direction * length * Main.rand.NextFloat();
                CustomParticles.SwanFeatherDrift(featherPos, Main.rand.NextBool() ? Color.White : new Color(20, 20, 30), 0.4f);
            }
        }
        
        #endregion
        
        #region La Campanella - Chime of Life
        
        /// <summary>
        /// Bell toll impact - resonating shockwave with fire.
        /// </summary>
        public static void LaCampanellaBellToll(Vector2 center, int tollNumber, float intensity = 1f)
        {
            // Core flash - infernal orange
            CustomParticles.GenericFlare(center, Color.White, 1.3f * intensity, 22);
            CustomParticles.GenericFlare(center, new Color(255, 140, 40), 1f * intensity, 25);
            CustomParticles.GenericFlare(center, new Color(255, 100, 0), 0.7f * intensity, 28);
            
            // Resonance halos - more rings for higher toll numbers
            int ringCount = 4 + tollNumber;
            for (int ring = 0; ring < ringCount; ring++)
            {
                float ringProgress = (float)ring / ringCount;
                Color ringColor = Color.Lerp(new Color(255, 140, 40), new Color(200, 50, 30), ringProgress);
                CustomParticles.HaloRing(center, ringColor, 0.4f * intensity + ring * 0.15f, 16 + ring * 4);
            }
            
            // Fire burst
            int emberCount = 10 + tollNumber * 3;
            for (int i = 0; i < emberCount; i++)
            {
                float angle = MathHelper.TwoPi * i / emberCount;
                Vector2 emberVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 12f);
                
                Dust ember = Dust.NewDustPerfect(center, DustID.Torch, emberVel, 0, 
                    new Color(255, 100 + Main.rand.Next(100), 0), 1.5f);
                ember.noGravity = true;
            }
            
            // Smoke puff
            for (int i = 0; i < 5 + tollNumber; i++)
            {
                Vector2 smokeVel = Main.rand.NextVector2Circular(3f, 3f) + new Vector2(0, -2f);
                Dust smoke = Dust.NewDustPerfect(center + Main.rand.NextVector2Circular(20f, 20f), DustID.Smoke, smokeVel, 100, 
                    new Color(50, 40, 40), 1.5f);
                smoke.noGravity = false;
            }
            
            // Heavy fog
            WeaponFogVFX.SpawnAttackFog(center, "LaCampanella", 1f * intensity + tollNumber * 0.2f, Vector2.Zero);
            LightBeamImpactVFX.SpawnImpact(center, "LaCampanella", 1f * intensity);
        }
        
        /// <summary>
        /// Infernal judgment attack signature VFX.
        /// </summary>
        public static void LaCampanellaInfernalJudgment(Vector2 center, int wave, int totalWaves, float intensity = 1f)
        {
            float waveProgress = (float)wave / totalWaves;
            
            // Massive infernal flash
            CustomParticles.GenericFlare(center, Color.White, 1.8f * intensity * (1f + waveProgress * 0.4f), 25);
            CustomParticles.GenericFlare(center, new Color(255, 100, 0), 1.4f * intensity, 28);
            
            // Heavy smoke ring
            int smokeCount = 15 + wave * 5;
            for (int i = 0; i < smokeCount; i++)
            {
                float angle = MathHelper.TwoPi * i / smokeCount;
                Vector2 smokeVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f) + new Vector2(0, -1f);
                
                Dust smoke = Dust.NewDustPerfect(center + smokeVel * 5f, DustID.Smoke, smokeVel * 0.5f, 150, 
                    new Color(30, 20, 25), 2f);
                smoke.noGravity = false;
            }
            
            // Fire halos
            int ringCount = 8 + wave * 2;
            for (int ring = 0; ring < ringCount; ring++)
            {
                float ringProgress = (float)ring / ringCount;
                Color ringColor = Color.Lerp(new Color(255, 140, 40), new Color(200, 50, 30), ringProgress);
                ringColor = Color.Lerp(ringColor, new Color(30, 20, 25), ringProgress * 0.3f);
                CustomParticles.HaloRing(center, ringColor, 0.4f * intensity + ring * 0.12f, 20 + ring * 2);
            }
            
            // Ember explosion
            int emberCount = 20 + wave * 8;
            for (int i = 0; i < emberCount; i++)
            {
                float angle = MathHelper.TwoPi * i / emberCount;
                Vector2 emberVel = angle.ToRotationVector2() * (10f + wave * 3f + Main.rand.NextFloat(-2f, 2f));
                
                Dust ember = Dust.NewDustPerfect(center, DustID.Torch, emberVel, 0, 
                    Color.Lerp(new Color(255, 140, 40), new Color(255, 200, 50), Main.rand.NextFloat()), 1.8f);
                ember.noGravity = true;
            }
            
            // Sky flash on final wave
            if (wave == totalWaves)
            {
                DynamicSkyboxSystem.TriggerFlash(new Color(255, 140, 40), 1.2f * intensity);
            }
            
            WeaponFogVFX.SpawnAttackFog(center, "LaCampanella", 1.5f * intensity, Vector2.Zero);
        }
        
        #endregion
        
        #region Enigma - The Hollow Mystery
        
        /// <summary>
        /// Void gaze attack - eyes watching and firing.
        /// </summary>
        public static void EnigmaVoidGaze(Vector2 center, Vector2 targetDirection, float intensity = 1f)
        {
            // Void core
            CustomParticles.GenericFlare(center, new Color(50, 220, 100), 0.9f * intensity, 20);
            CustomParticles.GenericFlare(center, new Color(140, 60, 200), 0.6f * intensity, 22);
            
            // Eye glyphs watching target
            for (int i = 0; i < 4; i++)
            {
                float angle = targetDirection.ToRotation() + MathHelper.PiOver4 * (i - 1.5f);
                Vector2 eyePos = center + angle.ToRotationVector2() * 40f;
                CustomParticles.Glyph(eyePos, new Color(140, 60, 200), 0.4f * intensity, 8 + i % 4); // Eye variants
            }
            
            // Void particles flowing toward target
            for (int i = 0; i < 8; i++)
            {
                float spread = Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 particlePos = center + targetDirection.RotatedBy(spread) * Main.rand.NextFloat(20f, 60f);
                Vector2 particleVel = targetDirection * Main.rand.NextFloat(3f, 8f);
                
                CustomParticles.GenericFlare(particlePos, new Color(50, 220, 100) * 0.6f, 0.25f, 15);
            }
            
            // Void halos
            CustomParticles.HaloRing(center, new Color(140, 60, 200), 0.4f * intensity, 18);
            CustomParticles.HaloRing(center, new Color(50, 220, 100), 0.3f * intensity, 20);
            
            WeaponFogVFX.SpawnAttackFog(center, "EnigmaVariations", 0.7f * intensity, targetDirection);
        }
        
        /// <summary>
        /// Paradox judgment signature attack.
        /// </summary>
        public static void EnigmaParadoxJudgment(Vector2 center, int wave, int totalWaves, float intensity = 1f)
        {
            float waveProgress = (float)wave / totalWaves;
            
            // Void flash
            CustomParticles.GenericFlare(center, new Color(50, 220, 100), 1.5f * intensity, 25);
            CustomParticles.GenericFlare(center, new Color(140, 60, 200), 1.1f * intensity, 28);
            
            // Eye formation
            int eyeCount = 6 + wave * 2;
            for (int i = 0; i < eyeCount; i++)
            {
                float angle = MathHelper.TwoPi * i / eyeCount + Main.GameUpdateCount * 0.01f * (wave % 2 == 0 ? 1 : -1);
                float radius = 60f + wave * 15f;
                Vector2 eyePos = center + angle.ToRotationVector2() * radius;
                
                CustomParticles.Glyph(eyePos, new Color(140, 60, 200) * (0.6f + waveProgress * 0.4f), 0.5f * intensity, (i + 8) % 12);
            }
            
            // Glyph burst
            int glyphCount = 8 + wave * 3;
            for (int i = 0; i < glyphCount; i++)
            {
                float angle = MathHelper.TwoPi * i / glyphCount;
                Vector2 glyphVel = angle.ToRotationVector2() * (8f + wave * 2f);
                
                CustomParticles.Glyph(center + glyphVel * 3f, new Color(50, 220, 100), 0.4f, i % 12);
            }
            
            // Mystery halos
            int ringCount = 6 + wave * 2;
            for (int ring = 0; ring < ringCount; ring++)
            {
                float ringProgress = (float)ring / ringCount;
                Color ringColor = Color.Lerp(new Color(140, 60, 200), new Color(50, 220, 100), ringProgress);
                CustomParticles.HaloRing(center, ringColor, 0.35f * intensity + ring * 0.12f, 18 + ring * 3);
            }
            
            WeaponFogVFX.SpawnAttackFog(center, "EnigmaVariations", 1.3f * intensity, Vector2.Zero);
        }
        
        #endregion
        
        #region Dies Irae - Herald of Judgment
        
        /// <summary>
        /// Wrath strike - divine judgment fire.
        /// </summary>
        public static void DiesIraeWrathStrike(Vector2 position, Vector2 direction, float intensity = 1f)
        {
            // Wrathful core
            CustomParticles.GenericFlare(position, Color.White, 1.2f * intensity, 20);
            CustomParticles.GenericFlare(position, new Color(200, 30, 30), 0.9f * intensity, 22);
            CustomParticles.GenericFlare(position, new Color(150, 20, 20), 0.6f * intensity, 24);
            
            // Judgment flames in direction
            for (int i = 0; i < (int)(12 * intensity); i++)
            {
                float angle = direction.ToRotation() + Main.rand.NextFloat(-0.6f, 0.6f);
                Vector2 flameVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 12f);
                
                Dust flame = Dust.NewDustPerfect(position, DustID.Torch, flameVel, 0, 
                    new Color(200, 30 + Main.rand.Next(50), 30), 1.5f);
                flame.noGravity = true;
            }
            
            // Wrath halos
            CustomParticles.HaloRing(position, new Color(200, 30, 30), 0.4f * intensity, 18);
            CustomParticles.HaloRing(position, new Color(150, 20, 20), 0.3f * intensity, 20);
            
            WeaponFogVFX.SpawnAttackFog(position, "DiesIrae", 0.9f * intensity, direction);
            LightBeamImpactVFX.SpawnImpact(position, "DiesIrae", 1f * intensity);
        }
        
        /// <summary>
        /// Day of Wrath judgment attack.
        /// </summary>
        public static void DiesIraeDayOfWrath(Vector2 center, int wave, int totalWaves, float intensity = 1f)
        {
            float waveProgress = (float)wave / totalWaves;
            
            // Divine wrath flash
            CustomParticles.GenericFlare(center, Color.White, 2f * intensity, 25);
            CustomParticles.GenericFlare(center, new Color(200, 30, 30), 1.5f * intensity, 28);
            
            // Judgment fire halos
            int ringCount = 10 + wave * 2;
            for (int ring = 0; ring < ringCount; ring++)
            {
                float ringProgress = (float)ring / ringCount;
                Color ringColor = Color.Lerp(new Color(200, 30, 30), new Color(100, 15, 15), ringProgress);
                CustomParticles.HaloRing(center, ringColor, 0.45f * intensity + ring * 0.12f, 20 + ring * 2);
            }
            
            // Flame explosion
            int flameCount = 25 + wave * 8;
            for (int i = 0; i < flameCount; i++)
            {
                float angle = MathHelper.TwoPi * i / flameCount;
                Vector2 flameVel = angle.ToRotationVector2() * (12f + wave * 3f);
                
                Dust flame = Dust.NewDustPerfect(center, DustID.Torch, flameVel, 0, 
                    Color.Lerp(new Color(255, 50, 30), new Color(200, 30, 30), Main.rand.NextFloat()), 2f);
                flame.noGravity = true;
            }
            
            // Sky flash
            if (wave == totalWaves)
            {
                DynamicSkyboxSystem.TriggerFlash(new Color(200, 30, 30), 1.3f * intensity);
            }
            
            WeaponFogVFX.SpawnAttackFog(center, "DiesIrae", 1.5f * intensity, Vector2.Zero);
        }
        
        #endregion
        
        #region Moonlight Sonata - Lunar Effects
        
        /// <summary>
        /// Lunar beam strike.
        /// </summary>
        public static void MoonlightLunarStrike(Vector2 position, Vector2 direction, float intensity = 1f)
        {
            // Lunar core
            CustomParticles.GenericFlare(position, Color.White, 1f * intensity, 20);
            CustomParticles.GenericFlare(position, new Color(135, 206, 250), 0.7f * intensity, 22);
            CustomParticles.GenericFlare(position, new Color(75, 0, 130), 0.5f * intensity, 24);
            
            // Ethereal mist
            for (int i = 0; i < 8; i++)
            {
                Vector2 mistPos = position + Main.rand.NextVector2Circular(25f, 25f);
                Dust mist = Dust.NewDustPerfect(mistPos, DustID.PurpleTorch, 
                    direction * 0.5f + Main.rand.NextVector2Circular(1f, 1f), 100, new Color(75, 0, 130), 0.8f);
                mist.noGravity = true;
                mist.fadeIn = 1.2f;
            }
            
            // Lunar halos
            CustomParticles.HaloRing(position, new Color(135, 206, 250), 0.4f * intensity, 18);
            CustomParticles.HaloRing(position, new Color(75, 0, 130), 0.3f * intensity, 20);
            
            WeaponFogVFX.SpawnAttackFog(position, "MoonlightSonata", 0.8f * intensity, direction);
            LightBeamImpactVFX.SpawnImpact(position, "MoonlightSonata", 0.9f * intensity);
        }
        
        /// <summary>
        /// Moonlight sonata signature attack.
        /// </summary>
        public static void MoonlightSonataSignature(Vector2 center, int wave, int totalWaves, float intensity = 1f)
        {
            float waveProgress = (float)wave / totalWaves;
            
            // Lunar flash
            CustomParticles.GenericFlare(center, Color.White, 1.5f * intensity, 25);
            CustomParticles.GenericFlare(center, new Color(135, 206, 250), 1.1f * intensity, 28);
            CustomParticles.GenericFlare(center, new Color(75, 0, 130), 0.8f * intensity, 30);
            
            // Ethereal halos
            int ringCount = 8 + wave * 2;
            for (int ring = 0; ring < ringCount; ring++)
            {
                float ringProgress = (float)ring / ringCount;
                Color ringColor = Color.Lerp(new Color(135, 206, 250), new Color(75, 0, 130), ringProgress);
                CustomParticles.HaloRing(center, ringColor, 0.35f * intensity + ring * 0.1f, 20 + ring * 2);
            }
            
            // Lunar mist burst
            int mistCount = 15 + wave * 5;
            for (int i = 0; i < mistCount; i++)
            {
                float angle = MathHelper.TwoPi * i / mistCount;
                Vector2 mistVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 10f);
                
                Dust mist = Dust.NewDustPerfect(center, DustID.PurpleTorch, mistVel, 100, 
                    Color.Lerp(new Color(75, 0, 130), new Color(135, 206, 250), Main.rand.NextFloat()), 1.2f);
                mist.noGravity = true;
                mist.fadeIn = 1.3f;
            }
            
            // Silver sparkle ring
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f + Main.GameUpdateCount * 0.02f;
                Vector2 sparklePos = center + angle.ToRotationVector2() * 50f;
                CustomParticles.GenericFlare(sparklePos, new Color(220, 220, 235), 0.3f, 15);
            }
            
            WeaponFogVFX.SpawnAttackFog(center, "MoonlightSonata", 1.2f * intensity, Vector2.Zero);
        }
        
        #endregion
        
        #region Seasonal Bosses
        
        /// <summary>
        /// Spring (Primavera) bloom attack.
        /// </summary>
        public static void SpringBloomBurst(Vector2 center, float intensity = 1f)
        {
            // Soft pink/green core
            CustomParticles.GenericFlare(center, Color.White, 1f * intensity, 20);
            CustomParticles.GenericFlare(center, new Color(255, 180, 200), 0.8f * intensity, 22);
            CustomParticles.GenericFlare(center, new Color(150, 255, 150), 0.6f * intensity, 24);
            
            // Petal burst
            for (int i = 0; i < 15; i++)
            {
                float angle = MathHelper.TwoPi * i / 15f;
                Vector2 petalVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 10f);
                
                Dust petal = Dust.NewDustPerfect(center, DustID.PinkTorch, petalVel, 0, 
                    new Color(255, 180 + Main.rand.Next(50), 200), 1.2f);
                petal.noGravity = true;
            }
            
            // Spring halos
            CustomParticles.HaloRing(center, new Color(255, 180, 200), 0.4f * intensity, 18);
            CustomParticles.HaloRing(center, new Color(150, 255, 150), 0.3f * intensity, 20);
            
            WeaponFogVFX.SpawnAttackFog(center, "Spring", 1f * intensity, Vector2.Zero);
        }
        
        /// <summary>
        /// Summer (L'Estate) heat wave.
        /// </summary>
        public static void SummerHeatWave(Vector2 center, float intensity = 1f)
        {
            // Hot orange/gold core
            CustomParticles.GenericFlare(center, Color.White, 1.2f * intensity, 20);
            CustomParticles.GenericFlare(center, new Color(255, 180, 50), 0.9f * intensity, 22);
            CustomParticles.GenericFlare(center, new Color(255, 140, 50), 0.7f * intensity, 24);
            
            // Heat waves
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 heatVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 12f);
                
                Dust heat = Dust.NewDustPerfect(center, DustID.Torch, heatVel, 0, 
                    new Color(255, 140 + Main.rand.Next(100), 50), 1.5f);
                heat.noGravity = true;
            }
            
            // Summer halos
            CustomParticles.HaloRing(center, new Color(255, 180, 50), 0.4f * intensity, 18);
            CustomParticles.HaloRing(center, new Color(255, 140, 50), 0.3f * intensity, 20);
            
            WeaponFogVFX.SpawnAttackFog(center, "Summer", 1f * intensity, Vector2.Zero);
        }
        
        /// <summary>
        /// Autumn (L'Autunno) leaf storm.
        /// </summary>
        public static void AutumnLeafStorm(Vector2 center, float intensity = 1f)
        {
            // Amber/crimson core
            CustomParticles.GenericFlare(center, Color.White, 1f * intensity, 20);
            CustomParticles.GenericFlare(center, new Color(200, 150, 80), 0.8f * intensity, 22);
            CustomParticles.GenericFlare(center, new Color(180, 80, 50), 0.6f * intensity, 24);
            
            // Leaf swirl
            for (int i = 0; i < 18; i++)
            {
                float angle = MathHelper.TwoPi * i / 18f + Main.GameUpdateCount * 0.03f;
                Vector2 leafVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 10f);
                
                Color leafColor = Main.rand.NextBool() ? new Color(200, 150, 80) : new Color(180, 80, 50);
                Dust leaf = Dust.NewDustPerfect(center, DustID.Torch, leafVel, 0, leafColor, 1.3f);
                leaf.noGravity = false; // Leaves fall
            }
            
            // Autumn halos
            CustomParticles.HaloRing(center, new Color(200, 150, 80), 0.4f * intensity, 18);
            CustomParticles.HaloRing(center, new Color(180, 80, 50), 0.3f * intensity, 20);
            
            WeaponFogVFX.SpawnAttackFog(center, "Autumn", 1f * intensity, Vector2.Zero);
        }
        
        /// <summary>
        /// Winter (L'Inverno) frost burst.
        /// </summary>
        public static void WinterFrostBurst(Vector2 center, float intensity = 1f)
        {
            // Ice blue/white core
            CustomParticles.GenericFlare(center, Color.White, 1.2f * intensity, 20);
            CustomParticles.GenericFlare(center, new Color(150, 200, 255), 0.9f * intensity, 22);
            CustomParticles.GenericFlare(center, new Color(100, 150, 255), 0.7f * intensity, 24);
            
            // Ice crystal burst
            for (int i = 0; i < 15; i++)
            {
                float angle = MathHelper.TwoPi * i / 15f;
                Vector2 iceVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 12f);
                
                Dust ice = Dust.NewDustPerfect(center, DustID.Frost, iceVel, 0, 
                    new Color(150, 200, 255), 1.4f);
                ice.noGravity = true;
            }
            
            // Winter halos
            CustomParticles.HaloRing(center, new Color(150, 200, 255), 0.4f * intensity, 18);
            CustomParticles.HaloRing(center, Color.White, 0.3f * intensity, 20);
            
            WeaponFogVFX.SpawnAttackFog(center, "Winter", 1f * intensity, Vector2.Zero);
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// Generic boss attack release burst - use when specific boss VFX not available.
        /// </summary>
        public static void GenericAttackRelease(Vector2 center, string theme, float intensity = 1f)
        {
            var style = UniqueWeaponVFXStyles.GetStyle(theme);
            
            // Core flash
            CustomParticles.GenericFlare(center, Color.White, 1.2f * intensity, 20);
            CustomParticles.GenericFlare(center, style.Fog.PrimaryColor, 0.9f * intensity, 22);
            CustomParticles.GenericFlare(center, style.Fog.SecondaryColor, 0.6f * intensity, 24);
            
            // Halos
            for (int ring = 0; ring < 5; ring++)
            {
                Color ringColor = Color.Lerp(style.Fog.PrimaryColor, style.Fog.SecondaryColor, ring / 5f);
                CustomParticles.HaloRing(center, ringColor, 0.3f * intensity + ring * 0.1f, 16 + ring * 2);
            }
            
            // Fog and beam
            WeaponFogVFX.SpawnAttackFog(center, theme, 1f * intensity, Vector2.Zero);
            LightBeamImpactVFX.SpawnImpact(center, theme, 0.9f * intensity);
        }
        
        /// <summary>
        /// Generic boss death explosion.
        /// </summary>
        public static void GenericBossDeathExplosion(Vector2 center, string theme, float intensity = 1f)
        {
            var style = UniqueWeaponVFXStyles.GetStyle(theme);
            
            // Massive core flash
            CustomParticles.GenericFlare(center, Color.White, 2.5f * intensity, 30);
            CustomParticles.GenericFlare(center, style.Fog.PrimaryColor, 2f * intensity, 35);
            CustomParticles.GenericFlare(center, style.Fog.SecondaryColor, 1.5f * intensity, 40);
            
            // Many halos
            for (int ring = 0; ring < 15; ring++)
            {
                Color ringColor = Color.Lerp(style.Fog.PrimaryColor, style.Fog.SecondaryColor, ring / 15f);
                CustomParticles.HaloRing(center, ringColor, 0.5f * intensity + ring * 0.18f, 25 + ring * 3);
            }
            
            // Particle explosion
            for (int i = 0; i < 50; i++)
            {
                float angle = MathHelper.TwoPi * i / 50f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(8f, 20f);
                Color particleColor = Color.Lerp(style.Fog.PrimaryColor, style.Fog.SecondaryColor, Main.rand.NextFloat());
                
                CustomParticles.GenericFlare(center + vel * 3f, particleColor, 0.5f + Main.rand.NextFloat(0.3f), 30);
            }
            
            // Major fog and beam
            WeaponFogVFX.SpawnAttackFog(center, theme, 3f * intensity, Vector2.Zero);
            LightBeamImpactVFX.SpawnImpact(center, theme, 3f * intensity);
            
            // Sky flash
            DynamicSkyboxSystem.TriggerFlash(style.Fog.PrimaryColor, 1.5f * intensity);
            
            // Screen shake
            if (Main.LocalPlayer.Distance(center) < 2000f)
            {
                Main.LocalPlayer.GetModPlayer<MagnumScreenShakePlayer>()?.AddShake(25f * intensity, 50);
            }
        }
        
        #endregion
    }
}
