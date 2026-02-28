using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.InfernalChimesCalling.Utilities
{
    /// <summary>
    /// Tracks minion hit counts for shockwave trigger (every 5th hit).
    /// </summary>
    public class InfernalChimesCallingPlayer : ModPlayer
    {
        public int MinionHitCounter;
        public const int ShockwaveThreshold = 5;

        /// <summary>Returns true when shockwave triggers.</summary>
        public bool RegisterMinionHit()
        {
            MinionHitCounter++;
            if (MinionHitCounter >= ShockwaveThreshold)
            {
                MinionHitCounter = 0;
                return true;
            }
            return false;
        }

        public override void OnRespawn() { MinionHitCounter = 0; }
    }

    public static class InfernalChimesCallingPlayerExtensions
    {
        public static InfernalChimesCallingPlayer InfernalChimesCalling(this Player player) =>
            player.GetModPlayer<InfernalChimesCallingPlayer>();
    }
}
