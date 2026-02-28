using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.DeathTollingBell.Buffs
{
    /// <summary>
    /// Buff that represents the Death Tolling Bell summon being active.
    /// While this buff is active, the bell minion persists.
    /// </summary>
    public class DeathTollingBellBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // Check if the player still owns a DeathTollingBell minion
            if (player.ownedProjectileCounts[ModContent.ProjectileType<Projectiles.BellTollingMinion>()] > 0)
                player.buffTime[buffIndex] = 18000;
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
}
