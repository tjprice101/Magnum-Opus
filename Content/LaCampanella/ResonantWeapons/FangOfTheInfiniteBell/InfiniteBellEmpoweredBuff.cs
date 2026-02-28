using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.FangOfTheInfiniteBell
{
    /// <summary>
    /// Empowered state buff from Fang of the Infinite Bell.
    /// Grants infinite mana and enables empowered lightning strikes on hit.
    /// Visual indicator: golden glow around player.
    /// </summary>
    public class InfiniteBellEmpoweredBuff : ModBuff
    {
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/FangOfTheInfiniteBell/InfiniteBellEmpoweredBuff";

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = false;
            Main.pvpBuff[Type] = false;
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // Infinite mana handled via FangOfTheInfiniteBellPlayer.PostUpdate
            // Lightning effect triggered in projectile OnHitNPC when empowered
            player.GetAttackSpeed(DamageClass.Magic) += 0.10f;
        }
    }
}
