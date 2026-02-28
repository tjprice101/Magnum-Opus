using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Buffs
{
    /// <summary>
    /// Hellfire Immolation - DoT debuff applied by Wrath's Cleaver on contact.
    /// Deals heavy fire damage over time. Stacks intensity with combo.
    /// </summary>
    public class HellfireImmolation : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_OnFire";

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.lifeRegen -= 80; // 40 DPS
            if (npc.oiled) npc.lifeRegen -= 40; // Extra if oiled
        }
    }

    /// <summary>
    /// Wrath Mark - Applied by the infernal eruption (full wrath meter).
    /// Marked enemies take 25% more damage from all sources.
    /// </summary>
    public class WrathMark : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_OnFire";

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }

    /// <summary>
    /// Global NPC hook to apply Wrath Mark damage amplification.
    /// </summary>
    public class WrathMarkGlobalNPC : GlobalNPC
    {
        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (npc.HasBuff(ModContent.BuffType<WrathMark>()))
                modifiers.FinalDamage *= 1.25f;
        }

        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            if (npc.HasBuff(ModContent.BuffType<WrathMark>()))
                modifiers.FinalDamage *= 1.25f;
        }
    }
}
