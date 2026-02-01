using Microsoft.Xna.Framework;
using System.Collections.Generic;
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
    /// Bell-Born Pickaxe - A powerful pickaxe forged in infernal flames.
    /// Tier 3 pickaxe - higher than Eroica, can mine Enigma ore.
    /// REQUIRES La Campanella Resonant Energy (boss drop) to craft.
    /// </summary>
    public class BellBornPickaxe : ModItem
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
            Item.damage = 135; // Higher than Eroica (101)
            Item.DamageType = DamageClass.Melee;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 3;
            Item.useAnimation = 6;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 7f;
            Item.value = Item.sellPrice(gold: 25);
            Item.rare = ModContent.RarityType<LaCampanellaRarity>();
            Item.UseSound = SoundID.Item1 with { Pitch = -0.2f };
            Item.autoReuse = true;
            Item.useTurn = true;

            // Pickaxe power - can mine Enigma ore
            Item.pick = 400;
            
            Item.maxStack = 1;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Chimes through ore with infernal resonance'") { OverrideColor = new Color(255, 140, 40) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfLaCampanella>(), 20)
                .AddIngredient(ModContent.ItemType<LaCampanellaResonantEnergy>(), 15) // BOSS DROP REQUIRED
                .AddIngredient(ItemID.SoulofFright, 15)
                .AddIngredient(ItemID.HellstoneBar, 10)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            // Orange flame particles
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.Torch, player.velocity.X * 0.2f, player.velocity.Y * 0.2f, 150, CampanellaOrange, 1.4f);
                dust.noGravity = true;
                dust.velocity *= 1.3f;
            }

            // Black smoke accents
            if (Main.rand.NextBool(3))
            {
                Dust smoke = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.Smoke, 0f, -0.5f, 150, Color.Black, 1.2f);
                smoke.noGravity = true;
                smoke.velocity = Main.rand.NextVector2Circular(2f, 2f);
            }

            // Gold sparkle accents
            if (Main.rand.NextBool(4))
            {
                Dust sparkle = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.GoldFlame, 0f, 0f, 0, CampanellaGold, 0.9f);
                sparkle.noGravity = true;
                sparkle.velocity *= 0.5f;
            }

            // Music notes
            if (Main.rand.NextBool(5))
            {
                Vector2 notePos = hitbox.Center.ToVector2();
                ThemedParticles.LaCampanellaMusicNotes(notePos, 1, 12f);
            }
        }
    }
}
