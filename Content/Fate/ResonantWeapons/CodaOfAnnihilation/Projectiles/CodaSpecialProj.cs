using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.ResonantWeapons.CodaOfAnnihilation.Projectiles
{
    /// <summary>
    /// Coda Special Projectile — Spectral sword copy.
    /// Orbits around the player in a spiral pattern, leaves a dust trail.
    /// ai[0] = orbit index (0-5), used to offset the orbit angle.
    /// Spawned on right-click charge release (6 swords in a ring).
    /// </summary>
    public class CodaSpecialProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow";

        private const float BaseOrbitRadius = 80f;
        private const float OrbitSpeed = 0.05f;

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }

            // Track orbit timer
            Projectile.localAI[0] += 1f;
            float timer = Projectile.localAI[0];

            // Orbit index from ai[0] determines angular offset
            float orbitIndex = Projectile.ai[0];
            float baseAngle = MathHelper.TwoPi / 6f * orbitIndex;

            // Spiral orbit: radius expands slightly, angle increases
            float currentAngle = baseAngle + timer * OrbitSpeed;
            float radiusPulse = BaseOrbitRadius + 15f * MathF.Sin(timer * 0.03f);
            Vector2 orbitOffset = currentAngle.ToRotationVector2() * radiusPulse;

            Projectile.Center = owner.Center + orbitOffset;
            Projectile.rotation = currentAngle + MathHelper.PiOver2;

            // Dust trail
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.PurpleTorch, 0f, 0f);
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.5f, 1.0f);
                d.velocity = Projectile.velocity * 0.1f;
            }

            // Crimson-purple light
            float lightPulse = 0.4f + 0.2f * MathF.Sin(timer * 0.1f);
            Lighting.AddLight(Projectile.Center, 0.7f * lightPulse, 0.15f * lightPulse, 0.5f * lightPulse);

            // Fade out near end of life
            if (Projectile.timeLeft < 30)
                Projectile.Opacity = Projectile.timeLeft / 30f;
        }
    }
}
