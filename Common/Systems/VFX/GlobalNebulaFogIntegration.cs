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
    /// GLOBAL NEBULA FOG INTEGRATION
    /// 
    /// Automatically applies nebula fog effects to MagnumOpus content:
    /// - Weapons: Fog aura while holding, fog bursts on swing
    /// - Projectiles: Trail fog overlays, impact fog bursts
    /// - Bosses: Ambient constellation fog, attack fog, death fog
    /// 
    /// This system hooks into existing GlobalVFXOverhaul, GlobalWeaponVFXOverhaul,
    /// and GlobalBossVFXOverhaul to add the fog layer automatically.
    /// </summary>
    public class GlobalNebulaFogIntegration : ModSystem
    {
        // Configuration for automatic fog application
        public static bool EnableWeaponFog = true;
        public static bool EnableProjectileFog = true;
        public static bool EnableBossFog = true;
        public static float GlobalFogIntensity = 1f;
        
        private static int _fogSpawnCooldown = 0;
        
        public override void Load()
        {
            // Hook into draw events
            On_Main.DrawProjectiles += DrawProjectileFog;
            On_Main.DrawNPCs += DrawBossFog;
        }
        
        public override void Unload()
        {
            On_Main.DrawProjectiles -= DrawProjectileFog;
            On_Main.DrawNPCs -= DrawBossFog;
        }
        
        public override void PostUpdateEverything()
        {
            if (_fogSpawnCooldown > 0)
                _fogSpawnCooldown--;
        }
        
        #region Projectile Fog Integration
        
        private void DrawProjectileFog(On_Main.orig_DrawProjectiles orig, Main self)
        {
            // Draw original projectiles first
            orig(self);
            
            if (!EnableProjectileFog) return;
            
            // Draw fog effects for all MagnumOpus projectiles
            NebulaFogSystem.DrawAllFogs(Main.spriteBatch);
        }
        
        /// <summary>
        /// Call this from projectile AI to add fog trail effects.
        /// </summary>
        public static void ApplyProjectileFog(Projectile projectile, string theme)
        {
            if (!EnableProjectileFog || GlobalFogIntensity <= 0f) return;
            
            // Attach trail fog overlay
            NebulaFogSystem.AttachTrailFog(projectile.whoAmI, theme, 25f * GlobalFogIntensity, 0.4f);
            
            // Periodic ambient fog puffs
            if (Main.rand.NextBool(8))
            {
                Vector2 offset = Main.rand.NextVector2Circular(15f, 15f);
                var config = NebulaFogSystem.FogCloudConfig.Default;
                config.Scale = 0.4f * GlobalFogIntensity;
                config.Lifetime = 40;
                config.Opacity = 0.3f;
                config.Theme = theme;
                
                var colors = NebulaFogSystem.GetThemeColors(theme);
                config.PrimaryColor = colors.primary;
                config.SecondaryColor = colors.secondary;
                
                NebulaFogSystem.SpawnFogCloud(projectile.Center + offset, config);
            }
        }
        
        /// <summary>
        /// Call this on projectile death for spectacular fog impact.
        /// </summary>
        public static void ApplyProjectileImpactFog(Vector2 position, string theme, float intensity = 1f)
        {
            if (!EnableProjectileFog || GlobalFogIntensity <= 0f) return;
            
            NebulaFogSystem.SpawnImpactFog(position, theme, intensity * GlobalFogIntensity);
        }
        
        #endregion
        
        #region Weapon Fog Integration
        
        /// <summary>
        /// Call this when a weapon is held to add ambient fog aura.
        /// </summary>
        public static void ApplyWeaponHoldFog(Player player, string theme)
        {
            if (!EnableWeaponFog || GlobalFogIntensity <= 0f) return;
            if (_fogSpawnCooldown > 0) return;
            
            _fogSpawnCooldown = 12; // Only spawn fog every 12 frames
            
            // Ambient fog around weapon
            Vector2 weaponPos = player.Center + new Vector2(player.direction * 30f, -5f);
            
            var config = NebulaFogSystem.FogCloudConfig.Default;
            config.Scale = 0.5f * GlobalFogIntensity;
            config.Lifetime = 35;
            config.Opacity = 0.2f;
            config.Style = NebulaFogSystem.FogStyle.Ambient;
            config.Theme = theme;
            
            var colors = NebulaFogSystem.GetThemeColors(theme);
            config.PrimaryColor = colors.primary * 0.8f;
            config.SecondaryColor = colors.secondary * 0.6f;
            
            NebulaFogSystem.SpawnFogCloud(weaponPos + Main.rand.NextVector2Circular(10f, 10f), config);
        }
        
        /// <summary>
        /// Call this on melee weapon swing for fog arc effects.
        /// </summary>
        public static void ApplyMeleeSwingFog(Player player, string theme, float swingProgress, Vector2 swingDirection)
        {
            if (!EnableWeaponFog || GlobalFogIntensity <= 0f) return;
            
            // Only spawn during active swing phase
            if (swingProgress < 0.1f || swingProgress > 0.9f) return;
            
            // Fog puffs along the swing arc
            if (Main.rand.NextBool(3))
            {
                float arcOffset = (swingProgress - 0.5f) * MathHelper.Pi;
                Vector2 arcPos = player.Center + swingDirection.RotatedBy(arcOffset) * 60f;
                
                var config = NebulaFogSystem.FogCloudConfig.Default;
                config.Scale = 0.6f * GlobalFogIntensity;
                config.Lifetime = 30;
                config.Opacity = 0.35f;
                config.Velocity = swingDirection * 2f;
                config.Theme = theme;
                
                var colors = NebulaFogSystem.GetThemeColors(theme);
                config.PrimaryColor = colors.primary;
                config.SecondaryColor = colors.secondary;
                config.SparkleThreshold = 0.85f;
                config.SparkleIntensity = 1.5f;
                
                NebulaFogSystem.SpawnFogCloud(arcPos, config);
            }
        }
        
        /// <summary>
        /// Call this on weapon hit for fog impact burst.
        /// </summary>
        public static void ApplyWeaponHitFog(Vector2 hitPosition, string theme, bool crit = false)
        {
            if (!EnableWeaponFog || GlobalFogIntensity <= 0f) return;
            
            float intensity = crit ? 1.5f : 1f;
            NebulaFogSystem.SpawnFogBurst(hitPosition, theme, crit ? 6 : 4, 0.6f * intensity * GlobalFogIntensity);
            
            // Extra constellation fog on crit
            if (crit)
            {
                NebulaFogSystem.SpawnConstellationFog(hitPosition, theme, 1.2f * GlobalFogIntensity);
            }
        }
        
        #endregion
        
        #region Boss Fog Integration
        
        private void DrawBossFog(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles)
        {
            orig(self, behindTiles);
            
            // Fog for bosses is drawn in the projectile pass to avoid layering issues
        }
        
        /// <summary>
        /// Call this in boss AI for ambient constellation fog aura.
        /// </summary>
        public static void ApplyBossAmbientFog(NPC boss, string theme)
        {
            if (!EnableBossFog || GlobalFogIntensity <= 0f) return;
            
            // Large ambient constellation fog
            if (Main.rand.NextBool(20))
            {
                Vector2 offset = Main.rand.NextVector2Circular(boss.width, boss.height);
                NebulaFogSystem.SpawnConstellationFog(boss.Center + offset, theme, 2f * GlobalFogIntensity);
            }
            
            // Smaller fog puffs
            if (Main.rand.NextBool(8))
            {
                Vector2 offset = Main.rand.NextVector2Circular(boss.width * 0.7f, boss.height * 0.7f);
                NebulaFogSystem.SpawnThemedFog(boss.Center + offset, theme, 0.8f * GlobalFogIntensity, 60);
            }
        }
        
        /// <summary>
        /// Call this when boss enters attack windup for charging fog.
        /// </summary>
        public static void ApplyBossWindupFog(Vector2 position, string theme, float chargeProgress)
        {
            if (!EnableBossFog || GlobalFogIntensity <= 0f) return;
            
            // Converging fog as charge builds
            if (Main.rand.NextBool(4))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float distance = 100f * (1f - chargeProgress * 0.5f);
                Vector2 spawnPos = position + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * distance;
                Vector2 velocity = (position - spawnPos).SafeNormalize(Vector2.Zero) * (2f + chargeProgress * 4f);
                
                var config = NebulaFogSystem.FogCloudConfig.Default;
                config.Scale = (0.5f + chargeProgress * 0.5f) * GlobalFogIntensity;
                config.Lifetime = (int)(40 * (1f - chargeProgress * 0.5f));
                config.Velocity = velocity;
                config.Opacity = 0.4f + chargeProgress * 0.3f;
                config.SparkleThreshold = 0.7f - chargeProgress * 0.2f;
                config.SparkleIntensity = 1f + chargeProgress * 2f;
                config.Theme = theme;
                
                var colors = NebulaFogSystem.GetThemeColors(theme);
                config.PrimaryColor = colors.primary;
                config.SecondaryColor = colors.secondary;
                
                NebulaFogSystem.SpawnFogCloud(spawnPos, config);
            }
        }
        
        /// <summary>
        /// Call this when boss attack releases for explosive fog burst.
        /// </summary>
        public static void ApplyBossAttackReleaseFog(Vector2 position, string theme, float intensity = 1.5f)
        {
            if (!EnableBossFog || GlobalFogIntensity <= 0f) return;
            
            // Large central constellation fog
            NebulaFogSystem.SpawnConstellationFog(position, theme, 2f * intensity * GlobalFogIntensity);
            
            // Radial fog burst
            NebulaFogSystem.SpawnFogBurst(position, theme, 8, 1f * intensity * GlobalFogIntensity);
            
            // Extra sparkle fogs
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 50f;
                
                var config = NebulaFogSystem.FogCloudConfig.Default;
                config.Scale = 0.7f * intensity * GlobalFogIntensity;
                config.Lifetime = 50;
                config.SparkleThreshold = 0.65f;
                config.SparkleIntensity = 2.5f;
                config.Velocity = offset.SafeNormalize(Vector2.Zero) * 3f;
                config.Theme = theme;
                
                var colors = NebulaFogSystem.GetThemeColors(theme);
                config.PrimaryColor = colors.primary;
                config.SecondaryColor = colors.secondary;
                
                NebulaFogSystem.SpawnFogCloud(position + offset * 0.5f, config);
            }
        }
        
        /// <summary>
        /// Call this on boss dash for trailing fog.
        /// </summary>
        public static void ApplyBossDashFog(NPC boss, string theme, Vector2 velocity)
        {
            if (!EnableBossFog || GlobalFogIntensity <= 0f) return;
            
            // Trailing fog puffs
            if (Main.rand.NextBool(2))
            {
                Vector2 offset = Main.rand.NextVector2Circular(boss.width * 0.3f, boss.height * 0.3f);
                
                var config = NebulaFogSystem.FogCloudConfig.Default;
                config.Scale = 1f * GlobalFogIntensity;
                config.Lifetime = 45;
                config.Velocity = -velocity.SafeNormalize(Vector2.Zero) * 2f;
                config.Opacity = 0.5f;
                config.Theme = theme;
                
                var colors = NebulaFogSystem.GetThemeColors(theme);
                config.PrimaryColor = colors.primary;
                config.SecondaryColor = colors.secondary;
                
                NebulaFogSystem.SpawnFogCloud(boss.Center + offset, config);
            }
        }
        
        /// <summary>
        /// Call this on boss phase transition for spectacular fog explosion.
        /// </summary>
        public static void ApplyBossPhaseTransitionFog(Vector2 position, string theme)
        {
            if (!EnableBossFog || GlobalFogIntensity <= 0f) return;
            
            // Massive central constellation fog
            NebulaFogSystem.SpawnConstellationFog(position, theme, 3f * GlobalFogIntensity);
            
            // Ring of fog bursts
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 80f;
                NebulaFogSystem.SpawnConstellationFog(position + offset, theme, 1.2f * GlobalFogIntensity);
            }
            
            // Outward fog burst
            NebulaFogSystem.SpawnFogBurst(position, theme, 16, 1.5f * GlobalFogIntensity);
        }
        
        /// <summary>
        /// Call this on boss death for ultimate fog spectacle.
        /// </summary>
        public static void ApplyBossDeathFog(Vector2 position, string theme)
        {
            if (!EnableBossFog || GlobalFogIntensity <= 0f) return;
            
            // Multiple waves of constellation fog
            for (int wave = 0; wave < 3; wave++)
            {
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f + wave * 0.3f;
                    float distance = 50f + wave * 60f;
                    Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * distance;
                    
                    var config = NebulaFogSystem.FogCloudConfig.Default;
                    config.Scale = (2f - wave * 0.3f) * GlobalFogIntensity;
                    config.Lifetime = 120 + wave * 30;
                    config.Style = NebulaFogSystem.FogStyle.Constellation;
                    config.SparkleThreshold = 0.6f;
                    config.SparkleIntensity = 3f;
                    config.Velocity = offset.SafeNormalize(Vector2.Zero) * (2f + wave);
                    config.Theme = theme;
                    
                    var colors = NebulaFogSystem.GetThemeColors(theme);
                    config.PrimaryColor = colors.primary;
                    config.SecondaryColor = colors.secondary;
                    
                    NebulaFogSystem.SpawnFogCloud(position + offset * 0.3f, config);
                }
            }
            
            // Massive central fog
            NebulaFogSystem.SpawnConstellationFog(position, theme, 4f * GlobalFogIntensity);
        }
        
        #endregion
        
        #region Special Effect Methods
        
        /// <summary>
        /// Creates a vortex fog effect (particles spiral inward).
        /// </summary>
        public static void SpawnVortexFog(Vector2 center, string theme, float radius = 100f, int count = 8)
        {
            if (GlobalFogIntensity <= 0f) return;
            
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.GlobalTimeWrappedHourly;
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                Vector2 velocity = offset.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.Zero) * 3f;
                velocity += (center - (center + offset)).SafeNormalize(Vector2.Zero) * 1.5f;
                
                var config = NebulaFogSystem.FogCloudConfig.Default;
                config.Scale = 0.6f * GlobalFogIntensity;
                config.Lifetime = 60;
                config.Velocity = velocity;
                config.Opacity = 0.4f;
                config.RotationSpeed = 0.05f;
                config.Theme = theme;
                
                var colors = NebulaFogSystem.GetThemeColors(theme);
                config.PrimaryColor = colors.primary;
                config.SecondaryColor = colors.secondary;
                
                NebulaFogSystem.SpawnFogCloud(center + offset, config);
            }
        }
        
        /// <summary>
        /// Creates a rising fog pillar effect.
        /// </summary>
        public static void SpawnFogPillar(Vector2 basePosition, string theme, float height = 200f)
        {
            if (GlobalFogIntensity <= 0f) return;
            
            int fogCount = (int)(height / 30f);
            
            for (int i = 0; i < fogCount; i++)
            {
                float verticalOffset = -i * 30f;
                Vector2 pos = basePosition + new Vector2(Main.rand.NextFloat(-20f, 20f), verticalOffset);
                
                var config = NebulaFogSystem.FogCloudConfig.Default;
                config.Scale = (0.8f - i * 0.05f) * GlobalFogIntensity;
                config.Lifetime = 80 + i * 5;
                config.Velocity = new Vector2(0, -2f);
                config.Opacity = 0.5f - i * 0.03f;
                config.Style = NebulaFogSystem.FogStyle.Constellation;
                config.Theme = theme;
                
                var colors = NebulaFogSystem.GetThemeColors(theme);
                config.PrimaryColor = colors.primary;
                config.SecondaryColor = colors.secondary;
                
                NebulaFogSystem.SpawnFogCloud(pos, config);
            }
        }
        
        /// <summary>
        /// Creates a fog ring expanding outward.
        /// </summary>
        public static void SpawnFogRing(Vector2 center, string theme, float startRadius = 30f, float speed = 4f)
        {
            if (GlobalFogIntensity <= 0f) return;
            
            int fogCount = 16;
            
            for (int i = 0; i < fogCount; i++)
            {
                float angle = MathHelper.TwoPi * i / fogCount;
                Vector2 direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                Vector2 pos = center + direction * startRadius;
                
                var config = NebulaFogSystem.FogCloudConfig.Default;
                config.Scale = 0.7f * GlobalFogIntensity;
                config.Lifetime = 60;
                config.Velocity = direction * speed;
                config.Opacity = 0.5f;
                config.SparkleThreshold = 0.75f;
                config.Theme = theme;
                
                var colors = NebulaFogSystem.GetThemeColors(theme);
                config.PrimaryColor = colors.primary;
                config.SecondaryColor = colors.secondary;
                
                NebulaFogSystem.SpawnFogCloud(pos, config);
            }
        }
        
        #endregion
    }
}
