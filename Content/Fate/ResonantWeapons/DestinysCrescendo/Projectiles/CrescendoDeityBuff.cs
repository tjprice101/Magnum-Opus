using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.ResonantWeapons.DestinysCrescendo
{
    /// <summary>
    /// Buff for the CrescendoDeityMinion. Standard summon buff behavior:
    /// buffNoSave, buffNoTimeDisplay, keeps minion alive while buff is active.
    /// 
    /// Uses the existing buff icon texture at Content/Fate/Projectiles/CosmicDeityBuff.png.
    /// </summary>
    public class CrescendoDeityBuff : ModBuff
    {
        public override string Texture => "MagnumOpus/Content/Fate/Projectiles/CosmicDeityBuff";

        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<CrescendoDeityMinion>()] > 0)
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
