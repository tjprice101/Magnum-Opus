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
using MagnumOpus.Content.OdeToJoy.Projectiles;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;
using MagnumOpus.Content.OdeToJoy.HarmonicCores;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.OdeToJoy.Weapons.Ranged
{
    #region The Pollinator
    
    /// <summary>
    /// The Pollinator - Seed launcher that explodes into petal storms
    /// Fires seeking pollen seeds that burst into multiple homing petals
    /// Seeds mark enemies - marked enemies take bonus petal damage
    /// </summary>
    public class ThePollinator : ModItem
    {
        private int shotCounter = 0;

        public override void SetDefaults()
        {
            Item.width = 58;
            Item.height = 32;
            Item.damage = 3200; // Post-Dies Irae tier ranged
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
            Item.UseSound = SoundID.Item61;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<PollenSeedProjectile>();
            Item.shootSpeed = 16f;
            Item.useAmmo = AmmoID.Bullet;
            Item.crit = 20;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Converts bullets into seeking pollen seeds"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Seeds burst into homing petal swarms on impact"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Marked enemies take 25% bonus petal damage"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Spreading joy, one bloom at a time'") 
            { 
                OverrideColor = OdeToJoyColors.GoldenPollen 
            });
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            type = ModContent.ProjectileType<PollenSeedProjectile>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            shotCounter++;
            
            Vector2 muzzlePos = position + velocity.SafeNormalize(Vector2.UnitX) * 45f;
            
            // Fire main pollen seed
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            
            // Muzzle flash VFX - enhanced chromatic petal burst
            OdeToJoyProjectiles.ChromaticRosePetalBurst(muzzlePos, 6, 4f, 0.45f, false);
            
            // Harmonic note sparkle with music notes
            OdeToJoyProjectiles.HarmonicNoteSparkle(muzzlePos, 4, 3f, 0.4f, false);
            
            // Every 4th shot fires a burst of 3 seeds
            if (shotCounter >= 4)
            {
                shotCounter = 0;
                
                SoundEngine.PlaySound(SoundID.Item62 with { Pitch = 0.2f }, position);
                
                for (int i = 0; i < 2; i++)
                {
                    Vector2 spreadVel = velocity.RotatedBy(MathHelper.ToRadians(-15f + i * 30f)) * 0.9f;
                    Projectile.NewProjectile(source, position, spreadVel, type, damage * 3 / 4, knockback / 2, player.whoAmI);
                }
                
                OdeToJoyProjectiles.OdeToJoySignatureExplosion(muzzlePos, 0.65f);
            }
            
            return false;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-8f, 0f);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            float time = Main.GameUpdateCount * 0.05f;
            float pulse = 1f + (float)Math.Sin(time * 2f) * 0.08f;
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            spriteBatch.Draw(texture, position, null, OdeToJoyColors.GoldenPollen * 0.35f, rotation, origin, scale * pulse * 1.25f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, OdeToJoyColors.VerdantGreen * 0.25f, rotation, origin, scale * pulse * 1.12f, SpriteEffects.None, 0f);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Lighting.AddLight(Item.Center, OdeToJoyColors.GoldenPollen.ToVector3() * 0.4f);
            
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfOdeToJoy>(), 20)
                .AddIngredient(ModContent.ItemType<OdeToJoyResonantEnergy>(), 15)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfOdeToJoy>(), 2)
                .AddIngredient(ItemID.LunarBar, 15)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    /// <summary>
    /// Pollen Seed Projectile - Seeks targets, bursts into petals
    /// </summary>
    public class PollenSeedProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/EnergyFlare3";

        private float homingStrength = 0.02f;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation += 0.15f;
            
            // Gentle homing
            NPC target = FindClosestNPC(600f);
            if (target != null)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), homingStrength);
            }
            
            // Trail particles
            if (Main.rand.NextBool(2))
            {
                Color trailColor = Color.Lerp(OdeToJoyColors.GoldenPollen, OdeToJoyColors.VerdantGreen, Main.rand.NextFloat());
                var trail = new GenericGlowParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(5f, 5f),
                    -Projectile.velocity * 0.1f,
                    trailColor * 0.7f,
                    0.3f,
                    15,
                    true
                );
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // Pollen dust
            if (Main.rand.NextBool(3))
            {
                Dust pollen = Dust.NewDustPerfect(Projectile.Center, DustID.GoldCoin, 
                    Main.rand.NextVector2Circular(1f, 1f), 100, OdeToJoyColors.GoldenPollen, 0.8f);
                pollen.noGravity = true;
            }
            
            Lighting.AddLight(Projectile.Center, OdeToJoyColors.GoldenPollen.ToVector3() * 0.5f);
        }

        private NPC FindClosestNPC(float maxRange)
        {
            NPC closest = null;
            float closestDist = maxRange;
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage)
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

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Mark enemy with pollen
            target.AddBuff(BuffID.Poisoned, 300);
        }

        public override void OnKill(int timeLeft)
        {
            // Burst into homing petals
            SoundEngine.PlaySound(SoundID.Grass with { Volume = 0.7f }, Projectile.Center);
            
            int petalCount = 6;
            for (int i = 0; i < petalCount; i++)
            {
                float angle = MathHelper.TwoPi * i / petalCount + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 petalVel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 10f);
                
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    petalVel,
                    ModContent.ProjectileType<HomingPetalProjectile>(),
                    Projectile.damage / 3,
                    Projectile.knockBack / 3,
                    Projectile.owner
                );
            }
            
            // Use the Chromatic Rose Petal Burst AND Harmonic Note Sparkle together!
            OdeToJoyVFX.ChromaticRosePetalBurst(Projectile.Center, 12, 6f, 0.8f, true);
            OdeToJoyProjectiles.HarmonicNoteSparkle(Projectile.Center, 10, 5f, 0.6f, true);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.15f;
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Golden glow layers
            Main.spriteBatch.Draw(texture, drawPos, null, OdeToJoyColors.GoldenPollen * 0.5f, Projectile.rotation, origin, 0.6f * pulse, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(texture, drawPos, null, OdeToJoyColors.VerdantGreen * 0.4f, Projectile.rotation * 0.7f, origin, 0.5f * pulse, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(texture, drawPos, null, Color.White * 0.6f, Projectile.rotation * 1.3f, origin, 0.3f * pulse, SpriteEffects.None, 0f);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
    }

    /// <summary>
    /// Homing Petal Projectile - Spawned from pollen seeds
    /// </summary>
    public class HomingPetalProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/OdeToJoy/Projectiles/SmallPetalProjectile";

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Projectile.rotation += 0.2f;
            
            // Strong homing
            NPC target = FindClosestNPC(500f);
            if (target != null)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 14f, 0.08f);
            }
            
            // Trail
            if (Main.rand.NextBool(2))
            {
                Color trailColor = OdeToJoyColors.GetPetalGradient(Main.rand.NextFloat());
                var trail = new GenericGlowParticle(
                    Projectile.Center,
                    -Projectile.velocity * 0.1f,
                    trailColor * 0.6f,
                    0.2f,
                    12,
                    true
                );
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            Lighting.AddLight(Projectile.Center, OdeToJoyColors.RosePink.ToVector3() * 0.3f);
        }

        private NPC FindClosestNPC(float maxRange)
        {
            NPC closest = null;
            float closestDist = maxRange;
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage)
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

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 120);
            // Use Chromatic Petal Burst for homing petal impact!
            OdeToJoyVFX.ChromaticRosePetalBurst(target.Center, 6, 3f, 0.4f, false);
        }

        public override void OnKill(int timeLeft)
        {
            OdeToJoyVFX.ChromaticRosePetalBurst(Projectile.Center, 8, 3f, 0.5f, false);
        }
    }
    
    #endregion

    #region Petal Storm Cannon
    
    /// <summary>
    /// Petal Storm Cannon - Heavy cannon that fires explosive petal bombs
    /// Each bomb creates a lingering petal storm that damages enemies over time
    /// </summary>
    public class PetalStormCannon : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 76;
            Item.height = 34;
            Item.damage = 4800; // Post-Dies Irae heavy cannon
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 50;
            Item.useAnimation = 50;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 10f;
            Item.value = Item.sellPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
            Item.UseSound = SoundID.Item38 with { Pitch = 0.1f };
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<PetalBombProjectile>();
            Item.shootSpeed = 12f;
            Item.useAmmo = AmmoID.Rocket;
            Item.crit = 15;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires explosive petal bombs"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Explosions create lingering petal storms"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Petal storms deal damage over time to enemies inside"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'When joy erupts, all feel its embrace'") 
            { 
                OverrideColor = OdeToJoyColors.RosePink 
            });
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            type = ModContent.ProjectileType<PetalBombProjectile>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 muzzlePos = position + velocity.SafeNormalize(Vector2.UnitX) * 55f;
            
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            
            // Massive muzzle flash - chromatic petal burst
            OdeToJoyProjectiles.ChromaticRosePetalBurst(muzzlePos, 10, 6f, 0.85f, true);
            
            // Petal burst
            for (int i = 0; i < 8; i++)
            {
                Vector2 petalVel = velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.8f, 0.8f)) * Main.rand.NextFloat(2f, 5f);
                var petal = new GenericGlowParticle(muzzlePos, petalVel, OdeToJoyColors.GetPetalGradient(Main.rand.NextFloat()) * 0.7f, 0.4f, 25, true);
                MagnumParticleHandler.SpawnParticle(petal);
            }
            
            // Music note burst with harmonic sparkle
            OdeToJoyProjectiles.HarmonicNoteSparkle(muzzlePos, 6, 4f, 0.7f, false);
            
            return false;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-12f, 0f);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfOdeToJoy>(), 20)
                .AddIngredient(ModContent.ItemType<OdeToJoyResonantEnergy>(), 15)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfOdeToJoy>(), 2)
                .AddIngredient(ItemID.LunarBar, 15)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    /// <summary>
    /// Petal Bomb Projectile - Explodes into a lingering petal storm
    /// </summary>
    public class PetalBombProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow2";

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Projectile.rotation += 0.1f;
            Projectile.velocity.Y += 0.15f; // Gravity
            
            // Trail
            for (int i = 0; i < 2; i++)
            {
                Color trailColor = OdeToJoyColors.GetGradient(Main.rand.NextFloat());
                var trail = new GenericGlowParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    -Projectile.velocity * 0.15f,
                    trailColor * 0.6f,
                    0.35f,
                    18,
                    true
                );
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // Sparkles
            if (Main.rand.NextBool(2))
            {
                var sparkle = new SparkleParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    Main.rand.NextVector2Circular(1f, 1f),
                    OdeToJoyColors.GoldenPollen,
                    0.3f,
                    15
                );
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            Lighting.AddLight(Projectile.Center, OdeToJoyColors.RosePink.ToVector3() * 0.6f);
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.2f }, Projectile.Center);
            
            // Spawn lingering petal storm
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                Vector2.Zero,
                ModContent.ProjectileType<LingeringPetalStorm>(),
                Projectile.damage / 4,
                0f,
                Projectile.owner
            );
            
            // Explosion VFX - SIGNATURE EXPLOSION
            OdeToJoyProjectiles.OdeToJoySignatureExplosion(Projectile.Center, 1.0f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.2f;
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Main.spriteBatch.Draw(texture, drawPos, null, OdeToJoyColors.RosePink * 0.6f, Projectile.rotation, origin, 0.8f * pulse, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(texture, drawPos, null, OdeToJoyColors.VerdantGreen * 0.5f, -Projectile.rotation * 0.7f, origin, 0.6f * pulse, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(texture, drawPos, null, Color.White * 0.7f, Projectile.rotation * 1.2f, origin, 0.4f * pulse, SpriteEffects.None, 0f);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
    }

    /// <summary>
    /// Lingering Petal Storm - Damages enemies over time
    /// </summary>
    public class LingeringPetalStorm : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow2";

        public override void SetDefaults()
        {
            Projectile.width = 200;
            Projectile.height = 200;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180; // 3 seconds
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            float lifePercent = Projectile.timeLeft / 180f;
            
            // Swirling petal particles
            if (Main.rand.NextBool(2))
            {
                float angle = Main.GameUpdateCount * 0.1f + Main.rand.NextFloat() * MathHelper.TwoPi;
                float radius = Main.rand.NextFloat(30f, 100f) * lifePercent;
                Vector2 offset = angle.ToRotationVector2() * radius;
                Vector2 vel = offset.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.Zero) * 3f;
                
                Color petalColor = OdeToJoyColors.GetPetalGradient(Main.rand.NextFloat());
                var petal = new GenericGlowParticle(
                    Projectile.Center + offset,
                    vel,
                    petalColor * 0.6f * lifePercent,
                    0.35f,
                    20,
                    true
                );
                MagnumParticleHandler.SpawnParticle(petal);
            }
            
            // Golden sparkles
            if (Main.rand.NextBool(3))
            {
                var sparkle = new SparkleParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(80f, 80f),
                    Main.rand.NextVector2Circular(2f, 2f),
                    OdeToJoyColors.GoldenPollen * lifePercent,
                    0.3f,
                    15
                );
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Music notes
            if (Main.rand.NextBool(8))
            {
                OdeToJoyVFX.SpawnMusicNote(
                    Projectile.Center + Main.rand.NextVector2Circular(60f, 60f),
                    Main.rand.NextVector2Circular(2f, 2f),
                    OdeToJoyColors.RosePink * lifePercent,
                    0.7f
                );
            }
            
            Lighting.AddLight(Projectile.Center, OdeToJoyColors.RosePink.ToVector3() * 0.5f * lifePercent);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 60);
            OdeToJoyProjectiles.ChromaticRosePetalBurst(target.Center, 4, 2f, 0.35f, false);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Don't draw, only particles
            return false;
        }
    }
    
    #endregion

    #region Thorn Spray Repeater
    
    /// <summary>
    /// Thorn Spray Repeater - Fast firing thorn crossbow
    /// Fires rapid thorns that stick to enemies and explode
    /// Stacked thorns deal bonus explosion damage
    /// </summary>
    public class ThornSprayRepeater : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 52;
            Item.height = 26;
            Item.damage = 2400; // Fast fire, lower damage per hit
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 6;
            Item.useAnimation = 6;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 2f;
            Item.value = Item.sellPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
            Item.UseSound = SoundID.Item5;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<ThornBoltProjectile>();
            Item.shootSpeed = 20f;
            Item.useAmmo = AmmoID.Arrow;
            Item.crit = 12;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Converts arrows into rapid thorn bolts"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Thorns stick to enemies and explode after 1 second"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Multiple thorns on same enemy deal +50% explosion damage each"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A thousand thorns, a thousand joys'") 
            { 
                OverrideColor = OdeToJoyColors.VerdantGreen 
            });
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            type = ModContent.ProjectileType<ThornBoltProjectile>();
            // Add slight spread
            velocity = velocity.RotatedByRandom(MathHelper.ToRadians(5f));
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            
            // Small muzzle flash - chromatic vine burst
            Vector2 muzzlePos = position + velocity.SafeNormalize(Vector2.UnitX) * 30f;
            OdeToJoyProjectiles.ChromaticVineGrowthBurst(muzzlePos, 2, 3f, 0.3f, false);
            
            return false;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-6f, 0f);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfOdeToJoy>(), 20)
                .AddIngredient(ModContent.ItemType<OdeToJoyResonantEnergy>(), 15)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfOdeToJoy>(), 2)
                .AddIngredient(ItemID.LunarBar, 15)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    /// <summary>
    /// Thorn Bolt Projectile - Sticks to enemies, explodes
    /// </summary>
    public class ThornBoltProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/OdeToJoy/Projectiles/ThornProjectile";

        private bool stuck = false;
        private NPC stuckTarget = null;
        private Vector2 stuckOffset = Vector2.Zero;
        private int stuckTimer = 0;
        private const int ExplodeTime = 60;

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            if (!stuck)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
                
                // Trail
                if (Main.rand.NextBool(2))
                {
                    var trail = new GenericGlowParticle(
                        Projectile.Center,
                        -Projectile.velocity * 0.1f,
                        OdeToJoyColors.VerdantGreen * 0.6f,
                        0.2f,
                        10,
                        true
                    );
                    MagnumParticleHandler.SpawnParticle(trail);
                }
            }
            else
            {
                // Stuck to enemy
                if (stuckTarget == null || !stuckTarget.active)
                {
                    Explode();
                    return;
                }
                
                Projectile.Center = stuckTarget.Center + stuckOffset;
                stuckTimer++;
                
                // Pulsing warning as explosion approaches
                if (stuckTimer > ExplodeTime - 20)
                {
                    if (Main.rand.NextBool(2))
                    {
                        var warning = new GenericGlowParticle(
                            Projectile.Center + Main.rand.NextVector2Circular(5f, 5f),
                            Main.rand.NextVector2Circular(2f, 2f),
                            OdeToJoyColors.GoldenPollen,
                            0.25f,
                            8,
                            true
                        );
                        MagnumParticleHandler.SpawnParticle(warning);
                    }
                }
                
                if (stuckTimer >= ExplodeTime)
                {
                    Explode();
                }
            }
            
            Lighting.AddLight(Projectile.Center, OdeToJoyColors.VerdantGreen.ToVector3() * 0.3f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!stuck)
            {
                stuck = true;
                stuckTarget = target;
                stuckOffset = Projectile.Center - target.Center;
                Projectile.velocity = Vector2.Zero;
                Projectile.tileCollide = false;
                Projectile.penetrate = -1; // Don't die yet
                
                target.AddBuff(BuffID.Poisoned, 180);
            }
        }

        private void Explode()
        {
            // Count other thorns on same target for bonus damage
            int thornCount = 0;
            if (stuckTarget != null)
            {
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (proj.active && proj.type == Type && proj.whoAmI != Projectile.whoAmI)
                    {
                        if (proj.ModProjectile is ThornBoltProjectile thorn && thorn.stuckTarget == stuckTarget)
                        {
                            thornCount++;
                        }
                    }
                }
            }
            
            float damageMultiplier = 1f + thornCount * 0.5f;
            
            // Explosion
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.6f, Pitch = 0.3f }, Projectile.Center);
            
            if (stuckTarget != null && stuckTarget.active)
            {
                int explosionDamage = (int)(Projectile.damage * damageMultiplier);
                stuckTarget.SimpleStrikeNPC(explosionDamage, 0, false, 0f, DamageClass.Ranged);
            }
            
            // Chromatic explosion based on thorn count
            OdeToJoyProjectiles.HarmonicNoteSparkle(Projectile.Center, 5 + thornCount, 4f, 0.45f + thornCount * 0.08f, true);
            OdeToJoyProjectiles.ChromaticRosePetalBurst(Projectile.Center, 6 + thornCount, 4f, 0.5f + thornCount * 0.1f, false);
            
            Projectile.Kill();
        }

        public override void OnKill(int timeLeft)
        {
            if (!stuck)
            {
                OdeToJoyProjectiles.ChromaticVineGrowthBurst(Projectile.Center, 2, 2f, 0.25f, false);
            }
        }
    }
    
    #endregion
}
