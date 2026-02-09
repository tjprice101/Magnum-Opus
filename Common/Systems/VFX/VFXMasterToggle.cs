namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// MASTER TOGGLE FOR GLOBAL VFX SYSTEMS
    /// 
    /// When GlobalSystemsEnabled is FALSE (default), ALL global VFX hooks are disabled.
    /// This means NO automatic VFX will be applied to weapons, projectiles, or bosses.
    /// 
    /// Instead, each weapon/projectile/boss should implement its own unique VFX directly
    /// in its .cs file, similar to Calamity's Ark of the Cosmos approach.
    /// 
    /// The VFX utility classes (BloomRenderer, EnhancedTrailRenderer, InterpolatedRenderer, etc.)
    /// remain available as libraries for manual use in individual weapon files.
    /// 
    /// ===== ARCHITECTURE PHILOSOPHY =====
    /// 
    /// BEFORE (Global Systems - DISABLED):
    /// - GlobalVFXOverhaul automatically applied trails to ALL projectiles
    /// - GlobalWeaponVFXOverhaul automatically applied swing effects to ALL weapons
    /// - GlobalBossVFXOverhaul automatically applied effects to ALL bosses
    /// - Result: Generic, cookie-cutter VFX that all looked the same
    /// 
    /// AFTER (Per-Weapon Systems - ENABLED):
    /// - Each weapon has its OWN unique VFX code in its .cs file
    /// - Each projectile has its OWN unique trail/bloom/effects
    /// - Each boss has its OWN unique VFX tailored to its theme
    /// - Result: Every weapon feels unique and memorable
    /// 
    /// ===== HOW TO ADD VFX TO A WEAPON =====
    /// 
    /// In your weapon's .cs file:
    /// 
    /// 1. Override PreDraw/PostDraw for custom rendering
    /// 2. Use BloomRenderer.DrawBloomStack() for glow effects
    /// 3. Use EnhancedTrailRenderer for primitive trails
    /// 4. Store position history for trail rendering
    /// 5. Call VFX methods directly in AI/UseItem/etc.
    /// 
    /// Example weapon structure:
    /// 
    /// public class MyEpicSword : ModItem
    /// {
    ///     // Store trail data
    ///     private List<Vector2> trailPositions = new();
    ///     
    ///     public override void UseItemFrame(Player player)
    ///     {
    ///         // Update trail
    ///         trailPositions.Insert(0, player.Center);
    ///         if (trailPositions.Count > 20) trailPositions.RemoveAt(20);
    ///         
    ///         // Draw custom trail
    ///         // Spawn unique particles
    ///     }
    /// }
    /// </summary>
    public static class VFXMasterToggle
    {
        /// <summary>
        /// When FALSE, all Global VFX systems are DISABLED.
        /// Each weapon must implement its own VFX.
        /// 
        /// Default: FALSE (global systems disabled)
        /// </summary>
        public static bool GlobalSystemsEnabled = false;
        
        /// <summary>
        /// When TRUE, shader-based effects (shaders that modify the entire screen)
        /// are allowed to run. These don't conflict with per-weapon VFX.
        /// 
        /// Default: TRUE (screen shaders allowed)
        /// </summary>
        public static bool ScreenShadersEnabled = true;
        
        /// <summary>
        /// When TRUE, sky effects (custom backgrounds during boss fights) are allowed.
        /// 
        /// Default: TRUE (sky effects allowed)
        /// </summary>
        public static bool SkyEffectsEnabled = true;
        
        /// <summary>
        /// When TRUE, the particle system is allowed to render particles.
        /// Individual weapons still need to spawn particles manually.
        /// 
        /// Default: TRUE (particle rendering allowed)
        /// </summary>
        public static bool ParticleRenderingEnabled = true;
    }
}
