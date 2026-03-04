using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.ResonantWeapons.DestinysCrescendo
{
    /// <summary>
    /// Per-player state for Destiny's Crescendo.
    /// Tracks active cosmic deity minion count, summoning intensity,
    /// Escalation Phases (Pianissimo → Fortissimo), Crescendo Reset,
    /// and Deity Presence passive buffs.
    /// </summary>
    public class CrescendoPlayer : ModPlayer
    {
        /// <summary>Number of CrescendoDeityMinion projectiles currently alive.</summary>
        public int ActiveDeityCount;

        /// <summary>True while the staff is held (enables ambient VFX).</summary>
        public bool IsHoldingStaff;

        /// <summary>0-1 intensity that ramps up during active summoning.</summary>
        public float SummonIntensity;

        // === ESCALATION PHASES (Doc: deity phases up every 15s of combat) ===
        /// <summary>Current escalation phase (0=Pianissimo, 1=Piano, 2=Forte, 3=Fortissimo).</summary>
        public int EscalationPhase;

        /// <summary>Ticks in current phase. Advances to next phase at 900 (15s).</summary>
        public int PhaseTimer;
        private const int PhaseAdvanceTime = 900; // 15 seconds per phase

        /// <summary>Number of beams the deity fires per volley based on phase.</summary>
        public int BeamsPerVolley => EscalationPhase switch
        {
            0 => 1,  // Pianissimo: 1 beam
            1 => 2,  // Piano: 2 beams
            2 => 3,  // Forte: 3 beams
            _ => 5   // Fortissimo: 5 beams
        };

        /// <summary>Whether the deity should spawn star shields (Phase 2+).</summary>
        public bool HasStarShields => EscalationPhase >= 2;

        /// <summary>Whether star shields become offensive (Phase 3 only).</summary>
        public bool OffensiveShields => EscalationPhase >= 3;

        // === DEITY PRESENCE PASSIVE BUFFS ===
        /// <summary>Damage bonus from Deity Presence based on phase.</summary>
        public float DeityDamageBonus => EscalationPhase switch
        {
            0 => 0.03f,  // P1: +3%
            1 => 0.05f,  // P2: +5%
            2 => 0.08f,  // P3: +8%
            _ => 0.12f   // P4: +12%
        };

        /// <summary>Life regen bonus from Deity Presence (HP/s).</summary>
        public int DeityRegenBonus => EscalationPhase switch
        {
            0 => 0,  // P1: none
            1 => 2,  // P2: 1/s (2 in Terraria regen units)
            2 => 4,  // P3: 2/s
            _ => 6   // P4: 3/s
        };

        /// <summary>Defense bonus from Deity Presence.</summary>
        public int DeityDefenseBonus => EscalationPhase switch
        {
            0 => 0,
            1 => 0,
            2 => 5,   // P3: +5 defense
            _ => 10   // P4: +10 defense
        };

        // === CRESCENDO RESET ===
        /// <summary>Player's HP last frame for reset detection.</summary>
        private int _lastHP;

        public override void ResetEffects()
        {
            IsHoldingStaff = false;

            // Decay summon intensity when not actively summoning
            if (ActiveDeityCount <= 0)
                SummonIntensity = MathHelper.Lerp(SummonIntensity, 0f, 0.05f);
        }

        public override void PostUpdate()
        {
            // Escalation phase advancement
            if (ActiveDeityCount > 0)
            {
                PhaseTimer++;
                if (PhaseTimer >= PhaseAdvanceTime && EscalationPhase < 3)
                {
                    EscalationPhase++;
                    PhaseTimer = 0;
                }

                // Apply Deity Presence passive buffs
                Player.GetDamage(DamageClass.Generic) += DeityDamageBonus;
                Player.lifeRegen += DeityRegenBonus;
                Player.statDefense += DeityDefenseBonus;

                // Crescendo Reset: if player takes >20% HP in one frame
                int hpLoss = _lastHP - Player.statLife;
                if (hpLoss > Player.statLifeMax2 * 0.2f && EscalationPhase > 0)
                {
                    EscalationPhase = 0;
                    PhaseTimer = 0;
                }
            }
            else
            {
                // No deity = reset everything
                EscalationPhase = 0;
                PhaseTimer = 0;
            }

            _lastHP = Player.statLife;
        }

        /// <summary>Called by the item each time a deity is summoned.</summary>
        public void OnSummon()
        {
            SummonIntensity = MathHelper.Clamp(SummonIntensity + 0.25f, 0f, 1f);
        }
    }

    /// <summary>
    /// Extension: <c>player.Crescendo()</c> returns the <see cref="CrescendoPlayer"/> instance.
    /// </summary>
    public static class CrescendoPlayerExtensions
    {
        public static CrescendoPlayer Crescendo(this Player player)
            => player.GetModPlayer<CrescendoPlayer>();
    }
}
