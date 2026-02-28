using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.CelestialValor.Buffs
{
    public class HeroicBurn : ModBuff
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
            npc.lifeRegen -= 60;

            if (Main.rand.NextBool(8))
            {
                int d = Dust.NewDust(npc.position, npc.width, npc.height, DustID.CrimsonTorch, 0f, -2f, 0, default, 1.4f);
                Main.dust[d].noGravity = true;
            }
        }
    }

    public class ValorStagger : ModBuff
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
            npc.velocity *= 0.80f;

            if (Main.rand.NextBool(12))
            {
                int d = Dust.NewDust(npc.position, npc.width, npc.height, DustID.GoldFlame, 0f, -1f, 0, default, 1.1f);
                Main.dust[d].noGravity = true;
            }
        }
    }
}
