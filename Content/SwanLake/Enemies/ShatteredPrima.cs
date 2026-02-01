using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.SwanLake.ResonanceEnergies;
using MagnumOpus.Content.SwanLake.ResonantOres;
using MagnumOpus.Content.SwanLake.HarmonicCores;
using MagnumOpus.Content.Materials.EnemyDrops;

namespace MagnumOpus.Content.SwanLake.Enemies
{
    /// <summary>
    /// Shattered Prima - The Prima Ballerina who danced until she shattered.
    /// A tragic mini-boss that spawns in the Hallow after Moon Lord is defeated.
    /// 
    /// She was once the greatest dancer of the monochromatic ballet, but her obsessive
    /// pursuit of perfection fractured her very being. Now she dances eternally,
    /// her body cracked like porcelain, leaking prismatic light from within.
    /// 
    /// 5 GRACEFUL BUT DEADLY ATTACKS:
    /// 1. Pirouette of Blades - Spinning attack launching feather blades in spirals
    /// 2. Grand Jeté - Leaping dash attack with feather explosion on landing
    /// 3. Pas de Deux - Creates a shadow clone that mirrors attacks
    /// 4. Swan's Lament - Channeled scream that sends out damaging sound waves
    /// 5. Dying Swan - Ultimate: Graceful death dance with massive feather storm
    /// 
    /// Features monochrome body with rainbow fractures, floating feathers, and
    /// graceful ballet-inspired animations. She weeps prismatic tears.
    /// </summary>
    public class ShatteredPrima : ModNPC
    {
        // Custom sprite
        public override string Texture => "MagnumOpus/Content/SwanLake/Enemies/ShatteredPrima";
        
        // Swan Lake theme colors - monochrome with rainbow accents
        private static readonly Color SwanWhite = new Color(255, 255, 255);
        private static readonly Color SwanBlack = new Color(20, 20, 30);
        private static readonly Color SwanSilver = new Color(220, 225, 235);
        
        // Size variation
        private float sizeMultiplier = 1f;
        private bool hasSetSize = false;
        
        // Animation - 6x6 spritesheet
        private int frameCounter = 0;
        private int currentFrame = 0;
        private const int FrameColumns = 6;
        private const int FrameRows = 6;
        private const int TotalFrames = 36;
        private const int FrameSpeed = 5;
        
        // Visual effects
        private float gracePulse = 0f;
        private float auraPulse = 0f;
        private float pirouetteRotation = 0f;
        private List<Vector2> orbitingFeatherPositions = new List<Vector2>();
        private float rainbowHue = 0f;
        
        // Shadow clone for Pas de Deux
        private int shadowCloneIndex = -1;
        private bool hasShadowClone = false;
        
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
            Floating,           // Graceful idle
            PirouetteOfBlades,  // Attack 1 - Spinning blade launch
            GrandJete,          // Attack 2 - Leaping dash
            PasDeDeux,          // Attack 3 - Shadow clone
            SwansLament,        // Attack 4 - Sound wave scream
            DyingSwan           // Attack 5 - Ultimate feather storm
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
            
            NPCID.Sets.TrailCacheLength[Type] = 10;
            NPCID.Sets.TrailingMode[Type] = 1;
            
            // Debuff immunities - ethereal dancer
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Frostburn] = true;
            
            // Register for minimap
            MinibossMinimapSystem.RegisterSwanLakeMiniboss(Type);
        }

        public override void SetDefaults()
        {
            // SWAN LAKE MINI-BOSS STATS - Graceful but deadly
            NPC.width = 60;
            NPC.height = 90;
            NPC.damage = 140; // Slightly lower than other minibosses
            NPC.defense = 55; // Graceful = less armored
            NPC.lifeMax = 75000; // Mini-boss HP
            NPC.HitSound = SoundID.NPCHit52; // Crystal/ethereal hit
            NPC.DeathSound = SoundID.NPCDeath55; // Ethereal death
            NPC.knockBackResist = 0.1f; // Light, can be pushed slightly
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = Item.buyPrice(gold: 50);
            NPC.aiStyle = -1; // Custom AI
            NPC.lavaImmune = true;
            NPC.npcSlots = 8f;
            NPC.boss = false; // Mini-boss
            NPC.Opacity = 0.95f;
            NPC.alpha = 25; // Slightly translucent
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheHallow,
                new FlavorTextBestiaryInfoElement(
                    "Once the most celebrated Prima Ballerina of the Monochromatic Ballet, " +
                    "she pursued perfection until her very soul fractured. Now she dances eternally " +
                    "in the Hallow, her porcelain form cracked with prismatic light, " +
                    "weeping rainbow tears as she performs her final, deadly dance.")
            });
        }

        public override void OnSpawn(IEntitySource source)
        {
            // Size variation ±15%
            if (!hasSetSize)
            {
                sizeMultiplier = Main.rand.NextFloat(0.85f, 1.15f);
                NPC.scale *= sizeMultiplier;
                hasSetSize = true;
            }
            
            // Spawn announcement
            if (Main.netMode != NetmodeID.Server)
            {
                Main.NewText("The Shattered Prima rises to dance once more...", SwanSilver);
            }
            
            // Dramatic entrance VFX
            SpawnEntranceVFX();
        }

        private void SpawnEntranceVFX()
        {
            // Feather burst
            for (int i = 0; i < 30; i++)
            {
                float angle = MathHelper.TwoPi * i / 30f;
                Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Color featherColor = Main.rand.NextBool() ? SwanWhite : SwanBlack;
                
                CustomParticles.SwanFeatherDrift(NPC.Center + Main.rand.NextVector2Circular(30f, 30f), featherColor, 0.5f);
            }
            
            // Prismatic burst
            CustomParticles.PrismaticSparkleRainbow(NPC.Center, 20);
            
            // Central flash
            CustomParticles.GenericFlare(NPC.Center, SwanWhite, 1.5f, 30);
            
            // Cascading halos
            for (int i = 0; i < 5; i++)
            {
                Color haloColor = i % 2 == 0 ? SwanWhite : SwanBlack;
                CustomParticles.HaloRing(NPC.Center, haloColor, 0.4f + i * 0.15f, 20 + i * 3);
            }
        }

        public override void AI()
        {
            Player target = Main.player[NPC.target];
            
            // Target validation
            if (!target.active || target.dead)
            {
                NPC.TargetClosest(true);
                target = Main.player[NPC.target];
                
                if (!target.active || target.dead)
                {
                    NPC.velocity.Y += 0.1f;
                    NPC.timeLeft = Math.Min(NPC.timeLeft, 60);
                    return;
                }
            }
            
            // Update visual timers
            gracePulse += 0.03f;
            auraPulse += 0.05f;
            rainbowHue = (rainbowHue + 0.01f) % 1f;
            
            // Dynamic lighting - rainbow shimmer
            float lightPulse = 0.7f + (float)Math.Sin(gracePulse * 2f) * 0.2f;
            Color rainbowLight = Main.hslToRgb(rainbowHue, 0.5f, 0.6f);
            Lighting.AddLight(NPC.Center, rainbowLight.ToVector3() * lightPulse);
            
            // Spawn ambient particles
            SpawnAmbientParticles();
            
            // Update orbiting feathers
            UpdateOrbitingFeathers();
            
            // Manage shadow clone
            ManageShadowClone();
            
            // Decrement attack cooldown
            if (attackCooldown > 0) attackCooldown--;
            
            // State machine
            switch (State)
            {
                case AIState.Floating:
                    AI_Floating(target);
                    break;
                case AIState.PirouetteOfBlades:
                    AI_PirouetteOfBlades(target);
                    break;
                case AIState.GrandJete:
                    AI_GrandJete(target);
                    break;
                case AIState.PasDeDeux:
                    AI_PasDeDeux(target);
                    break;
                case AIState.SwansLament:
                    AI_SwansLament(target);
                    break;
                case AIState.DyingSwan:
                    AI_DyingSwan(target);
                    break;
            }
            
            Timer++;
        }

        private void SpawnAmbientParticles()
        {
            // Floating feathers
            if (Main.rand.NextBool(8))
            {
                Color featherColor = Main.rand.NextBool() ? SwanWhite : SwanBlack;
                Vector2 offset = Main.rand.NextVector2Circular(40f, 60f);
                CustomParticles.SwanFeatherDrift(NPC.Center + offset, featherColor, Main.rand.NextFloat(0.2f, 0.4f));
            }
            
            // Prismatic sparkles from cracks
            if (Main.rand.NextBool(6))
            {
                Vector2 crackOffset = Main.rand.NextVector2Circular(25f, 35f);
                Color sparkleColor = Main.hslToRgb(Main.rand.NextFloat(), 0.9f, 0.8f);
                CustomParticles.PrismaticSparkle(NPC.Center + crackOffset, sparkleColor, Main.rand.NextFloat(0.2f, 0.35f));
            }
            
            // Prismatic tears
            if (Main.rand.NextBool(15))
            {
                Vector2 eyeOffset = new Vector2(NPC.direction * 8f, -20f);
                Color tearColor = Main.hslToRgb(rainbowHue, 0.8f, 0.75f);
                var tear = new GenericGlowParticle(
                    NPC.Center + eyeOffset,
                    new Vector2(0, 2f),
                    tearColor, 0.2f, 30, true);
                MagnumParticleHandler.SpawnParticle(tear);
            }
            
            // Soft glow aura
            if (Main.rand.NextBool(10))
            {
                float glowAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 glowPos = NPC.Center + glowAngle.ToRotationVector2() * Main.rand.NextFloat(30f, 50f);
                Color glowColor = Main.rand.NextBool() ? SwanWhite * 0.5f : Main.hslToRgb(rainbowHue, 0.6f, 0.7f) * 0.5f;
                var glow = new GenericGlowParticle(glowPos, Vector2.Zero, glowColor, 0.25f, 20, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
        }

        private void UpdateOrbitingFeathers()
        {
            // Maintain 6 orbiting feathers
            int featherCount = 6;
            float orbitRadius = 50f + (float)Math.Sin(gracePulse) * 10f;
            float baseAngle = gracePulse * 0.5f;
            
            orbitingFeatherPositions.Clear();
            for (int i = 0; i < featherCount; i++)
            {
                float angle = baseAngle + MathHelper.TwoPi * i / featherCount;
                Vector2 featherPos = NPC.Center + angle.ToRotationVector2() * orbitRadius;
                orbitingFeatherPositions.Add(featherPos);
                
                // Subtle trail
                if (Main.rand.NextBool(8))
                {
                    Color trailColor = i % 2 == 0 ? SwanWhite * 0.5f : SwanBlack * 0.8f;
                    var trail = new GenericGlowParticle(featherPos, Vector2.Zero, trailColor, 0.15f, 10, true);
                    MagnumParticleHandler.SpawnParticle(trail);
                }
            }
        }

        private void ManageShadowClone()
        {
            if (hasShadowClone && shadowCloneIndex >= 0)
            {
                Projectile clone = Main.projectile[shadowCloneIndex];
                if (!clone.active || clone.type != ModContent.ProjectileType<ShatteredPrimaShadow>())
                {
                    hasShadowClone = false;
                    shadowCloneIndex = -1;
                }
            }
        }

        #region AI States

        private void AI_Floating(Player target)
        {
            // Graceful floating movement
            Vector2 toTarget = target.Center - NPC.Center;
            float distance = toTarget.Length();
            
            // Maintain comfortable distance (200-350 units)
            float idealDistance = 275f;
            Vector2 idealPosition = target.Center - toTarget.SafeNormalize(Vector2.UnitX) * idealDistance;
            
            // Add gentle bobbing
            idealPosition.Y += (float)Math.Sin(gracePulse * 2f) * 20f;
            
            // Smooth movement
            Vector2 toIdeal = idealPosition - NPC.Center;
            NPC.velocity = Vector2.Lerp(NPC.velocity, toIdeal * 0.03f, 0.08f);
            
            // Limit speed
            if (NPC.velocity.Length() > 6f)
                NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * 6f;
            
            // Face target
            NPC.direction = toTarget.X > 0 ? 1 : -1;
            NPC.spriteDirection = NPC.direction;
            
            // Attack selection
            if (attackCooldown <= 0 && distance < 600f)
            {
                SelectNextAttack(target);
            }
        }

        private void SelectNextAttack(Player target)
        {
            // Calculate HP percentage for attack selection
            float hpPercent = (float)NPC.life / NPC.lifeMax;
            
            List<AIState> availableAttacks = new List<AIState>
            {
                AIState.PirouetteOfBlades,
                AIState.GrandJete,
                AIState.SwansLament
            };
            
            // Add Pas de Deux at 70% HP
            if (hpPercent < 0.7f && !hasShadowClone)
            {
                availableAttacks.Add(AIState.PasDeDeux);
            }
            
            // Add Dying Swan at 30% HP
            if (hpPercent < 0.3f)
            {
                availableAttacks.Add(AIState.DyingSwan);
            }
            
            // Random selection
            State = availableAttacks[Main.rand.Next(availableAttacks.Count)];
            Timer = 0;
            SubTimer = 0;
            AttackPhase = 0;
            attackCounter++;
            
            // Attack announcement VFX
            CustomParticles.GenericFlare(NPC.Center, SwanWhite, 0.8f, 15);
        }

        private void AI_PirouetteOfBlades(Player target)
        {
            int windupTime = 60;
            int spinTime = 180;
            int recoveryTime = 45;
            
            if (AttackPhase == 0) // Windup
            {
                // Rise up slightly
                NPC.velocity.Y = MathHelper.Lerp(NPC.velocity.Y, -2f, 0.1f);
                NPC.velocity.X *= 0.95f;
                
                // Build pirouette rotation
                float progress = Timer / (float)windupTime;
                pirouetteRotation += progress * 0.2f;
                
                // Converging particles
                BossVFXOptimizer.ConvergingWarning(NPC.Center, 100f, progress, SwanWhite, 8);
                
                // Sound cue
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.5f }, NPC.Center);
                }
                
                if (Timer >= windupTime)
                {
                    Timer = 0;
                    AttackPhase = 1;
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f }, NPC.Center);
                }
            }
            else if (AttackPhase == 1) // Spinning attack
            {
                // Rapid rotation
                pirouetteRotation += 0.5f;
                
                // Stay relatively still
                NPC.velocity *= 0.9f;
                
                // Launch feather blades periodically
                int fireInterval = 12;
                if (Timer % fireInterval == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int bladeCount = 4;
                    for (int i = 0; i < bladeCount; i++)
                    {
                        float angle = pirouetteRotation + MathHelper.TwoPi * i / bladeCount;
                        Vector2 velocity = angle.ToRotationVector2() * 10f;
                        
                        // Alternate black and white blades
                        int bladeType = i % 2 == 0 ? 1 : 0; // 1 = white, 0 = black
                        
                        Projectile.NewProjectile(
                            NPC.GetSource_FromAI(),
                            NPC.Center,
                            velocity,
                            ModContent.ProjectileType<PrimaFeatherBlade>(),
                            NPC.damage / 2,
                            2f,
                            Main.myPlayer,
                            bladeType
                        );
                    }
                    
                    // VFX burst
                    for (int i = 0; i < bladeCount; i++)
                    {
                        float angle = pirouetteRotation + MathHelper.TwoPi * i / bladeCount;
                        Vector2 offset = angle.ToRotationVector2() * 30f;
                        Color bladeColor = i % 2 == 0 ? SwanWhite : SwanBlack;
                        CustomParticles.GenericFlare(NPC.Center + offset, bladeColor, 0.4f, 12);
                    }
                }
                
                // Central spinning VFX
                if (Timer % 6 == 0)
                {
                    float ringAngle = pirouetteRotation * 2f;
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = ringAngle + MathHelper.TwoPi * i / 8f;
                        Vector2 pos = NPC.Center + angle.ToRotationVector2() * 25f;
                        Color ringColor = Main.hslToRgb((rainbowHue + i * 0.1f) % 1f, 0.8f, 0.8f);
                        var glow = new GenericGlowParticle(pos, Vector2.Zero, ringColor * 0.6f, 0.2f, 8, true);
                        MagnumParticleHandler.SpawnParticle(glow);
                    }
                }
                
                if (Timer >= spinTime)
                {
                    Timer = 0;
                    AttackPhase = 2;
                }
            }
            else // Recovery
            {
                pirouetteRotation *= 0.95f;
                NPC.velocity *= 0.95f;
                
                if (Timer >= recoveryTime)
                {
                    EndAttack();
                }
            }
        }

        private void AI_GrandJete(Player target)
        {
            int windupTime = 45;
            int leapTime = 30;
            int recoveryTime = 60;
            
            if (AttackPhase == 0) // Windup - crouch
            {
                NPC.velocity *= 0.9f;
                
                float progress = Timer / (float)windupTime;
                
                // Warning line to target
                Vector2 toTarget = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                BossVFXOptimizer.WarningLine(NPC.Center, toTarget, 400f, 10, WarningType.Danger);
                
                // Building energy
                if (Timer % 5 == 0)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 offset = Main.rand.NextVector2Circular(40f, 40f);
                        var glow = new GenericGlowParticle(
                            NPC.Center + offset,
                            -offset * 0.1f,
                            Main.rand.NextBool() ? SwanWhite : SwanBlack,
                            0.25f, 15, true);
                        MagnumParticleHandler.SpawnParticle(glow);
                    }
                }
                
                if (Timer >= windupTime)
                {
                    Timer = 0;
                    AttackPhase = 1;
                    SubTimer = 0;
                    
                    // Calculate leap velocity
                    Vector2 leapTarget = target.Center + target.velocity * 20f;
                    NPC.velocity = (leapTarget - NPC.Center).SafeNormalize(Vector2.UnitX) * 25f;
                    
                    SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.3f }, NPC.Center);
                    
                    // Launch VFX
                    BossVFXOptimizer.AttackReleaseBurst(NPC.Center, SwanWhite, SwanBlack, 1.2f);
                }
            }
            else if (AttackPhase == 1) // Leaping
            {
                // Trail particles
                for (int i = 0; i < 3; i++)
                {
                    Vector2 trailPos = NPC.Center - NPC.velocity * 0.3f + Main.rand.NextVector2Circular(15f, 15f);
                    Color trailColor = Main.rand.NextBool() ? SwanWhite : SwanBlack;
                    CustomParticles.SwanFeatherDrift(trailPos, trailColor, 0.35f);
                }
                
                // Rainbow sparkle trail
                if (Timer % 3 == 0)
                {
                    Color sparkleColor = Main.hslToRgb((rainbowHue + Timer * 0.02f) % 1f, 0.9f, 0.85f);
                    CustomParticles.PrismaticSparkle(NPC.Center, sparkleColor, 0.3f);
                }
                
                // Check if passed target or time's up
                SubTimer++;
                if (SubTimer >= leapTime)
                {
                    Timer = 0;
                    AttackPhase = 2;
                    
                    // Landing explosion
                    SpawnLandingExplosion();
                }
            }
            else // Recovery
            {
                NPC.velocity *= 0.92f;
                
                if (Timer >= recoveryTime)
                {
                    EndAttack();
                }
            }
        }

        private void SpawnLandingExplosion()
        {
            // Massive feather explosion
            for (int i = 0; i < 24; i++)
            {
                float angle = MathHelper.TwoPi * i / 24f;
                Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 12f);
                Color featherColor = i % 2 == 0 ? SwanWhite : SwanBlack;
                CustomParticles.SwanFeatherDrift(NPC.Center + velocity * 2f, featherColor, Main.rand.NextFloat(0.3f, 0.5f));
            }
            
            // Spawn damaging feather projectiles
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int projectileCount = 16;
                for (int i = 0; i < projectileCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / projectileCount;
                    Vector2 velocity = angle.ToRotationVector2() * 8f;
                    
                    Projectile.NewProjectile(
                        NPC.GetSource_FromAI(),
                        NPC.Center,
                        velocity,
                        ModContent.ProjectileType<PrimaFeatherBlade>(),
                        NPC.damage / 2,
                        2f,
                        Main.myPlayer,
                        Main.rand.Next(2)
                    );
                }
            }
            
            // Prismatic flash
            CustomParticles.GenericFlare(NPC.Center, SwanWhite, 1.5f, 25);
            CustomParticles.PrismaticSparkleRainbow(NPC.Center, 15);
            
            // Cascading halos
            for (int i = 0; i < 6; i++)
            {
                Color haloColor = i % 2 == 0 ? SwanWhite : SwanBlack;
                CustomParticles.HaloRing(NPC.Center, haloColor, 0.4f + i * 0.12f, 18 + i * 2);
            }
            
            // Screen shake
            MagnumScreenEffects.AddScreenShake(8f);
            
            SoundEngine.PlaySound(SoundID.Item122 with { Volume = 1.3f, Pitch = -0.2f }, NPC.Center);
        }

        private void AI_PasDeDeux(Player target)
        {
            int summonTime = 90;
            int danceTime = 300;
            int recoveryTime = 60;
            
            if (AttackPhase == 0) // Summoning the shadow
            {
                NPC.velocity *= 0.9f;
                
                float progress = Timer / (float)summonTime;
                
                // Mirror appears
                Vector2 mirrorPos = NPC.Center + new Vector2(NPC.direction * -100f, 0);
                
                // Building shadow particles
                if (Timer % 4 == 0)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        Vector2 offset = Main.rand.NextVector2Circular(50f, 50f);
                        var shadow = new GenericGlowParticle(
                            mirrorPos + offset,
                            -offset * 0.05f,
                            SwanBlack * (0.3f + progress * 0.5f),
                            0.3f, 20, true);
                        MagnumParticleHandler.SpawnParticle(shadow);
                    }
                }
                
                // Central flare grows
                if (Timer % 10 == 0)
                {
                    CustomParticles.GenericFlare(mirrorPos, SwanBlack, 0.3f + progress * 0.5f, 15);
                }
                
                if (Timer >= summonTime)
                {
                    Timer = 0;
                    AttackPhase = 1;
                    
                    // Spawn the shadow clone
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        shadowCloneIndex = Projectile.NewProjectile(
                            NPC.GetSource_FromAI(),
                            mirrorPos,
                            Vector2.Zero,
                            ModContent.ProjectileType<ShatteredPrimaShadow>(),
                            NPC.damage / 2,
                            2f,
                            Main.myPlayer,
                            NPC.whoAmI
                        );
                        hasShadowClone = true;
                    }
                    
                    // Spawn VFX
                    CustomParticles.GenericFlare(mirrorPos, SwanBlack, 1.2f, 25);
                    for (int i = 0; i < 5; i++)
                    {
                        CustomParticles.HaloRing(mirrorPos, SwanBlack, 0.3f + i * 0.1f, 15 + i * 2);
                    }
                    
                    SoundEngine.PlaySound(SoundID.Item103 with { Pitch = -0.3f }, mirrorPos);
                }
            }
            else if (AttackPhase == 1) // Dancing with shadow
            {
                // Graceful movement - both entities move in mirrored patterns
                Vector2 toTarget = target.Center - NPC.Center;
                float idealDistance = 250f;
                Vector2 idealPos = target.Center - toTarget.SafeNormalize(Vector2.UnitX) * idealDistance;
                
                // Circular dance around player
                float danceAngle = Timer * 0.02f;
                idealPos += new Vector2((float)Math.Cos(danceAngle), (float)Math.Sin(danceAngle)) * 80f;
                
                NPC.velocity = Vector2.Lerp(NPC.velocity, (idealPos - NPC.Center) * 0.05f, 0.1f);
                
                // Face target
                NPC.direction = toTarget.X > 0 ? 1 : -1;
                NPC.spriteDirection = NPC.direction;
                
                // Synchronized attacks every 60 frames
                if (Timer % 60 == 0 && Timer > 0 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    // Launch feathers at player
                    Vector2 velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX) * 10f;
                    
                    Projectile.NewProjectile(
                        NPC.GetSource_FromAI(),
                        NPC.Center,
                        velocity,
                        ModContent.ProjectileType<PrimaFeatherBlade>(),
                        NPC.damage / 2,
                        2f,
                        Main.myPlayer,
                        1 // White blade from Prima
                    );
                    
                    CustomParticles.GenericFlare(NPC.Center, SwanWhite, 0.6f, 15);
                }
                
                // Dance particles
                if (Timer % 15 == 0)
                {
                    Vector2 trailOffset = Main.rand.NextVector2Circular(20f, 20f);
                    Color trailColor = Main.hslToRgb((rainbowHue + Timer * 0.005f) % 1f, 0.7f, 0.7f);
                    CustomParticles.PrismaticSparkle(NPC.Center + trailOffset, trailColor, 0.25f);
                }
                
                if (Timer >= danceTime || !hasShadowClone)
                {
                    Timer = 0;
                    AttackPhase = 2;
                }
            }
            else // Recovery
            {
                NPC.velocity *= 0.95f;
                
                if (Timer >= recoveryTime)
                {
                    EndAttack();
                }
            }
        }

        private void AI_SwansLament(Player target)
        {
            int windupTime = 75;
            int screamTime = 120;
            int recoveryTime = 60;
            
            if (AttackPhase == 0) // Windup - throwing head back
            {
                NPC.velocity *= 0.9f;
                
                float progress = Timer / (float)windupTime;
                
                // Building energy at throat
                Vector2 throatPos = NPC.Center + new Vector2(0, -15f);
                
                if (Timer % 3 == 0)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 offset = Main.rand.NextVector2Circular(60f, 60f);
                        var glow = new GenericGlowParticle(
                            throatPos + offset,
                            -offset * 0.08f,
                            Main.hslToRgb(rainbowHue, 0.8f, 0.8f) * progress,
                            0.2f, 15, true);
                        MagnumParticleHandler.SpawnParticle(glow);
                    }
                }
                
                // Danger zone warning
                BossVFXOptimizer.DangerZoneRing(NPC.Center, 200f + progress * 100f, 16);
                
                if (Timer >= windupTime)
                {
                    Timer = 0;
                    AttackPhase = 1;
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.5f, Volume = 1.5f }, NPC.Center);
                }
            }
            else if (AttackPhase == 1) // Screaming - sound waves
            {
                NPC.velocity *= 0.95f;
                
                // Spawn sound wave rings periodically
                if (Timer % 20 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int waveIndex = (int)(Timer / 20);
                    
                    Projectile.NewProjectile(
                        NPC.GetSource_FromAI(),
                        NPC.Center,
                        Vector2.Zero,
                        ModContent.ProjectileType<PrimaSoundWave>(),
                        NPC.damage / 3,
                        0f,
                        Main.myPlayer,
                        waveIndex * 20f // Starting radius
                    );
                    
                    // VFX ring
                    Color waveColor = waveIndex % 2 == 0 ? SwanWhite : Main.hslToRgb(rainbowHue, 0.9f, 0.8f);
                    CustomParticles.HaloRing(NPC.Center, waveColor, 0.5f, 25);
                }
                
                // Continuous rainbow particles
                if (Timer % 4 == 0)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 8f + Timer * 0.05f;
                        float radius = 30f + Timer * 1.5f;
                        Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                        Color sparkleColor = Main.hslToRgb((rainbowHue + i * 0.1f) % 1f, 0.9f, 0.85f);
                        CustomParticles.PrismaticSparkle(pos, sparkleColor, 0.25f);
                    }
                }
                
                // Feathers blown outward
                if (Timer % 8 == 0)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        Vector2 featherPos = NPC.Center + angle.ToRotationVector2() * 40f;
                        Color featherColor = Main.rand.NextBool() ? SwanWhite : SwanBlack;
                        CustomParticles.SwanFeatherDrift(featherPos, featherColor, 0.35f);
                    }
                }
                
                if (Timer >= screamTime)
                {
                    Timer = 0;
                    AttackPhase = 2;
                }
            }
            else // Recovery
            {
                NPC.velocity *= 0.95f;
                
                if (Timer >= recoveryTime)
                {
                    EndAttack();
                }
            }
        }

        private void AI_DyingSwan(Player target)
        {
            int windupTime = 90;
            int danceTime = 240;
            int finaleTime = 60;
            int recoveryTime = 90;
            
            if (AttackPhase == 0) // Dramatic windup
            {
                NPC.velocity *= 0.9f;
                
                float progress = Timer / (float)windupTime;
                
                // Text announcement at start
                if (Timer == 1 && Main.netMode != NetmodeID.Server)
                {
                    Main.NewText("The Shattered Prima begins her final dance...", Main.hslToRgb(rainbowHue, 0.7f, 0.8f));
                }
                
                // Building dramatic energy
                if (Timer % 2 == 0)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 6f + Timer * 0.1f;
                        float radius = 150f * (1f - progress * 0.5f);
                        Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                        
                        Color glowColor = Main.hslToRgb((rainbowHue + i * 0.15f) % 1f, 0.9f, 0.8f);
                        var glow = new GenericGlowParticle(pos, -angle.ToRotationVector2() * 2f, glowColor * progress, 0.3f, 15, true);
                        MagnumParticleHandler.SpawnParticle(glow);
                    }
                }
                
                // Feathers spiraling inward
                if (Timer % 5 == 0)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = Timer * 0.2f + MathHelper.TwoPi * i / 8f;
                        float radius = 200f * (1f - progress);
                        Vector2 featherPos = NPC.Center + angle.ToRotationVector2() * radius;
                        Color featherColor = i % 2 == 0 ? SwanWhite : SwanBlack;
                        CustomParticles.SwanFeatherDrift(featherPos, featherColor, 0.4f);
                    }
                }
                
                // Screen shake builds
                if (Timer > windupTime * 0.7f)
                {
                    MagnumScreenEffects.AddScreenShake(progress * 3f);
                }
                
                if (Timer >= windupTime)
                {
                    Timer = 0;
                    AttackPhase = 1;
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f, Volume = 1.5f }, NPC.Center);
                }
            }
            else if (AttackPhase == 1) // The dying dance - feather storm
            {
                // Slow, graceful spiral movement
                float spiralAngle = Timer * 0.03f;
                float spiralRadius = 100f + (float)Math.Sin(Timer * 0.05f) * 50f;
                Vector2 idealPos = target.Center + spiralAngle.ToRotationVector2() * spiralRadius;
                
                NPC.velocity = Vector2.Lerp(NPC.velocity, (idealPos - NPC.Center) * 0.04f, 0.05f);
                
                // Constant feather projectile rain
                if (Timer % 8 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int featherCount = 3;
                    for (int i = 0; i < featherCount; i++)
                    {
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 10f);
                        
                        Projectile.NewProjectile(
                            NPC.GetSource_FromAI(),
                            NPC.Center,
                            velocity,
                            ModContent.ProjectileType<PrimaFeatherBlade>(),
                            NPC.damage / 2,
                            2f,
                            Main.myPlayer,
                            Main.rand.Next(2)
                        );
                    }
                }
                
                // Massive ambient particles
                if (Timer % 3 == 0)
                {
                    // Feathers everywhere
                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 randomPos = NPC.Center + Main.rand.NextVector2Circular(150f, 150f);
                        Color featherColor = Main.rand.NextBool() ? SwanWhite : SwanBlack;
                        CustomParticles.SwanFeatherDrift(randomPos, featherColor, Main.rand.NextFloat(0.3f, 0.5f));
                    }
                    
                    // Rainbow sparkles
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 sparklePos = NPC.Center + Main.rand.NextVector2Circular(100f, 100f);
                        Color sparkleColor = Main.hslToRgb(Main.rand.NextFloat(), 0.9f, 0.85f);
                        CustomParticles.PrismaticSparkle(sparklePos, sparkleColor, Main.rand.NextFloat(0.2f, 0.4f));
                    }
                }
                
                // Periodic halo bursts
                if (Timer % 30 == 0)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Color haloColor = i % 2 == 0 ? SwanWhite : Main.hslToRgb(rainbowHue, 0.8f, 0.7f);
                        CustomParticles.HaloRing(NPC.Center, haloColor, 0.4f + i * 0.1f, 15 + i * 2);
                    }
                }
                
                if (Timer >= danceTime)
                {
                    Timer = 0;
                    AttackPhase = 2;
                }
            }
            else if (AttackPhase == 2) // Finale - massive explosion
            {
                NPC.velocity *= 0.9f;
                
                float progress = Timer / (float)finaleTime;
                
                // Building to climax
                if (Timer % 2 == 0)
                {
                    int burstCount = (int)(8 + progress * 12);
                    for (int i = 0; i < burstCount; i++)
                    {
                        float angle = MathHelper.TwoPi * i / burstCount;
                        float radius = 20f + progress * 80f;
                        Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                        Color glowColor = Main.hslToRgb((rainbowHue + progress + i * 0.05f) % 1f, 1f, 0.85f);
                        CustomParticles.GenericFlare(pos, glowColor, 0.3f + progress * 0.3f, 10);
                    }
                }
                
                // Final explosion
                if (Timer == finaleTime - 1)
                {
                    // MASSIVE feather explosion
                    for (int i = 0; i < 48; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 48f;
                        Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(8f, 16f);
                        Color featherColor = i % 2 == 0 ? SwanWhite : SwanBlack;
                        CustomParticles.SwanFeatherDrift(NPC.Center, featherColor, Main.rand.NextFloat(0.4f, 0.6f));
                    }
                    
                    // Full rainbow sparkle ring
                    CustomParticles.PrismaticSparkleRainbow(NPC.Center, 30);
                    
                    // Central flash
                    CustomParticles.GenericFlare(NPC.Center, Color.White, 2f, 35);
                    
                    // Massive cascading halos
                    for (int i = 0; i < 10; i++)
                    {
                        Color haloColor = Main.hslToRgb(i * 0.1f, 0.9f, 0.8f);
                        CustomParticles.HaloRing(NPC.Center, haloColor, 0.5f + i * 0.15f, 25 + i * 3);
                    }
                    
                    // Spawn final projectile burst
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int projectileCount = 24;
                        for (int i = 0; i < projectileCount; i++)
                        {
                            float angle = MathHelper.TwoPi * i / projectileCount;
                            Vector2 velocity = angle.ToRotationVector2() * 12f;
                            
                            Projectile.NewProjectile(
                                NPC.GetSource_FromAI(),
                                NPC.Center,
                                velocity,
                                ModContent.ProjectileType<PrimaFeatherBlade>(),
                                NPC.damage / 2,
                                2f,
                                Main.myPlayer,
                                i % 2
                            );
                        }
                    }
                    
                    MagnumScreenEffects.AddScreenShake(15f);
                    SoundEngine.PlaySound(SoundID.Item122 with { Volume = 1.5f, Pitch = 0.5f }, NPC.Center);
                }
                
                if (Timer >= finaleTime)
                {
                    Timer = 0;
                    AttackPhase = 3;
                }
            }
            else // Recovery
            {
                NPC.velocity *= 0.95f;
                
                if (Timer >= recoveryTime)
                {
                    EndAttack();
                }
            }
        }

        private void EndAttack()
        {
            State = AIState.Floating;
            Timer = 0;
            SubTimer = 0;
            AttackPhase = 0;
            attackCooldown = 120f; // 2 second cooldown between attacks
        }

        #endregion

        #region Drawing

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[Type].Value;
            Vector2 drawPos = NPC.Center - screenPos;
            Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
            
            // Calculate frame rectangle for 6x6 spritesheet
            int frameWidth = texture.Width / FrameColumns;
            int frameHeight = texture.Height / FrameRows;
            int frameX = currentFrame % FrameColumns;
            int frameY = currentFrame / FrameColumns;
            Rectangle sourceRect = new Rectangle(frameX * frameWidth, frameY * frameHeight, frameWidth, frameHeight);
            origin = new Vector2(frameWidth / 2f, frameHeight / 2f);
            
            // Draw afterimages
            for (int i = NPC.oldPos.Length - 1; i >= 0; i--)
            {
                Vector2 afterimagePos = NPC.oldPos[i] + NPC.Size / 2f - screenPos;
                float progress = (float)i / NPC.oldPos.Length;
                Color afterimageColor = (i % 2 == 0 ? SwanWhite : SwanBlack) * (1f - progress) * 0.4f;
                
                spriteBatch.Draw(texture, afterimagePos, sourceRect, afterimageColor, 
                    NPC.rotation + pirouetteRotation * (1f - progress), origin, NPC.scale * (1f - progress * 0.2f), 
                    NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            }
            
            // Begin additive blending for glow
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Rainbow glow layers
            float glowPulse = 0.8f + (float)Math.Sin(gracePulse * 3f) * 0.2f;
            for (int i = 0; i < 4; i++)
            {
                float hue = (rainbowHue + i * 0.2f) % 1f;
                Color glowColor = Main.hslToRgb(hue, 0.8f, 0.7f) * (0.3f / (i + 1)) * glowPulse;
                float glowScale = NPC.scale * (1.1f + i * 0.08f);
                
                spriteBatch.Draw(texture, drawPos, sourceRect, glowColor,
                    NPC.rotation + pirouetteRotation, origin, glowScale,
                    NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            }
            
            // End additive, return to normal
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Draw orbiting feathers
            Texture2D featherTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SwanFeather3").Value;
            foreach (var featherPos in orbitingFeatherPositions)
            {
                int index = orbitingFeatherPositions.IndexOf(featherPos);
                Color featherColor = index % 2 == 0 ? SwanWhite : SwanBlack;
                featherColor *= 0.8f;
                
                spriteBatch.Draw(featherTex, featherPos - screenPos, null, featherColor,
                    gracePulse + index, featherTex.Size() / 2f, 0.3f, SpriteEffects.None, 0f);
            }
            
            // Draw main sprite
            spriteBatch.Draw(texture, drawPos, sourceRect, drawColor * NPC.Opacity,
                NPC.rotation + pirouetteRotation, origin, NPC.scale,
                NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            
            return false;
        }

        public override void FindFrame(int frameHeight)
        {
            frameCounter++;
            if (frameCounter >= FrameSpeed)
            {
                frameCounter = 0;
                currentFrame++;
                if (currentFrame >= TotalFrames)
                    currentFrame = 0;
            }
        }

        #endregion

        #region Loot and Spawning

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // Essence drops only after killing the main Swan Lake boss
            LeadingConditionRule afterBossRule = new LeadingConditionRule(new DownedSwanLakeCondition());
            
            // Mini-boss tier drops - matching other themes
            // Swan Lake materials
            afterBossRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<SwansResonanceEnergy>(), 1, 8, 12));
            afterBossRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<RemnantOfSwansHarmony>(), 1, 12, 18));
            afterBossRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<ShardOfTheFeatheredTempo>(), 1, 3, 6));
            
            // Small chance for Resonant Core
            afterBossRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<ResonantCoreOfSwanLake>(), 10));
            
            npcLoot.Add(afterBossRule);
            
            // Money always drops
            npcLoot.Add(ItemDropRule.Common(ItemID.GoldCoin, 1, 8, 15));
            
            // Grace Essence - theme essence drop (15%)
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<GraceEssence>(), 7));
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            // Mini-boss - 5% spawn rate in Hallow after Moon Lord
            if (!NPC.downedMoonlord) return 0f;
            if (!spawnInfo.Player.ZoneHallow) return 0f;
            if (spawnInfo.Player.ZoneTowerNebula || spawnInfo.Player.ZoneTowerSolar || 
                spawnInfo.Player.ZoneTowerStardust || spawnInfo.Player.ZoneTowerVortex) return 0f;
            
            // Don't spawn if one already exists
            if (NPC.AnyNPCs(Type)) return 0f;
            
            return 0.05f; // 5% chance
        }

        public override void OnKill()
        {
            // Death VFX
            // Massive feather explosion
            for (int i = 0; i < 60; i++)
            {
                float angle = MathHelper.TwoPi * i / 60f;
                Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 12f);
                Color featherColor = i % 2 == 0 ? SwanWhite : SwanBlack;
                CustomParticles.SwanFeatherDrift(NPC.Center + velocity * 2f, featherColor, Main.rand.NextFloat(0.3f, 0.6f));
            }
            
            // Full rainbow explosion
            CustomParticles.PrismaticSparkleRainbow(NPC.Center, 40);
            
            // Central white flash
            CustomParticles.GenericFlare(NPC.Center, Color.White, 2f, 40);
            
            // Cascading rainbow halos
            for (int i = 0; i < 12; i++)
            {
                Color haloColor = Main.hslToRgb(i / 12f, 1f, 0.85f);
                CustomParticles.HaloRing(NPC.Center, haloColor, 0.5f + i * 0.12f, 25 + i * 3);
            }
            
            if (Main.netMode != NetmodeID.Server)
            {
                Main.NewText("The Prima's dance ends at last...", SwanSilver);
            }
        }

        #endregion
    }

    #region Projectiles

    /// <summary>
    /// Feather blade projectile for Shattered Prima attacks.
    /// ai[0] = 0 for black, 1 for white
    /// </summary>
    public class PrimaFeatherBlade : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SwanFeather4";
        
        private bool isWhite => Projectile.ai[0] == 1;
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.alpha = 50;
        }

        public override void AI()
        {
            // Rotation based on velocity
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Trail particles
            if (Main.rand.NextBool(3))
            {
                Color trailColor = isWhite ? Color.White * 0.6f : new Color(20, 20, 30) * 0.8f;
                var trail = new GenericGlowParticle(
                    Projectile.Center,
                    -Projectile.velocity * 0.1f,
                    trailColor,
                    0.25f, 15, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // Occasional prismatic sparkle
            if (Main.rand.NextBool(8))
            {
                Color sparkleColor = Main.hslToRgb(Main.rand.NextFloat(), 0.8f, 0.8f);
                CustomParticles.PrismaticSparkle(Projectile.Center, sparkleColor, 0.2f);
            }
            
            // ☁EMUSICAL NOTATION - Swan Lake graceful melody
            if (Main.rand.NextBool(8))
            {
                float hue = (Main.GameUpdateCount * 0.01f + Main.rand.NextFloat()) % 1f;
                Color noteColor = Main.hslToRgb(hue, 0.8f, 0.9f);
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.3f, 30);
            }
            
            // Light
            Color lightColor = isWhite ? Color.White : new Color(50, 50, 60);
            Lighting.AddLight(Projectile.Center, lightColor.ToVector3() * 0.5f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;
            
            Color baseColor = isWhite ? Color.White : new Color(30, 30, 40);
            
            // Draw trail
            for (int i = Projectile.oldPos.Length - 1; i >= 0; i--)
            {
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                float progress = (float)i / Projectile.oldPos.Length;
                Color trailColor = baseColor * (1f - progress) * 0.5f;
                float trailScale = 0.5f * (1f - progress * 0.5f);
                
                spriteBatch.Draw(texture, trailPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
            }
            
            // Draw glow layers
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            // Outer glow
            spriteBatch.Draw(texture, drawPos, null, baseColor * 0.4f, Projectile.rotation, origin, 0.8f, SpriteEffects.None, 0f);
            // Inner glow
            spriteBatch.Draw(texture, drawPos, null, baseColor * 0.6f, Projectile.rotation, origin, 0.5f, SpriteEffects.None, 0f);
            // Core
            spriteBatch.Draw(texture, drawPos, null, Color.White * 0.8f, Projectile.rotation, origin, 0.25f, SpriteEffects.None, 0f);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // Small feather burst
            for (int i = 0; i < 6; i++)
            {
                Color burstColor = isWhite ? Color.White : new Color(30, 30, 40);
                Vector2 burstVel = Main.rand.NextVector2Circular(3f, 3f);
                var glow = new GenericGlowParticle(Projectile.Center, burstVel, burstColor * 0.6f, 0.2f, 12, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            CustomParticles.GenericFlare(Projectile.Center, isWhite ? Color.White : new Color(50, 50, 60), 0.4f, 12);
            
            // ☁EMUSICAL FINALE - Feathered symphony
            float hue = (Main.GameUpdateCount * 0.02f) % 1f;
            Color finaleColor = Main.hslToRgb(hue, 0.9f, 0.85f);
            ThemedParticles.MusicNoteBurst(Projectile.Center, finaleColor, 4, 3f);
        }
    }

    /// <summary>
    /// Sound wave projectile for Swan's Lament attack.
    /// Expands outward from spawn point.
    /// </summary>
    public class PrimaSoundWave : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/GlowingHalo4";
        
        private float radius => Projectile.ai[0] + Projectile.localAI[0];
        
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
        }

        public override void AI()
        {
            // Expand the wave
            Projectile.localAI[0] += 8f;
            
            // Update hitbox based on radius
            Projectile.width = (int)(radius * 2);
            Projectile.height = (int)(radius * 2);
            Projectile.Center = Projectile.position + new Vector2(radius, radius);
            
            // Ring particles
            if (Main.GameUpdateCount % 2 == 0)
            {
                int particleCount = (int)(radius * 0.1f);
                for (int i = 0; i < particleCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / particleCount;
                    Vector2 pos = Projectile.Center + angle.ToRotationVector2() * radius;
                    
                    Color particleColor = Main.rand.NextBool() ? Color.White * 0.6f : Main.hslToRgb((Main.GameUpdateCount * 0.01f + i * 0.05f) % 1f, 0.8f, 0.8f) * 0.6f;
                    var glow = new GenericGlowParticle(pos, Vector2.Zero, particleColor, 0.2f, 8, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
                
                // ☁EMUSICAL NOTATION - Swan Lake graceful melody on sound wave
                if (Main.rand.NextBool(4))
                {
                    float noteAngle = MathHelper.TwoPi * Main.rand.NextFloat();
                    Vector2 notePos = Projectile.Center + noteAngle.ToRotationVector2() * radius;
                    float hue = (Main.GameUpdateCount * 0.01f + Main.rand.NextFloat()) % 1f;
                    Color noteColor = Main.hslToRgb(hue, 0.8f, 0.9f);
                    Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1f);
                    ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.35f, 35);
                }
            }
            
            // Kill if too large
            if (radius > 500f)
            {
                Projectile.Kill();
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Ring collision - hits in a ring shape
            Vector2 targetCenter = targetHitbox.Center.ToVector2();
            float distanceToTarget = Vector2.Distance(Projectile.Center, targetCenter);
            
            // Hit if target is within the ring (radius ± 20)
            return distanceToTarget >= radius - 25f && distanceToTarget <= radius + 25f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Draw as an expanding ring
            SpriteBatch spriteBatch = Main.spriteBatch;
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 center = Projectile.Center - Main.screenPosition;
            
            // Draw ring of particles
            int segments = (int)(radius * 0.15f) + 8;
            for (int i = 0; i < segments; i++)
            {
                float angle = MathHelper.TwoPi * i / segments;
                Vector2 pos = center + angle.ToRotationVector2() * radius;
                
                float hue = (Main.GameUpdateCount * 0.01f + i * (1f / segments)) % 1f;
                Color segmentColor = Main.hslToRgb(hue, 0.7f, 0.75f) * 0.6f;
                
                float scale = 0.3f + (float)Math.Sin(angle * 4f + Main.GameUpdateCount * 0.1f) * 0.1f;
                spriteBatch.Draw(texture, pos, null, segmentColor, angle, texture.Size() / 2f, scale, SpriteEffects.None, 0f);
            }
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
    }

    /// <summary>
    /// Shadow clone for Pas de Deux attack.
    /// Mirrors the Prima's movements and attacks.
    /// </summary>
    public class ShatteredPrimaShadow : ModProjectile
    {
        public override string Texture => "Terraria/Images/NPC_" + NPCID.Harpy;
        
        private int parentNPC => (int)Projectile.ai[0];
        private float shadowTimer = 0f;
        
        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 90;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600; // 10 seconds max
            Projectile.alpha = 100;
        }

        public override void AI()
        {
            // Check if parent exists
            if (parentNPC < 0 || parentNPC >= Main.maxNPCs || !Main.npc[parentNPC].active)
            {
                Projectile.Kill();
                return;
            }
            
            NPC parent = Main.npc[parentNPC];
            shadowTimer += 0.05f;
            
            // Mirror position relative to player
            Player target = Main.player[parent.target];
            if (!target.active || target.dead)
            {
                Projectile.Kill();
                return;
            }
            
            // Position opposite the parent, relative to target
            Vector2 parentOffset = parent.Center - target.Center;
            Vector2 idealPos = target.Center - parentOffset; // Mirror position
            
            // Smooth movement to mirror position
            Projectile.velocity = (idealPos - Projectile.Center) * 0.08f;
            
            // Face target
            Projectile.direction = (target.Center.X - Projectile.Center.X) > 0 ? 1 : -1;
            Projectile.spriteDirection = Projectile.direction;
            
            // Attack synchronized with parent (every 60 frames)
            if ((int)(shadowTimer * 20f) % 60 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                Vector2 velocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 10f;
                
                Projectile.NewProjectile(
                    Projectile.GetSource_FromAI(),
                    Projectile.Center,
                    velocity,
                    ModContent.ProjectileType<PrimaFeatherBlade>(),
                    Projectile.damage,
                    2f,
                    Main.myPlayer,
                    0 // Black blade from shadow
                );
                
                CustomParticles.GenericFlare(Projectile.Center, new Color(30, 30, 40), 0.6f, 15);
            }
            
            // Shadow particles
            if (Main.rand.NextBool(4))
            {
                Vector2 offset = Main.rand.NextVector2Circular(30f, 40f);
                var shadow = new GenericGlowParticle(
                    Projectile.Center + offset,
                    Vector2.Zero,
                    new Color(20, 20, 30) * 0.5f,
                    0.3f, 15, true);
                MagnumParticleHandler.SpawnParticle(shadow);
            }
            
            // ☁EMUSICAL NOTATION - Swan Lake graceful melody (shadowy variant)
            if (Main.rand.NextBool(10))
            {
                float hue = (Main.GameUpdateCount * 0.01f + Main.rand.NextFloat()) % 1f;
                Color noteColor = Main.hslToRgb(hue, 0.6f, 0.7f) * 0.7f;
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.3f, 30);
            }
            
            // Light (darker than Prima)
            Lighting.AddLight(Projectile.Center, 0.2f, 0.2f, 0.3f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
            
            // Draw shadowy afterimages
            for (int i = 0; i < 5; i++)
            {
                float offset = i * 3f;
                Vector2 afterimagePos = drawPos + new Vector2(
                    (float)Math.Sin(shadowTimer * 2f + i) * offset,
                    (float)Math.Cos(shadowTimer * 2f + i) * offset);
                
                Color afterimageColor = new Color(20, 20, 35) * (0.3f / (i + 1));
                
                spriteBatch.Draw(texture, afterimagePos, null, afterimageColor,
                    Projectile.rotation, origin, Projectile.scale * (1f + i * 0.05f),
                    Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            }
            
            // Main shadow sprite
            Color shadowColor = new Color(30, 30, 45) * 0.8f;
            spriteBatch.Draw(texture, drawPos, null, shadowColor,
                Projectile.rotation, origin, Projectile.scale,
                Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // Shadow dissipation
            for (int i = 0; i < 20; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(5f, 5f);
                var shadow = new GenericGlowParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(30f, 40f),
                    velocity,
                    new Color(25, 25, 35) * 0.6f,
                    0.35f, 25, true);
                MagnumParticleHandler.SpawnParticle(shadow);
            }
            
            CustomParticles.HaloRing(Projectile.Center, new Color(30, 30, 40), 0.6f, 20);
            
            // ☁EMUSICAL FINALE - Feathered symphony (shadowy variant)
            float hue = (Main.GameUpdateCount * 0.02f) % 1f;
            Color finaleColor = Main.hslToRgb(hue, 0.7f, 0.75f) * 0.8f;
            ThemedParticles.MusicNoteBurst(Projectile.Center, finaleColor, 4, 3f);
        }
    }

    #endregion
}
