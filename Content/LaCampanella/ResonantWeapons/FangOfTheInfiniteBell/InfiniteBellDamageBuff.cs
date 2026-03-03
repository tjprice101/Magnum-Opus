using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.FangOfTheInfiniteBell
{
    /// <summary>
    /// Stacking damage buff from Fang of the Infinite Bell bounces.
    /// +3% magic damage per bounce stack (max 20 stacks = +60%).
    /// </summary>
    public class InfiniteBellDamageBuff : ModBuff
    {
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/FangOfTheInfiniteBell/InfiniteBellDamageBuff";

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = false;
            Main.pvpBuff[Type] = false;
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            var fbPlayer = player.GetModPlayer<Utilities.FangOfTheInfiniteBellPlayer>();
            float bonus = fbPlayer.BounceStacks * Utilities.FangOfTheInfiniteBellPlayer.DamagePerStack;
            player.GetDamage(DamageClass.Magic) += bonus;
        }
    }
}
