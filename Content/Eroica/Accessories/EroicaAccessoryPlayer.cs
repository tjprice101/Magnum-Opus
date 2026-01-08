using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.DataStructures;
using ReLogic.Content;
using static Terraria.ModLoader.PlayerDrawLayer;

namespace MagnumOpus.Content.Eroica.Accessories
{
    /// <summary>
    /// ModPlayer class that handles all Eroica accessory effects.
    /// </summary>
    public class EroicaAccessoryPlayer : ModPlayer
    {
        // ========== PYRE OF THE FALLEN HERO (Melee) ==========
        public bool hasPyreOfTheFallenHero = false;
        public int furyStacks = 0;
        private const int MaxFuryStacks = 12;
        public int damageBoostTimer = 0;
        private const int DamageBoostDuration = 120; // 2 seconds
        
        // ========== SAKURA'S BURNING WILL (Summoner) ==========
        public bool hasSakurasBurningWill = false;
        public int heroicSpiritTimer = 0;
        private const int HeroicSpiritInterval = 720; // 12 seconds
        
        // ========== FUNERAL MARCH INSIGNIA (Mage) ==========
        public bool hasFuneralMarchInsignia = false;
        public int heroicEncoreCooldown = 0;
        private const int HeroicEncoreCooldownMax = 10800; // 180 seconds (3 minutes)
        public bool heroicEncoreActive = false;
        public int heroicEncoreTimer = 0;
        private const int HeroicEncoreDuration = 180; // 3 seconds of invulnerability
        
        // ========== SYMPHONY OF SCARLET FLAMES (Ranger) ==========
        public bool hasSymphonyOfScarletFlames = false;
        public int lastTargetHit = -1;
        public int consecutiveHits = 0;
        
        // ========== FLOATING VISUAL ==========
        public float floatAngle = 0f;
        
        public override void ResetEffects()
        {
            hasPyreOfTheFallenHero = false;
            hasSakurasBurningWill = false;
            hasFuneralMarchInsignia = false;
            hasSymphonyOfScarletFlames = false;
        }
        
        public override void PostUpdate()
        {
            // ========== FLOATING VISUAL ANGLE ==========
            if (hasFuneralMarchInsignia || hasSakurasBurningWill)
                floatAngle += 0.03f;
            
            // ========== PYRE OF THE FALLEN HERO ==========
            if (damageBoostTimer > 0)
                damageBoostTimer--;
            
            // Reset fury if not wearing accessory
            if (!hasPyreOfTheFallenHero)
                furyStacks = 0;
            
            // ========== SAKURA'S BURNING WILL ==========
            if (hasSakurasBurningWill)
            {
                heroicSpiritTimer++;
                if (heroicSpiritTimer >= HeroicSpiritInterval)
                {
                    heroicSpiritTimer = 0;
                    SummonHeroicSpirit();
                }
                
                // Check proximity to minions for defense bonus
                UpdateMinionProximityBonus();
            }
            else
            {
                heroicSpiritTimer = 0;
            }
            
            // ========== FUNERAL MARCH INSIGNIA ==========
            if (heroicEncoreCooldown > 0)
                heroicEncoreCooldown--;
            
            if (heroicEncoreActive)
            {
                heroicEncoreTimer++;
                Player.immune = true;
                Player.immuneTime = 2;
                
                // Dramatic invulnerability visuals
                if (Main.rand.NextBool(2))
                {
                    Dust flame = Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(30f, 40f),
                        DustID.CrimsonTorch, Main.rand.NextVector2Circular(3f, 3f), 100, default, 2f);
                    flame.noGravity = true;
                }
                if (Main.rand.NextBool(3))
                {
                    Dust smoke = Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(25f, 35f),
                        DustID.Smoke, new Vector2(0, -2f), 150, Color.Black, 1.5f);
                    smoke.noGravity = true;
                }
                
                if (heroicEncoreTimer >= HeroicEncoreDuration)
                {
                    heroicEncoreActive = false;
                    heroicEncoreTimer = 0;
                }
            }
            
            // ========== SYMPHONY OF SCARLET FLAMES ==========
            // Reset consecutive hits if not wearing accessory
            if (!hasSymphonyOfScarletFlames)
            {
                consecutiveHits = 0;
                lastTargetHit = -1;
            }
        }
        
        public override void UpdateLifeRegen()
        {
            // Funeral March Insignia - Triple mana regen when below 20% mana
            if (hasFuneralMarchInsignia)
            {
                float manaPercent = (float)Player.statMana / Player.statManaMax2;
                if (manaPercent < 0.2f)
                {
                    Player.manaRegen += Player.manaRegen * 2; // Triple total
                }
            }
        }
        
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // Heroic Encore - Double magic damage during active
            if (heroicEncoreActive)
            {
                modifiers.SourceDamage *= 2f;
            }
        }
        
        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            // Funeral March Insignia - Prevent death if we have mana
            if (hasFuneralMarchInsignia && heroicEncoreCooldown <= 0 && Player.statMana > 0)
            {
                // Consume ALL mana
                Player.statMana = 0;
                
                // Activate Heroic Encore
                heroicEncoreActive = true;
                heroicEncoreTimer = 0;
                heroicEncoreCooldown = HeroicEncoreCooldownMax;
                
                // Heal to prevent death
                Player.statLife = 1;
                
                // Dramatic revival effect
                SoundEngine.PlaySound(SoundID.Item119 with { Pitch = -0.5f, Volume = 1.5f }, Player.Center);
                SoundEngine.PlaySound(SoundID.NPCDeath6 with { Pitch = 0.3f }, Player.Center);
                
                // Massive flame burst
                for (int ring = 0; ring < 3; ring++)
                {
                    for (int i = 0; i < 30; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 30f;
                        Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (8f + ring * 4f);
                        Dust flame = Dust.NewDustPerfect(Player.Center, DustID.CrimsonTorch, vel, 0, default, 2.5f - ring * 0.3f);
                        flame.noGravity = true;
                        flame.fadeIn = 1.5f;
                    }
                }
                
                // Black smoke burst
                for (int i = 0; i < 40; i++)
                {
                    Dust smoke = Dust.NewDustPerfect(Player.Center, DustID.Smoke,
                        Main.rand.NextVector2Circular(10f, 10f), 200, Color.Black, 2f);
                    smoke.noGravity = true;
                }
                
                // Golden sparks
                for (int i = 0; i < 20; i++)
                {
                    Dust spark = Dust.NewDustPerfect(Player.Center, DustID.GoldCoin,
                        Main.rand.NextVector2Circular(8f, 8f), 0, default, 1.5f);
                    spark.noGravity = true;
                }
                
                Main.NewText("Heroic Encore! The music refuses to end!", new Color(255, 100, 100));
                
                return false; // Prevent death
            }
            
            return true;
        }
        
        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            HandleMeleeHit(target, damageDone, item.DamageType);
            HandleRangedHit(target, damageDone, item.DamageType);
        }
        
        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            HandleMeleeHit(target, damageDone, proj.DamageType);
            HandleRangedHit(target, damageDone, proj.DamageType);
        }
        
        private void HandleMeleeHit(NPC target, int damageDone, DamageClass damageType)
        {
            if (!hasPyreOfTheFallenHero) return;
            if (damageType != DamageClass.Melee && damageType != DamageClass.MeleeNoSpeed) return;
            
            // Build fury stacks
            furyStacks++;
            
            // Visual feedback for stack building
            if (furyStacks < MaxFuryStacks)
            {
                // Small flame burst per hit
                for (int i = 0; i < 3 + furyStacks / 2; i++)
                {
                    Dust flame = Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(15f, 15f),
                        DustID.CrimsonTorch, Main.rand.NextVector2Circular(3f, 3f), 100, default, 1f + furyStacks * 0.08f);
                    flame.noGravity = true;
                }
            }
            
            // At max stacks, release the fury wave
            if (furyStacks >= MaxFuryStacks)
            {
                ReleaseFuryWave(damageDone);
                furyStacks = 0;
            }
        }
        
        private void HandleRangedHit(NPC target, int damageDone, DamageClass damageType)
        {
            if (!hasSymphonyOfScarletFlames) return;
            if (damageType != DamageClass.Ranged) return;
            
            // Check if same target
            if (target.whoAmI == lastTargetHit)
            {
                consecutiveHits++;
                
                // Visual feedback - growing mark on enemy
                int particleCount = 5 + consecutiveHits * 3;
                for (int i = 0; i < particleCount; i++)
                {
                    int dustType = Main.rand.NextBool() ? DustID.CrimsonTorch : DustID.GoldCoin;
                    Dust mark = Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(target.width / 2f, target.height / 2f),
                        dustType, Main.rand.NextVector2Circular(2f, 2f), 100, default, 1f + consecutiveHits * 0.2f);
                    mark.noGravity = true;
                }
                
                // 4th hit triggers the explosion
                if (consecutiveHits >= 3)
                {
                    TriggerTriumphantPrecision(target, damageDone);
                    consecutiveHits = 0;
                }
            }
            else
            {
                // New target, reset counter
                lastTargetHit = target.whoAmI;
                consecutiveHits = 1;
                
                // Small initial mark
                for (int i = 0; i < 5; i++)
                {
                    Dust mark = Dust.NewDustPerfect(target.Center, DustID.GoldCoin,
                        Main.rand.NextVector2Circular(2f, 2f), 100, default, 1f);
                    mark.noGravity = true;
                }
            }
        }
        
        private void ReleaseFuryWave(int baseDamage)
        {
            // Spawn the 360° slash wave projectile
            if (Main.myPlayer == Player.whoAmI)
            {
                int damage = (int)(baseDamage * 4f); // 400% damage
                Projectile.NewProjectile(
                    Player.GetSource_Accessory(new Item()),
                    Player.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<PyreSlashWave>(),
                    damage,
                    12f,
                    Player.whoAmI
                );
            }
            
            // Sound effect
            SoundEngine.PlaySound(SoundID.Item71 with { Pitch = -0.3f, Volume = 1.2f }, Player.Center);
            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Volume = 0.8f }, Player.Center);
        }
        
        private void TriggerTriumphantPrecision(NPC target, int baseDamage)
        {
            // Spawn petal explosion at target
            if (Main.myPlayer == Player.whoAmI)
            {
                int damage = (int)(baseDamage * 3f); // 300% damage
                Projectile.NewProjectile(
                    Player.GetSource_Accessory(new Item()),
                    target.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<PetalExplosion>(),
                    damage,
                    8f,
                    Player.whoAmI
                );
            }
            
            // Sound effect
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.5f, Volume = 1f }, target.Center);
            SoundEngine.PlaySound(SoundID.Item119 with { Pitch = 0.3f, Volume = 0.8f }, target.Center);
        }
        
        private void SummonHeroicSpirit()
        {
            if (Main.myPlayer == Player.whoAmI)
            {
                // Find a nearby enemy to target
                NPC target = null;
                float maxDist = 600f;
                
                foreach (NPC npc in Main.ActiveNPCs)
                {
                    if (npc.CanBeChasedBy())
                    {
                        float dist = Vector2.Distance(Player.Center, npc.Center);
                        if (dist < maxDist)
                        {
                            maxDist = dist;
                            target = npc;
                        }
                    }
                }
                
                // Spawn the heroic spirit
                Vector2 spawnPos = Player.Center + new Vector2(Main.rand.NextFloat(-50f, 50f), -60f);
                int damage = (int)(Player.GetTotalDamage(DamageClass.Summon).ApplyTo(150)); // Base 150 damage scaled
                
                Projectile.NewProjectile(
                    Player.GetSource_Accessory(new Item()),
                    spawnPos,
                    Vector2.Zero,
                    ModContent.ProjectileType<HeroicSpiritMinion>(),
                    damage,
                    6f,
                    Player.whoAmI,
                    target?.whoAmI ?? -1
                );
            }
            
            // Summoning effect
            SoundEngine.PlaySound(SoundID.Item78 with { Pitch = 0.2f }, Player.Center);
            
            // Dramatic appearance particles
            for (int i = 0; i < 25; i++)
            {
                float angle = MathHelper.TwoPi * i / 25f;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 5f;
                Dust flame = Dust.NewDustPerfect(Player.Center + new Vector2(0, -60f), DustID.CrimsonTorch, vel, 0, default, 1.8f);
                flame.noGravity = true;
            }
        }
        
        private void UpdateMinionProximityBonus()
        {
            bool nearMinion = false;
            
            foreach (Projectile proj in Main.ActiveProjectiles)
            {
                if (proj.owner == Player.whoAmI && proj.minion)
                {
                    float dist = Vector2.Distance(Player.Center, proj.Center);
                    if (dist < 240f) // 15 tiles = 240 pixels
                    {
                        nearMinion = true;
                        break;
                    }
                }
            }
            
            if (nearMinion)
            {
                Player.statDefense += 8;
            }
        }
        
        public override void OnHurt(Player.HurtInfo info)
        {
            // Pyre of the Fallen Hero - Taking damage grants attack speed boost
            if (hasPyreOfTheFallenHero)
            {
                damageBoostTimer = DamageBoostDuration;
                
                // Visual feedback
                for (int i = 0; i < 15; i++)
                {
                    Dust rage = Dust.NewDustPerfect(Player.Center, DustID.Torch,
                        Main.rand.NextVector2Circular(5f, 5f), 100, new Color(255, 150, 50), 1.5f);
                    rage.noGravity = true;
                }
            }
        }
        
        public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
        {
            // Sakura's Burning Will - Minions get +20% damage when player is near
            if (hasSakurasBurningWill && item.DamageType == DamageClass.Summon)
            {
                // This bonus is always active - the proximity check in UpdateMinionProximityBonus
                // gives defense to player, but minions always get the damage boost
                damage *= 1.20f;
            }
        }
    }
    
    /// <summary>
    /// Draw layer for floating Eroica accessories (FuneralMarchInsignia and SakurasBurningWill).
    /// </summary>
    public class EroicaFloatDrawLayer : PlayerDrawLayer
    {
        private static Asset<Texture2D> funeralFloatTexture;
        private static Asset<Texture2D> sakuraFloatTexture;
        
        // 6x6 sprite sheet = 36 frames
        private const int FrameColumns = 6;
        private const int FrameRows = 6;
        private const int TotalFrames = 36;
        private const int FrameTime = 4;
        
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.BackAcc);
        
        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            var modPlayer = drawInfo.drawPlayer.GetModPlayer<EroicaAccessoryPlayer>();
            return modPlayer.hasFuneralMarchInsignia || modPlayer.hasSakurasBurningWill;
        }
        
        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            var modPlayer = drawInfo.drawPlayer.GetModPlayer<EroicaAccessoryPlayer>();
            Player player = drawInfo.drawPlayer;
            
            // Load textures if needed
            if (funeralFloatTexture == null)
                funeralFloatTexture = ModContent.Request<Texture2D>("MagnumOpus/Content/Eroica/Accessories/FuneralMarchInsignia_Float");
            if (sakuraFloatTexture == null)
                sakuraFloatTexture = ModContent.Request<Texture2D>("MagnumOpus/Content/Eroica/Accessories/SakurasBurningWill_Float");
            
            float baseAngle = modPlayer.floatAngle;
            
            // Calculate current animation frame based on game time
            int currentFrame = (int)(Main.GameUpdateCount / FrameTime) % TotalFrames;
            int frameX = currentFrame % FrameColumns;
            int frameY = currentFrame / FrameColumns;
            
            // Draw Funeral March Insignia float (left side)
            if (modPlayer.hasFuneralMarchInsignia && funeralFloatTexture != null && funeralFloatTexture.IsLoaded)
            {
                Texture2D texture = funeralFloatTexture.Value;
                
                // Calculate frame dimensions from sprite sheet
                int frameWidth = texture.Width / FrameColumns;
                int frameHeight = texture.Height / FrameRows;
                Rectangle sourceRect = new Rectangle(frameX * frameWidth, frameY * frameHeight, frameWidth, frameHeight);
                Vector2 origin = new Vector2(frameWidth / 2f, frameHeight / 2f);
                
                // Float on left side with gentle bob
                Vector2 offset = new Vector2((float)Math.Cos(baseAngle) * 25f - 35f, (float)Math.Sin(baseAngle * 1.5f) * 12f - 30f);
                Vector2 drawPos = player.Center + offset - Main.screenPosition;
                
                SpriteEffects effects = SpriteEffects.None;
                Color lightColor = Lighting.GetColor((int)(player.Center.X / 16), (int)(player.Center.Y / 16));
                
                // Black/red glow effect for Funeral March theme
                Color glowColor = new Color(40, 0, 0, 0) * 0.5f;
                for (int i = 0; i < 4; i++)
                {
                    Vector2 glowOffset = new Vector2(2f, 0f).RotatedBy(i * MathHelper.PiOver2);
                    drawInfo.DrawDataCache.Add(new DrawData(
                        texture,
                        drawPos + glowOffset,
                        sourceRect,
                        glowColor,
                        0f,
                        origin,
                        1f,
                        effects,
                        0
                    ));
                }
                
                drawInfo.DrawDataCache.Add(new DrawData(
                    texture,
                    drawPos,
                    sourceRect,
                    lightColor,
                    0f,
                    origin,
                    1f,
                    effects,
                    0
                ));
            }
            
            // Draw Sakura's Burning Will float (right side) - offset frame for visual variety
            if (modPlayer.hasSakurasBurningWill && sakuraFloatTexture != null && sakuraFloatTexture.IsLoaded)
            {
                Texture2D texture = sakuraFloatTexture.Value;
                
                // Calculate frame dimensions from sprite sheet (offset by half for visual variety)
                int sakuraFrame = (currentFrame + TotalFrames / 2) % TotalFrames;
                int sakuraFrameX = sakuraFrame % FrameColumns;
                int sakuraFrameY = sakuraFrame / FrameColumns;
                
                int frameWidth = texture.Width / FrameColumns;
                int frameHeight = texture.Height / FrameRows;
                Rectangle sourceRect = new Rectangle(sakuraFrameX * frameWidth, sakuraFrameY * frameHeight, frameWidth, frameHeight);
                Vector2 origin = new Vector2(frameWidth / 2f, frameHeight / 2f);
                
                // Float on right side with gentle bob (opposite phase)
                Vector2 offset = new Vector2((float)Math.Cos(baseAngle + MathHelper.Pi) * 25f + 35f, (float)Math.Sin(baseAngle * 1.5f + 1f) * 12f - 25f);
                Vector2 drawPos = player.Center + offset - Main.screenPosition;
                
                SpriteEffects effects = SpriteEffects.None;
                Color lightColor = Lighting.GetColor((int)(player.Center.X / 16), (int)(player.Center.Y / 16));
                
                // Pink/scarlet glow effect for Sakura theme
                Color glowColor = new Color(255, 150, 180, 0) * 0.4f;
                for (int i = 0; i < 4; i++)
                {
                    Vector2 glowOffset = new Vector2(2f, 0f).RotatedBy(i * MathHelper.PiOver2);
                    drawInfo.DrawDataCache.Add(new DrawData(
                        texture,
                        drawPos + glowOffset,
                        sourceRect,
                        glowColor,
                        0f,
                        origin,
                        1f,
                        effects,
                        0
                    ));
                }
                
                drawInfo.DrawDataCache.Add(new DrawData(
                    texture,
                    drawPos,
                    sourceRect,
                    lightColor,
                    0f,
                    origin,
                    1f,
                    effects,
                    0
                ));
            }
        }
    }
    
    /// <summary>
    /// GlobalNPC for drawing Heroic Target indicator on marked enemies.
    /// </summary>
    public class HeroicTargetGlobalNPC : GlobalNPC
    {
        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            // Check if local player has Symphony of Scarlet Flames and this NPC is marked
            Player player = Main.LocalPlayer;
            var modPlayer = player.GetModPlayer<EroicaAccessoryPlayer>();
            
            if (!modPlayer.hasSymphonyOfScarletFlames || modPlayer.lastTargetHit != npc.whoAmI || modPlayer.consecutiveHits < 1)
                return;
            
            // Draw heroic target indicator above NPC
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.2f + 0.8f;
            Vector2 drawPos = npc.Top - screenPos + new Vector2(0, -25f);
            
            // Draw rotating crosshair/target
            float rotation = Main.GameUpdateCount * 0.05f;
            
            // Outer ring
            for (int i = 0; i < 4; i++)
            {
                float angle = rotation + MathHelper.PiOver2 * i;
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 12f * pulse;
                
                // Gold/red marks showing hit count
                Color markColor = modPlayer.consecutiveHits >= 3 ? 
                    new Color(255, 100, 100, 200) : // Ready to proc - bright red
                    new Color(255, 200, 100, 180);  // Building - gold
                
                // Draw small diamond marks
                spriteBatch.Draw(
                    Terraria.GameContent.TextureAssets.MagicPixel.Value,
                    drawPos + offset,
                    new Rectangle(0, 0, 4, 4),
                    markColor * pulse,
                    angle + MathHelper.PiOver4,
                    new Vector2(2, 2),
                    1.2f,
                    SpriteEffects.None,
                    0f);
            }
            
            // Inner circle indicator showing stacks
            for (int i = 0; i < modPlayer.consecutiveHits; i++)
            {
                float stackAngle = -MathHelper.PiOver2 + MathHelper.TwoPi * i / 3f;
                Vector2 stackOffset = new Vector2((float)Math.Cos(stackAngle), (float)Math.Sin(stackAngle)) * 6f;
                
                Color stackColor = i < modPlayer.consecutiveHits ? 
                    new Color(255, 50, 50, 255) : 
                    new Color(100, 100, 100, 100);
                
                spriteBatch.Draw(
                    Terraria.GameContent.TextureAssets.MagicPixel.Value,
                    drawPos + stackOffset,
                    new Rectangle(0, 0, 3, 3),
                    stackColor * pulse,
                    0f,
                    new Vector2(1.5f, 1.5f),
                    1f,
                    SpriteEffects.None,
                    0f);
            }
            
            // Add ambient particle effect on marked target
            if (Main.rand.NextBool(8))
            {
                Vector2 dustPos = npc.Center + Main.rand.NextVector2Circular(npc.width * 0.5f, npc.height * 0.5f);
                Dust mark = Dust.NewDustPerfect(dustPos, DustID.GoldFlame, new Vector2(0, -1.5f), 0, default, 0.8f);
                mark.noGravity = true;
            }
        }
    }
}
