using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using ReLogic.Content;
using MagnumOpus.Content.LaCampanella.ResonanceEnergies;
using MagnumOpus.Content.LaCampanella.ResonantWeapons;
using MagnumOpus.Content.LaCampanella.Accessories;
using MagnumOpus.Common;

namespace MagnumOpus.Content.LaCampanella.Bosses
{
    /// <summary>
    /// Treasure Bag dropped by La Campanella boss in Expert/Master mode.
    /// Contains: 20-25 Energy, 30-35 Remnant items, 3 random weapons (no dupes), 10-20 Bell shards
    /// Uses separate texture for ground/world rendering.
    /// Color scheme: Black, dark gray, white with red/orange/yellow flames.
    /// </summary>
    public class LaCampanellaTreasureBag : ModItem
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
        
        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            // Draw with enhanced color to counteract any desaturation
            Texture2D texture = Terraria.GameContent.TextureAssets.Item[Item.type].Value;
            
            // Draw a subtle orange glow behind the bag
            Color glowColor = new Color(255, 120, 40, 0) * 0.4f;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(2f, 0f).RotatedBy(MathHelper.PiOver2 * i);
                spriteBatch.Draw(texture, position + offset, frame, glowColor, 0f, origin, scale, SpriteEffects.None, 0f);
            }
            
            // Draw with full brightness white tint to make colors pop
            spriteBatch.Draw(texture, position, frame, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
            
            return false; // Don't draw default
        }
        
        public override void RightClick(Player player)
        {
            // Play bell opening sound
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.2f, Volume = 0.6f }, player.Center);
            
            // Fire explosion particles on opening
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                
                Color color = i % 3 == 0 ? new Color(255, 100, 0) :
                              i % 3 == 1 ? new Color(255, 180, 50) : new Color(30, 25, 20);
                
                Dust flame = Dust.NewDustPerfect(player.Center, DustID.Torch, velocity, 100, color, 2f);
                flame.noGravity = true;
            }
        }
        
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            // Use the ground texture when dropped in the world
            _groundTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Content/LaCampanella/Bosses/LaCampanellaTreasureBag_Ground");
            
            if (_groundTexture.State == AssetState.Loaded)
            {
                Texture2D texture = _groundTexture.Value;
                Vector2 drawPos = Item.position - Main.screenPosition + new Vector2(Item.width / 2f, Item.height / 2f);
                Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
                
                // Fire glow effect - pulsing orange/red
                float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.1f) * 0.15f + 0.35f;
                Color glowColor = new Color(255, 120, 30, 0) * pulse;
                for (int i = 0; i < 4; i++)
                {
                    Vector2 offset = new Vector2(3f, 0f).RotatedBy(MathHelper.PiOver2 * i + Main.GameUpdateCount * 0.02f);
                    spriteBatch.Draw(texture, drawPos + offset, null, glowColor, rotation, origin, scale, SpriteEffects.None, 0f);
                }
                
                // Draw main texture
                spriteBatch.Draw(texture, drawPos, null, lightColor, rotation, origin, scale, SpriteEffects.None, 0f);
                
                // Spawn occasional fire dust
                if (Main.rand.NextBool(8))
                {
                    Dust flame = Dust.NewDustPerfect(
                        Item.position + new Vector2(Main.rand.Next(Item.width), Main.rand.Next(Item.height)),
                        DustID.Torch, new Vector2(0f, -1f), 100, new Color(255, 100, 0), 1.2f);
                    flame.noGravity = true;
                }
                
                return false; // Don't draw the default texture
            }
            
            return true;
        }

        public override void ModifyItemLoot(ItemLoot itemLoot)
        {
            // 20-25 La Campanella's Resonant Energy
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<LaCampanellaResonantEnergy>(), 1, 20, 25));
            
            // 30-35 Remnant of the Infernal Bell (create this if needed)
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<RemnantOfTheInfernalBell>(), 1, 30, 35));
            
            // 10-20 Shard of the Burning Tempo
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<ShardOfTheBurningTempo>(), 1, 10, 20));
            
            // 3 random weapons (no duplicates)
            itemLoot.Add(new LaCampanellaTreasureBagWeaponRule());
            
            // Expert-only accessory chance (one of the basic ones)
            // LeadingConditionRule expertRule = new LeadingConditionRule(new Conditions.IsExpert());
            // expertRule.OnSuccess(ItemDropRule.OneFromOptionsNotScalingWithLuck(4, 
            //     ModContent.ItemType<ChamberOfBellfire>()));
            // itemLoot.Add(expertRule);
        }

        public override Color? GetAlpha(Color lightColor)
        {
            // Black with orange/red tint from fire
            return new Color(60, 40, 30, 255);
        }
        
        public override void PostUpdate()
        {
            // Fire lighting
            Lighting.AddLight(Item.Center, 0.6f, 0.3f, 0.1f);
        }
    }
    
    /// <summary>
    /// Custom drop rule for treasure bag that drops 3 random weapons without duplicates.
    /// </summary>
    public class LaCampanellaTreasureBagWeaponRule : IItemDropRule
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
                // Melee weapons
                ModContent.ItemType<DualFatedChime>(),
                ModContent.ItemType<IgnitionOfTheBell>(),
                
                // Magic weapon
                ModContent.ItemType<FangOfTheInfiniteBell>(),
                
                // Summon weapon
                ModContent.ItemType<InfernalChimesCalling>(),
                
                // Ranger weapons
                ModContent.ItemType<PiercingBellsResonance>(),
                ModContent.ItemType<GrandioseChime>(),
                ModContent.ItemType<SymphonicBellfireAnnihilator>()
            };
        }
    }
    
    /// <summary>
    /// Remnant material dropped by La Campanella boss.
    /// </summary>
    public class RemnantOfTheInfernalBell : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
            ItemID.Sets.ItemNoGravity[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ModContent.RarityType<LaCampanellaRarity>();
        }

        public override void PostUpdate()
        {
            // Fire glow
            Lighting.AddLight(Item.Center, 0.5f, 0.3f, 0.1f);
            
            if (Main.rand.NextBool(12))
            {
                Color color = Main.rand.NextBool() ? new Color(255, 100, 0) : new Color(255, 180, 50);
                Dust flame = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Torch, 
                    0f, -0.5f, 100, color, 1.0f);
                flame.noGravity = true;
                flame.velocity *= 0.3f;
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 150, 80, 255);
        }
    }
    
    /// <summary>
    /// Shard material dropped by La Campanella boss.
    /// </summary>
    public class ShardOfTheBurningTempo : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
            ItemID.Sets.ItemNoGravity[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(silver: 50);
            Item.rare = ModContent.RarityType<LaCampanellaRarity>();
        }

        public override void PostUpdate()
        {
            Lighting.AddLight(Item.Center, 0.4f, 0.2f, 0.05f);
            
            if (Main.rand.NextBool(15))
            {
                Dust spark = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.GoldFlame, 
                    0f, -0.3f, 100, new Color(255, 150, 50), 0.8f);
                spark.noGravity = true;
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 180, 100, 255);
        }
    }
}
