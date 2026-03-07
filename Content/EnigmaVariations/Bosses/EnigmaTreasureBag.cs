using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;
using ReLogic.Content;
using MagnumOpus.Content.EnigmaVariations.ResonanceEnergies;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.CipherNocturne;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.DissonanceOfSecrets;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.FugueOfTheUnknown;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TacetsEnigma;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheSilentMeasure;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheUnresolvedCadence;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheWatchingRefrain;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.VariationsOfTheVoid;
using MagnumOpus.Content.EnigmaVariations.HarmonicCores;
using MagnumOpus.Content.EnigmaVariations.Accessories;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.EnigmaVariations.Bosses
{
    /// <summary>
    /// Treasure Bag dropped by Enigma, The Hollow Mystery in Expert/Master mode.
    /// Contains: Resonance Energy, Harmonic Core, weapons, and class-specific accessories.
    /// Uses separate texture for ground/world rendering.
    /// </summary>
    public class EnigmaTreasureBag : ModItem
    {
        private Asset<Texture2D> _groundTexture;
        
        public override void SetStaticDefaults()
        {
            ItemID.Sets.BossBag[Type] = true;
            ItemID.Sets.PreHardmodeLikeBossBag[Type] = false;
            Item.ResearchUnlockCount = 3;
        }

        public override void SetDefaults()
        {
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true;
            Item.width = 24;  // Reduced from 32 for smaller minimap icon
            Item.height = 24; // Reduced from 32 for smaller minimap icon
            Item.rare = ItemRarityID.Expert;
            Item.expert = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Answers to questions never asked'") { OverrideColor = new Color(140, 60, 200) });
        }

        public override bool CanRightClick() => true;
        
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            // Use the ground texture when dropped in the world
            _groundTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Content/EnigmaVariations/Bosses/EnigmaTreasureBag_Ground");
            
            if (_groundTexture.State == AssetState.Loaded)
            {
                Texture2D texture = _groundTexture.Value;
                Vector2 drawPos = Item.position - Main.screenPosition + new Vector2(Item.width / 2f, Item.height / 2f);
                Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
                
                // Enigma-themed glow effect - cycling purple to green
                float cycle = (Main.GameUpdateCount * 0.02f) % 1f;
                Color glowColor = ThemedParticles.GetEnigmaGradient(cycle) * 0.35f;
                for (int i = 0; i < 4; i++)
                {
                    Vector2 offset = new Vector2(2f, 0f).RotatedBy(MathHelper.PiOver2 * i);
                    spriteBatch.Draw(texture, drawPos + offset, null, glowColor, rotation, origin, scale, SpriteEffects.None, 0f);
                }
                
                // Draw main texture
                spriteBatch.Draw(texture, drawPos, null, lightColor, rotation, origin, scale, SpriteEffects.None, 0f);
                
                return false;
            }
            
            return true;
        }

        public override void ModifyItemLoot(ItemLoot itemLoot)
        {
            // Resonance Energy (20-35)
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<EnigmaResonantEnergy>(), 1, 20, 35));
            
            // Extra Harmonic Core in expert (1-2)
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<HarmonicCoreOfEnigma>(), 1, 1, 2));
            
            // 3 random weapons (no duplicates) — custom drop rule
            itemLoot.Add(new EnigmaTreasureBagWeaponRule());
            
            // Class-specific accessory (one random drop)
            itemLoot.Add(ItemDropRule.OneFromOptions(1,
                ModContent.ItemType<IgnitionOfMystery>(),       // Melee
                ModContent.ItemType<PendantOfAThousandPuzzles>(), // Mage
                ModContent.ItemType<AlchemicalParadox>(),        // Ranger
                ModContent.ItemType<RiddlemastersCauldron>()));   // Summoner
        }
        
        public override Color? GetAlpha(Color lightColor)
        {
            // Enigma mystical shimmer
            float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.05f) * 0.15f + 0.85f;
            return new Color((int)(220 * pulse), (int)(180 * pulse), (int)(255 * pulse), 255);
        }
    }

    /// <summary>
    /// Custom drop rule: drops 3 random weapons without duplicates from the full Enigma weapon pool.
    /// </summary>
    public class EnigmaTreasureBagWeaponRule : IItemDropRule
    {
        public List<IItemDropRuleChainAttempt> ChainedRules => new List<IItemDropRuleChainAttempt>();
        public bool CanDrop(DropAttemptInfo info) => true;

        public void ReportDroprates(List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo)
        {
            int[] possibleDrops = GetPossibleDrops();
            float individualChance = 3f / possibleDrops.Length;
            foreach (int itemType in possibleDrops)
                drops.Add(new DropRateInfo(itemType, 1, 1, individualChance, ratesInfo.conditions));
        }

        public ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info)
        {
            int[] possibleDrops = GetPossibleDrops();
            List<int> shuffled = new List<int>(possibleDrops);
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int j = Main.rand.Next(i + 1);
                (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
            }
            for (int i = 0; i < 3 && i < shuffled.Count; i++)
                CommonCode.DropItem(info, shuffled[i], 1);
            
            return new ItemDropAttemptResult { State = ItemDropAttemptResultState.Success };
        }

        private int[] GetPossibleDrops()
        {
            return new int[]
            {
                ModContent.ItemType<VariationsOfTheVoidItem>(),      // Melee Sword
                ModContent.ItemType<TheUnresolvedCadenceItem>(),     // Melee Broadsword
                ModContent.ItemType<DissonanceOfSecrets>(),          // Magic Staff
                ModContent.ItemType<CipherNocturne>(),               // Magic Beam
                ModContent.ItemType<FugueOfTheUnknown>(),            // Magic Tome
                ModContent.ItemType<TheWatchingRefrain>(),           // Summon
                ModContent.ItemType<TheSilentMeasure>(),             // Ranged Gun
                ModContent.ItemType<TacetsEnigma>()                  // Ranged
            };
        }
    }
}
