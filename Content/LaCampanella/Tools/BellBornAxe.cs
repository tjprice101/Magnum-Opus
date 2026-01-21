using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.LaCampanella.ResonanceEnergies;
using MagnumOpus.Content.LaCampanella.HarmonicCores;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.LaCampanella.Tools
{
    /// <summary>
    /// Bell-Born Axe - A powerful axe forged in infernal flames.
    /// Tier 3 axe - higher than Eroica.
    /// REQUIRES La Campanella Resonant Energy (boss drop) to craft.
    /// </summary>
    public class BellBornAxe : ModItem
    {
        // La Campanella theme colors
        private static readonly Color CampanellaOrange = new Color(255, 140, 40);
        private static readonly Color CampanellaGold = new Color(255, 200, 80);

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 120; // Higher than Eroica (91)
            Item.DamageType = DamageClass.Melee;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 5;
            Item.useAnimation = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 7f;
            Item.value = Item.sellPrice(gold: 22);
            Item.rare = ModContent.RarityType<LaCampanellaRarity>();
            Item.UseSound = SoundID.Item1 with { Pitch = -0.2f };
            Item.autoReuse = true;
            Item.useTurn = true;

            // Axe power - higher than Eroica
            Item.axe = 45; // 225% axe power (45 * 5)
            
            Item.maxStack = 1;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfLaCampanella>(), 18)
                .AddIngredient(ModContent.ItemType<LaCampanellaResonantEnergy>(), 12) // BOSS DROP REQUIRED
                .AddIngredient(ItemID.SoulofFright, 12)
                .AddIngredient(ItemID.HellstoneBar, 8)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            // Orange flame particles
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.Torch, player.velocity.X * 0.2f, player.velocity.Y * 0.2f, 150, CampanellaOrange, 1.3f);
                dust.noGravity = true;
                dust.velocity *= 1.2f;
            }

            // Black smoke accents
            if (Main.rand.NextBool(3))
            {
                Dust smoke = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.Smoke, 0f, -0.5f, 150, Color.Black, 1.1f);
                smoke.noGravity = true;
                smoke.velocity = Main.rand.NextVector2Circular(2f, 2f);
            }

            // Gold sparkle accents
            if (Main.rand.NextBool(5))
            {
                Dust sparkle = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.GoldFlame, 0f, 0f, 0, CampanellaGold, 0.8f);
                sparkle.noGravity = true;
                sparkle.velocity *= 0.4f;
            }
        }
    }
}
