using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.SymphonicBellfireAnnihilator
{
    /// <summary>
    /// GrandCrescendoBuff — +20% damage and +15% attack speed for 15 seconds.
    /// Granted on completing the Grand Crescendo (every 3rd full volley).
    /// </summary>
    public class GrandCrescendoBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Generic) += 0.20f;
            player.GetAttackSpeed(DamageClass.Generic) += 0.15f;
        }
    }
}
