using System.Linq;
using MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon.Projectiles;
using MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon.Utilities;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Content.MoonlightSonata.Enemies;
using MagnumOpus.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon
{
    /// <summary>
    /// EternalMoon — "The Eternal Tide"
    /// 
    /// Moonlight Sonata's signature melee weapon. A blade forged from crystallized moonlight
    /// that channels the lunar cycle itself. Each swing advances through a 5-phase lunar combo
    /// (New Moon → Waxing → Half Moon → Waning → Full Moon) with escalating VFX intensity,
    /// homing tidal wave projectiles, ghost reflection echoes, and a devastating Full Moon
    /// tidal detonation on the final phase.
    /// 
    /// Special Attack (Alt): Lunar Surge — a dash attack where the player surges forward
    /// on a wave of moonlight. On hit, applies Lunar Stasis and spawns crescent slash VFX.
    /// If a surge connects, the next swing becomes an empowered Full Moon slash regardless
    /// of current combo position.
    /// 
    /// Architecture: Self-contained weapon system with own primitive renderer, particle system,
    /// shader loader, utility library, buff types, and ModPlayer. Inspired by the Sandbox
    /// Exoblade architecture but themed for Moonlight Sonata's lunar tidal identity.
    /// </summary>
    public class EternalMoon : ModItem
    {
        // === SOUND DEFINITIONS ===
        // Placeholder — use vanilla sounds until custom sounds are created
        // Future: "MagnumOpus/Content/MoonlightSonata/Weapons/EternalMoon/Sounds/..."

        // === BALANCE CONSTANTS ===
        public static int SurgeDashTime = 45;
        public static int BaseUseTime = 38;

        public override void SetDefaults()
        {
            Item.width = 50;
            Item.height = 50;
            Item.damage = 300;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = BaseUseTime;
            Item.useAnimation = BaseUseTime;
            Item.useTurn = true;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.knockBack = 7f;
            Item.autoReuse = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.value = Item.buyPrice(gold: 25);
            Item.shoot = ProjectileType<EternalMoonSwing>();
            Item.shootSpeed = 8f;
            Item.rare = RarityType<MoonlightSonataRarity>();
        }

        public override bool CanShoot(Player player)
        {
            // Alt-click (Lunar Surge): only one swing projectile at a time
            if (player.altFunctionUse == 2)
                return !Main.projectile.Any(n => n.active && n.owner == player.whoAmI && n.type == ProjectileType<EternalMoonSwing>());

            // Normal swing: don't overlap with an active non-stasis swing
            return !Main.projectile.Any(n => n.active && n.owner == player.whoAmI && n.type == ProjectileType<EternalMoonSwing>()
                && !(n.ai[0] == 1 && n.ai[1] == 1));
        }

        public override void HoldItem(Player player)
        {
            player.EternalMoon().RightClickListener = true;
            player.EternalMoon().MouseWorldListener = true;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool? CanHitNPC(Player player, NPC target) => false;
        public override bool CanHitPvp(Player player, Player target) => false;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float state = 0;

            // Check for empowered Full Moon slash opportunity (after successful surge hit)
            bool empoweredSlash = false;
            foreach (Projectile p in Main.ActiveProjectiles)
            {
                if (p.owner == player.whoAmI && p.type == Item.shoot &&
                    p.ai[0] == 1 && p.ai[1] == 1 &&
                    p.timeLeft > 60 * 3) // SurgeCooldown
                {
                    empoweredSlash = true;
                    break;
                }
            }

            if (empoweredSlash)
            {
                state = 2; // Empowered Full Moon slash
                foreach (Projectile p in Main.ActiveProjectiles)
                {
                    if (p.owner != player.whoAmI || p.type != Item.shoot || p.ai[0] != 1 || p.ai[1] != 1)
                        continue;
                    p.timeLeft = 60 * 3; // End the opportunity
                    p.netUpdate = true;
                }
            }

            if (player.altFunctionUse == 2)
                state = 1; // Lunar Surge

            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, state, 0);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            // Get current phase for display
            var player = Main.LocalPlayer;
            string phaseName = EternalMoonUtils.GetLunarPhaseName(player.EternalMoon().LunarPhase);

            tooltips.Add(new TooltipLine(Mod, "Effect1",
                $"5-phase Tidal Lunar Cycle combo with escalating crescent waves"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
                "Right-click for Lunar Surge dash attack"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
                "Surge hits empower the next swing to Full Moon crescendo"));
            tooltips.Add(new TooltipLine(Mod, "Effect4",
                "Ghost reflections echo at Half Moon, tidal detonation at Full Moon"));
            tooltips.Add(new TooltipLine(Mod, "Phase",
                $"Current phase: {phaseName}")
            { OverrideColor = EternalMoonUtils.GetLunarGradient(player.EternalMoon().LunarPhase / 4f) });
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'The eternal cycle made blade — each swing echoes moonlight on water'")
            { OverrideColor = new Color(140, 100, 200) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<MoonlightsResonantEnergy>(), 30)
                .AddIngredient(ItemType<ResonantCoreOfMoonlightSonata>(), 5)
                .AddIngredient(ItemType<ShardsOfMoonlitTempo>(), 10)
                .AddTile(TileType<MoonlightAnvilTile>())
                .Register();
        }
    }
}
