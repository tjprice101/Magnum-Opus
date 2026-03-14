using MagnumOpus.Content.Nachtmusik.Weapons.TwilightSeverance.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Nachtmusik.Weapons.TwilightSeverance.Projectiles
{
    /// <summary>
    /// Constellation Break strike — fast homing slash spawned by right-click.
    /// ai[0] = stacks consumed from target, ai[1] = target NPC index.
    /// </summary>
    public sealed class TwilightSlashProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private int StacksConsumed => (int)Projectile.ai[0];
        private int TargetNPC => (int)Projectile.ai[1];
        private bool hasPlayedNote;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 30;
            Projectile.alpha = 0;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            if (TargetNPC >= 0 && TargetNPC < Main.maxNPCs)
            {
                NPC target = Main.npc[TargetNPC];
                if (target.active && !target.friendly)
                {
                    Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    float homingStrength = 0.18f;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), homingStrength);
                }
            }

            if (Main.rand.NextBool(2))
            {
                Vector2 dustVel = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.5f, 0.5f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.ShimmerSpark,
                    dustVel, 0, new Color(180, 200, 240), 0.6f);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, new Vector3(0.2f, 0.24f, 0.4f) * Projectile.Opacity);
        }

        public override bool? CanHitNPC(NPC target)
        {
            if (!target.CanBeChasedBy(Projectile))
                return false;
            return base.CanHitNPC(target);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item162 with { Pitch = 0.15f, Volume = 0.7f }, target.Center);

            if (!hasPlayedNote)
            {
                hasPlayedNote = true;
                TwilightSeveranceVFX.DrawBreakMusicNote(target.Center, StacksConsumed);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            TwilightSeveranceVFX.DrawBreakSlash(Projectile);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 5 + StacksConsumed * 2; i++)
            {
                Vector2 v = Main.rand.NextVector2Circular(2f, 2f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.ShimmerSpark, v, 0,
                    new Color(200, 215, 255), 0.65f);
                d.noGravity = true;
            }
        }
    }
}
