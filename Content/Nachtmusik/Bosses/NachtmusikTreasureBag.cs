using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.Creative;
using MagnumOpus.Common;
using MagnumOpus.Content.Nachtmusik.ResonanceEnergies;
using MagnumOpus.Content.Nachtmusik.HarmonicCores;
using MagnumOpus.Content.Nachtmusik.ResonantWeapons;

namespace MagnumOpus.Content.Nachtmusik.Bosses
{
    /// <summary>
    /// Nachtmusik Treasure Bag - Expert/Master mode boss bag for Nachtmusik, Queen of Radiance.
    /// Contains bonus loot and the exclusive Expert accessory.
    /// </summary>
    public class NachtmusikTreasureBag : ModItem
    {
        // Theme colors
        private static readonly Color DeepPurple = new Color(45, 27, 78);
        private static readonly Color Gold = new Color(255, 215, 0);
        
        public override void SetStaticDefaults()
        {
            ItemID.Sets.BossBag[Type] = true;
            ItemID.Sets.PreHardmodeLikeBossBag[Type] = false;
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 3;
        }

        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 36;
            Item.maxStack = 9999;
            Item.consumable = true;
            Item.rare = ItemRarityID.Expert;
            Item.expert = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Treasures revealed by moonlit serenade'") { OverrideColor = new Color(100, 80, 160) });
        }

        public override bool CanRightClick()
        {
            return true;
        }

        public override void ModifyItemLoot(ItemLoot itemLoot)
        {
            // Resonant Energy (25-35) - guaranteed (higher than Fate)
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<NachtmusikResonantEnergy>(), 1, 25, 35));
            
            // Resonant Core (12-18) - guaranteed (higher than Fate)
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<NachtmusikResonantCore>(), 1, 12, 18));
            
            // Harmonic Core (2-3) - guaranteed (higher than Fate)
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<HarmonicCoreOfNachtmusik>(), 1, 2, 3));
            
            // Remnants (50-70) - guaranteed (higher than Fate)
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<RemnantOfNachtmusiksHarmony>(), 1, 50, 70));
            
            // Shard of Nachtmusik's Tempo (18-28) - guaranteed (equivalent to Fate's Shard)
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<ShardOfNachtmusiksTempo>(), 1, 18, 28));
            
            // ==================== RESONANT WEAPONS ====================
            // Drop 3 random weapons from the weapon pool (no duplicates)
            itemLoot.Add(new NachtmusikTreasureBagWeaponRule());
            
            // Expert-exclusive accessory - Radiance of the Night Queen
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<Accessories.RadianceOfTheNightQueen>(), 1));
            
            // Money
            itemLoot.Add(ItemDropRule.CoinsBasedOnNPCValue(ModContent.NPCType<NachtmusikQueenOfRadiance>()));
        }

        public override Color? GetAlpha(Color lightColor)
        {
            // Subtle glow effect
            return Color.Lerp(lightColor, Color.White, 0.4f);
        }

        public override void PostUpdate()
        {
            // Floating ambient particles
            if (Main.rand.NextBool(15))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, 
                    DustID.PurpleTorch, 0f, -0.5f, 150, default, 0.8f);
                dust.noGravity = true;
            }
            
            if (Main.rand.NextBool(25))
            {
                Dust gold = Dust.NewDustDirect(Item.position, Item.width, Item.height, 
                    DustID.GoldFlame, 0f, -0.3f, 0, default, 0.6f);
                gold.noGravity = true;
            }
            
            // Lighting
            Lighting.AddLight(Item.Center, DeepPurple.ToVector3() * 0.4f);
        }
    }
    
    /// <summary>
    /// Custom drop rule that drops 3 random Nachtmusik weapons without duplicates.
    /// Similar to Fate's treasure bag weapon drop system.
    /// </summary>
    public class NachtmusikTreasureBagWeaponRule : IItemDropRule
    {
        public List<IItemDropRuleChainAttempt> ChainedRules { get; private set; } = new List<IItemDropRuleChainAttempt>();
        
        private static readonly int[] WeaponPool = new int[]
        {
            // Melee (3)
            ModContent.ItemType<NocturnalExecutioner>(),
            ModContent.ItemType<MidnightsCrescendo>(),
            ModContent.ItemType<TwilightSeverance>(),
            // Ranged (3)
            ModContent.ItemType<ConstellationPiercer>(),
            ModContent.ItemType<NebulasWhisper>(),
            ModContent.ItemType<SerenadeOfDistantStars>(),
            // Magic (2)
            ModContent.ItemType<StarweaversGrimoire>(),
            ModContent.ItemType<RequiemOfTheCosmos>(),
            // Summon (3)
            ModContent.ItemType<CelestialChorusBaton>(),
            ModContent.ItemType<GalacticOverture>(),
            ModContent.ItemType<ConductorOfConstellations>(),
        };
        
        public bool CanDrop(DropAttemptInfo info) => true;
        
        public ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info)
        {
            // Shuffle and pick 3 unique weapons
            List<int> available = new List<int>(WeaponPool);
            int dropCount = 3;
            
            for (int i = 0; i < dropCount && available.Count > 0; i++)
            {
                int index = Main.rand.Next(available.Count);
                int weaponType = available[index];
                available.RemoveAt(index);
                
                CommonCode.DropItem(info, weaponType, 1);
            }
            
            return new ItemDropAttemptResult { State = ItemDropAttemptResultState.Success };
        }
        
        public void ReportDroprates(List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo)
        {
            // Report each weapon as having roughly 30% chance (3 out of 10)
            float baseChance = 3f / WeaponPool.Length;
            
            foreach (int weaponType in WeaponPool)
            {
                drops.Add(new DropRateInfo(weaponType, 1, 1, baseChance, ratesInfo.conditions));
            }
        }
    }
}
