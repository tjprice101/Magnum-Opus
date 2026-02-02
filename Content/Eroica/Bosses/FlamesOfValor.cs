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
    /// Flames of Valor - Eroica boss minion that orbits the main boss.
    /// Attacks by either charging at the player or firing a red/gold beam.
    /// </summary>
    public class FlamesOfValor : ModNPC
    {
        // Animation constants - 6x6 sprite sheet
        private const int FrameColumns = 6;
        private const int FrameRows = 6;
        private const int TotalFrames = 36; // 6x6 = 36 frames
        private const int FrameTime = 4;    // Ticks per frame
        
        private int parentBossIndex = -1;
        private float orbitAngle = 0f;
        private float orbitOffset = 0f; // Different offset for each flame
        private const float OrbitRadius = 180f;
        private const float OrbitSpeed = 0.025f;
        
        // Fluid movement
        private float waveOffset = 0f;
        private float radiusWobble = 0f;
        
        // Attack system
        private enum AttackState
        {
            Orbiting,
            ChargeWindup,
            Charging,
            ChargeReturn,
            BeamWindup,
            BeamFiring,
            // New attacks
            SpiralBurstWindup,
            SpiralBurstFiring,
            RingExplosionWindup,
            RingExplosionFiring,
            DiveBombWindup,
            DiveBombing,
            
            // Foundation pattern attacks (from brainstorming)
            SpiralProjectilePatternWindup,
            SpiralProjectilePatternFiring,
            RecursiveSplitExplosionWindup,
            RecursiveSplitExplosionFiring
        }
        
        private AttackState currentAttack = AttackState.Orbiting;
        private Vector2 chargeDirection = Vector2.Zero;
        private Vector2 returnPosition = Vector2.Zero;
        private Vector2 savedOrbitPosition = Vector2.Zero;
        
        // Animation
        private int frameCounter = 0;
        private int currentFrame = 0;
        
        // Glow effect when attacking
        private float glowIntensity = 0f;
        private bool isGlowing = false;

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

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = TotalFrames;
            NPCID.Sets.TrailCacheLength[Type] = 10;
            NPCID.Sets.TrailingMode[Type] = 1;
            
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Frostburn] = true;
            
            // Register for minimap music note icon
            MinibossMinimapSystem.RegisterEroicaMiniboss(Type);
        }

        public override void SetDefaults()
        {
            // Hitbox = 80% of visual size (544x736 frame × 0.35 scale)
            NPC.width = 152;
            NPC.height = 206;
            NPC.damage = 70;
            NPC.defense = 60;
            NPC.lifeMax = 240254; // Keep original minion health
            NPC.HitSound = SoundID.NPCHit54;
            NPC.DeathSound = SoundID.NPCDeath52;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = 0;
            NPC.aiStyle = -1;
            NPC.scale = 0.35f; // 65% size reduction (35% of original)
        }
        
        public void SetOrbitOffset(float offset)
        {
            orbitOffset = offset;
            orbitAngle = offset; // Start at offset position
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
            
            // Update animation
            UpdateAnimation();
            
            // Update fluid movement variables
            waveOffset += 0.04f;
            radiusWobble = (float)Math.Sin(waveOffset * 0.7f) * 20f;
            
            // Lighting - red and gold
            float lightPulse = 0.8f + glowIntensity * 0.4f;
            Lighting.AddLight(NPC.Center, 1f * lightPulse, 0.6f * lightPulse, 0.2f * lightPulse);
            
            // Spawn ambient particles
            SpawnAmbientParticles();
            
            // State machine
            switch (currentAttack)
            {
                case AttackState.Orbiting:
                    OrbitBehavior(parentBoss, target);
                    break;
                case AttackState.ChargeWindup:
                    ChargeWindupBehavior(target);
                    break;
                case AttackState.Charging:
                    ChargeBehavior(target);
                    break;
                case AttackState.ChargeReturn:
                    ChargeReturnBehavior(parentBoss);
                    break;
                case AttackState.BeamWindup:
                    BeamWindupBehavior(target);
                    break;
                case AttackState.BeamFiring:
                    BeamFiringBehavior(target);
                    break;
                // New attacks
                case AttackState.SpiralBurstWindup:
                    SpiralBurstWindupBehavior(target);
                    break;
                case AttackState.SpiralBurstFiring:
                    SpiralBurstFiringBehavior(target);
                    break;
                case AttackState.RingExplosionWindup:
                    RingExplosionWindupBehavior(target);
                    break;
                case AttackState.RingExplosionFiring:
                    RingExplosionFiringBehavior(target);
                    break;
                case AttackState.DiveBombWindup:
                    DiveBombWindupBehavior(target);
                    break;
                case AttackState.DiveBombing:
                    DiveBombingBehavior(target, parentBoss);
                    break;
                    
                // Foundation pattern attacks
                case AttackState.SpiralProjectilePatternWindup:
                    SpiralProjectilePatternWindupBehavior(target);
                    break;
                case AttackState.SpiralProjectilePatternFiring:
                    SpiralProjectilePatternFiringBehavior(target);
                    break;
                case AttackState.RecursiveSplitExplosionWindup:
                    RecursiveSplitExplosionWindupBehavior(target);
                    break;
                case AttackState.RecursiveSplitExplosionFiring:
                    RecursiveSplitExplosionFiringBehavior(target);
                    break;
            }
            
            // Face movement direction
            if (Math.Abs(NPC.velocity.X) > 1f)
            {
                NPC.spriteDirection = NPC.velocity.X > 0 ? 1 : -1;
            }
        }
        
        private void OrbitBehavior(NPC parentBoss, Player target)
        {
            // Smooth dynamic orbit
            float currentOrbitRadius = OrbitRadius + radiusWobble;
            orbitAngle += OrbitSpeed;
            
            // Figure-8 wobble for organic movement
            float wobbleX = (float)Math.Sin(waveOffset * 2f) * 15f;
            float wobbleY = (float)Math.Sin(waveOffset * 1.5f) * 10f;
            
            Vector2 orbitPosition = parentBoss.Center + new Vector2(
                (float)Math.Cos(orbitAngle + orbitOffset) * currentOrbitRadius + wobbleX,
                (float)Math.Sin(orbitAngle + orbitOffset) * currentOrbitRadius + wobbleY
            );
            
            // Smooth movement to orbit position
            Vector2 direction = orbitPosition - NPC.Center;
            if (direction.Length() > 5f)
            {
                direction.Normalize();
                float speed = 14f + (float)Math.Sin(waveOffset * 3f) * 3f;
                NPC.velocity = Vector2.Lerp(NPC.velocity, direction * speed, 0.12f);
            }
            else
            {
                NPC.velocity *= 0.85f;
            }
            
            // Attack cooldown
            AttackCooldown++;
            
            // Frequently initiate attacks (every 90-150 frames = 1.5-2.5 seconds)
            if (AttackCooldown > 63 + Main.rand.Next(42))
            {
                AttackCooldown = 0;
                AttackTimer = 0;
                savedOrbitPosition = orbitPosition;
                
                // Weighted random attack selection for more variety
                int attackRoll = Main.rand.Next(100);
                
                if (attackRoll < 25) // 25% - Charge attack
                {
                    currentAttack = AttackState.ChargeWindup;
                    chargeDirection = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
                }
                else if (attackRoll < 45) // 20% - Beam attack
                {
                    currentAttack = AttackState.BeamWindup;
                }
                else if (attackRoll < 65) // 20% - Spiral burst
                {
                    currentAttack = AttackState.SpiralBurstWindup;
                }
                else if (attackRoll < 80) // 15% - Ring explosion
                {
                    currentAttack = AttackState.RingExplosionWindup;
                }
                else if (attackRoll < 88) // 8% - Dive bomb
                {
                    currentAttack = AttackState.DiveBombWindup;
                    chargeDirection = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
                }
                else if (attackRoll < 94) // 6% - Expanding spiral pattern
                {
                    currentAttack = AttackState.SpiralProjectilePatternWindup;
                }
                else // 6% - Recursive split explosion
                {
                    currentAttack = AttackState.RecursiveSplitExplosionWindup;
                }
                
                isGlowing = true;
                SoundEngine.PlaySound(SoundID.Item15 with { Pitch = 0.2f, Volume = 0.6f }, NPC.Center);
            }
            
            // Decay glow
            if (glowIntensity > 0)
                glowIntensity -= 0.02f;
        }
        
        private void ChargeWindupBehavior(Player target)
        {
            AttackTimer++;
            
            // Build up glow
            glowIntensity = Math.Min(1f, AttackTimer / 21f);
            
            // Slow down and face target
            NPC.velocity *= 0.9f;
            chargeDirection = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
            
            // VFX - red and gold sparks gathering
            if (AttackTimer % 3 == 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector2 dustOffset = Main.rand.NextVector2Circular(60f, 60f);
                    int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                    Dust dust = Dust.NewDustPerfect(NPC.Center + dustOffset, dustType, -dustOffset * 0.08f, 100, default, 1.8f);
                    dust.noGravity = true;
                }
            }
            
            // Windup complete - launch!
            if (AttackTimer >= 28)
            {
                AttackTimer = 0;
                currentAttack = AttackState.Charging;
                NPC.velocity = chargeDirection * 28f;
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f, Volume = 0.8f }, NPC.Center);
                
                // Burst particles at launch
                for (int i = 0; i < 15; i++)
                {
                    int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                    Dust dust = Dust.NewDustDirect(NPC.Center, 1, 1, dustType, 0f, 0f, 100, default, 2f);
                    dust.noGravity = true;
                    dust.velocity = Main.rand.NextVector2Circular(8f, 8f);
                }
            }
        }
        
        private void ChargeBehavior(Player target)
        {
            AttackTimer++;
            
            // Custom particles during charge
            if (AttackTimer % 3 == 0)
            {
                CustomParticles.EroicaFlare(NPC.Center, 0.4f);
            }
            
            // Afterimage trail
            if (AttackTimer % 2 == 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                    Dust trail = Dust.NewDustPerfect(NPC.Center + Main.rand.NextVector2Circular(NPC.width / 3f, NPC.height / 3f), 
                        dustType, -NPC.velocity * 0.1f, 100, default, 1.5f);
                    trail.noGravity = true;
                }
            }
            
            // Charge duration
            if (AttackTimer >= 14)
            {
                AttackTimer = 0;
                currentAttack = AttackState.ChargeReturn;
                returnPosition = savedOrbitPosition;
                
                // Subtle warm glow on charge end
                CustomParticles.EroicaFlare(NPC.Center, 0.4f);
                CustomParticles.GenericGlow(NPC.Center, new Color(255, 180, 80), 0.5f, 20);
            }
        }
        
        private void ChargeReturnBehavior(NPC parentBoss)
        {
            AttackTimer++;
            
            // Ease back to orbit position
            Vector2 targetPos = parentBoss.Center + new Vector2(
                (float)Math.Cos(orbitAngle + orbitOffset) * OrbitRadius,
                (float)Math.Sin(orbitAngle + orbitOffset) * OrbitRadius
            );
            
            Vector2 direction = targetPos - NPC.Center;
            float distance = direction.Length();
            
            if (distance > 20f)
            {
                direction.Normalize();
                NPC.velocity = Vector2.Lerp(NPC.velocity, direction * 16f, 0.15f);
            }
            else
            {
                NPC.velocity *= 0.8f;
            }
            
            // Return complete or timeout
            if (distance < 30f || AttackTimer >= 42)
            {
                AttackTimer = 0;
                currentAttack = AttackState.Orbiting;
                isGlowing = false;
                glowIntensity = 0f;
            }
        }
        
        private void BeamWindupBehavior(Player target)
        {
            AttackTimer++;
            
            // Build up glow
            glowIntensity = Math.Min(1f, AttackTimer / 32f);
            
            // Slow down and aim at player
            NPC.velocity *= 0.92f;
            
            // VFX - red and gold gathering with beam telegraph
            if (AttackTimer % 4 == 0)
            {
                for (int i = 0; i < 5; i++)
                {
                    Vector2 dustOffset = Main.rand.NextVector2Circular(80f, 80f);
                    int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                    Dust dust = Dust.NewDustPerfect(NPC.Center + dustOffset, dustType, -dustOffset * 0.06f, 100, default, 2f);
                    dust.noGravity = true;
                }
            }
            
            // Telegraph line toward player
            if (AttackTimer > 20)
            {
                float lineProgress = (AttackTimer - 20f) / 30f;
                Vector2 toPlayer = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
                float lineLength = 400f * Math.Min(1f, lineProgress);
                
                for (float dist = 0; dist < lineLength; dist += 30f)
                {
                    Vector2 linePos = NPC.Center + toPlayer * dist;
                    int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                    Dust warning = Dust.NewDustPerfect(linePos, dustType, Vector2.Zero, 150, default, 1.2f);
                    warning.noGravity = true;
                    warning.fadeIn = 0.3f;
                }
            }
            
            // Fire beam!
            if (AttackTimer >= 28 && Main.netMode != NetmodeID.MultiplayerClient) // Fires more often (was 50)
            {
                AttackTimer = 0;
                currentAttack = AttackState.BeamFiring;
                
                Vector2 beamDirection = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
                float beamSpeed = 21.6f; // 20% faster (was 18)
                
                // Fire 3 beams: one toward player, one +45°, one -45°
                float[] angles = { 0f, MathHelper.ToRadians(45f), MathHelper.ToRadians(-45f) };
                foreach (float angle in angles)
                {
                    Vector2 velocity = beamDirection.RotatedBy(angle) * beamSpeed;
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity,
                        ModContent.ProjectileType<FlameOfValorBeam>(), 90, 2f, Main.myPlayer);
                }
                
                SoundEngine.PlaySound(SoundID.Item72 with { Pitch = -0.2f, Volume = 0.9f }, NPC.Center);
                
                // Musical particle burst on attack!
                ThemedParticles.EroicaMusicNotes(NPC.Center, 6, 35f);
                ThemedParticles.EroicaAccidentals(NPC.Center, 3, 25f);
                
                // Burst particles
                for (int i = 0; i < 20; i++)
                {
                    int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                    Dust dust = Dust.NewDustDirect(NPC.Center, 1, 1, dustType, 0f, 0f, 100, default, 2.5f);
                    dust.noGravity = true;
                    dust.velocity = beamDirection * Main.rand.NextFloat(5f, 15f) + Main.rand.NextVector2Circular(3f, 3f);
                }
            }
        }
        
        private void BeamFiringBehavior(Player target)
        {
            AttackTimer++;
            
            // Brief pause after firing
            NPC.velocity *= 0.95f;
            glowIntensity = Math.Max(0f, 1f - AttackTimer / 30f);
            
            if (AttackTimer >= 28)
            {
                AttackTimer = 0;
                currentAttack = AttackState.Orbiting;
                isGlowing = false;
            }
        }
        
        #region New Attacks
        
        private void SpiralBurstWindupBehavior(Player target)
        {
            AttackTimer++;
            glowIntensity = Math.Min(1f, AttackTimer / 25f);
            NPC.velocity *= 0.92f;
            
            // Spiral gathering VFX
            if (AttackTimer % 3 == 0)
            {
                float spiralAngle = AttackTimer * 0.25f;
                for (int i = 0; i < 3; i++)
                {
                    float angle = spiralAngle + MathHelper.TwoPi * i / 3f;
                    float radius = 80f * (1f - AttackTimer / 25f);
                    Vector2 dustPos = NPC.Center + angle.ToRotationVector2() * radius;
                    int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                    Dust dust = Dust.NewDustPerfect(dustPos, dustType, (NPC.Center - dustPos) * 0.08f, 100, default, 1.5f);
                    dust.noGravity = true;
                }
            }
            
            if (AttackTimer >= 25)
            {
                AttackTimer = 0;
                currentAttack = AttackState.SpiralBurstFiring;
            }
        }
        
        private void SpiralBurstFiringBehavior(Player target)
        {
            AttackTimer++;
            NPC.velocity *= 0.95f;
            
            // Fire spiral of projectiles over time
            if (AttackTimer <= 21 && AttackTimer % 4 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                float spiralAngle = AttackTimer * 0.3f;
                int arms = 4;
                
                for (int arm = 0; arm < arms; arm++)
                {
                    float angle = spiralAngle + MathHelper.TwoPi * arm / arms;
                    Vector2 vel = angle.ToRotationVector2() * 14f; // Fast!
                    Color color = arm % 2 == 0 ? new Color(255, 200, 80) : new Color(200, 50, 50);
                    BossProjectileHelper.SpawnHostileOrb(NPC.Center, vel, 75, color, 0f);
                }
                
                CustomParticles.GenericFlare(NPC.Center, new Color(255, 180, 80), 0.4f, 10);
                SoundEngine.PlaySound(SoundID.Item12 with { Pitch = 0.3f + AttackTimer * 0.02f, Volume = 0.5f }, NPC.Center);
            }
            
            if (AttackTimer >= 35)
            {
                AttackTimer = 0;
                currentAttack = AttackState.Orbiting;
                isGlowing = false;
            }
        }
        
        private void RingExplosionWindupBehavior(Player target)
        {
            AttackTimer++;
            glowIntensity = Math.Min(1f, AttackTimer / 32f);
            NPC.velocity *= 0.9f;
            
            // Pulsing ring telegraph
            if (AttackTimer % 8 == 0)
            {
                float ringProgress = AttackTimer / 32f;
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi * i / 12f;
                    Vector2 pos = NPC.Center + angle.ToRotationVector2() * (50f + ringProgress * 30f);
                    int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                    Dust dust = Dust.NewDustPerfect(pos, dustType, Vector2.Zero, 100, default, 1.8f);
                    dust.noGravity = true;
                }
            }
            
            if (AttackTimer >= 32)
            {
                AttackTimer = 0;
                currentAttack = AttackState.RingExplosionFiring;
            }
        }
        
        private void RingExplosionFiringBehavior(Player target)
        {
            AttackTimer++;
            NPC.velocity *= 0.95f;
            
            // Burst ring of fast projectiles
            if (AttackTimer == 1 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                int projectiles = 16;
                for (int i = 0; i < projectiles; i++)
                {
                    float angle = MathHelper.TwoPi * i / projectiles;
                    Vector2 vel = angle.ToRotationVector2() * 16f; // Fast expanding ring!
                    Color color = i % 2 == 0 ? new Color(255, 200, 80) : new Color(200, 50, 50);
                    BossProjectileHelper.SpawnHostileOrb(NPC.Center, vel, 70, color, 0f);
                }
                
                // Burst VFX
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 0.8f }, NPC.Center);
                CustomParticles.GenericFlare(NPC.Center, Color.White, 0.8f, 18);
                for (int i = 0; i < 4; i++)
                {
                    CustomParticles.HaloRing(NPC.Center, Color.Lerp(new Color(200, 50, 50), new Color(255, 200, 80), i / 4f), 0.3f + i * 0.12f, 12 + i * 2);
                }
                ThemedParticles.SakuraPetals(NPC.Center, 8, 50f);
            }
            
            if (AttackTimer >= 28)
            {
                AttackTimer = 0;
                currentAttack = AttackState.Orbiting;
                isGlowing = false;
            }
        }
        
        private void DiveBombWindupBehavior(Player target)
        {
            AttackTimer++;
            glowIntensity = Math.Min(1f, AttackTimer / 21f);
            
            // Rise up above player
            Vector2 riseTarget = target.Center + new Vector2(0, -350f);
            Vector2 toRise = (riseTarget - NPC.Center).SafeNormalize(Vector2.Zero);
            NPC.velocity = Vector2.Lerp(NPC.velocity, toRise * 18f, 0.12f);
            
            // Trail while rising
            if (AttackTimer % 4 == 0)
            {
                int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                Dust dust = Dust.NewDustPerfect(NPC.Center, dustType, -NPC.velocity * 0.1f, 100, default, 1.5f);
                dust.noGravity = true;
            }
            
            // Target player and prepare to dive
            if (AttackTimer >= 21)
            {
                chargeDirection = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
                AttackTimer = 0;
                currentAttack = AttackState.DiveBombing;
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.5f, Volume = 0.9f }, NPC.Center);
            }
        }
        
        private void DiveBombingBehavior(Player target, NPC parentBoss)
        {
            AttackTimer++;
            
            if (AttackTimer == 1)
            {
                NPC.velocity = chargeDirection * 32f; // Fast dive!
                CustomParticles.GenericFlare(NPC.Center, Color.White, 0.7f, 15);
            }
            
            // Trail particles during dive
            if (AttackTimer % 2 == 0)
            {
                CustomParticles.EroicaFlare(NPC.Center, 0.5f);
                
                // Fire trailing projectiles
                if (Main.netMode != NetmodeID.MultiplayerClient && AttackTimer % 6 == 0)
                {
                    Vector2 perp = chargeDirection.RotatedBy(MathHelper.PiOver2);
                    BossProjectileHelper.SpawnHostileOrb(NPC.Center + perp * 15f, perp * 4f, 65, new Color(255, 200, 80), 0f);
                    BossProjectileHelper.SpawnHostileOrb(NPC.Center - perp * 15f, -perp * 4f, 65, new Color(200, 50, 50), 0f);
                }
            }
            
            // End dive and return
            if (AttackTimer >= 18)
            {
                AttackTimer = 0;
                currentAttack = AttackState.ChargeReturn;
                returnPosition = savedOrbitPosition;
                
                // Impact burst
                CustomParticles.GenericFlare(NPC.Center, new Color(255, 180, 80), 0.6f, 15);
                ThemedParticles.SakuraPetals(NPC.Center, 5, 30f);
            }
        }
        
        #region Foundation Pattern Attacks
        
        /// <summary>
        /// SpiralProjectilePattern - Projectiles spawn in an expanding spiral from center.
        /// Uses polar coordinates with incrementing angle and radius.
        /// </summary>
        private void SpiralProjectilePatternWindupBehavior(Player target)
        {
            AttackTimer++;
            glowIntensity = Math.Min(1f, AttackTimer / 28f);
            NPC.velocity *= 0.9f;
            
            // Telegraph: Show spiral arm directions converging
            if (AttackTimer % 4 == 0)
            {
                int armCount = 5;
                float windupAngle = AttackTimer * 0.08f;
                for (int arm = 0; arm < armCount; arm++)
                {
                    float armAngle = windupAngle + MathHelper.TwoPi * arm / armCount;
                    float radius = 100f * (1f - AttackTimer / 28f) + 20f;
                    Vector2 pos = NPC.Center + armAngle.ToRotationVector2() * radius;
                    
                    int dustType = arm % 2 == 0 ? DustID.GoldFlame : DustID.CrimsonTorch;
                    Dust dust = Dust.NewDustPerfect(pos, dustType, (NPC.Center - pos) * 0.06f, 100, default, 1.6f);
                    dust.noGravity = true;
                }
            }
            
            // Converging particles toward center
            if (AttackTimer % 6 == 0)
            {
                CustomParticles.GenericFlare(NPC.Center, new Color(255, 180, 80) * glowIntensity, 0.3f + glowIntensity * 0.2f, 12);
            }
            
            if (AttackTimer >= 28)
            {
                AttackTimer = 0;
                currentAttack = AttackState.SpiralProjectilePatternFiring;
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.4f, Volume = 0.9f }, NPC.Center);
            }
        }
        
        private void SpiralProjectilePatternFiringBehavior(Player target)
        {
            AttackTimer++;
            NPC.velocity *= 0.95f;
            
            // Parameters
            int armCount = 5;
            int projectilesPerArm = 6;
            float spiralTightness = 0.4f; // Radians per projectile
            float expansionSpeed = 25f; // Pixels per projectile outward
            float projectileSpeed = 10f;
            int firingDuration = armCount * projectilesPerArm * 2; // 2 frames per projectile
            
            // Fire projectiles progressively in expanding spiral
            if (AttackTimer <= firingDuration && AttackTimer % 2 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                int projectileIndex = (int)(AttackTimer / 2);
                int currentArm = projectileIndex % armCount;
                int currentProjectileInArm = projectileIndex / armCount;
                
                // Calculate spiral position using polar coordinates
                float baseAngle = MathHelper.TwoPi * currentArm / armCount;
                float spiralOffset = currentProjectileInArm * spiralTightness;
                float angle = baseAngle + spiralOffset;
                float radius = 20f + currentProjectileInArm * expansionSpeed;
                
                Vector2 spawnOffset = angle.ToRotationVector2() * radius;
                Vector2 spawnPos = NPC.Center + spawnOffset;
                Vector2 vel = spawnOffset.SafeNormalize(Vector2.UnitY) * projectileSpeed;
                
                // Alternate colors per arm
                Color color = currentArm % 2 == 0 ? new Color(255, 200, 80) : new Color(200, 50, 50);
                BossProjectileHelper.SpawnHostileOrb(spawnPos, vel, 70, color, 0.005f); // Slight homing
                
                // VFX at spawn
                CustomParticles.GenericFlare(spawnPos, color, 0.35f, 10);
                
                // Sound pitch rises as spiral expands
                if (projectileIndex % 5 == 0)
                {
                    float pitchProgress = (float)projectileIndex / (armCount * projectilesPerArm);
                    SoundEngine.PlaySound(SoundID.Item12 with { Pitch = -0.2f + pitchProgress * 0.6f, Volume = 0.4f }, NPC.Center);
                }
            }
            
            // Central glow during firing
            if (AttackTimer % 4 == 0)
            {
                CustomParticles.GenericFlare(NPC.Center, new Color(255, 180, 80), 0.5f, 8);
            }
            
            if (AttackTimer >= firingDuration + 30)
            {
                AttackTimer = 0;
                currentAttack = AttackState.Orbiting;
                isGlowing = false;
            }
        }
        
        /// <summary>
        /// RecursiveSplitExplosion - Fires large projectiles that split into smaller ones.
        /// Each child can potentially split again (depth limited).
        /// </summary>
        private void RecursiveSplitExplosionWindupBehavior(Player target)
        {
            AttackTimer++;
            glowIntensity = Math.Min(1f, AttackTimer / 32f);
            NPC.velocity *= 0.88f;
            
            // Pulsing expansion telegraph
            if (AttackTimer % 5 == 0)
            {
                float pulseRadius = 30f + (AttackTimer / 32f) * 60f;
                int points = 8;
                for (int i = 0; i < points; i++)
                {
                    float angle = MathHelper.TwoPi * i / points + AttackTimer * 0.02f;
                    Vector2 pos = NPC.Center + angle.ToRotationVector2() * pulseRadius;
                    
                    int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                    Dust dust = Dust.NewDustPerfect(pos, dustType, Vector2.Zero, 100, default, 2f);
                    dust.noGravity = true;
                }
                
                // Growing central glow
                CustomParticles.GenericFlare(NPC.Center, new Color(255, 100, 50), 0.3f + glowIntensity * 0.4f, 10);
            }
            
            // Warning text-style buildup
            if (AttackTimer % 15 == 0)
            {
                CustomParticles.HaloRing(NPC.Center, new Color(255, 180, 80) * glowIntensity, 0.25f + glowIntensity * 0.15f, 12);
            }
            
            if (AttackTimer >= 32)
            {
                AttackTimer = 0;
                currentAttack = AttackState.RecursiveSplitExplosionFiring;
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.3f, Volume = 1.0f }, NPC.Center);
            }
        }
        
        private void RecursiveSplitExplosionFiringBehavior(Player target)
        {
            AttackTimer++;
            NPC.velocity *= 0.95f;
            
            // Fire initial large projectiles that will split
            if (AttackTimer == 1 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                int initialCount = 6;
                for (int i = 0; i < initialCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / initialCount;
                    Vector2 vel = angle.ToRotationVector2() * 8f; // Slower initial projectiles
                    
                    // Spawn splitting projectile with recursion depth 2
                    Projectile.NewProjectile(
                        NPC.GetSource_FromAI(),
                        NPC.Center,
                        vel,
                        ModContent.ProjectileType<EroicaSplittingOrb>(),
                        75,
                        2f,
                        Main.myPlayer,
                        ai0: 2f // recursionDepth
                    );
                }
                
                // Burst VFX
                CustomParticles.GenericFlare(NPC.Center, Color.White, 1.0f, 20);
                for (int i = 0; i < 6; i++)
                {
                    CustomParticles.HaloRing(NPC.Center, Color.Lerp(new Color(200, 50, 50), new Color(255, 200, 80), i / 6f), 0.3f + i * 0.1f, 14 + i * 2);
                }
                ThemedParticles.SakuraPetals(NPC.Center, 10, 60f);
            }
            
            // Dramatic pause while projectiles split
            if (AttackTimer >= 56)
            {
                AttackTimer = 0;
                currentAttack = AttackState.Orbiting;
                isGlowing = false;
            }
        }
        
        #endregion
        
        #endregion
        
        private void SpawnAmbientParticles()
        {
            // Use themed particles for ambient effect
            if (isGlowing)
            {
                ThemedParticles.EroicaAura(NPC.Center, NPC.width * 0.6f);
            }
            else if (Main.rand.NextBool(6))
            {
                ThemedParticles.EroicaAura(NPC.Center, NPC.width * 0.4f);
            }
        }
        
        private void UpdateAnimation()
        {
            frameCounter++;
            int animSpeed = isGlowing ? 3 : FrameTime;
            if (frameCounter >= animSpeed)
            {
                frameCounter = 0;
                currentFrame++;
                if (currentFrame >= TotalFrames)
                    currentFrame = 0;
            }
        }

        private void FindParentBoss()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].type == ModContent.NPCType<EroicasRetribution>())
                {
                    parentBossIndex = i;
                    return;
                }
            }
            parentBossIndex = -1;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // Keep original drops
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<RemnantOfEroicasTriumph>(), 1, 10, 20));
        }

        public override void OnKill()
        {
            // Enhanced death burst with multi-layer bloom - red and gold
            UnifiedVFXBloom.Eroica.ImpactEnhanced(NPC.Center, 2.5f);
            EnhancedThemedParticles.EroicaBloomBurstEnhanced(NPC.Center, 1.8f);
            ThemedParticles.EroicaShockwave(NPC.Center, 1.5f);
            ThemedParticles.SakuraPetals(NPC.Center, 12, NPC.width);
            
            // Radial bloom flare burst
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 offset = angle.ToRotationVector2() * 40f;
                float gradientProgress = (float)i / 8f;
                Color flareColor = Color.Lerp(new Color(220, 50, 50), new Color(255, 215, 0), gradientProgress);
                EnhancedParticles.BloomFlare(NPC.Center + offset, flareColor, 0.5f, 20, 3, 0.85f);
            }
            
            // Music notes cascade
            EnhancedThemedParticles.EroicaMusicNotesEnhanced(NPC.Center, 6, 50f);

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Main.NewText("A Flame of Valor has been extinguished...", 255, 180, 100);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Npc[Type].Value;
            
            // Calculate frame from 6x6 grid
            int frameWidth = texture.Width / FrameColumns;
            int frameHeight = texture.Height / FrameRows;
            int column = currentFrame % FrameColumns;
            int row = currentFrame / FrameColumns;
            Rectangle sourceRect = new Rectangle(column * frameWidth, row * frameHeight, frameWidth, frameHeight);
            Vector2 drawOrigin = new Vector2(frameWidth / 2, frameHeight / 2);
            
            SpriteEffects effects = NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // Draw trail
            for (int k = 0; k < NPC.oldPos.Length; k++)
            {
                Vector2 drawPos = NPC.oldPos[k] - screenPos + new Vector2(NPC.width / 2, NPC.height / 2);
                Color trailColor = new Color(255, 150, 50, 80) * ((float)(NPC.oldPos.Length - k) / NPC.oldPos.Length);
                if (isGlowing)
                    trailColor = new Color(255, 200, 100, 100) * ((float)(NPC.oldPos.Length - k) / NPC.oldPos.Length);
                float scale = NPC.scale * (1f - k * 0.08f);
                spriteBatch.Draw(texture, drawPos, sourceRect, trailColor, NPC.rotation, drawOrigin, scale, effects, 0f);
            }
            
            // Glow effect when attacking
            if (glowIntensity > 0)
            {
                Color glowColor = new Color(255, 200, 100, 0) * glowIntensity * 0.5f;
                for (int i = 0; i < 4; i++)
                {
                    Vector2 offset = new Vector2(4f, 0).RotatedBy(MathHelper.PiOver2 * i);
                    spriteBatch.Draw(texture, NPC.Center - screenPos + offset, sourceRect, glowColor, NPC.rotation, drawOrigin, NPC.scale * 1.1f, effects, 0f);
                }
            }
            
            // Draw main sprite
            Vector2 mainDrawPos = NPC.Center - screenPos;
            Color mainColor = NPC.GetAlpha(drawColor);
            spriteBatch.Draw(texture, mainDrawPos, sourceRect, mainColor, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);

            return false;
        }

        public override Color? GetAlpha(Color drawColor)
        {
            // Bright red-gold tint
            return new Color(255, 220, 180, 230);
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            scale = 1.2f;
            return null;
        }
    }
}
