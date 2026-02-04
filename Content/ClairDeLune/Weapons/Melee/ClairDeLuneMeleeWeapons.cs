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
using MagnumOpus.Content.ClairDeLune.Projectiles;
using MagnumOpus.Content.ClairDeLune.ResonanceEnergies;
using MagnumOpus.Content.ClairDeLune.HarmonicCores;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.ClairDeLune.Weapons.Melee
{
    #region Chronologicality - Time-Rending Drill
    
    /// <summary>
    /// Chronologicality - SUPREME FINAL BOSS drill-style chainsaw
    /// A time-rending mechanism that tears through the fabric of reality
    /// Held like a drill, creates temporal rifts and clockwork devastation
    /// Fires clockwork gear projectiles that chain through time echoes
    /// MUST EXCEED Ode to Joy: Rose Thorn Chainsaw (4400 damage)
    /// </summary>
    public class Chronologicality : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
            ItemID.Sets.IsDrill[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 5200; // FINAL BOSS - 18% above Rose Thorn (4400)
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.width = 58;
            Item.height = 28;
            
            // Fast drill timing
            Item.useTime = 1;
            Item.useAnimation = 1;
            
            // DRILL-SPECIFIC settings
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.channel = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item22;
            Item.autoReuse = true;
            Item.crit = 26; // Higher than Ode to Joy (22)

            Item.shoot = ModContent.ProjectileType<ChronologicalityProjectile>();
            Item.shootSpeed = 45f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Rapidly tears through enemies with temporal gears"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Spawns clockwork rifts that chain through time echoes"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Contact creates temporal fractures that slow enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Critical hits trigger massive lightning discharges"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Where time itself becomes the blade'") 
            { 
                OverrideColor = ClairDeLuneColors.Crimson 
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfClairDeLune>(), 30)
                .AddIngredient(ModContent.ItemType<ClairDeLuneResonantEnergy>(), 25)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfClairDeLune>(), 4)
                .AddIngredient(ItemID.LunarBar, 25)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    /// <summary>
    /// Chronologicality held projectile - drill AI with temporal rift spawning
    /// </summary>
    public class ChronologicalityProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/ClairDeLune/Weapons/Melee/Chronologicality";

        private int riftTimer = 0;
        private const int RiftInterval = 10; // Faster than Rose Thorn
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 58;
            Projectile.height = 28;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.ownerHitCheck = true;
            Projectile.aiStyle = 20;
            Projectile.hide = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            
            // Spawn temporal rift projectiles periodically
            riftTimer++;
            if (riftTimer >= RiftInterval)
            {
                riftTimer = 0;
                
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 riftDir = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
                    Vector2 spawnPos = player.Center + riftDir * 65f;
                    
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        spawnPos,
                        riftDir * 20f,
                        ModContent.ProjectileType<TemporalRiftProjectile>(),
                        Projectile.damage / 3,
                        Projectile.knockBack / 2f,
                        Projectile.owner
                    );
                }
                
                // === SPECTACULAR VFX BURST ===
                Vector2 burstPos = player.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 45f;
                
                // Clockwork gear cascade
                ClairDeLuneVFX.ClockworkGearCascade(burstPos, 6, 5f, 0.7f);
                
                // Lightning burst
                ClairDeLuneVFX.SpawnLightningBurst(burstPos, Projectile.velocity * 0.3f, false, 0.6f);
            }
            
            // === DENSE TEMPORAL PARTICLE TRAIL ===
            // Heavy clockwork trail every frame
            for (int i = 0; i < 3; i++)
            {
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(18f, 18f);
                Vector2 dustVel = Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedByRandom(0.6f) * Main.rand.NextFloat(3f, 7f);
                
                Color trailColor = ClairDeLuneColors.GetGradient(Main.rand.NextFloat());
                var particle = new GenericGlowParticle(dustPos, dustVel, trailColor, 0.4f, 20, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }
            
            // Crimson energy sparks
            if (Main.rand.NextBool(2))
            {
                Vector2 sparkPos = Projectile.Center + Main.rand.NextVector2Circular(15f, 15f);
                var crimson = new GenericGlowParticle(
                    sparkPos,
                    Main.rand.NextVector2Circular(4f, 4f) - Vector2.UnitY * 2f,
                    ClairDeLuneColors.Crimson * 0.8f,
                    0.35f,
                    22,
                    true
                );
                MagnumParticleHandler.SpawnParticle(crimson);
            }
            
            // Crystal sparkles
            if (Main.rand.NextBool(2))
            {
                Vector2 sparklePos = Projectile.Center + Main.rand.NextVector2Circular(22f, 22f);
                var sparkle = new SparkleParticle(
                    sparklePos,
                    Main.rand.NextVector2Circular(3f, 3f),
                    ClairDeLuneColors.Crystal,
                    0.45f,
                    25
                );
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Brass gear particles
            if (Main.rand.NextBool(3))
            {
                var gear = new GenericGlowParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(20f, 20f),
                    Main.rand.NextVector2Circular(3f, 3f),
                    ClairDeLuneColors.Brass * 0.7f,
                    0.3f,
                    18,
                    true
                );
                MagnumParticleHandler.SpawnParticle(gear);
            }
            
            // Electric dust for density
            for (int i = 0; i < 2; i++)
            {
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(12f, 12f),
                    DustID.Electric,
                    Main.rand.NextVector2Circular(5f, 5f),
                    0, ClairDeLuneColors.ElectricBlue, 1.6f
                );
                dust.noGravity = true;
            }
            
            // Music notes occasionally
            if (Main.rand.NextBool(6))
            {
                ClairDeLuneVFX.SpawnMusicNote(
                    Projectile.Center + Main.rand.NextVector2Circular(30f, 30f),
                    Main.rand.NextVector2Circular(2.5f, 2.5f),
                    ClairDeLuneColors.Crimson,
                    0.8f
                );
            }
            
            // Orbiting gears visual
            if (Main.GameUpdateCount % 4 == 0)
            {
                ClairDeLuneVFX.OrbitingGears(Projectile.Center, 35f, 4, Main.GameUpdateCount * 0.05f, 0.5f);
            }
            
            // Intense lighting
            Lighting.AddLight(Projectile.Center, ClairDeLuneColors.Crimson.ToVector3() * 1.2f);
            Lighting.AddLight(Projectile.Center, ClairDeLuneColors.ElectricBlue.ToVector3() * 0.5f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply temporal slow debuff
            target.AddBuff(BuffID.Slow, 240);
            target.AddBuff(BuffID.Frostburn2, 180);
            
            // === IMPACT VFX ===
            ClairDeLuneVFX.TemporalImpact(target.Center, 0.7f);
            
            // Critical hit = massive lightning discharge
            if (hit.Crit)
            {
                ClairDeLuneVFX.LightningStrikeExplosion(target.Center, 0.8f);
                
                // Extra clockwork cascade on crit
                ClairDeLuneVFX.ClockworkGearCascade(target.Center, 10, 8f, 0.9f);
                
                // Extra rift on crit
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 riftDir = (target.Center - Main.player[Projectile.owner].Center).SafeNormalize(Vector2.UnitX);
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        target.Center,
                        riftDir.RotatedByRandom(0.4f) * 15f,
                        ModContent.ProjectileType<TemporalRiftProjectile>(),
                        Projectile.damage / 4,
                        3f,
                        Projectile.owner
                    );
                }
                
                SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.8f, Pitch = -0.2f }, target.Center);
            }
        }
    }

    /// <summary>
    /// Temporal Rift Projectile - Chains through time echoes between enemies
    /// </summary>
    public class TemporalRiftProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/ClockworkGearLarge";
        
        private int bounceCount = 0;
        private const int MaxBounces = 6; // More than Rose Thorn
        private NPC lastHitTarget = null;
        private float rotation = 0f;

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = MaxBounces + 1;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            rotation += 0.15f;
            Projectile.rotation = rotation;
            
            // === CLOCKWORK TEMPORAL TRAIL ===
            ClairDeLuneVFX.TemporalTrail(Projectile.Center, Projectile.velocity, 0.9f);
            
            // Extra gear particles
            if (Main.rand.NextBool(4))
            {
                ClairDeLuneVFX.SpawnClockworkGear(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    -Projectile.velocity * 0.2f,
                    false, 0.5f
                );
            }
            
            // Lightning micro-arcs
            if (Main.rand.NextBool(6))
            {
                ClairDeLuneVFX.SpawnLightningBurst(
                    Projectile.Center,
                    Main.rand.NextVector2Circular(3f, 3f),
                    false, 0.4f
                );
            }
            
            Lighting.AddLight(Projectile.Center, ClairDeLuneColors.Brass.ToVector3() * 0.6f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            bounceCount++;
            lastHitTarget = target;
            
            // Impact VFX
            ClairDeLuneVFX.TemporalImpact(target.Center, 0.5f);
            
            // Apply temporal debuffs
            target.AddBuff(BuffID.Slow, 180);
            
            // Chain to next enemy through "time echo"
            if (bounceCount <= MaxBounces)
            {
                NPC nextTarget = FindNextTarget(target);
                if (nextTarget != null)
                {
                    Vector2 newDir = (nextTarget.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    Projectile.velocity = newDir * Projectile.velocity.Length();
                    
                    // Lightning arc chain effect between targets
                    ClairDeLuneVFX.LightningArc(target.Center, nextTarget.Center, 6, 15f, 0.6f);
                    
                    // Gear connection line
                    for (int i = 0; i < 10; i++)
                    {
                        float progress = i / 10f;
                        Vector2 linePos = Vector2.Lerp(target.Center, nextTarget.Center, progress);
                        
                        Color lineColor = ClairDeLuneColors.GetGradient(progress);
                        var lineParticle = new GenericGlowParticle(
                            linePos + Main.rand.NextVector2Circular(6f, 6f),
                            Vector2.Zero,
                            lineColor * 0.7f,
                            0.25f,
                            12,
                            true
                        );
                        MagnumParticleHandler.SpawnParticle(lineParticle);
                    }
                }
            }
        }

        private NPC FindNextTarget(NPC excludeTarget)
        {
            float maxRange = 500f; // Longer range than Rose Thorn
            NPC closest = null;
            float closestDist = maxRange;
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc == excludeTarget || npc == lastHitTarget)
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

        public override void OnKill(int timeLeft)
        {
            ClairDeLuneVFX.TemporalDeathExplosion(Projectile.Center, 0.8f);
            SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.6f, Pitch = 0.4f }, Projectile.Center);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            // Multi-layer spinning gear draw
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = tex.Size() / 2f;
            
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.15f;
            
            // Additive bloom layers
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Outer crimson glow
            Color outerGlow = ClairDeLuneColors.Crimson * 0.4f;
            outerGlow.A = 0;
            Main.spriteBatch.Draw(tex, drawPos, null, outerGlow, rotation * 0.7f, origin, 1.5f * pulse, SpriteEffects.None, 0f);
            
            // Middle brass glow
            Color midGlow = ClairDeLuneColors.Brass * 0.6f;
            midGlow.A = 0;
            Main.spriteBatch.Draw(tex, drawPos, null, midGlow, rotation, origin, 1.2f * pulse, SpriteEffects.None, 0f);
            
            // Inner crystal glow
            Color innerGlow = ClairDeLuneColors.Crystal * 0.7f;
            innerGlow.A = 0;
            Main.spriteBatch.Draw(tex, drawPos, null, innerGlow, rotation * 1.3f, origin, 1f * pulse, SpriteEffects.None, 0f);
            
            // White core
            Color coreGlow = ClairDeLuneColors.BrightWhite * 0.8f;
            coreGlow.A = 0;
            Main.spriteBatch.Draw(tex, drawPos, null, coreGlow, rotation, origin, 0.6f, SpriteEffects.None, 0f);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
    }

    #endregion

    #region Temporal Piercer - Time-Shattering Lance
    
    /// <summary>
    /// Temporal Piercer - SUPREME FINAL BOSS spear/lance weapon
    /// A crystalline lance that pierces through time itself
    /// Thrusting attacks that create temporal shockwaves
    /// Every 4th thrust triggers a massive time fracture explosion
    /// MUST EXCEED Ode to Joy damage standards
    /// </summary>
    public class TemporalPiercer : ModItem
    {
        private int thrustCounter = 0;

        public override void SetDefaults()
        {
            Item.width = 84;
            Item.height = 84;
            Item.damage = 4900; // FINAL BOSS tier
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 7f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item71;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.crit = 22;
            Item.shoot = ModContent.ProjectileType<TemporalPiercerProjectile>();
            Item.shootSpeed = 5f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Thrusting attacks pierce through time itself"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Creates temporal shockwaves on each thrust"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Every 4th thrust triggers a massive time fracture"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Time fractures slow all nearby enemies"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The spear that shatters the hourglass'") 
            { 
                OverrideColor = ClairDeLuneColors.Crystal 
            });
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            thrustCounter++;
            
            // Always fire the lance projectile
            Vector2 mouseDir = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
            Projectile.NewProjectile(source, player.Center, mouseDir * Item.shootSpeed, type, damage, knockback, player.whoAmI, thrustCounter >= 4 ? 1f : 0f);
            
            // VFX for thrust
            ClairDeLuneVFX.CrystalShatterBurst(player.Center + mouseDir * 50f, 5, 5f, 0.6f);
            
            // Every 4th thrust - massive time fracture
            if (thrustCounter >= 4)
            {
                thrustCounter = 0;
                
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.3f }, player.Center);
                
                // SIGNATURE TEMPORAL EXPLOSION
                ClairDeLuneVFX.TemporalDeathExplosion(player.Center + mouseDir * 80f, 1.3f);
                
                // Music note spiral
                ClairDeLuneVFX.MusicNoteBurst(player.Center + mouseDir * 80f, 12, 7f, 1f);
                
                // Lightning storm
                ClairDeLuneVFX.LightningStrikeExplosion(player.Center + mouseDir * 80f, 1f);
                
                // Spawn area-slow projectile
                Projectile.NewProjectile(source, player.Center + mouseDir * 80f, Vector2.Zero,
                    ModContent.ProjectileType<TimeFractureExplosion>(), damage * 2, knockback * 2, player.whoAmI);
            }
            
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfClairDeLune>(), 28)
                .AddIngredient(ModContent.ItemType<ClairDeLuneResonantEnergy>(), 22)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfClairDeLune>(), 3)
                .AddIngredient(ItemID.LunarBar, 22)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    /// <summary>
    /// Temporal Piercer thrust projectile
    /// </summary>
    public class TemporalPiercerProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/ClairDeLune/Weapons/Melee/TemporalPiercer";

        private bool IsPowerThrust => Projectile.ai[0] == 1f;
        private float thrustProgress = 0f;

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 20;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            
            // Thrust forward animation
            thrustProgress += 0.05f;
            float thrustDistance = (float)Math.Sin(thrustProgress * MathHelper.Pi) * 80f;
            
            Projectile.Center = player.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * (40f + thrustDistance);
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            
            // Keep player facing direction
            player.ChangeDir(Projectile.velocity.X > 0 ? 1 : -1);
            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;
            player.itemRotation = Projectile.velocity.ToRotation();
            if (player.direction != 1)
                player.itemRotation -= MathHelper.Pi;
            
            // === SPECTACULAR TRAIL ===
            if (IsPowerThrust)
            {
                // Extra heavy trail for power thrust
                ClairDeLuneVFX.HeavyTemporalTrail(Projectile.Center, Projectile.velocity, 1.2f);
                ClairDeLuneVFX.OrbitingGears(Projectile.Center, 25f, 6, Main.GameUpdateCount * 0.08f, 0.6f);
            }
            else
            {
                ClairDeLuneVFX.TemporalTrail(Projectile.Center, Projectile.velocity, 1f);
            }
            
            // Crystal sparks along thrust
            if (Main.rand.NextBool(2))
            {
                ClairDeLuneVFX.SpawnCrystalShard(
                    Projectile.Center + Main.rand.NextVector2Circular(15f, 15f),
                    Projectile.velocity * 0.3f + Main.rand.NextVector2Circular(4f, 4f),
                    false, 0.5f
                );
            }
            
            // Lightning crackle at tip
            if (Main.rand.NextBool(3))
            {
                ClairDeLuneVFX.SpawnLightningBurst(
                    Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 30f,
                    Projectile.velocity * 0.2f,
                    false, 0.5f
                );
            }
            
            Lighting.AddLight(Projectile.Center, ClairDeLuneColors.Crystal.ToVector3() * 1f);
            Lighting.AddLight(Projectile.Center, ClairDeLuneColors.Crimson.ToVector3() * 0.6f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply slow debuffs
            target.AddBuff(BuffID.Slow, 180);
            
            // Impact VFX based on thrust type
            if (IsPowerThrust)
            {
                ClairDeLuneVFX.TemporalImpact(target.Center, 1f);
                ClairDeLuneVFX.LightningStrikeExplosion(target.Center, 0.7f);
            }
            else
            {
                ClairDeLuneVFX.TemporalImpact(target.Center, 0.6f);
            }
            
            // Crystal shatter on hit
            ClairDeLuneVFX.CrystalShatterBurst(target.Center, 8, 6f, 0.6f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = tex.Size() / 2f;
            
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.1f;
            
            // Additive glow layers
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Outer glow
            Color outerGlow = (IsPowerThrust ? ClairDeLuneColors.Crimson : ClairDeLuneColors.Crystal) * 0.4f;
            outerGlow.A = 0;
            Main.spriteBatch.Draw(tex, drawPos, null, outerGlow, Projectile.rotation, origin, 1.3f * pulse, SpriteEffects.None, 0f);
            
            // Middle glow
            Color midGlow = ClairDeLuneColors.MoonlightSilver * 0.5f;
            midGlow.A = 0;
            Main.spriteBatch.Draw(tex, drawPos, null, midGlow, Projectile.rotation, origin, 1.15f * pulse, SpriteEffects.None, 0f);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Main sprite
            Main.spriteBatch.Draw(tex, drawPos, null, lightColor, Projectile.rotation, origin, 1f, SpriteEffects.None, 0f);
            
            return false;
        }
    }

    /// <summary>
    /// Time Fracture Explosion - Area damage + slow
    /// </summary>
    public class TimeFractureExplosion : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/LightningBurst";

        public override void SetDefaults()
        {
            Projectile.width = 200;
            Projectile.height = 200;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 10;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            // Expanding temporal distortion visual
            if (Projectile.timeLeft == 10)
            {
                ClairDeLuneVFX.TemporalImpact(Projectile.Center, 1.5f);
            }
            
            Projectile.alpha += 25;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Slow, 300);
            target.AddBuff(BuffID.Frostburn2, 240);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }

    #endregion

    #region Clockwork Harmony - Mechanical Greatsword
    
    /// <summary>
    /// Clockwork Harmony - SUPREME FINAL BOSS greatsword
    /// A massive mechanical blade with interlocking gears
    /// Swings release cascading gear waves
    /// Every 5th swing creates a synchronized clockwork explosion
    /// Gears mark enemies, marked enemies take bonus damage
    /// MUST EXCEED Ode to Joy: Thornbound Reckoning (4200)
    /// </summary>
    public class ClockworkHarmony : ModItem
    {
        private int swingCounter = 0;

        public override void SetDefaults()
        {
            Item.width = 88;
            Item.height = 88;
            Item.damage = 4950; // FINAL BOSS - 18% above Thornbound (4200)
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 9f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item71;
            Item.autoReuse = true;
            Item.useTurn = false;
            Item.scale = 1.6f;
            Item.crit = 22;
            Item.shoot = ModContent.ProjectileType<GearWaveProjectile>();
            Item.shootSpeed = 16f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "On swing releases cascading gear waves"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Every 5th swing creates a synchronized explosion"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Gears mark enemies - marked take 35% more damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Marked enemies explode when hit again"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The symphony of perfect mechanism'") 
            { 
                OverrideColor = ClairDeLuneColors.Brass 
            });
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            swingCounter++;
            
            // Always fire gear wave
            Vector2 mouseDir = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
            Projectile.NewProjectile(source, player.Center, mouseDir * Item.shootSpeed, 
                ModContent.ProjectileType<GearWaveProjectile>(), damage, knockback, player.whoAmI);
            
            // VFX for swing
            ClairDeLuneVFX.ClockworkGearCascade(player.Center + mouseDir * 55f, 5, 5f, 0.7f);
            SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.2f }, player.Center);
            
            // Every 5th swing - synchronized clockwork explosion
            if (swingCounter >= 5)
            {
                swingCounter = 0;
                
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0f }, player.Center);
                
                // SIGNATURE CLOCKWORK EXPLOSION
                Vector2 explosionPos = player.Center + mouseDir * 70f;
                ClairDeLuneVFX.TemporalDeathExplosion(explosionPos, 1.4f);
                
                // Clockwork cascade in all directions
                ClairDeLuneVFX.ClockworkGearCascade(explosionPos, 20, 12f, 1.2f);
                
                // Lightning web
                for (int i = 0; i < 10; i++)
                {
                    float angle = MathHelper.TwoPi * i / 10f;
                    Vector2 endPos = explosionPos + angle.ToRotationVector2() * 120f;
                    ClairDeLuneVFX.LightningArc(explosionPos, endPos, 5, 12f, 0.7f);
                }
                
                // Music note spiral
                for (int ring = 0; ring < 3; ring++)
                {
                    int notesInRing = 6 + ring * 2;
                    float ringRadius = 50f + ring * 35f;
                    for (int i = 0; i < notesInRing; i++)
                    {
                        float angle = MathHelper.TwoPi * i / notesInRing + ring * 0.4f;
                        Vector2 notePos = explosionPos + angle.ToRotationVector2() * ringRadius;
                        ClairDeLuneVFX.SpawnMusicNote(notePos, angle.ToRotationVector2() * 5f, ClairDeLuneColors.GetGradient(Main.rand.NextFloat()), 1f);
                    }
                }
                
                // Spawn synchronized explosion projectile
                Projectile.NewProjectile(source, explosionPos, Vector2.Zero,
                    ModContent.ProjectileType<SynchronizedGearExplosion>(), damage * 3, knockback * 3, player.whoAmI);
            }
            
            return false;
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            // Heavy melee trail
            for (int i = 0; i < 3; i++)
            {
                Vector2 dustPos = hitbox.Center.ToVector2() + Main.rand.NextVector2Circular(hitbox.Width / 2, hitbox.Height / 2);
                Color trailColor = ClairDeLuneColors.GetGearGradient(Main.rand.NextFloat());
                
                var particle = new GenericGlowParticle(dustPos, player.velocity * 0.4f + Main.rand.NextVector2Circular(3f, 3f), trailColor, 0.45f, 18, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }
            
            // Gear sparkles
            if (Main.rand.NextBool(2))
            {
                var sparkle = new SparkleParticle(
                    hitbox.Center.ToVector2() + Main.rand.NextVector2Circular(20f, 20f),
                    Main.rand.NextVector2Circular(3f, 3f),
                    ClairDeLuneColors.Crystal,
                    0.45f,
                    20
                );
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Lightning crackles
            if (Main.rand.NextBool(4))
            {
                ClairDeLuneVFX.SpawnLightningBurst(
                    hitbox.Center.ToVector2() + Main.rand.NextVector2Circular(25f, 25f),
                    Main.rand.NextVector2Circular(4f, 4f),
                    false, 0.5f
                );
            }
            
            // Music notes in swing
            if (Main.rand.NextBool(5))
            {
                ClairDeLuneVFX.SpawnMusicNote(
                    hitbox.Center.ToVector2() + Main.rand.NextVector2Circular(20f, 20f),
                    Main.rand.NextVector2Circular(3f, 3f),
                    ClairDeLuneColors.Brass,
                    0.85f
                );
            }
            
            Lighting.AddLight(hitbox.Center.ToVector2(), ClairDeLuneColors.Brass.ToVector3() * 0.8f);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfClairDeLune>(), 32)
                .AddIngredient(ModContent.ItemType<ClairDeLuneResonantEnergy>(), 24)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfClairDeLune>(), 4)
                .AddIngredient(ItemID.LunarBar, 25)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    /// <summary>
    /// Gear Wave Projectile - Cascading gear that marks enemies
    /// </summary>
    public class GearWaveProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/ClockworkGearLarge";
        
        private float rotation = 0f;

        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 48;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 8;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            rotation += 0.12f;
            Projectile.rotation = rotation;
            
            // Slight homing to nearby enemies
            NPC closest = null;
            float closestDist = 300f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && !npc.dontTakeDamage)
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }
            
            if (closest != null)
            {
                Vector2 toTarget = (closest.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), 0.02f);
            }
            
            // === CLOCKWORK TRAIL ===
            ClairDeLuneVFX.HeavyTemporalTrail(Projectile.Center, Projectile.velocity, 0.8f);
            
            // Orbiting mini-gears
            if (Main.GameUpdateCount % 3 == 0)
            {
                ClairDeLuneVFX.OrbitingGears(Projectile.Center, 30f, 3, Main.GameUpdateCount * 0.07f, 0.4f);
            }
            
            Lighting.AddLight(Projectile.Center, ClairDeLuneColors.Brass.ToVector3() * 0.8f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Mark enemy for bonus damage
            target.AddBuff(BuffID.BetsysCurse, 300); // 35% increased damage taken
            target.AddBuff(BuffID.Slow, 180);
            
            // Impact VFX
            ClairDeLuneVFX.TemporalImpact(target.Center, 0.7f);
            ClairDeLuneVFX.ClockworkGearCascade(target.Center, 8, 6f, 0.7f);
        }

        public override void OnKill(int timeLeft)
        {
            ClairDeLuneVFX.TemporalDeathExplosion(Projectile.Center, 0.7f);
            SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.5f, Pitch = 0.3f }, Projectile.Center);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = tex.Size() / 2f;
            
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.12f;
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Brass outer glow
            Color outerGlow = ClairDeLuneColors.Brass * 0.5f;
            outerGlow.A = 0;
            Main.spriteBatch.Draw(tex, drawPos, null, outerGlow, rotation * 0.8f, origin, 1.6f * pulse, SpriteEffects.None, 0f);
            
            // Gold middle glow
            Color midGlow = ClairDeLuneColors.GearGold * 0.6f;
            midGlow.A = 0;
            Main.spriteBatch.Draw(tex, drawPos, null, midGlow, rotation, origin, 1.3f * pulse, SpriteEffects.None, 0f);
            
            // Crystal inner glow
            Color innerGlow = ClairDeLuneColors.Crystal * 0.7f;
            innerGlow.A = 0;
            Main.spriteBatch.Draw(tex, drawPos, null, innerGlow, rotation * 1.2f, origin, 1f * pulse, SpriteEffects.None, 0f);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
    }

    /// <summary>
    /// Synchronized Gear Explosion - Massive area damage
    /// </summary>
    public class SynchronizedGearExplosion : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/ClockworkGearLarge";

        public override void SetDefaults()
        {
            Projectile.width = 280;
            Projectile.height = 280;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 12;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            // Expanding shockwave visual
            if (Projectile.timeLeft == 12)
            {
                ClairDeLuneVFX.TemporalImpact(Projectile.Center, 2f);
                ClairDeLuneVFX.ClockworkGearCascade(Projectile.Center, 25, 15f, 1.5f);
            }
            
            Projectile.alpha += 21;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.BetsysCurse, 600); // Mark for extended duration
            target.AddBuff(BuffID.Slow, 360);
            
            // Marked enemies explode
            ClairDeLuneVFX.ClockworkGearCascade(target.Center, 6, 5f, 0.6f);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }

    #endregion
}
