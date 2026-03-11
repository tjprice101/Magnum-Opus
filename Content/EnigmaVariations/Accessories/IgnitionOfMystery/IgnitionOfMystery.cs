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
    /// Ignition of Mystery - Melee Accessory
    /// 
    /// "Mysteries Unveiled" - Every melee hit builds mystery stacks (up to 10).
    /// At max stacks, your next melee attack unleashes a massive eye burst that 
    /// marks all nearby enemies with "Watched" debuff (15% increased damage taken).
    /// Additionally, melee speed increases by 2% per stack (up to 20%).
    /// 
    /// Theme: The revelation of hidden truths through combat, 
    /// each strike bringing the unknown closer to light.
    /// </summary>
    public class IgnitionOfMystery : ModItem
    {
        public override string Texture => "MagnumOpus/Content/EnigmaVariations/Accessories/IgnitionOfMystery/IgnitionOfMystery";

        // Enigma color palette
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreenFlame = new Color(50, 220, 100);
        
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
            modPlayer.hasIgnitionOfMystery = true;
        }
        
        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "MysteryHeader", "Mysteries Unveiled:")
            {
                OverrideColor = EnigmaGreenFlame
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Melee attacks build mystery stacks (max 10)"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Each stack grants +2% melee speed (up to +20%)"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "At max stacks, unleash a devastating eye burst"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Marked enemies take 15% increased damage for 5 seconds"));
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The unknown fears those who seek it'")
            {
                OverrideColor = EnigmaPurple
            });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonanceEnergies.EnigmaResonantEnergy>(), 15)
                .AddIngredient(ModContent.ItemType<HarmonicCores.HarmonicCoreOfEnigma>(), 1)
                .AddIngredient(ItemID.MagmaStone)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
