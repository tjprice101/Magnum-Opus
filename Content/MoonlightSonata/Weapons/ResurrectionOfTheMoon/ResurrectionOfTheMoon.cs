using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Content.MoonlightSonata.Enemies;
using MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon.Utilities;
using MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon.Projectiles;
using MagnumOpus.Content.MoonlightSonata.Accessories;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon
{
    /// <summary>
    /// Resurrection of the Moon — "The Final Movement".
    /// A devastating moonlight ranged weapon with 3 switchable chambers:
    ///   Standard — Ricochets 10 times with escalating crater detonations
    ///   Comet Core — Pierces through 5 enemies with burning ember wake
    ///   Supernova — Arcing artillery that detonates in massive AoE
    /// Right-click cycles chambers. Requires reload between shots.
    /// Synergizes with Moonlit Gyre: +25% damage, +15% velocity.
    /// </summary>
    public class ResurrectionOfTheMoon : ModItem
    {
        // =================================================================
        // CHAMBER STATS
        // =================================================================

        /// <summary>Base damage per chamber (index 0=Standard, 1=CometCore, 2=Supernova).</summary>
        private static readonly float[] ChamberDamageMultiplier = { 1.0f, 1.2f, 0.8f };

        /// <summary>Shoot speed per chamber.</summary>
        private static readonly float[] ChamberShootSpeed = { 24f, 30f, 16f };

        /// <summary>UseTime per chamber (before reload).</summary>
        private static readonly int[] ChamberUseTime = { 30, 25, 40 };

        /// <summary>Synergy bonus — Moonlit Gyre damage multiplier.</summary>
        private const float GyreDamageBonus = 1.25f;

        /// <summary>Synergy bonus — Moonlit Gyre velocity multiplier.</summary>
        private const float GyreVelocityBonus = 1.15f;

        // =================================================================
        // SETUP
        // =================================================================

        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 70;
            Item.height = 26;
            Item.damage = 1500;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 12f;
            Item.value = Item.buyPrice(platinum: 3);
            Item.rare = ModContent.RarityType<MoonlightSonataRarity>();
            Item.UseSound = SoundID.Item40;
            Item.autoReuse = false;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<ResurrectionProjectile>();
            Item.shootSpeed = 24f;
            Item.useAmmo = AmmoID.Bullet;
        }

        // =================================================================
        // ALT FIRE — CHAMBER CYCLING
        // =================================================================

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            var cp = player.Resurrection();

            if (player.altFunctionUse == 2)
            {
                // Right click = cycle chamber (instant, no ammo, no projectile)
                Item.useTime = 12;
                Item.useAnimation = 12;
                Item.UseSound = SoundID.MenuTick;
                Item.reuseDelay = 0;
                return true;
            }

            // Left click = fire
            if (!cp.CanFire) return false;

            int chamber = cp.ActiveChamber;
            Item.useTime = ChamberUseTime[chamber];
            Item.useAnimation = ChamberUseTime[chamber];
            Item.reuseDelay = CometPlayer.ReloadTimes[chamber];
            Item.UseSound = chamber switch
            {
                1 => SoundID.Item40 with { Pitch = 0.2f },   // Comet Core — higher pitch
                2 => SoundID.Item14 with { Pitch = -0.3f },   // Supernova — deep boom
                _ => SoundID.Item40                             // Standard
            };

            return base.CanUseItem(player);
        }

        public override bool CanConsumeAmmo(Item ammo, Player player)
        {
            // Don't consume ammo on chamber cycling
            if (player.altFunctionUse == 2)
                return false;

            // 15% ammo save chance base
            if (Main.rand.NextFloat() < 0.15f)
                return false;

            return true;
        }

        // =================================================================
        // SHOOT
        // =================================================================

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity,
            ref int type, ref int damage, ref float knockback)
        {
            if (player.altFunctionUse == 2) return;

            var cp = player.Resurrection();
            int chamber = cp.ActiveChamber;

            // Apply chamber damage multiplier
            damage = (int)(damage * ChamberDamageMultiplier[chamber]);

            // Apply lunar cycle damage multiplier
            damage = (int)(damage * CometPlayer.LunarPhaseDamageMultiplier[cp.LunarCyclePhase]);

            // Apply chamber velocity
            velocity = velocity.SafeNormalize(Vector2.UnitX) * ChamberShootSpeed[chamber];

            // Moonlit Gyre synergy
            var accessoryPlayer = player.GetModPlayer<MoonlightAccessoryPlayer>();
            if (accessoryPlayer.hasMoonlitGyre)
            {
                damage = (int)(damage * GyreDamageBonus);
                velocity *= GyreVelocityBonus;
            }

            // Gun tip offset
            position += velocity.SafeNormalize(Vector2.UnitX) * 40f;
        }

        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var cp = player.Resurrection();

            if (player.altFunctionUse == 2)
            {
                // Chamber cycling — no projectile
                cp.CycleNextChamber();
                CombatText.NewText(player.getRect(), GetChamberColor(cp.ActiveChamber),
                    CometPlayer.ChamberNames[cp.ActiveChamber], true);
                return false;
            }

            // Fire the appropriate projectile based on chamber
            int chamber = cp.ActiveChamber;
            int projType = chamber switch
            {
                1 => ModContent.ProjectileType<CometCore>(),
                2 => ModContent.ProjectileType<SupernovaShell>(),
                _ => ModContent.ProjectileType<ResurrectionProjectile>()
            };

            Projectile.NewProjectile(source, position, velocity, projType,
                damage, knockback, player.whoAmI);

            // Start reload
            cp.StartReload();

            // Advance lunar cycle
            cp.AdvanceLunarCycle();

            // Muzzle flash VFX with lunar phase indicator
            SpawnMuzzleFlash(player, position, velocity);

            // Recoil
            float recoilForce = chamber switch
            {
                2 => -8f,  // Supernova — heavy recoil
                1 => -4f,  // Comet Core — moderate recoil
                _ => -3f   // Standard — light recoil
            };
            player.velocity += velocity.SafeNormalize(Vector2.UnitX) * recoilForce;

            return false;
        }

        private void SpawnMuzzleFlash(Player player, Vector2 position, Vector2 velocity)
        {
            if (Main.dedServ) return;

            var cp = player.Resurrection();
            Color flashColor = GetChamberColor(cp.ActiveChamber);
            Color lunarColor = cp.CurrentLunarColor;

            // Muzzle bloom — blended with lunar phase color
            Color blendedFlash = Color.Lerp(flashColor, lunarColor, 0.35f);
            Particles.CometParticleHandler.Spawn(new Particles.CraterBloomParticle(
                position, blendedFlash, 0.8f, 10));

            // Lunar phase indicator ring — shows which phase the shot was in
            Particles.CometParticleHandler.Spawn(new Particles.LunarCycleRingParticle(
                position, lunarColor, 1.2f, 18));

            // Second ring for Full Moon — extra dramatic
            if (cp.IsFullMoon)
            {
                Particles.CometParticleHandler.Spawn(new Particles.LunarCycleRingParticle(
                    position, CometUtils.FrigidImpact, 1.8f, 25));
                Particles.CometParticleHandler.Spawn(new Particles.CraterBloomParticle(
                    position, CometUtils.FrigidImpact, 1.5f, 15));
            }

            // Muzzle sparks — count and intensity vary by lunar phase
            int sparkCount = 6 + cp.LunarCyclePhase * 2;
            for (int i = 0; i < sparkCount; i++)
            {
                Vector2 sparkVel = CometUtils.RandomConeDirection(velocity.SafeNormalize(Vector2.UnitX) * 5f,
                    MathHelper.ToRadians(25f));
                Color sparkColor = Color.Lerp(flashColor, lunarColor, Main.rand.NextFloat(0.4f));
                Particles.CometParticleHandler.Spawn(new Particles.EmberTrailParticle(
                    position, sparkVel, 0.3f + Main.rand.NextFloat(0.2f), 10 + Main.rand.Next(8)));
            }

            // Music notes on muzzle flash — the gun sings with each shot
            int noteCount = 1 + (cp.IsFullMoon ? 3 : cp.LunarCyclePhase);
            MoonlightVFXLibrary.SpawnMusicNotes(position, count: noteCount,
                spread: 15f + cp.LunarCyclePhase * 5f, minScale: 0.4f, maxScale: 0.7f,
                lifetime: 25 + cp.LunarCyclePhase * 5);

            // Dust burst from barrel
            for (int i = 0; i < 4; i++)
            {
                Vector2 dustVel = CometUtils.RandomConeDirection(velocity.SafeNormalize(Vector2.UnitX) * 4f,
                    MathHelper.ToRadians(30f));
                int d = Dust.NewDust(position, 4, 4,
                    ModContent.DustType<Dusts.CometDust>(), dustVel.X, dustVel.Y);
                Main.dust[d].noGravity = true;
                Main.dust[d].scale = 1.0f;
            }
        }

        private static Color GetChamberColor(int chamber)
        {
            return chamber switch
            {
                1 => CometUtils.CometCoreColor,    // Gold-white for Comet Core
                2 => CometUtils.SupernovaColor,     // Deep violet for Supernova
                _ => CometUtils.StandardRoundColor   // Crater violet-white for Standard
            };
        }

        // =================================================================
        // TOOLTIPS
        // =================================================================

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var cp = player.Resurrection();

            // Chamber info
            string chamberName = CometPlayer.ChamberNames[cp.ActiveChamber];
            Color chamberColor = GetChamberColor(cp.ActiveChamber);
            tooltips.Add(new TooltipLine(Mod, "Chamber",
                $"Active Chamber: {chamberName}")
            { OverrideColor = chamberColor });

            // Lunar cycle phase info
            string phaseName = CometPlayer.LunarPhaseNames[cp.LunarCyclePhase];
            Color phaseColor = CometPlayer.LunarPhaseColors[cp.LunarCyclePhase];
            tooltips.Add(new TooltipLine(Mod, "LunarPhase",
                $"Lunar Phase: {phaseName}")
            { OverrideColor = phaseColor });

            // Effect descriptions
            tooltips.Add(new TooltipLine(Mod, "Effect1",
                "Fires a devastating moonlight round with shattering lunar force"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
                "Right-click to cycle between three chamber types:"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
                "  Standard — Ricochets 10 times with escalating crater detonations")
            { OverrideColor = CometUtils.StandardRoundColor });
            tooltips.Add(new TooltipLine(Mod, "Effect4",
                "  Comet Core — Pierces through 5 enemies with burning ember wake")
            { OverrideColor = CometUtils.CometCoreColor });
            tooltips.Add(new TooltipLine(Mod, "Effect5",
                "  Supernova — Arcing artillery that detonates in massive AoE")
            { OverrideColor = CometUtils.SupernovaColor });
            tooltips.Add(new TooltipLine(Mod, "Effect6",
                "Each shot advances the Lunar Cycle — New Moon, Waxing, Full Moon, Waning")
            { OverrideColor = new Color(160, 140, 220) });
            tooltips.Add(new TooltipLine(Mod, "Effect7",
                "Full Moon shots deal 30% more damage with 50% increased AoE")
            { OverrideColor = new Color(220, 230, 255) });

            // Reload status
            if (cp.IsReloading)
            {
                int percent = (int)(cp.ReloadProgress * 100f);
                tooltips.Add(new TooltipLine(Mod, "Reload",
                    $"Reloading... {percent}%")
                { OverrideColor = Color.Gray });
            }

            // Moonlit Gyre synergy
            var accessoryPlayer = player.GetModPlayer<MoonlightAccessoryPlayer>();
            if (accessoryPlayer.hasMoonlitGyre)
            {
                tooltips.Add(new TooltipLine(Mod, "Synergy",
                    "Moonlit Gyre: +25% damage, +15% velocity")
                { OverrideColor = new Color(120, 190, 255) });
            }

            // Lore
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'From death comes rebirth in silver light — the final movement that silences all'")
            { OverrideColor = new Color(140, 100, 200) });
        }

        // =================================================================
        // RECIPE (preserved from original)
        // =================================================================

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
