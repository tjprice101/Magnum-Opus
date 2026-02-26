using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using MagnumOpus.Content.MoonlightSonata.Dusts;
using MagnumOpus.Content.MoonlightSonata.Projectiles;
using MagnumOpus.Content.MoonlightSonata.Accessories;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;

namespace MagnumOpus.Content.MoonlightSonata.Minions
{
    /// <summary>
    /// Goliath of Moonlight - A massive lunar guardian minion.
    /// Has gravity, floats toward player when can't reach them.
    /// Fires devastating Last Prism-style beams after a 2 second charge.
    /// Uses a 6x6 spritesheet animation (36 frames).
    ///
    /// Conductor Mode Integration:
    ///   When the owner has Conductor Mode active (from Staff of the Lunar Phases),
    ///   the Goliath aims its charged beam toward the owner's cursor position
    ///   instead of auto-targeting the nearest enemy.
    ///
    /// Shader Integration:
    ///   GravitationalRift.fx — Spiral vortex overlay during beam charge
    ///   SummonCircle.fx     — Sigil overlay beneath Goliath during charge
    /// </summary>
    public class GoliathOfMoonlight : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/MoonlightSonata/Minions/GoliathOfMoonlight/GoliathOfMoonlight";

        // Spritesheet configuration - 6x6 grid
        public const int FrameColumns = 6;
        public const int FrameRows = 6;
        public const int TotalFrames = 36;
        public const int FrameTime = 4; // Game ticks per animation frame

        // Charge time - 2 seconds = 120 ticks
        private const int ChargeUpTime = 120;

        private enum AIState
        {
            Idle,
            Attacking,
            Floating,
            ChargingBeam
        }

        private AIState State
        {
            get => (AIState)Projectile.ai[0];
            set => Projectile.ai[0] = (float)value;
        }

        private float Timer
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        private int frameCounter = 0;
        private int currentFrame = 0;
        private int attackCooldown = 0;
        private int chargeTimer = 0;
        private bool isCharging = false;
        private NPC chargeTarget = null;
        private bool wasOnGround = false;
        private Vector2 frozenPosition = Vector2.Zero;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 1; // We handle frames manually
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
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.minionSlots = 1f;
        }

        public override bool? CanCutTiles() => false;

        public override bool MinionContactDamage() => true;

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return false;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            if (!CheckActive(owner))
                return;

            UpdateAnimation();
            ApplyGravity();

            bool onGround = IsOnGround();
            var modPlayer = owner.GetModPlayer<MoonlightAccessoryPlayer>();

            // In Conductor Mode, use cursor position for targeting when no specific target is set
            NPC target = FindTarget(owner);
            float distToPlayer = Vector2.Distance(Projectile.Center, owner.Center);

            if (distToPlayer > 600f || !CanReachPlayer(owner))
            {
                State = AIState.Floating;
                FloatTowardPlayer(owner);
            }
            else if (target != null || (modPlayer.staffConductorMode && isCharging))
            {
                State = AIState.Attacking;
                AttackTarget(target, owner, onGround);
            }
            else if (modPlayer.staffConductorMode && !isCharging)
            {
                // In Conductor Mode without a target, still allow initiating charge
                // toward the cursor if there are enemies in general range
                State = AIState.Attacking;
                AttackTarget(null, owner, onGround);
            }
            else
            {
                State = AIState.Idle;
                IdleMovement(owner, onGround);
            }

            // Ambient VFX via GoliathVFX — enhanced in Conductor Mode
            if (!Main.dedServ)
            {
                if (modPlayer.staffConductorMode)
                    GoliathVFX.ConductorModeAmbient(Projectile.Center, (int)Main.GameUpdateCount, Main.MouseWorld);
                else
                    GoliathVFX.AmbientAura(Projectile.Center, (int)Main.GameUpdateCount);
            }

            Lighting.AddLight(Projectile.Center, GoliathVFX.NebulaPurple.ToVector3() * 0.4f);

            if (Math.Abs(Projectile.velocity.X) > 0.5f)
            {
                Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;
            }

            wasOnGround = onGround;
        }

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

        private void ApplyGravity()
        {
            if (State != AIState.Floating)
            {
                Projectile.velocity.Y += 0.35f;
                if (Projectile.velocity.Y > 16f)
                    Projectile.velocity.Y = 16f;
            }
        }

        private bool IsOnGround()
        {
            Vector2 checkPos = Projectile.BottomLeft;
            for (int x = 0; x < Projectile.width / 16 + 1; x++)
            {
                int tileX = (int)((checkPos.X + x * 16) / 16);
                int tileY = (int)((checkPos.Y + 4) / 16);

                Tile tile = Framing.GetTileSafely(tileX, tileY);
                if (tile.HasTile && Main.tileSolid[tile.TileType])
                    return true;
            }
            return false;
        }

        private bool CanReachPlayer(Player owner)
        {
            Vector2 direction = owner.Center - Projectile.Center;
            float distance = direction.Length();
            direction.Normalize();

            for (float i = 0; i < distance; i += 16f)
            {
                Vector2 checkPos = Projectile.Center + direction * i;
                int tileX = (int)(checkPos.X / 16);
                int tileY = (int)(checkPos.Y / 16);

                Tile tile = Framing.GetTileSafely(tileX, tileY);
                if (tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
                {
                    if (i > 200f)
                        return false;
                }
            }
            return true;
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<GoliathOfMoonlightBuff>());
                Projectile.Kill();
                return false;
            }

            if (owner.HasBuff(ModContent.BuffType<GoliathOfMoonlightBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            return true;
        }

        private NPC FindTarget(Player owner)
        {
            float maxDistance = 700f;
            NPC closestTarget = null;
            float closestDist = maxDistance;

            if (owner.HasMinionAttackTargetNPC)
            {
                NPC target = Main.npc[owner.MinionAttackTargetNPC];
                if (target.CanBeChasedBy(this) && Vector2.Distance(Projectile.Center, target.Center) < maxDistance)
                {
                    return target;
                }
            }

            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.CanBeChasedBy(this))
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestTarget = npc;
                    }
                }
            }

            return closestTarget;
        }

        private void FloatTowardPlayer(Player owner)
        {
            Projectile.tileCollide = false;

            Vector2 targetPos = owner.Center + new Vector2(-40f * owner.direction, -60f);
            Vector2 direction = targetPos - Projectile.Center;
            float distance = direction.Length();

            float speed = 12f;
            float inertia = 15f;

            if (distance > 20f)
            {
                direction.Normalize();
                direction *= Math.Min(distance / 8f, speed);
                Projectile.velocity = (Projectile.velocity * (inertia - 1) + direction) / inertia;
            }
            else
            {
                Projectile.velocity *= 0.9f;
            }

            if (distance < 100f)
            {
                Projectile.tileCollide = true;
            }

            // Floating trail — GravityWellDust cosmic wake
            if (!Main.dedServ && Main.rand.NextBool(4))
            {
                Vector2 dustVel = new Vector2(0, -2f);
                Color dustColor = Color.Lerp(GoliathVFX.CosmicVoid, GoliathVFX.GravityWell, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(Projectile.Center,
                    ModContent.DustType<GravityWellDust>(),
                    dustVel, 0, dustColor, 0.2f);
                d.customData = new GravityWellBehavior
                {
                    GravityCenter = Vector2.Zero,
                    PullStrength = 0f,
                    BaseScale = 0.2f,
                    Lifetime = 20,
                    VelocityDecay = 0.96f
                };
            }
        }

        private void AttackTarget(NPC target, Player owner, bool onGround)
        {
            Projectile.tileCollide = true;
            var modPlayer = owner.GetModPlayer<MoonlightAccessoryPlayer>();
            bool conductorMode = modPlayer.staffConductorMode;

            // Determine aim direction based on conductor mode
            Vector2 aimPos;
            if (conductorMode)
            {
                // In Conductor Mode: aim toward owner's cursor position
                aimPos = Main.MouseWorld;
            }
            else if (target != null)
            {
                aimPos = target.Center;
            }
            else
            {
                // No target and not in conductor mode — shouldn't reach here normally
                return;
            }

            Vector2 direction = aimPos - Projectile.Center;
            float distance = direction.Length();

            // Handle charging state - FREEZE during 2 second charge
            if (isCharging)
            {
                Projectile.velocity = Vector2.Zero;
                Projectile.Center = frozenPosition;
                State = AIState.ChargingBeam;

                chargeTimer++;
                float chargeProgress = (float)chargeTimer / ChargeUpTime;

                // Delegate ALL charge VFX to GoliathVFX (conductor-aware with shader integration)
                if (!Main.dedServ)
                {
                    if (conductorMode)
                        GoliathVFX.ConductorChargeBuildup(Projectile.Center, chargeProgress, Main.MouseWorld);
                    else
                        GoliathVFX.ChargeBuildup(Projectile.Center, chargeProgress);
                }

                // Growing glow at center
                float glowIntensity = 0.3f + chargeProgress * 0.8f;
                Lighting.AddLight(Projectile.Center, GoliathVFX.NebulaPurple.ToVector3() * glowIntensity);

                // Sound cues during charge
                if (chargeTimer == 1)
                    SoundEngine.PlaySound(SoundID.Item15 with { Pitch = 0.3f, Volume = 0.6f }, Projectile.Center);
                if (chargeTimer == 40)
                    SoundEngine.PlaySound(SoundID.Item15 with { Pitch = 0.5f, Volume = 0.7f }, Projectile.Center);
                if (chargeTimer == 80)
                    SoundEngine.PlaySound(SoundID.Item15 with { Pitch = 0.7f, Volume = 0.8f }, Projectile.Center);
                if (chargeTimer == 110)
                    SoundEngine.PlaySound(SoundID.Item105 with { Volume = 0.9f }, Projectile.Center);

                // Fire devastating beam after 2 second charge
                if (chargeTimer >= ChargeUpTime)
                {
                    if (Main.myPlayer == Projectile.owner)
                    {
                        if (conductorMode)
                        {
                            // Conductor Mode: Fire toward cursor position
                            FireDevastatingBeamAtPosition(Main.MouseWorld, owner);
                        }
                        else if (chargeTarget != null && chargeTarget.active)
                        {
                            // Auto-target: Fire at locked NPC
                            FireDevastatingBeam(chargeTarget, owner);
                        }
                    }

                    isCharging = false;
                    chargeTimer = 0;
                    chargeTarget = null;

                    attackCooldown = modPlayer.hasFractalOfMoonlight ? 135 : 180;
                    State = AIState.Attacking;
                }
                return;
            }

            // Normal movement when not charging
            if (target != null)
            {
                Vector2 moveDir = target.Center - Projectile.Center;
                if (onGround)
                {
                    float moveSpeed = 6f;
                    float targetX = Math.Sign(moveDir.X) * moveSpeed;

                    Projectile.velocity.X = MathHelper.Lerp(Projectile.velocity.X, targetX, 0.1f);

                    if (target.Center.Y < Projectile.Center.Y - 80f && Math.Abs(moveDir.X) < 200f)
                    {
                        Projectile.velocity.Y = -10f;

                        if (!Main.dedServ)
                        {
                            GoliathVFX.JumpEffect(Projectile.BottomLeft + new Vector2(Projectile.width / 2f, 0f));
                        }
                    }
                }
                else
                {
                    float airControl = 0.08f;
                    Projectile.velocity.X += Math.Sign(moveDir.X) * airControl;
                    Projectile.velocity.X = MathHelper.Clamp(Projectile.velocity.X, -8f, 8f);
                }
            }
            else if (conductorMode)
            {
                // In conductor mode without a target, idle but ready to charge toward cursor
                Projectile.velocity.X *= 0.95f;
            }

            // Start charging attack
            attackCooldown--;
            bool canCharge = conductorMode
                ? attackCooldown <= 0 && !isCharging  // Conductor: charge toward cursor anytime
                : attackCooldown <= 0 && distance < 600f && !isCharging && target != null;

            if (canCharge)
            {
                isCharging = true;
                chargeTimer = 0;
                chargeTarget = target; // null in conductor mode — that's OK
                frozenPosition = Projectile.Center;

                SoundEngine.PlaySound(SoundID.Item15 with { Pitch = 0.2f, Volume = 0.5f }, Projectile.Center);
            }
        }

        private void FireDevastatingBeam(NPC target, Player owner)
        {
            Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
            Vector2 muzzlePos = Projectile.Center + toTarget * 20f;

            // Delegate muzzle flash VFX to GoliathVFX
            if (!Main.dedServ)
            {
                GoliathVFX.ChargeReleaseFlash(muzzlePos, toTarget);
            }

            // Fire sound
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.2f, Volume = 1.2f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item125 with { Pitch = 0.3f, Volume = 0.8f }, Projectile.Center);

            Lighting.AddLight(Projectile.Center, GoliathVFX.StarCore.ToVector3() * 1.5f);

            int beamDamage = (int)(Projectile.damage * 1.5f);

            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                toTarget,
                ModContent.ProjectileType<GoliathDevastatingBeam>(),
                beamDamage,
                Projectile.knockBack * 2f,
                Projectile.owner
            );
        }

        /// <summary>
        /// Fires a devastating beam toward a specific world position (Conductor Mode).
        /// </summary>
        private void FireDevastatingBeamAtPosition(Vector2 targetPos, Player owner)
        {
            Vector2 toTarget = (targetPos - Projectile.Center).SafeNormalize(Vector2.Zero);
            Vector2 muzzlePos = Projectile.Center + toTarget * 20f;

            // Delegate muzzle flash VFX to GoliathVFX
            if (!Main.dedServ)
            {
                GoliathVFX.ChargeReleaseFlash(muzzlePos, toTarget);
            }

            // Fire sound — slightly higher pitch in conductor mode
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0f, Volume = 1.2f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item125 with { Pitch = 0.5f, Volume = 0.8f }, Projectile.Center);

            Lighting.AddLight(Projectile.Center, GoliathVFX.StarCore.ToVector3() * 1.5f);

            int beamDamage = (int)(Projectile.damage * 1.5f);

            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                toTarget,
                ModContent.ProjectileType<GoliathDevastatingBeam>(),
                beamDamage,
                Projectile.knockBack * 2f,
                Projectile.owner
            );
        }

        private void IdleMovement(Player owner, bool onGround)
        {
            Projectile.tileCollide = true;

            Vector2 targetPos = owner.Center + new Vector2(-60f * owner.direction, 0f);
            Vector2 direction = targetPos - Projectile.Center;
            float distance = direction.Length();

            if (onGround)
            {
                if (Math.Abs(direction.X) > 40f)
                {
                    float walkSpeed = 4f;
                    Projectile.velocity.X = MathHelper.Lerp(Projectile.velocity.X, Math.Sign(direction.X) * walkSpeed, 0.08f);
                }
                else
                {
                    Projectile.velocity.X *= 0.9f;
                }

                if (owner.Center.Y < Projectile.Center.Y - 100f && Math.Abs(direction.X) < 150f)
                {
                    Projectile.velocity.Y = -12f;
                }
            }
            else
            {
                Projectile.velocity.X += Math.Sign(direction.X) * 0.05f;
                Projectile.velocity.X = MathHelper.Clamp(Projectile.velocity.X, -6f, 6f);
            }

            // Teleport if too far
            if (distance > 1500f)
            {
                Projectile.Center = owner.Center + new Vector2(-40f * owner.direction, 0f);
                Projectile.velocity = Vector2.Zero;

                if (!Main.dedServ)
                {
                    GoliathVFX.TeleportFlash(Projectile.Center);
                }

                SoundEngine.PlaySound(SoundID.Item8, Projectile.Center);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 180);

            if (!Main.dedServ)
            {
                // Massive Goliath impact via GoliathVFX
                GoliathVFX.MeleeHitImpact(target.Center);

                // Additional custom particle burst
                CustomParticles.MoonlightHalo(target.Center, 0.9f);
                CustomParticles.ExplosionBurst(target.Center, GoliathVFX.EnergyTendril, 10, 6f);
            }

            Lighting.AddLight(target.Center, GoliathVFX.NebulaPurple.ToVector3() * 0.8f);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Player owner = Main.player[Projectile.owner];
            var modPlayer = owner.GetModPlayer<MoonlightAccessoryPlayer>();

            if (modPlayer.hasFractalOfMoonlight)
            {
                modifiers.FinalDamage *= 1.5f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;

            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            int frameWidth = texture.Width / FrameColumns;
            int frameHeight = texture.Height / FrameRows;

            int col = currentFrame % FrameColumns;
            int row = currentFrame / FrameColumns;

            Rectangle sourceRect = new Rectangle(col * frameWidth, row * frameHeight, frameWidth, frameHeight);
            Vector2 origin = new Vector2(frameWidth / 2f, frameHeight);
            Vector2 drawPos = new Vector2(Projectile.Center.X, Projectile.position.Y + Projectile.height) - Main.screenPosition;

            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // === COSMIC BLOOM AURA via GoliathVFX (uses {A=0}, no SpriteBatch restart) ===
            float chargeProgress = isCharging ? (float)chargeTimer / ChargeUpTime : 0f;
            GoliathVFX.DrawCosmicBloom(spriteBatch, Projectile.Center, isCharging, chargeProgress);

            // === SHADER OVERLAY — GravitationalRift during charge ===
            if (isCharging && chargeProgress > 0.2f && MoonlightSonataShaderManager.HasGravitationalRift)
            {
                try
                {
                    MoonlightSonataShaderManager.BeginShaderBatch(spriteBatch);

                    // Draw GravitationalRift overlay around Goliath during charge
                    Weapons.StaffOfTheLunarPhases.StaffOfTheLunarPhasesVFX.DrawGravitationalRiftOverlay(
                        spriteBatch, Projectile.Center, chargeProgress, 0.8f + chargeProgress * 0.4f);

                    // Draw SummonCircle below Goliath during high charge
                    if (chargeProgress > 0.5f && MoonlightSonataShaderManager.HasSummonCircle)
                    {
                        Vector2 circlePos = Projectile.Center + new Vector2(0, Projectile.height * 0.3f);
                        Weapons.StaffOfTheLunarPhases.StaffOfTheLunarPhasesVFX.DrawSummonCircleOverlay(
                            spriteBatch, circlePos, chargeProgress, 0.6f + chargeProgress * 0.3f);
                    }

                    MoonlightSonataShaderManager.RestoreDefaultBatch(spriteBatch);
                }
                catch
                {
                    // Shader fallback — restore batch state
                    try { MoonlightSonataShaderManager.RestoreDefaultBatch(spriteBatch); } catch { }
                }
            }

            // === CONDUCTOR MODE — Beam direction indicator line ===
            if (isCharging && chargeProgress > 0.2f)
            {
                Player owner2 = Main.player[Projectile.owner];
                var modPlayer2 = owner2.GetModPlayer<MoonlightAccessoryPlayer>();
                if (modPlayer2.staffConductorMode)
                {
                    GoliathVFX.DrawConductorTargetLine(spriteBatch, Projectile.Center, Main.MouseWorld, chargeProgress);
                }
            }

            // === BODY BLOOM — 5-layer {A=0} bloom stack with cosmic entity palette ===
            float glowMult = isCharging ? 1f + chargeProgress * 0.5f : 1f;

            // Layer 1: Outer cosmic void bloom
            Color outerGlow = (GoliathVFX.CosmicVoid with { A = 0 }) * 0.30f * glowMult;
            Main.EntitySpriteDraw(texture, drawPos, sourceRect, outerGlow, 0f, origin, Projectile.scale * 1.18f, effects, 0);

            // Layer 2: Event horizon mid
            Color eventGlow = (GoliathVFX.EventHorizon with { A = 0 }) * 0.28f * glowMult;
            Main.EntitySpriteDraw(texture, drawPos, sourceRect, eventGlow, 0f, origin, Projectile.scale * 1.12f, effects, 0);

            // Layer 3: Nebula purple bloom
            Color nebGlow = (GoliathVFX.NebulaPurple with { A = 0 }) * 0.25f * glowMult;
            Main.EntitySpriteDraw(texture, drawPos, sourceRect, nebGlow, 0f, origin, Projectile.scale * 1.08f, effects, 0);

            // Layer 4: Inner energy tendril (enhanced during charge)
            Color innerGlow = (GoliathVFX.EnergyTendril with { A = 0 }) * 0.22f * glowMult;
            Main.EntitySpriteDraw(texture, drawPos, sourceRect, innerGlow, 0f, origin, Projectile.scale * 1.04f, effects, 0);

            // Layer 5: Star core (visible during charge)
            if (isCharging)
            {
                Color coreGlow = (GoliathVFX.StarCore with { A = 0 }) * 0.20f * chargeProgress;
                Main.EntitySpriteDraw(texture, drawPos, sourceRect, coreGlow, 0f, origin, Projectile.scale * 1.02f, effects, 0);
            }

            // Draw main sprite
            Main.EntitySpriteDraw(texture, drawPos, sourceRect, lightColor, 0f, origin, Projectile.scale, effects, 0);

            return false;
        }
    }
}
