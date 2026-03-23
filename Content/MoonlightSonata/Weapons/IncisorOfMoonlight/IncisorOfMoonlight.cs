using System;
using System.Collections.Generic;
using System.Linq;
using MagnumOpus.Common;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Projectiles;
using MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight
{
    /// <summary>
    /// Incisor of Moonlight — "The Stellar Scalpel".
    /// Moonlight Sonata endgame melee weapon.
    ///
    /// Left-click: Channelled resonance swing — shader-driven slash arc,
    ///             fires homing Lunar Beams, can be held to chain swings.
    /// Right-click: Super Lunar Orb — requires full Lunar Charge meter.
    ///             Homing orb with 3 orbiting sub-orbs, creates lunar zone on impact.
    /// </summary>
    public class IncisorOfMoonlight : ModItem
    {
        // =====================================================================
        // TUNING CONSTANTS
        // =====================================================================

        /// <summary>Damage multiplier for non-true-melee projectiles (beams, slashes).</summary>
        public const float NotTrueMeleePenalty = 0.45f;
        /// <summary>Number of homing beams fired per swing.</summary>
        public const int BeamsPerSwing = 3;
        /// <summary>Damage multiplier for the Super Lunar Orb.</summary>
        public const float SuperOrbDamageFactor = 2.0f;

        // Sounds
        private static readonly SoundStyle SwingSound = SoundID.Item71 with { PitchVariance = 0.3f, Volume = 0.65f };

        // =====================================================================
        // SETUP
        // =====================================================================

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 80;
            Item.height = 80;
            Item.scale = 0.08f;
            Item.damage = 280;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.useTime = 12;
            Item.useAnimation = 12;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6.5f;
            Item.value = Item.sellPrice(gold: 25);
            Item.rare = ModContent.RarityType<MoonlightSonataRarity>();
            Item.autoReuse = true;
            Item.shootSpeed = 8f;

            // Channel-held swing — projectile-based, not vanilla swing
            Item.channel = true;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<IncisorSwingProj>();
        }

        // =====================================================================
        // TOOLTIPS
        // =====================================================================

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1",
                "Channelled swing fires homing lunar beams"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
                "Hits build lunar charge — kills charge significantly more"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
                "Right-click at full charge to unleash a devastating Super Lunar Orb"));
            tooltips.Add(new TooltipLine(Mod, "Effect4",
                "The orb homes in and creates a persistent lunar damage zone on impact"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'A blade forged from crystallized moonlight — each swing traces a constellation'")
            { OverrideColor = new Color(140, 100, 200) });
        }

        // =====================================================================
        // USAGE CONTROL
        // =====================================================================

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            // Block use during existing swing
            if (player.ownedProjectileCounts[ModContent.ProjectileType<IncisorSwingProj>()] > 0)
            {
                var proj = Main.projectile.FirstOrDefault(p =>
                    p.active && p.owner == player.whoAmI && p.type == Item.shoot);
                if (proj != default && proj.active)
                    return false;
            }
            return true;
        }

        public override bool CanShoot(Player player)
        {
            // Right-click: always allow (handled in Shoot)
            if (player.altFunctionUse == 2)
                return true;

            // Left-click: prevent spawning duplicate swing projectiles
            return player.ownedProjectileCounts[ModContent.ProjectileType<IncisorSwingProj>()] <= 0;
        }

        public override void HoldItem(Player player)
        {
            var ip = player.Incisor();
            ip.rightClickListener = true;
            ip.mouseWorldListener = true;
            ip.IsHoldingIncisor = true;
        }

        // =====================================================================
        // SHOOT — decides action: normal swing or Super Lunar Orb
        // =====================================================================

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var ip = player.Incisor();

            if (player.altFunctionUse == 2)
            {
                // Right-click: fire Super Lunar Orb if charge is full
                if (ip.IsChargeFull)
                {
                    ip.ConsumeCharge();
                    int orbDmg = (int)(damage * SuperOrbDamageFactor);
                    Vector2 orbVel = velocity.SafeNormalize(Vector2.UnitX) * 12f;
                    Projectile.NewProjectile(source, position, orbVel,
                        ModContent.ProjectileType<SuperLunarOrbProj>(),
                        orbDmg, knockback, player.whoAmI);
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 0.9f }, position);
                }
                else
                {
                    SoundEngine.PlaySound(SoundID.Item27 with { Pitch = -0.3f, Volume = 0.4f }, position);
                }
                return false;
            }

            // Normal swing
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback,
                player.whoAmI, 0f, 0f);

            return false;
        }

        // =====================================================================
        // NO DIRECT HIT — damage is dealt by projectile
        // =====================================================================

        public override bool? CanHitNPC(Player player, NPC target) => false;
        public override bool CanHitPvp(Player player, Player target) => false;

        // =====================================================================
        // RECIPE
        // =====================================================================

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<MoonlightsResonantEnergy>(), 20)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfMoonlightSonata>(), 10)
                .AddIngredient(ModContent.ItemType<Enemies.ShardsOfMoonlitTempo>(), 25)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }
}
