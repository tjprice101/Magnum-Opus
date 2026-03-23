using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.RoseThornChainsaw.Projectiles
{
    /// <summary>
    /// Empowerment aura projectile for Rose Thorn Chainsaw's charged right-click.
    /// Tracks the player position for the full duration. Not friendly (buff-only aura).
    /// </summary>
    public class RoseThornChainsawSpecialProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow";

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            // Track the owning player
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }

            Projectile.Center = owner.Center;

            // Gentle pulsing scale
            float pulse = 0.9f + 0.1f * (float)System.Math.Sin(Main.GlobalTimeWrappedHourly * 4f);
            Projectile.scale = pulse;

            // Fade in over the first 30 frames, fade out over the last 60 frames
            if (Projectile.timeLeft > 540)
                Projectile.Opacity = 1f - (Projectile.timeLeft - 540) / 60f;
            else if (Projectile.timeLeft < 60)
                Projectile.Opacity = Projectile.timeLeft / 60f;
            else
                Projectile.Opacity = 1f;

            // Rose-gold empowerment light
            float intensity = 0.3f * Projectile.Opacity * pulse;
            Lighting.AddLight(Projectile.Center, new Vector3(intensity * 1.0f, intensity * 0.6f, intensity * 0.4f));
        }
    }
}
