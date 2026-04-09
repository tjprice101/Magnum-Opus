using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.Common.Accessories;
using MagnumOpus.Content.EnigmaVariations.ResonanceEnergies;
using MagnumOpus.Content.EnigmaVariations.HarmonicCores;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;

namespace MagnumOpus.Content.EnigmaVariations.Accessories
{
    /// <summary>
    /// Ignition of Mystery - Enigma melee class accessory.
    /// 'Resonance Sliced' Melodic Attunement with fire/lava/confusion/slow immunity.
    /// </summary>
    public class IgnitionOfMystery : ModItem
    {
        public override string Texture => "MagnumOpus/Content/EnigmaVariations/Accessories/IgnitionOfMystery/IgnitionOfMystery";

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 3);
            Item.rare = ModContent.RarityType<EnigmaVariationsRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var attunement = player.GetModPlayer<MelodicAttunementPlayer>();
            attunement.resonantBurnDmgBonus += 0.45f;
            attunement.critDmgBonusOnBurn += 0.025f;
            attunement.meleeAttunement = true;

            // Fire/lava/confusion/slow immunity
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.OnFire3] = true;
            player.lavaImmune = true;
            player.buffImmune[BuffID.Confused] = true;
            player.buffImmune[BuffID.Slow] = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color lore = new Color(140, 60, 200);

            tooltips.Add(new TooltipLine(Mod, "Effect1", "'Resonance Sliced' Melodic Attunement"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+45% increased Resonant Burn damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Hitting an enemy 10 times with melee damage while inflicted with Resonant Burn heals 10% HP"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Critical strike damage on Resonant Burn enemies increased by 2.5%"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Immunity to fire debuffs, lava, confusion, and slow"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The unknown fears those who seek it'")
            {
                OverrideColor = lore
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<EnigmaResonantEnergy>(), 15)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfEnigma>(), 1)
                .AddIngredient(ModContent.ItemType<ShardOfTheMysterysTempo>(), 5)
                .AddIngredient(ItemID.MagmaStone)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }
}
