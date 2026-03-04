using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.InfernalChimesCalling.Utilities;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.InfernalChimesCalling.Particles;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.InfernalChimesCalling.Primitives;
using MagnumOpus.Content.LaCampanella;
using MagnumOpus.Content.LaCampanella.Debuffs;
using ReLogic.Content;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.InfernalChimesCalling.Projectiles
{
    /// <summary>
    /// Campanella Choir Bell Minion 遯ｶ繝ｻspectral bell that hovers in arc formation.
    /// 
    /// Bells attack sequentially with 0.3s stagger. Each fires a MinionShockwaveProj.
    /// During Infernal Crescendo (every 12s): all bells charge for 2s, then fire simultaneously.
    /// Bell formation: hover in arc above player, evenly spaced.
    /// </summary>
    public class CampanellaChoirMinion : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/InfernalChimesCalling/CampanellaChoirMinion";

        private enum BellState { Idle, AttackReady, Firing, CrescendoCharge, CrescendoFire }
        private BellState CurrentState
        {
            get => (BellState)(int)Projectile.ai[0];
            set => Projectile.ai[0] = (float)value;
        }

        private int StateTimer
        {
            get => (int)Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        // Attack constants
        private const float DetectionRange = 800f;
        private const float HoverDistance = 90f;
        private const int AttackCooldown = 60; // 1 second between attack sequences
        private const int FiringDuration = 15;
        private const int CrescendoFireDuration = 25;

        private int _attackCooldownTimer;
        private int _bellIndex; // Which bell am I in the formation?
        private int _totalBells; // How many bells are there?
        private float _hoverOffset;
        private int _targetNPC = -1;

        // Trail
        private List<Vector2> _trailPositions = new List<Vector2>();
        private const int MaxTrailPoints = 12;
        private InfernalChimesPrimitiveRenderer _trailRenderer;

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
            Projectile.localNPCHitCooldown = 30;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => false; // Bells attack via shockwaves, not contact

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!CheckActive(owner)) return;

            // Determine our index among active bells
            ComputeBellIndex(owner);

            // Record trail
            _trailPositions.Insert(0, Projectile.Center);
            if (_trailPositions.Count > MaxTrailPoints)
                _trailPositions.RemoveAt(_trailPositions.Count - 1);

            // Find target
            _targetNPC = FindTarget(owner);

            var icPlayer = owner.InfernalChimesCalling();

            // Infernal Crescendo check
            if (icPlayer.CrescendoCharging && CurrentState != BellState.CrescendoCharge && CurrentState != BellState.CrescendoFire)
            {
                CurrentState = BellState.CrescendoCharge;
                StateTimer = 0;
            }

            switch (CurrentState)
            {
                case BellState.Idle:
                    IdleBehavior(owner, icPlayer);
                    break;
                case BellState.AttackReady:
                    AttackReadyBehavior(owner, icPlayer);
                    break;
                case BellState.Firing:
                    FiringBehavior(owner);
                    break;
                case BellState.CrescendoCharge:
                    CrescendoChargeBehavior(owner, icPlayer);
                    break;
                case BellState.CrescendoFire:
                    CrescendoFireBehavior(owner);
                    break;
            }

            // Ambient particles
            if (Main.rand.NextBool(10))
            {
                InfernalChimesParticleHandler.SpawnParticle(new ChoirEmberParticle(
                    Projectile.Center, Main.rand.NextFloat(MathHelper.TwoPi),
                    8f, Main.rand.NextFloat(0.04f, 0.08f), Main.rand.Next(30, 60)));
            }
            if (Main.rand.NextBool(30))
            {
                InfernalChimesParticleHandler.SpawnParticle(new MusicalChoirNoteParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(20, 20),
                    new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-1.5f, -0.5f)),
                    Main.rand.Next(50, 80)));
            }

            // Sprite direction
            if (_targetNPC >= 0)
                Projectile.spriteDirection = Main.npc[_targetNPC].Center.X > Projectile.Center.X ? 1 : -1;
            else
                Projectile.spriteDirection = owner.direction;

            // Light
            float lightScale = CurrentState == BellState.CrescendoCharge ? 0.7f : 0.4f;
            Lighting.AddLight(Projectile.Center, InfernalChimesCallingUtils.ChoirPalette[2].ToVector3() * lightScale);
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

        private void ComputeBellIndex(Player owner)
        {
            int myType = Projectile.type;
            int index = 0;
            int total = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.type == myType && p.owner == owner.whoAmI)
                {
                    if (p.whoAmI == Projectile.whoAmI)
                        _bellIndex = total;
                    total++;
                }
            }
            _totalBells = total;
        }

        private int FindTarget(Player owner)
        {
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

        /// <summary>Get arc formation position for this bell.</summary>
        private Vector2 GetFormationPosition(Player owner)
        {
            // Arc formation: bells spread in a semicircle above the player
            float arcSpan = MathHelper.Pi * 0.6f; // 108 degree arc
            float startAngle = -MathHelper.PiOver2 - arcSpan / 2f; // Centered above

            float angleStep = _totalBells > 1 ? arcSpan / (_totalBells - 1) : 0f;
            float angle = startAngle + angleStep * _bellIndex;

            // Small bob per bell for organic feel
            float bob = (float)Math.Sin(Main.GameUpdateCount * 0.03f + _bellIndex * 1.5f) * 8f;

            return owner.Center + new Vector2(
                (float)Math.Cos(angle) * HoverDistance,
                (float)Math.Sin(angle) * HoverDistance + bob - 30f);
        }

        private void MoveToward(Vector2 target, float speed)
        {
            Vector2 toTarget = target - Projectile.Center;
            float dist = toTarget.Length();

            if (dist > 600f)
            {
                Projectile.Center = target; // Teleport if too far
                Projectile.velocity = Vector2.Zero;
            }
            else if (dist > 4f)
            {
                Projectile.velocity = toTarget * speed;
            }
            else
            {
                Projectile.velocity *= 0.85f;
            }
        }

        #region State Behaviors

        private void IdleBehavior(Player owner, InfernalChimesCallingPlayer icPlayer)
        {
            // Hover in formation
            MoveToward(GetFormationPosition(owner), 0.08f);

            _attackCooldownTimer--;

            // Check if it's our turn to attack
            if (_targetNPC >= 0 && _attackCooldownTimer <= 0)
            {
                // Check if we're the next bell in the stagger sequence
                if (icPlayer.NextBellToFire == _bellIndex && icPlayer.StaggerTimer <= 0)
                {
                    CurrentState = BellState.AttackReady;
                    StateTimer = 0;
                }
            }
        }

        private void AttackReadyBehavior(Player owner, InfernalChimesCallingPlayer icPlayer)
        {
            StateTimer++;

            // Brief windup (pull back slightly)
            if (_targetNPC >= 0)
            {
                NPC target = Main.npc[_targetNPC];
                Vector2 away = Projectile.Center.SafeDirectionTo(target.Center);
                Projectile.velocity = -away * 2f * (1f - (float)StateTimer / 10f);
            }

            // Charge glow particles
            if (StateTimer % 3 == 0)
            {
                InfernalChimesParticleHandler.SpawnParticle(new ChoirEmberParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(25, 25),
                    Main.rand.NextFloat(MathHelper.TwoPi), 12f, -0.08f, 15));
            }

            if (StateTimer >= 10)
            {
                // Fire shockwave!
                FireShockwave(owner, false);

                // Advance the stagger sequence
                icPlayer.NextBellToFire = (_bellIndex + 1) % _totalBells;
                icPlayer.StaggerTimer = InfernalChimesCallingPlayer.StaggerDelay;

                CurrentState = BellState.Firing;
                StateTimer = 0;
            }
        }

        private void FiringBehavior(Player owner)
        {
            StateTimer++;
            Projectile.velocity *= 0.9f;

            if (StateTimer >= FiringDuration)
            {
                CurrentState = BellState.Idle;
                StateTimer = 0;
                _attackCooldownTimer = AttackCooldown;
            }
        }

        private void CrescendoChargeBehavior(Player owner, InfernalChimesCallingPlayer icPlayer)
        {
            StateTimer++;

            // Move toward tighter formation during charge
            Vector2 chargePos = GetFormationPosition(owner);
            MoveToward(chargePos, 0.12f);

            // Intense charging particles 遯ｶ繝ｻspiral inward
            if (StateTimer % 2 == 0)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 30f;
                InfernalChimesParticleHandler.SpawnParticle(new ChoirEmberParticle(
                    Projectile.Center + offset, angle + MathHelper.Pi, 15f, -0.1f, 20));
            }

            // Crescendo ready when player's charge timer hits 0
            if (!icPlayer.CrescendoCharging)
            {
                // Fire Infernal Crescendo!
                FireShockwave(owner, true);
                SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);

                CurrentState = BellState.CrescendoFire;
                StateTimer = 0;
            }
        }

        private void CrescendoFireBehavior(Player owner)
        {
            StateTimer++;
            Projectile.velocity *= 0.85f;

            // Extra particles during crescendo recovery
            if (StateTimer % 3 == 0)
            {
                InfernalChimesParticleHandler.SpawnParticle(new ChoirEmberParticle(
                    Projectile.Center, Main.rand.NextFloat(MathHelper.TwoPi),
                    6f, 0.06f, Main.rand.Next(20, 35)));
            }

            if (StateTimer >= CrescendoFireDuration)
            {
                CurrentState = BellState.Idle;
                StateTimer = 0;
                _attackCooldownTimer = AttackCooldown * 2; // Longer recovery after crescendo
            }
        }

        #endregion

        private void FireShockwave(Player owner, bool isCrescendo)
        {
            if (_targetNPC < 0) return;

            NPC target = Main.npc[_targetNPC];
            int damage = isCrescendo ? (int)(Projectile.damage * 2f) : Projectile.damage;
            float kb = isCrescendo ? Projectile.knockBack * 2f : Projectile.knockBack;

            // ai[0] = 0 normal / 2 = crescendo, ai[1] = 0 normal / 1 = sacrifice
            float crescendoFlag = isCrescendo ? 2f : 0f;
            Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero,
                ModContent.ProjectileType<MinionShockwaveProj>(),
                damage, kb, Projectile.owner, crescendoFlag, 0f);

            // Register for Harmonic Convergence
            var icPlayer = owner.InfernalChimesCalling();
            bool convergence = icPlayer.RegisterShockwave(_totalBells);
            // Harmonic Convergence bonus damage is handled in MinionShockwaveProj when waves overlap

            SoundEngine.PlaySound(SoundID.Item28, Projectile.Center);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);
        }

        private static int _lastParticleDrawFrame;

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {

            // Draw particles (only from first bell to avoid multi-draw)
            if (_bellIndex == 0)
            {
                int currentFrame = (int)Main.GameUpdateCount;
                if (_lastParticleDrawFrame != currentFrame)
                {
                    _lastParticleDrawFrame = currentFrame;
                    InfernalChimesParticleHandler.DrawAllParticles(sb);
                }
            }

            // Draw minion sprite
            var tex = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Color drawColor = Lighting.GetColor((int)(Projectile.Center.X / 16f), (int)(Projectile.Center.Y / 16f));

            // Glow intensity based on state
            float glowIntensity = CurrentState switch
            {
                BellState.AttackReady => 0.5f,
                BellState.Firing => 0.4f,
                BellState.CrescendoCharge => 0.3f + 0.4f * (float)Math.Sin(StateTimer * 0.15f),
                BellState.CrescendoFire => 0.7f * (1f - (float)StateTimer / CrescendoFireDuration),
                _ => 0.15f + (float)Math.Sin(Main.GameUpdateCount * 0.05f + _bellIndex * 1.2f) * 0.1f
            };

            drawColor = Color.Lerp(drawColor, Color.White, glowIntensity);

            SpriteEffects fx = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            sb.Draw(tex, drawPos, null, drawColor, Projectile.rotation, tex.Size() / 2f, Projectile.scale, fx, 0f);

            // Bloom overlay
            Texture2D bloomTex = null;
            try
            {
                bloomTex = ModContent.Request<Texture2D>(
                    "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow",
                    ReLogic.Content.AssetRequestMode.ImmediateLoad)?.Value;
            }
            catch { }

            if (bloomTex != null)
            {
                try { sb.End(); } catch { }
                try
                {
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Color bloomCol = InfernalChimesCallingUtils.Additive(
                    InfernalChimesCallingUtils.ChoirPalette[2], glowIntensity * 0.35f);
                sb.Draw(bloomTex, drawPos, null, bloomCol, 0f, bloomTex.Size() / 2f, 0.3f, SpriteEffects.None, 0f);

                // Extra glow during Crescendo charge
                if (CurrentState == BellState.CrescendoCharge)
                {
                    float chargeGlow = (float)StateTimer / InfernalChimesCallingPlayer.CrescendoChargeTime;
                    Color chargeColor = InfernalChimesCallingUtils.Additive(
                        InfernalChimesCallingUtils.ChoirPalette[4], chargeGlow * 0.4f);
                    sb.Draw(bloomTex, drawPos, null, chargeColor, 0f, bloomTex.Size() / 2f,
                        0.2f + chargeGlow * 0.3f, SpriteEffects.None, 0f);

                    // --- LC Power Effect Ring 遯ｶ繝ｻbuilding concentric ring during crescendo charge ---
                    float ringRot = (float)Main.GameUpdateCount * 0.03f + _bellIndex * MathHelper.PiOver4;
                    LaCampanellaVFXLibrary.DrawPowerEffectRing(sb, drawPos,
                        0.25f + chargeGlow * 0.2f, ringRot,
                        0.2f * chargeGlow, LaCampanellaPalette.InfernalOrange);

                    // --- LC Bright Star 遯ｶ繝ｻbuilding star at crescendo peak ---
                    if (chargeGlow > 0.5f)
                    {
                        float starIntensity = (chargeGlow - 0.5f) * 2f;
                        LaCampanellaVFXLibrary.DrawBrightStar(sb, drawPos,
                            0.12f + starIntensity * 0.1f, -ringRot * 1.5f,
                            0.3f * starIntensity, LaCampanellaPalette.BellGold);
                    }
                }
                }
                catch { }
                finally
                {
                    try { sb.End(); } catch { }
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }

            // Theme texture accents
            try { sb.End(); } catch { }
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            InfernalChimesCallingUtils.DrawThemeAccents(sb, Projectile.Center - Main.screenPosition, Projectile.scale);
            try { sb.End(); } catch { }
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            } // end outer try
            catch
            {
                try
                {
                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                }
                catch { }
            }
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            _trailRenderer?.Dispose();
            _trailRenderer = null;

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