using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;
using MagnumOpus.Content.EnigmaVariations.ResonanceEnergies;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons;
using MagnumOpus.Content.EnigmaVariations.HarmonicCores;

namespace MagnumOpus.Content.EnigmaVariations.Bosses
{
    public class EnigmaTreasureBag : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.EyeOfCthulhuBossBag; // Placeholder

        public override void SetStaticDefaults()
        {
            ItemID.Sets.BossBag[Type] = true;
            Item.ResearchUnlockCount = 3;
        }

        public override void SetDefaults()
        {
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true;
            Item.width = 32;
            Item.height = 32;
            Item.rare = ItemRarityID.Purple;
            Item.expert = true;
        }

        public override bool CanRightClick() => true;

        public override void ModifyItemLoot(ItemLoot itemLoot)
        {
            // Resonance Energy
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<EnigmaResonantEnergy>(), 1, 20, 35));
            
            // Extra Harmonic Core in expert
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<HarmonicCoreOfEnigma>(), 1, 1, 2));
            
            // Weapons
            itemLoot.Add(ItemDropRule.OneFromOptions(1,
                ModContent.ItemType<Enigma1>(),
                ModContent.ItemType<Enigma2>(),
                ModContent.ItemType<Enigma3>(),
                ModContent.ItemType<Enigma4>(),
                ModContent.ItemType<Enigma5>(),
                ModContent.ItemType<Enigma6>()));
            
            itemLoot.Add(ItemDropRule.OneFromOptions(2,
                ModContent.ItemType<Enigma7>(),
                ModContent.ItemType<Enigma8>(),
                ModContent.ItemType<Enigma9>(),
                ModContent.ItemType<Enigma10>(),
                ModContent.ItemType<Enigma11>(),
                ModContent.ItemType<Enigma12>()));
        }
    }
}
