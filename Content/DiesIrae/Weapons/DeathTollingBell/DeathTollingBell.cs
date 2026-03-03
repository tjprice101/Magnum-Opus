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

namespace MagnumOpus.Content.DiesIrae.Weapons.DeathTollingBell
{
    public class DeathTollingBell : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 1450;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 22;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(gold: 35);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.DiesIraeRarity>();
            Item.UseSound = SoundID.Item44;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<Projectiles.BellTollingMinion>();
            Item.buffType = ModContent.BuffType<Buffs.DeathTollingBellBuff>();
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
            .AddIngredient(ModContent.ItemType<Content.DiesIrae.ResonanceEnergies.DiesIraeResonantEnergy>(), 30)
            .AddIngredient(ModContent.ItemType<Content.DiesIrae.ResonanceEnergies.ResonantCoreOfDiesIrae>(), 12)
            .AddIngredient(ModContent.ItemType<Content.DiesIrae.HarmonicCores.HarmonicCoreOfDiesIrae>(), 2)
            .AddIngredient(ItemID.LunarBar, 20)
            .AddTile(ModContent.TileType<Content.Fate.CraftingStations.FatesCosmicAnvilTile>())
            .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Summons a spectral bell of wrath that hovers near you"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "The bell periodically tolls, releasing concentric rings of devastating shockwaves"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Three rings of twelve toll waves strike with escalating fury"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'When the bell tolls, no prayer is answered — only judgment remains.'")
            {
                OverrideColor = new Color(200, 50, 30)
            });
        }
    }
}