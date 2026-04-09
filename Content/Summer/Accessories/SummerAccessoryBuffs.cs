using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Summer.Accessories
{
    /// <summary>Heat Intensity: Stacking buff (max 10). 5 stacks = +12% attack speed. 10 stacks = Solar Zenith.</summary>
    public class HeatIntensity : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/HeatIntensity";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }
    }

    /// <summary>Solar Zenith: Triggered at 10 Heat Intensity stacks for 6s — +20% all damage, attacks apply Ichor.</summary>
    public class SolarZenith : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/SolarZenith";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Generic) += 0.20f;
        }
    }

    /// <summary>Scorched: Burning enemies take +5% damage from all sources for 3s.</summary>
    public class Scorched : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/Scorched";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }
}
