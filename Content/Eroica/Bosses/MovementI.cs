using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Content.Eroica.Projectiles;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.Eroica.Bosses
{
    /// <summary>
    /// Movement I - Eroica miniboss with boss-quality attack patterns.
    /// Features: Mini Heroic Barrage, Mini Golden Rain, Mini Radial Burst
    /// All attacks use proper telegraphs, warnings, and cascading VFX.
    /// </summary>
    public class MovementI : ModNPC
    {
        public override string Texture => "MagnumOpus/Content/Eroica/Bosses/ArchangelOfEroica";

        // Theme colors (matching main boss)
        private static readonly Color EroicaGold = new Color(255, 200, 80);
        private static readonly Color EroicaScarlet = new Color(200, 50, 50);
        private static readonly Color EroicaPink = new Color(255, 150, 200);
        
        private int parentBossIndex = -1;
        private float orbitAngle = 0f;
        private const float BaseOrbitRadius = 150f;
        private const float BaseOrbitSpeed = 0.02f;
        
        // Fluid movement variables
        private float waveOffset = 0f;
        private float radiusWobble = 0f;
        private float speedVariation = 0f;
        
        // Attack system - proper state machine like main boss
        private enum AttackState
        {
            Orbiting,
            MiniBarrageWindup,
            MiniBarrageFiring,
            MiniGoldenRainWindup,
            MiniGoldenRainFiring,
            MiniRadialBurstWindup,
            MiniRadialBurstFiring,
            Recovery
        }
        
        private AttackState currentAttack = AttackState.Orbiting;
        private float glowIntensity = 0f;

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
            NPCID.Sets.TrailCacheLength[Type] = 8;
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
            NPC.damage = 60;
            NPC.defense = 60;
            NPC.lifeMax = 240254;
            NPC.HitSound = SoundID.NPCHit54;
            NPC.DeathSound = SoundID.NPCDeath52;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = 0;
            NPC.aiStyle = -1;
            NPC.dontTakeDamage = false;
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
            
            // Update fluid movement
            waveOffset += 0.05f;
            radiusWobble = (float)Math.Sin(waveOffset * 0.7f) * 30f;
            speedVariation = (float)Math.Sin(waveOffset * 1.2f) * 0.01f;
            
            // Pink glow (intensifies during attacks)
            float baseGlow = 0.8f + glowIntensity * 0.4f;
            Lighting.AddLight(NPC.Center, 0.9f * baseGlow, 0.4f * baseGlow, 0.6f * baseGlow);
            
            // Execute current attack state
            switch (currentAttack)
            {
                case AttackState.Orbiting:
                    OrbitalBehavior(parentBoss, target);
                    break;
                case AttackState.MiniBarrageWindup:
                    MiniBarrageWindup(target);
                    break;
                case AttackState.MiniBarrageFiring:
                    MiniBarrageFiring(target);
                    break;
                case AttackState.MiniGoldenRainWindup:
                    MiniGoldenRainWindup(target);
                    break;
                case AttackState.MiniGoldenRainFiring:
                    MiniGoldenRainFiring(target);
                    break;
                case AttackState.MiniRadialBurstWindup:
                    MiniRadialBurstWindup(target);
                    break;
                case AttackState.MiniRadialBurstFiring:
                    MiniRadialBurstFiring(target);
                    break;
                case AttackState.Recovery:
                    RecoveryBehavior(parentBoss);
                    break;
            }
            
            // Ambient particles
            if (Main.rand.NextBool(4))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.PinkTorch, 0f, 0f, 100, default, 1.2f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }

            NPC.rotation = 0f;
        }

        private void OrbitalBehavior(NPC parentBoss, Player target)
        {
            // Fluid orbit
            float currentOrbitRadius = BaseOrbitRadius + radiusWobble;
            float currentOrbitSpeed = BaseOrbitSpeed + speedVariation;
            orbitAngle += currentOrbitSpeed;
            
            float wobbleX = (float)Math.Sin(waveOffset * 2f) * 20f;
            float wobbleY = (float)Math.Sin(waveOffset * 1.5f) * 15f;
            
            Vector2 orbitPosition = parentBoss.Center + new Vector2(
                (float)Math.Cos(orbitAngle) * currentOrbitRadius + wobbleX,
                (float)Math.Sin(orbitAngle) * currentOrbitRadius + wobbleY
            );
            
            Vector2 direction = orbitPosition - NPC.Center;
            if (direction.Length() > 5f)
            {
                direction.Normalize();
                float speed = 12f + (float)Math.Sin(waveOffset * 3f) * 3f;
                NPC.velocity = Vector2.Lerp(NPC.velocity, direction * speed, 0.1f);
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
                currentAttack = AttackState.MiniBarrageWindup;
            }
            else if (roll < 70)
            {
                currentAttack = AttackState.MiniGoldenRainWindup;
            }
            else
            {
                currentAttack = AttackState.MiniRadialBurstWindup;
            }
            
            AttackTimer = 0;
            SubPhase = 0;
        }

        #region Mini Heroic Barrage
        
        private void MiniBarrageWindup(Player target)
        {
            AttackTimer++;
            NPC.velocity *= 0.95f;
            
            float progress = AttackTimer / 40f;
            glowIntensity = progress;
            
            // Telegraph: Converging particles
            if (AttackTimer % 3 == 0)
            {
                int particleCount = 4 + (int)(progress * 4);
                for (int i = 0; i < particleCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / particleCount + AttackTimer * 0.1f;
                    float radius = 60f * (1f - progress * 0.5f);
                    Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                    Color color = Color.Lerp(EroicaPink, EroicaGold, progress);
                    CustomParticles.GenericFlare(pos, color, 0.25f + progress * 0.15f, 10);
                }
            }
            
            // Warning arc toward player
            if (AttackTimer > 20)
            {
                Vector2 toPlayer = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                float arcAngle = MathHelper.ToRadians(35f);
                
                for (int i = -3; i <= 3; i++)
                {
                    float angle = toPlayer.ToRotation() + arcAngle * i / 3f;
                    Vector2 warningPos = NPC.Center + angle.ToRotationVector2() * (60f + (AttackTimer - 20) * 3f);
                    CustomParticles.GenericFlare(warningPos, EroicaGold * 0.5f, 0.15f, 5);
                }
            }
            
            if (AttackTimer >= 28)
            {
                AttackTimer = 0;
                currentAttack = AttackState.MiniBarrageFiring;
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 0.7f }, NPC.Center);
            }
        }
        
        private void MiniBarrageFiring(Player target)
        {
            AttackTimer++;
            NPC.velocity *= 0.92f;
            
            if (AttackTimer == 1 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                // Attack release VFX
                CustomParticles.GenericFlare(NPC.Center, Color.White, 0.7f, 18);
                CustomParticles.GenericFlare(NPC.Center, EroicaGold, 0.5f, 15);
                
                for (int i = 0; i < 4; i++)
                {
                    Color haloColor = Color.Lerp(EroicaPink, EroicaGold, i / 4f);
                    CustomParticles.HaloRing(NPC.Center, haloColor, 0.2f + i * 0.08f, 12 + i * 2);
                }
                
                // Fire spread
                Vector2 toPlayer = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                int projectileCount = 7;
                float arcAngle = MathHelper.ToRadians(50f);
                
                for (int i = 0; i < projectileCount; i++)
                {
                    float spreadAngle = MathHelper.Lerp(-arcAngle, arcAngle, (float)i / (projectileCount - 1));
                    Vector2 dir = toPlayer.RotatedBy(spreadAngle);
                    float speed = 10f + Main.rand.NextFloat(2f);
                    
                    if (i % 2 == 0)
                        BossProjectileHelper.SpawnHostileOrb(NPC.Center, dir * speed * 0.7f, 65, EroicaPink, 0.02f);
                    else
                        BossProjectileHelper.SpawnAcceleratingBolt(NPC.Center, dir * speed * 0.6f, 65, EroicaGold, 8f);
                }
                
                EnhancedThemedParticles.EroicaMusicNotesEnhanced(NPC.Center, 4, 30f);
            }
            
            if (AttackTimer >= 18)
            {
                currentAttack = AttackState.Recovery;
                AttackTimer = 0;
            }
        }
        
        #endregion

        #region Mini Golden Rain
        
        private void MiniGoldenRainWindup(Player target)
        {
            AttackTimer++;
            
            float progress = Math.Min(1f, AttackTimer / 32f);
            glowIntensity = progress;
            
            // Rise above player
            Vector2 riseTarget = target.Center + new Vector2(0, -250f);
            Vector2 toTarget = (riseTarget - NPC.Center).SafeNormalize(Vector2.Zero);
            NPC.velocity = Vector2.Lerp(NPC.velocity, toTarget * 14f, 0.08f);
            
            // Warning particles below
            if (AttackTimer % 5 == 0 && AttackTimer > 15)
            {
                for (int i = 0; i < 2; i++)
                {
                    float xOffset = Main.rand.NextFloat(-120f, 120f);
                    Vector2 warningPos = target.Center + new Vector2(xOffset, 0f);
                    CustomParticles.GenericFlare(warningPos, EroicaGold * 0.4f, 0.2f, 8);
                }
            }
            
            // Converging particles
            if (AttackTimer % 4 == 0)
            {
                int count = 3 + (int)(progress * 4);
                for (int i = 0; i < count; i++)
                {
                    float angle = MathHelper.TwoPi * i / count + AttackTimer * 0.08f;
                    float radius = 50f * (1f - progress * 0.4f);
                    Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                    CustomParticles.GenericFlare(pos, Color.Lerp(EroicaPink, EroicaGold, progress), 0.2f, 8);
                }
            }
            
            if (AttackTimer >= 32)
            {
                AttackTimer = 0;
                SubPhase = 0;
                currentAttack = AttackState.MiniGoldenRainFiring;
                SoundEngine.PlaySound(SoundID.Item117 with { Pitch = 0.3f }, NPC.Center);
            }
        }
        
        private void MiniGoldenRainFiring(Player target)
        {
            AttackTimer++;
            
            // Hover above player
            Vector2 hoverTarget = target.Center + new Vector2(0, -250f);
            Vector2 toTarget = (hoverTarget - NPC.Center).SafeNormalize(Vector2.Zero);
            NPC.velocity = Vector2.Lerp(NPC.velocity, toTarget * 6f, 0.05f);
            
            // Rain projectiles
            int fireInterval = 6;
            if (AttackTimer % fireInterval == 0 && AttackTimer <= 42 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                int count = 2;
                for (int i = 0; i < count; i++)
                {
                    float xOffset = Main.rand.NextFloat(-150f, 150f);
                    Vector2 spawnPos = target.Center + new Vector2(xOffset, -350f);
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-1f, 1f), 10f);
                    
                    CustomParticles.GenericFlare(spawnPos, EroicaGold, 0.3f, 8);
                    
                    if (i % 2 == 0)
                        BossProjectileHelper.SpawnAcceleratingBolt(spawnPos, vel * 0.7f, 60, EroicaGold, 12f);
                    else
                        BossProjectileHelper.SpawnHostileOrb(spawnPos, vel, 60, EroicaPink, 0.01f);
                }
            }
            
            if (AttackTimer >= 49)
            {
                currentAttack = AttackState.Recovery;
                AttackTimer = 0;
            }
        }
        
        #endregion

        #region Mini Radial Burst
        
        private void MiniRadialBurstWindup(Player target)
        {
            AttackTimer++;
            NPC.velocity *= 0.95f;
            
            float progress = AttackTimer / 35f;
            glowIntensity = progress;
            
            // Converging particles
            if (AttackTimer % 3 == 0)
            {
                int count = 6 + (int)(progress * 6);
                for (int i = 0; i < count; i++)
                {
                    float angle = MathHelper.TwoPi * i / count + AttackTimer * 0.06f;
                    float radius = 80f * (1f - progress * 0.6f);
                    Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                    Color color = Color.Lerp(EroicaPink, Color.White, progress);
                    CustomParticles.GenericFlare(pos, color, 0.2f + progress * 0.2f, 10);
                }
            }
            
            // Safe arc indicator
            if (AttackTimer > 25)
            {
                float safeAngle = (target.Center - NPC.Center).ToRotation();
                float safeArcWidth = MathHelper.ToRadians(35f);
                
                for (int i = -2; i <= 2; i++)
                {
                    float arcAngle = safeAngle + safeArcWidth * i / 2f;
                    Vector2 markerPos = NPC.Center + arcAngle.ToRotationVector2() * 70f;
                    CustomParticles.GenericFlare(markerPos, Color.Cyan * 0.6f, 0.15f, 5);
                }
            }
            
            if (AttackTimer >= 35)
            {
                AttackTimer = 0;
                currentAttack = AttackState.MiniRadialBurstFiring;
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.2f, Volume = 0.8f }, NPC.Center);
            }
        }
        
        private void MiniRadialBurstFiring(Player target)
        {
            AttackTimer++;
            NPC.velocity *= 0.9f;
            
            if (AttackTimer == 1 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                // Release VFX
                CustomParticles.GenericFlare(NPC.Center, Color.White, 1.0f, 22);
                CustomParticles.GenericFlare(NPC.Center, EroicaGold, 0.8f, 20);
                CustomParticles.GenericFlare(NPC.Center, EroicaPink, 0.6f, 18);
                
                for (int i = 0; i < 6; i++)
                {
                    Color haloColor = Color.Lerp(EroicaPink, EroicaGold, i / 6f);
                    CustomParticles.HaloRing(NPC.Center, haloColor, 0.25f + i * 0.1f, 14 + i * 2);
                }
                
                EnhancedThemedParticles.EroicaMusicNotesEnhanced(NPC.Center, 6, 40f);
                
                // Radial burst with safe arc
                int projectileCount = 20;
                float safeAngle = (target.Center - NPC.Center).ToRotation();
                float safeArc = MathHelper.ToRadians(30f);
                
                for (int i = 0; i < projectileCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / projectileCount;
                    
                    float angleDiff = MathHelper.WrapAngle(angle - safeAngle);
                    if (Math.Abs(angleDiff) < safeArc) continue;
                    
                    float speed = 9f + Main.rand.NextFloat(2f);
                    Vector2 vel = angle.ToRotationVector2() * speed;
                    
                    if (i % 3 == 0)
                        BossProjectileHelper.SpawnHostileOrb(NPC.Center, vel * 0.7f, 60, EroicaPink, 0.025f);
                    else
                        BossProjectileHelper.SpawnAcceleratingBolt(NPC.Center, vel * 0.7f, 60, EroicaGold, 10f);
                }
            }
            
            if (AttackTimer >= 30)
            {
                currentAttack = AttackState.Recovery;
                AttackTimer = 0;
            }
        }
        
        #endregion

        private void RecoveryBehavior(NPC parentBoss)
        {
            AttackTimer++;
            glowIntensity = Math.Max(0f, glowIntensity - 0.05f);
            
            float currentOrbitRadius = BaseOrbitRadius + radiusWobble;
            Vector2 orbitPosition = parentBoss.Center + orbitAngle.ToRotationVector2() * currentOrbitRadius;
            Vector2 toOrbit = (orbitPosition - NPC.Center).SafeNormalize(Vector2.Zero);
            NPC.velocity = Vector2.Lerp(NPC.velocity, toOrbit * 12f, 0.08f);
            
            if (AttackTimer >= 32)
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
                    orbitAngle = 0f;
                    return;
                }
            }
            parentBossIndex = -1;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<RemnantOfEroicasTriumph>(), 1, 10, 20));
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
                Main.NewText("Movement I has concluded...", 255, 150, 200);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Npc[Type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
            
            for (int k = 0; k < NPC.oldPos.Length; k++)
            {
                Vector2 drawPos = NPC.oldPos[k] - screenPos + new Vector2(NPC.width / 2, NPC.height / 2);
                float trailProgress = (float)(NPC.oldPos.Length - k) / NPC.oldPos.Length;
                Color trailColor = Color.Lerp(EroicaPink * 0.3f, EroicaGold * 0.5f, trailProgress) * trailProgress;
                spriteBatch.Draw(texture, drawPos, null, trailColor, NPC.rotation, drawOrigin, NPC.scale * 0.8f * trailProgress, SpriteEffects.None, 0f);
            }
            
            if (glowIntensity > 0.1f)
            {
                Color glowColor = Color.Lerp(EroicaPink, EroicaGold, (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.5f + 0.5f) * glowIntensity * 0.4f;
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
            return new Color(255, 180, 220, 200);
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            scale = 1.2f;
            return null;
        }
    }
}
