using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.OdeToJoy.Weapons.FountainOfJoyousHarmony.Projectiles;

namespace MagnumOpus.Content.OdeToJoy.Weapons.FountainOfJoyousHarmony.Buffs
{
    /// <summary>
    /// Joyous Fountain Buff — active while player owns a fountain minion.
    /// Uses a vanilla buff texture to avoid needing a custom PNG.
    /// </summary>
    public class JoyousFountainBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_0";

        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<JoyousFountainMinion>()] > 0)
            {
                player.buffTime[buffIndex] = 18000;
            }
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
}
