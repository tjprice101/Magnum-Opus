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
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.EnigmaVariations.Debuffs;
using MagnumOpus.Content.EnigmaVariations.ResonanceEnergies;
using MagnumOpus.Content.EnigmaVariations.ResonantOres;

namespace MagnumOpus.Content.EnigmaVariations.Enemies
{
    /// <summary>
    /// Mystery's End - A powerful Jungle mini-boss from the Enigma theme.
    /// Features ±15% size variation for visual variety.
    /// Spawns randomly in the Jungle biome after Moon Lord is defeated (5% chance).
    /// Drops Enigma-themed crafting materials similar to Eroica's desert mini-bosses.
    /// 
    /// 5 ENIGMATIC ATTACKS (with glyphs and watching eyes):
    /// 1. Paradox Gaze - Eyes spawn around target, watching and damaging
    /// 2. Glyph Cascade - Raining arcane glyphs from above
    /// 3. Watching Volley - Projectiles with trailing eyes
    /// 4. Mystery Vortex - Swirling glyph circle pulls and damages
    /// 5. Enigma Revelation - Ultimate eye explosion with glyph burst
    /// </summary>
    public class MysterysEnd : ModNPC
    {
        // Texture path
        public override string Texture => "MagnumOpus/Content/EnigmaVariations/Enemies/MysterysEnd";
        
        // Enigma theme colors
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        private static readonly Color EnigmaDeepPurple = new Color(80, 20, 120);
        
        // Size variation tracking
        private float sizeMultiplier = 1f;
        private bool hasSetSize = false;
        
        // Animation - 6x6 spritesheet (36 frames total)
        private int frameCounter = 0;
        private int currentFrame = 0;
        private const int FrameColumns = 6;
        private const int FrameRows = 6;
        private const int TotalFrames = 36; // 6x6 spritesheet
        private const int FrameSpeed = 6;
        
        // Visual effects
        private float eyeGlow = 0f;
        private float auraPulse = 0f;
        
        // Attack tracking
        private int attackCounter = 0;
        private float attackCooldown = 0f;
        
        // AI states - Expanded with 5 unique attacks
        private enum AIState
        {
            Idle,
            Walking,
            Jumping,
            ParadoxGaze,        // Attack 1 - Spawning watching eyes
            GlyphCascade,       // Attack 2 - Raining glyphs
            WatchingVolley,     // Attack 3 - Eye-trailing projectiles
            MysteryVortex,      // Attack 4 - Swirling glyph vortex
            EnigmaRevelation    // Attack 5 - Ultimate eye/glyph explosion
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
            Main.npcFrameCount[Type] = TotalFrames; // 6x6 spritesheet = 36 frames
            
            NPCID.Sets.TrailCacheLength[Type] = 5;
            NPCID.Sets.TrailingMode[Type] = 0;
            
            // Debuff immunities
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            
            // Register for minimap music note icon
            MinibossMinimapSystem.RegisterEnigmaMiniboss(Type);
        }

        public override void SetDefaults()
        {
            // MINI-BOSS STATS - Similar to Eroica mini-bosses
            NPC.width = 80;
            NPC.height = 70;
            NPC.damage = 140;
            NPC.defense = 65;
            NPC.lifeMax = 45000; // Mini-boss HP tier
            NPC.HitSound = SoundID.NPCHit8;
            NPC.DeathSound = SoundID.NPCDeath10;
            NPC.knockBackResist = 0f; // Immune to knockback like other mini-bosses
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.value = Item.buyPrice(gold: 35);
            NPC.aiStyle = -1; // Custom AI
            NPC.lavaImmune = false;
            NPC.npcSlots = 5f; // Mini-boss slot count
            NPC.noGravity = true; // Flying creature
            NPC.noTileCollide = true; // Can pass through tiles while flying
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Jungle,
                new FlavorTextBestiaryInfoElement("Mystery's End - " +
                    "A massive creature born from the unknown depths of the Enigma. " +
                    "Its many eyes see truths hidden from mortal perception. " +
                    "When it emerges from the jungle's depths, it brings with it fragments of arcane power.")
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
                NPC.width = (int)(80 * sizeMultiplier);
                NPC.height = (int)(70 * sizeMultiplier);
                
                hasSetSize = true;
            }
        }

        public override void AI()
        {
            // Update timers
            Timer++;
            auraPulse += 0.05f;
            eyeGlow = 0.5f + (float)Math.Sin(auraPulse * 2f) * 0.3f;
            
            if (attackCooldown > 0f)
                attackCooldown--;
            
            // Lighting
            float lightIntensity = eyeGlow * 0.4f;
            Lighting.AddLight(NPC.Center, EnigmaGreen.ToVector3() * lightIntensity);
            
            // Ambient particles
            SpawnAmbientParticles();
            
            NPC.TargetClosest(true);
            Player target = Main.player[NPC.target];
            
            // Despawn if no valid target - fly away
            if (!target.active || target.dead)
            {
                NPC.velocity.Y -= 0.5f; // Fly upward
                NPC.velocity.X *= 0.98f;
                if (NPC.timeLeft > 60)
                    NPC.timeLeft = 60;
                return;
            }
            
            float distanceToTarget = Vector2.Distance(NPC.Center, target.Center);
            
            // Select attack when ready
            if (attackCooldown <= 0f && distanceToTarget < 500f && 
                (State == AIState.Idle || State == AIState.Walking))
            {
                SelectNextAttack(target, distanceToTarget);
            }
            
            // State machine
            switch (State)
            {
                case AIState.Idle:
                    IdleBehavior(target, false);
                    break;
                case AIState.Walking:
                    WalkingBehavior(target, false);
                    break;
                case AIState.Jumping:
                    JumpingBehavior(target, false);
                    break;
                case AIState.ParadoxGaze:
                    HandleParadoxGaze(target);
                    break;
                case AIState.GlyphCascade:
                    HandleGlyphCascade(target);
                    break;
                case AIState.WatchingVolley:
                    HandleWatchingVolley(target);
                    break;
                case AIState.MysteryVortex:
                    HandleMysteryVortex(target);
                    break;
                case AIState.EnigmaRevelation:
                    HandleEnigmaRevelation(target);
                    break;
            }
            
            // Face target
            if (State != AIState.Jumping && State != AIState.MysteryVortex)
            {
                NPC.spriteDirection = NPC.direction = (target.Center.X > NPC.Center.X) ? 1 : -1;
            }
        }
        
        private void SelectNextAttack(Player target, float distance)
        {
            attackCounter++;
            int attackChoice = attackCounter % 5;
            
            // Telegraph sound
            SoundEngine.PlaySound(SoundID.Item103 with { Pitch = 0.3f, Volume = 0.7f }, NPC.Center);
            
            switch (attackChoice)
            {
                case 0: // Paradox Gaze - watching eyes
                    State = AIState.ParadoxGaze;
                    Timer = 0;
                    break;
                    
                case 1: // Glyph Cascade - raining glyphs
                    State = AIState.GlyphCascade;
                    Timer = 0;
                    break;
                    
                case 2: // Watching Volley - eye projectiles
                    State = AIState.WatchingVolley;
                    Timer = 0;
                    break;
                    
                case 3: // Mystery Vortex - glyph vortex
                    State = AIState.MysteryVortex;
                    Timer = 0;
                    break;
                    
                case 4: // Enigma Revelation - ultimate attack (more likely when low HP)
                    if (NPC.life < NPC.lifeMax * 0.4f || Main.rand.NextBool(3))
                    {
                        State = AIState.EnigmaRevelation;
                        Timer = 0;
                        SoundEngine.PlaySound(SoundID.Item119, NPC.Center);
                    }
                    else
                    {
                        State = AIState.ParadoxGaze;
                        Timer = 0;
                    }
                    break;
            }
        }
        
        #region Attack 1: Paradox Gaze - Watching Eyes
        private void HandleParadoxGaze(Player target)
        {
            NPC.velocity.X *= 0.9f;
            
            // Spawn watching eyes around the target
            if (Timer < 60f)
            {
                if (Timer % 10 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    // Spawn eye at random position around target
                    float angle = MathHelper.TwoPi * Main.rand.NextFloat();
                    float distance = 100f + Main.rand.NextFloat(80f);
                    Vector2 eyePos = target.Center + angle.ToRotationVector2() * distance;
                    
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), eyePos, Vector2.Zero,
                        ModContent.ProjectileType<ParadoxGazeEye>(), NPC.damage / 3, 0f, Main.myPlayer, target.whoAmI);
                }
                
                // VFX - eyes appearing
                if (Timer % 10 == 0)
                {
                    CustomParticles.EnigmaEyeGaze(target.Center + Main.rand.NextVector2Circular(100f, 100f), 
                        EnigmaPurple, 0.5f, (NPC.Center - target.Center).SafeNormalize(Vector2.UnitY));
                }
            }
            else
            {
                EndAttack(80f);
            }
        }
        #endregion
        
        #region Attack 2: Glyph Cascade - Raining Glyphs
        private void HandleGlyphCascade(Player target)
        {
            NPC.velocity.X *= 0.85f;
            
            if (Timer < 90f)
            {
                // Rain glyphs from above
                if (Timer % 8 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    float xOffset = Main.rand.NextFloat(-200f, 200f);
                    Vector2 spawnPos = new Vector2(target.Center.X + xOffset, target.Center.Y - 400f);
                    Vector2 velocity = new Vector2(Main.rand.NextFloat(-1f, 1f), 6f + Main.rand.NextFloat(2f));
                    
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, velocity,
                        ModContent.ProjectileType<CascadingGlyph>(), NPC.damage / 4, 1f, Main.myPlayer);
                }
                
                // Glyph particle effects
                if (Timer % 5 == 0)
                {
                    CustomParticles.GlyphBurst(NPC.Center + new Vector2(0, -30), EnigmaPurple, 2, 3f);
                }
            }
            else
            {
                EndAttack(70f);
            }
        }
        #endregion
        
        #region Attack 3: Watching Volley - Eye Projectiles
        private void HandleWatchingVolley(Player target)
        {
            NPC.velocity.X *= 0.9f;
            
            // Windup
            if (Timer < 25f)
            {
                float progress = Timer / 25f;
                CustomParticles.GenericFlare(NPC.Center, EnigmaGreen, 0.3f + progress * 0.4f, 15);
                
                // Eyes orbiting during charge
                if (Timer % 8 == 0)
                {
                    float orbitAngle = Timer * 0.2f;
                    Vector2 orbitPos = NPC.Center + orbitAngle.ToRotationVector2() * 40f;
                    CustomParticles.EnigmaEyeGaze(orbitPos, EnigmaGreen, 0.4f);
                }
            }
            // Fire volley
            else if (Timer == 25f)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 toTarget = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                    
                    // Fire spread of eye projectiles
                    for (int i = -2; i <= 2; i++)
                    {
                        float spreadAngle = i * 0.15f;
                        Vector2 velocity = toTarget.RotatedBy(spreadAngle) * 9f;
                        
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity,
                            ModContent.ProjectileType<WatchingEyeProjectile>(), NPC.damage / 3, 2f, Main.myPlayer);
                    }
                }
                
                // Fire VFX
                ThemedParticles.EnigmaImpact(NPC.Center, 1f);
                CustomParticles.EnigmaEyeExplosion(NPC.Center, EnigmaGreen, 5, 5f);
                SoundEngine.PlaySound(SoundID.Item8 with { Pitch = -0.2f }, NPC.Center);
            }
            else if (Timer >= 50f)
            {
                EndAttack(60f);
            }
        }
        #endregion
        
        #region Attack 4: Mystery Vortex - Glyph Vortex
        private void HandleMysteryVortex(Player target)
        {
            NPC.velocity *= 0.95f;
            
            if (Timer < 80f)
            {
                // Create vortex at boss position
                float vortexAngle = Timer * 0.15f;
                float vortexRadius = 60f + (float)Math.Sin(Timer * 0.1f) * 20f;
                
                // Orbiting glyphs
                if (Timer % 4 == 0)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        float angle = vortexAngle + MathHelper.TwoPi * i / 3f;
                        Vector2 glyphPos = NPC.Center + angle.ToRotationVector2() * vortexRadius;
                        float progress = (float)i / 3f;
                        Color glyphColor = Color.Lerp(EnigmaPurple, EnigmaGreen, progress);
                        CustomParticles.Glyph(glyphPos, glyphColor, 0.4f, -1);
                    }
                }
                
                // Pull effect - damage nearby players
                if (Timer % 10 == 0 && Timer > 20f && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    // Spawn damaging vortex zone
                    if (Timer == 30f)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
                            ModContent.ProjectileType<MysteryVortexZone>(), NPC.damage / 5, 0f, Main.myPlayer);
                    }
                }
                
                // VFX
                if (Timer % 6 == 0)
                {
                    CustomParticles.GlyphCircle(NPC.Center, EnigmaPurple, 4, vortexRadius, 0.1f);
                }
            }
            else
            {
                // End with glyph burst
                CustomParticles.GlyphBurst(NPC.Center, EnigmaGreen, 8, 6f);
                EndAttack(100f);
            }
        }
        #endregion
        
        #region Attack 5: Enigma Revelation - Ultimate
        private void HandleEnigmaRevelation(Player target)
        {
            NPC.velocity *= 0.95f;
            
            if (Timer < 60f)
            {
                // Buildup - gathering eyes and glyphs
                float progress = Timer / 60f;
                
                // Inward pulling particles
                for (int i = 0; i < 3; i++)
                {
                    Vector2 offset = Main.rand.NextVector2CircularEdge(120f + (1f - progress) * 80f, 120f + (1f - progress) * 80f);
                    Vector2 pullPos = NPC.Center + offset;
                    Vector2 velocity = (NPC.Center - pullPos).SafeNormalize(Vector2.Zero) * 4f;
                    
                    Color pullColor = Main.rand.NextBool() ? EnigmaPurple : EnigmaGreen;
                    Dust pull = Dust.NewDustDirect(pullPos, 0, 0, DustID.GreenTorch, velocity.X, velocity.Y, 100, pullColor, 1.5f);
                    pull.noGravity = true;
                }
                
                // Growing glyph circle
                if (Timer % 10 == 0)
                {
                    CustomParticles.GlyphCircle(NPC.Center, EnigmaPurple, (int)(4 + progress * 4), 40f + progress * 30f, 0.05f);
                }
                
                // Eyes watching inward
                if (Timer % 15 == 0)
                {
                    CustomParticles.EnigmaEyeFormation(NPC.Center, EnigmaGreen, 4, 60f + progress * 40f);
                }
                
                // Core glow
                CustomParticles.GenericFlare(NPC.Center, EnigmaGreen, 0.3f + progress * 0.6f, 20);
            }
            else if (Timer == 60f)
            {
                // REVELATION EXPLOSION!
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    // Eye projectile barrage
                    for (int i = 0; i < 16; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 16f;
                        float speed = 7f + Main.rand.NextFloat(3f);
                        Vector2 velocity = angle.ToRotationVector2() * speed;
                        
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity,
                            ModContent.ProjectileType<RevelationEyeProjectile>(), NPC.damage / 3, 2f, Main.myPlayer);
                    }
                    
                    // Glyph ring
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 8f;
                        Vector2 velocity = angle.ToRotationVector2() * 5f;
                        
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity,
                            ModContent.ProjectileType<CascadingGlyph>(), NPC.damage / 4, 1f, Main.myPlayer);
                    }
                }
                
                // Massive VFX
                UnifiedVFX.EnigmaVariations.Explosion(NPC.Center, 1.8f);
                CustomParticles.EnigmaEyeExplosion(NPC.Center, EnigmaGreen, 12, 8f);
                CustomParticles.GlyphBurst(NPC.Center, EnigmaPurple, 12, 10f);
                
                // Multiple halo rings
                for (int i = 0; i < 6; i++)
                {
                    Color ringColor = Color.Lerp(EnigmaPurple, EnigmaGreen, (float)i / 6f);
                    CustomParticles.HaloRing(NPC.Center, ringColor, 0.4f + i * 0.15f, 18 + i * 4);
                }
                
                SoundEngine.PlaySound(SoundID.Item119 with { Pitch = 0.2f }, NPC.Center);
                SoundEngine.PlaySound(SoundID.Item103, NPC.Center);
            }
            
            if (Timer >= 90f)
            {
                EndAttack(150f);
            }
        }
        #endregion
        
        private void EndAttack(float cooldown)
        {
            State = AIState.Walking;
            Timer = 0;
            attackCooldown = cooldown;
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
            // Flying creature - hover and bob gently
            NPC.velocity *= 0.95f;
            
            // Gentle floating motion
            float floatSpeed = 0.5f;
            NPC.velocity.Y += (float)Math.Sin(Timer * 0.05f) * floatSpeed * 0.1f;
            
            // Start chasing after brief idle
            if (Timer >= 45)
            {
                State = AIState.Walking; // Walking = flying/chasing
                Timer = 0;
            }
        }
        
        private void WalkingBehavior(Player target, bool grounded)
        {
            // Flying behavior - chase target smoothly
            Vector2 toTarget = target.Center - NPC.Center;
            float distance = toTarget.Length();
            
            // Desired position is near the target but with some offset
            Vector2 targetPos = target.Center + new Vector2(0, -100f); // Hover above player
            Vector2 toTargetPos = targetPos - NPC.Center;
            
            float flySpeed = 5f * sizeMultiplier;
            float acceleration = 0.15f;
            
            // Accelerate toward target
            if (toTargetPos.Length() > 50f)
            {
                Vector2 direction = toTargetPos.SafeNormalize(Vector2.Zero);
                NPC.velocity += direction * acceleration;
                
                // Cap speed
                if (NPC.velocity.Length() > flySpeed)
                    NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * flySpeed;
            }
            else
            {
                // Near target, slow down
                NPC.velocity *= 0.95f;
            }
            
            // Add gentle bobbing
            NPC.velocity.Y += (float)Math.Sin(Timer * 0.08f) * 0.1f;
            
            // Return to idle after a while
            if (Timer > 180)
            {
                State = AIState.Idle;
                Timer = 0;
            }
        }
        
        private void JumpingBehavior(Player target, bool grounded)
        {
            // For flying creature, this is a dash attack
            if (Timer == 1)
            {
                // Dash toward target
                Vector2 toTarget = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                float dashSpeed = 12f * sizeMultiplier;
                
                NPC.velocity = toTarget * dashSpeed;
                
                // Dash particles
                CustomParticles.GenericFlare(NPC.Center, EnigmaPurple * 0.8f, 0.4f * sizeMultiplier, 18);
                SoundEngine.PlaySound(SoundID.Item24 with { Pitch = 0.2f, Volume = 0.5f }, NPC.Center);
            }
            
            // Slow down during dash
            NPC.velocity *= 0.97f;
            
            // End dash
            if (Timer > 45 || NPC.velocity.Length() < 2f)
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
            // Mini-boss - 5% spawn rate in Jungle after Moon Lord
            if (!NPC.downedMoonlord)
                return 0f;
            
            if (spawnInfo.Player.ZoneJungle && !spawnInfo.Player.ZoneDungeon && !spawnInfo.PlayerSafe)
            {
                return 0.05f; // 5% spawn chance - mini-boss tier
            }
            
            return 0f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // Essence drops only after killing the main Enigma boss
            LeadingConditionRule afterBossRule = new LeadingConditionRule(new DownedEnigmaCondition());
            
            // Mini-boss tier drops - matching Eroica mini-bosses
            // Remnant of Mysteries (theme-specific crafting material like ShardOfTriumphsTempo)
            afterBossRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<RemnantOfMysteries>(), 1, 5, 10));
            
            // Enigma Resonance Energy (guaranteed)
            afterBossRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<EnigmaResonantEnergy>(), 1, 8, 15));
            
            // Resonant Core of Enigma (guaranteed)
            afterBossRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<HarmonicCores.ResonantCoreOfEnigma>(), 1, 3, 6));
            
            // Enigma Ore bonus
            afterBossRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<EnigmaResonanceOre>(), 2, 5, 12));
            
            npcLoot.Add(afterBossRule);
        }

        public override void FindFrame(int frameHeight)
        {
            frameCounter++;
            
            int speed = FrameSpeed;
            if (State == AIState.Walking || State == AIState.Idle)
                speed = 5; // Normal animation
            else if (State == AIState.ParadoxGaze || State == AIState.WatchingVolley || 
                     State == AIState.GlyphCascade || State == AIState.MysteryVortex ||
                     State == AIState.EnigmaRevelation)
                speed = 3; // Fast animation during attacks
            
            if (frameCounter >= speed)
            {
                frameCounter = 0;
                currentFrame++;
                if (currentFrame >= TotalFrames)
                    currentFrame = 0;
            }
            
            // 6x6 spritesheet - calculate frame Y position
            // Note: We handle the actual frame rectangle in PreDraw
            NPC.frame.Y = currentFrame * frameHeight;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[Type].Value;
            
            // 6x6 spritesheet - calculate frame from current animation frame
            int frameWidth = texture.Width / FrameColumns;
            int frameHeight = texture.Height / FrameRows;
            
            int frameX = currentFrame % FrameColumns;
            int frameY = currentFrame / FrameColumns;
            
            Rectangle frame = new Rectangle(frameX * frameWidth, frameY * frameHeight, frameWidth, frameHeight);
            
            Vector2 drawPos = NPC.Center - screenPos;
            Vector2 origin = new Vector2(frameWidth / 2f, frameHeight / 2f);
            // FIXED: Sprite is drawn facing left by default, flip when facing right
            SpriteEffects effects = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            
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
            // Enhanced with multi-layer bloom
            EnhancedParticles.BloomFlare(Projectile.Center, EnigmaGreen, 0.4f, 15, 3, 0.7f);
            EnhancedParticles.BloomFlare(Projectile.Center, EnigmaPurple, 0.3f, 12, 2, 0.6f);
            EnhancedThemedParticles.EnigmaBloomBurstEnhanced(Projectile.Center, 0.4f);
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
    
    #region Attack Projectiles
    
    /// <summary>
    /// Attack 1: Paradox Gaze - A watching eye that spawns around the target and damages on proximity
    /// </summary>
    public class ParadoxGazeEye : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/EnigmaEye1";
        
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 180;
            Projectile.penetrate = -1;
            Projectile.alpha = 255;
        }
        
        public override void AI()
        {
            Player targetPlayer = Main.player[(int)Projectile.ai[0]];
            
            // Fade in
            if (Projectile.alpha > 0)
                Projectile.alpha -= 10;
            
            // Look at target player
            if (targetPlayer.active)
            {
                Vector2 toTarget = targetPlayer.Center - Projectile.Center;
                Projectile.rotation = toTarget.ToRotation();
            }
            
            // Ambient eye particles
            if (Main.rand.NextBool(8))
            {
                CustomParticles.EnigmaEyeGaze(Projectile.Center, EnigmaPurple * 0.5f, 0.3f);
            }
            
            // Pulsing glow
            float pulse = (float)Math.Sin(Projectile.timeLeft * 0.1f) * 0.2f + 0.8f;
            Lighting.AddLight(Projectile.Center, EnigmaGreen.ToVector3() * 0.3f * pulse);
            
            // Fade out at end
            if (Projectile.timeLeft < 30)
                Projectile.alpha += 8;
        }
        
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 120);
        }
        
        public override void OnKill(int timeLeft)
        {
            // Enhanced with bloom
            CustomParticles.EnigmaEyeGaze(Projectile.Center, EnigmaGreen, 0.5f);
            EnhancedParticles.BloomFlare(Projectile.Center, EnigmaPurple, 0.4f, 15, 3, 0.7f);
            EnhancedThemedParticles.EnigmaBloomBurstEnhanced(Projectile.Center, 0.35f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnigmaEye" + Main.rand.Next(1, 9)).Value;
            Vector2 pos = Projectile.Center - Main.screenPosition;
            float alpha = 1f - Projectile.alpha / 255f;
            
            // Glow underlay
            Main.spriteBatch.Draw(texture, pos, null, EnigmaPurple * alpha * 0.5f, Projectile.rotation, texture.Size() / 2, 0.6f, SpriteEffects.None, 0f);
            // Main eye
            Main.spriteBatch.Draw(texture, pos, null, Color.White * alpha, Projectile.rotation, texture.Size() / 2, 0.5f, SpriteEffects.None, 0f);
            
            return false;
        }
    }
    
    /// <summary>
    /// Attack 2: Glyph Cascade - Falling arcane glyphs
    /// </summary>
    public class CascadingGlyph : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/Glyphs1";
        
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private int glyphIndex = 1;
        
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 300;
            Projectile.penetrate = 1;
        }
        
        public override void AI()
        {
            if (Projectile.ai[0] == 0)
            {
                glyphIndex = Main.rand.Next(1, 13);
                Projectile.ai[0] = 1;
            }
            
            // Slow rotation
            Projectile.rotation += 0.05f;
            
            // Trail
            if (Main.rand.NextBool(3))
            {
                Color trailColor = Color.Lerp(EnigmaPurple, EnigmaGreen, Main.rand.NextFloat());
                CustomParticles.Glyph(Projectile.Center, trailColor * 0.4f, 0.2f, glyphIndex);
            }
            
            Lighting.AddLight(Projectile.Center, EnigmaPurple.ToVector3() * 0.25f);
        }
        
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 90);
        }
        
        public override void OnKill(int timeLeft)
        {
            // Enhanced with bloom
            CustomParticles.GlyphBurst(Projectile.Center, EnigmaGreen, 4, 3f);
            EnhancedParticles.BloomFlare(Projectile.Center, EnigmaPurple, 0.4f, 15, 3, 0.7f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/Glyphs" + glyphIndex).Value;
            Vector2 pos = Projectile.Center - Main.screenPosition;
            
            // Glow
            Main.spriteBatch.Draw(texture, pos, null, EnigmaPurple * 0.6f, Projectile.rotation, texture.Size() / 2, 0.5f, SpriteEffects.None, 0f);
            // Main glyph
            Main.spriteBatch.Draw(texture, pos, null, Color.White * 0.9f, Projectile.rotation, texture.Size() / 2, 0.4f, SpriteEffects.None, 0f);
            
            return false;
        }
    }
    
    /// <summary>
    /// Attack 3: Watching Volley - Eye projectile with trailing eyes
    /// </summary>
    public class WatchingEyeProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/EnigmaEye1";
        
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 180;
            Projectile.penetrate = 1;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Trail of eyes
            if (Main.rand.NextBool(4))
            {
                CustomParticles.EnigmaEyeGaze(Projectile.Center, EnigmaGreen * 0.5f, 0.25f, Projectile.velocity.SafeNormalize(Vector2.UnitX));
            }
            
            // Glyph accents
            if (Main.rand.NextBool(8))
            {
                CustomParticles.Glyph(Projectile.Center, EnigmaPurple * 0.4f, 0.2f, -1);
            }
            
            Lighting.AddLight(Projectile.Center, EnigmaGreen.ToVector3() * 0.3f);
        }
        
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 120);
        }
        
        public override void OnKill(int timeLeft)
        {
            // Enhanced with bloom
            CustomParticles.EnigmaEyeExplosion(Projectile.Center, EnigmaGreen, 3, 3f);
            EnhancedParticles.BloomFlare(Projectile.Center, EnigmaPurple, 0.5f, 18, 3, 0.8f);
            EnhancedThemedParticles.EnigmaBloomBurstEnhanced(Projectile.Center, 0.5f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnigmaEye" + ((Projectile.timeLeft % 8) + 1)).Value;
            Vector2 pos = Projectile.Center - Main.screenPosition;
            
            Main.spriteBatch.Draw(texture, pos, null, EnigmaPurple * 0.5f, Projectile.rotation, texture.Size() / 2, 0.45f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(texture, pos, null, Color.White, Projectile.rotation, texture.Size() / 2, 0.35f, SpriteEffects.None, 0f);
            
            return false;
        }
    }
    
    /// <summary>
    /// Attack 4: Mystery Vortex - Damaging glyph zone
    /// </summary>
    public class MysteryVortexZone : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        public override void SetDefaults()
        {
            Projectile.width = 120;
            Projectile.height = 120;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 60;
            Projectile.penetrate = -1;
        }
        
        public override void AI()
        {
            // Vortex visuals
            float vortexAngle = Projectile.ai[0];
            Projectile.ai[0] += 0.1f;
            
            // Orbiting glyphs
            for (int i = 0; i < 4; i++)
            {
                float angle = vortexAngle + MathHelper.TwoPi * i / 4f;
                Vector2 glyphPos = Projectile.Center + angle.ToRotationVector2() * 50f;
                float progress = (float)i / 4f;
                Color glyphColor = Color.Lerp(EnigmaPurple, EnigmaGreen, progress);
                CustomParticles.Glyph(glyphPos, glyphColor, 0.35f, -1);
            }
            
            // Pull effect toward center
            foreach (Player player in Main.player)
            {
                if (player.active && !player.dead && Vector2.Distance(player.Center, Projectile.Center) < 100f)
                {
                    Vector2 pull = (Projectile.Center - player.Center).SafeNormalize(Vector2.Zero) * 2f;
                    player.velocity += pull * 0.1f;
                }
            }
            
            Lighting.AddLight(Projectile.Center, EnigmaPurple.ToVector3() * 0.5f);
        }
        
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 60);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D glow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 pos = Projectile.Center - Main.screenPosition;
            
            // Swirling vortex effect
            float pulse = (float)Math.Sin(Projectile.ai[0] * 2f) * 0.2f + 0.8f;
            Main.spriteBatch.Draw(glow, pos, null, EnigmaPurple * 0.4f * pulse, Projectile.ai[0], glow.Size() / 2, 1.5f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(glow, pos, null, EnigmaGreen * 0.3f * pulse, -Projectile.ai[0] * 0.5f, glow.Size() / 2, 1.2f, SpriteEffects.None, 0f);
            
            return false;
        }
    }
    
    /// <summary>
    /// Attack 5: Enigma Revelation - Ultimate eye projectile
    /// </summary>
    public class RevelationEyeProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/EnigmaEye1";
        
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 240;
            Projectile.penetrate = 2;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Heavy trail
            for (int i = 0; i < 2; i++)
            {
                Color trailColor = Color.Lerp(EnigmaPurple, EnigmaGreen, Main.rand.NextFloat());
                Vector2 offset = Main.rand.NextVector2Circular(5f, 5f);
                CustomParticles.GenericGlow(Projectile.Center + offset, trailColor * 0.5f, 0.25f, 15);
            }
            
            // Eye trail
            if (Main.rand.NextBool(5))
            {
                CustomParticles.EnigmaEyeGaze(Projectile.Center, EnigmaGreen * 0.6f, 0.3f, Projectile.velocity.SafeNormalize(Vector2.UnitX));
            }
            
            // Glyph sparkles
            if (Main.rand.NextBool(6))
            {
                CustomParticles.Glyph(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f), EnigmaPurple * 0.5f, 0.2f, -1);
            }
            
            Lighting.AddLight(Projectile.Center, EnigmaGreen.ToVector3() * 0.4f);
        }
        
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 180);
        }
        
        public override void OnKill(int timeLeft)
        {
            // Big eye explosion with enhanced bloom
            CustomParticles.EnigmaEyeExplosion(Projectile.Center, EnigmaGreen, 5, 5f);
            CustomParticles.GlyphBurst(Projectile.Center, EnigmaPurple, 4, 4f);
            EnhancedParticles.BloomFlare(Projectile.Center, Color.White, 0.6f, 20, 4, 1f);
            UnifiedVFXBloom.EnigmaVariations.ImpactEnhanced(Projectile.Center, 1f);
            
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Color burstColor = Color.Lerp(EnigmaPurple, EnigmaGreen, (float)i / 8f);
                EnhancedParticles.BloomFlare(Projectile.Center + angle.ToRotationVector2() * 20f, burstColor, 0.35f, 15, 3, 0.75f);
            }
            
            // Music notes cascade
            EnhancedThemedParticles.EnigmaMusicNotesEnhanced(Projectile.Center, 5, 40f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            int eyeIndex = ((Projectile.timeLeft / 5) % 8) + 1;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnigmaEye" + eyeIndex).Value;
            Vector2 pos = Projectile.Center - Main.screenPosition;
            
            // Glow layers
            Main.spriteBatch.Draw(texture, pos, null, EnigmaPurple * 0.5f, Projectile.rotation, texture.Size() / 2, 0.55f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(texture, pos, null, EnigmaGreen * 0.3f, Projectile.rotation, texture.Size() / 2, 0.6f, SpriteEffects.None, 0f);
            // Main eye
            Main.spriteBatch.Draw(texture, pos, null, Color.White, Projectile.rotation, texture.Size() / 2, 0.4f, SpriteEffects.None, 0f);
            
            return false;
        }
    }
    
    #endregion
}
