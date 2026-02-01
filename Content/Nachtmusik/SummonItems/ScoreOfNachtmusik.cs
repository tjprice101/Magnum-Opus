using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using MagnumOpus.Content.Nachtmusik.Bosses;
using MagnumOpus.Content.Nachtmusik.HarmonicCores;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Nachtmusik.SummonItems
{
    /// <summary>
    /// Score of Nachtmusik - Summons Nachtmusik, Queen of Radiance
    /// 
    /// A celestial musical score that, when performed under the night sky,
    /// calls forth the Queen of Radiance from the heavens.
    /// </summary>
    public class ScoreOfNachtmusik : ModItem
    {
        // Theme colors
        private static readonly Color DeepPurple = new Color(45, 27, 78);
        private static readonly Color Gold = new Color(255, 215, 0);
        private static readonly Color Violet = new Color(123, 104, 238);
        
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 3;
            ItemID.Sets.SortingPriorityBossSpawns[Type] = 17;
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 20;
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.useAnimation = 45;
            Item.useTime = 45;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = true;
            Item.value = Item.buyPrice(gold: 10);
        }

        public override bool CanUseItem(Player player)
        {
            // Must be at night
            if (Main.dayTime)
            {
                if (Main.netMode != NetmodeID.Server)
                    Main.NewText("The Queen only answers when the stars shine...", Violet);
                return false;
            }
            
            // Must not already have the boss active
            if (NPC.AnyNPCs(ModContent.NPCType<NachtmusikQueenOfRadiance>()))
            {
                return false;
            }
            
            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                SoundEngine.PlaySound(SoundID.Roar with { Pitch = 0.3f }, player.Center);
                
                // Spawn position above player
                int spawnX = (int)player.Center.X;
                int spawnY = (int)player.Center.Y - 600;
                
                // Find valid spawn point
                for (int attempts = 0; attempts < 50; attempts++)
                {
                    if (!Collision.SolidTiles(spawnX / 16 - 5, spawnX / 16 + 5, spawnY / 16 - 5, spawnY / 16 + 5))
                        break;
                    spawnY -= 16;
                }
                
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int npcIndex = NPC.NewNPC(player.GetSource_ItemUse(Item), spawnX, spawnY, 
                        ModContent.NPCType<NachtmusikQueenOfRadiance>());
                    
                    if (Main.netMode == NetmodeID.Server && npcIndex < Main.maxNPCs)
                    {
                        NetMessage.SendData(MessageID.SyncNPC, number: npcIndex);
                    }
                }
                
                // Summon VFX
                SpawnSummonEffects(player, new Vector2(spawnX, spawnY));
            }
            
            return true;
        }
        
        private void SpawnSummonEffects(Player player, Vector2 spawnPos)
        {
            // Rising starlight from player to spawn point
            for (int i = 0; i < 20; i++)
            {
                float progress = i / 20f;
                Vector2 pos = Vector2.Lerp(player.Center, spawnPos, progress);
                pos += Main.rand.NextVector2Circular(30f, 30f);
                
                Color color = Color.Lerp(DeepPurple, Gold, progress);
                CustomParticles.GenericFlare(pos, color, 0.4f + progress * 0.3f, 20);
            }
            
            // Expanding starbursts at spawn
            for (int i = 0; i < 8; i++)
            {
                Color color = Color.Lerp(Violet, Gold, i / 8f);
                var summonBurst = new StarBurstParticle(spawnPos, Vector2.Zero, color, 0.35f + i * 0.08f, 18 + i * 2, i % 2);
                MagnumParticleHandler.SpawnParticle(summonBurst);
            }
            
            // Star particles at player
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 vel = angle.ToRotationVector2() * 3f + new Vector2(0, -2f);
                CustomParticles.GenericFlare(player.Center + angle.ToRotationVector2() * 40f, Violet, 0.35f, 15);
            }
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Summons Nachtmusik, Queen of Radiance"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Can only be used at night"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The stars themselves tremble at her approach'")
            {
                OverrideColor = new Color(123, 104, 238)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfNachtmusik>(), 1)
                .AddIngredient(ItemID.FallenStar, 15)
                .AddIngredient(ItemID.SoulofLight, 10)
                .AddIngredient(ItemID.SoulofNight, 10)
                .AddTile<FatesCosmicAnvilTile>()
                .Register();
        }
    }
}
