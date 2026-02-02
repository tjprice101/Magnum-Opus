using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.DiesIrae.Bosses;
using MagnumOpus.Content.DiesIrae.ResonanceEnergies;
using MagnumOpus.Content.Fate.CraftingStations;

namespace MagnumOpus.Content.DiesIrae.SummonItems
{
    /// <summary>
    /// Score of Dies Irae - Summon item for Dies Irae, Herald of Judgment.
    /// Used in the Underworld after defeating Nachtmusik.
    /// </summary>
    public class ScoreOfDiesIrae : ModItem
    {
        private static readonly Color BloodRed = new Color(139, 0, 0);
        private static readonly Color EmberOrange = new Color(255, 69, 0);

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 20;
            Item.useAnimation = 45;
            Item.useTime = 45;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = true;
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
            Item.value = Item.sellPrice(gold: 5);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Summons Dies Irae, Herald of Judgment"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Use in the Underworld"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The final score that heralds the end of all things'") 
            { 
                OverrideColor = BloodRed 
            });
        }

        public override bool CanUseItem(Player player)
        {
            // Must be in the Underworld
            if (player.ZoneUnderworldHeight)
            {
                // Check that boss isn't already alive
                return !NPC.AnyNPCs(ModContent.NPCType<DiesIraeHeraldOfJudgement>());
            }
            return false;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                SoundEngine.PlaySound(SoundID.Roar, player.Center);
                
                // Spawn the boss above the player
                int type = ModContent.NPCType<DiesIraeHeraldOfJudgement>();
                
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 spawnPos = player.Center + new Vector2(0, -400f);
                    NPC.SpawnOnPlayer(player.whoAmI, type);
                }
                else
                {
                    // In multiplayer, send a request to spawn the boss
                    NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, number: player.whoAmI, number2: type);
                }
            }
            
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfDiesIrae>(), 10)
                .AddIngredient(ModContent.ItemType<DiesIraeResonantEnergy>(), 5)
                .AddIngredient(ItemID.SoulofFright, 5)
                .AddIngredient(ItemID.SoulofMight, 5)
                .AddIngredient(ItemID.SoulofSight, 5)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }
}
