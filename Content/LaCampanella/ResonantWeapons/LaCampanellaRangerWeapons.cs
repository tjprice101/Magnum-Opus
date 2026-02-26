using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Content.LaCampanella.Debuffs;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons
{
    #region Piercing Bell's Resonance (Gun)
    
    /// <summary>
    /// Piercing Bell's Resonance - Ultra rapid-fire gun with bell chime tempo.
    /// Fires blazing musical bullets at extreme speeds in the tempo of a bell's chime.
    /// Special: Scorching Staccato - sustained fire accelerates to insane speeds, 
    /// bullets leave trails of music notes that detonate on hit.
    /// Bell-ringing explosions cascade to nearby enemies until trigger released.
    /// Secondary: Every 20th bullet fires a massive RESONANT BELL BLAST that 
    /// spawns homing music note projectiles.
    /// </summary>
    public class PiercingBellsResonance : ModItem
    {
        private int rapidFireCounter = 0;
        private float fireRateBonus = 0f;
        private int resonantBlastCounter = 0;
        
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 165;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 60;
            Item.height = 30;
            Item.useTime = 4; // ULTRA FAST - was 12
            Item.useAnimation = 4;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 2.5f;
            Item.value = Item.sellPrice(gold: 50);
            Item.rare = ModContent.RarityType<LaCampanellaRainbowRarity>();
            Item.UseSound = SoundID.Item11;
            Item.autoReuse = true;
            Item.shoot = ProjectileID.Bullet;
            Item.shootSpeed = 20f; // Faster bullets
            Item.useAmmo = AmmoID.Bullet;
            Item.noMelee = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Ultra rapid-fire gun that accelerates with sustained fire"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Every 20th shot fires a resonant bell blast with homing music notes"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Bullets leave trails of detonating music notes"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The bell's echo shatters through a storm of blazing staccato'") { OverrideColor = new Color(255, 140, 40) });
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            // Convert to fire bullet
            type = ModContent.ProjectileType<BellFireBullet>();
            
            // Increase fire rate with sustained fire (Scorching Staccato) - MORE AGGRESSIVE
            rapidFireCounter++;
            fireRateBonus = Math.Min(rapidFireCounter * 0.02f, 0.6f); // Max 60% faster (was 40%)
        }

        public override float UseSpeedMultiplier(Player player)
        {
            return 1f + fireRateBonus;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Fire main bullet
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            
            // Track for Resonant Bell Blast
            resonantBlastCounter++;
            
            // === RESONANT BELL BLAST - Every 20th shot! ===
            if (resonantBlastCounter >= 20)
            {
                resonantBlastCounter = 0;
                TriggerResonantBellBlast(player, source, position, velocity, damage, knockback);
            }
            
            // Spawn trailing music notes on every 5th bullet
            if (rapidFireCounter % 5 == 0)
            {
                Projectile.NewProjectile(source, position, velocity * 0.7f,
                    ModContent.ProjectileType<TrailingMusicNote>(), damage / 3, knockback * 0.3f, player.whoAmI);
            }
            
            // === BELL SOUND ===
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.2f + fireRateBonus * 0.5f + Main.rand.NextFloat(-0.1f, 0.1f), Volume = 0.35f }, position);
            
            return false;
        }
        
        private void TriggerResonantBellBlast(Player player, IEntitySource source, Vector2 position, Vector2 velocity, int damage, float knockback)
        {
            // === RESONANT BELL BLAST - Massive special shot! ===
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f, Volume = 0.7f }, position);
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = -0.3f, Volume = 0.4f }, position);
            
            // Fire a massive bullet
            int proj = Projectile.NewProjectile(source, position, velocity * 1.5f,
                ModContent.ProjectileType<ResonantBellBlast>(), damage * 3, knockback * 2f, player.whoAmI);
            
            // Spawn homing music notes
            for (int i = 0; i < 5; i++)
            {
                float angle = (i - 2) * 0.3f;
                Vector2 noteVel = velocity.RotatedBy(angle) * 0.6f;
                Projectile.NewProjectile(source, position, noteVel,
                    ModContent.ProjectileType<HomingMusicNote>(), damage / 2, knockback * 0.5f, player.whoAmI);
            }
            
            // === SEEKING INFERNAL CRYSTALS - La Campanella Fire Crystals ===
            SeekingCrystalHelper.SpawnLaCampanellaCrystals(
                source, position, velocity * 0.5f, (int)(damage * 0.45f), knockback, player.whoAmI, 6);
            
        }

        public override void HoldItem(Player player)
        {
            // Reset rapid fire when not shooting
            if (!player.controlUseItem)
            {
                rapidFireCounter = 0;
                fireRateBonus = 0f;
            }
        }
    }

    /// <summary>
    /// Bell fire bullet that cascades explosions on hit.
    /// Uses the gun sprite scaled down for bullet visual.
    /// </summary>
    public class BellFireBullet : ModProjectile
    {
        // Use the gun weapon as the bullet sprite
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/PiercingBellsResonance";
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.extraUpdates = 1;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);
            
            // === SEEKING CRYSTALS - 25% chance on hit ===
            if (Main.rand.NextBool(4))
            {
                SeekingCrystalHelper.SpawnLaCampanellaCrystals(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    Projectile.velocity,
                    (int)(damageDone * 0.18f),
                    Projectile.knockBack,
                    Projectile.owner,
                    3
                );
            }
            
            // Cascade explosion to nearby enemies
            CascadeExplosion(target.Center);
        }

        private void CascadeExplosion(Vector2 position)
        {
            // === BELL EXPLOSION SOUNDS ===
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = Main.rand.NextFloat(0.3f, 0.6f), Volume = 0.4f }, position);
            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Pitch = 0.5f, Volume = 0.2f }, position);
            
            // Cascade to nearby enemies (only if player is still firing)
            Player owner = Main.player[Projectile.owner];
            if (owner.controlUseItem)
            {
                float cascadeRadius = 100f;
                foreach (NPC npc in Main.npc)
                {
                    if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;
                    if (npc.Center == position) continue;
                    
                    if (Vector2.Distance(position, npc.Center) <= cascadeRadius)
                    {
                        // Small cascade damage
                        npc.SimpleStrikeNPC(Projectile.damage / 3, 0, false, 0f, null, false, 0f, true);
                        npc.GetGlobalNPC<ResonantTollNPC>().AddStacks(npc, 1);
                    }
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return true;
        }
    }
    
    #endregion

    #region Grandiose Chime (Rifle)
    
    /// <summary>
    /// Grandiose Chime - High-power bell-infused beam rifle with EXTREME fire rate.
    /// Fires beams of high-power, bell-infused musical energy at blistering speeds.
    /// Special: Bellfire Barrage - every third shot releases inferno spread of burning music notes.
    /// Secondary: HARMONIC CONVERGENCE - beams leave behind music note mines that 
    /// detonate in chain reactions when enemies approach.
    /// Kills create "Resonant Echoes" - ghostly music notes that seek new targets.
    /// </summary>
    public class GrandioseChime : ModItem
    {
        private int shotCounter = 0;
        private int echoChargeCounter = 0;
        
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 240;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 70;
            Item.height = 30;
            Item.useTime = 6; // ULTRA FAST - was 25
            Item.useAnimation = 6;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(gold: 50);
            Item.rare = ModContent.RarityType<LaCampanellaRainbowRarity>();
            Item.UseSound = SoundID.Item75;
            Item.autoReuse = true;
            Item.shoot = ProjectileID.Bullet;
            Item.shootSpeed = 24f; // Faster projectiles
            Item.useAmmo = AmmoID.Bullet;
            Item.noMelee = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires high-power bell-infused energy beams at extreme speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Every third shot triggers a bellfire barrage of burning music notes"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Leaves behind music note mines that detonate in chain reactions"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Kills create resonant echoes that seek new targets"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Each beam carries the grandeur of a thousand chiming bells'") { OverrideColor = new Color(255, 140, 40) });
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            type = ModContent.ProjectileType<BellEnergyBeam>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            shotCounter++;
            echoChargeCounter++;
            
            // Main beam
            int proj = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            
            // Leave behind music note mines every 4 shots (Harmonic Convergence)
            if (shotCounter % 4 == 0)
            {
                Vector2 minePos = position + velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(60f, 120f);
                Projectile.NewProjectile(source, minePos, Vector2.Zero,
                    ModContent.ProjectileType<MusicNoteMine>(), damage / 2, knockback * 0.3f, player.whoAmI);
            }
            
            // === SOUND EFFECTS - Less frequent for fast fire ===
            if (Main.rand.NextBool(3))
            {
                SoundEngine.PlaySound(SoundID.Item35 with { Pitch = Main.rand.NextFloat(-0.1f, 0.3f), Volume = 0.4f }, position);
            }
            if (Main.rand.NextBool(4))
            {
                SoundEngine.PlaySound(SoundID.Item75 with { Pitch = 0.3f, Volume = 0.2f }, position);
            }
            
            // === MUZZLE EFFECTS ===
            
            // Screen shake
            // REMOVED: Screen shake disabled for La Campanella weapons
            // if (Main.rand.NextBool(3))
            // {
            //     player.GetModPlayer<ScreenShakePlayer>()?.AddShake(1f, 3);
            // }
            
            // Bellfire Barrage on every 3rd shot (was 5th)
            if (shotCounter >= 3)
            {
                shotCounter = 0;
                TriggerBellfireBarrage(player, source, position, velocity, damage, knockback);
            }
            
            return false;
        }

        private void TriggerBellfireBarrage(Player player, IEntitySource source, Vector2 position, Vector2 velocity, int damage, float knockback)
        {
            // === BELLFIRE BARRAGE SOUNDS ===
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.2f, Volume = 0.7f }, position);
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = -0.3f, Volume = 0.4f }, position);
            
            // Fire spread of burning musical notes
            int noteCount = 7;
            float spreadAngle = MathHelper.ToRadians(40f);
            
            for (int i = 0; i < noteCount; i++)
            {
                float angle = (i - noteCount / 2f) * spreadAngle / noteCount;
                Vector2 noteVelocity = velocity.RotatedBy(angle) * 0.8f;
                
                Projectile.NewProjectile(source, position, noteVelocity,
                    ModContent.ProjectileType<BurningMusicalNote>(), (int)(damage * 0.7f), knockback * 0.5f, player.whoAmI);
            }
        }

        public override void HoldItem(Player player)
        {
        }
    }

    /// <summary>
    /// Bell energy beam projectile.
    /// Pure particle-based with glowing trail effect.
    /// </summary>
    public class BellEnergyBeam : ModProjectile
    {
        // Use the rifle weapon as base, drawn with custom glow
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/GrandioseChime";
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 180;
            Projectile.extraUpdates = 2;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);
            
            // === HIT SOUND ===
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = Main.rand.NextFloat(0.3f, 0.7f), Volume = 0.4f }, target.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return true;
        }

        public override void OnKill(int timeLeft)
        {
        }
    }

    /// <summary>
    /// Burning musical note projectile from Bellfire Barrage.
    /// Glowing music note with fire trail.
    /// </summary>
    public class BurningMusicalNote : ModProjectile
    {
        // Use the rifle weapon as base, rendered as glowing note
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/GrandioseChime";
        
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 100;
        }

        public override void AI()
        {
            Projectile.rotation += 0.15f * Math.Sign(Projectile.velocity.X);
            Projectile.velocity *= 0.98f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);
        }

        public override void OnKill(int timeLeft)
        {
        }

        public override bool PreDraw(ref Color lightColor) => true;
    }
    
    #endregion

    #region Symphonic Bellfire Annihilator (Rocket Launcher)
    
    /// <summary>
    /// Symphonic Bellfire Annihilator - Ultimate homing rocket launcher.
    /// Fires 5 HOMING ROCKETS per shot that aggressively seek enemies!
    /// If all 5 rockets hit, spawns 5 MORE HOMING ROCKETS around the target!
    /// If ALL 10 ROCKETS HIT (both volleys), grants BELLFIRE CRESCENDO buff:
    /// +10% movement speed and +10% damage for 30 seconds (cannot stack).
    /// Special: Grand Crescendo - multiple quick detonations trigger screen-wide wave of destruction.
    /// Secondary: INFERNAL SYMPHONY - each rocket has orbiting music notes.
    /// </summary>
    public class SymphonicBellfireAnnihilator : ModItem
    {
        private int recentExplosions = 0;
        private int explosionTimer = 0;
        private const int GrandCrescendoThreshold = 3;
        
        // === VOLLEY TRACKING SYSTEM ===
        private int currentVolleyId = 0; // Unique ID for each volley
        private static Dictionary<int, VolleyTracker> activeVolleys = new Dictionary<int, VolleyTracker>();
        
        public class VolleyTracker
        {
            public int OwnerId;
            public int TotalRockets;
            public int HitCount;
            public bool IsSecondaryVolley;
            public int ParentVolleyId;
            public Vector2 LastHitPosition;
            public int TimeAlive;
            public const int MaxVolleyTime = 300; // 5 seconds max tracking time
            
            public bool AllRocketsHit => HitCount >= TotalRockets;
        }
        
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 494; // Base damage increased 30% for smaller explosions
            Item.DamageType = DamageClass.Ranged;
            Item.width = 70;
            Item.height = 30;
            Item.useTime = 35; // Slower since we fire 5 rockets at once
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(gold: 50);
            Item.rare = ModContent.RarityType<LaCampanellaRainbowRarity>();
            Item.UseSound = SoundID.Item92;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<HomingBellRocket>();
            Item.shootSpeed = 14f;
            Item.useAmmo = AmmoID.Rocket;
            Item.noMelee = true;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var descLine = new TooltipLine(Mod, "Description", 
                "[c/FF6600:Fires 5 HOMING ROCKETS that aggressively seek enemies!]\n" +
                "[c/FFAA00:If all 5 hit ↁESpawns 5 MORE homing rockets around target!]\n" +
                "[c/FFD700:If ALL 10 hit ↁEBELLFIRE CRESCENDO: +10% damage & speed for 30s!]\n" +
                "[c/FF4400:'The symphony of destruction seeks its audience...']");
            tooltips.Add(descLine);
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            type = ModContent.ProjectileType<HomingBellRocket>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Create new volley tracker
            currentVolleyId++;
            var volley = new VolleyTracker
            {
                OwnerId = player.whoAmI,
                TotalRockets = 5,
                HitCount = 0,
                IsSecondaryVolley = false,
                ParentVolleyId = -1,
                LastHitPosition = Vector2.Zero,
                TimeAlive = 0
            };
            activeVolleys[currentVolleyId] = volley;
            
            // === FIRE 5 HOMING ROCKETS IN A SPREAD! ===
            float spreadAngle = MathHelper.ToRadians(35f); // Total spread
            for (int i = 0; i < 5; i++)
            {
                float angleOffset = spreadAngle * ((i - 2f) / 2f); // -35, -17.5, 0, +17.5, +35 degrees
                Vector2 rocketVel = velocity.RotatedBy(angleOffset);
                
                int proj = Projectile.NewProjectile(source, position, rocketVel, type, damage, knockback, player.whoAmI);
                if (Main.projectile[proj].ModProjectile is HomingBellRocket rocket)
                {
                    rocket.SetOwnerWeapon(this);
                    rocket.SetVolleyId(currentVolleyId);
                }
                
                // Spawn orbiting music notes for each rocket
                for (int j = 0; j < 2; j++)
                {
                    float noteAngle = MathHelper.TwoPi * j / 2f;
                    Vector2 noteVel = rocketVel.RotatedBy(noteAngle) * 0.3f;
                    Projectile.NewProjectile(source, position, noteVel,
                        ModContent.ProjectileType<OrbitingMusicNote>(), damage / 4, knockback * 0.2f, player.whoAmI, proj);
                }
            }
            
            // === VOLLEY LAUNCH SOUNDS ===
            SoundEngine.PlaySound(SoundID.Item92 with { Pitch = 0.1f, Volume = 0.6f }, position);
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = -0.4f, Volume = 0.5f }, position);
            
            return false;
        }

        public void OnRocketExplode(Vector2 position)
        {
            recentExplosions++;
            explosionTimer = 60; // 1 second window
            
            if (recentExplosions >= GrandCrescendoThreshold)
            {
                recentExplosions = 0;
                TriggerGrandCrescendo(position);
            }
        }

        private void TriggerGrandCrescendo(Vector2 position)
        {
            Player owner = Main.player[Main.myPlayer];
            
            // === GRAND CRESCENDO SOUNDS ===
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.5f, Volume = 0.9f }, position);
            SoundEngine.PlaySound(SoundID.Item45 with { Pitch = 0f, Volume = 0.6f }, position);
            
            // Screen-wide wave effect
            for (int i = 0; i < 36; i++)
            {
                float angle = MathHelper.TwoPi * i / 36f;
                Vector2 velocity = angle.ToRotationVector2() * 15f;
                
                Projectile.NewProjectile(owner.GetSource_ItemUse(owner.HeldItem), position, velocity,
                    ModContent.ProjectileType<GrandCrescendoWave>(), (int)(Item.damage * 0.5f), 5f, owner.whoAmI);
            }
            
            // Grant movement speed buff
            owner.AddBuff(ModContent.BuffType<GrandCrescendoBuff>(), 300); // 5 seconds
            
            // Destroy nearby enemy projectiles
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.hostile && Vector2.Distance(proj.Center, position) < 600f)
                {
                    proj.Kill();
                }
            }
        }

        public override void HoldItem(Player player)
        {
            // Decay explosion counter
            if (explosionTimer > 0)
            {
                explosionTimer--;
                if (explosionTimer <= 0)
                    recentExplosions = 0;
            }
            
            // === VOLLEY CLEANUP - Remove old volleys ===
            List<int> toRemove = new List<int>();
            foreach (var kvp in activeVolleys)
            {
                kvp.Value.TimeAlive++;
                if (kvp.Value.TimeAlive > VolleyTracker.MaxVolleyTime)
                    toRemove.Add(kvp.Key);
            }
            foreach (int id in toRemove)
                activeVolleys.Remove(id);
        }
        
        /// <summary>
        /// Called when a homing rocket hits an enemy. Tracks volley completion.
        /// </summary>
        public void OnRocketHit(int volleyId, Vector2 hitPosition, int ownerId)
        {
            if (!activeVolleys.TryGetValue(volleyId, out var volley))
                return;
            
            volley.HitCount++;
            volley.LastHitPosition = hitPosition;
            
            // Check if all rockets in this volley have hit
            if (volley.AllRocketsHit)
            {
                if (!volley.IsSecondaryVolley)
                {
                    // === PRIMARY VOLLEY COMPLETE - SPAWN 5 MORE HOMING ROCKETS! ===
                    TriggerSecondaryVolley(hitPosition, ownerId, volleyId);
                }
                else
                {
                    // === SECONDARY VOLLEY COMPLETE - Check if primary also completed! ===
                    // If the parent volley (primary) also completed, grant the buff!
                    if (volley.ParentVolleyId >= 0 && activeVolleys.TryGetValue(volley.ParentVolleyId, out var parentVolley))
                    {
                        if (parentVolley.AllRocketsHit)
                        {
                            // === ALL 10 ROCKETS HIT! GRANT BELLFIRE CRESCENDO! ===
                            TriggerBellfireCrescendo(hitPosition, ownerId);
                        }
                    }
                }
                
                // Don't remove volley yet - we need to track it for the buff check
            }
        }
        
        private void TriggerSecondaryVolley(Vector2 position, int ownerId, int parentVolleyId)
        {
            Player owner = Main.player[ownerId];
            
            // === EPIC SECONDARY VOLLEY SPAWN EFFECT! ===
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 0.8f }, position);
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.3f, Volume = 0.6f }, position);
            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot with { Pitch = 0.4f, Volume = 0.5f }, position);
            
            // Create secondary volley tracker
            currentVolleyId++;
            var secondaryVolley = new VolleyTracker
            {
                OwnerId = ownerId,
                TotalRockets = 5,
                HitCount = 0,
                IsSecondaryVolley = true,
                ParentVolleyId = parentVolleyId,
                LastHitPosition = position,
                TimeAlive = 0
            };
            activeVolleys[currentVolleyId] = secondaryVolley;
            
            // === SPAWN 5 HOMING ROCKETS AROUND THE TARGET! ===
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 spawnOffset = angle.ToRotationVector2() * 80f; // Spawn in a ring around target
                Vector2 spawnPos = position + spawnOffset;
                
                // Rockets home toward center (where enemies likely are)
                Vector2 rocketVel = -spawnOffset.SafeNormalize(Vector2.Zero) * 8f;
                
                int proj = Projectile.NewProjectile(owner.GetSource_FromThis(), spawnPos, rocketVel,
                    ModContent.ProjectileType<HomingBellRocket>(), (int)(Item.damage * 0.8f), Item.knockBack * 0.8f, ownerId);
                if (Main.projectile[proj].ModProjectile is HomingBellRocket rocket)
                {
                    rocket.SetOwnerWeapon(this);
                    rocket.SetVolleyId(currentVolleyId);
                    rocket.IsSecondaryRocket = true;
                }
            }
            
            // Screen shake
            // REMOVED: Screen shake disabled for La Campanella weapons
            // owner.GetModPlayer<ScreenShakePlayer>()?.AddShake(8f, 15);
        }
        
        private void TriggerBellfireCrescendo(Vector2 position, int ownerId)
        {
            Player owner = Main.player[ownerId];
            
            // === BELLFIRE CRESCENDO - ALL 10 ROCKETS HIT! ===
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.3f, Volume = 1f }, position);
            SoundEngine.PlaySound(SoundID.Item45 with { Pitch = 0.2f, Volume = 0.7f }, position);
            
            // === GRANT BELLFIRE CRESCENDO BUFF - 30 SECONDS! ===
            // Only if not already active (cannot stack)
            if (!owner.HasBuff(ModContent.BuffType<BellfireCrescendoBuff>()))
            {
                owner.AddBuff(ModContent.BuffType<BellfireCrescendoBuff>(), 1800); // 30 seconds
            }
        }
    }

    /// <summary>
    /// Bell-shaped rocket projectile.
    /// Blazing rocket with bell-flame trail.
    /// </summary>
    public class BellRocket : ModProjectile
    {
        // Use the launcher weapon as base for bell rocket visual
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/SymphonicBellfireAnnihilator";
        
        private SymphonicBellfireAnnihilator ownerWeapon;
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }

        public void SetOwnerWeapon(SymphonicBellfireAnnihilator weapon)
        {
            ownerWeapon = weapon;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
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
            // === BELL EXPLOSION SOUNDS ===
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = Main.rand.NextFloat(0.1f, 0.4f), Volume = 0.7f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.3f, Volume = 0.4f }, Projectile.Center);
            
            // Harmonic pulse projectiles (pierce walls)
            int pulseCount = 8;
            for (int i = 0; i < pulseCount; i++)
            {
                float angle = MathHelper.TwoPi * i / pulseCount;
                Vector2 velocity = angle.ToRotationVector2() * 10f;
                
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity,
                    ModContent.ProjectileType<HarmonicPulse>(), Projectile.damage / 3, 3f, Projectile.owner);
            }
            
            // AOE damage and stun
            float explosionRadius = 105f; // 30% smaller explosions for more focused damage
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;
                
                if (Vector2.Distance(Projectile.Center, npc.Center) <= explosionRadius)
                {
                    npc.GetGlobalNPC<ResonantTollNPC>().AddStacks(npc, 2);
                    
                    // Brief stun (slow)
                    npc.velocity *= 0.5f;
                }
            }
            
            // Notify weapon for Grand Crescendo
            ownerWeapon?.OnRocketExplode(Projectile.Center);

            Projectile.Kill();
        }
        
        public override bool PreDraw(ref Color lightColor) => true;
    }
    
    /// <summary>
    /// HOMING Bell Rocket - Aggressively seeks enemies!
    /// Part of the 5-rocket volley system. Tracks hits for Bellfire Crescendo buff.
    /// </summary>
    public class HomingBellRocket : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/SymphonicBellfireAnnihilator";
        
        private SymphonicBellfireAnnihilator ownerWeapon;
        private int volleyId = -1;
        private float homingStrength = 0f;
        public bool IsSecondaryRocket = false;
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1; // Single hit for tracking
            Projectile.timeLeft = 240;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }

        public void SetOwnerWeapon(SymphonicBellfireAnnihilator weapon)
        {
            ownerWeapon = weapon;
        }
        
        public void SetVolleyId(int id)
        {
            volleyId = id;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            
            // === AGGRESSIVE HOMING LOGIC ===
            homingStrength = Math.Min(homingStrength + 0.008f, 0.18f); // Ramps up fast
            
            // Find target - prioritize closest enemy
            NPC target = null;
            float closestDist = 800f; // Long range homing
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    target = npc;
                }
            }
            
            // Home toward target aggressively
            if (target != null)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                float targetSpeed = Projectile.velocity.Length();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity.SafeNormalize(Vector2.Zero), toTarget, homingStrength) * targetSpeed;
                
                // Maintain minimum speed
                if (Projectile.velocity.Length() < 10f)
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 10f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 2);
            
            // === NOTIFY WEAPON OF HIT FOR VOLLEY TRACKING ===
            ownerWeapon?.OnRocketHit(volleyId, target.Center, Projectile.owner);
            
            // Sound
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f + (IsSecondaryRocket ? 0.3f : 0f), Volume = 0.6f }, target.Center);
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.1f, Volume = 0.4f }, target.Center);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // Explode on tile collision (missed shot - doesn't count toward volley)
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.3f, Volume = 0.5f }, Projectile.Center);
            return true;
        }

        public override void OnKill(int timeLeft)
        {
        }

        public override bool PreDraw(ref Color lightColor) => true;
    }
    
    /// <summary>
    /// Bellfire Crescendo Buff - Granted when ALL 10 HOMING ROCKETS HIT!
    /// +10% damage and +10% movement speed for 30 seconds. Cannot stack.
    /// </summary>
    public class BellfireCrescendoBuff : ModBuff
    {
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/SymphonicBellfireAnnihilator";
        
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // +10% damage
            player.GetDamage(DamageClass.Generic) += 0.1f;
            
            // +10% movement speed
            player.moveSpeed += 0.1f;
            player.maxRunSpeed += 1f;
        }
    }

    /// <summary>
    /// Harmonic pulse that pierces walls.
    /// Pure particle visual - no texture drawn.
    /// </summary>
    public class HarmonicPulse : ModProjectile
    {
        // Uses weapon texture for loading, drawn entirely as particles
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/SymphonicBellfireAnnihilator";
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 15;
            Projectile.height = 15;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false; // Pierces walls
            Projectile.ignoreWater = true;
            Projectile.alpha = 150;
        }

        public override void AI()
        {
            Projectile.alpha = Math.Max(0, Projectile.alpha - 5);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);
        }

        public override void OnKill(int timeLeft)
        {
        }

        public override bool PreDraw(ref Color lightColor) => true;
    }

    /// <summary>
    /// Grand Crescendo wave projectile.
    /// Pure particle visual - expanding flame wave.
    /// </summary>
    public class GrandCrescendoWave : ModProjectile
    {
        // Uses weapon texture for loading, drawn entirely as particles
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/SymphonicBellfireAnnihilator";
        
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public override void AI()
        {
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);
        }

        public override void OnKill(int timeLeft)
        {
        }

        public override bool PreDraw(ref Color lightColor) => true;
    }

    /// <summary>
    /// Grand Crescendo buff - movement speed boost.
    /// </summary>
    public class GrandCrescendoBuff : ModBuff
    {
        // Use launcher weapon as buff icon
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/SymphonicBellfireAnnihilator";
        
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.moveSpeed += 0.5f;
            player.maxRunSpeed += 4f;
        }
    }
    
    #endregion
    
    #region New Special Projectiles
    
    /// <summary>
    /// Trailing music note that follows bullets and detonates on hit.
    /// </summary>
    public class TrailingMusicNote : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/PiercingBellsResonance";
        
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 50;
        }

        public override void AI()
        {
            Projectile.rotation += 0.2f;
            Projectile.velocity *= 0.98f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);
            
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.5f, Volume = 0.4f }, target.Center);
        }

        public override void OnKill(int timeLeft)
        {
        }

        public override bool PreDraw(ref Color lightColor) => true;
    }
    
    /// <summary>
    /// Resonant Bell Blast - Massive bullet from Piercing Bell's Resonance special.
    /// </summary>
    public class ResonantBellBlast : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/PiercingBellsResonance";
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 180;
            Projectile.extraUpdates = 1;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 3);
            
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f, Volume = 0.6f }, target.Center);
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = -0.2f, Volume = 0.5f }, target.Center);
        }

        public override void OnKill(int timeLeft)
        {
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return true;
        }
    }
    
    /// <summary>
    /// Homing music note that seeks enemies.
    /// </summary>
    public class HomingMusicNote : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/PiercingBellsResonance";
        
        private float homingStrength = 0f;
        
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Projectile.rotation += 0.25f;
            
            // Ramp up homing over time
            homingStrength = Math.Min(homingStrength + 0.02f, 0.15f);
            
            // Find target
            NPC target = null;
            float closestDist = 500f;
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    target = npc;
                }
            }
            
            // Home toward target
            if (target != null)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 12f, homingStrength);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);
            
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.6f, Volume = 0.35f }, target.Center);
        }

        public override void OnKill(int timeLeft)
        {
        }

        public override bool PreDraw(ref Color lightColor) => true;
    }
    
    /// <summary>
    /// Music note mine that detonates when enemies approach.
    /// </summary>
    public class MusicNoteMine : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/GrandioseChime";
        
        private float pulseTimer = 0f;
        
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300; // 5 seconds
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 60;
        }

        public override void AI()
        {
            pulseTimer += 0.1f;
            
            // Gentle floating
            Projectile.velocity.Y = (float)Math.Sin(pulseTimer) * 0.3f;
            Projectile.rotation += 0.05f;
            
            // Check for nearby enemies - detonate if close
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;
                
                if (Vector2.Distance(Projectile.Center, npc.Center) < 80f)
                {
                    Detonate();
                    break;
                }
            }
        }
        
        private void Detonate()
        {
            // Chain reaction - spawn more music notes
            for (int i = 0; i < 3; i++)
            {
                float angle = MathHelper.TwoPi * i / 3f;
                Vector2 vel = angle.ToRotationVector2() * 8f;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vel,
                    ModContent.ProjectileType<HomingMusicNote>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            }
            
            // AOE damage
            float radius = 100f;
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;
                
                if (Vector2.Distance(Projectile.Center, npc.Center) < radius)
                {
                    npc.SimpleStrikeNPC(Projectile.damage, 0, false, 0f, null, false, 0f, true);
                    npc.GetGlobalNPC<ResonantTollNPC>().AddStacks(npc, 2);
                }
            }
            
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.4f, Volume = 0.5f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.2f, Volume = 0.4f }, Projectile.Center);
            
            Projectile.Kill();
        }

        public override void OnKill(int timeLeft)
        {
            if (timeLeft > 0) return; // Already detonated
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return true;
        }
    }
    
    /// <summary>
    /// Orbiting music note that follows a rocket and deals contact damage.
    /// Explodes when the parent rocket dies or when hitting an enemy.
    /// </summary>
    public class OrbitingMusicNote : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/SymphonicBellfireAnnihilator";
        
        private float orbitAngle = 0f;
        private int parentRocketIndex = -1;
        
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            // First frame - store parent rocket index
            if (Projectile.localAI[0] == 0)
            {
                parentRocketIndex = (int)Projectile.ai[0];
                orbitAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Projectile.localAI[0] = 1;
            }
            
            // Check if parent rocket still exists
            if (parentRocketIndex >= 0 && parentRocketIndex < Main.maxProjectiles)
            {
                Projectile parent = Main.projectile[parentRocketIndex];
                if (parent.active && parent.type == ModContent.ProjectileType<BellRocket>())
                {
                    // Orbit around parent
                    orbitAngle += 0.15f;
                    float orbitRadius = 30f;
                    Vector2 targetPos = parent.Center + orbitAngle.ToRotationVector2() * orbitRadius;
                    
                    Projectile.velocity = (targetPos - Projectile.Center) * 0.3f;
                    Projectile.rotation = orbitAngle + MathHelper.PiOver2;
                }
                else
                {
                    // Parent died - explode
                    Explode();
                    return;
                }
            }
            else
            {
                // No parent - seek enemies
                NPC target = null;
                float closestDist = 300f;
                foreach (NPC npc in Main.npc)
                {
                    if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        target = npc;
                    }
                }
                
                if (target != null)
                {
                    Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 10f, 0.1f);
                }
                
                Projectile.rotation += 0.2f;
            }
            
            // === Gameplay logic only (VFX stripped) ===
        }
        
        private void Explode()
        {
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.5f, Volume = 0.35f }, Projectile.Center);
            
            // AOE damage
            float radius = 60f;
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;
                
                if (Vector2.Distance(Projectile.Center, npc.Center) < radius)
                {
                    npc.SimpleStrikeNPC(Projectile.damage, 0, false, 0f, null, false, 0f, true);
                    npc.GetGlobalNPC<ResonantTollNPC>().AddStacks(npc, 1);
                }
            }
            
            Projectile.Kill();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);
        }

        public override void OnKill(int timeLeft)
        {
        }

        public override bool PreDraw(ref Color lightColor) => true;
    }
    
    #endregion
}
