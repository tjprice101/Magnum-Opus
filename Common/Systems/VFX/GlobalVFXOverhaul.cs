using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// GLOBAL VFX OVERHAUL SYSTEM
    /// 
    /// This GlobalProjectile automatically applies Calamity-style VFX to ALL MagnumOpus projectiles.
    /// 
    /// Features Applied Automatically:
    /// - Multi-pass primitive trail rendering with bloom
    /// - Sub-pixel interpolation for 144Hz+ smoothness
    /// - Theme-based color cycling
    /// - Music note integration
    /// - Screen distortion on impacts
    /// - Cascading death effects
    /// </summary>
    public class GlobalVFXOverhaul : GlobalProjectile
    {
        // Per-projectile data
        private class ProjectileVFXData
        {
            public int TrailId = -1;
            public BezierProjectileSystem.BezierState BezierState;
            public Vector2 LastPosition;
            public float LastRotation;
            public bool Initialized = false;
            public string DetectedTheme = "generic";
        }
        
        private static Dictionary<int, ProjectileVFXData> _vfxData = new Dictionary<int, ProjectileVFXData>();

        public override bool InstancePerEntity => false;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            // Only apply to MagnumOpus projectiles
            return entity.ModProjectile?.Mod == ModContent.GetInstance<MagnumOpus>();
        }

        public override void SetDefaults(Projectile projectile)
        {
            // Enable trail cache for primitive rendering
            if (projectile.ModProjectile?.Mod == ModContent.GetInstance<MagnumOpus>())
            {
                // Ensure trail positions are tracked
                if (Terraria.ID.ProjectileID.Sets.TrailCacheLength[projectile.type] < 20)
                {
                    Terraria.ID.ProjectileID.Sets.TrailCacheLength[projectile.type] = 20;
                }
            }
        }

        public override void OnSpawn(Projectile projectile, Terraria.DataStructures.IEntitySource source)
        {
            if (projectile.ModProjectile?.Mod != ModContent.GetInstance<MagnumOpus>())
                return;
            
            // Initialize VFX data
            var data = new ProjectileVFXData
            {
                LastPosition = projectile.Center,
                LastRotation = projectile.rotation,
                DetectedTheme = DetectThemeFromProjectile(projectile)
            };
            
            // Create primitive trail
            data.TrailId = AdvancedTrailSystem.CreateThemeTrail(data.DetectedTheme, 22f, maxPoints: 25, intensity: 1f);
            
            _vfxData[projectile.whoAmI] = data;
            
            // Spawn creation VFX
            SpawnCreationEffect(projectile, data.DetectedTheme);
        }

        public override void AI(Projectile projectile)
        {
            if (!_vfxData.TryGetValue(projectile.whoAmI, out var data))
                return;
            
            // === UPDATE PRIMITIVE TRAIL ===
            if (data.TrailId >= 0)
            {
                AdvancedTrailSystem.UpdateTrail(data.TrailId, projectile.Center, projectile.rotation);
            }
            
            // === ENHANCED TRAIL PARTICLES ===
            ApplyEnhancedTrailEffect(projectile, data);
            
            // === ORBITING MUSIC NOTES ===
            ApplyOrbitingMusicNotes(projectile, data);
            
            // === INTERPOLATED LIGHTING ===
            ApplySmoothLighting(projectile, data);
            
            // === NEW: ADVANCED VFX EXTENSIONS ===
            // Apply new Calamity-style systems: Verlet constellations, kinetic ripples, layered rendering
            AdvancedVFXExtensions.ApplyExtendedProjectileVFX(projectile, data.DetectedTheme);
            
            // Update last position for interpolation
            data.LastPosition = projectile.Center;
            data.LastRotation = projectile.rotation;
        }

        /// <summary>
        /// Spawns impact light rays and VFX when projectile hits an NPC.
        /// Creates buttery-smooth collision effects using interpolation.
        /// </summary>
        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!_vfxData.TryGetValue(projectile.whoAmI, out var data))
                return;
            
            // Calculate interpolated hit position for smooth visuals
            Vector2 hitPos = InterpolatedRenderer.GetInterpolatedCenter(projectile);
            
            // Scale impact based on damage dealt (crit = bigger impact)
            float impactScale = hit.Crit ? 1.4f : 1f;
            impactScale *= Math.Clamp(damageDone / 100f, 0.6f, 1.8f);
            
            // === IMPACT LIGHT RAYS - The signature collision effect ===
            ImpactLightRays.SpawnImpactRays(hitPos, data.DetectedTheme, 5, impactScale, includeMusicNotes: true);
            
            // Themed impact flares at hit location
            ThemedMusicalImpactFlares.SpawnImpact(hitPos, data.DetectedTheme, impactScale * 0.7f, includeLightRays: false);
            
            // === NEW: ADVANCED VFX EXTENSIONS - Impact effects ===
            // Apply kinetic ripples, god rays, fractal shatter on crits
            AdvancedVFXExtensions.ApplyExtendedProjectileImpactVFX(projectile, target, data.DetectedTheme, impactScale);
            
            // Additional sparkle burst for hit feedback
            var palette = MagnumThemePalettes.GetThemePalette(data.DetectedTheme);
            Color primary = palette != null && palette.Length > 0 ? palette[0] : Color.White;
            
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 sparkleVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                var sparkle = new SparkleParticle(hitPos, sparkleVel, primary * 0.9f, 0.3f * impactScale, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Bright flash at impact point
            Lighting.AddLight(hitPos, primary.ToVector3() * impactScale * 1.2f);
        }
        
        /// <summary>
        /// Spawns impact light rays when projectile collides with tiles.
        /// </summary>
        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            if (_vfxData.TryGetValue(projectile.whoAmI, out var data))
            {
                // Calculate impact position (where the projectile hit the tile)
                Vector2 hitPos = projectile.Center;
                
                // Spawn impact light rays at tile collision point
                ImpactLightRays.SpawnImpactRays(hitPos, data.DetectedTheme, 4, 0.8f, includeMusicNotes: true);
                
                // Themed impact with reduced scale
                ThemedMusicalImpactFlares.SpawnImpact(hitPos, data.DetectedTheme, 0.6f, includeLightRays: false);
                
                // Quick sparkle burst
                var palette = MagnumThemePalettes.GetThemePalette(data.DetectedTheme);
                Color primary = palette != null && palette.Length > 0 ? palette[0] : Color.White;
                
                for (int i = 0; i < 4; i++)
                {
                    Vector2 sparkleVel = Main.rand.NextVector2Circular(4f, 4f);
                    var sparkle = new SparkleParticle(hitPos, sparkleVel, primary * 0.8f, 0.25f, 15);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
                
                Lighting.AddLight(hitPos, primary.ToVector3() * 0.8f);
            }
            
            return true; // Let the projectile's normal tile collision behavior run
        }

        public override void OnKill(Projectile projectile, int timeLeft)
        {
            if (!_vfxData.TryGetValue(projectile.whoAmI, out var data))
                return;
            
            // End the trail
            if (data.TrailId >= 0)
            {
                AdvancedTrailSystem.EndTrail(data.TrailId);
            }
            
            // === ENHANCED IMPACT SYSTEM ===
            var palette = MagnumThemePalettes.GetThemePalette(data.DetectedTheme);
            
            // 1. Themed musical impact flares (with glint slashes and music notes)
            ThemedMusicalImpactFlares.SpawnImpact(projectile.Center, data.DetectedTheme, 1f, includeLightRays: true);
            
            // 2. Unique trail styles impact (theme+damageClass specific particles)
            UniqueTrailStyles.SpawnUniqueImpact(projectile.Center, data.DetectedTheme, projectile.DamageType, palette, 1f);
            
            // 3. Spectacular death effect (cascading halos, music note burst)
            CalamityStyleVFX.SpectacularDeath(projectile.Center, data.DetectedTheme, 1f);
            
            // === NEW: ADVANCED VFX EXTENSIONS - Death effects ===
            // Apply alpha erosion dissolve, god rays, kinetic ripples
            AdvancedVFXExtensions.ApplyExtendedProjectileDeathVFX(projectile, data.DetectedTheme, 1f);
            
            // === BÉZIER TRAIL CLEANUP ===
            BezierWeaponTrails.ClearProjectileTrail(projectile.whoAmI);
            
            // Clean up
            _vfxData.Remove(projectile.whoAmI);
        }

        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            if (!_vfxData.TryGetValue(projectile.whoAmI, out var data))
                return true;
            
            // Draw enhanced primitive trail (behind the projectile)
            DrawEnhancedPrimitiveTrail(projectile, data);
            
            // Draw pre-bloom layers (behind the projectile sprite)
            DrawPreBloomLayers(projectile, data, lightColor);
            
            return true; // Let the projectile's own PreDraw handle sprite rendering (important for custom projectiles!)
        }

        public override void PostDraw(Projectile projectile, Color lightColor)
        {
            if (!_vfxData.TryGetValue(projectile.whoAmI, out var data))
                return;
            
            // Draw post-glow layers (on top of the projectile sprite)
            DrawPostGlowLayers(projectile, data, lightColor);
        }

        #region VFX Application Methods

        private void SpawnCreationEffect(Projectile projectile, string theme)
        {
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            Color primary = palette != null && palette.Length > 0 ? palette[0] : Color.White;
            Color secondary = palette != null && palette.Length > 1 ? palette[1] : primary;
            
            // Central flash
            CustomParticles.GenericFlare(projectile.Center, Color.White, 0.8f, 15);
            CustomParticles.GenericFlare(projectile.Center, primary, 0.6f, 12);
            
            // Creation ring
            CustomParticles.HaloRing(projectile.Center, primary, 0.3f, 10);
            
            // Radial sparkles
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 sparkleVel = angle.ToRotationVector2() * 3f + projectile.velocity * 0.3f;
                var sparkle = new SparkleParticle(projectile.Center, sparkleVel, primary * 0.8f, 0.25f, 15);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }

        private void ApplyEnhancedTrailEffect(Projectile projectile, ProjectileVFXData data)
        {
            var palette = MagnumThemePalettes.GetThemePalette(data.DetectedTheme);
            Color primary = palette != null && palette.Length > 0 ? palette[0] : Color.White;
            Color secondary = palette != null && palette.Length > 1 ? palette[1] : primary;
            
            // === NEW: UNIQUE TRAIL STYLES INTEGRATION ===
            // Use the 107 particle PNGs for unique theme+damageClass trail effects
            if (Main.GameUpdateCount % 2 == 0)
            {
                UniqueTrailStyles.SpawnUniqueTrail(
                    projectile.Center, 
                    -projectile.velocity * 0.15f, 
                    data.DetectedTheme, 
                    projectile.DamageType, 
                    palette
                );
            }
            
            // === NEW: BÉZIER FLOWING TRAIL ===
            // Create smooth curved particle flows for projectile trails
            if (projectile.oldPos != null && projectile.oldPos.Length >= 3 && Main.rand.NextBool(4))
            {
                List<Vector2> positions = new List<Vector2>();
                for (int i = 0; i < Math.Min(8, projectile.oldPos.Length); i++)
                {
                    if (projectile.oldPos[i] != Vector2.Zero)
                        positions.Add(projectile.oldPos[i] + projectile.Size * 0.5f);
                }
                if (positions.Count >= 3)
                {
                    Vector2[] flowingTrail = BezierWeaponTrails.GenerateFlowingTrail(positions.ToArray(), 8);
                    BezierWeaponTrails.SpawnParticlesAlongCurve(flowingTrail, palette, data.DetectedTheme, 0.1f);
                }
            }
            
            // Reduced dust trail - 1 every other frame (was 2 per frame)
            if (Main.GameUpdateCount % 3 == 0)
            {
                Vector2 dustOffset = Main.rand.NextVector2Circular(4f, 4f);
                Vector2 dustVel = -projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f);
                
                float colorProgress = (Main.GameUpdateCount * 0.02f) % 1f;
                Color dustColor = VFXUtilities.PaletteLerp(palette, colorProgress);
                
                var glow = new GenericGlowParticle(
                    projectile.Center + dustOffset,
                    dustVel,
                    dustColor * 0.6f, // Reduced opacity
                    0.22f, // Reduced scale
                    18,
                    true
                );
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Contrasting sparkles - reduced to 1 in 6 (was 1 in 2)
            if (Main.rand.NextBool(6))
            {
                var sparkle = new SparkleParticle(
                    projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    -projectile.velocity * 0.06f,
                    Color.White * 0.5f, // Reduced opacity
                    0.16f, // Reduced scale
                    12
                );
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Flares - reduced to 1 in 8 (was 1 in 2)
            if (Main.rand.NextBool(8))
            {
                Vector2 flarePos = projectile.Center + Main.rand.NextVector2Circular(8f, 8f);
                CustomParticles.GenericFlare(flarePos, primary * 0.7f, 0.22f, 10); // Reduced opacity and scale
            }
            
            // Color oscillation - reduced to 1 in 10 (was 1 in 3)
            if (Main.rand.NextBool(10))
            {
                float hue = (Main.GameUpdateCount * 0.015f) % 1f;
                Color shiftedColor = GetThemeHueShiftColor(data.DetectedTheme, hue);
                CustomParticles.GenericFlare(projectile.Center, shiftedColor * 0.6f, 0.18f, 8);
            }
            
            // === FATE-STYLE ELEVATED VFX (Applied to ALL themes) ===
            ApplyElevatedCosmicVFX(projectile, data);
        }
        
        /// <summary>
        /// Applies Fate-style "elevated" VFX to projectiles, adapted to each theme.
        /// Includes: Orbiting glyph sparkles, star trails, constellation connections.
        /// This brings the premium Fate VFX quality to ALL themes.
        /// </summary>
        private void ApplyElevatedCosmicVFX(Projectile projectile, ProjectileVFXData data)
        {
            var palette = MagnumThemePalettes.GetThemePalette(data.DetectedTheme);
            Color primary = palette != null && palette.Length > 0 ? palette[0] : Color.White;
            Color secondary = palette != null && palette.Length > 1 ? palette[1] : primary;
            Color accent = palette != null && palette.Length > 2 ? palette[2] : Color.White;
            
            // === ORBITING GLYPH SPARKLES (like Fate's cosmic orbs) ===
            // Every theme gets subtle orbiting accent particles
            if (Main.GameUpdateCount % 8 == 0)
            {
                float orbitAngle = Main.GameUpdateCount * 0.06f;
                float orbitRadius = 10f + (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 4f;
                
                for (int i = 0; i < 2; i++)
                {
                    float sparkAngle = orbitAngle + MathHelper.Pi * i;
                    Vector2 sparkPos = projectile.Center + sparkAngle.ToRotationVector2() * orbitRadius;
                    
                    // Subtle glyph-like sparkle
                    var sparkle = new SparkleParticle(
                        sparkPos,
                        projectile.velocity * 0.3f + sparkAngle.ToRotationVector2() * 0.5f,
                        accent * 0.7f,
                        0.15f,
                        12
                    );
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }
            
            // === STAR TRAIL SPARKLES (like Fate's twinkling stars) ===
            // Occasional star-like sparkles in the wake
            if (Main.rand.NextBool(12))
            {
                Vector2 starOffset = Main.rand.NextVector2Circular(12f, 12f);
                Vector2 starPos = projectile.Center - projectile.velocity.SafeNormalize(Vector2.Zero) * 8f + starOffset;
                
                // Multi-layer bloom for star effect
                CustomParticles.GenericFlare(starPos, Color.White * 0.6f, 0.2f, 10);
                CustomParticles.GenericFlare(starPos, primary * 0.4f, 0.35f, 12);
            }
            
            // === THEME-SPECIFIC ELEVATED EFFECTS ===
            ApplyThemeSpecificElevatedVFX(projectile, data, primary, secondary, accent);
        }
        
        /// <summary>
        /// Applies theme-specific elevated VFX inspired by Fate's cosmic style.
        /// Each theme gets unique elevated effects matching its identity.
        /// </summary>
        private void ApplyThemeSpecificElevatedVFX(Projectile projectile, ProjectileVFXData data, 
            Color primary, Color secondary, Color accent)
        {
            string theme = data.DetectedTheme.ToLower();
            
            switch (theme)
            {
                case "fate":
                    // Fate: Full cosmic treatment with glyphs and constellations
                    if (Main.rand.NextBool(15))
                    {
                        // Cosmic glyph particles
                        CustomParticles.Glyph(
                            projectile.Center + Main.rand.NextVector2Circular(15f, 15f),
                            secondary * 0.6f, 0.2f, -1
                        );
                    }
                    break;
                    
                case "eroica":
                    // Eroica: Rising ember particles (heroic ascension)
                    if (Main.rand.NextBool(10))
                    {
                        Vector2 emberVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-2f, -1f));
                        var ember = new GenericGlowParticle(
                            projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                            emberVel,
                            Color.Lerp(primary, Color.Orange, Main.rand.NextFloat(0.3f)) * 0.7f,
                            0.18f, 20, true
                        );
                        MagnumParticleHandler.SpawnParticle(ember);
                    }
                    break;
                    
                case "swanlake" or "swan":
                    // Swan Lake: Prismatic shimmer and delicate feather particles
                    if (Main.rand.NextBool(12))
                    {
                        float hue = Main.rand.NextFloat();
                        Color rainbowShimmer = Main.hslToRgb(hue, 0.9f, 0.85f);
                        var shimmer = new SparkleParticle(
                            projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                            Main.rand.NextVector2Circular(1f, 1f),
                            rainbowShimmer * 0.5f,
                            0.2f, 15
                        );
                        MagnumParticleHandler.SpawnParticle(shimmer);
                    }
                    break;
                    
                case "lacampanella" or "campanella":
                    // La Campanella: Smoky infernal wisps
                    if (Main.rand.NextBool(10))
                    {
                        var smoke = new GenericGlowParticle(
                            projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                            -projectile.velocity * 0.08f + new Vector2(0, Main.rand.NextFloat(-0.5f, 0f)),
                            Color.Lerp(Color.Black, primary, 0.3f) * 0.5f,
                            0.25f, 25, true
                        );
                        MagnumParticleHandler.SpawnParticle(smoke);
                    }
                    break;
                    
                case "enigmavariations" or "enigma":
                    // Enigma: Mysterious eye-like particles and void wisps
                    if (Main.rand.NextBool(18))
                    {
                        // Void wisp
                        var voidWisp = new GenericGlowParticle(
                            projectile.Center + Main.rand.NextVector2Circular(12f, 12f),
                            Main.rand.NextVector2Circular(1f, 1f),
                            Color.Lerp(secondary, accent, Main.rand.NextFloat()) * 0.5f,
                            0.22f, 22, true
                        );
                        MagnumParticleHandler.SpawnParticle(voidWisp);
                    }
                    break;
                    
                case "moonlightsonata" or "moonlight":
                    // Moonlight Sonata: Soft lunar mist and silver highlights
                    if (Main.rand.NextBool(10))
                    {
                        var mist = new GenericGlowParticle(
                            projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                            new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-0.3f, 0.3f)),
                            Color.Lerp(primary, Color.Silver, 0.5f) * 0.4f,
                            0.28f, 28, true
                        );
                        MagnumParticleHandler.SpawnParticle(mist);
                    }
                    break;
                    
                default:
                    // Generic: Subtle sparkle accents
                    if (Main.rand.NextBool(15))
                    {
                        var sparkle = new SparkleParticle(
                            projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                            -projectile.velocity * 0.05f,
                            primary * 0.5f,
                            0.15f, 12
                        );
                        MagnumParticleHandler.SpawnParticle(sparkle);
                    }
                    break;
            }
        }

        private void ApplyOrbitingMusicNotes(Projectile projectile, ProjectileVFXData data)
        {
            // Music notes that ORBIT the projectile (reduced: 1 in 20 instead of 1 in 8)
            if (Main.rand.NextBool(20))
            {
                var palette = MagnumThemePalettes.GetThemePalette(data.DetectedTheme);
                Color noteColor = palette != null && palette.Length > 2 ? palette[2] : 
                                  (palette != null && palette.Length > 0 ? palette[0] : Color.White);
                
                float orbitAngle = Main.GameUpdateCount * 0.08f;
                float orbitRadius = 12f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 4f;
                
                // Reduced: 2 notes instead of 3
                for (int i = 0; i < 2; i++)
                {
                    float noteAngle = orbitAngle + MathHelper.TwoPi * i / 2f;
                    Vector2 noteOffset = noteAngle.ToRotationVector2() * orbitRadius;
                    Vector2 notePos = projectile.Center + noteOffset;
                    
                    // Note velocity matches projectile + slight drift
                    Vector2 noteVel = projectile.velocity * 0.5f + noteAngle.ToRotationVector2() * 0.3f;
                    
                    // Reduced scale (0.55f) for less visual clutter
                    ThemedParticles.MusicNote(notePos, noteVel, noteColor * 0.75f, 0.55f, 22);
                }
            }
        }

        private void ApplySmoothLighting(Projectile projectile, ProjectileVFXData data)
        {
            var palette = MagnumThemePalettes.GetThemePalette(data.DetectedTheme);
            Color lightColor = palette != null && palette.Length > 0 ? palette[0] : Color.White;
            
            // Pulsing light
            float pulse = 0.8f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.2f;
            Lighting.AddLight(projectile.Center, lightColor.ToVector3() * pulse);
        }

        private void DrawEnhancedPrimitiveTrail(Projectile projectile, ProjectileVFXData data)
        {
            if (projectile.oldPos == null || projectile.oldPos.Length < 2)
                return;
            
            // Build valid positions
            List<Vector2> positions = new List<Vector2>();
            foreach (var pos in projectile.oldPos)
            {
                if (pos != Vector2.Zero)
                    positions.Add(pos + projectile.Size * 0.5f);
            }
            
            // Add current position
            positions.Add(projectile.Center);
            
            if (positions.Count < 2)
                return;
            
            var palette = MagnumThemePalettes.GetThemePalette(data.DetectedTheme);
            
            // === PRIMARY: Use PrimitiveTrailRenderer (from Common/Systems) for GPU-rendered trails ===
            Color startColor = palette != null && palette.Length > 0 ? palette[0] : Color.White;
            Color endColor = palette != null && palette.Length > 1 ? palette[1] : startColor;
            Common.Systems.PrimitiveTrailRenderer.RenderProjectileTrail(
                projectile,
                startColor,
                endColor,
                22f, // startWidth
                4f   // endWidth
            );
            
            // === SECONDARY: EnhancedTrailRenderer for multi-pass bloom ===
            EnhancedTrailRenderer.RenderMultiPassTrail(
                positions.ToArray(),
                EnhancedTrailRenderer.InverseLerpBumpWidth(6f, 22f),
                EnhancedTrailRenderer.PaletteColor(palette, projectile.Opacity),
                bloomMultiplier: 2.5f,
                coreMultiplier: 0.4f,
                segmentCount: 35
            );
        }

        /// <summary>
        /// Draws subtle bloom layers BEHIND the projectile sprite (PreDraw).
        /// The projectile's own PreDraw handles the actual sprite rendering.
        /// </summary>
        private void DrawPreBloomLayers(Projectile projectile, ProjectileVFXData data, Color lightColor)
        {
            // Get interpolated position for sub-frame smoothness
            Vector2 drawPos = InterpolatedRenderer.GetInterpolatedCenter(projectile) - Main.screenPosition;
            float rotation = InterpolatedRenderer.GetInterpolatedRotation(projectile);
            
            // Get texture for bloom layers
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[projectile.type].Value;
            if (texture == null) return;
            
            Rectangle frame = texture.Frame(1, Main.projFrames[projectile.type], 0, projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;
            
            var palette = MagnumThemePalettes.GetThemePalette(data.DetectedTheme);
            Color primary = palette != null && palette.Length > 0 ? palette[0] : lightColor;
            
            SpriteBatch sb = Main.spriteBatch;
            
            float time = Main.GameUpdateCount * 0.05f;
            float pulse = 1f + (float)Math.Sin(time * 2f) * 0.06f; // Reduced pulse intensity
            
            // === SUBTLE OUTER GLOW (Additive, behind sprite) ===
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                     DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Single subtle outer glow - reduced from multiple layers
            Color outerGlow = primary with { A = 0 } * 0.15f; // Reduced opacity
            sb.Draw(texture, drawPos, frame, outerGlow, rotation, origin, projectile.scale * pulse * 1.35f, SpriteEffects.None, 0f);
            
            // Restore normal blending for projectile's own PreDraw
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, 
                     DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Draws subtle glow accents ON TOP of the projectile sprite (PostDraw).
        /// </summary>
        private void DrawPostGlowLayers(Projectile projectile, ProjectileVFXData data, Color lightColor)
        {
            // Get interpolated position for sub-frame smoothness
            Vector2 drawPos = InterpolatedRenderer.GetInterpolatedCenter(projectile) - Main.screenPosition;
            float rotation = InterpolatedRenderer.GetInterpolatedRotation(projectile);
            
            // Get texture for glow layers
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[projectile.type].Value;
            if (texture == null) return;
            
            Rectangle frame = texture.Frame(1, Main.projFrames[projectile.type], 0, projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;
            
            var palette = MagnumThemePalettes.GetThemePalette(data.DetectedTheme);
            Color secondary = palette != null && palette.Length > 1 ? palette[1] : lightColor;
            
            SpriteBatch sb = Main.spriteBatch;
            
            float time = Main.GameUpdateCount * 0.05f;
            float pulse = 1f + (float)Math.Sin(time * 2f) * 0.06f;
            
            // === SUBTLE INNER GLOW (Additive, on top of sprite) ===
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                     DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Subtle inner glow for ethereal effect
            Color innerGlow = secondary with { A = 0 } * 0.1f; // Very subtle
            sb.Draw(texture, drawPos, frame, innerGlow, rotation, origin, projectile.scale * pulse * 1.08f, SpriteEffects.None, 0f);
            
            // Restore normal blending
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, 
                     DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawInterpolatedProjectile(Projectile projectile, ProjectileVFXData data, Color lightColor)
        {
            // Get interpolated position for sub-frame smoothness
            Vector2 drawPos = InterpolatedRenderer.GetInterpolatedCenter(projectile) - Main.screenPosition;
            float rotation = InterpolatedRenderer.GetInterpolatedRotation(projectile);
            
            // Get texture
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[projectile.type].Value;
            if (texture == null) return;
            
            Rectangle frame = texture.Frame(1, Main.projFrames[projectile.type], 0, projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;
            
            var palette = MagnumThemePalettes.GetThemePalette(data.DetectedTheme);
            Color primary = palette != null && palette.Length > 0 ? palette[0] : lightColor;
            Color secondary = palette != null && palette.Length > 1 ? palette[1] : primary;
            
            SpriteBatch sb = Main.spriteBatch;
            
            // === MULTI-LAYER BLOOM DRAWING ===
            float time = Main.GameUpdateCount * 0.05f;
            float pulse = 1f + (float)Math.Sin(time * 2f) * 0.12f;
            
            // End default blending
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                     DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Layer 1: Outer glow (large, dim)
            Color outerGlow = primary with { A = 0 } * 0.25f;
            sb.Draw(texture, drawPos, frame, outerGlow, rotation, origin, projectile.scale * pulse * 1.6f, SpriteEffects.None, 0f);
            
            // Layer 2: Middle glow (spinning)
            Color midGlow = Color.Lerp(primary, secondary, 0.5f) with { A = 0 } * 0.35f;
            sb.Draw(texture, drawPos, frame, midGlow, rotation + time * 0.3f, origin, projectile.scale * pulse * 1.3f, SpriteEffects.None, 0f);
            
            // Layer 3: Inner glow
            Color innerGlow = secondary with { A = 0 } * 0.5f;
            sb.Draw(texture, drawPos, frame, innerGlow, rotation, origin, projectile.scale * pulse * 1.1f, SpriteEffects.None, 0f);
            
            // Layer 4: Core (bright)
            Color coreGlow = Color.White with { A = 0 } * 0.6f;
            sb.Draw(texture, drawPos, frame, coreGlow, rotation, origin, projectile.scale * 0.5f, SpriteEffects.None, 0f);
            
            // Restore normal blending
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, 
                     DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Draw actual projectile texture on top
            sb.Draw(texture, drawPos, frame, lightColor, rotation, origin, projectile.scale, SpriteEffects.None, 0f);
        }

        #endregion

        #region Theme Detection

        private string DetectThemeFromProjectile(Projectile projectile)
        {
            if (projectile.ModProjectile == null)
                return "generic";
            
            string projectileNamespace = projectile.ModProjectile.GetType().Namespace ?? "";
            string projectileName = projectile.ModProjectile.GetType().Name.ToLower();
            
            // Check namespace for theme
            if (projectileNamespace.Contains("Eroica")) return "Eroica";
            if (projectileNamespace.Contains("SwanLake")) return "SwanLake";
            if (projectileNamespace.Contains("LaCampanella") || projectileNamespace.Contains("Campanella")) return "LaCampanella";
            if (projectileNamespace.Contains("MoonlightSonata") || projectileNamespace.Contains("Moonlight")) return "MoonlightSonata";
            if (projectileNamespace.Contains("EnigmaVariations") || projectileNamespace.Contains("Enigma")) return "EnigmaVariations";
            if (projectileNamespace.Contains("Fate")) return "Fate";
            if (projectileNamespace.Contains("DiesIrae")) return "DiesIrae";
            if (projectileNamespace.Contains("ClairDeLune")) return "ClairDeLune";
            if (projectileNamespace.Contains("Nachtmusik")) return "Nachtmusik";
            if (projectileNamespace.Contains("OdeToJoy")) return "OdeToJoy";
            if (projectileNamespace.Contains("Spring")) return "Spring";
            if (projectileNamespace.Contains("Summer")) return "Summer";
            if (projectileNamespace.Contains("Autumn")) return "Autumn";
            if (projectileNamespace.Contains("Winter")) return "Winter";
            
            // Check projectile name for theme keywords
            if (projectileName.Contains("sakura") || projectileName.Contains("valor") || projectileName.Contains("hero")) return "Eroica";
            if (projectileName.Contains("swan") || projectileName.Contains("feather") || projectileName.Contains("iridescent")) return "SwanLake";
            if (projectileName.Contains("bell") || projectileName.Contains("infernal") || projectileName.Contains("campanella")) return "LaCampanella";
            if (projectileName.Contains("lunar") || projectileName.Contains("moon") || projectileName.Contains("sonata")) return "MoonlightSonata";
            if (projectileName.Contains("enigma") || projectileName.Contains("void") || projectileName.Contains("paradox")) return "EnigmaVariations";
            if (projectileName.Contains("fate") || projectileName.Contains("cosmic") || projectileName.Contains("destiny")) return "Fate";
            
            return "generic";
        }

        private Color GetThemeHueShiftColor(string theme, float progress)
        {
            // Get theme-appropriate hue range
            (float minHue, float maxHue) = theme.ToLower() switch
            {
                "eroica" => (0.95f, 0.12f), // Red to gold
                "swanlake" or "swan" => (0f, 1f), // Full rainbow
                "lacampanella" or "campanella" => (0.05f, 0.15f), // Orange range
                "moonlightsonata" or "moonlight" => (0.65f, 0.8f), // Purple to blue
                "enigmavariations" or "enigma" => (0.7f, 0.35f), // Purple to green
                "fate" => (0.85f, 0.0f), // Pink to red
                _ => (0f, 1f)
            };
            
            float hue = minHue + progress * (maxHue - minHue);
            if (hue < 0) hue += 1f;
            if (hue > 1) hue -= 1f;
            
            return Main.hslToRgb(hue, 0.85f, 0.7f);
        }

        #endregion
    }
}
