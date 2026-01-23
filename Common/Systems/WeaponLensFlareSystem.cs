using Microsoft.Xna.Framework;
using Terraria;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// Static utility class for spawning lens flare effects on projectile impacts/explosions.
    /// Called randomly by LensFlareGlobalProjectile when MagnumOpus projectiles die.
    /// </summary>
    public static class WeaponLensFlare
    {
        /// <summary>
        /// Attempts to spawn a lens flare effect at the given position with a random chance.
        /// </summary>
        public static void TrySpawnImpactLensFlare(Vector2 position, Color primaryColor, Color secondaryColor, float chance = 0.15f)
        {
            if (Main.rand.NextFloat() > chance)
                return;
            
            SpawnImpactLensFlare(position, primaryColor, secondaryColor);
        }
        
        private static void SpawnImpactLensFlare(Vector2 position, Color primary, Color secondary)
        {
            float flarePulse = Main.rand.NextFloat(0.7f, 1.0f);
            
            // Central bright flash
            CustomParticles.GenericFlare(position, Color.White * flarePulse, 0.5f * flarePulse, 8);
            CustomParticles.GenericFlare(position, primary * flarePulse * 0.9f, 0.4f * flarePulse, 10);
            
            // Quick 4-6 point star rays
            int rayCount = Main.rand.Next(4, 7);
            float baseAngle = Main.rand.NextFloat(MathHelper.TwoPi);
            for (int ray = 0; ray < rayCount; ray++)
            {
                float rayAngle = baseAngle + MathHelper.TwoPi * ray / rayCount;
                float rayLength = 25f + Main.rand.NextFloat(20f);
                Vector2 rayEnd = position + rayAngle.ToRotationVector2() * rayLength;
                
                Color rayColor = Color.Lerp(primary, secondary, (float)ray / rayCount);
                CustomParticles.GenericFlare(rayEnd, rayColor * 0.6f, 0.2f, 6);
                
                // Streak along ray
                Vector2 streakPos = Vector2.Lerp(position, rayEnd, 0.5f);
                var streak = new GenericGlowParticle(streakPos, rayAngle.ToRotationVector2() * 2f,
                    rayColor * 0.5f, 0.15f, 8, true);
                MagnumParticleHandler.SpawnParticle(streak);
            }
            
            // Occasional horizontal anamorphic streak
            if (Main.rand.NextBool(3))
            {
                float streakLength = 30f + Main.rand.NextFloat(25f);
                for (int s = -1; s <= 1; s += 2)
                {
                    Vector2 streakPos = position + new Vector2(s * streakLength * 0.5f, 0);
                    Color streakColor = s < 0 ? primary : secondary;
                    CustomParticles.GenericFlare(streakPos, Color.Lerp(streakColor, Color.White, 0.4f) * 0.5f, 0.15f, 7);
                }
            }
        }
    }
}
