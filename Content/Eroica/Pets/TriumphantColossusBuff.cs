using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Pets
{
    /// <summary>
    /// Buff that keeps the Triumphant Colossus pet active.
    /// </summary>
    public class TriumphantColossusBuff : ModBuff
    {
        // Use vanilla pet buff texture as placeholder until custom sprite is made
        public override string Texture => "Terraria/Images/Buff_176"; // Companion Cube buff

        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.vanityPet[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            bool unused = false;
            player.BuffHandle_SpawnPetIfNeededAndSetTime(buffIndex, ref unused, ModContent.ProjectileType<TriumphantColossusPet>());
        }
    }
}
