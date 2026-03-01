using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon.Buffs
{
    /// <summary>
    /// Tidal Drowning — a DoT debuff applied by Eternal Moon attacks.
    /// The target is engulfed in crushing tidal moonlight that saps their life force.
    /// Damage scales with the lunar phase when applied.
    /// </summary>
    public class TidalDrowning : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_24";

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            // 25 DPS tidal damage
            if (Main.GameUpdateCount % 15 == 0)
            {
                npc.lifeRegen -= 50;
            }

            // Subtle visual: purple-blue dust
            if (Main.rand.NextBool(8) && !Main.dedServ)
            {
                Dust d = Dust.NewDustDirect(npc.position, npc.width, npc.height, Terraria.ID.DustID.PurpleTorch, 0f, -1f);
                d.noGravity = true;
                d.scale = 0.8f;
                d.alpha = 100;
            }
        }
    }

    /// <summary>
    /// Lunar Stasis — a brief freeze debuff applied by the Lunar Surge dash on contact.
    /// The target is locked in crystallized moonlight for a short duration.
    /// </summary>
    public class LunarStasis : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_44";

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            // Slow the target significantly
            if (npc.knockBackResist > 0f)
            {
                npc.velocity *= 0.85f;
            }

            // Frost-like visual: ice blue dust
            if (Main.rand.NextBool(6) && !Main.dedServ)
            {
                Dust d = Dust.NewDustDirect(npc.position, npc.width, npc.height, Terraria.ID.DustID.IceTorch, 0f, -0.5f);
                d.noGravity = true;
                d.scale = 0.6f;
                d.alpha = 120;
            }
        }
    }
}
