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
    /// Eventide Quiver - Ranged accessory for Nachtmusik theme.
    /// Every 4th ranged crit grants Rondo Allegro. Kills during Rondo refresh duration.
    /// </summary>
    public class EventideQuiver : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<EventideQuiverPlayer>().hasEventideQuiver = true;

            player.GetDamage(DamageClass.Ranged) += 0.35f;
            player.GetCritChance(DamageClass.Ranged) += 20;
            player.GetAttackSpeed(DamageClass.Ranged) += 0.15f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+35% ranged damage, +20% ranged crit, +15% ranged attack speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Every 4th ranged crit grants 'Rondo Allegro' for 3s (+15% ranged damage, +10% ranged crit)"));
            tooltips.Add(new TooltipLine(Mod, "NightBonus", "At night: Rondo Allegro lasts 5 seconds")
            {
                OverrideColor = new Color(100, 120, 200)
            });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The quiver holds only twilight — but twilight has never missed'")
            {
                OverrideColor = new Color(100, 120, 200)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<NachtmusikResonantEnergy>(), 12)
                .AddIngredient(ModContent.ItemType<NachtmusikResonantCore>(), 4)
                .AddIngredient(ItemID.FragmentVortex, 8)
                .AddIngredient(ItemID.LunarBar, 8)
                .AddIngredient(ItemID.FallenStar, 15)
                .AddIngredient(ModContent.ItemType<ShardOfNachtmusiksTempo>(), 5)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    public class EventideQuiverPlayer : ModPlayer
    {
        public bool hasEventideQuiver;
        private int critCounter;

        public override void ResetEffects()
        {
            hasEventideQuiver = false;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!hasEventideQuiver || !proj.CountsAsClass(DamageClass.Ranged) || !hit.Crit) return;

            critCounter++;
            if (critCounter >= 4)
            {
                critCounter = 0;
                int duration = !Main.dayTime ? 300 : 180; // 5s night, 3s day
                Player.AddBuff(ModContent.BuffType<RondoAllegroBuff>(), duration);
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!hasEventideQuiver || !item.CountsAsClass(DamageClass.Ranged) || !hit.Crit) return;

            critCounter++;
            if (critCounter >= 4)
            {
                critCounter = 0;
                int duration = !Main.dayTime ? 300 : 180;
                Player.AddBuff(ModContent.BuffType<RondoAllegroBuff>(), duration);
            }
        }
    }
}
