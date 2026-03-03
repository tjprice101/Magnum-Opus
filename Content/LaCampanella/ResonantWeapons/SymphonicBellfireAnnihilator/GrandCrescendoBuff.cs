using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.SymphonicBellfireAnnihilator.Utilities;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.SymphonicBellfireAnnihilator
{
    /// <summary>
    /// GrandCrescendoBuff — Stacking buff from crescendo wave kills (max 5).
    /// Each stack: +8% ranged damage. Visual indicator of wave empowerment.
    /// </summary>
    public class GrandCrescendoBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_24";

        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            var modPlayer = player.GetModPlayer<SymphonicBellfirePlayer>();
            float bonus = modPlayer.GrandCrescendoStacks * 0.08f;
            player.GetDamage(DamageClass.Ranged) += bonus;
        }
    }
}
