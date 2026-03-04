using System;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.HymnOfTheVictorious.Utilities
{
    /// <summary>
    /// Tracks 4-verse Hymn cycle: Exordium(0) → Rising(1) → Apex(2) → Gloria(3).
    /// Complete Hymn fires all 4 simultaneously when sequence completed without pause.
    /// Encore resets to Verse 4 (Gloria) on Complete Hymn kill.
    /// </summary>
    public class HymnPlayer : ModPlayer
    {
        /// <summary>Current verse index (0=Exordium, 1=Rising, 2=Apex, 3=Gloria).</summary>
        public int CurrentVerse;

        /// <summary>How many consecutive verses fired without missing.</summary>
        public int ConsecutiveVerses;

        /// <summary>Timer since last shot — sequence breaks if > 45 frames (0.75s).</summary>
        public int LastShotTimer;

        /// <summary>Whether Complete Hymn should fire this shot.</summary>
        public bool CompleteHymnReady;

        /// <summary>Encore mode — after Complete Hymn kill, stay on Gloria for repeated cycles.</summary>
        public bool EncoreActive;

        /// <summary>Encore timer — lasts 300 frames (5s).</summary>
        public int EncoreTimer;

        /// <summary>
        /// Advance the verse cycle. Returns the verse type to fire (0-3), or -1 for Complete Hymn.
        /// </summary>
        public int AdvanceVerse()
        {
            if (LastShotTimer > 45)
            {
                // Sequence broken — reset 
                ConsecutiveVerses = 0;
                CurrentVerse = 0;
                CompleteHymnReady = false;
            }

            LastShotTimer = 0;

            if (CompleteHymnReady)
            {
                CompleteHymnReady = false;
                ConsecutiveVerses = 0;
                CurrentVerse = 0;
                return -1; // Fire Complete Hymn
            }

            int verse = CurrentVerse;
            ConsecutiveVerses++;
            CurrentVerse = (CurrentVerse + 1) % 4;

            // After completing all 4 verses, next shot is Complete Hymn
            if (ConsecutiveVerses >= 4)
            {
                CompleteHymnReady = true;
            }

            return verse;
        }

        /// <summary>
        /// Activate Encore mode after a Complete Hymn kill.
        /// </summary>
        public void TriggerEncore()
        {
            EncoreActive = true;
            EncoreTimer = 300;
            CurrentVerse = 3; // Lock to Gloria
        }

        public override void PostUpdate()
        {
            LastShotTimer++;
            if (LastShotTimer > 45 && ConsecutiveVerses > 0)
            {
                ConsecutiveVerses = 0;
                CurrentVerse = 0;
                CompleteHymnReady = false;
            }

            if (EncoreActive)
            {
                EncoreTimer--;
                if (EncoreTimer <= 0)
                {
                    EncoreActive = false;
                    CurrentVerse = 0;
                }
            }
        }
    }
}
