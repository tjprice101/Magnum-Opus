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
    /// Enigma's Hammer - A mysterious hammer that shatters the unknown.
    /// Features eerie green flame effects and occasional watching eye particles.
    /// Tier between Swan Lake and Fate.
    /// </summary>
    public class EnigmasHammer : ModItem
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
            Item.damage = 210; // Higher than Swan Lake
            Item.DamageType = DamageClass.Melee;
            Item.width = 46;
            Item.height = 46;
            Item.useTime = 4;
            Item.useAnimation = 6;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 12f;
            Item.value = Item.sellPrice(gold: 50);
            Item.rare = ModContent.RarityType<EnigmaVariationsRarity>();
            Item.UseSound = SoundID.Item1 with { Pitch = -0.4f, Volume = 0.8f };
            Item.autoReuse = true;
            Item.useTurn = true;

            // Hammer power - high tier
            Item.hammer = 105; // Tier 4: Between La Campanella (95) and Swan Lake (165)
            
            Item.maxStack = 1;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Breaks apart certainty with each blow'") { OverrideColor = new Color(140, 60, 200) });
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
            // Eerie green flame particles - more intense for hammer
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.GreenTorch, player.velocity.X * 0.25f, player.velocity.Y * 0.25f, 150, EnigmaGreen, 1.6f);
                dust.noGravity = true;
                dust.velocity *= 1.5f;
            }

            // Purple arcane mist
            if (Main.rand.NextBool(3))
            {
                Dust mist = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.PurpleTorch, 0f, -0.7f, 120, EnigmaPurple, 1.3f);
                mist.noGravity = true;
                mist.velocity = Main.rand.NextVector2Circular(3f, 3f);
            }

            // Occasional watching eye effect - more frequent for hammer impacts
            if (Main.rand.NextBool(8))
            {
                Vector2 eyePos = hitbox.Center.ToVector2();
                CustomParticles.EnigmaEyeGaze(eyePos, EnigmaGreen * 0.8f, 0.4f);
            }

            // Glyph sparkles - more for heavy impacts
            if (Main.rand.NextBool(6))
            {
                Vector2 glyphPos = hitbox.Center.ToVector2() + Main.rand.NextVector2Circular(15f, 15f);
                CustomParticles.Glyph(glyphPos, EnigmaPurple * 0.6f, 0.3f, -1);
            }

            // Music notes in tool swing
            if (Main.rand.NextBool(4))
            {
                Vector2 notePos = hitbox.Center.ToVector2();
                ThemedParticles.EnigmaMusicNotes(notePos, 3, 20f);
            }
        }
    }
}
