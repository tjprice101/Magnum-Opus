using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Nachtmusik.Accessories
{
    /// <summary>Lullaby: Minion attacks at night 10% chance — -15% movement speed, -5 defense for 3s.</summary>
    public class Lullaby : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/Lullaby";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }

    /// <summary>Serenade Echo: 12% on magic hit — +10% magic damage taken for 4s (6s at night).</summary>
    public class SerenadeEcho : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/SerenadeEcho";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }

    /// <summary>Sotto Voce: Enemies that hit you at night for 2s — -10% attack speed.</summary>
    public class SottoVoce : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/SottoVoce";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }
}
