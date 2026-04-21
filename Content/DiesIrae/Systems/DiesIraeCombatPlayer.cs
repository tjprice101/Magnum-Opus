using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Systems
{
    /// <summary>
    /// ModPlayer tracking weapon-specific combat state for Dies Irae weapons.
    /// </summary>
    public class DiesIraeCombatPlayer : ModPlayer
    {
        // Wrath's Cleaver — 4-phase combo
        public int WrathsCleaverComboPhase;

        // Sin Collector — Sin economy
        public int SinCollectorSinStacks;
        public int SinCollectorSinTimer;
        public const int MaxSinStacks = 30;
        public const int SinDecayFrames = 300; // 5 seconds

        // Wrathful Contract — Kill tracking & Frenzy
        public int WrathfulContractKillCount;
        public int WrathfulContractFrenzyTimer;

        // Grimoire of Condemnation — Cast counter & kill bonus
        public int GrimoireCastCounter;
        public float GrimoireKillBonus;
        public int GrimoireKillBonusShots;

        // Arbiter's Sentence — Consecutive hit tracking
        public int ArbiterConsecutiveHits;
        public int ArbiterFocusShots;
        public int ArbiterHitTimer;

        // Damnation's Cannon — Shot counter
        public int DamnationShotCounter;

        // Executioner's Verdict — Hit counter per target
        public int ExecutionerHitCount;
        public int ExecutionerTargetWhoAmI = -1;

        // Harmony of Judgment — Execution tracking for Harmonized Verdict
        public int HarmonyRecentExecutions;
        public int HarmonyExecutionTimer;
        public bool HarmonizedVerdictActive;
        public int HarmonizedVerdictTimer;
        public const int HarmonizedVerdictWindow = 600; // 10 seconds
        public const int HarmonizedVerdictDuration = 300; // 5 seconds
        public const int ExecutionsForVerdict = 5;

        public override void ResetEffects()
        {
            // Sin decay
            if (SinCollectorSinTimer > 0)
            {
                SinCollectorSinTimer--;
                if (SinCollectorSinTimer <= 0)
                    SinCollectorSinStacks = 0;
            }

            // Frenzy decay
            if (WrathfulContractFrenzyTimer > 0)
                WrathfulContractFrenzyTimer--;

            // Arbiter hit timer decay
            if (ArbiterHitTimer > 0)
            {
                ArbiterHitTimer--;
                if (ArbiterHitTimer <= 0)
                    ArbiterConsecutiveHits = 0;
            }

            // Kill bonus shot decay
            if (GrimoireKillBonusShots <= 0)
                GrimoireKillBonus = 0f;

            // Harmony execution tracking decay
            if (HarmonyExecutionTimer > 0)
            {
                HarmonyExecutionTimer--;
                if (HarmonyExecutionTimer <= 0)
                    HarmonyRecentExecutions = 0;
            }

            // Harmonized Verdict decay
            if (HarmonizedVerdictTimer > 0)
            {
                HarmonizedVerdictTimer--;
                if (HarmonizedVerdictTimer <= 0)
                    HarmonizedVerdictActive = false;
            }
        }

        /// <summary>
        /// Adds sin stacks and resets decay timer.
        /// </summary>
        public void AddSin(int amount = 1)
        {
            SinCollectorSinStacks = System.Math.Min(SinCollectorSinStacks + amount, MaxSinStacks);
            SinCollectorSinTimer = SinDecayFrames;
        }

        /// <summary>
        /// Consumes sin stacks. Returns the tier consumed (0 = none, 1 = Penance, 2 = Absolution, 3 = Damnation).
        /// </summary>
        public int ConsumeSins()
        {
            if (SinCollectorSinStacks >= 30)
            {
                SinCollectorSinStacks -= 30;
                return 3; // Damnation
            }
            if (SinCollectorSinStacks >= 20)
            {
                SinCollectorSinStacks -= 20;
                return 2; // Absolution
            }
            if (SinCollectorSinStacks >= 10)
            {
                SinCollectorSinStacks -= 10;
                return 1; // Penance
            }
            return 0;
        }

        /// <summary>
        /// Increments Arbiter consecutive hits and resets timer.
        /// </summary>
        public void IncrementArbiterHits()
        {
            ArbiterConsecutiveHits++;
            ArbiterHitTimer = 60; // 1 second window

            if (ArbiterConsecutiveHits >= 5)
            {
                ArbiterFocusShots = 3;
                ArbiterConsecutiveHits = 0;
            }
        }

        /// <summary>
        /// Tracks hits on the same target for Executioner's Verdict.
        /// Returns the current hit count after incrementing.
        /// </summary>
        public int IncrementExecutionerHits(int targetWhoAmI)
        {
            if (ExecutionerTargetWhoAmI != targetWhoAmI)
            {
                ExecutionerTargetWhoAmI = targetWhoAmI;
                ExecutionerHitCount = 0;
            }
            ExecutionerHitCount++;
            return ExecutionerHitCount;
        }

        /// <summary>
        /// Resets Executioner hit counter after Verdict.
        /// </summary>
        public void ResetExecutionerHits()
        {
            ExecutionerHitCount = 0;
            ExecutionerTargetWhoAmI = -1;
        }

        /// <summary>
        /// Tracks an execution from Harmony of Judgment sigil.
        /// Returns true if Harmonized Verdict was triggered.
        /// </summary>
        public bool TrackHarmonyExecution()
        {
            HarmonyRecentExecutions++;
            HarmonyExecutionTimer = HarmonizedVerdictWindow;

            if (HarmonyRecentExecutions >= ExecutionsForVerdict && !HarmonizedVerdictActive)
            {
                HarmonizedVerdictActive = true;
                HarmonizedVerdictTimer = HarmonizedVerdictDuration;
                HarmonyRecentExecutions = 0;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds kill bonus for Grimoire of Condemnation.
        /// </summary>
        public void AddGrimoireKillBonus()
        {
            GrimoireKillBonus = System.Math.Min(GrimoireKillBonus + 0.05f, 0.50f);
            GrimoireKillBonusShots = 5;
        }
    }
}
