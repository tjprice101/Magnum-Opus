using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.SwanLake.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;

namespace MagnumOpus.Content.SwanLake.Tools
{
    /// <summary>
    /// The Swan's Hammer - a graceful, powerful hammer crafted from Swan Lake materials.
    /// Higher tier than Eroica's Hammer.
    /// </summary>
    public class TheSwansHammer : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 165; // Higher than Eroica (76)
            Item.DamageType = DamageClass.Melee;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 4; // Faster than Eroica (14)
            Item.useAnimation = 10; // Faster than Eroica (26)
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 12f;
            Item.value = Item.sellPrice(gold: 26);
            Item.rare = ItemRarityID.Cyan;
            Item.UseSound = SoundID.Item29 with { Pitch = 0.2f, Volume = 0.65f }; // Fractal crystal sound
            Item.autoReuse = true;
            Item.useTurn = true;

            // Hammer power - stronger than Eroica (140%)
            Item.hammer = 165;
            
            // Enable reforging
            Item.maxStack = 1;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfSwanLake>(), 15)
                .AddIngredient(ModContent.ItemType<SwansResonanceEnergy>(), 10)
                .AddIngredient(ModContent.ItemType<RemnantOfSwansHarmony>(), 5)
                .AddIngredient(ModContent.ItemType<ShardOfTheFeatheredTempo>(), 2)
                .AddIngredient(ItemID.SoulofFlight, 5)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }

        public override void MeleeEffects(Player player, Microsoft.Xna.Framework.Rectangle hitbox)
        {
            // Icy white and blue particles with rainbow shimmer
            if (Main.rand.NextBool(2))
            {
                // Main icy white dust
                Dust dust = Dust.NewDustDirect(new Microsoft.Xna.Framework.Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.IceTorch, player.velocity.X * 0.2f, player.velocity.Y * 0.2f, 150, default, 1.3f);
                dust.noGravity = true;
                dust.velocity *= 1.4f;
            }

            if (Main.rand.NextBool(3))
            {
                // Feathery cloud particles
                Dust dust2 = Dust.NewDustDirect(new Microsoft.Xna.Framework.Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.Cloud, 0f, -0.5f, 100, default, 1.0f);
                dust2.noGravity = true;
                dust2.velocity = Main.rand.NextVector2Circular(2f, 2f);
            }

            if (Main.rand.NextBool(4))
            {
                // Rainbow shimmer sparkle
                int dustType = Main.rand.Next(4) switch
                {
                    0 => DustID.BlueTorch,
                    1 => DustID.PurpleTorch,
                    2 => DustID.PinkTorch,
                    _ => DustID.WhiteTorch
                };
                Dust sparkle = Dust.NewDustDirect(new Microsoft.Xna.Framework.Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    dustType, 0f, 0f, 0, default, 0.7f);
                sparkle.noGravity = true;
                sparkle.velocity *= 0.5f;
            }
            
            // Swan feather burst on heavy swing
            if (Main.rand.NextBool(6))
            {
                Microsoft.Xna.Framework.Vector2 swingPos = new Microsoft.Xna.Framework.Vector2(hitbox.X + hitbox.Width / 2f, hitbox.Y + hitbox.Height / 2f);
                global::MagnumOpus.Common.Systems.CustomParticles.SwanFeatherBurst(swingPos, 3, 0.25f);
            }
        }
    }
}
