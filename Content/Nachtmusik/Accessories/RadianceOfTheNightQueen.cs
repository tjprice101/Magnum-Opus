using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.Nachtmusik.ResonanceEnergies;
using MagnumOpus.Content.Nachtmusik.HarmonicCores;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.Materials.EnemyDrops;

namespace MagnumOpus.Content.Nachtmusik.Accessories
{
    /// <summary>
    /// Radiance of the Night Queen - Universal accessory for Nachtmusik theme.
    /// Every 10s (7s at night) grants Eine Kleine buff (+12% all damage, +5% crit for 6s).
    /// </summary>
    public class RadianceOfTheNightQueen : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 4);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<RadianceOfTheNightQueenPlayer>().hasRadianceOfTheNightQueen = true;
            player.GetDamage(DamageClass.Generic) += 0.25f;
            player.GetCritChance(DamageClass.Generic) += 15;
            player.moveSpeed += 0.20f;
            player.maxMinions += 2;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+25% all damage, +15% crit, +20% movement speed, +2 max minions"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Every 10 seconds, gain 'Eine Kleine' for 6s (+12% all damage, +5% crit)"));
            tooltips.Add(new TooltipLine(Mod, "NightBonus", "At night: Eine Kleine cooldown reduced to 7 seconds")
            {
                OverrideColor = new Color(100, 120, 200)
            });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'She crowns the worthy with starlight — and the night bends its knee'")
            {
                OverrideColor = new Color(100, 120, 200)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<NachtmusikResonantEnergy>(), 15)
                .AddIngredient(ModContent.ItemType<NachtmusikResonantCore>(), 6)
                .AddIngredient(ModContent.ItemType<NachtmusikEssence>(), 8)
                .AddIngredient(ItemID.FragmentSolar, 5)
                .AddIngredient(ItemID.FragmentNebula, 5)
                .AddIngredient(ItemID.FragmentVortex, 5)
                .AddIngredient(ItemID.FragmentStardust, 5)
                .AddIngredient(ItemID.LunarBar, 10)
                .AddIngredient(ModContent.ItemType<ShardOfNachtmusiksTempo>(), 5)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    public class RadianceOfTheNightQueenPlayer : ModPlayer
    {
        public bool hasRadianceOfTheNightQueen;
        private int eineKleineTimer;

        public override void ResetEffects()
        {
            hasRadianceOfTheNightQueen = false;
        }

        public override void PostUpdate()
        {
            if (!hasRadianceOfTheNightQueen)
            {
                eineKleineTimer = 0;
                return;
            }

            eineKleineTimer++;
            int cooldown = !Main.dayTime ? 420 : 600; // 7s night, 10s day

            if (eineKleineTimer >= cooldown)
            {
                eineKleineTimer = 0;
                Player.AddBuff(ModContent.BuffType<EineKleineBuff>(), 360); // 6 seconds
            }
        }
    }

    /// <summary>
    /// Kept for backward compatibility with existing buff icon texture.
    /// Replaced functionally by EineKleineBuff.
    /// </summary>
    public class QueensRadianceBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // Intentionally empty — legacy buff kept for backward compat texture references only
        }
    }
}