using Microsoft.Xna.Framework;
using System.Collections.Generic;
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
    /// Enigma's Pickaxe - A mysterious pickaxe that questions the very stone it breaks.
    /// Features eerie green flame effects and occasional watching eye particles.
    /// Tier 4 - between La Campanella and Swan Lake (Moonlight ↁEEroica ↁELa Campanella ↁEEnigma ↁESwan Lake ↁEFate)
    /// </summary>
    public class EnigmasPickaxe : ModItem
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
            Item.damage = 150; // Tier 4: Between La Campanella (135) and Swan Lake (185)
            Item.DamageType = DamageClass.Melee;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 2;
            Item.useAnimation = 5;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 7f;
            Item.value = Item.sellPrice(gold: 35);
            Item.rare = ModContent.RarityType<EnigmaVariationsRarity>();
            Item.UseSound = SoundID.Item15 with { Pitch = -0.2f, Volume = 0.7f };
            Item.autoReuse = true;
            Item.useTurn = true;

            // Pickaxe power - can mine Swan Lake ore (450), Tier 4 between La Campanella (400) and Swan Lake (500)
            Item.pick = 450;
            
            Item.maxStack = 1;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Each strike reveals another question'") { OverrideColor = new Color(140, 60, 200) });
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
                    DustID.GreenTorch, player.velocity.X * 0.2f, player.velocity.Y * 0.2f, 150, EnigmaGreen, 1.4f);
                dust.noGravity = true;
                dust.velocity *= 1.3f;
            }

            // Purple arcane mist
            if (Main.rand.NextBool(3))
            {
                Dust mist = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.PurpleTorch, 0f, -0.5f, 120, EnigmaPurple, 1.1f);
                mist.noGravity = true;
                mist.velocity = Main.rand.NextVector2Circular(2f, 2f);
            }

            // Occasional watching eye effect
            if (Main.rand.NextBool(12))
            {
                Vector2 eyePos = hitbox.Center.ToVector2();
                CustomParticles.EnigmaEyeGaze(eyePos, EnigmaGreen * 0.7f, 0.3f);
            }

            // Glyph sparkles
            if (Main.rand.NextBool(8))
            {
                Vector2 glyphPos = hitbox.Center.ToVector2() + Main.rand.NextVector2Circular(10f, 10f);
                CustomParticles.Glyph(glyphPos, EnigmaPurple * 0.5f, 0.2f, -1);
            }

            // Music notes in tool swing
            if (Main.rand.NextBool(5))
            {
                Vector2 notePos = hitbox.Center.ToVector2();
                ThemedParticles.EnigmaMusicNotes(notePos, 2, 15f);
            }
        }
    }
}
