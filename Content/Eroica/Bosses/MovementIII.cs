using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.Eroica.Bosses
{
    /// <summary>
    /// Movement III - Eroica miniboss with boss-quality attack patterns.
    /// Features: Mini Sakura Storm (spiral), Mini Star Burst, Mini Heroes Judgment
    /// All attacks use proper telegraphs, warnings, and cascading VFX.
    /// </summary>
    public class MovementIII : ModNPC
    {
        public override string Texture => "MagnumOpus/Content/Eroica/Bosses/ArchangelOfEroica";

        // Theme colors
        private static readonly Color EroicaGold = new Color(255, 200, 80);
        private static readonly Color EroicaScarlet = new Color(200, 50, 50);
        private static readonly Color SakuraPink = new Color(255, 150, 180);
        private static readonly Color EroicaCrimson = new Color(180, 30, 60);
        
        private int parentBossIndex = -1;
        private float orbitAngle = 0f;
        private const float BaseOrbitRadius = 175f;
        private const float BaseOrbitSpeed = 0.025f;
        
        // Fluid movement
        private float waveOffset = 0f;
        private float radiusWobble = 0f;
        private float speedVariation = 0f;
        
        // Attack state machine
        private enum AttackState
        {
            Orbiting,
            MiniSakuraStormWindup,
            MiniSakuraStormFiring,
            MiniStarBurstWindup,
            MiniStarBurstFiring,
            MiniJudgmentWindup,
            MiniJudgmentFiring,
            Recovery
        }
        
        private AttackState currentAttack = AttackState.Orbiting;
        private float glowIntensity = 0f;
        private float spiralAngle = 0f;

        private float AttackTimer
        {
            get => NPC.ai[0];
            set => NPC.ai[0] = value;
        }
        
        private float AttackCooldown
        {
            get => NPC.ai[1];
            set => NPC.ai[1] = value;
        }
        
        private int SubPhase
        {
            get => (int)NPC.ai[2];
            set => NPC.ai[2] = value;
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1;
            NPCID.Sets.TrailCacheLength[Type] = 10;
            NPCID.Sets.TrailingMode[Type] = 1;
            
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
            
            // Register for minimap music note icon
            MinibossMinimapSystem.RegisterEroicaMiniboss(Type);
        }

        public override void SetDefaults()
        {
            // Hitbox = 80% of visual size (146x82 × 0.4 scale)
            NPC.width = 46;
            NPC.height = 26;
            NPC.damage = 80;
            NPC.defense = 60;
            NPC.lifeMax = 260254;
            NPC.HitSound = SoundID.NPCHit54;
            NPC.DeathSound = SoundID.NPCDeath52;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = 0;
            NPC.aiStyle = -1;
            NPC.scale = 0.4f;
        }

        public override void AI()
        {
            // Find parent boss
            if (parentBossIndex == -1 || !Main.npc[parentBossIndex].active || 
                Main.npc[parentBossIndex].type != ModContent.NPCType<EroicasRetribution>())
            {
                FindParentBoss();
            }

            if (parentBossIndex == -1)
            {
                NPC.active = false;
                return;
            }

            NPC parentBoss = Main.npc[parentBossIndex];
            Player target = Main.player[parentBoss.target];

            // Fluid movement updates
            waveOffset += 0.07f;
            radiusWobble = (float)Math.Sin(waveOffset * 0.7f) * 40f;
            speedVariation = (float)Math.Sin(waveOffset * 1.4f) * 0.015f;

            // Glow (intensifies during attacks)
            float baseGlow = 0.9f + glowIntensity * 0.6f;
            Lighting.AddLight(NPC.Center, 0.95f * baseGlow, 0.6f * baseGlow, 0.3f * baseGlow);

            // Execute current state
            switch (currentAttack)
            {
                case AttackState.Orbiting:
                    OrbitalBehavior(parentBoss, target);
                    break;
                case AttackState.MiniSakuraStormWindup:
                    MiniSakuraStormWindup(target);
                    break;
                case AttackState.MiniSakuraStormFiring:
                    MiniSakuraStormFiring(target);
                    break;
                case AttackState.MiniStarBurstWindup:
                    MiniStarBurstWindup(target);
                    break;
                case AttackState.MiniStarBurstFiring:
                    MiniStarBurstFiring(target);
                    break;
                case AttackState.MiniJudgmentWindup:
                    MiniJudgmentWindup(target);
                    break;
                case AttackState.MiniJudgmentFiring:
                    MiniJudgmentFiring(target);
                    break;
                case AttackState.Recovery:
                    RecoveryBehavior(parentBoss);
                    break;
            }

            // Ambient particles
            bool isAttacking = currentAttack != AttackState.Orbiting && currentAttack != AttackState.Recovery;
            if (Main.rand.NextBool(isAttacking ? 1 : 3))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.GoldFlame, 0f, 0f, 100, default, isAttacking ? 1.8f : 1.1f);
                dust.noGravity = true;
                dust.velocity = isAttacking ? -NPC.velocity * 0.2f : Main.rand.NextVector2Circular(1f, 1f);
            }

            NPC.rotation = isAttacking ? NPC.velocity.ToRotation() : 0f;
        }

        private void OrbitalBehavior(NPC parentBoss, Player target)
        {
            float currentOrbitRadius = BaseOrbitRadius + radiusWobble;
            float currentOrbitSpeed = BaseOrbitSpeed + speedVariation;
            orbitAngle += currentOrbitSpeed;
            
            float offsetAngle = MathHelper.TwoPi * 2f / 3f; // 240 degrees offset
            
            float weaveX = (float)Math.Sin(waveOffset * 2.2f) * 30f;
            float weaveY = (float)Math.Sin(waveOffset * 1.6f) * 22f;
            
            Vector2 orbitPosition = parentBoss.Center + new Vector2(
                (float)Math.Cos(orbitAngle + offsetAngle) * currentOrbitRadius + weaveX,
                (float)Math.Sin(orbitAngle + offsetAngle) * currentOrbitRadius + weaveY
            );

            Vector2 direction = orbitPosition - NPC.Center;
            if (direction.Length() > 5f)
            {
                direction.Normalize();
                float speed = 13f + (float)Math.Sin(waveOffset * 2f) * 5f;
                NPC.velocity = Vector2.Lerp(NPC.velocity, direction * speed, 0.12f);
            }
            else
            {
                NPC.velocity *= 0.8f;
            }

            // Attack selection
            AttackCooldown++;
            if (AttackCooldown >= 126 && target.active && !target.dead)
            {
                AttackCooldown = 0;
                SelectNextAttack();
            }
            
            glowIntensity = Math.Max(0f, glowIntensity - 0.02f);
        }

        private void SelectNextAttack()
        {
            int roll = Main.rand.Next(100);
            
            if (roll < 40)
            {
                currentAttack = AttackState.MiniSakuraStormWindup;
                spiralAngle = 0f;
            }
            else if (roll < 70)
            {
                currentAttack = AttackState.MiniStarBurstWindup;
            }
            else
            {
                currentAttack = AttackState.MiniJudgmentWindup;
            }
            
            AttackTimer = 0;
            SubPhase = 0;
        }

        #region Mini Sakura Storm (Spiral projectiles while orbiting player)
        
        private void MiniSakuraStormWindup(Player target)
        {
            AttackTimer++;
            NPC.velocity *= 0.92f;
            
            float progress = AttackTimer / 40f;
            glowIntensity = progress;
            
            // Converging sakura particles
            if (AttackTimer % 3 == 0)
            {
                int count = 6 + (int)(progress * 6);
                for (int i = 0; i < count; i++)
                {
                    float angle = MathHelper.TwoPi * i / count + AttackTimer * 0.1f;
                    float radius = 80f * (1f - progress * 0.5f);
                    Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                    CustomParticles.GenericFlare(pos, Color.Lerp(SakuraPink, EroicaGold, progress), 0.22f, 12);
                }
            }
            
            // Sakura petals swirling in
            if (AttackTimer % 5 == 0)
            {
                EnhancedThemedParticles.SakuraPetalsEnhanced(NPC.Center + Main.rand.NextVector2Circular(60f, 60f), 1, 25f);
            }
            
            if (AttackTimer >= 28)
            {
                AttackTimer = 0;
                currentAttack = AttackState.MiniSakuraStormFiring;
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.4f, Volume = 0.8f }, NPC.Center);
                
                CustomParticles.GenericFlare(NPC.Center, Color.White, 0.7f, 18);
            }
        }
        
        private void MiniSakuraStormFiring(Player target)
        {
            AttackTimer++;
            
            int duration = 70;
            int arms = 3;
            
            // Orbit around player while firing
            float orbitSpeed = 0.035f;
            float orbitRadius = 180f;
            spiralAngle += orbitSpeed;
            
            Vector2 orbitPos = target.Center + spiralAngle.ToRotationVector2() * orbitRadius;
            Vector2 toOrbit = (orbitPos - NPC.Center).SafeNormalize(Vector2.Zero);
            NPC.velocity = Vector2.Lerp(NPC.velocity, toOrbit * 14f, 0.08f);
            
            // Fire spiral projectiles
            int fireInterval = 4;
            if (AttackTimer % fireInterval == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                float fireAngle = AttackTimer * 0.18f;
                
                for (int arm = 0; arm < arms; arm++)
                {
                    float armAngle = fireAngle + MathHelper.TwoPi * arm / arms;
                    float speed = 8f;
                    Vector2 vel = armAngle.ToRotationVector2() * speed;
                    
                    Color color = arm % 2 == 0 ? SakuraPink : EroicaGold;
                    
                    if (arm == 0)
                        BossProjectileHelper.SpawnHostileOrb(NPC.Center, vel, 60, color, 0.01f);
                    else
                        BossProjectileHelper.SpawnAcceleratingBolt(NPC.Center, vel * 0.8f, 60, color, 5f);
                }
                
                CustomParticles.GenericFlare(NPC.Center, SakuraPink, 0.3f, 10);
            }
            
            // Ambient sakura
            if (AttackTimer % 8 == 0)
            {
                EnhancedThemedParticles.SakuraPetalsEnhanced(NPC.Center, 2, 30f);
            }
            
            if (AttackTimer >= duration)
            {
                // Finale burst + attack ending cue
                BossVFXOptimizer.AttackEndCue(NPC.Center, EroicaGold, SakuraPink, 0.6f);
                
                CustomParticles.GenericFlare(NPC.Center, EroicaGold, 0.6f, 20);
                EnhancedThemedParticles.EroicaMusicNotesEnhanced(NPC.Center, 5, 40f);
                
                currentAttack = AttackState.Recovery;
                AttackTimer = 0;
            }
        }
        
        #endregion

        #region Mini Star Burst (8-point star pattern)
        
        private void MiniStarBurstWindup(Player target)
        {
            AttackTimer++;
            NPC.velocity *= 0.9f;
            
            float progress = AttackTimer / 50f;
            glowIntensity = progress;
            
            // Converging particles forming star shape
            if (AttackTimer % 3 == 0)
            {
                int count = 8;
                for (int i = 0; i < count; i++)
                {
                    float angle = MathHelper.TwoPi * i / count;
                    float radius = 100f * (1f - progress * 0.6f);
                    Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                    CustomParticles.GenericFlare(pos, Color.Lerp(EroicaGold, EroicaScarlet, progress), 0.25f, 12);
                }
            }
            
            // Warning star pattern
            if (AttackTimer > 25)
            {
                float warningProgress = (AttackTimer - 25f) / 25f;
                
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    float lineLength = 50f + warningProgress * 80f;
                    
                    for (float dist = 20f; dist < lineLength; dist += 18f)
                    {
                        Vector2 warningPos = NPC.Center + angle.ToRotationVector2() * dist;
                        CustomParticles.GenericFlare(warningPos, EroicaGold * 0.35f, 0.1f, 4);
                    }
                }
            }
            
            if (AttackTimer >= 35)
            {
                AttackTimer = 0;
                SubPhase = 0;
                currentAttack = AttackState.MiniStarBurstFiring;
                SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.8f }, NPC.Center);
            }
        }
        
        private void MiniStarBurstFiring(Player target)
        {
            AttackTimer++;
            NPC.velocity *= 0.9f;
            
            int waveCount = 3;
            int waveDelay = 14;
            
            if (SubPhase < waveCount)
            {
                if (AttackTimer == 1)
                {
                    // Release VFX
                    CustomParticles.GenericFlare(NPC.Center, Color.White, 0.9f, 22);
                    CustomParticles.GenericFlare(NPC.Center, EroicaGold, 0.7f, 20);
                    
                    for (int i = 0; i < 6; i++)
                    {
                        CustomParticles.HaloRing(NPC.Center, Color.Lerp(EroicaGold, EroicaScarlet, i / 6f), 0.25f + i * 0.1f, 14 + i * 2);
                    }
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        float rotationOffset = SubPhase * MathHelper.PiOver4 / 4f; // Slight rotation per wave
                        int projectilesPerArm = 2 + SubPhase;
                        
                        for (int arm = 0; arm < 8; arm++)
                        {
                            float armAngle = MathHelper.TwoPi * arm / 8f + rotationOffset;
                            
                            for (int p = 0; p < projectilesPerArm; p++)
                            {
                                float speed = 8f + p * 3f;
                                Vector2 vel = armAngle.ToRotationVector2() * speed;
                                Color color = arm % 2 == 0 ? EroicaGold : EroicaScarlet;
                                
                                BossProjectileHelper.SpawnAcceleratingBolt(NPC.Center, vel * 0.7f, 60, color, 6f);
                            }
                        }
                    }
                }
                
                if (AttackTimer >= waveDelay)
                {
                    AttackTimer = 0;
                    SubPhase++;
                }
            }
            else
            {
                if (AttackTimer >= 18)
                {
                    // Attack ending cue
                    BossVFXOptimizer.AttackEndCue(NPC.Center, EroicaGold, SakuraPink, 0.6f);
                    
                    EnhancedThemedParticles.EroicaMusicNotesEnhanced(NPC.Center, 6, 40f);
                    currentAttack = AttackState.Recovery;
                    AttackTimer = 0;
                }
            }
        }
        
        #endregion

        #region Mini Heroes Judgment (Radial burst with safe arc)
        
        private void MiniJudgmentWindup(Player target)
        {
            AttackTimer++;
            NPC.velocity *= 0.88f;
            
            float progress = Math.Min(1f, AttackTimer / 60f);
            glowIntensity = progress;
            
            // Converging particle ring (like main boss)
            if (AttackTimer % 3 == 0)
            {
                int particleCount = 8 + (int)(progress * 8);
                for (int i = 0; i < particleCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / particleCount + AttackTimer * 0.08f;
                    float radius = 120f * (1f - progress * 0.5f);
                    Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                    Color color = Color.Lerp(EroicaGold, Color.White, progress);
                    CustomParticles.GenericFlare(pos, color, 0.3f + progress * 0.2f, 12);
                }
            }
            
            // Safe zone indicator for player (cyan)
            if (AttackTimer > 30)
            {
                float safeRadius = 80f;
                for (int i = 0; i < 10; i++)
                {
                    float angle = MathHelper.TwoPi * i / 10f + AttackTimer * 0.04f;
                    Vector2 safePos = target.Center + angle.ToRotationVector2() * safeRadius;
                    CustomParticles.GenericFlare(safePos, Color.Cyan * 0.5f, 0.18f, 5);
                }
            }
            
            // Screen shake at end
            if (AttackTimer > 50)
            {
                MagnumScreenEffects.AddScreenShake(progress * 3f);
            }
            
            if (AttackTimer >= 42)
            {
                AttackTimer = 0;
                SubPhase = 0;
                currentAttack = AttackState.MiniJudgmentFiring;
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.3f, Volume = 1f }, NPC.Center);
                MagnumScreenEffects.AddScreenShake(8f);
            }
        }
        
        private void MiniJudgmentFiring(Player target)
        {
            AttackTimer++;
            NPC.velocity *= 0.9f;
            
            int waveCount = 2;
            int waveDelay = 21;
            
            if (SubPhase < waveCount)
            {
                if (AttackTimer == 1)
                {
                    // Massive release VFX
                    CustomParticles.GenericFlare(NPC.Center, Color.White, 1.2f, 25);
                    CustomParticles.GenericFlare(NPC.Center, EroicaGold, 1f, 22);
                    
                    for (int i = 0; i < 8; i++)
                    {
                        CustomParticles.HaloRing(NPC.Center, Color.Lerp(EroicaScarlet, EroicaGold, i / 8f), 0.3f + i * 0.12f, 15 + i * 3);
                    }
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int projectileCount = 20 + SubPhase * 4;
                        float safeAngle = (target.Center - NPC.Center).ToRotation();
                        float safeArc = MathHelper.ToRadians(28f); // 28 degree safe zone
                        
                        for (int i = 0; i < projectileCount; i++)
                        {
                            float angle = MathHelper.TwoPi * i / projectileCount;
                            
                            // Skip projectiles aimed at player (safe arc)
                            float angleDiff = MathHelper.WrapAngle(angle - safeAngle);
                            if (Math.Abs(angleDiff) < safeArc) continue;
                            
                            float speed = 10f + SubPhase * 2f;
                            Vector2 vel = angle.ToRotationVector2() * speed;
                            
                            if (i % 3 == 0)
                                BossProjectileHelper.SpawnHostileOrb(NPC.Center, vel * 0.7f, 65, EroicaGold, 0.015f);
                            else
                                BossProjectileHelper.SpawnAcceleratingBolt(NPC.Center, vel * 0.6f, 65, EroicaScarlet, 8f);
                        }
                    }
                }
                
                if (AttackTimer >= waveDelay)
                {
                    AttackTimer = 0;
                    SubPhase++;
                }
            }
            else
            {
                if (AttackTimer >= 21)
                {
                    // Attack ending cue
                    BossVFXOptimizer.AttackEndCue(NPC.Center, EroicaGold, SakuraPink, 0.6f);
                    
                    EnhancedThemedParticles.EroicaMusicNotesEnhanced(NPC.Center, 8, 50f);
                    currentAttack = AttackState.Recovery;
                    AttackTimer = 0;
                }
            }
        }
        
        #endregion

        private void RecoveryBehavior(NPC parentBoss)
        {
            AttackTimer++;
            float duration = 32f;
            float progress = AttackTimer / duration;
            
            glowIntensity = Math.Max(0f, glowIntensity - 0.05f);
            
            float currentOrbitRadius = BaseOrbitRadius + radiusWobble;
            float offsetAngle = MathHelper.TwoPi * 2f / 3f;
            Vector2 orbitPosition = parentBoss.Center + (orbitAngle + offsetAngle).ToRotationVector2() * currentOrbitRadius;
            Vector2 toOrbit = (orbitPosition - NPC.Center).SafeNormalize(Vector2.Zero);
            
            // Smooth movement using easing - bell curve speed
            float speedCurve = BossAIUtilities.Easing.EaseOutQuad(progress) * BossAIUtilities.Easing.EaseInQuad(1f - progress) * 4f;
            float speed = 15f * Math.Max(0.3f, speedCurve);
            NPC.velocity = Vector2.Lerp(NPC.velocity, toOrbit * speed, 0.08f);
            
            // Recovery shimmer - vulnerability indicator
            if (AttackTimer % 4 == 0)
            {
                BossVFXOptimizer.RecoveryShimmer(NPC.Center, SakuraPink, 45f, progress);
            }
            
            // Deceleration trail while moving
            if (NPC.velocity.Length() > 2f)
            {
                BossVFXOptimizer.DecelerationTrail(NPC.Center, NPC.velocity, EroicaGold, progress);
            }
            
            if (AttackTimer >= 32)
            {
                // Ready to attack again
                BossVFXOptimizer.ReadyToAttackCue(NPC.Center, EroicaScarlet, 0.5f);
                
                currentAttack = AttackState.Orbiting;
                AttackTimer = 0;
            }
        }

        private void FindParentBoss()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].type == ModContent.NPCType<EroicasRetribution>())
                {
                    parentBossIndex = i;
                    orbitAngle = MathHelper.TwoPi * 2f / 3f;
                    return;
                }
            }
            parentBossIndex = -1;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // Essence drops only after killing the main Eroica boss
            LeadingConditionRule afterBossRule = new LeadingConditionRule(new DownedEroicaCondition());
            afterBossRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<RemnantOfEroicasTriumph>(), 1, 15, 25));
            npcLoot.Add(afterBossRule);
        }

        public override void OnKill()
        {
            UnifiedVFXBloom.Eroica.ImpactEnhanced(NPC.Center, 1.6f);
            EnhancedThemedParticles.EroicaBloomBurstEnhanced(NPC.Center, 1.3f);
            
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 offset = angle.ToRotationVector2() * 40f;
                Color flareColor = Color.Lerp(EroicaGold, EroicaScarlet, (float)i / 10f);
                EnhancedParticles.BloomFlare(NPC.Center + offset, flareColor, 0.5f, 22, 4, 0.95f);
            }
            
            EnhancedThemedParticles.SakuraPetalsEnhanced(NPC.Center, 15, 60f);
            EnhancedThemedParticles.EroicaMusicNotesEnhanced(NPC.Center, 10, 55f);

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Main.NewText("Movement III has reached its finale!", 255, 200, 80);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Npc[Type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
            
            // Trail
            for (int k = 0; k < NPC.oldPos.Length; k++)
            {
                Vector2 drawPos = NPC.oldPos[k] - screenPos + new Vector2(NPC.width / 2, NPC.height / 2);
                float trailProgress = (float)(NPC.oldPos.Length - k) / NPC.oldPos.Length;
                Color trailColor = Color.Lerp(EroicaGold * 0.3f, SakuraPink * 0.5f, trailProgress) * trailProgress;
                float trailScale = NPC.scale * (glowIntensity > 0.5f ? 1f : 0.8f) * trailProgress;
                spriteBatch.Draw(texture, drawPos, null, trailColor, NPC.oldRot[k], drawOrigin, trailScale, SpriteEffects.None, 0f);
            }
            
            // Glow during attacks
            if (glowIntensity > 0.1f)
            {
                Color glowColor = Color.Lerp(EroicaGold, SakuraPink, (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.5f + 0.5f) * glowIntensity * 0.4f;
                for (int i = 0; i < 3; i++)
                {
                    float glowScale = NPC.scale * (1.1f + i * 0.12f);
                    spriteBatch.Draw(texture, NPC.Center - screenPos, null, glowColor * (1f - i * 0.3f), NPC.rotation, drawOrigin, glowScale, SpriteEffects.None, 0f);
                }
            }

            return true;
        }

        public override Color? GetAlpha(Color drawColor)
        {
            return new Color(255, 210, 130, 200);
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            scale = 1.3f;
            return null;
        }
    }
}
