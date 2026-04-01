using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Accessories
{
    /// <summary>
    /// Global NPC that handles Dies Irae accessory debuff effects on enemies.
    /// Tracks Wrathfire stacks (Ember of the Condemned), Condemned marks (Seal of Damnation),
    /// and Chains of Judgment marks (Requiem's Shackle).
    /// </summary>
    public class DiesIraeAccessoryGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        // --- Wrathfire Cascade (Ember of the Condemned) ---
        public int wrathfireStacks;
        public int wrathfireDamageAccumulated;
        public int wrathfireTickTimer;

        // --- Condemned Mark (Seal of Damnation) ---
        public bool condemnedMark;

        // --- Chains of Judgment (Requiem's Shackle) ---
        public bool chainsOfJudgment;
        public int chainsTimer;

        public override void ResetEffects(NPC npc)
        {
            // Wrathfire DoT tick
            if (wrathfireStacks > 0)
            {
                wrathfireTickTimer++;
                if (wrathfireTickTimer >= 60) // every second
                {
                    wrathfireTickTimer = 0;
                    // Each stack deals 3% of accumulated damage per second
                    int dotDamage = (int)(wrathfireDamageAccumulated * 0.03f * wrathfireStacks);
                    if (dotDamage > 0 && npc.active && !npc.dontTakeDamage)
                    {
                        npc.life -= dotDamage;
                        if (npc.life <= 0)
                        {
                            npc.life = 1;
                            npc.checkDead();
                        }
                        npc.HitEffect();
                    }

                    // Fire dust per tick
                    for (int i = 0; i < wrathfireStacks; i++)
                    {
                        Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height,
                            Terraria.ID.DustID.Torch, 0f, -1.5f, 100, default, 1.2f);
                        dust.noGravity = true;
                    }
                }
            }

            // Chains of Judgment timer decay
            if (chainsTimer > 0)
            {
                chainsTimer--;
                if (chainsTimer <= 0)
                    chainsOfJudgment = false;
            }
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (chainsOfJudgment)
                modifiers.FinalDamage += 0.20f;
        }

        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            if (chainsOfJudgment)
                modifiers.FinalDamage += 0.20f;
        }

        /// <summary>
        /// Add a Wrathfire stack to this NPC. At 5 stacks, erupts for AoE.
        /// </summary>
        public void AddWrathfireStack(NPC npc, int damageDealt)
        {
            wrathfireStacks++;
            wrathfireDamageAccumulated += damageDealt;

            // Per-hit ember dust (scales with stacks)
            for (int i = 0; i < System.Math.Min(wrathfireStacks, 3); i++)
            {
                Dust dust = Dust.NewDustDirect(npc.Center + Main.rand.NextVector2Circular(12f, 12f),
                    0, 0, Terraria.ID.DustID.Torch, 0f, -1f, 100, default, 0.8f + wrathfireStacks * 0.1f);
                dust.noGravity = true;
            }

            if (wrathfireStacks >= 5)
            {
                // ERUPTION: AoE damage to nearby enemies
                int eruptionDamage = (int)(wrathfireDamageAccumulated * 0.5f);

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC other = Main.npc[i];
                    if (other.active && other.whoAmI != npc.whoAmI && !other.friendly && other.CanBeChasedBy()
                        && Vector2.Distance(other.Center, npc.Center) < 150f)
                    {
                        other.SimpleStrikeNPC(eruptionDamage, 0, false, 0f, null, false, 0f, true);
                    }
                }

                // Eruption VFX: burst of fire dust
                for (int i = 0; i < 8; i++)
                {
                    Vector2 vel = Main.rand.NextVector2CircularEdge(4f, 4f);
                    Dust dust = Dust.NewDustDirect(npc.Center, 0, 0,
                        Terraria.ID.DustID.Torch, vel.X, vel.Y, 80, default, 1.5f);
                    dust.noGravity = true;
                }
                Lighting.AddLight(npc.Center, 1.0f, 0.5f, 0.1f);

                // Reset
                wrathfireStacks = 0;
                wrathfireDamageAccumulated = 0;
                wrathfireTickTimer = 0;
            }
        }

        /// <summary>
        /// Apply Chains of Judgment mark (+20% damage from all sources for 4s).
        /// </summary>
        public void ApplyChainsOfJudgment(NPC npc)
        {
            chainsOfJudgment = true;
            chainsTimer = 240; // 4 seconds

            // Mark VFX
            for (int i = 0; i < 2; i++)
            {
                Dust dust = Dust.NewDustDirect(npc.Center + Main.rand.NextVector2Circular(10f, 10f),
                    0, 0, Terraria.ID.DustID.RedTorch, 0f, -0.5f, 100, default, 0.9f);
                dust.noGravity = true;
            }
        }
    }
}
