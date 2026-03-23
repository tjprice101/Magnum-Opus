using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.ResonantWeapons.RequiemOfReality.Projectiles
{
    /// <summary>
    /// Requiem Special Projectile — Freeze/stun aura.
    /// Tracks player position, applies velocity reduction to nearby enemies.
    /// Not friendly (does not deal damage directly), acts as a debuff zone.
    /// Spawned on right-click charge release.
    /// </summary>
    public class RequiemSpecialProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow";

        private const float AuraRadius = 250f;

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180; // 3 seconds
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            // Track owning player
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }
            Projectile.Center = owner.Center;

            // Freeze nearby enemies (reduce their velocity)
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (!npc.CanBeChasedBy()) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist > AuraRadius) continue;

                // Stronger slow the closer the enemy is
                float slowFactor = 1f - (dist / AuraRadius);
                slowFactor = MathHelper.Clamp(slowFactor, 0f, 0.8f);
                npc.velocity *= (1f - slowFactor);
            }

            // Visual dust ring
            if (Main.rand.NextBool(3))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = AuraRadius * Main.rand.NextFloat(0.7f, 1.0f);
                Vector2 dustPos = Projectile.Center + angle.ToRotationVector2() * radius;
                Dust d = Dust.NewDustDirect(dustPos, 0, 0, DustID.IceTorch, 0f, 0f);
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.5f, 1.0f);
                d.velocity = (Projectile.Center - dustPos).SafeNormalize(Vector2.Zero) * 0.5f;
            }

            // Fade-in/out opacity
            float life = 1f - (Projectile.timeLeft / 180f);
            float fadeIn = MathHelper.Clamp(life * 5f, 0f, 1f);
            float fadeOut = MathHelper.Clamp(Projectile.timeLeft / 30f, 0f, 1f);
            Projectile.Opacity = fadeIn * fadeOut;

            // Cold light aura
            float lightIntensity = Projectile.Opacity * 0.5f;
            Lighting.AddLight(Projectile.Center, 0.3f * lightIntensity, 0.4f * lightIntensity, 0.9f * lightIntensity);
        }
    }
}
