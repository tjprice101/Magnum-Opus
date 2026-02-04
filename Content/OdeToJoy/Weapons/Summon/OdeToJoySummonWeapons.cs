using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.OdeToJoy.Projectiles;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;
using MagnumOpus.Content.OdeToJoy.HarmonicCores;
using MagnumOpus.Content.Fate.CraftingStations;

namespace MagnumOpus.Content.OdeToJoy.Weapons.Summon
{
    #region The Standing Ovation
    
    /// <summary>
    /// The Standing Ovation - Summons applauding spirit minions
    /// Spirits hover and release waves of joy energy that damage enemies
    /// Multiple spirits synchronize their attacks for bonus damage
    /// </summary>
    public class TheStandingOvation : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 46;
            Item.height = 46;
            Item.damage = 2600; // Post-Dies Irae summon
            Item.DamageType = DamageClass.Summon;
            Item.mana = 20;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<StandingOvationMinion>();
            Item.buffType = ModContent.BuffType<StandingOvationBuff>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Summon", "Summons an applauding spirit to fight for you"));
            tooltips.Add(new TooltipLine(Mod, "Behavior", "Spirits hover and release waves of joyful energy"));
            tooltips.Add(new TooltipLine(Mod, "Synergy", "Multiple spirits synchronize for +20% damage per spirit"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The crowd rises in celebration'") 
            { 
                OverrideColor = OdeToJoyColors.GoldenPollen 
            });
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            
            Vector2 spawnPos = Main.MouseWorld;
            
            // Enhanced entrance VFX - chromatic signature explosion
            OdeToJoyProjectiles.OdeToJoySignatureExplosion(spawnPos, 1.0f);
            
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f, Volume = 0.8f }, spawnPos);
            
            Projectile.NewProjectile(source, spawnPos, Vector2.Zero, type, damage, knockback, player.whoAmI);
            
            return false;
        }

        public override void HoldItem(Player player)
        {
            if (Main.rand.NextBool(20))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                OdeToJoyVFX.SpawnMusicNote(notePos, new Vector2(0, -0.5f), OdeToJoyColors.GoldenPollen, 0.65f);
            }
            
            Lighting.AddLight(player.Center, OdeToJoyColors.GoldenPollen.ToVector3() * 0.2f);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfOdeToJoy>(), 25)
                .AddIngredient(ModContent.ItemType<OdeToJoyResonantEnergy>(), 20)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfOdeToJoy>(), 3)
                .AddIngredient(ItemID.LunarBar, 20)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    /// <summary>
    /// Standing Ovation Buff
    /// </summary>
    public class StandingOvationBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<StandingOvationMinion>()] > 0)
            {
                player.buffTime[buffIndex] = 18000;
            }
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }

    /// <summary>
    /// Standing Ovation Minion - Applauding spirit
    /// </summary>
    public class StandingOvationMinion : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/OdeToJoy/Weapons/Summon/TheStandingOvationMinion";

        private int attackTimer = 0;
        private float hoverAngle = 0f;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => true;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            
            if (!CheckActive(owner))
                return;
            
            // Hover around player
            hoverAngle += 0.03f;
            Vector2 idlePosition = owner.Center + new Vector2(0, -80f) + hoverAngle.ToRotationVector2() * 20f;
            
            // Find target
            NPC target = FindClosestNPC(800f);
            
            if (target != null)
            {
                // Move toward target area
                Vector2 targetPos = target.Center + new Vector2(0, -100f);
                Vector2 toTarget = targetPos - Projectile.Center;
                
                if (toTarget.Length() > 50f)
                {
                    toTarget = toTarget.SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 12f, 0.1f);
                }
                else
                {
                    Projectile.velocity *= 0.9f;
                }
                
                // Count other spirits for sync bonus
                int spiritCount = CountOtherSpirits();
                float damageBonus = 1f + spiritCount * 0.2f;
                
                // Attack
                attackTimer++;
                if (attackTimer >= 60)
                {
                    attackTimer = 0;
                    
                    // Fire joy wave
                    Vector2 attackDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        attackDir * 14f,
                        ModContent.ProjectileType<JoyWaveProjectile>(),
                        (int)(Projectile.damage * damageBonus),
                        Projectile.knockBack,
                        Projectile.owner
                    );
                    
                    SoundEngine.PlaySound(SoundID.Item29 with { Volume = 0.6f, Pitch = 0.4f }, Projectile.Center);
                    OdeToJoyProjectiles.HarmonicNoteSparkle(Projectile.Center, 6, 4f, 0.5f, false);
                }
            }
            else
            {
                // Return to player
                Vector2 toIdle = idlePosition - Projectile.Center;
                
                if (toIdle.Length() > 30f)
                {
                    toIdle = toIdle.SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdle * 10f, 0.08f);
                }
                else
                {
                    Projectile.velocity *= 0.9f;
                }
            }
            
            // Face movement direction
            if (Math.Abs(Projectile.velocity.X) > 0.5f)
            {
                Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;
            }
            
            // Ambient particles
            if (Main.rand.NextBool(6))
            {
                Color trailColor = OdeToJoyColors.GetGradient(Main.rand.NextFloat());
                var trail = new GenericGlowParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(15f, 15f),
                    Main.rand.NextVector2Circular(1f, 1f),
                    trailColor * 0.6f,
                    0.25f,
                    15,
                    true
                );
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            Lighting.AddLight(Projectile.Center, OdeToJoyColors.GoldenPollen.ToVector3() * 0.4f);
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<StandingOvationBuff>());
                return false;
            }
            
            if (owner.HasBuff(ModContent.BuffType<StandingOvationBuff>()))
            {
                Projectile.timeLeft = 2;
            }
            
            return true;
        }

        private int CountOtherSpirits()
        {
            int count = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.owner == Projectile.owner && proj.type == Type && proj.whoAmI != Projectile.whoAmI)
                {
                    count++;
                }
            }
            return count;
        }

        private NPC FindClosestNPC(float maxRange)
        {
            NPC closest = null;
            float closestDist = maxRange;
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || !npc.CanBeChasedBy())
                    continue;
                
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }
            
            return closest;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.1f;
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            
            // Glow behind
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Main.spriteBatch.Draw(texture, drawPos, null, OdeToJoyColors.GoldenPollen * 0.4f, 0f, origin, pulse * 1.3f, effects, 0f);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Main sprite
            Main.spriteBatch.Draw(texture, drawPos, null, lightColor, 0f, origin, pulse, effects, 0f);
            
            return false;
        }
    }

    /// <summary>
    /// Joy Wave Projectile - Attack from Standing Ovation
    /// </summary>
    public class JoyWaveProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/GlowingHalo2";

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.scale = 1f + (float)Math.Sin((90 - Projectile.timeLeft) * 0.1f) * 0.3f;
            
            // Trail
            if (Main.rand.NextBool(2))
            {
                Color trailColor = OdeToJoyColors.GetGradient(Main.rand.NextFloat());
                var trail = new GenericGlowParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(20f, 20f),
                    -Projectile.velocity * 0.1f,
                    trailColor * 0.6f,
                    0.35f,
                    15,
                    true
                );
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // Music notes
            if (Main.rand.NextBool(4))
            {
                OdeToJoyVFX.SpawnMusicNote(Projectile.Center,
                    Main.rand.NextVector2Circular(2f, 2f),
                    OdeToJoyColors.RosePink, 0.7f);
            }
            
            Lighting.AddLight(Projectile.Center, OdeToJoyColors.GoldenPollen.ToVector3() * 0.5f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Confused, 60);
            // Use new Harmonic Note Sparkle for magical summon impact!
            OdeToJoyVFX.HarmonicNoteSparkle(target.Center, 6, 4f, 0.6f, false);
        }

        public override void OnKill(int timeLeft)
        {
            // Use Chromatic Rose Petal Burst for beautiful death!
            OdeToJoyVFX.ChromaticRosePetalBurst(Projectile.Center, 8, 5f, 0.7f, true);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Main.spriteBatch.Draw(texture, drawPos, null, OdeToJoyColors.GoldenPollen * 0.7f, Projectile.rotation, origin, Projectile.scale * 0.6f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(texture, drawPos, null, Color.White * 0.5f, Projectile.rotation, origin, Projectile.scale * 0.4f, SpriteEffects.None, 0f);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
    }
    
    #endregion

    #region Fountain of Joyous Harmony
    
    /// <summary>
    /// Fountain of Joyous Harmony - Summons a fountain that heals allies and damages enemies
    /// The fountain stays stationary and creates an area of effect
    /// </summary>
    public class FountainOfJoyousHarmony : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 50;
            Item.height = 50;
            Item.damage = 2200; // Post-Dies Irae summon (support focused)
            Item.DamageType = DamageClass.Summon;
            Item.mana = 25;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 2f;
            Item.value = Item.sellPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<JoyousFountainMinion>();
            Item.buffType = ModContent.BuffType<JoyousFountainBuff>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Summon", "Summons a joyous fountain at your cursor position"));
            tooltips.Add(new TooltipLine(Mod, "Behavior", "The fountain stays in place, healing allies and damaging enemies"));
            tooltips.Add(new TooltipLine(Mod, "Healing", "Heals nearby players for 3 HP per second"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'From joy springs eternal life'") 
            { 
                OverrideColor = OdeToJoyColors.VerdantGreen 
            });
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            
            Vector2 spawnPos = Main.MouseWorld;
            
            // Entrance VFX - Use the FULL signature explosion!
            OdeToJoyVFX.OdeToJoySignatureExplosion(spawnPos, 1.2f);
            
            // Additional water burst for fountain theme
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f) + new Vector2(0, -2f);
                Color burstColor = Color.Lerp(OdeToJoyColors.VerdantGreen, OdeToJoyColors.RosePink, Main.rand.NextFloat());
                var burst = new GenericGlowParticle(spawnPos, burstVel, burstColor * 0.7f, 0.4f, 25, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
            
            // Music notes rising
            for (int i = 0; i < 6; i++)
            {
                OdeToJoyVFX.SpawnMusicNote(spawnPos, new Vector2(Main.rand.NextFloat(-2f, 2f), -3f), 
                    OdeToJoyColors.GetGradient(Main.rand.NextFloat()), 0.9f);
            }
            
            SoundEngine.PlaySound(SoundID.Item21 with { Pitch = 0.3f }, spawnPos);
            
            Projectile.NewProjectile(source, spawnPos, Vector2.Zero, type, damage, knockback, player.whoAmI);
            
            return false;
        }

        public override void HoldItem(Player player)
        {
            if (Main.rand.NextBool(25))
            {
                Vector2 particlePos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                var glow = new GenericGlowParticle(particlePos, new Vector2(0, -1f), OdeToJoyColors.VerdantGreen * 0.5f, 0.2f, 20, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfOdeToJoy>(), 25)
                .AddIngredient(ModContent.ItemType<OdeToJoyResonantEnergy>(), 20)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfOdeToJoy>(), 3)
                .AddIngredient(ItemID.LunarBar, 20)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    /// <summary>
    /// Joyous Fountain Buff
    /// </summary>
    public class JoyousFountainBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<JoyousFountainMinion>()] > 0)
            {
                player.buffTime[buffIndex] = 18000;
            }
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }

    /// <summary>
    /// Joyous Fountain Minion - Stationary healing/damage fountain
    /// </summary>
    public class JoyousFountainMinion : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/OdeToJoy/Weapons/Summon/FountainOfJoyousHarmonyMinion";

        private int healTimer = 0;
        private int attackTimer = 0;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => false;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            
            if (!CheckActive(owner))
                return;
            
            // Stationary - no movement
            Projectile.velocity = Vector2.Zero;
            
            // Heal nearby players
            healTimer++;
            if (healTimer >= 60) // Every second
            {
                healTimer = 0;
                
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player player = Main.player[i];
                    if (!player.active || player.dead)
                        continue;
                    
                    float dist = Vector2.Distance(Projectile.Center, player.Center);
                    if (dist < 200f)
                    {
                        player.Heal(3);
                        
                        // Healing VFX
                        for (int j = 0; j < 3; j++)
                        {
                            OdeToJoyVFX.SpawnMusicNote(player.Center,
                                new Vector2(Main.rand.NextFloat(-1f, 1f), -2f),
                                OdeToJoyColors.VerdantGreen, 0.7f);
                        }
                    }
                }
            }
            
            // Attack nearby enemies
            attackTimer++;
            if (attackTimer >= 45)
            {
                attackTimer = 0;
                
                NPC target = FindClosestNPC(400f);
                if (target != null)
                {
                    Vector2 attackDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY) * -1f;
                    attackDir = attackDir.RotatedByRandom(0.3f);
                    
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center + new Vector2(0, -20f),
                        attackDir * 10f + new Vector2(0, -8f),
                        ModContent.ProjectileType<FountainWaterProjectile>(),
                        Projectile.damage,
                        Projectile.knockBack,
                        Projectile.owner
                    );
                    
                    SoundEngine.PlaySound(SoundID.Item21 with { Volume = 0.5f, Pitch = 0.4f }, Projectile.Center);
                }
            }
            
            // Ambient water particles rising
            if (Main.rand.NextBool(3))
            {
                Color waterColor = Color.Lerp(OdeToJoyColors.VerdantGreen, OdeToJoyColors.RosePink, Main.rand.NextFloat());
                var water = new GenericGlowParticle(
                    Projectile.Center + new Vector2(Main.rand.NextFloat(-15f, 15f), -20f),
                    new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -2f),
                    waterColor * 0.6f,
                    0.25f,
                    20,
                    true
                );
                MagnumParticleHandler.SpawnParticle(water);
            }
            
            // Music notes
            if (Main.rand.NextBool(10))
            {
                OdeToJoyVFX.SpawnMusicNote(Projectile.Center + new Vector2(0, -30f),
                    new Vector2(Main.rand.NextFloat(-1f, 1f), -1.5f),
                    OdeToJoyColors.GetGradient(Main.rand.NextFloat()), 0.75f);
            }
            
            Lighting.AddLight(Projectile.Center, OdeToJoyColors.VerdantGreen.ToVector3() * 0.5f);
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<JoyousFountainBuff>());
                return false;
            }
            
            if (owner.HasBuff(ModContent.BuffType<JoyousFountainBuff>()))
            {
                Projectile.timeLeft = 2;
            }
            
            return true;
        }

        private NPC FindClosestNPC(float maxRange)
        {
            NPC closest = null;
            float closestDist = maxRange;
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || !npc.CanBeChasedBy())
                    continue;
                
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }
            
            return closest;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            // Glow
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.1f;
            Main.spriteBatch.Draw(texture, drawPos, null, OdeToJoyColors.VerdantGreen * 0.4f, 0f, origin, pulse * 1.2f, SpriteEffects.None, 0f);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Main.spriteBatch.Draw(texture, drawPos, null, lightColor, 0f, origin, 1f, SpriteEffects.None, 0f);
            
            return false;
        }
    }

    /// <summary>
    /// Fountain Water Projectile - Arc attack from fountain
    /// </summary>
    public class FountainWaterProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow2";

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Projectile.velocity.Y += 0.2f; // Gravity for arc
            Projectile.rotation += 0.1f;
            
            // Trail
            Color trailColor = Color.Lerp(OdeToJoyColors.VerdantGreen, OdeToJoyColors.RosePink, Main.rand.NextFloat());
            var trail = new GenericGlowParticle(
                Projectile.Center,
                -Projectile.velocity * 0.1f,
                trailColor * 0.6f,
                0.2f,
                12,
                true
            );
            MagnumParticleHandler.SpawnParticle(trail);
            
            Lighting.AddLight(Projectile.Center, OdeToJoyColors.VerdantGreen.ToVector3() * 0.3f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Wet, 180);
            OdeToJoyProjectiles.ChromaticRosePetalBurst(target.Center, 6, 3f, 0.4f, false);
        }

        public override void OnKill(int timeLeft)
        {
            OdeToJoyProjectiles.ChromaticVineGrowthBurst(Projectile.Center, 3, 3f, 0.35f, false);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Main.spriteBatch.Draw(texture, drawPos, null, OdeToJoyColors.VerdantGreen * 0.7f, Projectile.rotation, origin, 0.4f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(texture, drawPos, null, Color.White * 0.5f, Projectile.rotation, origin, 0.25f, SpriteEffects.None, 0f);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
    }
    
    #endregion

    #region Triumphant Chorus
    
    /// <summary>
    /// Triumphant Chorus - The ultimate Ode to Joy summon
    /// Summons a choir of spirits that attack in unison with devastating harmonic blasts
    /// Uses 2 minion slots but deals massive coordinated damage
    /// </summary>
    public class TriumphantChorus : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 52;
            Item.height = 52;
            Item.damage = 3400; // Post-Dies Irae ultimate summon
            Item.DamageType = DamageClass.Summon;
            Item.mana = 40;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(platinum: 3, gold: 75);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
            Item.UseSound = SoundID.Item82;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<TriumphantChorusMinion>();
            Item.buffType = ModContent.BuffType<TriumphantChorusBuff>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Summon", "Summons a triumphant spirit chorus to fight for you"));
            tooltips.Add(new TooltipLine(Mod, "Slots", "Uses 2 minion slots"));
            tooltips.Add(new TooltipLine(Mod, "Behavior", "The chorus attacks with devastating coordinated harmonic blasts"));
            tooltips.Add(new TooltipLine(Mod, "Ultimate", "Periodically unleashes a grand finale attack"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'All voices rise as one in triumphant song'") 
            { 
                OverrideColor = OdeToJoyColors.RosePink 
            });
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            
            Vector2 spawnPos = Main.MouseWorld;
            
            // Grand entrance VFX - FULL SIGNATURE EXPLOSION
            OdeToJoyProjectiles.OdeToJoySignatureExplosion(spawnPos, 1.3f);
            
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.5f, Volume = 1.0f }, spawnPos);
            
            Projectile.NewProjectile(source, spawnPos, Vector2.Zero, type, damage, knockback, player.whoAmI);
            
            return false;
        }

        public override void HoldItem(Player player)
        {
            if (Main.rand.NextBool(15))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(40f, 40f);
                OdeToJoyVFX.SpawnMusicNote(notePos, new Vector2(0, -0.8f), 
                    OdeToJoyColors.GetGradient(Main.rand.NextFloat()), 0.75f);
            }
            
            Lighting.AddLight(player.Center, OdeToJoyColors.RosePink.ToVector3() * 0.25f);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfOdeToJoy>(), 30)
                .AddIngredient(ModContent.ItemType<OdeToJoyResonantEnergy>(), 25)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfOdeToJoy>(), 4)
                .AddIngredient(ItemID.LunarBar, 25)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    /// <summary>
    /// Triumphant Chorus Buff
    /// </summary>
    public class TriumphantChorusBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<TriumphantChorusMinion>()] > 0)
            {
                player.buffTime[buffIndex] = 18000;
            }
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }

    /// <summary>
    /// Triumphant Chorus Minion - Powerful coordinated attacker
    /// </summary>
    public class TriumphantChorusMinion : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/OdeToJoy/Weapons/Summon/TriumphantChorusMinion";

        private int attackTimer = 0;
        private int ultimateTimer = 0;
        private float hoverAngle = 0f;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 2f; // Uses 2 slots
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => true;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            
            if (!CheckActive(owner))
                return;
            
            // Hover around player
            hoverAngle += 0.025f;
            Vector2 idlePosition = owner.Center + new Vector2(0, -100f) + hoverAngle.ToRotationVector2() * 30f;
            
            // Find target
            NPC target = FindClosestNPC(900f);
            
            if (target != null)
            {
                // Move toward target area
                Vector2 targetPos = target.Center + new Vector2(0, -120f);
                Vector2 toTarget = targetPos - Projectile.Center;
                
                if (toTarget.Length() > 60f)
                {
                    toTarget = toTarget.SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 14f, 0.1f);
                }
                else
                {
                    Projectile.velocity *= 0.9f;
                }
                
                // Regular attack
                attackTimer++;
                if (attackTimer >= 40)
                {
                    attackTimer = 0;
                    
                    // Fire harmonic blast
                    Vector2 attackDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        attackDir * 16f,
                        ModContent.ProjectileType<HarmonicBlastProjectile>(),
                        Projectile.damage,
                        Projectile.knockBack,
                        Projectile.owner
                    );
                    
                    SoundEngine.PlaySound(SoundID.Item71 with { Volume = 0.7f, Pitch = 0.3f }, Projectile.Center);
                    // Use Harmonic Note Sparkle for musical attack!
                    OdeToJoyVFX.HarmonicNoteSparkle(Projectile.Center, 6, 4f, 0.6f, true);
                }
                
                // Ultimate attack
                ultimateTimer++;
                if (ultimateTimer >= 300) // Every 5 seconds
                {
                    ultimateTimer = 0;
                    
                    // Grand finale blast
                    SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.9f, Pitch = 0.4f }, Projectile.Center);
                    
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 8f;
                        Vector2 blastVel = angle.ToRotationVector2() * 12f;
                        
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            Projectile.Center,
                            blastVel,
                            ModContent.ProjectileType<GrandFinaleProjectile>(),
                            Projectile.damage * 2,
                            Projectile.knockBack * 2,
                            Projectile.owner
                        );
                    }
                    
                    // Use FULL SIGNATURE EXPLOSION for the ultimate attack!
                    OdeToJoyVFX.OdeToJoySignatureExplosion(Projectile.Center, 1.4f);
                    
                    // Additional massive music note burst
                    for (int i = 0; i < 12; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 12f;
                        OdeToJoyVFX.SpawnMusicNote(Projectile.Center,
                            angle.ToRotationVector2() * 5f,
                            OdeToJoyColors.GetGradient(i / 12f), 1.0f);
                    }
                }
            }
            else
            {
                // Return to player
                Vector2 toIdle = idlePosition - Projectile.Center;
                
                if (toIdle.Length() > 40f)
                {
                    toIdle = toIdle.SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdle * 12f, 0.08f);
                }
                else
                {
                    Projectile.velocity *= 0.9f;
                }
            }
            
            // Face movement direction
            if (Math.Abs(Projectile.velocity.X) > 0.5f)
            {
                Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;
            }
            
            // Ambient particles - more elaborate
            if (Main.rand.NextBool(4))
            {
                Color trailColor = OdeToJoyColors.GetGradient(Main.rand.NextFloat());
                var trail = new GenericGlowParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(20f, 20f),
                    Main.rand.NextVector2Circular(1.5f, 1.5f),
                    trailColor * 0.6f,
                    0.3f,
                    18,
                    true
                );
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // Music notes
            if (Main.rand.NextBool(8))
            {
                OdeToJoyVFX.SpawnMusicNote(Projectile.Center + Main.rand.NextVector2Circular(15f, 15f),
                    Main.rand.NextVector2Circular(1f, 1f),
                    OdeToJoyColors.GetGradient(Main.rand.NextFloat()), 0.7f);
            }
            
            Lighting.AddLight(Projectile.Center, OdeToJoyColors.RosePink.ToVector3() * 0.5f);
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<TriumphantChorusBuff>());
                return false;
            }
            
            if (owner.HasBuff(ModContent.BuffType<TriumphantChorusBuff>()))
            {
                Projectile.timeLeft = 2;
            }
            
            return true;
        }

        private NPC FindClosestNPC(float maxRange)
        {
            NPC closest = null;
            float closestDist = maxRange;
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || !npc.CanBeChasedBy())
                    continue;
                
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }
            
            return closest;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.12f;
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            
            // Glow behind
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Main.spriteBatch.Draw(texture, drawPos, null, OdeToJoyColors.RosePink * 0.5f, 0f, origin, pulse * 1.4f, effects, 0f);
            Main.spriteBatch.Draw(texture, drawPos, null, OdeToJoyColors.GoldenPollen * 0.3f, 0f, origin, pulse * 1.2f, effects, 0f);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Main sprite
            Main.spriteBatch.Draw(texture, drawPos, null, lightColor, 0f, origin, pulse, effects, 0f);
            
            return false;
        }
    }

    /// <summary>
    /// Harmonic Blast Projectile - Regular attack from Triumphant Chorus
    /// </summary>
    public class HarmonicBlastProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/EnergyFlare2";

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Trail
            if (Main.rand.NextBool(2))
            {
                Color trailColor = OdeToJoyColors.GetGradient(Main.rand.NextFloat());
                var trail = new GenericGlowParticle(
                    Projectile.Center,
                    -Projectile.velocity * 0.1f,
                    trailColor * 0.7f,
                    0.3f,
                    15,
                    true
                );
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            Lighting.AddLight(Projectile.Center, OdeToJoyColors.RosePink.ToVector3() * 0.5f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Confused, 90);
            OdeToJoyProjectiles.ChromaticRosePetalBurst(target.Center, 6, 4f, 0.4f, false);
        }

        public override void OnKill(int timeLeft)
        {
            OdeToJoyProjectiles.HarmonicNoteSparkle(Projectile.Center, 5, 3f, 0.5f, false);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.15f;
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Main.spriteBatch.Draw(texture, drawPos, null, OdeToJoyColors.RosePink * 0.6f, Projectile.rotation, origin, 0.5f * pulse, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(texture, drawPos, null, OdeToJoyColors.GoldenPollen * 0.5f, Projectile.rotation * 0.8f, origin, 0.4f * pulse, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(texture, drawPos, null, Color.White * 0.6f, Projectile.rotation, origin, 0.25f * pulse, SpriteEffects.None, 0f);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
    }

    /// <summary>
    /// Grand Finale Projectile - Ultimate attack from Triumphant Chorus
    /// </summary>
    public class GrandFinaleProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/StarBurst1";

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            Projectile.rotation += 0.15f;
            
            // Heavy trail
            for (int i = 0; i < 2; i++)
            {
                Color trailColor = OdeToJoyColors.GetGradient(Main.rand.NextFloat());
                var trail = new GenericGlowParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    -Projectile.velocity * 0.15f,
                    trailColor * 0.8f,
                    0.4f,
                    20,
                    true
                );
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // Music notes
            if (Main.rand.NextBool(3))
            {
                OdeToJoyVFX.SpawnMusicNote(Projectile.Center,
                    Main.rand.NextVector2Circular(2f, 2f),
                    OdeToJoyColors.GetGradient(Main.rand.NextFloat()), 0.85f);
            }
            
            // Sparkles
            if (Main.rand.NextBool(2))
            {
                var sparkle = new SparkleParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(15f, 15f),
                    Main.rand.NextVector2Circular(2f, 2f),
                    OdeToJoyColors.GoldenPollen,
                    0.35f,
                    15
                );
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            Lighting.AddLight(Projectile.Center, OdeToJoyColors.GoldenPollen.ToVector3() * 0.7f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Confused, 180);
            target.AddBuff(BuffID.OnFire, 180);
            OdeToJoyProjectiles.HarmonicNoteSparkle(target.Center, 8, 5f, 0.7f, true);
        }

        public override void OnKill(int timeLeft)
        {
            // Signature explosion on death
            OdeToJoyProjectiles.OdeToJoySignatureExplosion(Projectile.Center, 0.9f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.2f;
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Main.spriteBatch.Draw(texture, drawPos, null, OdeToJoyColors.GoldenPollen * 0.7f, Projectile.rotation, origin, 0.7f * pulse, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(texture, drawPos, null, OdeToJoyColors.RosePink * 0.5f, -Projectile.rotation * 0.8f, origin, 0.55f * pulse, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(texture, drawPos, null, Color.White * 0.7f, Projectile.rotation, origin, 0.35f * pulse, SpriteEffects.None, 0f);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
    }
    
    #endregion
}
