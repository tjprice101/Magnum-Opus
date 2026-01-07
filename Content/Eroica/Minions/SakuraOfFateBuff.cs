using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Minions
{
    /// <summary>
    /// Buff for the Sakura of Fate minion.
    /// </summary>
    public class SakuraOfFateBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // If the player has the buff, check for minions
            if (player.ownedProjectileCounts[ModContent.ProjectileType<SakuraOfFate>()] > 0)
            {
                player.buffTime[buffIndex] = 18000; // Keep the buff active
            }
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
}
