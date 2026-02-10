using Microsoft.Xna.Framework;
using MagnumOpus.Common.Systems.Particles;
using System;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// Central integration hub for all MagnumOpus VFX systems.
    /// 
    /// This class provides easy-to-use wrapper methods that combine multiple VFX systems
    /// for common effects across bosses, weapons, and projectiles.
    /// 
    /// Systems Integrated:
    /// - FluidBossMovement: Smooth acceleration-based movement
    /// - TelegraphSystem: Visual attack warnings
    /// - DynamicSkyboxSystem: Full-screen atmospheric effects
    /// - BezierProjectileSystem: Curved projectile paths
    /// - RainbowGradientSystem: Theme-specific color cycling
    /// </summary>
    public static class VFXIntegration
    {
        #region Boss Attack Patterns
        
        /// <summary>
        /// Complete VFX for a charged radial burst attack (Hero's Judgment style).
        /// Handles telegraph, skybox effects, and release VFX.
        /// </summary>
        /// <param name="bossCenter">Boss position</param>
        /// <param name="chargeProgress">0-1 progress of charge</param>
        /// <param name="theme">Theme name for skybox (e.g., "Eroica")</param>
        /// <param name="color">Primary theme color</param>
        /// <param name="maxRadius">Converging ring max radius</param>
        public static void ChargedBurstTelegraph(Vector2 bossCenter, float chargeProgress, string theme, Color color, float maxRadius = 200f)
        {
            // Telegraph: Converging ring
            if (chargeProgress < 1f && chargeProgress > 0f)
            {
                int remainingFrames = (int)((1f - chargeProgress) * 60);
                if (remainingFrames > 0)
                {
                    TelegraphSystem.ConvergingRing(bossCenter, maxRadius * (1f - chargeProgress), remainingFrames, color);
                }
            }
            
            // Skybox: Intensify based on charge
            float intensity = chargeProgress * 0.6f;
            SetThemeSkybox(theme, intensity);
            
            // Final flash on release
            if (chargeProgress >= 1f)
            {
                DynamicSkyboxSystem.TriggerFlash(GetThemeColor(theme), 1f);
            }
        }
        
        /// <summary>
        /// Complete VFX for a dash attack with warning line.
        /// </summary>
        /// <param name="startPos">Dash start position</param>
        /// <param name="direction">Dash direction (normalized)</param>
        /// <param name="length">Dash length</param>
        /// <param name="warningFrames">How many frames to show warning</param>
        /// <param name="color">Warning line color</param>
        public static void DashAttackTelegraph(Vector2 startPos, Vector2 direction, float length, int warningFrames, Color color)
        {
            TelegraphSystem.ThreatLine(startPos, direction.SafeNormalize(Vector2.UnitX), length, warningFrames, color, 1f);
        }
        
        /// <summary>
        /// Shows safe zone around player for radial burst attacks.
        /// </summary>
        /// <param name="playerCenter">Player position</param>
        /// <param name="safeRadius">Radius of safe zone</param>
        /// <param name="duration">Duration in frames</param>
        public static void ShowSafeZone(Vector2 playerCenter, float safeRadius, int duration)
        {
            TelegraphSystem.SafeZone(playerCenter, safeRadius, duration);
        }
        
        /// <summary>
        /// Shows laser beam path warning.
        /// </summary>
        /// <param name="start">Beam origin</param>
        /// <param name="end">Beam endpoint</param>
        /// <param name="width">Beam width</param>
        /// <param name="duration">Warning duration</param>
        /// <param name="color">Beam color</param>
        public static void LaserBeamWarning(Vector2 start, Vector2 end, float width, int duration, Color color)
        {
            TelegraphSystem.LaserPath(start, end, width, duration, color);
        }
        
        /// <summary>
        /// Shows ground impact warning for dive attacks.
        /// </summary>
        /// <param name="impactPos">Impact position</param>
        /// <param name="radius">Impact radius</param>
        /// <param name="progress">Warning progress 0-1</param>
        /// <param name="theme">Theme name for color</param>
        public static void DiveImpactWarning(Vector2 impactPos, float radius, float progress, string theme = null)
        {
            Color color = theme != null ? GetThemeColor(theme, progress) : Color.Yellow;
            int duration = (int)(20 * (1f - progress) + 5);
            // Use danger zone for the area warning (signature: center, radius, duration, color?)
            TelegraphSystem.DangerZone(impactPos, radius * (1f + progress * 0.5f), duration, color * progress);
            // Add impact point indicator
            TelegraphSystem.ImpactPoint(impactPos, radius * 0.3f, 5);
            // Add particles for visual flair
            if (Main.rand.NextBool(3))
            {
                CustomParticles.GenericFlare(impactPos + Main.rand.NextVector2Circular(radius * 0.5f, radius * 0.5f), color, 0.3f * progress, 8);
            }
        }
        
        /// <summary>
        /// Overload for simple integer duration.
        /// </summary>
        public static void DiveImpactWarning(Vector2 impactPos, float radius, int duration)
        {
            TelegraphSystem.ImpactPoint(impactPos, radius, duration);
        }
        
        /// <summary>
        /// Shows sweeping attack cone warning.
        /// </summary>
        /// <param name="origin">Sweep origin</param>
        /// <param name="direction">Sweep center direction</param>
        /// <param name="halfAngle">Half-angle of cone in radians</param>
        /// <param name="length">Sweep length</param>
        /// <param name="duration">Warning duration</param>
        /// <param name="color">Cone color</param>
        public static void SweepAttackWarning(Vector2 origin, Vector2 direction, float halfAngle, float length, int duration, Color color)
        {
            TelegraphSystem.SectorCone(origin, direction, halfAngle, length, duration, color);
        }
        
        /// <summary>
        /// Shows circular danger zone.
        /// </summary>
        public static void DangerZoneWarning(Vector2 center, float radius, int duration, Color color)
        {
            TelegraphSystem.DangerZone(center, radius, duration, color);
        }
        
        #endregion
        
        #region Boss Movement Integration
        
        /// <summary>
        /// Applies fluid movement toward a target position.
        /// Use this instead of direct velocity setting for smooth boss movement.
        /// </summary>
        /// <param name="npc">The boss NPC</param>
        /// <param name="targetPos">Target position</param>
        /// <param name="maxSpeed">Maximum movement speed</param>
        /// <param name="acceleration">How quickly to accelerate (0.1 = slow, 0.5 = fast)</param>
        /// <param name="drag">How quickly to slow down when near target</param>
        public static void FluidMoveToward(NPC npc, Vector2 targetPos, float maxSpeed, float acceleration = 0.15f, float drag = 0.02f)
        {
            FluidBossMovement.MoveToward(npc, targetPos, maxSpeed, acceleration, drag);
        }
        
        /// <summary>
        /// Applies fluid orbit movement around a target.
        /// Perfect for circling attacks.
        /// </summary>
        /// <param name="npc">The boss NPC</param>
        /// <param name="orbitCenter">Center to orbit around</param>
        /// <param name="orbitRadius">Desired orbit radius</param>
        /// <param name="orbitSpeed">Rotation speed (radians per frame)</param>
        /// <param name="acceleration">Movement smoothness</param>
        public static void FluidOrbitAround(NPC npc, Vector2 orbitCenter, float orbitRadius, float orbitSpeed, float acceleration = 0.1f)
        {
            FluidBossMovement.OrbitAround(npc, orbitCenter, orbitRadius, orbitSpeed, acceleration);
        }
        
        /// <summary>
        /// Performs a smooth hover above the target.
        /// </summary>
        /// <param name="npc">The boss NPC</param>
        /// <param name="targetPos">Target to hover above</param>
        /// <param name="hoverHeight">Height above target</param>
        /// <param name="maxSpeed">Maximum movement speed</param>
        /// <param name="acceleration">Movement smoothness</param>
        public static void FluidHoverAbove(NPC npc, Vector2 targetPos, float hoverHeight, float maxSpeed, float acceleration = 0.12f)
        {
            Vector2 hoverPos = targetPos + new Vector2(0, -hoverHeight);
            FluidBossMovement.MoveToward(npc, hoverPos, maxSpeed, acceleration, 0.02f);
        }
        
        /// <summary>
        /// Smoothly updates boss velocity with fluid physics.
        /// </summary>
        public static Vector2 FluidVelocityUpdate(Vector2 currentVelocity, Vector2 targetVelocity, float acceleration, float drag)
        {
            return FluidBossMovement.UpdateMovement(currentVelocity, targetVelocity, acceleration, drag);
        }
        
        #endregion
        
        #region Projectile Path Integration
        
        /// <summary>
        /// Gets a curved homing arc path from start to target.
        /// </summary>
        /// <param name="start">Projectile spawn position</param>
        /// <param name="target">Target position</param>
        /// <param name="arcHeight">Arc height multiplier (0.5 = standard)</param>
        /// <param name="curveDir">-1 left, 0 up, 1 right</param>
        /// <returns>Array of 4 control points for cubic Bézier</returns>
        public static Vector2[] GetHomingArcPath(Vector2 start, Vector2 target, float arcHeight = 0.5f, float curveDir = 0f)
        {
            return BezierProjectileSystem.GenerateHomingArc(start, target, arcHeight, curveDir);
        }
        
        /// <summary>
        /// Gets a snaking S-curve path.
        /// </summary>
        public static Vector2[] GetSnakingPath(Vector2 start, Vector2 target, float amplitude = 100f)
        {
            return BezierProjectileSystem.GenerateSnakingPath(start, target, amplitude);
        }
        
        /// <summary>
        /// Gets a spiral approach path.
        /// </summary>
        public static Vector2[] GetSpiralPath(Vector2 start, Vector2 target, float radius = 150f, bool clockwise = true)
        {
            return BezierProjectileSystem.GenerateSpiralApproach(start, target, radius, clockwise);
        }
        
        /// <summary>
        /// Evaluates a cubic Bézier path at time t.
        /// </summary>
        public static Vector2 EvaluatePath(Vector2[] path, float t)
        {
            if (path == null || path.Length < 4) return Vector2.Zero;
            return BezierProjectileSystem.CubicBezier(path[0], path[1], path[2], path[3], t);
        }
        
        /// <summary>
        /// Gets the tangent (facing direction) at time t on the path.
        /// </summary>
        public static Vector2 GetPathTangent(Vector2[] path, float t)
        {
            if (path == null || path.Length < 4) return Vector2.UnitX;
            return BezierProjectileSystem.CubicBezierTangent(path[0], path[1], path[2], path[3], t);
        }
        
        #endregion
        
        #region Theme Color Integration
        
        /// <summary>
        /// Gets the current cycling color for a theme.
        /// </summary>
        /// <param name="theme">Theme name</param>
        /// <param name="offset">Time offset for variation</param>
        public static Color GetThemeColor(string theme, float offset = 0f)
        {
            return theme?.ToLower() switch
            {
                "eroica" => RainbowGradientSystem.GetEroicaFlame(offset),
                "lacampanella" or "campanella" => RainbowGradientSystem.GetCampanellaInferno(offset),
                "swanlake" or "swan" => RainbowGradientSystem.GetSwanLakeShimmer(offset),
                "moonlight" or "moonlightsonata" => RainbowGradientSystem.GetMoonlightGlow(offset),
                "enigma" or "enigmavariations" => RainbowGradientSystem.GetEnigmaVoid(offset),
                "fate" => RainbowGradientSystem.GetFateCosmic(offset),
                "rainbow" => RainbowGradientSystem.GetRainbowColor(offset),
                _ => RainbowGradientSystem.GetRainbowColor(offset)
            };
        }
        
        /// <summary>
        /// Gets rainbow color with full parameter control.
        /// </summary>
        public static Color GetRainbow(float offset = 0f, float speed = 1f, float saturation = 1f, float luminosity = 0.65f)
        {
            return RainbowGradientSystem.GetRainbowColor(offset, speed, saturation, luminosity);
        }
        
        /// <summary>
        /// Gets rainbow constrained to a specific hue range.
        /// </summary>
        public static Color GetConstrainedRainbow(float minHue, float maxHue, float offset = 0f, float speed = 1f)
        {
            return RainbowGradientSystem.GetConstrainedRainbow(minHue, maxHue, offset, speed);
        }
        
        #endregion
        
        #region Skybox Integration
        
        /// <summary>
        /// Sets the dynamic skybox effect for a theme.
        /// </summary>
        /// <param name="theme">Theme name</param>
        /// <param name="intensity">Effect intensity 0-1</param>
        public static void SetThemeSkybox(string theme, float intensity = 0.5f)
        {
            DynamicSkyboxSystem.SkyboxEffect effect = theme?.ToLower() switch
            {
                "eroica" => DynamicSkyboxSystem.SkyboxEffect.EroicaHeroic,
                "lacampanella" or "campanella" => DynamicSkyboxSystem.SkyboxEffect.LaCampanellaInferno,
                "swanlake" or "swan" => DynamicSkyboxSystem.SkyboxEffect.SwanLakeMonochrome,
                "moonlight" or "moonlightsonata" => DynamicSkyboxSystem.SkyboxEffect.MoonlightLunar,
                "enigma" or "enigmavariations" => DynamicSkyboxSystem.SkyboxEffect.EnigmaVoid,
                "fate" => DynamicSkyboxSystem.SkyboxEffect.FateCosmic,
                "diesirae" or "dies" => DynamicSkyboxSystem.SkyboxEffect.DiesIraeWrath,
                "clairdelune" or "clair" => DynamicSkyboxSystem.SkyboxEffect.ClairDeLuneDream,
                _ => DynamicSkyboxSystem.SkyboxEffect.None
            };
            
            DynamicSkyboxSystem.ActivateEffect(effect, intensity);
        }
        
        /// <summary>
        /// Activates a specific skybox effect.
        /// </summary>
        public static void ActivateSkybox(DynamicSkyboxSystem.SkyboxEffect effect, float intensity = 0.5f)
        {
            DynamicSkyboxSystem.ActivateEffect(effect, intensity);
        }
        
        /// <summary>
        /// Deactivates all skybox effects.
        /// </summary>
        public static void DeactivateSkybox()
        {
            DynamicSkyboxSystem.DeactivateEffect();
        }
        
        /// <summary>
        /// Triggers a screen flash.
        /// </summary>
        public static void ScreenFlash(float intensity, Color? color = null)
        {
            Color flashColor = color ?? Color.White;
            DynamicSkyboxSystem.TriggerFlash(flashColor, intensity);
        }
        
        /// <summary>
        /// Sets chromatic aberration intensity.
        /// </summary>
        public static void SetChromaticAberration(float intensity)
        {
            DynamicSkyboxSystem.SetChromaticAberration(intensity);
        }
        
        /// <summary>
        /// Sets vignette intensity.
        /// </summary>
        public static void SetVignette(float intensity)
        {
            DynamicSkyboxSystem.SetVignette(intensity);
        }
        
        #endregion
        
        #region Combined Boss Phase Effects
        
        /// <summary>
        /// Call this when a boss enters combat.
        /// Sets up theme-appropriate atmospheric effects.
        /// </summary>
        public static void OnBossSpawn(string theme, Vector2 bossCenter = default)
        {
            SetThemeSkybox(theme, 0.3f);
            // Optional spawn VFX at boss center
            if (bossCenter != default)
            {
                ScreenFlash(0.3f, GetThemeColor(theme));
            }
        }
        
        /// <summary>
        /// Call this during boss phase transitions.
        /// Creates dramatic atmospheric shift.
        /// </summary>
        public static void OnPhaseTransition(string theme, Vector2 bossCenter)
        {
            // Flash
            ScreenFlash(1f, GetThemeColor(theme));
            
            // Intensify skybox
            SetThemeSkybox(theme, 0.6f);
            
            // Brief chromatic aberration
            SetChromaticAberration(0.3f);
        }
        
        /// <summary>
        /// Call this during boss enrage.
        /// </summary>
        public static void OnBossEnrage(string theme, Vector2 bossCenter = default)
        {
            SetThemeSkybox(theme, 0.9f);
            SetChromaticAberration(0.4f);
            SetVignette(0.6f);
            // Optional enrage flash at boss center
            if (bossCenter != default)
            {
                ScreenFlash(0.6f, GetThemeColor(theme));
            }
        }
        
        /// <summary>
        /// Call this when boss is defeated.
        /// Enhanced with advanced VFX systems.
        /// </summary>
        public static void OnBossDeath(string theme, Vector2 bossCenter)
        {
            // Massive flash
            ScreenFlash(1.5f, Color.White);
            
            // Fade out skybox
            DeactivateSkybox();
            
            // NEW: Advanced death explosion using new systems
            AdvancedVFXIntegration.BossDeathExplosion(theme, bossCenter, 2f);
            
            // NEW: Trigger theme-appropriate screen distortion
            ScreenDistortionManager.TriggerThemeEffect(theme, bossCenter, 0.8f, 60);
        }
        
        /// <summary>
        /// Call this when boss despawns (player died, etc.).
        /// </summary>
        public static void OnBossDespawn()
        {
            DeactivateSkybox();
            SetChromaticAberration(0f);
            SetVignette(0f);
        }
        
        #endregion
        
        #region Attack Release Effects
        
        /// <summary>
        /// Creates a dramatic attack release effect combining multiple systems.
        /// Call when a charged attack fires.
        /// Enhanced with screen distortion effects.
        /// </summary>
        public static void AttackRelease(string theme, Vector2 position, float intensity = 1f)
        {
            // Screen flash
            ScreenFlash(intensity * 0.5f, GetThemeColor(theme));
            
            // Brief chromatic pulse
            SetChromaticAberration(intensity * 0.2f);
            
            // NEW: Advanced attack release VFX
            AdvancedVFXIntegration.BossAttackRelease(theme, position, intensity);
        }
        
        /// <summary>
        /// Creates a dramatic ultimate attack effect.
        /// Enhanced with advanced screen distortions.
        /// </summary>
        public static void UltimateAttackRelease(string theme, Vector2 position, float intensity = 1f)
        {
            // Massive flash
            ScreenFlash(intensity, GetThemeColor(theme));
            
            // Strong chromatic aberration
            SetChromaticAberration(0.5f * intensity);
            
            // Vignette
            SetVignette(0.4f * intensity);
            
            // Max skybox intensity
            SetThemeSkybox(theme, 1f);
            
            // NEW: Advanced screen distortion for ultimate attacks
            ScreenDistortionManager.TriggerThemeEffect(theme, position, intensity * 0.7f, 40);
            
            // NEW: Music note explosion for musical themes
            AdvancedVFXIntegration.BossAttackRelease(theme, position, intensity * 1.3f);
        }
        
        #endregion
        
        #region Advanced Trail Integration
        
        /// <summary>
        /// Creates an advanced theme-styled trail for boss dashes.
        /// Returns trail ID for later updates.
        /// </summary>
        public static int CreateBossDashTrail(string theme, Vector2 startPos, float width = 30f)
        {
            return AdvancedVFXIntegration.BossDashStart(theme, startPos, width);
        }
        
        /// <summary>
        /// Updates a boss dash trail position.
        /// </summary>
        public static void UpdateBossDashTrail(int trailId, Vector2 position, float rotation)
        {
            AdvancedVFXIntegration.BossDashUpdate(trailId, position, rotation);
        }
        
        /// <summary>
        /// Ends a boss dash trail with impact effect.
        /// </summary>
        public static void EndBossDashTrail(string theme, int trailId, Vector2 endPosition, float scale = 1f)
        {
            AdvancedVFXIntegration.BossDashEnd(theme, trailId, endPosition, scale);
        }
        
        /// <summary>
        /// Enhanced phase transition with advanced screen effects.
        /// </summary>
        public static void OnPhaseTransitionEnhanced(string theme, Vector2 bossCenter, float scale = 1f)
        {
            // Call standard phase transition
            OnPhaseTransition(theme, bossCenter);
            
            // Add advanced VFX
            AdvancedVFXIntegration.BossPhaseTransition(theme, bossCenter, scale);
        }
        
        /// <summary>
        /// Enhanced attack windup with particle effects.
        /// </summary>
        public static void AttackWindup(string theme, Vector2 position, float progress, float scale = 1f)
        {
            // Standard telegraph
            ChargedBurstTelegraph(position, progress, theme, GetThemeColor(theme));
            
            // Advanced particle buildup
            AdvancedVFXIntegration.BossAttackWindup(theme, position, progress, scale);
        }        
        #endregion
    }
}