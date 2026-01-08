using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using ReLogic.Content;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Content.Eroica.ResonantWeapons;
using MagnumOpus.Content.Eroica.Pets;
using MagnumOpus.Content.Eroica.Enemies;
using MagnumOpus.Common;

namespace MagnumOpus.Content.Eroica.Bosses
{
    /// <summary>
    /// Treasure Bag dropped by Eroica, God of Valor in Expert/Master mode.
    /// Contains: 20-25 Energy, 30-35 Remnant, 3 random weapons/bell (no dupes), 10-20 Shard of Tempo
    /// Uses separate texture for ground/world rendering.
    /// </summary>
    public class EroicaTreasureBag : ModItem
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
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true;
            Item.rare = ItemRarityID.Expert;
            Item.expert = true;
        }

        public override bool CanRightClick() => true;
        
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            // Use the ground texture when dropped in the world
            _groundTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Content/Eroica/Bosses/EroicaTreasureBag_Ground");
            
            if (_groundTexture.State == AssetState.Loaded)
            {
                Texture2D texture = _groundTexture.Value;
                Vector2 drawPos = Item.position - Main.screenPosition + new Vector2(Item.width / 2f, Item.height / 2f);
                Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
                
                // Add a subtle glow effect
                Color glowColor = new Color(255, 180, 100, 0) * 0.3f;
                for (int i = 0; i < 4; i++)
                {
                    Vector2 offset = new Vector2(2f, 0f).RotatedBy(MathHelper.PiOver2 * i);
                    spriteBatch.Draw(texture, drawPos + offset, null, glowColor, rotation, origin, scale, SpriteEffects.None, 0f);
                }
                
                // Draw main texture
                spriteBatch.Draw(texture, drawPos, null, lightColor, rotation, origin, scale, SpriteEffects.None, 0f);
                
                return false; // Don't draw the default texture
            }
            
            return true;
        }

        public override void ModifyItemLoot(ItemLoot itemLoot)
        {
            // 20-25 Eroica's Resonant Energy
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<EroicasResonantEnergy>(), 1, 20, 25));
            
            // 30-35 Remnant of Eroica's Triumph
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<RemnantOfEroicasTriumph>(), 1, 30, 35));
            
            // 10-20 Shard of Triumph's Tempo
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<ShardOfTriumphsTempo>(), 1, 10, 20));
            
            // 3 random weapons or bell (no duplicates)
            itemLoot.Add(new EroicaTreasureBagWeaponRule());
        }

        public override Color? GetAlpha(Color lightColor)
        {
            // Red and gold tint
            return new Color(255, 220, 180, 255);
        }
    }
    
    /// <summary>
    /// Custom drop rule for treasure bag that drops 3 random weapons/bell without duplicates.
    /// </summary>
    public class EroicaTreasureBagWeaponRule : IItemDropRule
    {
        public List<IItemDropRuleChainAttempt> ChainedRules => new List<IItemDropRuleChainAttempt>();

        public bool CanDrop(DropAttemptInfo info) => true;

        public void ReportDroprates(List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo)
        {
            int[] possibleDrops = GetPossibleDrops();
            float individualChance = 3f / possibleDrops.Length; // 3 items from the pool
            
            foreach (int itemType in possibleDrops)
            {
                drops.Add(new DropRateInfo(itemType, 1, 1, individualChance, ratesInfo.conditions));
            }
        }

        public ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info)
        {
            int[] possibleDrops = GetPossibleDrops();
            
            // Shuffle the array
            List<int> shuffled = new List<int>(possibleDrops);
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int j = Main.rand.Next(i + 1);
                int temp = shuffled[i];
                shuffled[i] = shuffled[j];
                shuffled[j] = temp;
            }
            
            // Drop 3 items (no duplicates since we're taking from shuffled list)
            for (int i = 0; i < 3 && i < shuffled.Count; i++)
            {
                CommonCode.DropItem(info, shuffled[i], 1);
            }
            
            return new ItemDropAttemptResult
            {
                State = ItemDropAttemptResultState.Success
            };
        }
        
        private int[] GetPossibleDrops()
        {
            return new int[]
            {
                ModContent.ItemType<BellOfEroica>(),
                ModContent.ItemType<FuneralPrayer>(),
                ModContent.ItemType<TriumphantFractal>(),
                ModContent.ItemType<SakurasBlossom>(),
                ModContent.ItemType<BlossomOfTheSakura>(),
                ModContent.ItemType<FinalityOfTheSakura>(),
                ModContent.ItemType<PiercingLightOfTheSakura>(),
                ModContent.ItemType<CelestialValor>()
            };
        }
    }
}
