using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.HymnOfTheVictorious.Utilities
{
    public class HymnOfTheVictoriousPlayer : ModPlayer
    {
        // Hymn verses build from 0-3, each verse amplifying the song of victory
        public int hymnVerses;
        public bool isActive;
        public int activeTimer;

        public override void ResetEffects()
        {
            if (!isActive)
            {
                if (activeTimer > 0)
                    activeTimer--;
                if (activeTimer <= 0)
                {
                    hymnVerses = 0;
                }
            }
            isActive = false;
        }

        public void AddVerse()
        {
            hymnVerses = System.Math.Min(hymnVerses + 1, 3);
            activeTimer = 120;
        }

        public void ConsumeVerses()
        {
            hymnVerses = 0;
        }

        public float GetHymnIntensity()
        {
            return hymnVerses / 3f;
        }

        public bool IsFullHymn()
        {
            return hymnVerses >= 3;
        }
    }

    public static class HymnOfTheVictoriousPlayerExtensions
    {
        public static HymnOfTheVictoriousPlayer HymnOfTheVictorious(this Player player)
            => player.GetModPlayer<HymnOfTheVictoriousPlayer>();
    }
}
