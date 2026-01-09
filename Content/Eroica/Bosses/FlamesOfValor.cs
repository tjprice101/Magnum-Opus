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
            BeamFiring
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
        }

        public override void SetDefaults()
        {
            NPC.width = 80;   // Hitbox sized to match visual sprite
            NPC.height = 80;  // Hitbox sized to match visual sprite
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
            if (AttackCooldown > 90 + Main.rand.Next(60))
            {
                AttackCooldown = 0;
                AttackTimer = 0;
                savedOrbitPosition = orbitPosition;
                
                // 60% charge, 40% beam
                if (Main.rand.NextBool(3, 5)) // 60% chance
                {
                    currentAttack = AttackState.ChargeWindup;
                    chargeDirection = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
                }
                else
                {
                    currentAttack = AttackState.BeamWindup;
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
            glowIntensity = Math.Min(1f, AttackTimer / 30f);
            
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
            if (AttackTimer >= 40)
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
            if (AttackTimer >= 20)
            {
                AttackTimer = 0;
                currentAttack = AttackState.ChargeReturn;
                returnPosition = savedOrbitPosition;
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
            if (distance < 30f || AttackTimer >= 60)
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
            glowIntensity = Math.Min(1f, AttackTimer / 45f);
            
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
            if (AttackTimer >= 40 && Main.netMode != NetmodeID.MultiplayerClient) // Fires more often (was 50)
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
            
            if (AttackTimer >= 40)
            {
                AttackTimer = 0;
                currentAttack = AttackState.Orbiting;
                isGlowing = false;
            }
        }
        
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
            // Death burst - red and gold
            ThemedParticles.EroicaImpact(NPC.Center, 2.5f);
            ThemedParticles.EroicaShockwave(NPC.Center, 1.5f);
            ThemedParticles.SakuraPetals(NPC.Center, 12, NPC.width);

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
