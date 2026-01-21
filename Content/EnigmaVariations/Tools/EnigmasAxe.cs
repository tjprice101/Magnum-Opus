using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.EnigmaVariations.ResonanceEnergies;
using MagnumOpus.Content.EnigmaVariations.HarmonicCores;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common;

namespace MagnumOpus.Content.EnigmaVariations.Tools
{
    /// <summary>
    /// Enigma's Axe - A mysterious axe that reveals hidden truths with each swing.
    /// Features eerie green flame effects and occasional watching eye particles.
    /// Tier between Swan Lake and Fate.
    /// </summary>
    public class EnigmasAxe : ModItem
    {
        // Enigma theme colors
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 200; // Higher than Swan Lake
            Item.DamageType = DamageClass.Melee;
            Item.width = 44;
            Item.height = 44;
            Item.useTime = 3;
            Item.useAnimation = 5;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 9f;
            Item.value = Item.sellPrice(gold: 50);
            Item.rare = ModContent.RarityType<EnigmaVariationsRarity>();
            Item.UseSound = SoundID.Item15 with { Pitch = -0.3f, Volume = 0.7f };
            Item.autoReuse = true;
            Item.useTurn = true;

            // Axe power - high tier
            Item.axe = 50; // Tier 4: 250% axe power - between La Campanella (225%) and Swan Lake (300%)
            
            Item.maxStack = 1;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfEnigma>(), 20)
                .AddIngredient(ModContent.ItemType<EnigmaResonantEnergy>(), 15)
                .AddIngredient(ModContent.ItemType<RemnantOfMysteries>(), 10)
                .AddIngredient(ItemID.SoulofNight, 12)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            // Eerie green flame particles
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.GreenTorch, player.velocity.X * 0.2f, player.velocity.Y * 0.2f, 150, EnigmaGreen, 1.5f);
                dust.noGravity = true;
                dust.velocity *= 1.4f;
            }

            // Purple arcane mist
            if (Main.rand.NextBool(3))
            {
                Dust mist = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.PurpleTorch, 0f, -0.6f, 120, EnigmaPurple, 1.2f);
                mist.noGravity = true;
                mist.velocity = Main.rand.NextVector2Circular(2.5f, 2.5f);
            }

            // Occasional watching eye effect
            if (Main.rand.NextBool(10))
            {
                Vector2 eyePos = hitbox.Center.ToVector2();
                CustomParticles.EnigmaEyeGaze(eyePos, EnigmaGreen * 0.7f, 0.35f);
            }

            // Glyph sparkles
            if (Main.rand.NextBool(7))
            {
                Vector2 glyphPos = hitbox.Center.ToVector2() + Main.rand.NextVector2Circular(12f, 12f);
                CustomParticles.Glyph(glyphPos, EnigmaPurple * 0.5f, 0.25f, -1);
            }

            // Music notes in tool swing
            if (Main.rand.NextBool(5))
            {
                Vector2 notePos = hitbox.Center.ToVector2();
                ThemedParticles.EnigmaMusicNotes(notePos, 2, 18f);
            }
        }
    }
}
