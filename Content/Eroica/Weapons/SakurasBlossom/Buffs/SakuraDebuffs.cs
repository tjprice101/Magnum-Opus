using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.SakurasBlossom.Buffs
{
    public class SakuraBlight : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_24";

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.lifeRegen -= 50;

            if (Main.rand.NextBool(5))
            {
                int d = Dust.NewDust(npc.position, npc.width, npc.height, DustID.PinkTorch, 0f, -1f, 0, default, 1f);
                Main.dust[d].noGravity = true;
            }
        }
    }

    public class PetalWound : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_24";

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.defense -= 15;

            if (Main.rand.NextBool(8))
            {
                Dust d = Dust.NewDustPerfect(npc.Center + Main.rand.NextVector2Circular(npc.width / 2, npc.height / 2),
                    DustID.PinkFairy, new Microsoft.Xna.Framework.Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-2f, -0.5f)));
                d.noGravity = true;
                d.scale = 0.6f;
            }
        }
    }
}
