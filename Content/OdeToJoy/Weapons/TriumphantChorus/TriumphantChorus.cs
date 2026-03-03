using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy.Weapons.TriumphantChorus.Buffs;
using MagnumOpus.Content.OdeToJoy.Weapons.TriumphantChorus.Projectiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Content.OdeToJoy.HarmonicCores;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TriumphantChorus
{
    public class TriumphantChorus : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 46;
            Item.height = 46;
            Item.damage = 3400;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 35;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5f;
            Item.crit = 4;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.OdeToJoyRarity>();
            Item.UseSound = SoundID.Item44;
            Item.autoReuse = false;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<TriumphantChorusMinion>();
            Item.shootSpeed = 10f;
            Item.buffType = ModContent.BuffType<TriumphantChorusBuff>();
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
            .AddIngredient(ModContent.ItemType<ResonantCoreOfOdeToJoy>(), 30)
            .AddIngredient(ModContent.ItemType<OdeToJoyResonantEnergy>(), 25)
            .AddIngredient(ModContent.ItemType<HarmonicCoreOfOdeToJoy>(), 4)
            .AddIngredient(ItemID.LunarBar, 25)
            .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
            .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Summon", "Summons a triumphant chorus entity that orbits you"));
            tooltips.Add(new TooltipLine(Mod, "Slots", "Requires 2 minion slots"));
            tooltips.Add(new TooltipLine(Mod, "Attack", "Fires homing harmonic blasts at nearby enemies"));
            tooltips.Add(new TooltipLine(Mod, "Finale", "Every 5 seconds, performs a grand finale — an 8-way radial burst at double damage"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'When every voice rings true, the world itself sings back in jubilation'")
            {
                OverrideColor = new Color(255, 200, 50)
            });
        }
    }
}