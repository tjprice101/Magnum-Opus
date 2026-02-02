using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.Fate.ResonanceEnergies;
using MagnumOpus.Content.Fate.ResonantOres;
using MagnumOpus.Content.Fate.HarmonicCores;
using MagnumOpus.Content.Materials.EnemyDrops;

namespace MagnumOpus.Content.Fate.Enemies
{
    /// <summary>
    /// Herald of Fate - A terrifying mini-boss that spawns in Corruption/Crimson after Moon Lord.
    /// MORE POWERFUL than Swan Lake tier - this is endgame Fate content.
    /// 
    /// 7 COSMIC CATACLYSMIC ATTACKS:
    /// 1. Cosmic Rend - Reality-tearing slashes that leave lingering damage zones
    /// 2. Stellar Barrage - Rapid-fire star projectiles in spiraling patterns
    /// 3. Destiny's Chain - Connecting beams between glyphs that track the player
    /// 4. Void Collapse - Gravitational pull that damages and slows
    /// 5. Constellation Judgment - Massive star formation attack with converging beams
    /// 6. Reality Fracture - Screen-distorting teleport attacks
    /// 7. Fate Sealed - Ultimate attack: glyph circle + cosmic explosion
    /// </summary>
    public class HeraldOfFate : ModNPC
    {
        // Fate theme colors - cosmic void aesthetic
        private static readonly Color FateBlack = new Color(8, 3, 20);
        private static readonly Color FateDarkPink = new Color(180, 50, 100);
        private static readonly Color FateBrightRed = new Color(255, 60, 80);
        private static readonly Color FatePurple = new Color(120, 30, 140);
        private static readonly Color FateWhite = new Color(255, 255, 255);
        
        // Size variation
        private float sizeMultiplier = 1f;
        private bool hasSetSize = false;
        
        // Animation - single frame sprite (no spritesheet)
        private int frameCounter = 0;
        private int currentFrame = 0;
        private const int TotalFrames = 1;
        private const int FrameSpeed = 5;
        
        // Visual effects
        private float cosmicGlow = 0f;
        private float auraPulse = 0f;
        private float glyphRotation = 0f;
        private List<Vector2> orbitingGlyphPositions = new List<Vector2>();
        
        // Attack tracking
        private int attackCounter = 0;
        private float attackCooldown = 0f;
        private int consecutiveAttacks = 0;
        
        // Enrage tracking
        private bool isEnraged = false;
        private float enrageTimer = 0f;
        
        // AI states
        private enum AIState
        {
            Hovering,
            CosmicRend,           // Attack 1 - Reality slashes
            StellarBarrage,       // Attack 2 - Star projectile spiral
            DestinyChain,         // Attack 3 - Glyph beam chains
            VoidCollapse,         // Attack 4 - Gravitational pull
            ConstellationJudgment,// Attack 5 - Massive star formation
            RealityFracture,      // Attack 6 - Teleport dash attacks
            FateSealed            // Attack 7 - Ultimate cosmic explosion
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
        
        private float SubTimer
        {
            get => NPC.ai[2];
            set => NPC.ai[2] = value;
        }
        
        private int AttackPhase
        {
            get => (int)NPC.ai[3];
            set => NPC.ai[3] = value;
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = TotalFrames;
            
            NPCID.Sets.TrailCacheLength[Type] = 8;
            NPCID.Sets.TrailingMode[Type] = 1;
            
            // Debuff immunities - cosmic entity
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Frostburn] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.CursedInferno] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.ShadowFlame] = true;
            
            // Boss-like immunity
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            
            // Register for minimap
            MinibossMinimapSystem.RegisterFateMiniboss(Type);
        }

        public override void SetDefaults()
        {
            // FATE MINI-BOSS STATS - STRONGER than Swan Lake (950k boss, 170 damage)
            // This is endgame content - terrifying power
            // Hitbox = 292 × 164 × 0.8 = 233 × 131 (single frame sprite)
            NPC.width = 233;
            NPC.height = 131;
            NPC.damage = 220; // Higher than Swan Lake boss (170)
            NPC.defense = 85; // Solid defense
            NPC.lifeMax = 85000; // Mini-boss HP - higher than Enigma's 45k
            NPC.HitSound = SoundID.NPCHit54; // Ethereal hit
            NPC.DeathSound = SoundID.NPCDeath52; // Cosmic death
            NPC.knockBackResist = 0f; // Immune to knockback
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = Item.buyPrice(gold: 75); // High value
            NPC.aiStyle = -1; // Custom AI
            NPC.lavaImmune = true;
            NPC.npcSlots = 8f; // Major threat
            NPC.boss = false; // Mini-boss, not full boss
            NPC.Opacity = 0.95f;
            
            // Boss-like behavior
            NPC.dontTakeDamage = false;
        }

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            // Scale for multiplayer
            NPC.lifeMax = (int)(NPC.lifeMax * balance * bossAdjustment);
            NPC.damage = (int)(NPC.damage * balance);
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheCorruption,
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheCrimson,
                new FlavorTextBestiaryInfoElement("Herald of Fate - " +
                    "An emissary from beyond the veil of reality, this cosmic horror announces the coming of inevitable destiny. " +
                    "Where it walks, the fabric of existence trembles. Those who witness its arrival rarely live to tell of it. " +
                    "It is said that this creature appears only to those whose fate hangs by the thinnest of threads.")
            });
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            // Spawn in Corruption/Crimson after Moon Lord
            if (!NPC.downedMoonlord) return 0f;
            if (!spawnInfo.Player.ZoneCorrupt && !spawnInfo.Player.ZoneCrimson) return 0f;
            if (spawnInfo.Player.ZoneTowerNebula || spawnInfo.Player.ZoneTowerSolar || 
                spawnInfo.Player.ZoneTowerStardust || spawnInfo.Player.ZoneTowerVortex) return 0f;
            
            // Only one at a time
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && npc.type == Type)
                    return 0f;
            }
            
            // 3% chance - rare but findable
            return 0.03f;
        }

        public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
        {
            // Apply size variation
            if (!hasSetSize)
            {
                sizeMultiplier = 0.9f + Main.rand.NextFloat() * 0.2f; // 0.9 to 1.1
                NPC.scale = sizeMultiplier;
                
                NPC.lifeMax = (int)(NPC.lifeMax * sizeMultiplier);
                NPC.life = NPC.lifeMax;
                NPC.damage = (int)(NPC.damage * sizeMultiplier);
                
                NPC.width = (int)(100 * sizeMultiplier);
                NPC.height = (int)(100 * sizeMultiplier);
                
                hasSetSize = true;
            }
            
            // Dramatic spawn announcement
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Main.NewText("The Herald of Fate has arrived...", FateDarkPink);
            }
            
            // Spawn VFX
            SpawnCosmicBurst(NPC.Center, 2f);
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.5f, Volume = 1.5f }, NPC.Center);
        }

        public override void AI()
        {
            // Update timers
            Timer++;
            auraPulse += 0.04f;
            glyphRotation += 0.02f;
            cosmicGlow = 0.6f + (float)Math.Sin(auraPulse * 2f) * 0.25f;
            
            if (attackCooldown > 0f)
                attackCooldown--;
            
            // Enrage check - below 30% HP
            if (!isEnraged && NPC.life < NPC.lifeMax * 0.3f)
            {
                isEnraged = true;
                Main.NewText("The Herald's fury is unleashed!", FateBrightRed);
                SpawnCosmicBurst(NPC.Center, 3f);
                SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
            }
            
            if (isEnraged)
            {
                enrageTimer += 0.02f;
                // Faster attacks when enraged
                if (attackCooldown > 0)
                    attackCooldown -= 0.5f;
            }
            
            // Dynamic lighting
            float lightIntensity = cosmicGlow * 0.6f;
            Vector3 lightColor = Vector3.Lerp(FatePurple.ToVector3(), FateBrightRed.ToVector3(), 
                (float)Math.Sin(auraPulse) * 0.5f + 0.5f);
            Lighting.AddLight(NPC.Center, lightColor * lightIntensity);
            
            // Ambient particles
            SpawnAmbientParticles();
            UpdateOrbitingGlyphs();
            
            NPC.TargetClosest(true);
            Player target = Main.player[NPC.target];
            
            // Despawn if no valid target
            if (!target.active || target.dead)
            {
                NPC.velocity.Y -= 0.3f;
                NPC.velocity.X *= 0.95f;
                NPC.alpha += 3;
                if (NPC.alpha >= 255 || NPC.timeLeft > 60)
                    NPC.timeLeft = 60;
                return;
            }
            
            float distanceToTarget = Vector2.Distance(NPC.Center, target.Center);
            
            // Select attack when ready
            if (attackCooldown <= 0f && distanceToTarget < 800f && State == AIState.Hovering)
            {
                SelectNextAttack(target, distanceToTarget);
            }
            
            // Execute current state
            switch (State)
            {
                case AIState.Hovering:
                    AI_Hovering(target);
                    break;
                case AIState.CosmicRend:
                    AI_CosmicRend(target);
                    break;
                case AIState.StellarBarrage:
                    AI_StellarBarrage(target);
                    break;
                case AIState.DestinyChain:
                    AI_DestinyChain(target);
                    break;
                case AIState.VoidCollapse:
                    AI_VoidCollapse(target);
                    break;
                case AIState.ConstellationJudgment:
                    AI_ConstellationJudgment(target);
                    break;
                case AIState.RealityFracture:
                    AI_RealityFracture(target);
                    break;
                case AIState.FateSealed:
                    AI_FateSealed(target);
                    break;
            }
            
            // Face target
            NPC.spriteDirection = NPC.Center.X < target.Center.X ? 1 : -1;
        }

        private void SelectNextAttack(Player target, float distance)
        {
            consecutiveAttacks++;
            attackCounter++;
            
            // Weight attacks based on situation
            List<AIState> availableAttacks = new List<AIState>
            {
                AIState.CosmicRend,
                AIState.StellarBarrage,
                AIState.DestinyChain,
                AIState.VoidCollapse
            };
            
            // Add powerful attacks after enough basic attacks
            if (consecutiveAttacks >= 3)
            {
                availableAttacks.Add(AIState.ConstellationJudgment);
                availableAttacks.Add(AIState.RealityFracture);
            }
            
            // Ultimate attack when enraged or after many attacks
            if ((isEnraged && consecutiveAttacks >= 4) || consecutiveAttacks >= 6)
            {
                availableAttacks.Add(AIState.FateSealed);
                consecutiveAttacks = 0; // Reset after ultimate becomes available
            }
            
            // Close range favors certain attacks
            if (distance < 300f)
            {
                availableAttacks.Add(AIState.CosmicRend);
                availableAttacks.Add(AIState.VoidCollapse);
            }
            
            State = availableAttacks[Main.rand.Next(availableAttacks.Count)];
            Timer = 0;
            SubTimer = 0;
            AttackPhase = 0;
            
            // Base cooldown - reduced when enraged
            attackCooldown = isEnraged ? 45f : 75f;
        }

        #region AI States

        private void AI_Hovering(Player target)
        {
            // Float toward target at comfortable distance
            float idealDistance = 350f;
            Vector2 toTarget = target.Center - NPC.Center;
            float currentDist = toTarget.Length();
            
            Vector2 idealPos = target.Center - toTarget.SafeNormalize(Vector2.Zero) * idealDistance;
            idealPos.Y -= 100f; // Hover above
            
            Vector2 toIdeal = idealPos - NPC.Center;
            float speed = 8f;
            
            if (toIdeal.Length() > 20f)
            {
                NPC.velocity = Vector2.Lerp(NPC.velocity, toIdeal.SafeNormalize(Vector2.Zero) * speed, 0.06f);
            }
            else
            {
                NPC.velocity *= 0.9f;
            }
            
            // Gentle bobbing
            NPC.velocity.Y += (float)Math.Sin(Timer * 0.05f) * 0.1f;
        }

        private void AI_CosmicRend(Player target)
        {
            // ATTACK 1: Reality-tearing slashes
            int windupTime = isEnraged ? 30 : 45;
            int slashCount = isEnraged ? 5 : 3;
            int slashInterval = 12;
            
            if (AttackPhase == 0) // Windup
            {
                NPC.velocity *= 0.92f;
                
                // Converging warning particles
                float progress = Timer / windupTime;
                if (Timer % 3 == 0)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 6f + Timer * 0.1f;
                        float radius = 120f * (1f - progress * 0.5f);
                        Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                        CustomParticles.GenericFlare(pos, FateDarkPink, 0.3f + progress * 0.3f, 12);
                    }
                }
                
                if (Timer >= windupTime)
                {
                    Timer = 0;
                    AttackPhase = 1;
                    SubTimer = 0;
                }
            }
            else if (AttackPhase == 1) // Slashing
            {
                if (SubTimer < slashCount && Timer % slashInterval == 0)
                {
                    // Perform slash
                    Vector2 slashDir = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                    slashDir = slashDir.RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f));
                    
                    // Spawn slash projectile
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int damage = NPC.damage / 2;
                        float speed = 18f;
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, slashDir * speed,
                            ModContent.ProjectileType<CosmicRendSlash>(), damage, 3f, Main.myPlayer);
                    }
                    
                    // Slash VFX
                    SpawnSlashVFX(NPC.Center, slashDir);
                    SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.3f }, NPC.Center);
                    
                    SubTimer++;
                }
                
                // Slight movement during slashes
                Vector2 toTarget = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                NPC.velocity = Vector2.Lerp(NPC.velocity, toTarget * 6f, 0.1f);
                
                if (SubTimer >= slashCount && Timer > slashInterval * slashCount + 20)
                {
                    ReturnToHover();
                }
            }
        }

        private void AI_StellarBarrage(Player target)
        {
            // ATTACK 2: Spiral star projectiles
            int duration = isEnraged ? 90 : 120;
            int fireRate = isEnraged ? 3 : 5;
            
            // Orbit around target while firing
            float orbitSpeed = 0.03f;
            float orbitRadius = 300f;
            Vector2 orbitPos = target.Center + (Timer * orbitSpeed).ToRotationVector2() * orbitRadius;
            orbitPos.Y -= 50f;
            
            Vector2 toOrbit = orbitPos - NPC.Center;
            NPC.velocity = Vector2.Lerp(NPC.velocity, toOrbit * 0.08f, 0.1f);
            
            // Fire stars in spiral pattern
            if (Timer % fireRate == 0 && Timer < duration - 30)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int damage = NPC.damage / 3;
                    float baseAngle = Timer * 0.15f;
                    int projectileCount = isEnraged ? 3 : 2;
                    
                    for (int i = 0; i < projectileCount; i++)
                    {
                        float angle = baseAngle + MathHelper.TwoPi * i / projectileCount;
                        Vector2 vel = angle.ToRotationVector2() * 12f;
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, vel,
                            ModContent.ProjectileType<StellarBolt>(), damage, 2f, Main.myPlayer);
                    }
                }
                
                // Fire sound
                if (Timer % (fireRate * 3) == 0)
                    SoundEngine.PlaySound(SoundID.Item125 with { Pitch = 0.5f, Volume = 0.6f }, NPC.Center);
            }
            
            if (Timer >= duration)
            {
                ReturnToHover();
            }
        }

        private void AI_DestinyChain(Player target)
        {
            // ATTACK 3: Glyph beam chains
            int setupTime = 40;
            int chainDuration = isEnraged ? 120 : 90;
            int glyphCount = isEnraged ? 6 : 4;
            
            if (AttackPhase == 0) // Setup glyphs
            {
                NPC.velocity *= 0.9f;
                
                if (Timer == 1)
                {
                    // Spawn glyphs around player
                    orbitingGlyphPositions.Clear();
                    for (int i = 0; i < glyphCount; i++)
                    {
                        float angle = MathHelper.TwoPi * i / glyphCount;
                        Vector2 glyphPos = target.Center + angle.ToRotationVector2() * 200f;
                        orbitingGlyphPositions.Add(glyphPos);
                        
                        // Spawn VFX
                        CustomParticles.GlyphBurst(glyphPos, FatePurple, 4, 3f);
                        CustomParticles.GenericFlare(glyphPos, FateWhite, 0.6f, 20);
                    }
                    
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.3f }, NPC.Center);
                }
                
                if (Timer >= setupTime)
                {
                    Timer = 0;
                    AttackPhase = 1;
                }
            }
            else if (AttackPhase == 1) // Active chains
            {
                // Glyphs orbit and fire beams between each other
                float orbitSpeed = 0.02f + (isEnraged ? 0.01f : 0f);
                
                for (int i = 0; i < orbitingGlyphPositions.Count; i++)
                {
                    // Orbit around player
                    float angle = MathHelper.TwoPi * i / orbitingGlyphPositions.Count + Timer * orbitSpeed;
                    Vector2 newPos = target.Center + angle.ToRotationVector2() * (180f + (float)Math.Sin(Timer * 0.1f) * 30f);
                    orbitingGlyphPositions[i] = newPos;
                    
                    // Glyph particle
                    if (Timer % 5 == 0)
                    {
                        CustomParticles.Glyph(newPos, FateDarkPink, 0.4f, -1);
                    }
                    
                    // Draw beam to next glyph
                    int nextIndex = (i + 1) % orbitingGlyphPositions.Count;
                    Vector2 nextPos = orbitingGlyphPositions[nextIndex];
                    
                    // Beam damage check
                    if (Timer % 10 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        // Check if player intersects beam
                        Vector2 beamDir = (nextPos - newPos).SafeNormalize(Vector2.Zero);
                        float beamLength = Vector2.Distance(newPos, nextPos);
                        
                        for (float t = 0; t < beamLength; t += 20f)
                        {
                            Vector2 checkPos = newPos + beamDir * t;
                            if (Vector2.Distance(checkPos, target.Center) < 30f)
                            {
                                target.Hurt(Terraria.DataStructures.PlayerDeathReason.ByNPC(NPC.whoAmI), 
                                    NPC.damage / 4, 0);
                                break;
                            }
                        }
                    }
                    
                    // Beam VFX
                    if (Timer % 3 == 0)
                    {
                        Vector2 beamMid = (newPos + nextPos) / 2f;
                        CustomParticles.GenericFlare(beamMid, FateBrightRed * 0.6f, 0.2f, 5);
                    }
                }
                
                // Glow particles on glyphs
                foreach (Vector2 glyphPos in orbitingGlyphPositions)
                {
                    Lighting.AddLight(glyphPos, FatePurple.ToVector3() * 0.4f);
                }
                
                if (Timer >= chainDuration)
                {
                    // Collapse glyphs with explosion
                    foreach (Vector2 glyphPos in orbitingGlyphPositions)
                    {
                        CustomParticles.GenericFlare(glyphPos, FateBrightRed, 0.8f, 20);
                        CustomParticles.HaloRing(glyphPos, FateDarkPink, 0.4f, 15);
                    }
                    orbitingGlyphPositions.Clear();
                    ReturnToHover();
                }
            }
        }

        private void AI_VoidCollapse(Player target)
        {
            // ATTACK 4: Gravitational pull
            int chargeDuration = 30;
            int pullDuration = isEnraged ? 120 : 90;
            float pullStrength = isEnraged ? 0.8f : 0.5f;
            float pullRadius = 400f;
            
            if (AttackPhase == 0) // Charge
            {
                NPC.velocity *= 0.85f;
                
                float progress = Timer / (float)chargeDuration;
                
                // Converging void particles
                if (Timer % 2 == 0)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 8f + Timer * 0.1f;
                        float radius = pullRadius * (1f - progress * 0.5f);
                        Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                        Vector2 vel = (NPC.Center - pos).SafeNormalize(Vector2.Zero) * (3f + progress * 5f);
                        
                        var particle = new GenericGlowParticle(pos, vel, FatePurple * 0.8f, 0.3f, 15, true);
                        MagnumParticleHandler.SpawnParticle(particle);
                    }
                }
                
                if (Timer >= chargeDuration)
                {
                    Timer = 0;
                    AttackPhase = 1;
                    SoundEngine.PlaySound(SoundID.Item104 with { Pitch = -0.7f }, NPC.Center);
                }
            }
            else if (AttackPhase == 1) // Pull
            {
                NPC.velocity *= 0.9f;
                
                // Pull player toward center
                float distToPlayer = Vector2.Distance(NPC.Center, target.Center);
                if (distToPlayer < pullRadius && distToPlayer > 50f)
                {
                    Vector2 pullDir = (NPC.Center - target.Center).SafeNormalize(Vector2.Zero);
                    float pullForce = pullStrength * (1f - distToPlayer / pullRadius);
                    target.velocity += pullDir * pullForce;
                    
                    // Slow player
                    target.velocity *= 0.97f;
                }
                
                // Damage if too close
                if (distToPlayer < 80f && Timer % 15 == 0)
                {
                    target.Hurt(Terraria.DataStructures.PlayerDeathReason.ByNPC(NPC.whoAmI), 
                        NPC.damage / 3, 0);
                }
                
                // Vortex VFX
                if (Timer % 2 == 0)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        float angle = Timer * 0.15f + MathHelper.PiOver2 * i;
                        float radius = 40f + (float)Math.Sin(Timer * 0.1f + i) * 20f;
                        Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                        CustomParticles.GenericFlare(pos, FateBlack, 0.4f, 8);
                    }
                }
                
                // Central void glow
                CustomParticles.GenericFlare(NPC.Center, FateDarkPink, 0.6f + (float)Math.Sin(Timer * 0.2f) * 0.2f, 5);
                
                if (Timer >= pullDuration)
                {
                    // Collapse explosion
                    SpawnCosmicBurst(NPC.Center, 1.5f);
                    SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.5f }, NPC.Center);
                    ReturnToHover();
                }
            }
        }

        private void AI_ConstellationJudgment(Player target)
        {
            // ATTACK 5: Massive star formation with converging beams
            int setupTime = 60;
            int fireTime = 30;
            int starCount = isEnraged ? 8 : 6;
            float starRadius = 350f;
            
            if (AttackPhase == 0) // Setup constellation
            {
                NPC.velocity *= 0.9f;
                
                float progress = Timer / (float)setupTime;
                
                // Spawn stars in formation
                if (Timer % (setupTime / starCount) == 0 && Timer < setupTime)
                {
                    int starIndex = (int)(Timer / (setupTime / starCount));
                    float angle = MathHelper.TwoPi * starIndex / starCount;
                    Vector2 starPos = target.Center + angle.ToRotationVector2() * starRadius;
                    
                    // Star spawn VFX
                    CustomParticles.GenericFlare(starPos, FateWhite, 1f, 30);
                    CustomParticles.HaloRing(starPos, FateDarkPink, 0.5f, 20);
                    SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.3f + starIndex * 0.1f }, starPos);
                }
                
                // Warning lines to target
                if (Timer > setupTime * 0.7f && Timer % 3 == 0)
                {
                    for (int i = 0; i < starCount; i++)
                    {
                        float angle = MathHelper.TwoPi * i / starCount;
                        Vector2 starPos = target.Center + angle.ToRotationVector2() * starRadius;
                        Vector2 toTarget = (target.Center - starPos).SafeNormalize(Vector2.Zero);
                        
                        for (float t = 0; t < starRadius; t += 30f)
                        {
                            Vector2 linePos = starPos + toTarget * t;
                            CustomParticles.GenericFlare(linePos, FateBrightRed * 0.4f, 0.15f, 3);
                        }
                    }
                }
                
                if (Timer >= setupTime)
                {
                    Timer = 0;
                    AttackPhase = 1;
                }
            }
            else if (AttackPhase == 1) // Fire beams
            {
                if (Timer == 1)
                {
                    // Fire all beams at once toward target
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int damage = NPC.damage / 2;
                        
                        for (int i = 0; i < starCount; i++)
                        {
                            float angle = MathHelper.TwoPi * i / starCount;
                            Vector2 starPos = target.Center + angle.ToRotationVector2() * starRadius;
                            Vector2 vel = (target.Center - starPos).SafeNormalize(Vector2.Zero) * 20f;
                            
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), starPos, vel,
                                ModContent.ProjectileType<ConstellationBeam>(), damage, 4f, Main.myPlayer);
                        }
                    }
                    
                    // Massive VFX
                    SpawnCosmicBurst(target.Center, 2f);
                    MagnumScreenEffects.AddScreenShake(12f);
                    SoundEngine.PlaySound(SoundID.Item122 with { Volume = 1.2f }, target.Center);
                }
                
                if (Timer >= fireTime)
                {
                    ReturnToHover();
                }
            }
        }

        private void AI_RealityFracture(Player target)
        {
            // ATTACK 6: Teleport dash attacks
            int dashCount = isEnraged ? 5 : 3;
            int telegraphTime = 20;
            int dashTime = 10;
            int pauseTime = 15;
            int cycleTime = telegraphTime + dashTime + pauseTime;
            
            int currentDash = (int)(Timer / cycleTime);
            int cycleProgress = (int)(Timer % cycleTime);
            
            if (currentDash >= dashCount)
            {
                ReturnToHover();
                return;
            }
            
            if (cycleProgress < telegraphTime) // Telegraph
            {
                // Teleport to position around target
                if (cycleProgress == 0)
                {
                    float angle = MathHelper.TwoPi * currentDash / dashCount + Main.rand.NextFloat(-0.3f, 0.3f);
                    Vector2 teleportPos = target.Center + angle.ToRotationVector2() * 250f;
                    
                    // Departure VFX
                    CustomParticles.GenericFlare(NPC.Center, FatePurple, 0.8f, 15);
                    CustomParticles.HaloRing(NPC.Center, FateDarkPink, 0.5f, 12);
                    
                    // Teleport
                    NPC.Center = teleportPos;
                    NPC.velocity = Vector2.Zero;
                    
                    // Arrival VFX
                    CustomParticles.GenericFlare(NPC.Center, FateWhite, 1f, 20);
                    CustomParticles.GlyphBurst(NPC.Center, FatePurple, 6, 4f);
                    SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
                }
                
                // Warning line toward target
                if (cycleProgress % 2 == 0)
                {
                    Vector2 toTarget = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                    float dist = Vector2.Distance(NPC.Center, target.Center);
                    for (float t = 0; t < dist; t += 25f)
                    {
                        Vector2 linePos = NPC.Center + toTarget * t;
                        CustomParticles.GenericFlare(linePos, FateBrightRed * 0.5f, 0.2f, 4);
                    }
                }
                
                NPC.velocity *= 0.8f;
            }
            else if (cycleProgress < telegraphTime + dashTime) // Dash
            {
                if (cycleProgress == telegraphTime)
                {
                    // Start dash
                    Vector2 dashDir = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                    NPC.velocity = dashDir * 35f;
                    SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.5f }, NPC.Center);
                }
                
                // Trail VFX
                CustomParticles.GenericFlare(NPC.Center, FateDarkPink, 0.5f, 10);
                
                // Spawn damaging trail
                if (Main.netMode != NetmodeID.MultiplayerClient && cycleProgress % 3 == 0)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
                        ModContent.ProjectileType<RealityFractureTrail>(), NPC.damage / 4, 0f, Main.myPlayer);
                }
            }
            else // Pause
            {
                NPC.velocity *= 0.85f;
            }
        }

        private void AI_FateSealed(Player target)
        {
            // ATTACK 7: Ultimate - Glyph circle + cosmic explosion
            int chargeTime = 90;
            int explosionTime = 30;
            int glyphCount = 12;
            float glyphRadius = 300f;
            
            if (AttackPhase == 0) // Charge
            {
                NPC.velocity *= 0.9f;
                
                float progress = Timer / (float)chargeTime;
                
                // Growing glyph circle around player
                if (Timer % 5 == 0)
                {
                    for (int i = 0; i < glyphCount; i++)
                    {
                        float angle = MathHelper.TwoPi * i / glyphCount + Timer * 0.02f;
                        Vector2 glyphPos = target.Center + angle.ToRotationVector2() * (glyphRadius * progress);
                        CustomParticles.Glyph(glyphPos, Color.Lerp(FatePurple, FateBrightRed, progress), 0.4f + progress * 0.3f, -1);
                    }
                }
                
                // Converging energy
                if (Timer % 3 == 0)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        float radius = 400f * (1f - progress * 0.7f);
                        Vector2 pos = target.Center + angle.ToRotationVector2() * radius;
                        Vector2 vel = (target.Center - pos).SafeNormalize(Vector2.Zero) * (5f + progress * 10f);
                        
                        Color particleColor = Color.Lerp(FateDarkPink, FateWhite, progress);
                        var particle = new GenericGlowParticle(pos, vel, particleColor, 0.35f, 18, true);
                        MagnumParticleHandler.SpawnParticle(particle);
                    }
                }
                
                // Screen effects
                if (progress > 0.5f)
                {
                    MagnumScreenEffects.AddScreenShake(progress * 4f);
                }
                
                // Sound buildup
                if (Timer == 1)
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.8f }, NPC.Center);
                if (Timer == chargeTime / 2)
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.4f }, NPC.Center);
                
                if (Timer >= chargeTime)
                {
                    Timer = 0;
                    AttackPhase = 1;
                }
            }
            else if (AttackPhase == 1) // Explosion
            {
                if (Timer == 1)
                {
                    // MASSIVE explosion
                    SpawnCosmicBurst(target.Center, 4f);
                    
                    // Damage in area
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        float dist = Vector2.Distance(NPC.Center, target.Center);
                        if (dist < glyphRadius + 50f)
                        {
                            int damage = (int)(NPC.damage * 0.8f);
                            target.Hurt(Terraria.DataStructures.PlayerDeathReason.ByNPC(NPC.whoAmI), damage, 0);
                        }
                        
                        // Spawn lingering damage projectiles
                        for (int i = 0; i < 16; i++)
                        {
                            float angle = MathHelper.TwoPi * i / 16f;
                            Vector2 vel = angle.ToRotationVector2() * 8f;
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), target.Center, vel,
                                ModContent.ProjectileType<FateSealedShard>(), NPC.damage / 3, 2f, Main.myPlayer);
                        }
                    }
                    
                    MagnumScreenEffects.AddScreenShake(20f);
                    SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.5f, Volume = 1.5f }, target.Center);
                    SoundEngine.PlaySound(SoundID.Roar with { Volume = 0.8f }, target.Center);
                }
                
                // Aftershock particles
                if (Timer < 20 && Timer % 3 == 0)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 12f;
                        Vector2 pos = target.Center + angle.ToRotationVector2() * (Timer * 15f);
                        CustomParticles.GenericFlare(pos, FateBrightRed, 0.5f, 10);
                    }
                }
                
                if (Timer >= explosionTime)
                {
                    ReturnToHover();
                    attackCooldown = isEnraged ? 90f : 150f; // Longer cooldown after ultimate
                }
            }
        }

        #endregion

        #region Helper Methods

        private void ReturnToHover()
        {
            State = AIState.Hovering;
            Timer = 0;
            SubTimer = 0;
            AttackPhase = 0;
        }

        private void SpawnAmbientParticles()
        {
            if (Main.rand.NextBool(4))
            {
                Vector2 offset = Main.rand.NextVector2Circular(NPC.width * 0.5f, NPC.height * 0.5f);
                Color particleColor = Main.rand.NextBool() ? FateDarkPink : FatePurple;
                
                var particle = new GenericGlowParticle(NPC.Center + offset, 
                    Main.rand.NextVector2Circular(1f, 1f), particleColor * 0.6f, 0.25f, 20, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }
            
            // Occasional glyph
            if (Main.rand.NextBool(20))
            {
                Vector2 glyphPos = NPC.Center + Main.rand.NextVector2Circular(60f, 60f);
                CustomParticles.Glyph(glyphPos, FatePurple * 0.7f, 0.3f, -1);
            }
            
            // Star sparkles
            if (Main.rand.NextBool(8))
            {
                Vector2 sparklePos = NPC.Center + Main.rand.NextVector2Circular(50f, 50f);
                CustomParticles.GenericFlare(sparklePos, FateWhite * 0.5f, 0.15f, 10);
            }
        }

        private void UpdateOrbitingGlyphs()
        {
            // Visual orbiting glyphs around the Herald
            if (Main.rand.NextBool(15))
            {
                float angle = glyphRotation + Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = 60f + Main.rand.NextFloat(20f);
                Vector2 glyphPos = NPC.Center + angle.ToRotationVector2() * radius;
                CustomParticles.Glyph(glyphPos, FateDarkPink * 0.8f, 0.25f, -1);
            }
        }

        private void SpawnCosmicBurst(Vector2 position, float scale)
        {
            // Central flash
            CustomParticles.GenericFlare(position, FateWhite, 1.2f * scale, 25);
            CustomParticles.GenericFlare(position, FateBrightRed, 0.9f * scale, 22);
            CustomParticles.GenericFlare(position, FateDarkPink, 0.7f * scale, 20);
            
            // Cascading halos
            for (int i = 0; i < 6; i++)
            {
                Color ringColor = Color.Lerp(FateDarkPink, FatePurple, i / 6f);
                CustomParticles.HaloRing(position, ringColor, (0.3f + i * 0.15f) * scale, 15 + i * 3);
            }
            
            // Glyph burst
            CustomParticles.GlyphBurst(position, FatePurple, 8, 6f * scale);
            
            // Radial particles
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 vel = angle.ToRotationVector2() * (8f * scale);
                Color particleColor = Color.Lerp(FateDarkPink, FateBrightRed, Main.rand.NextFloat());
                
                var particle = new GenericGlowParticle(position, vel, particleColor, 0.4f * scale, 25, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }
            
            // Star sparkles
            for (int i = 0; i < 12; i++)
            {
                Vector2 sparklePos = position + Main.rand.NextVector2Circular(40f * scale, 40f * scale);
                CustomParticles.GenericFlare(sparklePos, FateWhite, 0.3f * scale, 15);
            }
        }

        private void SpawnSlashVFX(Vector2 position, Vector2 direction)
        {
            // Slash trail
            for (int i = 0; i < 8; i++)
            {
                float spread = MathHelper.ToRadians(30f);
                Vector2 vel = direction.RotatedBy(Main.rand.NextFloat(-spread, spread)) * Main.rand.NextFloat(4f, 10f);
                Color slashColor = Color.Lerp(FateDarkPink, FateBrightRed, Main.rand.NextFloat());
                
                var particle = new GenericGlowParticle(position, vel, slashColor, 0.35f, 18, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }
            
            CustomParticles.GenericFlare(position, FateWhite, 0.6f, 12);
        }

        #endregion

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // Essence drops only after killing the main Fate boss
            LeadingConditionRule afterBossRule = new LeadingConditionRule(new DownedFateCondition());
            
            // FATE MINI-BOSS DROPS - More valuable than other mini-bosses
            
            // Fate Resonant Energy (guaranteed, generous amount)
            afterBossRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<FateResonantEnergy>(), 1, 12, 20));
            
            // Resonant Core of Fate (guaranteed)
            afterBossRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<ResonantCoreOfFate>(), 1, 4, 8));
            
            // Fate Resonance Ore bonus
            afterBossRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<FateResonanceOre>(), 1, 8, 16));
            
            // Small chance for Harmonic Core of Fate (normally boss-only)
            afterBossRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<HarmonicCoreOfFate>(), 5)); // 20% chance
            
            npcLoot.Add(afterBossRule);
            
            // Money always drops
            npcLoot.Add(ItemDropRule.Common(ItemID.GoldCoin, 1, 5, 10));
            
            // Fate Essence - theme essence drop (15%)
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FateEssence>(), 7));
        }

        public override void OnKill()
        {
            // Death VFX
            SpawnCosmicBurst(NPC.Center, 3f);
            
            // Screen shake
            MagnumScreenEffects.AddScreenShake(15f);
            
            // Sound
            SoundEngine.PlaySound(SoundID.NPCDeath52 with { Volume = 1.2f }, NPC.Center);
            
            // Message
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Main.NewText("The Herald of Fate has been vanquished.", FateDarkPink);
            }
        }

        public override void FindFrame(int frameHeight)
        {
            frameCounter++;
            
            int speed = FrameSpeed;
            if (State != AIState.Hovering)
                speed = 3; // Faster animation during attacks
            
            if (isEnraged)
                speed = Math.Max(2, speed - 1); // Even faster when enraged
            
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
            
            int frameWidth = texture.Width;
            int frameHeight = texture.Height / TotalFrames;
            
            Rectangle frame = new Rectangle(0, currentFrame * frameHeight, frameWidth, frameHeight);
            Vector2 drawPos = NPC.Center - screenPos;
            Vector2 origin = new Vector2(frameWidth / 2f, frameHeight / 2f);
            
            SpriteEffects effects = NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            
            // Afterimage trail
            for (int i = 0; i < NPC.oldPos.Length; i++)
            {
                float progress = (float)i / NPC.oldPos.Length;
                Color trailColor = Color.Lerp(FateDarkPink, FatePurple, progress) * (1f - progress) * 0.4f;
                Vector2 trailPos = NPC.oldPos[i] + NPC.Size / 2f - screenPos;
                spriteBatch.Draw(texture, trailPos, frame, trailColor, NPC.rotation, origin, NPC.scale * (1f - progress * 0.2f), effects, 0f);
            }
            
            // Additive glow layer
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            float pulse = 0.9f + (float)Math.Sin(auraPulse * 2f) * 0.1f;
            Color glowColor = isEnraged ? FateBrightRed : FateDarkPink;
            spriteBatch.Draw(texture, drawPos, frame, glowColor * 0.3f * cosmicGlow, NPC.rotation, origin, NPC.scale * 1.1f * pulse, effects, 0f);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Main sprite
            spriteBatch.Draw(texture, drawPos, frame, drawColor * NPC.Opacity, NPC.rotation, origin, NPC.scale, effects, 0f);
            
            return false;
        }

        public override Color? GetAlpha(Color drawColor)
        {
            // Ensure visibility even in darkness
            float minBrightness = 0.6f;
            Color result = drawColor;
            result.R = (byte)Math.Max(result.R, 255 * minBrightness);
            result.G = (byte)Math.Max(result.G, 255 * minBrightness);
            result.B = (byte)Math.Max(result.B, 255 * minBrightness);
            return result;
        }
    }

    #region Herald of Fate Projectiles

    /// <summary>
    /// Reality-tearing slash projectile
    /// </summary>
    public class CosmicRendSlash : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SwordArc3";

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 20;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Trail
            if (Main.rand.NextBool(2))
            {
                var particle = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f,
                    new Color(180, 50, 100) * 0.7f, 0.3f, 15, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }
            
            Lighting.AddLight(Projectile.Center, new Color(180, 50, 100).ToVector3() * 0.4f);
        }

        public override void OnKill(int timeLeft)
        {
            // Leave lingering damage zone
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero,
                    ModContent.ProjectileType<RealityFractureTrail>(), Projectile.damage / 2, 0f, Main.myPlayer);
            }
            
            CustomParticles.GenericFlare(Projectile.Center, new Color(255, 60, 80), 0.6f, 15);
        }
    }

    /// <summary>
    /// Stellar bolt projectile
    /// </summary>
    public class StellarBolt : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/PrismaticSparkle7";

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Projectile.rotation += 0.2f;
            
            // Sparkle trail
            if (Main.rand.NextBool(3))
            {
                CustomParticles.GenericFlare(Projectile.Center, new Color(255, 255, 255) * 0.5f, 0.15f, 8);
            }
            
            Lighting.AddLight(Projectile.Center, new Color(180, 50, 100).ToVector3() * 0.3f);
        }

        public override void OnKill(int timeLeft)
        {
            CustomParticles.GenericFlare(Projectile.Center, new Color(255, 60, 80), 0.5f, 12);
            CustomParticles.HaloRing(Projectile.Center, new Color(180, 50, 100), 0.3f, 10);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = tex.Size() / 2f;
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            float pulse = 0.9f + (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.1f;
            spriteBatch.Draw(tex, drawPos, null, new Color(120, 30, 140) * 0.5f, Projectile.rotation, origin, 0.4f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, new Color(180, 50, 100) * 0.6f, Projectile.rotation, origin, 0.25f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, Color.White * 0.7f, Projectile.rotation, origin, 0.12f * pulse, SpriteEffects.None, 0f);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
    }

    /// <summary>
    /// Constellation beam projectile
    /// </summary>
    public class ConstellationBeam : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/Glyphs6";

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 2;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Intense trail
            var particle = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.05f,
                new Color(255, 60, 80) * 0.8f, 0.35f, 12, true);
            MagnumParticleHandler.SpawnParticle(particle);
            
            if (Main.rand.NextBool(3))
            {
                CustomParticles.GenericFlare(Projectile.Center, Color.White * 0.6f, 0.2f, 6);
            }
            
            Lighting.AddLight(Projectile.Center, new Color(255, 60, 80).ToVector3() * 0.5f);
        }

        public override void OnKill(int timeLeft)
        {
            CustomParticles.GenericFlare(Projectile.Center, new Color(255, 60, 80), 0.8f, 18);
            CustomParticles.HaloRing(Projectile.Center, new Color(180, 50, 100), 0.5f, 15);
        }
    }

    /// <summary>
    /// Lingering damage trail from reality fracture
    /// </summary>
    public class RealityFractureTrail : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/Glyphs7";

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 100;
        }

        public override void AI()
        {
            Projectile.alpha += 3;
            if (Projectile.alpha >= 255)
                Projectile.Kill();
            
            // Fade effect
            if (Main.rand.NextBool(5))
            {
                CustomParticles.GenericFlare(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f),
                    new Color(120, 30, 140) * 0.4f, 0.2f, 10);
            }
            
            Lighting.AddLight(Projectile.Center, new Color(120, 30, 140).ToVector3() * 0.2f * (1f - Projectile.alpha / 255f));
        }

        public override bool? CanDamage() => Projectile.alpha < 200;
    }

    /// <summary>
    /// Shards from Fate Sealed ultimate attack
    /// </summary>
    public class FateSealedShard : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/PrismaticSparkle9";

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.velocity *= 0.98f; // Slow down over time
            
            if (Main.rand.NextBool(4))
            {
                var particle = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f,
                    new Color(180, 50, 100) * 0.6f, 0.2f, 12, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }
            
            Lighting.AddLight(Projectile.Center, new Color(255, 60, 80).ToVector3() * 0.25f);
        }

        public override void OnKill(int timeLeft)
        {
            CustomParticles.GenericFlare(Projectile.Center, new Color(180, 50, 100), 0.4f, 10);
        }
    }

    #endregion
}
