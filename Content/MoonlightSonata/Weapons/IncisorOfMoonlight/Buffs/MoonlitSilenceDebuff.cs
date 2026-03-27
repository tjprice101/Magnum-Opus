using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Buffs
{
    /// <summary>
    /// Moonlit Silence — applied by the Presto Agitato shockwave (Movement III finale).
    /// Reduces enemy movement speed by 40% for 3 seconds.
    /// "The final note hangs in the air… and the world falls silent."
    /// </summary>
    public class MoonlitSilenceDebuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            // 40% speed reduction — enemies move at 60% of their normal velocity
            npc.velocity *= 0.6f;
        }
    }
}
