using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.SwanLake.Bosses;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// Scene effect for Swan Lake boss fight music.
    /// Uses BossHigh priority to ensure the music plays during the fight.
    /// </summary>
    public class SwanLakeMusicScene : ModSceneEffect
    {
        public override int Music => MusicLoader.GetMusicSlot(Mod, "Assets/Music/SwanOfAThousandChords");
        
        public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;
        
        public override bool IsSceneEffectActive(Player player)
        {
            // Check if Swan Lake boss is active
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].type == ModContent.NPCType<SwanLakeTheMonochromaticFractal>())
                {
                    return true;
                }
            }
            return false;
        }
    }
    
    /// <summary>
    /// System to boost Swan Lake boss music volume by 80% and reduce other sound effects by 30%.
    /// </summary>
    public class SwanLakeMusicVolumeBoost : ModSystem
    {
        private static float originalMusicVolume = -1f;
        private static float originalSoundVolume = -1f;
        private static bool wasSwanLakeActive = false;
        
        public override void PostUpdateWorld()
        {
            bool swanLakeActive = false;
            
            // Check if Swan Lake boss is active
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].type == ModContent.NPCType<SwanLakeTheMonochromaticFractal>())
                {
                    swanLakeActive = true;
                    break;
                }
            }
            
            // Boost volume when Swan Lake fight starts
            if (swanLakeActive && !wasSwanLakeActive)
            {
                // Store original volumes
                originalMusicVolume = Main.musicVolume;
                originalSoundVolume = Main.soundVolume;
                
                // Boost music by 80%
                Main.musicVolume = System.Math.Min(1f, Main.musicVolume * 1.8f);
                
                // Reduce sound effects by 30% (keep 70%)
                Main.soundVolume = Main.soundVolume * 0.7f;
            }
            // Restore volume when Swan Lake fight ends
            else if (!swanLakeActive && wasSwanLakeActive)
            {
                if (originalMusicVolume >= 0f)
                {
                    Main.musicVolume = originalMusicVolume;
                    originalMusicVolume = -1f;
                }
                if (originalSoundVolume >= 0f)
                {
                    Main.soundVolume = originalSoundVolume;
                    originalSoundVolume = -1f;
                }
            }
            
            wasSwanLakeActive = swanLakeActive;
        }
        
        public override void OnWorldUnload()
        {
            // Restore volumes on world unload
            if (originalMusicVolume >= 0f)
            {
                Main.musicVolume = originalMusicVolume;
                originalMusicVolume = -1f;
            }
            if (originalSoundVolume >= 0f)
            {
                Main.soundVolume = originalSoundVolume;
                originalSoundVolume = -1f;
            }
            wasSwanLakeActive = false;
        }
    }
}
