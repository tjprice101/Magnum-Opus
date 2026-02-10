using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// Base projectile class providing buttery smooth Calamity-style rendering.
    /// Inherit from this for automatic Bézier trails, bloom, screen effects, and unique visual identity.
    /// </summary>
    public abstract class EnhancedProjectileBase : ModProjectile
    {
        #region Abstract Properties - Override these for unique identity
        
        /// <summary>
        /// The theme this projectile belongs to. Used for color palette and particle selection.
        /// </summary>
        public abstract string ProjectileTheme { get; }
        
        /// <summary>
        /// Primary color for this projectile. Overrides theme default if set.
        /// </summary>
        public virtual Color? PrimaryColor => null;
        
        /// <summary>
        /// Secondary color for gradient effects. Overrides theme default if set.
        /// </summary>
        public virtual Color? SecondaryColor => null;
        
        /// <summary>
        /// Accent color for highlights. Overrides theme default if set.
        /// </summary>
        public virtual Color? AccentColor => null;
        
        /// <summary>
        /// Base bloom intensity (0-1). Higher = more glow.
        /// </summary>
        public virtual float BloomIntensity => 0.8f;
        
        /// <summary>
        /// Number of bloom layers (1-5). More layers = softer glow.
        /// </summary>
        public virtual int BloomLayers => 4;
        
        /// <summary>
        /// Trail length in frames (how many positions to remember).
        /// </summary>
        public virtual int TrailLength => 16;
        
        /// <summary>
        /// Whether to spawn orbiting particles around the projectile.
        /// </summary>
        public virtual bool HasOrbitingParticles => true;
        
        /// <summary>
        /// Number of orbiting elements.
        /// </summary>
        public virtual int OrbitCount => 3;
        
        /// <summary>
        /// Whether to spawn music notes in the trail.
        /// </summary>
        public virtual bool HasMusicNotes => true;
        
        /// <summary>
        /// Music note spawn chance (1 in X).
        /// </summary>
        public virtual int MusicNoteChance => 6;
        
        /// <summary>
        /// Whether to trigger screen effects on impact.
        /// </summary>
        public virtual bool HasScreenEffects => true;
        
        /// <summary>
        /// Screen distortion intensity on impact (0-1).
        /// </summary>
        public virtual float ScreenEffectIntensity => 0.3f;
        
        /// <summary>
        /// Whether to use Bézier curve interpolation for smooth trails.
        /// </summary>
        public virtual bool UseBezierTrails => true;
        
        /// <summary>
        /// Particle density multiplier (1 = normal, 2 = double particles).
        /// </summary>
        public virtual float ParticleDensity => 1f;
        
        /// <summary>
        /// Pulse speed for the glow effect.
        /// </summary>
        public virtual float PulseSpeed => 0.1f;
        
        /// <summary>
        /// Orbit rotation speed.
        /// </summary>
        public virtual float OrbitSpeed => 0.08f;
        
        /// <summary>
        /// Orbit radius around projectile center.
        /// </summary>
        public virtual float OrbitRadius => 15f;
        
        #endregion
        
        #region Protected State
        
        protected float PulsePhase;
        protected float OrbitAngle;
        protected List<Vector2> PositionHistory = new List<Vector2>();
        protected List<float> RotationHistory = new List<float>();
        protected Color[] ThemePalette;
        protected UniqueTrailStyles.TrailConfig TrailConfig;
        
        #endregion
        
        #region Lifecycle
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TrailLength;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void AI()
        {
            // Initialize on first frame
            if (PositionHistory.Count == 0)
            {
                ThemePalette = MagnumThemePalettes.GetThemePalette(ProjectileTheme);
                TrailConfig = UniqueTrailStyles.GetTrailConfig(ProjectileTheme, Projectile.DamageType);
            }
            
            // Update state
            PulsePhase += PulseSpeed;
            OrbitAngle += OrbitSpeed;
            
            // Record position for Bézier trail
            RecordPosition();
            
            // Register with advanced renderer for interpolation
            AdvancedProjectileRenderer.RegisterProjectile(Projectile);
            
            // Spawn visual effects
            SpawnTrailEffects();
            SpawnOrbitingParticles();
            SpawnMusicNotes();
            
            // Add dynamic lighting
            ApplyDynamicLighting();
            
            // Call derived AI
            EnhancedAI();
        }
        
        /// <summary>
        /// Override this instead of AI() for your custom projectile logic.
        /// </summary>
        protected virtual void EnhancedAI() { }
        
        public override void OnKill(int timeLeft)
        {
            // Spawn impact effects
            SpawnImpactEffects();
            
            // Trigger screen effects
            if (HasScreenEffects)
            {
                TriggerScreenEffects();
            }
            
            // Clean up
            AdvancedProjectileRenderer.UnregisterProjectile(Projectile.whoAmI);
            BezierWeaponTrails.ClearProjectileTrail(Projectile.whoAmI);
            
            // Call derived kill
            EnhancedOnKill(timeLeft);
        }
        
        /// <summary>
        /// Override this instead of OnKill() for your custom death logic.
        /// </summary>
        protected virtual void EnhancedOnKill(int timeLeft) { }
        
        #endregion
        
        #region Trail System
        
        private void RecordPosition()
        {
            PositionHistory.Insert(0, Projectile.Center);
            RotationHistory.Insert(0, Projectile.rotation);
            
            // Limit history length
            while (PositionHistory.Count > TrailLength)
            {
                PositionHistory.RemoveAt(PositionHistory.Count - 1);
                RotationHistory.RemoveAt(RotationHistory.Count - 1);
            }
        }
        
        private void SpawnTrailEffects()
        {
            int frameInterval = Math.Max(1, (int)(3 / ParticleDensity));
            if (Main.GameUpdateCount % frameInterval != 0) return;
            
            Color primaryColor = PrimaryColor ?? VFXUtilities.PaletteLerp(ThemePalette, 0.2f);
            Color secondaryColor = SecondaryColor ?? VFXUtilities.PaletteLerp(ThemePalette, 0.6f);
            
            // Use UniqueTrailStyles for themed particles
            UniqueTrailStyles.SpawnUniqueTrail(Projectile.Center, -Projectile.velocity * 0.1f, ProjectileTheme, Projectile.DamageType, ThemePalette);
            
            // Dense dust trail
            for (int i = 0; i < (int)(2 * ParticleDensity); i++)
            {
                Vector2 dustOffset = Main.rand.NextVector2Circular(6f, 6f);
                Vector2 dustVel = -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                
                float colorProgress = (Main.GameUpdateCount * 0.02f + i * 0.1f) % 1f;
                Color dustColor = Color.Lerp(primaryColor, secondaryColor, colorProgress);
                
                var glow = new GenericGlowParticle(
                    Projectile.Center + dustOffset,
                    dustVel,
                    dustColor * 0.7f,
                    0.25f + Main.rand.NextFloat(0.1f),
                    18,
                    true
                );
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Sparkle accents
            if (Main.rand.NextBool(2))
            {
                Vector2 sparklePos = Projectile.Center + Main.rand.NextVector2Circular(10f, 10f);
                var sparkle = new SparkleParticle(sparklePos, Main.rand.NextVector2Circular(2f, 2f), Color.White * 0.7f, 0.3f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Bézier curve spawning
            if (UseBezierTrails && PositionHistory.Count >= 4 && Main.rand.NextBool(3))
            {
                Vector2[] trail = BezierWeaponTrails.GenerateFlowingTrail(PositionHistory.ToArray(), 6);
                BezierWeaponTrails.SpawnParticlesAlongCurve(trail, ThemePalette, ProjectileTheme, 0.08f);
            }
        }
        
        private void SpawnOrbitingParticles()
        {
            if (!HasOrbitingParticles) return;
            if (Main.GameUpdateCount % 4 != 0) return;
            
            Color primaryColor = PrimaryColor ?? VFXUtilities.PaletteLerp(ThemePalette, 0.3f);
            
            for (int i = 0; i < OrbitCount; i++)
            {
                float angle = OrbitAngle + MathHelper.TwoPi * i / OrbitCount;
                float wobble = (float)Math.Sin(PulsePhase * 2f + i * 0.5f) * 3f;
                Vector2 orbitPos = Projectile.Center + angle.ToRotationVector2() * (OrbitRadius + wobble);
                
                // Orbiting flare
                float hue = (angle / MathHelper.TwoPi + Main.GameUpdateCount * 0.01f) % 1f;
                Color orbitColor = Main.hslToRgb(hue, 0.8f, 0.6f);
                if (ProjectileTheme != "SwanLake") // Swan Lake gets rainbow, others get theme
                {
                    orbitColor = Color.Lerp(primaryColor, VFXUtilities.PaletteLerp(ThemePalette, hue), 0.5f);
                }
                
                CustomParticles.GenericFlare(orbitPos, orbitColor, 0.15f, 8);
            }
        }
        
        private void SpawnMusicNotes()
        {
            if (!HasMusicNotes) return;
            if (!Main.rand.NextBool(MusicNoteChance)) return;
            
            Color noteColor = PrimaryColor ?? VFXUtilities.PaletteLerp(ThemePalette, Main.rand.NextFloat());
            Vector2 noteVel = -Projectile.velocity * 0.05f + Main.rand.NextVector2Circular(1f, 1f);
            noteVel.Y -= 0.5f; // Float upward
            
            // VISIBLE scale (0.7f+)
            float noteScale = Main.rand.NextFloat(0.7f, 0.95f);
            
            ThemedParticles.MusicNote(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), noteVel, noteColor, noteScale, 35);
            
            // Sparkle companion for visibility
            var sparkle = new SparkleParticle(Projectile.Center, noteVel * 0.5f, Color.White * 0.5f, 0.2f, 20);
            MagnumParticleHandler.SpawnParticle(sparkle);
        }
        
        #endregion
        
        #region Impact Effects
        
        private void SpawnImpactEffects()
        {
            Color primaryColor = PrimaryColor ?? VFXUtilities.PaletteLerp(ThemePalette, 0.2f);
            Color secondaryColor = SecondaryColor ?? VFXUtilities.PaletteLerp(ThemePalette, 0.6f);
            Color accentColor = AccentColor ?? VFXUtilities.PaletteLerp(ThemePalette, 0.8f);
            
            // === CENTRAL GLIMMER CASCADE ===
            
            // White flash core
            CustomParticles.GenericFlare(Projectile.Center, Color.White, 1.0f * BloomIntensity, 22);
            
            // Layered theme flares (spinning)
            for (int layer = 0; layer < BloomLayers; layer++)
            {
                float progress = (float)layer / BloomLayers;
                Color layerColor = Color.Lerp(primaryColor, secondaryColor, progress);
                float layerScale = (0.8f - layer * 0.15f) * BloomIntensity;
                int layerLife = 20 - layer * 2;
                CustomParticles.GenericFlare(Projectile.Center, layerColor, layerScale, layerLife);
            }
            
            // === EXPANDING HALO RINGS ===
            for (int ring = 0; ring < 4; ring++)
            {
                float progress = (float)ring / 4f;
                Color ringColor = Color.Lerp(primaryColor, accentColor, progress);
                float ringScale = 0.3f + ring * 0.15f;
                int ringLife = 14 + ring * 3;
                CustomParticles.HaloRing(Projectile.Center, ringColor, ringScale, ringLife);
            }
            
            // === RADIAL SPARKLE BURST ===
            int sparkleCount = (int)(12 * ParticleDensity);
            for (int i = 0; i < sparkleCount; i++)
            {
                float angle = MathHelper.TwoPi * i / sparkleCount;
                Vector2 sparkleVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Color sparkleColor = Color.Lerp(primaryColor, Color.White, (float)i / sparkleCount * 0.5f);
                
                var sparkle = new SparkleParticle(Projectile.Center, sparkleVel, sparkleColor, 0.4f, 25);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // === THEMED PARTICLE BURST ===
            UniqueTrailStyles.SpawnUniqueImpact(Projectile.Center, ProjectileTheme, Projectile.DamageType, ThemePalette, 1.2f);
            
            // === MUSIC NOTE FINALE ===
            if (HasMusicNotes)
            {
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f;
                    Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                    Color noteColor = Color.Lerp(primaryColor, secondaryColor, (float)i / 6f);
                    ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.8f, 30);
                }
            }
            
            // === DUST EXPLOSION ===
            for (int i = 0; i < (int)(15 * ParticleDensity); i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(6f, 6f);
                Color dustColor = Color.Lerp(primaryColor, secondaryColor, Main.rand.NextFloat());
                
                var glow = new GenericGlowParticle(Projectile.Center, dustVel, dustColor * 0.8f, 0.35f, 22, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // === BRIGHT LIGHTING PULSE ===
            Lighting.AddLight(Projectile.Center, primaryColor.ToVector3() * 1.5f);
        }
        
        private void TriggerScreenEffects()
        {
            if (ScreenEffectIntensity <= 0f) return;
            
            // Screen shake for significant impacts
            if (ScreenEffectIntensity > 0.2f)
            {
                Player localPlayer = Main.LocalPlayer;
                float distance = Vector2.Distance(localPlayer.Center, Projectile.Center);
                if (distance < 800f)
                {
                    float falloff = 1f - (distance / 800f);
                    float shakeIntensity = ScreenEffectIntensity * falloff * 4f;
                    
                    // Use screen effect manager
                    MagnumScreenEffects.AddScreenShake(shakeIntensity);
                }
            }
            
            // Theme-specific screen effects
            ScreenDistortionManager.TriggerThemeEffect(ProjectileTheme, Projectile.Center, ScreenEffectIntensity);
        }
        
        #endregion
        
        #region Rendering
        
        private void ApplyDynamicLighting()
        {
            Color lightColor = PrimaryColor ?? VFXUtilities.PaletteLerp(ThemePalette, 0.3f);
            float pulse = 0.8f + (float)Math.Sin(PulsePhase) * 0.2f;
            Lighting.AddLight(Projectile.Center, lightColor.ToVector3() * pulse * BloomIntensity);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            // Let derived class do custom pre-draw first
            bool continueDefault = EnhancedPreDraw(ref lightColor);
            if (!continueDefault) return false;
            
            // Draw multi-layer bloom behind the projectile
            DrawBloomLayers();
            
            // Draw Bézier trail
            if (UseBezierTrails && PositionHistory.Count >= 3)
            {
                DrawBezierTrail();
            }
            else
            {
                DrawSimpleTrail();
            }
            
            // Draw orbiting elements
            if (HasOrbitingParticles)
            {
                DrawOrbitingElements();
            }
            
            return true; // Draw default sprite on top
        }
        
        /// <summary>
        /// Override this for custom pre-draw effects. Return false to skip default rendering.
        /// </summary>
        protected virtual bool EnhancedPreDraw(ref Color lightColor) => true;
        
        private void DrawBloomLayers()
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D softGlow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            Texture2D flare = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 glowOrigin = softGlow.Size() * 0.5f;
            Vector2 flareOrigin = flare.Size() * 0.5f;
            
            float pulse = 1f + (float)Math.Sin(PulsePhase) * 0.15f;
            float baseScale = Projectile.scale * 0.5f;
            
            Color primaryColor = PrimaryColor ?? VFXUtilities.PaletteLerp(ThemePalette, 0.2f);
            Color secondaryColor = SecondaryColor ?? VFXUtilities.PaletteLerp(ThemePalette, 0.6f);
            
            // Switch to additive blending
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Outer ethereal glow
            Color outerGlow = primaryColor.WithoutAlpha() * 0.25f * BloomIntensity;
            sb.Draw(softGlow, drawPos, null, outerGlow, 0f, glowOrigin, baseScale * pulse * 2.0f, SpriteEffects.None, 0f);
            
            // Middle layer
            Color middleGlow = secondaryColor.WithoutAlpha() * 0.4f * BloomIntensity;
            sb.Draw(softGlow, drawPos, null, middleGlow, 0f, glowOrigin, baseScale * pulse * 1.4f, SpriteEffects.None, 0f);
            
            // Core flare (rotating)
            float rotation = Main.GameUpdateCount * 0.05f;
            Color coreGlow = Color.Lerp(primaryColor, Color.White, 0.3f).WithoutAlpha() * 0.6f * BloomIntensity;
            sb.Draw(flare, drawPos, null, coreGlow, rotation, flareOrigin, baseScale * pulse * 0.8f, SpriteEffects.None, 0f);
            
            // White-hot center
            Color whiteCore = Color.White.WithoutAlpha() * 0.5f * BloomIntensity;
            sb.Draw(flare, drawPos, null, whiteCore, -rotation * 0.7f, flareOrigin, baseScale * pulse * 0.4f, SpriteEffects.None, 0f);
            
            // Restore normal blending
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        
        private void DrawBezierTrail()
        {
            if (PositionHistory.Count < 3) return;
            
            Vector2[] smoothTrail = BezierWeaponTrails.GenerateFlowingTrail(PositionHistory.ToArray(), 4);
            if (smoothTrail.Length == 0) return;
            
            SpriteBatch sb = Main.spriteBatch;
            Texture2D softGlow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            Vector2 origin = softGlow.Size() * 0.5f;
            
            Color primaryColor = PrimaryColor ?? VFXUtilities.PaletteLerp(ThemePalette, 0.2f);
            Color secondaryColor = SecondaryColor ?? VFXUtilities.PaletteLerp(ThemePalette, 0.6f);
            
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            for (int i = 0; i < smoothTrail.Length; i++)
            {
                float progress = (float)i / smoothTrail.Length;
                float opacity = (1f - progress) * 0.6f * BloomIntensity;
                float scale = (1f - progress * 0.6f) * 0.35f * Projectile.scale;
                
                Color trailColor = Color.Lerp(primaryColor, secondaryColor, progress);
                trailColor = trailColor.WithoutAlpha() * opacity;
                
                Vector2 drawPos = smoothTrail[i] - Main.screenPosition;
                sb.Draw(softGlow, drawPos, null, trailColor, 0f, origin, scale, SpriteEffects.None, 0f);
            }
            
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        
        private void DrawSimpleTrail()
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = tex.Size() * 0.5f;
            
            Color primaryColor = PrimaryColor ?? VFXUtilities.PaletteLerp(ThemePalette, 0.2f);
            Color secondaryColor = SecondaryColor ?? VFXUtilities.PaletteLerp(ThemePalette, 0.6f);
            
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                
                float progress = (float)i / Projectile.oldPos.Length;
                float opacity = (1f - progress) * 0.5f * BloomIntensity;
                float scale = (1f - progress * 0.5f) * Projectile.scale;
                
                Color trailColor = Color.Lerp(primaryColor, secondaryColor, progress);
                trailColor = trailColor.WithoutAlpha() * opacity;
                
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                float rot = Projectile.oldRot.Length > i ? Projectile.oldRot[i] : Projectile.rotation;
                
                sb.Draw(tex, drawPos, null, trailColor, rot, origin, scale, SpriteEffects.None, 0f);
            }
            
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        
        private void DrawOrbitingElements()
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D flare = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            Vector2 origin = flare.Size() * 0.5f;
            
            Color primaryColor = PrimaryColor ?? VFXUtilities.PaletteLerp(ThemePalette, 0.3f);
            
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            for (int i = 0; i < OrbitCount; i++)
            {
                float angle = OrbitAngle + MathHelper.TwoPi * i / OrbitCount;
                float wobble = (float)Math.Sin(PulsePhase * 2f + i * 0.5f) * 3f;
                Vector2 orbitPos = Projectile.Center + angle.ToRotationVector2() * (OrbitRadius + wobble) - Main.screenPosition;
                
                float hue = (angle / MathHelper.TwoPi + Main.GameUpdateCount * 0.01f) % 1f;
                Color orbitColor;
                if (ProjectileTheme == "SwanLake")
                {
                    orbitColor = Main.hslToRgb(hue, 0.9f, 0.7f);
                }
                else
                {
                    orbitColor = Color.Lerp(primaryColor, VFXUtilities.PaletteLerp(ThemePalette, hue), 0.5f);
                }
                
                orbitColor = orbitColor.WithoutAlpha() * 0.7f * BloomIntensity;
                sb.Draw(flare, orbitPos, null, orbitColor, angle, origin, 0.12f, SpriteEffects.None, 0f);
            }
            
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        
        #endregion
    }
}
