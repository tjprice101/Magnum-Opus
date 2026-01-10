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

namespace MagnumOpus.Content.SwanLake.Bosses
{
    /// <summary>
    /// Swan Lake, The Monochromatic Fractal - An elegant yet foreboding celestial deity boss.
    /// Features 6 swan wings, detached limbs held by rainbow flames, and spectacular attacks.
    /// </summary>
    public class SwanLakeTheMonochromaticFractal : ModNPC
    {
        // Idle sprite
        public override string Texture => "MagnumOpus/Content/SwanLake/Bosses/SwanLakeTheMonochromaticFractal";
        
        // Attack sprite path
        private const string AttackTexture = "MagnumOpus/Content/SwanLake/Bosses/SwanLakeTheMonochromaticFractal_Attack";
        
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
            // Attack 5 - Ultimate: Monochromatic Apocalypse
            ApocalypseWindup,
            ApocalypseAttack
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
            NPC.width = 140;
            NPC.height = 180;
            NPC.damage = 130; // Higher than Eroica's 90
            NPC.defense = 100; // Higher than Eroica's 80
            NPC.lifeMax = 600000; // Significantly higher than Eroica's 406,306
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath14;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = Item.buyPrice(gold: 30);
            NPC.boss = true;
            NPC.npcSlots = 20f;
            NPC.aiStyle = -1;
            NPC.scale = 0.8f;
            
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
            NPC.TargetClosest(true);
            Player target = Main.player[NPC.target];

            // Despawn check
            if (!target.active || target.dead)
            {
                NPC.velocity.Y -= 0.5f;
                backgroundDarknessAlpha = MathHelper.Lerp(backgroundDarknessAlpha, 0f, 0.05f);
                NPC.EncourageDespawn(60);
                return;
            }

            // Update timers
            Timer++;
            pulseTimer += 0.04f;
            
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
            }

            // Face player
            NPC.spriteDirection = NPC.direction = (target.Center.X > NPC.Center.X) ? 1 : -1;
        }

        private void SpawnAmbientParticles()
        {
            // Swan feathers floating around - black and white
            if (Main.rand.NextBool(4))
            {
                Vector2 offset = Main.rand.NextVector2Circular(NPC.width * 0.6f, NPC.height * 0.6f);
                Color featherColor = Main.rand.NextBool() ? Color.White : Color.Black;
                CustomParticles.SwanFeatherDrift(NPC.Center + offset, featherColor, 0.3f);
            }
            
            // Occasional monochrome sparkles with rainbow shimmer
            if (Main.rand.NextBool(6))
            {
                Vector2 offset = Main.rand.NextVector2Circular(NPC.width * 0.7f, NPC.height * 0.7f);
                // Mostly white/gray, occasionally rainbow
                Color sparkleColor = Main.rand.NextBool(5) 
                    ? Main.hslToRgb(Main.rand.NextFloat(), 0.8f, 0.7f) 
                    : (Main.rand.NextBool() ? Color.White : Color.Silver);
                CustomParticles.PrismaticSparkle(NPC.Center + offset, sparkleColor, 0.2f);
            }
            
            // Pearlescent rainbow flame wisps from detached limbs (kept as rainbow for the "held by rainbow flames" theme)
            if (Main.rand.NextBool(3))
            {
                Vector2 offset = Main.rand.NextVector2Circular(NPC.width * 0.5f, NPC.height * 0.5f);
                Color rainbowFlame = Main.hslToRgb((Main.GameUpdateCount * 0.002f) % 1f, 0.6f, 0.7f);
                CustomParticles.GenericFlare(NPC.Center + offset, rainbowFlame * 0.4f, 0.15f, 12);
            }
        }

        private void IdleHover(Player target)
        {
            isUsingAttackSprite = false;
            
            // Elegant hovering movement
            float hoverX = (float)Math.Sin(pulseTimer * 1.5f) * 50f;
            float hoverY = (float)Math.Sin(pulseTimer * 2f) * 30f;
            
            Vector2 hoverPosition = target.Center + new Vector2(hoverX, -350 + hoverY);
            Vector2 direction = hoverPosition - NPC.Center;
            float distance = direction.Length();

            if (distance > 20f)
            {
                direction.Normalize();
                NPC.velocity = Vector2.Lerp(NPC.velocity, direction * 8f, 0.05f);
            }
            else
            {
                NPC.velocity *= 0.92f;
            }

            // Choose next attack after hovering
            if (Timer > 80)
            {
                Timer = 0;
                AttackPhase = 0;
                
                // Attack weighting:
                // 35% - Attack 1 (Feather Cascade)
                // 35% - Attack 2 (Prismatic Ring)
                // 15% - Attack 3 (Dual Slash)
                // 10% - Attack 4 (Lightning Storm)
                // 5% - Attack 5 (Apocalypse)
                
                int roll = Main.rand.Next(100);
                
                if (roll < 35) // 0-34 = 35%
                {
                    State = ActionState.FeatherCascadeWindup;
                }
                else if (roll < 70) // 35-69 = 35%
                {
                    State = ActionState.PrismaticRingWindup;
                }
                else if (roll < 85) // 70-84 = 15%
                {
                    State = ActionState.DualSlashWindup;
                }
                else if (roll < 95) // 85-94 = 10%
                {
                    State = ActionState.LightningStormWindup;
                }
                else // 95-99 = 5%
                {
                    State = ActionState.ApocalypseWindup;
                }
                
                NPC.netUpdate = true;
            }
        }

        #region Attack 1: Feather Cascade (Easy)

        private void FeatherCascadeWindup(Player target)
        {
            const int WindupTime = 30;
            isUsingAttackSprite = true;
            
            NPC.velocity *= 0.9f;
            
            if (Timer == 1)
            {
                SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0.3f }, NPC.Center);
            }
            
            // Gathering feathers - black and white spiral
            if (Timer % 3 == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 offset = Main.rand.NextVector2Circular(100f, 100f);
                    Color spiralColor = i % 2 == 0 ? Color.White : Color.Black;
                    CustomParticles.SwanFeatherSpiral(NPC.Center + offset, spiralColor, 4);
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
            const int AttackDuration = 90;
            isUsingAttackSprite = true;
            
            NPC.velocity *= 0.95f;
            
            // Spawn cascading feathers - black and white with rainbow impacts
            if (Timer % 6 == 0 && Timer < AttackDuration - 20)
            {
                SoundEngine.PlaySound(SoundID.Item1 with { Pitch = 0.5f, Volume = 0.3f }, NPC.Center);
                
                // Spray of feathers downward in a spread
                for (int i = 0; i < 5; i++)
                {
                    float angle = MathHelper.ToRadians(-60 + i * 30);
                    Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 8f;
                    Vector2 featherPos = NPC.Center + new Vector2(0, 40) + velocity * 5f;
                    
                    // Alternate black and white feathers
                    Color featherColor = i % 2 == 0 ? Color.White : Color.Black;
                    CustomParticles.SwanFeatherBurst(featherPos, 3, 0.4f);
                    
                    // Pearlescent rainbow explosion on spawn
                    Color rainbowColor = Main.hslToRgb((Timer * 0.03f + i * 0.2f) % 1f, 0.9f, 0.7f);
                    CustomParticles.PrismaticSparkleBurst(featherPos, rainbowColor, 2);
                    
                    // Dust feathers - monochrome
                    for (int j = 0; j < 3; j++)
                    {
                        Dust feather = Dust.NewDustDirect(NPC.Center, 20, 20, DustID.Cloud, velocity.X, velocity.Y, 100, featherColor, 1.5f);
                        feather.noGravity = true;
                    }
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

        #region Attack 2: Prismatic Sparkle Ring (Easy)

        private void PrismaticRingWindup(Player target)
        {
            const int WindupTime = 25;
            isUsingAttackSprite = true;
            
            NPC.velocity *= 0.88f;
            
            if (Timer == 1)
            {
                SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.2f }, NPC.Center);
            }
            
            // Monochrome energy gathering with rainbow hints
            if (Timer % 2 == 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 offset = Main.rand.NextVector2CircularEdge(80f, 80f);
                    // Mostly white/black with occasional rainbow sparkle
                    Color baseColor = Main.rand.NextBool() ? Color.White : Color.Gray;
                    CustomParticles.PrismaticSparkle(NPC.Center + offset, baseColor, 0.3f);
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
            const int AttackDuration = 60;
            isUsingAttackSprite = true;
            
            NPC.velocity *= 0.92f;
            
            // Spawn rings of black/white sparkles with rainbow explosion bursts
            if (Timer % 10 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item9 with { Volume = 0.5f }, NPC.Center);
                
                int sparkleCount = 16;
                float radius = 100f + (Timer / 10f) * 50f;
                
                for (int i = 0; i < sparkleCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / sparkleCount;
                    Vector2 position = NPC.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                    
                    // Alternate black and white particles in ring
                    Color monoColor = i % 2 == 0 ? Color.White : new Color(30, 30, 30);
                    CustomParticles.PrismaticSparkleBurst(position, monoColor, 3);
                    
                    // Pearlescent rainbow explosion at each point
                    Color rainbowColor = Main.hslToRgb(angle / MathHelper.TwoPi, 0.9f, 0.7f);
                    CustomParticles.GenericFlare(position, rainbowColor * 0.6f, 0.25f, 10);
                    
                    Dust sparkle = Dust.NewDustPerfect(position, DustID.Cloud, Vector2.Zero, 0, monoColor, 1.2f);
                    sparkle.noGravity = true;
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

        #region Attack 3: Dual Swan Arc Slashes (Medium)

        private void DualSlashWindup(Player target)
        {
            const int WindupTime = 40;
            isUsingAttackSprite = true;
            
            NPC.velocity *= 0.85f;
            
            if (Timer == 1)
            {
                SoundEngine.PlaySound(SoundID.Item71 with { Pitch = -0.2f }, NPC.Center);
            }
            
            // Building energy with feathers and arcs
            if (Timer % 4 == 0)
            {
                Vector2 offset = Main.rand.NextVector2Circular(120f, 120f);
                CustomParticles.SwordArcWave(NPC.Center + offset, Vector2.Zero, Color.White * 0.5f, 0.3f);
                CustomParticles.SwanFeatherAura(NPC.Center, 40f, 3);
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
            const int SlashInterval = 25;
            const int TotalSlashes = 6; // 3 pairs
            isUsingAttackSprite = true;
            
            NPC.velocity *= 0.93f;
            
            // Fire dual slashes
            if (Timer == 1 && AttackPhase < TotalSlashes)
            {
                SoundEngine.PlaySound(SoundID.Item1 with { Pitch = 0.3f }, NPC.Center);
                
                // Dual arc slashes - one black, one white
                Vector2 toPlayer = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
                float baseAngle = toPlayer.ToRotation();
                
                // Black swan slash (left)
                Vector2 leftDir = baseAngle.ToRotationVector2().RotatedBy(MathHelper.ToRadians(-20));
                CustomParticles.SwordArcCrescent(NPC.Center, leftDir * 15f, Color.Black, 0.8f);
                CustomParticles.SwanFeatherExplosion(NPC.Center + leftDir * 50, 8, 0.5f);
                // Rainbow explosion at black slash impact
                Color leftRainbow = Main.hslToRgb((AttackPhase * 0.15f) % 1f, 1f, 0.7f);
                CustomParticles.PrismaticSparkleBurst(NPC.Center + leftDir * 80, leftRainbow, 5);
                
                // White swan slash (right)
                Vector2 rightDir = baseAngle.ToRotationVector2().RotatedBy(MathHelper.ToRadians(20));
                CustomParticles.SwordArcCrescent(NPC.Center, rightDir * 15f, Color.White, 0.8f);
                CustomParticles.SwanFeatherExplosion(NPC.Center + rightDir * 50, 8, 0.5f);
                // Rainbow explosion at white slash impact
                Color rightRainbow = Main.hslToRgb((AttackPhase * 0.15f + 0.5f) % 1f, 1f, 0.7f);
                CustomParticles.PrismaticSparkleBurst(NPC.Center + rightDir * 80, rightRainbow, 5);
                
                // Visual effects - monochrome dust
                for (int i = 0; i < 20; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(10f, 10f);
                    Color dustColor = i % 2 == 0 ? Color.White : new Color(40, 40, 40);
                    Dust slash = Dust.NewDustDirect(NPC.Center, 30, 30, DustID.Cloud, vel.X, vel.Y, 100, dustColor, 2f);
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

        private void LightningStormWindup(Player target)
        {
            const int WindupTime = 50;
            isUsingAttackSprite = true;
            
            NPC.velocity *= 0.82f;
            
            if (Timer == 1)
            {
                SoundEngine.PlaySound(SoundID.Thunder, NPC.Center);
                EroicaScreenShake.MediumShake(NPC.Center);
            }
            
            // Crackling monochrome energy buildup with rainbow edges
            if (Timer % 3 == 0)
            {
                Vector2 offset = Main.rand.NextVector2Circular(150f, 150f);
                // White/silver lightning core
                CustomParticles.GenericFlare(NPC.Center + offset, Color.White * 0.6f, 0.2f, 10);
                
                Dust lightning = Dust.NewDustDirect(NPC.Center + offset, 1, 1, DustID.Cloud, 0, 0, 100, Color.White, 1.5f);
                lightning.noGravity = true;
                
                // Rainbow shimmer outline
                if (Main.rand.NextBool(3))
                {
                    Color rainbowEdge = Main.hslToRgb(Main.rand.NextFloat(), 0.8f, 0.6f);
                    CustomParticles.PrismaticSparkle(NPC.Center + offset, rainbowEdge, 0.15f);
                }
            }
            
            if (Timer >= WindupTime)
            {
                Timer = 0;
                AttackPhase = 0;
                State = ActionState.LightningStormAttack;
                NPC.netUpdate = true;
            }
        }

        private void LightningStormAttack(Player target)
        {
            const int AttackDuration = 120;
            isUsingAttackSprite = true;
            
            NPC.velocity *= 0.96f;
            
            // Spawn monochrome fractal lightning with rainbow impact explosions
            if (Timer % 8 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.5f, Volume = 0.6f }, NPC.Center);
                
                // Random target position near player
                Vector2 strikePos = target.Center + Main.rand.NextVector2Circular(200f, 200f);
                
                // White fractal lightning from boss to strike point
                MagnumVFX.DrawMoonlightLightning(NPC.Center, strikePos, 6, 25f, 0, 0f);
                
                // Black and white core impact
                CustomParticles.ExplosionBurst(strikePos, Color.White, 10, 8f);
                CustomParticles.ExplosionBurst(strikePos, Color.Black, 6, 5f);
                CustomParticles.SwanFeatherBurst(strikePos, 5, 0.4f);
                
                // Pearlescent rainbow explosion ring
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 30f;
                    Color rainbowColor = Main.hslToRgb(i / 8f, 1f, 0.7f);
                    CustomParticles.PrismaticSparkleBurst(strikePos + offset, rainbowColor, 3);
                }
                
                // Monochrome dust cloud
                for (int i = 0; i < 15; i++)
                {
                    Color dustColor = Main.rand.NextBool() ? Color.White : new Color(40, 40, 40);
                    Dust cloud = Dust.NewDustDirect(strikePos, 30, 30, DustID.Cloud, Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f), 100, dustColor, 1.8f);
                    cloud.noGravity = true;
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

        #region Attack 5: Monochromatic Apocalypse (Ultimate)

        private void ApocalypseWindup(Player target)
        {
            const int WindupTime = 80;
            isUsingAttackSprite = true;
            
            NPC.velocity *= 0.75f;
            
            if (Timer == 1)
            {
                SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
                EroicaScreenShake.LargeShake(NPC.Center);
                Main.NewText("Swan Lake prepares her ultimate attack!", 255, 255, 255);
            }
            
            // Massive monochrome energy buildup with rainbow edges
            if (Timer % 2 == 0)
            {
                Vector2 offset = Main.rand.NextVector2Circular(200f, 200f);
                
                // Black and white flares
                Color monoColor = Main.rand.NextBool() ? Color.White : new Color(30, 30, 30);
                CustomParticles.GenericFlare(NPC.Center + offset, monoColor * 0.8f, 0.4f, 20);
                
                // Feather storm - black and white
                Color featherColor = Main.rand.NextBool() ? Color.White : Color.Black;
                CustomParticles.SwanFeatherSpiral(NPC.Center + offset, featherColor, 6);
                
                // Rainbow pearlescent sparkle edges
                Color rainbowEdge = Main.hslToRgb((Timer * 0.02f) % 1f, 0.9f, 0.7f);
                CustomParticles.PrismaticSparkleBurst(NPC.Center + Main.rand.NextVector2Circular(150f, 150f), rainbowEdge, 3);
            }
            
            if (Timer >= WindupTime)
            {
                Timer = 0;
                AttackPhase = 0;
                State = ActionState.ApocalypseAttack;
                SoundEngine.PlaySound(SoundID.DD2_BetsyWindAttack, NPC.Center);
                EroicaScreenShake.Phase2EnrageShake(NPC.Center);
                NPC.netUpdate = true;
            }
        }

        private void ApocalypseAttack(Player target)
        {
            const int AttackDuration = 180;
            isUsingAttackSprite = true;
            
            NPC.velocity *= 0.98f;
            
            // Massive combined attack - everything at once!
            
            // 1. Continuous feather explosions
            if (Timer % 5 == 0)
            {
                Vector2 pos = target.Center + Main.rand.NextVector2Circular(300f, 300f);
                CustomParticles.SwanFeatherExplosion(pos, 12, 0.7f);
                CustomParticles.SwanFeatherBurst(pos, 8, 0.6f);
            }
            
            // 2. Monochrome sparkle rings with rainbow explosion centers
            if (Timer % 15 == 0)
            {
                int sparkleCount = 24;
                float radius = 150f + (Timer % 60) * 3f;
                
                for (int i = 0; i < sparkleCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / sparkleCount;
                    Vector2 position = NPC.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                    
                    // Alternating black/white sparkles
                    Color monoColor = i % 2 == 0 ? Color.White : new Color(30, 30, 30);
                    CustomParticles.PrismaticSparkleBurst(position, monoColor, 5);
                    
                    // Rainbow explosion at each point
                    Color rainbowColor = Main.hslToRgb(angle / MathHelper.TwoPi, 1f, 0.7f);
                    CustomParticles.GenericFlare(position, rainbowColor * 0.6f, 0.35f, 12);
                }
            }
            
            // 3. Arc slashes in all directions
            if (Timer % 20 == 0)
            {
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                    
                    Color slashColor = i % 2 == 0 ? Color.White : Color.Black;
                    CustomParticles.SwordArcCrescent(NPC.Center, direction * 18f, slashColor, 1f);
                    CustomParticles.SwordArcBurst(NPC.Center + direction * 80, slashColor * 0.8f, 4, 0.6f);
                }
                
                SoundEngine.PlaySound(SoundID.Item1 with { Pitch = 0.2f }, NPC.Center);
            }
            
            // 4. Monochrome fractal lightning with rainbow impact
            if (Timer % 10 == 0)
            {
                Vector2 strikePos = target.Center + Main.rand.NextVector2Circular(250f, 250f);
                MagnumVFX.DrawMoonlightLightning(NPC.Center, strikePos, 8, 30f, 0, 0f);
                
                // White and black impact
                CustomParticles.ExplosionBurst(strikePos, Color.White, 12, 10f);
                CustomParticles.ExplosionBurst(strikePos, new Color(30, 30, 30), 8, 6f);
                
                // Rainbow ring explosion
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f;
                    Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 40f;
                    Color rainbowColor = Main.hslToRgb(i / 6f, 1f, 0.7f);
                    CustomParticles.PrismaticSparkleBurst(strikePos + offset, rainbowColor, 4);
                }
            }
            
            // 5. Monochrome explosions with rainbow pearlescent edges
            if (Timer % 12 == 0)
            {
                Vector2 explosionPos = target.Center + Main.rand.NextVector2Circular(280f, 280f);
                
                // Core black/white explosion
                Color coreColor = Main.rand.NextBool() ? Color.White : new Color(20, 20, 20);
                CustomParticles.GenericFlare(explosionPos, coreColor, 0.8f, 25);
                CustomParticles.ExplosionBurst(explosionPos, Color.White, 15, 10f);
                CustomParticles.ExplosionBurst(explosionPos, Color.Black, 10, 7f);
                
                // Pearlescent rainbow ring around explosion
                Color rainbowRing = Main.hslToRgb((Timer * 0.05f) % 1f, 1f, 0.7f);
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 50f;
                    CustomParticles.PrismaticSparkleBurst(explosionPos + offset, Main.hslToRgb((Timer * 0.05f + i * 0.125f) % 1f, 1f, 0.7f), 3);
                }
                
                CustomParticles.SwanFeatherDuality(explosionPos, 8, 0.8f);
            }
            
            // 6. Black/white duality spirals with rainbow accents
            if (Timer % 8 == 0)
            {
                CustomParticles.SwanFeatherDuality(NPC.Center, 6, 0.6f);
                Color spiralColor = Main.rand.NextBool() ? Color.White : Color.Black;
                CustomParticles.SwanFeatherSpiral(NPC.Center + Main.rand.NextVector2Circular(200f, 200f), spiralColor, 6);
                
                // Rainbow pearlescent accent
                Color accentColor = Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.7f);
                CustomParticles.PrismaticSparkle(NPC.Center + Main.rand.NextVector2Circular(150f, 150f), accentColor, 0.3f);
            }
            
            // Screen shake throughout
            if (Timer % 15 == 0)
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
            Vector2 origin = new Vector2(frameWidth / 2f, frameHeight / 2f);
            SpriteEffects effects = NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // Pulse effect
            float pulse = 1f + (float)Math.Sin(pulseTimer * 3f) * 0.05f;
            
            // Draw glow layers (additive)
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Rainbow pearlescent glow
            Color glowColor = Main.hslToRgb((Main.GameUpdateCount * 0.005f) % 1f, 0.5f, 0.6f) * 0.4f;
            spriteBatch.Draw(texture, position, sourceRect, glowColor, NPC.rotation, origin, NPC.scale * pulse * 1.15f, effects, 0f);
            
            // White/silver outer glow
            spriteBatch.Draw(texture, position, sourceRect, Color.White * 0.3f, NPC.rotation, origin, NPC.scale * pulse * 1.25f, effects, 0f);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Draw main sprite
            spriteBatch.Draw(texture, position, sourceRect, Color.White, NPC.rotation, origin, NPC.scale, effects, 0f);
            
            return false;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            // Background is now drawn in PreDraw, nothing needed here
        }

        private void DrawBackgroundDarkness(SpriteBatch spriteBatch)
        {
            // Draw black overlay over entire screen
            Rectangle screenRect = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
            
            // Black background
            spriteBatch.Draw(pixel, screenRect, pixel.Bounds, Color.Black * (backgroundDarknessAlpha * 0.7f));
            
            // Rainbow sparkles floating across screen
            for (int i = 0; i < 30; i++)
            {
                float sparkleX = (Main.GameUpdateCount * 0.5f + i * 100f) % Main.screenWidth;
                float sparkleY = (Main.GameUpdateCount * 0.3f + i * 150f) % Main.screenHeight;
                Color sparkleColor = Main.hslToRgb((i * 0.1f + Main.GameUpdateCount * 0.001f) % 1f, 0.9f, 0.7f);
                
                Vector2 sparklePos = new Vector2(sparkleX, sparkleY);
                spriteBatch.Draw(pixel, new Rectangle((int)sparkleX, (int)sparkleY, 2, 2), pixel.Bounds, sparkleColor * backgroundDarknessAlpha);
            }
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        [Obsolete]
        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ItemID.GreaterHealingPotion;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // Expert/Master mode treasure bag
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<SwanLakeTreasureBag>()));
            
            // TODO: Add normal mode loot items when created
        }
    }
}
