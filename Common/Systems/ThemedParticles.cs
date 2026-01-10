using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// Themed particle effect helpers for Moonlight Sonata and Eroica content.
    /// Provides easy-to-use methods that spawn the new custom particles with correct color palettes.
    /// 
    /// MOONLIGHT PALETTE: Dark purple (#4B0082) → Light blue (#87CEEB) with silver/white accents
    /// EROICA PALETTE: Deep scarlet (#8B0000) → Gold (#FFD700) with black accents
    /// </summary>
    public static class ThemedParticles
    {
        #region Moonlight Sonata Color Palette
        
        /// <summary>Dark purple - primary moonlight color</summary>
        public static readonly Color MoonlightDarkPurple = new Color(75, 0, 130);
        /// <summary>Medium purple - transition color</summary>
        public static readonly Color MoonlightMediumPurple = new Color(138, 43, 226);
        /// <summary>Light purple/lavender</summary>
        public static readonly Color MoonlightLightPurple = new Color(180, 150, 255);
        /// <summary>Light blue accent</summary>
        public static readonly Color MoonlightLightBlue = new Color(135, 206, 250);
        /// <summary>Ice blue accent</summary>
        public static readonly Color MoonlightIceBlue = new Color(200, 230, 255);
        /// <summary>Silver accent</summary>
        public static readonly Color MoonlightSilver = new Color(220, 220, 235);
        /// <summary>White core color</summary>
        public static readonly Color MoonlightWhite = new Color(240, 235, 255);
        
        #endregion
        
        #region Eroica Color Palette
        
        /// <summary>Deep scarlet red - primary eroica color</summary>
        public static readonly Color EroicaScarlet = new Color(139, 0, 0);
        /// <summary>Bright crimson</summary>
        public static readonly Color EroicaCrimson = new Color(220, 50, 50);
        /// <summary>Orange-red fire color</summary>
        public static readonly Color EroicaFlame = new Color(255, 100, 50);
        /// <summary>Gold accent - heroic/triumphant</summary>
        public static readonly Color EroicaGold = new Color(255, 215, 0);
        /// <summary>Light gold/amber</summary>
        public static readonly Color EroicaAmber = new Color(255, 191, 100);
        /// <summary>Black smoke accent</summary>
        public static readonly Color EroicaBlack = new Color(30, 20, 25);
        /// <summary>Pink sakura accent</summary>
        public static readonly Color EroicaSakura = new Color(255, 150, 180);
        
        #endregion
        
        #region Musical Note Colors
        
        /// <summary>Bright golden note color</summary>
        public static readonly Color MusicGold = new Color(255, 220, 100);
        /// <summary>Ethereal silver note color</summary>
        public static readonly Color MusicSilver = new Color(230, 230, 255);
        /// <summary>Warm amber note color</summary>
        public static readonly Color MusicAmber = new Color(255, 180, 80);
        
        #endregion
        
        #region Moonlight Sonata Particle Effects
        
        /// <summary>
        /// Creates a moonlight-themed bloom burst - dark purple fading to light blue.
        /// Use for projectile impacts, magical bursts, lunar effects.
        /// </summary>
        public static void MoonlightBloomBurst(Vector2 position, float intensity = 1f)
        {
            int count = (int)(6 * intensity);
            
            // Dark purple core blooms
            for (int i = 0; i < count; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(2f, 2f) * intensity;
                var bloom = new BloomParticle(position, velocity, MoonlightDarkPurple, 
                    0.4f * intensity, 0.8f * intensity, Main.rand.Next(25, 45));
                MagnumParticleHandler.SpawnParticle(bloom);
            }
            
            // Light blue outer blooms
            for (int i = 0; i < count / 2; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(3f, 3f) * intensity;
                var bloom = new BloomParticle(position, velocity, MoonlightLightBlue * 0.7f, 
                    0.3f * intensity, 0.6f * intensity, Main.rand.Next(30, 50));
                MagnumParticleHandler.SpawnParticle(bloom);
            }
        }
        
        /// <summary>
        /// Creates moonlight sparkles - silver and light purple twinkling stars.
        /// Use for ambient magical effects, enchantment visuals, ethereal auras.
        /// </summary>
        public static void MoonlightSparkles(Vector2 position, int count = 8, float spreadRadius = 30f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(spreadRadius, spreadRadius);
                Vector2 velocity = Main.rand.NextVector2Circular(1f, 1f);
                Color color = Main.rand.NextBool() ? MoonlightSilver : MoonlightLightPurple;
                Color bloom = MoonlightDarkPurple * 0.6f;
                
                var sparkle = new SparkleParticle(position + offset, velocity, color, bloom, 
                    Main.rand.NextFloat(0.4f, 0.9f), Main.rand.Next(40, 80), 0.06f, 1.2f);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }
        
        /// <summary>
        /// Creates a moonlight shockwave ring - expanding purple to blue ring.
        /// Use for teleportation effects, wave attacks, pulse abilities.
        /// </summary>
        public static void MoonlightShockwave(Vector2 position, float scale = 1f)
        {
            // Inner purple ring
            var innerRing = new BloomRingParticle(position, Vector2.Zero, MoonlightDarkPurple * 0.8f, 
                0.3f * scale, 35, 0.08f);
            MagnumParticleHandler.SpawnParticle(innerRing);
            
            // Outer blue ring (slightly delayed by starting smaller)
            var outerRing = new BloomRingParticle(position, Vector2.Zero, MoonlightLightBlue * 0.5f, 
                0.2f * scale, 45, 0.1f);
            MagnumParticleHandler.SpawnParticle(outerRing);
        }
        
        /// <summary>
        /// Creates moonlight energy sparks - directional purple/silver sparks.
        /// Use for sword slashes, projectile trails, hit effects.
        /// </summary>
        public static void MoonlightSparks(Vector2 position, Vector2 direction, int count = 6, float speed = 5f)
        {
            float baseAngle = direction.ToRotation();
            
            for (int i = 0; i < count; i++)
            {
                float angle = baseAngle + Main.rand.NextFloat(-0.6f, 0.6f);
                float sparkSpeed = Main.rand.NextFloat(speed * 0.6f, speed * 1.2f);
                Vector2 velocity = angle.ToRotationVector2() * sparkSpeed;
                
                Color color = i % 2 == 0 ? MoonlightLightPurple : MoonlightSilver;
                var spark = new GlowSparkParticle(position, velocity, false, 
                    Main.rand.Next(20, 35), Main.rand.NextFloat(0.3f, 0.6f), color,
                    new Vector2(0.4f, 1.8f), false, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
        }
        
        /// <summary>
        /// Creates moonlight impact effect - combined bloom, ring, and sparks.
        /// Use for major hits, projectile explosions, ability triggers.
        /// </summary>
        public static void MoonlightImpact(Vector2 position, float intensity = 1f)
        {
            // Central bloom
            MoonlightBloomBurst(position, intensity);
            
            // Shockwave
            MoonlightShockwave(position, intensity);
            
            // Radial sparks
            for (int i = 0; i < (int)(8 * intensity); i++)
            {
                float angle = MathHelper.TwoPi * i / (8 * intensity);
                Vector2 dir = angle.ToRotationVector2();
                MoonlightSparks(position, dir, 2, 4f * intensity);
            }
            
            // Ambient sparkles
            MoonlightSparkles(position, (int)(10 * intensity), 40f * intensity);
            
            // Light burst
            Lighting.AddLight(position, MoonlightMediumPurple.ToVector3() * 0.8f * intensity);
        }
        
        /// <summary>
        /// Creates moonlight trail particles for projectiles.
        /// Use in AI() methods for continuous trailing effect.
        /// </summary>
        public static void MoonlightTrail(Vector2 position, Vector2 velocity)
        {
            if (Main.rand.NextBool(2))
            {
                var glow = new GenericGlowParticle(position + Main.rand.NextVector2Circular(6f, 6f), 
                    -velocity * 0.1f, MoonlightDarkPurple, Main.rand.NextFloat(0.2f, 0.4f), 
                    Main.rand.Next(15, 30), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            if (Main.rand.NextBool(4))
            {
                var sparkle = new SparkleParticle(position, -velocity * 0.05f + Main.rand.NextVector2Circular(0.5f, 0.5f), 
                    MoonlightSilver, MoonlightLightBlue * 0.5f, Main.rand.NextFloat(0.3f, 0.6f), 
                    Main.rand.Next(20, 40), 0.04f, 1f);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }
        
        /// <summary>
        /// Creates moonlight ambient aura particles around an entity.
        /// Use in UpdateAccessory or AI for persistent visual effects.
        /// </summary>
        public static void MoonlightAura(Vector2 center, float radius = 40f)
        {
            if (Main.rand.NextBool(8))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 offset = angle.ToRotationVector2() * Main.rand.NextFloat(radius * 0.5f, radius);
                Vector2 velocity = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-1f, -0.3f));
                
                Color color = Main.rand.NextBool(3) ? MoonlightSilver : MoonlightLightPurple;
                var glow = new GenericGlowParticle(center + offset, velocity, color, 
                    Main.rand.NextFloat(0.15f, 0.35f), Main.rand.Next(40, 70), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
        }
        
        #endregion
        
        #region Eroica Particle Effects
        
        /// <summary>
        /// Creates an eroica-themed bloom burst - scarlet with gold cores.
        /// Use for fire explosions, heroic bursts, triumphant effects.
        /// </summary>
        public static void EroicaBloomBurst(Vector2 position, float intensity = 1f)
        {
            int count = (int)(6 * intensity);
            
            // Scarlet fire blooms
            for (int i = 0; i < count; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(2.5f, 2.5f) * intensity;
                var bloom = new BloomParticle(position, velocity, EroicaCrimson, 
                    0.4f * intensity, 0.9f * intensity, Main.rand.Next(20, 40));
                MagnumParticleHandler.SpawnParticle(bloom);
            }
            
            // Gold heroic core blooms
            for (int i = 0; i < count / 2; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(2f, 2f) * intensity;
                var bloom = new BloomParticle(position, velocity, EroicaGold, 
                    0.25f * intensity, 0.5f * intensity, Main.rand.Next(15, 30));
                MagnumParticleHandler.SpawnParticle(bloom);
            }
        }
        
        /// <summary>
        /// Creates eroica sparkles - gold and flame colored sparks.
        /// Use for heroic effects, triumphant visuals, valor displays.
        /// </summary>
        public static void EroicaSparkles(Vector2 position, int count = 8, float spreadRadius = 30f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(spreadRadius, spreadRadius);
                Vector2 velocity = Main.rand.NextVector2Circular(1f, 1f) + new Vector2(0, -0.5f);
                Color color = Main.rand.NextBool() ? EroicaGold : EroicaAmber;
                Color bloom = EroicaCrimson * 0.5f;
                
                var sparkle = new SparkleParticle(position + offset, velocity, color, bloom, 
                    Main.rand.NextFloat(0.4f, 0.9f), Main.rand.Next(35, 70), 0.05f, 1.3f);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }
        
        /// <summary>
        /// Creates an eroica shockwave ring - crimson fire expanding ring.
        /// Use for explosions, heroic abilities, fire waves.
        /// </summary>
        public static void EroicaShockwave(Vector2 position, float scale = 1f)
        {
            // Inner crimson ring
            var innerRing = new BloomRingParticle(position, Vector2.Zero, EroicaCrimson * 0.9f, 
                0.35f * scale, 30, 0.1f);
            MagnumParticleHandler.SpawnParticle(innerRing);
            
            // Outer gold ring
            var outerRing = new BloomRingParticle(position, Vector2.Zero, EroicaGold * 0.5f, 
                0.25f * scale, 40, 0.12f);
            MagnumParticleHandler.SpawnParticle(outerRing);
        }
        
        /// <summary>
        /// Creates eroica fire sparks - directional flame particles.
        /// Use for sword slashes, fire trails, combat effects.
        /// </summary>
        public static void EroicaSparks(Vector2 position, Vector2 direction, int count = 6, float speed = 6f)
        {
            float baseAngle = direction.ToRotation();
            
            for (int i = 0; i < count; i++)
            {
                float angle = baseAngle + Main.rand.NextFloat(-0.5f, 0.5f);
                float sparkSpeed = Main.rand.NextFloat(speed * 0.7f, speed * 1.3f);
                Vector2 velocity = angle.ToRotationVector2() * sparkSpeed;
                
                Color color = i % 3 == 0 ? EroicaGold : (i % 3 == 1 ? EroicaCrimson : EroicaFlame);
                var spark = new GlowSparkParticle(position, velocity, true, 
                    Main.rand.Next(25, 45), Main.rand.NextFloat(0.35f, 0.7f), color,
                    new Vector2(0.5f, 1.6f), false, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
        }
        
        /// <summary>
        /// Creates eroica impact effect - fiery explosion with gold sparks.
        /// Use for major hits, explosions, triumphant abilities.
        /// </summary>
        public static void EroicaImpact(Vector2 position, float intensity = 1f)
        {
            // Central bloom
            EroicaBloomBurst(position, intensity);
            
            // Shockwave
            EroicaShockwave(position, intensity);
            
            // Radial fire sparks
            for (int i = 0; i < (int)(10 * intensity); i++)
            {
                float angle = MathHelper.TwoPi * i / (10 * intensity);
                Vector2 dir = angle.ToRotationVector2();
                EroicaSparks(position, dir, 2, 5f * intensity);
            }
            
            // Gold sparkles
            EroicaSparkles(position, (int)(8 * intensity), 35f * intensity);
            
            // Black smoke puffs
            for (int i = 0; i < (int)(4 * intensity); i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(3f, 3f) + new Vector2(0, -1f);
                var smoke = new HeavySmokeParticle(position + Main.rand.NextVector2Circular(15f, 15f), 
                    velocity, EroicaBlack, Main.rand.Next(40, 70), Main.rand.NextFloat(0.4f, 0.7f) * intensity, 
                    0.6f, 0.02f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Light burst
            Lighting.AddLight(position, EroicaCrimson.ToVector3() * 0.9f * intensity);
        }
        
        /// <summary>
        /// Creates eroica trail particles for projectiles.
        /// Use in AI() methods for continuous fiery trailing effect.
        /// </summary>
        public static void EroicaTrail(Vector2 position, Vector2 velocity)
        {
            if (Main.rand.NextBool(2))
            {
                Vector2 offset = Main.rand.NextVector2Circular(6f, 6f);
                var glow = new GenericGlowParticle(position + offset, -velocity * 0.15f + new Vector2(0, -0.5f), 
                    EroicaCrimson, Main.rand.NextFloat(0.2f, 0.45f), Main.rand.Next(15, 30), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            if (Main.rand.NextBool(3))
            {
                var spark = new GlowSparkParticle(position, -velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f), 
                    true, Main.rand.Next(15, 25), Main.rand.NextFloat(0.2f, 0.4f), 
                    Main.rand.NextBool() ? EroicaFlame : EroicaGold, new Vector2(0.4f, 1.4f), true, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // Occasional smoke
            if (Main.rand.NextBool(8))
            {
                var smoke = new HeavySmokeParticle(position, -velocity * 0.05f, EroicaBlack, 
                    Main.rand.Next(30, 50), Main.rand.NextFloat(0.2f, 0.4f), 0.4f, 0.01f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
        }
        
        /// <summary>
        /// Creates eroica ambient aura particles - rising embers and flames.
        /// Use in UpdateAccessory or AI for persistent fire effects.
        /// </summary>
        public static void EroicaAura(Vector2 center, float radius = 40f)
        {
            // Rising ember
            if (Main.rand.NextBool(6))
            {
                Vector2 offset = Main.rand.NextVector2Circular(radius, radius * 0.5f);
                offset.Y += radius * 0.3f; // Start lower
                Vector2 velocity = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-2f, -1f));
                
                Color color = Main.rand.NextBool(3) ? EroicaGold : EroicaCrimson;
                var glow = new GenericGlowParticle(center + offset, velocity, color, 
                    Main.rand.NextFloat(0.15f, 0.35f), Main.rand.Next(35, 60), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Occasional spark
            if (Main.rand.NextBool(12))
            {
                Vector2 offset = Main.rand.NextVector2Circular(radius * 0.8f, radius * 0.8f);
                Vector2 velocity = Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0, -1f);
                
                var spark = new GlowSparkParticle(center + offset, velocity, true, 
                    Main.rand.Next(20, 35), Main.rand.NextFloat(0.2f, 0.4f), EroicaFlame,
                    new Vector2(0.3f, 1.2f), true, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
        }
        
        /// <summary>
        /// Creates sakura petal particles - pink floating petals for Eroica aesthetic.
        /// Use for sakura-themed items and abilities.
        /// </summary>
        public static void SakuraPetals(Vector2 position, int count = 5, float spreadRadius = 40f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(spreadRadius, spreadRadius);
                Vector2 velocity = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-1f, 0.5f));
                
                var petal = new GenericGlowParticle(position + offset, velocity, EroicaSakura, 
                    Main.rand.NextFloat(0.25f, 0.5f), Main.rand.Next(50, 90), false);
                MagnumParticleHandler.SpawnParticle(petal);
            }
        }
        
        #endregion
        
        #region Special Combined Effects
        
        /// <summary>
        /// Creates a dodge/dash trail effect for wings.
        /// </summary>
        public static void DodgeTrail(Vector2 position, Vector2 velocity, bool isMoonlight)
        {
            if (isMoonlight)
            {
                // Purple/blue energy trail
                for (int i = 0; i < 3; i++)
                {
                    Vector2 offset = Main.rand.NextVector2Circular(10f, 10f);
                    var glow = new GenericGlowParticle(position + offset, -velocity * 0.2f, 
                        i == 0 ? MoonlightDarkPurple : MoonlightLightBlue, 
                        Main.rand.NextFloat(0.3f, 0.6f), Main.rand.Next(10, 20), true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
            }
            else
            {
                // Scarlet/gold fire trail
                for (int i = 0; i < 3; i++)
                {
                    Vector2 offset = Main.rand.NextVector2Circular(10f, 10f);
                    var glow = new GenericGlowParticle(position + offset, -velocity * 0.2f + new Vector2(0, -0.5f), 
                        i == 0 ? EroicaCrimson : EroicaGold, 
                        Main.rand.NextFloat(0.3f, 0.6f), Main.rand.Next(10, 20), true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
            }
        }
        
        /// <summary>
        /// Creates a teleport/warp burst effect.
        /// </summary>
        public static void TeleportBurst(Vector2 position, bool isMoonlight)
        {
            if (isMoonlight)
            {
                MoonlightShockwave(position, 1.5f);
                MoonlightSparkles(position, 15, 50f);
                
                // Imploding particles
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi * i / 12f;
                    Vector2 startPos = position + angle.ToRotationVector2() * 60f;
                    Vector2 velocity = (position - startPos).SafeNormalize(Vector2.Zero) * 8f;
                    
                    var spark = new GlowSparkParticle(startPos, velocity, false, 15, 0.5f, 
                        MoonlightLightPurple, new Vector2(0.3f, 2f), true, true);
                    MagnumParticleHandler.SpawnParticle(spark);
                }
            }
            else
            {
                EroicaShockwave(position, 1.5f);
                EroicaSparkles(position, 12, 50f);
                
                // Fire burst outward
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi * i / 12f;
                    Vector2 velocity = angle.ToRotationVector2() * 6f;
                    
                    var spark = new GlowSparkParticle(position, velocity, true, 20, 0.5f, 
                        EroicaFlame, new Vector2(0.4f, 1.8f), false, true);
                    MagnumParticleHandler.SpawnParticle(spark);
                }
                
                // Black smoke
                for (int i = 0; i < 6; i++)
                {
                    var smoke = new HeavySmokeParticle(position, Main.rand.NextVector2Circular(4f, 4f), 
                        EroicaBlack, 50, 0.5f, 0.5f, 0.02f, false);
                    MagnumParticleHandler.SpawnParticle(smoke);
                }
            }
        }
        
        #endregion
        
        #region Moonlight Musical Effects
        
        /// <summary>
        /// Creates floating music notes with Moonlight Sonata theming.
        /// Notes drift upward with gentle wobble, perfect for ambient or impact effects.
        /// </summary>
        /// <param name="position">Center position for spawning</param>
        /// <param name="count">Number of notes to spawn</param>
        /// <param name="spreadRadius">Spawn area radius</param>
        public static void MoonlightMusicNotes(Vector2 position, int count = 5, float spreadRadius = 30f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(spreadRadius, spreadRadius);
                Vector2 velocity = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-2f, -1f));
                
                // Moonlight colors - purples and silvers
                Color color = Main.rand.Next(4) switch
                {
                    0 => MoonlightDarkPurple,
                    1 => MoonlightMediumPurple,
                    2 => MoonlightLightBlue,
                    _ => MoonlightSilver
                };
                
                // Random note type is handled by the simple constructor
                float scale = Main.rand.NextFloat(0.4f, 0.8f);
                int lifetime = Main.rand.Next(60, 100);
                
                var note = new MusicNoteParticle(position + offset, velocity, color, scale, lifetime);
                MagnumParticleHandler.SpawnParticle(note);
            }
        }
        
        /// <summary>
        /// Creates a dramatic clef symbol for Moonlight effects.
        /// Treble or bass clef that expands and fades - use for major ability activations.
        /// </summary>
        /// <param name="position">Position for the clef</param>
        /// <param name="useTrebleClef">True for treble clef, false for bass clef</param>
        /// <param name="scale">Base scale multiplier</param>
        public static void MoonlightClef(Vector2 position, bool useTrebleClef = true, float scale = 1f)
        {
            Color color = MoonlightLightPurple;
            Color bloomColor = MoonlightDarkPurple * 0.6f;
            var clef = new ClefParticle(position, Vector2.Zero, color, bloomColor, scale, 80, useTrebleClef);
            MagnumParticleHandler.SpawnParticle(clef);
            
            // Accompanying sparkles
            MoonlightSparkles(position, 6, 30f * scale);
        }
        
        /// <summary>
        /// Creates glowing music staff lines for Moonlight effects.
        /// Five horizontal lines that shimmer - great for spell casting or charging.
        /// </summary>
        /// <param name="position">Center position of the staff</param>
        /// <param name="scale">Scale multiplier</param>
        public static void MoonlightMusicStaff(Vector2 position, float scale = 1f)
        {
            var staff = new MusicStaffParticle(position, Vector2.Zero, MoonlightLightBlue * 0.8f, scale * 0.8f, 60);
            MagnumParticleHandler.SpawnParticle(staff);
        }
        
        /// <summary>
        /// Creates sharp or flat accidental symbols for Moonlight effects.
        /// Small musical accidentals that drift - use for magical accents.
        /// </summary>
        /// <param name="position">Center spawn position</param>
        /// <param name="count">Number of accidentals</param>
        /// <param name="spreadRadius">Spawn area radius</param>
        public static void MoonlightAccidentals(Vector2 position, int count = 3, float spreadRadius = 20f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(spreadRadius, spreadRadius);
                Vector2 velocity = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-1.5f, -0.5f));
                
                Color color = Main.rand.NextBool() ? MoonlightSilver : MoonlightLightPurple;
                bool isSharp = Main.rand.NextBool();
                
                var accidental = new AccidentalParticle(position + offset, velocity, color,
                    Main.rand.NextFloat(0.4f, 0.7f), Main.rand.Next(50, 80), isSharp);
                MagnumParticleHandler.SpawnParticle(accidental);
            }
        }
        
        /// <summary>
        /// Creates a full musical impact effect for Moonlight Sonata.
        /// Combines notes, sparkles, and optional clef for dramatic moments.
        /// </summary>
        /// <param name="position">Impact center</param>
        /// <param name="intensity">Effect intensity multiplier</param>
        /// <param name="includeClef">Whether to spawn a clef symbol</param>
        public static void MoonlightMusicalImpact(Vector2 position, float intensity = 1f, bool includeClef = false)
        {
            // Burst of music notes
            MoonlightMusicNotes(position, (int)(8 * intensity), 40f * intensity);
            
            // Sparkles
            MoonlightSparkles(position, (int)(10 * intensity), 35f * intensity);
            
            // Accidentals
            MoonlightAccidentals(position, (int)(4 * intensity), 25f * intensity);
            
            // Central bloom
            var bloom = new BloomParticle(position, Vector2.Zero, MoonlightDarkPurple * 0.7f, 
                0.5f * intensity, 1f * intensity, 30);
            MagnumParticleHandler.SpawnParticle(bloom);
            
            // Optional clef for major impacts
            if (includeClef)
            {
                MoonlightClef(position, Main.rand.NextBool(), 0.8f * intensity);
            }
            
            Lighting.AddLight(position, MoonlightMediumPurple.ToVector3() * 0.6f * intensity);
        }
        
        /// <summary>
        /// Creates a musical note trail for Moonlight projectiles.
        /// Call in AI() for continuous musical trailing effect.
        /// </summary>
        public static void MoonlightMusicTrail(Vector2 position, Vector2 velocity)
        {
            // Occasional music note
            if (Main.rand.NextBool(6))
            {
                Vector2 noteVel = -velocity * 0.1f + new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-1f, 0f));
                Color color = Main.rand.NextBool() ? MoonlightLightPurple : MoonlightSilver;
                
                var note = new MusicNoteParticle(position, noteVel, color, Main.rand.NextFloat(0.15f, 0.27f),
                    Main.rand.Next(40, 60));
                MagnumParticleHandler.SpawnParticle(note);
            }
            
            // Regular glow trail
            if (Main.rand.NextBool(3))
            {
                var glow = new GenericGlowParticle(position + Main.rand.NextVector2Circular(4f, 4f), 
                    -velocity * 0.08f, MoonlightDarkPurple, Main.rand.NextFloat(0.15f, 0.3f), 
                    Main.rand.Next(15, 25), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
        }
        
        #endregion
        
        #region Eroica Musical Effects
        
        /// <summary>
        /// Creates floating music notes with Eroica theming.
        /// Notes drift upward with fiery colors - heroic and triumphant.
        /// </summary>
        /// <param name="position">Center position for spawning</param>
        /// <param name="count">Number of notes to spawn</param>
        /// <param name="spreadRadius">Spawn area radius</param>
        public static void EroicaMusicNotes(Vector2 position, int count = 5, float spreadRadius = 30f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(spreadRadius, spreadRadius);
                Vector2 velocity = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-2.5f, -1.5f));
                
                // Eroica colors - golds, crimsons, and flames
                Color color = Main.rand.Next(4) switch
                {
                    0 => EroicaGold,
                    1 => EroicaCrimson,
                    2 => EroicaFlame,
                    _ => EroicaAmber
                };
                
                // Random note type is handled by the simple constructor
                float scale = Main.rand.NextFloat(0.4f, 0.8f);
                int lifetime = Main.rand.Next(50, 90);
                
                var note = new MusicNoteParticle(position + offset, velocity, color, scale, lifetime);
                MagnumParticleHandler.SpawnParticle(note);
            }
        }
        
        /// <summary>
        /// Creates a dramatic clef symbol for Eroica effects.
        /// Bold golden clef that pulses - use for heroic ability activations.
        /// </summary>
        /// <param name="position">Position for the clef</param>
        /// <param name="useTrebleClef">True for treble clef, false for bass clef</param>
        /// <param name="scale">Base scale multiplier</param>
        public static void EroicaClef(Vector2 position, bool useTrebleClef = true, float scale = 1f)
        {
            Color color = EroicaGold;
            Color bloomColor = EroicaCrimson * 0.7f;
            var clef = new ClefParticle(position, Vector2.Zero, color, bloomColor, scale * 1.1f, 70, useTrebleClef);
            MagnumParticleHandler.SpawnParticle(clef);
            
            // Accompanying fire sparkles
            EroicaSparkles(position, 6, 30f * scale);
        }
        
        /// <summary>
        /// Creates glowing music staff lines for Eroica effects.
        /// Five bold lines with fiery glow - great for dramatic moments.
        /// </summary>
        /// <param name="position">Center position of the staff</param>
        /// <param name="scale">Scale multiplier</param>
        public static void EroicaMusicStaff(Vector2 position, float scale = 1f)
        {
            var staff = new MusicStaffParticle(position, Vector2.Zero, EroicaGold * 0.9f, scale * 0.9f, 55);
            MagnumParticleHandler.SpawnParticle(staff);
        }
        
        /// <summary>
        /// Creates sharp or flat accidental symbols for Eroica effects.
        /// Bold accidentals with fire colors - use for dramatic accents.
        /// </summary>
        /// <param name="position">Center spawn position</param>
        /// <param name="count">Number of accidentals</param>
        /// <param name="spreadRadius">Spawn area radius</param>
        public static void EroicaAccidentals(Vector2 position, int count = 3, float spreadRadius = 20f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(spreadRadius, spreadRadius);
                Vector2 velocity = new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), Main.rand.NextFloat(-2f, -0.8f));
                
                Color color = Main.rand.NextBool() ? EroicaGold : EroicaCrimson;
                bool isSharp = Main.rand.NextBool();
                
                var accidental = new AccidentalParticle(position + offset, velocity, color, 
                    Main.rand.NextFloat(0.5f, 0.8f), Main.rand.Next(45, 70), isSharp);
                MagnumParticleHandler.SpawnParticle(accidental);
            }
        }
        
        /// <summary>
        /// Creates a full musical impact effect for Eroica.
        /// Combines notes, fire sparkles, and optional clef for heroic moments.
        /// </summary>
        /// <param name="position">Impact center</param>
        /// <param name="intensity">Effect intensity multiplier</param>
        /// <param name="includeClef">Whether to spawn a clef symbol</param>
        public static void EroicaMusicalImpact(Vector2 position, float intensity = 1f, bool includeClef = false)
        {
            // Burst of music notes
            EroicaMusicNotes(position, (int)(8 * intensity), 45f * intensity);
            
            // Fire sparkles
            EroicaSparkles(position, (int)(10 * intensity), 35f * intensity);
            
            // Accidentals
            EroicaAccidentals(position, (int)(4 * intensity), 25f * intensity);
            
            // Central fire bloom
            var bloom = new BloomParticle(position, Vector2.Zero, EroicaCrimson, 
                0.5f * intensity, 1.1f * intensity, 25);
            MagnumParticleHandler.SpawnParticle(bloom);
            
            // Gold bloom core
            var coreBloom = new BloomParticle(position, Vector2.Zero, EroicaGold * 0.8f, 
                0.3f * intensity, 0.6f * intensity, 20);
            MagnumParticleHandler.SpawnParticle(coreBloom);
            
            // Optional clef for major impacts
            if (includeClef)
            {
                EroicaClef(position, Main.rand.NextBool(), 0.9f * intensity);
            }
            
            // Some smoke for drama
            if (intensity > 0.7f)
            {
                for (int i = 0; i < 2; i++)
                {
                    var smoke = new HeavySmokeParticle(position + Main.rand.NextVector2Circular(15f, 15f), 
                        Main.rand.NextVector2Circular(2f, 2f), EroicaBlack, 40, 0.4f * intensity, 0.5f, 0.015f, false);
                    MagnumParticleHandler.SpawnParticle(smoke);
                }
            }
            
            Lighting.AddLight(position, EroicaCrimson.ToVector3() * 0.7f * intensity);
        }
        
        /// <summary>
        /// Creates a musical note trail for Eroica projectiles.
        /// Call in AI() for continuous fiery musical trailing effect.
        /// </summary>
        public static void EroicaMusicTrail(Vector2 position, Vector2 velocity)
        {
            // Occasional music note
            if (Main.rand.NextBool(5))
            {
                Vector2 noteVel = -velocity * 0.12f + new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-1.5f, -0.5f));
                Color color = Main.rand.NextBool() ? EroicaGold : EroicaCrimson;
                
                var note = new MusicNoteParticle(position, noteVel, color, Main.rand.NextFloat(0.15f, 0.27f),
                    Main.rand.Next(35, 55));
                MagnumParticleHandler.SpawnParticle(note);
            }
            
            // Fire glow trail
            if (Main.rand.NextBool(3))
            {
                var glow = new GenericGlowParticle(position + Main.rand.NextVector2Circular(5f, 5f), 
                    -velocity * 0.1f + new Vector2(0, -0.4f), EroicaCrimson, Main.rand.NextFloat(0.15f, 0.35f), 
                    Main.rand.Next(15, 25), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Occasional spark
            if (Main.rand.NextBool(8))
            {
                var spark = new GlowSparkParticle(position, -velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f), 
                    true, Main.rand.Next(15, 25), Main.rand.NextFloat(0.2f, 0.35f), 
                    EroicaFlame, new Vector2(0.3f, 1.2f), true, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
        }
        
        #endregion
        
        #region Generic Musical Effects
        
        /// <summary>
        /// Creates generic music notes with custom color.
        /// Use for items that don't fit Moonlight/Eroica theming.
        /// </summary>
        /// <param name="position">Center position for spawning</param>
        /// <param name="color">Base color for the notes</param>
        /// <param name="count">Number of notes to spawn</param>
        /// <param name="spreadRadius">Spawn area radius</param>
        public static void MusicNotes(Vector2 position, Color color, int count = 5, float spreadRadius = 30f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(spreadRadius, spreadRadius);
                Vector2 velocity = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-2f, -1f));
                
                // Slight color variation
                Color noteColor = color * Main.rand.NextFloat(0.8f, 1.2f);
                
                float scale = Main.rand.NextFloat(0.4f, 0.8f);
                int lifetime = Main.rand.Next(50, 90);
                
                var note = new MusicNoteParticle(position + offset, velocity, noteColor, scale, lifetime);
                MagnumParticleHandler.SpawnParticle(note);
            }
        }
        
        /// <summary>
        /// Creates a cascade of falling music notes - great for weapon abilities.
        /// Notes fall from above like musical rain.
        /// </summary>
        /// <param name="position">Center position (notes spawn above)</param>
        /// <param name="color">Base color for notes</param>
        /// <param name="count">Number of notes</param>
        /// <param name="width">Horizontal spread</param>
        public static void MusicNoteCascade(Vector2 position, Color color, int count = 10, float width = 100f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 spawnPos = position + new Vector2(Main.rand.NextFloat(-width / 2f, width / 2f), -Main.rand.NextFloat(50f, 150f));
                Vector2 velocity = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(2f, 4f));
                
                Color noteColor = color * Main.rand.NextFloat(0.85f, 1.15f);
                
                var note = new MusicNoteParticle(spawnPos, velocity, noteColor, Main.rand.NextFloat(0.4f, 0.7f),
                    Main.rand.Next(60, 100));
                MagnumParticleHandler.SpawnParticle(note);
            }
        }
        
        /// <summary>
        /// Creates a circular burst of music notes - great for AOE effects.
        /// Notes explode outward from center in all directions.
        /// </summary>
        /// <param name="position">Burst center</param>
        /// <param name="color">Base color for notes</param>
        /// <param name="count">Number of notes</param>
        /// <param name="speed">Outward velocity</param>
        public static void MusicNoteBurst(Vector2 position, Color color, int count = 12, float speed = 4f)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(speed * 0.7f, speed * 1.3f);
                
                Color noteColor = color * Main.rand.NextFloat(0.85f, 1.15f);
                
                var note = new MusicNoteParticle(position, velocity, noteColor, Main.rand.NextFloat(0.5f, 0.8f),
                    Main.rand.Next(50, 80));
                MagnumParticleHandler.SpawnParticle(note);
            }
        }
        
        /// <summary>
        /// Creates a swirling ring of music notes around a position.
        /// Notes orbit the center - great for charging effects.
        /// </summary>
        /// <param name="center">Center of the ring</param>
        /// <param name="color">Note color</param>
        /// <param name="radius">Ring radius</param>
        /// <param name="count">Number of notes in the ring</param>
        public static void MusicNoteRing(Vector2 center, Color color, float radius = 50f, int count = 8)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                Vector2 offset = angle.ToRotationVector2() * radius;
                Vector2 velocity = (angle + MathHelper.PiOver2).ToRotationVector2() * 2f; // Tangential velocity
                
                var note = new MusicNoteParticle(center + offset, velocity, color,
                    Main.rand.NextFloat(0.4f, 0.6f), 40);
                MagnumParticleHandler.SpawnParticle(note);
            }
        }
        
        #endregion
        
        #region Swan Lake Color Palette
        
        /// <summary>Pure white - primary swan lake color</summary>
        public static readonly Color SwanWhite = new Color(255, 255, 255);
        /// <summary>Deep black - contrasting swan lake color</summary>
        public static readonly Color SwanBlack = new Color(20, 20, 25);
        /// <summary>Pearl white with slight shimmer</summary>
        public static readonly Color SwanPearl = new Color(250, 248, 255);
        /// <summary>Soft gray transition</summary>
        public static readonly Color SwanGray = new Color(180, 180, 190);
        /// <summary>Icy blue accent</summary>
        public static readonly Color SwanIcyBlue = new Color(200, 230, 255);
        /// <summary>Rainbow pearlescent pink</summary>
        public static readonly Color SwanPearlPink = new Color(255, 230, 240);
        /// <summary>Rainbow pearlescent blue</summary>
        public static readonly Color SwanPearlBlue = new Color(230, 240, 255);
        /// <summary>Rainbow pearlescent green</summary>
        public static readonly Color SwanPearlGreen = new Color(240, 255, 245);
        
        #endregion
        
        #region Swan Lake Particle Effects
        
        /// <summary>
        /// Creates a swan lake bloom burst - black and white with pearlescent shimmer.
        /// Use for projectile impacts, magical bursts, swan effects.
        /// </summary>
        public static void SwanLakeBloomBurst(Vector2 position, float intensity = 1f)
        {
            int count = (int)(8 * intensity);
            
            // Alternating black and white blooms
            for (int i = 0; i < count; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(3f, 3f) * intensity;
                Color color = i % 2 == 0 ? SwanWhite : SwanBlack * 0.8f;
                var bloom = new BloomParticle(position, velocity, color, 
                    0.35f * intensity, 0.7f * intensity, Main.rand.Next(25, 45));
                MagnumParticleHandler.SpawnParticle(bloom);
            }
            
            // Pearlescent shimmer accents
            for (int i = 0; i < count / 2; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(2f, 2f) * intensity;
                Color pearl = i % 3 == 0 ? SwanPearlPink : (i % 3 == 1 ? SwanPearlBlue : SwanPearlGreen);
                var bloom = new BloomParticle(position, velocity, pearl * 0.6f, 
                    0.25f * intensity, 0.5f * intensity, Main.rand.Next(30, 50));
                MagnumParticleHandler.SpawnParticle(bloom);
            }
        }
        
        /// <summary>
        /// Creates swan lake sparkles - black and white twinkling with rainbow accents.
        /// Use for ambient magical effects, enchantment visuals, ethereal auras.
        /// </summary>
        public static void SwanLakeSparkles(Vector2 position, int count = 8, float spreadRadius = 30f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(spreadRadius, spreadRadius);
                Vector2 velocity = Main.rand.NextVector2Circular(1f, 1f) + new Vector2(0, -0.3f);
                
                // Black/white base with pearlescent bloom
                Color color = Main.rand.NextBool() ? SwanWhite : SwanGray;
                Color bloom = Main.rand.Next(3) switch
                {
                    0 => SwanPearlPink * 0.4f,
                    1 => SwanPearlBlue * 0.4f,
                    _ => SwanPearlGreen * 0.4f
                };
                
                var sparkle = new SparkleParticle(position + offset, velocity, color, bloom, 
                    Main.rand.NextFloat(0.5f, 1f), Main.rand.Next(45, 85), 0.05f, 1.2f);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }
        
        /// <summary>
        /// Creates a swan lake shockwave ring - alternating black and white expanding rings.
        /// Use for teleportation effects, wave attacks, pulse abilities.
        /// </summary>
        public static void SwanLakeShockwave(Vector2 position, float scale = 1f)
        {
            // White inner ring
            var whiteRing = new BloomRingParticle(position, Vector2.Zero, SwanWhite * 0.9f, 
                0.3f * scale, 35, 0.08f);
            MagnumParticleHandler.SpawnParticle(whiteRing);
            
            // Black outer ring
            var blackRing = new BloomRingParticle(position, Vector2.Zero, SwanBlack * 0.7f, 
                0.2f * scale, 45, 0.1f);
            MagnumParticleHandler.SpawnParticle(blackRing);
            
            // Rainbow shimmer ring
            float hue = Main.rand.NextFloat();
            Color rainbow = Main.hslToRgb(hue, 0.5f, 0.8f);
            var rainbowRing = new BloomRingParticle(position, Vector2.Zero, rainbow * 0.4f, 
                0.25f * scale, 40, 0.09f);
            MagnumParticleHandler.SpawnParticle(rainbowRing);
        }
        
        /// <summary>
        /// Creates swan lake sparks - directional black/white particles with rainbow trails.
        /// Use for sword slashes, projectile trails, hit effects.
        /// </summary>
        public static void SwanLakeSparks(Vector2 position, Vector2 direction, int count = 6, float speed = 5f)
        {
            float baseAngle = direction.ToRotation();
            
            for (int i = 0; i < count; i++)
            {
                float angle = baseAngle + Main.rand.NextFloat(-0.6f, 0.6f);
                float sparkSpeed = Main.rand.NextFloat(speed * 0.6f, speed * 1.2f);
                Vector2 velocity = angle.ToRotationVector2() * sparkSpeed;
                
                Color color = i % 2 == 0 ? SwanWhite : SwanGray;
                var spark = new GlowSparkParticle(position, velocity, false, 
                    Main.rand.Next(20, 40), Main.rand.NextFloat(0.3f, 0.6f), color,
                    new Vector2(0.4f, 1.8f), false, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
        }
        
        /// <summary>
        /// Creates a massive rainbow explosion effect for Swan Lake.
        /// Black/white core that explodes into pearlescent rainbow colors.
        /// </summary>
        public static void SwanLakeRainbowExplosion(Vector2 position, float intensity = 1f)
        {
            // Central black/white burst
            SwanLakeBloomBurst(position, intensity);
            
            // Multiple rainbow-colored shockwaves
            for (int ring = 0; ring < 3; ring++)
            {
                float hue = (Main.GameUpdateCount * 0.01f + ring * 0.33f) % 1f;
                Color rainbow = Main.hslToRgb(hue, 0.6f, 0.7f);
                var rainbowRing = new BloomRingParticle(position, Vector2.Zero, rainbow * 0.5f, 
                    (0.2f + ring * 0.1f) * intensity, 30 + ring * 10, 0.1f + ring * 0.02f);
                MagnumParticleHandler.SpawnParticle(rainbowRing);
            }
            
            // Radial rainbow sparkle burst
            int sparkleCount = (int)(16 * intensity);
            for (int i = 0; i < sparkleCount; i++)
            {
                float angle = MathHelper.TwoPi * i / sparkleCount;
                Vector2 velocity = angle.ToRotationVector2() * (3f + Main.rand.NextFloat(2f)) * intensity;
                float hue = (float)i / sparkleCount;
                Color rainbow = Main.hslToRgb(hue, 0.7f, 0.75f);
                
                var sparkle = new SparkleParticle(position, velocity, rainbow, SwanWhite * 0.3f, 
                    Main.rand.NextFloat(0.6f, 1f) * intensity, Main.rand.Next(40, 70), 0.04f, 1.3f);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Pearlescent glow particles
            for (int i = 0; i < (int)(12 * intensity); i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(4f, 4f) * intensity;
                Color pearl = i % 3 == 0 ? SwanPearlPink : (i % 3 == 1 ? SwanPearlBlue : SwanPearlGreen);
                var glow = new GenericGlowParticle(position + Main.rand.NextVector2Circular(10f, 10f), 
                    velocity, pearl, Main.rand.NextFloat(0.3f, 0.6f) * intensity, Main.rand.Next(35, 55), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            Lighting.AddLight(position, 1f * intensity, 1f * intensity, 1f * intensity);
        }
        
        /// <summary>
        /// Creates swan lake impact effect - combined bloom, ring, sparks with rainbow burst.
        /// Use for major hits, projectile explosions, ability triggers.
        /// </summary>
        public static void SwanLakeImpact(Vector2 position, float intensity = 1f)
        {
            // Central bloom
            SwanLakeBloomBurst(position, intensity);
            
            // Shockwave
            SwanLakeShockwave(position, intensity);
            
            // Radial sparks
            for (int i = 0; i < (int)(8 * intensity); i++)
            {
                float angle = MathHelper.TwoPi * i / (8 * intensity);
                Vector2 dir = angle.ToRotationVector2();
                SwanLakeSparks(position, dir, 2, 4f * intensity);
            }
            
            // Rainbow sparkles
            SwanLakeSparkles(position, (int)(12 * intensity), 40f * intensity);
            
            // Light burst (white)
            Lighting.AddLight(position, 0.9f * intensity, 0.9f * intensity, 1f * intensity);
        }
        
        /// <summary>
        /// Creates swan lake trail particles for projectiles.
        /// Use in AI() methods for continuous black/white trailing effect.
        /// </summary>
        public static void SwanLakeTrail(Vector2 position, Vector2 velocity)
        {
            if (Main.rand.NextBool(2))
            {
                Color color = Main.rand.NextBool() ? SwanWhite : SwanGray;
                var glow = new GenericGlowParticle(position + Main.rand.NextVector2Circular(6f, 6f), 
                    -velocity * 0.1f, color, Main.rand.NextFloat(0.2f, 0.4f), 
                    Main.rand.Next(15, 30), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            if (Main.rand.NextBool(4))
            {
                // Occasional rainbow shimmer
                float hue = Main.rand.NextFloat();
                Color rainbow = Main.hslToRgb(hue, 0.4f, 0.85f);
                var sparkle = new SparkleParticle(position, -velocity * 0.05f + Main.rand.NextVector2Circular(0.5f, 0.5f), 
                    SwanWhite, rainbow * 0.5f, Main.rand.NextFloat(0.3f, 0.6f), 
                    Main.rand.Next(20, 40), 0.04f, 1f);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }
        
        /// <summary>
        /// Creates swan lake ambient aura particles around an entity.
        /// Use in UpdateAccessory or AI for persistent visual effects.
        /// </summary>
        public static void SwanLakeAura(Vector2 center, float radius = 40f)
        {
            if (Main.rand.NextBool(6))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 offset = angle.ToRotationVector2() * Main.rand.NextFloat(radius * 0.5f, radius);
                Vector2 velocity = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-1f, -0.3f));
                
                Color color = Main.rand.NextBool() ? SwanWhite : SwanGray;
                var glow = new GenericGlowParticle(center + offset, velocity, color, 
                    Main.rand.NextFloat(0.15f, 0.35f), Main.rand.Next(40, 70), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Occasional pearlescent shimmer
            if (Main.rand.NextBool(12))
            {
                Vector2 offset = Main.rand.NextVector2Circular(radius, radius);
                Color pearl = Main.rand.Next(3) switch
                {
                    0 => SwanPearlPink,
                    1 => SwanPearlBlue,
                    _ => SwanPearlGreen
                };
                
                var sparkle = new SparkleParticle(center + offset, Vector2.Zero, pearl * 0.8f, SwanWhite * 0.3f,
                    Main.rand.NextFloat(0.4f, 0.7f), Main.rand.Next(30, 50), 0.03f, 1f);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }
        
        /// <summary>
        /// Creates swan lake music notes - black and white notes with rainbow trail.
        /// Use for musical thematic effects.
        /// </summary>
        public static void SwanLakeMusicNotes(Vector2 position, int count = 5, float spreadRadius = 30f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(spreadRadius, spreadRadius);
                Vector2 velocity = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), Main.rand.NextFloat(-2f, -1f));
                
                // Alternate black and white notes
                Color color = i % 2 == 0 ? SwanWhite : SwanGray;
                
                var note = new MusicNoteParticle(position + offset, velocity, color, 
                    Main.rand.NextFloat(0.4f, 0.7f), Main.rand.Next(50, 80));
                MagnumParticleHandler.SpawnParticle(note);
            }
        }
        
        /// <summary>
        /// Creates swan lake accidentals - sharp and flat symbols in black/white.
        /// Use for musical accent effects.
        /// </summary>
        public static void SwanLakeAccidentals(Vector2 position, int count = 3, float spreadRadius = 20f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(spreadRadius, spreadRadius);
                Vector2 velocity = new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), Main.rand.NextFloat(-2f, -0.8f));
                
                Color color = i % 2 == 0 ? SwanWhite : SwanGray;
                bool isSharp = Main.rand.NextBool();
                
                var accidental = new AccidentalParticle(position + offset, velocity, color, 
                    Main.rand.NextFloat(0.5f, 0.8f), Main.rand.Next(45, 70), isSharp);
                MagnumParticleHandler.SpawnParticle(accidental);
            }
        }
        
        /// <summary>
        /// Creates a full musical impact effect for Swan Lake.
        /// Combines notes, sparkles, and rainbow burst for dramatic moments.
        /// </summary>
        public static void SwanLakeMusicalImpact(Vector2 position, float intensity = 1f, bool includeRainbow = true)
        {
            // Burst of music notes
            SwanLakeMusicNotes(position, (int)(8 * intensity), 45f * intensity);
            
            // Black/white sparkles
            SwanLakeSparkles(position, (int)(10 * intensity), 35f * intensity);
            
            // Accidentals
            SwanLakeAccidentals(position, (int)(4 * intensity), 25f * intensity);
            
            // Central black/white bloom
            var whiteBloom = new BloomParticle(position, Vector2.Zero, SwanWhite, 
                0.5f * intensity, 1f * intensity, 25);
            MagnumParticleHandler.SpawnParticle(whiteBloom);
            
            var blackBloom = new BloomParticle(position, Vector2.Zero, SwanBlack * 0.6f, 
                0.4f * intensity, 0.8f * intensity, 30);
            MagnumParticleHandler.SpawnParticle(blackBloom);
            
            // Rainbow explosion for major impacts
            if (includeRainbow && intensity > 0.7f)
            {
                SwanLakeRainbowExplosion(position, intensity * 0.7f);
            }
            
            Lighting.AddLight(position, 0.8f * intensity, 0.8f * intensity, 0.9f * intensity);
        }
        
        /// <summary>
        /// Creates a black/white halo effect around a position.
        /// Alternating black and white particles in a ring with pearlescent shimmer.
        /// </summary>
        public static void SwanLakeHalo(Vector2 center, float radius = 40f, int particleCount = 12)
        {
            for (int i = 0; i < particleCount; i++)
            {
                float angle = MathHelper.TwoPi * i / particleCount;
                Vector2 pos = center + angle.ToRotationVector2() * radius;
                Vector2 velocity = (angle + MathHelper.PiOver2).ToRotationVector2() * 1.5f;
                
                Color color = i % 2 == 0 ? SwanWhite : SwanGray;
                var glow = new GenericGlowParticle(pos, velocity, color, 0.4f, 30, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Inner pearlescent ring
            for (int i = 0; i < particleCount / 2; i++)
            {
                float angle = MathHelper.TwoPi * i / (particleCount / 2) + Main.rand.NextFloat(0.2f);
                Vector2 pos = center + angle.ToRotationVector2() * (radius * 0.7f);
                
                Color pearl = i % 3 == 0 ? SwanPearlPink : (i % 3 == 1 ? SwanPearlBlue : SwanPearlGreen);
                var sparkle = new SparkleParticle(pos, Vector2.Zero, pearl * 0.7f, SwanWhite * 0.3f,
                    0.5f, 25, 0.03f, 1f);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }
        
        /// <summary>
        /// Creates a feather-like particle burst for Swan Lake.
        /// Gentle floating particles that drift like feathers.
        /// </summary>
        public static void SwanLakeFeathers(Vector2 position, int count = 8, float spreadRadius = 40f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(spreadRadius, spreadRadius);
                Vector2 velocity = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-0.5f, 1f));
                
                Color color = i % 2 == 0 ? SwanWhite : SwanPearl;
                var glow = new GenericGlowParticle(position + offset, velocity, color, 
                    Main.rand.NextFloat(0.3f, 0.5f), Main.rand.Next(60, 100), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
        }
        
        #endregion
    }
}
