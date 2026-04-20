using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.ClairDeLune;
using MagnumOpus.Content.ClairDeLune.Weapons.CogAndHammer.Utilities;

namespace MagnumOpus.Content.ClairDeLune.Weapons.CogAndHammer.Projectiles
{
    /// <summary>
    /// Clockwork Bomb — "Ticking Bombs" projectile fired by Cog and Hammer.
    /// State machine: flying -> landed/arming -> armed/detecting -> ticking -> detonate.
    ///
    /// State machine phases:
    ///   0 = Flying (gravity arc)
    ///   1 = Landed/Arming (15 frames)
    ///   2 = Armed/Detecting (proximity 100px)
    ///   3 = Ticking (3 ticks at 20 frames each, scale pulses)
    ///
    /// Foundation-pattern rendering: safe SpriteBatch, IncisorOrbRenderer visuals.
    /// </summary>
    public class ClockworkBombProjectile : ModProjectile
    {
        #region Properties

        private const float MaxSpeed = 16f;
        private const float ProximityRange = 100f;
        private const int ArmingDuration = 15;
        private const int TickDuration = 20;
        private const int MaxTicks = 3;
        private const float DetonationRadius = 120f;
        private const int MaxLifetime = 300; // 5 seconds self-detonate

        private Player Owner => Main.player[Projectile.owner];
        private bool _initialized;

        private VertexStrip _strip;

        // State machine
        private int _phase; // 0=flying, 1=arming, 2=armed, 3=ticking
        private int _phaseTimer;
        private int _tickCount;
        private int _totalLifeTimer;

        #endregion

        public override string Texture => "MagnumOpus/Content/ClairDeLune/Weapons/CogAndHammer/CogAndHammer";

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
            Projectile.penetrate = -1; // Infinite penetrate — damage dealt via manual AoE on detonation
            Projectile.timeLeft = 600; // Generous lifetime, self-managed via MaxLifetime
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override bool? CanHitNPC(NPC target)
        {
            // Only deal contact damage during detonation (handled in Detonate method)
            // Prevent normal contact damage during all phases
            return false;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (_phase == 0)
            {
                // Transition to landed/arming on tile contact
                TransitionToPhase(1);
                return false; // Don't kill
            }
            return false;
        }

        public override void AI()
        {
            _totalLifeTimer++;

            // Self-detonate after max lifetime
            if (_totalLifeTimer >= MaxLifetime && _phase < 3)
            {
                Detonate();
                return;
            }

            switch (_phase)
            {
                case 0: PhaseFlying(); break;
                case 1: PhaseArming(); break;
                case 2: PhaseArmed(); break;
                case 3: PhaseTicking(); break;
            }

            // Pulsing light — color shifts by phase
            float pulse = 1f + 0.15f * (float)Math.Sin(_totalLifeTimer * 0.2f);
            Vector3 lightColor = _phase switch
            {
                0 => new Vector3(0.35f, 0.45f, 0.6f),   // Blue while flying
                1 => new Vector3(0.4f, 0.4f, 0.3f),      // Warm while arming
                2 => new Vector3(0.5f, 0.3f, 0.2f),      // Orange while armed
                3 => new Vector3(0.6f, 0.2f, 0.15f),     // Red while ticking
                _ => new Vector3(0.35f, 0.45f, 0.6f),
            };
            Lighting.AddLight(Projectile.Center, lightColor * 0.4f * pulse);
        }

        private void PhaseFlying()
        {
            if (!_initialized)
            {
                _initialized = true;
                Projectile.rotation = Projectile.velocity.ToRotation();
            }

            // Gravity arc
            Projectile.velocity.Y += 0.15f;

            // Decelerate after initial flight
            if (_totalLifeTimer > 20)
            {
                Projectile.velocity *= 0.98f;
                if (Projectile.velocity.Length() < 0.5f)
                {
                    // Came to rest mid-air — transition to arming
                    TransitionToPhase(1);
                    return;
                }
            }

            if (Projectile.velocity.Length() > MaxSpeed * 1.5f)
                Projectile.velocity = Vector2.Normalize(Projectile.velocity) * MaxSpeed * 1.5f;

            Projectile.rotation += 0.08f;

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
        }

        private void PhaseArming()
        {
            Projectile.velocity = Vector2.Zero;
            Projectile.tileCollide = false;
            _phaseTimer++;

            // Gentle pulsing rotation during arming
            Projectile.rotation += 0.02f;

            // Arming sparkle dust
            if (Main.rand.NextBool(4))
            {
                Color col = new Color(200, 180, 100); // Brass arming glow
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    DustID.WhiteTorch, new Vector2(0, -0.5f), 0, col, 0.4f);
                d.noGravity = true;
            }

            if (_phaseTimer >= ArmingDuration)
                TransitionToPhase(2);
        }

        private void PhaseArmed()
        {
            Projectile.velocity = Vector2.Zero;
            Projectile.rotation += 0.01f; // Very slow idle rotation

            // Proximity detection — scan for enemies within range
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < ProximityRange)
                {
                    TransitionToPhase(3);
                    return;
                }
            }

            // Armed idle dust — subtle ring
            if (Main.rand.NextBool(6))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 dustPos = Projectile.Center + angle.ToRotationVector2() * ProximityRange * 0.8f;
                Dust d = Dust.NewDustPerfect(dustPos, DustID.WhiteTorch, Vector2.Zero, 0,
                    new Color(150, 200, 255) * 0.5f, 0.3f);
                d.noGravity = true;
                d.fadeIn = 0.2f;
            }
        }

        private void PhaseTicking()
        {
            Projectile.velocity = Vector2.Zero;
            _phaseTimer++;

            // Determine current tick (0, 1, 2)
            int currentTick = _phaseTimer / TickDuration;
            float tickProgress = (_phaseTimer % TickDuration) / (float)TickDuration;

            // Scale pulse per tick: 1.0 -> 1.2 -> 1.5
            float[] tickScales = { 1.0f, 1.2f, 1.5f };
            float baseScale = currentTick < MaxTicks ? tickScales[currentTick] : 1.5f;
            float scalePulse = baseScale + 0.1f * MathF.Sin(tickProgress * MathHelper.TwoPi * 2f);
            Projectile.scale = scalePulse;

            // Rotation speeds up with each tick
            Projectile.rotation += 0.04f * (1 + currentTick);

            // Tick dust — increasingly intense
            if (Main.rand.NextBool(Math.Max(1, 4 - currentTick)))
            {
                Color col = currentTick switch
                {
                    0 => new Color(200, 180, 100),  // Brass
                    1 => new Color(200, 120, 60),    // Dark orange
                    _ => new Color(200, 50, 50),     // Red
                };
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(2f + currentTick, 2f + currentTick);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch, sparkVel, 0, col, 0.5f + currentTick * 0.2f);
                d.noGravity = true;
            }

            // Detonate after 3 ticks complete
            if (_phaseTimer >= MaxTicks * TickDuration)
            {
                Detonate();
            }
        }

        private void TransitionToPhase(int newPhase)
        {
            _phase = newPhase;
            _phaseTimer = 0;
        }

        private void Detonate()
        {
            if (Main.myPlayer == Projectile.owner)
            {
                // AoE damage to all enemies within detonation radius
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < DetonationRadius)
                    {
                        // Deal damage with distance falloff
                        float falloff = 1f - (dist / DetonationRadius) * 0.4f;
                        int aoeDamage = (int)(Projectile.damage * falloff);
                        npc.SimpleStrikeNPC(aoeDamage, Projectile.Center.X < npc.Center.X ? 1 : -1,
                            false, Projectile.knockBack, Projectile.DamageType);
                    }
                }

                // Spawn 4 debris orbs via GenericHomingOrbChild (cardinal directions)
                for (int i = 0; i < 4; i++)
                {
                    float angle = MathHelper.PiOver2 * i + Main.rand.NextFloat(-0.15f, 0.15f);
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 10f);
                    GenericHomingOrbChild.SpawnChild(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center, vel,
                        (int)(Projectile.damage * 0.4f), Projectile.knockBack * 0.5f, Projectile.owner,
                        homingStrength: 0.05f,
                        behaviorFlags: GenericHomingOrbChild.FLAG_GRAVITY,
                        themeIndex: GenericHomingOrbChild.THEME_CLAIRDELUNE,
                        scaleMult: 0.7f,
                        timeLeft: 60
                    );
                }
            }

            // Detonation VFX
            for (int i = 0; i < 15; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(8f, 8f);
                Color col = i % 3 == 0 ? new Color(200, 50, 50) :
                            (i % 3 == 1 ? new Color(200, 180, 100) : Color.White);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.IceTorch, sparkVel, 0, col, 0.8f);
                d.noGravity = true;
            }

            // Inner blast ring
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi / 20f * i;
                Vector2 vel = angle.ToRotationVector2() * 5f;
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch, vel, 0,
                    new Color(240, 240, 255), 0.6f);
                d.noGravity = true;
            }

            try { ClairDeLuneVFXLibrary.SpawnMusicNotes(Projectile.Center, 3, 20f, 0.6f, 1.0f, 30); } catch { }
            try { ClairDeLuneVFXLibrary.SpawnMixedSparkleImpact(Projectile.Center, 1.2f, 6, 6); } catch { }
            try { ClairDeLuneVFXLibrary.SpawnLunarSparkles(Projectile.Center, 5, 25f); } catch { }

            Projectile.Kill();
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
            // Death VFX — moonlit spark burst (for non-detonation kills like timeLeft expiry)
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
