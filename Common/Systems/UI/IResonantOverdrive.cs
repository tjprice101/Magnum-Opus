using Microsoft.Xna.Framework;
using Terraria;

namespace MagnumOpus.Common.Systems.UI
{
    public interface IResonantOverdrive
    {
        bool IsHoldingOverdriveWeapon { get; }
        float OverdriveCharge { get; }
        bool IsOverdriveReady { get; }
        Color OverdriveLowColor { get; }
        Color OverdriveHighColor { get; }
        bool ActivateOverdrive(Player player);
        bool IsOverdriveOnCooldown => false;
        string OverdriveCooldownMessage => null;
    }
}
