using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.Nachtmusik.Weapons.CelestialChorusBaton.Buffs;
using MagnumOpus.Content.Nachtmusik.Weapons.CelestialChorusBaton.Utilities;

namespace MagnumOpus.Content.Nachtmusik.Weapons.CelestialChorusBaton.Projectiles
{
    /// <summary>
    /// Nocturnal Guardian minion -- scaffold based on BlackSwanFlareProj pattern.
    /// Orbit + dash attack minion with IncisorOrbRenderer visuals.
    /// </summary>
    public class NocturnalGuardianMinion : ModProjectile
    {
        private const float HomingRange = 800f;
        private const float MaxSpeed = 22f;
        private Player Owner => Main.player[Projectile.owner];
        private bool _initialized;
        private VertexStrip _strip;

        private float orbitAngle;
        private int attackCooldown;
        private bool isAttacking;
        private Vector2 attackTarget;
        private int attackTimer;

        public override string Texture => "MagnumOpus/Content/Nachtmusik/Weapons/CelestialChorusBaton/NocturnalGuardianMinion";

        public override void SetStaticDefaults()
        {
            Main.projPet[Type] = true;
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
        }

        public override bool? CanCutTiles() => false;

        public override bool MinionContactDamage() => true;

        public override void AI()
        {
            if (!CheckActive(Owner))
                return;

            if (!_initialized)
            {
                _initialized = true;
            }

            orbitAngle += 0.04f;
            attackCooldown = Math.Max(0, attackCooldown - 1);

            NPC target = CelestialChorusBatonUtils.ClosestNPCAt(Owner.Center, HomingRange);

            if (!isAttacking)
            {
                // Orbit around player with pulsing radius
                float orbitRadius = 80f + 30f * (float)Math.Sin(orbitAngle * 0.5f);
                Vector2 idealPos = Owner.Center + orbitAngle.ToRotationVector2() * orbitRadius;
                Vector2 toIdeal = idealPos - Projectile.Center;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdeal * 0.15f, 0.1f);

                // Check for attack opportunity
                if (target != null && attackCooldown == 0)
                {
                    isAttacking = true;
                    attackTarget = target.Center;
                    attackTimer = 0;
                    attackCooldown = 45;
                }
            }
            else
            {
                attackTimer++;

                // Dash attack
                if (attackTimer < 15)
                {
                    Vector2 toTarget = (attackTarget - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    Projectile.velocity = toTarget * MaxSpeed;
                }
                else
                {
                    // Return to orbit
                    isAttacking = false;
                }
            }

            Projectile.rotation = Projectile.velocity.X * 0.02f;
            Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;

            // Trail dust
            if (Main.rand.NextBool(3))
            {
                int dustType = Main.rand.NextBool() ? DustID.WhiteTorch : DustID.BlueTorch;
                Color dustColor = Main.rand.NextBool() ? new Color(180, 200, 255) : new Color(60, 70, 150);
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    dustType, -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    0, dustColor, 0.8f);
                d.noGravity = true;
                d.fadeIn = 0.6f;
            }

            // Pulsing light
            float pulse = 1f + 0.15f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 4f);
            Lighting.AddLight(Projectile.Center, new Vector3(0.3f, 0.35f, 0.6f) * 0.5f * pulse);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 hitPos = target.Center;
            for (int i = 0; i < 6; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(4f, 4f);
                Color col = i % 2 == 0 ? new Color(60, 70, 150) : new Color(180, 200, 255);
                Dust d = Dust.NewDustPerfect(hitPos, DustID.WhiteTorch, sparkVel, 0, col, 0.5f);
                d.noGravity = true;
            }
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0, -1f);
                Dust d = Dust.NewDustPerfect(hitPos + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.BlueTorch, vel, 0, new Color(60, 70, 150), 0.5f);
                d.noGravity = true;
            }
            try { NachtmusikVFXLibrary.SpawnMusicNotes(hitPos, 1, 12f, 0.4f, 0.7f, 20); } catch { }
            try { NachtmusikVFXLibrary.SpawnMixedSparkleImpact(hitPos, 0.6f, 4, 4); } catch { }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, IncisorOrbRenderer.Nachtmusik, ref _strip);
            }
            catch { }
            finally
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(3f, 3f);
                Color col = Main.rand.NextBool() ? new Color(60, 70, 150) : new Color(180, 200, 255);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch, sparkVel, 0, col, 0.3f);
                d.noGravity = true;
            }
            try { NachtmusikVFXLibrary.SpawnMusicNotes(Projectile.Center, 1, 12f, 0.5f, 0.7f, 20); } catch { }
            try { NachtmusikVFXLibrary.SpawnMixedSparkleImpact(Projectile.Center, 0.5f, 4, 4); } catch { }
            try { NachtmusikVFXLibrary.SpawnCelestialSparkles(Projectile.Center, 3, 15f); } catch { }
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<CelestialChorusBatonBuff>());
                return false;
            }

            if (owner.HasBuff(ModContent.BuffType<CelestialChorusBatonBuff>()))
                Projectile.timeLeft = 2;

            return true;
        }
    }
}
