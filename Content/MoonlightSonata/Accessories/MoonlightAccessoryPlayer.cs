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

namespace MagnumOpus.Content.MoonlightSonata.Accessories
{
    /// <summary>
    /// ModPlayer class that handles all Moonlight Sonata accessory effects.
    /// </summary>
    public class MoonlightAccessoryPlayer : ModPlayer
    {
        // Moonlit Engine (Melee)
        public bool hasMoonlitEngine = false;
        public int meleeStrikeCounter = 0;
        
        // Moonlit Gyre (Ranger)
        public bool hasMoonlitGyre = false;
        
        // Fractal of Moonlight (Summoner)
        public bool hasFractalOfMoonlight = false;
        public int crescendoTimer = 0;
        private const int CrescendoInterval = 600; // 10 seconds
        
        // Ember of the Moon (Mage)
        public bool hasEmberOfTheMoon = false;
        public int manaRestoreCooldown = 0;
        private const int ManaRestoreCooldownMax = 7200; // 120 seconds
        
        // Floating visual tracking
        public float floatAngle = 0f;
        
        public override void ResetEffects()
        {
            hasMoonlitEngine = false;
            hasMoonlitGyre = false;
            hasFractalOfMoonlight = false;
            hasEmberOfTheMoon = false;
        }
        
        public override void PostUpdate()
        {
            // Update float angle for visual orbiting
            floatAngle += 0.03f;
            if (floatAngle > MathHelper.TwoPi)
                floatAngle -= MathHelper.TwoPi;
            
            // Fractal of Moonlight - Crescendo Attack timer
            if (hasFractalOfMoonlight)
            {
                crescendoTimer++;
                if (crescendoTimer >= CrescendoInterval)
                {
                    crescendoTimer = 0;
                    PerformCrescendoAttack();
                }
            }
            else
            {
                crescendoTimer = 0;
            }
            
            // Ember of the Moon - Mana restore cooldown
            if (manaRestoreCooldown > 0)
                manaRestoreCooldown--;
            
            // Ember of the Moon - Auto mana restore when low
            if (hasEmberOfTheMoon && manaRestoreCooldown <= 0)
            {
                float manaPercent = (float)Player.statMana / Player.statManaMax2;
                if (manaPercent < 0.2f)
                {
                    Player.statMana = Math.Min(Player.statMana + 100, Player.statManaMax2);
                    manaRestoreCooldown = ManaRestoreCooldownMax;
                    
                    // Visual effect
                    for (int i = 0; i < 30; i++)
                    {
                        int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch;
                        Dust dust = Dust.NewDustDirect(Player.Center, 1, 1, dustType,
                            Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f), 100, default, 2f);
                        dust.noGravity = true;
                    }
                    
                    SoundEngine.PlaySound(SoundID.Item4 with { Pitch = 0.5f }, Player.Center);
                }
            }
        }
        
        private void PerformCrescendoAttack()
        {
            // Find nearest boss
            NPC targetBoss = null;
            float closestDist = 2000f;
            
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.boss && npc.CanBeChasedBy())
                {
                    float dist = Vector2.Distance(Player.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        targetBoss = npc;
                    }
                }
            }
            
            // If no boss, find strongest enemy
            if (targetBoss == null)
            {
                int highestLife = 0;
                foreach (NPC npc in Main.ActiveNPCs)
                {
                    if (npc.CanBeChasedBy() && npc.lifeMax > highestLife)
                    {
                        float dist = Vector2.Distance(Player.Center, npc.Center);
                        if (dist < closestDist)
                        {
                            highestLife = npc.lifeMax;
                            targetBoss = npc;
                        }
                    }
                }
            }
            
            if (targetBoss == null)
                return;
            
            // Calculate combined minion damage (500% of combined damage)
            int totalMinionDamage = 0;
            int minionCount = 0;
            
            foreach (Projectile proj in Main.ActiveProjectiles)
            {
                if (proj.owner == Player.whoAmI && proj.minion)
                {
                    totalMinionDamage += proj.damage;
                    minionCount++;
                }
            }
            
            if (minionCount == 0)
                return;
            
            int crescendoDamage = (int)(totalMinionDamage * 5f); // 500%
            
            // Deal the damage
            if (Main.myPlayer == Player.whoAmI)
            {
                Player.ApplyDamageToNPC(targetBoss, crescendoDamage, 0f, Player.direction, false);
            }
            
            // MASSIVE visual effect - Crescendo Attack!
            Vector2 targetCenter = targetBoss.Center;
            
            // Converging beams from all minions
            foreach (Projectile proj in Main.ActiveProjectiles)
            {
                if (proj.owner == Player.whoAmI && proj.minion)
                {
                    // Beam line from minion to target
                    Vector2 direction = (targetCenter - proj.Center).SafeNormalize(Vector2.Zero);
                    float distance = Vector2.Distance(proj.Center, targetCenter);
                    
                    for (float i = 0; i < distance; i += 15f)
                    {
                        Vector2 dustPos = proj.Center + direction * i;
                        int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch;
                        Dust dust = Dust.NewDustPerfect(dustPos, dustType, direction * 2f, 0, default, 2f);
                        dust.noGravity = true;
                        dust.fadeIn = 1.5f;
                    }
                }
            }
            
            // Explosion at target
            for (int ring = 0; ring < 3; ring++)
            {
                for (int i = 0; i < 25; i++)
                {
                    float angle = MathHelper.TwoPi * i / 25f;
                    Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (8f + ring * 4f);
                    int dustType = (i + ring) % 2 == 0 ? DustID.PurpleTorch : DustID.IceTorch;
                    Dust dust = Dust.NewDustPerfect(targetCenter, dustType, vel, 0, default, 2.5f - ring * 0.3f);
                    dust.noGravity = true;
                    dust.fadeIn = 1.5f;
                }
            }
            
            // Sound
            SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.8f, Pitch = 0.3f }, targetCenter);
            SoundEngine.PlaySound(SoundID.Item105 with { Volume = 0.6f }, targetCenter);
        }
        
        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Moonlit Engine - Melee shockwave every 5th hit
            if (hasMoonlitEngine && item.DamageType == DamageClass.Melee)
            {
                meleeStrikeCounter++;
                if (meleeStrikeCounter >= 5)
                {
                    meleeStrikeCounter = 0;
                    CreateMoonlitShockwave(target.Center, (int)(damageDone * 2f)); // 200% damage
                }
            }
            
            // Fractal of Moonlight - 2% lifesteal for minion-related effects
            // (This is handled in OnHitNPCWithProj for actual minions)
        }
        
        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Moonlit Engine - Also works with melee projectiles
            if (hasMoonlitEngine && proj.DamageType == DamageClass.Melee)
            {
                meleeStrikeCounter++;
                if (meleeStrikeCounter >= 5)
                {
                    meleeStrikeCounter = 0;
                    CreateMoonlitShockwave(target.Center, (int)(damageDone * 2f));
                }
            }
            
            // Fractal of Moonlight - 2% lifesteal for minions
            if (hasFractalOfMoonlight && proj.minion)
            {
                int healAmount = Math.Max(1, (int)(damageDone * 0.02f));
                Player.statLife = Math.Min(Player.statLife + healAmount, Player.statLifeMax2);
                Player.HealEffect(healAmount, false);
            }
            
            // Moonlit Gyre - Crit sonic boom for ranged
            if (hasMoonlitGyre && proj.DamageType == DamageClass.Ranged && hit.Crit)
            {
                CreateSonicBoom(target.Center, damageDone);
            }
        }
        
        private void CreateMoonlitShockwave(Vector2 position, int damage)
        {
            // Spawn the shockwave projectile
            if (Main.myPlayer == Player.whoAmI)
            {
                Projectile.NewProjectile(
                    Player.GetSource_Accessory(new Item()),
                    position,
                    Vector2.Zero,
                    ModContent.ProjectileType<MoonlitEngineShockwave>(),
                    damage,
                    10f,
                    Player.whoAmI
                );
            }
            
            // Sound
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.7f, Pitch = 0.5f }, position);
            SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.5f, Pitch = 0.8f }, position);
        }
        
        private void CreateSonicBoom(Vector2 position, int damage)
        {
            // Spawn the sonic boom projectile
            if (Main.myPlayer == Player.whoAmI)
            {
                Projectile.NewProjectile(
                    Player.GetSource_Accessory(new Item()),
                    position,
                    Vector2.Zero,
                    ModContent.ProjectileType<MoonlitGyreSonicBoom>(),
                    damage,
                    5f,
                    Player.whoAmI
                );
            }
        }
        
        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            // Fractal of Moonlight - +30% minion damage
            if (hasFractalOfMoonlight && proj.minion)
            {
                modifiers.FinalDamage *= 1.3f;
            }
        }
        
        public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
        {
            // Ember of the Moon - +25% magic damage
            if (hasEmberOfTheMoon && item.DamageType == DamageClass.Magic)
            {
                damage *= 1.25f;
            }
        }
        
        public override void ModifyManaCost(Item item, ref float reduce, ref float mult)
        {
            // Ember of the Moon - -30% mana cost
            if (hasEmberOfTheMoon && item.DamageType == DamageClass.Magic)
            {
                mult *= 0.7f;
            }
        }
        
        public override bool Shoot(Item item, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Ember of the Moon - 15% chance to cast twice
            if (hasEmberOfTheMoon && item.DamageType == DamageClass.Magic)
            {
                if (Main.rand.NextFloat() < 0.15f)
                {
                    // Fire an extra projectile with slight offset
                    Vector2 offsetVel = velocity.RotatedByRandom(0.1f);
                    Projectile.NewProjectile(source, position, offsetVel, type, damage, knockback, Player.whoAmI);
                    
                    // Visual cue
                    for (int i = 0; i < 8; i++)
                    {
                        Dust dust = Dust.NewDustPerfect(position, DustID.PurpleTorch, 
                            Main.rand.NextVector2Circular(3f, 3f), 100, default, 1.2f);
                        dust.noGravity = true;
                    }
                }
            }
            
            return true;
        }
        
        public override void ModifyShootStats(Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            // Moonlit Gyre effects are handled by GlobalProjectile
        }
        
        public override bool CanConsumeAmmo(Item weapon, Item ammo)
        {
            // Moonlit Gyre - 40% chance not to consume ammo
            if (hasMoonlitGyre && weapon.DamageType == DamageClass.Ranged)
            {
                if (Main.rand.NextFloat() < 0.4f)
                    return false;
            }
            
            return true;
        }
    }
    
    /// <summary>
    /// GlobalProjectile to handle Moonlit Gyre ricochet mechanics.
    /// </summary>
    public class MoonlitGyreGlobalProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        
        public int ricochetCount = 0;
        public int missTimer = 0;
        public bool hasCheckedMiss = false;
        public Vector2 lastPosition = Vector2.Zero;
        
        public override bool PreAI(Projectile projectile)
        {
            Player owner = Main.player[projectile.owner];
            var modPlayer = owner.GetModPlayer<MoonlightAccessoryPlayer>();
            
            // Only apply to ranged projectiles from players with Moonlit Gyre
            if (!modPlayer.hasMoonlitGyre || projectile.DamageType != DamageClass.Ranged)
                return true;
            
            if (!projectile.friendly || projectile.hostile)
                return true;
            
            // Skip certain projectile types that shouldn't ricochet
            if (projectile.minion || projectile.bobber || projectile.aiStyle == ProjAIStyleID.Hook)
                return true;
            
            // Track for miss detection
            if (projectile.active && projectile.timeLeft < projectile.extraUpdates * 60 + 55 && !hasCheckedMiss)
            {
                missTimer++;
                
                // After some time without hitting, check if it's about to die
                if (projectile.timeLeft <= 5 && ricochetCount < 3)
                {
                    // Find new target
                    NPC newTarget = FindNearestEnemy(projectile.Center, 500f, projectile);
                    if (newTarget != null)
                    {
                        // Ricochet toward new target
                        Vector2 newDirection = (newTarget.Center - projectile.Center).SafeNormalize(Vector2.Zero);
                        float speed = projectile.velocity.Length();
                        if (speed < 8f) speed = 8f;
                        projectile.velocity = newDirection * speed;
                        projectile.timeLeft = 120;
                        ricochetCount++;
                        
                        // Ricochet visual
                        for (int i = 0; i < 10; i++)
                        {
                            int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch;
                            Dust dust = Dust.NewDustPerfect(projectile.Center, dustType, 
                                Main.rand.NextVector2Circular(4f, 4f), 100, default, 1.3f);
                            dust.noGravity = true;
                        }
                        
                        SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.3f, Pitch = 0.5f }, projectile.Center);
                    }
                }
            }
            
            lastPosition = projectile.Center;
            return true;
        }
        
        private NPC FindNearestEnemy(Vector2 position, float range, Projectile proj)
        {
            NPC closest = null;
            float closestDist = range;
            
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.CanBeChasedBy(proj))
                {
                    float dist = Vector2.Distance(position, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }
            
            return closest;
        }
        
        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            hasCheckedMiss = true; // Hit something, no need to ricochet
        }
    }
    
    /// <summary>
    /// Drawing layer for floating accessory visuals.
    /// </summary>
    public class MoonlightAccessoryDrawLayer : PlayerDrawLayer
    {
        private Asset<Texture2D> engineFloatTexture;
        private Asset<Texture2D> gyreFloatTexture;
        
        // Animation constants for sprite sheets
        private const int FrameColumns = 6;
        private const int FrameRows = 6;
        private const int TotalFrames = 36;
        private const int FrameTime = 4;
        
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.BackAcc);
        
        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            var modPlayer = drawInfo.drawPlayer.GetModPlayer<MoonlightAccessoryPlayer>();
            return modPlayer.hasMoonlitEngine || modPlayer.hasMoonlitGyre;
        }
        
        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            var modPlayer = drawInfo.drawPlayer.GetModPlayer<MoonlightAccessoryPlayer>();
            Player player = drawInfo.drawPlayer;
            
            // Load textures if needed
            if (engineFloatTexture == null)
                engineFloatTexture = ModContent.Request<Texture2D>("MagnumOpus/Content/MoonlightSonata/Accessories/MoonlitEngine_Float");
            if (gyreFloatTexture == null)
                gyreFloatTexture = ModContent.Request<Texture2D>("MagnumOpus/Content/MoonlightSonata/Accessories/MoonlitGyre_Float");
            
            float baseAngle = modPlayer.floatAngle;
            
            // Calculate current animation frame based on game time
            int currentFrame = (int)(Main.GameUpdateCount / FrameTime) % TotalFrames;
            int frameX = currentFrame % FrameColumns;
            int frameY = currentFrame / FrameColumns;
            
            // Draw Moonlit Engine float
            if (modPlayer.hasMoonlitEngine && engineFloatTexture.IsLoaded)
            {
                Texture2D texture = engineFloatTexture.Value;
                
                // Calculate frame dimensions from sprite sheet
                int frameWidth = texture.Width / FrameColumns;
                int frameHeight = texture.Height / FrameRows;
                Rectangle sourceRect = new Rectangle(frameX * frameWidth, frameY * frameHeight, frameWidth, frameHeight);
                Vector2 origin = new Vector2(frameWidth / 2f, frameHeight / 2f);
                
                Vector2 offset = new Vector2((float)Math.Cos(baseAngle) * 30f, (float)Math.Sin(baseAngle) * 15f - 40f);
                Vector2 drawPos = player.Center + offset - Main.screenPosition;
                
                SpriteEffects effects = player.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                Color lightColor = Lighting.GetColor((int)(player.Center.X / 16), (int)(player.Center.Y / 16));
                
                // Glow effect
                Color glowColor = new Color(150, 100, 200, 0) * 0.5f;
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
            
            // Draw Moonlit Gyre float (opposite side) - offset frame for variety
            if (modPlayer.hasMoonlitGyre && gyreFloatTexture.IsLoaded)
            {
                Texture2D texture = gyreFloatTexture.Value;
                
                // Calculate frame dimensions from sprite sheet (offset by half for visual variety)
                int gyreFrame = (currentFrame + TotalFrames / 2) % TotalFrames;
                int gyreFrameX = gyreFrame % FrameColumns;
                int gyreFrameY = gyreFrame / FrameColumns;
                
                int frameWidth = texture.Width / FrameColumns;
                int frameHeight = texture.Height / FrameRows;
                Rectangle sourceRect = new Rectangle(gyreFrameX * frameWidth, gyreFrameY * frameHeight, frameWidth, frameHeight);
                Vector2 origin = new Vector2(frameWidth / 2f, frameHeight / 2f);
                
                Vector2 offset = new Vector2((float)Math.Cos(baseAngle + MathHelper.Pi) * 35f, (float)Math.Sin(baseAngle + MathHelper.Pi) * 15f - 35f);
                Vector2 drawPos = player.Center + offset - Main.screenPosition;
                
                SpriteEffects effects = player.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                Color lightColor = Lighting.GetColor((int)(player.Center.X / 16), (int)(player.Center.Y / 16));
                
                // Glow effect
                Color glowColor = new Color(100, 150, 220, 0) * 0.5f;
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
}
