using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.EnigmaVariations.Debuffs;
using MagnumOpus.Content.EnigmaVariations.ResonanceEnergies;
using MagnumOpus.Content.EnigmaVariations.ResonantOres;

namespace MagnumOpus.Content.EnigmaVariations.Enemies
{
    /// <summary>
    /// Mystery's End - A mysterious Jungle creature from the Enigma theme.
    /// Features ±15% size variation for visual variety.
    /// Spawns in the Jungle biome after Moon Lord is defeated.
    /// </summary>
    public class MysterysEnd : ModNPC
    {
        // Texture path
        public override string Texture => "MagnumOpus/Content/EnigmaVariations/Enemies/MysterysEnd";
        
        // Enigma theme colors
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        // Size variation tracking
        private float sizeMultiplier = 1f;
        private bool hasSetSize = false;
        
        // Animation
        private int frameCounter = 0;
        private int currentFrame = 0;
        private const int TotalFrames = 8;
        private const int FrameSpeed = 6;
        
        // Visual effects
        private float eyeGlow = 0f;
        private float auraPulse = 0f;
        
        // AI states
        private enum AIState
        {
            Idle,
            Walking,
            Jumping,
            Attacking
        }
        
        private AIState State
        {
            get => (AIState)NPC.ai[0];
            set => NPC.ai[0] = (float)value;
        }
        
        private float Timer
        {
            get => NPC.ai[1];
            set => NPC.ai[1] = value;
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = TotalFrames;
            
            NPCID.Sets.TrailCacheLength[Type] = 5;
            NPCID.Sets.TrailingMode[Type] = 0;
            
            // Debuff immunities
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
        }

        public override void SetDefaults()
        {
            NPC.width = 50;
            NPC.height = 44;
            NPC.damage = 80;
            NPC.defense = 35;
            NPC.lifeMax = 3500;
            NPC.HitSound = SoundID.NPCHit8;
            NPC.DeathSound = SoundID.NPCDeath10;
            NPC.knockBackResist = 0.25f;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.value = Item.buyPrice(silver: 50);
            NPC.aiStyle = -1; // Custom AI
            NPC.lavaImmune = false;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Jungle,
                new FlavorTextBestiaryInfoElement("Mystery's End - " +
                    "A creature born from the unknown depths of the Enigma. " +
                    "Its many eyes see truths hidden from mortal perception.")
            });
        }

        public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
        {
            // Apply size variation on spawn
            if (!hasSetSize)
            {
                // ±15% size variation
                sizeMultiplier = 0.85f + Main.rand.NextFloat() * 0.3f; // 0.85 to 1.15
                NPC.scale = sizeMultiplier;
                
                // Scale stats slightly with size
                NPC.lifeMax = (int)(NPC.lifeMax * sizeMultiplier);
                NPC.life = NPC.lifeMax;
                NPC.damage = (int)(NPC.damage * sizeMultiplier);
                NPC.defense = (int)(NPC.defense * sizeMultiplier);
                
                // Adjust hitbox based on scale
                NPC.width = (int)(50 * sizeMultiplier);
                NPC.height = (int)(44 * sizeMultiplier);
                
                hasSetSize = true;
            }
        }

        public override void AI()
        {
            // Update timers
            Timer++;
            auraPulse += 0.05f;
            eyeGlow = 0.5f + (float)Math.Sin(auraPulse * 2f) * 0.3f;
            
            // Lighting
            float lightIntensity = eyeGlow * 0.4f;
            Lighting.AddLight(NPC.Center, EnigmaGreen.ToVector3() * lightIntensity);
            
            // Ambient particles
            SpawnAmbientParticles();
            
            NPC.TargetClosest(true);
            Player target = Main.player[NPC.target];
            
            // Despawn if no valid target
            if (!target.active || target.dead)
            {
                NPC.velocity.Y += 0.1f;
                if (NPC.timeLeft > 60)
                    NPC.timeLeft = 60;
                return;
            }
            
            // Ground check
            bool grounded = CheckGrounded();
            
            // State machine
            switch (State)
            {
                case AIState.Idle:
                    IdleBehavior(target, grounded);
                    break;
                case AIState.Walking:
                    WalkingBehavior(target, grounded);
                    break;
                case AIState.Jumping:
                    JumpingBehavior(target, grounded);
                    break;
                case AIState.Attacking:
                    AttackingBehavior(target, grounded);
                    break;
            }
            
            // Face target
            if (State != AIState.Jumping)
            {
                NPC.spriteDirection = NPC.direction = (target.Center.X > NPC.Center.X) ? 1 : -1;
            }
        }
        
        private bool CheckGrounded()
        {
            Vector2 bottomCenter = new Vector2(NPC.Center.X, NPC.position.Y + NPC.height + 4);
            Point tilePos = bottomCenter.ToTileCoordinates();
            return WorldGen.SolidTile(tilePos.X, tilePos.Y) && NPC.velocity.Y >= 0;
        }
        
        private void SpawnAmbientParticles()
        {
            // Occasional void particles
            if (Main.rand.NextBool(20))
            {
                Vector2 particlePos = NPC.Center + Main.rand.NextVector2Circular(30, 20);
                Color particleColor = Main.rand.NextBool() ? EnigmaPurple : EnigmaGreen;
                CustomParticles.GenericGlow(particlePos, particleColor * 0.4f, 0.2f, 25);
            }
            
            // Eye sparkle
            if (Main.rand.NextBool(30))
            {
                Vector2 eyePos = NPC.Center + new Vector2(0, -10 * sizeMultiplier);
                CustomParticles.GenericFlare(eyePos, EnigmaGreen * 0.6f, 0.15f * sizeMultiplier, 10);
            }
        }
        
        private void IdleBehavior(Player target, bool grounded)
        {
            NPC.velocity.X *= 0.9f;
            
            // Start moving after brief idle
            if (Timer >= 45)
            {
                float distanceToTarget = Vector2.Distance(NPC.Center, target.Center);
                
                if (distanceToTarget < 200f && grounded)
                {
                    // Attack if close
                    State = AIState.Attacking;
                    Timer = 0;
                }
                else if (distanceToTarget > 400f && grounded && Main.rand.NextBool(3))
                {
                    // Jump to close distance
                    State = AIState.Jumping;
                    Timer = 0;
                }
                else
                {
                    // Walk towards target
                    State = AIState.Walking;
                    Timer = 0;
                }
            }
        }
        
        private void WalkingBehavior(Player target, bool grounded)
        {
            float direction = Math.Sign(target.Center.X - NPC.Center.X);
            float walkSpeed = 3.5f * sizeMultiplier;
            
            if (grounded)
            {
                NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, direction * walkSpeed, 0.1f);
                
                // Jump over obstacles
                if (Math.Abs(NPC.velocity.X) < 0.5f && Timer > 30)
                {
                    State = AIState.Jumping;
                    Timer = 0;
                    return;
                }
            }
            
            // Check for attack opportunity
            float distanceToTarget = Vector2.Distance(NPC.Center, target.Center);
            if (distanceToTarget < 150f && Timer > 60)
            {
                State = AIState.Attacking;
                Timer = 0;
                return;
            }
            
            // Occasional jump
            if (Timer > 120 && grounded && Main.rand.NextBool(60))
            {
                State = AIState.Jumping;
                Timer = 0;
                return;
            }
            
            // Return to idle after walking for a while
            if (Timer > 180)
            {
                State = AIState.Idle;
                Timer = 0;
            }
        }
        
        private void JumpingBehavior(Player target, bool grounded)
        {
            if (Timer == 1 && grounded)
            {
                // Execute jump
                Vector2 toTarget = target.Center - NPC.Center;
                float jumpPower = 10f + Math.Abs(toTarget.Y) * 0.01f;
                jumpPower = MathHelper.Clamp(jumpPower, 8f, 16f);
                
                NPC.velocity.Y = -jumpPower;
                NPC.velocity.X = MathHelper.Clamp(toTarget.X * 0.02f, -6f, 6f);
                
                // Jump particles
                CustomParticles.GenericFlare(NPC.Bottom, EnigmaPurple * 0.6f, 0.3f * sizeMultiplier, 15);
                SoundEngine.PlaySound(SoundID.Item24 with { Pitch = 0.2f, Volume = 0.5f }, NPC.Center);
            }
            
            // Air control
            if (!grounded)
            {
                float direction = Math.Sign(target.Center.X - NPC.Center.X);
                NPC.velocity.X += direction * 0.1f;
                NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -8f, 8f);
            }
            
            // Land
            if (grounded && Timer > 15)
            {
                // Landing impact
                CustomParticles.GenericFlare(NPC.Bottom, EnigmaPurple, 0.25f * sizeMultiplier, 12);
                
                State = AIState.Walking;
                Timer = 0;
            }
            
            // Timeout
            if (Timer > 120)
            {
                State = AIState.Idle;
                Timer = 0;
            }
        }
        
        private void AttackingBehavior(Player target, bool grounded)
        {
            NPC.velocity.X *= 0.9f;
            
            // Attack windup
            if (Timer < 30)
            {
                // Charge particles
                if (Timer % 5 == 0)
                {
                    CustomParticles.GenericFlare(NPC.Center, EnigmaGreen, 0.2f + Timer * 0.01f, 12);
                }
                return;
            }
            
            // Execute attack
            if (Timer == 30)
            {
                // Shoot paradox projectile
                Vector2 toTarget = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, toTarget * 10f,
                        ModContent.ProjectileType<MysterysEndProjectile>(), NPC.damage / 2, 3f, Main.myPlayer);
                }
                
                // Attack VFX
                CustomParticles.GenericFlare(NPC.Center, EnigmaGreen, 0.5f * sizeMultiplier, 18);
                CustomParticles.GenericFlare(NPC.Center, EnigmaPurple, 0.4f * sizeMultiplier, 15);
                SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0.3f }, NPC.Center);
            }
            
            // Recovery
            if (Timer >= 60)
            {
                State = AIState.Walking;
                Timer = 0;
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            // Apply Paradox Brand debuff
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 180);
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            // Hit particles
            for (int i = 0; i < 5; i++)
            {
                Color hitColor = Main.rand.NextBool() ? EnigmaPurple : EnigmaGreen;
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                CustomParticles.GenericGlow(NPC.Center + vel, hitColor, 0.25f * sizeMultiplier, 20);
            }
            
            // Death effects
            if (NPC.life <= 0)
            {
                // Death explosion
                UnifiedVFX.EnigmaVariations.Impact(NPC.Center, 0.8f * sizeMultiplier);
                CustomParticles.EnigmaEyeExplosion(NPC.Center, EnigmaGreen, 4, 4f);
                
                for (int i = 0; i < 15; i++)
                {
                    Color deathColor = Color.Lerp(EnigmaPurple, EnigmaGreen, Main.rand.NextFloat());
                    Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                    CustomParticles.GenericGlow(NPC.Center + vel, deathColor, 0.3f * sizeMultiplier, 30);
                }
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            // Only spawn in Jungle after Moon Lord is defeated
            if (!NPC.downedMoonlord)
                return 0f;
            
            if (spawnInfo.Player.ZoneJungle && !spawnInfo.Player.ZoneDungeon)
            {
                return 0.15f; // 15% spawn chance in Jungle
            }
            
            return 0f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // Enigma Resonance Energy
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<EnigmaResonantEnergy>(), 3, 1, 3));
            
            // Enigma Ore
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<EnigmaResonanceOre>(), 4, 2, 5));
        }

        public override void FindFrame(int frameHeight)
        {
            frameCounter++;
            
            int speed = FrameSpeed;
            if (State == AIState.Walking)
                speed = 4; // Faster animation when moving
            else if (State == AIState.Attacking)
                speed = 3;
            
            if (frameCounter >= speed)
            {
                frameCounter = 0;
                currentFrame++;
                if (currentFrame >= TotalFrames)
                    currentFrame = 0;
            }
            
            NPC.frame.Y = currentFrame * frameHeight;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[Type].Value;
            Vector2 drawPos = NPC.Center - screenPos;
            Rectangle frame = NPC.frame;
            Vector2 origin = frame.Size() / 2f;
            SpriteEffects effects = NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            
            // Glow underlay
            if (eyeGlow > 0.3f)
            {
                Color glowColor = EnigmaGreen * eyeGlow * 0.3f;
                spriteBatch.Draw(texture, drawPos, frame, glowColor, 0f, origin, NPC.scale * 1.05f, effects, 0f);
            }
            
            // Main draw
            spriteBatch.Draw(texture, drawPos, frame, drawColor, 0f, origin, NPC.scale, effects, 0f);
            
            // Eye overlay
            Color eyeColor = EnigmaGreen * eyeGlow * 0.4f;
            spriteBatch.Draw(texture, drawPos, frame, eyeColor, 0f, origin, NPC.scale, effects, 0f);
            
            return false;
        }
    }
    
    /// <summary>
    /// Projectile shot by Mystery's End enemy
    /// </summary>
    public class MysterysEndProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 180;
            Projectile.penetrate = 1;
        }
        
        public override void AI()
        {
            // Trail
            if (Main.rand.NextBool(2))
            {
                Color trailColor = Main.rand.NextBool() ? EnigmaPurple : EnigmaGreen;
                CustomParticles.GenericGlow(Projectile.Center, trailColor * 0.5f, 0.2f, 15);
            }
            
            Projectile.rotation = Projectile.velocity.ToRotation();
            Lighting.AddLight(Projectile.Center, EnigmaGreen.ToVector3() * 0.3f);
        }
        
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 180);
        }
        
        public override void OnKill(int timeLeft)
        {
            CustomParticles.GenericFlare(Projectile.Center, EnigmaGreen, 0.4f, 15);
            CustomParticles.GenericFlare(Projectile.Center, EnigmaPurple, 0.3f, 12);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D glow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 pos = Projectile.Center - Main.screenPosition;
            
            Main.spriteBatch.Draw(glow, pos, null, EnigmaGreen, 0f, glow.Size() / 2, 0.4f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(glow, pos, null, EnigmaPurple * 0.6f, 0f, glow.Size() / 2, 0.5f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(glow, pos, null, Color.White * 0.3f, 0f, glow.Size() / 2, 0.2f, SpriteEffects.None, 0f);
            
            return false;
        }
    }
}
