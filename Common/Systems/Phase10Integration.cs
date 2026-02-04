using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// PHASE 10: INTEGRATED MUSICAL VFX SYSTEM
    /// 
    /// This class provides easy integration of Phase 10 musical effects
    /// into existing boss attacks and weapon effects.
    /// 
    /// Usage:
    /// - Call the appropriate method for your boss theme
    /// - Methods enhance existing attacks with musical VFX
    /// - Does NOT replace existing logic, only ADDS visual enhancement
    /// </summary>
    public static class Phase10Integration
    {
        #region Eroica Theme Integration (Heroic, Triumphant)
        
        /// <summary>
        /// Enhances Eroica boss attacks with heroic musical motifs
        /// </summary>
        public static class Eroica
        {
            private static readonly Color EroicaGold = new Color(255, 200, 80);
            private static readonly Color EroicaScarlet = new Color(200, 50, 50);
            private static readonly Color SakuraPink = new Color(255, 150, 180);
            
            /// <summary>
            /// Add heroic fanfare VFX to dash attacks
            /// Call during dash execution phase
            /// </summary>
            public static void DashFanfare(Vector2 position, Vector2 velocity)
            {
                float time = Main.GameUpdateCount * 0.1f;
                
                // Heroic brass-like flares trailing behind
                if (Main.rand.NextBool(2))
                {
                    Vector2 trailPos = position - velocity.SafeNormalize(Vector2.Zero) * 30f;
                    Color fanfareColor = Color.Lerp(EroicaGold, Color.White, (float)Math.Sin(time) * 0.3f + 0.3f);
                    
                    var flare = new GenericGlowParticle(
                        trailPos + Main.rand.NextVector2Circular(10f, 10f),
                        -velocity * 0.1f,
                        fanfareColor,
                        0.4f,
                        15,
                        true
                    );
                    MagnumParticleHandler.SpawnParticle(flare);
                }
                
                // Triumphant sparkle cascade
                if (Main.rand.NextBool(3))
                {
                    var sparkle = new SparkleParticle(
                        position + Main.rand.NextVector2Circular(20f, 20f),
                        velocity * -0.2f + Main.rand.NextVector2Circular(2f, 2f),
                        EroicaGold,
                        0.35f,
                        20
                    );
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
                
                // Music note accents (like brass notes)
                if (Main.rand.NextBool(6))
                {
                    ThemedParticles.EroicaMusicNotes(position + Main.rand.NextVector2Circular(25f, 25f), 1, 15f);
                }
            }
            
            /// <summary>
            /// Add heroic impact VFX to attacks hitting enemies
            /// Call when attack makes contact
            /// </summary>
            public static void HeroicImpact(Vector2 position, float intensity = 1f)
            {
                // Chord resolution burst
                Phase10BossVFX.ChordResolutionBloom(position, new[] { EroicaGold, EroicaScarlet, SakuraPink }, intensity);
                
                // Heroic cymbal crash
                if (intensity >= 0.7f)
                {
                    Phase10BossVFX.CymbalCrashBurst(position, intensity);
                }
                
                // Triumphant note scatter
                for (int i = 0; i < (int)(4 * intensity); i++)
                {
                    Vector2 noteVel = Main.rand.NextVector2Circular(4f, 4f);
                    ThemedParticles.EroicaMusicNotes(position + noteVel * 5f, 1, 10f);
                }
            }
            
            /// <summary>
            /// Add march-like rhythm to barrage attacks
            /// Call during projectile spawning
            /// </summary>
            public static void MarchingBarrage(Vector2 position, int timer)
            {
                // Strong beat accent every 12 frames (like 4/4 march)
                bool isStrongBeat = timer % 12 < 3;
                
                if (isStrongBeat)
                {
                    // Timpani-like pulse
                    Phase10BossVFX.TimpaniDrumrollImpact(position, EroicaScarlet, 0.6f);
                    
                    // Music staff lines emanating
                    Phase10BossVFX.StaffLineConvergence(position, EroicaGold, 0.5f, 60f);
                }
            }
            
            /// <summary>
            /// Add sakura crescendo to ultimate attacks
            /// Call during charge-up phase
            /// </summary>
            public static void SakuraCrescendo(Vector2 position, float chargeProgress)
            {
                // Crescendo danger rings
                Phase10BossVFX.CrescendoDangerRings(position, SakuraPink, chargeProgress, 3);
                
                // Accelerating petal spiral
                if (chargeProgress > 0.3f)
                {
                    int petalCount = (int)(chargeProgress * 8);
                    ThemedParticles.SakuraPetals(position, petalCount, 40f + chargeProgress * 60f);
                }
                
                // Dynamic marking (pp to ff) glow
                Phase10BossVFX.DynamicsWave(position, chargeProgress, EroicaGold);
            }
            
            /// <summary>
            /// Hero's Judgment - Signature spectacle attack enhancement
            /// </summary>
            public static void HeroesJudgmentVFX(Vector2 position, float attackProgress, int waveNumber)
            {
                // Fortissimo flash at each wave
                if (attackProgress < 0.1f && waveNumber > 0)
                {
                    Phase10BossVFX.FortissimoFlashWarning(position, EroicaGold, 1f);
                }
                
                // Tutti ensemble burst for final wave
                if (waveNumber >= 3)
                {
                    Phase10BossVFX.TuttiFullEnsemble(position, new[] { EroicaGold, EroicaScarlet, Color.White }, 1.2f);
                }
                
                // Harmonic overtone ripples
                if (attackProgress > 0.5f)
                {
                    Phase10BossVFX.HarmonicOvertoneBeam(
                        position,
                        position + new Vector2(0, 200f),
                        EroicaGold
                    );
                }
            }
        }
        
        #endregion
        
        #region Swan Lake Theme Integration (Graceful, Prismatic)
        
        public static class SwanLake
        {
            private static readonly Color SwanWhite = new Color(255, 255, 255);
            private static readonly Color SwanBlack = new Color(30, 30, 40);
            
            /// <summary>
            /// Add ballet-like grace to movement
            /// </summary>
            public static void BalletGrace(Vector2 position, Vector2 velocity, int timer)
            {
                // Waltz-time movement accents (3/4)
                int beatInMeasure = (timer % 18) / 6;
                
                if (beatInMeasure == 0)
                {
                    // Strong downbeat - feather flourish
                    ThemedParticles.SwanFeatherBurst(position, 3, 25f);
                }
                
                // Legato wave following movement
                if (velocity.Length() > 1f)
                {
                    Phase10BossVFX.LegatoWaveWash(position, velocity.SafeNormalize(Vector2.Zero), 
                        SwanWhite, velocity.Length() * 5f);
                }
                
                // Prismatic sparkle trail
                if (Main.rand.NextBool(3))
                {
                    Color rainbow = Main.hslToRgb((timer * 0.01f) % 1f, 0.8f, 0.8f);
                    CustomParticles.PrismaticSparkle(position + Main.rand.NextVector2Circular(15f, 15f), rainbow, 0.3f);
                }
            }
            
            /// <summary>
            /// Add dying swan melancholy to low HP phase
            /// </summary>
            public static void DyingSwanMelancholy(Vector2 position, float hpPercent)
            {
                float melancholy = 1f - hpPercent;
                
                // Ritardando - slowing, fading notes
                Phase10BossVFX.RubatoBreath(position, SwanWhite, melancholy * 0.8f);
                
                // Diminuendo visual - dimming particles
                if (Main.rand.NextFloat() < melancholy * 0.3f)
                {
                    var dimFeather = new GenericGlowParticle(
                        position + Main.rand.NextVector2Circular(40f, 40f),
                        new Vector2(0, -1f),
                        SwanWhite * (1f - melancholy * 0.5f),
                        0.3f,
                        30,
                        true
                    );
                    MagnumParticleHandler.SpawnParticle(dimFeather);
                }
                
                // Fermata hold moments
                if (Main.rand.NextBool(20))
                {
                    Phase10BossVFX.FermataHoldIndicator(position, SwanBlack, melancholy);
                }
            }
            
            /// <summary>
            /// Add chromatic surge to prismatic attacks
            /// </summary>
            public static void ChromaticSurge(Vector2 position, float progress)
            {
                // Rainbow scale cascade
                int noteCount = (int)(progress * 12);
                for (int i = 0; i < noteCount; i++)
                {
                    float hue = (float)i / 12f;
                    Color noteColor = Main.hslToRgb(hue, 1f, 0.75f);
                    
                    float angle = MathHelper.TwoPi * i / noteCount + Main.GameUpdateCount * 0.02f;
                    Vector2 notePos = position + angle.ToRotationVector2() * (30f + progress * 50f);
                    
                    CustomParticles.GenericFlare(notePos, noteColor, 0.25f + progress * 0.15f, 12);
                }
                
                // Glissando slide
                Phase10BossVFX.GlissandoSlideWarning(position, position + new Vector2(100f, 0), 
                    SwanWhite, progress);
            }
            
            /// <summary>
            /// Monochromatic Apocalypse - Ultimate attack enhancement
            /// </summary>
            public static void MonochromaticApocalypseVFX(Vector2 position, float rotationAngle, float intensity)
            {
                // Counterpoint duality - black and white interweaving
                Phase10BossVFX.CounterpointDuality(position, SwanWhite, SwanBlack, rotationAngle);
                
                // Fugue-like interlaced patterns
                Phase10BossVFX.FugueInterlace(position, new[] { SwanWhite, SwanBlack, SwanWhite }, rotationAngle * 0.5f);
                
                // Coda finale at full intensity
                if (intensity >= 1f)
                {
                    Phase10BossVFX.CodaFinale(position, SwanWhite, SwanBlack, 1.5f);
                }
            }
        }
        
        #endregion
        
        #region Enigma Theme Integration (Mysterious, Void)
        
        public static class Enigma
        {
            private static readonly Color EnigmaPurple = new Color(140, 60, 200);
            private static readonly Color EnigmaGreen = new Color(50, 220, 100);
            private static readonly Color EnigmaVoid = new Color(15, 10, 20);
            
            /// <summary>
            /// Add mysterious trill effects to void attacks
            /// </summary>
            public static void MysteriousTrill(Vector2 position, int timer)
            {
                // Trill vibration between two mystery notes
                Phase10BossVFX.TrillVibrationWarning(position, EnigmaPurple, EnigmaGreen, 
                    (float)(timer % 60) / 60f);
                
                // Enigma eye watches ominously
                if (Main.rand.NextBool(8))
                {
                    CustomParticles.EnigmaEyeGaze(position + Main.rand.NextVector2Circular(30f, 30f),
                        EnigmaPurple, 0.4f);
                }
            }
            
            /// <summary>
            /// Add syncopated surprise to paradox attacks
            /// </summary>
            public static void ParadoxSyncopation(Vector2 position, float bpm = 120f)
            {
                Phase10BossVFX.SyncopationStutter(position, EnigmaPurple, bpm);
                
                // Dissonance storm during off-beats
                Phase10BossVFX.DissonanceStorm(position, 50f, EnigmaPurple, EnigmaGreen);
            }
            
            /// <summary>
            /// Add void harmonic overtones
            /// </summary>
            public static void VoidHarmonics(Vector2 start, Vector2 end)
            {
                Phase10BossVFX.HarmonicOvertoneBeam(start, end, EnigmaPurple);
                
                // Glyph accents along the harmonic
                int glyphCount = (int)(Vector2.Distance(start, end) / 40f);
                for (int i = 0; i < glyphCount; i++)
                {
                    Vector2 glyphPos = Vector2.Lerp(start, end, (float)i / glyphCount);
                    CustomParticles.Glyph(glyphPos, EnigmaPurple, 0.3f, Main.rand.Next(1, 13));
                }
            }
            
            /// <summary>
            /// Paradox Judgment - Signature attack enhancement
            /// </summary>
            public static void ParadoxJudgmentVFX(Vector2 position, float progress)
            {
                // Tempo distortion effect - shifting BPM as paradox intensifies
                Phase10BossVFX.TempoShiftDistortion(position, 120f, 120f * (1f + progress * 0.5f), 60f);
                
                // Key change flash as paradox resolves
                if (progress > 0.8f)
                {
                    Phase10BossVFX.KeyChangeFlash(position, EnigmaPurple, EnigmaGreen, progress);
                }
                
                // Cadence finisher
                Color[] paradoxColors = { EnigmaPurple, EnigmaGreen, EnigmaVoid, Color.White };
                Phase10BossVFX.CadenceFinisher(position, paradoxColors, progress);
            }
        }
        
        #endregion
        
        #region La Campanella Theme Integration (Infernal, Bell-like)
        
        public static class LaCampanella
        {
            private static readonly Color CampanellaOrange = new Color(255, 140, 40);
            private static readonly Color CampanellaGold = new Color(255, 200, 80);
            private static readonly Color CampanellaBlack = new Color(30, 20, 25);
            
            /// <summary>
            /// Add bell chime resonance to attacks
            /// </summary>
            public static void BellChimeResonance(Vector2 position, int timer)
            {
                // Bell toll rhythm (slower, resonant)
                bool isBellToll = timer % 30 == 0;
                
                if (isBellToll)
                {
                    // Pizzicato pop like bell strike
                    Phase10BossVFX.PizzicatoPop(position, CampanellaGold);
                    
                    // Expanding resonance rings
                    Phase10BossVFX.CrescendoRing(position, ((timer / 30f) % 1f) * 60f, 60f, CampanellaOrange);
                }
                
                // Smoky undertone
                if (Main.rand.NextBool(4))
                {
                    var smoke = new HeavySmokeParticle(
                        position + Main.rand.NextVector2Circular(20f, 20f),
                        new Vector2(Main.rand.NextFloat(-1f, 1f), -2f),
                        CampanellaBlack,
                        25,
                        0.3f,
                        0.5f,
                        0.01f,
                        false
                    );
                    MagnumParticleHandler.SpawnParticle(smoke);
                }
            }
            
            /// <summary>
            /// Add infernal tremolo to fire attacks
            /// </summary>
            public static void InfernalTremolo(Vector2 position)
            {
                // Rapid bell-like tremolo
                Phase10BossVFX.TrillVibrationWarning(position, CampanellaOrange, CampanellaGold, 
                    (Main.GameUpdateCount * 0.1f) % 1f);
                
                // Fire glow underneath
                Lighting.AddLight(position, CampanellaOrange.ToVector3() * 1.2f);
            }
            
            /// <summary>
            /// Infernal Judgment - Signature attack enhancement
            /// </summary>
            public static void InfernalJudgmentVFX(Vector2 position, float chargeProgress)
            {
                // Fortissimo warning flash
                if (chargeProgress > 0.9f)
                {
                    Phase10BossVFX.FortissimoFlashWarning(position, CampanellaOrange, 1f);
                }
                
                // Sforzando spike at release
                Phase10BossVFX.SforzandoSpike(position, CampanellaOrange, chargeProgress);
                
                // Bell-shaped crescendo rings
                Phase10BossVFX.CrescendoDangerRings(position, CampanellaGold, chargeProgress, 4);
            }
        }
        
        #endregion
        
        #region Fate Theme Integration (Cosmic, Celestial)
        
        public static class Fate
        {
            private static readonly Color FateBlack = new Color(15, 5, 20);
            private static readonly Color FatePink = new Color(180, 50, 100);
            private static readonly Color FateRed = new Color(255, 60, 80);
            private static readonly Color FateWhite = new Color(255, 255, 255);
            
            /// <summary>
            /// Add cosmic chord progressions to celestial attacks
            /// </summary>
            public static void CosmicChordProgression(Vector2 position, int chordNumber)
            {
                Color[] cosmicChord = { FateBlack, FatePink, FateRed, FateWhite };
                int colorIndex = chordNumber % cosmicChord.Length;
                
                // Chord resolution with cosmic colors
                Phase10BossVFX.ChordResolutionBloom(position, cosmicChord, 0.8f);
                
                // Glyphs orbiting
                float glyphAngle = Main.GameUpdateCount * 0.02f + chordNumber * 0.5f;
                for (int i = 0; i < 4; i++)
                {
                    float angle = glyphAngle + MathHelper.TwoPi * i / 4f;
                    Vector2 glyphPos = position + angle.ToRotationVector2() * 50f;
                    CustomParticles.Glyph(glyphPos, cosmicChord[i], 0.35f, Main.rand.Next(1, 13));
                }
                
                // Star sparkle accents
                for (int i = 0; i < 3; i++)
                {
                    Vector2 starPos = position + Main.rand.NextVector2Circular(40f, 40f);
                    CustomParticles.GenericFlare(starPos, FateWhite, 0.25f, 15);
                }
            }
            
            /// <summary>
            /// Add reality-bending tempo distortion
            /// </summary>
            public static void RealityTempoDistortion(Vector2 position, float intensity)
            {
                // Tempo shift representing reality bending
                Phase10BossVFX.TempoShiftDistortion(position, 120f, 120f * (1f + intensity * 0.5f), 80f);
                
                // Chromatic aberration visual
                Vector2 offset1 = new Vector2(-3f, 0);
                Vector2 offset2 = new Vector2(3f, 0);
                CustomParticles.GenericFlare(position + offset1, new Color(255, 0, 0) * 0.3f, 0.4f, 10);
                CustomParticles.GenericFlare(position + offset2, new Color(0, 0, 255) * 0.3f, 0.4f, 10);
            }
            
            /// <summary>
            /// Add constellation-like arpeggio patterns
            /// </summary>
            public static void ConstellationArpeggio(Vector2 position, int noteIndex, int totalNotes)
            {
                // Each note is a star in the constellation
                float progress = (float)noteIndex / totalNotes;
                Color starColor = Color.Lerp(FatePink, FateWhite, progress);
                
                Phase10BossVFX.ArpeggioCascade(position, starColor, noteIndex + 1, 80f);
                
                // Connect with faint constellation lines
                if (noteIndex > 0)
                {
                    // Line would be drawn to previous note position
                    Phase10BossVFX.StaffLineConvergence(position, FateWhite * 0.3f, 0.3f, 40f);
                }
            }
            
            /// <summary>
            /// Cosmic Judgment - Ultimate attack enhancement
            /// </summary>
            public static void CosmicJudgmentVFX(Vector2 position, float progress)
            {
                // Tutti full ensemble for cosmic scale
                Phase10BossVFX.TuttiFullEnsemble(position, 
                    new[] { FateBlack, FatePink, FateRed, FateWhite }, 1.5f);
                
                // Coda finale at completion
                if (progress >= 0.95f)
                {
                    Phase10BossVFX.CodaFinale(position, FateWhite, FateBlack, 2f);
                }
                
                // Cadence finisher
                Phase10BossVFX.CadenceFinisher(position, 
                    new[] { FateBlack, FatePink, FateRed, FateWhite }, progress);
            }
        }
        
        #endregion
        
        #region Universal Musical Enhancements
        
        /// <summary>
        /// Generic musical VFX that works with any theme color
        /// </summary>
        public static class Universal
        {
            /// <summary>
            /// Add musical note trail to any projectile
            /// Call in projectile AI
            /// </summary>
            public static void MusicalProjectileTrail(Vector2 position, Vector2 velocity, Color themeColor, int timer)
            {
                // Note constellation trail
                if (timer % 8 == 0)
                {
                    float hueShift = (timer * 0.02f) % 0.2f - 0.1f;
                    Color noteColor = RotateHue(themeColor, hueShift);
                    
                    CustomParticles.GenericMusicNotes(position, noteColor, 1, 10f);
                }
                
                // Staff line segments
                if (timer % 15 == 0)
                {
                    Phase10BossVFX.StaffLineConvergence(position, themeColor * 0.6f, 0.3f, 30f);
                }
                
                // Sparkle dust
                if (Main.rand.NextBool(3))
                {
                    var sparkle = new SparkleParticle(
                        position + Main.rand.NextVector2Circular(8f, 8f),
                        -velocity * 0.1f,
                        themeColor,
                        0.25f,
                        18
                    );
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }
            
            /// <summary>
            /// Add crescendo buildup to any charge attack
            /// </summary>
            public static void CrescendoChargeUp(Vector2 position, Color themeColor, float chargeProgress)
            {
                Phase10BossVFX.CrescendoDangerRings(position, themeColor, chargeProgress, 2 + (int)(chargeProgress * 2));
                Phase10BossVFX.DynamicsWave(position, chargeProgress, themeColor);
                
                // Accelerating particle spiral
                if (chargeProgress > 0.2f)
                {
                    Phase10BossVFX.AccelerandoSpiral(position, themeColor, chargeProgress, 
                        4 + (int)(chargeProgress * 8));
                }
            }
            
            /// <summary>
            /// Add dramatic impact to any attack hit
            /// </summary>
            public static void DramaticImpact(Vector2 position, Color primaryColor, Color secondaryColor, float intensity)
            {
                // Chord resolution
                Phase10BossVFX.ChordResolutionBloom(position, new[] { primaryColor, secondaryColor }, intensity);
                
                // Cymbal crash for strong hits
                if (intensity >= 0.6f)
                {
                    Phase10BossVFX.CymbalCrashBurst(position, intensity);
                }
                
                // Staccato multi-burst for rapid hits
                if (intensity < 0.4f)
                {
                    Phase10BossVFX.StaccatoMultiBurst(position, primaryColor, 3, 20f);
                }
                
                // Music note scatter
                int noteCount = 2 + (int)(intensity * 4);
                for (int i = 0; i < noteCount; i++)
                {
                    Vector2 noteVel = Main.rand.NextVector2Circular(3f, 3f);
                    CustomParticles.GenericMusicNotes(position + noteVel * 5f, primaryColor, 1, 8f);
                }
            }
            
            /// <summary>
            /// Add beat-synced rhythm to timed attacks
            /// </summary>
            public static void BeatSyncedRhythm(Vector2 position, Color themeColor, float bpm, int timer)
            {
                float framesPerBeat = 3600f / bpm;
                bool isOnBeat = (timer % (int)framesPerBeat) < 4;
                bool isOnOffBeat = ((timer + (int)(framesPerBeat / 2)) % (int)framesPerBeat) < 4;
                
                if (isOnBeat)
                {
                    // Strong beat - timpani pulse
                    Phase10BossVFX.TimpaniDrumrollImpact(position, themeColor, 0.6f);
                }
                else if (isOnOffBeat)
                {
                    // Syncopation accent
                    Phase10BossVFX.SyncopationStutter(position, themeColor, bpm);
                }
            }
            
            /// <summary>
            /// Add finale VFX to death animations
            /// </summary>
            public static void DeathFinale(Vector2 position, Color primaryColor, Color secondaryColor)
            {
                // Grand coda finale
                Phase10BossVFX.CodaFinale(position, primaryColor, secondaryColor, 2f);
                
                // Cadence resolution
                Phase10BossVFX.CadenceFinisher(position, new[] { primaryColor, secondaryColor, Color.White }, 1f);
                
                // Fermata release - dramatic hold then burst
                Phase10BossVFX.FermataReleaseBurst(position, Color.White, 2f);
                
                // Final note scatter
                for (int i = 0; i < 15; i++)
                {
                    float angle = MathHelper.TwoPi * i / 15f;
                    Vector2 notePos = position + angle.ToRotationVector2() * 60f;
                    CustomParticles.GenericMusicNotes(notePos, Color.Lerp(primaryColor, secondaryColor, i / 15f), 1, 25f);
                }
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        private static Color RotateHue(Color color, float amount)
        {
            Vector3 hsv = Main.rgbToHsl(color);
            hsv.X = (hsv.X + amount + 1f) % 1f;
            return Main.hslToRgb(hsv.X, hsv.Y, hsv.Z);
        }
        
        #endregion
    }
}
