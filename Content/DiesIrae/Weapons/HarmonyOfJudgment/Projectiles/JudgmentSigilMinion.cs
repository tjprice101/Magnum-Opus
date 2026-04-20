using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.DiesIrae;
using MagnumOpus.Content.DiesIrae.Weapons.HarmonyOfJudgment.Utilities;

namespace MagnumOpus.Content.DiesIrae.Weapons.HarmonyOfJudgment.Projectiles
{
    /// <summary>
    /// Judgment sigil minion — implements the Autonomous Triad behavior.
    /// Each sigil cycles through three phases autonomously:
    ///   Scan (ai[0]=0) → Judge (ai[0]=1) → Execute (ai[0]=2) → back to Scan.
    /// ai[1] stores the target NPC index for cross-sigil coordination.
    /// Collective Judgment: 2+ sigils in Execute on same NPC → 2x damage.
    /// Harmonized Verdict: all 3 sigils executing same target → 5x damage, 2x scale.
    /// </summary>
    public class JudgmentSigilMinion : ModProjectile
    {
        // --- Phase constants (stored in ai[0]) ---
        private const int PHASE_SCAN = 0;
        private const int PHASE_JUDGE = 1;
        private const int PHASE_EXECUTE = 2;

        // --- Tuning constants ---
        private const float TargetSearchRange = 600f;
        private const float AttackHoverRange = 250f;
        private const float MaxSpeed = 16f;
        private const int ScanFireInterval = 60;
        private const int JudgeOrbInterval = 30; // 1 orb every 30 frames = 2 per 60 frames
        private const int JudgeHitsRequired = 3;
        private const float ScanOrbHoming = 0.08f;
        private const float JudgeOrbHoming = 0.10f;
        private const float ExecuteOrbHoming = 0.14f;
        private const float ExecuteOrbScale = 1.5f;
        private const float ExecuteAoeRadius = 100f;
        private const float HarmonizedVerdictScale = 2.0f;

        // --- Synced state (ai[] fields, visible to other projectiles for cross-sigil checks) ---
        private Player Owner => Main.player[Projectile.owner];
        private int Phase { get => (int)Projectile.ai[0]; set => Projectile.ai[0] = value; }
        private int TargetNpcIndex { get => (int)Projectile.ai[1]; set => Projectile.ai[1] = value; }

        // --- Private state ---
        private bool _initialized;
        private VertexStrip _strip;
        private int _phaseTimer;
        private int _judgeHitCount;
        private int _executeOrbIndex = -1;

        public override string Texture => "MagnumOpus/Content/DiesIrae/Weapons/HarmonyOfJudgment/HarmonyOfJudgment";

        public override void SetStaticDefaults()
        {
            Main.projPet[Type] = true;
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minion = true;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => true;

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!CheckActive(player)) return;

            if (!_initialized)
            {
                _initialized = true;
                Projectile.rotation = Projectile.velocity.ToRotation();
                TargetNpcIndex = -1;
            }

            _phaseTimer++;

            // Validate current target — reset if dead, untargetable, or too far
            if (TargetNpcIndex >= 0)
            {
                NPC currentTarget = Main.npc[TargetNpcIndex];
                if (!currentTarget.active || !currentTarget.CanBeChasedBy() ||
                    Vector2.Distance(Projectile.Center, currentTarget.Center) > TargetSearchRange * 2f)
                {
                    ResetToScan();
                }
            }

            // --- Phase state machine ---
            switch (Phase)
            {
                case PHASE_SCAN:
                    RunScanPhase();
                    break;
                case PHASE_JUDGE:
                    RunJudgePhase();
                    break;
                case PHASE_EXECUTE:
                    RunExecutePhase();
                    break;
                default:
                    ResetToScan();
                    break;
            }

            // --- Movement ---
            NPC moveTarget = (TargetNpcIndex >= 0 && TargetNpcIndex < Main.maxNPCs)
                ? Main.npc[TargetNpcIndex] : null;

            if (moveTarget != null && moveTarget.active && moveTarget.CanBeChasedBy())
            {
                // Hover at range from target, closer during Execute
                float hoverDist = Phase == PHASE_EXECUTE ? AttackHoverRange * 0.7f : AttackHoverRange;
                Vector2 toTarget = moveTarget.Center - Projectile.Center;
                float dist = toTarget.Length();

                if (dist > hoverDist + 50f)
                {
                    // Approach target
                    Vector2 desiredDir = toTarget.SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredDir * MaxSpeed * 0.8f, 0.06f);
                }
                else if (dist < hoverDist - 50f)
                {
                    // Retreat to maintain range
                    Vector2 awayDir = (-toTarget).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, awayDir * MaxSpeed * 0.5f, 0.04f);
                }
                else
                {
                    // Gentle orbit at hover distance
                    float orbitAngle = (float)Main.timeForVisualEffects * 0.02f + Projectile.whoAmI * 2.1f;
                    Vector2 orbitOffset = new Vector2(MathF.Cos(orbitAngle), MathF.Sin(orbitAngle)) * 30f;
                    Vector2 idealPos = moveTarget.Center - toTarget.SafeNormalize(Vector2.UnitX) * hoverDist + orbitOffset;
                    Vector2 toIdeal = idealPos - Projectile.Center;
                    float speed = MathHelper.Clamp(toIdeal.Length() * 0.06f, 1f, MaxSpeed * 0.6f);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdeal.SafeNormalize(Vector2.Zero) * speed, 0.08f);
                }
            }
            else
            {
                // Hover near player when no target — orbit pattern
                int sigilIndex = 0, sigilCount = 0;
                foreach (var proj in Main.ActiveProjectiles)
                {
                    if (proj.type == Projectile.type && proj.owner == Projectile.owner && proj.active)
                    {
                        if (proj.whoAmI == Projectile.whoAmI) sigilIndex = sigilCount;
                        sigilCount++;
                    }
                }

                float angle = (float)Main.timeForVisualEffects * 0.015f + sigilIndex * MathHelper.TwoPi / Math.Max(sigilCount, 1);
                float orbitRadius = 80f + sigilIndex * 30f;
                Vector2 targetPos = player.Center + new Vector2(MathF.Cos(angle) * orbitRadius, MathF.Sin(angle) * orbitRadius - 50f);

                Vector2 toTargetPos = targetPos - Projectile.Center;
                float distToTarget = toTargetPos.Length();
                if (distToTarget > 800f)
                    Projectile.Center = targetPos;
                else if (distToTarget > 5f)
                {
                    float speed = MathHelper.Clamp(distToTarget * 0.08f, 2f, 15f);
                    Projectile.velocity = toTargetPos.SafeNormalize(Vector2.Zero) * speed;
                }
                else
                    Projectile.velocity *= 0.9f;
            }

            if (Projectile.velocity.Length() > MaxSpeed)
                Projectile.velocity = Vector2.Normalize(Projectile.velocity) * MaxSpeed;

            Projectile.rotation = Projectile.velocity.ToRotation();

            // --- Trail dust (color varies by phase) ---
            if (Main.rand.NextBool(3))
            {
                int dustType = Main.rand.NextBool() ? DustID.Torch : DustID.SolarFlare;
                Color dustColor = Phase switch
                {
                    PHASE_JUDGE => Main.rand.NextBool() ? new Color(255, 200, 80) : new Color(200, 40, 20),
                    PHASE_EXECUTE => Main.rand.NextBool() ? new Color(255, 255, 200) : new Color(255, 80, 30),
                    _ => Main.rand.NextBool() ? new Color(255, 180, 50) : new Color(200, 40, 20),
                };
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    dustType, -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    0, dustColor, 0.8f);
                d.noGravity = true;
                d.fadeIn = 0.6f;
            }

            // --- Lighting (intensity increases with phase) ---
            float phaseIntensity = Phase switch
            {
                PHASE_JUDGE => 0.45f,
                PHASE_EXECUTE => 0.6f,
                _ => 0.35f,
            };
            float pulse = 1f + 0.15f * MathF.Sin(Main.GameUpdateCount * 0.2f);
            Lighting.AddLight(Projectile.Center, new Vector3(0.6f, 0.2f, 0.1f) * phaseIntensity * pulse);
        }

        #region Phase Logic — Scan / Judge / Execute

        private void RunScanPhase()
        {
            // Acquire target if we don't have one
            if (TargetNpcIndex < 0)
            {
                NPC target = HarmonyOfJudgmentUtils.ClosestNPCAt(Projectile.Center, TargetSearchRange);
                if (target != null)
                    TargetNpcIndex = target.whoAmI;
            }

            if (TargetNpcIndex < 0) return; // No enemies in range

            // Fire 1 homing orb every 60 frames
            if (_phaseTimer >= ScanFireInterval && _phaseTimer % ScanFireInterval == 0)
            {
                NPC target = Main.npc[TargetNpcIndex];
                if (target.active && target.CanBeChasedBy())
                {
                    SpawnOrb(target, ScanOrbHoming, 1.0f, 1);

                    // After first scan volley, transition to Judge
                    TransitionToJudge();
                }
            }
        }

        private void RunJudgePhase()
        {
            if (TargetNpcIndex < 0)
            {
                ResetToScan();
                return;
            }

            NPC target = Main.npc[TargetNpcIndex];
            if (!target.active || !target.CanBeChasedBy())
            {
                ResetToScan();
                return;
            }

            // Fire 1 orb every 30 frames (= 2 per 60 frames as specified)
            if (_phaseTimer > 0 && _phaseTimer % JudgeOrbInterval == 0)
            {
                SpawnOrb(target, JudgeOrbHoming, 1.0f, 1);
                _judgeHitCount++;
            }

            // After 3 hits → transition to Execute
            if (_judgeHitCount >= JudgeHitsRequired)
            {
                TransitionToExecute();
            }
        }

        private void RunExecutePhase()
        {
            if (TargetNpcIndex < 0)
            {
                ResetToScan();
                return;
            }

            NPC target = Main.npc[TargetNpcIndex];

            // Fire the execute orb once upon entering this phase
            if (_executeOrbIndex < 0 && _phaseTimer <= 1)
            {
                if (!target.active || !target.CanBeChasedBy())
                {
                    ResetToScan();
                    return;
                }

                // Calculate damage multiplier from Collective/Harmonized Judgment
                int damageMultiplier = GetExecuteDamageMultiplier(out bool isHarmonized);
                float orbScale = isHarmonized ? HarmonizedVerdictScale : ExecuteOrbScale;

                Vector2 dir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                Vector2 vel = dir * 10f;

                int idx = GenericHomingOrbChild.SpawnChild(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center, vel,
                    Projectile.damage * damageMultiplier, Projectile.knockBack, Projectile.owner,
                    homingStrength: ExecuteOrbHoming,
                    behaviorFlags: GenericHomingOrbChild.FLAG_ACCELERATE,
                    themeIndex: GenericHomingOrbChild.THEME_DIESIRAE,
                    scaleMult: orbScale,
                    timeLeft: 180);

                _executeOrbIndex = idx;

                // Dramatic VFX burst on execute fire
                if (!Main.dedServ)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        Vector2 sparkVel = Main.rand.NextVector2CircularEdge(5f, 5f);
                        Color col = i % 2 == 0 ? new Color(255, 80, 30) : new Color(255, 200, 80);
                        Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, sparkVel, 0, col, 1.0f);
                        d.noGravity = true;
                    }
                    try { DiesIraeVFXLibrary.SpawnMusicNotes(Projectile.Center, 2, 15f, 0.5f, 0.9f, 25); } catch { }
                }
            }

            // Track execute orb — when it hits or expires, spawn AoE and return to Scan
            if (_executeOrbIndex >= 0)
            {
                Projectile orb = Main.projectile[_executeOrbIndex];
                bool orbGone = !orb.active
                    || orb.type != ModContent.ProjectileType<GenericHomingOrbChild>()
                    || orb.owner != Projectile.owner;

                if (orbGone)
                {
                    // Spawn AoE at target position (or fallback to sigil position)
                    Vector2 aoePos = (target.active && target.CanBeChasedBy()) ? target.Center : Projectile.Center;
                    int aoeDamageMultiplier = GetExecuteDamageMultiplier(out _);

                    GenericDamageZone.SpawnZone(
                        Projectile.GetSource_FromThis(),
                        aoePos,
                        Projectile.damage * aoeDamageMultiplier,
                        Projectile.knockBack,
                        Projectile.owner,
                        modeFlags: 0,
                        radius: ExecuteAoeRadius,
                        themeIndex: GenericHomingOrbChild.THEME_DIESIRAE,
                        durationFrames: 45);

                    // AoE impact VFX
                    if (!Main.dedServ)
                    {
                        try { DiesIraeVFXLibrary.SpawnMixedSparkleImpact(aoePos, 1.2f, 8, 6); } catch { }
                        try { DiesIraeVFXLibrary.SpawnInfernalSparkles(aoePos, 6, 20f); } catch { }
                    }

                    _executeOrbIndex = -1;
                    ResetToScan();
                    return;
                }
            }

            // Timeout safety — if execute orb hasn't resolved in 240 frames, reset
            if (_phaseTimer > 240)
            {
                _executeOrbIndex = -1;
                ResetToScan();
            }
        }

        #endregion

        #region Phase Transitions

        private void ResetToScan()
        {
            Phase = PHASE_SCAN;
            _phaseTimer = 0;
            _judgeHitCount = 0;
            _executeOrbIndex = -1;
            TargetNpcIndex = -1;
        }

        private void TransitionToJudge()
        {
            Phase = PHASE_JUDGE;
            _phaseTimer = 0;
            _judgeHitCount = 0;
        }

        private void TransitionToExecute()
        {
            Phase = PHASE_EXECUTE;
            _phaseTimer = 0;
            _executeOrbIndex = -1;
        }

        #endregion

        #region Orb Spawning & Damage Multiplier

        private void SpawnOrb(NPC target, float homingStrength, float scale, int damageMultiplier)
        {
            Vector2 dir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
            Vector2 vel = dir * 8f + Main.rand.NextVector2Circular(1f, 1f);

            GenericHomingOrbChild.SpawnChild(
                Projectile.GetSource_FromThis(),
                Projectile.Center, vel,
                Projectile.damage * damageMultiplier, Projectile.knockBack, Projectile.owner,
                homingStrength: homingStrength,
                behaviorFlags: 0,
                themeIndex: GenericHomingOrbChild.THEME_DIESIRAE,
                scaleMult: scale);
        }

        /// <summary>
        /// Checks other sigils owned by the same player to determine Execute damage multiplier.
        /// Collective Judgment: 2+ sigils in Execute targeting same NPC → 2x damage.
        /// Harmonized Verdict: all 3 sigils executing same target simultaneously → 5x damage + 2x scale.
        /// </summary>
        private int GetExecuteDamageMultiplier(out bool isHarmonized)
        {
            isHarmonized = false;
            int targetNpc = TargetNpcIndex;
            if (targetNpc < 0) return 1;

            int executeOnSameTarget = 0;
            int totalSigils = 0;

            foreach (var proj in Main.ActiveProjectiles)
            {
                if (proj.type == Projectile.type && proj.owner == Projectile.owner && proj.active)
                {
                    totalSigils++;
                    // Check if this sigil is in Execute phase targeting the same NPC
                    if ((int)proj.ai[0] == PHASE_EXECUTE && (int)proj.ai[1] == targetNpc)
                        executeOnSameTarget++;
                }
            }

            // Harmonized Verdict: all 3+ sigils execute same target simultaneously
            if (executeOnSameTarget >= 3 && totalSigils >= 3)
            {
                isHarmonized = true;
                return 5;
            }

            // Collective Judgment: 2+ sigils in Execute targeting same NPC
            if (executeOnSameTarget >= 2)
                return 2;

            return 1;
        }

        #endregion

        #region Unchanged — CheckActive, OnHitNPC, PreDraw, OnKill

        private bool CheckActive(Player player)
        {
            if (player.dead || !player.active)
            {
                player.ClearBuff(ModContent.BuffType<Buffs.HarmonyOfJudgmentBuff>());
                Projectile.Kill();
                return false;
            }
            if (player.HasBuff(ModContent.BuffType<Buffs.HarmonyOfJudgmentBuff>()))
                Projectile.timeLeft = 2;
            return true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 180);

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

        #endregion
    }
}
