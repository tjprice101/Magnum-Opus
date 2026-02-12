using Microsoft.Xna.Framework;
using Terraria;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// Central VFX integration system for boss lifecycle events.
    /// Provides themed particle effects for boss spawn, enrage, and death events.
    /// </summary>
    public static class VFXIntegration
    {
        #region Boss Lifecycle Events
        
        /// <summary>
        /// Spawn VFX for when a boss appears.
        /// </summary>
        /// <param name="theme">Theme name (e.g., "Eroica", "LaCampanella", "SwanLake")</param>
        /// <param name="position">World position for the effect</param>
        public static void OnBossSpawn(string theme, Vector2 position)
        {
            Color primaryColor = GetThemePrimaryColor(theme);
            Color secondaryColor = GetThemeSecondaryColor(theme);
            
            // Central flash
            SpawnBloomBurst(position, primaryColor, 1.5f);
            
            // Expanding halos
            for (int i = 0; i < 5; i++)
            {
                float delay = i * 3;
                Color haloColor = Color.Lerp(primaryColor, secondaryColor, i / 5f);
                SpawnExpandingHalo(position, haloColor, 0.5f + i * 0.2f, (int)(15 + delay));
            }
            
            // Radial sparkle burst
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                SpawnSparkle(position, vel, primaryColor, 25);
            }
            
            // Theme-specific particles
            SpawnThemedParticles(theme, position, 15, 80f);
            
            // Screen effects
            ScreenDistortionManager.TriggerRipple(position, primaryColor, 0.6f, 25);
            MagnumScreenEffects.AddScreenShake(10f);
            
            // Lighting flash
            Lighting.AddLight(position, primaryColor.ToVector3() * 2f);
        }
        
        /// <summary>
        /// Enrage VFX for when a boss enters a more aggressive phase.
        /// </summary>
        /// <param name="theme">Theme name</param>
        /// <param name="position">World position for the effect</param>
        public static void OnBossEnrage(string theme, Vector2 position)
        {
            Color primaryColor = GetThemePrimaryColor(theme);
            Color secondaryColor = GetThemeSecondaryColor(theme);
            Color enrageColor = GetThemeEnrageColor(theme);
            
            // Intense central flash
            SpawnBloomBurst(position, Color.White, 1.2f);
            SpawnBloomBurst(position, enrageColor, 1.8f);
            
            // Violent expanding rings
            for (int i = 0; i < 8; i++)
            {
                Color ringColor = Color.Lerp(primaryColor, enrageColor, i / 8f);
                SpawnExpandingHalo(position, ringColor, 0.4f + i * 0.15f, 12 + i * 2);
            }
            
            // Radial particle explosion
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 12f);
                Color sparkColor = Color.Lerp(primaryColor, enrageColor, Main.rand.NextFloat());
                SpawnSparkle(position, vel, sparkColor, 30);
            }
            
            // Theme particles burst outward
            SpawnThemedParticles(theme, position, 25, 120f);
            
            // Screen effects - more intense
            ScreenDistortionManager.TriggerRipple(position, enrageColor, 0.8f, 30);
            MagnumScreenEffects.AddScreenShake(15f);
            
            // Intense lighting
            Lighting.AddLight(position, enrageColor.ToVector3() * 2.5f);
        }
        
        /// <summary>
        /// Death VFX for when a boss is defeated.
        /// </summary>
        /// <param name="theme">Theme name</param>
        /// <param name="position">World position for the effect</param>
        public static void OnBossDeath(string theme, Vector2 position)
        {
            Color primaryColor = GetThemePrimaryColor(theme);
            Color secondaryColor = GetThemeSecondaryColor(theme);
            
            // Massive central flash cascade
            SpawnBloomBurst(position, Color.White, 2.0f);
            SpawnBloomBurst(position, primaryColor, 1.6f);
            SpawnBloomBurst(position, secondaryColor, 1.2f);
            
            // Many expanding halos
            for (int i = 0; i < 10; i++)
            {
                float progress = i / 10f;
                Color haloColor = Color.Lerp(primaryColor, secondaryColor, progress);
                SpawnExpandingHalo(position, haloColor, 0.3f + i * 0.2f, 15 + i * 3);
            }
            
            // Massive radial burst
            for (int i = 0; i < 30; i++)
            {
                float angle = MathHelper.TwoPi * i / 30f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(8f, 15f);
                Color sparkColor = Color.Lerp(primaryColor, secondaryColor, Main.rand.NextFloat());
                SpawnSparkle(position, vel, sparkColor, 40);
            }
            
            // Music note finale
            SpawnMusicNoteBurst(position, primaryColor, 12, 100f);
            
            // Massive theme particle burst
            SpawnThemedParticles(theme, position, 40, 150f);
            
            // Intense screen effects
            ScreenDistortionManager.TriggerRipple(position, primaryColor, 1.0f, 40);
            MagnumScreenEffects.AddScreenShake(20f);
            
            // God rays for dramatic effect
            GodRaySystem.CreateBurst(position, primaryColor, 12, 150f, 50);
            
            // Bright lighting
            Lighting.AddLight(position, primaryColor.ToVector3() * 3f);
        }
        
        #endregion
        
        #region Theme Color Helpers
        
        private static Color GetThemePrimaryColor(string theme)
        {
            return theme switch
            {
                "Eroica" => new Color(200, 50, 50),      // Scarlet
                "LaCampanella" => new Color(255, 140, 40), // Orange
                "SwanLake" => Color.White,
                "MoonlightSonata" => new Color(75, 0, 130), // Dark purple
                "EnigmaVariations" or "Enigma" => new Color(140, 60, 200), // Purple
                "Fate" => new Color(180, 50, 100),       // Dark pink
                "DiesIrae" => new Color(180, 30, 50),    // Blood red
                "ClairDeLune" => new Color(140, 170, 220), // Soft blue
                "OdeToJoy" => new Color(255, 200, 80),   // Gold
                "Nachtmusik" => new Color(255, 220, 150), // Warm gold
                "Spring" => new Color(255, 180, 200),    // Pink
                "Summer" => new Color(255, 140, 50),     // Orange
                "Autumn" => new Color(200, 150, 80),     // Amber
                "Winter" => new Color(150, 200, 255),    // Ice blue
                _ => new Color(200, 200, 200)            // Default white-ish
            };
        }
        
        private static Color GetThemeSecondaryColor(string theme)
        {
            return theme switch
            {
                "Eroica" => new Color(255, 215, 0),      // Gold
                "LaCampanella" => new Color(30, 20, 25), // Black
                "SwanLake" => new Color(30, 30, 40),     // Black
                "MoonlightSonata" => new Color(135, 206, 250), // Light blue
                "EnigmaVariations" or "Enigma" => new Color(50, 220, 100), // Green
                "Fate" => new Color(255, 60, 80),        // Bright red
                "DiesIrae" => new Color(50, 20, 30),     // Dark crimson
                "ClairDeLune" => new Color(240, 240, 250), // Pearl white
                "OdeToJoy" => new Color(255, 100, 80),   // Coral
                "Nachtmusik" => new Color(200, 180, 220), // Lavender
                "Spring" => new Color(180, 255, 180),    // Light green
                "Summer" => new Color(255, 255, 100),    // Yellow
                "Autumn" => new Color(180, 80, 50),      // Rust
                "Winter" => new Color(200, 200, 255),    // Pale blue
                _ => new Color(150, 150, 150)            // Default gray
            };
        }
        
        private static Color GetThemeEnrageColor(string theme)
        {
            return theme switch
            {
                "Eroica" => new Color(255, 100, 50),     // Intense flame
                "LaCampanella" => new Color(255, 80, 20), // Intense orange
                "SwanLake" => new Color(255, 50, 50),    // Red contrast
                "MoonlightSonata" => new Color(180, 50, 255), // Intense violet
                "EnigmaVariations" or "Enigma" => new Color(100, 255, 150), // Intense green
                "Fate" => new Color(255, 100, 150),      // Bright pink
                "DiesIrae" => new Color(255, 50, 50),    // Bright red
                "ClairDeLune" => new Color(100, 150, 255), // Intense blue
                "OdeToJoy" => new Color(255, 150, 50),   // Intense gold
                "Nachtmusik" => new Color(255, 200, 100), // Intense warm
                "Spring" => new Color(255, 100, 150),    // Intense pink
                "Summer" => new Color(255, 100, 50),     // Intense orange
                "Autumn" => new Color(255, 120, 50),     // Intense amber
                "Winter" => new Color(100, 200, 255),    // Intense ice
                _ => new Color(255, 100, 100)            // Default red
            };
        }
        
        #endregion
        
        #region VFX Helpers
        
        private static void SpawnBloomBurst(Vector2 position, Color color, float scale)
        {
            var particle = new BloomParticle(
                position,
                Vector2.Zero,
                color,
                scale * 0.5f,
                scale,
                20
            );
            MagnumParticleHandler.SpawnParticle(particle);
        }
        
        private static void SpawnExpandingHalo(Vector2 position, Color color, float scale, int lifetime)
        {
            var halo = new BloomRingParticle(
                position,
                Vector2.Zero,
                color,
                scale * 0.3f,
                lifetime,
                scale * 0.05f
            );
            MagnumParticleHandler.SpawnParticle(halo);
        }
        
        private static void SpawnSparkle(Vector2 position, Vector2 velocity, Color color, int lifetime)
        {
            var sparkle = new SparkleParticle(
                position,
                velocity,
                color,
                Main.rand.NextFloat(0.3f, 0.5f),
                lifetime
            );
            MagnumParticleHandler.SpawnParticle(sparkle);
        }
        
        private static void SpawnMusicNoteBurst(Vector2 position, Color color, int count, float spread)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Vector2 pos = position + Main.rand.NextVector2Circular(spread * 0.3f, spread * 0.3f);
                
                // Use music note dust or themed particles
                int dustType = Main.rand.Next(new[] { 15, 57, 58, 59, 60 }); // Various sparkle dusts
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, 0, color, 1.5f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }
        }
        
        private static void SpawnThemedParticles(string theme, Vector2 position, int count, float spread)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Vector2 pos = position + Main.rand.NextVector2Circular(spread * 0.5f, spread * 0.5f);
                
                Color color = Color.Lerp(
                    GetThemePrimaryColor(theme),
                    GetThemeSecondaryColor(theme),
                    Main.rand.NextFloat()
                );
                
                // Spawn themed dust
                int dustType = GetThemeDustType(theme);
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, 0, color, Main.rand.NextFloat(1.2f, 1.8f));
                d.noGravity = true;
                d.fadeIn = 1f;
            }
        }
        
        private static int GetThemeDustType(string theme)
        {
            return theme switch
            {
                "Eroica" => 6,                    // Fire-like
                "LaCampanella" => 6,              // Fire
                "SwanLake" => 15,                 // White sparkle
                "MoonlightSonata" => 27,          // Purple
                "EnigmaVariations" or "Enigma" => 27, // Purple
                "Fate" => 272,                    // Cosmic
                "DiesIrae" => 60,                 // Blood red
                "ClairDeLune" => 15,              // Light sparkle
                "OdeToJoy" => 57,                 // Gold
                "Nachtmusik" => 57,               // Gold
                "Spring" => 75,                   // Pink
                "Summer" => 6,                    // Fire
                "Autumn" => 64,                   // Amber
                "Winter" => 92,                   // Ice
                _ => 15                           // Default sparkle
            };
        }
        
        #endregion
        
        #region Boss Movement Helpers
        
        /// <summary>
        /// Smoothly move an entity toward a target position.
        /// </summary>
        /// <param name="current">Current position</param>
        /// <param name="target">Target position</param>
        /// <param name="speed">Movement speed</param>
        /// <returns>New velocity vector</returns>
        public static Vector2 FluidMoveToward(Vector2 current, Vector2 target, float speed)
        {
            Vector2 direction = target - current;
            float distance = direction.Length();
            
            if (distance < 1f)
                return Vector2.Zero;
            
            direction.Normalize();
            return direction * MathHelper.Min(speed, distance);
        }
        
        /// <summary>
        /// Smoothly move an NPC toward a target position with acceleration and lerp smoothing.
        /// </summary>
        /// <param name="npc">The NPC to move</param>
        /// <param name="targetPos">Target position to move toward</param>
        /// <param name="speed">Maximum movement speed</param>
        /// <param name="acceleration">Acceleration rate</param>
        /// <param name="lerpFactor">Velocity smoothing factor (0-1)</param>
        public static void FluidMoveToward(Terraria.NPC npc, Vector2 targetPos, float speed, float acceleration, float lerpFactor)
        {
            Vector2 direction = targetPos - npc.Center;
            float distance = direction.Length();
            
            if (distance < 1f)
            {
                npc.velocity *= 0.9f;
                return;
            }
            
            direction.Normalize();
            
            // Calculate target velocity based on distance
            float targetSpeed = MathHelper.Min(speed, distance * acceleration * 10f);
            Vector2 targetVelocity = direction * targetSpeed;
            
            // Smoothly lerp toward target velocity
            npc.velocity = Vector2.Lerp(npc.velocity, targetVelocity, lerpFactor);
        }
        
        /// <summary>
        /// Smoothly orbit an NPC around a center point.
        /// </summary>
        /// <param name="npc">The NPC to move</param>
        /// <param name="center">Center point to orbit around</param>
        /// <param name="radius">Orbital radius</param>
        /// <param name="spinSpeed">Angular velocity in radians per frame</param>
        /// <param name="lerpFactor">Smoothing factor (0-1)</param>
        public static void FluidOrbitAround(Terraria.NPC npc, Vector2 center, float radius, float spinSpeed, float lerpFactor)
        {
            // Calculate current angle from center to NPC
            Vector2 toNPC = npc.Center - center;
            float currentAngle = toNPC.ToRotation();
            
            // Increment angle
            float newAngle = currentAngle + spinSpeed;
            
            // Calculate target position on orbit
            Vector2 targetPos = center + new Vector2(
                (float)System.Math.Cos(newAngle) * radius,
                (float)System.Math.Sin(newAngle) * radius
            );
            
            // Smoothly move NPC toward target position
            npc.velocity = Vector2.Lerp(npc.velocity, (targetPos - npc.Center) * 0.5f, lerpFactor);
        }
        
        #endregion
        
        #region Attack VFX Methods
        
        /// <summary>
        /// VFX for attack release moment.
        /// </summary>
        /// <param name="theme">Theme name</param>
        /// <param name="position">World position</param>
        /// <param name="scale">Effect scale multiplier</param>
        public static void AttackRelease(string theme, Vector2 position, float scale = 1f)
        {
            Color primary = GetThemePrimaryColor(theme);
            Color secondary = GetThemeSecondaryColor(theme);
            
            // Central flash
            SpawnBloomBurst(position, Color.White, 0.8f * scale);
            SpawnBloomBurst(position, primary, 1.0f * scale);
            
            // Expanding halo
            SpawnExpandingHalo(position, primary, 0.5f * scale, 15);
            SpawnExpandingHalo(position, secondary, 0.35f * scale, 12);
            
            // Sparkle burst
            int sparkleCount = (int)(8 * scale);
            for (int i = 0; i < sparkleCount; i++)
            {
                float angle = MathHelper.TwoPi * i / sparkleCount;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f) * scale;
                SpawnSparkle(position, vel, primary, 20);
            }
            
            // Light
            Lighting.AddLight(position, primary.ToVector3() * 1.5f * scale);
        }
        
        /// <summary>
        /// VFX for phase transition.
        /// </summary>
        /// <param name="theme">Theme name</param>
        /// <param name="position">World position</param>
        public static void OnPhaseTransition(string theme, Vector2 position)
        {
            Color primary = GetThemePrimaryColor(theme);
            Color secondary = GetThemeSecondaryColor(theme);
            
            // Intense flash cascade
            SpawnBloomBurst(position, Color.White, 1.5f);
            SpawnBloomBurst(position, primary, 1.2f);
            SpawnBloomBurst(position, secondary, 0.9f);
            
            // Multiple halos
            for (int i = 0; i < 6; i++)
            {
                Color haloColor = Color.Lerp(primary, secondary, i / 6f);
                SpawnExpandingHalo(position, haloColor, 0.4f + i * 0.15f, 15 + i * 2);
            }
            
            // Screen effects
            ScreenDistortionManager.TriggerRipple(position, primary, 0.5f, 20);
            MagnumScreenEffects.AddScreenShake(12f);
            
            // Theme particles
            SpawnThemedParticles(theme, position, 20, 100f);
            
            Lighting.AddLight(position, primary.ToVector3() * 2f);
        }
        
        /// <summary>
        /// Telegraph VFX for dash attack.
        /// </summary>
        /// <param name="position">Start position</param>
        /// <param name="direction">Dash direction (normalized)</param>
        /// <param name="length">Length of the dash path</param>
        /// <param name="duration">Duration in frames for the telegraph</param>
        /// <param name="color">Color for the warning line</param>
        public static void DashAttackTelegraph(Vector2 position, Vector2 direction, float length, int duration, Color color)
        {
            // Warning line particles along path
            int segments = (int)(length / 30f);
            for (int i = 0; i < segments; i++)
            {
                float progress = (float)i / segments;
                Vector2 pos = position + direction * (progress * length);
                
                // Spawn warning flare
                var flare = new BloomParticle(
                    pos,
                    Vector2.Zero,
                    color * 0.6f,
                    0.2f,
                    0.3f,
                    duration
                );
                MagnumParticleHandler.SpawnParticle(flare);
                
                // Light along path
                Lighting.AddLight(pos, color.ToVector3() * 0.5f);
            }
            
            // Converging particles at start
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 offset = angle.ToRotationVector2() * 40f;
                Vector2 vel = -offset.SafeNormalize(Vector2.Zero) * 3f;
                SpawnSparkle(position + offset, vel, color, duration);
            }
        }
        
        /// <summary>
        /// Warning VFX for dive attack impact zone.
        /// </summary>
        /// <param name="position">Impact position</param>
        /// <param name="radius">Impact radius</param>
        /// <param name="progress">Animation progress (0-1)</param>
        /// <param name="theme">Theme name</param>
        public static void DiveImpactWarning(Vector2 position, float radius, float progress, string theme)
        {
            Color primary = GetThemePrimaryColor(theme);
            
            // Scale effect based on progress (builds up)
            float currentRadius = radius * (0.3f + progress * 0.7f);
            float alpha = 0.3f + progress * 0.5f;
            
            // Ground impact warning circle
            int points = (int)(currentRadius / 20f) + 6;
            for (int i = 0; i < points; i++)
            {
                float angle = MathHelper.TwoPi * i / points;
                Vector2 pos = position + angle.ToRotationVector2() * currentRadius;
                
                var flare = new BloomParticle(
                    pos,
                    Vector2.Zero,
                    primary * alpha,
                    0.15f + progress * 0.1f,
                    0.25f + progress * 0.15f,
                    5
                );
                MagnumParticleHandler.SpawnParticle(flare);
            }
            
            // Central warning pulse
            SpawnExpandingHalo(position, primary * alpha, 0.3f + progress * 0.2f, 8);
        }
        
        /// <summary>
        /// VFX for ultimate attack release.
        /// </summary>
        /// <param name="theme">Theme name</param>
        /// <param name="position">World position</param>
        /// <param name="scale">Effect scale multiplier</param>
        public static void UltimateAttackRelease(string theme, Vector2 position, float scale = 1f)
        {
            Color primary = GetThemePrimaryColor(theme);
            Color secondary = GetThemeSecondaryColor(theme);
            Color enrage = GetThemeEnrageColor(theme);
            
            // Massive flash cascade
            SpawnBloomBurst(position, Color.White, 2.0f);
            SpawnBloomBurst(position, primary, 1.8f);
            SpawnBloomBurst(position, secondary, 1.5f);
            SpawnBloomBurst(position, enrage, 1.2f);
            
            // Many expanding halos
            for (int i = 0; i < 8; i++)
            {
                Color haloColor = Color.Lerp(primary, enrage, i / 8f);
                SpawnExpandingHalo(position, haloColor, 0.5f + i * 0.2f, 18 + i * 3);
            }
            
            // Massive sparkle burst
            for (int i = 0; i < 24; i++)
            {
                float angle = MathHelper.TwoPi * i / 24f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(8f, 14f);
                Color sparkColor = Color.Lerp(primary, enrage, Main.rand.NextFloat());
                SpawnSparkle(position, vel, sparkColor, 35);
            }
            
            // Screen effects
            ScreenDistortionManager.TriggerRipple(position, enrage, 1.0f, 35);
            MagnumScreenEffects.AddScreenShake(18f);
            
            // God rays
            GodRaySystem.CreateBurst(position, primary, 8, 120f, 40);
            
            // Intense lighting
            Lighting.AddLight(position, enrage.ToVector3() * 2.5f);
        }
        
        /// <summary>
        /// Show a safe zone indicator for players.
        /// </summary>
        /// <param name="center">Center of safe zone</param>
        /// <param name="radius">Radius of safe zone</param>
        /// <param name="duration">Duration in frames for the indicator</param>
        public static void ShowSafeZone(Vector2 center, float radius, int duration)
        {
            Color safeColor = Color.Cyan;
            
            // Ring of particles showing safe area
            int points = (int)(radius / 20f) + 8;
            for (int i = 0; i < points; i++)
            {
                float angle = MathHelper.TwoPi * i / points;
                Vector2 pos = center + angle.ToRotationVector2() * radius;
                
                var flare = new BloomParticle(
                    pos,
                    Vector2.Zero,
                    safeColor * 0.6f,
                    0.15f,
                    0.25f,
                    duration
                );
                MagnumParticleHandler.SpawnParticle(flare);
            }
        }
        
        #endregion
        
        #region Screen Effects
        
        /// <summary>
        /// Set chromatic aberration intensity.
        /// </summary>
        /// <param name="intensity">Intensity (0-1)</param>
        public static void SetChromaticAberration(float intensity)
        {
            // Store for shader use - would integrate with screen shader system
            // For now, simulate with screen shake if intense
            if (intensity > 0.5f)
            {
                MagnumScreenEffects.AddScreenShake(intensity * 5f);
            }
        }
        
        /// <summary>
        /// Set vignette effect.
        /// </summary>
        /// <param name="intensity">Intensity (0-1)</param>
        public static void SetVignette(float intensity)
        {
            // Would integrate with screen shader system
            // For now, add dramatic lighting at screen edges
            if (intensity > 0.3f)
            {
                Vector2 playerPos = Main.LocalPlayer.Center;
                Lighting.AddLight(playerPos, new Vector3(0.5f, 0.3f, 0.3f) * intensity * 0.5f);
            }
        }
        
        /// <summary>
        /// Set theme-specific skybox/background effect.
        /// </summary>
        /// <param name="theme">Theme name</param>
        /// <param name="intensity">Effect intensity (0-1)</param>
        public static void SetThemeSkybox(string theme, float intensity)
        {
            // Would trigger sky effect system
            // For now, just add ambient lighting
            Color primary = GetThemePrimaryColor(theme);
            Vector2 playerPos = Main.LocalPlayer.Center;
            Lighting.AddLight(playerPos, primary.ToVector3() * intensity * 0.5f);
        }
        
        #endregion
        
        #region Boss Despawn
        
        /// <summary>
        /// VFX for when a boss despawns (escapes, player dies, etc.).
        /// Uses current player position for effects.
        /// </summary>
        public static void OnBossDespawn()
        {
            Vector2 position = Main.LocalPlayer.Center;
            Color primary = new Color(150, 100, 200); // Generic purple
            Color secondary = new Color(100, 150, 220);
            
            // Fading flash
            SpawnBloomBurst(position, primary, 1.0f);
            
            // Collapsing halos (reverse of spawn)
            for (int i = 0; i < 4; i++)
            {
                Color haloColor = Color.Lerp(primary, secondary, i / 4f) * 0.5f;
                SpawnExpandingHalo(position, haloColor, 0.8f - i * 0.15f, 20);
            }
            
            // Subtle screen effect
            ScreenDistortionManager.TriggerRipple(position, primary, 0.3f, 15);
        }
        
        /// <summary>
        /// VFX for when a boss despawns (escapes, player dies, etc.) - themed version.
        /// </summary>
        /// <param name="theme">Theme name</param>
        /// <param name="position">Last known position</param>
        public static void OnBossDespawn(string theme, Vector2 position)
        {
            Color primary = GetThemePrimaryColor(theme);
            Color secondary = GetThemeSecondaryColor(theme);
            
            // Fading flash
            SpawnBloomBurst(position, primary, 1.0f);
            
            // Collapsing halos (reverse of spawn)
            for (int i = 0; i < 4; i++)
            {
                Color haloColor = Color.Lerp(primary, secondary, i / 4f) * 0.5f;
                SpawnExpandingHalo(position, haloColor, 0.8f - i * 0.15f, 20);
            }
            
            // Fading particles
            SpawnThemedParticles(theme, position, 10, 60f);
            
            // Subtle screen effect
            ScreenDistortionManager.TriggerRipple(position, primary, 0.3f, 15);
        }
        
        #endregion
        
        #region Public Color Accessors (for external use)
        
        /// <summary>
        /// Get the theme's color cycling based on progress.
        /// </summary>
        /// <param name="theme">Theme name</param>
        /// <param name="progress">Progress through the color cycle (0-1, wraps)</param>
        /// <returns>Interpolated theme color</returns>
        public static Color GetThemeColor(string theme, float progress)
        {
            Color primary = GetThemePrimaryColor(theme);
            Color secondary = GetThemeSecondaryColor(theme);
            Color enrage = GetThemeEnrageColor(theme);
            
            // Wrap progress to 0-1 range
            progress = progress % 1f;
            if (progress < 0) progress += 1f;
            
            // Create smooth cycling through theme colors
            if (progress < 0.33f)
            {
                return Color.Lerp(primary, secondary, progress * 3f);
            }
            else if (progress < 0.67f)
            {
                return Color.Lerp(secondary, enrage, (progress - 0.33f) * 3f);
            }
            else
            {
                return Color.Lerp(enrage, primary, (progress - 0.67f) * 3f);
            }
        }
        
        /// <summary>
        /// Get the theme's primary color (single color, no cycling).
        /// </summary>
        public static Color GetThemePrimary(string theme) => GetThemePrimaryColor(theme);
        
        /// <summary>
        /// Get the theme's secondary color.
        /// </summary>
        public static Color GetThemeSecondary(string theme) => GetThemeSecondaryColor(theme);
        
        /// <summary>
        /// Get the theme's enrage color.
        /// </summary>
        public static Color GetThemeEnrage(string theme) => GetThemeEnrageColor(theme);
        
        #endregion
    }
}
