using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Audio;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.Enemies;
using MagnumOpus.Content.MoonlightSonata.Projectiles;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Content.MoonlightSonata.Accessories;
using MagnumOpus.Common;

namespace MagnumOpus.Content.MoonlightSonata.Weapons
{
    /// <summary>
    /// Resurrection of the Moon - A devastating moonlight sniper rifle.
    /// Fires slowly but deals massive damage.
    /// Bullets ricochet 10 times to nearby enemies with radial explosions.
    /// Has a reloading mechanic with sound effects.
    /// </summary>
    public class ResurrectionOfTheMoon : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 70;
            Item.height = 26;
            Item.damage = 1500; // Balanced: Heavy sniper, burst damage compensates slow rate
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 90; // Very slow fire rate
            Item.useAnimation = 90;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 12f;
            Item.value = Item.buyPrice(platinum: 3);
            Item.rare = ModContent.RarityType<MoonlightSonataRarity>();
            Item.UseSound = null; // Custom sound handling
            Item.autoReuse = false;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<ResurrectionProjectile>();
            Item.shootSpeed = 24f;
            Item.useAmmo = AmmoID.Bullet;
            Item.maxStack = 1;
        }

        public override void HoldItem(Player player)
        {
            var modPlayer = player.GetModPlayer<MoonlightAccessoryPlayer>();
            
            // Handle reload timer using ModPlayer state
            if (!modPlayer.resurrectionIsReloaded)
            {
                modPlayer.resurrectionReloadTimer++;
                
                // Play reload sound at the start
                if (modPlayer.resurrectionReloadTimer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item149 with { Volume = 0.8f, Pitch = -0.3f }, player.Center);
                    modPlayer.resurrectionPlayedReadySound = false;
                }
                
                // Reload complete
                if (modPlayer.resurrectionReloadTimer >= MoonlightAccessoryPlayer.ResurrectionReloadTime)
                {
                    modPlayer.resurrectionIsReloaded = true;
                    modPlayer.resurrectionReloadTimer = 0;
                    
                    // Play ready *clink* sound
                    if (!modPlayer.resurrectionPlayedReadySound)
                    {
                        SoundEngine.PlaySound(SoundID.Unlock with { Volume = 1f, Pitch = 0.5f }, player.Center);
                        SoundEngine.PlaySound(SoundID.Item37 with { Volume = 0.6f, Pitch = 0.8f }, player.Center);
                        modPlayer.resurrectionPlayedReadySound = true;
                        
                        // Visual indicator when ready
                        for (int i = 0; i < 12; i++)
                        {
                            Vector2 dustVel = Main.rand.NextVector2Circular(3f, 3f);
                            int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.SilverCoin;
                            Dust dust = Dust.NewDustPerfect(player.Center + new Vector2(30 * player.direction, -5), dustType, dustVel, 100, default, 1.2f);
                            dust.noGravity = true;
                        }
                    }
                }
            }
            
            // Ambient glow effect when held
            Lighting.AddLight(player.Center, 0.3f, 0.15f, 0.45f);
        }

        public override bool CanUseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<MoonlightAccessoryPlayer>();
            return modPlayer.resurrectionIsReloaded;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            var modPlayer = player.GetModPlayer<MoonlightAccessoryPlayer>();
            
            // If player has Moonlit Gyre, buff the damage and velocity
            if (modPlayer.hasMoonlitGyre)
            {
                damage = (int)(damage * 1.25f); // 25% more bullet damage
                velocity *= 1.15f; // Slightly faster bullets
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Fire our custom projectile instead of the ammo type
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<ResurrectionProjectile>(), damage, knockback, player.whoAmI);
            
            // Powerful shot sound
            SoundEngine.PlaySound(SoundID.Item40 with { Volume = 1.2f, Pitch = -0.5f }, position);
            SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.7f, Pitch = -0.3f }, position);
            
            // Muzzle flash effect
            for (int i = 0; i < 25; i++)
            {
                Vector2 dustVel = velocity.SafeNormalize(Vector2.Zero).RotatedByRandom(0.4f) * Main.rand.NextFloat(3f, 8f);
                int dustType = Main.rand.NextBool(3) ? DustID.IceTorch : DustID.PurpleTorch;
                Dust dust = Dust.NewDustPerfect(position + velocity.SafeNormalize(Vector2.Zero) * 40f, dustType, dustVel, 100, default, 1.8f);
                dust.noGravity = true;
            }
            
            // White sparkles at barrel
            for (int i = 0; i < 10; i++)
            {
                Vector2 sparkVel = velocity.SafeNormalize(Vector2.Zero).RotatedByRandom(0.2f) * Main.rand.NextFloat(5f, 12f);
                Dust spark = Dust.NewDustPerfect(position + velocity.SafeNormalize(Vector2.Zero) * 40f, DustID.SilverCoin, sparkVel, 0, Color.White, 1.2f);
                spark.noGravity = true;
            }
            
            // Recoil dust behind player
            for (int i = 0; i < 8; i++)
            {
                Vector2 recoilVel = -velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(2f, 5f);
                recoilVel += Main.rand.NextVector2Circular(2f, 2f);
                Dust recoil = Dust.NewDustPerfect(player.Center, DustID.Smoke, recoilVel, 150, default, 1.5f);
                recoil.noGravity = true;
            }
            
            // Start reload using ModPlayer state
            var modPlayer = player.GetModPlayer<MoonlightAccessoryPlayer>();
            modPlayer.resurrectionIsReloaded = false;
            modPlayer.resurrectionReloadTimer = 0;
            modPlayer.resurrectionPlayedReadySound = false;
            
            return false; // We handled the projectile spawning
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var modPlayer = player.GetModPlayer<MoonlightAccessoryPlayer>();
            
            tooltips.Add(new TooltipLine(Mod, "DevastatingShot", "Fires a devastating moonlight bullet")
            {
                OverrideColor = new Color(180, 120, 255)
            });
            
            tooltips.Add(new TooltipLine(Mod, "RicochetEffect", "Bullets ricochet 10 times to nearby enemies")
            {
                OverrideColor = new Color(150, 180, 220)
            });
            
            tooltips.Add(new TooltipLine(Mod, "ExplosionEffect", "Each hit creates a devastating radial explosion")
            {
                OverrideColor = new Color(120, 80, 180)
            });
            
            tooltips.Add(new TooltipLine(Mod, "ReloadMechanic", "Requires reloading between shots")
            {
                OverrideColor = new Color(200, 200, 200)
            });
            
            // Show Moonlit Gyre synergy
            if (modPlayer.hasMoonlitGyre)
            {
                tooltips.Add(new TooltipLine(Mod, "GyreSynergy", "Moonlit Gyre: +25% damage, +15% velocity")
                {
                    OverrideColor = new Color(100, 255, 150)
                });
            }
            
            // Show reload status using ModPlayer state
            if (!modPlayer.resurrectionIsReloaded)
            {
                float reloadPercent = (float)modPlayer.resurrectionReloadTimer / MoonlightAccessoryPlayer.ResurrectionReloadTime * 100f;
                tooltips.Add(new TooltipLine(Mod, "ReloadStatus", $"Reloading... {reloadPercent:F0}%")
                {
                    OverrideColor = new Color(255, 200, 100)
                });
            }
            else
            {
                tooltips.Add(new TooltipLine(Mod, "ReloadReady", "Ready to fire!")
                {
                    OverrideColor = new Color(100, 255, 100)
                });
            }
            
            tooltips.Add(new TooltipLine(Mod, "Flavor", "'From death comes rebirth in silver light'")
            {
                OverrideColor = new Color(120, 120, 180)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<MoonlightsResonantEnergy>(), 10)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfMoonlightSonata>(), 20)
                .AddIngredient(ModContent.ItemType<ShardsOfMoonlitTempo>(), 10)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }
}
