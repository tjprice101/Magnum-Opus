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
    /// GLOBAL BOSS VFX OVERHAUL
    /// 
    /// Automatically applies Calamity-style spectacular VFX to ALL MagnumOpus bosses.
    /// 
    /// Features:
    /// - Interpolated rendering for 144Hz+ smoothness
    /// - Screen distortion effects
    /// - Spectacular phase transitions
    /// - Attack windup/release VFX
    /// - Ambient aura systems
    /// - Primitive trail dashes
    /// - Death spectacles
    /// </summary>
    public class GlobalBossVFXOverhaul : GlobalNPC
    {
        // Per-NPC VFX tracking
        private static Dictionary<int, BossVFXState> _bossStates = new Dictionary<int, BossVFXState>();
        
        private class BossVFXState
        {
            public string Theme = "generic";
            public int DashTrailId = -1;
            public bool IsDashing = false;
            public float AttackWindupProgress = 0f;
            public bool IsWindingUp = false;
            public int LastPhase = 0;
            public Vector2 LastPosition;
            public float LastRotation;
            public int AmbientTimer = 0;
        }

        public override bool InstancePerEntity => false;

        public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        {
            // Apply to MagnumOpus bosses
            return entity.ModNPC?.Mod == ModContent.GetInstance<MagnumOpus>() && entity.boss;
        }

        public override void OnSpawn(NPC npc, Terraria.DataStructures.IEntitySource source)
        {
            if (!npc.boss || npc.ModNPC?.Mod != ModContent.GetInstance<MagnumOpus>())
                return;
            
            // Initialize boss VFX state
            var state = new BossVFXState
            {
                Theme = DetectThemeFromNPC(npc),
                LastPosition = npc.Center,
                LastRotation = npc.rotation
            };
            
            _bossStates[npc.whoAmI] = state;
            
            // Dramatic spawn effect
            SpawnBossEntrance(npc, state.Theme);
        }

        public override void AI(NPC npc)
        {
            if (!_bossStates.TryGetValue(npc.whoAmI, out var state))
                return;
            
            // Apply ambient VFX
            state.AmbientTimer++;
            ApplyAmbientBossVFX(npc, state);
            
            // Track movement for dash detection
            Vector2 velocity = npc.Center - state.LastPosition;
            float speed = velocity.Length();
            
            // Auto-detect dashing (fast movement)
            if (speed > 25f && !state.IsDashing)
            {
                StartBossDash(npc, state);
            }
            else if (speed < 15f && state.IsDashing)
            {
                EndBossDash(npc, state);
            }
            
            // Update dash trail
            if (state.IsDashing && state.DashTrailId >= 0)
            {
                AdvancedTrailSystem.UpdateTrail(state.DashTrailId, npc.Center, npc.rotation);
                ApplyDashParticles(npc, state);
            }
            
            // Dynamic lighting
            ApplyBossLighting(npc, state);
            
            // Update last position
            state.LastPosition = npc.Center;
            state.LastRotation = npc.rotation;
        }

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (!_bossStates.TryGetValue(npc.whoAmI, out var state))
                return true;
            
            // Draw PRE-bloom layers (behind the boss sprite)
            // The boss's own PreDraw will handle the actual sprite rendering (especially for grid sprite sheets)
            DrawPreBloomLayers(npc, state, spriteBatch, screenPos, drawColor);
            
            // === NEW: SHADER-ENHANCED BLOOM ===
            // Apply shader-based multi-pass bloom if shader available
            Texture2D bossTexture = Terraria.GameContent.TextureAssets.Npc[npc.type].Value;
            if (bossTexture != null)
            {
                Vector2 drawPos = InterpolatedRenderer.GetInterpolatedCenter(npc) - screenPos;
                float intensity = state.IsDashing ? 1.4f : 0.8f;
                
                // Draw shader-enhanced bloom behind sprite
                BossShaderEffects.DrawBossWithShaderBloom(
                    spriteBatch, 
                    bossTexture, 
                    drawPos, 
                    npc.frame, 
                    state.Theme, 
                    npc.rotation, 
                    npc.scale, 
                    intensity);
            }
            
            return true; // Let the boss's own PreDraw handle sprite rendering (required for 6x6 grid sheets!)
        }

        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (!_bossStates.TryGetValue(npc.whoAmI, out var state))
                return;
            
            // Draw POST-glow layers (on top of the boss sprite)
            DrawPostGlowLayers(npc, state, spriteBatch, screenPos, drawColor);
            
            // === NEW: THEME-SPECIFIC EFFECTS ===
            BossShaderEffects.ApplyThemeSpecificEffect(npc, state.Theme, spriteBatch);
            
            // === NEW: BOSS AURA (when attacking) ===
            if (state.AttackWindupProgress > 0)
            {
                BossShaderEffects.DrawBossAura(spriteBatch, npc, state.Theme, 
                    radius: 60f + state.AttackWindupProgress * 40f, 
                    intensity: state.AttackWindupProgress);
            }
        }

        public override void OnKill(NPC npc)
        {
            if (!_bossStates.TryGetValue(npc.whoAmI, out var state))
                return;
            
            // End any active trails
            if (state.DashTrailId >= 0)
            {
                AdvancedTrailSystem.EndTrail(state.DashTrailId);
            }
            
            // SPECTACULAR DEATH
            SpawnBossDeathSpectacle(npc, state);
            
            // Cleanup
            _bossStates.Remove(npc.whoAmI);
        }

        #region Boss Entrance

        private void SpawnBossEntrance(NPC npc, string theme)
        {
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            Color primary = palette != null && palette.Length > 0 ? palette[0] : Color.White;
            Color secondary = palette != null && palette.Length > 1 ? palette[1] : primary;
            
            Vector2 bossCenter = npc.Center;
            
            // === ACTIVATE CALAMITY-STYLE SKYBOX ===
            ActivateThemedSkybox(theme);
            
            // === SCREEN DISTORTION ===
            ScreenDistortionManager.TriggerThemeEffect(theme, bossCenter, 0.8f, 45);
            
            // === SKY FLASH ===
            CalamitySkyboxRenderer.TriggerFlash(primary, 1.2f);
            DynamicSkyboxSystem.TriggerFlash(primary, 1.2f);
            
            // === MASSIVE CENTRAL NOVA ===
            CustomParticles.GenericFlare(bossCenter, Color.White, 2.5f, 35);
            CustomParticles.GenericFlare(bossCenter, primary, 2f, 32);
            CustomParticles.GenericFlare(bossCenter, secondary, 1.5f, 28);
            
            // === CASCADING HALO RINGS ===
            for (int phase = 0; phase < 4; phase++)
            {
                for (int i = 0; i < 5; i++)
                {
                    float ringScale = 0.4f + i * 0.25f + phase * 0.5f;
                    Color ringColor = VFXUtilities.PaletteLerp(palette, (phase * 5 + i) / 20f);
                    int ringLife = 20 + phase * 10 + i * 4;
                    CustomParticles.HaloRing(bossCenter, ringColor * (1f - phase * 0.2f), ringScale, ringLife);
                }
            }
            
            // === MASSIVE PARTICLE EXPLOSION ===
            for (int i = 0; i < 50; i++)
            {
                float angle = MathHelper.TwoPi * i / 50f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(12f, 25f);
                Color particleColor = VFXUtilities.PaletteLerp(palette, Main.rand.NextFloat());
                
                var glow = new GenericGlowParticle(bossCenter, vel, particleColor, 0.7f, 40, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // === SPARKLE BURST ===
            for (int i = 0; i < 30; i++)
            {
                float angle = MathHelper.TwoPi * i / 30f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(8f, 15f);
                var sparkle = new SparkleParticle(bossCenter, vel, Color.White * 0.9f, 0.5f, 35);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // === MUSIC NOTE CASCADE ===
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 10f);
                ThemedParticles.MusicNote(bossCenter, noteVel, primary, 1f, 45);
            }
            
            // === EXTENDED VFX: God rays, dimensional tears, kinetic shockwave for entrance ===
            GodRaySystem.CreateBurst(bossCenter, primary, 8, 150f, 60);
            KineticRippleSystem.CreateShockwave(bossCenter, 1.2f);
            
            // Theme-specific dimensional tear for dramatic entrances
            if (theme == "Fate" || theme == "EnigmaVariations")
            {
                Vector2 tearEnd = bossCenter + Main.rand.NextVector2Unit() * 100f;
                // CreateTear(startPos, endPos, width, style, lifetime)
                DimensionalTearSystem.CreateTear(bossCenter, tearEnd, 60f, DimensionalTearSystem.TearStyle.Cosmic, 60);
            }
        }

        #endregion

        #region Ambient VFX

        private void ApplyAmbientBossVFX(NPC npc, BossVFXState state)
        {
            var palette = MagnumThemePalettes.GetThemePalette(state.Theme);
            Color primary = palette != null && palette.Length > 0 ? palette[0] : Color.White;
            Color secondary = palette != null && palette.Length > 1 ? palette[1] : primary;
            
            Vector2 bossCenter = npc.Center;
            
            // === NEW: UNIQUE THEME PARTICLES FROM 107 PNGs ===
            // Spawn theme-specific ambient particles
            if (state.AmbientTimer % 20 == 0)
            {
                Vector2 ambientPos = bossCenter + Main.rand.NextVector2Circular(npc.width * 0.7f, npc.height * 0.7f);
                Vector2 ambientVel = Main.rand.NextVector2Circular(1f, 1f) + new Vector2(0, -0.3f);
                UniqueTrailStyles.SpawnUniqueTrail(ambientPos, ambientVel, state.Theme, DamageClass.Generic, palette);
            }
            
            // === ORBITING PARTICLES === (REDUCED: every 12 frames instead of 3)
            if (state.AmbientTimer % 12 == 0)
            {
                float orbitAngle = Main.GameUpdateCount * 0.04f;
                float orbitRadius = npc.width * 0.8f + (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 15f;
                
                // REDUCED: 3 particles instead of 5
                for (int i = 0; i < 3; i++)
                {
                    float angle = orbitAngle + MathHelper.TwoPi * i / 3f;
                    Vector2 orbitPos = bossCenter + angle.ToRotationVector2() * orbitRadius;
                    
                    float colorProgress = (i / 3f + Main.GameUpdateCount * 0.008f) % 1f;
                    Color orbitColor = VFXUtilities.PaletteLerp(palette, colorProgress);
                    
                    // REDUCED: Lower opacity and scale
                    CustomParticles.GenericFlare(orbitPos, orbitColor * 0.4f, 0.18f, 12);
                }
            }
            
            // === ETHEREAL AURA === (REDUCED: every 15 frames instead of 5)
            if (state.AmbientTimer % 15 == 0)
            {
                Vector2 auraPos = bossCenter + Main.rand.NextVector2Circular(npc.width * 0.6f, npc.height * 0.6f);
                var glow = new GenericGlowParticle(auraPos, Main.rand.NextVector2Circular(0.5f, 0.5f), primary * 0.35f, 0.18f, 18, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // === MUSIC NOTE ACCENTS === (REDUCED: every 30 frames instead of 15, 2 notes instead of 3)
            if (state.AmbientTimer % 30 == 0)
            {
                float noteAngle = Main.GameUpdateCount * 0.06f;
                for (int i = 0; i < 2; i++)
                {
                    float angle = noteAngle + MathHelper.TwoPi * i / 2f;
                    Vector2 notePos = bossCenter + angle.ToRotationVector2() * (npc.width * 0.5f + 20f);
                    Vector2 noteVel = new Vector2(0, -0.6f) + angle.ToRotationVector2() * 0.3f;
                    
                    ThemedParticles.MusicNote(notePos, noteVel, secondary * 0.8f, 0.55f, 25);
                }
            }
            
            // === CONTRASTING SPARKLES === (REDUCED: 1 in 12 instead of 1 in 4)
            if (Main.rand.NextBool(12))
            {
                Vector2 sparklePos = bossCenter + Main.rand.NextVector2Circular(npc.width * 0.5f, npc.height * 0.5f);
                var sparkle = new SparkleParticle(sparklePos, Main.rand.NextVector2Circular(0.5f, 0.5f), Color.White * 0.4f, 0.15f, 12);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // === EXTENDED VFX: Constellation aura, dimensional tears, god rays ===
            AdvancedVFXExtensions.ApplyExtendedBossAmbientVFX(npc, state.Theme);
        }

        #endregion

        #region Dash VFX

        private void StartBossDash(NPC npc, BossVFXState state)
        {
            state.IsDashing = true;
            
            var palette = MagnumThemePalettes.GetThemePalette(state.Theme);
            Color primary = palette != null && palette.Length > 0 ? palette[0] : Color.White;
            
            // Create dash trail
            state.DashTrailId = AdvancedTrailSystem.CreateThemeTrail(state.Theme, 40f, maxPoints: 20, intensity: 1.5f);
            
            // Departure burst
            CustomParticles.GenericFlare(npc.Center, Color.White, 1f, 18);
            CustomParticles.GenericFlare(npc.Center, primary, 0.8f, 15);
            CustomParticles.HaloRing(npc.Center, primary, 0.5f, 15);
            
            // Screen distortion
            ScreenDistortionManager.TriggerThemeEffect(state.Theme, npc.Center, 0.3f, 12);
        }

        private void ApplyDashParticles(NPC npc, BossVFXState state)
        {
            var palette = MagnumThemePalettes.GetThemePalette(state.Theme);
            Color primary = palette != null && palette.Length > 0 ? palette[0] : Color.White;
            Color secondary = palette != null && palette.Length > 1 ? palette[1] : primary;
            
            // === NEW: UNIQUE TRAIL STYLES FOR BOSS DASHES ===
            // Use theme-specific particles from our 107 particle PNGs
            UniqueTrailStyles.SpawnUniqueTrail(npc.Center, -npc.velocity * 0.2f, state.Theme, DamageClass.Generic, palette);
            
            // === NEW: BÉZIER CURVE DASH TRAIL ===
            // Create smooth flowing particle stream along dash path
            if (Main.rand.NextBool(2))
            {
                Vector2[] dashCurve = new Vector2[] { state.LastPosition, npc.Center };
                if (dashCurve.Length >= 2)
                {
                    // Interpolate for smoother curve
                    Vector2 mid = Vector2.Lerp(state.LastPosition, npc.Center, 0.5f);
                    mid += npc.velocity.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(-15f, 15f);
                    Vector2[] smoothCurve = new Vector2[] { state.LastPosition, mid, npc.Center };
                    Vector2[] flowingTrail = BezierWeaponTrails.GenerateFlowingTrail(smoothCurve, 6);
                    BezierWeaponTrails.SpawnParticlesAlongCurve(flowingTrail, palette, state.Theme, 0.08f);
                }
            }
            
            // Dense trail particles
            for (int i = 0; i < 3; i++)
            {
                Vector2 dustPos = npc.Center + Main.rand.NextVector2Circular(npc.width * 0.3f, npc.height * 0.3f);
                Vector2 dustVel = -npc.velocity.SafeNormalize(Vector2.Zero) * 3f + Main.rand.NextVector2Circular(2f, 2f);
                
                float colorProgress = Main.rand.NextFloat();
                Color dustColor = VFXUtilities.PaletteLerp(palette, colorProgress);
                
                var glow = new GenericGlowParticle(dustPos, dustVel, dustColor * 0.85f, 0.4f, 20, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Trailing flares
            if (Main.rand.NextBool(2))
            {
                Vector2 flarePos = npc.Center + Main.rand.NextVector2Circular(npc.width * 0.4f, npc.height * 0.4f);
                CustomParticles.GenericFlare(flarePos, primary, 0.4f, 10);
            }
        }

        private static void EndBossDash(NPC npc, BossVFXState state)
        {
            state.IsDashing = false;
            
            // End trail
            if (state.DashTrailId >= 0)
            {
                AdvancedTrailSystem.EndTrail(state.DashTrailId);
                state.DashTrailId = -1;
            }
            
            // Impact effect
            CalamityStyleVFX.GlimmerCascadeImpact(npc.Center, state.Theme, 1f);
            
            // === EXTENDED VFX: Kinetic ripple and dimensional tear on dash end ===
            AdvancedVFXExtensions.ApplyExtendedBossDashVFX(npc, npc.velocity, state.Theme);
        }

        #endregion

        #region Boss Lighting

        private void ApplyBossLighting(NPC npc, BossVFXState state)
        {
            var palette = MagnumThemePalettes.GetThemePalette(state.Theme);
            Color lightColor = palette != null && palette.Length > 0 ? palette[0] : Color.White;
            
            // Pulsing boss glow
            float pulse = 0.6f + (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.15f;
            float intensity = state.IsDashing ? 1.2f : 0.8f;
            
            Lighting.AddLight(npc.Center, lightColor.ToVector3() * pulse * intensity);
        }

        #endregion

        #region Boss Death Spectacle

        private void SpawnBossDeathSpectacle(NPC npc, BossVFXState state)
        {
            var palette = MagnumThemePalettes.GetThemePalette(state.Theme);
            Color primary = palette != null && palette.Length > 0 ? palette[0] : Color.White;
            Color secondary = palette != null && palette.Length > 1 ? palette[1] : primary;
            Color tertiary = palette != null && palette.Length > 2 ? palette[2] : secondary;
            
            Vector2 bossCenter = npc.Center;
            
            // === DEACTIVATE CALAMITY-STYLE SKYBOX ===
            DeactivateThemedSkybox();
            
            // === MASSIVE SCREEN DISTORTION ===
            ScreenDistortionManager.TriggerThemeEffect(state.Theme, bossCenter, 1.2f, 60);
            
            // === PROLONGED SKY FLASH ===
            CalamitySkyboxRenderer.TriggerFlash(primary, 2.5f);
            DynamicSkyboxSystem.TriggerFlash(primary, 2f);
            
            // === QUADRUPLE NOVA FLASH ===
            CustomParticles.GenericFlare(bossCenter, Color.White, 4f, 50);
            CustomParticles.GenericFlare(bossCenter, primary, 3.2f, 45);
            CustomParticles.GenericFlare(bossCenter, secondary, 2.5f, 40);
            CustomParticles.GenericFlare(bossCenter, tertiary, 1.8f, 35);
            
            // === MASSIVE CASCADING SHOCKWAVE (20 rings!) ===
            for (int phase = 0; phase < 5; phase++)
            {
                for (int i = 0; i < 4; i++)
                {
                    float ringScale = 0.5f + i * 0.4f + phase * 0.8f;
                    Color ringColor = VFXUtilities.PaletteLerp(palette, (phase * 4 + i) / 20f);
                    int ringLife = 30 + phase * 12 + i * 5;
                    CustomParticles.HaloRing(bossCenter, ringColor * (1f - phase * 0.15f), ringScale, ringLife);
                }
            }
            
            // === MASSIVE PARTICLE EXPLOSION (100 particles!) ===
            for (int i = 0; i < 100; i++)
            {
                float angle = MathHelper.TwoPi * i / 100f + Main.rand.NextFloat(-0.05f, 0.05f);
                float speed = Main.rand.NextFloat(15f, 35f);
                Vector2 vel = angle.ToRotationVector2() * speed;
                Color particleColor = VFXUtilities.PaletteLerp(palette, Main.rand.NextFloat());
                
                var glow = new GenericGlowParticle(bossCenter, vel, particleColor * 0.95f, Main.rand.NextFloat(0.5f, 0.9f), 50, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // === SPARKLE SUPERNOVA ===
            for (int i = 0; i < 60; i++)
            {
                float angle = MathHelper.TwoPi * i / 60f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(12f, 22f);
                var sparkle = new SparkleParticle(bossCenter, vel, Color.White, 0.6f, 45);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // === MUSIC NOTE GRAND FINALE ===
            for (int i = 0; i < 24; i++)
            {
                float angle = MathHelper.TwoPi * i / 24f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(8f, 14f);
                Color noteColor = i % 3 == 0 ? primary : (i % 3 == 1 ? secondary : tertiary);
                ThemedParticles.MusicNote(bossCenter, noteVel, noteColor, 1.2f, 55);
            }
            
            // === SPIRAL GALAXY EFFECT ===
            for (int arm = 0; arm < 8; arm++)
            {
                float armAngle = MathHelper.TwoPi * arm / 8f;
                for (int point = 0; point < 10; point++)
                {
                    float spiralAngle = armAngle + point * 0.35f;
                    float spiralRadius = 25f + point * 20f;
                    Vector2 spiralPos = bossCenter + spiralAngle.ToRotationVector2() * spiralRadius;
                    
                    Color galaxyColor = VFXUtilities.PaletteLerp(palette, (arm * 10 + point) / 80f);
                    CustomParticles.GenericFlare(spiralPos, galaxyColor, 0.4f + point * 0.04f, 30 + point * 3);
                }
            }
            
            // === EXTENDED VFX: Massive fractal shatter, god rays, dimensional tears ===
            AdvancedVFXExtensions.ApplyExtendedBossDeathVFX(npc, state.Theme, 2f);
        }

        #endregion

        #region Interpolated Drawing

        /// <summary>
        /// Draws subtle bloom layers BEHIND the boss sprite (PreDraw).
        /// The boss's own PreDraw handles the actual sprite rendering.
        /// </summary>
        private void DrawPreBloomLayers(NPC npc, BossVFXState state, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            // Get interpolated position for sub-frame smoothness
            Vector2 drawPos = InterpolatedRenderer.GetInterpolatedCenter(npc) - screenPos;
            
            // Get texture for bloom layers
            Texture2D texture = Terraria.GameContent.TextureAssets.Npc[npc.type].Value;
            if (texture == null) return;
            
            // Use npc.frame - this is just for bloom silhouettes, not precise rendering
            Rectangle frame = npc.frame;
            Vector2 origin = frame.Size() * 0.5f;
            
            var palette = MagnumThemePalettes.GetThemePalette(state.Theme);
            Color primary = palette != null && palette.Length > 0 ? palette[0] : drawColor;
            
            float time = Main.GameUpdateCount * 0.04f;
            float pulse = 1f + (float)Math.Sin(time * 2f) * 0.04f; // Reduced pulse intensity
            
            // === SUBTLE OUTER GLOW (Additive, behind sprite) ===
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                             DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Single subtle outer glow - reduced from multiple layers to avoid overwhelming
            Color outerGlow = primary with { A = 0 } * 0.12f; // Reduced opacity
            spriteBatch.Draw(texture, drawPos, frame, outerGlow, npc.rotation, origin, npc.scale * pulse * 1.25f, 
                            npc.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            
            // Restore normal blending for boss's own PreDraw
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                             DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Draws subtle glow accents ON TOP of the boss sprite (PostDraw).
        /// </summary>
        private void DrawPostGlowLayers(NPC npc, BossVFXState state, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            // Get interpolated position for sub-frame smoothness
            Vector2 drawPos = InterpolatedRenderer.GetInterpolatedCenter(npc) - screenPos;
            
            // Get texture for glow layers
            Texture2D texture = Terraria.GameContent.TextureAssets.Npc[npc.type].Value;
            if (texture == null) return;
            
            Rectangle frame = npc.frame;
            Vector2 origin = frame.Size() * 0.5f;
            
            var palette = MagnumThemePalettes.GetThemePalette(state.Theme);
            Color secondary = palette != null && palette.Length > 1 ? palette[1] : drawColor;
            
            float time = Main.GameUpdateCount * 0.04f;
            float pulse = 1f + (float)Math.Sin(time * 2f) * 0.04f;
            
            // === SUBTLE INNER GLOW (Additive, on top of sprite) ===
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                             DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Subtle inner glow for ethereal effect
            Color innerGlow = secondary with { A = 0 } * 0.08f; // Very subtle
            spriteBatch.Draw(texture, drawPos, frame, innerGlow, npc.rotation, origin, npc.scale * pulse * 1.05f, 
                            npc.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            
            // Restore normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                             DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// DEPRECATED: Full boss drawing - now handled by boss's own PreDraw for proper grid sprite sheet support.
        /// Kept for reference.
        /// </summary>
        [Obsolete("Use DrawPreBloomLayers + boss's own PreDraw + DrawPostGlowLayers instead")]
        private void DrawInterpolatedBoss(NPC npc, BossVFXState state, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            // Get interpolated position for sub-frame smoothness
            Vector2 drawPos = InterpolatedRenderer.GetInterpolatedCenter(npc) - screenPos;
            float rotation = InterpolatedRenderer.GetInterpolatedRotation(npc);
            
            // Get texture
            Texture2D texture = Terraria.GameContent.TextureAssets.Npc[npc.type].Value;
            if (texture == null) return;
            
            Rectangle frame = npc.frame;
            Vector2 origin = frame.Size() * 0.5f;
            
            var palette = MagnumThemePalettes.GetThemePalette(state.Theme);
            Color primary = palette != null && palette.Length > 0 ? palette[0] : drawColor;
            Color secondary = palette != null && palette.Length > 1 ? palette[1] : primary;
            
            float time = Main.GameUpdateCount * 0.04f;
            float pulse = 1f + (float)Math.Sin(time * 2f) * 0.08f;
            
            // === BLOOM LAYERS (Additive) ===
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                             DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Outer glow
            Color outerGlow = primary with { A = 0 } * 0.2f;
            spriteBatch.Draw(texture, drawPos, frame, outerGlow, rotation, origin, npc.scale * pulse * 1.4f, SpriteEffects.None, 0f);
            
            // Middle glow (spinning slightly)
            Color midGlow = Color.Lerp(primary, secondary, 0.5f) with { A = 0 } * 0.25f;
            spriteBatch.Draw(texture, drawPos, frame, midGlow, rotation + time * 0.15f, origin, npc.scale * pulse * 1.2f, SpriteEffects.None, 0f);
            
            // Inner glow
            Color innerGlow = secondary with { A = 0 } * 0.35f;
            spriteBatch.Draw(texture, drawPos, frame, innerGlow, rotation, origin, npc.scale * pulse * 1.08f, SpriteEffects.None, 0f);
            
            // Restore normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                             DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Draw actual boss sprite
            spriteBatch.Draw(texture, drawPos, frame, drawColor, rotation, origin, npc.scale, 
                            npc.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
        }

        #endregion

        #region Theme Detection

        private string DetectThemeFromNPC(NPC npc)
        {
            if (npc.ModNPC == null)
                return "generic";
            
            string npcNamespace = npc.ModNPC.GetType().Namespace ?? "";
            string npcName = npc.ModNPC.GetType().Name.ToLower();
            
            // Check namespace
            if (npcNamespace.Contains("Eroica")) return "Eroica";
            if (npcNamespace.Contains("SwanLake")) return "SwanLake";
            if (npcNamespace.Contains("LaCampanella")) return "LaCampanella";
            if (npcNamespace.Contains("MoonlightSonata") || npcNamespace.Contains("Moonlight")) return "MoonlightSonata";
            if (npcNamespace.Contains("EnigmaVariations") || npcNamespace.Contains("Enigma")) return "EnigmaVariations";
            if (npcNamespace.Contains("Fate")) return "Fate";
            if (npcNamespace.Contains("DiesIrae")) return "DiesIrae";
            if (npcNamespace.Contains("ClairDeLune")) return "ClairDeLune";
            if (npcNamespace.Contains("Nachtmusik")) return "Nachtmusik";
            if (npcNamespace.Contains("OdeToJoy")) return "OdeToJoy";
            
            // Check NPC name
            if (npcName.Contains("eroica") || npcName.Contains("valor")) return "Eroica";
            if (npcName.Contains("swan")) return "SwanLake";
            if (npcName.Contains("campanella")) return "LaCampanella";
            if (npcName.Contains("enigma")) return "EnigmaVariations";
            if (npcName.Contains("fate")) return "Fate";
            
            return "generic";
        }

        /// <summary>
        /// Activates the Calamity-style skybox with the appropriate theme preset.
        /// </summary>
        private void ActivateThemedSkybox(string theme)
        {
            switch (theme)
            {
                case "Eroica":
                    CalamitySkyboxRenderer.Presets.Eroica();
                    break;
                case "LaCampanella":
                    CalamitySkyboxRenderer.Presets.LaCampanella();
                    break;
                case "SwanLake":
                    CalamitySkyboxRenderer.Presets.SwanLake();
                    break;
                case "MoonlightSonata":
                    CalamitySkyboxRenderer.Presets.MoonlightSonata();
                    break;
                case "EnigmaVariations":
                    CalamitySkyboxRenderer.Presets.EnigmaVariations();
                    break;
                case "Fate":
                    CalamitySkyboxRenderer.Presets.Fate();
                    break;
                case "DiesIrae":
                    // Dies Irae - blood red theme with dark atmosphere
                    CalamitySkyboxRenderer.ActivateBossSky(
                        "DiesIrae",
                        new Color(120, 20, 20), // Crimson primary
                        new Color(180, 30, 30), // Blood red secondary
                        0.65f, 0.03f);
                    break;
                case "ClairDeLune":
                    // Clair de Lune - soft moonlit blue
                    CalamitySkyboxRenderer.ActivateBossSky(
                        "ClairDeLune",
                        new Color(60, 80, 120),  // Soft blue primary
                        new Color(180, 200, 240), // Moonlight secondary
                        0.4f, 0.015f);
                    break;
                case "Nachtmusik":
                    // Nachtmusik - elegant night purple
                    CalamitySkyboxRenderer.ActivateBossSky(
                        "Nachtmusik",
                        new Color(80, 50, 100),  // Purple primary
                        new Color(150, 120, 180), // Soft lavender secondary
                        0.5f, 0.02f);
                    break;
                case "OdeToJoy":
                    // Ode to Joy - triumphant gold and white
                    CalamitySkyboxRenderer.ActivateBossSky(
                        "OdeToJoy",
                        new Color(180, 150, 80), // Golden primary
                        new Color(255, 230, 150), // Bright gold secondary
                        0.55f, 0.025f);
                    break;
                default:
                    // Generic theme - neutral atmospheric effect
                    CalamitySkyboxRenderer.ActivateBossSky(
                        theme,
                        new Color(80, 80, 100),
                        Color.White,
                        0.5f, 0.02f);
                    break;
            }
        }

        /// <summary>
        /// Deactivates the Calamity-style skybox when boss dies.
        /// </summary>
        private void DeactivateThemedSkybox()
        {
            CalamitySkyboxRenderer.DeactivateBossSky();
        }

        #endregion

        #region Public API for Boss Manual VFX Calls

        /// <summary>
        /// Triggers attack windup VFX for a boss. Call this during attack charge-up.
        /// </summary>
        public static void TriggerAttackWindup(NPC boss, float progress)
        {
            if (!_bossStates.TryGetValue(boss.whoAmI, out var state))
                return;
            
            state.IsWindingUp = true;
            state.AttackWindupProgress = progress;
            
            CalamityStyleVFX.BossAttackWindup(boss, state.Theme, progress);
            
            // === EXTENDED VFX: Kinetic pulse and god ray convergence ===
            AdvancedVFXExtensions.ApplyExtendedBossWindupVFX(boss, progress, state.Theme);
        }

        /// <summary>
        /// Triggers attack release VFX for a boss. Call this when attack fires.
        /// </summary>
        public static void TriggerAttackRelease(NPC boss, float scale = 1f)
        {
            if (!_bossStates.TryGetValue(boss.whoAmI, out var state))
                return;
            
            state.IsWindingUp = false;
            state.AttackWindupProgress = 0f;
            
            CalamityStyleVFX.BossAttackRelease(boss, state.Theme, scale);
            
            // === EXTENDED VFX: Massive shockwave, god rays, dimensional tears ===
            AdvancedVFXExtensions.ApplyExtendedBossReleaseVFX(boss, state.Theme, scale);
        }

        /// <summary>
        /// Triggers phase transition VFX. Call this when boss enters a new phase.
        /// </summary>
        public static void TriggerPhaseTransition(NPC boss, float scale = 1.5f)
        {
            if (!_bossStates.TryGetValue(boss.whoAmI, out var state))
                return;
            
            CalamityStyleVFX.BossPhaseTransition(boss, state.Theme, scale);
            
            // === EXTENDED VFX: Full phase transition spectacle ===
            AdvancedVFXExtensions.ApplyExtendedBossPhaseTransitionVFX(boss, state.Theme, scale);
        }

        /// <summary>
        /// Manually starts a dash trail for boss.
        /// </summary>
        public static int StartDash(NPC boss, float width = 40f)
        {
            if (!_bossStates.TryGetValue(boss.whoAmI, out var state))
                return -1;
            
            StartBossDash_Internal(boss, state);
            return state.DashTrailId;
        }

        /// <summary>
        /// Updates dash trail position. Call every frame during dash.
        /// </summary>
        public static void UpdateDash(NPC boss)
        {
            if (!_bossStates.TryGetValue(boss.whoAmI, out var state) || !state.IsDashing)
                return;
            
            if (state.DashTrailId >= 0)
            {
                AdvancedTrailSystem.UpdateTrail(state.DashTrailId, boss.Center, boss.rotation);
            }
        }

        /// <summary>
        /// Ends dash trail with impact VFX.
        /// </summary>
        public static void EndDash(NPC boss)
        {
            if (!_bossStates.TryGetValue(boss.whoAmI, out var state) || !state.IsDashing)
                return;
            
            EndBossDash(boss, state);
        }

        private static void StartBossDash_Internal(NPC npc, BossVFXState state)
        {
            state.IsDashing = true;
            
            var palette = MagnumThemePalettes.GetThemePalette(state.Theme);
            Color primary = palette != null && palette.Length > 0 ? palette[0] : Color.White;
            
            state.DashTrailId = AdvancedTrailSystem.CreateThemeTrail(state.Theme, 40f, maxPoints: 20, intensity: 1.5f);
            
            CustomParticles.GenericFlare(npc.Center, Color.White, 1f, 18);
            CustomParticles.GenericFlare(npc.Center, primary, 0.8f, 15);
            CustomParticles.HaloRing(npc.Center, primary, 0.5f, 15);
            
            ScreenDistortionManager.TriggerThemeEffect(state.Theme, npc.Center, 0.3f, 12);
        }

        #endregion
    }
}
