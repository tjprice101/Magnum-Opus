using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// PHASE 10: BOSS ATTACK PATTERNS
    /// 
    /// Contains 70 unique boss attack patterns categorized by type:
    /// - Projectile Patterns (Musical formations)
    /// - Movement Patterns (Rhythmic movement)
    /// - Multi-Phase Attacks (Complex sequences)
    /// - Iterative/Dynamic Attacks (Evolving patterns)
    /// </summary>
    public static class Phase10BossAttacks
    {
        #region Musical Timing Helpers
        
        private const float BPM_SLOW = 60f;
        private const float BPM_MODERATE = 120f;
        private const float BPM_FAST = 180f;
        
        private static float GetFramesPerBeat(float bpm) => 3600f / bpm;
        private static bool IsOnBeat(float bpm) => (int)(Main.GameUpdateCount % GetFramesPerBeat(bpm)) < 4;
        private static bool IsOnBeatOffset(float bpm, float offset) => (int)((Main.GameUpdateCount + offset) % GetFramesPerBeat(bpm)) < 4;
        
        #endregion
        
        #region Projectile Patterns (1-20): Musical Formation Attacks
        
        /// <summary>
        /// #1 - Staff Line Barrage: 5 horizontal projectile lines like a musical staff
        /// </summary>
        public static void StaffLineBarrage(NPC npc, Player target, int damage, Color color, float speed = 10f)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            
            Vector2 direction = (target.Center - npc.Center).SafeNormalize(Vector2.UnitX);
            Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
            
            // 5 lines of the staff
            for (int line = -2; line <= 2; line++)
            {
                Vector2 spawnPos = npc.Center + perpendicular * (line * 25f);
                Vector2 velocity = direction * speed;
                
                // Spawn themed projectile
                BossProjectileHelper.SpawnHostileOrb(spawnPos, velocity, damage, color, 0f);
            }
            
            // VFX
            Phase10BossVFX.StaffLineConvergence(npc.Center + direction * 50f, color, 1f, 100f);
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f }, npc.Center);
        }
        
        /// <summary>
        /// #2 - Chord Burst: 3-4 projectiles fired simultaneously representing chord notes
        /// </summary>
        public static void ChordBurst(NPC npc, Player target, int damage, Color[] chordColors, float spread = 0.3f, float speed = 12f)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            if (chordColors == null || chordColors.Length == 0) return;
            
            Vector2 baseDirection = (target.Center - npc.Center).SafeNormalize(Vector2.UnitX);
            int noteCount = chordColors.Length;
            
            for (int i = 0; i < noteCount; i++)
            {
                float angleOffset = MathHelper.Lerp(-spread / 2f, spread / 2f, noteCount > 1 ? (float)i / (noteCount - 1) : 0.5f);
                Vector2 velocity = baseDirection.RotatedBy(angleOffset) * speed;
                
                BossProjectileHelper.SpawnAcceleratingBolt(npc.Center, velocity, damage, chordColors[i], 5f);
            }
            
            // Chord resolution VFX
            Phase10BossVFX.ChordResolutionBloom(npc.Center, chordColors, 0.6f);
        }
        
        /// <summary>
        /// #3 - Arpeggio Wave: Projectiles fired in quick sequence creating a wave pattern
        /// </summary>
        public static void ArpeggioWave(NPC npc, Player target, int damage, Color baseColor, int timer, int noteCount = 8, float delayPerNote = 4f)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            
            int currentNote = (int)(timer / delayPerNote);
            if (currentNote >= noteCount) return;
            if (timer % (int)delayPerNote != 0) return;
            
            Vector2 direction = (target.Center - npc.Center).SafeNormalize(Vector2.UnitX);
            float arcOffset = MathHelper.Lerp(-0.4f, 0.4f, (float)currentNote / (noteCount - 1));
            
            Vector2 velocity = direction.RotatedBy(arcOffset) * (10f + currentNote * 0.5f);
            Color noteColor = RotateHue(baseColor, (float)currentNote / noteCount * 0.2f);
            
            BossProjectileHelper.SpawnHostileOrb(npc.Center, velocity, damage, noteColor, 0.01f);
            
            // Arpeggio VFX
            Phase10BossVFX.ArpeggioCascade(npc.Center, baseColor, currentNote + 1, 60f);
        }
        
        /// <summary>
        /// #4 - Scale Run: 8-note ascending/descending scale pattern
        /// </summary>
        public static void ScaleRun(NPC npc, Player target, int damage, Color color, bool ascending, float startAngle = -0.5f, float endAngle = 0.5f)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            
            Vector2 baseDirection = (target.Center - npc.Center).SafeNormalize(Vector2.UnitX);
            int noteCount = 8;
            
            for (int i = 0; i < noteCount; i++)
            {
                float progress = ascending ? (float)i / (noteCount - 1) : 1f - (float)i / (noteCount - 1);
                float angle = MathHelper.Lerp(startAngle, endAngle, progress);
                
                Vector2 velocity = baseDirection.RotatedBy(angle) * (8f + i * 1.5f);
                
                // Stagger spawn times by giving different initial speeds
                BossProjectileHelper.SpawnAcceleratingBolt(npc.Center, velocity * (0.6f + i * 0.05f), damage, color, 8f);
            }
            
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = ascending ? -0.3f : 0.3f }, npc.Center);
        }
        
        /// <summary>
        /// #5 - Tremolo Burst: Rapid alternating projectiles between two angles
        /// </summary>
        public static void TremoloBurst(NPC npc, Player target, int damage, Color colorA, Color colorB, int timer, int duration = 60, float angleA = -0.2f, float angleB = 0.2f)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            if (timer >= duration || timer % 4 != 0) return;
            
            Vector2 baseDirection = (target.Center - npc.Center).SafeNormalize(Vector2.UnitX);
            bool useA = (timer / 4) % 2 == 0;
            
            float angle = useA ? angleA : angleB;
            Color color = useA ? colorA : colorB;
            
            Vector2 velocity = baseDirection.RotatedBy(angle) * 14f;
            BossProjectileHelper.SpawnHostileOrb(npc.Center, velocity, damage, color, 0.005f);
            
            // Trill VFX
            Phase10BossVFX.TrillVibrationWarning(npc.Center + baseDirection * 30f, colorA, colorB, (float)timer / duration);
        }
        
        /// <summary>
        /// #6 - Fermata Hold Burst: Charged attack that releases after held buildup
        /// </summary>
        public static void FermataHoldBurst(NPC npc, Player target, int damage, Color color, int chargeTimer, int chargeRequired, bool release)
        {
            if (!release)
            {
                // Charging - show fermata indicator
                float chargeProgress = (float)chargeTimer / chargeRequired;
                Phase10BossVFX.FermataHoldIndicator(npc.Center, color, chargeProgress);
                return;
            }
            
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            
            // Release - massive burst scaled to charge time
            float chargeBonus = Math.Min(2f, (float)chargeTimer / chargeRequired);
            int projectileCount = (int)(8 + chargeBonus * 12);
            
            for (int i = 0; i < projectileCount; i++)
            {
                float angle = MathHelper.TwoPi * i / projectileCount;
                Vector2 velocity = angle.ToRotationVector2() * (10f + chargeBonus * 5f);
                
                BossProjectileHelper.SpawnAcceleratingBolt(npc.Center, velocity, (int)(damage * (0.8f + chargeBonus * 0.4f)), color, 10f);
            }
            
            Phase10BossVFX.FermataReleaseBurst(npc.Center, color, chargeBonus);
        }
        
        /// <summary>
        /// #7 - Staccato Spray: Short, sharp bursts of projectiles
        /// </summary>
        public static void StaccatoSpray(NPC npc, Player target, int damage, Color color, int burstCount = 5, int projectilesPerBurst = 3)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            
            Vector2 baseDirection = (target.Center - npc.Center).SafeNormalize(Vector2.UnitX);
            
            for (int burst = 0; burst < burstCount; burst++)
            {
                Vector2 burstOffset = Main.rand.NextVector2Circular(50f, 50f);
                Vector2 burstCenter = npc.Center + burstOffset;
                
                for (int p = 0; p < projectilesPerBurst; p++)
                {
                    float spread = Main.rand.NextFloat(-0.2f, 0.2f);
                    Vector2 velocity = baseDirection.RotatedBy(spread) * Main.rand.NextFloat(10f, 15f);
                    
                    BossProjectileHelper.SpawnHostileOrb(burstCenter, velocity, damage, color, 0f);
                }
            }
            
            Phase10BossVFX.StaccatoMultiBurst(npc.Center, color, burstCount, 50f);
        }
        
        /// <summary>
        /// #8 - Legato Stream: Continuous stream of connected projectiles
        /// </summary>
        public static void LegatoStream(NPC npc, Player target, int damage, Color color, int timer, float speed = 8f)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            if (timer % 6 != 0) return; // Continuous but not overwhelming
            
            Vector2 direction = (target.Center - npc.Center).SafeNormalize(Vector2.UnitX);
            
            // Slight wave to the stream
            float waveOffset = (float)Math.Sin(timer * 0.1f) * 0.15f;
            Vector2 velocity = direction.RotatedBy(waveOffset) * speed;
            
            BossProjectileHelper.SpawnWaveProjectile(npc.Center, velocity, damage, color, 2f);
            
            // Smooth legato VFX
            Vector2 targetPoint = npc.Center + direction * 200f;
            Phase10BossVFX.LegatoWaveWash(npc.Center, direction, color, 150f);
        }
        
        /// <summary>
        /// #9 - Fortissimo Slam: Maximum intensity attack
        /// </summary>
        public static void FortissimoSlam(NPC npc, Player target, int damage, Color color)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            
            // Massive radial burst
            int projectileCount = 24;
            for (int i = 0; i < projectileCount; i++)
            {
                float angle = MathHelper.TwoPi * i / projectileCount;
                Vector2 velocity = angle.ToRotationVector2() * 16f;
                
                BossProjectileHelper.SpawnAcceleratingBolt(npc.Center, velocity, (int)(damage * 1.3f), color, 12f);
            }
            
            // Ground waves
            for (int w = 0; w < 8; w++)
            {
                float waveAngle = MathHelper.PiOver4 * w;
                Vector2 waveVel = waveAngle.ToRotationVector2() * 8f;
                waveVel.Y = Math.Abs(waveVel.Y) * 0.3f; // Ground hugging
                
                BossProjectileHelper.SpawnWaveProjectile(npc.Center, waveVel, damage, color, 4f);
            }
            
            Phase10BossVFX.SforzandoSpike(npc.Center, color, 1.5f);
            MagnumScreenEffects.AddScreenShake(15f);
        }
        
        /// <summary>
        /// #10 - Pianissimo Whisper: Quiet, subtle attack pattern
        /// </summary>
        public static void PianissimoWhisper(NPC npc, Player target, int damage, Color color, int timer)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            if (timer % 20 != 0) return; // Sparse, quiet attacks
            
            Vector2 direction = (target.Center - npc.Center).SafeNormalize(Vector2.UnitX);
            Vector2 velocity = direction * 6f; // Slow, gentle
            
            // Single quiet projectile
            BossProjectileHelper.SpawnHostileOrb(npc.Center, velocity, (int)(damage * 0.6f), color * 0.7f, 0.02f);
            
            // Subtle VFX
            Phase10BossVFX.DynamicsWave(npc.Center, 0.2f, color);
        }
        
        /// <summary>
        /// #11 - Sforzando Strike: Sudden accent attack
        /// </summary>
        public static void SforzandoStrike(NPC npc, Player target, int damage, Color color)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            
            Vector2 direction = (target.Center - npc.Center).SafeNormalize(Vector2.UnitX);
            
            // Central powerful projectile
            BossProjectileHelper.SpawnAcceleratingBolt(npc.Center, direction * 20f, (int)(damage * 1.5f), Color.White, 15f);
            
            // Surrounding accents
            for (int i = 0; i < 4; i++)
            {
                float offset = (i - 1.5f) * 0.15f;
                Vector2 accentVel = direction.RotatedBy(offset) * 15f;
                BossProjectileHelper.SpawnHostileOrb(npc.Center, accentVel, damage, color, 0f);
            }
            
            Phase10BossVFX.SforzandoSpike(npc.Center, color, 1f);
        }
        
        /// <summary>
        /// #12 - Crescendo Wave: Attack that builds in intensity
        /// </summary>
        public static void CrescendoWave(NPC npc, Player target, int damage, Color color, int timer, int duration = 120)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            
            float progress = (float)timer / duration;
            int fireRate = Math.Max(3, 15 - (int)(progress * 12)); // Fires faster as it progresses
            
            if (timer % fireRate != 0) return;
            
            Vector2 direction = (target.Center - npc.Center).SafeNormalize(Vector2.UnitX);
            float speed = 6f + progress * 10f;
            int currentDamage = (int)(damage * (0.5f + progress * 0.8f));
            
            Vector2 velocity = direction.RotatedBy(Main.rand.NextFloat(-0.1f, 0.1f)) * speed;
            BossProjectileHelper.SpawnHostileOrb(npc.Center, velocity, currentDamage, color, 0.01f + progress * 0.02f);
            
            Phase10BossVFX.DynamicsWave(npc.Center, progress, color);
        }
        
        /// <summary>
        /// #13 - Decrescendo Fade: Attack that diminishes in intensity
        /// </summary>
        public static void DecrescendoFade(NPC npc, Player target, int damage, Color color, int timer, int duration = 120)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            
            float progress = (float)timer / duration;
            float intensity = 1f - progress;
            int fireRate = 3 + (int)(progress * 15); // Fires slower as it progresses
            
            if (timer % fireRate != 0) return;
            
            Vector2 direction = (target.Center - npc.Center).SafeNormalize(Vector2.UnitX);
            float speed = 16f - progress * 10f;
            int currentDamage = (int)(damage * (1f - progress * 0.5f));
            
            Vector2 velocity = direction.RotatedBy(Main.rand.NextFloat(-0.1f, 0.1f) * intensity) * speed;
            BossProjectileHelper.SpawnHostileOrb(npc.Center, velocity, currentDamage, color * (0.5f + intensity * 0.5f), 0.03f - progress * 0.02f);
            
            Phase10BossVFX.DynamicsWave(npc.Center, intensity, color);
        }
        
        /// <summary>
        /// #14 - Syncopated Burst: Off-beat surprise attacks
        /// </summary>
        public static void SyncopatedBurst(NPC npc, Player target, int damage, Color color, float bpm = 120f)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            
            // Fire on the off-beat (between beats)
            float halfBeat = GetFramesPerBeat(bpm) / 2f;
            bool onOffBeat = ((int)(Main.GameUpdateCount % halfBeat) < 4) && !IsOnBeat(bpm);
            
            if (!onOffBeat) return;
            
            Vector2 direction = (target.Center - npc.Center).SafeNormalize(Vector2.UnitX);
            
            // Unexpected angle
            float surpriseAngle = Main.rand.NextFloat(-0.4f, 0.4f);
            Vector2 velocity = direction.RotatedBy(surpriseAngle) * 14f;
            
            BossProjectileHelper.SpawnAcceleratingBolt(npc.Center, velocity, damage, color, 8f);
            
            Phase10BossVFX.SyncopationStutter(npc.Center, color, bpm);
        }
        
        /// <summary>
        /// #15 - Triplet Cluster: Groups of 3 projectiles
        /// </summary>
        public static void TripletCluster(NPC npc, Player target, int damage, Color color, int clusterCount = 4)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            
            Vector2 baseDirection = (target.Center - npc.Center).SafeNormalize(Vector2.UnitX);
            
            for (int cluster = 0; cluster < clusterCount; cluster++)
            {
                float clusterAngle = MathHelper.Lerp(-0.4f, 0.4f, (float)cluster / (clusterCount - 1));
                Vector2 clusterDir = baseDirection.RotatedBy(clusterAngle);
                
                // 3 projectiles per cluster
                for (int i = 0; i < 3; i++)
                {
                    float tripletOffset = (i - 1) * 0.08f;
                    Vector2 velocity = clusterDir.RotatedBy(tripletOffset) * (10f + i * 2f);
                    
                    BossProjectileHelper.SpawnHostileOrb(npc.Center, velocity, damage, color, 0.01f);
                }
            }
            
            SoundEngine.PlaySound(SoundID.Item122, npc.Center);
        }
        
        /// <summary>
        /// #16 - Octave Jump: Projectiles at doubled/halved intervals
        /// </summary>
        public static void OctaveJump(NPC npc, Player target, int damage, Color lowColor, Color highColor)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            
            Vector2 direction = (target.Center - npc.Center).SafeNormalize(Vector2.UnitX);
            
            // Low octave - slow, heavy
            for (int i = 0; i < 4; i++)
            {
                float angle = (i - 1.5f) * 0.2f;
                Vector2 lowVel = direction.RotatedBy(angle) * 6f;
                BossProjectileHelper.SpawnWaveProjectile(npc.Center, lowVel, (int)(damage * 1.2f), lowColor, 5f);
            }
            
            // High octave - fast, light
            for (int i = 0; i < 8; i++)
            {
                float angle = (i - 3.5f) * 0.1f;
                Vector2 highVel = direction.RotatedBy(angle) * 14f;
                BossProjectileHelper.SpawnHostileOrb(npc.Center, highVel, (int)(damage * 0.7f), highColor, 0.005f);
            }
        }
        
        /// <summary>
        /// #17 - Harmonic Series: Projectiles at musical harmonic ratios
        /// </summary>
        public static void HarmonicSeries(NPC npc, Player target, int damage, Color baseColor)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            
            Vector2 direction = (target.Center - npc.Center).SafeNormalize(Vector2.UnitX);
            
            // Fundamental and harmonics
            float[] harmonicRatios = { 1f, 0.5f, 0.333f, 0.25f, 0.2f }; // 1st through 5th harmonic
            float baseSpeed = 12f;
            
            for (int h = 0; h < harmonicRatios.Length; h++)
            {
                float speed = baseSpeed * harmonicRatios[h] * 2f; // Inverse for speed
                int projCount = h + 1; // More projectiles for higher harmonics
                
                for (int p = 0; p < projCount; p++)
                {
                    float spread = projCount > 1 ? MathHelper.Lerp(-0.15f, 0.15f, (float)p / (projCount - 1)) : 0f;
                    Vector2 velocity = direction.RotatedBy(spread) * speed;
                    
                    Color harmonicColor = RotateHue(baseColor, h * 0.1f);
                    BossProjectileHelper.SpawnHostileOrb(npc.Center, velocity, damage, harmonicColor, 0.01f);
                }
            }
            
            Phase10BossVFX.HarmonicOvertoneBeam(npc.Center, npc.Center + direction * 200f, baseColor);
        }
        
        /// <summary>
        /// #18 - Counterpoint Dual: Two independent projectile streams
        /// </summary>
        public static void CounterpointDual(NPC npc, Player target, int damage, Color colorA, Color colorB, int timer)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            
            Vector2 direction = (target.Center - npc.Center).SafeNormalize(Vector2.UnitX);
            
            // Voice A - fires on even intervals, upper trajectory
            if (timer % 10 == 0)
            {
                Vector2 velA = direction.RotatedBy(-0.3f) * 10f;
                BossProjectileHelper.SpawnHostileOrb(npc.Center + new Vector2(0, -30f), velA, damage, colorA, 0.01f);
            }
            
            // Voice B - fires on odd intervals (offset), lower trajectory
            if (timer % 10 == 5)
            {
                Vector2 velB = direction.RotatedBy(0.3f) * 12f;
                BossProjectileHelper.SpawnHostileOrb(npc.Center + new Vector2(0, 30f), velB, damage, colorB, 0.01f);
            }
            
            Phase10BossVFX.CounterpointDuality(npc.Center, colorA, colorB, timer * 0.1f);
        }
        
        /// <summary>
        /// #19 - Canon Round: Delayed repetition of projectile patterns
        /// </summary>
        public static void CanonRound(NPC npc, Player target, int damage, Color color, int timer, int voices = 3, float voiceDelay = 20f)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            
            Vector2 direction = (target.Center - npc.Center).SafeNormalize(Vector2.UnitX);
            
            for (int voice = 0; voice < voices; voice++)
            {
                int voiceTimer = timer - (int)(voice * voiceDelay);
                if (voiceTimer < 0 || voiceTimer % 15 != 0) continue;
                
                float voiceAngle = voice * 0.2f - 0.2f; // Spread voices
                Vector2 velocity = direction.RotatedBy(voiceAngle) * (10f + voice * 2f);
                
                Color voiceColor = RotateHue(color, voice * 0.15f);
                BossProjectileHelper.SpawnHostileOrb(npc.Center, velocity, damage, voiceColor, 0.01f);
            }
            
            Phase10BossVFX.CanonEcho(npc.Center, color, voices, voiceDelay);
        }
        
        /// <summary>
        /// #20 - Fugue Subject: Complex interwoven attack patterns
        /// </summary>
        public static void FugueSubject(NPC npc, Player target, int damage, Color[] voiceColors, int timer)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            if (voiceColors == null || voiceColors.Length == 0) return;
            
            Vector2 direction = (target.Center - npc.Center).SafeNormalize(Vector2.UnitX);
            int voiceCount = Math.Min(voiceColors.Length, 4);
            
            for (int v = 0; v < voiceCount; v++)
            {
                // Each voice enters at different time with different pattern
                int voiceEntry = v * 30;
                int voiceTimer = timer - voiceEntry;
                if (voiceTimer < 0) continue;
                
                // Voice-specific firing rate and pattern
                int fireRate = 8 + v * 2;
                if (voiceTimer % fireRate != 0) continue;
                
                float voiceAngle = (float)Math.Sin(voiceTimer * 0.1f + v * MathHelper.PiOver2) * 0.4f;
                Vector2 velocity = direction.RotatedBy(voiceAngle) * (8f + v * 2f);
                
                BossProjectileHelper.SpawnHostileOrb(npc.Center, velocity, damage, voiceColors[v], 0.01f + v * 0.005f);
            }
            
            Phase10BossVFX.FugueInterlace(npc.Center, voiceColors, timer * 0.02f);
        }
        
        #endregion
        
        #region Movement Patterns (21-35): Rhythmic Boss Movement
        
        /// <summary>
        /// #21 - Waltz Step: 3/4 time movement pattern (1-2-3, 1-2-3)
        /// </summary>
        public static Vector2 WaltzStep(Vector2 currentPos, Vector2 targetPos, int timer, float speed = 5f)
        {
            int beatInMeasure = (timer % 18) / 6; // 0, 1, 2 for each beat in 3/4 time
            
            Vector2 toTarget = (targetPos - currentPos).SafeNormalize(Vector2.Zero);
            Vector2 perpendicular = toTarget.RotatedBy(MathHelper.PiOver2);
            
            // Beat 1: Strong forward movement
            // Beat 2: Slight right sway
            // Beat 3: Slight left sway
            Vector2 movement = beatInMeasure switch
            {
                0 => toTarget * speed * 1.5f,
                1 => toTarget * speed * 0.5f + perpendicular * speed * 0.7f,
                2 => toTarget * speed * 0.5f - perpendicular * speed * 0.7f,
                _ => toTarget * speed
            };
            
            return currentPos + movement;
        }
        
        /// <summary>
        /// #22 - March Step: 4/4 time strong beat movement
        /// </summary>
        public static Vector2 MarchStep(Vector2 currentPos, Vector2 targetPos, int timer, float speed = 6f)
        {
            int beatInMeasure = (timer % 24) / 6; // 0, 1, 2, 3 for 4/4 time
            
            Vector2 toTarget = (targetPos - currentPos).SafeNormalize(Vector2.Zero);
            
            // Beats 1 and 3 are strong, 2 and 4 are weak
            float beatStrength = (beatInMeasure == 0 || beatInMeasure == 2) ? 1.5f : 0.8f;
            
            return currentPos + toTarget * speed * beatStrength;
        }
        
        /// <summary>
        /// #23 - Rubato Drift: Flexible, expressive movement
        /// </summary>
        public static Vector2 RubatoDrift(Vector2 currentPos, Vector2 targetPos, int timer, float baseSpeed = 4f)
        {
            // Speed varies organically
            float breathCycle = (float)Math.Sin(timer * 0.03f) * 0.5f + 0.5f; // 0 to 1
            float currentSpeed = baseSpeed * (0.5f + breathCycle);
            
            Vector2 toTarget = (targetPos - currentPos).SafeNormalize(Vector2.Zero);
            
            // Add slight random drift
            Vector2 drift = new Vector2(
                (float)Math.Sin(timer * 0.07f) * 0.3f,
                (float)Math.Cos(timer * 0.05f) * 0.3f
            );
            
            return currentPos + (toTarget + drift).SafeNormalize(Vector2.Zero) * currentSpeed;
        }
        
        /// <summary>
        /// #24 - Accelerando Rush: Movement that speeds up over time
        /// </summary>
        public static Vector2 AccelerandoRush(Vector2 currentPos, Vector2 targetPos, int timer, int duration, float minSpeed = 2f, float maxSpeed = 15f)
        {
            float progress = Math.Min(1f, (float)timer / duration);
            float currentSpeed = MathHelper.Lerp(minSpeed, maxSpeed, progress * progress); // Quadratic acceleration
            
            Vector2 toTarget = (targetPos - currentPos).SafeNormalize(Vector2.Zero);
            return currentPos + toTarget * currentSpeed;
        }
        
        /// <summary>
        /// #25 - Ritardando Slow: Movement that slows over time
        /// </summary>
        public static Vector2 RitardandoSlow(Vector2 currentPos, Vector2 targetPos, int timer, int duration, float maxSpeed = 15f, float minSpeed = 1f)
        {
            float progress = Math.Min(1f, (float)timer / duration);
            float currentSpeed = MathHelper.Lerp(maxSpeed, minSpeed, progress);
            
            Vector2 toTarget = (targetPos - currentPos).SafeNormalize(Vector2.Zero);
            return currentPos + toTarget * currentSpeed;
        }
        
        /// <summary>
        /// #26 - Staccato Dash: Quick, disconnected dashes
        /// </summary>
        public static Vector2 StaccatoDash(Vector2 currentPos, Vector2 targetPos, int timer, float dashSpeed = 20f, int dashDuration = 8, int pauseDuration = 15)
        {
            int cycleLength = dashDuration + pauseDuration;
            int posInCycle = timer % cycleLength;
            
            if (posInCycle < dashDuration)
            {
                // Dashing
                Vector2 toTarget = (targetPos - currentPos).SafeNormalize(Vector2.Zero);
                return currentPos + toTarget * dashSpeed;
            }
            else
            {
                // Paused
                return currentPos;
            }
        }
        
        /// <summary>
        /// #27 - Legato Glide: Smooth, connected movement
        /// </summary>
        public static Vector2 LegatoGlide(Vector2 currentPos, Vector2 targetPos, float smoothing = 0.05f)
        {
            // Simple smooth interpolation
            return Vector2.Lerp(currentPos, targetPos, smoothing);
        }
        
        /// <summary>
        /// #28 - Fermata Pause: Hold position dramatically
        /// </summary>
        public static Vector2 FermataPause(Vector2 currentPos, Vector2 targetPos, int timer, int holdStart, int holdDuration)
        {
            if (timer >= holdStart && timer < holdStart + holdDuration)
            {
                // Held - don't move
                return currentPos;
            }
            
            // Normal movement
            Vector2 toTarget = (targetPos - currentPos).SafeNormalize(Vector2.Zero);
            return currentPos + toTarget * 5f;
        }
        
        /// <summary>
        /// #29 - Trill Vibrate: Rapid small oscillations
        /// </summary>
        public static Vector2 TrillVibrate(Vector2 currentPos, Vector2 basePos, int timer, float amplitude = 15f, float frequency = 0.5f)
        {
            float offset = (float)Math.Sin(timer * frequency) * amplitude;
            return basePos + new Vector2(offset, 0);
        }
        
        /// <summary>
        /// #30 - Crescendo Approach: Get closer as intensity builds
        /// </summary>
        public static Vector2 CrescendoApproach(Vector2 currentPos, Vector2 targetPos, int timer, int duration, float startDist = 400f, float endDist = 100f)
        {
            float progress = Math.Min(1f, (float)timer / duration);
            float currentDist = MathHelper.Lerp(startDist, endDist, progress);
            
            Vector2 toTarget = (targetPos - currentPos).SafeNormalize(Vector2.Zero);
            float actualDist = Vector2.Distance(currentPos, targetPos);
            
            if (actualDist > currentDist)
            {
                return currentPos + toTarget * 8f;
            }
            else if (actualDist < currentDist - 20f)
            {
                return currentPos - toTarget * 4f;
            }
            
            return currentPos;
        }
        
        #endregion
        
        #region Multi-Phase Attacks (36-50): Complex Sequence Attacks
        
        /// <summary>
        /// #36 - Sonata Form Attack: Exposition-Development-Recapitulation structure
        /// Returns current phase (0=expo, 1=dev, 2=recap, 3=done)
        /// </summary>
        public static int SonataFormAttack(NPC npc, Player target, int damage, Color themeA, Color themeB, int timer, 
            int expositionDuration = 90, int developmentDuration = 120, int recapitulationDuration = 90)
        {
            int phase;
            int phaseTimer;
            
            if (timer < expositionDuration)
            {
                phase = 0; // Exposition
                phaseTimer = timer;
                
                // Theme A and B introduced
                if (phaseTimer < expositionDuration / 2)
                {
                    if (phaseTimer % 15 == 0)
                        ChordBurst(npc, target, damage, new[] { themeA }, 0.3f, 10f);
                }
                else
                {
                    if (phaseTimer % 15 == 0)
                        ChordBurst(npc, target, damage, new[] { themeB }, 0.3f, 10f);
                }
            }
            else if (timer < expositionDuration + developmentDuration)
            {
                phase = 1; // Development
                phaseTimer = timer - expositionDuration;
                
                // Themes combined and varied
                if (phaseTimer % 10 == 0)
                    CounterpointDual(npc, target, damage, themeA, themeB, phaseTimer);
            }
            else if (timer < expositionDuration + developmentDuration + recapitulationDuration)
            {
                phase = 2; // Recapitulation
                phaseTimer = timer - expositionDuration - developmentDuration;
                
                // Themes return together
                if (phaseTimer % 12 == 0)
                    ChordBurst(npc, target, damage, new[] { themeA, themeB }, 0.4f, 12f);
            }
            else
            {
                phase = 3; // Done
            }
            
            return phase;
        }
        
        /// <summary>
        /// #37 - Theme and Variations: Base pattern with progressive modifications
        /// </summary>
        public static void ThemeAndVariations(NPC npc, Player target, int damage, Color color, int variationNumber, int timer)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            
            Vector2 direction = (target.Center - npc.Center).SafeNormalize(Vector2.UnitX);
            int fireRate = Math.Max(5, 15 - variationNumber * 2);
            
            if (timer % fireRate != 0) return;
            
            switch (variationNumber % 6)
            {
                case 0: // Original theme
                    BossProjectileHelper.SpawnHostileOrb(npc.Center, direction * 10f, damage, color, 0.01f);
                    break;
                    
                case 1: // Rhythmic variation (faster)
                    BossProjectileHelper.SpawnHostileOrb(npc.Center, direction * 14f, (int)(damage * 0.8f), color, 0.01f);
                    break;
                    
                case 2: // Melodic variation (spread)
                    for (int i = -1; i <= 1; i++)
                    {
                        Vector2 vel = direction.RotatedBy(i * 0.15f) * 10f;
                        BossProjectileHelper.SpawnHostileOrb(npc.Center, vel, (int)(damage * 0.7f), color, 0.01f);
                    }
                    break;
                    
                case 3: // Harmonic variation (colors)
                    Color varColor = RotateHue(color, 0.2f);
                    BossProjectileHelper.SpawnHostileOrb(npc.Center, direction * 10f, damage, varColor, 0.02f);
                    break;
                    
                case 4: // Ornamental variation (trails)
                    BossProjectileHelper.SpawnWaveProjectile(npc.Center, direction * 8f, damage, color, 3f);
                    break;
                    
                case 5: // Finale variation (powerful)
                    BossProjectileHelper.SpawnAcceleratingBolt(npc.Center, direction * 8f, (int)(damage * 1.3f), color, 12f);
                    break;
            }
        }
        
        /// <summary>
        /// #38 - Rondo Return: A-B-A-C-A-D-A pattern where A always returns
        /// </summary>
        public static void RondoReturn(NPC npc, Player target, int damage, Color mainThemeColor, Color[] episodeColors, int section, int sectionTimer)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            if (episodeColors == null) episodeColors = new[] { Color.White };
            
            bool isMainTheme = section % 2 == 0;
            int episodeIndex = section / 2;
            
            if (isMainTheme)
            {
                // Main theme (Refrain) - always the same
                if (sectionTimer % 12 == 0)
                {
                    StaffLineBarrage(npc, target, damage, mainThemeColor, 10f);
                }
            }
            else
            {
                // Episode - varies each time
                Color episodeColor = episodeColors[episodeIndex % episodeColors.Length];
                
                if (sectionTimer % 10 == 0)
                {
                    float spread = 0.3f + episodeIndex * 0.1f;
                    ChordBurst(npc, target, damage, new[] { episodeColor, Color.Lerp(episodeColor, Color.White, 0.3f) }, spread, 12f);
                }
            }
        }
        
        /// <summary>
        /// #39 - Cadenza Frenzy: Virtuosic rapid-fire sequence
        /// </summary>
        public static void CadenzaFrenzy(NPC npc, Player target, int damage, Color color, int timer, int duration = 60)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            if (timer >= duration) return;
            
            float progress = (float)timer / duration;
            int fireRate = Math.Max(1, 4 - (int)(progress * 3)); // Gets faster
            
            if (timer % fireRate != 0) return;
            
            Vector2 direction = (target.Center - npc.Center).SafeNormalize(Vector2.UnitX);
            
            // Rapid varied angles
            float angle = (float)Math.Sin(timer * 0.5f) * 0.6f;
            float speed = 10f + Main.rand.NextFloat(8f);
            
            Vector2 velocity = direction.RotatedBy(angle) * speed;
            BossProjectileHelper.SpawnHostileOrb(npc.Center, velocity, damage, color, 0f);
            
            Phase10BossVFX.AccelerandoSpiral(npc.Center, color, 1f + progress, 8);
        }
        
        /// <summary>
        /// #40 - Coda Climax: Final powerful sequence
        /// </summary>
        public static void CodaClimax(NPC npc, Player target, int damage, Color primaryColor, Color secondaryColor, int timer, int duration = 90)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            
            float progress = (float)timer / duration;
            
            // Multiple attack types escalating
            if (timer % 20 == 0 && progress < 0.5f)
            {
                ChordBurst(npc, target, damage, new[] { primaryColor, secondaryColor }, 0.4f, 10f);
            }
            
            if (timer % 15 == 0 && progress >= 0.3f && progress < 0.7f)
            {
                StaffLineBarrage(npc, target, damage, primaryColor, 12f);
            }
            
            if (timer % 10 == 0 && progress >= 0.5f)
            {
                FortissimoSlam(npc, target, damage, Color.Lerp(primaryColor, secondaryColor, progress));
            }
            
            // Final burst
            if (timer == duration - 1)
            {
                Phase10BossVFX.CodaFinale(npc.Center, primaryColor, secondaryColor, 1f);
            }
            
            Phase10BossVFX.CadenceFinisher(npc.Center, new[] { primaryColor, secondaryColor }, progress);
        }
        
        #endregion
        
        #region Iterative/Dynamic Attacks (51-70): Evolving Patterns
        
        /// <summary>
        /// #51 - Evolving Melody: Pattern that develops based on player actions
        /// </summary>
        public static void EvolvingMelody(NPC npc, Player target, int damage, Color color, int hitsTaken, int timer)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            
            int evolutionLevel = Math.Min(5, hitsTaken / 3); // Evolves every 3 hits taken
            int fireRate = 15 - evolutionLevel * 2;
            float speed = 8f + evolutionLevel * 2f;
            int projectileCount = 1 + evolutionLevel;
            
            if (timer % Math.Max(3, fireRate) != 0) return;
            
            Vector2 direction = (target.Center - npc.Center).SafeNormalize(Vector2.UnitX);
            
            for (int i = 0; i < projectileCount; i++)
            {
                float offset = projectileCount > 1 ? MathHelper.Lerp(-0.2f, 0.2f, (float)i / (projectileCount - 1)) : 0f;
                Vector2 velocity = direction.RotatedBy(offset) * speed;
                
                BossProjectileHelper.SpawnHostileOrb(npc.Center, velocity, damage, color, 0.01f * evolutionLevel);
            }
        }
        
        /// <summary>
        /// #52 - Adaptive Tempo: Attack speed adjusts to player movement
        /// </summary>
        public static void AdaptiveTempo(NPC npc, Player target, int damage, Color color, int timer)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            
            float playerSpeed = target.velocity.Length();
            int fireRate = Math.Max(3, 20 - (int)(playerSpeed * 2)); // Faster if player moves fast
            
            if (timer % fireRate != 0) return;
            
            Vector2 direction = (target.Center - npc.Center).SafeNormalize(Vector2.UnitX);
            float projectileSpeed = 8f + playerSpeed * 0.5f;
            
            BossProjectileHelper.SpawnHostileOrb(npc.Center, direction * projectileSpeed, damage, color, 0.01f);
        }
        
        /// <summary>
        /// #53 - Dynamic Range: Intensity scales with boss HP
        /// </summary>
        public static void DynamicRange(NPC npc, Player target, int damage, Color color, int timer)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            
            float hpPercent = (float)npc.life / npc.lifeMax;
            float intensity = 1f - hpPercent; // More intense as HP drops
            
            int fireRate = (int)MathHelper.Lerp(20f, 5f, intensity);
            int projectileCount = 1 + (int)(intensity * 4);
            float speed = 8f + intensity * 8f;
            
            if (timer % fireRate != 0) return;
            
            Vector2 direction = (target.Center - npc.Center).SafeNormalize(Vector2.UnitX);
            
            for (int i = 0; i < projectileCount; i++)
            {
                float spread = Main.rand.NextFloat(-0.3f, 0.3f) * intensity;
                Vector2 velocity = direction.RotatedBy(spread) * speed;
                
                BossProjectileHelper.SpawnHostileOrb(npc.Center, velocity, (int)(damage * (0.7f + intensity * 0.5f)), color, intensity * 0.02f);
            }
            
            Phase10BossVFX.DynamicsWave(npc.Center, intensity, color);
        }
        
        /// <summary>
        /// #54 - Phrase Building: Attack builds phrases over time
        /// </summary>
        public static void PhraseBuilding(NPC npc, Player target, int damage, Color color, int timer, int phraseLength = 60)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            
            int posInPhrase = timer % phraseLength;
            float phraseProgress = (float)posInPhrase / phraseLength;
            
            // Phrase builds then releases
            if (phraseProgress < 0.8f)
            {
                // Building phase
                int buildRate = (int)MathHelper.Lerp(15f, 5f, phraseProgress);
                if (posInPhrase % Math.Max(3, buildRate) != 0) return;
                
                Vector2 direction = (target.Center - npc.Center).SafeNormalize(Vector2.UnitX);
                float speed = 6f + phraseProgress * 8f;
                
                BossProjectileHelper.SpawnHostileOrb(npc.Center, direction * speed, damage, color, 0.01f);
            }
            else
            {
                // Release/cadence
                if (posInPhrase == (int)(phraseLength * 0.8f))
                {
                    SforzandoStrike(npc, target, (int)(damage * 1.3f), color);
                }
            }
        }
        
        /// <summary>
        /// #55 - Call and Response: Boss attacks, waits for player position, then responds
        /// </summary>
        public static void CallAndResponse(NPC npc, Player target, int damage, Color callColor, Color responseColor, int timer, int responseDelay = 30)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            
            int cycleLength = responseDelay * 2;
            int posInCycle = timer % cycleLength;
            
            if (posInCycle == 0)
            {
                // Call - initial attack
                Vector2 direction = (target.Center - npc.Center).SafeNormalize(Vector2.UnitX);
                ChordBurst(npc, target, damage, new[] { callColor }, 0.3f, 10f);
            }
            else if (posInCycle == responseDelay)
            {
                // Response - adapted to where player moved
                Vector2 newDirection = (target.Center - npc.Center).SafeNormalize(Vector2.UnitX);
                
                // Wider spread to catch dodging player
                for (int i = -2; i <= 2; i++)
                {
                    Vector2 velocity = newDirection.RotatedBy(i * 0.15f) * 12f;
                    BossProjectileHelper.SpawnAcceleratingBolt(npc.Center, velocity, damage, responseColor, 8f);
                }
            }
        }
        
        /// <summary>
        /// #56 - Modulating Key: Attack pattern shifts through different "keys" (variations)
        /// </summary>
        public static void ModulatingKey(NPC npc, Player target, int damage, Color[] keyColors, int timer, int modulationInterval = 90)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            if (keyColors == null || keyColors.Length == 0) return;
            
            int currentKey = (timer / modulationInterval) % keyColors.Length;
            int timerInKey = timer % modulationInterval;
            
            Color currentColor = keyColors[currentKey];
            
            // Modulation transition effect
            if (timerInKey < 15)
            {
                Phase10BossVFX.KeyChangeFlash(npc.Center, 
                    keyColors[(currentKey + keyColors.Length - 1) % keyColors.Length],
                    currentColor, 
                    timerInKey / 15f);
            }
            
            // Attack in current key
            if (timerInKey % 12 == 0 && timerInKey >= 15)
            {
                Vector2 direction = (target.Center - npc.Center).SafeNormalize(Vector2.UnitX);
                
                // Different attack style per key
                switch (currentKey % 3)
                {
                    case 0:
                        BossProjectileHelper.SpawnHostileOrb(npc.Center, direction * 10f, damage, currentColor, 0.01f);
                        break;
                    case 1:
                        BossProjectileHelper.SpawnWaveProjectile(npc.Center, direction * 8f, damage, currentColor, 3f);
                        break;
                    case 2:
                        BossProjectileHelper.SpawnAcceleratingBolt(npc.Center, direction * 6f, damage, currentColor, 10f);
                        break;
                }
            }
        }
        
        /// <summary>
        /// #57 - Improvisation Chaos: Semi-random patterns with musical constraints
        /// </summary>
        public static void ImprovisationChaos(NPC npc, Player target, int damage, Color color, int timer)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            if (timer % 8 != 0) return;
            
            Vector2 direction = (target.Center - npc.Center).SafeNormalize(Vector2.UnitX);
            
            // Random but musically constrained
            float[] allowedAngles = { -0.4f, -0.2f, 0f, 0.2f, 0.4f }; // Pentatonic-like intervals
            float angle = allowedAngles[Main.rand.Next(allowedAngles.Length)];
            
            float[] allowedSpeeds = { 8f, 10f, 12f }; // Rhythmic speed values
            float speed = allowedSpeeds[Main.rand.Next(allowedSpeeds.Length)];
            
            Vector2 velocity = direction.RotatedBy(angle) * speed;
            
            // Random projectile type
            switch (Main.rand.Next(3))
            {
                case 0:
                    BossProjectileHelper.SpawnHostileOrb(npc.Center, velocity, damage, color, 0.01f);
                    break;
                case 1:
                    BossProjectileHelper.SpawnWaveProjectile(npc.Center, velocity * 0.8f, damage, color, 2f);
                    break;
                case 2:
                    BossProjectileHelper.SpawnAcceleratingBolt(npc.Center, velocity * 0.6f, damage, color, 10f);
                    break;
            }
        }
        
        /// <summary>
        /// #58 - Ostinato Loop: Repeating pattern that forms the base for variations
        /// </summary>
        public static void OstinatoLoop(NPC npc, Player target, int damage, Color color, int timer, int loopLength = 40)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            
            int posInLoop = timer % loopLength;
            int loopCount = timer / loopLength;
            
            // Base pattern - always the same rhythm
            int[] fireFrames = { 0, 10, 15, 25 };
            
            foreach (int frame in fireFrames)
            {
                if (posInLoop == frame)
                {
                    Vector2 direction = (target.Center - npc.Center).SafeNormalize(Vector2.UnitX);
                    
                    // Slight variation in each loop
                    float angleOffset = (float)Math.Sin(loopCount * 0.3f) * 0.1f;
                    float speed = 10f + loopCount % 3;
                    
                    Vector2 velocity = direction.RotatedBy(angleOffset) * speed;
                    BossProjectileHelper.SpawnHostileOrb(npc.Center, velocity, damage, color, 0.01f);
                }
            }
        }
        
        /// <summary>
        /// #59 - Polyrhythm Complex: Multiple overlapping rhythmic patterns
        /// </summary>
        public static void PolyrhythmComplex(NPC npc, Player target, int damage, Color colorA, Color colorB, Color colorC, int timer)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            
            Vector2 direction = (target.Center - npc.Center).SafeNormalize(Vector2.UnitX);
            
            // Pattern A: Every 12 frames (like 4/4)
            if (timer % 12 == 0)
            {
                BossProjectileHelper.SpawnHostileOrb(npc.Center, direction * 10f, damage, colorA, 0f);
            }
            
            // Pattern B: Every 16 frames (like 3/4)
            if (timer % 16 == 0)
            {
                Vector2 velB = direction.RotatedBy(0.3f) * 8f;
                BossProjectileHelper.SpawnHostileOrb(npc.Center, velB, damage, colorB, 0.01f);
            }
            
            // Pattern C: Every 20 frames (like 5/4)
            if (timer % 20 == 0)
            {
                Vector2 velC = direction.RotatedBy(-0.3f) * 12f;
                BossProjectileHelper.SpawnHostileOrb(npc.Center, velC, damage, colorC, 0.02f);
            }
        }
        
        /// <summary>
        /// #60 - Beat Drop: Building tension then sudden intense attack
        /// </summary>
        public static void BeatDrop(NPC npc, Player target, int damage, Color color, int timer, int buildupDuration = 60, int dropDuration = 30)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            
            int totalDuration = buildupDuration + dropDuration;
            
            if (timer < buildupDuration)
            {
                // Buildup - sparse but accelerating
                float progress = (float)timer / buildupDuration;
                int rate = (int)MathHelper.Lerp(20f, 5f, progress * progress);
                
                if (timer % Math.Max(3, rate) == 0)
                {
                    Vector2 direction = (target.Center - npc.Center).SafeNormalize(Vector2.UnitX);
                    BossProjectileHelper.SpawnHostileOrb(npc.Center, direction * (5f + progress * 5f), (int)(damage * 0.5f), color * 0.5f, 0f);
                }
                
                Phase10BossVFX.CrescendoDangerRings(npc.Center, color, progress, 3);
            }
            else if (timer < totalDuration)
            {
                // Drop - intense rapid fire
                int dropTimer = timer - buildupDuration;
                
                if (dropTimer == 0)
                {
                    // Initial drop burst
                    FortissimoSlam(npc, target, (int)(damage * 1.5f), color);
                    MagnumScreenEffects.AddScreenShake(20f);
                }
                else if (dropTimer % 3 == 0)
                {
                    Vector2 direction = (target.Center - npc.Center).SafeNormalize(Vector2.UnitX);
                    float angle = Main.rand.NextFloat(-0.4f, 0.4f);
                    BossProjectileHelper.SpawnAcceleratingBolt(npc.Center, direction.RotatedBy(angle) * 15f, damage, color, 12f);
                }
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        private static Color RotateHue(Color color, float amount)
        {
            Vector3 hsv = Main.rgbToHsl(color);
            hsv.X = (hsv.X + amount) % 1f;
            return Main.hslToRgb(hsv.X, hsv.Y, hsv.Z);
        }
        
        #endregion
    }
}
