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
using MagnumOpus.Content.Nachtmusik.Weapons.StarweaversGrimoire.Utilities;

namespace MagnumOpus.Content.Nachtmusik.Weapons.StarweaversGrimoire.Projectiles
{
    /// <summary>
    /// Starweaver orb projectile — Constellation Web behavior.
    /// Mode 0: flies with gentle homing, decelerates to stationary node.
    ///   Nodes persist 3s, tether to nearby nodes dealing damage along the line.
    /// Mode 1: Tapestry Weave seeker (right-click burst), aggressive homing.
    /// Mode 2: Bonus seeking orb (4th cast), moderate homing.
    ///
    /// ai[0] = mode (0=node, 1=tapestry weave seeker, 2=bonus seeker)
    /// ai[1] = frame timer
    /// localAI[0] = node state (0=flying, 1=stationary node)
    /// localAI[1] = node lifetime countdown
    /// </summary>
    public class StarweaverOrbProjectile : ModProjectile
    {
        // --- Node System Constants ---
        private const float HomingRange = 350f;
        private const float MaxSpeed = 16f;
        private const int FlyingPhaseFrames = 40;
        private const int NodeLifetime = 180;         // 3 seconds at 60fps
        private const float TetherRange = 300f;
        private const float TetherHitWidth = 20f;
        private const int MaxNodes = 8;
        private const int TetherHitCooldown = 20;

        // --- Seeker Constants ---
        private const float SeekerHomingStrength = 0.10f;
        private const float SeekerMaxSpeed = 18f;
        private const float BonusHomingStrength = 0.06f;
        private const float BonusMaxSpeed = 14f;

        private Player Owner => Main.player[Projectile.owner];
        private bool _initialized;
        private VertexStrip _strip;

        private bool IsNode => Projectile.localAI[0] == 1f;

        public override string Texture => "MagnumOpus/Content/Nachtmusik/Weapons/StarweaversGrimoire/StarweaversGrimoire";

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
            Projectile.timeLeft = 300;
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
            float mode = Projectile.ai[0];

            if (mode == 0f)
                RunNodeBehavior();
            else
                RunSeekerBehavior();

            Projectile.rotation += 0.05f;

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

            // Ambient node shimmer dust
            if (IsNode && Main.rand.NextBool(5))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(12f, 12f),
                    DustID.WhiteTorch, Main.rand.NextVector2Circular(0.3f, 0.3f), 0, new Color(120, 140, 255), 0.5f);
                d.noGravity = true;
            }

            // Pulsing light (brighter for nodes)
            float pulse = 1f + 0.15f * (float)Math.Sin(Projectile.timeLeft * 0.2f);
            float lightIntensity = IsNode ? 0.5f : 0.35f;
            Lighting.AddLight(Projectile.Center, new Vector3(0.3f, 0.35f, 0.6f) * lightIntensity * pulse);
        }

        #region Node Behavior

        private void RunNodeBehavior()
        {
            if (!IsNode)
            {
                // Flying phase: gentle homing then decelerate to stationary
                if (Projectile.ai[1] < FlyingPhaseFrames)
                {
                    NPC target = StarweaversGrimoireUtils.ClosestNPCAt(Projectile.Center, HomingRange);
                    if (target != null)
                    {
                        Vector2 desiredDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredDir * Projectile.velocity.Length(), 0.04f);
                    }
                    if (Projectile.velocity.Length() > MaxSpeed)
                        Projectile.velocity = Vector2.Normalize(Projectile.velocity) * MaxSpeed;
                }
                else
                {
                    // Deceleration phase
                    Projectile.velocity *= 0.93f;
                    if (Projectile.velocity.Length() < 0.5f)
                        BecomeNode();
                }
            }
            else
            {
                // Stationary node: check tether damage, countdown lifetime
                Projectile.velocity = Vector2.Zero;
                Projectile.localAI[1]--;

                if (Projectile.localAI[1] <= 0)
                {
                    Projectile.Kill();
                    return;
                }

                // Check tether line damage against enemies
                CheckTetherDamage();

                // Decrement local NPC immunity timers for tether hits
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (Projectile.localNPCImmunity[i] > 0)
                        Projectile.localNPCImmunity[i]--;
                }
            }
        }

        private void BecomeNode()
        {
            Projectile.velocity = Vector2.Zero;
            Projectile.tileCollide = false;
            Projectile.localAI[0] = 1f;
            Projectile.localAI[1] = NodeLifetime;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = TetherHitCooldown;
            Projectile.timeLeft = NodeLifetime + 10;

            // Enforce max node count — kill oldest if exceeded
            EnforceMaxNodes();

            // Node formation VFX burst
            for (int i = 0; i < 8; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(3f, 3f);
                Color col = i % 2 == 0 ? new Color(60, 70, 150) : new Color(180, 200, 255);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch, sparkVel, 0, col, 0.6f);
                d.noGravity = true;
            }
            try { NachtmusikVFXLibrary.SpawnCelestialSparkles(Projectile.Center, 4, 12f); } catch { }
        }

        private void EnforceMaxNodes()
        {
            int nodeCount = 0;
            int oldestIndex = -1;
            float lowestLifetime = float.MaxValue;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (!p.active || p.owner != Projectile.owner || p.type != Projectile.type) continue;
                if (p.localAI[0] != 1f) continue; // Not a node
                nodeCount++;

                if (p.localAI[1] < lowestLifetime)
                {
                    lowestLifetime = p.localAI[1];
                    oldestIndex = i;
                }
            }

            if (nodeCount > MaxNodes && oldestIndex >= 0)
                Main.projectile[oldestIndex].Kill();
        }

        private void CheckTetherDamage()
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile other = Main.projectile[i];
                if (other.whoAmI == Projectile.whoAmI) continue;
                if (!other.active || other.owner != Projectile.owner || other.type != Projectile.type) continue;
                if (other.localAI[0] != 1f) continue; // Not a node

                float dist = Vector2.Distance(Projectile.Center, other.Center);
                if (dist > TetherRange || dist < 1f) continue;

                // Only process from the lower whoAmI to avoid double-damage
                if (Projectile.whoAmI > other.whoAmI) continue;

                for (int j = 0; j < Main.maxNPCs; j++)
                {
                    NPC npc = Main.npc[j];
                    if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immortal) continue;
                    if (Projectile.localNPCImmunity[j] > 0) continue;

                    float collisionPoint = 0f;
                    if (Collision.CheckAABBvLineCollision(npc.position, new Vector2(npc.width, npc.height),
                        Projectile.Center, other.Center, TetherHitWidth, ref collisionPoint))
                    {
                        // Deal tether damage
                        int dir = (npc.Center.X > Projectile.Center.X) ? 1 : -1;
                        npc.SimpleStrikeNPC(Projectile.damage, dir, false, Projectile.knockBack * 0.5f, null, false, 0f, true);

                        Projectile.localNPCImmunity[j] = TetherHitCooldown;

                        // Tether hit VFX
                        for (int k = 0; k < 4; k++)
                        {
                            Dust d = Dust.NewDustPerfect(npc.Center, DustID.WhiteTorch,
                                Main.rand.NextVector2CircularEdge(3f, 3f), 0, new Color(120, 140, 255), 0.5f);
                            d.noGravity = true;
                        }
                        try { NachtmusikVFXLibrary.SpawnMusicNotes(npc.Center, 1, 10f, 0.3f, 0.5f, 15); } catch { }
                    }
                }
            }
        }

        #endregion

        #region Seeker Behavior

        private void RunSeekerBehavior()
        {
            float homingStr = Projectile.ai[0] == 1f ? SeekerHomingStrength : BonusHomingStrength;
            float maxSpd = Projectile.ai[0] == 1f ? SeekerMaxSpeed : BonusMaxSpeed;

            NPC target = StarweaversGrimoireUtils.ClosestNPCAt(Projectile.Center, 600f);
            if (target != null)
            {
                Vector2 desiredDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredDir * Projectile.velocity.Length(), homingStr);
            }

            if (Projectile.velocity.Length() < maxSpd)
                Projectile.velocity *= 1.02f;
            if (Projectile.velocity.Length() > maxSpd)
                Projectile.velocity = Vector2.Normalize(Projectile.velocity) * maxSpd;
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

            // Mode 0 orbs convert to stationary node on enemy hit
            if (Projectile.ai[0] == 0f && Projectile.localAI[0] == 0f)
            {
                BecomeNode();
            }
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
    }
}
