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
using MagnumOpus.Content.DiesIrae;
using MagnumOpus.Content.DiesIrae.Weapons.StaffOfFinalJudgment.Utilities;

namespace MagnumOpus.Content.DiesIrae.Weapons.StaffOfFinalJudgment.Projectiles
{
    public class FloatingIgnitionProjectile : ModProjectile
    {
        // =====================================================================
        // STATE MACHINE
        // ai[0] = state: 0=flying, 1=hovering/arming, 2=armed/detecting, 3=detonating
        // ai[1] = mode flag: 0=mine, 1=direct shot (right-click)
        // =====================================================================

        private const float DecelerationFactor = 0.93f;
        private const int ArmFrames = 20;
        private const float DetectionRange = 120f;
        private const int DetonationFrames = 8;

        // Judgment Storm: static counter shared across all mines
        private static int _judgmentStormCounter;
        private static int _judgmentStormTimer;
        private const int JudgmentStormWindow = 60; // 1 second at 60fps
        private const int JudgmentStormThreshold = 3;

        private Player Owner => Main.player[Projectile.owner];
        private bool _initialized;
        private VertexStrip _strip;
        private int _stateTimer; // frames spent in current state
        private bool _hasDetonated;

        // Direct shot homing
        private const float DirectHomingRange = 350f;
        private const float DirectHomingStrength = 0.08f;
        private const float DirectMaxSpeed = 16f;

        public override string Texture => "MagnumOpus/Content/DiesIrae/Weapons/StaffOfFinalJudgment/StaffOfFinalJudgment";

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
            Projectile.timeLeft = 600; // 10 seconds for mines (plenty of hover time)
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }

        /// <summary>
        /// Called once per frame from UpdateFrame to tick the Judgment Storm timer.
        /// </summary>
        public static void UpdateJudgmentStormTimer()
        {
            if (_judgmentStormTimer > 0)
                _judgmentStormTimer--;
            else
                _judgmentStormCounter = 0;
        }

        private bool IsDirectShot => Projectile.ai[1] == 1f;
        private float State { get => Projectile.ai[0]; set => Projectile.ai[0] = value; }

        public override void AI()
        {
            // Tick the shared Judgment Storm timer (only once per frame via first active mine)
            UpdateJudgmentStormTimer();

            if (!_initialized)
            {
                _initialized = true;
                Projectile.rotation = Projectile.velocity.ToRotation();

                if (IsDirectShot)
                {
                    // Direct shot: shorter lifetime, standard penetrate
                    Projectile.timeLeft = 240;
                    State = -1f; // special state for direct shot
                }
            }

            if (IsDirectShot)
            {
                AI_DirectShot();
                return;
            }

            AI_Mine();
        }

        /// <summary>
        /// Direct shot behavior: fly straight with light homing, no deceleration.
        /// </summary>
        private void AI_DirectShot()
        {
            NPC target = StaffOfFinalJudgmentUtils.ClosestNPCAt(Projectile.Center, DirectHomingRange);
            if (target != null)
            {
                Vector2 desiredDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredDir * Projectile.velocity.Length(), DirectHomingStrength);
            }

            if (Projectile.velocity.Length() > DirectMaxSpeed)
                Projectile.velocity = Vector2.Normalize(Projectile.velocity) * DirectMaxSpeed;

            Projectile.rotation += 0.06f;
            SpawnFlightDust();

            float pulse = 1f + 0.15f * MathF.Sin(Projectile.timeLeft * 0.2f);
            Lighting.AddLight(Projectile.Center, new Vector3(0.6f, 0.2f, 0.1f) * 0.35f * pulse);
        }

        /// <summary>
        /// Mine state machine: Flying -> Hovering/Arming -> Armed/Detecting -> Detonating.
        /// </summary>
        private void AI_Mine()
        {
            _stateTimer++;

            switch ((int)State)
            {
                case 0: // Flying — initial velocity, decelerate to hover
                    AI_State_Flying();
                    break;

                case 1: // Hovering/Arming — slowing down, counting to arm threshold
                    AI_State_Arming();
                    break;

                case 2: // Armed/Detecting — stationary, scanning for enemies
                    AI_State_Armed();
                    break;

                case 3: // Detonating — brief expansion then kill
                    AI_State_Detonating();
                    break;
            }

            Projectile.rotation += State >= 2 ? 0.03f : 0.06f;
            SpawnMineDust();

            float lightIntensity = State >= 2 ? 0.5f : 0.35f;
            float pulse = 1f + 0.15f * MathF.Sin(Projectile.timeLeft * 0.2f);
            Lighting.AddLight(Projectile.Center, DiesIraePalette.InfernalRed.ToVector3() * lightIntensity * pulse);
        }

        private void AI_State_Flying()
        {
            // Decelerate toward hover
            Projectile.velocity *= DecelerationFactor;

            if (Projectile.velocity.LengthSquared() < 1f * 1f || _stateTimer >= 30)
            {
                // Transition to arming
                State = 1f;
                _stateTimer = 0;
            }
        }

        private void AI_State_Arming()
        {
            // Continue decelerating to full stop
            Projectile.velocity *= 0.9f;
            if (Projectile.velocity.LengthSquared() < 0.01f)
            {
                Projectile.velocity = Vector2.Zero;
                Projectile.tileCollide = false;
            }

            if (_stateTimer >= ArmFrames)
            {
                // Armed! Transition to detection state
                State = 2f;
                _stateTimer = 0;
                Projectile.tileCollide = false;

                // Arming VFX: small pulse burst
                if (!Main.dedServ)
                {
                    try { DiesIraeVFXLibrary.SpawnInfernalSparkles(Projectile.Center, 4, 12f); } catch { }
                    SoundEngine.PlaySound(SoundID.Item101 with { Volume = 0.5f, Pitch = 0.3f }, Projectile.Center);
                }
            }
            else if (!Main.dedServ && _stateTimer % 5 == 0)
            {
                // Arming tick VFX: small dust puff
                float armProgress = (float)_stateTimer / ArmFrames;
                Color tickColor = Color.Lerp(DiesIraePalette.BloodRed, DiesIraePalette.JudgmentGold, armProgress);
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.Torch, Vector2.Zero, 0, tickColor, 0.5f + armProgress * 0.3f);
                d.noGravity = true;
            }
        }

        private void AI_State_Armed()
        {
            // Stationary — scan for enemies in detection range
            Projectile.velocity = Vector2.Zero;

            // Ambient armed VFX: gentle pulsing ring of dust
            if (!Main.dedServ && _stateTimer % 8 == 0)
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 ringPos = Projectile.Center + angle.ToRotationVector2() * DetectionRange * 0.3f;
                Color ringCol = Color.Lerp(DiesIraePalette.InfernalRed, DiesIraePalette.JudgmentGold, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(ringPos, DustID.Torch,
                    (Projectile.Center - ringPos).SafeNormalize(Vector2.Zero) * 0.5f, 0, ringCol, 0.4f);
                d.noGravity = true;
            }

            // Proximity check
            NPC target = StaffOfFinalJudgmentUtils.ClosestNPCAt(Projectile.Center, DetectionRange);
            if (target != null)
            {
                // Triggered! Begin detonation
                State = 3f;
                _stateTimer = 0;

                if (!Main.dedServ)
                    SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.6f, Pitch = 0.4f }, Projectile.Center);
            }
        }

        private void AI_State_Detonating()
        {
            if (_stateTimer >= DetonationFrames && !_hasDetonated)
            {
                _hasDetonated = true;
                Detonate();
                Projectile.Kill();
            }
        }

        /// <summary>
        /// Core detonation: AoE damage + spawn homing child orbs.
        /// </summary>
        private void Detonate()
        {
            // Register detonation for Judgment Storm tracking
            _judgmentStormCounter++;
            _judgmentStormTimer = JudgmentStormWindow;

            bool isJudgmentStorm = _judgmentStormCounter >= JudgmentStormThreshold;
            int childCount = isJudgmentStorm ? 5 : 3;

            // Spawn child homing orbs toward nearby enemies
            SpawnChildOrbs(childCount);

            // Detonation VFX
            if (!Main.dedServ)
            {
                float intensity = isJudgmentStorm ? 1.5f : 1f;

                // Core explosion VFX
                try { DiesIraeVFXLibrary.SpawnInfernalExplosion(Projectile.Center, intensity); } catch { }
                try { DiesIraeVFXLibrary.SpawnGradientHaloRings(Projectile.Center, isJudgmentStorm ? 5 : 3, 0.3f * intensity); } catch { }
                try { DiesIraeVFXLibrary.SpawnMusicNotes(Projectile.Center, isJudgmentStorm ? 4 : 2, 25f, 0.6f, 1.0f, 30); } catch { }

                // Screen shake
                MagnumScreenEffects.AddScreenShake(isJudgmentStorm ? 6f : 3f);

                // Judgment Storm extra VFX
                if (isJudgmentStorm)
                {
                    try { DiesIraeVFXLibrary.SpawnFireHaloRings(Projectile.Center, 4, 0.35f); } catch { }
                    try { DiesIraeVFXLibrary.SpawnInfernalSwirl(Projectile.Center, 8, 80f, 0.8f); } catch { }

                    SoundEngine.PlaySound(SoundID.Item62 with { Volume = 0.8f, Pitch = -0.3f }, Projectile.Center);
                }

                // Dynamic lighting flash
                Lighting.AddLight(Projectile.Center, DiesIraePalette.WrathWhite.ToVector3() * intensity);
            }
        }

        /// <summary>
        /// Spawns homing child orbs targeting nearby enemies.
        /// </summary>
        private void SpawnChildOrbs(int count)
        {
            if (Main.myPlayer != Projectile.owner) return;

            var source = Projectile.GetSource_FromThis();
            int damage = (int)(Projectile.damage * 0.6f);
            float kb = Projectile.knockBack * 0.5f;

            for (int i = 0; i < count; i++)
            {
                // Spread child orbs in an arc
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 9f);

                GenericHomingOrbChild.SpawnChild(
                    source,
                    Projectile.Center, velocity,
                    damage, kb, Projectile.owner,
                    homingStrength: 0.10f,
                    behaviorFlags: GenericHomingOrbChild.FLAG_ACCELERATE,
                    themeIndex: GenericHomingOrbChild.THEME_DIESIRAE,
                    scaleMult: 0.8f,
                    timeLeft: 120
                );
            }
        }

        // =====================================================================
        // DUST HELPERS
        // =====================================================================

        private void SpawnFlightDust()
        {
            if (Main.rand.NextBool(3))
            {
                int dustType = Main.rand.NextBool() ? DustID.Torch : DustID.SolarFlare;
                Color dustColor = Main.rand.NextBool() ? new Color(255, 180, 50) : new Color(200, 40, 20);
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    dustType, -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    0, dustColor, 0.8f);
                d.noGravity = true;
                d.fadeIn = 0.6f;
            }
        }

        private void SpawnMineDust()
        {
            int state = (int)State;

            if (state <= 1)
            {
                // Flying/arming: standard flight dust
                SpawnFlightDust();
                return;
            }

            // Armed: ambient smoldering
            if (state == 2)
            {
                if (Main.rand.NextBool(4))
                {
                    Color col = Color.Lerp(DiesIraePalette.BloodRed, DiesIraePalette.InfernalRed, Main.rand.NextFloat());
                    Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                        DustID.Torch, Main.rand.NextVector2Circular(0.3f, 0.3f) + new Vector2(0, -0.4f),
                        0, col, 0.6f);
                    d.noGravity = true;
                }
                return;
            }

            // Detonating: intense fire burst
            if (state == 3)
            {
                for (int i = 0; i < 3; i++)
                {
                    Color col = Color.Lerp(DiesIraePalette.InfernalRed, DiesIraePalette.HellfireGold, Main.rand.NextFloat());
                    Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                        DustID.Torch, Main.rand.NextVector2CircularEdge(3f, 3f),
                        0, col, 1.2f);
                    d.noGravity = true;
                }
            }
        }

        // =====================================================================
        // HIT & KILL VFX (preserved from original)
        // =====================================================================

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 hitPos = target.Center;
            for (int i = 0; i < 6; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(4f, 4f);
                Color col = i % 2 == 0 ? new Color(200, 40, 20) : new Color(255, 180, 50);
                Dust d = Dust.NewDustPerfect(hitPos, DustID.Torch, sparkVel, 0, col, 0.5f);
                d.noGravity = true;
            }
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0, -1f);
                Dust d = Dust.NewDustPerfect(hitPos + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.SolarFlare, vel, 0, new Color(200, 40, 20), 0.5f);
                d.noGravity = true;
            }
            try { DiesIraeVFXLibrary.SpawnMusicNotes(hitPos, 1, 12f, 0.4f, 0.7f, 20); } catch { }
            try { DiesIraeVFXLibrary.SpawnMixedSparkleImpact(hitPos, 0.6f, 4, 4); } catch { }
        }

        // =====================================================================
        // RENDERING (preserved — IncisorOrbRenderer.DrawOrbVisuals)
        // =====================================================================

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, IncisorOrbRenderer.DiesIrae, ref _strip);
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
                Color col = Main.rand.NextBool() ? new Color(200, 40, 20) : new Color(255, 180, 50);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, sparkVel, 0, col, 0.3f);
                d.noGravity = true;
            }
            try { DiesIraeVFXLibrary.SpawnMusicNotes(Projectile.Center, 1, 12f, 0.5f, 0.7f, 20); } catch { }
            try { DiesIraeVFXLibrary.SpawnMixedSparkleImpact(Projectile.Center, 0.5f, 4, 4); } catch { }
            try { DiesIraeVFXLibrary.SpawnInfernalSparkles(Projectile.Center, 3, 15f); } catch { }
        }
    }
}
