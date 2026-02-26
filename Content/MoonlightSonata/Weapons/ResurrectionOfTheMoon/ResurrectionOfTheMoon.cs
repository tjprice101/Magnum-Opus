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
using MagnumOpus.Content.MoonlightSonata.Dusts;
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
    ///
    /// Chamber Mechanic (right-click to cycle):
    ///   Chamber 0 — Standard: ResurrectionProjectile, ricochets 10 times with crater detonations
    ///   Chamber 1 — Comet Core: CometCore, pierces through 5 enemies with burning wake
    ///   Chamber 2 — Supernova: SupernovaShell, arcing artillery that detonates in massive AoE
    ///
    /// Has a reloading mechanic with converging charge VFX between shots.
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

        public override bool AltFunctionUse(Player player) => true;

        public override void HoldItem(Player player)
        {
            if (Main.dedServ) return;

            var modPlayer = player.GetModPlayer<MoonlightAccessoryPlayer>();
            float time = Main.GameUpdateCount * 0.04f;

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

                // COMET EMBER charge particles — converge as reload progresses
                if (modPlayer.resurrectionReloadTimer % 6 == 0)
                {
                    float orbitAngle = Main.GameUpdateCount * (0.1f + reloadProgress * 0.15f);
                    for (int i = 0; i < 3; i++)
                    {
                        float angle = orbitAngle + MathHelper.TwoPi * i / 3f;
                        float radius = 25f - reloadProgress * 15f; // Converge inward
                        Vector2 orbitPos = gunPos + angle.ToRotationVector2() * radius;
                        Color chargeColor = Color.Lerp(ResurrectionVFX.DeepSpaceViolet,
                            ResurrectionVFX.LunarShine, reloadProgress);
                        Dust ember = Dust.NewDustPerfect(orbitPos,
                            ModContent.DustType<CometEmberDust>(),
                            (gunPos - orbitPos).SafeNormalize(Vector2.Zero) * (1f + reloadProgress * 2f),
                            0, chargeColor, 0.2f + reloadProgress * 0.15f);
                        ember.customData = new CometEmberBehavior
                        {
                            VelocityDecay = 0.9f,
                            RotationSpeed = 0.08f,
                            BaseScale = 0.2f + reloadProgress * 0.15f,
                            Lifetime = 18,
                            HasGravity = false
                        };
                    }
                }

                // StarPointDust sparkles converging toward barrel
                if (Main.rand.NextBool(4))
                {
                    Vector2 dustStart = gunPos + Main.rand.NextVector2Circular(40f, 40f);
                    Vector2 dustVel = (gunPos - dustStart).SafeNormalize(Vector2.Zero) * (2f + reloadProgress * 3f);
                    Color sparkColor = Color.Lerp(ResurrectionVFX.CometTrail,
                        ResurrectionVFX.CometCore, reloadProgress);
                    Dust star = Dust.NewDustPerfect(dustStart,
                        ModContent.DustType<StarPointDust>(),
                        dustVel, 0, sparkColor, 0.16f + reloadProgress * 0.08f);
                    star.customData = new StarPointBehavior
                    {
                        RotationSpeed = 0.12f,
                        TwinkleFrequency = 0.5f,
                        Lifetime = 22,
                        FadeStartTime = 8
                    };
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
                // === READY STATE — comet ember ambient glow + chamber indicator ===
                Vector2 gunPos = player.Center + new Vector2(35 * player.direction, -5);
                int chamber = modPlayer.resurrectionActiveChamber;

                // Chamber charge indicator VFX — orbiting dust in active chamber color
                ResurrectionVFX.ChamberChargeFrame(gunPos, chamber, 1f);

                // Smoldering CometEmberDust at barrel tip — tinted by active chamber
                if (Main.rand.NextBool(8))
                {
                    Color emberColor = ResurrectionVFX.GetChamberColor(chamber);
                    Dust ember = Dust.NewDustPerfect(
                        gunPos + Main.rand.NextVector2Circular(8f, 8f),
                        ModContent.DustType<CometEmberDust>(),
                        Main.rand.NextVector2Circular(0.5f, 0.5f),
                        0, emberColor, 0.15f);
                    ember.customData = new CometEmberBehavior(0.15f, 20, false);
                }

                // StarPointDust ready twinkle
                if (Main.rand.NextBool(12))
                {
                    Color starColor = Color.Lerp(ResurrectionVFX.GetChamberColor(chamber),
                        ResurrectionVFX.GetChamberGlowColor(chamber), Main.rand.NextFloat());
                    Dust star = Dust.NewDustPerfect(
                        gunPos + Main.rand.NextVector2Circular(10f, 10f),
                        ModContent.DustType<StarPointDust>(),
                        Vector2.Zero, 0, starColor, 0.14f);
                    star.customData = new StarPointBehavior
                    {
                        RotationSpeed = 0.08f,
                        TwinkleFrequency = 0.4f,
                        Lifetime = 22,
                        FadeStartTime = 6
                    };
                }
            }

            // Orbiting CometEmberDust — comet fragments circling player
            if (Main.rand.NextBool(7))
            {
                float orbitAngle = time + Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = 22f + MathF.Sin(time * 2f + Main.rand.NextFloat()) * 6f;
                Vector2 orbitPos = player.Center + orbitAngle.ToRotationVector2() * radius;
                Color orbitColor = ResurrectionVFX.GetCometColor(
                    (MathF.Sin(orbitAngle * 2f) + 1f) * 0.5f, 0);
                Dust ember = Dust.NewDustPerfect(orbitPos,
                    ModContent.DustType<CometEmberDust>(),
                    Vector2.Zero, 0, orbitColor, 0.18f);
                ember.customData = new CometEmberBehavior
                {
                    VelocityDecay = 0.96f,
                    RotationSpeed = 0.06f,
                    BaseScale = 0.18f,
                    Lifetime = 22,
                    HasGravity = false
                };
            }

            // LunarMote crescent notes — 2 orbiting crescents
            if (Main.rand.NextBool(10))
            {
                float moteAngle = time * 0.8f;
                for (int i = 0; i < 2; i++)
                {
                    float noteAngle = moteAngle + MathHelper.Pi * i;
                    float noteRadius = 26f + MathF.Sin(time * 1.2f + i * 0.7f) * 5f;
                    Vector2 notePos = player.Center + noteAngle.ToRotationVector2() * noteRadius;
                    Color moteColor = Color.Lerp(ResurrectionVFX.CometTrail,
                        MoonlightVFXLibrary.IceBlue, (float)i / 2f);
                    Dust mote = Dust.NewDustPerfect(notePos,
                        ModContent.DustType<LunarMote>(),
                        Vector2.Zero, 0, moteColor, 0.22f);
                    mote.customData = new LunarMoteBehavior(player.Center, noteAngle)
                    {
                        OrbitRadius = noteRadius,
                        OrbitSpeed = 0.04f,
                        Lifetime = 28,
                        FadePower = 0.92f
                    };
                }
            }

            // StarPointDust twinkles around player
            if (Main.rand.NextBool(12))
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                Color starColor = Color.Lerp(ResurrectionVFX.LunarShine,
                    ResurrectionVFX.CometCore, Main.rand.NextFloat());
                Dust star = Dust.NewDustPerfect(player.Center + offset,
                    ModContent.DustType<StarPointDust>(),
                    Vector2.Zero, 0, starColor, 0.16f);
                star.customData = new StarPointBehavior
                {
                    RotationSpeed = 0.08f,
                    TwinkleFrequency = 0.4f,
                    Lifetime = 24,
                    FadeStartTime = 7
                };
            }

            // Music notes
            if (Main.rand.NextBool(10))
            {
                float noteOrbitAngle = time * 0.8f;
                for (int i = 0; i < 2; i++)
                {
                    float noteAngle = noteOrbitAngle + MathHelper.TwoPi * i / 2f;
                    Vector2 notePos = player.Center + noteAngle.ToRotationVector2() * 28f;
                    MoonlightVFXLibrary.SpawnMusicNotes(notePos, 1, 2f, 0.75f, 0.9f, 40);
                }
            }

            // Pulsing comet glow
            float lightPulse = 0.5f + MathF.Sin(time * 1.5f) * 0.15f;
            Color lightColor = Color.Lerp(ResurrectionVFX.CometTrail,
                ResurrectionVFX.LunarShine,
                MathF.Sin(time * 0.7f) * 0.5f + 0.5f);
            Lighting.AddLight(player.Center, lightColor.ToVector3() * lightPulse * 0.45f);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor,
            ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() * 0.5f;
            float pulse = 1f + MathF.Sin(Main.GameUpdateCount * 0.05f) * 0.1f;

            // 5-layer bloom using {A=0} — comet palette

            // Layer 1: Outer deep space halo (cycling color)
            Color outerColor = ResurrectionVFX.GetCometColor(
                Main.GlobalTimeWrappedHourly % 1f, 0);
            spriteBatch.Draw(texture, position, null,
                (outerColor with { A = 0 }) * 0.2f,
                rotation, origin, scale * pulse * 1.4f, SpriteEffects.None, 0f);

            // Layer 2: Deep space violet halo
            spriteBatch.Draw(texture, position, null,
                (ResurrectionVFX.DeepSpaceViolet with { A = 0 }) * 0.3f,
                rotation, origin, scale * pulse * 1.25f, SpriteEffects.None, 0f);

            // Layer 3: Comet trail violet glow
            spriteBatch.Draw(texture, position, null,
                (ResurrectionVFX.CometTrail with { A = 0 }) * 0.35f,
                rotation, origin, scale * pulse * 1.12f, SpriteEffects.None, 0f);

            // Layer 4: Moonrise gold inner
            spriteBatch.Draw(texture, position, null,
                (ResurrectionVFX.LunarShine with { A = 0 }) * 0.4f,
                rotation, origin, scale * pulse * 1.05f, SpriteEffects.None, 0f);

            // Layer 5: White-hot core
            spriteBatch.Draw(texture, position, null,
                (Color.White with { A = 0 }) * 0.25f,
                rotation, origin, scale * pulse, SpriteEffects.None, 0f);

            Lighting.AddLight(Item.Center, ResurrectionVFX.CometTrail.ToVector3() * 0.4f);

            return true;
        }

        public override bool CanUseItem(Player player)
        {
            // Alt-fire (right-click) cycles chambers — always allowed
            if (player.altFunctionUse == 2)
            {
                Item.useTime = 20;
                Item.useAnimation = 20;
                Item.useAmmo = AmmoID.None;
                Item.UseSound = null;
                return true;
            }

            // Primary fire requires reload
            Item.useTime = 90;
            Item.useAnimation = 90;
            Item.useAmmo = AmmoID.Bullet;
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
            var modPlayer = player.GetModPlayer<MoonlightAccessoryPlayer>();

            // === ALT-FIRE: CHAMBER SWITCH ===
            if (player.altFunctionUse == 2)
            {
                int oldChamber = modPlayer.resurrectionActiveChamber;
                modPlayer.resurrectionActiveChamber = (modPlayer.resurrectionActiveChamber + 1) % ResurrectionVFX.ChamberCount;
                int newChamber = modPlayer.resurrectionActiveChamber;

                // Chamber switch VFX + sound
                Vector2 gunPos = player.Center + new Vector2(35 * player.direction, -5);
                ResurrectionVFX.ChamberSwitchVFX(gunPos, oldChamber, newChamber);
                SoundEngine.PlaySound(SoundID.Unlock with { Volume = 0.7f, Pitch = 0.3f + newChamber * 0.2f }, gunPos);

                return false;
            }

            // === PRIMARY FIRE: CHAMBER-SPECIFIC PROJECTILE ===
            int activeChamber = modPlayer.resurrectionActiveChamber;
            int projectileType = activeChamber switch
            {
                ResurrectionVFX.ChamberCometCore => ModContent.ProjectileType<CometCore>(),
                ResurrectionVFX.ChamberSupernova => ModContent.ProjectileType<SupernovaShell>(),
                _ => ModContent.ProjectileType<ResurrectionProjectile>()
            };

            // Damage modifier per chamber
            int chamberDamage = activeChamber switch
            {
                ResurrectionVFX.ChamberCometCore => (int)(damage * 0.7f),  // Lower per-hit, but pierces 5
                ResurrectionVFX.ChamberSupernova => (int)(damage * 1.5f),  // Heavy single hit + AoE
                _ => damage
            };

            // Speed modifier per chamber
            Vector2 chamberVelocity = activeChamber switch
            {
                ResurrectionVFX.ChamberCometCore => velocity * 1.3f,  // Faster penetrating round
                ResurrectionVFX.ChamberSupernova => velocity * 0.7f,  // Slower artillery shell
                _ => velocity
            };

            Projectile.NewProjectile(source, position, chamberVelocity,
                projectileType, chamberDamage, knockback, player.whoAmI);

            // Powerful shot sounds — pitch varies by chamber
            float pitchOffset = activeChamber * 0.15f;
            SoundEngine.PlaySound(SoundID.Item40 with { Volume = 1.2f, Pitch = -0.5f + pitchOffset }, position);
            SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.7f, Pitch = -0.3f + pitchOffset }, position);

            Vector2 muzzlePos = position + velocity.SafeNormalize(Vector2.Zero) * 45f;
            Vector2 direction = velocity.SafeNormalize(Vector2.UnitX);

            // Massive muzzle flash via ResurrectionVFX
            ResurrectionVFX.MuzzleFlash(muzzlePos, direction);
            ResurrectionVFX.ChamberMuzzleAccent(muzzlePos, direction, activeChamber);

            // CometEmberDust recoil blast behind player
            Color recoilColor = ResurrectionVFX.GetChamberColor(activeChamber);
            Vector2 recoilPos = player.Center - direction * 20f;
            for (int i = 0; i < 8; i++)
            {
                Vector2 recoilVel = -direction * Main.rand.NextFloat(3f, 7f) + Main.rand.NextVector2Circular(2f, 2f);
                Dust ember = Dust.NewDustPerfect(recoilPos,
                    ModContent.DustType<CometEmberDust>(),
                    recoilVel, 0, recoilColor, 0.25f);
                ember.customData = new CometEmberBehavior
                {
                    VelocityDecay = 0.93f,
                    RotationSpeed = 0.08f,
                    BaseScale = 0.25f,
                    Lifetime = 22,
                    HasGravity = true
                };
            }

            // Start reload
            modPlayer.resurrectionIsReloaded = false;
            modPlayer.resurrectionReloadTimer = 0;
            modPlayer.resurrectionPlayedReadySound = false;

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var modPlayer = player.GetModPlayer<MoonlightAccessoryPlayer>();
            int activeChamber = modPlayer.resurrectionActiveChamber;

            tooltips.Add(new TooltipLine(Mod, "DevastatingShot",
                "Fires a devastating moonlight round with shattering lunar force")
            { OverrideColor = ResurrectionVFX.CometTrail });

            // Chamber mechanic description
            tooltips.Add(new TooltipLine(Mod, "ChamberMechanic",
                "Right-click to cycle between three chamber types:")
            { OverrideColor = new Color(200, 200, 200) });

            // Chamber descriptions with active indicator
            string standardMarker = activeChamber == ResurrectionVFX.ChamberStandard ? " [Active]" : "";
            tooltips.Add(new TooltipLine(Mod, "ChamberStandard",
                $"  Standard — Ricochets 10 times with escalating crater detonations{standardMarker}")
            { OverrideColor = activeChamber == ResurrectionVFX.ChamberStandard
                ? ResurrectionVFX.CometTrail : new Color(160, 140, 200) });

            string cometMarker = activeChamber == ResurrectionVFX.ChamberCometCore ? " [Active]" : "";
            tooltips.Add(new TooltipLine(Mod, "ChamberCometCore",
                $"  Comet Core — Pierces through 5 enemies with burning ember wake{cometMarker}")
            { OverrideColor = activeChamber == ResurrectionVFX.ChamberCometCore
                ? ResurrectionVFX.CometCore : new Color(160, 170, 200) });

            string supernovaMarker = activeChamber == ResurrectionVFX.ChamberSupernova ? " [Active]" : "";
            tooltips.Add(new TooltipLine(Mod, "ChamberSupernova",
                $"  Supernova — Arcing artillery shell that detonates in massive AoE{supernovaMarker}")
            { OverrideColor = activeChamber == ResurrectionVFX.ChamberSupernova
                ? ResurrectionVFX.SupernovaWhite : new Color(170, 170, 200) });

            tooltips.Add(new TooltipLine(Mod, "ReloadMechanic",
                "Requires reloading between shots — the final movement demands patience")
            { OverrideColor = new Color(200, 200, 200) });

            // Moonlit Gyre synergy
            if (modPlayer.hasMoonlitGyre)
            {
                tooltips.Add(new TooltipLine(Mod, "GyreSynergy", "Moonlit Gyre: +25% damage, +15% velocity")
                { OverrideColor = new Color(100, 255, 150) });
            }

            // Reload status
            if (!modPlayer.resurrectionIsReloaded)
            {
                float reloadPercent = (float)modPlayer.resurrectionReloadTimer / MoonlightAccessoryPlayer.ResurrectionReloadTime * 100f;
                tooltips.Add(new TooltipLine(Mod, "ReloadStatus", $"Reloading... {reloadPercent:F0}%")
                { OverrideColor = ResurrectionVFX.LunarShine });
            }
            else
            {
                tooltips.Add(new TooltipLine(Mod, "ReloadReady", "Ready to fire!")
                { OverrideColor = new Color(100, 255, 100) });
            }

            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'From death comes rebirth in silver light — the final movement that silences all'")
            { OverrideColor = new Color(140, 100, 200) });
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
