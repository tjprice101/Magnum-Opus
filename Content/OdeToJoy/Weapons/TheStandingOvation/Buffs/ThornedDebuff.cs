using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TheStandingOvation.Buffs
{
    /// <summary>
    /// Thorned debuff — 2% weapon damage/s bleed from thrown roses.
    /// Applied by ThrownRoseProjectile. 3 second duration.
    /// </summary>
    public class ThornedDebuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }

    /// <summary>
    /// Global NPC for Thorned debuff — applies 2% bleed DoT.
    /// </summary>
    public class ThornedDebuffNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (npc.HasBuff(ModContent.BuffType<ThornedDebuff>()))
            {
                // 2% of max life per second (as life regen penalty)
                int bleedPerSec = (int)(npc.lifeMax * 0.02f);
                if (bleedPerSec < 4) bleedPerSec = 4;
                npc.lifeRegen -= bleedPerSec * 2; // lifeRegen is half-units per second
                if (damage < bleedPerSec / 2) damage = bleedPerSec / 2;
            }
        }
    }
}
