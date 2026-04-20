using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.Nachtmusik.Weapons.RequiemOfTheCosmos.Utilities;

namespace MagnumOpus.Content.Nachtmusik.Weapons.RequiemOfTheCosmos.Projectiles
{
    /// <summary>
    /// Cosmic requiem orb projectile — Gravity Well behavior.
    /// Mode 0 (normal): standard homing (0.08 strength, 400px range).
    /// Mode 1 (gravity well): decelerates to hover, becomes gravity well for 2s (120 frames),
    ///   pulls enemies within 200px, detonates on expiry spawning 3 homing children.
    /// Mode 2 (event horizon): 2x radius (400px), 3s duration (180 frames), spawns 5 children.
    ///
    /// ai[0] = mode (0=normal, 1=gravity well, 2=event horizon)
    /// ai[1] = frame timer
    /// localAI[0] = gravity well state (0=flying, 1=gravity well active)
    /// localAI[1] = gravity well lifetime countdown
    /// </summary>
    public class CosmicRequiemOrbProjectile : ModProjectile
    {
        // --- Mode Constants ---
        private const float MODE_NORMAL = 0f;
        private const float MODE_GRAVITY_WELL = 1f;
        private const float MODE_EVENT_HORIZON = 2f;

        // --- Normal Mode ---
        private const float NormalHomingRange = 400f;
        private const float NormalHomingStrength = 0.08f;
        private const float NormalMaxSpeed = 16f;

        // --- Gravity Well Mode ---
        private const int FlyingPhaseFrames = 50;
        private const float GravityWellRadius = 200f;
        private const float EventHorizonRadius = 400f;
        private const int GravityWellDuration = 120;    // 2 seconds
        private const int EventHorizonDuration = 180;   // 3 seconds
        private const float PullStrength = 0.6f;
        private const int GravityWellChildren = 3;
        private const int EventHorizonChildren = 5;
        private const int WellHitCooldown = 15;

        private Player Owner => Main.player[Projectile.owner];
        private bool _initialized;
        private VertexStrip _strip;

        private float Mode => Projectile.ai[0];
        private bool IsGravityWell => Projectile.localAI[0] == 1f;

        public override string Texture => "MagnumOpus/Content/Nachtmusik/Weapons/RequiemOfTheCosmos/RequiemOfTheCosmos";

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
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 240;
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

            Projectile.ai[1]++;

            if (Mode == MODE_NORMAL)
                RunNormalHoming();
            else
                RunGravityWellBehavior();

            Projectile.rotation += 0.04f;

            // Trail dust
            if (Main.rand.NextBool(3))
            {
                int dustType = Main.rand.NextBool() ? DustID.WhiteTorch : DustID.BlueTorch;
                Color dustColor = Main.rand.NextBool() ? new Color(180, 200, 255) : new Color(60, 70, 150);
                float dustScale = IsGravityWell ? 1.1f : 0.8f;
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    dustType, -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    0, dustColor, dustScale);
                d.noGravity = true;
                d.fadeIn = 0.6f;
            }

            // Gravity well ambient effects
            if (IsGravityWell)
            {
                float wellRadius = (Mode == MODE_EVENT_HORIZON) ? EventHorizonRadius : GravityWellRadius;

                // Swirling dust ring around well
                if (Main.rand.NextBool(2))
                {
                    float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                    float dist = wellRadius * (0.6f + Main.rand.NextFloat() * 0.4f);
                    Vector2 dustPos = Projectile.Center + angle.ToRotationVector2() * dist;
                    Vector2 tangent = new Vector2(-MathF.Sin(angle), MathF.Cos(angle)) * 1.5f;
                    Color col = Main.rand.NextBool() ? new Color(60, 70, 150) : new Color(100, 80, 180);
                    Dust d = Dust.NewDustPerfect(dustPos, DustID.WhiteTorch, tangent, 0, col, 0.6f);
                    d.noGravity = true;
                }

                // Core pulsing dust
                if (Main.rand.NextBool(4))
                {
                    Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                        DustID.BlueTorch, Main.rand.NextVector2Circular(0.5f, 0.5f), 0, new Color(140, 120, 220), 0.7f);
                    d.noGravity = true;
                }
            }

            // Pulsing light — stronger for gravity wells
            float pulse = 1f + 0.15f * (float)Math.Sin(Projectile.timeLeft * 0.2f);
            float lightMult = IsGravityWell ? 0.6f : 0.35f;
            Lighting.AddLight(Projectile.Center, new Vector3(0.3f, 0.35f, 0.6f) * lightMult * pulse);
        }

        #region Normal Homing

        private void RunNormalHoming()
        {
            NPC target = RequiemOfTheCosmosUtils.ClosestNPCAt(Projectile.Center, NormalHomingRange);
            if (target != null)
            {
                Vector2 desiredDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredDir * Projectile.velocity.Length(), NormalHomingStrength);
            }
            if (Projectile.velocity.Length() > NormalMaxSpeed)
                Projectile.velocity = Vector2.Normalize(Projectile.velocity) * NormalMaxSpeed;
        }

        #endregion

        #region Gravity Well Behavior

        private void RunGravityWellBehavior()
        {
            if (!IsGravityWell)
            {
                // Flying phase: home then decelerate
                if (Projectile.ai[1] < FlyingPhaseFrames)
                {
                    NPC target = RequiemOfTheCosmosUtils.ClosestNPCAt(Projectile.Center, NormalHomingRange);
                    if (target != null)
                    {
                        Vector2 desiredDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredDir * Projectile.velocity.Length(), NormalHomingStrength);
                    }
                    if (Projectile.velocity.Length() > NormalMaxSpeed)
                        Projectile.velocity = Vector2.Normalize(Projectile.velocity) * NormalMaxSpeed;
                }
                else
                {
                    // Decelerate to hover
                    Projectile.velocity *= 0.90f;
                    if (Projectile.velocity.Length() < 1f)
                        BecomeGravityWell();
                }
            }
            else
            {
                // Gravity well active: pull enemies, countdown
                Projectile.velocity = Vector2.Zero;
                Projectile.localAI[1]--;

                PullNearbyEnemies();

                // Decrement local NPC immunity timers
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (Projectile.localNPCImmunity[i] > 0)
                        Projectile.localNPCImmunity[i]--;
                }

                if (Projectile.localAI[1] <= 0)
                {
                    DetonateGravityWell();
                    Projectile.Kill();
                }
            }
        }

        private void BecomeGravityWell()
        {
            Projectile.velocity = Vector2.Zero;
            Projectile.tileCollide = false;
            Projectile.localAI[0] = 1f;
            int duration = (Mode == MODE_EVENT_HORIZON) ? EventHorizonDuration : GravityWellDuration;
            Projectile.localAI[1] = duration;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = WellHitCooldown;
            Projectile.timeLeft = duration + 10;

            // Formation burst VFX
            int burstCount = (Mode == MODE_EVENT_HORIZON) ? 14 : 8;
            for (int i = 0; i < burstCount; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(4f, 4f);
                Color col = i % 2 == 0 ? new Color(60, 70, 150) : new Color(140, 120, 220);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch, sparkVel, 0, col, 0.7f);
                d.noGravity = true;
            }
            try { NachtmusikVFXLibrary.SpawnCelestialSparkles(Projectile.Center, 5, 15f); } catch { }
        }

        private void PullNearbyEnemies()
        {
            float pullRadius = (Mode == MODE_EVENT_HORIZON) ? EventHorizonRadius : GravityWellRadius;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immortal) continue;

                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist > pullRadius || dist < 1f) continue;

                // Pull force — stronger the closer to center
                float pullFactor = 1f - (dist / pullRadius);
                Vector2 pullDir = (Projectile.Center - npc.Center).SafeNormalize(Vector2.Zero);
                npc.velocity += pullDir * PullStrength * pullFactor;

                // Deal proximity damage with cooldown
                if (dist < pullRadius * 0.5f && Projectile.localNPCImmunity[i] <= 0)
                {
                    int dir = (npc.Center.X > Projectile.Center.X) ? 1 : -1;
                    npc.SimpleStrikeNPC((int)(Projectile.damage * 0.3f), dir, false, 0f, null, false, 0f, true);
                    Projectile.localNPCImmunity[i] = WellHitCooldown;

                    // Proximity damage VFX
                    for (int k = 0; k < 3; k++)
                    {
                        Dust d = Dust.NewDustPerfect(npc.Center, DustID.BlueTorch,
                            Main.rand.NextVector2CircularEdge(2f, 2f), 0, new Color(100, 80, 180), 0.4f);
                        d.noGravity = true;
                    }
                }
            }

            // Also gently curve friendly projectiles toward the well
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (!p.active || p.whoAmI == Projectile.whoAmI || !p.friendly || p.owner != Projectile.owner) continue;

                float dist = Vector2.Distance(Projectile.Center, p.Center);
                if (dist > pullRadius || dist < 1f) continue;

                float pullFactor = 1f - (dist / pullRadius);
                Vector2 pullDir = (Projectile.Center - p.Center).SafeNormalize(Vector2.Zero);
                p.velocity += pullDir * PullStrength * pullFactor * 0.3f;
            }
        }

        private void DetonateGravityWell()
        {
            float detonateRadius = (Mode == MODE_EVENT_HORIZON) ? EventHorizonRadius : GravityWellRadius;
            int childCount = (Mode == MODE_EVENT_HORIZON) ? EventHorizonChildren : GravityWellChildren;

            // AoE detonation damage to all enemies in radius
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immortal) continue;

                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist > detonateRadius) continue;

                int dir = (npc.Center.X > Projectile.Center.X) ? 1 : -1;
                npc.SimpleStrikeNPC(Projectile.damage, dir, false, Projectile.knockBack, null, false, 0f, true);
            }

            // Spawn homing children
            float angleStep = MathHelper.TwoPi / childCount;
            for (int i = 0; i < childCount; i++)
            {
                float angle = angleStep * i + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 childVel = angle.ToRotationVector2() * 8f;
                GenericHomingOrbChild.SpawnChild(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center, childVel,
                    (int)(Projectile.damage * 0.5f), Projectile.knockBack * 0.4f, Projectile.owner,
                    0.08f, GenericHomingOrbChild.FLAG_ACCELERATE, GenericHomingOrbChild.THEME_NACHTMUSIK,
                    0.9f, 75);
            }

            // Detonation VFX burst
            int burstCount = (Mode == MODE_EVENT_HORIZON) ? 20 : 12;
            for (int i = 0; i < burstCount; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(6f, 6f);
                Color col = Main.rand.NextBool() ? new Color(60, 70, 150) : new Color(180, 200, 255);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch, sparkVel, 0, col, 0.8f);
                d.noGravity = true;
            }
            try { NachtmusikVFXLibrary.SpawnMusicNotes(Projectile.Center, 3, 18f, 0.6f, 1.0f, 30); } catch { }
            try { NachtmusikVFXLibrary.SpawnMixedSparkleImpact(Projectile.Center, 1.0f, 6, 6); } catch { }
            try { NachtmusikVFXLibrary.SpawnCelestialSparkles(Projectile.Center, 6, 20f); } catch { }
        }

        #endregion

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
            // If this was a gravity well that didn't get to detonate normally, still do the burst
            if (IsGravityWell && Projectile.localAI[1] > 0)
                DetonateGravityWell();

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
    }
}
