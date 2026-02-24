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
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.MoonlightSonata;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.Enemies;
using MagnumOpus.Content.MoonlightSonata.Projectiles;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Content.MoonlightSonata.Accessories;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon
{
    /// <summary>
    /// Resurrection of the Moon — "The Final Movement".
    /// A devastating moonlight sniper rifle with heavy astronomical impact.
    /// Fires slowly but deals massive damage with comet-like projectiles.
    /// Bullets ricochet 10 times to nearby enemies with crater detonations.
    /// Has a reloading mechanic with converging charge VFX.
    /// </summary>
    public class ResurrectionOfTheMoon : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 70;
            Item.height = 26;
            Item.damage = 1500;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 90;
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
            if (Main.dedServ) return;

            var modPlayer = player.GetModPlayer<MoonlightAccessoryPlayer>();

            // === RELOAD PHASE ===
            if (!modPlayer.resurrectionIsReloaded)
            {
                modPlayer.resurrectionReloadTimer++;

                // Play reload sound at the start
                if (modPlayer.resurrectionReloadTimer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item149 with { Volume = 0.8f, Pitch = -0.3f }, player.Center);
                    modPlayer.resurrectionPlayedReadySound = false;
                }

                float reloadProgress = (float)modPlayer.resurrectionReloadTimer / MoonlightAccessoryPlayer.ResurrectionReloadTime;
                Vector2 gunPos = player.Center + new Vector2(35 * player.direction, -5);

                // Orbiting charge particles — converge as reload progresses
                if (modPlayer.resurrectionReloadTimer % 8 == 0)
                {
                    float orbitAngle = Main.GameUpdateCount * (0.1f + reloadProgress * 0.15f);
                    for (int i = 0; i < 3; i++)
                    {
                        float angle = orbitAngle + MathHelper.TwoPi * i / 3f;
                        float radius = 15f + reloadProgress * 8f;
                        Vector2 orbitPos = gunPos + angle.ToRotationVector2() * radius;
                        Color chargeColor = Color.Lerp(MoonlightVFXLibrary.DarkPurple, MoonlightVFXLibrary.IceBlue, reloadProgress);
                        CustomParticles.GenericFlare(orbitPos, chargeColor * (0.4f + reloadProgress * 0.5f), 0.2f + reloadProgress * 0.15f, 12);
                    }
                }

                // Charging particles flowing toward gun barrel
                if (Main.rand.NextBool(4))
                {
                    Vector2 dustStart = gunPos + Main.rand.NextVector2Circular(40f, 40f);
                    Vector2 dustVel = (gunPos - dustStart).SafeNormalize(Vector2.Zero) * 2f;
                    Color chargeColor = Color.Lerp(MoonlightVFXLibrary.Violet, MoonlightVFXLibrary.MoonWhite, reloadProgress);
                    var spark = new SparkleParticle(dustStart, dustVel, chargeColor, 0.18f + reloadProgress * 0.12f, 18);
                    MagnumParticleHandler.SpawnParticle(spark);
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

                        // Ready flash burst via ResurrectionVFX
                        ResurrectionVFX.ReadyFlash(gunPos);
                    }
                }
            }
            else
            {
                // === READY STATE — subtle ambient glow ===
                if (Main.rand.NextBool(8))
                {
                    Vector2 gunPos = player.Center + new Vector2(35 * player.direction, -5);
                    CustomParticles.GenericFlare(gunPos + Main.rand.NextVector2Circular(10f, 10f),
                        MoonlightVFXLibrary.MoonWhite * 0.6f, 0.2f, 15);
                }
            }

            // Ambient pulsing glow
            float pulse = MathF.Sin(Main.GameUpdateCount * 0.05f) * 0.1f + 0.9f;
            Lighting.AddLight(player.Center, MoonlightVFXLibrary.Violet.ToVector3() * pulse * 0.5f);

            // Ethereal ambient particles
            if (Main.rand.NextBool(6))
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                Color dustColor = Color.Lerp(MoonlightVFXLibrary.DarkPurple, MoonlightVFXLibrary.IceBlue, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(player.Center + offset, DustID.PurpleTorch,
                    Main.rand.NextVector2Circular(0.5f, 0.5f), 100, dustColor, 1.0f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor,
            ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() * 0.5f;
            float pulse = 1f + MathF.Sin(Main.GameUpdateCount * 0.05f) * 0.1f;

            // 4-layer bloom using {A=0} — no SpriteBatch restart needed

            // Layer 1: Outer deep purple aura
            spriteBatch.Draw(texture, position, null,
                (MoonlightVFXLibrary.DarkPurple with { A = 0 }) * 0.35f,
                rotation, origin, scale * pulse * 1.4f, SpriteEffects.None, 0f);

            // Layer 2: Mid violet glow
            spriteBatch.Draw(texture, position, null,
                (MoonlightVFXLibrary.Violet with { A = 0 }) * 0.30f,
                rotation, origin, scale * pulse * 1.2f, SpriteEffects.None, 0f);

            // Layer 3: Inner ice blue
            spriteBatch.Draw(texture, position, null,
                (MoonlightVFXLibrary.IceBlue with { A = 0 }) * 0.25f,
                rotation, origin, scale * pulse * 1.08f, SpriteEffects.None, 0f);

            // Layer 4: White-hot core
            spriteBatch.Draw(texture, position, null,
                (Color.White with { A = 0 }) * 0.20f,
                rotation, origin, scale * pulse, SpriteEffects.None, 0f);

            Lighting.AddLight(Item.Center, MoonlightVFXLibrary.Violet.ToVector3() * 0.5f);

            return true;
        }

        public override bool CanUseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<MoonlightAccessoryPlayer>();
            return modPlayer.resurrectionIsReloaded;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity,
            ref int type, ref int damage, ref float knockback)
        {
            var modPlayer = player.GetModPlayer<MoonlightAccessoryPlayer>();

            // Moonlit Gyre synergy
            if (modPlayer.hasMoonlitGyre)
            {
                damage = (int)(damage * 1.25f);
                velocity *= 1.15f;
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position,
            Vector2 velocity, int type, int damage, float knockback)
        {
            // Fire custom projectile
            Projectile.NewProjectile(source, position, velocity,
                ModContent.ProjectileType<ResurrectionProjectile>(), damage, knockback, player.whoAmI);

            // Powerful shot sounds
            SoundEngine.PlaySound(SoundID.Item40 with { Volume = 1.2f, Pitch = -0.5f }, position);
            SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.7f, Pitch = -0.3f }, position);

            Vector2 muzzlePos = position + velocity.SafeNormalize(Vector2.Zero) * 45f;
            Vector2 direction = velocity.SafeNormalize(Vector2.UnitX);

            // Massive muzzle flash via ResurrectionVFX
            ResurrectionVFX.MuzzleFlash(muzzlePos, direction);

            // Recoil dust behind player
            Vector2 recoilPos = player.Center - direction * 20f;
            for (int i = 0; i < 10; i++)
            {
                Vector2 recoilVel = -direction * Main.rand.NextFloat(3f, 6f) + Main.rand.NextVector2Circular(2f, 2f);
                Dust recoil = Dust.NewDustPerfect(recoilPos, DustID.Smoke, recoilVel, 150, default, 1.5f);
                recoil.noGravity = true;
            }

            // Start reload
            var modPlayer = player.GetModPlayer<MoonlightAccessoryPlayer>();
            modPlayer.resurrectionIsReloaded = false;
            modPlayer.resurrectionReloadTimer = 0;
            modPlayer.resurrectionPlayedReadySound = false;

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
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

            // Moonlit Gyre synergy
            if (modPlayer.hasMoonlitGyre)
            {
                tooltips.Add(new TooltipLine(Mod, "GyreSynergy", "Moonlit Gyre: +25% damage, +15% velocity")
                {
                    OverrideColor = new Color(100, 255, 150)
                });
            }

            // Reload status
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
