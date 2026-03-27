using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Buffs
{
    /// <summary>
    /// Moonlit Stasis — brief velocity freeze on dash hit.
    /// The target is suspended in moonlight for a single frame.
    /// </summary>
    public class MoonlitStasis : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.velocity = Vector2.Zero;
            npc.DelBuff(buffIndex);
            buffIndex--;
        }
    }
}
