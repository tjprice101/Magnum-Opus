using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.EnigmaVariations.Accessories
{
    /// <summary>
    /// Pendant of a Thousand Puzzles - Mage Accessory
    /// 
    /// "Arcane Enigma" - Magic attacks have a 15% chance to spawn a "Puzzle Fragment".
    /// Collecting 5 fragments grants "Puzzle Mastery" buff for 8 seconds:
    /// +30% magic damage, -20% mana cost, and magic projectiles leave glowing glyph trails.
    /// Fragments orbit the player and can be collected by touching them.
    /// 
    /// Theme: The accumulation of arcane knowledge, 
    /// each fragment a piece of an unknowable puzzle.
    /// </summary>
    public class PendantOfAThousandPuzzles : ModItem
    {
        public override string Texture => "MagnumOpus/Content/EnigmaVariations/Accessories/PendantOfAThousandPuzzles/PendantOfAThousandPuzzles";

        // Enigma color palette
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreenFlame = new Color(50, 220, 100);
        private static readonly Color EnigmaDeepPurple = new Color(80, 20, 120);
        
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 3);
            Item.rare = ModContent.RarityType<EnigmaVariationsRarity>();
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<EnigmaAccessoryPlayer>();
            modPlayer.hasPendantOfAThousandPuzzles = true;
        }
        
        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "ArcaneHeader", "Arcane Enigma:")
            {
                OverrideColor = EnigmaPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Magic attacks have 15% chance to spawn puzzle fragments"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Collect 5 fragments to activate Puzzle Mastery (8 seconds):"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "  +30% magic damage, -20% mana cost"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "  Magic projectiles leave glowing glyph trails"));
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A thousand answers to questions never asked'")
            {
                OverrideColor = EnigmaGreenFlame
            });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonanceEnergies.EnigmaResonantEnergy>(), 15)
                .AddIngredient(ModContent.ItemType<HarmonicCores.HarmonicCoreOfEnigma>(), 1)
                .AddIngredient(ItemID.CelestialEmblem)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
