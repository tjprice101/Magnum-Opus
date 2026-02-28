using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.InfernalChimesCalling.Utilities;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.InfernalChimesCalling.Particles;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.InfernalChimesCalling.Primitives;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.InfernalChimesCalling.Shaders;
using MagnumOpus.Content.LaCampanella.Debuffs;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.InfernalChimesCalling.Projectiles
{
    /// <summary>
    /// Summoner minion  Ean infernal bell choir entity with three attack modes:
    /// 1) Flame Slam  Echarges into enemies for melee contact damage
    /// 2) Fire Breath  Estationary cone of flame projectiles
    /// 3) Bell Ring  Eresonant AoE pulse that damages all nearby enemies
    /// Every 5th minion hit triggers MinionShockwaveProj via InfernalChimesCallingPlayer.
    /// </summary>
    public class CampanellaChoirMinion : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/InfernalChimesCalling/CampanellaChoirMinion";

        // AI states
        private enum MinionState { Idle, FlameSlamWindup, FlameSlam, FireBreath, BellRing }
        private MinionState CurrentState
        {
            get => (MinionState)(int)Projectile.ai[0];
            set => Projectile.ai[0] = (float)value;
        }
        private int StateTimer
        {
            get => (int)Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        // Constants
        private const float IdleHoverDistance = 80f;
        private const float DetectionRange = 800f;
        private const float SlamSpeed = 18f;
        private const int SlamWindupTime = 15;
        private const int SlamDuration = 25;
        private const int FireBreathDuration = 40;
        private const int BellRingDuration = 30;
        private const float BellRingRadius = 150f;
        private const int AttackCooldown = 30;

        private int attackCooldownTimer;
        private int targetNPC = -1;
        private float idleRotation;
        private Vector2 slamDirection;

        // Trail
        private List<Vector2> trailPositions = new List<Vector2>();
        private const int MaxTrailPoints = 16;
        private InfernalChimesPrimitiveRenderer trailRenderer;

        public override void SetStaticDefaults()
        {
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 36;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => CurrentState == MinionState.FlameSlam;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!CheckActive(owner)) return;

            // Update trail
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > MaxTrailPoints)
                trailPositions.RemoveAt(trailPositions.Count - 1);

            // Find target
            targetNPC = FindTarget(owner);

            switch (CurrentState)
            {
                case MinionState.Idle:
                    IdleBehavior(owner);
                    break;
                case MinionState.FlameSlamWindup:
                    FlameSlamWindupBehavior();
                    break;
                case MinionState.FlameSlam:
                    FlameSlamBehavior();
                    break;
                case MinionState.FireBreath:
                    FireBreathBehavior();
                    break;
                case MinionState.BellRing:
                    BellRingBehavior();
                    break;
            }

            // Ambient particles
            if (Main.rand.NextBool(8))
            {
                InfernalChimesParticleHandler.SpawnParticle(new ChoirEmberParticle(
                    Projectile.Center, Main.rand.NextFloat(MathHelper.TwoPi),
                    8f, Main.rand.NextFloat(0.04f, 0.08f), Main.rand.Next(30, 60)));
            }
            if (Main.rand.NextBool(25))
            {
                InfernalChimesParticleHandler.SpawnParticle(new MusicalChoirNoteParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(20, 20),
                    new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-1.5f, -0.5f)),
                    Main.rand.Next(50, 80)));
            }

            // Sprite direction
            if (targetNPC >= 0)
                Projectile.spriteDirection = Main.npc[targetNPC].Center.X > Projectile.Center.X ? 1 : -1;
            else
                Projectile.spriteDirection = owner.direction;
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<CampanellaChoirBuff>());
                return false;
            }
            if (owner.HasBuff(ModContent.BuffType<CampanellaChoirBuff>()))
                Projectile.timeLeft = 2;
            return true;
        }

        private int FindTarget(Player owner)
        {
            // Check for player-targeted NPC first
            if (owner.HasMinionAttackTargetNPC)
            {
                NPC target = Main.npc[owner.MinionAttackTargetNPC];
                if (target.CanBeChasedBy() && Vector2.Distance(Projectile.Center, target.Center) < DetectionRange * 1.5f)
                    return owner.MinionAttackTargetNPC;
            }

            int closest = -1;
            float closestDist = DetectionRange;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy()) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = i;
                }
            }
            return closest;
        }

        private void IdleBehavior(Player owner)
        {
            attackCooldownTimer--;
            idleRotation += 0.03f;

            // Hover near owner
            Vector2 idlePos = owner.Center + new Vector2(
                (float)Math.Cos(idleRotation) * IdleHoverDistance,
                -40f + (float)Math.Sin(idleRotation * 1.5f) * 15f);

            Vector2 toIdle = idlePos - Projectile.Center;
            float dist = toIdle.Length();
            if (dist > 600f)
                Projectile.Center = owner.Center; // Teleport if too far
            else if (dist > 4f)
                Projectile.velocity = toIdle * 0.08f;
            else
                Projectile.velocity *= 0.9f;

            // Attack decision
            if (targetNPC >= 0 && attackCooldownTimer <= 0)
            {
                NPC target = Main.npc[targetNPC];
                float targetDist = Vector2.Distance(Projectile.Center, target.Center);

                // Choose attack based on distance and randomness
                int choice = Main.rand.Next(3);
                if (targetDist < 200f) choice = 0; // Close range -> slam
                else if (targetDist > 450f) choice = 2; // Far range -> bell ring AoE

                switch (choice)
                {
                    case 0: // Flame Slam
                        CurrentState = MinionState.FlameSlamWindup;
                        StateTimer = 0;
                        slamDirection = Vector2.Normalize(target.Center - Projectile.Center);
                        break;
                    case 1: // Fire Breath
                        CurrentState = MinionState.FireBreath;
                        StateTimer = 0;
                        break;
                    case 2: // Bell Ring
                        CurrentState = MinionState.BellRing;
                        StateTimer = 0;
                        break;
                }
            }
        }

        private void FlameSlamWindupBehavior()
        {
            StateTimer++;
            // Pull back before slam
            Projectile.velocity = -slamDirection * 3f * (1f - (float)StateTimer / SlamWindupTime);

            // Charge particles
            if (StateTimer % 3 == 0)
            {
                InfernalChimesParticleHandler.SpawnParticle(new ChoirEmberParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(30, 30),
                    Main.rand.NextFloat(MathHelper.TwoPi), 15f, -0.1f, 20));
            }

            if (StateTimer >= SlamWindupTime)
            {
                CurrentState = MinionState.FlameSlam;
                StateTimer = 0;
                Projectile.velocity = slamDirection * SlamSpeed;
            }
        }

        private void FlameSlamBehavior()
        {
            StateTimer++;
            Projectile.velocity *= 0.96f;

            // Fire trail particles during slam
            if (StateTimer % 2 == 0)
            {
                InfernalChimesParticleHandler.SpawnParticle(new ChoirEmberParticle(
                    Projectile.Center, Main.rand.NextFloat(MathHelper.TwoPi),
                    5f, Main.rand.NextFloat(0.05f, 0.1f), Main.rand.Next(20, 40)));
            }

            if (StateTimer >= SlamDuration)
            {
                ReturnToIdle();
            }
        }

        private void FireBreathBehavior()
        {
            StateTimer++;
            Projectile.velocity *= 0.85f;

            // Face target
            if (targetNPC >= 0)
            {
                NPC target = Main.npc[targetNPC];
                Vector2 toTarget = Vector2.Normalize(target.Center - Projectile.Center);

                // Spawn fire breath particles in a cone
                if (StateTimer % 4 == 0 && StateTimer < FireBreathDuration - 10)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        float spread = Main.rand.NextFloat(-0.4f, 0.4f);
                        Vector2 breathDir = toTarget.RotatedBy(spread) * Main.rand.NextFloat(8f, 14f);
                        InfernalChimesParticleHandler.SpawnParticle(new ChoirEmberParticle(
                            Projectile.Center + toTarget * 20f,
                            Main.rand.NextFloat(MathHelper.TwoPi),
                            3f, Main.rand.NextFloat(0.02f, 0.06f), Main.rand.Next(20, 35)));
                    }

                    // Deal damage in cone area
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (!npc.CanBeChasedBy()) continue;
                        Vector2 toNPC = npc.Center - Projectile.Center;
                        float dist = toNPC.Length();
                        if (dist > 300f) continue;

                        float angle = Math.Abs(MathHelper.WrapAngle(toNPC.ToRotation() - toTarget.ToRotation()));
                        if (angle < 0.5f) // ~28 degree cone
                        {
                            int dmg = Projectile.damage;
                            npc.SimpleStrikeNPC(dmg, Projectile.Center.X < npc.Center.X ? 1 : -1, false, Projectile.knockBack);
                            npc.GetGlobalNPC<ResonantTollNPC>().AddStacks(npc, 1);
                            RegisterHit(npc);
                        }
                    }
                }
            }

            if (StateTimer >= FireBreathDuration)
                ReturnToIdle();
        }

        private void BellRingBehavior()
        {
            StateTimer++;
            Projectile.velocity *= 0.8f;

            // Expanding ring VFX
            if (StateTimer == 5)
            {
                InfernalChimesParticleHandler.SpawnParticle(new BellRingPulseParticle(
                    Projectile.Center, BellRingRadius, 25));

                // Musical notes burst
                for (int i = 0; i < 6; i++)
                {
                    Vector2 vel = Main.rand.NextVector2CircularEdge(3f, 3f) + new Vector2(0, -1f);
                    InfernalChimesParticleHandler.SpawnParticle(new MusicalChoirNoteParticle(
                        Projectile.Center, vel, Main.rand.Next(40, 70)));
                }
            }

            // Damage pulse at peak
            if (StateTimer == 10)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.CanBeChasedBy()) continue;
                    if (Vector2.Distance(Projectile.Center, npc.Center) <= BellRingRadius)
                    {
                        int dmg = (int)(Projectile.damage * 1.2f);
                        npc.SimpleStrikeNPC(dmg, Projectile.Center.X < npc.Center.X ? 1 : -1, false, Projectile.knockBack * 1.5f);
                        npc.GetGlobalNPC<ResonantTollNPC>().AddStacks(npc, 2);
                        RegisterHit(npc);
                    }
                }
            }

            if (StateTimer >= BellRingDuration)
                ReturnToIdle();
        }

        private void RegisterHit(NPC target)
        {
            Player owner = Main.player[Projectile.owner];
            var modPlayer = owner.GetModPlayer<InfernalChimesCallingPlayer>();
            if (modPlayer.RegisterMinionHit())
            {
                // 5th hit  Espawn shockwave
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), target.Center, Vector2.Zero,
                    ModContent.ProjectileType<MinionShockwaveProj>(), Projectile.damage * 2, 8f, Projectile.owner);
            }
        }

        private void ReturnToIdle()
        {
            CurrentState = MinionState.Idle;
            StateTimer = 0;
            attackCooldownTimer = AttackCooldown;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);
            RegisterHit(target);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;

            // Draw trail
            if (trailPositions.Count >= 2 && CurrentState == MinionState.FlameSlam)
            {
                DrawTrail(sb);
            }

            // Draw particles
            InfernalChimesParticleHandler.DrawAllParticles(sb);

            // Draw minion sprite
            var tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Color drawColor = Lighting.GetColor((int)(Projectile.Center.X / 16f), (int)(Projectile.Center.Y / 16f));

            // Glow effect based on state
            float glowIntensity = CurrentState switch
            {
                MinionState.FlameSlam => 0.6f,
                MinionState.FireBreath => 0.5f,
                MinionState.BellRing => 0.7f * (1f - (float)StateTimer / BellRingDuration),
                _ => 0.15f + (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.1f
            };

            drawColor = Color.Lerp(drawColor, Color.White, glowIntensity);

            SpriteEffects fx = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            sb.Draw(tex, drawPos, null, drawColor, Projectile.rotation, tex.Size() / 2f, Projectile.scale, fx, 0f);

            // Bloom overlay
            var bloomTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow").Value;
            Color bloomCol = InfernalChimesCallingUtils.ChoirPalette[1] * glowIntensity * 0.3f;
            sb.Draw(bloomTex, drawPos, null, bloomCol, 0f, bloomTex.Size() / 2f, 0.3f, SpriteEffects.None, 0f);

            return false;
        }

        private void DrawTrail(SpriteBatch sb)
        {
            if (trailPositions.Count < 3) return;

            try
            {
                trailRenderer ??= new InfernalChimesPrimitiveRenderer();

                Color colorStart = InfernalChimesCallingUtils.ChoirPalette[2];
                Color colorEnd = InfernalChimesCallingUtils.ChoirPalette[0] * 0.3f;

                var settings = new InfernalChimesPrimitiveRenderer.ChimesTrailSettings(
                    w: t => MathHelper.Lerp(20f, 2f, t),
                    c: t => Color.Lerp(colorStart, colorEnd, t),
                    s: null,
                    smooth: true
                );

                trailRenderer.RenderTrail(trailPositions.ToArray(), settings);
            }
            catch { }
        }

        public override void OnKill(int timeLeft)
        {
            trailRenderer?.Dispose();
            trailRenderer = null;

            // Death burst
            for (int i = 0; i < 8; i++)
            {
                InfernalChimesParticleHandler.SpawnParticle(new ChoirEmberParticle(
                    Projectile.Center, MathHelper.TwoPi / 8f * i,
                    5f, 0.08f, Main.rand.Next(20, 40)));
            }
        }
    }
}
