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
    /// BOSS & ENEMY VFX INTEGRATION SYSTEM
    /// 
    /// Automatically applies unique, dynamic visual effects to ALL bosses and enemies:
    /// 1. UNIQUE FOG EFFECTS - Theme-specific nebula/mist on attacks
    /// 2. LIGHT BEAM IMPACTS - Stretching rays on hits
    /// 3. INTERPOLATED RENDERING - 144Hz+ smooth visuals
    /// 4. ATTACK TELEGRAPHS - Warning visuals before attacks
    /// 5. AMBIENT AURAS - Passive visual presence
    /// 6. DEATH SPECTACLES - Dramatic death animations
    /// 7. ATTACK-SPECIFIC VFX - Per-attack unique visuals
    /// 
    /// Uses: WeaponFogAndBeamVFX, UniqueWeaponVFXStyles, CalamityStyleVFX, 
    ///       ImpactLightRays, NebulaFogSystem, EnhancedTrailRenderer
    /// </summary>
    public class BossEnemyVFXIntegration : ModSystem
    {
        #region Active Effect Instances
        
        private static List<BossAura> _activeAuras = new List<BossAura>();
        private static List<AttackTelegraph> _activeTelegraphs = new List<AttackTelegraph>();
        private static List<DeathSpectacle> _activeDeaths = new List<DeathSpectacle>();
        
        private const int MaxAuras = 20;
        private const int MaxTelegraphs = 50;
        private const int MaxDeaths = 10;
        
        #endregion
        
        #region Aura System
        
        private class BossAura
        {
            public int NPCIndex;
            public string Theme;
            public float Intensity;
            public float Rotation;
            public float PulseTimer;
            public Color PrimaryColor;
            public Color SecondaryColor;
            public AuraType Type;
            public List<OrbitingElement> Orbiters;
        }
        
        private struct OrbitingElement
        {
            public float Angle;
            public float Radius;
            public float Speed;
            public float Size;
            public OrbiterType Type;
        }
        
        private enum OrbiterType { Glyph, Star, MusicNote, Feather, Flame, Eye, Crystal }
        
        public enum AuraType
        {
            CosmicNebula,       // Fate - swirling cosmic clouds
            InfernalFlame,      // LaCampanella - burning embers
            PrismaticBallet,    // SwanLake - rainbow feathers
            VoidMystery,        // Enigma - watching eyes, glyphs
            HeroicGlow,         // Eroica - golden sakura
            LunarMist,          // MoonlightSonata - purple mist
            CelestialLight,     // ClairDeLune - soft glow
            SpringBloom,        // Spring - flower petals
            SummerHeat,         // Summer - heat shimmer
            AutumnLeaves,       // Autumn - falling leaves
            WinterFrost         // Winter - ice crystals
        }
        
        #endregion
        
        #region Telegraph System
        
        private class AttackTelegraph
        {
            public Vector2 Position;
            public Vector2 PreviousPosition;
            public int Timer;
            public int MaxDuration;
            public float Scale;
            public Color Color;
            public TelegraphType Type;
            public Vector2 Direction;
            public float Radius;
            public List<TelegraphElement> Elements;
            public string Theme;
        }
        
        private struct TelegraphElement
        {
            public Vector2 LocalPos;
            public float Rotation;
            public float Scale;
            public Color Tint;
        }
        
        public enum TelegraphType
        {
            ConvergingParticles,    // Particles spiral inward
            ExpandingRing,          // Warning ring expands
            DirectionalLine,        // Line shows attack direction
            SafeZoneArc,            // Shows safe area
            GlyphCircle,            // Magic circle forms
            EyeFormation,           // Eyes gather and watch
            StarConstellation,      // Stars connect in pattern
            FlameGathering,         // Flames converge
            FeatherSwirl,           // Feathers spiral
            LunarPhases,            // Moon phases appear
            FrostPattern            // Ice crystal formation
        }
        
        #endregion
        
        #region Death Spectacle System
        
        private class DeathSpectacle
        {
            public Vector2 Position;
            public int Timer;
            public int MaxDuration;
            public string Theme;
            public float Intensity;
            public List<DeathParticle> Particles;
            public int Phase; // 0 = building, 1 = climax, 2 = fadeout
        }
        
        private struct DeathParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public Color Color;
            public float Rotation;
            public float RotationSpeed;
            public int Lifetime;
            public int Timer;
        }
        
        #endregion
        
        #region System Updates
        
        public override void PostUpdateEverything()
        {
            UpdateAuras();
            UpdateTelegraphs();
            UpdateDeaths();
        }
        
        private void UpdateAuras()
        {
            for (int i = _activeAuras.Count - 1; i >= 0; i--)
            {
                var aura = _activeAuras[i];
                
                // Check if NPC still exists
                if (aura.NPCIndex < 0 || aura.NPCIndex >= Main.maxNPCs || !Main.npc[aura.NPCIndex].active)
                {
                    _activeAuras.RemoveAt(i);
                    continue;
                }
                
                NPC npc = Main.npc[aura.NPCIndex];
                
                // Update rotation and pulse
                aura.Rotation += 0.02f;
                aura.PulseTimer += 0.05f;
                float pulse = 1f + (float)Math.Sin(aura.PulseTimer) * 0.15f;
                
                // Update orbiters
                foreach (var orbiter in aura.Orbiters)
                {
                    // Orbiters will be rendered in draw phase
                }
                
                // Spawn ambient particles based on aura type
                SpawnAuraParticles(npc.Center, aura, pulse);
            }
        }
        
        private void SpawnAuraParticles(Vector2 center, BossAura aura, float pulse)
        {
            if (Main.rand.NextBool(3))
            {
                float angle = aura.Rotation + Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = 60f * pulse + Main.rand.NextFloat(40f);
                Vector2 particlePos = center + angle.ToRotationVector2() * radius;
                
                switch (aura.Type)
                {
                    case AuraType.CosmicNebula:
                        SpawnCosmicAuraParticle(particlePos, aura);
                        break;
                    case AuraType.InfernalFlame:
                        SpawnInfernalAuraParticle(particlePos, aura);
                        break;
                    case AuraType.PrismaticBallet:
                        SpawnPrismaticAuraParticle(particlePos, aura);
                        break;
                    case AuraType.VoidMystery:
                        SpawnVoidAuraParticle(particlePos, aura);
                        break;
                    case AuraType.HeroicGlow:
                        SpawnHeroicAuraParticle(particlePos, aura);
                        break;
                    case AuraType.LunarMist:
                        SpawnLunarAuraParticle(particlePos, aura);
                        break;
                    default:
                        SpawnGenericAuraParticle(particlePos, aura);
                        break;
                }
            }
        }
        
        private void SpawnCosmicAuraParticle(Vector2 pos, BossAura aura)
        {
            // Cosmic nebula - stars and cosmic clouds
            CustomParticles.GenericFlare(pos, aura.PrimaryColor * 0.6f, 0.3f, 20);
            if (Main.rand.NextBool(2))
                CustomParticles.Glyph(pos, aura.SecondaryColor, 0.25f, -1);
        }
        
        private void SpawnInfernalAuraParticle(Vector2 pos, BossAura aura)
        {
            // Infernal - embers and smoke
            CustomParticles.GenericFlare(pos, aura.PrimaryColor, 0.35f, 18);
            Dust d = Dust.NewDustPerfect(pos, DustID.Torch, Main.rand.NextVector2Circular(2f, 2f), 0, aura.PrimaryColor, 1.2f);
            d.noGravity = true;
        }
        
        private void SpawnPrismaticAuraParticle(Vector2 pos, BossAura aura)
        {
            // Prismatic - rainbow feathers and sparkles
            float hue = (Main.GameUpdateCount * 0.02f) % 1f;
            Color rainbow = Main.hslToRgb(hue, 1f, 0.7f);
            CustomParticles.PrismaticSparkle(pos, rainbow, 0.3f);
            if (Main.rand.NextBool(3))
                CustomParticles.SwanFeatherDrift(pos, Color.White, 0.4f);
        }
        
        private void SpawnVoidAuraParticle(Vector2 pos, BossAura aura)
        {
            // Void - eyes and glyphs
            CustomParticles.GenericFlare(pos, aura.SecondaryColor * 0.5f, 0.25f, 22);
            if (Main.rand.NextBool(4))
                CustomParticles.Glyph(pos, aura.PrimaryColor, 0.3f, -1);
        }
        
        private void SpawnHeroicAuraParticle(Vector2 pos, BossAura aura)
        {
            // Heroic - golden embers and sakura
            CustomParticles.GenericFlare(pos, aura.PrimaryColor, 0.3f, 18);
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustPerfect(pos, DustID.Enchanted_Gold, Main.rand.NextVector2Circular(1f, 1f), 0, aura.SecondaryColor, 1f);
                d.noGravity = true;
            }
        }
        
        private void SpawnLunarAuraParticle(Vector2 pos, BossAura aura)
        {
            // Lunar - soft purple mist
            CustomParticles.GenericFlare(pos, aura.PrimaryColor * 0.4f, 0.35f, 25);
            Dust d = Dust.NewDustPerfect(pos, DustID.PurpleTorch, Main.rand.NextVector2Circular(1f, 1f), 100, aura.PrimaryColor, 0.8f);
            d.noGravity = true;
            d.fadeIn = 1.2f;
        }
        
        private void SpawnGenericAuraParticle(Vector2 pos, BossAura aura)
        {
            CustomParticles.GenericFlare(pos, aura.PrimaryColor * 0.5f, 0.3f, 20);
        }
        
        private void UpdateTelegraphs()
        {
            for (int i = _activeTelegraphs.Count - 1; i >= 0; i--)
            {
                var telegraph = _activeTelegraphs[i];
                telegraph.PreviousPosition = telegraph.Position;
                telegraph.Timer++;
                
                if (telegraph.Timer >= telegraph.MaxDuration)
                {
                    _activeTelegraphs.RemoveAt(i);
                    continue;
                }
                
                float progress = (float)telegraph.Timer / telegraph.MaxDuration;
                
                // Spawn telegraph particles based on type
                SpawnTelegraphParticles(telegraph, progress);
            }
        }
        
        private void SpawnTelegraphParticles(AttackTelegraph telegraph, float progress)
        {
            switch (telegraph.Type)
            {
                case TelegraphType.ConvergingParticles:
                    SpawnConvergingTelegraph(telegraph, progress);
                    break;
                case TelegraphType.ExpandingRing:
                    SpawnExpandingRingTelegraph(telegraph, progress);
                    break;
                case TelegraphType.DirectionalLine:
                    SpawnDirectionalLineTelegraph(telegraph, progress);
                    break;
                case TelegraphType.GlyphCircle:
                    SpawnGlyphCircleTelegraph(telegraph, progress);
                    break;
                case TelegraphType.EyeFormation:
                    SpawnEyeFormationTelegraph(telegraph, progress);
                    break;
                case TelegraphType.StarConstellation:
                    SpawnConstellationTelegraph(telegraph, progress);
                    break;
                case TelegraphType.FlameGathering:
                    SpawnFlameGatheringTelegraph(telegraph, progress);
                    break;
                case TelegraphType.FeatherSwirl:
                    SpawnFeatherSwirlTelegraph(telegraph, progress);
                    break;
            }
        }
        
        private void SpawnConvergingTelegraph(AttackTelegraph telegraph, float progress)
        {
            float inverseProgress = 1f - progress;
            int particleCount = (int)(8 + progress * 8);
            
            for (int i = 0; i < particleCount; i++)
            {
                float angle = MathHelper.TwoPi * i / particleCount + Main.GameUpdateCount * 0.03f;
                float radius = telegraph.Radius * inverseProgress;
                Vector2 pos = telegraph.Position + angle.ToRotationVector2() * radius;
                
                Color color = Color.Lerp(telegraph.Color, Color.White, progress * 0.5f);
                CustomParticles.GenericFlare(pos, color, 0.3f + progress * 0.3f, 8);
            }
        }
        
        private void SpawnExpandingRingTelegraph(AttackTelegraph telegraph, float progress)
        {
            float radius = telegraph.Radius * progress;
            int segments = 16;
            
            for (int i = 0; i < segments; i++)
            {
                float angle = MathHelper.TwoPi * i / segments;
                Vector2 pos = telegraph.Position + angle.ToRotationVector2() * radius;
                
                Color color = telegraph.Color * (1f - progress * 0.5f);
                CustomParticles.GenericFlare(pos, color, 0.25f, 6);
            }
        }
        
        private void SpawnDirectionalLineTelegraph(AttackTelegraph telegraph, float progress)
        {
            int points = 10;
            float length = telegraph.Radius * (0.5f + progress * 0.5f);
            
            for (int i = 0; i < points; i++)
            {
                float t = (float)i / points;
                Vector2 pos = telegraph.Position + telegraph.Direction * length * t;
                
                Color color = Color.Lerp(telegraph.Color, Color.Red, t * progress);
                float scale = 0.3f * (1f - t * 0.5f);
                CustomParticles.GenericFlare(pos, color, scale, 5);
            }
        }
        
        private void SpawnGlyphCircleTelegraph(AttackTelegraph telegraph, float progress)
        {
            int glyphCount = 6 + (int)(progress * 4);
            float rotation = Main.GameUpdateCount * 0.02f;
            
            for (int i = 0; i < glyphCount; i++)
            {
                float angle = MathHelper.TwoPi * i / glyphCount + rotation;
                float radius = telegraph.Radius * (0.7f + progress * 0.3f);
                Vector2 pos = telegraph.Position + angle.ToRotationVector2() * radius;
                
                CustomParticles.Glyph(pos, telegraph.Color, 0.35f + progress * 0.2f, i % 12);
            }
        }
        
        private void SpawnEyeFormationTelegraph(AttackTelegraph telegraph, float progress)
        {
            int eyeCount = 5;
            float rotation = Main.GameUpdateCount * 0.01f;
            
            for (int i = 0; i < eyeCount; i++)
            {
                float angle = MathHelper.TwoPi * i / eyeCount + rotation;
                float radius = telegraph.Radius * progress;
                Vector2 pos = telegraph.Position + angle.ToRotationVector2() * radius;
                
                // Eyes look toward center (or player)
                CustomParticles.Glyph(pos, telegraph.Color, 0.4f, 8 + (i % 4)); // Eye glyphs
            }
        }
        
        private void SpawnConstellationTelegraph(AttackTelegraph telegraph, float progress)
        {
            int starCount = 8;
            
            for (int i = 0; i < starCount; i++)
            {
                float angle = MathHelper.TwoPi * i / starCount;
                float radius = telegraph.Radius * (0.5f + progress * 0.5f);
                Vector2 pos = telegraph.Position + angle.ToRotationVector2() * radius;
                
                CustomParticles.GenericFlare(pos, Color.White, 0.4f + progress * 0.3f, 10);
                
                // Connect stars with faint lines (visual through dust)
                if (Main.rand.NextBool(3))
                {
                    int next = (i + 1) % starCount;
                    float nextAngle = MathHelper.TwoPi * next / starCount;
                    Vector2 nextPos = telegraph.Position + nextAngle.ToRotationVector2() * radius;
                    Vector2 mid = (pos + nextPos) / 2f;
                    CustomParticles.GenericFlare(mid, telegraph.Color * 0.3f, 0.15f, 5);
                }
            }
        }
        
        private void SpawnFlameGatheringTelegraph(AttackTelegraph telegraph, float progress)
        {
            int flameCount = (int)(6 + progress * 10);
            float inverseProgress = 1f - progress;
            
            for (int i = 0; i < flameCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = telegraph.Radius * inverseProgress * Main.rand.NextFloat(0.5f, 1f);
                Vector2 pos = telegraph.Position + angle.ToRotationVector2() * radius;
                
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, (telegraph.Position - pos).SafeNormalize(Vector2.Zero) * 2f, 0, telegraph.Color, 1.5f);
                d.noGravity = true;
            }
        }
        
        private void SpawnFeatherSwirlTelegraph(AttackTelegraph telegraph, float progress)
        {
            int featherCount = (int)(4 + progress * 6);
            float rotation = Main.GameUpdateCount * 0.04f;
            
            for (int i = 0; i < featherCount; i++)
            {
                float angle = MathHelper.TwoPi * i / featherCount + rotation;
                float radius = telegraph.Radius * (1f - progress * 0.3f);
                Vector2 pos = telegraph.Position + angle.ToRotationVector2() * radius;
                
                CustomParticles.SwanFeatherDrift(pos, telegraph.Color, 0.4f);
            }
        }
        
        private void UpdateDeaths()
        {
            for (int i = _activeDeaths.Count - 1; i >= 0; i--)
            {
                var death = _activeDeaths[i];
                death.Timer++;
                
                if (death.Timer >= death.MaxDuration)
                {
                    _activeDeaths.RemoveAt(i);
                    continue;
                }
                
                float progress = (float)death.Timer / death.MaxDuration;
                
                // Update phase
                if (progress < 0.4f)
                    death.Phase = 0; // Building
                else if (progress < 0.7f)
                    death.Phase = 1; // Climax
                else
                    death.Phase = 2; // Fadeout
                
                // Spawn death particles
                SpawnDeathParticles(death, progress);
            }
        }
        
        private void SpawnDeathParticles(DeathSpectacle death, float progress)
        {
            var style = UniqueWeaponVFXStyles.GetStyle(death.Theme);
            
            switch (death.Phase)
            {
                case 0: // Building - particles converge
                    if (Main.rand.NextBool(2))
                    {
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        float radius = 150f * (1f - progress * 2f);
                        Vector2 pos = death.Position + angle.ToRotationVector2() * radius;
                        
                        CustomParticles.GenericFlare(pos, style.Fog.PrimaryColor, 0.4f, 15);
                        WeaponFogVFX.SpawnAttackFog(pos, death.Theme, 0.3f, (death.Position - pos).SafeNormalize(Vector2.Zero) * 3f);
                    }
                    break;
                    
                case 1: // Climax - massive explosion
                    if (death.Timer == (int)(death.MaxDuration * 0.4f) + 1)
                    {
                        // Single massive burst at climax
                        CalamityStyleVFX.SpectacularDeath(death.Position, death.Theme);
                        LightBeamImpactVFX.SpawnImpact(death.Position, death.Theme, 2.5f);
                        
                        // Cascading halos
                        for (int ring = 0; ring < 10; ring++)
                        {
                            Color ringColor = Color.Lerp(style.Fog.PrimaryColor, style.Fog.SecondaryColor, ring / 10f);
                            CustomParticles.HaloRing(death.Position, ringColor, 0.5f + ring * 0.2f, 20 + ring * 3);
                        }
                        
                        // Screen shake
                        if (Main.LocalPlayer.Distance(death.Position) < 1500f)
                        {
                            Main.LocalPlayer.GetModPlayer<MagnumScreenShakePlayer>()?.AddShake(20f, 40);
                        }
                    }
                    break;
                    
                case 2: // Fadeout - lingering particles
                    if (Main.rand.NextBool(4))
                    {
                        Vector2 pos = death.Position + Main.rand.NextVector2Circular(100f, 100f);
                        float fadeProgress = (progress - 0.7f) / 0.3f;
                        CustomParticles.GenericFlare(pos, style.Fog.PrimaryColor * (1f - fadeProgress), 0.3f, 20);
                    }
                    break;
            }
        }
        
        #endregion
        
        #region Rendering
        
        public override void PostDrawTiles()
        {
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            try
            {
                RenderAuras();
                RenderTelegraphs();
                RenderDeaths();
            }
            catch (Exception ex)
            {
                Mod.Logger.Warn($"BossEnemyVFXIntegration render error: {ex.Message}");
            }
            
            Main.spriteBatch.End();
        }
        
        private void RenderAuras()
        {
            Texture2D glowTex = MagnumTextureRegistry.GetBloom();
            
            foreach (var aura in _activeAuras)
            {
                if (aura.NPCIndex < 0 || aura.NPCIndex >= Main.maxNPCs || !Main.npc[aura.NPCIndex].active)
                    continue;
                
                NPC npc = Main.npc[aura.NPCIndex];
                Vector2 drawPos = npc.Center - Main.screenPosition;
                float pulse = 1f + (float)Math.Sin(aura.PulseTimer) * 0.15f;
                
                // Draw multi-layer bloom aura
                for (int layer = 0; layer < 4; layer++)
                {
                    float scale = (1.5f + layer * 0.5f) * pulse * aura.Intensity;
                    float alpha = 0.15f / (layer + 1);
                    Color color = Color.Lerp(aura.PrimaryColor, aura.SecondaryColor, layer / 4f);
                    
                    Main.spriteBatch.Draw(glowTex, drawPos, null, color * alpha, aura.Rotation + layer * 0.3f,
                        glowTex.Size() / 2f, scale, SpriteEffects.None, 0f);
                }
                
                // Render orbiters
                foreach (var orbiter in aura.Orbiters)
                {
                    float orbiterAngle = aura.Rotation * orbiter.Speed + orbiter.Angle;
                    Vector2 orbiterPos = drawPos + orbiterAngle.ToRotationVector2() * orbiter.Radius * pulse;
                    
                    Color orbiterColor = Color.Lerp(aura.PrimaryColor, aura.SecondaryColor, Main.rand.NextFloat());
                    Main.spriteBatch.Draw(glowTex, orbiterPos, null, orbiterColor * 0.7f, 0f,
                        glowTex.Size() / 2f, orbiter.Size * 0.3f, SpriteEffects.None, 0f);
                }
            }
        }
        
        private void RenderTelegraphs()
        {
            Texture2D glowTex = MagnumTextureRegistry.GetBloom();
            
            foreach (var telegraph in _activeTelegraphs)
            {
                float progress = (float)telegraph.Timer / telegraph.MaxDuration;
                Vector2 drawPos = telegraph.Position - Main.screenPosition;
                
                // Core warning glow
                float coreScale = telegraph.Scale * (0.5f + progress * 0.5f);
                float coreAlpha = 0.4f + progress * 0.3f;
                
                Main.spriteBatch.Draw(glowTex, drawPos, null, telegraph.Color * coreAlpha, 0f,
                    glowTex.Size() / 2f, coreScale, SpriteEffects.None, 0f);
                
                // Pulsing outer ring
                float ringScale = telegraph.Scale * 1.5f * (1f + (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.2f);
                Main.spriteBatch.Draw(glowTex, drawPos, null, telegraph.Color * 0.2f, 0f,
                    glowTex.Size() / 2f, ringScale, SpriteEffects.None, 0f);
            }
        }
        
        private void RenderDeaths()
        {
            Texture2D glowTex = MagnumTextureRegistry.GetBloom();
            
            foreach (var death in _activeDeaths)
            {
                float progress = (float)death.Timer / death.MaxDuration;
                Vector2 drawPos = death.Position - Main.screenPosition;
                var style = UniqueWeaponVFXStyles.GetStyle(death.Theme);
                
                // Phase-based rendering
                float intensity = death.Phase switch
                {
                    0 => progress * 2.5f,        // Building
                    1 => 2.5f - (progress - 0.4f) * 2f, // Climax fade
                    _ => (1f - progress) * 0.5f  // Fadeout
                };
                
                // Multi-layer death glow
                for (int layer = 0; layer < 5; layer++)
                {
                    float scale = (2f + layer) * intensity * death.Intensity;
                    float alpha = 0.2f / (layer + 1);
                    Color color = Color.Lerp(style.Fog.PrimaryColor, style.Fog.SecondaryColor, layer / 5f);
                    
                    Main.spriteBatch.Draw(glowTex, drawPos, null, color * alpha, Main.GameUpdateCount * 0.01f * (layer + 1),
                        glowTex.Size() / 2f, scale, SpriteEffects.None, 0f);
                }
            }
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Creates a persistent aura around a boss/enemy NPC.
        /// </summary>
        public static void CreateBossAura(int npcIndex, string theme, float intensity = 1f)
        {
            if (_activeAuras.Count >= MaxAuras)
                _activeAuras.RemoveAt(0);
            
            // Check if aura already exists for this NPC
            foreach (var existing in _activeAuras)
            {
                if (existing.NPCIndex == npcIndex)
                    return;
            }
            
            var style = UniqueWeaponVFXStyles.GetStyle(theme);
            var auraType = GetAuraTypeForTheme(theme);
            
            var aura = new BossAura
            {
                NPCIndex = npcIndex,
                Theme = theme,
                Intensity = intensity,
                Rotation = 0f,
                PulseTimer = 0f,
                PrimaryColor = style.Fog.PrimaryColor,
                SecondaryColor = style.Fog.SecondaryColor,
                Type = auraType,
                Orbiters = GenerateOrbiters(auraType, 5)
            };
            
            _activeAuras.Add(aura);
        }
        
        /// <summary>
        /// Spawns an attack telegraph effect.
        /// </summary>
        public static void SpawnAttackTelegraph(Vector2 position, string theme, TelegraphType type, 
            int duration = 60, float radius = 100f, Vector2? direction = null)
        {
            if (_activeTelegraphs.Count >= MaxTelegraphs)
                _activeTelegraphs.RemoveAt(0);
            
            var style = UniqueWeaponVFXStyles.GetStyle(theme);
            
            var telegraph = new AttackTelegraph
            {
                Position = position,
                PreviousPosition = position,
                Timer = 0,
                MaxDuration = duration,
                Scale = 1f,
                Color = style.Fog.PrimaryColor,
                Type = type,
                Direction = direction ?? Vector2.UnitX,
                Radius = radius,
                Elements = new List<TelegraphElement>(),
                Theme = theme
            };
            
            _activeTelegraphs.Add(telegraph);
        }
        
        /// <summary>
        /// Spawns a dramatic death spectacle for a boss/enemy.
        /// </summary>
        public static void SpawnDeathSpectacle(Vector2 position, string theme, float intensity = 1f, int duration = 180)
        {
            if (_activeDeaths.Count >= MaxDeaths)
                _activeDeaths.RemoveAt(0);
            
            var death = new DeathSpectacle
            {
                Position = position,
                Timer = 0,
                MaxDuration = duration,
                Theme = theme,
                Intensity = intensity,
                Particles = new List<DeathParticle>(),
                Phase = 0
            };
            
            _activeDeaths.Add(death);
        }
        
        /// <summary>
        /// Removes the aura for a specific NPC.
        /// </summary>
        public static void RemoveBossAura(int npcIndex)
        {
            _activeAuras.RemoveAll(a => a.NPCIndex == npcIndex);
        }
        
        #endregion
        
        #region Helper Methods
        
        private static AuraType GetAuraTypeForTheme(string theme)
        {
            return theme.ToLower() switch
            {
                "fate" => AuraType.CosmicNebula,
                "lacampanella" => AuraType.InfernalFlame,
                "swanlake" => AuraType.PrismaticBallet,
                "enigmavariations" or "enigma" => AuraType.VoidMystery,
                "eroica" => AuraType.HeroicGlow,
                "moonlightsonata" => AuraType.LunarMist,
                "clairdelune" => AuraType.CelestialLight,
                "spring" => AuraType.SpringBloom,
                "summer" => AuraType.SummerHeat,
                "autumn" => AuraType.AutumnLeaves,
                "winter" => AuraType.WinterFrost,
                _ => AuraType.HeroicGlow
            };
        }
        
        private static List<OrbitingElement> GenerateOrbiters(AuraType type, int count)
        {
            var orbiters = new List<OrbitingElement>();
            
            for (int i = 0; i < count; i++)
            {
                orbiters.Add(new OrbitingElement
                {
                    Angle = MathHelper.TwoPi * i / count,
                    Radius = 50f + Main.rand.NextFloat(30f),
                    Speed = 0.5f + Main.rand.NextFloat(1f),
                    Size = 0.5f + Main.rand.NextFloat(0.5f),
                    Type = GetOrbiterTypeForAura(type, i)
                });
            }
            
            return orbiters;
        }
        
        private static OrbiterType GetOrbiterTypeForAura(AuraType aura, int index)
        {
            return aura switch
            {
                AuraType.CosmicNebula => index % 2 == 0 ? OrbiterType.Star : OrbiterType.Glyph,
                AuraType.InfernalFlame => OrbiterType.Flame,
                AuraType.PrismaticBallet => index % 2 == 0 ? OrbiterType.Feather : OrbiterType.Crystal,
                AuraType.VoidMystery => index % 2 == 0 ? OrbiterType.Eye : OrbiterType.Glyph,
                AuraType.HeroicGlow => index % 3 == 0 ? OrbiterType.MusicNote : OrbiterType.Flame,
                AuraType.LunarMist => OrbiterType.Crystal,
                _ => OrbiterType.Star
            };
        }
        
        #endregion
    }
    
    /// <summary>
    /// Screen shake player component.
    /// </summary>
    public class MagnumScreenShakePlayer : ModPlayer
    {
        private float _shakeIntensity;
        private int _shakeDuration;
        private int _shakeTimer;
        
        public void AddShake(float intensity, int duration)
        {
            if (intensity > _shakeIntensity)
            {
                _shakeIntensity = intensity;
                _shakeDuration = duration;
                _shakeTimer = 0;
            }
        }
        
        public override void ModifyScreenPosition()
        {
            if (_shakeTimer < _shakeDuration && _shakeIntensity > 0)
            {
                float progress = (float)_shakeTimer / _shakeDuration;
                float currentIntensity = _shakeIntensity * (1f - progress);
                
                Main.screenPosition += Main.rand.NextVector2Circular(currentIntensity, currentIntensity);
                _shakeTimer++;
            }
            else
            {
                _shakeIntensity = 0;
            }
        }
    }
}
