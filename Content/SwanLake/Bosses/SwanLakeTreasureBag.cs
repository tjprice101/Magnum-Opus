using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using ReLogic.Content;
using MagnumOpus.Content.SwanLake.ResonanceEnergies;
using MagnumOpus.Content.SwanLake.ResonantWeapons;
using MagnumOpus.Content.SwanLake.Items;
using MagnumOpus.Common;

namespace MagnumOpus.Content.SwanLake.Bosses
{
    /// <summary>
    /// Treasure Bag dropped by Swan Lake, The Monochromatic Fractal in Expert/Master mode.
    /// Contains: 20-25 Energy, 30-35 Remnant, 3 random weapons (no dupes), 10-20 Harmonic items
    /// Uses separate texture for ground/world rendering.
    /// </summary>
    public class SwanLakeTreasureBag : ModItem
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

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Elegance distilled into material form'") { OverrideColor = new Color(240, 240, 255) });
        }

        public override bool CanRightClick() => true;
        
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            // Use the ground texture when dropped in the world
            _groundTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Content/SwanLake/Bosses/SwanLakeTreasureBag_Ground");
            
            if (_groundTexture.State == AssetState.Loaded)
            {
                Texture2D texture = _groundTexture.Value;
                Vector2 drawPos = Item.position - Main.screenPosition + new Vector2(Item.width / 2f, Item.height / 2f);
                Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
                
                // Add a subtle rainbow glow effect
                Color glowColor = Main.hslToRgb((Main.GameUpdateCount * 0.01f) % 1f, 0.6f, 0.7f) * 0.3f;
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
            // 20-25 Swan's Resonance Energy
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<SwansResonanceEnergy>(), 1, 20, 25));
            
            // 30-35 Remnant of Swan's Harmony
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<RemnantOfSwansHarmony>(), 1, 30, 35));
            
            // 10-20 Shard of the Feathered Tempo
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<ShardOfTheFeatheredTempo>(), 1, 10, 20));
            
            // 3 random weapons (no duplicates)
            itemLoot.Add(new SwanLakeTreasureBagWeaponRule());
            
            // Feather's Call - Expert/Master exclusive drop (5% chance from treasure bag)
            // This rare transformation item is only available in Expert/Master mode
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<FeathersCall>(), 20)); // 1/20 = 5%
        }

        public override Color? GetAlpha(Color lightColor)
        {
            // Monochromatic with pearlescent shimmer
            return new Color(245, 245, 250, 255);
        }
    }
    
    /// <summary>
    /// Custom drop rule for treasure bag that drops 3 random weapons without duplicates.
    /// </summary>
    public class SwanLakeTreasureBagWeaponRule : IItemDropRule
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
                ModContent.ItemType<CalloftheBlackSwan>(),
                ModContent.ItemType<CallofthePearlescentLake>(),
                ModContent.ItemType<ChromaticSwanSong>(),
                ModContent.ItemType<FeatheroftheIridescentFlock>(),
                ModContent.ItemType<IridescentWingspan>(),
                ModContent.ItemType<TheSwansLament>()
            };
        }
    }
}
