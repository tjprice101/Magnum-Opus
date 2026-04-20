using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.ClairDeLune;
using MagnumOpus.Content.ClairDeLune.Weapons.MidnightMechanism.Utilities;

namespace MagnumOpus.Content.ClairDeLune.Weapons.MidnightMechanism.Projectiles
{
    /// <summary>
    /// Mechanism Bullet — "Spin-Up Gatling" projectile fired by Midnight Mechanism.
    /// Phase-based behavior driven by ai[0] (phase 0-4) and ai[1] (Midnight Strike flag).
    ///
    /// ai[0] = phase index (0=no homing, 1=spread only, 2=homing 0.04, 3=homing 0.06, 4=homing 0.08)
    /// ai[1] = 1 for Midnight Strike (2x scale, enhanced VFX)
    ///
    /// Foundation-pattern rendering: safe SpriteBatch, IncisorOrbRenderer visuals.
    /// </summary>
    public class MechanismBulletProjectile : ModProjectile
    {
        #region Properties

        private const float MaxSpeed = 24f;

        private Player Owner => Main.player[Projectile.owner];
        private bool _initialized;

        private VertexStrip _strip;

        /// <summary>Phase index from weapon heat level (0-4).</summary>
        private int Phase => (int)Projectile.ai[0];

        /// <summary>Whether this is a Midnight Strike bullet (10x damage, 2x scale).</summary>
        private bool IsMidnightStrike => Projectile.ai[1] >= 1f;

        #endregion

        public override string Texture => "MagnumOpus/Content/ClairDeLune/Weapons/MidnightMechanism/MidnightMechanism";

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

                // Midnight Strike: scale up
                if (IsMidnightStrike)
                {
                    Projectile.scale = 2f;
                    Projectile.penetrate = 3; // Pierce through multiple enemies

                    // Dramatic spawn burst
                    for (int i = 0; i < 10; i++)
                    {
                        Vector2 sparkVel = Main.rand.NextVector2CircularEdge(6f, 6f);
                        Color col = i % 3 == 0 ? new Color(200, 50, 50) : (i % 3 == 1 ? new Color(150, 200, 255) : Color.White);
                        Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.IceTorch, sparkVel, 0, col, 0.8f);
                        d.noGravity = true;
                    }
                }
            }

            // Acceleration — all phases accelerate to max speed
            if (Projectile.velocity.Length() < MaxSpeed)
                Projectile.velocity *= 1.03f;
            if (Projectile.velocity.Length() > MaxSpeed)
                Projectile.velocity = Vector2.Normalize(Projectile.velocity) * MaxSpeed;

            // Phase-based homing
            float homingStrength = Phase switch
            {
                0 => 0f,
                1 => 0f,
                2 => 0.04f,
                3 => 0.06f,
                4 => 0.08f,
                _ => 0f,
            };

            if (homingStrength > 0f)
            {
                NPC target = FindClosestNPC(400f);
                if (target != null)
                {
                    Vector2 desiredDir = (target.Center - Projectile.Center).SafeNormalize(Projectile.velocity.SafeNormalize(Vector2.UnitX));
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredDir * Projectile.velocity.Length(), homingStrength);
                }
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Trail dust — intensity scales with phase
            float phaseIntensity = MathHelper.Clamp(Phase / 4f, 0.2f, 1f);
            if (IsMidnightStrike) phaseIntensity = 1.5f;

            if (Main.rand.NextBool(3))
            {
                int dustType = Main.rand.NextBool() ? DustID.IceTorch : DustID.WhiteTorch;
                Color dustColor;
                if (IsMidnightStrike)
                    dustColor = Main.rand.NextBool() ? new Color(200, 50, 50) : Color.White;
                else
                    dustColor = Main.rand.NextBool() ? new Color(150, 200, 255) : new Color(240, 240, 255);

                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    dustType, -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    0, dustColor, 0.8f * phaseIntensity);
                d.noGravity = true;
                d.fadeIn = 0.6f;
            }

            // Extra dust at higher phases
            if (Phase >= 3 && Main.rand.NextBool(2))
            {
                Color col = new Color(150, 200, 255) * 0.5f;
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    DustID.WhiteTorch, -Projectile.velocity * 0.08f, 0, col, 0.4f);
                d.noGravity = true;
            }

            // Pulsing light — brighter at higher phases
            float pulse = 1f + 0.15f * (float)Math.Sin(Projectile.timeLeft * 0.2f);
            float lightMult = IsMidnightStrike ? 0.8f : (0.25f + phaseIntensity * 0.25f);
            Vector3 lightColor = IsMidnightStrike
                ? new Vector3(0.6f, 0.2f, 0.2f)
                : new Vector3(0.35f, 0.45f, 0.6f);
            Lighting.AddLight(Projectile.Center, lightColor * lightMult * pulse);
        }

        private NPC FindClosestNPC(float maxDist)
        {
            NPC closest = null;
            float closestDist = maxDist;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy()) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }
            return closest;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 hitPos = target.Center;

            int sparkCount = IsMidnightStrike ? 12 : 6;
            for (int i = 0; i < sparkCount; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(IsMidnightStrike ? 7f : 4f, IsMidnightStrike ? 7f : 4f);
                Color col;
                if (IsMidnightStrike)
                    col = i % 3 == 0 ? new Color(200, 50, 50) : (i % 3 == 1 ? new Color(150, 200, 255) : Color.White);
                else
                    col = i % 2 == 0 ? new Color(150, 200, 255) : new Color(240, 240, 255);
                Dust d = Dust.NewDustPerfect(hitPos, DustID.IceTorch, sparkVel, 0, col, IsMidnightStrike ? 0.8f : 0.5f);
                d.noGravity = true;
            }

            // Pearl accent on impact
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0, -1f);
                Dust d = Dust.NewDustPerfect(hitPos + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.WhiteTorch, vel, 0, new Color(240, 240, 255), 0.5f);
                d.noGravity = true;
            }

            try { ClairDeLuneVFXLibrary.SpawnMusicNotes(hitPos, IsMidnightStrike ? 2 : 1, 12f, 0.4f, 0.7f, 20); } catch { }
            try { ClairDeLuneVFXLibrary.SpawnMixedSparkleImpact(hitPos, IsMidnightStrike ? 1f : 0.6f, 4, 4); } catch { }
        }

        #region Rendering

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                IncisorOrbRenderer.DrawOrbVisuals(Main.spriteBatch, Projectile, IncisorOrbRenderer.ClairDeLune, ref _strip);
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

        #endregion

        public override void OnKill(int timeLeft)
        {
            int dustCount = IsMidnightStrike ? 8 : 4;
            for (int i = 0; i < dustCount; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(IsMidnightStrike ? 5f : 3f, IsMidnightStrike ? 5f : 3f);
                Color col;
                if (IsMidnightStrike)
                    col = Main.rand.NextBool() ? new Color(200, 50, 50) : Color.White;
                else
                    col = Main.rand.NextBool() ? new Color(150, 200, 255) : new Color(240, 240, 255);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.IceTorch, sparkVel, 0, col, 0.3f);
                d.noGravity = true;
            }

            try { ClairDeLuneVFXLibrary.SpawnMusicNotes(Projectile.Center, 1, 12f, 0.5f, 0.7f, 20); } catch { }
            try { ClairDeLuneVFXLibrary.SpawnMixedSparkleImpact(Projectile.Center, 0.5f, 4, 4); } catch { }
            try { ClairDeLuneVFXLibrary.SpawnLunarSparkles(Projectile.Center, 3, 15f); } catch { }
        }
    }
}
