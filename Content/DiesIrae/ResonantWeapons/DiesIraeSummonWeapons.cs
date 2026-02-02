using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.DiesIrae.Projectiles;
using MagnumOpus.Content.DiesIrae.ResonanceEnergies;
using MagnumOpus.Content.DiesIrae.HarmonicCores;
using MagnumOpus.Content.Fate.CraftingStations;

namespace MagnumOpus.Content.DiesIrae.ResonantWeapons
{
    #region Summon Weapons
    
    /// <summary>
    /// Death Tolling Bell - Summons a tolling bell minion that creates shockwaves.
    /// The bell hovers and periodically releases devastating toll attacks.
    /// POST-NACHTMUSIK ULTIMATE TIER
    /// </summary>
    public class DeathTollingBell : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 44;
            Item.damage = 1450; // POST-NACHTMUSIK (higher than Nachtmusik's 1100)
            Item.DamageType = DamageClass.Summon;
            Item.mana = 22;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(gold: 55);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<DeathTollingBellMinion>();
            Item.buffType = ModContent.BuffType<DeathTollingBellBuff>();
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            
            Vector2 spawnPos = Main.MouseWorld;
            
            // Entrance VFX - Infernal explosion
            DiesIraeVFX.FireImpact(spawnPos, 1.2f);
            
            // Music note burst
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                DiesIraeVFX.SpawnMusicNote(spawnPos, noteVel, DiesIraeColors.GetGradient(Main.rand.NextFloat()), 0.8f);
            }
            
            // Heavy smoke burst
            for (int i = 0; i < 8; i++)
            {
                var smoke = new HeavySmokeParticle(
                    spawnPos + Main.rand.NextVector2Circular(20f, 20f),
                    Main.rand.NextVector2Circular(3f, 3f),
                    DiesIraeColors.CharredBlack * 0.7f,
                    Main.rand.Next(40, 60), 0.5f, 0.7f, 0.02f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.5f, Volume = 0.8f }, spawnPos);
            
            Projectile.NewProjectile(source, spawnPos, Vector2.Zero, type, damage, knockback, player.whoAmI);
            
            return false;
        }
        
        public override void HoldItem(Player player)
        {
            // Sparse ambient ember
            if (Main.rand.NextBool(25))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                DiesIraeVFX.SpawnMusicNote(notePos, new Vector2(0, -0.4f), DiesIraeColors.EmberOrange, 0.7f);
            }
            
            Lighting.AddLight(player.Center, DiesIraeColors.EmberOrange.ToVector3() * 0.2f);
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Summon", "Summons a Death Tolling Bell to fight for you"));
            tooltips.Add(new TooltipLine(Mod, "Behavior", "The bell hovers and periodically releases devastating toll shockwaves"));
            tooltips.Add(new TooltipLine(Mod, "Debuff", "Bell attacks spread infernal fire to nearby enemies"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'When the bell tolls, even the damned flee'")
            {
                OverrideColor = DiesIraeColors.HellfireGold
            });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<DiesIraeResonantEnergy>(), 30)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfDiesIrae>(), 12)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfDiesIrae>(), 2)
                .AddIngredient(ItemID.LunarBar, 20)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }
    
    /// <summary>
    /// Harmony of Judgement - Summons an angelic judge minion.
    /// The judge hovers near the player and fires judgment rays at enemies.
    /// POST-NACHTMUSIK ULTIMATE TIER
    /// </summary>
    public class HarmonyOfJudgement : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.damage = 1300; // POST-NACHTMUSIK (higher than Nachtmusik's 980)
            Item.DamageType = DamageClass.Summon;
            Item.mana = 20;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3.5f;
            Item.value = Item.sellPrice(gold: 52);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<HarmonyOfJudgementMinion>();
            Item.buffType = ModContent.BuffType<HarmonyOfJudgementBuff>();
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            
            Vector2 spawnPos = Main.MouseWorld;
            
            // Angelic entrance VFX
            DiesIraeVFX.FireImpact(spawnPos, 0.8f);
            
            // Music note burst
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 3.5f);
                DiesIraeVFX.SpawnMusicNote(spawnPos, noteVel, DiesIraeColors.GetGradient(Main.rand.NextFloat()), 0.75f);
            }
            
            // Golden fire burst
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 firePos = spawnPos + angle.ToRotationVector2() * 25f;
                CustomParticles.GenericFlare(firePos, DiesIraeColors.HellfireGold, 0.4f, 18);
            }
            
            SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.2f }, spawnPos);
            
            Projectile.NewProjectile(source, spawnPos, Vector2.Zero, type, damage, knockback, player.whoAmI);
            
            return false;
        }
        
        public override void HoldItem(Player player)
        {
            // Sparse ambient golden flame
            if (Main.rand.NextBool(25))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(25f, 25f);
                DiesIraeVFX.SpawnMusicNote(notePos, new Vector2(0, -0.6f), DiesIraeColors.HellfireGold, 0.65f);
            }
            
            Lighting.AddLight(player.Center, DiesIraeColors.HellfireGold.ToVector3() * 0.2f);
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Summon", "Summons a Harmony of Judgement to fight for you"));
            tooltips.Add(new TooltipLine(Mod, "Behavior", "The angelic judge hovers nearby and fires judgment rays"));
            tooltips.Add(new TooltipLine(Mod, "Debuff", "Attacks mark enemies for judgment, increasing damage taken"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The righteous fury of final judgment'")
            {
                OverrideColor = DiesIraeColors.HellfireGold
            });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<DiesIraeResonantEnergy>(), 28)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfDiesIrae>(), 10)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfDiesIrae>(), 2)
                .AddIngredient(ItemID.LunarBar, 18)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }
    
    /// <summary>
    /// Wrathful Contract - The ultimate summon weapon from Dies Irae.
    /// Summons a demonic entity bound by a contract of wrath.
    /// Uses 2 minion slots but deals massive damage.
    /// POST-NACHTMUSIK ULTIMATE TIER
    /// </summary>
    public class WrathfulContract : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 48;
            Item.damage = 1650; // POST-NACHTMUSIK ULTIMATE (higher than Nachtmusik's 1250)
            Item.DamageType = DamageClass.Summon;
            Item.mana = 40;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(gold: 65);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
            Item.UseSound = SoundID.Item82;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<WrathfulContractMinion>();
            Item.buffType = ModContent.BuffType<WrathfulContractBuff>();
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            
            Vector2 spawnPos = Main.MouseWorld;
            
            // Grand demonic entrance VFX
            DiesIraeVFX.FireImpact(spawnPos, 1.5f);
            
            // Music note burst
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                DiesIraeVFX.SpawnMusicNote(spawnPos, noteVel, DiesIraeColors.GetGradient(Main.rand.NextFloat()), 0.8f);
            }
            
            // Contract circle
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 circlePos = spawnPos + angle.ToRotationVector2() * 50f;
                Color circleColor = i % 2 == 0 ? DiesIraeColors.BloodRed : DiesIraeColors.EmberOrange;
                CustomParticles.GenericFlare(circlePos, circleColor, 0.5f, 20);
            }
            
            // Heavy smoke
            for (int i = 0; i < 12; i++)
            {
                var smoke = new HeavySmokeParticle(
                    spawnPos + Main.rand.NextVector2Circular(30f, 30f),
                    Main.rand.NextVector2Circular(4f, 4f),
                    DiesIraeColors.CharredBlack * 0.8f,
                    Main.rand.Next(50, 70), 0.6f, 0.8f, 0.018f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            MagnumScreenEffects.AddScreenShake(6f);
            SoundEngine.PlaySound(SoundID.Roar with { Pitch = -0.3f, Volume = 0.7f }, spawnPos);
            
            Projectile.NewProjectile(source, spawnPos, Vector2.Zero, type, damage, knockback, player.whoAmI);
            
            return false;
        }
        
        public override void HoldItem(Player player)
        {
            // Ominous ambient effect
            if (Main.rand.NextBool(20))
            {
                Vector2 flamePos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                Color flameColor = Main.rand.NextBool() ? DiesIraeColors.BloodRed : DiesIraeColors.EmberOrange;
                var flame = new GenericGlowParticle(flamePos, new Vector2(0, -1f), flameColor * 0.4f, 0.3f, 25, true);
                MagnumParticleHandler.SpawnParticle(flame);
            }
            
            if (Main.rand.NextBool(30))
            {
                DiesIraeVFX.SpawnMusicNote(player.Center + Main.rand.NextVector2Circular(30f, 30f), 
                    new Vector2(0, -0.5f), DiesIraeColors.Crimson, 0.7f);
            }
            
            Lighting.AddLight(player.Center, DiesIraeColors.BloodRed.ToVector3() * 0.25f);
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Summon", "Summons a Wrathful Entity bound by contract"));
            tooltips.Add(new TooltipLine(Mod, "Slots", "Uses 2 minion slots"));
            tooltips.Add(new TooltipLine(Mod, "Behavior", "The entity aggressively pursues enemies with devastating attacks"));
            tooltips.Add(new TooltipLine(Mod, "Special", "Attacks cause infernal explosions and spread damnation fire"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Sign in blood. Suffer the consequences.'")
            {
                OverrideColor = DiesIraeColors.BloodRed
            });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<DiesIraeResonantEnergy>(), 35)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfDiesIrae>(), 15)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfDiesIrae>(), 3)
                .AddIngredient(ItemID.LunarBar, 25)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }
    
    #endregion
    
    #region Minion Projectiles
    
    /// <summary>
    /// Death Tolling Bell Minion - A hovering bell that releases devastating toll shockwaves.
    /// </summary>
    public class DeathTollingBellMinion : ModProjectile
    {
        private float hoverOffset;
        private int tollCooldown;
        private int tollChargeTime;
        private bool isCharging;
        
        public override void SetStaticDefaults()
        {
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
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }
        
        public override bool? CanCutTiles() => false;
        
        public override bool MinionContactDamage() => true;
        
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            
            if (!CheckActive(owner))
                return;
            
            hoverOffset += 0.03f;
            tollCooldown = Math.Max(0, tollCooldown - 1);
            
            // Hover near player with bob
            float bobY = (float)Math.Sin(hoverOffset) * 15f;
            Vector2 idealPos = owner.Center + new Vector2(owner.direction * 50f, -80f + bobY);
            Vector2 toIdeal = idealPos - Projectile.Center;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdeal * 0.08f, 0.06f);
            
            // Slight rotation bob
            Projectile.rotation = (float)Math.Sin(hoverOffset * 0.7f) * 0.1f;
            
            NPC target = FindTarget(owner, 900f);
            
            // Toll attack logic
            if (target != null && tollCooldown == 0 && !isCharging)
            {
                isCharging = true;
                tollChargeTime = 0;
            }
            
            if (isCharging)
            {
                tollChargeTime++;
                
                // Charging VFX - glowing buildup
                float chargeProgress = tollChargeTime / 45f;
                if (tollChargeTime % 5 == 0)
                {
                    Color chargeColor = Color.Lerp(DiesIraeColors.EmberOrange, DiesIraeColors.HellfireGold, chargeProgress);
                    CustomParticles.GenericFlare(Projectile.Center, chargeColor, 0.3f + chargeProgress * 0.3f, 10);
                }
                
                // Release toll
                if (tollChargeTime >= 45)
                {
                    PerformTollAttack();
                    isCharging = false;
                    tollCooldown = 90; // 1.5 second cooldown
                }
            }
            
            // Ambient fire effect
            if (Main.rand.NextBool(5))
            {
                var ember = new GenericGlowParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(20f, 20f),
                    new Vector2(0, Main.rand.NextFloat(-1f, -2f)),
                    DiesIraeColors.EmberOrange * 0.5f, 0.25f, 20, true);
                MagnumParticleHandler.SpawnParticle(ember);
            }
            
            // Music note presence
            if (Main.rand.NextBool(15))
            {
                DiesIraeVFX.SpawnMusicNote(Projectile.Center + Main.rand.NextVector2Circular(15f, 15f),
                    new Vector2(0, -0.5f), DiesIraeColors.HellfireGold, 0.7f);
            }
            
            Lighting.AddLight(Projectile.Center, DiesIraeColors.EmberOrange.ToVector3() * 0.5f);
        }
        
        private void PerformTollAttack()
        {
            // Sound
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.3f, Volume = 0.9f }, Projectile.Center);
            
            // Shockwave VFX
            DiesIraeVFX.FireImpact(Projectile.Center, 1.4f);
            
            // Expanding damage rings
            for (int ring = 0; ring < 3; ring++)
            {
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi * i / 12f;
                    float speed = 8f + ring * 4f;
                    Vector2 ringVel = angle.ToRotationVector2() * speed;
                    
                    // Fire wave projectile
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, ringVel,
                        ModContent.ProjectileType<BellTollWave>(), Projectile.damage / 2, 2f, Projectile.owner);
                }
            }
            
            // Heavy smoke
            for (int i = 0; i < 10; i++)
            {
                var smoke = new HeavySmokeParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(25f, 25f),
                    Main.rand.NextVector2Circular(5f, 5f),
                    DiesIraeColors.CharredBlack * 0.6f,
                    Main.rand.Next(35, 50), 0.4f, 0.6f, 0.02f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Music notes burst
            for (int n = 0; n < 6; n++)
            {
                float noteAngle = MathHelper.TwoPi * n / 6f + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 noteVel = noteAngle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                DiesIraeVFX.SpawnMusicNote(Projectile.Center, noteVel, DiesIraeColors.GetGradient(Main.rand.NextFloat()), 0.75f);
            }
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            DiesIraeVFX.FireImpact(target.Center, 0.6f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            // Glow behind
            Texture2D glow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow3").Value;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.1f + 0.9f;
            
            // Charging glow intensifies
            float chargeGlow = isCharging ? tollChargeTime / 45f : 0f;
            
            sb.Draw(glow, drawPos, null, DiesIraeColors.EmberOrange * (0.3f + chargeGlow * 0.4f), 0f, glow.Size() / 2f, (0.6f + chargeGlow * 0.3f) * pulse, SpriteEffects.None, 0f);
            sb.Draw(glow, drawPos, null, DiesIraeColors.HellfireGold * (0.2f + chargeGlow * 0.3f), 0f, glow.Size() / 2f, 0.4f * pulse, SpriteEffects.None, 0f);
            
            // Main sprite
            sb.Draw(tex, drawPos, null, Color.White, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            
            return false;
        }
        
        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<DeathTollingBellBuff>());
                return false;
            }
            
            if (owner.HasBuff(ModContent.BuffType<DeathTollingBellBuff>()))
                Projectile.timeLeft = 2;
            
            return true;
        }
        
        private NPC FindTarget(Player owner, float range)
        {
            if (owner.HasMinionAttackTargetNPC)
            {
                NPC manual = Main.npc[owner.MinionAttackTargetNPC];
                if (manual.active && manual.CanBeChasedBy(Projectile) && Vector2.Distance(owner.Center, manual.Center) < range * 1.5f)
                    return manual;
            }
            
            NPC closest = null;
            float closestDist = range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy(Projectile))
                {
                    float dist = Vector2.Distance(owner.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }
            return closest;
        }
    }
    
    /// <summary>
    /// Bell Toll Wave - Expanding shockwave from the bell's toll attack.
    /// </summary>
    public class BellTollWave : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/FlameImpactExplosion";
        
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 40;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }
        
        public override void AI()
        {
            // === LAYER 1: Core glow trail ===
            if (Main.rand.NextBool(2))
            {
                var trail = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f,
                    DiesIraeColors.EmberOrange * 0.5f, 0.25f, 15, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // === LAYER 2: Afterimage echo ===
            if (Projectile.velocity.Length() > 3f)
            {
                DiesIraeVFX.AfterimageTrail(Projectile.Center, Projectile.velocity, 0.35f, DiesIraeColors.HellfireGold, 3);
            }
            
            // === LAYER 3: Spiral shockwave pattern ===
            if (Main.rand.NextBool(2))
            {
                float spiralAngle = Main.GameUpdateCount * 0.2f;
                DiesIraeVFX.SpiralTrail(Projectile.Center, Projectile.velocity, DiesIraeColors.BloodRed, 0.3f, spiralAngle);
            }
            
            // === LAYER 4: Music note trace ===
            if (Main.rand.NextBool(8))
            {
                DiesIraeVFX.SpawnMusicNote(Projectile.Center, -Projectile.velocity * 0.1f, DiesIraeColors.HellfireGold, 0.7f);
            }
            
            Projectile.rotation = Projectile.velocity.ToRotation();
            Lighting.AddLight(Projectile.Center, DiesIraeColors.EmberOrange.ToVector3() * 0.4f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            DiesIraeVFX.FireImpact(target.Center, 0.5f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = tex.Size() / 2f;
            
            float progress = 1f - Projectile.timeLeft / 40f;
            Color waveColor = Color.Lerp(DiesIraeColors.HellfireGold, DiesIraeColors.EmberOrange, progress) * (1f - progress * 0.5f);
            waveColor.A = 0;
            
            sb.Draw(tex, drawPos, null, waveColor * 0.8f, Projectile.rotation, origin, 0.4f, SpriteEffects.None, 0f);
            sb.Draw(tex, drawPos, null, waveColor * 0.5f, Projectile.rotation, origin, 0.6f, SpriteEffects.None, 0f);
            
            return false;
        }
    }
    
    /// <summary>
    /// Harmony of Judgement Minion - An angelic judge that fires judgment rays.
    /// </summary>
    public class HarmonyOfJudgementMinion : ModProjectile
    {
        private float hoverAngle;
        private int attackCooldown;
        
        public override void SetStaticDefaults()
        {
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 36;
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
            
            hoverAngle += 0.02f;
            attackCooldown = Math.Max(0, attackCooldown - 1);
            
            // Hover near player
            float hoverOffset = (float)Math.Sin(hoverAngle) * 25f;
            Vector2 idealPos = owner.Center + new Vector2(owner.direction * -70f, -60f + hoverOffset);
            Vector2 toIdeal = idealPos - Projectile.Center;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdeal * 0.1f, 0.07f);
            
            Projectile.spriteDirection = owner.direction;
            
            // Find and attack target
            NPC target = FindTarget(owner, 800f);
            if (target != null && attackCooldown == 0)
            {
                // Fire judgment ray
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, toTarget * 16f,
                    ModContent.ProjectileType<JudgementRay>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                
                attackCooldown = 18;
                
                // Fire VFX
                DiesIraeVFX.FireImpact(Projectile.Center + toTarget * 20f, 0.4f);
                DiesIraeVFX.SpawnMusicNote(Projectile.Center + toTarget * 15f, toTarget * 2f, DiesIraeColors.HellfireGold, 0.75f);
                
                SoundEngine.PlaySound(SoundID.Item12 with { Pitch = 0.3f, Volume = 0.6f }, Projectile.Center);
            }
            
            // Ambient golden aura
            if (Main.rand.NextBool(6))
            {
                var glow = new GenericGlowParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(18f, 18f),
                    new Vector2(0, -0.8f),
                    DiesIraeColors.HellfireGold * 0.4f, 0.2f, 18, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Music notes
            if (Main.rand.NextBool(18))
            {
                DiesIraeVFX.SpawnMusicNote(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f),
                    new Vector2(0, -0.6f), DiesIraeColors.HellfireGold, 0.7f);
            }
            
            Lighting.AddLight(Projectile.Center, DiesIraeColors.HellfireGold.ToVector3() * 0.5f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            
            // Golden glow
            Texture2D glow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow4").Value;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.12f + 0.88f;
            sb.Draw(glow, drawPos, null, DiesIraeColors.HellfireGold * 0.35f, 0f, glow.Size() / 2f, 0.6f * pulse, SpriteEffects.None, 0f);
            sb.Draw(glow, drawPos, null, DiesIraeColors.InfernalWhite * 0.2f, 0f, glow.Size() / 2f, 0.4f * pulse, SpriteEffects.None, 0f);
            
            sb.Draw(tex, drawPos, null, Color.White, 0f, origin, Projectile.scale, effects, 0f);
            
            return false;
        }
        
        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<HarmonyOfJudgementBuff>());
                return false;
            }
            
            if (owner.HasBuff(ModContent.BuffType<HarmonyOfJudgementBuff>()))
                Projectile.timeLeft = 2;
            
            return true;
        }
        
        private NPC FindTarget(Player owner, float range)
        {
            if (owner.HasMinionAttackTargetNPC)
            {
                NPC manual = Main.npc[owner.MinionAttackTargetNPC];
                if (manual.active && manual.CanBeChasedBy(Projectile) && Vector2.Distance(owner.Center, manual.Center) < range * 1.5f)
                    return manual;
            }
            
            NPC closest = null;
            float closestDist = range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy(Projectile))
                {
                    float dist = Vector2.Distance(owner.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }
            return closest;
        }
    }
    
    /// <summary>
    /// Judgement Ray - Projectile fired by the Harmony of Judgement minion.
    /// </summary>
    public class JudgementRay : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/FlamingWispProjectileSmall";
        
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
            Projectile.extraUpdates = 1;
        }
        
        public override void AI()
        {
            // === LAYER 1: Core glow trail ===
            if (Main.rand.NextBool(2))
            {
                var trail = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.08f,
                    DiesIraeColors.HellfireGold * 0.6f, 0.2f, 12, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // === LAYER 2: Afterimage ray effect ===
            if (Projectile.velocity.Length() > 5f)
            {
                DiesIraeVFX.AfterimageTrail(Projectile.Center, Projectile.velocity, 0.25f, DiesIraeColors.HellfireGold, 4);
            }
            
            // === LAYER 3: Spiral judgment trail ===
            if (Main.rand.NextBool(2))
            {
                DiesIraeVFX.SpiralTrail(Projectile.Center, Projectile.velocity, DiesIraeColors.EmberOrange, 0.2f, Main.GameUpdateCount * 0.2f);
            }
            
            // === LAYER 4: Small orbiting sparks ===
            if (Main.GameUpdateCount % 5 == 0)
            {
                DiesIraeVFX.OrbitingSparks(Projectile.Center, DiesIraeColors.Crimson, 8f, 2, Main.GameUpdateCount * 0.15f, 0.15f);
            }
            
            // === LAYER 5: Occasional music note ===
            if (Main.rand.NextBool(12))
            {
                DiesIraeVFX.SpawnMusicNote(Projectile.Center, -Projectile.velocity * 0.1f, DiesIraeColors.HellfireGold, 0.6f);
            }
            
            Projectile.rotation = Projectile.velocity.ToRotation();
            Lighting.AddLight(Projectile.Center, DiesIraeColors.HellfireGold.ToVector3() * 0.45f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            DiesIraeVFX.FireImpact(target.Center, 0.5f);
        }
        
        public override void OnKill(int timeLeft)
        {
            CustomParticles.GenericFlare(Projectile.Center, DiesIraeColors.HellfireGold, 0.5f, 15);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = tex.Size() / 2f;
            
            // Bloom layers
            Color glowColor = DiesIraeColors.HellfireGold;
            glowColor.A = 0;
            
            sb.Draw(tex, drawPos, null, glowColor * 0.4f, Projectile.rotation, origin, 0.6f, SpriteEffects.None, 0f);
            sb.Draw(tex, drawPos, null, glowColor * 0.7f, Projectile.rotation, origin, 0.4f, SpriteEffects.None, 0f);
            sb.Draw(tex, drawPos, null, DiesIraeColors.InfernalWhite * 0.8f, Projectile.rotation, origin, 0.25f, SpriteEffects.None, 0f);
            
            return false;
        }
    }
    
    /// <summary>
    /// Wrathful Contract Minion - A demonic entity bound by a contract of wrath.
    /// Aggressively pursues enemies with devastating attacks.
    /// </summary>
    public class WrathfulContractMinion : ModProjectile
    {
        private float orbitAngle;
        private int attackCooldown;
        private bool isAttacking;
        private Vector2 attackTarget;
        private int attackTimer;
        private int comboCounter;
        
        public override void SetStaticDefaults()
        {
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 44;
            Projectile.height = 44;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 2f; // Uses 2 slots
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }
        
        public override bool? CanCutTiles() => false;
        
        public override bool MinionContactDamage() => true;
        
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            
            if (!CheckActive(owner))
                return;
            
            orbitAngle += 0.035f;
            attackCooldown = Math.Max(0, attackCooldown - 1);
            
            NPC target = FindTarget(owner, 1000f);
            
            if (!isAttacking)
            {
                // Orbit around player
                float orbitRadius = 100f + 40f * (float)Math.Sin(orbitAngle * 0.4f);
                Vector2 idealPos = owner.Center + orbitAngle.ToRotationVector2() * orbitRadius;
                Vector2 toIdeal = idealPos - Projectile.Center;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdeal * 0.12f, 0.08f);
                
                // Check for attack opportunity
                if (target != null && attackCooldown == 0)
                {
                    isAttacking = true;
                    attackTarget = target.Center;
                    attackTimer = 0;
                    attackCooldown = 35;
                    comboCounter++;
                }
            }
            else
            {
                attackTimer++;
                
                // Dash attack
                if (attackTimer < 18)
                {
                    Vector2 toTarget = (attackTarget - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    Projectile.velocity = toTarget * 26f;
                    
                    // Attack VFX - heavy trails
                    DiesIraeVFX.FireTrail(Projectile.Center, Projectile.velocity, 0.6f);
                    
                    // Every 3rd combo attack causes explosion
                    if (attackTimer == 15 && comboCounter % 3 == 0)
                    {
                        // Big explosion at current position
                        DiesIraeVFX.FireImpact(Projectile.Center, 1.0f);
                        
                        // Spawn fire projectiles
                        for (int i = 0; i < 6; i++)
                        {
                            float angle = MathHelper.TwoPi * i / 6f;
                            Vector2 fireVel = angle.ToRotationVector2() * 8f;
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, fireVel,
                                ModContent.ProjectileType<WrathFireball>(), Projectile.damage / 3, 1f, Projectile.owner);
                        }
                        
                        SoundEngine.PlaySound(SoundID.Item74 with { Pitch = -0.2f }, Projectile.Center);
                    }
                }
                else
                {
                    isAttacking = false;
                }
            }
            
            Projectile.rotation = Projectile.velocity.X * 0.015f;
            Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;
            
            // Ambient particles - heavy and ominous
            if (Main.rand.NextBool(3))
            {
                Color flameColor = Main.rand.NextBool() ? DiesIraeColors.BloodRed : DiesIraeColors.EmberOrange;
                var flame = new GenericGlowParticle(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f),
                    new Vector2(0, Main.rand.NextFloat(-1f, -2.5f)), flameColor * 0.5f, 0.3f, 20, true);
                MagnumParticleHandler.SpawnParticle(flame);
            }
            
            // Heavy smoke
            if (Main.rand.NextBool(5))
            {
                var smoke = new HeavySmokeParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(15f, 15f),
                    new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(0.5f, 1.5f)),
                    DiesIraeColors.CharredBlack * 0.5f,
                    Main.rand.Next(25, 40), 0.25f, 0.35f, 0.02f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Music notes
            if (Main.rand.NextBool(12))
            {
                DiesIraeVFX.SpawnMusicNote(Projectile.Center + Main.rand.NextVector2Circular(18f, 18f),
                    new Vector2(0, -0.6f), DiesIraeColors.Crimson, 0.75f);
            }
            
            Lighting.AddLight(Projectile.Center, DiesIraeColors.BloodRed.ToVector3() * 0.6f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            DiesIraeVFX.FireImpact(target.Center, 0.9f);
            
            // Music note burst on hit
            for (int n = 0; n < 4; n++)
            {
                float noteAngle = MathHelper.TwoPi * n / 4f + Main.rand.NextFloat(-0.4f, 0.4f);
                Vector2 noteVel = noteAngle.ToRotationVector2() * Main.rand.NextFloat(2f, 3f);
                DiesIraeVFX.SpawnMusicNote(target.Center, noteVel, DiesIraeColors.GetGradient(Main.rand.NextFloat()), 0.7f);
            }
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            
            // Demonic glow
            Texture2D glow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow3").Value;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.15f + 0.85f;
            
            // Intensify during attack
            float attackIntensity = isAttacking ? 1.3f : 1f;
            
            sb.Draw(glow, drawPos, null, DiesIraeColors.BloodRed * 0.4f * attackIntensity, 0f, glow.Size() / 2f, 0.8f * pulse, SpriteEffects.None, 0f);
            sb.Draw(glow, drawPos, null, DiesIraeColors.EmberOrange * 0.3f * attackIntensity, 0f, glow.Size() / 2f, 0.5f * pulse, SpriteEffects.None, 0f);
            
            // Main sprite
            sb.Draw(tex, drawPos, null, Color.White, Projectile.rotation, origin, Projectile.scale, effects, 0f);
            
            return false;
        }
        
        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<WrathfulContractBuff>());
                return false;
            }
            
            if (owner.HasBuff(ModContent.BuffType<WrathfulContractBuff>()))
                Projectile.timeLeft = 2;
            
            return true;
        }
        
        private NPC FindTarget(Player owner, float range)
        {
            if (owner.HasMinionAttackTargetNPC)
            {
                NPC manual = Main.npc[owner.MinionAttackTargetNPC];
                if (manual.active && manual.CanBeChasedBy(Projectile) && Vector2.Distance(owner.Center, manual.Center) < range * 1.5f)
                    return manual;
            }
            
            NPC closest = null;
            float closestDist = range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy(Projectile))
                {
                    float dist = Vector2.Distance(owner.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }
            return closest;
        }
    }
    
    /// <summary>
    /// Wrath Fireball - Fireball spawned by the Wrathful Contract's combo attack.
    /// </summary>
    public class WrathFireball : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/FlamingWispProjectileSmall";
        
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 50;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }
        
        public override void AI()
        {
            // Gravity
            Projectile.velocity.Y += 0.15f;
            
            // === LAYER 1: Core glow trail ===
            if (Main.rand.NextBool(2))
            {
                var trail = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f,
                    DiesIraeColors.EmberOrange * 0.6f, 0.3f, 15, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // === LAYER 2: Fire trail effect ===
            DiesIraeVFX.FireTrail(Projectile.Center, Projectile.velocity, 0.5f);
            
            // === LAYER 3: Afterimage for speed ===
            if (Projectile.velocity.Length() > 4f)
            {
                DiesIraeVFX.AfterimageTrail(Projectile.Center, Projectile.velocity, 0.35f, DiesIraeColors.BloodRed, 3);
            }
            
            // === LAYER 4: Spiral ember pattern ===
            if (Main.rand.NextBool(2))
            {
                DiesIraeVFX.SpiralTrail(Projectile.Center, Projectile.velocity, DiesIraeColors.Crimson, 0.3f, Projectile.rotation);
            }
            
            // === LAYER 5: Small orbiting sparks ===
            if (Main.GameUpdateCount % 4 == 0)
            {
                DiesIraeVFX.OrbitingSparks(Projectile.Center, DiesIraeColors.EmberOrange, 10f, 2, Projectile.rotation * 0.5f, 0.2f);
            }
            
            // === LAYER 6: Occasional music note ===
            if (Main.rand.NextBool(10))
            {
                DiesIraeVFX.SpawnMusicNote(Projectile.Center, -Projectile.velocity * 0.1f, DiesIraeColors.HellfireGold, 0.65f);
            }
            
            Projectile.rotation += 0.15f;
            Lighting.AddLight(Projectile.Center, DiesIraeColors.EmberOrange.ToVector3() * 0.5f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            DiesIraeVFX.FireImpact(target.Center, 0.5f);
        }
        
        public override void OnKill(int timeLeft)
        {
            DiesIraeVFX.FireImpact(Projectile.Center, 0.4f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = tex.Size() / 2f;
            
            Color glowColor = DiesIraeColors.EmberOrange;
            glowColor.A = 0;
            
            sb.Draw(tex, drawPos, null, glowColor * 0.5f, Projectile.rotation, origin, 0.5f, SpriteEffects.None, 0f);
            sb.Draw(tex, drawPos, null, DiesIraeColors.HellfireGold * 0.7f, Projectile.rotation, origin, 0.35f, SpriteEffects.None, 0f);
            sb.Draw(tex, drawPos, null, DiesIraeColors.InfernalWhite * 0.8f, Projectile.rotation, origin, 0.2f, SpriteEffects.None, 0f);
            
            return false;
        }
    }
    
    #endregion
    
    #region Minion Buffs
    
    public class DeathTollingBellBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_" + BuffID.OnFire;
        
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }
        
        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<DeathTollingBellMinion>()] > 0)
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
    
    public class HarmonyOfJudgementBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_" + BuffID.OnFire;
        
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }
        
        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<HarmonyOfJudgementMinion>()] > 0)
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
    
    public class WrathfulContractBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_" + BuffID.OnFire;
        
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }
        
        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<WrathfulContractMinion>()] > 0)
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
    
    #endregion
}
