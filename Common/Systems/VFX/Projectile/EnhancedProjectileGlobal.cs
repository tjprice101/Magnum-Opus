using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// GlobalProjectile that automatically applies Calamity-style buttery smooth rendering
    /// to ALL MagnumOpus projectiles without requiring inheritance.
    /// </summary>
    public class EnhancedProjectileGlobal : GlobalProjectile
    {
        // Per-projectile state (instanced)
        public override bool InstancePerEntity => true;
        
        #region State
        
        private List<Vector2> positionHistory = new List<Vector2>();
        private List<float> rotationHistory = new List<float>();
        private float pulsePhase;
        private float orbitAngle;
        private string detectedTheme;
        private Color[] themePalette;
        private bool initialized;
        private int trailLength = 16;
        
        #endregion
        
        #region Configuration
        
        // Default settings - can be overridden per projectile type
        private static readonly Dictionary<int, ProjectileVFXConfig> CustomConfigs = new Dictionary<int, ProjectileVFXConfig>();
        
        public class ProjectileVFXConfig
        {
            public float BloomIntensity = 0.8f;
            public int BloomLayers = 4;
            public int TrailLength = 16;
            public bool HasOrbitingParticles = true;
            public int OrbitCount = 3;
            public float OrbitRadius = 15f;
            public bool HasMusicNotes = true;
            public int MusicNoteChance = 6;
            public float ParticleDensity = 1f;
            public bool UseBezierTrails = true;
            public bool EnableScreenEffects = true;
            public float ScreenEffectIntensity = 0.3f;
        }
        
        /// <summary>
        /// Register a custom VFX configuration for a specific projectile type.
        /// </summary>
        public static void RegisterConfig(int projectileType, ProjectileVFXConfig config)
        {
            CustomConfigs[projectileType] = config;
        }
        
        private ProjectileVFXConfig GetConfig(Projectile projectile)
        {
            if (CustomConfigs.TryGetValue(projectile.type, out var config))
                return config;
            return DefaultConfig;
        }
        
        private static readonly ProjectileVFXConfig DefaultConfig = new ProjectileVFXConfig();
        
        #endregion
        
        #region Theme Detection
        
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            // MASTER TOGGLE: When disabled, this global system does nothing
            // Each projectile implements its own unique VFX instead
            if (!VFXMasterToggle.GlobalSystemsEnabled)
                return false;
            
            // Only apply to MagnumOpus projectiles
            if (entity.ModProjectile == null) return false;
            
            // Exclude debug weapons
            if (VFXExclusionHelper.ShouldExcludeProjectile(entity)) return false;
            
            string fullName = entity.ModProjectile.GetType().FullName ?? "";
            return fullName.Contains("MagnumOpus");
        }
        
        private void Initialize(Projectile projectile)
        {
            if (initialized) return;
            
            detectedTheme = DetectProjectileTheme(projectile);
            themePalette = MagnumThemePalettes.GetThemePalette(detectedTheme);
            trailLength = GetConfig(projectile).TrailLength;
            initialized = true;
        }
        
        private string DetectProjectileTheme(Projectile projectile)
        {
            string fullName = projectile.ModProjectile?.GetType().FullName ?? "";
            string name = projectile.ModProjectile?.GetType().Name ?? "";
            
            // Check namespace for theme
            if (fullName.Contains(".Fate.") || fullName.Contains("Fate")) return "Fate";
            if (fullName.Contains(".Eroica.") || fullName.Contains("Eroica")) return "Eroica";
            if (fullName.Contains(".SwanLake.") || fullName.Contains("Swan")) return "SwanLake";
            if (fullName.Contains(".LaCampanella.") || fullName.Contains("Campanella")) return "LaCampanella";
            if (fullName.Contains(".EnigmaVariations.") || fullName.Contains("Enigma")) return "EnigmaVariations";
            if (fullName.Contains(".MoonlightSonata.") || fullName.Contains("Moonlight")) return "MoonlightSonata";
            if (fullName.Contains(".DiesIrae.") || fullName.Contains("Dies")) return "DiesIrae";
            if (fullName.Contains(".ClairDeLune.") || fullName.Contains("Clair")) return "ClairDeLune";
            if (fullName.Contains(".OdeToJoy.") || fullName.Contains("Ode")) return "OdeToJoy";
            if (fullName.Contains(".Nachtmusik.") || fullName.Contains("Nacht")) return "Nachtmusik";
            if (fullName.Contains(".Spring.") || fullName.Contains("Spring")) return "Spring";
            if (fullName.Contains(".Summer.") || fullName.Contains("Summer")) return "Summer";
            if (fullName.Contains(".Autumn.") || fullName.Contains("Autumn")) return "Autumn";
            if (fullName.Contains(".Winter.") || fullName.Contains("Winter")) return "Winter";
            
            return "Eroica"; // Default fallback
        }
        
        #endregion
        
        #region AI Hook
        
        public override void AI(Projectile projectile)
        {
            Initialize(projectile);
            
            // Update state
            pulsePhase += 0.1f;
            orbitAngle += 0.08f;
            
            // Record position history
            RecordPosition(projectile);
            
            // Register with advanced renderer for sub-frame interpolation
            AdvancedProjectileRenderer.RegisterProjectile(projectile);
            
            var config = GetConfig(projectile);
            
            // Spawn visual effects based on configuration
            SpawnTrailEffects(projectile, config);
            SpawnOrbitingParticles(projectile, config);
            SpawnMusicNotes(projectile, config);
            ApplyDynamicLighting(projectile, config);
        }
        
        private void RecordPosition(Projectile projectile)
        {
            positionHistory.Insert(0, projectile.Center);
            rotationHistory.Insert(0, projectile.rotation);
            
            while (positionHistory.Count > trailLength)
            {
                positionHistory.RemoveAt(positionHistory.Count - 1);
                rotationHistory.RemoveAt(rotationHistory.Count - 1);
            }
        }
        
        #endregion
        
        #region Visual Effect Spawning
        
        private void SpawnTrailEffects(Projectile projectile, ProjectileVFXConfig config)
        {
            int frameInterval = Math.Max(1, (int)(3 / config.ParticleDensity));
            if (Main.GameUpdateCount % frameInterval != 0) return;
            
            Color primaryColor = VFXUtilities.PaletteLerp(themePalette, 0.2f);
            Color secondaryColor = VFXUtilities.PaletteLerp(themePalette, 0.6f);
            
            // Theme-specific unique trails
            UniqueTrailStyles.SpawnUniqueTrail(projectile.Center, -projectile.velocity * 0.1f, detectedTheme, projectile.DamageType, themePalette);
            
            // Dense dust trail (Iridescent Wingspan pattern)
            for (int i = 0; i < (int)(2 * config.ParticleDensity); i++)
            {
                Vector2 dustOffset = Main.rand.NextVector2Circular(6f, 6f);
                Vector2 dustVel = -projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                
                float colorProgress = (Main.GameUpdateCount * 0.02f + i * 0.1f) % 1f;
                Color dustColor = Color.Lerp(primaryColor, secondaryColor, colorProgress);
                
                var glow = new GenericGlowParticle(
                    projectile.Center + dustOffset,
                    dustVel,
                    dustColor * 0.7f,
                    0.25f + Main.rand.NextFloat(0.1f),
                    18,
                    true
                );
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Contrasting sparkle accents (1 in 2)
            if (Main.rand.NextBool(2))
            {
                Vector2 sparklePos = projectile.Center + Main.rand.NextVector2Circular(10f, 10f);
                var sparkle = new SparkleParticle(sparklePos, Main.rand.NextVector2Circular(2f, 2f), Color.White * 0.7f, 0.3f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Frequent flares littering the air (1 in 2)
            if (Main.rand.NextBool(2))
            {
                Vector2 flarePos = projectile.Center + Main.rand.NextVector2Circular(8f, 8f);
                CustomParticles.GenericFlare(flarePos, primaryColor, 0.4f, 15);
            }
            
            // Color oscillation using Main.hslToRgb
            if (Main.rand.NextBool(3))
            {
                float hue = (Main.GameUpdateCount * 0.02f + Main.rand.NextFloat(0.1f)) % 1f;
                Color shiftedColor;
                
                // Theme-specific hue ranges
                if (detectedTheme == "SwanLake")
                {
                    shiftedColor = Main.hslToRgb(hue, 1f, 0.85f); // Full rainbow
                }
                else
                {
                    // Constrain to theme's color range
                    float[] hueRange = GetThemeHueRange(detectedTheme);
                    hue = hueRange[0] + (hue * (hueRange[1] - hueRange[0]));
                    shiftedColor = Main.hslToRgb(hue, 0.9f, 0.75f);
                }
                
                CustomParticles.GenericFlare(projectile.Center, shiftedColor, 0.35f, 12);
            }
            
            // Bézier curve trail spawning
            if (config.UseBezierTrails && positionHistory.Count >= 4 && Main.rand.NextBool(3))
            {
                Vector2[] trail = BezierWeaponTrails.GenerateFlowingTrail(positionHistory.ToArray(), 6);
                if (trail.Length > 0)
                {
                    BezierWeaponTrails.SpawnParticlesAlongCurve(trail, themePalette, detectedTheme, 0.08f);
                }
            }
        }
        
        private void SpawnOrbitingParticles(Projectile projectile, ProjectileVFXConfig config)
        {
            if (!config.HasOrbitingParticles) return;
            if (Main.GameUpdateCount % 4 != 0) return;
            
            Color primaryColor = VFXUtilities.PaletteLerp(themePalette, 0.3f);
            
            for (int i = 0; i < config.OrbitCount; i++)
            {
                float angle = orbitAngle + MathHelper.TwoPi * i / config.OrbitCount;
                float wobble = (float)Math.Sin(pulsePhase * 2f + i * 0.5f) * 3f;
                Vector2 orbitPos = projectile.Center + angle.ToRotationVector2() * (config.OrbitRadius + wobble);
                
                float hue = (angle / MathHelper.TwoPi + Main.GameUpdateCount * 0.01f) % 1f;
                Color orbitColor;
                
                if (detectedTheme == "SwanLake")
                {
                    orbitColor = Main.hslToRgb(hue, 0.9f, 0.7f); // Rainbow for Swan Lake
                }
                else
                {
                    orbitColor = Color.Lerp(primaryColor, VFXUtilities.PaletteLerp(themePalette, hue), 0.5f);
                }
                
                CustomParticles.GenericFlare(orbitPos, orbitColor, 0.15f, 8);
            }
        }
        
        private void SpawnMusicNotes(Projectile projectile, ProjectileVFXConfig config)
        {
            if (!config.HasMusicNotes) return;
            if (!Main.rand.NextBool(config.MusicNoteChance)) return;
            
            Color noteColor = VFXUtilities.PaletteLerp(themePalette, Main.rand.NextFloat());
            Vector2 noteVel = -projectile.velocity * 0.05f + Main.rand.NextVector2Circular(1f, 1f);
            noteVel.Y -= 0.5f; // Float upward
            
            // VISIBLE scale (0.7f+)
            float noteScale = Main.rand.NextFloat(0.7f, 0.95f);
            
            ThemedParticles.MusicNote(projectile.Center + Main.rand.NextVector2Circular(8f, 8f), noteVel, noteColor, noteScale, 35);
            
            // Sparkle companion for visibility
            var sparkle = new SparkleParticle(projectile.Center, noteVel * 0.5f, Color.White * 0.5f, 0.2f, 20);
            MagnumParticleHandler.SpawnParticle(sparkle);
        }
        
        private void ApplyDynamicLighting(Projectile projectile, ProjectileVFXConfig config)
        {
            Color lightColor = VFXUtilities.PaletteLerp(themePalette, 0.3f);
            float pulse = 0.8f + (float)Math.Sin(pulsePhase) * 0.2f;
            Lighting.AddLight(projectile.Center, lightColor.ToVector3() * pulse * config.BloomIntensity);
        }
        
        private float[] GetThemeHueRange(string theme)
        {
            return theme switch
            {
                "LaCampanella" => new[] { 0.05f, 0.12f },  // Orange range
                "Eroica" => new[] { 0.95f, 0.08f },        // Red-gold range
                "MoonlightSonata" => new[] { 0.65f, 0.75f }, // Purple-blue range
                "EnigmaVariations" => new[] { 0.75f, 0.35f }, // Purple-green range
                "Fate" => new[] { 0.85f, 0.95f },          // Pink-magenta range
                "DiesIrae" => new[] { 0.98f, 0.05f },      // Deep red range
                "ClairDeLune" => new[] { 0.55f, 0.65f },   // Blue range
                "Spring" => new[] { 0.88f, 0.95f },        // Pink range
                "Summer" => new[] { 0.08f, 0.18f },        // Yellow-orange range
                "Autumn" => new[] { 0.05f, 0.12f },        // Orange-brown range
                "Winter" => new[] { 0.52f, 0.62f },        // Ice blue range
                _ => new[] { 0f, 1f }                       // Full range
            };
        }
        
        #endregion
        
        #region Kill Hook
        
        public override void OnKill(Projectile projectile, int timeLeft)
        {
            Initialize(projectile);
            
            var config = GetConfig(projectile);
            
            // Spawn impact effects
            SpawnImpactEffects(projectile, config);
            
            // Trigger screen effects
            if (config.EnableScreenEffects)
            {
                TriggerScreenEffects(projectile, config);
            }
            
            // Clean up
            AdvancedProjectileRenderer.UnregisterProjectile(projectile.whoAmI);
            BezierWeaponTrails.ClearProjectileTrail(projectile.whoAmI);
        }
        
        private void SpawnImpactEffects(Projectile projectile, ProjectileVFXConfig config)
        {
            Color primaryColor = VFXUtilities.PaletteLerp(themePalette, 0.2f);
            Color secondaryColor = VFXUtilities.PaletteLerp(themePalette, 0.6f);
            Color accentColor = VFXUtilities.PaletteLerp(themePalette, 0.8f);
            
            // === CENTRAL GLIMMER CASCADE ===
            
            // White flash core
            CustomParticles.GenericFlare(projectile.Center, Color.White, 1.0f * config.BloomIntensity, 22);
            
            // Layered theme flares
            for (int layer = 0; layer < config.BloomLayers; layer++)
            {
                float progress = (float)layer / config.BloomLayers;
                Color layerColor = Color.Lerp(primaryColor, secondaryColor, progress);
                float layerScale = (0.8f - layer * 0.15f) * config.BloomIntensity;
                int layerLife = 20 - layer * 2;
                CustomParticles.GenericFlare(projectile.Center, layerColor, layerScale, layerLife);
            }
            
            // === EXPANDING HALO RINGS ===
            for (int ring = 0; ring < 4; ring++)
            {
                float progress = (float)ring / 4f;
                Color ringColor = Color.Lerp(primaryColor, accentColor, progress);
                float ringScale = 0.3f + ring * 0.15f;
                int ringLife = 14 + ring * 3;
                CustomParticles.HaloRing(projectile.Center, ringColor, ringScale, ringLife);
            }
            
            // === RADIAL SPARKLE BURST ===
            int sparkleCount = (int)(12 * config.ParticleDensity);
            for (int i = 0; i < sparkleCount; i++)
            {
                float angle = MathHelper.TwoPi * i / sparkleCount;
                Vector2 sparkleVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Color sparkleColor = Color.Lerp(primaryColor, Color.White, (float)i / sparkleCount * 0.5f);
                
                var sparkle = new SparkleParticle(projectile.Center, sparkleVel, sparkleColor, 0.4f, 25);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // === THEMED PARTICLE BURST ===
            UniqueTrailStyles.SpawnUniqueImpact(projectile.Center, detectedTheme, projectile.DamageType, themePalette, 1.2f);
            
            // === MUSIC NOTE FINALE ===
            if (config.HasMusicNotes)
            {
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f;
                    Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                    Color noteColor = Color.Lerp(primaryColor, secondaryColor, (float)i / 6f);
                    ThemedParticles.MusicNote(projectile.Center, noteVel, noteColor, 0.8f, 30);
                }
            }
            
            // === DUST EXPLOSION ===
            for (int i = 0; i < (int)(15 * config.ParticleDensity); i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(6f, 6f);
                Color dustColor = Color.Lerp(primaryColor, secondaryColor, Main.rand.NextFloat());
                
                var glow = new GenericGlowParticle(projectile.Center, dustVel, dustColor * 0.8f, 0.35f, 22, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // === BRIGHT LIGHTING PULSE ===
            Lighting.AddLight(projectile.Center, primaryColor.ToVector3() * 1.5f);
        }
        
        private void TriggerScreenEffects(Projectile projectile, ProjectileVFXConfig config)
        {
            if (config.ScreenEffectIntensity <= 0f) return;
            
            // Screen shake for significant impacts
            if (config.ScreenEffectIntensity > 0.2f)
            {
                float distance = Vector2.Distance(Main.LocalPlayer.Center, projectile.Center);
                if (distance < 800f)
                {
                    float falloff = 1f - (distance / 800f);
                    float shakeIntensity = config.ScreenEffectIntensity * falloff * 4f;
                    MagnumScreenEffects.AddScreenShake(shakeIntensity);
                }
            }
            
            // Theme-specific screen effects
            ScreenDistortionManager.TriggerThemeEffect(detectedTheme, projectile.Center, config.ScreenEffectIntensity);
        }
        
        #endregion
        
        #region Rendering
        
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Initialize(projectile);
            
            var config = GetConfig(projectile);
            
            // Draw multi-layer bloom behind the projectile
            DrawBloomLayers(projectile, config);
            
            // Draw Bézier trail
            if (config.UseBezierTrails && positionHistory.Count >= 3)
            {
                DrawBezierTrail(projectile, config);
            }
            else
            {
                DrawSimpleTrail(projectile, config);
            }
            
            // Draw orbiting elements
            if (config.HasOrbitingParticles)
            {
                DrawOrbitingElements(projectile, config);
            }
            
            return true; // Draw default sprite on top
        }
        
        private void DrawBloomLayers(Projectile projectile, ProjectileVFXConfig config)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D softGlow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2", AssetRequestMode.ImmediateLoad).Value;
            Texture2D flare = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare", AssetRequestMode.ImmediateLoad).Value;
            
            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Vector2 glowOrigin = softGlow.Size() * 0.5f;
            Vector2 flareOrigin = flare.Size() * 0.5f;
            
            float pulse = 1f + (float)Math.Sin(pulsePhase) * 0.15f;
            float baseScale = projectile.scale * 0.5f;
            
            Color primaryColor = VFXUtilities.PaletteLerp(themePalette, 0.2f);
            Color secondaryColor = VFXUtilities.PaletteLerp(themePalette, 0.6f);
            
            // Switch to additive blending
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Outer ethereal glow
            Color outerGlow = primaryColor.WithoutAlpha() * 0.25f * config.BloomIntensity;
            sb.Draw(softGlow, drawPos, null, outerGlow, 0f, glowOrigin, baseScale * pulse * 2.0f, SpriteEffects.None, 0f);
            
            // Middle layer
            Color middleGlow = secondaryColor.WithoutAlpha() * 0.4f * config.BloomIntensity;
            sb.Draw(softGlow, drawPos, null, middleGlow, 0f, glowOrigin, baseScale * pulse * 1.4f, SpriteEffects.None, 0f);
            
            // Core flare (rotating)
            float rotation = Main.GameUpdateCount * 0.05f;
            Color coreGlow = Color.Lerp(primaryColor, Color.White, 0.3f).WithoutAlpha() * 0.6f * config.BloomIntensity;
            sb.Draw(flare, drawPos, null, coreGlow, rotation, flareOrigin, baseScale * pulse * 0.8f, SpriteEffects.None, 0f);
            
            // White-hot center
            Color whiteCore = Color.White.WithoutAlpha() * 0.5f * config.BloomIntensity;
            sb.Draw(flare, drawPos, null, whiteCore, -rotation * 0.7f, flareOrigin, baseScale * pulse * 0.4f, SpriteEffects.None, 0f);
            
            // Restore normal blending
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        
        private void DrawBezierTrail(Projectile projectile, ProjectileVFXConfig config)
        {
            if (positionHistory.Count < 3) return;
            
            Vector2[] smoothTrail = BezierWeaponTrails.GenerateFlowingTrail(positionHistory.ToArray(), 4);
            if (smoothTrail.Length == 0) return;
            
            SpriteBatch sb = Main.spriteBatch;
            Texture2D softGlow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2", AssetRequestMode.ImmediateLoad).Value;
            Vector2 origin = softGlow.Size() * 0.5f;
            
            Color primaryColor = VFXUtilities.PaletteLerp(themePalette, 0.2f);
            Color secondaryColor = VFXUtilities.PaletteLerp(themePalette, 0.6f);
            
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            for (int i = 0; i < smoothTrail.Length; i++)
            {
                float progress = (float)i / smoothTrail.Length;
                float opacity = (1f - progress) * 0.6f * config.BloomIntensity;
                float scale = (1f - progress * 0.6f) * 0.35f * projectile.scale;
                
                Color trailColor = Color.Lerp(primaryColor, secondaryColor, progress);
                trailColor = trailColor.WithoutAlpha() * opacity;
                
                Vector2 drawPos = smoothTrail[i] - Main.screenPosition;
                sb.Draw(softGlow, drawPos, null, trailColor, 0f, origin, scale, SpriteEffects.None, 0f);
            }
            
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        
        private void DrawSimpleTrail(Projectile projectile, ProjectileVFXConfig config)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = TextureAssets.Projectile[projectile.type].Value;
            Vector2 origin = tex.Size() * 0.5f;
            
            Color primaryColor = VFXUtilities.PaletteLerp(themePalette, 0.2f);
            Color secondaryColor = VFXUtilities.PaletteLerp(themePalette, 0.6f);
            
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            for (int i = 0; i < projectile.oldPos.Length; i++)
            {
                if (projectile.oldPos[i] == Vector2.Zero) continue;
                
                float progress = (float)i / projectile.oldPos.Length;
                float opacity = (1f - progress) * 0.5f * config.BloomIntensity;
                float scale = (1f - progress * 0.5f) * projectile.scale;
                
                Color trailColor = Color.Lerp(primaryColor, secondaryColor, progress);
                trailColor = trailColor.WithoutAlpha() * opacity;
                
                Vector2 drawPos = projectile.oldPos[i] + projectile.Size * 0.5f - Main.screenPosition;
                float rot = projectile.oldRot.Length > i ? projectile.oldRot[i] : projectile.rotation;
                
                sb.Draw(tex, drawPos, null, trailColor, rot, origin, scale, SpriteEffects.None, 0f);
            }
            
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        
        private void DrawOrbitingElements(Projectile projectile, ProjectileVFXConfig config)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D flare = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare", AssetRequestMode.ImmediateLoad).Value;
            Vector2 origin = flare.Size() * 0.5f;
            
            Color primaryColor = VFXUtilities.PaletteLerp(themePalette, 0.3f);
            
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            for (int i = 0; i < config.OrbitCount; i++)
            {
                float angle = orbitAngle + MathHelper.TwoPi * i / config.OrbitCount;
                float wobble = (float)Math.Sin(pulsePhase * 2f + i * 0.5f) * 3f;
                Vector2 orbitPos = projectile.Center + angle.ToRotationVector2() * (config.OrbitRadius + wobble) - Main.screenPosition;
                
                float hue = (angle / MathHelper.TwoPi + Main.GameUpdateCount * 0.01f) % 1f;
                Color orbitColor;
                
                if (detectedTheme == "SwanLake")
                {
                    orbitColor = Main.hslToRgb(hue, 0.9f, 0.7f);
                }
                else
                {
                    orbitColor = Color.Lerp(primaryColor, VFXUtilities.PaletteLerp(themePalette, hue), 0.5f);
                }
                
                orbitColor = orbitColor.WithoutAlpha() * 0.7f * config.BloomIntensity;
                sb.Draw(flare, orbitPos, null, orbitColor, angle, origin, 0.12f, SpriteEffects.None, 0f);
            }
            
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        
        #endregion
    }
}
