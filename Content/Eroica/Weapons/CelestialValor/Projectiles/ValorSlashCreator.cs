using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.CelestialValor.Projectiles
{
    /// <summary>
    /// Timed cross-slash spawner — creates delayed ValorSlash bursts on targets.
    /// Mirrors ExobeamSlashCreator pattern: invisible projectile that periodically
    /// fires oriented slash visuals at the target NPC.
    /// </summary>
    public class ValorSlashCreator : ModProjectile
    {
        private NPC Target => Main.npc[(int)Projectile.ai[0]];

        private float SlashDirection
        {
            get
            {
                if (Projectile.ai[1] > MathHelper.Pi)
                    return Main.rand.NextFloatDirection();
                return Projectile.ai[1] + Main.rand.NextFloatDirection() * 0.25f;
            }
        }

        public override string Texture => "MagnumOpus/Content/SandboxExoblade/Projectiles/InvisibleProj";

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 40;
            Projectile.MaxUpdates = 2;
            Projectile.noEnchantmentVisuals = true;
        }

        public override void AI()
        {
            if (Main.myPlayer == Projectile.owner && Projectile.timeLeft % 18 == 17)
            {
                float maxOffset = Target.width * 0.35f;
                if (maxOffset > 250f) maxOffset = 250f;

                Vector2 spawnOffset = SlashDirection.ToRotationVector2() * Main.rand.NextFloatDirection() * maxOffset;
                Vector2 sliceVel = spawnOffset.SafeNormalize(Vector2.UnitY) * 0.1f;

                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Target.Center + spawnOffset,
                    sliceVel, ModContent.ProjectileType<ValorSlash>(),
                    Projectile.damage, 0f, Projectile.owner);
            }
        }

        public override bool? CanDamage() => false;
    }
}
