using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Winter.Accessories
{
    /// <summary>Winter's Focus: Triggered when applying Frozen Paradox for 4s — +10% crit chance, +8% all damage.</summary>
    public class WintersFocus : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/WintersFocus";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetCritChance(DamageClass.Generic) += 10;
            player.GetDamage(DamageClass.Generic) += 0.08f;
        }
    }

    /// <summary>Frozen Paradox: Enemies w/ Frostburn + Paradox for 5s — -30% speed, take +15% damage.</summary>
    public class FrozenParadox : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/FrozenParadox";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }
}
