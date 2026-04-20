using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.OdeToJoy;

namespace MagnumOpus.Content.OdeToJoy.Weapons.PetalStormCannon.Projectiles
{
    /// <summary>
    /// Petal cannonball projectile for PetalStormCannon.
    /// BlackSwanFlareProj scaffold — homing sub-projectile with IncisorOrb rendering.
    /// </summary>
    public class PetalBombProjectile : ModProjectile
    {
        private const float HomingRange = 350f;
        private const float HomingStrength = 0.08f;
        private const float MaxSpeed = 16f;
        private Player Owner => Main.player[Projectile.owner];
        private bool _initialized;
        private VertexStrip _strip;
        private bool _hasClusterSplit; // Prevents double-splitting

        public override string Texture => "MagnumOpus/Content/OdeToJoy/Weapons/PetalStormCannon/PetalStormCannon";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            if (!_initialized)
            {
                _initialized = true;
                Projectile.rotation = Projectile.velocity.ToRotation();
            }

            // Gravity Arc: petal cannonball with lobbed trajectory
            Projectile.velocity.Y += 0.05f;
            if (Projectile.velocity.Length() > MaxSpeed * 1.5f)
                Projectile.velocity = Vector2.Normalize(Projectile.velocity) * MaxSpeed * 1.5f;

            Projectile.rotation = Projectile.velocity.ToRotation();

            if (Main.rand.NextBool(3))
            {
                int dustType = Main.rand.NextBool() ? DustID.GreenTorch : DustID.GoldFlame;
                Color dustColor = Main.rand.NextBool() ? new Color(90, 200, 60) : new Color(255, 210, 60);
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    dustType, -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    0, dustColor, 0.8f);
                d.noGravity = true;
                d.fadeIn = 0.6f;
            }

            float pulse = 1f + 0.15f * (float)Math.Sin(Projectile.timeLeft * 0.2f);
            Lighting.AddLight(Projectile.Center, new Vector3(0.4f, 0.55f, 0.2f) * 0.35f * pulse);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Trigger cluster split on hit
            SpawnClusterChildren();

            Vector2 hitPos = target.Center;
            for (int i = 0; i < 6; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(4f, 4f);
                Color col = i % 2 == 0 ? new Color(90, 200, 60) : new Color(255, 210, 60);
                Dust d = Dust.NewDustPerfect(hitPos, DustID.GreenTorch, sparkVel, 0, col, 0.5f);
                d.noGravity = true;
            }
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0, -1f);
                Dust d = Dust.NewDustPerfect(hitPos + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.GoldFlame, vel, 0, new Color(255, 210, 60), 0.5f);
                d.noGravity = true;
            }
            try { OdeToJoyVFXLibrary.SpawnMusicNotes(hitPos, 1, 12f, 0.4f, 0.7f, 20); } catch { }
            try { OdeToJoyVFXLibrary.SpawnMixedSparkleImpact(hitPos, 0.6f, 4, 4); } catch { }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // Trigger cluster split on tile collision, then die
            SpawnClusterChildren();
            return true; // Kill the projectile
        }

        /// <summary>
        /// Spawns cluster children based on mode (normal vs Hurricane).
        /// Only triggers once per projectile lifetime.
        /// </summary>
        private void SpawnClusterChildren()
        {
            if (_hasClusterSplit) return;
            _hasClusterSplit = true;

            bool isHurricane = Projectile.ai[0] == 1f;

            try
            {
                // Both modes: spawn 3 homing child orbs scattering outward
                for (int i = 0; i < 3; i++)
                {
                    float angle = MathHelper.TwoPi / 3f * i + Main.rand.NextFloat(-0.3f, 0.3f);
                    Vector2 childVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 8f);
                    GenericHomingOrbChild.SpawnChild(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center, childVel,
                        Projectile.damage / 3, Projectile.knockBack * 0.5f, Projectile.owner,
                        homingStrength: 0.04f,
                        behaviorFlags: 0,
                        themeIndex: GenericHomingOrbChild.THEME_ODETOJOY,
                        scaleMult: 0.7f,
                        timeLeft: 45);
                }

                // Hurricane mode: also spawn a pull damage zone
                if (isHurricane)
                {
                    GenericDamageZone.SpawnZone(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center, Projectile.damage / 2, Projectile.knockBack, Projectile.owner,
                        GenericDamageZone.FLAG_PULL,
                        150f,
                        GenericHomingOrbChild.THEME_ODETOJOY,
                        durationFrames: 180);
                }
            }
            catch { }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, IncisorOrbRenderer.OdeToJoy, ref _strip);
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
            // Also trigger cluster split on death (e.g., from timeLeft expiring)
            SpawnClusterChildren();

            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(3f, 3f);
                Color col = Main.rand.NextBool() ? new Color(90, 200, 60) : new Color(255, 210, 60);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GreenTorch, sparkVel, 0, col, 0.3f);
                d.noGravity = true;
            }
            try { OdeToJoyVFXLibrary.SpawnMusicNotes(Projectile.Center, 1, 12f, 0.5f, 0.7f, 20); } catch { }
            try { OdeToJoyVFXLibrary.SpawnMixedSparkleImpact(Projectile.Center, 0.5f, 4, 4); } catch { }
            try { OdeToJoyVFXLibrary.SpawnJoyousSparkles(Projectile.Center, 3, 15f); } catch { }
        }
    }
}
