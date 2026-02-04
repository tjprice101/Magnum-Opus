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
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.ClairDeLune.Projectiles;
using MagnumOpus.Content.ClairDeLune.ResonanceEnergies;
using MagnumOpus.Content.ClairDeLune.HarmonicCores;
using MagnumOpus.Content.Fate.CraftingStations;

namespace MagnumOpus.Content.ClairDeLune.Weapons.Summon
{
    #region Lunar Phylactery - Crystal Soul Vessel
    
    /// <summary>
    /// Lunar Phylactery - SUPREME FINAL BOSS summon weapon
    /// Summons a crystalline soul vessel that fires temporal beams
    /// Stores enemy souls to empower attacks - more kills = more damage
    /// MUST USE: LunarPhylacteryMinion.png
    /// MUST EXCEED Ode to Joy summon damage (2600)
    /// </summary>
    public class LunarPhylactery : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.damage = 3100; // FINAL BOSS - 19% above Ode to Joy (2600)
            Item.DamageType = DamageClass.Summon;
            Item.mana = 18;
            Item.useTime = 28;
            Item.useAnimation = 28;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<LunarPhylacteryMinionProjectile>();
            Item.buffType = ModContent.BuffType<LunarPhylacteryBuff>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Summon", "Summons a crystalline soul vessel to fight for you"));
            tooltips.Add(new TooltipLine(Mod, "Behavior", "Fires temporal beams that pierce multiple enemies"));
            tooltips.Add(new TooltipLine(Mod, "Mechanic", "Killing enemies empowers the phylactery (+5% damage per soul, max 50%)"));
            tooltips.Add(new TooltipLine(Mod, "Synergy", "Multiple phylacteries share souls for massive damage"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A vessel for souls lost to time'") 
            { 
                OverrideColor = ClairDeLuneColors.Crystal 
            });
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            
            Vector2 spawnPos = Main.MouseWorld;
            
            // Entrance VFX
            ClairDeLuneVFX.TemporalChargeRelease(spawnPos, 0.8f);
            ClairDeLuneVFX.CrystalShatterBurst(spawnPos, 12, 6f, 0.7f);
            
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 0.9f }, spawnPos);
            
            Projectile.NewProjectile(source, spawnPos, Vector2.Zero, type, damage, knockback, player.whoAmI);
            
            return false;
        }

        public override void HoldItem(Player player)
        {
            if (Main.rand.NextBool(15))
            {
                Vector2 crystalPos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                ClairDeLuneVFX.SpawnCrystalShard(crystalPos, Main.rand.NextVector2Circular(1f, 1f), false, 0.35f);
            }
            
            Lighting.AddLight(player.Center, ClairDeLuneColors.Crystal.ToVector3() * 0.2f);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfClairDeLune>(), 26)
                .AddIngredient(ModContent.ItemType<ClairDeLuneResonantEnergy>(), 20)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfClairDeLune>(), 3)
                .AddIngredient(ItemID.LunarBar, 20)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    /// <summary>
    /// Lunar Phylactery Buff
    /// </summary>
    public class LunarPhylacteryBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_" + BuffID.StardustMinionBleed;

        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<LunarPhylacteryMinionProjectile>()] > 0)
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
    /// Lunar Phylactery Minion - USES LunarPhylacteryMinion.png
    /// </summary>
    public class LunarPhylacteryMinionProjectile : ModProjectile
    {
        // CRITICAL: This texture path points to the PROVIDED PNG
        public override string Texture => "MagnumOpus/Content/ClairDeLune/Weapons/Summon/LunarPhylacteryMinion";

        private int attackTimer = 0;
        private int soulCount = 0;
        private const int MaxSouls = 10;
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
            Projectile.width = 34;
            Projectile.height = 42;
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
            hoverAngle += 0.025f;
            Vector2 idlePosition = owner.Center + new Vector2(0, -90f) + hoverAngle.ToRotationVector2() * 25f;
            
            // Find target
            NPC target = FindClosestNPC(900f);
            
            // Calculate soul bonus - shares souls with other phylacteries
            int totalSouls = GetTotalSouls(owner);
            float damageBonus = 1f + totalSouls * 0.05f; // +5% per soul, max 50% with 10 souls
            
            if (target != null)
            {
                // Move toward engagement range
                Vector2 targetPos = target.Center + new Vector2(0, -120f);
                Vector2 toTarget = targetPos - Projectile.Center;
                
                if (toTarget.Length() > 60f)
                {
                    toTarget = toTarget.SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 14f, 0.08f);
                }
                else
                {
                    Projectile.velocity *= 0.92f;
                }
                
                // Attack
                attackTimer++;
                if (attackTimer >= 45) // Fast attack rate
                {
                    attackTimer = 0;
                    
                    // Fire temporal beam
                    Vector2 attackDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    
                    int proj = Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        attackDir * 18f,
                        ModContent.ProjectileType<PhylacteryBeamProjectile>(),
                        (int)(Projectile.damage * damageBonus),
                        Projectile.knockBack,
                        Projectile.owner
                    );
                    
                    // Pass soul count for special effects
                    if (proj >= 0 && proj < Main.maxProjectiles)
                    {
                        Main.projectile[proj].ai[0] = totalSouls;
                    }
                    
                    SoundEngine.PlaySound(SoundID.Item33 with { Volume = 0.7f, Pitch = 0.3f }, Projectile.Center);
                    ClairDeLuneVFX.SpawnLightningBurst(Projectile.Center, attackDir * 4f, false, 0.5f);
                }
            }
            else
            {
                // Return to player
                Vector2 toIdle = idlePosition - Projectile.Center;
                
                if (toIdle.Length() > 40f)
                {
                    toIdle = toIdle.SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdle * 12f, 0.07f);
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
            
            // Soul visual - more souls = more crystals orbiting
            if (totalSouls > 0)
            {
                float orbitAngle = Main.GameUpdateCount * 0.05f;
                float orbitRadius = 20f + totalSouls * 2f;
                
                for (int i = 0; i < Math.Min(totalSouls, 5); i++)
                {
                    float angle = orbitAngle + MathHelper.TwoPi * i / 5f;
                    Vector2 orbitPos = Projectile.Center + angle.ToRotationVector2() * orbitRadius;
                    
                    if (Main.GameUpdateCount % 3 == 0)
                    {
                        ClairDeLuneVFX.SpawnCrystalShard(orbitPos, Vector2.Zero, false, 0.25f);
                    }
                }
            }
            
            // Ambient particles
            if (Main.rand.NextBool(5))
            {
                var trail = new GenericGlowParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(12f, 12f),
                    Main.rand.NextVector2Circular(0.5f, 0.5f) + Vector2.UnitY * -0.5f,
                    ClairDeLuneColors.Crystal * 0.6f,
                    0.25f,
                    18,
                    true
                );
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            Lighting.AddLight(Projectile.Center, ClairDeLuneColors.Crystal.ToVector3() * (0.4f + totalSouls * 0.02f));
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<LunarPhylacteryBuff>());
                return false;
            }
            
            if (owner.HasBuff(ModContent.BuffType<LunarPhylacteryBuff>()))
            {
                Projectile.timeLeft = 2;
            }
            
            return true;
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

        private int GetTotalSouls(Player owner)
        {
            int total = soulCount;
            
            // Sum souls from all phylacteries
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.owner == owner.whoAmI && 
                    proj.type == Type && proj.whoAmI != Projectile.whoAmI)
                {
                    if (proj.ModProjectile is LunarPhylacteryMinionProjectile phylactery)
                    {
                        total += phylactery.soulCount;
                    }
                }
            }
            
            return Math.Min(total, MaxSouls);
        }

        // Called when our attack kills an enemy
        public void GainSoul()
        {
            if (soulCount < MaxSouls)
            {
                soulCount++;
                ClairDeLuneVFX.TemporalImpact(Projectile.Center, 0.4f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = tex.Size() / 2f;
            
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.05f;
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            
            // Draw glow
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            int totalSouls = GetTotalSouls(Main.player[Projectile.owner]);
            float glowIntensity = 0.3f + totalSouls * 0.03f;
            
            Color glow = ClairDeLuneColors.Crystal * glowIntensity;
            glow.A = 0;
            Main.spriteBatch.Draw(tex, drawPos, null, glow, Projectile.rotation, origin, 1.3f * pulse, effects, 0f);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Draw main sprite
            Main.spriteBatch.Draw(tex, drawPos, null, lightColor, Projectile.rotation, origin, pulse, effects, 0f);
            
            return false;
        }
    }

    /// <summary>
    /// Phylactery Beam - Piercing temporal beam attack
    /// </summary>
    public class PhylacteryBeamProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/LightningStreak";

        private int SoulCount => (int)Projectile.ai[0];

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 80;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 2;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            ClairDeLuneVFX.TemporalTrail(Projectile.Center, Projectile.velocity, 0.5f);
            
            if (SoulCount > 3 && Main.rand.NextBool(3))
            {
                ClairDeLuneVFX.SpawnCrystalShard(Projectile.Center, -Projectile.velocity * 0.2f, false, 0.3f);
            }
            
            Lighting.AddLight(Projectile.Center, ClairDeLuneColors.Crystal.ToVector3() * 0.4f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            ClairDeLuneVFX.CrystalShatterBurst(target.Center, 6, 4f, 0.5f);
            target.AddBuff(BuffID.Frostburn2, 120);
            
            // If kill, try to give soul to phylactery
            if (target.life <= 0)
            {
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (proj.active && proj.owner == Projectile.owner && 
                        proj.type == ModContent.ProjectileType<LunarPhylacteryMinionProjectile>())
                    {
                        if (proj.ModProjectile is LunarPhylacteryMinionProjectile phylactery)
                        {
                            phylactery.GainSoul();
                            break; // Only one phylactery gains the soul
                        }
                    }
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            ClairDeLuneVFX.CrystalShatterBurst(Projectile.Center, 8, 5f, 0.5f);
        }
    }

    #endregion

    #region Gear-Driven Arbiter - Clockwork Guardian
    
    /// <summary>
    /// Gear-Driven Arbiter - SUPREME FINAL BOSS summon weapon
    /// Summons a clockwork construct that judges enemies
    /// Tags enemies with temporal marks, marked enemies take bonus damage from all sources
    /// MUST USE: GearDrivenArbiterMinion.png
    /// </summary>
    public class GearDrivenArbiter : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 44;
            Item.damage = 3400; // FINAL BOSS tier
            Item.DamageType = DamageClass.Summon;
            Item.mana = 22;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<GearDrivenArbiterMinionProjectile>();
            Item.buffType = ModContent.BuffType<GearDrivenArbiterBuff>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Summon", "Summons a clockwork arbiter to judge your foes"));
            tooltips.Add(new TooltipLine(Mod, "Behavior", "Fires gear projectiles that mark enemies"));
            tooltips.Add(new TooltipLine(Mod, "Mechanic", "Marked enemies take +15% damage from all sources"));
            tooltips.Add(new TooltipLine(Mod, "Synergy", "Multiple arbiters spread marks faster"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The gears of justice turn without mercy'") 
            { 
                OverrideColor = ClairDeLuneColors.Brass 
            });
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            
            Vector2 spawnPos = Main.MouseWorld;
            
            // Entrance VFX
            ClairDeLuneVFX.ClockworkGearCascade(spawnPos, 12, 8f, 0.8f);
            ClairDeLuneVFX.LightningStrikeExplosion(spawnPos, 0.6f);
            
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.2f, Volume = 0.9f }, spawnPos);
            
            Projectile.NewProjectile(source, spawnPos, Vector2.Zero, type, damage, knockback, player.whoAmI);
            
            return false;
        }

        public override void HoldItem(Player player)
        {
            if (Main.rand.NextBool(12))
            {
                Vector2 gearPos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                ClairDeLuneVFX.SpawnClockworkGear(gearPos, Main.rand.NextVector2Circular(1f, 1f), false, 0.35f);
            }
            
            Lighting.AddLight(player.Center, ClairDeLuneColors.Brass.ToVector3() * 0.2f);
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
    /// Gear-Driven Arbiter Buff
    /// </summary>
    public class GearDrivenArbiterBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_" + BuffID.StardustGuardianMinion;

        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<GearDrivenArbiterMinionProjectile>()] > 0)
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
    /// Gear-Driven Arbiter Minion - USES GearDrivenArbiterMinion.png
    /// </summary>
    public class GearDrivenArbiterMinionProjectile : ModProjectile
    {
        // CRITICAL: This texture path points to the PROVIDED PNG
        public override string Texture => "MagnumOpus/Content/ClairDeLune/Weapons/Summon/GearDrivenArbiterMinion";

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
            Projectile.height = 48;
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
            hoverAngle += 0.02f;
            Vector2 idlePosition = owner.Center + new Vector2(0, -100f) + hoverAngle.ToRotationVector2() * 30f;
            
            // Find target - prioritize unmarked enemies
            NPC target = FindTarget(850f);
            
            if (target != null)
            {
                // Move toward engagement range
                Vector2 targetPos = target.Center + new Vector2(0, -80f);
                Vector2 toTarget = targetPos - Projectile.Center;
                
                if (toTarget.Length() > 70f)
                {
                    toTarget = toTarget.SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 13f, 0.07f);
                }
                else
                {
                    Projectile.velocity *= 0.9f;
                }
                
                // Attack
                attackTimer++;
                if (attackTimer >= 50)
                {
                    attackTimer = 0;
                    
                    // Fire gear judgment
                    Vector2 attackDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        attackDir * 16f,
                        ModContent.ProjectileType<ArbiterGearProjectile>(),
                        Projectile.damage,
                        Projectile.knockBack,
                        Projectile.owner
                    );
                    
                    SoundEngine.PlaySound(SoundID.Item37 with { Volume = 0.7f, Pitch = 0.1f }, Projectile.Center);
                    ClairDeLuneVFX.ClockworkGearCascade(Projectile.Center, 6, 4f, 0.5f);
                }
            }
            else
            {
                // Return to player
                Vector2 toIdle = idlePosition - Projectile.Center;
                
                if (toIdle.Length() > 50f)
                {
                    toIdle = toIdle.SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdle * 11f, 0.06f);
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
            
            // Orbiting gears visual
            ClairDeLuneVFX.OrbitingGears(Projectile.Center, 25f, 3, Main.GameUpdateCount * 0.04f, 0.35f);
            
            // Ambient particles
            if (Main.rand.NextBool(6))
            {
                var trail = new GenericGlowParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(15f, 15f),
                    Main.rand.NextVector2Circular(0.8f, 0.8f),
                    ClairDeLuneColors.Brass * 0.5f,
                    0.22f,
                    15,
                    true
                );
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            Lighting.AddLight(Projectile.Center, ClairDeLuneColors.Brass.ToVector3() * 0.45f);
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<GearDrivenArbiterBuff>());
                return false;
            }
            
            if (owner.HasBuff(ModContent.BuffType<GearDrivenArbiterBuff>()))
            {
                Projectile.timeLeft = 2;
            }
            
            return true;
        }

        private NPC FindTarget(float range)
        {
            NPC closest = null;
            NPC closestUnmarked = null;
            float closestDist = range;
            float closestUnmarkedDist = range;
            
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
                    
                    // Prioritize unmarked
                    if (!npc.HasBuff(ModContent.BuffType<TemporalJudgmentDebuff>()) && dist < closestUnmarkedDist)
                    {
                        closestUnmarkedDist = dist;
                        closestUnmarked = npc;
                    }
                }
            }
            
            return closestUnmarked ?? closest;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = tex.Size() / 2f;
            
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            
            // Draw glow
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Color glow = ClairDeLuneColors.Brass * 0.35f;
            glow.A = 0;
            Main.spriteBatch.Draw(tex, drawPos, null, glow, Projectile.rotation, origin, 1.25f, effects, 0f);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Draw main sprite
            Main.spriteBatch.Draw(tex, drawPos, null, lightColor, Projectile.rotation, origin, 1f, effects, 0f);
            
            return false;
        }
    }

    /// <summary>
    /// Arbiter Gear - Marks enemies with temporal judgment
    /// </summary>
    public class ArbiterGearProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/ClockworkGearSmall";

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            Projectile.rotation += 0.2f;
            
            // Slight homing
            NPC target = FindClosestNPC(400f);
            if (target != null && Projectile.timeLeft < 70)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 16f, 0.04f);
            }
            
            if (Main.rand.NextBool(3))
            {
                var trail = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.05f,
                    ClairDeLuneColors.Brass * 0.5f, 0.2f, 10, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            Lighting.AddLight(Projectile.Center, ClairDeLuneColors.Brass.ToVector3() * 0.3f);
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
            ClairDeLuneVFX.ClockworkGearCascade(target.Center, 6, 5f, 0.5f);
            
            // Apply temporal judgment mark
            target.AddBuff(ModContent.BuffType<TemporalJudgmentDebuff>(), 300);
        }

        public override void OnKill(int timeLeft)
        {
            ClairDeLuneVFX.ClockworkGearCascade(Projectile.Center, 8, 6f, 0.5f);
        }
    }

    /// <summary>
    /// Temporal Judgment Debuff - Marked enemies take +15% damage
    /// </summary>
    public class TemporalJudgmentDebuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_" + BuffID.Slow;

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }

    // GlobalNPC to apply the damage bonus
    public class TemporalJudgmentGlobalNPC : GlobalNPC
    {
        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            if (npc.HasBuff(ModContent.BuffType<TemporalJudgmentDebuff>()))
            {
                modifiers.FinalDamage *= 1.15f; // +15% damage from all sources
            }
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (npc.HasBuff(ModContent.BuffType<TemporalJudgmentDebuff>()))
            {
                // Gear mark visual
                if (Main.rand.NextBool(5))
                {
                    ClairDeLuneVFX.SpawnClockworkGear(npc.Center + Main.rand.NextVector2Circular(npc.width / 2, npc.height / 2),
                        Main.rand.NextVector2Circular(1f, 1f), false, 0.3f);
                }
                
                // Tint slightly
                drawColor = Color.Lerp(drawColor, ClairDeLuneColors.Brass, 0.2f);
            }
        }
    }

    #endregion

    #region Automaton's Tuning Fork - Resonance Construct
    
    /// <summary>
    /// Automaton's Tuning Fork - SUPREME FINAL BOSS summon weapon
    /// Summons a resonating automaton that pulses with temporal energy
    /// Creates resonance fields that amplify all nearby summon damage
    /// MUST USE: AutomatonsTuningForkMinion.png
    /// </summary>
    public class AutomatonsTuningFork : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.damage = 3000; // FINAL BOSS tier - balanced for support role
            Item.DamageType = DamageClass.Summon;
            Item.mana = 20;
            Item.useTime = 32;
            Item.useAnimation = 32;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<AutomatonsTuningForkMinionProjectile>();
            Item.buffType = ModContent.BuffType<AutomatonsTuningForkBuff>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Summon", "Summons a resonating automaton to harmonize your summons"));
            tooltips.Add(new TooltipLine(Mod, "Behavior", "Pulses with temporal energy, damaging nearby enemies"));
            tooltips.Add(new TooltipLine(Mod, "Mechanic", "Creates a resonance field - all summons within deal +25% damage"));
            tooltips.Add(new TooltipLine(Mod, "Synergy", "Multiple automatons stack resonance up to +50% bonus"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The perfect pitch of destruction'") 
            { 
                OverrideColor = ClairDeLuneColors.MoonlightSilver 
            });
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            
            Vector2 spawnPos = Main.MouseWorld;
            
            // Entrance VFX - resonance burst
            ClairDeLuneVFX.TemporalChargeRelease(spawnPos, 0.9f);
            
            // Resonance ring
            for (int i = 0; i < 3; i++)
            {
                CustomParticles.HaloRing(spawnPos, ClairDeLuneColors.MoonlightSilver, 0.4f + i * 0.15f, 15 + i * 5);
            }
            
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.5f, Volume = 0.9f }, spawnPos);
            
            Projectile.NewProjectile(source, spawnPos, Vector2.Zero, type, damage, knockback, player.whoAmI);
            
            return false;
        }

        public override void HoldItem(Player player)
        {
            if (Main.rand.NextBool(18))
            {
                Vector2 ringPos = player.Center;
                CustomParticles.HaloRing(ringPos, ClairDeLuneColors.MoonlightSilver * 0.4f, 0.2f, 12);
            }
            
            Lighting.AddLight(player.Center, ClairDeLuneColors.MoonlightSilver.ToVector3() * 0.2f);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfClairDeLune>(), 24)
                .AddIngredient(ModContent.ItemType<ClairDeLuneResonantEnergy>(), 18)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfClairDeLune>(), 3)
                .AddIngredient(ItemID.LunarBar, 18)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    /// <summary>
    /// Automaton's Tuning Fork Buff
    /// </summary>
    public class AutomatonsTuningForkBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_" + BuffID.HornetMinion;

        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<AutomatonsTuningForkMinionProjectile>()] > 0)
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
    /// Automaton's Tuning Fork Minion - USES AutomatonsTuningForkMinion.png
    /// </summary>
    public class AutomatonsTuningForkMinionProjectile : ModProjectile
    {
        // CRITICAL: This texture path points to the PROVIDED PNG
        public override string Texture => "MagnumOpus/Content/ClairDeLune/Weapons/Summon/AutomatonsTuningForkMinion";

        private int pulseTimer = 0;
        private const int PulseInterval = 90;
        private const float ResonanceRadius = 300f;
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
            Projectile.width = 36;
            Projectile.height = 44;
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
            
            // Hover near player
            hoverAngle += 0.018f;
            Vector2 idlePosition = owner.Center + new Vector2(-60f, -70f) + hoverAngle.ToRotationVector2() * 15f;
            
            // Always stay somewhat close to player (support role)
            Vector2 toIdle = idlePosition - Projectile.Center;
            
            if (toIdle.Length() > 100f)
            {
                toIdle = toIdle.SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdle * 10f, 0.06f);
            }
            else if (toIdle.Length() > 30f)
            {
                toIdle = toIdle.SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdle * 6f, 0.05f);
            }
            else
            {
                Projectile.velocity *= 0.9f;
            }
            
            // Resonance pulse
            pulseTimer++;
            if (pulseTimer >= PulseInterval)
            {
                pulseTimer = 0;
                
                // Visual pulse
                for (int ring = 0; ring < 4; ring++)
                {
                    Color ringColor = Color.Lerp(ClairDeLuneColors.MoonlightSilver, ClairDeLuneColors.ElectricBlue, ring / 4f);
                    CustomParticles.HaloRing(Projectile.Center, ringColor, 0.3f + ring * 0.2f, 20 + ring * 5);
                }
                
                ClairDeLuneVFX.TemporalChargeRelease(Projectile.Center, 0.5f);
                SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.6f, Volume = 0.5f }, Projectile.Center);
                
                // Damage nearby enemies
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && !npc.dontTakeDamage)
                    {
                        float dist = Vector2.Distance(Projectile.Center, npc.Center);
                        if (dist < ResonanceRadius && Main.myPlayer == Projectile.owner)
                        {
                            npc.SimpleStrikeNPC(Projectile.damage / 2, 0, false, 0f);
                            ClairDeLuneVFX.TemporalImpact(npc.Center, 0.3f);
                        }
                    }
                }
            }
            
            // Face movement direction
            if (Math.Abs(Projectile.velocity.X) > 0.5f)
            {
                Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;
            }
            
            // Resonance field visual
            float resonanceProgress = pulseTimer / (float)PulseInterval;
            float fieldAlpha = 0.1f + resonanceProgress * 0.1f;
            
            if (Main.GameUpdateCount % 5 == 0)
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = Main.rand.NextFloat(ResonanceRadius * 0.8f, ResonanceRadius);
                Vector2 fieldPos = Projectile.Center + angle.ToRotationVector2() * dist;
                
                var fieldParticle = new GenericGlowParticle(fieldPos,
                    (Projectile.Center - fieldPos).SafeNormalize(Vector2.Zero) * 0.5f,
                    ClairDeLuneColors.MoonlightSilver * fieldAlpha, 0.2f, 20, true);
                MagnumParticleHandler.SpawnParticle(fieldParticle);
            }
            
            // Ambient particles
            if (Main.rand.NextBool(4))
            {
                var trail = new GenericGlowParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    Main.rand.NextVector2Circular(0.5f, 0.5f),
                    ClairDeLuneColors.MoonlightSilver * 0.5f,
                    0.2f,
                    15,
                    true
                );
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            Lighting.AddLight(Projectile.Center, ClairDeLuneColors.MoonlightSilver.ToVector3() * 0.5f);
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<AutomatonsTuningForkBuff>());
                return false;
            }
            
            if (owner.HasBuff(ModContent.BuffType<AutomatonsTuningForkBuff>()))
            {
                Projectile.timeLeft = 2;
            }
            
            return true;
        }

        // Get resonance bonus for other minions
        public static float GetResonanceBonus(Player player)
        {
            int automatonCount = 0;
            
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.owner == player.whoAmI && 
                    proj.type == ModContent.ProjectileType<AutomatonsTuningForkMinionProjectile>())
                {
                    automatonCount++;
                }
            }
            
            // +25% per automaton, max +50%
            return Math.Min(automatonCount * 0.25f, 0.5f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = tex.Size() / 2f;
            
            float resonanceProgress = pulseTimer / (float)PulseInterval;
            float pulse = 1f + resonanceProgress * 0.15f;
            
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            
            // Draw glow
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Color glow = ClairDeLuneColors.MoonlightSilver * (0.3f + resonanceProgress * 0.2f);
            glow.A = 0;
            Main.spriteBatch.Draw(tex, drawPos, null, glow, Projectile.rotation, origin, 1.3f * pulse, effects, 0f);
            
            // Resonance rings
            if (resonanceProgress > 0.5f)
            {
                float ringAlpha = (resonanceProgress - 0.5f) * 2f * 0.3f;
                Color ringGlow = ClairDeLuneColors.ElectricBlue * ringAlpha;
                ringGlow.A = 0;
                Main.spriteBatch.Draw(tex, drawPos, null, ringGlow, Projectile.rotation, origin, 1.5f * pulse, effects, 0f);
            }
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Draw main sprite
            Main.spriteBatch.Draw(tex, drawPos, null, lightColor, Projectile.rotation, origin, pulse, effects, 0f);
            
            return false;
        }
    }

    // GlobalProjectile to apply resonance bonus to summon damage
    public class ResonanceFieldGlobalProjectile : GlobalProjectile
    {
        public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
        {
            // Only affects summon projectiles
            if (projectile.minion || projectile.DamageType == DamageClass.Summon)
            {
                Player owner = Main.player[projectile.owner];
                
                // Check if within resonance field of any automaton
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile automaton = Main.projectile[i];
                    if (automaton.active && automaton.owner == projectile.owner && 
                        automaton.type == ModContent.ProjectileType<AutomatonsTuningForkMinionProjectile>())
                    {
                        float dist = Vector2.Distance(projectile.Center, automaton.Center);
                        if (dist < 300f) // Resonance radius
                        {
                            // Apply resonance bonus
                            float bonus = AutomatonsTuningForkMinionProjectile.GetResonanceBonus(owner);
                            modifiers.FinalDamage *= 1f + bonus;
                            
                            // Visual feedback
                            if (Main.rand.NextBool(3))
                            {
                                ClairDeLuneVFX.SpawnLightningBurst(projectile.Center, Main.rand.NextVector2Circular(2f, 2f), false, 0.25f);
                            }
                            
                            break; // Only apply once
                        }
                    }
                }
            }
        }
    }

    #endregion
}
