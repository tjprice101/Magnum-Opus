using MagnumOpus.Common;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using MagnumOpus.Content.DiesIrae.HarmonicCores;
using MagnumOpus.Content.DiesIrae.ResonanceEnergies;
using MagnumOpus.Content.Fate.CraftingStations;

namespace MagnumOpus.Content.DiesIrae.Weapons.WrathfulContract
{
    public class WrathfulContract : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.damage = 1650;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 40;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(platinum: 2, gold: 25);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.DiesIraeRarity>();
            Item.UseSound = SoundID.Item82;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<Projectiles.WrathDemonMinion>();
            Item.shootSpeed = 0f;
            Item.buffType = ModContent.BuffType<Buffs.WrathfulContractBuff>();
            Item.buffTime = 18000;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<Content.DiesIrae.ResonanceEnergies.ResonantCoreOfDiesIrae>(), 15)
            .AddIngredient(ModContent.ItemType<Content.DiesIrae.ResonanceEnergies.DiesIraeResonantEnergy>(), 35)
            .AddIngredient(ModContent.ItemType<Content.DiesIrae.HarmonicCores.HarmonicCoreOfDiesIrae>(), 3)
            .AddIngredient(ItemID.LunarBar, 25)
            .AddTile(ModContent.TileType<Content.Fate.CraftingStations.FatesCosmicAnvilTile>())
            .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Summons a wrathful demon that orbits and dashes at enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Every 3rd dash unleashes a burst of 6 homing wrath fireballs"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Costs 2 minion slots"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The contract is sealed in hellfire — there is no breaking it'")
            {
                OverrideColor = new Color(200, 50, 30)
            });
        }
    }
}