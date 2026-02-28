using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict.Utilities;

namespace MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict.Buffs
{
    /// <summary>
    /// Execution Brand — marks enemies for death. Branded enemies take 30% more damage
    /// and display an intensifying death mark when below 30% HP.
    /// </summary>
    public class ExecutionBrand : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }
    }

    /// <summary>
    /// Pyre Immolation — heavy fire DoT unique to Executioner's Verdict.
    /// 60 DPS (endgame-appropriate for the heaviest hitter).
    /// </summary>
    public class PyreImmolation : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }
    }

    /// <summary>
    /// Handles damage amplification from Execution Brand + Pyre DoT.
    /// </summary>
    public class ExecutionersVerdictGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (npc.HasBuff(ModContent.BuffType<ExecutionBrand>()))
            {
                modifiers.FinalDamage *= 1.3f;
            }
        }

        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            if (npc.HasBuff(ModContent.BuffType<ExecutionBrand>()))
            {
                modifiers.FinalDamage *= 1.3f;
            }
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (npc.HasBuff(ModContent.BuffType<PyreImmolation>()))
            {
                if (npc.lifeRegen > 0)
                    npc.lifeRegen = 0;
                npc.lifeRegen -= 120; // 60 DPS
                damage = 60;
            }
        }
    }
}
