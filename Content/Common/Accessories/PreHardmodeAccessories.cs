using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems;
using MagnumOpus.Content.Materials.Foundation;
using System;
using System.Collections.Generic;

namespace MagnumOpus.Content.Common.Accessories
{
    #region Composer's Notebook

    /// <summary>
    /// Composer's Notebook - Pre-Hardmode base accessory.
    /// A leather-bound journal containing the foundations of musical power.
    /// +5% Melee and Ranger Damage, +5% Attack Speed.
    /// </summary>
    public class ComposersNotebook : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 26;
            Item.accessory = true;
            Item.value = Item.buyPrice(gold: 2);
            Item.rare = ItemRarityID.Blue;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // +5% Melee and Ranger Damage
            player.GetDamage(DamageClass.Melee) += 0.05f;
            player.GetDamage(DamageClass.Ranged) += 0.05f;

            // +5% Attack Speed
            player.GetAttackSpeed(DamageClass.Generic) += 0.05f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "DamageBoost", "+5% melee and ranged damage")
            {
                OverrideColor = new Color(180, 150, 255)
            });

            tooltips.Add(new TooltipLine(Mod, "AttackSpeed", "+5% attack speed")
            {
                OverrideColor = new Color(255, 200, 100)
            });

            tooltips.Add(new TooltipLine(Mod, "Flavor", "'The first steps of a grand composition'")
            {
                OverrideColor = new Color(150, 150, 150)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCrystalShard>(), 5)
                .AddIngredient(ModContent.ItemType<FadedSheetMusic>(), 1)
                .AddIngredient(ItemID.Book, 1)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }

    #endregion

    #region Resonant Pendant

    /// <summary>
    /// Resonant Pendant - Pre-Hardmode base accessory.
    /// A circular metal disc that resonates with harmonic frequencies.
    /// +3% Magic and Summoner Damage, +8% Attack and Summon Speed.
    /// </summary>
    public class ResonantPendant : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 24;
            Item.accessory = true;
            Item.value = Item.buyPrice(gold: 2);
            Item.rare = ItemRarityID.Blue;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // +3% Magic and Summoner Damage
            player.GetDamage(DamageClass.Magic) += 0.03f;
            player.GetDamage(DamageClass.Summon) += 0.03f;

            // +8% Attack and Summon Speed
            player.GetAttackSpeed(DamageClass.Generic) += 0.08f;

            // Music note drop chance handled by global NPC
            player.GetModPlayer<ResonantPendantPlayer>().hasResonantPendant = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "DamageBoost", "+3% magic and summoner damage")
            {
                OverrideColor = new Color(180, 150, 255)
            });

            tooltips.Add(new TooltipLine(Mod, "AttackSpeed", "+8% attack and summon speed")
            {
                OverrideColor = new Color(255, 200, 100)
            });

            tooltips.Add(new TooltipLine(Mod, "NoteDrop", "Enemies may drop Minor Music Notes")
            {
                OverrideColor = new Color(255, 215, 0)
            });

            tooltips.Add(new TooltipLine(Mod, "Flavor", "'Attuned to the frequencies of creation'")
            {
                OverrideColor = new Color(150, 150, 150)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<TuningFork>(), 1)
                .AddIngredient(ModContent.ItemType<DullResonator>(), 1)
                .AddIngredient(ModContent.ItemType<ResonantCrystalShard>(), 3)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }

    public class ResonantPendantPlayer : ModPlayer
    {
        public bool hasResonantPendant;

        public override void ResetEffects()
        {
            hasResonantPendant = false;
        }
    }

    public class ResonantPendantGlobalNPC : GlobalNPC
    {
        public override void OnKill(NPC npc)
        {
            if (Main.player[Main.myPlayer].GetModPlayer<ResonantPendantPlayer>().hasResonantPendant)
            {
                // 5% chance to drop Minor Music Note
                if (Main.rand.NextFloat() < 0.05f && npc.lifeMax > 5 && !npc.friendly)
                {
                    Item.NewItem(npc.GetSource_Loot(), npc.getRect(), ModContent.ItemType<MinorMusicNote>(), 1);
                }
            }
        }
    }

    #endregion

    #region Melodic Charm

    /// <summary>
    /// Melodic Charm - Pre-Hardmode combined accessory.
    /// Combines the Composer's Notebook and Resonant Pendant.
    /// +10% All Damage, +15 Mana Regen/second, +12% Attack and Summon Speed.
    /// </summary>
    public class MelodicCharm : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.accessory = true;
            Item.value = Item.buyPrice(gold: 5);
            Item.rare = ItemRarityID.Green;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // +10% All Damage
            player.GetDamage(DamageClass.Generic) += 0.10f;

            // +15 Mana Regeneration/second
            player.manaRegenBonus += 15;

            // +12% Attack and Summon Speed
            player.GetAttackSpeed(DamageClass.Generic) += 0.12f;

            // Music note drop chance
            player.GetModPlayer<ResonantPendantPlayer>().hasResonantPendant = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "DamageBoost", "+10% damage")
            {
                OverrideColor = new Color(200, 170, 255)
            });

            tooltips.Add(new TooltipLine(Mod, "ManaRegen", "+15 mana regeneration per second")
            {
                OverrideColor = new Color(100, 150, 255)
            });

            tooltips.Add(new TooltipLine(Mod, "AttackSpeed", "+12% attack and summon speed")
            {
                OverrideColor = new Color(255, 200, 100)
            });

            tooltips.Add(new TooltipLine(Mod, "NoteDrop", "Enemies may drop Minor Music Notes")
            {
                OverrideColor = new Color(255, 215, 0)
            });

            tooltips.Add(new TooltipLine(Mod, "Flavor", "'The melody begins to take shape'")
            {
                OverrideColor = new Color(150, 150, 150)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ComposersNotebook>(), 1)
                .AddIngredient(ModContent.ItemType<ResonantPendant>(), 1)
                .AddIngredient(ModContent.ItemType<MinorMusicNote>(), 5)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }

    #endregion
}
