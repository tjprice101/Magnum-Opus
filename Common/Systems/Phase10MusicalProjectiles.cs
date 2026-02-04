using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
    /// PHASE 10: PROPER POLISH - MUSICAL PROJECTILE SYSTEMS
    /// 
    /// DESIGN PHILOSOPHY:
    /// - Every projectile has MULTIPLE VISUAL LAYERS (core + orbiting elements + trail + particles)
    /// - All projectiles incorporate MUSICAL ELEMENTS (notes, clefs, staves, rhythm)
    /// - Projectiles have DYNAMIC BEHAVIORS (splitting, spawning sub-projectiles, beams)
    /// - Visual identity is UNIQUE per projectile type
    /// 
    /// This file contains the advanced projectile types from Phase 10 concepts.
    /// </summary>
    
    #region Musical Projectile Base Classes
    
    /// <summary>
    /// Base class for all Phase 10 musical projectiles with multi-layer rendering
    /// </summary>
    public abstract class MusicalProjectileBase : ModProjectile
    {
        // Shared musical properties
        protected float pulseTimer = 0f;
        protected float orbitAngle = 0f;
        protected float rhythmBeat = 0f;
        protected int noteVariant = 0;
        
        // Visual layers
        protected float coreScale = 1f;
        protected float orbitRadius = 15f;
        protected int orbitCount = 3;
        
        // Musical timing (60 BPM = 60 frames per beat)
        protected const float BPM = 120f;
        protected const float FramesPerBeat = 3600f / BPM; // 30 frames at 120 BPM
        
        public abstract Color PrimaryColor { get; }
        public abstract Color SecondaryColor { get; }
        public virtual Color AccentColor => Color.White;
        
        protected bool IsOnBeat => (int)(Main.GameUpdateCount % FramesPerBeat) < 4;
        protected float BeatProgress => (Main.GameUpdateCount % FramesPerBeat) / FramesPerBeat;
        
        public override void AI()
        {
            pulseTimer += 0.1f;
            orbitAngle += 0.08f;
            rhythmBeat = BeatProgress;
            
            // Beat-synchronized pulse
            if (IsOnBeat)
            {
                coreScale = 1.15f;
            }
            else
            {
                coreScale = MathHelper.Lerp(coreScale, 1f, 0.1f);
            }
            
            UpdateMusicalBehavior();
            SpawnOrbitingElements();
            SpawnTrailParticles();
            
            Lighting.AddLight(Projectile.Center, PrimaryColor.ToVector3() * 0.6f);
        }
        
        protected abstract void UpdateMusicalBehavior();
        
        protected virtual void SpawnOrbitingElements()
        {
            // Default: spawn orbiting music notes
            if (Projectile.timeLeft % 8 == 0)
            {
                for (int i = 0; i < orbitCount; i++)
                {
                    float angle = orbitAngle + MathHelper.TwoPi * i / orbitCount;
                    Vector2 orbitPos = Projectile.Center + angle.ToRotationVector2() * orbitRadius;
                    
                    // Music note at orbit position
                    ThemedParticles.MusicNote(orbitPos, Projectile.velocity * 0.3f, PrimaryColor, 0.6f, 15);
                }
            }
        }
        
        protected virtual void SpawnTrailParticles()
        {
            // Default: dense musical trail
            if (Main.rand.NextBool(2))
            {
                Vector2 dustOffset = Main.rand.NextVector2Circular(6f, 6f);
                var sparkle = new SparkleParticle(Projectile.Center + dustOffset, 
                    -Projectile.velocity * 0.15f, SecondaryColor, 0.35f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Contrasting dust
            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.MagicMirror, 
                -Projectile.velocity * 0.1f, 0, PrimaryColor, 1.2f);
            dust.noGravity = true;
        }
        
        protected void SpawnMusicNoteImpact(Vector2 position, int count = 8)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color noteColor = Color.Lerp(PrimaryColor, SecondaryColor, (float)i / count);
                ThemedParticles.MusicNote(position, vel, noteColor, 0.8f, 30);
            }
        }
    }
    
    #endregion
    
    #region Tier 1: Core Musical Projectiles
    
    /// <summary>
    /// #1 - Resonating Treble Orb: Spinning treble clef with orbiting eighth notes
    /// </summary>
    public class ResonatingTrebleOrb : MusicalProjectileBase
    {
        public override string Texture => "MagnumOpus/Assets/Particles/MusicNote";
        
        private Color _primaryColor;
        private Color _secondaryColor;
        
        public override Color PrimaryColor => _primaryColor;
        public override Color SecondaryColor => _secondaryColor;
        
        private float trebleRotation = 0f;
        private float staffTrailTimer = 0f;
        
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 240;
            Projectile.scale = 0.6f;
            
            orbitCount = 5; // 5 eighth notes
            orbitRadius = 25f;
        }
        
        public void Initialize(Color primary, Color secondary)
        {
            _primaryColor = primary;
            _secondaryColor = secondary;
        }
        
        protected override void UpdateMusicalBehavior()
        {
            trebleRotation += 0.12f;
            staffTrailTimer += 0.15f;
            
            Projectile.rotation = trebleRotation;
            
            // Sinusoidal staff line trail
            if (Projectile.timeLeft % 4 == 0)
            {
                float waveOffset = (float)Math.Sin(staffTrailTimer) * 8f;
                Vector2 perpendicular = Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2);
                Vector2 staffPos = Projectile.Center + perpendicular * waveOffset;
                
                // Draw 5 staff lines
                for (int line = -2; line <= 2; line++)
                {
                    Vector2 linePos = staffPos + perpendicular * (line * 4f);
                    var staffDust = Dust.NewDustPerfect(linePos, DustID.GoldCoin, -Projectile.velocity * 0.05f, 0, _primaryColor, 0.8f);
                    staffDust.noGravity = true;
                }
            }
        }
        
        protected override void SpawnOrbitingElements()
        {
            // Orbiting eighth notes at varying distances
            if (Projectile.timeLeft % 6 == 0)
            {
                for (int i = 0; i < orbitCount; i++)
                {
                    float angle = orbitAngle + MathHelper.TwoPi * i / orbitCount;
                    float radius = orbitRadius + (float)Math.Sin(pulseTimer + i) * 5f;
                    Vector2 orbitPos = Projectile.Center + angle.ToRotationVector2() * radius;
                    
                    // Visible music note (scale 0.7f+)
                    ThemedParticles.MusicNote(orbitPos, Projectile.velocity * 0.4f, 
                        Color.Lerp(_primaryColor, _secondaryColor, (float)i / orbitCount), 0.7f, 20);
                    
                    // Sparkle companion
                    CustomParticles.GenericFlare(orbitPos, _secondaryColor * 0.6f, 0.2f, 8);
                }
            }
        }
        
        public override void OnKill(int timeLeft)
        {
            // Notes scatter radially with harmonic tones
            SpawnMusicNoteImpact(Projectile.Center, 12);
            
            // Central treble clef burst
            EnhancedParticles.BloomFlare(Projectile.Center, Color.White, 0.6f, 20, 4, 1f);
            EnhancedParticles.BloomFlare(Projectile.Center, _primaryColor, 0.5f, 25, 3, 0.8f);
            
            // Expanding halo rings
            for (int i = 0; i < 4; i++)
            {
                Color ringColor = Color.Lerp(_primaryColor, _secondaryColor, i / 4f);
                CustomParticles.HaloRing(Projectile.Center, ringColor, 0.3f + i * 0.15f, 15 + i * 3);
            }
            
            SoundEngine.PlaySound(SoundID.Item26 with { Pitch = 0.3f }, Projectile.Center);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/TallMusicNote").Value;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2").Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            float pulse = coreScale + (float)Math.Sin(pulseTimer * 2f) * 0.1f;
            
            // Glow base
            Main.spriteBatch.Draw(glowTex, drawPos, null, (_primaryColor with { A = 0 }) * 0.4f, 
                0f, glowOrigin, 0.8f * pulse, SpriteEffects.None, 0f);
            
            // Spinning treble clef (represented by tall note)
            Main.spriteBatch.Draw(tex, drawPos, null, _primaryColor, 
                trebleRotation, origin, 0.5f * pulse, SpriteEffects.None, 0f);
            
            // Inner glow
            Main.spriteBatch.Draw(tex, drawPos, null, Color.White * 0.6f, 
                trebleRotation, origin, 0.3f * pulse, SpriteEffects.None, 0f);
            
            return false;
        }
    }
    
    /// <summary>
    /// #3 - Chromatic Scale Spiral: Rainbow notes spiraling around central beam
    /// </summary>
    public class ChromaticScaleSpiral : MusicalProjectileBase
    {
        public override string Texture => "MagnumOpus/Assets/Particles/EnergyFlare";
        
        public override Color PrimaryColor => Color.White;
        public override Color SecondaryColor => GetChromaticColor(spiralPhase);
        
        private float spiralPhase = 0f;
        private float[] noteAngles = new float[12]; // 12 chromatic tones
        
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 200;
            Projectile.scale = 0.5f;
            
            // Initialize note angles
            for (int i = 0; i < 12; i++)
            {
                noteAngles[i] = MathHelper.TwoPi * i / 12f;
            }
        }
        
        private Color GetChromaticColor(float phase)
        {
            // Cycle through rainbow based on phase
            float hue = (phase * 0.1f) % 1f;
            return Main.hslToRgb(hue, 1f, 0.7f);
        }
        
        protected override void UpdateMusicalBehavior()
        {
            spiralPhase += 0.08f;
            
            // Update note angles - spiral motion
            for (int i = 0; i < 12; i++)
            {
                noteAngles[i] += 0.05f + (i * 0.008f); // Slightly different speeds
            }
        }
        
        protected override void SpawnOrbitingElements()
        {
            // 12 chromatic notes spiraling
            if (Projectile.timeLeft % 5 == 0)
            {
                for (int i = 0; i < 12; i++)
                {
                    float radius = 18f + (float)Math.Sin(spiralPhase + i * 0.5f) * 8f;
                    Vector2 notePos = Projectile.Center + noteAngles[i].ToRotationVector2() * radius;
                    
                    // Each note a different hue
                    float noteHue = (float)i / 12f;
                    Color noteColor = Main.hslToRgb(noteHue, 1f, 0.7f);
                    
                    ThemedParticles.MusicNote(notePos, Projectile.velocity * 0.3f, noteColor, 0.65f, 15);
                }
            }
        }
        
        protected override void SpawnTrailParticles()
        {
            // Prismatic dust wake
            if (Main.rand.NextBool(2))
            {
                float hue = Main.rand.NextFloat();
                Color trailColor = Main.hslToRgb(hue, 1f, 0.75f);
                
                var sparkle = new SparkleParticle(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    -Projectile.velocity * 0.12f, trailColor, 0.4f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Central beam dust
            Dust beam = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowTorch, 
                -Projectile.velocity * 0.05f, 0, Color.White, 1.0f);
            beam.noGravity = true;
        }
        
        public override void OnKill(int timeLeft)
        {
            // Notes arrange into chord formation before exploding
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 chordPos = Projectile.Center + angle.ToRotationVector2() * 25f;
                
                float noteHue = (float)i / 12f;
                Color noteColor = Main.hslToRgb(noteHue, 1f, 0.7f);
                
                // Chord formation flash
                EnhancedParticles.BloomFlare(chordPos, noteColor, 0.3f, 15, 2, 0.6f);
                
                // Then burst outward
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                ThemedParticles.MusicNote(chordPos, burstVel, noteColor, 0.8f, 25);
            }
            
            // Central rainbow explosion
            EnhancedParticles.BloomFlare(Projectile.Center, Color.White, 0.8f, 25, 4, 1f);
            
            for (int ring = 0; ring < 6; ring++)
            {
                float ringHue = ring / 6f;
                Color ringColor = Main.hslToRgb(ringHue, 1f, 0.7f);
                CustomParticles.HaloRing(Projectile.Center, ringColor, 0.25f + ring * 0.12f, 12 + ring * 2);
            }
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            // Central white beam
            Main.spriteBatch.Draw(tex, drawPos, null, (Color.White with { A = 0 }) * 0.9f, 
                Projectile.rotation, origin, 0.4f * coreScale, SpriteEffects.None, 0f);
            
            // Rainbow glow layers
            for (int i = 0; i < 4; i++)
            {
                float hue = (spiralPhase * 0.1f + i * 0.25f) % 1f;
                Color layerColor = Main.hslToRgb(hue, 1f, 0.7f) with { A = 0 };
                float layerScale = 0.6f + i * 0.15f;
                float layerAlpha = 0.3f - i * 0.05f;
                
                Main.spriteBatch.Draw(tex, drawPos, null, layerColor * layerAlpha, 
                    spiralPhase + i * 0.5f, origin, layerScale * coreScale, SpriteEffects.None, 0f);
            }
            
            return false;
        }
    }
    
    /// <summary>
    /// #7 - Crescendo Swell: Grows larger and more powerful the further it travels
    /// </summary>
    public class CrescendoSwell : MusicalProjectileBase
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow2";
        
        private Color _baseColor;
        public override Color PrimaryColor => _baseColor;
        public override Color SecondaryColor => Color.Lerp(_baseColor, Color.White, growthProgress);
        
        private float growthProgress = 0f;
        private float travelDistance = 0f;
        private Vector2 startPosition;
        private string[] dynamicMarkings = { "pp", "p", "mp", "mf", "f", "ff", "fff" };
        private int currentDynamic = 0;
        
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.scale = 0.3f; // Starts small
        }
        
        public void Initialize(Color baseColor, Vector2 start)
        {
            _baseColor = baseColor;
            startPosition = start;
        }
        
        protected override void UpdateMusicalBehavior()
        {
            // Calculate growth based on distance traveled
            travelDistance = Vector2.Distance(Projectile.Center, startPosition);
            growthProgress = Math.Min(1f, travelDistance / 800f); // Max at 800 pixels
            
            // Scale increases with distance
            Projectile.scale = 0.3f + growthProgress * 0.7f;
            
            // Update dynamic marking
            int newDynamic = (int)(growthProgress * 6);
            if (newDynamic != currentDynamic && newDynamic < dynamicMarkings.Length)
            {
                currentDynamic = newDynamic;
                // Flash on dynamic change
                EnhancedParticles.BloomFlare(Projectile.Center, SecondaryColor, 0.3f + growthProgress * 0.3f, 12, 2, 0.5f);
            }
            
            // Damage scales with growth (handled by projectile damage modifier if needed)
        }
        
        protected override void SpawnTrailParticles()
        {
            // Expanding particle wake
            int particleCount = 1 + (int)(growthProgress * 3);
            
            for (int i = 0; i < particleCount; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(8f * Projectile.scale, 8f * Projectile.scale);
                Color trailColor = Color.Lerp(_baseColor, Color.White, growthProgress * 0.5f);
                
                var glow = new GenericGlowParticle(Projectile.Center + offset, 
                    -Projectile.velocity * 0.1f, trailColor * 0.6f, 0.2f + growthProgress * 0.2f, 18, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Dynamic marking particles trailing
            if (Projectile.timeLeft % 15 == 0 && currentDynamic < dynamicMarkings.Length)
            {
                // Spawn visual representation of current dynamic
                CustomParticles.GenericFlare(Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.Zero) * 20f,
                    SecondaryColor, 0.2f + currentDynamic * 0.08f, 20);
            }
        }
        
        protected override void SpawnOrbitingElements()
        {
            // More notes orbit as projectile grows
            int noteCount = 2 + (int)(growthProgress * 4);
            
            if (Projectile.timeLeft % 8 == 0)
            {
                for (int i = 0; i < noteCount; i++)
                {
                    float angle = orbitAngle + MathHelper.TwoPi * i / noteCount;
                    float radius = (15f + growthProgress * 20f);
                    Vector2 notePos = Projectile.Center + angle.ToRotationVector2() * radius;
                    
                    ThemedParticles.MusicNote(notePos, Projectile.velocity * 0.3f, SecondaryColor, 
                        0.5f + growthProgress * 0.4f, 15);
                }
            }
        }
        
        public override void OnKill(int timeLeft)
        {
            // Explosion size scales with growth
            float explosionScale = 0.5f + growthProgress * 1.5f;
            int particleCount = 8 + (int)(growthProgress * 16);
            
            // Massive fff explosion at max growth
            EnhancedParticles.BloomFlare(Projectile.Center, Color.White, explosionScale, 25, 4, 1f);
            EnhancedParticles.BloomFlare(Projectile.Center, _baseColor, explosionScale * 0.8f, 30, 3, 0.8f);
            
            // Radial note burst
            SpawnMusicNoteImpact(Projectile.Center, particleCount);
            
            // Cascading halos
            for (int i = 0; i < 6; i++)
            {
                float scale = (0.2f + i * 0.15f) * (1f + growthProgress);
                Color ringColor = Color.Lerp(_baseColor, Color.White, i / 6f);
                CustomParticles.HaloRing(Projectile.Center, ringColor, scale, 15 + i * 3);
            }
            
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = growthProgress * 0.5f, Volume = 0.7f + growthProgress * 0.5f }, Projectile.Center);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2").Value;
            Texture2D noteTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/WholeNote").Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 noteOrigin = noteTex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            float pulse = coreScale + (float)Math.Sin(pulseTimer * 2f) * 0.1f * (1f + growthProgress);
            
            // Growing glow
            Main.spriteBatch.Draw(tex, drawPos, null, (_baseColor with { A = 0 }) * (0.3f + growthProgress * 0.4f),
                0f, origin, Projectile.scale * 2f * pulse, SpriteEffects.None, 0f);
            
            // Core note that grows
            Main.spriteBatch.Draw(noteTex, drawPos, null, SecondaryColor,
                0f, noteOrigin, Projectile.scale * pulse, SpriteEffects.None, 0f);
            
            // White hot center at high growth
            if (growthProgress > 0.5f)
            {
                Main.spriteBatch.Draw(noteTex, drawPos, null, Color.White * (growthProgress - 0.5f) * 2f,
                    0f, noteOrigin, Projectile.scale * 0.6f * pulse, SpriteEffects.None, 0f);
            }
            
            return false;
        }
    }
    
    /// <summary>
    /// #9 - Tempo Metronome: Pendulum-swinging projectile that ticks damage
    /// </summary>
    public class TempoMetronome : MusicalProjectileBase
    {
        public override string Texture => "MagnumOpus/Assets/Particles/EnergyFlare";
        
        private Color _baseColor;
        public override Color PrimaryColor => _baseColor;
        public override Color SecondaryColor => Color.Lerp(_baseColor, Color.Gold, 0.3f);
        
        private float pendulumAngle = 0f;
        private float pendulumSpeed = 0.15f;
        private int tickCount = 0;
        private const int MaxTicks = 8;
        private bool swingingRight = true;
        
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.scale = 0.5f;
        }
        
        public void Initialize(Color baseColor)
        {
            _baseColor = baseColor;
        }
        
        protected override void UpdateMusicalBehavior()
        {
            // Pendulum swing
            float maxAngle = MathHelper.PiOver4;
            
            if (swingingRight)
            {
                pendulumAngle += pendulumSpeed;
                if (pendulumAngle >= maxAngle)
                {
                    swingingRight = false;
                    OnTick(); // Tick at apex
                }
            }
            else
            {
                pendulumAngle -= pendulumSpeed;
                if (pendulumAngle <= -maxAngle)
                {
                    swingingRight = true;
                    OnTick(); // Tick at apex
                }
            }
            
            // Apply pendulum offset to velocity direction
            Vector2 baseDirection = Projectile.velocity.SafeNormalize(Vector2.Zero);
            Vector2 swingOffset = baseDirection.RotatedBy(MathHelper.PiOver2) * (float)Math.Sin(pendulumAngle) * 3f;
            Projectile.Center += swingOffset;
        }
        
        private void OnTick()
        {
            tickCount++;
            
            // Tick visual
            EnhancedParticles.BloomFlare(Projectile.Center, SecondaryColor, 0.4f, 12, 2, 0.6f);
            
            // Spawn tick mark
            Vector2 tickPos = Projectile.Center + (swingingRight ? Vector2.UnitX : -Vector2.UnitX) * 15f;
            CustomParticles.GenericFlare(tickPos, Color.Gold, 0.25f, 15);
            
            // Tick sound
            SoundEngine.PlaySound(SoundID.Tink with { Pitch = 0.5f, Volume = 0.4f }, Projectile.Center);
            
            // Spawn mini damage pulses
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                // Could spawn tick damage projectiles here
            }
        }
        
        protected override void SpawnOrbitingElements()
        {
            // BPM display pulsing
            if (Projectile.timeLeft % 10 == 0)
            {
                float displayOffset = (float)Math.Sin(pulseTimer) * 5f;
                Vector2 displayPos = Projectile.Center + new Vector2(0, -20f + displayOffset);
                CustomParticles.GenericFlare(displayPos, Color.Gold * 0.5f, 0.15f, 8);
            }
        }
        
        public override void OnKill(int timeLeft)
        {
            // Ticking bomb explosion - pulses 4 times
            for (int pulse = 0; pulse < 4; pulse++)
            {
                // Staggered halos
                float delay = pulse * 0.25f;
                Color pulseColor = Color.Lerp(_baseColor, Color.Gold, pulse / 4f);
                CustomParticles.HaloRing(Projectile.Center, pulseColor, 0.3f + pulse * 0.15f, 12 + pulse * 4);
            }
            
            EnhancedParticles.BloomFlare(Projectile.Center, Color.Gold, 0.7f, 20, 4, 1f);
            SpawnMusicNoteImpact(Projectile.Center, 8);
            
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f }, Projectile.Center);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            // Pendulum body
            float visualAngle = Projectile.velocity.ToRotation() + pendulumAngle;
            
            // Metronome arm
            Vector2 armEnd = drawPos + visualAngle.ToRotationVector2() * 30f;
            Main.spriteBatch.Draw(tex, drawPos, null, (_baseColor with { A = 0 }) * 0.4f,
                visualAngle, origin, new Vector2(0.1f, 0.8f), SpriteEffects.None, 0f);
            
            // Weight at end
            Main.spriteBatch.Draw(tex, armEnd, null, (SecondaryColor with { A = 0 }) * 0.8f,
                0f, origin, 0.3f * coreScale, SpriteEffects.None, 0f);
            
            // Core
            Main.spriteBatch.Draw(tex, drawPos, null, (Color.Gold with { A = 0 }) * 0.6f,
                0f, origin, 0.2f * coreScale, SpriteEffects.None, 0f);
            
            return false;
        }
    }
    
    #endregion
    
    #region Tier 2: Instrument-Based Projectiles
    
    /// <summary>
    /// #16 - Symphony Conductor's Baton: Directs orbiting instruments to attack
    /// </summary>
    public class SymphonyConductorBaton : MusicalProjectileBase
    {
        public override string Texture => "MagnumOpus/Assets/Particles/EnergyFlare4";
        
        private Color _baseColor;
        public override Color PrimaryColor => _baseColor;
        public override Color SecondaryColor => Color.Gold;
        
        // Orbiting "instruments" - represented by different colored orbs
        private float[] instrumentAngles = new float[4];
        private Color[] instrumentColors;
        private int[] instrumentFireTimers = new int[4];
        
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 360;
            Projectile.scale = 0.6f;
            
            // Initialize instruments
            instrumentColors = new Color[]
            {
                new Color(139, 90, 43),  // Violin - brown
                new Color(218, 165, 32), // Trumpet - gold
                new Color(192, 192, 192), // Flute - silver
                new Color(101, 67, 33)   // Drum - dark brown
            };
            
            for (int i = 0; i < 4; i++)
            {
                instrumentAngles[i] = MathHelper.TwoPi * i / 4f;
                instrumentFireTimers[i] = 30 + i * 15; // Staggered firing
            }
        }
        
        public void Initialize(Color baseColor)
        {
            _baseColor = baseColor;
        }
        
        protected override void UpdateMusicalBehavior()
        {
            // Rotate instruments
            for (int i = 0; i < 4; i++)
            {
                instrumentAngles[i] += 0.03f + i * 0.005f;
                instrumentFireTimers[i]--;
                
                // Fire when timer reaches 0
                if (instrumentFireTimers[i] <= 0 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    FireInstrumentProjectile(i);
                    instrumentFireTimers[i] = 60 + Main.rand.Next(30); // Reset with variance
                }
            }
        }
        
        private void FireInstrumentProjectile(int instrumentIndex)
        {
            Vector2 instrumentPos = Projectile.Center + instrumentAngles[instrumentIndex].ToRotationVector2() * 35f;
            Player target = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
            
            if (target.active && !target.dead)
            {
                Vector2 toTarget = (target.Center - instrumentPos).SafeNormalize(Vector2.Zero);
                
                // Each instrument fires different themed projectile
                Color projColor = instrumentColors[instrumentIndex];
                BossProjectileHelper.SpawnHostileOrb(instrumentPos, toTarget * 8f, 40, projColor, 0.02f);
                
                // Fire flash
                EnhancedParticles.BloomFlare(instrumentPos, projColor, 0.35f, 12, 2, 0.5f);
                CustomParticles.GenericFlare(instrumentPos, Color.White * 0.7f, 0.2f, 8);
            }
        }
        
        protected override void SpawnOrbitingElements()
        {
            // Draw instrument positions with light trails
            for (int i = 0; i < 4; i++)
            {
                Vector2 instPos = Projectile.Center + instrumentAngles[i].ToRotationVector2() * 35f;
                
                // Instrument glow
                if (Projectile.timeLeft % 4 == 0)
                {
                    var glow = new GenericGlowParticle(instPos, Vector2.Zero, 
                        instrumentColors[i] * 0.5f, 0.2f, 10, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
                
                // Light trail connecting to baton
                if (Projectile.timeLeft % 8 == 0)
                {
                    Vector2 midPoint = Vector2.Lerp(Projectile.Center, instPos, 0.5f);
                    CustomParticles.GenericFlare(midPoint, Color.Gold * 0.3f, 0.1f, 6);
                }
            }
        }
        
        public override void OnKill(int timeLeft)
        {
            // All instruments play finale
            for (int i = 0; i < 4; i++)
            {
                Vector2 instPos = Projectile.Center + instrumentAngles[i].ToRotationVector2() * 35f;
                EnhancedParticles.BloomFlare(instPos, instrumentColors[i], 0.5f, 18, 3, 0.7f);
                
                // Radial burst from each instrument
                for (int j = 0; j < 4; j++)
                {
                    float angle = MathHelper.TwoPi * j / 4f;
                    Vector2 burstVel = angle.ToRotationVector2() * 5f;
                    ThemedParticles.MusicNote(instPos, burstVel, instrumentColors[i], 0.7f, 20);
                }
            }
            
            // Central conductor finale
            EnhancedParticles.BloomFlare(Projectile.Center, Color.Gold, 0.8f, 25, 4, 1f);
            SpawnMusicNoteImpact(Projectile.Center, 12);
            
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.4f, Volume = 1.2f }, Projectile.Center);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2").Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            // Baton glow
            Main.spriteBatch.Draw(glowTex, drawPos, null, (Color.Gold with { A = 0 }) * 0.5f,
                0f, glowOrigin, 0.7f * coreScale, SpriteEffects.None, 0f);
            
            // Baton tip
            Main.spriteBatch.Draw(tex, drawPos, null, (Color.White with { A = 0 }) * 0.8f,
                Projectile.rotation, origin, 0.3f * coreScale, SpriteEffects.None, 0f);
            
            // Draw orbiting instruments
            for (int i = 0; i < 4; i++)
            {
                Vector2 instPos = Projectile.Center + instrumentAngles[i].ToRotationVector2() * 35f - Main.screenPosition;
                Color instColor = instrumentColors[i] with { A = 0 };
                
                // Instrument glow
                Main.spriteBatch.Draw(glowTex, instPos, null, instColor * 0.6f,
                    0f, glowOrigin, 0.3f, SpriteEffects.None, 0f);
                
                // Instrument core
                Main.spriteBatch.Draw(tex, instPos, null, instColor * 0.9f,
                    instrumentAngles[i], origin, 0.2f, SpriteEffects.None, 0f);
            }
            
            return false;
        }
    }
    
    /// <summary>
    /// #17 - Vinyl Record Disc: Spinning record that shoots sound wave slices
    /// </summary>
    public class VinylRecordDisc : MusicalProjectileBase
    {
        public override string Texture => "MagnumOpus/Assets/Particles/GlowingHalo1";
        
        public override Color PrimaryColor => new Color(20, 20, 25);
        public override Color SecondaryColor => Color.Gold;
        
        private float discRotation = 0f;
        private float rotationsCompleted = 0f;
        private const float DegreesPerSlice = 90f; // Fire every 90 degrees
        
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 240;
            Projectile.scale = 0.7f;
        }
        
        protected override void UpdateMusicalBehavior()
        {
            float previousRotation = discRotation;
            discRotation += 0.2f; // Fast spin
            
            // Check if we crossed a slice threshold
            float prevDegrees = MathHelper.ToDegrees(previousRotation) % 360f;
            float currDegrees = MathHelper.ToDegrees(discRotation) % 360f;
            
            // Fire on every 90-degree mark
            for (float threshold = 0; threshold < 360; threshold += DegreesPerSlice)
            {
                if (prevDegrees < threshold && currDegrees >= threshold)
                {
                    FireSoundWaveSlice(threshold);
                }
            }
            
            Projectile.rotation = discRotation;
        }
        
        private void FireSoundWaveSlice(float angleDegrees)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            
            float angleRad = MathHelper.ToRadians(angleDegrees);
            Vector2 sliceDir = angleRad.ToRotationVector2();
            
            // Fire groove wave
            BossProjectileHelper.SpawnWaveProjectile(Projectile.Center, sliceDir * 10f, 35, SecondaryColor, 2f);
            
            // Groove flash
            Vector2 flashPos = Projectile.Center + sliceDir * 20f;
            EnhancedParticles.BloomFlare(flashPos, SecondaryColor, 0.3f, 10, 2, 0.5f);
        }
        
        protected override void SpawnTrailParticles()
        {
            // Groove lines emanating
            if (Projectile.timeLeft % 3 == 0)
            {
                float grooveAngle = discRotation + Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 groovePos = Projectile.Center + grooveAngle.ToRotationVector2() * 15f;
                
                Dust groove = Dust.NewDustPerfect(groovePos, DustID.GoldCoin, 
                    grooveAngle.ToRotationVector2() * 2f, 0, SecondaryColor, 0.6f);
                groove.noGravity = true;
            }
        }
        
        protected override void SpawnOrbitingElements()
        {
            // Album label glow in center
            if (Projectile.timeLeft % 10 == 0)
            {
                EnhancedParticles.BloomFlare(Projectile.Center, Color.Red * 0.5f, 0.15f, 8, 2, 0.3f);
            }
        }
        
        public override void OnKill(int timeLeft)
        {
            // Record shatter
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 shardVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                
                // Vinyl shards
                var shard = new GlowSparkParticle(Projectile.Center, shardVel, true, 25, 0.3f, 
                    PrimaryColor, new Vector2(0.02f, 1.5f));
                MagnumParticleHandler.SpawnParticle(shard);
                
                // Gold groove dust
                Dust grooveDust = Dust.NewDustPerfect(Projectile.Center, DustID.GoldCoin, shardVel * 0.8f, 0, SecondaryColor, 1f);
                grooveDust.noGravity = true;
            }
            
            EnhancedParticles.BloomFlare(Projectile.Center, SecondaryColor, 0.6f, 20, 3, 0.8f);
            SpawnMusicNoteImpact(Projectile.Center, 8);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D discTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/GlowingHalo1").Value;
            Texture2D labelTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Vector2 discOrigin = discTex.Size() / 2f;
            Vector2 labelOrigin = labelTex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            // Vinyl disc (black with golden grooves)
            Main.spriteBatch.Draw(discTex, drawPos, null, (PrimaryColor with { A = 0 }) * 0.9f,
                discRotation, discOrigin, Projectile.scale, SpriteEffects.None, 0f);
            
            // Golden groove highlights
            Main.spriteBatch.Draw(discTex, drawPos, null, (SecondaryColor with { A = 0 }) * 0.3f,
                discRotation * 0.5f, discOrigin, Projectile.scale * 0.85f, SpriteEffects.None, 0f);
            
            // Red center label
            Main.spriteBatch.Draw(labelTex, drawPos, null, (Color.DarkRed with { A = 0 }) * 0.8f,
                0f, labelOrigin, 0.15f, SpriteEffects.None, 0f);
            
            return false;
        }
    }
    
    #endregion
    
    #region Helper Methods for Spawning Phase 10 Projectiles
    
    public static class Phase10ProjectileHelper
    {
        /// <summary>
        /// Spawns a Resonating Treble Orb with orbiting eighth notes
        /// </summary>
        public static int SpawnResonatingTrebleOrb(Vector2 position, Vector2 velocity, int damage, Color primary, Color secondary)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return -1;
            
            int proj = Projectile.NewProjectile(null, position, velocity, 
                ModContent.ProjectileType<ResonatingTrebleOrb>(), damage, 0f, Main.myPlayer);
            
            if (proj >= 0 && proj < Main.maxProjectiles && Main.projectile[proj].ModProjectile is ResonatingTrebleOrb treble)
            {
                treble.Initialize(primary, secondary);
            }
            
            return proj;
        }
        
        /// <summary>
        /// Spawns a Chromatic Scale Spiral with 12 rainbow notes
        /// </summary>
        public static int SpawnChromaticScaleSpiral(Vector2 position, Vector2 velocity, int damage)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return -1;
            
            return Projectile.NewProjectile(null, position, velocity,
                ModContent.ProjectileType<ChromaticScaleSpiral>(), damage, 0f, Main.myPlayer);
        }
        
        /// <summary>
        /// Spawns a Crescendo Swell that grows as it travels
        /// </summary>
        public static int SpawnCrescendoSwell(Vector2 position, Vector2 velocity, int damage, Color baseColor)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return -1;
            
            int proj = Projectile.NewProjectile(null, position, velocity,
                ModContent.ProjectileType<CrescendoSwell>(), damage, 0f, Main.myPlayer);
            
            if (proj >= 0 && proj < Main.maxProjectiles && Main.projectile[proj].ModProjectile is CrescendoSwell crescendo)
            {
                crescendo.Initialize(baseColor, position);
            }
            
            return proj;
        }
        
        /// <summary>
        /// Spawns a Tempo Metronome that swings and ticks damage
        /// </summary>
        public static int SpawnTempoMetronome(Vector2 position, Vector2 velocity, int damage, Color baseColor)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return -1;
            
            int proj = Projectile.NewProjectile(null, position, velocity,
                ModContent.ProjectileType<TempoMetronome>(), damage, 0f, Main.myPlayer);
            
            if (proj >= 0 && proj < Main.maxProjectiles && Main.projectile[proj].ModProjectile is TempoMetronome metro)
            {
                metro.Initialize(baseColor);
            }
            
            return proj;
        }
        
        /// <summary>
        /// Spawns a Symphony Conductor's Baton with orbiting instruments
        /// </summary>
        public static int SpawnSymphonyConductorBaton(Vector2 position, Vector2 velocity, int damage, Color baseColor)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return -1;
            
            int proj = Projectile.NewProjectile(null, position, velocity,
                ModContent.ProjectileType<SymphonyConductorBaton>(), damage, 0f, Main.myPlayer);
            
            if (proj >= 0 && proj < Main.maxProjectiles && Main.projectile[proj].ModProjectile is SymphonyConductorBaton baton)
            {
                baton.Initialize(baseColor);
            }
            
            return proj;
        }
        
        /// <summary>
        /// Spawns a Vinyl Record Disc that fires sound wave slices
        /// </summary>
        public static int SpawnVinylRecordDisc(Vector2 position, Vector2 velocity, int damage)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return -1;
            
            return Projectile.NewProjectile(null, position, velocity,
                ModContent.ProjectileType<VinylRecordDisc>(), damage, 0f, Main.myPlayer);
        }
    }
    
    #endregion
}
