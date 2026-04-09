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
    /// Pendant of a Thousand Puzzles - Enigma magic class accessory.
    /// 'Resonance Seared' Melodic Attunement with -10% mana cost and 2% mana refund on hit.
    /// </summary>
    public class PendantOfAThousandPuzzles : ModItem
    {
        public override string Texture => "MagnumOpus/Content/EnigmaVariations/Accessories/PendantOfAThousandPuzzles/PendantOfAThousandPuzzles";

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
            attunement.magicAttunement = true;

            var pendant = player.GetModPlayer<PendantOfAThousandPuzzlesPlayer>();
            pendant.equipped = true;

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

            tooltips.Add(new TooltipLine(Mod, "Effect1", "'Resonance Seared' Melodic Attunement"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+45% increased Resonant Burn damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Hitting an enemy 15 times with magic damage while inflicted with Resonant Burn heals 10% HP"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Critical strike damage on Resonant Burn enemies increased by 2.5%"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "-10% mana cost"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Refunds 2% mana consumed on enemy hit"));
            tooltips.Add(new TooltipLine(Mod, "Effect7", "Immunity to fire debuffs, lava, confusion, and slow"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A thousand answers to questions never asked'")
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
                .AddIngredient(ItemID.CelestialEmblem)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }

    public class PendantOfAThousandPuzzlesPlayer : ModPlayer
    {
        public bool equipped = false;

        public override void ResetEffects()
        {
            equipped = false;
        }

        public override void ModifyManaCost(Item item, ref float reduce, ref float mult)
        {
            if (equipped)
                mult *= 0.9f; // -10% mana cost
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (equipped && proj.DamageType.CountsAsClass(DamageClass.Magic))
            {
                int manaRefund = Math.Max(1, (int)(Player.statManaMax2 * 0.02f));
                Player.statMana = Math.Min(Player.statMana + manaRefund, Player.statManaMax2);
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (equipped && item.DamageType.CountsAsClass(DamageClass.Magic))
            {
                int manaRefund = Math.Max(1, (int)(Player.statManaMax2 * 0.02f));
                Player.statMana = Math.Min(Player.statMana + manaRefund, Player.statManaMax2);
            }
        }
    }
}
