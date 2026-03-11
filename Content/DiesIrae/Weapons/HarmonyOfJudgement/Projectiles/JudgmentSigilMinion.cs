using MagnumOpus.Common;
using MagnumOpus.Content.DiesIrae;
using MagnumOpus.Content.DiesIrae.Weapons.HarmonyOfJudgement.Particles;
using MagnumOpus.Content.DiesIrae.Weapons.HarmonyOfJudgement.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.HarmonyOfJudgement.Projectiles
{
    /// <summary>
    /// Judgment Sigil Minion — autonomous Scan→Judge→Execute cycle.
    /// Floats near player, acquires targets within range, locks beam on them,
    /// processes through the judgment cycle, then delivers execute damage.
    /// 
    /// VFX: RadialNoiseMaskShader sigil body (VoronoiCell + StarField dual-noise),
    /// phase-driven beam (4px→6px→10px), RippleShader execute burst,
    /// XSlashShader harmonized verdict after 5 rapid executions.
    /// 
    /// AI fields:
    ///   ai[0] = current state (cast to JudgmentState)
    ///   ai[1] = state timer (frames in current phase)
    ///   localAI[0] = harmonized execution counter
    ///   localAI[1] = harmonized state timer (frames remaining)
    /// </summary>
    public class JudgmentSigilMinion : ModProjectile
    {
        // ═══════════════════════════════════════════════════════
        //  CONSTANTS
        // ═══════════════════════════════════════════════════════
        private const float DetectionRange = 900f;
        private const int ScanDuration = 60;     // 1s at 60fps
        private const int JudgeDuration = 30;    // 0.5s
        private const int ExecuteFlashFrames = 8;
        private const int CooldownDuration = 20;

        private const int HarmonizedThreshold = 5;
        private const int HarmonizedWindow = 600;     // 10s
        private const int HarmonizedStateDuration = 480; // 8s

        // ═══════════════════════════════════════════════════════
        //  STATE MACHINE
        // ═══════════════════════════════════════════════════════
        private enum JudgmentState { Idle, Scan, Judge, Execute, Cooldown }

        private JudgmentState CurrentState
        {
            get => (JudgmentState)(int)Projectile.ai[0];
            set => Projectile.ai[0] = (float)value;
        }

        private int StateTimer
        {
            get => (int)Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        private int TargetNPC { get; set; } = -1;

        private int HarmonizedCounter
        {
            get => (int)Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }

        private int HarmonizedTimer
        {
            get => (int)Projectile.localAI[1];
            set => Projectile.localAI[1] = value;
        }

        private bool IsHarmonized => HarmonizedTimer > 0;
        private float _seed;
        private int[] _recentExecutionTimestamps = new int[HarmonizedThreshold];
        private int _executionIndex = 0;

        public override void SetStaticDefaults()
        {
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 36;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minion = true;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
            Projectile.timeLeft = 2;
            _seed = Main.rand.NextFloat(100f);
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => false;

        // ═══════════════════════════════════════════════════════
        //  AI — JUDGMENT CYCLE
        // ═══════════════════════════════════════════════════════
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!CheckActive(player)) return;

            FloatNearPlayer(player);

            if (HarmonizedTimer > 0)
                HarmonizedTimer--;

            switch (CurrentState)
            {
                case JudgmentState.Idle:
                    AI_Idle();
                    break;
                case JudgmentState.Scan:
                    AI_Scan();
                    break;
                case JudgmentState.Judge:
                    AI_Judge();
                    break;
                case JudgmentState.Execute:
                    AI_Execute();
                    break;
                case JudgmentState.Cooldown:
                    AI_Cooldown();
                    break;
            }

            HarmonyOfJudgementUtils.DoAmbientParticles(Projectile.Center, GetCurrentPhase());
        }

        private bool CheckActive(Player player)
        {
            if (player.dead || !player.active)
            {
                player.ClearBuff(ModContent.BuffType<Buffs.HarmonyOfJudgementBuff>());
                Projectile.Kill();
                return false;
            }
            if (player.HasBuff(ModContent.BuffType<Buffs.HarmonyOfJudgementBuff>()))
                Projectile.timeLeft = 2;
            return true;
        }

        private void FloatNearPlayer(Player player)
        {
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

            Vector2 toTarget = targetPos - Projectile.Center;
            float dist = toTarget.Length();
            float speed = MathHelper.Clamp(dist * 0.08f, 2f, 15f);

            if (dist > 5f)
                Projectile.velocity = toTarget.SafeNormalize(Vector2.Zero) * speed;
            else
                Projectile.velocity *= 0.9f;
        }

        private void AI_Idle()
        {
            int target = FindBestTarget();
            if (target >= 0)
            {
                TargetNPC = target;
                CurrentState = JudgmentState.Scan;
                StateTimer = 0;
            }
        }

        private void AI_Scan()
        {
            StateTimer++;
            int duration = IsHarmonized ? ScanDuration / 2 : ScanDuration;
            if (!ValidateTarget()) { ResetToIdle(); return; }
            if (StateTimer >= duration)
            {
                CurrentState = JudgmentState.Judge;
                StateTimer = 0;
            }
        }

        private void AI_Judge()
        {
            StateTimer++;
            int duration = IsHarmonized ? JudgeDuration / 2 : JudgeDuration;
            if (!ValidateTarget()) { ResetToIdle(); return; }
            if (StateTimer >= duration)
            {
                CurrentState = JudgmentState.Execute;
                StateTimer = 0;
            }
        }

        private void AI_Execute()
        {
            StateTimer++;
            if (StateTimer == 1)
            {
                if (ValidateTarget())
                {
                    NPC target = Main.npc[TargetNPC];
                    float dmgMultiplier = IsHarmonized ? 1.5f : 1.0f;
                    int sigilsOnTarget = CountSigilsOnTarget(TargetNPC);
                    if (sigilsOnTarget > 1) dmgMultiplier *= 2f;

                    if (Main.myPlayer == Projectile.owner)
                    {
                        int damage = (int)(Projectile.damage * dmgMultiplier);
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            Projectile.Center,
                            (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY) * 20f,
                            ModContent.ProjectileType<JudgmentRayProjectile>(),
                            damage, Projectile.knockBack, Projectile.owner,
                            TargetNPC, IsHarmonized ? 1f : 0f);
                    }

                    HarmonyOfJudgementUtils.DoExecuteBurst(target.Center, IsHarmonized);
                    TrackExecution();
                }
            }
            if (StateTimer >= ExecuteFlashFrames)
            {
                CurrentState = JudgmentState.Cooldown;
                StateTimer = 0;
            }
        }

        private void AI_Cooldown()
        {
            StateTimer++;
            if (StateTimer >= CooldownDuration) ResetToIdle();
        }

        // ═══════════════════════════════════════════════════════
        //  TARGET ACQUISITION
        // ═══════════════════════════════════════════════════════
        private int FindBestTarget()
        {
            float bestDist = DetectionRange;
            int bestIndex = -1;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.CountsAsACritter) continue;
                if (!npc.CanBeChasedBy()) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < bestDist && Collision.CanHitLine(Projectile.Center, 1, 1, npc.position, npc.width, npc.height))
                {
                    bestDist = dist;
                    bestIndex = i;
                }
            }
            return bestIndex;
        }

        private bool ValidateTarget()
        {
            if (TargetNPC < 0 || TargetNPC >= Main.maxNPCs) return false;
            NPC npc = Main.npc[TargetNPC];
            if (!npc.active || npc.friendly || npc.dontTakeDamage) return false;
            return Vector2.Distance(Projectile.Center, npc.Center) <= DetectionRange * 1.5f;
        }

        private int CountSigilsOnTarget(int npcIndex)
        {
            int count = 0;
            foreach (var proj in Main.ActiveProjectiles)
            {
                if (proj.type == Projectile.type && proj.owner == Projectile.owner && proj.active && proj.whoAmI != Projectile.whoAmI)
                {
                    var otherSigil = proj.ModProjectile as JudgmentSigilMinion;
                    if (otherSigil?.TargetNPC == npcIndex &&
                        (otherSigil.CurrentState == JudgmentState.Execute || otherSigil.CurrentState == JudgmentState.Judge))
                        count++;
                }
            }
            return count + 1;
        }

        private void ResetToIdle()
        {
            CurrentState = JudgmentState.Idle;
            StateTimer = 0;
            TargetNPC = -1;
        }

        // ═══════════════════════════════════════════════════════
        //  HARMONIZED STATE TRACKING
        // ═══════════════════════════════════════════════════════
        private void TrackExecution()
        {
            int currentTime = (int)Main.GameUpdateCount;
            _recentExecutionTimestamps[_executionIndex % HarmonizedThreshold] = currentTime;
            _executionIndex++;

            if (_executionIndex >= HarmonizedThreshold)
            {
                int oldest = _recentExecutionTimestamps[_executionIndex % HarmonizedThreshold];
                if (currentTime - oldest <= HarmonizedWindow && !IsHarmonized)
                {
                    HarmonizedTimer = HarmonizedStateDuration;
                    SigilParticleHandler.Spawn(new ExecuteBurstParticle(
                        Projectile.Center, HarmonyOfJudgementUtils.HarmonizedGold, 0.05f, 0.5f, 30));
                }
            }
        }

        // ═══════════════════════════════════════════════════════
        //  RENDERING
        // ═══════════════════════════════════════════════════════
        private HarmonyOfJudgementUtils.SigilPhase GetCurrentPhase()
        {
            if (IsHarmonized) return HarmonyOfJudgementUtils.SigilPhase.Harmonized;
            return CurrentState switch
            {
                JudgmentState.Scan => HarmonyOfJudgementUtils.SigilPhase.Scan,
                JudgmentState.Judge => HarmonyOfJudgementUtils.SigilPhase.Judge,
                JudgmentState.Execute => HarmonyOfJudgementUtils.SigilPhase.Execute,
                _ => HarmonyOfJudgementUtils.SigilPhase.Idle
            };
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            // ── MINION SPRITE: Draw base PNG sprite ──
            Texture2D minionTex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 minionOrigin = minionTex.Size() / 2f;
            sb.Draw(minionTex, drawPos, null, lightColor * Projectile.Opacity, Projectile.rotation, minionOrigin, Projectile.scale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

            float timer = (float)Main.timeForVisualEffects;
            var phase = GetCurrentPhase();

            HarmonyOfJudgementUtils.DrawJudgmentAura(sb, Projectile.Center, timer);

            if (ValidateTarget() && CurrentState != JudgmentState.Idle && CurrentState != JudgmentState.Cooldown)
            {
                NPC target = Main.npc[TargetNPC];
                float beamAlpha = CurrentState switch
                {
                    JudgmentState.Scan => 0.6f,
                    JudgmentState.Judge => 0.8f,
                    JudgmentState.Execute => 1f - (StateTimer / (float)ExecuteFlashFrames) * 0.8f,
                    _ => 0f
                };
                HarmonyOfJudgementUtils.DrawJudgmentBeam(sb, Projectile.Center, target.Center, phase, beamAlpha);
            }

            // DrawSigilBody handles its own batch changes internally
            HarmonyOfJudgementUtils.DrawSigilBody(sb, Projectile.Center, timer, phase, _seed);

            // Dies Irae theme accent layer
            HarmonyOfJudgementUtils.DrawThemeAccents(sb, Projectile.Center, 1f, 0.6f);


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
    }

    // ═══════════════════════════════════════════════════════════════
    //  JUDGMENT RAY PROJECTILE — Execute damage delivery
    // ═══════════════════════════════════════════════════════════════
    /// <summary>
    /// Homing damage projectile fired during Execute phase.
    /// ai[0] = target NPC index, ai[1] = harmonized flag
    /// </summary>
    public class JudgmentRayProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";
        private int TargetNPC => (int)Projectile.ai[0];
        private bool IsHarmonized => Projectile.ai[1] > 0;

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            if (TargetNPC >= 0 && TargetNPC < Main.maxNPCs)
            {
                NPC target = Main.npc[TargetNPC];
                if (target.active && !target.friendly)
                {
                    Vector2 dir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity.SafeNormalize(Vector2.UnitY), dir, 0.3f) * Projectile.velocity.Length();
                }
            }
            Projectile.rotation = Projectile.velocity.ToRotation();

            if (Main.rand.NextBool(2))
            {
                Color sparkColor = IsHarmonized ? DiesIraePalette.JudgmentGold : DiesIraePalette.InfernalRed;
                Vector2 sparkVel = Main.rand.NextVector2Circular(1.5f, 1.5f);
                SigilParticleHandler.Spawn(new JudgmentBeamSparkParticle(
                    Projectile.Center, sparkVel, sparkColor, 0.015f, 12));
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 180);

            // Dies Irae VFX: judgment beam impact
            DiesIraeVFXLibrary.MeleeImpact(target.Center, 0);
            DiesIraeVFXLibrary.SpawnContrastSparkle(target.Center, Projectile.velocity);

            for (int i = 0; i < 5; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(2f, 2f);
                Color sparkColor = Color.Lerp(DiesIraePalette.InfernalRed, DiesIraePalette.JudgmentGold, Main.rand.NextFloat());
                SigilParticleHandler.Spawn(new JudgmentBeamSparkParticle(
                    target.Center, sparkVel, sparkColor, 0.02f, 15));
            }
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }
}