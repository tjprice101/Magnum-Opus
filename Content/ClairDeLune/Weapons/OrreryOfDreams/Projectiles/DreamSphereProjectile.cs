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
using MagnumOpus.Content.ClairDeLune;
using MagnumOpus.Content.ClairDeLune.Weapons.OrreryOfDreams.Utilities;

namespace MagnumOpus.Content.ClairDeLune.Weapons.OrreryOfDreams.Projectiles
{
    /// <summary>
    /// Dream Sphere — Orbiting magic sub-projectile fired by Orrery of Dreams.
    /// ai[0] = orbit layer: 0=inner (60px, fast), 1=middle (120px, medium), 2=outer (180px, slow)
    /// ai[1] = state: 0=orbiting player, 1=released (aggressive homing)
    /// Each layer fires GenericHomingOrbChild at different rates while orbiting.
    /// Right-click releases all orbs for aggressive homing attack.
    /// Foundation-pattern rendering: safe SpriteBatch, IncisorOrbRenderer visuals.
    /// </summary>
    public class DreamSphereProjectile : ModProjectile
    {
        #region Properties

        // Orbit configuration per layer
        private static readonly float[] OrbitRadii = { 60f, 120f, 180f };
        private static readonly float[] OrbitSpeeds = { 0.06f, 0.04f, 0.025f }; // radians per frame
        private static readonly int[] FireRates = { 30, 60, 90 }; // frames between shots

        private const float ReleasedHomingStrength = 0.12f;
        private const float ReleasedMaxSpeed = 18f;
        private const float HomingRange = 600f;

        private Player Owner => Main.player[Projectile.owner];
        private bool _initialized;
        private float _orbitAngle;
        private int _fireTimer;
        private int _alignmentTimer;

        private VertexStrip _strip;

        #endregion

        public override string Texture => "MagnumOpus/Content/ClairDeLune/Weapons/OrreryOfDreams/OrreryOfDreams";

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
            Projectile.timeLeft = 600;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            int layer = (int)Projectile.ai[0];
            layer = Math.Clamp(layer, 0, 2);
            bool orbiting = Projectile.ai[1] == 0f;

            if (!_initialized)
            {
                _initialized = true;
                // Stagger initial orbit angles per layer so they don't overlap
                _orbitAngle = layer * (MathHelper.TwoPi / 3f);
                Projectile.rotation = Projectile.velocity.ToRotation();
            }

            if (orbiting)
            {
                OrbitalAI(layer);
            }
            else
            {
                ReleasedAI();
            }

            // Trail dust — moonlit theme
            if (Main.rand.NextBool(3))
            {
                int dustType = Main.rand.NextBool() ? DustID.IceTorch : DustID.WhiteTorch;
                Color dustColor = Main.rand.NextBool() ? new Color(150, 200, 255) : new Color(240, 240, 255);
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    dustType, -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    0, dustColor, 0.8f);
                d.noGravity = true;
                d.fadeIn = 0.6f;
            }

            // Pulsing light
            float pulse = 1f + 0.15f * (float)Math.Sin(Projectile.timeLeft * 0.2f);
            Lighting.AddLight(Projectile.Center, new Vector3(0.35f, 0.45f, 0.6f) * 0.35f * pulse);
        }

        private void OrbitalAI(int layer)
        {
            if (!Owner.active || Owner.dead)
            {
                Projectile.Kill();
                return;
            }

            // Hold timeLeft while orbiting
            Projectile.timeLeft = 600;

            // Advance orbit angle
            _orbitAngle += OrbitSpeeds[layer];

            // Calculate orbit position around player
            float radius = OrbitRadii[layer];
            Vector2 targetPos = Owner.MountedCenter + new Vector2(
                MathF.Cos(_orbitAngle) * radius,
                MathF.Sin(_orbitAngle) * radius * 0.6f // Slight elliptical flattening
            );

            // Smoothly move to orbit position
            Vector2 toTarget = targetPos - Projectile.Center;
            Projectile.velocity = toTarget * 0.2f;
            Projectile.rotation = _orbitAngle + MathHelper.PiOver2;

            // Fire GenericHomingOrbChild at layer-specific rate
            _fireTimer++;
            if (_fireTimer >= FireRates[layer])
            {
                _fireTimer = 0;

                NPC target = OrreryOfDreamsUtils.ClosestNPCAt(Projectile.Center, 600f);
                if (target != null)
                {
                    Vector2 fireVel = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 10f;
                    GenericHomingOrbChild.SpawnChild(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center, fireVel,
                        Projectile.damage / 2, Projectile.knockBack * 0.5f, Projectile.owner,
                        homingStrength: 0.06f,
                        behaviorFlags: GenericHomingOrbChild.FLAG_ACCELERATE,
                        themeIndex: GenericHomingOrbChild.THEME_CLAIRDELUNE,
                        scaleMult: 0.7f,
                        timeLeft: 75);
                }
            }

            // Dream Alignment: every 720 frames (~12s), all orbit layers align and fire simultaneously
            _alignmentTimer++;
            if (_alignmentTimer >= 720)
            {
                _alignmentTimer = 0;

                // Force all DreamSphereProjectile orbs to same angle
                float alignAngle = _orbitAngle;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile p = Main.projectile[i];
                    if (!p.active || p.owner != Projectile.owner || p.type != Projectile.type || p.whoAmI == Projectile.whoAmI) continue;
                    if (p.ai[1] != 0f) continue; // Only orbiting orbs
                    if (p.ModProjectile is DreamSphereProjectile dsp)
                    {
                        dsp._orbitAngle = alignAngle;
                        dsp._fireTimer = FireRates[(int)Math.Clamp(p.ai[0], 0, 2)] - 1; // Force fire next frame
                    }
                }
                _fireTimer = FireRates[layer] - 1; // This orb also fires next frame

                // Alignment VFX burst
                for (int k = 0; k < 8; k++)
                {
                    Vector2 sparkVel = Main.rand.NextVector2CircularEdge(5f, 5f);
                    Color col = k % 2 == 0 ? new Color(150, 200, 255) : new Color(240, 240, 255);
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch, sparkVel, 0, col, 0.7f);
                    d.noGravity = true;
                }
            }
        }

        private void ReleasedAI()
        {
            // Aggressive homing toward nearest enemy
            NPC target = OrreryOfDreamsUtils.ClosestNPCAt(Projectile.Center, HomingRange);
            if (target != null)
            {
                Vector2 idealDir = (target.Center - Projectile.Center).SafeNormalize(Projectile.velocity.SafeNormalize(Vector2.UnitX));
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, idealDir * ReleasedMaxSpeed, ReleasedHomingStrength);
            }

            // Cap speed
            if (Projectile.velocity.Length() > ReleasedMaxSpeed)
                Projectile.velocity = Vector2.Normalize(Projectile.velocity) * ReleasedMaxSpeed;

            // If velocity is zero (just released from orbit), give initial push toward nearest enemy or cursor
            if (Projectile.velocity.LengthSquared() < 1f)
            {
                if (target != null)
                    Projectile.velocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 12f;
                else
                    Projectile.velocity = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitX) * 12f;
            }

            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 hitPos = target.Center;

            // Impact sparks — moonlit dual tone
            for (int i = 0; i < 6; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(4f, 4f);
                Color col = i % 2 == 0 ? new Color(150, 200, 255) : new Color(240, 240, 255);
                Dust d = Dust.NewDustPerfect(hitPos, DustID.IceTorch, sparkVel, 0, col, 0.5f);
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

            try { ClairDeLuneVFXLibrary.SpawnMusicNotes(hitPos, 1, 12f, 0.4f, 0.7f, 20); } catch { }
            try { ClairDeLuneVFXLibrary.SpawnMixedSparkleImpact(hitPos, 0.6f, 4, 4); } catch { }
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
            // Death VFX — moonlit spark burst
            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(3f, 3f);
                Color col = Main.rand.NextBool() ? new Color(150, 200, 255) : new Color(240, 240, 255);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.IceTorch, sparkVel, 0, col, 0.3f);
                d.noGravity = true;
            }

            try { ClairDeLuneVFXLibrary.SpawnMusicNotes(Projectile.Center, 1, 12f, 0.5f, 0.7f, 20); } catch { }
            try { ClairDeLuneVFXLibrary.SpawnMixedSparkleImpact(Projectile.Center, 0.5f, 4, 4); } catch { }
            try { ClairDeLuneVFXLibrary.SpawnLunarSparkles(Projectile.Center, 3, 15f); } catch { }
        }
    }
}
