using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy.Weapons.TheStandingOvation.Buffs;
using MagnumOpus.Content.OdeToJoy.Weapons.TheStandingOvation.Projectiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Content.OdeToJoy.HarmonicCores;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TheStandingOvation
{
    public class TheStandingOvation : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 46;
            Item.height = 46;
            Item.damage = 2600;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 20;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3f;
            Item.crit = 4;
            Item.value = Item.sellPrice(platinum: 4);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.OdeToJoyRarity>();
            Item.UseSound = SoundID.Item44;
            Item.autoReuse = false;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<StandingOvationMinion>();
            Item.shootSpeed = 10f;
            Item.buffType = ModContent.BuffType<StandingOvationBuff>();
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
            .AddIngredient(ModContent.ItemType<ResonantCoreOfOdeToJoy>(), 25)
            .AddIngredient(ModContent.ItemType<OdeToJoyResonantEnergy>(), 20)
            .AddIngredient(ModContent.ItemType<HarmonicCoreOfOdeToJoy>(), 3)
            .AddIngredient(ItemID.LunarBar, 20)
            .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
            .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Summon", "Summons an applauding spirit to fight for you"));
            tooltips.Add(new TooltipLine(Mod, "Behavior", "Spirits hover and release waves of joyful energy"));
            tooltips.Add(new TooltipLine(Mod, "Synergy", "Multiple spirits synchronize for +20% damage per spirit"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The crowd rises — a symphony of spirit, unbroken and glorious'")
            {
                OverrideColor = new Color(255, 200, 50)
            });
        }
    }
}