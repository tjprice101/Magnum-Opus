using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Utilities
{
    /// <summary>
    /// Tracks Thornbound Reckoning combo phase, reckoning charge, and vine synergy state.
    /// </summary>
    public class ThornboundPlayer : ModPlayer
    {
        /// <summary>Current combo phase (0=Vine Wave, 1=Thorn Lash, 2=Botanical Burst)</summary>
        public int ComboPhase;

        /// <summary>Reckoning Charge (0-100). +8 per vine wave hit, +12 per thorn embed.
        /// At 100: next Phase 3 creates double thorn wall + golden burst.</summary>
        public float ReckoningCharge;

        /// <summary>Number of sequential combo completions (for tracking vine synergy)</summary>
        public int ComboCompletions;

        /// <summary>Timer to reset combo if player doesn't swing within window</summary>
        public int ComboResetTimer;

        /// <summary>Frames since last swing (for combo continuation)</summary>
        private const int ComboWindowFrames = 90; // 1.5s window

        public override void ResetEffects()
        {
            if (ComboResetTimer > 0)
            {
                ComboResetTimer--;
                if (ComboResetTimer <= 0)
                {
                    ComboPhase = 0;
                    ComboCompletions = 0;
                }
            }
        }

        /// <summary>Advances to the next combo phase and resets the combo timer.</summary>
        public void AdvanceCombo()
        {
            ComboPhase = (ComboPhase + 1) % 3;
            ComboResetTimer = ComboWindowFrames;

            if (ComboPhase == 0)
                ComboCompletions++;
        }

        /// <summary>Adds charge from vine wave hits.</summary>
        public void AddVineWaveCharge()
        {
            ReckoningCharge = MathHelper.Clamp(ReckoningCharge + 8f, 0f, 100f);
        }

        /// <summary>Adds charge from thorn embeds.</summary>
        public void AddThornEmbedCharge()
        {
            ReckoningCharge = MathHelper.Clamp(ReckoningCharge + 12f, 0f, 100f);
        }

        /// <summary>Consumes full charge. Returns true if charge was full.</summary>
        public bool TryConsumeFullCharge()
        {
            if (ReckoningCharge >= 100f)
            {
                ReckoningCharge = 0f;
                return true;
            }
            return false;
        }
    }
}
