using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.OdeToJoy.Weapons.TheStandingOvation.Projectiles;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TheStandingOvation.Buffs
{
    /// <summary>
    /// Standing Ovation Buff — active while player owns ovation minions.
    /// </summary>
    public class StandingOvationBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<StandingOvationMinion>()] > 0)
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
