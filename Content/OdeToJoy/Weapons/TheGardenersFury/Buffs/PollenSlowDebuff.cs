using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TheGardenersFury.Buffs
{
    /// <summary>
    /// Pollen Cloud Slow — 40% movement slow from pollen pod detonations.
    /// Golden pollen visual effect on afflicted enemies.
    /// </summary>
    public class PollenSlowDebuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_24";

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            // Slow effect handled via GlobalNPC or direct velocity modification
            npc.velocity *= 0.6f; // 40% slow

            // Golden pollen visual
            if (Main.rand.NextBool(5))
            {
                Vector2 dustPos = npc.position + new Vector2(
                    Main.rand.Next(npc.width),
                    Main.rand.Next(npc.height));
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.RainbowMk2,
                    new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-1f, -0.3f)),
                    newColor: GardenerFuryTextures.BloomGold,
                    Scale: Main.rand.NextFloat(0.2f, 0.4f));
                dust.noGravity = true;
                dust.fadeIn = 0.3f;
            }
        }
    }
}
