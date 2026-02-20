using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace MagnumOpus.Content.TestWeapons.SandboxWeapons
{
    /// <summary>
    /// Shared VFX utility methods for the Sandbox Terra Blade weapon system.
    /// Centralises texture loading so satellite files (LightShardProjectile,
    /// LightShardBeam, LightShardExplosion) use the same safe pattern as the
    /// main swing projectile.
    /// </summary>
    internal static class SandboxVFXHelper
    {
        /// <summary>
        /// Safely request a <see cref="Texture2D"/> by mod asset path.
        /// Uses <see cref="ModContent.HasAsset"/> to avoid exceptions
        /// from missing or unloaded assets.
        /// </summary>
        /// <param name="path">
        /// The mod-relative asset path (e.g. "MagnumOpus/Assets/Particles/SoftGlow2").
        /// </param>
        /// <returns>The texture if available, otherwise <c>null</c>.</returns>
        internal static Texture2D SafeRequest(string path)
        {
            try
            {
                if (ModContent.HasAsset(path))
                    return ModContent.Request<Texture2D>(path).Value;
            }
            catch { }
            return null;
        }
    }
}
