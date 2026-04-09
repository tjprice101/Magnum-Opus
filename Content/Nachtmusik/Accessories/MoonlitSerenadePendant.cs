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
    /// Moonlit Serenade Pendant - Magic accessory for Nachtmusik theme.
    /// Magic hits have a 12% chance to apply Serenade's Echo. Echo spreads to nearby enemies on hit.
    /// </summary>
    public class MoonlitSerenadePendant : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<MoonlitSerenadePendantPlayer>().hasMoonlitSerenadePendant = true;
            player.GetDamage(DamageClass.Magic) += 0.35f;
            player.manaRegenBonus += 50;
            player.manaCost -= 0.20f;
            player.GetCritChance(DamageClass.Magic) += 15;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+35% magic damage, +50 mana regen, -20% mana cost, +15% magic crit"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "12% on magic hit: apply 'Serenade's Echo' on enemy for 4s (+10% magic damage taken)"));
            tooltips.Add(new TooltipLine(Mod, "NightBonus", "At night: Serenade's Echo duration increases to 6 seconds")
            {
                OverrideColor = new Color(100, 120, 200)
            });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Her serenade drifts on the evening air — each note finds the next listener before it fades'")
            {
                OverrideColor = new Color(100, 120, 200)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<NachtmusikResonantEnergy>(), 12)
                .AddIngredient(ModContent.ItemType<NachtmusikResonantCore>(), 4)
                .AddIngredient(ItemID.FragmentNebula, 8)
                .AddIngredient(ItemID.LunarBar, 8)
                .AddIngredient(ItemID.FallenStar, 15)
                .AddIngredient(ModContent.ItemType<ShardOfNachtmusiksTempo>(), 5)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    public class MoonlitSerenadePendantPlayer : ModPlayer
    {
        public bool hasMoonlitSerenadePendant;

        public override void ResetEffects()
        {
            hasMoonlitSerenadePendant = false;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!hasMoonlitSerenadePendant || !proj.CountsAsClass(DamageClass.Magic)) return;

            // 12% chance to apply Serenade's Echo
            if (Main.rand.NextFloat() < 0.12f)
            {
                int duration = !Main.dayTime ? 360 : 240; // 6s night, 4s day
                target.GetGlobalNPC<NachtmusikAccessoryGlobalNPC>().ApplySerenadeEcho(duration);
            }
        }
    }
}
