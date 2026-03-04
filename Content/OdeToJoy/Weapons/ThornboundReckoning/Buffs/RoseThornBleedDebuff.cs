using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Buffs
{
    /// <summary>
    /// Rose Thorn Bleed — Embedded thorns deal 3% weapon damage/s for 4s.
    /// Max 5 embeds per enemy, refreshes per embed.
    /// </summary>
    public class RoseThornBleedDebuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_24";

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }
}
