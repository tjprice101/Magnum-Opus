using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThornSprayRepeater.Utilities
{
    /// <summary>
    /// Tracks Thorn Spray Repeater state per player:
    /// - ShotsFired: counts toward Bloom Reload (36 shots = reload)
    /// - BloomReloadActive: 1s pause + pollen burst
    /// - BloomThornCount: first 6 shots post-reload are Bloom Thorns
    /// - SpreadAccum: spread widens with sustained fire
    /// </summary>
    public class ThornSprayPlayer : ModPlayer
    {
        public int ShotsFired;
        public int BloomReloadTimer;
        public bool BloomReloadActive;
        public int BloomThornCount;
        public float SpreadAccum;
        
        private int _resetTimer;

        public override void ResetEffects()
        {
            // Reset spread accumulation if not firing
            if (_resetTimer > 0)
                _resetTimer--;
            else
            {
                SpreadAccum = 0f;
                ShotsFired = 0;
            }
        }

        /// <summary>
        /// Call when a thorn is fired. Returns true if this shot should be a Bloom Thorn.
        /// </summary>
        public bool RegisterShot()
        {
            _resetTimer = 30; // 0.5s grace period before reset
            ShotsFired++;
            SpreadAccum += 0.5f;
            if (SpreadAccum > 6f) SpreadAccum = 6f;

            bool isBloom = BloomThornCount > 0;
            if (isBloom) BloomThornCount--;

            // Bloom Reload at 36 shots
            if (ShotsFired >= 36)
            {
                BloomReloadActive = true;
                BloomReloadTimer = 60; // 1 second
                ShotsFired = 0;
                BloomThornCount = 6;
                SpreadAccum = 0f;
            }

            return isBloom;
        }

        /// <summary>
        /// Tick bloom reload. Returns true while reload is active.
        /// </summary>
        public bool TickBloomReload()
        {
            if (!BloomReloadActive) return false;

            BloomReloadTimer--;
            if (BloomReloadTimer <= 0)
            {
                BloomReloadActive = false;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Gets current spread angle in radians based on accumulated shots.
        /// Crouching (velocity.Y == 0 and not moving) tightens spread.
        /// </summary>
        public float GetSpreadAngle()
        {
            float baseSpread = 3f + SpreadAccum;
            if (Player.velocity.Length() < 0.5f)
                baseSpread *= 0.3f; // Precision Spray: crouching = tight spread
            return MathHelper.ToRadians(baseSpread);
        }
    }
}
