using MagnumOpus.Common;
using MagnumOpus.Content.DiesIrae.Weapons.ArbitersSentence.Projectiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using MagnumOpus.Content.DiesIrae.HarmonicCores;
using MagnumOpus.Content.DiesIrae.ResonanceEnergies;
using MagnumOpus.Content.Fate.CraftingStations;

namespace MagnumOpus.Content.DiesIrae.Weapons.ArbitersSentence
{
    public class ArbitersSentence : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 64;
            Item.height = 24;
            Item.damage = 850;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 3;
            Item.useAnimation = 9;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 1f;
            Item.value = Item.sellPrice(platinum: 1, gold: 50);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.DiesIraeRarity>();
            Item.UseSound = SoundID.Item34 with { Pitch = 0.15f, Volume = 0.6f };
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<JudgmentFlameProjectile>();
            Item.shootSpeed = 11f;
            Item.useAmmo = AmmoID.Gel;
            Item.crit = 15;
        }

        public override Vector2? HoldoutOffset() => new Vector2(-10f, 0f);

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Spray slight random spread for flamethrower feel
            float spread = MathHelper.ToRadians(4f);
            velocity = velocity.RotatedByRandom(spread);

            // Occasionally spawn lingering purgatory embers (every ~10th shot)
            if (Main.rand.NextBool(10))
            {
                Vector2 emberVel = velocity * 0.4f;
                Projectile.NewProjectile(source, position, emberVel,
                    ModContent.ProjectileType<PurgatoryEmberProjectile>(),
                    (int)(damage * 0.3f), knockback * 0.2f, player.whoAmI);
            }

            return true;
        }

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
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Precision flamethrower that applies stacking Judgment Flame (15 damage/s per stack)"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "At 5 stacks: Sentence Cage roots enemy, next hit deals double damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "5 consecutive hits on same target activates Arbiter's Focus — 3 precision shots with +40% damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Killing sentenced enemies transfers flames to nearby foes"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The arbiter does not miss. The arbiter does not forgive.'")
            {
                OverrideColor = new Color(200, 50, 30)
            });
        }
    }
}