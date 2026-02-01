using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.SwanLake.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.SwanLake.Tools
{
    /// <summary>
    /// The Swan's Pickaxe - a graceful, powerful pickaxe crafted from Swan Lake materials.
    /// Higher tier than Eroica's Pickaxe.
    /// </summary>
    public class TheSwansPickaxe : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 185; // Near-Fate tier damage
            Item.DamageType = DamageClass.Melee;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 2; // Fast
            Item.useAnimation = 4; // Very fast
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 8f;
            Item.value = Item.sellPrice(gold: 45);
            Item.rare = ItemRarityID.Cyan;
            Item.UseSound = SoundID.Item29 with { Pitch = 0.5f, Volume = 0.6f }; // Fractal crystal sound
            Item.autoReuse = true;
            Item.useTurn = true;

            // Pickaxe power - can mine Fate ore (500), just below Fate tier
            Item.pick = 500;
            
            // Enable reforging
            Item.maxStack = 1;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Glides through stone with elegant precision'") { OverrideColor = new Color(240, 240, 255) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfSwanLake>(), 20)
                .AddIngredient(ModContent.ItemType<SwansResonanceEnergy>(), 15)
                .AddIngredient(ModContent.ItemType<RemnantOfSwansHarmony>(), 10)
                .AddIngredient(ModContent.ItemType<ShardOfTheFeatheredTempo>(), 5)
                .AddIngredient(ItemID.SoulofFlight, 12)
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
            
            // Swan feather trail on swing
            if (Main.rand.NextBool(5))
            {
                Microsoft.Xna.Framework.Vector2 swingPos = new Microsoft.Xna.Framework.Vector2(hitbox.X + hitbox.Width / 2f, hitbox.Y + hitbox.Height / 2f);
                CustomParticles.SwanFeatherTrail(swingPos, player.velocity, 0.25f);
            }

            // Music notes in tool swing
            if (Main.rand.NextBool(4))
            {
                Microsoft.Xna.Framework.Vector2 notePos = hitbox.Center.ToVector2();
                ThemedParticles.SwanLakeMusicNotes(notePos, 2, 15f);
            }
        }
    }
}
