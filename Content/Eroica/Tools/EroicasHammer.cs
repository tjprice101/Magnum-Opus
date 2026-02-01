using System.Collections.Generic;
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
    /// Eroica's Hammer - a powerful hammer crafted from Eroica materials.
    /// Higher tier than Moonlight's Hammer.
    /// </summary>
    public class EroicasHammer : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 76; // Higher than Moonlight
            Item.DamageType = DamageClass.Melee;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 14;
            Item.useAnimation = 26;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 7f;
            Item.value = Item.sellPrice(gold: 14);
            Item.rare = ModContent.RarityType<EroicaRarity>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.useTurn = true;

            // Hammer power - stronger than Moonlight (125%)
            Item.hammer = 140;
            
            // Enable reforging
            Item.maxStack = 1;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Each blow rings with triumphant fury'") { OverrideColor = new Microsoft.Xna.Framework.Color(200, 50, 50) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfEroica>(), 12)
                .AddIngredient(ModContent.ItemType<EroicasResonantEnergy>(), 5)
                .AddIngredient(ModContent.ItemType<ShardOfTriumphsTempo>(), 2)
                .AddIngredient(ItemID.SoulofMight, 8)
                .AddTile(ModContent.TileType<MoonlightSonata.CraftingStations.MoonlightAnvilTile>())
                .Register();
        }

        public override void MeleeEffects(Player player, Microsoft.Xna.Framework.Rectangle hitbox)
        {
            // Scarlet red and black particles
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(new Microsoft.Xna.Framework.Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.RedTorch, player.velocity.X * 0.2f, player.velocity.Y * 0.2f, 150, default, 1.2f);
                dust.noGravity = true;
                dust.velocity *= 1.3f;
            }

            if (Main.rand.NextBool(4))
            {
                Dust dust2 = Dust.NewDustDirect(new Microsoft.Xna.Framework.Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.Smoke, 0f, 0f, 100, default, 0.9f);
                dust2.noGravity = true;
                dust2.velocity = Main.rand.NextVector2Circular(2f, 2f);
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
