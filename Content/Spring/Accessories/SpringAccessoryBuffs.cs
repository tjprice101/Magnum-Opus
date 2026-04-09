using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Spring.Accessories
{
    /// <summary>Verdant Renewal: Triggered on killing enemies with Withered Root for 3s — +4 life regen/s (stacks 3x).</summary>
    public class VerdantRenewal : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/VerdantRenewal";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.lifeRegen += 4;
        }
    }

    /// <summary>Withered Root: 12% chance on hit for 4s — slowed movement, -10 defense.</summary>
    public class WitheredRoot : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/WitheredRoot";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }
}
