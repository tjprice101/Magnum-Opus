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
    /// Special Attack (Alt): At full charge, releases a ring of sparkly lunar orbs that
    /// shimmer and explode on contact with enemies or tiles.
    /// </summary>
    public class EternalMoon : ModItem
    {
        public static int BaseUseTime = 38;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 50;
            Item.height = 50;
            Item.scale = 0.10f;
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
            // Right-click: always allow (Shoot handles deny logic)
            if (player.altFunctionUse == 2)
                return true;

            // Normal swing: don't overlap with an active swing
            return !Main.projectile.Any(n => n.active && n.owner == player.whoAmI && n.type == ProjectileType<EternalMoonSwing>());
        }

        public override void HoldItem(Player player)
        {
            player.EternalMoon().IsHoldingEternalMoon = true;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool? CanHitNPC(Player player, NPC target) => false;
        public override bool CanHitPvp(Player player, Player target) => false;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Right-click: charge-gated special
            if (player.altFunctionUse == 2)
            {
                var emp = player.EternalMoon();
                if (emp.IsChargeFull)
                {
                    emp.ConsumeCharge();
                    int orbDmg = (int)(damage * 1.5f);
                    Projectile.NewProjectile(source, player.MountedCenter, Vector2.Zero,
                        ProjectileType<EternalMoonOrbRing>(),
                        orbDmg, knockback, player.whoAmI);
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 0.9f }, player.MountedCenter);
                }
                else
                {
                    SoundEngine.PlaySound(SoundID.Item27 with { Pitch = -0.3f, Volume = 0.4f }, player.MountedCenter);
                }
                return false;
            }

            // Normal left-click: spawn swing projectile
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0f, 0);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var player = Main.LocalPlayer;
            string phaseName = EternalMoonUtils.GetLunarPhaseName(player.EternalMoon().LunarPhase);

            tooltips.Add(new TooltipLine(Mod, "Effect1",
                "5-phase Tidal Lunar Cycle combo with escalating crescent waves"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
                "Hits build tidal charge — kills charge significantly more"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
                "Right-click at full charge to unleash a ring of homing lunar orbs"));
            tooltips.Add(new TooltipLine(Mod, "Effect4",
                "Ghost reflections echo at Half Moon, tidal detonation at Full Moon"));
            tooltips.Add(new TooltipLine(Mod, "Phase",
                $"Current phase: {phaseName}")
            { OverrideColor = EternalMoonUtils.GetLunarGradient(player.EternalMoon().LunarPhase / 4f) });
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'The tide remembers what the shore forgets.'")
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
