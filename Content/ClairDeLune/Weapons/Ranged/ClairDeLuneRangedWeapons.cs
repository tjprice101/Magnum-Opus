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

namespace MagnumOpus.Content.ClairDeLune.Weapons.Ranged
{
    #region Starfall Whisper - Temporal Sniper
    
    /// <summary>
    /// Starfall Whisper - SUPREME FINAL BOSS sniper rifle
    /// Fires crystalline bolts that pierce through time
    /// Charged shots create temporal rifts that rain starfall
    /// MUST EXCEED Ode to Joy ranged damage (3200)
    /// </summary>
    public class StarfallWhisper : ModItem
    {
        private int chargeLevel = 0;
        private const int MaxCharge = 40;

        public override void SetDefaults()
        {
            Item.width = 68;
            Item.height = 28;
            Item.damage = 3800; // FINAL BOSS - 19% above Ode to Joy (3200)
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 8f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item75;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<StarfallBoltProjectile>();
            Item.shootSpeed = 22f;
            Item.useAmmo = AmmoID.Bullet;
            Item.crit = 28; // High crit for sniper
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Converts bullets into time-piercing crystal bolts"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Hold fire to charge - charged shots create starfall rifts"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Critical hits trigger chain lightning between enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Starfall damages and slows all enemies in an area"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'When stars whisper, time listens'") 
            { 
                OverrideColor = ClairDeLuneColors.MoonlightSilver 
            });
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            type = ModContent.ProjectileType<StarfallBoltProjectile>();
        }

        public override void HoldItem(Player player)
        {
            // Charge while holding
            if (player.channel && player.itemAnimation == 0)
            {
                chargeLevel = Math.Min(chargeLevel + 1, MaxCharge);
                
                // Charging VFX
                float progress = (float)chargeLevel / MaxCharge;
                if (chargeLevel > 10)
                {
                    Vector2 muzzlePos = player.Center + (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX) * 50f;
                    ClairDeLuneVFX.TemporalChargeUp(muzzlePos, progress, 0.6f);
                    
                    // Crystal convergence
                    if (Main.rand.NextBool(4))
                    {
                        ClairDeLuneVFX.SpawnCrystalShard(muzzlePos + Main.rand.NextVector2Circular(60f * (1f - progress), 60f * (1f - progress)),
                            (muzzlePos - player.Center).SafeNormalize(Vector2.Zero) * 2f, false, 0.4f * progress);
                    }
                }
            }
            else if (chargeLevel > 0 && player.itemAnimation > 0)
            {
                chargeLevel = 0;
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float chargeBonus = chargeLevel >= MaxCharge ? 2.5f : 1f;
            bool isCharged = chargeLevel >= MaxCharge;
            
            Vector2 muzzlePos = position + velocity.SafeNormalize(Vector2.UnitX) * 55f;
            
            // Fire main crystal bolt
            int projDamage = (int)(damage * chargeBonus);
            int proj = Projectile.NewProjectile(source, position, velocity * (isCharged ? 1.5f : 1f), type, projDamage, knockback, player.whoAmI, isCharged ? 1f : 0f);
            
            // Muzzle flash VFX
            ClairDeLuneVFX.CrystalShatterBurst(muzzlePos, 8, 5f, 0.6f);
            ClairDeLuneVFX.SpawnLightningBurst(muzzlePos, velocity * 0.3f, false, 0.7f);
            
            if (isCharged)
            {
                // Charged shot VFX
                ClairDeLuneVFX.TemporalChargeRelease(muzzlePos, 1f);
                ClairDeLuneVFX.LightningStrikeExplosion(muzzlePos, 0.7f);
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f }, position);
                
                // Create starfall rift at mouse position
                Projectile.NewProjectile(source, Main.MouseWorld, Vector2.Zero,
                    ModContent.ProjectileType<StarfallRiftProjectile>(), damage, 0f, player.whoAmI);
            }
            
            chargeLevel = 0;
            return false;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-10f, 0f);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            float time = Main.GameUpdateCount * 0.05f;
            float pulse = 1f + (float)Math.Sin(time * 2f) * 0.1f;
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            spriteBatch.Draw(texture, position, null, ClairDeLuneColors.Crystal * 0.4f, rotation, origin, scale * pulse * 1.25f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, ClairDeLuneColors.MoonlightSilver * 0.3f, rotation, origin, scale * pulse * 1.12f, SpriteEffects.None, 0f);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Lighting.AddLight(Item.Center, ClairDeLuneColors.Crystal.ToVector3() * 0.5f);
            
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfClairDeLune>(), 24)
                .AddIngredient(ModContent.ItemType<ClairDeLuneResonantEnergy>(), 18)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfClairDeLune>(), 3)
                .AddIngredient(ItemID.LunarBar, 20)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    /// <summary>
    /// Starfall Bolt Projectile - Crystal bolt that pierces and chains lightning
    /// </summary>
    public class StarfallBoltProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/MediumCrystalShard";

        private bool IsCharged => Projectile.ai[0] == 1f;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = IsCharged ? 5 : 3;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 2;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            
            // Crystal trail
            ClairDeLuneVFX.TemporalTrail(Projectile.Center, Projectile.velocity, IsCharged ? 1.2f : 0.8f);
            
            // Extra crystal shards for charged
            if (IsCharged && Main.rand.NextBool(3))
            {
                ClairDeLuneVFX.SpawnCrystalShard(Projectile.Center, -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(3f, 3f), false, 0.5f);
            }
            
            // Lightning crackle
            if (Main.rand.NextBool(5))
            {
                ClairDeLuneVFX.SpawnLightningBurst(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f), 
                    Main.rand.NextVector2Circular(4f, 4f), false, 0.4f);
            }
            
            Lighting.AddLight(Projectile.Center, ClairDeLuneColors.Crystal.ToVector3() * 0.7f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Impact VFX
            ClairDeLuneVFX.CrystalShatterBurst(target.Center, 10, 7f, 0.7f);
            
            // Critical hit = chain lightning
            if (hit.Crit)
            {
                // Find nearby enemies and chain lightning
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.whoAmI != target.whoAmI)
                    {
                        float dist = Vector2.Distance(target.Center, npc.Center);
                        if (dist < 350f)
                        {
                            ClairDeLuneVFX.LightningArc(target.Center, npc.Center, 6, 12f, 0.7f);
                            
                            // Deal bonus damage to chained enemy
                            if (Main.myPlayer == Projectile.owner)
                            {
                                npc.SimpleStrikeNPC(damageDone / 3, hit.HitDirection, false, 0f);
                            }
                        }
                    }
                }
            }
            
            target.AddBuff(BuffID.Frostburn2, 180);
        }

        public override void OnKill(int timeLeft)
        {
            ClairDeLuneVFX.CrystalShatterBurst(Projectile.Center, 12, 8f, 0.8f);
            ClairDeLuneVFX.SpawnLightningBurst(Projectile.Center, Vector2.Zero, true, 0.6f);
            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.7f, Pitch = 0.5f }, Projectile.Center);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = tex.Size() / 2f;
            
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.15f;
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Crystal glow layers
            Color outerGlow = ClairDeLuneColors.MoonlightSilver * 0.4f;
            outerGlow.A = 0;
            Main.spriteBatch.Draw(tex, drawPos, null, outerGlow, Projectile.rotation, origin, 1.5f * pulse, SpriteEffects.None, 0f);
            
            Color midGlow = ClairDeLuneColors.Crystal * 0.6f;
            midGlow.A = 0;
            Main.spriteBatch.Draw(tex, drawPos, null, midGlow, Projectile.rotation, origin, 1.2f * pulse, SpriteEffects.None, 0f);
            
            Color innerGlow = ClairDeLuneColors.BrightWhite * 0.8f;
            innerGlow.A = 0;
            Main.spriteBatch.Draw(tex, drawPos, null, innerGlow, Projectile.rotation, origin, 0.8f * pulse, SpriteEffects.None, 0f);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
    }

    /// <summary>
    /// Starfall Rift - Rains crystals and starlight in an area
    /// </summary>
    public class StarfallRiftProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/LightningBurst";

        private int spawnTimer = 0;

        public override void SetDefaults()
        {
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 180; // 3 seconds
            Projectile.penetrate = -1;
        }

        public override void AI()
        {
            spawnTimer++;
            
            // Spawn starfall crystals periodically
            if (spawnTimer % 8 == 0 && Main.myPlayer == Projectile.owner)
            {
                Vector2 spawnPos = Projectile.Center + new Vector2(Main.rand.NextFloat(-150f, 150f), -600f);
                Vector2 velocity = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(12f, 18f));
                
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPos, velocity,
                    ModContent.ProjectileType<StarfallCrystalProjectile>(), Projectile.damage / 2, 0f, Projectile.owner);
            }
            
            // Rift visual
            if (Main.GameUpdateCount % 3 == 0)
            {
                ClairDeLuneVFX.SpawnLightningBurst(Projectile.Center + Main.rand.NextVector2Circular(100f, 50f), 
                    Vector2.UnitY * -5f, false, 0.5f);
            }
            
            // Crystal particles falling
            if (Main.rand.NextBool(3))
            {
                Vector2 particlePos = Projectile.Center + Main.rand.NextVector2Circular(120f, 30f);
                ClairDeLuneVFX.SpawnCrystalShard(particlePos, Vector2.UnitY * Main.rand.NextFloat(3f, 6f), false, 0.4f);
            }
            
            Lighting.AddLight(Projectile.Center, ClairDeLuneColors.MoonlightSilver.ToVector3() * 0.5f);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }

    /// <summary>
    /// Individual starfall crystal
    /// </summary>
    public class StarfallCrystalProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SmallCrystalShard";

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.velocity.Y += 0.2f; // Gravity
            
            ClairDeLuneVFX.TemporalTrail(Projectile.Center, Projectile.velocity, 0.5f);
            Lighting.AddLight(Projectile.Center, ClairDeLuneColors.Crystal.ToVector3() * 0.4f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Slow, 120);
            ClairDeLuneVFX.CrystalShatterBurst(target.Center, 6, 4f, 0.5f);
        }

        public override void OnKill(int timeLeft)
        {
            ClairDeLuneVFX.CrystalShatterBurst(Projectile.Center, 8, 5f, 0.5f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Color glow = ClairDeLuneColors.Crystal * 0.6f;
            glow.A = 0;
            Main.spriteBatch.Draw(tex, drawPos, null, glow, Projectile.rotation, tex.Size() / 2f, 1.3f, SpriteEffects.None, 0f);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return true;
        }
    }

    #endregion

    #region Midnight Mechanism - Clockwork Gatling
    
    /// <summary>
    /// Midnight Mechanism - SUPREME FINAL BOSS rapid-fire gatling gun
    /// Clockwork mechanism that fires temporal bolts in rapid succession
    /// Spin-up mechanic - fires faster the longer you shoot
    /// Every 50 shots triggers a synchronized gear barrage
    /// </summary>
    public class MidnightMechanism : ModItem
    {
        private int shotCounter = 0;
        private int spinUpLevel = 0;
        private const int MaxSpinUp = 20;

        public override void SetDefaults()
        {
            Item.width = 72;
            Item.height = 36;
            Item.damage = 2400; // Lower base, but VERY fast fire rate
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 6; // Very fast base
            Item.useAnimation = 6;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 2f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item11;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<MechanismBoltProjectile>();
            Item.shootSpeed = 20f;
            Item.useAmmo = AmmoID.Bullet;
            Item.crit = 16;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Clockwork gatling with spin-up mechanic"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Fire rate increases the longer you shoot"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Every 50 shots fires a synchronized gear barrage"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Gear barrage marks all enemies hit"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The midnight hour strikes eternally'") 
            { 
                OverrideColor = ClairDeLuneColors.DarkGray 
            });
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            type = ModContent.ProjectileType<MechanismBoltProjectile>();
        }

        public override float UseSpeedMultiplier(Player player)
        {
            // Spin-up increases fire rate
            return 1f + (spinUpLevel * 0.1f);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            shotCounter++;
            spinUpLevel = Math.Min(spinUpLevel + 1, MaxSpinUp);
            
            // Spread increases slightly with spin-up for balance
            float spread = MathHelper.ToRadians(3f + spinUpLevel * 0.2f);
            Vector2 spreadVel = velocity.RotatedByRandom(spread);
            
            Vector2 muzzlePos = position + velocity.SafeNormalize(Vector2.UnitX) * 60f;
            
            // Fire main bolt
            Projectile.NewProjectile(source, position, spreadVel, type, damage, knockback, player.whoAmI);
            
            // Muzzle flash VFX
            ClairDeLuneVFX.SpawnClockworkGear(muzzlePos, velocity * 0.1f, false, 0.4f);
            
            if (Main.rand.NextBool(3))
            {
                ClairDeLuneVFX.SpawnLightningBurst(muzzlePos, velocity * 0.2f, false, 0.35f);
            }
            
            // Gear ejection effect
            Vector2 ejectDir = velocity.RotatedBy(MathHelper.PiOver2 * player.direction).SafeNormalize(Vector2.UnitX);
            if (Main.rand.NextBool(2))
            {
                var gearEject = new GenericGlowParticle(
                    muzzlePos + ejectDir * 10f,
                    ejectDir * Main.rand.NextFloat(2f, 5f) + Vector2.UnitY * -2f,
                    ClairDeLuneColors.Brass * 0.7f,
                    0.3f,
                    20,
                    true
                );
                MagnumParticleHandler.SpawnParticle(gearEject);
            }
            
            // Every 50 shots - gear barrage
            if (shotCounter >= 50)
            {
                shotCounter = 0;
                
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.5f }, position);
                
                // Synchronized gear barrage - 8 gears in spread
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.ToRadians(-28f + i * 8f);
                    Vector2 gearVel = velocity.RotatedBy(angle) * 0.8f;
                    
                    Projectile.NewProjectile(source, position, gearVel,
                        ModContent.ProjectileType<SynchronizedGearBoltProjectile>(), damage * 2, knockback * 2, player.whoAmI);
                }
                
                ClairDeLuneVFX.ClockworkGearCascade(muzzlePos, 12, 8f, 0.9f);
            }
            
            return false;
        }

        public override void HoldItem(Player player)
        {
            // Decay spin-up when not firing
            if (player.itemAnimation == 0 && spinUpLevel > 0)
            {
                spinUpLevel = Math.Max(0, spinUpLevel - 1);
            }
            
            // Visual for spin-up level
            if (spinUpLevel > 10)
            {
                Vector2 gunPos = player.Center + (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX) * 40f;
                ClairDeLuneVFX.OrbitingGears(gunPos, 25f, 3, Main.GameUpdateCount * 0.1f * (1f + spinUpLevel * 0.05f), 0.3f);
            }
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-12f, 2f);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfClairDeLune>(), 26)
                .AddIngredient(ModContent.ItemType<ClairDeLuneResonantEnergy>(), 20)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfClairDeLune>(), 3)
                .AddIngredient(ItemID.LunarBar, 22)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    /// <summary>
    /// Mechanism Bolt - Rapid fire clockwork bolt
    /// </summary>
    public class MechanismBoltProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/ClockworkGearSmall";

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation += 0.2f;
            
            // Light trail
            if (Main.rand.NextBool(2))
            {
                Color trailColor = ClairDeLuneColors.GetGearGradient(Main.rand.NextFloat());
                var trail = new GenericGlowParticle(
                    Projectile.Center,
                    -Projectile.velocity * 0.05f,
                    trailColor * 0.5f,
                    0.2f,
                    10,
                    true
                );
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            Lighting.AddLight(Projectile.Center, ClairDeLuneColors.Brass.ToVector3() * 0.3f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            ClairDeLuneVFX.SpawnClockworkGear(target.Center, Main.rand.NextVector2Circular(3f, 3f), false, 0.4f);
        }

        public override void OnKill(int timeLeft)
        {
            ClairDeLuneVFX.SpawnClockworkGear(Projectile.Center, Vector2.Zero, false, 0.5f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Color glow = ClairDeLuneColors.Brass * 0.5f;
            glow.A = 0;
            Main.spriteBatch.Draw(tex, drawPos, null, glow, Projectile.rotation, tex.Size() / 2f, 1.4f, SpriteEffects.None, 0f);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return true;
        }
    }

    /// <summary>
    /// Synchronized Gear Bolt - Powerful gear projectile from barrage
    /// </summary>
    public class SynchronizedGearBoltProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/ClockworkGearLarge";

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 150;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            Projectile.rotation += 0.15f;
            
            ClairDeLuneVFX.HeavyTemporalTrail(Projectile.Center, Projectile.velocity, 0.7f);
            
            if (Main.rand.NextBool(4))
            {
                ClairDeLuneVFX.SpawnLightningBurst(Projectile.Center, Main.rand.NextVector2Circular(3f, 3f), false, 0.4f);
            }
            
            Lighting.AddLight(Projectile.Center, ClairDeLuneColors.GearGold.ToVector3() * 0.5f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.BetsysCurse, 240); // Mark for bonus damage
            target.AddBuff(BuffID.Slow, 180);
            
            ClairDeLuneVFX.TemporalImpact(target.Center, 0.6f);
        }

        public override void OnKill(int timeLeft)
        {
            ClairDeLuneVFX.ClockworkGearCascade(Projectile.Center, 8, 6f, 0.7f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = tex.Size() / 2f;
            
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.1f;
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Color outerGlow = ClairDeLuneColors.GearGold * 0.5f;
            outerGlow.A = 0;
            Main.spriteBatch.Draw(tex, drawPos, null, outerGlow, Projectile.rotation * 0.8f, origin, 1.5f * pulse, SpriteEffects.None, 0f);
            
            Color midGlow = ClairDeLuneColors.Brass * 0.6f;
            midGlow.A = 0;
            Main.spriteBatch.Draw(tex, drawPos, null, midGlow, Projectile.rotation, origin, 1.2f * pulse, SpriteEffects.None, 0f);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return true;
        }
    }

    #endregion

    #region Cog & Hammer - Clockwork Launcher
    
    /// <summary>
    /// Cog & Hammer - SUPREME FINAL BOSS explosive launcher
    /// Fires massive clockwork bombs that explode into gear shrapnel
    /// Charged shots create temporal singularity explosions
    /// </summary>
    public class CogAndHammer : ModItem
    {
        private int chargeLevel = 0;
        private const int MaxCharge = 50;

        public override void SetDefaults()
        {
            Item.width = 64;
            Item.height = 32;
            Item.damage = 5500; // High burst damage launcher
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 12f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item61;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<ClockworkBombProjectile>();
            Item.shootSpeed = 14f;
            Item.useAmmo = AmmoID.Rocket;
            Item.crit = 18;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires massive clockwork bombs"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Bombs explode into razor gear shrapnel"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Hold to charge - charged shots create singularity explosions"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Singularities pull in enemies and shred them"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Forged where time and steel become one'") 
            { 
                OverrideColor = ClairDeLuneColors.Brass 
            });
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            type = ModContent.ProjectileType<ClockworkBombProjectile>();
        }

        public override void HoldItem(Player player)
        {
            if (player.channel && player.itemAnimation == 0)
            {
                chargeLevel = Math.Min(chargeLevel + 1, MaxCharge);
                
                float progress = (float)chargeLevel / MaxCharge;
                if (chargeLevel > 15)
                {
                    Vector2 muzzlePos = player.Center + (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX) * 45f;
                    ClairDeLuneVFX.TemporalChargeUp(muzzlePos, progress, 0.7f);
                    
                    // Gears converging
                    if (Main.rand.NextBool(3))
                    {
                        ClairDeLuneVFX.SpawnClockworkGear(
                            muzzlePos + Main.rand.NextVector2Circular(80f * (1f - progress), 80f * (1f - progress)),
                            (muzzlePos - player.Center).SafeNormalize(Vector2.Zero) * 3f, 
                            Main.rand.NextBool(), 0.5f * progress);
                    }
                }
            }
            else if (chargeLevel > 0 && player.itemAnimation > 0)
            {
                chargeLevel = 0;
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            bool isCharged = chargeLevel >= MaxCharge;
            float damageBonus = isCharged ? 3f : 1f;
            
            Vector2 muzzlePos = position + velocity.SafeNormalize(Vector2.UnitX) * 50f;
            
            // Fire clockwork bomb
            int projDamage = (int)(damage * damageBonus);
            Projectile.NewProjectile(source, position, velocity, type, projDamage, knockback, player.whoAmI, isCharged ? 1f : 0f);
            
            // Muzzle flash
            ClairDeLuneVFX.ClockworkGearCascade(muzzlePos, 10, 6f, 0.8f);
            ClairDeLuneVFX.SpawnLightningBurst(muzzlePos, velocity * 0.3f, true, 0.7f);
            
            if (isCharged)
            {
                ClairDeLuneVFX.TemporalChargeRelease(muzzlePos, 1.2f);
                ClairDeLuneVFX.LightningStrikeExplosion(muzzlePos, 0.8f);
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.3f }, position);
            }
            
            chargeLevel = 0;
            return false;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-8f, 4f);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfClairDeLune>(), 28)
                .AddIngredient(ModContent.ItemType<ClairDeLuneResonantEnergy>(), 22)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfClairDeLune>(), 4)
                .AddIngredient(ItemID.LunarBar, 24)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    /// <summary>
    /// Clockwork Bomb - Explosive projectile that creates gear shrapnel
    /// </summary>
    public class ClockworkBombProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/ClockworkGearLarge";

        private bool IsCharged => Projectile.ai[0] == 1f;
        private float rotation = 0f;

        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            rotation += 0.08f;
            Projectile.rotation = rotation;
            
            // Gravity
            Projectile.velocity.Y += 0.15f;
            
            // Trail
            ClairDeLuneVFX.HeavyTemporalTrail(Projectile.Center, Projectile.velocity, IsCharged ? 1f : 0.7f);
            
            // Orbiting gears
            if (Main.GameUpdateCount % 3 == 0)
            {
                ClairDeLuneVFX.OrbitingGears(Projectile.Center, 20f, 4, Main.GameUpdateCount * 0.06f, 0.4f);
            }
            
            Lighting.AddLight(Projectile.Center, (IsCharged ? ClairDeLuneColors.Crimson : ClairDeLuneColors.Brass).ToVector3() * 0.6f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Explode();
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Explode();
            return true;
        }

        private void Explode()
        {
            if (IsCharged)
            {
                // Singularity explosion
                ClairDeLuneVFX.TemporalDeathExplosion(Projectile.Center, 1.8f);
                
                // Spawn singularity
                if (Main.myPlayer == Projectile.owner)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                        ModContent.ProjectileType<TemporalSingularityProjectile>(), Projectile.damage / 2, 0f, Projectile.owner);
                }
                
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.5f }, Projectile.Center);
            }
            else
            {
                // Normal explosion with gear shrapnel
                ClairDeLuneVFX.TemporalImpact(Projectile.Center, 1.2f);
                ClairDeLuneVFX.ClockworkGearCascade(Projectile.Center, 16, 10f, 1f);
                
                // Spawn gear shrapnel
                if (Main.myPlayer == Projectile.owner)
                {
                    int shrapnelCount = 8;
                    for (int i = 0; i < shrapnelCount; i++)
                    {
                        float angle = MathHelper.TwoPi * i / shrapnelCount + Main.rand.NextFloat(-0.2f, 0.2f);
                        Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(8f, 14f);
                        
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vel,
                            ModContent.ProjectileType<GearShrapnelProjectile>(), Projectile.damage / 3, 2f, Projectile.owner);
                    }
                }
                
                SoundEngine.PlaySound(SoundID.Item62 with { Pitch = 0.2f }, Projectile.Center);
            }
            
            Projectile.Kill();
        }

        public override void OnKill(int timeLeft)
        {
            // Already handled in Explode()
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = tex.Size() / 2f;
            
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.15f;
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Color outerColor = IsCharged ? ClairDeLuneColors.Crimson : ClairDeLuneColors.Brass;
            Color outerGlow = outerColor * 0.5f;
            outerGlow.A = 0;
            Main.spriteBatch.Draw(tex, drawPos, null, outerGlow, rotation * 0.7f, origin, 1.8f * pulse, SpriteEffects.None, 0f);
            
            Color midGlow = ClairDeLuneColors.GearGold * 0.6f;
            midGlow.A = 0;
            Main.spriteBatch.Draw(tex, drawPos, null, midGlow, rotation, origin, 1.4f * pulse, SpriteEffects.None, 0f);
            
            Color innerGlow = ClairDeLuneColors.Crystal * 0.7f;
            innerGlow.A = 0;
            Main.spriteBatch.Draw(tex, drawPos, null, innerGlow, rotation * 1.3f, origin, 1f * pulse, SpriteEffects.None, 0f);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
    }

    /// <summary>
    /// Gear Shrapnel - Fragment from bomb explosion
    /// </summary>
    public class GearShrapnelProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/ClockworkGearSmall";

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            Projectile.rotation += 0.2f;
            Projectile.velocity.Y += 0.1f;
            
            if (Main.rand.NextBool(2))
            {
                var trail = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.05f, 
                    ClairDeLuneColors.Brass * 0.5f, 0.2f, 10, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            ClairDeLuneVFX.SpawnClockworkGear(target.Center, Main.rand.NextVector2Circular(3f, 3f), false, 0.4f);
        }

        public override void OnKill(int timeLeft)
        {
            ClairDeLuneVFX.SpawnClockworkGear(Projectile.Center, Vector2.Zero, false, 0.4f);
        }
    }

    /// <summary>
    /// Temporal Singularity - Pulls in and damages enemies
    /// </summary>
    public class TemporalSingularityProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/LightningBurst";

        private int lifeTimer = 0;

        public override void SetDefaults()
        {
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 120;
            Projectile.penetrate = -1;
        }

        public override void AI()
        {
            lifeTimer++;
            float pullRadius = 250f;
            float damageRadius = 80f;
            
            // Pull enemies in
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && !npc.dontTakeDamage)
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < pullRadius && dist > 10f)
                    {
                        Vector2 pullDir = (Projectile.Center - npc.Center).SafeNormalize(Vector2.Zero);
                        float pullStrength = (1f - dist / pullRadius) * 6f;
                        npc.velocity += pullDir * pullStrength;
                    }
                    
                    // Damage nearby enemies
                    if (dist < damageRadius && lifeTimer % 10 == 0 && Main.myPlayer == Projectile.owner)
                    {
                        npc.SimpleStrikeNPC(Projectile.damage, 0, false, 0f);
                    }
                }
            }
            
            // Visual
            if (Main.GameUpdateCount % 2 == 0)
            {
                ClairDeLuneVFX.OrbitingGears(Projectile.Center, 60f, 6, Main.GameUpdateCount * 0.08f, 0.6f);
            }
            
            if (Main.rand.NextBool(2))
            {
                float inwardAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 particleStart = Projectile.Center + inwardAngle.ToRotationVector2() * pullRadius;
                Vector2 inwardVel = (Projectile.Center - particleStart).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(8f, 15f);
                
                var inward = new GenericGlowParticle(particleStart, inwardVel, 
                    ClairDeLuneColors.GetGradient(Main.rand.NextFloat()), 0.4f, 15, true);
                MagnumParticleHandler.SpawnParticle(inward);
            }
            
            // Lightning crackles
            if (Main.rand.NextBool(3))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 lightningEnd = Projectile.Center + angle.ToRotationVector2() * Main.rand.NextFloat(40f, 80f);
                ClairDeLuneVFX.LightningArc(Projectile.Center, lightningEnd, 4, 8f, 0.5f);
            }
            
            Lighting.AddLight(Projectile.Center, ClairDeLuneColors.Crimson.ToVector3() * 1.2f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Draw as a glowing vortex
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.2f;
            
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2").Value;
            Vector2 origin = tex.Size() / 2f;
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Outer dark glow
            Color outerGlow = ClairDeLuneColors.VoidBlack * 0.8f;
            outerGlow.A = 0;
            Main.spriteBatch.Draw(tex, drawPos, null, outerGlow, Main.GameUpdateCount * 0.02f, origin, 2f * pulse, SpriteEffects.None, 0f);
            
            // Crimson ring
            Color crimsonGlow = ClairDeLuneColors.Crimson * 0.6f;
            crimsonGlow.A = 0;
            Main.spriteBatch.Draw(tex, drawPos, null, crimsonGlow, -Main.GameUpdateCount * 0.03f, origin, 1.5f * pulse, SpriteEffects.None, 0f);
            
            // White core
            Color coreGlow = ClairDeLuneColors.BrightWhite * 0.7f;
            coreGlow.A = 0;
            Main.spriteBatch.Draw(tex, drawPos, null, coreGlow, 0f, origin, 0.6f * pulse, SpriteEffects.None, 0f);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
    }

    #endregion
}
