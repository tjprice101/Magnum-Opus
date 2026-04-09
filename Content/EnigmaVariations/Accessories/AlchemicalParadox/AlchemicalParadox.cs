using Microsoft.Xna.Framework;
using System;
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
    /// Alchemical Paradox - Enigma ranged class accessory.
    /// 'Resonance Pierced' Melodic Attunement with 8% ammo conservation + 1% HP heal per saved shot.
    /// </summary>
    public class AlchemicalParadox : ModItem
    {
        public override string Texture => "MagnumOpus/Content/EnigmaVariations/Accessories/AlchemicalParadox/AlchemicalParadox";

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
            attunement.rangedAttunement = true;

            var paradox = player.GetModPlayer<AlchemicalParadoxPlayer>();
            paradox.equipped = true;

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

            tooltips.Add(new TooltipLine(Mod, "Effect1", "'Resonance Pierced' Melodic Attunement"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+45% increased Resonant Burn damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Hitting an enemy 25 times with ranged damage while inflicted with Resonant Burn heals 10% HP"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Critical strike damage on Resonant Burn enemies increased by 2.5%"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "8% chance to not consume ammo"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "For every shot not consumed, heal 1% HP"));
            tooltips.Add(new TooltipLine(Mod, "Effect7", "Immunity to fire debuffs, lava, confusion, and slow"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'In contradiction, truth unravels'")
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
                .AddIngredient(ItemID.RangerEmblem)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }

    public class AlchemicalParadoxPlayer : ModPlayer
    {
        public bool equipped = false;

        public override void ResetEffects()
        {
            equipped = false;
        }

        public override bool CanConsumeAmmo(Item weapon, Item ammo)
        {
            if (equipped && Main.rand.NextFloat() < 0.08f)
            {
                // Heal 1% max HP when ammo is saved
                int healAmount = Math.Max(1, (int)(Player.statLifeMax2 * 0.01f));
                Player.Heal(healAmount);
                return false;
            }
            return true;
        }
    }
}
