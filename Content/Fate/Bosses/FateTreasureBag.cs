using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using ReLogic.Content;
using MagnumOpus.Content.Fate.ResonanceEnergies;
using MagnumOpus.Content.Fate.ResonantWeapons;
using MagnumOpus.Common;

namespace MagnumOpus.Content.Fate.Bosses
{
    /// <summary>
    /// Treasure Bag dropped by Fate, The Warden of Universal Melodies in Expert/Master mode.
    /// Contains: 25-35 Energy, 35-45 Remnant, 3 random weapons (no dupes), 15-25 Shard items
    /// Uses separate texture for ground/world rendering.
    /// </summary>
    public class FateTreasureBag : ModItem
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
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Destiny's rewards for those who dare'") { OverrideColor = new Color(180, 40, 80) });
        }

        public override bool CanRightClick() => true;
        
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            // Use the ground texture when dropped in the world
            _groundTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Content/Fate/Bosses/FateTreasureBag_Ground");
            
            if (_groundTexture.State == AssetState.Loaded)
            {
                Texture2D texture = _groundTexture.Value;
                Vector2 drawPos = Item.position - Main.screenPosition + new Vector2(Item.width / 2f, Item.height / 2f);
                Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
                
                // Cosmic glow effect - dark pink to crimson cycling
                float hueBase = (Main.GameUpdateCount * 0.008f) % 1f;
                Color glowColor1 = new Color(180, 50, 100) * 0.4f; // FateDarkPink
                Color glowColor2 = new Color(255, 60, 80) * 0.3f;  // FateBrightRed
                Color glowColor = Color.Lerp(glowColor1, glowColor2, (float)System.Math.Sin(hueBase * MathHelper.TwoPi) * 0.5f + 0.5f);
                
                for (int i = 0; i < 4; i++)
                {
                    Vector2 offset = new Vector2(2f, 0f).RotatedBy(MathHelper.PiOver2 * i);
                    spriteBatch.Draw(texture, drawPos + offset, null, glowColor, rotation, origin, scale, SpriteEffects.None, 0f);
                }
                
                // Star sparkle accents
                if (Main.rand.NextBool(30))
                {
                    Vector2 sparkleOffset = Main.rand.NextVector2Circular(12f, 12f);
                    Dust star = Dust.NewDustPerfect(Item.Center + sparkleOffset, DustID.WhiteTorch, Vector2.Zero, 0, Color.White, 0.6f);
                    star.noGravity = true;
                    star.fadeIn = 0.5f;
                }
                
                // Draw main texture
                spriteBatch.Draw(texture, drawPos, null, lightColor, rotation, origin, scale, SpriteEffects.None, 0f);
                
                return false;
            }
            
            return true;
        }

        public override void ModifyItemLoot(ItemLoot itemLoot)
        {
            // 25-35 Fate's Resonance Energy
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<FateResonantEnergy>(), 1, 25, 35));
            
            // 35-45 Remnant of the Galaxy's Harmony
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<RemnantOfTheGalaxysHarmony>(), 1, 35, 45));
            
            // 15-25 Shard of Fate's Tempo
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<ShardOfFatesTempo>(), 1, 15, 25));
            
            // 3 random weapons (no duplicates)
            itemLoot.Add(new FateTreasureBagWeaponRule());
            
            // Resonant Core of Fate - Expert/Master exclusive (10% chance)
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<ResonantCoreOfFate>(), 10));
            
            // Seed of Universal Melodies - legendary crafting material (always 2-3 in Expert/Master)
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<Content.Items.SeedOfUniversalMelodies>(), 1, 2, 3));
        }

        public override Color? GetAlpha(Color lightColor)
        {
            // Cosmic dark with pink/red tint
            return new Color(200, 180, 200, 255);
        }
    }
    
    /// <summary>
    /// Custom drop rule for treasure bag that drops 3 random weapons without duplicates.
    /// </summary>
    public class FateTreasureBagWeaponRule : IItemDropRule
    {
        public List<IItemDropRuleChainAttempt> ChainedRules => new List<IItemDropRuleChainAttempt>();

        public bool CanDrop(DropAttemptInfo info) => true;

        public void ReportDroprates(List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo)
        {
            int[] possibleDrops = GetPossibleDrops();
            float individualChance = 3f / possibleDrops.Length;
            
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
            
            // Drop first 3 unique weapons
            int dropCount = System.Math.Min(3, shuffled.Count);
            for (int i = 0; i < dropCount; i++)
            {
                CommonCode.DropItem(info, shuffled[i], 1);
            }
            
            return new ItemDropAttemptResult { State = ItemDropAttemptResultState.Success };
        }
        
        private static int[] GetPossibleDrops()
        {
            return new int[]
            {
                ModContent.ItemType<CodaOfAnnihilation>(),
                ModContent.ItemType<DestinysCrescendo>(),
                ModContent.ItemType<FractalOfTheStars>(),
                ModContent.ItemType<LightOfTheFuture>(),
                ModContent.ItemType<OpusUltima>(),
                ModContent.ItemType<RequiemOfReality>(),
                ModContent.ItemType<ResonanceOfABygoneReality>(),
                ModContent.ItemType<SymphonysEnd>(),
                ModContent.ItemType<TheConductorsLastConstellation>(),
                ModContent.ItemType<TheFinalFermata>()
            };
        }
    }
}
