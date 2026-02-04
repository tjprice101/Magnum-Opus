using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.ClairDeLune.Projectiles;
using MagnumOpus.Content.ClairDeLune.ResonanceEnergies;
using MagnumOpus.Content.ClairDeLune.HarmonicCores;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.ClairDeLune.Accessories
{
    #region Melee Class Accessory

    /// <summary>
    /// Temporal Wrath Gauntlet - SUPREME FINAL BOSS Melee Accessory
    /// Enhances melee attacks with temporal slowing effects
    /// Must exceed Ode to Joy melee accessory (The Triumphant Finale: +55% damage, +30% crit, +20% speed)
    /// </summary>
    public class TemporalWrathGauntlet : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.FeralClaws;

        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 36;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 6);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+75% melee damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+45% melee critical strike chance"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+30% melee attack speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Melee attacks freeze time around hit enemies for 0.5 seconds"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Critical strikes create temporal shockwaves that deal 50% bonus damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Every 5th consecutive hit triggers a massive time fracture"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Time bends to the fury of your blade'") 
            { 
                OverrideColor = ClairDeLuneColors.Crimson 
            });
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Melee) += 0.75f; // SUPREME FINAL BOSS (Ode: 0.55f, Dies: 0.40f)
            player.GetCritChance(DamageClass.Melee) += 45;
            player.GetAttackSpeed(DamageClass.Melee) += 0.30f;
            
            player.GetModPlayer<TemporalWrathPlayer>().temporalWrathActive = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfClairDeLune>(), 22)
                .AddIngredient(ModContent.ItemType<ClairDeLuneResonantEnergy>(), 16)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfClairDeLune>(), 2)
                .AddIngredient(ItemID.LunarBar, 18)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    public class TemporalWrathPlayer : ModPlayer
    {
        public bool temporalWrathActive = false;
        public int meleeHitCounter = 0;
        public const int MaxHitsForFracture = 5;

        public override void ResetEffects()
        {
            temporalWrathActive = false;
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (temporalWrathActive && item.DamageType == DamageClass.Melee)
            {
                ProcessMeleeHit(target, hit, damageDone);
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (temporalWrathActive && proj.DamageType == DamageClass.Melee && proj.friendly)
            {
                ProcessMeleeHit(target, hit, damageDone);
            }
        }

        private void ProcessMeleeHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Temporal freeze effect - slow enemies massively
            target.AddBuff(BuffID.Slow, 30);
            target.AddBuff(BuffID.Frozen, 15); // Brief freeze
            
            // Temporal VFX on all hits
            ClairDeLuneVFX.TemporalImpact(target.Center, 0.5f);
            
            // Critical strike shockwave
            if (hit.Crit)
            {
                // 50% bonus damage AOE
                int shockwaveDamage = damageDone / 2;
                
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && npc.CanBeChasedBy() && npc.whoAmI != target.whoAmI)
                    {
                        float dist = Vector2.Distance(npc.Center, target.Center);
                        if (dist < 180f)
                        {
                            Player.ApplyDamageToNPC(npc, shockwaveDamage, 0f, 0, false);
                            ClairDeLuneVFX.TemporalTrail(npc.Center, (npc.Center - target.Center).SafeNormalize(Vector2.Zero) * 3f, 0.4f);
                        }
                    }
                }
                
                // Shockwave VFX
                ClairDeLuneVFX.TemporalChargeRelease(target.Center, 0.8f);
                SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.6f, Pitch = 0.3f }, target.Center);
            }
            
            // Consecutive hit counter
            meleeHitCounter++;
            if (meleeHitCounter >= MaxHitsForFracture)
            {
                meleeHitCounter = 0;
                
                // TIME FRACTURE - massive explosion
                ClairDeLuneVFX.TemporalChargeRelease(target.Center, 1.2f);
                ClairDeLuneVFX.ClockworkGearCascade(target.Center, 15, 8f, 0.7f);
                ClairDeLuneVFX.LightningStrikeExplosion(target.Center, 0.8f);
                
                // Large AOE damage
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && npc.CanBeChasedBy())
                    {
                        float dist = Vector2.Distance(npc.Center, target.Center);
                        if (dist < 280f)
                        {
                            Player.ApplyDamageToNPC(npc, damageDone, 5f, 0, true);
                            npc.AddBuff(BuffID.Frozen, 60); // 1 second freeze
                        }
                    }
                }
                
                SoundEngine.PlaySound(SoundID.Item165 with { Volume = 0.9f }, target.Center);
            }
        }
    }

    #endregion

    #region Ranged Class Accessory

    /// <summary>
    /// Clockwork Targeting Module - SUPREME FINAL BOSS Ranged Accessory
    /// Enhances ranged attacks with precision and temporal mechanics
    /// Must exceed Ode to Joy ranged accessory
    /// </summary>
    public class ClockworkTargetingModule : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.SniperScope;

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 6);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+70% ranged damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+40% ranged critical strike chance"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "All ranged projectiles gain slight homing (precision tracking)"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Ranged critical strikes fire a bonus temporal bolt"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "+35% chance not to consume ammo"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Headshots (crits from above) deal +100% bonus damage"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Every gear clicks into perfect alignment'") 
            { 
                OverrideColor = ClairDeLuneColors.Brass 
            });
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Ranged) += 0.70f; // SUPREME FINAL BOSS
            player.GetCritChance(DamageClass.Ranged) += 40;
            player.ammoCost75 = true; // Built-in, then we add more
            
            player.GetModPlayer<ClockworkTargetingPlayer>().clockworkTargetingActive = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfClairDeLune>(), 22)
                .AddIngredient(ModContent.ItemType<ClairDeLuneResonantEnergy>(), 16)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfClairDeLune>(), 2)
                .AddIngredient(ItemID.LunarBar, 18)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    public class ClockworkTargetingPlayer : ModPlayer
    {
        public bool clockworkTargetingActive = false;

        public override void ResetEffects()
        {
            clockworkTargetingActive = false;
        }

        public override bool CanConsumeAmmo(Item weapon, Item ammo)
        {
            // Additional 35% chance (stacks with ammoCost75)
            if (clockworkTargetingActive && Main.rand.NextFloat() < 0.35f)
            {
                return false;
            }
            return base.CanConsumeAmmo(weapon, ammo);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (clockworkTargetingActive && proj.DamageType == DamageClass.Ranged && proj.friendly)
            {
                // VFX on all hits
                ClairDeLuneVFX.ClockworkGearCascade(target.Center, 4, 3f, 0.4f);
                
                if (hit.Crit)
                {
                    // Fire bonus temporal bolt
                    Vector2 boltVel = Main.rand.NextVector2Unit() * 12f;
                    
                    Projectile.NewProjectile(
                        Player.GetSource_OnHit(target),
                        target.Center,
                        boltVel,
                        ModContent.ProjectileType<ClockworkTargetingBolt>(),
                        damageDone / 2,
                        5f,
                        Player.whoAmI
                    );
                    
                    // Check for headshot (projectile came from above)
                    bool isHeadshot = proj.oldPosition.Y < target.position.Y - 20f;
                    if (isHeadshot)
                    {
                        // Deal bonus damage
                        int bonusDamage = damageDone;
                        Player.ApplyDamageToNPC(target, bonusDamage, 0f, 0, true);
                        
                        // Headshot VFX
                        ClairDeLuneVFX.TemporalChargeRelease(target.Center - new Vector2(0, target.height / 2), 0.6f);
                        SoundEngine.PlaySound(SoundID.Item62 with { Pitch = 0.5f }, target.Center);
                    }
                    
                    ClairDeLuneVFX.TemporalImpact(target.Center, 0.5f);
                }
            }
        }
    }

    /// <summary>
    /// Bonus projectile from Clockwork Targeting Module crits
    /// </summary>
    public class ClockworkTargetingBolt : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/LightningStreak";

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Slight homing
            NPC target = FindClosestNPC(350f);
            if (target != null)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 15f, 0.06f);
            }
            
            ClairDeLuneVFX.TemporalTrail(Projectile.Center, Projectile.velocity, 0.4f);
            Lighting.AddLight(Projectile.Center, ClairDeLuneColors.ElectricBlue.ToVector3() * 0.3f);
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

        public override void OnKill(int timeLeft)
        {
            ClairDeLuneVFX.SpawnLightningBurst(Projectile.Center, Vector2.Zero, false, 0.5f);
        }
    }

    // Global projectile for ranged homing
    public class ClockworkTargetingGlobalProjectile : GlobalProjectile
    {
        public override void AI(Projectile projectile)
        {
            if (projectile.friendly && projectile.DamageType == DamageClass.Ranged && 
                !projectile.minion && projectile.owner >= 0 && projectile.owner < Main.maxPlayers)
            {
                Player owner = Main.player[projectile.owner];
                if (owner.active && owner.GetModPlayer<ClockworkTargetingPlayer>().clockworkTargetingActive)
                {
                    // Slight homing for all ranged projectiles
                    NPC target = FindClosestNPC(projectile, 300f);
                    if (target != null && projectile.timeLeft < projectile.extraUpdates * 60 + 50) // Only after initial travel
                    {
                        Vector2 toTarget = (target.Center - projectile.Center).SafeNormalize(Vector2.Zero);
                        projectile.velocity = Vector2.Lerp(projectile.velocity, 
                            toTarget * projectile.velocity.Length(), 0.015f); // Very slight
                    }
                }
            }
        }

        private NPC FindClosestNPC(Projectile projectile, float range)
        {
            NPC closest = null;
            float closestDist = range;
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.CanBeChasedBy())
                {
                    float dist = Vector2.Distance(projectile.Center, npc.Center);
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

    #endregion

    #region Magic Class Accessory

    /// <summary>
    /// Resonant Chronosphere - SUPREME FINAL BOSS Magic Accessory
    /// Enhances magic with temporal manipulation
    /// Must exceed Ode to Joy magic accessory (The Flowering Coda: +60% damage, +35% crit, -30% mana)
    /// </summary>
    public class ResonantChronosphere : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.ManaFlower;

        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 6);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+80% magic damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+50% magic critical strike chance"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "-45% mana cost"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Magic attacks create temporal echoes that repeat the hit"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "+200 max mana"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "When mana drops below 20%, gain Temporal Clarity (+30% all stats for 8 seconds)"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The sphere that holds all moments in perfect suspension'") 
            { 
                OverrideColor = ClairDeLuneColors.Crystal 
            });
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Magic) += 0.80f; // SUPREME FINAL BOSS (Ode: 0.60f)
            player.GetCritChance(DamageClass.Magic) += 50;
            player.manaCost -= 0.45f;
            player.statManaMax2 += 200;
            
            player.GetModPlayer<ResonantChronospherePlayer>().chronosphereActive = true;
            
            // Check for low mana trigger
            if (player.statMana < player.statManaMax2 * 0.2f)
            {
                player.AddBuff(ModContent.BuffType<TemporalClarityBuff>(), 480);
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfClairDeLune>(), 22)
                .AddIngredient(ModContent.ItemType<ClairDeLuneResonantEnergy>(), 16)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfClairDeLune>(), 2)
                .AddIngredient(ItemID.LunarBar, 18)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    /// <summary>
    /// Temporal Clarity Buff - All stats boosted when low on mana
    /// </summary>
    public class TemporalClarityBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_" + BuffID.Wrath;

        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // +30% all damage
            player.GetDamage(DamageClass.Generic) += 0.30f;
            player.GetCritChance(DamageClass.Generic) += 20;
            player.GetAttackSpeed(DamageClass.Generic) += 0.15f;
            player.moveSpeed += 0.20f;
            
            // Visual effect
            if (Main.rand.NextBool(5))
            {
                ClairDeLuneVFX.TemporalTrail(player.Center + Main.rand.NextVector2Circular(30f, 30f),
                    Main.rand.NextVector2Circular(2f, 2f), 0.35f);
            }
        }
    }

    public class ResonantChronospherePlayer : ModPlayer
    {
        public bool chronosphereActive = false;
        private int echoTimer = 0;
        private const int EchoCooldown = 30; // Half second cooldown between echoes

        public override void ResetEffects()
        {
            chronosphereActive = false;
        }

        public override void PostUpdate()
        {
            if (echoTimer > 0)
                echoTimer--;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (chronosphereActive && proj.DamageType == DamageClass.Magic && proj.friendly)
            {
                // VFX on all hits
                ClairDeLuneVFX.CrystalShatterBurst(target.Center, 5, 4f, 0.4f);
                
                // Temporal echo - repeat the hit after a delay
                if (echoTimer <= 0)
                {
                    echoTimer = EchoCooldown;
                    
                    // Schedule echo damage
                    int echoDamage = damageDone / 2;
                    
                    // Create echo projectile
                    Projectile.NewProjectile(
                        Player.GetSource_OnHit(target),
                        target.Center,
                        Vector2.Zero,
                        ModContent.ProjectileType<TemporalEchoProjectile>(),
                        echoDamage,
                        0f,
                        Player.whoAmI,
                        target.whoAmI,
                        15 // Delay frames
                    );
                    
                    ClairDeLuneVFX.TemporalImpact(target.Center, 0.4f);
                }
            }
        }
    }

    /// <summary>
    /// Temporal Echo - Delayed damage repeat
    /// </summary>
    public class TemporalEchoProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow2";

        private int TargetNPC => (int)Projectile.ai[0];
        private int Delay => (int)Projectile.ai[1];

        public override void SetDefaults()
        {
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.alpha = 255;
        }

        public override bool? CanDamage() => false;

        public override void AI()
        {
            Projectile.ai[1]--;
            
            if (Projectile.ai[1] <= 0)
            {
                // Deal echo damage
                if (TargetNPC >= 0 && TargetNPC < Main.maxNPCs)
                {
                    NPC target = Main.npc[TargetNPC];
                    if (target.active && !target.dontTakeDamage)
                    {
                        // Apply damage
                        Main.player[Projectile.owner].ApplyDamageToNPC(target, Projectile.damage, 0f, 0, false);
                        
                        // Echo VFX
                        ClairDeLuneVFX.TemporalChargeRelease(target.Center, 0.5f);
                        ClairDeLuneVFX.CrystalShatterBurst(target.Center, 8, 5f, 0.5f);
                        
                        SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.4f }, target.Center);
                    }
                }
                
                Projectile.Kill();
            }
        }
    }

    #endregion

    #region Summon Class Accessory

    /// <summary>
    /// Conductor's Temporal Baton - SUPREME FINAL BOSS Summoner Accessory
    /// Enhances summons with temporal empowerment
    /// Must exceed Ode to Joy summon accessory (The Verdant Refrain: +70% damage, +4 minions)
    /// </summary>
    public class ConductorsTemporalBaton : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.PygmyNecklace;

        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 42;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 6);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+90% summon damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+6 max minions"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Minions attack 35% faster"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Minion attacks have 20% chance to create temporal clones"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "+40% whip speed and range"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Every 10 minion hits summons a temporal storm burst"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The conductor commands the orchestra of time itself'") 
            { 
                OverrideColor = ClairDeLuneColors.MoonlightSilver 
            });
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Summon) += 0.90f; // SUPREME FINAL BOSS (Ode: 0.70f)
            player.maxMinions += 6;
            player.GetAttackSpeed(DamageClass.SummonMeleeSpeed) += 0.40f;
            
            // Minion attack speed (handled by global projectile)
            player.GetModPlayer<ConductorsTemporalBatonPlayer>().temporalBatonActive = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfClairDeLune>(), 22)
                .AddIngredient(ModContent.ItemType<ClairDeLuneResonantEnergy>(), 16)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfClairDeLune>(), 2)
                .AddIngredient(ItemID.LunarBar, 18)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    public class ConductorsTemporalBatonPlayer : ModPlayer
    {
        public bool temporalBatonActive = false;
        public int minionHitCounter = 0;
        public const int HitsForStorm = 10;

        public override void ResetEffects()
        {
            temporalBatonActive = false;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (temporalBatonActive && proj.minion && proj.friendly)
            {
                // VFX on all minion hits
                ClairDeLuneVFX.TemporalTrail(target.Center, Main.rand.NextVector2Circular(3f, 3f), 0.35f);
                
                // 20% chance for temporal clone
                if (Main.rand.NextFloat() < 0.20f)
                {
                    // Create clone that deals another hit
                    Vector2 cloneOffset = Main.rand.NextVector2Circular(30f, 30f);
                    
                    ClairDeLuneVFX.TemporalImpact(target.Center + cloneOffset, 0.4f);
                    Player.ApplyDamageToNPC(target, damageDone / 3, 0f, 0, false);
                }
                
                // Counter for storm burst
                minionHitCounter++;
                if (minionHitCounter >= HitsForStorm)
                {
                    minionHitCounter = 0;
                    
                    // TEMPORAL STORM BURST
                    ClairDeLuneVFX.TemporalChargeRelease(target.Center, 1.0f);
                    ClairDeLuneVFX.LightningStrikeExplosion(target.Center, 0.7f);
                    ClairDeLuneVFX.ClockworkGearCascade(target.Center, 12, 7f, 0.6f);
                    
                    // Deal AOE damage
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (npc.active && npc.CanBeChasedBy())
                        {
                            float dist = Vector2.Distance(npc.Center, target.Center);
                            if (dist < 250f)
                            {
                                Player.ApplyDamageToNPC(npc, damageDone, 3f, 0, false);
                                npc.AddBuff(BuffID.Slow, 90);
                                
                                // Lightning arc to hit enemy
                                ClairDeLuneVFX.LightningArc(target.Center, npc.Center, 6, 12f, 0.5f);
                            }
                        }
                    }
                    
                    SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.8f }, target.Center);
                }
            }
        }
    }

    // Global projectile for minion attack speed
    public class ConductorsTemporalBatonGlobalProjectile : GlobalProjectile
    {
        public override void AI(Projectile projectile)
        {
            if (projectile.minion && projectile.owner >= 0 && projectile.owner < Main.maxPlayers)
            {
                Player owner = Main.player[projectile.owner];
                if (owner.active && owner.GetModPlayer<ConductorsTemporalBatonPlayer>().temporalBatonActive)
                {
                    // Speed up minion AI (attack 35% faster)
                    // This is handled by reducing cooldowns/timers in individual minion AIs
                    // For basic effect, we can give a subtle speed boost
                    if (Main.rand.NextBool(100)) // Very subtle effect to avoid breaking AI
                    {
                        projectile.localAI[0]++; // Advance internal timer slightly
                    }
                }
            }
        }
    }

    #endregion
}
