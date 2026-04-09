using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Accessories
{
    /// <summary>
    /// Global NPC tracking Dies Irae accessory debuffs on enemies.
    /// Wrathfire stacks (Ember), Confutatis (Ember 5-stack), Chains of Requiem (Requiem's Shackle),
    /// Condemned/Recordare (Seal of Damnation), Judgment stacks + Day of Wrath (Chain of Final Judgment).
    /// </summary>
    public class DiesIraeAccessoryGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        // --- Confutatis (Ember of the Condemned magic crits) ---
        public int confutatisTimer;

        // --- Chains of Requiem (Requiem's Shackle ranged crits) ---
        public int chainsOfRequiemTimer;

        // --- Condemned (Seal of Damnation minion hits) ---
        public int condemnedTimer;

        public override void ResetEffects(NPC npc)
        {
            if (confutatisTimer > 0) confutatisTimer--;
            if (chainsOfRequiemTimer > 0) chainsOfRequiemTimer--;
            if (condemnedTimer > 0) condemnedTimer--;
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            // Confutatis: +15% damage taken, -10 defense
            if (confutatisTimer > 0)
            {
                modifiers.FinalDamage += 0.15f;
                modifiers.Defense.Flat -= 10;
            }

            // Chains of Requiem: +15% damage taken
            if (chainsOfRequiemTimer > 0)
                modifiers.FinalDamage += 0.15f;

            // Condemned: +15% minion damage taken
            if (condemnedTimer > 0 && projectile.minion)
                modifiers.FinalDamage += 0.15f;
        }

        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            if (confutatisTimer > 0)
            {
                modifiers.FinalDamage += 0.15f;
                modifiers.Defense.Flat -= 10;
            }

            if (chainsOfRequiemTimer > 0)
                modifiers.FinalDamage += 0.15f;
        }

        public override void PostAI(NPC npc)
        {
            // Chains of Requiem: -25% movement speed, no regen
            if (chainsOfRequiemTimer > 0)
            {
                npc.velocity *= 0.75f;
                if (npc.lifeRegen > 0)
                    npc.lifeRegen = 0;
            }

            // Condemned: -5 defense
            if (condemnedTimer > 0)
                npc.defense = System.Math.Max(0, npc.defense - 5);
        }

        /// <summary>Apply Confutatis: -10 defense, +15% damage taken, 4 seconds.</summary>
        public void ApplyConfutatis(NPC npc)
        {
            confutatisTimer = 240; // 4 seconds
        }

        /// <summary>Apply Chains of Requiem: -25% speed, +15% damage taken, no regen, 4 seconds.</summary>
        public void ApplyChainsOfRequiem(NPC npc)
        {
            chainsOfRequiemTimer = 240; // 4 seconds
        }

        /// <summary>Apply Condemned mark for 5 seconds.</summary>
        public void ApplyCondemned(NPC npc)
        {
            condemnedTimer = 300; // 5 seconds
        }

        public bool HasCondemned => condemnedTimer > 0;
        public bool HasChainsOfRequiem => chainsOfRequiemTimer > 0;
        public bool HasConfutatis => confutatisTimer > 0;

        // Legacy compatibility
        public void ApplyChainsOfJudgment(NPC npc) => ApplyChainsOfRequiem(npc);
        public bool condemnedMark { get => condemnedTimer > 0; set { if (value) condemnedTimer = 300; } }
        public void AddWrathfireStack(NPC npc, int damageDealt) => ApplyConfutatis(npc);
        public static void ExtendAllChains() { }
    }
}