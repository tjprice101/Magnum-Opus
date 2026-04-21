using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.DiesIrae;
using MagnumOpus.Content.DiesIrae.Systems;
using MagnumOpus.Content.DiesIrae.Weapons.HarmonyOfJudgment.Buffs;

namespace MagnumOpus.Content.DiesIrae.Weapons.HarmonyOfJudgment.Projectiles
{
    /// <summary>
    /// Judgment Sigil Minion — Autonomous Triad pattern.
    /// Three sigils cycle through phases:
    /// - Scan (0): Target nearest, fire 1 orb every 60 frames
    /// - Judge (1): Fire 2 orbs per 60 frames, track hits (3 hits → Execute)
    /// - Execute (2): Fire 1.5x scale orb with aggressive homing, 100px AoE on hit
    ///
    /// Collective Judgment: 2+ sigils executing same target = 2x damage
    /// Harmonized Verdict: 5 rapid executions within 10s = massive 2x scale, 5x damage orbs
    /// </summary>
    public class JudgmentSigilMinion : ModProjectile
    {
        private const int BaseFireRate = 60;
        private const int HitsForExecute = 3;
        private const int PhaseDuration = 180; // 3 seconds per phase max

        // Phase tracking: 0 = Scan, 1 = Judge, 2 = Execute
        private int Phase
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        private int CurrentTargetWhoAmI
        {
            get => (int)Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        private ref float HitCount => ref Projectile.localAI[0];
        private ref float FireTimer => ref Projectile.localAI[1];

        private float hoverAngle;
        private float pulseTimer;
        private int phaseTimer;
        private int sigilIndex; // Which sigil this is (for orbit spacing)
        private VertexStrip _strip;

        private DiesIraeCombatPlayer CombatPlayer => Main.player[Projectile.owner].GetModPlayer<DiesIraeCombatPlayer>();
        private bool InHarmonizedVerdict => CombatPlayer.HarmonizedVerdictActive;

        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        public override void SetStaticDefaults()
        {
            Main.projPet[Type] = true;
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => true;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!CheckActive(owner))
                return;

            // Determine sigil index for orbit spacing
            sigilIndex = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.type == Type && p.owner == Projectile.owner)
                {
                    if (p.whoAmI == Projectile.whoAmI)
                        break;
                    sigilIndex++;
                }
            }

            hoverAngle += 0.025f;
            pulseTimer += 0.08f;
            FireTimer++;
            phaseTimer++;

            NPC target = FindTarget(owner, 800f);

            // Update target tracking
            if (target != null)
            {
                if (CurrentTargetWhoAmI != target.whoAmI)
                {
                    // New target - reset to Scan phase
                    CurrentTargetWhoAmI = target.whoAmI;
                    HitCount = 0;
                    Phase = 0;
                    phaseTimer = 0;
                }
            }
            else
            {
                // No target - reset to idle
                CurrentTargetWhoAmI = -1;
                HitCount = 0;
                Phase = 0;
                phaseTimer = 0;
            }

            // Movement: Orbit owner with phase-based offset
            float orbitRadius = 80f + sigilIndex * 35f;
            float orbitSpeed = 0.02f + Phase * 0.01f;
            float orbitAngle = hoverAngle + sigilIndex * MathHelper.TwoPi / 3f;

            if (target != null)
            {
                // Move toward target when in combat
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                float approachSpeed = Phase == 2 ? 16f : 12f; // Faster during Execute
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * approachSpeed, 0.08f);
            }
            else
            {
                // Orbit around owner when idle
                Vector2 idealPos = owner.Center + new Vector2(MathF.Cos(orbitAngle), MathF.Sin(orbitAngle) * 0.6f) * orbitRadius;
                Vector2 toIdeal = idealPos - Projectile.Center;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdeal * 0.12f, 0.1f);
            }

            Projectile.rotation = Projectile.velocity.X * 0.02f;

            // Phase-based behavior
            if (target != null && Main.myPlayer == Projectile.owner)
            {
                int fireRate = InHarmonizedVerdict ? BaseFireRate / 2 : BaseFireRate;

                switch (Phase)
                {
                    case 0: // Scan
                        if (FireTimer >= fireRate)
                        {
                            FireTimer = 0;
                            FireScanOrb(target);
                        }
                        // Auto-advance to Judge after firing
                        if (phaseTimer >= PhaseDuration)
                        {
                            Phase = 1;
                            phaseTimer = 0;
                        }
                        break;

                    case 1: // Judge
                        if (FireTimer >= fireRate)
                        {
                            FireTimer = 0;
                            FireJudgeOrbs(target);
                        }
                        // Check for Execute transition
                        if (HitCount >= HitsForExecute)
                        {
                            Phase = 2;
                            phaseTimer = 0;
                        }
                        break;

                    case 2: // Execute
                        if (FireTimer >= fireRate)
                        {
                            FireTimer = 0;
                            FireExecuteOrb(target);

                            // Track execution for Harmonized Verdict
                            bool triggered = CombatPlayer.TrackHarmonyExecution();
                            if (triggered)
                            {
                                SoundEngine.PlaySound(SoundID.Item119 with { Pitch = -0.4f, Volume = 1.0f }, owner.Center);
                                DiesIraeVFXLibrary.FinisherSlam(owner.Center, 1.0f);
                            }

                            // Reset to Scan after Execute
                            Phase = 0;
                            HitCount = 0;
                            phaseTimer = 0;
                        }
                        break;
                }
            }

            // Ambient VFX based on phase
            if (Main.rand.NextBool(4 - Phase))
            {
                Color dustCol = Phase switch
                {
                    0 => DiesIraePalette.InfernalRed,
                    1 => DiesIraePalette.JudgmentGold,
                    2 => DiesIraePalette.WrathWhite,
                    _ => DiesIraePalette.InfernalRed
                };
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(16f, 16f),
                    DustID.Torch, -Projectile.velocity * 0.1f, 0, dustCol, 0.7f + Phase * 0.2f);
                d.noGravity = true;
            }

            // Harmonized Verdict sparks
            if (InHarmonizedVerdict && Main.rand.NextBool(2))
            {
                Dust spark = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f),
                    DustID.SolarFlare, Main.rand.NextVector2Circular(2f, 2f), 0, default, 0.8f);
                spark.noGravity = true;
            }

            // Lighting
            float intensity = 0.3f + Phase * 0.15f + (InHarmonizedVerdict ? 0.3f : 0f);
            float pulse = 0.85f + 0.15f * MathF.Sin(pulseTimer);
            Color phaseColor = Phase switch
            {
                0 => DiesIraePalette.InfernalRed,
                1 => DiesIraePalette.JudgmentGold,
                2 => DiesIraePalette.WrathWhite,
                _ => DiesIraePalette.InfernalRed
            };
            Lighting.AddLight(Projectile.Center, phaseColor.ToVector3() * intensity * pulse);
        }

        private void FireScanOrb(NPC target)
        {
            Vector2 vel = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 10f;

            GenericHomingOrbChild.SpawnChild(
                Projectile.GetSource_FromThis(),
                Projectile.Center, vel,
                Projectile.damage, Projectile.knockBack, Projectile.owner,
                homingStrength: 0.04f,
                behaviorFlags: 0,
                themeIndex: GenericHomingOrbChild.THEME_DIESIRAE,
                scaleMult: 0.9f,
                timeLeft: 90);

            SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0.2f, Volume = 0.5f }, Projectile.Center);
        }

        private void FireJudgeOrbs(NPC target)
        {
            for (int i = 0; i < 2; i++)
            {
                float angleOffset = (i - 0.5f) * 0.25f;
                Vector2 dir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                Vector2 vel = dir.RotatedBy(angleOffset) * 11f;

                GenericHomingOrbChild.SpawnChild(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center, vel,
                    Projectile.damage, Projectile.knockBack, Projectile.owner,
                    homingStrength: 0.06f,
                    behaviorFlags: GenericHomingOrbChild.FLAG_ACCELERATE,
                    themeIndex: GenericHomingOrbChild.THEME_DIESIRAE,
                    scaleMult: 1f,
                    timeLeft: 100);
            }

            SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0f, Volume = 0.6f }, Projectile.Center);
        }

        private void FireExecuteOrb(NPC target)
        {
            // Check for Collective Judgment (2+ sigils executing same target)
            int sigilsOnSameTarget = CountSigilsExecutingSameTarget(target);
            bool collectiveJudgment = sigilsOnSameTarget >= 2;

            // Harmonized Verdict massively empowers
            bool verdict = InHarmonizedVerdict;

            float damage = Projectile.damage;
            float scale = 1.5f;
            float homing = 0.12f;

            if (collectiveJudgment)
            {
                damage *= 2f;
                scale *= 1.2f;
            }
            if (verdict)
            {
                damage *= 5f;
                scale *= 2f;
                homing = 0.16f;
            }

            Vector2 vel = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 14f;

            // Spawn execute orb that creates AoE on hit
            GenericHomingOrbChild.SpawnChild(
                Projectile.GetSource_FromThis(),
                Projectile.Center, vel,
                (int)damage, Projectile.knockBack * 1.5f, Projectile.owner,
                homingStrength: homing,
                behaviorFlags: GenericHomingOrbChild.FLAG_ACCELERATE | GenericHomingOrbChild.FLAG_ZONE_ON_KILL,
                themeIndex: GenericHomingOrbChild.THEME_DIESIRAE,
                scaleMult: scale,
                timeLeft: 120);

            // VFX
            SoundStyle sound = verdict
                ? SoundID.Item119 with { Pitch = -0.3f, Volume = 0.9f }
                : collectiveJudgment
                    ? SoundID.Item73 with { Pitch = -0.1f, Volume = 0.8f }
                    : SoundID.Item73 with { Pitch = 0.1f, Volume = 0.7f };
            SoundEngine.PlaySound(sound, Projectile.Center);

            // Execute flash
            DiesIraeVFXLibrary.SpawnWrathBurst(Projectile.Center, verdict ? 10 : 6, verdict ? 1f : 0.7f);
        }

        private int CountSigilsExecutingSameTarget(NPC target)
        {
            int count = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.type == Type && p.owner == Projectile.owner)
                {
                    if ((int)p.ai[0] == 2 && (int)p.ai[1] == target.whoAmI) // Phase 2 (Execute) and same target
                        count++;
                }
            }
            return count;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Track hits for phase progression
            if (CurrentTargetWhoAmI == target.whoAmI && Phase == 1)
            {
                HitCount++;
            }

            // Fire debuff
            target.AddBuff(BuffID.OnFire3, 120 + Phase * 60);

            // Impact VFX
            int burstCount = 4 + Phase * 2;
            for (int i = 0; i < burstCount; i++)
            {
                Color col = Phase switch
                {
                    0 => DiesIraePalette.InfernalRed,
                    1 => DiesIraePalette.JudgmentGold,
                    2 => DiesIraePalette.WrathWhite,
                    _ => DiesIraePalette.InfernalRed
                };
                Dust d = Dust.NewDustPerfect(target.Center, DustID.Torch,
                    Main.rand.NextVector2CircularEdge(3f + Phase, 3f + Phase), 0, col, 0.8f);
                d.noGravity = true;
            }
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<HarmonyOfJudgmentBuff>());
                return false;
            }
            if (owner.HasBuff(ModContent.BuffType<HarmonyOfJudgmentBuff>()))
                Projectile.timeLeft = 2;
            return true;
        }

        private NPC FindTarget(Player owner, float range)
        {
            if (owner.HasMinionAttackTargetNPC)
            {
                NPC manual = Main.npc[owner.MinionAttackTargetNPC];
                if (manual.active && manual.CanBeChasedBy(Projectile) && Vector2.Distance(owner.Center, manual.Center) < range * 1.5f)
                    return manual;
            }
            NPC closest = null;
            float closestDist = range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy(Projectile))
                {
                    float dist = Vector2.Distance(owner.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }
            return closest;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            IncisorOrbRenderer.DrawOrbVisuals(Main.spriteBatch, Projectile, IncisorOrbRenderer.DiesIrae, ref _strip);

            // Phase indicator overlay: 3 orbiting dots + execute pulsing ring + Harmonized Verdict ring
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                float pulse = 0.9f + 0.1f * MathF.Sin(pulseTimer * 2f);
                float baseScale = InHarmonizedVerdict ? 1.4f : 1f;
                Vector2 drawPos = Projectile.Center - Main.screenPosition;

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);

                Texture2D ring = ModContent.Request<Texture2D>(
                    "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom",
                    ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                Vector2 ringOrigin = ring.Size() / 2f;

                Color coreCol = Phase switch
                {
                    0 => DiesIraePalette.InfernalRed,
                    1 => DiesIraePalette.JudgmentGold,
                    2 => DiesIraePalette.WrathWhite,
                    _ => DiesIraePalette.InfernalRed
                };

                // 3 phase dots orbiting
                for (int i = 0; i < 3; i++)
                {
                    float dotAngle = hoverAngle * 2f + i * MathHelper.TwoPi / 3f;
                    Vector2 dotOffset = new Vector2(MathF.Cos(dotAngle), MathF.Sin(dotAngle)) * 20f * baseScale;
                    Color dotCol = i <= Phase ? coreCol : (DiesIraePalette.CharcoalBlack * 0.5f);
                    float dotScale = i <= Phase ? 0.08f * pulse : 0.05f;
                    sb.Draw(ring, drawPos + dotOffset, null, (dotCol with { A = 0 }) * 0.6f,
                        0f, ringOrigin, dotScale * baseScale, SpriteEffects.None, 0f);
                }

                // Execute mode pulsing ring
                if (Phase == 2)
                {
                    Texture2D glow = ModContent.Request<Texture2D>(
                        "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow",
                        ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                    float ringScale = 0.35f + 0.08f * MathF.Sin(pulseTimer * 5f);
                    sb.Draw(glow, drawPos, null, (DiesIraePalette.HellfireGold with { A = 0 }) * 0.25f,
                        pulseTimer, glow.Size() / 2f, ringScale * baseScale, SpriteEffects.None, 0f);
                }

                // Harmonized Verdict outer ring
                if (InHarmonizedVerdict)
                {
                    Texture2D glow = ModContent.Request<Texture2D>(
                        "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow",
                        ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                    float verdictRing = 0.6f + 0.15f * MathF.Sin(pulseTimer * 8f);
                    sb.Draw(glow, drawPos, null, (DiesIraePalette.WrathWhite with { A = 0 }) * 0.2f,
                        -pulseTimer * 1.5f, glow.Size() / 2f, verdictRing, SpriteEffects.None, 0f);
                }
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
}
