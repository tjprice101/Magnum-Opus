using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Fate
{
    /// <summary>
    /// The Fate Astrograph System - Creates massive solar constellation effects around the player.
    /// Each weapon triggers a unique constellation pattern that forms around the player,
    /// then explodes into a brilliant array of cosmic energy.
    /// 
    /// Color Palette:
    /// - Primary: Shiny Black (cosmic void)
    /// - Energy: Dark Pink, Purple-Red (cosmic energy, sparkles)
    /// - Accents: Solar Flares (bright orange/yellow bursts)
    /// </summary>
    public class FateAstrographSystem : ModSystem
    {
        // ========== FATE COLOR PALETTE ==========
        public static readonly Color ShinyBlack = new Color(20, 10, 25);
        public static readonly Color CosmicBlack = new Color(35, 15, 40);
        public static readonly Color DarkPink = new Color(200, 60, 120);
        public static readonly Color CosmicPink = new Color(255, 100, 150);
        public static readonly Color PurpleRed = new Color(180, 40, 100);
        public static readonly Color CosmicRed = new Color(255, 50, 80);
        public static readonly Color DeepPurple = new Color(120, 30, 140);
        public static readonly Color SolarOrange = new Color(255, 160, 60);
        public static readonly Color SolarYellow = new Color(255, 220, 100);
        public static readonly Color StarWhite = new Color(255, 250, 255);
        
        /// <summary>
        /// Get a gradient color for Fate effects
        /// </summary>
        public static Color GetFateGradient(float progress)
        {
            if (progress < 0.3f)
                return Color.Lerp(ShinyBlack, DarkPink, progress / 0.3f);
            else if (progress < 0.6f)
                return Color.Lerp(DarkPink, PurpleRed, (progress - 0.3f) / 0.3f);
            else if (progress < 0.85f)
                return Color.Lerp(PurpleRed, CosmicPink, (progress - 0.6f) / 0.25f);
            else
                return Color.Lerp(CosmicPink, StarWhite, (progress - 0.85f) / 0.15f);
        }
        
        /// <summary>
        /// Get solar flare color (orange to yellow to white)
        /// </summary>
        public static Color GetSolarFlareColor(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(SolarOrange, SolarYellow, progress * 2f);
            else
                return Color.Lerp(SolarYellow, StarWhite, (progress - 0.5f) * 2f);
        }
    }
    
    /// <summary>
    /// Defines different constellation patterns for each Fate weapon
    /// </summary>
    public enum ConstellationType
    {
        // Melee Constellations
        Orion,          // Fate1Sword - The Hunter
        Scorpius,       // Fate2Sword - The Scorpion
        Leo,            // Fate3Sword - The Lion
        Draco,          // Fate4Sword - The Dragon
        Phoenix,        // Fate5Sword - The Phoenix
        
        // Ranged Constellations
        Sagittarius,    // Fate1Ranged - The Archer
        Aquila,         // Fate2Ranged - The Eagle
        
        // Magic Constellations
        Lyra,           // Fate1Magic - The Lyre
        Andromeda,      // Fate2Magic - The Princess
        
        // Summon Constellation
        Hydra           // Fate1Summon - The Serpent
    }
    
    /// <summary>
    /// Static class containing constellation star patterns
    /// </summary>
    public static class ConstellationPatterns
    {
        /// <summary>
        /// Get the star positions for a given constellation type.
        /// Returns array of Vector2 offsets from center.
        /// </summary>
        public static Vector2[] GetPattern(ConstellationType type)
        {
            return type switch
            {
                ConstellationType.Orion => new Vector2[]
                {
                    new Vector2(0, -80),      // Betelgeuse (shoulder)
                    new Vector2(50, -70),     // Bellatrix (shoulder)
                    new Vector2(10, -40),     // Belt star 1
                    new Vector2(25, -35),     // Belt star 2
                    new Vector2(40, -30),     // Belt star 3
                    new Vector2(-10, 20),     // Saiph (foot)
                    new Vector2(60, 30),      // Rigel (foot)
                    new Vector2(25, 0),       // Sword star
                },
                ConstellationType.Scorpius => new Vector2[]
                {
                    new Vector2(-70, -40),    // Antares (heart)
                    new Vector2(-50, -60),    // Head star 1
                    new Vector2(-30, -70),    // Head star 2
                    new Vector2(-10, -65),    // Head star 3
                    new Vector2(-60, -20),    // Body 1
                    new Vector2(-40, 0),      // Body 2
                    new Vector2(-20, 20),     // Body 3
                    new Vector2(10, 35),      // Tail 1
                    new Vector2(40, 45),      // Tail 2
                    new Vector2(70, 40),      // Stinger
                },
                ConstellationType.Leo => new Vector2[]
                {
                    new Vector2(-60, -50),    // Regulus (heart)
                    new Vector2(-40, -70),    // Head 1
                    new Vector2(-20, -80),    // Head 2
                    new Vector2(10, -75),     // Mane 1
                    new Vector2(-70, -30),    // Chest
                    new Vector2(-50, 0),      // Body
                    new Vector2(-20, 20),     // Hindquarters
                    new Vector2(30, 30),      // Tail
                    new Vector2(60, 20),      // Tail tip
                },
                ConstellationType.Draco => new Vector2[]
                {
                    new Vector2(-80, -60),    // Head
                    new Vector2(-60, -40),    // Neck 1
                    new Vector2(-30, -50),    // Neck 2
                    new Vector2(0, -40),      // Body 1
                    new Vector2(30, -20),     // Body 2
                    new Vector2(50, 10),      // Body 3
                    new Vector2(40, 40),      // Body 4
                    new Vector2(10, 60),      // Tail 1
                    new Vector2(-30, 50),     // Tail 2
                    new Vector2(-60, 30),     // Tail tip
                },
                ConstellationType.Phoenix => new Vector2[]
                {
                    new Vector2(0, -90),      // Head
                    new Vector2(-20, -60),    // Neck
                    new Vector2(0, -30),      // Body
                    new Vector2(-60, -40),    // Left wing 1
                    new Vector2(-90, -20),    // Left wing 2
                    new Vector2(60, -40),     // Right wing 1
                    new Vector2(90, -20),     // Right wing 2
                    new Vector2(-30, 20),     // Tail 1
                    new Vector2(0, 50),       // Tail 2
                    new Vector2(30, 20),      // Tail 3
                },
                ConstellationType.Sagittarius => new Vector2[]
                {
                    new Vector2(-40, -70),    // Bow top
                    new Vector2(-60, -40),    // Bow upper
                    new Vector2(-50, 0),      // Bow lower
                    new Vector2(-30, 30),     // Bow bottom
                    new Vector2(0, -20),      // Arrow nock
                    new Vector2(40, -30),     // Arrow shaft
                    new Vector2(80, -40),     // Arrow head
                    new Vector2(-20, 50),     // Body
                },
                ConstellationType.Aquila => new Vector2[]
                {
                    new Vector2(0, -80),      // Head
                    new Vector2(0, -40),      // Body
                    new Vector2(-70, -50),    // Left wing 1
                    new Vector2(-100, -30),   // Left wing 2
                    new Vector2(70, -50),     // Right wing 1
                    new Vector2(100, -30),    // Right wing 2
                    new Vector2(-30, 20),     // Tail left
                    new Vector2(30, 20),      // Tail right
                },
                ConstellationType.Lyra => new Vector2[]
                {
                    new Vector2(0, -70),      // Vega (top)
                    new Vector2(-30, -40),    // Frame left top
                    new Vector2(30, -40),     // Frame right top
                    new Vector2(-40, 0),      // Frame left mid
                    new Vector2(40, 0),       // Frame right mid
                    new Vector2(-30, 40),     // Frame left bottom
                    new Vector2(30, 40),      // Frame right bottom
                    new Vector2(0, 50),       // Base
                },
                ConstellationType.Andromeda => new Vector2[]
                {
                    new Vector2(0, -80),      // Head
                    new Vector2(0, -40),      // Torso
                    new Vector2(-50, -60),    // Left arm 1
                    new Vector2(-90, -50),    // Left arm 2 (chained)
                    new Vector2(50, -60),     // Right arm 1
                    new Vector2(90, -50),     // Right arm 2 (chained)
                    new Vector2(-30, 20),     // Left leg
                    new Vector2(30, 20),      // Right leg
                    new Vector2(0, 60),       // Feet
                },
                ConstellationType.Hydra => new Vector2[]
                {
                    new Vector2(-90, -40),    // Head 1
                    new Vector2(-80, -60),    // Head 2
                    new Vector2(-70, -35),    // Head 3
                    new Vector2(-50, -30),    // Neck
                    new Vector2(-20, -20),    // Body 1
                    new Vector2(20, -10),     // Body 2
                    new Vector2(50, 10),      // Body 3
                    new Vector2(70, 35),      // Body 4
                    new Vector2(50, 55),      // Tail 1
                    new Vector2(20, 60),      // Tail tip
                },
                _ => new Vector2[] { Vector2.Zero }
            };
        }
        
        /// <summary>
        /// Get the connection lines for a constellation (pairs of indices to connect)
        /// </summary>
        public static (int, int)[] GetConnections(ConstellationType type)
        {
            return type switch
            {
                ConstellationType.Orion => new (int, int)[]
                {
                    (0, 2), (1, 4), (2, 3), (3, 4), (2, 5), (4, 6), (3, 7)
                },
                ConstellationType.Scorpius => new (int, int)[]
                {
                    (0, 1), (1, 2), (2, 3), (0, 4), (4, 5), (5, 6), (6, 7), (7, 8), (8, 9)
                },
                ConstellationType.Leo => new (int, int)[]
                {
                    (0, 1), (1, 2), (2, 3), (0, 4), (4, 5), (5, 6), (6, 7), (7, 8)
                },
                ConstellationType.Draco => new (int, int)[]
                {
                    (0, 1), (1, 2), (2, 3), (3, 4), (4, 5), (5, 6), (6, 7), (7, 8), (8, 9)
                },
                ConstellationType.Phoenix => new (int, int)[]
                {
                    (0, 1), (1, 2), (2, 3), (3, 4), (2, 5), (5, 6), (2, 7), (7, 8), (8, 9)
                },
                ConstellationType.Sagittarius => new (int, int)[]
                {
                    (0, 1), (1, 2), (2, 3), (1, 4), (4, 5), (5, 6), (2, 7)
                },
                ConstellationType.Aquila => new (int, int)[]
                {
                    (0, 1), (1, 2), (2, 3), (1, 4), (4, 5), (1, 6), (1, 7)
                },
                ConstellationType.Lyra => new (int, int)[]
                {
                    (0, 1), (0, 2), (1, 3), (2, 4), (3, 5), (4, 6), (5, 7), (6, 7)
                },
                ConstellationType.Andromeda => new (int, int)[]
                {
                    (0, 1), (1, 2), (2, 3), (1, 4), (4, 5), (1, 6), (1, 7), (6, 8), (7, 8)
                },
                ConstellationType.Hydra => new (int, int)[]
                {
                    (0, 3), (1, 3), (2, 3), (3, 4), (4, 5), (5, 6), (6, 7), (7, 8), (8, 9)
                },
                _ => Array.Empty<(int, int)>()
            };
        }
    }
    
    /// <summary>
    /// Astrograph Constellation Projectile - The main visual effect that forms around the player.
    /// Creates a constellation pattern that builds up, then explodes.
    /// </summary>
    public class AstrographConstellation : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/Glyphs5";
        
        private const int FormationTime = 45;
        private const int HoldTime = 15;
        private const int ExplosionTime = 20;
        private const int TotalTime = FormationTime + HoldTime + ExplosionTime;
        
        private ConstellationType constellationType;
        private Vector2[] starPositions;
        private (int, int)[] connections;
        private float[] starRotations;
        private bool initialized = false;
        
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = TotalTime;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
        }
        
        private void Initialize()
        {
            if (initialized) return;
            initialized = true;
            
            constellationType = (ConstellationType)(int)Projectile.ai[0];
            starPositions = ConstellationPatterns.GetPattern(constellationType);
            connections = ConstellationPatterns.GetConnections(constellationType);
            starRotations = new float[starPositions.Length];
            
            for (int i = 0; i < starRotations.Length; i++)
            {
                starRotations[i] = Main.rand.NextFloat(MathHelper.TwoPi);
            }
        }
        
        public override void AI()
        {
            Initialize();
            
            // Follow owner
            Player owner = Main.player[Projectile.owner];
            if (owner.active)
            {
                Projectile.Center = owner.Center;
            }
            
            int timer = TotalTime - Projectile.timeLeft;
            
            // Spin stars
            for (int i = 0; i < starRotations.Length; i++)
            {
                starRotations[i] += 0.08f;
            }
            
            // Spawn ambient particles during formation
            if (timer < FormationTime + HoldTime)
            {
                float progress = Math.Min(1f, (float)timer / FormationTime);
                
                // Cosmic dust particles
                if (Main.rand.NextBool(3))
                {
                    Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(120f, 120f);
                    Vector2 dustVel = (Projectile.Center - dustPos).SafeNormalize(Vector2.Zero) * 2f;
                    Color dustColor = FateAstrographSystem.GetFateGradient(Main.rand.NextFloat());
                    var dust = new GlowSparkParticle(dustPos, dustVel, dustColor, 0.15f, 20);
                    MagnumParticleHandler.SpawnParticle(dust);
                }
                
                // Solar flare particles
                if (Main.rand.NextBool(8) && progress > 0.5f)
                {
                    int starIndex = Main.rand.Next(starPositions.Length);
                    Vector2 flarePos = Projectile.Center + starPositions[starIndex] * progress;
                    Vector2 flareVel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(3f, 6f);
                    Color flareColor = FateAstrographSystem.GetSolarFlareColor(Main.rand.NextFloat());
                    var flare = new GlowSparkParticle(flarePos, flareVel, flareColor, 0.25f, 15);
                    MagnumParticleHandler.SpawnParticle(flare);
                }
            }
            
            // Explosion phase
            if (timer == FormationTime + HoldTime)
            {
                TriggerExplosion();
            }
            
            // Lighting
            float intensity = timer < FormationTime ? (float)timer / FormationTime : 1f;
            Lighting.AddLight(Projectile.Center, FateAstrographSystem.DarkPink.ToVector3() * intensity * 0.8f);
        }
        
        private void TriggerExplosion()
        {
            // Each star explodes
            for (int i = 0; i < starPositions.Length; i++)
            {
                Vector2 starPos = Projectile.Center + starPositions[i];
                Color starColor = FateAstrographSystem.GetFateGradient((float)i / starPositions.Length);
                
                // Radial spark burst from each star
                for (int j = 0; j < 8; j++)
                {
                    float angle = MathHelper.TwoPi * j / 8f + Main.rand.NextFloat(-0.2f, 0.2f);
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(8f, 14f);
                    var spark = new GlowSparkParticle(starPos, vel, starColor, 0.35f, 25);
                    MagnumParticleHandler.SpawnParticle(spark);
                }
                
                // Solar flare burst
                for (int j = 0; j < 4; j++)
                {
                    float angle = MathHelper.TwoPi * j / 4f + Main.rand.NextFloat(-0.3f, 0.3f);
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 10f);
                    Color flareColor = FateAstrographSystem.GetSolarFlareColor(Main.rand.NextFloat());
                    var flare = new GlowSparkParticle(starPos, vel, flareColor, 0.3f, 18);
                    MagnumParticleHandler.SpawnParticle(flare);
                }
            }
            
            // Massive central burst
            for (int i = 0; i < 24; i++)
            {
                float angle = MathHelper.TwoPi * i / 24f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(12f, 20f);
                Color sparkColor = FateAstrographSystem.GetFateGradient((float)i / 24f);
                var spark = new GlowSparkParticle(Projectile.Center, vel, sparkColor, 0.4f, 30);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // Solar flare ring
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(15f, 25f);
                Color flareColor = FateAstrographSystem.GetSolarFlareColor((float)i / 12f);
                var flare = new GlowSparkParticle(Projectile.Center, vel, flareColor, 0.5f, 25);
                MagnumParticleHandler.SpawnParticle(flare);
            }
            
            Lighting.AddLight(Projectile.Center, FateAstrographSystem.SolarYellow.ToVector3() * 1.5f);
        }
        
        public override bool? CanDamage() => false;
        
        public override bool PreDraw(ref Color lightColor)
        {
            if (!initialized) return false;
            
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D starTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/Glyphs5").Value;
            Vector2 origin = starTex.Size() / 2f;
            
            int timer = TotalTime - Projectile.timeLeft;
            float formationProgress = Math.Min(1f, (float)timer / FormationTime);
            float explosionProgress = timer > FormationTime + HoldTime ? 
                (float)(timer - FormationTime - HoldTime) / ExplosionTime : 0f;
            
            float overallAlpha = 1f - explosionProgress;
            float scale = 1f + explosionProgress * 0.5f;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.1f + 1f;
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Draw connecting lines first
            if (formationProgress > 0.3f)
            {
                float lineAlpha = Math.Min(1f, (formationProgress - 0.3f) / 0.4f) * overallAlpha * 0.6f;
                
                foreach (var (start, end) in connections)
                {
                    if (start < starPositions.Length && end < starPositions.Length)
                    {
                        Vector2 startPos = Projectile.Center + starPositions[start] * formationProgress * scale - Main.screenPosition;
                        Vector2 endPos = Projectile.Center + starPositions[end] * formationProgress * scale - Main.screenPosition;
                        
                        DrawConstellationLine(spriteBatch, starTex, startPos, endPos, 
                            FateAstrographSystem.DarkPink * lineAlpha, origin);
                    }
                }
            }
            
            // Draw stars
            for (int i = 0; i < starPositions.Length; i++)
            {
                float starFormation = Math.Min(1f, formationProgress * 1.5f - (float)i / starPositions.Length * 0.5f);
                if (starFormation <= 0) continue;
                
                Vector2 starPos = Projectile.Center + starPositions[i] * formationProgress * scale - Main.screenPosition;
                Color starColor = FateAstrographSystem.GetFateGradient((float)i / starPositions.Length);
                float starScale = (0.35f + starFormation * 0.25f) * pulse * overallAlpha;
                
                // Outer glow (shiny black tinted)
                spriteBatch.Draw(starTex, starPos, null, FateAstrographSystem.CosmicBlack * starFormation * 0.5f * overallAlpha, 
                    starRotations[i], origin, starScale * 1.5f, SpriteEffects.None, 0f);
                
                // Main star (dark pink/purple-red)
                spriteBatch.Draw(starTex, starPos, null, starColor * starFormation * 0.8f * overallAlpha, 
                    starRotations[i], origin, starScale, SpriteEffects.None, 0f);
                
                // Inner core (bright)
                spriteBatch.Draw(starTex, starPos, null, FateAstrographSystem.StarWhite * starFormation * 0.6f * overallAlpha, 
                    starRotations[i], origin, starScale * 0.4f, SpriteEffects.None, 0f);
            }
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
        
        private void DrawConstellationLine(SpriteBatch spriteBatch, Texture2D tex, Vector2 start, Vector2 end, Color color, Vector2 origin)
        {
            Vector2 direction = end - start;
            float length = direction.Length();
            int segments = Math.Max(3, (int)(length / 15f));
            
            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                Vector2 pos = Vector2.Lerp(start, end, t);
                float segmentScale = 0.06f + (float)Math.Sin(t * MathHelper.Pi) * 0.03f;
                spriteBatch.Draw(tex, pos, null, color, Main.GameUpdateCount * 0.03f + t * 2f, origin, segmentScale, SpriteEffects.None, 0f);
            }
        }
    }
    
    /// <summary>
    /// Helper class to spawn Astrograph effects from weapons
    /// </summary>
    public static class FateAstrographHelper
    {
        /// <summary>
        /// Spawn an Astrograph constellation effect around the player
        /// </summary>
        public static void SpawnConstellation(Player player, ConstellationType type)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                Projectile.NewProjectile(
                    player.GetSource_FromThis(),
                    player.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<AstrographConstellation>(),
                    0,
                    0f,
                    player.whoAmI,
                    (float)type
                );
            }
        }
        
        /// <summary>
        /// Spawn solar flare particles at a position
        /// </summary>
        public static void SpawnSolarFlares(Vector2 position, int count, float speed)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(speed * 0.7f, speed * 1.3f);
                Color flareColor = FateAstrographSystem.GetSolarFlareColor(Main.rand.NextFloat());
                var flare = new GlowSparkParticle(position, vel, flareColor, 0.3f, 20);
                MagnumParticleHandler.SpawnParticle(flare);
            }
        }
        
        /// <summary>
        /// Spawn cosmic energy sparks at a position
        /// </summary>
        public static void SpawnCosmicSparks(Vector2 position, int count, float speed)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(speed * 0.6f, speed * 1.4f);
                Color sparkColor = FateAstrographSystem.GetFateGradient((float)i / count);
                var spark = new GlowSparkParticle(position, vel, sparkColor, 0.25f, 18);
                MagnumParticleHandler.SpawnParticle(spark);
            }
        }
        
        /// <summary>
        /// Spawn a shiny black void burst with pink/red energy
        /// </summary>
        public static void SpawnVoidBurst(Vector2 position, float scale)
        {
            // Dark void core sparks
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f) * scale;
                var spark = new GlowSparkParticle(position, vel, FateAstrographSystem.ShinyBlack, 0.3f * scale, 15);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // Pink/red energy outer ring
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 12f) * scale;
                Color energyColor = FateAstrographSystem.GetFateGradient((float)i / 12f);
                var spark = new GlowSparkParticle(position, vel, energyColor, 0.25f * scale, 20);
                MagnumParticleHandler.SpawnParticle(spark);
            }
        }
    }
}
