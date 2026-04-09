using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.LaCampanella.Accessories
{
    /// <summary>Tolling Death: 5-10% on hit — every subsequent hit strikes again at 25% less damage, applies Withered Weapon.</summary>
    public class TollingDeath : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/TollingDeath";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }
}
