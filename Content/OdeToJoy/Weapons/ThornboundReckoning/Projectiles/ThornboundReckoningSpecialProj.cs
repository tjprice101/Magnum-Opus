using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Projectiles
{
    /// <summary>
    /// Bouncing blade projectile for Thornbound Reckoning's charged right-click.
    /// Weak homing toward nearby enemies with a gold flame dust trail.
    /// </summary>
    public class ThornboundReckoningSpecialProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow";

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            // Weak homing toward nearest enemy
            float homingStrength = 0.04f;
            float detectionRange = 600f;
            NPC closest = null;
            float closestDist = detectionRange;

            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.CanBeChasedBy())
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }

            if (closest != null)
            {
                Vector2 desired = (closest.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired * Projectile.velocity.Length(), homingStrength);
            }

            // Rotation follows velocity
            Projectile.rotation += 0.3f * Projectile.direction;

            // Gold flame dust trail
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.GoldFlame, 0f, 0f, 100, default, 1.2f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }

            // Subtle golden light
            Lighting.AddLight(Projectile.Center, new Vector3(0.6f, 0.5f, 0.1f));
        }
    }
}
