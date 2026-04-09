using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Accessories
{
    /// <summary>Heroic Surge: Triggered on kill for 5s — +25% damage.</summary>
    public class HeroicSurge : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/HeroicSurge";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Generic) += 0.25f;
        }
    }
}
