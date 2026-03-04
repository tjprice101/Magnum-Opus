using System;
using MagnumOpus.Content.DiesIrae;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.ArbitersSentence.Buffs
{
    /// <summary>
    /// Judgment Flame — stacking debuff (1-5). Each stack deals 15 damage/s.
    /// At 5 stacks: Sentence Cage triggers — enemy rooted, next hit deals 2x.
    /// </summary>
    public class JudgmentFlameDebuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_24";

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }

    /// <summary>
    /// Sentence Cage — applied at 5 Judgment Flame stacks.
    /// Enemy rooted for 1s, next hit deals 2x damage.
    /// </summary>
    public class SentenceCageDebuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_24";

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }

    /// <summary>
    /// GlobalNPC tracking Judgment Flame stacks and implementing Sentence Cage mechanics.
    /// </summary>
    public class ArbitersSentenceGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public int JudgmentFlameStacks { get; set; } = 0;
        public int FlameStackCooldown { get; set; } = 0;
        public bool IsSentenced { get; set; } = false;
        public int ConsecutiveHits { get; set; } = 0;
        public int ConsecutiveHitTimer { get; set; } = 0;
        public bool HasFocus { get; set; } = false;
        public int FocusShotsRemaining { get; set; } = 0;

        /// <summary>
        /// Increments Judgment Flame stacks on target. At 5 stacks → Sentence Cage.
        /// </summary>
        public void IncrementFlameStack(NPC npc)
        {
            if (FlameStackCooldown > 0) return;

            JudgmentFlameStacks++;
            FlameStackCooldown = 10; // Small debounce

            // Apply stacking debuff visual
            npc.AddBuff(ModContent.BuffType<JudgmentFlameDebuff>(), 180);

            if (JudgmentFlameStacks >= 5)
            {
                // Sentence Cage! Root enemy and prime for 2x hit
                IsSentenced = true;
                npc.AddBuff(ModContent.BuffType<SentenceCageDebuff>(), 60); // 1 second root
                JudgmentFlameStacks = 0; // Reset stacks after sentencing
            }
        }

        /// <summary>
        /// Track consecutive hits for Arbiter's Focus crosshair.
        /// 5 consecutive hits on same target → Focus mode.
        /// </summary>
        public void TrackConsecutiveHit(NPC npc)
        {
            ConsecutiveHits++;
            ConsecutiveHitTimer = 120; // 2s window

            if (ConsecutiveHits >= 5 && !HasFocus)
            {
                HasFocus = true;
                FocusShotsRemaining = 3;
                ConsecutiveHits = 0;

                // Focus activation VFX
                Utilities.ArbitersSentenceUtils.SpawnFocusDust(npc.Center);
            }
        }

        /// <summary>
        /// Consume one Focus shot. Returns true if there was a Focus charge remaining.
        /// </summary>
        public bool TryConsumeFocusShot()
        {
            if (!HasFocus || FocusShotsRemaining <= 0) return false;
            FocusShotsRemaining--;
            if (FocusShotsRemaining <= 0)
                HasFocus = false;
            return true;
        }

        public override void ResetEffects(NPC npc)
        {
            if (FlameStackCooldown > 0) FlameStackCooldown--;
            if (ConsecutiveHitTimer > 0)
            {
                ConsecutiveHitTimer--;
                if (ConsecutiveHitTimer <= 0)
                    ConsecutiveHits = 0;
            }
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            // Judgment Flame DoT: 15 damage/s per stack (30 life regen reduction per stack)
            if (npc.HasBuff(ModContent.BuffType<JudgmentFlameDebuff>()) && JudgmentFlameStacks > 0)
            {
                int stackDps = JudgmentFlameStacks * 30; // 15 damage/s per stack = 30 liferegen reduction
                npc.lifeRegen -= stackDps;
                if (damage < JudgmentFlameStacks * 15)
                    damage = JudgmentFlameStacks * 15;
            }
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            // Sentence Cage: next hit deals 2x
            if (IsSentenced && npc.HasBuff(ModContent.BuffType<SentenceCageDebuff>()))
            {
                modifiers.FinalDamage *= 2f;
                IsSentenced = false;
            }

            // Arbiter's Focus: +40% bonus on focus shots
            if (HasFocus && FocusShotsRemaining > 0 &&
                projectile.type == ModContent.ProjectileType<Projectiles.JudgmentFlameProjectile>())
            {
                modifiers.FinalDamage *= 1.4f;
            }
        }

        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            if (IsSentenced && npc.HasBuff(ModContent.BuffType<SentenceCageDebuff>()))
            {
                modifiers.FinalDamage *= 2f;
                IsSentenced = false;
            }
        }

        public override void AI(NPC npc)
        {
            // Sentence Cage: root enemy (suppress movement)
            if (npc.HasBuff(ModContent.BuffType<SentenceCageDebuff>()))
            {
                npc.velocity *= 0.05f;
            }
        }

        public override void OnKill(NPC npc)
        {
            // Final Judgment: flame transfer on kill of a stacked enemy
            if (JudgmentFlameStacks > 0 || IsSentenced)
            {
                // Find nearest enemy for flame transfer
                float minDist = 400f; // 25 tile range
                NPC closest = null;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC candidate = Main.npc[i];
                    if (candidate.active && !candidate.friendly && candidate.whoAmI != npc.whoAmI && candidate.CanBeChasedBy())
                    {
                        float dist = Vector2.Distance(npc.Center, candidate.Center);
                        if (dist < minDist)
                        {
                            minDist = dist;
                            closest = candidate;
                        }
                    }
                }

                if (closest != null)
                {
                    // Transfer flames
                    var targetGlobal = closest.GetGlobalNPC<ArbitersSentenceGlobalNPC>();
                    int transferStacks = Math.Max(JudgmentFlameStacks, 2);
                    targetGlobal.JudgmentFlameStacks = Math.Min(targetGlobal.JudgmentFlameStacks + transferStacks, 4);
                    closest.AddBuff(ModContent.BuffType<JudgmentFlameDebuff>(), 180);

                    // Flame transfer VFX
                    Utilities.ArbitersSentenceUtils.DoFlameTransfer(npc.Center, closest.Center);
                }
            }
        }
    }
}
