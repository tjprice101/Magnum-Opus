using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Buffs
{
    /// <summary>
    /// Lunar Resonance — DoT debuff applied by Incisor beam/explosion hits.
    /// Enemies vibrate at a destructive harmonic frequency.
    /// </summary>
    public class LunarResonanceDebuff : ModBuff
    {
        // Use a vanilla frost-themed buff icon
        public override string Texture => "Terraria/Images/Buff_44";

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.lifeRegen -= 2400;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.lifeRegen -= 60;
        }
    }
}
