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
    /// Right-click: Lunar Dash — pierce through enemies with constellation trail.
    /// After dash-hit: Next swing is empowered → Lunar Nova explosion + lifesteal.
    /// </summary>
    public class IncisorOfMoonlight : ModItem
    {
        // =====================================================================
        // TUNING CONSTANTS — adjust balance here
        // =====================================================================

        /// <summary>Damage multiplier for non-true-melee projectiles (beams, slashes).</summary>
        public const float NotTrueMeleePenalty = 0.45f;
        /// <summary>Damage multiplier for Lunar Nova explosion.</summary>
        public const float ExplosionDamageFactor = 2.5f;
        /// <summary>Damage multiplier for dash constellation slashes.</summary>
        public const float LungeDamageFactor = 0.65f;
        /// <summary>Travel speed during the lunge portion of a dash.</summary>
        public const float LungeSpeed = 28f;
        /// <summary>Fraction of DashTime spent in the forward lunge.</summary>
        public const float LungePercent = 0.6f;
        /// <summary>Total dash duration in frames (before MaxUpdates multiplier).</summary>
        public const int DashTime = 26;
        /// <summary>Cooldown frames after dash ends before next action.</summary>
        public const int LungeCooldown = 18;
        /// <summary>Frames after dash-hit during which empowered swing is available.</summary>
        public const int OpportunityForBigSlash = 40;
        /// <summary>Rebound velocity magnitude when rebounding off a dash-hit NPC.</summary>
        public const float ReboundSpeed = 8f;
        /// <summary>Scale multiplier for empowered swing arc.</summary>
        public const float BigSlashUpscale = 1.3f;
        /// <summary>Number of homing beams fired per swing.</summary>
        public const int BeamsPerSwing = 3;

        // Sounds
        private static readonly SoundStyle SwingSound = SoundID.Item71 with { PitchVariance = 0.3f, Volume = 0.65f };
        private static readonly SoundStyle DashSound = SoundID.Item163 with { Volume = 0.6f, PitchVariance = 0.2f };

        // Empowered window tracking
        private int empoweredTimer;

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
                "Right-click to dash through enemies with a constellation pierce"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
                "After a successful dash-hit, your next swing is empowered with a Lunar Nova"));
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
            // Block use during dash cooldown
            if (player.ownedProjectileCounts[ModContent.ProjectileType<IncisorSwingProj>()] > 0)
            {
                // Allow channelling existing projectiles
                var proj = Main.projectile.FirstOrDefault(p =>
                    p.active && p.owner == player.whoAmI && p.type == Item.shoot);
                if (proj != default && proj.active)
                    return false;
            }
            return true;
        }

        public override bool CanShoot(Player player)
        {
            // Prevent spawning duplicates — swing projectile is channelled
            return player.ownedProjectileCounts[ModContent.ProjectileType<IncisorSwingProj>()] <= 0;
        }

        public override void HoldItem(Player player)
        {
            var ip = player.Incisor();
            ip.rightClickListener = true;
            ip.mouseWorldListener = true;

            // Tick empowered window
            if (empoweredTimer > 0)
                empoweredTimer--;
        }

        // =====================================================================
        // SHOOT — decides swing state: normal / dash / empowered
        // =====================================================================

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Determine state
            float state = 0; // Normal swing
            if (player.altFunctionUse == 2)
                state = 1; // Lunar Dash

            // Empowered swing (after dash-hit window)
            if (empoweredTimer > 0 && player.altFunctionUse != 2)
            {
                state = 2; // Empowered big swing
                empoweredTimer = 0;
            }

            Projectile.NewProjectile(source, position, velocity, type, damage, knockback,
                player.whoAmI, state, 0f);

            return false;
        }

        /// <summary>Called from dash-hit to grant empowered window.</summary>
        public void GrantEmpoweredWindow()
        {
            empoweredTimer = OpportunityForBigSlash;
        }

        // =====================================================================
        // NO DIRECT HIT — damage is dealt by projectile
        // =====================================================================

        public override bool? CanHitNPC(Player player, NPC target) => false;
        public override bool CanHitPvp(Player player, Player target) => false;

        // =====================================================================
        // RECIPE — same as before
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
