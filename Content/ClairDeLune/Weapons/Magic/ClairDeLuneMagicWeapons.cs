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

namespace MagnumOpus.Content.ClairDeLune.Weapons.Magic
{
    #region Clockwork Grimoire - Temporal Tome
    
    /// <summary>
    /// Clockwork Grimoire - SUPREME FINAL BOSS magic tome
    /// Pages turn with temporal energy, each page summons different spells
    /// Cycles through 4 spell modes - lightning, crystal, gears, time fracture
    /// MUST EXCEED Ode to Joy magic damage (3600)
    /// </summary>
    public class ClockworkGrimoire : ModItem
    {
        private int spellMode = 0;
        private const int NumModes = 4;

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.damage = 4200; // FINAL BOSS - 17% above Ode to Joy (3600)
            Item.DamageType = DamageClass.Magic;
            Item.mana = 16;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item103;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<GrimoireLightningProjectile>();
            Item.shootSpeed = 16f;
            Item.crit = 20;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            string[] modeNames = { "Lightning Arc", "Crystal Storm", "Gear Barrage", "Time Fracture" };
            tooltips.Add(new TooltipLine(Mod, "Effect1", "A tome of temporal magic - cycles through 4 spell modes"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", $"Current mode: {modeNames[spellMode]}"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Right-click to cycle spell modes"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Each spell has unique properties and effects"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Forbidden knowledge etched in clockwork precision'") 
            { 
                OverrideColor = ClairDeLuneColors.DarkGray 
            });
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // Cycle mode
                spellMode = (spellMode + 1) % NumModes;
                SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.3f + spellMode * 0.1f }, player.Center);
                
                // Mode change VFX
                ClairDeLuneVFX.OrbitingGears(player.Center, 40f, 4, Main.GameUpdateCount * 0.1f, 0.5f);
                
                return false;
            }
            return base.CanUseItem(player);
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            switch (spellMode)
            {
                case 0: // Lightning Arc
                    type = ModContent.ProjectileType<GrimoireLightningProjectile>();
                    break;
                case 1: // Crystal Storm
                    type = ModContent.ProjectileType<GrimoireCrystalProjectile>();
                    break;
                case 2: // Gear Barrage
                    type = ModContent.ProjectileType<GrimoireGearProjectile>();
                    break;
                case 3: // Time Fracture
                    type = ModContent.ProjectileType<GrimoireTimeFractureProjectile>();
                    damage = (int)(damage * 1.5f); // Bonus damage for slow attack
                    break;
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 spawnPos = player.Center + velocity.SafeNormalize(Vector2.UnitX) * 30f;
            
            switch (spellMode)
            {
                case 0: // Lightning Arc - fires chain lightning
                    Projectile.NewProjectile(source, spawnPos, velocity, type, damage, knockback, player.whoAmI);
                    ClairDeLuneVFX.SpawnLightningBurst(spawnPos, velocity * 0.3f, false, 0.6f);
                    break;
                    
                case 1: // Crystal Storm - fires spread of crystals
                    for (int i = 0; i < 5; i++)
                    {
                        float spread = MathHelper.ToRadians(-12f + i * 6f);
                        Vector2 spreadVel = velocity.RotatedBy(spread);
                        Projectile.NewProjectile(source, spawnPos, spreadVel, type, damage, knockback, player.whoAmI);
                    }
                    ClairDeLuneVFX.CrystalShatterBurst(spawnPos, 6, 4f, 0.5f);
                    break;
                    
                case 2: // Gear Barrage - fires 3 homing gears
                    for (int i = 0; i < 3; i++)
                    {
                        float spread = MathHelper.ToRadians(-15f + i * 15f);
                        Vector2 spreadVel = velocity.RotatedBy(spread) * 0.9f;
                        Projectile.NewProjectile(source, spawnPos, spreadVel, type, damage, knockback, player.whoAmI);
                    }
                    ClairDeLuneVFX.ClockworkGearCascade(spawnPos, 6, 5f, 0.6f);
                    break;
                    
                case 3: // Time Fracture - fires slow but devastating time bomb
                    Projectile.NewProjectile(source, spawnPos, velocity * 0.6f, type, damage, knockback * 2, player.whoAmI);
                    ClairDeLuneVFX.TemporalChargeRelease(spawnPos, 0.7f);
                    Item.useTime = 35;
                    Item.useAnimation = 35;
                    break;
            }
            
            // Reset use time for non-Time Fracture modes
            if (spellMode != 3)
            {
                Item.useTime = 22;
                Item.useAnimation = 22;
            }
            
            return false;
        }

        public override void HoldItem(Player player)
        {
            // Ambient page-turning effect
            if (Main.rand.NextBool(10))
            {
                Vector2 tomePos = player.Center + player.direction * new Vector2(25f, -10f);
                Color modeColor = spellMode switch
                {
                    0 => ClairDeLuneColors.ElectricBlue,
                    1 => ClairDeLuneColors.Crystal,
                    2 => ClairDeLuneColors.Brass,
                    3 => ClairDeLuneColors.Crimson,
                    _ => ClairDeLuneColors.MoonlightSilver
                };
                
                var pageParticle = new GenericGlowParticle(tomePos, 
                    Main.rand.NextVector2Circular(1f, 2f) + Vector2.UnitY * -1f,
                    modeColor * 0.7f, 0.3f, 20, true);
                MagnumParticleHandler.SpawnParticle(pageParticle);
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfClairDeLune>(), 24)
                .AddIngredient(ModContent.ItemType<ClairDeLuneResonantEnergy>(), 18)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfClairDeLune>(), 3)
                .AddIngredient(ItemID.SpellTome, 1)
                .AddIngredient(ItemID.LunarBar, 18)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    /// <summary>
    /// Grimoire Lightning - Chain lightning spell
    /// </summary>
    public class GrimoireLightningProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/LightningStreak";

        private int chainsRemaining = 4;
        private int lastHitNPC = -1;

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 2;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Lightning trail
            if (Main.rand.NextBool(2))
            {
                ClairDeLuneVFX.SpawnLightningBurst(Projectile.Center, Main.rand.NextVector2Circular(2f, 2f), false, 0.4f);
            }
            
            ClairDeLuneVFX.TemporalTrail(Projectile.Center, Projectile.velocity, 0.6f);
            Lighting.AddLight(Projectile.Center, ClairDeLuneColors.ElectricBlue.ToVector3() * 0.7f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            ClairDeLuneVFX.LightningStrikeExplosion(target.Center, 0.6f);
            
            // Chain to nearby enemies
            if (chainsRemaining > 0 && Main.myPlayer == Projectile.owner)
            {
                NPC nextTarget = FindChainTarget(target, 350f);
                if (nextTarget != null)
                {
                    // Draw chain lightning arc
                    ClairDeLuneVFX.LightningArc(target.Center, nextTarget.Center, 8, 15f, 0.6f);
                    
                    // Redirect projectile
                    Vector2 newVel = (nextTarget.Center - target.Center).SafeNormalize(Vector2.UnitX) * 20f;
                    Projectile.Center = target.Center;
                    Projectile.velocity = newVel;
                    
                    lastHitNPC = target.whoAmI;
                    chainsRemaining--;
                }
            }
            
            target.AddBuff(BuffID.Electrified, 180);
        }

        private NPC FindChainTarget(NPC current, float range)
        {
            NPC closest = null;
            float closestDist = range;
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.whoAmI != current.whoAmI && npc.whoAmI != lastHitNPC)
                {
                    float dist = Vector2.Distance(current.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }
            
            return closest;
        }

        public override void OnKill(int timeLeft)
        {
            ClairDeLuneVFX.SpawnLightningBurst(Projectile.Center, Vector2.Zero, true, 0.6f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Color glow = ClairDeLuneColors.ElectricBlue * 0.8f;
            glow.A = 0;
            Main.spriteBatch.Draw(tex, drawPos, null, glow, Projectile.rotation, tex.Size() / 2f, 1.3f, SpriteEffects.None, 0f);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return true;
        }
    }

    /// <summary>
    /// Grimoire Crystal - Piercing crystal shard
    /// </summary>
    public class GrimoireCrystalProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SmallCrystalShard";

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            
            ClairDeLuneVFX.TemporalTrail(Projectile.Center, Projectile.velocity, 0.5f);
            Lighting.AddLight(Projectile.Center, ClairDeLuneColors.Crystal.ToVector3() * 0.4f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            ClairDeLuneVFX.CrystalShatterBurst(target.Center, 6, 4f, 0.5f);
            target.AddBuff(BuffID.Frostburn2, 120);
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
            Main.spriteBatch.Draw(tex, drawPos, null, glow, Projectile.rotation, tex.Size() / 2f, 1.4f, SpriteEffects.None, 0f);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return true;
        }
    }

    /// <summary>
    /// Grimoire Gear - Homing gear projectile
    /// </summary>
    public class GrimoireGearProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/ClockworkGearSmall";

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 150;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            Projectile.rotation += 0.15f;
            
            // Homing after initial flight
            if (Projectile.timeLeft < 130)
            {
                NPC target = FindClosestNPC(500f);
                if (target != null)
                {
                    Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 16f, 0.06f);
                }
            }
            
            if (Main.rand.NextBool(3))
            {
                var trail = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.05f,
                    ClairDeLuneColors.Brass * 0.5f, 0.2f, 12, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            Lighting.AddLight(Projectile.Center, ClairDeLuneColors.Brass.ToVector3() * 0.4f);
        }

        private NPC FindClosestNPC(float range)
        {
            NPC closest = null;
            float closestDist = range;
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.CanBeChasedBy())
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }
            
            return closest;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            ClairDeLuneVFX.SpawnClockworkGear(target.Center, Main.rand.NextVector2Circular(4f, 4f), false, 0.5f);
            target.AddBuff(BuffID.BetsysCurse, 180);
        }

        public override void OnKill(int timeLeft)
        {
            ClairDeLuneVFX.ClockworkGearCascade(Projectile.Center, 6, 5f, 0.5f);
        }
    }

    /// <summary>
    /// Grimoire Time Fracture - Slow but devastating temporal bomb
    /// </summary>
    public class GrimoireTimeFractureProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/LightningBurst";

        private int timer = 0;

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public override void AI()
        {
            timer++;
            Projectile.velocity *= 0.97f; // Slow down over time
            
            float progress = timer / 90f;
            
            // Growing temporal distortion
            ClairDeLuneVFX.HeavyTemporalTrail(Projectile.Center, Projectile.velocity, 0.8f + progress * 0.4f);
            
            if (Main.GameUpdateCount % 3 == 0)
            {
                ClairDeLuneVFX.OrbitingGears(Projectile.Center, 25f + progress * 15f, 4, Main.GameUpdateCount * 0.08f, 0.4f + progress * 0.3f);
            }
            
            // Lightning crackle
            if (Main.rand.NextBool(4))
            {
                ClairDeLuneVFX.SpawnLightningBurst(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f),
                    Main.rand.NextVector2Circular(3f, 3f), false, 0.4f);
            }
            
            Lighting.AddLight(Projectile.Center, ClairDeLuneColors.Crimson.ToVector3() * (0.5f + progress * 0.5f));
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Slow, 240);
            target.AddBuff(BuffID.Confused, 120);
        }

        public override void OnKill(int timeLeft)
        {
            // MASSIVE temporal explosion
            ClairDeLuneVFX.TemporalDeathExplosion(Projectile.Center, 1.5f);
            ClairDeLuneVFX.LightningStrikeExplosion(Projectile.Center, 1f);
            
            // Damage all nearby enemies
            float explosionRadius = 200f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && !npc.dontTakeDamage)
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < explosionRadius && Main.myPlayer == Projectile.owner)
                    {
                        float damageScale = 1f - (dist / explosionRadius) * 0.5f;
                        npc.SimpleStrikeNPC((int)(Projectile.damage * damageScale), 0, false, 0f);
                        npc.AddBuff(BuffID.Slow, 300);
                    }
                }
            }
            
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.3f }, Projectile.Center);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            float progress = timer / 90f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2").Value;
            Vector2 origin = tex.Size() / 2f;
            
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.2f;
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Color outerGlow = ClairDeLuneColors.Crimson * 0.5f;
            outerGlow.A = 0;
            Main.spriteBatch.Draw(tex, drawPos, null, outerGlow, Main.GameUpdateCount * 0.02f, origin, (1f + progress) * pulse, SpriteEffects.None, 0f);
            
            Color midGlow = ClairDeLuneColors.DarkGray * 0.6f;
            midGlow.A = 0;
            Main.spriteBatch.Draw(tex, drawPos, null, midGlow, -Main.GameUpdateCount * 0.03f, origin, (0.7f + progress * 0.5f) * pulse, SpriteEffects.None, 0f);
            
            Color coreGlow = ClairDeLuneColors.BrightWhite * 0.7f;
            coreGlow.A = 0;
            Main.spriteBatch.Draw(tex, drawPos, null, coreGlow, 0f, origin, (0.3f + progress * 0.2f) * pulse, SpriteEffects.None, 0f);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
    }

    #endregion

    #region Orrery of Dreams - Celestial Magic Orb
    
    /// <summary>
    /// Orrery of Dreams - SUPREME FINAL BOSS magic staff
    /// Summons a miniature clockwork orrery that fires celestial beams
    /// Orbs orbit the orrery and can be launched at enemies
    /// Ultimate attack aligns the orrery for a devastating cosmic laser
    /// </summary>
    public class OrreryOfDreams : ModItem
    {
        private int alignmentCharge = 0;
        private const int AlignmentMax = 80;

        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 48;
            Item.damage = 4500; // FINAL BOSS tier
            Item.DamageType = DamageClass.Magic;
            Item.mana = 22;
            Item.useTime = 28;
            Item.useAnimation = 28;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item117;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<OrreryOrbitProjectile>();
            Item.shootSpeed = 0f;
            Item.crit = 22;
            Item.channel = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Summons a clockwork orrery with orbiting celestial spheres"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Spheres automatically fire beams at nearby enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Hold fire to charge alignment - releases cosmic laser"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Cosmic laser deals massive damage in a line"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Dreams of celestial mechanics made manifest'") 
            { 
                OverrideColor = ClairDeLuneColors.MoonlightSilver 
            });
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Check for existing orrery
            bool hasOrrery = false;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].owner == player.whoAmI &&
                    Main.projectile[i].type == ModContent.ProjectileType<OrreryControllerProjectile>())
                {
                    hasOrrery = true;
                    break;
                }
            }
            
            if (!hasOrrery)
            {
                // Spawn the orrery controller
                Projectile.NewProjectile(source, player.Center, Vector2.Zero,
                    ModContent.ProjectileType<OrreryControllerProjectile>(), damage, knockback, player.whoAmI);
            }
            
            return false;
        }

        public override void HoldItem(Player player)
        {
            // Charge alignment while channeling
            if (player.channel)
            {
                alignmentCharge = Math.Min(alignmentCharge + 1, AlignmentMax);
                
                float progress = (float)alignmentCharge / AlignmentMax;
                
                // Charging VFX
                if (alignmentCharge > 20)
                {
                    ClairDeLuneVFX.TemporalChargeUp(player.Center, progress * 0.5f, 0.5f);
                    
                    // Find orrery and tell it to align
                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        if (Main.projectile[i].active && Main.projectile[i].owner == player.whoAmI &&
                            Main.projectile[i].type == ModContent.ProjectileType<OrreryControllerProjectile>())
                        {
                            Main.projectile[i].ai[1] = progress; // Send alignment progress
                            break;
                        }
                    }
                }
                
                // Release cosmic laser at full charge
                if (alignmentCharge >= AlignmentMax)
                {
                    FireCosmicLaser(player);
                    alignmentCharge = 0;
                }
            }
            else
            {
                alignmentCharge = Math.Max(0, alignmentCharge - 2);
            }
        }

        private void FireCosmicLaser(Player player)
        {
            Vector2 direction = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
            
            // Fire cosmic laser
            Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, direction * 25f,
                ModContent.ProjectileType<CosmicLaserProjectile>(), Item.damage * 3, Item.knockBack * 2, player.whoAmI);
            
            ClairDeLuneVFX.TemporalChargeRelease(player.Center, 1.2f);
            ClairDeLuneVFX.LightningStrikeExplosion(player.Center, 0.8f);
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.5f, Volume = 1.2f }, player.Center);
            
            player.itemTime = 60;
            player.itemAnimation = 60;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfClairDeLune>(), 28)
                .AddIngredient(ModContent.ItemType<ClairDeLuneResonantEnergy>(), 22)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfClairDeLune>(), 4)
                .AddIngredient(ItemID.LunarBar, 22)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    /// <summary>
    /// Orrery Controller - Controls the orbiting spheres and auto-attacks
    /// </summary>
    public class OrreryControllerProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/ClockworkGearLarge";

        private float rotation = 0f;
        private float AlignmentProgress => Projectile.ai[1];

        public override void SetDefaults()
        {
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 600;
            Projectile.penetrate = -1;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            
            // Stay alive while player holds the weapon
            if (owner.HeldItem.type == ModContent.ItemType<OrreryOfDreams>())
            {
                Projectile.timeLeft = 30;
            }
            
            // Follow player
            Projectile.Center = owner.Center;
            
            rotation += 0.03f;
            
            // Draw orbiting spheres visually
            int sphereCount = 3;
            float orbitRadius = 80f;
            
            for (int i = 0; i < sphereCount; i++)
            {
                float sphereAngle = rotation + MathHelper.TwoPi * i / sphereCount;
                Vector2 spherePos = Projectile.Center + sphereAngle.ToRotationVector2() * orbitRadius;
                
                ClairDeLuneVFX.SpawnClockworkGear(spherePos, Vector2.Zero, false, 0.4f);
                
                // Auto-attack nearby enemies
                if (Main.GameUpdateCount % 30 == i * 10 && Main.myPlayer == Projectile.owner)
                {
                    NPC target = FindClosestNPC(spherePos, 400f);
                    if (target != null)
                    {
                        Vector2 beamDir = (target.Center - spherePos).SafeNormalize(Vector2.UnitX);
                        
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), spherePos, beamDir * 20f,
                            ModContent.ProjectileType<OrreryBeamProjectile>(), Projectile.damage / 2, 2f, Projectile.owner);
                        
                        ClairDeLuneVFX.SpawnLightningBurst(spherePos, beamDir * 3f, false, 0.5f);
                    }
                }
            }
            
            // Center gear
            ClairDeLuneVFX.OrbitingGears(Projectile.Center, 30f, 4, Main.GameUpdateCount * 0.05f, 0.3f);
            
            // Alignment visual
            if (AlignmentProgress > 0)
            {
                ClairDeLuneVFX.TemporalChargeUp(Projectile.Center, AlignmentProgress, 0.6f + AlignmentProgress * 0.4f);
            }
            
            Lighting.AddLight(Projectile.Center, ClairDeLuneColors.MoonlightSilver.ToVector3() * 0.6f);
        }

        private NPC FindClosestNPC(Vector2 position, float range)
        {
            NPC closest = null;
            float closestDist = range;
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.CanBeChasedBy())
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

        public override bool PreDraw(ref Color lightColor) => false;
    }

    /// <summary>
    /// Orrery Orbit Projectile - Unused, but required for shoot type
    /// </summary>
    public class OrreryOrbitProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SmallCrystalShard";

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.timeLeft = 1;
            Projectile.tileCollide = false;
        }
    }

    /// <summary>
    /// Orrery Beam - Auto-attack beam from orrery spheres
    /// </summary>
    public class OrreryBeamProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/LightningStreak";

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 3;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            ClairDeLuneVFX.TemporalTrail(Projectile.Center, Projectile.velocity, 0.4f);
            Lighting.AddLight(Projectile.Center, ClairDeLuneColors.Crystal.ToVector3() * 0.3f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            ClairDeLuneVFX.CrystalShatterBurst(target.Center, 4, 3f, 0.4f);
        }
    }

    /// <summary>
    /// Cosmic Laser - Ultimate charged attack
    /// </summary>
    public class CosmicLaserProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/LightningBurstThick";

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.extraUpdates = 2;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Heavy laser trail
            ClairDeLuneVFX.HeavyTemporalTrail(Projectile.Center, Projectile.velocity, 1.2f);
            
            // Lightning along beam
            if (Main.rand.NextBool(2))
            {
                Vector2 perpendicular = Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2);
                Vector2 offset = perpendicular * Main.rand.NextFloat(-40f, 40f);
                ClairDeLuneVFX.SpawnLightningBurst(Projectile.Center + offset, Main.rand.NextVector2Circular(3f, 3f), false, 0.5f);
            }
            
            // Crystal particles
            if (Main.rand.NextBool(3))
            {
                ClairDeLuneVFX.SpawnCrystalShard(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f),
                    -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(3f, 3f), false, 0.5f);
            }
            
            Lighting.AddLight(Projectile.Center, ClairDeLuneColors.MoonlightSilver.ToVector3() * 1.5f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            ClairDeLuneVFX.TemporalImpact(target.Center, 0.8f);
            target.AddBuff(BuffID.Electrified, 300);
            target.AddBuff(BuffID.Frostburn2, 300);
        }

        public override void OnKill(int timeLeft)
        {
            ClairDeLuneVFX.TemporalDeathExplosion(Projectile.Center, 1f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = tex.Size() / 2f;
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.15f;
            
            Color outerGlow = ClairDeLuneColors.MoonlightSilver * 0.5f;
            outerGlow.A = 0;
            Main.spriteBatch.Draw(tex, drawPos, null, outerGlow, Projectile.rotation, origin, 2f * pulse, SpriteEffects.None, 0f);
            
            Color midGlow = ClairDeLuneColors.Crystal * 0.7f;
            midGlow.A = 0;
            Main.spriteBatch.Draw(tex, drawPos, null, midGlow, Projectile.rotation, origin, 1.4f * pulse, SpriteEffects.None, 0f);
            
            Color coreGlow = ClairDeLuneColors.BrightWhite * 0.9f;
            coreGlow.A = 0;
            Main.spriteBatch.Draw(tex, drawPos, null, coreGlow, Projectile.rotation, origin, 0.8f * pulse, SpriteEffects.None, 0f);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
    }

    #endregion

    #region Requiem of Time - Mana Sword
    
    /// <summary>
    /// Requiem of Time - SUPREME FINAL BOSS "mana sword"
    /// A sword that uses mana instead of melee - magic weapon with sword aesthetics
    /// Swings create temporal slashes that travel forward
    /// Charged attack creates a massive time-freezing sweep
    /// </summary>
    public class RequiemOfTime : ModItem
    {
        private int chargeLevel = 0;
        private const int MaxCharge = 50;

        public override void SetDefaults()
        {
            Item.width = 60;
            Item.height = 60;
            Item.damage = 4800; // FINAL BOSS tier - "sword but uses mana"
            Item.DamageType = DamageClass.Magic; // Magic damage type!
            Item.mana = 12;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing; // Sword swing style!
            Item.knockBack = 7f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item71;
            Item.autoReuse = true;
            Item.noMelee = true; // Damage comes from projectile
            Item.noUseGraphic = false;
            Item.shoot = ModContent.ProjectileType<TemporalSlashProjectile>();
            Item.shootSpeed = 18f;
            Item.crit = 24;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "A blade forged from crystallized time"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Swings release temporal slashes"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Hold attack to charge a time-freezing sweep"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Charged sweep freezes and damages all enemies hit"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'When the final note plays, time itself shall cease'") 
            { 
                OverrideColor = ClairDeLuneColors.Crimson 
            });
        }

        public override void HoldItem(Player player)
        {
            // Charge while holding (right-click style mechanic even though it's swing)
            if (player.channel && player.itemAnimation == 0)
            {
                chargeLevel = Math.Min(chargeLevel + 1, MaxCharge);
                
                float progress = (float)chargeLevel / MaxCharge;
                if (chargeLevel > 15)
                {
                    Vector2 swordTip = player.Center + (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX) * 50f;
                    ClairDeLuneVFX.TemporalChargeUp(swordTip, progress, 0.6f);
                    
                    // Crystal convergence on blade
                    if (Main.rand.NextBool(3))
                    {
                        ClairDeLuneVFX.SpawnCrystalShard(swordTip + Main.rand.NextVector2Circular(50f * (1f - progress), 50f * (1f - progress)),
                            (swordTip - player.Center).SafeNormalize(Vector2.Zero) * 2f, false, 0.4f * progress);
                    }
                }
            }
            else if (chargeLevel > 0 && player.itemAnimation > 0)
            {
                chargeLevel = 0;
            }
            
            // Ambient blade glow
            if (Main.rand.NextBool(8))
            {
                Vector2 bladePos = player.Center + player.direction * new Vector2(30f, -10f);
                ClairDeLuneVFX.SpawnCrystalShard(bladePos, Main.rand.NextVector2Circular(1f, 1f), false, 0.3f);
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            bool isCharged = chargeLevel >= MaxCharge;
            
            if (isCharged)
            {
                // Charged sweep attack
                Projectile.NewProjectile(source, position, velocity, 
                    ModContent.ProjectileType<TimeFreezeSweepProjectile>(), damage * 2, knockback * 2, player.whoAmI);
                
                ClairDeLuneVFX.TemporalChargeRelease(position, 1f);
                ClairDeLuneVFX.LightningStrikeExplosion(position, 0.8f);
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f }, position);
            }
            else
            {
                // Normal slash
                float spread = MathHelper.ToRadians(Main.rand.NextFloat(-8f, 8f));
                Vector2 spreadVel = velocity.RotatedBy(spread);
                Projectile.NewProjectile(source, position, spreadVel, type, damage, knockback, player.whoAmI);
                
                ClairDeLuneVFX.CrystalShatterBurst(position + velocity.SafeNormalize(Vector2.Zero) * 40f, 6, 4f, 0.5f);
            }
            
            chargeLevel = 0;
            return false;
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            // Swing trail VFX (even though it's magic, it swings like a sword)
            Vector2 center = hitbox.Center.ToVector2();
            
            for (int i = 0; i < 2; i++)
            {
                Vector2 dustPos = center + Main.rand.NextVector2Circular(hitbox.Width / 2f, hitbox.Height / 2f);
                var swingParticle = new GenericGlowParticle(dustPos, 
                    player.velocity * 0.3f + Main.rand.NextVector2Circular(2f, 2f),
                    ClairDeLuneColors.Crystal * 0.7f, 0.3f, 15, true);
                MagnumParticleHandler.SpawnParticle(swingParticle);
            }
            
            if (Main.rand.NextBool(2))
            {
                ClairDeLuneVFX.SpawnCrystalShard(center, Main.rand.NextVector2Circular(3f, 3f), false, 0.35f);
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfClairDeLune>(), 30)
                .AddIngredient(ModContent.ItemType<ClairDeLuneResonantEnergy>(), 24)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfClairDeLune>(), 4)
                .AddIngredient(ItemID.LunarBar, 24)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    /// <summary>
    /// Temporal Slash - Basic swing projectile
    /// </summary>
    public class TemporalSlashProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SwordArc1";

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.velocity *= 0.98f; // Slight slowdown
            
            ClairDeLuneVFX.HeavyTemporalTrail(Projectile.Center, Projectile.velocity, 0.7f);
            
            if (Main.rand.NextBool(3))
            {
                ClairDeLuneVFX.SpawnCrystalShard(Projectile.Center, -Projectile.velocity * 0.2f, false, 0.35f);
            }
            
            Lighting.AddLight(Projectile.Center, ClairDeLuneColors.Crystal.ToVector3() * 0.6f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            ClairDeLuneVFX.TemporalImpact(target.Center, 0.6f);
            target.AddBuff(BuffID.Slow, 120);
        }

        public override void OnKill(int timeLeft)
        {
            ClairDeLuneVFX.CrystalShatterBurst(Projectile.Center, 10, 6f, 0.6f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = tex.Size() / 2f;
            
            float progress = 1f - (Projectile.timeLeft / 60f);
            float alpha = 1f - progress * 0.5f;
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Color outerGlow = ClairDeLuneColors.MoonlightSilver * 0.5f * alpha;
            outerGlow.A = 0;
            Main.spriteBatch.Draw(tex, drawPos, null, outerGlow, Projectile.rotation, origin, 1.4f, SpriteEffects.None, 0f);
            
            Color midGlow = ClairDeLuneColors.Crystal * 0.7f * alpha;
            midGlow.A = 0;
            Main.spriteBatch.Draw(tex, drawPos, null, midGlow, Projectile.rotation, origin, 1.1f, SpriteEffects.None, 0f);
            
            Color coreGlow = ClairDeLuneColors.BrightWhite * 0.8f * alpha;
            coreGlow.A = 0;
            Main.spriteBatch.Draw(tex, drawPos, null, coreGlow, Projectile.rotation, origin, 0.8f, SpriteEffects.None, 0f);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
    }

    /// <summary>
    /// Time Freeze Sweep - Charged attack that freezes enemies
    /// </summary>
    public class TimeFreezeSweepProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SwordArc3";

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 45;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Massive trail
            ClairDeLuneVFX.HeavyTemporalTrail(Projectile.Center, Projectile.velocity, 1.5f);
            
            // Crystal storm
            for (int i = 0; i < 2; i++)
            {
                Vector2 perpendicular = Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2);
                Vector2 offset = perpendicular * Main.rand.NextFloat(-50f, 50f);
                ClairDeLuneVFX.SpawnCrystalShard(Projectile.Center + offset, -Projectile.velocity * 0.3f, false, 0.6f);
            }
            
            // Lightning edges
            if (Main.rand.NextBool(2))
            {
                Vector2 perpendicular = Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2);
                float side = Main.rand.NextBool() ? 1f : -1f;
                ClairDeLuneVFX.SpawnLightningBurst(Projectile.Center + perpendicular * 40f * side,
                    Main.rand.NextVector2Circular(4f, 4f), false, 0.5f);
            }
            
            Lighting.AddLight(Projectile.Center, ClairDeLuneColors.Crimson.ToVector3() * 1.2f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            ClairDeLuneVFX.TemporalImpact(target.Center, 1f);
            ClairDeLuneVFX.CrystalShatterBurst(target.Center, 12, 8f, 0.8f);
            
            // Freeze effect - heavy slow + stun
            target.AddBuff(BuffID.Slow, 300);
            target.AddBuff(BuffID.Frozen, 60);
            target.AddBuff(BuffID.Frostburn2, 240);
        }

        public override void OnKill(int timeLeft)
        {
            ClairDeLuneVFX.TemporalDeathExplosion(Projectile.Center, 1.2f);
            ClairDeLuneVFX.LightningStrikeExplosion(Projectile.Center, 0.9f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = tex.Size() / 2f;
            
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.1f;
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Color outerGlow = ClairDeLuneColors.Crimson * 0.5f;
            outerGlow.A = 0;
            Main.spriteBatch.Draw(tex, drawPos, null, outerGlow, Projectile.rotation, origin, 2f * pulse, SpriteEffects.None, 0f);
            
            Color midGlow = ClairDeLuneColors.Crystal * 0.7f;
            midGlow.A = 0;
            Main.spriteBatch.Draw(tex, drawPos, null, midGlow, Projectile.rotation, origin, 1.5f * pulse, SpriteEffects.None, 0f);
            
            Color coreGlow = ClairDeLuneColors.BrightWhite * 0.9f;
            coreGlow.A = 0;
            Main.spriteBatch.Draw(tex, drawPos, null, coreGlow, Projectile.rotation, origin, 1f * pulse, SpriteEffects.None, 0f);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
    }

    #endregion
}
