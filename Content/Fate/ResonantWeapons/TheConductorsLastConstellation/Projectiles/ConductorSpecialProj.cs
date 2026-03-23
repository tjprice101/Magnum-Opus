using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.ResonantWeapons.TheConductorsLastConstellation.Projectiles
{
    /// <summary>
    /// Conductor Special Projectile — Spectral thrown blade.
    /// Weak homing toward nearest enemy, sparkle trail, crimson/pink light.
    /// Spawned on right-click charge release.
    /// </summary>
    public class ConductorSpecialProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow";

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            // Rotate visually
            Projectile.rotation += 0.15f;

            // Weak homing toward nearest enemy
            float homingStrength = 0.04f;
            float detectionRange = 600f;
            NPC closest = null;
            float closestDist = detectionRange;

            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (!npc.CanBeChasedBy()) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }

            if (closest != null)
            {
                Vector2 toTarget = (closest.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), homingStrength);
            }

            // Sparkle trail dust
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.PinkTorch, 0f, 0f);
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
                d.velocity *= 0.3f;
            }

            // Crimson/pink light
            float pulse = 0.5f + 0.2f * MathHelper.Clamp((float)System.Math.Sin(Projectile.ai[0]++ * 0.1f), 0f, 1f);
            Lighting.AddLight(Projectile.Center, 0.8f * pulse, 0.2f * pulse, 0.4f * pulse);
        }
    }
}
