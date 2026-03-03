using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy.Weapons.FountainOfJoyousHarmony.Buffs;
using MagnumOpus.Content.OdeToJoy.Weapons.FountainOfJoyousHarmony.Projectiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Content.OdeToJoy.HarmonicCores;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;

namespace MagnumOpus.Content.OdeToJoy.Weapons.FountainOfJoyousHarmony
{
    public class FountainOfJoyousHarmony : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 46;
            Item.height = 46;
            Item.damage = 2200;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 25;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 2f;
            Item.crit = 4;
            Item.value = Item.sellPrice(platinum: 4);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.OdeToJoyRarity>();
            Item.UseSound = SoundID.Item44;
            Item.autoReuse = false;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<JoyousFountainMinion>();
            Item.shootSpeed = 0.01f;
            Item.buffType = ModContent.BuffType<JoyousFountainBuff>();
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
            tooltips.Add(new TooltipLine(Mod, "Summon", "Places a stationary fountain at the cursor position"));
            tooltips.Add(new TooltipLine(Mod, "Heal", "Heals 3 HP every second when within range"));
            tooltips.Add(new TooltipLine(Mod, "Attack", "Fires homing water bolts that burst into rose petals"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Where waters rise and petals fall, joy sings its endless song for all'")
            {
                OverrideColor = new Color(255, 200, 50)
            });
        }
    }
}