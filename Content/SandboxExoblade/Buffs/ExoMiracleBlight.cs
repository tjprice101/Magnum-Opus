using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.SandboxExoblade.Buffs
{
    public class ExoMiracleBlight : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.LongerExpertDebuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.lifeRegen -= 80;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.lifeRegen -= 3000;
        }
    }
}
