using System.Linq;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Content.MoonlightSonata.Enemies;
using MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Projectiles;
using MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Utilities;
using static Terraria.ModLoader.ModContent;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling
{
    /// <summary>
    /// Moonlight's Calling — "The Serenade".
    /// 
    /// A prismatic beam magic weapon channeling refracted moonlight.
    /// Left-click: rapid-fire bouncing prismatic beams that split into spectral
    ///   child beams after 3+ bounces and detonate on the 5th bounce.
    /// Right-click: Serenade Mode — a devastating channeled mega-beam that
    ///   pierces enemies and walls with full spectral chromatic aberration.
    ///   40 mana cost, 3 second channel, 3 second cooldown.
    /// 
    /// Self-contained weapon system with own primitive renderer, particle system,
    /// shader loader, utility library, debuff, dust, and ModPlayer.
    /// </summary>
    public class MoonlightsCalling : ModItem
    {
        // === BALANCE CONSTANTS ===
        public const int SerenadeManaCost = 40;
        public const float SerenadeDamageMultiplier = 1.8f;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 30;
            Item.damage = 200;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 8;
            Item.useTime = 12;
            Item.useAnimation = 12;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3f;
            Item.value = Item.buyPrice(gold: 25);
            Item.rare = RarityType<MoonlightSonataRarity>();
            Item.UseSound = SoundID.Item72;
            Item.autoReuse = true;
            Item.shoot = ProjectileType<SerenadeBeam>();
            Item.shootSpeed = 16f;
            Item.noMelee = true;
            Item.staff[Item.type] = true;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override void HoldItem(Player player)
        {
            player.Serenade().RightClickListener = true;
            player.Serenade().MouseWorldListener = true;
        }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // Serenade Mode: check cooldown and mana
                if (!player.Serenade().CanSerenade)
                    return false;

                // Don't stack holdout projectiles
                if (Main.projectile.Any(p => p.active && p.owner == player.whoAmI
                    && p.type == ProjectileType<SerenadeHoldout>()))
                    return false;

                // Check mana upfront for full cost
                if (player.statMana < SerenadeManaCost)
                    return false;

                // Configure as channeled holdout
                Item.channel = true;
                Item.noUseGraphic = true;
                Item.useStyle = ItemUseStyleID.Shoot;
            }
            else
            {
                // Normal mode: rapid-fire beams
                Item.channel = false;
                Item.noUseGraphic = false;
            }

            return base.CanUseItem(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                // === SERENADE MODE ===
                // Consume extra mana
                player.statMana -= SerenadeManaCost;
                if (player.statMana < 0) player.statMana = 0;
                player.manaRegenDelay = 120;

                player.Serenade().SerenadeActive = true;

                // Spawn holdout beam
                Projectile.NewProjectile(source, position, velocity,
                    ProjectileType<SerenadeHoldout>(),
                    (int)(damage * SerenadeDamageMultiplier), knockback * 1.5f,
                    player.whoAmI);

                SoundEngine.PlaySound(SoundID.Item164 with { Pitch = 0.4f, Volume = 0.8f }, position);
                return false;
            }
            else
            {
                // === NORMAL MODE: Rapid-fire bouncing beams ===
                // Slight spread for visual variety
                float spread = MathHelper.ToRadians(5f);
                Vector2 fireVel = velocity.RotatedByRandom(spread);

                Projectile.NewProjectile(source, position, fireVel, type,
                    damage, knockback, player.whoAmI);

                return false;
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var player = Main.LocalPlayer;
            var serenade = player.Serenade();

            tooltips.Add(new TooltipLine(Mod, "Effect1",
                "Rapid-fire bouncing prismatic beams with spectral refraction"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
                "Each bounce intensifies the spectral cascade — after 3 bounces, beams split into spectral children"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
                "Final bounce detonates into a full prismatic explosion with god rays"));
            tooltips.Add(new TooltipLine(Mod, "Effect4",
                $"Right-click: Serenade Mode — channeled prismatic mega-beam ({SerenadeManaCost} mana, 3s cooldown)"));
            tooltips.Add(new TooltipLine(Mod, "Effect5",
                "Serenade builds resonance through 5 stages — harmonic nodes deal 1.5x damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect6",
                "Inflicts Musical Dissonance on enemies"));

            if (serenade.SerenadeActive && serenade.ResonanceLevel > 0)
            {
                string resName = SerenadePlayer.ResonanceLevelNames[serenade.ResonanceLevel];
                Color resColor = SerenadePlayer.ResonanceColors[serenade.ResonanceLevel];
                tooltips.Add(new TooltipLine(Mod, "Resonance",
                    $"Resonance: {resName}")
                { OverrideColor = resColor });
            }

            if (serenade.SerenadeCooldown > 0)
            {
                float seconds = serenade.SerenadeCooldown / 60f;
                tooltips.Add(new TooltipLine(Mod, "Cooldown",
                    $"Serenade cooldown: {seconds:F1}s")
                { OverrideColor = new Color(255, 100, 100) });
            }

            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'She called to the moon, and the moon wept silver.'")
            { OverrideColor = new Color(140, 100, 200) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<MoonlightsResonantEnergy>(), 15)
                .AddIngredient(ItemType<ResonantCoreOfMoonlightSonata>(), 5)
                .AddIngredient(ItemType<ShardsOfMoonlitTempo>(), 10)
                .AddTile(TileType<MoonlightAnvilTile>())
                .Register();
        }
    }
}
