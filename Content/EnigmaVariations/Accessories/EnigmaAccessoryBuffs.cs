using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.EnigmaVariations.Accessories
{
    /// <summary>Paradox: 8-10% chance on hit — applies random debuff (Confused, Slow, Cursed Inferno, or Ichor).</summary>
    public class Paradox : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/Paradox";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }

    /// <summary>Paradox Rush: 12% chance on hit, stackable (max 5). At 5 stacks triggers Void Collapse.</summary>
    public class ParadoxRush : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/ParadoxRush";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }

    /// <summary>Void Collapse: At 5 ParadoxRush stacks — deals 2% of boss's current HP as damage.</summary>
    public class VoidCollapse : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/VoidCollapse";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }
}
