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
    /// Riddlemaster's Cauldron - Enigma summoner class accessory.
    /// 'Resonance Born' Melodic Attunement with 20% universal crit, +5% summon dmg, +2 minion slots.
    /// </summary>
    public class RiddlemastersCauldron : ModItem
    {
        public override string Texture => "MagnumOpus/Content/EnigmaVariations/Accessories/RiddlemastersCauldron/RiddlemastersCauldron";

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
            attunement.critDmgAll += 0.20f;
            attunement.summonAttunement = true;

            // +5% summon damage, +2 minion slots
            player.GetDamage(DamageClass.Summon) += 0.05f;
            player.maxMinions += 2;

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

            tooltips.Add(new TooltipLine(Mod, "Effect1", "'Resonance Born' Melodic Attunement"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+45% increased Resonant Burn damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Hitting an enemy 30 times with summon or whip damage while inflicted with Resonant Burn heals 10% HP"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Increased critical damage against all enemies by 20%"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "+5% summon damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "+2 minion slots"));
            tooltips.Add(new TooltipLine(Mod, "Effect7", "Immunity to fire debuffs, lava, confusion, and slow"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The answer was always in the question'")
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
                .AddIngredient(ItemID.PygmyNecklace)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }
}
