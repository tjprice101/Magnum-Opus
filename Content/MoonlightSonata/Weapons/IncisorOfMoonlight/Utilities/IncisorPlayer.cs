using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Utilities
{
    /// <summary>
    /// Per-player state tracker for the Incisor of Moonlight.
    /// Manages dash lunging, input listeners, and staccato hit tracking.
    /// </summary>
    public class IncisorPlayer : ModPlayer
    {
        public bool LungingDown = false;
        public bool rightClickListener = false;
        public bool mouseWorldListener = false;
        public Vector2 mouseWorld => Main.MouseWorld;

        /// <summary>
        /// Tracks staccato note hits per NPC for detonation mechanics.
        /// Key: NPC whoAmI, Value: hit count
        /// </summary>
        private Dictionary<int, int> staccatoHits = new Dictionary<int, int>();

        public override void ResetEffects()
        {
            LungingDown = false;
            rightClickListener = false;
            mouseWorldListener = false;
        }

        public override void PostUpdate()
        {
            if (!LungingDown)
                return;
            Player.fullRotation = 0f;
        }

        /// <summary>
        /// Registers a staccato note hit on the specified NPC.
        /// </summary>
        public void RegisterStaccatoHit(int npcIndex)
        {
            if (staccatoHits.ContainsKey(npcIndex))
                staccatoHits[npcIndex]++;
            else
                staccatoHits[npcIndex] = 1;
        }

        /// <summary>
        /// Gets the number of staccato hits on the specified NPC.
        /// </summary>
        public int GetStaccatoHits(int npcIndex)
        {
            return staccatoHits.ContainsKey(npcIndex) ? staccatoHits[npcIndex] : 0;
        }

        /// <summary>
        /// Resets the staccato hit counter for the specified NPC after detonation.
        /// </summary>
        public void ResetStaccatoHits(int npcIndex)
        {
            if (staccatoHits.ContainsKey(npcIndex))
                staccatoHits[npcIndex] = 0;
        }
    }

    public static class IncisorPlayerExtensions
    {
        public static IncisorPlayer Incisor(this Player player) => player.GetModPlayer<IncisorPlayer>();
    }
}
