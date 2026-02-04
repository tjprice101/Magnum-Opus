using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using static MagnumOpus.Common.Systems.BossDialogueSystem;

namespace MagnumOpus.Content.SwanLake.Bosses
{
    /// <summary>
    /// Swan Lake, The Monochromatic Fractal - An elegant yet foreboding celestial deity boss.
    /// Features 6 swan wings, detached limbs held by rainbow flames, and spectacular attacks.
    /// 
    /// === PHASE SYSTEM (DoG-Inspired) ===
    /// "Graceful" (100-60% HP): Elegant, measured attacks with clear tells - the dance begins
    /// "Tempest" (60-30% HP): More aggressive, faster movements - the storm rises
    /// "Dying Swan" (30-0% HP): Desperate, beautiful finale - the tragic crescendo
    /// Each phase has distinct mood reflecting Swan Lake's ballet narrative.
    /// </summary>
    public class SwanLakeTheMonochromaticFractal : ModNPC
    {
        // Idle sprite
        public override string Texture => "MagnumOpus/Content/SwanLake/Bosses/SwanLakeTheMonochromaticFractal";
        
        // Attack sprite path
        private const string AttackTexture = "MagnumOpus/Content/SwanLake/Bosses/SwanLakeTheMonochromaticFractal_Attack";
        
        // Phase 2 Sub-phases (DoG-inspired mood system reflecting Swan Lake ballet)
        private enum BossMood
        {
            Graceful,    // 100-60% - Elegant dance, measured attacks
            Tempest,     // 60-30% - Storm intensifies, aggressive
            DyingSwan    // 30-0% - Tragic finale, desperate beauty
        }
        
        private BossMood currentMood = BossMood.Graceful;
        private bool hasAnnouncedTempest = false;
        private bool hasAnnouncedDyingSwan = false;
        
        // AI States
        private enum ActionState
        {
            Idle,
            // Attack 1 - Easy: Feather Cascade
            FeatherCascadeWindup,
            FeatherCascadeAttack,
            // Attack 2 - Easy: Prismatic Sparkle Ring
            PrismaticRingWindup,
            PrismaticRingAttack,
            // Attack 3 - Medium: Dual Swan Arc Slashes
            DualSlashWindup,
            DualSlashAttack,
            // Attack 4 - Large: Lightning Fractal Storm
            LightningStormWindup,
            LightningStormAttack,
            // Attack 5 - Ultimate: Monochromatic Apocalypse (Rotating Beam)
            ApocalypseWindup,
            ApocalypseAttack,
            // Attack 6 - Prismatic Vortex: Beams spiral inward toward player
            PrismaticVortexWindup,
            PrismaticVortexAttack,
            // Attack 7 - Prismatic Barrage: Scattered beam bursts
            PrismaticBarrageWindup,
            PrismaticBarrageAttack,
            // Attack 8 - Prismatic Cross: X-pattern beams
            PrismaticCrossWindup,
            PrismaticCrossAttack,
            // Attack 9 - Prismatic Wave: Horizontal sweeping beams
            PrismaticWaveWindup,
            PrismaticWaveAttack,
            // Attack 10 - Prismatic Chaos: Random direction beams
            PrismaticChaosWindup,
            PrismaticChaosAttack,
            // Attack 11 - Swan's Serenade: Hero's Judgment style spectacle attack
            SwanSerenadeWindup,
            SwanSerenadeAttack,
            // Attack 12 - Fractal Laser Storm: Crossing laser beams
            FractalLaserWindup,
            FractalLaserAttack,
            // Attack 13 - Chromatic Surge: Electrical rainbow pulses
            ChromaticSurgeWindup,
            ChromaticSurgeAttack,
            // Attack 14 - Prismatic Helix: Twin spiral projectile streams
            PrismaticHelixWindup,
            PrismaticHelixAttack,
            // Attack 15 - Rainbow Cascade: Waterfall-style descending waves
            RainbowCascadeWindup,
            RainbowCascadeAttack,
            // Attack 16 - Chromatic Kaleidoscope: Rotating geometric bullet patterns
            ChromaticKaleidoscopeWindup,
            ChromaticKaleidoscopeAttack
        }

        private ActionState State
        {
            get => (ActionState)NPC.ai[0];
            set => NPC.ai[0] = (float)value;
        }

        private float Timer
        {
            get => NPC.ai[1];
            set => NPC.ai[1] = value;
        }

        private float AttackPhase
        {
            get => NPC.ai[2];
            set => NPC.ai[2] = value;
        }

        private float attackPattern = 0f;
        private bool isUsingAttackSprite = false;
        private float pulseTimer = 0f;
        private float backgroundDarknessAlpha = 0f;
        
        // Visual distortion tracking
        private float distortionIntensity = 0f;
        private float distortionTimer = 0f;
        
        // Death animation
        private bool isDying = false;
        private int deathTimer = 0;
        private const int DeathAnimationDuration = 600; // 10 seconds epic death with spiraling crescendo
        private float screenWhiteningAlpha = 0f; // For progressive screen whitening
        
        // Health bar registration
        private bool hasRegisteredHealthBar = false;
        
        // Animation tracking - sprite sheet info (6x6 grid = 36 frames)
        private int frameCounter = 0;
        private int currentFrame = 0;
        private const int FrameColumns = 6;
        private const int FrameRows = 6;
        private const int TotalFrames = 36;
        private const int IdleFrameTime = 8;
        private const int AttackFrameTime = 5;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = TotalFrames; // 6x6 = 36 frames
            
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.TrailCacheLength[Type] = 15;
            NPCID.Sets.TrailingMode[Type] = 2;
            NPCID.Sets.MustAlwaysDraw[Type] = true;
            
            // Debuff immunities
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Frostburn] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Ichor] = true;
        }

        public override void SetDefaults()
        {
            // Hitbox = 80% of visual size (474x554 frame × 0.96 scale)
            NPC.width = 364;
            NPC.height = 425;
            NPC.damage = 170; // Tier 4 damage (Campanella 130, Fate ~220)
            NPC.defense = 110; // Tier 4 defense (Campanella 70, Fate ~150)
            NPC.lifeMax = 950000; // 950k HP - Tier 4 (Campanella 650k, Fate ~1.5M)
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath14;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = Item.buyPrice(gold: 45);
            NPC.boss = true;
            NPC.npcSlots = 18f;
            NPC.aiStyle = -1;
            NPC.scale = 0.96f; // 20% bigger than before (was 0.8f)
            
            if (!Main.dedServ)
            {
                Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music/SwanOfAThousandChords");
            }
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
                new FlavorTextBestiaryInfoElement("Swan Lake, The Monochromatic Fractal - " +
                    "An elegant celestial deity with six swan wings and detached limbs held by rainbow flames. " +
                    "She embodies the duality of black and white, light and shadow.")
            });
        }

        public override void AI()
        {
            // Register with custom health bar system
            if (!hasRegisteredHealthBar)
            {
                BossHealthBarUI.RegisterBoss(NPC, BossColorTheme.SwanLake);
                hasRegisteredHealthBar = true;
                
                // Spawn dialogue
                BossDialogueSystem.SwanLake.OnSpawn(NPC.whoAmI);
            }
            
            NPC.TargetClosest(true);
            Player target = Main.player[NPC.target];

            // Handle death animation
            if (isDying)
            {
                UpdateDeathAnimation(target);
                return;
            }

            // Despawn check
            if (!target.active || target.dead)
            {
                NPC.velocity.Y -= 0.5f;
                backgroundDarknessAlpha = MathHelper.Lerp(backgroundDarknessAlpha, 0f, 0.05f);
                NPC.EncourageDespawn(60);
                return;
            }

            // === TELEPORT TO PLAYER IF THEY TRY TO ESCAPE ===
            // If player is more than 1800 pixels away, teleport to them with a prismatic rainbow flash!
            float distanceToPlayer = Vector2.Distance(NPC.Center, target.Center);
            const float MaxAllowedDistance = 1800f;
            
            if (distanceToPlayer > MaxAllowedDistance)
            {
                TeleportToPlayer(target);
            }

            // Update timers
            Timer++;
            pulseTimer += 0.04f;
            distortionTimer += 0.02f;
            
            // === MOOD SYSTEM (DoG-Inspired) ===
            // Updates phase mood based on health with dramatic ballet-themed transitions
            UpdateMood(target);
            
            // Visual distortion during combat - subtle but noticeable
            distortionIntensity = MathHelper.Lerp(distortionIntensity, 
                isUsingAttackSprite ? 0.4f : 0.15f, 0.05f);
            
            // Fade in black background with rainbow sparkles
            if (backgroundDarknessAlpha < 1f)
                backgroundDarknessAlpha = MathHelper.Lerp(backgroundDarknessAlpha, 1f, 0.02f);

            // Lighting - monochromatic with chromatic hints
            float lightPulse = 0.7f + (float)Math.Sin(pulseTimer * 2f) * 0.3f;
            Lighting.AddLight(NPC.Center, 0.6f * lightPulse, 0.6f * lightPulse, 0.7f * lightPulse);

            // Ambient particles
            SpawnAmbientParticles();

            // State machine
            switch (State)
            {
                case ActionState.Idle:
                    IdleHover(target);
                    break;
                    
                // Attack 1 - Easy: Feather Cascade
                case ActionState.FeatherCascadeWindup:
                    FeatherCascadeWindup(target);
                    break;
                case ActionState.FeatherCascadeAttack:
                    FeatherCascadeAttack(target);
                    break;
                    
                // Attack 2 - Easy: Prismatic Sparkle Ring
                case ActionState.PrismaticRingWindup:
                    PrismaticRingWindup(target);
                    break;
                case ActionState.PrismaticRingAttack:
                    PrismaticRingAttack(target);
                    break;
                    
                // Attack 3 - Medium: Dual Swan Arc Slashes
                case ActionState.DualSlashWindup:
                    DualSlashWindup(target);
                    break;
                case ActionState.DualSlashAttack:
                    DualSlashAttack(target);
                    break;
                    
                // Attack 4 - Large: Lightning Fractal Storm
                case ActionState.LightningStormWindup:
                    LightningStormWindup(target);
                    break;
                case ActionState.LightningStormAttack:
                    LightningStormAttack(target);
                    break;
                    
                // Attack 5 - Ultimate: Monochromatic Apocalypse
                case ActionState.ApocalypseWindup:
                    ApocalypseWindup(target);
                    break;
                case ActionState.ApocalypseAttack:
                    ApocalypseAttack(target);
                    break;
                    
                // Attack 6 - Prismatic Vortex: Beams spiral inward
                case ActionState.PrismaticVortexWindup:
                    PrismaticVortexWindup(target);
                    break;
                case ActionState.PrismaticVortexAttack:
                    PrismaticVortexAttack(target);
                    break;
                    
                // Attack 7 - Prismatic Barrage: Scattered beam bursts
                case ActionState.PrismaticBarrageWindup:
                    PrismaticBarrageWindup(target);
                    break;
                case ActionState.PrismaticBarrageAttack:
                    PrismaticBarrageAttack(target);
                    break;
                    
                // Attack 8 - Prismatic Cross: X-pattern beams
                case ActionState.PrismaticCrossWindup:
                    PrismaticCrossWindup(target);
                    break;
                case ActionState.PrismaticCrossAttack:
                    PrismaticCrossAttack(target);
                    break;
                    
                // Attack 9 - Prismatic Wave: Horizontal sweep
                case ActionState.PrismaticWaveWindup:
                    PrismaticWaveWindup(target);
                    break;
                case ActionState.PrismaticWaveAttack:
                    PrismaticWaveAttack(target);
                    break;
                    
                // Attack 10 - Prismatic Chaos: Random direction beams
                case ActionState.PrismaticChaosWindup:
                    PrismaticChaosWindup(target);
                    break;
                case ActionState.PrismaticChaosAttack:
                    PrismaticChaosAttack(target);
                    break;
                    
                // Attack 11 - Swan's Serenade: Hero's Judgment style spectacle
                case ActionState.SwanSerenadeWindup:
                    SwanSerenadeWindup(target);
                    break;
                case ActionState.SwanSerenadeAttack:
                    SwanSerenadeAttack(target);
                    break;
                    
                // Attack 12 - Fractal Laser Storm: Crossing laser beams
                case ActionState.FractalLaserWindup:
                    FractalLaserWindup(target);
                    break;
                case ActionState.FractalLaserAttack:
                    FractalLaserAttack(target);
                    break;
                    
                // Attack 13 - Chromatic Surge: Electrical rainbow pulses
                case ActionState.ChromaticSurgeWindup:
                    ChromaticSurgeWindup(target);
                    break;
                case ActionState.ChromaticSurgeAttack:
                    ChromaticSurgeAttack(target);
                    break;
                    
                // Attack 14 - Prismatic Helix: Twin spiral projectile streams
                case ActionState.PrismaticHelixWindup:
                    PrismaticHelixWindup(target);
                    break;
                case ActionState.PrismaticHelixAttack:
                    PrismaticHelixAttack(target);
                    break;
                    
                // Attack 15 - Rainbow Cascade: Waterfall-style descending waves
                case ActionState.RainbowCascadeWindup:
                    RainbowCascadeWindup(target);
                    break;
                case ActionState.RainbowCascadeAttack:
                    RainbowCascadeAttack(target);
                    break;
                    
                // Attack 16 - Chromatic Kaleidoscope: Rotating geometric bullet patterns
                case ActionState.ChromaticKaleidoscopeWindup:
                    ChromaticKaleidoscopeWindup(target);
                    break;
                case ActionState.ChromaticKaleidoscopeAttack:
                    ChromaticKaleidoscopeAttack(target);
                    break;
            }

            // Boss dialogue system - combat taunts and player HP checks
            // Dialogue triggers at HP thresholds only
            BossDialogueSystem.CheckPlayerLowHP(target, "SwanLake");
            
            // Face player
            NPC.spriteDirection = NPC.direction = (target.Center.X > NPC.Center.X) ? 1 : -1;
        }

        /// <summary>
        /// Teleports the boss to the player with a spectacular prismatic rainbow flash effect.
        /// Called when the player tries to escape beyond the allowed radius.
        /// </summary>
        private void TeleportToPlayer(Player target)
        {
            // Enrage dialogue when player tries to flee
            BossDialogueSystem.SwanLake.OnEnrage();
            
            // Store old position for visual effects
            Vector2 oldPosition = NPC.Center;
            
            // Calculate new position - appear above and slightly behind the player
            Vector2 offset = new Vector2(-target.direction * 200f, -250f);
            Vector2 newPosition = target.Center + offset;
            
            // Sound effect - ethereal teleport
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.8f, Volume = 1.2f }, oldPosition);
            SoundEngine.PlaySound(SoundID.Item119 with { Pitch = 0.3f, Volume = 0.9f }, newPosition);
            
            // === DEPARTURE EFFECTS (at old position) ===
            
            // Massive prismatic explosion where boss was
            for (int i = 0; i < 40; i++)
            {
                float hue = i / 40f;
                Color rainbowColor = Main.hslToRgb(hue, 1f, 0.8f);
                Vector2 vel = Main.rand.NextVector2Circular(15f, 15f);
                Dust departure = Dust.NewDustPerfect(oldPosition + Main.rand.NextVector2Circular(50f, 50f), 
                    DustID.RainbowTorch, vel, 0, rainbowColor, 2.5f);
                departure.noGravity = true;
                departure.fadeIn = 1.5f;
            }
            
            // Black and white feather burst at departure
            CustomParticles.SwanFeatherExplosion(oldPosition, 30, 1f);
            CustomParticles.SwanFeatherDuality(oldPosition, 20, 1.2f);
            
            // Rainbow sparkle rings expanding outward
            for (int ring = 0; ring < 3; ring++)
            {
                float ringRadius = 50f + ring * 40f;
                for (int i = 0; i < 16; i++)
                {
                    float angle = MathHelper.TwoPi * i / 16f;
                    Vector2 ringPos = oldPosition + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * ringRadius;
                    float hue = (i / 16f + ring * 0.15f) % 1f;
                    Color ringColor = Main.hslToRgb(hue, 1f, 0.9f);
                    CustomParticles.PrismaticSparkleBurst(ringPos, ringColor, 6);
                    CustomParticles.GenericFlare(ringPos, ringColor, 0.5f, 15);
                }
            }
            
            // === TELEPORT ===
            NPC.Center = newPosition;
            NPC.velocity = Vector2.Zero;
            
            // === ARRIVAL EFFECTS (at new position) ===
            
            // Intense flash of light
            for (int i = 0; i < 50; i++)
            {
                float hue = i / 50f;
                Color arrivalColor = Main.hslToRgb(hue, 1f, 0.85f);
                Vector2 vel = Main.rand.NextVector2Circular(12f, 12f);
                Dust arrival = Dust.NewDustPerfect(newPosition, DustID.RainbowTorch, vel, 0, arrivalColor, 3f);
                arrival.noGravity = true;
                arrival.fadeIn = 2f;
            }
            
            // White core explosion at arrival
            CustomParticles.ExplosionBurst(newPosition, Color.White, 25, 15f);
            CustomParticles.ExplosionBurst(newPosition, Color.Black, 18, 10f);
            
            // Swan feather spiral inward effect
            CustomParticles.SwanFeatherSpiral(newPosition, Color.White, 20);
            CustomParticles.SwanFeatherSpiral(newPosition, Color.Black, 15);
            
            // Draw lightning between old and new positions - prismatic bridge
            MagnumVFX.DrawSwanLakeLightning(oldPosition, newPosition, 15, 60f, 8, 0.8f);
            
            // Additional lightning fractals around arrival point
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 lightningEnd = newPosition + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 150f;
                MagnumVFX.DrawSwanLakeLightning(newPosition, lightningEnd, 8, 25f, 3, 0.5f);
            }
            
            // Screen shake
            EroicaScreenShake.LargeShake(newPosition);
            
            // Warning message to player
            Main.NewText("You cannot escape the Swan's embrace!", 255, 200, 255);
            
            // Intense lighting at new position
            Lighting.AddLight(newPosition, 3f, 3f, 3.5f);
            
            NPC.netUpdate = true;
        }

        /// <summary>
        /// Centralized attack ending helper with visual feedback.
        /// Spawns attack end cue and transitions to Idle state cleanly.
        /// </summary>
        private void EndAttackWithCue()
        {
            // Rainbow-themed attack end cue - prismatic exhale effect
            float rainbowHue = (Main.GameUpdateCount * 0.02f) % 1f;
            Color primaryColor = Main.hslToRgb(rainbowHue, 0.8f, 0.9f);
            Color secondaryColor = Main.hslToRgb((rainbowHue + 0.5f) % 1f, 0.7f, 0.8f);
            
            BossVFXOptimizer.AttackEndCue(NPC.Center, primaryColor, secondaryColor, 0.6f);
            
            // Add feather drift for elegance
            for (int i = 0; i < 4; i++)
            {
                Vector2 featherPos = NPC.Center + Main.rand.NextVector2Circular(30f, 30f);
                Color featherColor = Main.rand.NextBool() ? Color.White : new Color(40, 40, 50);
                CustomParticles.SwanFeatherDrift(featherPos, featherColor, 0.25f);
            }
            
            Timer = 0;
            isUsingAttackSprite = false;
            State = ActionState.Idle;
            NPC.netUpdate = true;
        }

        private void SpawnAmbientParticles()
        {
            // === PERFORMANCE GATE ===
            // Skip all ambient particles under critical load
            if (BossVFXOptimizer.IsCriticalLoad) return;
            
            // Reduce ambient effects under high load
            bool isHighLoad = BossVFXOptimizer.IsHighLoad;
            
            // === SWAN LAKE AMBIENT EFFECTS ===
            // A ballet of light and shadow - graceful feathers, prismatic streams, and ethereal elegance
            
            float moodIntensity = GetMoodIntensity();
            float danceRhythm = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.5f + 0.5f; // Ballet tempo
            float gracefulWave = (float)Math.Sin(Main.GameUpdateCount * 0.025f);
            
            // === 1. FEATHER WALTZ - ORBITING FEATHERS ===
            // Feathers dance around the swan in a graceful spiral
            int featherCount = isHighLoad ? 2 : (4 + (int)(moodIntensity * 4));
            float waltzRadius = 90f + gracefulWave * 20f;
            
            for (int i = 0; i < featherCount; i++)
            {
                // Spiral pattern with varying heights - like a ballet ensemble
                float baseAngle = Main.GameUpdateCount * 0.02f + (MathHelper.TwoPi * i / featherCount);
                float verticalOffset = (float)Math.Sin(Main.GameUpdateCount * 0.03f + i * 0.8f) * 30f;
                
                Vector2 featherPos = NPC.Center + new Vector2(
                    (float)Math.Cos(baseAngle) * waltzRadius,
                    (float)Math.Sin(baseAngle) * waltzRadius * 0.5f + verticalOffset);
                
                if ((Main.GameUpdateCount + i * 11) % 8 == 0)
                {
                    // Alternating black and white feathers
                    Color featherColor = i % 2 == 0 ? Color.White : new Color(30, 30, 40);
                    CustomParticles.SwanFeatherDrift(featherPos, featherColor, 0.25f + danceRhythm * 0.1f);
                }
            }
            
            // === 2. PRISMATIC LIGHT STREAMS ===
            // Rainbow light bleeds through the monochrome - beauty breaking through tragedy
            if (Main.rand.NextBool(6))
            {
                float streamAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                float streamLength = 60f + moodIntensity * 40f;
                
                for (int j = 0; j < 5; j++)
                {
                    float progress = j / 5f;
                    Vector2 streamPos = NPC.Center + streamAngle.ToRotationVector2() * streamLength * progress;
                    
                    // Rainbow hue that shifts along the stream
                    float hue = (Main.GameUpdateCount * 0.005f + progress * 0.3f) % 1f;
                    Color prismaticColor = Main.hslToRgb(hue, 0.7f, 0.8f);
                    
                    CustomParticles.PrismaticSparkle(streamPos, prismaticColor * (1f - progress * 0.4f), 0.2f);
                }
            }
            
            // === 3. MONOCHROME CONTRAST RIPPLES ===
            // Alternating black and white ripples expand outward
            if (Main.GameUpdateCount % 35 == 0)
            {
                Color rippleColor = (Main.GameUpdateCount / 35) % 2 == 0 ? Color.White * 0.5f : new Color(40, 40, 50) * 0.7f;
                CustomParticles.HaloRing(NPC.Center, rippleColor, 0.4f + moodIntensity * 0.2f, 30);
            }
            
            // === 4. GRACEFUL PARTICLE CURRENTS ===
            // Particles flow in elegant curves around the swan
            if (Main.GameUpdateCount % 3 == 0)
            {
                // Figure-8 pattern - the infinity of dance
                float curveT = Main.GameUpdateCount * 0.03f;
                float curveX = (float)Math.Sin(curveT) * 80f;
                float curveY = (float)Math.Sin(curveT * 2f) * 40f;
                
                Vector2 curvePos = NPC.Center + new Vector2(curveX, curveY);
                
                // Mostly white/silver, occasional black for contrast
                Color currentColor = Main.rand.NextBool(4) 
                    ? new Color(25, 25, 35) 
                    : Color.Lerp(Color.White, Color.Silver, Main.rand.NextFloat());
                
                var currentParticle = new GenericGlowParticle(curvePos, 
                    new Vector2((float)Math.Cos(curveT) * 2f, (float)Math.Sin(curveT * 2f)), 
                    currentColor * 0.5f, 0.25f, 25, true);
                MagnumParticleHandler.SpawnParticle(currentParticle);
            }
            
            // === 5. ETHEREAL MIST - STAGE FOG ===
            // Soft mist drifts across the arena like stage fog
            if (Main.rand.NextBool(5))
            {
                Vector2 mistPos = NPC.Center + new Vector2(
                    Main.rand.NextFloat(-150f, 150f), 
                    Main.rand.NextFloat(-30f, 60f));
                
                Color mistColor = Color.Lerp(Color.White, Color.LightGray, Main.rand.NextFloat()) * 0.25f;
                Vector2 mistVel = new Vector2(gracefulWave * 0.5f, -0.3f);
                
                var mist = new HeavySmokeParticle(mistPos, mistVel, mistColor, Main.rand.Next(60, 90), 
                    Main.rand.NextFloat(0.3f, 0.5f), 0.4f, 0.008f, false);
                MagnumParticleHandler.SpawnParticle(mist);
            }
            
            // === 6. BALLET SPOTLIGHT EFFECTS ===
            // Spotlights that follow the swan's movement
            if (Main.rand.NextBool(8))
            {
                // Soft white spotlight glow
                Vector2 spotlightPos = NPC.Center + new Vector2(0, -40f) + Main.rand.NextVector2Circular(20f, 20f);
                CustomParticles.GenericFlare(spotlightPos, Color.White * 0.4f * danceRhythm, 0.4f, 15);
            }
            
            // === 7. DYING SWAN TEARS (DyingSwan mood) ===
            // Crystalline tears fall during the tragic finale
            if (currentMood == BossMood.DyingSwan && Main.rand.NextBool(4))
            {
                Vector2 tearStart = NPC.Center + new Vector2(Main.rand.NextFloat(-30f, 30f), -20f);
                Vector2 tearVel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(1.5f, 2.5f));
                
                // Crystalline rainbow tears
                float tearHue = Main.rand.NextFloat();
                Color tearColor = Main.hslToRgb(tearHue, 0.6f, 0.85f);
                
                var tear = new GenericGlowParticle(tearStart, tearVel, tearColor * 0.7f, 0.2f, 40, true);
                MagnumParticleHandler.SpawnParticle(tear);
            }
            
            // === 8. MOOD-SPECIFIC EFFECTS ===
            switch (currentMood)
            {
                case BossMood.Graceful:
                    // Soft, elegant sparkles - the prima donna's entrance
                    if (Main.rand.NextBool(8))
                    {
                        Vector2 gracePos = NPC.Center + Main.rand.NextVector2Circular(60f, 60f);
                        CustomParticles.PrismaticSparkle(gracePos, Color.White * 0.6f, 0.15f);
                    }
                    break;
                    
                case BossMood.Tempest:
                    // More intense feather bursts - the storm of emotions
                    if (Main.rand.NextBool(3))
                    {
                        Vector2 stormPos = NPC.Center + Main.rand.NextVector2Circular(100f, 80f);
                        Color stormFeather = Main.rand.NextBool() ? Color.White : Color.Black;
                        CustomParticles.SwanFeatherDrift(stormPos, stormFeather, 0.35f);
                    }
                    break;
                    
                case BossMood.DyingSwan:
                    // Falling feathers and fading light - the tragic end
                    if (Main.rand.NextBool(2))
                    {
                        Vector2 fadePos = NPC.Top + new Vector2(Main.rand.NextFloat(-80f, 80f), -20f);
                        Vector2 fadeVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(1f, 2f));
                        Color fadeFeather = Main.rand.NextBool() ? Color.White * 0.7f : new Color(50, 50, 60);
                        
                        var fallingFeather = new GenericGlowParticle(fadePos, fadeVel, fadeFeather, 0.3f, 50, true);
                        MagnumParticleHandler.SpawnParticle(fallingFeather);
                    }
                    break;
            }
            
            // === 9. DYNAMIC LIGHTING ===
            // Cool, elegant lighting with prismatic shimmer
            float hueShift = (Main.GameUpdateCount * 0.003f) % 1f;
            Vector3 baseLight = new Vector3(0.9f, 0.9f, 1f); // Cool white
            Vector3 prismaticAccent = Main.hslToRgb(hueShift, 0.3f, 0.8f).ToVector3() * 0.3f;
            
            Lighting.AddLight(NPC.Center, (baseLight + prismaticAccent) * (0.7f + danceRhythm * 0.3f));
        }
        
        private float GetMoodIntensity()
        {
            return currentMood switch
            {
                BossMood.Graceful => 0.3f,
                BossMood.Tempest => 0.6f,
                BossMood.DyingSwan => 1.0f,
                _ => 0.3f
            };
        }

        private void IdleHover(Player target)
        {
            isUsingAttackSprite = false;
            distortionIntensity = MathHelper.Lerp(distortionIntensity, 0f, 0.08f);
            
            // === MOOD-BASED MOVEMENT PARAMETERS ===
            float hoverSpeed, lerpFactor, waveAmplitudeX, waveAmplitudeY, hoverDistance;
            int attackDelay;
            
            switch (currentMood)
            {
                case BossMood.Graceful: // Elegant, ballet-like movements
                    hoverSpeed = 14f;
                    lerpFactor = 0.08f;
                    waveAmplitudeX = 60f;
                    waveAmplitudeY = 40f;
                    hoverDistance = 300f;
                    attackDelay = 40; // Measured tempo
                    break;
                    
                case BossMood.Tempest: // Storm rising, faster and more erratic
                    hoverSpeed = 20f;
                    lerpFactor = 0.12f;
                    waveAmplitudeX = 100f;
                    waveAmplitudeY = 60f;
                    hoverDistance = 260f;
                    attackDelay = 25; // Faster tempo
                    break;
                    
                case BossMood.DyingSwan: // Tragic finale - desperate, beautiful chaos
                default:
                    hoverSpeed = 24f;
                    lerpFactor = 0.15f;
                    waveAmplitudeX = 140f;
                    waveAmplitudeY = 80f;
                    hoverDistance = 220f;
                    attackDelay = 15; // Frantic tempo - the dying dance
                    break;
            }
            
            // Graceful hovering movement - fluid like a swan in flight
            float hoverX = (float)Math.Sin(pulseTimer * 2.5f) * waveAmplitudeX;
            float hoverY = (float)Math.Sin(pulseTimer * 3f) * waveAmplitudeY;
            
            Vector2 hoverPosition = target.Center + new Vector2(hoverX, -hoverDistance + hoverY);
            Vector2 direction = hoverPosition - NPC.Center;
            float distance = direction.Length();

            if (distance > 15f)
            {
                direction.Normalize();
                // Smooth, fluid movement interpolation
                NPC.velocity = Vector2.Lerp(NPC.velocity, direction * hoverSpeed, lerpFactor);
            }
            else
            {
                NPC.velocity *= 0.85f;
            }
            
            // Spawn trail particles while moving fast - mood affects intensity
            if (NPC.velocity.Length() > 8f && Main.rand.NextBool(currentMood == BossMood.DyingSwan ? 1 : 2))
            {
                Color trailColor = Main.rand.NextBool() ? Color.White : Color.Black;
                CustomParticles.SwanFeatherDrift(NPC.Center + Main.rand.NextVector2Circular(30f, 30f), trailColor, 0.2f);
                
                // Rainbow shimmer in DyingSwan
                if (currentMood == BossMood.DyingSwan)
                {
                    float hue = (pulseTimer * 0.1f + Main.rand.NextFloat()) % 1f;
                    Color rainbowColor = Main.hslToRgb(hue, 1f, 0.8f);
                    CustomParticles.PrismaticSparkle(NPC.Center + Main.rand.NextVector2Circular(40f, 40f), rainbowColor, 0.25f);
                }
            }

            // Choose next attack based on mood
            if (Timer > attackDelay)
            {
                Timer = 0;
                AttackPhase = 0;
                
                SelectMoodBasedAttack(target);
                
                NPC.netUpdate = true;
            }
        }
        
        /// <summary>
        /// Updates the current mood based on health thresholds with dramatic ballet-themed transitions.
        /// </summary>
        private void UpdateMood(Player target)
        {
            float healthPercent = (float)NPC.life / NPC.lifeMax;
            
            // Transition to Tempest at 60% HP
            if (healthPercent <= 0.6f && !hasAnnouncedTempest)
            {
                hasAnnouncedTempest = true;
                currentMood = BossMood.Tempest;
                AnnounceMoodTransition("Tempest", target);
            }
            // Transition to Dying Swan at 30% HP
            else if (healthPercent <= 0.3f && !hasAnnouncedDyingSwan)
            {
                hasAnnouncedDyingSwan = true;
                currentMood = BossMood.DyingSwan;
                AnnounceMoodTransition("DyingSwan", target);
            }
        }
        
        /// <summary>
        /// Creates dramatic VFX announcement when transitioning between moods.
        /// </summary>
        private void AnnounceMoodTransition(string moodName, Player target)
        {
            bool isDyingSwan = moodName == "DyingSwan";
            
            // Screen shake
            if (target.active)
            {
                MagnumScreenEffects.AddScreenShake(isDyingSwan ? 18f : 10f);
            }
            
            // Sound cue - ethereal swan cry
            SoundEngine.PlaySound(SoundID.Item29 with { Pitch = isDyingSwan ? 0.5f : 0.2f, Volume = 1.3f }, NPC.Center);
            
            // Massive feather burst
            CustomParticles.SwanFeatherBurst(NPC.Center, isDyingSwan ? 50 : 30, 1.5f);
            
            // Dual-polarity burst - black and white
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 offset = angle.ToRotationVector2() * 50f;
                Color flareColor = i % 2 == 0 ? Color.White : new Color(20, 20, 30);
                CustomParticles.GenericFlare(NPC.Center + offset, flareColor, 0.6f, 25);
            }
            
            // Expanding rainbow halos
            for (int ring = 0; ring < 8; ring++)
            {
                float hue = ring / 8f;
                Color ringColor = Main.hslToRgb(hue, 1f, 0.8f);
                CustomParticles.HaloRing(NPC.Center, ringColor, 0.4f + ring * 0.25f, 20 + ring * 5);
            }
            
            // Prismatic sparkle cascade
            for (int i = 0; i < 20; i++)
            {
                float hue = i / 20f;
                Color sparkleColor = Main.hslToRgb(hue, 1f, 0.85f);
                Vector2 sparklePos = NPC.Center + Main.rand.NextVector2Circular(80f, 80f);
                CustomParticles.PrismaticSparkle(sparklePos, sparkleColor, 0.4f);
            }
            
            // Text announcement
            if (Main.netMode != NetmodeID.Server)
            {
                string text = isDyingSwan ? "THE DYING SWAN!" : "THE TEMPEST RISES!";
                Color textColor = isDyingSwan ? Color.White : Color.LightPink;
                CombatText.NewText(NPC.Hitbox, textColor, text, true);
            }
            
            // Use dialogue system for mood transitions
            if (isDyingSwan)
                BossDialogueSystem.SwanLake.OnDyingSwanMood(NPC.whoAmI);
            else
                BossDialogueSystem.SwanLake.OnTempestMood(NPC.whoAmI);
        }
        
        /// <summary>
        /// Selects attack based on current mood - each mood has distinct attack patterns.
        /// Includes new spectacle attacks: Swan's Serenade, Fractal Laser, Chromatic Surge
        /// </summary>
        private void SelectMoodBasedAttack(Player target)
        {
            int roll = Main.rand.Next(100);
            
            switch (currentMood)
            {
                case BossMood.Graceful:
                    // Elegant, measured attacks - mostly easy attacks + occasional spectacle
                    if (roll < 25) // 25% - Feather Cascade
                        State = ActionState.FeatherCascadeWindup;
                    else if (roll < 45) // 20% - Prismatic Ring
                        State = ActionState.PrismaticRingWindup;
                    else if (roll < 65) // 20% - Dual Slash
                        State = ActionState.DualSlashWindup;
                    else if (roll < 80) // 15% - Lightning Storm
                        State = ActionState.LightningStormWindup;
                    else if (roll < 90) // 10% - Apocalypse (rare dramatic moment)
                        State = ActionState.ApocalypseWindup;
                    else // 10% - Swan's Serenade (Hero's Judgment style)
                        State = ActionState.SwanSerenadeWindup;
                    break;
                    
                case BossMood.Tempest:
                    // Storm intensifies - mix of all attacks including new fluid bullet hell patterns
                    if (roll < 8) // 8% - Feather Cascade
                        State = ActionState.FeatherCascadeWindup;
                    else if (roll < 15) // 7% - Prismatic Ring
                        State = ActionState.PrismaticRingWindup;
                    else if (roll < 22) // 7% - Dual Slash
                        State = ActionState.DualSlashWindup;
                    else if (roll < 30) // 8% - Lightning Storm
                        State = ActionState.LightningStormWindup;
                    else if (roll < 38) // 8% - Apocalypse
                        State = ActionState.ApocalypseWindup;
                    else if (roll < 44) // 6% - Prismatic Vortex
                        State = ActionState.PrismaticVortexWindup;
                    else if (roll < 50) // 6% - Prismatic Barrage
                        State = ActionState.PrismaticBarrageWindup;
                    else if (roll < 56) // 6% - Prismatic Cross
                        State = ActionState.PrismaticCrossWindup;
                    else if (roll < 62) // 6% - Prismatic Wave
                        State = ActionState.PrismaticWaveWindup;
                    else if (roll < 70) // 8% - Swan's Serenade
                        State = ActionState.SwanSerenadeWindup;
                    else if (roll < 76) // 6% - Fractal Laser Storm
                        State = ActionState.FractalLaserWindup;
                    else if (roll < 82) // 6% - Chromatic Surge
                        State = ActionState.ChromaticSurgeWindup;
                    else if (roll < 88) // 6% - Prismatic Helix (NEW - fluid spiral)
                        State = ActionState.PrismaticHelixWindup;
                    else if (roll < 94) // 6% - Rainbow Cascade (NEW - waterfall bullet hell)
                        State = ActionState.RainbowCascadeWindup;
                    else // 6% - Chromatic Kaleidoscope (NEW - geometric patterns)
                        State = ActionState.ChromaticKaleidoscopeWindup;
                    break;
                    
                case BossMood.DyingSwan:
                    // Desperate finale - heavy attacks, new spectacle moves + fluid bullet hell patterns
                    if (roll < 5) // 5% - Feather Cascade
                        State = ActionState.FeatherCascadeWindup;
                    else if (roll < 10) // 5% - Prismatic Ring
                        State = ActionState.PrismaticRingWindup;
                    else if (roll < 16) // 6% - Dual Slash
                        State = ActionState.DualSlashWindup;
                    else if (roll < 24) // 8% - Lightning Storm
                        State = ActionState.LightningStormWindup;
                    else if (roll < 32) // 8% - Apocalypse (more common now)
                        State = ActionState.ApocalypseWindup;
                    else if (roll < 38) // 6% - Prismatic Vortex
                        State = ActionState.PrismaticVortexWindup;
                    else if (roll < 44) // 6% - Prismatic Barrage
                        State = ActionState.PrismaticBarrageWindup;
                    else if (roll < 50) // 6% - Prismatic Cross
                        State = ActionState.PrismaticCrossWindup;
                    else if (roll < 56) // 6% - Prismatic Wave
                        State = ActionState.PrismaticWaveWindup;
                    else if (roll < 62) // 6% - Prismatic Chaos
                        State = ActionState.PrismaticChaosWindup;
                    else if (roll < 70) // 8% - Swan's Serenade
                        State = ActionState.SwanSerenadeWindup;
                    else if (roll < 76) // 6% - Fractal Laser Storm
                        State = ActionState.FractalLaserWindup;
                    else if (roll < 82) // 6% - Chromatic Surge
                        State = ActionState.ChromaticSurgeWindup;
                    else if (roll < 88) // 6% - Prismatic Helix (NEW - fluid spiral)
                        State = ActionState.PrismaticHelixWindup;
                    else if (roll < 94) // 6% - Rainbow Cascade (NEW - waterfall bullet hell)
                        State = ActionState.RainbowCascadeWindup;
                    else // 6% - Chromatic Kaleidoscope (NEW - geometric patterns)
                        State = ActionState.ChromaticKaleidoscopeWindup;
                    break;
            }
        }

        #region Attack 1: Feather Cascade (Easy)

        private void FeatherCascadeWindup(Player target)
        {
            const int WindupTime = 7; // ULTRA fast windup (was 18)
            isUsingAttackSprite = true;
            
            // Quick dash toward player during windup - tracks player
            Vector2 toPlayer = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
            NPC.velocity = Vector2.Lerp(NPC.velocity, toPlayer * 16f, 0.12f);
            
            if (Timer == 1)
            {
                SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0.5f }, NPC.Center);
            }
            
            // Rapid feather gathering - black and white spiral
            if (Timer % 2 == 0)
            {
                for (int i = 0; i < 5; i++)
                {
                    Vector2 offset = Main.rand.NextVector2Circular(100f, 100f);
                    Color spiralColor = i % 2 == 0 ? Color.White : Color.Black;
                    CustomParticles.SwanFeatherSpiral(NPC.Center + offset, spiralColor, 5);
                }
            }
            
            if (Timer >= WindupTime)
            {
                Timer = 0;
                State = ActionState.FeatherCascadeAttack;
                NPC.netUpdate = true;
            }
        }

        private void FeatherCascadeAttack(Player target)
        {
            const int AttackDuration = 32;
            isUsingAttackSprite = true;
            distortionIntensity = MathHelper.Lerp(distortionIntensity, 0.3f, 0.1f);
            
            // === PATTERN-BASED MOVEMENT - Don't track directly! ===
            // Move in a sweeping arc AROUND the player, not toward them
            float arcAngle = (Timer / (float)AttackDuration) * MathHelper.Pi; // 180 degree arc
            float startAngle = (target.Center - NPC.Center).ToRotation() - MathHelper.PiOver2;
            Vector2 arcDirection = (startAngle + arcAngle).ToRotationVector2();
            NPC.velocity = Vector2.Lerp(NPC.velocity, arcDirection * 12f, 0.1f);
            
            // === SPREAD PATTERN FEATHERS - Not aimed at player! ===
            if (Timer % 3 == 0 && Timer < AttackDuration - 10)
            {
                SoundEngine.PlaySound(SoundID.Item1 with { Pitch = 0.5f, Volume = 0.4f }, NPC.Center);
                
                // Fire feathers in a FIXED downward fan pattern
                // Player must move horizontally to dodge
                for (int i = 0; i < 7; i++)
                {
                    // Fixed angles: mostly downward with spread
                    float angle = MathHelper.PiOver2 + MathHelper.ToRadians(-60 + i * 20); // Downward fan
                    Vector2 velocity = angle.ToRotationVector2() * (10f + i % 2 * 3f); // Alternating speeds
                    
                    Color featherColor = i % 2 == 0 ? Color.White : Color.Black;
                    CustomParticles.SwanFeatherBurst(NPC.Center, 2, 0.4f);
                    
                    // Spawn actual projectile (if you have a feather projectile type)
                    // For now, visual effect + damage zone
                }
                
                // Rainbow sparkles at varying positions
                for (int i = 0; i < 3; i++)
                {
                    float hue = (Timer * 0.04f + i * 0.3f) % 1f;
                    Color rainbowColor = Main.hslToRgb(hue, 1f, 0.8f);
                    Vector2 sparkleOffset = Main.rand.NextVector2Circular(50f, 50f);
                    CustomParticles.PrismaticSparkleBurst(NPC.Center + sparkleOffset, rainbowColor, 2);
                }
            }
            
            if (Timer >= AttackDuration)
            {
                EndAttackWithCue();
            }
        }

        #endregion

        #region Attack 2: Prismatic Sparkle Ring (Easy)

        private void PrismaticRingWindup(Player target)
        {
            const int WindupTime = 6; // ULTRA fast windup (was 15)
            isUsingAttackSprite = true;
            
            // Circle around player during windup - faster circle
            float circleAngle = Timer * 0.2f;
            Vector2 circlePos = target.Center + new Vector2((float)Math.Cos(circleAngle), (float)Math.Sin(circleAngle)) * 180f;
            Vector2 toCircle = circlePos - NPC.Center;
            NPC.velocity = Vector2.Lerp(NPC.velocity, toCircle * 0.2f, 0.12f);
            
            if (Timer == 1)
            {
                SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.4f }, NPC.Center);
            }
            
            // Monochrome energy gathering with rainbow hints
            if (Timer % 2 == 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector2 offset = Main.rand.NextVector2CircularEdge(80f, 80f);
                    Color baseColor = Main.rand.NextBool() ? Color.White : Color.Gray;
                    CustomParticles.PrismaticSparkle(NPC.Center + offset, baseColor, 0.4f);
                }
            }
            
            if (Timer >= WindupTime)
            {
                Timer = 0;
                State = ActionState.PrismaticRingAttack;
                NPC.netUpdate = true;
            }
        }

        private void PrismaticRingAttack(Player target)
        {
            const int AttackDuration = 32; // Faster attack
            isUsingAttackSprite = true;
            distortionIntensity = MathHelper.Lerp(distortionIntensity, 0.35f, 0.1f); // Visual distortion during attack
            
            // Aggressive approach during attack
            Vector2 toPlayer = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
            NPC.velocity = Vector2.Lerp(NPC.velocity, toPlayer * 10f, 0.06f);
            
            // Spawn rings of black/white sparkles more rapidly with rainbow explosion bursts
            if (Timer % 6 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item9 with { Volume = 0.6f }, NPC.Center);
                
                int sparkleCount = 20;
                float radius = 80f + (Timer / 6f) * 60f;
                
                for (int i = 0; i < sparkleCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / sparkleCount;
                    Vector2 position = NPC.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                    
                    // Alternate black and white particles in ring
                    Color monoColor = i % 2 == 0 ? Color.White : new Color(30, 30, 30);
                    CustomParticles.PrismaticSparkleBurst(position, monoColor, 4);
                    
                    // Pearlescent rainbow explosion at each point
                    Color rainbowColor = Main.hslToRgb(angle / MathHelper.TwoPi, 1f, 0.8f);
                    CustomParticles.GenericFlare(position, rainbowColor * 0.7f, 0.3f, 12);
                    
                    Dust sparkle = Dust.NewDustPerfect(position, DustID.Cloud, Vector2.Zero, 0, monoColor, 1.5f);
                    sparkle.noGravity = true;
                }
            }
            
            if (Timer >= AttackDuration)
            {
                EndAttackWithCue();
            }
        }

        #endregion

        #region Attack 3: Dual Swan Arc Slashes (Medium)

        // Store committed direction for predictable slashes
        private float dualSlashCommittedAngle = 0f;
        private Vector2 dualSlashTargetPos = Vector2.Zero;
        
        private void DualSlashWindup(Player target)
        {
            const int WindupTime = 14; // Slightly longer for telegraph
            isUsingAttackSprite = true;
            
            // === COMMIT TO DIRECTION EARLY ===
            if (Timer == 1)
            {
                // Lock direction toward player at START of windup
                dualSlashTargetPos = target.Center;
                dualSlashCommittedAngle = (target.Center - NPC.Center).ToRotation();
                SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0f }, NPC.Center);
            }
            
            // Move toward COMMITTED position, not current player position
            Vector2 toTarget = (dualSlashTargetPos - NPC.Center).SafeNormalize(Vector2.Zero);
            NPC.velocity = Vector2.Lerp(NPC.velocity, toTarget * 18f, 0.12f);
            
            // Building energy with feathers and arcs
            if (Timer % 3 == 0)
            {
                Vector2 offset = Main.rand.NextVector2Circular(100f, 100f);
                CustomParticles.SwordArcWave(NPC.Center + offset, Vector2.Zero, Color.White * 0.6f, 0.4f);
                CustomParticles.SwanFeatherAura(NPC.Center, 50f, 5);
            }
            
            // === TELEGRAPH: Show committed slash direction ===
            if (Timer > 8)
            {
                float telegraphIntensity = (Timer - 8f) / 12f;
                Vector2 leftDir = dualSlashCommittedAngle.ToRotationVector2().RotatedBy(MathHelper.ToRadians(-25));
                Vector2 rightDir = dualSlashCommittedAngle.ToRotationVector2().RotatedBy(MathHelper.ToRadians(25));
                
                // Show slash paths
                CustomParticles.GenericFlare(NPC.Center + leftDir * 80f, Color.Black * telegraphIntensity * 0.5f, 0.3f * telegraphIntensity, 5);
                CustomParticles.GenericFlare(NPC.Center + rightDir * 80f, Color.White * telegraphIntensity * 0.5f, 0.3f * telegraphIntensity, 5);
            }
            
            if (Timer >= WindupTime)
            {
                Timer = 0;
                AttackPhase = 0;
                State = ActionState.DualSlashAttack;
                NPC.netUpdate = true;
            }
        }

        private void DualSlashAttack(Player target)
        {
            const int SlashInterval = 12; // Slightly slower for readability
            const int TotalSlashes = 8; // Reduced for balance
            isUsingAttackSprite = true;
            distortionIntensity = MathHelper.Lerp(distortionIntensity, 0.45f, 0.12f);
            
            // === PREDICTABLE: Continue toward committed direction ===
            // Re-commit direction only every 2 slashes (less tracking)
            if (AttackPhase % 2 == 0 && Timer == 1)
            {
                // Update target, but use PREDICTED position (where player will be)
                Vector2 predictedPos = target.Center + target.velocity * 10f; // Lead the target
                dualSlashCommittedAngle = (predictedPos - NPC.Center).ToRotation();
            }
            
            Vector2 moveDir = dualSlashCommittedAngle.ToRotationVector2();
            NPC.velocity = Vector2.Lerp(NPC.velocity, moveDir * 16f, 0.1f);
            
            // Fire dual slashes using COMMITTED angle
            if (Timer == 1 && AttackPhase < TotalSlashes)
            {
                SoundEngine.PlaySound(SoundID.Item1 with { Pitch = 0.4f }, NPC.Center);
                
                // Use COMMITTED angle, not current player direction
                float baseAngle = dualSlashCommittedAngle;
                
                // Black swan slash (left)
                Vector2 leftDir = baseAngle.ToRotationVector2().RotatedBy(MathHelper.ToRadians(-25));
                CustomParticles.SwordArcCrescent(NPC.Center, leftDir * 20f, Color.Black, 1f);
                CustomParticles.SwanFeatherExplosion(NPC.Center + leftDir * 60, 10, 0.6f);
                Color leftRainbow = Main.hslToRgb((AttackPhase * 0.12f) % 1f, 1f, 0.8f);
                CustomParticles.PrismaticSparkleBurst(NPC.Center + leftDir * 100, leftRainbow, 7);
                
                // White swan slash (right)
                Vector2 rightDir = baseAngle.ToRotationVector2().RotatedBy(MathHelper.ToRadians(25));
                CustomParticles.SwordArcCrescent(NPC.Center, rightDir * 20f, Color.White, 1f);
                CustomParticles.SwanFeatherExplosion(NPC.Center + rightDir * 60, 10, 0.6f);
                Color rightRainbow = Main.hslToRgb((AttackPhase * 0.12f + 0.5f) % 1f, 1f, 0.8f);
                CustomParticles.PrismaticSparkleBurst(NPC.Center + rightDir * 100, rightRainbow, 7);
                
                // Visual effects - monochrome dust burst
                for (int i = 0; i < 25; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(14f, 14f);
                    Color dustColor = i % 2 == 0 ? Color.White : new Color(40, 40, 40);
                    Dust slash = Dust.NewDustDirect(NPC.Center, 30, 30, DustID.Cloud, vel.X, vel.Y, 100, dustColor, 2.2f);
                    slash.noGravity = true;
                }
                
                AttackPhase++;
            }
            
            if (Timer >= SlashInterval)
            {
                Timer = 0;
                
                if (AttackPhase >= TotalSlashes)
                {
                    State = ActionState.Idle;
                    NPC.netUpdate = true;
                }
            }
        }

        #endregion

        #region Attack 4: Lightning Fractal Storm (Large)

        // Store telegraphed strike positions for lightning attack
        private Vector2[] telegraphedLightningPositions = new Vector2[8];
        private int lightningStrikeIndex = 0;
        
        private void LightningStormWindup(Player target)
        {
            const int WindupTime = 18; // ULTRA fast windup (was 45) - barely any telegraph time!
            isUsingAttackSprite = true;
            
            // Hover above player menacingly with FASTER circling - tracks player
            float circleAngle = Timer * 0.15f;
            Vector2 hoverPos = target.Center + new Vector2((float)Math.Cos(circleAngle) * 80f, -200);
            Vector2 toHover = hoverPos - NPC.Center;
            NPC.velocity = Vector2.Lerp(NPC.velocity, toHover * 0.2f, 0.12f);
            
            if (Timer == 1)
            {
                SoundEngine.PlaySound(SoundID.Thunder, NPC.Center);
                EroicaScreenShake.MediumShake(NPC.Center);
                
                // Pre-calculate strike positions - NEVER at player's exact location
                // Strike positions are in a pattern around where the player IS, not where they're going
                lightningStrikeIndex = 0;
                for (int i = 0; i < 8; i++)
                {
                    // Calculate position offset from player - minimum 80 pixels away from center
                    float angle = MathHelper.TwoPi * i / 8f + Main.rand.NextFloat(-0.3f, 0.3f);
                    float distance = 100f + Main.rand.NextFloat(80f); // 100-180 away from player
                    telegraphedLightningPositions[i] = target.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * distance;
                }
            }
            
            // Intense crackling monochrome energy buildup with rainbow edges
            if (Timer % 2 == 0)
            {
                Vector2 offset = Main.rand.NextVector2Circular(150f, 150f);
                CustomParticles.GenericFlare(NPC.Center + offset, Color.White * 0.7f, 0.25f, 12);
                
                Dust lightning = Dust.NewDustDirect(NPC.Center + offset, 1, 1, DustID.Cloud, 0, 0, 100, Color.White, 1.8f);
                lightning.noGravity = true;
                
                // More rainbow shimmer
                if (Main.rand.NextBool(2))
                {
                    Color rainbowEdge = Main.hslToRgb(Main.rand.NextFloat(), 0.9f, 0.7f);
                    CustomParticles.PrismaticSparkle(NPC.Center + offset, rainbowEdge, 0.2f);
                }
            }
            
            // TELEGRAPH: Show warning particles at upcoming strike positions
            if (Timer > 15)
            {
                float telegraphIntensity = (Timer - 15f) / 30f; // 0 to 1 as windup progresses
                foreach (var pos in telegraphedLightningPositions)
                {
                    // Warning glow at strike position - grows more intense
                    if (Main.rand.NextFloat() < telegraphIntensity * 0.5f)
                    {
                        CustomParticles.GenericFlare(pos, Color.Yellow * telegraphIntensity * 0.5f, 0.2f * telegraphIntensity, 8);
                        
                        // Warning ring expanding
                        Dust warning = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(20f * telegraphIntensity, 20f * telegraphIntensity), 
                            DustID.Electric, Vector2.Zero, 100, Color.Yellow, 0.5f * telegraphIntensity);
                        warning.noGravity = true;
                    }
                }
            }
            
            if (Timer >= WindupTime)
            {
                Timer = 0;
                AttackPhase = 0;
                lightningStrikeIndex = 0;
                State = ActionState.LightningStormAttack;
                NPC.netUpdate = true;
            }
        }

        private void LightningStormAttack(Player target)
        {
            const int AttackDuration = 42; // FASTER attack (was 80)
            isUsingAttackSprite = true;
            distortionIntensity = MathHelper.Lerp(distortionIntensity, 0.55f, 0.1f); // Strong distortion during lightning
            
            // Aggressive pursuit during lightning storm - STRONG tracking
            Vector2 toPlayer = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
            NPC.velocity = Vector2.Lerp(NPC.velocity, toPlayer * 14f, 0.08f);
            
            // Spawn monochrome fractal lightning at TELEGRAPHED positions, not player's exact location
            if (Timer % 7 == 0) // FASTER rate (was 10)
            {
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.6f, Volume = 0.7f }, NPC.Center);
                
                // Strike at telegraphed position - cycles through the 8 pre-calculated positions
                if (lightningStrikeIndex < 8)
                {
                    Vector2 strikePos = telegraphedLightningPositions[lightningStrikeIndex];
                    lightningStrikeIndex++;
                    
                    // Black fractal lightning with rainbow outline from boss to strike point
                    MagnumVFX.DrawSwanLakeLightning(NPC.Center, strikePos, 10, 35f, 4, 0.5f);
                    
                    // Black and white core impact - bigger!
                    CustomParticles.ExplosionBurst(strikePos, Color.White, 12, 10f);
                    CustomParticles.ExplosionBurst(strikePos, Color.Black, 8, 6f);
                    CustomParticles.SwanFeatherBurst(strikePos, 6, 0.5f);
                    
                    // Pearlescent rainbow explosion ring
                    for (int i = 0; i < 10; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 10f;
                        Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 35f;
                        Color rainbowColor = Main.hslToRgb(i / 10f, 1f, 0.8f);
                        CustomParticles.PrismaticSparkleBurst(strikePos + offset, rainbowColor, 4);
                    }
                    
                    // Monochrome dust cloud
                    for (int i = 0; i < 18; i++)
                    {
                        Color dustColor = Main.rand.NextBool() ? Color.White : new Color(40, 40, 40);
                        Dust cloud = Dust.NewDustDirect(strikePos, 30, 30, DustID.Cloud, Main.rand.NextFloat(-7f, 7f), Main.rand.NextFloat(-7f, 7f), 100, dustColor, 2f);
                        cloud.noGravity = true;
                    }
                }
            }
            
            // Also spawn some warning indicators for next potential strikes
            if (Timer % 15 == 0 && lightningStrikeIndex < 7)
            {
                // Show where the next 2 strikes will land
                for (int w = 1; w <= Math.Min(2, 8 - lightningStrikeIndex); w++)
                {
                    Vector2 warningPos = telegraphedLightningPositions[lightningStrikeIndex + w - 1];
                    CustomParticles.GenericFlare(warningPos, Color.Yellow * 0.4f, 0.3f, 10);
                }
            }
            
            if (Timer >= AttackDuration)
            {
                Timer = 0;
                State = ActionState.Idle;
                NPC.netUpdate = true;
            }
        }

        #endregion

        #region Attack 5: Prismatic Radiant Beam (Ultimate)

        // Rotating beam attack
        private float beamRotationAngle = 0f;
        private float beamLength = 0f;
        
        private void ApocalypseWindup(Player target)
        {
            const int WindupTime = 32; // FASTER windup (was 75) - less time to prepare!
            isUsingAttackSprite = true;
            distortionIntensity = MathHelper.Lerp(distortionIntensity, 0.6f, 0.08f);
            
            // Rise above and hover in place, gathering energy - tracks player
            Vector2 risePos = target.Center + new Vector2(0, -280);
            Vector2 toRise = risePos - NPC.Center;
            NPC.velocity = Vector2.Lerp(NPC.velocity, toRise * 0.15f, 0.1f);
            
            if (Timer == 1)
            {
                SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
                EroicaScreenShake.LargeShake(NPC.Center);
                Main.NewText("Swan Lake channels the Prismatic Radiance!", 255, 255, 255);
                beamRotationAngle = 0f;
                beamLength = 0f;
            }
            
            // Intense energy buildup - rings of prismatic light
            if (Timer % 2 == 0)
            {
                // Charging rings spiraling inward
                float chargeProgress = Timer / (float)WindupTime;
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f + Timer * 0.15f;
                    float radius = 200f * (1f - chargeProgress * 0.5f);
                    Vector2 ringPos = NPC.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                    
                    // Prismatic rainbow colors
                    float hue = (i / 6f + Timer * 0.02f) % 1f;
                    Color beamColor = Main.hslToRgb(hue, 1f, 0.8f);
                    CustomParticles.GenericFlare(ringPos, beamColor, 0.4f * chargeProgress, 15);
                    CustomParticles.PrismaticSparkleBurst(ringPos, beamColor, 3);
                }
                
                // Core energy buildup
                Color coreColor = Main.hslToRgb((Timer * 0.03f) % 1f, 1f, 0.9f);
                CustomParticles.GenericFlare(NPC.Center, coreColor, 0.3f + chargeProgress * 0.5f, 20);
                CustomParticles.SwanFeatherDuality(NPC.Center, 6, 0.5f);
            }
            
            // Screenshake building up
            if (Timer % 10 == 0)
            {
                EroicaScreenShake.SmallShake(NPC.Center);
            }
            
            if (Timer >= WindupTime)
            {
                Timer = 0;
                AttackPhase = 0;
                beamRotationAngle = 0f;
                beamLength = 4000f; // MASSIVE beam - stretches across the map!
                State = ActionState.ApocalypseAttack;
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.5f, Volume = 1.2f }, NPC.Center);
                EroicaScreenShake.Phase2EnrageShake(NPC.Center);
                NPC.netUpdate = true;
            }
        }

        private void ApocalypseAttack(Player target)
        {
            const int AttackDuration = 126; // Full duration for learnable pattern
            isUsingAttackSprite = true;
            distortionIntensity = 0.7f; // Strong distortion during ultimate
            
            // === PREDICTABLE BEHAVIOR: Boss STAYS IN PLACE - no tracking ===
            // Slow to a stop, giving player predictable beam origin
            NPC.velocity *= 0.92f;
            
            // === LEARNABLE ROTATION PATTERN ===
            // Phase 1 (0-60): Slow clockwise rotation
            // Phase 2 (60-120): Faster clockwise, second beam appears
            // Phase 3 (120-180): Reverse direction (counter-clockwise) - players must adapt!
            
            float rotationSpeed;
            if (Timer < 42)
            {
                // Phase 1: Slow, predictable clockwise
                rotationSpeed = 0.035f;
            }
            else if (Timer < 84)
            {
                // Phase 2: Faster clockwise, second beam offset by 90 degrees (not 180)
                rotationSpeed = 0.055f;
            }
            else
            {
                // Phase 3: REVERSE DIRECTION - players must react!
                rotationSpeed = -0.065f;
                
                // Warning flash when direction changes
                if (Timer == 84)
                {
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.8f, Volume = 1.2f }, NPC.Center);
                    CustomParticles.ExplosionBurst(NPC.Center, Color.Red, 20, 15f);
                    EroicaScreenShake.MediumShake(NPC.Center);
                }
            }
            
            beamRotationAngle += rotationSpeed;
            
            // Draw the primary beam
            DrawPrismaticBeam(NPC.Center, beamRotationAngle, beamLength);
            
            // Second beam at 90 degrees offset (creates + pattern, not line)
            // This creates 4 "safe quadrants" that rotate - predictable!
            if (Timer >= 42)
            {
                DrawPrismaticBeam(NPC.Center, beamRotationAngle + MathHelper.PiOver2, beamLength * 0.85f);
            }
            
            // Third beam in DyingSwan mood - creates triangle pattern
            if (currentMood == BossMood.DyingSwan && Timer >= 63)
            {
                DrawPrismaticBeam(NPC.Center, beamRotationAngle + MathHelper.TwoPi / 3f, beamLength * 0.7f);
            }
            
            // === VISUAL TELEGRAPH FOR ROTATION CHANGE ===
            // Show warning particles 15 frames before direction change
            if (Timer >= 74 && Timer < 84)
            {
                float warningIntensity = (Timer - 74f) / 10f;
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 warningPos = NPC.Center + angle.ToRotationVector2() * (60f + warningIntensity * 30f);
                    CustomParticles.GenericFlare(warningPos, Color.Red * warningIntensity, 0.4f * warningIntensity, 8);
                }
            }
            
            // Particles spiraling around boss
            if (Timer % 2 == 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    float spiralAngle = beamRotationAngle * 2f + i * MathHelper.PiOver2;
                    float spiralRadius = 50f + (float)Math.Sin(Timer * 0.1f + i) * 20f;
                    Vector2 spiralPos = NPC.Center + new Vector2((float)Math.Cos(spiralAngle), (float)Math.Sin(spiralAngle)) * spiralRadius;
                    
                    float hue = (Timer * 0.02f + i * 0.25f) % 1f;
                    Color spiralColor = Main.hslToRgb(hue, 1f, 0.8f);
                    CustomParticles.PrismaticSparkleBurst(spiralPos, spiralColor, 4);
                }
            }
            
            // Feather duality spirals
            if (Timer % 8 == 0)
            {
                CustomParticles.SwanFeatherDuality(NPC.Center, 10, 0.8f);
                
                // Fractal flares along beam direction
                Vector2 beamDir = new Vector2((float)Math.Cos(beamRotationAngle), (float)Math.Sin(beamRotationAngle));
                for (int f = 0; f < 5; f++)
                {
                    Vector2 flarePos = NPC.Center + beamDir * (100f + f * 100f);
                    float hue = (Timer * 0.03f + f * 0.1f) % 1f;
                    CustomParticles.GenericFlare(flarePos, Main.hslToRgb(hue, 1f, 0.9f), 0.6f, 15);
                }
            }
            
            // Screenshake
            if (Timer % 8 == 0)
            {
                EroicaScreenShake.SmallShake(NPC.Center);
            }
            
            // Sound effects
            if (Timer % 15 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item15 with { Pitch = 0.3f, Volume = 0.6f }, NPC.Center);
            }
            
            if (Timer >= AttackDuration)
            {
                Timer = 0;
                beamLength = 0f;
                State = ActionState.Idle;
                NPC.netUpdate = true;
            }
        }
        
        private void DrawPrismaticBeam(Vector2 origin, float angle, float length)
        {
            Vector2 direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
            Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
            
            // Draw beam in segments - MORE segments for smoother, more visible beam
            int segments = 50;
            float segmentLength = length / segments;
            
            for (int i = 0; i < segments; i++)
            {
                Vector2 segmentPos = origin + direction * (i * segmentLength);
                float segmentProgress = i / (float)segments;
                
                // BRIGHT rainbow color gradient along beam - higher saturation and luminosity
                float hue = (Main.GameUpdateCount * 0.02f + segmentProgress * 0.6f) % 1f;
                Color beamColor = Main.hslToRgb(hue, 1f, 0.92f); // Brighter!
                
                // === ENHANCED CORE BEAM - Much more visible! ===
                // Large central flare - the main visible part
                float coreSize = 0.9f - segmentProgress * 0.4f;
                CustomParticles.GenericFlare(segmentPos, Color.White, coreSize * 0.8f, 20); // Bright white core
                CustomParticles.GenericFlare(segmentPos, beamColor, coreSize, 15); // Rainbow overlay
                
                // WIDE white core particles on BOTH sides - MUCH LARGER
                if (i % 2 == 0)
                {
                    float dustScale = 4f - segmentProgress * 1.5f; // Much larger dust
                    
                    // Triple-layer beam width
                    for (int layer = 0; layer < 3; layer++)
                    {
                        float layerOffset = 8f + layer * 12f;
                        
                        // Top side of beam
                        Dust coreTop = Dust.NewDustPerfect(segmentPos + perpendicular * layerOffset, DustID.WhiteTorch,
                            perpendicular * Main.rand.NextFloat(0.5f, 2f),
                            0, Color.White, dustScale - layer * 0.8f);
                        coreTop.noGravity = true;
                        
                        // Bottom side of beam
                        Dust coreBottom = Dust.NewDustPerfect(segmentPos - perpendicular * layerOffset, DustID.WhiteTorch,
                            -perpendicular * Main.rand.NextFloat(0.5f, 2f),
                            0, Color.White, dustScale - layer * 0.8f);
                        coreBottom.noGravity = true;
                    }
                }
                
                // Black contrast outline on outer edges - creates definition
                if (i % 4 == 0)
                {
                    float blackOffset = 35f + Main.rand.NextFloat(10f);
                    Dust blackTop = Dust.NewDustPerfect(segmentPos + perpendicular * blackOffset,
                        DustID.Smoke, Vector2.Zero, 200, Color.Black, 2.5f);
                    blackTop.noGravity = true;
                    
                    Dust blackBottom = Dust.NewDustPerfect(segmentPos - perpendicular * blackOffset,
                        DustID.Smoke, Vector2.Zero, 200, Color.Black, 2.5f);
                    blackBottom.noGravity = true;
                }
                
                // Rainbow edge sparkles - MUCH more visible
                if (i % 3 == 0)
                {
                    float perpOffset = 25f - segmentProgress * 8f;
                    Color edgeColor = Main.hslToRgb((hue + 0.5f) % 1f, 1f, 0.95f); // Complementary bright color
                    
                    // Top edge sparkles - larger
                    CustomParticles.PrismaticSparkleBurst(segmentPos + perpendicular * perpOffset, edgeColor, 4);
                    CustomParticles.GenericFlare(segmentPos + perpendicular * perpOffset, edgeColor, 0.5f, 12);
                    
                    // Bottom edge sparkles - larger
                    CustomParticles.PrismaticSparkleBurst(segmentPos - perpendicular * perpOffset, edgeColor, 4);
                    CustomParticles.GenericFlare(segmentPos - perpendicular * perpOffset, edgeColor, 0.5f, 12);
                }
                
                // INTENSE Lighting along beam - creates the glowing visibility
                float lightIntensity = 2f - segmentProgress * 0.8f; // Much brighter
                Vector3 lightColor = beamColor.ToVector3() * lightIntensity;
                Vector3 whiteBoost = new Vector3(1.5f, 1.5f, 1.8f) * (1f - segmentProgress * 0.5f);
                
                Lighting.AddLight(segmentPos, lightColor + whiteBoost);
                Lighting.AddLight(segmentPos + perpendicular * 20f, (lightColor + whiteBoost) * 0.6f);
                Lighting.AddLight(segmentPos - perpendicular * 20f, (lightColor + whiteBoost) * 0.6f);
            }
            
            // === MASSIVE Impact point at end of beam ===
            Vector2 beamEnd = origin + direction * length;
            
            // Large white core explosion
            CustomParticles.ExplosionBurst(beamEnd, Color.White, 15, 12f);
            
            // Rainbow ring at impact
            for (int r = 0; r < 8; r++)
            {
                float ringAngle = MathHelper.TwoPi * r / 8f;
                Vector2 ringOffset = new Vector2((float)Math.Cos(ringAngle), (float)Math.Sin(ringAngle)) * 30f;
                float ringHue = r / 8f;
                CustomParticles.GenericFlare(beamEnd + ringOffset, Main.hslToRgb(ringHue, 1f, 0.9f), 0.6f, 15);
            }
            
            // Swan Lake lightning from beam end - more frequent
            if (Main.rand.NextBool(2))
            {
                Vector2 lightningEnd = beamEnd + Main.rand.NextVector2Circular(120f, 120f);
                MagnumVFX.DrawSwanLakeLightning(beamEnd, lightningEnd, 8, 25f, 3, 0.4f);
            }
        }

        #endregion

        #region Attack 6: Prismatic Vortex (Spiral Inward Beams)

        private float[] vortexBeamAngles = new float[8];
        
        private void PrismaticVortexWindup(Player target)
        {
            const int WindupTime = 28;
            isUsingAttackSprite = true;
            distortionIntensity = MathHelper.Lerp(distortionIntensity, 0.5f, 0.1f);
            
            // Rise and prepare
            Vector2 risePos = target.Center + new Vector2(0, -250);
            Vector2 toRise = risePos - NPC.Center;
            NPC.velocity = Vector2.Lerp(NPC.velocity, toRise * 0.12f, 0.08f);
            
            if (Timer == 1)
            {
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.5f, Volume = 1f }, NPC.Center);
                EroicaScreenShake.MediumShake(NPC.Center);
                
                // Initialize vortex angles evenly distributed
                for (int i = 0; i < 8; i++)
                {
                    vortexBeamAngles[i] = MathHelper.TwoPi * i / 8f;
                }
            }
            
            // Charging rings spiraling
            if (Timer % 2 == 0)
            {
                float chargeProgress = Timer / (float)WindupTime;
                for (int i = 0; i < 8; i++)
                {
                    float angle = vortexBeamAngles[i] + Timer * 0.1f;
                    float radius = 180f * (1f - chargeProgress * 0.3f);
                    Vector2 ringPos = NPC.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                    
                    float hue = (i / 8f + Timer * 0.02f) % 1f;
                    Color beamColor = Main.hslToRgb(hue, 1f, 0.8f);
                    CustomParticles.GenericFlare(ringPos, beamColor, 0.35f * chargeProgress, 12);
                    CustomParticles.PrismaticSparkleBurst(ringPos, beamColor, 3);
                }
            }
            
            if (Timer >= WindupTime)
            {
                Timer = 0;
                State = ActionState.PrismaticVortexAttack;
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.3f, Volume = 1.1f }, NPC.Center);
                NPC.netUpdate = true;
            }
        }

        private void PrismaticVortexAttack(Player target)
        {
            const int AttackDuration = 105; // Longer for learnable patterns
            isUsingAttackSprite = true;
            distortionIntensity = 0.6f;
            
            // === PREDICTABLE: Boss stays still during vortex ===
            NPC.velocity *= 0.9f;
            
            // === LEARNABLE PATTERN: Constant rotation speed with phases ===
            // Phase 1 (0-60): 8 beams rotating SLOWLY counter-clockwise
            // Phase 2 (60-120): Beams contract inward + speed up
            // Phase 3 (120-150): Beams REVERSE direction briefly before ending
            
            float spiralProgress = Timer / (float)AttackDuration;
            float beamLength;
            float rotationSpeed;
            
            if (Timer < 42)
            {
                // Phase 1: Full length, slow rotation
                beamLength = 4000f;
                rotationSpeed = -0.03f; // Slow counter-clockwise
            }
            else if (Timer < 84)
            {
                // Phase 2: Contracting + faster
                float phase2Progress = (Timer - 42f) / 42f;
                beamLength = 4000f * (1f - phase2Progress * 0.5f); // Shrinks to 2000
                rotationSpeed = -0.05f; // Faster counter-clockwise
            }
            else
            {
                // Phase 3: REVERSE and expand - warning for players!
                if (Timer == 84)
                {
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.5f, Volume = 1.0f }, NPC.Center);
                    CustomParticles.ExplosionBurst(NPC.Center, Color.Yellow, 15, 12f);
                }
                float phase3Progress = (Timer - 84f) / 21f;
                beamLength = 2000f + phase3Progress * 1000f; // Expands back out
                rotationSpeed = 0.06f; // CLOCKWISE now!
            }
            
            // Apply CONSTANT rotation to all beams (predictable!)
            for (int i = 0; i < 8; i++)
            {
                vortexBeamAngles[i] += rotationSpeed;
                DrawPrismaticBeam(NPC.Center, vortexBeamAngles[i], beamLength);
            }
            
            // Safe zone indicators - show the gaps between beams
            if (Timer % 6 == 0)
            {
                for (int i = 0; i < 8; i++)
                {
                    // Mark the MIDDLE of each gap (45 degrees between beams)
                    float gapAngle = vortexBeamAngles[i] + MathHelper.PiOver4; // Halfway between beams
                    Vector2 safeIndicator = NPC.Center + gapAngle.ToRotationVector2() * 100f;
                    CustomParticles.GenericFlare(safeIndicator, Color.White * 0.3f, 0.2f, 8);
                }
            }
            
            // Particles at center becoming more intense
            if (Timer % 3 == 0)
            {
                for (int p = 0; p < 4; p++)
                {
                    float hue = (Timer * 0.03f + p * 0.25f) % 1f;
                    CustomParticles.GenericFlare(NPC.Center + Main.rand.NextVector2Circular(30f, 30f), Main.hslToRgb(hue, 1f, 0.9f), 0.5f, 10);
                }
                CustomParticles.SwanFeatherDuality(NPC.Center, 8, 0.7f);
            }
            
            if (Timer % 12 == 0)
            {
                EroicaScreenShake.SmallShake(NPC.Center);
            }
            
            if (Timer >= AttackDuration)
            {
                Timer = 0;
                State = ActionState.Idle;
                NPC.netUpdate = true;
            }
        }

        #endregion

        #region Attack 7: Prismatic Barrage (Scattered Beam Bursts)

        private void PrismaticBarrageWindup(Player target)
        {
            const int WindupTime = 21;
            isUsingAttackSprite = true;
            distortionIntensity = MathHelper.Lerp(distortionIntensity, 0.4f, 0.1f);
            
            // Aggressive approach
            Vector2 toPlayer = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
            NPC.velocity = Vector2.Lerp(NPC.velocity, toPlayer * 12f, 0.1f);
            
            if (Timer == 1)
            {
                SoundEngine.PlaySound(SoundID.Item15 with { Pitch = 0.2f, Volume = 0.9f }, NPC.Center);
            }
            
            // Charging sparkles
            if (Timer % 2 == 0)
            {
                for (int i = 0; i < 6; i++)
                {
                    Vector2 offset = Main.rand.NextVector2Circular(80f, 80f);
                    float hue = Main.rand.NextFloat();
                    CustomParticles.PrismaticSparkleBurst(NPC.Center + offset, Main.hslToRgb(hue, 1f, 0.8f), 4);
                }
            }
            
            if (Timer >= WindupTime)
            {
                Timer = 0;
                AttackPhase = 0;
                State = ActionState.PrismaticBarrageAttack;
                NPC.netUpdate = true;
            }
        }

        private void PrismaticBarrageAttack(Player target)
        {
            const int AttackDuration = 70;
            const int BurstInterval = 8;
            isUsingAttackSprite = true;
            distortionIntensity = 0.5f;
            
            // === PREDICTABLE MOVEMENT: Circle around player, not chase ===
            float circleAngle = Timer * 0.05f;
            float circleRadius = 300f;
            Vector2 circlePos = target.Center + circleAngle.ToRotationVector2() * circleRadius;
            Vector2 toCircle = circlePos - NPC.Center;
            NPC.velocity = Vector2.Lerp(NPC.velocity, toCircle * 0.08f, 0.06f);
            
            // === PATTERN-BASED BEAMS: Rotating burst pattern, not random ===
            if (Timer % BurstInterval == 0)
            {
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.1f + AttackPhase * 0.05f, Volume = 0.8f }, NPC.Center);
                EroicaScreenShake.SmallShake(NPC.Center);
                
                // FIXED pattern: 5 beams in a spread that ROTATES each burst
                // Each burst is offset by 22.5 degrees from the last
                float burstBaseAngle = AttackPhase * MathHelper.ToRadians(22.5f);
                int beamCount = 5;
                float spreadAngle = MathHelper.ToRadians(30f); // 30 degrees between beams
                
                for (int i = 0; i < beamCount; i++)
                {
                    // Spread from -60 to +60 degrees relative to base angle
                    float offsetAngle = (i - (beamCount - 1) / 2f) * spreadAngle;
                    float finalAngle = burstBaseAngle + offsetAngle;
                    float beamLength = 3500f; // Consistent length
                    DrawPrismaticBeam(NPC.Center, finalAngle, beamLength);
                    
                    // Impact effect at beam end
                    Vector2 impactPos = NPC.Center + finalAngle.ToRotationVector2() * beamLength;
                    float hue = (float)i / beamCount;
                    CustomParticles.ExplosionBurst(impactPos, Main.hslToRgb(hue, 1f, 0.85f), 6, 5f);
                }
                
                // Show telegraph for NEXT burst direction
                float nextBurstAngle = (AttackPhase + 1) * MathHelper.ToRadians(22.5f);
                for (int w = 0; w < 3; w++)
                {
                    Vector2 warningPos = NPC.Center + (nextBurstAngle + (w - 1) * spreadAngle).ToRotationVector2() * 80f;
                    CustomParticles.GenericFlare(warningPos, Color.Yellow * 0.4f, 0.25f, BurstInterval - 2);
                }
                
                AttackPhase++;
                CustomParticles.SwanFeatherExplosion(NPC.Center, 12, 0.8f);
            }
            
            if (Timer >= AttackDuration)
            {
                Timer = 0;
                State = ActionState.Idle;
                NPC.netUpdate = true;
            }
        }

        #endregion

        #region Attack 8: Prismatic Cross (X-Pattern Beams)

        private float crossRotation = 0f;
        
        private void PrismaticCrossWindup(Player target)
        {
            const int WindupTime = 25;
            isUsingAttackSprite = true;
            distortionIntensity = MathHelper.Lerp(distortionIntensity, 0.55f, 0.1f);
            
            // Position above player
            Vector2 hoverPos = target.Center + new Vector2(0, -220);
            Vector2 toHover = hoverPos - NPC.Center;
            NPC.velocity = Vector2.Lerp(NPC.velocity, toHover * 0.15f, 0.1f);
            
            if (Timer == 1)
            {
                SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.3f, Volume = 1f }, NPC.Center);
                crossRotation = MathHelper.PiOver4; // Start at 45 degrees for X shape
            }
            
            // X-shaped charging particles
            if (Timer % 2 == 0)
            {
                float chargeProgress = Timer / (float)WindupTime;
                for (int arm = 0; arm < 4; arm++)
                {
                    float armAngle = crossRotation + arm * MathHelper.PiOver2;
                    for (int p = 0; p < 3; p++)
                    {
                        float dist = 50f + p * 40f;
                        Vector2 pos = NPC.Center + new Vector2((float)Math.Cos(armAngle), (float)Math.Sin(armAngle)) * dist;
                        float hue = (arm * 0.25f + Timer * 0.02f) % 1f;
                        CustomParticles.PrismaticSparkle(pos, Main.hslToRgb(hue, 1f, 0.8f), 0.25f * chargeProgress);
                    }
                }
            }
            
            if (Timer >= WindupTime)
            {
                Timer = 0;
                State = ActionState.PrismaticCrossAttack;
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.2f, Volume = 1.1f }, NPC.Center);
                EroicaScreenShake.MediumShake(NPC.Center);
                NPC.netUpdate = true;
            }
        }

        private void PrismaticCrossAttack(Player target)
        {
            const int AttackDuration = 84; // Longer for phase pattern
            isUsingAttackSprite = true;
            distortionIntensity = 0.65f;
            
            // === PREDICTABLE: Boss stays STILL during cross attack ===
            NPC.velocity *= 0.9f;
            
            // === LEARNABLE ROTATION PHASES ===
            // Phase 1 (0-40): Slow clockwise
            // Phase 2 (40-80): Faster clockwise  
            // Phase 3 (80-120): REVERSE to counter-clockwise!
            
            float rotationSpeed;
            if (Timer < 28)
            {
                rotationSpeed = 0.02f; // Slow
            }
            else if (Timer < 56)
            {
                rotationSpeed = 0.04f; // Faster
                
                // Warning at phase change
                if (Timer == 28)
                {
                    SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.5f }, NPC.Center);
                    CustomParticles.GenericFlare(NPC.Center, Color.Orange, 0.8f, 15);
                }
            }
            else
            {
                rotationSpeed = -0.05f; // REVERSE!
                
                // Big warning at reversal
                if (Timer == 56)
                {
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.7f, Volume = 1.1f }, NPC.Center);
                    CustomParticles.ExplosionBurst(NPC.Center, Color.Red, 15, 10f);
                    EroicaScreenShake.SmallShake(NPC.Center);
                }
            }
            
            crossRotation += rotationSpeed;
            
            // Draw 4 beams in X pattern - MASSIVE map-spanning beams!
            for (int arm = 0; arm < 4; arm++)
            {
                float armAngle = crossRotation + arm * MathHelper.PiOver2;
                DrawPrismaticBeam(NPC.Center, armAngle, 4000f);
            }
            
            // === TELEGRAPH: Show beam positions 10 frames ahead ===
            if (Timer % 8 == 0)
            {
                float futureRotation = crossRotation + rotationSpeed * 10f;
                for (int arm = 0; arm < 4; arm++)
                {
                    float futureAngle = futureRotation + arm * MathHelper.PiOver2;
                    Vector2 telegraphPos = NPC.Center + futureAngle.ToRotationVector2() * 120f;
                    CustomParticles.GenericFlare(telegraphPos, Color.Yellow * 0.3f, 0.25f, 8);
                }
            }
            
            // Center particle effects
            if (Timer % 3 == 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    float angle = crossRotation + i * MathHelper.PiOver2;
                    Vector2 sparkPos = NPC.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 40f;
                    float hue = (i * 0.25f + Timer * 0.025f) % 1f;
                    CustomParticles.GenericFlare(sparkPos, Main.hslToRgb(hue, 1f, 0.9f), 0.4f, 10);
                }
                CustomParticles.SwanFeatherDuality(NPC.Center, 6, 0.6f);
            }
            
            if (Timer % 6 == 0)
            {
                EroicaScreenShake.SmallShake(NPC.Center);
            }
            
            if (Timer >= AttackDuration)
            {
                Timer = 0;
                State = ActionState.Idle;
                NPC.netUpdate = true;
            }
        }

        #endregion

        #region Attack 9: Prismatic Wave (Horizontal Sweep)

        private float waveAngle = 0f;
        private bool waveSweepRight = true;
        
        private void PrismaticWaveWindup(Player target)
        {
            const int WindupTime = 21;
            isUsingAttackSprite = true;
            distortionIntensity = MathHelper.Lerp(distortionIntensity, 0.45f, 0.1f);
            
            // Position to the side of player
            float sideOffset = waveSweepRight ? -300f : 300f;
            Vector2 sidePos = target.Center + new Vector2(sideOffset, -100);
            Vector2 toSide = sidePos - NPC.Center;
            NPC.velocity = Vector2.Lerp(NPC.velocity, toSide * 0.2f, 0.12f);
            
            if (Timer == 1)
            {
                SoundEngine.PlaySound(SoundID.Item15 with { Pitch = 0.4f, Volume = 0.9f }, NPC.Center);
                waveAngle = waveSweepRight ? -MathHelper.PiOver4 : MathHelper.Pi + MathHelper.PiOver4;
                waveSweepRight = !waveSweepRight; // Alternate direction next time
            }
            
            // Charging wave particles
            if (Timer % 2 == 0)
            {
                float chargeProgress = Timer / (float)WindupTime;
                for (int i = 0; i < 5; i++)
                {
                    Vector2 chargePos = NPC.Center + new Vector2((float)Math.Cos(waveAngle), (float)Math.Sin(waveAngle)) * (50f + i * 40f);
                    float hue = (i * 0.2f + Timer * 0.03f) % 1f;
                    CustomParticles.GenericFlare(chargePos, Main.hslToRgb(hue, 1f, 0.8f), 0.3f * chargeProgress, 8);
                }
            }
            
            if (Timer >= WindupTime)
            {
                Timer = 0;
                State = ActionState.PrismaticWaveAttack;
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0f, Volume = 1f }, NPC.Center);
                NPC.netUpdate = true;
            }
        }

        private void PrismaticWaveAttack(Player target)
        {
            const int AttackDuration = 56;
            isUsingAttackSprite = true;
            distortionIntensity = 0.55f;
            
            // Stay relatively still
            NPC.velocity *= 0.92f;
            
            // Sweep the beam horizontally
            float sweepSpeed = 0.035f;
            if (waveAngle < MathHelper.Pi)
            {
                waveAngle += sweepSpeed; // Sweep right
            }
            else
            {
                waveAngle -= sweepSpeed; // Sweep left
            }
            
            // Draw the sweeping beam - MASSIVE map-spanning sweep!
            DrawPrismaticBeam(NPC.Center, waveAngle, 4500f);
            
            // Secondary beams for effect - also extended
            DrawPrismaticBeam(NPC.Center, waveAngle + 0.15f, 3000f);
            DrawPrismaticBeam(NPC.Center, waveAngle - 0.15f, 3000f);
            
            // Trail particles
            if (Timer % 2 == 0)
            {
                Vector2 beamDir = new Vector2((float)Math.Cos(waveAngle), (float)Math.Sin(waveAngle));
                for (int t = 0; t < 3; t++)
                {
                    Vector2 trailPos = NPC.Center + beamDir * (150f + t * 150f);
                    float hue = (Timer * 0.03f + t * 0.15f) % 1f;
                    CustomParticles.PrismaticSparkleBurst(trailPos, Main.hslToRgb(hue, 1f, 0.85f), 5);
                }
            }
            
            if (Timer % 5 == 0)
            {
                EroicaScreenShake.SmallShake(NPC.Center);
            }
            
            if (Timer >= AttackDuration)
            {
                Timer = 0;
                State = ActionState.Idle;
                NPC.netUpdate = true;
            }
        }

        #endregion

        #region Attack 10: Prismatic Chaos (Random Direction Beams)

        private float[] chaosBeamAngles = new float[6];
        private float[] chaosBeamSpeeds = new float[6];
        
        private void PrismaticChaosWindup(Player target)
        {
            const int WindupTime = 25;
            isUsingAttackSprite = true;
            distortionIntensity = MathHelper.Lerp(distortionIntensity, 0.6f, 0.1f);
            
            // Hover in place with slight erratic movement
            Vector2 hoverPos = target.Center + new Vector2((float)Math.Sin(Timer * 0.2f) * 50f, -260);
            Vector2 toHover = hoverPos - NPC.Center;
            NPC.velocity = Vector2.Lerp(NPC.velocity, toHover * 0.1f, 0.08f);
            
            if (Timer == 1)
            {
                SoundEngine.PlaySound(SoundID.Roar with { Pitch = 0.3f, Volume = 0.8f }, NPC.Center);
                
                // Initialize random beam angles and rotation speeds
                for (int i = 0; i < 6; i++)
                {
                    chaosBeamAngles[i] = Main.rand.NextFloat(MathHelper.TwoPi);
                    chaosBeamSpeeds[i] = Main.rand.NextFloat(-0.08f, 0.08f);
                    if (Math.Abs(chaosBeamSpeeds[i]) < 0.02f)
                        chaosBeamSpeeds[i] = 0.03f * Math.Sign(chaosBeamSpeeds[i] + 0.001f);
                }
            }
            
            // Chaotic charging particles
            if (Timer % 2 == 0)
            {
                for (int i = 0; i < 8; i++)
                {
                    Vector2 chaosOffset = Main.rand.NextVector2Circular(120f, 120f);
                    float hue = Main.rand.NextFloat();
                    CustomParticles.GenericFlare(NPC.Center + chaosOffset, Main.hslToRgb(hue, 1f, 0.75f), 0.3f, 10);
                }
            }
            
            if (Timer % 10 == 0)
            {
                EroicaScreenShake.SmallShake(NPC.Center);
            }
            
            if (Timer >= WindupTime)
            {
                Timer = 0;
                State = ActionState.PrismaticChaosAttack;
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.4f, Volume = 1.2f }, NPC.Center);
                EroicaScreenShake.LargeShake(NPC.Center);
                NPC.netUpdate = true;
            }
        }

        private void PrismaticChaosAttack(Player target)
        {
            const int AttackDuration = 105;
            isUsingAttackSprite = true;
            distortionIntensity = 0.75f;
            
            // Erratic movement
            NPC.velocity += Main.rand.NextVector2Circular(0.5f, 0.5f);
            NPC.velocity *= 0.95f;
            
            // Update and draw chaotic beams with random rotations
            for (int i = 0; i < 6; i++)
            {
                chaosBeamAngles[i] += chaosBeamSpeeds[i];
                
                // Occasionally change direction
                if (Main.rand.NextBool(120))
                {
                    chaosBeamSpeeds[i] = -chaosBeamSpeeds[i] + Main.rand.NextFloat(-0.02f, 0.02f);
                }
                
                float beamLength = 3500f + (float)Math.Sin(Timer * 0.1f + i) * 1000f; // Massive 3500-4500 beams!
                DrawPrismaticBeam(NPC.Center, chaosBeamAngles[i], beamLength);
            }
            
            // Chaotic particle effects
            if (Timer % 3 == 0)
            {
                for (int p = 0; p < 6; p++)
                {
                    Vector2 chaosPos = NPC.Center + Main.rand.NextVector2Circular(80f, 80f);
                    float hue = Main.rand.NextFloat();
                    CustomParticles.ExplosionBurst(chaosPos, Main.hslToRgb(hue, 1f, 0.85f), 6, 4f);
                }
                
                // Lightning effects
                if (Main.rand.NextBool(3))
                {
                    int randomBeam = Main.rand.Next(6);
                    Vector2 lightningStart = NPC.Center + new Vector2((float)Math.Cos(chaosBeamAngles[randomBeam]), (float)Math.Sin(chaosBeamAngles[randomBeam])) * 200f;
                    Vector2 lightningEnd = lightningStart + Main.rand.NextVector2Circular(150f, 150f);
                    MagnumVFX.DrawSwanLakeLightning(lightningStart, lightningEnd, 8, 25f, 3, 0.4f);
                }
            }
            
            // Feather chaos
            if (Timer % 6 == 0)
            {
                CustomParticles.SwanFeatherExplosion(NPC.Center + Main.rand.NextVector2Circular(100f, 100f), 15, 0.9f);
            }
            
            // Screen shake intensity varies
            if (Timer % 4 == 0)
            {
                EroicaScreenShake.SmallShake(NPC.Center);
            }
            if (Timer % 20 == 0)
            {
                EroicaScreenShake.MediumShake(NPC.Center);
            }
            
            if (Timer >= AttackDuration)
            {
                Timer = 0;
                State = ActionState.Idle;
                NPC.netUpdate = true;
            }
        }

        #endregion

        #region Attack 11: Swan's Serenade (Hero's Judgment Style)
        
        private float[] serenadeBeamAngles; // For fractal laser attack
        
        /// <summary>
        /// SWAN'S SERENADE - Hero's Judgment style spectacle attack
        /// Features: Converging feathers, safe zone indicators, multi-wave burst with safe arc
        /// 150 BPM timing: 24 frames per beat
        /// OPTIMIZED: Reduced particle counts, BossVFXOptimizer warnings
        /// </summary>
        private void SwanSerenadeWindup(Player target)
        {
            // DIFFICULTY: Shorter charge time
            int chargeTime = currentMood == BossMood.DyingSwan ? 29 : 38; // Shorter (was 60/72)
            isUsingAttackSprite = true;
            distortionIntensity = MathHelper.Lerp(distortionIntensity, 0.5f, 0.08f);
            
            // Hover above player during windup
            Vector2 hoverPos = target.Center + new Vector2(0, -280f);
            Vector2 toHover = hoverPos - NPC.Center;
            NPC.velocity = Vector2.Lerp(NPC.velocity, toHover * 0.1f, 0.08f); // Faster hover
            
            if (Timer == 1)
            {
                SoundEngine.PlaySound(SoundID.Item165 with { Pitch = 0.4f, Volume = 1.3f }, NPC.Center);
                
                if (Main.netMode != NetmodeID.Server)
                {
                    string text = currentMood == BossMood.DyingSwan ? "HEAR MY FINAL SONG!" : "LISTEN TO THE SERENADE!";
                    CombatText.NewText(NPC.Hitbox, Color.White, text, true);
                }
            }
            
            float progress = Timer / (float)chargeTime;
            
            // OPTIMIZED: Use BossVFXOptimizer for converging ring - reduced frequency
            if (Timer % 5 == 0)
            {
                BossVFXOptimizer.ConvergingWarning(NPC.Center, 280f, progress, Color.White, 8);
                
                // Occasional feathers (reduced)
                if (Timer % 10 == 0)
                {
                    CustomParticles.SwanFeatherSpiral(NPC.Center + Main.rand.NextVector2CircularEdge(200f * (1f - progress * 0.4f), 200f * (1f - progress * 0.4f)), 
                        Main.rand.NextBool() ? Color.White : Color.Black, 4);
                }
            }
            
            // SAFE ZONE INDICATOR - earlier start, use optimizer
            if (Timer > chargeTime / 3)
            {
                BossVFXOptimizer.SafeZoneRing(target.Center, 110f, 12);
                
                // Show safe arc from boss
                float safeAngle = (target.Center - NPC.Center).ToRotation();
                BossVFXOptimizer.SafeArcIndicator(NPC.Center, safeAngle, MathHelper.ToRadians(32f), 180f, 8);
            }
            
            // Building screen shake
            if (Timer > chargeTime * 0.6f)
            {
                EroicaScreenShake.SmallShake(NPC.Center);
            }
            
            if (Timer >= chargeTime)
            {
                Timer = 0;
                AttackPhase = 0;
                State = ActionState.SwanSerenadeAttack;
                NPC.netUpdate = true;
            }
        }
        
        private void SwanSerenadeAttack(Player target)
        {
            // DIFFICULTY: More waves, faster timing
            int waveCount = currentMood == BossMood.DyingSwan ? 6 : 5; // More waves (was 5/4)
            int waveDelay = 36; // Faster (was 48)
            isUsingAttackSprite = true;
            distortionIntensity = 0.6f;
            
            if (AttackPhase < waveCount)
            {
                // === MULTI-WAVE BURST WITH SAFE ARC ===
                if (Timer == 1)
                {
                    EroicaScreenShake.LargeShake(NPC.Center);
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f + AttackPhase * 0.1f, Volume = 1.4f }, NPC.Center);
                    
                    // OPTIMIZED: Use BossVFXOptimizer for central flash
                    BossVFXOptimizer.AttackReleaseBurst(NPC.Center, Color.White, Main.hslToRgb((AttackPhase * 0.15f) % 1f, 1f, 0.8f), 1.3f);
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        // DIFFICULTY: More projectiles, faster, tighter safe arc
                        int projectileCount = 32 + (int)AttackPhase * 5; // More (was 24+4)
                        float safeAngle = (target.Center - NPC.Center).ToRotation();
                        float safeArc = MathHelper.ToRadians(30f - AttackPhase * 2f); // Tighter (was 40f)
                        
                        for (int i = 0; i < projectileCount; i++)
                        {
                            float angle = MathHelper.TwoPi * i / projectileCount;
                            
                            // SAFE ARC - skip projectiles aimed toward player
                            float angleDiff = MathHelper.WrapAngle(angle - safeAngle);
                            if (Math.Abs(angleDiff) < safeArc) continue;
                            
                            // DIFFICULTY: Much faster projectiles
                            float speed = 14f + AttackPhase * 3f; // Faster (was 10f+2f)
                            Vector2 vel = angle.ToRotationVector2() * speed;
                            
                            // Alternate with more tracking
                            Color projColor = i % 2 == 0 ? Color.White : new Color(30, 30, 35);
                            float homing = i % 3 == 0 ? 0.02f : 0f; // More homing (was i%4, 0.015f)
                            
                            if (i % 4 == 0)
                                BossProjectileHelper.SpawnAcceleratingBolt(NPC.Center, vel * 0.7f, 100, projColor, 18f);
                            else
                                BossProjectileHelper.SpawnHostileOrb(NPC.Center, vel, 100, projColor, homing);
                        }
                    }
                    
                    // OPTIMIZED: Cascading halos with fewer particles
                    BossVFXOptimizer.OptimizedCascadingHalos(NPC.Center, Color.White, Color.Black, 6, 0.4f, 16);
                    
                    // OPTIMIZED: Reduced feather burst
                    for (int f = 0; f < 6; f++)
                    {
                        Color fColor = f % 2 == 0 ? Color.White : Color.Black;
                        float featherAngle = MathHelper.TwoPi * f / 6f;
                        CustomParticles.SwanFeatherSpiral(NPC.Center + featherAngle.ToRotationVector2() * 25f, fColor, 3);
                    }
                }
                
                if (Timer >= waveDelay)
                {
                    Timer = 0;
                    AttackPhase++;
                }
            }
            else
            {
                // Shorter recovery
                if (Timer >= 21) // Was 40
                {
                    Timer = 0;
                    AttackPhase = 0;
                    State = ActionState.Idle;
                    NPC.netUpdate = true;
                }
            }
        }
        
        #endregion
        
        #region Attack 12: Fractal Laser Storm
        
        /// <summary>
        /// FRACTAL LASER STORM - Crossing laser beams that sweep the arena
        /// Features: Warning indicators, sweeping prismatic beams
        /// OPTIMIZED: BossVFXOptimizer warnings, reduced particles
        /// </summary>
        private void FractalLaserWindup(Player target)
        {
            const int WindupTime = 25; // Shorter (was 48)
            isUsingAttackSprite = true;
            distortionIntensity = MathHelper.Lerp(distortionIntensity, 0.55f, 0.1f);
            
            int laserCount = currentMood == BossMood.DyingSwan ? 9 : 7; // More lasers (was 8/6)
            
            // Hover above arena center
            Vector2 hoverPos = target.Center + new Vector2(0, -260f);
            NPC.velocity = Vector2.Lerp(NPC.velocity, (hoverPos - NPC.Center) * 0.08f, 0.06f);
            
            if (Timer == 1)
            {
                SoundEngine.PlaySound(SoundID.Item15 with { Pitch = 0.5f }, NPC.Center);
                serenadeBeamAngles = new float[laserCount];
                for (int i = 0; i < laserCount; i++)
                {
                    serenadeBeamAngles[i] = MathHelper.TwoPi * i / laserCount;
                }
            }
            
            float progress = Timer / (float)WindupTime;
            
            // OPTIMIZED: Use LaserBeamWarning for clear telegraph
            if (Timer % 4 == 0)
            {
                for (int i = 0; i < laserCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / laserCount + Timer * 0.015f;
                    BossVFXOptimizer.LaserBeamWarning(NPC.Center, angle, 1500f, progress);
                }
            }
            
            // OPTIMIZED: Gathering sparkles - reduced frequency
            if (Timer % 8 == 0)
            {
                BossVFXOptimizer.ConvergingWarning(NPC.Center, 100f, progress, Color.White, 6);
            }
            
            if (Timer >= WindupTime)
            {
                Timer = 0;
                State = ActionState.FractalLaserAttack;
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f, Volume = 1.4f }, NPC.Center);
                EroicaScreenShake.MediumShake(NPC.Center);
                NPC.netUpdate = true;
            }
        }
        
        private void FractalLaserAttack(Player target)
        {
            // DIFFICULTY: Faster sweep, wider angle
            int sweepDuration = currentMood == BossMood.DyingSwan ? 42 : 50; // Faster (was 84/96)
            int laserCount = serenadeBeamAngles?.Length ?? 7;
            isUsingAttackSprite = true;
            distortionIntensity = 0.65f;
            
            float sweepProgress = Timer / (float)sweepDuration;
            float sweepAngle = sweepProgress * MathHelper.Pi * 1.3f; // Wider sweep (was Pi)
            
            // OPTIMIZED: Draw beams with reduced particles and spawn projectiles
            for (int i = 0; i < laserCount; i++)
            {
                float currentAngle = serenadeBeamAngles[i] + sweepAngle * (i % 2 == 0 ? 1 : -1);
                
                // OPTIMIZED: Draw beam less frequently but keep threat visible
                if (Timer % 2 == 0)
                {
                    float beamLength = 2000f;
                    DrawPrismaticBeam(NPC.Center, currentAngle, beamLength);
                }
                
                // DIFFICULTY: More frequent projectiles, faster (6 frames = quarter beat)
                if (Timer % 6 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int p = 0; p < 5 + (currentMood == BossMood.DyingSwan ? 1 : 0); p++) // More projectiles
                    {
                        float dist = 150f + p * 220f;
                        Vector2 projPos = NPC.Center + currentAngle.ToRotationVector2() * dist;
                        float projSpeed = 7f + p * 1f; // Faster
                        Vector2 projVel = currentAngle.ToRotationVector2() * projSpeed;
                        float hue = (i / (float)laserCount + Timer * 0.01f) % 1f;
                        
                        if (p % 2 == 0)
                            BossProjectileHelper.SpawnAcceleratingBolt(projPos, projVel, 95, Main.hslToRgb(hue, 1f, 0.8f), 15f);
                        else
                            BossProjectileHelper.SpawnHostileOrb(projPos, projVel, 95, Main.hslToRgb(hue, 1f, 0.8f), 0.025f);
                    }
                }
            }
            
            if (Timer >= sweepDuration)
            {
                Timer = 0;
                State = ActionState.Idle;
                NPC.netUpdate = true;
            }
        }
        
        #endregion
        
        #region Attack 13: Chromatic Surge
        
        /// <summary>
        /// CHROMATIC SURGE - Electrical rainbow pulses expanding outward
        /// Features: Building charge, expanding shock rings, lightning arcs
        /// OPTIMIZED: BossVFXOptimizer, reduced particles, danger zone warning
        /// </summary>
        private void ChromaticSurgeWindup(Player target)
        {
            const int ChargeTime = 36; // Shorter (was 48)
            isUsingAttackSprite = true;
            distortionIntensity = MathHelper.Lerp(distortionIntensity, 0.5f, 0.1f);
            
            NPC.velocity *= 0.9f;
            
            if (Timer == 1)
            {
                SoundEngine.PlaySound(SoundID.Item93 with { Pitch = 0.2f, Volume = 1.2f }, NPC.Center);
            }
            
            float progress = Timer / (float)ChargeTime;
            
            // OPTIMIZED: Use ElectricalBuildupWarning with rainbow tint
            if (Timer % 5 == 0)
            {
                float hue = (Timer * 0.03f) % 1f;
                BossVFXOptimizer.ElectricalBuildupWarning(NPC.Center, Main.hslToRgb(hue, 1f, 0.85f), 220f * (1f - progress * 0.6f), progress);
            }
            
            // OPTIMIZED: Reduced charge VFX frequency
            if (Timer % 6 == 0)
            {
                BossVFXOptimizer.OptimizedRadialFlares(NPC.Center, Main.hslToRgb((Timer * 0.02f) % 1f, 1f, 0.9f),
                    6, 60f - progress * 35f, 0.35f + progress * 0.25f, 10);
            }
            
            // Show danger zone expanding
            if (Timer > ChargeTime / 2)
            {
                BossVFXOptimizer.DangerZoneRing(NPC.Center, 100f + progress * 250f, 12);
            }
            
            if (Timer >= ChargeTime)
            {
                Timer = 0;
                AttackPhase = 0;
                State = ActionState.ChromaticSurgeAttack;
                NPC.netUpdate = true;
            }
        }
        
        private void ChromaticSurgeAttack(Player target)
        {
            // DIFFICULTY: More waves, faster, more projectiles
            int surgeWaves = currentMood == BossMood.DyingSwan ? 7 : 6; // More waves (was 6/5)
            int waveDelay = 13; // Much faster (was 24)
            isUsingAttackSprite = true;
            distortionIntensity = 0.7f;
            
            if (AttackPhase < surgeWaves)
            {
                if (Timer == 1)
                {
                    EroicaScreenShake.MediumShake(NPC.Center);
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.5f + AttackPhase * 0.08f, Volume = 1.3f }, NPC.Center);
                    
                    // OPTIMIZED: Use BossVFXOptimizer for central burst
                    float baseHue = (AttackPhase * 0.12f) % 1f;
                    BossVFXOptimizer.AttackReleaseBurst(NPC.Center, Main.hslToRgb(baseHue, 1f, 0.9f), 
                        Main.hslToRgb((baseHue + 0.3f) % 1f, 1f, 0.8f), 1.1f);
                    
                    // Spawn expanding ring projectiles
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        // DIFFICULTY: More projectiles, faster
                        int ringCount = 24 + (int)AttackPhase * 4; // More (was 18+3)
                        float baseSpeed = 13f + AttackPhase * 3f; // Much faster (was 9f+2f)
                        
                        for (int i = 0; i < ringCount; i++)
                        {
                            float angle = MathHelper.TwoPi * i / ringCount;
                            Vector2 vel = angle.ToRotationVector2() * baseSpeed;
                            
                            float hue = (i / (float)ringCount + AttackPhase * 0.1f) % 1f;
                            Color projColor = Main.hslToRgb(hue, 1f, 0.8f);
                            
                            // Mix projectile types for variety and difficulty
                            if (i % 4 == 0)
                                BossProjectileHelper.SpawnAcceleratingBolt(NPC.Center, vel * 0.6f, 95, projColor, 20f);
                            else if (i % 4 == 1)
                                BossProjectileHelper.SpawnHostileOrb(NPC.Center, vel, 95, projColor, 0.02f);
                            else
                                BossProjectileHelper.SpawnHostileOrb(NPC.Center, vel * 0.9f, 95, projColor, 0f);
                        }
                    }
                    
                    // OPTIMIZED: Cascading halos
                    BossVFXOptimizer.OptimizedCascadingHalos(NPC.Center, 
                        Main.hslToRgb((AttackPhase * 0.12f) % 1f, 1f, 0.85f),
                        Main.hslToRgb((AttackPhase * 0.12f + 0.5f) % 1f, 1f, 0.85f),
                        5, 0.35f, 14);
                }
                
                if (Timer >= waveDelay)
                {
                    Timer = 0;
                    AttackPhase++;
                }
            }
            else
            {
                if (Timer >= 20) // Shorter recovery (was 40)
                {
                    Timer = 0;
                    AttackPhase = 0;
                    State = ActionState.Idle;
                    NPC.netUpdate = true;
                }
            }
        }
        
        #endregion

        #region Attack 14: Prismatic Helix (Fluid Twin Spiral Bullet Hell)
        
        private float helixAngle = 0f;
        private float helixSecondaryAngle = 0f;
        
        /// <summary>
        /// PRISMATIC HELIX - Twin spiral projectile streams that dance around each other
        /// Creates beautiful, flowing DNA-like bullet patterns with rainbow gradients
        /// Highly fluid and mesmerizing, requires circular dodging
        /// </summary>
        private void PrismaticHelixWindup(Player target)
        {
            const int WindupTime = 28;
            isUsingAttackSprite = true;
            distortionIntensity = MathHelper.Lerp(distortionIntensity, 0.5f, 0.08f);
            
            // Hover above player with graceful sway
            float swayX = (float)Math.Sin(Timer * 0.08f) * 60f;
            Vector2 hoverPos = target.Center + new Vector2(swayX, -300);
            Vector2 toHover = hoverPos - NPC.Center;
            NPC.velocity = Vector2.Lerp(NPC.velocity, toHover * 0.08f, 0.06f);
            
            if (Timer == 1)
            {
                SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.4f, Volume = 0.9f }, NPC.Center);
                helixAngle = 0f;
                helixSecondaryAngle = MathHelper.Pi; // Opposite side
            }
            
            // Charging helix visual - two spiraling particle streams
            float progress = Timer / (float)WindupTime;
            if (Timer % 2 == 0)
            {
                for (int spiral = 0; spiral < 2; spiral++)
                {
                    float baseAngle = spiral == 0 ? Timer * 0.15f : Timer * 0.15f + MathHelper.Pi;
                    float radius = 40f + progress * 80f;
                    Vector2 spiralPos = NPC.Center + baseAngle.ToRotationVector2() * radius;
                    
                    float hue = (Timer * 0.02f + spiral * 0.5f) % 1f;
                    CustomParticles.GenericFlare(spiralPos, Main.hslToRgb(hue, 1f, 0.85f), 0.35f + progress * 0.25f, 12);
                }
            }
            
            if (Timer >= WindupTime)
            {
                Timer = 0;
                State = ActionState.PrismaticHelixAttack;
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 1.1f }, NPC.Center);
                EroicaScreenShake.MediumShake(NPC.Center);
                NPC.netUpdate = true;
            }
        }
        
        private void PrismaticHelixAttack(Player target)
        {
            const int AttackDuration = 126; // 3 seconds of flowing spiral
            isUsingAttackSprite = true;
            distortionIntensity = 0.55f;
            
            // Gentle hover with rotation matching helix
            float swayX = (float)Math.Sin(Timer * 0.05f) * 30f;
            float swayY = (float)Math.Cos(Timer * 0.04f) * 20f;
            Vector2 hoverPos = target.Center + new Vector2(swayX, -280 + swayY);
            Vector2 toHover = hoverPos - NPC.Center;
            NPC.velocity = Vector2.Lerp(NPC.velocity, toHover * 0.05f, 0.04f);
            
            // Update helix rotation - smooth and continuous
            float rotationSpeed = 0.06f + (currentMood == BossMood.DyingSwan ? 0.02f : 0f);
            helixAngle += rotationSpeed;
            helixSecondaryAngle += rotationSpeed;
            
            // Spawn twin spiraling projectile streams
            int spawnInterval = currentMood == BossMood.DyingSwan ? 3 : 4;
            if (Timer % spawnInterval == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int spiral = 0; spiral < 2; spiral++)
                {
                    float baseAngle = spiral == 0 ? helixAngle : helixSecondaryAngle;
                    
                    // Spawn projectiles at the spiral positions
                    float spawnRadius = 50f;
                    Vector2 spawnPos = NPC.Center + baseAngle.ToRotationVector2() * spawnRadius;
                    
                    // Velocity curves outward with slight arc
                    float velocityAngle = baseAngle + (spiral == 0 ? 0.4f : -0.4f);
                    float speed = 10f + (currentMood == BossMood.DyingSwan ? 3f : 0f);
                    Vector2 vel = velocityAngle.ToRotationVector2() * speed;
                    
                    // Rainbow color based on timer
                    float hue = (Timer * 0.015f + spiral * 0.5f) % 1f;
                    Color projColor = Main.hslToRgb(hue, 1f, 0.8f);
                    
                    // Mix of projectile types for visual interest
                    if (Timer % 12 == 0)
                        BossProjectileHelper.SpawnWaveProjectile(spawnPos, vel, 90, projColor, 2.5f);
                    else
                        BossProjectileHelper.SpawnHostileOrb(spawnPos, vel, 90, projColor, 0f);
                }
            }
            
            // Flowing particle trails at spawn points
            if (Timer % 3 == 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    float angle = i == 0 ? helixAngle : helixSecondaryAngle;
                    Vector2 trailPos = NPC.Center + angle.ToRotationVector2() * 50f;
                    float hue = (Timer * 0.02f + i * 0.5f) % 1f;
                    
                    for (int p = 0; p < 3; p++)
                    {
                        Vector2 sparkVel = angle.ToRotationVector2().RotatedByRandom(0.5f) * Main.rand.NextFloat(2f, 5f);
                        var spark = new GenericGlowParticle(trailPos, sparkVel, Main.hslToRgb(hue, 1f, 0.85f) * 0.8f, 0.3f, 18, true);
                        MagnumParticleHandler.SpawnParticle(spark);
                    }
                }
            }
            
            // Rainbow feathers occasionally
            if (Timer % 15 == 0)
            {
                float featherHue = (Timer * 0.02f) % 1f;
                CustomParticles.SwanFeatherExplosion(NPC.Center, 6, 0.6f);
            }
            
            if (Timer >= AttackDuration)
            {
                Timer = 0;
                State = ActionState.Idle;
                NPC.netUpdate = true;
            }
        }
        
        #endregion
        
        #region Attack 15: Rainbow Cascade (Waterfall Bullet Hell)
        
        /// <summary>
        /// RAINBOW CASCADE - Waterfall-style descending waves of prismatic projectiles
        /// Creates beautiful curtains of rainbow bullets that sweep across the arena
        /// Very fluid, requires horizontal movement to find safe gaps
        /// </summary>
        private void RainbowCascadeWindup(Player target)
        {
            const int WindupTime = 45;
            isUsingAttackSprite = true;
            distortionIntensity = MathHelper.Lerp(distortionIntensity, 0.6f, 0.08f);
            
            // Rise high above the arena
            Vector2 risePos = target.Center + new Vector2(0, -450);
            Vector2 toRise = risePos - NPC.Center;
            NPC.velocity = Vector2.Lerp(NPC.velocity, toRise * 0.1f, 0.06f);
            
            if (Timer == 1)
            {
                SoundEngine.PlaySound(SoundID.Item25 with { Pitch = -0.2f, Volume = 1.0f }, NPC.Center);
            }
            
            // Rainbow particles converging upward
            float progress = Timer / (float)WindupTime;
            if (Timer % 3 == 0)
            {
                int particleCount = 6 + (int)(progress * 8);
                for (int i = 0; i < particleCount; i++)
                {
                    float xOffset = Main.rand.NextFloat(-400f, 400f);
                    Vector2 particleStart = NPC.Center + new Vector2(xOffset, 200f);
                    Vector2 velocity = (NPC.Center - particleStart).SafeNormalize(Vector2.Zero) * (3f + progress * 4f);
                    
                    float hue = (i / (float)particleCount + Timer * 0.01f) % 1f;
                    var converge = new GenericGlowParticle(particleStart, velocity, Main.hslToRgb(hue, 1f, 0.85f), 0.4f, 25, true);
                    MagnumParticleHandler.SpawnParticle(converge);
                }
            }
            
            if (Timer >= WindupTime)
            {
                Timer = 0;
                AttackPhase = 0;
                State = ActionState.RainbowCascadeAttack;
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.1f, Volume = 1.2f }, NPC.Center);
                EroicaScreenShake.LargeShake(NPC.Center);
                NPC.netUpdate = true;
            }
        }
        
        private void RainbowCascadeAttack(Player target)
        {
            int waveCount = currentMood == BossMood.DyingSwan ? 8 : 6;
            int waveDelay = 14;
            isUsingAttackSprite = true;
            distortionIntensity = 0.65f;
            
            // Stay high, gentle horizontal sway
            float swayX = (float)Math.Sin(Timer * 0.04f + AttackPhase * 0.5f) * 100f;
            Vector2 hoverPos = target.Center + new Vector2(swayX, -420);
            Vector2 toHover = hoverPos - NPC.Center;
            NPC.velocity = Vector2.Lerp(NPC.velocity, toHover * 0.06f, 0.04f);
            
            if (AttackPhase < waveCount)
            {
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item25 with { Pitch = 0.1f + AttackPhase * 0.1f, Volume = 0.9f }, NPC.Center);
                    EroicaScreenShake.SmallShake(NPC.Center);
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        // Create waterfall curtain of projectiles
                        int curtainWidth = 800; // Wide curtain
                        int projectilesPerWave = currentMood == BossMood.DyingSwan ? 18 : 14;
                        float gapPosition = Main.rand.NextFloat(-200f, 200f); // Random safe gap
                        float gapWidth = currentMood == BossMood.DyingSwan ? 100f : 140f;
                        
                        for (int i = 0; i < projectilesPerWave; i++)
                        {
                            float xPos = -curtainWidth / 2f + (curtainWidth * i / (float)(projectilesPerWave - 1));
                            
                            // Skip projectiles in the gap zone
                            if (Math.Abs(xPos - gapPosition) < gapWidth / 2f)
                                continue;
                            
                            Vector2 spawnPos = NPC.Center + new Vector2(xPos, 0);
                            Vector2 vel = new Vector2(Main.rand.NextFloat(-1f, 1f), 8f + Main.rand.NextFloat(0f, 3f));
                            
                            // Accelerate as they fall
                            vel.Y += AttackPhase * 0.5f;
                            
                            float hue = (i / (float)projectilesPerWave + AttackPhase * 0.12f) % 1f;
                            Color projColor = Main.hslToRgb(hue, 1f, 0.8f);
                            
                            BossProjectileHelper.SpawnAcceleratingBolt(spawnPos, vel, 90, projColor, 8f);
                        }
                    }
                    
                    // Visual cascade effect
                    for (int i = 0; i < 10; i++)
                    {
                        float xOffset = Main.rand.NextFloat(-350f, 350f);
                        float hue = (i / 10f + AttackPhase * 0.12f) % 1f;
                        CustomParticles.GenericFlare(NPC.Center + new Vector2(xOffset, 0), Main.hslToRgb(hue, 1f, 0.85f), 0.5f, 15);
                    }
                }
                
                if (Timer >= waveDelay)
                {
                    Timer = 0;
                    AttackPhase++;
                }
            }
            else
            {
                if (Timer >= 25)
                {
                    Timer = 0;
                    AttackPhase = 0;
                    State = ActionState.Idle;
                    NPC.netUpdate = true;
                }
            }
        }
        
        #endregion
        
        #region Attack 16: Chromatic Kaleidoscope (Geometric Bullet Hell)
        
        private float kaleidoscopeRotation = 0f;
        
        /// <summary>
        /// CHROMATIC KALEIDOSCOPE - Rotating geometric bullet patterns
        /// Creates beautiful mandala-like patterns with rainbow projectiles
        /// Very visually impressive, requires precise positioning to find safe zones
        /// </summary>
        private void ChromaticKaleidoscopeWindup(Player target)
        {
            const int WindupTime = 35;
            isUsingAttackSprite = true;
            distortionIntensity = MathHelper.Lerp(distortionIntensity, 0.7f, 0.08f);
            
            // Center above player
            Vector2 centerPos = target.Center + new Vector2(0, -280);
            Vector2 toCenter = centerPos - NPC.Center;
            NPC.velocity = Vector2.Lerp(NPC.velocity, toCenter * 0.08f, 0.06f);
            
            if (Timer == 1)
            {
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.3f, Volume = 0.9f }, NPC.Center);
                kaleidoscopeRotation = 0f;
            }
            
            // Geometric mandala forming
            float progress = Timer / (float)WindupTime;
            if (Timer % 3 == 0)
            {
                int segments = 6 + (int)(progress * 4);
                for (int i = 0; i < segments; i++)
                {
                    float angle = MathHelper.TwoPi * i / segments + Timer * 0.04f;
                    float radius = 30f + progress * 100f;
                    Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                    
                    float hue = (i / (float)segments + Timer * 0.01f) % 1f;
                    CustomParticles.GenericFlare(pos, Main.hslToRgb(hue, 1f, 0.85f), 0.3f + progress * 0.3f, 12);
                }
            }
            
            // Inner rotating triangle
            if (Timer % 4 == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    float angle = MathHelper.TwoPi * i / 3f - Timer * 0.06f;
                    float innerRadius = 20f + progress * 40f;
                    Vector2 innerPos = NPC.Center + angle.ToRotationVector2() * innerRadius;
                    CustomParticles.GenericFlare(innerPos, Color.White, 0.25f + progress * 0.2f, 10);
                }
            }
            
            if (Timer >= WindupTime)
            {
                Timer = 0;
                AttackPhase = 0;
                State = ActionState.ChromaticKaleidoscopeAttack;
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f, Volume = 1.3f }, NPC.Center);
                EroicaScreenShake.LargeShake(NPC.Center);
                NPC.netUpdate = true;
            }
        }
        
        private void ChromaticKaleidoscopeAttack(Player target)
        {
            int patternCount = currentMood == BossMood.DyingSwan ? 6 : 4;
            int patternDelay = 35;
            isUsingAttackSprite = true;
            distortionIntensity = 0.75f;
            
            // Stay centered, slow rotation
            NPC.velocity *= 0.95f;
            kaleidoscopeRotation += 0.025f;
            
            if (AttackPhase < patternCount)
            {
                if (Timer == 1)
                {
                    EroicaScreenShake.MediumShake(NPC.Center);
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f + AttackPhase * 0.1f, Volume = 1.1f }, NPC.Center);
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        // Spawn geometric mandala pattern
                        int symmetry = 6; // Hexagonal symmetry
                        int ringsPerPattern = 3;
                        float baseSpeed = 8f + (currentMood == BossMood.DyingSwan ? 3f : 0f);
                        
                        // Calculate safe angle (toward player with gap)
                        float safeAngle = (target.Center - NPC.Center).ToRotation();
                        float safeArc = MathHelper.ToRadians(25f);
                        
                        for (int ring = 0; ring < ringsPerPattern; ring++)
                        {
                            int projectilesInRing = symmetry * (ring + 2);
                            float ringOffset = ring * 0.15f; // Stagger timing
                            
                            for (int i = 0; i < projectilesInRing; i++)
                            {
                                float angle = MathHelper.TwoPi * i / projectilesInRing + kaleidoscopeRotation + AttackPhase * 0.4f;
                                
                                // Safe arc check
                                float angleDiff = MathHelper.WrapAngle(angle - safeAngle);
                                if (Math.Abs(angleDiff) < safeArc) continue;
                                
                                float speed = baseSpeed + ring * 2f;
                                Vector2 vel = angle.ToRotationVector2() * speed;
                                
                                float hue = (i / (float)projectilesInRing + ring * 0.2f + AttackPhase * 0.15f) % 1f;
                                Color projColor = Main.hslToRgb(hue, 1f, 0.8f);
                                
                                // Inner rings accelerate, outer rings wave
                                if (ring == 0)
                                    BossProjectileHelper.SpawnAcceleratingBolt(NPC.Center, vel * 0.7f, 90, projColor, 15f);
                                else if (ring == 1)
                                    BossProjectileHelper.SpawnHostileOrb(NPC.Center, vel, 90, projColor, 0.01f);
                                else
                                    BossProjectileHelper.SpawnWaveProjectile(NPC.Center, vel * 0.9f, 90, projColor, 2f);
                            }
                        }
                    }
                    
                    // Mandala burst VFX
                    BossVFXOptimizer.AttackReleaseBurst(NPC.Center, Color.White, Main.hslToRgb((AttackPhase * 0.2f) % 1f, 1f, 0.85f), 1.2f);
                    
                    for (int i = 0; i < 6; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 6f + kaleidoscopeRotation;
                        float hue = (i / 6f + AttackPhase * 0.15f) % 1f;
                        CustomParticles.HaloRing(NPC.Center + angle.ToRotationVector2() * 60f, Main.hslToRgb(hue, 1f, 0.85f), 0.35f, 14);
                    }
                }
                
                // Continuous geometric particle pattern while active
                if (Timer % 4 == 0)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 6f + kaleidoscopeRotation + Timer * 0.05f;
                        Vector2 pos = NPC.Center + angle.ToRotationVector2() * (50f + Timer);
                        float hue = (i / 6f + Timer * 0.02f) % 1f;
                        CustomParticles.GenericFlare(pos, Main.hslToRgb(hue, 1f, 0.8f), 0.3f, 10);
                    }
                }
                
                if (Timer >= patternDelay)
                {
                    Timer = 0;
                    AttackPhase++;
                }
            }
            else
            {
                if (Timer >= 28)
                {
                    Timer = 0;
                    AttackPhase = 0;
                    State = ActionState.Idle;
                    NPC.netUpdate = true;
                }
            }
        }
        
        #endregion

        public override void FindFrame(int frameHeight)
        {
            // Animate through frames
            frameCounter++;
            int frameTime = isUsingAttackSprite ? AttackFrameTime : IdleFrameTime;
            
            if (frameCounter >= frameTime)
            {
                frameCounter = 0;
                currentFrame++;
                if (currentFrame >= TotalFrames)
                    currentFrame = 0;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            // Draw background darkness FIRST (before the boss)
            if (backgroundDarknessAlpha > 0.1f)
            {
                DrawBackgroundDarkness(spriteBatch);
            }
            
            // Choose texture based on attack state
            Texture2D texture;
            if (isUsingAttackSprite)
            {
                texture = ModContent.Request<Texture2D>(AttackTexture).Value;
            }
            else
            {
                texture = ModContent.Request<Texture2D>(Texture).Value;
            }

            // Calculate frame rectangle for 6x6 sprite sheet
            int frameWidth = texture.Width / FrameColumns;
            int frameHeight = texture.Height / FrameRows;
            int frameX = (currentFrame % FrameColumns) * frameWidth;
            int frameY = (currentFrame / FrameColumns) * frameHeight;
            Rectangle sourceRect = new Rectangle(frameX, frameY, frameWidth, frameHeight);

            Vector2 position = NPC.Center - screenPos;
            
            // Apply visual distortion effect
            if (distortionIntensity > 0.05f)
            {
                float waveX = (float)Math.Sin(distortionTimer * 8f + position.Y * 0.02f) * distortionIntensity * 3f;
                float waveY = (float)Math.Cos(distortionTimer * 6f + position.X * 0.02f) * distortionIntensity * 2f;
                position += new Vector2(waveX, waveY);
            }
            
            Vector2 origin = new Vector2(frameWidth / 2f, frameHeight / 2f);
            SpriteEffects effects = NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // Pulse effect - enhanced during death
            float pulse = 1f + (float)Math.Sin(pulseTimer * 3f) * 0.05f;
            if (isDying)
            {
                float deathPulse = 1f + (float)Math.Sin(deathTimer * 0.2f) * 0.15f * (deathTimer / (float)DeathAnimationDuration);
                pulse *= deathPulse;
            }
            
            // Draw glow layers (additive)
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Rainbow pearlescent glow - brighter during death
            float glowIntensity = isDying ? 0.4f + (deathTimer / (float)DeathAnimationDuration) * 0.6f : 0.4f;
            Color glowColor = Main.hslToRgb((Main.GameUpdateCount * 0.005f) % 1f, isDying ? 0.3f : 0.5f, isDying ? 0.8f : 0.6f) * glowIntensity;
            spriteBatch.Draw(texture, position, sourceRect, glowColor, NPC.rotation, origin, NPC.scale * pulse * 1.15f, effects, 0f);
            
            // White/silver outer glow - much brighter during death
            float whiteGlow = isDying ? 0.3f + (deathTimer / (float)DeathAnimationDuration) * 0.7f : 0.3f;
            spriteBatch.Draw(texture, position, sourceRect, Color.White * whiteGlow, NPC.rotation, origin, NPC.scale * pulse * 1.25f, effects, 0f);
            
            // Additional distortion ghost images during intense moments
            if (distortionIntensity > 0.3f)
            {
                for (int i = 0; i < 3; i++)
                {
                    float ghostOffset = (float)Math.Sin(distortionTimer * 5f + i * 2f) * distortionIntensity * 8f;
                    Vector2 ghostPos = position + new Vector2(ghostOffset, ghostOffset * 0.5f);
                    Color ghostColor = Main.hslToRgb((Main.GameUpdateCount * 0.01f + i * 0.3f) % 1f, 0.8f, 0.6f) * 0.15f;
                    spriteBatch.Draw(texture, ghostPos, sourceRect, ghostColor, NPC.rotation, origin, NPC.scale * pulse * 1.1f, effects, 0f);
                }
            }
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Draw main sprite - fade to white during death
            Color mainColor = isDying ? Color.Lerp(Color.White, new Color(255, 255, 255, 200), deathTimer / (float)DeathAnimationDuration) : Color.White;
            spriteBatch.Draw(texture, position, sourceRect, mainColor, NPC.rotation, origin, NPC.scale, effects, 0f);
            
            return false;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            // Background is now drawn in PreDraw, nothing needed here
        }

        private void DrawBackgroundDarkness(SpriteBatch spriteBatch)
        {
            // Draw COMPLETELY BLACK background like a void night sky
            Rectangle screenRect = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
            
            // Complete black void background
            spriteBatch.Draw(pixel, screenRect, pixel.Bounds, Color.Black * backgroundDarknessAlpha);
            
            // === PEARLESCENT WAVES like aurora borealis ===
            for (int wave = 0; wave < 5; wave++)
            {
                float waveOffset = Main.GameUpdateCount * 0.003f + wave * 0.5f;
                float waveY = Main.screenHeight * (0.2f + wave * 0.15f);
                
                for (int x = 0; x < Main.screenWidth; x += 8)
                {
                    float sineOffset = (float)Math.Sin(x * 0.01f + waveOffset) * 40f;
                    float sineOffset2 = (float)Math.Sin(x * 0.007f + waveOffset * 1.3f) * 25f;
                    float y = waveY + sineOffset + sineOffset2;
                    
                    // Pearlescent rainbow color shifting
                    float hue = (x * 0.001f + Main.GameUpdateCount * 0.002f + wave * 0.2f) % 1f;
                    Color waveColor = Main.hslToRgb(hue, 0.4f, 0.5f) * (0.15f * backgroundDarknessAlpha);
                    
                    // Draw gradient wave band
                    for (int h = 0; h < 30; h++)
                    {
                        float fade = 1f - (h / 30f);
                        spriteBatch.Draw(pixel, new Rectangle(x, (int)(y + h), 8, 1), pixel.Bounds, waveColor * fade);
                    }
                }
            }
            
            // === WHITE STARS - twinkling night sky ===
            for (int i = 0; i < 80; i++)
            {
                // Deterministic star positions based on index
                float starX = ((i * 137 + 47) % Main.screenWidth);
                float starY = ((i * 89 + 23) % Main.screenHeight);
                
                // Twinkling effect
                float twinkle = (float)Math.Sin(Main.GameUpdateCount * 0.08f + i * 0.5f);
                float brightness = 0.5f + twinkle * 0.5f;
                
                int starSize = (i % 3 == 0) ? 3 : (i % 2 == 0) ? 2 : 1;
                Color starColor = Color.White * (brightness * backgroundDarknessAlpha);
                
                spriteBatch.Draw(pixel, new Rectangle((int)starX, (int)starY, starSize, starSize), pixel.Bounds, starColor);
            }
            
            // === RAINBOW FLARES - large, dramatic ===
            for (int i = 0; i < 15; i++)
            {
                float flareX = (Main.GameUpdateCount * 0.8f + i * 180f) % (Main.screenWidth + 100) - 50;
                float flareY = (Main.GameUpdateCount * 0.4f + i * 220f + (float)Math.Sin(i * 0.7f) * 100f) % (Main.screenHeight + 100) - 50;
                
                // Rainbow color cycling
                float hue = (i * 0.1f + Main.GameUpdateCount * 0.003f) % 1f;
                Color flareColor = Main.hslToRgb(hue, 1f, 0.7f) * (0.6f * backgroundDarknessAlpha);
                
                // Draw flare with glow
                int flareSize = 4 + (i % 4) * 2;
                for (int g = flareSize; g > 0; g--)
                {
                    float glowFade = g / (float)flareSize;
                    spriteBatch.Draw(pixel, new Rectangle((int)flareX - g, (int)flareY - g, g * 2, g * 2), pixel.Bounds, flareColor * glowFade * 0.3f);
                }
                spriteBatch.Draw(pixel, new Rectangle((int)flareX - 1, (int)flareY - 1, 3, 3), pixel.Bounds, Color.White * backgroundDarknessAlpha);
            }
            
            // === WHITE FLARES - bright streaking ===
            for (int i = 0; i < 20; i++)
            {
                float flareX = (Main.GameUpdateCount * 1.2f + i * 130f) % (Main.screenWidth + 50) - 25;
                float flareY = (Main.GameUpdateCount * 0.6f + i * 170f) % (Main.screenHeight + 50) - 25;
                
                Color whiteFlare = Color.White * (0.7f * backgroundDarknessAlpha);
                
                // Streak effect
                int streakLength = 8 + (i % 5) * 3;
                for (int s = 0; s < streakLength; s++)
                {
                    float fade = 1f - (s / (float)streakLength);
                    spriteBatch.Draw(pixel, new Rectangle((int)(flareX - s * 0.5f), (int)(flareY - s * 0.3f), 2, 2), pixel.Bounds, whiteFlare * fade);
                }
            }
            
            // === BLACK FLARES with rainbow outlines ===
            for (int i = 0; i < 10; i++)
            {
                float flareX = (Main.GameUpdateCount * 0.5f + i * 250f + 500) % (Main.screenWidth + 80) - 40;
                float flareY = (Main.GameUpdateCount * 0.35f + i * 190f + 300) % (Main.screenHeight + 80) - 40;
                
                // Rainbow outline
                float hue = (i * 0.15f + Main.GameUpdateCount * 0.004f) % 1f;
                Color outlineColor = Main.hslToRgb(hue, 1f, 0.6f) * (0.8f * backgroundDarknessAlpha);
                
                int blackSize = 6 + (i % 3) * 2;
                // Rainbow outline glow
                spriteBatch.Draw(pixel, new Rectangle((int)flareX - blackSize - 2, (int)flareY - blackSize - 2, blackSize * 2 + 4, blackSize * 2 + 4), pixel.Bounds, outlineColor * 0.5f);
                // Black core
                spriteBatch.Draw(pixel, new Rectangle((int)flareX - blackSize, (int)flareY - blackSize, blackSize * 2, blackSize * 2), pixel.Bounds, Color.Black * backgroundDarknessAlpha);
            }
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        #region Epic Death Animation

        public override bool CheckDead()
        {
            if (!isDying)
            {
                // Start death animation instead of dying immediately
                isDying = true;
                deathTimer = 0;
                NPC.life = 1;
                NPC.dontTakeDamage = true;
                NPC.velocity = Vector2.Zero;
                
                SoundEngine.PlaySound(SoundID.Roar with { Pitch = -0.5f, Volume = 1.5f }, NPC.Center);
                
                return false; // Don't die yet
            }
            return true;
        }

        private void UpdateDeathAnimation(Player target)
        {
            deathTimer++;
            float progress = deathTimer / (float)DeathAnimationDuration;
            
            // Stop all movement
            NPC.velocity = Vector2.Zero;
            
            // Progressive screen whitening throughout the entire animation
            screenWhiteningAlpha = MathHelper.Lerp(0f, 0.95f, (float)Math.Pow(progress, 1.5f));
            
            // Phase 1: Initial buildup - Background intensifies (0-150 frames)
            // OPTIMIZED: Reduced particle spawning frequency
            if (deathTimer < 150)
            {
                float phase1Progress = deathTimer / 150f;
                
                backgroundDarknessAlpha = 1f;
                
                // Screen shake removed during death animation buildup
                
                // Rainbow elements - REDUCED frequency (every 6 frames instead of 2)
                if (deathTimer % 6 == 0)
                {
                    for (int i = 0; i < 4; i++) // Reduced from 10 to 4
                    {
                        Vector2 offset = Main.rand.NextVector2Circular(100f * (1f + phase1Progress), 100f * (1f + phase1Progress));
                        float hue = (Main.GameUpdateCount * 0.02f + i * 0.25f) % 1f;
                        Color baseColor = Main.hslToRgb(hue, 1f - phase1Progress * 0.5f, 0.7f + phase1Progress * 0.3f);
                        // Enhanced flare with multi-layer bloom
                        EnhancedParticles.BloomFlare(NPC.Center + offset, baseColor, 0.5f + phase1Progress * 0.5f, 25, 3, 0.8f);
                    }
                    
                    // Music notes with enhanced bloom
                    EnhancedThemedParticles.SwanLakeMusicNotesEnhanced(NPC.Center + Main.rand.NextVector2Circular(150f, 150f), 3, 40f);
                }
                
                // Initial lightning - less frequent
                if (deathTimer % 12 == 0)
                {
                    float lightningAngle = deathTimer * 0.08f;
                    Vector2 lightningStart = NPC.Center + new Vector2((float)Math.Cos(lightningAngle), (float)Math.Sin(lightningAngle)) * 80f;
                    Vector2 lightningEnd = lightningStart + new Vector2(Main.rand.NextFloat(-50f, 50f), Main.rand.NextFloat(50f, 150f));
                    MagnumVFX.DrawSwanLakeLightning(lightningStart, lightningEnd, 6, 20f, 2, 0.3f);
                }
                
                distortionIntensity = 0.3f + phase1Progress * 0.4f;
            }
            // Phase 2: Lightning spiral begins (150-300 frames) - OPTIMIZED
            else if (deathTimer < 300)
            {
                float phase2Progress = (deathTimer - 150f) / 150f;
                
                // Screen shake removed during death animation
                
                // Expanding glow - REDUCED (every 6 frames, fewer particles)
                if (deathTimer % 6 == 0)
                {
                    for (int i = 0; i < 6; i++) // Reduced from 16 to 6
                    {
                        float angle = MathHelper.TwoPi * i / 6f + deathTimer * 0.08f;
                        float radius = 60f + phase2Progress * 180f;
                        Vector2 pos = NPC.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                        Color glowColor = Color.Lerp(Main.hslToRgb((angle / MathHelper.TwoPi + deathTimer * 0.01f) % 1f, 0.8f, 0.8f), Color.White, phase2Progress * 0.6f);
                        // Enhanced with multi-layer bloom
                        EnhancedParticles.BloomFlare(pos, glowColor, 0.7f + phase2Progress * 0.5f, 18, 3, 0.85f);
                    }
                }
                
                // Lightning - REDUCED frequency and count
                if (deathTimer % 8 == 0)
                {
                    int lightningCount = 2 + (int)(phase2Progress * 3); // Reduced
                    
                    for (int i = 0; i < lightningCount; i++)
                    {
                        float baseAngle = MathHelper.TwoPi * i / lightningCount + deathTimer * 0.1f;
                        float angle = baseAngle + Main.rand.NextFloat(-0.5f, 0.5f);
                        
                        float startRadius = 50f + phase2Progress * 100f;
                        float endRadius = startRadius + 80f + phase2Progress * 150f;
                        
                        Vector2 lightningStart = NPC.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * startRadius;
                        Vector2 lightningEnd = NPC.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * endRadius;
                        
                        MagnumVFX.DrawSwanLakeLightning(lightningStart, lightningEnd, 6, 25f, 3, 0.5f);
                    }
                }
                
                // Music notes with enhanced bloom
                if (deathTimer % 10 == 0)
                {
                    EnhancedThemedParticles.SwanLakeMusicNotesEnhanced(NPC.Center + Main.rand.NextVector2Circular(100f, 100f), 4, 40f);
                }
                
                // Feathers - less frequent
                if (deathTimer % 8 == 0)
                {
                    CustomParticles.SwanFeatherDuality(NPC.Center, 8, 1f);
                }
                
                if (deathTimer == 200)
                {
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.8f, Volume = 1.3f }, NPC.Center);
                }
                
                distortionIntensity = 0.7f + phase2Progress * 0.5f;
            }
            // Phase 3: Chaotic lightning spiral (300-450 frames) - HEAVILY OPTIMIZED
            else if (deathTimer < 450)
            {
                float phase3Progress = (deathTimer - 300f) / 150f;
                
                // Screen shake removed during death animation
                
                // Lightning - SIGNIFICANTLY REDUCED
                if (deathTimer % 6 == 0)
                {
                    int lightningCount = 4 + (int)(phase3Progress * 4); // Reduced from 12+12 to 4+4
                    float maxRadius = 200f + phase3Progress * 400f;
                    
                    for (int i = 0; i < lightningCount; i++)
                    {
                        float baseAngle = MathHelper.TwoPi * i / lightningCount + deathTimer * 0.15f;
                        float angle = baseAngle + Main.rand.NextFloat(-1f, 1f);
                        
                        float startRadius = 30f + Main.rand.NextFloat(30f);
                        float endRadius = startRadius + maxRadius * 0.7f;
                        
                        Vector2 lightningStart = NPC.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * startRadius;
                        Vector2 lightningEnd = NPC.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * endRadius;
                        
                        MagnumVFX.DrawSwanLakeLightning(lightningStart, lightningEnd, 8, 35f, 3, 0.6f);
                    }
                    
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = Main.rand.NextFloat(-0.4f, 0.4f), Volume = 0.5f }, NPC.Center);
                }
                
                // Music notes with enhanced bloom
                if (deathTimer % 8 == 0)
                {
                    for (int n = 0; n < 3; n++) // Reduced from 8 to 3
                    {
                        float musicAngle = MathHelper.TwoPi * n / 3f + deathTimer * 0.12f;
                        float musicRadius = 60f + phase3Progress * 200f;
                        Vector2 notePos = NPC.Center + new Vector2((float)Math.Cos(musicAngle), (float)Math.Sin(musicAngle)) * musicRadius;
                        EnhancedThemedParticles.SwanLakeMusicNotesEnhanced(notePos, 4, 45f);
                    }
                }
                
                // Flares with enhanced bloom
                if (deathTimer % 4 == 0)
                {
                    for (int f = 0; f < 3; f++) // Reduced from 8 to 3
                    {
                        float hue = (deathTimer * 0.025f + f * 0.33f) % 1f;
                        Color flareColor = Color.Lerp(Main.hslToRgb(hue, 1f, 0.9f), Color.White, phase3Progress * 0.5f);
                        EnhancedParticles.BloomFlare(NPC.Center + Main.rand.NextVector2Circular(100f, 100f), flareColor, 0.8f, 12, 3, 0.9f);
                    }
                }
                
                // Feathers - reduced
                if (deathTimer % 10 == 0)
                {
                    CustomParticles.SwanFeatherExplosion(NPC.Center + Main.rand.NextVector2Circular(100f, 100f), 10, 1f);
                }
                
                distortionIntensity = 1.2f + phase3Progress * 0.5f;
            }
            // Phase 4: Final crescendo (450-570 frames) - HEAVILY OPTIMIZED
            else if (deathTimer < 570)
            {
                float phase4Progress = (deathTimer - 450f) / 120f;
                
                // Screen shake removed during death animation
                
                // Lightning - DRASTICALLY REDUCED
                if (deathTimer % 4 == 0)
                {
                    int lightningCount = 6; // Reduced from 24
                    float maxRadius = 500f + phase4Progress * 300f;
                    
                    for (int i = 0; i < lightningCount; i++)
                    {
                        float chaosAngle = MathHelper.TwoPi * i / lightningCount + deathTimer * 0.2f + Main.rand.NextFloat(-1f, 1f);
                        
                        Vector2 lightningStart = NPC.Center + new Vector2((float)Math.Cos(chaosAngle), (float)Math.Sin(chaosAngle)) * Main.rand.NextFloat(20f, 60f);
                        Vector2 lightningEnd = NPC.Center + new Vector2((float)Math.Cos(chaosAngle), (float)Math.Sin(chaosAngle)) * maxRadius;
                        
                        MagnumVFX.DrawSwanLakeLightning(lightningStart, lightningEnd, 10, 50f, 4, 0.8f);
                    }
                }
                
                // Thunder sounds - less frequent
                if (deathTimer % 20 == 0)
                {
                    SoundEngine.PlaySound(SoundID.Thunder with { Pitch = Main.rand.NextFloat(-0.5f, 0.3f), Volume = 0.9f }, NPC.Center);
                }
                
                // Music notes with enhanced bloom
                if (deathTimer % 6 == 0)
                {
                    for (int n = 0; n < 4; n++) // Reduced from 12 to 4
                    {
                        float noteAngle = MathHelper.TwoPi * n / 4f + deathTimer * 0.15f;
                        float noteRadius = 100f + phase4Progress * 200f;
                        Vector2 notePos = NPC.Center + new Vector2((float)Math.Cos(noteAngle), (float)Math.Sin(noteAngle)) * noteRadius;
                        EnhancedThemedParticles.SwanLakeMusicNotesEnhanced(notePos, 5, 50f);
                    }
                }
                
                // Core flares with enhanced bloom
                if (deathTimer % 4 == 0)
                {
                    for (int c = 0; c < 4; c++) // Reduced from 15 to 4
                    {
                        float hue = (deathTimer * 0.03f + c * 0.25f) % 1f;
                        Color coreColor = Color.Lerp(Main.hslToRgb(hue, 0.8f, 0.95f), Color.White, phase4Progress * 0.7f);
                        EnhancedParticles.BloomFlare(NPC.Center + Main.rand.NextVector2Circular(80f, 80f), coreColor, 1f, 15, 4, 1f);
                    }
                }
                
                // Feathers - reduced
                if (deathTimer % 8 == 0)
                {
                    CustomParticles.SwanFeatherExplosion(NPC.Center, 12, 1.2f);
                }
                
                distortionIntensity = 1.7f - phase4Progress * 0.3f;
                
                // Core lighting
                Lighting.AddLight(NPC.Center, 3f + phase4Progress, 3f + phase4Progress, 3.5f + phase4Progress);
            }
            // Phase 5: Final supernova burst (570-600 frames) - OPTIMIZED
            else
            {
                float phase5Progress = (deathTimer - 570f) / 30f;
                
                // Initial supernova burst - ONE TIME only
                if (deathTimer == 570)
                {
                    SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.8f, Volume = 1.8f }, NPC.Center);
                    SoundEngine.PlaySound(SoundID.Thunder with { Pitch = -0.6f, Volume = 1.5f }, NPC.Center);
                    EroicaScreenShake.LargeShake(NPC.Center);
                    
                    // Supernova burst - REDUCED from 60 to 24
                    for (int i = 0; i < 24; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 24f;
                        Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                        
                        Dust white = Dust.NewDustPerfect(NPC.Center, DustID.WhiteTorch, vel * 25f, 0, Color.White * 0.8f, 3.5f);
                        white.noGravity = true;
                        white.fadeIn = 1.8f;
                    }
                    
                    // Wing-shaped supernova with enhanced bloom - REDUCED from 40+40 to 15+15
                    for (int i = 0; i < 15; i++)
                    {
                        float wingAngle = MathHelper.PiOver2 + Main.rand.NextFloat(-0.8f, 0.8f);
                        Vector2 wingVel = new Vector2((float)Math.Cos(wingAngle), (float)Math.Sin(wingAngle)) * Main.rand.NextFloat(15f, 35f);
                        wingVel.X *= 2.5f;
                        Color wingColor = Main.hslToRgb(Main.rand.NextFloat(), 0.5f, 0.9f);
                        EnhancedParticles.BloomFlare(NPC.Center + wingVel * 0.5f, wingColor, 0.8f, 30, 4, 1f);
                    }
                    
                    for (int i = 0; i < 15; i++)
                    {
                        float wingAngle = -MathHelper.PiOver2 + Main.rand.NextFloat(-0.8f, 0.8f);
                        Vector2 wingVel = new Vector2((float)Math.Cos(wingAngle), (float)Math.Sin(wingAngle)) * Main.rand.NextFloat(15f, 35f);
                        wingVel.X *= 2.5f;
                        Color wingColor = Main.hslToRgb(Main.rand.NextFloat(), 0.5f, 0.9f);
                        EnhancedParticles.BloomFlare(NPC.Center + wingVel * 0.5f, wingColor, 0.8f, 30, 4, 1f);
                    }
                    
                    // Eye center glow with enhanced bloom - REDUCED
                    CustomParticles.ExplosionBurst(NPC.Center, Color.White * 0.8f, 15, 18f);
                    UnifiedVFXBloom.SwanLake.ExplosionEnhanced(NPC.Center, 2.5f);
                    EnhancedThemedParticles.SwanLakeBloomBurstEnhanced(NPC.Center, 2f);
                }
                
                // Fading residual effects - LESS FREQUENT
                if (deathTimer % 8 == 0)
                {
                    for (int i = 0; i < 2; i++) // Reduced from 6 to 2
                    {
                        Vector2 lightningStart = NPC.Center + Main.rand.NextVector2Circular(200f, 200f);
                        Vector2 lightningEnd = lightningStart + new Vector2(Main.rand.NextFloat(-100f, 100f), Main.rand.NextFloat(100f, 250f));
                        MagnumVFX.DrawSwanLakeLightning(lightningStart, lightningEnd, 6, 30f, 2, 0.4f * (1f - phase5Progress));
                    }
                }
                
                // Screen shake removed during death fade-out
                
                // Feathers - less frequent
                if (deathTimer % 10 == 0)
                {
                    CustomParticles.SwanFeatherExplosion(NPC.Center + Main.rand.NextVector2Circular(150f, 150f), 6, 0.8f * (1f - phase5Progress));
                }
                
                distortionIntensity = 1.4f * (1f - phase5Progress);
                
                Lighting.AddLight(NPC.Center, 2.5f * (1f - phase5Progress), 2.5f * (1f - phase5Progress), 3f * (1f - phase5Progress));
            }
            
            // Actually die at the end
            if (deathTimer >= DeathAnimationDuration)
            {
                // Death dialogue
                BossDialogueSystem.SwanLake.OnDeath();
                BossDialogueSystem.CleanupDialogue(NPC.whoAmI);
                
                // Set boss downed flag for miniboss essence drops
                MoonlightSonataSystem.DownedSwanLake = true;
                if (Main.netMode == NetmodeID.Server)
                    NetMessage.SendData(MessageID.WorldData);
                
                NPC.life = 0;
                NPC.checkDead();
                
                // Final explosion - REDUCED from 80 to 30
                for (int i = 0; i < 30; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(18f, 18f);
                    Color col = Main.rand.NextBool() ? Color.White * 0.8f : Main.hslToRgb(Main.rand.NextFloat(), 0.6f, 0.85f);
                    Dust d = Dust.NewDustPerfect(NPC.Center, DustID.WhiteTorch, vel, 0, col, 2.5f);
                    d.noGravity = true;
                    d.fadeIn = 1.5f;
                }
            }
        }

        #endregion

        [Obsolete]
        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ItemID.SuperHealingPotion; // Upgraded to Super Healing for near-Fate tier
        }

        public override void OnKill()
        {
            // Death message is handled by BossDialogueSystem.SwanLake.OnDeath() in UpdateDeathAnimation
            // Multiplayer broadcast for legacy support
            if (Main.netMode == NetmodeID.Server)
            {
                Terraria.Chat.ChatHelper.BroadcastChatMessage(
                    Terraria.Localization.NetworkText.FromLiteral("The Swan's dance... concludes."),
                    new Color(255, 240, 255));
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // Expert/Master mode treasure bag
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<SwanLakeTreasureBag>()));
            
            // Normal mode: Drop 1-2 Swan Lake weapons directly
            // These are class-based weapons from the Swan Lake tier
            LeadingConditionRule normalModeRule = new LeadingConditionRule(new Conditions.NotExpert());
            
            // Normal mode: Swan's Resonance Energy (15-20, half of Expert mode 20-25)
            normalModeRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<ResonanceEnergies.SwansResonanceEnergy>(), 1, 15, 20));
            
            // Normal mode: Remnant of Swan's Harmony (20-25, half of Expert mode 30-35)
            normalModeRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<ResonanceEnergies.RemnantOfSwansHarmony>(), 1, 20, 25));
            
            // 1-2 random weapons from Swan Lake weapon pool
            int[] swanWeapons = new int[]
            {
                ModContent.ItemType<ResonantWeapons.CalloftheBlackSwan>(),           // Melee
                ModContent.ItemType<ResonantWeapons.TheSwansLament>(),               // Melee
                ModContent.ItemType<ResonantWeapons.IridescentWingspan>(),           // Ranger
                ModContent.ItemType<ResonantWeapons.FeatheroftheIridescentFlock>(),  // Magic
                ModContent.ItemType<ResonantWeapons.ChromaticSwanSong>(),            // Summoner
                ModContent.ItemType<ResonantWeapons.CallofthePearlescentLake>(),     // Magic
            };
            
            // Always drop 1 weapon
            normalModeRule.OnSuccess(ItemDropRule.OneFromOptions(1, swanWeapons));
            
            // 50% chance to drop a second weapon
            normalModeRule.OnSuccess(ItemDropRule.OneFromOptionsNotScalingWithLuck(2, swanWeapons));
            
            npcLoot.Add(normalModeRule);
            
            // Feather's Call - Now EXPERT/MASTER only (dropped from treasure bag)
            // Removed from normal mode direct drop - see SwanLakeTreasureBag.cs
            
            // Normal mode: 5-10 Shard of the Feathered Tempo (half of Expert mode 10-20)
            LeadingConditionRule normalShardRule = new LeadingConditionRule(new Conditions.NotExpert());
            normalShardRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<ResonanceEnergies.ShardOfTheFeatheredTempo>(), 1, 5, 10));
            npcLoot.Add(normalShardRule);
        }
    }
}
