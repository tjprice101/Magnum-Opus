using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Systems
{
    /// <summary>
    /// ModPlayer tracking weapon-specific combat state for Ode to Joy weapons.
    /// </summary>
    public class OdeToJoyCombatPlayer : ModPlayer
    {
        // ═══════════════════════════════════════════════════════
        // Thornbound Reckoning — Thorn Convergence
        // ═══════════════════════════════════════════════════════
        public int ThornConvergenceTimer;
        public int ThornConvergenceHits;
        public int ThornConvergenceTargetWhoAmI = -1;

        /// <summary>
        /// Tracks hits for thorn convergence. Returns true if bonus applies (2+ hits in 10 frames).
        /// </summary>
        public bool TrackThornConvergence(int targetWhoAmI)
        {
            if (ThornConvergenceTargetWhoAmI != targetWhoAmI)
            {
                ThornConvergenceTargetWhoAmI = targetWhoAmI;
                ThornConvergenceHits = 0;
            }
            ThornConvergenceHits++;
            ThornConvergenceTimer = 10;
            return ThornConvergenceHits >= 2;
        }

        // ═══════════════════════════════════════════════════════
        // Rose Thorn Chainsaw — Empowerment Aura
        // ═══════════════════════════════════════════════════════
        public int ChainsawEmpowermentTimer;
        public bool ChainsawEmpowered => ChainsawEmpowermentTimer > 0;

        public void ActivateChainsawEmpowerment()
        {
            ChainsawEmpowermentTimer = 300; // 5 seconds
        }

        // ═══════════════════════════════════════════════════════
        // Hymn of the Victorious — Verse Cycle
        // ═══════════════════════════════════════════════════════
        public int HymnVerseIndex; // 0 = Exordium, 1 = Rising, 2 = Apex, 3 = Gloria
        public int HymnResonanceStacks;
        public const int MaxHymnResonance = 3;

        public void AdvanceHymnVerse()
        {
            HymnVerseIndex = (HymnVerseIndex + 1) % 4;
            if (HymnVerseIndex == 0)
            {
                // Completed a cycle
                HymnResonanceStacks = System.Math.Min(HymnResonanceStacks + 1, MaxHymnResonance);
            }
        }

        // ═══════════════════════════════════════════════════════
        // Anthem of Glory — Crescendo & Victory Fanfare
        // ═══════════════════════════════════════════════════════
        public int AnthemChannelTimer;
        public int AnthemKillsDuringChannel;
        public bool VictoryFanfareActive;
        public int VictoryFanfareTimer;

        public void IncrementAnthemChannel()
        {
            AnthemChannelTimer++;
        }

        public void ResetAnthemChannel()
        {
            AnthemChannelTimer = 0;
            AnthemKillsDuringChannel = 0;
        }

        public void TrackAnthemKill()
        {
            AnthemKillsDuringChannel++;
            if (AnthemKillsDuringChannel >= 3)
            {
                VictoryFanfareActive = true;
                VictoryFanfareTimer = 60;
                // Partial reset
                AnthemChannelTimer = (int)(AnthemChannelTimer * 0.5f);
                AnthemKillsDuringChannel = 0;
            }
        }

        // ═══════════════════════════════════════════════════════
        // Thorn Spray Repeater — Shot Counter & Bloom Reload
        // ═══════════════════════════════════════════════════════
        public int ThornSprayShotCounter;
        public int BloomReloadShotsRemaining;
        public const int BloomReloadThreshold = 36;

        public bool FireThornSpray()
        {
            ThornSprayShotCounter++;
            if (ThornSprayShotCounter >= BloomReloadThreshold)
            {
                ThornSprayShotCounter = 0;
                BloomReloadShotsRemaining = 6;
                return true; // Bloom reload triggered
            }
            return false;
        }

        public bool IsBloomReloadActive()
        {
            if (BloomReloadShotsRemaining > 0)
            {
                BloomReloadShotsRemaining--;
                return true;
            }
            return false;
        }

        // ═══════════════════════════════════════════════════════
        // Petal Storm Cannon — Hurricane Shot
        // ═══════════════════════════════════════════════════════
        public int PetalStormShotCounter;

        public bool IsHurricaneShot()
        {
            PetalStormShotCounter++;
            if (PetalStormShotCounter >= 3)
            {
                PetalStormShotCounter = 0;
                return true;
            }
            return false;
        }

        // ═══════════════════════════════════════════════════════
        // The Pollinator — Harvest Season
        // ═══════════════════════════════════════════════════════
        public int PollinatorBloomKills;
        public int HarvestSeasonShotsRemaining;
        public const int BloomKillsForHarvest = 5;

        public bool TrackBloomKill()
        {
            PollinatorBloomKills++;
            if (PollinatorBloomKills >= BloomKillsForHarvest)
            {
                PollinatorBloomKills = 0;
                HarvestSeasonShotsRemaining = 10;
                return true; // Harvest Season triggered
            }
            return false;
        }

        public bool IsHarvestSeasonActive()
        {
            if (HarvestSeasonShotsRemaining > 0)
            {
                HarvestSeasonShotsRemaining--;
                return true;
            }
            return false;
        }

        // ═══════════════════════════════════════════════════════
        // The Standing Ovation — Ovation Meter
        // ═══════════════════════════════════════════════════════
        public float OvationMeter; // 0.0 - 1.0
        public int OvationEncoreTimer;
        public int OvationDrainTimer;
        public const int EncoreDuration = 300; // 5 seconds
        public const int DrainDelay = 120; // 2 seconds out of combat

        public bool IsEncoreActive => OvationEncoreTimer > 0;

        public void AddOvationMeter(float amount)
        {
            OvationMeter = System.Math.Min(OvationMeter + amount, 1f);
            OvationDrainTimer = DrainDelay;

            if (OvationMeter >= 1f && OvationEncoreTimer <= 0)
            {
                OvationEncoreTimer = EncoreDuration;
                OvationMeter = 0f;
            }
        }

        public int GetOvationTier()
        {
            if (OvationMeter >= 1f || OvationEncoreTimer > 0) return 4; // Standing Ovation / Encore
            if (OvationMeter >= 0.75f) return 3;
            if (OvationMeter >= 0.50f) return 2;
            if (OvationMeter >= 0.25f) return 1;
            return 0;
        }

        // ═══════════════════════════════════════════════════════
        // RESET EFFECTS
        // ═══════════════════════════════════════════════════════

        public override void ResetEffects()
        {
            // Thorn convergence decay
            if (ThornConvergenceTimer > 0)
            {
                ThornConvergenceTimer--;
                if (ThornConvergenceTimer <= 0)
                {
                    ThornConvergenceHits = 0;
                    ThornConvergenceTargetWhoAmI = -1;
                }
            }

            // Chainsaw empowerment decay
            if (ChainsawEmpowermentTimer > 0)
                ChainsawEmpowermentTimer--;

            // Victory Fanfare decay
            if (VictoryFanfareTimer > 0)
            {
                VictoryFanfareTimer--;
                if (VictoryFanfareTimer <= 0)
                    VictoryFanfareActive = false;
            }

            // Encore decay
            if (OvationEncoreTimer > 0)
                OvationEncoreTimer--;

            // Ovation meter drain when out of combat
            if (OvationDrainTimer > 0)
            {
                OvationDrainTimer--;
            }
            else if (OvationMeter > 0)
            {
                OvationMeter = System.Math.Max(0f, OvationMeter - 0.002f); // Slow drain
            }
        }
    }
}
