using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.Eroica.Bosses;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// Scene effect for Eroica boss fight music.
    /// Uses BossHigh priority to ensure the music plays during the fight.
    /// </summary>
    public class EroicaMusicScene : ModSceneEffect
    {
        public override int Music => MusicLoader.GetMusicSlot(Mod, "Assets/Music/CrownOfEroica");
        
        public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;
        
        public override bool IsSceneEffectActive(Player player)
        {
            // Check if Eroica, God of Valor is active
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].type == ModContent.NPCType<EroicasRetribution>())
                {
                    return true;
                }
            }
            return false;
        }
    }
    
    /// <summary>
    /// System to boost Eroica boss music volume by 10%.
    /// </summary>
    public class EroicaMusicVolumeBoost : ModSystem
    {
        private static float originalMusicVolume = -1f;
        private static bool wasEroicaActive = false;
        
        public override void PostUpdateWorld()
        {
            bool eroicaActive = false;
            
            // Check if Eroica boss is active
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].type == ModContent.NPCType<EroicasRetribution>())
                {
                    eroicaActive = true;
                    break;
                }
            }
            
            // Boost volume when Eroica fight starts
            if (eroicaActive && !wasEroicaActive)
            {
                // Store original volume and boost by 50%
                originalMusicVolume = Main.musicVolume;
                Main.musicVolume = System.Math.Min(1f, Main.musicVolume * 1.5f);
            }
            // Restore volume when Eroica fight ends
            else if (!eroicaActive && wasEroicaActive && originalMusicVolume >= 0f)
            {
                Main.musicVolume = originalMusicVolume;
                originalMusicVolume = -1f;
            }
            
            wasEroicaActive = eroicaActive;
        }
        
        public override void OnWorldUnload()
        {
            // Restore volume on world unload
            if (originalMusicVolume >= 0f)
            {
                Main.musicVolume = originalMusicVolume;
                originalMusicVolume = -1f;
            }
            wasEroicaActive = false;
        }
    }
}
