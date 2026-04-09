using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.SwanLake.Debuffs
{
    /// <summary>
    /// Odile's Beauty — DoT debuff from "Dying Swan's Grace" (Swan's Chromatic Diadem airborne mechanic).
    /// Deals 5% of the attacker's weapon damage per second for 5 seconds.
    /// Cannot stack or be reapplied until the duration is over.
    /// </summary>
    public class OdilesBeauty : ModBuff
    {
        public override string Texture => "MagnumOpus/Assets/BuffIcons/OdilesBeauty";

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<OdilesBeautyNPC>().HasOdilesBeauty = true;
        }
    }

    /// <summary>
    /// GlobalNPC handler for Odile's Beauty DoT.
    /// Tracks damage per tick and applies via UpdateLifeRegen.
    /// </summary>
    public class OdilesBeautyNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public bool HasOdilesBeauty { get; set; }

        /// <summary>
        /// Damage per second — 5% of the attacker's weapon damage at the time of application.
        /// </summary>
        public int DamagePerSecond { get; set; }

        public override void ResetEffects(NPC npc)
        {
            HasOdilesBeauty = false;
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (HasOdilesBeauty && DamagePerSecond > 0)
            {
                // lifeRegen is in half-HP per second (2 lifeRegen = 1 HP/sec)
                int regenReduction = DamagePerSecond * 2;
                npc.lifeRegen -= regenReduction;
                if (damage < DamagePerSecond)
                    damage = DamagePerSecond;
            }
        }

        /// <summary>
        /// Set the DoT damage. Called when the debuff is first applied.
        /// </summary>
        public void SetDamage(int weaponDamage)
        {
            DamagePerSecond = Math.Max(1, (int)(weaponDamage * 0.05f));
        }
    }
}
