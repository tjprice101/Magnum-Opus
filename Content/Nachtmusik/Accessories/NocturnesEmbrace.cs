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

namespace MagnumOpus.Content.Nachtmusik.Accessories
{
    /// <summary>
    /// Nocturne's Embrace - Summoner accessory for Nachtmusik theme.
    /// Minions gain Andante Grazioso (+12% speed). Every 8s grants Starlit Fervor;
    /// during Fervor, minion hits apply Diminuendo on enemies.
    /// </summary>
    public class NocturnesEmbrace : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<NocturnesEmbracePlayer>();
            modPlayer.hasNocturnesEmbrace = true;

            player.GetDamage(DamageClass.Summon) += 0.45f;
            player.maxMinions += 4;
            player.GetKnockback(DamageClass.Summon) += 0.25f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+45% summon damage, +4 max minions, +25% minion knockback"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Every 8 seconds, gain 'Starlit Fervor' for 4s (+25% summon damage)"));
            tooltips.Add(new TooltipLine(Mod, "NightBonus", "At night: Starlit Fervor cooldown reduced to 6 seconds")
            {
                OverrideColor = new Color(100, 120, 200)
            });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The night's orchestra plays on — and every instrument knows its part by heart'")
            {
                OverrideColor = new Color(100, 120, 200)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<NachtmusikResonantEnergy>(), 12)
                .AddIngredient(ModContent.ItemType<NachtmusikResonantCore>(), 4)
                .AddIngredient(ItemID.FragmentStardust, 10)
                .AddIngredient(ItemID.LunarBar, 8)
                .AddIngredient(ItemID.FallenStar, 20)
                .AddIngredient(ModContent.ItemType<ShardOfNachtmusiksTempo>(), 5)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    public class NocturnesEmbracePlayer : ModPlayer
    {
        public bool hasNocturnesEmbrace;
        private int fervorTimer;

        public override void ResetEffects()
        {
            hasNocturnesEmbrace = false;
        }

        public override void PostUpdate()
        {
            if (!hasNocturnesEmbrace)
            {
                fervorTimer = 0;
                return;
            }

            fervorTimer++;
            int cooldown = !Main.dayTime ? 360 : 480; // 6s night, 8s day

            if (fervorTimer >= cooldown)
            {
                fervorTimer = 0;
                Player.AddBuff(ModContent.BuffType<StarlitFervorBuff>(), 240); // 4 seconds
            }
        }
    }
}
