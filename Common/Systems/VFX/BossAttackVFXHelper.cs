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
    /// BOSS ATTACK VFX HELPER
    /// 
    /// Provides specialized VFX methods for UNIQUE boss attack patterns:
    /// 
    /// ATTACK TYPES:
    /// 1. RADIAL BURSTS - Hero's Judgment style with safe arcs
    /// 2. DASH ATTACKS - Afterimage trails, warning lines
    /// 3. SLAM ATTACKS - Ground impact shockwaves
    /// 4. BEAM ATTACKS - Laser warning and execution
    /// 5. SPIRAL ATTACKS - Rotating projectile patterns
    /// 6. SUMMON ATTACKS - Minion spawn VFX
    /// 7. PHASE TRANSITIONS - Dramatic phase change effects
    /// 
    /// Each boss should call these for consistent, spectacular VFX.
    /// </summary>
    public static class BossAttackVFXHelper
    {
        #region Radial Burst Attacks (Hero's Judgment Style)
        
        /// <summary>
        /// Spawn telegraph for radial burst attack.
        /// Shows converging particles + safe arc indicator.
        /// </summary>
        public static void RadialBurstTelegraph(Vector2 center, string theme, float progress, float radius, float safeAngle, float safeArcWidth)
        {
            var style = UniqueWeaponVFXStyles.GetStyle(theme);
            
            // Converging ring - shrinks as progress increases
            float currentRadius = radius * (1f - progress * 0.5f);
            int particleCount = (int)(8 + progress * 12);
            
            for (int i = 0; i < particleCount; i++)
            {
                float angle = MathHelper.TwoPi * i / particleCount + Main.GameUpdateCount * 0.05f;
                Vector2 pos = center + angle.ToRotationVector2() * currentRadius;
                
                Color color = Color.Lerp(style.Fog.SecondaryColor, Color.White, progress * 0.5f);
                float scale = 0.2f + progress * 0.3f;
                
                CustomParticles.GenericFlare(pos, color, scale, 8);
            }
            
            // Safe arc indicator (cyan)
            if (progress > 0.3f)
            {
                int safeMarkers = 8;
                for (int i = 0; i < safeMarkers; i++)
                {
                    float t = (float)i / (safeMarkers - 1) - 0.5f; // -0.5 to 0.5
                    float markerAngle = safeAngle + t * safeArcWidth;
                    Vector2 markerPos = center + markerAngle.ToRotationVector2() * (radius * 0.8f);
                    
                    CustomParticles.GenericFlare(markerPos, Color.Cyan * 0.7f, 0.25f, 6);
                }
            }
            
            // Building intensity at center
            if (progress > 0.5f)
            {
                float intensity = (progress - 0.5f) * 2f;
                CustomParticles.GenericFlare(center, style.Fog.PrimaryColor, 0.4f + intensity * 0.5f, 10);
            }
        }
        
        /// <summary>
        /// Spawn release VFX for radial burst attack.
        /// Cascading halos + radial flares.
        /// </summary>
        public static void RadialBurstRelease(Vector2 center, string theme, float scale = 1f, int ringCount = 8)
        {
            var style = UniqueWeaponVFXStyles.GetStyle(theme);
            
            // Central flash
            CustomParticles.GenericFlare(center, Color.White, 1.5f * scale, 25);
            CustomParticles.GenericFlare(center, style.Fog.PrimaryColor, 1.2f * scale, 22);
            
            // Cascading halo rings
            for (int ring = 0; ring < ringCount; ring++)
            {
                Color ringColor = Color.Lerp(style.Fog.PrimaryColor, style.Fog.SecondaryColor, (float)ring / ringCount);
                float ringScale = (0.4f + ring * 0.15f) * scale;
                int lifetime = 18 + ring * 3;
                
                CustomParticles.HaloRing(center, ringColor, ringScale, lifetime);
            }
            
            // Radial flare pattern
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 pos = center + angle.ToRotationVector2() * 40f;
                Color color = Color.Lerp(style.Fog.PrimaryColor, style.Fog.SecondaryColor, (float)i / 12f);
                
                CustomParticles.GenericFlare(pos, color, 0.5f * scale, 18);
            }
            
            // Screen shake
            if (Main.LocalPlayer.Distance(center) < 1500f)
            {
                Main.LocalPlayer.GetModPlayer<MagnumScreenShakePlayer>()?.AddShake(12f * scale, 20);
            }
            
            // Fog burst
            WeaponFogVFX.SpawnAttackFog(center, theme, 1.5f * scale, Vector2.Zero);
        }
        
        #endregion
        
        #region Dash Attacks
        
        /// <summary>
        /// Spawn warning line for dash attack.
        /// </summary>
        public static void DashTelegraph(Vector2 start, Vector2 direction, float length, string theme, float progress)
        {
            var style = UniqueWeaponVFXStyles.GetStyle(theme);
            
            // Warning line markers
            int markers = (int)(length / 30f);
            for (int i = 0; i < markers; i++)
            {
                float t = (float)i / markers;
                Vector2 pos = start + direction * (t * length);
                
                Color markerColor = Color.Lerp(Color.Yellow, Color.Red, progress) * (0.4f + progress * 0.4f);
                float scale = 0.2f + (1f - t) * 0.15f;
                
                CustomParticles.GenericFlare(pos, markerColor, scale, 5);
            }
            
            // Converging at start
            if (progress > 0.5f)
            {
                float intensity = (progress - 0.5f) * 2f;
                CustomParticles.GenericFlare(start, style.Fog.PrimaryColor, 0.3f + intensity * 0.4f, 8);
            }
        }
        
        /// <summary>
        /// Spawn afterimage trail during dash.
        /// Call every few frames during dash.
        /// </summary>
        public static void DashAfterimage(Vector2 position, float rotation, string theme, float scale = 1f)
        {
            var style = UniqueWeaponVFXStyles.GetStyle(theme);
            
            // Ghostly afterimage glow
            CustomParticles.GenericFlare(position, style.Fog.PrimaryColor * 0.6f, 0.4f * scale, 15);
            
            // Theme-specific trail
            switch (theme.ToLower())
            {
                case "fate":
                    CustomParticles.Glyph(position, style.Fog.SecondaryColor, 0.3f, -1);
                    break;
                case "swanlake":
                    CustomParticles.SwanFeatherDrift(position, Color.White, 0.4f);
                    break;
                case "eroica":
                    Dust ember = Dust.NewDustPerfect(position, DustID.Enchanted_Gold, Main.rand.NextVector2Circular(2f, 2f), 0, Color.Gold, 0.9f);
                    ember.noGravity = true;
                    break;
            }
        }
        
        /// <summary>
        /// Spawn dash end impact.
        /// </summary>
        public static void DashImpact(Vector2 position, string theme, float scale = 1f)
        {
            var style = UniqueWeaponVFXStyles.GetStyle(theme);
            
            // Impact flash
            CustomParticles.GenericFlare(position, Color.White, 0.8f * scale, 18);
            CustomParticles.GenericFlare(position, style.Fog.PrimaryColor, 0.6f * scale, 15);
            
            // Impact halo
            CustomParticles.HaloRing(position, style.Fog.SecondaryColor, 0.4f * scale, 15);
            
            // Dust burst
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                Dust d = Dust.NewDustPerfect(position, DustID.MagicMirror, vel, 0, style.Fog.PrimaryColor, 1.2f);
                d.noGravity = true;
            }
        }
        
        #endregion
        
        #region Slam/Ground Attacks
        
        /// <summary>
        /// Spawn ground impact warning.
        /// </summary>
        public static void SlamTelegraph(Vector2 impactPoint, float radius, string theme, float progress)
        {
            var style = UniqueWeaponVFXStyles.GetStyle(theme);
            
            // Warning circle
            int markers = 16;
            for (int i = 0; i < markers; i++)
            {
                float angle = MathHelper.TwoPi * i / markers;
                Vector2 pos = impactPoint + angle.ToRotationVector2() * radius;
                
                Color color = Color.Lerp(Color.Yellow, Color.Red, progress) * (0.3f + progress * 0.5f);
                CustomParticles.GenericFlare(pos, color, 0.2f, 5);
            }
            
            // Pulsing center
            if (progress > 0.5f)
            {
                float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.3f) * 0.2f + 0.8f;
                CustomParticles.GenericFlare(impactPoint, Color.Red * pulse, 0.3f + progress * 0.2f, 8);
            }
        }
        
        /// <summary>
        /// Spawn ground slam impact.
        /// </summary>
        public static void SlamImpact(Vector2 position, string theme, float scale = 1f)
        {
            var style = UniqueWeaponVFXStyles.GetStyle(theme);
            
            // Central impact
            CustomParticles.GenericFlare(position, Color.White, 1.2f * scale, 25);
            CustomParticles.GenericFlare(position, style.Fog.PrimaryColor, 1f * scale, 22);
            
            // Ground shockwave rings
            for (int ring = 0; ring < 5; ring++)
            {
                Color ringColor = Color.Lerp(style.Fog.PrimaryColor, style.Fog.SecondaryColor, ring / 5f);
                float ringScale = (0.5f + ring * 0.2f) * scale;
                CustomParticles.HaloRing(position, ringColor, ringScale, 20 + ring * 4);
            }
            
            // Debris particles
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 12f);
                
                Dust d = Dust.NewDustPerfect(position, DustID.Smoke, vel, 100, Color.Gray, 1.5f);
                d.noGravity = false;
            }
            
            // Fog burst
            WeaponFogVFX.SpawnAttackFog(position, theme, 1.5f * scale, Vector2.Zero);
            
            // Screen shake
            if (Main.LocalPlayer.Distance(position) < 1200f)
            {
                Main.LocalPlayer.GetModPlayer<MagnumScreenShakePlayer>()?.AddShake(15f * scale, 25);
            }
        }
        
        #endregion
        
        #region Beam/Laser Attacks
        
        /// <summary>
        /// Spawn laser warning line.
        /// </summary>
        public static void LaserTelegraph(Vector2 start, float angle, float length, string theme, float progress)
        {
            var style = UniqueWeaponVFXStyles.GetStyle(theme);
            Vector2 direction = angle.ToRotationVector2();
            
            // Warning beam
            int segments = (int)(length / 20f);
            for (int i = 0; i < segments; i++)
            {
                float t = (float)i / segments;
                Vector2 pos = start + direction * (t * length);
                
                float intensity = 0.3f + progress * 0.5f;
                float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.2f + t * 10f) * 0.2f + 0.8f;
                
                Color color = Color.Lerp(Color.Red * 0.5f, style.Fog.PrimaryColor, progress) * intensity * pulse;
                CustomParticles.GenericFlare(pos, color, 0.15f, 4);
            }
            
            // Origin buildup
            if (progress > 0.5f)
            {
                float buildupScale = (progress - 0.5f) * 2f;
                CustomParticles.GenericFlare(start, style.Fog.PrimaryColor, 0.4f + buildupScale * 0.4f, 10);
            }
        }
        
        /// <summary>
        /// Spawn laser execution VFX.
        /// Call periodically during beam firing.
        /// </summary>
        public static void LaserExecution(Vector2 start, float angle, float length, string theme, float intensity = 1f)
        {
            var style = UniqueWeaponVFXStyles.GetStyle(theme);
            Vector2 direction = angle.ToRotationVector2();
            
            // Beam particles along length
            int particles = (int)(length / 40f);
            for (int i = 0; i < particles; i++)
            {
                float t = (float)i / particles + Main.rand.NextFloat(-0.05f, 0.05f);
                Vector2 pos = start + direction * (t * length);
                Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
                pos += perpendicular * Main.rand.NextFloat(-8f, 8f);
                
                Color color = Color.Lerp(style.Fog.PrimaryColor, Color.White, Main.rand.NextFloat(0.3f));
                CustomParticles.GenericFlare(pos, color, 0.3f * intensity, 10);
            }
            
            // Origin flare
            CustomParticles.GenericFlare(start, style.Fog.PrimaryColor, 0.5f * intensity, 12);
        }
        
        #endregion
        
        #region Spiral/Pattern Attacks
        
        /// <summary>
        /// Spawn spiral pattern telegraph.
        /// </summary>
        public static void SpiralTelegraph(Vector2 center, int arms, float rotation, string theme, float progress)
        {
            var style = UniqueWeaponVFXStyles.GetStyle(theme);
            
            // Spiral arm markers
            for (int arm = 0; arm < arms; arm++)
            {
                float armAngle = MathHelper.TwoPi * arm / arms + rotation;
                
                for (int point = 0; point < 5; point++)
                {
                    float spiralAngle = armAngle + point * 0.3f;
                    float radius = 30f + point * 20f;
                    Vector2 pos = center + spiralAngle.ToRotationVector2() * radius;
                    
                    float alpha = 0.3f + progress * 0.5f;
                    Color color = Color.Lerp(style.Fog.SecondaryColor, style.Fog.PrimaryColor, (float)point / 5f) * alpha;
                    
                    CustomParticles.GenericFlare(pos, color, 0.2f, 6);
                }
            }
            
            // Center buildup
            if (progress > 0.6f)
            {
                CustomParticles.GenericFlare(center, style.Fog.PrimaryColor, 0.3f + progress * 0.3f, 10);
            }
        }
        
        /// <summary>
        /// Spawn spiral arm release.
        /// </summary>
        public static void SpiralRelease(Vector2 center, float armAngle, string theme, float scale = 1f)
        {
            var style = UniqueWeaponVFXStyles.GetStyle(theme);
            
            // Arm trail
            for (int i = 0; i < 6; i++)
            {
                Vector2 pos = center + armAngle.ToRotationVector2() * (20f + i * 15f);
                Color color = Color.Lerp(style.Fog.PrimaryColor, style.Fog.SecondaryColor, (float)i / 6f);
                
                CustomParticles.GenericFlare(pos, color, 0.35f * scale, 15);
            }
        }
        
        #endregion
        
        #region Summon Attacks
        
        /// <summary>
        /// Spawn minion summon VFX.
        /// </summary>
        public static void SummonTelegraph(Vector2 position, string theme, float progress)
        {
            var style = UniqueWeaponVFXStyles.GetStyle(theme);
            
            // Magic circle forming
            float circleRadius = 40f * progress;
            int glyphCount = 6;
            
            for (int i = 0; i < glyphCount; i++)
            {
                float angle = MathHelper.TwoPi * i / glyphCount + Main.GameUpdateCount * 0.02f;
                Vector2 pos = position + angle.ToRotationVector2() * circleRadius;
                
                CustomParticles.Glyph(pos, style.Fog.SecondaryColor * (0.3f + progress * 0.5f), 0.3f, i % 12);
            }
            
            // Central gathering
            if (progress > 0.5f)
            {
                float intensity = (progress - 0.5f) * 2f;
                CustomParticles.GenericFlare(position, style.Fog.PrimaryColor, 0.3f + intensity * 0.4f, 10);
            }
        }
        
        /// <summary>
        /// Spawn minion appearance VFX.
        /// </summary>
        public static void SummonAppear(Vector2 position, string theme, float scale = 1f)
        {
            var style = UniqueWeaponVFXStyles.GetStyle(theme);
            
            // Appearance flash
            CustomParticles.GenericFlare(position, Color.White, 0.8f * scale, 20);
            CustomParticles.GenericFlare(position, style.Fog.PrimaryColor, 0.6f * scale, 18);
            
            // Glyph burst
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 pos = position + angle.ToRotationVector2() * 30f;
                CustomParticles.Glyph(pos, style.Fog.SecondaryColor, 0.4f * scale, i % 12);
            }
            
            // Halo
            CustomParticles.HaloRing(position, style.Fog.SecondaryColor, 0.5f * scale, 18);
        }
        
        #endregion
        
        #region Phase Transitions
        
        /// <summary>
        /// Spawn phase transition VFX.
        /// </summary>
        public static void PhaseTransition(Vector2 center, string theme, int phase, float progress)
        {
            var style = UniqueWeaponVFXStyles.GetStyle(theme);
            
            // Building energy
            float intensity = progress * (1f + phase * 0.3f);
            
            // Expanding rings
            if (Main.GameUpdateCount % 5 == 0)
            {
                for (int ring = 0; ring < 3 + phase; ring++)
                {
                    float delay = ring * 0.15f;
                    if (progress > delay)
                    {
                        float ringProgress = (progress - delay) / (1f - delay);
                        Color ringColor = Color.Lerp(style.Fog.PrimaryColor, style.Fog.SecondaryColor, ringProgress);
                        float ringScale = 0.3f + ringProgress * 0.5f;
                        CustomParticles.HaloRing(center, ringColor * (1f - ringProgress), ringScale, 15);
                    }
                }
            }
            
            // Converging particles
            int particleCount = (int)(12 + phase * 6);
            float radius = 150f * (1f - progress);
            
            for (int i = 0; i < particleCount; i++)
            {
                if (Main.rand.NextBool(3)) continue;
                
                float angle = MathHelper.TwoPi * i / particleCount + Main.GameUpdateCount * 0.03f;
                Vector2 pos = center + angle.ToRotationVector2() * radius;
                
                Color color = Color.Lerp(style.Fog.SecondaryColor, Color.White, progress);
                CustomParticles.GenericFlare(pos, color * intensity, 0.25f + progress * 0.2f, 12);
            }
            
            // Central buildup
            CustomParticles.GenericFlare(center, style.Fog.PrimaryColor, 0.4f + progress * 0.8f, 15);
            
            // Screen shake building
            if (progress > 0.7f && Main.LocalPlayer.Distance(center) < 1500f)
            {
                float shakeIntensity = (progress - 0.7f) * 3f * (1f + phase * 0.5f);
                Main.LocalPlayer.GetModPlayer<MagnumScreenShakePlayer>()?.AddShake(shakeIntensity * 5f, 5);
            }
        }
        
        /// <summary>
        /// Spawn phase transition climax.
        /// </summary>
        public static void PhaseTransitionClimax(Vector2 center, string theme, int phase)
        {
            var style = UniqueWeaponVFXStyles.GetStyle(theme);
            float scale = 1f + phase * 0.3f;
            
            // Massive central flash
            CustomParticles.GenericFlare(center, Color.White, 2f * scale, 35);
            CustomParticles.GenericFlare(center, style.Fog.PrimaryColor, 1.5f * scale, 30);
            CustomParticles.GenericFlare(center, style.Fog.SecondaryColor, 1.2f * scale, 28);
            
            // Cascading halos
            int ringCount = 10 + phase * 2;
            for (int ring = 0; ring < ringCount; ring++)
            {
                Color ringColor = Color.Lerp(style.Fog.PrimaryColor, style.Fog.SecondaryColor, (float)ring / ringCount);
                float ringScale = (0.5f + ring * 0.2f) * scale;
                int lifetime = 25 + ring * 4;
                CustomParticles.HaloRing(center, ringColor, ringScale, lifetime);
            }
            
            // Radial flare burst
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 pos = center + angle.ToRotationVector2() * 60f;
                Color color = Color.Lerp(style.Fog.PrimaryColor, style.Fog.SecondaryColor, (float)i / 16f);
                CustomParticles.GenericFlare(pos, color, 0.6f * scale, 25);
            }
            
            // Fog explosion
            WeaponFogVFX.SpawnAttackFog(center, theme, 2.5f * scale, Vector2.Zero);
            
            // Light beam burst
            LightBeamImpactVFX.SpawnImpact(center, theme, 2f * scale);
            
            // Major screen shake
            if (Main.LocalPlayer.Distance(center) < 2000f)
            {
                Main.LocalPlayer.GetModPlayer<MagnumScreenShakePlayer>()?.AddShake(20f * scale, 40);
            }
            
            // Sky flash
            DynamicSkyboxSystem.TriggerFlash(style.Fog.PrimaryColor, 0.8f * scale);
        }
        
        #endregion
        
        #region Theme-Specific Attack VFX
        
        /// <summary>
        /// Fate: Cosmic Judgment attack with glyphs and stars.
        /// </summary>
        public static void FateCosmicJudgment(Vector2 center, float progress)
        {
            // Glyph circle forming
            int glyphCount = 12;
            float radius = 80f * (0.5f + progress * 0.5f);
            
            for (int i = 0; i < glyphCount; i++)
            {
                float angle = MathHelper.TwoPi * i / glyphCount - Main.GameUpdateCount * 0.02f;
                Vector2 pos = center + angle.ToRotationVector2() * radius;
                
                CustomParticles.Glyph(pos, new Color(200, 80, 120) * (0.4f + progress * 0.5f), 0.35f, i % 12);
            }
            
            // Star constellation
            if (progress > 0.5f)
            {
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f + Main.GameUpdateCount * 0.01f;
                    Vector2 pos = center + angle.ToRotationVector2() * (radius * 1.4f);
                    CustomParticles.GenericFlare(pos, Color.White, 0.25f, 10);
                }
            }
            
            // Central void
            CustomParticles.GenericFlare(center, new Color(15, 5, 20), 0.4f + progress * 0.3f, 12);
        }
        
        /// <summary>
        /// Swan Lake: Graceful ballet attack with feathers.
        /// </summary>
        public static void SwanLakeGracefulStrike(Vector2 center, Vector2 direction, float progress)
        {
            // Feather trail forming
            for (int i = 0; i < 6; i++)
            {
                float offset = (i - 2.5f) * 20f;
                Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
                Vector2 pos = center + direction * (30f + i * 15f) + perpendicular * offset * progress;
                
                Color color = Main.rand.NextBool() ? Color.White : new Color(20, 20, 30);
                CustomParticles.SwanFeatherDrift(pos, color * (0.4f + progress * 0.4f), 0.35f);
            }
            
            // Prismatic shimmer
            if (progress > 0.5f)
            {
                float hue = (Main.GameUpdateCount * 0.03f) % 1f;
                Color rainbow = Main.hslToRgb(hue, 1f, 0.8f);
                CustomParticles.PrismaticSparkle(center + direction * 50f, rainbow, 0.3f);
            }
        }
        
        /// <summary>
        /// La Campanella: Infernal bell toll attack.
        /// </summary>
        public static void LaCampanellaBellToll(Vector2 center, float progress)
        {
            // Fire circle
            int fireCount = 12;
            float radius = 50f + progress * 30f;
            
            for (int i = 0; i < fireCount; i++)
            {
                float angle = MathHelper.TwoPi * i / fireCount + Main.GameUpdateCount * 0.03f;
                Vector2 pos = center + angle.ToRotationVector2() * radius;
                
                Dust flame = Dust.NewDustPerfect(pos, DustID.Torch, Main.rand.NextVector2Circular(1f, 1f), 0, new Color(255, 140, 40), 1.4f);
                flame.noGravity = true;
            }
            
            // Bell chime effect - expanding ring
            if (Main.GameUpdateCount % 20 == 0)
            {
                CustomParticles.HaloRing(center, new Color(255, 200, 80), 0.5f + progress * 0.3f, 20);
            }
            
            // Smoke buildup
            if (Main.rand.NextBool(3))
            {
                Vector2 smokeVel = Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0, -1f);
                Dust smoke = Dust.NewDustPerfect(center + Main.rand.NextVector2Circular(30f, 30f), DustID.Smoke, smokeVel, 100, Color.Black, 1.5f);
                smoke.noGravity = true;
            }
        }
        
        /// <summary>
        /// Enigma: Void mystery attack with eyes.
        /// </summary>
        public static void EnigmaVoidGaze(Vector2 center, Vector2 lookDirection, float progress)
        {
            // Watching eye positions
            int eyeCount = 5;
            float radius = 60f;
            
            for (int i = 0; i < eyeCount; i++)
            {
                float angle = MathHelper.TwoPi * i / eyeCount + Main.GameUpdateCount * 0.01f;
                Vector2 pos = center + angle.ToRotationVector2() * radius;
                
                // Eyes look toward the direction
                CustomParticles.Glyph(pos, new Color(140, 60, 200) * (0.4f + progress * 0.5f), 0.35f, 8 + (i % 4)); // Eye variants
            }
            
            // Void particles
            if (Main.rand.NextBool(3))
            {
                Vector2 voidPos = center + Main.rand.NextVector2Circular(40f, 40f);
                CustomParticles.GenericFlare(voidPos, new Color(50, 220, 100) * 0.5f, 0.2f, 15);
            }
            
            // Central mystery
            if (progress > 0.7f)
            {
                CustomParticles.Glyph(center, new Color(80, 20, 120), 0.5f, -1);
            }
        }
        
        /// <summary>
        /// Eroica: Heroic valor attack with sakura and gold.
        /// </summary>
        public static void EroicaValorStrike(Vector2 center, Vector2 direction, float progress)
        {
            // Golden trail
            for (int i = 0; i < 5; i++)
            {
                Vector2 pos = center + direction * (20f + i * 25f);
                
                Dust gold = Dust.NewDustPerfect(pos, DustID.Enchanted_Gold, 
                    Main.rand.NextVector2Circular(1f, 1f), 0, Color.Gold, 1.2f);
                gold.noGravity = true;
                
                CustomParticles.GenericFlare(pos, new Color(255, 200, 80) * (0.4f + progress * 0.4f), 0.3f, 12);
            }
            
            // Sakura hints
            if (progress > 0.4f && Main.rand.NextBool(4))
            {
                Vector2 petalPos = center + direction * Main.rand.NextFloat(30f, 80f);
                petalPos += direction.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-20f, 20f);
                
                // Sakura pink dust
                Dust petal = Dust.NewDustPerfect(petalPos, DustID.PinkTorch, 
                    -direction * 0.5f + new Vector2(0, -0.5f), 100, new Color(255, 150, 180), 0.8f);
                petal.noGravity = true;
                petal.fadeIn = 1.2f;
            }
        }
        
        #endregion
    }
}
