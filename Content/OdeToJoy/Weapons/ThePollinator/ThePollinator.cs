using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy.Weapons.ThePollinator.Projectiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Content.OdeToJoy.HarmonicCores;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThePollinator
{
    /// <summary>
    /// The Pollinator — ranged weapon of patient, spreading destruction.
    /// Fires pollen shots that apply Pollinated debuff (DoT + chain spreading).
    /// Pollinated enemies trigger Mass Bloom on death (AoE + homing seeds + golden field).
    /// Harvest Season at 5 blooms: 3x DoT + doubled bloom radius.
    /// </summary>
    public class ThePollinator : ModItem
    {
        private int _massBloomCount;
        private int _massBloomTimer;

        public override void SetDefaults()
        {
            Item.width = 52;
            Item.height = 28;
            Item.damage = 3200;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.OdeToJoyRarity>();
            Item.UseSound = SoundID.Item11;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.crit = 15;
            Item.shoot = ModContent.ProjectileType<PollenShotProjectile>();
            Item.shootSpeed = 14f;
            Item.useAmmo = AmmoID.Bullet;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Override ammo to pollen shot
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<PollenShotProjectile>(), damage, knockback, player.whoAmI);
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<ResonantCoreOfOdeToJoy>(), 20)
            .AddIngredient(ModContent.ItemType<OdeToJoyResonantEnergy>(), 15)
            .AddIngredient(ModContent.ItemType<HarmonicCoreOfOdeToJoy>(), 2)
            .AddIngredient(ItemID.LunarBar, 15)
            .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
            .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Converts bullets into pollen shots that apply Pollinated — 1% HP/s DoT that spreads to nearby enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Pollinated enemies trigger Mass Bloom on death — golden explosion + 3 homing seed projectiles"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Mass Bloom sites become Golden Fields that heal allies 3 HP/s and grant +5% damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "After 5 Mass Blooms within 10 seconds, triggers Harvest Season — 3x DoT for 5s"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The pollen does not hate. The pollen simply is. And soon, everything else simply was.'")
            {
                OverrideColor = new Color(255, 200, 50)
            });
        }
    }
}