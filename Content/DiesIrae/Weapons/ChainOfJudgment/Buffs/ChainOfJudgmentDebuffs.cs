using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.ChainOfJudgment.Buffs
{
    /// <summary>
    /// Chain Link Mark — stacks via repeated hits (max 5).
    /// Each stack increases damage from all sources by 6%.
    /// At 5 stacks, triggers Fully Bound conversion.
    /// </summary>
    public class ChainLinkMark : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_24";

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }
    }

    /// <summary>
    /// Fully Bound — applied at 5 Chain Link stacks.
    /// Immobilizes enemy for 2 seconds, +30% damage taken from all sources.
    /// On death while Fully Bound, triggers chain shrapnel burst.
    /// </summary>
    public class FullyBound : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_24";

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }
    }

    /// <summary>
    /// Handles Chain Link Mark stacking → Fully Bound conversion,
    /// Fully Bound damage amplification, movement suppression, and death shrapnel.
    /// </summary>
    public class ChainOfJudgmentGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        /// <summary>Tracks chain link stacks (0–5). Written by projectile on hit.</summary>
        public int ChainLinkStacks;

        /// <summary>Timer to debounce stack increments so rapid multi-ticks don't over-stack.</summary>
        private int stackCooldown;

        public override void ResetEffects(NPC npc)
        {
            if (stackCooldown > 0) stackCooldown--;
        }

        /// <summary>
        /// Call from projectile OnHitNPC to increment chain link stacks.
        /// Returns the current stack count after increment.
        /// </summary>
        public int IncrementChainLink(NPC npc)
        {
            if (stackCooldown > 0) return ChainLinkStacks;
            stackCooldown = 8; // brief cooldown between stacks

            ChainLinkStacks++;
            if (ChainLinkStacks >= 5)
            {
                ChainLinkStacks = 0;
                // Remove ChainLinkMark, apply FullyBound
                int markIdx = npc.FindBuffIndex(ModContent.BuffType<ChainLinkMark>());
                if (markIdx >= 0) npc.DelBuff(markIdx);
                npc.AddBuff(ModContent.BuffType<FullyBound>(), 120); // 2 seconds
            }
            return ChainLinkStacks;
        }

        // ─── Damage amplification ───────────────────────────────────────

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            // Chain Link Mark: +6% per stack
            if (npc.HasBuff(ModContent.BuffType<ChainLinkMark>()) && ChainLinkStacks > 0)
                modifiers.FinalDamage *= 1f + 0.06f * ChainLinkStacks;

            // Fully Bound: +30%
            if (npc.HasBuff(ModContent.BuffType<FullyBound>()))
                modifiers.FinalDamage *= 1.3f;
        }

        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            if (npc.HasBuff(ModContent.BuffType<ChainLinkMark>()) && ChainLinkStacks > 0)
                modifiers.FinalDamage *= 1f + 0.06f * ChainLinkStacks;

            if (npc.HasBuff(ModContent.BuffType<FullyBound>()))
                modifiers.FinalDamage *= 1.3f;
        }

        // ─── Movement suppression for Fully Bound ───────────────────────

        public override void PostAI(NPC npc)
        {
            if (npc.HasBuff(ModContent.BuffType<FullyBound>()))
            {
                npc.velocity *= 0.05f; // Nearly immobilized
            }
        }

        // ─── Death shrapnel ─────────────────────────────────────────────

        public override void OnKill(NPC npc)
        {
            if (npc.HasBuff(ModContent.BuffType<FullyBound>()))
            {
                Utilities.ChainOfJudgmentUtils.DoShrapnelBurst(npc.Center, 40);
            }
        }
    }
}
