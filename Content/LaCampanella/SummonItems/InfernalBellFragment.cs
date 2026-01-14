using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.LaCampanella.Bosses;
using MagnumOpus.Content.LaCampanella.ResonanceEnergies;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common;

namespace MagnumOpus.Content.LaCampanella.SummonItems
{
    /// <summary>
    /// Infernal Bell Fragment - Summon item for La Campanella, Chime of Life boss.
    /// Used at night in the Underworld or anywhere post-Moon Lord.
    /// Crafted from La Campanella Resonant Energy and Hellstone.
    /// </summary>
    public class InfernalBellFragment : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 3;
            ItemID.Sets.SortingPriorityBossSpawns[Type] = 13; // After Moon Lord summon
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 20;
            Item.rare = ModContent.RarityType<LaCampanellaRarity>();
            Item.useAnimation = 45;
            Item.useTime = 45;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = true;
            Item.value = Item.buyPrice(gold: 5);
        }

        public override bool CanUseItem(Player player)
        {
            // Check if boss is already spawned
            if (NPC.AnyNPCs(ModContent.NPCType<LaCampanellaChimeOfLife>()))
            {
                return false;
            }
            
            // Can be used in Underworld at night, or anywhere post-Moon Lord
            bool inUnderworld = player.ZoneUnderworldHeight;
            bool isNight = !Main.dayTime;
            bool postMoonLord = NPC.downedMoonlord;
            
            return (inUnderworld && isNight) || postMoonLord;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                // Play summoning sound
                SoundEngine.PlaySound(SoundID.Item35 with { Pitch = -0.5f, Volume = 1.5f }, player.Center);
                
                // Spawn effects
                ThemedParticles.LaCampanellaShockwave(player.Center, 2f);
                ThemedParticles.LaCampanellaImpact(player.Center, 1.5f);
                
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    // Spawn boss above and in front of player
                    // Boss is 240 pixels tall (15 tiles), spawn higher to account for this
                    Vector2 spawnPos = player.Center + new Vector2(player.direction * 400, -350);
                    
                    // Make sure spawn position is valid (not in tiles)
                    // Check multiple points for the boss's full height (240 pixels = ~15 tiles)
                    Point tileCheck = spawnPos.ToTileCoordinates();
                    Point bottomCheck = (spawnPos + new Vector2(0, 240)).ToTileCoordinates();
                    
                    // Move up until the bottom of the boss is clear
                    int attempts = 0;
                    while ((WorldGen.SolidTile(tileCheck.X, tileCheck.Y) || WorldGen.SolidTile(bottomCheck.X, bottomCheck.Y)) && attempts < 30)
                    {
                        spawnPos.Y -= 16;
                        tileCheck = spawnPos.ToTileCoordinates();
                        bottomCheck = (spawnPos + new Vector2(0, 240)).ToTileCoordinates();
                        attempts++;
                    }
                    
                    int bossIndex = NPC.NewNPC(player.GetSource_ItemUse(Item), (int)spawnPos.X, (int)spawnPos.Y, 
                        ModContent.NPCType<LaCampanellaChimeOfLife>());
                    
                    if (Main.netMode == NetmodeID.Server && bossIndex < Main.maxNPCs)
                    {
                        NetMessage.SendData(MessageID.SyncNPC, number: bossIndex);
                    }
                }
                
                // Screen shake
                Main.LocalPlayer.position += Main.rand.NextVector2Circular(10f, 10f);
            }
            
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<LaCampanellaResonantEnergy>(), 15)
                .AddIngredient(ItemID.HellstoneBar, 10)
                .AddIngredient(ItemID.SoulofFright, 5)
                .AddIngredient(ItemID.SoulofMight, 5)
                .AddIngredient(ItemID.SoulofSight, 5)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            // Add summon conditions
            tooltips.Add(new TooltipLine(Mod, "SummonCondition", 
                "Summons La Campanella, Chime of Life")
            {
                OverrideColor = ThemedParticles.CampanellaOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "SummonCondition2", 
                "Use in the Underworld at night, or anywhere after defeating Moon Lord")
            {
                OverrideColor = Color.Gray
            });
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            // Pulsing glow effect
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.2f + 0.8f;
            
            Texture2D texture = Terraria.GameContent.TextureAssets.Item[Type].Value;
            
            // Draw glow
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = (MathHelper.TwoPi * i / 4f + Main.GameUpdateCount * 0.05f).ToRotationVector2() * 2f;
                spriteBatch.Draw(texture, position + offset, frame, ThemedParticles.CampanellaOrange * 0.3f * pulse, 
                    0f, origin, scale, SpriteEffects.None, 0f);
            }
            
            return true;
        }
    }
}
