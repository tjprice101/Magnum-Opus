using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy.Weapons.RoseThornChainsaw.Projectiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Content.OdeToJoy.HarmonicCores;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;

namespace MagnumOpus.Content.OdeToJoy.Weapons.RoseThornChainsaw
{
    public class RoseThornChainsaw : ModItem
    {
        public override void SetDefaults()
        {
            Item.ResearchUnlockCount = 1;
            Item.damage = 4400;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.width = 54;
            Item.height = 24;
            Item.useTime = 1;
            Item.useAnimation = 1;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.channel = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.knockBack = 1f;
            Item.crit = 10;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<ChainsawHoldoutProjectile>();
            Item.shootSpeed = 32f;
            Item.UseSound = SoundID.Item22;
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.OdeToJoyRarity>();
            Item.value = Item.sellPrice(platinum: 4);
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
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Rapidly shreds enemies with a whirling storm of enchanted thorns"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Periodically launches thorn chains that ricochet off terrain"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Contact inflicts venomous bloom — Poisoned and Venom stack on hit"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Critical strikes spawn bonus thorn shrapnel"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
            "'From the garden's deepest tangle, where joy takes root in thorns, the song of reckless spring roars forth'")
            {
                OverrideColor = new Color(255, 200, 50)
            });
        }
    }
}