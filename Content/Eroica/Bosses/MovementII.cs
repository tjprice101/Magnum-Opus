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
    /// Movement II - Eroica miniboss with boss-quality attack patterns.
    /// Features: Mini Triumphant Charge (multi-dash), Mini Valor Cross, Mini Phoenix Swoop
    /// All attacks use proper telegraphs, warnings, and cascading VFX.
    /// </summary>
    public class MovementII : ModNPC
    {
        public override string Texture => "MagnumOpus/Content/Eroica/Bosses/ArchangelOfEroica";

        // Theme colors
        private static readonly Color EroicaGold = new Color(255, 200, 80);
        private static readonly Color EroicaScarlet = new Color(200, 50, 50);
        private static readonly Color EroicaPink = new Color(255, 120, 180);
        
        private int parentBossIndex = -1;
        private float orbitAngle = 0f;
        private const float BaseOrbitRadius = 150f;
        private const float BaseOrbitSpeed = 0.02f;
        
        // Fluid movement
        private float waveOffset = 0f;
        private float radiusWobble = 0f;
        private float speedVariation = 0f;
        
        // Attack state machine
        private enum AttackState
        {
            Orbiting,
            MiniChargeWindup,
            MiniCharging,
            MiniChargeRecovery,
            MiniValorCrossWindup,
            MiniValorCrossFiring,
            MiniPhoenixSwoopWindup,
            MiniPhoenixSwooping,
            Recovery
        }
        
        private AttackState currentAttack = AttackState.Orbiting;
        private Vector2 chargeDirection = Vector2.Zero;
        private Vector2 chargeTarget = Vector2.Zero;
        private float glowIntensity = 0f;
        private int dashCount = 0;
        private int maxDashes = 3;

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
            // Hitbox = 80% of visual size (146x82 × 0.35 scale)
            NPC.width = 41;
            NPC.height = 23;
            NPC.damage = 75;
            NPC.defense = 55;
            NPC.lifeMax = 240254;
            NPC.HitSound = SoundID.NPCHit54;
            NPC.DeathSound = SoundID.NPCDeath52;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = 0;
            NPC.aiStyle = -1;
            NPC.scale = 0.35f;
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
            waveOffset += 0.06f;
            radiusWobble = (float)Math.Sin(waveOffset * 0.8f) * 35f;
            speedVariation = (float)Math.Sin(waveOffset * 1.3f) * 0.012f;

            // Glow (intensifies during attacks)
            float baseGlow = 0.8f + glowIntensity * 0.5f;
            Lighting.AddLight(NPC.Center, 0.9f * baseGlow, 0.4f * baseGlow, 0.6f * baseGlow);

            // Execute current state
            switch (currentAttack)
            {
                case AttackState.Orbiting:
                    OrbitalBehavior(parentBoss, target);
                    break;
                case AttackState.MiniChargeWindup:
                    MiniChargeWindup(target);
                    break;
                case AttackState.MiniCharging:
                    MiniCharging(target);
                    break;
                case AttackState.MiniChargeRecovery:
                    MiniChargeRecovery(target);
                    break;
                case AttackState.MiniValorCrossWindup:
                    MiniValorCrossWindup(target);
                    break;
                case AttackState.MiniValorCrossFiring:
                    MiniValorCrossFiring(target);
                    break;
                case AttackState.MiniPhoenixSwoopWindup:
                    MiniPhoenixSwoopWindup(target);
                    break;
                case AttackState.MiniPhoenixSwooping:
                    MiniPhoenixSwooping(target, parentBoss);
                    break;
                case AttackState.Recovery:
                    RecoveryBehavior(parentBoss);
                    break;
            }

            // Ambient particles
            bool isAttacking = currentAttack != AttackState.Orbiting && currentAttack != AttackState.Recovery;
            if (Main.rand.NextBool(isAttacking ? 1 : 4))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.PinkTorch, 0f, 0f, 100, default, isAttacking ? 2f : 1.2f);
                dust.noGravity = true;
                dust.velocity = isAttacking ? -NPC.velocity * 0.2f : Vector2.Zero;
            }

            NPC.rotation = isAttacking ? NPC.velocity.ToRotation() : 0f;
        }

        private void OrbitalBehavior(NPC parentBoss, Player target)
        {
            float currentOrbitRadius = BaseOrbitRadius + radiusWobble;
            float currentOrbitSpeed = BaseOrbitSpeed + speedVariation;
            orbitAngle += currentOrbitSpeed;
            
            float offsetAngle = MathHelper.TwoPi / 3f; // 120 degrees offset
            
            float weaveX = (float)Math.Sin(waveOffset * 2.5f) * 25f;
            float weaveY = (float)Math.Sin(waveOffset * 1.8f) * 18f;
            
            Vector2 orbitPosition = parentBoss.Center + new Vector2(
                (float)Math.Cos(orbitAngle + offsetAngle) * currentOrbitRadius + weaveX,
                (float)Math.Sin(orbitAngle + offsetAngle) * currentOrbitRadius + weaveY
            );

            Vector2 direction = orbitPosition - NPC.Center;
            if (direction.Length() > 5f)
            {
                direction.Normalize();
                float speed = 12f + (float)Math.Sin(waveOffset * 2f) * 4f;
                NPC.velocity = Vector2.Lerp(NPC.velocity, direction * speed, 0.12f);
            }
            else
            {
                NPC.velocity *= 0.8f;
            }

            // Attack selection
            AttackCooldown++;
            if (AttackCooldown >= 140 && target.active && !target.dead)
            {
                AttackCooldown = 0;
                SelectNextAttack();
            }
            
            glowIntensity = Math.Max(0f, glowIntensity - 0.02f);
        }

        private void SelectNextAttack()
        {
            int roll = Main.rand.Next(100);
            
            if (roll < 45)
            {
                currentAttack = AttackState.MiniChargeWindup;
                dashCount = 0;
                maxDashes = 2 + Main.rand.Next(2); // 2-3 dashes
            }
            else if (roll < 75)
            {
                currentAttack = AttackState.MiniValorCrossWindup;
            }
            else
            {
                currentAttack = AttackState.MiniPhoenixSwoopWindup;
            }
            
            AttackTimer = 0;
            SubPhase = 0;
        }

        #region Mini Triumphant Charge (Multi-Dash)
        
        private void MiniChargeWindup(Player target)
        {
            AttackTimer++;
            NPC.velocity *= 0.9f;
            
            float progress = AttackTimer / 35f;
            glowIntensity = progress;
            
            // Converging particles
            if (AttackTimer % 3 == 0)
            {
                int count = 4 + (int)(progress * 4);
                for (int i = 0; i < count; i++)
                {
                    float angle = MathHelper.TwoPi * i / count + AttackTimer * 0.12f;
                    float radius = 50f * (1f - progress * 0.5f);
                    Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                    CustomParticles.GenericFlare(pos, Color.Lerp(EroicaPink, EroicaScarlet, progress), 0.25f, 10);
                }
            }
            
            // Warning line toward player
            if (AttackTimer > 15)
            {
                chargeTarget = target.Center + target.velocity * 15f;
                Vector2 toTarget = (chargeTarget - NPC.Center).SafeNormalize(Vector2.UnitX);
                
                // Draw warning trajectory
                for (float dist = 30f; dist < 250f; dist += 25f)
                {
                    Vector2 warningPos = NPC.Center + toTarget * dist;
                    float fade = 1f - dist / 300f;
                    CustomParticles.GenericFlare(warningPos, EroicaScarlet * (0.4f * fade), 0.12f, 4);
                }
            }
            
            if (AttackTimer >= 25)
            {
                AttackTimer = 0;
                chargeDirection = (chargeTarget - NPC.Center).SafeNormalize(Vector2.UnitX);
                currentAttack = AttackState.MiniCharging;
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f, Volume = 0.8f }, NPC.Center);
                
                // Launch VFX
                CustomParticles.GenericFlare(NPC.Center, Color.White, 0.6f, 15);
                for (int i = 0; i < 3; i++)
                {
                    CustomParticles.HaloRing(NPC.Center, Color.Lerp(EroicaPink, EroicaScarlet, i / 3f), 0.2f + i * 0.05f, 10);
                }
            }
        }
        
        private void MiniCharging(Player target)
        {
            AttackTimer++;
            
            // High-speed charge
            NPC.velocity = chargeDirection * 22f;
            
            // Trail particles
            if (AttackTimer % 2 == 0)
            {
                CustomParticles.GenericFlare(NPC.Center, EroicaScarlet, 0.35f, 10);
                
                // Fire projectiles during charge
                if (Main.netMode != NetmodeID.MultiplayerClient && AttackTimer % 8 == 0)
                {
                    Vector2 perpendicular = chargeDirection.RotatedBy(MathHelper.PiOver2);
                    BossProjectileHelper.SpawnHostileOrb(NPC.Center + perpendicular * 15f, perpendicular * 3f, 55, EroicaGold, 0f);
                    BossProjectileHelper.SpawnHostileOrb(NPC.Center - perpendicular * 15f, -perpendicular * 3f, 55, EroicaGold, 0f);
                }
            }
            
            if (AttackTimer >= 21)
            {
                AttackTimer = 0;
                dashCount++;
                currentAttack = AttackState.MiniChargeRecovery;
            }
        }
        
        private void MiniChargeRecovery(Player target)
        {
            AttackTimer++;
            NPC.velocity *= 0.88f;
            
            if (AttackTimer >= 14)
            {
                if (dashCount < maxDashes && target.active && !target.dead)
                {
                    // Prepare for next dash
                    AttackTimer = 0;
                    currentAttack = AttackState.MiniChargeWindup;
                }
                else
                {
                    // Done with dashes
                    currentAttack = AttackState.Recovery;
                    AttackTimer = 0;
                    
                    // Final burst VFX
                    CustomParticles.GenericFlare(NPC.Center, EroicaGold, 0.5f, 18);
                    EnhancedThemedParticles.EroicaMusicNotesEnhanced(NPC.Center, 4, 30f);
                }
            }
        }
        
        #endregion

        #region Mini Valor Cross (4-arm star pattern)
        
        private void MiniValorCrossWindup(Player target)
        {
            AttackTimer++;
            NPC.velocity *= 0.93f;
            
            float progress = AttackTimer / 45f;
            glowIntensity = progress;
            
            // Converging particles
            if (AttackTimer % 3 == 0)
            {
                int count = 4 + (int)(progress * 6);
                for (int i = 0; i < count; i++)
                {
                    float angle = MathHelper.TwoPi * i / count + AttackTimer * 0.08f;
                    float radius = 70f * (1f - progress * 0.5f);
                    Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                    CustomParticles.GenericFlare(pos, Color.Lerp(EroicaPink, EroicaGold, progress), 0.2f, 10);
                }
            }
            
            // Show cross pattern warning (4 arms)
            if (AttackTimer > 20)
            {
                float warningProgress = (AttackTimer - 20f) / 25f;
                float baseAngle = MathHelper.PiOver4 + SubPhase * MathHelper.PiOver4 * 0.25f;
                
                for (int arm = 0; arm < 4; arm++)
                {
                    float armAngle = baseAngle + MathHelper.PiOver2 * arm;
                    float lineLength = 80f + warningProgress * 60f;
                    
                    for (float dist = 20f; dist < lineLength; dist += 20f)
                    {
                        Vector2 warningPos = NPC.Center + armAngle.ToRotationVector2() * dist;
                        CustomParticles.GenericFlare(warningPos, EroicaGold * 0.4f, 0.12f, 4);
                    }
                }
            }
            
            if (AttackTimer >= 32)
            {
                AttackTimer = 0;
                SubPhase = 0;
                currentAttack = AttackState.MiniValorCrossFiring;
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.1f, Volume = 0.7f }, NPC.Center);
            }
        }
        
        private void MiniValorCrossFiring(Player target)
        {
            AttackTimer++;
            NPC.velocity *= 0.92f;
            
            int waveCount = 2;
            int waveDelay = 18;
            
            if (SubPhase < waveCount)
            {
                if (AttackTimer == 1)
                {
                    // Release VFX
                    CustomParticles.GenericFlare(NPC.Center, Color.White, 0.8f, 20);
                    CustomParticles.GenericFlare(NPC.Center, EroicaGold, 0.6f, 18);
                    
                    for (int i = 0; i < 5; i++)
                    {
                        CustomParticles.HaloRing(NPC.Center, Color.Lerp(EroicaPink, EroicaGold, i / 5f), 0.2f + i * 0.08f, 12 + i * 2);
                    }
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        float baseAngle = MathHelper.PiOver4 + SubPhase * MathHelper.PiOver4 * 0.25f;
                        int projectilesPerArm = 3;
                        
                        for (int arm = 0; arm < 4; arm++)
                        {
                            float armAngle = baseAngle + MathHelper.PiOver2 * arm;
                            
                            for (int p = 0; p < projectilesPerArm; p++)
                            {
                                float speed = 9f + p * 2f;
                                Vector2 vel = armAngle.ToRotationVector2() * speed;
                                Color color = arm % 2 == 0 ? EroicaGold : EroicaScarlet;
                                
                                if (p == projectilesPerArm - 1)
                                    BossProjectileHelper.SpawnHostileOrb(NPC.Center, vel * 0.7f, 60, color, 0.02f);
                                else
                                    BossProjectileHelper.SpawnAcceleratingBolt(NPC.Center, vel * 0.7f, 60, color, 8f);
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
                if (AttackTimer >= 14)
                {
                    EnhancedThemedParticles.EroicaMusicNotesEnhanced(NPC.Center, 5, 35f);
                    currentAttack = AttackState.Recovery;
                    AttackTimer = 0;
                }
            }
        }
        
        #endregion

        #region Mini Phoenix Swoop (Dive + Pull Up)
        
        private void MiniPhoenixSwoopWindup(Player target)
        {
            AttackTimer++;
            
            float progress = Math.Min(1f, AttackTimer / 40f);
            glowIntensity = progress;
            
            // Rise up above player
            Vector2 riseTarget = target.Center + new Vector2(0, -300f);
            Vector2 toTarget = (riseTarget - NPC.Center).SafeNormalize(Vector2.Zero);
            NPC.velocity = Vector2.Lerp(NPC.velocity, toTarget * 16f, 0.1f);
            
            // Converging particles
            if (AttackTimer % 4 == 0)
            {
                int count = 4 + (int)(progress * 4);
                for (int i = 0; i < count; i++)
                {
                    float angle = MathHelper.TwoPi * i / count + AttackTimer * 0.1f;
                    float radius = 60f * (1f - progress * 0.4f);
                    Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                    CustomParticles.GenericFlare(pos, Color.Lerp(EroicaPink, EroicaScarlet, progress), 0.2f, 10);
                }
            }
            
            // Ground impact warning
            if (AttackTimer > 20)
            {
                Vector2 impactPos = target.Center;
                float warningRadius = 60f + (AttackTimer - 20) * 2f;
                
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f + AttackTimer * 0.05f;
                    Vector2 warningPos = impactPos + angle.ToRotationVector2() * warningRadius;
                    CustomParticles.GenericFlare(warningPos, EroicaScarlet * 0.4f, 0.15f, 5);
                }
            }
            
            if (AttackTimer >= 28)
            {
                AttackTimer = 0;
                chargeTarget = target.Center;
                chargeDirection = (chargeTarget - NPC.Center).SafeNormalize(Vector2.UnitY);
                currentAttack = AttackState.MiniPhoenixSwooping;
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.5f, Volume = 0.9f }, NPC.Center);
                
                CustomParticles.GenericFlare(NPC.Center, Color.White, 0.8f, 18);
            }
        }
        
        private void MiniPhoenixSwooping(Player target, NPC parentBoss)
        {
            AttackTimer++;
            
            if (AttackTimer <= 25)
            {
                // Diving phase
                NPC.velocity = chargeDirection * 28f;
                
                // Fire trail particles
                if (AttackTimer % 2 == 0)
                {
                    CustomParticles.GenericFlare(NPC.Center, EroicaScarlet, 0.4f, 12);
                    
                    // Shed projectiles during dive
                    if (Main.netMode != NetmodeID.MultiplayerClient && AttackTimer % 6 == 0)
                    {
                        Vector2 perpendicular = chargeDirection.RotatedBy(MathHelper.PiOver2);
                        BossProjectileHelper.SpawnAcceleratingBolt(NPC.Center, perpendicular * 4f, 55, EroicaGold, 6f);
                        BossProjectileHelper.SpawnAcceleratingBolt(NPC.Center, -perpendicular * 4f, 55, EroicaGold, 6f);
                    }
                }
            }
            else if (AttackTimer <= 45)
            {
                // Pull up and away
                Vector2 awayDir = (parentBoss.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
                awayDir.Y -= 0.5f;
                awayDir = awayDir.SafeNormalize(Vector2.UnitY);
                NPC.velocity = Vector2.Lerp(NPC.velocity, awayDir * 18f, 0.12f);
                
                // Arc trail
                if (AttackTimer % 3 == 0)
                {
                    CustomParticles.GenericFlare(NPC.Center, EroicaGold, 0.3f, 10);
                }
            }
            else
            {
                // Impact effect at end
                if (AttackTimer == 46)
                {
                    CustomParticles.GenericFlare(chargeTarget, Color.White, 0.7f, 20);
                    CustomParticles.GenericFlare(chargeTarget, EroicaScarlet, 0.5f, 18);
                    
                    for (int i = 0; i < 4; i++)
                    {
                        CustomParticles.HaloRing(chargeTarget, Color.Lerp(EroicaScarlet, EroicaGold, i / 4f), 0.3f + i * 0.1f, 12 + i * 2);
                    }
                    
                    EnhancedThemedParticles.EroicaMusicNotesEnhanced(chargeTarget, 4, 35f);
                }
                
                if (AttackTimer >= 39)
                {
                    currentAttack = AttackState.Recovery;
                    AttackTimer = 0;
                }
            }
        }
        
        #endregion

        private void RecoveryBehavior(NPC parentBoss)
        {
            AttackTimer++;
            glowIntensity = Math.Max(0f, glowIntensity - 0.05f);
            
            float currentOrbitRadius = BaseOrbitRadius + radiusWobble;
            float offsetAngle = MathHelper.TwoPi / 3f;
            Vector2 orbitPosition = parentBoss.Center + (orbitAngle + offsetAngle).ToRotationVector2() * currentOrbitRadius;
            Vector2 toOrbit = (orbitPosition - NPC.Center).SafeNormalize(Vector2.Zero);
            NPC.velocity = Vector2.Lerp(NPC.velocity, toOrbit * 14f, 0.08f);
            
            if (AttackTimer >= 28)
            {
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
                    orbitAngle = MathHelper.TwoPi / 3f;
                    return;
                }
            }
            parentBossIndex = -1;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // Essence drops only after killing the main Eroica boss
            LeadingConditionRule afterBossRule = new LeadingConditionRule(new DownedEroicaCondition());
            afterBossRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<RemnantOfEroicasTriumph>(), 1, 10, 20));
            npcLoot.Add(afterBossRule);
        }

        public override void OnKill()
        {
            UnifiedVFXBloom.Eroica.ImpactEnhanced(NPC.Center, 1.5f);
            EnhancedThemedParticles.EroicaBloomBurstEnhanced(NPC.Center, 1.2f);
            
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 offset = angle.ToRotationVector2() * 35f;
                Color flareColor = Color.Lerp(EroicaPink, EroicaGold, (float)i / 8f);
                EnhancedParticles.BloomFlare(NPC.Center + offset, flareColor, 0.45f, 20, 4, 0.9f);
            }
            
            EnhancedThemedParticles.EroicaMusicNotesEnhanced(NPC.Center, 8, 50f);

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Main.NewText("Movement II has concluded...", 255, 120, 180);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Npc[Type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
            
            // Trail (longer during charges)
            for (int k = 0; k < NPC.oldPos.Length; k++)
            {
                Vector2 drawPos = NPC.oldPos[k] - screenPos + new Vector2(NPC.width / 2, NPC.height / 2);
                float trailProgress = (float)(NPC.oldPos.Length - k) / NPC.oldPos.Length;
                Color trailColor = Color.Lerp(EroicaPink * 0.3f, EroicaScarlet * 0.5f, trailProgress) * trailProgress;
                float trailScale = NPC.scale * (glowIntensity > 0.5f ? 1f : 0.8f) * trailProgress;
                spriteBatch.Draw(texture, drawPos, null, trailColor, NPC.oldRot[k], drawOrigin, trailScale, SpriteEffects.None, 0f);
            }
            
            // Glow during attacks
            if (glowIntensity > 0.1f)
            {
                Color glowColor = Color.Lerp(EroicaPink, EroicaScarlet, (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.5f + 0.5f) * glowIntensity * 0.4f;
                for (int i = 0; i < 3; i++)
                {
                    float glowScale = NPC.scale * (1.1f + i * 0.1f);
                    spriteBatch.Draw(texture, NPC.Center - screenPos, null, glowColor * (1f - i * 0.3f), NPC.rotation, drawOrigin, glowScale, SpriteEffects.None, 0f);
                }
            }

            return true;
        }

        public override Color? GetAlpha(Color drawColor)
        {
            return new Color(255, 160, 200, 200);
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            scale = 1.2f;
            return null;
        }
    }
}
