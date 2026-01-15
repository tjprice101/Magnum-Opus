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
using MagnumOpus.Common.Systems.Particles;

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
                
                // === CALAMITY-INSPIRED RELOAD CHARGING VISUAL ===
                float reloadProgress = (float)modPlayer.resurrectionReloadTimer / MoonlightAccessoryPlayer.ResurrectionReloadTime;
                
                // Pulsing charge indicator around the gun
                if (modPlayer.resurrectionReloadTimer % 8 == 0)
                {
                    Vector2 gunPos = player.Center + new Vector2(35 * player.direction, -5);
                    
                    // Orbiting charge particles that get faster as reload progresses
                    float orbitAngle = Main.GameUpdateCount * (0.1f + reloadProgress * 0.15f);
                    for (int i = 0; i < 3; i++)
                    {
                        float angle = orbitAngle + MathHelper.TwoPi * i / 3f;
                        float radius = 15f + reloadProgress * 8f;
                        Vector2 orbitPos = gunPos + angle.ToRotationVector2() * radius;
                        Color chargeColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.LightBlue, reloadProgress);
                        CustomParticles.GenericFlare(orbitPos, chargeColor * (0.4f + reloadProgress * 0.5f), 0.2f + reloadProgress * 0.15f, 12);
                    }
                }
                
                // Charging dust particles flowing toward gun
                if (Main.rand.NextBool(4))
                {
                    Vector2 gunPos = player.Center + new Vector2(35 * player.direction, -5);
                    Vector2 dustStart = gunPos + Main.rand.NextVector2Circular(40f, 40f);
                    Vector2 dustVel = (gunPos - dustStart).SafeNormalize(Vector2.Zero) * 2f;
                    
                    var chargeDust = new GenericGlowParticle(dustStart, dustVel, 
                        Color.Lerp(UnifiedVFX.MoonlightSonata.MediumPurple, UnifiedVFX.MoonlightSonata.Silver, reloadProgress),
                        0.18f + reloadProgress * 0.12f, 18, true);
                    MagnumParticleHandler.SpawnParticle(chargeDust);
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
                        
                        Vector2 gunPos = player.Center + new Vector2(35 * player.direction, -5);
                        
                        // === READY FLASH - Calamity-inspired burst ===
                        // Central flash
                        CustomParticles.GenericFlare(gunPos, Color.White, 0.7f, 20);
                        CustomParticles.GenericFlare(gunPos, UnifiedVFX.MoonlightSonata.LightBlue, 0.6f, 18);
                        
                        // Fractal burst
                        for (int i = 0; i < 6; i++)
                        {
                            float angle = MathHelper.TwoPi * i / 6f;
                            Vector2 flareOffset = angle.ToRotationVector2() * 20f;
                            float progress = (float)i / 6f;
                            Color fractalColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.Silver, progress);
                            CustomParticles.GenericFlare(gunPos + flareOffset, fractalColor, 0.35f, 15);
                        }
                        
                        // Halo ring
                        CustomParticles.HaloRing(gunPos, UnifiedVFX.MoonlightSonata.LightBlue, 0.4f, 18);
                        
                        // Music notes to indicate ready
                        ThemedParticles.MoonlightMusicNotes(gunPos, 4, 25f);
                        
                        // Dust burst
                        for (int i = 0; i < 12; i++)
                        {
                            Vector2 dustVel = Main.rand.NextVector2Circular(3f, 3f);
                            int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.SilverCoin;
                            Dust dust = Dust.NewDustPerfect(gunPos, dustType, dustVel, 100, default, 1.2f);
                            dust.noGravity = true;
                        }
                    }
                }
            }
            else
            {
                // === READY STATE - Subtle ambient glow ===
                if (Main.rand.NextBool(8))
                {
                    Vector2 gunPos = player.Center + new Vector2(35 * player.direction, -5);
                    CustomParticles.GenericFlare(gunPos + Main.rand.NextVector2Circular(10f, 10f), 
                        UnifiedVFX.MoonlightSonata.Silver * 0.6f, 0.2f, 15);
                }
            }
            
            // Ambient glow effect when held - pulsing
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.1f + 0.9f;
            Lighting.AddLight(player.Center, 0.35f * pulse, 0.18f * pulse, 0.5f * pulse);
            
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
            // Fire our custom projectile instead of the ammo type
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<ResurrectionProjectile>(), damage, knockback, player.whoAmI);
            
            // Powerful shot sound
            SoundEngine.PlaySound(SoundID.Item40 with { Volume = 1.2f, Pitch = -0.5f }, position);
            SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.7f, Pitch = -0.3f }, position);
            
            Vector2 muzzlePos = position + velocity.SafeNormalize(Vector2.Zero) * 45f;
            Vector2 direction = velocity.SafeNormalize(Vector2.UnitX);
            
            // === CALAMITY-INSPIRED DEVASTATING MUZZLE FLASH ===
            
            // Phase 1: Central white explosion
            CustomParticles.GenericFlare(muzzlePos, Color.White, 1.2f, 22);
            CustomParticles.GenericFlare(muzzlePos, UnifiedVFX.MoonlightSonata.Silver, 0.9f, 20);
            
            // Phase 2: UnifiedVFX explosion (without screen shake)
            ThemedParticles.MoonlightShockwave(muzzlePos, 1.0f);
            
            // Phase 3: Spiral galaxy fractal burst
            for (int arm = 0; arm < 6; arm++)
            {
                float armAngle = MathHelper.TwoPi * arm / 6f;
                for (int point = 0; point < 4; point++)
                {
                    float spiralAngle = armAngle + point * 0.35f;
                    float spiralRadius = 12f + point * 14f;
                    Vector2 spiralPos = muzzlePos + spiralAngle.ToRotationVector2() * spiralRadius;
                    float progress = (arm * 4 + point) / 24f;
                    Color galaxyColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.LightBlue, progress);
                    CustomParticles.GenericFlare(spiralPos, galaxyColor, 0.45f + point * 0.08f, 16 + point * 2);
                }
            }
            
            // Phase 4: Layered halo cascade
            for (int ring = 0; ring < 5; ring++)
            {
                float ringProgress = (float)ring / 5f;
                Color ringColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.Silver, ringProgress);
                CustomParticles.HaloRing(muzzlePos, ringColor, 0.35f + ring * 0.15f, 14 + ring * 4);
            }
            
            // Phase 5: Directional muzzle flash particles
            for (int i = 0; i < 18; i++)
            {
                Vector2 flashVel = direction.RotatedByRandom(0.5f) * Main.rand.NextFloat(6f, 14f);
                float progress = (float)i / 18f;
                Color flashColor = Color.Lerp(UnifiedVFX.MoonlightSonata.MediumPurple, UnifiedVFX.MoonlightSonata.LightBlue, progress);
                
                var flash = new GenericGlowParticle(muzzlePos, flashVel, flashColor, 0.4f, 20, true);
                MagnumParticleHandler.SpawnParticle(flash);
            }
            
            // Phase 6: Music notes burst - the lunar sonata fires
            ThemedParticles.MoonlightMusicNotes(muzzlePos, 10, 50f);
            
            // Phase 7: Music notes trail along shot direction
            for (int i = 0; i < 5; i++)
            {
                Vector2 noteOffset = direction * (25f + i * 18f);
                float noteProgress = (float)i / 5f;
                Color noteColor = Color.Lerp(UnifiedVFX.MoonlightSonata.MediumPurple, UnifiedVFX.MoonlightSonata.Silver, noteProgress);
                ThemedParticles.MusicNote(muzzlePos + noteOffset, direction * 1.5f, noteColor, 0.3f, 22);
            }
            
            // Phase 8: Lightning fractals shooting from muzzle
            for (int i = 0; i < 3; i++)
            {
                float lightningAngle = direction.ToRotation() + MathHelper.ToRadians((i - 1) * 25f);
                Vector2 lightningEnd = muzzlePos + lightningAngle.ToRotationVector2() * 70f;
                MagnumVFX.DrawMoonlightLightning(muzzlePos, lightningEnd, 8, 22f, 2, 0.45f);
            }
            
            // Phase 9: Recoil dust behind player
            Vector2 recoilPos = player.Center - direction * 20f;
            for (int i = 0; i < 10; i++)
            {
                Vector2 recoilVel = -direction * Main.rand.NextFloat(3f, 6f) + Main.rand.NextVector2Circular(2f, 2f);
                Dust recoil = Dust.NewDustPerfect(recoilPos, DustID.Smoke, recoilVel, 150, default, 1.5f);
                recoil.noGravity = true;
            }
            
            // Start reload using ModPlayer state
            var modPlayer = player.GetModPlayer<MoonlightAccessoryPlayer>();
            modPlayer.resurrectionIsReloaded = false;
            modPlayer.resurrectionReloadTimer = 0;
            modPlayer.resurrectionPlayedReadySound = false;
            
            return false;
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
