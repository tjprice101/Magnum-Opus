using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.MoonlightSonata.Weapons.StaffOfTheLunarPhases.Utilities;
using MagnumOpus.Content.MoonlightSonata.Weapons.StaffOfTheLunarPhases.Particles;
using MagnumOpus.Content.MoonlightSonata.Weapons.StaffOfTheLunarPhases.Dusts;
using MagnumOpus.Content.MoonlightSonata.Weapons.StaffOfTheLunarPhases.Projectiles;
using MagnumOpus.Content.LaCampanella.Debuffs;
using ReLogic.Content;

namespace MagnumOpus.Content.MoonlightSonata.Minions
{
    /// <summary>
    /// Goliath of Moonlight 遯ｶ繝ｻA massive hovering lunar guardian minion.
    /// State machine AI: Idle 遶翫・Chase 遶翫・BeamAttack 遶翫・DevastatingBeam.
    /// Fires ricocheting moonlight beams that heal the owner and inflict Musical Dissonance.
    /// Conductor Mode (right-click toggle) directs beams toward cursor.
    /// Uses a 6x6 spritesheet animation (36 frames).
    /// Ambient VFX: gravitational rift shader overlay, orbiting cosmic motes, gravity well particles.
    /// </summary>
    public class GoliathOfMoonlight : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/MoonlightSonata/Minions/GoliathOfMoonlight/GoliathOfMoonlight";

        // =================================================================
        // ANIMATION CONSTANTS
        // =================================================================

        public const int FrameColumns = 6;
        public const int FrameRows = 6;
        public const int TotalFrames = 36;
        public const int FrameTime = 4;

        private int frameCounter = 0;
        private int currentFrame = 0;

        // =================================================================
        // AI CONSTANTS
        // =================================================================

        /// <summary>Range to detect enemies.</summary>
        public const float DetectionRange = 900f;

        /// <summary>Range to start chasing (from idle).</summary>
        public const float ChaseRange = 700f;

        /// <summary>Range at which beams can be fired.</summary>
        public const float BeamRange = 600f;

        /// <summary>Base attack cooldown in ticks (2 seconds).</summary>
        public const int BaseAttackCooldown = 120;

        /// <summary>Conductor Mode cooldown (faster, 1.5 sec).</summary>
        public const int ConductorAttackCooldown = 90;

        /// <summary>Beam charge-up duration in ticks.</summary>
        public const int ChargeUpDuration = 25;

        /// <summary>Hover height offset above player.</summary>
        public const float HoverHeight = -100f;

        /// <summary>Hover bob speed.</summary>
        public const float BobSpeed = 0.04f;

        /// <summary>Hover bob amplitude.</summary>
        public const float BobAmplitude = 8f;

        // =================================================================
        // STATE MACHINE
        // =================================================================

        private enum GoliathState
        {
            Idle = 0,
            Chase = 1,
            BeamAttack = 2,
            DevastatingBeam = 3
        }

        /// <summary>ai[0] = attack cooldown timer.</summary>
        public ref float AttackCooldown => ref Projectile.ai[0];

        /// <summary>ai[1] = current state (float cast of enum).</summary>
        public ref float StateValue => ref Projectile.ai[1];

        private GoliathState CurrentState
        {
            get => (GoliathState)(int)StateValue;
            set => StateValue = (float)value;
        }

        /// <summary>localAI[0] = charge-up timer for beam attacks.</summary>
        public ref float ChargeTimer => ref Projectile.localAI[0];

        /// <summary>localAI[1] = total alive time.</summary>
        public ref float AliveTime => ref Projectile.localAI[1];

        /// <summary>Tracked target NPC index.</summary>
        private int _targetNPC = -1;

        // =================================================================
        // SETUP
        // =================================================================

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 1;
            Main.projPet[Type] = true;
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 18000;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.minionSlots = 1f;
            Projectile.netImportant = true;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => true;

        // =================================================================
        // MAIN AI
        // =================================================================

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!CheckActive(owner))
                return;

            AliveTime++;
            UpdateAnimation();

            // Tick down attack cooldown
            if (AttackCooldown > 0)
                AttackCooldown--;

            // Find target
            _targetNPC = FindTargetIndex(owner);
            float distToPlayer = Vector2.Distance(Projectile.Center, owner.Center);

            // State machine
            switch (CurrentState)
            {
                case GoliathState.Idle:
                    RunIdleState(owner, distToPlayer);
                    break;
                case GoliathState.Chase:
                    RunChaseState(owner, distToPlayer);
                    break;
                case GoliathState.BeamAttack:
                    RunBeamAttackState(owner);
                    break;
                case GoliathState.DevastatingBeam:
                    RunDevastatingBeamState(owner);
                    break;
            }

            // Face movement direction
            if (Math.Abs(Projectile.velocity.X) > 0.3f)
                Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;
            else if (_targetNPC != -1)
                Projectile.spriteDirection = Main.npc[_targetNPC].Center.X > Projectile.Center.X ? 1 : -1;

            // Teleport safety
            if (distToPlayer > 2000f)
            {
                Projectile.Center = owner.Center + new Vector2(-50f * owner.direction, HoverHeight);
                Projectile.velocity = Vector2.Zero;
            }

            // Ambient lighting
            Color ambientCol = GoliathUtils.GetCosmicGradient(0.5f);
            Lighting.AddLight(Projectile.Center, ambientCol.ToVector3() * 0.3f);

            // Spawn ambient VFX
            SpawnAmbientVFX();
        }

        // =================================================================
        // STATE: IDLE
        // =================================================================

        private void RunIdleState(Player owner, float distToPlayer)
        {
            // Hover near player with gentle bob
            float bobOffset = MathF.Sin(AliveTime * BobSpeed) * BobAmplitude;
            Vector2 idleTarget = owner.Center + new Vector2(-60f * owner.direction, HoverHeight + bobOffset);

            SmoothFlyToward(idleTarget, 8f, 0.06f);

            // Transition to chase if enemy found
            if (_targetNPC != -1)
            {
                float distToTarget = Vector2.Distance(Projectile.Center, Main.npc[_targetNPC].Center);
                if (distToTarget < ChaseRange)
                {
                    CurrentState = GoliathState.Chase;
                    return;
                }
            }

            // Return to player if too far
            if (distToPlayer > 800f)
                SmoothFlyToward(owner.Center + new Vector2(0, HoverHeight), 14f, 0.1f);
        }

        // =================================================================
        // STATE: CHASE
        // =================================================================

        private void RunChaseState(Player owner, float distToPlayer)
        {
            if (_targetNPC == -1 || !Main.npc[_targetNPC].active)
            {
                CurrentState = GoliathState.Idle;
                return;
            }

            NPC target = Main.npc[_targetNPC];
            float distToTarget = Vector2.Distance(Projectile.Center, target.Center);

            // Fly toward target but maintain some distance (hover above)
            Vector2 attackPos = target.Center + new Vector2(0, -120f);
            SmoothFlyToward(attackPos, 10f, 0.08f);

            // If in beam range and cooldown ready, start beam attack
            if (distToTarget < BeamRange && AttackCooldown <= 0)
            {
                GoliathPlayer gp = owner.Goliath();
                if (gp.ConductorMode && gp.NextBeamIsDevastating)
                {
                    CurrentState = GoliathState.DevastatingBeam;
                    ChargeTimer = 0;
                }
                else
                {
                    CurrentState = GoliathState.BeamAttack;
                    ChargeTimer = 0;
                }
                return;
            }

            // Return to idle if target moves out of range or player is far
            if (distToTarget > DetectionRange || distToPlayer > 1200f)
            {
                CurrentState = GoliathState.Idle;
            }
        }

        // =================================================================
        // STATE: BEAM ATTACK (Standard Moonlight Beam)
        // =================================================================

        private void RunBeamAttackState(Player owner)
        {
            // Slow down during charge-up
            Projectile.velocity *= 0.92f;
            ChargeTimer++;

            // Charge-up VFX
            if (!Main.dedServ && ChargeTimer % 3 == 0)
            {
                float chargeProgress = ChargeTimer / (float)ChargeUpDuration;
                GoliathParticleHandler.Spawn(new GravityWellParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(40f, 40f),
                    Projectile.Center, 0.3f + chargeProgress * 0.3f,
                    15 + (int)(chargeProgress * 10)));
            }

            // Fire beam after charge-up complete
            if (ChargeTimer >= ChargeUpDuration)
            {
                FireMoonlightBeam(owner);
                GoliathPlayer gp = owner.Goliath();
                AttackCooldown = gp.ConductorMode ? ConductorAttackCooldown : BaseAttackCooldown;
                gp.OnBeamFired();
                CurrentState = _targetNPC != -1 ? GoliathState.Chase : GoliathState.Idle;
            }
        }

        // =================================================================
        // STATE: DEVASTATING BEAM (Conductor Mode enhanced)
        // =================================================================

        private void RunDevastatingBeamState(Player owner)
        {
            // Full stop during devastating charge
            Projectile.velocity *= 0.88f;
            ChargeTimer++;

            // Heavier charge-up VFX
            if (!Main.dedServ)
            {
                float chargeProgress = ChargeTimer / (float)(ChargeUpDuration + 10);

                if (ChargeTimer % 2 == 0)
                {
                    GoliathParticleHandler.Spawn(new GravityWellParticle(
                        Projectile.Center + Main.rand.NextVector2Circular(60f, 60f),
                        Projectile.Center, 0.4f + chargeProgress * 0.4f,
                        18 + (int)(chargeProgress * 12)));
                }

                if (ChargeTimer % 4 == 0)
                {
                    GoliathParticleHandler.Spawn(new ConductorGlyphParticle(
                        Projectile.Center + Main.rand.NextVector2Circular(30f, 30f),
                        Main.rand.NextVector2Circular(0.5f, 0.5f),
                        0.5f + chargeProgress * 0.3f, 25));
                }
            }

            if (ChargeTimer >= ChargeUpDuration + 10)
            {
                FireDevastatingBeam(owner);
                GoliathPlayer gp = owner.Goliath();
                AttackCooldown = ConductorAttackCooldown;
                gp.OnBeamFired();
                CurrentState = _targetNPC != -1 ? GoliathState.Chase : GoliathState.Idle;
            }
        }

        // =================================================================
        // BEAM FIRING
        // =================================================================

        private void FireMoonlightBeam(Player owner)
        {
            if (Projectile.owner != Main.myPlayer) return;

            int targetIdx = GetBeamTarget(owner);
            if (targetIdx == -1) return;

            GoliathPlayer gp = owner.Goliath();
            Vector2 toTarget = Main.npc[targetIdx].Center - Projectile.Center;
            toTarget.Normalize();
            float speedMult = GoliathPlayer.PhaseBeamSpeedMultiplier[gp.LunarPhaseMode];
            Vector2 beamVel = toTarget * GoliathMoonlightBeam.BeamSpeed * speedMult;

            int proj = Projectile.NewProjectile(
                Projectile.GetSource_FromThis(), Projectile.Center, beamVel,
                ModContent.ProjectileType<GoliathMoonlightBeam>(),
                (int)(Projectile.damage * GoliathPlayer.PhaseDamageMultiplier[gp.LunarPhaseMode]),
                Projectile.knockBack, Projectile.owner,
                GoliathMoonlightBeam.MaxBounces, -1);

            // Fire sound
            SoundEngine.PlaySound(SoundID.Item12 with { Volume = 0.5f, Pitch = 0.2f }, Projectile.Center);

            // Fire VFX 遯ｶ繝ｻcolored by lunar phase
            if (!Main.dedServ)
            {
                Color phaseColor = gp.CurrentPhaseColor;
                GoliathParticleHandler.Spawn(new ImpactBloomParticle(
                    Projectile.Center, Color.Lerp(GoliathUtils.IceBlueBrilliance, phaseColor, 0.3f), 0.6f, 12));

                // Lunar phase ring on fire
                GoliathParticleHandler.Spawn(new LunarPhaseRingParticle(
                    Projectile.Center, phaseColor, 0.8f, 15));

                // Music note on beam fire
                GoliathParticleHandler.Spawn(new MusicNoteParticle(
                    Projectile.Center, toTarget * 1.5f + new Vector2(0, -1f),
                    Color.Lerp(phaseColor, GoliathUtils.EnergyTendril, 0.4f),
                    0.35f, 35));
            }
        }

        private void FireDevastatingBeam(Player owner)
        {
            if (Projectile.owner != Main.myPlayer) return;

            int targetIdx = GetBeamTarget(owner);
            if (targetIdx == -1) return;

            GoliathPlayer gp = owner.Goliath();
            Vector2 toTarget = Main.npc[targetIdx].Center - Projectile.Center;
            toTarget.Normalize();
            Vector2 beamVel = toTarget * GoliathDevastatingBeam.BeamSpeed;

            int proj = Projectile.NewProjectile(
                Projectile.GetSource_FromThis(), Projectile.Center, beamVel,
                ModContent.ProjectileType<GoliathDevastatingBeam>(),
                (int)(Projectile.damage * GoliathPlayer.PhaseDamageMultiplier[gp.LunarPhaseMode]),
                Projectile.knockBack, Projectile.owner);

            // Heavier fire sound
            SoundEngine.PlaySound(SoundID.Item29 with { Volume = 0.6f, Pitch = -0.2f }, Projectile.Center);

            // Dramatic fire VFX 遯ｶ繝ｻphase tinted
            if (!Main.dedServ)
            {
                Color phaseColor = gp.CurrentPhaseColor;

                GoliathParticleHandler.Spawn(new ImpactBloomParticle(
                    Projectile.Center, GoliathUtils.SupermoonWhite, 1.0f, 18));
                GoliathParticleHandler.Spawn(new SummonGlowParticle(
                    Projectile.Center, Color.Lerp(GoliathUtils.NebulaPurple, phaseColor, 0.4f) * 0.5f, 1.5f, 22));

                // Lunar phase ring burst on devastating fire
                GoliathParticleHandler.Spawn(new LunarPhaseRingParticle(
                    Projectile.Center, phaseColor, 1.5f, 20));

                // Music note burst 遯ｶ繝ｻ3 notes for devastating beam
                for (int i = 0; i < 3; i++)
                {
                    float angle = MathHelper.TwoPi * i / 3f + Main.rand.NextFloat(-0.3f, 0.3f);
                    Vector2 noteVel = angle.ToRotationVector2() * (1.5f + Main.rand.NextFloat(1f));
                    GoliathParticleHandler.Spawn(new MusicNoteParticle(
                        Projectile.Center, noteVel,
                        Color.Lerp(phaseColor, GoliathUtils.StarCore, 0.3f),
                        0.4f + Main.rand.NextFloat(0.2f), 40));
                }
            }

            // Screen shake
            try
            {
                if (Projectile.owner == Main.myPlayer)
                {
                    var shaker = Main.LocalPlayer.GetModPlayer<ScreenShakePlayer>();
                    shaker.AddShake(4f, 10);
                }
            }
            catch { }
        }

        /// <summary>
        /// Get the target NPC index for beam attacks.
        /// In Conductor Mode, prioritize enemies near cursor.
        /// </summary>
        private int GetBeamTarget(Player owner)
        {
            GoliathPlayer gp = owner.Goliath();

            // Conductor Mode: target near cursor
            if (gp.ConductorMode)
            {
                int cursorTarget = GoliathUtils.ClosestNPCNearCursor(gp.ConductorTarget, 300f);
                if (cursorTarget != -1)
                    return cursorTarget;
            }

            // Normal targeting: use tracked target or find closest
            if (_targetNPC != -1 && Main.npc[_targetNPC].active && Main.npc[_targetNPC].CanBeChasedBy(Projectile))
                return _targetNPC;

            return GoliathUtils.ClosestNPCAt(Projectile.Center, BeamRange);
        }

        // =================================================================
        // MOVEMENT HELPER
        // =================================================================

        private void SmoothFlyToward(Vector2 target, float maxSpeed, float acceleration)
        {
            Vector2 direction = target - Projectile.Center;
            float distance = direction.Length();

            if (distance > 10f)
            {
                direction.Normalize();
                float speed = Math.Min(distance * 0.08f, maxSpeed);
                direction *= speed;
                Projectile.velocity = (Projectile.velocity * (1f / acceleration - 1f) + direction) / (1f / acceleration);
            }
            else
            {
                Projectile.velocity *= 0.9f;
            }
        }

        // =================================================================
        // ANIMATION
        // =================================================================

        private void UpdateAnimation()
        {
            frameCounter++;
            if (frameCounter >= FrameTime)
            {
                frameCounter = 0;
                currentFrame++;
                if (currentFrame >= TotalFrames)
                    currentFrame = 0;
            }
        }

        // =================================================================
        // BUFF / ACTIVE CHECK
        // =================================================================

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<GoliathOfMoonlightBuff>());
                Projectile.Kill();
                return false;
            }

            if (owner.HasBuff(ModContent.BuffType<GoliathOfMoonlightBuff>()))
                Projectile.timeLeft = 2;

            return true;
        }

        // =================================================================
        // TARGET FINDING
        // =================================================================

        private int FindTargetIndex(Player owner)
        {
            float maxDistance = DetectionRange;

            // Priority: player's manual target (right-click on enemy)
            if (owner.HasMinionAttackTargetNPC)
            {
                NPC target = Main.npc[owner.MinionAttackTargetNPC];
                if (target.CanBeChasedBy(Projectile) && Vector2.Distance(Projectile.Center, target.Center) < maxDistance)
                    return owner.MinionAttackTargetNPC;
            }

            // Conductor Mode: target near cursor
            GoliathPlayer gp = owner.Goliath();
            if (gp.ConductorMode)
            {
                int cursorTarget = GoliathUtils.ClosestNPCNearCursor(gp.ConductorTarget, 400f);
                if (cursorTarget != -1)
                    return cursorTarget;
            }

            // Default: find closest
            return GoliathUtils.ClosestNPCAt(Projectile.Center, maxDistance);
        }

        // =================================================================
        // AMBIENT VFX
        // =================================================================

        private void SpawnAmbientVFX()
        {
            if (Main.dedServ) return;

            Player owner = Main.player[Projectile.owner];
            GoliathPlayer gp = owner.Goliath();
            int lunarPhase = gp.LunarPhaseMode;
            Color phaseColor = GoliathPlayer.LunarPhaseColors[lunarPhase];

            // Foundation VFX: GoliathMaskAura — persistent aura overlay, refreshed every 100 ticks
            if (AliveTime % 100 == 1 && Projectile.owner == Main.myPlayer)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                    ModContent.ProjectileType<GoliathMaskAura>(),
                    0, 0f, Projectile.owner,
                    ai0: Projectile.whoAmI, ai1: lunarPhase);
            }

            // Orbiting rift motes 遯ｶ繝ｻevery 15 ticks, color tinted by lunar phase
            if (AliveTime % 15 == 0)
            {
                float startAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = 30f + Main.rand.NextFloat(20f);
                GoliathParticleHandler.Spawn(new RiftMoteParticle(
                    Projectile.Center, radius, startAngle,
                    0.3f + Main.rand.NextFloat(0.2f), 60 + Main.rand.Next(30)));
            }

            // Gravity well particles 遯ｶ繝ｻevery 20 ticks
            if (AliveTime % 20 == 0)
            {
                GoliathParticleHandler.Spawn(new GravityWellParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(50f, 50f),
                    Projectile.Center, 0.2f + Main.rand.NextFloat(0.15f),
                    30 + Main.rand.Next(20)));
            }

            // Cosmic dust 遯ｶ繝ｻevery 8 ticks
            if (AliveTime % 8 == 0)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(1f, 1f);
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    ModContent.DustType<GoliathDust>(), dustVel.X, dustVel.Y);
                Main.dust[d].noGravity = true;
                Main.dust[d].scale = 0.6f + Main.rand.NextFloat(0.3f);
            }

            // Lunar phase ring 遯ｶ繝ｻexpanding ring every 40 ticks showing current phase
            if (AliveTime % 40 == 0)
            {
                GoliathParticleHandler.Spawn(new LunarPhaseRingParticle(
                    Projectile.Center, phaseColor, 1.5f, 30));
            }

            // Phase-specific ambient particles
            switch (lunarPhase)
            {
                case 0: // New Moon 遯ｶ繝ｻsubtle void wisps
                    if (AliveTime % 25 == 0)
                    {
                        GoliathParticleHandler.Spawn(new GravityWellParticle(
                            Projectile.Center + Main.rand.NextVector2Circular(60f, 60f),
                            Projectile.Center, 0.15f, 40));
                    }
                    break;

                case 1: // Waxing 遯ｶ繝ｻgrowing energy sparks
                    if (AliveTime % 18 == 0)
                    {
                        GoliathParticleHandler.Spawn(new BeamSparkParticle(
                            Projectile.Center + Main.rand.NextVector2Circular(35f, 35f),
                            Main.rand.NextVector2Circular(0.5f, 0.5f),
                            0.25f + Main.rand.NextFloat(0.15f), 30));
                    }
                    break;

                case 2: // Full Moon 遯ｶ繝ｻradiant music notes + bright motes
                    if (AliveTime % 12 == 0)
                    {
                        GoliathParticleHandler.Spawn(new MusicNoteParticle(
                            Projectile.Center + Main.rand.NextVector2Circular(30f, 30f),
                            Main.rand.NextVector2Circular(1f, 1f) + new Vector2(0, -0.8f),
                            GoliathUtils.IceBlueBrilliance * 0.8f,
                            0.3f + Main.rand.NextFloat(0.2f), 50));
                    }
                    if (AliveTime % 20 == 0)
                    {
                        GoliathParticleHandler.Spawn(new BeamSparkParticle(
                            Projectile.Center + Main.rand.NextVector2Circular(40f, 40f),
                            Main.rand.NextVector2CircularEdge(1.5f, 1.5f),
                            0.3f, 25));
                    }
                    break;

                case 3: // Waning 遯ｶ繝ｻhealing orbs drifting toward player
                    if (AliveTime % 30 == 0)
                    {
                        GoliathParticleHandler.Spawn(new LunarHealingParticle(
                            Projectile.Center, owner.Center,
                            0.3f + Main.rand.NextFloat(0.15f), 40));
                    }
                    break;
            }

            // Orbital music notes 遯ｶ繝ｻevery 30 ticks (musical identity)
            if (AliveTime % 30 == 0)
            {
                GoliathParticleHandler.Spawn(new MusicNoteParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(25f, 25f),
                    Main.rand.NextVector2Circular(0.8f, 0.8f) + new Vector2(0, -0.5f),
                    Color.Lerp(phaseColor, GoliathUtils.EnergyTendril, 0.3f) * 0.6f,
                    0.25f + Main.rand.NextFloat(0.15f), 55 + Main.rand.Next(20)));
            }

            // Conductor Mode: additional glyph particles
            if (gp.ConductorMode && AliveTime % 12 == 0)
            {
                GoliathParticleHandler.Spawn(new ConductorGlyphParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(25f, 25f),
                    Main.rand.NextVector2Circular(0.3f, 0.3f),
                    0.3f + Main.rand.NextFloat(0.2f), 40 + Main.rand.Next(20)));
            }
        }

        // =================================================================
        // RENDERING
        // =================================================================

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;

            SpriteBatch sb = Main.spriteBatch;

            // Pass 1: Gravitational rift shader glow (under the sprite) — Additive
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);
            DrawRiftGlow(sb);

            // Pass 2: Goliath sprite with 6x6 spritesheet — AlphaBlend
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);
            DrawGoliathSprite(sb, lightColor);

            // Pass 3: Additive glow overlay
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);
            DrawGoliathGlow(sb);

            // Restore to AlphaBlend
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        private void DrawRiftGlow(SpriteBatch sb)
        {
            Texture2D bloom = GoliathTextures.SoftRadialBloom;
            Vector2 origin = bloom.Size() * 0.5f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Player owner = Main.player[Projectile.owner];
            GoliathPlayer gp = owner.Goliath();
            Color phaseColor = gp.CurrentPhaseColor;

            // Ambient rift glow — static, tinted by lunar phase
            float pulse = 0.15f;

            // Charge state intensifies glow
            if (CurrentState == GoliathState.BeamAttack || CurrentState == GoliathState.DevastatingBeam)
            {
                float chargeProgress = ChargeTimer / (float)ChargeUpDuration;
                pulse += chargeProgress * 0.15f;
            }

            // Full Moon phase = brighter glow
            if (gp.IsFullMoon)
                pulse *= 1.1f;

            // Reduced by 80% per user feedback — Goliath bloom was obscuring the summon
            Color riftColor = Color.Lerp(GoliathUtils.GravityWell, phaseColor, 0.3f) with { A = 0 } * (pulse * 0.2f);
            sb.Draw(bloom, drawPos, null, riftColor, 0f, origin, 0.03f, SpriteEffects.None, 0f);

            Color innerRift = Color.Lerp(GoliathUtils.NebulaPurple, phaseColor, 0.2f) with { A = 0 } * (pulse * 0.08f);
            sb.Draw(bloom, drawPos, null, innerRift, 0f, origin, 0.02f, SpriteEffects.None, 0f);

            // Phase-specific accent glow — reduced
            Color accentColor = phaseColor with { A = 0 } * (pulse * 0.05f);
            sb.Draw(bloom, drawPos, null, accentColor, 0f, origin, 0.04f, SpriteEffects.None, 0f);
        }

        private void DrawGoliathSprite(SpriteBatch sb, Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;

            int frameWidth = texture.Width / FrameColumns;
            int frameHeight = texture.Height / FrameRows;

            int col = currentFrame % FrameColumns;
            int row = currentFrame / FrameColumns;

            Rectangle sourceRect = new Rectangle(col * frameWidth, row * frameHeight, frameWidth, frameHeight);
            Vector2 origin = new Vector2(frameWidth / 2f, frameHeight / 2f);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Main.EntitySpriteDraw(texture, drawPos, sourceRect, lightColor, 0f, origin, Projectile.scale, effects, 0);
        }

        private void DrawGoliathGlow(SpriteBatch sb)
        {
            Texture2D bloom = GoliathTextures.SoftRadialBloom;
            Vector2 origin = bloom.Size() * 0.5f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Player owner = Main.player[Projectile.owner];
            GoliathPlayer gp = owner.Goliath();
            Color phaseColor = gp.CurrentPhaseColor;

            // Eye / core glow — tinted by lunar phase (reduced 80% per feedback)
            float eyePulse = 0.06f;
            Color eyeColor = Color.Lerp(GoliathUtils.IceBlueBrilliance, phaseColor, 0.25f) with { A = 0 } * eyePulse;
            sb.Draw(bloom, drawPos, null, eyeColor, 0f, origin, 0.025f, SpriteEffects.None, 0f);

            // During beam charge: intensified glow with phase tint
            // Beam charge — reduced 80%
            if (CurrentState == GoliathState.BeamAttack || CurrentState == GoliathState.DevastatingBeam)
            {
                float chargeProgress = ChargeTimer / (float)ChargeUpDuration;
                Color chargeColor = Color.Lerp(GoliathUtils.StarCore, phaseColor, 0.3f) with { A = 0 } * (chargeProgress * 0.1f);
                sb.Draw(bloom, drawPos, null, chargeColor, 0f, origin, 0.03f + chargeProgress * 0.02f, SpriteEffects.None, 0f);
            }

            // Full Moon phase: extra radiance — reduced 80%
            if (gp.IsFullMoon)
            {
                float fullPulse = 0.03f;
                Color fullColor = GoliathUtils.SupermoonWhite with { A = 0 } * fullPulse;
                sb.Draw(bloom, drawPos, null, fullColor, 0f, origin, 0.035f, SpriteEffects.None, 0f);
            }

            // Conductor Mode: additional outer ring glow — reduced 80%
            if (gp.ConductorMode)
            {
                float conductorPulse = gp.ConductorPulse;
                Color conductorColor = GoliathUtils.ConductorHighlight with { A = 0 } * (0.03f + 0.02f * MathF.Sin(conductorPulse * MathHelper.TwoPi));
                sb.Draw(bloom, drawPos, null, conductorColor, 0f, origin, 0.04f, SpriteEffects.None, 0f);
            }
        }
    }
}