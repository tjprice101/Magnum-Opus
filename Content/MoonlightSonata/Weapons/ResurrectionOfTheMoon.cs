using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Audio;
using Terraria.GameContent;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.Enemies;
using MagnumOpus.Content.MoonlightSonata.Projectiles;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Content.MoonlightSonata.Accessories;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;

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
            
            // Ethereal particles while holding
            if (Main.rand.NextBool(6))
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                ThemedParticles.MoonlightAura(player.Center + offset, 12f);
            }
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            // Draw glowing backlight effect when dropped in world
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            // Calculate pulse - powerful and ominous
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.1f + 1f;
            
            // Begin additive blending for glow
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Outer deep purple aura - resurrection power
            spriteBatch.Draw(texture, position, null, new Color(80, 20, 120) * 0.5f, rotation, origin, scale * pulse * 1.4f, SpriteEffects.None, 0f);
            
            // Middle violet/magenta glow - moonlight energy
            spriteBatch.Draw(texture, position, null, new Color(180, 80, 200) * 0.35f, rotation, origin, scale * pulse * 1.2f, SpriteEffects.None, 0f);
            
            // Inner silver/white glow - devastating power
            spriteBatch.Draw(texture, position, null, new Color(220, 210, 255) * 0.25f, rotation, origin, scale * pulse * 1.08f, SpriteEffects.None, 0f);
            
            // Return to normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Add lighting
            Lighting.AddLight(Item.Center, 0.5f, 0.3f, 0.65f);
            
            return true;
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
            // GRADIENT COLORS: Dark Purple → Violet → Light Blue
            Color darkPurple = new Color(75, 0, 130);
            Color violet = new Color(138, 43, 226);
            Color lightBlue = new Color(135, 206, 250);
            Color silver = new Color(220, 220, 235);
            
            // Fire our custom projectile instead of the ammo type
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<ResurrectionProjectile>(), damage, knockback, player.whoAmI);
            
            // Powerful shot sound
            SoundEngine.PlaySound(SoundID.Item40 with { Volume = 1.2f, Pitch = -0.5f }, position);
            SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.7f, Pitch = -0.3f }, position);
            
            Vector2 muzzlePos = position + velocity.SafeNormalize(Vector2.Zero) * 40f;
            
            // === CUSTOM PARTICLES WITH GRADIENT ===
            // Fractal geometric burst
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 offset = angle.ToRotationVector2() * 30f;
                float progress = (float)i / 8f;
                Color fractalColor = Color.Lerp(darkPurple, lightBlue, progress);
                CustomParticles.GenericFlare(muzzlePos + offset, fractalColor, 0.5f, 18);
            }
            
            // Gradient halo rings
            for (int ring = 0; ring < 4; ring++)
            {
                float progress = (float)ring / 4f;
                Color ringColor = Color.Lerp(darkPurple, lightBlue, progress);
                CustomParticles.HaloRing(muzzlePos, ringColor, 0.4f + ring * 0.15f, 15 + ring * 4);
            }
            
            // Explosion burst with gradient
            for (int i = 0; i < 16; i++)
            {
                float progress = (float)i / 16f;
                Color burstColor = Color.Lerp(violet, lightBlue, progress);
                CustomParticles.GenericGlow(muzzlePos, burstColor, 0.35f, 20);
            }
            
            // Central white flash
            CustomParticles.GenericFlare(muzzlePos, silver, 0.8f, 15);
            ThemedParticles.MoonlightHaloBurst(muzzlePos, 1.0f);
            
            // Muzzle flash effect with gradient dust
            for (int i = 0; i < 25; i++)
            {
                Vector2 dustVel = velocity.SafeNormalize(Vector2.Zero).RotatedByRandom(0.4f) * Main.rand.NextFloat(3f, 8f);
                float progress = (float)i / 25f;
                Color dustColor = Color.Lerp(darkPurple, lightBlue, progress);
                int dustType = progress < 0.5f ? DustID.PurpleTorch : DustID.IceTorch;
                Dust dust = Dust.NewDustPerfect(muzzlePos, dustType, dustVel, 100, dustColor, 1.8f);
                dust.noGravity = true;
            }
            
            // White sparkles at barrel with gradient
            for (int i = 0; i < 10; i++)
            {
                float progress = (float)i / 10f;
                Color sparkColor = Color.Lerp(lightBlue, silver, progress);
                CustomParticles.GenericGlow(muzzlePos, sparkColor, 0.3f, 12);
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
