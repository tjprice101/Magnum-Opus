using MagnumOpus.Common;
using MagnumOpus.Content.DiesIrae;
using MagnumOpus.Content.DiesIrae.Weapons.WrathfulContract.Particles;
using MagnumOpus.Content.DiesIrae.Weapons.WrathfulContract.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.WrathfulContract.Projectiles
{
    /// <summary>
    /// Wrath Demon Minion — a blood-contract bound demon that constantly drains player HP.
    /// 
    /// States:
    ///   Normal: Orbits player, charges at enemies. 1 HP/s drain.
    ///   Frenzy: After 3 kills → 2x attack speed, +30% damage, 3 HP/s drain, 8s duration.
    ///   Breach of Contract: Player drops below 10% HP → demon turns hostile for 5s.
    ///   Blood Sacrifice: Player sacrifices 20% HP → demon deals 3x damage for 5s.
    /// 
    /// VFX: RadialNoiseMaskShader demon body (FBMNoise + VoronoiCell), StarFlare eyes,
    /// ThinLaserFoundation blood tether, ExplosionParticles frenzy kill burst,
    /// SmokeFoundation ambient smoke, ImpactFoundation breach/frenzy ripples.
    /// 
    /// AI fields:
    ///   ai[0] = state (0=Normal, 1=Frenzy, 2=Breach)
    ///   ai[1] = state timer
    ///   localAI[0] = kill counter (for Frenzy threshold)
    ///   localAI[1] = attack cooldown timer
    /// </summary>
    public class WrathDemonMinion : ModProjectile
    {
        // ═══════════════════════════════════════════════════════
        //  CONSTANTS
        // ═══════════════════════════════════════════════════════
        private const float DetectionRange = 750f;
        private const float ChargeSpeed = 14f;
        private const float FrenzyChargeSpeed = 22f;
        private const int AttackCooldown = 40;
        private const int FrenzyAttackCooldown = 20;
        private const int FrenzyKillThreshold = 3;
        private const int FrenzyDuration = 480;  // 8s
        private const int BreachDuration = 300;  // 5s
        private const float BreachHPThreshold = 0.10f;
        private const int BreachCooldown = 600;  // 10s between breaches

        // ═══════════════════════════════════════════════════════
        //  STATE
        // ═══════════════════════════════════════════════════════
        private enum DemonAIState { Normal, Frenzy, Breach }

        private DemonAIState CurrentState
        {
            get => (DemonAIState)(int)Projectile.ai[0];
            set => Projectile.ai[0] = (float)value;
        }

        private int StateTimer
        {
            get => (int)Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        private int KillCounter
        {
            get => (int)Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }

        private int AttackTimer
        {
            get => (int)Projectile.localAI[1];
            set => Projectile.localAI[1] = value;
        }

        private int _breachCooldownTimer = 0;
        private bool _charging = false;
        private int _chargeTarget = -1;
        private float _seed;

        public override void SetStaticDefaults()
        {
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minion = true;
            Projectile.minionSlots = 2f;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.timeLeft = 2;
            _seed = Main.rand.NextFloat(100f);
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => CurrentState != DemonAIState.Breach;

        // ═══════════════════════════════════════════════════════
        //  AI
        // ═══════════════════════════════════════════════════════
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!CheckActive(player)) return;

            // Update ModPlayer state
            var modPlayer = player.GetModPlayer<WrathfulContractPlayer>();
            modPlayer.HasActiveDemon = true;
            if (CurrentState == DemonAIState.Frenzy)
                modPlayer.DemonInFrenzy = true;

            // Breach cooldown
            if (_breachCooldownTimer > 0) _breachCooldownTimer--;

            // Check for breach threshold
            if (CurrentState != DemonAIState.Breach && _breachCooldownTimer <= 0 &&
                modPlayer.IsBelowBreachThreshold())
            {
                EnterBreach();
            }

            // State machine
            switch (CurrentState)
            {
                case DemonAIState.Normal:
                    AI_Normal(player);
                    break;
                case DemonAIState.Frenzy:
                    AI_Frenzy(player);
                    break;
                case DemonAIState.Breach:
                    AI_Breach(player);
                    break;
            }

            // Blood sacrifice check
            var dmgMult = GetDamageMultiplier(player);

            // Ambient smoke
            var vfxState = GetVFXState();
            WrathfulContractUtils.DoAmbientSmoke(Projectile.Center, vfxState);
        }

        private bool CheckActive(Player player)
        {
            if (player.dead || !player.active)
            {
                player.ClearBuff(ModContent.BuffType<Buffs.WrathfulContractBuff>());
                Projectile.Kill();
                return false;
            }
            if (player.HasBuff(ModContent.BuffType<Buffs.WrathfulContractBuff>()))
                Projectile.timeLeft = 2;
            return true;
        }

        // ── NORMAL STATE ──
        private void AI_Normal(Player player)
        {
            if (AttackTimer > 0) AttackTimer--;

            if (_charging && _chargeTarget >= 0)
                DoCharge(ChargeSpeed);
            else
            {
                FloatNearPlayer(player);
                if (AttackTimer <= 0)
                    TryAcquireTarget();
            }
        }

        // ── FRENZY STATE ──
        private void AI_Frenzy(Player player)
        {
            StateTimer--;
            if (StateTimer <= 0)
            {
                CurrentState = DemonAIState.Normal;
                KillCounter = 0;
                return;
            }

            if (AttackTimer > 0) AttackTimer--;

            if (_charging && _chargeTarget >= 0)
                DoCharge(FrenzyChargeSpeed);
            else
            {
                FloatNearPlayer(player);
                if (AttackTimer <= 0)
                    TryAcquireTarget();
            }
        }

        // ── BREACH STATE (hostile to player) ──
        private void AI_Breach(Player player)
        {
            StateTimer--;
            if (StateTimer <= 0)
            {
                CurrentState = DemonAIState.Normal;
                _breachCooldownTimer = BreachCooldown;
                _charging = false;
                Projectile.friendly = true;
                Projectile.hostile = false;

                // Re-bind ripple VFX
                WrathfulContractUtils.DoBreachWarningPulse(Projectile.Center);
                return;
            }

            // Chase the player!
            Vector2 dir = (player.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
            float dist = Vector2.Distance(player.Center, Projectile.Center);
            float speed = MathHelper.Clamp(dist * 0.05f, 3f, 10f);
            Projectile.velocity = dir * speed;

            // Breach warning pulses every 30 frames
            if (StateTimer % 30 == 0)
                WrathfulContractUtils.DoBreachWarningPulse(Projectile.Center);
        }

        private void EnterBreach()
        {
            CurrentState = DemonAIState.Breach;
            StateTimer = BreachDuration;
            _charging = false;

            // Make demon hostile
            Projectile.friendly = false;
            Projectile.hostile = true;

            // Breach warning burst
            WrathfulContractUtils.DoBreachWarningPulse(Projectile.Center);
        }

        // ═══════════════════════════════════════════════════════
        //  MOVEMENT & TARGETING
        // ═══════════════════════════════════════════════════════

        private void FloatNearPlayer(Player player)
        {
            float bobOffset = MathF.Sin((float)Main.timeForVisualEffects * 0.03f + _seed) * 8f;
            Vector2 targetPos = player.Center + new Vector2(player.direction * -60f, -70f + bobOffset);

            Vector2 toTarget = targetPos - Projectile.Center;
            float dist = toTarget.Length();
            float speed = MathHelper.Clamp(dist * 0.06f, 1f, 12f);

            if (dist > 800f)
                Projectile.Center = targetPos;
            else if (dist > 5f)
                Projectile.velocity = toTarget.SafeNormalize(Vector2.Zero) * speed;
            else
                Projectile.velocity *= 0.9f;
        }

        private void TryAcquireTarget()
        {
            float bestDist = DetectionRange;
            int bestIndex = -1;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.CountsAsACritter) continue;
                if (!npc.CanBeChasedBy()) continue;

                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestIndex = i;
                }
            }

            if (bestIndex >= 0)
            {
                _chargeTarget = bestIndex;
                _charging = true;
            }
        }

        private void DoCharge(float speed)
        {
            if (_chargeTarget < 0 || _chargeTarget >= Main.maxNPCs)
            {
                EndCharge();
                return;
            }

            NPC target = Main.npc[_chargeTarget];
            if (!target.active || target.friendly || target.dontTakeDamage)
            {
                EndCharge();
                return;
            }

            Vector2 dir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
            Projectile.velocity = dir * speed;

            // If we're close enough, the contact damage handles it
            float dist = Vector2.Distance(Projectile.Center, target.Center);
            if (dist > DetectionRange * 1.5f)
                EndCharge();
        }

        private void EndCharge()
        {
            _charging = false;
            _chargeTarget = -1;
            int cooldown = CurrentState == DemonAIState.Frenzy ? FrenzyAttackCooldown : AttackCooldown;
            AttackTimer = cooldown;
        }

        // ═══════════════════════════════════════════════════════
        //  ON HIT / ON KILL
        // ═══════════════════════════════════════════════════════
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 180);
            EndCharge();

            // Dies Irae VFX: demon charge impact
            DiesIraeVFXLibrary.MeleeImpact(target.Center, 1);
            DiesIraeVFXLibrary.SpawnEmberScatter(target.Center, 6, 4f);

            // Contract Clause healing: 5% enemy max HP
            Player player = Main.player[Projectile.owner];
            int healing = (int)(target.lifeMax * 0.05f);
            healing = Math.Max(1, Math.Min(healing, 50)); // Cap at 50 HP per heal
            player.Heal(healing);

            // Fire 6 homing fireballs every 3rd hit
            if (Main.myPlayer == Projectile.owner && Main.rand.NextBool(3))
            {
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi / 6f * i;
                    Vector2 vel = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 8f;
                    float dmgMult = GetDamageMultiplier(player);
                    int fbDamage = (int)(Projectile.damage * 0.5f * dmgMult);
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        target.Center, vel,
                        ModContent.ProjectileType<WrathFireballProjectile>(),
                        fbDamage, 2f, Projectile.owner);
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Not used — minions don't "die" normally
        }

        /// <summary>
        /// Called when our contact damage kills an NPC. We detect this via damage comparison.
        /// </summary>
        public void OnTargetKilled(Vector2 killPos)
        {
            KillCounter++;

            // Frenzy kill burst VFX
            if (CurrentState == DemonAIState.Frenzy)
                WrathfulContractUtils.DoFrenzyKillBurst(killPos);

            // Check Frenzy threshold
            if (KillCounter >= FrenzyKillThreshold && CurrentState == DemonAIState.Normal)
            {
                CurrentState = DemonAIState.Frenzy;
                StateTimer = FrenzyDuration;
                KillCounter = 0;

                // Frenzy activation burst
                DemonParticleHandler.Spawn(new BloodSacrificeFlashParticle(Projectile.Center, 0.3f, 25));
            }
        }

        private float GetDamageMultiplier(Player player)
        {
            float mult = 1f;
            if (CurrentState == DemonAIState.Frenzy) mult *= 1.3f;

            var modPlayer = player.GetModPlayer<WrathfulContractPlayer>();
            if (modPlayer.BloodSacrificeTimer > 0) mult *= 3f;

            return mult;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Player player = Main.player[Projectile.owner];
            float mult = GetDamageMultiplier(player);
            if (mult > 1f)
                modifiers.FinalDamage *= mult;
        }

        // ═══════════════════════════════════════════════════════
        //  RENDERING
        // ═══════════════════════════════════════════════════════
        private WrathfulContractUtils.DemonState GetVFXState()
        {
            Player player = Main.player[Projectile.owner];
            var modPlayer = player.GetModPlayer<WrathfulContractPlayer>();

            if (modPlayer.BloodSacrificeTimer > 0) return WrathfulContractUtils.DemonState.Sacrifice;
            return CurrentState switch
            {
                DemonAIState.Frenzy => WrathfulContractUtils.DemonState.Frenzy,
                DemonAIState.Breach => WrathfulContractUtils.DemonState.Breach,
                _ => WrathfulContractUtils.DemonState.Normal
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
            var vfxState = GetVFXState();
            Player player = Main.player[Projectile.owner];

            // Blood contract tether
            bool sacrificeReversed = player.GetModPlayer<WrathfulContractPlayer>().BloodSacrificeTimer > 0;
            if (CurrentState != DemonAIState.Breach)
            {
                WrathfulContractUtils.DrawBloodTether(sb, player.Center, Projectile.Center,
                    vfxState, sacrificeReversed);
            }

            // Demon body (MaskFoundation shader handles batch internally)
            WrathfulContractUtils.DrawDemonBody(sb, Projectile.Center, timer, vfxState, _seed);


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
    //  WRATH FIREBALL PROJECTILE — Homing fireballs from demon attacks
    // ═══════════════════════════════════════════════════════════════
    /// <summary>
    /// Homing fireball with ember trail. Homes toward nearest enemy.
    /// </summary>
    public class WrathFireballProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            // Home toward nearest enemy
            float bestDist = 400f;
            NPC bestTarget = null;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                if (!npc.CanBeChasedBy()) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestTarget = npc;
                }
            }

            if (bestTarget != null)
            {
                Vector2 dir = (bestTarget.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                float speed = Projectile.velocity.Length();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity.SafeNormalize(Vector2.UnitY), dir, 0.15f) * speed;
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Ember trail
            if (Main.rand.NextBool(2))
            {
                Color emberColor = Color.Lerp(DiesIraePalette.EmberOrange, DiesIraePalette.InfernalRed, Main.rand.NextFloat());
                Vector2 vel = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.5f, 0.5f);
                DemonParticleHandler.Spawn(new DemonEmberParticle(Projectile.Center, vel, emberColor, 0.012f, 15));
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 120);

            // Dies Irae VFX: fireball impact with color-ramped sparkle explosion
            DiesIraeVFXLibrary.SpawnColorRampedSparkleExplosion(target.Center, 6, 4f, 0.25f);
            DiesIraeVFXLibrary.SpawnContrastSparkle(target.Center, Projectile.velocity);

            // Impact sparks
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(2f, 2f);
                Color color = Color.Lerp(DiesIraePalette.EmberOrange, DiesIraePalette.JudgmentGold, Main.rand.NextFloat());
                DemonParticleHandler.Spawn(new FrenzyKillSparkParticle(target.Center, vel, color, 0.015f, 12));
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            Texture2D glow = DemonTextures.SoftGlow;
            if (glow == null) return false;

            Vector2 pos = Projectile.Center - Main.screenPosition;
            Vector2 origin = glow.Size() / 2f;
            float time = (float)Main.GameUpdateCount;
            float pulse = 0.85f + 0.15f * MathF.Sin(time * 0.12f);

            // ── Switch to Additive for all glow layers ──
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Layer 1: Wide infernal halo
            sb.Draw(glow, pos, null, DiesIraePalette.BloodRed * 0.2f * pulse, 0f, origin, 0.12f, SpriteEffects.None, 0f);

            // Layer 2: Mid ember glow
            sb.Draw(glow, pos, null, DiesIraePalette.EmberOrange * 0.45f * pulse, 0f, origin, 0.07f, SpriteEffects.None, 0f);

            // Layer 3: Inner infernal bloom
            sb.Draw(glow, pos, null, DiesIraePalette.InfernalRed * 0.6f, 0f, origin, 0.04f, SpriteEffects.None, 0f);

            // Layer 4: White-hot core
            sb.Draw(glow, pos, null, DiesIraePalette.WrathWhite * 0.75f, 0f, origin, 0.02f, SpriteEffects.None, 0f);

            // Layer 5: Star flare accent (if available)
            Texture2D starFlare = DemonTextures.DIStarFlare ?? DemonTextures.StarFlare;
            if (starFlare != null)
            {
                Vector2 starOrigin = starFlare.Size() / 2f;
                float rot = time * 0.04f;
                sb.Draw(starFlare, pos, null, DiesIraePalette.JudgmentGold * 0.35f * pulse, rot, starOrigin, 0.04f, SpriteEffects.None, 0f);
            }

            // ── Restore default SpriteBatch ──

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