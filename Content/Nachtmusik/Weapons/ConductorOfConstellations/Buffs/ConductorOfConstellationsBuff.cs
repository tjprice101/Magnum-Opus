using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.Nachtmusik.Weapons.ConductorOfConstellations.Projectiles;

namespace MagnumOpus.Content.Nachtmusik.Weapons.ConductorOfConstellations.Buffs
{
    /// <summary>
    /// Conductor of Constellations Buff — active while the player owns a Stellar Conductor minion.
    /// Uses vanilla buff texture to avoid needing a custom PNG.
    /// </summary>
    public class ConductorOfConstellationsBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_1";

        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<StellarConductorMinion>()] > 0)
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
