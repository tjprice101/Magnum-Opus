using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Content.Eroica.Enemies;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Eroica.Tools
{
    /// <summary>
    /// Eroica's Pickaxe - a powerful pickaxe crafted from Eroica materials.
    /// Higher tier than Moonlight's Pickaxe - can mine Swan Lake Resonant Ore.
    /// </summary>
    public class EroicasPickaxe : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 101; // Higher than Moonlight (81)
            Item.DamageType = DamageClass.Melee;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 3; // Faster than Moonlight (4)
            Item.useAnimation = 7; // Faster than Moonlight (8)
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6.5f;
            Item.value = Item.sellPrice(gold: 18);
            Item.rare = ModContent.RarityType<EroicaRarity>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.useTurn = true;

            // Pickaxe power - stronger than Moonlight (300%)
            Item.pick = 350; // Can mine La Campanella ore (progression: Moonlight → Eroica → La Campanella → Enigma → Swan Lake → Fate)
            
            // Enable reforging
            Item.maxStack = 1;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfEroica>(), 20)
                .AddIngredient(ModContent.ItemType<EroicasResonantEnergy>(), 15)
                .AddIngredient(ModContent.ItemType<ShardOfTriumphsTempo>(), 5)
                .AddIngredient(ItemID.SoulofMight, 15)
                .AddTile(ModContent.TileType<MoonlightSonata.CraftingStations.MoonlightAnvilTile>())
                .Register();
        }

        public override void MeleeEffects(Player player, Microsoft.Xna.Framework.Rectangle hitbox)
        {
            // Scarlet red and black particles
            if (Main.rand.NextBool(2))
            {
                // Main scarlet red dust
                Dust dust = Dust.NewDustDirect(new Microsoft.Xna.Framework.Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.RedTorch, player.velocity.X * 0.2f, player.velocity.Y * 0.2f, 150, default, 1.3f);
                dust.noGravity = true;
                dust.velocity *= 1.4f;
            }

            if (Main.rand.NextBool(3))
            {
                // Black smoke accents
                Dust dust2 = Dust.NewDustDirect(new Microsoft.Xna.Framework.Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.Smoke, 0f, 0f, 100, default, 1.0f);
                dust2.noGravity = true;
                dust2.velocity = Main.rand.NextVector2Circular(2.5f, 2.5f);
            }

            if (Main.rand.NextBool(4))
            {
                // Occasional bright flame sparkle
                Dust sparkle = Dust.NewDustDirect(new Microsoft.Xna.Framework.Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.Torch, 0f, 0f, 0, default, 0.8f);
                sparkle.noGravity = true;
                sparkle.velocity *= 0.5f;
            }

            // Music notes in tool swing
            if (Main.rand.NextBool(4))
            {
                Microsoft.Xna.Framework.Vector2 notePos = hitbox.Center.ToVector2();
                ThemedParticles.EroicaMusicNotes(notePos, 2, 15f);
            }
        }
    }
}
