using MagnumOpus.Common;
using MagnumOpus.Content.DiesIrae;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using MagnumOpus.Content.DiesIrae.HarmonicCores;
using MagnumOpus.Content.DiesIrae.ResonanceEnergies;
using MagnumOpus.Content.Fate.CraftingStations;

namespace MagnumOpus.Content.DiesIrae.Weapons.SinCollector
{
    /// <summary>
    /// Sin Collector — ranged gun that collects sin on each hit.
    /// Primary fire: Sin Bullets with escalating VFX.
    /// Alt fire (right click): Expend collected sin for powerful enhanced shots.
    ///   Tier 1 (10-19 Sins): Penance Shot — piercing, 1.5x damage
    ///   Tier 2 (20-29 Sins): Absolution Shot — wide pierce, explosion on impact, 2x damage
    ///   Tier 3 (30 Sins): Damnation Shot — homing, infinite pierce, 3x damage
    /// </summary>
    public class SinCollector : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 70;
            Item.height = 28;
            Item.damage = 2400;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 8f;
            Item.value = Item.sellPrice(platinum: 2, gold: 50);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.DiesIraeRarity>();
            Item.UseSound = SoundID.Item40 with { Pitch = -0.2f };
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<Projectiles.SinBulletProjectile>();
            Item.shootSpeed = 25f;
            Item.useAmmo = AmmoID.Bullet;
            Item.crit = 35;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                var sinPlayer = player.GetModPlayer<SinCollectorPlayer>();
                return sinPlayer.GetExpendTier() >= 1;
            }
            return base.CanUseItem(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                // Expenditure shot — consume sin, fire enhanced projectile
                var sinPlayer = player.GetModPlayer<SinCollectorPlayer>();
                int tier = sinPlayer.TryExpendSin();
                if (tier <= 0) return false;

                float damageMult = tier switch { 1 => 1.5f, 2 => 2.0f, _ => 3.0f };
                float speedMult = tier switch { 1 => 1.2f, 2 => 1.0f, _ => 0.9f };

                Projectile.NewProjectile(
                    source, position, velocity * speedMult,
                    ModContent.ProjectileType<Projectiles.PenanceShotProjectile>(),
                    (int)(damage * damageMult), knockback * 1.5f,
                    player.whoAmI, tier);

                // VFX feedback
                if (!Main.dedServ)
                {
                    for (int i = 0; i < 6 + tier * 4; i++)
                    {
                        Dust d = Dust.NewDustPerfect(position, DustID.Torch,
                            velocity.SafeNormalize(Vector2.Zero).RotatedByRandom(0.4) * Main.rand.NextFloat(2f, 5f),
                            0, DiesIraePalette.EmberOrange, 0.8f + tier * 0.3f);
                        d.noGravity = true;
                    }
                }

                return false; // we handled the projectile spawn
            }

            // Normal fire: Sin Bullet
            return true;
        }

        public override Vector2? HoldoutOffset() => new Vector2(-10f, 0f);

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<Content.DiesIrae.ResonanceEnergies.ResonantCoreOfDiesIrae>(), 20)
            .AddIngredient(ModContent.ItemType<Content.DiesIrae.ResonanceEnergies.DiesIraeResonantEnergy>(), 15)
            .AddIngredient(ModContent.ItemType<Content.DiesIrae.HarmonicCores.HarmonicCoreOfDiesIrae>(), 2)
            .AddIngredient(ItemID.LunarBar, 15)
            .AddTile(ModContent.TileType<Content.Fate.CraftingStations.FatesCosmicAnvilTile>())
            .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Each hit collects sin from the target"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Right-click expends collected sin for devastating enhanced shots"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "10+ Sins: Penance Shot, 20+: Absolution, 30: Damnation"));

            // Show current sin count if available
            if (Main.LocalPlayer != null)
            {
                var sinPlayer = Main.LocalPlayer.GetModPlayer<SinCollectorPlayer>();
                if (sinPlayer.SinCount > 0)
                {
                    tooltips.Add(new TooltipLine(Mod, "SinCount",
                        $"Current Sin: {sinPlayer.SinCount}/{SinCollectorPlayer.MaxSin}")
                    {
                        OverrideColor = Utilities.SinCollectorUtils.GetSinColor(sinPlayer.SinCount)
                    });
                }
            }

            tooltips.Add(new TooltipLine(Mod, "Lore", "'Your sins are not forgiven. They are collected.'")
            {
                OverrideColor = new Color(200, 50, 30)
            });
        }
    }
}