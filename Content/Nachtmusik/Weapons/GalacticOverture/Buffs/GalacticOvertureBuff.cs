using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.Nachtmusik.Weapons.GalacticOverture.Projectiles;

namespace MagnumOpus.Content.Nachtmusik.Weapons.GalacticOverture.Buffs
{
    /// <summary>
    /// Galactic Overture Buff — active while the player owns a Celestial Muse minion.
    /// Uses vanilla buff texture to avoid needing a custom PNG.
    /// </summary>
    public class GalacticOvertureBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<CelestialMuseMinion>()] > 0)
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
