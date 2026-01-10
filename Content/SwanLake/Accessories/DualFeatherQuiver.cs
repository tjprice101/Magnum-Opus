using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
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
    /// WHITE MODE (Healing Trails):
    /// - Projectiles leave healing white flame trails
    /// - +12% accuracy (tighter spread)
    /// - +3 life regen, hits restore 1 HP
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
                // White mode: +3 life regen
                player.lifeRegen += 3;
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
            
            string modeText = modPlayer.quiverIsBlackMode ? "BLACK SWAN (Pierce)" : "WHITE SWAN (Healing)";
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
            tooltips.Add(new TooltipLine(Mod, "WhiteHeader", "White Swan (Healing Trails):")
            {
                OverrideColor = whiteColor
            });
            tooltips.Add(new TooltipLine(Mod, "WhiteEffect1", "  Projectiles leave healing white flame trails")
            {
                OverrideColor = whiteColor
            });
            tooltips.Add(new TooltipLine(Mod, "WhiteEffect2", "  +12% accuracy (tighter spread)")
            {
                OverrideColor = whiteColor
            });
            tooltips.Add(new TooltipLine(Mod, "WhiteEffect3", "  +3 life regeneration")
            {
                OverrideColor = whiteColor
            });
            tooltips.Add(new TooltipLine(Mod, "WhiteEffect4", "  Hits restore 1 HP")
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
                .AddIngredient(ItemID.SoulofSight, 5)
                .AddIngredient(ItemID.SoulofFlight, 8)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }

    /// <summary>
    /// Helper ModPlayer to handle projectile modifications for the Dual Feather Quiver.
    /// Also tracks shot count for spawning healing halos every 20 shots.
    /// </summary>
    public class DualFeatherQuiverProjectileBonus : ModPlayer
    {
        public int extraPierce = 0;
        public int whiteModeShots = 0; // Shot counter for healing halo spawning
        private const int ShotsPerHealingHalo = 20;

        public override void ResetEffects()
        {
            extraPierce = 0;
        }

        public override void ModifyShootStats(Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            // White mode: Reduce spread for better accuracy
            var swanPlayer = Player.GetModPlayer<SwanLakeAccessoryPlayer>();
            if (swanPlayer.hasDualFeatherQuiver && !swanPlayer.quiverIsBlackMode && item.DamageType == DamageClass.Ranged)
            {
                // Track shots and spawn healing halo every 20 shots
                whiteModeShots++;
                
                if (whiteModeShots >= ShotsPerHealingHalo)
                {
                    whiteModeShots = 0;
                    SpawnHealingHalo(position);
                }
            }
        }
        
        private void SpawnHealingHalo(Vector2 position)
        {
            // Spawn a healing halo projectile that lasts 5 seconds
            Projectile.NewProjectile(
                Player.GetSource_Accessory(null), 
                position, 
                Vector2.Zero, 
                ModContent.ProjectileType<HealingSwanHalo>(), 
                0, 
                0, 
                Player.whoAmI
            );
            
            // Spawn effect
            ThemedParticles.SwanLakeBloomBurst(position, 0.8f);
            ThemedParticles.SwanLakeHalo(position, 30f, 10);
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
                // White mode: Heal player on hit
                if (healCooldown <= 0)
                {
                    player.Heal(1);
                    healCooldown = 10; // Small cooldown to prevent spam healing
                    
                    // Healing visual burst
                    for (int i = 0; i < 8; i++)
                    {
                        Dust heal = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(15f, 15f),
                            DustID.WhiteTorch, Main.rand.NextVector2Circular(2f, 2f), 80, default, 0.9f);
                        heal.noGravity = true;
                    }
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
    /// Healing Swan Halo - A persistent healing aura that lasts 5 seconds.
    /// Spawned every 20 shots when Dual Feather Quiver is in White mode.
    /// Heals the player when they stand in it.
    /// </summary>
    public class HealingSwanHalo : ModProjectile
    {
        // Use a simple vanilla texture as placeholder
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.HallowStar;
        
        private const int Duration = 300; // 5 seconds at 60 fps
        private const float HealRadius = 80f;
        private const int HealCooldown = 30; // Heal every 0.5 seconds
        private int healTimer = 0;
        
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 1;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = Duration;
            Projectile.alpha = 255; // Invisible texture, we draw custom particles
        }
        
        public override void AI()
        {
            healTimer++;
            
            // Spawn halo particles
            if (Main.rand.NextBool(2))
            {
                ThemedParticles.SwanLakeAura(Projectile.Center, HealRadius);
            }
            
            // Rotating halo ring
            if (Main.GameUpdateCount % 4 == 0)
            {
                int particleCount = 8;
                for (int i = 0; i < particleCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / particleCount + Main.GameUpdateCount * 0.03f;
                    Vector2 pos = Projectile.Center + angle.ToRotationVector2() * HealRadius * 0.8f;
                    
                    Color color = i % 2 == 0 ? Color.White : new Color(200, 255, 220); // White and healing green
                    Dust ring = Dust.NewDustPerfect(pos, DustID.WhiteTorch, Vector2.Zero, 50, default, 1.2f);
                    ring.noGravity = true;
                }
            }
            
            // Pearlescent sparkles rising
            if (Main.rand.NextBool(3))
            {
                Vector2 offset = Main.rand.NextVector2Circular(HealRadius * 0.7f, HealRadius * 0.7f);
                Color pearl = Main.rand.Next(3) switch
                {
                    0 => new Color(255, 240, 245),
                    1 => new Color(240, 255, 240), // Green tint for healing
                    _ => new Color(200, 255, 220)
                };
                Dust sparkle = Dust.NewDustPerfect(Projectile.Center + offset, DustID.TintableDustLighted,
                    new Vector2(0, -1f), 0, pearl, 0.9f);
                sparkle.noGravity = true;
            }
            
            // Heal players in radius
            if (healTimer >= HealCooldown)
            {
                healTimer = 0;
                Player owner = Main.player[Projectile.owner];
                
                if (Vector2.Distance(owner.Center, Projectile.Center) <= HealRadius)
                {
                    owner.Heal(2);
                    
                    // Healing visual
                    for (int i = 0; i < 6; i++)
                    {
                        Dust heal = Dust.NewDustPerfect(owner.Center + Main.rand.NextVector2Circular(15f, 15f),
                            DustID.WhiteTorch, new Vector2(0, -2f), 80, default, 1f);
                        heal.noGravity = true;
                    }
                }
            }
            
            // Light
            Lighting.AddLight(Projectile.Center, 0.5f, 0.5f, 0.55f);
            
            // Fade out effect near end of life
            if (Projectile.timeLeft < 60)
            {
                // Reduce particle spawn rate as it fades
            }
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            // Don't draw the texture, particles only
            return false;
        }
    }
}
