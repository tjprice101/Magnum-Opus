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
    /// Starweaver's Signet - Melee accessory for Nachtmusik theme.
    /// Crits apply Sotto Voce on enemies. Every 5th melee crit grants Nocturne's Cadence.
    /// </summary>
    public class StarweaversSignet : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<StarweaversSignetPlayer>().hasStarweaversSignet = true;
            player.GetDamage(DamageClass.Melee) += 0.38f;
            player.GetAttackSpeed(DamageClass.Melee) += 0.25f;
            player.GetCritChance(DamageClass.Melee) += 18;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+38% melee damage, +25% melee attack speed, +18% melee crit"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Melee crits grant 'Nocturne's Cadence' for 4s (+12% melee damage, +5% melee crit)"));
            tooltips.Add(new TooltipLine(Mod, "NightBonus", "At night: Nocturne's Cadence extends to 6 seconds")
            {
                OverrideColor = new Color(100, 120, 200)
            });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Each swing scatters the night sky — and the stars remember where they fell'")
            {
                OverrideColor = new Color(100, 120, 200)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<NachtmusikResonantEnergy>(), 12)
                .AddIngredient(ModContent.ItemType<NachtmusikResonantCore>(), 4)
                .AddIngredient(ItemID.FragmentSolar, 8)
                .AddIngredient(ItemID.LunarBar, 8)
                .AddIngredient(ItemID.FallenStar, 15)
                .AddIngredient(ModContent.ItemType<ShardOfNachtmusiksTempo>(), 5)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    public class StarweaversSignetPlayer : ModPlayer
    {
        public bool hasStarweaversSignet;

        public override void ResetEffects()
        {
            hasStarweaversSignet = false;
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!hasStarweaversSignet || !item.CountsAsClass(DamageClass.Melee) || !hit.Crit) return;
            int duration = !Main.dayTime ? 360 : 240; // 6s night, 4s day
            Player.AddBuff(ModContent.BuffType<NocturnesCadenceBuff>(), duration);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!hasStarweaversSignet || !proj.CountsAsClass(DamageClass.Melee) || !hit.Crit) return;
            int duration = !Main.dayTime ? 360 : 240;
            Player.AddBuff(ModContent.BuffType<NocturnesCadenceBuff>(), duration);
        }
    }
}
