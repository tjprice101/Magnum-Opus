using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.Debuffs
{
    /// <summary>
    /// SpectralResonance — Requiem of Reality's stacking debuff.
    /// Stacks 1-3. At 3 stacks, triggers a delayed cosmic burst after 1 second.
    /// Visual: crimson rings around the enemy per stack.
    /// </summary>
    public class SpectralResonance : ModBuff
    {
        public const int MaxStacks = 3;
        public const int BurstDelay = 60; // 1 second
        public const float BurstDamageMultiplier = 2.5f;

        public override string Texture => "Terraria/Images/Buff_" + BuffID.Obstructed;

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<SpectralResonanceNPC>().HasResonance = true;
        }
    }

    public class SpectralResonanceNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public bool HasResonance;
        public int Stacks;
        public int BurstTimer;
        public int LastDamageReceived;
        public bool BurstPending;

        public override void ResetEffects(NPC npc)
        {
            if (!HasResonance)
            {
                Stacks = 0;
                BurstTimer = 0;
                BurstPending = false;
            }
            HasResonance = false;
        }

        /// <summary>Add a resonance stack. Returns true if we just reached max stacks.</summary>
        public bool AddStack(int damage)
        {
            LastDamageReceived = damage;
            if (Stacks < SpectralResonance.MaxStacks)
            {
                Stacks++;
                if (Stacks >= SpectralResonance.MaxStacks && !BurstPending)
                {
                    BurstPending = true;
                    BurstTimer = SpectralResonance.BurstDelay;
                    return true;
                }
            }
            return false;
        }

        public override void PostAI(NPC npc)
        {
            if (!HasResonance || !BurstPending) return;

            BurstTimer--;
            if (BurstTimer <= 0)
            {
                // Cosmic burst — deal accumulated damage
                int burstDamage = (int)(LastDamageReceived * SpectralResonance.BurstDamageMultiplier);
                npc.SimpleStrikeNPC(burstDamage, 0, false, 0f, null, false, 0f, true);

                // VFX: crimson expanding ring burst
                if (!Main.dedServ)
                {
                    for (int i = 0; i < 15; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 15f;
                        Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                        Dust d = Dust.NewDustPerfect(npc.Center, DustID.GemRuby, vel, 0, default, 1.5f);
                        d.noGravity = true;
                    }
                    // Big flash
                    for (int i = 0; i < 5; i++)
                    {
                        Dust flash = Dust.NewDustPerfect(npc.Center, DustID.PinkTorch, Vector2.Zero, 0, default, 2f);
                        flash.noGravity = true;
                    }
                }

                // Reset stacks
                Stacks = 0;
                BurstPending = false;
                BurstTimer = 0;
            }

            // Per-stack VFX: orbiting crimson rings
            if (!Main.dedServ && Stacks > 0 && Main.GameUpdateCount % 6 == 0)
            {
                for (int s = 0; s < Stacks; s++)
                {
                    float angle = Main.GameUpdateCount * 0.05f + MathHelper.TwoPi * s / Stacks;
                    float radius = 20f + s * 8f;
                    Vector2 pos = npc.Center + angle.ToRotationVector2() * radius;
                    Dust d = Dust.NewDustPerfect(pos, DustID.GemRuby, Vector2.Zero, 0, default, 0.8f);
                    d.noGravity = true;
                    d.velocity *= 0.1f;
                }
            }
        }
    }
}
