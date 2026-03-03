using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.FangOfTheInfiniteBell
{
    /// <summary>
    /// Empowered state buff from Fang of the Infinite Bell.
    /// Active at 10+ bounce stacks — indicates lightning arcs are active.
    /// Grants +10% attack speed as visual/mechanical feedback.
    /// </summary>
    public class InfiniteBellEmpoweredBuff : ModBuff
    {
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/FangOfTheInfiniteBell/InfiniteBellEmpoweredBuff";

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = false;
            Main.pvpBuff[Type] = false;
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // Lightning arcs are handled by InfiniteBellOrbProj at 10+ stacks
            player.GetAttackSpeed(DamageClass.Magic) += 0.10f;
        }
    }
}
