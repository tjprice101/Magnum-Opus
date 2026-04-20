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
using MagnumOpus.Content.ClairDeLune.Weapons.StarfallWhisper.Utilities;

namespace MagnumOpus.Content.ClairDeLune.Weapons.StarfallWhisper.Projectiles
{
    /// <summary>
    /// Temporal Arrow — "Delayed Replay" projectile fired by Starfall Whisper.
    /// Fast straight shot (no homing). On hit: records impact position + velocity,
    /// then spawns a "replay" copy after 90 frames (1.5s delay) at the impact point.
    /// Max 3 replay generations via ai[1].
    ///
    /// ai[0] = delay countdown (0 = active, >0 = waiting to activate)
    /// ai[1] = generation counter (0=original, 1=first replay, 2=second, 3=third/max)
    /// localAI[0] = stored velocity X (for replays)
    /// localAI[1] = stored velocity Y (for replays)
    ///
    /// Foundation-pattern rendering: safe SpriteBatch, IncisorOrbRenderer visuals.
    /// </summary>
    public class TemporalArrowProjectile : ModProjectile
    {
        #region Properties

        private const float MaxSpeed = 16f;
        private const int MaxGeneration = 3;
        private const int ReplayDelay = 90; // 1.5 seconds

        private Player Owner => Main.player[Projectile.owner];
        private bool _initialized;

        private VertexStrip _strip;

        // Hit tracking for replay spawning
        private bool _hasHit;
        private Vector2 _hitPosition;
        private Vector2 _hitDirection;

        /// <summary>Whether this projectile is still in its delay countdown phase.</summary>
        private bool IsDelayed => Projectile.ai[0] > 0f;

        /// <summary>Current generation (0=original, 1-3=replays).</summary>
        private int Generation => (int)Projectile.ai[1];

        #endregion

        public override string Texture => "MagnumOpus/Content/ClairDeLune/Weapons/StarfallWhisper/StarfallWhisper";

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
            // === Delay phase: waiting to activate (replay copies) ===
            if (IsDelayed)
            {
                Projectile.ai[0]--;
                // Invisible, non-damaging, stationary during delay
                Projectile.velocity = Vector2.Zero;
                Projectile.hide = true;
                Projectile.friendly = false;
                Projectile.tileCollide = false;

                // Subtle "charging" dust at spawn point during delay
                if (Main.rand.NextBool(4))
                {
                    Color col = Main.rand.NextBool() ? new Color(150, 200, 255) : new Color(240, 240, 255);
                    Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                        DustID.IceTorch, Main.rand.NextVector2Circular(0.5f, 0.5f), 0, col, 0.4f);
                    d.noGravity = true;
                }

                // Pulsing light while waiting
                float chargePulse = 0.3f + 0.2f * MathF.Sin(Projectile.ai[0] * 0.1f);
                Lighting.AddLight(Projectile.Center, new Vector3(0.35f, 0.45f, 0.6f) * chargePulse);

                // When delay ends, activate with stored velocity
                if (Projectile.ai[0] <= 0f)
                {
                    Projectile.velocity = new Vector2(Projectile.localAI[0], Projectile.localAI[1]);
                    Projectile.hide = false;
                    Projectile.friendly = true;
                    Projectile.tileCollide = true;
                    Projectile.timeLeft = 120;
                    _initialized = false; // Re-trigger initialization VFX

                    // Activation burst VFX
                    for (int i = 0; i < 8; i++)
                    {
                        Vector2 sparkVel = Main.rand.NextVector2CircularEdge(5f, 5f);
                        Color col = i % 2 == 0 ? new Color(150, 200, 255) : new Color(240, 240, 255);
                        Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.IceTorch, sparkVel, 0, col, 0.6f);
                        d.noGravity = true;
                    }

                    try { ClairDeLuneVFXLibrary.SpawnMixedSparkleImpact(Projectile.Center, 0.8f, 5, 5); } catch { }
                }

                return;
            }

            // === Active phase: fast straight shot ===
            if (!_initialized)
            {
                _initialized = true;
                Projectile.rotation = Projectile.velocity.ToRotation();
            }

            // Fast Straight: high-speed linear arrow, no homing
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Trail dust — moonlit theme, slightly dimmer for higher generations
            float genFade = 1f - Generation * 0.15f;
            if (Main.rand.NextBool(3))
            {
                int dustType = Main.rand.NextBool() ? DustID.IceTorch : DustID.WhiteTorch;
                Color dustColor = Main.rand.NextBool() ? new Color(150, 200, 255) : new Color(240, 240, 255);
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    dustType, -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    0, dustColor, 0.8f * genFade);
                d.noGravity = true;
                d.fadeIn = 0.6f;
            }

            // Pulsing light
            float pulse = 1f + 0.15f * (float)Math.Sin(Projectile.timeLeft * 0.2f);
            Lighting.AddLight(Projectile.Center, new Vector3(0.35f, 0.45f, 0.6f) * 0.35f * pulse * genFade);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 hitPos = target.Center;

            // Record hit data for replay spawning
            if (!_hasHit && Generation < MaxGeneration)
            {
                _hasHit = true;
                _hitPosition = hitPos;
                _hitDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX) * MaxSpeed;
            }

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
            // Don't draw during delay phase
            if (IsDelayed)
                return false;

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
            // Spawn replay copy if we hit something and haven't exceeded max generations
            if (_hasHit && Generation < MaxGeneration && Main.myPlayer == Projectile.owner)
            {
                int nextGen = Generation + 1;
                float replayDamage = 0.75f; // 75% damage per generation

                int idx = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    _hitPosition,
                    Vector2.Zero, // Start stationary
                    Type,
                    (int)(Projectile.damage * replayDamage),
                    Projectile.knockBack * 0.5f,
                    Projectile.owner,
                    ai0: ReplayDelay, // Delay countdown
                    ai1: nextGen     // Generation counter
                );

                // Store the replay velocity in localAI
                if (idx >= 0 && idx < Main.maxProjectiles)
                {
                    Main.projectile[idx].localAI[0] = _hitDirection.X;
                    Main.projectile[idx].localAI[1] = _hitDirection.Y;
                }
            }

            // Death VFX — moonlit spark burst (only if not delayed)
            if (!IsDelayed)
            {
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
}
