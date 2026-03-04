using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.Nachtmusik.Weapons.CelestialChorusBaton.Projectiles;

namespace MagnumOpus.Content.Nachtmusik.Weapons.CelestialChorusBaton.Buffs
{
    /// <summary>
    /// Celestial Chorus Baton Buff — active while the player owns a Nocturnal Guardian minion.
    /// Uses vanilla buff texture to avoid needing a custom PNG.
    /// </summary>
    public class CelestialChorusBatonBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_1";

        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<NocturnalGuardianMinion>()] > 0)
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
