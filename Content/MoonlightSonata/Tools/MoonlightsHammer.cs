using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Common;

namespace MagnumOpus.Content.MoonlightSonata.Tools
{
    /// <summary>
    /// Moonlight's Hammer - a powerful hammer crafted from Moonlight Sonata materials.
    /// </summary>
    public class MoonlightsHammer : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 106; // +25%
            Item.DamageType = DamageClass.Melee;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 7;
            Item.useAnimation = 14;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 9f;
            Item.value = Item.sellPrice(gold: 8);
            Item.rare = ModContent.RarityType<MoonlightSonataRarity>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.useTurn = true;

            // Hammer power - significantly stronger than Luminite hammers (100%)
            Item.hammer = 125;            
            // Enable reforging
            Item.maxStack = 1;        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<MoonlightsResonantEnergy>(), 5)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfMoonlightSonata>(), 10)
                .AddIngredient(ModContent.ItemType<Enemies.ShardsOfMoonlitTempo>(), 2)
                .AddIngredient(ItemID.SoulofNight, 5)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }

        public override void MeleeEffects(Player player, Microsoft.Xna.Framework.Rectangle hitbox)
        {
            // Purple sparkles when swinging
            if (Main.rand.NextBool(2))
            {
                // Main purple dust
                Dust dust = Dust.NewDustDirect(new Microsoft.Xna.Framework.Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.PurpleTorch, player.velocity.X * 0.2f, player.velocity.Y * 0.2f, 150, default, 1.1f);
                dust.noGravity = true;
                dust.velocity *= 1.2f;
            }

            if (Main.rand.NextBool(4))
            {
                // Blue-ish purple accents
                Dust dust2 = Dust.NewDustDirect(new Microsoft.Xna.Framework.Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.PurpleCrystalShard, 0f, 0f, 100, default, 0.7f);
                dust2.noGravity = true;
                dust2.velocity = Main.rand.NextVector2Circular(2f, 2f);
            }

            if (Main.rand.NextBool(6))
            {
                // Occasional bright sparkle
                Dust sparkle = Dust.NewDustDirect(new Microsoft.Xna.Framework.Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.Enchanted_Pink, 0f, 0f, 0, default, 0.5f);
                sparkle.noGravity = true;
                sparkle.velocity *= 0.4f;
            }
        }
    }
}
