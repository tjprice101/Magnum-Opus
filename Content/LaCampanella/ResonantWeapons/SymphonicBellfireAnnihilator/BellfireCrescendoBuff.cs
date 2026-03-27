using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.SymphonicBellfireAnnihilator.Utilities;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.SymphonicBellfireAnnihilator
{
    /// <summary>
    /// BellfireCrescendoBuff — Stacking buff from bellfire rocket kills (max 3).
    /// Each stack adds one rocket to the burst count. Visual indicator of rocket empowerment.
    /// </summary>
    public class BellfireCrescendoBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            var modPlayer = player.GetModPlayer<SymphonicBellfirePlayer>();
            // Rocket burst count is handled in player tracker; buff serves as visual indicator
            float bonus = modPlayer.BellfireCrescendoStacks * 0.05f;
            player.GetAttackSpeed(DamageClass.Ranged) += bonus;
        }
    }
}
