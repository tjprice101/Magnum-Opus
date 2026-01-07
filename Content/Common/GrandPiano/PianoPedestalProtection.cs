using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Common.GrandPiano
{
    /// <summary>
    /// GlobalTile that protects the piano pedestal tiles from destruction.
    /// Works on any tile type in the protected positions.
    /// </summary>
    public class PianoPedestalProtection : GlobalTile
    {
        public override bool CanKillTile(int i, int j, int type, ref bool blockDamaged)
        {
            // Check if this position is a protected pedestal tile
            if (MoonlightSonataSystem.ProtectedPedestalTiles.Contains(new Point(i, j)))
            {
                blockDamaged = false;
                return false; // Cannot be destroyed
            }
            return base.CanKillTile(i, j, type, ref blockDamaged);
        }

        public override bool CanExplode(int i, int j, int type)
        {
            // Check if this position is a protected pedestal tile
            if (MoonlightSonataSystem.ProtectedPedestalTiles.Contains(new Point(i, j)))
            {
                return false; // Cannot be exploded
            }
            return base.CanExplode(i, j, type);
        }

        public override bool Slope(int i, int j, int type)
        {
            // Check if this position is a protected pedestal tile
            if (MoonlightSonataSystem.ProtectedPedestalTiles.Contains(new Point(i, j)))
            {
                return false; // Cannot be sloped/hammered
            }
            return base.Slope(i, j, type);
        }

        public override bool CanReplace(int i, int j, int type, int tileTypeBeingPlaced)
        {
            // Check if this position is a protected pedestal tile
            if (MoonlightSonataSystem.ProtectedPedestalTiles.Contains(new Point(i, j)))
            {
                return false; // Cannot be replaced
            }
            return base.CanReplace(i, j, type, tileTypeBeingPlaced);
        }
    }
}
