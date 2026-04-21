using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.DiesIrae;

namespace MagnumOpus.Content.DiesIrae.Systems
{
    /// <summary>
    /// GlobalNPC tracking Judgment Flame stacks from Arbiter's Sentence.
    /// 5 stacks = Sentence Cage (root + 2x damage on next hit).
    /// </summary>
    public class JudgmentFlameGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public int JudgmentFlameStacks;
        public int JudgmentFlameTimer;
        public bool SentenceCageActive;
        public int SentenceCageTimer;
        public int LastHitByOwner = -1;

        private const int StackDecayFrames = 180; // 3 seconds
        private const int MaxStacks = 5;
        private const int CageDuration = 120; // 2 seconds

        public override void ResetEffects(NPC npc)
        {
            // Stack decay
            if (JudgmentFlameTimer > 0)
            {
                JudgmentFlameTimer--;
                if (JudgmentFlameTimer <= 0)
                    JudgmentFlameStacks = 0;
            }

            // Cage decay
            if (SentenceCageTimer > 0)
            {
                SentenceCageTimer--;
                if (SentenceCageTimer <= 0)
                    SentenceCageActive = false;
            }
        }

        public override void AI(NPC npc)
        {
            // Stack DoT damage
            if (JudgmentFlameStacks > 0 && npc.lifeRegen > 0)
                npc.lifeRegen = 0;
            if (JudgmentFlameStacks > 0)
            {
                // 15 damage per second per stack
                npc.lifeRegen -= JudgmentFlameStacks * 30; // lifeRegen is in half-HP per second
            }

            // Sentence Cage root effect
            if (SentenceCageActive)
            {
                npc.velocity *= 0.85f; // Slow to near-stop

                // Cage VFX
                if (Main.rand.NextBool(3))
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 offset = angle.ToRotationVector2() * 30f;
                    Dust d = Dust.NewDustPerfect(npc.Center + offset, DustID.GoldFlame,
                        -offset * 0.03f, 0, DiesIraePalette.JudgmentGold, 0.6f);
                    d.noGravity = true;
                }
            }

            // Stack VFX
            if (JudgmentFlameStacks > 0 && Main.rand.NextBool(6 - JudgmentFlameStacks))
            {
                Color col = Color.Lerp(DiesIraePalette.InfernalRed, DiesIraePalette.JudgmentGold, (float)JudgmentFlameStacks / MaxStacks);
                Dust d = Dust.NewDustPerfect(npc.Center + Main.rand.NextVector2Circular(npc.width / 2, npc.height / 2),
                    DustID.Torch, new Vector2(0, -1f), 0, col, 0.5f + JudgmentFlameStacks * 0.15f);
                d.noGravity = true;
            }
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            // Tint based on stacks
            if (JudgmentFlameStacks > 0)
            {
                float stackProgress = (float)JudgmentFlameStacks / MaxStacks;
                Color tint = Color.Lerp(Color.White, DiesIraePalette.InfernalRed, stackProgress * 0.3f);
                drawColor = Color.Lerp(drawColor, tint, 0.2f);
            }

            // Cage golden tint
            if (SentenceCageActive)
            {
                drawColor = Color.Lerp(drawColor, DiesIraePalette.JudgmentGold, 0.25f);
            }
        }

        /// <summary>
        /// Adds a Judgment Flame stack. Returns true if Sentence Cage was triggered.
        /// </summary>
        public bool AddStack(int ownerWhoAmI)
        {
            LastHitByOwner = ownerWhoAmI;
            JudgmentFlameTimer = StackDecayFrames;

            if (JudgmentFlameStacks < MaxStacks)
                JudgmentFlameStacks++;

            // Trigger Sentence Cage at 5 stacks
            if (JudgmentFlameStacks >= MaxStacks && !SentenceCageActive)
            {
                SentenceCageActive = true;
                SentenceCageTimer = CageDuration;
                JudgmentFlameStacks = 0; // Reset stacks after cage
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if Sentence Cage double damage applies and consumes it.
        /// </summary>
        public bool ConsumeCageBonus()
        {
            if (SentenceCageActive)
            {
                SentenceCageActive = false;
                SentenceCageTimer = 0;
                return true;
            }
            return false;
        }

        public override void OnKill(NPC npc)
        {
            // Transfer flames to nearby enemies on kill
            if (JudgmentFlameStacks > 0 || SentenceCageActive)
            {
                int stacksToTransfer = System.Math.Max(JudgmentFlameStacks, SentenceCageActive ? 3 : 0);
                if (stacksToTransfer > 0)
                {
                    List<NPC> nearby = new List<NPC>();
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC other = Main.npc[i];
                        if (other.active && other.whoAmI != npc.whoAmI && other.CanBeChasedBy() &&
                            Vector2.Distance(npc.Center, other.Center) < 300f)
                        {
                            nearby.Add(other);
                        }
                    }

                    // Transfer to up to 3 nearby enemies
                    int transferred = 0;
                    foreach (var target in nearby)
                    {
                        if (transferred >= 3) break;
                        var targetFlame = target.GetGlobalNPC<JudgmentFlameGlobalNPC>();
                        for (int s = 0; s < stacksToTransfer; s++)
                            targetFlame.AddStack(LastHitByOwner);
                        transferred++;

                        // Transfer VFX
                        for (int j = 0; j < 5; j++)
                        {
                            Vector2 vel = (target.Center - npc.Center).SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(4f, 8f);
                            Dust d = Dust.NewDustPerfect(npc.Center, DustID.Torch, vel, 0, DiesIraePalette.InfernalRed, 0.9f);
                            d.noGravity = true;
                        }
                    }
                }
            }
        }
    }
}
