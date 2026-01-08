using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.GameContent.ItemDropRules;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// UI System that manages the Harmonic Core interface.
    /// </summary>
    public class HarmonicCoreUISystem : ModSystem
    {
        internal HarmonicCoreUIState UIState;
        private UserInterface userInterface;
        
        public override void Load()
        {
            if (!Main.dedServ)
            {
                UIState = new HarmonicCoreUIState();
                UIState.Activate();
                userInterface = new UserInterface();
                userInterface.SetState(UIState);
            }
        }
        
        public override void Unload()
        {
            UIState = null;
            userInterface = null;
        }
        
        public override void UpdateUI(GameTime gameTime)
        {
            if (ShouldShowUI())
                userInterface?.Update(gameTime);
        }
        
        private bool ShouldShowUI()
        {
            if (Main.gameMenu || Main.LocalPlayer == null || !Main.LocalPlayer.active)
                return false;
            
            var player = Main.LocalPlayer.GetModPlayer<HarmonicCoreModPlayer>();
            return Main.playerInventory && player.HasUnlockedHarmonicSlots;
        }
        
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
            if (inventoryIndex != -1)
            {
                layers.Insert(inventoryIndex + 1, new LegacyGameInterfaceLayer(
                    "MagnumOpus: Harmonic Core",
                    delegate
                    {
                        if (ShouldShowUI())
                        {
                            UIState.RefreshDisplay();
                            userInterface.Draw(Main.spriteBatch, new GameTime());
                        }
                        return true;
                    },
                    InterfaceScaleType.UI));
            }
        }
    }
    
    /// <summary>
    /// Tracks Moon Lord kills and adds Heart of Music to loot (once per player).
    /// </summary>
    public class MoonLordKillTracker : GlobalNPC
    {
        public override void OnKill(NPC npc)
        {
            if (npc.type == NPCID.MoonLordCore)
            {
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    if (Main.player[i].active)
                    {
                        var modPlayer = Main.player[i].GetModPlayer<HarmonicCoreModPlayer>();
                        modPlayer.HasKilledMoonLord = true;
                        
                        // Drop Heart of Music only if player hasn't unlocked slots yet
                        if (!modPlayer.HasUnlockedHarmonicSlots)
                        {
                            int itemIndex = Item.NewItem(
                                npc.GetSource_Loot(),
                                npc.getRect(),
                                ModContent.ItemType<Content.Items.HeartOfMusic>(),
                                1);
                            
                            // Sync in multiplayer
                            if (Main.netMode == NetmodeID.Server && itemIndex >= 0)
                                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, itemIndex);
                        }
                    }
                }
            }
        }
    }
}
