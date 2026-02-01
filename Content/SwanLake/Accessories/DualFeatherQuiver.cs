using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Audio;
using Terraria.GameContent;
using MagnumOpus.Common;
using MagnumOpus.Content.SwanLake.ResonantOres;
using MagnumOpus.Content.SwanLake.ResonanceEnergies;
using MagnumOpus.Content.SwanLake.HarmonicCores;
using MagnumOpus.Content.SwanLake.Debuffs;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.SwanLake.Accessories
{
    /// <summary>
    /// Dual Feather Quiver - Ranger Accessory
    /// A mystical quiver crafted from the feathers of twin swans.
    /// Works with both arrows AND bullets.
    /// 
    /// WHITE MODE (Grace of Odette):
    /// - Every 5th projectile spawns a homing swan feather that seeks enemies
    /// - +10% ranged crit chance
    /// - ALL hits have 50% chance to drop a healing feather (restores 5 HP)
    /// 
    /// BLACK MODE (Pierce &amp; Destruction):
    /// - +2 projectile pierce
    /// - Applies Flame of the Swan (10% damage vulnerability, 4s)
    /// - +18% ranged damage
    /// 
    /// Right-click while in inventory to toggle modes.
    /// </summary>
    public class DualFeatherQuiver : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 3);
            Item.rare = ModContent.RarityType<SwanRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<SwanLakeAccessoryPlayer>();
            modPlayer.hasDualFeatherQuiver = true;

            // Mode-specific stat bonuses
            if (modPlayer.quiverIsBlackMode)
            {
                // Black mode: +18% ranged damage and +2 pierce
                player.GetDamage(DamageClass.Ranged) += 0.18f;
                player.GetModPlayer<DualFeatherQuiverProjectileBonus>().extraPierce = 2;
            }
            else
            {
                // White mode: +10% ranged crit chance
                player.GetCritChance(DamageClass.Ranged) += 10;
            }

            // Ambient particles based on mode
            if (!hideVisual)
            {
                // Feather particles (both modes)
                if (Main.rand.NextBool(12))
                {
                    Color featherColor = modPlayer.quiverIsBlackMode 
                        ? new Color(40, 35, 50) 
                        : new Color(250, 248, 255);
                    
                    Vector2 offset = new Vector2(player.direction * -15f, -20f) + Main.rand.NextVector2Circular(5f, 5f);
                    Dust feather = Dust.NewDustPerfect(player.Center + offset, DustID.TintableDustLighted,
                        new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(1f, 2f)), 0, featherColor, 0.8f);
                    feather.noGravity = false;
                    feather.velocity *= 0.5f;
                }
                
                // Swan feather from quiver
                if (Main.rand.NextBool(15))
                {
                    Color swanFeatherColor = modPlayer.quiverIsBlackMode ? new Color(30, 30, 35) : Color.White;
                    CustomParticles.SwanFeatherDrift(player.Center + new Vector2(player.direction * -15f, -20f), swanFeatherColor, 0.2f);
                }

                // Pearlescent shimmer
                if (Main.rand.NextBool(10))
                {
                    Color pearlescent = Main.rand.Next(3) switch
                    {
                        0 => new Color(255, 240, 245),
                        1 => new Color(240, 245, 255),
                        _ => new Color(250, 255, 245)
                    };
                    Vector2 offset = new Vector2(player.direction * -12f, -18f) + Main.rand.NextVector2Circular(8f, 12f);
                    Dust shimmer = Dust.NewDustPerfect(player.Center + offset, DustID.TintableDustLighted,
                        Vector2.Zero, 0, pearlescent, 0.6f);
                    shimmer.noGravity = true;
                }

                // Mode-specific flame particles
                if (Main.rand.NextBool(8))
                {
                    Vector2 offset = new Vector2(player.direction * -14f, -22f);
                    if (modPlayer.quiverIsBlackMode)
                    {
                        Dust black = Dust.NewDustPerfect(player.Center + offset, DustID.Smoke,
                            new Vector2(0, -1f), 180, Color.Black, 0.9f);
                        black.noGravity = true;
                    }
                    else
                    {
                        Dust white = Dust.NewDustPerfect(player.Center + offset, DustID.WhiteTorch,
                            new Vector2(0, -0.8f), 100, default, 0.8f);
                        white.noGravity = true;
                    }
                }
            }
        }

        public override bool CanRightClick()
        {
            return true;
        }

        public override void RightClick(Player player)
        {
            var modPlayer = player.GetModPlayer<SwanLakeAccessoryPlayer>();
            modPlayer.ToggleQuiverMode();
            Item.stack++;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var modPlayer = player.GetModPlayer<SwanLakeAccessoryPlayer>();
            
            string modeText = modPlayer.quiverIsBlackMode ? "BLACK SWAN (Pierce)" : "WHITE SWAN (Grace)";
            Color modeColor = modPlayer.quiverIsBlackMode ? new Color(30, 30, 40) : new Color(240, 245, 255);
            
            tooltips.Add(new TooltipLine(Mod, "CurrentMode", $"Current Mode: {modeText}")
            {
                OverrideColor = modeColor
            });
            
            tooltips.Add(new TooltipLine(Mod, "ToggleHint", "[Right-click to toggle mode]")
            {
                OverrideColor = new Color(180, 180, 200)
            });
            
            tooltips.Add(new TooltipLine(Mod, "AmmoType", "Works with both arrows and bullets")
            {
                OverrideColor = new Color(255, 220, 150)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer1", " "));
            
            // White mode tooltip
            Color whiteColor = modPlayer.quiverIsBlackMode ? new Color(100, 100, 110) : new Color(240, 245, 255);
            tooltips.Add(new TooltipLine(Mod, "WhiteHeader", "White Swan (Grace of Odette):")
            {
                OverrideColor = whiteColor
            });
            tooltips.Add(new TooltipLine(Mod, "WhiteEffect1", "  Every 5th shot spawns a homing swan feather")
            {
                OverrideColor = whiteColor
            });
            tooltips.Add(new TooltipLine(Mod, "WhiteEffect2", "  +10% ranged critical strike chance")
            {
                OverrideColor = whiteColor
            });
            tooltips.Add(new TooltipLine(Mod, "WhiteEffect3", "  All hits have 50% chance to drop healing feather")
            {
                OverrideColor = whiteColor
            });
            tooltips.Add(new TooltipLine(Mod, "WhiteEffect4", "  (Restores 5 HP when collected)")
            {
                OverrideColor = whiteColor
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer2", " "));
            
            // Black mode tooltip
            Color blackColor = modPlayer.quiverIsBlackMode ? new Color(200, 180, 220) : new Color(80, 80, 90);
            tooltips.Add(new TooltipLine(Mod, "BlackHeader", "Black Swan (Pierce & Destruction):")
            {
                OverrideColor = blackColor
            });
            tooltips.Add(new TooltipLine(Mod, "BlackEffect1", "  +2 projectile pierce")
            {
                OverrideColor = blackColor
            });
            tooltips.Add(new TooltipLine(Mod, "BlackEffect2", "  Applies Flame of the Swan (4s)")
            {
                OverrideColor = blackColor
            });
            tooltips.Add(new TooltipLine(Mod, "BlackEffect3", "  Enemies take 10% more damage")
            {
                OverrideColor = new Color(255, 180, 180)
            });
            tooltips.Add(new TooltipLine(Mod, "BlackEffect4", "  +18% ranged damage")
            {
                OverrideColor = blackColor
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer3", " "));
            
            tooltips.Add(new TooltipLine(Mod, "Flavor", "'Each feather remembers the flight - one toward the light, one into darkness'")
            {
                OverrideColor = new Color(150, 140, 170)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SwansResonanceEnergy>(), 5)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfSwanLake>(), 5)
                .AddIngredient(ModContent.ItemType<RemnantOfSwansHarmony>(), 5)
                .AddIngredient(ModContent.ItemType<ShardOfTheFeatheredTempo>(), 5)
                .AddIngredient(ItemID.SoulofSight, 5)
                .AddIngredient(ItemID.SoulofFlight, 8)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }

    /// <summary>
    /// Helper ModPlayer to handle projectile modifications for the Dual Feather Quiver.
    /// Also tracks shot count for spawning homing feathers every 5 shots.
    /// </summary>
    public class DualFeatherQuiverProjectileBonus : ModPlayer
    {
        public int extraPierce = 0;
        public int whiteModeShots = 0; // Shot counter for homing feather spawning
        private const int ShotsPerHomingFeather = 5;

        public override void ResetEffects()
        {
            extraPierce = 0;
        }

        public override void ModifyShootStats(Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            // White mode: Spawn homing feather every 5 shots
            var swanPlayer = Player.GetModPlayer<SwanLakeAccessoryPlayer>();
            if (swanPlayer.hasDualFeatherQuiver && !swanPlayer.quiverIsBlackMode && item.DamageType == DamageClass.Ranged)
            {
                whiteModeShots++;
                
                if (whiteModeShots >= ShotsPerHomingFeather)
                {
                    whiteModeShots = 0;
                    SpawnHomingFeather(position, velocity, damage);
                }
            }
        }
        
        private void SpawnHomingFeather(Vector2 position, Vector2 velocity, int damage)
        {
            // Spawn a homing swan feather projectile - TRIPLE DAMAGE!
            int proj = Projectile.NewProjectile(
                Player.GetSource_Accessory(null), 
                position, 
                velocity.SafeNormalize(Vector2.UnitY) * 14f, // Fast speed
                ModContent.ProjectileType<HomingSwanFeather>(), 
                (int)(damage * 3f), // TRIPLE DAMAGE!
                4f, 
                Player.whoAmI
            );
            
            // SPECIAL AUDIO CUE - distinct chime sound when fired
            SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.6f, Volume = 0.8f }, position); // Crystal chime
            SoundEngine.PlaySound(SoundID.Item25 with { Pitch = 0.3f, Volume = 0.5f }, position); // Feather whoosh
            
            // Spawn effect - BLACK feather with rainbow burst!
            for (int i = 0; i < 12; i++)
            {
                float hue = i / 12f;
                Color rainbowCol = Main.hslToRgb(hue, 1f, 0.7f);
                Dust rainbow = Dust.NewDustPerfect(position, DustID.RainbowTorch,
                    Main.rand.NextVector2Circular(4f, 4f), 0, rainbowCol, 1.3f);
                rainbow.noGravity = true;
            }
            
            // Black core burst
            for (int i = 0; i < 6; i++)
            {
                Dust dark = Dust.NewDustPerfect(position, DustID.Shadowflame,
                    Main.rand.NextVector2Circular(3f, 3f), 150, Color.Black, 1.2f);
                dark.noGravity = true;
            }
            
            // Rainbow flare
            CustomParticles.GenericFlare(position, Color.White, 0.5f, 18);
            ThemedParticles.SwanLakeFractalTrail(position, 0.5f);
        }
    }

    /// <summary>
    /// GlobalProjectile to handle pierce bonuses, Flame of the Swan, and healing effects from the Dual Feather Quiver.
    /// Works with BOTH arrows and bullets (all ranged projectiles).
    /// </summary>
    public class DualFeatherQuiverGlobalProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        
        private bool hasQuiverBonus = false;
        private bool isBlackMode = false;
        private int healCooldown = 0;

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            // Check if this is a player-fired projectile
            if (projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
                return;
                
            Player player = Main.player[projectile.owner];
            if (player == null || !player.active)
                return;
                
            var swanPlayer = player.GetModPlayer<SwanLakeAccessoryPlayer>();
            var quiverBonus = player.GetModPlayer<DualFeatherQuiverProjectileBonus>();

            // Check if this is a ranged projectile (use CountsAsClass for wider compatibility)
            bool isRanged = projectile.DamageType == DamageClass.Ranged || 
                           projectile.DamageType.CountsAsClass(DamageClass.Ranged);
            
            // Works with ALL ranged projectiles when quiver is equipped
            if (swanPlayer.hasDualFeatherQuiver && isRanged && !projectile.minion && !projectile.sentry)
            {
                hasQuiverBonus = true;
                isBlackMode = swanPlayer.quiverIsBlackMode;

                // Black mode: Add extra pierce
                if (isBlackMode && quiverBonus.extraPierce > 0)
                {
                    if (projectile.penetrate > 0)
                        projectile.penetrate += quiverBonus.extraPierce;
                    else if (projectile.penetrate == 1)
                        projectile.penetrate = 1 + quiverBonus.extraPierce;
                }
            }
        }

        public override void AI(Projectile projectile)
        {
            // Fallback check in case OnSpawn was called before accessory update
            if (!hasQuiverBonus && projectile.owner >= 0 && projectile.owner < Main.maxPlayers)
            {
                Player player = Main.player[projectile.owner];
                if (player != null && player.active)
                {
                    var swanPlayer = player.GetModPlayer<SwanLakeAccessoryPlayer>();
                    bool isRanged = projectile.DamageType == DamageClass.Ranged || 
                                   projectile.DamageType.CountsAsClass(DamageClass.Ranged);
                    
                    if (swanPlayer.hasDualFeatherQuiver && isRanged && !projectile.minion && !projectile.sentry)
                    {
                        hasQuiverBonus = true;
                        isBlackMode = swanPlayer.quiverIsBlackMode;
                        
                        // Apply pierce bonus if not already applied
                        var quiverBonus = player.GetModPlayer<DualFeatherQuiverProjectileBonus>();
                        if (isBlackMode && quiverBonus.extraPierce > 0 && projectile.penetrate > 0)
                        {
                            // Only apply if penetrate hasn't been modified significantly
                            // This prevents double-application
                        }
                    }
                }
            }
            
            if (!hasQuiverBonus)
                return;

            if (healCooldown > 0)
                healCooldown--;

            // Trail particles
            if (Main.rand.NextBool(2))
            {
                if (isBlackMode)
                {
                    // Black flame trail - menacing dark wisps
                    Dust black = Dust.NewDustPerfect(projectile.Center, DustID.Smoke,
                        -projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1.5f, 1.5f), 200, Color.Black, 1.2f);
                    black.noGravity = true;
                    
                    // Occasional white accent
                    if (Main.rand.NextBool(4))
                    {
                        Dust accent = Dust.NewDustPerfect(projectile.Center, DustID.WhiteTorch,
                            -projectile.velocity * 0.1f, 100, default, 0.6f);
                        accent.noGravity = true;
                    }
                }
                else
                {
                    // White healing trail - bright and warm
                    Dust white = Dust.NewDustPerfect(projectile.Center, DustID.WhiteTorch,
                        -projectile.velocity * 0.12f + Main.rand.NextVector2Circular(0.8f, 0.8f), 80, default, 1.1f);
                    white.noGravity = true;
                    
                    // Healing shimmer particles
                    if (Main.rand.NextBool(5))
                    {
                        Color healColor = new Color(150, 255, 200); // Soft green healing tint
                        Dust heal = Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2Circular(5f, 5f),
                            DustID.TintableDustLighted, Vector2.Zero, 0, healColor, 0.7f);
                        heal.noGravity = true;
                    }
                }
            }

            // Pearlescent shimmer trail (both modes)
            if (Main.rand.NextBool(6))
            {
                Color pearlescent = Main.rand.Next(3) switch
                {
                    0 => new Color(255, 240, 245),
                    1 => new Color(240, 245, 255),
                    _ => new Color(250, 255, 245)
                };
                Dust pearl = Dust.NewDustPerfect(projectile.Center, DustID.TintableDustLighted,
                    -projectile.velocity * 0.05f, 0, pearlescent, 0.5f);
                pearl.noGravity = true;
            }
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!hasQuiverBonus) return;

            Player player = Main.player[projectile.owner];

            if (isBlackMode)
            {
                // Apply Flame of the Swan debuff (4 seconds = 240 ticks)
                target.AddBuff(ModContent.BuffType<FlameOfTheSwan>(), 240);
                
                // Black/white flame burst on hit
                for (int i = 0; i < 12; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 vel = new Vector2((float)System.Math.Cos(angle), (float)System.Math.Sin(angle)) * Main.rand.NextFloat(2f, 4f);
                    
                    if (i % 2 == 0)
                    {
                        Dust black = Dust.NewDustPerfect(target.Center, DustID.Smoke, vel, 200, Color.Black, 1.4f);
                        black.noGravity = true;
                    }
                    else
                    {
                        Dust white = Dust.NewDustPerfect(target.Center, DustID.WhiteTorch, vel * 0.8f, 80, default, 1f);
                        white.noGravity = true;
                    }
                }
            }
            else
            {
                // White mode: 50% chance on ALL HITS to drop healing feather
                if (Main.rand.NextBool(2))  // 50% chance on ANY hit
                {
                    // Spawn healing feather pickup
                    int healProj = Projectile.NewProjectile(
                        projectile.GetSource_FromThis(),
                        target.Center,
                        new Vector2(Main.rand.NextFloat(-2f, 2f), -4f),
                        ModContent.ProjectileType<HealingFeatherPickup>(),
                        0,
                        0,
                        player.whoAmI
                    );
                }
                
                // White flame hit effect
                for (int i = 0; i < 6; i++)
                {
                    Dust white = Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(10f, 10f),
                        DustID.WhiteTorch, Main.rand.NextVector2Circular(1.5f, 1.5f), 80, default, 1f);
                    white.noGravity = true;
                }
            }
        }
    }
    
    /// <summary>
    /// Homing Swan Feather - A BLACK feather with rainbow outline that seeks nearby enemies.
    /// Spawned every 5 shots when Dual Feather Quiver is in White mode (Odette).
    /// Deals TRIPLE damage and has special audio + visual effects!
    /// </summary>
    public class HomingSwanFeather : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SwanFeather5";
        
        private const float MaxHomingDistance = 500f;
        private const float HomingStrength = 0.18f;
        private const float MaxSpeed = 18f;
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 240; // 4 seconds
            Projectile.alpha = 0;
            Projectile.extraUpdates = 1;
            // TRIPLE DAMAGE is applied via damage calculation in SpawnHomingFeather
        }
        
        public override void AI()
        {
            // Rotate to face movement direction
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            
            // Find nearest enemy
            NPC target = FindClosestEnemy();
            if (target != null)
            {
                // Strong homing towards target
                Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * MaxSpeed, HomingStrength);
            }
            
            // Clamp speed
            if (Projectile.velocity.Length() > MaxSpeed)
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * MaxSpeed;
            
            // === BLACK FEATHER WITH RAINBOW OUTLINE TRAIL ===
            // Dark core trail
            if (Main.rand.NextBool(2))
            {
                Dust dark = Dust.NewDustPerfect(Projectile.Center, DustID.Shadowflame,
                    -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1f, 1f), 150, Color.Black, 1.4f);
                dark.noGravity = true;
            }
            
            // RAINBOW OUTLINE - cycling hue particles around the feather
            float hue = (Main.GameUpdateCount * 0.03f + Projectile.whoAmI * 0.1f) % 1f;
            Color rainbowColor = Main.hslToRgb(hue, 1f, 0.7f);
            
            // Rainbow glow trail
            if (Main.rand.NextBool(2))
            {
                Dust rainbow = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f), DustID.RainbowTorch,
                    -Projectile.velocity * 0.12f + Main.rand.NextVector2Circular(0.8f, 0.8f), 0, rainbowColor, 1.5f);
                rainbow.noGravity = true;
            }
            
            // Occasional rainbow flare
            if (Main.rand.NextBool(6))
            {
                CustomParticles.GenericFlare(Projectile.Center, rainbowColor, 0.35f, 14);
            }
            
            // Fractal sparkle effect occasionally
            if (Main.rand.NextBool(10))
            {
                ThemedParticles.SwanLakeFractalTrail(Projectile.Center, 0.4f);
            }
            
            // Pearlescent shimmer
            if (Main.rand.NextBool(4))
            {
                Color pearl = Main.rand.Next(3) switch
                {
                    0 => new Color(255, 240, 245),
                    1 => new Color(240, 245, 255),
                    _ => new Color(250, 255, 245)
                };
                Dust shimmer = Dust.NewDustPerfect(Projectile.Center, DustID.TintableDustLighted,
                    Vector2.Zero, 0, pearl, 0.8f);
                shimmer.noGravity = true;
            }
            
            // ☁EMUSICAL NOTATION - Rainbow feather melody trail
            if (Main.rand.NextBool(6))
            {
                float noteHue = (Main.GameUpdateCount * 0.01f + Projectile.whoAmI * 0.1f) % 1f;
                Color noteColor = Main.hslToRgb(noteHue, 1f, 0.85f);
                ThemedParticles.MusicNote(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), 
                    -Projectile.velocity * 0.1f, noteColor, 0.35f, 20);
            }
            
            // Rainbow cycling light
            Vector3 lightColor = Main.hslToRgb(hue, 0.8f, 0.6f).ToVector3();
            Lighting.AddLight(Projectile.Center, lightColor * 0.6f);
        }
        
        private NPC FindClosestEnemy()
        {
            NPC closest = null;
            float closestDist = MaxHomingDistance;
            
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (!npc.CanBeChasedBy() || npc.friendly)
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
            // BLACK AND RAINBOW feather burst on death
            for (int i = 0; i < 12; i++)
            {
                float h = i / 12f;
                Color rainbowCol = Main.hslToRgb(h, 1f, 0.7f);
                Dust burst = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowTorch,
                    Main.rand.NextVector2Circular(5f, 5f), 0, rainbowCol, 1.5f);
                burst.noGravity = true;
            }
            
            // Black core burst
            for (int i = 0; i < 8; i++)
            {
                Dust dark = Dust.NewDustPerfect(Projectile.Center, DustID.Shadowflame,
                    Main.rand.NextVector2Circular(4f, 4f), 150, Color.Black, 1.3f);
                dark.noGravity = true;
            }
            
            // Fractal burst
            ThemedParticles.SwanLakeFractalGemBurst(Projectile.Center, Color.Black, 0.5f, 4, false);
            
            // ☁EMUSICAL FINALE - Rainbow feathered symphony
            float finaleHue = (Main.GameUpdateCount * 0.02f) % 1f;
            Color finaleColor = Main.hslToRgb(finaleHue, 1f, 0.85f);
            ThemedParticles.MusicNoteBurst(Projectile.Center, finaleColor, 6, 4f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            // Draw rainbow glow outline trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                float fade = 1f - (i / (float)Projectile.oldPos.Length);
                float trailScale = (1f - i * 0.08f) * 0.8f;
                float hue = ((Main.GameUpdateCount * 0.03f) + i * 0.1f) % 1f;
                Color trailColor = Main.hslToRgb(hue, 1f, 0.7f) * fade * 0.6f;
                
                // Rainbow glow texture
                Texture2D glowTex = TextureAssets.Extra[ExtrasID.SharpTears].Value;
                spriteBatch.Draw(glowTex, trailPos, null, trailColor, Projectile.oldRot[i], glowTex.Size() / 2f, trailScale * 0.4f, SpriteEffects.None, 0f);
            }
            
            // Draw rainbow outline glow around feather
            float currentHue = (Main.GameUpdateCount * 0.03f) % 1f;
            Color glowColor = Main.hslToRgb(currentHue, 1f, 0.7f) * 0.7f;
            Texture2D glow = TextureAssets.Extra[ExtrasID.SharpTears].Value;
            spriteBatch.Draw(glow, drawPos, null, glowColor, Projectile.rotation, glow.Size() / 2f, 0.5f, SpriteEffects.None, 0f);
            
            // Draw black feather core (tinted dark)
            spriteBatch.Draw(texture, drawPos, null, new Color(30, 30, 35), Projectile.rotation, origin, 0.8f, SpriteEffects.None, 0f);
            
            return false;
        }
    }
    
    /// <summary>
    /// Healing Feather Pickup - A graceful white feather that floats down and heals the player on pickup.
    /// 50% chance to drop when hitting enemies in White mode (Odette).
    /// Features elegant pearlescent glow and swan-themed visuals.
    /// </summary>
    public class HealingFeatherPickup : ModProjectile
    {
        // Use the Feather's Call texture for a proper feather look
        public override string Texture => "MagnumOpus/Content/SwanLake/Items/FeathersCall";
        
        private float glowPulse = 0f;
        private float swayOffset = 0f;
        
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 600; // 10 seconds
            Projectile.alpha = 0;
            Projectile.scale = 0.7f;
        }
        
        public override void AI()
        {
            // Initialize sway offset
            if (swayOffset == 0f)
                swayOffset = Main.rand.NextFloat(0f, MathHelper.TwoPi);
            
            // Gentle floating/falling motion like a real feather
            Projectile.velocity.Y += 0.03f; // Very slow gravity
            if (Projectile.velocity.Y > 1.5f)
                Projectile.velocity.Y = 1.5f;
            
            // Elegant sway motion
            float swaySpeed = 0.04f;
            Projectile.velocity.X = (float)System.Math.Sin(Main.GameUpdateCount * swaySpeed + swayOffset) * 0.8f;
            
            // Rotate based on movement - feather tumbles gracefully
            Projectile.rotation = Projectile.velocity.X * 0.15f + (float)System.Math.Sin(Main.GameUpdateCount * 0.03f + swayOffset) * 0.2f;
            
            // Glow pulse
            glowPulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.08f) * 0.3f + 0.7f;
            
            // === WHITE SWAN FEATHER PARTICLES ===
            // Pearlescent shimmer
            if (Main.rand.NextBool(4))
            {
                Color pearlColor = Main.rand.Next(4) switch
                {
                    0 => new Color(255, 250, 255), // Pure white
                    1 => new Color(255, 245, 250), // Soft pink tint
                    2 => new Color(245, 250, 255), // Soft blue tint
                    _ => new Color(250, 255, 250)  // Soft green tint
                };
                Dust shimmer = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    DustID.TintableDustLighted, 
                    new Vector2(0, -0.3f), 
                    0, pearlColor, 0.9f);
                shimmer.noGravity = true;
            }
            
            // Soft white glow trail
            if (Main.rand.NextBool(3))
            {
                Dust glow = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(6f, 6f), 
                    DustID.WhiteTorch,
                    new Vector2(0, 0.2f), 100, default, 0.7f);
                glow.noGravity = true;
            }
            
            // Healing sparkle (subtle golden-white)
            if (Main.rand.NextBool(6))
            {
                Dust heal = Dust.NewDustPerfect(
                    Projectile.Center, 
                    DustID.GoldCoin,
                    Main.rand.NextVector2Circular(1f, 1f) + new Vector2(0, -0.5f), 
                    100, default, 0.5f);
                heal.noGravity = true;
            }
            
            // Rainbow iridescence occasionally
            if (Main.rand.NextBool(10))
            {
                float hue = (Main.GameUpdateCount * 0.02f + Projectile.whoAmI * 0.3f) % 1f;
                Color rainbowTint = Main.hslToRgb(hue, 0.4f, 0.9f);
                Dust iridescent = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.TintableDustLighted, 
                    Vector2.Zero, 0, rainbowTint, 0.6f);
                iridescent.noGravity = true;
            }
            
            // ☁EMUSICAL NOTATION - Gentle healing melody trail
            if (Main.rand.NextBool(10))
            {
                float noteHue = (Main.GameUpdateCount * 0.01f + Projectile.whoAmI * 0.2f) % 1f;
                Color noteColor = Main.hslToRgb(noteHue, 0.4f, 0.9f);
                ThemedParticles.MusicNote(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f), 
                    new Vector2(0, -0.5f), noteColor * 0.8f, 0.25f, 18);
            }
            
            // Check for player pickup
            Player owner = Main.player[Projectile.owner];
            if (Vector2.Distance(owner.Center, Projectile.Center) < 50f)
            {
                // Heal player
                owner.Heal(5);
                
                // === PICKUP EFFECTS ===
                SoundEngine.PlaySound(SoundID.Item4 with { Pitch = 0.6f, Volume = 0.5f }, Projectile.Center);
                
                // White feather burst
                for (int i = 0; i < 20; i++)
                {
                    Color white = new Color(255, 252, 255);
                    Dust burst = Dust.NewDustPerfect(
                        Projectile.Center, 
                        DustID.TintableDustLighted,
                        Main.rand.NextVector2Circular(5f, 5f), 
                        0, white, 1.3f);
                    burst.noGravity = true;
                }
                
                // Golden healing sparkles rising
                for (int i = 0; i < 12; i++)
                {
                    Dust gold = Dust.NewDustPerfect(
                        Projectile.Center, 
                        DustID.GoldCoin,
                        new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-4f, -1f)), 
                        80, default, 0.9f);
                    gold.noGravity = true;
                }
                
                // Pearlescent shimmer ring
                for (int i = 0; i < 8; i++)
                {
                    float angle = i / 8f * MathHelper.TwoPi;
                    Vector2 offset = new Vector2((float)System.Math.Cos(angle), (float)System.Math.Sin(angle)) * 20f;
                    Color pearl = Main.hslToRgb((i / 8f + Main.GameUpdateCount * 0.01f) % 1f, 0.3f, 0.95f);
                    Dust ring = Dust.NewDustPerfect(
                        Projectile.Center + offset, 
                        DustID.TintableDustLighted,
                        offset * 0.15f, 0, pearl, 1f);
                    ring.noGravity = true;
                }
                
                // ☁EMUSICAL HEALING - Healing feather symphony
                ThemedParticles.MusicNoteBurst(Projectile.Center, Color.White, 5, 3.5f);
                
                Projectile.Kill();
            }
            
            // Elegant white light with subtle warmth
            float lightIntensity = 0.4f + glowPulse * 0.2f;
            Lighting.AddLight(Projectile.Center, lightIntensity, lightIntensity * 0.98f, lightIntensity * 0.95f);
        }
        
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // Gentle bounce like a real feather
            Projectile.velocity.Y = -oldVelocity.Y * 0.2f;
            Projectile.velocity.X = oldVelocity.X * 0.8f;
            
            // Soft landing particles
            for (int i = 0; i < 3; i++)
            {
                Dust land = Dust.NewDustPerfect(
                    Projectile.Bottom, 
                    DustID.WhiteTorch,
                    new Vector2(Main.rand.NextFloat(-1f, 1f), -0.5f), 
                    100, default, 0.5f);
                land.noGravity = true;
            }
            
            return false;
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            // === GLOW BACKDROP ===
            Texture2D glowTex = TextureAssets.Extra[ExtrasID.SharpTears].Value; // Soft glow texture
            float glowScale = 0.6f + glowPulse * 0.15f;
            
            // White glow base
            spriteBatch.Draw(glowTex, drawPos, null, 
                Color.White * 0.4f * glowPulse, 
                0f, glowTex.Size() / 2f, glowScale, SpriteEffects.None, 0f);
            
            // Subtle golden healing glow
            spriteBatch.Draw(glowTex, drawPos, null, 
                new Color(255, 245, 200) * 0.2f * glowPulse, 
                0f, glowTex.Size() / 2f, glowScale * 0.8f, SpriteEffects.None, 0f);
            
            // Rainbow iridescent outer ring (very subtle)
            float hue = (Main.GameUpdateCount * 0.015f) % 1f;
            Color iridescentColor = Main.hslToRgb(hue, 0.5f, 0.9f) * 0.15f;
            spriteBatch.Draw(glowTex, drawPos, null, 
                iridescentColor, 
                0f, glowTex.Size() / 2f, glowScale * 1.2f, SpriteEffects.None, 0f);
            
            // === MAIN FEATHER ===
            // Draw with bright white tint (self-illuminated look)
            Color featherColor = Color.Lerp(Color.White, new Color(255, 252, 250), 0.5f);
            spriteBatch.Draw(texture, drawPos, null, 
                featherColor, 
                Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            
            // Slight additive glow overlay on the feather itself
            spriteBatch.Draw(texture, drawPos, null, 
                Color.White * 0.2f * glowPulse, 
                Projectile.rotation, origin, Projectile.scale * 1.05f, SpriteEffects.None, 0f);
            
            return false;
        }
        
        public override Color? GetAlpha(Color lightColor)
        {
            // Self-illuminated white feather
            return Color.White;
        }
    }
}

