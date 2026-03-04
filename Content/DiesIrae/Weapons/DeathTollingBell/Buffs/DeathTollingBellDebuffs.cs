using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.DeathTollingBell.Buffs
{
    /// <summary>
    /// Tolled debuff — stacks 1-5 on enemies hit by toll waves.
    /// At 5 stacks, converts to Death-Mark (2x damage from all sources).
    /// </summary>
    public class TolledDebuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_24";

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }

    /// <summary>
    /// Death-Mark debuff — enemy takes 2x damage from all sources.
    /// Applied when Tolled reaches 5 stacks.
    /// </summary>
    public class DeathMarkDebuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_24";

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }

    /// <summary>
    /// GlobalNPC handling Tolled stacking and Death-Mark mechanics.
    /// </summary>
    public class DeathTollingBellGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public int tolledStacks = 0;
        private int tolledTimer = 0;

        public override void ResetEffects(NPC npc)
        {
            if (tolledTimer > 0)
                tolledTimer--;

            if (tolledTimer <= 0 && tolledStacks > 0 && !npc.HasBuff(ModContent.BuffType<TolledDebuff>()))
            {
                tolledStacks = 0;
            }
        }

        /// <summary>
        /// Increment Tolled stacks. At 5, convert to Death-Mark.
        /// </summary>
        public void IncrementTolledStack(NPC npc)
        {
            tolledStacks++;
            tolledTimer = 300; // 5 seconds

            if (!npc.HasBuff(ModContent.BuffType<TolledDebuff>()))
                npc.AddBuff(ModContent.BuffType<TolledDebuff>(), 300);

            if (tolledStacks >= 5)
            {
                // Convert to Death-Mark
                tolledStacks = 0;
                npc.DelBuff(npc.FindBuffIndex(ModContent.BuffType<TolledDebuff>()));
                npc.AddBuff(ModContent.BuffType<DeathMarkDebuff>(), 300);

                // VFX flash on Death-Mark application
                for (int i = 0; i < 12; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                    Dust d = Dust.NewDustPerfect(npc.Center, DustID.GoldFlame, vel, 0, default, 1.5f);
                    d.noGravity = true;
                }
            }
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            // Tolled stacks deal 10 DPS per stack
            if (npc.HasBuff(ModContent.BuffType<TolledDebuff>()) && tolledStacks > 0)
            {
                npc.lifeRegen -= tolledStacks * 20; // 10 DPS per stack
                if (damage < tolledStacks * 5)
                    damage = tolledStacks * 5;
            }

            // Death-Mark: passive damage from the mark itself
            if (npc.HasBuff(ModContent.BuffType<DeathMarkDebuff>()))
            {
                npc.lifeRegen -= 40; // 20 DPS
                if (damage < 10)
                    damage = 10;
            }
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            // Death-Marked enemies take 2x damage from all sources
            if (npc.HasBuff(ModContent.BuffType<DeathMarkDebuff>()))
            {
                modifiers.FinalDamage *= 2f;
            }
        }
    }
}
