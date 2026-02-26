using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.SandboxExoblade.Buffs
{
    public class ExoGlacialState : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.LongerExpertDebuff[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            // Freeze the NPC briefly
            npc.velocity *= 0f;
            npc.DelBuff(buffIndex);
            buffIndex--;
        }
    }
}
