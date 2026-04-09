using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Accessories
{
    /// <summary>Shattered Moon: 10% chance on magic attack, -20% defense on enemy for 3s.</summary>
    public class ShatteredMoon : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/ShatteredMoon";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }

    /// <summary>Moonstruck: Magic attacks inflict this debuff — slowed movement, -15 defense.</summary>
    public class Moonstruck : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/Moonstruck";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }

    /// <summary>Moonlit Serenity: +50% life regen, +10 defense, -15% damage taken for 6s (stand still 3s).</summary>
    public class MoonlitSerenity : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/MoonlitSerenity";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.lifeRegen += 10;
            player.statDefense += 10;
            player.endurance += 0.15f;
        }
    }
}
