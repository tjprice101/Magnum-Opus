using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// Attribute to mark projectiles that should KEEP their large hitboxes.
    /// Use this for intentional AoE projectiles like shockwaves and explosions.
    /// Projectiles WITHOUT this attribute will be capped at 48x48 (3 blocks).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AllowLargeHitboxAttribute : Attribute
    {
        public string Reason { get; }
        
        public AllowLargeHitboxAttribute(string reason = "Intentional AoE projectile")
        {
            Reason = reason;
        }
    }

    /// <summary>
    /// PROJECTILE SIZE ENFORCER
    /// 
    /// Enforces a maximum hitbox size of 48x48 pixels (3 blocks) for all MagnumOpus projectiles
    /// UNLESS they have the [AllowLargeHitbox] attribute.
    /// 
    /// This follows the Fate VFX design pattern:
    /// - Small hitboxes (16x16 to 48x48) for accurate hit detection
    /// - Rich VFX particles to create the visual "size" impression
    /// - Smooth, interpolated rendering
    /// 
    /// Large visual projectiles should use:
    /// - Small hitbox (Projectile.width/height = 24-48)
    /// - PreDraw with additive bloom layers for visual size
    /// - Particle trails for impact impression
    /// </summary>
    public class ProjectileSizeEnforcer : GlobalProjectile
    {
        // Maximum allowed hitbox size (3 blocks × 16 pixels = 48 pixels)
        public const int MaxHitboxSize = 48;
        
        // Minimum hitbox size for very small projectiles
        public const int MinHitboxSize = 8;

        public override bool InstancePerEntity => false;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            // MASTER TOGGLE: When disabled, this global system does nothing
            if (!VFXMasterToggle.GlobalSystemsEnabled)
                return false;
            
            return entity.ModProjectile?.Mod == ModContent.GetInstance<MagnumOpus>();
        }

        public override void SetDefaults(Projectile projectile)
        {
            if (projectile.ModProjectile?.Mod != ModContent.GetInstance<MagnumOpus>())
                return;

            // Check if this projectile has the AllowLargeHitbox attribute
            var projType = projectile.ModProjectile.GetType();
            bool allowsLargeHitbox = Attribute.IsDefined(projType, typeof(AllowLargeHitboxAttribute));
            
            if (allowsLargeHitbox)
                return; // Don't modify AoE projectiles

            // Store original size for VFX scaling reference
            int originalWidth = projectile.width;
            int originalHeight = projectile.height;

            // Enforce maximum size
            bool wasResized = false;
            if (projectile.width > MaxHitboxSize)
            {
                projectile.width = MaxHitboxSize;
                wasResized = true;
            }
            if (projectile.height > MaxHitboxSize)
            {
                projectile.height = MaxHitboxSize;
                wasResized = true;
            }

            // Log resize for debugging (only in debug mode)
            #if DEBUG
            if (wasResized)
            {
                string projName = projType.Name;
                Main.NewText($"[VFX] Resized {projName}: {originalWidth}x{originalHeight} → {projectile.width}x{projectile.height}", 
                    new Color(255, 200, 100));
            }
            #endif
        }

        /// <summary>
        /// Helper method to check if a projectile was intended to be large
        /// </summary>
        public static bool IsIntentionallyLarge(Projectile projectile)
        {
            if (projectile.ModProjectile == null)
                return false;
                
            return Attribute.IsDefined(projectile.ModProjectile.GetType(), typeof(AllowLargeHitboxAttribute));
        }

        /// <summary>
        /// Get the visual scale factor for VFX based on original intended size
        /// Projectiles that were resized can use this to scale their VFX appropriately
        /// </summary>
        public static float GetVisualScaleFactor(Projectile projectile, int originalWidth, int originalHeight)
        {
            float widthScale = (float)originalWidth / projectile.width;
            float heightScale = (float)originalHeight / projectile.height;
            return Math.Max(widthScale, heightScale);
        }
    }
}
