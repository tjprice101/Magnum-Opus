using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// GLOBAL NPC VFX HOOKS - ENHANCED WITH INTERPOLATION, TRAILS, AND SHADERS
    /// 
    /// Automatically applies unique VFX to ALL MagnumOpus bosses and enemies:
    /// 
    /// 1. SPAWN EFFECTS - Dramatic entrance with theme fog and light beams
    /// 2. AMBIENT AURAS - Persistent visual presence while alive
    /// 3. ATTACK TELEGRAPHS - Warning visuals before attacks  
    /// 4. HIT EFFECTS - Light beam bursts when hit
    /// 5. DEATH SPECTACLES - Dramatic multi-phase death animations
    /// 6. INTERPOLATED RENDERING - 144Hz+ smooth rendering via InterpolatedRenderer
    /// 7. PRIMITIVE TRAILS - Shader-enhanced movement trails
    /// 8. FOG INTEGRATION - Theme-specific atmospheric effects
    /// 
    /// Integrates with: BossEnemyVFXIntegration, WeaponFogAndBeamVFX, 
    ///                  UniqueWeaponVFXStyles, CalamityStyleVFX,
    ///                  InterpolatedRenderer, EnhancedTrailRenderer, AdvancedTrailSystem
    /// </summary>
    public class GlobalNPCVFXHooks : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        
        // Track NPC state
        private bool _hasSpawned = false;
        private bool _hasAura = false;
        private string _detectedTheme = "";
        private bool _isBoss = false;
        private int _lastAttackState = -1;
        private Vector2 _previousPosition;
        private Vector2 _oldPreviousPosition; // For interpolation
        private float _interpolationTimer;
        
        // Trail system
        private int _trailId = -1;
        private bool _hasTrail = false;
        private Vector2[] _positionHistory = new Vector2[25];
        private float[] _rotationHistory = new float[25];
        private int _historyIndex = 0;
        private bool _isMovingFast = false;
        
        // Attack detection
        private float _lastAI0 = 0f;
        private float _lastAI1 = 0f;
        private int _attackCooldown = 0;
        
        // Visual state
        private float _damageFlashTimer = 0f;
        private int _lastHP = 0;
        
        public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        {
            // MASTER TOGGLE: When disabled, this global system does nothing
            // Each NPC/boss implements its own unique VFX instead
            if (!VFXMasterToggle.GlobalSystemsEnabled)
                return false;
            
            // Apply to all MagnumOpus NPCs
            return entity.ModNPC?.Mod == ModContent.GetInstance<MagnumOpus>();
        }
        
        public override void OnSpawn(NPC npc, Terraria.DataStructures.IEntitySource source)
        {
            _detectedTheme = DetectTheme(npc);
            _isBoss = npc.boss || npc.lifeMax > 50000;
            _previousPosition = npc.Center;
            _oldPreviousPosition = npc.Center;
            _lastHP = npc.life;
            
            // Initialize position history
            for (int i = 0; i < _positionHistory.Length; i++)
            {
                _positionHistory[i] = npc.Center;
                _rotationHistory[i] = npc.rotation;
            }
            
            // Create trail for bosses
            if (_isBoss)
            {
                _trailId = AdvancedTrailSystem.CreateThemeTrail(_detectedTheme, 28f, 25, 1.2f);
                _hasTrail = _trailId >= 0;
            }
            
            // Spawn entrance VFX with fog and beams
            SpawnEntranceEffect(npc);
            _hasSpawned = true;
        }
        
        public override void AI(NPC npc)
        {
            // Update interpolation tracking
            _oldPreviousPosition = _previousPosition;
            _previousPosition = npc.Center;
            _interpolationTimer = InterpolatedRenderer.PartialTicks;
            
            // Update position history for trail rendering
            _historyIndex = (_historyIndex + 1) % _positionHistory.Length;
            _positionHistory[_historyIndex] = npc.Center;
            _rotationHistory[_historyIndex] = npc.rotation;
            
            // Check if NPC is moving fast enough for trail
            float speed = npc.velocity.Length();
            _isMovingFast = speed > 6f;
            
            // Update trail
            if (_hasTrail && _trailId >= 0)
            {
                AdvancedTrailSystem.UpdateTrail(_trailId, npc.Center, npc.rotation);
            }
            // Create trail for fast-moving non-bosses
            else if (_isMovingFast && !_hasTrail && !_isBoss)
            {
                _trailId = AdvancedTrailSystem.CreateThemeTrail(_detectedTheme, 18f, 15, 0.8f);
                _hasTrail = _trailId >= 0;
            }
            
            // Create aura if boss and doesn't have one
            if (_isBoss && !_hasAura && _hasSpawned)
            {
                float intensity = npc.boss ? 1.5f : 0.8f;
                BossEnemyVFXIntegration.CreateBossAura(npc.whoAmI, _detectedTheme, intensity);
                _hasAura = true;
            }
            
            // Detect attack state changes for telegraphs
            DetectAttackChanges(npc);
            
            // Spawn ambient particles (with fog hints)
            if (Main.rand.NextBool(_isBoss ? 3 : 6))
            {
                SpawnAmbientParticles(npc);
            }
            
            // Spawn fog during fast movement
            if (_isMovingFast && Main.rand.NextBool(_isBoss ? 2 : 4))
            {
                SpawnMovementFog(npc);
            }
            
            // Update damage flash
            if (_damageFlashTimer > 0)
                _damageFlashTimer -= 0.05f;
            
            // Cooldown
            if (_attackCooldown > 0)
                _attackCooldown--;
        }
        
        private void SpawnMovementFog(NPC npc)
        {
            var style = UniqueWeaponVFXStyles.GetStyle(_detectedTheme);
            
            // Fog particles behind the NPC as it moves
            Vector2 behindPos = npc.Center - npc.velocity.SafeNormalize(Vector2.Zero) * 20f;
            behindPos += Main.rand.NextVector2Circular(15f, 15f);
            
            float intensity = _isBoss ? 0.4f : 0.2f;
            WeaponFogVFX.SpawnAttackFog(behindPos, _detectedTheme, intensity, -npc.velocity * 0.3f);
            
            // Dust trail
            Dust trail = Dust.NewDustPerfect(behindPos, DustID.Cloud, 
                -npc.velocity * 0.1f, 80, style.Fog.PrimaryColor * 0.5f, 0.8f);
            trail.noGravity = true;
            trail.fadeIn = 1.1f;
        }
        
        public override void HitEffect(NPC npc, NPC.HitInfo hit)
        {
            if (string.IsNullOrEmpty(_detectedTheme))
                _detectedTheme = DetectTheme(npc);
            
            // Damage flash
            _damageFlashTimer = 1f;
            
            // Light beam impact on hit
            float impactScale = _isBoss ? 1.2f : 0.6f;
            if (hit.Crit)
                impactScale *= 1.5f;
            
            // Calculate hit direction vector from HitDirection (-1 = left, 1 = right)
            Vector2 hitDir = new Vector2(hit.HitDirection, 0f);
            LightBeamImpactVFX.SpawnEnemyImpact(npc.Center, hitDir, _detectedTheme, impactScale);
            
            // Spawn fog burst on heavy hits
            if (hit.Damage > npc.lifeMax * 0.05f || hit.Crit)
            {
                WeaponFogVFX.SpawnAttackFog(npc.Center, _detectedTheme, 0.5f, hitDir * -2f);
            }
            
            // Theme-specific hit particles
            SpawnHitParticles(npc, hit);
        }
        
        public override void OnKill(NPC npc)
        {
            if (string.IsNullOrEmpty(_detectedTheme))
                _detectedTheme = DetectTheme(npc);
            
            // Remove aura
            if (_hasAura)
            {
                BossEnemyVFXIntegration.RemoveBossAura(npc.whoAmI);
            }
            
            // End trail
            if (_hasTrail && _trailId >= 0)
            {
                AdvancedTrailSystem.EndTrail(_trailId);
            }
            
            // Death spectacle - more dramatic for bosses
            float intensity = _isBoss ? 2f : 0.8f;
            int duration = _isBoss ? 240 : 90;
            
            BossEnemyVFXIntegration.SpawnDeathSpectacle(npc.Center, _detectedTheme, intensity, duration);
            
            // Additional death effects with fog burst
            SpawnDeathEffects(npc);
            
            // Massive fog explosion on boss death
            if (_isBoss)
            {
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 fogPos = npc.Center + angle.ToRotationVector2() * 60f;
                    Vector2 fogVel = angle.ToRotationVector2() * 4f;
                    WeaponFogVFX.SpawnAttackFog(fogPos, _detectedTheme, 1.2f, fogVel);
                }
                
                // Sky flash
                var style = UniqueWeaponVFXStyles.GetStyle(_detectedTheme);
                DynamicSkyboxSystem.TriggerFlash(style.Fog.PrimaryColor, 1.5f);
            }
        }
        
        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (string.IsNullOrEmpty(_detectedTheme))
                return true;
            
            // Update partial ticks for accurate interpolation
            InterpolatedRenderer.UpdatePartialTicks();
            
            // Apply interpolated rendering for smooth 144Hz+
            Vector2 interpolatedPos = InterpolatedRenderer.GetInterpolatedCenter(npc);
            
            // Draw primitive trail using position history
            if (_isMovingFast || _isBoss)
            {
                DrawPrimitiveTrail(npc, spriteBatch, screenPos);
            }
            
            // Draw pre-effects (behind NPC)
            DrawPreEffects(npc, spriteBatch, interpolatedPos - screenPos);
            
            return true;
        }
        
        private void DrawPrimitiveTrail(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos)
        {
            if (_historyIndex < 3) return;
            
            var style = UniqueWeaponVFXStyles.GetStyle(_detectedTheme);
            
            // Build position array from history
            int pointCount = Math.Min(_historyIndex + 1, _positionHistory.Length);
            Vector2[] positions = new Vector2[pointCount];
            
            for (int i = 0; i < pointCount; i++)
            {
                int histIdx = (_historyIndex - i + _positionHistory.Length) % _positionHistory.Length;
                positions[i] = _positionHistory[histIdx];
            }
            
            // Only draw if we have enough points and movement
            if (positions.Length < 3) return;
            if (Vector2.Distance(positions[0], positions[positions.Length - 1]) < 20f) return;
            
            // Create width function based on boss status
            float baseWidth = _isBoss ? 24f : 14f;
            var widthFunc = EnhancedTrailRenderer.QuadraticBumpWidth(baseWidth);
            var colorFunc = EnhancedTrailRenderer.GradientColor(style.Fog.PrimaryColor, style.Fog.SecondaryColor, 0.6f);
            
            // Multi-pass trail (bloom outer, main, core)
            try
            {
                EnhancedTrailRenderer.RenderMultiPassTrail(positions, widthFunc, colorFunc, 2.5f, 0.35f, null, 40);
            }
            catch { /* Trail rendering is optional */ }
        }
        
        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (string.IsNullOrEmpty(_detectedTheme))
                return;
            
            Vector2 interpolatedPos = Vector2.Lerp(_previousPosition, npc.Center, _interpolationTimer);
            Vector2 drawPos = interpolatedPos - screenPos;
            
            // Draw post-effects (in front of NPC)
            DrawPostEffects(npc, spriteBatch, drawPos);
            
            // Damage flash overlay
            if (_damageFlashTimer > 0)
            {
                DrawDamageFlash(npc, spriteBatch, drawPos);
            }
        }
        
        #region Theme Detection
        
        private string DetectTheme(NPC npc)
        {
            string typeName = npc.ModNPC?.GetType().FullName ?? "";
            string name = npc.ModNPC?.Name ?? "";
            
            // Check namespace/name for theme keywords
            if (typeName.Contains("Fate") || name.Contains("Fate") || name.Contains("Herald") || name.Contains("Warden"))
                return "Fate";
            if (typeName.Contains("Eroica") || name.Contains("Eroica") || name.Contains("Valor") || name.Contains("Behemoth"))
                return "Eroica";
            if (typeName.Contains("SwanLake") || name.Contains("Swan") || name.Contains("Prima") || name.Contains("Monochromatic"))
                return "SwanLake";
            if (typeName.Contains("LaCampanella") || name.Contains("Campanella") || name.Contains("Bell") || name.Contains("Chime"))
                return "LaCampanella";
            if (typeName.Contains("MoonlightSonata") || name.Contains("Moonlight") || name.Contains("Lunar") || name.Contains("Waning"))
                return "MoonlightSonata";
            if (typeName.Contains("Enigma") || name.Contains("Enigma") || name.Contains("Mystery") || name.Contains("Hollow"))
                return "EnigmaVariations";
            if (typeName.Contains("DiesIrae") || name.Contains("Dies") || name.Contains("Irae") || name.Contains("Judgment"))
                return "DiesIrae";
            if (typeName.Contains("ClairDeLune") || name.Contains("Clair") || name.Contains("Lune"))
                return "ClairDeLune";
            if (typeName.Contains("Spring") || name.Contains("Primavera") || name.Contains("Spring"))
                return "Spring";
            if (typeName.Contains("Summer") || name.Contains("Estate") || name.Contains("Summer"))
                return "Summer";
            if (typeName.Contains("Autumn") || name.Contains("Autunno") || name.Contains("Autumn"))
                return "Autumn";
            if (typeName.Contains("Winter") || name.Contains("Inverno") || name.Contains("Winter"))
                return "Winter";
            if (typeName.Contains("Nachtmusik") || name.Contains("Nachtmusik") || name.Contains("Radiance"))
                return "Nachtmusik";
            if (typeName.Contains("OdeToJoy") || name.Contains("Ode") || name.Contains("Joy") || name.Contains("Chromatic"))
                return "OdeToJoy";
            
            // Default
            return "Eroica";
        }
        
        #endregion
        
        #region Effect Spawning
        
        private void SpawnEntranceEffect(NPC npc)
        {
            if (string.IsNullOrEmpty(_detectedTheme))
                _detectedTheme = DetectTheme(npc);
            
            var style = UniqueWeaponVFXStyles.GetStyle(_detectedTheme);
            
            // Fog burst on spawn
            float fogScale = _isBoss ? 1.5f : 0.7f;
            WeaponFogVFX.SpawnAttackFog(npc.Center, _detectedTheme, fogScale, Vector2.Zero);
            
            // Light beam burst
            LightBeamImpactVFX.SpawnImpact(npc.Center, _detectedTheme, fogScale);
            
            // Spawn entrance particles
            int particleCount = _isBoss ? 30 : 12;
            for (int i = 0; i < particleCount; i++)
            {
                float angle = MathHelper.TwoPi * i / particleCount;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                
                CustomParticles.GenericFlare(npc.Center + vel * 5f, style.Fog.PrimaryColor, 0.4f, 25);
            }
            
            // Cascading halos for bosses
            if (_isBoss)
            {
                for (int ring = 0; ring < 6; ring++)
                {
                    Color ringColor = Color.Lerp(style.Fog.PrimaryColor, style.Fog.SecondaryColor, ring / 6f);
                    CustomParticles.HaloRing(npc.Center, ringColor, 0.4f + ring * 0.15f, 18 + ring * 3);
                }
                
                // Screen shake for boss spawn
                if (Main.LocalPlayer.Distance(npc.Center) < 1500f)
                {
                    Main.LocalPlayer.GetModPlayer<MagnumScreenShakePlayer>()?.AddShake(12f, 25);
                }
            }
        }
        
        private void SpawnAmbientParticles(NPC npc)
        {
            var style = UniqueWeaponVFXStyles.GetStyle(_detectedTheme);
            
            Vector2 offset = Main.rand.NextVector2Circular(npc.width * 0.6f, npc.height * 0.6f);
            Vector2 pos = npc.Center + offset;
            
            // Theme-specific ambient particles
            switch (_detectedTheme.ToLower())
            {
                case "fate":
                    // Cosmic nebula + glyphs + stars
                    CustomParticles.GenericFlare(pos, style.Fog.PrimaryColor * 0.4f, 0.25f, 20);
                    if (Main.rand.NextBool(4))
                        CustomParticles.Glyph(pos, style.Fog.SecondaryColor, 0.3f, -1);
                    break;
                    
                case "eroica":
                    // Heroic embers + sakura hints
                    CustomParticles.GenericFlare(pos, style.Fog.PrimaryColor * 0.5f, 0.3f, 18);
                    if (Main.rand.NextBool(3))
                    {
                        Dust d = Dust.NewDustPerfect(pos, DustID.Enchanted_Gold, Main.rand.NextVector2Circular(1f, 1f), 0, Color.Gold, 0.8f);
                        d.noGravity = true;
                    }
                    break;
                    
                case "swanlake":
                    // Prismatic feathers + sparkles
                    float hue = (Main.GameUpdateCount * 0.02f) % 1f;
                    Color rainbow = Main.hslToRgb(hue, 1f, 0.7f);
                    CustomParticles.PrismaticSparkle(pos, rainbow, 0.25f);
                    if (Main.rand.NextBool(4))
                        CustomParticles.SwanFeatherDrift(pos, Color.White, 0.35f);
                    break;
                    
                case "lacampanella":
                    // Infernal smoke + embers
                    CustomParticles.GenericFlare(pos, style.Fog.PrimaryColor * 0.6f, 0.3f, 16);
                    Dust ember = Dust.NewDustPerfect(pos, DustID.Torch, Main.rand.NextVector2Circular(2f, 2f), 0, style.Fog.PrimaryColor, 1f);
                    ember.noGravity = true;
                    break;
                    
                case "moonlightsonata":
                    // Lunar mist + soft purple
                    CustomParticles.GenericFlare(pos, style.Fog.PrimaryColor * 0.35f, 0.35f, 25);
                    Dust lunar = Dust.NewDustPerfect(pos, DustID.PurpleTorch, Main.rand.NextVector2Circular(1f, 1f), 100, style.Fog.PrimaryColor, 0.7f);
                    lunar.noGravity = true;
                    lunar.fadeIn = 1.2f;
                    break;
                    
                case "enigmavariations":
                case "enigma":
                    // Void mystery + eyes + glyphs
                    CustomParticles.GenericFlare(pos, style.Fog.SecondaryColor * 0.4f, 0.25f, 22);
                    if (Main.rand.NextBool(5))
                        CustomParticles.Glyph(pos, style.Fog.PrimaryColor, 0.35f, -1);
                    break;
                    
                default:
                    CustomParticles.GenericFlare(pos, style.Fog.PrimaryColor * 0.4f, 0.25f, 20);
                    break;
            }
        }
        
        private void DetectAttackChanges(NPC npc)
        {
            // Detect AI state changes that might indicate attack windups
            float currentAI0 = npc.ai[0];
            
            if (currentAI0 != _lastAI0 && _attackCooldown <= 0)
            {
                // AI state changed - likely entering an attack
                // Spawn telegraph based on current state
                
                if (_isBoss && Main.rand.NextBool(2)) // Not every state change
                {
                    SpawnAttackTelegraph(npc, currentAI0);
                    _attackCooldown = 30; // Cooldown before next telegraph
                }
            }
            
            _lastAI0 = currentAI0;
            _lastAI1 = npc.ai[1];
        }
        
        private void SpawnAttackTelegraph(NPC npc, float attackState)
        {
            // Choose telegraph type based on theme
            var telegraphType = _detectedTheme.ToLower() switch
            {
                "fate" => BossEnemyVFXIntegration.TelegraphType.StarConstellation,
                "eroica" => BossEnemyVFXIntegration.TelegraphType.FlameGathering,
                "swanlake" => BossEnemyVFXIntegration.TelegraphType.FeatherSwirl,
                "lacampanella" => BossEnemyVFXIntegration.TelegraphType.FlameGathering,
                "moonlightsonata" => BossEnemyVFXIntegration.TelegraphType.ExpandingRing,
                "enigmavariations" or "enigma" => BossEnemyVFXIntegration.TelegraphType.EyeFormation,
                _ => BossEnemyVFXIntegration.TelegraphType.ConvergingParticles
            };
            
            float radius = _isBoss ? 120f : 60f;
            int duration = 45;
            
            BossEnemyVFXIntegration.SpawnAttackTelegraph(npc.Center, _detectedTheme, telegraphType, duration, radius);
        }
        
        private void SpawnHitParticles(NPC npc, NPC.HitInfo hit)
        {
            var style = UniqueWeaponVFXStyles.GetStyle(_detectedTheme);
            int particleCount = hit.Crit ? 12 : 6;
            
            for (int i = 0; i < particleCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f);
                
                Color color = Color.Lerp(style.Fog.PrimaryColor, style.Fog.SecondaryColor, Main.rand.NextFloat());
                CustomParticles.GenericFlare(npc.Center + vel * 3f, color, 0.3f, 15);
            }
            
            // Theme-specific hit effects
            switch (_detectedTheme.ToLower())
            {
                case "fate":
                    CustomParticles.Glyph(npc.Center, style.Fog.SecondaryColor, 0.5f, -1);
                    break;
                case "swanlake":
                    CustomParticles.SwanFeatherDrift(npc.Center, Color.White, 0.5f);
                    break;
                case "enigmavariations":
                case "enigma":
                    CustomParticles.Glyph(npc.Center, style.Fog.PrimaryColor, 0.45f, -1);
                    break;
            }
        }
        
        private void SpawnDeathEffects(NPC npc)
        {
            var style = UniqueWeaponVFXStyles.GetStyle(_detectedTheme);
            
            // Massive particle burst
            int particleCount = _isBoss ? 50 : 20;
            for (int i = 0; i < particleCount; i++)
            {
                float angle = MathHelper.TwoPi * i / particleCount;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 15f);
                
                Color color = Color.Lerp(style.Fog.PrimaryColor, style.Fog.SecondaryColor, (float)i / particleCount);
                CustomParticles.GenericFlare(npc.Center + vel * 2f, color, 0.5f + Main.rand.NextFloat(0.3f), 30);
            }
            
            // Cascading halos
            int ringCount = _isBoss ? 12 : 5;
            for (int ring = 0; ring < ringCount; ring++)
            {
                Color ringColor = Color.Lerp(style.Fog.PrimaryColor, style.Fog.SecondaryColor, (float)ring / ringCount);
                float scale = 0.4f + ring * 0.15f;
                int lifetime = 20 + ring * 4;
                CustomParticles.HaloRing(npc.Center, ringColor, scale, lifetime);
            }
            
            // Theme-specific death extras
            switch (_detectedTheme.ToLower())
            {
                case "fate":
                    // Glyph explosion
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 8f;
                        Vector2 glyphPos = npc.Center + angle.ToRotationVector2() * 50f;
                        CustomParticles.Glyph(glyphPos, style.Fog.SecondaryColor, 0.6f, i % 12);
                    }
                    break;
                    
                case "swanlake":
                    // Feather explosion
                    for (int i = 0; i < 15; i++)
                    {
                        CustomParticles.SwanFeatherDrift(npc.Center + Main.rand.NextVector2Circular(40f, 40f), 
                            Main.rand.NextBool() ? Color.White : new Color(20, 20, 30), 0.6f);
                    }
                    break;
                    
                case "enigmavariations":
                case "enigma":
                    // Eye burst
                    for (int i = 0; i < 6; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 6f;
                        Vector2 eyePos = npc.Center + angle.ToRotationVector2() * 40f;
                        CustomParticles.Glyph(eyePos, style.Fog.PrimaryColor, 0.5f, 8 + (i % 4));
                    }
                    break;
            }
            
            // Fog burst
            WeaponFogVFX.SpawnAttackFog(npc.Center, _detectedTheme, _isBoss ? 2f : 1f, Vector2.Zero);
            
            // Light beam death burst
            LightBeamImpactVFX.SpawnImpact(npc.Center, _detectedTheme, _isBoss ? 2.5f : 1.2f);
        }
        
        #endregion
        
        #region Rendering
        
        private void DrawPreEffects(NPC npc, SpriteBatch spriteBatch, Vector2 drawPos)
        {
            // Draw subtle glow behind NPC with multi-layer bloom
            Texture2D glowTex = MagnumTextureRegistry.GetBloom();
            var style = UniqueWeaponVFXStyles.GetStyle(_detectedTheme);
            
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.15f;
            float baseScale = (_isBoss ? 1.8f : 0.9f) * pulse;
            
            // Multi-layer bloom (4 layers for smooth glow)
            float[] scales = { 2.2f, 1.6f, 1.1f, 0.6f };
            float[] opacities = { 0.08f, 0.12f, 0.18f, 0.25f };
            
            for (int i = 0; i < 4; i++)
            {
                Color layerColor = Color.Lerp(style.Fog.PrimaryColor, style.Fog.SecondaryColor, i / 4f);
                layerColor = layerColor.WithoutAlpha(); // Remove alpha for additive
                
                spriteBatch.Draw(glowTex, drawPos, null, layerColor * opacities[i], 0f,
                    glowTex.Size() / 2f, baseScale * scales[i], SpriteEffects.None, 0f);
            }
            
            // Boss-specific aura ring
            if (_isBoss)
            {
                float ringRot = Main.GameUpdateCount * 0.02f;
                Texture2D haloTex = MagnumTextureRegistry.GetHaloRing();
                if (haloTex != null)
                {
                    spriteBatch.Draw(haloTex, drawPos, null, style.Fog.PrimaryColor * 0.2f, ringRot,
                        haloTex.Size() / 2f, baseScale * 1.8f, SpriteEffects.None, 0f);
                }
            }
        }
        
        private void DrawPostEffects(NPC npc, SpriteBatch spriteBatch, Vector2 drawPos)
        {
            var style = UniqueWeaponVFXStyles.GetStyle(_detectedTheme);
            
            // Draw theme-specific overlays
            if (_isBoss)
            {
                // Orbiting glyph particles for magical bosses
                if (_detectedTheme == "Fate" || _detectedTheme == "EnigmaVariations")
                {
                    DrawOrbitingGlyphs(spriteBatch, drawPos, style);
                }
                
                // Orbiting feathers for Swan Lake
                if (_detectedTheme == "SwanLake")
                {
                    DrawOrbitingFeathers(spriteBatch, drawPos);
                }
            }
        }
        
        private void DrawOrbitingGlyphs(SpriteBatch spriteBatch, Vector2 drawPos, UniqueWeaponVFXStyles.ThemeVFXStyle style)
        {
            Texture2D glyphTex = MagnumTextureRegistry.GetFlare();
            if (glyphTex == null) return;
            
            int glyphCount = 4;
            float orbitRadius = 80f;
            float rotSpeed = Main.GameUpdateCount * 0.025f;
            
            for (int i = 0; i < glyphCount; i++)
            {
                float angle = rotSpeed + MathHelper.TwoPi * i / glyphCount;
                Vector2 offset = angle.ToRotationVector2() * orbitRadius;
                float glyphRot = angle + MathHelper.PiOver2;
                
                spriteBatch.Draw(glyphTex, drawPos + offset, null, style.Fog.SecondaryColor * 0.4f, glyphRot,
                    glyphTex.Size() / 2f, 0.35f, SpriteEffects.None, 0f);
            }
        }
        
        private void DrawOrbitingFeathers(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            Texture2D featherTex = MagnumTextureRegistry.GetSoftGlow();
            if (featherTex == null) return;
            
            int featherCount = 6;
            float orbitRadius = 90f;
            float rotSpeed = Main.GameUpdateCount * 0.015f;
            
            for (int i = 0; i < featherCount; i++)
            {
                float angle = rotSpeed + MathHelper.TwoPi * i / featherCount;
                Vector2 offset = angle.ToRotationVector2() * orbitRadius;
                float wobble = (float)Math.Sin(Main.GameUpdateCount * 0.1f + i) * 10f;
                offset.Y += wobble;
                
                bool isBlack = i % 2 == 0;
                Color featherColor = isBlack ? new Color(30, 30, 40) : Color.White;
                float featherRot = angle + MathHelper.PiOver4;
                
                spriteBatch.Draw(featherTex, drawPos + offset, null, featherColor * 0.5f, featherRot,
                    featherTex.Size() / 2f, 0.4f, SpriteEffects.None, 0f);
            }
        }
        
        private void DrawDamageFlash(NPC npc, SpriteBatch spriteBatch, Vector2 drawPos)
        {
            Texture2D glowTex = MagnumTextureRegistry.GetBloom();
            var style = UniqueWeaponVFXStyles.GetStyle(_detectedTheme);
            
            float alpha = _damageFlashTimer * 0.4f;
            float scale = 1f + _damageFlashTimer * 0.6f;
            
            // Multi-layer damage flash
            spriteBatch.Draw(glowTex, drawPos, null, Color.White * alpha, 0f,
                glowTex.Size() / 2f, scale * 1.2f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, style.Fog.PrimaryColor * alpha * 0.7f, 0f,
                glowTex.Size() / 2f, scale, SpriteEffects.None, 0f);
        }
        
        #endregion
    }
}
