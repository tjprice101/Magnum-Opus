using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.SymphonicBellfireAnnihilator
{
    /// <summary>
    /// BellfireCrescendoBuff — +10% damage and +10% attack speed for 30 seconds.
    /// Granted on completing a 10-shot volley.
    /// </summary>
    public class BellfireCrescendoBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Generic) += 0.10f;
            player.GetAttackSpeed(DamageClass.Generic) += 0.10f;
        }
    }
}
