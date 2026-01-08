using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;

namespace MagnumOpus.Content.Eroica.Pets
{
    /// <summary>
    /// Bell of Eroica - Summons the Triumphant Colossus pet.
    /// Equipped in the pet slot.
    /// </summary>
    public class BellOfEroica : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 32;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.UseSound = SoundID.Item35; // Bell sound
            Item.shoot = ModContent.ProjectileType<TriumphantColossusPet>();
            Item.buffType = ModContent.BuffType<TriumphantColossusBuff>();
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ModContent.RarityType<EroicaRarity>();
        }

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            if (player.whoAmI == Main.myPlayer && player.itemTime == 0)
            {
                player.AddBuff(Item.buffType, 3600);
            }
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                player.AddBuff(Item.buffType, 3600);

                // Ring effect when summoning
                for (int i = 0; i < 15; i++)
                {
                    Dust ring = Dust.NewDustDirect(player.Center + Main.rand.NextVector2Circular(30f, 30f), 1, 1, DustID.GoldFlame, 0f, 0f, 100, default, 1.2f);
                    ring.noGravity = true;
                    ring.velocity = Main.rand.NextVector2Circular(3f, 3f);
                }

                for (int i = 0; i < 10; i++)
                {
                    Dust red = Dust.NewDustDirect(player.Center + Main.rand.NextVector2Circular(25f, 25f), 1, 1, DustID.Torch, 0f, 0f, 100, Color.DarkRed, 1.0f);
                    red.noGravity = true;
                    red.velocity = Main.rand.NextVector2Circular(2f, 2f);
                }
            }

            return true;
        }
    }
}
