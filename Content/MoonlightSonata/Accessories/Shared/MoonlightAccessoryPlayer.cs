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
using MagnumOpus.Common.Systems;

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
        public int minionSurgeTimer = 0;
        private const int MinionSurgeInterval = 300; // 5 seconds
        
        // Ember of the Moon (Mage)
        public bool hasEmberOfTheMoon = false;
        public int manaRestoreCooldown = 0;
        private const int ManaRestoreCooldownMax = 7200; // 120 seconds
        
        // Resurrection of the Moon reload state
        public int resurrectionReloadTimer = 0;
        public const int ResurrectionReloadTime = 90; // 1.5 seconds
        public bool resurrectionIsReloaded = true;
        public bool resurrectionPlayedReadySound = false;
        
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
            
            // Fractal of Moonlight - Minion Surge timer (speed boost)
            if (hasFractalOfMoonlight)
            {
                minionSurgeTimer++;
                if (minionSurgeTimer >= MinionSurgeInterval)
                {
                    minionSurgeTimer = 0;
                    PerformMinionSurge();
                }
            }
            else
            {
                minionSurgeTimer = 0;
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
        
        private void PerformMinionSurge()
        {
            // Boost all minions' attack speed temporarily via a visual surge effect
            // This creates a visual burst around each minion
            foreach (Projectile proj in Main.ActiveProjectiles)
            {
                if (proj.owner == Player.whoAmI && proj.minion)
                {
                    // Custom particles - moonlight glow flare on each minion
                    CustomParticles.MoonlightFlare(proj.Center, 0.5f);
                    
                    // Visual surge effect around each minion
                    for (int i = 0; i < 15; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 15f;
                        Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 4f;
                        int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch;
                        Dust dust = Dust.NewDustPerfect(proj.Center, dustType, vel, 0, default, 1.5f);
                        dust.noGravity = true;
                        dust.fadeIn = 1.2f;
                    }
                }
            }
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
            
            // Note: Sonic boom effect removed - Moonlit Gyre now only buffs Moonlight rifle weapons
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
            
            // Custom particles - ethereal moonlight flash
            CustomParticles.MoonlightFlare(position, 0.9f);
            CustomParticles.GenericGlow(position, new Color(150, 100, 220), 1.0f, 30);
            CustomParticles.MoonlightMusicNotes(position, 3, 25f);
            
            // Sound
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.7f, Pitch = 0.5f }, position);
            SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.5f, Pitch = 0.8f }, position);
        }
        
        // Note: CreateSonicBoom removed - Moonlit Gyre now only buffs specific Moonlight rifle weapons
        
        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            // Note: Specific Moonlight minion damage boosts are now handled in the minion projectiles themselves
            // when checking for hasFractalOfMoonlight
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
            // Moonlit Gyre - 20% chance not to consume ammo for ALL ranged weapons
            if (hasMoonlitGyre && weapon.DamageType == DamageClass.Ranged)
            {
                if (Main.rand.NextFloat() < 0.2f)
                    return false;
            }
            
            return true;
        }
    }
    
    // Note: MoonlitGyreGlobalProjectile removed - Moonlit Gyre now only buffs specific Moonlight rifle weapons
    // The ricochet effect for all ranged weapons has been removed as it was too powerful/not working as intended
    
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
