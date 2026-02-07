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
    /// CALAMITY VFX INTEGRATION - MASTER CONTROLLER
    /// 
    /// This system integrates all Calamity-style VFX components:
    /// - PrimitiveTrailRenderer (Bézier/Catmull-Rom trails)
    /// - MultiLayerProjectileVFX (5-layer projectile rendering)
    /// - CalamitySkyboxRenderer (boss atmospherics)
    /// - MeleeSwingPrimitives (swing trails)
    /// - Shader-enhanced bloom and screen effects
    /// 
    /// Provides unified API for all content to access VFX features.
    /// </summary>
    public class CalamityVFXIntegration : ModSystem
    {
        // Track active VFX instances per entity
        private static Dictionary<int, ProjectileVFXInstance> _projectileVFX = new();
        private static Dictionary<int, BossVFXInstance> _bossVFX = new();
        private static Dictionary<int, WeaponSwingInstance> _swingVFX = new();
        
        #region VFX Instance Classes
        
        public class ProjectileVFXInstance
        {
            public string Theme;
            public Color[] Palette;
            public float TrailWidth;
            public float BloomIntensity;
            public bool UseShaderTrail;
            public bool UseBezierPath;
            public Vector2[] PositionHistory;
            public int HistoryIndex;
            public float HomingStrength;
            public Vector2 LastPosition;
            public float LastRotation;
        }
        
        public class BossVFXInstance
        {
            public string Theme;
            public bool SkyActive;
            public float SkyIntensity;
            public float FlashTimer;
            public Color[] Palette;
            public List<Vector2> AttackTelegraphs;
        }
        
        public class WeaponSwingInstance
        {
            public string Theme;
            public Color[] Palette;
            public float SwingProgress;
            public Vector2 SwingDirection;
            public float SwingRadius;
            public Vector2[] TrailPositions;
            public int TrailIndex;
        }
        
        #endregion
        
        #region Initialization
        
        public override void Load()
        {
            On_Main.DrawProjectiles += DrawProjectilesHook;
        }
        
        public override void Unload()
        {
            _projectileVFX.Clear();
            _bossVFX.Clear();
            _swingVFX.Clear();
        }
        
        private void DrawProjectilesHook(On_Main.orig_DrawProjectiles orig, Main self)
        {
            // Render all primitive trails before projectiles
            RenderAllPrimitiveTrails();
            
            orig(self);
            
            // Render bloom overlays after projectiles
            RenderAllBloomOverlays();
        }
        
        #endregion
        
        #region Projectile VFX API
        
        /// <summary>
        /// Initialize VFX for a projectile. Call in OnSpawn or first AI frame.
        /// </summary>
        public static void InitializeProjectileVFX(Projectile proj, string theme, float trailWidth = 20f, 
            float bloomIntensity = 1f, bool useShaderTrail = true, bool useBezierPath = false, float homingStrength = 0f)
        {
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            
            _projectileVFX[proj.whoAmI] = new ProjectileVFXInstance
            {
                Theme = theme,
                Palette = palette,
                TrailWidth = trailWidth,
                BloomIntensity = bloomIntensity,
                UseShaderTrail = useShaderTrail,
                UseBezierPath = useBezierPath,
                PositionHistory = new Vector2[25],
                HistoryIndex = 0,
                HomingStrength = homingStrength,
                LastPosition = proj.Center,
                LastRotation = proj.rotation
            };
            
            // Register with PrimitiveTrailRenderer
            PrimitiveTrailRenderer.TrackPosition(proj.whoAmI, proj.Center, proj.rotation, 
                palette[0], palette.Length > 1 ? palette[1] : palette[0], trailWidth);
        }
        
        /// <summary>
        /// Update VFX for a projectile. Call every frame in AI.
        /// </summary>
        public static void UpdateProjectileVFX(Projectile proj, Player owner = null, NPC target = null)
        {
            if (!_projectileVFX.TryGetValue(proj.whoAmI, out var vfx))
                return;
            
            // Update position history
            vfx.PositionHistory[vfx.HistoryIndex] = proj.Center;
            vfx.HistoryIndex = (vfx.HistoryIndex + 1) % vfx.PositionHistory.Length;
            
            // Apply smooth homing if configured
            if (vfx.HomingStrength > 0 && target != null)
            {
                // Create a LayerConfig with just the homing settings
                var homingConfig = new MultiLayerProjectileVFX.LayerConfig
                {
                    HomingStrength = vfx.HomingStrength,
                    MaxHomingAngle = 0.15f // Default max homing angle
                };
                MultiLayerProjectileVFX.ApplySmoothHoming(proj, target.Center, homingConfig);
            }
            
            // Update trail renderer
            PrimitiveTrailRenderer.TrackPosition(proj.whoAmI, proj.Center, proj.rotation,
                vfx.Palette[0], vfx.Palette.Length > 1 ? vfx.Palette[1] : vfx.Palette[0], vfx.TrailWidth);
            
            // Spawn trail particles
            SpawnEnhancedTrailParticles(proj, vfx);
            
            // Apply interpolated lighting
            ApplyProjectileLighting(proj, vfx);
            
            vfx.LastPosition = proj.Center;
            vfx.LastRotation = proj.rotation;
        }
        
        /// <summary>
        /// Render VFX for a projectile. Call in PreDraw, returns false to skip default drawing.
        /// </summary>
        public static bool RenderProjectileVFX(Projectile proj, SpriteBatch spriteBatch, Color lightColor)
        {
            if (!_projectileVFX.TryGetValue(proj.whoAmI, out var vfx))
                return true;
            
            // Render multi-layer bloom
            RenderProjectileBloom(proj, spriteBatch, vfx, lightColor);
            
            // Render primitive trail
            if (vfx.UseShaderTrail)
            {
                PrimitiveTrailRenderer.RenderTrailCustom(
                    proj.whoAmI,
                    spriteBatch,
                    progress => vfx.TrailWidth * PrimitiveTrailRenderer.QuadraticBump(1f - progress),
                    progress => Color.Lerp(vfx.Palette[0], vfx.Palette[vfx.Palette.Length - 1], progress) * (1f - progress * 0.5f)
                );
            }
            
            return true; // Let default drawing happen too
        }
        
        /// <summary>
        /// Clean up VFX for a projectile. Call in OnKill.
        /// </summary>
        public static void CleanupProjectileVFX(Projectile proj)
        {
            if (!_projectileVFX.TryGetValue(proj.whoAmI, out var vfx))
                return;
            
            // Spawn death effect
            CalamityStyleVFX.SpectacularDeath(proj.Center, vfx.Theme, vfx.BloomIntensity);
            
            // Clean up trail
            PrimitiveTrailRenderer.ClearTrail(proj.whoAmI);
            
            _projectileVFX.Remove(proj.whoAmI);
        }
        
        #endregion
        
        #region Boss VFX API
        
        /// <summary>
        /// Initialize VFX for a boss. Call in OnSpawn.
        /// </summary>
        public static void InitializeBossVFX(NPC boss, string theme)
        {
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            
            _bossVFX[boss.whoAmI] = new BossVFXInstance
            {
                Theme = theme,
                SkyActive = false,
                SkyIntensity = 0f,
                FlashTimer = 0f,
                Palette = palette,
                AttackTelegraphs = new List<Vector2>()
            };
        }
        
        /// <summary>
        /// Activate boss sky effect with smooth transition.
        /// </summary>
        public static void ActivateBossSky(NPC boss)
        {
            if (!_bossVFX.TryGetValue(boss.whoAmI, out var vfx))
                return;
            
            vfx.SkyActive = true;
            
            // Use CalamitySkyboxRenderer presets
            switch (vfx.Theme.ToLower())
            {
                case "eroica":
                    CalamitySkyboxRenderer.Presets.Eroica();
                    break;
                case "lacampanella":
                case "campanella":
                    CalamitySkyboxRenderer.Presets.LaCampanella();
                    break;
                case "swanlake":
                case "swan":
                    CalamitySkyboxRenderer.Presets.SwanLake();
                    break;
                case "moonlightsonata":
                case "moonlight":
                    CalamitySkyboxRenderer.Presets.MoonlightSonata();
                    break;
                case "enigmavariations":
                case "enigma":
                    CalamitySkyboxRenderer.Presets.EnigmaVariations();
                    break;
                case "fate":
                    CalamitySkyboxRenderer.Presets.Fate();
                    break;
                default:
                    // Generic boss sky with neutral colors
                    CalamitySkyboxRenderer.ActivateBossSky("Generic", new Color(80, 80, 100), Color.White, 0.5f, 0.02f);
                    break;
            }
        }
        
        /// <summary>
        /// Trigger screen flash for boss attacks.
        /// </summary>
        public static void TriggerBossFlash(NPC boss, float intensity = 0.5f)
        {
            if (!_bossVFX.TryGetValue(boss.whoAmI, out var vfx))
                return;
            
            vfx.FlashTimer = intensity;
            Color flashColor = vfx.Palette != null && vfx.Palette.Length > 0 ? vfx.Palette[0] : Color.White;
            CalamitySkyboxRenderer.TriggerFlash(flashColor, intensity);
        }
        
        /// <summary>
        /// Add attack telegraph for boss attack preview.
        /// </summary>
        public static void AddAttackTelegraph(NPC boss, Vector2 position)
        {
            if (!_bossVFX.TryGetValue(boss.whoAmI, out var vfx))
                return;
            
            vfx.AttackTelegraphs.Add(position);
            
            // Spawn warning particle
            CustomParticles.GenericFlare(position, vfx.Palette[0] * 0.5f, 0.4f, 30);
        }
        
        /// <summary>
        /// Update boss VFX. Call every frame in AI.
        /// </summary>
        public static void UpdateBossVFX(NPC boss)
        {
            if (!_bossVFX.TryGetValue(boss.whoAmI, out var vfx))
                return;
            
            // Update sky intensity
            if (vfx.SkyActive)
            {
                vfx.SkyIntensity = Math.Min(1f, vfx.SkyIntensity + 0.02f);
            }
            
            // Update flash timer
            if (vfx.FlashTimer > 0)
            {
                vfx.FlashTimer -= 0.05f;
            }
            
            // Clear old telegraphs
            vfx.AttackTelegraphs.RemoveAll(t => !IsValidTelegraph(t, boss));
            
            // Spawn ambient particles
            SpawnBossAmbientParticles(boss, vfx);
        }
        
        /// <summary>
        /// Spawn boss phase transition VFX.
        /// </summary>
        public static void BossPhaseTransition(NPC boss, float intensity = 1.5f)
        {
            if (!_bossVFX.TryGetValue(boss.whoAmI, out var vfx))
                return;
            
            CalamityStyleVFX.BossPhaseTransition(boss, vfx.Theme, intensity);
            TriggerBossFlash(boss, 0.8f);
        }
        
        /// <summary>
        /// Spawn boss death VFX.
        /// </summary>
        public static void BossDeath(NPC boss)
        {
            if (!_bossVFX.TryGetValue(boss.whoAmI, out var vfx))
                return;
            
            // Massive death explosion
            CalamityStyleVFX.SpectacularDeath(boss.Center, vfx.Theme, 3f);
            
            // Deactivate sky
            CalamitySkyboxRenderer.DeactivateBossSky();
            
            _bossVFX.Remove(boss.whoAmI);
        }
        
        #endregion
        
        #region Weapon Swing VFX API
        
        /// <summary>
        /// Initialize swing VFX for a melee weapon.
        /// </summary>
        public static void InitializeSwingVFX(Player player, string theme, float swingRadius = 60f)
        {
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            
            _swingVFX[player.whoAmI] = new WeaponSwingInstance
            {
                Theme = theme,
                Palette = palette,
                SwingProgress = 0f,
                SwingDirection = Vector2.UnitX * player.direction,
                SwingRadius = swingRadius,
                TrailPositions = new Vector2[15],
                TrailIndex = 0
            };
        }
        
        /// <summary>
        /// Update swing VFX. Call during UseAnimation.
        /// </summary>
        public static void UpdateSwingVFX(Player player, float progress, Vector2 direction)
        {
            if (!_swingVFX.TryGetValue(player.whoAmI, out var vfx))
                return;
            
            vfx.SwingProgress = progress;
            vfx.SwingDirection = direction;
            
            // Calculate blade tip position
            float swingAngle = (progress - 0.5f) * MathHelper.Pi * 1.2f;
            Vector2 tipPos = player.Center + direction.RotatedBy(swingAngle) * vfx.SwingRadius;
            
            // Track position
            vfx.TrailPositions[vfx.TrailIndex] = tipPos;
            vfx.TrailIndex = (vfx.TrailIndex + 1) % vfx.TrailPositions.Length;
            
            // Use CalamityStyleVFX for smooth swing effect
            CalamityStyleVFX.SmoothMeleeSwing(player, vfx.Theme, progress, direction, vfx.SwingRadius);
            
            // Spawn trail particles
            SpawnSwingTrailParticles(player, vfx, tipPos);
        }
        
        /// <summary>
        /// End swing VFX.
        /// </summary>
        public static void EndSwingVFX(Player player)
        {
            _swingVFX.Remove(player.whoAmI);
        }
        
        #endregion
        
        #region Private Helper Methods
        
        private static void SpawnEnhancedTrailParticles(Projectile proj, ProjectileVFXInstance vfx)
        {
            // Dense dust trail (every other frame)
            if (Main.GameUpdateCount % 2 == 0)
            {
                Vector2 dustPos = proj.Center + Main.rand.NextVector2Circular(4f, 4f);
                Vector2 dustVel = -proj.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f);
                
                float colorProgress = (Main.GameUpdateCount * 0.02f) % 1f;
                Color dustColor = VFXUtilities.PaletteLerp(vfx.Palette, colorProgress);
                
                var glow = new GenericGlowParticle(dustPos, dustVel, dustColor * 0.7f, 0.25f, 18, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Contrasting sparkle (1 in 4)
            if (Main.rand.NextBool(4))
            {
                var sparkle = new SparkleParticle(
                    proj.Center + Main.rand.NextVector2Circular(6f, 6f),
                    -proj.velocity * 0.05f,
                    Color.White * 0.6f,
                    0.2f,
                    15
                );
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Orbiting music notes (1 in 15)
            if (Main.rand.NextBool(15))
            {
                float orbitAngle = Main.GameUpdateCount * 0.08f;
                for (int i = 0; i < 2; i++)
                {
                    float noteAngle = orbitAngle + MathHelper.TwoPi * i / 2f;
                    Vector2 notePos = proj.Center + noteAngle.ToRotationVector2() * 12f;
                    Vector2 noteVel = proj.velocity * 0.4f;
                    Color noteColor = vfx.Palette.Length > 2 ? vfx.Palette[2] : vfx.Palette[0];
                    ThemedParticles.MusicNote(notePos, noteVel, noteColor * 0.8f, 0.6f, 25);
                }
            }
        }
        
        private static void ApplyProjectileLighting(Projectile proj, ProjectileVFXInstance vfx)
        {
            float pulse = 0.8f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.2f;
            Lighting.AddLight(proj.Center, vfx.Palette[0].ToVector3() * pulse * vfx.BloomIntensity);
        }
        
        private static void RenderProjectileBloom(Projectile proj, SpriteBatch spriteBatch, ProjectileVFXInstance vfx, Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[proj.type].Value;
            if (texture == null) return;
            
            Vector2 drawPos = InterpolatedRenderer.GetInterpolatedCenter(proj) - Main.screenPosition;
            float rotation = InterpolatedRenderer.GetInterpolatedRotation(proj);
            Rectangle frame = texture.Frame(1, Main.projFrames[proj.type], 0, proj.frame);
            Vector2 origin = frame.Size() * 0.5f;
            
            float time = Main.GameUpdateCount * 0.05f;
            float pulse = 1f + (float)Math.Sin(time * 2f) * 0.1f;
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Multi-layer bloom
            for (int i = 0; i < 4; i++)
            {
                float scale = (1.6f - i * 0.3f) * pulse;
                float opacity = (0.2f + i * 0.1f) * vfx.BloomIntensity;
                Color bloomColor = Color.Lerp(vfx.Palette[0], vfx.Palette[vfx.Palette.Length - 1], i / 4f) with { A = 0 } * opacity;
                spriteBatch.Draw(texture, drawPos, frame, bloomColor, rotation + (i % 2 == 0 ? time * 0.2f : 0), origin, proj.scale * scale, SpriteEffects.None, 0f);
            }
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        
        private static void SpawnBossAmbientParticles(NPC boss, BossVFXInstance vfx)
        {
            if (Main.rand.NextBool(8))
            {
                Vector2 particlePos = boss.Center + Main.rand.NextVector2Circular(boss.width * 0.7f, boss.height * 0.7f);
                Vector2 particleVel = Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0, -1f);
                
                Color particleColor = VFXUtilities.PaletteLerp(vfx.Palette, Main.rand.NextFloat());
                
                var glow = new GenericGlowParticle(particlePos, particleVel, particleColor * 0.5f, 0.3f, 30, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Orbiting glyphs/symbols (1 in 20)
            if (Main.rand.NextBool(20))
            {
                float orbitAngle = Main.GameUpdateCount * 0.03f;
                float orbitRadius = boss.width * 0.8f;
                Vector2 glyphPos = boss.Center + orbitAngle.ToRotationVector2() * orbitRadius;
                CustomParticles.Glyph(glyphPos, vfx.Palette[0] * 0.6f, 0.4f, -1);
            }
        }
        
        private static void SpawnSwingTrailParticles(Player player, WeaponSwingInstance vfx, Vector2 tipPos)
        {
            // Dense trail dust
            if (Main.rand.NextBool(2))
            {
                Vector2 dustVel = vfx.SwingDirection.RotatedBy(MathHelper.PiOver2 * player.direction) * 4f;
                Dust dust = Dust.NewDustPerfect(tipPos, Terraria.ID.DustID.MagicMirror, dustVel, 0, vfx.Palette[0], 1.3f);
                dust.noGravity = true;
                dust.fadeIn = 1.2f;
            }
            
            // Sparkle accents
            if (Main.rand.NextBool(3))
            {
                var sparkle = new SparkleParticle(tipPos, Main.rand.NextVector2Circular(2f, 2f), Color.White * 0.7f, 0.25f, 15);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Music notes (1 in 8)
            if (Main.rand.NextBool(8))
            {
                Vector2 noteVel = vfx.SwingDirection.RotatedByRandom(0.5f) * 2f;
                ThemedParticles.MusicNote(tipPos, noteVel, vfx.Palette[0] * 0.9f, 0.7f, 25);
            }
        }
        
        private static bool IsValidTelegraph(Vector2 pos, NPC boss)
        {
            return Vector2.Distance(pos, boss.Center) < 2000f;
        }
        
        private static void RenderAllPrimitiveTrails()
        {
            // Called before projectiles are drawn
            // The PrimitiveTrailRenderer handles its own rendering in individual projectile PreDraw calls
        }
        
        private static void RenderAllBloomOverlays()
        {
            // Called after projectiles are drawn
            // Additional global overlays can be rendered here
        }
        
        #endregion
        
        #region Theme Palette Shortcuts
        
        /// <summary>
        /// Get the primary color for a theme.
        /// </summary>
        public static Color GetThemePrimary(string theme)
        {
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            return palette != null && palette.Length > 0 ? palette[0] : Color.White;
        }
        
        /// <summary>
        /// Get the secondary color for a theme.
        /// </summary>
        public static Color GetThemeSecondary(string theme)
        {
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            return palette != null && palette.Length > 1 ? palette[1] : GetThemePrimary(theme);
        }
        
        /// <summary>
        /// Get a gradient color from the theme palette.
        /// </summary>
        public static Color GetThemeGradient(string theme, float progress)
        {
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            return VFXUtilities.PaletteLerp(palette, progress);
        }
        
        #endregion
    }
}
