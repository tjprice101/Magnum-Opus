using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.HymnOfTheVictorious.Utilities
{
    public class HymnOfTheVictoriousPlayer : ModPlayer
    {
        // Hymn verses cycle 0-3 continuously, tracking full cycle completions
        public int hymnVerses;
        public int completedCycles;
        public bool isActive;
        public int activeTimer;

        /// <summary>Returns the current verse position (0-3) within the four-verse cycle.</summary>
        public int currentVerse => hymnVerses % 4;

        public override void ResetEffects()
        {
            if (!isActive)
            {
                if (activeTimer > 0)
                    activeTimer--;
                if (activeTimer <= 0)
                {
                    hymnVerses = 0;
                    completedCycles = 0;
                }
            }
            isActive = false;
        }

        public void AddVerse()
        {
            hymnVerses++;
            activeTimer = 120;

            // Track completed 4-verse cycles
            if (hymnVerses % 4 == 0 && hymnVerses > 0)
                completedCycles++;
        }

        public void ConsumeVerses()
        {
            hymnVerses = 0;
        }

        public float GetHymnIntensity()
        {
            return (hymnVerses % 4) / 3f;
        }

        public bool IsFullHymn()
        {
            return (hymnVerses % 4) == 3;
        }
    }

    public static class HymnOfTheVictoriousPlayerExtensions
    {
        public static HymnOfTheVictoriousPlayer HymnOfTheVictorious(this Player player)
            => player.GetModPlayer<HymnOfTheVictoriousPlayer>();
    }
}
