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
            Item.shoot = ModContent.ProjectileType<Projectiles.JudgmentFlameProjectile>();
            Item.shootSpeed = 11f;
            Item.useAmmo = AmmoID.Gel;
            Item.crit = 15;
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
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Unleashes a continuous stream of judgment fire"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Leaves lingering purgatory embers that burn enemies"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The sentence is written in fire, and none may appeal'")
            {
                OverrideColor = new Color(200, 50, 30)
            });
        }
    }
}